using Xunit;
using NetSdrClientApp.Messages;
using NetSdrClientApp.Networking; 
using System;
using System.Threading.Tasks;
using NetArchTest.Rules; 
using System.Reflection; 
using NetSdrClientApp; 
using System.Collections.Generic;
using System.Threading; 

namespace NetSdrClientAppTests
{
    // --- ВАШІ СТАРІ ТЕСТИ (ОСТАЮТЬСЯ БЕЗ ІЗМІН) ---
    public class NetSdrMessageHelperTests
    {
        [Fact]
        public void GetControlItemMessage_ShouldCreateCorrectByteArray()
        {
            var type = NetSdrMessageHelper.MsgTypes.SetControlItem;
            var itemCode = NetSdrMessageHelper.ControlItemCodes.ReceiverFrequency;
            var parameters = new byte[] { 0xDE, 0xAD };
            var expectedResult = new byte[] { 0x06, 0x00, 0x20, 0x00, 0xDE, 0xAD };
            var actual = NetSdrMessageHelper.GetControlItemMessage(type, itemCode, parameters);
            Assert.Equal(expectedResult, actual);
        }

        [Fact]
        public void GetDataItemMessage_WithEmptyParams_ShouldCreateCorrectByteArray()
        {
            var type = NetSdrMessageHelper.MsgTypes.Ack;
            var parameters = Array.Empty<byte>();
            var expectedResult = new byte[] { 0x02, 0x60 };
            var actual = NetSdrMessageHelper.GetDataItemMessage(type, parameters);
            Assert.Equal(expectedResult, actual);
        }

        [Fact]
        public void GetControlItemMessage_RFFilter_ShouldCreateCorrectByteArray()
        {
            var type = NetSdrMessageHelper.MsgTypes.SetControlItem;
            var itemCode = NetSdrMessageHelper.ControlItemCodes.RFFilter;
            var parameters = new byte[] { 0x01 };
            var expectedResult = new byte[] { 0x05, 0x00, 0x44, 0x00, 0x01 };
            var actual = NetSdrMessageHelper.GetControlItemMessage(type, itemCode, parameters);
            Assert.Equal(expectedResult, actual);
        }

        [Fact]
        public void GetDataItemMessage_DataItem1_ShouldCreateCorrectByteArray()
        {
            var type = NetSdrMessageHelper.MsgTypes.DataItem1;
            var parameters = new byte[] { 0xAA, 0xBB, 0xCC };
            var expectedResult = new byte[] { 0x05, 0xA0, 0xAA, 0xBB, 0xCC };
            var actual = NetSdrMessageHelper.GetDataItemMessage(type, parameters);
            Assert.Equal(expectedResult, actual);
        }
    }

    public class UdpClientWrapperTests
    {
        [Fact]
        public void Exit_ShouldCallStopListening_WithoutErrors()
        {
            var wrapper = new UdpClientWrapper(9999); 
            wrapper.Exit(); 
            Assert.True(true); 
        }
    }

    public class TcpClientWrapperTests
    {
        [Fact]
        public async Task SendMessageAsync_StringOverload_ThrowsWhenNotConnected()
        {
            var wrapper = new TcpClientWrapper("localhost", 9996);
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => wrapper.SendMessageAsync("test message")
            );
            Assert.Equal("Not connected to a server.", ex.Message);
        }
    }

    // --- НОВІ ТЕСТИ ДЛЯ NETSDRCLIENT (БЕЗ MOQ) ---

    #region "Ручні стаби" (Manual Stubs)
    
    //
    // <--- ВОТ ИСПРАВЛЕНИЕ "ЗАВИСАНИЯ" (HANG)
    //
    public class StubTcpClient : ITcpClient
    {
        public bool IsConnected { get; set; } = false;
        public int ConnectCallCount { get; private set; } = 0;
        public int SendMessageAsyncCallCount { get; private set; } = 0;
        public bool Connected => IsConnected;
        public event EventHandler<byte[]> MessageReceived;
        public void Connect()
        {
            ConnectCallCount++;
            IsConnected = true; 
        }
        public void Disconnect() { }
        
        // Коли SendMessageAsync викликається...
        public Task SendMessageAsync(byte[] data)
        {
            SendMessageAsyncCallCount++;
            // ...негайно симулюємо відповідь, щоб "розблокувати" SendTcpRequest
            MessageReceived?.Invoke(this, new byte[] { 0x01 }); 
            return Task.CompletedTask;
        }
        public Task SendMessageAsync(string str)
        {
            SendMessageAsyncCallCount++;
            MessageReceived?.Invoke(this, new byte[] { 0x01 }); 
            return Task.CompletedTask;
        }
    }

    public class StubUdpClient : IUdpClient
    {
        public int StartListeningAsyncCallCount { get; private set; } = 0;
        public int StopListeningCallCount { get; private set; } = 0;
        public event EventHandler<byte[]> MessageReceived; 
        public Task StartListeningAsync()
        {
            StartListeningAsyncCallCount++;
            return Task.CompletedTask;
        }
        public void StopListening()
        {
            StopListeningCallCount++;
        }
        
        public void Exit() { }
    }
    
    #endregion

    public class NetSdrClientTests
    {
        private readonly StubTcpClient _stubTcpClient;
        private readonly StubUdpClient _stubUdpClient;
        private readonly NetSdrClient _client; 

        public NetSdrClientTests()
        {
            _stubTcpClient = new StubTcpClient();
            _stubUdpClient = new StubUdpClient();
            _client = new NetSdrClient(_stubTcpClient, _stubUdpClient);
        }
        
        [Fact]
        public async Task ConnectAsync_WhenNotConnected_ShouldCallTcpConnectAndSendMessages()
        {
            _stubTcpClient.IsConnected = false;
            await _client.ConnectAsync();
            Assert.Equal(1, _stubTcpClient.ConnectCallCount);
            Assert.Equal(3, _stubTcpClient.SendMessageAsyncCallCount);
        }

        [Fact]
        public async Task ConnectAsync_WhenAlreadyConnected_ShouldDoNothing()
        {
            _stubTcpClient.IsConnected = true;
            await _client.ConnectAsync();
            Assert.Equal(0, _stubTcpClient.ConnectCallCount);
            Assert.Equal(0, _stubTcpClient.SendMessageAsyncCallCount);
        }

        [Fact]
        public async Task StartIQAsync_WhenConnected_ShouldSendStartMessageAndListen()
        {
            _stubTcpClient.IsConnected = true;
            await _client.StartIQAsync();
            Assert.True(_client.IQStarted);
            Assert.Equal(1, _stubTcpClient.SendMessageAsyncCallCount);
            Assert.Equal(1, _stubUdpClient.StartListeningAsyncCallCount);
        }

        [Fact]
        public async Task StopIQAsync_WhenConnected_ShouldSendStopMessageAndStopListening()
        {
            _stubTcpClient.IsConnected = true;
            await _client.StartIQAsync(); 
            await _client.StopIQAsync();
            Assert.False(_client.IQStarted);
            Assert.Equal(2, _stubTcpClient.SendMessageAsyncCallCount); // 1 на старт + 1 на стоп
            Assert.Equal(1, _stubUdpClient.StopListeningCallCount);
        }
    }
}