using UnityEngine;

namespace TCG_Card_System.Scripts.EventArgs
{
    public class CardDragEventArgs : System.EventArgs
    {
        public Card Card { get; set; }
        public Vector3 Position { get; set; }
    }
}