using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using LegendsOfDescent.Analytics;
using Microsoft.Xna.Framework;
using ProtoBuf;
using System.Threading;

#if WINDOWS_PHONE
using System.Windows;
using Microsoft.Phone.Info;
using Microsoft.Phone.Net.NetworkInformation;
#elif !WIN8
using System.Windows.Forms;
using System.Net.NetworkInformation;
using System.Diagnostics;
#endif

namespace LegendsOfDescent
{
    public class AnalyticsManager
    {
        private static readonly string SessionFileName = "sessions.dat";
        //private static readonly string SessionUrl = "http://localhost:50539/analytics/uploadsession";
        private static readonly string SessionUrl = "http://legendsofdescent.com/analytics/uploadsession";

        protected static Session session = null;

#if WINDOWS_PHONE
        public static void Initialize(Game game)
        {
            BeginSession();
            game.Deactivated += (s, e) => End();
            Application.Current.UnhandledException += (s, e) =>
            {
                session.UnhandledException = new ExceptionInfo(e.ExceptionObject);
                End();
            };
        }
#elif !WIN8
        public static void Initialize(Game game)
        {
            BeginSession();
            game.Exiting += (s, e) => End();
            AppDomain.CurrentDomain.UnhandledException += (s, a) =>
            {
                session.UnhandledException = new ExceptionInfo(a.ExceptionObject as Exception);
                End();
            };  
        }
#endif
        
        private static string defaultAnid = "A=" + Guid.Empty;

        /// <summary>
        /// Called after SaveGameManager is initialized, so we can collect the rest of the info
        /// </summary>
        public static void Augment()
        {
            session.IsPaidVersion = DungeonGame.paid;
            session.IsTrialMode = DungeonGame.IsTrialModeCached;
            session.TotalPlays = SaveGameManager.TotalPlays;

            SaveSession(session);

            TryUploadFile(SessionUrl, SessionFileName);
        }

#if WINDOWS_PHONE
        private static void BeginSession()
        {
            session = new Session()
            {
                SessionGuid = Guid.NewGuid(),
                ApplicationId = ApplicationId,
                ApplicationVersion = ApplicationVersion,
                StartTime = DateTime.UtcNow,
                OsVersion  = Environment.OSVersion.ToString(),
                TimeZoneOffsetFromUtcInMinutes = (short)TimeZoneInfo.Local.BaseUtcOffset.TotalMinutes,

                Platform = Platform.WindowsPhone,
                DeviceUniqueId = Convert.ToBase64String((byte[])DeviceExtendedProperties.GetValue("DeviceUniqueId")),
                AnonymousUserId = (UserExtendedProperties.GetValue("ANID") as string ?? defaultAnid).Substring(2, 32),
                ApplicationMemoryUsageLimit = DeviceStatus.ApplicationMemoryUsageLimit,
                DeviceFirmwareVersion = DeviceStatus.DeviceFirmwareVersion,
                DeviceHardwareVersion = DeviceStatus.DeviceHardwareVersion,
                DeviceManufacturer  = DeviceStatus.DeviceManufacturer,
                DeviceName = DeviceStatus.DeviceName,
                DeviceTotalMemory = DeviceStatus.DeviceTotalMemory,
                IsKeyboardDeployed = DeviceStatus.IsKeyboardDeployed,
                IsKeyboardPresent = DeviceStatus.IsKeyboardPresent,
                HasExternalPower = DeviceStatus.PowerSource == PowerSource.External,
                IsNetworkAvailable = DeviceNetworkInformation.IsNetworkAvailable,
                MobileOperator = DeviceNetworkInformation.CellularMobileOperator,
                IsCellularDataEnabled = DeviceNetworkInformation.IsCellularDataEnabled,
                IsWiFiEnabled = DeviceNetworkInformation.IsWiFiEnabled,
            };

            if (session.IsNetworkAvailable)
            {
                DeviceNetworkInformation.ResolveHostNameAsync(
                    new DnsEndPoint("microsoft.com", 80),
                    new NameResolutionCallback(nrr =>
                    {
                        var info = nrr.NetworkInterface;
                        session.NetworkInterfaceType = (byte)info.InterfaceType;
                        session.NetworkInterfaceSubType = (byte)info.InterfaceSubtype;
                    }), null);
            }
        }
        
