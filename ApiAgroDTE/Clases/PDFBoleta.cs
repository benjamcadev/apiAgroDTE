using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Font = iTextSharp.text.Font;
using System.Xml;
using Rectangle = iTextSharp.text.Rectangle;
using System.Xml.XPath;
using ApiAgroDTE.Clases;

namespace ApiAgroDTE.Clases
{
    public class PDFBoleta
    {
        public string crearDirectorio()
        {
            //Crear ruta del archivo xml saliente
            string currentDay = DateTime.Now.Day.ToString();
            string currentMonth = DateTime.Now.Month.ToString();
            string currentYear = DateTime.Now.Year.ToString();
            string archxml = "";
            return archxml = verificarDirectorio( currentYear, "M" + currentMonth, "D" + currentDay);
        }

         public string verificarDirectorio(string year, string mes, string dia)
        {

            //VERIFICAR AÑO
            string folderPathYear = @"C:\inetpub\wwwroot\api_agrodte\AgroDTE_Archivos\PDF\" + year;

            if (!Directory.Exists(folderPathYear))
            {
                Directory.CreateDirectory(folderPathYear);
            }
            //VERIFICAR MES
            string folderPathMes = @"C:\inetpub\wwwroot\api_agrodte\AgroDTE_Archivos\PDF\" + year + @"\" + mes;
            if (!Directory.Exists(folderPathMes))
            {
                Directory.CreateDirectory(folderPathMes);
            }

            //VERIFICAR DIA
            string folderPathDia = @"C:\inetpub\wwwroot\api_agrodte\AgroDTE_Archivos\PDF\" + year + @"\" + mes + @"\" + dia;
            if (!Directory.Exists(folderPathDia))
            {
                Directory.CreateDirectory(folderPathDia);
            }


            return folderPathDia;
        }

        public string CrearBoleta(string path_boleta)
        {
            try{                  



                XmlDocument documentoXmlBoleta = new XmlDocument();
                documentoXmlBoleta.Load(path_boleta);

                //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
                // FUENTES --------------------------------------------------------------------------------------------------------------------------------------------
                //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

                iTextSharp.text.Font _fuenteRedBold10 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.BOLD, BaseColor.RED);
                iTextSharp.text.Font _fuenteBlackBold10 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                iTextSharp.text.Font _fuenteBlack9 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 9, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                iTextSharp.text.Font _fuenteBlack7 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 7, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                iTextSharp.text.Font _fuenteBlackBold7 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 7, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                iTextSharp.text.Font _fuenteBlackBold9 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 9, iTextSharp.text.Font.BOLD, BaseColor.BLACK);

                //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
                // ENCABEZADO --------------------------------------------------------------------------------------------------------------------------------------------
                //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

                PdfPTable tablaSubHeader = new PdfPTable(1);

                tablaSubHeader.TotalWidth = 160f;
                tablaSubHeader.LockedWidth = true;

                XmlNodeList rutEmisorList = documentoXmlBoleta.GetElementsByTagName("RutEmisor");
                XmlNodeList TipoDTEList = documentoXmlBoleta.GetElementsByTagName("TipoDTE");
                XmlNodeList FolioList = documentoXmlBoleta.GetElementsByTagName("Folio");
                XmlNodeList dirOrigenList = documentoXmlBoleta.GetElementsByTagName("DirOrigen");//cambiar
                 //XmlNodeList TipoDTEList = documentoXmlBoleta.GetElementsByTagName("TipoDTE");

                // rescatamos el rut y le agregamos separadores de miles 
                string strTipoDTE = TipoDTEList[0].InnerXml;          
                string rut1 = rutEmisorList[0].InnerXml.Substring(0, rutEmisorList[0].InnerXml.Length - 2);
                string rut2 = rutEmisorList[0].InnerXml.Substring(rutEmisorList[0].InnerXml.Length - 2, 2);
                int rut3 = int.Parse(rut1);
                string rut4 = rut3.ToString("N0", new CultureInfo("es-CL"));
                string rutEmisor = rut4 + rut2;

