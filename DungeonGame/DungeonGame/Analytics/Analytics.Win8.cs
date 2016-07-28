using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using LegendsOfDescent.Analytics;
using Microsoft.Xna.Framework;
using ProtoBuf;
using System.Threading;

using Windows.Devices.Enumeration.Pnp;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Security.Cryptography.Core;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.System.Profile;

namespace LegendsOfDescent
{
    public class AnalyticsManagerWin8 : AnalyticsManager
    {
        public static async void Initialize()
        {
            session = new Session();
            BeginSession();

            App.Current.Suspending += (s, e) => End();
            App.Current.UnhandledException += (s, a) =>
            {
                session.UnhandledException = new ExceptionInfo(a.Exception);
                End();
            };
        }

        private static class PnpProperty
        {
            public const string ModelName = "System.Devices.ModelName";
            public const string Manufacturer = "System.Devices.Manufacturer";
        }

        private static void BeginSession()
        {
            session.StartTime = DateTime.UtcNow;
            // Do everything in Augment instead, so we know that we have finished before saving the file
        }

        public static async new void Augment()
        {
            await LoadManifest();

            session.SessionGuid = Guid.NewGuid();
            session.Platform = Platform.WindowsMetro;

            session.OsVersion = await GetOsVersionAsync();
            session.TimeZoneOffsetFromUtcInMinutes = (short)TimeZoneInfo.Local.BaseUtcOffset.TotalMinutes;

            session.DeviceUniqueId = Convert.ToBase64String(GetSha1Hash(HardwareIdentification.GetPackageSpecificToken(null).Id));

            string[] properties = { PnpProperty.ModelName, PnpProperty.Manufacturer };
            var rootContainer = await PnpObject.CreateFromIdAsync(PnpObjectType.DeviceContainer, "{00000000-0000-0000-FFFF-FFFFFFFFFFFF}", properties);
            string modelName = (string)rootContainer.Properties[PnpProperty.ModelName];
            string manufacturer = (string)rootContainer.Properties[PnpProperty.Manufacturer];

            session.DeviceManufacturer = manufacturer;
            session.DeviceName = modelName;

            AnalyticsManager.Augment();
        }

        public static async Task LoadManifest()
        {
            var xml = await FileIO.ReadTextAsync(await Package.Current.InstalledLocation.GetFileAsync("AppxManifest.xml"));
            var doc = XDocument.Load(new StringReader(xml));
            var idNode = doc.Descendants().ToArray()[1];
            session.ApplicationId = GetMd5HashAsGuid(idNode.Attribute("Name").Value);
            session.ApplicationVersion = idNode.Attribute("Version").Value;
        }

        /// <summary>
        /// http://www.michielpost.nl/PostDetail_74.aspx
        /// </summary>
        /// <returns></returns>
        private static async Task<string> GetOsVersionAsync()
        {

            string userAgent = await GetUserAgent();

            string result = string.Empty;

            //Parse user agent
            int startIndex = userAgent.ToLower().IndexOf("windows");
            if (startIndex > 0)
            {
                int endIndex = userAgent.IndexOf(";", startIndex);

                if (endIndex > startIndex)
                {
                    result = userAgent.Substring(startIndex, endIndex - startIndex);
                }

            }

            return result;
        }

        private static Task<string> GetUserAgent()
        {

            var tcs = new TaskCompletionSource<string>();
            WebView webView = new WebView();

            string htmlFragment = @"<html>

                    <head>
                        <script type='text/javascript'>
                            function GetUserAgent()
                            {
                                return navigator.userAgent;
                            }
                        </script>
                    </head>
                </html>";
            webView.LoadCompleted += (sender, e) =>
            {

                try
                {
                    //Invoke the javascript when the html load is complete
                    string result = webView.InvokeScript("GetUserAgent", null);

                    //Set the task result
                    tcs.TrySetResult(result);

                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }

            };

            //Load Html
            webView.NavigateToString(htmlFragment);

            return tcs.Task;
        }

        private static byte[] GetSha1Hash(IBuffer buffer)
        {
            var alg = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha1);
            var hashed = alg.HashData(buffer);
            byte[] bytes;
            CryptographicBuffer.CopyToByteArray(hashed, out bytes);
            return bytes;
        }

        private static byte[] GetMd5Hash(IBuffer buffer)
        {
            var alg = HashAlgorithmProvider.OpenAlgorithm("MD5");
            var hashed = alg.HashData(buffer);
            byte[] bytes;
            CryptographicBuffer.CopyToByteArray(hashed, out bytes);
            return bytes;
        }

        private static Guid GetMd5HashAsGuid(IBuffer buffer)
        {
            return new Guid(GetMd5Hash(buffer));
        }

        private static Guid GetMd5HashAsGuid(string value)
        {
            return GetMd5HashAsGuid(CryptographicBuffer.ConvertStringToBinary(value, BinaryStringEncoding.Utf8));
        }
    }
}