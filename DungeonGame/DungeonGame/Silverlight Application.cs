using System;
using ExEnSilver;

namespace DungeonGame
{
	public class App : ExEnSilverApplication
	{
		protected override void SetupMainPage(MainPage mainPage)
		{
			var game = new DungeonGame();
			mainPage.Children.Add(game);
			game.Play();
		}
	}
}
