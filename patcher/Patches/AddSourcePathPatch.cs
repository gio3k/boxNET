using BoxNET.Compiler;

namespace BoxNET.Patcher;

public class AddSourcePathPatch : IPatch
{
	public bool Patch( AssemblyPatcher patcher )
	{
		var method = patcher.GetCompilerMethod( "AddSourcePath" );
		if ( method == null )
		{
			Static.Info( "AddSourcePath method not found" );
			return false;
		}

		var module = patcher.AssemblyDefinition.MainModule;
		var reference =
			module.ImportReference(
				typeof(CompilerShim).GetMethod( "AddSourcePath" ) );

		method.RedirectMethod( reference );

		return true;
	}
}
