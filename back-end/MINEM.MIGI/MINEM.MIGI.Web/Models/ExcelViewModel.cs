using MINEM.MIGI.Entidad;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace MINEM.MIGI.Web.Models
{
    public class ExcelViewModel
    {
        [DisplayName("Excel")]
        public HttpPostedFileBase excel { get; set; }        
        public List<OrdenCompraBE> lstOrdenCompra { get; set; }
        [DisplayName("Mes")]
        public string mes { get; set; }
        [DisplayName("Año")]
        public string anio { get; set; }
    }
}