using System;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using CoreStorage.Extensions;
using CoreStorage.Models;
using CoreStorage.Models.Enums;
using CoreStorage.Services;

namespace CoreStorage.Controllers
{
  public class UploadController : Controller
  {
    private readonly IValidator<UploadInfo> _uploadValidator;
    private readonly IUploadingFileCatalog _catalog;

    public UploadController(IValidator<UploadInfo> uploadValidator, IUploadingFileCatalog catalog)
    {
      _uploadValidator = uploadValidator ?? throw new ArgumentNullException(nameof(uploadValidator));
      _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
    }

    [HttpPut]
    [Route("/uploads/")]
    public async Task<IActionResult> Put([FromBody]UploadInfo chunk)
    {
      var validationResult = _uploadValidator.Validate(chunk);
      if (!validationResult.IsValid)
        return BadRequest(string.Join(Environment.NewLine, validationResult.Errors));

      var state = await _catalog.BeginUploadAsync(chunk.Filename, chunk.Extension, chunk.FileSize);
      switch (state.Status)
      {
        case UploadStatus.Continue:
          return Accepted(state.UploadId);
        default:
          return BadRequest(state.Message);
      }
    }

    [HttpPut]
    [Route("/uploads/{uploadId}")]
    public async Task<IActionResult> Put(Guid uploadId)
    {
      var chunk = await Request.Body.ReadAllBytesAsync();
      if (chunk.Length == 0)
        return BadRequest("No data supplied");

      var state = await _catalog.ContunueUploadAsync(uploadId, chunk);
      switch (state.Status)
      {
        case UploadStatus.Continue:
          return NoContent();
        case UploadStatus.Finished:
          return Ok();
        default:
          return BadRequest(state.Message);
      }
    }
  }
}