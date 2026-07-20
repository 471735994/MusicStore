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
    // 内部领域模型引用，一旦构造后不可更改。
    // 通过此模型获取专辑的基本信息，并保持 ViewModel 和模型之间的一致性。
    private readonly Album _album;

    // AlbumService 的共享实例，用于加载和保存专辑数据及封面图像。
    // 使用静态字段可避免为每个 ViewModel 再次创建服务对象。
    private static readonly AlbumService s_albumService = new();

    /// <summary>
    /// 构造函数，接收一个 <see cref="Album"/> 实例并保留引用。
    /// </summary>
    public AlbumViewModel(Album album)
    {
        _album = album;
    }

    /// <summary>
    /// 专辑艺术家名称（来自内部模型）。
    /// </summary>
    public string Artist => _album.Artist;

    /// <summary>
    /// 专辑标题（来自内部模型）。
    /// </summary>
    public string Title => _album.Title;

    /// <summary>
    /// 异步封面属性。
    /// Avalonia 支持对 Task&lt;Bitmap?&gt; 的异步绑定，例如 XAML 中的 `Cover^` 语法。
    /// 每次访问时都会触发一次封面加载任务。
    /// </summary>
    public Task<Bitmap?> Cover => LoadCoverAsync();

    /// <summary>
    /// 异步加载封面位图。
    /// 1. 等待短暂延迟，提升 UI 体验；
    /// 2. 从 <see cref="AlbumService"/> 获取封面流；
    /// 3. 在后台线程中将流解码为指定宽度的 <see cref="Bitmap"/>；
    /// 4. 如果出现异常则返回 null。
    /// </summary>
    private async Task<Bitmap?> LoadCoverAsync()
    {
        try
        {
            // 轻微延迟有助于避免绑定时瞬间闪烁，并让 UI 有时间显示加载状态。
            await Task.Delay(200);

            // 从服务异步获取封面图像流。此流可能来自缓存、文件或网络下载。
            await using (var imageStream = await s_albumService.LoadCoverBitmapAsync(_album))
            {
                // 在线程池线程中解码位图，防止阻塞 UI 线程。
                return await Task.Run(() => Bitmap.DecodeToWidth(imageStream, 400));
            }
        }
        catch
        {
            // 任何加载/解码失败都统一返回 null，调用方可以显示默认占位图。
            return null;
        }
    }

    /// <summary>
    /// 判断两个 AlbumViewModel 是否相同，比较内部 Album 模型对象。
    /// </summary>
    public bool Equals(AlbumViewModel? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;

        // 仅当内部 Album 对象相等时，两个视图模型才视为相等。
        return _album.Equals(other._album);
    }

    public override bool Equals(object? obj) => Equals(obj as AlbumViewModel);

    public override int GetHashCode() => _album?.GetHashCode() ?? 0;

    /// <summary>
    /// 将专辑数据保存到磁盘，并随后保存对应的封面位图。
    /// </summary>
    public async Task SaveToDiskAsync()
    {
        // 先保存专辑模型数据（例如 metadata、库存信息等）。
        await s_albumService.SaveAsync(_album);

        // 重新加载封面以确保其可用，然后将位图保存到磁盘。
        if (await LoadCoverAsync() is { } cover)
        {
            await Task.Run(() =>
            {
                using (var fs = s_albumService.SaveCoverBitmapStream(_album))
                {
                    cover.Save(fs);
                }
            });
        }
    }
}
