using System;
using CoreStorage.Models.Enums;

namespace CoreStorage.Models
{
  public class BeginUploadState : UploadState
  {
    public BeginUploadState(UploadStatus status, string message) 
      : base(status, message)
    {
      UploadId = Guid.NewGuid();
    }

    public Guid UploadId { get; }
  }
}