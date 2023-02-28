using Sandbox;

namespace BoxNET.Compiler;

public class CompilerWrapper : IDisposable
{
	public CompilerWrapper( object internalCompiler ) =>
		InternalCompilerReference = new WeakReference( internalCompiler );

	private BoxNetCompiler? _subCompiler;
	protected WeakReference InternalCompilerReference { get; private set; }
	public object? InternalCompiler => InternalCompilerReference.Target;
	public bool IsActive => InternalCompilerReference.IsAlive;

	public void LoadSettings( CompilerSettings settings )
	{
		var constants = settings.DefineConstants.Split( ";",
			StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries );

		if ( constants.Contains( "BOXNET_VB" ) )
			_subCompiler ??= new VbCompiler( this );
		else if ( constants.Contains( "BOXNET_FSHARP" ) )
			_subCompiler ??= new FSharpCompiler( this );
		else
			_subCompiler ??= new CSharpCompiler( this );

		_subCompiler.LoadSettings( settings );
	}

	public void AddSourcePath( string path ) => _subCompiler?.AddSourcePath( path );

	public virtual void Dispose()
	{
	}
}
