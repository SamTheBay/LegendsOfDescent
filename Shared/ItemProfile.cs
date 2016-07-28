using System.Collections.Generic;
using ProtoBuf;

namespace LegendsOfDescent
{
    [ProtoContract]
    public class ItemProfile
    {
        public ItemProfile()
        {
            this.Properties = new Dictionary<string, string>();
        }

        /// <summary>
        /// The level of the item should be drawn after the name in () and the color is based on the ItemClass value
        /// </summary>
        [ProtoMember(1)]
        public string Name { get; set; }

        /// <summary>
        /// many items have an empty string for this currently
        /// </summary>
        [ProtoMember(2)]
        public string Description { get; set; }

        [ProtoMember(3)]
        public int Level { get; set; }

        [ProtoMember(4)]
        public int IconFrame { get; set; }

        [ProtoMember(5)]
        public int ItemClass { get; set; }

        [ProtoMember(6)]
        public Dictionary<string, string> Properties { get; set; }
    }
}
