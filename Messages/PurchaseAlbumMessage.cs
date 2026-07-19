using CommunityToolkit.Mvvm.Messaging.Messages;
using MusicStore.ViewModels;

namespace MusicStore.Messages;
// 创建消息类型
public class PurchaseAlbumMessage : AsyncRequestMessage<AlbumViewModel?>;

