using DumpAzurePublishProfiles.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Management.WebSites;
using Microsoft.WindowsAzure.Management.WebSites.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
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

			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);

			var token = GetAuthorizationHeader();
			var cred = new TokenCloudCredentials(
			  ConfigurationManager.AppSettings["subscriptionId"], token);

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

					var filePath = Path.Combine(path, website.Name + ".PublishSettings");

					Console.WriteLine("Writing publishsettings for: " + website.Name);

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

		private static string GetAuthorizationHeader()
		{
			AuthenticationResult result = null;

			var context = new AuthenticationContext(string.Format(
			  ConfigurationManager.AppSettings["login"],
			  ConfigurationManager.AppSettings["tenantId"]));

			var thread = new Thread(() =>
			{
				result = context.AcquireToken(
				  ConfigurationManager.AppSettings["apiEndpoint"],
				  ConfigurationManager.AppSettings["clientId"],
				  new Uri(ConfigurationManager.AppSettings["redirectUri"]));
			});

			thread.SetApartmentState(ApartmentState.STA);
			thread.Name = "AquireTokenThread";
			thread.Start();
			thread.Join();

			if (result == null)
			{
				throw new InvalidOperationException("Failed to obtain the JWT token");
			}

			string token = result.AccessToken;
			return token;
		}



	}
}
