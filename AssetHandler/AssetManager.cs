/*******************************************************************************
 * Copyright 2011 https://github.com/libgdx/libgdx/blob/master/AUTHORS
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *   http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 ******************************************************************************/

using AssetHandler.Loaders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

namespace AssetHandler
{
	/// <summary>
	/// A class used to load and keep track of assets.
	/// This is a port of the AssetManager class from libGDX, a Java game development platform.
	/// </summary>
	/// 
	/// Original implementation:
	/// https://github.com/libgdx/libgdx/blob/master/gdx/src/com/badlogic/gdx/assets/AssetManager.java
	/// 
	/// Things missing from this implementation compared to original:
	/// - some public query methods
	/// - error listener
	/// - logging
	/// - diagnostics
	/// - some comments / documentation
	///
	public sealed class AssetManager
	{
		private readonly Dictionary<AssetDescriptor, IReferencedObject> assets =
			new Dictionary<AssetDescriptor, IReferencedObject>();
		private readonly Dictionary<AssetDescriptor, List<AssetDescriptor>> assetDependencies =
			new Dictionary<AssetDescriptor, List<AssetDescriptor>>();

		private readonly Dictionary<Type, AssetLoader> loaders = new Dictionary<Type, AssetLoader>();

		// C# versions of Queue and Stack don't allow getting an element by index.
		// Since we need the ability to do that sometimes, we use List instead.
		private readonly List<AssetDescriptor> loadQueue = new List<AssetDescriptor>();
		private readonly List<AssetLoadingTask> taskStack = new List<AssetLoadingTask>();

		private int toLoad = 0;
		private int loaded = 0;
		private long timeStarted = 0;

		public AssetManager()
			: this( new RelativeResolver() )
		{
		}

		public AssetManager( IFileInfoResolver resolver )
		{
			SetLoader<Texture2D>( new TextureLoader( resolver ) );
			SetLoader<Sprite>( new SpriteLoader( resolver ) );
			SetLoader<AudioClip>( new AudioClipLoader( resolver ) );
			// TODO: Add loaders for basic Unity resources here.
			// Loaders for custom resources should be added in the method
			// that created the AssetManager.
		}

		// 
		// Methods for registering loaders for asset types.
		//
		#region Loader Setting / Getting

		/// <summary>
		/// Sets a loader for asset of the given type.
		/// </summary>
		public void SetLoader<T>( AssetLoader loader )
		{
			if ( loader == null )
				throw new ArgumentNullException( "loader" );

			SetLoader( typeof( T ), loader );
		}

		/// <summary>
		/// Sets a loader for asset of the given type.
		/// </summary>
		public void SetLoader( Type type, AssetLoader loader )
		{
			if ( type == null )
				throw new ArgumentNullException( "type" );
			if ( loader == null )
				throw new ArgumentNullException( "loader" );

			using ( Key.Lock( loaders ) ) {
				loaders.Add( type, loader );
			}
		}

		/// <summary>
		/// Returns the loader currently set for assets of the given type, or null if not set.
		/// </summary>
		public AssetLoader GetLoader<T>()
		{
			return GetLoader( typeof( T ) );
		}

		/// <summary>
		/// Returns the loader currently set for assets of the given type, or null if not set.
		/// </summary>
		public AssetLoader GetLoader( Type type )
		{
			using ( Key.Lock( loaders ) ) {
				if ( loaders.ContainsKey( type ) ) {
					return loaders[type];
				}
				return null;
			}
		}

		#endregion

		//
		// Methods used to schedule the loading of an asset, or immediate unloading thereof.
		//
		#region Load / Unload Methods

		/// <summary>
		/// Schedules the specified asset for loading. The request will not be processed
		/// until Update() is called.
		/// </summary>
		public void Load<T>( string path, IAssetLoaderParameters param = null )
		{
			if ( path == null )
				throw new ArgumentNullException( "path" );
			Load( new AssetDescriptor( typeof( T ), path, param ) );
		}

		/// <summary>
		/// Schedules the specified asset for loading. The request will not be processed
		/// until Update() is called.
		/// </summary>
		public void Load( Type type, string path, IAssetLoaderParameters param = null )
		{
			if ( type == null )
				throw new ArgumentNullException( "type" );
			if ( path == null )
				throw new ArgumentNullException( "path" );
			Load( new AssetDescriptor( type, path, param ) );
		}

