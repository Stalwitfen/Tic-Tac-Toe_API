namespace TicTacToe_API
{
    public class Step
    {
        public int CellId { get; set; }         //0-8
        public string PlayerNum { get; set; }   //"first" or "second"
        public int SessionId { get; set; }
    }
}
