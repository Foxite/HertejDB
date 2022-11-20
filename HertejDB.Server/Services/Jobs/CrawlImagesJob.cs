using CronScheduler.Extensions.Scheduler;
using HertejDB.Server.Crawling;

namespace HertejDB.Server.Jobs; 

public class CrawlImagesJob : IScheduledJob {
	private readonly IServiceProvider m_Services;

	public string Name => nameof(CrawlImagesJob);

	public CrawlImagesJob(IServiceProvider services) {
		m_Services = services;
	}
	
	public async Task ExecuteAsync(CancellationToken cancellationToken) {
		await using AsyncServiceScope scope = m_Services.CreateAsyncScope();
		var crawlService = scope.ServiceProvider.GetRequiredService<CrawlService>();
		await crawlService.ExecutePendingCrawls(cancellationToken);
	}
}
