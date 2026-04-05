using System.ComponentModel.DataAnnotations;

public class CreateGuildRequest
{
    [Required(ErrorMessage = "Campo obrigatório")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Nome deve ter entre 3 e 100 caracteres")]
    public required string Name { get; set; }
}