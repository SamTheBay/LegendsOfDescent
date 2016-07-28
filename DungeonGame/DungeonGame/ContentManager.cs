using System.Collections.Generic;
using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace LegendsOfDescent
{
    public class ContentHolder
    {
        public string ContentName;
        public Texture2D Texture;
    }

    class InternalContentManager
    {
        static Dictionary<string, ContentHolder> contextDictionary = new Dictionary<string, ContentHolder>();
        static ContentManager playerTextureContent = null;
        static string lastLoadedPlayerTextures;

        public static void Load()
        {
            // load all of the sprites
            LoadContentHolder("Blank", "Blank_Sprite", null);
            LoadContentHolder("Clear", "Clear_Sprite", null);
            LoadContentHolder("Grayout", "greyout", null);

            // projectiles
            LoadContentHolder("Fireball", "Projectiles/LargeFireball", null);
            LoadContentHolder("Firebolt", "Projectiles/Fireball", null);
            LoadContentHolder("IceBolt", "Projectiles/IceBolt", null);
            LoadContentHolder("PoisonBall", "Projectiles/PoisonBall", null);
            LoadContentHolder("Arrow", "Projectiles/Arrow", null);
            LoadContentHolder("Traps", "Projectiles/Traps", null);
            LoadContentHolder("Lightning", "Projectiles/Lightning", null);

            // Icons
            LoadContentHolder("CelticSide32", "UI/celticSide32", null);
            LoadContentHolder("CelticSide40", "UI/celticSide40", null);
            LoadContentHolder("CelticSide50", "UI/celticSide50", null);
            LoadContentHolder("CelticSide55", "UI/celticSide55", null);
            LoadContentHolder("CelticSide60", "UI/celticSide60", null);
            LoadContentHolder("CelticSide70", "UI/celticSide70", null);
            LoadContentHolder("CelticSide75", "UI/celticSide75", null);
            LoadContentHolder("CelticSide32Select", "UI/celticSide32Select", null);
            LoadContentHolder("CelticSide40Select", "UI/celticSide40Select", null);
            LoadContentHolder("CelticSide50Select", "UI/celticSide50Select", null);
            LoadContentHolder("CelticSide55Select", "UI/celticSide55Select", null);
            LoadContentHolder("CelticSide60Select", "UI/celticSide60Select", null);
            LoadContentHolder("CelticSide70Select", "UI/celticSide70Select", null);
            LoadContentHolder("CelticSide75Select", "UI/celticSide75Select", null);
            LoadContentHolder("AbilityIcons", "Abilities/AbilityIcons", null);
            LoadContentHolder("Swap", "UI/Swap", null);
            LoadContentHolder("SwapSelected", "UI/SwapSelected", null);
            LoadContentHolder("TownButton", "UI/town", null);
            LoadContentHolder("TownSelected", "UI/townSelected", null);
            LoadContentHolder("Help", "UI/help", null);
            LoadContentHolder("HelpSelect", "UI/helpSelect", null);
            LoadContentHolder("Inventory", "UI/inventory", null);
            LoadContentHolder("Ability", "UI/ability", null);
            LoadContentHolder("Stats", "UI/stats", null);
            LoadContentHolder("Settings", "UI/settings", null);
            LoadContentHolder("Quest", "UI/quest", null);
            LoadContentHolder("Back", "UI/back", null);
            LoadContentHolder("Delete", "UI/delete", null);
            LoadContentHolder("InventorySelected", "UI/inventorySelected", null);
            LoadContentHolder("AbilitySelected", "UI/abilitySelected", null);
            LoadContentHolder("StatsSelected", "UI/statsSelected", null);
            LoadContentHolder("SettingsSelected", "UI/settingsSelected", null);
            LoadContentHolder("QuestSelected", "UI/questSelected", null);
            LoadContentHolder("BackSelect", "UI/backSelect", null);
            LoadContentHolder("DeleteSelect", "UI/deleteSelected", null);
            LoadContentHolder("InUse", "UI/inUse", null);
            LoadContentHolder("QuestAvail", "UI/QuestAvail", null);
            LoadContentHolder("Screenshot", "UI/Screenshot", null);
            LoadContentHolder("ScreenshotSelect", "UI/ScreenshotSelect", null);
            LoadContentHolder("Menu", "UI/menu", null);
            LoadContentHolder("MenuSelect", "UI/menuSelect", null);
            LoadContentHolder("names", "UI/names", null);
            LoadContentHolder("namesSelect", "UI/namesSelect", null);
            LoadContentHolder("Check", "UI/check", null);
            LoadContentHolder("CheckSelected", "UI/checkSelected", null);
            LoadContentHolder("ActionButton", "UI/ActionButton", null);
            LoadContentHolder("ActionButtonPress", "UI/ActionButtonPress", null); 

            // Interface
            LoadContentHolder("FullBarHoriz", "PlayerDisplay/BlankFullBarHorizs", null);
            LoadContentHolder("EmptyBarHoriz", "PlayerDisplay/emptybarhorizs", null);
            LoadContentHolder("Portraits", "UI/Portraits", null);
            LoadContentHolder("LoDTitle", "UI/LoDTitle", null);
            LoadContentHolder("Logo", "UI/Logo", null);
            LoadContentHolder("JoystickCircle", "UI/JoystickCircle", null);
            LoadContentHolder("JoystickDot", "UI/JoystickDot", null);

            // NPC
            LoadContentHolder("NPCCommonGray", "NPC/NPCCommonGray", null);
            LoadContentHolder("NPCCommonGray2", "NPC/NPCCommonGray2", null);
            LoadContentHolder("NPCCommonRed", "NPC/NPCCommonRed", null);
            LoadContentHolder("NPCCommonBlue", "NPC/NPCCommonBlue", null);
            LoadContentHolder("NPCCommonYellow", "NPC/NPCCommonYellow", null);
            LoadContentHolder("NPCMageRed", "NPC/NPCMageRed", null);
            LoadContentHolder("NPCMageRed2", "NPC/NPCMageRed2", null);
            LoadContentHolder("NPCMageGreen", "NPC/NPCMageGreen", null);
            LoadContentHolder("NPCMageYellow", "NPC/NPCMageYellow", null);
            LoadContentHolder("NPCMageGray", "NPC/NPCMageGray", null);

            // Items
            LoadContentHolder("ItemIcons", "Items/ItemIcons", null);
            LoadContentHolder("Chests", "Items/Chests", null);
            LoadContentHolder("TownPortal", "Items/TownPortal", null);
            LoadContentHolder("Doors", "Tiles/Doors", null);

            // Particles
            LoadContentHolder("ExplosionParticle", "Particles/explosion", null);
            LoadContentHolder("ExplosionWhiteParticle", "Particles/explWhite", null);
            LoadContentHolder("SmokeParticle", "Particles/smoke", null);
            LoadContentHolder("StarburstParticle", "Particles/starburst", null);
            LoadContentHolder("GlowParticle", "Particles/glow", null);


            // load and translate potions
            LoadContentHolder("RedPotions", "Items/potions", null);
            CopyTexture("BluePotions", "RedPotions");
            TextureModifier.AdjustHSLInBand(GetTexture("BluePotions"), new Color(225, 60, 60), 160, hueAdjust: 240);
            CopyTexture("GreenPotions", "RedPotions");
            TextureModifier.AdjustHSLInBand(GetTexture("GreenPotions"), new Color(225, 60, 60), 160, hueAdjust: 118);
            CopyTexture("OrangePotions", "RedPotions");
            TextureModifier.AdjustHSLInBand(GetTexture("OrangePotions"), new Color(225, 60, 60), 160, hueAdjust: 23);
            CopyTexture("PurplePotions", "RedPotions");
            TextureModifier.AdjustHSLInBand(GetTexture("PurplePotions"), new Color(225, 60, 60), 160, hueAdjust: 275);
            CopyTexture("BlackPotions", "RedPotions");
            TextureModifier.AdjustHSLInBand(GetTexture("BlackPotions"), new Color(225, 60, 60), 160, lightAdjust:-100);
            CopyTexture("YellowPotions", "RedPotions");
            TextureModifier.AdjustHSLInBand(GetTexture("YellowPotions"), new Color(225, 60, 60), 160, hueAdjust: 60);
            CopyTexture("CyanPotions", "RedPotions");
            TextureModifier.AdjustHSLInBand(GetTexture("CyanPotions"), new Color(225, 60, 60), 160, hueAdjust: 180);
            CopyTexture("MagentaPotions", "RedPotions");
            TextureModifier.AdjustHSLInBand(GetTexture("MagentaPotions"), new Color(225, 60, 60), 300, hueAdjust: 180);
        }

        static string[] playerTexTitles = { "DwarfAttack", "DwarfWalking", "DwarfDie", "DwarfAttackSword", "DwarfWalkingSword", "DwarfAttackBow", "DwarfWalkingBow" };
        static string[] playerTexPostfix = { "_attack_unarmed", "_walk_unarmed", "_death_unarmed", "_attack_sword", "_walk_sword", "_attack_bow", "_walk_bow" };

        public static void LoadPlayer(PlayerClass playerClass)
        {
            // Check if we already have this texture loaded
            if (playerClass.texturePrefix == lastLoadedPlayerTextures)
            {
                return;
            }

            // if we currently have player textures loaded then unload them
            if (playerTextureContent != null && playerClass.texturePrefix != lastLoadedPlayerTextures)
            {
                for (int i = 0; i < playerTexTitles.Length; i++)
                {
                    Texture2D tex = GetTexture(playerTexTitles[i]);
                    contextDictionary.Remove(playerTexTitles[i]);
                    tex.Dispose();
                }
                playerTextureContent.Dispose();
            }

            // create a new content manager for the textures
            playerTextureContent = new ContentManager(DungeonGame.Instance.Services);
            playerTextureContent.RootDirectory = DungeonGame.ContentRoot;


            for (int i = 0; i < playerTexTitles.Length; i++)
            {
                LoadContentHolder(playerTexTitles[i], "Player/" + playerClass.texturePrefix + playerTexPostfix[i], playerTextureContent);
            }

            lastLoadedPlayerTextures = playerClass.texturePrefix;
        }


        private static Dictionary<EnemyType, string[]> enemyTextures = new Dictionary<EnemyType, string[]>()
        {
            { EnemyType.Ogre,           new[] { "OgreRun", "OgreAttack", "OgreDie" } },
            { EnemyType.SkeletonKnight, new[] { "SkelKnightWalk", "SkelKnightAttack", "SkelKnightDie" } },
            { EnemyType.SkeletonArcher, new[] { "SkelArcherWalk", "SkelArcherAttack", "SkelArcherDie" } },
            { EnemyType.LavaTroll,      new[] { "LavaTrollWalk", "LavaTrollAttack", "LavaTrollDie" } },
            { EnemyType.Ghost,          new[] { "GhostWalk", "GhostAttack", "GhostDie" } },
            { EnemyType.Goblin,         new[] { "GoblinWalk", "GoblinAttack", "GoblinDie","GoblinWalkLeader", "GoblinAttackLeader", "GoblinDieLeader" } },
            { EnemyType.DwarfBoss,      new[] { "DwarfBossWalk", "DwarfBossAttack"} },
            { EnemyType.DwarfBoss2,      new[] { "DwarfBossWalk2", "DwarfBossAttack2"} },
            { EnemyType.DwarfBoss3,      new[] { "DwarfBossWalk3", "DwarfBossAttack3"} },
            { EnemyType.Drake,     new[] { "DragonWalk", "DragonAttack", "DragonDie" } },
        };

        public static void LoadDungeon(DungeonLevel dungeon)
        {
            // load tileset texture
            if (DungeonGame.isRT && !contextDictionary.ContainsKey(dungeon.TileSet.TextureName))
            {
                LoadContentHolder(dungeon.TileSet.TextureName, "Tiles/" + dungeon.TileSet.TextureName, null);
            }
            if (!contextDictionary.ContainsKey(dungeon.TileSet.SetName))
            {
                if (DungeonGame.isRT)
                {
                    CopyTexture(dungeon.TileSet.SetName, dungeon.TileSet.TextureName);
                }
                else
                {
                    LoadContentHolder(dungeon.TileSet.SetName, "Tiles/" + dungeon.TileSet.TextureName, dungeon.Content);
                }

                dungeon.TileSet.ApplyTextureAdjustment();
            }

            // load enemy textures
            foreach (var enemy in dungeon.Config.Enemies)
            {
                if (enemy.Type == EnemyType.LavaTroll)
                {
                    LavaTroll.ElementalType = enemy.Element;

                    if (DungeonGame.isRT)
                    {
                        foreach (string textureName in enemyTextures[enemy.Type])
                        {
                            if (!contextDictionary.ContainsKey(textureName + LavaTroll.ElementalType.ToString()))
                            {
                                if (!contextDictionary.ContainsKey(textureName))
                                {
                                    LoadContentHolder(textureName, "Enemies/" + textureName, null);
                                }

                                CopyTexture(textureName + LavaTroll.ElementalType.ToString(), textureName);
                                EnemySprite.AdjustTexture(GetTexture(textureName + LavaTroll.ElementalType.ToString()), enemy.Type, dungeon.Level, textureName + LavaTroll.ElementalType.ToString());

                                if (!DungeonGame.lowMemoryMode)
                                {
                                    CopyTexture(textureName + LavaTroll.ElementalType.ToString() + "Champ", textureName);
                                    EnemySprite.AdjustChampionTexture(GetTexture(textureName + LavaTroll.ElementalType.ToString() + "Champ"), enemy.Type, dungeon.Level);
                                }
                            }
                        }

                        continue;
                    }
                }
                else if (enemy.Type == EnemyType.Drake)
                {
                    Drake.ElementalType = enemy.Element;
                }

                foreach (string textureName in enemyTextures[enemy.Type])
                {
                    LoadContentHolder(textureName, "Enemies/" + textureName, dungeon.Content);
                    EnemySprite.AdjustTexture(GetTexture(textureName), enemy.Type, dungeon.Level, textureName);

                    if (!DungeonGame.lowMemoryMode && !DungeonGame.isRT)
                    {
                        CopyTexture(textureName + "Champ", textureName);
                        EnemySprite.AdjustChampionTexture(GetTexture(textureName + "Champ"), enemy.Type, dungeon.Level);
                    }
                }
            }
        }

        public static void UnloadDungeon(DungeonLevel dungeon)
        {
            Texture2D tex;

            if (!DungeonGame.isRT)
            {
                // in RT we keep these loaded
                tex = GetTexture(dungeon.TileSet.SetName);
                contextDictionary.Remove(dungeon.TileSet.SetName);
                tex.Dispose();
            }

            foreach (var enemyType in dungeon.EnemyTypes)
            {
                if (DungeonGame.isRT && enemyType == EnemyType.LavaTroll)
                {
                    // in RT we keep these loaded
                    continue;
                }

                foreach (string textureName in enemyTextures[enemyType])
                {
                    tex = GetTexture(textureName);
                    contextDictionary.Remove(textureName);
                    tex.Dispose();

                    // explicitly dispose champion textures since they are not in the XNA content pipeline
                    if (!DungeonGame.lowMemoryMode && !DungeonGame.isRT)
                    {
                        tex = GetTexture(textureName + "Champ");
                        contextDictionary.Remove(textureName + "Champ");
                        tex.Dispose();
                    }
                }
            }
        }

        public static void LoadContentHolder(string name, string textureLocation, ContentManager content)
        {
            ContentHolder holder = new ContentHolder();
            holder.ContentName = name;
            if (content == null)
            {
                holder.Texture = DungeonGame.Instance.Content.Load<Texture2D>(System.IO.Path.Combine(@"Textures", textureLocation));
            }
            else
            {
                holder.Texture = content.Load<Texture2D>(System.IO.Path.Combine(@"Textures", textureLocation));
            }
            contextDictionary.Add(name, holder);
        }



        public static void CopyTexture(string name, string copyFromName)
        {
            if (GetTexture(copyFromName).Format == SurfaceFormat.Color)
            {
                CopyTextureColor(name, copyFromName);
            }
            else if (GetTexture(copyFromName).Format == SurfaceFormat.Dxt5)
            {
                CopyTextureDXT5(name, copyFromName);
            }
            else
            {
                throw new Exception("invalid texture type for copy");
            }
        }


        public static void CopyTextureDXT5(string name, string copyFromName)
        {
            ContentHolder holder = new ContentHolder();
            holder.ContentName = name;
            Texture2D fromTexture = GetTexture(copyFromName);
            holder.Texture = new Texture2D(DungeonGame.graphics.GraphicsDevice, fromTexture.Width, fromTexture.Height, false, SurfaceFormat.Dxt5);
            byte[] data = new byte[fromTexture.Width * fromTexture.Height];
#if !SILVERLIGHT
            fromTexture.GetData(data);
            holder.Texture.SetData(data);
#endif
            contextDictionary.Add(name, holder);
        }


        public static void CopyTextureColor(string name, string copyFromName)
        {
            ContentHolder holder = new ContentHolder();
            holder.ContentName = name;
            Texture2D fromTexture = GetTexture(copyFromName);
            holder.Texture = new Texture2D(DungeonGame.graphics.GraphicsDevice, fromTexture.Width, fromTexture.Height, false, SurfaceFormat.Color);
            Color[] data = new Color[fromTexture.Width * fromTexture.Height];
            fromTexture.GetData(data);
            holder.Texture.SetData(data);
            contextDictionary.Add(name, holder);
        }


        public static Texture2D GetTexture(string name)
        {
            ContentHolder contentHolder = GetContentHolder(name);
            return contentHolder.Texture;
        }

        private static Dictionary<Color, Texture2D> solidColorTextures = new Dictionary<Color, Texture2D>();

        public static Texture2D GetSolidColorTexture(Color color)
        {
            if (solidColorTextures.ContainsKey(color))
            {
                return solidColorTextures[color];
            }
            else
            {
                Texture2D rectangleTexture = new Texture2D(
                    DungeonGame.Instance.GraphicsDevice,
                    1,
                    1,
                    true,
                    SurfaceFormat.Color);
                Color[] colorArray = { color };
                rectangleTexture.SetData(colorArray);
                solidColorTextures[color] = rectangleTexture;
                return rectangleTexture;
            }
        }

        private static ContentHolder GetContentHolder(string name)
        {
            return contextDictionary[name];
        }
    }
}