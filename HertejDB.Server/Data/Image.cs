using HertejDB.Common;

namespace HertejDB.Server.Data;

public class Image {
	public long Id { get; set; }
	public string Category { get; set; }
	public string StorageId { get; set; }
	public string MimeType { get; set; }
	public DateTime Added { get; set; }
	
	public RatingStatus RatingStatus { get; set; }
	
	public ImageSourceAttribution? SourceAttribution { get; set; }
	
	public ICollection<ImageRating> Ratings { get; set; }
}
