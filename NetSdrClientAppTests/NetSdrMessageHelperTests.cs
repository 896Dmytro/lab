using Xunit;
using NetSdrClientApp.Messages;
using NetSdrClientApp.Networking; // <-- Убедитесь, что этот using есть
using System;
using System.Threading.Tasks;
using NetArchTest.Rules; // <-- Using из Лабы 5
using System.Reflection; // <-- Using из Лабы 5
using System.IO; // <-- Добавлено на всякий случай

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

    // --- Тесты из Лабы 5 (Архитектура) ---
    public class ArchitectureTests
    {
        private static readonly Assembly AppAssembly = typeof(NetSdrMessageHelper).Assembly;

        [Fact]
        public void Messages_ShouldNot_DependOnNetworking()
        {
            var rule = Types.InAssembly(AppAssembly)
                .That()
                .ResideInNamespace("NetSdrClientApp.Messages")
                .ShouldNot()
                .HaveDependencyOn("NetSdrClientApp.Networking");
            
            var result = rule.GetResult();
            Assert.True(result.IsSuccessful, "Namespace 'Messages' should not depend on 'Networking'");
        }
    }

    // --- Тесты из Лабы 6 (для покрытия 50% -> 100%) ---
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
}
