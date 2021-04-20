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
    public class AnioDA : BaseDA
    {
        public bool GuardarAnio(AnioBE entidad, OracleConnection db)
        {
            bool seGuardo = false;
            try
            {
                string sp = $"{Package.Mantenimiento}USP_PRC_GUARDAR_ANIO";
                var p = new OracleDynamicParameters();
                p.Add("PI_ID_ANIO", entidad.ID_ANIO);
                p.Add("PI_ANIO", entidad.ANIO);
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

        public bool EliminarAnio(AnioBE entidad, OracleConnection db)
        {
            bool seGuardo = false;
            try
            {
                string sp = $"{Package.Mantenimiento}USP_DEL_ANIO";
                var p = new OracleDynamicParameters();
                p.Add("PI_ID_ANIO", entidad.ID_ANIO);
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

        public AnioBE ObtenerAnio(int idanio, OracleConnection db)
        {
            AnioBE item = null;

            try
            {
                string sp = $"{Package.Mantenimiento}USP_SEL_ANIO";
                var p = new OracleDynamicParameters();
                p.Add("PI_ID_ANIO", idanio);
                p.Add("PO_REF", dbType: OracleDbType.RefCursor, direction: ParameterDirection.Output);
                item = db.Query<AnioBE>(sp, p, commandType: CommandType.StoredProcedure).FirstOrDefault();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            return item;
        }

        public List<AnioBE> BuscarAnios(string busqueda, int registros, int pagina, string columna, string orden, OracleConnection db)
        {
            List<AnioBE> lista = new List<AnioBE>();

            try
            {
                string sp = $"{Package.Mantenimiento}USP_SEL_LISTA_BUSQ_ANIO";
                var p = new OracleDynamicParameters();
                p.Add("PI_BUSCAR", busqueda);
                p.Add("PI_REGISTROS", registros);
                p.Add("PI_PAGINA", pagina);
                p.Add("PI_COLUMNA", columna);
                p.Add("PI_ORDEN", orden);
                p.Add("PO_REF", dbType: OracleDbType.RefCursor, direction: ParameterDirection.Output);
                lista = db.Query<AnioBE>(sp, p, commandType: CommandType.StoredProcedure).ToList();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            return lista;
        }

        public List<AnioBE> ListarAnio(OracleConnection db)
        {
            List<AnioBE> lista = new List<AnioBE>();

            try
            {
                string sp = $"{Package.Mantenimiento}USP_SEL_LISTA_ANIO";
                OracleDynamicParameters p = new OracleDynamicParameters();
                p.Add("PO_REFCURSOR", dbType: OracleDbType.RefCursor, direction: ParameterDirection.Output);
                lista = db.Query<AnioBE>(sp, p, commandType: CommandType.StoredProcedure).ToList();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            return lista;
        }
    }
}
