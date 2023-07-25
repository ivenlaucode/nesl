using Nesl.EslClient.Internal;
using Nesl.EslClient.Transport;
using Nesl.EslClient.Transport.Event;

namespace Nesl.EslClient.Inbound;

public interface IEslProtocolListener
{
    void AuthResponseReceived(CommandResponse response);

    void EventReceived(Context ctx, EslEvent e);

    void Disconnected();
}