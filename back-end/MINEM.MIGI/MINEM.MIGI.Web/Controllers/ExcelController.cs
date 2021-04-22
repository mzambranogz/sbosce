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
    [Autenticado]
    public class ExcelController : BaseController
    {
        // GET: Excel
        ExcelLN ExcelLN = new ExcelLN();
        BusquedaLN BusquedaLN = new BusquedaLN();
        public ActionResult Index()
        {
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
                    var stringTable = workbookPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();
                    OpenXmlReader reader = OpenXmlReader.Create(worksheetPart);

                    DataTable dt = new DataTable();
                    dt = ArmarColumnas(dt);
                    while (reader.Read())
                    {
                        if (reader.ElementType == typeof(Row))
                        {
                            Row r = (Row)reader.LoadCurrentElement();
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
                        ExcelLN.GuardarDatosExcel(dt);
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

            if (esNuevo) esValido = ExcelLN.GuardarDatosArchivo(new ExcelBE { NOMBRE = model.excel.FileName, ID_TIPO_EXCEL = 2,  UPD_USUARIO = ObtenerSesion().ID_USUARIO });
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
                    if (esNuevo)
                        ExcelLN.GuardarDatosExcelM8U(dt);
                }
            }
            return esNuevo;
        }

        private bool verificarCelda(string celda, int index) {
            //string cadena = "AB8";
            bool validar;
            List<string> Letters = new List<string>() { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
            Regex quitarNumero = new Regex(@"\d");
            celda = quitarNumero.Replace(celda, "");            int div = index / 26;
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
                if (celda == l1+l2)
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

        public JsonResult ListarExcels(int id) {
            List<ExcelBE> lista = ExcelLN.ListarExcels(id);

            var jsonResult = Json(new { list = lista }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        //public UsuarioBE ObtenerSesion()
        //{            
        //    return Session["user"] == null ? new UsuarioBE() : (UsuarioBE)Session["user"];
        //}

        string docxMIMEType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        public ActionResult ExportarExcel()
        {
            BusquedaBE objBusqueda = new BusquedaBE();
            objBusqueda.LISTA_PALABRAS = (List<PalabraClaveBE>)Session["lista_palabras"];
            objBusqueda.LISTA_PALABRAS_CANTIDAD = (List<PalabraClaveCantidadBE>)Session["lista_palabras_cantidad"];
            objBusqueda.LISTA_ANIOS = (List<AnioBE>)Session["lista_anios"];
            int tipoBusqueda = (int)Session["tipo_busqueda"];
            List<BusquedaBE> lista = new List<BusquedaBE>();
            lista = tipoBusqueda == 1 ? BusquedaLN.FiltrarInformacionExportar(objBusqueda) : BusquedaLN.FiltrarInformacionExportarM8U(objBusqueda);

            using (var stream = new MemoryStream())
            {
                using (var excelDocument = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook, true))
                {
                    var workBookPart = excelDocument.AddWorkbookPart();
                    workBookPart.Workbook = new Workbook();

                    var part = workBookPart.AddNewPart<WorksheetPart>();
                    part.Worksheet = new Worksheet(new SheetData());

                    var sheets = workBookPart.Workbook.AppendChild(new Sheets());
                    var sheet = new Sheet()
                    {
                        Id = workBookPart.GetIdOfPart(part),
                        SheetId = 1,
                        Name = "Resultados"
                    };

                    var sheetData = part.Worksheet.Elements<SheetData>().First();

                    if (tipoBusqueda == 1)
                        armarExcel(lista, sheetData);
                    else
                        armarExcelM8U(lista, sheetData);
                    //AgregarCabecera(sheetData);

                    //foreach (BusquedaBE obj in lista)
                    //{
                    //    foreach (OrdenCompraBE oc in obj.LISTA_ORDENCOMPRA)
                    //    {
                    //        var row = sheetData.AppendChild(new Row());

                    //        var entidad = ConstructCell(oc.ENTIDAD, CellValues.String);
                    //        row.Append(entidad);
                    //        var ruc_entidad = ConstructCell(oc.RUC_ENTIDAD, CellValues.String);
                    //        row.Append(ruc_entidad);
                    //        var fecha_registro = ConstructCell(oc.FECHA_REGISTRO, CellValues.String);
                    //        row.Append(fecha_registro);
                    //        var tipoorden = ConstructCell(oc.TIPOORDEN, CellValues.String);
                    //        row.Append(tipoorden);
                    //        var nro_de_orden = ConstructCell(oc.NRO_DE_ORDEN, CellValues.String);
                    //        row.Append(nro_de_orden);
                    //        var orden = ConstructCell(oc.ORDEN, CellValues.String);
                    //        row.Append(orden);
                    //        var descripcion_orden = ConstructCell(oc.DESCRIPCION_ORDEN, CellValues.String);
                    //        row.Append(descripcion_orden);
                    //        var moneda = ConstructCell(oc.MONEDA, CellValues.String);
                    //        row.Append(moneda);
                    //        var ruc_contratista = ConstructCell(oc.RUC_CONTRATISTA, CellValues.String);
                    //        row.Append(ruc_contratista);
                    //        var nombre_razon_contratista = ConstructCell(oc.NOMBRE_RAZON_CONTRATISTA, CellValues.String);
                    //        row.Append(nombre_razon_contratista);
                    //    }
                    //}

                    //Columns columns = AutoSize(sheetData);
                    //sheets.Append(columns);

                    sheets.Append(sheet);
                    workBookPart.Workbook.Save();
                    excelDocument.Close();
                }
                return File(stream.ToArray(), docxMIMEType, "Excel Sheet Basic Example.xlsx");
            }
        }

        private void armarExcel(List<BusquedaBE> lista,  SheetData sheetData) {

            AgregarCabecera(sheetData);
            foreach (BusquedaBE obj in lista)
            {
                foreach (OrdenCompraBE oc in obj.LISTA_ORDENCOMPRA)
                {
                    var row = sheetData.AppendChild(new Row());

                    var entidad = ConstructCell(oc.ENTIDAD, CellValues.String);
                    row.Append(entidad);
                    var ruc_entidad = ConstructCell(oc.RUC_ENTIDAD, CellValues.String);
                    row.Append(ruc_entidad);
                    var fecha_registro = ConstructCell(oc.FECHA_REGISTRO, CellValues.String);
                    row.Append(fecha_registro);
                    var tipoorden = ConstructCell(oc.TIPOORDEN, CellValues.String);
                    row.Append(tipoorden);
                    var nro_de_orden = ConstructCell(oc.NRO_DE_ORDEN, CellValues.String);
                    row.Append(nro_de_orden);
                    var orden = ConstructCell(oc.ORDEN, CellValues.String);
                    row.Append(orden);
                    var descripcion_orden = ConstructCell(oc.DESCRIPCION_ORDEN, CellValues.String);
                    row.Append(descripcion_orden);
                    var moneda = ConstructCell(oc.MONEDA, CellValues.String);
                    row.Append(moneda);
                    var ruc_contratista = ConstructCell(oc.RUC_CONTRATISTA, CellValues.String);
                    row.Append(ruc_contratista);
                    var nombre_razon_contratista = ConstructCell(oc.NOMBRE_RAZON_CONTRATISTA, CellValues.String);
                    row.Append(nombre_razon_contratista);
                }
            }
        }

        private void armarExcelM8U(List<BusquedaBE> lista, SheetData sheetData)
        {
            AgregarCabeceraM8U(sheetData);
            foreach (BusquedaBE obj in lista)
            {
                foreach (OrdenCompraM8UBE oc in obj.LISTA_ORDENCOMPRAM8U)
                {
                    var row = sheetData.AppendChild(new Row());

                    var version = ConstructCell(oc.VERSION == null ? "" : oc.VERSION, CellValues.String);
                    row.Append(version);
                    var ruc_entidad = ConstructCell(oc.ENTIDAD_RUC == null ? "" : oc.ENTIDAD_RUC, CellValues.String);
                    row.Append(ruc_entidad);
                    var entidad = ConstructCell(oc.ENTIDAD == null ? "" : oc.ENTIDAD, CellValues.String);
                    row.Append(entidad);
                    var modalidad = ConstructCell(oc.MODALIDAD == null ? "" : oc.MODALIDAD, CellValues.String);
                    row.Append(modalidad);
                    var proceso_seleccion = ConstructCell(oc.PROCESO_SELECCION == null ? "" : oc.PROCESO_SELECCION, CellValues.String);
                    row.Append(proceso_seleccion);
                    var codigoconvocatoria = ConstructCell(oc.CODIGOCONVOCATORIA == null ? "" : oc.CODIGOCONVOCATORIA, CellValues.String);
                    row.Append(codigoconvocatoria);
                    var proceso = ConstructCell(oc.PROCESO == null ? "" : oc.PROCESO, CellValues.String);
                    row.Append(proceso);
                    var fecha_convocatoria = ConstructCell(oc.FECHA_CONVOCATORIA == null ? "" : oc.FECHA_CONVOCATORIA, CellValues.String);
                    row.Append(fecha_convocatoria);
                    var objeto_contractual = ConstructCell(oc.OBJETO_CONTRACTUAL == null ? "" : oc.OBJETO_CONTRACTUAL, CellValues.String);
                    row.Append(objeto_contractual);
                    var descripcion_proceso = ConstructCell(oc.DESCRIPCION_PROCESO == null ? "" : oc.DESCRIPCION_PROCESO, CellValues.String);
                    row.Append(descripcion_proceso);
                    var moneda = ConstructCell(oc.MONEDA == null ? "" : oc.MONEDA, CellValues.String);
                    row.Append(moneda);
                    var monto_referencial_proceso = ConstructCell(oc.MONTO_REFERENCIAL_PROCESO == null ? "" : oc.MONTO_REFERENCIAL_PROCESO, CellValues.String);
                    row.Append(monto_referencial_proceso);
                    var item = ConstructCell(oc.ITEM == null ? "" : oc.ITEM, CellValues.String);
                    row.Append(item);
                    var n_item = ConstructCell(oc.N_ITEM == null ? "" : oc.N_ITEM, CellValues.String);
                    row.Append(n_item);
                    var descripcion_item = ConstructCell(oc.DESCRIPCION_ITEM == null ? "" : oc.DESCRIPCION_ITEM, CellValues.String);
                    row.Append(descripcion_item);
                    var monto_referencial_item = ConstructCell(oc.MONTO_REFERENCIAL_ITEM == null ? "" : oc.MONTO_REFERENCIAL_ITEM, CellValues.String);
                    row.Append(monto_referencial_item);
                    var monto_adjudicado_item = ConstructCell(oc.MONTO_ADJUDICADO_ITEM == null ? "" : oc.MONTO_ADJUDICADO_ITEM, CellValues.String);
                    row.Append(monto_adjudicado_item);
                    var cantidad_adjudicado_item = ConstructCell(oc.CANTIDAD_ADJUDICADO_ITEM == null ? "" : oc.CANTIDAD_ADJUDICADO_ITEM, CellValues.String);
                    row.Append(cantidad_adjudicado_item);
                    var codigoruc_proveedor = ConstructCell(oc.CODIGORUC_PROVEEDOR == null ? "" : oc.CODIGORUC_PROVEEDOR, CellValues.String);
                    row.Append(codigoruc_proveedor);
                    var proveedor = ConstructCell(oc.PROVEEDOR == null ? "" : oc.PROVEEDOR, CellValues.String);
                    row.Append(proveedor);
                    var fecha_buenapro = ConstructCell(oc.FECHA_BUENAPRO == null ? "" : oc.FECHA_BUENAPRO, CellValues.String);
                    row.Append(fecha_buenapro);
                    var consorcio = ConstructCell(oc.CONSORCIO == null ? "" : oc.CONSORCIO, CellValues.String);
                    row.Append(consorcio);
                    var ruc_miembro = ConstructCell(oc.RUC_MIEMBRO == null ? "" : oc.RUC_MIEMBRO, CellValues.String);
                    row.Append(ruc_miembro);
                    var miembro = ConstructCell(oc.MIEMBRO == null ? "" : oc.MIEMBRO, CellValues.String);
                    row.Append(miembro);
                    var paquete = ConstructCell(oc.PAQUETE == null ? "" : oc.PAQUETE, CellValues.String);
                    row.Append(paquete);
                    var codigogrupo = ConstructCell(oc.CODIGOGRUPO == null ? "" : oc.CODIGOGRUPO, CellValues.String);
                    row.Append(codigogrupo);
                    var grupo = ConstructCell(oc.GRUPO == null ? "" : oc.GRUPO, CellValues.String);
                    row.Append(grupo);
                    var codigofamilia = ConstructCell(oc.CODIGOFAMILIA == null ? "" : oc.CODIGOFAMILIA, CellValues.String);
                    row.Append(codigofamilia);
                    var familia = ConstructCell(oc.FAMILIA == null ? "" : oc.FAMILIA, CellValues.String);
                    row.Append(familia);
                    var codigoclase = ConstructCell(oc.CODIGOCLASE == null ? "" : oc.CODIGOCLASE, CellValues.String);
                    row.Append(codigoclase);
                    var clase = ConstructCell(oc.CLASE == null ? "" : oc.CODIGOCOMMODITY, CellValues.String);
                    row.Append(clase);
                    var codigocommodity = ConstructCell(oc.CODIGOCOMMODITY == null ? "" : oc.CODIGOCOMMODITY, CellValues.String);
                    row.Append(codigocommodity);
                    var commodity = ConstructCell(oc.COMMODITY == null ? "" : oc.COMMODITY, CellValues.String);
                    row.Append(commodity);
                    var codigo_item = ConstructCell(oc.CODIGO_ITEM == null ? "" : oc.CODIGO_ITEM, CellValues.String);
                    row.Append(codigo_item);
                    var item_cubso = ConstructCell(oc.ITEM_CUBSO == null ? "" : oc.ITEM_CUBSO, CellValues.String);
                    row.Append(item_cubso);
                    var estado_item = ConstructCell(oc.ESTADO_ITEM == null ? "" : oc.ESTADO_ITEM, CellValues.String);
                    row.Append(estado_item);
                }
            }
        }

        public Cell ConstructCell(string Value, CellValues dataType) => new Cell() {
            CellValue = new CellValue(Value),
            DataType = new EnumValue<CellValues>(dataType)
        };

        private void AgregarCabecera(SheetData sheetData) {
            var row = sheetData.AppendChild(new Row());

            var entidad = ConstructCell("ENTIDAD", CellValues.String);
            row.Append(entidad);
            var ruc_entidad = ConstructCell("RUC_ENTIDAD", CellValues.String);
            row.Append(ruc_entidad);
            var fecha_registro = ConstructCell("FECHA_REGISTRO", CellValues.String);
            row.Append(fecha_registro);
            var tipoorden = ConstructCell("TIPOORDEN", CellValues.String);
            row.Append(tipoorden);
            var nro_de_orden = ConstructCell("NRO_DE_ORDEN", CellValues.String);
            row.Append(nro_de_orden);
            var orden = ConstructCell("ORDEN", CellValues.String);
            row.Append(orden);
            var descripcion_orden = ConstructCell("DESCRIPCION_ORDEN", CellValues.String);
            row.Append(descripcion_orden);
            var moneda = ConstructCell("MONEDA", CellValues.String);
            row.Append(moneda);
            var ruc_contratista = ConstructCell("RUC_CONTRATISTA", CellValues.String);
            row.Append(ruc_contratista);
            var nombre_razon_contratista = ConstructCell("NOMBRE_RAZON_CONTRATISTA", CellValues.String);
            row.Append(nombre_razon_contratista);
        }

        private void AgregarCabeceraM8U(SheetData sheetData)
        {
            var row = sheetData.AppendChild(new Row());

            var version = ConstructCell("VERSION", CellValues.String);
            row.Append(version);
            var ruc_entidad = ConstructCell("RUC_ENTIDAD", CellValues.String);
            row.Append(ruc_entidad);
            var entidad = ConstructCell("ENTIDAD", CellValues.String);
            row.Append(entidad);
            var modalidad = ConstructCell("MODALIDAD", CellValues.String);
            row.Append(modalidad);
            var proceso_seleccion = ConstructCell("PROCESO_SELECCION", CellValues.String);
            row.Append(proceso_seleccion);
            var codigoconvocatoria = ConstructCell("CODIGOCONVOCATORIA", CellValues.String);
            row.Append(codigoconvocatoria);
            var proceso = ConstructCell("PROCESO", CellValues.String);
            row.Append(proceso);
            var fecha_convocatoria = ConstructCell("FECHA_CONVOCATORIA", CellValues.String);
            row.Append(fecha_convocatoria);
            var objeto_contractual = ConstructCell("OBJETO_CONTRACTUAL", CellValues.String);
            row.Append(objeto_contractual);
            var descripcion_proceso = ConstructCell("DESCRIPCION_PROCESO", CellValues.String);
            row.Append(descripcion_proceso);
            var moneda = ConstructCell("MONEDA", CellValues.String);
            row.Append(moneda);
            var monto_referencial_proceso = ConstructCell("MONTO_REFERENCIAL_PROCESO", CellValues.String);
            row.Append(monto_referencial_proceso);
            var item = ConstructCell("ITEM", CellValues.String);
            row.Append(item);
            var n_item = ConstructCell("N_ITEM", CellValues.String);
            row.Append(n_item);
            var descripcion_item = ConstructCell("DESCRIPCION_ITEM", CellValues.String);
            row.Append(descripcion_item);
            var monto_referencial_item = ConstructCell("MONTO_REFERENCIAL_ITEM", CellValues.String);
            row.Append(monto_referencial_item);
            var monto_adjudicado_item = ConstructCell("MONTO_ADJUDICADO_ITEM", CellValues.String);
            row.Append(monto_adjudicado_item);
            var cantidad_adjudicado_item = ConstructCell("CANTIDAD_ADJUDICADO_ITEM", CellValues.String);
            row.Append(cantidad_adjudicado_item);
            var codigoruc_proveedor = ConstructCell("CODIGORUC_PROVEEDOR", CellValues.String);
            row.Append(codigoruc_proveedor);
            var proveedor = ConstructCell("PROVEEDOR", CellValues.String);
            row.Append(proveedor);
            var fecha_buenapro = ConstructCell("FECHA_BUENAPRO", CellValues.String);
            row.Append(fecha_buenapro);
            var consorcio = ConstructCell("CONSORCIO", CellValues.String);
            row.Append(consorcio);
            var ruc_miembro = ConstructCell("RUC_MIEMBRO", CellValues.String);
            row.Append(ruc_miembro);
            var miembro = ConstructCell("MIEMBRO", CellValues.String);
            row.Append(miembro);
            var paquete = ConstructCell("PAQUETE", CellValues.String);
            row.Append(paquete);
            var codigogrupo = ConstructCell("CODIGOGRUPO", CellValues.String);
            row.Append(codigogrupo);
            var grupo = ConstructCell("GRUPO", CellValues.String);
            row.Append(grupo);
            var codigofamilia = ConstructCell("CODIGOFAMILIA", CellValues.String);
            row.Append(codigofamilia);
            var familia = ConstructCell("FAMILIA", CellValues.String);
            row.Append(familia);
            var codigoclase = ConstructCell("CODIGOCLASE", CellValues.String);
            row.Append(codigoclase);
            var clase = ConstructCell("CLASE", CellValues.String);
            row.Append(clase);
            var codigocommodity = ConstructCell("CODIGOCOMMODITY", CellValues.String);
            row.Append(codigocommodity);
            var commodity = ConstructCell("COMMODITY", CellValues.String);
            row.Append(commodity);
            var codigo_item = ConstructCell("CODIGO_ITEM", CellValues.String);
            row.Append(codigo_item);
            var item_cubso = ConstructCell("ITEM_CUBSO", CellValues.String);
            row.Append(item_cubso);
            var estado_item = ConstructCell("ESTADO_ITEM", CellValues.String);
            row.Append(estado_item);
        }

        private Columns AutoSize(SheetData sheetData)
        {
            var maxColWidth = GetMaxCharacterWidth(sheetData);

            Columns columns = new Columns();
            //this is the width of my font - yours may be different
            double maxWidth = 7;
            foreach (var item in maxColWidth)
            {
                //width = Truncate([{Number of Characters} * {Maximum Digit Width} + {5 pixel padding}]/{Maximum Digit Width}*256)/256
                double width = Math.Truncate((item.Value * maxWidth + 5) / maxWidth * 256) / 256;

                //pixels=Truncate(((256 * {width} + Truncate(128/{Maximum Digit Width}))/256)*{Maximum Digit Width})
                double pixels = Math.Truncate(((256 * width + Math.Truncate(128 / maxWidth)) / 256) * maxWidth);

                //character width=Truncate(({pixels}-5)/{Maximum Digit Width} * 100+0.5)/100
                double charWidth = Math.Truncate((pixels - 5) / maxWidth * 100 + 0.5) / 100;

                Column col = new Column() { BestFit = true, Min = (UInt32)(item.Key + 1), Max = (UInt32)(item.Key + 1), CustomWidth = true, Width = (DoubleValue)width };
                columns.Append(col);
            }

            return columns;
        }

        private Dictionary<int, int> GetMaxCharacterWidth(SheetData sheetData)
        {
            //iterate over all cells getting a max char value for each column
            Dictionary<int, int> maxColWidth = new Dictionary<int, int>();
            var rows = sheetData.Elements<Row>();
            UInt32[] numberStyles = new UInt32[] { 5, 6, 7, 8 }; //styles that will add extra chars
            UInt32[] boldStyles = new UInt32[] { 1, 2, 3, 4, 6, 7, 8 }; //styles that will bold
            foreach (var r in rows)
            {
                var cells = r.Elements<Cell>().ToArray();

                //using cell index as my column
                for (int i = 0; i < cells.Length; i++)
                {
                    var cell = cells[i];
                    var cellValue = cell.CellValue == null ? string.Empty : cell.CellValue.InnerText;
                    var cellTextLength = cellValue.Length;

                    if (cell.StyleIndex != null && numberStyles.Contains(cell.StyleIndex))
                    {
                        int thousandCount = (int)Math.Truncate((double)cellTextLength / 4);

                        //add 3 for '.00' 
                        cellTextLength += (3 + thousandCount);
                    }

                    if (cell.StyleIndex != null && boldStyles.Contains(cell.StyleIndex))
                    {
                        //add an extra char for bold - not 100% acurate but good enough for what i need.
                        cellTextLength += 1;
                    }

                    if (maxColWidth.ContainsKey(i))
                    {
                        var current = maxColWidth[i];
                        if (cellTextLength > current)
                        {
                            maxColWidth[i] = cellTextLength;
                        }
                    }
                    else
                    {
                        maxColWidth.Add(i, cellTextLength);
                    }
                }
            }

            return maxColWidth;
        }
    }
}