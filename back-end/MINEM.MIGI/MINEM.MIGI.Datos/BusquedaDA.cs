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
    public class BusquedaDA : BaseDA
    {
        public BusquedaBE FiltrarInformacion(string cadena, string anios, string palabras_omitidas, OracleConnection db)
        {
            BusquedaBE item = new BusquedaBE();

            try
            {
                string sp = $"{Package.Filtro}USP_SEL_FILTRAR";
                OracleDynamicParameters p = new OracleDynamicParameters();
                p.Add("PI_PALABRAS", cadena);
                p.Add("PI_PALABRAS_OMITIDAS", palabras_omitidas);
                p.Add("PI_ANIOS", anios);                
                p.Add("PO_REFCURSOR", dbType: OracleDbType.RefCursor, direction: ParameterDirection.Output);
                item.LISTA_ORDENCOMPRA = db.Query<OrdenCompraBE>(sp, p, commandType: CommandType.StoredProcedure).ToList();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            return item;
        }

        public BusquedaBE FiltrarInformacionM8U(string cadena, string anios, string palabras_omitidas, OracleConnection db)
        {
            BusquedaBE item = new BusquedaBE();

            try
            {
                string sp = $"{Package.Filtro}USP_SEL_FILTRAR_M8U";
                OracleDynamicParameters p = new OracleDynamicParameters();
                p.Add("PI_PALABRAS", cadena);
                p.Add("PI_PALABRAS_OMITIDAS", palabras_omitidas);
                p.Add("PI_ANIOS", anios);
                p.Add("PO_REFCURSOR", dbType: OracleDbType.RefCursor, direction: ParameterDirection.Output);
                item.LISTA_ORDENCOMPRAM8U = db.Query<OrdenCompraM8UBE>(sp, p, commandType: CommandType.StoredProcedure).ToList();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            return item;
        }

        public GraficoBE FiltrarGraficoN1(string sql, OracleConnection db)
        {
            GraficoBE item = new GraficoBE();

            try
            {
                string sp = $"{Package.Filtro}USP_SEL_FILTRAR_GRAFICON1";
                OracleDynamicParameters p = new OracleDynamicParameters();
                p.Add("PI_SQL", sql);
                p.Add("PO_REFCURSOR", dbType: OracleDbType.RefCursor, direction: ParameterDirection.Output);
                item.LISTA_GRAFICON1 = db.Query<GraficoN1BE>(sp, p, commandType: CommandType.StoredProcedure).ToList();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            return item;
        }

        public List<OrdenCompraBE> FiltrarPalabraBusqueda(string sql, OracleConnection db)
        {
            List<OrdenCompraBE> lstOrdenCompra = new List<OrdenCompraBE>();

            try
            {
                string sp = $"{Package.Filtro}USP_SEL_FILTRAR_PALABRA_BUSQ";
                OracleDynamicParameters p = new OracleDynamicParameters();
                p.Add("PI_SQL", sql);
                p.Add("PO_REFCURSOR", dbType: OracleDbType.RefCursor, direction: ParameterDirection.Output);
                lstOrdenCompra = db.Query<OrdenCompraBE>(sp, p, commandType: CommandType.StoredProcedure).ToList();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            return lstOrdenCompra;
        }

        public List<GraficoN1M8UBE> FiltrarPalabraBusquedaM8U(string sql, OracleConnection db)
        {
            List<GraficoN1M8UBE> lstOrdenCompra = new List<GraficoN1M8UBE>();

            try
            {
                string sp = $"{Package.Filtro}USP_SEL_FILTRAR_PALABRA_BUSQ";
                OracleDynamicParameters p = new OracleDynamicParameters();
                p.Add("PI_SQL", sql);
                p.Add("PO_REFCURSOR", dbType: OracleDbType.RefCursor, direction: ParameterDirection.Output);
                lstOrdenCompra = db.Query<GraficoN1M8UBE>(sp, p, commandType: CommandType.StoredProcedure).ToList();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            return lstOrdenCompra;
        }

        public bool GuardarResultado(BusquedaBE obj, out int idresultado, OracleConnection db)
        {
            bool seGuardo = false;
            idresultado = -1;

            try
            {
                string sp = $"{Package.Filtro}USP_INS_GUARDAR_RESULTADO";
                var p = new OracleDynamicParameters();
                p.Add("PI_ID_TIPO_BUSQUEDA", obj.ID_TIPO_BUSQUEDA);
                p.Add("PI_ID_USUARIO", obj.UPD_USUARIO);
                p.Add("PI_ID_GET", 0, OracleDbType.Int32, ParameterDirection.Output);
                p.Add("PO_ROWAFFECTED", dbType: OracleDbType.Int32, direction: ParameterDirection.Output);
                db.Execute(sp, p, commandType: CommandType.StoredProcedure);
                idresultado = (int)p.Get<dynamic>("PI_ID_GET").Value;
                int filasAfectadas = (int)p.Get<dynamic>("PO_ROWAFFECTED").Value;
                seGuardo = filasAfectadas > 0 && idresultado > 0;
            }
            catch (Exception ex) { Log.Error(ex); }

            return seGuardo;
        }

        public bool GuardarResultadoPalabraClave(PalabraClaveBE obj, int idresultado, int idusuario, OracleConnection db)
        {
            bool seGuardo = false;

            try
            {
                string sp = $"{Package.Filtro}USP_INS_GUARDAR_PALABRA";
                var p = new OracleDynamicParameters();
                p.Add("PI_PALABRA", obj.PALABRA);
                p.Add("PI_ID_RESULTADO", idresultado);
                p.Add("PI_ID_USUARIO", idusuario);
                p.Add("PO_ROWAFFECTED", dbType: OracleDbType.Int32, direction: ParameterDirection.Output);
                db.Execute(sp, p, commandType: CommandType.StoredProcedure);
                int filasAfectadas = (int)p.Get<dynamic>("PO_ROWAFFECTED").Value;
                seGuardo = filasAfectadas > 0;
            }
            catch (Exception ex) { Log.Error(ex); }

            return seGuardo;
        }

        public bool GuardarResultadoPalabraClaveCantidad(PalabraClaveCantidadBE obj, int idresultado, int idusuario, OracleConnection db)
        {
            bool seGuardo = false;

            try
            {
                string sp = $"{Package.Filtro}USP_INS_GUARDAR_PALABRA_CANT";
                var p = new OracleDynamicParameters();
                p.Add("PI_PALABRA_CANTIDAD", obj.PALABRA_CANTIDAD);
                p.Add("PI_ID_RESULTADO", idresultado);
                p.Add("PI_ID_USUARIO", idusuario);
                p.Add("PO_ROWAFFECTED", dbType: OracleDbType.Int32, direction: ParameterDirection.Output);
                db.Execute(sp, p, commandType: CommandType.StoredProcedure);
                int filasAfectadas = (int)p.Get<dynamic>("PO_ROWAFFECTED").Value;
                seGuardo = filasAfectadas > 0;
            }
            catch (Exception ex) { Log.Error(ex); }

            return seGuardo;
        }

        public bool GuardarResultadoAnio(AnioBE obj, int idresultado, int idusuario, OracleConnection db)
        {
            bool seGuardo = false;

            try
            {
                string sp = $"{Package.Filtro}USP_INS_GUARDAR_ANIO";
                var p = new OracleDynamicParameters();
                p.Add("PI_ANIO", obj.ANIO);
                p.Add("PI_ID_RESULTADO", idresultado);
                p.Add("PI_ID_USUARIO", idusuario);
                p.Add("PO_ROWAFFECTED", dbType: OracleDbType.Int32, direction: ParameterDirection.Output);
                db.Execute(sp, p, commandType: CommandType.StoredProcedure);
                int filasAfectadas = (int)p.Get<dynamic>("PO_ROWAFFECTED").Value;
                seGuardo = filasAfectadas > 0;
            }
            catch (Exception ex) { Log.Error(ex); }

            return seGuardo;
        }
    }
}
