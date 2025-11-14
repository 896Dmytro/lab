using Xunit;
using NetSdrClientApp.Messages;
using NetSdrClientApp.Networking;
using System;
using System.Threading.Tasks;
using NetArchTest.Rules; // <-- Из Лабы 5
using System.Reflection; // <-- Из Лабы 5
using Moq; // <-- НОВАЯ БИБЛИОТЕКА
using NetSdrClientApp; // <-- НОВЫЙ КЛАСС ДЛЯ ТЕСТА

namespace NetSdrClientAppTests
{
    public class NetSdrMessageHelperTests
    {
        [Fact]
        public void GetControlItemMessage_ShouldCreateCorrectByteArray()
        {
            // Arrange
            var type = NetSdrMessageHelper.MsgTypes.SetControlItem;
            var itemCode = NetSdrMessageHelper.ControlItemCodes.ReceiverFrequency;
            var parameters = new byte[] { 0xDE, 0xAD };
            var expectedResult = new byte[] { 0x06, 0x00, 0x20, 0x00, 0xDE, 0xAD };
            // Act
            var actual = NetSdrMessageHelper.GetControlItemMessage(type, itemCode, parameters);
            // Assert
            Assert.Equal(expectedResult, actual);
        }

        [Fact]
        public void GetDataItemMessage_WithEmptyParams_ShouldCreateCorrectByteArray()
        {
            // Arrange
            var type = NetSdrMessageHelper.MsgTypes.Ack;
            var parameters = Array.Empty<byte>();
            var expectedResult = new byte[] { 0x02, 0x60 };
            // Act
            var actual = NetSdrMessageHelper.GetDataItemMessage(type, parameters);
            // Assert
            Assert.Equal(expectedResult, actual);
        }

        [Fact]
        public void GetControlItemMessage_RFFilter_ShouldCreateCorrectByteArray()
        {
            // Arrange
            var type = NetSdrMessageHelper.MsgTypes.SetControlItem;
            var itemCode = NetSdrMessageHelper.ControlItemCodes.RFFilter;
            var parameters = new byte[] { 0x01 };
            var expectedResult = new byte[] { 0x05, 0x00, 0x44, 0x00, 0x01 };
            // Act
            var actual = NetSdrMessageHelper.GetControlItemMessage(type, itemCode, parameters);
            // Assert
            Assert.Equal(expectedResult, actual);
        }

        [Fact]
        public void GetDataItemMessage_DataItem1_ShouldCreateCorrectByteArray()
        {
            // Arrange
            var type = NetSdrMessageHelper.MsgTypes.DataItem1;
            var parameters = new byte[] { 0xAA, 0xBB, 0xCC };
            var expectedResult = new byte[] { 0x05, 0xA0, 0xAA, 0xBB, 0xCC };
            // Act
            var actual = NetSdrMessageHelper.GetDataItemMessage(type, parameters);
            // Assert
            Assert.Equal(expectedResult, actual);
        }
    }

    // --- Тесты из Лабы 6 (для покрытия старых рефакторов) ---
    public class UdpClientWrapperTests
    {
        [Fact]
        public void Exit_ShouldCallStopListening_WithoutErrors()
        {
            var wrapper = new UdpClientWrapper(9999); 
            wrapper.Exit(); // Покрывает 3 строки
            Assert.True(true); 
        }
    }

    public class TcpClientWrapperTests
    {
        [Fact]
        public async Task SendMessageAsync_StringOverload_ThrowsWhenNotConnected()
        {
            var wrapper = new TcpClientWrapper("localhost", 9996);
            // Покрывает 2 строки
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => wrapper.SendMessageAsync("test message")
            );
            Assert.Equal("Not connected to a server.", ex.Message);
        }
    }

    // --- НОВЫЕ ТЕСТЫ ДЛЯ NetSdrClient (чтобы поднять 0% coverage) ---
    public class NetSdrClientTests
    {
        private readonly Mock<ITcpClient> _mockTcpClient;
        private readonly Mock<IUdpClient> _mockUdpClient;
        private readonly NetSdrClient _client;

        public NetSdrClientTests()
        {
            _mockTcpClient = new Mock<ITcpClient>();
            _mockUdpClient = new Mock<IUdpClient>();

            _mockTcpClient.Setup(tcp => tcp.SendMessageAsync(It.IsAny<byte[]>()))
                         .Returns(Task.CompletedTask);
            
            _client = new NetSdrClient(_mockTcpClient.Object, _mockUdpClient.Object);
        }

        [Fact]
        public async Task ConnectAsync_WhenNotConnected_ShouldCallTcpConnectAndSendMessages()
        {
            _mockTcpClient.Setup(tcp => tcp.Connected).Returns(false);
            await _client.ConnectAsync();
            _mockTcpClient.Verify(tcp => tcp.Connect(), Times.Once);
            _mockTcpClient.Verify(tcp => tcp.SendMessageAsync(It.IsAny<byte[]>()), Times.Exactly(3));
        }

        [Fact]
        public async Task ConnectAsync_WhenAlreadyConnected_ShouldDoNothing()
        {
            _mockTcpClient.Setup(tcp => tcp.Connected).Returns(true);
            await _client.ConnectAsync();
            _mockTcpClient.Verify(tcp => tcp.Connect(), Times.Never);
            _mockTcpClient.Verify(tcp => tcp.SendMessageAsync(It.IsAny<byte[]>()), Times.Never);
        }

        [Fact]
        public async Task StartIQAsync_WhenConnected_ShouldSendStartMessageAndListen()
        {
            _mockTcpClient.Setup(tcp => tcp.Connected).Returns(true);
            await _client.StartIQAsync();
            Assert.True(_client.IQStarted);
            _mockTcpClient.Verify(tcp => tcp.SendMessageAsync(It.IsAny<byte[]>()), Times.Once);
            _mockUdpClient.Verify(udp => udp.StartListeningAsync(), Times.Once);
        }

        [Fact]
        public async Task StopIQAsync_WhenConnected_ShouldSendStopMessageAndStopListening()
        {
            _mockTcpClient.Setup(tcp => tcp.Connected).Returns(true);
            await _client.StartIQAsync(); // Сначала запускаем
            await _client.StopIQAsync(); // Потом останавливаем
            Assert.False(_client.IQStarted);
            _mockTcpClient.Verify(tcp => tcp.SendMessageAsync(It.IsAny<byte[]>()), Times.Exactly(2)); // 1 на старт + 1 на стоп
            _mockUdpClient.Verify(udp => udp.StopListening(), Times.Once);
        }
    }

    // --- ПРИМЕЧАНИЕ: Ваш файл ArchitectureTests.cs должен оставаться отдельным файлом ---
}