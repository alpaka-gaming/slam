using System.Collections.Generic;

namespace Core.Models
{
    public class Library
    {
        public string Path { get; set; }
        public string Label { get; set; }
        public long ContentId { get; set; }
        public long TotalSize { get; set; }
        public Dictionary<long, long> Apps { get; set; }

        public override string ToString()
        {
            return $"{Label} ({Path})";
        }
    }
}
