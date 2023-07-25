namespace Nesl.EslClient.Transport;

public static class HeaderParser
{
    public static string[] SplitHeader(string sb)
    {
        var length = sb.Length;
        var nameStart = FindNonWhitespace(sb, 0);
        int nameEnd;
        for (nameEnd = nameStart; nameEnd < length; nameEnd++)
        {
            var ch = sb[nameEnd];
            if (ch == ':' || ch == ' ') break;
        }

        int colonEnd;
        for (colonEnd = nameEnd; colonEnd < length; colonEnd++)
            if (sb[colonEnd] == ':')
            {
                colonEnd++;
                break;
            }

        var valueStart = FindNonWhitespace(sb, colonEnd);
        if (valueStart == length)
            return new[]
            {
                sb.Substring(nameStart, nameEnd - nameStart),
                ""
            };
        var valueEnd = FindEndOfString(sb);
        return new[]
        {
            sb.Substring(nameStart, nameEnd - nameStart),
            sb.Substring(valueStart, valueEnd - valueStart)
        };
    }

    private static int FindNonWhitespace(string sb, int offset)
    {
        int result;
        for (result = offset; result < sb.Length; result++)
            if (sb[result] != ' ')
                break;
        return result;
    }

    private static int FindEndOfString(string sb)
    {
        int result;
        for (result = sb.Length; result > 0; result--)
            if (sb[result - 1] != ' ')
                break;
        return result;
    }
}