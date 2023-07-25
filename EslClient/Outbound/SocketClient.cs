using DotNetty.Codecs;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Nesl.EslClient.Transport.Message;
using System.Net;

namespace Nesl.EslClient.Outbound;

public class SocketClient
{
    private readonly AbstractOutboundClientHandler _handler;
    private readonly int _port = 8989;

    public SocketClient(AbstractOutboundClientHandler handler, int port)
    {
        _port = port;
        _handler = handler;
    }

    public AbstractOutboundClientHandler Handler => _handler;

    public int Port => _port;

    public async void Start()
    {
        IEventLoopGroup bossGroup = new MultithreadEventLoopGroup();
        var workerGroup = new MultithreadEventLoopGroup();
        var bootstrap = new ServerBootstrap()
            .Group(bossGroup, workerGroup)
            .Channel<TcpServerSocketChannel>()
            .Option(ChannelOption.SoBacklog, 8192)
            .ChildOption(ChannelOption.SoKeepalive, true)
            .ChildOption(ChannelOption.TcpNodelay, true)
            .ChildOption(ChannelOption.SoReuseport, true)
            .ChildHandler(new ActionChannelInitializer<ISocketChannel>(channel =>
            {
                // now the outbound client logic
                channel.Pipeline.AddLast("decoder", new EslFrameDecoder(0));
                channel.Pipeline.AddLast("clientHandler", Handler);
                channel.Pipeline.AddLast("encoder", new StringEncoder());
            }));
        var serverChannel = await bootstrap.BindAsync(IPAddress.Loopback, Port);
    }
}