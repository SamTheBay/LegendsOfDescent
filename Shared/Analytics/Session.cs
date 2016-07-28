using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;
using PetaPoco;

namespace LegendsOfDescent.Analytics
{
    [ProtoContract, TableName("Sessions"), PrimaryKey("SessionId", autoIncrement = true)]
    public class Session
    {
        /// <summary>
        /// Database primary key
        /// </summary>
        public int SessionId { get; set; }

        [ProtoMember(1)]
        public Guid SessionGuid { get; set; }

        [ProtoMember(2)]
        public Platform Platform { get; set; }
        [ProtoMember(3)]
        public Guid ApplicationId { get; set; }
        [ProtoMember(4)]
        public string ApplicationVersion { get; set; }
        [ProtoMember(5)]
        public bool IsPaidVersion { get; set; }

        [ProtoMember(6)]
        public string DeviceUniqueId { get; set; }
        [ProtoMember(7)]
        public string AnonymousUserId { get; set; }

        [ProtoMember(8)]
        public DateTime StartTime { get; set; }
        [ProtoMember(9)]
        public DateTime EndTime { get; set; }

        [ProtoMember(10)]
        public string DeviceManufacturer { get; set; }
        [ProtoMember(12)]
        public string DeviceName { get; set; }
        [ProtoMember(13)]
        public string DeviceFirmwareVersion { get; set; }
        [ProtoMember(14)]
        public string OsVersion { get; set; }
        [ProtoMember(15)]
        public string DeviceHardwareVersion { get; set; }
        [ProtoMember(16)]
        public long DeviceTotalMemory { get; set; }
        [ProtoMember(17)]
        public bool IsKeyboardPresent { get; set; }
        [ProtoMember(18)]
        public bool IsKeyboardDeployed { get; set; }

        [ProtoMember(19)]
        public string MobileOperator { get; set; }
        [ProtoMember(20)]
        public bool IsCellularDataEnabled { get; set; }
        [ProtoMember(21)]
        public bool IsWiFiEnabled { get; set; }
        [ProtoMember(22)]
        public bool IsNetworkAvailable { get; set; }
        [ProtoMember(23)]
        public byte NetworkInterfaceType { get; set; }
        [ProtoMember(24)]
        public byte NetworkInterfaceSubType { get; set; }

        /// <summary>
        /// Assigned at the server.
        /// </summary>
        public string ExternalIpAddress { get; set; }

        [ProtoMember(25)]
        public bool HasExternalPower { get; set; }

        [ProtoMember(26)]
        public bool IsPortrait { get; set; }

        [ProtoMember(27)]
        public long ApplicationPeakMemoryUsage { get; set; }
        [ProtoMember(28)]
        public long ApplicationMemoryUsageLimit { get; set; }

        [ProtoMember(29), Ignore]
        public ExceptionInfo UnhandledException { get; set; }

        /// <summary>
        /// Only in database.
        /// </summary>
        public string UnhandledExceptionJson { get; set; }

        [ProtoMember(30)]
        public int ProcessorCount { get; set; }

        [ProtoMember(31)]
        public bool IsTrialMode { get; set; }

        [ProtoMember(32)]
        public short TimeZoneOffsetFromUtcInMinutes { get; set; }

        [ProtoMember(33), Ignore]
        public GamerProfile GamerProfileSnapshot { get; set; }

        /// <summary>
        /// Only in database.
        /// </summary>
        public byte[] GamerProfileSnapshotBin { get; set; }

        [ProtoMember(34)]
        public int TotalPlays { get; set; }
    }
}
