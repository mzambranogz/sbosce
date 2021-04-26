using MINEM.MIGI.Datos;
using MINEM.MIGI.Entidad;
using Oracle.DataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MINEM.MIGI.Logica
{
    public class UsuarioLN : BaseLN
    {
        UsuarioDA usuarioDA = new UsuarioDA();

        public List<UsuarioBE> BuscarUsuarios(string busqueda, int registros, int pagina, string columna, string orden)
        {
            List<UsuarioBE> lista = new List<UsuarioBE>();

            try
            {
                cn.Open();
                lista = usuarioDA.BuscarUsuarios(busqueda, registros, pagina, columna, orden, cn);
            }
            finally { if (cn.State == ConnectionState.Open) cn.Close(); }

            return lista;
        }

        public UsuarioBE ObtenerUsuario(int idUsuario)
        {
            UsuarioBE item = null;

            try
            {
                cn.Open();
                item = usuarioDA.ObtenerUsuario(idUsuario, cn);
            }
            finally { if (cn.State == ConnectionState.Open) cn.Close(); }

            return item;
        }

        public bool GuardarUsuario(UsuarioBE objUsuario)
        {
            bool seGuardo = false;

            try
            {
                cn.Open();
                using (OracleTransaction ot = cn.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
                {
                    seGuardo = usuarioDA.GuardarUsuario(objUsuario, cn);

                    if (seGuardo) ot.Commit();
                    else ot.Rollback();
                }
            }
            finally { if (cn.State == ConnectionState.Open) cn.Close(); }

            return seGuardo;
        }

        public bool VerificarCredenciales(UsuarioBE objUsuario, out UsuarioBE objUsuarioTemp)
        {
            bool esValido = false;
            objUsuarioTemp = null;
            try
            {
                cn.Open();
                objUsuarioTemp = usuarioDA.GetUsuarioByCorreo(objUsuario.CORREO, cn);
                esValido = objUsuarioTemp != null;
                if (esValido) esValido = objUsuario.CONTRASENA.Equals(objUsuarioTemp.CONTRASENA);
            }
            finally { if (cn.State == ConnectionState.Open) cn.Close(); }

            return esValido;
        }

        public bool VerificarCorreo(string correo)
        {
            bool valor = false;

            try
            {
                cn.Open();
                valor = usuarioDA.VerificarCorreo(correo, cn);
            }
            finally { if (cn.State == ConnectionState.Open) cn.Close(); }

            return valor;
        }

        public UsuarioBE ObtenerUsuarioPorCorreo(string correo)
        {
            UsuarioBE item = null;

            try
            {
                cn.Open();
                item = usuarioDA.GetUsuarioByCorreo(correo, cn);
            }
            finally { if (cn.State == ConnectionState.Open) cn.Close(); }

            return item;
        }

        public int NuevaContrasena(UsuarioBE usuario)
        {
            int estado = 0;
            bool cambio = false;
            try
            {
                cn.Open();
                if (estado == 0)
                {
                    cambio = usuarioDA.CambiarContrasena(usuario.ID_USUARIO, usuario.CONTRASENA_NUEVO, cn);
                    estado = cambio ? 2 : 1;
                }
            }
            finally { if (cn.State == ConnectionState.Open) cn.Close(); }
            return estado;
        }
    }
}
