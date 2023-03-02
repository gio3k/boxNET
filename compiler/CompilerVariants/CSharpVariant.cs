using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Sandbox;

namespace BoxNET.Compiler;

[FileExtension( ".cs" )]
[SubFileExtension( ".razor" )]
public class CSharpVariant : CompilerVariant
{
	public override string ShortName => "CS";

	public CSharpVariant( CompilerWrapper wrapper ) : base( wrapper )
	{
	}

	public override SyntaxTree ParseText( string text, ParseOptions options, string path = "",
		Encoding? encoding = null )
	{
		if ( options is not CSharpParseOptions cSharpParseOptions )
			throw new Exception( "Incorrect parse options type" );
		return CSharpSyntaxTree.ParseText( text, cSharpParseOptions, path, encoding );
	}

	public override CompilationOptions CreateCompilationOptions() =>
		new CSharpCompilationOptions( OutputKind.DynamicallyLinkedLibrary ).WithConcurrentBuild( true )
			.WithDeterministic( Wrapper.Settings.ReleaseMode == CompilerReleaseMode.Release )
			.WithOptimizationLevel( Wrapper.Settings.ReleaseMode == CompilerReleaseMode.Release
				? OptimizationLevel.Release
				: OptimizationLevel.Debug )
			.WithGeneralDiagnosticOption( ReportDiagnostic.Info )
			.WithPlatform( Platform.X64 )
			.WithNullableContextOptions( Wrapper.Settings.Nullables
				? NullableContextOptions.Enable
				: NullableContextOptions.Disable )
			.WithAllowUnsafe( false );

	public override Compilation CreateCompiler( IEnumerable<SyntaxTree>? syntaxTrees = null,
		IEnumerable<MetadataReference>? references = null,
		CompilationOptions? options = null )
	{
		if ( options is not CSharpCompilationOptions cSharpCompilationOptions )
			throw new Exception( "Incorrect options type" );
		return CSharpCompilation.Create( Wrapper.AssemblyName,
			syntaxTrees.OrderByDescending( v => v.FilePath ), references,
			cSharpCompilationOptions );
	}

	public override ParseOptions ParseOptions
	{
		get
		{
			var constants = Wrapper.Settings.DefineConstants.Split( ";",
				StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries ).ToHashSet();

			return CSharpParseOptions.Default.WithLanguageVersion( LanguageVersion.CSharp11 )
				.WithPreprocessorSymbols( constants );
		}
	}

	public override string CompilerExtraFileName => "/.obj/__compiler_extra.cs";
}
