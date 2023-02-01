using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ApiAgroDTE.Clases
{
    public class Operaciones
    {
        public string Montos(List<object> ListaDetalles,string MntNeto,string TasaIVA,string IVA,string MntTotal){
            
            List<double> ListaQtyItem = new List<double>();
            List<double> ListaPrcItem = new List<double>();
            List<double> ListaDescuentoMonto = new List<double>();
            List<double> ListaDctoPct = new List<double>();
            List<double> ListaRecargoMonto = new List<double>();
            List<double> ListaRecargoPct = new List<double>();
            List<double> ListaMontoItem = new List<double>();
            List<double> ListaMontoItemLocal = new List<double>();
            //List<double> ListaRecargoMontoLocal = new List<double>();
            //List<double> ListaDescuentoMontoLocal = new List<double>();
           
            for (int i=0; i< ListaDetalles.Count(); i++)
            {
                Detalle detalleObject = JsonConvert.DeserializeObject<Detalle>(ListaDetalles[i].ToString());

                SubDscto SubDsctoOp = new SubDscto();
                SubRecargo SubRecargoOp = new SubRecargo();

                ListaQtyItem.Add(detalleObject.QtyItem);
                ListaPrcItem.Add(detalleObject.PrcItem);
                ListaDescuentoMonto.Add(detalleObject.DescuentoMonto);
                ListaDctoPct.Add(detalleObject.DescuentoPct);
                ListaRecargoMonto.Add(detalleObject.RecargoMonto);
                ListaRecargoPct.Add(detalleObject.RecargoPct);
                ListaMontoItem.Add(detalleObject.MontoItem);

                double DescuentoMontoLocal = 0;          
                double RecargoMontoLocal = 0;
                double MontoItemLocal = ListaQtyItem[i] * ListaPrcItem[i];
                //double MontoConDescuento = 0;
                //double MontoConRecargo = 0;               
                
                if(!(detalleObject.RecargoMonto == 0)){ 

                    SubRecargoOp = detalleObject.SubRecargo;
                    var TipoRecargo = SubRecargoOp.TipoRecargo.ToString();
                    var ValorRecargo = SubRecargoOp.ValorRecargo.ToString(); 
                    
                    RecargoMontoLocal = MontoItemLocal * (ListaRecargoPct[i]/100);
                                        
                    //si trae en %, convertir a monto y comparar RecargoMontoLocal con ListaRecargoMonto[i]
                    
                    if(Math.Round(RecargoMontoLocal) == ListaRecargoMonto[i] || Math.Round(RecargoMontoLocal)+1 == ListaRecargoMonto[i] || Math.Round(RecargoMontoLocal)-1 == ListaRecargoMonto[i]){
                       
                    }else{ return "Error en RecargoMonto: "+ListaRecargoMonto[i].ToString()+"/"+Math.Round(RecargoMontoLocal).ToString();
                    }

                     //MontoItemLocal = MontoItemLocal + RecargoMontoLocal;
                      if(TipoRecargo == "$"){
                    //double RecargoMontoLocalSub = MontoItemLocal + ValorRecargo; 
                        if(double.Parse(ValorRecargo)+1 == ListaRecargoMonto[i] || double.Parse(ValorRecargo)-1 == ListaRecargoMonto[i] || double.Parse(ValorRecargo) == ListaRecargoMonto[i]){                            
                        }else{return "Error en ValorRecargo: "+ValorRecargo+"/"+ListaRecargoMonto[i].ToString();
                        }
                     }else{
                        if(double.Parse(ValorRecargo) == ListaRecargoPct[i] || 1+double.Parse(ValorRecargo) == ListaRecargoPct[i] || 1-double.Parse(ValorRecargo) == ListaRecargoPct[i]){                            
                        }else{return "Error en ValorRecargo: "+ValorRecargo+" % /"+ListaRecargoPct[i].ToString() + "%";}

                    } 
                }                
                

                if(!(detalleObject.DescuentoPct == 0)){

                    if (detalleObject.SubDscto is not null)
                    {


                        SubDsctoOp = detalleObject.SubDscto;
                        var TipoDscto = SubDsctoOp.TipoDscto.ToString();
                        var ValorDscto = SubDsctoOp.ValorDscto.ToString();
                        //si trae en %, convertir a monto y comparar DescuentoMontoLocal con ListaDescuentoMonto[i]
                        DescuentoMontoLocal = MontoItemLocal * (ListaDctoPct[i] / 100);

                        if (Math.Round(DescuentoMontoLocal) == ListaDescuentoMonto[i] || Math.Round(DescuentoMontoLocal) + 1 == ListaDescuentoMonto[i] || Math.Round(DescuentoMontoLocal) - 1 == ListaDescuentoMonto[i])
                        {
                        }
                        else { return "Error en  DescuentoMonto: " + ListaDescuentoMonto[i].ToString() + "/" + Math.Round(DescuentoMontoLocal).ToString(); }

                        //MontoItemLocal = MontoItemLocal - DescuentoMontoLocal;   
                        if (TipoDscto == "$")
                        {
                            //double RecargoMontoLocalSub = MontoItemLocal + ValorRecargo; 
                            if (double.Parse(ValorDscto) == ListaDescuentoMonto[i] || double.Parse(ValorDscto) + 1 == ListaDescuentoMonto[i] || double.Parse(ValorDscto) - 1 == ListaDescuentoMonto[i])
                            {
                            }
                            else { return "Error en ValorDscto: " + ValorDscto + "/" + ListaDescuentoMonto[i].ToString(); }

                        }
                        else
                        {
                            if (double.Parse(ValorDscto) == ListaDctoPct[i] || double.Parse(ValorDscto) + 1 == ListaDctoPct[i] || double.Parse(ValorDscto) - 1 == ListaDctoPct[i])
                            {
                                return "Error en ValorDscto: " + ValorDscto + " % /" + ListaDctoPct[i].ToString() + "%";
                            }

                        }
                    }
                }
                              
                
            } 

            return "ta bien";   

        }

        public string VerificarNC1(string MntNeto, string TasaIVA, string IVA, string MntTotal, string FolioRef, string TpoDocRef)
        {
            // nota de credito - anulacion total, traer valores de la factura y comprobar
            //TRAER TOTALES DE LA FACTURA, CONECTAR A LA BD
            ConexionBD conexion = new ConexionBD();

            List<string> Totales = new List<string>();

            //VERIFICAR SI LA NC ES DE FACTURA O BOLETA
            if (TpoDocRef == "39")
            {
                Totales = conexion.Select("SELECT mnttotal_boleta FROM boleta WHERE folio_boleta = '" + FolioRef + "' ");
            }
            else if (TpoDocRef == "41")
            {
                Totales = conexion.Select("SELECT mnttotal_boleta_exenta FROM boleta_exenta WHERE folio_boleta_exenta = '" + FolioRef + "' ");
            }
            else if (TpoDocRef == "34")
            {
                Totales = conexion.Select("SELECT mnttotal_factura_exenta FROM factura_exenta WHERE folio_factura_exenta = '" + FolioRef + "' ");
            }
            else
            {
                Totales = conexion.Select("SELECT mnttotal_factura FROM factura WHERE folio_factura = '" + FolioRef + "' ");
            }


            

            if (Totales.Count == 0)
            {
                return "Error Factura/Boleta no registrada en la base de datos";
            }
            if (Totales[0] != MntTotal)
            {
                //SI LOS TOTALES SON DISTINTOS
                return "Error en <MntTotal>, Diferencia en los montos totales "+ MntTotal + " / "+ Totales[0];
            } 



            return "ta bien";
        }
        public string VerificarNC2(string MntNeto,string IVA,string MntTotal,List<double> ListaMontoItem){
            // nota de credito - cambio de texto, valores en 0
            string MontoItems = ListaMontoItem.Sum().ToString();

            if(!(MntNeto == "0")){
                 return "Error en MntNeto: "+ MntNeto +"/ 0"; 
            }
            if(!(IVA == "0")){
                 return "Error en IVA: "+ IVA +"/ 0"; 
            }
            if(!(MntTotal == "0")){
                 return "Error en MntTotal: "+ MntTotal +"/ 0"; 
            }
             if(!(MontoItems == "0")){
                 return "Error en MontoItem: "+ MontoItems +"/ 0"; 
            }           

            return "ta bien";
        }

        public string VerificarNCD3(string MntTotal, string FolioRef,string TipoDTE, string TpoDocRef)
        {
            // nota de debito - anulacion total, traer valores de la factura y comprobar
            //TRAER TOTALES DE LA FACTURA, CONECTAR A LA BD
            ConexionBD conexion = new ConexionBD();
            List<string> Totales = new List<string>();

          

            if (TpoDocRef == "39")
            {
                Totales = conexion.Select("SELECT mnttotal_boleta FROM boleta WHERE folio_boleta = '" + FolioRef + "' ");
            }else if(TpoDocRef == "41")
            {
                Totales = conexion.Select("SELECT mnttotal_boleta_exenta FROM boleta_exenta WHERE folio_boleta_exenta = '" + FolioRef + "' ");
            }
            else if (TpoDocRef == "61")
            {
                Totales = conexion.Select("SELECT mnttotal_nota_credito FROM nota_credito WHERE folio_nota_credito = '" + FolioRef + "' ");
            }
             else if (TpoDocRef == "34")
            {
                Totales = conexion.Select("SELECT mnttotal_factura_exenta FROM factura_exenta WHERE folio_factura_exenta = '" + FolioRef + "' ");
            }
            else
            {
                Totales = conexion.Select("SELECT mnttotal_factura FROM factura WHERE folio_factura = '" + FolioRef + "' ");
            }
                //List<string> Totales_Factura = conexion.Select("SELECT mnttotal_factura FROM factura WHERE folio_factura = '"+FolioRef+"' ");



            if (Totales.Count == 0)
            {
                return "Error Factura/Boleta no registrada en la base de datos";
            }
            if(TipoDTE == "61"){
                if (int.Parse(Totales[0]) < int.Parse(MntTotal))
                {
                    //SI LOS TOTALES SON DISTINTOS
                    return "Error en <MntTotal>, En modificación de montos no puede exeder o ser igual al monto original "+ MntTotal + " > o = "+ Totales[0];
                } 
            }
             if(TipoDTE == "56" && TpoDocRef == "61")
            {
                if (int.Parse(Totales[0]) == int.Parse(MntTotal) || int.Parse(Totales[0]) > int.Parse(MntTotal))
                {
                    //SI LOS TOTALES SON DISTINTOS
                    return "Error en <MntTotal>, En modificación de montos no puede ser menor igual al monto original "+ MntTotal + " < o = "+ Totales[0];
                } 
            }
            return "ta bien";
        }

       

            public string VerificarND1(string MntNeto, string TasaIVA, string IVA, string MntTotal, string FolioRef)
        {
            // nota de debito - anulacion total, traer valores de la Nota de Credito y comprobar
            //TRAER TOTALES DE LA Nota de credito, CONECTAR A LA BD
            ConexionBD conexion = new ConexionBD();
            List<string> Totales_NC = conexion.Select("SELECT mnttotal_nota_credito FROM nota_credito WHERE folio_nota_credito = '"+FolioRef+"' ");

            if (Totales_NC.Count == 0)
            {
                return "Error Nota de Credito no registrada en la base de datos";
            }
            if (Totales_NC[0] != MntTotal)
            {
                //SI LOS TOTALES SON DISTINTOS
                return "Error en MntTotal, Diferencia en los montos totales "+ MntTotal + " / "+ Totales_NC[0];
            } 



            return "ta bien";
        }

        public string VerificarDscRcgGlobal (List<object> ListaDscRcg,string MntNetoStr,string TasaIVAStr,string IVAStr,string MntTotalStr,List<double> ListaMontoItem,string TipoDTE){

            //List<double> ListaMntNetoLocal =new List<double>();
            //List<double> ListaIVALocal =new List<double>();
            List<double> ListaDscRcgLocal =new List<double>();

            double IVAApi = double.Parse(IVAStr);
                double TasaIVA = double.Parse(TasaIVAStr);
                double MntNetoApi = double.Parse(MntNetoStr);
                double MntTotalApi = double.Parse(MntTotalStr);
                double ValorRecargoPctLocal = 0;
                double ValorDescuentoPctLocal = 0;
                double TasaIVALocal = double.Parse(TasaIVAStr) / 100;
                //double MntNetoLocal =0;
                double MontoDscRcg = 0;
                double IVALocal = 0;
                double MntTotalLocal = 0;
                double MntNetoLocal = 0;

               if(TipoDTE == "39" || TipoDTE == "41")
           {
                    MntNetoLocal = Math.Round((ListaMontoItem.Sum() * 100) / 119);
              }
              else{
                    MntNetoLocal = ListaMontoItem.Sum();
              }                
              
               

            for(int i=0; i< ListaDscRcg.Count(); i++ ){

                DscRcgGlobal DscRcgObject = JsonConvert.DeserializeObject<DscRcgGlobal>(ListaDscRcg[i].ToString());

                string TpoValorStr = DscRcgObject.TpoValor;
                 double ValorDscRcg = DscRcgObject.ValorDR;
               
               
                string TpoMovStr = DscRcgObject.TpoMov;
                

                if(TpoMovStr == "D"){

                    if(TpoValorStr == "%"){
                        ValorDescuentoPctLocal = ValorDscRcg / 100;  
                        MontoDscRcg = MntNetoLocal * ValorDescuentoPctLocal; 
                        MntNetoLocal = MntNetoLocal - MontoDscRcg;
                        
                    }
                    if(TpoValorStr == "$"){
                         if(TipoDTE == "39" || TipoDTE == "41"){
                    ValorDscRcg =  Math.Round((ValorDscRcg* 100) / 119);
                }

                        MntNetoLocal = MntNetoLocal - ValorDscRcg;                        
                    }
                }

                if (TpoMovStr == "R"){

                    if(TpoValorStr == "%"){
                        ValorRecargoPctLocal = ValorDscRcg / 100;  
                        MontoDscRcg = MntNetoLocal * ValorRecargoPctLocal;
                        MntNetoLocal = MntNetoLocal + MontoDscRcg;                        
                                        
                    }

                    if(TpoValorStr == "$"){
                         if(TipoDTE == "39" || TipoDTE == "41"){
                    ValorDscRcg =  Math.Round((ValorDscRcg* 100) / 119);
                }
                        //MntNetoLocal = SumaMontoItemLocal;
                        MntNetoLocal = MntNetoLocal + ValorDscRcg;
                        
                    }

                }

                //MntNetoLocal = MntNetoLocal;                             
                
            }           

            //double MontoDscRcgLocal = ListaDscRcgLocal.Sum();
            //MntNetoLocal = SumaMontoItemLocal + MontoDscRcgLocal;
            
            //if(TipoDTE == "39" || TipoDTE == "41")
           //{}
           //else{
            IVALocal = MntNetoLocal * TasaIVALocal;
            MntTotalLocal = MntNetoLocal + IVALocal; 
           //}
            

            if(MntNetoApi == Math.Round(MntNetoLocal) || MntNetoApi == Math.Round(MntNetoLocal)+1 || MntNetoApi == Math.Round(MntNetoLocal-1) ){               
            }else{ 
                if (TipoDTE == "41")
                {
                    return "ta bien";
                }
                return "Error en MntNeto: "+MntNetoApi.ToString()+"/"+ Math.Round(MntNetoLocal).ToString();
            }

            if(Math.Round(IVALocal) == IVAApi || Math.Round(IVALocal) == IVAApi+1 || Math.Round(IVALocal) == IVAApi-1){               
            }else{ 
                if(TipoDTE == "34" || TipoDTE == "41")
                {
                    return "ta bien";
                }
                return "Error en IVA: "+IVAApi.ToString()+"/"+ Math.Round(IVALocal).ToString();
            }

            if(MntTotalApi == Math.Round(MntTotalLocal) || MntTotalApi == Math.Round(MntTotalLocal)+1 || MntTotalApi == Math.Round(MntTotalLocal)-1){
               
            }else{ return "Error en MntTotal: "+MntTotalApi.ToString()+"/"+ Math.Round(MntTotalLocal).ToString();}

            return "ta bien";

        }

        public string LimpiarCaracter(string strCadena)
        {
            strCadena = strCadena.Replace("°", " ");
            strCadena = strCadena.Replace("*", " ");
            strCadena = strCadena.Replace("_", " ");
            strCadena = strCadena.Replace("á", "a");
            strCadena = strCadena.Replace("é", "e");
            strCadena = strCadena.Replace("í", "i");
            strCadena = strCadena.Replace("ó", "o");
            strCadena = strCadena.Replace("ú", "u");
            strCadena = strCadena.Replace("ñ", "n");
            strCadena = strCadena.Replace("Á", "A");
            strCadena = strCadena.Replace("É", "E");
            strCadena = strCadena.Replace("Ó", "O");
            strCadena = strCadena.Replace("Ú", "U");
            strCadena = strCadena.Replace("Ñ", "N");
            strCadena = strCadena.Replace("Ü", "U");
            strCadena = strCadena.Replace("ü", "u");
            strCadena = strCadena.Replace("\"", "");
            strCadena = strCadena.Replace("º", "");
            strCadena = strCadena.Replace("Nº", "N");
            strCadena = strCadena.Replace("/", "");
            strCadena = strCadena.Replace("Í", "I");
            
           



            return strCadena;
        }


        /*
                public double[] RecargoGlobal (string TpoValorStr, string ValorDRStr,string MntNetoStr,string IVAStr, string MntTotalStr){

                    double IVADbl = double.Parse(IVAStr);
                    double MntNetoDbl = double.Parse(MntNetoStr);
                    double ValorDRDbl = double.Parse(ValorDRStr);
                    double MntTotalDbl = double.Parse(MntTotalStr);

                    if(TpoValorStr == "%"){
                        double valorDescuento = 1 + (ValorDRDbl / 100);
                        double MntNeto = MntNetoDbl * valorDescuento;
                        double IVA = IVADbl * valorDescuento;
                        double MntTotal = MntTotalDbl * valorDescuento;



                        double[] ResultadoPorcentaje = new double[]{Math.Round(MntNeto),Math.Round(IVA),Math.Round(MntTotal)};
                        return ResultadoPorcentaje;

                    }
                    if(TpoValorStr == "$"){
                        //REGLA DE 3 SIMPLE PARA DETERMINAR EL PORCENTAJE DE DESCUENTO Y A PLICARLO A LOS DEMAS MONTOS
                        double PorRecargo = 1 + ((ValorDRDbl*1)/MntNetoDbl);
                        double IVA = IVADbl * PorRecargo;
                        double MntTotal = MntTotalDbl * PorRecargo;
                        double MntNeto = MntNetoDbl + ValorDRDbl; //SE PUEDE SUMAR O UTILIZAR EL PORCENTAJE, DA EL MISMO RESULTADO

                        double[] ResultadoPesos = new double[]{Math.Round(MntNeto),Math.Round(IVA),Math.Round(MntTotal)};
                        return ResultadoPesos;
                    }

                    double[] Resultado = new double[]{}; 
                    return Resultado;
                }

                public double[] DescuentoGlobal (string TpoValorStr, string ValorDRStr,string MntNetoStr,string IVAStr, string MntTotalStr){

                    double IVADbl = double.Parse(IVAStr);
                    double MntNetoDbl = double.Parse(MntNetoStr);
                    double ValorDRDbl = double.Parse(ValorDRStr);
                    double MntTotalDbl = double.Parse(MntTotalStr);

                    if(TpoValorStr == "%"){
                        double valorDescuento = 1 - (ValorDRDbl / 100);
                        double MntNeto = MntNetoDbl * valorDescuento;
                        double IVA = IVADbl * valorDescuento;
                        double MntTotal = MntTotalDbl * valorDescuento;

                        double[] ResultadoPorcentaje = new double[]{Math.Round(MntNeto),Math.Round(IVA),Math.Round(MntTotal)};
                        return ResultadoPorcentaje;

                    }
                    if(TpoValorStr == "$"){
                        //REGLA DE 3 SIMPLE PARA DETERMINAR EL PORCENTAJE DE DESCUENTO Y A PLICARLO A LOS DEMAS MONTOS
                        double PorDescuento = 1 - ((ValorDRDbl*1)/MntNetoDbl);
                        double IVA = IVADbl * PorDescuento;
                        double MntTotal = MntTotalDbl * PorDescuento;
                        double MntNeto = MntNetoDbl - ValorDRDbl; //SE PUEDE RESTAR O UTILIZAR EL PORCENTAJE, DA EL MISMO RESULTADO

                        double[] ResultadoPesos = new double[]{Math.Round(MntNeto),Math.Round(IVA),Math.Round(MntTotal)};
                        return ResultadoPesos;
                    }

                    double[] Resultado = new double[]{}; 
                    return Resultado;
                }*/
    }  

    //nueva clase para notas de credito, montos = a 0

    public class Detalle
        {
            public int NroLinDet { get; set; }
            public CdgItem CdgItem { get; set; }
            public string NmbItem { get; set; }
            public double QtyItem { get; set; }
            public double PrcItem { get; set; }
            public double DescuentoPct { get; set; }
            public double RecargoPct { get; set; }
            public double DescuentoMonto { get; set; }
            public double RecargoMonto { get; set; }
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

        public class DscRcgGlobal
        {
            public int NroLinDR { get; set; }
            public string TpoMov { get; set; }
            public string GlosaDR { get; set; }
            public string TpoValor { get; set; }
            public double ValorDR { get; set; }
        }
        
    }