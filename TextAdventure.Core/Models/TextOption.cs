using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextAdventure.Core.Models
{
    [Serializable]
    public class TextOption
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public List<Action> Actions { get; set; } = new List<Action>();
        public Condition ShowCondition { get; set; }
    }
}
