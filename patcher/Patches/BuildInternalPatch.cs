using BoxNET.Compiler;

namespace BoxNET.Patcher;

public class BuildInternalPatch : IPatch
{
	public bool Patch( AssemblyPatcher patcher )
	{
		var type =
			patcher.CompilerType.NestedTypes.SingleOrDefault( v => v.Name == "<BuildInternal>d__65" );
		if ( type == null )
		{
			Static.Info( "Sandbox.Compiler+<BuildInternal>d__65 not found" );
			return false;
		}

		var method = type.Methods.SingleOrDefault( v => v.Name == "MoveNext" );
		if ( method == null )
		{
			Static.Info( "MoveNext not found" );
			return false;
		}

		var module = patcher.AssemblyDefinition.MainModule;
		var reference =
			module.ImportReference(
				typeof(CompilerShim).GetMethod( nameof(CompilerShim.BuildInternal_MoveNext) ) );

		method.RedirectMethod( reference );

		return true;
	}
}
