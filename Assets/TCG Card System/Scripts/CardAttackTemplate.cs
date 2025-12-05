using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TCG_Card_System.Scripts
{
    [CreateAssetMenu(fileName = "CardAttackTemplate", menuName = "Card Attack Template", order = 0)]
    public class CardAttackTemplate : ScriptableObject
    {
        
        public List<Sprite> onCardSprite;
        public Material onCardMaterial;
        [ColorUsage(true, true)]
        public List<Color> onCardColor;
        public List<Sprite> onTargetSprite;
        public Material onTargetMaterial;
        [ColorUsage(true, true)]
        public List<Color> onTargetColor;
        public List<Sprite> bulletSprite;
        public Material bulletMaterial;
        [ColorUsage(true, true)]
        public List<Color> bulletColor;
        public List<Sprite> bulletTrailSprite;
        public Material bulletTrailMaterial;
        [ColorUsage(true, true)]
        public List<Color> bulletTrailColor;
        public List<Sprite> muzzleFlashSprite;
        public Material muzzleFlashMaterial;
        [ColorUsage(true, true)]
        public List<Color> muzzleFlashColor;

        [SerializeField, Tooltip("X: Start Duration, Y: Duration, Z: End Duration, W: Delay")]
        public List<Vector4> onCardEffectDurationFactor = 
            new List<Vector4>() {new Vector4(0.06f, 3f, 0.06f, 0f)};
        [SerializeField, Tooltip("X: Start Duration, Y: Duration, Z: End Duration, W: Delay")]
        public List<Vector4> onEnemyEffectDurationFactor = 
            new List<Vector4>() {new Vector4(0.06f, 3f, 0.06f, 0f)};
        [SerializeField, Tooltip("X: Start Duration, Y: Duration, Z: End Duration, W: Delay")]
        public List<Vector4> bulletTrailEffectDurationFactor = 
            new List<Vector4>() {new Vector4(0.06f, 3f, 0.06f, 0f)};
    }
}