using Microsoft.CodeAnalysis;
using Sandbox;

namespace BoxNET.Compiler;

public static class CompilerShim
{
	private static readonly List<CompilerWrapper> CompilerWrappers = new();

	/// <summary>
	/// Find <see cref="CompilerWrapper"/> for provided Sandbox.Compiler instance
	/// </summary>
	/// <param name="internalCompiler">Sandbox.Compiler instance</param>
	/// <returns>CompilerWrapper or null for none found</returns>
	private static CompilerWrapper? FindCompilerWrapper( object internalCompiler )
	{
		for ( var i = CompilerWrappers.Count - 1; i >= 0; i-- )
		{
			var compiler = CompilerWrappers[i];
			if ( !compiler.IsActive )
			{
				compiler.Dispose();
				CompilerWrappers.RemoveAt( i );
				continue;
			}

			if ( compiler == internalCompiler )
				return compiler;
		}

		return null;
	}

	/// <summary>
	/// Find or create <see cref="CompilerWrapper"/> for provided Sandbox.Compiler instance
	/// </summary>
	/// <param name="internalCompiler">Sandbox.Compiler instance</param>
	/// <returns>CompilerWrapper</returns>
	private static CompilerWrapper FindOrCreateCompilerWrapper( object internalCompiler )
	{
		{
			var wrapper = FindCompilerWrapper( internalCompiler );
			if ( wrapper != null ) return wrapper;
		}

		{
			var wrapper = new CompilerWrapper( internalCompiler );
			CompilerWrappers.Add( wrapper );
			return wrapper;
		}
	}

	public static Task BuildInternal( object internalCompiler ) =>
		FindOrCreateCompilerWrapper( internalCompiler ).BuildInternal();

	public static void OnFileChanged( object internalCompiler, string file ) =>
		FindOrCreateCompilerWrapper( internalCompiler ).OnFileChanged( file );
}
