using System;
using System.Collections.Generic;
using ProtoBuf;

namespace LegendsOfDescent
{
    [ProtoContract]
    public class GamerProfile
    {
        [ProtoMember(1)]
        public List<PlayerProfile> Players { get; set; }
    }
}
