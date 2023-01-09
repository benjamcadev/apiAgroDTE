using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace ApiAgroDTE.Clases
{
    public class Boleta
    {
        string archxml = "";
        public string crearBoleta(JsonElement Encabezado, JsonElement Detalles, JsonElement ReferenciaOp, Boolean RefFlag, JsonElement DscRcgGlobalOp, Boolean DscRcgFlag, int nuevoFolio, string archxml, string T39B)
        {

            //------------------------------------------------------------------------------------------------------------------
            //CAPTURAR DATOS EN VARIABLES----------------<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
            //------------------------------------------------------------------------------------------------------------------
            JsonElement IdDoc = Encabezado.GetProperty("IdDoc");

            string TipoDTE = IdDoc.GetProperty("TipoDTE").ToString();// Requerido
            string Folio = nuevoFolio.ToString();// Requerido
            string FchEmis = IdDoc.GetProperty("FchEmis").ToString();// Requerido
            string IndServicio = IdDoc.GetProperty("IndServicio").ToString();// Requerido
                                                                         //SPLITEAT LA FECHA PARA SEPARAR DATOS


            JsonElement Emisor = Encabezado.GetProperty("Emisor");

            string RUTEmisor = Emisor.GetProperty("RUTEmisor").ToString();// Requerido
            string RznSocEmisor = Emisor.GetProperty("RznSocEmisor").ToString();// Requerido
            string GiroEmisor = Emisor.GetProperty("GiroEmisor").ToString();// Requerido
           
            string DirOrigen = Emisor.GetProperty("DirOrigen").ToString();// Requerido
            string CmnaOrigen = Emisor.GetProperty("CmnaOrigen").ToString();// Requerido

            string CiudadOrigenOp = "";//Opcional
            if (Emisor.TryGetProperty("CiudadOrigen", out var CiudadOrigen))
            {
                CiudadOrigenOp = CiudadOrigen.ToString();
            }

            string TelefonoOp = "";//Opcional
            if (Emisor.TryGetProperty("Telefono", out var Telefono))
            {
                TelefonoOp = Telefono.ToString();
            }

            string CdgSIISucurOp = ""; //opcional
            if (Emisor.TryGetProperty("CdgSIISucur", out var CdgSIISucur))
            {
                CdgSIISucurOp = CdgSIISucur.ToString();
            }

          
            JsonElement Receptor = Encabezado.GetProperty("Receptor");

            string RUTRecep = "66666666-6";// Requerido
            string RznSocRecep = "66666666-6";
           




            JsonElement Totales = Encabezado.GetProperty("Totales");
            string MntNeto = Totales.GetProperty("MntNeto").ToString();
            string TasaIVA = "19";
            string IVA = Totales.GetProperty("IVA").ToString();
            string MntTotal = Totales.GetProperty("MntTotal").ToString();



            //CAPTURA DATOS DEL CAF---------------<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<            

            Caf caf = new Caf();
            string[] stringArray_result = caf.rescatarCaf(TipoDTE);
            string rutaCaf = stringArray_result[0];
            string cafStr = stringArray_result[1];
            string fechahhmm = stringArray_result[2];
            string RE = stringArray_result[3];
            string RS = stringArray_result[4];
            string TD = stringArray_result[5];
            string D = stringArray_result[6];
            string H = stringArray_result[7];
            string FA = stringArray_result[8];
            string M = stringArray_result[9];
            string E = stringArray_result[10];
            string IDK = stringArray_result[11];
            string FRMA = stringArray_result[12];
            string pk = stringArray_result[13];

            string[] stringCrearDD_result = caf.crearDD(rutaCaf, Detalles, Encabezado, TipoDTE, Folio, cafStr, fechahhmm, MntTotal);
            string Algoritmo = stringCrearDD_result[0];
            string strDD = stringCrearDD_result[1];


            //------------------------------------------------------------------------------------------------------------------
            //CONSTRUIR XML------------------------------<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
            //------------------------------------------------------------------------------------------------------------------

            XmlTextWriter writer;
            XNamespace xmlns = XNamespace.Get("http://www.sii.cl/SiiDte");

            string file_path = archxml;
            writer = new XmlTextWriter(file_path, Encoding.GetEncoding("ISO-8859-1"));


            //ENCABEZADO----------------------------------<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<


            writer.Formatting = System.Xml.Formatting.Indented;
            writer.Indentation = 0;
            writer.WriteStartDocument();
            //    writer.WriteProcessingInstruction("xml","version=\"1.0\" encoding=\"ISO-8859-1\"");
            writer.WriteStartElement("DTE");


            //  writer.WriteAttributeString("xmlns", null, null, "http://www.sii.cl/SiiDte");
            writer.WriteAttributeString("version", "1.0");
            writer.WriteStartElement("Documento");
            writer.WriteAttributeString("ID", T39B);

            writer.WriteStartElement("Encabezado");

            writer.WriteStartElement("IdDoc");
            writer.WriteElementString("TipoDTE", TipoDTE);
            writer.WriteElementString("Folio", Folio);
            writer.WriteElementString("FchEmis", FchEmis);
            writer.WriteElementString("IndServicio", IndServicio);
            writer.WriteEndElement();

            writer.WriteStartElement("Emisor");
            writer.WriteElementString("RUTEmisor", RUTEmisor);
            writer.WriteElementString("RznSocEmisor", RznSocEmisor);
            writer.WriteElementString("GiroEmisor", GiroEmisor);
            
           
            if (!string.IsNullOrEmpty(CdgSIISucurOp))
            {
                writer.WriteElementString("CdgSIISucur", CdgSIISucurOp);
            }
            writer.WriteElementString("DirOrigen", DirOrigen);
            writer.WriteElementString("CmnaOrigen", CmnaOrigen);
           
            writer.WriteEndElement();

            writer.WriteStartElement("Receptor");
            writer.WriteElementString("RUTRecep", RUTRecep);
        
            writer.WriteEndElement();


            writer.WriteStartElement("Totales");
            writer.WriteElementString("MntNeto", MntNeto);
            //writer.WriteElementString("TasaIVA", TasaIVA);
            writer.WriteElementString("IVA", IVA);
            writer.WriteElementString("MntTotal", MntTotal);
           
            writer.WriteEndElement();
            writer.WriteEndElement();

            //DETALLE----------------------------------<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

            //RESCATAR EL OBJECTO DETALLE
            JArray detallesArray = JArray.Parse(Detalles.ToString());
            //var cantidadDetalles = detallesArray.Count();


            // CREAMOS UNA LISTA PARA METER LOS DATOS DE DETALLE 
            List<object> ListaDetalles = new List<object>();
            List<double> ListaMontoItemStr = new List<double>();

            //Recorro los detalles enviados y los agrego a una lista
            foreach (var detalle in detallesArray)
            {
                ListaDetalles.Add(detalle);
            }

            //ENVIAMOS LA LISTA PARA REALIZAR OPERACIONES Y VALIDAR CUADRE DE MONTOS




            for (int i = 0; i < detallesArray.Count(); i++)
            {

                Detalle detalleObject = JsonConvert.DeserializeObject<Detalle>(ListaDetalles[i].ToString());

                // EJEMPLO DE APLICAR detalleObject.NroLinDet

                var NroLinDetStr = detalleObject.NroLinDet.ToString();
                var NmbItemStr = detalleObject.NmbItem.ToString();
                var QtyItemStr = detalleObject.QtyItem.ToString();
                var PrcItemStr = detalleObject.PrcItem.ToString();
              string DescuentoPct = "";
                string DescuentoMonto = "";
                string RecargoPct = "";
                string RecargoMonto = "";
                

                if (detalleObject.DescuentoPct is not null)
                {
                    DescuentoPct = detalleObject.DescuentoPct.ToString();
                }
                if (detalleObject.DescuentoMonto is not null)
                {
                    DescuentoMonto = detalleObject.DescuentoMonto.ToString();
                }
                if (detalleObject.RecargoPct is not null)
                {
                    RecargoPct = detalleObject.RecargoPct.ToString();
                }
                if (detalleObject.RecargoMonto is not null)
                {
                    RecargoMonto = detalleObject.RecargoMonto.ToString();
                }
                var MontoItemStr = detalleObject.MontoItem.ToString();




                QtyItemStr = QtyItemStr.Replace(",",".");
                PrcItemStr = PrcItemStr.Replace(",",".");
                DescuentoPct = DescuentoPct.Replace(",",".");
                DescuentoMonto = DescuentoMonto.Replace(",",".");
                RecargoPct = RecargoPct.Replace(",",".");
                RecargoMonto = RecargoMonto.Replace(",",".");
                MontoItemStr = MontoItemStr.Replace(",",".");    


                writer.WriteStartElement("Detalle");
                writer.WriteElementString("NroLinDet", NroLinDetStr);

                //SI EL DETALLE TRAE <CdgItem> LO CAPTURAMOS E INSERTAMOS -----------<<<<<<<<<<<<<<<<<<<

                CdgItem CdgItemOp = new CdgItem();
                Boolean CdgItemFlag = false;

                if (!(detalleObject.CdgItem == null))
                {
                    CdgItemOp = detalleObject.CdgItem;//ESTO LO COMENTEEEEEEE
                    CdgItemFlag = true;
                }

                if (CdgItemFlag == true)
                {

                    var TpoCodigoStr = CdgItemOp.TpoCodigo.ToString();
                    var VlrCodigoStr = CdgItemOp.VlrCodigo.ToString();


                    writer.WriteStartElement("CdgItem");
                    writer.WriteElementString("TpoCodigo", TpoCodigoStr);
                    writer.WriteElementString("VlrCodigo", VlrCodigoStr);
                    writer.WriteEndElement();
                }

                writer.WriteElementString("NmbItem", NmbItemStr);
                writer.WriteElementString("QtyItem", QtyItemStr);
                writer.WriteElementString("PrcItem", PrcItemStr);

                // EN CASO DE QUE UN PRODUCTO TRAIGA DESCUENTO O RECARGO--------------------------------------------------------------

                SubDscto SubDsctoOp = new SubDscto();
                SubRecargo SubRecargoOp = new SubRecargo();
                Boolean SubDsctoFlag = false;
                Boolean SubRecargoFlag = false;

                if (!(detalleObject.DescuentoMonto == null))
                {
                    SubDsctoOp = detalleObject.SubDscto;
                    SubDsctoFlag = true;

                    if (!(detalleObject.DescuentoPct == null))
                    {
                        writer.WriteElementString("DescuentoPct", DescuentoPct);
                    }
                    writer.WriteElementString("DescuentoMonto", DescuentoMonto);

                }
                if (SubDsctoFlag == true)
                {
                    if (SubDsctoOp is not null)
                    {
                        var TipoDscto = SubDsctoOp.TipoDscto.ToString();
                        var ValorDscto = SubDsctoOp.ValorDscto.ToString();

                        writer.WriteStartElement("SubDscto");
                        writer.WriteElementString("TipoDscto", TipoDscto);
                        writer.WriteElementString("ValorDscto", ValorDscto);
                        writer.WriteEndElement();
                    }
                }
                if (!(detalleObject.RecargoMonto == null))
                {
                    SubRecargoOp = detalleObject.SubRecargo;
                    SubRecargoFlag = true;

                    if (!(detalleObject.RecargoPct == null))
                    {
                        writer.WriteElementString("RecargoPct", RecargoPct);
                    }
                    writer.WriteElementString("RecargoMonto", RecargoMonto);
                }

                if (SubRecargoFlag == true)
                {

                    var TipoRecargo = SubRecargoOp.TipoRecargo.ToString();
                    var ValorRecargo = SubRecargoOp.ValorRecargo.ToString();

                    writer.WriteStartElement("SubRecargo");
                    writer.WriteElementString("TipoRecargo", TipoRecargo);
                    writer.WriteElementString("ValorRecargo", ValorRecargo);
                    writer.WriteEndElement();

                }

                //----------------------------------------------------------------------------------
                writer.WriteElementString("MontoItem", MontoItemStr);
                writer.WriteEndElement();

                ListaMontoItemStr.Add(double.Parse(MontoItemStr));


            }



            // EN CASO DE QUE LA FACTURA VENGA CON UN DESCUENTO O RECARGO GLOBAL----------------<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
            List<object> ListaDscRcg = new List<object>();
            if (DscRcgFlag == true)
            {
                //MODIFICACION: 19-04-2022: DGZ MANDA SOLO 1 OBJECT DE DESCUENTOS Y NO UN ARRAYS CON VARIOS DESCUENTOS COMO OBJECT DENTRO, POR LO TANTO TUVE QUE AGREGAR "[" "]" AL COMIENZO Y AL FINAL PARA QUE QUEDE COMO ARRAY.
                string descuentosStrObject = DscRcgGlobalOp.ToString();
                string descuentosSrtArray = "[" + descuentosStrObject + "]";
                JArray DscRcgArray = JArray.Parse(descuentosSrtArray);
                var cantidadDscRcg = DscRcgArray.Count();

                // Create a list  


                //Operaciones operacion = new Operaciones();
                //Recorro los detalles enviados y los agrego a una lista
                foreach (var DscRcg in DscRcgArray)
                {
                    ListaDscRcg.Add(DscRcg);
                }
                //VALIDAMOS QUE LOS DESCUENTOS ESTEN BIEN


                //Recorro los detalles enviados y los agrego a una lista

                for (int i = 0; i < DscRcgArray.Count(); i++)
                {

                    DscRcgGlobal DscRcgObject = JsonConvert.DeserializeObject<DscRcgGlobal>(ListaDscRcg[i].ToString());

                    var NroLinDRStr = DscRcgObject.NroLinDR.ToString();
                    var TpoMovStr = DscRcgObject.TpoMov.ToString();

                    //MODIFICACION 19-04-2022: DGZ NO MANDA GLOSA POR LO TANTO SE TUVO QUE HACER OPCIONAL
                    string GlosaDRStr = "";
                    if (DscRcgObject.GlosaDR is not null)
                    {
                        GlosaDRStr = DscRcgObject.GlosaDR.ToString();
                    }
                    
                    
                    var TpoValorStr = DscRcgObject.TpoValor.ToString();
                    var ValorDRStr = DscRcgObject.ValorDR.ToString();

                    writer.WriteStartElement("DscRcgGlobal");
                    writer.WriteElementString("NroLinDR", NroLinDRStr);
                    writer.WriteElementString("TpoMov", TpoMovStr);
                    writer.WriteElementString("GlosaDR", GlosaDRStr);
                    writer.WriteElementString("TpoValor", TpoValorStr);
                    writer.WriteElementString("ValorDR", ValorDRStr);
                    writer.WriteEndElement();


                }
            }
            else { }


            //REFERENCIA OPCIONAL----------------------------------<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
            string FolioRef = ""; // OPCIONAL
            string TpoDocRef = "";
            if (RefFlag == true)
            {
                //MODIFICACION: 19-04-2022: DGZ MANDA SOLO 1 OBJECT DE DESCUENTOS Y NO UN ARRAYS CON VARIOS DESCUENTOS COMO OBJECT DENTRO, POR LO TANTO TUVE QUE AGREGAR "[" "]" AL COMIENZO Y AL FINAL PARA QUE QUEDE COMO ARRAY.
                string referenciaStrObject = ReferenciaOp.ToString();
                string referenciaSrtArray = "[" + referenciaStrObject + "]";


                JArray referenciasArray = JArray.Parse(referenciaSrtArray.ToString());
                var cantidadReferencias = referenciasArray.Count();

                // Create a list  
                List<object> ListaReferencias = new List<object>();

                //Recorro los detalles enviados y los agrego a una lista
                foreach (var referencia in referenciasArray)
                {
                    ListaReferencias.Add(referencia);
                }
                for (int i = 0; i < referenciasArray.Count(); i++)
                {

                    Referencia referenciaObject = JsonConvert.DeserializeObject<Referencia>(ListaReferencias[i].ToString());

                    var NroLinRefStr = referenciaObject.NroLinRef.ToString();
                    var TpoDocRefStr = referenciaObject.TpoDocRef.ToString();
                    var FolioRefStr = referenciaObject.FolioRef.ToString();

                   // var FchRefStr = referenciaObject.FchRef.ToString();
                    var RazonRefStr = referenciaObject.RazonRef.ToString();

                    writer.WriteStartElement("Referencia");
                    writer.WriteElementString("NroLinRef", NroLinRefStr);
                    writer.WriteElementString("TpoDocRef", TpoDocRefStr);
                    writer.WriteElementString("FolioRef", FolioRefStr);
                   // writer.WriteElementString("FchRef", FchRefStr);
                    writer.WriteElementString("RazonRef", RazonRefStr);
                    writer.WriteEndElement();

                    FolioRef = FolioRefStr;
                    TpoDocRef = TpoDocRefStr;
                }
            }
            else { }

            writer.WriteStartElement("TED");
            writer.WriteAttributeString("version", "1.0");

            //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<-----------------VERIFICACION DE MONTOS---------------------->>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> 

            Operaciones operaciones = new Operaciones();
            string RespuestaMontos = operaciones.Montos(ListaDetalles, MntNeto, TasaIVA, IVA, MntTotal);
            //SI TA BIEN CONTINUA, SI NO, DEVUELVE EL ERROR Y CANCELA LA CREACION DE LA FACTURA ********************************* VORTEX
            if (RespuestaMontos == "ta bien")
            {
            }
            else
            {
                //ELIMINAR EL ARCHIVO DE LA FACTURA
                writer.WriteEndDocument();
                writer.Flush();
                writer.Close();
                File.Delete(file_path);

                return RespuestaMontos;
            }

            string RespuestaDscRcg = operaciones.VerificarDscRcgGlobal(ListaDscRcg, MntNeto, TasaIVA, IVA, MntTotal, ListaMontoItemStr, TipoDTE);

            //SI TA BIEN CONTINUA, SI NO, DEVUELVE EL ERROR Y CANCELA LA CREACION DE LA FACTURA ********************************* VORTEX
            if (RespuestaDscRcg == "ta bien")
            {
            }
            else
            {
                //ELIMINAR EL ARCHIVO DE LA FACTURA
                writer.WriteEndDocument();
                writer.Flush();
                writer.Close();
                File.Delete(file_path);
                return RespuestaDscRcg;
            }


            //RESUMEN DD----------------------------------<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

            Detalle detalleObjectI = JsonConvert.DeserializeObject<Detalle>(ListaDetalles[0].ToString());

            //SETEAMOS EL LARGO MAX DEL NMB ITEM ES DE 40 CARACTERES
            int MaxLength = 40;
            string nmbItem = "";
            if (detalleObjectI.NmbItem.Length > MaxLength)
            {
                nmbItem = detalleObjectI.NmbItem.Substring(0, 39);
            }
            else
            {
                nmbItem = detalleObjectI.NmbItem;
            }

            string RS_limitado = "";
            if (RS.Length > MaxLength)
            {
                RS_limitado = RS.Substring(0, 39);
            }
            else
            {
                RS_limitado = RS;
            }


            writer.WriteStartElement("DD");
            writer.WriteElementString("RE", RUTEmisor);
            writer.WriteElementString("TD", TipoDTE);
            writer.WriteElementString("F", Folio);
            writer.WriteElementString("FE", FchEmis);
            writer.WriteElementString("RR", RUTRecep);
            writer.WriteElementString("RSR", RznSocRecep);
            writer.WriteElementString("MNT", MntTotal);
            writer.WriteElementString("IT1", nmbItem);

            //CAF -----------------------------------------<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

            writer.WriteStartElement("CAF");
            writer.WriteAttributeString("version", "1.0");

            writer.WriteStartElement("DA");
            writer.WriteElementString("RE", RE);
            writer.WriteElementString("RS", RS_limitado);
            writer.WriteElementString("TD", TD);

            writer.WriteStartElement("RNG");
            writer.WriteElementString("D", D);
            writer.WriteElementString("H", H);
            writer.WriteEndElement();

            writer.WriteElementString("FA", FA);

            writer.WriteStartElement("RSAPK");
            writer.WriteElementString("M", M);
            writer.WriteElementString("E", E);
            writer.WriteEndElement();
            //writer.WriteEndElement();

            writer.WriteElementString("IDK", IDK);
            writer.WriteEndElement();

            // se coloca algoritmo de CAF------------------------
            string prefix = writer.LookupPrefix("FRMA");

            writer.WriteStartElement(prefix, "FRMA", null);
            writer.WriteAttributeString("algoritmo", Algoritmo);
            writer.WriteString(FRMA);
            writer.WriteEndElement();
            //----------------------------------------------------
            writer.WriteEndElement();
            writer.WriteElementString("TSTED", fechahhmm);


            //  algoritmo de contribuyente FRMT--------------------------------------------------------//
            //richTextBox1.Text = pk;
            string FRMT_str = caf.crearTimbre(pk, strDD);
            //richTextBox1.Text = FRMT_str;
            //---------------------------------------------------------------------------------///

            writer.WriteEndElement();

            writer.WriteStartElement(prefix, "FRMT", null);
            writer.WriteAttributeString("algoritmo", Algoritmo);
            writer.WriteString(FRMT_str);
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteElementString("TmstFirma", fechahhmm);

            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Flush();
            writer.Close();

            XmlDocument oDocument = new XmlDocument();
            oDocument.PreserveWhitespace = true;
            oDocument.Load(archxml);
            oDocument.InnerXml = oDocument.InnerXml.Replace("<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>", "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>");
            oDocument.Save(archxml);
            Debug.Write("Se genero XML");

            Certificado certificado = new Certificado();
            string respuestaFinal = certificado.firmarConCertificado(T39B, oDocument, archxml);


            if (respuestaFinal != "Listo !")
            {
                return "Error al firmar el documento, revisar certificado digital";
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------------
            // GUARDAMOS EL XML GENERADO EN LA BASE DE DATOS ------------------------------------------------------------------------------------------------------------
            //-----------------------------------------------------------------------------------------------------------------------------------------------------------
            ConexionBD conexion = new ConexionBD();

            string DetalleFactura = "";

            for (int i = 0; i < detallesArray.Count(); i++)
            {

                Detalle detalleObject = JsonConvert.DeserializeObject<Detalle>(ListaDetalles[i].ToString());
                if (i < (detallesArray.Count() - 1))
                {
                    DetalleFactura = DetalleFactura + detalleObject.NmbItem.ToString() + ";";

                }
                else
                {
                    DetalleFactura = DetalleFactura + detalleObject.NmbItem.ToString();
                }



            }

            //CREAMOS LA CONSULTA POR SECCIONES
            string queryUpdateInicial = "UPDATE boleta SET fechaemis_boleta = '" + FchEmis + "',mnttotal_boleta = '" + MntTotal + "',detalle_boleta = '" + DetalleFactura + "'";
            string queryUpdateRef = ",folioref_boleta = '" + FolioRef + "', tipo_dteref_boleta = '"+ TpoDocRef + "'";
            string queryUpdateFinal = " WHERE folio_boleta = '" + Folio + "'";
            string queryUpdate = "";
            //string queryInsertInicial = "INSERT INTO detalle_dte (folio_dte_detalle, tipo_dte_detalle,descripcion_detalle) VALUES (";

            //queryInsertDetalle.Append(string.Join(",",ListaDetalles));
            //queryInsertDetalle.Append(";");

            //SI TIENE REFERENCIA SE FUARDA CON REFERENCIA
            if (RefFlag == true)
            {
                queryUpdate = queryUpdateInicial + queryUpdateRef + queryUpdateFinal;
            }
            else
            {
                queryUpdate = queryUpdateInicial + queryUpdateFinal;
            }

            //EJECUTA EL UPDATE EN LA BD

            conexion.Consulta(queryUpdate);

            return archxml;
        }


        public class Detalle
        {
            public int NroLinDet { get; set; }
            public CdgItem CdgItem { get; set; }
            public string NmbItem { get; set; }
            public double QtyItem { get; set; }
            public double PrcItem { get; set; }
            public string DescuentoPct { get; set; }
            public string RecargoPct { get; set; }
            public string DescuentoMonto { get; set; }
            public string RecargoMonto { get; set; }
            public SubDscto SubDscto { get; set; }
            public SubRecargo SubRecargo { get; set; }
            public double MontoItem { get; set; }
        }

        public class CdgItem
        {
            public string TpoCodigo { get; set; }
            public string VlrCodigo { get; set; }
        }

        public class SubDscto
        {
            public string TipoDscto { get; set; }
            public string ValorDscto { get; set; }
        }

        public class SubRecargo
        {
            public string TipoRecargo { get; set; }
            public string ValorRecargo { get; set; }
        }

        public class Referencia
        {
            public int NroLinRef { get; set; }
            public string TpoDocRef { get; set; }
            public string FolioRef { get; set; }
            public string FchRef { get; set; }
            public string RazonRef { get; set; }
        }

        public class DscRcgGlobal
        {
            public int NroLinDR { get; set; }
            public string TpoMov { get; set; }
            public string GlosaDR { get; set; }
            public string TpoValor { get; set; }
            public double ValorDR { get; set; }
        }


    }
}
