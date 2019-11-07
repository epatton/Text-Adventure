using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextAdventure.Core.Models
{
    [Serializable]
    public enum ActionType
    {
        ChangeScreen = 1,
        ModifyGold = 2,
        AddCompanion = 3,
        RemoveCompanion = 4,
        DisplayMessage = 5,
        AddItem = 6,
        RemoveItem = 7,
        EndGame = 8,
        ShowShop = 9
    }

    [Serializable]
    public class Action
    {
        public int Id { get; set; }
        public ActionType Type { get; set; }
        public dynamic Parameter { get; set; }

        public void Execute()
        {
            switch(Type)
            {
                case ActionType.AddCompanion:
                    AddCompanion();
                    break;
                case ActionType.AddItem:
                    AddItem();
                    break;
                case ActionType.ChangeScreen:
                    ChangeScreen();
                    break;
                case ActionType.DisplayMessage:
                    DisplayMessage();
                    break;
                case ActionType.ModifyGold:
                    ModifyGold();
                    break;
                case ActionType.RemoveCompanion:
                    RemoveCompanion();
                    break;
                case ActionType.RemoveItem:
                    RemoveItem();
                    break;
                case ActionType.ShowShop:
                    ShowShop();
                    break;
                case ActionType.EndGame:
                    GameManager.Instance.EndGame();
                    break;
            }
        }

        private void ModifyGold()
        {
            var gold = Convert.ToInt32(Parameter);
            GameManager.Instance.Player.Gold += gold;
        }

        private void DisplayMessage()
        {
            var msg = Convert.ToString(Parameter);
            Console.WriteLine(msg);
            Console.ReadKey();
        }

        private void ChangeScreen()
        {
            var screenId = Convert.ToInt32(Parameter);
            GameManager.Instance.LoadScreen(screenId);
        }

        private void AddItem()
        {
            var itemId = Convert.ToInt32(Parameter);
            var item = GameManager.Instance.Items.Single(x => x.Id == itemId);
            GameManager.Instance.Player.Backpack.Add(item);
        }

        private void RemoveItem()
        {
            var itemId = Convert.ToInt32(Parameter);
            var item = GameManager.Instance.Player.Backpack
                .First(x => x.Id == itemId);
            GameManager.Instance.Player.Backpack.Remove(item);
        }

        private void AddCompanion()
        {
            var compId = Convert.ToInt32(Parameter);
            var comp = GameManager.Instance.Companions
                .Single(x => x.Id == compId);
            if (!GameManager.Instance.Player.Companions.Any(x => x.Id == comp.Id))
                GameManager.Instance.Player.Companions.Add(comp);
        }

        private void RemoveCompanion()
        {
            var compId = Convert.ToInt32(Parameter);
            var comp = GameManager.Instance.Player.Companions
                .SingleOrDefault(x => x.Id == compId);
            if (comp != null)
                GameManager.Instance.Player.Companions.Remove(comp);
        }

        private void ShowShop()
        {
            var shopId = Convert.ToInt32(Parameter);
            var shop = GameManager.Instance.Shops
                .SingleOrDefault(x => x.Id == shopId);
            if(shop != null)
            {
                shop.ShowStore();
            }
        }
    }
}
