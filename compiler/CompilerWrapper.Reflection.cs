using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using Sandbox;
using Sandbox.Internal;

#pragma warning disable CS8600
#pragma warning disable CS8603
#pragma warning disable CS8602

namespace BoxNET.Compiler;

public partial class CompilerWrapper
{
	private readonly Type _internalCompilerType;
	private PropertyInfo? _sourceLocationsProperty;
	private PropertyInfo? _buildResultProperty;
	private PropertyInfo? _generatedCodeProperty;
	private PropertyInfo? _assemblyNameProperty;
	private PropertyInfo? _referencesProperty;
	private PropertyInfo? _nameProperty;
	private PropertyInfo? _diagnosticsProperty;
	private PropertyInfo? _buildSuccessProperty;
	private PropertyInfo? _settingsProperty;
	private MethodInfo? _buildReferencesMethod;
	private MethodInfo? _collectAdditionalFilesMethod;
	private MethodInfo? _markForRecompileMethod;
	private FieldInfo? _asmBinaryField;
	private FieldInfo? _metadataReferenceField;
	private FieldInfo? _compilerCounterField;

	public CompilerSettings Settings
	{
		get
		{
			_settingsProperty ??= _internalCompilerType
				.GetProperty( "Settings", BindingFlags.Public | BindingFlags.Instance );
			return (CompilerSettings)_settingsProperty.GetValue( InternalCompiler );
		}

		set
		{
			_settingsProperty ??= _internalCompilerType
				.GetProperty( "Settings", BindingFlags.Public | BindingFlags.Instance );
			_settingsProperty.SetValue( InternalCompiler, value );
		}
	}

	private bool BuildSuccess
	{
		get
		{
			_buildSuccessProperty ??= _internalCompilerType
				.GetProperty( "BuildSuccess", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance );
			return (bool)_buildSuccessProperty.GetValue( InternalCompiler );
		}

		set
		{
			_buildSuccessProperty ??= _internalCompilerType
				.GetProperty( "BuildSuccess", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance );
			_buildSuccessProperty.SetValue( InternalCompiler, value );
		}
	}

	private List<BaseFileSystem>? SourceLocations
	{
		get
		{
			_sourceLocationsProperty ??= _internalCompilerType
				.GetProperty( "SourceLocations", BindingFlags.NonPublic | BindingFlags.Instance );
			return (List<BaseFileSystem>?)_sourceLocationsProperty.GetValue( InternalCompiler );
		}

		set
		{
			_sourceLocationsProperty ??= _internalCompilerType
				.GetProperty( "SourceLocations", BindingFlags.NonPublic | BindingFlags.Instance );
			_sourceLocationsProperty.SetValue( InternalCompiler, value );
		}
	}

	private HashSet<string>? References
	{
		get
		{
			_referencesProperty ??= _internalCompilerType
				.GetProperty( "References", BindingFlags.NonPublic | BindingFlags.Instance );
			return (HashSet<string>?)_referencesProperty.GetValue( InternalCompiler );
		}

		set
		{
			_referencesProperty ??= _internalCompilerType
				.GetProperty( "References", BindingFlags.NonPublic | BindingFlags.Instance );
			_referencesProperty.SetValue( InternalCompiler, value );
		}
	}

	private ICSharpCompiler.Diagnostic[]? Diagnostics
	{
		get
		{
			_diagnosticsProperty ??= _internalCompilerType
				.GetProperty( "Diagnostics", BindingFlags.Public | BindingFlags.Instance );
			return (ICSharpCompiler.Diagnostic[])_diagnosticsProperty.GetValue( InternalCompiler );
		}

		set
		{
			_diagnosticsProperty ??= _internalCompilerType
				.GetProperty( "Diagnostics", BindingFlags.Public | BindingFlags.Instance );
			_diagnosticsProperty.SetValue( InternalCompiler, value );
		}
	}

	private byte[] AsmBinary
	{
		get
		{
			_asmBinaryField ??= _internalCompilerType
				.GetField( "AsmBinary", BindingFlags.NonPublic | BindingFlags.Instance );
			return (byte[])_asmBinaryField.GetValue( InternalCompiler );
		}

		set
		{
			_asmBinaryField ??= _internalCompilerType
				.GetField( "AsmBinary", BindingFlags.NonPublic | BindingFlags.Instance );
			_asmBinaryField.SetValue( InternalCompiler, value );
		}
	}

