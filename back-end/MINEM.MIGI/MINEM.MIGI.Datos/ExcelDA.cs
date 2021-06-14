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
    public class ExcelDA : BaseDA
    {
        public bool GuardarMasivo(DataTable dt, string dtName, OracleConnection db)
        {
            bool seGuardo = false;

            try
            {
                db.Open();
                using (OracleBulkCopy bulkCopy = new OracleBulkCopy(db))
                {
                    bulkCopy.DestinationTableName = dtName;
                    bulkCopy.WriteToServer(dt);
                }
                db.Close();
                seGuardo = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            return seGuardo;
        }

        public bool VerificarOrden(OrdenCompraBE obj, OracleConnection db)
        {
            bool esValido = false;

            try
            {
                string sp = $"{Package.Excel}USP_SEL_VERIFICAR_ORDEN";
                OracleDynamicParameters p = new OracleDynamicParameters();
                p.Add("PI_ANIO", obj.ANIO);
                p.Add("PI_MES", obj.MES);
                p.Add("PI_ORDEN", obj.ORDEN);
                p.Add("PO_REFCURSOR", dbType: OracleDbType.RefCursor, direction: ParameterDirection.Output);
                string consulta = db.ExecuteScalar(sp, p, commandType: CommandType.StoredProcedure).ToString();
                esValido = Convert.ToUInt32(consulta) > 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            return esValido;
        }

        public bool VerificarCodigoConvocatoriaMiembro(OrdenCompraM8UBE obj, OracleConnection db)
        {
            bool esValido = false;

            try
            {
                string sp = $"{Package.Excel}USP_SEL_VERIFICAR_CONV_MIEMBRO";
                OracleDynamicParameters p = new OracleDynamicParameters();
                p.Add("PI_CODIGOCONVOCATORIA", obj.CODIGOCONVOCATORIA);
                p.Add("PI_MIEMBRO", obj.MIEMBRO);
                p.Add("PO_REFCURSOR", dbType: OracleDbType.RefCursor, direction: ParameterDirection.Output);
                string consulta = db.ExecuteScalar(sp, p, commandType: CommandType.StoredProcedure).ToString();
                esValido = Convert.ToUInt32(consulta) > 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            return esValido;
        }

        public bool GuardarDatosArchivo(ExcelBE obj, OracleConnection db)
        {
            bool seGuardo = false;
            try
            {
                string sp = $"{Package.Excel}USP_PRC_GUARDAR_EXCEL";
                var p = new OracleDynamicParameters();
                //p.Add("PI_ID_EXCEL", entidad.ID_EXCEL);
                p.Add("PI_NOMBRE", obj.NOMBRE);
                p.Add("PI_ANIO", obj.ANIO);
                p.Add("PI_MES", obj.MES);
                p.Add("PI_ID_TIPO_EXCEL", obj.ID_TIPO_EXCEL);
                p.Add("PI_UPD_USUARIO", obj.UPD_USUARIO);
                p.Add("PO_ROWAFFECTED", dbType: OracleDbType.Int32, direction: ParameterDirection.Output);
                db.Execute(sp, p, commandType: CommandType.StoredProcedure);
                int filasAfectadas = (int)p.Get<dynamic>("PO_ROWAFFECTED").Value;
                seGuardo = filasAfectadas > 0;
            }
            catch (Exception ex) { Log.Error(ex); }

            return seGuardo;
        }

        public List<ExcelBE> ListarExcels(int tipoexcel, int registros, int pagina, string columna, string orden, OracleConnection db)
        {
            List<ExcelBE> lista = new List<ExcelBE>();

            try
            {
                string sp = $"{Package.Excel}USP_SEL_LISTA_EXCEL";
                var p = new OracleDynamicParameters();
                p.Add("PI_TIPO_EXCEL", tipoexcel);
                p.Add("PI_REGISTROS", registros);
                p.Add("PI_PAGINA", pagina);
                p.Add("PI_COLUMNA", columna);
                p.Add("PI_ORDEN", orden);
                p.Add("PO_REF", dbType: OracleDbType.RefCursor, direction: ParameterDirection.Output);
                lista = db.Query<ExcelBE>(sp, p, commandType: CommandType.StoredProcedure).ToList();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            return lista;
        }
    }
}
