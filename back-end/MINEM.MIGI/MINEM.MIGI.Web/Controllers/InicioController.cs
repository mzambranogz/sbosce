using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using MINEM.MIGI.Entidad;
using MINEM.MIGI.Logica;
using MINEM.MIGI.Web.Filter;
using MINEM.MIGI.Web.Helper;
using MINEM.MIGI.Web.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using MINEM.MIGI.Util;

namespace MINEM.MIGI.Web.Controllers
{
    [HandleError]
    public class InicioController : Controller
    {
        // GET: Inicio
        UsuarioLN UsuarioLN = new UsuarioLN();
        Mailing mailing = new Mailing();

        [NoLogin]
        public ActionResult Index()
        {
            string keySiteCaptcha = ConfigurationManager.AppSettings["ReCAPTCHA_Site_Key"];
            ViewData["keySiteCaptcha"] = keySiteCaptcha;
            return View();
        }

        public ActionResult OlvidarContrasena() {
            return View();
        }

        [NoLogin]
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

        [Autenticado]
        public ActionResult Salir()
        {
            SessionHelper.DestroyUserSession();
            Session["user"] = null;
            return RedirectToAction("Index", "Inicio");
        }

        [NoLogin]
        public JsonResult RecuperarContrasena(string correo)
        {
            UsuarioBE usuario = UsuarioLN.ObtenerUsuarioPorCorreo(correo);

            if (usuario == null) {
                var jResult = Json(new { success = false, message = "Este correo no ha sido registrado" }, JsonRequestBehavior.AllowGet);
                jResult.MaxJsonLength = int.MaxValue;
                return jResult;
            }

            string fechaHoraExpiracion = DateTime.Now.AddMinutes(10).ToString("yyyy-MM-ddTHH:mm:ss.fffK");
            string idUsuario = usuario.ID_USUARIO.ToString();

            string fieldServer = "[SERVER]", fieldNombres = "[NOMBRES]", fieldIdUsuario = "[ID_USUARIO]";
            string[] fields = new string[] { fieldServer, fieldNombres, fieldIdUsuario };
            string[] fieldsRequire = new string[] { fieldServer, fieldNombres, fieldIdUsuario };
            Dictionary<string, string> dataBody = new Dictionary<string, string> { [fieldServer] = ConfigurationManager.AppSettings["Server"], [fieldNombres] = usuario.NOMBRES, [fieldIdUsuario] = usuario.ID_USUARIO.ToString() };
            string subject = $"{usuario.NOMBRES} {usuario.APELLIDOS}, recupere su contraseña";
            MailAddressCollection mailTo = new MailAddressCollection();
            mailTo.Add(new MailAddress(usuario.CORREO, $"{usuario.NOMBRES} {usuario.APELLIDOS}"));

            Task.Factory.StartNew(() => mailing.SendMail(Mailing.Templates.RecuperarClave, dataBody, fields, fieldsRequire, subject, mailTo));

            Session["recuperar"] = true;
            var jsonResult = Json(new { success = true, message = $"se envió link de recuperación de contraseña al correo {correo}" }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        [NoLogin]
        public ActionResult CambiarContrasena(int id)
        {
            bool validar = Session["recuperar"] == null ? false : (bool)Session["recuperar"];
            if (validar)
            {
                ViewData["idusuario"] = id;
                return View();
            }
            else
            {
                return RedirectToAction("Index");
            }            
        }

        [NoLogin]
        public ActionResult NuevaContrasena(UsuarioBE usuario)
        {
            int estado = UsuarioLN.NuevaContrasena(usuario);
            Session["recuperar"] = null;
            var jsonResult = Json(new { Estado = estado }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
    }
}