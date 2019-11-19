using System.IO;
using System.Net;

namespace SchTech.Web.Manager.Abstract
{
    public interface IWebClientService
    {
        int RequestStatusCode { get; set; }

        string WebErrorMessage { get; set; }

        HttpWebResponse WebClientResponse { get; set; }

        HttpWebRequest WebClientRequest { get; set; }

        StreamReader StreamReader { get; set; }

        string WebUserAgentString { get; set; }

        bool SuccessfulWebRequest { get; set; }

        bool IsDisposed { get; set; }
        void Dispose();

        string HttpGetRequest(string url, bool followRedirect = true);

        bool DownloadWebBasedFile(string fileUrl, bool useOriginalFileName = true, string newFileName = null,
            string localSaveDirectory = null);


        string HttpPostRequest(string url, string post, bool followRedirect = true, string refer = "");

        void DebugHtml(string html);
    }
}