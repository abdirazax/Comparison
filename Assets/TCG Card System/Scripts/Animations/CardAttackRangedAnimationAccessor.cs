using UnityEngine;

namespace TCG_Card_System.Scripts.Animations
{
    public class CardAttackRangedAnimationAccessor : MonoBehaviour
    {
        [SerializeField] 
        public GameObject hitGameObject;
        
        [SerializeField] 
        public ParticleSystem hitParticleSystem;
        
        [SerializeField] 
        public ParticleSystem projectileParticleSystem;
        
        // [SerializeField] 
        // private GameObject flashGameObject;
    }
}