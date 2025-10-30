// using EchoServer; // <--- ВИДАЛІТЬ АБО ЗАКОМЕНТУЙТЕ ЦЕЙ РЯДОК
using Microsoft.Extensions.Logging.Abstractions;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EchoServer.Tests
{
    public class UnitTest1
    {
        [Fact]
        public async Task ProcessClientStreamAsync_ShouldEchoData_WhenDataIsSent()
        {
            // ... (весь код тесту залишається без змін)
            var logger = new NullLogger<EchoServer>();
            var server = new EchoServer(1234, logger);
            // ...
        }
    }
}
