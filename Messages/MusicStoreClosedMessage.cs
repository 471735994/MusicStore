using MusicStore.ViewModels;

namespace MusicStore.Messages;

public class MusicStoreClosedMessage(AlbumViewModel selectedAlbum)
{
    public AlbumViewModel SelectedAlbum { get; } = selectedAlbum;
}
