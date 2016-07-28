using System.IO;
using LegendsOfDescent;

namespace LegendsOfDescent.Quests
{
    /// <summary>
    /// Places an item on the map, quest is complete when the player finds and returns the item.
    /// Params:
    ///     dungeonLevel - the level on which to place the item
    ///     itemLevel - the level of item to create
    ///     itemName - optional, the specific item name to use; otherwise, uses a random equipable item
    ///     itemDisplayName - optional, the name of the item to display
    ///     returnToNpcName - optional, the name of the NPC to whom the item must be returned (default is QuesterSprite.MainQuesterName)
    ///     
    /// </summary>
    public class FindItemQuest : Quest
    {
        private string itemName;
        private State state = State.Created;
        private ItemSprite item;

        enum State
        {
            Created,
            Placed,
            PickedUp
        }

        public FindItemQuest()
        {
        }

        public FindItemQuest(QuestParams p)
        {
            this.DungeonLevel = p.GetInt("dungeonLevel");
            var itemLevel = p.GetInt("itemLevel");
            var itemType = p.Get("itemName");
            if (itemType != null)
            {
                this.item = ItemManager.Instance.GetItem(itemType, itemLevel);
            }
            else
            {
                this.item = ItemManager.Instance.GetBasicItem(itemLevel, MerchantType.Equipable);
            }

            if (p.ContainsKey("returnToNpcName"))
            {
                this.ReturnToNpcName = p["returnToNpcName"];
            }
            else
            {
                this.ReturnToNpcName = QuesterSprite.MainQuesterName;
            }

            var displayName = p.Get("itemDisplayName");
            this.item.Name = this.itemName = displayName ?? "The Coveted " + this.item.Name;
            this.item.Value = (int)((100 + 10 * Util.Random.Between(5f, 10f)) * DungeonLevel);
            this.item.CannotBeSoldOrDestroyed = true;
            this.reward.Experience = Util.Random.InRange(10, 15) * BalanceManager.GetExperienceForEnemyKill(this.DungeonLevel);
            this.reward.Gold = Util.Random.InRange(5, 10) * BalanceManager.GetBaseGoldDrop(this.DungeonLevel);
            this.Name = this.itemName;
            this.Description = "Find " + this.itemName + " on dungeon level " + DungeonLevel.ToString() + " and return it to " + ReturnToNpcName;
            this.InProgressText = "Let me know once you have found the " + itemName + ".";
            this.AskText = this.itemName + " was stolen during the raid of the city. " +
                "It must be located in the depths below! Can you find it and bring it back to us?";

        }

        public override void SetDungeon(DungeonLevel dungeon)
        {
            base.SetDungeon(dungeon);

            if (dungeon.Level == this.DungeonLevel && state == State.Created)
            {
                this.item.Position = this.dungeon.GetRandomOpenTile(minimumDistanceFromPlayer: 30).Position;
                this.dungeon.AddEnvItem(this.item);
                this.item.Activate();
                this.state = State.Placed;
            }
        }

        public override void ItemPickedUp(ItemSprite pickedUpItem)
        {
            if (this.state == State.Placed && pickedUpItem.Name == this.itemName)
            {
                player.QuestLog.SetQuestUpdateNotification(this);
                this.state = State.PickedUp;
                this.Description = "You've found " + this.itemName + ". Now return it to " + this.ReturnToNpcName + " to complete the quest.";            }
        }

        public override bool TalkedToNpc(DialogueNPCSprite npc)
        {
            if (!isComplete && this.state == State.PickedUp && npc.Name == this.ReturnToNpcName && player.Inventory.Remove(i => i.Name == this.itemName))
            {
                this.isComplete = true;
                return false;
            }
            return true;
        }

        public override string Status
        {
            get { return state == State.PickedUp ? "found, not returned" : "not found"; }
        }

        public override void Persist(BinaryWriter writer)
        {
            base.Persist(writer);
            writer.Write(this.itemName);
            writer.Write((byte)state);

            if (item == null)
            {
                // backward compat safety. Should never happen, but could happen when old quests transfer over to v0.4
                // However, these old quests "disapear" anyways since we move quests off the dungeons to the player
                this.item = ItemManager.Instance.GetBasicItem(1, MerchantType.Equipable);
            }

            writer.Write(item);
        }

        public override bool Load(BinaryReader reader, int dataVersion)
        {
            base.Load(reader, dataVersion);
            this.itemName = reader.ReadString();
            this.state = (State)reader.ReadByte();

            if (dataVersion <= 6) return true;

            this.item = ItemSprite.LoadItem(reader, dataVersion);

            if (dataVersion <= 8)
            {
                // this has been moved onto the quest object for version 9
                this.ReturnToNpcName = reader.ReadString();
            }   
            return true;
        }

        public override bool ReadyForReturn()
        {
            return (state == State.PickedUp && !isComplete);
        }


        public string ItemName
        {
            get { return itemName; }
        }
    }
}
