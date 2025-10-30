// using EchoServer; // <--- ВИДАЛІТЬ АБО ЗАКОМЕНТУЙТЕ ЦЕЙ РЯДОК
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

// (Решта файлу залишається без змін)
public class Program
{
    public static async Task Main(string[] args)
    {
        // ...
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });
        ILogger<EchoServer> logger = loggerFactory.CreateLogger<EchoServer>();

        EchoServer server = new EchoServer(5000, logger);
        // ...
        // (Решта коду)
        // ...
    }
}

public class UdpTimedSender : IDisposable
{
    // ... (без змін)
}
