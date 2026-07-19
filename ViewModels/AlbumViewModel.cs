using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using MusicStore.Models;
using MusicStore.Services;

namespace MusicStore.ViewModels;

public class AlbumViewModel : ViewModelBase
{
    private readonly Album _album;
    public AlbumViewModel(Album album)
    {
        _album = album;
    }
    public string Artist => _album.Artist;
    public string Title => _album.Title;
    private static AlbumService s_albumService = new();
    public Task<Bitmap?> Cover => LoadCoverAsync();
    private async Task<Bitmap?> LoadCoverAsync()
    {
        try
        {
            await Task.Delay(200);

            await using (var imageStream = await s_albumService.LoadCoverBitmapAsync(_album))
            {
                return await Task.Run(() => Bitmap.DecodeToWidth(imageStream, 400));
            }
        }
        catch
        {
            return null;
        }
    }
}