using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextAdventure.Core.Models
{
    [Serializable]
    public class Companion
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
