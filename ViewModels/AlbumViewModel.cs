using System;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using MusicStore.Models;
using MusicStore.Services;

namespace MusicStore.ViewModels;

/// <summary>
/// 包装 <see cref="Album"/> 的视图模型（ViewModel）。
/// 提供给视图用于显示专辑的标题、艺术家及异步加载封面图像。
/// </summary>
public class AlbumViewModel : ViewModelBase, IEquatable<AlbumViewModel>
{
    // 持有的领域模型引用（不可变）
    private readonly Album _album;

    // 单例的服务用于从网络或缓存加载封面图片。
    private static AlbumService s_albumService = new();

    /// <summary>
    /// 构造函数，接收一个 <see cref="Album"/> 实例并保留引用。
    /// </summary>
    public AlbumViewModel(Album album)
    {
        _album = album;
    }

    /// <summary>
    /// 专辑艺术家名称（来自模型）。
    /// </summary>
    public string Artist => _album.Artist;

    /// <summary>
    /// 专辑标题（来自模型）。
    /// </summary>
    public string Title => _album.Title;

    /// <summary>
    /// 异步封面属性，返回一个正在进行的任务，该任务最终解析为 <see cref="Bitmap"/> 或 null。
    /// Avalonia 可以使用对 Task&lt;Bitmap?&gt; 的异步绑定（例如 XAML 中的 `Cover^` 语法）。
    /// </summary>
    public Task<Bitmap?> Cover => LoadCoverAsync();

    /// <summary>
    /// 异步加载封面位图：
    /// - 尝试从 <see cref="AlbumService"/> 获取封面流（可能来自缓存或网络）
    /// - 使用 <see cref="Bitmap.DecodeToWidth"/> 在后台线程解码为指定宽度的位图
    /// - 出现任何异常时返回 null
    /// </summary>
    private async Task<Bitmap?> LoadCoverAsync()
    {
        try
        {
            // 轻微延迟用于模拟或避免瞬时闪烁（UI 友好），并给绑定时间稳定下来
            await Task.Delay(200);

            // 从服务获取图片流（可能是 MemoryStream 或文件流）
            await using (var imageStream = await s_albumService.LoadCoverBitmapAsync(_album))
            {
                // 在线程池线程中进行位图解码，避免阻塞 UI 线程
                return await Task.Run(() => Bitmap.DecodeToWidth(imageStream, 400));
            }
        }
        catch
        {
            // 任意异常统一处理为 null，表明没有可用的封面图像
            return null;
        }
    }

    /// <summary>
    /// 基于内部模型判断是否相等。
    /// </summary>
    public bool Equals(AlbumViewModel? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;
        return _album.Equals(other._album);
    }

    public override bool Equals(object? obj) => Equals(obj as AlbumViewModel);

    public override int GetHashCode() => _album?.GetHashCode() ?? 0;
}
