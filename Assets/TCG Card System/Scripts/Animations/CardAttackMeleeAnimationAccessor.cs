using UnityEngine;

namespace TCG_Card_System.Scripts.Animations
{
    public class CardAttackMeleeAnimationAccessor : MonoBehaviour
    {
        [SerializeField] 
        public GameObject hitGameObject;
        
        [SerializeField] 
        public ParticleSystem hitParticleSystem;
    }
}