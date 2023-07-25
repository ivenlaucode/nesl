namespace Nesl.EslClient.Transport.Message;

public static class HeaderName
{
    public const string ContentType = "Content-Type";

    public const string ContentLength = "Content-Length";

    public const string ReplyText = "Reply-Text";

    public const string JobUuid = "Job-UUID";

    public const string SocketMode = "Socket-Mode";

    public const string Control = "Control";
}

public static class HeaderValue
{
    public const string Ok = "+OK";

    public const string AuthRequest = "auth/request";

    public const string ApiResponse = "api/response";

    public const string CommandReply = "command/reply";

    public const string TextEventPlain = "text/event-plain";

    public const string TextEventXml = "text/event-xml";

    public const string TextDisconnectNotice = "text/disconnect-notice";

    public const string ErrInvalid = "-ERR invalid";
}