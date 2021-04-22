using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using MINEM.MIGI.Entidad;
using MINEM.MIGI.Logica;
using MINEM.MIGI.Web.Helper;
using MINEM.MIGI.Web.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MINEM.MIGI.Web.Controllers
{
    public class InicioController : Controller
    {
        // GET: Inicio
        UsuarioLN UsuarioLN = new UsuarioLN();
        public ActionResult Index()
        {
            string keySiteCaptcha = ConfigurationManager.AppSettings["ReCAPTCHA_Site_Key"];
            ViewData["keySiteCaptcha"] = keySiteCaptcha;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult InicioSesion(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, tipo = "", message = "" });
            }

            UsuarioBE objUsuario = null;
            bool v = UsuarioLN.VerificarCredenciales(new UsuarioBE { CORREO = model.CORREO, CONTRASENA = model.CONTRASENA }, out objUsuario);
            if (v)
            {
                Session["user"] = objUsuario;
                SessionHelper.AddUserToSession(objUsuario.ID_USUARIO.ToString());
                return Json(new { success = true, tipo = "Ok", message = "Busqueda/Index" });
            }
            else
                return Json(new { success = false, tipo = "Error", message = "Usuario y/o contraseña incorrectos" });

        }

        public ActionResult Salir()
        {
            Session["user"] = null;
            return RedirectToAction("Index", "Inicio");
        }
    }
}