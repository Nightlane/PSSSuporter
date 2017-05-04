using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.IO;
using System.Xml.Linq;

namespace PSSSupporter
{
    public static class httpCommunications
    {
        public static string userAgent = "PixelStarships/1 CFNetwork/808.1.4 Darwin/16.1.0";
        public static Dictionary<string, object> getParsedPostContent(string url) {
            var doc = XDocument.Parse(getContent(url, "POST", null));
            return (Dictionary<string, object>)xml2Dictionary.Parse(doc.Root);
        }
        public static Dictionary<string, object> getParsedContent(string url) {
            var doc = XDocument.Parse(getContent(url, "GET", null));
            return (Dictionary<string, object>)xml2Dictionary.Parse(doc.Root);
        }
        public static string getPostContent(string url)
        {
            return getContent(url, "POST", null);
        }
        public static string getPostContent(string url, string contentToSend) {
            //Console.WriteLine();
            //XElement test = XElement.Parse(contentToSend);
            //Console.WriteLine(contentToSend);
            return getContent(url,"POST", contentToSend);
        }
        public static string getContent(string url)
        {
            return getContent(url, "GET", null);
        }
        public static string getContent(string url, string method, string contentToSend)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            //webRequest.Proxy = new WebProxy("192.168.1.130:8888");
            webRequest.Method = method;
            webRequest.UserAgent = userAgent;
            if (contentToSend != null) {
                byte[] buffer = Encoding.ASCII.GetBytes(contentToSend);
                webRequest.ContentLength = buffer.Length;
                var requestStream = webRequest.GetRequestStream();
                requestStream.Write(buffer,0,buffer.Length);
                requestStream.Close();
            }
            // webRequest.Headers.Add("User-Agent: PixelStarships/1 CFNetwork/808.1.4 Darwin/16.1.0");
            using (var response = webRequest.GetResponse())
            using (var content = response.GetResponseStream())
            using (var reader = new StreamReader(content))
            {
                string responseContent = reader.ReadToEnd();
                return responseContent;
            }
        }
    }
}
