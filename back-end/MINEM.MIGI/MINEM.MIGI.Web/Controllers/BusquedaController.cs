using MINEM.MIGI.Entidad;
using MINEM.MIGI.Logica;
using MINEM.MIGI.Web.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MINEM.MIGI.Web.Controllers
{
    [Autenticado]
    public class BusquedaController : BaseController
    {
        // GET: Busqueda
        AnioLN AnioLN = new AnioLN();
        EquipoLN EquipoLN = new EquipoLN();
        PalabraClaveLN PalabraClaveLN = new PalabraClaveLN();
        PalabraClaveCantidadLN PalabraClaveCantidadLN = new PalabraClaveCantidadLN();
        BusquedaLN BusquedaLN = new BusquedaLN();
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult ListarAnios()
        {
            List<AnioBE> lista = AnioLN.ListarAnio();
            var jsonResult = Json(lista, JsonRequestBehavior.AllowGet);
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

        public JsonResult ListarPalabrasClaves()
        {
            List<PalabraClaveBE> lista = PalabraClaveLN.ListarPalabraClave();
            var jsonResult = Json(lista, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public JsonResult ListarPalabrasClavesCantidad()
        {
            List<PalabraClaveCantidadBE> lista = PalabraClaveCantidadLN.ListarPalabraClaveCantidad();
            var jsonResult = Json(lista, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public JsonResult FiltrarInformacion(BusquedaBE obj)
        {
            Session["lista_palabras"] = obj.LISTA_PALABRAS;
            Session["lista_palabras_cantidad"] = obj.LISTA_PALABRAS_CANTIDAD;
            Session["lista_anios"] = obj.LISTA_ANIOS;
            Session["tipo_busqueda"] = obj.ID_TIPO_BUSQUEDA;
            BusquedaBE lista = obj.ID_TIPO_BUSQUEDA == 1 ? BusquedaLN.FiltrarInformacion(obj) : BusquedaLN.FiltrarInformacionM8U(obj);
            var jsonResult = Json(lista, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public JsonResult GuardarResultado() {
            BusquedaBE obj = new BusquedaBE();
            obj.LISTA_PALABRAS = (List<PalabraClaveBE>)Session["lista_palabras"];
            obj.LISTA_PALABRAS_CANTIDAD = Session["lista_palabras_cantidad"] == null ? new List<PalabraClaveCantidadBE>() : (List<PalabraClaveCantidadBE>)Session["lista_palabras_cantidad"];
            obj.LISTA_ANIOS = (List<AnioBE>)Session["lista_anios"];
            obj.ID_TIPO_BUSQUEDA = (int)Session["tipo_busqueda"];
            obj.UPD_USUARIO = ObtenerSesion().ID_USUARIO;
            bool v = BusquedaLN.GuardarResultado(obj);
            var jsonResult = Json(v, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

    }
}