using Cysharp.Threading.Tasks;
using TCG_Card_System.Scripts.Animations;
using UnityEngine;

namespace TCG_Card_System.Scripts.Effects
{
    public class DissolveEffect : MonoBehaviourExtended<GameObject>
    {
        [SerializeField]
        private string amountPropertyReference = "_DissolveAmount";
        
        [SerializeField]
        public float dissolveTime = 0.75f;
        
        private int _dissolveAmountProperty;
        private Material _material;
        private MeshCollider _collider;

        private bool _visible;

        private void Awake()
        {
            _dissolveAmountProperty = Shader.PropertyToID(amountPropertyReference);
            _material = GetComponent<SpriteRenderer>().material;
            _collider = GetComponent<MeshCollider>();

            _visible = _material.GetFloat(_dissolveAmountProperty) > 0f;
        }

        public void SetVisibility(bool visible)
        {
            _material.SetFloat(_dissolveAmountProperty, visible ? 0f : 1f);
            _collider.enabled = visible;
            _visible = visible;
            
            gameObject.SetActive(visible);
            SetChildsVisibility(visible);
        }

        public async UniTask Appear()
        {
            if (_visible)
                return;
            
            _visible = true;
            
            gameObject.SetActive(true);
            _collider.enabled = true;
            
            await 
            (
                AnimationStart(gameObject, () => AnimationDefinition(true)),
                UniTask.Create
                (
                    () =>
                    {
                        SetChildsVisibility(true);
                        
                        return UniTask.CompletedTask;
                    }
                )
            );
        }
        
        public async UniTask Disappear()
        {
            if (!_visible)
                return;
            
            _visible = false;
            
            await 
            (
                AnimationStart(gameObject, () => AnimationDefinition()),
                UniTask.Create
                (
                    () =>
                    {
                        SetChildsVisibility(false);
                        
                        return UniTask.CompletedTask;
                    }
                )
            );
            
            gameObject.SetActive(false);
            _collider.enabled = false;
        }

        private void SetChildsVisibility(bool visible)
        {
            for (var i = 0; i < gameObject.transform.childCount; i++)
            {
                var childGameObject = gameObject.transform.GetChild(i).gameObject;
                childGameObject.SetActive(visible);
            }
        }

        private async UniTask AnimationDefinition(bool revert = false)
        {
            var amountFrom = _material.GetFloat(_dissolveAmountProperty);
            var amountTo = revert ? 0f : 1f;
            var elapsedTime = 0f;
            
            while (elapsedTime < dissolveTime)
            {
                elapsedTime += Time.deltaTime;

                var lerpedDissolve = Mathf.Lerp
                (
                    amountFrom,
                    amountTo,
                    elapsedTime / dissolveTime      
                );

                _material.SetFloat(_dissolveAmountProperty, lerpedDissolve);

                await UniTask.Yield();
            }
        }
    }
}