		/// <summary>
		/// Schedules the specified asset for loading. The request will not be processed
		/// until Update() is called.
		/// </summary>
		public void Load( AssetDescriptor descriptor )
		{
			AssetLoader loader = GetLoader( descriptor.Type );
			if ( loader == null )
				throw new Exception( "No loader for type: " + descriptor.Type );

			using ( Key.Lock( loadQueue ) ) {
				// Reset progress stats
				if ( loadQueue.Count == 0 ) {
					loaded = 0;
					toLoad = 0;
					timeStarted = 0;
				}

				loadQueue.Enqueue( descriptor );
				++toLoad;
			}
		}

		/// <summary>
		/// Unloads the asset associated with the specified path.
		/// If there are no references left to the asset, it is removed and destroyed, if needed.
		/// </summary>
		public void Unload( Type type, string path, IAssetLoaderParameters param = null )
		{
			Unload( new AssetDescriptor( type, path, param ) );
		}

		/// <summary>
		/// Unloads the asset associated with the specified path.
		/// If there are no references left to the asset, it is removed and destroyed, if needed.
		/// </summary>
		public void Unload<T>( string path, IAssetLoaderParameters param = null )
		{
			Unload( new AssetDescriptor( typeof( T ), path, param ) );
		}

		/// <summary>
		/// Unloads the asset associated with the specified path.
		/// If there are no references left to the asset, it is removed and destroyed, if needed.
		/// </summary>
		public void Unload( AssetDescriptor desc )
		{
			using ( Key.Lock( loadQueue, taskStack, assets ) ) {
				// Check if the asset is not scheduled for loading first
				int foundIndex = -1;
				for ( int i = 0; i < loadQueue.Count; ++i ) {
					if ( loadQueue[i].Path == desc.Path ) {
						foundIndex = i;
						break;
					}
				}
				if ( foundIndex != -1 ) {
					--toLoad;
					loadQueue.RemoveAt( foundIndex );
					return;
				}

				if ( taskStack.Count > 0 ) {
					AssetLoadingTask task = taskStack[0];
					if ( task.AssetDesc.Path == desc.Path ) {
						task.Cancel();
						return;
					}
				}

				if ( !assets.ContainsKey( desc ) ) {
					return;
				}

				IReferencedObject ro = assets[desc];

				// Decrement reference count, and get rid of the asset if there are no references left
				--ro.RefCount;
				if ( ro.RefCount <= 0 ) {
					Dispose( ro.Asset );

					assets.Remove( desc );
				}

				// Remove any dependencies (or just decrement their ref count)
				if ( assetDependencies.ContainsKey( desc ) ) {
					foreach ( AssetDescriptor dependency in assetDependencies[desc] ) {
						if ( IsLoaded( dependency.Type, dependency.Path ) )
							Unload( dependency.Type, dependency.Path );
					}
				}

				// Remove dependencies if ref count <= 0
				if ( ro.RefCount <= 0 ) {
					assetDependencies.Remove( desc );
				}
			}
		}

		#endregion

		// 
		// Methods that supply information about the AssetManager's current state,
		// or used to retrieve the assets that have been loaded thus far.
		// 
		#region Query Methods and Properties

		public int LoadedAssetCount
		{
			get
			{
				using ( Key.Lock( assets ) )
					return assets.Count;
			}
		}

		public int QueuedAssetCount
		{
			get
			{
				using ( Key.Lock( loadQueue, taskStack ) )
					return loadQueue.Count + taskStack.Count;
			}
		}

		public long TimeStarted
		{
			get
			{
				return timeStarted;
			}
		}

		public float Progress
		{
			get
			{
				if ( toLoad == 0 )
					return 1;
				return Mathf.Min( 1, loaded / (float)toLoad );
			}
		}

		public bool IsLoaded( AssetDescriptor desc )
		{
			using ( Key.Lock( assets ) ) {
				return assets.ContainsKey( desc );
			}
		}

		public bool IsLoaded<T>( string path )
		{
			if ( path == null )
				return false;

			using ( Key.Lock( assets ) ) {
				return assets.ContainsKey( new AssetDescriptor( typeof( T ), path ) );
			}
		}

