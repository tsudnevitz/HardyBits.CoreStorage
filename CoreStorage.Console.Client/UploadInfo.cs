using Newtonsoft.Json;

namespace CoreStorage.Console.Client
{
  public class UploadInfo
  {
    [JsonProperty("filename")]
    public string Filename { get; set; }
    [JsonProperty("extension")]
    public string Extension { get; set; }
    [JsonProperty("file-size")]
    public long FileSize { get; set; }
  }
}