using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class ChanceValuePair<T>
{
    [Min(0.0f)] public float chance;
    public T value;
}

[Serializable]
public class ChanceValueElector<T> : IList<ChanceValuePair<T>>
{
    public int Count => _chanceValuePairs.Count;
    public bool IsReadOnly => false;
    public ChanceValuePair<T> this[int index] 
    {
        get => _chanceValuePairs[index];
        set => _chanceValuePairs[index] = value;
    }

    [SerializeField] private List<ChanceValuePair<T>> _chanceValuePairs = new();

    private readonly List<float> chanceSumBefore = new();

    public ChanceValueElector()
    {
        RecalculateArray();
    }

    private void RecalculateArray()
    {
        chanceSumBefore.Clear();

        float sum = 0.0f;
        for (int i = 0; i < Count; i++)
        {
            chanceSumBefore.Add(sum);
            sum += _chanceValuePairs[i].chance;
        }
        chanceSumBefore.Add(sum);
    }

    public T GetNextValueByChance()
    {
        if (chanceSumBefore.Count != Count + 1)
            RecalculateArray();

        float luck = Random.Range(0.0f, chanceSumBefore[Count]);
        for (int i = 0; i < Count; i++)
        {
            if (luck >= chanceSumBefore[i] && luck <= chanceSumBefore[i + 1])
                return _chanceValuePairs[i].value;
        }
        return default;
    }

    public int IndexOf(ChanceValuePair<T> item) => _chanceValuePairs.IndexOf(item);

    public void Insert(int index, ChanceValuePair<T> item)
    {
        _chanceValuePairs.Insert(index, item);
        RecalculateArray();
    }

    public void RemoveAt(int index)
    {
        _chanceValuePairs.RemoveAt(index);
        RecalculateArray();
    }

    public void Add(ChanceValuePair<T> item)
    {
        _chanceValuePairs.Add(item);
        chanceSumBefore.Add(chanceSumBefore[Count - 1] + item.chance);
    }

    public void Clear()
    {
        _chanceValuePairs.Clear();
        chanceSumBefore.Clear();
        chanceSumBefore.Add(0.0f);
    }

    public bool Contains(ChanceValuePair<T> item) => _chanceValuePairs.Contains(item);

    public void CopyTo(ChanceValuePair<T>[] array, int arrayIndex)
    {
        _chanceValuePairs.CopyTo(array, arrayIndex);
    }

    public bool Remove(ChanceValuePair<T> item)
    {
        if (_chanceValuePairs.Remove(item))
        {
            RecalculateArray();
            return true;
        }
        return false;
    }

    public IEnumerator<ChanceValuePair<T>> GetEnumerator() => _chanceValuePairs.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _chanceValuePairs.GetEnumerator();
}
