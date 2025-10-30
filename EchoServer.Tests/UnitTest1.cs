using EchoServer; // <-- Переконайтеся, що це правильний namespace вашого EchoServer
using Microsoft.Extensions.Logging.Abstractions; // Потрібно для NullLogger
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EchoServer.Tests
{
    // Я залишив назву класу UnitTest1, щоб вона відповідала вашому файлу
    public class UnitTest1
    {
        [Fact]
        public async Task ProcessClientStreamAsync_ShouldEchoData_WhenDataIsSent()
        {
            // --- Arrange (Підготовка) ---

            // 1. Створюємо "фейковий" логгер, який нічого не робить
            var logger = new NullLogger<EchoServer>();
            
            // 2. Створюємо наш сервер (порт не має значення для цього тесту)
            var server = new EchoServer(1234, logger);
            
            // 3. Готуємо наше "повідомлення"
            var testMessage = "Hello World";
            var testBytes = Encoding.UTF8.GetBytes(testMessage);

            // 4. Створюємо MemoryStream, який імітує мережеве підключення
            using (var stream = new MemoryStream())
            {
                // Імітуємо, що клієнт надіслав дані
                await stream.WriteAsync(testBytes, 0, testBytes.Length);
                stream.Position = 0; // "Перемотуємо" стрім на початок, щоб сервер міг його прочитати

                // --- Act (Дія) ---
                
                // Викликаємо ТІЛЬКИ той метод, який ми рефакторили
                await server.ProcessClientStreamAsync(stream, CancellationToken.None);

                // --- Assert (Перевірка) ---
                
                // 'stream' тепер має містити "Hello WorldHello World"
                // (оригінальне повідомлення + "ехо"-відповідь)
                
                // Перевіряємо, що розмір стріму подвоївся
                Assert.Equal(testBytes.Length * 2, stream.Length);

                // Читаємо "ехо"-відповідь (другу половину стріму)
                stream.Position = testBytes.Length; // Переходимо до початку відповіді
                var buffer = new byte[testBytes.Length];
                await stream.ReadAsync(buffer, 0, buffer.Length);
                
                var echoedMessage = Encoding.UTF8.GetString(buffer);
                
                // Перевіряємо, що відповідь збігається з оригіналом
                Assert.Equal(testMessage, echoedMessage);
            }
        }
    }
}