using Oracle.DataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MINEM.MIGI.Logica
{
    public class BaseLN
    {
        static string nameConnection = ConfigurationManager.AppSettings["NombreConexion"];
        static string cadenaConexion = ConfigurationManager.ConnectionStrings[nameConnection].ConnectionString;

        protected OracleConnection cn = new OracleConnection(cadenaConexion);
    }
}
