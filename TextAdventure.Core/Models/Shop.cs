using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextAdventure.Core.Models
{
    [Serializable]
    public class Shop
    {
        public int Id { get; set; }
        public List<Item> Items { get; set; } = new List<Item>();
        public string ShopName { get; set; }
        public string WelcomeMessage { get; set; }

        public void ShowStore()
        {
            int selectedItem = -1;
            while (selectedItem < 0)
            {
                Console.Clear();

                if(!Items.Any())
                {
                    Console.WriteLine("Sorry, but I'm out of items! Thanks for your business!");
                    Console.ReadKey();
                    break;
                }

                Console.WriteLine(WelcomeMessage);
                var maxLength = Items
                    .Aggregate((max, cur) =>
                        max.CombinedName.Length > cur.CombinedName.Length ? max : cur)
                    .CombinedName.Length;

                var topAndBottomBorder = new String('*', maxLength + 4);
                Console.WriteLine(topAndBottomBorder);
                foreach (var item in Items)
                {
                    var spacing = "";
                    var numSpaces = maxLength - item.CombinedName.Length;
                    for (int i = 0; i < numSpaces; ++i)
                    {
                        spacing += " ";
                    }

                    Console.WriteLine("* " + item.CombinedNameWithoutCost + spacing + item.Cost + "G *");
                }
                Console.WriteLine(topAndBottomBorder);
                Console.WriteLine("Enter Choice (type 'q' to exit store): ");
                Console.WriteLine("You have " + GameManager.Instance.Player.Gold + "G");
                var input = Console.ReadLine();
                if(input.ToLower() == "q")
                {
                    break;
                }

                if (int.TryParse(input, out selectedItem))
                {
                    if(Items.Any(x => x.Id == selectedItem))
                    {
                        var item = Items.First(x => x.Id == selectedItem);
                        if(GameManager.Instance.Player.Gold >= item.Cost)
                        {
                            GameManager.Instance.Player.Gold -= item.Cost;
                            GameManager.Instance.Player.Backpack.Add(item);
                            Items.Remove(item);
                            Console.WriteLine("You bought " + item.Name + " for " + item.Cost + "G");
                            Console.ReadKey();
                            selectedItem = -1;
                        }
                        else
                        {
                            selectedItem = -1;
                            Console.WriteLine("You're too poor to afford this!");
                        }
                    }
                    else
                    {
                        selectedItem = -1;
                        Console.WriteLine("Please enter a number for an item listed above.");
                    }
                }
                else
                {
                    Console.WriteLine("Please enter a number for your choice.");
                }
            }

            GameManager.Instance.ReturnToScreen();
        }
    }
}
