﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MINEM.MIGI.Entidad
{
    public class BaseBE
    {
        public string BUSCAR { get; set; }
        public int CANTIDAD_REGISTROS { get; set; }
        public int TOTAL_PAGINAS { get; set; }
        public int PAGINA { get; set; }
        public string ORDER_BY { get; set; }
        public string ORDER_ORDEN { get; set; }
        public int ROWNUMBER { get; set; }
        public int TOTAL_REGISTROS { get; set; }
        public bool OK { get; set; }
        public int REG_USUARIO { get; set; }
        public int UPD_USUARIO { get; set; }
        public DateTime REG_FECHA { get; set; }
        public int VAL { get; set; }
    }
}
