using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace BoxNET.Compiler.Remakes;

public class Processor
{
	public Compilation Compiler { get; set; }
	public GeneratorExecutionContext? Context { get; set; }
	public string AddonName { get; set; } = "AddonName";
	public Dictionary<string, string> AddonFileMap { get; set; } = new();
	public Dictionary<string, string> AdditionalFiles { get; set; } = new();

	public void AddTrees( IEnumerable<SyntaxTree> syntaxTrees )
	{
		if ( Context == null )
			Compiler = Compiler.AddSyntaxTrees( syntaxTrees.ToArray() );
		foreach ( var syntaxTree in syntaxTrees )
			Context?.AddSource( syntaxTree.FilePath, SourceText.From( syntaxTree.ToString(), Encoding.UTF8 ) );
	}

	public void Run( Compilation compiler )
	{
		Compiler = compiler;

		if ( Context?.AdditionalFiles != null )
		{
			foreach ( var additionalText in Context.Value.AdditionalFiles )
				AdditionalFiles[additionalText.Path] = additionalText.Path;
		}

		foreach ( var key in AdditionalFiles.Keys.Where( key =>
			         !key.Contains( "\\source\\lut\\Sandbox-Engine\\" ) &&
			         !key.Contains( "\\Sandbox.Test\\bin\\Debug\\" ) ) )
		{
			if ( key.Contains( "net7.0" ) ) AdditionalFiles.Remove( key );

			if ( key.Contains( "\\bin\\Debug\\" ) ) AdditionalFiles.Remove( key );

			if ( key.Contains( "\\bin\\Release\\" ) ) AdditionalFiles.Remove( key );
		}

		if ( Compiler.SyntaxTrees.Any() )
		{
		}
	}
}
