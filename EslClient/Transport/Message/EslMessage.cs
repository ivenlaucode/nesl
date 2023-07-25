namespace Nesl.EslClient.Transport.Message;

public class EslMessage
{
    private readonly List<string> _body = new();
    private readonly Dictionary<string, string> _headers = new();
    private int? _contentLength;

    public Dictionary<string, string> GetHeaders()
    {
        return _headers;
    }

    public bool HasHeader(string headerName)
    {
        return _headers.ContainsKey(headerName);
    }

    public string GetHeaderValue(string headerName)
    {
        return _headers[headerName];
    }

    public bool HasContentLength()
    {
        return _headers.ContainsKey(HeaderName.ContentLength);
    }

    public int? GetContentLength()
    {
        if (_contentLength != null) return _contentLength;
        if (HasContentLength()) _contentLength = Convert.ToInt32(_headers[HeaderName.ContentLength]);
        return _contentLength;
    }

    public string GetContentType()
    {
        if (_headers.ContainsKey(HeaderName.ContentType))
            return _headers[HeaderName.ContentType];
        return string.Empty;
    }

    public List<string> GetBodyLines()
    {
        return _body;
    }

    public void AddHeader(string name, string value)
    {
        _headers.TryAdd(name, value);
    }

    public void AddBodyLine(string line)
    {
        _body.Add(line);
    }

    public bool IsReplyOk()
    {
        return GetHeaderValue(HeaderName.ReplyText).Trim().Equals(HeaderValue.Ok);
    }
}