//creo validar log in

using Microsoft.Data.SqlClient;
using Dapper;
namespace log_in.Models;

public class BD
{
    private string _connectionString = @"Server=localhost;DataBase=Usuarios; Integrated Security=True; TrustServerCertificate=True;";
    public void Registrarse(Usuario usuario)
    {
        string query = "INSERT INTO Usuario (NombreUsuario, Contraseña, Nombre, Apellido) VALUES (@nombre, @apellido, @usuario, @contrasena)";
        using(SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Execute(query, new { nombre = usuario.Nombre, apellido = usuario.Apellido, usuario = usuario.NombreUsuario, contrasena = usuario.Contraseña });
        }
    }

    public bool ValidarRegistro(Usuario usuario)
    {
        List<Usuario> listaUsuario = new List<Usuario>();
        string query = "select * from Usuario WHERE usuario = @usuario";
        using(SqlConnection connection = new SqlConnection(_connectionString))
        {
            listaUsuario = connection.Query<Usuario>(query, new { usuario = usuario.usuario }).ToList();
        }
        if (listaUsuario.Count > 0)
        {
            return false;
        }
        return true;
    }

    public Usuario? ValidarLogin(Usuario usuario)
    {
        Usuario usuarioExistente = new Usuario();
        string query = "select * from Usuario WHERE NombreUsuario = @usuario AND Constraseña = @contrasena";
        using(SqlConnection connection = new SqlConnection(_connectionString))
        {
            usuarioExistente = connection.QueryFirstOrDefault<Usuario>(query, new { usuario = usuario.NombreUsuario, contrasena = usuario.Contraseña });
        }
        return usuarioExistente;
    }
    }
    //ActualizarContrasena
    public void ActualizarContrasena(Usuario usuario, string nuevaContrasena, string contrasenaActual)
    {
        string query = "UPDATE Usuario SET Constraseña = @contrasena WHERE NombreUsuario = @usuario AND Constraseña = @contrasenaActual";
        using(SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Execute(query, new { contrasena = nuevaContrasena, usuario = usuario.NombreUsuario, contrasenaActual = contrasenaActual });
        }
    }