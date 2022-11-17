using HertejDB.Server.Data;

namespace HertejDB.Server.Dtos; 

public class GetImageDto {
	public long Id { get; set; }
	public string Category { get; set; }
	public string MimeType { get; set; }
	public DateTime Added { get; set; }
	public ImageSourceAttribution Attribution { get; set; }

	public GetImageDto(Image image) {
		Id = image.Id;
		Category = image.Category;
		MimeType = image.MimeType;
		Added = image.Added;
		Attribution = image.SourceAttribution;
	}
}
