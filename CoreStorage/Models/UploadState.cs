using CoreStorage.Models.Enums;

namespace CoreStorage.Models
{
  public class UploadState
  {
    public UploadState(UploadStatus status, string message)
    {
      Status = status;
      Message = message;
    }

    public UploadStatus Status { get; }
    public string Message { get; }
  }
}