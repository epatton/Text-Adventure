using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextAdventure.Core.Models
{
    [Serializable]
    public class Screen
    {
        public int Id { get; set; }
        public string InternalName { get; set; }
        public string ScreenIntroMessage { get; set; }
        public List<TextOption> Options { get; set; } = new List<TextOption>();

        private bool _endGame = false;

        public void DisplayOptions()
        {
            Console.WriteLine(ScreenIntroMessage);
            Console.WriteLine("-------------------------------");
            foreach (var option in Options)
            {
                if(option.ShowCondition == null || option.ShowCondition.ConditionMet())
                    Console.WriteLine(option.Id + " - " + option.Text);
            }

            Console.WriteLine("-------------------------------");
            int selectedOption = -1;
            while(selectedOption < 0)
            {
                if (_endGame)
                    break;

                Console.WriteLine("Enter Choice: ");
                var input = Console.ReadLine();
                if(int.TryParse(input, out selectedOption))
                {
                    if(!SelectOption(selectedOption))
                    {
                        selectedOption = -1;
                        Console.WriteLine("Please enter a number for an option listed above.");
                    }
                }
                else
                {
                    Console.WriteLine("Please enter a number for your choice.");
                }
            }
        }

        public bool SelectOption(int id)
        {
            var result = false;
            var actions = Options
                .SingleOrDefault(x => x.Id == id)?
                .Actions;

            if(actions != null)
            {
                foreach(var action in actions)
                {
                    action.Execute();
                    result = true;
                }
            }

            return result;
        }

        public void EndGame()
        {
            _endGame = true;
        }
    }
}
