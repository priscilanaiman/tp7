//como funciona el session
//crear metodo para iniciar sesion
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using tp7.Models;

namespace tp7.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Registrarse(Usuario usuario)
    {
        BD baseDatos = new BD();

        if (!baseDatos.ValidarRegistro(usuario))
        {
            ViewBag.Message = "El usuario ya existe. Podés iniciar sesión o elegir otro nombre de usuario.";
            return View("Index");
        }

        baseDatos.Registrarse(usuario);

        Usuario usuarioCreado = baseDatos.ValidarLogin(usuario);
        if (usuarioCreado is not null)
        {
            HttpContext.Session.SetInt32("IdUsuario", usuarioCreado.Id);
            HttpContext.Session.SetString("USUARIO", usuarioCreado.NombreUsuario ?? string.Empty);
            HttpContext.Session.SetString("NOMBRE", usuarioCreado.Nombre ?? string.Empty);
        }

        return RedirectToAction("RedSocial");
    }

    [HttpPost]
    public IActionResult IniciarSesion(Usuario usuario)
    {
        BD baseDatos = new BD();
        Usuario? usuarioExistente = baseDatos.ValidarLogin(usuario);

        if (usuarioExistente is not null)
        {
            HttpContext.Session.SetInt32("IdUsuario", usuarioExistente.Id);
            HttpContext.Session.SetString("USUARIO", usuarioExistente.NombreUsuario ?? string.Empty);
            HttpContext.Session.SetString("NOMBRE", usuarioExistente.Nombre ?? string.Empty);
            return RedirectToAction("RedSocial");
        }

        ViewBag.Message = "Usuario o contraseña incorrectos.";
        return View("IniciarSesion");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index");
    }

    public IActionResult CambiarContrasena()
    {
        int? idUsuario = HttpContext.Session.GetInt32("IdUsuario");
        if (idUsuario is null)
        {
            return RedirectToAction("Index");
        }

        BD baseDatos = new BD();
        Usuario usuarioActual = baseDatos.BuscarUsuario(idUsuario.Value);
        ViewBag.ObjUsuario = usuarioActual;
        return View();
    }

    [HttpPost]
    public IActionResult ActualizarContrasena(string contrasenaNueva, Usuario objUsuario, string contrasenaActual)
    {
        BD baseDatos = new BD();
        int? idUsuario = HttpContext.Session.GetInt32("IdUsuario");

        if (idUsuario is null)
        {
            return RedirectToAction("Index");
        }

        objUsuario.Id = idUsuario.Value;
        baseDatos.ActualizarContrasena(objUsuario, contrasenaNueva, contrasenaActual);
        return RedirectToAction("RedSocial");
    }

    public IActionResult RedSocial()
    {
        int? idUsuario = HttpContext.Session.GetInt32("IdUsuario");
        if (idUsuario is null)
        {
            return RedirectToAction("Index");
        }

        BD baseDatos = new BD();
        List<dynamic> publicaciones = baseDatos.ObtenerPublicacionesPaginadas(0, 10, idUsuario.Value);
        ViewBag.Publicaciones = publicaciones;
        return View();
    }

    [HttpPost]
    public IActionResult CrearPublicacion(Publicacion publicacion)
    {
        int? idUsuario = HttpContext.Session.GetInt32("IdUsuario");
        if (idUsuario is null)
        {
            return RedirectToAction("Index");
        }

        publicacion.IdUsuario = idUsuario.Value;
        publicacion.FechaPublicacion = DateTime.Now;

        BD baseDatos = new BD();
        baseDatos.CrearPublicacion(publicacion);
        return RedirectToAction("RedSocial");
    }

    [HttpPost]
    public IActionResult ToggleLike(int idPublicacion)
    {
        int? idUsuario = HttpContext.Session.GetInt32("IdUsuario");
        if (idUsuario is null)
        {
            return RedirectToAction("Index");
        }

        BD baseDatos = new BD();
        baseDatos.ToggleLike(idPublicacion, idUsuario.Value);
        return RedirectToAction("RedSocial");
    }

    [HttpPost]
    public IActionResult AgregarComentario(int idPublicacion, string texto)
    {
        int? idUsuario = HttpContext.Session.GetInt32("IdUsuario");
        if (idUsuario is null)
        {
            return RedirectToAction("Index");
        }

        if (string.IsNullOrWhiteSpace(texto))
        {
            return RedirectToAction("RedSocial");
        }

        Comentario comentario = new Comentario
        {
            IdPublicacion = idPublicacion,
            IdUsuarioComenta = idUsuario.Value,
            Texto = texto,
            FechaComentario = DateTime.Now
        };

        BD baseDatos = new BD();
        baseDatos.AgregarComentario(comentario);
        return RedirectToAction("RedSocial");
    }

    public IActionResult VerMasPublicaciones(int offset)
    {
        int? idUsuario = HttpContext.Session.GetInt32("IdUsuario");
        if (idUsuario is null)
        {
            return RedirectToAction("Index");
        }

        BD baseDatos = new BD();
        List<dynamic> publicaciones = baseDatos.ObtenerPublicacionesPaginadas(offset, 10, idUsuario.Value);
        ViewBag.Publicaciones = publicaciones;
        return View("RedSocial");
    }
}
