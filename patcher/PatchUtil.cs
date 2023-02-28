using Mono.Cecil;
using Mono.Cecil.Cil;

namespace BoxNET.Patcher;

public static class PatchUtil
{
	public static void RedirectMethod( this MethodDefinition method, MethodReference target ) =>
		RedirectMethod( method, target, method.Parameters.Count );

	public static void RedirectMethod( this MethodDefinition method, MethodReference target, int parameterCount )
	{
		var processor = method.Body.GetILProcessor();

		var instructions = new List<Instruction>();
		instructions.Add( processor.Create( OpCodes.Nop ) );
		instructions.Add( processor.Create( OpCodes.Ldarg_0 ) );
		if ( parameterCount > 0 )
			instructions.Add( processor.Create( OpCodes.Ldarg_1 ) );
		if ( parameterCount > 1 )
			instructions.Add( processor.Create( OpCodes.Ldarg_2 ) );
		if ( parameterCount > 2 )
			instructions.Add( processor.Create( OpCodes.Ldarg_3 ) );
		instructions.Add( processor.Create( OpCodes.Call, target ) );
		instructions.Add( processor.Create( OpCodes.Ret ) );

		foreach ( var instruction in instructions.Reverse<Instruction>() )
			processor.InsertBefore( processor.Body.Instructions.First(), instruction );
	}
}
