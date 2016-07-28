using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;


namespace LegendsOfDescent
{
    public enum TileType
    {
        Empty,
        Open,
        Open2,
        Open3,

        WallVertical,
        WallVertical2,
        WallVertical3,
        WallHorizontal,
        WallHorizontal2,
        WallHorizontal3,
        WallCornerBottomRight,
        WallCornerTopLeft,
        WallCornerTopRight,
        WallCornerBottomLeft,
        StairsUp,
        StairsDown,
        Column,

        InnerWallVertical,
        InnerWallVertical2,
        InnerWallHorizontal,
        InnerWallHorizontal2,
        InnerWallVerticalBroken,
        InnerWallVerticalBroken2,
        InnerWallHorizontalBroken,
        InnerWallHorizontalBroken2,
        InnerWallCornerBottomRight,
        InnerWallCornerTopLeft,
        InnerWallCornerTopRight,
        InnerWallCornerBottomLeft,
        InnerWallColumn,

        InnerWallRichColumn,
        InnerWallRichVertical,
        InnerWallRichVertical2,
        InnerWallRichVertical3,
        InnerWallRichVertical4,
        InnerWallRichHorizontal,
        InnerWallRichHorizontal2,
        InnerWallRichHorizontal3,
        InnerWallRichHorizontal4,
        InnerWallRichCornerBottomRight,
        InnerWallRichCornerTopLeft,
        InnerWallRichCornerTopRight,
        InnerWallRichCornerBottomLeft,
        
        Road,
        Dirt,
        Vent,
        InnerInsideRich,
        InnerInsideRich2,
        InnerInsideRich3,
        InnerInsideRich4,
        InnerInsideRich5,
        InnerInsideRich6,
        InnerInside,
        InnerInside2,
        InnerInside3,
        InnerInside4,
        Torch,
        Anvil,
        Pillar,

        ShroomForest,
        ShroomForest2,
        ShroomForest3,
        ShroomForest4,

        WoodenDoorVirtical,
        WoodenDoorHorizontal,
        MetalDoorVirtical,
        MetalDoorHorizontal
    }

    public class TileSet
    {
        static List<TileSet> TileSets = new List<TileSet>();
        public static List<string> Names = new List<string>();
        public static List<string> RandomNames = new List<string>();
        Dictionary<TileType, int> tileMapping = new Dictionary<TileType, int>();
        String textureName;
        String setName;
        int framesPerRow;
        int tileDimension;
        int spriteDimension;

        // color adjustments
        int hue = 0;
        int sat = 0;
        int light = 0;

        public TileSet(String textureName)
        {
            this.textureName = textureName;
        }

        public int GetTileOffset(TileType tileType)
        {
            return tileMapping[tileType];
        }


        public int FramesPerRow
        {
            get { return framesPerRow; }
        }

        public String TextureName
        {
            get { return textureName; }
        }

        public String SetName
        {
            get { return setName; }
        }
        public int TileDimension
        {
            get { return tileDimension; }
        }

        public int SpriteDimension
        {
            get { return spriteDimension; }
        }


        public void ApplyTextureAdjustment()
        {
            Texture2D texture = InternalContentManager.GetTexture(setName);
            TextureModifier.AdjustHSL(texture, hue, sat, light);
        }

        private static void Add(TileSet tileSet)
        {
            TileSets.Add(tileSet);
            Names.Add(tileSet.setName);
            if (tileSet.setName != "Town")
                RandomNames.Add(tileSet.SetName);
        }

