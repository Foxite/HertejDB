using System.Runtime.CompilerServices;
using FlickrNet;
using HertejDB.Common;

namespace HertejDB.Server.Crawling;

public class FlickrImageAcquirer : ImageAcquirer {
	private readonly Flickr m_Flickr;

	public override string Name => "Flickr";

	public FlickrImageAcquirer(Flickr flickr) {
		m_Flickr = flickr;
	}

	public async override IAsyncEnumerable<RemoteImage> AcquireImagesAsync(int maximum, string searchParameter, CheckImageExists imageExists, string? lastPosition, [EnumeratorCancellation] CancellationToken cancellationToken) {
		var options = new PhotoSearchOptions() {
			Text = searchParameter,
			Licenses = {
				LicenseType.AttributionNoncommercialShareAlikeCC,
				LicenseType.AttributionNoncommercialCC,
				LicenseType.AttributionNoncommercialNoDerivativesCC,
				LicenseType.AttributionCC,
				LicenseType.AttributionShareAlikeCC,
				LicenseType.AttributionNoDerivativesCC,
				LicenseType.NoKnownCopyrightRestrictions,
				LicenseType.UnitedStatesGovernmentWork,
				LicenseType.PublicDomainDedicationCC0,
				LicenseType.PublicDomainMark,
			},
			Extras =
				PhotoSearchExtras.License |
				PhotoSearchExtras.DateTaken |
				PhotoSearchExtras.OwnerName |
				PhotoSearchExtras.Tags,
			PerPage = maximum
		};

		int page;
		if (lastPosition != null && int.TryParse(lastPosition, out int parsedPage)) {
			page = parsedPage;
		} else {
			page = 0;
		}

		int startedAtPage = page;
		int yielded = 0;

		while (yielded < maximum) {
			options.Page = page;
			PhotoCollection photos = await m_Flickr.PhotosSearchAsync(options);
			bool any = false;

			foreach (Photo photo in photos) {
				any = true;
				if (await imageExists(photo.PhotoId)) {
					continue;
				}

				if (photo.Farm == "0") {
					// Not sure why, but sometimes Flickr gives a photo with urls for farm 0, which doesn't exist.
					// You can usually get the image from another farm but I'm not gonna bother fixing the url
					continue;
				}

				yield return new RemoteImage(
					new ImageSourceAttribution() {
						Author = photo.OwnerName,
						Creation = new DateTimeOffset(photo.DateTaken, TimeSpan.Zero),
						License = photo.License switch {
							LicenseType.AttributionNoncommercialShareAlikeCC => "CC-BY-SA-NC",
							LicenseType.AttributionNoncommercialCC => "CC-BY-NC",
							LicenseType.AttributionNoncommercialNoDerivativesCC => "CC-BY-NC-ND",
							LicenseType.AttributionCC => "CC-BY",
							LicenseType.AttributionShareAlikeCC => "CC-SA",
							LicenseType.AttributionNoDerivativesCC => "CC-ND",
							LicenseType.PublicDomainDedicationCC0 => "CC-0",
							LicenseType.UnitedStatesGovernmentWork => "U.S. Government work",
							LicenseType.NoKnownCopyrightRestrictions => "No known copyright restrictions",
							LicenseType.PublicDomainMark => "Public domain",
							LicenseType.AllRightsReserved => "All rights reserved",
							_ => throw new ArgumentOutOfRangeException()
						},
						RemoteId = photo.PhotoId,
						RemoteUrl = photo.WebUrl,
						SourceName = Name,
					},
					http => http.GetAsync(photo.LargeUrl ?? photo.MediumUrl ?? photo.SmallUrl, cancellationToken),
					page.ToString()
				);

				yielded++;

				if (yielded > maximum) {
					yield break;
				}
			}

			if (any || page == 0) {
				// Next page
				page++;
			} else {
				// Page empty. Restart from top
				page = 0;
			}
			
			if (page == startedAtPage) {
				// We have enumerated all pages
				yield break;
			}
		}
	}
}
