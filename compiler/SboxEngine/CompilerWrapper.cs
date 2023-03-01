using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using Sandbox;

namespace BoxNET.Compiler;

public class CompilerWrapper : IDisposable
{
	public CompilerWrapper( object internalCompiler )
	{
		InternalCompilerReference = new WeakReference( internalCompiler );
		_subCompiler = new CSharpCompiler( this );
	}

	private BoxNetCompiler? _subCompiler;
	protected WeakReference InternalCompilerReference { get; private set; }
	public object? InternalCompiler => InternalCompilerReference.Target;
	public bool IsActive => InternalCompilerReference.IsAlive;

	private PropertyInfo? _sourceLocationsProperty;
	private PropertyInfo? _buildResultProperty;
	private PropertyInfo? _generatedCodeProperty;
	private PropertyInfo? _assemblyNameProperty;
	private PropertyInfo? _buildSuccessProperty;
	private PropertyInfo? _settingsProperty;
	private MethodInfo? _buildReferencesMethod;
	private FieldInfo? _asmBinaryField;
	private FieldInfo? _metadataReferenceField;

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

	public void AddSourcePath( string path )
	{
		// Make sure SourceLocations is initialized
		SourceLocations ??= new List<BaseFileSystem>();

		// Create a Sandbox.RootFileSystem
		SourceLocations.Add( FsUtil.CreateRootFileSystem( path ) );

		_subCompiler?.AddSourcePath( path );
	}

	public Task BuildInternal() => _subCompiler?.BuildInternal() ?? Task.CompletedTask;

	public virtual void Dispose()
	{
	}
}
