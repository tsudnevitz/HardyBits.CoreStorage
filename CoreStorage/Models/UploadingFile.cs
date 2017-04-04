using System;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace CoreStorage.Models
{
  public class UploadingFile : IDisposable
  {
    private readonly ISubject<byte[]> _observable = new Subject<byte[]>();
    private readonly ISubject<UploadingFile> _timeoutSubject = new Subject<UploadingFile>();

    private readonly string _filename;
    private readonly string _extension;
    private readonly FileStream _fileStream;

    public UploadingFile(string filename, string extension, int fileSize)
    {
      _filename = filename;
      _extension = extension;
      FileSize = fileSize;

      _observable.Timeout(TimeSpan.FromSeconds(5))
        .Subscribe(WriteToStream, RiseTimeout);

      _fileStream = new FileStream($@"C:\Uploads\{filename}.{extension}", FileMode.Create);
    }

    public int UploadedBytes { get; private set; }
    public int FileSize { get; }
    public int BytesLeft => FileSize - UploadedBytes;
    public IObservable<UploadingFile> OnTimeout => _timeoutSubject;

    public Task<int> AppendChunkAsync(byte[] chunk)
    {
      if(chunk.Length > BytesLeft)
        throw new ArgumentException("More uploaded bytes than declared.");

      _observable.OnNext(chunk);
      UploadedBytes += chunk.Length;
      return Task.FromResult(BytesLeft);
    }

    private void RiseTimeout(Exception ex)
    {
      _timeoutSubject.OnNext(this);
    }

    private void WriteToStream(byte[] chunk)
    {
      _fileStream.Write(chunk, 0, chunk.Length);
      _fileStream.Flush(true);
    }

    public void Dispose()
    {
      _observable.OnCompleted();
      _timeoutSubject.OnCompleted();
      _fileStream.Dispose();
    }

    public override string ToString()
    {
      return $"{_filename}.{_extension} ({FileSize} bytes)";
    }
  }
}