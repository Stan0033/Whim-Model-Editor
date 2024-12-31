using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
namespace MDLLibs.Classes.Misc
{
    internal static class UpdateChecker
    {
        
        internal static void OpenPage(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                return;
            }
        }
        private static async Task<string> GetHtmlContentAsync(string url)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string htmlContent = await client.GetStringAsync(url);
                    return htmlContent;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return "";
            }
        }
        public static async Task<bool> IsInternetAvailableAsync()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(5);
                    HttpResponseMessage response = await client.GetAsync("http://www.google.com");
                    return response.IsSuccessStatusCode;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
