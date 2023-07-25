using System.Net;
using Nesl.EslClient.Internal;
using Nesl.EslClient.Transport;
using Nesl.EslClient.Transport.Event;
using Nesl.EslClient.Transport.Message;
using DotNetty.Codecs;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;

namespace Nesl.EslClient.Inbound;

public class FsClient
{
    private readonly List<IEslEventListener> _eventListeners = new();
    private Context? _context;

    public List<IEslEventListener> EventListeners => _eventListeners;

    public Context? Context { get => _context; set => _context = value; }


    public async Task Connect(string host, int port, string password)
    {
        var workerGroup = new MultithreadEventLoopGroup();
        var protocolListener = new ProtocolListener(EventListeners);
        var handler = new InboundClientHandler(password, protocolListener);
        var bootstrap = new Bootstrap()
            .Group(workerGroup)
            .Channel<TcpSocketChannel>()
            .Option(ChannelOption.SoKeepalive, true)
            .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
            {
                // now the inbound client logic
                channel.Pipeline.AddLast("decoder", new EslFrameDecoder(0));
                channel.Pipeline.AddLast("clientHandler", handler);
                channel.Pipeline.AddLast("encoder", new StringEncoder());
            }));
        var ipAddress = IPAddress.Parse(host);
        var channel = await bootstrap.ConnectAsync(ipAddress, port);
        Context = new Context(channel, handler);
        var authenticated = await Task.Run(() => protocolListener.IsAuthAuthenticated());
    }

    public void AddEventListener(IEslEventListener listener)
    {
        EventListeners.Add(listener);
    }

    public async Task<CommandResponse> SetEventSubscriptions(string format, string events)
    {
        if (Context != null) return await Context.SetEventSubscriptions(format, events);
        return new CommandResponse(string.Empty, null);
    }

    public async Task<CommandResponse> CancelEventSubscriptions()
    {
        if (Context != null) return await Context.CancelEventSubscriptions();
        return new CommandResponse(string.Empty, null);
    }

    public async Task<EslMessage?> SendApiCommand(string command, string arg)
    {
        if (Context != null) return await Context.SendApiCommand(command, arg);
        return new EslMessage();
    }

    public async Task<string> SendBackgroundApiCommand(string command, string arg)
    {
        if (Context != null) return await Context.SendBackgroundApiCommand(command, arg);
        return string.Empty;
    }

    public async Task<CommandResponse> AddEventFilter(string eventHeader, string valueToFilter)
    {
        if (Context != null) return await Context.AddEventFilter(eventHeader, valueToFilter);
        return new CommandResponse(string.Empty, null);
    }

    public async Task<CommandResponse> DeleteEventFilter(string eventHeader, string valueToFilter)
    {
        if (Context != null) return await Context.DeleteEventFilter(eventHeader, valueToFilter);
        return new CommandResponse(string.Empty, null);
    }

    public async Task<CommandResponse> SendMessage(SendMsg sendMsg)
    {
        if (Context != null) return await Context.SendMessage(sendMsg);
        return new CommandResponse(string.Empty, null);
    }
}

public class ProtocolListener : IEslProtocolListener
{
    private readonly CountdownEvent _countdown = new(1);
    private readonly List<IEslEventListener> _eventListeners;

    private bool _authenticated;

    public ProtocolListener(List<IEslEventListener> eventListeners)
    {
        _eventListeners = eventListeners;
    }

    public CountdownEvent Countdown => _countdown;

    public List<IEslEventListener> EventListeners => _eventListeners;

    public bool Authenticated { get => _authenticated; set => _authenticated = value; }

    public void AuthResponseReceived(CommandResponse response)
    {
        if (response.IsOk()) Authenticated = true;
        Countdown.Signal();
    }

    public void Disconnected()
    {
        
    }

    public void EventReceived(Context ctx, EslEvent e)
    {
        foreach (var listener in EventListeners) Task.Run(() => listener.OnEslEvent(ctx, e));
    }

    public bool IsAuthAuthenticated()
    {
        Countdown.Wait();
        return Authenticated;
    }

}