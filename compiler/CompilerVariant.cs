using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;

namespace BoxNET.Compiler;

public abstract class CompilerVariant
{
	protected CompilerVariant( CompilerWrapper wrapper ) => Wrapper = wrapper;
	public CompilerWrapper Wrapper { get; set; }

	public abstract string ShortName { get; }

	public abstract SyntaxTree? ParseText( string text, ParseOptions options, string path = "",
		Encoding? encoding = null );

	public abstract CompilationOptions? CreateCompilationOptions();

	public abstract Compilation? CreateCompiler( IEnumerable<SyntaxTree>? syntaxTrees = null,
		IEnumerable<MetadataReference>? references = null, CompilationOptions? options = null );

	public abstract ParseOptions ParseOptions { get; }
	public abstract string CompilerExtraFileName { get; }

	private static List<(Type, List<string>, List<string>)>? _variantToFileExtCache;

	private static void CacheVariantFileExt()
	{
		_variantToFileExtCache = new List<(Type, List<string>, List<string>)>();

		var assembly = Assembly.GetAssembly( typeof(CompilerVariant) );
		foreach ( var type in assembly.GetTypes() )
		{
			if ( !type.IsSubclassOf( typeof(CompilerVariant) ) )
				continue;

			_variantToFileExtCache.Add(
				(type,
					type.GetCustomAttributes( typeof(FileExtensionAttribute) )
						.Select( customAttribute => ((FileExtensionAttribute)customAttribute).FileExtension ).ToList(),
					type.GetCustomAttributes( typeof(SubFileExtensionAttribute) )
						.Select( customAttribute => ((SubFileExtensionAttribute)customAttribute).FileExtension )
						.ToList())
			);
		}
	}

	public static CompilerVariant? CreateByFileExtension( string extension, CompilerWrapper wrapper )
	{
		if ( _variantToFileExtCache == null ) CacheVariantFileExt();
		foreach ( var (type, mainExtensions, _) in _variantToFileExtCache! )
		{
			if ( !mainExtensions.Contains( extension ) )
				continue;
			return (CompilerVariant?)Activator.CreateInstance( type, wrapper );
		}

		return null;
	}

	public static Type? FindTypeByFileExtension( string extension )
	{
		if ( _variantToFileExtCache == null ) CacheVariantFileExt();
		foreach ( var (type, mainExtensions, _) in _variantToFileExtCache! )
		{
			if ( !mainExtensions.Contains( extension ) )
				continue;
			return type;
		}

		return null;
	}

	public List<string> GetFileExtensions()
	{
		if ( _variantToFileExtCache == null ) CacheVariantFileExt();
		foreach ( var (type, mainExtensions, _) in _variantToFileExtCache! )
		{
			if ( type != GetType() ) continue;
			var result = mainExtensions.ToList();
			return result;
		}

		return null;
	}

	public List<string> GetAllFileExtensions()
	{
		if ( _variantToFileExtCache == null ) CacheVariantFileExt();
		foreach ( var (type, mainExtensions, secondaryExtensions) in _variantToFileExtCache! )
		{
			if ( type != GetType() ) continue;
			var result = mainExtensions.ToList();
			result.AddRange( secondaryExtensions );
			return result;
		}

		return null;
	}

	public static List<string> GetAllRegisteredFileExtensions()
	{
		if ( _variantToFileExtCache == null ) CacheVariantFileExt();
		var result = new List<string>();
		foreach ( var (_, mainExtensions, secondaryExtensions) in _variantToFileExtCache! )
		{
			foreach ( var v in mainExtensions.Where( v => !result.Contains( v ) ) ) result.Add( v );
			foreach ( var v in secondaryExtensions.Where( v => !result.Contains( v ) ) ) result.Add( v );
		}

		return result;
	}
}

[AttributeUsage( AttributeTargets.Class )]
public class FileExtensionAttribute : Attribute
{
	public string FileExtension { get; set; }

	public FileExtensionAttribute( string fileExtension ) => FileExtension = fileExtension;
}

[AttributeUsage( AttributeTargets.Class, AllowMultiple = true )]
public class SubFileExtensionAttribute : Attribute
{
	public string FileExtension { get; set; }

	public SubFileExtensionAttribute( string fileExtension ) => FileExtension = fileExtension;
}
