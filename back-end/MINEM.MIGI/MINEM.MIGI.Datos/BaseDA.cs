using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MINEM.MIGI.Datos
{
    public class BaseDA
    {
        string user { get { return ConfigurationManager.AppSettings["userBD"]; } }

        protected dynamic Package
        {
            get
            {
                return new
                {
                    Admin = $"{user}.PKG_MIGI_ADMIN.",
                    Filtro = $"{user}.PKG_MIGI_FILTRO.",
                    Mantenimiento = $"{user}.PKG_MIGI_MANTENIMIENTO.",
                    Excel = $"{user}.PKG_MIGI_EXCEL.",
                    Resultado = $"{user}.PKG_MIGI_RESULTADO."
                };
            }
        }
    }
}
