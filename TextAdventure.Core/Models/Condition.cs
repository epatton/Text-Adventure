using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextAdventure.Core.Models
{
    [Serializable]
    public enum ConditionType
    {
        HasItem = 1,
        HasCompanion = 2,
        HasGold = 3
    }

    [Serializable]
    public class Condition
    {
        public ConditionType Type { get; set; }
        public dynamic Parameter { get; set; }

        public bool ConditionMet()
        {
            switch(Type)
            {
                case ConditionType.HasItem:
                    var itemId = (int)Parameter;
                    return GameManager.Instance.Player.Backpack
                        .Any(x => x.Id == itemId);
                case ConditionType.HasGold:
                    var goldNeeded = (int)Parameter;
                    return GameManager.Instance.Player.Gold >= goldNeeded;
                case ConditionType.HasCompanion:
                    var companionId = (int)Parameter;
                    return GameManager.Instance.Player.Companions
                        .Any(x => x.Id == companionId);
                default:
                    return false;
            }
        }
    }
}
