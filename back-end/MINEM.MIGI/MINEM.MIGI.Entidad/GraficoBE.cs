using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MINEM.MIGI.Entidad
{
    public class GraficoBE
    {
        public string HALLAZGO { get; set; }
        public List<GraficoN1BE> LISTA_GRAFICON1 { get; set; }
        public List<GraficoN3BE> LISTA_GRAFICON3 { get; set; }
        public List<GraficoM8UBE> LISTA_GRAFICOM8U { get; set; }
    }
}
