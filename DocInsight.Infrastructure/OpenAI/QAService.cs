using DocInsight.Core.Interfaces;
using DocInsight.Core.Models;
using OpenAI.Chat;

namespace DocInsight.Infrastructure.OpenAI;

public class QAService : IQAService
{
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorStore _vectorStore;
    private readonly ChatClient _chatClient;

    public QAService(
        IEmbeddingService embeddingService,
        IVectorStore vectorStore,
        ChatClient chatClient)
    {
        _embeddingService = embeddingService;
        _vectorStore = vectorStore;
        _chatClient = chatClient;
    }

    public async Task<QuestionResponse> AnswerAsync(
        QuestionRequest request,
        CancellationToken cancellationToken = default)
    {
        // Step 1: Convert question to vector
        var questionEmbedding = await _embeddingService
            .GenerateEmbeddingAsync(
                request.Question,
                cancellationToken);

        // Step 2: Find relevant chunks
        var relevantChunks = await _vectorStore.SearchAsync(
            questionEmbedding,
            request.MaxResults,
            cancellationToken);

        // Step 3: Build prompt
        var prompt = BuildPrompt(
            request.Question,
            relevantChunks);

        // Step 4: Generate answer
        var answer = await GenerateAnswerAsync(
            prompt,
            cancellationToken);

        // Step 5: Return response with sources
        return new QuestionResponse
        {
            Question = request.Question,
            Answer = answer,
            Sources = relevantChunks
        };
    }

    private static string BuildPrompt(
        string question,
        IReadOnlyList<SearchResult> chunks)
    {
        var context = string.Join(
            "\n\n",
            chunks.Select((chunk, index) =>
                $"[Source {index + 1}: {chunk.SourceDocument}]\n{chunk.Content}"));

        return $"""
            You are a helpful assistant that answers questions 
            based strictly on the provided context.
            
            If the answer cannot be found in the context, 
            say "I don't have enough information to answer 
            this question based on the provided documents."
            
            Do not make up information or use knowledge 
            outside the provided context.
            
            Context:
            {context}
            
            Question: {question}
            
            Answer:
            """;
    }

    private async Task<string> GenerateAnswerAsync(
        string prompt,
        CancellationToken cancellationToken)
    {
        var messages = new List<ChatMessage>
        {
            new UserChatMessage(prompt)
        };

        var response = await _chatClient.CompleteChatAsync(
            messages,
            cancellationToken: cancellationToken);

        return response.Value.Content[0].Text;
    }
}