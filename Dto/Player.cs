namespace Dto
{
    class Player
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public int Points { get; set; }
        public bool IsAdmin { get; set; }
        public bool GuessedCorrectly { get; set; }
    }
}