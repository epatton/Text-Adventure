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
        None = 0,
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
                    var itemId = Convert.ToInt32(Parameter);
                    return GameManager.Instance.Player.Backpack
                        .Any(x => x.Id == itemId);
                case ConditionType.HasGold:
                    var goldNeeded = Convert.ToInt32(Parameter);
                    return GameManager.Instance.Player.Gold >= goldNeeded;
                case ConditionType.HasCompanion:
                    var companionId = Convert.ToInt32(Parameter);
                    return GameManager.Instance.Player.Companions
                        .Any(x => x.Id == companionId);
                case ConditionType.None:
                    return true;
                default:
                    return false;
            }
        }
    }
}
