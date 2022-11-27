using HertejDB.Common;

namespace HertejDB.Server.Crawling;

public class RemoteImage {
	private readonly Func<HttpClient, Task<HttpResponseMessage>> m_DownloadFunc;
	
	public ImageSourceAttribution SourceAttribution { get; }
	public string PositionData { get; }

	public RemoteImage(ImageSourceAttribution sourceAttribution, Func<HttpClient, Task<HttpResponseMessage>> downloadFunc, string positionData) {
		m_DownloadFunc = downloadFunc;
		PositionData = positionData;
		SourceAttribution = sourceAttribution;
	}

	public Task<HttpResponseMessage> DownloadAsync(HttpClient http) => m_DownloadFunc(http);
}
