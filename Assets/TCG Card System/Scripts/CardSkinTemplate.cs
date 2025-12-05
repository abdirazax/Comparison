using System.Collections.Generic;
using TCG_Card_System.Scripts.Enums;
using UnityEngine;
using UnityEngine.Serialization;

namespace TCG_Card_System.Scripts
{
    [CreateAssetMenu(fileName = "New Card Skin", menuName = "Card Skin")]
    public class CardSkinTemplate : ScriptableObject
    {
        [SerializeField]
        public string id;
        [SerializeField]
        public Texture2D[] frontSprite = new Texture2D[3];
        
        [SerializeField]
        public Texture2D[] frontMaskSprite = new Texture2D[3];
        
        [SerializeField]
        public Texture2D[] backSprite = new Texture2D[3];
        
        
    }
}