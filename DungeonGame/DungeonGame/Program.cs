using System;

namespace LegendsOfDescent
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (DungeonGame game = new DungeonGame())
            {
                game.Run();
            }
        }
    }
}

