using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MINEM.MIGI.Entidad
{
    public class OrdenCompraBE
    {
        public string ANIO { get; set; }
        public string MES { get; set; }
        public string ENTIDAD { get; set; }
        public string RUC_ENTIDAD { get; set; }
        public string FECHA_REGISTRO { get; set; }
        public string FECHA_DE_EMISION { get; set; }
        public string FECHA_COMPROMISO_PRESUPUESTAL { get; set; }
        public string FECHA_DE_NOTIFICACION { get; set; }
        public string TIPOORDEN { get; set; }
        public string NRO_DE_ORDEN { get; set; }
        public string ORDEN { get; set; }
        public string DESCRIPCION_ORDEN { get; set; }
        public string MONEDA { get; set; }
        public string MONTO_TOTAL_ORDEN_ORIGINAL { get; set; }
        public string OBJETOCONTRACTUAL { get; set; }
        public string ESTADOCONTRATACION { get; set; }
        public string TIPODECONTRATACION { get; set; }
        public string DEPARTAMENTO__ENTIDAD { get; set; }
        public string RUC_CONTRATISTA { get; set; }
        public string NOMBRE_RAZON_CONTRATISTA { get; set; }
    }
}
