using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace APP.Filters
{
    public class AuthorizeSessionAttribute : ActionFilterAttribute
    {
        private readonly string[] _rolesPermitidos;

        public AuthorizeSessionAttribute(params string[] rolesPermitidos)
        {
            _rolesPermitidos = rolesPermitidos;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Verificar si hay sesión activa
            var username = context.HttpContext.Session.GetString("Username");
            var rol = context.HttpContext.Session.GetString("Rol");

            // Debug: Verificar valores de sesión
            Console.WriteLine($"🔐 Filter - Username: {username}, Rol: {rol}");
            Console.WriteLine($"🔐 Roles permitidos: {string.Join(", ", _rolesPermitidos)}");

            // No hay sesión activa
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(rol))
            {
                Console.WriteLine("❌ Sesión inválida o expirada");
                context.Result = new RedirectToActionResult("Index", "Login", null);
                return;
            }

            // Si se especificaron roles, verificar permisos
            if (_rolesPermitidos.Length > 0 && !_rolesPermitidos.Contains(rol))
            {
                Console.WriteLine($"❌ Acceso denegado. Rol actual: {rol}, Roles requeridos: {string.Join(", ", _rolesPermitidos)}");
                context.Result = new RedirectToActionResult("AccesoDenegado", "Home", null);
                return;
            }

            Console.WriteLine("✅ Acceso autorizado");
        }
    }
}