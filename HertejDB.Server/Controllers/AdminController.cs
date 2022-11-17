using HertejDB.Server.Jobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HertejDB.Server.Controllers; 

[ApiController]
[Route("[controller]")]
[Authorize("Admin")]
public class AdminController : ControllerBase {
	private readonly CompleteRatingsJob m_RatingsJob;

	public AdminController(CompleteRatingsJob ratingsJob) {
		m_RatingsJob = ratingsJob;
	}

	[HttpPost("CompleteRatings")]
	public async Task<IActionResult> CompleteRatingsNow() {
		await m_RatingsJob.ExecuteAsync(CancellationToken.None);
		return Ok();
	}
}
