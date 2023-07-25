using System.Text;
using Nesl.EslClient.Transport.Event;
using Nesl.EslClient.Transport.Message;
using DotNetty.Transport.Channels;

namespace Nesl.EslClient.Internal;

public abstract class AbstractEslClientHandler : SimpleChannelInboundHandler<EslMessage>
{
    private static readonly string MessageTerminator = "\n\n";
    private static readonly string LineTerminator = "\n";

    private readonly Queue<MessageCallbask> _apiCallbacks = new();

    public Queue<MessageCallbask> ApiCallbacks => _apiCallbacks;

    protected AbstractEslClientHandler() : this(false)
    {
    }

    protected AbstractEslClientHandler(bool autoRelease) : base(autoRelease)
    {
    }

    protected override void ChannelRead0(IChannelHandlerContext ctx, EslMessage message)
    {
        var contentType = message.GetContentType();
        if (contentType.Equals(HeaderValue.TextEventPlain) ||
            contentType.Equals(HeaderValue.TextEventXml))
        {
            //  transform into an event
            var eslEvent = new EslEvent(message);
            if (eslEvent.GetEventName().Equals("BACKGROUND_JOB"))
            {
                var headers = eslEvent.GetEventHeaders();
                if (headers != null)
                {
                    // todo
                }
            }
            else
            {
                HandleEslEvent(ctx, eslEvent);
            }
        }
        else
        {
            HandleEslMessage(ctx, message);
        }
    }

    private void HandleEslMessage(IChannelHandlerContext ctx, EslMessage message)
    {
        var contentType = message.GetContentType();

        switch (contentType)
        {
            case HeaderValue.ApiResponse:
            case HeaderValue.CommandReply:
                var callbask = ApiCallbacks.Dequeue();
                callbask.Message = message;
                break;

            case HeaderValue.AuthRequest:
                HandleAuthRequest(ctx);
                break;

            case HeaderValue.TextDisconnectNotice:
                HandleDisconnectionNotice();
                break;
        }
    }

    public async Task<EslMessage?> SendApiSingleLineCommand(IChannel channel, string command)
    {
        var callbask = new MessageCallbask();
        ApiCallbacks.Enqueue(callbask);
        await channel.WriteAndFlushAsync(command + MessageTerminator);
        return await Task.Run(() => callbask.Message);
    }

    public Task<EslMessage?> SendSyncApiCommand(IChannel channel, string command, string arg)
    {
        CheckArgument(!string.IsNullOrEmpty(command), "command may not be null or empty");
        CheckArgument(!string.IsNullOrEmpty(arg), "arg may not be null or empty");

        return SendApiSingleLineCommand(channel, "api " + command + ' ' + arg);
    }

    public async Task<EslMessage?> SendApiMultiLineCommand(IChannel channel, List<string> commandLines)
    {
        var sb = new StringBuilder();
        foreach (var line in commandLines)
        {
            sb.Append(line);
            sb.Append(LineTerminator);
        }

        sb.Append(LineTerminator);

        var callbask = new MessageCallbask();
        ApiCallbacks.Enqueue(callbask);
        await channel.WriteAndFlushAsync(sb.ToString());
        return await Task.Run(() => callbask.Message);
    }

    public async Task<string> SendBackgroundApiCommand(IChannel channel, string command)
    {
        var result = await SendApiSingleLineCommand(channel, command);
        if (result != null && result.HasHeader(HeaderName.JobUuid))
        {
            var jobId = result.GetHeaderValue(HeaderName.JobUuid);
            return jobId;
        }

        throw new Exception("Missing Job-UUID header in bgapi response");
    }

    private void CheckArgument(bool success, string message)
    {
        if (!success) Console.WriteLine(message);
    }

    protected abstract void HandleEslEvent(IChannelHandlerContext ctx, EslEvent e);

    protected abstract void HandleAuthRequest(IChannelHandlerContext ctx);

    protected abstract void HandleDisconnectionNotice();
}

public class MessageCallbask
{
    private readonly CountdownEvent _countdown = new(1);
    private EslMessage? _message;

    public EslMessage? Message
    {
        get
        {
            Countdown.Wait();
            return _message;
        }
        set
        {
            _message = value;
            Countdown.Signal();
        }
    }

    public CountdownEvent Countdown => _countdown;
}