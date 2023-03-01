namespace BoxNET.Compiler;

public class VbCompiler : BoxNetCompiler
{
	public VbCompiler( CompilerWrapper wrapper ) : base( wrapper )
	{
		Sandbox.Internal.GlobalSystemNamespace.Log.Info( "vbcompilerrrr" );
	}
}
