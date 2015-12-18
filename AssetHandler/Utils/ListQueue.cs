using System.Collections.Generic;

/// <summary>
/// List-based implementation of a Queue.
/// </summary>
internal class ListQueue<T> : List<T>
{
	public T Peek()
	{
		return this[0];
	}

	public T Dequeue()
	{
		T result = Peek();
		Remove( result );
		return result;
	}

	public void Enqueue( T element )
	{
		Add( element );
	}
}
