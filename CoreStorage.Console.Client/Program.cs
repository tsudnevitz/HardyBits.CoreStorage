using System;
using System.IO;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace CoreStorage.Console.Client
{
  public class Program
  {
    public static void Main(string[] args)
    {
      HttpClient client = new HttpClient();
      Uri.TryCreate("http://localhost:54159/uploads", UriKind.Absolute, out Uri uri);

      var file = new FileInfo(@"C:\foto.jpg");
      var ui = new UploadInfo
      {
        Filename = file.Name,
        Extension = file.Extension,
        FileSize = file.Length
      };

      var json = JsonConvert.SerializeObject(ui);
      var result = client.PutAsync(uri, new StringContent(json, Encoding.UTF8, "application/json")).Result;
      var stringResult = result.Content.ReadAsStringAsync().Result;
      var id = JsonConvert.DeserializeObject<Guid>(stringResult);

      using (var reader = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
      {
        Uri.TryCreate($"http://localhost:54159/uploads/{id}", UriKind.Absolute, out uri);

        int bytesRead;
        var buffer = new byte[1024];
        while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
        {
          var content = new byte[bytesRead];
          Array.Copy(buffer, content, bytesRead);

          client.PutAsync(uri, new ByteArrayContent(content));
        }
      }
      System.Console.ReadLine();
    }
  }
}