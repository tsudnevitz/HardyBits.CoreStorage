using System;
using System.Threading.Tasks;
using CoreStorage.Models;

namespace CoreStorage.Services
{
  public interface IUploadingFileCatalog
  {
    Task<BeginUploadState> BeginUploadAsync(string filename, string extension, int fileSize);
    Task<UploadState> ContunueUploadAsync(Guid uploadId, byte[] chunk);
  }
}