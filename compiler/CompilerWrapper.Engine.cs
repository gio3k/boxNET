namespace BoxNET.Compiler;

public partial class CompilerWrapper
{
	public CompilerWrapper( object internalCompiler ) =>
		InternalCompilerReference = new WeakReference( internalCompiler );

	private WeakReference InternalCompilerReference { get; set; }
	private object? InternalCompiler => InternalCompilerReference.Target;
	public bool IsActive => InternalCompilerReference.IsAlive;
}
