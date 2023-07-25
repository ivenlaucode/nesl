using Nesl.EslClient.Transport;
using Nesl.EslClient.Transport.Event;
using DotNetty.Transport.Channels;

namespace Nesl.EslClient.Outbound;

public class SimpleHangupOutboundHandler : AbstractOutboundClientHandler
{
    protected override void HandleConnectResponse(IChannelHandlerContext ctx, EslEvent e)
    {
        var headers = e.GetEventHeaders();
        if (headers != null && e.GetEventName() != null && e.GetEventName().Equals("CHANNEL_DATA"))
        {
            // this is the response to the initial connect 
            Console.WriteLine("=======================  incoming channel data  =============================");
            Console.WriteLine("Event-Date-Local: [{}]", e.GetEventDateLocal());
            Console.WriteLine("Unique-ID: [{}]", headers["Unique-ID"]);      
            Console.WriteLine("Channel-ANI: [{}]", headers["Channel-ANI"]);
            Console.WriteLine("Answer-State: [{}]", headers["Answer-State"]);
            Console.WriteLine("Caller-Destination-Number: [{}]", headers["Caller-Destination-Number"]);
            Console.WriteLine("=======================  = = = = = = = = = = =  =============================");

            // now hangup the call
            HangupCall(ctx.Channel);
        }
        else
        {
            throw new Exception("Unexpected e after connect: [" + e.GetEventName() + ']');
        }
    }

    protected override void HandleEslEvent(IChannelHandlerContext ctx, EslEvent e)
    {
        Console.WriteLine("Received e [{}]", e);
    }

    private async void HangupCall(IChannel channel)
    {
        var hangupMsg = new SendMsg();
        hangupMsg.AddCallCommand("execute");
        hangupMsg.AddExecuteAppName("hangup");

        var response = await SendApiMultiLineCommand(channel, hangupMsg.GetMsgLines());

        if (response != null && response.GetHeaderValue("Reply-Text").StartsWith("+OK"))
            Console.WriteLine("Call hangup successful");
        else if (response != null) Console.WriteLine("Call hangup failed: [{}}", response.GetHeaderValue("Reply-Text"));
    }

}