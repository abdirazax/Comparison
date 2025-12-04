using UnityEngine;
using UnityEngine.Serialization;

namespace TCG_Card_System.Scripts
{
    public class CardAttackDisplay : MonoBehaviour
    {
        MaterialPropertyBlock _onCardMaterialPropertyBlock;
        MaterialPropertyBlock _onEnemyMaterialPropertyBlock;
        
        SpriteRenderer _spriteRenderer;
        [SerializeField] private SpriteRenderer onEnemyRenderer;
        [SerializeField] private SpriteRenderer onBulletTrailRenderer;
        private CardAttackTemplate _attackTemplate;
        
        private static readonly int ShaderLocalTime = Shader.PropertyToID("_LocalTime");
        private static readonly int ShaderAttackIndex = Shader.PropertyToID("_AttackIndex");
        private static readonly int ShaderDissolveAlpha = Shader.PropertyToID("_DissolveAlpha");
        private static readonly int ShaderCardMask = Shader.PropertyToID("_CardMask");
        private static readonly int ShaderSlotSize = Shader.PropertyToID("_SlotSize");
        private static readonly int ShaderRandomSeedPerAttack = Shader.PropertyToID("_RandomSeedPerAttack");
        private static readonly int ShaderTint = Shader.PropertyToID("_Tint");

        private bool _initialized = false;
        private Coroutine _coroutineShowTooltip;

        public void UpdateUI(Card card)
        {
            if (!_initialized)
                InitializeUI(card);
            _spriteRenderer.SetPropertyBlock(_onCardMaterialPropertyBlock);
        }

        

        private void InitializeUI(Card card)
        {
            _initialized = true;
            _attackTemplate = card.Template.attackTemplate;
            
            _onCardMaterialPropertyBlock = new MaterialPropertyBlock();
            _onEnemyMaterialPropertyBlock = new MaterialPropertyBlock();

            _spriteRenderer = GetComponent<SpriteRenderer>();
            _spriteRenderer.sprite = _attackTemplate?.onCardSprite[card.AttackIndexInCombo];
            
            _spriteRenderer.material = _attackTemplate?.onCardMaterial;
            onEnemyRenderer.material = _attackTemplate?.onTargetMaterial;
            
            
            _spriteRenderer.size= new Vector2(card.Template.slotSize * .5f, 1);
            onEnemyRenderer.size = new Vector2(1, 1);
            
            _onCardMaterialPropertyBlock.SetTexture(ShaderCardMask, card.Template.cardSkin.frontMaskSprite[card.Template.slotSize - 1]);
            _onCardMaterialPropertyBlock.SetFloat(ShaderLocalTime, 0);
            _onCardMaterialPropertyBlock.SetInt(ShaderSlotSize, card.Template.slotSize);
            _onEnemyMaterialPropertyBlock.SetFloat(ShaderLocalTime, 0);
        }

        public void AnimateAttackOnCard
            (float localTime, 
                int attackIndex, 
                float dissolveAlpha,
                Vector2 randomSeedPerAttack)
        {
            if (!_initialized)
            {
                Debug.LogError("CardAttackDisplay not initialized before AnimateAttack call.");
                return;
            }
            _spriteRenderer.sprite = _attackTemplate?.onCardSprite[attackIndex];
            _onCardMaterialPropertyBlock.SetFloat(ShaderLocalTime, localTime);
            _onCardMaterialPropertyBlock.SetFloat(ShaderDissolveAlpha, dissolveAlpha);
            _onCardMaterialPropertyBlock.SetInt(ShaderAttackIndex, attackIndex);
            _onCardMaterialPropertyBlock.SetColor(ShaderTint, _attackTemplate.onCardColor[attackIndex]);
            _onCardMaterialPropertyBlock.SetVector(ShaderRandomSeedPerAttack, randomSeedPerAttack);
            _spriteRenderer.SetPropertyBlock(_onCardMaterialPropertyBlock);
        }
        
        public void AnimateAttackOnEnemy
            (float localTime, 
                int attackIndex, 
                float dissolveAlpha, 
                TeamDisplay enemyTeamDisplay,
                Vector2 randomSeedPerAttack)
        {
            if (!_initialized)
            {
                Debug.LogError("CardAttackDisplay not initialized before AnimateAttack call.");
                return;
            }
            Debug.Log("Animating attack on enemy with localTime: " + localTime + ", attackIndex: " + attackIndex + ", dissolveAlpha: " + dissolveAlpha);
            onEnemyRenderer.sprite = _attackTemplate?.onTargetSprite[attackIndex];
            onEnemyRenderer.transform.position = enemyTeamDisplay.portraitRenderer.transform.position;
            onEnemyRenderer.size = new Vector2(1, 1);
            
            _onEnemyMaterialPropertyBlock.SetFloat(ShaderLocalTime, localTime);
            _onEnemyMaterialPropertyBlock.SetFloat(ShaderDissolveAlpha, dissolveAlpha);
            _onEnemyMaterialPropertyBlock.SetInt(ShaderAttackIndex, attackIndex);
            _onEnemyMaterialPropertyBlock.SetColor(ShaderTint, _attackTemplate.onTargetColor[attackIndex]);
            _onEnemyMaterialPropertyBlock.SetVector(ShaderRandomSeedPerAttack, randomSeedPerAttack);
            onEnemyRenderer.SetPropertyBlock(_onEnemyMaterialPropertyBlock);
        }
    }
}