		public bool IsLoaded( Type type, string path, IAssetLoaderParameters param = null )
		{
			if ( type == null || path == null )
				return false;

			using ( Key.Lock( assets ) ) {
				return assets.ContainsKey( new AssetDescriptor( type, path ) );
			}
		}

		public bool ContainsAsset<T>( T asset )
		{
			if ( asset == null )
				throw new ArgumentNullException( "asset" );

			using ( Key.Lock( assets ) ) {
				foreach ( IReferencedObject ro in assets.Values ) {
					object otherAsset = ro.Asset;
					if ( asset.Equals( otherAsset ) )
						return true;
				}

				return false;
			}
		}

		public string GetAssetPath<T>( T asset )
		{
			if ( asset == null )
				throw new ArgumentNullException( "asset" );

			using ( Key.Lock( assets ) ) {
				foreach ( AssetDescriptor desc in assets.Keys ) {
					object otherAsset = assets[desc].Asset;
					if ( otherAsset.Equals( asset ) ) {
						return desc.Path;
					}
				}

				return null;
			}
		}

		public int GetRefCount<T>( T asset )
		{
			if ( asset == null )
				throw new ArgumentNullException( "asset" );

			using ( Key.Lock( assets ) ) {
				foreach ( AssetDescriptor desc in assets.Keys ) {
					IReferencedObject ro = assets[desc];
					if ( ro.Asset.Equals( asset ) ) {
						return ro.RefCount;
					}
				}

				return 0;
			}
		}

		public int GetRefCount<T>( string path, IAssetLoaderParameters param = null )
		{
			if ( path == null )
				throw new ArgumentNullException( "path" );

			using ( Key.Lock( assets ) ) {
				AssetDescriptor desc = AssetDescriptor.Create<T>( path, param );
				if ( assets.ContainsKey( desc ) ) {
					return assets[desc].RefCount;
				}

				return 0;
			}
		}

		/// <summary>
		/// Returns the asset associated with the specified path.
		/// Throws an AssetNotLoadedException if no asset is found.
		/// </summary>
		public T Get<T>( string path, IAssetLoaderParameters param = null )
		{
			Type type = typeof( T );
			if ( type == null )
				throw new AssetNotLoadedException( typeof( T ), path );

			using ( Key.Lock( assets ) ) {
				IReferencedObject ro = assets.Get( new AssetDescriptor( type, path, param ), null );
				if ( ro == null )
					throw new AssetNotLoadedException( typeof( T ), path );

				object result = ro.Asset;
				if ( result == null )
					throw new AssetNotLoadedException( typeof( T ), path );

				return (T)result;
			}
		}

		/// <summary>
		/// Returns the asset associated with the specified path.
		/// Throws an AssetNotLoadedException if no asset is found.
		/// </summary>
		public object Get( Type type, string path )
		{
			using ( Key.Lock( assets ) ) {
				IReferencedObject ro = assets.Get( new AssetDescriptor( type, path ), null );
				if ( ro == null )
					throw new AssetNotLoadedException( type, path );

				object result = ro.Asset;
				if ( result == null )
					throw new AssetNotLoadedException( type, path );

				return result;
			}
		}

		/// <summary>
		/// Returns the asset specified by the descriptor.
		/// Throws an AssetNotLoadedException if no asset is found.
		/// </summary>
		public object Get( AssetDescriptor descriptor )
		{
			if ( descriptor == null ) {
				throw new ArgumentNullException( "descriptor" );
			}

			using ( Key.Lock( assets ) ) {
				IReferencedObject ro = assets.Get( descriptor, null );
				if ( ro == null )
					throw new AssetNotLoadedException( descriptor );

				object result = ro.Asset;
				if ( result == null )
					throw new AssetNotLoadedException( descriptor );

				return result;
			}
		}

		#endregion

		//
		// Methods used to actually process and load the scheduled assets, or clear them.
		//
		#region Update / Clear Methods

