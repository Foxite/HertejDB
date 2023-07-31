using CronScheduler.Extensions.Internal;
using CronScheduler.Extensions.Scheduler;
using HertejDB.Common;
using HertejDB.Server.Crawling;
using HertejDB.Server.Data;
using HertejDB.Server.Jobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HertejDB.Server.Controllers; 

[ApiController]
[Route("[controller]")]
[Authorize("Admin")]
public class AdminController : ControllerBase {
	private readonly ISchedulerRegistration m_Scheduler;
	private readonly CrawlService m_CrawlService;
	private readonly HertejDbContext m_DbContext;

	public AdminController(ISchedulerRegistration scheduler, CrawlService crawlService, HertejDbContext dbContext) {
		m_CrawlService = crawlService;
		m_DbContext = dbContext;
		m_Scheduler = scheduler;
	}

	[HttpPost("ExecuteJobNow")]
	public async Task<IActionResult> ExecuteJobNow([FromQuery] string jobName, CancellationToken cancellationToken) {
		if (!m_Scheduler.Jobs.TryGetValue(jobName, out SchedulerTaskWrapper? job)) {
			return NotFound();
		}
		
		await job.ScheduledJob.ExecuteAsync(cancellationToken);
		return Ok();
	}

	[HttpGet("TestCrawl")]
	public async Task<IActionResult> TestAcquire([FromQuery] string source, [FromQuery] string parameter, [FromQuery] int maximum, [FromQuery] string? position, CancellationToken cancellationToken) {
		var result = new List<ImageSourceAttribution>();
		await foreach (RemoteImage remoteImage in m_CrawlService.GetImages(maximum, source, parameter, position, cancellationToken)) {
			result.Add(remoteImage.SourceAttribution);
		}

		return Ok(result);
	}
	
	[HttpGet("PendingCrawl")]
	public async Task<IActionResult> GetPendingCrawls() {
		ICollection<PendingCrawl> result = await m_CrawlService.GetPendingCrawls();
		return Ok(result);
	}
	
	[HttpGet("PendingCrawl/{id:long}")]
	public async Task<IActionResult> GetPendingCrawls([FromRoute] long id) {
		PendingCrawl? result = await m_CrawlService.GetPendingCrawl(id);
		if (result == null) {
			return NotFound();
		} else {
			return Ok(result);
		}
	}
	
    [HttpPost("PendingCrawl")]
    public async Task<IActionResult> AddPendingCrawl([FromBody] AddPendingCrawlDto dto) {
	    PendingCrawl result = await m_CrawlService.AddPendingCrawl(dto.Category, dto.DesiredCount, dto.SearchParameter, dto.Source, dto.MaxAtOnce);
	    return Ok(result);
    }
    
    [HttpDelete("PendingCrawl/{id:long}")]
    public async Task<IActionResult> RemovePendingCrawl([FromRoute] long id) {
	    if (await m_CrawlService.RemovePendingCrawl(id)) {
		    return Ok();
	    } else {
		    return NotFound();
	    }
    }

	[HttpGet("UserWeight/{userId}")]
	public async Task<IActionResult> GetUserWeight([FromRoute] string userId) {
		User? user = await m_DbContext.Users.FirstOrDefaultAsync(user => user.UserId == userId);
		if (user == null) {
			return NoContent();
		} else {
			return Ok(user.Weight);
		}
	}

	[HttpPost("UserWeight/{userId}")]
	public async Task<IActionResult> SetUserWeight([FromRoute] string userId, [FromBody] int weight) {
		User? user = await m_DbContext.Users.FirstOrDefaultAsync(user => user.UserId == userId);
		if (user == null) {
			user = new User() {
				UserId = userId
			};
			m_DbContext.Users.Add(user);
		}

		user.Weight = weight;

		await m_DbContext.SaveChangesAsync();
		return Ok();
	}
}