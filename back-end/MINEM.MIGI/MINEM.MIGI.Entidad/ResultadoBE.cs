using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MINEM.MIGI.Entidad
{
    public class ResultadoBE : BaseBE
    {
        public int ID_RESULTADO { get; set; }
        public int ID_TIPO_BUSQUEDA { get; set; }
        public string FLAG_ESTADO { get; set; }

        public string FECHA_REGISTRO
        {
            get
            {
                string fecha = REG_FECHA.ToString("dd/MM/yyyy");
                return fecha;
            }
        }
    }
}
