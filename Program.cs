using System.Text.Json;
using System.Text.Encodings.Web;
using LocadoraAPI.Models;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var opcoesJson = new JsonSerializerOptions
{
    WriteIndented = true,
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
};

string caminhoFilmes = "filmes.json";
string caminhoClientes = "clientes.json";
string caminhoAlugueis = "alugueis.json";


List<Filme> filmes = File.Exists(caminhoFilmes)
    ? JsonSerializer.Deserialize<List<Filme>>(File.ReadAllText(caminhoFilmes)) ?? new()
    : new();

List<Cliente> clientes = File.Exists(caminhoClientes)
    ? JsonSerializer.Deserialize<List<Cliente>>(File.ReadAllText(caminhoClientes)) ?? new()
    : new();

List<Aluguel> alugueis = File.Exists(caminhoAlugueis)
    ? JsonSerializer.Deserialize<List<Aluguel>>(File.ReadAllText(caminhoAlugueis)) ?? new()
    : new();

// ------------------- ROTAS FILMES -------------------

app.MapGet("/filmes", () => filmes);

app.MapGet("/filmes/{id}", (int id) =>
{
    var filme = filmes.FirstOrDefault(f => f.Id == id);
    return filme is not null ? Results.Ok(filme) : Results.NotFound("Filme não encontrado.");
});

app.MapPost("/filmes", (Filme novo) =>
{
    if (!novo.Validar(out string erro))
        return Results.BadRequest(erro);

    if (filmes.Any(f => f.Id == novo.Id))
        return Results.BadRequest("Filme com esse ID já existe.");

    filmes.Add(novo);
    File.WriteAllText(caminhoFilmes, JsonSerializer.Serialize(filmes, opcoesJson));

    return Results.Created($"/filmes/{novo.Id}", novo);
});

app.MapPut("/filmes/{id}", (int id, Filme atualizacao) =>
{
    var index = filmes.FindIndex(f => f.Id == id);
    if (index == -1) return Results.NotFound("Filme não encontrado.");

    if (!atualizacao.Validar(out string erro))
        return Results.BadRequest(erro);

    atualizacao.Id = id;
    filmes[index] = atualizacao;
    File.WriteAllText(caminhoFilmes, JsonSerializer.Serialize(filmes, opcoesJson));

    return Results.Ok(atualizacao);
});

app.MapDelete("/filmes/{id}", (int id) =>
{
    var filme = filmes.FirstOrDefault(f => f.Id == id);
    if (filme is null) return Results.NotFound("Filme não encontrado.");

    filmes.Remove(filme);
    File.WriteAllText(caminhoFilmes, JsonSerializer.Serialize(filmes, opcoesJson));

    return Results.Ok($"Filme ID {id} excluído com sucesso.");
});

// ------------------- ROTAS CLIENTES -------------------

app.MapGet("/clientes", () => clientes);

app.MapGet("/clientes/{id}", (int id) =>
{
    var cliente = clientes.FirstOrDefault(c => c.Id == id);
    return cliente is not null ? Results.Ok(cliente) : Results.NotFound("Cliente não encontrado.");
});

app.MapPost("/clientes", (Cliente novo) =>
{
    if (!novo.Validar(out string erro))
        return Results.BadRequest(erro);

    if (clientes.Any(c => c.Id == novo.Id))
        return Results.BadRequest("Cliente com esse ID já existe.");

    clientes.Add(novo);
    File.WriteAllText(caminhoClientes, JsonSerializer.Serialize(clientes, opcoesJson));

    return Results.Created($"/clientes/{novo.Id}", novo);
});

app.MapPut("/clientes/{id}", (int id, Cliente atualizacao) =>
{
    var index = clientes.FindIndex(c => c.Id == id);
    if (index == -1) return Results.NotFound("Cliente não encontrado.");

    if (!atualizacao.Validar(out string erro))
        return Results.BadRequest(erro);

    atualizacao.Id = id;
    clientes[index] = atualizacao;
    File.WriteAllText(caminhoClientes, JsonSerializer.Serialize(clientes, opcoesJson));

    return Results.Ok(atualizacao);
});

