namespace HertejDB.Server.Data;

public class ImageRating {
	public long ImageId { get; set; }
	public string UserId { get; set; }
	
	public Image Image { get; set; }
	public bool IsSuitable { get; set; }
}
