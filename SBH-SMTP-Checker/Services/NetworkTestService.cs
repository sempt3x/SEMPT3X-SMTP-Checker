using System.Net;
using System.Net.Sockets;

namespace SBH_SMTP_Checker.Services;

public sealed class NetworkTestService(AppLogger logger)
{
    private static readonly TimeSpan Port465Timeout = TimeSpan.FromSeconds(5);

    public async Task<IPAddress[]> ResolveAsync(string host, CancellationToken cancellationToken)
    {
        logger.Info($"Resolving server '{host}'...");
        var addresses = await Dns.GetHostAddressesAsync(host, cancellationToken);
        logger.Success($"Server resolved to {string.Join(", ", addresses.Select(x => x.ToString()))}.");
        return addresses;
    }

    public async Task TestPortAsync(string host, int port, CancellationToken cancellationToken)
    {
        logger.Info($"Testing TCP port {port}...");
        using var client = new TcpClient();

        if (port != 465)
        {
            await client.ConnectAsync(host, port, cancellationToken);
        }
        else
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(Port465Timeout);
            try
            {
                await client.ConnectAsync(host, port, timeout.Token);
            }
            catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                throw new TimeoutException(
                    $"TCP connection to {host}:465 timed out after {Port465Timeout.TotalSeconds:0} seconds.", ex);
            }
        }

        logger.Success($"TCP port {port} is reachable.");
    }
}