		/// <summary>
		/// Updates the AssetManager, causing it to load any assets in the preload queue.
		/// Returns true if all loading is finished.
		/// </summary>
		public bool Update()
		{
			try {
				using ( Key.Lock( loadQueue, taskStack ) ) {
					if ( timeStarted == 0 )
						timeStarted = Environment.TickCount;

					if ( taskStack.Count == 0 ) {
						while ( loadQueue.Count > 0 && taskStack.Count == 0 )
							NextTask();

						if ( taskStack.Count == 0 )
							return true;
					}

					return UpdateTask() && loadQueue.Count == 0 && taskStack.Count == 0;
				}
			}
			catch ( Exception ex ) {
				HandleTaskError( ex );
				return loadQueue.Count == 0;
			}
		}

		/// <summary>
		/// Updates the AssetManager continuously for the specified number of miliseconds.
		/// </summary>
		public bool Update( int miliseconds )
		{
			long endTime = Environment.TickCount + miliseconds;
			while ( true ) {
				bool done = Update();
				if ( done || Environment.TickCount > endTime )
					return done;
				System.Threading.Thread.Sleep( 0 );
			}
		}

		/// <summary>
		/// Blocks until all assets are loaded.
		/// </summary>
		public void FinishLoading()
		{
			while ( !Update() )
				;
		}

		public void Clear()
		{
			using ( Key.Lock( loadQueue, taskStack, assets, assetDependencies ) ) {
				loadQueue.Clear();
				FinishLoading();

				Dictionary<AssetDescriptor, int> dependencyCount = new Dictionary<AssetDescriptor, int>();
				while ( assets.Count > 0 ) {
					dependencyCount.Clear();

					AssetDescriptor[] descs = new AssetDescriptor[assets.Keys.Count];
					assets.Keys.CopyTo( descs, 0 );

					foreach ( AssetDescriptor desc in descs ) {
						if ( !assetDependencies.ContainsKey( desc ) )
							continue;
						List<AssetDescriptor> dependencies = assetDependencies[desc];
						foreach ( AssetDescriptor dependency in dependencies ) {
							int count = dependencyCount.Get( dependency, 0 );
							dependencyCount[dependency] = ++count;
						}
					}

					// Only dispose of assets that are root assets (not referenced)
					foreach ( AssetDescriptor desc in descs ) {
						if ( dependencyCount.Get( desc, 0 ) == 0 ) {
							Unload( desc );
						}
					}
				}

				assets.Clear();
				assetDependencies.Clear();
				loadQueue.Clear();
				taskStack.Clear();
			}
		}

		#endregion

		//
		// Methods useful in debugging
		//
		#region Debug

		public List<string> DumpAllPaths<T>()
		{
			List<string> result = new List<string>();

			Type type = typeof( T );
			foreach ( AssetDescriptor p in assets.Keys ) {
				if ( p.Type == type )
					result.Add( p.Path );
			}

			return result;
		}

		public List<KeyValuePair<string, object>> DumpAllPathsAssets<T>()
		{
			List<KeyValuePair<string, object>> result = new List<KeyValuePair<string, object>>();

			Type type = typeof( T );
			foreach ( AssetDescriptor p in assets.Keys ) {
				if ( p.Type == type )
					result.Add( new KeyValuePair<string, object>( p.Path, assets[p].Asset ) );
			}

			return result;
		}

		#endregion

		//
		// Things used internally by the AssetManager, that are not intended to be accessed
		// by anyone outside.
		// 
		#region Internals

		//
		// Methods pertaining to how the AssetManager handles loading tasks internally.
		// 
		#region Load Task Handling

		private void InjectDependencies( AssetDescriptor parent, List<AssetDescriptor> dependedDescs )
		{
			HashSet<AssetDescriptor> injected = new HashSet<AssetDescriptor>();
			foreach ( AssetDescriptor desc in dependedDescs ) {
				// Ignore subsequent dependencies if there are duplicates.
				if ( injected.Contains( desc ) )
					continue;
				injected.Add( desc );
				InjectDependency( parent, desc );
			}
			injected.Clear();
		}

		private void InjectDependency( AssetDescriptor parent, AssetDescriptor dependedDesc )
		{
			using ( Key.Lock( assets, assetDependencies ) ) {
				// Add the asset as a dependency of the parent asset
				List<AssetDescriptor> dependencies = assetDependencies.Get( parent, null );
				if ( dependencies == null ) {
					dependencies = new List<AssetDescriptor>();
					assetDependencies.Add( parent, dependencies );
				}
				dependencies.Add( dependedDesc );

				// If the asset is already loaded, increase its ref count
				if ( IsLoaded( dependedDesc ) ) {
					IReferencedObject ro = assets[dependedDesc];
					++ro.RefCount;
					IncrementRefCountedDependencies( dependedDesc );
				}
				else {
					// Else, add a new task to load it
					AddTask( dependedDesc );
				}
			}
		}

