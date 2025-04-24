using Microsoft.Extensions.Configuration;
using Minio;
using Minio.DataModel.Args;
using MusicWebAPI.Domain.Interfaces.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MusicWebAPI.Infrastructure.FileService
{
    public class FileStorageService
    {
        private readonly IMinioClient _minioClient;
        private readonly string _bucketName;
        private readonly string _fileServerAddress;

        public FileStorageService(IMinioClient minioClient, IConfiguration configuration)
        {
            _minioClient = minioClient;
            _bucketName = configuration["MINIO:BUCKET_NAME"] ?? "music-bucket";
            _fileServerAddress = configuration["MINIO:FILE_SERVER_ADDRESS"] ?? "http://localhost:9011/";
        }

        public async Task<string> UploadFile(string objectId, string base64File)
        {
            await EnsureBucketExistsAsync();

            var objectExists = await CheckFileExistence(objectId);
            if (objectExists)
            {
                await DeleteFile(objectId);
            }

            if (string.IsNullOrWhiteSpace(base64File))
            {
                throw new ArgumentException("Base64 file string is null or empty.");
            }

            var fileBytes = Convert.FromBase64String(base64File);
            var stream = new MemoryStream(fileBytes);

            var putObjectArgs = new PutObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectId)
                .WithStreamData(stream)
                .WithContentType("audio/mpeg")
                .WithObjectSize(fileBytes.Length);

            await _minioClient.PutObjectAsync(putObjectArgs);

            var fileUrl = $"{_fileServerAddress.TrimEnd('/')}/{_bucketName}/{objectId}";

            return fileUrl;
        }

        public async Task DeleteFile(string objectId)
        {
            var removeObjectArgs = new RemoveObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectId);

            await _minioClient.RemoveObjectAsync(removeObjectArgs);
        }

        private async Task<bool> CheckFileExistence(string objectId)
        {
            try
            {
                var objectExistenceArgs = new StatObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(objectId);

                await _minioClient.StatObjectAsync(objectExistenceArgs);

                return true;
            }
            catch
            {
                return false;
            }
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
    }
}
