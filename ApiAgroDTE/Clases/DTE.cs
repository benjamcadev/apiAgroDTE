using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using static ApiAgroDTE.Controllers.PruebaControlador;
using System.Net;
using System.Text;
using System.IO;
using iTextSharp.text.pdf;
using System.Drawing;
using System.Drawing.Imaging;


namespace ApiAgroDTE.Clases
{
    public class DTE
    {
        public string[] crearDTE(JsonElement dte)
        {
            EnvioDTE envioDTE = new EnvioDTE();         
            Schemas schemas = new Schemas();
            ConexionBD conexion = new ConexionBD();
            string[] respuestaCrearDTE = new string[7];

            if (!dte.TryGetProperty("Encabezado", out var dte_content))
            {
                respuestaCrearDTE[0] = "Error, en json de request no viene key Encabezado";
                return respuestaCrearDTE;
            }

            JsonElement Encabezado = dte.GetProperty("Encabezado");

            if (!dte.TryGetProperty("Detalle", out var dte_content2))
            {
                respuestaCrearDTE[0] = "Error, en json de request no viene key Detalle";
                return respuestaCrearDTE;
            }
            JsonElement Detalles = dte.GetProperty("Detalle");

           
            if (Detalles.GetArrayLength() <= 0)
            {
                //EN EL DETALLES NO VIENEN DATOS
                respuestaCrearDTE[0] = "Error, en json de request no vienen datos en el key Detalle";
                return respuestaCrearDTE;
            }
            

            JsonElement DscRcgGlobalOp = new JsonElement(); //Opcional
            JsonElement ReferenciaOp = new JsonElement();//Opcional
            Boolean RefFlag = false; //BANDERIN PARA REFERENCIA YA QUE ES JSON Y NO PODEMOS VERIFICAR CORRECTAMENTE LOS DATOS VACIOS
            Boolean DscRcgFlag = false;
            

            //SI REFERENCIA TRAE DATOS, EL BANDERIN CAMBIA A TRUE PARA CREAR REFERENCIA EN FACTURA
            if (dte.TryGetProperty("Referencia", out var Referencia))
            {
                ReferenciaOp = Referencia;
                RefFlag = true;
            }

            if (dte.TryGetProperty("DscRcgGlobal", out var DscRcgGlobal))
            {
                DscRcgGlobalOp = DscRcgGlobal;
                DscRcgFlag = true;
            }

            JsonElement IdDoc = Encabezado.GetProperty("IdDoc");
            int TipoDTE = int.Parse(IdDoc.GetProperty("TipoDTE").ToString());
            JsonElement Receptor = Encabezado.GetProperty("Receptor");
            string strRUTRecep = Receptor.GetProperty("RUTRecep").ToString();// Requerido

            int nuevoFolio = 0;
            string directorioFechaActual = "";
            string fileNameXML = "";
            string directorioDTE = "";
            string directorio = "";
            string queryEliminar = "";
            string ErrorValidarMontos = "";
            string directorioSobre ="";
            string directorioSobreCliente ="";
            string respuestaSchema ="";
            string respuestaSchemaCliente = "";
            string queryUpdateDirectorio = "";
            
            
            switch (TipoDTE)
            {
                case 33: // <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<-----------FACTURA----------->>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> 

                Factura factura = new Factura();

                //GENERAR FOLIO NUEVO PARA EL DTE
                nuevoFolio = generarFolio(TipoDTE);

                //CREAR DIRECTORIO
                string T33F = "T"+TipoDTE+"F"+nuevoFolio;
                directorioFechaActual = crearDirectorio(nuevoFolio,TipoDTE,T33F);
                directorioDTE = directorioFechaActual + @"\" + T33F + ".xml";          

                //GENERAR LA FACTURA    
                directorio = factura.crearFactura(Encabezado, Detalles, ReferenciaOp, RefFlag, DscRcgGlobalOp, DscRcgFlag, nuevoFolio, directorioDTE, T33F);

                if(directorio.Contains("Error")){

                    //BORRA LA FACTURA DE LA BD
                    queryEliminar ="DELETE FROM factura WHERE folio_factura = '" + nuevoFolio + "'";
                    conexion.Consulta(queryEliminar);

                    ErrorValidarMontos = directorio;
                    respuestaCrearDTE[0] = ErrorValidarMontos;
                    respuestaCrearDTE[1] = "";
                    return respuestaCrearDTE;
                }

                //GENERAR EL SOBRE DE ENVIO
                directorioSobre = envioDTE.crearSobreEnvio(TipoDTE.ToString(),directorioDTE, directorioFechaActual,nuevoFolio);
                directorioSobreCliente = envioDTE.crearSobreEnvioCliente(TipoDTE.ToString(),directorioDTE, directorioFechaActual,nuevoFolio,strRUTRecep);


                //VALIDAR CON LOS SCHEMAS EL SOBRE
                respuestaSchema = schemas.validarXML(directorioSobre, TipoDTE.ToString());
                respuestaSchemaCliente = schemas.validarXML(directorioSobreCliente, TipoDTE.ToString());

                 

                if (respuestaSchema == "XML Valido" && respuestaSchemaCliente == "XML Valido"){
                //UPDATEAR LA UBICACION DEL XML DE LA FACTURA
                queryUpdateDirectorio = "UPDATE factura SET ubicacion_factura = '" + directorio + "' WHERE folio_factura = '" + nuevoFolio + "'";
                queryUpdateDirectorio = queryUpdateDirectorio.Replace("\\", "\\\\");
                conexion.Consulta(queryUpdateDirectorio);                   

                }else{
                    //BORRA LA FACTURA DE LA BD
                    queryEliminar ="DELETE FROM factura WHERE folio_factura = '" + nuevoFolio + "'";
                    conexion.Consulta(queryEliminar);
                    //UPDATEAR EL ESTADO DEL SOBRE A NO VALIDO POR SCHEMA                    
                }
                
                respuestaCrearDTE[0] = respuestaSchema;
                respuestaCrearDTE[1] = directorioSobre;
                respuestaCrearDTE[2] = nuevoFolio.ToString();
                respuestaCrearDTE[3] = TipoDTE.ToString();
                respuestaCrearDTE[4] = respuestaSchemaCliente;
                respuestaCrearDTE[5] = directorioSobreCliente;
                respuestaCrearDTE[6] = strRUTRecep;


                //COMENTABLE


                return respuestaCrearDTE;
                
                break;

                case 34: // <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<-----------FACTURA EXENTA----------->>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> 
                
                FacturaExenta facturaExenta = new FacturaExenta();
                //GENERAR FOLIO NUEVO PARA EL DTE
                nuevoFolio = generarFolio(TipoDTE);

                //CREAR DIRECTORIO
                string T34F = "T"+ TipoDTE + "F" + nuevoFolio;
                directorioFechaActual = crearDirectorio(nuevoFolio, TipoDTE, T34F);
                directorioDTE = directorioFechaActual + @"\" + T34F + ".xml";



                    //GENERAR LA FACTURA    
                    directorio = facturaExenta.crearFacturaExenta(Encabezado, Detalles, ReferenciaOp, RefFlag, DscRcgGlobalOp, DscRcgFlag, nuevoFolio, directorioDTE, T34F);

                if(directorio.Contains("Error")){

                    //BORRA LA FACTURA DE LA BD
                    queryEliminar ="DELETE FROM factura_exenta WHERE folio_factura_exenta = '" + nuevoFolio + "'";
                    conexion.Consulta(queryEliminar);

                    ErrorValidarMontos = directorio;
                    respuestaCrearDTE[0] = ErrorValidarMontos;
                    respuestaCrearDTE[1] = "";
                    return respuestaCrearDTE;
                }

               //GENERAR EL SOBRE DE ENVIO
                directorioSobre = envioDTE.crearSobreEnvio(TipoDTE.ToString(),directorioDTE, directorioFechaActual,nuevoFolio);
                directorioSobreCliente = envioDTE.crearSobreEnvioCliente(TipoDTE.ToString(),directorioDTE, directorioFechaActual,nuevoFolio,strRUTRecep);


                //VALIDAR CON LOS SCHEMAS EL SOBRE
                respuestaSchema = schemas.validarXML(directorioSobre, TipoDTE.ToString());
                respuestaSchemaCliente = schemas.validarXML(directorioSobreCliente, TipoDTE.ToString());

                if(respuestaSchema == "XML Valido" && respuestaSchemaCliente == "XML Valido"){
                    //UPDATEAR LA UBICACION DEL XML DE LA FACTURA
                    queryUpdateDirectorio = "UPDATE factura_exenta SET ubicacion_factura_exenta = '" + directorio + "' WHERE folio_factura_exenta = '" + nuevoFolio + "'";
                    queryUpdateDirectorio = queryUpdateDirectorio.Replace("\\", "\\\\");
                    conexion.Consulta(queryUpdateDirectorio);                   

                }else{
                    //BORRA LA FACTURA DE LA BD
                    queryEliminar ="DELETE FROM factura_exenta WHERE folio_factura_exenta = '" + nuevoFolio + "'";
                    conexion.Consulta(queryEliminar);
                    //UPDATEAR EL ESTADO DEL SOBRE A NO VALIDO POR SCHEMA                    
                }
                
                respuestaCrearDTE[0] = respuestaSchema;
                respuestaCrearDTE[1] = directorioSobre;
                respuestaCrearDTE[2] = nuevoFolio.ToString();
                respuestaCrearDTE[3] = TipoDTE.ToString();
                respuestaCrearDTE[4] = respuestaSchemaCliente;
                respuestaCrearDTE[5] = directorioSobreCliente;
                 respuestaCrearDTE[6] = strRUTRecep;

                    return respuestaCrearDTE;

               
                break;

                case 61:// <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<-----------NOTA DE CREDITO----------->>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> 
                    
                    NotaCredito notacredito = new NotaCredito();

                    //GENERAR FOLIO NUEVO PARA EL DTE
                    nuevoFolio = generarFolio(TipoDTE);

                    //CREAR DIRECTORIO
                    string T61NC = "T" + TipoDTE + "NC" + nuevoFolio;
                    directorioFechaActual = crearDirectorio(nuevoFolio, TipoDTE, T61NC);
                    directorioDTE = directorioFechaActual + @"\" + T61NC + ".xml";

                   

                    

                    //GENERAR LA NOTA DE CREDITO   
                    directorio = notacredito.crearNotaCredito(Encabezado, Detalles, ReferenciaOp, RefFlag, DscRcgGlobalOp, DscRcgFlag, nuevoFolio, directorioDTE, T61NC);

                    if (directorio.Contains("Error"))
                    {

                        //BORRA LA NOTA DE CREDITO DE LA BD
                        queryEliminar = "DELETE FROM nota_credito WHERE folio_nota_credito = '" + nuevoFolio + "'";
                        conexion.Consulta(queryEliminar);

                        ErrorValidarMontos = directorio;
                        respuestaCrearDTE[0] = ErrorValidarMontos;
                        respuestaCrearDTE[1] = "";
                        return respuestaCrearDTE;
                    }

                    //GENERAR EL SOBRE DE ENVIO
                    directorioSobre = envioDTE.crearSobreEnvio(TipoDTE.ToString(),directorioDTE, directorioFechaActual,nuevoFolio);
                    directorioSobreCliente = envioDTE.crearSobreEnvioCliente(TipoDTE.ToString(),directorioDTE, directorioFechaActual,nuevoFolio,strRUTRecep);


                    //VALIDAR CON LOS SCHEMAS EL SOBRE
                    respuestaSchema = schemas.validarXML(directorioSobre, TipoDTE.ToString());
                    respuestaSchemaCliente = schemas.validarXML(directorioSobreCliente, TipoDTE.ToString());


                    if (respuestaSchema == "XML Valido" && respuestaSchemaCliente == "XML Valido")
                    {
                        //UPDATEAR LA UBICACION DEL XML DE LA NOTA DE CREDITO
                        queryUpdateDirectorio = "UPDATE nota_credito SET ubicacion_nota_credito = '" + directorio + "' WHERE folio_nota_credito = '" + nuevoFolio + "'";
                        queryUpdateDirectorio = queryUpdateDirectorio.Replace("\\", "\\\\");
                        conexion.Consulta(queryUpdateDirectorio);

                    }
                    else
                    {
                        //BORRA LA NOTA CCREDITO DE LA BD
                        queryEliminar = "DELETE FROM nota_credito WHERE folio_nota_credito = '" + nuevoFolio + "'";
                        conexion.Consulta(queryEliminar);
                        //UPDATEAR EL ESTADO DEL SOBRE A NO VALIDO POR SCHEMA                    
                    }

                    respuestaCrearDTE[0] = respuestaSchema;
                    respuestaCrearDTE[1] = directorioSobre;
                    respuestaCrearDTE[2] = nuevoFolio.ToString();
                    respuestaCrearDTE[3] = TipoDTE.ToString();
                    respuestaCrearDTE[4] = respuestaSchemaCliente;
                    respuestaCrearDTE[5] = directorioSobreCliente;
                     respuestaCrearDTE[6] = strRUTRecep;

                    return respuestaCrearDTE;


                    break;

                case 56:// <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<-----------NOTA DE DEBITO----------->>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> 
                    
                    NotaDebito notaDebito = new NotaDebito();

                    //GENERAR FOLIO NUEVO PARA EL DTE
                    nuevoFolio = generarFolio(TipoDTE);

                    //CREAR DIRECTORIO
                    string T56ND = "T" + TipoDTE + "ND" + nuevoFolio;
                    directorioFechaActual = crearDirectorio(nuevoFolio, TipoDTE, T56ND);
                    directorioDTE = directorioFechaActual + @"\" + T56ND + ".xml";
                    
                    

                    //GENERAR LA NOTA DE CREDITO   
                    directorio = notaDebito.crearNotaDebito(Encabezado, Detalles, ReferenciaOp, RefFlag, DscRcgGlobalOp, DscRcgFlag, nuevoFolio, directorioDTE, T56ND);

                    if (directorio.Contains("Error"))
                    {

                        //BORRA LA NOTA DE CREDITO DE LA BD
                        queryEliminar = "DELETE FROM nota_debito WHERE folio_nota_debito = '" + nuevoFolio + "'";
                        conexion.Consulta(queryEliminar);

                        ErrorValidarMontos = directorio;
                        respuestaCrearDTE[0] = ErrorValidarMontos;
                        respuestaCrearDTE[1] = "";
                        return respuestaCrearDTE;
                    }

                    //GENERAR EL SOBRE DE ENVIO
                    directorioSobre = envioDTE.crearSobreEnvio(TipoDTE.ToString(),directorioDTE, directorioFechaActual,nuevoFolio);
                    directorioSobreCliente = envioDTE.crearSobreEnvioCliente(TipoDTE.ToString(),directorioDTE, directorioFechaActual,nuevoFolio,strRUTRecep);


                    //VALIDAR CON LOS SCHEMAS EL SOBRE
                    respuestaSchema = schemas.validarXML(directorioSobre, TipoDTE.ToString());
                    respuestaSchemaCliente = schemas.validarXML(directorioSobreCliente, TipoDTE.ToString());


                    if (respuestaSchema == "XML Valido" && respuestaSchemaCliente == "XML Valido")
                    {
                        //UPDATEAR LA UBICACION DEL XML DE LA NOTA DE CREDITO
                        queryUpdateDirectorio = "UPDATE nota_debito SET ubicacion_nota_debito = '" + directorio + "' WHERE folio_nota_debito = '" + nuevoFolio + "'";
                        queryUpdateDirectorio = queryUpdateDirectorio.Replace("\\", "\\\\");
                        conexion.Consulta(queryUpdateDirectorio);

                    }
                    else
                    {
                        //BORRA LA NOTA CCREDITO DE LA BD
                        queryEliminar = "DELETE FROM nota_debito WHERE folio_nota_debito = '" + nuevoFolio + "'";
                        conexion.Consulta(queryEliminar);
                        //UPDATEAR EL ESTADO DEL SOBRE A NO VALIDO POR SCHEMA                    
                    }

                    respuestaCrearDTE[0] = respuestaSchema;
                    respuestaCrearDTE[1] = directorioSobre;
                    respuestaCrearDTE[2] = nuevoFolio.ToString();
                    respuestaCrearDTE[3] = TipoDTE.ToString();
                    respuestaCrearDTE[4] = respuestaSchemaCliente;
                    respuestaCrearDTE[5] = directorioSobreCliente;
                     respuestaCrearDTE[6] = strRUTRecep;

                    return respuestaCrearDTE;



                    break;

                case 52: // <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<-----------GUIA DESPACHO----------->>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> 

                    GuiaDespacho guiadespacho = new GuiaDespacho();
                   
                    //GENERAR FOLIO NUEVO PARA EL DTE
                    nuevoFolio = generarFolio(TipoDTE);

                    //CREAR DIRECTORIO
                    string T52GD = "T" + TipoDTE + "GD" + nuevoFolio;
                    directorioFechaActual = crearDirectorio(nuevoFolio, TipoDTE, T52GD);
                    directorioDTE = directorioFechaActual + @"\" + T52GD + ".xml";
                  

                    

                    //GENERAR GUIA DESPACHO  
                    directorio = guiadespacho.crearGuiaDespacho(Encabezado, Detalles, ReferenciaOp, RefFlag, DscRcgGlobalOp, DscRcgFlag, nuevoFolio, directorioDTE, T52GD);

                    if (directorio.Contains("Error"))
                    {

                        //BORRA LA FACTURA DE LA BD
                        queryEliminar = "DELETE FROM guia_despacho WHERE folio_guia_despacho = '" + nuevoFolio + "'";
                        conexion.Consulta(queryEliminar);

                        ErrorValidarMontos = directorio;
                        respuestaCrearDTE[0] = ErrorValidarMontos;
                        respuestaCrearDTE[1] = "";
                        return respuestaCrearDTE;
                    }

                    //GENERAR EL SOBRE DE ENVIO
                    directorioSobre = envioDTE.crearSobreEnvio(TipoDTE.ToString(),directorioDTE, directorioFechaActual,nuevoFolio);
                    directorioSobreCliente = envioDTE.crearSobreEnvioCliente(TipoDTE.ToString(),directorioDTE, directorioFechaActual,nuevoFolio,strRUTRecep);


                    //VALIDAR CON LOS SCHEMAS EL SOBRE
                    respuestaSchema = schemas.validarXML(directorioSobre, TipoDTE.ToString());
                    respuestaSchemaCliente = schemas.validarXML(directorioSobreCliente, TipoDTE.ToString());

                    if (respuestaSchema == "XML Valido" && respuestaSchemaCliente == "XML Valido")
                    {
                        //UPDATEAR LA UBICACION DEL XML DE LA FACTURA
                        queryUpdateDirectorio = "UPDATE guia_despacho SET ubicacion_guia_despacho = '" + directorio + "' WHERE folio_guia_despacho = '" + nuevoFolio + "'";
                        queryUpdateDirectorio = queryUpdateDirectorio.Replace("\\", "\\\\");
                        conexion.Consulta(queryUpdateDirectorio);

                    }
                    else
                    {
                        //BORRA LA FACTURA DE LA BD
                        queryEliminar = "DELETE FROM guia_despacho WHERE folio_guia_despacho = '" + nuevoFolio + "'";
                        conexion.Consulta(queryEliminar);
                        //UPDATEAR EL ESTADO DEL SOBRE A NO VALIDO POR SCHEMA                    
                    }

                    respuestaCrearDTE[0] = respuestaSchema;
                    respuestaCrearDTE[1] = directorioSobre;
                    respuestaCrearDTE[2] = nuevoFolio.ToString();
                    respuestaCrearDTE[3] = TipoDTE.ToString();
                    respuestaCrearDTE[4] = respuestaSchemaCliente;
                    respuestaCrearDTE[5] = directorioSobreCliente;
                     respuestaCrearDTE[6] = strRUTRecep;

                    return respuestaCrearDTE;

                    break;

                case 39: // <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<-----------BOLETA----------->>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> 
                    Boleta boleta = new Boleta();

                    //GENERAR FOLIO NUEVO PARA EL DTE
                    nuevoFolio = generarFolio(TipoDTE);

                    //CREAR DIRECTORIO
                    string T39B = "T" + TipoDTE + "B" + nuevoFolio;
                    directorioFechaActual = crearDirectorio(nuevoFolio, TipoDTE, T39B);
                    directorioDTE = directorioFechaActual + @"\" + T39B + ".xml";
                   

                   

                    //GENERAR LA FACTURA    
                    directorio = boleta.crearBoleta(Encabezado, Detalles, ReferenciaOp, RefFlag, DscRcgGlobalOp, DscRcgFlag, nuevoFolio, directorioDTE, T39B);

                    if (directorio.Contains("Error"))
                    {

                        //BORRA LA FACTURA DE LA BD
                        queryEliminar = "DELETE FROM boleta WHERE folio_boleta = '" + nuevoFolio + "'";
                        conexion.Consulta(queryEliminar);

                        ErrorValidarMontos = directorio;
                        respuestaCrearDTE[0] = ErrorValidarMontos;
                        respuestaCrearDTE[1] = "";
                        return respuestaCrearDTE;
                    }

                    //GENERAR EL SOBRE DE ENVIO
                    directorioSobre = envioDTE.crearSobreEnvio(TipoDTE.ToString(), directorioDTE, directorioFechaActual, nuevoFolio);

                    //VALIDAR CON LOS SCHEMAS EL SOBRE
                    respuestaSchema = schemas.validarXML(directorioSobre,TipoDTE.ToString());

                    if (respuestaSchema == "XML Valido")
                    {
                        //UPDATEAR LA UBICACION DEL XML DE LA BOLETA
                        queryUpdateDirectorio = "UPDATE boleta SET ubicacion_boleta = '" + directorio + "' WHERE folio_boleta = '" + nuevoFolio + "'";
                        queryUpdateDirectorio = queryUpdateDirectorio.Replace("\\", "\\\\");
                        conexion.Consulta(queryUpdateDirectorio);

                    }
                    else
                    {
                        //BORRA LA FACTURA DE LA BD
                        queryEliminar = "DELETE FROM boleta WHERE folio_boleta = '" + nuevoFolio + "'";
                        conexion.Consulta(queryEliminar);
                        //UPDATEAR EL ESTADO DEL SOBRE A NO VALIDO POR SCHEMA                    
                    }

                    respuestaCrearDTE[0] = respuestaSchema;
                    respuestaCrearDTE[1] = directorioSobre;
                    respuestaCrearDTE[2] = nuevoFolio.ToString();
                    respuestaCrearDTE[3] = TipoDTE.ToString(); 
                    respuestaCrearDTE[4] = "XML Valido";
                    respuestaCrearDTE[5] = "";   
                    respuestaCrearDTE[6] = "";                 

                    return respuestaCrearDTE;

                    break;

                case 41: // <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<-----------BOLETA EXENTA----------->>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> 
                    BoletaExenta boleta_exenta = new BoletaExenta();

                    //GENERAR FOLIO NUEVO PARA EL DTE
                    nuevoFolio = generarFolio(TipoDTE);

                    //CREAR DIRECTORIO
                    string T41B = "T" + TipoDTE + "B" + nuevoFolio;
                    directorioFechaActual = crearDirectorio(nuevoFolio, TipoDTE, T41B);
                    directorioDTE = directorioFechaActual + @"\" + T41B + ".xml";
                    

                   

                    //GENERAR LA FACTURA    
                    directorio = boleta_exenta.crearBoleta(Encabezado, Detalles, ReferenciaOp, RefFlag, DscRcgGlobalOp, DscRcgFlag, nuevoFolio, directorioDTE, T41B);

                    if (directorio.Contains("Error"))
                    {

                        //BORRA LA FACTURA DE LA BD
                        queryEliminar = "DELETE FROM boleta_exenta WHERE folio_boleta_exenta = '" + nuevoFolio + "'";
                        conexion.Consulta(queryEliminar);

                        ErrorValidarMontos = directorio;
                        respuestaCrearDTE[0] = ErrorValidarMontos;
                        respuestaCrearDTE[1] = "";
                        return respuestaCrearDTE;
                    }

                    //GENERAR EL SOBRE DE ENVIO
                    directorioSobre = envioDTE.crearSobreEnvio(TipoDTE.ToString(), directorioDTE, directorioFechaActual, nuevoFolio);

                    //VALIDAR CON LOS SCHEMAS EL SOBRE
                    respuestaSchema = schemas.validarXML(directorioSobre, TipoDTE.ToString());

                    if (respuestaSchema == "XML Valido")
                    {
                        //UPDATEAR LA UBICACION DEL XML DE LA BOLETA
                        queryUpdateDirectorio = "UPDATE boleta_exenta SET ubicacion_boleta_exenta = '" + directorio + "' WHERE folio_boleta_exenta = '" + nuevoFolio + "'";
                        queryUpdateDirectorio = queryUpdateDirectorio.Replace("\\", "\\\\");
                        conexion.Consulta(queryUpdateDirectorio);

                    }
                    else
                    {
                        //BORRA LA FACTURA DE LA BD
                        queryEliminar = "DELETE FROM boleta_exenta WHERE folio_boleta_exenta = '" + nuevoFolio + "'";
                        conexion.Consulta(queryEliminar);
                        //UPDATEAR EL ESTADO DEL SOBRE A NO VALIDO POR SCHEMA                    
                    }

                    respuestaCrearDTE[0] = respuestaSchema;
                    respuestaCrearDTE[1] = directorioSobre;
                    respuestaCrearDTE[2] = nuevoFolio.ToString();
                    respuestaCrearDTE[3] = TipoDTE.ToString();
                    respuestaCrearDTE[4] = "XML Valido";
                    respuestaCrearDTE[5] = ""; 
                    respuestaCrearDTE[6] = "";  

                    return respuestaCrearDTE;

                    break;
                case 800:

                    break;
                default:
                    break;
            }

           
            return respuestaCrearDTE;
            
        }