		private void IncrementRefCountedDependencies( AssetDescriptor parent )
		{
			if ( !assetDependencies.ContainsKey( parent ) )
				return;

			foreach ( AssetDescriptor dependency in assetDependencies[parent] ) {
				IReferencedObject ro = assets[dependency];
				++ro.RefCount;
				IncrementRefCountedDependencies( dependency );
			}
		}

		private void HandleTaskError( Exception ex )
		{
			if ( taskStack.Count == 0 )
				throw ex;

			// Remove the faulty task from the stack
			AssetLoadingTask task = taskStack.Pop();

			// Remove the task's dependencies
			if ( task.depsLoaded && task.dependencies != null ) {
				foreach ( AssetDescriptor desc in task.dependencies ) {
					Unload( desc );
				}
			}

			taskStack.Clear();

			throw ex;
		}

		private void NextTask()
		{
			AssetDescriptor descriptor = loadQueue.Dequeue();

			if ( IsLoaded( descriptor ) ) {
				IReferencedObject asset = assets[descriptor];
				++asset.RefCount;
				IncrementRefCountedDependencies( descriptor );
				if ( descriptor.Params != null ) {
					descriptor.Params.FireOnLoaded( this, descriptor );
				}
				++loaded;
			}
			else {
				AddTask( descriptor );
			}
		}

		private void AddTask( AssetDescriptor descriptor )
		{
			AssetLoader loader = GetLoader( descriptor.Type );
			if ( loader == null )
				throw new Exception( "No loader for type: " + descriptor.Type );
			taskStack.Push( new AssetLoadingTask( this, descriptor, loader ) );
		}

		private bool UpdateTask()
		{
			AssetLoadingTask task = taskStack.Peek();

			if ( task.Update() ) {
				AddAsset( task.AssetDesc, task.Asset );

				if ( taskStack.Count == 1 )
					++loaded;
				taskStack.Pop();

				if ( task.Cancelled ) {
					Unload( task.AssetDesc.Type, task.AssetDesc.Path );
				}
				else {
					if ( task.AssetDesc.Params != null ) {
						task.AssetDesc.Params.FireOnLoaded( this, task.AssetDesc );
					}
				}

				return true;
			}
			else {
				return false;
			}
		}

		private void AddAsset<T>( AssetDescriptor desc, T asset )
		{
			if ( asset == null )
				throw new AssetNotLoadedException( desc );
			assets.Add( desc, new ReferencedObject<T>( asset ) );
		}

		#endregion

		//
		// Methods used to dispose loaded assets
		//
		#region Dispose Methods

		private void DisposeDependencies( AssetDescriptor desc )
		{
			if ( assetDependencies.ContainsKey( desc ) ) {
				foreach ( AssetDescriptor dependency in assetDependencies[desc] )
					DisposeDependencies( dependency );
			}

			if ( assets.ContainsKey( desc ) ) {
				IReferencedObject ro = assets[desc];
				Dispose( ro.Asset );
			}
		}

		private void Dispose( object o )
		{
			if ( o == null )
				return;
			else if ( o is IDisposable )
				( (IDisposable)o ).Dispose();
			else if ( o is UnityEngine.Object )
				UnityEngine.Object.Destroy( o as UnityEngine.Object );
		}

		#endregion

		private class AssetLoadingTask
		{
			private readonly AssetManager manager;
			private readonly AssetDescriptor descriptor;
			private readonly AssetLoader loader;

			private volatile bool asyncStarted = false;
			private volatile bool asyncDone = false;

			private volatile bool depsStarted = false;
			public volatile bool depsLoaded = false;
			public volatile List<AssetDescriptor> dependencies;

			private volatile bool cancelled = false;
			private volatile object asset = null;

			private volatile Exception exception = null;