	private PortableExecutableReference MetadataReference
	{
		get
		{
			_metadataReferenceField ??= _internalCompilerType
				.GetField( "MetadataReference", BindingFlags.NonPublic | BindingFlags.Instance );
			return (PortableExecutableReference)_metadataReferenceField.GetValue( InternalCompiler );
		}

		set
		{
			_metadataReferenceField ??= _internalCompilerType
				.GetField( "MetadataReference", BindingFlags.NonPublic | BindingFlags.Instance );
			_metadataReferenceField.SetValue( InternalCompiler, value );
		}
	}

	private int CompilerCounter
	{
		get
		{
			_compilerCounterField ??= _internalCompilerType
				.GetField( "compileCounter", BindingFlags.NonPublic | BindingFlags.Static );
			return (int)_compilerCounterField.GetValue( InternalCompiler );
		}

		set
		{
			_compilerCounterField ??= _internalCompilerType
				.GetField( "compileCounter", BindingFlags.NonPublic | BindingFlags.Static );
			_compilerCounterField.SetValue( InternalCompiler, value );
		}
	}

	private EmitResult BuildResult
	{
		get
		{
			_buildResultProperty ??= _internalCompilerType
				.GetProperty( "BuildResult", BindingFlags.Public | BindingFlags.Instance );
			return (EmitResult)_buildResultProperty.GetValue( InternalCompiler );
		}

		set
		{
			_buildResultProperty ??= _internalCompilerType
				.GetProperty( "BuildResult", BindingFlags.Public | BindingFlags.Instance );
			_buildResultProperty.SetValue( InternalCompiler, value );
		}
	}

	private StringBuilder GeneratedCode
	{
		get
		{
			_generatedCodeProperty ??= _internalCompilerType
				.GetProperty( "GeneratedCode", BindingFlags.Public | BindingFlags.Instance );
			return (StringBuilder)_generatedCodeProperty.GetValue( InternalCompiler );
		}

		set
		{
			_generatedCodeProperty ??= _internalCompilerType
				.GetProperty( "GeneratedCode", BindingFlags.Public | BindingFlags.Instance );
			_generatedCodeProperty.SetValue( InternalCompiler, value );
		}
	}

	public string AssemblyName
	{
		get
		{
			_assemblyNameProperty ??= _internalCompilerType
				.GetProperty( "AssemblyName", BindingFlags.Public | BindingFlags.Instance );
			return (string)_assemblyNameProperty.GetValue( InternalCompiler );
		}
	}

	/// <summary>
	/// confusing name - seems to be Project Name?
	/// </summary>
	private string Name
	{
		get
		{
			_nameProperty ??= _internalCompilerType
				.GetProperty( "Name", BindingFlags.Public | BindingFlags.Instance );
			return (string)_nameProperty.GetValue( InternalCompiler );
		}
	}

	private IEnumerable<PortableExecutableReference> BuildReferences()
	{
		_buildReferencesMethod ??= _internalCompilerType
			.GetMethod( "BuildReferences", BindingFlags.NonPublic | BindingFlags.Instance );
		return (List<PortableExecutableReference>)_buildReferencesMethod.Invoke( InternalCompiler,
			Array.Empty<object>() );
	}

	private void MarkForRecompile()
	{
		_markForRecompileMethod ??= _internalCompilerType
			.GetMethod( "MarkForRecompile", BindingFlags.NonPublic | BindingFlags.Instance );
		_markForRecompileMethod.Invoke( InternalCompiler, null );
	}

	public void CollectAdditionalFiles( Dictionary<string, string> codeFiles )
	{
		_collectAdditionalFilesMethod ??= _internalCompilerType
			.GetMethod( "CollectAdditionalFiles", BindingFlags.NonPublic | BindingFlags.Instance );
		_collectAdditionalFilesMethod.Invoke( InternalCompiler, new object?[] { codeFiles } );
	}
}