        public int generarFolio(int TipoDTE){

            string tableName = "";
            string folioDte = "";

            if(TipoDTE == 33)
            {
                tableName = "factura";
                folioDte = "folio_factura";

            }
            else if(TipoDTE == 34)
            {
                tableName = "factura_exenta";
                folioDte = "folio_factura_exenta";
            }
            else if(TipoDTE == 61)
            {
                tableName = "nota_credito";
                folioDte = "folio_nota_credito";
            }
            else if(TipoDTE == 56)
            {
                tableName = "nota_debito";
                folioDte = "folio_nota_debito";
            }
            else if (TipoDTE == 52)
            {
                tableName = "guia_despacho";
                folioDte = "folio_guia_despacho";
            }

            else if (TipoDTE == 39)
            {
                tableName = "boleta";
                folioDte = "folio_boleta";
            }
            else if (TipoDTE == 41)
            {
                tableName = "boleta_exenta";
                folioDte = "folio_boleta_exenta";
            }

            string datosCaf = "SELECT rango_minimo_caf, rango_maximo_caf FROM xml_caf WHERE tipo_documento_caf="+TipoDTE+" AND estado_caf=1";
            string folioFactura = "SELECT "+folioDte+" FROM "+tableName+" WHERE "+folioDte+"=(SELECT max("+folioDte+") FROM "+tableName+")";

            //string directorioCAF = Path.Combine(Environment.CurrentDirectory, "", @"\CAF_PRUEBA\FoliosSII76958430333012022124144.xml");
           
           //INSTANCIAMOS LA CONEXION Y TRAEMOS LOS RANGOS MAXIMOS Y MINIMOS DEL CAF DEPENDIENDO DEL TIPO DE DOCUMENTO
            ConexionBD conexion = new ConexionBD();
            List<string> resultCaf = conexion.Select(datosCaf);
            List<string> resultFolio = conexion.Select(folioFactura);

            int rangoMin = int.Parse(resultCaf[0]);
            int rangoMax = int.Parse(resultCaf[1]);
            int folio = int.Parse(resultFolio[0]);

            //SI EL FOLIO RESCATADO ESTÁ DENTRO DEL RANGO DEL CAF GENERA EL SIGUIENTE FOLIO Y LO INSERTA EL LA BASE DE DATOS
            if (folio >= rangoMin && folio < rangoMax) {

                int folioNuevo = folio + 1;
                string consultaSql = "INSERT INTO "+tableName+" ("+folioDte+") VALUES ('" + folioNuevo + "')";
                conexion.Consulta(consultaSql);

                return folioNuevo;
            }else if (folio >= rangoMax ) {

                folio = folio +1 ;

                string consultaDeshabilitar = "UPDATE xml_caf SET estado_caf = 0 WHERE estado_caf = 1 AND tipo_documento_caf = '"+TipoDTE+"' ";
                string consultaHabilitar = "UPDATE xml_caf SET estado_caf = 1 WHERE rango_minimo_caf = "+folio+" AND estado_caf = 0 AND tipo_documento_caf ='"+TipoDTE+"'";

                conexion.Consulta(consultaDeshabilitar);
                conexion.Consulta(consultaHabilitar);

                string folioFactura2 = "SELECT "+folioDte+" FROM "+tableName+" WHERE "+folioDte+"=(SELECT max("+folioDte+") FROM "+tableName+")";
                List<string> resultFolio2 = conexion.Select(folioFactura2);

                int folio2 = int.Parse(resultFolio2[0]);

                int folioNuevo2 = folio2 + 1;

                string consultaSql2 = "INSERT INTO "+tableName+" ("+folioDte+") VALUES ('" + folioNuevo2 + "')";
                conexion.Consulta(consultaSql2);

	 		    return folioNuevo2;
            }	 		 		
	 	


            //Debug.Write(result);

            return 0;
        }

