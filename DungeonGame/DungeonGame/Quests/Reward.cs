using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;

namespace LegendsOfDescent.Quests
{
    public class Reward : ISaveable
    {
        public int Gold { get; set; }
        public int Experience { get; set; }

        public void GiveTo(PlayerSprite player)
        {
            if (Gold > 0)
            {
                player.AddGold(Gold);
                ParticleSystem.AddParticles(player.CenteredPosition, ParticleType.Starburst, color: Color.Goldenrod);
            }

            if (Experience > 0)
            {
                player.AddExperience(Experience);
            }
        }

        public void Persist(BinaryWriter writer)
        {
            writer.Write(this.Gold);
            writer.Write(this.Experience);
        }

        public bool Load(BinaryReader reader, int dataVersion)
        {
            this.Gold = reader.ReadInt32();
            this.Experience = reader.ReadInt32();
            return true;
        }

        public override string ToString()
        {
            var list = new List<string>(2);
            if (Gold > 0) list.Add(Gold + " gold");
            if (Experience > 0) list.Add(Experience + " exp");
            return list.Count == 0 ? "none" : string.Join(", ", list.ToArray());
        }
    }
}
