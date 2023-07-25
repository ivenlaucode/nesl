using System.Text;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;

namespace Nesl.EslClient.Transport.Message;

public class EslFrameDecoder : ReplayingDecoder<int>
{
    private static readonly byte Lf = 10;

    private readonly int _maxHeaderSize = 8192;

    private readonly int _state;
    private readonly bool _treatUnknownHeadersAsBody = false;
    private EslMessage? _currentMessage;

    public EslFrameDecoder(int state) : base(state)
    {
        _state = state;
    }

    protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
    {
        switch (State)
        {
            case 0:
                _currentMessage ??= new EslMessage();

                var reachedDoubleLf = false;
                while (!reachedDoubleLf)
                {
                    // this will read or fail
                    var headerLine = ReadToLineFeedOrFail(input, _maxHeaderSize);
                    if (!string.IsNullOrEmpty(headerLine))
                    {
                        // split the header line
                        var headerParts = HeaderParser.SplitHeader(headerLine);
                        var headerName = headerParts[0];
                        if (string.IsNullOrEmpty(headerName))
                        {
                            if (_treatUnknownHeadersAsBody)
                                // cache this 'header' as a body line <-- useful for Outbound client mode
                                _currentMessage.AddBodyLine(headerLine);
                            else
                                // throw new Exception("Unhandled ESL header [" + headerParts[0] + ']');
                                Console.WriteLine("Unhandled ESL header [" + headerParts[0] + ']');
                            _currentMessage.AddHeader("", headerParts[1]);
                        }
                        else
                        {
                            _currentMessage.AddHeader(headerName, headerParts[1]);
                        }
                    }
                    else
                    {
                        reachedDoubleLf = true;
                    }

                    // do not read in this line again
                    Checkpoint();
                }

                // have read all headers - check for content-length
                if (_currentMessage.HasContentLength())
                {
                    Checkpoint(1);
                    break;
                }

                // end of message
                Checkpoint(0);
                // send message upstream
                var dm = _currentMessage;
                _currentMessage = null;

                output.Add(dm);
                break;

            case 1:
                int? contentLength = null;
                if (_currentMessage != null) contentLength = _currentMessage.GetContentLength();
                IByteBuffer? bodyBytes = null;
                if (contentLength != null && contentLength > 0)
                {
                    if (input.ReadableBytes < contentLength)
                        bodyBytes = input.ReadBytes(input.ReadableBytes);
                    else
                        bodyBytes = input.ReadBytes(contentLength.Value);
                }

                // most bodies are line based, so split on LF
                while (bodyBytes != null && bodyBytes.IsReadable())
                    if (contentLength != null)
                    {
                        var bodyLine = ReadLine(bodyBytes, contentLength.Value);
                        if (_currentMessage != null) _currentMessage.AddBodyLine(bodyLine);
                    }

                // end of message
                Checkpoint(0);
                // send message upstream
                var decodedMessage = _currentMessage;
                _currentMessage = null;
                if (decodedMessage != null) output.Add(decodedMessage);
                break;

            default:
                throw new Exception("Illegal state: [" + _state + ']');
        }
    }

    private string ReadToLineFeedOrFail(IByteBuffer buffer, int maxLineLegth)
    {
        var sb = new StringBuilder(64);
        while (true)
            try
            {
                // this read might fail
                var nextByte = buffer.ReadByte();
                if (nextByte == Lf) return sb.ToString();

                // Abort decoding if the decoded line is too large.
                if (sb.Length >= maxLineLegth)
                    throw new TooLongFrameException(
                        "ESL header line is longer than " + maxLineLegth + " bytes.");
                sb.Append((char)nextByte);
            }
            catch (Exception e)
            {
                Console.WriteLine("ReadToLineFeedFail exec fail, message: " + e.Message);
                return string.Empty;
            }
    }

    private string ReadLine(IByteBuffer buffer, int maxLineLength)
    {
        var sb = new StringBuilder(64);
        while (buffer.IsReadable())
        {
            // this read should always succeed
            var nextByte = buffer.ReadByte();
            if (nextByte == Lf) return sb.ToString();

            // Abort decoding if the decoded line is too large.
            if (sb.Length >= maxLineLength)
                throw new TooLongFrameException(
                    "ESL message line is longer than " + maxLineLength + " bytes.");
            sb.Append((char)nextByte);
        }

        return sb.ToString();
    }
}