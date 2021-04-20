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
    public class ResultadoLN : BaseLN
    {
        ResultadoDA ResultadoDA = new ResultadoDA();

        public List<ResultadoBE> ListaResultado()
        {
            List<ResultadoBE> lista = new List<ResultadoBE>();
            try
            {
                cn.Open();
                lista = ResultadoDA.ListaResultado(cn);
            }
            finally
            {
                if (cn.State == ConnectionState.Open) cn.Close();
            }
            return lista;
        }

        public BusquedaBE ListaBusqueda(int idresultado) {
            BusquedaBE obj = new BusquedaBE();
            try
            {
                ResultadoBE resultado = ResultadoDA.ObtenerResultado(idresultado, cn);
                obj.ID_TIPO_BUSQUEDA = resultado.ID_TIPO_BUSQUEDA;
                obj.LISTA_PALABRAS = ResultadoDA.ObtenerPalabraClave(idresultado, cn);
                if (resultado.ID_TIPO_BUSQUEDA == 1) obj.LISTA_PALABRAS_CANTIDAD = ResultadoDA.ObtenerPalabraClaveCantidad(idresultado, cn);
                obj.LISTA_ANIOS = ResultadoDA.ObtenerAnio(idresultado, cn);
            }
            finally
            {
                if (cn.State == ConnectionState.Open) cn.Close();
            }
            return obj;
        }
    }
}
