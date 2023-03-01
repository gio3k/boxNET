using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Sandbox;

namespace BoxNET.Compiler;

public class CSharpCompiler : BoxNetCompiler
{
	private CSharpParseOptions? _parseOptions;
	private bool _isRelease = true;
	private readonly Dictionary<string, string> _sourceFileMap = new();
	private MethodInfo? _runGeneratorsMethod;

	public CSharpCompiler( CompilerWrapper wrapper ) : base( wrapper )
	{
	}

	public override void LoadSettings( CompilerSettings settings )
	{
		base.LoadSettings( settings );

		var constants = settings.DefineConstants.Split( ";",
			StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries ).ToHashSet();

		_parseOptions = CSharpParseOptions.Default.WithLanguageVersion( LanguageVersion.CSharp11 )
			.WithPreprocessorSymbols( constants );

		_isRelease = settings.ReleaseMode == CompilerReleaseMode.Release;
	}

	public override async Task BuildInternal()
	{
		Wrapper.BuildSuccess = false;

		var refs = Wrapper.BuildReferences();
		var trees = new List<SyntaxTree>();

		GetSyntaxTree( trees );

		var diags = Wrapper.Settings.NoWarn.Split( ";",
			StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries );

		var options = new CSharpCompilationOptions( OutputKind.DynamicallyLinkedLibrary ).WithConcurrentBuild( true )
			.WithDeterministic( _isRelease )
			.WithOptimizationLevel( _isRelease ? OptimizationLevel.Release : OptimizationLevel.Debug )
			.WithGeneralDiagnosticOption( ReportDiagnostic.Info )
			.WithPlatform( Platform.X64 )
			.WithNullableContextOptions( Wrapper.Settings.Nullables
				? NullableContextOptions.Enable
				: NullableContextOptions.Disable )
			.WithAllowUnsafe( false );

		var compiler = CSharpCompilation.Create( Wrapper.AssemblyName, trees.OrderByDescending( v => v.FilePath ), refs,
			options );

		trees.Clear();

		compiler = RunGenerators( compiler );

		using ( var stream = new MemoryStream() )
		{
			Wrapper.BuildResult = compiler.Emit( stream,
				options: new EmitOptions().WithDebugInformationFormat( DebugInformationFormat.Embedded ) );
			Wrapper.AsmBinary = stream.ToArray();
		}

		Wrapper.BuildSuccess = Wrapper.BuildResult.Success;

		if ( Wrapper.BuildSuccess )
		{
			using var stream = new MemoryStream( Wrapper.AsmBinary );
			Wrapper.MetadataReference = MetadataReference.CreateFromStream( stream );
		}

		Sandbox.Internal.GlobalSystemNamespace.Log.Info( $"compile {Wrapper.BuildResult.Success} - {this}" );
	}

	public CSharpCompilation RunGenerators( CSharpCompilation compiler )
	{
		_runGeneratorsMethod ??= Wrapper.InternalCompiler.GetType()
			.GetMethod( "RunGenerators", BindingFlags.NonPublic | BindingFlags.Instance );
		return (CSharpCompilation)_runGeneratorsMethod.Invoke( Wrapper.InternalCompiler, new[] { compiler } );
	}

	public void GetSyntaxTree( List<SyntaxTree> codeFiles, int? buildNumber = null )
	{
		try
		{
			foreach ( var sourceLocation in Wrapper.SourceLocations ) AddSourceFiles( sourceLocation, codeFiles );

			var generatedCode = Wrapper.GeneratedCode.ToString();
			if ( string.IsNullOrEmpty( generatedCode ) )
				return;

			var tree = CSharpSyntaxTree.ParseText( generatedCode, _parseOptions, "/.obj/__compiler_extra.cs",
				Encoding.UTF8 );

			codeFiles.Add( tree );
		}
		catch ( Exception e )
		{
			Sandbox.Internal.GlobalSystemNamespace.Log.Warning( $"boxNET GetSyntaxTree fail! {e}" );
		}
	}

	private void AddSourceFiles( BaseFileSystem fileSystem, List<SyntaxTree> codeFiles )
	{
		Parallel.ForEach<string>( fileSystem.FindFile( "/", "*.cs", true ), file =>
		{
			if ( file.StartsWith( "obj/" ) )
				return; // Skip files in obj folder

			var path = fileSystem.GetFullPath( file );
			var text = FsUtil.ReadTextForgiving( path );

			lock ( _sourceFileMap ) _sourceFileMap[path] = text;

			var tree = CSharpSyntaxTree.ParseText( text, _parseOptions, path, Encoding.UTF8 );

			lock ( codeFiles ) codeFiles.Add( tree );
		} );
	}
}