app.MapDelete("/clientes/{id}", (int id) =>
{
    var cliente = clientes.FirstOrDefault(c => c.Id == id);
    if (cliente is null) return Results.NotFound("Cliente não encontrado.");

    clientes.Remove(cliente);
    File.WriteAllText(caminhoClientes, JsonSerializer.Serialize(clientes, opcoesJson));

    return Results.Ok($"Cliente ID {id} excluído com sucesso.");
});

// ------------------- ROTAS ALUGUEIS -------------------

app.MapGet("/alugueis", () => alugueis);

app.MapGet("/alugueis/{id}", (int id) =>
{
    var aluguel = alugueis.FirstOrDefault(a => a.Id == id);
    return aluguel is not null
        ? Results.Ok(aluguel)
        : Results.NotFound("Aluguel não encontrado.");
});

app.MapPost("/alugueis", (int clienteId, int filmeId) =>
{
    var cliente = clientes.FirstOrDefault(c => c.Id == clienteId);
    var filme = filmes.FirstOrDefault(f => f.Id == filmeId);

    if (cliente == null)
        return Results.NotFound("Cliente não encontrado.");
    if (filme == null)
        return Results.NotFound("Filme não encontrado.");
    if (!filme.Disponivel)
        return Results.BadRequest("Filme não está disponível para aluguel.");

    filme.Alugar();

    int novoId = alugueis.Count > 0 ? alugueis.Max(a => a.Id) + 1 : 1;

    var aluguel = new Aluguel
    {
        Id = novoId,
        ClienteId = cliente.Id,
        FilmeId = filme.Id,
        DataAluguel = DateTime.Now
    };

    alugueis.Add(aluguel);

    File.WriteAllText(caminhoAlugueis, JsonSerializer.Serialize(alugueis, opcoesJson));
    File.WriteAllText(caminhoFilmes, JsonSerializer.Serialize(filmes, opcoesJson));

    return Results.Ok($"Filme '{filme.Titulo}' alugado para cliente '{cliente.Nome}'.");
});


app.MapPost("/devolucoes", (int filmeId) =>
{
    var filme = filmes.FirstOrDefault(f => f.Id == filmeId);
    if (filme == null) return Results.NotFound("Filme não encontrado.");
    if (filme.Disponivel) return Results.BadRequest("Filme já está disponível.");

    filme.Devolver();

    var aluguel = alugueis.FirstOrDefault(a => a.FilmeId == filmeId && a.DataDevolucao == null);
    if (aluguel != null)
        aluguel.DataDevolucao = DateTime.Now;

    File.WriteAllText(caminhoAlugueis, JsonSerializer.Serialize(alugueis, opcoesJson));
    File.WriteAllText(caminhoFilmes, JsonSerializer.Serialize(filmes, opcoesJson));

    return Results.Ok($"Filme '{filme.Titulo}' devolvido com sucesso.");
});

app.MapPut("/alugueis/{id}", (int id, Aluguel atualizacao) =>
{
    var index = alugueis.FindIndex(a => a.Id == id);
    if (index == -1)
        return Results.NotFound("Aluguel não encontrado.");

    var cliente = clientes.FirstOrDefault(c => c.Id == atualizacao.ClienteId);
    var filme = filmes.FirstOrDefault(f => f.Id == atualizacao.FilmeId);

    if (cliente == null)
        return Results.BadRequest("Cliente inválido.");
    if (filme == null)
        return Results.BadRequest("Filme inválido.");

    atualizacao.Id = id;
    alugueis[index] = atualizacao;

    File.WriteAllText(caminhoAlugueis, JsonSerializer.Serialize(alugueis, opcoesJson));

    return Results.Ok(atualizacao);
});


app.MapDelete("/alugueis/{id}", (int id) =>
{
    var index = alugueis.FindIndex(a => a.Id == id);
    if (index == -1)
        return Results.NotFound("Aluguel não encontrado.");

    alugueis.RemoveAt(index);

    File.WriteAllText(caminhoAlugueis, JsonSerializer.Serialize(alugueis, opcoesJson));

    return Results.Ok($"Aluguel ID {id} excluído com sucesso.");
});

app.Run();
