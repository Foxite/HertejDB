using HertejDB.Common;
using HertejDB.Server.Data;
using HertejDB.Server.Services;
using HertejDB.Server.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HertejDB.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class ImageController : ControllerBase {
	private readonly FileStorage m_FileStorage;
	private readonly ImageService m_Service;

	public ImageController(FileStorage fileStorage, ImageService service) {
		m_FileStorage = fileStorage;
		m_Service = service;
	}

	private async Task<IActionResult> LambdaOrNotFound(Task<Image?> imageTask, Func<Image, IActionResult> selector) {
		Image? image = await imageTask;
		if (image != null) {
			return selector(image);
		} else {
			return NotFound();
		}
	}

	[HttpGet("categories")]
	public Task<string[]> GetCategories() {
		return m_Service.GetCategories();
	}

	[HttpGet("random")]
	public Task<IActionResult> GetRandomImage([FromQuery] string category) {
		return LambdaOrNotFound(m_Service.GetRandomImageAsync(category), Ok);
	}

	[HttpGet]
	public async Task<IActionResult> GetImages([FromQuery] string category) {
		return Ok((await m_Service.GetImages(category)).Select(GetGetImageDto));
	}

	[HttpGet("{id:long}")]
	public Task<IActionResult> GetImage([FromRoute] long id) {
		return LambdaOrNotFound(m_Service.GetImageById(id), image => Ok(GetGetImageDto(image)));
	}

	[HttpGet("{id:long}/download")]
	public Task<IActionResult> DownloadImage([FromRoute] long id) {
		return LambdaOrNotFound(m_Service.GetImageById(id), image => m_FileStorage.Get(image));
	}

	[HttpDelete("{id:long}")]
	public async Task<IActionResult> DeleteImage([FromRoute] long id) {
		if (await m_Service.DeleteImage(id)) {
			return Ok();
		} else {
			return NotFound();
		}
	}

	[HttpPost]
	[Authorize("Upload")]
	public async Task<IActionResult> UploadImage([FromForm] string category, IFormFile file) {
		Image image = await m_Service.StoreNewImage(category, file.OpenReadStream(), file.ContentType, null);

		return CreatedAtAction(nameof(GetImage), new { id = image.Id }, null);
	}

	private GetImageDto GetGetImageDto(Image image) {
		return new GetImageDto() {
			Id = image.Id,
			Category = image.Category,
			MimeType = image.MimeType,
			Added = image.Added,
			Attribution = image.SourceAttribution,
		};
	}
}
