namespace HertejDB.Server.Options;

public class RatingOptions {
	/// <summary>
	/// Images must have at least this amount of ratings to be finalized.
	/// </summary>
	public int MinimumToComplete { get; set; }
	
	/// <summary>
	/// Images must have at least this fraction (0-1) of positive ratings to be approved.
	/// </summary>
	public float MinimumToApprove { get; set; }
	
	/// <summary>
	/// Images must have at most this fraction (0-1) of positive ratings to be rejected.
	/// </summary>
	public float MaximumToReject { get; set; }
}
