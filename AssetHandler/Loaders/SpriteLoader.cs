using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AssetHandler.Loaders
{
	public class SpriteLoader : SynchronousAssetLoader<Sprite, SpriteParameters>
	{
		public readonly SpriteMeshType DefaultMeshType;
		public readonly Vector4 DefaultBorders;
		public readonly uint DefaultExtrude;
		public readonly int DefaultPixelsPerUnit;
		public readonly Vector2 DefaultPivot;

		/// <summary>
		/// Defines the type of the mesh.
		/// </summary>
		private SpriteMeshType meshType;

		/// <summary>
		/// Sprite's borders, used for nine-patches (sliced sprites)
		/// </summary>
		private Vector4 borders = Vector4.zero;

		/// <summary>
		/// How much empty area to leave around the sprite in the generated mesh.
		/// </summary>
		private uint extrude = 0;

		/// <summary>
		/// Pixels per game unit. This should be constant for all sprites.
		/// </summary>
		private int pixelsPerUnit = 1;

		/// <summary>
		/// The pivot of the sprite.
		/// </summary>
		private Vector2 pivot = new Vector2( 0, 1 );

		/// <summary>
		/// Parameters for the TextureLoader
		/// </summary>
		private Texture2DParameters textureParams = null;

		public SpriteLoader( IFileInfoResolver resolver )
			: this( resolver, Vector2.zero, Vector4.zero )
		{
		}

		public SpriteLoader( IFileInfoResolver resolver,
				Vector2 pivot,
				Vector4 borders,
				SpriteMeshType meshType = SpriteMeshType.FullRect,
				uint extrude = 0,
				int pxPerUnit = 100 )
			: base( resolver )
		{
			DefaultMeshType = meshType;
			DefaultBorders = borders;
			DefaultExtrude = extrude;
			DefaultPixelsPerUnit = pxPerUnit;
			DefaultPivot = pivot;
		}

		public override Sprite LoadSync( AssetManager manager, string path, FileInfo fileHandle, SpriteParameters param )
		{
			Texture2D tex = manager.Get<Texture2D>( path, textureParams );
			Rect rect = new Rect( 0, 0, tex.width, tex.height );

			Sprite result = Sprite.Create( tex, rect, pivot, pixelsPerUnit, extrude, meshType, borders );
			result.name = path;
			return result;
		}

		public override List<AssetDescriptor> GetDependencies( string path, FileInfo fileHandle, SpriteParameters param )
		{
			meshType = DefaultMeshType;
			borders = DefaultBorders;
			extrude = DefaultExtrude;
			pixelsPerUnit = DefaultPixelsPerUnit;
			pivot = DefaultPivot;

			textureParams = null;

			if ( param != null ) {
				textureParams = new Texture2DParameters();
				textureParams.filterMode = param.filterMode;
				textureParams.wrapMode = param.wrapMode;

				meshType = param.meshType;
				borders = param.borders;
				extrude = param.extrude;
				pivot = param.pivot;

				if ( param.pixelsPerUnit > 0 )
					pixelsPerUnit = param.pixelsPerUnit;
			}

			return new List<AssetDescriptor>() { AssetDescriptor.Create<Texture2D>( path, textureParams ) };
		}
	}

	public sealed class SpriteParameters : AssetLoaderParameters<Sprite>
	{
		public FilterMode filterMode;
		public TextureWrapMode wrapMode;

		/// <summary>
		/// Defines the type of the mesh.
		/// </summary>
		public SpriteMeshType meshType;

		/// <summary>
		/// Sprite's borders, used for nine-patches (sliced sprites)
		/// </summary>
		public Vector4 borders;

		/// <summary>
		/// How much empty area to leave around the sprite in the generated mesh.
		/// </summary>
		public uint extrude;

		/// <summary>
		/// Pixels per game unit. This should be constant for all sprites.
		/// </summary>
		public int pixelsPerUnit;

		/// <summary>
		/// The pivot of the sprite.
		/// </summary>
		public Vector2 pivot;

		public override bool Equals( object obj )
		{
			if ( obj is SpriteParameters == false )
				return false;
			SpriteParameters o = (SpriteParameters)obj;
			return filterMode == o.filterMode &&
				wrapMode == o.wrapMode &&
				meshType == o.meshType &&
				borders == o.borders &&
				extrude == o.extrude &&
				pixelsPerUnit == o.pixelsPerUnit &&
				pivot == o.pivot;
		}

		public override int GetHashCode()
		{
			int hash = 17;
			hash = hash * 31 + filterMode.GetHashCode();
			hash = hash * 31 + wrapMode.GetHashCode();
			hash = hash * 31 + meshType.GetHashCode();
			hash = hash * 31 + borders.GetHashCode();
			hash = hash * 31 + extrude.GetHashCode();
			hash = hash * 31 + pixelsPerUnit.GetHashCode();
			hash = hash * 31 + pivot.GetHashCode();
			return hash;
		}
	}
}
