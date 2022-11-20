using HertejDB.Server.Data;
using Microsoft.AspNetCore.Mvc;

namespace HertejDB.Server.Storage; 

public abstract class FileStorage {
	public abstract IActionResult Get(Image image);
	public abstract Task<string> StoreAsync(Image image, Stream fileStream);
	public abstract void Delete(Image image);
}
