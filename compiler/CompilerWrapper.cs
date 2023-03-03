using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Sandbox;
using Sandbox.Internal;

namespace BoxNET.Compiler;

public sealed partial class CompilerWrapper : IDisposable
{
	private readonly Dictionary<string, string> _sourceFileMap = new();
	private List<string>? _variantFileExtensions = new();
	private static string? _baseLocation;

	private CompilerVariant? Variant { get; set; }

	internal async Task BuildInternal()
	{
		BuildSuccess = false;

		var constants = Settings.DefineConstants.Split( ";",
			StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries );

		if ( constants.Contains( Constants.ForceReferenceGame ) )
			References.Add( "Sandbox.Game" );

		var refs = BuildReferences();
		var trees = new List<SyntaxTree>();

		foreach ( var constant in constants )
		{
			if ( constant == Constants.ForceReferenceBase )
			{
				// Reference base
				var md = Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(
					_baseLocation ??
					"assemblies\\package.base.dll" );
				((List<PortableExecutableReference>)refs).Add( md );
			}

			if ( !constant.StartsWith( Constants.PostReference ) )
				continue;

			{
				var name = $"assemblies\\package.{constant[Constants.PostReference.Length..].Replace( '_', '.' )}.dll";
				var md = Microsoft.CodeAnalysis.MetadataReference.CreateFromFile( name );
				if ( md == null )
				{
					Log.Warning( $"Couldn't find MetadataReference {name} while compiling {Name}" );
					continue;
				}

				((List<PortableExecutableReference>)refs).Add( md );
			}
		}

		GetSyntaxTree( trees, CompilerCounter++ );

		if ( Variant == null )
		{
			Log.Warning( $"No compiler variant found for {Name} - does it have any code?" );
			return;
		}

		var diagnosticOptions = Settings.NoWarn.Split( ";",
				StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries )
			.ToDictionary( v => $"{Variant.ShortName}{v}", _ => ReportDiagnostic.Suppress );

		var options = Variant.CreateCompilationOptions().WithSpecificDiagnosticOptions( diagnosticOptions );

		var compiler = Variant.CreateCompiler( trees.OrderByDescending( v => v.FilePath ), refs, options );

		trees.Clear();

		compiler = RunGenerators( compiler );

		if ( Diagnostics.Any( x => x.Severity == ICSharpCompiler.DiagnosticSeverity.Error ) )
		{
			BuildSuccess = false;
			return;
		}

		using ( var stream = new MemoryStream() )
		{
			BuildResult = compiler.Emit( stream,
				options: new EmitOptions().WithDebugInformationFormat( DebugInformationFormat.Embedded ) );
			AsmBinary = stream.ToArray();
		}

		Diagnostics =
			Diagnostics.Concat(
					BuildResult.Diagnostics.Select( x => Util.CreateInternalDiagnostic( x, Name ) ) )
				.ToArray();

		BuildSuccess = BuildResult.Success;

		if ( BuildSuccess )
		{
			using var stream = new MemoryStream( AsmBinary );
			MetadataReference = Microsoft.CodeAnalysis.MetadataReference.CreateFromStream( stream );
			if ( MetadataReference == null ) throw new Exception( "MetadataReference == null" );

			// Save assembly to the assemblies folder (this is a hacky way to do this!)
			Directory.CreateDirectory( "assemblies" );
			await File.WriteAllBytesAsync( $"assemblies\\{AssemblyName}.dll", AsmBinary );

			// Hacky way to find where we saved base assembly
			if ( Name == "base" )
			{
				_baseLocation = Path.GetFullPath( $"assemblies\\{AssemblyName}.dll" );
				Log.Info( $"base assembly location @ {_baseLocation}" );
			}
		}

		Log.Info( $"compile status for {Name} == {BuildResult.Success} ({this})" );
	}

	private Compilation RunGenerators( Compilation compiler )
	{
		if ( Variant is CSharpVariant )
		{
			var runGeneratorsMethod = _internalCompilerType
				.GetMethod( "RunGenerators", BindingFlags.NonPublic | BindingFlags.Instance );
			return (CSharpCompilation)runGeneratorsMethod.Invoke( InternalCompiler, new object?[] { compiler } );
		}

		Diagnostics ??= Array.Empty<ICSharpCompiler.Diagnostic>();

		return compiler;
	}

	private void GetSyntaxTree( List<SyntaxTree> codeFiles, int? buildNumber = null )
	{
		try
		{
			foreach ( var sourceLocation in SourceLocations ) AddSourceFiles( sourceLocation, codeFiles );
		}
		catch ( Exception e )
		{
			Log.Warning( $"boxNET GetSyntaxTree - AddSourceFiles fail! {e}" );
		}

		var generatedCode = GeneratedCode.ToString();
		if ( buildNumber != null )
		{
			generatedCode +=
				$"{Environment.NewLine}[assembly: global::System.Reflection.AssemblyVersion(\"0.0.{buildNumber}.0\")]";
			generatedCode +=
				$"{Environment.NewLine}[assembly: global::System.Reflection.AssemblyFileVersion(\"0.0.{buildNumber}.0\")]";
		}

		if ( string.IsNullOrEmpty( generatedCode ) )
			return;

		if ( Variant is VisualBasicVariant or null )
			return;

		var tree = Variant.ParseText( generatedCode, Variant.ParseOptions, Variant.CompilerExtraFileName,
			Encoding.UTF8 );

		codeFiles.Add( tree );
	}

	/// <summary>
	/// Adds source files to syntax tree list - also finds the CompilerVariant to use
	/// </summary>
	private void AddSourceFiles( BaseFileSystem fileSystem, List<SyntaxTree> codeFiles )
	{
		Parallel.ForEach<string>( fileSystem.FindFile( "/", "*", true ), file =>
		{
			if ( file.StartsWith( "obj/" ) )
				return; // Skip files in obj folder

			var path = fileSystem.GetFullPath( file );

			var extension = Path.GetExtension( path );
			if ( Variant == null )
				// Try to find a compiler variant
			{
				Variant = CompilerVariant.CreateByFileExtension( extension, this );
				_variantFileExtensions = Variant?.GetFileExtensions();
			}
			else
			{
				var alternateVariantType = CompilerVariant.FindTypeByFileExtension( extension );
				if ( alternateVariantType != null && alternateVariantType != Variant.GetType() )
					// Make sure this variant handles this file extension
					throw new Exception( $"CompilerVariant {Variant.GetType().Name} can't handle {extension} files!" );
			}

			var text = Util.ReadTextForgiving( path );

			lock ( _sourceFileMap ) _sourceFileMap[path] = text;

			if ( _variantFileExtensions == null || !_variantFileExtensions.Contains( extension ) )
				return;

			var tree = Variant?.ParseText( text, Variant.ParseOptions, path, Encoding.UTF8 );

			lock ( codeFiles ) codeFiles.Add( tree );
		} );
	}

	internal void OnFileChanged( string file )
	{
		var extension = Path.GetExtension( file );
		if ( !CompilerVariant.GetAllRegisteredFileExtensions().Contains( extension ) )
			return;
		MarkForRecompile();
		{
			// check if Group name == Server
			var property = _internalCompilerType
				.GetProperty( "Group", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );
			var group = property.GetValue( InternalCompiler );
			var name = property.PropertyType.GetProperty( "Name", BindingFlags.Public | BindingFlags.Instance )
				.GetValue( group );
			if ( (string?)name != "Server" ) return;
		}
		Util.StartCodeIterate();
	}

	public void Dispose()
	{
	}
}
