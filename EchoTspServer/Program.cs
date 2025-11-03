using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
// using System.Diagnostics.CodeAnalysis; // <-- АТРИБУТЫ УДАЛЕНЫ
using System.Security.Cryptography; 

public class Program
{
    public static async Task Main(string[] args)
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });
        
        ILogger<MyEchoServer> logger = loggerFactory.CreateLogger<MyEchoServer>();
        MyEchoServer server = new MyEchoServer(5000, logger);
        
        _ = Task.Run(() => server.StartAsync()); 

        string host = "127.0.0.1";
        int port = 60000;
        int intervalMilliseconds = 5000;

        using (var sender = new UdpTimedSender(host, port))
        {
            Console.WriteLine("Press any key to stop sending...");
            sender.StartSending(intervalMilliseconds);

            Console.WriteLine("Press 'q' to quit...");
            while (Console.ReadKey(intercept: true).Key != ConsoleKey.Q)
            {
                // Wait
            }

            sender.StopSending();
            server.Stop();
            Console.WriteLine("Sender stopped.");
        }
    }
}

public class UdpTimedSender : IDisposable
{
    private readonly string _host;
    private readonly int _port;
    private readonly UdpClient _udpClient;
    private Timer _timer;

    public UdpTimedSender(string host, int port)
    {
        _host = host;
        _port = port;
        _udpClient = new UdpClient();
    }

    public void StartSending(int intervalMilliseconds)
    {
        if (_timer != null)
            throw new InvalidOperationException("Sender is already running.");

        _timer = new Timer(SendMessageCallback, null, 0, intervalMilliseconds);
    }

    ushort i = 0;

    private void SendMessageCallback(object state)
    {
        try
        {
            byte[] samples = new byte[1024];
            RandomNumberGenerator.Fill(samples);
            
            i++;

            byte[] msg = (new byte[] { 0x04, 0x04 }).Concat(BitConverter.GetBytes(i)).Concat(samples).ToArray();
            var endpoint = new IPEndPoint(IPAddress.Parse(_host), _port);

            _udpClient.Send(msg, msg.Length, endpoint);
            Console.WriteLine($"Message sent to {_host}:{_port} ");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending message: {ex.Message}");
        }
    }

    public void StopSending()
    {
        _timer?.Dispose();
        _timer = null;
    }
    
    public void Dispose()
    {
        StopSending();
        _udpClient.Dispose();
    }
}
