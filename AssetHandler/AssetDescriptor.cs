using AssetHandler.Loaders;
using System;
using System.IO;

namespace AssetHandler
{
	/// <summary>
	/// A descriptor of an asset, specifying the asset to load, as well as optional
	/// parameters for the loader associated with it.
	/// </summary>
	public class AssetDescriptor
	{
		public Type Type { get; set; }

		public string Path { get; set; }

		/// <summary>
		/// Optional parameters for the AssetLoader.
		/// </summary>
		public IAssetLoaderParameters Params { get; private set; }

		/// <summary>
		/// The resolved file. May be null if the file name has not been resolved yet.
		/// </summary>
		public FileInfo File { get; set; }

		public AssetDescriptor( Type type, string path, IAssetLoaderParameters param = null )
		{
			if ( path != null )
				path = path.Replace( "\\", "/" );
			Type = type;
			Path = path;
			Params = param;
		}

		public static AssetDescriptor Create<T>( string path, IAssetLoaderParameters param = null )
		{
			if ( path != null )
				path = path.Replace( "\\", "/" );
			return new AssetDescriptor( typeof( T ), path, param );
		}

		public override bool Equals( object other )
		{
			if ( other is AssetDescriptor == false )
				return false;
			return this.Equals( other as AssetDescriptor );
		}

		public bool Equals( AssetDescriptor other )
		{
			if ( other == null )
				return false;

			return CompareNullSafe( Type, other.Type ) &&
				CompareNullSafe( Path, other.Path ) &&
				CompareNullSafe( Params, other.Params );
		}

		private static bool CompareNullSafe( object o1, object o2 )
		{
			if ( o1 == null && o2 == null )
				return true;
			if ( o1 == null ^ o2 == null )
				return false;
			return o1.Equals( o2 );
		}

		public override int GetHashCode()
		{
			int hash = 17;
			hash = hash * 31 + ( Type == null ? 0 : Type.GetHashCode() );
			hash = hash * 31 + ( Path == null ? 0 : Path.GetHashCode() );
			hash = hash * 31 + ( Params == null ? 0 : Params.GetHashCode() );
			return hash;
		}

		public override string ToString()
		{
			return string.Format( "AssetDescriptor [ {0}, '{1}', {2} ]", Type, Path, Params );
		}
	}
}
