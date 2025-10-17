using Xunit;
using NetSdrClientApp.Messages;
using NetSdrClientApp;

namespace NetSdrClientAppTests;

public class CoverageTests
{
    [Fact]
    public void Test1_ConnectMessage_CreatesValidMessage()
    {
        var message = NetSdrMessageHelper.CreateConnectMessage("TEST");
        Assert.NotNull(message);
        Assert.Equal(MessageType.Command, message.Header.MessageType);
    }
    
    [Fact]
    public void Test2_InvalidData_ParseReturnsFalse()
    {
        var result = NetSdrMessage.TryParse(new byte[] { 0x00 }, out var message);
        Assert.False(result);
        Assert.Null(message);
    }
    
    [Fact]
    public void Test3_DataMessage_WithPayload_SetsCorrectLength()
    {
        var payload = new byte[] { 0x01, 0x02, 0x03 };
        var message = NetSdrMessageHelper.CreateDataMessage(payload);
        Assert.NotNull(message);
        Assert.Equal(payload.Length, message.Header.DataItemCount);
    }
    
    [Fact]
    public void Test4_MessageHeader_DefaultConstructor_Works()
    {
        var header = new MessageHeader();
        Assert.Equal(MessageType.Command, header.MessageType);
        Assert.Equal(0, header.DataItemCount);
    }
    
    [Fact]
    public void Test5_CreateDisconnectMessage_CreatesMessage()
    {
        var message = NetSdrMessageHelper.CreateDisconnectMessage();
        Assert.NotNull(message);
        Assert.Equal(MessageType.Command, message.Header.MessageType);
    }
    
    [Fact] 
    public void Test6_MessageHeader_Properties_CanBeSet()
    {
        var header = new MessageHeader();
        header.MessageType = MessageType.Data;
        header.DataItemCount = 5;
        
        Assert.Equal(MessageType.Data, header.MessageType);
        Assert.Equal(5, header.DataItemCount);
    }
}
