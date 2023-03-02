using System.Reflection;
using Microsoft.CodeAnalysis;
using Sandbox.Internal;

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

	public static ICSharpCompiler.Diagnostic CreateInternalDiagnostic( Diagnostic diagnostic, string name )
	{
		var result = new ICSharpCompiler.Diagnostic
		{
			Project = name,
			Severity = (ICSharpCompiler.DiagnosticSeverity)diagnostic.Severity,
			Code = diagnostic.Id,
			Message = diagnostic.GetMessage()
		};

		var mappedLineSpan = diagnostic.Location.GetMappedLineSpan();
		result.FilePath = mappedLineSpan.HasMappedPath ? mappedLineSpan.Path : diagnostic.Location.GetLineSpan().Path;
		result.LineNumber = mappedLineSpan.Span.Start.Line + 1;
		result.CharNumber = mappedLineSpan.Span.Start.Character + 1;

		return result;
	}

	public static void StartCodeIterate()
	{
		var assembly = Assembly.Load( "Sandbox.Engine" );
		var type = assembly.GetType( "Sandbox.Diagnostics.CodeIterate" );
		type.GetMethod( "Start", BindingFlags.Public | BindingFlags.Static ).Invoke( null, null );
	}
}
