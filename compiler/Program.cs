global using static Sandbox.Internal.GlobalSystemNamespace;

namespace BoxNET.Compiler;

public class Program
{
	public static Program Instance { get; private set; }

	public static void Main( string[] args ) => Instance = new Program( args );

	private Program( string[] args )
	{
	}
}
