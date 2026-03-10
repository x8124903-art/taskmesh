namespace MsGateway.Domain.Services
{
    public sealed class GatewayService : IGatewayService
    {
        public string GetStatus() => "ok";
    }
}