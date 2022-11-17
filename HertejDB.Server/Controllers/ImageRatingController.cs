using System.Net;
using HertejDB.Server.Data;
using HertejDB.Server.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HertejDB.Server.Controllers; 

[ApiController]
[Route("[controller]")]
[Authorize("Rate")] // TODO authorize only specific apps to provide a UserId, otherwise get UserId from principal and open up this controller
public class ImageRatingController : ControllerBase {
	private readonly HertejDbContext m_DbContext;

	public ImageRatingController(HertejDbContext dbContext) {
		m_DbContext = dbContext;
	}

	[HttpGet("unrated")]
	public async Task<IActionResult> GetUnratedImage([FromQuery] string userId, [FromQuery] string? category = null) {
		IQueryable<Image> query = m_DbContext.Images
			.Where(image =>
				(image.RatingStatus == RatingStatus.InProgress || image.RatingStatus == RatingStatus.NotRated) &&
				!image.Ratings.Any(ir => ir.UserId == userId)
			);

		// Sorting order:
		// - Images that match the category (if provided)
		// - Images that have RatingStatus.InProgress
		// - Oldest images
		if (category != null) {
			query = query
				.OrderBy(image => image.Category == category)
				.ThenBy (image => image.RatingStatus == RatingStatus.InProgress)
				.ThenBy (image => image.Added);
		} else {
			query = query
				.OrderBy(image => image.RatingStatus == RatingStatus.InProgress)
				.ThenBy (image => image.Added);
		}

		Image? image = await query.FirstOrDefaultAsync();
		
		if (image != null) {
			return Ok(new GetImageDto(image));
		} else {
			return NoContent();
		}
	}
	
	[HttpPut("{imageId:long}")]
	public async Task<IActionResult> SubmitRating([FromRoute] long imageId, [FromBody] SubmitRatingDto dto) {
		Image? image = await m_DbContext.Images.FindAsync(imageId);

		if (image == null) {
			return NotFound();
		}

		if (image.RatingStatus is RatingStatus.Passed or RatingStatus.Rejected) {
			return StatusCode((int) HttpStatusCode.Gone);
		}
		
		ImageRating? rating = await m_DbContext.ImageRatings.FindAsync(imageId, dto.UserId);

		if (rating != null) {
			rating.IsSuitable = dto.IsSuitable;
		} else {
			rating = new ImageRating() {
				ImageId = imageId,
				UserId = dto.UserId,
				IsSuitable = dto.IsSuitable,
			};
			m_DbContext.ImageRatings.Add(rating);
		}

		image.RatingStatus = RatingStatus.InProgress;
		
		await m_DbContext.SaveChangesAsync();

		return Ok();
	}
}
