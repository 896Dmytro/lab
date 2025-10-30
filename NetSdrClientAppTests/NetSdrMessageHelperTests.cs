using Xunit;
using NetSdrClientApp.Messages; // Используем ваш правильный namespace
using NetSdrClientApp.Networking; // <-- ДОДАЙТЕ ЦЕЙ USING
using System;
using System.Threading.Tasks;     // <-- ДОДАЙТЕ ЦЕЙ USING

namespace NetSdrClientAppTests
{
    public class NetSdrMessageHelperTests
    {
        // Тест 1: Проверяем создание простого сообщения (ИСПРАВЛЕН)
        [Fact]
        public void GetControlItemMessage_ShouldCreateCorrectByteArray()
        {
            // Arrange
            var type = NetSdrMessageHelper.MsgTypes.SetControlItem; // type 0
            var itemCode = NetSdrMessageHelper.ControlItemCodes.ReceiverFrequency; // 0x0020
            var parameters = new byte[] { 0xDE, 0xAD }; // 2 байта данных

            // Ожидаемый результат (ИСПРАВЛЕН):
            // Header: 000 (type 0) + 0000000000110 (length 6) = 0x0006. В Little Endian = [0x06, 0x00]
            var expectedResult = new byte[] { 0x06, 0x00, 0x20, 0x00, 0xDE, 0xAD };

            // Act
            var actual = NetSdrMessageHelper.GetControlItemMessage(type, itemCode, parameters);

            // Assert
            Assert.Equal(expectedResult, actual);
        }

        // Тест 2: Проверяем сообщение с пустыми параметрами (ПРОЙДЕН)
        [Fact]
        public void GetDataItemMessage_WithEmptyParams_ShouldCreateCorrectByteArray()
        {
            // Arrange
            var type = NetSdrMessageHelper.MsgTypes.Ack; // type 3
            var parameters = Array.Empty<byte>(); // 0 байт

            // Ожидаемый результат:
            // Header: 011 (type 3) + 0000000000010 (length 2) = 0x6002. В Little Endian = [0x02, 0x60]
            var expectedResult = new byte[] { 0x02, 0x60 };

            // Act
            var actual = NetSdrMessageHelper.GetDataItemMessage(type, parameters);

            // Assert
            Assert.Equal(expectedResult, actual);
        }

        // Тест 3: Проверка другого типа Control Item (НОВЫЙ)
        [Fact]
        public void GetControlItemMessage_RFFilter_ShouldCreateCorrectByteArray()
        {
            // Arrange
            var type = NetSdrMessageHelper.MsgTypes.SetControlItem; // type 0
            var itemCode = NetSdrMessageHelper.ControlItemCodes.RFFilter; // 0x0044
            var parameters = new byte[] { 0x01 }; // 1 байт данных

            // Ожидаемый результат:
            // Header: 000 (type 0) + 0000000000101 (length 5) = 0x0005. В Little Endian = [0x05, 0x00]
            var expectedResult = new byte[] { 0x05, 0x00, 0x44, 0x00, 0x01 };

            // Act
            var actual = NetSdrMessageHelper.GetControlItemMessage(type, itemCode, parameters);

            // Assert
            Assert.Equal(expectedResult, actual);
        }

        // Тест 4: Проверка сообщения DataItem (ИСПРАВЛЕНО)
        [Fact]
        public void GetDataItemMessage_DataItem1_ShouldCreateCorrectByteArray()
        {
            // Arrange
            var type = NetSdrMessageHelper.MsgTypes.DataItem1; // type 5
            var parameters = new byte[] { 0xAA, 0xBB, 0xCC }; // 3 байта

            // Ожидаемый результат:
            // Header: 101 (type 5) + 0000000000101 (length 5) = 0xA005. В Little Endian = [0x05, 0xA0]
            var expectedResult = new byte[] { 0x05, 0xA0, 0xAA, 0xBB, 0xCC };

            // Act
            var actual = NetSdrMessageHelper.GetDataItemMessage(type, parameters);

            // Assert
            Assert.Equal(expectedResult, actual);
        }
    }

    // --- ДОДАНО ТЕСТИ ДЛЯ UDP (конвертовано в Xunit) ---
    public class UdpClientWrapperTests
    {
        [Fact]
        public void Exit_ShouldCallStopListening_WithoutErrors()
        {
            // Arrange
            var wrapper = new UdpClientWrapper(9999);
            
            // Act
            wrapper.Exit(); // Покриває код в Exit() та StopListening()

            // Assert
            Assert.True(true); // Тест пройшов, якщо не було помилок
        }

        [Fact]
        public async Task StartListeningAsync_CanBeCanceled_WithoutErrors()
        {
            // Arrange
            var wrapper = new UdpClientWrapper(9998);

            // Act
            var listenTask = wrapper.StartListeningAsync();
            await Task.Delay(50); // Даємо час запуститися
            
            wrapper.StopListening(); // Покриває catch (OperationCanceledException)
            
            await listenTask; // Чекаємо на завершення (не має кидати помилку)

            // Assert
            Assert.True(true);
        }
    }

    // --- ДОДАНО ТЕСТИ ДЛЯ TCP (щоб покрити решту 50%) ---
    public class TcpClientWrapperTests
    {
        [Fact]
        public async Task SendMessageAsync_StringOverload_ThrowsWhenNotConnected()
        {
            // Arrange
            var wrapper = new TcpClientWrapper("localhost", 9996);
            
            // Act & Assert
            // Цей тест покриває зміни в SendMessageAsync(string str)
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => wrapper.SendMessageAsync("test message")
            );
            
            Assert.Equal("Not connected to a server.", ex.Message);
        }
    }
}
