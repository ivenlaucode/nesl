using System.Text;
using System.Web;
using Nesl.EslClient.Transport.Message;

namespace Nesl.EslClient.Transport.Event;

public class EslEvent
{
    private readonly bool _decodeEventHeaders = true;
    private readonly List<string>? _eventBody;
    private readonly Dictionary<string, string>? _eventHeaders;
    private readonly Dictionary<string, string>? _messageHeaders;

    public bool DecodeEventHeaders => _decodeEventHeaders;

    public List<string>? EventBody => _eventBody;

    public Dictionary<string, string>? EventHeaders => _eventHeaders;

    public Dictionary<string, string>? MessageHeaders => _messageHeaders;

    public EslEvent(EslMessage rawMessage) : this(rawMessage, false)
    {
    }

    public EslEvent(EslMessage rawMessage, bool parseCommandReply)
    {
        _messageHeaders = rawMessage.GetHeaders();

        _eventHeaders = new Dictionary<string, string>(rawMessage.GetBodyLines().Count);

        _eventBody = new List<string>();

        if (rawMessage.GetContentType().Equals(HeaderValue.TextEventPlain))
            ParsePlainBody(rawMessage.GetBodyLines());
        else if (rawMessage.GetContentType().Equals(HeaderValue.TextEventXml))
            throw new Exception("XML events are not yet supported");
        else if (rawMessage.GetContentType().Equals(HeaderValue.CommandReply) && parseCommandReply)
            ParsePlainBody(rawMessage.GetBodyLines());
        else
            throw new Exception("Unexpected EVENT content-type: " +
                                rawMessage.GetContentType());
    }

    public Dictionary<string, string>? GetMessageHeaders()
    {
        return MessageHeaders;
    }

    public Dictionary<string, string>? GetEventHeaders()
    {
        return EventHeaders;
    }

    public List<string>? GetEventBodyLines()
    {
        return EventBody;
    }

    public string GetEventName()
    {
        var dic = GetEventHeaders();
        if (dic != null && dic.ContainsKey(EslEventHeaderNames.EventName))
            return dic[EslEventHeaderNames.EventName];
        return string.Empty;
    }

    public long? GetEventDateTimestamp()
    {
        var dic = GetEventHeaders();
        if (dic != null && dic.ContainsKey(EslEventHeaderNames.EventDateTimestamp))
            return long.Parse(dic[EslEventHeaderNames.EventDateTimestamp]);
        return null;
    }

    public string GetEventDateLocal()
    {
        var dic = GetEventHeaders();
        if (dic != null && dic.ContainsKey(EslEventHeaderNames.EventDateLocal))
            return dic[EslEventHeaderNames.EventDateLocal];
        return string.Empty;
    }

    public string GetEventDateGmt()
    {
        var dic = GetEventHeaders();
        if (dic != null && dic.ContainsKey(EslEventHeaderNames.EventDateGmt))
            return dic[EslEventHeaderNames.EventDateGmt];
        return string.Empty;
    }

    public bool HasEventBody()
    {
        return EventBody != null && EventBody.Count > 0;
    }

    private void ParsePlainBody(List<string> rawBodyLines)
    {
        var isEventBody = false;
        foreach (var rawLine in rawBodyLines)
            if (!isEventBody)
            {
                var headerParts = HeaderParser.SplitHeader(rawLine);
                if (DecodeEventHeaders)
                {
                    try
                    {
                        var decodedValue = HttpUtility.UrlDecode(headerParts[1], Encoding.UTF8);
                        if (EventHeaders != null && !EventHeaders.ContainsKey(headerParts[0]))
                            EventHeaders.Add(headerParts[0], decodedValue);
                    }
                    catch (Exception)
                    {
                        if (EventHeaders != null && !EventHeaders.ContainsKey(headerParts[0]))
                            EventHeaders.Add(headerParts[0], headerParts[1]);
                    }
                }
                else
                {
                    if (EventHeaders != null && !EventHeaders.ContainsKey(headerParts[0]))
                        EventHeaders.Add(headerParts[0], headerParts[1]);
                }

                if (headerParts[0].Equals(EslEventHeaderNames.ContentLength)) isEventBody = true;
            }
            else
            {
                if (rawLine.Length > 0 && EventBody != null) EventBody.Add(rawLine);
            }
    }
}