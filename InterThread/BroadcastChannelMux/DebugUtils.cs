using System.Collections.Generic;
using System.Diagnostics;

namespace BroadcastChannelMux; 

internal interface IDebugEnumerable<T>
{
    IEnumerator<T> GetEnumerator();
}

internal sealed class DebugEnumeratorDebugView<T>
{
    public DebugEnumeratorDebugView(IDebugEnumerable<T> enumerable)
    {
        var list = new List<T>();
        foreach (T item in enumerable)
        {
            list.Add(item);
        }
        Items = list.ToArray();
    }

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public T[] Items { get; }
}