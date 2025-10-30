using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class UdpClientWrapper : IUdpClient
{
    private readonly IPEndPoint _localEndPoint;
    private CancellationTokenSource? _cts;
    private UdpClient? _udpClient;

    public event EventHandler<byte[]>? MessageReceived;

    public UdpClientWrapper(int port)
    {
        _localEndPoint = new IPEndPoint(IPAddress.Any, port);
    }

    public async Task StartListeningAsync()
    {
        _cts = new CancellationTokenSource();
        Console.WriteLine("Start listening for UDP messages...");

        try
        {
            _udpClient = new UdpClient(_localEndPoint);
            while (!_cts.Token.IsCancellationRequested)
            {
                UdpReceiveResult result = await _udpClient.ReceiveAsync(_cts.Token);
                MessageReceived?.Invoke(this, result.Buffer);

                Console.WriteLine($"Received from {result.RemoteEndPoint}");
            }
        }
        // --- ВИПРАВЛЕННЯ "CODE SMELL" ---
        // Sonar не любить порожні 'catch'. Додаємо логування.
        catch (OperationCanceledException)
        {
            Console.WriteLine("Listening was canceled successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error receiving message: {ex.Message}");
        }
    }

    // --- "ГОЛОВНИЙ" МЕТОД З ЛОГІКОЮ ---
    public void StopListening()
    {
        try
        {
            _cts?.Cancel();
            _udpClient?.Close();
            Console.WriteLine("Stopped listening for UDP messages.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while stopping: {ex.Message}");
        }
    }

    // --- ВИПРАВЛЕННЯ ДУБЛЮВАННЯ ---
    // Цей метод тепер просто викликає інший, щоб уникнути дублювання
    public void Exit()
    {
        StopListening();
    }

    public override int GetHashCode()
    {
        var payload = $"{nameof(UdpClientWrapper)}|{_localEndPoint.Address}|{_localEndPoint.Port}";

        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(payload));

        return BitConverter.ToInt32(hash, 0);
    }
}
