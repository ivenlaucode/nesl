using Nesl.EslClient.Internal;
using Nesl.EslClient.Transport.Event;

namespace Nesl.EslClient.Inbound;

public interface IEslEventListener
{
    public void OnEslEvent(Context ctx, EslEvent e);
}