using TCG_Card_System.Scripts.Interfaces;
using UnityEngine;

namespace TCG_Card_System.Scripts
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class FitSpriteToScreen : MonoBehaviour, IResizeOnScreenChange
    {
        public Camera targetCamera;
        private SpriteRenderer _spriteRenderer;
        [SerializeField]
        float widthFactor = 100f, heightFactor = 100f; 
        [SerializeField]
        bool keepAspectRatio = true;
        private float _aspectRatio;

        private void Start()
        {
        
            if (targetCamera == null)
                targetCamera = Camera.main;
            if (targetCamera == null)
            {
                Debug.LogError("No camera found!");
                return;
            }
        
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer.sprite == null)
            {
                Debug.LogWarning("No sprite found!");
                return;
            }
        
            if (keepAspectRatio)
            {
                _aspectRatio = _spriteRenderer.sprite.bounds.size.x / _spriteRenderer.sprite.bounds.size.y;
            }
            Resize();
        }

        public void Resize()
        {
            Vector2 spriteSize = _spriteRenderer.sprite.bounds.size;
            float z = Vector3.Dot(transform.position - targetCamera.transform.position, targetCamera.transform.forward);
        
            Vector3 bottomLeft = targetCamera.ViewportToWorldPoint(new Vector3(0, 0, z));
            Vector3 topRight = targetCamera.ViewportToWorldPoint(new Vector3(1, 1, z));
            Vector3 screenWorldSize = topRight - bottomLeft;

            float width = screenWorldSize.x / spriteSize.x;
            float height = screenWorldSize.z / spriteSize.y;

            if (keepAspectRatio)
            {
                if (width > height)
                {
                    width = height * _aspectRatio;
                }
                else
                {
                    height = width / _aspectRatio;
                }
            }
            _spriteRenderer.size = new Vector2(width*widthFactor/50f, height*heightFactor/50f);
        }

    
    }
}

