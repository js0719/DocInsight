using Azure;
using Azure.AI.OpenAI;
using Azure.Search.Documents.Indexes;
using DocInsight.Api.Endpoints;
using DocInsight.Core.Interfaces;
using DocInsight.Infrastructure.FileProcessing;
using DocInsight.Infrastructure.OpenAI;
using DocInsight.Infrastructure.Search;

var builder = WebApplication.CreateBuilder(args);

// ---- Configuration ----
var openAIEndpoint = builder.Configuration["AzureOpenAI:Endpoint"]
    ?? throw new InvalidOperationException(
        "AzureOpenAI:Endpoint is not configured");

var openAIKey = builder.Configuration["AzureOpenAI:ApiKey"]
    ?? throw new InvalidOperationException(
        "AzureOpenAI:ApiKey is not configured");

var embeddingDeployment = builder.Configuration
    ["AzureOpenAI:EmbeddingDeployment"]
    ?? throw new InvalidOperationException(
        "AzureOpenAI:EmbeddingDeployment is not configured");

var chatDeployment = builder.Configuration
    ["AzureOpenAI:ChatDeployment"]
    ?? throw new InvalidOperationException(
        "AzureOpenAI:ChatDeployment is not configured");

var searchEndpoint = builder.Configuration["AzureSearch:Endpoint"]
    ?? throw new InvalidOperationException(
        "AzureSearch:Endpoint is not configured");

var searchKey = builder.Configuration["AzureSearch:ApiKey"]
    ?? throw new InvalidOperationException(
        "AzureSearch:ApiKey is not configured");

// ---- Azure OpenAI Clients ----
var openAIClient = new AzureOpenAIClient(
    new Uri(openAIEndpoint),
    new AzureKeyCredential(openAIKey));

builder.Services.AddSingleton(
    openAIClient.GetEmbeddingClient(embeddingDeployment));

builder.Services.AddSingleton(
    openAIClient.GetChatClient(chatDeployment));

// ---- Azure AI Search Client ----
builder.Services.AddSingleton(new SearchIndexClient(
    new Uri(searchEndpoint),
    new AzureKeyCredential(searchKey)));

// ---- Register Services ----
builder.Services.AddSingleton<IEmbeddingService,
    AzureEmbeddingService>();
builder.Services.AddSingleton<IVectorStore,
    AzureVectorStore>();
builder.Services.AddSingleton<IDocumentProcessor,
    DocumentProcessor>();
builder.Services.AddSingleton<IQAService, QAService>();

// ---- Middleware ----
builder.Services.AddAntiforgery();

var app = builder.Build();

app.UseAntiforgery();

// ---- Endpoints ----
app.MapDocumentEndpoints();

app.Run();