                string Folio = FolioList[0].InnerXml;
                string TipoDTE = TipoDTEList[0].InnerXml;

                if (TipoDTE == "39")
                {
                    TipoDTE = "BOLETA ELECTRÓNICA";
                }
                if (TipoDTE == "41")
                {
                    TipoDTE = " BOLETA EXENTA ELECTRÓNICA";
                }

                string strEncabezadoBoleta = "R.U.T.: " + rutEmisor + " \n \n " + TipoDTE + " \n \n N° " + Folio;

                // LOGO AGROPLASTICO----------------------------------------
                // Creamos la imagen y le ajustamos el tamaño
                iTextSharp.text.Image imagen = iTextSharp.text.Image.GetInstance(@"C:\inetpub\wwwroot\api_agrodte\AgroDTE_Archivos\logofactura.png");
                imagen.BorderWidth = 0;
                imagen.Alignment = Element.ALIGN_RIGHT;
                imagen.Alignment = Element.ALIGN_MIDDLE;
                float percentage = 0.0f;
                percentage = 110 / imagen.Width;
                imagen.ScalePercent(percentage * 100);

                // CREAMOS CELDAS CON FORMATO           
                PdfPCell celdaRojaEncabezado = new PdfPCell(new Phrase(strEncabezadoBoleta, _fuenteRedBold10));
                PdfPCell celdaLS2 = new PdfPCell(new Phrase(" S.I.I. LA SERENA", _fuenteBlackBold9));
                PdfPCell celdaimagen = new PdfPCell(imagen);           
                PdfPCell celdaEmptyEncabezado = new PdfPCell(new Phrase(" "));

                celdaRojaEncabezado.BorderWidth = 1.5f;
                celdaRojaEncabezado.BorderWidthBottom = 1.5f;
                celdaEmptyEncabezado.BorderWidth = 0f;
                celdaLS2.BorderWidth = 0f;
                celdaimagen.BorderWidth = 0f;

                celdaRojaEncabezado.BorderColor = BaseColor.RED;            

                celdaRojaEncabezado.HorizontalAlignment = Element.ALIGN_CENTER;
                celdaRojaEncabezado.VerticalAlignment = Element.ALIGN_MIDDLE;
                celdaLS2.HorizontalAlignment = Element.ALIGN_CENTER;
                celdaLS2.VerticalAlignment = Element.ALIGN_MIDDLE;
                celdaimagen.HorizontalAlignment = Element.ALIGN_CENTER;
                celdaimagen.VerticalAlignment = Element.ALIGN_MIDDLE;


                tablaSubHeader.AddCell(celdaEmptyEncabezado);
                tablaSubHeader.AddCell(celdaRojaEncabezado);
                tablaSubHeader.AddCell(celdaLS2);           
                tablaSubHeader.AddCell(celdaimagen);
                tablaSubHeader.AddCell(celdaEmptyEncabezado);

                //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
                // DATOS EMISOR --------------------------------------------------------------------------------------------------------------------------------------------
                //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

                PdfPTable tablaEmisor = new PdfPTable(2);

                tablaEmisor.TotalWidth = 197f;
                tablaEmisor.LockedWidth = true;

                float[] widthsHeader = new float[] { 10f, 25f};
                tablaEmisor.SetWidths(widthsHeader);

                XmlNodeList DirOrigenList = documentoXmlBoleta.GetElementsByTagName("DirOrigen");
                XmlNodeList CmnaOrigenList = documentoXmlBoleta.GetElementsByTagName("CmnaOrigen");
                XmlNodeList TmstFirmaEnvList = documentoXmlBoleta.GetElementsByTagName("TmstFirmaEnv");

                List<string> fechaList = new List<string>();
                fechaList = TmstFirmaEnvList[0].InnerText.Split('T').ToList();

                string strRazonEmis = "IMPORTADORA COMERCIALIZADORA Y DISTRIBUIDORA AGROPLASTIC \n LTDA";
                string strGiroEmis = "VENTA AL POR MAYOR NO ESPECIALIZADA";
                string strSucursalEmis =": "+ "VICENTE ZORRILLA #835, LA SERENA";
                string strFonoEmis =": "+ "(51) 2222189 - (51) 2221421";

