using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LegendsOfDescent
{
    public interface ISaveable
    {
        void Persist(BinaryWriter writer);
        bool Load(BinaryReader reader, int dataVersion);
    }
}
