using Cysharp.Threading.Tasks;
using TCG_Card_System.Scripts.Animations;
using UnityEngine;

namespace TCG_Card_System.Scripts.Effects
{
    public class HighlightEffect : MonoBehaviourExtended<GameObject>
    {
        [SerializeField]
        private string thicknessPropertyReference = "_OutlineThickness";
        
        [SerializeField]
        private string colorPropertyReference = "_OutlineColor";
        
        [SerializeField]
        public float animateTime = 0.4f;
        
        private int _thicknessAmountProperty;
        private int _colorProperty;
        
        private Material _material;
        private Color _color; 
            
        private bool _visible;

        private void Awake()
        {
            _thicknessAmountProperty = Shader.PropertyToID(thicknessPropertyReference);
            _colorProperty = Shader.PropertyToID(colorPropertyReference);
            
            _material = GetComponent<SpriteRenderer>().material;
            _color = _material.GetColor(_colorProperty);
            _visible = _material.GetFloat(_thicknessAmountProperty) > 0f;
        }

        public async UniTask Show(float thickness = 8, Color? color = null, float intensity = 4)
        {
            if (_visible)
                return;

            if (color.HasValue)
            {
                _material.SetColor
                (
                    _colorProperty,
                    new Vector4
                    (
                        color.Value.r / 100,
                        color.Value.g / 100,
                        color.Value.b / 100,
                        (255f - color.Value.a) / 255f
                    ) * Mathf.Pow(2.0F, intensity)
                );
            }
            
            _visible = true;
            
            await AnimationStart(gameObject, () => AnimationDefinition(thickness));
        }
        
        public async UniTask Hide()
        {
            if (!_visible)
                return;
            
            _visible = false;

            await AnimationStart(gameObject, () => AnimationDefinition(0f));
            
            _material.SetColor(_colorProperty, _color);
        }

        private async UniTask AnimationDefinition(float thickness)
        {
            var amountFrom = _material.GetFloat(_thicknessAmountProperty);
            var amountTo = thickness;
            var elapsedTime = 0f;
            
            while (elapsedTime < animateTime)
            {
                elapsedTime += Time.deltaTime;

                var lerpedHighlight = Mathf.Lerp
                (
                    amountFrom,
                    amountTo,
                    elapsedTime / animateTime
                );

                _material.SetFloat(_thicknessAmountProperty, lerpedHighlight);

                await UniTask.Yield();
            }
        }
    }
}