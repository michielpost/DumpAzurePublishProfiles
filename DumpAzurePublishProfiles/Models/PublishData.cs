using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DumpAzurePublishProfiles.Models
{
	[XmlRoot(ElementName = "publishData")]
	public class PublishData
	{
		public PublishData()
		{
			PublishProfile = new List<Models.PublishProfile>();
		}

		[XmlElement(ElementName = "publishProfile")]
		public List<PublishProfile> PublishProfile { get; set; }
	}
}
