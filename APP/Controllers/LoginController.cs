using Microsoft.AspNetCore.Mvc;
using APP.Data;
using Microsoft.AspNetCore.Http;

namespace APP.Controllers
{
    public class LoginController : Controller
    {
        private readonly ConexionMySql _db;

        public LoginController(ConexionMySql db)
        {
            _db = db;
        }

        // GET: Login
        public IActionResult Index()
        {
            var rol = HttpContext.Session.GetString("Rol");
            if (!string.IsNullOrEmpty(rol))
            {
                return RedirectToAction("Index", "Consolas");
            }

            return View();
        }

        // POST: Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(string username, string password)
        {
            var usuario = _db.ObtenerUsuario(username, password);

            if (usuario == null)
            {
                ViewBag.Error = "Usuario o contraseña incorrectos";
                return View();
            }

            HttpContext.Session.SetString("Username", usuario.Username);
            HttpContext.Session.SetString("Rol", usuario.Rol);

            return RedirectToAction("Index", "Consolas");
        }

        // POST: Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Login");
        }
    }
}