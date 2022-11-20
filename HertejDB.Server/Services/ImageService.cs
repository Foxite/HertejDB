using HertejDB.Common;
using HertejDB.Server.Data;
using HertejDB.Server.Storage;
using Microsoft.EntityFrameworkCore;

namespace HertejDB.Server.Services; 

public class ImageService {
	private readonly HertejDbContext m_DbContext;
	private readonly FileStorage m_FileStorage;

	public ImageService(HertejDbContext dbContext, FileStorage fileStorage) {
		m_DbContext = dbContext;
		m_FileStorage = fileStorage;
	}
	
	public async Task<Image?> GetRandomImageAsync(string category) {
		return await m_DbContext.Images.Include(image => image.SourceAttribution).Where(image => image.RatingStatus == RatingStatus.Passed && image.Category == category).OrderBy(image => EF.Functions.Random()).FirstOrDefaultAsync();
	}

	public Task<string[]> GetCategories() {
		return m_DbContext.Images.Where(image => image.RatingStatus == RatingStatus.Passed).Select(image => image.Category).Distinct().ToArrayAsync();
	}

	public Task<Image?> GetImageById(long id) {
		return m_DbContext.Images.Include(image => image.SourceAttribution).FirstOrDefaultAsync(image => image.Id == id);
	}
	
	public async Task<Image> StoreNewImage(string category, Stream download, string contentType, ImageSourceAttribution sourceAttribution) {
		var image = new Image() {
			Category = category,
			MimeType = contentType,
			Added = DateTime.UtcNow,
			RatingStatus = RatingStatus.NotRated,
			SourceAttribution = sourceAttribution
		};

		string storageId = await m_FileStorage.StoreAsync(image, download);
		image.StorageId = storageId;
		m_DbContext.Images.Add(image);

		await m_DbContext.SaveChangesAsync();

		return image;
	}

	public async Task<ICollection<Image>> GetImages(string category) {
		return await m_DbContext.Images.Include(image => image.SourceAttribution).Where(image => image.Category == category).ToListAsync();
	}

	public async Task<bool> DeleteImage(long id) {
		//return (await m_DbContext.Images.Where(image => image.Id == id).ExecuteDeleteAsync()) > 0;
		Image? image = await m_DbContext.Images.FindAsync(id);
		if (image == null) {
			return false;
		} else {
			m_FileStorage.Delete(image);
			m_DbContext.Remove(image);
			await m_DbContext.SaveChangesAsync();
			return true;
		}
	}
}
