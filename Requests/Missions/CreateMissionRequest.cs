using System.ComponentModel.DataAnnotations;

public class CreateMissionRequest
{
    [Required(ErrorMessage = "Campo obrigatório")]
    public required string Name { get; set; }

    [Required(ErrorMessage = "Campo obrigatório")]
    public required string Task { get; set; }

    [Range(0.01, float.MaxValue, ErrorMessage = "Recompensa deve ser maior que zero")]
    public float Reward { get; set; }

    [Required(ErrorMessage = "Campo obrigatório")]
    public Guid GuildId { get; set; }
}