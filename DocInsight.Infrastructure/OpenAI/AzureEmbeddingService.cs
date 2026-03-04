using Azure.AI.OpenAI;
using DocInsight.Core.Interfaces;
using OpenAI.Embeddings;

namespace DocInsight.Infrastructure.OpenAI;

public class AzureEmbeddingService : IEmbeddingService
{
    private readonly EmbeddingClient _embeddingClient;

    public AzureEmbeddingService(EmbeddingClient embeddingClient)
    {
        _embeddingClient = embeddingClient;
    }

    public async Task<float[]> GenerateEmbeddingAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        var result = await _embeddingClient.GenerateEmbeddingAsync(
            text,
            cancellationToken: cancellationToken);

        return result.Value.ToFloats().ToArray();
    }
}