                if (DirOrigenList.Count != 0 && CmnaOrigenList.Count !=0)
                {
                    strSucursalEmis = ": " + DirOrigenList[0].InnerText + ", " + CmnaOrigenList[0].InnerText.ToUpper(); 
                }

                PdfPCell celdaRazonEmis = new PdfPCell(new Phrase(strRazonEmis, _fuenteBlackBold9));
                PdfPCell celdaGiroEmis = new PdfPCell(new Phrase(strGiroEmis, _fuenteBlack9));
                PdfPCell celdaSubSucursalEmis = new PdfPCell(new Phrase("CASA MATRIZ", _fuenteBlackBold7));
                PdfPCell celdaSubFonoEmis = new PdfPCell(new Phrase("FONO", _fuenteBlackBold7));
                PdfPCell celdaSucursalEmis = new PdfPCell(new Phrase(strSucursalEmis, _fuenteBlack7));
                PdfPCell celdaFonoEmis = new PdfPCell(new Phrase(strFonoEmis, _fuenteBlack7));
                PdfPCell celdaSubFechaEmis = new PdfPCell(new Phrase("FECHA", _fuenteBlackBold7));
                PdfPCell celdaSubHoraEmis = new PdfPCell(new Phrase("HORA", _fuenteBlackBold7));
                PdfPCell celdaFechaEmis = new PdfPCell(new Phrase(": " + fechaList[0], _fuenteBlack7));
                PdfPCell celdaHoraEmis = new PdfPCell(new Phrase(": " + fechaList[1], _fuenteBlack7));
                PdfPCell celdaEmptyEmisor = new PdfPCell(new Phrase(" "));

            
                celdaRazonEmis.BorderWidth = 0f;
                celdaGiroEmis.BorderWidth = 0f;
                celdaSubSucursalEmis.BorderWidth = 0f;
                celdaSubFonoEmis.BorderWidth = 0f;
                celdaSucursalEmis.BorderWidth = 0f;
                celdaFonoEmis.BorderWidth = 0f;
                celdaSubFechaEmis.BorderWidth = 0f;
                celdaSubHoraEmis.BorderWidth = 0f;
                celdaFechaEmis.BorderWidth = 0f;
                celdaHoraEmis.BorderWidth = 0f;
                celdaEmptyEmisor.BorderWidth = 0f;

                celdaRazonEmis.HorizontalAlignment = Element.ALIGN_CENTER;
                celdaGiroEmis.HorizontalAlignment = Element.ALIGN_CENTER;
                celdaSubSucursalEmis.HorizontalAlignment = Element.ALIGN_LEFT;
                celdaSubFonoEmis.HorizontalAlignment = Element.ALIGN_LEFT;
                celdaSucursalEmis.HorizontalAlignment = Element.ALIGN_LEFT;
                celdaFonoEmis.HorizontalAlignment = Element.ALIGN_LEFT;
                celdaEmptyEmisor.HorizontalAlignment = Element.ALIGN_LEFT;
                celdaSubFechaEmis.HorizontalAlignment = Element.ALIGN_LEFT;
                celdaSubHoraEmis.HorizontalAlignment = Element.ALIGN_LEFT;
                celdaFechaEmis.HorizontalAlignment = Element.ALIGN_LEFT;
                celdaHoraEmis.HorizontalAlignment = Element.ALIGN_LEFT;

                celdaRazonEmis.Colspan = 2;
                celdaGiroEmis.Colspan = 2;
                celdaEmptyEmisor.Colspan = 2;

