namespace HertejDB.Common; 

public class GetImageDto {
	public long Id { get; set; }
	public string Category { get; set; }
	public string MimeType { get; set; }
	public DateTime Added { get; set; }
	public ImageSourceAttribution? Attribution { get; set; }
}
