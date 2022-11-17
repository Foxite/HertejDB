using CronScheduler.Extensions.Scheduler;
using HertejDB.Server.Data;
using HertejDB.Server.Options;
using Microsoft.EntityFrameworkCore;
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

		foreach (var row in from image in dbContext.Images
			let approval = image.Ratings.Count(rating => rating.IsSuitable)
			let totalRatings = image.Ratings.Count
			let opinion = (float) approval / totalRatings
			where
				image.RatingStatus == RatingStatus.InProgress &&
				totalRatings >= options.Value.MinimumToComplete &&
				(opinion <= options.Value.MaximumToReject || opinion >= options.Value.MinimumToApprove)
			select new { image.Id, opinion }
	    ) {
			var image = new Image();
			image.Id = row.Id;
			dbContext.Attach(image);
			
			if (row.opinion >= options.Value.MinimumToApprove) {
				image.RatingStatus = RatingStatus.Passed;
			} else {
				image.RatingStatus = RatingStatus.Rejected;
			}
		}

		await dbContext.SaveChangesAsync(cancellationToken);
	}
}
