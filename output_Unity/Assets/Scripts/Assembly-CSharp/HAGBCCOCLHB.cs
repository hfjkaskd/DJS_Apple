using System.Collections;
using System.Collections.Generic;

internal class HAGBCCOCLHB : IDictionaryEnumerator, IEnumerator
{
	private IEnumerator<KeyValuePair<string, DPDFFKDDFOG>> list_enumerator;

	public object Current => null;

	public DictionaryEntry Entry => default(DictionaryEntry);

	public object Key => null;

	public object Value => null;

	public HAGBCCOCLHB(IEnumerator<KeyValuePair<string, DPDFFKDDFOG>> enumerator)
	{
	}

	public bool MoveNext()
	{
		return false;
	}

	public void Reset()
	{
	}
}
