using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextAdventure.Core.Helpers;

namespace TextAdventure.Core.Models
{
    public class GameManager
    {
        public static GameManager Instance { get; set; }
        private Screen _currentScreen;

        public Player Player { get; } = new Player();
        public List<Screen> Screens { get; set; } = new List<Screen>();
        public List<Companion> Companions { get; set; } = new List<Companion>();
        public List<Item> Items { get; set; } = new List<Item>();
        public List<Shop> Shops { get; set; } = new List<Shop>();

        public GameManager()
        {
            Instance = this;
        }

        public void StartGame()
        {
            var saveHandler = new SaveHandler();
            Screens = (List<Screen>)saveHandler
                .Load(typeof(List<Screen>), saveHandler.ScreensFile);

            Companions = (List<Companion>)saveHandler
                .Load(typeof(List<Companion>), saveHandler.CompanionsFile);

            Items = (List<Item>)saveHandler
                .Load(typeof(List<Item>), saveHandler.ItemsFile);

            Shops = (List<Shop>)saveHandler
                .Load(typeof(List<Shop>), saveHandler.ShopsFile);

            LoadScreen(Screens.First().Id);
        }

        public void LoadScreen(int screenId)
        {
            Console.Clear();
            _currentScreen = Screens
                .SingleOrDefault(x => x.Id == screenId);

            if (_currentScreen != null)
            {
                _currentScreen.DisplayOptions();
            }
        }

        public void ReturnToScreen()
        {
            LoadScreen(_currentScreen.Id);
        }

        public void EndGame()
        {
            _currentScreen.EndGame();
        }
    }
}
