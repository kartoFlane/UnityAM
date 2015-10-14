using System.IO;

namespace AssetHandler.Loaders
{
	public class RelativeResolver : IFileInfoResolver
	{
		public FileInfo Resolve( string path )
		{
			return new FileInfo( path );
		}
	}
}
