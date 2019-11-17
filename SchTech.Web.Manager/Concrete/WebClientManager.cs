using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SchTech.Web.Manager.Abstract;

namespace SchTech.Web.Manager.Concrete
{
    public class WebClientManager : IDisposable, IWebClientService
    {
        /// <summary>
        /// Initialize Log4net
        /// </summary>
        private static readonly log4net.ILog Log = 
            log4net.LogManager.GetLogger(typeof(WebClientManager));

        #region Properties

        public int RequestStatusCode { get; set; }
        public string WebErrorMessage { get; set; }
        public HttpWebResponse WebClientResponse { get; set; }
        public HttpWebRequest WebClientRequest { get; set; }
        public StreamReader StreamReader { get; set; }
        public string WebUserAgentString { get; set; }
        public bool SuccessfulWebRequest { get; set; }

        private int WebRetries = 5;

        private CookieContainer cJar;

        #endregion

        #region IDisposableFunctions

        public bool IsDisposed { get; set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.
                //
            }

            // Free any unmanaged objects here.
            //
            IsDisposed = true;
        }

        ~WebClientManager()
        {
            Dispose(false);
        }

        #endregion

        public WebClientManager()
        {
            cJar = new CookieContainer();
        }

        private bool CheckWebResponse()
        {
            try
            {
                WebClientResponse = (HttpWebResponse)WebClientRequest.GetResponse();

                RequestStatusCode = (int)WebClientResponse.StatusCode;
                if (RequestStatusCode != (int)HttpStatusCode.OK)
                {
                    SuccessfulWebRequest = false;
                    throw new Exception($"Api Error: Request status code does not match 200OK: {RequestStatusCode}");
                }
                else
                {
                    SuccessfulWebRequest = true;
                }
            }
            catch (Exception cwrException)
            {
                if (WebRetries <= 5)
                {
                    WebRequestRetries();
                }
                else
                {
                    Log.Error($"Http Get Exception: {cwrException.Message}");
                    if (cwrException.InnerException != null)
                    {
                        Log.Error($"Inner Exception: {cwrException.InnerException.Message}");
                    }
                    SuccessfulWebRequest = false;
                }
            }

            return SuccessfulWebRequest;
        }

        private void WebRequestRetries()
        {
            for (int r = 1; r <= WebRetries; r++)
            {
                Thread.Sleep(2000);

                Log.Info($"HTTP Get Retry: {r} of {WebRetries}");
                CheckWebResponse();
            }
        }

        public string HttpGetRequest(string url, bool followRedirect = true)
        {
            RequestStatusCode = 0;
            WebClientRequest = (HttpWebRequest)WebRequest.Create(url);
            WebClientRequest.CookieContainer = cJar;
            WebClientRequest.UserAgent = WebUserAgentString;
            WebClientRequest.Accept = "*/*";
            WebClientRequest.KeepAlive = false;
            WebClientRequest.Method = "GET";

            if (followRedirect)
            {
                WebClientRequest.AllowAutoRedirect = false;
            }

            if (CheckWebResponse())
            {
                if (followRedirect && (RequestStatusCode == (int)HttpStatusCode.Moved ||
                                        RequestStatusCode == (int)HttpStatusCode.Found))
                {
                    while (WebClientResponse.StatusCode == HttpStatusCode.Found ||
                           WebClientResponse.StatusCode == HttpStatusCode.Moved)
                    {
                        WebClientResponse.Close();
                        WebClientRequest = (HttpWebRequest)HttpWebRequest.Create(WebClientResponse.Headers["Location"]);
                        WebClientRequest.AllowAutoRedirect = false;
                        WebClientRequest.CookieContainer = cJar;
                        WebClientResponse = (HttpWebResponse)WebClientRequest.GetResponse();
                    }
                }

                StreamReader = new StreamReader(WebClientResponse.GetResponseStream() 
                                                ?? throw new InvalidOperationException());

                RequestStatusCode = (int)WebClientResponse.StatusCode;
                var responseData = StreamReader.ReadToEnd();

                return responseData;
            }

            throw new Exception($"Exception during http get call - status code: " +
                                $"{(int)WebClientResponse.StatusCode}, " +
                                $"status string: {WebClientResponse.StatusCode} " +
                                $"{WebClientResponse.StatusDescription}");
        }

        public bool DownloadWebBasedFile(string fileUrl, bool useOriginalFileName = true, string newFileName = null,
            string localSaveDirectory = null)
        {
            using (WebClient client = new WebClient())
            {
                Uri uri = new Uri(fileUrl);

                string fileName = useOriginalFileName == true && string.IsNullOrEmpty(newFileName)
                    ? fileName = $"{localSaveDirectory}\\{Path.GetFileName(uri.LocalPath)}"
                    : newFileName;

                client.DownloadFile(uri, fileName);
            }

            return true;
        }

        public string HttpPostRequest(string url, string post, bool followRedirect = true, string refer = "")
        {
            WebClientRequest = (HttpWebRequest)WebRequest.Create(url);
            WebClientRequest.CookieContainer = cJar;
            WebClientRequest.UserAgent = WebUserAgentString;
            WebClientRequest.KeepAlive = false;
            WebClientRequest.Method = "POST";
            WebClientRequest.Referer = refer;

            if (followRedirect)
                WebClientRequest.AllowAutoRedirect = false;

            byte[] postBytes = Encoding.ASCII.GetBytes(post);
            WebClientRequest.ContentType = "text/x-www-form-urlencoded";
            WebClientRequest.ContentLength = postBytes.Length;

            Stream requestStream = WebClientRequest.GetRequestStream();
            requestStream.Write(postBytes, 0, postBytes.Length);
            requestStream.Close();

            WebClientResponse = (HttpWebResponse)WebClientRequest.GetResponse();

            if (followRedirect && (WebClientResponse.StatusCode == HttpStatusCode.Moved
                                    || WebClientResponse.StatusCode == HttpStatusCode.Found))
            {
                while (WebClientResponse.StatusCode == HttpStatusCode.Found 
                       || WebClientResponse.StatusCode == HttpStatusCode.Moved)
                {
                    WebClientResponse.Close();
                    WebClientRequest = (HttpWebRequest)WebRequest.Create(WebClientResponse.Headers["Location"]);
                    WebClientRequest.AllowAutoRedirect = false;
                    WebClientRequest.CookieContainer = cJar;
                    WebClientResponse = (HttpWebResponse)WebClientRequest.GetResponse();
                }
            }

            StreamReader = new StreamReader(WebClientResponse.GetResponseStream() 
                                            ?? throw new InvalidOperationException());

            return StreamReader.ReadToEnd();
        }

        public void DebugHtml(string html)
        {
            StreamWriter sw = new StreamWriter("debug.html");
            sw.Write(html);
            sw.Close();
        }
    }
}
