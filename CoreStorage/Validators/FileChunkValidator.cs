using FluentValidation;
using CoreStorage.Models;

namespace CoreStorage.Validators
{
  public class FileChunkValidator : AbstractValidator<UploadInfo>
  {
    public FileChunkValidator()
    {
      RuleFor(chunk => chunk.Filename).NotNull();
      RuleFor(chunk => chunk.Extension).NotNull();
      RuleFor(chunk => chunk.FileSize).GreaterThan(0);
    }
  }
}