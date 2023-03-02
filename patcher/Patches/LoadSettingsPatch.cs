namespace BoxNET.Patcher;

public class LoadSettingsPatch : IPatch
{
	public bool Patch( AssemblyPatcher patcher )
	{
		return true;

		var method = patcher.GetCompilerMethod( "LoadSettings" );
		if ( method == null )
		{
			Static.Info( "LoadSettings method not found" );
			return false;
		}

		var module = patcher.AssemblyDefinition.MainModule;
		var reference =
			module.ImportReference(
				typeof(Compiler.CompilerShim).GetMethod( "LoadSettings" ) );

		method.RedirectMethod( reference );

		return true;
	}
}
