using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MINEM.MIGI.Entidad
{
    public class BusquedaBE : BaseBE
    {
        public int ID_TIPO_BUSQUEDA {get; set;}
        public List<PalabraClaveBE> LISTA_PALABRAS { get; set; }
        public List<PalabraClaveCantidadBE> LISTA_PALABRAS_CANTIDAD { get; set; }
        public List<AnioBE> LISTA_ANIOS { get; set; }
        public List<OrdenCompraBE> LISTA_ORDENCOMPRA { get; set; }
        public List<OrdenCompraM8UBE> LISTA_ORDENCOMPRAM8U { get; set; }
        public List<GraficoBE> LISTA_GRAFICO { get; set; }
        public List<GraficoBE> LISTA_GRAFICON3 { get; set; }
        public List<GraficoN1BE> TABLA_BIENES { get; set; }
        public List<GraficoN1BE> TABLA_SERVICIOS { get; set; }
        public List<GraficoN1BE> TABLA_RESUMEN { get; set; }
        public List<GraficoN1BE> TABLA_ESTIMADO { get; set; }
        public int[] ARR_ANIOS { get; set; }
    }
}
