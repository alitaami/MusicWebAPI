using Microsoft.Extensions.Configuration;
using Minio;
using Minio.ApiEndpoints;
using Minio.DataModel.Args;
using Minio.Exceptions;
using MusicWebAPI.Core.Utilities;
using MusicWebAPI.Domain.External.FileService;
using MusicWebAPI.Domain.Interfaces.Services;
using System;
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace MusicWebAPI.Infrastructure.FileService
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IMinioClient _minioClient;
        private readonly string _bucketName;
        private readonly string _fileServerDownloadUrl;
        private readonly string _fileStreamUrl;

        public FileStorageService(IMinioClient minioClient, IConfiguration configuration)
        {
            _minioClient = minioClient;
            _bucketName = Environment.GetEnvironmentVariable("MINIO_BUCKET_NAME");
            _fileServerDownloadUrl = Environment.GetEnvironmentVariable("FILE_SERVER_DOWNLOAD_URL");
            _fileStreamUrl = Environment.GetEnvironmentVariable("FILE_STREAM_URL");
        }

        public async Task<FileStreamResult?> GetFileStream(string objectId, string rangeHeader)
        {
            try
            {
                var statArgs = new StatObjectArgs()
                                .WithBucket(_bucketName)
                                .WithObject(objectId);

                var stat = await _minioClient.StatObjectAsync(statArgs);
                var fileSize = stat.Size;

                long start = 0;
                long end = fileSize - 1;

                bool isPartial = false;
                string? contentRange = null;

                if (!string.IsNullOrEmpty(rangeHeader) && rangeHeader.StartsWith("bytes="))
                {
                    var range = rangeHeader.Replace("bytes=", "").Split('-');

                    if (long.TryParse(range[0], out var parsedStart))
                        start = parsedStart;

                    if (range.Length > 1 && long.TryParse(range[1], out var parsedEnd))
                        end = parsedEnd;

                    if (end >= fileSize) end = fileSize - 1;

                    if (start > end)
                        throw new ArgumentOutOfRangeException(nameof(rangeHeader), "Invalid range");

                    isPartial = true;
                    contentRange = $"bytes {start}-{end}/{fileSize}";
                }

                var objectUrl = $"{_fileStreamUrl.TrimEnd('/')}/{_bucketName}/{objectId}";

                using var httpClient = new HttpClient();

                var request = new HttpRequestMessage(HttpMethod.Get, objectUrl);

                if (isPartial)
                {
                    request.Headers.Range = new RangeHeaderValue(start, end);
                }

                var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var stream = await response.Content.ReadAsStreamAsync();

                return new FileStreamResult
                {
                    Stream = stream,
                    IsPartial = isPartial,
                    ContentRange = contentRange,
                    ContentType = /*response.Content.Headers.ContentType?.ToString() ??*/ "audio/mpeg"
                };
            }
            catch (ObjectNotFoundException)
            {
                // Return null to indicate file not found instead of throwing
                return null;
            }
        }

        public async Task<string> UploadFile(string objectId, string base64File)
        {
            await EnsureBucketExistsAsync();

            var objectExists = await CheckFileExistence(objectId);
            if (objectExists)
            {
                await DeleteFile(objectId);
            }

            // Clean base64 if needed
            if (base64File.Contains(","))
                base64File = base64File.Substring(base64File.IndexOf(",") + 1);

            var fileBytes = Convert.FromBase64String(base64File);
            using var stream = new MemoryStream(fileBytes);

            var putObjectArgs = new PutObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectId)
                .WithStreamData(stream)
                .WithObjectSize(fileBytes.Length);

            await _minioClient.PutObjectAsync(putObjectArgs);

            var fileUrl = $"{_fileServerDownloadUrl.TrimEnd('/')}/{_bucketName}/{objectId}";

            return fileUrl;
        }

        public async Task DeleteFile(string url)
        {
            string objectId;
            try
            {
                var uri = new Uri(url);
                objectId = uri.Segments.Last();
            }
            catch (UriFormatException)
            {
                // url is not a full URI, so treat it as a file path or filename
                objectId = Path.GetFileName(url);
            }

            // Check object existence
            var objectExists = await CheckFileExistence(objectId);

            // Delete object if exists
            if (objectExists)
            {
                var removeObjectArgs = new RemoveObjectArgs()
                                    .WithBucket(_bucketName)
                                    .WithObject(objectId);

                await _minioClient.RemoveObjectAsync(removeObjectArgs);
            }
        }

        private async Task<bool> CheckFileExistence(string objectId)
        {
            try
            {
                // Check file existence
                // throws an error if the file does not exists
                var objectExistenceArgs = new StatObjectArgs()
                                    .WithBucket(_bucketName)
                                    .WithObject(objectId);

                _ = await _minioClient.StatObjectAsync(objectExistenceArgs);

                return true;
            }
            catch (Exception)
            {
            }

            return false;
        }
        private async Task EnsureBucketExistsAsync()
        {
            var bucketExistsArgs = new BucketExistsArgs().WithBucket(_bucketName);
            bool bucketExists = await _minioClient.BucketExistsAsync(bucketExistsArgs);

            if (!bucketExists)
            {
                var makeBucketArgs = new MakeBucketArgs().WithBucket(_bucketName);
                await _minioClient.MakeBucketAsync(makeBucketArgs);
            }
        }

        private async Task ListBucketObjectsAsync()
        {
            var listObjectsArgs = new ListObjectsArgs()
                .WithBucket(_bucketName)
                .WithRecursive(true);

            var observable = _minioClient.ListObjectsAsync(listObjectsArgs);

            var tcs = new TaskCompletionSource<bool>();

            var subscription = observable.Subscribe(
                ex =>
                {
                    tcs.SetResult(false);
                },
                () =>
                {
                    tcs.SetResult(true);
                });

            // Wait for the listing to complete
            await tcs.Task;

            // Dispose subscription
            subscription.Dispose();
        }
    }
}
