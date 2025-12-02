using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NetworkInspector.Models;

namespace NetworkInspector.Storage
{
    public class InMemoryRequestStorage : IRequestStorage
    {
        private readonly ConcurrentQueue<RequestModel> _requests = new();
        private readonly int _maxRequests;

        public InMemoryRequestStorage(int maxRequests = 100)
        {
            _maxRequests = maxRequests;
        }

        public void Add(RequestModel request)
        {
            _requests.Enqueue(request);
            while (_requests.Count > _maxRequests)
            {
                _requests.TryDequeue(out _);
            }
        }

        public IEnumerable<RequestModel> GetAll()
        {
            return _requests.Reverse().ToList();
        }

        public RequestModel? Get(string id)
        {
            return _requests.FirstOrDefault(r => r.Id == id);
        }

        public void Clear()
        {
            _requests.Clear();
        }
    }
}
