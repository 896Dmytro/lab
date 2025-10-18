using Xunit;
using NetSdrClientApp.Messages; // Используем ваш правильный namespace
using System;                   // Добавляем для Array.Empty

namespace NetSdrClientAppTests
{
    public class NetSdrMessageHelperTests
    {
        // Тест 1: Проверяем создание простого сообщения (ИСПРАВЛЕН)
        [Fact]
        public void GetControlItemMessage_ShouldCreateCorrectByteArray()
        {
            // Arrange
            var type = NetSdrMessageHelper.MsgTypes.SetControlItem;
            var itemCode = NetSdrMessageHelper.ControlItemCodes.ReceiverFrequency; // 0x0020
            var parameters = new byte[] { 0xDE, 0xAD }; // 2 байта данных

            // Ожидаемый результат (ИСПРАВЛЕН):
            // Header (2) + ItemCode (2) + Params (2) = 6 байт
            // Header: 000 (type 0) + 0000000000110 (length 6) = 0x0006. В Little Endian = [0x06, 0x00]
            // ItemCode: 0x0020. В Little Endian = [0x20, 0x00]
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
            // Header (2) + ItemCode (0) + Params (0) = 2 байта
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
            // Header (2) + ItemCode (2) + Params (1) = 5 байт
            // Header: 000 (type 0) + 0000000000101 (length 5) = 0x0005. В Little Endian = [0x05, 0x00]
            // ItemCode: 0x0044. В Little Endian = [0x44, 0x00]
            var expectedResult = new byte[] { 0x05, 0x00, 0x44, 0x00, 0x01 };

            // Act
            var actual = NetSdrMessageHelper.GetControlItemMessage(type, itemCode, parameters);

            // Assert
            Assert.Equal(expectedResult, actual);
        }

        
    }
}