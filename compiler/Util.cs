using System.Reflection;

namespace BoxNET.Compiler;

public static class Util
{
	public static string? ReadTextForgiving( string path, int retries = 10, int msChangeDelta = 5 )
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
