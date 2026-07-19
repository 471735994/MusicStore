using Tmds.DBus.Protocol;

namespace MusicStore.Messages;

public class NotificationMessage
{
    public string Message { get; }

    public NotificationMessage(string message)
    {
        Message = message;
    }
}
