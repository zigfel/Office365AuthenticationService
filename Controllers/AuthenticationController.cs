using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Caching;
using System.Text;
using System.Web.Http;
using System.Web.Http.Cors;

namespace AAD.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*", SupportsCredentials = true)]
    public class AuthenticationController : ApiController
    {
        // GET: Authenticate
        [System.Web.Http.Authorize]
        [System.Web.Http.HttpGet]
        public string Get()
        {
            try
            {
                string userID = User.Identity.Name;
                string token = CheckManageCache(userID);
                if (token != "")
                    return token;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ConfigurationManager.AppSettings["TokenUrl"] + ConfigurationManager.AppSettings["OrganizationId"]);
                request.ContentType = "application/json";
                request.Headers.Add("Authorization", "Bearer " + ConfigurationManager.AppSettings["ApiKey"]);
                string bodyContent = "{\"userIds\": [{ \"name\": \"" + userID + "\", \"provider\": \"Email Security Provider\"}]}";
                Byte[] body = Encoding.ASCII.GetBytes(bodyContent);
                request.Method = "POST";
                request.ContentLength = body.Length;

                request.GetRequestStream().Write(body, 0, body.Length);
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    string resp = reader.ReadToEnd();
                     AddManageCache(userID, resp);
                    return resp;

                }

            }
            catch (Exception )
            {
                return null;
            }

        }

       
        private string CheckManageCache(string userID)
        {
            MemoryCache memCache = MemoryCache.Default;
            var res = memCache.Get(userID);
            if (res != null)
                return res.ToString();
            else
                return "";
        }
        private void AddManageCache(string userID, string token)
        {
            MemoryCache memCache = MemoryCache.Default;
            memCache.Add(userID, token, DateTimeOffset.UtcNow.AddHours(23));
        }
    }
}
