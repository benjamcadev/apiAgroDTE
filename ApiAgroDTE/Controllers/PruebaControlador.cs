using ApiAgroDTE.Clases;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace ApiAgroDTE.Controllers
{
    //[Route("api/[controller]")]
    [ApiController]
    public class PruebaControlador : ControllerBase
    {
        /*SERVIDORES:
       apicert - Certificacion Boletas Token
       api - Produccion Boletas Token
       maullin - Certificacion facturas token
       palena - Produccion Facturas 
       */


        public static string servidor_boletas = "api"; //api: produccion --  apicert: certificacion
        public static string servidor_facturas = "palena"; //maullin: certificacion -- palena: produccion
        public static string directorio_archivos = @"C:\inetpub\wwwroot\api_agrodte\AgroDTE_Archivos";

       
        [HttpGet("api/dte/taxpayer/{rut}")]
        public ContentResult datosCliente(string rut)
        {

            //GUARDA LAS REQUEST
            string archivostr = rut.ToString();
            string hora_actual = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff");
            using (StreamWriter writetext = new StreamWriter(@"C:\inetpub\wwwroot\api_agrodte\AgroDTE_Archivos\Log\Cliente_Request_" + hora_actual + ".txt"))
            {
                writetext.WriteLine(archivostr);
            }

            ContentResult respuesta = new ContentResult();
            string JsonResponse;

            Operaciones op = new Operaciones();
            if (op.ValidaRut(rut))
            {

            }
            else
            {
                JsonResponse = @"{""error"": { ""message"": ""Validación de Campos"", ""code"": ""OF-10"",""details"": [{"
                                           + @"""field"": ""Error"","
                                           + @"""issue"": ""Rut Incorrecto"" } ] } }";
                respuesta.Content = JsonResponse;
                respuesta.ContentType = "application /json";
                respuesta.StatusCode = 400;
                return respuesta;
            }



            string consulta = "SELECT contribuyentes_direccion.rut,contribuyentes_acteco.razon_social,"
                                + "contribuyentes_direccion.calle,contribuyentes_direccion.numero,"
                                + "contribuyentes_direccion.ciudad,contribuyentes_direccion.comuna,"
                                + "contribuyentes_direccion.codigo"
                                + " FROM contribuyentes_direccion"
                                + " INNER JOIN contribuyentes_acteco ON contribuyentes_acteco.rut = contribuyentes_direccion.rut" 
                                + @" WHERE contribuyentes_direccion.rut = """ + rut + @""""
                                + " GROUP BY calle";

            //ConexionBD conexion = new ConexionBD();
            //APUNTAMOS A OTRO SERVIDOR PARA NO CARGAR AL SERVIDOR PRINCIPAL
            ConexionBD_2 conexion2 = new ConexionBD_2();
            List<string> lista_datos = conexion2.Select(consulta);
            

            string consulta_correo = "SELECT correo,fono_contacto FROM contribuyentes_correo WHERE rut = \"" + rut + "\"";
            List<string> lista_datos_correo = conexion2.Select(consulta_correo);

            string correo = "";
            string fono = "";
            if (lista_datos_correo.Count == 0)
            {
                correo = "0";
                fono = "0";
            }
            else
            {
                correo = lista_datos_correo[0];
                fono = lista_datos_correo[1];
            }



            if (lista_datos.Count == 0)
            {
                JsonResponse = @"{""error"": { ""message"": ""Problema al procesar los datos"", ""code"": ""OF-22"",""details"": {"
                                        + @"""message"": ""Ocurrió un error inesperado, favor de re intentar más tarde o contactese con soporte."","
                                        + @"""code"": 500 } } }";
                respuesta.Content = JsonResponse;
                respuesta.ContentType = "application /json";
                respuesta.StatusCode = 400;
                return respuesta;

            }


            string consulta_acteco = @"SELECT des_actividad_economica,des_actividad_economica,codigo_actividad FROM contribuyentes_acteco WHERE rut = """+rut+@"""";
            List<string> lista_datos_acteco = conexion2.Select(consulta_acteco);


            JsonResponse = @"{""rut"": """ + lista_datos[0] + @""",""razonSocial"": """ + lista_datos[1] + @""",""email"": """ + correo + @""",""telefono"": """ + fono + @""",""direccion"": """ + lista_datos[2] + " " + lista_datos[3] + " " + lista_datos[4] + @" "",""comuna"": """ + lista_datos[5] + @""",";

            JsonResponse = JsonResponse + @"""actividades"": [";
            //CICLO PARA ESCRIBIR LAS ACTIVIDADES ECONOMICAS
            bool actividadPrincipal = true;
            int contador_giro = 0, contador_actividadEconomica = 1,contador_codigo_actividad = 2;

            for (int i = 0; i < lista_datos_acteco.Count / 3; i++)
            {
                
                JsonResponse = JsonResponse + "{";
                JsonResponse = JsonResponse + @"""giro"": """ + lista_datos_acteco[contador_giro] + @""",";
                JsonResponse = JsonResponse + @"""actividadEconomica"": """ + lista_datos_acteco[contador_actividadEconomica] + @""",";
                JsonResponse = JsonResponse + @"""codigoActividadEconomica"": """ + lista_datos_acteco[contador_codigo_actividad] + @""",";

                if (actividadPrincipal)
                {
                  JsonResponse = JsonResponse + @"""actividadPrincipal"": true";
                  actividadPrincipal = false;
                }
                else
                {
                  JsonResponse = JsonResponse + @"""actividadPrincipal"": false";
                }
               

                JsonResponse = JsonResponse + "}";
                if (i + 1 == lista_datos_acteco.Count / 3)
                {
                    JsonResponse = JsonResponse + "";
                }
                else { JsonResponse = JsonResponse + ","; }

                contador_giro = contador_giro + 3;
                contador_actividadEconomica = contador_actividadEconomica + 3;
                contador_codigo_actividad = contador_codigo_actividad + 3;

            }

            JsonResponse = JsonResponse + "],";


            JsonResponse = JsonResponse + @"""sucursales"": [";
            //CICLO PARA ESCRIBIR LAS SUCURSALES
            int contador_cdgSIISucur = 6, contador_comuna = 5, contador_direccion = 2,contador_direccion_numero = 3, contador_ciudad = 4, contador_telefono = 3;
            for (int i = 0; i < lista_datos.Count / 7; i++)
            {
               

                JsonResponse = JsonResponse + "{";
                JsonResponse = JsonResponse + @"""cdgSIISucur"": """ + lista_datos[contador_cdgSIISucur] + @""",";
                JsonResponse = JsonResponse + @"""comuna"": """ + lista_datos[contador_comuna] + @""",";
                JsonResponse = JsonResponse + @"""direccion"": """ + lista_datos[contador_direccion] +" "+ lista_datos[contador_direccion_numero] +" "+ lista_datos[contador_ciudad] + @""",";
                JsonResponse = JsonResponse + @"""ciudad"": """ + lista_datos[contador_ciudad] + @""",";
                JsonResponse = JsonResponse + @"""telefono"": """ + fono + @"""";

                JsonResponse = JsonResponse + "}";
                if (i + 1 == lista_datos.Count / 7)
                {
                    JsonResponse = JsonResponse + "";
                }
                else { JsonResponse = JsonResponse + ","; }

                contador_cdgSIISucur = contador_cdgSIISucur + 7;
                contador_comuna = contador_comuna + 7;
                contador_direccion = contador_direccion + 7;
                contador_direccion_numero = contador_direccion_numero + 7;
                contador_ciudad = contador_ciudad + 7;
                contador_telefono = contador_telefono + 7;


            }

            JsonResponse = JsonResponse + "]";

            JsonResponse  =  JsonResponse + "}";

            respuesta.Content = JsonResponse;
            respuesta.ContentType = "application/json";
            respuesta.StatusCode = 200;



            return respuesta;
        }

        [HttpPost("api/dte/cargarXML")]
        public ContentResult cargarXML([FromBody] JsonElement values)
        {
            /*{
                   "datosCargarXML":{
                                       "base64XML":""
                   }
           }*/

            //CREAR RESPUESTA
            ContentResult respuesta = new ContentResult();

            //VALIDAR QUE VENGAN DATOS
            if (!values.TryGetProperty("datosCargarXML", out var json_content))
            {
                //SE TIENE QUE ENVIAR CON CODIGO 200 OK Y NO CON 500 PORQUE SI NO EN LA APP AUTOMATICA ENTRA AUTOMATICAMENTE EN CATCH EL CODIGO.
                string JsonResponse = @"{""statusCode"": 500,""message"": ""ERROR: se necesita key 'datosCargarXML'""}";

                respuesta.Content = JsonResponse;
                respuesta.ContentType = "application/json";
                respuesta.StatusCode = 200;

                return respuesta;
            }

            //SACAMOS LOS DATOS DESDE EL JSON
            JsonElement datosXML = values.GetProperty("datosCargarXML");
            string base64XML = datosXML.GetProperty("base64XML").ToString();

            Byte[] bytes = Convert.FromBase64String(base64XML);

            string fecha_actual = DateTime.Now.ToString("hhmmssfff");

            XmlDocument xml_file = new XmlDocument();
            string xml = Encoding.UTF8.GetString(bytes);
            xml_file.LoadXml(xml);
            xml_file.PreserveWhitespace = true;
            string pathXML_Temp = @"C:\inetpub\wwwroot\api_agrodte\AgroDTE_Archivos\Temp\xml_upload_" + fecha_actual + ".xml";
            xml_file.Save(pathXML_Temp);

            var dte = xml_file.GetElementsByTagName("DTE");


            //VALIDAR XML CON SCHEMA

            //string filename = pathXML;
            string respuestaSchema = "";
            string schemaFileName = schemaFileName = @"C:\inetpub\wwwroot\api_agrodte\AgroDTE_Archivos\Schemas\DTEs\EnvioDTE_v10.xsd";

           
            try
            {
               //string  path_servicio_validar_xml = "http://192.168.1.9:90/WebServiceValidarXML/ValidarXML.asmx/ValidarXml?xmlFilename=" + pathXML_Temp + "&schemaFilename=" + schemaFileName;

                string path_servicio_validar_xml = "http://localhost:81/WebServiceValidarXML/ValidarXML.asmx/ValidarXml?xmlFilename=" + pathXML_Temp + "&schemaFilename=" + schemaFileName;

                WebRequest request = WebRequest.Create(path_servicio_validar_xml);
                request.Method = "GET";
                WebResponse response = request.GetResponse();

                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    respuestaSchema = reader.ReadToEnd(); // do something fun...
                }

                //ESTA RESPUESTA ES UN XML PERO ES UN STRING, POR LO TANTO PARSEAMOS DE STRING A XML
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(respuestaSchema);

                XmlNodeList elemlist = xmlDoc.GetElementsByTagName("string");
                string resultadoSchema = elemlist[0].InnerXml;
                string mensajeSchema = elemlist[1].InnerXml;

                if (resultadoSchema == "False")
                {
                    string JsonResponse = @"{""statusCode"": 500,""message"": ""ERROR: HUBO UN ERROR DTE INVALIDO REVISADO POR SCHEMAS: " + mensajeSchema + @" ""}";

                    respuesta.Content = JsonResponse;
                    respuesta.ContentType = "application/json";
                    respuesta.StatusCode = 200;
                    return respuesta;
                }

                if (resultadoSchema == "True")
                {
                    //DESMENUZAR EL XML Y GUARDAR EN VARIABLES
                    string tipo_dte = "";
                    string folio = "";
                    string fchemis = "";
                    string rutrecep = "";
                    string rznsocrecep = "";
                    string cmnaorigenrecep = "";
                    string mnttotal = "";
                    string detalles = "";
                    string referencia_folio = "";
                    string referencia_tipodte = "";
                    string query = "";
                   

                    for (int j = 0; j < dte.Count; j++)
                    {
                        bool errorDTE = false;

                        XmlNode nodo_documento = dte[j].ChildNodes[0];

                        if (nodo_documento.Name == "Documento")
                        {
                            for (int o = 0; o < nodo_documento.ChildNodes.Count; o++)
                            {
                                if (nodo_documento.ChildNodes[o].Name == "Encabezado")
                                {
                                    //BUSCAR ETIQUETAS DENTRO DEL <Encabezado>
                                    var nodo_encabezado = nodo_documento.ChildNodes[o];

                                    for (int p = 0; p < nodo_encabezado.ChildNodes.Count; p++)
                                    {
                                        //BUSCAR NODOS DENTRO DE <idDoc>
                                        if (nodo_encabezado.ChildNodes[p].Name == "IdDoc")
                                        {
                                            var nodo_idDoc = nodo_encabezado.ChildNodes[p];

                                            for (int u = 0; u < nodo_idDoc.ChildNodes.Count; u++)
                                            {

                                                if (nodo_idDoc.ChildNodes[u].Name == "TipoDTE")
                                                {
                                                    tipo_dte = nodo_idDoc.ChildNodes[u].InnerText;
                                                }
                                                else if (nodo_idDoc.ChildNodes[u].Name == "Folio")
                                                {
                                                    folio = nodo_idDoc.ChildNodes[u].InnerText;
                                                }
                                                else if (nodo_idDoc.ChildNodes[u].Name == "FchEmis")
                                                {
                                                    fchemis = nodo_idDoc.ChildNodes[u].InnerText;
                                                }

                                            }

                                        }
                                        
                                        else if (nodo_encabezado.ChildNodes[p].Name == "Receptor")
                                        {
                                            var nodo_Receptor = nodo_encabezado.ChildNodes[p];
                                            for (int r = 0; r < nodo_Receptor.ChildNodes.Count; r++)
                                            {
                                                if (nodo_Receptor.ChildNodes[r].Name == "RUTRecep")
                                                {
                                                    rutrecep = nodo_Receptor.ChildNodes[r].InnerText;

                                                }

                                                else if (nodo_Receptor.ChildNodes[r].Name == "RznSocRecep")
                                                {
                                                    rznsocrecep = nodo_Receptor.ChildNodes[r].InnerText;
                                                }
                                                else if (nodo_Receptor.ChildNodes[r].Name == "CmnaRecep")
                                                {
                                                    cmnaorigenrecep = nodo_Receptor.ChildNodes[r].InnerText;
                                                }
                                            }

                                        }

                                        //BUSCAR NODOS DENTRO DE <Totales>
                                        else if (nodo_encabezado.ChildNodes[p].Name == "Totales")
                                        {
                                            var nodo_Totales = nodo_encabezado.ChildNodes[p];

                                            for (int m = 0; m < nodo_Totales.ChildNodes.Count; m++)
                                            {
                                                if (nodo_Totales.ChildNodes[m].Name == "MntTotal")
                                                {
                                                    mnttotal = nodo_Totales.ChildNodes[m].InnerText;
                                                }

                                            }
                                        }
                                    }

                                }
                                //BUSCAR ETIQUETAS DENTRO DEL <Detalle>
                                else if (nodo_documento.ChildNodes[o].Name == "Detalle")
                                {
                                    var nodo_detalle = nodo_documento.ChildNodes[o];
                                    for (int e = 0; e < nodo_detalle.ChildNodes.Count; e++)
                                    {
                                        if (nodo_detalle.ChildNodes[e].Name == "NmbItem")
                                        {
                                            detalles = detalles + nodo_detalle.ChildNodes[e].InnerText + ";";
                                        }
                                    }

                                }
                                //BUSCAR ETIQUETAS DENTRO DEL <Referencia>
                                else if (nodo_documento.ChildNodes[o].Name == "Referencia")
                                {
                                    var nodo_referencia = nodo_documento.ChildNodes[o];
                                    for (int e = 0; e < nodo_referencia.ChildNodes.Count; e++)
                                    {
                                        if (nodo_referencia.ChildNodes[e].Name == "TpoDocRef")
                                        {
                                            referencia_tipodte = nodo_referencia.ChildNodes[e].InnerText;
                                        }
                                        else if (nodo_referencia.ChildNodes[e].Name == "FolioRef")
                                        {
                                            referencia_folio = nodo_referencia.ChildNodes[e].InnerText;
                                        }
                                    }
                                }
                            }

                        }

                        //INSERTAR DTE DEPENDIENDO DEL TIPO

                        //Eliminar el ultimo ; del detalle string
                        if (detalles.Length == 0)
                        {

                        }
                        else
                        {
                            detalles = detalles.Remove(detalles.Length - 1);
                        }


                        if (j == 0)
                        {
                            pathXML_Temp = pathXML_Temp.Replace("\\", "\\\\");
                        }

                        ConexionBD conexion = new ConexionBD();

                        //INSERTAR EN ENVIO_DTE
                        string query_envio_dte = "INSERT INTO envio_dte (trackid_envio_dte, estado_envio_dte, rutaxml_envio_dte, informados_envio_dte, aceptados_envio_dte, fecha_envio_dte, revision_envio_dte, envio_cliente_envio_dte, tipo_dte_envio_dte) VALUES (0, 'Enviado', '" + pathXML_Temp + "', 1, 1, '" + fchemis + "', 1, 1, '" + tipo_dte + "')";
                        conexion.Consulta(query_envio_dte);


                        //COPIAR AL DIRECTORIO DE FACTURAS
                        //CREAR DIRECTORIO
                        string T33F = "T" + tipo_dte + "F" + folio;
                        int folio_int = int.Parse(folio);
                        int tipo_dte_int = int.Parse(tipo_dte);

                        string directorioFechaActual = "";
                        string directorioDTE = "";

                        directorioFechaActual = Clases.DTE.crearDirectorio(folio_int, tipo_dte_int, T33F);
                        directorioDTE = directorioFechaActual + @"\" + T33F + ".xml";

                        xml_file.Save(directorioDTE);


                        switch (tipo_dte)
                        {
                            case "33":
                                try
                                {

                                    string query_verificar = "SELECT folio_factura FROM factura WHERE folio_factura = '" + folio + "' AND rutemis_factura_compra = '" + rutrecep + "'";

                                    List<string> lista_verificar = conexion.Select(query_verificar);
                                    if (lista_verificar.Count == 0)
                                    {
                                        query = "INSERT INTO factura (folio_factura,fchemis_factura,rutrecep_factura," +
                                                                                                        "rznsocrecep_factura,cmnarecep_factura,mnttotal_factura,detalle_factura,folioref_factura,tipo_dteref_factura,ubicacion_factura)" +
                                                                                                        "VALUES ('" + folio + "', '" + fchemis + "', '" + rutrecep + "', '" + rznsocrecep + "', '" + cmnaorigenrecep + "', '" + mnttotal + "'," +
                                                                                                        " '" + detalles + "', '" + referencia_folio + "','" + referencia_tipodte + "', '" + directorioDTE + "')";

                                        conexion.Consulta(query);
                                        detalles = "";
                                        //respuestaDte.Add("RecepcionDTE-OK");
                                    }
                                   



                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("HUBO UN PROBLEMA AL INSERTAR XML FOLIO: " + folio + " MENSAJE: " + e.Message);

                                }



                                break;

                            /*case "34":
                                try
                                {
                                    string query_verificar = "SELECT folio_factura_exenta FROM factura_exenta WHERE folio_factura_exenta = '" + folio + "' AND rutrecep_factura_exenta_compra = '" + rutemis + "'";

                                    List<string> lista_verificar = conexion.Select(query_verificar);

                                    if (lista_verificar.Count == 0)
                                    {
                                        query = "INSERT INTO factura_exenta_compra (folio_factura_exenta_compra,uid_correo,fchemis_factura_exenta_compra,rutemis_factura_exenta_compra," +
                                   "rznsocemis_factura_exenta_compra,cmnaorigen_factura_exenta_compra,mnttotal_factura_exenta_compra,detalle_factura_exenta_compra,folioref_factura_exenta_compra,tipo_dteref_factura_exenta_compra,ubicacion_factura_exenta_compra,estado_schema_factura_exenta_compra)" +
                                   "VALUES ('" + folio + "', '" + correo_uid + "', '" + fchemis + "', '" + rutemis + "', '" + rznsocemisor + "', '" + cmnaorigenemisor + "', '" + mnttotal + "'," +
                                   " '" + detalles + "', '" + referencia_folio + "','" + referencia_tipodte + "', '" + filePath + "','" + errorSchema + "')";

                                        conexion.Consulta(query);
                                        detalles = "";
                                        respuestaDte.Add("RecepcionDTE-OK");
                                    }
                                    else
                                    {
                                        //AUMENTA EL CONTADOR
                                        contador_repetidos_dte++;
                                        if (!errorDTE)
                                        {
                                            respuestaDte.Add("RecepcionDTE-Repetido");
                                        }
                                        else
                                        {

                                        }
                                    }




                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("HUBO UN PROBLEMA AL LEER XML DE PROVEEDOR: " + correo_uid + " MENSAJE: " + e.Message);

                                }


                                break;

                            case "61":
                                try
                                {
                                    string query_verificar = "SELECT folio_nota_credito_compra FROM nota_credito_compra WHERE folio_nota_credito_compra = '" + folio + "' AND rutemis_nota_credito_compra = '" + rutemis + "'";

                                    List<string> lista_verificar = conexion.Select(query_verificar);
                                    if (lista_verificar.Count == 0)
                                    {



                                        query = "INSERT INTO nota_credito_compra (folio_nota_credito_compra,uid_correo,fchemis_nota_credito_compra,rutemis_nota_credito_compra," +
                                        "rznsocemis_nota_credito_compra,cmnaorigen_nota_credito_compra,mnttotal_nota_credito_compra,detalle_nota_credito_compra,folioref_nota_credito_compra,tipo_dteref_nota_credito_compra,ubicacion_nota_credito_compra,estado_schema_nota_credito_compra)" +
                                        "VALUES ('" + folio + "', '" + correo_uid + "', '" + fchemis + "', '" + rutemis + "', '" + rznsocemisor + "', '" + cmnaorigenemisor + "', '" + mnttotal + "'," +
                                        " '" + detalles + "', '" + referencia_folio + "','" + referencia_tipodte + "', '" + filePath + "','" + errorSchema + "')";

                                        conexion.Consulta(query);
                                        detalles = "";
                                        respuestaDte.Add("RecepcionDTE-OK");
                                    }
                                    else
                                    {
                                        //AUMENTA EL CONTADOR
                                        contador_repetidos_dte++;
                                        if (!errorDTE)
                                        {
                                            respuestaDte.Add("RecepcionDTE-Repetido");
                                        }
                                        else
                                        {

                                        }
                                    }

                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("HUBO UN PROBLEMA AL LEER XML DE PROVEEDOR: " + correo_uid + " MENSAJE: " + e.Message);

                                }


                                break;

                            case "56":
                                try
                                {
                                    string query_verificar = "SELECT folio_nota_debito_compra FROM nota_debito_compra WHERE folio_nota_debito_compra = '" + folio + "' AND rutemis_nota_debito_compra = '" + rutemis + "'";

                                    List<string> lista_verificar = conexion.Select(query_verificar);

                                    if (lista_verificar.Count == 0)
                                    {
                                        query = "INSERT INTO nota_debito_compra (folio_nota_debito_compra,uid_correo,fchemis_nota_debito_compra,rutemis_nota_debito_compra," +
                                                                                                       "rznsocemis_nota_debito_compra,cmnaorigen_nota_debito_compra,mnttotal_nota_debito_compra,detalle_nota_debito_compra,folioref_nota_debito_compra,tipo_dteref_nota_debito_compra,ubicacion_nota_debito_compra,estado_schema_nota_debito_compra)" +
                                                                                                       "VALUES ('" + folio + "', '" + correo_uid + "', '" + fchemis + "', '" + rutemis + "', '" + rznsocemisor + "', '" + cmnaorigenemisor + "', '" + mnttotal + "'," +
                                                                                                       " '" + detalles + "', '" + referencia_folio + "','" + referencia_tipodte + "', '" + filePath + "','" + errorSchema + "')";

                                        conexion.Consulta(query);

                                        detalles = "";
                                        respuestaDte.Add("RecepcionDTE-OK");
                                    }
                                    else
                                    {
                                        //AUMENTA EL CONTADOR
                                        contador_repetidos_dte++;
                                        if (!errorDTE)
                                        {
                                            respuestaDte.Add("RecepcionDTE-Repetido");
                                        }
                                        else
                                        {

                                        }

                                    }



                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("HUBO UN PROBLEMA AL LEER XML DE PROVEEDOR: " + correo_uid + " MENSAJE: " + e.Message);

                                }


                                break;

                            case "52":
                                try
                                {
                                    string query_verificar = "SELECT folio_guia_despacho_compra FROM guia_despacho_compra WHERE folio_guia_despacho_compra = '" + folio + "' AND rutemis_guia_despacho_compra = '" + rutemis + "'";

                                    List<string> lista_verificar = conexion.Select(query_verificar);
                                    if (lista_verificar.Count == 0)
                                    {

                                        query = "INSERT INTO guia_despacho_compra (folio_guia_despacho_compra,uid_correo,fchemis_guia_despacho_compra,rutemis_guia_despacho_compra," +
                                        "rznsocemis_guia_despacho_compra,cmnaorigen_guia_despacho_compra,mnttotal_guia_despacho_compra,detalle_guia_despacho_compra,folioref_guia_despacho_compra,tipo_dteref_guia_despacho_compra,ubicacion_guia_despacho_compra,estado_schema_guia_despacho_compra)" +
                                        "VALUES ('" + folio + "', '" + correo_uid + "', '" + fchemis + "', '" + rutemis + "', '" + rznsocemisor + "', '" + cmnaorigenemisor + "', '" + mnttotal + "'," +
                                        " '" + detalles + "', '" + referencia_folio + "', '" + referencia_tipodte + "','" + filePath + "','" + errorSchema + "')";

                                        conexion.Consulta(query);
                                        detalles = "";
                                        respuestaDte.Add("RecepcionDTE-OK");
                                    }
                                    else
                                    {
                                        //AUMENTA EL CONTADOR
                                        contador_repetidos_dte++;
                                        if (!errorDTE)
                                        {
                                            respuestaDte.Add("RecepcionDTE-Repetido");
                                        }
                                        else
                                        {

                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("HUBO UN PROBLEMA AL LEER XML DE PROVEEDOR: " + correo_uid + " MENSAJE: " + e.Message);

                                }
                                break;
                            default:
                                break;*/
                        }

                    }
                }


            }
            catch (Exception e)
            {
                string JsonResponse = @"{""statusCode"": 500,""message"": ""HUBO UN ERROR : "+e.Message+@" ""}";

                respuesta.Content = JsonResponse;
                respuesta.ContentType = "application/json";
                respuesta.StatusCode = 200;
                return respuesta;

            }

            






                return respuesta;
        }



            [HttpPost("api/dte/EmitirAcuseRecibo")]
        public ContentResult crearAcuseRecibo([FromBody] JsonElement values)
        {
            /*{
                    "datosAcuseRecibo":{
                                        "PathXML":"",
                                        "RespuestaSobre": "",
                                        "respuestaDte": "",
                                        "RutEmpresaResponde": "",
                                        "prefixPathXML": ""
                                        
                    }
            }*/

            //GENERAMOS VARIABLE PARA RETORNAR RESPUESTA
            ContentResult respuesta = new ContentResult();

            //VALIDAR QUE VENGAN DATOS
            if (!values.TryGetProperty("datosAcuseRecibo", out var json_content))
            {
                //SE TIENE QUE ENVIAR CON CODIGO 200 OK Y NO CON 500 PORQUE SI NO EN LA APP AUTOMATICA ENTRA AUTOMATICAMENTE EN CATCH EL CODIGO.
                string JsonResponse = @"{""statusCode"": 500,""message"": ""ERROR: se necesita key 'datosAcuseRecibo'""}";

                respuesta.Content = JsonResponse;
                respuesta.ContentType = "application/json";
                respuesta.StatusCode = 200;

                return respuesta;
            }

            JsonElement datosAcuse = values.GetProperty("datosAcuseRecibo");
            string pathXML = datosAcuse.GetProperty("PathXML").ToString();
            string RespuestaSobre = datosAcuse.GetProperty("RespuestaSobre").ToString();
            string rutempresaResponde = datosAcuse.GetProperty("rutEmpresaResponde").ToString();

            string[] arrayRespuestaDTE = new string[] { };
            if (datosAcuse.TryGetProperty("respuestaDte", out JsonElement respuestaDTE))
            {
                JArray respuestaDTEArray = JArray.Parse(respuestaDTE.ToString());
                arrayRespuestaDTE = respuestaDTEArray.ToObject<string[]>();
            }
            string prefixPathXML = datosAcuse.GetProperty("prefixPathXML").ToString();

            string respuesta_directorio = verificarDirectorio(@"C:\inetpub\wwwroot\api_agrodte\AgroDTE_Archivos\AcuseRecibo");


            if (respuesta_directorio == "Creado Directorio" || respuesta_directorio == "Directorio Existe")
            {
                //DIRECTORIO EXISTE

                //INVOCAR METODO ACUSE RECIBO
                AcuseRecibo acuse = new AcuseRecibo();
               string respuesta_crearAcuseRecibo = acuse.crearAcuseRecibo(RespuestaSobre, arrayRespuestaDTE, pathXML,rutempresaResponde, prefixPathXML);

                if (respuesta_crearAcuseRecibo == "ok")
                {
                    string JsonResponse = @"{""statusCode"": 200,""message"": ""Acuse recibo " + RespuestaSobre + @" Creada con exito "" }";
                    respuesta.Content = JsonResponse;
                }
                else
                {
                    string JsonResponse = @"{""statusCode"": 200,""message"": ""ERROR: "+respuesta_crearAcuseRecibo +   @""" }";
                    respuesta.Content = JsonResponse;
                }

              
            }
            else
            {
                string JsonResponse = @"{""statusCode"": 500,""message"": ""ERROR: no se pudo crear el directorio AgroDTE_Archivos""}";

                respuesta.Content = JsonResponse;
                respuesta.ContentType = "application/json";
                respuesta.StatusCode = 500;

                return respuesta;
            }



           
            return respuesta;

        }

        [HttpPost("api/dte/document/crearDTEfront")]
        public ContentResult crearDTEfront([FromBody] JsonElement values)
        {
            ContentResult respuesta = new ContentResult();
            string JsonResponse = "";

           //4 JsonResponse = @"{""statusCode"": 200,""pdf_base64"": """ + pdf_base64 + @""", ""tipo_dte"": """ + tipo_dte_str + @"""}";
            respuesta.Content = JsonResponse;
            respuesta.ContentType = "application/json";
            respuesta.StatusCode = 200;

            return respuesta;

        }


            [HttpPost("api/dte/document/crearPDF")]
        public ContentResult crearPdfDTE([FromBody] JsonElement values)
        {
            ContentResult respuesta = new ContentResult();
            string JsonResponse = "";

            var result = JsonConvert.DeserializeObject<JObject>(values.ToString());
            var tipo_dte = result["tipo_dte"];
            var path = result["path"];

            //MODIFICACION 24-02-2023 ESTAS VARIABLES SE CREARON PARA EL CASO DE DTE COMPRA
            var folio = result["folio"];
            var compra_dte = result["compra_dte"];


            if (path == null || tipo_dte == null)
            {
                JsonResponse = @"{""statusCode"": 500,""message"": ""ERROR: faltan keys en json""}";

                respuesta.Content = JsonResponse;
                respuesta.ContentType = "application/json";
                respuesta.StatusCode = 500;

                return respuesta;
            }

            string path_archivo = path.ToString();
            string tipo_dte_str = tipo_dte.ToString();

            if (tipo_dte_str == "39" || tipo_dte_str == "41")
            {
                PDFBoleta pdf_boleta = new PDFBoleta();
                string pdf_boleta_base64 = pdf_boleta.CrearBoleta(path_archivo);
                JsonResponse = @"{""statusCode"": 200,""pdf_base64"": """+pdf_boleta_base64+ @""", ""tipo_dte"": """ + tipo_dte_str + @"""}";
                respuesta.Content = JsonResponse;
                respuesta.ContentType = "application/json";
                respuesta.StatusCode = 200;
            }
            else
            {
                PDF pdf = new PDF();
                string pdf_base64 = "";

                //ESTO SE MODIFICO 24-02-2023 PARA PODER VISUALIZAR DTE DE COMPRA CON VARIOS DTE EN EL MISMO XML
                if (compra_dte == null )
                {
                    pdf_base64 = pdf.CrearPDF(path_archivo);
                    JsonResponse = @"{""statusCode"": 200,""pdf_base64"": """ + pdf_base64 + @""", ""tipo_dte"": """ + tipo_dte_str + @"""}";
                    respuesta.Content = JsonResponse;
                    respuesta.ContentType = "application/json";
                    respuesta.StatusCode = 200;
                }
                else
                {
                    pdf_base64 = pdf.CrearPDFCompra(path_archivo,folio.ToString());
                    JsonResponse = @"{""statusCode"": 200,""pdf_base64"": """ + pdf_base64 + @""", ""tipo_dte"": """ + tipo_dte_str + @"""}";
                    respuesta.Content = JsonResponse;
                    respuesta.ContentType = "application/json";
                    respuesta.StatusCode = 200;
                }
                    
            }



            return respuesta;
        }
        [HttpPost("api/dte/document/envioboleta")]
        public ContentResult pruebaPost([FromBody] JsonElement values)
        {
            ContentResult respuesta = new ContentResult();
            string JsonResponse = "";

            var result = JsonConvert.DeserializeObject<JObject>(values.ToString());
            var path = result["path"];
            var rut_emisor = result["rut_emisor"];
            var rut_empresa = result["rut_empresa"];

            if (path == null || rut_emisor == null || rut_empresa == null)
            {
                JsonResponse = @"{""statusCode"": 500,""message"": ""ERROR: faltan keys en json""}";

                respuesta.Content = JsonResponse;
                respuesta.ContentType = "application/json";
                respuesta.StatusCode = 500;

                return respuesta;
            }

            EnvioDTE envio = new EnvioDTE();
            string respuestaEnvio = "";
            string TrackId_str = "";
            string path_archivo = path.ToString();
            string rutEmisor = rut_emisor.ToString();
            string rutEmpresa = rut_empresa.ToString();

            try
            {
                respuestaEnvio = envio.enviarSobreBoleta(path_archivo, rutEmisor, rutEmpresa);
                TrackId_str = respuestaEnvio;

                //SE CONTROLA ERROR DE ENVIO BOLETAS HACE UN INTENTO DE 2 VECES
                if (TrackId_str == "TRKID ERROR")
                {
                    for (int i = 0; i < 2; i++)
                    {
                        respuestaEnvio = envio.enviarSobreBoleta(path_archivo, rutEmisor, rutEmpresa);
                        TrackId_str = respuestaEnvio;

                        if (TrackId_str != "TRKID ERROR")
                        {
                            break;
                        }

                    }

                }

                if (ulong.TryParse(TrackId_str, out ulong numeroEnvio))
                {
                    ConexionBD conexion = new ConexionBD();
                    //GUARDAR ESTADO DE ENVIO CON TRACKID
                    string queryUpdateTrackid = "UPDATE envio_dte SET trackid_envio_dte = '" + TrackId_str + "', estado_envio_dte = 'Enviado' WHERE rutaxml_envio_dte = '" + path_archivo + "'";
                    queryUpdateTrackid = queryUpdateTrackid.Replace("\\", "\\\\");
                    conexion.Consulta(queryUpdateTrackid);

                    JsonResponse = @"{ ""respuesta"" : ""ok"", ""track_id"": """+TrackId_str+@"""}";
                    respuesta.Content = JsonResponse;
                    respuesta.ContentType = "application/json";
                    respuesta.StatusCode = 200;
                }
                else
                {
                    JsonResponse = @"{ ""Error"" : ""error: """ + respuestaEnvio + "}";
                    respuesta.Content = JsonResponse;
                    respuesta.ContentType = "application/json";
                    respuesta.StatusCode = 400;
                }
            }
            catch (Exception ex)
            {
                JsonResponse = @"{ ""Error"" : ""error: """ + ex.Message + "}";
                respuesta.Content = JsonResponse;
                respuesta.ContentType = "application/json";
                respuesta.StatusCode = 400;

            }
           



                return respuesta;
           
           
        }
        [HttpPost("api/dte/document/enviocliente")]
        public ContentResult enviarSobreCliente([FromBody] JsonElement values)
        {
            /*
             {
                "rut": "76958430-7",
                "path": "C:\\inetpub\\wwwroot\\api_agrodte\\AgroDTE_Archivos\\XML\\2022\\M5\\D9\\SobreEnvio20220509121329303-cliente.xml"
            } */
            ContentResult respuesta = new ContentResult();
            string JsonResponse = "";
            //VALIDAMOS QUE VENGA EN EL JSON 'path'
            var result = JsonConvert.DeserializeObject<JObject>(values.ToString());
            var path = result["path"];
            var rut = result["rut"];

            if (path == null)
            {
                JsonResponse = @"{""statusCode"": 500,""message"": ""ERROR: se necesita key 'path'""}";

                respuesta.Content = JsonResponse;
                respuesta.ContentType = "application/json";
                respuesta.StatusCode = 500;

                return respuesta;
            }

            if (rut == null)
            {
                JsonResponse = @"{""statusCode"": 500,""message"": ""ERROR: se necesita key 'rut'""}";

                respuesta.Content = JsonResponse;
                respuesta.ContentType = "application/json";
                respuesta.StatusCode = 500;

                return respuesta;
            }



            string path_archivo = path.ToString();
            string rut_recibe = rut.ToString();

            //SI TODO SALE OK, ENVIAMOS POR CORREO AL CLIENTE EL XML DEL DTE ENVIADO AL SII PERO CON SOBRE DIRIGIDO A CLIENTE
            AcuseRecibo acuseRecibo = new AcuseRecibo();
            string respuestaAcuseRecibo = acuseRecibo.enviarEmailAcuseRecibo(path_archivo, rut_recibe, "XML Cliente");

            if (respuestaAcuseRecibo == "ok")
            {
               JsonResponse = @"{ ""respuesta"" : ""ok""}";
            }
            else { 
                JsonResponse = @"{ ""respuesta"" : ""error: """+ respuestaAcuseRecibo + "}"; 
            }
           
            respuesta.Content = JsonResponse;
            respuesta.ContentType = "application/json";
            respuesta.StatusCode = 200;

            return respuesta;
        }

        [HttpGet("api/dte/document/{token}/{value}")]
        public ContentResult verificarDTE(string token,string value, [FromHeader(Name = "apikey")] string valuesHeader)
        {
            ContentResult respuesta = new ContentResult();
            //VALIDAR API KEY
            if (valuesHeader == "928e15a2d14d4a6292345f04960f4cc3")
            {
                ConexionBD conexion = new ConexionBD();
               
                string JsonResponse = "";

                string archivostr = token.ToString();
                                using (StreamWriter writetext = new StreamWriter(@"C:\inetpub\wwwroot\api_agrodte\request_comprobacion_dte.txt"))
                                {
                                    writetext.WriteLine(archivostr);
                                } 

                switch (value)
                {
                    case "status":
                        string consulta_status = "SELECT aceptados_envio_dte,rechazos_envio_dte,reparos_envio_dte,revision_envio_dte FROM envio_dte WHERE id_envio_dte = '" + token + "'";
                        List<string> dte_status = conexion.Select(consulta_status);

                        if (dte_status.Count != 0)
                        {
                            if (dte_status[0] != "0")
                            {
                                JsonResponse = @"{""estado"": ""Aceptado"",""token"": """ + token + @"""}";
                            }

                            if (dte_status[1] != "0")
                            {
                                JsonResponse = @"{""estado"": ""Rechazado"",""token"": """ + token + @"""}";
                            }

                            if (dte_status[2] != "0")
                            {
                                JsonResponse = @"{""estado"": ""Aceptado con Reparo"",""token"": """ + token + @"""}";
                            }

                            if (dte_status[3] == "0")
                            {
                                JsonResponse = @"{""estado"": ""Pendiente"",""token"": """ + token + @"""}";
                            }
                        }
                        else
                        {
                            JsonResponse = @"{""error"": { ""message"": ""Validación de Campos"",""code"": ""OF-10"",""details"": [{""field"": ""Token"",""issue"": ""Token Incorrecto""}]}}";
                        }



                        respuesta.Content = JsonResponse;
                        respuesta.ContentType = "application/json";
                        respuesta.StatusCode = 200;

                        return respuesta;



                        break;
                    default:
                        JsonResponse = @"{""error"": { ""message"": ""Validación de Campos"",""code"": ""OF-10"",""details"": [{""field"": ""Error"",""issue"": """+ value + @" es Incorrecto""}]}}";
                        respuesta.Content = JsonResponse;
                        respuesta.ContentType = "application/json";
                        respuesta.StatusCode = 400;

                        return respuesta;
                        break;
                }

                //GENERAMOS VARIABLE PARA RETORNAR RESPUESTA

                respuesta.Content = JsonResponse;
                respuesta.ContentType = "application/json";
                respuesta.StatusCode = 200;

                return respuesta;
            }
            else
            {
                string JsonResponse = @"{""statusCode"": 401,""message"": ""Access denied due to invalid subscription key. Make sure to provide a valid key for an active subscription.""}";

                respuesta.Content = JsonResponse;
                respuesta.ContentType = "application/json";
                respuesta.StatusCode = 401;

                return respuesta;
            }

        }

        // public static bool getEstadoRequest()
        //{

        //    //CREAR EL ARCHIVO TXT DE COLA SI NO EXISTE
        //    string fileColaRequest = Environment.CurrentDirectory + "\\colaRequest.txt"; //NOMBRE DEL ARCHIVO TXT

        //    var respuestaCola = System.IO.File.Exists(fileColaRequest);

        //    if (respuestaCola)
        //    {
        //        //EXISTE ARCHIVO DE COLA REQUEST
        //        using (var reader = new StreamReader(fileColaRequest))
        //        {
        //            string line;
        //            while ((line = reader.ReadLine()) != null)
        //            {
        //                if (line == "Procesando Request")
        //                {
        //                    //Console.WriteLine("Se esta procesando una request...");
        //                    reader.Close();
        //                    Thread.Sleep(5000); //ESTO SIMULA EL TIEMPO QUE ESPERA LA PETICION DE UNA REQUEST QUE ESTA EN COLA

        //                    return false;
        //                }

        //                if (line == "Sin Request")
        //                {

        //                    reader.Close();
        //                    //ESTO SIMULA EL CAMBIO DE ESTADO DE UNA REQUEST
        //                    using (StreamWriter writetext = new StreamWriter(fileColaRequest))
        //                    {
        //                        writetext.WriteLine("Procesando Request");
        //                        writetext.Close();
        //                    }




        //                    Thread.Sleep(8000); //ESTO SIMULA LO QUE SE DEMORA UNA FACTURA O BOLETA
        //                    //Console.WriteLine("No hay request procesando...");
        //                    reader.Close();



        //                    //ESTO SIMULA EL CAMBIO DE ESTADO DE UNA REQUEST
        //                    using (StreamWriter writetext = new StreamWriter(fileColaRequest))
        //                    {
        //                        writetext.WriteLine("Sin Request");
        //                        writetext.Close();
        //                    }


        //                    return true;
        //                }


        //            }
        //        }
        //        Console.WriteLine("Existe archivo");
        //    }
        //    else
        //    {
        //        //NO EXISTE ARCHIVO COLA REQUEST, SE CREA...
        //        using (StreamWriter writetext = new StreamWriter(fileColaRequest))
        //        {
        //            writetext.WriteLine("Procesando Request");
        //            writetext.Close();
        //        }

        //        Thread.Sleep(8000); //ESTO SIMULA LO QUE SE DEMORA UNA FACTURA O BOLETA
        //                            //Console.WriteLine("No hay request procesando...");



        //        //ESTO SIMULA EL CAMBIO DE ESTADO DE UNA REQUEST
        //        using (StreamWriter writetext = new StreamWriter(fileColaRequest))
        //        {
        //            writetext.WriteLine("Sin Request");
        //            writetext.Close();
        //        }

        //        //Console.WriteLine("No existe archivo creando...");
        //        return false;
        //    }

        //    return true;
        //}

        [HttpPost("api/dte/document")]
        public ContentResult crearDTE([FromBody] JsonElement values, [FromHeader(Name = "apikey")] string valuesHeader)
        {
            ContentResult respuesta = new ContentResult();

            // COLA PROVISORIA ARTESA Y QUE PAZAAAAAA ----------------------------------------------------------------
            for (bool r = false; r == false;)
            {
                //CREAR EL ARCHIVO TXT DE COLA SI NO EXISTE
                string fileColaRequest = Environment.CurrentDirectory + "\\colaRequest.txt"; //NOMBRE DEL ARCHIVO TXT

                var respuestaCola = System.IO.File.Exists(fileColaRequest);

                if (respuestaCola)
                {
                    using (var reader = new StreamReader(fileColaRequest))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (line == "Procesando Request")
                            {
                                //Console.WriteLine("Se esta procesando una request...");
                                reader.Close();
                                Thread.Sleep(1000); //ESTO SIMULA EL TIEMPO QUE ESPERA LA PETICION DE UNA REQUEST QUE ESTA EN COLA

                                r = false;
                                break;
                            }
                            if (line == "Sin Request")
                            {
                                reader.Close();

                                using (StreamWriter writetext = new StreamWriter(fileColaRequest))
                                {
                                    writetext.WriteLine("Procesando Request");
                                    writetext.Close();
                                }

                                //GUARDA LA ULTIMA REQUEST EMITIDA
                                string archivostr = values.ToString();
                               /* using (StreamWriter writetext = new StreamWriter(@"C:\inetpub\wwwroot\api_agrodte\request.txt"))
                                {
                                    writetext.WriteLine(archivostr);
                                }*/

                                //GUARDA LAS REQUEST
                                /* string hora_actual = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff");
                                 using (StreamWriter writetext = new StreamWriter(@"C:\inetpub\wwwroot\api_agrodte\AgroDTE_Archivos\Log\Request_" + hora_actual + ".txt"))
                                 {
                                     writetext.WriteLine(archivostr);
                                 }*/


                                ConexionBD conexion = new ConexionBD();
                               



                                //VALIDAMOS QUE VENGA EN EL JSON 'dte'
                                if (!values.TryGetProperty("dte", out var dte_content))
                                {
                                    string JsonResponse = @"{""statusCode"": 500,""message"": ""ERROR: se necesita key 'dte'""}";

                                    respuesta.Content = JsonResponse;
                                    respuesta.ContentType = "application/json";
                                    respuesta.StatusCode = 500;

                                    using (StreamWriter writetext = new StreamWriter(fileColaRequest))
                                    {
                                        writetext.WriteLine("Sin Request");
                                        writetext.Close();
                                    }

                                    //GUARDA LAS RESPONSE ERROR SIN KEY DTE
                                   /* string hora_actual_response_error_sin_key_dte = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff");
                                    using (StreamWriter writetext = new StreamWriter(@"C:\inetpub\wwwroot\api_agrodte\AgroDTE_Archivos\Log\Response_Error_SinKey_DTE_" + hora_actual_response_error_sin_key_dte + ".txt"))
                                    {
                                        writetext.WriteLine(respuesta.Content);
                                    }*/

                                    string log_Response = "INSERT INTO log_event (mensaje_log_event, fecha_log_event, referencia_log_event,query_request_log_event) VALUES ('Response Sin Key',NOW(),'- ','" + respuesta.Content + "')";
                                    conexion.Consulta(log_Response);

                                    return respuesta;
                                }


                                //GUARDAR LA REQUEST ENVIADA
                               
                                DTE dte = new DTE();
                                string cuerpoDte = values.GetProperty("dte").ToString();
                                string mensajeLogEvent = "JsonElement recibido del ERP para la creacion de DTE";
                                string direccionEnvio = "IP";
                                string consultaDte = "INSERT INTO log_event (mensaje_log_event, fecha_log_event, referencia_log_event,query_request_log_event) VALUES ('" + mensajeLogEvent + "',NOW(),'- ','" + archivostr + "')";
                                conexion.Consulta(consultaDte);

                                //TRAER ESTOS CAMPOS DE LA BASE DE DATOS
                                List<string> datosEmpresa = conexion.Select("SELECT rut_empresa, rut_rrll, fecha_res, numero_res FROM empresa WHERE id_empresa = '1'");
                                string rutEmisor = datosEmpresa[1];
                                string rutEmpresa = datosEmpresa[0];
                                string fechaResolucion = datosEmpresa[2];
                                fechaResolucion = fechaResolucion.Remove(10);
                                string numeroResolucion = datosEmpresa[3];


                                bool responseOp = false;// Respuesta Opcional
                                string[] arrayPedido = new string[] { };
                                if (values.TryGetProperty("response", out JsonElement pedidoRespuesta))
                                {
                                    JArray pedidoRespuestaArray = JArray.Parse(pedidoRespuesta.ToString());
                                    arrayPedido = pedidoRespuestaArray.ToObject<string[]>();
                                }







                                //VALIDAR API KEY
                                if (valuesHeader == "928e15a2d14d4a6292345f04960f4cc3")
                                {
                                    //VERIFICAR O CREAR DIRECTORIO PARA ARCHIVOS
                                    //VERIFICAR SI EXISTE EL DIRECTORIO EN C:\AgroDTE_Archivos\

                                    string respuesta_directorio = verificarDirectorio(directorio_archivos);

                                    if (respuesta_directorio == "Creado Directorio" || respuesta_directorio == "Directorio Existe")
                                    {
                                        //DIRECTORIO EXISTE
                                    }
                                    else
                                    {
                                        string JsonResponse = @"{""statusCode"": 500,""message"": ""ERROR: no se pudo crear el directorio AgroDTE_Archivos""}";

                                        respuesta.Content = JsonResponse;
                                        respuesta.ContentType = "application/json";
                                        respuesta.StatusCode = 500;

                                        using (StreamWriter writetext = new StreamWriter(fileColaRequest))
                                        {
                                            writetext.WriteLine("Sin Request");
                                            writetext.Close();
                                        }

                                        //GUARDA LAS RESPONSE ERROR CREAR DIRECTORIO DE ARCHIVOS
                                        /*string hora_actual_response_error_directorio = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff");
                                        using (StreamWriter writetext = new StreamWriter(@"C:\inetpub\wwwroot\api_agrodte\AgroDTE_Archivos\Log\Response_Error_Directorio_AgroDTE_" + hora_actual_response_error_directorio + ".txt"))
                                        {
                                            writetext.WriteLine(respuesta.Content);
                                        }*/


                                        string log_Response = "INSERT INTO log_event (mensaje_log_event, fecha_log_event, referencia_log_event,query_request_log_event) VALUES ('Response Error Directorio AgroDTE',NOW(),'- ','" + respuesta.Content + "')";
                                        conexion.Consulta(log_Response);

                                        return respuesta;
                                    }

                                    //CREAR DTE HASTA VALIDARLO CON SCHEMA
                                    DTE emitir = new DTE();
                                    string[] respuestaCrearDTE = emitir.crearDTE(values.GetProperty("dte"));
                                    /*respuestaCrearDTE[0] = Respuesta Schema
                                      respuestaCrearDTE[1] = Directorio Sobre
                                      respuestaCrearDTE[2] = Folio
                                      respuestaCrearDTE[3] = Tipo DTE
                                      respuestaCrearDTE[4] = Respuesta Schema cliente - en boleta es valido
                                      respuestaCrearDTE[5] = Directorio Sobre cliente - en boleta es ""
                                      respuestaCrearDTE[6] = Rut cliente - en boleta es ""*/

                                    //EN CASO DE QUE VENGAN NULL, RELLENAMOS CON ESPACIOS VACIOS PARA EVITAR CAIDAS

                                    for (int i = 0; i < 7; i++)
                                    {
                                        if (respuestaCrearDTE[i] == null)
                                        {
                                            respuestaCrearDTE[i] = "";
                                        }
                                    }

                                    //DEPENDIENDO LA RESPUESTA DEL SCHEMA EJECUTA IF
                                    if (respuestaCrearDTE[0] == "XML Valido" && respuestaCrearDTE[4] == "XML Valido")
                                    {

                                        //TRAEMOS EL ESTADO DESDE LA BASE DE DATOS PARA QUE LA API SEPA SI TIENE QUE PREGUNTAR POR CONEXION A INTERNET O NO
                                        List<string> datosConexion = conexion.Select("SELECT estado_conexion_empresa FROM empresa WHERE id_empresa = 1");

                                        string respuestaPing = "";

                                        if (datosConexion[0] == "Sin Conexion")
                                        {
                                            //SETEADO EN LA BD QUE NO HAY CONEXION, PASA DIRECTO A REALIZAR EL DTE SIN ENVIAR
                                            respuestaPing = "Sin Conexion";
                                        }
                                        else if(datosConexion[0] == "Con Conexion")
                                        {

                                            //CHEQUEAR SI HAY CONEXION A INTERNET 
                                            respuestaPing = checkPing("palena.sii.cl"); //PRODUCCION
                                            //respuestaPing = checkPing("maullin.sii.cl"); //CERTIFICACION
                                            // ----DESCONTINUADO ----string respuestaConexion = checkConnection("https://palena.sii.cl/DTEWS/CrSeed.jws"); //PRODUCCION
                                            //-----DESCONTINUADO-----string respuestaConexion = checkConnection("https://maullin.sii.cl/DTEWS/CrSeed.jws"); //CERTIFICACION
                                        }


                                        //ESTA VARIABLE ES EN CASO QUE HAYA REITENTADO MUCHAS VECES ENVIAR EL SOBRE, TIENE UN GOTO MAS ABAJO EL CUAL VA MAS ABAJO
                                        bool responseErrorSII = false;

                                        if (respuestaPing.Contains("Error"))
                                        {
                                            for (int i = 0; i < 3; i++)
                                            {
                                                respuestaPing = checkPing("palena.sii.cl");

                                                if (respuestaPing == "Conexion Exitosa")
                                                {
                                                    break;
                                                }
                                                Thread.Sleep(1000);
                                            }
                                           
                                        }

                                        salto2:
                                        if (responseErrorSII)
                                        {
                                            respuestaPing = "Hubo Error en ping";
                                        }
                                        
                                        //if (respuestaPing == respuestaConexion)
                                        if (respuestaPing == "Conexion Exitosa")
                                        {
                                            //EXISTE CONEXION, SOLICITAR SEMILLA, TOKEN Y ENVIAR SOBRE DTE
                                            EnvioDTE envio = new EnvioDTE();
                                            string respuestaEnvio = "";
                                            string TrackId_str = "";
                                            if (respuestaCrearDTE[3] == "39" || respuestaCrearDTE[3] == "41")
                                            {
                                                respuestaEnvio = envio.enviarSobreBoleta(respuestaCrearDTE[1], rutEmisor, rutEmpresa);
                                                TrackId_str = respuestaEnvio;

                                                

                                                if (TrackId_str == "TRKID ERROR")
                                                {
                                                    for (int i = 0; i < 2; i++)
                                                    {
                                                        respuestaEnvio = envio.enviarSobreBoleta(respuestaCrearDTE[1], rutEmisor, rutEmpresa);
                                                        TrackId_str = respuestaEnvio;

                                                        if (TrackId_str != "TRKID ERROR")
                                                        {
                                                            break;
                                                        }
                                                        
                                                    }

                                                }

                                                //SI SIGUE CON ERROR TRKID ERROR USAR EL gotto: para saltar al if sin conexion

                                                goto salto;

                                            }
                                            else
                                            {
                                               
                                                respuestaEnvio = envio.enviarSobre(respuestaCrearDTE[1], rutEmisor, rutEmpresa);
                                                //string respuestaEnvio es el TRACKID EN XML
                                                XmlDocument xmlDoc2 = new XmlDocument();
                                                xmlDoc2.LoadXml(respuestaEnvio);
                                                XmlNodeList TrackId = xmlDoc2.GetElementsByTagName("string");
                                                TrackId_str = TrackId[0].InnerXml;

                                                //SI EL SII ENVIA UN TRACK ID 0 EJECUTAMOS OTRA VEZ EL ENVIAR SOBRE

                                                if (TrackId_str == "0" || TrackId_str == "HEFESTO.DTE.AUTENTICACION.ENT.Respuesta")
                                                {
                                                    for (int i = 0; i < 2; i++)
                                                    {
                                                        respuestaEnvio = envio.enviarSobre(respuestaCrearDTE[1], rutEmisor, rutEmpresa);
                                                        XmlDocument xmlDoc3 = new XmlDocument();
                                                        xmlDoc2.LoadXml(respuestaEnvio);
                                                        XmlNodeList TrackId2 = xmlDoc2.GetElementsByTagName("string");
                                                        TrackId_str = TrackId2[0].InnerXml;
                                                        if (TrackId_str != "0" || TrackId_str != "HEFESTO.DTE.AUTENTICACION.ENT.Respuesta")
                                                        {
                                                            break;
                                                        }
                                                    }

                                                }

                                                //SI SIGUE CON ERROR HEFESTO.DTE.AUTENTICACION.ENT.Respuesta ERROR USAR EL gotto: para saltar al if sin conexion

                                                goto salto;
                                            }


                                        salto:
                                            if (TrackId_str == "TRKID ERROR" || TrackId_str == "0" || TrackId_str == "HEFESTO.DTE.AUTENTICACION.ENT.Respuesta")
                                            {
                                                responseErrorSII = true;
                                                goto salto2;
                                            }



                                            //SI ESTA CORRECTO respuestaEnvio DEVUELVE EL TRACKID, SI NO EL XML COMPLETO EN STRING DE UN ERROR

                                            if (ulong.TryParse(TrackId_str, out ulong numeroEnvio))
                                            {

                                                //SI respuestaEnvio TRAE UN NUMERO (TRACKID) SE PROCEDE A GUARDAR EN LA BD
                                                //GUARDAR ESTADO DE ENVIO CON TRACKID
                                                string queryUpdateTrackid = "UPDATE envio_dte SET trackid_envio_dte = '" + TrackId_str + "', estado_envio_dte = 'Enviado' WHERE rutaxml_envio_dte = '" + respuestaCrearDTE[1] + "'";
                                                queryUpdateTrackid = queryUpdateTrackid.Replace("\\", "\\\\");
                                                conexion.Consulta(queryUpdateTrackid);

                                                //TRAEMOS LA ID DEL SOBRE EN LA BD
                                                string queryIdSobre = "SELECT id_envio_dte FROM envio_dte WHERE rutaxml_envio_dte = '" + respuestaCrearDTE[1] + "'";
                                                queryIdSobre = queryIdSobre.Replace("\\", "\\\\");
                                                List<string> listIdSobre = conexion.Select(queryIdSobre);
                                                string id_sobre = listIdSobre[0];

                                                //GENERACION DE RESPUESTAS

                                                //VARIABLE "TOKEN" SIEMPRE SE DEVUELVE, REPRESENTA EL ID DE LA BD ENVIO SOBRE
                                                string respuestaExito = @"{""TOKEN"":""" + id_sobre + @"""";


                                                if (arrayPedido.Contains("PDF"))
                                                {
                                                    //GENERAR PDF
                                                    if (respuestaCrearDTE[3] == "39" || respuestaCrearDTE[3] == "41")
                                                    {
                                                        PDFBoleta pdfBoleta = new PDFBoleta();
                                                        string pdfBoleta_base64 = pdfBoleta.CrearBoleta(respuestaCrearDTE[1]);
                                                        respuestaExito = respuestaExito + @", ""PDF"": """ + pdfBoleta_base64 + @"""";
                                                        respuesta.Content = respuestaExito;
                                                    }
                                                    else
                                                    {
                                                        PDF pdf = new PDF();
                                                        string pdf_base64 = pdf.CrearPDF(respuestaCrearDTE[1]);
                                                        respuestaExito = respuestaExito + @", ""PDF"": """ + pdf_base64 + @"""";
                                                        respuesta.Content = respuestaExito;

                                                    }


                                                }
                                                if (arrayPedido.Contains("XML"))
                                                {
                                                    //CODIFICAR XML DE SOBRE A BASE 64
                                                    XmlDocument oDoc2 = new XmlDocument();
                                                    oDoc2.PreserveWhitespace = true;
                                                    oDoc2.Load(respuestaCrearDTE[1]);
                                                    string str_xml = oDoc2.InnerXml;
                                                    string encodedStr = Convert.ToBase64String(Encoding.UTF8.GetBytes(str_xml));
                                                    respuestaExito = respuestaExito + @", ""XML"": """ + encodedStr + @"""";
                                                    respuesta.Content = respuestaExito;

                                                }
                                                if (arrayPedido.Contains("TIMBRE"))
                                                {
                                                    //GENERAR EL TIMBRE DEL PDF EN PNG 2021*967 Y CODIFICARLO EN BASE64
                                                    //CAPTURAR EL NODO TED DEL XML
                                                    XmlDocument xml_sobre = new XmlDocument();
                                                    xml_sobre.PreserveWhitespace = true;
                                                    xml_sobre.Load(respuestaCrearDTE[1]);
                                                    XmlNodeList DocumentoList = xml_sobre.GetElementsByTagName("Documento");
                                                    var nodoDocumento = DocumentoList.Item(0).ChildNodes;
                                                    string nodoTed = nodoDocumento.Item(nodoDocumento.Count - 4).OuterXml;

                                                    //ENVIAMOS EL STRING DEL NODO TED Y EL FOLIO DEL DTE Y EL TIPO DTE
                                                    string[] respuesta_timbre = new string[] { };
                                                    respuesta_timbre = dte.generarTimbrePDF417(nodoTed, respuestaCrearDTE[2], respuestaCrearDTE[3]);
                                                    //respuesta_timbre[0] = Timbre en base64
                                                    //respuesta_timbre[1] = ruta del timbre


                                                    respuestaExito = respuestaExito + @", ""TIMBRE"": """ + respuesta_timbre[0] + @"""";
                                                    respuesta.Content = respuestaExito;
                                                }
                                                if (arrayPedido.Contains("LOGO"))
                                                {
                                                    //LOGO EN PNG DE LA EMPRESA 150*100
                                                    respuestaExito = respuestaExito + @", ""LOGO"": ""aki ba el logo empresa png en base64""";
                                                    respuesta.Content = respuestaExito;
                                                }
                                                if (arrayPedido.Contains("FOLIO"))
                                                {
                                                    //FOLIO DE LA FACTURA
                                                    respuestaExito = respuestaExito + @", ""FOLIO"": """ + respuestaCrearDTE[2] + @"""";
                                                    respuesta.Content = respuestaExito;
                                                }
                                                if (arrayPedido.Contains("RESOLUCION"))
                                                {
                                                    respuestaExito = respuestaExito + @", ""RESOLUCION"": { ""fecha"": """ + fechaResolucion + @""" , ""numero"": " + numeroResolucion + "}";
                                                    respuesta.Content = respuestaExito;
                                                }

                                                respuestaExito = respuestaExito + "}";
                                                respuesta.Content = respuestaExito;
                                                respuesta.ContentType = "application/json";
                                                respuesta.StatusCode = 200;

                                                //SI TODO SALE OK, ENVIAMOS POR CORREO AL CLIENTE EL XML DEL DTE ENVIADO AL SII PERO CON SOBRE DIRIGIDO A CLIENTE
                                                /* AcuseRecibo acuseRecibo = new AcuseRecibo();
                                                 string respuestaAcuseRecibo = acuseRecibo.enviarEmailAcuseRecibo(respuestaCrearDTE[5], respuestaCrearDTE[6],"XML Cliente");*/

                                                using (StreamWriter writetext = new StreamWriter(fileColaRequest))
                                                {
                                                    writetext.WriteLine("Sin Request");
                                                    writetext.Close();
                                                }

                                                //GUARDA LAS RESPONSE CON CONEXION
                                                /*string hora_actual_response_con_conexion = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff");
                                                using (StreamWriter writetext = new StreamWriter(@"C:\inetpub\wwwroot\api_agrodte\AgroDTE_Archivos\Log\Response_Con_conexion_" + hora_actual_response_con_conexion + ".txt"))
                                                {
                                                    writetext.WriteLine(respuesta.Content);
                                                }*/

                                               

                                                string log_Response3 = "INSERT INTO log_event (mensaje_log_event, fecha_log_event, referencia_log_event,query_request_log_event) VALUES ('Response Con Conexion',NOW(),'- ','" + respuesta.Content.ToString() + "')";
                                                conexion.Consulta(log_Response3);

                                                return respuesta;
                                            }
                                            else
                                            {
                                                //SI NO TRAE NUMERO ES PORQUE OCURRIO ERROR Y DEBE RETORNAR ESTE MENSAJE AL CLIENTE
                                                string respuesta_trackid_error = @"{""error"": {""message"": ""Error con numero track id"",""code"": ""OF-0"",""details"": [{""field"": ""Error de Track id"",""issue"": " + TrackId_str + "}]}}";

                                                respuesta.Content = respuesta_trackid_error;
                                                respuesta.ContentType = "application/json";
                                                respuesta.StatusCode = 400;

                                                using (StreamWriter writetext = new StreamWriter(fileColaRequest))
                                                {
                                                    writetext.WriteLine("Sin Request");
                                                    writetext.Close();
                                                }

                                                //GUARDA LAS RESPONSE ERROR TRACK ID
                                                /*string hora_actual_response_error_track_id = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff");
                                                using (StreamWriter writetext = new StreamWriter(@"C:\inetpub\wwwroot\api_agrodte\AgroDTE_Archivos\Log\Response_Error_numeroTrackid_" + hora_actual_response_error_track_id + ".txt"))
                                                {
                                                    writetext.WriteLine(respuesta.Content);
                                                }*/

                                                string log_Response2 = "INSERT INTO log_event (mensaje_log_event, fecha_log_event, referencia_log_event,query_request_log_event) VALUES ('Response Error numero track id',NOW(),'- ','" + respuesta.Content + "')";
                                                conexion.Consulta(log_Response2);

                                                return respuesta;
                                            }


                                        }
                                        else
                                        {
                                            //NO EXISTE CONEXION, GENERAR SOLAMENTE PDF  Y REGISTRAR SIN TRACK ID EN LA TABLA SOBRE
                                           
                                            //TRAEMOS LA ID DEL SOBRE EN LA BD
                                            string queryIdSobre = "SELECT id_envio_dte FROM envio_dte WHERE rutaxml_envio_dte = '" + respuestaCrearDTE[1] + "'";
                                            queryIdSobre = queryIdSobre.Replace("\\", "\\\\");
                                            List<string> listIdSobre = conexion.Select(queryIdSobre);
                                            string id_sobre = listIdSobre[0];

                                            //GENERACION DE RESPUESTAS

                                            string respuestaExito = @"{""TOKEN"":""" + id_sobre + @"""";


                                            if (arrayPedido.Contains("PDF"))
                                            {
                                                //GENERAR PDF
                                                if (respuestaCrearDTE[3] == "39" || respuestaCrearDTE[3] == "41")
                                                {
                                                    PDFBoleta pdfBoleta = new PDFBoleta();
                                                    string pdfBoleta_base64 = pdfBoleta.CrearBoleta(respuestaCrearDTE[1]);
                                                    respuestaExito = respuestaExito + @", ""PDF"": """ + pdfBoleta_base64 + @"""";
                                                    respuesta.Content = respuestaExito;
                                                }
                                                else
                                                {
                                                    PDF pdf = new PDF();
                                                    string pdf_base64 = pdf.CrearPDF(respuestaCrearDTE[1]);
                                                    respuestaExito = respuestaExito + @", ""PDF"": """ + pdf_base64 + @"""";
                                                    respuesta.Content = respuestaExito;

                                                }

                                            }
                                            if (arrayPedido.Contains("XML"))
                                            {
                                                //CODIFICAR XML DE SOBRE A BASE 64
                                                XmlDocument oDoc2 = new XmlDocument();
                                                oDoc2.PreserveWhitespace = true;
                                                oDoc2.Load(respuestaCrearDTE[1]);
                                                string str_xml = oDoc2.InnerXml;
                                                string encodedStr = Convert.ToBase64String(Encoding.UTF8.GetBytes(str_xml));
                                                respuestaExito = respuestaExito + @", ""XML"": """ + encodedStr + @"""";
                                                respuesta.Content = respuestaExito;

                                            }
                                            if (arrayPedido.Contains("TIMBRE"))
                                            {
                                                //GENERAR EL TIMBRE DEL PDF EN PNG 2021*967 Y CODIFICARLO EN BASE64
                                                //CAPTURAR EL NODO TED DEL XML
                                                XmlDocument xml_sobre = new XmlDocument();
                                                xml_sobre.PreserveWhitespace = true;
                                                xml_sobre.Load(respuestaCrearDTE[1]);
                                                XmlNodeList DocumentoList = xml_sobre.GetElementsByTagName("Documento");
                                                var nodoDocumento = DocumentoList.Item(0).ChildNodes;
                                                string nodoTed = nodoDocumento.Item(nodoDocumento.Count - 4).OuterXml;

                                                //ENVIAMOS EL STRING DEL NODO TED Y EL FOLIO DEL DTE Y EL TIPO DTE
                                                string[] respuesta_timbre = new string[] { };
                                                respuesta_timbre = dte.generarTimbrePDF417(nodoTed, respuestaCrearDTE[2], respuestaCrearDTE[3]);
                                                //respuesta_timbre[0] = Timbre en base64
                                                //respuesta_timbre[1] = ruta del timbre


                                                respuestaExito = respuestaExito + @", ""TIMBRE"": """ + respuesta_timbre[0] + @"""";
                                                respuesta.Content = respuestaExito;
                                            }
                                            if (arrayPedido.Contains("LOGO"))
                                            {
                                                //LOGO EN PNG DE LA EMPRESA 150*100
                                                respuestaExito = respuestaExito + @", ""LOGO"": ""aki ba el logo empresa png en base64""";
                                                respuesta.Content = respuestaExito;
                                            }
                                            if (arrayPedido.Contains("FOLIO"))
                                            {
                                                //FOLIO DE LA FACTURA
                                                respuestaExito = respuestaExito + @", ""FOLIO"": """ + respuestaCrearDTE[2] + @"""";
                                                respuesta.Content = respuestaExito;
                                            }
                                            if (arrayPedido.Contains("RESOLUCION"))
                                            {
                                                respuestaExito = respuestaExito + @", ""RESOLUCION"": { ""fecha"": """ + fechaResolucion + @""" , ""numero"": " + numeroResolucion + "}";
                                                respuesta.Content = respuestaExito;
                                            }
                                            respuestaExito = respuestaExito + "}";

                                            respuesta.Content = respuestaExito;
                                            respuesta.ContentType = "application/json";
                                            respuesta.StatusCode = 200;
                                        }

                                        using (StreamWriter writetext = new StreamWriter(fileColaRequest))
                                        {
                                            writetext.WriteLine("Sin Request");
                                            writetext.Close();
                                        }


                                        //GUARDA LAS RESPONSE DE SIN CONEXION
                                       /* string hora_actual_response_sin_conexion = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff");
                                        using (StreamWriter writetext = new StreamWriter(@"C:\inetpub\wwwroot\api_agrodte\AgroDTE_Archivos\Log\Response_Sin_conexion_" + hora_actual_response_sin_conexion + ".txt"))
                                        {
                                            writetext.WriteLine(respuesta.Content);
                                        }*/


                                        string log_Response = "INSERT INTO log_event (mensaje_log_event, fecha_log_event, referencia_log_event,query_request_log_event) VALUES ('Response Sin Conexion',NOW(),'- ','" + respuesta.Content + "')";
                                        conexion.Consulta(log_Response);

                                        return respuesta;

                                    }
                                    else if (respuestaCrearDTE[0].Contains("XML Invalido:") || respuestaCrearDTE[4].Contains("XML Invalido:"))
                                    {//se agregan 3 guiones "---" para separar los errores del dte que va al si, con el dte que va al cliente, cualquiera de los 2 puede tener errores
                                     //XML RECHAZADO SCHEMA
                                        string JsonResponse = @"{""error"": {""message"": ""Validación de Esquema"",""code"": ""OF-08"",""details"": [{""field"": ""Error de Schema"",""issue"": """ + respuestaCrearDTE[0] + "---" + respuestaCrearDTE[4] + @"""}]}}";

                                        respuesta.Content = JsonResponse;
                                        respuesta.ContentType = "application/json";
                                        respuesta.StatusCode = 400;

                                        using (StreamWriter writetext = new StreamWriter(fileColaRequest))
                                        {
                                            writetext.WriteLine("Sin Request");
                                            writetext.Close();
                                        }

                                        //GUARDA LAS RESPONSE ERROR DE ESQUEMA O SCHEMA
                                        /*string hora_actual_response_error_schema = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff");
                                        using (StreamWriter writetext = new StreamWriter(@"C:\inetpub\wwwroot\api_agrodte\AgroDTE_Archivos\Log\Response_Error_Schema_" + hora_actual_response_error_schema + ".txt"))
                                        {
                                            writetext.WriteLine(respuesta.Content);
                                        }*/

                                        string log_Response = "INSERT INTO log_event (mensaje_log_event, fecha_log_event, referencia_log_event,query_request_log_event) VALUES ('Response Error Schema',NOW(),'- ','" + respuesta.Content + "')";
                                        conexion.Consulta(log_Response);

                                        return respuesta;
                                    }
                                    else if (respuestaCrearDTE[0].Contains("Error") || respuestaCrearDTE[4].Contains("Error"))
                                    {//se agregan 3 guiones "---" para separar los errores del dte que va al si, con el dte que va al cliente, cualquiera de los 2 puede tener errores
                                        string JsonResponse = @"{""error"": {""message"": ""Validacion de Montos"",""code"": ""OF-09"",""details"": [{""field"": ""Error de Operacion"",""issue"": """ + respuestaCrearDTE[0] + "---" + respuestaCrearDTE[4] + @"""}]}}";

                                        respuesta.Content = JsonResponse;
                                        respuesta.ContentType = "application/json";
                                        respuesta.StatusCode = 400;

                                        using (StreamWriter writetext = new StreamWriter(fileColaRequest))
                                        {
                                            writetext.WriteLine("Sin Request");
                                            writetext.Close();
                                        }

                                        //GUARDA LAS RESPONSE ERROR DE OPERACION
                                        /*string hora_actual_response_error_operacion = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff");
                                        using (StreamWriter writetext = new StreamWriter(@"C:\inetpub\wwwroot\api_agrodte\AgroDTE_Archivos\Log\Response_Error_Operacion_" + hora_actual_response_error_operacion + ".txt"))
                                        {
                                            writetext.WriteLine(respuesta.Content);
                                        }*/

                                        string log_Response = "INSERT INTO log_event (mensaje_log_event, fecha_log_event, referencia_log_event,query_request_log_event) VALUES ('Response Error Operacion',NOW(),'- ','" + respuesta.Content + "')";
                                        conexion.Consulta(log_Response);


                                        return respuesta;
                                    }
                                    else
                                    {//se agregan 3 guiones "---" para separar los errores del dte que va al si, con el dte que va al cliente, cualquiera de los 2 puede tener errores
                                        string JsonResponse = @"{""error"": {""message"": ""Error no controlado"",""code"": ""OF-666"",""details"": [{""field"": ""Error Desconocido"",""issue"": """ + respuestaCrearDTE[0] + "---" + respuestaCrearDTE[4] + @"""}]}}";

                                        respuesta.Content = JsonResponse;
                                        respuesta.ContentType = "application/json";
                                        respuesta.StatusCode = 400;

                                        using (StreamWriter writetext = new StreamWriter(fileColaRequest))
                                        {
                                            writetext.WriteLine("Sin Request");
                                            writetext.Close();
                                        }

                                        //GUARDA LAS RESPONSE ERROR NO CONTROLADO
                                        /*string hora_actual_response_error_no_controlado = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff");
                                        using (StreamWriter writetext = new StreamWriter(@"C:\inetpub\wwwroot\api_agrodte\AgroDTE_Archivos\Log\Response_Error_No_Controlado_" + hora_actual_response_error_no_controlado + ".txt"))
                                        {
                                            writetext.WriteLine(respuesta.Content);
                                        }*/

                                        string log_Response = "INSERT INTO log_event (mensaje_log_event, fecha_log_event, referencia_log_event,query_request_log_event) VALUES ('Response Error No Controlado',NOW(),'- ','" + respuesta.Content + "')";
                                        conexion.Consulta(log_Response);

                                        return respuesta;
                                    }
                                }
                                else
                                {
                                    string JsonResponse = @"{""statusCode"": 401,""message"": ""Access denied due to invalid subscription key. Make sure to provide a valid key for an active subscription.""}";

                                    respuesta.Content = JsonResponse;
                                    respuesta.ContentType = "application/json";
                                    respuesta.StatusCode = 401;

                                    using (StreamWriter writetext = new StreamWriter(fileColaRequest))
                                    {
                                        writetext.WriteLine("Sin Request");
                                        writetext.Close();
                                    }

                                    //GUARDA LAS RESPONSE ERROR SIN KEY
                                    /*string hora_actual_response_error_sin_key = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff");
                                    using (StreamWriter writetext = new StreamWriter(@"C:\inetpub\wwwroot\api_agrodte\AgroDTE_Archivos\Log\Response_Error_SinKey_" + hora_actual_response_error_sin_key + ".txt"))
                                    {
                                        writetext.WriteLine(respuesta.Content);
                                    }*/

                                    string log_Response = "INSERT INTO log_event (mensaje_log_event, fecha_log_event, referencia_log_event,query_request_log_event) VALUES ('Response Error Sin ApiKey',NOW(),'- ','" + respuesta.Content + "')";
                                    conexion.Consulta(log_Response);

                                    return respuesta;
                                }


                               // reader.Close();

                                //ESTO SIMULA EL CAMBIO DE ESTADO DE UNA REQUEST
                                using (StreamWriter writetext = new StreamWriter(fileColaRequest))
                                {
                                    writetext.WriteLine("Sin Request");
                                    writetext.Close();
                                }

                                r = true;

                                //GUARDA LAS RESPONSE DESCONOCIDO
                               /* string hora_actual_response_desconocida = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff");
                                using (StreamWriter writetext = new StreamWriter(@"C:\inetpub\wwwroot\api_agrodte\AgroDTE_Archivos\Log\Response_Desconocido_" + hora_actual_response_desconocida + ".txt"))
                                {
                                    writetext.WriteLine(respuesta.Content);
                                }*/

                                return respuesta;
                                //break;
                            }
                           
                        }
                    }
                }
                else
                {
                    //NO EXISTE ARCHIVO COLA REQUEST, SE CREA...
                    using (StreamWriter writetext = new StreamWriter(fileColaRequest))
                    {
                        writetext.WriteLine("Procesando Request");
                        writetext.Close();
                    }

                    Thread.Sleep(2000); //ESTO SIMULA LO QUE SE DEMORA UNA FACTURA O BOLETA
                                        //Console.WriteLine("No hay request procesando...");



                    //ESTO SIMULA EL CAMBIO DE ESTADO DE UNA REQUEST
                    using (StreamWriter writetext = new StreamWriter(fileColaRequest))
                    {
                        writetext.WriteLine("Sin Request");
                        writetext.Close();
                    }

                    //Console.WriteLine("No existe archivo creando...");
                    r = false;
                }


                //r = true;
                //i = getEstadoRequest();

            }
            // -------------------------------------------------------------------------------------------------------

            //GUARDA LAS RESPONSE DESCONOCIDO
           /* string hora_actual_response_desconocida_2 = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff");
            using (StreamWriter writetext = new StreamWriter(@"C:\inetpub\wwwroot\api_agrodte\AgroDTE_Archivos\Log\Response_Desconocido_2" + hora_actual_response_desconocida_2 + ".txt"))
            {
                writetext.WriteLine(respuesta.Content);
            }*/
            return respuesta;

        }

        public static string verificarDirectorio(string directorio)
        {
            if (!Directory.Exists(directorio))
            {
               
                try
                {
                    Directory.CreateDirectory(directorio);
                    return "Creado Directorio";
                   
                }
                catch (Exception e)
                {
                    return e.Message;


                }

            }
            return "Directorio Existe";
        }

        public static string checkPing(string host)
        {
            try
            {
                Ping myPing = new Ping();
                // String host = "192.168.1.6";//agroplastic.cl
                byte[] buffer = new byte[32];
                int timeout = 10000;
                PingOptions pingOptions = new PingOptions();
                PingReply reply = myPing.Send(host, timeout, buffer, pingOptions);
                //Console.WriteLine(reply.Status == IPStatus.Success);

                if (reply.Status == IPStatus.Success)
                {
                    return "Conexion Exitosa";
                }
            }
            catch (Exception e)
            {

                return "Hubo Error en ping - " + e.Message;
            }

            return "";
        }

        public static string checkConnection(string host)
        {

            try
            {
                System.Net.WebClient client = new WebClient();
                client.DownloadData(host);//http://www.agroplastic.cl, espera 20seg si no hay conexion aprox
                return "Conexion Exitosa";
            }
            catch (Exception e)
            {

                return "hubo error en establecer conexion con SII - " + e.Message;
            }

        }

        public class DTEJson
        {
            public object response { get; set; }
            public object dte { get; set; }

        }

       



    }
}
