using DocInsight.Core.Interfaces;
using DocInsight.Core.Models;

namespace DocInsight.Api.Endpoints;

public static class DocumentEndpoints
{
    public static void MapDocumentEndpoints(
        this WebApplication app)
    {
        app.MapPost("/documents/ingest", IngestDocument)
            .DisableAntiforgery()
            .WithName("IngestDocument")
            .WithDescription("Upload and process a document for QA");

        app.MapPost("/documents/query", QueryDocument)
            .WithName("QueryDocument")
            .WithDescription("Ask a question about ingested documents");

        app.MapGet("/health", HealthCheck)
            .WithName("HealthCheck")
            .WithDescription("Health check endpoint");
    }

    private static async Task<IResult> IngestDocument(
        IDocumentProcessor processor,
        IEmbeddingService embeddingService,
        IVectorStore vectorStore,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file.Length == 0)
            return Results.BadRequest(
                new { Error = "File is empty" });

        if (!file.FileName.EndsWith(".txt",
            StringComparison.OrdinalIgnoreCase))
            return Results.BadRequest(
                new { Error = "Only .txt files are supported" });

        var tempPath = Path.GetTempFileName();

        try
        {
            await using var stream = File.OpenWrite(tempPath);
            await file.CopyToAsync(stream, cancellationToken);
            stream.Close();

            var chunks = await processor.ProcessDocumentAsync(
                tempPath, cancellationToken);

            var embeddedChunks = new List<DocumentChunk>();

            foreach (var chunk in chunks)
            {
                var embedding = await embeddingService
                    .GenerateEmbeddingAsync(
                        chunk.Content,
                        cancellationToken);

                embeddedChunks.Add(
                    chunk with { Embedding = embedding });
            }

            await vectorStore.UpsertDocumentChunksAsync(
                embeddedChunks, cancellationToken);

            return Results.Ok(new
            {
                Message = "Document ingested successfully",
                ChunkCount = embeddedChunks.Count,
                FileName = file.FileName
            });
        }
        catch (OperationCanceledException)
        {
            return Results.StatusCode(499);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: 500,
                title: "Document ingestion failed");
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    private static async Task<IResult> QueryDocument(
        IQAService qaService,
        QuestionRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
            return Results.BadRequest(
                new { Error = "Question cannot be empty" });

        try
        {
            var response = await qaService.AnswerAsync(
                request, cancellationToken);

            return Results.Ok(response);
        }
        catch (OperationCanceledException)
        {
            return Results.StatusCode(499);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: 500,
                title: "Query failed");
        }
    }

    private static IResult HealthCheck()
    {
        return Results.Ok(new
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Service = "DocInsight API"
        });
    }
}