                //tablaEmisor.AddCell(celdaEmptyEmisor);
                tablaEmisor.AddCell(celdaRazonEmis);
                tablaEmisor.AddCell(celdaGiroEmis);
                tablaEmisor.AddCell(celdaSubSucursalEmis);
                tablaEmisor.AddCell(celdaSucursalEmis);
                tablaEmisor.AddCell(celdaSubFonoEmis);
                tablaEmisor.AddCell(celdaFonoEmis);
                tablaEmisor.AddCell(celdaEmptyEmisor);
                tablaEmisor.AddCell(celdaSubFechaEmis);
                tablaEmisor.AddCell(celdaFechaEmis);
                tablaEmisor.AddCell(celdaSubHoraEmis);
                tablaEmisor.AddCell(celdaHoraEmis);

            

                //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
                // DETALLE DE LA BOLETA ---------------------------------------------------------------------------------------------------------------------------
                //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

                PdfPTable tablaDetalle = new PdfPTable(3);

                tablaDetalle.TotalWidth = 197f;
                tablaDetalle.LockedWidth = true;

                float[] widthsDetalle = new float[] { 30f, 10f, 10f };
                tablaDetalle.SetWidths(widthsDetalle);

                string tituloDetalle = "DETALLE";
                string subDescripcion = "DESCRIPCION \n CANTIDAD X PRECIO";
                string subValor = "VALOR";
                string subDesc = "DESC.";
                string separador = "----------------------------------------------------------------------------------";

                PdfPCell celdaTituloDetalle = new PdfPCell(new Phrase(tituloDetalle, _fuenteBlackBold7));
                PdfPCell celdaSubDescripcion = new PdfPCell(new Phrase(subDescripcion, _fuenteBlack7));
                PdfPCell celdaSubValor = new PdfPCell(new Phrase(subValor, _fuenteBlack7));
                PdfPCell celdaSubDesc = new PdfPCell(new Phrase(subDesc, _fuenteBlack7));
                PdfPCell celdaSeparador = new PdfPCell(new Phrase(separador, _fuenteBlack7));
                PdfPCell celdaEmptyDetalle = new PdfPCell(new Phrase(" "));

                celdaTituloDetalle.BorderWidth = 0f;
                celdaSubDescripcion.BorderWidth = 0f;
                celdaSubValor.BorderWidth = 0f;
                celdaSeparador.BorderWidth = 0f;
                celdaSubDesc.BorderWidth = 0f;
                celdaEmptyDetalle.BorderWidth = 0f;

                celdaTituloDetalle.HorizontalAlignment = Element.ALIGN_CENTER;
                celdaSubDescripcion.HorizontalAlignment = Element.ALIGN_CENTER;
                celdaSubValor.HorizontalAlignment = Element.ALIGN_RIGHT;
                celdaSubDesc.HorizontalAlignment = Element.ALIGN_RIGHT;
                celdaSubDescripcion.VerticalAlignment = Element.ALIGN_MIDDLE;
                celdaSubValor.VerticalAlignment = Element.ALIGN_MIDDLE;
                celdaSubDesc.VerticalAlignment = Element.ALIGN_MIDDLE;
                celdaSeparador.HorizontalAlignment = Element.ALIGN_LEFT;

                celdaTituloDetalle.Colspan = 3;
                celdaSeparador.Colspan = 3;
            
                tablaDetalle.AddCell(celdaSeparador);
                tablaDetalle.AddCell(celdaTituloDetalle);
                tablaDetalle.AddCell(celdaSeparador);
                tablaDetalle.AddCell(celdaSubDescripcion);
                tablaDetalle.AddCell(celdaSubDesc);
                tablaDetalle.AddCell(celdaSubValor);
                tablaDetalle.AddCell(celdaSeparador);

                // LLENADO DE DETALLE ------------------------------------------------------------------------------------------------------------------------------------------------

                XmlNodeList Detalle = documentoXmlBoleta.GetElementsByTagName("Detalle");
                XmlNodeList NmbItem = documentoXmlBoleta.GetElementsByTagName("NmbItem");
                XmlNodeList QtyItem = documentoXmlBoleta.GetElementsByTagName("QtyItem");
                XmlNodeList PrcItem = documentoXmlBoleta.GetElementsByTagName("PrcItem");
                XmlNodeList MontoItem = documentoXmlBoleta.GetElementsByTagName("MontoItem");

