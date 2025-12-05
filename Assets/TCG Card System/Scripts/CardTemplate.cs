using System.Collections.Generic;
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
        public Sprite characterImage;
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
        public List<int> attack;
        public CardAttackTemplate attackTemplate;
        [SerializeField]
        public int health;
        
        [SerializeField]
        public int slotSize = 1; 
        [SerializeField, Tooltip("The delay between auto attacks in seconds.")]
        public float autoAttackInterval = 1f;
        [SerializeField]
        public int attackPerInterval = 1;
        
        public bool IsMelee => attackType.type == ECardAttack.Melee;
        public bool IsRanged => attackType.type == ECardAttack.Ranged;

        [SerializeField] public CardSkinTemplate cardSkin;
    }
}