namespace Dto
{
    class Position
    {
        public double X { get; set; }
        public double Y { get; set; }
        public int Drawing { get; set; }
        // brush color and brush width go to brush model
        public string BrushColor { get; set; }
        public int BrushWidth { get; set; }
        public int CanvasWidth { get; set; }
        public int CanvasHeight { get; set; }
        public bool IsUndo { get; set; }
    }
}