                List<int> listaDescuentos = new List<int>();
                List<int> listaRecargos = new List<int>();
                string precioItem = "--";
                string cantidadItem = "--";

                for (int i = 0; i < Detalle.Count; i++)
                {
                    var nodosDetalle = Detalle.Item(i).ChildNodes;

                    //DEFINIMOS LA CULTURA EN LA QUE VIENE EL DATO
                    CultureInfo cultureUS = new CultureInfo("en-US");
                    if (PrcItem.Count != 0)
                    {
                        //PARSEAMOS A DECIMAL CON LOS DECIMALES EN US, Y CONVERTIMOS A STRING CON SEPARADOR DE MILES N                   
                        precioItem = decimal.Parse(PrcItem[i].InnerXml, cultureUS).ToString("N");

                        double numEnero = double.Parse(precioItem.Split(',')[0]);
                        double numDecimal = double.Parse(precioItem.Split(',')[1]);

                        // SI EL DECIMAL ES 0, ENTONCES LO OMITE
                        if (numDecimal != 0)
                        {
                            precioItem = "$" + decimal.Parse(PrcItem[i].InnerXml, cultureUS).ToString("N");
                        }
                        else
                        {
                            precioItem = String.Format("{0:C}", double.Parse(PrcItem[i].InnerText));
                        }

                    }

                    // AVECES NO VIENE LA CANTIDAD DEL ITEM (NOTAS DE CREDITO O DEBITO)
                    if (QtyItem.Count != 0)
                    {
                        //PARSEAMOS A DECIMAL CON LOS DECIMALES EN US, Y CONVERTIMOS A STRING CON SEPARADOR DE MILES N
                        cantidadItem = QtyItem[i].InnerXml;//<<<<<<<<<<<<<<<<<<<<<<<<opcional
                    }

                    string subTotal = String.Format("{0:C}", double.Parse(MontoItem[i].InnerXml));// FORMATO MONEDA SIN DECIMALES
                    string strDescuento = "";
                    string strRecargo = "";
                    string strDRItem = "--";

                    //TODOS ESTOS IF SOLO POR SI VIENENE O NO DESCUENTOS, DESPETANDO LOS LUGARES DE LOS ITEMS (CACHO)
                    for (int j = 0; j < nodosDetalle.Count; j++)
                    {

                        if (nodosDetalle.Item(j).Name == "DescuentoMonto")
                        {
                            listaDescuentos.Add(int.Parse(nodosDetalle.Item(j).InnerText));
                        }

                        if (nodosDetalle.Item(j).Name == "RecargoMonto")
                        {
                            listaRecargos.Add(int.Parse(nodosDetalle.Item(j).InnerText));
                        }

                    }
                    if (!listaDescuentos.Any())
                    {
                        listaDescuentos.Add(0);
                    }
                    if (listaDescuentos.Count <= i)
                    {
                        listaDescuentos.Add(0);
                    }


                    if (!listaRecargos.Any())
                    {
                        listaRecargos.Add(0);
                    }
                    if (listaRecargos.Count <= i)
                    {
                        listaRecargos.Add(0);
                    }


                    if (listaDescuentos[i] != 0)
                    {
                        strDescuento = "- " + String.Format("{0:C}", listaDescuentos[i]);
                    }

                    if (listaRecargos[i] != 0)
                    {
                        strRecargo = String.Format("{0:C}", listaRecargos[i]);
                    }

                    if (strDescuento != "" && strRecargo != "")
                    {
                        strDRItem = strDescuento + " / " + strRecargo;
                    }
                    if (strDescuento == "" && strRecargo != "")
                    {
                        strDRItem = strRecargo;
                    }
                    if (strDescuento != "" && strRecargo == "")
                    {
                        strDRItem = strDescuento;
                    }

                    string cantPrecio = cantidadItem + "  X  " + precioItem;

                    PdfPCell celdaNmbItem = new PdfPCell(new Phrase(NmbItem[i].InnerXml + "\n                     " + cantPrecio, _fuenteBlack7));
                    PdfPCell celdaMontoItem = new PdfPCell(new Phrase(subTotal, _fuenteBlack7));
                    PdfPCell celdaDRMontoItem = new PdfPCell(new Phrase(strDRItem, _fuenteBlack7));

                    celdaNmbItem.BorderWidth = 0f;
                    celdaMontoItem.BorderWidth = 0f;
                    celdaDRMontoItem.BorderWidth = 0f;

                    celdaNmbItem.HorizontalAlignment = Element.ALIGN_LEFT;
                    celdaNmbItem.VerticalAlignment = Element.ALIGN_MIDDLE;
                    celdaMontoItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                    celdaDRMontoItem.HorizontalAlignment = Element.ALIGN_RIGHT;        

                    tablaDetalle.AddCell(celdaNmbItem);
                    tablaDetalle.AddCell(celdaDRMontoItem);
                    tablaDetalle.AddCell(celdaMontoItem);             

                }

