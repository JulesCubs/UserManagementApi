// Program.cs

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;

public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Email { get; set; }
}

public class Program
{
    private static List<Usuario> usuarios = new List<Usuario>();
    private static Dictionary<int, Usuario> tempStorage = new Dictionary<int, Usuario>();

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

        app.MapGet("/users", GetUsers);
        app.MapGet("/users/{id}", GetUserById);
        app.MapPost("/users/", AddUser);
        app.MapPut("/users/{id}", UpdateUser);
        app.MapDelete("/users/{id}", DeleteUser);

        app.Run();
    }

    public static async Task<IResult> GetUsers(HttpRequest request)
{
    var ids = request.Query["ids"].Select(int.Parse).ToArray();
    
    if (ids.Length == 0)
    {
        return Results.Ok(usuarios); // Retorna todos los usuarios si no se proporciona ningún ID específico
    }

    var resultado = usuarios.Where(u => ids.Contains(u.Id)).ToList();
    return Results.Ok(resultado);
}

    public static async Task<IResult> GetUserById(int id)
    {
        var usuario = usuarios.Find(u => u.Id == id);
        if (usuario != null)
        {
            tempStorage[id] = usuario;
            return Results.Ok(usuario);
        }
        else
        {
            return Results.NotFound();
        }
    }

    public static async Task<IResult> AddUser(Usuario usuario)
    {
        usuarios.Add(usuario);
        return Results.Created($"/users/{usuario.Id}", usuario);
    }

    public static async Task<IResult> UpdateUser(int id, Usuario usuarioActualizado)
    {
        var index = usuarios.FindIndex(u => u.Id == id);
        if (index == -1) return Results.NotFound();
        usuarios[index] = new Usuario
        {
            Id = id,
            Nombre = usuarioActualizado.Nombre,
            Email = usuarioActualizado.Email
        };
        return Results.Ok(usuarios[index]);
    }

    public static async Task<IResult> DeleteUser(int id)
    {
        var usuario = usuarios.Find(u => u.Id == id);
        if (usuario is null) return Results.NotFound();
        usuarios.Remove(usuario);
        return Results.NoContent();
    }
}