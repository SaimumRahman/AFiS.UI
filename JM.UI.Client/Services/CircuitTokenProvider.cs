using JM.UI.Entities.Services;

namespace JM.UI.Client.Services
{
    public class CircuitTokenProvider : ITokenProvider
    {
        private readonly CircuitHandlerService _circuitHandler;

        public CircuitTokenProvider(CircuitHandlerService circuitHandler)
        {
            _circuitHandler = circuitHandler;
        }

        public string GetToken()
        {
            var token = _circuitHandler.Token;

            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("⚠️ CircuitTokenProvider: Token is NULL or empty");
                Console.WriteLine($"Circuit ID: {_circuitHandler.CircuitId}");
            }
            else
            {
                Console.WriteLine($"✅ CircuitTokenProvider: Token retrieved: {token.Substring(0, Math.Min(20, token.Length))}...");
            }

            return token ?? string.Empty;
        }

        public void SetToken(string token)
        {
            _circuitHandler.Token = token;
            Console.WriteLine($"💾 CircuitTokenProvider: Token set: {token.Substring(0, Math.Min(20, token.Length))}...");
            Console.WriteLine($"Circuit ID: {_circuitHandler.CircuitId}");
        }

        public void ClearToken()
        {
            _circuitHandler.Token = string.Empty;
            Console.WriteLine("🗑️ CircuitTokenProvider: Token cleared");
        }
    }
}
