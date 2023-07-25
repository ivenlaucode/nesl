using System.Text;

namespace Nesl.EslClient.Transport;

public class SendMsg
{
    private readonly bool _hasUuid;
    private readonly List<string> _msgLines = new();

    public bool HasUuid1 => _hasUuid;

    public List<string> MsgLines => _msgLines;

    public SendMsg()
    {
        _msgLines.Add("sendmsg");
        _hasUuid = false;
    }

    public SendMsg(string uuid)
    {
        _msgLines.Add("sendmsg " + uuid);
        _hasUuid = true;
    }

    public SendMsg AddCallCommand(string command)
    {
        MsgLines.Add("call-command: " + command);
        return this;
    }

    public SendMsg AddExecuteAppName(string appName)
    {
        MsgLines.Add("execute-app-name: " + appName);
        return this;
    }

    public SendMsg AddExecuteAppArg(string arg)
    {
        MsgLines.Add("execute-app-arg: " + arg);
        return this;
    }

    public SendMsg AddLoops(int count)
    {
        MsgLines.Add("loops: " + count);
        return this;
    }

    public SendMsg AddHangupCause(string cause)
    {
        MsgLines.Add("hangup-cause: " + cause);
        return this;
    }

    public SendMsg AddNomediaUuid(string value)
    {
        MsgLines.Add("nomedia-uuid: " + value);
        return this;
    }

    public SendMsg AddEventLock()
    {
        MsgLines.Add("event-lock: true");
        return this;
    }

    public SendMsg AddGenericLine(string name, string value)
    {
        MsgLines.Add(name + ": " + value);
        return this;
    }

    public List<string> GetMsgLines()
    {
        return MsgLines;
    }

    public bool HasUuid()
    {
        return HasUuid1;
    }

    public override string ToString()
    {
        var sb = new StringBuilder("SendMsg: ");
        if (MsgLines.Count > 1)
            sb.Append(MsgLines[0]);
        else if (MsgLines.Count > 0) sb.Append(0);

        return sb.ToString();
    }
}