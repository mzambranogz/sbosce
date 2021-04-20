using Dapper;
using MINEM.MIGI.Entidad;
using MINEM.MIGI.Util;
using Oracle.DataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MINEM.MIGI.Datos
{
    public class PalabraClaveDA : BaseDA
    {
        public bool GuardarPalabraClave(PalabraClaveBE entidad, OracleConnection db)
        {
            bool seGuardo = false;
            try
            {
                string sp = $"{Package.Mantenimiento}USP_PRC_GUARDAR_PALABRA";
                var p = new OracleDynamicParameters();
                p.Add("PI_ID_PALABRA", entidad.ID_PALABRA);
                p.Add("PI_PALABRA", entidad.PALABRA);
                p.Add("PI_ID_EQUIPO", entidad.ID_EQUIPO);
                p.Add("PI_UPD_USUARIO", entidad.UPD_USUARIO);
                p.Add("PO_ROWAFFECTED", dbType: OracleDbType.Int32, direction: ParameterDirection.Output);
                db.Execute(sp, p, commandType: CommandType.StoredProcedure);
                int filasAfectadas = (int)p.Get<dynamic>("PO_ROWAFFECTED").Value;
                seGuardo = filasAfectadas > 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            return seGuardo;
        }

        public bool EliminarPalabraClave(PalabraClaveBE entidad, OracleConnection db)
        {
            bool seGuardo = false;
            try
            {
                string sp = $"{Package.Mantenimiento}USP_DEL_PALABRA";
                var p = new OracleDynamicParameters();
                p.Add("PI_ID_PALABRA", entidad.ID_PALABRA);
                p.Add("PI_UPD_USUARIO", entidad.UPD_USUARIO);
                p.Add("PO_ROWAFFECTED", dbType: OracleDbType.Int32, direction: ParameterDirection.Output);
                db.Execute(sp, p, commandType: CommandType.StoredProcedure);
                int filasAfectadas = (int)p.Get<dynamic>("PO_ROWAFFECTED").Value;
                seGuardo = filasAfectadas > 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            return seGuardo;
        }

        public PalabraClaveBE ObtenerPalabraClave(int id, OracleConnection db)
        {
            PalabraClaveBE item = null;

            try
            {
                string sp = $"{Package.Mantenimiento}USP_SEL_PALABRA";
                var p = new OracleDynamicParameters();
                p.Add("PI_ID_PALABRA", id);
                p.Add("PO_REF", dbType: OracleDbType.RefCursor, direction: ParameterDirection.Output);
                item = db.Query<PalabraClaveBE>(sp, p, commandType: CommandType.StoredProcedure).FirstOrDefault();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            return item;
        }

        public List<PalabraClaveBE> BuscarPalabrasClaves(string busqueda, int registros, int pagina, string columna, string orden, OracleConnection db)
        {
            List<PalabraClaveBE> lista = new List<PalabraClaveBE>();

            try
            {
                string sp = $"{Package.Mantenimiento}USP_SEL_LISTA_BUSQ_PALABRA";
                var p = new OracleDynamicParameters();
                p.Add("PI_BUSCAR", busqueda);
                p.Add("PI_REGISTROS", registros);
                p.Add("PI_PAGINA", pagina);
                p.Add("PI_COLUMNA", columna);
                p.Add("PI_ORDEN", orden);
                p.Add("PO_REF", dbType: OracleDbType.RefCursor, direction: ParameterDirection.Output);
                lista = db.Query<PalabraClaveBE>(sp, p, commandType: CommandType.StoredProcedure).ToList();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            return lista;
        }

        public List<PalabraClaveBE> ListarPalabraClave(OracleConnection db)
        {
            List<PalabraClaveBE> lista = new List<PalabraClaveBE>();

            try
            {
                string sp = $"{Package.Mantenimiento}USP_SEL_LISTA_PALABRAS";
                OracleDynamicParameters p = new OracleDynamicParameters();
                p.Add("PO_REFCURSOR", dbType: OracleDbType.RefCursor, direction: ParameterDirection.Output);
                lista = db.Query<PalabraClaveBE>(sp, p, commandType: CommandType.StoredProcedure).ToList();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            return lista;
        }
    }
}
