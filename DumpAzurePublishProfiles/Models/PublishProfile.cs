﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DumpAzurePublishProfiles.Models
{
	[XmlRoot(ElementName = "publishProfile")]
	public class PublishProfile
	{
		[XmlElement(ElementName = "databases")]
		public string Databases { get; set; }
		[XmlAttribute(AttributeName = "profileName")]
		public string ProfileName { get; set; }
		[XmlAttribute(AttributeName = "publishMethod")]
		public string PublishMethod { get; set; }
		[XmlAttribute(AttributeName = "publishUrl")]
		public string PublishUrl { get; set; }
		[XmlAttribute(AttributeName = "msdeploySite")]
		public string MsdeploySite { get; set; }
		[XmlAttribute(AttributeName = "userName")]
		public string UserName { get; set; }
		[XmlAttribute(AttributeName = "userPWD")]
		public string UserPWD { get; set; }
		[XmlAttribute(AttributeName = "destinationAppUrl")]
		public string DestinationAppUrl { get; set; }
		[XmlAttribute(AttributeName = "SQLServerDBConnectionString")]
		public string SQLServerDBConnectionString { get; set; }
		[XmlAttribute(AttributeName = "mySQLDBConnectionString")]
		public string MySQLDBConnectionString { get; set; }
		[XmlAttribute(AttributeName = "hostingProviderForumLink")]
		public string HostingProviderForumLink { get; set; }
		[XmlAttribute(AttributeName = "controlPanelLink")]
		public string ControlPanelLink { get; set; }
		[XmlAttribute(AttributeName = "webSystem")]
		public string WebSystem { get; set; }
		[XmlAttribute(AttributeName = "ftpPassiveMode")]
		public string FtpPassiveMode { get; set; }
	}
}
