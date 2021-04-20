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
    public class EquipoDA : BaseDA
    {
        public bool GuardarEquipo(EquipoBE entidad, OracleConnection db)
        {
            bool seGuardo = false;
            try
            {
                string sp = $"{Package.Mantenimiento}USP_PRC_GUARDAR_EQUIPO";
                var p = new OracleDynamicParameters();
                p.Add("PI_ID_EQUIPO", entidad.ID_EQUIPO);
                p.Add("PI_EQUIPO", entidad.EQUIPO);
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

        public bool EliminarEquipo(EquipoBE entidad, OracleConnection db)
        {
            bool seGuardo = false;
            try
            {
                string sp = $"{Package.Mantenimiento}USP_DEL_EQUIPO";
                var p = new OracleDynamicParameters();
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

        public EquipoBE ObtenerEquipo(int id, OracleConnection db)
        {
            EquipoBE item = null;

            try
            {
                string sp = $"{Package.Mantenimiento}USP_SEL_EQUIPO";
                var p = new OracleDynamicParameters();
                p.Add("PI_ID_EQUIPO", id);
                p.Add("PO_REF", dbType: OracleDbType.RefCursor, direction: ParameterDirection.Output);
                item = db.Query<EquipoBE>(sp, p, commandType: CommandType.StoredProcedure).FirstOrDefault();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            return item;
        }

        public List<EquipoBE> BuscarEquipos(string busqueda, int registros, int pagina, string columna, string orden, OracleConnection db)
        {
            List<EquipoBE> lista = new List<EquipoBE>();

            try
            {
                string sp = $"{Package.Mantenimiento}USP_SEL_LISTA_BUSQ_EQUIPO";
                var p = new OracleDynamicParameters();
                p.Add("PI_BUSCAR", busqueda);
                p.Add("PI_REGISTROS", registros);
                p.Add("PI_PAGINA", pagina);
                p.Add("PI_COLUMNA", columna);
                p.Add("PI_ORDEN", orden);
                p.Add("PO_REF", dbType: OracleDbType.RefCursor, direction: ParameterDirection.Output);
                lista = db.Query<EquipoBE>(sp, p, commandType: CommandType.StoredProcedure).ToList();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            return lista;
        }

        public List<EquipoBE> ListarEquipo(OracleConnection db)
        {
            List<EquipoBE> lista = new List<EquipoBE>();

            try
            {
                string sp = $"{Package.Mantenimiento}USP_SEL_LISTA_EQUIPO";
                OracleDynamicParameters p = new OracleDynamicParameters();
                p.Add("PO_REFCURSOR", dbType: OracleDbType.RefCursor, direction: ParameterDirection.Output);
                lista = db.Query<EquipoBE>(sp, p, commandType: CommandType.StoredProcedure).ToList();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            return lista;
        }
    }
}
