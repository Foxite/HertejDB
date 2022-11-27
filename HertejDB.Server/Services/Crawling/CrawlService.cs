using HertejDB.Server.Data;
using HertejDB.Server.Services;
using Microsoft.EntityFrameworkCore;

namespace HertejDB.Server.Crawling; 

public class CrawlService {
	private readonly IEnumerable<ImageAcquirer> m_Acquirers;
	private readonly HertejDbContext m_DbContext;
	private readonly ImageService m_ImageService;
	private readonly HttpClient m_Http;

	public CrawlService(IEnumerable<ImageAcquirer> acquirers, HertejDbContext dbContext, ImageService imageService, HttpClient http) {
		m_Acquirers = acquirers;
		m_DbContext = dbContext;
		m_ImageService = imageService;
		m_Http = http;
	}

	public IAsyncEnumerable<RemoteImage> GetImages(int maximum, string source, string parameter, string? lastPosition, CancellationToken cancellationToken) {
		ImageAcquirer acquirer = m_Acquirers.First(acquirer => acquirer.Name == source);

		return acquirer.AcquireImagesAsync(maximum, parameter, remoteId => CheckImageExists(source, remoteId), lastPosition, cancellationToken);
	}

	private Task<bool> CheckImageExists(string source, string remoteId) {
		return m_DbContext.Images.AnyAsync(image => image.SourceAttribution != null && image.SourceAttribution.SourceName == source && image.SourceAttribution.RemoteId == remoteId);
	}

	public async Task ExecutePendingCrawls(CancellationToken? cancellationTokenParam = null) {
		CancellationToken cancellationToken = cancellationTokenParam ?? CancellationToken.None;
		
		// ToList to avoid concurrent operations
		foreach (PendingCrawl pendingCrawl in await m_DbContext.PendingCrawls.ToListAsync(cancellationToken)) {
			int neededImages = Math.Min(pendingCrawl.MaxAtOnce, pendingCrawl.DesiredCount - await m_DbContext.Images.CountAsync(image => image.Category == pendingCrawl.Category, cancellationToken: cancellationToken));
			if (neededImages > 0) {
				await ExecutePendingCrawl(neededImages, pendingCrawl, cancellationToken);
			}
		}

		await m_DbContext.SaveChangesAsync(cancellationToken);
	}

	private async Task ExecutePendingCrawl(int neededImages, PendingCrawl pendingCrawl, CancellationToken cancellationToken) {
		await foreach (RemoteImage remoteImage in GetImages(neededImages, pendingCrawl.Source, pendingCrawl.SearchParameter, pendingCrawl.LastPosition, cancellationToken)) {
			pendingCrawl.LastPosition = remoteImage.PositionData;
			using HttpResponseMessage hrm = await remoteImage.DownloadAsync(m_Http);
			hrm.EnsureSuccessStatusCode();
			await m_ImageService.StoreNewImage(pendingCrawl.Category, await hrm.Content.ReadAsStreamAsync(cancellationToken), hrm.Content.Headers.ContentType!.MediaType!, remoteImage.SourceAttribution);
		}
	}

	public async Task<ICollection<PendingCrawl>> GetPendingCrawls() {
		return await m_DbContext.PendingCrawls.ToListAsync();
	}

	public async Task<PendingCrawl> AddPendingCrawl(string category, int desiredCount, string searchParameter, string source, int maxAtOnce) {
		var pendingCrawl = new PendingCrawl() {
			Category = category,
			DesiredCount = desiredCount,
			SearchParameter = searchParameter,
			Source = source,
			MaxAtOnce = maxAtOnce
		};
		
		m_DbContext.PendingCrawls.Add(pendingCrawl);

		await m_DbContext.SaveChangesAsync();

		return pendingCrawl;
	}

	public async Task<bool> RemovePendingCrawl(long id) {
		return (await m_DbContext.PendingCrawls.Where(pc => pc.Id == id).ExecuteDeleteAsync()) > 0;
	}

	public ValueTask<PendingCrawl?> GetPendingCrawl(long id) {
		return m_DbContext.PendingCrawls.FindAsync(id);
	}
}
