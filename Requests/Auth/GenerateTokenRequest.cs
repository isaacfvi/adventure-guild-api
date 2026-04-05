using System.ComponentModel.DataAnnotations;

public class GenerateTokenRequest
{
    [Required(ErrorMessage = "Campo obrigatório")]
    public required string Role { get; set; }
}
