using CronScheduler.Extensions.Scheduler;
using HertejDB.Server.Data;
using HertejDB.Server.Options;
using Microsoft.Extensions.Options;

namespace HertejDB.Server.Jobs;

public class CompleteRatingsJob : IScheduledJob {
	private readonly IServiceProvider m_Services;
	
	public string Name => nameof(CompleteRatingsJob);

	public CompleteRatingsJob(IServiceProvider services) {
		m_Services = services;
	}
	
	public async Task ExecuteAsync(CancellationToken cancellationToken) {
		await using var scope = m_Services.CreateAsyncScope();
		var options = scope.ServiceProvider.GetRequiredService<IOptions<RatingOptions>>();
		var dbContext = scope.ServiceProvider.GetRequiredService<HertejDbContext>();

		foreach (var row in
			from image in dbContext.Images
			where image.RatingStatus == RatingStatus.InProgress
			let approval = image.Ratings
				.Join(
					dbContext.Users,
					rating => rating.UserId,
					user => user.UserId,
					(rating, user) => new { rating, user }
				)
				.Sum(item => (item.rating.IsSuitable ? 1 : -1) * (item.user == null ? 1 : item.user.Weight))
			where approval <= options.Value.MaximumToReject || approval >= options.Value.MinimumToApprove
			select new { image, approval }
	    ) {
			if (row.approval >= options.Value.MinimumToApprove) {
				Console.WriteLine("passed");
				row.image.RatingStatus = RatingStatus.Passed;
			} else {
				Console.WriteLine("rejected");
				row.image.RatingStatus = RatingStatus.Rejected;
			}
		}

		await dbContext.SaveChangesAsync(cancellationToken);
	}
}
