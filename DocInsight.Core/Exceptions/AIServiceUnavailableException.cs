namespace DocInsight.Core.Exceptions
{
    public class AIServiceUnavailableException : Exception
    {
        public AIServiceUnavailableException(string message,
            Exception inner) : base(message, inner) { }
    }
}
