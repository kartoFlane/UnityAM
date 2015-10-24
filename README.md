# UnityAM
A C# implementation of libGDX's AssetManager, for use with Unity. Based on https://github.com/libgdx/libgdx

---
## How to use

### Basics

The basic features of the `AssetManager` that you need to know to be able to use it at all

#### Initialization

First, add it to your project: copy the `AssetHandler` folder from this repository into your project's `Assets/Plugins` directory.

Next, in your main method - the entry point for the game - you need to instantiate the `AssetManager`.  
Resources loaded using the `AssetManager` are cached inside of that instance, so you'll need to keep a reference to it in a singleton or a context, depending on your preferred pattern.  
`AssetManager` itself is not a singleton, and you can instantiate multiple managers if you need to, though you probably won't need that most of the time.

```c#
using AssetHandler;
...
    
void YourEntryPointMethod()
{
    ...
    // This is a file resolver. It is used to resolve a path string into a FileInfo object.
    // You'll probably never need anything other than the default RelativeResolver.
    IFileInfoResolver resolver = new RelativeResolver();

    // Save the instance in your singleton holder or whatever. Something that allows your
    // classes that use the resources to obtain a reference to the manager.
    AssetManager manager = new AssetManager( resolver );

    // If you have custom resources, you'll need to register loaders for them here
    // (see Intermediate/Custom Loaders)
    ...
}
```

---

#### Loading

In order to load a resource, you need to schedule it for loading using the `Load()` method in the `AssetManager`.  
There are several overloads of this method available, but the most convenient is `Load<T>( string path )`, where T is the type of the asset you wish to load:

```c#
assetManager.Load<Texture2D>( "image.png" );
```

You can schedule any number of assets this way, and they'll be loaded in the order they've been scheduled (most of the time - see Intermediate/Custom Loaders for details).  
After you've scheduled your files for loading, you need to tell the `AssetManager` to get to work. You can do that by invoking `FinishLoading()`:

```c#
assetManager.Load<Texture2D>( "image.png" );
assetManager.FinishLoading();
```

However, that method blocks until all assets are loaded, which is not a good idea when you load a lot of resources - for example, in a loading screen. In that case, you may want to tell the `AssetManager` to keep loading for some amount of time, then return, so that your game remains responsive.  
The method you need to use in this scenario is `Update( int miliseconds )`. It also returns a boolean value, which signifies whether all assets have been loaded.  

Example:

```c#
public class LoadingScreen : MonoBehaviour
{
    private void Awake()
    {
        // Schedule assets for loading here
    }
    
    private void Update()
    {
        // Load for 16ms (1/60th of a second, ie. 1 frame), then return.
        // You can load for any length of time you want, but there's not much difference
        // between 16ms and 100ms, and keeping this value lower allows for smoother animation.
        if ( assetManager.Update( 16 ) ) {
            UnityEngine.Debug.Log( "Finished loading!" );
            // Continue to the next scene...
        }
    }
}
```

---

#### Asset usage

Now that you've loaded your assets, you can use the `AssetManager` to grab a reference to them. To do that, you have to use the `Get()` method. As with `Load()` previously, there's a number of overloads, but the most convenient is `Get<T>( string path )`, where T is the type of the asset:

```c#
Texture2D texture = assetManager.Get<Texture2D>( "image.png" );
```

---

### Intermediate

Some of the more advanced features of the `AssetManager` that you'll likely need if you're making a proper game.

#### Custom Loaders

TODO

---

#### Manual asset management

TODO
