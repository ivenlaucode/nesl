using System.Text;
using Nesl.EslClient.Transport;
using Nesl.EslClient.Transport.Message;
using DotNetty.Transport.Channels;

namespace Nesl.EslClient.Internal;

public class Context : IModEslApi
{
    private readonly IChannel _channel;
    private readonly AbstractEslClientHandler _handler;

    public IChannel Channel => _channel;

    public AbstractEslClientHandler Handler => _handler;

    public Context(IChannel channel, AbstractEslClientHandler clientHandler)
    {
        _handler = clientHandler;
        _channel = channel;
    }

    public bool CanSend()
    {
        return Channel.Active;
    }

    public async Task<EslMessage?> SendApiCommand(string command, string arg)
    {
        CheckArgument(!string.IsNullOrEmpty(command), "command cannot be null or empty");

        var sb = new StringBuilder();
        sb.Append("api ").Append(command);
        if (!string.IsNullOrEmpty(arg)) sb.Append(' ').Append(arg);

        return await Handler.SendApiSingleLineCommand(Channel, sb.ToString());
    }

    public async Task<string> SendBackgroundApiCommand(string command, string arg)
    {
        CheckArgument(!string.IsNullOrEmpty(command), "command cannot be null or empty");

        var sb = new StringBuilder();
        sb.Append("bgapi ").Append(command);
        if (!string.IsNullOrEmpty(arg)) sb.Append(' ').Append(arg);

        return await Handler.SendBackgroundApiCommand(Channel, sb.ToString());
    }

    public async Task<CommandResponse> SetEventSubscriptions(string format, string events)
    {
        // temporary hack
        CheckArgument(format.Equals("PLAIN"), "Only 'plain' event format is supported at present");

        var sb = new StringBuilder();
        sb.Append("event ").Append(format);
        if (!string.IsNullOrEmpty(events)) sb.Append(' ').Append(events);

        var response = await Handler.SendApiSingleLineCommand(Channel, sb.ToString());
        return new CommandResponse(sb.ToString(), response);
    }

    public async Task<CommandResponse> CancelEventSubscriptions()
    {
        var response = await Handler.SendApiSingleLineCommand(Channel, "noevents");
        return new CommandResponse("noevents", response);
    }

    public async Task<CommandResponse> AddEventFilter(string eventHeader, string valueToFilter)
    {
        CheckArgument(!string.IsNullOrEmpty(eventHeader), "eventHeader cannot be null or empty");

        var sb = new StringBuilder();
        sb.Append("filter ").Append(eventHeader);
        if (!string.IsNullOrEmpty(valueToFilter)) sb.Append(' ').Append(valueToFilter);

        var response = await Handler.SendApiSingleLineCommand(Channel, sb.ToString());
        return new CommandResponse(sb.ToString(), response);
    }

    public async Task<CommandResponse> DeleteEventFilter(string eventHeader, string valueToFilter)
    {
        CheckArgument(!string.IsNullOrEmpty(eventHeader), "eventHeader cannot be null or empty");

        var sb = new StringBuilder();
        sb.Append("filter delete ").Append(eventHeader);
        if (!string.IsNullOrEmpty(valueToFilter)) sb.Append(' ').Append(valueToFilter);

        var response = await Handler.SendApiSingleLineCommand(Channel, sb.ToString());
        return new CommandResponse(sb.ToString(), response);
    }

    public async Task<CommandResponse> SendMessage(SendMsg sendMsg)
    {
        var response = await Handler.SendApiMultiLineCommand(Channel, sendMsg.GetMsgLines());
        return new CommandResponse(sendMsg.ToString(), response);
    }

    public async Task<EslMessage?> SendCommand(string command)
    {
        CheckArgument(!string.IsNullOrEmpty(command), "command cannot be null or empty");

        return await Handler.SendApiSingleLineCommand(Channel, command.ToLowerInvariant().Trim());
    }

    public void CloseChannel()
    {
        if (Channel.Open)
            Channel.CloseAsync();
    }

    private static void CheckArgument(bool success, string message)
    {
        if (!success) Console.WriteLine(message);
        // todo log here
    }
}