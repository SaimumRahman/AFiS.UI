using Microsoft.AspNetCore.Components.Server.Circuits;

namespace JM.UI.Client.Services
{
    public class CircuitHandlerService : CircuitHandler
    {
        public string CircuitId { get; private set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public int CompanyId { get; set; }

        public override Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            CircuitId = circuit.Id;
            Console.WriteLine($"Circuit connected: {CircuitId}");
            return Task.CompletedTask;
        }

        public override Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Circuit disconnected: {CircuitId}");
            return Task.CompletedTask;
        }

        public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            CircuitId = circuit.Id;
            Console.WriteLine($"Circuit opened: {CircuitId}");
            return Task.CompletedTask;
        }

        public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Circuit closed: {CircuitId}");
            Token = string.Empty;
            return Task.CompletedTask;
        }
    }
}
