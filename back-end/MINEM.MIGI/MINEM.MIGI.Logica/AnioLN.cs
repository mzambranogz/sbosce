using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MINEM.MIGI.Datos;
using MINEM.MIGI.Entidad;
using System.Data;

namespace MINEM.MIGI.Logica
{
    public class AnioLN : BaseLN
    {
        AnioDA AnioDA = new AnioDA();

        public bool GuardarAnio(AnioBE entidad)
        {
            bool seGuardo = false;

            try
            {
                cn.Open();
                seGuardo = AnioDA.GuardarAnio(entidad, cn);
            }
            finally { if (cn.State == ConnectionState.Open) cn.Close(); }

            return seGuardo;
        }

        public bool EliminarAnio(AnioBE entidad)
        {
            bool seGuardo = false;

            try
            {
                cn.Open();
                seGuardo = AnioDA.EliminarAnio(entidad, cn);
            }
            finally { if (cn.State == ConnectionState.Open) cn.Close(); }

            return seGuardo;
        }

        public AnioBE ObtenerAnio(int idanio)
        {
            AnioBE item = null;

            try
            {
                cn.Open();
                item = AnioDA.ObtenerAnio(idanio, cn);
            }
            finally { if (cn.State == ConnectionState.Open) cn.Close(); }

            return item;
        }

        public List<AnioBE> BuscarAnios(string busqueda, int registros, int pagina, string columna, string orden)
        {
            List<AnioBE> lista = new List<AnioBE>();

            try
            {
                cn.Open();
                lista = AnioDA.BuscarAnios(busqueda, registros, pagina, columna, orden, cn);
            }
            finally { if (cn.State == ConnectionState.Open) cn.Close(); }

            return lista;
        }

        public List<AnioBE> ListarAnio()
        {
            List<AnioBE> lista = new List<AnioBE>();
            try
            {
                cn.Open();
                lista = AnioDA.ListarAnio( cn);
            }
            finally { if (cn.State == ConnectionState.Open) cn.Close(); }

            return lista;
        }
    }
}
