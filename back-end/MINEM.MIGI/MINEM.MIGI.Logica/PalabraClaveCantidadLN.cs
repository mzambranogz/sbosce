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
    public class PalabraClaveCantidadLN : BaseLN
    {
        PalabraClaveCantidadDA PalabraClaveDA = new PalabraClaveCantidadDA();

        public bool GuardarPalabraClaveCantidad(PalabraClaveCantidadBE entidad)
        {
            bool seGuardo = false;

            try
            {
                cn.Open();
                seGuardo = PalabraClaveDA.GuardarPalabraClaveCantidad(entidad, cn);
            }
            finally { if (cn.State == ConnectionState.Open) cn.Close(); }

            return seGuardo;
        }

        public bool EliminarPalabraClaveCantidad(PalabraClaveCantidadBE entidad)
        {
            bool seGuardo = false;

            try
            {
                cn.Open();
                seGuardo = PalabraClaveDA.EliminarPalabraClaveCantidad(entidad, cn);
            }
            finally { if (cn.State == ConnectionState.Open) cn.Close(); }

            return seGuardo;
        }

        public PalabraClaveCantidadBE ObtenerPalabraClaveCantidad(int id)
        {
            PalabraClaveCantidadBE item = null;

            try
            {
                cn.Open();
                item = PalabraClaveDA.ObtenerPalabraClaveCantidad(id, cn);
            }
            finally { if (cn.State == ConnectionState.Open) cn.Close(); }

            return item;
        }

        public List<PalabraClaveCantidadBE> BuscarPalabraClavesCantidad(string busqueda, int registros, int pagina, string columna, string orden)
        {
            List<PalabraClaveCantidadBE> lista = new List<PalabraClaveCantidadBE>();

            try
            {
                cn.Open();
                lista = PalabraClaveDA.BuscarPalabrasClavesCantidad(busqueda, registros, pagina, columna, orden, cn);
            }
            finally { if (cn.State == ConnectionState.Open) cn.Close(); }

            return lista;
        }
        public List<PalabraClaveCantidadBE> ListarPalabraClaveCantidad()
        {
            List<PalabraClaveCantidadBE> lista = new List<PalabraClaveCantidadBE>();
            try
            {
                cn.Open();
                lista = PalabraClaveDA.ListarPalabraClaveCantidad(cn);
            }
            finally { if (cn.State == ConnectionState.Open) cn.Close(); }

            return lista;
        }
    }
}
