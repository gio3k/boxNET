using System.Reflection;
using Sandbox;

namespace BoxNET.Compiler;

public abstract class BoxNetCompiler
{
	protected List<BaseFileSystem> SourceLocations { get; } = new();
	protected CompilerWrapper Wrapper { get; private set; }

	protected BoxNetCompiler( CompilerWrapper wrapper ) => Wrapper = wrapper;

	public virtual void LoadSettings( CompilerSettings settings )
	{
	}

	public virtual void AddSourcePath( string path )
	{
		// Create a Sandbox.RootFileSystem
		var rfs = Assembly.Load( "Sandbox.Engine" ).GetType( "Sandbox.RootFileSystem" );
		SourceLocations.Add( (BaseFileSystem)Activator.CreateInstance( rfs, path ) );
	}
}
