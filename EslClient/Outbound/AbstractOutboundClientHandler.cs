using Nesl.EslClient.Internal;
using Nesl.EslClient.Transport.Event;
using DotNetty.Transport.Channels;

namespace Nesl.EslClient.Outbound;

public abstract class AbstractOutboundClientHandler : AbstractEslClientHandler
{
    protected override async void HandleEslEvent(IChannelHandlerContext ctx, EslEvent e)
    {
        var response = await SendApiSingleLineCommand(ctx.Channel, "connect");
        // The message decoder for outbound, treats most of this incoming message as an 'event' in 
        // message body, so it parse now
        if (response != null)
        {
            var channelDataEvent = new EslEvent(response, true);
            // Let implementing sub classes choose what to do next
            HandleConnectResponse(ctx, channelDataEvent);
        }
    }

    protected override void HandleAuthRequest(IChannelHandlerContext ctx)
    {
        Console.WriteLine("Auth request received in outbound mode, ignoring");
    }

    protected override void HandleDisconnectionNotice()
    {
        Console.WriteLine("Received disconnection notice");
    }

    protected abstract void HandleConnectResponse(IChannelHandlerContext ctx, EslEvent e);
}