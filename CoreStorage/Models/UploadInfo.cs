using Newtonsoft.Json;

namespace CoreStorage.Models
{
  public class UploadInfo
  {
    [JsonProperty("filename")]
    public string Filename { get; set; }
    [JsonProperty("extension")]
    public string Extension { get; set; }
    [JsonProperty("file-size")]
    public int FileSize { get; set; }
  }
}