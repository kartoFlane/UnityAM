using System.IO;

namespace AssetHandler.Loaders
{
	/// <summary>
	/// An interface for classes that can map a file name to a FileInfo.
	/// Used to allow the AssetManager to load files from anywhere, or
	/// implement caching strategies.
	/// </summary>
	public interface IFileInfoResolver
	{
		FileInfo Resolve( string path );
	}
}
