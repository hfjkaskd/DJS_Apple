using System.Collections;
using System.Collections.Generic;

public class JFDHADOAJPO<T> : IReadOnlyList<T>, IEnumerable<T>, IEnumerable, IReadOnlyCollection<T>
{
	private List<T> items;

	public T this[int index] => default(T);

	public T LastItem => default(T);

	public int Count => 0;

	public bool IsNull => false;

	public void SetList(List<T> InList)
	{
	}

	public bool Contains(T InItem)
	{
		return false;
	}

	public IEnumerator<T> GetEnumerator()
	{
		return null;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return null;
	}
}
