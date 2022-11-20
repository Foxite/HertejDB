namespace HertejDB.Server.Crawling; 

public abstract class ImageAcquirer {
	public abstract string Name { get; }
	public abstract IAsyncEnumerable<RemoteImage> AcquireImagesAsync(int maximum, string searchParameter, CheckImageExists imageExists, CancellationToken cancellationToken);

	public delegate Task<bool> CheckImageExists(string remoteId);
}
