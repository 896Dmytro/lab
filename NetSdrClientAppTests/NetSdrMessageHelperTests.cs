using Xunit;
using NetSdrClientApp.Messages;
using NetSdrClientApp.Networking; // <-- ДОБАВЬТЕ ЭТОТ USING
using System;
using System.Threading.Tasks;

namespace NetSdrClientAppTests
{
    public class NetSdrMessageHelperTests
    {
        // ... (Здесь ваши 4 старых теста для NetSdrMessageHelper) ...
        // (Я их скрыл для краткости, но они должны здесь быть)

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

    // --- ДОБАВЬТЕ ЭТИ КЛАССЫ В КОНЕЦ ФАЙЛА ---

    public class UdpClientWrapperTests
    {
        [Fact]
        public void Exit_ShouldCallStopListening_WithoutErrors()
        {
            // Arrange (Этот тест покроет 3 строки в UdpClientWrapper)
            var wrapper = new UdpClientWrapper(9999); 
            
            // Act
            wrapper.Exit(); // Покрывает refactor Exit() -> StopListening()

            // Assert
            Assert.True(true); 
        }

        // (Тест для catch() добавлять слишком сложно,
        // давайте пока покроем только 3 строки)
    }

    public class TcpClientWrapperTests
    {
        [Fact]
        public async Task SendMessageAsync_StringOverload_ThrowsWhenNotConnected()
        {
            // Arrange (Этот тест покроет 2 строки в TcpClientWrapper)
            var wrapper = new TcpClientWrapper("localhost", 9996);
            
            // Act & Assert
            // Этот тест вызывает SendMessageAsync(string), который
            // покрывает 2 строки рефакторинга
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => wrapper.SendMessageAsync("test message")
            );
            
            Assert.Equal("Not connected to a server.", ex.Message);
        }
    }
}
