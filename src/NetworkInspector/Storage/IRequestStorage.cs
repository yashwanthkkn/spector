using System.Collections.Generic;
using NetworkInspector.Models;

namespace NetworkInspector.Storage
{
    public interface IRequestStorage
    {
        void Add(RequestModel request);
        IEnumerable<RequestModel> GetAll();
        RequestModel? Get(string id);
        void Clear();
    }
}
