using CommandLine;

namespace BoxNET.Patcher;

public class Program
{
	private static Program Instance { get; set; }

	public static void Main( string[] args ) => Instance = new Program( args );

	private void PostParse( Options opts )
	{
		if ( opts.Verbose )
			Static.ShowInfo = true;

		AssemblyPatcher patcher = new(opts.Path);
		patcher.PatchAll();

		var outputPath = opts.OutputPath ?? opts.Path;
		patcher.Write( outputPath );
	}

	private class Options
	{
		[Option( 'v', "verbose", Default = false )]
		public bool Verbose { get; set; }

		[Option( 'p', "path", Required = true )]
		public string Path { get; set; }

		[Option( 'o', "output", Required = false )]
		public string OutputPath { get; set; }
	}

	private Program( IEnumerable<string> args )
	{
		Parser.Default.ParseArguments<Options>( args )
			.WithParsed( PostParse );
	}
}
