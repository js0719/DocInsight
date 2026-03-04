using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using DocInsight.Core.Interfaces;
using DocInsight.Core.Models;

namespace DocInsight.Infrastructure.Search;

public class AzureVectorStore : IVectorStore
{
    private readonly SearchIndexClient _indexClient;
    private readonly SearchClient _searchClient;
    private const string IndexName = "docinsight-index";
    private const int VectorDimensions = 1536;

    public AzureVectorStore(
        SearchIndexClient indexClient)
    {
        _indexClient = indexClient;
        _searchClient = indexClient.GetSearchClient(IndexName);
    }

    public async Task UpsertDocumentChunksAsync(
        IEnumerable<DocumentChunk> chunks,
        CancellationToken cancellationToken = default)
    {
        await EnsureIndexExistsAsync(cancellationToken);

        var documents = chunks.Select(chunk => new SearchDocument
        {
            ["id"] = chunk.Id,
            ["content"] = chunk.Content,
            ["sourceDocument"] = chunk.SourceDocument,
            ["chunkIndex"] = chunk.ChunkIndex,
            ["embedding"] = chunk.Embedding
        });

        await _searchClient.MergeOrUploadDocumentsAsync(
            documents,
            cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<SearchResult>> SearchAsync(
        float[] queryEmbedding,
        int maxResults = 3,
        CancellationToken cancellationToken = default)
    {
        var vectorQuery = new VectorizedQuery(queryEmbedding)
        {
            KNearestNeighborsCount = maxResults,
            Fields = { "embedding" }
        };

        var searchOptions = new SearchOptions
        {
            VectorSearch = new VectorSearchOptions
            {
                Queries = { vectorQuery }
            },
            Select = { "content", "sourceDocument", "chunkIndex" },
            Size = maxResults
        };

        var response = await _searchClient.SearchAsync<SearchDocument>(
            searchText: null,
            searchOptions,
            cancellationToken);

        var results = new List<SearchResult>();

        await foreach (var result in response.Value.GetResultsAsync())
        {
            results.Add(new SearchResult
            {
                Content = result.Document["content"].ToString()!,
                SourceDocument = result.Document["sourceDocument"].ToString()!,
                ChunkIndex = Convert.ToInt32(
                    result.Document["chunkIndex"]),
                Score = result.Score ?? 0
            });
        }

        return results;
    }

    private async Task EnsureIndexExistsAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            await _indexClient.GetIndexAsync(
                IndexName, cancellationToken);
        }
        catch (RequestFailedException ex)
            when (ex.Status == 404)
        {
            await CreateIndexAsync(cancellationToken);
        }
    }

    private async Task CreateIndexAsync(
        CancellationToken cancellationToken)
    {
        var fields = new List<SearchField>
        {
            new SimpleField("id",
                SearchFieldDataType.String)
            {
                IsKey = true,
                IsFilterable = true
            },
            new SearchableField("content"),
            new SimpleField("sourceDocument",
                SearchFieldDataType.String)
            {
                IsFilterable = true
            },
            new SimpleField("chunkIndex",
                SearchFieldDataType.Int32),
            new VectorSearchField("embedding",
                VectorDimensions,
                "my-vector-profile")
        };

        var vectorSearch = new VectorSearch();
        vectorSearch.Algorithms.Add(
            new HnswAlgorithmConfiguration("my-hnsw"));
        vectorSearch.Profiles.Add(
            new VectorSearchProfile(
                "my-vector-profile", "my-hnsw"));

        var index = new SearchIndex(IndexName)
        {
            Fields = fields,
            VectorSearch = vectorSearch
        };

        await _indexClient.CreateIndexAsync(
            index, cancellationToken);
    }
}