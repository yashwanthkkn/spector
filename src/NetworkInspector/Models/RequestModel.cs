using System;
using System.Collections.Generic;

namespace NetworkInspector.Models
{
    public class RequestModel
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public required string Method { get; set; }
        public required string Url { get; set; }
        public int StatusCode { get; set; }
        public long DurationMs { get; set; }
        public DateTime Timestamp { get; set; }
        public string TraceId { get; set; } = string.Empty;
        public Dictionary<string, string> RequestHeaders { get; set; } = new();
        public Dictionary<string, string> ResponseHeaders { get; set; } = new();
        public string? RequestBody { get; set; }
        public string? ResponseBody { get; set; }
        
        // For future use with DB queries and outgoing requests
        public List<object> ChildEvents { get; set; } = new();
    }
}
