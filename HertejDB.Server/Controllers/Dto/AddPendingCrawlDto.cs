namespace HertejDB.Server.Controllers;

public class AddPendingCrawlDto {
	public string Category { get; set; }
	public string SearchParameter { get; set; }
	public string Source { get; set; }
	public int DesiredCount { get; set; }
	public int MaxAtOnce { get; set; }
}
