using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace LegendsOfDescent.Analytics
{
    public class ActivityLog : ISaveable
    {
        public int Id { get; set; }
        public string DeviceUniqueId { get; set; }
        public string AnonymousUserId { get; set; }
        public Guid ApplicationId { get; set; }
        public string ApplicationVersion { get; set; }
        public Platform Platform { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int Type { get; set; } // Use ActivityType.X (Entity Framework does not support enums...lame!)
        public string Value { get; set; }

        public ActivityLog()
        {
            Value = string.Empty;
        }

        public void Persist(BinaryWriter writer)
        {
            writer.Write(Id);
            writer.Write(DeviceUniqueId);
            writer.Write(AnonymousUserId);
            writer.Write(ApplicationId);
            writer.Write(ApplicationVersion);
            writer.Write(StartTime);
            writer.Write(EndTime);
            writer.Write(Type);
            writer.Write(Value);
        }

        public bool Load(BinaryReader reader, int dataVersion)
        {
            Id = reader.ReadInt32();
            DeviceUniqueId = reader.ReadString();
            AnonymousUserId = reader.ReadString();
            ApplicationId = reader.ReadGuid();
            ApplicationVersion = reader.ReadString();
            StartTime = reader.ReadDateTime();
            EndTime = reader.ReadDateTime();
            Type = reader.ReadInt32();
            Value = reader.ReadString();
            return true;
        }
    }
}