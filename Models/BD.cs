//creo validar log in

using Microsoft.Data.SqlClient;
using Dapper;
using tp7.Models;

namespace tp7.Models;

public class BD
{
    private string _connectionString = @"Server=localhost;Database=DBRedSocial;Integrated Security=True;TrustServerCertificate=True;";

    public void Registrarse(Usuario usuario)
    {
        string query = @"INSERT INTO Usuarios (NombreUsuario, Contraseña, Nombre, Apellido)
                         VALUES (@NombreUsuario, @Contraseña, @Nombre, @Apellido);";

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Execute(query, new
            {
                NombreUsuario = usuario.NombreUsuario,
                Contraseña = usuario.Contraseña,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido
            });
        }
    }

    public bool ValidarRegistro(Usuario usuario)
    {
        string query = "SELECT Id FROM Usuarios WHERE NombreUsuario = @NombreUsuario";
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            List<Usuario> listaUsuario = connection.Query<Usuario>(query, new { NombreUsuario = usuario.NombreUsuario }).ToList();
            return listaUsuario.Count == 0;
        }
    }

    public Usuario? ValidarLogin(Usuario usuario)
    {
        string query = "SELECT * FROM Usuarios WHERE NombreUsuario = @NombreUsuario AND Contraseña = @Contraseña";
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            Usuario? usuarioExistente = connection.QueryFirstOrDefault<Usuario>(query, new
            {
                NombreUsuario = usuario.NombreUsuario,
                Contraseña = usuario.Contraseña
            });
            return usuarioExistente;
        }
    }

    public Usuario BuscarUsuario(int idUsuario)
    {
        string query = "SELECT * FROM Usuarios WHERE Id = @IdUsuario";
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            Usuario usuario = connection.QueryFirstOrDefault<Usuario>(query, new { IdUsuario = idUsuario }) ?? new Usuario();
            return usuario;
        }
    }

    public void ActualizarContrasena(Usuario usuario, string nuevaContrasena, string contrasenaActual)
    {
        string query = @"UPDATE Usuarios SET Contraseña = @NuevaContrasena WHERE Id = @IdUsuario AND Contraseña = @ContrasenaActual";

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Execute(query, new
            {
                NuevaContrasena = nuevaContrasena,
                IdUsuario = usuario.Id,
                ContrasenaActual = contrasenaActual
            });
        }
    }

    public void CrearPublicacion(Publicacion publicacion)
    {
        string query = @"INSERT INTO Publicaciones (IdUsuario, Titulo, Descripcion, Imagen, FechaPublicacion)
                         VALUES (@IdUsuario, @Titulo, @Descripcion, @Imagen, @FechaPublicacion);";

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Execute(query, publicacion);
        }
    }

    public List<dynamic> ObtenerPublicacionesPaginadas(int offset, int limit, int idUsuarioActual)
    {
        string query = @"
            SELECT
                Publicaciones.Id,
                Publicaciones.Titulo,
                Publicaciones.Descripcion,
                Publicaciones.Imagen,
                Publicaciones.FechaPublicacion,
                Usuarios.Id AS IdUsuario,
                Usuarios.NombreUsuario,
                COUNT(DISTINCT LikesTotales.Id) AS CantidadLikes,
                MAX(CASE WHEN MiLike.IdUsuario = @IdUsuarioActual THEN 1 ELSE 0 END) AS LeDioLike,
                COUNT(DISTINCT Comentarios.Id) AS CantidadComentarios
            FROM Publicaciones
            INNER JOIN Usuarios
                ON Usuarios.Id = Publicaciones.IdUsuario
            LEFT JOIN PublicacionesMeGusta AS LikesTotales
                ON LikesTotales.IdPublicación = Publicaciones.Id
            LEFT JOIN PublicacionesMeGusta AS MiLike
                ON MiLike.IdPublicación = Publicaciones.Id
               AND MiLike.IdUsuario = @IdUsuarioActual
            LEFT JOIN Comentarios
                ON Comentarios.IdPublicacion = Publicaciones.Id
            GROUP BY
                Publicaciones.Id,
                Publicaciones.Titulo,
                Publicaciones.Descripcion,
                Publicaciones.Imagen,
                Publicaciones.FechaPublicacion,
                Usuarios.Id,
                Usuarios.NombreUsuario
            ORDER BY
                Publicaciones.FechaPublicacion DESC
            OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY;";

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            List<dynamic> lista = connection.Query(query, new
            {
                IdUsuarioActual = idUsuarioActual,
                Offset = offset,
                Limit = limit
            }).ToList();

            return lista;
        }
    }

    public void ToggleLike(int idPublicacion, int idUsuario)
    {
        string existeQuery = @"SELECT Id FROM PublicacionesMeGusta WHERE IdPublicación = @IdPublicacion AND IdUsuario = @IdUsuario";

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            int? existe = connection.QueryFirstOrDefault<int?>(existeQuery, new
            {
                IdPublicacion = idPublicacion,
                IdUsuario = idUsuario
            });

            if (existe is not null)
            {
                string deleteQuery = @"DELETE FROM PublicacionesMeGusta WHERE IdPublicación = @IdPublicacion AND IdUsuario = @IdUsuario";
                connection.Execute(deleteQuery, new
                {
                    IdPublicacion = idPublicacion,
                    IdUsuario = idUsuario
                });
                return;
            }

            string insertQuery = @"INSERT INTO PublicacionesMeGusta (IdPublicación, IdUsuario) VALUES (@IdPublicacion, @IdUsuario)";
            connection.Execute(insertQuery, new
            {
                IdPublicacion = idPublicacion,
                IdUsuario = idUsuario
            });
        }
    }

    public void AgregarComentario(Comentario comentario)
    {
        string query = @"INSERT INTO Comentarios (IdPublicacion, IdUsuarioComenta, Texto, FechaComentario)
                         VALUES (@IdPublicacion, @IdUsuarioComenta, @Texto, @FechaComentario)";

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Execute(query, comentario);
        }
    }
}