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
    public class EquipoLN : BaseLN
    {
        EquipoDA EquipoDA = new EquipoDA();

        public bool GuardarEquipo(EquipoBE entidad)
        {
            bool seGuardo = false;

            try
            {
                cn.Open();
                seGuardo = EquipoDA.GuardarEquipo(entidad, cn);
            }
            finally { if (cn.State == ConnectionState.Open) cn.Close(); }

            return seGuardo;
        }

        public bool EliminarEquipo(EquipoBE entidad)
        {
            bool seGuardo = false;

            try
            {
                cn.Open();
                seGuardo = EquipoDA.EliminarEquipo(entidad, cn);
            }
            finally { if (cn.State == ConnectionState.Open) cn.Close(); }

            return seGuardo;
        }

        public EquipoBE ObtenerEquipo(int idequipo)
        {
            EquipoBE item = null;

            try
            {
                cn.Open();
                item = EquipoDA.ObtenerEquipo(idequipo, cn);
            }
            finally { if (cn.State == ConnectionState.Open) cn.Close(); }

            return item;
        }

        public List<EquipoBE> BuscarEquipos(string busqueda, int registros, int pagina, string columna, string orden)
        {
            List<EquipoBE> lista = new List<EquipoBE>();

            try
            {
                cn.Open();
                lista = EquipoDA.BuscarEquipos(busqueda, registros, pagina, columna, orden, cn);
            }
            finally { if (cn.State == ConnectionState.Open) cn.Close(); }

            return lista;
        }

        public List<EquipoBE> ListarEquipo()
        {
            List<EquipoBE> lista = new List<EquipoBE>();
            try
            {
                cn.Open();
                lista = EquipoDA.ListarEquipo(cn);
            }
            finally { if (cn.State == ConnectionState.Open) cn.Close(); }

            return lista;
        }
    }
}
