using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Sandbox;

namespace BoxNET.Compiler;

[FileExtension( ".vb" )]
[SubFileExtension( ".razor" )]
[SubFileExtension( ".scss" )]
public class VisualBasicVariant : CompilerVariant
{
	public VisualBasicVariant( CompilerWrapper wrapper ) : base( wrapper )
	{
	}

	public override SyntaxTree ParseText( string text, ParseOptions options, string path = "",
		Encoding? encoding = null )
	{
		if ( options is not VisualBasicParseOptions visualBasicParseOptions )
			throw new Exception( "Incorrect parse options type" );
		return VisualBasicSyntaxTree.ParseText( text, visualBasicParseOptions, path, encoding );
	}

	public override CompilationOptions CreateCompilationOptions() =>
		new VisualBasicCompilationOptions( OutputKind.DynamicallyLinkedLibrary ).WithConcurrentBuild( true )
			.WithDeterministic( Wrapper.Settings.ReleaseMode == CompilerReleaseMode.Release )
			.WithOptimizationLevel( Wrapper.Settings.ReleaseMode == CompilerReleaseMode.Release
				? OptimizationLevel.Release
				: OptimizationLevel.Debug )
			.WithGeneralDiagnosticOption( ReportDiagnostic.Info )
			.WithPlatform( Platform.X64 );

	public override Compilation CreateCompiler( IEnumerable<SyntaxTree>? syntaxTrees = null,
		IEnumerable<MetadataReference>? references = null,
		CompilationOptions? options = null )
	{
		if ( options is not VisualBasicCompilationOptions visualBasicCompilationOptions )
			throw new Exception( "Incorrect options type" );
		return VisualBasicCompilation.Create( Wrapper.AssemblyName,
			syntaxTrees.OrderByDescending( v => v.FilePath ), references,
			visualBasicCompilationOptions );
	}

	public override ParseOptions ParseOptions
	{
		get
		{
			var constants = Wrapper.Settings.DefineConstants.Split( ";",
				StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries ).ToHashSet();

			return VisualBasicParseOptions.Default.WithLanguageVersion( LanguageVersion.Latest );
		}
	}

	public override string CompilerExtraFileName => "/.obj/__compiler_extra.vb";
}
