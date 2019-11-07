using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextAdventure.Core.Models
{
    [Serializable]
    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Cost { get; set; }
        public string CombinedName
        {
            get
            {
                return Id.ToString() + " - " + Name + " " + Cost.ToString() + "G";
            }
        }

        public string CombinedNameWithoutCost
        {
            get
            {
                return Id.ToString() + " - " + Name + " ";
            }
        }
    }
}
