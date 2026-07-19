using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Input;
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

        private static string CachePath(Album album) => $"./Cache/{SanitizeFileName(album.Artist)} - {SanitizeFileName(album.Title)}";

        private static object SanitizeFileName(string input)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                input = input.Replace(c, '_');
            }
            return input;
        }

        public async Task<Stream> LoadCoverBitmapAsync(Album album)
        {
            var cachePath = CachePath(album);
            if (File.Exists(cachePath + ".bmp"))
            {
                return File.Open(cachePath + ".bmp", FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            else
            {
                var data = await s_httpClient.GetByteArrayAsync(album.CoverUrl);
                return new MemoryStream(data);
            }

        }

    }
}
