using DocInsight.Core.Models;

namespace DocInsight.Core.Interfaces;

public interface IVectorStore
{
    Task UpsertDocumentChunksAsync(
        IEnumerable<DocumentChunk> chunks,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SearchResult>> SearchAsync(
        float[] queryEmbedding,
        int maxResults = 3,
        CancellationToken cancellationToken = default);
}