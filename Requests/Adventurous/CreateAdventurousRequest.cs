using System.ComponentModel.DataAnnotations;

public class CreateAdventurousRequest
{
    [Required(ErrorMessage = "Campo obrigatório")]
    public required string Name { get; set; }

    [Required(ErrorMessage = "Campo obrigatório")]
    public required int Level { get; set; }

    [Required(ErrorMessage = "Campo obrigatório")]
    public required string Class { get; set; }
}