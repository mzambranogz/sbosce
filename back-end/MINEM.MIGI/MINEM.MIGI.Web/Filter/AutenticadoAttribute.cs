using MINEM.MIGI.Entidad;
using MINEM.MIGI.Util;
using MINEM.MIGI.Web.Controllers;
using MINEM.MIGI.Web.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace MINEM.MIGI.Web.Filter
{
    public class AutenticadoAttribute : ActionFilterAttribute
    {
        // Si no estamos logueado, regresamos al login
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //try
            //{
            //    base.OnActionExecuting(filterContext);
            //    HttpSessionStateBase session = filterContext.HttpContext.Session;
            //    if (!SessionHelper.ExistUserInSession())
            //    {
            //        filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
            //        {
            //            controller = "Inicio",
            //            action = "Index"
            //        }));
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Log.Error(ex);
            //}

            try
            {
                base.OnActionExecuting(filterContext);

                UsuarioBE usuario = (UsuarioBE)HttpContext.Current.Session["user"];

                if (usuario == null)
                {
                    if (filterContext.Controller is InicioController == false)
                    {
                        //filterContext.HttpContext.Response.Redirect("~/Security/Login");
                        filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                        {
                            controller = "Inicio",
                            action = "Index"
                        }));
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }

    // No acceder al Login cuando ya se inicio sesion
    public class NoLoginAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            try
            {
                base.OnActionExecuting(filterContext);
                //HttpSessionStateBase session = filterContext.HttpContext.Session;
                UsuarioBE usuario = (UsuarioBE)HttpContext.Current.Session["user"];
                if (usuario != null)
                {
                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                    {
                        controller = "Busqueda",
                        action = "Index"
                    }));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }
    }
}