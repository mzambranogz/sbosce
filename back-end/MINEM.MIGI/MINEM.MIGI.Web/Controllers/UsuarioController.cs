using MINEM.MIGI.Entidad;
using MINEM.MIGI.Logica;
using MINEM.MIGI.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MINEM.MIGI.Web.Controllers
{
    public class UsuarioController : Controller
    {
        // GET: Usuario
        UsuarioLN UsuarioLN = new UsuarioLN();
        public ActionResult Registrar()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Registrar(UsuarioViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Registrar", model);
            }

            bool seGuardo = UsuarioLN.GuardarUsuario(
                new UsuarioBE() {
                    ID_USUARIO = -1,
                    CORREO = model.CORREO,
                    NOMBRES = model.NOMBRES,
                    APELLIDOS = model.APELLIDOS,
                    CONTRASENA = model.CONTRASENA,
                    ID_ROL = 1,
                    FLAG_ESTADO = "1"
                });

            if (seGuardo)
                return RedirectToAction("Index","Inicio");
            else
            {
                ModelState.AddModelError("", "Hubo un problema en la grabación de los datos, intentelo nuevamente");
                return View();
            }
        }
    }
}