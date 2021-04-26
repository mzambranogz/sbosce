using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MINEM.MIGI.Entidad
{
    public class UsuarioBE : BaseBE
    {
        public int ID_USUARIO { get; set; }
        public string CORREO { get; set; }
        public string NOMBRES { get; set; }
        public string APELLIDOS { get; set; }
        public string CONTRASENA { get; set; }
        public string CONTRASENA_NUEVO { get; set; }
        public int ID_ROL { get; set; }
        public string FLAG_ESTADO { get; set; }
    }
}
