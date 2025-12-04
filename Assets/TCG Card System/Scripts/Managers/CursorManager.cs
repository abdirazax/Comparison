using UnityEngine;

namespace TCG_Card_System.Scripts.Managers
{
    public class CursorManager : MonoBehaviour
    {
        public static CursorManager Instance { get; private set; }

        public Texture2D defaultTexture;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;

            SetDefault();
        }

        public void SetDefault() => 
            Cursor.SetCursor(defaultTexture, Vector2.zero, CursorMode.ForceSoftware);

        public void Show() => Cursor.visible = true;

        public void Hide() => Cursor.visible = false;
    }
}