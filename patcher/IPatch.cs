namespace BoxNET.Patcher;

public interface IPatch
{
	/// <summary>
	/// Patch a part of assembly
	/// </summary>
	/// <param name="patcher"><see cref="AssemblyPatcher"/></param>
	/// <returns>True for success</returns>
	public bool Patch( AssemblyPatcher patcher );
}
