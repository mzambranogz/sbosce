using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MINEM.MIGI.Entidad
{
    public class ExcelBE : BaseBE
    {
        public int ID_EXCEL { get; set; }
        public string NOMBRE { get; set; }
        public string ANIO { get; set; }
        public string MES { get; set; }
        public int ID_TIPO_EXCEL { get; set; }
        public string NOMBRE_MES {
            get {
                string mes = "";
                if (MES == "1") mes = "Enero";
                else if (MES == "2") mes = "Febrero";
                else if (MES == "3") mes = "Marzo";
                else if (MES == "4") mes = "Abril";
                else if (MES == "5") mes = "Mayo";
                else if (MES == "6") mes = "Junio";
                else if (MES == "7") mes = "Julio";
                else if (MES == "8") mes = "Agosto";
                else if (MES == "9") mes = "Setiembre";
                else if (MES == "10") mes = "Octubre";
                else if (MES == "11") mes = "Noviembre";
                else if (MES == "12") mes = "Diciembre";
                return mes;
            }
        }
        public string FLAG_ESTADO { get; set; }
    }
}
