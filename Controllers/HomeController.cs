//como funciona el session
//crear metodo para iniciar sesion
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using log_in.Models;

namespace log_in.Controllers;

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
    public IActionResult Registrarse(Usuario user)
    {   
        BD BD = new BD();
        if (!BD.ValidarRegistro(user))
        {
            ViewBag.Message = "El usuario ya existe. Puedes probar iniciar sesión con ese usuario o registrarte con otro.";
            return View("Index");
        }
        else
        {
            BD.Registrarse(user);
            HttpContext.Session.SetString("USUARIO", user.NombreUsuario);
            HttpContext.Session.SetString("NOMBRE", user.Nombre);
            return RedirectToAction("RedSocial");        }
    }

    [HttpPost]
    public IActionResult IniciarSesion(Usuario user)
    {
        BD BD = new BD();
        Usuario? usuarioExistente = BD.ValidarLogin(user);

        if (usuarioExistente is not null)
        {
            HttpContext.Session.SetString("USUARIO", usuarioExistente.NombreUsuario);
            HttpContext.Session.SetString("NOMBRE", usuarioExistente.Nombre);
            return RedirectToAction("RedSocial");        
        }
    else
    {
        ViewBag.Message = "Usuario o contraseña incorrectos.";
        return View("IniciarSesion");
    }
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

    [HttpPost]
    public IActionResult ActualizarContrasena(string contrasenaNueva, Usuario ObjUsuario, string contrasenaActual)
    {
        BD bd = new BD();

        bd.ActualizarContrasena(ObjUsuario, contrasenaNueva, contrasenaActual);
        return RedirectToAction("RedSocial");
    }

    public IActionResult CambiarContrasena()
    {
        BD bd = new BD();
        if (HttpContext.Session.GetString("DNI") is null)
        {
            return RedirectToAction("Index");
        }
        Usuario ObjUsuario = bd.BuscarUsuario(int.Parse(HttpContext.Session.GetString("DNI")));
        ViewBag.ObjUsuario = ObjUsuario;
        return View();
    }
    
    public IActionResult RedSocial()
    {
        BD bd = new BD();
        if (HttpContext.Session.GetString("USUARIO") is null)
        {
            return RedirectToAction("Index");
        }
        return View("RedSocial");
    }


}
