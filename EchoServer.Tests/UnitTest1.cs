using Microsoft.Extensions.Logging.Abstractions;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

// Рядок 'using EchoServer;' було видалено, ЯКЩО ВІН ТУТ БУВ

namespace EchoServer.Tests
{
    public class UnitTest1
    {
        [Fact]
        public async Task ProcessClientStreamAsync_ShouldEchoData_WhenDataIsSent()
        {
            // --- Arrange (Підготовка) ---
            
            // 
            // --- ОСЬ ВИПРАВЛЕННЯ ---
            //
            var logger = new NullLogger<global::EchoServer>();
            var server = new global::EchoServer(1234, logger);
            //
            // --- КІНЕЦЬ ВИПРАВЛЕННЯ ---
            //

            var testMessage = "Hello World";
            var testBytes = Encoding.UTF8.GetBytes(testMessage);

            using (var stream = new MemoryStream())
            {
                await stream.WriteAsync(testBytes, 0, testBytes.Length);
                stream.Position = 0; 

                // --- Act (Дія) ---
                await server.ProcessClientStreamAsync(stream, CancellationToken.None);

                // --- Assert (Перевірка) ---
                Assert.Equal(testBytes.Length * 2, stream.Length);

                stream.Position = testBytes.Length; 
                var buffer = new byte[testBytes.Length];
                await stream.ReadAsync(buffer, 0, buffer.Length);
                
                var echoedMessage = Encoding.UTF8.GetString(buffer);
                
                Assert.Equal(testMessage, echoedMessage);
            }
        }
    }
}