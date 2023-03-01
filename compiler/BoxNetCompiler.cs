using System.Reflection;
using Microsoft.CodeAnalysis;
using Sandbox;

namespace BoxNET.Compiler;

public abstract class BoxNetCompiler
{
	protected CompilerWrapper Wrapper { get; private set; }

	protected BoxNetCompiler( CompilerWrapper wrapper ) => Wrapper = wrapper;

	public virtual void LoadSettings( CompilerSettings settings ) { }

	public virtual void AddSourcePath( string path ) { }

	public virtual Task BuildInternal() => Task.CompletedTask;
}
