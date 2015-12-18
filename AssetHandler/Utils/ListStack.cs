using System.Collections.Generic;

/// <summary>
/// List-based implementation of a Stack.
/// </summary>
internal class ListStack<T> : List<T>
{
	public T Peek()
	{
		return this[Count - 1];
	}

	public T Pop()
	{
		T result = Peek();
		Remove( result );
		return result;
	}

	public void Push( T element )
	{
		Add( element );
	}
}