        private static TileSet LoadTileSet(string texture)
        {
            TileSet tileSet = new TileSet(texture);
            tileSet.spriteDimension = 64;
            tileSet.tileDimension = 64;
            tileSet.framesPerRow = 4;
            tileSet.tileMapping.Add(TileType.WallVertical, 0);
            tileSet.tileMapping.Add(TileType.WallVertical2, 0);
            tileSet.tileMapping.Add(TileType.WallVertical3, 0);
            tileSet.tileMapping.Add(TileType.WallCornerBottomLeft, 0);
            tileSet.tileMapping.Add(TileType.WallHorizontal, 1);
            tileSet.tileMapping.Add(TileType.WallHorizontal2, 1);
            tileSet.tileMapping.Add(TileType.WallHorizontal3, 1);
            tileSet.tileMapping.Add(TileType.WallCornerTopRight, 1);
            tileSet.tileMapping.Add(TileType.WallCornerBottomRight, 2);
            tileSet.tileMapping.Add(TileType.WallCornerTopLeft, 3);
            tileSet.tileMapping.Add(TileType.Open, 4);
            tileSet.tileMapping.Add(TileType.Open2, 5);
            tileSet.tileMapping.Add(TileType.Open3, 6);
            tileSet.tileMapping.Add(TileType.StairsUp, 8);
            tileSet.tileMapping.Add(TileType.StairsDown, 9);
            tileSet.tileMapping.Add(TileType.Column, 10);

            if (texture == "TownTiles")
            {
                tileSet.tileMapping.Add(TileType.InnerWallVertical, 12);
                tileSet.tileMapping.Add(TileType.InnerWallVertical2, 25);
                tileSet.tileMapping.Add(TileType.InnerWallCornerBottomLeft, 12);
                tileSet.tileMapping.Add(TileType.InnerWallHorizontal, 13);
                tileSet.tileMapping.Add(TileType.InnerWallHorizontal2, 24);
                tileSet.tileMapping.Add(TileType.InnerWallCornerTopRight, 13);
                tileSet.tileMapping.Add(TileType.InnerWallCornerBottomRight, 14);
                tileSet.tileMapping.Add(TileType.InnerWallCornerTopLeft, 15);
                tileSet.tileMapping.Add(TileType.InnerWallVerticalBroken, 20);
                tileSet.tileMapping.Add(TileType.InnerWallVerticalBroken2, 21);
                tileSet.tileMapping.Add(TileType.InnerWallHorizontalBroken, 22);
                tileSet.tileMapping.Add(TileType.InnerWallHorizontalBroken2, 23);
                tileSet.tileMapping.Add(TileType.InnerWallColumn, 30);

                tileSet.tileMapping.Add(TileType.InnerWallRichVertical, 39);
                tileSet.tileMapping.Add(TileType.InnerWallRichVertical2, 45);
                tileSet.tileMapping.Add(TileType.InnerWallRichVertical3, 46);
                tileSet.tileMapping.Add(TileType.InnerWallRichVertical4, 47);
                tileSet.tileMapping.Add(TileType.InnerWallRichHorizontal, 38);
                tileSet.tileMapping.Add(TileType.InnerWallRichHorizontal2, 42);
                tileSet.tileMapping.Add(TileType.InnerWallRichHorizontal3, 43);
                tileSet.tileMapping.Add(TileType.InnerWallRichHorizontal4, 44);
                tileSet.tileMapping.Add(TileType.InnerWallRichCornerBottomRight, 40);
                tileSet.tileMapping.Add(TileType.InnerWallRichCornerTopLeft, 41);
                tileSet.tileMapping.Add(TileType.InnerWallRichCornerTopRight, 38);
                tileSet.tileMapping.Add(TileType.InnerWallRichCornerBottomLeft, 39);
                tileSet.tileMapping.Add(TileType.InnerWallRichColumn, 55);
        

                tileSet.tileMapping.Add(TileType.ShroomForest, 32);
                tileSet.tileMapping.Add(TileType.ShroomForest2, 33);
                tileSet.tileMapping.Add(TileType.ShroomForest3, 34);
                tileSet.tileMapping.Add(TileType.ShroomForest4, 35);

                tileSet.tileMapping.Add(TileType.Road, 11);
                tileSet.tileMapping.Add(TileType.Dirt, 36);
                tileSet.tileMapping.Add(TileType.Vent, 27);
                tileSet.tileMapping.Add(TileType.InnerInsideRich, 52);
                tileSet.tileMapping.Add(TileType.InnerInsideRich2, 49);
                tileSet.tileMapping.Add(TileType.InnerInsideRich3, 50);
                tileSet.tileMapping.Add(TileType.InnerInsideRich4, 51);
                tileSet.tileMapping.Add(TileType.InnerInsideRich5, 48);
                tileSet.tileMapping.Add(TileType.InnerInsideRich6, 53);
                tileSet.tileMapping.Add(TileType.InnerInside, 16);
                tileSet.tileMapping.Add(TileType.InnerInside2, 17);
                tileSet.tileMapping.Add(TileType.InnerInside3, 18);
                tileSet.tileMapping.Add(TileType.InnerInside4, 19);
                tileSet.tileMapping.Add(TileType.Torch, 37);
                tileSet.tileMapping.Add(TileType.Anvil, 26);
                tileSet.tileMapping.Add(TileType.Pillar, 54);

            }

            // just fillers, doors are special cased
            tileSet.tileMapping.Add(TileType.WoodenDoorVirtical, 0);
            tileSet.tileMapping.Add(TileType.WoodenDoorHorizontal, 0);
            tileSet.tileMapping.Add(TileType.MetalDoorVirtical, 0);
            tileSet.tileMapping.Add(TileType.MetalDoorHorizontal, 0);

            return tileSet;
        }

        public static void InitializeTileSets()
        {
            TileSet tileSet;

            // sewer
            tileSet = LoadTileSet("BrickTiles");
            tileSet.setName = "Sewer";
            Add(tileSet);
            

            // red sewer
            tileSet = LoadTileSet("BrickTiles");
            tileSet.setName = "RedSewer";
            tileSet.hue = -60;
            Add(tileSet);

            // cave
            tileSet = LoadTileSet("CaveTiles");
            tileSet.setName = "Cave";
            Add(tileSet);

            // dark cave
            tileSet = LoadTileSet("CaveTiles");
            tileSet.setName = "DarkCave";
            tileSet.light = -65;
            Add(tileSet);

            // brown cave
            tileSet = LoadTileSet("CaveTiles");
            tileSet.setName = "BrownCave";
            tileSet.hue = -12;
            tileSet.sat = 90;
            tileSet.light = -30;
            Add(tileSet);

            // crystal cave
            tileSet = LoadTileSet("CrystalTiles");
            tileSet.setName = "Crystal";
            Add(tileSet);

            // Purple Crystal
            tileSet = LoadTileSet("CrystalTiles");
            tileSet.setName = "PurpleCrystal";
            tileSet.hue = 108;
            tileSet.sat = -20;
            Add(tileSet);

            // Dark Blue Crystal
            tileSet = LoadTileSet("CrystalTiles");
            tileSet.setName = "DarkBlueCrystal";
            tileSet.hue = 53;
            tileSet.light = -30;
            Add(tileSet);

            // Lava cave
            tileSet = LoadTileSet("LavaTiles");
            tileSet.setName = "LavaCave";
            Add(tileSet);

            // Dark lava cave
            tileSet = LoadTileSet("LavaTiles");
            tileSet.setName = "DarkLavaCave";
            tileSet.light = -40;
            Add(tileSet);

            tileSet = LoadTileSet("TownTiles");
            tileSet.setName = "Town";
            Add(tileSet);
        }


        public static TileSet GetTileSet(string setName)
        {
            for (int i = 0; i < TileSets.Count; i++)
            {
                if (TileSets[i].setName == setName)
                {
                    return TileSets[i];
                }
            }

            return null;
        }


        public static TileSet GetTileSet()
        {
            return TileSets.Random();
        }

    }
}
