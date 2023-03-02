using BoxNET.Compiler;

namespace BoxNET.Patcher;

public class OnFileChangedPatch : IPatch
{
	public bool Patch( AssemblyPatcher patcher )
	{
		var method = patcher.GetCompilerMethod( "OnFileChanged" );
		if ( method == null )
		{
			Static.Info( "OnFileChanged method not found" );
			return false;
		}

		var module = patcher.AssemblyDefinition.MainModule;
		var reference =
			module.ImportReference(
				typeof(CompilerShim).GetMethod( "OnFileChanged" ) );

		method.RedirectMethod( reference );

		return true;
	}
}
