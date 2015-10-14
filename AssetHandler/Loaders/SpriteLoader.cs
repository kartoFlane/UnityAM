﻿using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AssetHandler.Loaders
{
	public class SpriteLoader : SynchronousAssetLoader<Sprite, SpriteParameters>
	{
		// Constants
		private static readonly Vector2 _pivot = new Vector2( 0.5f, 0.5f );
		private static readonly int _pixelsPerUnit = 100;
		private static readonly uint _extrude = 0;
		private static readonly SpriteMeshType _meshType = SpriteMeshType.FullRect;

		// Loader parameters

		/// <summary>
		/// Sprite's borders, used for nine-patches (sliced sprites)
		/// </summary>
		private Vector4 borders = Vector4.zero;

		/// <summary>
		/// Parameters for the TextureLoader
		/// </summary>
		private Texture2DParameters textureParams = null;

		public SpriteLoader( IFileInfoResolver resolver ) : base( resolver ) { }

		public override Sprite LoadSync( AssetManager manager, string path, FileInfo fileHandle, SpriteParameters param )
		{
			Texture2D tex = manager.Get<Texture2D>( path, textureParams );
			Rect rect = new Rect( 0, 0, tex.width, tex.height );

			Sprite result = Sprite.Create( tex, rect, _pivot, _pixelsPerUnit, _extrude, _meshType, borders );
			result.name = path;
			return result;
		}

		public override List<AssetDescriptor> GetDependencies( string path, FileInfo fileHandle, SpriteParameters param )
		{
			borders = Vector4.zero;
			textureParams = null;

			if ( param != null ) {
				textureParams = new Texture2DParameters();
				textureParams.filterMode = param.filterMode;
				textureParams.wrapMode = param.wrapMode;

				borders = param.borders;
			}

			return new List<AssetDescriptor>() { AssetDescriptor.Create<Texture2D>( path, textureParams ) };
		}
	}

	public sealed class SpriteParameters : AssetLoaderParameters<Sprite>
	{
		public FilterMode filterMode = FilterMode.Point;
		public TextureWrapMode wrapMode = TextureWrapMode.Clamp;
		public Vector4 borders = Vector4.zero;

		public override bool Equals( object obj )
		{
			if ( obj is SpriteParameters == false )
				return false;
			SpriteParameters o = (SpriteParameters)obj;
			return filterMode == o.filterMode && wrapMode == o.wrapMode && borders == o.borders;
		}

		public override int GetHashCode()
		{
			int hash = 17;
			hash = hash * 31 + filterMode.GetHashCode();
			hash = hash * 31 + wrapMode.GetHashCode();
			hash = hash * 31 + borders.GetHashCode();
			return hash;
		}
	}
}
