using System;
using System.Threading;

/// <summary>
/// A class used to simplify the acquisition of multiple locks in synchronized theading scenarios.
/// </summary>
/// 
/// Usage:
/// 
///		using ( Key.Lock( locker1, locker2 ) ) {
///			// operations on locker1 and locker2, which
///			// need to be synchronized between threads.
///		}
///	
internal class Key : IDisposable
{
	private object[] padlocks;

	private Key()
	{
	}

	/// <summary>
	/// Attempts to acquire locks on every object passed as argument.
	/// <para/>
	/// 
	/// If any of the arguments is null, the method throws an exception.
	/// If the method fails to acquire a lock on any of the arguments, it also
	/// throws an exception.
	/// <para/>
	/// 
	/// In case of failure, all locks acquired by this method are released
	/// before the exception is thrown.
	/// </summary>
	public static Key Lock( params object[] lockers )
	{
		if ( lockers == null )
			throw new ArgumentNullException( "lockers" );

		Key result = new Key();
		result.padlocks = new object[lockers.Length];

		try {
			for ( int i = 0; i < lockers.Length; ++i ) {
				object locker = lockers[i];

				if ( locker == null )
					throw new ArgumentNullException( "locker" );
				if ( !Monitor.TryEnter( locker ) )
					throw new LockAcquisitionFailedException( locker );

				result.padlocks[i] = locker;
			}
		}
		catch ( Exception ex ) {
			// Release any locks that we managed to acquire
			result.Dispose();
			throw ex;
		}

		return result;
	}

	public void Dispose()
	{
		if ( padlocks == null )
			throw new Exception( "Already disposed." );

		// When this falls out of scope (after a using {...} ), release the lock
		foreach ( object padlock in padlocks ) {
			if ( padlock == null )
				continue;
			Monitor.Exit( padlock );
		}
		padlocks = null;
	}
}

internal class LockAcquisitionFailedException : Exception
{
	private readonly object locker;

	public LockAcquisitionFailedException( object locker )
		: base( string.Format( "Could not acquire lock on {0}, becasue it's already taken by a different thread.", locker ) )
	{
		this.locker = locker;
	}

	/// <summary>
	/// Returns the object on which the failed lock acquisition attempt was made.
	/// </summary>
	public object GetFailedLockObject()
	{
		return locker;
	}
}
