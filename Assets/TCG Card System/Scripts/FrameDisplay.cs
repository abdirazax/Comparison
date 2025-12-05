using Cysharp.Threading.Tasks;
using TCG_Card_System.Scripts.Effects;
using TMPro;
using UnityEngine;

namespace TCG_Card_System.Scripts
{
    public class FrameDisplay : MonoBehaviour, IFrameDisplay
    {
        [SerializeField]
        private TextMeshPro attackText;
        
        [SerializeField]
        private TextMeshPro heathText;

        [SerializeField] 
        private Texture frameTexture;
        [SerializeField] 
        private Texture frameMaskTexture;

        [Header("Indicators")]
        
        [SerializeField]
        private GameObject indicatorsGameObject;
        private ScaleEffect _indicatorsScaleEffect;
        
        [SerializeField]
        private GameObject indicatorDamageTakenGameObject;
        private TMP_Text _indicatorDamageTakenText;
        
        [Header("Effects")]
        
        private static readonly int ShaderFrameImageId = Shader.PropertyToID("_FrameImage");
        private static readonly int ShaderFrameMaskId = Shader.PropertyToID("_FrameMask");
        private static readonly int ShaderBackgroundImageId = Shader.PropertyToID("_BackgroundImage");
        private static readonly int ShaderCharacterImageId = Shader.PropertyToID("_CharacterImage");

        private Material _material;
        
        private void Awake()
        {
            _material = GetComponent<MeshRenderer>().material;
            _indicatorsScaleEffect = indicatorsGameObject.GetComponent<ScaleEffect>();

            _indicatorDamageTakenText = indicatorDamageTakenGameObject.GetComponentInChildren<TMP_Text>();
            
            _indicatorsScaleEffect.SetScale(Vector3.zero);
            indicatorDamageTakenGameObject.SetActive(false);
        }
        
        public void UpdateUI(Card card)
        {
            _material.SetTexture(ShaderFrameImageId, frameTexture);
            _material.SetTexture(ShaderFrameMaskId, frameMaskTexture);
            
            _material.SetTexture(ShaderBackgroundImageId, card.Template.backgroundImage);
            _material.SetTexture(ShaderCharacterImageId, card.Template.characterImage);
            
            attackText.text = card.Data.Attack.ToString();
            heathText.text = card.Data.Health.ToString();
        }

        public async UniTask ShowDamageTaken(int damage)
        {
            indicatorDamageTakenGameObject.SetActive(true);
            _indicatorDamageTakenText.text = damage.ToString();
            
            await _indicatorsScaleEffect.ScaleUpAndDown(0.15f, 0.8f);
            
            indicatorDamageTakenGameObject.SetActive(false);
            _indicatorDamageTakenText.text = string.Empty;
        }
    }
}