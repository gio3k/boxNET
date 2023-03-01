using BoxNET.Compiler;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace BoxNET.Patcher;

public class BuildPatch : IPatch
{
	public bool Patch( AssemblyPatcher patcher )
	{
		var type =
			patcher.CompilerType.NestedTypes.SingleOrDefault( v => v.Name == "<Build>d__68" );
		if ( type == null )
		{
			Static.Info( "Sandbox.Compiler+<Build>d__68 not found" );
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
				typeof(CompilerShim).GetMethod( "BuildInternal" ) );

		foreach ( var bodyInstruction in method.Body.Instructions.Where( bodyInstruction =>
			         bodyInstruction.Operand is MethodDefinition { Name: "BuildInternal" } ) )
		{
			var processor = method.Body.GetILProcessor();
			processor.Replace(
				bodyInstruction,
				processor.Create( OpCodes.Call, reference )
			);
			return true;
		}

		return false;
	}
}
