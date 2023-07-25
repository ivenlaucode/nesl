using Nesl.EslClient.Internal;
using Nesl.EslClient.Transport;
using Nesl.EslClient.Transport.Event;
using Nesl.EslClient.Transport.Message;
using DotNetty.Transport.Channels;

namespace Nesl.EslClient.Inbound;

public class InboundClientHandler : AbstractEslClientHandler
{
    private readonly IEslProtocolListener _listener;
    private readonly string _password;

    public InboundClientHandler(string password, IEslProtocolListener listener)
    {
        _password = password;
        _listener = listener;
    }

    protected override async void HandleAuthRequest(IChannelHandlerContext ctx)
    {
        var response = await SendApiSingleLineCommand(ctx.Channel, "auth " + _password);
        if (response != null && response.GetContentType().Equals(HeaderValue.CommandReply))
        {
            var commonResponse = new CommandResponse("auth " + _password, response);
            _listener.AuthResponseReceived(commonResponse);
        }
    }

    protected override void HandleDisconnectionNotice()
    {
        _listener.Disconnected();
    }

    protected override void HandleEslEvent(IChannelHandlerContext ctx, EslEvent e)
    {
        _listener.EventReceived(new Context(ctx.Channel, this), e);
    }
}