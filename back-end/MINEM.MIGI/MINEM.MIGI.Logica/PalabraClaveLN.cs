using MINEM.MIGI.Datos;
using MINEM.MIGI.Entidad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MINEM.MIGI.Logica
{
    public class PalabraClaveLN : BaseLN
    {
        PalabraClaveDA PalabraClaveDA = new PalabraClaveDA();

        public bool GuardarPalabraClave(PalabraClaveBE entidad)
        {
            bool seGuardo = false;

            try
            {
                cn.Open();
                seGuardo = PalabraClaveDA.GuardarPalabraClave(entidad, cn);
            }
            finally { if (cn.State == ConnectionState.Open) cn.Close(); }

            return seGuardo;
        }

        public bool EliminarPalabraClave(PalabraClaveBE entidad)
        {
            bool seGuardo = false;

            try
            {
                cn.Open();
                seGuardo = PalabraClaveDA.EliminarPalabraClave(entidad, cn);
            }
            finally { if (cn.State == ConnectionState.Open) cn.Close(); }

            return seGuardo;
        }

        public PalabraClaveBE ObtenerPalabraClave(int idequipo)
        {
            PalabraClaveBE item = null;

            try
            {
                cn.Open();
                item = PalabraClaveDA.ObtenerPalabraClave(idequipo, cn);
            }
            finally { if (cn.State == ConnectionState.Open) cn.Close(); }

            return item;
        }

        public List<PalabraClaveBE> BuscarPalabraClaves(string busqueda, int registros, int pagina, string columna, string orden)
        {
            List<PalabraClaveBE> lista = new List<PalabraClaveBE>();

            try
            {
                cn.Open();
                lista = PalabraClaveDA.BuscarPalabrasClaves(busqueda, registros, pagina, columna, orden, cn);
            }
            finally { if (cn.State == ConnectionState.Open) cn.Close(); }

            return lista;
        }
        public List<PalabraClaveBE> ListarPalabraClave()
        {
            List<PalabraClaveBE> lista = new List<PalabraClaveBE>();
            try
            {
                cn.Open();
                lista = PalabraClaveDA.ListarPalabraClave(cn);
            }
            finally { if (cn.State == ConnectionState.Open) cn.Close(); }

            return lista;
        }
    }
}
