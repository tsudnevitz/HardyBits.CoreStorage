using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CoreStorage.Models.Enums;
using CoreStorage.Models;

namespace CoreStorage.Services
{
  public class UploadingFileCatalog : IUploadingFileCatalog
  {
    private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
    private readonly Dictionary<Guid, UploadingFile> _uploads = new Dictionary<Guid, UploadingFile>();

    public async Task<BeginUploadState> BeginUploadAsync(string filename, string extension, int fileSize)
    {
      try
      {
        _lock.EnterWriteLock();

        var file = new UploadingFile(filename, extension, fileSize);
        var state = new BeginUploadState(UploadStatus.Continue,
          $"File '{file}' upload started. Total bytes left: {file.BytesLeft}.");

        _uploads.Add(state.UploadId, file);
        //file.OnTimeout.Subscribe(uf => RemoveFileOnTimeout(uf, state.UploadId));

        return state;
      }
      catch (Exception ex)
      {
        return new BeginUploadState(UploadStatus.Error, ex.Message);
      }
      finally
      {
        _lock.ExitWriteLock();
      }
    }

    private void RemoveFileOnTimeout(UploadingFile file, Guid uploadId)
    {
      try
      {
        _lock.EnterWriteLock();
        _uploads.Remove(uploadId);
        file.Dispose();
      }
      catch (Exception)
      {
        // logging
      }
      finally
      {
        _lock.ExitWriteLock();
      }
    }

    public async Task<UploadState> ContunueUploadAsync(Guid uploadId, byte[] chunk)
    {
      try
      {
        _lock.EnterUpgradeableReadLock();
        if (!_uploads.TryGetValue(uploadId, out UploadingFile file))
          return new UploadState(UploadStatus.Error, $"Upload {uploadId} not found.");

        if (await file.AppendChunkAsync(chunk) != 0)
          return new UploadState(UploadStatus.Continue, $" File '{file}'. Bytes left: {file.BytesLeft}.");

        try
        {
          _lock.EnterWriteLock();
          _uploads.Remove(uploadId);
        }
        finally
        {
          _lock.ExitWriteLock();
        }

        file.Dispose();
        return new UploadState(UploadStatus.Finished,
          $"File '{file}' uploaded. Total bytes: {file.FileSize}.");
      }
      catch (Exception ex)
      {
        return new UploadState(UploadStatus.Error, ex.Message);
      }
      finally
      {
        _lock.ExitUpgradeableReadLock();
      }
    }
  }
}