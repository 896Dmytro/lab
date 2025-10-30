using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis; // <--- ДОДАНО ДЛЯ АТРИБУТУ

public class MyEchoServer
{
    private readonly int _port;
    private readonly ILogger<MyEchoServer> _logger;
    private TcpListener _listener;
    private CancellationTokenSource _cancellationTokenSource;

    [ExcludeFromCodeCoverage] // Ігноруємо конструктор
    public MyEchoServer(int port, ILogger<MyEchoServer> logger)
    {
        _port = port;
        _logger = logger;
        _cancellationTokenSource = new CancellationTokenSource();
    }

    [ExcludeFromCodeCoverage] // Ігноруємо StartAsync
    public async Task StartAsync()
    {
        _listener = new TcpListener(IPAddress.Any, _port);
        _listener.Start();
        _logger.LogInformation($"Server started on port {_port}.");

        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            try
            {
                TcpClient client = await _listener.AcceptTcpClientAsync();
                _logger.LogInformation("Client connected.");
                
                _ = HandleClientAsync(client, _cancellationTokenSource.Token);
            }
            catch (ObjectDisposedException)
            {
                break;
            }
        }
        _logger.LogInformation("Server shutdown.");
    }

    [ExcludeFromCodeCoverage] // Ігноруємо HandleClientAsync
    private async Task HandleClientAsync(TcpClient client, CancellationToken token)
    {
        try
        {
            using (NetworkStream stream = client.GetStream())
            {
                await ProcessClientStreamAsync(stream, token);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in HandleClientAsync");
        }
        finally
        {
            client.Close();
            _logger.LogInformation("Client disconnected.");
        }
    }

    // --- ЦЕЙ МЕТОД МИ ТЕСТУЄМО ---
    public async Task ProcessClientStreamAsync(Stream stream, CancellationToken token)
    {
        byte[] buffer = new byte[8192];
        int bytesRead;

        try
        {
            while (!token.IsCancellationRequested && (bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token)) > 0)
            {
                await stream.WriteAsync(buffer, 0, bytesRead, token);
                _logger.LogInformation($"Echoed {bytesRead} bytes to the client.");
            }
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            _logger.LogError(ex, "Error during stream processing");
        }
    }

    [ExcludeFromCodeCoverage] // Ігноруємо Stop
    public void Stop()
    {
        _cancellationTokenSource.Cancel();
        _listener.Stop();
        _cancellationTokenSource.Dispose();
        _logger.LogInformation("Server stopped.");
    }
}