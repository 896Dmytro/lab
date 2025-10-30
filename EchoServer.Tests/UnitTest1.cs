using Microsoft.Extensions.Logging.Abstractions;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

//
// --- ОСЬ КОРІННЕ ВИПРАВЛЕННЯ КОНФЛІКТУ ІМЕН ---
//
namespace EchoServer.UnitTests // Раніше було 'EchoServer.Tests'
{
    public class UnitTest1
    {
        [Fact]
        public async Task ProcessClientStreamAsync_ShouldEchoData_WhenDataIsSent()
        {
            var logger = new NullLogger<EchoServer>();
            var server = new EchoServer(1234, logger);
            
            var testMessage = "Hello World";
            var testBytes = Encoding.UTF8.GetBytes(testMessage);

            using (var stream = new MemoryStream())
            {
                await stream.WriteAsync(testBytes, 0, testBytes.Length);
                stream.Position = 0; 
                await server.ProcessClientStreamAsync(stream, CancellationToken.None);
                
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