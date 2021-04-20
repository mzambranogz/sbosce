using MINEM.MIGI.Entidad;
using Oracle.DataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using MINEM.MIGI.Util;

namespace MINEM.MIGI.Datos
{
    public class UsuarioDA : BaseDA
    {
        public List<UsuarioBE> BuscarUsuarios(string busqueda, int registros, int pagina, string columna, string orden, OracleConnection db)
        {
            List<UsuarioBE> lista = null;

            try
            {
                string sp = $"{Package.Mantenimiento}USP_SEL_LISTA_BUSQ_USUARIO";
                var p = new OracleDynamicParameters();
                p.Add("PI_BUSCAR", busqueda);
                p.Add("PI_REGISTROS", registros);
                p.Add("PI_PAGINA", pagina);
                p.Add("PI_COLUMNA", columna);
                p.Add("PI_ORDEN", orden);
                p.Add("PO_REF", dbType: OracleDbType.RefCursor, direction: ParameterDirection.Output);
                lista = db.Query<dynamic>(sp, p, commandType: CommandType.StoredProcedure)
                    .Select(x => new UsuarioBE
                    {
                        ID_USUARIO = (int)x.ID_USUARIO,
                        NOMBRES = (string)x.NOMBRES,
                        APELLIDOS = (string)x.APELLIDOS,
                        CORREO = (string)x.CORREO,
                        ID_ROL = (int)x.ID_ROL,
                        FLAG_ESTADO = (string)x.FLAG_ESTADO,
                        TOTAL_PAGINAS = (int)x.TOTAL_PAGINAS,
                        PAGINA = (int)x.PAGINA,
                        CANTIDAD_REGISTROS = (int)x.CANTIDAD_REGISTROS,
                        TOTAL_REGISTROS = (int)x.TOTAL_REGISTROS
                    })
                    .ToList();
            }
            catch (Exception ex) { Log.Error(ex); }

            return lista;
        }

        public UsuarioBE ObtenerUsuario(int idusuario, OracleConnection db)
        {
            UsuarioBE item = null;

            try
            {
                string sp = $"{Package.Mantenimiento}USP_SEL_USUARIO";
                var p = new OracleDynamicParameters();
                p.Add("PI_ID_USUARIO", idusuario);
                p.Add("PO_REF", dbType: OracleDbType.RefCursor, direction: ParameterDirection.Output);
                item = db.QueryFirstOrDefault<UsuarioBE>(sp, p, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex) { Log.Error(ex); }

            return item;
        }

        public bool GuardarUsuario(UsuarioBE entidad, OracleConnection db)
        {
            bool seGuardo = false;
            try
            {
                string sp = $"{Package.Admin}USP_PRC_GUARDAR_USUARIO";
                var p = new OracleDynamicParameters();
                p.Add("PI_ID_USUARIO", entidad.ID_USUARIO);
                p.Add("PI_CORREO", entidad.CORREO);
                p.Add("PI_NOMBRES", entidad.NOMBRES);
                p.Add("PI_APELLIDOS", entidad.APELLIDOS);
                p.Add("PI_CONTRASENA", entidad.CONTRASENA);
                p.Add("PI_ID_ROL", entidad.ID_ROL);
                p.Add("PI_FLAG_ESTADO", entidad.FLAG_ESTADO);
                p.Add("PI_UPD_USUARIO", entidad.UPD_USUARIO);
                p.Add("PO_ROWAFFECTED", dbType: OracleDbType.Int32, direction: ParameterDirection.Output);
                db.Execute(sp, p, commandType: CommandType.StoredProcedure);
                int filasAfectadas = (int)p.Get<dynamic>("PO_ROWAFFECTED").Value;
                seGuardo = filasAfectadas > 0;
            }
            catch (Exception ex) { Log.Error(ex); }

            return seGuardo;
        }

        public UsuarioBE GetUsuarioByCorreo(string correo, OracleConnection db)
        {
            UsuarioBE item = null;

            try
            {
                string sp = $"{Package.Admin}USP_SEL_USUARIO_CORREO";
                OracleDynamicParameters p = new OracleDynamicParameters();
                p.Add("PI_CORREO", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, value: correo);
                p.Add("PO_REFCURSOR", dbType: OracleDbType.RefCursor, direction: ParameterDirection.Output);
                item = db.QueryFirstOrDefault<UsuarioBE>(sp, p, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            return item;
        }

        public bool VerificarCorreo(string correo, OracleConnection db)
        {
            bool verificacion = false;
            try
            {
                string sp = $"{Package.Admin}USP_SEL_VERIFICAR_EMAIL";
                var p = new OracleDynamicParameters();
                p.Add("PI_EMAIL_USUARIO", correo);
                p.Add("PI_VERIFICAR", dbType: OracleDbType.Int32, direction: ParameterDirection.Output);
                db.Execute(sp, p, commandType: CommandType.StoredProcedure);
                int cantidad = (int)p.Get<dynamic>("PI_VERIFICAR").Value;
                verificacion = cantidad > 0;
            }
            catch (Exception ex) { Log.Error(ex); }

            return verificacion;
        }
    }
}
