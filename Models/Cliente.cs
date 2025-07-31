namespace LocadoraAPI.Models;

public class Cliente
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public bool Validar(out string erro)
    {
        erro = string.Empty;

        if (Id <= 0)
            erro = "ID deve ser maior que 0.";
        else if (string.IsNullOrWhiteSpace(Nome))
            erro = "Nome é obrigatório.";
        else if (!Email.Contains("@"))
            erro = "E-mail inválido.";

        return string.IsNullOrEmpty(erro);
    }
}
