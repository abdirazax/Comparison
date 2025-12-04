namespace TCG_Card_System.Scripts.EventArgs
{
    public class CardPlayEventArgs : System.EventArgs
    {
        public int HandIndex { get; set; }
        public int BoardIndex { get; set; }
        
        public CardData CardData { get; set; }
    }
}