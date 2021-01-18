using System;
using System.IO;
using System.Net;

namespace Connect4Game
{
    internal class InternetAPI
    {
        private readonly string _downloads = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/EthirixPrograms";

        public string DownloadLocation { get => _downloads; private set => throw new Exception("Cannot change Download Location as all downloads are consistent across programs."); }

        public static bool CheckForInternetConnection()
        {
            try {
                using (WebClient client = new WebClient()) {
                    using (client.OpenRead("http://clients1.google.com/generate_204")) { return true; }
                }
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
                return false;
            }
        }

        public void DownloadConnectionStatus()
        {
            using (WebClient client = new WebClient()) {
                if (!Directory.Exists(_downloads)) { Directory.CreateDirectory(_downloads); }
                if (!File.Exists(_downloads + "/redx.png")) { client.DownloadFile(new Uri("https://upload.wikimedia.org/wikipedia/commons/thumb/5/5f/Red_X.svg/480px-Red_X.svg.png"), _downloads + "/redx.png"); }
                if (!File.Exists(_downloads + "/greentick.png")) { client.DownloadFile(new Uri("https://upload.wikimedia.org/wikipedia/commons/thumb/e/e5/Green_tick_pointed.svg/480px-Green_tick_pointed.svg.png"), _downloads + "/greentick.png"); }
            }
        }
    }
}
