using BoxNET.Compiler;

namespace BoxNET.Patcher;

public class GetSyntaxTreePatch : IPatch
{
	public bool Patch( AssemblyPatcher patcher )
	{
		return true;

		var method = patcher.GetCompilerMethod( "GetSyntaxTree" );
		if ( method == null )
		{
			Static.Info( "GetSyntaxTree method not found" );
			return false;
		}

		var module = patcher.AssemblyDefinition.MainModule;
		var reference =
			module.ImportReference(
				typeof(CompilerShim).GetMethod( "GetSyntaxTree" ) );

		method.RedirectMethod( reference );

		return true;
	}
}