                tablaDetalle.AddCell(celdaSeparador);

                //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
                // TABLA TIMBRE Y RESUMEN TOTALES ------------------------------------------------------------------------------------------------------------------------------
                //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

                PdfPTable tablaFooter = new PdfPTable(3);
            
                tablaFooter.TotalWidth = 197f;
                tablaFooter.LockedWidth = true;

                float[] widthsFooter = new float[] {40f, 10f, 10F };
                tablaFooter.SetWidths(widthsFooter);
            
                // TIMBRE
                // Creamos la imagen y le ajustamos el tamaño
                //ENVIAMOS EL STRING DEL NODO TED Y EL FOLIO DEL DTE Y EL TIPO DTE
                string[] respuesta_timbre = new string[] { };
                DTE dte = new DTE();

                //CAPTURAR EL NODO TED DEL XML <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
                
                XmlNodeList DocumentoList = documentoXmlBoleta.GetElementsByTagName("Documento");
                var nodoDocumento = DocumentoList.Item(0).ChildNodes;
                string nodoTed = nodoDocumento.Item(nodoDocumento.Count - 2).OuterXml;

                respuesta_timbre = dte.generarTimbrePDF417(nodoTed, Folio, strTipoDTE);

                iTextSharp.text.Image imagen2 = iTextSharp.text.Image.GetInstance(respuesta_timbre[1]);
                imagen2.BorderWidth = 0;
                imagen2.Alignment = Element.ALIGN_MIDDLE;
                float percentage2 = 0.0f;
                percentage2 = 180 / imagen2.Width;
                imagen2.ScalePercent(percentage2 * 100);

                XmlNodeList MntNetoList = documentoXmlBoleta.GetElementsByTagName("MntNeto");
                XmlNodeList IVAList = documentoXmlBoleta.GetElementsByTagName("IVA");
                XmlNodeList MntTotalList = documentoXmlBoleta.GetElementsByTagName("MntTotal");
                XmlNodeList MntExeList = documentoXmlBoleta.GetElementsByTagName("MntExe");

                string MntNeto = "";
                string IVA = "";
                string MntTotal = "";
                string MntExe = "";
                int MntExed = 0;
                double MntTotald = 0;
                double IVAd = 0;
                double MntNetod = 0;

                if (MntExeList.Count != 0)
                {
                    MntExed = int.Parse(MntExeList[0].InnerXml);
                }

