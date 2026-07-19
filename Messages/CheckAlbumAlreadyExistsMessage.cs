using CommunityToolkit.Mvvm.Messaging.Messages;
using MusicStore.ViewModels;

namespace MusicStore.Messages;

public class CheckAlbumAlreadyExistsMessage : RequestMessage<bool>
{
    public AlbumViewModel Album { get; }

    public CheckAlbumAlreadyExistsMessage(AlbumViewModel album)
    {
        Album = album;
    }
}
