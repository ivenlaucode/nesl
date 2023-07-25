using Nesl.EslClient.Transport.Message;

namespace Nesl.EslClient.Transport;

public class CommandResponse
{
    private readonly string _command;
    private readonly string _replyText = "";
    private readonly EslMessage? _response;
    private readonly bool _success;

    public CommandResponse(string command, EslMessage? response)
    {
        _command = command;
        _response = response;
        if (response != null) _replyText = response.GetHeaderValue(HeaderName.ReplyText);
        _success = _replyText.StartsWith(HeaderValue.Ok);
    }

    public string GetCommand()
    {
        return _command;
    }

    public bool IsOk()
    {
        return _success;
    }

    public string GetReplyText()
    {
        return _replyText;
    }

    public EslMessage? GetResponse()
    {
        return _response;
    }
}