using System.Collections.Generic;
using System.IO;

namespace AssetHandler.Loaders
{
	public abstract class SynchronousAssetLoader : AssetLoader
	{
		public SynchronousAssetLoader( IFileInfoResolver resolver ) : base( resolver ) { }

		public abstract object LoadSync( AssetManager manager, string path, FileInfo fileHandle, IAssetLoaderParameters param );
	}

	public abstract class SynchronousAssetLoader<T, P> : SynchronousAssetLoader
		where P : AssetLoaderParameters<T>
	{
		public SynchronousAssetLoader( IFileInfoResolver resolver ) : base( resolver ) { }

		public abstract T LoadSync( AssetManager manager, string path, FileInfo fileHandle, P param );

		public override object LoadSync( AssetManager manager, string path, FileInfo fileHandle, IAssetLoaderParameters param )
		{
			return LoadSync( manager, path, fileHandle, (P)param );
		}

		public abstract List<AssetDescriptor> GetDependencies( string path, FileInfo fileHandle, P param );

		public override List<AssetDescriptor> GetDependencies( string path, FileInfo fileHandle, IAssetLoaderParameters param )
		{
			return GetDependencies( path, fileHandle, (P)param );
		}
	}
}