                //SI NO VIENEN MONTOS EXENTOS---------------------------
                if (MntExeList.Count == 0 || MntExed == 0)
                {
                    MntNetod = double.Parse(MntNetoList[0].InnerXml);
                    IVAd = double.Parse(IVAList[0].InnerXml);
                    MntTotald = double.Parse(MntTotalList[0].InnerXml);
                    MntNeto = MntNetod.ToString("N0", new CultureInfo("es-CL"));
                    IVA = IVAd.ToString("N0", new CultureInfo("es-CL"));
                    MntTotal = MntTotald.ToString("N0", new CultureInfo("es-CL"));
                }
                else
                {
                    //SI VIENEN MONTOS EXENTOS Y TAMBIEN MONTOS NETOS------------------
                    if (MntNetoList.Count != 0)
                    {
                        MntNetod = double.Parse(MntNetoList[0].InnerXml);
                        IVAd = double.Parse(IVAList[0].InnerXml);
                        MntNeto = MntNetod.ToString("N0", new CultureInfo("es-CL"));
                        IVA = IVAd.ToString("N0", new CultureInfo("es-CL"));
                        MntExed = int.Parse(MntExeList[0].InnerXml);
                        MntTotald = double.Parse(MntTotalList[0].InnerXml);
                        MntExe = MntExed.ToString("N0", new CultureInfo("es-CL"));
                        MntTotal = MntTotald.ToString("N0", new CultureInfo("es-CL"));
                    }
                    //SI SOLO VIENEN EXENTOS -------------------------
                    else
                    {
                        MntExed = int.Parse(MntExeList[0].InnerXml);
                        MntTotald = double.Parse(MntTotalList[0].InnerXml);
                        MntExe = MntExed.ToString("N0", new CultureInfo("es-CL"));
                        MntTotal = MntTotald.ToString("N0", new CultureInfo("es-CL"));
                    }

                }

                string Leyenda = "Timbre electrónico S.I.I. \n Res. 90 de 2014 \n Verifique documento: www.sii.cl";           

                // CREAMOS CELDAS CON FORMATO
                PdfPCell celdaTimbre = new PdfPCell(imagen2);
                PdfPCell celdaLeyenda = new PdfPCell(new Phrase(Leyenda, _fuenteBlack7));
                PdfPCell celdaSubMonto = new PdfPCell(new Phrase("MONTO NETO", _fuenteBlack7));
                PdfPCell celdaSubExento = new PdfPCell(new Phrase("MONTO EXENTO", _fuenteBlack7));
                PdfPCell celdaSubIVA = new PdfPCell(new Phrase("IVA 19%", _fuenteBlack7));
                PdfPCell celdaSubTotal = new PdfPCell(new Phrase("TOTAL", _fuenteBlack7));
                PdfPCell celdaSubSeparador = new PdfPCell(new Phrase("$", _fuenteBlack7));          
                PdfPCell celdaMonto = new PdfPCell(new Phrase(MntNeto, _fuenteBlack7));
                PdfPCell celdaMontoExento = new PdfPCell(new Phrase(MntExe, _fuenteBlack7));
                PdfPCell celdaIVA = new PdfPCell(new Phrase(IVA, _fuenteBlack7));
                PdfPCell celdaTotal = new PdfPCell(new Phrase(MntTotal, _fuenteBlack7));           
                PdfPCell celdaEmptyFila = new PdfPCell(new Phrase(" "));           

                celdaEmptyFila.BorderWidth = 0f;
                celdaSubMonto.BorderWidth = 0f;          
                celdaSubExento.BorderWidth = 0f;
                celdaTimbre.BorderWidth = 0f;
                celdaLeyenda.BorderWidth = 0f;
                celdaSubIVA.BorderWidth = 0f;
                celdaSubTotal.BorderWidth = 0f;
                celdaSubSeparador.BorderWidth = 0f;
                celdaMonto.BorderWidth = 0f;
                celdaMontoExento.BorderWidth = 0f;
                celdaIVA.BorderWidth = 0f;
                celdaTotal.BorderWidth = 0f;

                celdaEmptyFila.Colspan = 3;
                celdaTimbre.Colspan = 3;
                celdaLeyenda.Colspan = 3;

                celdaLeyenda.HorizontalAlignment = Element.ALIGN_CENTER;
                celdaLeyenda.VerticalAlignment = Element.ALIGN_MIDDLE;
                celdaSubMonto.HorizontalAlignment = Element.ALIGN_LEFT;         
                celdaSubExento.HorizontalAlignment = Element.ALIGN_LEFT;           
                celdaSubIVA.HorizontalAlignment = Element.ALIGN_LEFT;         
                celdaSubTotal.HorizontalAlignment = Element.ALIGN_LEFT;
                celdaSubSeparador.HorizontalAlignment = Element.ALIGN_CENTER;     
                celdaMonto.HorizontalAlignment = Element.ALIGN_RIGHT;            
                celdaMontoExento.HorizontalAlignment = Element.ALIGN_RIGHT;            
                celdaIVA.HorizontalAlignment = Element.ALIGN_RIGHT;
                celdaTotal.HorizontalAlignment = Element.ALIGN_RIGHT;
                celdaTimbre.HorizontalAlignment = Element.ALIGN_CENTER;

