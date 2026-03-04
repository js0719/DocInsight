using DocInsight.Core.Interfaces;
using DocInsight.Core.Models;

namespace DocInsight.Infrastructure.FileProcessing;

public class DocumentProcessor : IDocumentProcessor
{
    private const int ChunkSize = 500;
    private const int ChunkOverlap = 50;

    public async Task<IReadOnlyList<DocumentChunk>> ProcessDocumentAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        var text = await File.ReadAllTextAsync(filePath, cancellationToken);
        var words = text.Split(' ',
            StringSplitOptions.RemoveEmptyEntries);
        var chunks = new List<DocumentChunk>();
        var chunkIndex = 0;
        var position = 0;

        while (position < words.Length)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var chunkWords = words
                .Skip(position)
                .Take(ChunkSize)
                .ToArray();

            var content = string.Join(' ', chunkWords);

            chunks.Add(new DocumentChunk
            {
                Id = Guid.NewGuid().ToString(),
                Content = content,
                SourceDocument = Path.GetFileName(filePath),
                ChunkIndex = chunkIndex
            });

            position += ChunkSize - ChunkOverlap;
            chunkIndex++;
        }

        return chunks;
    }
}