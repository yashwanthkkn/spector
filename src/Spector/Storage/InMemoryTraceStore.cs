using System.Collections.Concurrent;
using Spector.Models;

namespace Spector.Storage;

public class InMemoryTraceStore
{
    private readonly ConcurrentQueue<TraceDto> _q = new();
    private readonly int _max;

    public InMemoryTraceStore(int max = 5000) => _max = max;

    public void Add(TraceDto dto)
    {
        _q.Enqueue(dto);
        while (_q.Count > _max && _q.TryDequeue(out _)) { }
    }

    public List<TraceDto> GetAll() => _q.ToList();
}