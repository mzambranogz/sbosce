using MINEM.MIGI.Entidad;
using MINEM.MIGI.Logica;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MINEM.MIGI.Web.Controllers
{
    public class MantenimientoController : Controller
    {
        // GET: Mantenimiento
        UsuarioLN UsuarioLN = new UsuarioLN();
        AnioLN AnioLN = new AnioLN();
        EquipoLN EquipoLN = new EquipoLN();
        PalabraClaveLN PalabraClaveLN = new PalabraClaveLN();
        PalabraClaveCantidadLN PalabraClaveCantidadLN = new PalabraClaveCantidadLN();
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Usuario()
        {
            return View();
        }

        public ActionResult Equipo()
        {
            return View();
        }

        public ActionResult PalabraClave()
        {
            return View();
        }

        public ActionResult PalabraClaveCantidad()
        {
            return View();
        }

        public ActionResult Anio()
        {
            return View();
        }

        /* MANTENIMIENTO USUARIO */
        public JsonResult BuscarUsuarios(string busqueda, int registros, int pagina, string columna, string orden)
        {
            string message = string.Empty;
            List<UsuarioBE> lista = UsuarioLN.BuscarUsuarios(busqueda, registros, pagina, columna, orden);

            var jsonResult = Json(new { list = lista }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public JsonResult ObtenerUsuario(int idusuario)
        {
            string message = string.Empty;
            UsuarioBE entidad = UsuarioLN.ObtenerUsuario(idusuario);

            var jsonResult = Json(new { obj = entidad }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public JsonResult GuardarUsuario(UsuarioBE objUsuario)
        {
            string message = string.Empty;
            bool seGuardo = UsuarioLN.GuardarUsuario(objUsuario);

            if (seGuardo)
                message = "Se guardó correctamente";
            else
                message = "Hubo un problema en la grabación de los datos, intentelo nuevamente";

            var jsonResult = Json(new { success = seGuardo, message = message }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public JsonResult VerificarCorreo(string correo)
        {
            string message = string.Empty;
            bool existe = UsuarioLN.VerificarCorreo(correo);

            var jsonResult = Json(new { success = existe }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        /* MANTENIMIENTO EQUIPOS */
        public JsonResult BuscarEquipos(string busqueda, int registros, int pagina, string columna, string orden)
        {
            string message = string.Empty;
            List<EquipoBE> lista = EquipoLN.BuscarEquipos(busqueda, registros, pagina, columna, orden);

            var jsonResult = Json(new { list = lista }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public JsonResult EliminarEquipo(EquipoBE obj)
        {
            string message = string.Empty;
            bool seElimino = EquipoLN.EliminarEquipo(obj);

            var jsonResult = Json(new { success = seElimino }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public JsonResult ObtenerEquipo(int idequipo)
        {
            string message = string.Empty;
            EquipoBE entidad = EquipoLN.ObtenerEquipo(idequipo);

            var jsonResult = Json(new { obj = entidad }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public JsonResult GuardarEquipo(EquipoBE objAnio)
        {
            string message = string.Empty;
            bool seGuardo = EquipoLN.GuardarEquipo(objAnio);

            if (seGuardo)
                message = "Se guardó correctamente";
            else
                message = "Hubo un problema en la grabación de los datos, intentelo nuevamente";

            var jsonResult = Json(new { success = seGuardo, message = message }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        /* MANTENIMIENTO PALABRAS CLAVES */
        public JsonResult BuscarPalabrasClaves(string busqueda, int registros, int pagina, string columna, string orden)
        {
            string message = string.Empty;
            List<PalabraClaveBE> lista = PalabraClaveLN.BuscarPalabraClaves(busqueda, registros, pagina, columna, orden);

            var jsonResult = Json(new { list = lista }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public JsonResult EliminarPalabraClave(PalabraClaveBE obj)
        {
            string message = string.Empty;
            bool seElimino = PalabraClaveLN.EliminarPalabraClave(obj);

            var jsonResult = Json(new { success = seElimino }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public JsonResult ObtenerPalabraClave(int idpalabra)
        {
            string message = string.Empty;
            PalabraClaveBE entidad = PalabraClaveLN.ObtenerPalabraClave(idpalabra);

            var jsonResult = Json(new { obj = entidad }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public JsonResult GuardarPalabraClave(PalabraClaveBE obj)
        {
            string message = string.Empty;
            bool seGuardo = PalabraClaveLN.GuardarPalabraClave(obj);

            if (seGuardo)
                message = "Se guardó correctamente";
            else
                message = "Hubo un problema en la grabación de los datos, intentelo nuevamente";

            var jsonResult = Json(new { success = seGuardo, message = message }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        /* MANTENIMIENTO PALABRAS CLAVES CANTIDAD */
        public JsonResult BuscarPalabrasClavesCantidad(string busqueda, int registros, int pagina, string columna, string orden)
        {
            string message = string.Empty;
            List<PalabraClaveCantidadBE> lista = PalabraClaveCantidadLN.BuscarPalabraClavesCantidad(busqueda, registros, pagina, columna, orden);

            var jsonResult = Json(new { list = lista }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public JsonResult EliminarPalabraClaveCantidad(PalabraClaveCantidadBE obj)
        {
            string message = string.Empty;
            bool seElimino = PalabraClaveCantidadLN.EliminarPalabraClaveCantidad(obj);

            var jsonResult = Json(new { success = seElimino }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public JsonResult ObtenerPalabraClaveCantidad(int idpalabra)
        {
            string message = string.Empty;
            PalabraClaveCantidadBE entidad = PalabraClaveCantidadLN.ObtenerPalabraClaveCantidad(idpalabra);

            var jsonResult = Json(new { obj = entidad }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public JsonResult GuardarPalabraClaveCantidad(PalabraClaveCantidadBE obj)
        {
            string message = string.Empty;
            bool seGuardo = PalabraClaveCantidadLN.GuardarPalabraClaveCantidad(obj);

            if (seGuardo)
                message = "Se guardó correctamente";
            else
                message = "Hubo un problema en la grabación de los datos, intentelo nuevamente";

            var jsonResult = Json(new { success = seGuardo, message = message }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        /* MANTENIMIENTO AÑOS */
        public JsonResult BuscarAnios(string busqueda, int registros, int pagina, string columna, string orden)
        {
            string message = string.Empty;
            List<AnioBE> lista = AnioLN.BuscarAnios(busqueda, registros, pagina, columna, orden);

            var jsonResult = Json(new { list = lista }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public JsonResult EliminarAnio(AnioBE obj)
        {
            string message = string.Empty;
            bool seElimino = AnioLN.EliminarAnio(obj);

            var jsonResult = Json(new { success = seElimino }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public JsonResult ObtenerAnio(int idanio)
        {
            string message = string.Empty;
            AnioBE entidad = AnioLN.ObtenerAnio(idanio);

            var jsonResult = Json(new { obj = entidad }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public JsonResult GuardarAnio(AnioBE objAnio)
        {
            string message = string.Empty;
            bool seGuardo = AnioLN.GuardarAnio(objAnio);

            if (seGuardo)
                message = "Se guardó correctamente";
            else
                message = "Hubo un problema en la grabación de los datos, intentelo nuevamente";

            var jsonResult = Json(new { success = seGuardo, message = message }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public JsonResult ListarEquipos()
        {
            List<EquipoBE> lista = EquipoLN.ListarEquipo();
            var jsonResult = Json(lista, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
    }
}