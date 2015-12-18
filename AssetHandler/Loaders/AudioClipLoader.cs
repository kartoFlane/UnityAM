using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AssetHandler.Loaders
{
	public class AudioClipLoader : SynchronousAssetLoader<AudioClip, AudioClipParameters>
	{
		public AudioClipLoader( IFileInfoResolver resolver ) : base( resolver ) { }

		public override AudioClip LoadSync( AssetManager manager, string path, FileInfo fileHandle, AudioClipParameters param )
		{
			string fullPath = Application.dataPath;
			fullPath = fullPath.Substring( 0, fullPath.LastIndexOf( "/" ) + 1 ) + path;

			WWW www = new WWW( "file://" + fullPath );
			while ( !www.isDone )
				;

			AudioClip result = www.GetAudioClip( false );
			result.name = path;

			return result;
		}

		public override List<AssetDescriptor> GetDependencies( AssetManager manager, string path, FileInfo fileHandle, AudioClipParameters param )
		{
			return null;
		}
	}

	public sealed class AudioClipParameters : AssetLoaderParameters<AudioClip>
	{
		// No parameters.
	}
}
