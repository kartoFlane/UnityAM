using System.Collections.Generic;
using System.IO;

namespace AssetHandler.Loaders
{
	/// <summary>
	/// Base class for asynchronous AssetLoaders. Such loaders try to load parts of an OpenGL resource
	/// on a separate thread to then load the actual resource on the thread the OpenGL context is active on.
	/// </summary>
	public abstract class AsynchronousAssetLoader : AssetLoader
	{
		public AsynchronousAssetLoader( IFileInfoResolver resolver ) : base( resolver ) { }

		public abstract object LoadSync( AssetManager manager, string path, FileInfo fileHandle, IAssetLoaderParameters param );

		public abstract void LoadAsync( AssetManager manager, string path, FileInfo fileHandle, IAssetLoaderParameters param );
	}

	public abstract class AsynchronousAssetLoader<T, P> : AsynchronousAssetLoader
		where P : AssetLoaderParameters<T>
	{
		public AsynchronousAssetLoader( IFileInfoResolver resolver ) : base( resolver ) { }

		public abstract T LoadSync( AssetManager manager, string path, FileInfo fileHandle, P param );

		public abstract void LoadAsync( AssetManager manager, string path, FileInfo fileHandle, P param );

		public override object LoadSync( AssetManager manager, string path, FileInfo fileHandle, IAssetLoaderParameters param )
		{
			return LoadSync( manager, path, fileHandle, (P)param );
		}

		public override void LoadAsync( AssetManager manager, string path, FileInfo fileHandle, IAssetLoaderParameters param )
		{
			LoadAsync( manager, path, fileHandle, (P)param );
		}

		public abstract List<AssetDescriptor> GetDependencies( AssetManager manager, string path, FileInfo fileHandle, P param );

		public override List<AssetDescriptor> GetDependencies( AssetManager manager, string path, FileInfo fileHandle, IAssetLoaderParameters param )
		{
			return GetDependencies( manager, path, fileHandle, (P)param );
		}
	}
}
