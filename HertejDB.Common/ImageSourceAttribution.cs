namespace HertejDB.Common;

public class ImageSourceAttribution {
	public string SourceName { get; set; }
	public string RemoteId { get; set; }
	public string RemoteUrl { get; set; }
	public string Author { get; set; }
	public string License { get; set; }
	public DateTimeOffset Creation { get; set; }
}
