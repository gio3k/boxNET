using System.Reflection;
using System.Text;
using BoxNET.Compiler.Remakes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Sandbox;

namespace BoxNET.Compiler;

public sealed partial class CompilerWrapper : IDisposable
{
	private readonly Dictionary<string, string> _sourceFileMap = new();
	private List<string>? _variantFileExtensions = new();

	public CompilerVariant? Variant { get; set; }

	public async Task BuildInternal()
	{
		BuildSuccess = false;

		var refs = BuildReferences();
		var trees = new List<SyntaxTree>();

		GetSyntaxTree( trees, CompilerCounter++ );

		var diags = Settings.NoWarn.Split( ";",
			StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries );

		var options = Variant.CreateCompilationOptions();

		var compiler = Variant.CreateCompiler( trees.OrderByDescending( v => v.FilePath ), refs, options );

		trees.Clear();

		compiler = RunGenerators( compiler );

		using ( var stream = new MemoryStream() )
		{
			BuildResult = compiler.Emit( stream,
				options: new EmitOptions().WithDebugInformationFormat( DebugInformationFormat.Embedded ) );
			AsmBinary = stream.ToArray();
		}

		BuildSuccess = BuildResult.Success;

		if ( BuildSuccess )
		{
			using var stream = new MemoryStream( AsmBinary );
			MetadataReference = Microsoft.CodeAnalysis.MetadataReference.CreateFromStream( stream );
		}

		Log.Info( $"compile {BuildResult.Success} - {this}" );
	}

	public Compilation RunGenerators( Compilation compiler )
	{
		if ( Variant is CSharpVariant variant )
		{
			var runGeneratorsMethod = InternalCompiler.GetType()
				.GetMethod( "RunGenerators", BindingFlags.NonPublic | BindingFlags.Instance );
			return (CSharpCompilation)runGeneratorsMethod.Invoke( InternalCompiler, new[] { compiler } );
		}

		return compiler;
		var processor = new Processor();

		CollectAdditionalFiles( processor.AdditionalFiles );
	}

	public void GetSyntaxTree( List<SyntaxTree> codeFiles, int? buildNumber = null )
	{
		try
		{
			foreach ( var sourceLocation in SourceLocations ) AddSourceFiles( sourceLocation, codeFiles );

			var generatedCode = GeneratedCode.ToString();
			if ( string.IsNullOrEmpty( generatedCode ) )
				return;

			if ( Variant is VisualBasicVariant )
				return;

			var tree = Variant.ParseText( generatedCode, Variant.ParseOptions, Variant.CompilerExtraFileName,
				Encoding.UTF8 );

			codeFiles.Add( tree );
		}
		catch ( Exception e )
		{
			Log.Warning( $"boxNET GetSyntaxTree fail! {e}" );
		}
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
			{
				// Log.Info( $"skipping {file}, ({extension})" );
				return;
			}

			var tree = Variant?.ParseText( text, Variant.ParseOptions, path, Encoding.UTF8 );

			lock ( codeFiles ) codeFiles.Add( tree );
		} );
	}

	public void Dispose()
	{
	}
}
