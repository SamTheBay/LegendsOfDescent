using Microsoft.Xna.Framework;

namespace LegendsOfDescent
{
    public interface IMap
    {
        Point Dimension { get; }

        Point StartLocation { get; set; }

        void GenerationComplete();

        void SetTile(int x, int y, TileType firstTile, TileType secondTile = TileType.Empty, TileType thirdTile = TileType.Empty);

        Point StairsUpLocation { get; set; }

        Point StairsDownLocation { get; set; }

        bool GoingDown { get; }

        int Level { get; }

        void CreateAndAddStairs();

        string MapType { get; }

        void AddEnvItem(IEnvItem envItem, int minDistance = 0, bool generatePosition = false);

        int TileDimension { get; }
    }
}
