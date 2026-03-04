using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocInsight.Core.Models
{
    public record DocumentChunk
    {
        public required string Id { get; init; }
        public required string Content { get; init; }
        public required string SourceDocument { get; init; }
        public required int ChunkIndex { get; init; }
        public float[]? Embedding { get; init; }
    }
}
