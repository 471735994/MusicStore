using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace MusicStore.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [RelayCommand]
    private async Task AddAlbumAsync()
    {
        // 将消息发送给之前注册的处理程序并等待选定的 Album
        var album = await WeakReferenceMessenger.Default.Send(new PurchaseAlbumMessage());
    }
}
