namespace Dto
{
    class Move
    {
        public Position Position { get; set; }
        public int Drawing { get; set; }
        public Brush Brush { get; set; }
        public Canvas Canvas { get; set; }
        public bool IsUndo { get; set; }
    }
}