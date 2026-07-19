using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MusicStore.Messages;
using MusicStore.Services;

namespace MusicStore.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private static readonly AlbumService s_albumService = new();
    public ObservableCollection<AlbumViewModel> Albums { get; } = new();

    public MainWindowViewModel()
    {
        WeakReferenceMessenger.Default.Register<CheckAlbumAlreadyExistsMessage>(
            this,
            (v, m) =>
            {
                m.Reply(Albums.Contains(m.Album));
            }
        );
    }

    [RelayCommand]
    private async Task AddAlbumAsync()
    {
        // 将消息发送给之前注册的处理程序并等待选定的 Album
        var album = await WeakReferenceMessenger.Default.Send(new PurchaseAlbumMessage());
        if (album != null)
        {
            Albums.Add(album);
        }
    }
}
