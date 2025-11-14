using Xunit;
using NetSdrClientApp.Messages;
using NetSdrClientApp.Networking; 
using System;
using System.Threading.Tasks;
using NetArchTest.Rules; 
using System.Reflection; 
// using Moq; // <-- УДАЛЕНО
using NetSdrClientApp; 
using System.Collections.Generic; // <-- Добавлено
using System.Threading; // <-- Добавлено

namespace NetSdrClientAppTests
{
    // --- ВАШИ СТАРЫЕ ТЕСТЫ (ОСТАЮТСЯ БЕЗ ИЗМЕНЕНИЙ) ---
    public class NetSdrMessageHelperTests
    {
        [Fact]
        public void GetControlItemMessage_ShouldCreateCorrectByteArray() { /* ... */ }
        [Fact]
        public void GetDataItemMessage_WithEmptyParams_ShouldCreateCorrectByteArray() { /* ... */ }
        [Fact]
        public void GetControlItemMessage_RFFilter_ShouldCreateCorrectByteArray() { /* ... */ }
        [Fact]
        public void GetDataItemMessage_DataItem1_ShouldCreateCorrectByteArray() { /* ... */ }
    }

    public class UdpClientWrapperTests
    {
        [Fact]
        public void Exit_ShouldCallStopListening_WithoutErrors() { /* ... */ }
    }

    public class TcpClientWrapperTests
    {
        [Fact]
        public async Task SendMessageAsync_StringOverload_ThrowsWhenNotConnected() { /* ... */ }
    }

    // --- НОВЫЕ ТЕСТЫ ДЛЯ NETSDRCLIENT (БЕЗ MOQ) ---

    #region "Ручные стабы" (Manual Stubs)
    
    // Это "фальшивый" TCP клиент для тестов
    public class StubTcpClient : ITcpClient
    {
        public bool IsConnected { get; set; } = false;
        public int ConnectCallCount { get; private set; } = 0;
        public int SendMessageAsyncCallCount { get; private set; } = 0;

        // Имплементируем интерфейс
        public bool Connected => IsConnected;
        public event EventHandler<byte[]> MessageReceived; // Нам не нужен для этих тестов

        public void Connect()
        {
            ConnectCallCount++;
            IsConnected = true; // Симулируем подключение
        }

        public void Disconnect() { }
        public Task SendMessageAsync(byte[] data)
        {
            SendMessageAsyncCallCount++;
            return Task.CompletedTask;
        }
        public Task SendMessageAsync(string str)
        {
            SendMessageAsyncCallCount++;
            return Task.CompletedTask;
        }
    }

    // Это "фальшивый" UDP клиент для тестов
    public class StubUdpClient : IUdpClient
    {
        public int StartListeningAsyncCallCount { get; private set; } = 0;
        public int StopListeningCallCount { get; private set; } = 0;

        // Имплементируем интерфейс
        public event EventHandler<byte[]> MessageReceived; // Не нужен
        public Task StartListeningAsync()
        {
            StartListeningAsyncCallCount++;
            return Task.CompletedTask;
        }
        public void StopListening()
        {
            StopListeningCallCount++;
        }
    }
    
    #endregion

    public class NetSdrClientTests
    {
        private readonly StubTcpClient _stubTcpClient;
        private readonly StubUdpClient _stubUdpClient;
        private readonly NetSdrClient _client; 

        public NetSdrClientTests()
        {
            // --- Arrange (Подготовка) ---
            
            // 1. Создаем наши "фальшивые" клиенты
            _stubTcpClient = new StubTcpClient();
            _stubUdpClient = new StubUdpClient();

            // 2. Создаем реальный NetSdrClient, передавая ему наши фальшивки
            _client = new NetSdrClient(_stubTcpClient, _stubUdpClient);
        }

        [Fact]
        public async Task ConnectAsync_WhenNotConnected_ShouldCallTcpConnectAndSendMessages()
        {
            // --- Arrange ---
            _stubTcpClient.IsConnected = false;

            // --- Act ---
            await _client.ConnectAsync();

            // --- Assert (Проверка) ---
            Assert.Equal(1, _stubTcpClient.ConnectCallCount);
            Assert.Equal(3, _stubTcpClient.SendMessageAsyncCallCount);
        }

        [Fact]
        public async Task ConnectAsync_WhenAlreadyConnected_ShouldDoNothing()
        {
            // --- Arrange ---
            _stubTcpClient.IsConnected = true;

            // --- Act ---
            await _client.ConnectAsync();

            // --- Assert ---
            Assert.Equal(0, _stubTcpClient.ConnectCallCount);
            Assert.Equal(0, _stubTcpClient.SendMessageAsyncCallCount);
        }

        [Fact]
        public async Task StartIQAsync_WhenConnected_ShouldSendStartMessageAndListen()
        {
            // --- Arrange ---
            _stubTcpClient.IsConnected = true;

            // --- Act ---
            await _client.StartIQAsync();

            // --- Assert ---
            Assert.True(_client.IQStarted);
            Assert.Equal(1, _stubTcpClient.SendMessageAsyncCallCount); // 1 TCP-запрос
            Assert.Equal(1, _stubUdpClient.StartListeningAsyncCallCount); // 1 UDP-старт
        }

        [Fact]
        public async Task StopIQAsync_WhenConnected_ShouldSendStopMessageAndStopListening()
        {
            // --- Arrange ---
            _stubTcpClient.IsConnected = true;
            await _client.StartIQAsync(); // Сначала запускаем

            // --- Act ---
            await _client.StopIQAsync();

            // --- Assert ---
            Assert.False(_client.IQStarted);
            Assert.Equal(2, _stubTcpClient.SendMessageAsyncCallCount); // 1 на старт + 1 на стоп
            Assert.Equal(1, _stubUdpClient.StopListeningCallCount);
        }
    }
}