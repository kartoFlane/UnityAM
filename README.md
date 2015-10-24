# UnityAM

### What is it?

UnityAM (Unity Asset Manager) is a trimmed C# implementation/port of [libGDX](https://github.com/libgdx/libgdx)'s [AssetManager](https://github.com/libgdx/libgdx/blob/master/gdx/src/com/badlogic/gdx/assets/AssetManager.java), intended - as the name implies - for use with Unity.

### Okay, but what does it do?

UnityAM provides a flexible framework for loading of external assets at runtime. Unity *kinda* allows you to do that through `Resources.Load()`, but this method only works for files that you've placed inside `Assets/Resources` folder in your project.

Of course, normally it's not a problem, your files are in the `Assets` folder anyway! After all, this allows Unity to optimize them, and handle their loading for you.

But it does become an issue if you want to **encourage modding of your game**, because Unity converts and compiles your resources into `.assets` files, requiring third-party tools to extract.

As you can imagine, this may be a bit of a deterrent for potential modders, or people who are just curious about the game's insides (or content thieves, but there's nothing you can do about that if you want modding...)

### How do I use this?

Check out the [Basics](https://github.com/kartoFlane/UnityAM/wiki/Basics) wiki article!

### License

As a port of a part of libGDX, UnityAM uses the same license - [Apache 2 License](http://www.apache.org/licenses/LICENSE-2.0.html) - meaning you can use it free of charge, without strings attached in commercial and non-commercial projects.
