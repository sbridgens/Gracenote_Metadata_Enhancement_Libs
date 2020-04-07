using log4net;
using SchTech.Web.Manager.Abstract;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace SchTech.Web.Manager.Concrete
{
    public class WebClientManager : IDisposable, IWebClientService
    {
        /// <summary>
        ///     Initialize Log4net
        /// </summary>
        private static readonly ILog Log =
            LogManager.GetLogger(typeof(WebClientManager));

        #region Properties

        public HttpWebResponse WebClientResponse { get; set; }
        public HttpWebRequest WebClientRequest { get; set; }
        public StreamReader StreamReader { get; set; }
        public string WebUserAgentString { get; set; }
        public bool SuccessfulWebRequest { get; set; }
        public string WebErrorMessage { get; set; }
        public int RequestStatusCode { get; set; }
        private const int MaxWebRetries = 5;
        private int CurrentRetryCount { get; set; }

        private readonly CookieContainer _cJar;

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
            _cJar = new CookieContainer();
        }
        
        public string HttpGetRequest(string url, bool followRedirect = true)
        {
            RequestStatusCode = 0;
            CurrentRetryCount = 0;
            WebClientRequest = (HttpWebRequest)WebRequest.Create(url);
            WebClientRequest.CookieContainer = _cJar;
            WebClientRequest.UserAgent = WebUserAgentString;
            WebClientRequest.Accept = "*/*";
            WebClientRequest.KeepAlive = false;
            WebClientRequest.Method = "GET";

            if (followRedirect) WebClientRequest.AllowAutoRedirect = false;

            while (!CheckWebResponse())
            {
                Log.Error("Exception during http GET request - status code: " +
                          $"{(int)WebClientResponse.StatusCode}, " +
                          $"status string: {WebClientResponse.StatusCode} " +
                          $"{WebClientResponse.StatusDescription}");

                Thread.Sleep(5000);

                CurrentRetryCount++;
                Log.Info($"HTTP Get Retry: {CurrentRetryCount} of {MaxWebRetries}");
                if (CurrentRetryCount >= MaxWebRetries)
                    return string.Empty;
            }
            

            if (followRedirect && (RequestStatusCode == (int)HttpStatusCode.Moved ||
                                   RequestStatusCode == (int)HttpStatusCode.Found))
                while (WebClientResponse.StatusCode == HttpStatusCode.Found ||
                       WebClientResponse.StatusCode == HttpStatusCode.Moved)
                {
                    WebClientResponse.Close();
                    WebClientRequest = (HttpWebRequest)WebRequest.Create(WebClientResponse.Headers["Location"]);
                    WebClientRequest.AllowAutoRedirect = false;
                    WebClientRequest.CookieContainer = _cJar;
                    WebClientResponse = (HttpWebResponse)WebClientRequest.GetResponse();
                }

            StreamReader = new StreamReader(WebClientResponse.GetResponseStream()
                                            ?? throw new InvalidOperationException());

            RequestStatusCode = (int)WebClientResponse.StatusCode;
            var responseData = StreamReader.ReadToEnd();

            return responseData;

        }

        public bool DownloadWebBasedFile(string fileUrl, bool useOriginalFileName = true, string newFileName = null,
            string localSaveDirectory = null)
        {
            using (var client = new WebClient())
            {
                var uri = new Uri(fileUrl);

                var fileName = useOriginalFileName && string.IsNullOrEmpty(newFileName)
                    ? $"{localSaveDirectory}\\{Path.GetFileName(uri.LocalPath)}"
                    : newFileName;

                if (fileName != null)
                    client.DownloadFile(uri, fileName);
            }

            return true;
        }

        public string HttpPostRequest(string url, string post, bool followRedirect = true, string refer = "")
        {
            WebClientRequest = (HttpWebRequest)WebRequest.Create(url);
            WebClientRequest.CookieContainer = _cJar;
            WebClientRequest.UserAgent = WebUserAgentString;
            WebClientRequest.KeepAlive = false;
            WebClientRequest.Method = "POST";
            WebClientRequest.Referer = refer;

            if (followRedirect)
                WebClientRequest.AllowAutoRedirect = false;

            var postBytes = Encoding.ASCII.GetBytes(post);
            WebClientRequest.ContentType = "text/x-www-form-urlencoded";
            WebClientRequest.ContentLength = postBytes.Length;

            var requestStream = WebClientRequest.GetRequestStream();
            requestStream.Write(postBytes, 0, postBytes.Length);
            requestStream.Close();

            WebClientResponse = (HttpWebResponse)WebClientRequest.GetResponse();

            if (followRedirect && (WebClientResponse.StatusCode == HttpStatusCode.Moved
                                   || WebClientResponse.StatusCode == HttpStatusCode.Found))
                while (WebClientResponse.StatusCode == HttpStatusCode.Found
                       || WebClientResponse.StatusCode == HttpStatusCode.Moved)
                {
                    WebClientResponse.Close();
                    WebClientRequest = (HttpWebRequest)WebRequest.Create(WebClientResponse.Headers["Location"]);
                    WebClientRequest.AllowAutoRedirect = false;
                    WebClientRequest.CookieContainer = _cJar;
                    WebClientResponse = (HttpWebResponse)WebClientRequest.GetResponse();
                }

            StreamReader = new StreamReader(WebClientResponse.GetResponseStream()
                                            ?? throw new InvalidOperationException());

            return StreamReader.ReadToEnd();
        }

        public void DebugHtml(string html)
        {
            var sw = new StreamWriter("debug.html");
            sw.Write(html);
            sw.Close();
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

                SuccessfulWebRequest = true;
            }
            catch (Exception cwrException)
            {
                Log.Error($"Http Get Exception: {cwrException.Message}");
                if (cwrException.InnerException != null)
                    Log.Error($"Inner Exception: {cwrException.InnerException.Message}");
                return false;
            }

            return SuccessfulWebRequest;
        }
        

    }
}