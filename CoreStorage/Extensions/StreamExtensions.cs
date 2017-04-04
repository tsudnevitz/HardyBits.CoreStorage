using System.IO;
using System.Threading.Tasks;

namespace CoreStorage.Extensions
{
  public static class StreamExtensions
  {
    public static byte[] ReadAllBytes(this Stream input)
    {
      using (var ms = new MemoryStream())
      {
        input.CopyTo(ms);
        return ms.ToArray();
      }
    }

    public static async Task<byte[]> ReadAllBytesAsync(this Stream input)
    {
      using (var ms = new MemoryStream())
      {
        await input.CopyToAsync(ms);
        return ms.ToArray();
      }
    }
  }
}