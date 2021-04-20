using MINEM.MIGI.Entidad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MINEM.MIGI.Web.Controllers
{
    public class BaseController : Controller
    {
        // GET: Base
        protected UsuarioBE ObtenerSesion()
        {
            return Session["user"] == null ? new UsuarioBE() : (UsuarioBE)Session["user"];
        }
    }
}