using System;
using System.Collections.Generic;

public sealed class IAMOPBKAHDO<T> where T : class
{
	private readonly int capacity;

	private readonly Stack<T> pool;

	private readonly Func<T> createFunc;

	private readonly Action<T> onGetCallback;

	private readonly Action<T> onReleaseCallback;

	public int CountAll { get; private set; }

	public int CountActive => 0;

	public int CountInactive => 0;

	public IAMOPBKAHDO(int InCapacity, Func<T> InCreatFunc, Action<T> InOnGetCallback = null, Action<T> InOnReleaseCallback = null)
	{
	}

	public T Get()
	{
		return null;
	}

	public void Release(T InT)
	{
	}

	public void Clear()
	{
	}
}
