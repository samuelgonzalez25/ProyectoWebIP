using Microsoft.AspNetCore.Mvc;
using APP.Data;
using APP.Models;
using APP.Filters;
using System.Collections.Generic;
using System.Linq;

namespace APP.Controllers
{
    [AuthorizeSession("ADMIN", "ENCARGADO", "EMPLEADO")] // Control de roles
    public class ConsolasController : Controller
    {
        private readonly ConexionMySql _db;

        public ConsolasController(ConexionMySql db)
        {
            _db = db;
        }

        private List<Proveedor> ObtenerProveedores()
        {
            return _db.ObtenerConsolas()
                      .Select(c => c.Proveedor)
                      .Where(p => p != null)
                      .GroupBy(p => p.IdProveedor)
                      .Select(g => g.First())
                      .ToList();
        }

        // --- LECTURA (todos los roles pueden)
        public IActionResult Index()
        {
            var lista = _db.ObtenerConsolas() ?? new List<Consola>();
            if (lista.Count == 0)
                ViewBag.Error = "No se encontraron consolas.";

            return View(lista);
        }

        [HttpPost]
        public IActionResult Find(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return RedirectToAction("Index");

            var lista = _db.ObtenerConsolas();
            var resultados = lista
                .Where(consola => !string.IsNullOrEmpty(consola.Modelo) &&
                                  consola.Modelo.ToLower().Contains(searchTerm.ToLower()))
                .ToList();

            if (resultados.Count == 0)
                ViewBag.Error = $"No se encontraron consolas con '{searchTerm}'.";

            return View("Index", resultados);
        }

        public IActionResult Details(int id)
        {
            var consola = _db.ObtenerConsolas().FirstOrDefault(c => c.IdConsola == id);
            if (consola == null) return NotFound();
            return View(consola);
        }

        // --- CREAR (solo ADMIN y ENCARGADO)
        [AuthorizeSession("ADMIN", "ENCARGADO")]
        public IActionResult Create()
        {
            ViewBag.Proveedores = ObtenerProveedores();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeSession("ADMIN", "ENCARGADO")]
        public IActionResult Create(
            Consola consola,
            string nuevoProveedorNombre,
            string nuevoProveedorDireccion,
            string nuevoProveedorTelefono,
            string nuevoProveedorEmail)
        {
            // Detectar si hay proveedor nuevo
            bool creandoNuevoProveedor = !string.IsNullOrEmpty(nuevoProveedorNombre);

            // Validación manual
            if (string.IsNullOrEmpty(consola.Marca)
                || string.IsNullOrEmpty(consola.Modelo)
                || string.IsNullOrEmpty(consola.Almacenamiento)
                || string.IsNullOrEmpty(consola.Generacion)
                || consola.Precio <= 0
                || (!creandoNuevoProveedor && consola.ProveedoresIdProveedor <= 0))
            {
                ViewBag.Proveedores = ObtenerProveedores();
                ViewBag.Error = "Por favor complete todos los campos requeridos";
                return View(consola);
            }

            // Crear proveedor nuevo si aplica
            Proveedor nuevoProveedor = null;
            if (creandoNuevoProveedor)
            {
                nuevoProveedor = new Proveedor
                {
                    Nombre = nuevoProveedorNombre,
                    Direccion = nuevoProveedorDireccion,
                    Telefono = nuevoProveedorTelefono,
                    Email = nuevoProveedorEmail
                };
            }

            bool resultado = _db.InsertarConsola(consola, nuevoProveedor);

            if (resultado)
                return RedirectToAction("Index");

            ViewBag.Error = "No se pudo insertar la consola.";
            ViewBag.Proveedores = ObtenerProveedores();
            return View(consola);
        }

        // --- EDITAR (solo ADMIN y ENCARGADO)
        [AuthorizeSession("ADMIN", "ENCARGADO")]
        public IActionResult Edit(int id)
        {
            var consola = _db.ObtenerConsolas().FirstOrDefault(c => c.IdConsola == id);
            if (consola == null)
                return NotFound();

            ViewBag.Proveedores = ObtenerProveedores();
            return View(consola);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeSession("ADMIN", "ENCARGADO")]
        public IActionResult Edit(
            Consola consola,
            string nuevoProveedorNombre,
            string nuevoProveedorDireccion,
            string nuevoProveedorTelefono,
            string nuevoProveedorEmail)
        {
            bool creandoNuevoProveedor = !string.IsNullOrEmpty(nuevoProveedorNombre);

            if (string.IsNullOrEmpty(consola.Marca)
                || string.IsNullOrEmpty(consola.Modelo)
                || string.IsNullOrEmpty(consola.Almacenamiento)
                || string.IsNullOrEmpty(consola.Generacion)
                || consola.Precio <= 0
                || (!creandoNuevoProveedor && consola.ProveedoresIdProveedor <= 0))
            {
                ViewBag.Proveedores = ObtenerProveedores();
                ViewBag.Error = "Por favor complete todos los campos requeridos";
                return View(consola);
            }

            Proveedor nuevoProveedor = null;
            if (creandoNuevoProveedor)
            {
                nuevoProveedor = new Proveedor
                {
                    Nombre = nuevoProveedorNombre,
                    Direccion = nuevoProveedorDireccion,
                    Telefono = nuevoProveedorTelefono,
                    Email = nuevoProveedorEmail
                };
            }

            bool actualizado = _db.EditarConsola(consola, nuevoProveedor);

            if (actualizado)
                return RedirectToAction("Details", new { id = consola.IdConsola });

            ViewBag.Proveedores = ObtenerProveedores();
            ViewBag.Error = "No se pudo actualizar la consola.";
            return View(consola);
        }

        // --- ELIMINAR (solo ADMIN)
        [AuthorizeSession("ADMIN")]
        public IActionResult Delete(int id)
        {
            _db.EliminarConsola(id);
            return RedirectToAction("Index");
        }
    }
}
