using System.Text;
using System.Net;
using System.IO;

namespace Requiem_Network_Launcher
{
    class CustomHttpMethod
    {
        public static HttpWebResponse Get(string url, string referer, ref CookieContainer cookies)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "GET";
            req.CookieContainer = cookies;
            req.UserAgent = "";
            req.Referer = referer;

            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            cookies.Add(resp.Cookies);

            /*
            string pageSrc;
            using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
            {
                pageSrc = sr.ReadToEnd();
            }*/

            return resp;
        }

        public static bool Post(string url, string postData, string referer, CookieContainer cookies)
        {
            string key = "The Display Name or Email Address you entered does not belong to any account. Make sure that it is typed correctly.";
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "POST";
            req.CookieContainer = cookies;
            req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36";
            req.Referer = referer;
            req.ContentType = "application/x-www-form-urlencoded";
            req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";

            Stream postStream = req.GetRequestStream();
            byte[] postBytes = Encoding.ASCII.GetBytes(postData);
            postStream.Write(postBytes, 0, postBytes.Length);
            postStream.Dispose();

            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            cookies.Add(resp.Cookies);

            StreamReader sr = new StreamReader(resp.GetResponseStream());
            string pageSrc = sr.ReadToEnd();
            sr.Dispose();
            //Console.WriteLine(pageSrc);
            return (!pageSrc.Contains(key));
        }
    }
}
