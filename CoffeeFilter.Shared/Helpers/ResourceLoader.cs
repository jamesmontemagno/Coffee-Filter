using System;
using System.Threading.Tasks;
using System.Net;

namespace CoffeeFilter.Shared
{
	public class ResourceLoader
	{
		static ResourceLoader loader;

		public static ResourceLoader DefaultLoader {
			get {
				if (loader == null)
					loader = new ResourceLoader ();

				return loader;
			}
		}

		ResourceLoader ()
		{
		}

		public async Task<byte[]> GetImageData (string resourceUrl)
		{
			byte[] data = null;

			using (var c = new GzipWebClient ())
				data = await c.DownloadDataTaskAsync (resourceUrl);

			return data;
		}

		class GzipWebClient : WebClient
		{
			protected override WebRequest GetWebRequest (Uri address)
			{
				var request = base.GetWebRequest (address);
				if (request is HttpWebRequest)
					((HttpWebRequest)request).AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
				return request;
			}
		}
	}
}