        private static void EndSession()
        {
            session.IsPortrait = DungeonGame.portraitMode;
            session.EndTime = DateTime.UtcNow;
            session.ApplicationPeakMemoryUsage = DeviceStatus.ApplicationPeakMemoryUsage;
        }
#elif !WIN8
        private static void BeginSession()
        {
            session = new Session()
            {
                SessionGuid = Guid.NewGuid(),
                ApplicationId = ApplicationId,
                ApplicationVersion = ApplicationVersion,
                StartTime = DateTime.UtcNow,
                OsVersion  = Environment.OSVersion.ToString(),
                TimeZoneOffsetFromUtcInMinutes = (short)TimeZoneInfo.Local.BaseUtcOffset.TotalMinutes,

                Platform = Platform.Pc,
                DeviceUniqueId = Environment.MachineName,
                AnonymousUserId = Environment.UserName,
                HasExternalPower = SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Online,
                ProcessorCount = Environment.ProcessorCount,
                IsNetworkAvailable = NetworkInterface.GetIsNetworkAvailable(),
                DeviceTotalMemory = (long)new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory
            };
        }
        
        private static void EndSession()
        {
            session.IsPortrait = DungeonGame.portraitMode;
            session.EndTime = DateTime.UtcNow;
            session.ApplicationPeakMemoryUsage = Process.GetCurrentProcess().PeakWorkingSet64;
        }
#else
        private static void EndSession()
        {
            session.IsPortrait = DungeonGame.portraitMode;
            session.EndTime = DateTime.UtcNow;
        }
#endif

        protected static void End()
        {
            if (session == null)
            {
                return; // Should not happen, GameActivate should always be called
            }

            EndSession();

            session.GamerProfileSnapshot = GetGamerProfile();

            SaveSession(session);
        }

        protected static void SaveSession(Session session)
        {
            using (var stream = Storage.OpenAppend(SessionFileName))
            {
                Serializer.SerializeWithLengthPrefix(stream, session, PrefixStyle.Base128, 0);
            }
        }

        private static GamerProfile GetGamerProfile()
        {
            if (!DungeonGame.initialLoadComplete)
                return null;

            return new GamerProfile()
            {
                Players = SaveGameManager.GetPlayerProfiles()
            };
        }

        protected static void TryUploadFile(string url, string fileName)
        {
            if (!Storage.FileExists(fileName))
            {
                return;
            }

            TryUpload(
                url,
                requestStream =>
                {
                    using (var fileStream = Storage.OpenRead(fileName))
                    {
                        fileStream.CopyTo(requestStream);
                    }
                },
                () => Storage.DeleteFile(fileName),
                null);
        }

        private static void TryUpload(string url, Action<Stream> onRequestStream, Action onSuccess, Action<Exception> onError)
        {
            HttpWebRequest request = HttpWebRequest.Create(url) as HttpWebRequest;
            request.Method = "POST";
            request.Accept = "application/json";
            request.ContentType = "application/json";
            request.BeginGetRequestStream(new AsyncCallback(a =>
            {
                try
                {
                    using (var requestStream = request.EndGetRequestStream(a))
                    {
                        onRequestStream(requestStream);
                    }

                    request.BeginGetResponse(new AsyncCallback(ar =>
                    {
                        try
                        {
                            var response = request.EndGetResponse(ar) as HttpWebResponse;
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                if (onSuccess != null) onSuccess();
                            }
                        }
                        catch (WebException ex) { if (onError != null) onError(ex); } // Server could not be contacted
                    }), null);
                }
                catch (WebException ex) { if (onError != null) onError(ex); } // Server could not be contacted
            }), null);
        }

        static AnalyticsManager()
        {
#if !WIN8
            ReadWMAppManifest();
#endif
        }

        private static void ReadWMAppManifest()
        {
            using (var strm = Storage.TitleStream("WMAppManifest.xml") ?? Storage.TitleStream("Properties/WMAppManifest.xml"))
            {
                var xml = XElement.Load(strm);
                var app = xml.Descendants("App").Single();

                var prodId = app.Attribute("ProductID").Value;

                ApplicationId = string.IsNullOrEmpty(prodId) ? Guid.Empty : new Guid(prodId);

                var version = app.Attribute("Version").Value;

                ApplicationVersion = version ?? string.Empty;
            }
        }

        public static Guid ApplicationId { get; private set; }

        public static string ApplicationVersion { get; private set; }
    }
}