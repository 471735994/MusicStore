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
        // 单例 HttpClient，用于发起网络请求，避免频繁创建和销毁导致端口耗尽
        private static readonly HttpClient s_httpClient = new();

        // Json 序列化/反序列化选项，忽略属性名大小写
        private static readonly JsonSerializerOptions s_jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        // 内部类型，用于绑定 iTunes 搜索结果的 JSON 结构
        private record ItunesAlbum(string ArtistName, string CollectionName, string ArtworkUrl100);

        // 内部类型，表示 iTunes API 返回的搜索结果集合
        private record ItunesSearchResult(ItunesAlbum[] Results);

        /// <summary>
        /// 异步搜索专辑，使用 iTunes Search API
        /// </summary>
        public async Task<IEnumerable<Album>> SearchAsync(string? searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                // 如果搜索词为空，则直接返回空集合
                return Enumerable.Empty<Album>();
            }

            var url =
                $"https://itunes.apple.com/search?term={Uri.EscapeDataString(searchTerm)}&entity=album&limit=25";

            // 从网络获取 JSON 数据
            var json = await s_httpClient.GetStringAsync(url).ConfigureAwait(false);
            var result = JsonSerializer.Deserialize<ItunesSearchResult>(json, s_jsonOptions);

            // 将 iTunes API 的结果映射为本地 Album 模型
            return result?.Results.Select(x => new Album(
                    x.ArtistName,
                    x.CollectionName,
                    x.ArtworkUrl100.Replace("100x100bb", "600x600bb")
                )) ?? Enumerable.Empty<Album>();
        }

        // 生成缓存文件路径，使用艺术家名和专辑标题作为文件名
        private static string CachePath(Album album) =>
            $"./Cache/{SanitizeFileName(album.Artist)} - {SanitizeFileName(album.Title)}";

        // 处理文件名非法字符，将其替换为下划线
        private static string SanitizeFileName(string input)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                input = input.Replace(c, '_');
            }
            return input;
        }

        /// <summary>
        /// 加载专辑封面图像的字节流，如果存在缓存则直接读取缓存，否则从网络下载
        /// </summary>
        public async Task<Stream> LoadCoverBitmapAsync(Album album)
        {
            var cachePath = CachePath(album);
            if (File.Exists(cachePath + ".bmp"))
            {
                // 如果缓存中已经存在 BMP 文件，直接从本地打开并返回
                return File.Open(
                    cachePath + ".bmp",
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read
                );
            }
            else
            {
                // 否则从网络下载图片数据并返回内存流
                var data = await s_httpClient.GetByteArrayAsync(album.CoverUrl);
                return new MemoryStream(data);
            }
        }

        #region 保存数据

        /// <summary>
        /// 将 Album 数据保存到本地 Cache 目录中的 JSON 文件
        /// </summary>
        public async Task SaveAsync(Album album)
        {
            if (!Directory.Exists("./Cache"))
            {
                Directory.CreateDirectory("./Cache");
            }
            using var fs = File.OpenWrite(CachePath(album) + ".json");
            await SaveToStreamAsync(album, fs);
        }

        /// <summary>
        /// 返回一个可写流，用于将封面 BMP 保存到本地缓存
        /// </summary>
        public Stream SaveCoverBitmapStream(Album album)
        {
            return File.OpenWrite(CachePath(album) + ".bmp");
        }

        /// <summary>
        /// 将 Album 对象序列化为 JSON，并写入指定的流中
        /// </summary>
        /// <param name="data">要保存的 Album 对象</param>
        /// <param name="stream">目标输出流（例如文件流）</param>
        private static async Task SaveToStreamAsync(Album data, Stream stream)
        {
            // JsonSerializer.SerializeAsync 会把对象序列化为 JSON 并异步写入流
            await JsonSerializer.SerializeAsync(stream, data).ConfigureAwait(false);
        }

        #endregion

        #region 加载数据

        /// <summary>
        /// 加载本地缓存中的所有 Album 数据（JSON 文件）
        /// </summary>
        public async Task<IEnumerable<Album>> LoadCachedAsync()
        {
            if (!Directory.Exists("./Cache"))
            {
                Directory.CreateDirectory("./Cache");
            }

            var results = new List<Album>();
            foreach (var file in Directory.EnumerateFiles("./Cache"))
            {
                if ((new DirectoryInfo(file).Extension) != ".json")
                    continue;

                await using var fs = File.Open(
                    file,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read
                );
                results.Add(await LoadFromStreamAsync(fs).ConfigureAwait(false));
            }
            return results;
        }

        private static async Task<Album> LoadFromStreamAsync(Stream stream)
        {
            return (await JsonSerializer.DeserializeAsync<Album>(stream).ConfigureAwait(false))!;
        }

        #endregion
    }
}