			public AssetLoadingTask( AssetManager manager, AssetDescriptor descriptor, AssetLoader loader )
			{
				this.manager = manager;
				this.descriptor = descriptor;
				this.loader = loader;
			}

			public AssetDescriptor AssetDesc { get { return descriptor; } }
			public object Asset { get { return asset; } }
			public bool Cancelled { get { return cancelled; } }

			public bool Update()
			{
				if ( loader is SynchronousAssetLoader )
					HandleSyncLoader();
				else if ( loader is AsynchronousAssetLoader )
					HandleAsyncLoader();
				else
					throw new Exception( "Loader is neither synchronous nor asynchronous." );

				if ( exception != null ) {
					Debug.LogErrorFormat( "Task failed for {0}, \nreason: {1}", descriptor, exception.Message );
					throw exception;
				}

				return asset != null;
			}

			public void Cancel()
			{
				cancelled = true;
			}

			private void HandleSyncLoader()
			{
				SynchronousAssetLoader syncLoader = (SynchronousAssetLoader)loader;

				if ( !depsLoaded ) {
					depsLoaded = true;
					dependencies = syncLoader.GetDependencies( descriptor.Path, Resolve( loader, descriptor ), AssetDesc.Params );
					if ( dependencies == null ) {
						asset = syncLoader.LoadSync( manager, descriptor.Path, Resolve( loader, descriptor ), descriptor.Params );
						return;
					}
					manager.InjectDependencies( descriptor, dependencies );
				}
				else {
					asset = syncLoader.LoadSync( manager, descriptor.Path, Resolve( loader, descriptor ), descriptor.Params );
				}
			}

			private void HandleAsyncLoader()
			{
				AsynchronousAssetLoader asyncLoader = (AsynchronousAssetLoader)loader;

				if ( !depsLoaded ) {
					if ( !depsStarted ) {
						ThreadPool.QueueUserWorkItem( LoadDeps );
						depsStarted = true;
					}
				}
				else {
					if ( !asyncStarted ) {
						ThreadPool.QueueUserWorkItem( LoadAsync );
						asyncStarted = true;
					}
					else if ( asyncDone ) {
						asset = asyncLoader.LoadSync( manager, descriptor.Path, Resolve( loader, descriptor ), descriptor.Params );
					}
				}
			}

			/// <summary>
			/// Inject dependencies for loading. The tasks will be injected before the current one, and will be processed first.
			/// </summary>
			private void LoadDeps( System.Object threadContext )
			{
				try {
					AsynchronousAssetLoader asyncLoader = (AsynchronousAssetLoader)loader;

					dependencies = asyncLoader.GetDependencies( descriptor.Path, Resolve( loader, descriptor ), descriptor.Params );
					if ( dependencies != null ) {
						manager.InjectDependencies( descriptor, dependencies );
					}

					depsLoaded = true;
				}
				catch ( Exception ex ) {
					exception = ex;
				}
			}

			/// <summary>
			/// Loads parts of the asset asynchronously, if the loader is an AsynchronousLoader.
			/// </summary>
			private void LoadAsync( System.Object threadContext )
			{
				try {
					AsynchronousAssetLoader asyncLoader = (AsynchronousAssetLoader)loader;

					if ( depsLoaded ) {
						asyncLoader.LoadAsync( manager, descriptor.Path, Resolve( loader, descriptor ), descriptor.Params );

						asyncDone = true;
					}
				}
				catch ( Exception ex ) {
					exception = ex;
				}
			}

			private FileInfo Resolve( AssetLoader loader, AssetDescriptor assetDesc )
			{
				if ( assetDesc.File == null )
					assetDesc.File = loader.Resolve( assetDesc.Path );
				return assetDesc.File;
			}
		}

		#endregion
	}

	public class AssetNotLoadedException : Exception
	{
		public Type Type { get; private set; }
		public string Path { get; private set; }
		public IAssetLoaderParameters Params { get; private set; }

		public AssetNotLoadedException( Type type, string path, IAssetLoaderParameters param = null )
			: base( string.Format( "Asset not loaded: '{0}' ({1})", path, type ) )
		{
			this.Type = type;
			this.Path = path;
			this.Params = param;
		}

		public AssetNotLoadedException( AssetDescriptor desc )
			: this( desc.Type, desc.Path, desc.Params )
		{
		}
	}
}
