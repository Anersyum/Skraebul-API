namespace Skraebul_API.Dto;

internal class RoundInfo
{
    public bool Won { get; set; }
    
    public string Username { get; set; }
    
    public bool IsLastRound { get; set; }
    
    public int Round { get; set; }
}