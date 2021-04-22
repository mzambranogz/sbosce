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
    public class ResultadoController : Controller
    {
        // GET: Resultado
        ResultadoLN ResultadoLN = new ResultadoLN();
        BusquedaLN BusquedaLN = new BusquedaLN();
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult ListaResultado() {
            List<ResultadoBE> lista = ResultadoLN.ListaResultado();
            var jsonResult = Json(lista, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public ActionResult VerResultado(int id) {
            ViewData["idresultado"] = id;
            return View();
        }

        public JsonResult MostrarResultado(int id)
        {
            BusquedaBE obj = ResultadoLN.ListaBusqueda(id);
            Session["lista_palabras"] = obj.LISTA_PALABRAS;
            Session["lista_palabras_cantidad"] = obj.LISTA_PALABRAS_CANTIDAD;
            Session["lista_anios"] = obj.LISTA_ANIOS;
            Session["tipo_busqueda"] = obj.ID_TIPO_BUSQUEDA;
            BusquedaBE entidad = obj.ID_TIPO_BUSQUEDA == 1 ? BusquedaLN.FiltrarInformacion(obj) : BusquedaLN.FiltrarInformacionM8U(obj);
            entidad.ID_TIPO_BUSQUEDA = obj.ID_TIPO_BUSQUEDA;
            entidad.LISTA_ANIOS = obj.LISTA_ANIOS;
            var jsonResult = Json(entidad, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
    }
}