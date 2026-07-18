using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using MusicStore.Models;

namespace MusicStore.Services
{
    public class AlbumService
    {
        private static readonly HttpClient s_httpClient = new();
        private static readonly JsonSerializerOptions s_jsonOptions = new() { PropertyNameCaseInsensitive = true };
        private record ItunesAlbum(string ArtistName, string CollectionName, string ArtworkUrl100);
        private record ItunesSearchResult(ItunesAlbum[] Results);
        public async Task<IEnumerable<Album>> SearchAsync(string? searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return Enumerable.Empty<Album>();
            }
            var url = $"https://itunes.apple.com/search?term={Uri.EscapeDataString(searchTerm)}&entity=album&limit=25";
            var json = await s_httpClient.GetStringAsync(url).ConfigureAwait(false);
            var result = JsonSerializer.Deserialize<ItunesSearchResult>(json, s_jsonOptions);

            return result?.Results.Select(x => new Album(x.ArtistName, x.CollectionName, x.ArtworkUrl100.Replace("100x100bb", "600x600bb"))) ?? Enumerable.Empty<Album>();
        }

    }
}