                if (MntExeList.Count == 0 || MntExed == 0)
                {              
                    tablaFooter.AddCell(celdaSubMonto);
                    tablaFooter.AddCell(celdaSubSeparador);
                    tablaFooter.AddCell(celdaMonto);                
                    tablaFooter.AddCell(celdaSubIVA);
                    tablaFooter.AddCell(celdaSubSeparador);
                    tablaFooter.AddCell(celdaIVA);
                }
                else
                {
                    if (MntNetoList.Count != 0)
                    {                   
                        tablaFooter.AddCell(celdaSubMonto);
                        tablaFooter.AddCell(celdaSubSeparador);
                        tablaFooter.AddCell(celdaMonto);                   
                        tablaFooter.AddCell(celdaSubExento);
                        tablaFooter.AddCell(celdaSubSeparador);
                        tablaFooter.AddCell(celdaMontoExento);                   
                        tablaFooter.AddCell(celdaSubIVA);
                        tablaFooter.AddCell(celdaSubSeparador);
                        tablaFooter.AddCell(celdaIVA);
                    }
                    else
                    {                    
                        tablaFooter.AddCell(celdaSubExento);
                        tablaFooter.AddCell(celdaSubSeparador);
                        tablaFooter.AddCell(celdaMontoExento);
                    }
                }

                tablaFooter.AddCell(celdaEmptyFila);           
                tablaFooter.AddCell(celdaSubTotal);
                tablaFooter.AddCell(celdaSubSeparador);
                tablaFooter.AddCell(celdaTotal);
                tablaFooter.AddCell(celdaEmptyFila);
                tablaFooter.AddCell(celdaTimbre);
                tablaFooter.AddCell(celdaLeyenda);
                tablaFooter.AddCell(celdaEmptyFila);
                tablaFooter.AddCell(celdaEmptyFila);
                tablaFooter.AddCell(celdaEmptyFila);


                //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
                // CREADO Y RELLENO DEL DOCUMENTO ---------------------------------------------------------------------------------------------------------------------------
                //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

                float h = tablaSubHeader.TotalHeight;
                float h2 = tablaEmisor.TotalHeight;
                float h3 = tablaDetalle.TotalHeight;
                float h4 = tablaFooter.TotalHeight;
                float hTotal = h + h2 + h3 + h4;
                var pgSize = new iTextSharp.text.Rectangle(210, hTotal);
                
                Document documentoPDF = new Document(pgSize, 0, 0, 0, 0);

                string fileNameXml = Path.GetFileName(path_boleta);
                string fileNamePdf = fileNameXml.Replace(".xml", ".pdf");

                
                string respuesta_directorio = crearDirectorio();

                string directorio_pdf = respuesta_directorio +"\\" + fileNamePdf;

                PdfWriter writer = PdfWriter.GetInstance(documentoPDF, new FileStream(directorio_pdf, FileMode.Create));

                documentoPDF.AddTitle("PDF_Boleta");
                documentoPDF.AddCreator("AGROPLASTIC");

                documentoPDF.Open();            

                documentoPDF.Add(tablaSubHeader);
                documentoPDF.Add(tablaEmisor);
                documentoPDF.Add(tablaDetalle);
                documentoPDF.Add(tablaFooter);

                documentoPDF.Close();
                writer.Close();

                //DEVOLVER EL PDF CONVERTIDO EN BASE64
                Byte[] bytes = File.ReadAllBytes(directorio_pdf);
                string file_pdf = Convert.ToBase64String(bytes);

                return file_pdf;
            }
            catch (Exception e)
            {
                return e.Message;
                
            }
        }
    }
}
