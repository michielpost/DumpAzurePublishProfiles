using Microsoft.WindowsAzure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DumpAzurePublishProfiles.Models
{
	public class PublishSettings
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public Uri ServiceUrl { get; set; }
		public X509Certificate2 Certificate { get; set; }

		public SubscriptionCloudCredentials GetCredentials()
		{
			return new CertificateCloudCredentials(Id, Certificate);
		}

	}


	public class PublishSettingsFile
	{
		public PublishSettingsFile(string fileContents)
		{
			var document = XDocument.Parse(fileContents);

			_subscriptions = document.Descendants("Subscription")
								.Select(ToPublishSettings).ToList();
		}

		private PublishSettings ToPublishSettings(XElement element)
		{
			var settings = new PublishSettings();
			settings.Id = Get(element, "Id");
			settings.Name = Get(element, "Name");
			settings.ServiceUrl = GetUri(element, "ServiceManagementUrl");
			settings.Certificate = GetCertificate(element, "ManagementCertificate");
			return settings;
		}

		private string Get(XElement element, string name)
		{
			return (string)element.Attribute(name);
		}

		private Uri GetUri(XElement element, string name)
		{
			return new Uri(Get(element, name));
		}

		private X509Certificate2 GetCertificate(XElement element, string name)
		{
			var encodedData = Get(element, name);
			var certificateAsBytes = Convert.FromBase64String(encodedData);
			return new X509Certificate2(certificateAsBytes);
		}

		public IEnumerable<PublishSettings> Subscriptions
		{
			get
			{
				return _subscriptions;
			}
		}

		private readonly IList<PublishSettings> _subscriptions;
	}

}
