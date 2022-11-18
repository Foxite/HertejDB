using HertejDB.Common;
using HertejDB.Server.Data;
using HertejDB.Server.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HertejDB.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class ImageController : ControllerBase {
	private readonly HertejDbContext m_DbContext;
	private readonly FileStorage m_FileStorage;

	public ImageController(HertejDbContext dbContext, FileStorage fileStorage) {
		m_DbContext = dbContext;
		m_FileStorage = fileStorage;
	}

	private async ValueTask<Image?> GetRandomImageAsync(string category) {
		return await m_DbContext.Images.Where(image => image.RatingStatus == RatingStatus.Passed && image.Category == category).OrderBy(image => EF.Functions.Random()).FirstOrDefaultAsync();
	}

	private async Task<IActionResult> LambdaOrNotFound(ValueTask<Image?> imageTask, Func<Image, IActionResult> selector) {
		Image? image = await imageTask;
		if (image != null) {
			return selector(image);
		} else {
			return NotFound();
		}
	}

	[HttpGet("categories")]
	public Task<string[]> GetCategories() {
		return m_DbContext.Images.Where(image => image.RatingStatus == RatingStatus.Passed).Select(image => image.Category).Distinct().ToArrayAsync();
	}

	[HttpGet("random")]
	public Task<IActionResult> GetRandomImage([FromQuery] string category) {
		return LambdaOrNotFound(GetRandomImageAsync(category), Ok);
	}

	[HttpGet("{id:long}")]
	public Task<IActionResult> GetImage([FromRoute] long id) {
		return LambdaOrNotFound(m_DbContext.Images.FindAsync(id), image => Ok(new GetImageDto() {
			Id = image.Id,
			Category = image.Category,
			MimeType = image.MimeType,
			Added = image.Added,
			Attribution = image.SourceAttribution,
		}));
	}

	[HttpGet("{id:long}/download")]
	public Task<IActionResult> DownloadImage([FromRoute] long id) {
		return LambdaOrNotFound(m_DbContext.Images.FindAsync(id), image => m_FileStorage.Get(image));
	}

	[HttpPost]
	[Authorize("Upload")]
	public async Task<IActionResult> UploadImage([FromForm] string category, IFormFile file) {
		// TODO authorize
		var image = new Image() {
			Category = category,
			MimeType = file.ContentType,
			Added = DateTime.UtcNow,
			RatingStatus = RatingStatus.NotRated
		};

		string storageId = await m_FileStorage.StoreAsync(image, file.OpenReadStream());
		image.StorageId = storageId;
		m_DbContext.Images.Add(image);

		await m_DbContext.SaveChangesAsync();

		return CreatedAtAction(nameof(GetImage), new { id = image.Id }, null);
	}
}
