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
    }
}
