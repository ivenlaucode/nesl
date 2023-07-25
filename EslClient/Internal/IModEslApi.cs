using Nesl.EslClient.Transport;
using Nesl.EslClient.Transport.Message;

namespace Nesl.EslClient.Internal;

public interface IModEslApi
{
    bool CanSend();

    Task<EslMessage?> SendApiCommand(string command, string arg);

    Task<string> SendBackgroundApiCommand(string command, string arg);

    Task<CommandResponse> SetEventSubscriptions(string format, string events);

    Task<CommandResponse> CancelEventSubscriptions();

    Task<CommandResponse> AddEventFilter(string eventHeader, string valueToFilter);

    Task<CommandResponse> DeleteEventFilter(string eventHeader, string valueToFilter);

    Task<CommandResponse> SendMessage(SendMsg sendMsg);
}