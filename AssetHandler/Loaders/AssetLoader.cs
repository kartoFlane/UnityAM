using System.Collections.Generic;
using System.IO;

namespace AssetHandler.Loaders
{
	/// <summary>
	/// An event that is sent when the AsssetManager finishes loading an asset.
	/// </summary>
	public delegate void AssetLoadedEventHandler( AssetManager manager, AssetDescriptor desc );

	public interface IAssetLoaderParameters
	{
		AssetLoadedEventHandler OnLoaded { get; set; }

		/// <summary>
		/// Used by the AssetManager to trigger the event.
		/// </summary>
		void FireOnLoaded( AssetManager manager, AssetDescriptor desc );
	}

	public abstract class AssetLoaderParameters<T> : IAssetLoaderParameters
	{
		public AssetLoadedEventHandler OnLoaded { get; set; }

		public void FireOnLoaded( AssetManager manager, AssetDescriptor desc )
		{
			if ( OnLoaded != null ) {
				OnLoaded( manager, desc );
			}
		}
	}

	public abstract class AssetLoader
	{
		private IFileInfoResolver resolver = null;

		public AssetLoader( IFileInfoResolver resolver )
		{
			this.resolver = resolver;
		}

		public FileInfo Resolve( string path )
		{
			return resolver.Resolve( path );
		}

		/// <param name="fileName">Name of the asset to load</param>
		/// <param name="fileHandle">The resolved file to load</param>
		/// <param name="param">parameters for loading the asset</param>
		public abstract List<AssetDescriptor> GetDependencies( string fileName, FileInfo fileHandle, IAssetLoaderParameters param );
	}
}
