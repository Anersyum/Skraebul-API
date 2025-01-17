namespace Skraebul_API.Dto;

internal class Player
{
    public int Id { get; set; }
    
    public string Username { get; set; }
    
    public int Points { get; set; }
    
    public int GottenPoints { get; set; }
    
    public bool IsAdmin { get; set; }
    
    public bool GuessedCorrectly { get; set; }
}