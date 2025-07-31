namespace LocadoraAPI.Models;

using System.Text.Json.Serialization;

public class Aluguel
{
    public int Id { get; set; }
    public int FilmeId { get; set; }
    public int ClienteId { get; set; }
    
    public DateTime DataAluguel { get; set; } = DateTime.Now;
    public DateTime? DataDevolucao { get; set; }

    public string Status => DataDevolucao == null ? "Alugado" : "Devolvido";

    [JsonIgnore]
    public Filme? Filme { get; set; }

    [JsonIgnore]
    public Cliente? Cliente { get; set; }
}
