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
    public class PDF
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

        public string CrearPDF(string path)
        {
            try
            {            
                string observaciones = "";           

                ConexionBD conexion = new ConexionBD();

                XmlDocument documentoXml = new XmlDocument();
                //string path = "C:\\Users\\Marcelo Riquelme\\source\\repos\\ConsoleApp1\\DTE 34 - Empresa 76795561 - Folio Nº 101882.xml";

                documentoXml.Load(path);

                // Creamos el documento con el tamaño de página tradicional
                Document documentoPDF = new Document(PageSize.LETTER, 20f, 20f, 20f, 20f);
                //Document documentoPDF = new Document();

                string fileNameXml = Path.GetFileName(path);
                string fileNamePdf = fileNameXml.Replace(".xml", ".pdf");
                // Indicamos donde vamos a guardar el documento


               

                string respuesta_directorio = crearDirectorio();



                string directorio_pdf = respuesta_directorio +"\\" + fileNamePdf;

                PdfWriter writer;
               
                writer = PdfWriter.GetInstance(documentoPDF, new FileStream(directorio_pdf, FileMode.Create));




                



                // Le colocamos el título y el autor
                // **Nota: Esto no será visible en el documento
                documentoPDF.AddTitle("PDF");
                documentoPDF.AddCreator("AGROPLASTIC");

                // ABRIMOS EL DOCUMENTO
                documentoPDF.Open();

                // SELECCIONAMOS EL TIPO DE DOCUMENTO Y SI ES TRANSPORTE, EL TIPO DE TRANSPORTE
                XmlNodeList TipoDTEList = documentoXml.GetElementsByTagName("TipoDTE");            
                XmlNodeList TransporteList = documentoXml.GetElementsByTagName("Transporte");

                int ejemplares = 0;
                string IndTraslado = "";
                string TipoDTE = TipoDTEList[0].InnerXml;
                
                if (TransporteList.Count != 0)
                {
                    XmlNodeList IndTrasladoList = documentoXml.GetElementsByTagName("IndTraslado");
                    IndTraslado = IndTrasladoList[0].InnerXml;
                }            

                // INDICAMOS LA CANTIDAD DE PAGINAS, DEPENDIENDO DEL DOCUMENTO
                if (TipoDTE == "56" || TipoDTE == "61" || IndTraslado == "5" || IndTraslado == "6")
                {
                    ejemplares = 1;
                }
                else
                {
                    ejemplares = 2;
                }

                //LLAMAMOS LA FUNCION DEPENDIENDO DE LAS PAGINAS
                for (int pagina = 0; pagina < ejemplares; pagina++)
                {
                   GenerarDocumento(documentoPDF, documentoXml, observaciones, writer, pagina);
                    documentoPDF.NewPage();
                }

                // CERRAMOS EL DOCUMENTO Y TERMINAMOS DE ESCRIBIR EN EL
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

        public string CrearPDFCompra(string path,string folio_compra)
        {
           
            try
            {
                string observaciones = "";

                ConexionBD conexion = new ConexionBD();

                XmlDocument documentoXml = new XmlDocument();
                //string path = "C:\\Users\\Marcelo Riquelme\\source\\repos\\ConsoleApp1\\DTE 34 - Empresa 76795561 - Folio Nº 101882.xml";

                documentoXml.Load(path);     

               


                // Creamos el documento con el tamaño de página tradicional
                Document documentoPDF = new Document(PageSize.LETTER, 20f, 20f, 20f, 20f);
                //Document documentoPDF = new Document();

                string fileNameXml = Path.GetFileName(path);
                string fileNamePdf = fileNameXml.Replace(".xml", ".pdf");
                // Indicamos donde vamos a guardar el documento

                string respuesta_directorio = crearDirectorio();



                string directorio_pdf = respuesta_directorio + "\\" + fileNamePdf;

                PdfWriter writer;

                writer = PdfWriter.GetInstance(documentoPDF, new FileStream(directorio_pdf, FileMode.Create));


                // Le colocamos el título y el autor
                // **Nota: Esto no será visible en el documento
                documentoPDF.AddTitle("PDF");
                documentoPDF.AddCreator("AGROPLASTIC");

                // ABRIMOS EL DOCUMENTO
                documentoPDF.Open();

                // SELECCIONAMOS EL TIPO DE DOCUMENTO Y SI ES TRANSPORTE, EL TIPO DE TRANSPORTE
                XmlNodeList TipoDTEList = documentoXml.GetElementsByTagName("TipoDTE");
                XmlNodeList TransporteList = documentoXml.GetElementsByTagName("Transporte");

                int ejemplares = 0;
                string IndTraslado = "";
                string TipoDTE = TipoDTEList[0].InnerXml;

                if (TransporteList.Count != 0)
                {
                    XmlNodeList IndTrasladoList = documentoXml.GetElementsByTagName("IndTraslado");

                    if (IndTrasladoList.Count != 0)
                    {
                        IndTraslado = IndTrasladoList[0].InnerXml;
                    }

                    
                }

                // INDICAMOS LA CANTIDAD DE PAGINAS, DEPENDIENDO DEL DOCUMENTO
                if (TipoDTE == "56" || TipoDTE == "61" || IndTraslado == "5" || IndTraslado == "6")
                {
                    ejemplares = 1;
                }
                else
                {
                    ejemplares = 2;
                }

                //LLAMAMOS LA FUNCION DEPENDIENDO DE LAS PAGINAS
                for (int pagina = 0; pagina < ejemplares; pagina++)
                {
                    //AQUI VEMOS SI EL XML VIENEN MAS DTE DENTRO DEL SOBRE
                    XmlNodeList DTEList = documentoXml.GetElementsByTagName("DTE");
                    //int count_DTEList = DTEList.Count;

                    //if (count_DTEList > 1)
                    //{


                        foreach (XmlNode dte in DTEList)
                        {

                        //AVECES SE PRESENTA EL CASO QUE EL XML VIENE CON UN COMENTARIO DENTRO DE LA ETIQUETA <DTE>
                        string dte_str = "";
                        if (dte.ChildNodes[0].OuterXml.Contains("<!--"))
                        {
                            dte_str = dte.ChildNodes[1].OuterXml;
                        }
                        else
                        {
                            dte_str = dte.ChildNodes[0].OuterXml;
                        }
                       

                        
                            XmlDocument documentoXML_DTE = new XmlDocument();
                            documentoXML_DTE.LoadXml(dte_str);
                            XmlNodeList Folio_list = documentoXML_DTE.GetElementsByTagName("Folio");

                            string folio_dte = Folio_list[0].InnerXml;

                            if (folio_compra == folio_dte)
                            {
                                GenerarDocumento(documentoPDF, documentoXML_DTE, observaciones, writer, pagina);
                                documentoPDF.NewPage();
                            }



                        }

                    //}
                   
                }

                // CERRAMOS EL DOCUMENTO Y TERMINAMOS DE ESCRIBIR EN EL
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

        static public void GenerarDocumento(Document documentoPDF, XmlDocument documentoXml, string observaciones, PdfWriter writer, int pagina)
        {
            //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<-------------------INICIO----------------------->>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
             ConexionBD conexion = new ConexionBD();

            string quieroVerLaWea = "";
            //COLORES BASE NUEVOS
            var FontColourAzul1 = new BaseColor(32, 67, 138);
            var FontColourAzul2 = new BaseColor(93, 127, 175);
            var FontColourPlomo = new BaseColor(194, 194, 194);
            var FontColourNiebla = new BaseColor(238, 240, 252);

            // FUENTES -----------------------------------------------------------------------------------------------------------------------------------------
            iTextSharp.text.Font _fuenteRedBold = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.BOLD, BaseColor.RED);
            iTextSharp.text.Font _fuenteBlackBold = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 9, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
            iTextSharp.text.Font _fuenteBlackBold11 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 11, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
            iTextSharp.text.Font _fuenteBlack = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 9, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
            iTextSharp.text.Font _fuenteBlack6 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 7, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
            iTextSharp.text.Font _fuenteWhiteBold = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 9, iTextSharp.text.Font.BOLD, BaseColor.WHITE);
            iTextSharp.text.Font _fuenteAzul18Bold = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.BOLD, FontColourAzul1);
            iTextSharp.text.Font _fuenteAzul110 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.NORMAL, FontColourAzul1);
            iTextSharp.text.Font _fuenteAzul210 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.NORMAL, FontColourAzul2);
            iTextSharp.text.Font _fuenteAzul19 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 9, iTextSharp.text.Font.NORMAL, FontColourAzul1);

           

            // CREACION DE TABLAS Y CANTIDAD DE COLUMNAS -----------------------------------------------------------------------------------------------------------
            PdfPTable tablaHeader = new PdfPTable(3);
            PdfPTable tablaSubHeader = new PdfPTable(1);

            // TAMAÑO DE TABLAS
            tablaHeader.TotalWidth = 570f;
            tablaHeader.LockedWidth = true;

            // PROPORCIONES RELATIVAS DE COLUMNAS (FRACCIONES)
            float[] widthsHeader = new float[] { 2f, 4f, 2f };
            tablaHeader.SetWidths(widthsHeader);

            // VARIABLES --------------------------------------
            TextInfo ti = CultureInfo.CurrentCulture.TextInfo;// SOLO SE USA PARA DARLE FORMATO A LA FECHA

            XmlNodeList rutEmisorList = documentoXml.GetElementsByTagName("RUTEmisor");
            XmlNodeList dirOrigenList = documentoXml.GetElementsByTagName("DirOrigen");//cambiar
            XmlNodeList cmnaOrigenList = documentoXml.GetElementsByTagName("CmnaOrigen");//cambiar
            XmlNodeList ciudadOrigenList = documentoXml.GetElementsByTagName("CiudadOrigen");
            XmlNodeList actecoList = documentoXml.GetElementsByTagName("Acteco");
            XmlNodeList FolioList = documentoXml.GetElementsByTagName("Folio");
            XmlNodeList TipoDTEList = documentoXml.GetElementsByTagName("TipoDTE");
            XmlNodeList FchEmisList = documentoXml.GetElementsByTagName("FchEmis");

            string rut1 = rutEmisorList[0].InnerXml.Substring(0, rutEmisorList[0].InnerXml.Length - 2);
            string rut2 = rutEmisorList[0].InnerXml.Substring(rutEmisorList[0].InnerXml.Length - 2, 2);
            int rut3 = int.Parse(rut1);
            string rut4 = rut3.ToString("N0", new CultureInfo("es-CL"));
            string rutEmisor = rut4 + rut2;

            //EN DESHUSO COLOCAMOS ESTA CONSULTA PARA TRAER LOS DATOS DE LA BD
            //string strRazonSocial = "SELECT des_actividad_economica,razon_social FROM contribuyentes_acteco WHERE rut ='" + rut1 + "'";//cambiar consultas                    

            string header1 = "--";
            string header2 = "--";
            string header3 = "--";
            string header4 = "";
            string header5 = "";
            string strComuna = "";
            string strCiudad = "";
            string strDirOrigen = "";

            if (rutEmisor == "76.958.430-7")
            {
                header1 = "IMPORTADORA COMERCIALIZADORA Y DISTRIBUIDORA AGROPLASTIC LTDA";
                header2 = "VENTA AL POR MAYOR NO ESPECIALIZADA";
                header3 = "VICENTE ZORRILLA #835, LA SERENA";
                header4 = "(51) 2222189 - (51) 2221421";
                header5 = "GERENCIA@AGROPLASTIC.CL";

                if (cmnaOrigenList.Count != 0)
                {
                    strComuna = cmnaOrigenList[0].InnerXml;         
                }


                 
            }
            else
            {
                //MODIFICAMOS PARA QUE SAQUE LA INFORMACION DESDE EL XML EN VES DE LA BASE DE DATOS
                //List<string> resultStrRazonSocial = conexion.Select(strRazonSocial);

                /*if (resultStrRazonSocial.Any())
             {
                 header1 = resultStrRazonSocial[1].ToUpper();
                 header2 = resultStrRazonSocial[0].ToUpper();
             }*/

                //NUEVO CODIGO
                //TRAEMOS RAZON SOCIAL Y GIRO DESDE EL XML
                XmlNodeList RznSocList = documentoXml.GetElementsByTagName("RznSoc");
                if (RznSocList.Count != 0)
                {
                    header1 = RznSocList[0].InnerXml;
                }
                XmlNodeList GiroEmisList = documentoXml.GetElementsByTagName("GiroEmis");
                if (GiroEmisList.Count != 0)
                {
                    header2 = GiroEmisList[0].InnerXml;
                }


             

                if (dirOrigenList.Count != 0)
                {
                    strDirOrigen = dirOrigenList[0].InnerXml;
                }

                if (cmnaOrigenList.Count != 0)
                {
                    strComuna = cmnaOrigenList[0].InnerXml;
                }

                if (ciudadOrigenList.Count != 0)
                {
                    strCiudad = ciudadOrigenList[0].InnerXml;
                }

                header3 = strDirOrigen;

                if (strComuna != "")
                {
                    header3 = strDirOrigen + ", " + strComuna;
                }
                
                header4 = strCiudad;
            }     

            // LOGO AGROPLASTICO----------------------------------------
            // Creamos la imagen y le ajustamos el tamaño
            iTextSharp.text.Image imagen = iTextSharp.text.Image.GetInstance(@"C:\inetpub\wwwroot\api_agrodte\AgroDTE_Archivos\logofactura.png");
            imagen.BorderWidth = 0;
            imagen.Alignment = Element.ALIGN_RIGHT;
            imagen.Alignment = Element.ALIGN_MIDDLE;
            float percentage = 0.0f;
            percentage = 140 / imagen.Width;
            imagen.ScalePercent(percentage * 100);

            // VARIABLES RECUADRO ROJO------------------------------------
            string Folio = FolioList[0].InnerXml;
            string strTipoDTE = TipoDTEList[0].InnerXml;
            string TipoDTE = "";
            var FchEmis = DateTime.Parse(FchEmisList[0].InnerXml);
            string fecha = FchEmis.ToLongDateString();
            

            if (strTipoDTE == "33")
            {
                TipoDTE = "FACTURA ELECTRÓNICA";
            }
            if (strTipoDTE == "34")
            {
                TipoDTE = "FACTURA NO AFECTA O EXENTA ELECTRÓNICA";
            }
            if (strTipoDTE == "39")
            {
                TipoDTE = "BOLETA ELECTRÓNICA";
            }
            if (strTipoDTE == "41")
            {
                TipoDTE = " BOLETA EXENTA ELECTRÓNICA";
            }
            if (strTipoDTE == "56")
            {
                TipoDTE = "NOTA DE DÉBITO ELECTRÓNICA";
            }
            if (strTipoDTE == "61")
            {
                TipoDTE = "NOTA DE CRÉDITO ELECTRÓNICA";
            }
            if (strTipoDTE == "52")
            {
                TipoDTE = "GUÍA DE DESPACHO ELECTRÓNICA";
            }

            string strFactura = " \n R.U.T.: " + rutEmisor + " \n \n " + TipoDTE + " \n \n " + Folio + " \n ";

            // CREAMOS CELDAS CON FORMATO
            PdfPCell celdaimagen = new PdfPCell(imagen);
            PdfPCell celdaFactura = new PdfPCell(new Phrase(strFactura, _fuenteRedBold));
            PdfPCell celdaAgro1 = new PdfPCell(new Phrase(header1, _fuenteAzul110));
            PdfPCell celdaAgro2 = new PdfPCell(new Phrase(header2 + "\n" + header3 + "\n" + header4 + "\n" + header5, _fuenteAzul210));
            PdfPCell celdaBottom = new PdfPCell(new Phrase(""));
            PdfPCell celdaTop = new PdfPCell(new Phrase(""));
            PdfPCell celdaEmpty1 = new PdfPCell(new Phrase(" "));
            PdfPCell celdaLS2 = new PdfPCell(new Phrase(" S.I.I. "+ strComuna +" \n \n", _fuenteRedBold));
            PdfPCell celdaFecha = new PdfPCell(new Phrase(ti.ToTitleCase(fecha), _fuenteBlack));
            PdfPCell celdaAgro = new PdfPCell(tablaSubHeader);

            // SETEAMOS CARCACTERISITCAS DE LAS CELDAS
            celdaimagen.BorderWidth = 0f;
            celdaAgro1.BorderWidth = 0f;
            celdaAgro2.BorderWidth = 0f;            
            celdaFactura.BorderWidth = 2f;
            celdaFactura.BorderWidthBottom = 2f;

            celdaFecha.BorderColor = FontColourPlomo;
            celdaTop.BorderColor = FontColourPlomo;
            celdaFactura.BorderColor = BaseColor.RED;
            celdaFactura.BorderColor = BaseColor.RED;
            celdaFactura.BorderColor = BaseColor.RED;
            celdaAgro.BorderColor = BaseColor.WHITE;

            celdaFactura.HorizontalAlignment = Element.ALIGN_CENTER;
            celdaFactura.VerticalAlignment = Element.ALIGN_MIDDLE;
            celdaFecha.HorizontalAlignment = Element.ALIGN_RIGHT;
            celdaLS2.HorizontalAlignment = Element.ALIGN_CENTER;
            celdaLS2.VerticalAlignment = Element.ALIGN_MIDDLE;

            celdaBottom.Border = Rectangle.BOTTOM_BORDER;
            celdaTop.Border = Rectangle.TOP_BORDER;
            celdaLS2.Border = Rectangle.BOTTOM_BORDER;
            celdaFecha.Border = Rectangle.TOP_BORDER;

            // AGREGAMOS LAS CELDAS A LAS COLUMNAS DE LAS TABLAS EN ORDEN DE IZQUIERDA A DERECHA

            tablaSubHeader.AddCell(celdaAgro1);
            tablaSubHeader.AddCell(celdaAgro2);           

            // TABLA HEADER (3 COLUMNAS - 1 FILA)
            if (rutEmisor == "76.958.430-7")
            {
                tablaHeader.AddCell(celdaimagen);
                tablaHeader.AddCell(celdaAgro);
            }
            else
            {
                celdaAgro.Colspan = 2;
                tablaHeader.AddCell(celdaAgro);
            }
           
            tablaHeader.AddCell(celdaFactura);
            tablaHeader.AddCell(celdaBottom);
            tablaHeader.AddCell(celdaBottom);
            tablaHeader.AddCell(celdaLS2);
            tablaHeader.AddCell(celdaTop);
            tablaHeader.AddCell(celdaTop);
            tablaHeader.AddCell(celdaFecha);

            documentoPDF.Add(tablaHeader);
            //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
            // RECEPTOR ---------------------------------------------------------------------------------------------------------------------------------------
            //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
            PdfPTable tablaReceptor = new PdfPTable(3);
            PdfPTable tablaRecepDatos = new PdfPTable(2);
            PdfPTable tablaRecepVenta = new PdfPTable(2);

            tablaReceptor.TotalWidth = 570f;
            tablaReceptor.LockedWidth = true;

            float[] widthsReceptor = new float[] { 6f, 1f, 3f };
            float[] widthsRecepDatos = new float[] { 1f, 5f };
            float[] widthsRecepVenta = new float[] { 3f, 4f };
            tablaReceptor.SetWidths(widthsReceptor);
            tablaRecepDatos.SetWidths(widthsRecepDatos);
            tablaRecepVenta.SetWidths(widthsRecepVenta);

            XmlNodeList RUTRecepList = documentoXml.GetElementsByTagName("RUTRecep");
            XmlNodeList RznSocRecepList = documentoXml.GetElementsByTagName("RznSocRecep");
            XmlNodeList GiroRecepList = documentoXml.GetElementsByTagName("GiroRecep");//opcional
            XmlNodeList DirRecepList = documentoXml.GetElementsByTagName("DirRecep");//opcional
            XmlNodeList CmnaRecepList = documentoXml.GetElementsByTagName("CmnaRecep");//opcional
            XmlNodeList TelRecepList = documentoXml.GetElementsByTagName("Telefono"); //opcional           
            XmlNodeList FrmaPagoList = documentoXml.GetElementsByTagName("FmaPago"); //opcional
            XmlNodeList TpoVentaList = documentoXml.GetElementsByTagName("TpoTranVenta"); //opcional
            XmlNodeList TpoCompraList = documentoXml.GetElementsByTagName("TpoTranCompra"); //opcional
            XmlNodeList VendedorList = documentoXml.GetElementsByTagName("CdgVendedor");//opcional
                        
            string RUTRecep = ": " + RUTRecepList[0].InnerXml;
            string RznSocRecep = ": " + RznSocRecepList[0].InnerXml;

            string GiroRecep = "";
            if (GiroRecepList.Count != 0)
            {
                GiroRecep = ": " + GiroRecepList[0].InnerXml;
            }          

            string DirRecep = "";
            if (DirRecepList.Count !=0 && CmnaRecepList.Count !=0)
            {
                DirRecep = ": " + DirRecepList[0].InnerXml + ", " + CmnaRecepList[0].InnerXml.ToUpper();
            }
            if (DirRecepList.Count != 0 && CmnaRecepList.Count == 0)
            {
                DirRecep = ": " + DirRecepList[0].InnerXml;
            }
            if (DirRecepList.Count == 0 && CmnaRecepList.Count != 0)
            {
                DirRecep = ": " + CmnaRecepList[0].InnerXml.ToUpper();
            }            

            string FrmaPago = "";
            if (FrmaPagoList.Count != 0)
            {
                FrmaPago = FrmaPagoList[0].InnerXml;
            }

            string TpoVenta = "";
            if (TpoVentaList.Count != 0)
            {
                TpoVenta = TpoVentaList[0].InnerXml;
                if (TpoVenta == "1")
                {
                    TpoVenta = ": Ventas del Giro.";
                }
                if (TpoVenta == "2")
                {
                    TpoVenta = ": Venta Activo Fijo.";
                }
                if (TpoVenta == "3")
                {
                    TpoVenta = ": Venta Bien Raíz.";
                }
            }

            string TpoCompra = "";
            if (TpoCompraList.Count != 0)
            {
                TpoCompra = TpoVentaList[0].InnerXml;
                if (TpoCompra == "1")
                {
                    TpoCompra = ": Compras del Giro.";
                }
                if (TpoCompra == "2")
                {
                    TpoCompra = ": Compras en Supermercados o similares.";
                }
                if (TpoCompra == "3")
                {
                    TpoCompra = ": Adquisición Bien Raíz.";
                }
                if (TpoCompra == "4")
                {
                    TpoCompra = ": Compra Activo Fijo.";
                }
                if (TpoCompra == "5")
                {
                    TpoCompra = ": Compra con IVA Uso Común.";
                }
                if (TpoCompra == "6")
                {
                    TpoCompra = ": Compra sin derecho a Crédito.";
                }
                if (TpoCompra == "7")
                {
                    TpoCompra = ": Compra que no corresponde incluir.";
                }
            }

            string Vendedor = "";
            if (VendedorList.Count != 0) // opcional
            {
                Vendedor = ": " + VendedorList[0].InnerXml;
            }

            string TelRecep = "";

            if (RUTRecepList[0].InnerXml == "76958430-7")
            {
                TelRecep = "222189";
            }
            else
            {
                if (TelRecepList.Count != 0) //opcional
                {
                    TelRecep = ": " + TelRecepList[0].InnerXml;
                }
            }
           

            


            if (FrmaPago == "1")
            {
                FrmaPago = ": Contado.";
            }
            if (FrmaPago == "2")
            {
                FrmaPago = ": Crédito.";
            }
            if (FrmaPago == "3")
            {
                FrmaPago = ": Sin costo.";
            }

            PdfPCell celda11 = new PdfPCell(new Phrase("R.U.T.", _fuenteBlackBold));
            PdfPCell celda12 = new PdfPCell(new Phrase("SEÑOR (ES)", _fuenteBlackBold));
            PdfPCell celda13 = new PdfPCell(new Phrase("GIRO", _fuenteBlackBold));
            PdfPCell celda14 = new PdfPCell(new Phrase("DIRECCIÓN", _fuenteBlackBold));
            PdfPCell celda15 = new PdfPCell(new Phrase("TELÉFONO", _fuenteBlackBold));

            PdfPCell celda21 = new PdfPCell(new Phrase(RUTRecep, _fuenteBlack));
            PdfPCell celda22 = new PdfPCell(new Phrase(RznSocRecep, _fuenteBlack));
            PdfPCell celda23 = new PdfPCell(new Phrase(GiroRecep, _fuenteBlack));
            PdfPCell celda24 = new PdfPCell(new Phrase(DirRecep, _fuenteBlack));
            PdfPCell celda25 = new PdfPCell(new Phrase(TelRecep, _fuenteBlack));

            PdfPCell celda31 = new PdfPCell(new Phrase("Forma de pago", _fuenteBlackBold));
            PdfPCell celda32 = new PdfPCell(new Phrase("Tipo Venta", _fuenteBlackBold));
            PdfPCell celda34 = new PdfPCell(new Phrase("Tipo Compra", _fuenteBlackBold));
            PdfPCell celda33 = new PdfPCell(new Phrase("Vendedor", _fuenteBlackBold));

            PdfPCell celda41 = new PdfPCell(new Phrase(FrmaPago, _fuenteBlack));
            PdfPCell celda42 = new PdfPCell(new Phrase(TpoVenta, _fuenteBlack));
            PdfPCell celda43 = new PdfPCell(new Phrase(Vendedor, _fuenteBlack));
            PdfPCell celda44 = new PdfPCell(new Phrase(TpoCompra, _fuenteBlack));

            // QUITAR BORDES POR CELDA
            celda11.BorderWidth = 0f;
            celda21.BorderWidth = 0f;
            celda12.BorderWidth = 0f;
            celda22.BorderWidth = 0f;
            celda13.BorderWidth = 0f;
            celda23.BorderWidth = 0f;
            celda14.BorderWidth = 0f;
            celda24.BorderWidth = 0f;
            celda15.BorderWidth = 0f;
            celda25.BorderWidth = 0f;

            celdaEmpty1.BorderWidth = 0f;
            celda31.BorderWidth = 0f;
            celda41.BorderWidth = 0f;
            celda32.BorderWidth = 0f;
            celda42.BorderWidth = 0f;
            celda33.BorderWidth = 0f;
            celda34.BorderWidth = 0f;
            celda44.BorderWidth = 0f;
            celda43.BorderWidth = 0f;

            // TABLA RECEPTOR DATOS (TABLA DE 2 COLUMNAS DENTRO DE LA PRIMERA COLUMNA DE RECEPTOR)
            tablaRecepDatos.AddCell(celda11);
            tablaRecepDatos.AddCell(celda21);
            tablaRecepDatos.AddCell(celda12);
            tablaRecepDatos.AddCell(celda22);
            tablaRecepDatos.AddCell(celda13);
            tablaRecepDatos.AddCell(celda23);
            tablaRecepDatos.AddCell(celda14);
            tablaRecepDatos.AddCell(celda24);
            if (TelRecep != "")
            {
                tablaRecepDatos.AddCell(celda15);
                tablaRecepDatos.AddCell(celda25);
            }

            // TIPO VENTA
            tablaRecepVenta.AddCell(celdaEmpty1);
            tablaRecepVenta.AddCell(celdaEmpty1);

            if (FrmaPago != "")
            {
                tablaRecepVenta.AddCell(celda31);
                tablaRecepVenta.AddCell(celda41);
            }

            if (TpoVenta != "")
            {
                tablaRecepVenta.AddCell(celda32);
                tablaRecepVenta.AddCell(celda42);
            }

            if (TpoCompra != "")
            {
                tablaRecepVenta.AddCell(celda34);
                tablaRecepVenta.AddCell(celda44);
            }

            if (Vendedor != "")
            {
                tablaRecepVenta.AddCell(celda33);
                tablaRecepVenta.AddCell(celda43);
            }

            tablaRecepVenta.AddCell(celdaEmpty1);
            tablaRecepVenta.AddCell(celdaEmpty1);

            PdfPCell celdaRecepDatos = new PdfPCell(tablaRecepDatos);
            PdfPCell celdaRecepVenta = new PdfPCell(tablaRecepVenta);

            celdaRecepDatos.BackgroundColor = FontColourNiebla;            

            celdaRecepDatos.BorderWidth = 0f;
            celdaRecepVenta.BorderWidth = 0f;

            tablaReceptor.AddCell(celdaRecepDatos);
            tablaReceptor.AddCell(celdaEmpty1);
            tablaReceptor.AddCell(celdaRecepVenta);           

            documentoPDF.Add(tablaReceptor);

            //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
            // TABLA DETALLE ------------------------------------------------------------------------------------------------------------------------
            //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

            // ENCABEZADOS --------------------------------------------------------------------------------

            PdfPTable tablaDetalle = new PdfPTable(5);

            tablaDetalle.TotalWidth = 570f;
            tablaDetalle.LockedWidth = true;

            float[] widthsDetalle = new float[] { 50f, 10f, 15f, 15f, 15f };

            tablaDetalle.SetWidths(widthsDetalle);

            PdfPCell celdaTituloDetalle = new PdfPCell(new Phrase("DETALLE ÍTEMS", _fuenteWhiteBold));

            celdaTituloDetalle.Colspan = 5;
            celdaTituloDetalle.HorizontalAlignment = 1;

            PdfPCell celdaSubItem = new PdfPCell(new Phrase("Ítem", _fuenteAzul18Bold));
            PdfPCell celdaSubCantidad = new PdfPCell(new Phrase("Cantidad", _fuenteAzul18Bold));
            PdfPCell celdaSubPrecioNeto = new PdfPCell(new Phrase("Precio Neto", _fuenteAzul18Bold));
            PdfPCell celdaSubDscRcg = new PdfPCell(new Phrase("Desc/Rec", _fuenteAzul18Bold));
            PdfPCell celdaSubSubTotal = new PdfPCell(new Phrase("Subtotal", _fuenteAzul18Bold));
            PdfPCell celdaEmpty2 = new PdfPCell(new Phrase(" "));

            celdaEmpty2.BorderWidth = 0f;
            celdaEmpty2.Colspan = 5;

            celdaSubItem.BorderColor = FontColourAzul2;
            celdaSubCantidad.BorderColor = FontColourAzul2;
            celdaSubPrecioNeto.BorderColor = FontColourAzul2;
            celdaSubSubTotal.BorderColor = FontColourAzul2;
            celdaSubDscRcg.BorderColor = FontColourAzul2;

            celdaSubItem.HorizontalAlignment = Element.ALIGN_CENTER;
            celdaSubItem.VerticalAlignment = Element.ALIGN_MIDDLE;
            celdaSubCantidad.HorizontalAlignment = Element.ALIGN_CENTER;
            celdaSubCantidad.VerticalAlignment = Element.ALIGN_MIDDLE;
            celdaSubPrecioNeto.HorizontalAlignment = Element.ALIGN_CENTER;
            celdaSubPrecioNeto.VerticalAlignment = Element.ALIGN_MIDDLE;
            celdaSubDscRcg.HorizontalAlignment = Element.ALIGN_CENTER;
            celdaSubDscRcg.VerticalAlignment = Element.ALIGN_MIDDLE;
            celdaSubSubTotal.HorizontalAlignment = Element.ALIGN_CENTER;
            celdaSubSubTotal.VerticalAlignment = Element.ALIGN_MIDDLE;

            celdaAgro.BorderColor = BaseColor.WHITE;
            celdaFecha.BorderColor = FontColourPlomo;
            celdaTop.BorderColor = FontColourPlomo;

            celdaSubItem.BackgroundColor = FontColourNiebla;
            celdaSubCantidad.BackgroundColor = FontColourNiebla;
            celdaSubPrecioNeto.BackgroundColor = FontColourNiebla;
            celdaSubDscRcg.BackgroundColor = FontColourNiebla;
            celdaSubSubTotal.BackgroundColor = FontColourNiebla;

            celdaTituloDetalle.BackgroundColor = FontColourAzul1;

            celdaTituloDetalle.BorderColor = FontColourAzul1;

            tablaDetalle.AddCell(celdaEmpty2);
            tablaDetalle.AddCell(celdaTituloDetalle);

            tablaDetalle.AddCell(celdaSubItem);
            tablaDetalle.AddCell(celdaSubCantidad);
            tablaDetalle.AddCell(celdaSubPrecioNeto);
            tablaDetalle.AddCell(celdaSubDscRcg);
            tablaDetalle.AddCell(celdaSubSubTotal);

            //DETALLE (CICLO DE LLENADO)---------------------------------------------------------------------------------------------------------------------------

            XmlNodeList Detalle = documentoXml.GetElementsByTagName("Detalle");           
            XmlNodeList NmbItem = documentoXml.GetElementsByTagName("NmbItem");
            XmlNodeList DscItem = documentoXml.GetElementsByTagName("DscItem");
            XmlNodeList QtyItem = documentoXml.GetElementsByTagName("QtyItem");
            XmlNodeList PrcItem = documentoXml.GetElementsByTagName("PrcItem");            
            XmlNodeList MontoItem = documentoXml.GetElementsByTagName("MontoItem");            

            List<int> listaDescuentos = new List<int>();
            List<int> listaRecargos = new List<int>();
            string precioItem = "--";
            string cantidadItem = "--";
            string descripcionItem = "";
            string strPrecioItem = "";

            for (int i = 0; i < Detalle.Count; i++)
            {
                var nodosDetalle = Detalle.Item(i).ChildNodes;
                
                //DEFINIMOS LA CULTURA EN LA QUE VIENE EL DATO
                CultureInfo cultureUS = new CultureInfo("en-US");
                if (PrcItem.Count != 0)
                {
                    //PARSEAMOS A DECIMAL CON LOS DECIMALES EN US, Y CONVERTIMOS A STRING CON SEPARADOR DE MILES N 
                     string precioItemTemp = PrcItem[i].InnerXml.ToString();
                     precioItemTemp = precioItemTemp.Replace('.',',');
                     precioItem = double.Parse(precioItemTemp).ToString();

                   
       
                   // double numEnero = double.Parse(precioItem.Split(',')[0]);
                    //double numDecimal = double.Parse(precioItem.Split(',')[1]);
                    
                    // SI EL DECIMAL ES 0, ENTONCES LO OMITE
                    if (precioItem.Contains(","))
                    {
                        precioItem = "$" + decimal.Parse(precioItem).ToString("N");
                    }
                    else
                    {                      
                        precioItem = String.Format("{0:C}", double.Parse(precioItem));
                    }
                   
                }

                // AVECES NO VIENE LA DESCRIPCION DEL ITEM (NOTAS DE CREDITO O DEBITO)
                if (DscItem.Count != 0)
                {
                    
                   descripcionItem = DscItem[i].InnerXml;//<<<<<<<<<<<<<<<<<<<<<<<<opcional
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

                string nombreDescripcion = NmbItem[i].InnerXml + " " + descripcionItem; //Juntando nombre y descripcion adicional del item

                PdfPCell celdaNmbItem = new PdfPCell(new Phrase(nombreDescripcion, _fuenteBlack)); 
                PdfPCell celdaQtyItem = new PdfPCell(new Phrase(cantidadItem, _fuenteBlack));
                PdfPCell celdaPrcItem = new PdfPCell(new Phrase(precioItem, _fuenteBlack));
                PdfPCell celdaMontoItem = new PdfPCell(new Phrase(subTotal, _fuenteBlack));
                PdfPCell celdaDRMontoItem = new PdfPCell(new Phrase(strDRItem, _fuenteBlack));

                celdaNmbItem.BorderColor = FontColourAzul2;
                celdaQtyItem.BorderColor = FontColourAzul2;
                celdaPrcItem.BorderColor = FontColourAzul2;
                celdaMontoItem.BorderColor = FontColourAzul2;
                celdaDRMontoItem.BorderColor = FontColourAzul2;

                celdaNmbItem.HorizontalAlignment = Element.ALIGN_LEFT;
                celdaNmbItem.VerticalAlignment = Element.ALIGN_MIDDLE;
                celdaQtyItem.HorizontalAlignment = Element.ALIGN_CENTER;
                celdaQtyItem.VerticalAlignment = Element.ALIGN_MIDDLE;

                if (precioItem == "--")
                {
                    celdaPrcItem.HorizontalAlignment = Element.ALIGN_CENTER;
                    celdaPrcItem.VerticalAlignment = Element.ALIGN_MIDDLE;
                }
                else
                {
                    celdaPrcItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                    celdaPrcItem.VerticalAlignment = Element.ALIGN_MIDDLE;
                }
                
                celdaMontoItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                celdaMontoItem.VerticalAlignment = Element.ALIGN_MIDDLE;
                celdaDRMontoItem.HorizontalAlignment = Element.ALIGN_CENTER;
                celdaDRMontoItem.VerticalAlignment = Element.ALIGN_MIDDLE;

                tablaDetalle.AddCell(celdaNmbItem);
                tablaDetalle.AddCell(celdaQtyItem);
                tablaDetalle.AddCell(celdaPrcItem);
                tablaDetalle.AddCell(celdaDRMontoItem);
                tablaDetalle.AddCell(celdaMontoItem);
            }

            documentoPDF.Add(tablaDetalle);
            
            //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
            // DESCUENTOS O RECARGOS GLOBALES -----------------------------------------------------------------------------------------------------------------------------------
            //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

            XmlNodeList DscRcgGlobalList = documentoXml.GetElementsByTagName("DscRcgGlobal");
            if (DscRcgGlobalList.Count != 0)
            {
                PdfPTable tablaDRGlobales = new PdfPTable(4);

                tablaDRGlobales.TotalWidth = 570f;
                tablaDRGlobales.LockedWidth = true;              

                PdfPCell celdaTituloDR = new PdfPCell(new Phrase("DESCUENTOS O RECARGOS GLOBALES", _fuenteWhiteBold));
                PdfPCell celdaSubTipo = new PdfPCell(new Phrase("Tipo", _fuenteAzul18Bold));
                PdfPCell celdaSubAplica = new PdfPCell(new Phrase("Aplica a", _fuenteAzul18Bold));
                PdfPCell celdaSubPorcentaje = new PdfPCell(new Phrase("%", _fuenteAzul18Bold));
                PdfPCell celdaSubMontoDR = new PdfPCell(new Phrase("Monto", _fuenteAzul18Bold));               
                PdfPCell celdaEmpty9 = new PdfPCell(new Phrase(" "));

                celdaEmpty9.BorderWidth = 0f;
                celdaEmpty9.Colspan = 4;
                celdaTituloDR.Colspan = 4;

                celdaTituloDR.BorderColor = FontColourAzul1;
                celdaSubTipo.BorderColor = FontColourAzul2;
                celdaSubAplica.BorderColor = FontColourAzul2;
                celdaSubPorcentaje.BorderColor = FontColourAzul2;
                celdaSubMontoDR.BorderColor = FontColourAzul2;
               
                celdaTituloDR.HorizontalAlignment = Element.ALIGN_CENTER;
                celdaTituloDR.VerticalAlignment = Element.ALIGN_MIDDLE;
                celdaSubTipo.HorizontalAlignment = Element.ALIGN_CENTER;
                celdaSubTipo.VerticalAlignment = Element.ALIGN_MIDDLE;
                celdaSubAplica.HorizontalAlignment = Element.ALIGN_CENTER;
                celdaSubAplica.VerticalAlignment = Element.ALIGN_MIDDLE;
                celdaSubPorcentaje.HorizontalAlignment = Element.ALIGN_CENTER;
                celdaSubPorcentaje.VerticalAlignment = Element.ALIGN_MIDDLE;
                celdaSubMontoDR.HorizontalAlignment = Element.ALIGN_CENTER;
                celdaSubMontoDR.VerticalAlignment = Element.ALIGN_MIDDLE;
               
                celdaTituloDR.BackgroundColor = FontColourAzul1;
                celdaSubTipo.BackgroundColor = FontColourNiebla;
                celdaSubAplica.BackgroundColor = FontColourNiebla;
                celdaSubPorcentaje.BackgroundColor = FontColourNiebla;
                celdaSubMontoDR.BackgroundColor = FontColourNiebla;

                tablaDRGlobales.AddCell(celdaEmpty9);
                tablaDRGlobales.AddCell(celdaTituloDR);

                tablaDRGlobales.AddCell(celdaSubTipo);
                tablaDRGlobales.AddCell(celdaSubAplica);
                tablaDRGlobales.AddCell(celdaSubPorcentaje);
                tablaDRGlobales.AddCell(celdaSubMontoDR);

                XmlNodeList DRGlobalList = documentoXml.GetElementsByTagName("DscRcgGlobal");
                XmlNodeList TpoMovList = documentoXml.GetElementsByTagName("TpoMov");
                XmlNodeList TpoValorList = documentoXml.GetElementsByTagName("TpoValor");
                XmlNodeList ValorDRList = documentoXml.GetElementsByTagName("ValorDR");
                XmlNodeList NetoList = documentoXml.GetElementsByTagName("MntNeto");

                // RESCATAR LOS MONTOSITEM DE CADA DETALLE Y METERLOS A UNA LISTA NORMAL(YA QUE LA NODELIST NO PERMITE SUMAR)
                var nsmgr = new XmlNamespaceManager(documentoXml.NameTable);
                nsmgr.AddNamespace("a", "http://www.sii.cl/SiiDte");                

                var nodosMontoItem = documentoXml.SelectNodes(@"//a:SetDTE/a:DTE/a:Documento/a:Detalle",nsmgr);
                List<int> listaMontoItem = new List<int>();               

                foreach (XmlNode item in nodosMontoItem)
                {
                    listaMontoItem.Add(int.Parse(item.SelectSingleNode("./a:MontoItem",nsmgr).InnerText));
                }

                double montoNetoOriginal = listaMontoItem.Sum();                
                double montoDRGloval = 0;
                string strPorcentaje = "--";

                //CONVIERTE LOS DESCUENTOS Y RECARGOS EN NUMEROS GRAFICOS
                for (int i = 0; i < DRGlobalList.Count; i++)
                {
                    string TpoMov = TpoMovList[i].InnerXml;
                    string TpoValor = TpoValorList[i].InnerXml;                    
                    double ValorDR = double.Parse(ValorDRList[i].InnerXml);                     

                    if (TpoValor == "%")
                    {
                        montoDRGloval = (ValorDR * montoNetoOriginal) / 100; ;
                        strPorcentaje = ValorDR + " %";
                    }
                    else
                    {
                        montoDRGloval = ValorDR;
                    }

                    if (TpoMov == "D")
                    {
                        TpoMov = "Descuento";
                        montoNetoOriginal = montoNetoOriginal - montoDRGloval;
                    }
                    if (TpoMov == "R")
                    {
                        TpoMov = "Recargo";
                        montoNetoOriginal = montoNetoOriginal + montoDRGloval;
                    }

                    string strValorDR = String.Format("{0:C}", montoDRGloval);

                    PdfPCell celdaTpoMov = new PdfPCell(new Phrase(TpoMov, _fuenteBlack));
                    PdfPCell celdaAplica = new PdfPCell(new Phrase("Montos Netos", _fuenteBlack));
                    PdfPCell celdaPorcentaje = new PdfPCell(new Phrase(strPorcentaje, _fuenteBlack));
                    PdfPCell celdaValorDR = new PdfPCell(new Phrase(strValorDR, _fuenteBlack));

                    celdaTpoMov.BorderColor = FontColourAzul2;
                    celdaAplica.BorderColor = FontColourAzul2;
                    celdaPorcentaje.BorderColor = FontColourAzul2;
                    celdaValorDR.BorderColor = FontColourAzul2;

                    celdaTpoMov.HorizontalAlignment = Element.ALIGN_CENTER;
                    celdaTpoMov.VerticalAlignment = Element.ALIGN_MIDDLE;
                    celdaAplica.HorizontalAlignment = Element.ALIGN_CENTER;
                    celdaAplica.VerticalAlignment = Element.ALIGN_MIDDLE;
                    celdaPorcentaje.HorizontalAlignment = Element.ALIGN_CENTER;
                    celdaPorcentaje.VerticalAlignment = Element.ALIGN_MIDDLE;
                    celdaValorDR.HorizontalAlignment = Element.ALIGN_RIGHT;
                    celdaValorDR.VerticalAlignment = Element.ALIGN_MIDDLE;

                    tablaDRGlobales.AddCell(celdaTpoMov);
                    tablaDRGlobales.AddCell(celdaAplica);
                    tablaDRGlobales.AddCell(celdaPorcentaje);
                    tablaDRGlobales.AddCell(celdaValorDR);

                }

                documentoPDF.Add(tablaDRGlobales);

            }

            //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
            // OBSERVACIONES -----------------------------------------------------------------------------------------------------------------------------------------------
            //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

            if (observaciones != "")
            {

                PdfPTable tablaObservaciones = new PdfPTable(3);

                tablaObservaciones.TotalWidth = 570f;
                tablaObservaciones.LockedWidth = true;

                float[] widthsObservaciones = new float[] { 4f, 1f, 1f };

                tablaObservaciones.SetWidths(widthsObservaciones);

                PdfPCell celdaTituloObservaciones = new PdfPCell(new Phrase("OBSERVACIONES", _fuenteWhiteBold));
                PdfPCell celdaObcervaciones = new PdfPCell(new Phrase(observaciones, _fuenteBlack));
                PdfPCell celdaEmpty3 = new PdfPCell(new Phrase(" "));

                celdaEmpty3.BorderWidth = 0f;
                celdaEmpty3.Colspan = 3;


                celdaTituloObservaciones.Colspan = 3;
                celdaObcervaciones.Colspan = 3;

                celdaTituloObservaciones.BackgroundColor = FontColourAzul1;
                celdaTituloObservaciones.BorderColor = FontColourAzul1;
                celdaObcervaciones.BorderColor = FontColourAzul2;

                celdaTituloObservaciones.HorizontalAlignment = Element.ALIGN_CENTER;
                celdaTituloObservaciones.VerticalAlignment = Element.ALIGN_MIDDLE;
                celdaObcervaciones.HorizontalAlignment = Element.ALIGN_LEFT;
                celdaObcervaciones.VerticalAlignment = Element.ALIGN_MIDDLE;

                tablaObservaciones.AddCell(celdaEmpty3);
                tablaObservaciones.AddCell(celdaTituloObservaciones);
                tablaObservaciones.AddCell(celdaObcervaciones);

                documentoPDF.Add(tablaObservaciones);

            }

            //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
            // TABLA TRANSPORTE (OPCIONAL)-----------------------------------------------------------------------------------------------------------------------------------
            //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

            XmlNodeList TransporteList = documentoXml.GetElementsByTagName("Transporte");

            if (TransporteList.Count != 0)
            {
                PdfPTable tablaTransporte = new PdfPTable(2);
                tablaTransporte.TotalWidth = 570f;
                tablaTransporte.LockedWidth = true;

                float[] widthsTransporte = new float[] { 1f, 3f };
                tablaTransporte.SetWidths(widthsTransporte);

                XmlNodeList DirDestList = documentoXml.GetElementsByTagName("DirDest");
                XmlNodeList CmnaDestList = documentoXml.GetElementsByTagName("CmnaDest");
                XmlNodeList IndTrasladoList = documentoXml.GetElementsByTagName("IndTraslado");
                XmlNodeList TipoDespachoList = documentoXml.GetElementsByTagName("TipoDespacho");


                string CmnaDest = "--";
                string IndTraslado = "--";
                string TipoDespacho = "--";
                string DirDest = "--";

                if (DirDestList.Count != 0)
                {
                   DirDest = DirDestList[0].InnerXml;
                }
                if (CmnaDestList.Count != 0)
                {
                    CmnaDest = CmnaDestList[0].InnerXml;
                }
                if (IndTrasladoList.Count != 0)
                {
                    IndTraslado = IndTrasladoList[0].InnerXml;
                }
                if (TipoDespachoList.Count != 0)
                {
                    TipoDespacho = TipoDespachoList[0].InnerXml;
                }



                if (IndTraslado == "1")
                {
                    IndTraslado = "Operación constituye venta";
                }
                if (IndTraslado == "2")
                {
                    IndTraslado = "Ventas por efectuar";
                }
                if (IndTraslado == "3")
                {
                    IndTraslado = "Consignaciones";
                }
                if (IndTraslado == "4")
                {
                    IndTraslado = "Entrega gratuita";
                }
                if (IndTraslado == "5")
                {
                    IndTraslado = "Traslados internos";
                }
                if (IndTraslado == "6")
                {
                    IndTraslado = "Otros traslados no venta";
                }
                if (IndTraslado == "7")
                {
                    IndTraslado = "Guía de devolución";
                }
                if (IndTraslado == "8")
                {
                    IndTraslado = "Traslado para exportación. (no venta)";
                }
                if (IndTraslado == "9")
                {
                    IndTraslado = "Venta para exportación";
                }

                 if (TipoDespacho == "1")
                {
                    TipoDespacho = "Despacho por cuenta del receptor del documento constituye venta";
                }
                if (TipoDespacho == "2")
                {
                    TipoDespacho = "Despacho por cuenta del emisor a instalaciones del cliente";
                }
                if (TipoDespacho == "3")
                {
                    TipoDespacho = "Despacho por cuenta del emisor a otras instalaciones";
                }

                PdfPCell celdaTituloTransporte = new PdfPCell(new Phrase("TRANSPORTE", _fuenteWhiteBold));
                PdfPCell celdaSubDestino = new PdfPCell(new Phrase("Dirección de Destino", _fuenteAzul18Bold));
                PdfPCell celdaSubTipoDespacho = new PdfPCell(new Phrase("Tipo de Despacho", _fuenteAzul18Bold));
                PdfPCell celdaSubIndicadorTras = new PdfPCell(new Phrase("Indicador de Traslado", _fuenteAzul18Bold));
                PdfPCell celdaDestino = new PdfPCell(new Phrase(DirDest + ", " + CmnaDest, _fuenteBlack));
                PdfPCell celdaTpoDespacho = new PdfPCell(new Phrase(TipoDespacho, _fuenteBlack));
                PdfPCell celdaIndicadorTras = new PdfPCell(new Phrase(IndTraslado, _fuenteBlack));
                PdfPCell celdaEmpty4 = new PdfPCell(new Phrase(" "));

                celdaEmpty4.BorderWidth = 0f;
                celdaEmpty4.Colspan = 2;

                celdaTituloTransporte.Colspan = 2;

                celdaTituloTransporte.BackgroundColor = FontColourAzul1;
                celdaTituloTransporte.BorderColor = FontColourAzul1;

                celdaSubDestino.BorderColor = FontColourAzul2;
                celdaSubTipoDespacho.BorderColor = FontColourAzul2;
                celdaSubIndicadorTras.BorderColor = FontColourAzul2;
                celdaDestino.BorderColor = FontColourAzul2;
                celdaTpoDespacho.BorderColor = FontColourAzul2;
                celdaIndicadorTras.BorderColor = FontColourAzul2;

                celdaSubDestino.BackgroundColor = FontColourNiebla;
                celdaSubTipoDespacho.BackgroundColor = FontColourNiebla;
                celdaSubIndicadorTras.BackgroundColor = FontColourNiebla;

                celdaTituloTransporte.HorizontalAlignment = Element.ALIGN_CENTER;
                celdaTituloTransporte.VerticalAlignment = Element.ALIGN_MIDDLE;
                celdaSubDestino.HorizontalAlignment = Element.ALIGN_LEFT;
                celdaSubDestino.VerticalAlignment = Element.ALIGN_MIDDLE;
                celdaSubTipoDespacho.HorizontalAlignment = Element.ALIGN_LEFT;
                celdaSubTipoDespacho.VerticalAlignment = Element.ALIGN_MIDDLE;
                celdaSubIndicadorTras.HorizontalAlignment = Element.ALIGN_LEFT;
                celdaSubIndicadorTras.VerticalAlignment = Element.ALIGN_MIDDLE;
                celdaDestino.HorizontalAlignment = Element.ALIGN_LEFT;
                celdaDestino.VerticalAlignment = Element.ALIGN_MIDDLE;
                celdaTpoDespacho.HorizontalAlignment = Element.ALIGN_LEFT;
                celdaTpoDespacho.VerticalAlignment = Element.ALIGN_MIDDLE;
                celdaIndicadorTras.HorizontalAlignment = Element.ALIGN_LEFT;
                celdaIndicadorTras.VerticalAlignment = Element.ALIGN_MIDDLE;

                tablaTransporte.AddCell(celdaEmpty4);
                tablaTransporte.AddCell(celdaTituloTransporte);

                tablaTransporte.AddCell(celdaSubDestino);
                tablaTransporte.AddCell(celdaDestino);
                tablaTransporte.AddCell(celdaSubTipoDespacho);
                tablaTransporte.AddCell(celdaTpoDespacho);
                tablaTransporte.AddCell(celdaSubIndicadorTras);
                tablaTransporte.AddCell(celdaIndicadorTras);

                documentoPDF.Add(tablaTransporte);


            }

            //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
            // REFERENCIA -------------------------------------------------------------------------------------------------------------------------------------------------
            //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

            XmlNodeList Referencia = documentoXml.GetElementsByTagName("Referencia");

            if (Referencia.Count != 0)
            {
                PdfPTable tablaReferencias = new PdfPTable(4);

                tablaReferencias.TotalWidth = 570f;
                tablaReferencias.LockedWidth = true;

                float[] widthsReferencia = new float[] { 4f, 3f, 2f, 4f };

                tablaReferencias.SetWidths(widthsReferencia);

                XmlNodeList TpoDocRefList = documentoXml.GetElementsByTagName("TpoDocRef");
                XmlNodeList FolioRefList = documentoXml.GetElementsByTagName("FolioRef");
                XmlNodeList FchRefList = documentoXml.GetElementsByTagName("FchRef");
                XmlNodeList CodRefList = documentoXml.GetElementsByTagName("CodRef");
                XmlNodeList RazonRefList = documentoXml.GetElementsByTagName("RazonRef");

                string TpoDocRef = TpoDocRefList[0].InnerXml;
                string FolioRef = FolioRefList[0].InnerXml;
                string FchRef = FchRefList[0].InnerXml;
                string RazonRef = "";

                // CONVERTIR A TEXTO LOS CODIGOS-----------------
                if (RazonRefList.Count != 0)
                {
                    RazonRef = RazonRefList[0].InnerXml;
                }
                else
                {
                    //MODIFICACION 24-02-2023 SE COLOCA OPCIONAL EL CODIGO DE REFERENCIA

                    if (CodRefList.Count != 0 && TpoDocRef != "801")
                    {
                        if (CodRefList[0].InnerXml == "1")
                        {
                            RazonRef = "Anula documento de referencia";
                        }
                        if (CodRefList[0].InnerXml == "2")
                        {
                            RazonRef = "Corrige texto documento de referencia";
                        }
                        if (CodRefList[0].InnerXml == "3")
                        {
                            RazonRef = "Corrige montos";
                        }
                    }
                    else
                    {
                        RazonRef = "Trazabilidad";
                    }
                }        

                if (TpoDocRef == "801")
                {
                    TpoDocRef = "Orden de Compra";
                }
                if (TpoDocRef == "33")
                {
                    TpoDocRef = "Factura Electrónica";
                }
                if (TpoDocRef == "34")
                {
                    TpoDocRef = "Factura No Afecta o Exenta Electrónica";
                }
                if (TpoDocRef == "39")
                {
                    TpoDocRef = "Boleta Electrónica";
                }
                if (TpoDocRef == "41")
                {
                    TpoDocRef = " Boleta Exenta Electrónica";
                }
                if (TpoDocRef == "56")
                {
                    TpoDocRef = "Nota de Débito Electrónica";
                }
                if (TpoDocRef == "61")
                {
                    TpoDocRef = "Nota de Crédito Electrónica";
                }
                if (TpoDocRef == "52")
                {
                    TpoDocRef = "Guía de Despacho Electrónica";
                }

                PdfPCell celdaTituloRef = new PdfPCell(new Phrase("REFERENCIAS", _fuenteWhiteBold));

                celdaTituloRef.Colspan = 4;
                celdaTituloRef.HorizontalAlignment = 1;

                PdfPCell celdaSubDocumento = new PdfPCell(new Phrase("Documento", _fuenteAzul18Bold));
                PdfPCell celdaSubFolio = new PdfPCell(new Phrase("Folio", _fuenteAzul18Bold));
                PdfPCell celdaSubFecha = new PdfPCell(new Phrase("Fecha Emisión", _fuenteAzul18Bold));
                PdfPCell celdaSubRazon = new PdfPCell(new Phrase("Razón Referencia", _fuenteAzul18Bold));
                PdfPCell celdaTpoDocRef = new PdfPCell(new Phrase(TpoDocRef, _fuenteBlack));
                PdfPCell celdaFolioRef = new PdfPCell(new Phrase(FolioRef, _fuenteBlack));
                PdfPCell celdaFchRef = new PdfPCell(new Phrase(FchRef, _fuenteBlack));
                PdfPCell celdaRazonRef = new PdfPCell(new Phrase(RazonRef, _fuenteBlack));
                PdfPCell celdaEmpty5 = new PdfPCell(new Phrase(" "));

                celdaEmpty5.BorderWidth = 0f;
                celdaEmpty5.Colspan = 4;

                celdaSubDocumento.BorderColor = FontColourAzul2;
                celdaSubFolio.BorderColor = FontColourAzul2;
                celdaSubFecha.BorderColor = FontColourAzul2;
                celdaSubRazon.BorderColor = FontColourAzul2;

                celdaSubDocumento.HorizontalAlignment = Element.ALIGN_CENTER;
                celdaSubDocumento.VerticalAlignment = Element.ALIGN_MIDDLE;
                celdaSubFolio.HorizontalAlignment = Element.ALIGN_CENTER;
                celdaSubFolio.VerticalAlignment = Element.ALIGN_MIDDLE;
                celdaSubFecha.HorizontalAlignment = Element.ALIGN_CENTER;
                celdaSubFecha.VerticalAlignment = Element.ALIGN_MIDDLE;
                celdaSubRazon.HorizontalAlignment = Element.ALIGN_CENTER;
                celdaSubRazon.VerticalAlignment = Element.ALIGN_MIDDLE;
                celdaTpoDocRef.HorizontalAlignment = Element.ALIGN_CENTER;
                celdaTpoDocRef.VerticalAlignment = Element.ALIGN_MIDDLE;
                celdaFolioRef.HorizontalAlignment = Element.ALIGN_CENTER;
                celdaFolioRef.VerticalAlignment = Element.ALIGN_MIDDLE;
                celdaFchRef.HorizontalAlignment = Element.ALIGN_CENTER;
                celdaFchRef.VerticalAlignment = Element.ALIGN_MIDDLE;
                celdaRazonRef.HorizontalAlignment = Element.ALIGN_CENTER;
                celdaRazonRef.VerticalAlignment = Element.ALIGN_MIDDLE;

                celdaSubDocumento.BackgroundColor = FontColourNiebla;
                celdaSubFolio.BackgroundColor = FontColourNiebla;
                celdaSubFecha.BackgroundColor = FontColourNiebla;
                celdaSubRazon.BackgroundColor = FontColourNiebla;

                celdaTpoDocRef.BorderColor = FontColourAzul2;
                celdaFolioRef.BorderColor = FontColourAzul2;
                celdaFchRef.BorderColor = FontColourAzul2;
                celdaRazonRef.BorderColor = FontColourAzul2;

                celdaTituloRef.BackgroundColor = FontColourAzul1;

                celdaTituloRef.BorderColor = FontColourAzul1;               

                tablaReferencias.AddCell(celdaEmpty5);
                tablaReferencias.AddCell(celdaTituloRef);

                tablaReferencias.AddCell(celdaSubDocumento);
                tablaReferencias.AddCell(celdaSubFolio);
                tablaReferencias.AddCell(celdaSubFecha);
                tablaReferencias.AddCell(celdaSubRazon);
                tablaReferencias.AddCell(celdaTpoDocRef);
                tablaReferencias.AddCell(celdaFolioRef);
                tablaReferencias.AddCell(celdaFchRef);
                tablaReferencias.AddCell(celdaRazonRef);

                documentoPDF.Add(tablaReferencias);
            }

            //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
            // TABLA TIMBRE Y RESUMEN TOTALES ------------------------------------------------------------------------------------------------------------------------------
            //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

            PdfPTable tablaFooter = new PdfPTable(3);
            PdfPTable tablaTimbre = new PdfPTable(1);
            PdfPTable tablaTotales = new PdfPTable(3);

            // TAMAÑO DE TABLAS
            tablaFooter.TotalWidth = 545f;
            tablaFooter.LockedWidth = true;            

            // PROPORCIONES RELATIVAS DE COLUMNAS (FRACCIONES)
            float[] widthsFooter = new float[] { 22f, 30f, 25f };
            tablaFooter.SetWidths(widthsFooter);
            float[] widthsTotales = new float[] { 4f, 1f, 4f };
            tablaTotales.SetWidths(widthsTotales);

            // TIMBRE
            // Creamos la imagen y le ajustamos el tamaño

            //ENVIAMOS EL STRING DEL NODO TED Y EL FOLIO DEL DTE Y EL TIPO DTE
            string[] respuesta_timbre = new string[] { };
            DTE dte = new DTE();

            //CAPTURAR EL NODO TED DEL XML <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
            
            XmlNodeList DocumentoList = documentoXml.GetElementsByTagName("Documento");
            var nodoDocumento = DocumentoList.Item(0).ChildNodes;
            string nodoTed = nodoDocumento.Item(nodoDocumento.Count - 2).OuterXml;

            respuesta_timbre = dte.generarTimbrePDF417(nodoTed, Folio, strTipoDTE);

            iTextSharp.text.Image imagen2 = iTextSharp.text.Image.GetInstance(respuesta_timbre[1]);
            imagen2.BorderWidth = 0;
            imagen2.Alignment = Element.ALIGN_MIDDLE;
            float percentage2 = 0.0f;
            percentage2 = 155 / imagen2.Width;
            imagen2.ScalePercent(percentage2 * 100);         

            XmlNodeList MntNetoList = documentoXml.GetElementsByTagName("MntNeto");
            XmlNodeList IVAList = documentoXml.GetElementsByTagName("IVA");
            XmlNodeList MntTotalList = documentoXml.GetElementsByTagName("MntTotal");
            XmlNodeList MntExeList = documentoXml.GetElementsByTagName("MntExe");

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
            string strCedible = "CEDIBLE";

            if (TipoDTE == "GUÍA DE DESPACHO ELECTRÓNICA")
            {
                strCedible = "CEDIBLE CON SU FACTURA";
            }

            // CREAMOS CELDAS CON FORMATO
            PdfPCell celdaTimbre = new PdfPCell(imagen2);
            PdfPCell celdaLeyenda = new PdfPCell(new Phrase(Leyenda, _fuenteBlack6));
            PdfPCell celdaSubMonto = new PdfPCell(new Phrase("MONTO NETO", _fuenteAzul19));
            PdfPCell celdaSubExento = new PdfPCell(new Phrase("MONTO EXENTO", _fuenteAzul19));
            PdfPCell celdaSubIVA = new PdfPCell(new Phrase("IVA 19%", _fuenteAzul19));
            PdfPCell celdaSubTotal = new PdfPCell(new Phrase("TOTAL", _fuenteWhiteBold));
            PdfPCell celdaSubSeparadorW = new PdfPCell(new Phrase("$", _fuenteWhiteBold));
            PdfPCell celdaSubSeparadorBTop = new PdfPCell(new Phrase("$", _fuenteBlack));
            PdfPCell celdaSubSeparadorBBot = new PdfPCell(new Phrase("$", _fuenteBlack));
            PdfPCell celdaMonto = new PdfPCell(new Phrase(MntNeto, _fuenteBlack));
            PdfPCell celdaMontoExento = new PdfPCell(new Phrase(MntExe, _fuenteBlack));
            PdfPCell celdaIVA = new PdfPCell(new Phrase(IVA, _fuenteBlack));
            PdfPCell celdaTotal = new PdfPCell(new Phrase(MntTotal, _fuenteWhiteBold));
            PdfPCell celdaTablaTotales = new PdfPCell(tablaTotales);
            PdfPCell celdaTablaTimbre = new PdfPCell(tablaTimbre);
            PdfPCell celdaCedible = new PdfPCell(new Phrase(strCedible, _fuenteBlackBold));
            PdfPCell celdaEmpty6 = new PdfPCell(new Phrase(" "));
            PdfPCell celdaEmpty7 = new PdfPCell(new Phrase(" "));

            celdaEmpty6.BorderWidth = 0f;
            celdaEmpty6.Colspan = 3;
            celdaEmpty7.BorderWidth = 0f;
            celdaEmpty7.Colspan = 1;
            celdaCedible.Colspan = 3;

            celdaTablaTimbre.BorderWidth = 0f;
            celdaTablaTotales.BorderWidth = 0f;
            celdaTimbre.BorderWidth = 0f;
            celdaLeyenda.BorderWidth = 0f;
            celdaSubSeparadorBBot.BorderWidth = 0f;
            celdaCedible.BorderWidth = 0f;


            celdaSubMonto.BorderColor = FontColourAzul2;
            celdaSubExento.BorderColor = FontColourAzul2;
            celdaSubIVA.BorderColor = FontColourAzul2;
            celdaSubTotal.BorderColor = FontColourAzul1;
            celdaSubSeparadorW.BorderColor = FontColourAzul1;
            celdaSubSeparadorBTop.BorderColor = FontColourAzul2;
            celdaMonto.BorderColor = FontColourAzul2;
            celdaMontoExento.BorderColor = FontColourAzul2;
            celdaIVA.BorderColor = FontColourAzul2;
            celdaTotal.BorderColor = FontColourAzul1;            

            celdaLeyenda.HorizontalAlignment = Element.ALIGN_CENTER;
            celdaLeyenda.VerticalAlignment = Element.ALIGN_MIDDLE;

            celdaSubMonto.HorizontalAlignment = Element.ALIGN_RIGHT;
            celdaSubMonto.VerticalAlignment = Element.ALIGN_MIDDLE;
            celdaSubExento.HorizontalAlignment = Element.ALIGN_RIGHT;
            celdaSubExento.VerticalAlignment = Element.ALIGN_MIDDLE;
            celdaSubIVA.HorizontalAlignment = Element.ALIGN_RIGHT;
            celdaSubIVA.VerticalAlignment = Element.ALIGN_MIDDLE;
            celdaSubTotal.HorizontalAlignment = Element.ALIGN_RIGHT;
            celdaSubTotal.VerticalAlignment = Element.ALIGN_MIDDLE;
            celdaSubSeparadorW.HorizontalAlignment = Element.ALIGN_CENTER;
            celdaSubSeparadorW.VerticalAlignment = Element.ALIGN_MIDDLE;
            celdaSubSeparadorBTop.HorizontalAlignment = Element.ALIGN_CENTER;
            celdaSubSeparadorBTop.VerticalAlignment = Element.ALIGN_MIDDLE;
            celdaSubSeparadorBBot.HorizontalAlignment = Element.ALIGN_CENTER;
            celdaSubSeparadorBBot.VerticalAlignment = Element.ALIGN_MIDDLE;
            celdaMonto.HorizontalAlignment = Element.ALIGN_RIGHT;
            celdaMonto.VerticalAlignment = Element.ALIGN_MIDDLE;
            celdaMontoExento.HorizontalAlignment = Element.ALIGN_RIGHT;
            celdaMontoExento.VerticalAlignment = Element.ALIGN_MIDDLE;
            celdaIVA.HorizontalAlignment = Element.ALIGN_RIGHT;
            celdaIVA.VerticalAlignment = Element.ALIGN_MIDDLE;
            celdaTotal.HorizontalAlignment = Element.ALIGN_RIGHT;
            celdaTotal.VerticalAlignment = Element.ALIGN_MIDDLE;
            celdaCedible.HorizontalAlignment = Element.ALIGN_RIGHT;


            celdaSubMonto.Border = Rectangle.TOP_BORDER | Rectangle.LEFT_BORDER;
            celdaSubExento.Border = Rectangle.TOP_BORDER | Rectangle.LEFT_BORDER;
            celdaSubIVA.Border = Rectangle.LEFT_BORDER;
            celdaSubTotal.Border = Rectangle.LEFT_BORDER | Rectangle.BOTTOM_BORDER;
            celdaSubSeparadorW.Border = Rectangle.BOTTOM_BORDER;
            celdaSubSeparadorBTop.Border = Rectangle.TOP_BORDER;
            celdaMonto.Border = Rectangle.RIGHT_BORDER | Rectangle.TOP_BORDER;
            celdaMontoExento.Border = Rectangle.RIGHT_BORDER | Rectangle.TOP_BORDER;
            celdaIVA.Border = Rectangle.RIGHT_BORDER;
            celdaTotal.Border = Rectangle.RIGHT_BORDER | Rectangle.BOTTOM_BORDER;

            celdaSubTotal.BackgroundColor = FontColourAzul1;
            celdaSubSeparadorW.BackgroundColor = FontColourAzul1;
            celdaTotal.BackgroundColor = FontColourAzul1;

            tablaTimbre.AddCell(celdaTimbre);
            tablaTimbre.AddCell(celdaLeyenda);

            tablaTotales.AddCell(celdaEmpty6);

            if (MntExeList.Count == 0 || MntExed == 0)
            {
                tablaTotales.AddCell(celdaSubMonto);
                tablaTotales.AddCell(celdaSubSeparadorBTop);
                tablaTotales.AddCell(celdaMonto);
                tablaTotales.AddCell(celdaSubIVA);
                tablaTotales.AddCell(celdaSubSeparadorBBot);
                tablaTotales.AddCell(celdaIVA);               
            }
            else
            {
                if (MntNetoList.Count != 0)
                {
                    tablaTotales.AddCell(celdaSubMonto);
                    tablaTotales.AddCell(celdaSubSeparadorBTop);
                    tablaTotales.AddCell(celdaMonto);
                    tablaTotales.AddCell(celdaSubExento);
                    tablaTotales.AddCell(celdaSubSeparadorBTop);
                    tablaTotales.AddCell(celdaMontoExento);
                    tablaTotales.AddCell(celdaSubIVA);
                    tablaTotales.AddCell(celdaSubSeparadorBBot);
                    tablaTotales.AddCell(celdaIVA);                    
                }
                else
                {
                    tablaTotales.AddCell(celdaSubExento);
                    tablaTotales.AddCell(celdaSubSeparadorBTop);
                    tablaTotales.AddCell(celdaMontoExento);
                }
            }

            tablaTotales.AddCell(celdaSubTotal);
            tablaTotales.AddCell(celdaSubSeparadorW);
            tablaTotales.AddCell(celdaTotal);
            tablaTotales.AddCell(celdaEmpty6);
          
            tablaFooter.AddCell(celdaTablaTimbre);
            tablaFooter.AddCell(celdaEmpty7);
            tablaFooter.AddCell(celdaTablaTotales);
            

            if (pagina == 1)
            {
                if (TipoDTE == "NOTA DE DÉBITO ELECTRÓNICA" || TipoDTE == "NOTA DE CRÉDITO ELECTRÓNICA")
                {
                    tablaFooter.WriteSelectedRows(0, 50, documentoPDF.Left + 23, documentoPDF.Bottom + 90, writer.DirectContent);
                }
                else
                {
                    tablaFooter.AddCell(celdaCedible);
                    tablaFooter.WriteSelectedRows(0, 50, documentoPDF.Left + 23, documentoPDF.Bottom + 90, writer.DirectContent);                    
                }
               
            }
            else
            {
                if (TipoDTE == "NOTA DE DÉBITO ELECTRÓNICA" || TipoDTE == "NOTA DE CRÉDITO ELECTRÓNICA")
                {
                    tablaFooter.WriteSelectedRows(0, 50, documentoPDF.Left + 23, documentoPDF.Bottom + 90, writer.DirectContent);
                }
                else
                {
                    tablaFooter.WriteSelectedRows(0, 50, documentoPDF.Left + 23, documentoPDF.Bottom + 140, writer.DirectContent); // POSICION DE LA TABLA (BOTTOM CON 2CM DE LA IZQUIERDA)
                }
                
            }

            //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
            //RECUADRO ACUSE RECIBO---------------------------------------------------------------------------------------------------------------------------------------
            //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

            if (TipoDTE == "NOTA DE DÉBITO ELECTRÓNICA" || TipoDTE == "NOTA DE CRÉDITO ELECTRÓNICA")
            { }
            else
            { 
                PdfPTable tablaAcuse = new PdfPTable(6);

                tablaAcuse.TotalWidth = 570f;
                tablaAcuse.LockedWidth = true;

                float[] widthsAcuse = new float[] { 1f, 3f, 1f, 3f, 1f, 3f };
                tablaAcuse.SetWidths(widthsAcuse);

                string strAcuse = "\"El acuse de recibo que se declara en este acto, de acuerdo a lo dispuesto en la letra b) del art. 4°, y la letra c) del Art. 5° de la ley 19.983, acredita que la entrega de mercaderías o servicio(s) prestado(s) ha(n) sido recibido(s)\"";
                
                // CREAMOS CELDAS CON FORMATO
                PdfPCell celdaSubNombre = new PdfPCell(new Phrase("Nombre:", _fuenteBlack));
                PdfPCell celdaNombre = new PdfPCell(new Phrase("________________________", _fuenteBlack));
                PdfPCell celdaSubRUT = new PdfPCell(new Phrase("Rut:", _fuenteBlack));
                PdfPCell celdaRUT = new PdfPCell(new Phrase("________________________", _fuenteBlack));
                PdfPCell celdaEspacio = new PdfPCell();
                PdfPCell celdaSubRecinto = new PdfPCell(new Phrase("Recinto:", _fuenteBlack));
                PdfPCell celdaRecinto = new PdfPCell(new Phrase("________________________", _fuenteBlack));
                PdfPCell celdaSubFecha2 = new PdfPCell(new Phrase("Fecha:", _fuenteBlack));
                PdfPCell celdaFecha2 = new PdfPCell(new Phrase("________________________", _fuenteBlack));
                PdfPCell celdaSubFirma = new PdfPCell(new Phrase("Firma:", _fuenteBlack));
                PdfPCell celdaFirma = new PdfPCell(new Phrase("________________________", _fuenteBlack));
                PdfPCell celdaStrAcuse = new PdfPCell(new Phrase(strAcuse, _fuenteBlack));
               
                PdfPCell celdaEmpty8 = new PdfPCell(new Phrase(" "));

                celdaEmpty8.BorderWidth = 0f;
                celdaEmpty8.Colspan = 6;

                celdaRecinto.BorderWidth = 0f;
                celdaSubFecha2.BorderWidth = 0f;
                celdaSubFirma.BorderWidth = 0f;
                celdaFecha2.BorderWidth = 0f;
                celdaStrAcuse.BorderWidthTop = 0f;
                

                celdaNombre.HorizontalAlignment = Element.ALIGN_CENTER;
                celdaRUT.HorizontalAlignment = Element.ALIGN_CENTER;
                celdaRecinto.HorizontalAlignment = Element.ALIGN_CENTER;
                celdaFecha2.HorizontalAlignment = Element.ALIGN_CENTER;
                celdaFirma.HorizontalAlignment = Element.ALIGN_CENTER;
                

                celdaSubNombre.Border = Rectangle.TOP_BORDER | Rectangle.LEFT_BORDER;
                celdaNombre.Border = Rectangle.TOP_BORDER;              
                celdaSubRUT.Border = Rectangle.TOP_BORDER;
                celdaRUT.Border = Rectangle.TOP_BORDER;
                celdaSubRecinto.Border = Rectangle.LEFT_BORDER;
                celdaEspacio.Border = Rectangle.RIGHT_BORDER | Rectangle.TOP_BORDER;
                celdaFirma.Border = Rectangle.RIGHT_BORDER;

                celdaStrAcuse.Colspan = 6;               
                celdaEspacio.Colspan = 2;                

                if (pagina == 0)
                {
                    tablaAcuse.AddCell(celdaSubNombre);
                    tablaAcuse.AddCell(celdaNombre);
                    tablaAcuse.AddCell(celdaSubRUT);
                    tablaAcuse.AddCell(celdaRUT);
                    tablaAcuse.AddCell(celdaEspacio);
                    tablaAcuse.AddCell(celdaSubRecinto);
                    tablaAcuse.AddCell(celdaRecinto);
                    tablaAcuse.AddCell(celdaSubFecha2);
                    tablaAcuse.AddCell(celdaFecha2);
                    tablaAcuse.AddCell(celdaSubFirma);
                    tablaAcuse.AddCell(celdaFirma);
                    tablaAcuse.AddCell(celdaStrAcuse);

                    tablaAcuse.WriteSelectedRows(0, 50, documentoPDF.Left, documentoPDF.Bottom + 55, writer.DirectContent);
                }
               
            }
        }
    }
}
