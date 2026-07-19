using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MusicStore.Messages;
using MusicStore.Services;

namespace MusicStore.ViewModels;

public partial class MusicStoreViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string? SearchText { get; set; }

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(BuyMusicCommand))]
    public partial AlbumViewModel? SelectedAlbum { get; set; }
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

    [RelayCommand(CanExecute = nameof(CanBuyMusic))]
    private void BuyMusic()
    {
        if (SelectedAlbum != null)
        {
            var album_exists = WeakReferenceMessenger.Default.Send(
                new CheckAlbumAlreadyExistsMessage(SelectedAlbum)
            );
            if (album_exists)
            {
                WeakReferenceMessenger.Default.Send(
                    new NotificationMessage("This album was already added")
                );
            }
            else
            {
                WeakReferenceMessenger.Default.Send(new MusicStoreClosedMessage(SelectedAlbum));
            }
        }
    }

    private bool CanBuyMusic() => SelectedAlbum is not null;
}
