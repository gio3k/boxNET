using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using Sandbox;

namespace BoxNET.Compiler;

public partial class CompilerWrapper
{
	private PropertyInfo? _sourceLocationsProperty;
	private PropertyInfo? _buildResultProperty;
	private PropertyInfo? _generatedCodeProperty;
	private PropertyInfo? _assemblyNameProperty;
	private PropertyInfo? _buildSuccessProperty;
	private PropertyInfo? _settingsProperty;
	private MethodInfo? _buildReferencesMethod;
	private MethodInfo? _collectAdditionalFilesMethod;
	private FieldInfo? _asmBinaryField;
	private FieldInfo? _metadataReferenceField;
	private FieldInfo? _compilerCounterField;

	public CompilerSettings Settings
	{
		get
		{
			_settingsProperty ??= InternalCompiler.GetType()
				.GetProperty( "Settings", BindingFlags.Public | BindingFlags.Instance );
			return (CompilerSettings)_settingsProperty.GetValue( InternalCompiler );
		}

		set
		{
			_settingsProperty ??= InternalCompiler.GetType()
				.GetProperty( "Settings", BindingFlags.Public | BindingFlags.Instance );
			_settingsProperty.SetValue( InternalCompiler, value );
		}
	}

	public bool BuildSuccess
	{
		get
		{
			_buildSuccessProperty ??= InternalCompiler.GetType()
				.GetProperty( "BuildSuccess", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance );
			return (bool)_buildSuccessProperty.GetValue( InternalCompiler );
		}

		set
		{
			_buildSuccessProperty ??= InternalCompiler.GetType()
				.GetProperty( "BuildSuccess", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance );
			_buildSuccessProperty.SetValue( InternalCompiler, value );
		}
	}

	public List<BaseFileSystem>? SourceLocations
	{
		get
		{
			_sourceLocationsProperty ??= InternalCompiler.GetType()
				.GetProperty( "SourceLocations", BindingFlags.NonPublic | BindingFlags.Instance );
			return (List<BaseFileSystem>?)_sourceLocationsProperty.GetValue( InternalCompiler );
		}

		set
		{
			_sourceLocationsProperty ??= InternalCompiler.GetType()
				.GetProperty( "SourceLocations", BindingFlags.NonPublic | BindingFlags.Instance );
			_sourceLocationsProperty.SetValue( InternalCompiler, value );
		}
	}

	public byte[] AsmBinary
	{
		get
		{
			_asmBinaryField ??= InternalCompiler.GetType()
				.GetField( "AsmBinary", BindingFlags.NonPublic | BindingFlags.Instance );
			return (byte[])_asmBinaryField.GetValue( InternalCompiler );
		}

		set
		{
			_asmBinaryField ??= InternalCompiler.GetType()
				.GetField( "AsmBinary", BindingFlags.NonPublic | BindingFlags.Instance );
			_asmBinaryField.SetValue( InternalCompiler, value );
		}
	}

	public PortableExecutableReference MetadataReference
	{
		get
		{
			_metadataReferenceField ??= InternalCompiler.GetType()
				.GetField( "MetadataReference", BindingFlags.NonPublic | BindingFlags.Instance );
			return (PortableExecutableReference)_metadataReferenceField.GetValue( InternalCompiler );
		}

		set
		{
			_metadataReferenceField ??= InternalCompiler.GetType()
				.GetField( "MetadataReference", BindingFlags.NonPublic | BindingFlags.Instance );
			_metadataReferenceField.SetValue( InternalCompiler, value );
		}
	}

	public int CompilerCounter
	{
		get
		{
			_compilerCounterField ??= InternalCompiler.GetType()
				.GetField( "compileCounter", BindingFlags.NonPublic | BindingFlags.Static );
			return (int)_compilerCounterField.GetValue( InternalCompiler );
		}

		set
		{
			_compilerCounterField ??= InternalCompiler.GetType()
				.GetField( "compileCounter", BindingFlags.NonPublic | BindingFlags.Static );
			_compilerCounterField.SetValue( InternalCompiler, value );
		}
	}

	public EmitResult BuildResult
	{
		get
		{
			_buildResultProperty ??= InternalCompiler.GetType()
				.GetProperty( "BuildResult", BindingFlags.Public | BindingFlags.Instance );
			return (EmitResult)_buildResultProperty.GetValue( InternalCompiler );
		}

		set
		{
			_buildResultProperty ??= InternalCompiler.GetType()
				.GetProperty( "BuildResult", BindingFlags.Public | BindingFlags.Instance );
			_buildResultProperty.SetValue( InternalCompiler, value );
		}
	}

	public StringBuilder GeneratedCode
	{
		get
		{
			_generatedCodeProperty ??= InternalCompiler.GetType()
				.GetProperty( "GeneratedCode", BindingFlags.Public | BindingFlags.Instance );
			return (StringBuilder)_generatedCodeProperty.GetValue( InternalCompiler );
		}

		set
		{
			_generatedCodeProperty ??= InternalCompiler.GetType()
				.GetProperty( "GeneratedCode", BindingFlags.Public | BindingFlags.Instance );
			_generatedCodeProperty.SetValue( InternalCompiler, value );
		}
	}

	public string AssemblyName
	{
		get
		{
			_assemblyNameProperty ??= InternalCompiler.GetType()
				.GetProperty( "AssemblyName", BindingFlags.Public | BindingFlags.Instance );
			return (string)_assemblyNameProperty.GetValue( InternalCompiler );
		}
	}

	public IEnumerable<PortableExecutableReference> BuildReferences()
	{
		_buildReferencesMethod ??= InternalCompiler.GetType()
			.GetMethod( "BuildReferences", BindingFlags.NonPublic | BindingFlags.Instance );
		return (List<PortableExecutableReference>)_buildReferencesMethod.Invoke( InternalCompiler,
			Array.Empty<object>() );
	}

	public void CollectAdditionalFiles( Dictionary<string, string> codeFiles )
	{
		_collectAdditionalFilesMethod ??= InternalCompiler.GetType()
			.GetMethod( "CollectAdditionalFiles", BindingFlags.NonPublic | BindingFlags.Instance );
		_collectAdditionalFilesMethod.Invoke( InternalCompiler, new object?[] { codeFiles } );
	}
}
