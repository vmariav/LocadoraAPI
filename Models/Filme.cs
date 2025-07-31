namespace LocadoraAPI.Models;

public class Filme
{
  public int Id { get; set; }
  public string Titulo { get; set; } = string.Empty;
  public string Genero { get; set; } = string.Empty;
  public int Ano { get; set; }
  public bool Disponivel { get; set; } = true;

  public bool Validar(out string erro)
  {
    erro = string.Empty;

    if (Id <= 0)
      erro = "ID deve ser maior que 0.";
    else if (string.IsNullOrWhiteSpace(Titulo))
      erro = "Título é obrigatório.";
    else if (Ano < 1900 || Ano > DateTime.Now.Year)
      erro = "Ano inválido.";

    return string.IsNullOrEmpty(erro);
  }

  public void Alugar()
  {
    Disponivel = false;
  }

  public void Devolver()
  {
    Disponivel = true;
  }
}
/*

http://localhost:5101/alugueis?clienteId=1&filmeId=2

http://localhost:5101/devolucoes?filmeId=2

*/