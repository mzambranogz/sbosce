﻿using MINEM.MIGI.Entidad;
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
    public class ResultadoDA : BaseDA
    {
        public List<ResultadoBE> ListaResultado(int registros, int pagina, string columna, string orden, OracleConnection db)
        {
            List<ResultadoBE> lista = new List<ResultadoBE>();

            try
            {
                string sp = $"{Package.Resultado}USP_SEL_LISTA_RESULTADO";
                var p = new OracleDynamicParameters();
                p.Add("PI_REGISTROS", registros);
                p.Add("PI_PAGINA", pagina);
                p.Add("PI_COLUMNA", columna);
                p.Add("PI_ORDEN", orden);
                p.Add("PO_REF", dbType: OracleDbType.RefCursor, direction: ParameterDirection.Output);
                lista = db.Query<ResultadoBE>(sp, p, commandType: CommandType.StoredProcedure).ToList();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            return lista;
        }

        public ResultadoBE ObtenerResultado(int idresultado, OracleConnection db)
        {
            ResultadoBE obj = new ResultadoBE();

            try
            {
                string sp = $"{Package.Resultado}USP_SEL_RESULTADO";
                var p = new OracleDynamicParameters();
                p.Add("PI_ID_RESULTADO", idresultado);
                p.Add("PO_REFCURSOR", dbType: OracleDbType.RefCursor, direction: ParameterDirection.Output);
                obj = db.Query<ResultadoBE>(sp, p, commandType: CommandType.StoredProcedure).FirstOrDefault();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            return obj;
        }

        public List<PalabraClaveBE> ObtenerPalabraClave(int idresultado, OracleConnection db)
        {
            List<PalabraClaveBE> lista = new List<PalabraClaveBE>();

            try
            {
                string sp = $"{Package.Resultado}USP_SEL_LISTA_PALABRA_CLAVE";
                var p = new OracleDynamicParameters();
                p.Add("PI_ID_RESULTADO", idresultado);
                p.Add("PO_REFCURSOR", dbType: OracleDbType.RefCursor, direction: ParameterDirection.Output);
                lista = db.Query<PalabraClaveBE>(sp, p, commandType: CommandType.StoredProcedure).ToList();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            return lista;
        }

        public List<PalabraClaveCantidadBE> ObtenerPalabraClaveCantidad(int idresultado, OracleConnection db)
        {
            List<PalabraClaveCantidadBE> lista = new List<PalabraClaveCantidadBE>();

            try
            {
                string sp = $"{Package.Resultado}USP_SEL_LISTA_PALABRA_CANT";
                var p = new OracleDynamicParameters();
                p.Add("PI_ID_RESULTADO", idresultado);
                p.Add("PO_REFCURSOR", dbType: OracleDbType.RefCursor, direction: ParameterDirection.Output);
                lista = db.Query<PalabraClaveCantidadBE>(sp, p, commandType: CommandType.StoredProcedure).ToList();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            return lista;
        }

        public List<AnioBE> ObtenerAnio(int idresultado, OracleConnection db)
        {
            List<AnioBE> lista = new List<AnioBE>();

            try
            {
                string sp = $"{Package.Resultado}USP_SEL_LISTA_ANIO";
                var p = new OracleDynamicParameters();
                p.Add("PI_ID_RESULTADO", idresultado);
                p.Add("PO_REFCURSOR", dbType: OracleDbType.RefCursor, direction: ParameterDirection.Output);
                lista = db.Query<AnioBE>(sp, p, commandType: CommandType.StoredProcedure).ToList();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            return lista;
        }

        public bool EliminarResultado(ResultadoBE obj, OracleConnection db)
        {
            bool seGuardo = false;

            try
            {
                string sp = $"{Package.Resultado}USP_DEL_ELIMINAR_RESULTADO";
                var p = new OracleDynamicParameters();
                p.Add("PI_ID_RESULTADO", obj.ID_RESULTADO);
                p.Add("PI_ID_USUARIO", obj.UPD_USUARIO);
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
