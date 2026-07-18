using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using MusicStore.Services;

namespace MusicStore.ViewModels;

public partial class MusicStoreViewModel : ViewModelBase
{
    [ObservableProperty] public partial string? SearchText { get; set; }
    [ObservableProperty] public partial bool IsBusy { get; set; }
    [ObservableProperty] public partial AlbumViewModel? SelectedAlbum { get; set; }
    public ObservableCollection<AlbumViewModel> SearchResults { get; } = new();
    private static readonly AlbumService s_albumService = new();
    partial void OnSearchTextChanged(string? oldValue, string? newValue)
    {
        _ = DoSearch(SearchText);
    }
    private async Task DoSearch(string? term)
    {
        IsBusy = true;
        SearchResults.Clear();

        var albums = await s_albumService.SearchAsync(term);
        foreach (var album in albums)
        {
            var vm = new AlbumViewModel(album);
            SearchResults.Add(vm);
        }
        IsBusy = false;
    }



}