
using MusicWebAPI.Core.Utilities;

namespace MusicWebAPI.Domain.External.FileService
{
    public interface IFileStorageService
    {
        /// <summary>
        /// Uploads a file to object storage using a base64-encoded string.
        /// </summary>
        /// <param name="objectId">The unique identifier for the object.</param>
        /// <param name="base64File">The base64-encoded file content.</param>
        /// <returns>The public URL to access the uploaded file.</returns>
        Task<string> UploadFile(string objectId, string base64File);

        /// <summary>
        /// Deletes a file from the object storage using its URL or object ID.
        /// </summary>
        /// <param name="url">The URL or object ID of the file to delete.</param>
        Task DeleteFile(string url, string bucketName);

        /// <summary>
        /// Retrieves a file stream from the object storage.
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="rangeHeader"></param>
        /// <returns></returns>
        Task<FileStreamResult> GetFileStream(string objectId, string rangeHeader);
    }
}