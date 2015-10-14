using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AssetHandler.Loaders
{
	public class TextureLoader : AsynchronousAssetLoader<Texture2D, Texture2DParameters>
	{
		private byte[] data;

		private FilterMode filterMode;
		private TextureWrapMode wrapMode;

		public TextureLoader( IFileInfoResolver resolver ) : base( resolver ) { }

		public override void LoadAsync( AssetManager manager, string path, FileInfo fileHandle, Texture2DParameters param )
		{
			data = File.ReadAllBytes( fileHandle.FullName );
		}

		public override Texture2D LoadSync( AssetManager manager, string path, FileInfo fileHandle, Texture2DParameters param )
		{
			Texture2D result = new Texture2D( 0, 0, TextureFormat.ARGB32, false );
			result.filterMode = filterMode;
			result.wrapMode = wrapMode;
			result.LoadImage( data );
			result.name = path;

			return result;
		}

		public override List<AssetDescriptor> GetDependencies( string path, FileInfo fileHandle, Texture2DParameters param )
		{
			data = null;

			filterMode = FilterMode.Point;
			wrapMode = TextureWrapMode.Clamp;

			if ( param != null ) {
				filterMode = param.filterMode;
				wrapMode = param.wrapMode;
			}

			return null;
		}
	}

	public sealed class Texture2DParameters : AssetLoaderParameters<Texture2D>
	{
		public FilterMode filterMode = FilterMode.Point;
		public TextureWrapMode wrapMode = TextureWrapMode.Clamp;

		public override bool Equals( object obj )
		{
			if ( obj is Texture2DParameters == false )
				return false;
			Texture2DParameters o = (Texture2DParameters)obj;
			return filterMode == o.filterMode && wrapMode == o.wrapMode;
		}

		public override int GetHashCode()
		{
			int hash = 17;
			hash = hash * 31 + filterMode.GetHashCode();
			hash = hash * 31 + wrapMode.GetHashCode();
			return hash;
		}
	}
}
