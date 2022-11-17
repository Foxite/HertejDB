namespace HertejDB.Server.Options; 

public class AuthOptions {
	public required string Authority { get; set; }
	public string? DiscoveryDocument { get; set; }
	public required string RateRole { get; set; }
	public required string UploadRole { get; set; }
	public required string AdminRole { get; set; }
	public required string[] Audiences { get; set; }

	public string GetDiscoveryDocument() => DiscoveryDocument ?? (Authority + "/.well-known/openid-configuration");
}
