using DocInsight.Core.Models;

namespace DocInsight.Core.Interfaces;

public interface IQAService
{
    Task<QuestionResponse> AnswerAsync(
        QuestionRequest request,
        CancellationToken cancellationToken = default);
}