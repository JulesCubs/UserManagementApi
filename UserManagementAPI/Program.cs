// Program.cs

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;

public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class Program
{
    private static Dictionary<int, Usuario> usuarios = new Dictionary<int, Usuario>();
    private static Dictionary<int, Usuario> tempStorage = new Dictionary<int, Usuario>();

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

        // Middleware de manejo global de errores (debe ir primero)
        app.UseMiddleware<ErrorHandlerMiddleware>();

        // Middleware de autenticación (debe ir antes de auditoría y endpoints)
        app.UseMiddleware<AuthMiddleware>();

        // Usar el middleware de auditoría
        app.UseMiddleware<AuditoriaMiddleware>();

        app.MapGet("/users", GetUsers);
        app.MapGet("/users/{id}", GetUserById);
        app.MapPost("/users/", AddUser);
        app.MapPut("/users/{id}", UpdateUser);
        app.MapDelete("/users/{id}", DeleteUser);

        app.Run();
    }

    public static async Task<IResult> GetUsers(HttpRequest request)
    {
        try
        {
            var ids = request.Query["ids"].Select(idStr =>
            {
                if (int.TryParse(idStr, out int id))
                    return id;
                else
                    throw new ArgumentException("Uno o más IDs no son válidos.");
            }).ToArray();

            if (ids.Length == 0)
            {
                return Results.Ok(usuarios.Values.ToList());
            }

            var resultado = ids
                .Where(id => usuarios.ContainsKey(id))
                .Select(id => usuarios[id])
                .ToList();

            return Results.Ok(resultado);
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    public static async Task<IResult> GetUserById(int id)
    {
        try
        {
            if (usuarios.TryGetValue(id, out var usuario))
            {
                tempStorage[id] = usuario;
                return Results.Ok(usuario);
            }
            else
            {
                return Results.NotFound();
            }
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    public static async Task<IResult> AddUser(Usuario usuario)
    {
        try
        {
            var validation = ValidarUsuario(usuario, true);
            if (!string.IsNullOrEmpty(validation))
                return Results.BadRequest(new { error = validation });

            if (usuarios.ContainsKey(usuario.Id))
                return Results.Conflict(new { error = "El usuario ya existe." });

            usuarios[usuario.Id] = usuario;
            return Results.Created($"/users/{usuario.Id}", usuario);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    public static async Task<IResult> UpdateUser(int id, Usuario usuarioActualizado)
    {
        try
        {
            var validation = ValidarUsuario(usuarioActualizado, false);
            if (!string.IsNullOrEmpty(validation))
                return Results.BadRequest(new { error = validation });

            if (!usuarios.ContainsKey(id))
                return Results.NotFound();

            usuarios[id] = new Usuario
            {
                Id = id,
                Nombre = usuarioActualizado.Nombre,
                Email = usuarioActualizado.Email
            };
            return Results.Ok(usuarios[id]);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    public static async Task<IResult> DeleteUser(int id)
    {
        try
        {
            if (!usuarios.ContainsKey(id))
                return Results.NotFound();

            usuarios.Remove(id);
            return Results.NoContent();
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    private static string ValidarUsuario(Usuario usuario, bool validarId)
    {
        if (usuario == null)
            return "El usuario no puede ser nulo.";

        if (validarId && usuario.Id <= 0)
            return "El Id debe ser un número positivo.";

        if (string.IsNullOrWhiteSpace(usuario.Nombre))
            return "El nombre es obligatorio.";

        if (string.IsNullOrWhiteSpace(usuario.Email))
            return "El email es obligatorio.";

        // Validación simple de email
        if (!usuario.Email.Contains("@") || usuario.Email.Length < 5)
            return "El email no es válido.";

        return string.Empty;
    }
}