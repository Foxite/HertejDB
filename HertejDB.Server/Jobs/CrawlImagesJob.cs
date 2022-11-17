using CronScheduler.Extensions.Scheduler;

namespace HertejDB.Server.Jobs; 

public class CrawlImagesJob : IScheduledJob {
	public string Name => nameof(CrawlImagesJob);
	
	public Task ExecuteAsync(CancellationToken cancellationToken) {
		throw new NotImplementedException();
	}
}