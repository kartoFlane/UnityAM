using System;
using System.Collections.Generic;

internal static class DictionaryExtension
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
}