        public static string crearDirectorio(int folio,int TipoDTE,string T33F)
        {
            //Crear ruta del archivo xml saliente
            string currentDay = DateTime.Now.Day.ToString();
            string currentMonth = DateTime.Now.Month.ToString();
            string currentYear = DateTime.Now.Year.ToString();
            string archxml = "";
            return archxml = verificarDirectorio(T33F, currentYear, "M" + currentMonth, "D" + currentDay);
        }

        public string crearDirectorioTimbre()
        {
            //Crear ruta del archivo xml saliente
            string currentDay = DateTime.Now.Day.ToString();
            string currentMonth = DateTime.Now.Month.ToString();
            string currentYear = DateTime.Now.Year.ToString();
            string archxml = "";
            return archxml = verificarDirectorioTimbre( currentYear, "M" + currentMonth, "D" + currentDay);
        }

        public string verificarDirectorioTimbre(string year, string mes, string dia)
        {

            //VERIFICAR AÑO
            string folderPathYear = @"C:\inetpub\wwwroot\api_agrodte\AgroDTE_Archivos\Timbre\" + year;

            if (!Directory.Exists(folderPathYear))
            {
                Directory.CreateDirectory(folderPathYear);
            }
            //VERIFICAR MES
            string folderPathMes = @"C:\inetpub\wwwroot\api_agrodte\AgroDTE_Archivos\Timbre\" + year + @"\" + mes;
            if (!Directory.Exists(folderPathMes))
            {
                Directory.CreateDirectory(folderPathMes);
            }

            //VERIFICAR DIA
            string folderPathDia = @"C:\inetpub\wwwroot\api_agrodte\AgroDTE_Archivos\Timbre\" + year + @"\" + mes + @"\" + dia;
            if (!Directory.Exists(folderPathDia))
            {
                Directory.CreateDirectory(folderPathDia);
            }


            return folderPathDia;
        }

