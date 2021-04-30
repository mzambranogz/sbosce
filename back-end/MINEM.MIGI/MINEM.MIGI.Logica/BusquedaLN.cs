using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MINEM.MIGI.Datos;
using MINEM.MIGI.Entidad;
using System.Data;
using System.Text.RegularExpressions;
using MINEM.MIGI.Util;
using Oracle.DataAccess.Client;

namespace MINEM.MIGI.Logica
{
    public class BusquedaLN : BaseLN
    {
        BusquedaDA BusquedaDA = new BusquedaDA();

        public BusquedaBE FiltrarInformacion(BusquedaBE obj)
        {
            BusquedaBE entidad = new BusquedaBE();
            List<string> lstCadena = new List<string>();
            List<string> lstAnio = new List<string>();
            string aniosFiltrar = "";
            try
            {
                cn.Open();
                //foreach (PalabraClaveBE pc in obj.LISTA_PALABRAS)
                //    lstCadena.Add($" LOWER(TRANSLATE(DESCRIPCION_ORDEN,'ÁÉÍÓÚáéíóú','AEIOUaeiou')) LIKE '%' || LOWER(TRANSLATE('{pc.PALABRA}','ÁÉÍÓÚáéíóú','AEIOUaeiou')) ||'%' ");
                //palabrasFiltrar = string.Join("OR", lstCadena);

                foreach (AnioBE a in obj.LISTA_ANIOS)
                    lstAnio.Add($" '{a.ANIO}' ");
                aniosFiltrar = string.Join(",", lstAnio);

                //entidad = BusquedaDA.FiltrarInformacion(palabrasFiltrar, aniosFiltrar, cn);

                List<GraficoBE> lstGrafico = new List<GraficoBE>();
                List<string> lstPalabraOmitidas = new List<string>();
                foreach (PalabraClaveBE pc in obj.LISTA_PALABRAS)
                {
                    string sql = "";
                    if (lstPalabraOmitidas.Count == 0)
                    {
                        sql = $"SELECT '{pc.PALABRA}' HALLAZGO, OBJETOCONTRACTUAL TIPO_REQUERIMIENTO,  COUNT(*) CANTIDAD, ANIO  FROM T_GENM_MIGI ";
                        sql += $" WHERE LOWER(TRANSLATE(DESCRIPCION_ORDEN,'ÁÉÍÓÚáéíóú','AEIOUaeiou')) LIKE '%' || LOWER(TRANSLATE('{pc.PALABRA}','ÁÉÍÓÚáéíóú','AEIOUaeiou')) ||'%' AND ANIO IN ({aniosFiltrar}) AND (OBJETOCONTRACTUAL LIKE '%SERVICIOS%' OR OBJETOCONTRACTUAL LIKE '%BIENES%') GROUP BY ANIO, OBJETOCONTRACTUAL";
                        lstPalabraOmitidas.Add(pc.PALABRA);
                    }
                    else
                    {
                        string po = "";
                        foreach (string p in lstPalabraOmitidas)
                        {
                            po += $" AND LOWER(TRANSLATE(DESCRIPCION_ORDEN,'ÁÉÍÓÚáéíóú','AEIOUaeiou')) NOT LIKE '%' || LOWER(TRANSLATE('{p}','ÁÉÍÓÚáéíóú','AEIOUaeiou')) ||'%' ";
                        }
                        sql = $"SELECT '{pc.PALABRA}' HALLAZGO, OBJETOCONTRACTUAL TIPO_REQUERIMIENTO,  COUNT(*) CANTIDAD, ANIO  FROM T_GENM_MIGI ";
                        sql += $" WHERE LOWER(TRANSLATE(DESCRIPCION_ORDEN,'ÁÉÍÓÚáéíóú','AEIOUaeiou')) LIKE '%' || LOWER(TRANSLATE('{pc.PALABRA}','ÁÉÍÓÚáéíóú','AEIOUaeiou')) ||'%' AND ANIO IN ({aniosFiltrar}) {po} AND (OBJETOCONTRACTUAL LIKE '%SERVICIOS%' OR OBJETOCONTRACTUAL LIKE '%BIENES%') GROUP BY ANIO, OBJETOCONTRACTUAL";
                        lstPalabraOmitidas.Add(pc.PALABRA);
                    }
                    lstGrafico.Add(BusquedaDA.FiltrarGraficoN1(sql, cn));
                }
                entidad.LISTA_GRAFICO = lstGrafico;

                //List<PalabraBusquedaBE> lstPalabraBusqueda = new List<PalabraBusquedaBE>();
                //lstPalabraBusqueda.Add(new PalabraBusquedaBE { ID_PALABRA_BUSQUEDA = 1, PALABRA_BUSQUEDA = "ADQUISICION DE" });

                List<GraficoBE> lstGeneralGraficoN3 = new List<GraficoBE>();               

                List<string> lstPalabraBusquedaOmit = new List<string>();
                foreach (PalabraClaveCantidadBE pb in obj.LISTA_PALABRAS_CANTIDAD)
                //foreach (PalabraBusquedaBE pb in lstPalabraBusqueda)
                {
                    List<string> lstPalabraOmit = new List<string>();
                    foreach (PalabraClaveBE pc in obj.LISTA_PALABRAS)
                    {
                        GraficoBE ggf = new GraficoBE();
                        List<GraficoN3BE> lstGraficoN3 = new List<GraficoN3BE>();
                        foreach (AnioBE a in obj.LISTA_ANIOS)
                        {
                            GraficoN3BE gfn3 = new GraficoN3BE() { HALLAZGO = pc.PALABRA, ANIO = a.ANIO, CANTIDAD = 0 };
                            string sql = "";
                            if (lstPalabraBusquedaOmit.Count == 0)
                            {
                                if (lstPalabraOmit.Count == 0)
                                {
                                    sql = $"SELECT '{pc.PALABRA}' HALLAZGO, OBJETOCONTRACTUAL, ANIO, DESCRIPCION_ORDEN  FROM T_GENM_MIGI ";
                                    sql += $" WHERE LOWER(TRANSLATE(DESCRIPCION_ORDEN,'ÁÉÍÓÚáéíóú','AEIOUaeiou')) LIKE '%' || LOWER(TRANSLATE('{pc.PALABRA}','ÁÉÍÓÚáéíóú','AEIOUaeiou')) ||'%' AND ANIO IN ({a.ANIO}) AND (OBJETOCONTRACTUAL LIKE '%SERVICIOS%' OR OBJETOCONTRACTUAL LIKE '%BIENES%') ";
                                    sql += $" AND LOWER(TRANSLATE(DESCRIPCION_ORDEN,'ÁÉÍÓÚáéíóú','AEIOUaeiou')) LIKE '%' || LOWER(TRANSLATE('{pb.PALABRA_CANTIDAD}','ÁÉÍÓÚáéíóú','AEIOUaeiou')) ||'%' ";
                                    //lstPalabraOmit.Add(pc.PALABRA);
                                }
                                else
                                {
                                    string po = "";
                                    foreach (string p in lstPalabraOmit)
                                    {
                                        po += $" AND LOWER(TRANSLATE(DESCRIPCION_ORDEN,'ÁÉÍÓÚáéíóú','AEIOUaeiou')) NOT LIKE '%' || LOWER(TRANSLATE('{p}','ÁÉÍÓÚáéíóú','AEIOUaeiou')) ||'%' ";
                                    }
                                    sql = $"SELECT '{pc.PALABRA}' HALLAZGO, OBJETOCONTRACTUAL, ANIO, DESCRIPCION_ORDEN  FROM T_GENM_MIGI ";
                                    sql += $" WHERE DESCRIPCION_ORDEN LIKE '%{pc.PALABRA}%' AND ANIO IN ({a.ANIO}) {po} AND (OBJETOCONTRACTUAL LIKE '%SERVICIOS%' OR OBJETOCONTRACTUAL LIKE '%BIENES%') ";
                                    sql += $" AND LOWER(TRANSLATE(DESCRIPCION_ORDEN,'ÁÉÍÓÚáéíóú','AEIOUaeiou')) LIKE '%' || LOWER(TRANSLATE('{pb.PALABRA_CANTIDAD}','ÁÉÍÓÚáéíóú','AEIOUaeiou')) ||'%' ";
                                    //lstPalabraOmit.Add(pc.PALABRA);
                                }
                            }
                            else
                            {
                                string po_b = "";
                                foreach (string p in lstPalabraBusquedaOmit)
                                {
                                    po_b += $" AND LOWER(TRANSLATE(DESCRIPCION_ORDEN,'ÁÉÍÓÚáéíóú','AEIOUaeiou')) NOT LIKE '%' || LOWER(TRANSLATE('{p}','ÁÉÍÓÚáéíóú','AEIOUaeiou')) ||'%' ";
                                }
                                if (lstPalabraOmit.Count == 0)
                                {
                                    sql = $"SELECT '{pc.PALABRA}' HALLAZGO, OBJETOCONTRACTUAL, ANIO, DESCRIPCION_ORDEN  FROM T_GENM_MIGI ";
                                    sql += $" WHERE LOWER(TRANSLATE(DESCRIPCION_ORDEN,'ÁÉÍÓÚáéíóú','AEIOUaeiou')) LIKE '%' || LOWER(TRANSLATE('{pc.PALABRA}','ÁÉÍÓÚáéíóú','AEIOUaeiou')) ||'%' AND ANIO IN ({a.ANIO}) AND (OBJETOCONTRACTUAL LIKE '%SERVICIOS%' OR OBJETOCONTRACTUAL LIKE '%BIENES%') ";
                                    sql += $" AND LOWER(TRANSLATE(DESCRIPCION_ORDEN,'ÁÉÍÓÚáéíóú','AEIOUaeiou')) LIKE '%' || LOWER(TRANSLATE('{pb.PALABRA_CANTIDAD}','ÁÉÍÓÚáéíóú','AEIOUaeiou')) ||'%' {po_b} ";
                                    //lstPalabraOmit.Add(pc.PALABRA);
                                }
                                else
                                {
                                    string po = "";
                                    foreach (string p in lstPalabraOmit)
                                    {
                                        po += $" AND LOWER(TRANSLATE(DESCRIPCION_ORDEN,'ÁÉÍÓÚáéíóú','AEIOUaeiou')) NOT LIKE '%' || LOWER(TRANSLATE('{p}','ÁÉÍÓÚáéíóú','AEIOUaeiou')) ||'%' ";
                                    }
                                    sql = $"SELECT '{pc.PALABRA}' HALLAZGO, OBJETOCONTRACTUAL, ANIO, DESCRIPCION_ORDEN  FROM T_GENM_MIGI ";
                                    sql += $" WHERE DESCRIPCION_ORDEN LIKE '%{pc.PALABRA}%' AND ANIO IN ({a.ANIO}) {po} AND (OBJETOCONTRACTUAL LIKE '%SERVICIOS%' OR OBJETOCONTRACTUAL LIKE '%BIENES%') ";
                                    sql += $" AND LOWER(TRANSLATE(DESCRIPCION_ORDEN,'ÁÉÍÓÚáéíóú','AEIOUaeiou')) LIKE '%' || LOWER(TRANSLATE('{pb.PALABRA_CANTIDAD}','ÁÉÍÓÚáéíóú','AEIOUaeiou')) ||'%' {po_b} ";
                                    //lstPalabraOmit.Add(pc.PALABRA);
                                }
                            }
                            List<OrdenCompraBE> lstOC = BusquedaDA.FiltrarPalabraBusqueda(sql, cn);
                            foreach (OrdenCompraBE palabraB in lstOC)
                            {
                                string palabraBusqueda = quitarAcentos(pb.PALABRA_CANTIDAD.ToLower());
                                string palabraEvaluar = quitarAcentos(palabraB.DESCRIPCION_ORDEN.ToLower());
                                int lastIndex = palabraEvaluar.IndexOf(palabraBusqueda);
                                if (lastIndex > -1)
                                {
                                    lastIndex += palabraBusqueda.Length - 1;
                                    int tamanio = palabraEvaluar.Length;
                                    if (tamanio > lastIndex + 1) //se suma 1 porque lastIndex es un indice que empiza en 0
                                    {
                                        string seccionrestante = palabraEvaluar.Substring(lastIndex + 2); //Se suma 2 corra una letra mas y para que salte el espacio
                                        if (seccionrestante.Length > 2)
                                        {
                                            int cantidad = 0;
                                            string[] result = seccionrestante.Split(' ');
                                            if (result.Length > 0)
                                            {
                                                int n;
                                                bool v = Int32.TryParse(result[0], out n);
                                                if (v)
                                                {
                                                    gfn3.CANTIDAD += Convert.ToInt32(result[0]);
                                                }
                                                else if (ValidarNumeroParent(result[0], out cantidad))
                                                {
                                                    gfn3.CANTIDAD += cantidad;
                                                }
                                                else
                                                {
                                                    int numero = 0;
                                                    int num_millones = 0;
                                                    foreach (string item in result)
                                                    {
                                                        int num = Conversion.LetrasANumero(quitarAcentos(item.Trim().ToLower()).ToUpper());
                                                        if (num == 0) break;
                                                        else if (num == -1) numero += 0;
                                                        else if (num > 0)
                                                        {
                                                            if (num == 1000000)
                                                            {
                                                                num_millones = numero == 0 ? num : numero * num;
                                                                numero = 0;
                                                            }
                                                            else if (num == 1000) numero = numero == 0 ? num : numero * num;
                                                            else numero += num;
                                                        }
                                                    }
                                                    numero += num_millones;
                                                    gfn3.CANTIDAD += numero;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            lstGraficoN3.Add(gfn3);
                        }
                        ggf.LISTA_GRAFICON3 = lstGraficoN3;
                        lstGeneralGraficoN3.Add(ggf);
                        lstPalabraOmit.Add(pc.PALABRA);
                    }
                    lstPalabraBusquedaOmit.Add(pb.PALABRA_CANTIDAD);
                }
                entidad.LISTA_GRAFICON3 = lstGeneralGraficoN3;
            }
            finally { if (cn.State == ConnectionState.Open) cn.Close(); }

            return entidad;
        }

        private bool ValidarNumeroParent(string result, out int cantidad)
        {
            bool valido = false;
            //string[] result = "(45) unidades".Split(' ');
            int tamnio = result.Trim().Length;
            cantidad = 0;
            for (int i = 0; i < tamnio; i++)
            {
                string dato = result.Substring(i, 1);
                switch (dato)
                {
                    case "(":
                        int fin, tamaniocontenido;
                        string contenido;
                        fin = result.IndexOf(")", i);
                        if (fin != -1)
                        {
                            tamaniocontenido = fin - i - 1;
                            contenido = result.Substring(i + 1, tamaniocontenido);
                            bool v = Int32.TryParse(contenido, out cantidad);
                            if (v)
                            {
                                cantidad = Convert.ToInt32(contenido);
                                valido = true;
                            }
                            i = fin;
                        }

                        break;
                    default: break;
                }
            }
            return valido;
        }

        private string quitarAcentos(string inputString) {
            Regex replace_a_Accents = new Regex("[á|à|ä|â]", RegexOptions.Compiled);
            Regex replace_e_Accents = new Regex("[é|è|ë|ê]", RegexOptions.Compiled);
            Regex replace_i_Accents = new Regex("[í|ì|ï|î]", RegexOptions.Compiled);
            Regex replace_o_Accents = new Regex("[ó|ò|ö|ô]", RegexOptions.Compiled);
            Regex replace_u_Accents = new Regex("[ú|ù|ü|û]", RegexOptions.Compiled);
            inputString = replace_a_Accents.Replace(inputString, "a");
            inputString = replace_e_Accents.Replace(inputString, "e");
            inputString = replace_i_Accents.Replace(inputString, "i");
            inputString = replace_o_Accents.Replace(inputString, "o");
            inputString = replace_u_Accents.Replace(inputString, "u");
            return inputString;
        }

        public BusquedaBE FiltrarInformacionM8U(BusquedaBE obj)
        {
            BusquedaBE entidad = new BusquedaBE();
            List<string> lstCadena = new List<string>();
            try
            {
                cn.Open();
                List<GraficoBE> lstGrafico = new List<GraficoBE>();
                List<string> lstPalabraOmitidas = new List<string>();
                foreach (PalabraClaveBE pc in obj.LISTA_PALABRAS)
                {
                    string sql = "";
                    List<GraficoM8UBE> lstGraficoN1M8U = new List<GraficoM8UBE>();                    
                    foreach (AnioBE a in obj.LISTA_ANIOS)
                    {
                        GraficoM8UBE ggf = new GraficoM8UBE();
                        if (lstPalabraOmitidas.Count == 0)
                        {
                            sql = $"SELECT '{pc.PALABRA}' HALLAZGO, OBJETO_CONTRACTUAL TIPO_REQUERIMIENTO,  COUNT(*) CANTIDAD, SUM(ROUND(TO_NUMBER(CANTIDAD_ADJUDICADO_ITEM, '9999999990.0000'))*100/100) CANTIDAD_ITEM, '{a.ANIO}' ANIO FROM T_GENM_MIGI_M8U ";
                            sql += $" WHERE LOWER(TRANSLATE(DESCRIPCION_PROCESO,'ÁÉÍÓÚáéíóú','AEIOUaeiou')) LIKE '%' || LOWER(TRANSLATE('{pc.PALABRA}','ÁÉÍÓÚáéíóú','AEIOUaeiou')) ||'%' AND (OBJETO_CONTRACTUAL LIKE '%SERVICIOS%' OR OBJETO_CONTRACTUAL LIKE '%BIENES%') AND FECHA_CONVOCATORIA LIKE '%/{a.ANIO.Substring(2)}' ";
                            sql += $" GROUP BY OBJETO_CONTRACTUAL ";
                        }
                        else
                        {
                            string po = "";
                            foreach (string p in lstPalabraOmitidas)
                            {
                                po += $" AND LOWER(TRANSLATE(DESCRIPCION_PROCESO,'ÁÉÍÓÚáéíóú','AEIOUaeiou')) NOT LIKE '%' || LOWER(TRANSLATE('{p}','ÁÉÍÓÚáéíóú','AEIOUaeiou')) ||'%' ";
                            }
                            sql = $"SELECT '{pc.PALABRA}' HALLAZGO, OBJETO_CONTRACTUAL TIPO_REQUERIMIENTO,  COUNT(*) CANTIDAD, SUM(ROUND(TO_NUMBER(CANTIDAD_ADJUDICADO_ITEM, '9999999990.0000'))*100/100) CANTIDAD_ITEM, '{a.ANIO}' ANIO FROM T_GENM_MIGI_M8U ";
                            sql += $" WHERE LOWER(TRANSLATE(DESCRIPCION_PROCESO,'ÁÉÍÓÚáéíóú','AEIOUaeiou')) LIKE '%' || LOWER(TRANSLATE('{pc.PALABRA}','ÁÉÍÓÚáéíóú','AEIOUaeiou')) ||'%'  {po}  AND (OBJETO_CONTRACTUAL LIKE '%SERVICIOS%' OR OBJETO_CONTRACTUAL LIKE '%BIENES%') AND FECHA_CONVOCATORIA LIKE '%/{a.ANIO.Substring(2)}' ";
                            sql += $" GROUP BY OBJETO_CONTRACTUAL ";
                        }
                        ggf.LISTA_GRAFICON1M8U = BusquedaDA.FiltrarPalabraBusquedaM8U(sql, cn);
                        lstGraficoN1M8U.Add(ggf);
                    }
                    lstPalabraOmitidas.Add(pc.PALABRA);
                    lstGrafico.Add(new GraficoBE { LISTA_GRAFICOM8U = lstGraficoN1M8U, HALLAZGO = pc.PALABRA });
                }
                entidad.LISTA_GRAFICO = lstGrafico;
            }
            finally { if (cn.State == ConnectionState.Open) cn.Close(); }

            return entidad;
        }

        public List<BusquedaBE> FiltrarInformacionExportar(BusquedaBE obj) {
            List<BusquedaBE> lstBusqueda = new List<BusquedaBE>();
            List<string> lstCadena = new List<string>();
            List<string> lstAnio = new List<string>();
            string palabrasFiltrar = "", aniosFiltrar = "", palabrasOmitidas = "";
            try
            {
                cn.Open();
                //foreach (PalabraClaveBE pc in obj.LISTA_PALABRAS)
                //    lstCadena.Add($" LOWER(TRANSLATE(DESCRIPCION_ORDEN,'ÁÉÍÓÚáéíóú','AEIOUaeiou')) LIKE '%' || LOWER(TRANSLATE('{pc.PALABRA}','ÁÉÍÓÚáéíóú','AEIOUaeiou')) ||'%' ");
                //palabrasFiltrar = string.Join("OR", lstCadena);

                foreach (AnioBE a in obj.LISTA_ANIOS)
                    lstAnio.Add($" '{a.ANIO}' ");
                aniosFiltrar = string.Join(",", lstAnio);

                //entidad = BusquedaDA.FiltrarInformacion(palabrasFiltrar, aniosFiltrar, cn);

                List<string> lstPalabraOmitidas = new List<string>();
                foreach (PalabraClaveBE pc in obj.LISTA_PALABRAS)
                {
                    BusquedaBE entidad = new BusquedaBE();
                    if (lstPalabraOmitidas.Count == 0)
                    {
                        palabrasFiltrar = $" LOWER(TRANSLATE(DESCRIPCION_ORDEN,'ÁÉÍÓÚáéíóú','AEIOUaeiou')) LIKE '%' || LOWER(TRANSLATE('{pc.PALABRA}','ÁÉÍÓÚáéíóú','AEIOUaeiou')) ||'%' ";
                        lstPalabraOmitidas.Add(pc.PALABRA);
                    }
                    else
                    {
                        foreach (string p in lstPalabraOmitidas)
                        {
                            palabrasOmitidas += $" AND LOWER(TRANSLATE(DESCRIPCION_ORDEN,'ÁÉÍÓÚáéíóú','AEIOUaeiou')) NOT LIKE '%' || LOWER(TRANSLATE('{p}','ÁÉÍÓÚáéíóú','AEIOUaeiou')) ||'%' ";
                        }
                        palabrasFiltrar = $" LOWER(TRANSLATE(DESCRIPCION_ORDEN,'ÁÉÍÓÚáéíóú','AEIOUaeiou')) LIKE '%' || LOWER(TRANSLATE('{pc.PALABRA}','ÁÉÍÓÚáéíóú','AEIOUaeiou')) ||'%' ";
                        lstPalabraOmitidas.Add(pc.PALABRA);
                    }
                    //lstGrafico.Add(BusquedaDA.FiltrarGraficoN1(sql, cn));
                    entidad = BusquedaDA.FiltrarInformacion(palabrasFiltrar, aniosFiltrar, palabrasOmitidas, cn);
                    lstBusqueda.Add(entidad);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
            return lstBusqueda;
        }

        public List<BusquedaBE> FiltrarInformacionExportarM8U(BusquedaBE obj)
        {
            List<BusquedaBE> lstBusqueda = new List<BusquedaBE>();
            List<string> lstCadena = new List<string>();
            List<string> lstAnio = new List<string>();
            string palabrasFiltrar = "", aniosFiltrar = "", palabrasOmitidas = "";
            try
            {
                cn.Open();
                List<string> lstPalabraOmitidas = new List<string>();
                foreach (PalabraClaveBE pc in obj.LISTA_PALABRAS)
                {
                    foreach (AnioBE a in obj.LISTA_ANIOS)
                    {
                        BusquedaBE entidad = new BusquedaBE();
                        if (lstPalabraOmitidas.Count == 0)
                        {
                            palabrasFiltrar = $" LOWER(TRANSLATE(DESCRIPCION_PROCESO,'ÁÉÍÓÚáéíóú','AEIOUaeiou')) LIKE '%' || LOWER(TRANSLATE('{pc.PALABRA}','ÁÉÍÓÚáéíóú','AEIOUaeiou')) ||'%' ";
                            aniosFiltrar = $" AND FECHA_CONVOCATORIA LIKE '%/{a.ANIO.Substring(2)}' ";
                        }
                        else
                        {
                            foreach (string p in lstPalabraOmitidas)
                            {
                                palabrasOmitidas += $" AND LOWER(TRANSLATE(DESCRIPCION_PROCESO,'ÁÉÍÓÚáéíóú','AEIOUaeiou')) NOT LIKE '%' || LOWER(TRANSLATE('{p}','ÁÉÍÓÚáéíóú','AEIOUaeiou')) ||'%' ";
                            }
                            palabrasFiltrar = $" LOWER(TRANSLATE(DESCRIPCION_PROCESO,'ÁÉÍÓÚáéíóú','AEIOUaeiou')) LIKE '%' || LOWER(TRANSLATE('{pc.PALABRA}','ÁÉÍÓÚáéíóú','AEIOUaeiou')) ||'%' ";
                            aniosFiltrar = $" AND FECHA_CONVOCATORIA LIKE '%/{a.ANIO.Substring(2)}' ";
                        }
                        entidad = BusquedaDA.FiltrarInformacionM8U(palabrasFiltrar, aniosFiltrar, palabrasOmitidas, cn);
                        lstBusqueda.Add(entidad);
                    }
                    lstPalabraOmitidas.Add(pc.PALABRA);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
            return lstBusqueda;
        }

        public bool GuardarResultado(BusquedaBE obj) {
            bool seGuardo = false;
            int idresultado = -1;
            try
            {
                cn.Open();
                using (OracleTransaction ot = cn.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
                {
                    seGuardo = BusquedaDA.GuardarResultado(obj, out idresultado, cn);
                    if (seGuardo)
                    {
                        if (obj.LISTA_PALABRAS != null)
                            if (obj.LISTA_PALABRAS.Count > 0)
                                foreach (PalabraClaveBE p in obj.LISTA_PALABRAS)
                                {
                                    if (!(seGuardo = BusquedaDA.GuardarResultadoPalabraClave(p, idresultado, obj.UPD_USUARIO, cn)))
                                        break;
                                }
                    }

                    if (seGuardo)
                    {
                        if (obj.LISTA_PALABRAS_CANTIDAD != null)
                            if (obj.LISTA_PALABRAS_CANTIDAD.Count > 0)
                                foreach (PalabraClaveCantidadBE pc in obj.LISTA_PALABRAS_CANTIDAD)
                                {
                                    if (!(seGuardo = BusquedaDA.GuardarResultadoPalabraClaveCantidad(pc, idresultado, obj.UPD_USUARIO, cn)))
                                        break;
                                }
                    }

                    if (seGuardo)
                    {
                        if (obj.LISTA_ANIOS != null)
                            if (obj.LISTA_ANIOS.Count > 0)
                                foreach (AnioBE a in obj.LISTA_ANIOS)
                                {
                                    if (!(seGuardo = BusquedaDA.GuardarResultadoAnio(a, idresultado, obj.UPD_USUARIO, cn)))
                                        break;
                                }
                    }

                    if (seGuardo) ot.Commit();
                    else ot.Rollback();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
            finally {
                if (cn.State == ConnectionState.Open) cn.Close();
            }

            return seGuardo;
        }
    }
}
