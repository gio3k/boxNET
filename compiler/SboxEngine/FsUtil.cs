using System.Reflection;
using Sandbox;

namespace BoxNET.Compiler;

public static class FsUtil
{
	private static Type? _typeRootFileSystem;

	/// <summary>
	/// Create Sandbox.RootFileSystem
	/// </summary>
	/// <param name="path">Path</param>
	/// <returns>RootFileSystem</returns>
	public static BaseFileSystem CreateRootFileSystem( string path )
	{
		_typeRootFileSystem ??= Assembly.Load( "Sandbox.Engine" ).GetType( "Sandbox.RootFileSystem" );
		var ctor = _typeRootFileSystem.GetConstructor( BindingFlags.NonPublic | BindingFlags.Instance,
			new[] { typeof(string) } );
		var instance = ctor.Invoke( new object[] { path } );
		return (BaseFileSystem)instance;
	}

	public static string ReadTextForgiving( string path, int retries = 10, int msChangeDelta = 5 )
	{
		for ( var i = 0; i < retries; ++i )
		{
			try
			{
				return File.ReadAllText( path );
			}
			catch ( IOException )
			{
				Thread.Sleep( msChangeDelta );
			}
		}

		return null;
	}
}
