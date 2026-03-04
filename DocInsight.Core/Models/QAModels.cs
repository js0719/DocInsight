using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocInsight.Core.Models
{
    public record QuestionRequest
    {
        public required string Question { get; init; }
        public int MaxResults { get; init; } = 3;
    }

    public record QuestionResponse
    {
        public required string Answer { get; init; }
        public required string Question { get; init; }
        public required IReadOnlyList<SearchResult> Sources { get; init; }
    }
}
