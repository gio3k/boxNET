using System.Reflection;
using Mono.Cecil;

namespace BoxNET.Patcher;

public class AssemblyPatcher
{
	public AssemblyPatcher( string path )
	{
		AssemblyDefinition = AssemblyDefinition.ReadAssembly( path,
			new ReaderParameters { ReadWrite = true, ReadingMode = ReadingMode.Immediate, InMemory = true } );
		CompilerType = AssemblyDefinition.MainModule.GetType( "Sandbox.Compiler" );
	}

	public AssemblyDefinition AssemblyDefinition { get; }
	public TypeDefinition CompilerType { get; }

	public void PatchAll()
	{
		foreach ( var type in AppDomain.CurrentDomain.GetAssemblies()
			         .SelectMany( s => s.GetTypes() )
			         .Where( p => typeof(IPatch).IsAssignableFrom( p ) ) )
		{
			if ( type == typeof(IPatch) ) continue;

			Static.Info( $"patching... ({type.Name})" );
			var instance = (IPatch?)Activator.CreateInstance( type );

			if ( instance == null || !instance.Patch( this ) )
				Static.Info( "^ patch failed" );
			else
				Static.Info( "^ patch succeeded" );
		}
	}

	public void Patch<T>() where T : IPatch, new()
	{
		var patch = new T();
		patch.Patch( this );
	}

	public void Write( string outputPath )
	{
		Static.Info( $"writing to {outputPath}..." );
		AssemblyDefinition.Write( outputPath );
	}

	/// <summary>
	/// Get Sandbox.Compiler method
	/// </summary>
	/// <param name="name">Method name</param>
	/// <returns><see cref="MethodDefinition"/> or null</returns>
	public MethodDefinition? GetCompilerMethod( string name ) =>
		CompilerType.Methods.SingleOrDefault( v => v.Name == name );
}
