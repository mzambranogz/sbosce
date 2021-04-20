using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MINEM.MIGI.Entidad
{
    public class PalabraClaveBE : BaseBE
    {
        public int ID_PALABRA { get; set; }
        public string PALABRA { get; set; }
        public int ID_EQUIPO { get; set; }
        public string NOMBRE_EQUIPO { get; set; }
        public string FLAG_ESTADO { get; set; }
    }
}
