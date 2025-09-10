using Microsoft.AspNetCore.Mvc;
using APP.Data;
using APP.Models;
using APP.Filters;
using System;
using System.Collections.Generic;

namespace APP.Controllers
{
    [AuthorizeSession("ADMIN")]
    public class UserController : Controller
    {
        private readonly ConexionMySql _db;

        public UserController(ConexionMySql db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            List<Usuario> lista = new List<Usuario>();

            try
            {
                lista = _db.ObtenerUsuarios() ?? new List<Usuario>();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar los usuarios: " + ex.Message;
            }

            return View(lista);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Usuario usuario)
        {
            if (!ModelState.IsValid)
                return View(usuario);

            try
            {
                string mensajeError;
                bool exito = _db.RegistrarUsuario(
                    usuario.Username,
                    usuario.Password,
                    usuario.Nombre,
                    usuario.Apellido,
                    usuario.Sexo,
                    usuario.NivelAcademico,
                    usuario.Institucion,
                    usuario.Rol,
                    out mensajeError
                );

                if (!exito)
                {
                    ModelState.AddModelError("", mensajeError);
                    return View(usuario);
                }

                TempData["Success"] = "Usuario creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al crear usuario: " + ex.Message);
                return View(usuario);
            }
        }

        public IActionResult Edit(int id)
        {
            var usuario = _db.ObtenerUsuarioPorId(id);
            if (usuario == null)
            {
                TempData["Error"] = "Usuario no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Usuario usuario)
        {
            if (!ModelState.IsValid)
                return View(usuario);

            try
            {
                string mensajeError;
                bool exito = _db.ActualizarUsuario(usuario, out mensajeError);

                if (!exito)
                {
                    ModelState.AddModelError("", mensajeError);
                    return View(usuario);
                }

                TempData["Success"] = "Usuario actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al actualizar usuario: " + ex.Message);
                return View(usuario);
            }
        }

        public IActionResult Delete(int id)
        {
            try
            {
                bool exito = _db.EliminarUsuario(id);
                if (!exito)
                    TempData["Error"] = "No se pudo eliminar el usuario.";
                else
                    TempData["Success"] = "Usuario eliminado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar usuario: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Details(int id)
        {
            var usuario = _db.ObtenerUsuarioPorId(id);
            if (usuario == null)
            {
                TempData["Error"] = "Usuario no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            return View(usuario);
        }

        public IActionResult Main()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Search(string searchTerm)
        {
            List<Usuario> lista = _db.ObtenerUsuarios() ?? new List<Usuario>();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                lista = lista.FindAll(u =>
                    u.Nombre.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    u.Apellido.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    u.Username.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    u.Id.ToString() == searchTerm
                );
            }

            return View("Index", lista);
        }
    }
}