using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextAdventure.Core.Models
{
    public class Player
    {
        public int Gold { get; set; }
        public List<Item> Backpack { get; set; } = new List<Item>();
        public List<Companion> Companions { get; set; } = new List<Companion>();
    }
}
