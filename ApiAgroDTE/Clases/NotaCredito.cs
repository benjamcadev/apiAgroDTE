﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Text;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace ApiAgroDTE.Clases
{
    public class NotaCredito
    {
        string archxml = "";

        public string crearNotaCredito(JsonElement Encabezado, JsonElement Detalles, JsonElement ReferenciaOp, Boolean RefFlag, JsonElement DscRcgGlobalOp, Boolean DscRcgFlag, int nuevoFolio, string archxml,string T33F)
        {
            Operaciones op = new Operaciones();
            //------------------------------------------------------------------------------------------------------------------
            //CAPTURAR DATOS EN VARIABLES----------------<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
            //------------------------------------------------------------------------------------------------------------------
            JsonElement IdDoc = Encabezado.GetProperty("IdDoc");

            string TipoDTE = IdDoc.GetProperty("TipoDTE").ToString();// Requerido
            string Folio = nuevoFolio.ToString();// Requerido
            string FchEmis = IdDoc.GetProperty("FchEmis").ToString();// Requerido

            //SPLITEAT LA FECHA PARA SEPARAR DATOS
/*
            string TpoTranCompraOp = "";//Opcional
            if (IdDoc.TryGetProperty("TpoTranCompra", out var TpoTranCompra))
            {
                TpoTranCompraOp = TpoTranCompra.ToString();
            }
*/
            string TpoTranVentaOp = "";//Opcional
            if (IdDoc.TryGetProperty("TpoTranVenta", out var TpoTranVenta))
            {
                TpoTranVentaOp = TpoTranVenta.ToString();
            }
            string FmaPagoOp = "";//Opcional
            if (IdDoc.TryGetProperty("FmaPago", out var FmaPago))
            {
                FmaPagoOp = FmaPago.ToString();
            }

            JsonElement Emisor = Encabezado.GetProperty("Emisor");

            string RUTEmisor = Emisor.GetProperty("RUTEmisor").ToString();// Requerido
            string RznSoc = Emisor.GetProperty("RznSoc").ToString();// Requerido
            string GiroEmis = Emisor.GetProperty("GiroEmis").ToString();// Requerido
            string Acteco = Emisor.GetProperty("Acteco").ToString();// Requerido
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

           string CorreoEmisorOp = "";//Opcional
            if (Emisor.TryGetProperty("CorreoEmisor", out var CorreoEmisor))
            {
                CorreoEmisorOp = CorreoEmisor.ToString();
            }


            //CAPTURAR DATOS REFERENCIA

            string CodRef = ""; // OPCIONAL
            string TpoDocRef = ""; // OPCIONAL
            if (RefFlag == true)
            {
                string referenciaStrObject = ReferenciaOp.ToString();
                string referenciaSrtArray = "[" + referenciaStrObject + "]";

                JArray referenciasArray = JArray.Parse(referenciaSrtArray);
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

                    var CodRefStr = referenciaObject.CodRef.ToString();
                    var TpoDocRefStr = referenciaObject.TpoDocRef.ToString();

                    CodRef = CodRefStr;
                    TpoDocRef = TpoDocRefStr;

                }
            }
            else { }



            //CAPTURAR DATOS RECEPTOR
            JsonElement Receptor = Encabezado.GetProperty("Receptor");

            //DISCRIMINAR SI ES UNA NC DE BOLETA O DE UNA FACTURA, ETC...

            string RUTRecep = "";
            string RznSocRecep = "";
            string GiroRecep = "";
            string DirRecep = "";
            string CmnaRecep = "";
            //string Contacto = "";

            if (TpoDocRef == "39" || TpoDocRef == "41")
            {
                RUTRecep = "66666666-6";
                RznSocRecep = "Contacto Anonimo";
                GiroRecep = "Sin datos";
                DirRecep = "Sin datos";
                CmnaRecep = "Sin datos";
            }
            else
            {
                RUTRecep = Receptor.GetProperty("RUTRecep").ToString().ToUpper();// Requerido
                RznSocRecep = Receptor.GetProperty("RznSocRecep").ToString();// Requerido
                GiroRecep = Receptor.GetProperty("GiroRecep").ToString();// Requerido
                DirRecep = Receptor.GetProperty("DirRecep").ToString();// Requerido
                CmnaRecep = Receptor.GetProperty("CmnaRecep").ToString();// Requerido
                //string Contacto = Receptor.GetProperty("Contacto").ToString();// Requerido

                 RznSocRecep = op.LimpiarCaracter(RznSocRecep);
                 DirRecep = op.LimpiarCaracter(DirRecep);
                CmnaRecep = op.LimpiarCaracter(CmnaRecep);

            }



            string CiudadRecepOp = "";//Opcional
            if (Receptor.TryGetProperty("CiudadRecep", out var CiudadRecep))
            {
                CiudadRecepOp = CiudadRecep.ToString();
            }
            

            //CAPTURAR DATOS TOTALES

            JsonElement Totales = Encabezado.GetProperty("Totales");
            string MntNeto = "";
            string MntExe = "";
            if (TpoDocRef == "34")
            {
                MntExe = Totales.GetProperty("MntExe").ToString();
            }
            else
            {
                MntNeto = Totales.GetProperty("MntNeto").ToString();
            }
           
            string TasaIVA = "";
            string IVA = Totales.GetProperty("IVA").ToString();
            string MntTotal = Totales.GetProperty("MntTotal").ToString();

            if (TpoDocRef == "39" || TpoDocRef == "41")
            {
                TasaIVA = "19";

            }
            else if(TpoDocRef == "34"){ 


            }else
            {
                TasaIVA = Totales.GetProperty("TasaIVA").ToString();

            }

            

            string MontoPeriodoOp = "";//Opcional
            if (Totales.TryGetProperty("MontoPeriodo", out var MontoPeriodo))
            {
                MontoPeriodoOp = MontoPeriodo.ToString();
            }
            string VlrPagarOp = "";//Opcional
            if (Totales.TryGetProperty("VlrPagar", out var VlrPagar))
            {
                VlrPagarOp = VlrPagar.ToString();
            }


               
            
            
            //CAPTURA DATOS DEL CAF---------------<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<            

            Caf caf = new Caf();
            string[] stringArray_result  = caf.rescatarCaf(TipoDTE);
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
            writer.WriteAttributeString("ID", T33F);

            writer.WriteStartElement("Encabezado");

            writer.WriteStartElement("IdDoc");
            writer.WriteElementString("TipoDTE", TipoDTE);
            writer.WriteElementString("Folio", Folio);
            writer.WriteElementString("FchEmis", FchEmis);

            if (TpoDocRef == "39" || TpoDocRef == "41")
            {
                writer.WriteElementString("MntBruto", "1");
            }
            else
            {
                if (!string.IsNullOrEmpty(TpoTranVentaOp))
                {
                    writer.WriteElementString("TpoTranVenta", TpoTranVentaOp);
                }
                if (!string.IsNullOrEmpty(FmaPagoOp))
                {
                    writer.WriteElementString("FmaPago", FmaPagoOp);
                }
            }
          
            
            writer.WriteEndElement();

            writer.WriteStartElement("Emisor");
            writer.WriteElementString("RUTEmisor", RUTEmisor);
            writer.WriteElementString("RznSoc", RznSoc);
            writer.WriteElementString("GiroEmis", GiroEmis);
            if (!string.IsNullOrEmpty(TelefonoOp))
            {
                writer.WriteElementString("Telefono", TelefonoOp);
            }
            writer.WriteElementString("Acteco", Acteco);
             if (!string.IsNullOrEmpty(CdgSIISucurOp))
            {
                writer.WriteElementString("CdgSIISucur", CdgSIISucurOp);
            }
            writer.WriteElementString("DirOrigen", DirOrigen);
            writer.WriteElementString("CmnaOrigen", CmnaOrigen);
            if (!string.IsNullOrEmpty(CiudadOrigenOp))
            {
                writer.WriteElementString("CiudadOrigen", CiudadOrigenOp);
            }        
            writer.WriteEndElement();

           

            writer.WriteStartElement("Receptor");
            writer.WriteElementString("RUTRecep", RUTRecep);
            writer.WriteElementString("RznSocRecep", RznSocRecep);
            writer.WriteElementString("GiroRecep", GiroRecep);
            writer.WriteElementString("DirRecep", DirRecep);
            writer.WriteElementString("CmnaRecep", CmnaRecep);
            if (!string.IsNullOrEmpty(CiudadRecepOp))
            {
                writer.WriteElementString("CiudadRecep", CiudadRecepOp);
            }  
            writer.WriteEndElement();


           
            writer.WriteStartElement("Totales");
            if (TpoDocRef == "34")
            {
                writer.WriteElementString("MntExe", MntExe);
            }
            else
            {
                writer.WriteElementString("MntNeto", MntNeto);
                writer.WriteElementString("TasaIVA", TasaIVA);
            }
          
            
            writer.WriteElementString("IVA", IVA);
            writer.WriteElementString("MntTotal", MntTotal);
           if (!string.IsNullOrEmpty(MontoPeriodoOp))
            {
                writer.WriteElementString("MontoPeriodo", MontoPeriodoOp);
            }
             if (!string.IsNullOrEmpty(VlrPagarOp))
            {
                writer.WriteElementString("VlrPagar", VlrPagarOp);
            }
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
            Operaciones operaciones = new Operaciones();
            string RespuestaMontos = operaciones.Montos(ListaDetalles,MntNeto,TasaIVA,IVA,MntTotal);
            //SI TA BIEN CONTINUA, SI NO, DEVUELVE EL ERROR Y CANCELA LA CREACION DE LA FACTURA ********************************* VORTEX
            if(RespuestaMontos == "ta bien"){
            }else{
                //ELIMINAR EL ARCHIVO DE LA FACTURA
                writer.WriteEndDocument();
                writer.Flush();
                writer.Close();
                File.Delete(file_path);
                
                return RespuestaMontos;
            }

            //ENVIAMOS LA LISTA PARA REALIZAR OPERACIONES Y VALIDAR CUADRE DE MONTOS           


            for (int i=0; i< detallesArray.Count(); i++){

                Detalle detalleObject = JsonConvert.DeserializeObject<Detalle>(ListaDetalles[i].ToString());

                // EJEMPLO DE APLICAR detalleObject.NroLinDet



                string NroLinDetStr = "";
                string IndExe = "";

                if (TpoDocRef == "34")
                {
                    IndExe = detalleObject.IndExe.ToString();
                }
               



                NroLinDetStr = detalleObject.NroLinDet.ToString();
                var NmbItemStr = detalleObject.NmbItem.ToString();                
                var QtyItemStr = detalleObject.QtyItem.ToString();
                var PrcItemStr = detalleObject.PrcItem.ToString();
                string DescuentoPct = "";
                string DescuentoMonto = "";
                string RecargoPct = "";
                string RecargoMonto = "";

                NmbItemStr = op.LimpiarCaracter(NmbItemStr);


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

                if (TpoDocRef == "34")
                {
                    writer.WriteElementString("IndExe", IndExe);
                }
                else
                {
                    
                }
                   
               
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
                
                if(CodRef!= "2"){
                    writer.WriteElementString("QtyItem", QtyItemStr);
                    writer.WriteElementString("PrcItem", PrcItemStr);
                }
                // EN CASO DE QUE UN PRODUCTO TRAIGA DESCUENTO O RECARGO--------------------------------------------------------------
                
                SubDscto SubDsctoOp = new SubDscto();
                SubRecargo SubRecargoOp = new SubRecargo();                
                Boolean SubDsctoFlag = false;
                Boolean SubRecargoFlag = false;

                if(CodRef == "2"){
                    if (!(detalleObject.DescuentoPct == null))
                    {                    
                        SubDsctoOp = detalleObject.SubDscto;
                        SubDsctoFlag = true;                        
                        writer.WriteElementString("DescuentoPct", DescuentoPct);
                    }
                }else {
                    if (!(detalleObject.DescuentoMonto == null))
                    {                    
                        SubDsctoOp = detalleObject.SubDscto;
                        SubDsctoFlag = true;

                        if(!(detalleObject.DescuentoPct == null))
                        {
                            writer.WriteElementString("DescuentoPct", DescuentoPct);
                        }                    
                        writer.WriteElementString("DescuentoMonto", DescuentoMonto);

                    }
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

                if(CodRef == "2"){
                    if (!(detalleObject.RecargoPct == null))
                    {                    
                        SubRecargoOp = detalleObject.SubRecargo;
                        SubRecargoFlag = true;                       
                        writer.WriteElementString("RecargoPct", RecargoPct);
                        
                    } 
                }else {
                    if (!(detalleObject.RecargoMonto == null))
                    {                    
                        SubRecargoOp = detalleObject.SubRecargo;
                        SubRecargoFlag = true;

                        if(!(detalleObject.RecargoPct == null))
                        {
                            writer.WriteElementString("RecargoPct", RecargoPct);
                        }                    
                        writer.WriteElementString("RecargoMonto", RecargoMonto);
                    } 

                }      
               
                if (SubRecargoFlag == true)
                {
                    if (SubRecargoOp is not null)
                    {
                        var TipoRecargo = SubRecargoOp.TipoRecargo.ToString();
                        var ValorRecargo = SubRecargoOp.ValorRecargo.ToString();

                        writer.WriteStartElement("SubRecargo");
                        writer.WriteElementString("TipoRecargo", TipoRecargo);
                        writer.WriteElementString("ValorRecargo", ValorRecargo);
                        writer.WriteEndElement();
                    }
                   
                }
            
                //----------------------------------------------------------------------------------
                writer.WriteElementString("MontoItem", MontoItemStr);
                writer.WriteEndElement();

                ListaMontoItemStr.Add(double.Parse(MontoItemStr)); 


            }

            
            
            // EN CASO DE QUE LA FACTURA VENGA CON UN DESCUENTO O RECARGO GLOBAL----------------<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
            List<object> ListaDscRcg = new List<object>();
            if(CodRef != "2"){
                if(DscRcgFlag == true){

                    //MODIFICACION: 23-01-2023: DGZ MANDA SOLO 1 OBJECT DE DESCUENTOS Y NO UN ARRAYS CON VARIOS DESCUENTOS COMO OBJECT DENTRO, POR LO TANTO TUVE QUE AGREGAR "[" "]" AL COMIENZO Y AL FINAL PARA QUE QUEDE COMO ARRAY.
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

                    for (int i=0; i< DscRcgArray.Count(); i++){

                        //MODIFICAION 23-01-2023: ACA LA REQUEST DEBE VENIR CON "glosaDR" en descuentos, de alguna forma setear la glosa en vacio si no viene en la request


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

                        GlosaDRStr = op.LimpiarCaracter(GlosaDRStr);

                        writer.WriteStartElement("DscRcgGlobal");
                        writer.WriteElementString("NroLinDR", NroLinDRStr);
                        writer.WriteElementString("TpoMov", TpoMovStr);
                       writer.WriteElementString("GlosaDR", GlosaDRStr);
                        writer.WriteElementString("TpoValor", TpoValorStr);
                        writer.WriteElementString("ValorDR", ValorDRStr);
                        writer.WriteEndElement();

            
                    }
                }
            }
            
 
           //REFERENCIA OPCIONAL----------------------------------<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
            string FolioRef = ""; // OPCIONAL
           
            if (RefFlag == true){
                string referenciaStrObject = ReferenciaOp.ToString();
                string referenciaSrtArray = "[" + referenciaStrObject + "]";

                JArray referenciasArray = JArray.Parse(referenciaSrtArray);
                var cantidadReferencias = referenciasArray.Count();

                // Create a list  
                List<object> ListaReferencias = new List<object>();

                //Recorro los detalles enviados y los agrego a una lista
                foreach (var referencia in referenciasArray)
                {
                    ListaReferencias.Add(referencia);
                }
                for (int i=0; i< referenciasArray.Count(); i++){

                    Referencia referenciaObject = JsonConvert.DeserializeObject<Referencia>(ListaReferencias[i].ToString());
               
                    var NroLinRefStr = referenciaObject.NroLinRef.ToString();
                    var TpoDocRefStr = referenciaObject.TpoDocRef.ToString();                
                    var FolioRefStr = referenciaObject.FolioRef.ToString();
                    var FchRefStr = referenciaObject.FchRef.ToString();
                    var CodRefStr = referenciaObject.CodRef.ToString();

                    var RazonRefStr = referenciaObject.RazonRef.ToString();

                    RazonRefStr = op.LimpiarCaracter(RazonRefStr);

                    writer.WriteStartElement("Referencia");
                    writer.WriteElementString("NroLinRef", NroLinRefStr);
                    writer.WriteElementString("TpoDocRef", TpoDocRefStr);
                    writer.WriteElementString("FolioRef", FolioRefStr);
                    writer.WriteElementString("FchRef", FchRefStr);
                    writer.WriteElementString("CodRef", CodRefStr);
                    writer.WriteElementString("RazonRef", RazonRefStr);
                    writer.WriteEndElement();

                    FolioRef = FolioRefStr;
                }
            }else{}

            if (CodRef == "1")
            {
                //COMPARAR VARIABLES CON LA FACTURA DE REFERENCIA 
                string respuestaNC1 = operaciones.VerificarNC1(MntNeto,TasaIVA,IVA,MntTotal, FolioRef,TpoDocRef);

                if(respuestaNC1 == "ta bien"){
                }else{
                    //ELIMINAR EL ARCHIVO DE LA FACTURA
                    writer.WriteEndDocument();
                    writer.Flush();
                    writer.Close();
                    File.Delete(file_path);
                    
                    return respuestaNC1;
                }



            }
            else if(CodRef == "2"){
            //VALIDAR TOTALES SIEMPRE EN "0"
                string respuestaNC2 = operaciones.VerificarNC2(MntNeto,IVA,MntTotal,ListaMontoItemStr);
                if(respuestaNC2 == "ta bien"){
                }else{
                    //ELIMINAR EL ARCHIVO DE LA FACTURA
                    writer.WriteEndDocument();
                    writer.Flush();
                    writer.Close();
                    File.Delete(file_path);
                    
                    return respuestaNC2;
                }

            }else if (CodRef == "3"){
                //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<-----------------VERIFICACION DE MONTOS---------------------->>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>             

                string RespuestaDscRcg = operaciones.VerificarDscRcgGlobal(ListaDscRcg,MntNeto,TasaIVA,IVA,MntTotal,ListaMontoItemStr,TipoDTE);
                string respuestaNCD3 = operaciones.VerificarNCD3(MntTotal,FolioRef,TipoDTE,TpoDocRef);

                //SI TA BIEN CONTINUA, SI NO, DEVUELVE EL ERROR Y CANCELA LA CREACION DE LA FACTURA ********************************* VORTEX
                if(RespuestaDscRcg == "ta bien" && respuestaNCD3 == "ta bien"){
                }else{
                    //ELIMINAR EL ARCHIVO DE LA FACTURA
                    writer.WriteEndDocument();
                    writer.Flush();
                    writer.Close();
                    File.Delete(file_path);
                    return RespuestaDscRcg;
                }
            }             

            writer.WriteStartElement("TED");
            writer.WriteAttributeString("version", "1.0");

            


            //RESUMEN DD----------------------------------<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

            Detalle detalleObjectI = JsonConvert.DeserializeObject<Detalle>(ListaDetalles[0].ToString());

            //SETEAMOS EL LARGO MAX DEL NMB ITEM ES DE 40 CARACTERES
            int MaxLength = 40;
            string nmbItem = "";
            if (detalleObjectI.NmbItem.Length > MaxLength)
            {
                nmbItem = detalleObjectI.NmbItem.Substring(0, 39);
                nmbItem = op.LimpiarCaracter(nmbItem);
            }
            else
            {
                nmbItem = detalleObjectI.NmbItem;
                nmbItem = op.LimpiarCaracter(nmbItem);
            }

            //SETEAMOS EL LARGO MAX DEL razonReceptor ES DE 40 CARACTERES
            //int MaxLength = 40;
            string razonReceptor = RznSocRecep;
            if (razonReceptor.Length > MaxLength)
            {
                razonReceptor = razonReceptor.Substring(0, 39);
                razonReceptor = op.LimpiarCaracter(razonReceptor);

            }
            else
            {
                razonReceptor = razonReceptor;
               
            }


            writer.WriteStartElement("DD");
            writer.WriteElementString("RE", RUTEmisor);
            writer.WriteElementString("TD", TipoDTE);
            writer.WriteElementString("F", Folio);
            writer.WriteElementString("FE", FchEmis);
            writer.WriteElementString("RR", RUTRecep);
            writer.WriteElementString("RSR", razonReceptor);
            writer.WriteElementString("MNT", MntTotal);
            writer.WriteElementString("IT1", nmbItem);

            //CAF -----------------------------------------<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

            writer.WriteStartElement("CAF");
            writer.WriteAttributeString("version", "1.0");

            writer.WriteStartElement("DA");
            writer.WriteElementString("RE", RE);
            writer.WriteElementString("RS", RS);
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
            string respuestaFinal = certificado.firmarConCertificado(T33F,oDocument,archxml);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        // GUARDAMOS EL XML GENERADO EN LA BASE DE DATOS ------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
            ConexionBD conexion = new ConexionBD();

            string DetalleFactura = "";

            for (int i = 0; i < detallesArray.Count(); i++){
                
                Detalle detalleObject = JsonConvert.DeserializeObject<Detalle>(ListaDetalles[i].ToString());
                if(i < (detallesArray.Count() -1) ){
                    DetalleFactura = DetalleFactura + detalleObject.NmbItem.ToString() + ";";

                }else{
                    DetalleFactura = DetalleFactura + detalleObject.NmbItem.ToString();
                }
                
                              

            }

            //CREAMOS LA CONSULTA
            //A DIFERENCIA DE FACTURA, LLEVA REFERENCIA SI O SI, POR LOQ UE LA CONSULTA ES DIRECTAMENTE CON REFERENCIA
            string queryUpdate = "UPDATE nota_credito SET fchemis_nota_credito = '" + FchEmis + "',rutrecep_nota_credito = '" + RUTRecep + "',rznsocrecep_nota_credito = '" + RznSocRecep + "',cmnarecep_nota_credito = '" + CmnaRecep + "',mnttotal_nota_credito = '" + MntTotal + "',detalle_nota_credito = '" + DetalleFactura + "',folioref_nota_credito = '" + FolioRef + "', tipo_dteref_nota_credito = '"+ TpoDocRef + "' WHERE folio_nota_credito = '" + Folio + "'";
                       
            //EJECUTA EL UPDATE EN LA BD
            
            conexion.Consulta(queryUpdate);

            return archxml;
        }

         public static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------------------------------

        public class Detalle
        {
            public int NroLinDet { get; set; }

            public string IndExe { get; set; }
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
            public string  TipoRecargo { get; set; }
            public string  ValorRecargo { get; set; }
        }

         public class Referencia
        {
            public int NroLinRef { get; set; }
            public string TpoDocRef { get; set; }
            public string FolioRef { get; set; }
            public string FchRef { get; set; }
            public string CodRef { get; set; }            
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
