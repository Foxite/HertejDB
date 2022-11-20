using HertejDB.Common;

namespace HertejDB.Server.Crawling;

public class RemoteImage {
	private readonly Func<HttpClient, Task<HttpResponseMessage>> m_DownloadFunc;
	
	public ImageSourceAttribution SourceAttribution { get; }

	public RemoteImage(ImageSourceAttribution sourceAttribution, Func<HttpClient, Task<HttpResponseMessage>> downloadFunc) {
		m_DownloadFunc = downloadFunc;
		SourceAttribution = sourceAttribution;
	}

	public Task<HttpResponseMessage> DownloadAsync(HttpClient http) => m_DownloadFunc(http);
}
