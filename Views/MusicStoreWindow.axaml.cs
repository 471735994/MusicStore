using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Messaging;
using MusicStore.Messages;

namespace MusicStore.Views;

/// <summary>
/// 主窗口，负责显示音乐商店界面并处理来自 ViewModel 的消息。
/// </summary>
public partial class MusicStoreWindow : Window
{
    public MusicStoreWindow()
    {
        InitializeComponent();

        // 当接收到 MusicStoreClosedMessage 时，关闭当前窗口并将选中的专辑对象传回。
        WeakReferenceMessenger.Default.Register<MusicStoreWindow, MusicStoreClosedMessage>(
            this,
            static (w, m) => w.Close(m.SelectedAlbum)
        );

        // 当接收到 NotificationMessage 时，显示一个警告通知。
        WeakReferenceMessenger.Default.Register<MusicStoreWindow, NotificationMessage>(
            this,
            static (w, m) =>
            {
                // 先关闭已有通知，避免重复显示。
                w.NotificationManager.CloseAll();

                // 显示新的通知消息，持续 3 秒。
                w.NotificationManager.Show(
                    m.Message,
                    NotificationType.Warning,
                    TimeSpan.FromSeconds(3)
                );
            }
        );
    }
}
