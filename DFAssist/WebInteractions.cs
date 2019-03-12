using System;
using System.Net;

namespace DFAssist
{
    public static class WebInteractions
    {
        public static string DownloadString(string url)
        {
            try
            {
                var webClient = new WebClient();
                webClient.Headers.Add("user-agent", "avoid 403");
                webClient.Encoding = System.Text.Encoding.UTF8;
                var downloadString = webClient.DownloadString(url);
                webClient.Dispose();
                return downloadString;
            }
            catch (Exception e)
            {
                Logger.Exception(e, "l-data-error");
            }

            return string.Empty;
        }
    }
}