using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using MINEM.MIGI.Entidad;
using MINEM.MIGI.Logica;
using MINEM.MIGI.Web.Filter;
using MINEM.MIGI.Web.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace MINEM.MIGI.Web.Controllers
{
    [HandleError]
    [Autenticado]
    public class ExcelController : BaseController
    {
        // GET: Excel
        ExcelLN ExcelLN = new ExcelLN();
        AnioLN AnioLN = new AnioLN();
        BusquedaLN BusquedaLN = new BusquedaLN();
        public ActionResult Index()
        {
            List<AnioBE> lista = AnioLN.ListarAnio();
            ViewData["listaAnio"] = lista;
            return View();
        }

        public ActionResult ExcelM8U()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Leer(ExcelViewModel model)
        {
            bool esNuevo = true, continua = false, esValido = true;
            string msg = "";
            string filePath = string.Empty;
            if (model.excel != null)
            {
                string path = Server.MapPath("~/Uploads/");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                filePath = path + Path.GetFileName(model.excel.FileName);
                string extension = Path.GetExtension(model.excel.FileName);
                model.excel.SaveAs(filePath);
            }
            ////
            esNuevo = LeerExcel(filePath, 0, 100000, model.anio, model.mes, 0, out msg, out continua);
            if (esNuevo && continua) LeerExcel(filePath, 100000, 200000, model.anio, model.mes, 5, out msg, out continua);
            if (esNuevo && continua) LeerExcel(filePath, 200000, 300000, model.anio, model.mes, 5, out msg, out continua);
            if (esNuevo && continua) LeerExcel(filePath, 300000, 400000, model.anio, model.mes, 5, out msg, out continua);
            if (esNuevo) msg = "Se proceso correctamente el excel";

            if (esNuevo) esValido = ExcelLN.GuardarDatosArchivo(new ExcelBE { NOMBRE = model.excel.FileName, ANIO = model.anio, MES = model.mes, ID_TIPO_EXCEL = 1, UPD_USUARIO = ObtenerSesion().ID_USUARIO });
            if (!esValido) msg = "Ocurrió un problema al momento de guardar los datos del excel";


            esNuevo = esNuevo && esValido ? true : !esValido ? false : !esNuevo ? false : true;

            return Json(new { success = esNuevo, mensaje = msg });
        }

        private bool LeerExcel(string filePath, int inicio, int fin, string anio, string mes, int contador, out string msg, out bool continua)
        {
            bool esNuevo = true;
            continua = false;
            msg = "";
            using (Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(stream, false))
                {
                    WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;
                    WorksheetPart worksheetPart = workbookPart.WorksheetParts.First();
                    //WorksheetPart worksheetPart = null;                    
                    int sheetIndex = 0;
                    bool encontrado = false;
                    foreach (WorksheetPart worksheetpartV in workbookPart.WorksheetParts)
                    {
                        string sheetName = workbookPart.Workbook.Descendants<Sheet>().ElementAt(sheetIndex).Name;
                        if (sheetName == "CONOSCE")
                            encontrado = true;
                        sheetIndex++;
                    }

                    if (!encontrado)
                    {
                        esNuevo = false;
                        msg = "La hoja de donde se obtendrán los datos debe llamarse CONOSCE y no se ha encontrado";
                        return esNuevo;
                    }

                    //if (sheetIndex > 1)
                    //{
                    //    esNuevo = false;
                    //    msg = "El archivo excel solo debe contener una hoja con el nombre CONOSCE";
                    //    return esNuevo;
                    //}

                    var stringTable = workbookPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();
                    OpenXmlReader reader = OpenXmlReader.Create(worksheetPart);

                    DataTable dt = new DataTable();
                    dt = ArmarColumnas(dt);
                    while (reader.Read())
                    {
                        if (reader.ElementType == typeof(Row))
                        {
                            Row r = (Row)reader.LoadCurrentElement();
                            if (r.RowIndex == 2)
                            {
                                int cont = 0;
                                foreach (var cell in r.Descendants<Cell>())
                                {
                                    cont++;
                                }
                                if (cont > 18 || cont < 18)
                                {
                                    esNuevo = false;
                                    msg = "El excel no presenta el formato de columnas correcto";
                                    break;
                                }
                            }
                            if (r.RowIndex >= inicio && r.RowIndex <= fin)
                            {
                                if (r.RowIndex > 2)
                                {
                                    var index = 0;
                                    DataRow row = dt.NewRow();
                                    row["ANIO"] = anio;
                                    row["MES"] = mes;
                                    foreach (var cell in r.Descendants<Cell>())
                                    {
                                        string cellValue = string.Empty;
                                        if (cell.DataType != null)
                                        {
                                            if (cell.DataType == CellValues.SharedString)
                                            {
                                                var id = -1;
                                                if (Int32.TryParse(cell.InnerText, out id))
                                                {
                                                    var item = workbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ElementAt(id);
                                                    if (item.Text != null)
                                                        cellValue = item.Text.Text;
                                                    else if (item.InnerText != null)
                                                        cellValue = item.InnerText;
                                                    else if (item.InnerXml != null)
                                                        cellValue = item.InnerXml;
                                                }
                                            }
                                            else
                                                cellValue = cell.InnerText;
                                        }
                                        else
                                            cellValue = cell.InnerText;

                                        switch (index)
                                        {
                                            case 0: row["ENTIDAD"] = cellValue; break;
                                            case 1: row["RUC_ENTIDAD"] = cellValue; break;
                                            case 2: row["FECHA_REGISTRO"] = cellValue; break;
                                            case 3: row["FECHA_DE_EMISION"] = cellValue; break;
                                            case 4: row["FECHA_COMPROMISO_PRESUPUESTAL"] = cellValue; break;
                                            case 5: row["FECHA_DE_NOTIFICACION"] = cellValue; break;
                                            case 6: row["TIPOORDEN"] = cellValue; break;
                                            case 7: row["NRO_DE_ORDEN"] = cellValue; break;
                                            case 8: row["ORDEN"] = cellValue; break;
                                            case 9: row["DESCRIPCION_ORDEN"] = cellValue; break;
                                            case 10: row["MONEDA"] = cellValue; break;
                                            case 11: row["MONTO_TOTAL_ORDEN_ORIGINAL"] = cellValue; break;
                                            case 12: row["OBJETOCONTRACTUAL"] = cellValue; break;
                                            case 13: row["ESTADOCONTRATACION"] = cellValue; break;
                                            case 14: row["TIPODECONTRATACION"] = cellValue; break;
                                            case 15: row["DEPARTAMENTO__ENTIDAD"] = cellValue; break;
                                            case 16: row["RUC_CONTRATISTA"] = cellValue; break;
                                            case 17: row["NOMBRE_RAZON_CONTRATISTA"] = cellValue; break;
                                            default: break;
                                        }
                                        index++;
                                    }

                                    if (contador < 5)
                                    {
                                        if (!(ExcelLN.VerificarOrden(new OrdenCompraBE
                                        {
                                            ANIO = Convert.ToString(row["ANIO"]),
                                            MES = Convert.ToString(row["MES"]),
                                            ORDEN = Convert.ToString(row["ORDEN"])
                                        })))
                                        {
                                            dt.Rows.Add(row);
                                        }
                                        else
                                        {
                                            esNuevo = false;
                                            msg = "El Orden: " + Convert.ToString(row["ORDEN"]) + " del año " + Convert.ToString(row["ANIO"]) + " y mes " + Convert.ToString(row["MES"]) + ", ya se encuentra registrado.";
                                            break;
                                        }
                                        contador++;
                                    }
                                    else
                                    {
                                        dt.Rows.Add(row);
                                    }
                                }
                            }
                            else
                            {
                                if (r.RowIndex > fin)
                                {
                                    continua = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (esNuevo)
                    {
                        int filas = dt.Rows.Count;
                        if (filas == 0)
                        {
                            esNuevo = false;
                            msg = "No se obtuvieron los datos de la hoja excel, por favor verificar si la hoja tiene los datos a cargar";
                        }
                        else
                            ExcelLN.GuardarDatosExcel(dt);
                    }
                    //if (esNuevo)
                    //    ExcelLN.GuardarDatosExcel(dt);
                }
            }
            return esNuevo;
        }

        private DataTable ArmarColumnas(DataTable dt)
        {
            dt.Columns.Add("ANIO");
            dt.Columns.Add("MES");
            dt.Columns.Add("ENTIDAD");
            dt.Columns.Add("RUC_ENTIDAD");
            dt.Columns.Add("FECHA_REGISTRO");
            dt.Columns.Add("FECHA_DE_EMISION");
            dt.Columns.Add("FECHA_COMPROMISO_PRESUPUESTAL");
            dt.Columns.Add("FECHA_DE_NOTIFICACION");
            dt.Columns.Add("TIPOORDEN");
            dt.Columns.Add("NRO_DE_ORDEN");
            dt.Columns.Add("ORDEN");
            dt.Columns.Add("DESCRIPCION_ORDEN");
            dt.Columns.Add("MONEDA");
            dt.Columns.Add("MONTO_TOTAL_ORDEN_ORIGINAL");
            dt.Columns.Add("OBJETOCONTRACTUAL");
            dt.Columns.Add("ESTADOCONTRATACION");
            dt.Columns.Add("TIPODECONTRATACION");
            dt.Columns.Add("DEPARTAMENTO__ENTIDAD");
            dt.Columns.Add("RUC_CONTRATISTA");
            dt.Columns.Add("NOMBRE_RAZON_CONTRATISTA");
            return dt;
        }

        public JsonResult LeerExcelM8U(ExcelViewModel model)
        {
            bool esNuevo = true, continua = false, esValido = true;
            string msg = "";
            string filePath = string.Empty;
            if (model.excel != null)
            {
                string path = Server.MapPath("~/Uploads/");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                filePath = path + Path.GetFileName(model.excel.FileName);
                string extension = Path.GetExtension(model.excel.FileName);
                model.excel.SaveAs(filePath);
            }
            ////
            esNuevo = LeerExcelM8U(filePath, 0, 50000, 0, out msg, out continua);
            if (esNuevo && continua) LeerExcelM8U(filePath, 50000, 100000, 10, out msg, out continua);
            if (esNuevo && continua) LeerExcelM8U(filePath, 100000, 150000, 10, out msg, out continua);
            if (esNuevo && continua) LeerExcelM8U(filePath, 200000, 250000, 10, out msg, out continua);
            if (esNuevo) msg = "Se proceso correctamente el excel";

            if (esNuevo) esValido = ExcelLN.GuardarDatosArchivo(new ExcelBE { NOMBRE = model.excel.FileName, ID_TIPO_EXCEL = 2, UPD_USUARIO = ObtenerSesion().ID_USUARIO });
            if (!esValido) msg = "Ocurrió un problema al momento de guardar los datos del excel";


            esNuevo = esNuevo && esValido ? true : !esValido ? false : !esNuevo ? false : true;

            return Json(new { success = esNuevo, mensaje = msg });
        }

        private bool LeerExcelM8U(string filePath, int inicio, int fin, int contador, out string msg, out bool continua)
        {
            bool esNuevo = true;
            continua = false;
            msg = "";
            using (Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(stream, false))
                {
                    WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;
                    WorksheetPart worksheetPart = workbookPart.WorksheetParts.First();
                    //WorksheetPart worksheetPart = null;
                    int sheetIndex = 0;
                    bool encontrado = false;
                    foreach (WorksheetPart worksheetpartV in workbookPart.WorksheetParts)
                    {
                        string sheetName = workbookPart.Workbook.Descendants<Sheet>().ElementAt(sheetIndex).Name;
                        if (sheetName == "CONOSCE")
                            encontrado = true;
                        sheetIndex++;
                    }

                    if (!encontrado)
                    {
                        esNuevo = false;
                        msg = "La hoja de donde se obtendrán los datos debe llamarse CONOSCE y no se ha encontrado";
                        return esNuevo;
                    }

                    if (sheetIndex > 1)
                    {
                        esNuevo = false;
                        msg = "El archivo excel solo debe contener una hoja con el nombre CONOSCE";
                        return esNuevo;
                    }

                    var stringTable = workbookPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();
                    OpenXmlReader reader = OpenXmlReader.Create(worksheetPart);

                    DataTable dt = new DataTable();
                    dt = ArmarColumnasExcelM8U(dt);
                    while (reader.Read())
                    {
                        if (reader.ElementType == typeof(Row))
                        {
                            //reader.ReadFirstChild();
                            //int index = 1;
                            //do
                            //{
                            //    if (reader.ElementType == typeof(Cell))
                            //    {
                            //        Cell c = (Cell)reader.LoadCurrentElement();

                            //        while (!verificarCelda(c.CellReference, index)) {
                            //            index++;
                            //        }

                            //        string cellValue;

                            //        if (c.DataType != null && c.DataType == CellValues.SharedString)
                            //        {
                            //            SharedStringItem ssi = workbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ElementAt(int.Parse(c.CellValue.InnerText));

                            //            cellValue = ssi.Text.Text;
                            //        }
                            //        else
                            //        {
                            //            cellValue = c.CellValue.InnerText;
                            //        }

                            //        Console.Out.Write("{0}: {1} ", c.CellReference, cellValue);
                            //    }
                            //    index++;
                            //} while (reader.ReadNextSibling());

                            Row r = (Row)reader.LoadCurrentElement();
                            if (r.RowIndex == 1)
                            {
                                int cont = 0;
                                foreach (var cell in r.Descendants<Cell>())
                                {
                                    cont++;
                                }
                                if (cont > 36 || cont < 36)
                                {
                                    esNuevo = false;
                                    msg = "El excel no presenta el formato de columnas correcto";
                                    break;
                                }
                            }
                            if (r.RowIndex >= inicio && r.RowIndex <= fin)
                            {
                                if (r.RowIndex > 1)
                                {
                                    var index = 1;
                                    DataRow row = dt.NewRow();
                                    foreach (var cell in r.Descendants<Cell>())
                                    {
                                        while (!verificarCelda(cell.CellReference, index))
                                        {
                                            index++;
                                        }
                                        string cellValue = string.Empty;
                                        if (cell.DataType != null)
                                        {
                                            if (cell.DataType == CellValues.SharedString)
                                            {
                                                var id = -1;
                                                if (Int32.TryParse(cell.InnerText, out id))
                                                {
                                                    var item = workbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ElementAt(id);
                                                    if (item.Text != null)
                                                        cellValue = item.Text.Text;
                                                    else if (item.InnerText != null)
                                                        cellValue = item.InnerText;
                                                    else if (item.InnerXml != null)
                                                        cellValue = item.InnerXml;
                                                }
                                            }
                                            else
                                                cellValue = cell.InnerText;
                                        }
                                        else
                                            cellValue = cell.InnerText;

                                        switch (index)
                                        {
                                            case 1: row["VERSION"] = cellValue; break;
                                            case 2: row["RUC_ENTIDAD"] = cellValue; break;
                                            case 3: row["ENTIDAD"] = cellValue; break;
                                            case 4: row["MODALIDAD"] = cellValue; break;
                                            case 5: row["PROCESO_SELECCION"] = cellValue; break;
                                            case 6: row["CODIGOCONVOCATORIA"] = cellValue; break;
                                            case 7: row["PROCESO"] = cellValue; break;
                                            case 8: row["FECHA_CONVOCATORIA"] = cellValue; break;
                                            case 9: row["OBJETO_CONTRACTUAL"] = cellValue; break;
                                            case 10: row["DESCRIPCION_PROCESO"] = cellValue; break;
                                            case 11: row["MONEDA"] = cellValue; break;
                                            case 12: row["MONTO_REFERENCIAL_PROCESO"] = cellValue; break;
                                            case 13: row["ITEM"] = cellValue; break;
                                            case 14: row["N_ITEM"] = cellValue; break;
                                            case 15: row["DESCRIPCION_ITEM"] = cellValue; break;
                                            case 16: row["MONTO_REFERENCIAL_ITEM"] = cellValue; break;
                                            case 17: row["MONTO_ADJUDICADO_ITEM"] = cellValue; break;
                                            case 18: row["CANTIDAD_ADJUDICADO_ITEM"] = cellValue; break;
                                            case 19: row["CODIGORUC_PROVEEDOR"] = cellValue; break;
                                            case 20: row["PROVEEDOR"] = cellValue; break;
                                            case 21: row["FECHA_BUENAPRO"] = cellValue; break;
                                            case 22: row["CONSORCIO"] = cellValue; break;
                                            case 23: row["RUC_MIEMBRO"] = cellValue; break;
                                            case 24: row["MIEMBRO"] = cellValue; break;
                                            case 25: row["PAQUETE"] = cellValue; break;
                                            case 26: row["CODIGOGRUPO"] = cellValue; break;
                                            case 27: row["GRUPO"] = cellValue; break;
                                            case 28: row["CODIGOFAMILIA"] = cellValue; break;
                                            case 29: row["FAMILIA"] = cellValue; break;
                                            case 30: row["CODIGOCLASE"] = cellValue; break;
                                            case 31: row["CLASE"] = cellValue; break;
                                            case 32: row["CODIGOCOMMODITY"] = cellValue; break;
                                            case 33: row["COMMODITY"] = cellValue; break;
                                            case 34: row["CODIGO_ITEM"] = cellValue; break;
                                            case 35: row["ITEM_CUBSO"] = cellValue; break;
                                            case 36: row["ESTADO_ITEM"] = cellValue; break;
                                            default: break;
                                        }
                                        index++;
                                    }

                                    if (contador < 10)
                                    {
                                        if (!(ExcelLN.VerificarOrdenM8U(new OrdenCompraM8UBE
                                        {
                                            CODIGOCONVOCATORIA = Convert.ToString(row["CODIGOCONVOCATORIA"]),
                                            MIEMBRO = Convert.ToString(row["MIEMBRO"])
                                        })))
                                        {
                                            dt.Rows.Add(row);
                                        }
                                        else
                                        {
                                            esNuevo = false;
                                            msg = "El registro con código Convocatoria: " + Convert.ToString(row["CODIGOCONVOCATORIA"]) + " y Miembro: " + Convert.ToString(row["MIEMBRO"]) + ", ya se encuentra registrado.";
                                            break;
                                        }
                                        contador++;
                                    }
                                    else
                                    {
                                        dt.Rows.Add(row);
                                    }
                                }
                            }
                            else
                            {
                                if (r.RowIndex > fin)
                                {
                                    continua = true;
                                    break;
                                }
                            }
                        }
                    }
                    
                    if (esNuevo) {
                        int filas = dt.Rows.Count;
                        if (filas == 0)
                        {
                            esNuevo = false;
                            msg = "No se obtuvieron los datos de la hoja excel, por favor verificar si la hoja tiene los datos a cargar";
                        }
                        else
                            ExcelLN.GuardarDatosExcelM8U(dt);
                    }
                        
                }
            }
            return esNuevo;
        }

        public string ObtenerLetra(int num)
        {
            List<string> Letters = new List<string>() { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
            return Letters[num - 1];
        }

        private bool verificarCelda(string celda, int index) {
            //string cadena = "AB8";
            bool validar;
            List<string> Letters = new List<string>() { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
            Regex quitarNumero = new Regex(@"\d");
            celda = quitarNumero.Replace(celda, "");
            int div = index / 26;
            int mod = index % 26;

            if (div == 0)
            {
                string l = Letters[mod - 1];
                if (celda == l)
                    validar = true;
                else
                    validar = false;
            }
            else
            {
                string l1 = mod == 0 ? Letters[26 - 1] : Letters[div - 1];
                string l2 = mod == 0 ? "" : Letters[mod - 1];
                if (celda == l1 + l2)
                    validar = true;
                else
                    validar = false;
            }

            return validar;
        }

        private DataTable ArmarColumnasExcelM8U(DataTable dt)
        {
            dt.Columns.Add("VERSION");
            dt.Columns.Add("RUC_ENTIDAD");
            dt.Columns.Add("ENTIDAD");
            dt.Columns.Add("MODALIDAD");
            dt.Columns.Add("PROCESO_SELECCION");
            dt.Columns.Add("CODIGOCONVOCATORIA");
            dt.Columns.Add("PROCESO");
            dt.Columns.Add("FECHA_CONVOCATORIA");
            dt.Columns.Add("OBJETO_CONTRACTUAL");
            dt.Columns.Add("DESCRIPCION_PROCESO");
            dt.Columns.Add("MONEDA");
            dt.Columns.Add("MONTO_REFERENCIAL_PROCESO");
            dt.Columns.Add("ITEM");
            dt.Columns.Add("N_ITEM");
            dt.Columns.Add("DESCRIPCION_ITEM");
            dt.Columns.Add("MONTO_REFERENCIAL_ITEM");
            dt.Columns.Add("MONTO_ADJUDICADO_ITEM");
            dt.Columns.Add("CANTIDAD_ADJUDICADO_ITEM");
            dt.Columns.Add("CODIGORUC_PROVEEDOR");
            dt.Columns.Add("PROVEEDOR");
            dt.Columns.Add("FECHA_BUENAPRO");
            dt.Columns.Add("CONSORCIO");
            dt.Columns.Add("RUC_MIEMBRO");
            dt.Columns.Add("MIEMBRO");
            dt.Columns.Add("PAQUETE");
            dt.Columns.Add("CODIGOGRUPO");
            dt.Columns.Add("GRUPO");
            dt.Columns.Add("CODIGOFAMILIA");
            dt.Columns.Add("FAMILIA");
            dt.Columns.Add("CODIGOCLASE");
            dt.Columns.Add("CLASE");
            dt.Columns.Add("CODIGOCOMMODITY");
            dt.Columns.Add("COMMODITY");
            dt.Columns.Add("CODIGO_ITEM");
            dt.Columns.Add("ITEM_CUBSO");
            dt.Columns.Add("ESTADO_ITEM");
            return dt;
        }

        public JsonResult ListarExcels(int tipoexcel, int registros, int pagina, string columna, string orden) {
            List<ExcelBE> lista = ExcelLN.ListarExcels(tipoexcel, registros, pagina, columna, orden);

            var jsonResult = Json(new { list = lista }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        //public UsuarioBE ObtenerSesion()
        //{            
        //    return Session["user"] == null ? new UsuarioBE() : (UsuarioBE)Session["user"];
        //}

        string docxMIMEType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        //public ActionResult ExportarExcel_()
        //{
        //    BusquedaBE objBusqueda = new BusquedaBE();
        //    objBusqueda.LISTA_PALABRAS = (List<PalabraClaveBE>)Session["lista_palabras"];
        //    objBusqueda.LISTA_PALABRAS_CANTIDAD = (List<PalabraClaveCantidadBE>)Session["lista_palabras_cantidad"];
        //    objBusqueda.LISTA_ANIOS = (List<AnioBE>)Session["lista_anios"];
        //    int tipoBusqueda = (int)Session["tipo_busqueda"];
        //    List<BusquedaBE> lista = new List<BusquedaBE>();
        //    lista = tipoBusqueda == 1 ? BusquedaLN.FiltrarInformacionExportar(objBusqueda) : BusquedaLN.FiltrarInformacionExportarM8U(objBusqueda);

        //    using (var stream = new MemoryStream())
        //    {
        //        using (var excelDocument = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook, true))
        //        {

        //            var workBookPart = excelDocument.AddWorkbookPart();
        //            workBookPart.Workbook = new Workbook();

        //            var part = workBookPart.AddNewPart<WorksheetPart>();
        //            part.Worksheet = new Worksheet(new SheetData());

        //            var sheets = workBookPart.Workbook.AppendChild(new Sheets());
        //            var sheet = new Sheet()
        //            {
        //                Id = workBookPart.GetIdOfPart(part),
        //                SheetId = 1,
        //                Name = "Resultados"
        //            };

        //            var sheetData = part.Worksheet.Elements<SheetData>().First();

        //            if (tipoBusqueda == 1)
        //                armarExcel(lista, sheetData);
        //            else
        //                armarExcelM8U(lista, sheetData);

        //            sheets.Append(sheet);
        //            workBookPart.Workbook.Save();
        //            excelDocument.Close();
        //        }
        //        return File(stream.ToArray(), docxMIMEType, "Excel Sheet Basic Example.xlsx");
        //    }
        //}

        //private void armarExcel(List<BusquedaBE> lista,  SheetData sheetData) {

        //    AgregarCabecera(sheetData);
        //    foreach (BusquedaBE obj in lista)
        //    {
        //        foreach (OrdenCompraBE oc in obj.LISTA_ORDENCOMPRA)
        //        {
        //            var row = sheetData.AppendChild(new Row());

        //            var entidad = ConstructCell(oc.ENTIDAD, CellValues.String);
        //            row.Append(entidad);
        //            var ruc_entidad = ConstructCell(oc.RUC_ENTIDAD, CellValues.String);
        //            row.Append(ruc_entidad);
        //            var fecha_registro = ConstructCell(oc.FECHA_REGISTRO, CellValues.String);
        //            row.Append(fecha_registro);
        //            var fecha_emision = ConstructCell(oc.FECHA_DE_EMISION, CellValues.String);
        //            row.Append(fecha_emision);
        //            var fecha_compromiso_presupuestal = ConstructCell(oc.FECHA_COMPROMISO_PRESUPUESTAL, CellValues.String);
        //            row.Append(fecha_compromiso_presupuestal);
        //            var fecha_notificacion = ConstructCell(oc.FECHA_DE_NOTIFICACION, CellValues.String);
        //            row.Append(fecha_notificacion);
        //            var tipoorden = ConstructCell(oc.TIPOORDEN, CellValues.String);
        //            row.Append(tipoorden);
        //            var nro_de_orden = ConstructCell(oc.NRO_DE_ORDEN, CellValues.String);
        //            row.Append(nro_de_orden);
        //            var orden = ConstructCell(oc.ORDEN, CellValues.String);
        //            row.Append(orden);
        //            var descripcion_orden = ConstructCell(oc.DESCRIPCION_ORDEN, CellValues.String);
        //            row.Append(descripcion_orden);
        //            var moneda = ConstructCell(oc.MONEDA, CellValues.String);
        //            row.Append(moneda);
        //            var monto_total = ConstructCell(oc.MONTO_TOTAL_ORDEN_ORIGINAL, CellValues.String);
        //            row.Append(monto_total);
        //            var objetocontractual = ConstructCell(oc.OBJETOCONTRACTUAL, CellValues.String);
        //            row.Append(objetocontractual);
        //            var estadocontratacion = ConstructCell(oc.ESTADOCONTRATACION, CellValues.String);
        //            row.Append(estadocontratacion);
        //            var tipocontratacion = ConstructCell(oc.TIPODECONTRATACION, CellValues.String);
        //            row.Append(tipocontratacion);
        //            var departamentoentidad = ConstructCell(oc.DEPARTAMENTO__ENTIDAD, CellValues.String);
        //            row.Append(departamentoentidad);
        //            var ruc_contratista = ConstructCell(oc.RUC_CONTRATISTA, CellValues.String);
        //            row.Append(ruc_contratista);
        //            var nombre_razon_contratista = ConstructCell(oc.NOMBRE_RAZON_CONTRATISTA, CellValues.String);
        //            row.Append(nombre_razon_contratista);
        //        }
        //    }
        //}

        //private void armarExcelM8U(List<BusquedaBE> lista, SheetData sheetData)
        //{
        //    AgregarCabeceraM8U(sheetData);
        //    foreach (BusquedaBE obj in lista)
        //    {
        //        foreach (OrdenCompraM8UBE oc in obj.LISTA_ORDENCOMPRAM8U)
        //        {
        //            var row = sheetData.AppendChild(new Row());

        //            var version = ConstructCell(oc.VERSION == null ? "" : oc.VERSION, CellValues.String);
        //            row.Append(version);
        //            var ruc_entidad = ConstructCell(oc.ENTIDAD_RUC == null ? "" : oc.ENTIDAD_RUC, CellValues.String);
        //            row.Append(ruc_entidad);
        //            var entidad = ConstructCell(oc.ENTIDAD == null ? "" : oc.ENTIDAD, CellValues.String);
        //            row.Append(entidad);
        //            var modalidad = ConstructCell(oc.MODALIDAD == null ? "" : oc.MODALIDAD, CellValues.String);
        //            row.Append(modalidad);
        //            var proceso_seleccion = ConstructCell(oc.PROCESO_SELECCION == null ? "" : oc.PROCESO_SELECCION, CellValues.String);
        //            row.Append(proceso_seleccion);
        //            var codigoconvocatoria = ConstructCell(oc.CODIGOCONVOCATORIA == null ? "" : oc.CODIGOCONVOCATORIA, CellValues.String);
        //            row.Append(codigoconvocatoria);
        //            var proceso = ConstructCell(oc.PROCESO == null ? "" : oc.PROCESO, CellValues.String);
        //            row.Append(proceso);
        //            var fecha_convocatoria = ConstructCell(oc.FECHA_CONVOCATORIA == null ? "" : oc.FECHA_CONVOCATORIA, CellValues.String);
        //            row.Append(fecha_convocatoria);
        //            var objeto_contractual = ConstructCell(oc.OBJETO_CONTRACTUAL == null ? "" : oc.OBJETO_CONTRACTUAL, CellValues.String);
        //            row.Append(objeto_contractual);
        //            var descripcion_proceso = ConstructCell(oc.DESCRIPCION_PROCESO == null ? "" : oc.DESCRIPCION_PROCESO, CellValues.String);
        //            row.Append(descripcion_proceso);
        //            var moneda = ConstructCell(oc.MONEDA == null ? "" : oc.MONEDA, CellValues.String);
        //            row.Append(moneda);
        //            var monto_referencial_proceso = ConstructCell(oc.MONTO_REFERENCIAL_PROCESO == null ? "" : oc.MONTO_REFERENCIAL_PROCESO, CellValues.String);
        //            row.Append(monto_referencial_proceso);
        //            var item = ConstructCell(oc.ITEM == null ? "" : oc.ITEM, CellValues.String);
        //            row.Append(item);
        //            var n_item = ConstructCell(oc.N_ITEM == null ? "" : oc.N_ITEM, CellValues.String);
        //            row.Append(n_item);
        //            var descripcion_item = ConstructCell(oc.DESCRIPCION_ITEM == null ? "" : oc.DESCRIPCION_ITEM, CellValues.String);
        //            row.Append(descripcion_item);
        //            var monto_referencial_item = ConstructCell(oc.MONTO_REFERENCIAL_ITEM == null ? "" : oc.MONTO_REFERENCIAL_ITEM, CellValues.String);
        //            row.Append(monto_referencial_item);
        //            var monto_adjudicado_item = ConstructCell(oc.MONTO_ADJUDICADO_ITEM == null ? "" : oc.MONTO_ADJUDICADO_ITEM, CellValues.String);
        //            row.Append(monto_adjudicado_item);
        //            var cantidad_adjudicado_item = ConstructCell(oc.CANTIDAD_ADJUDICADO_ITEM == null ? "" : oc.CANTIDAD_ADJUDICADO_ITEM, CellValues.String);
        //            row.Append(cantidad_adjudicado_item);
        //            var codigoruc_proveedor = ConstructCell(oc.CODIGORUC_PROVEEDOR == null ? "" : oc.CODIGORUC_PROVEEDOR, CellValues.String);
        //            row.Append(codigoruc_proveedor);
        //            var proveedor = ConstructCell(oc.PROVEEDOR == null ? "" : oc.PROVEEDOR, CellValues.String);
        //            row.Append(proveedor);
        //            var fecha_buenapro = ConstructCell(oc.FECHA_BUENAPRO == null ? "" : oc.FECHA_BUENAPRO, CellValues.String);
        //            row.Append(fecha_buenapro);
        //            var consorcio = ConstructCell(oc.CONSORCIO == null ? "" : oc.CONSORCIO, CellValues.String);
        //            row.Append(consorcio);
        //            var ruc_miembro = ConstructCell(oc.RUC_MIEMBRO == null ? "" : oc.RUC_MIEMBRO, CellValues.String);
        //            row.Append(ruc_miembro);
        //            var miembro = ConstructCell(oc.MIEMBRO == null ? "" : oc.MIEMBRO, CellValues.String);
        //            row.Append(miembro);
        //            var paquete = ConstructCell(oc.PAQUETE == null ? "" : oc.PAQUETE, CellValues.String);
        //            row.Append(paquete);
        //            var codigogrupo = ConstructCell(oc.CODIGOGRUPO == null ? "" : oc.CODIGOGRUPO, CellValues.String);
        //            row.Append(codigogrupo);
        //            var grupo = ConstructCell(oc.GRUPO == null ? "" : oc.GRUPO, CellValues.String);
        //            row.Append(grupo);
        //            var codigofamilia = ConstructCell(oc.CODIGOFAMILIA == null ? "" : oc.CODIGOFAMILIA, CellValues.String);
        //            row.Append(codigofamilia);
        //            var familia = ConstructCell(oc.FAMILIA == null ? "" : oc.FAMILIA, CellValues.String);
        //            row.Append(familia);
        //            var codigoclase = ConstructCell(oc.CODIGOCLASE == null ? "" : oc.CODIGOCLASE, CellValues.String);
        //            row.Append(codigoclase);
        //            var clase = ConstructCell(oc.CLASE == null ? "" : oc.CODIGOCOMMODITY, CellValues.String);
        //            row.Append(clase);
        //            var codigocommodity = ConstructCell(oc.CODIGOCOMMODITY == null ? "" : oc.CODIGOCOMMODITY, CellValues.String);
        //            row.Append(codigocommodity);
        //            var commodity = ConstructCell(oc.COMMODITY == null ? "" : oc.COMMODITY, CellValues.String);
        //            row.Append(commodity);
        //            var codigo_item = ConstructCell(oc.CODIGO_ITEM == null ? "" : oc.CODIGO_ITEM, CellValues.String);
        //            row.Append(codigo_item);
        //            var item_cubso = ConstructCell(oc.ITEM_CUBSO == null ? "" : oc.ITEM_CUBSO, CellValues.String);
        //            row.Append(item_cubso);
        //            var estado_item = ConstructCell(oc.ESTADO_ITEM == null ? "" : oc.ESTADO_ITEM, CellValues.String);
        //            row.Append(estado_item);
        //        }
        //    }
        //}

        //public Cell ConstructCell(string Value, CellValues dataType) => new Cell() {
        //    CellValue = new CellValue(Value),
        //    DataType = new EnumValue<CellValues>(dataType)
        //};

        //private void AgregarCabecera(SheetData sheetData) {
        //    var row = sheetData.AppendChild(new Row());

        //    var entidad = ConstructCell("ENTIDAD", CellValues.String);
        //    row.Append(entidad);
        //    var ruc_entidad = ConstructCell("RUC_ENTIDAD", CellValues.String);
        //    row.Append(ruc_entidad);
        //    var fecha_registro = ConstructCell("FECHA_REGISTRO", CellValues.String);
        //    row.Append(fecha_registro);
        //    var fecha_emision = ConstructCell("FECHA_DE_EMISION", CellValues.String);
        //    row.Append(fecha_emision);
        //    var fecha_compromiso_presupuestal = ConstructCell("FECHA_COMPROMISO_PRESUPUESTAL", CellValues.String);
        //    row.Append(fecha_compromiso_presupuestal);
        //    var fecha_notificacion = ConstructCell("FECHA_NOTIFICACION", CellValues.String);
        //    row.Append(fecha_notificacion);
        //    var tipoorden = ConstructCell("TIPOORDEN", CellValues.String);
        //    row.Append(tipoorden);
        //    var nro_de_orden = ConstructCell("NRO_DE_ORDEN", CellValues.String);
        //    row.Append(nro_de_orden);
        //    var orden = ConstructCell("ORDEN", CellValues.String);
        //    row.Append(orden);
        //    var descripcion_orden = ConstructCell("DESCRIPCION_ORDEN", CellValues.String);
        //    row.Append(descripcion_orden);
        //    var moneda = ConstructCell("MONEDA", CellValues.String);
        //    row.Append(moneda);
        //    var monto_total = ConstructCell("MONTO_TOTAL_ORDEN_ORIGINAL", CellValues.String);
        //    row.Append(monto_total);
        //    var objetocontraactual = ConstructCell("OBJETOCONTRACTUAL", CellValues.String);
        //    row.Append(objetocontraactual);
        //    var estadocontratacion = ConstructCell("ESTADOCONTRATACION", CellValues.String);
        //    row.Append(estadocontratacion);
        //    var tipocontratacion = ConstructCell("TIPODECONTRATACION", CellValues.String);
        //    row.Append(tipocontratacion);
        //    var departamentoentidad = ConstructCell("DEPARTAMENTO__ENTIDAD", CellValues.String);
        //    row.Append(departamentoentidad);
        //    var ruc_contratista = ConstructCell("RUC_CONTRATISTA", CellValues.String);
        //    row.Append(ruc_contratista);
        //    var nombre_razon_contratista = ConstructCell("NOMBRE_RAZON_CONTRATISTA", CellValues.String);
        //    row.Append(nombre_razon_contratista);
        //}

        //private void AgregarCabeceraM8U(SheetData sheetData)
        //{
        //    var row = sheetData.AppendChild(new Row());

        //    var version = ConstructCell("VERSION", CellValues.String);
        //    row.Append(version);
        //    var ruc_entidad = ConstructCell("RUC_ENTIDAD", CellValues.String);
        //    row.Append(ruc_entidad);
        //    var entidad = ConstructCell("ENTIDAD", CellValues.String);
        //    row.Append(entidad);
        //    var modalidad = ConstructCell("MODALIDAD", CellValues.String);
        //    row.Append(modalidad);
        //    var proceso_seleccion = ConstructCell("PROCESO_SELECCION", CellValues.String);
        //    row.Append(proceso_seleccion);
        //    var codigoconvocatoria = ConstructCell("CODIGOCONVOCATORIA", CellValues.String);
        //    row.Append(codigoconvocatoria);
        //    var proceso = ConstructCell("PROCESO", CellValues.String);
        //    row.Append(proceso);
        //    var fecha_convocatoria = ConstructCell("FECHA_CONVOCATORIA", CellValues.String);
        //    row.Append(fecha_convocatoria);
        //    var objeto_contractual = ConstructCell("OBJETO_CONTRACTUAL", CellValues.String);
        //    row.Append(objeto_contractual);
        //    var descripcion_proceso = ConstructCell("DESCRIPCION_PROCESO", CellValues.String);
        //    row.Append(descripcion_proceso);
        //    var moneda = ConstructCell("MONEDA", CellValues.String);
        //    row.Append(moneda);
        //    var monto_referencial_proceso = ConstructCell("MONTO_REFERENCIAL_PROCESO", CellValues.String);
        //    row.Append(monto_referencial_proceso);
        //    var item = ConstructCell("ITEM", CellValues.String);
        //    row.Append(item);
        //    var n_item = ConstructCell("N_ITEM", CellValues.String);
        //    row.Append(n_item);
        //    var descripcion_item = ConstructCell("DESCRIPCION_ITEM", CellValues.String);
        //    row.Append(descripcion_item);
        //    var monto_referencial_item = ConstructCell("MONTO_REFERENCIAL_ITEM", CellValues.String);
        //    row.Append(monto_referencial_item);
        //    var monto_adjudicado_item = ConstructCell("MONTO_ADJUDICADO_ITEM", CellValues.String);
        //    row.Append(monto_adjudicado_item);
        //    var cantidad_adjudicado_item = ConstructCell("CANTIDAD_ADJUDICADO_ITEM", CellValues.String);
        //    row.Append(cantidad_adjudicado_item);
        //    var codigoruc_proveedor = ConstructCell("CODIGORUC_PROVEEDOR", CellValues.String);
        //    row.Append(codigoruc_proveedor);
        //    var proveedor = ConstructCell("PROVEEDOR", CellValues.String);
        //    row.Append(proveedor);
        //    var fecha_buenapro = ConstructCell("FECHA_BUENAPRO", CellValues.String);
        //    row.Append(fecha_buenapro);
        //    var consorcio = ConstructCell("CONSORCIO", CellValues.String);
        //    row.Append(consorcio);
        //    var ruc_miembro = ConstructCell("RUC_MIEMBRO", CellValues.String);
        //    row.Append(ruc_miembro);
        //    var miembro = ConstructCell("MIEMBRO", CellValues.String);
        //    row.Append(miembro);
        //    var paquete = ConstructCell("PAQUETE", CellValues.String);
        //    row.Append(paquete);
        //    var codigogrupo = ConstructCell("CODIGOGRUPO", CellValues.String);
        //    row.Append(codigogrupo);
        //    var grupo = ConstructCell("GRUPO", CellValues.String);
        //    row.Append(grupo);
        //    var codigofamilia = ConstructCell("CODIGOFAMILIA", CellValues.String);
        //    row.Append(codigofamilia);
        //    var familia = ConstructCell("FAMILIA", CellValues.String);
        //    row.Append(familia);
        //    var codigoclase = ConstructCell("CODIGOCLASE", CellValues.String);
        //    row.Append(codigoclase);
        //    var clase = ConstructCell("CLASE", CellValues.String);
        //    row.Append(clase);
        //    var codigocommodity = ConstructCell("CODIGOCOMMODITY", CellValues.String);
        //    row.Append(codigocommodity);
        //    var commodity = ConstructCell("COMMODITY", CellValues.String);
        //    row.Append(commodity);
        //    var codigo_item = ConstructCell("CODIGO_ITEM", CellValues.String);
        //    row.Append(codigo_item);
        //    var item_cubso = ConstructCell("ITEM_CUBSO", CellValues.String);
        //    row.Append(item_cubso);
        //    var estado_item = ConstructCell("ESTADO_ITEM", CellValues.String);
        //    row.Append(estado_item);
        //}

        public enum TipoDato
        {
            Cadena = 1,
            Numero = 2,
            Formula = 3
        }

        public ActionResult ExportarExcel(BusquedaBE objBusqueda)
        {
            //BusquedaBE objBusqueda = new BusquedaBE();
            objBusqueda.LISTA_PALABRAS = (List<PalabraClaveBE>)Session["lista_palabras"];
            objBusqueda.LISTA_PALABRAS_CANTIDAD = (List<PalabraClaveCantidadBE>)Session["lista_palabras_cantidad"];
            objBusqueda.LISTA_ANIOS = (List<AnioBE>)Session["lista_anios"];
            int tipoBusqueda = (int)Session["tipo_busqueda"];
            List<BusquedaBE> lista = new List<BusquedaBE>();
            lista = tipoBusqueda == 1 ? BusquedaLN.FiltrarInformacionExportar(objBusqueda) : BusquedaLN.FiltrarInformacionExportarM8U(objBusqueda);

            var archivoBytes = ExcelLN.ObtenerPlantillaExportar("PlantillaReporte.xlsx");

            using (MemoryStream documentStream = new MemoryStream())
            {
                documentStream.Write(archivoBytes, 0, archivoBytes.Length);
                documentStream.Position = 0;

                PaginaN1(documentStream, lista, tipoBusqueda);
                PaginaN2(documentStream, objBusqueda);

                //using (SpreadsheetDocument spreadSheet = SpreadsheetDocument.Open(documentStream, true))
                //{
                    //Stylesheet stylesheet = spreadSheet.WorkbookPart.WorkbookStylesPart.Stylesheet;
                    //spreadSheet.WorkbookPart.Workbook.CalculationProperties.ForceFullCalculation = true;
                    //spreadSheet.WorkbookPart.Workbook.CalculationProperties.FullCalculationOnLoad = true;

                    //var estilos = ConfigurarEstilos(spreadSheet, stylesheet);
                    ////var totalRows = 0;
                    //var datos = lista; //AQUÍ VA LA DATA

                    //int k = 2;
                    //var sheetName = "Tecnologias";

                    //WorksheetPart worksheetPart = ObtenerHoja(spreadSheet.WorkbookPart, "Reporte", sheetName);

                    ////Definicion de columnas estáticas
                    //DocumentFormat.OpenXml.Spreadsheet.Columns columns1 = new DocumentFormat.OpenXml.Spreadsheet.Columns();
                    //SheetData sd = worksheetPart.Worksheet.Elements<SheetData>().FirstOrDefault();
                    //if ((sd != null))
                    //{
                    //    columns1 = worksheetPart.Worksheet.InsertBefore(new DocumentFormat.OpenXml.Spreadsheet.Columns(), sd);
                    //}
                    //else
                    //{
                    //    columns1 = new DocumentFormat.OpenXml.Spreadsheet.Columns();
                    //    worksheetPart.Worksheet.Append(columns1);
                    //}

                    ////Contenedor de Merge de columnas
                    //MergeCells mergeCells1 = new MergeCells();

                    ////TITULO DEL REPORTE
                    //SetCellValueOperacion(worksheetPart, "A", 1, TipoDato.Cadena, "OSCE - Resultados de búsqueda", string.Empty, estilos.INDICE_TITULO);
                    //mergeCells1.Append(new MergeCell() { Reference = "A1:O1" });

                    //if (tipoBusqueda == 1)
                    //{
                    //    AgregarFormatoColuma(columns1);
                    //    AgregarCabecera(worksheetPart, k, estilos);
                    //    AgregarDatos(worksheetPart, datos, estilos);
                    //}
                    //else
                    //{
                    //    AgregarFormatoColumaM8U(columns1);
                    //    AgregarCabeceraM8U(worksheetPart, k, estilos);
                    //    AgregarDatosM8U(worksheetPart, datos, estilos);
                    //}

                    //PageMargins pageMargins1 = worksheetPart.Worksheet.GetFirstChild<PageMargins>();
                    //worksheetPart.Worksheet.InsertBefore(mergeCells1, pageMargins1);
                    //spreadSheet.Close();
                //}
                //return documentStream.ToArray();
                return File(documentStream.ToArray(), docxMIMEType, "Excel Sheet Basic Example.xlsx");
            }
        }

        private void PaginaN1(MemoryStream documentStream, List<BusquedaBE> lista, int tipoBusqueda)
        {
            using (SpreadsheetDocument spreadSheet = SpreadsheetDocument.Open(documentStream, true))
            {
                Stylesheet stylesheet = spreadSheet.WorkbookPart.WorkbookStylesPart.Stylesheet;
                spreadSheet.WorkbookPart.Workbook.CalculationProperties.ForceFullCalculation = true;
                spreadSheet.WorkbookPart.Workbook.CalculationProperties.FullCalculationOnLoad = true;

                var estilos = ConfigurarEstilos(spreadSheet, stylesheet);
                //var totalRows = 0;
                var datos = lista; //AQUÍ VA LA DATA

                int k = 2;
                var sheetName = "Busqueda";

                WorksheetPart worksheetPart = ObtenerHoja(spreadSheet.WorkbookPart, "Reporte", sheetName);

                //Definicion de columnas estáticas
                DocumentFormat.OpenXml.Spreadsheet.Columns columns1 = new DocumentFormat.OpenXml.Spreadsheet.Columns();
                SheetData sd = worksheetPart.Worksheet.Elements<SheetData>().FirstOrDefault();
                if ((sd != null))
                {
                    columns1 = worksheetPart.Worksheet.InsertBefore(new DocumentFormat.OpenXml.Spreadsheet.Columns(), sd);
                }
                else
                {
                    columns1 = new DocumentFormat.OpenXml.Spreadsheet.Columns();
                    worksheetPart.Worksheet.Append(columns1);
                }

                //Contenedor de Merge de columnas
                MergeCells mergeCells1 = new MergeCells();

                //TITULO DEL REPORTE
                SetCellValueOperacion(worksheetPart, "A", 1, TipoDato.Cadena, "OSCE - Resultados de búsqueda", string.Empty, estilos.INDICE_TITULO);
                mergeCells1.Append(new MergeCell() { Reference = "A1:O1" });

                if (tipoBusqueda == 1)
                {
                    AgregarFormatoColuma(columns1);
                    AgregarCabecera(worksheetPart, k, estilos);
                    AgregarDatos(worksheetPart, datos, estilos);
                }
                else
                {
                    AgregarFormatoColumaM8U(columns1);
                    AgregarCabeceraM8U(worksheetPart, k, estilos);
                    AgregarDatosM8U(worksheetPart, datos, estilos);
                }

                PageMargins pageMargins1 = worksheetPart.Worksheet.GetFirstChild<PageMargins>();
                worksheetPart.Worksheet.InsertBefore(mergeCells1, pageMargins1);
                //worksheetPart.Worksheet.Save();
                spreadSheet.Close();
            }
        }

        private void PaginaN2(MemoryStream documentStream, BusquedaBE obj)
        {
            using (SpreadsheetDocument spreadSheet2 = SpreadsheetDocument.Open(documentStream, true))
            {
                //Stylesheet stylesheet = spreadSheet2.WorkbookPart.WorkbookStylesPart.Stylesheet;
                //spreadSheet2.WorkbookPart.Workbook.CalculationProperties.ForceFullCalculation = true;
                //spreadSheet2.WorkbookPart.Workbook.CalculationProperties.FullCalculationOnLoad = true;

                //var estilos2 = ConfigurarEstilos(spreadSheet2, stylesheet);

                var sheetName = "Resultado";

                WorksheetPart worksheetPart = ObtenerHoja(spreadSheet2.WorkbookPart, "Reporte2", sheetName);

                //Definicion de columnas estáticas
                DocumentFormat.OpenXml.Spreadsheet.Columns columns2 = new DocumentFormat.OpenXml.Spreadsheet.Columns();
                SheetData sd2 = worksheetPart.Worksheet.Elements<SheetData>().FirstOrDefault();
                if ((sd2 != null))
                {
                    columns2 = worksheetPart.Worksheet.InsertBefore(new DocumentFormat.OpenXml.Spreadsheet.Columns(), sd2);
                }
                else
                {
                    columns2 = new DocumentFormat.OpenXml.Spreadsheet.Columns();
                    worksheetPart.Worksheet.Append(columns2);
                }

                //Contenedor de Merge de columnas
                MergeCells mergeCells2 = new MergeCells();

                //TITULO DEL REPORTE
                //SetCellValueOperacion(worksheetPart2, "A", 1, TipoDato.Cadena, "OSCE - Resultados en tablas", string.Empty, estilos2.INDICE_TITULO);
                SetCellValueOperacion(worksheetPart, "A", 1, TipoDato.Cadena, "OSCE - Resultados en tablas", string.Empty);
                mergeCells2.Append(new MergeCell() { Reference = "A1:G1" });

                AgregarFormatoColumaTablas(columns2);
                SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

                int fila = 4;
                tablaBienes(obj.TABLA_BIENES, obj.ARR_ANIOS, worksheetPart, sheetData, fila);
                tablaServicios(obj.TABLA_SERVICIOS, obj.ARR_ANIOS, worksheetPart, sheetData, fila);
                TablaResumen(obj.TABLA_RESUMEN, obj.ARR_ANIOS, worksheetPart, sheetData, fila);
                if (obj.TABLA_ESTIMADO != null) if (obj.TABLA_ESTIMADO.Count > 0) TablaEstimado(obj.TABLA_ESTIMADO, obj.ARR_ANIOS, worksheetPart, sheetData, fila);

                PageMargins pageMargins2 = worksheetPart.Worksheet.GetFirstChild<PageMargins>();
                worksheetPart.Worksheet.InsertBefore(mergeCells2, pageMargins2);
                spreadSheet2.Close();
            }
        }

        private int tablaBienes(List<GraficoN1BE> lista, int[] anios, WorksheetPart worksheetPart, SheetData sheetData, int fila)
        {
            SetCellValueOperacion(worksheetPart, "A", fila-1, TipoDato.Cadena, "RESULTADO BIENES POR AÑOS", string.Empty);
            SetCellValueOperacion(worksheetPart, "A", fila, TipoDato.Cadena, "HALLAZGO", string.Empty);
            SetCellValueOperacion(worksheetPart, "B", fila, TipoDato.Cadena, "BIENES", string.Empty);
            int i;
            for (i = 0; i < anios.Count(); i++)
            {
                SetCellValueOperacion(worksheetPart, ObtenerLetra(i+3), fila, TipoDato.Cadena, anios[i], string.Empty);
            }
            SetCellValueOperacion(worksheetPart, ObtenerLetra(i+3), fila, TipoDato.Cadena, "TOTAL", string.Empty);

            fila++;
            foreach (var item in lista)
            {
                Row newRow = new Row();
                Cell cell = new Cell();
                fila++;

                cell = new Cell();
                cell.DataType = CellValues.String;
                cell.CellValue = new CellValue(item.HALLAZGO);
                newRow.AppendChild(cell);

                cell = new Cell();
                cell.DataType = CellValues.String;
                cell.CellValue = new CellValue(item.TIPO_REQUERIMIENTO);
                newRow.AppendChild(cell);

                for (int j = 0; j < item.ANIOS.Count(); j++)
                {
                    cell = new Cell();
                    cell.DataType = CellValues.Number;
                    cell.CellValue = new CellValue(item.ANIOS[j]);
                    newRow.AppendChild(cell);
                }

                cell = new Cell();
                cell.DataType = CellValues.Number;
                cell.CellValue = new CellValue(item.TOTAL);
                newRow.AppendChild(cell);

                sheetData.AppendChild(newRow);
            }

            return fila;
        }

        private int tablaServicios(List<GraficoN1BE> lista, int[] anios, WorksheetPart worksheetPart, SheetData sheetData, int fila)
        {
            Row newRowVacio = new Row();
            sheetData.AppendChild(newRowVacio);

            Row newRowtitulo = new Row();
            Cell celltitulo = new Cell();

            celltitulo = new Cell();
            celltitulo.DataType = CellValues.String;
            celltitulo.CellValue = new CellValue("RESULTADO SERVICIOS POR AÑOS");
            newRowtitulo.AppendChild(celltitulo);
            sheetData.AppendChild(newRowtitulo);

            Row newRowCabecera= new Row();
            Cell cellCabecera = new Cell();

            cellCabecera = new Cell();
            cellCabecera.DataType = CellValues.String;
            cellCabecera.CellValue = new CellValue("HALLAZGO");
            newRowCabecera.AppendChild(cellCabecera);

            cellCabecera = new Cell();
            cellCabecera.DataType = CellValues.String;
            cellCabecera.CellValue = new CellValue("SERVICIOS");
            newRowCabecera.AppendChild(cellCabecera);

            int i;
            for (i = 0; i < anios.Count(); i++)
            {
                cellCabecera = new Cell();
                cellCabecera.DataType = CellValues.String;
                cellCabecera.CellValue = new CellValue(anios[i]);
                newRowCabecera.AppendChild(cellCabecera);
            }

            cellCabecera = new Cell();
            cellCabecera.DataType = CellValues.String;
            cellCabecera.CellValue = new CellValue("TOTAL");
            newRowCabecera.AppendChild(cellCabecera);

            sheetData.AppendChild(newRowCabecera);

            fila++;
            foreach (var item in lista)
            {
                Row newRow = new Row();
                Cell cell = new Cell();
                fila++;

                cell = new Cell();
                cell.DataType = CellValues.String;
                cell.CellValue = new CellValue(item.HALLAZGO);
                newRow.AppendChild(cell);

                cell = new Cell();
                cell.DataType = CellValues.String;
                cell.CellValue = new CellValue(item.TIPO_REQUERIMIENTO);
                newRow.AppendChild(cell);

                for (int j = 0; j < item.ANIOS.Count(); j++)
                {
                    cell = new Cell();
                    cell.DataType = CellValues.Number;
                    cell.CellValue = new CellValue(item.ANIOS[j]);
                    newRow.AppendChild(cell);
                }

                cell = new Cell();
                cell.DataType = CellValues.Number;
                cell.CellValue = new CellValue(item.TOTAL);
                newRow.AppendChild(cell);

                sheetData.AppendChild(newRow);
            }

            return fila;
        }

        private void TablaResumen(List<GraficoN1BE> lista, int[] anios, WorksheetPart worksheetPart, SheetData sheetData, int fila) {
            Row newRowVacio = new Row();
            sheetData.AppendChild(newRowVacio);

            Row newRowtitulo = new Row();
            Cell celltitulo = new Cell();

            celltitulo = new Cell();
            celltitulo.DataType = CellValues.String;
            celltitulo.CellValue = new CellValue("RESUMEN POR AÑOS");
            newRowtitulo.AppendChild(celltitulo);
            sheetData.AppendChild(newRowtitulo);

            Row newRowCabecera = new Row();
            Cell cellCabecera = new Cell();

            cellCabecera = new Cell();
            cellCabecera.DataType = CellValues.String;
            cellCabecera.CellValue = new CellValue("HALLAZGO");
            newRowCabecera.AppendChild(cellCabecera);

            int i;
            for (i = 0; i < anios.Count(); i++)
            {
                cellCabecera = new Cell();
                cellCabecera.DataType = CellValues.String;
                cellCabecera.CellValue = new CellValue(anios[i]);
                newRowCabecera.AppendChild(cellCabecera);
            }

            cellCabecera = new Cell();
            cellCabecera.DataType = CellValues.String;
            cellCabecera.CellValue = new CellValue("TOTAL");
            newRowCabecera.AppendChild(cellCabecera);

            sheetData.AppendChild(newRowCabecera);

            foreach (var item in lista)
            {
                Row newRow = new Row();
                Cell cell = new Cell();
                fila++;

                cell = new Cell();
                cell.DataType = CellValues.String;
                cell.CellValue = new CellValue(item.HALLAZGO);
                newRow.AppendChild(cell);

                for (int m = 0; m < anios.Count(); m++)
                {
                    decimal suma = 0;
                    for (int j = 0; j < item.ANIOSAR.Count(); j++)
                    {
                        if (item.ANIOSAR[j][0] == anios[m]) suma += item.ANIOSAR[j][1];                        
                    }
                    cell = new Cell();
                    cell.DataType = CellValues.Number;
                    cell.CellValue = new CellValue(suma);
                    newRow.AppendChild(cell);
                }

                cell = new Cell();
                cell.DataType = CellValues.Number;
                cell.CellValue = new CellValue(item.TOTAL);
                newRow.AppendChild(cell);

                sheetData.AppendChild(newRow);
            }
        }

        private void TablaEstimado(List<GraficoN1BE> lista, int[] anios, WorksheetPart worksheetPart, SheetData sheetData, int fila)
        {
            Row newRowVacio = new Row();
            sheetData.AppendChild(newRowVacio);

            Row newRowtitulo = new Row();
            Cell celltitulo = new Cell();

            celltitulo = new Cell();
            celltitulo.DataType = CellValues.String;
            celltitulo.CellValue = new CellValue("RESUMEN CANTIDAD ESTIMADA");
            newRowtitulo.AppendChild(celltitulo);
            sheetData.AppendChild(newRowtitulo);

            Row newRowCabecera = new Row();
            Cell cellCabecera = new Cell();

            cellCabecera = new Cell();
            cellCabecera.DataType = CellValues.String;
            cellCabecera.CellValue = new CellValue("HALLAZGO");
            newRowCabecera.AppendChild(cellCabecera);

            int i;
            for (i = 0; i < anios.Count(); i++)
            {
                cellCabecera = new Cell();
                cellCabecera.DataType = CellValues.String;
                cellCabecera.CellValue = new CellValue(anios[i]);
                newRowCabecera.AppendChild(cellCabecera);
            }

            sheetData.AppendChild(newRowCabecera);

            foreach (var item in lista)
            {
                Row newRow = new Row();
                Cell cell = new Cell();
                fila++;

                cell = new Cell();
                cell.DataType = CellValues.String;
                cell.CellValue = new CellValue(item.HALLAZGO);
                newRow.AppendChild(cell);

                for (int m = 0; m < anios.Count(); m++)
                {
                    decimal suma = 0;
                    for (int j = 0; j < item.ANIOSAR.Count(); j++)
                    {
                        if (item.ANIOSAR[j][0] == anios[m]) suma += item.ANIOSAR[j][1];
                    }
                    cell = new Cell();
                    cell.DataType = CellValues.Number;
                    cell.CellValue = new CellValue(suma);
                    newRow.AppendChild(cell);
                }

                sheetData.AppendChild(newRow);
            }
        }

        private void AgregarFormatoColumaTablas(DocumentFormat.OpenXml.Spreadsheet.Columns columns1)
        {
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 1U, Max = 1U, Width = 15D, CustomWidth = true });//A
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 2U, Max = 2U, Width = 15D, CustomWidth = true });//B
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 3U, Max = 3U, Width = 15D, CustomWidth = true });//C
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 4U, Max = 4U, Width = 15D, CustomWidth = true });//D
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 5U, Max = 5U, Width = 15D, CustomWidth = true });//E
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 6U, Max = 6U, Width = 15D, CustomWidth = true });//F
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 7U, Max = 7U, Width = 15D, CustomWidth = true });//G
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 8U, Max = 8U, Width = 15D, CustomWidth = true });//H
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 9U, Max = 9U, Width = 15D, CustomWidth = true });//I
        }

        private void AgregarFormatoColuma(DocumentFormat.OpenXml.Spreadsheet.Columns columns1)
        {
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 1U, Max = 1U, Width = 65D, CustomWidth = true });//A
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 2U, Max = 2U, Width = 20D, CustomWidth = true });//B
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 3U, Max = 3U, Width = 20D, CustomWidth = true });//C
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 4U, Max = 4U, Width = 20D, CustomWidth = true });//D
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 5U, Max = 5U, Width = 20D, CustomWidth = true });//E
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 6U, Max = 6U, Width = 20D, CustomWidth = true });//F
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 7U, Max = 7U, Width = 25D, CustomWidth = true });//G
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 8U, Max = 8U, Width = 20D, CustomWidth = true });//H
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 9U, Max = 9U, Width = 60D, CustomWidth = true });//I
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 10U, Max = 10U, Width = 70D, CustomWidth = true });//J
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 11U, Max = 11U, Width = 15D, CustomWidth = true });//K
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 12U, Max = 12U, Width = 20D, CustomWidth = true });//L
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 13U, Max = 13U, Width = 20D, CustomWidth = true });//M
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 14U, Max = 14U, Width = 20D, CustomWidth = true });//N
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 15U, Max = 15U, Width = 60D, CustomWidth = true });//O
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 16U, Max = 16U, Width = 25D, CustomWidth = true });//P
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 17U, Max = 17U, Width = 20D, CustomWidth = true });//Q
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 18U, Max = 18U, Width = 40D, CustomWidth = true });//R
        }

        private void AgregarFormatoColumaM8U(DocumentFormat.OpenXml.Spreadsheet.Columns columns1)
        {
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 1U, Max = 1U, Width = 10D, CustomWidth = true });//A
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 2U, Max = 2U, Width = 15D, CustomWidth = true });//B
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 3U, Max = 3U, Width = 65D, CustomWidth = true });//C
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 4U, Max = 4U, Width = 15D, CustomWidth = true });//D
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 5U, Max = 5U, Width = 25D, CustomWidth = true });//E
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 6U, Max = 6U, Width = 30D, CustomWidth = true });//F
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 7U, Max = 7U, Width = 30D, CustomWidth = true });//G
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 8U, Max = 8U, Width = 35D, CustomWidth = true });//H
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 9U, Max = 9U, Width = 60D, CustomWidth = true });//I
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 10U, Max = 10U, Width = 20D, CustomWidth = true });//J
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 11U, Max = 11U, Width = 40D, CustomWidth = true });//K
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 12U, Max = 12U, Width = 15D, CustomWidth = true });//L
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 13U, Max = 13U, Width = 10D, CustomWidth = true });//M
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 14U, Max = 14U, Width = 75D, CustomWidth = true });//N
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 15U, Max = 15U, Width = 40D, CustomWidth = true });//O
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 16U, Max = 16U, Width = 35D, CustomWidth = true });//P
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 17U, Max = 17U, Width = 35D, CustomWidth = true });//Q
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 18U, Max = 18U, Width = 35D, CustomWidth = true });//R
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 19U, Max = 19U, Width = 60D, CustomWidth = true });//S
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 20U, Max = 20U, Width = 25D, CustomWidth = true });//T
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 21U, Max = 21U, Width = 60D, CustomWidth = true });//U
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 22U, Max = 22U, Width = 20D, CustomWidth = true });//V
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 23U, Max = 23U, Width = 50D, CustomWidth = true });//W
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 24U, Max = 24U, Width = 15D, CustomWidth = true });//X
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 25U, Max = 25U, Width = 20D, CustomWidth = true });//Y
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 26U, Max = 26U, Width = 60D, CustomWidth = true });//Z
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 27U, Max = 27U, Width = 25D, CustomWidth = true });//AA
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 28U, Max = 28U, Width = 50D, CustomWidth = true });//AB
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 29U, Max = 29U, Width = 20D, CustomWidth = true });//AC
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 30U, Max = 30U, Width = 40D, CustomWidth = true });//AD
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 31U, Max = 31U, Width = 25D, CustomWidth = true });//AE
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 32U, Max = 32U, Width = 50D, CustomWidth = true });//AF
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 33U, Max = 33U, Width = 20D, CustomWidth = true });//AG
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 34U, Max = 34U, Width = 60D, CustomWidth = true });//AH
            columns1.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 35U, Max = 35U, Width = 15D, CustomWidth = true });//AI
        }

        private void AgregarCabecera(WorksheetPart worksheetPart, int k, EstilosExcel estilos)
        {
            SetCellValueOperacion(worksheetPart, "A", k, TipoDato.Cadena, "ENTIDAD", string.Empty, estilos.INDICE_CABECERA);
            SetCellValueOperacion(worksheetPart, "B", k, TipoDato.Cadena, "RUC_ENTIDAD", string.Empty, estilos.INDICE_CABECERA);
            SetCellValueOperacion(worksheetPart, "C", k, TipoDato.Cadena, "FECHA_REGISTRO", string.Empty, estilos.INDICE_CABECERA);
            SetCellValueOperacion(worksheetPart, "D", k, TipoDato.Cadena, "FECHA_DE_EMISION", string.Empty, estilos.INDICE_CABECERA);
            SetCellValueOperacion(worksheetPart, "E", k, TipoDato.Cadena, "FECHA_COMPROMISO_PRESUPUESTAL", string.Empty, estilos.INDICE_CABECERA);
            SetCellValueOperacion(worksheetPart, "F", k, TipoDato.Cadena, "FECHA_DE_NOTIFICACION", string.Empty, estilos.INDICE_CABECERA);
            SetCellValueOperacion(worksheetPart, "G", k, TipoDato.Cadena, "TIPOORDEN", string.Empty, estilos.INDICE_CABECERA);
            SetCellValueOperacion(worksheetPart, "H", k, TipoDato.Cadena, "NRO_DE_ORDEN", string.Empty, estilos.INDICE_CABECERA);
            SetCellValueOperacion(worksheetPart, "I", k, TipoDato.Cadena, "ORDEN", string.Empty, estilos.INDICE_CABECERA);
            SetCellValueOperacion(worksheetPart, "J", k, TipoDato.Cadena, "DESCRIPCION_ORDEN", string.Empty, estilos.INDICE_CABECERA);
            SetCellValueOperacion(worksheetPart, "K", k, TipoDato.Cadena, "MONEDA", string.Empty, estilos.INDICE_CABECERA);
            SetCellValueOperacion(worksheetPart, "L", k, TipoDato.Cadena, "MONTO_TOTAL_ORDEN_ORIGINAL", string.Empty, estilos.INDICE_CABECERA);
            SetCellValueOperacion(worksheetPart, "M", k, TipoDato.Cadena, "OBJETOCONTRACTUAL", string.Empty, estilos.INDICE_CABECERA);
            SetCellValueOperacion(worksheetPart, "N", k, TipoDato.Cadena, "ESTADOCONTRATACION", string.Empty, estilos.INDICE_CABECERA);
            SetCellValueOperacion(worksheetPart, "O", k, TipoDato.Cadena, "TIPODECONTRATACION", string.Empty, estilos.INDICE_CABECERA);
            SetCellValueOperacion(worksheetPart, "P", k, TipoDato.Cadena, "DEPARTAMENTO__ENTIDAD", string.Empty, estilos.INDICE_CABECERA);
            SetCellValueOperacion(worksheetPart, "Q", k, TipoDato.Cadena, "RUC_CONTRATISTA", string.Empty, estilos.INDICE_CABECERA);
            SetCellValueOperacion(worksheetPart, "R", k, TipoDato.Cadena, "NOMBRE_RAZON_CONTRATISTA", string.Empty, estilos.INDICE_CABECERA);
        }

        private void AgregarCabeceraM8U(WorksheetPart worksheetPart, int k, EstilosExcel estilos)
        {
            SetCellValueOperacion(worksheetPart, "A", k, TipoDato.Cadena, "VERSION", string.Empty, estilos.INDICE_CABECERA);
            SetCellValueOperacion(worksheetPart, "B", k, TipoDato.Cadena, "RUC_ENTIDAD", string.Empty, estilos.INDICE_CABECERA);
            SetCellValueOperacion(worksheetPart, "C", k, TipoDato.Cadena, "ENTIDAD", string.Empty, estilos.INDICE_CABECERA);
            SetCellValueOperacion(worksheetPart, "D", k, TipoDato.Cadena, "MODALIDAD", string.Empty, estilos.INDICE_CABECERA);
            SetCellValueOperacion(worksheetPart, "E", k, TipoDato.Cadena, "PROCESO_SELECCION", string.Empty, estilos.INDICE_CABECERA);
            SetCellValueOperacion(worksheetPart, "F", k, TipoDato.Cadena, "CODIGOCONVOCATORIA", string.Empty, estilos.INDICE_CABECERA);
            SetCellValueOperacion(worksheetPart, "G", k, TipoDato.Cadena, "FECHA_CONVOCATORIA", string.Empty, estilos.INDICE_CABECERA);
            SetCellValueOperacion(worksheetPart, "H", k, TipoDato.Cadena, "OBJETO_CONTRACTUAL", string.Empty, estilos.INDICE_CABECERA);
            SetCellValueOperacion(worksheetPart, "I", k, TipoDato.Cadena, "DESCRIPCION_PROCESO", string.Empty, estilos.INDICE_CABECERA);
            SetCellValueOperacion(worksheetPart, "J", k, TipoDato.Cadena, "MONEDA", string.Empty, estilos.INDICE_CABECERA);
            SetCellValueOperacion(worksheetPart, "K", k, TipoDato.Cadena, "MONTO_REFERENCIAL_PROCESO", string.Empty, estilos.INDICE_CABECERA);
            SetCellValueOperacion(worksheetPart, "L", k, TipoDato.Cadena, "ITEM", string.Empty, estilos.INDICE_CABECERA);
            SetCellValueOperacion(worksheetPart, "M", k, TipoDato.Cadena, "N_ITEM", string.Empty, estilos.INDICE_CABECERA);
            SetCellValueOperacion(worksheetPart, "N", k, TipoDato.Cadena, "DESCRIPCION_ITEM", string.Empty, estilos.INDICE_CABECERA);
            SetCellValueOperacion(worksheetPart, "O", k, TipoDato.Cadena, "MONTO_REFERENCIAL_ITEM", string.Empty, estilos.INDICE_CABECERA);
            SetCellValueOperacion(worksheetPart, "P", k, TipoDato.Cadena, "MONTO_ADJUDICADO_ITEM", string.Empty, estilos.INDICE_CABECERA);
            SetCellValueOperacion(worksheetPart, "Q", k, TipoDato.Cadena, "CANTIDAD_ADJUDICADO_ITEM", string.Empty, estilos.INDICE_CABECERA);
            SetCellValueOperacion(worksheetPart, "R", k, TipoDato.Cadena, "CODIGORUC_PROVEEDOR", string.Empty, estilos.INDICE_CABECERA);
            SetCellValueOperacion(worksheetPart, "S", k, TipoDato.Cadena, "PROVEEDOR", string.Empty, estilos.INDICE_CABECERA);
            SetCellValueOperacion(worksheetPart, "T", k, TipoDato.Cadena, "FECHA_BUENAPRO", string.Empty, estilos.INDICE_CABECERA);
            SetCellValueOperacion(worksheetPart, "U", k, TipoDato.Cadena, "CONSORCIO", string.Empty, estilos.INDICE_CABECERA);
            SetCellValueOperacion(worksheetPart, "V", k, TipoDato.Cadena, "RUC_MIEMBRO", string.Empty, estilos.INDICE_CABECERA);
            SetCellValueOperacion(worksheetPart, "W", k, TipoDato.Cadena, "MIEMBRO", string.Empty, estilos.INDICE_CABECERA);
            SetCellValueOperacion(worksheetPart, "X", k, TipoDato.Cadena, "PAQUETE", string.Empty, estilos.INDICE_CABECERA);
            SetCellValueOperacion(worksheetPart, "Y", k, TipoDato.Cadena, "CODIGOGRUPO", string.Empty, estilos.INDICE_CABECERA);
            SetCellValueOperacion(worksheetPart, "Z", k, TipoDato.Cadena, "GRUPO", string.Empty, estilos.INDICE_CABECERA);
            SetCellValueOperacion(worksheetPart, "AA", k, TipoDato.Cadena, "CODIGOFAMILIA", string.Empty, estilos.INDICE_CABECERA);
            SetCellValueOperacion(worksheetPart, "AB", k, TipoDato.Cadena, "FAMILIA", string.Empty, estilos.INDICE_CABECERA);
            SetCellValueOperacion(worksheetPart, "AC", k, TipoDato.Cadena, "CODIGOCLASE", string.Empty, estilos.INDICE_CABECERA);
            SetCellValueOperacion(worksheetPart, "AD", k, TipoDato.Cadena, "CLASE", string.Empty, estilos.INDICE_CABECERA);
            SetCellValueOperacion(worksheetPart, "AE", k, TipoDato.Cadena, "CODIGOCOMMODITY", string.Empty, estilos.INDICE_CABECERA);
            SetCellValueOperacion(worksheetPart, "AF", k, TipoDato.Cadena, "COMMODITY", string.Empty, estilos.INDICE_CABECERA);
            SetCellValueOperacion(worksheetPart, "AG", k, TipoDato.Cadena, "CODIGO_ITEM", string.Empty, estilos.INDICE_CABECERA);
            SetCellValueOperacion(worksheetPart, "AH", k, TipoDato.Cadena, "ITEM_CUBSO", string.Empty, estilos.INDICE_CABECERA);
            SetCellValueOperacion(worksheetPart, "AI", k, TipoDato.Cadena, "ESTADO_ITEM", string.Empty, estilos.INDICE_CABECERA);
        }

        private void AgregarDatos(WorksheetPart worksheetPart, List<BusquedaBE> datos, EstilosExcel estilos)
        {
            if (worksheetPart != null)
            {
                SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
                foreach (BusquedaBE bq in datos)
                {
                    foreach (var item in bq.LISTA_ORDENCOMPRA)
                    {
                        Row newRow = new Row();
                        Cell cell = new Cell();

                        // A - ENTIDAD
                        cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(item.ENTIDAD);
                        cell.StyleIndex = uint.Parse(estilos.INDICE_DATOS.ToString());
                        newRow.AppendChild(cell);

                        // B - RUC_ENTIDAD
                        cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(item.RUC_ENTIDAD);
                        cell.StyleIndex = uint.Parse(estilos.INDICE_DATOS_CENTER.ToString());
                        newRow.AppendChild(cell);

                        // C - FECHA_REGISTRO
                        cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(item.FECHA_REGISTRO);
                        cell.StyleIndex = uint.Parse(estilos.INDICE_DATOS.ToString());
                        newRow.AppendChild(cell);

                        // D - FECHA_DE_EMISION
                        cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(item.FECHA_DE_EMISION);
                        cell.StyleIndex = uint.Parse(estilos.INDICE_DATOS.ToString());
                        newRow.AppendChild(cell);

                        // E - FECHA_COMPROMISO_PRESUPUESTAL
                        cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(item.FECHA_COMPROMISO_PRESUPUESTAL);
                        cell.StyleIndex = uint.Parse(estilos.INDICE_DATOS.ToString());
                        newRow.AppendChild(cell);

                        // F - FECHA_DE_NOTIFICACION
                        cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(item.FECHA_DE_NOTIFICACION);
                        cell.StyleIndex = uint.Parse(estilos.INDICE_DATOS_CENTER.ToString());
                        newRow.AppendChild(cell);

                        // G - TIPOORDEN
                        cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(item.TIPOORDEN);
                        cell.StyleIndex = uint.Parse(estilos.INDICE_DATOS.ToString());
                        newRow.AppendChild(cell);

                        // H - NRO_DE_ORDEN
                        cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(item.NRO_DE_ORDEN);
                        cell.StyleIndex = uint.Parse(estilos.INDICE_DATOS.ToString());
                        newRow.AppendChild(cell);

                        // I - ORDEN
                        cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(item.ORDEN);
                        cell.StyleIndex = uint.Parse(estilos.INDICE_DATOS.ToString());
                        newRow.AppendChild(cell);

                        // J - DESCRIPCION_ORDEN
                        cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(item.DESCRIPCION_ORDEN);
                        cell.StyleIndex = uint.Parse(estilos.INDICE_DATOS.ToString());
                        newRow.AppendChild(cell);

                        // K - MONEDA
                        cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(item.MONEDA);
                        cell.StyleIndex = uint.Parse(estilos.INDICE_DATOS.ToString());
                        newRow.AppendChild(cell);

                        // L - MONTO_TOTAL_ORDEN_ORIGINAL
                        cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(item.MONTO_TOTAL_ORDEN_ORIGINAL);
                        cell.StyleIndex = uint.Parse(estilos.INDICE_DATOS_CENTER.ToString());
                        newRow.AppendChild(cell);

                        // M - OBJETOCONTRACTUAL
                        cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(item.OBJETOCONTRACTUAL);
                        cell.StyleIndex = uint.Parse(estilos.INDICE_DATOS_CENTER.ToString());
                        newRow.AppendChild(cell);

                        // N - ESTADOCONTRATACION
                        cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(item.ESTADOCONTRATACION);
                        cell.StyleIndex = uint.Parse(estilos.INDICE_DATOS.ToString());
                        newRow.AppendChild(cell);

                        // O - TIPODECONTRATACION
                        cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(item.TIPODECONTRATACION);
                        cell.StyleIndex = uint.Parse(estilos.INDICE_DATOS.ToString());
                        newRow.AppendChild(cell);


                        // P - DEPARTAMENTO__ENTIDAD
                        cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(item.DEPARTAMENTO__ENTIDAD);
                        cell.StyleIndex = uint.Parse(estilos.INDICE_DATOS.ToString());
                        newRow.AppendChild(cell);

                        // Q - RUC_CONTRATISTA
                        cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(item.RUC_CONTRATISTA);
                        cell.StyleIndex = uint.Parse(estilos.INDICE_DATOS_CENTER.ToString());
                        newRow.AppendChild(cell);

                        // R - NOMBRE_RAZON_CONTRATISTA
                        cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(item.NOMBRE_RAZON_CONTRATISTA);
                        cell.StyleIndex = uint.Parse(estilos.INDICE_DATOS_CENTER.ToString());
                        newRow.AppendChild(cell);

                        sheetData.AppendChild(newRow);
                    }
                }
            }
        }

        private void AgregarDatosM8U(WorksheetPart worksheetPart, List<BusquedaBE> datos, EstilosExcel estilos) {
            if (worksheetPart != null)
            {
                SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
                foreach (BusquedaBE bq in datos)
                {
                    foreach (var item in bq.LISTA_ORDENCOMPRAM8U)
                    {
                        Row newRow = new Row();
                        Cell cell = new Cell();

                        // A - VERSION
                        cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(item.VERSION);
                        cell.StyleIndex = uint.Parse(estilos.INDICE_DATOS.ToString());
                        newRow.AppendChild(cell);

                        // B - ENTIDAD_RUC
                        cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(item.ENTIDAD_RUC);
                        cell.StyleIndex = uint.Parse(estilos.INDICE_DATOS_CENTER.ToString());
                        newRow.AppendChild(cell);

                        // C - ENTIDAD
                        cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(item.ENTIDAD);
                        cell.StyleIndex = uint.Parse(estilos.INDICE_DATOS.ToString());
                        newRow.AppendChild(cell);

                        // D - MODALIDAD
                        cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(item.MODALIDAD);
                        cell.StyleIndex = uint.Parse(estilos.INDICE_DATOS.ToString());
                        newRow.AppendChild(cell);

                        // E - PROCESO_SELECCION
                        cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(item.PROCESO_SELECCION);
                        cell.StyleIndex = uint.Parse(estilos.INDICE_DATOS.ToString());
                        newRow.AppendChild(cell);

                        // F - CODIGOCONVOCATORIA
                        cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(item.CODIGOCONVOCATORIA);
                        cell.StyleIndex = uint.Parse(estilos.INDICE_DATOS_CENTER.ToString());
                        newRow.AppendChild(cell);

                        // G - FECHA_CONVOCATORIA
                        cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(item.FECHA_CONVOCATORIA);
                        cell.StyleIndex = uint.Parse(estilos.INDICE_DATOS.ToString());
                        newRow.AppendChild(cell);

                        // H - OBJETO_CONTRACTUAL
                        cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(item.OBJETO_CONTRACTUAL);
                        cell.StyleIndex = uint.Parse(estilos.INDICE_DATOS.ToString());
                        newRow.AppendChild(cell);

                        // I - DESCRIPCION_PROCESO
                        cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(item.DESCRIPCION_PROCESO);
                        cell.StyleIndex = uint.Parse(estilos.INDICE_DATOS.ToString());
                        newRow.AppendChild(cell);

                        // J - MONEDA
                        cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(item.MONEDA);
                        cell.StyleIndex = uint.Parse(estilos.INDICE_DATOS.ToString());
                        newRow.AppendChild(cell);

                        // K - MONTO_REFERENCIAL_PROCESO
                        cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(item.MONTO_REFERENCIAL_PROCESO);
                        cell.StyleIndex = uint.Parse(estilos.INDICE_DATOS.ToString());
                        newRow.AppendChild(cell);

                        // L - ITEM
                        cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(item.ITEM);
                        cell.StyleIndex = uint.Parse(estilos.INDICE_DATOS_CENTER.ToString());
                        newRow.AppendChild(cell);

                        // M - N_ITEM
                        cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(item.N_ITEM);
                        cell.StyleIndex = uint.Parse(estilos.INDICE_DATOS_CENTER.ToString());
                        newRow.AppendChild(cell);

                        // N - DESCRIPCION_ITEM
                        cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(item.DESCRIPCION_ITEM);
                        cell.StyleIndex = uint.Parse(estilos.INDICE_DATOS.ToString());
                        newRow.AppendChild(cell);

                        // O - MONTO_REFERENCIAL_ITEM
                        cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(item.MONTO_REFERENCIAL_ITEM);
                        cell.StyleIndex = uint.Parse(estilos.INDICE_DATOS.ToString());
                        newRow.AppendChild(cell);


                        // P - MONTO_ADJUDICADO_ITEM
                        cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(item.MONTO_ADJUDICADO_ITEM);
                        cell.StyleIndex = uint.Parse(estilos.INDICE_DATOS.ToString());
                        newRow.AppendChild(cell);

                        // Q - CANTIDAD_ADJUDICADO_ITEM
                        cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(item.CANTIDAD_ADJUDICADO_ITEM);
                        cell.StyleIndex = uint.Parse(estilos.INDICE_DATOS_CENTER.ToString());
                        newRow.AppendChild(cell);

                        // R - CODIGORUC_PROVEEDOR
                        cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(item.CODIGORUC_PROVEEDOR);
                        cell.StyleIndex = uint.Parse(estilos.INDICE_DATOS_CENTER.ToString());
                        newRow.AppendChild(cell);

                        // S - PROVEEDOR
                        cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(item.PROVEEDOR);
                        cell.StyleIndex = uint.Parse(estilos.INDICE_DATOS.ToString());
                        newRow.AppendChild(cell);

                        // T - FECHA_BUENAPRO
                        cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(item.FECHA_BUENAPRO);
                        cell.StyleIndex = uint.Parse(estilos.INDICE_DATOS_CENTER.ToString());
                        newRow.AppendChild(cell);

                        // U - CONSORCIO
                        cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(item.CONSORCIO);
                        cell.StyleIndex = uint.Parse(estilos.INDICE_DATOS.ToString());
                        newRow.AppendChild(cell);

                        // V - RUC_MIEMBRO
                        cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(item.RUC_MIEMBRO);
                        cell.StyleIndex = uint.Parse(estilos.INDICE_DATOS_CENTER.ToString());
                        newRow.AppendChild(cell);

                        // W - MIEMBRO
                        cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(item.MIEMBRO);
                        cell.StyleIndex = uint.Parse(estilos.INDICE_DATOS.ToString());
                        newRow.AppendChild(cell);

                        // X - PAQUETE
                        cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(item.PAQUETE);
                        cell.StyleIndex = uint.Parse(estilos.INDICE_DATOS_CENTER.ToString());
                        newRow.AppendChild(cell);

                        // Y - CODIGOGRUPO
                        cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(item.CODIGOGRUPO);
                        cell.StyleIndex = uint.Parse(estilos.INDICE_DATOS_CENTER.ToString());
                        newRow.AppendChild(cell);

                        // Z - GRUPO
                        cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(item.GRUPO);
                        cell.StyleIndex = uint.Parse(estilos.INDICE_DATOS.ToString());
                        newRow.AppendChild(cell);

                        // AA - CODIGOFAMILIA
                        cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(item.CODIGOFAMILIA);
                        cell.StyleIndex = uint.Parse(estilos.INDICE_DATOS_CENTER.ToString());
                        newRow.AppendChild(cell);

                        // AB - FAMILIA
                        cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(item.FAMILIA);
                        cell.StyleIndex = uint.Parse(estilos.INDICE_DATOS.ToString());
                        newRow.AppendChild(cell);

                        // AC - CODIGOCLASE
                        cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(item.CODIGOCLASE);
                        cell.StyleIndex = uint.Parse(estilos.INDICE_DATOS_CENTER.ToString());
                        newRow.AppendChild(cell);


                        // AD - CLASE
                        cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(item.CLASE);
                        cell.StyleIndex = uint.Parse(estilos.INDICE_DATOS.ToString());
                        newRow.AppendChild(cell);

                        // AE - CODIGOCOMMODITY
                        cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(item.CODIGOCOMMODITY);
                        cell.StyleIndex = uint.Parse(estilos.INDICE_DATOS_CENTER.ToString());
                        newRow.AppendChild(cell);

                        // AF - COMMODITY
                        cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(item.COMMODITY);
                        cell.StyleIndex = uint.Parse(estilos.INDICE_DATOS.ToString());
                        newRow.AppendChild(cell);

                        // AG - CODIGO_ITEM
                        cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(item.CODIGO_ITEM);
                        cell.StyleIndex = uint.Parse(estilos.INDICE_DATOS_CENTER.ToString());
                        newRow.AppendChild(cell);

                        // AH - ITEM_CUBSO
                        cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(item.ITEM_CUBSO);
                        cell.StyleIndex = uint.Parse(estilos.INDICE_DATOS.ToString());
                        newRow.AppendChild(cell);

                        // AI - ESTADO_ITEM
                        cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(item.ESTADO_ITEM);
                        cell.StyleIndex = uint.Parse(estilos.INDICE_DATOS.ToString());
                        newRow.AppendChild(cell);


                        sheetData.AppendChild(newRow);
                    }
                }
            }
        }

        public static DocumentFormat.OpenXml.Spreadsheet.Row GetRow(WorksheetPart worksheetPart, int rowIndex)
        {
            return worksheetPart.Worksheet.Descendants<DocumentFormat.OpenXml.Spreadsheet.Row>().Where(r => rowIndex == r.RowIndex).FirstOrDefault();
        }
        public static DocumentFormat.OpenXml.Spreadsheet.Cell GetCell(WorksheetPart worksheetpart, string columnName, int rowIndex, int rowHeigth = 14)
        {
            DocumentFormat.OpenXml.Spreadsheet.Row row = GetRow(worksheetpart, rowIndex);

            if (row == null)
            {
                return InsertCellInWorksheet(columnName, rowIndex, worksheetpart.Worksheet, rowHeigth);
            }
            else
            {
                InsertCellInWorksheet(columnName, rowIndex, worksheetpart.Worksheet, rowHeigth);
                return row.Elements<DocumentFormat.OpenXml.Spreadsheet.Cell>().Where(c => string.Compare(c.CellReference.Value, columnName +
                       rowIndex, true) == 0).FirstOrDefault();
            }
        }
        public static DocumentFormat.OpenXml.Spreadsheet.Cell InsertCellInWorksheet(string columnName, int rowIndex, DocumentFormat.OpenXml.Spreadsheet.Worksheet worksheet, int rowHeight)
        {

            DocumentFormat.OpenXml.Spreadsheet.SheetData sheetData = worksheet.GetFirstChild<DocumentFormat.OpenXml.Spreadsheet.SheetData>();
            string cellReference = columnName + rowIndex;

            // If the worksheet does not contain a row with the specified row index, insert one.
            DocumentFormat.OpenXml.Spreadsheet.Row row;
            if (sheetData.Elements<DocumentFormat.OpenXml.Spreadsheet.Row>().Where(r => r.RowIndex == rowIndex).Count() != 0)
            {
                row = sheetData.Elements<DocumentFormat.OpenXml.Spreadsheet.Row>().Where(r => r.RowIndex == rowIndex).First();
            }
            else
            {
                row = new DocumentFormat.OpenXml.Spreadsheet.Row() { RowIndex = (uint)rowIndex, Height = rowHeight };
                sheetData.Append(row);
            }

            // If there is not a cell with the specified column name, insert one.  
            if (row.Elements<DocumentFormat.OpenXml.Spreadsheet.Cell>().Where(c => c.CellReference.Value == columnName + rowIndex).Count() > 0)
            {
                return row.Elements<DocumentFormat.OpenXml.Spreadsheet.Cell>().Where(c => c.CellReference.Value == cellReference).First();
            }
            else
            {
                // Cells must be in sequential order according to CellReference. Determine where to insert the new cell.
                DocumentFormat.OpenXml.Spreadsheet.Cell refCell = null;
                foreach (DocumentFormat.OpenXml.Spreadsheet.Cell cell in row.Elements<DocumentFormat.OpenXml.Spreadsheet.Cell>())
                {
                    if (string.Compare(cell.CellReference.Value, cellReference, true) > 0)
                    {
                        refCell = cell;
                        break;
                    }
                }

                DocumentFormat.OpenXml.Spreadsheet.Cell newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = cellReference };
                //row.InsertBefore(newCell, refCell);
                row.Append(newCell);


                return newCell;
            }
        }
        public static void SetCellValueOperacion(WorksheetPart ws
            , string columna
            , int fila
            , TipoDato tipo
            , object valor
            , string formula
            , int styleIndex = 0)
        {
            DocumentFormat.OpenXml.Spreadsheet.Cell nuevaCelda = GetCell(ws, columna, fila, 40);

            if (tipo == TipoDato.Cadena)
            {
                nuevaCelda.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.InlineString;
                nuevaCelda.InlineString = new DocumentFormat.OpenXml.Spreadsheet.InlineString() { Text = new DocumentFormat.OpenXml.Spreadsheet.Text((valor == null ? string.Empty : valor.ToString())) };
                nuevaCelda.StyleIndex = uint.Parse(styleIndex.ToString());
            }
            else if (tipo == TipoDato.Numero)
            {
                nuevaCelda.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.Number;
                nuevaCelda.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue((valor == null ? "0" : valor.ToString()));
                nuevaCelda.StyleIndex = uint.Parse(styleIndex.ToString());
            }
            else if (tipo == TipoDato.Formula)
            {
                nuevaCelda.CellFormula = new DocumentFormat.OpenXml.Spreadsheet.CellFormula(formula);
                nuevaCelda.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.Number;
                nuevaCelda.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue((valor == null ? "0" : valor.ToString()));
                nuevaCelda.StyleIndex = uint.Parse(styleIndex.ToString());
            }
        }

        public static WorksheetPart ObtenerHoja(WorkbookPart workbookPart, string sheetName, string newSheetName)
        {
            WorksheetPart worksheetPart = null;
            if (!string.IsNullOrEmpty(sheetName))
            {
                DocumentFormat.OpenXml.Spreadsheet.Sheet ss = workbookPart.Workbook.Descendants<DocumentFormat.OpenXml.Spreadsheet.Sheet>().Where(s => s.Name == sheetName).SingleOrDefault<DocumentFormat.OpenXml.Spreadsheet.Sheet>();
                ss.Name = newSheetName;
                worksheetPart = (WorksheetPart)workbookPart.GetPartById(ss.Id);
            }
            else
            {
                worksheetPart = workbookPart.WorksheetParts.FirstOrDefault();
            }
            return worksheetPart;
        }

        private EstilosExcel ConfigurarEstilos(SpreadsheetDocument spreadSheet, Stylesheet stylesheet)
        {
            var estilos = new EstilosExcel();

            DocumentFormat.OpenXml.Spreadsheet.Fonts fonts = stylesheet.Fonts;

            //Font fontTitulo = (Font)fonts.ToList()[1];
            //FontName fontNameTitulo = new FontName() { Val = "Calibri" };
            //fontTitulo.FontSize = new FontSize() { Val = 18D };
            //fontTitulo.Append(new Bold());
            //fontTitulo.Append(fontNameTitulo);
            //fontTitulo.Append(new Color() { Rgb = "FF000070" });
            //fontTitulo.Append(new Color() { Rgb = "414040" });

            DocumentFormat.OpenXml.Spreadsheet.Font font2 = new DocumentFormat.OpenXml.Spreadsheet.Font();
            DocumentFormat.OpenXml.Spreadsheet.FontSize fontSize2 = new DocumentFormat.OpenXml.Spreadsheet.FontSize() { Val = 10D };
            DocumentFormat.OpenXml.Spreadsheet.Color color2 = new DocumentFormat.OpenXml.Spreadsheet.Color() { Theme = (UInt32Value)1U };
            FontName fontName2 = new FontName() { Val = "Calibri" };
            FontFamilyNumbering fontFamilyNumbering2 = new FontFamilyNumbering() { Val = 2 };
            FontScheme fontScheme2 = new FontScheme() { Val = FontSchemeValues.Minor };

            font2.Append(fontSize2);
            font2.Append(color2);
            font2.Append(fontName2);
            font2.Append(fontFamilyNumbering2);
            font2.Append(fontScheme2);

            DocumentFormat.OpenXml.Spreadsheet.Font font3 = new DocumentFormat.OpenXml.Spreadsheet.Font();
            DocumentFormat.OpenXml.Spreadsheet.FontSize fontSize3 = new DocumentFormat.OpenXml.Spreadsheet.FontSize() { Val = 10D };
            DocumentFormat.OpenXml.Spreadsheet.Color color3 = new DocumentFormat.OpenXml.Spreadsheet.Color() { Theme = (UInt32Value)1U };
            FontName fontName3 = new FontName() { Val = "Calibri" };
            FontFamilyNumbering fontFamilyNumbering3 = new FontFamilyNumbering() { Val = 2 };
            FontScheme fontScheme3 = new FontScheme() { Val = FontSchemeValues.Minor };

            font3.Append(fontSize3);
            font3.Append(color3);
            font3.Append(fontName3);
            font3.Append(fontFamilyNumbering3);
            font3.Append(fontScheme3);

            DocumentFormat.OpenXml.Spreadsheet.Font font4 = new DocumentFormat.OpenXml.Spreadsheet.Font();
            DocumentFormat.OpenXml.Spreadsheet.Bold bold1 = new DocumentFormat.OpenXml.Spreadsheet.Bold();
            DocumentFormat.OpenXml.Spreadsheet.FontSize fontSize4 = new DocumentFormat.OpenXml.Spreadsheet.FontSize() { Val = 10D };
            DocumentFormat.OpenXml.Spreadsheet.Color color4 = new DocumentFormat.OpenXml.Spreadsheet.Color() { Theme = (UInt32Value)0U };
            FontName fontName4 = new FontName() { Val = "Calibri" };
            FontFamilyNumbering fontFamilyNumbering4 = new FontFamilyNumbering() { Val = 2 };
            FontScheme fontScheme4 = new FontScheme() { Val = FontSchemeValues.Minor };

            font4.Append(bold1);
            font4.Append(fontSize4);
            font4.Append(color4);
            font4.Append(fontName4);
            font4.Append(fontFamilyNumbering4);
            font4.Append(fontScheme4);

            DocumentFormat.OpenXml.Spreadsheet.Font font5 = new DocumentFormat.OpenXml.Spreadsheet.Font();
            DocumentFormat.OpenXml.Spreadsheet.Bold bold2 = new DocumentFormat.OpenXml.Spreadsheet.Bold();
            DocumentFormat.OpenXml.Spreadsheet.FontSize fontSize5 = new DocumentFormat.OpenXml.Spreadsheet.FontSize() { Val = 10D };
            DocumentFormat.OpenXml.Spreadsheet.Color color5 = new DocumentFormat.OpenXml.Spreadsheet.Color() { Rgb = "FF000000" };
            FontName fontName5 = new FontName() { Val = "Calibri" };
            FontFamilyNumbering fontFamilyNumbering5 = new FontFamilyNumbering() { Val = 1 };
            FontScheme fontScheme5 = new FontScheme() { Val = FontSchemeValues.Major };

            font5.Append(bold2);
            font5.Append(fontSize5);
            font5.Append(color5);
            font5.Append(fontName5);
            font5.Append(fontFamilyNumbering5);
            font5.Append(fontScheme5);

            DocumentFormat.OpenXml.Spreadsheet.Font font6 = new DocumentFormat.OpenXml.Spreadsheet.Font();
            DocumentFormat.OpenXml.Spreadsheet.Bold bold3 = new DocumentFormat.OpenXml.Spreadsheet.Bold();
            DocumentFormat.OpenXml.Spreadsheet.FontSize fontSize6 = new DocumentFormat.OpenXml.Spreadsheet.FontSize() { Val = 12D };
            DocumentFormat.OpenXml.Spreadsheet.Color color6 = new DocumentFormat.OpenXml.Spreadsheet.Color() { Rgb = "FFFFFFFF" }; //HEX8
            FontName fontName6 = new FontName() { Val = "Calibri" };
            FontFamilyNumbering fontFamilyNumbering6 = new FontFamilyNumbering() { Val = 2 };
            FontScheme fontScheme6 = new FontScheme() { Val = FontSchemeValues.Minor };

            font6.Append(bold3);
            font6.Append(fontSize6);
            font6.Append(color6);
            font6.Append(fontName6);
            font6.Append(fontFamilyNumbering6);
            font6.Append(fontScheme6);

            DocumentFormat.OpenXml.Spreadsheet.Font font7 = new DocumentFormat.OpenXml.Spreadsheet.Font();
            DocumentFormat.OpenXml.Spreadsheet.Bold bold4 = new DocumentFormat.OpenXml.Spreadsheet.Bold();
            DocumentFormat.OpenXml.Spreadsheet.FontSize fontSize7 = new DocumentFormat.OpenXml.Spreadsheet.FontSize() { Val = 14D };
            DocumentFormat.OpenXml.Spreadsheet.Color color7 = new DocumentFormat.OpenXml.Spreadsheet.Color() { Rgb = "FF0000FF" }; //HEX8
            FontName fontName7 = new FontName() { Val = "Calibri" };
            FontFamilyNumbering fontFamilyNumbering7 = new FontFamilyNumbering() { Val = 2 };
            FontScheme fontScheme7 = new FontScheme() { Val = FontSchemeValues.Minor };

            font7.Append(bold4);
            font7.Append(fontSize7);
            font7.Append(color7);
            font7.Append(fontName7);
            font7.Append(fontFamilyNumbering7);
            font7.Append(fontScheme7);

            fonts.Append(font2); //1
            fonts.Append(font3); //2
            fonts.Append(font4); //3
            fonts.Append(font5); //4
            fonts.Append(font6); //5
            fonts.Append(font7); //6

            Fills fills1 = stylesheet.Fills;

            Fill fill3 = new Fill();
            Fill fill5 = new Fill();

            //ESTILO COLOR DEL TITULO
            Fill fill4 = new Fill();
            PatternFill patternFill4 = new PatternFill() { PatternType = PatternValues.Solid };
            ForegroundColor foregroundColor2 = new ForegroundColor() { Rgb = new HexBinaryValue() { Value = "F7A033" } };
            BackgroundColor backgroundColor2 = new BackgroundColor() { Rgb = new HexBinaryValue() { Value = "F7A033" } };

            patternFill4.Append(foregroundColor2);
            patternFill4.Append(backgroundColor2);

            fill4.Append(patternFill4);

            Fill fill6 = new Fill();
            PatternFill patternFill6 = new PatternFill() { PatternType = PatternValues.Solid };
            ForegroundColor foregroundColor6 = new ForegroundColor() { Rgb = new HexBinaryValue() { Value = "F68B05" } };
            BackgroundColor backgroundColor6 = new BackgroundColor() { Rgb = new HexBinaryValue() { Value = "F68B05" } };
            patternFill6.Append(foregroundColor6);
            patternFill6.Append(backgroundColor6);

            fill6.Append(patternFill6);

            fills1.Append(fill3); //1
            fills1.Append(fill4); //2
            fills1.Append(fill5); //3
            fills1.Append(fill6); //5

            Borders borders1 = stylesheet.Borders;

            DocumentFormat.OpenXml.Spreadsheet.Border border2 = new DocumentFormat.OpenXml.Spreadsheet.Border();

            DocumentFormat.OpenXml.Spreadsheet.LeftBorder leftBorder2 = new DocumentFormat.OpenXml.Spreadsheet.LeftBorder() { Style = BorderStyleValues.Thin };
            DocumentFormat.OpenXml.Spreadsheet.Color color1 = new DocumentFormat.OpenXml.Spreadsheet.Color() { Auto = true };

            leftBorder2.Append(color1);

            DocumentFormat.OpenXml.Spreadsheet.RightBorder rightBorder2 = new DocumentFormat.OpenXml.Spreadsheet.RightBorder() { Style = BorderStyleValues.Thin };
            DocumentFormat.OpenXml.Spreadsheet.Color colorA = new DocumentFormat.OpenXml.Spreadsheet.Color() { Auto = true };

            rightBorder2.Append(colorA);

            DocumentFormat.OpenXml.Spreadsheet.TopBorder topBorder2 = new DocumentFormat.OpenXml.Spreadsheet.TopBorder() { Style = BorderStyleValues.Thin };
            DocumentFormat.OpenXml.Spreadsheet.Color colorB = new DocumentFormat.OpenXml.Spreadsheet.Color() { Auto = true };

            topBorder2.Append(colorB);

            DocumentFormat.OpenXml.Spreadsheet.BottomBorder bottomBorder2 = new DocumentFormat.OpenXml.Spreadsheet.BottomBorder() { Style = BorderStyleValues.Thin };
            DocumentFormat.OpenXml.Spreadsheet.Color colorC = new DocumentFormat.OpenXml.Spreadsheet.Color() { Auto = true };

            bottomBorder2.Append(colorC);
            DiagonalBorder diagonalBorder2 = new DiagonalBorder();

            border2.Append(leftBorder2);
            border2.Append(rightBorder2);
            border2.Append(topBorder2);
            border2.Append(bottomBorder2);
            border2.Append(diagonalBorder2);


            borders1.Append(border2);


            //Estilo para los datos 
            CellFormat datosFormat = new CellFormat()
            {
                FontId = (UInt32Value)2U,
                FillId = (UInt32Value)0U,
                BorderId = (UInt32Value)1U
            };

            Alignment alCabecera1 = new Alignment() { Horizontal = DocumentFormat.OpenXml.Spreadsheet.HorizontalAlignmentValues.Left, Vertical = DocumentFormat.OpenXml.Spreadsheet.VerticalAlignmentValues.Center, WrapText = true };
            datosFormat.Append(alCabecera1);
            stylesheet.CellFormats.AppendChild(datosFormat);
            estilos.INDICE_DATOS = spreadSheet.WorkbookPart.WorkbookStylesPart.Stylesheet.CellFormats.Count() - 1;

            CellFormat datosFormat2 = new CellFormat()
            {
                FontId = (UInt32Value)2U,
                FillId = (UInt32Value)0U,
                BorderId = (UInt32Value)1U
            };

            Alignment alCabecera2 = new Alignment() { Horizontal = DocumentFormat.OpenXml.Spreadsheet.HorizontalAlignmentValues.Center, Vertical = DocumentFormat.OpenXml.Spreadsheet.VerticalAlignmentValues.Center, WrapText = true };
            datosFormat2.Append(alCabecera2);
            stylesheet.CellFormats.AppendChild(datosFormat2);
            estilos.INDICE_DATOS_CENTER = spreadSheet.WorkbookPart.WorkbookStylesPart.Stylesheet.CellFormats.Count() - 1;


            CellFormat datosFormat3 = new CellFormat()
            {
                FontId = (UInt32Value)2U,
                FillId = (UInt32Value)0U,
                BorderId = (UInt32Value)1U
            };

            Alignment alCabecera3 = new Alignment() { Horizontal = DocumentFormat.OpenXml.Spreadsheet.HorizontalAlignmentValues.Right, Vertical = DocumentFormat.OpenXml.Spreadsheet.VerticalAlignmentValues.Center, WrapText = true };
            datosFormat3.Append(alCabecera3);
            stylesheet.CellFormats.AppendChild(datosFormat3);
            estilos.INDICE_DATOS_RIGHT = spreadSheet.WorkbookPart.WorkbookStylesPart.Stylesheet.CellFormats.Count() - 1;


            //Cabecera
            CellFormat datosFormat4 = new CellFormat()
            {
                FontId = 5U,
                FillId = 5U,// TODO
                BorderId = 1U
            };

            Alignment alCabecera4 = new Alignment() { Horizontal = DocumentFormat.OpenXml.Spreadsheet.HorizontalAlignmentValues.Center, Vertical = DocumentFormat.OpenXml.Spreadsheet.VerticalAlignmentValues.Center, WrapText = true };
            datosFormat4.Append(alCabecera4);
            stylesheet.CellFormats.AppendChild(datosFormat4);
            estilos.INDICE_CABECERA = spreadSheet.WorkbookPart.WorkbookStylesPart.Stylesheet.CellFormats.Count() - 1;

            //Agrupacion
            CellFormat datosFormat5 = new CellFormat()
            {
                FontId = (UInt32Value)3U,
                FillId = (UInt32Value)2U,
                BorderId = (UInt32Value)1U
            };

            Alignment alCabecera5 = new Alignment() { Horizontal = DocumentFormat.OpenXml.Spreadsheet.HorizontalAlignmentValues.Left, Vertical = DocumentFormat.OpenXml.Spreadsheet.VerticalAlignmentValues.Center, WrapText = false };
            datosFormat5.Append(alCabecera5);
            stylesheet.CellFormats.AppendChild(datosFormat5);
            estilos.INDICE_AGRUPACION = spreadSheet.WorkbookPart.WorkbookStylesPart.Stylesheet.CellFormats.Count() - 1;


            //Number format

            DocumentFormat.OpenXml.Spreadsheet.Fonts fonts1 = stylesheet.GetFirstChild<DocumentFormat.OpenXml.Spreadsheet.Fonts>();
            NumberingFormats numberingFormats1 = new NumberingFormats() { Count = (UInt32Value)1U };
            DocumentFormat.OpenXml.Spreadsheet.NumberingFormat numberingFormat1 = new DocumentFormat.OpenXml.Spreadsheet.NumberingFormat() { NumberFormatId = (UInt32Value)166U, FormatCode = "[$S/.-280A]\\ #,##0.00" };
            DocumentFormat.OpenXml.Spreadsheet.NumberingFormat numberingFormat2 = new DocumentFormat.OpenXml.Spreadsheet.NumberingFormat() { NumberFormatId = (UInt32Value)3453U, FormatCode = StringValue.FromString("0%") };

            numberingFormats1.Append(numberingFormat1);
            numberingFormats1.Append(numberingFormat2);
            stylesheet.InsertBefore(numberingFormats1, fonts1);


            CellFormat datosFormat6 = new CellFormat()
            {
                NumberFormatId = (UInt32Value)166U,
                FontId = (UInt32Value)0U,
                FillId = (UInt32Value)0U,
                BorderId = (UInt32Value)1U,
                FormatId = (UInt32Value)0U,
                ApplyNumberFormat = true
            };

            stylesheet.CellFormats.AppendChild(datosFormat6);
            estilos.INDICE_CURRENCY = spreadSheet.WorkbookPart.WorkbookStylesPart.Stylesheet.CellFormats.Count() - 1;

            CellFormat datosFormat7 = new CellFormat()
            {
                NumberFormatId = (UInt32Value)166U,
                FontId = (UInt32Value)2U,
                FillId = (UInt32Value)2U,
                BorderId = (UInt32Value)1U,
                FormatId = (UInt32Value)0U,
                ApplyNumberFormat = true
            };

            stylesheet.CellFormats.AppendChild(datosFormat7);
            estilos.INDICE_CURRENCY_TOTAL = spreadSheet.WorkbookPart.WorkbookStylesPart.Stylesheet.CellFormats.Count() - 1;


            //Agrupacion
            CellFormat datosFormat8 = new CellFormat()
            {
                FontId = (UInt32Value)2U,
                FillId = (UInt32Value)2U,
                BorderId = (UInt32Value)1U
            };

            Alignment alCabecera6 = new Alignment() { Horizontal = DocumentFormat.OpenXml.Spreadsheet.HorizontalAlignmentValues.Right, Vertical = DocumentFormat.OpenXml.Spreadsheet.VerticalAlignmentValues.Center, WrapText = false };
            datosFormat8.Append(alCabecera6);
            stylesheet.CellFormats.AppendChild(datosFormat8);
            estilos.INDICE_TOTAL = spreadSheet.WorkbookPart.WorkbookStylesPart.Stylesheet.CellFormats.Count() - 1;



            CellFormat datosFormat9 = new CellFormat()
            {
                NumberFormatId = (UInt32Value)3453U,
                FontId = (UInt32Value)2U,
                FillId = (UInt32Value)0U,
                BorderId = (UInt32Value)1U,
                FormatId = (UInt32Value)0U,
                ApplyNumberFormat = true
            };

            stylesheet.CellFormats.AppendChild(datosFormat9);
            estilos.INDICE_PERCENT = spreadSheet.WorkbookPart.WorkbookStylesPart.Stylesheet.CellFormats.Count() - 1;


            CellFormat datosFormat10 = new CellFormat()
            {
                FontId = (UInt32Value)6U,
                FillId = (UInt32Value)0U,
                BorderId = (UInt32Value)0U
            };

            Alignment alCabecera7 = new Alignment() { Horizontal = DocumentFormat.OpenXml.Spreadsheet.HorizontalAlignmentValues.Left, Vertical = DocumentFormat.OpenXml.Spreadsheet.VerticalAlignmentValues.Center, WrapText = true };
            datosFormat10.Append(alCabecera7);
            stylesheet.CellFormats.AppendChild(datosFormat10);
            estilos.INDICE_TITULO = spreadSheet.WorkbookPart.WorkbookStylesPart.Stylesheet.CellFormats.Count() - 1;

            CellFormat datosFormat11 = new CellFormat()
            {
                FontId = (UInt32Value)2U,
                FillId = (UInt32Value)4U,
                BorderId = (UInt32Value)1U
            };

            Alignment alCabecera8 = new Alignment() { Horizontal = DocumentFormat.OpenXml.Spreadsheet.HorizontalAlignmentValues.Left, Vertical = DocumentFormat.OpenXml.Spreadsheet.VerticalAlignmentValues.Center, WrapText = true };
            datosFormat11.Append(alCabecera8);
            stylesheet.CellFormats.AppendChild(datosFormat11);
            estilos.INDICE_COLOR = spreadSheet.WorkbookPart.WorkbookStylesPart.Stylesheet.CellFormats.Count() - 1;


            CellFormat datosFormat12 = new CellFormat()
            {
                NumberFormatId = (UInt32Value)3453U,
                FontId = (UInt32Value)2U,
                FillId = (UInt32Value)2U,
                BorderId = (UInt32Value)1U,
                FormatId = (UInt32Value)0U,
                ApplyNumberFormat = true
            };

            stylesheet.CellFormats.AppendChild(datosFormat12);
            estilos.INDICE_PERCENT_TOTAL = spreadSheet.WorkbookPart.WorkbookStylesPart.Stylesheet.CellFormats.Count() - 1;

            CellFormat datosFormat13 = new CellFormat()
            {
                FontId = (UInt32Value)2U,
                FillId = (UInt32Value)2U,
                BorderId = (UInt32Value)1U
            };

            Alignment alCabecera13 = new Alignment() { Horizontal = DocumentFormat.OpenXml.Spreadsheet.HorizontalAlignmentValues.Right, Vertical = DocumentFormat.OpenXml.Spreadsheet.VerticalAlignmentValues.Center, WrapText = true };
            datosFormat13.Append(alCabecera13);
            stylesheet.CellFormats.AppendChild(datosFormat13);
            estilos.INDICE_DATOS_TOTAL = spreadSheet.WorkbookPart.WorkbookStylesPart.Stylesheet.CellFormats.Count() - 1;

            CellFormat datosFormat14 = new CellFormat()
            {
                NumberFormatId = (UInt32Value)3453U,
                FontId = (UInt32Value)2U,
                FillId = (UInt32Value)4U,
                BorderId = (UInt32Value)1U,
                FormatId = (UInt32Value)0U,
                ApplyNumberFormat = true
            };

            Alignment alCabecera14 = new Alignment() { Horizontal = DocumentFormat.OpenXml.Spreadsheet.HorizontalAlignmentValues.Right, Vertical = DocumentFormat.OpenXml.Spreadsheet.VerticalAlignmentValues.Center, WrapText = true };
            datosFormat14.Append(alCabecera14);
            stylesheet.CellFormats.AppendChild(datosFormat14);
            estilos.INDICE_COLOR_TOTAL = spreadSheet.WorkbookPart.WorkbookStylesPart.Stylesheet.CellFormats.Count() - 1;


            //Estilo para el detalle de los montos
            CellFormat detalleFormat = new CellFormat()
            {
                FontId = (UInt32Value)2U,
                FillId = (UInt32Value)0U,
                BorderId = (UInt32Value)1U,
                NumberFormatId = (UInt32Value)4U

            };
            Alignment alDetalle = new Alignment() { Horizontal = DocumentFormat.OpenXml.Spreadsheet.HorizontalAlignmentValues.Right, Vertical = DocumentFormat.OpenXml.Spreadsheet.VerticalAlignmentValues.Center };
            detalleFormat.Append(alDetalle);
            stylesheet.CellFormats.AppendChild(detalleFormat);
            estilos.INDICE_DATOS_DETALLE = spreadSheet.WorkbookPart.WorkbookStylesPart.Stylesheet.CellFormats.Count() - 1;


            return estilos;
        }

        class EstilosExcel
        {
            public int INDICE_DATOS { get; set; }
            public int INDICE_DATOS_CENTER { get; set; }
            public int INDICE_DATOS_RIGHT { get; set; }
            public int INDICE_CABECERA { get; set; }
            public int INDICE_AGRUPACION { get; set; }
            public int INDICE_CURRENCY { get; set; }
            public int INDICE_CURRENCY_TOTAL { get; set; }
            public int INDICE_TOTAL { get; set; }
            public int INDICE_PERCENT { get; set; }
            public int INDICE_PERCENT_TOTAL { get; set; }
            public int INDICE_TITULO { get; set; }
            public int INDICE_COLOR { get; set; }
            public int INDICE_DATOS_TOTAL { get; set; }
            public int INDICE_COLOR_TOTAL { get; set; }
            public int INDICE_DATOS_DETALLE { get; set; }
        }

    }
}