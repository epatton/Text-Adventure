using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextAdventure.Core.Helpers;
using TextAdventure.Core.Models;
using Action = TextAdventure.Core.Models.Action;

namespace TextAdventure.UI
{
    class Program
    {
        static void Main(string[] args)
        {
            var game = new GameManager();
            game.Player.Gold = 100;
            game.StartGame();

            Console.ReadKey();
        }

        public static void TestFileSaving()
        {
            var screen1 = new Screen
            {
                Id = 1,
                ScreenIntroMessage = "Wow, so this is screen 1, eh? A little barren, don't you think?",
                Options = new List<TextOption>
                {
                    new TextOption
                    {
                        Id = 1,
                        Text = "Let's spruce this place up a bit!",
                        ShowCondition = new Condition
                        {
                            Type = ConditionType.HasGold,
                            Parameter = 10
                        },
                        Actions = new List<Action>
                        {
                            new Action
                            {
                                Type = ActionType.ChangeScreen,
                                Parameter = 2
                            }
                        }
                    },
                    new TextOption
                    {
                        Id = 2,
                        Text = "Meh, I'm bored.",
                        Actions = new List<Action>
                        {
                            new Action
                            {
                                Type = ActionType.EndGame
                            }
                        }
                    }
                }
            };
            var screen2 = new Screen
            {
                Id = 2,
                ScreenIntroMessage = "You bought so many nice things for the room! Too bad you can't see them...",
                Options = new List<TextOption>
                {
                    new TextOption
                    {
                        Id = 1,
                        Text = "Welp... Bye then.",
                        Actions = new List<Action>
                        {
                            new Action
                            {
                                Type = ActionType.DisplayMessage,
                                Parameter = "It sure was fun having you decorate my room! Goodbye!"
                            },
                            new Action
                            {
                                Type = ActionType.EndGame
                            }
                        }
                    }
                }
            };

            var screens = new List<Screen>();
            screens.Add(screen1);
            screens.Add(screen2);

            var saveHandler = new SaveHandler();
            saveHandler.Save(screens, typeof(List<Screen>), saveHandler.ScreensFile);
        }
    }
}
