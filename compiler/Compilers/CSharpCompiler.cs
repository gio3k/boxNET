using Microsoft.CodeAnalysis.CSharp;
using Sandbox;

namespace BoxNET.Compiler;

public class CSharpCompiler : BoxNetCompiler
{
	private CSharpParseOptions? _parseOptions;
	private bool _isRelease = true;

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
}
