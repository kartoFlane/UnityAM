using System;
using System.Collections.Generic;

internal static class CollectionsExtension
{
	/// <summary>
	/// Allows to retrieve the value associated with the specified key from the dictionary.
	/// If there's no such key in the dictionary, the default value is returned instead.
	/// </summary>
	public static V Get<K, V>( this Dictionary<K, V> self, K key, V defaultValue )
	{
		if ( key == null )
			throw new ArgumentNullException( "key" );
		if ( self.ContainsKey( key ) )
			return self[key];
		return defaultValue;
	}

	public static V Get<K, V>( this Dictionary<K, V> self, K key )
	{
		if ( key == null )
			throw new ArgumentNullException( "key" );
		if ( self.ContainsKey( key ) )
			return self[key];
		throw new KeyNotFoundException( key.ToString() );
	}

	/// <summary>
	/// Allows to add a new key to the dictionary, even if the dictionary already
	/// contains this key.
	/// </summary>
	public static void Put<K, V>( this Dictionary<K, V> self, K key, V value )
	{
		if ( self.ContainsKey( key ) )
			self.Remove( key );
		self.Add( key, value );
	}

	/// <summary>
	/// Removes and returns the topmost element from the list (stack)
	/// </summary>
	public static T Pop<T>( this List<T> self )
	{
		T result = self[self.Count - 1];
		self.Remove( result );
		return result;
	}

	/// <summary>
	/// Returns the topmost element from the list (stack) without removing it.
	/// </summary>
	public static T Peek<T>( this List<T> self )
	{
		return self[self.Count - 1];
	}

	/// <summary>
	/// Delegate to List.Add(), to complete the 'stack' facade
	/// </summary>
	public static void Push<T>( this List<T> self, T element )
	{
		self.Add( element );
	}

	/// <summary>
	/// Removes and returns the first element from the list (queue)
	/// </summary>
	public static T Dequeue<T>( this List<T> self )
	{
		T result = self[0];
		self.Remove( result );
		return result;
	}

	/// <summary>
	/// Delegate to List.Add(), to complete the 'queue' facade
	/// </summary>
	public static void Enqueue<T>( this List<T> self, T element )
	{
		self.Add( element );
	}
}
