using Microsoft.Extensions.Logging; // Потрібно додати
using System;
using System.IO; // Потрібно для Stream
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace EchoServer
{
    public class EchoServer
    {
        private readonly int _port;
        private readonly ILogger<EchoServer> _logger; // 1. Використовуємо ILogger
        private TcpListener _listener;
        private CancellationTokenSource _cancellationTokenSource;

        public EchoServer(int port, ILogger<EchoServer> logger) // 2. Отримуємо логгер
        {
            _port = port;
            _logger = logger;
            _cancellationTokenSource = new CancellationTokenSource();
        }

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
                    
                    // Запускаємо обробку, але не чекаємо (fire and forget)
                    _ = HandleClientAsync(client, _cancellationTokenSource.Token);
                }
                catch (ObjectDisposedException)
                {
                    break; // Listener зупинено
                }
            }
            _logger.LogInformation("Server shutdown.");
        }

        // 3. Це приватний МЕТОД ІНФРАСТРУКТУРИ (не тестується)
        private async Task HandleClientAsync(TcpClient client, CancellationToken token)
        {
            try
            {
                using (NetworkStream stream = client.GetStream())
                {
                    // 4. Викликаємо нашу тестовану логіку
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

        // 5. Це ПУБЛІЧНИЙ МЕТОД ЛОГІКИ (тестується)
        // Він працює з абстрактним 'Stream', а не 'NetworkStream'
        public async Task ProcessClientStreamAsync(Stream stream, CancellationToken token)
        {
            byte[] buffer = new byte[8192];
            int bytesRead;

            try
            {
                // Наша "бізнес-логіка" - читати і одразу писати
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
            // 'finally' і 'client.Close()' тут не потрібні,
            // за це відповідає 'HandleClientAsync'
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
            _listener.Stop();
            _cancellationTokenSource.Dispose();
            _logger.LogInformation("Server stopped.");
        }
    }
}
