using DumpAzurePublishProfiles.Models;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Management.WebSites;
using Microsoft.WindowsAzure.Management.WebSites.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace DumpAzurePublishProfiles
{
	class Program
	{
		static void Main(string[] args)
		{
			MainAsync(args).Wait();
			// or, if you want to avoid exceptions being wrapped into AggregateException:
			//  MainAsync().GetAwaiter().GetResult();

			Console.WriteLine("Press ENTER to exit.");
			Console.ReadLine();
		}


		static async Task MainAsync(string[] args)
		{
			string path = Directory.GetCurrentDirectory();
			if (args.Any())
				path = args.First();

			string writePath = Path.Combine(path, "PublishProfiles");
			if (!Directory.Exists(writePath))
				Directory.CreateDirectory(writePath);

			//Find .publishprofile file
			var publishProfile = Directory.GetFiles(path, "*.publishsettings").FirstOrDefault();
			if (publishProfile == null)
			{
				Console.WriteLine("No .publishsettings file found. Please download it here http://go.microsoft.com/fwlink/?LinkID=301775 and place it in the current directory.");
				return;
			}

			var pubProfileText = File.ReadAllText(publishProfile);
			PublishSettingsFile file = new PublishSettingsFile(pubProfileText);
			if (file.Subscriptions == null || !file.Subscriptions.Any())
			{
				Console.WriteLine("No subscriptions found in PublishSettingsFile");
				return;
			}
			if (file.Subscriptions.Count() > 1)
				Console.WriteLine("Multiple Subscriptions found, using the first: " + file.Subscriptions.First().Name);

			var cred = file.Subscriptions.First().GetCredentials();

			var client = new WebSiteManagementClient(cred);

			//
			var webSpaces = await client.WebSpaces.ListAsync();
			foreach (var webSpace in webSpaces)
			{
				var websites = await client.WebSpaces.ListWebSitesAsync(webSpace.Name, new WebSiteListParameters
				{
					PropertiesToInclude = new List<string>()
				});

				foreach (var website in websites)
				{
					var profiles = await client.WebSites.GetPublishProfileAsync(webSpace.Name, website.Name);

					PublishData publishData = new PublishData();
					publishData.PublishProfile = profiles.Select(ToPublishProfile).ToList();

					var filePath = Path.Combine(writePath, website.Name + ".PublishSettings");

					Console.WriteLine("Writing PublishProfile for: " + website.Name);

					SerializeObject(publishData, filePath);
                }
			}

		}

		/// <summary>
		/// Serializes an object.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="serializableObject"></param>
		/// <param name="fileName"></param>
		public static void SerializeObject<T>(T serializableObject, string fileName)
		{
			if (serializableObject == null) { return; }

			try
			{
				XmlDocument xmlDocument = new XmlDocument();
				XmlSerializer serializer = new XmlSerializer(serializableObject.GetType());
				using (MemoryStream stream = new MemoryStream())
				{
					serializer.Serialize(stream, serializableObject);
					stream.Position = 0;
					xmlDocument.Load(stream);
					xmlDocument.Save(fileName);
					stream.Close();
				}
			}
			catch (Exception ex)
			{
			}
		}


		private static PublishProfile ToPublishProfile(WebSiteGetPublishProfileResponse.PublishProfile arg)
		{
			PublishProfile result = new PublishProfile()
			{
				ControlPanelLink = arg.ControlPanelUri?.ToString(),
				DestinationAppUrl = arg.DestinationAppUri?.ToString(),
				FtpPassiveMode = arg.FtpPassiveMode.ToString(),
				HostingProviderForumLink = arg.HostingProviderForumUri?.ToString(),
				MsdeploySite = arg.MSDeploySite,
				MySQLDBConnectionString = arg.MySqlConnectionString,
				ProfileName = arg.ProfileName,
				PublishMethod = arg.PublishMethod,
				PublishUrl = arg.PublishUrl,
				SQLServerDBConnectionString = arg.SqlServerConnectionString,
				UserName = arg.UserName,
				UserPWD = arg.UserPassword,
				WebSystem = "WebSites"

			};

			return result;
        }

	}
}
