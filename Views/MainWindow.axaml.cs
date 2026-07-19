using Avalonia.Controls;
using CommunityToolkit.Mvvm.Messaging;
using MusicStore.Messages;
using MusicStore.ViewModels;

namespace MusicStore.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        if (Design.IsDesignMode)
        {
            return;
        }
        //每当调用“Send(new PurchaseAlbumMessage())”时，在 MainWindow 实例上调用此回调。
        WeakReferenceMessenger.Default.Register<MainWindow, PurchaseAlbumMessage>(
            this,
            static (w, m) =>
            {
                //创建 MusicStoreWindow 实例，并将 MusicStoreViewModel 设置为其 DataContext。
                var dialog = new MusicStoreWindow { DataContext = new MusicStoreViewModel() };
                // 显示对话框窗口，并在对话框关闭时回复返回的 AlbumViewModel 或 null。
                m.Reply(dialog.ShowDialog<AlbumViewModel?>(w));
            }
        );
    }
}
