using DocInsight.Core.Models;

namespace DocInsight.Core.Interfaces;

public interface IDocumentProcessor
{
    Task<IReadOnlyList<DocumentChunk>> ProcessDocumentAsync(
        string filePath,
        CancellationToken cancellationToken = default);
}