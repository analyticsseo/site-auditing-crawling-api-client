using System;
using System.Xml;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using CookComputing.XmlRpc;
using System.Xml.Serialization;
using System.Security.Cryptography;

namespace AseoAuditCrawling
{
	public interface Company: IXmlRpcProxy
	{
		[XmlRpcMethod("company.getSites", StructParams=false)]
		object getSites(string hash, string domain, string timestamp, string salt);	
	}

	public interface Site: IXmlRpcProxy
	{
		[XmlRpcMethod("site.getOrganicKeywordsRanking", StructParams=false)]
		object getOrganicKeywordsRanking(string hash, string domain, string timestamp, string salt, int siteId, 
		                                 int campaignId, string period, bool domain_alias, int offset, int limit);	
	
		[XmlRpcMethod("site.getUniversalKeywordsRanking", StructParams=false)]
		object getUniversalKeywordsRanking(string hash, string domain, string timestamp, string salt, 
		                                   int siteId, int campaignId, int offset, int limit);	
	}

	class AseoXmlRpcClient
	{
		public string apiUrl;
		public string domain;
		public string apiKey;
		public string httpUser;
		public string httpPasswd;

		public static Random random = new Random((int)DateTime.Now.Ticks);

		public AseoXmlRpcClient(string domain, string apiKey)
		{
			this.domain = domain;
			this.apiKey = apiKey;
		}

		public void connect(string apiUrl, string httpUser=null, string httpPasswd=null)
		{
			this.apiUrl = apiUrl;
			this.httpUser = httpUser;
			this.httpPasswd = httpPasswd;
		}

		public int generateTimestamp()
		{
			DateTime baseTimestamp = new DateTime (1970, 1, 1, 0, 0, 0);
			Int32 currentTimestamp = (Int32)(DateTime.Now.Subtract (baseTimestamp)).TotalSeconds;

			return (int) currentTimestamp;
		}

		public string generateSalt()
		{
			string inputCharacters = "abcdefghijklmnopqrstuvwxyz0123456789";

			StringBuilder builder = new StringBuilder ();
			char character;

			for (int i = 0; i < 12; i++)
			{
				character = inputCharacters[random.Next(0, inputCharacters.Length)];
				builder.Append(character);
			}

			return builder.ToString();
		}

		private string encrypt(string message, string key)
		{
			System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
			byte[] keyByte = encoding.GetBytes(key);

			HMACSHA256 hmacsha256 = new HMACSHA256(keyByte);

			byte[] messageBytes = encoding.GetBytes(message);
			byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
			return BitConverter.ToString(hashmessage).Replace("-", "").ToLower();
		}

		public string generateHash(string domain, string apiKey, string method, string timestamp, string salt)
		{
			string rawAuth = timestamp + ";" + domain + ";" + salt + ";" + method;
			return this.encrypt (rawAuth, apiKey).ToLower();
		}

		public string getSites()
		{
			string method = "company.getSites";
			string salt = this.generateSalt ();
			string timestamp = this.generateTimestamp().ToString();
			string hash = this.generateHash (this.domain, this.apiKey, method, timestamp, salt);

			Company company = XmlRpcProxyGen.Create<Company>();
			company.Url = this.apiUrl;
			company.Credentials = new NetworkCredential(this.httpUser, this.httpPasswd);

			object apiResponse = company.getSites (hash, domain, timestamp, salt);

			Console.Write (apiResponse);
			return JsonConvert.SerializeObject(apiResponse);
		}

		public string getOrganicKeywordsRanking(int siteId, int campaignId, string date, 
		                                     bool domain_alias, int offset, int limit)
		{
			string method = "site.getOrganicKeywordsRanking";
			string salt = this.generateSalt ();
			string timestamp = this.generateTimestamp().ToString();
			string hash = this.generateHash (this.domain, this.apiKey, method, timestamp, salt);

			Site site = XmlRpcProxyGen.Create<Site>();
			site.Url = this.apiUrl;
			site.Credentials = new NetworkCredential(this.httpUser, this.httpPasswd);

			object apiResponse = site.getOrganicKeywordsRanking(hash, domain, timestamp, salt, siteId, 
			                                                    campaignId, date, domain_alias, offset, limit);	
			return JsonConvert.SerializeObject(apiResponse);
		}

		public string getUniversalKeywordsRanking(int siteId, int campaignId, int offset, int limit)
		{
			string method = "site.getUniversalKeywordsRanking";
			string salt = this.generateSalt ();
			string timestamp = this.generateTimestamp().ToString();
			string hash = this.generateHash (this.domain, this.apiKey, method, timestamp, salt);

			Site site = XmlRpcProxyGen.Create<Site>();
			site.Url = this.apiUrl;
			site.Credentials = new NetworkCredential(this.httpUser, this.httpPasswd);

			object apiResponse = site.getUniversalKeywordsRanking(hash, domain, timestamp, salt, siteId, 
			                                                    campaignId, offset, limit);	
			return JsonConvert.SerializeObject(apiResponse);
		}

		static void Main (string[] args)
		{
			const string DOMAIN = "YOUR_DOMAIN";
			const string API_KEY = "YOUR_API_KEY";
			const string API_URL = "http://app.analyticsseo.com/services/xmlrpc";

			AseoXmlRpcClient client = new AseoXmlRpcClient(DOMAIN, API_KEY);
			client.connect (API_URL);

			string result = client.getOrganicKeywordsRanking (1, 2, "2016-06-07", false, 0, 10);
			Console.Write(result);
		}
	}
}

