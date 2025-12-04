using TCG_Card_System.Scripts.Enums;
using UnityEngine;

namespace TCG_Card_System.Scripts
{
    [CreateAssetMenu(fileName = "New card", menuName = "Card")]
    public class CardTemplate : ScriptableObject
    {
        [SerializeField]
        public string id;
        [SerializeField]
        public Texture2D characterImage;
        [SerializeField]
        public Texture2D backgroundImage;
        [SerializeField]
        public new string name;
        [SerializeField]
        public string description;
        
        [SerializeField]
        public CardAttack attackType;
        
        [SerializeField]
        public int mana;
        [SerializeField]
        public int attack;
        [SerializeField]
        public int health;
        
        public bool IsMelee => attackType.type == ECardAttack.Melee;
        public bool IsRanged => attackType.type == ECardAttack.Ranged;
    }
}