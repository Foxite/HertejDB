using HertejDB.Server.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SpookilySharp;

namespace HertejDB.Server.Storage; 

public class LocalFileStorage : FileStorage {
	private readonly IOptions<Options> m_Options;

	public LocalFileStorage(IOptions<Options> options) {
		m_Options = options;
	}
	
	public async override Task<string> StoreAsync(Image image, Stream download) {
		string tempPath = Path.GetTempFileName();
		bool tempFileCreated = false;
		try {
			string hash;
			await using (var hashDownload = new HashedStream(download)) {
				await using (FileStream file = File.OpenWrite(tempPath)) {
					tempFileCreated = true;
					await hashDownload.CopyToAsync(file);
				}

				hash = hashDownload.ReadHash128.ToString();
			}

			File.Move(tempPath, GetPath(hash));
			tempFileCreated = false;
			return hash;
		} finally {
			if (tempFileCreated) {
				File.Delete(tempPath);
			}
		}
	}
	
	public override IActionResult Get(Image image) {
		return new FileStreamResult(File.OpenRead(GetPath(image.StorageId)), image.MimeType);
	}

	public override void Delete(Image image) {
		File.Delete(GetPath(image.StorageId));
	}

	private string GetPath(string hash) {
		string directory = Path.Combine(m_Options.Value.StoragePath, hash[..2]);

		Directory.CreateDirectory(directory);
		
		return Path.Combine(directory, hash);
	}
	
	public class Options {
		public string StoragePath { get; set; }
	}
}
