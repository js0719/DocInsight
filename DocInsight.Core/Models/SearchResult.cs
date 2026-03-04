using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocInsight.Core.Models
{
    public record SearchResult
    {
        public required string Content { get; init; }
        public required string SourceDocument { get; init; }
        public required double Score { get; init; }
        public required int ChunkIndex { get; init; }
    }
}
