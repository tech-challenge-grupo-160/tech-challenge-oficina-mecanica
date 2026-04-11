namespace Fiap.TechChallenge.OficinaMecanica.Application.DTOs;

public class LoginResponseDto
{
    public string Token { get; set; } = null!;
    public DateTime ExpiraEm { get; set; }
    public string NomeUsuario { get; set; } = null!;
    public string Role { get; set; } = null!;
}
