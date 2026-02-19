namespace NBA.EFCore.DTOs
{
    public class PlayerDto
    {
        public string FullName { get; set; } = null!;
        public string TeamName { get; set; } = null!;
        public string Position { get; set; } = null!;
        public int Age { get; set; }
    }
    
}