        public static string verificarDirectorio(string nombreDTE, string year, string mes, string dia)
        {

            //VERIFICAR AÑO
            string folderPathYear = @"C:\inetpub\wwwroot\api_agrodte\AgroDTE_Archivos\XML\" + year;

            if (!Directory.Exists(folderPathYear))
            {
                Directory.CreateDirectory(folderPathYear);
            }
            //VERIFICAR MES
            string folderPathMes = @"C:\inetpub\wwwroot\api_agrodte\AgroDTE_Archivos\XML\" + year + @"\" + mes;
            if (!Directory.Exists(folderPathMes))
            {
                Directory.CreateDirectory(folderPathMes);
            }

            //VERIFICAR DIA
            string folderPathDia = @"C:\inetpub\wwwroot\api_agrodte\AgroDTE_Archivos\XML\" + year + @"\" + mes + @"\" + dia;
            if (!Directory.Exists(folderPathDia))
            {
                Directory.CreateDirectory(folderPathDia);
            }


            return folderPathDia;
        }

        public string [] generarTimbrePDF417(string texto, string folio, string TipoDTE)
        {
           
            BarcodePDF417 pdf417 = new BarcodePDF417();
            pdf417.Options = BarcodePDF417.PDF417_USE_ASPECT_RATIO;
            pdf417.ErrorLevel = 8;

            pdf417.Options = BarcodePDF417.PDF417_FORCE_BINARY;
            Encoding iso = Encoding.GetEncoding("ISO-8859-1");
            byte[] isoBytes = iso.GetBytes(texto);

            pdf417.Text = isoBytes;
            // pdf417.SetText(contenido);

            //pdf417.CreateDrawingImage(Color.Black, Color.White).Save("C:\\Temp\\iTextSharp_cs_01.jpg", ImageFormat.Jpeg);

            string directorioFechaActual = crearDirectorioTimbre();
            string directorioTimbre = directorioFechaActual + @"\DTE"+ TipoDTE +"_"+ folio + ".png";

            Bitmap imagen = new Bitmap(pdf417.CreateDrawingImage(Color.Black, Color.White), new Size(1062,354));
            imagen.Save(directorioTimbre, ImageFormat.Png);


            //DEVOLVER EL PDF CONVERTIDO EN BASE64
            Byte[] bytes = File.ReadAllBytes(directorioTimbre);
            string file_png = Convert.ToBase64String(bytes);

            string[] respuesta = new string[2];
            respuesta[0] = file_png;
            respuesta[1] = directorioTimbre;
            return respuesta;
        }





    }
}
