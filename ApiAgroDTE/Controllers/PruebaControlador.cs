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
        public static string servidor_boletas = "apicert"; //api: produccion --  apicert: certificacion
        public static string servidor_facturas = "maullin"; //maullin: certificacion -- palena: produccion
        public static string directorio_archivos = @"C:\inetpub\wwwroot\api_agrodte\AgroDTE_Archivos";

        //[HttpGet("api/PruebaMetodo/nombre/{id}")]

        //public string GetDevolverFactura(int id)
        //{
        //    for (bool i = false; i == false;)
        //    {
        //        i = getEstadoRequest();
        //    }

        //    return "Factura lista !";


        //}


        [HttpGet("api/PruebaMetodo/nombre/{id}")]

        public string GetDevolverNombre(int id)
        {
            switch (id)
            {
                case 1:

                    return "El Cheloko";
                    break;
                case 2:
                    return "El NaxoFish";
                    break;

                default:

                    break;
            }
            return "El Vergamix";
        }

       

        [HttpGet("api/PruebaMetodo/edad/{id}-{id2}")]
        public string GetDevolverEdad(int id, int id2)
        {
           
            return "los datos recbibidos son"+id+" - "+id2;
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
                string pdf_base64 = pdf.CrearPDF(path_archivo);
                JsonResponse = @"{""statusCode"": 200,""pdf_base64"": """ + pdf_base64 + @""", ""tipo_dte"": """+tipo_dte_str+@"""}";
                respuesta.Content = JsonResponse;
                respuesta.ContentType = "application/json";
                respuesta.StatusCode = 200;
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

                if (int.TryParse(TrackId_str, out int numeroEnvio))
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

                                string archivostr = values.ToString();
                                using (StreamWriter writetext = new StreamWriter(@"C:\inetpub\wwwroot\api_agrodte\request.txt"))
                                {
                                    writetext.WriteLine(archivostr);
                                } 

                               
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

                                    return respuesta;
                                }


                                //GUARDAR LA REQUEST ENVIADA
                                ConexionBD conexion = new ConexionBD();
                                DTE dte = new DTE();
                                string cuerpoDte = values.GetProperty("dte").ToString();
                                string mensajeLogEvent = "JsonElement recibido del ERP para la creacion de DTE";
                                string direccionEnvio = "IP";
                                string consultaDte = "INSERT INTO log_event (mensaje_log_event, fecha_log_event, referencia_log_event,query_request_log_event) VALUES ('" + mensajeLogEvent + "',NOW(),'- ','" + cuerpoDte + "')";
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
                                        //CHEQUEAR SI HAY CONEXION A INTERNET 
                                        string respuestaPing = checkPing("maullin.sii.cl");
                                        string respuestaConexion = checkConnection("https://maullin.sii.cl/DTEWS/CrSeed.jws");



                                        //if (respuestaPing == respuestaConexion)
                                        if(respuestaPing == "Conexion Exitosa")
                                        {
                                            //EXISTE CONEXION, SOLICITAR SEMILLA, TOKEN Y ENVIAR SOBRE DTE
                                            EnvioDTE envio = new EnvioDTE();
                                            string respuestaEnvio = "";
                                            string TrackId_str = "";
                                            if (respuestaCrearDTE[3] == "39" || respuestaCrearDTE[3] == "41")
                                            {
                                                respuestaEnvio = envio.enviarSobreBoleta(respuestaCrearDTE[1], rutEmisor, rutEmpresa);
                                                TrackId_str = respuestaEnvio;



                                            }
                                            else
                                            {
                                                respuestaEnvio = envio.enviarSobre(respuestaCrearDTE[1], rutEmisor, rutEmpresa);
                                                //string respuestaEnvio es el TRACKID EN XML
                                                XmlDocument xmlDoc2 = new XmlDocument();
                                                xmlDoc2.LoadXml(respuestaEnvio);
                                                XmlNodeList TrackId = xmlDoc2.GetElementsByTagName("string");
                                                TrackId_str = TrackId[0].InnerXml;
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

                                        return respuesta;

                                    }
                                    else if (respuestaCrearDTE[0].Contains("XML Invalido:") || respuestaCrearDTE[4].Contains("XML Invalido:"))
                                    {//se agregan 3 guiones "---" para separar los errores del dte que va al si, con el dte que va al cliente, cualquiera de los 2 puede tener errores
                                     //XML RECHAZADO SCHEMA
                                        string JsonResponse = @"{""error"": {""message"": ""Validación de Esquema"",""code"": ""OF-08"",""details"": [{""field"": ""Error de Schema"",""issue"": " + respuestaCrearDTE[0] + "---" + respuestaCrearDTE[4] + "}]}}";

                                        respuesta.Content = JsonResponse;
                                        respuesta.ContentType = "application/json";
                                        respuesta.StatusCode = 400;

                                        using (StreamWriter writetext = new StreamWriter(fileColaRequest))
                                        {
                                            writetext.WriteLine("Sin Request");
                                            writetext.Close();
                                        }
                                        return respuesta;
                                    }
                                    else if (respuestaCrearDTE[0].Contains("Error") || respuestaCrearDTE[4].Contains("Error"))
                                    {//se agregan 3 guiones "---" para separar los errores del dte que va al si, con el dte que va al cliente, cualquiera de los 2 puede tener errores
                                        string JsonResponse = @"{""error"": {""message"": ""Validacion de Montos"",""code"": ""OF-09"",""details"": [{""field"": ""Error de Operacion"",""issue"": " + respuestaCrearDTE[0] + "---" + respuestaCrearDTE[4] + "}]}}";

                                        respuesta.Content = JsonResponse;
                                        respuesta.ContentType = "application/json";
                                        respuesta.StatusCode = 400;

                                        using (StreamWriter writetext = new StreamWriter(fileColaRequest))
                                        {
                                            writetext.WriteLine("Sin Request");
                                            writetext.Close();
                                        }
                                        return respuesta;
                                    }
                                    else
                                    {//se agregan 3 guiones "---" para separar los errores del dte que va al si, con el dte que va al cliente, cualquiera de los 2 puede tener errores
                                        string JsonResponse = @"{""error"": {""message"": ""Error no controlado"",""code"": ""OF-666"",""details"": [{""field"": ""Error Desconocido"",""issue"": " + respuestaCrearDTE[0] + "---" + respuestaCrearDTE[4] + "}]}}";

                                        respuesta.Content = JsonResponse;
                                        respuesta.ContentType = "application/json";
                                        respuesta.StatusCode = 400;

                                        using (StreamWriter writetext = new StreamWriter(fileColaRequest))
                                        {
                                            writetext.WriteLine("Sin Request");
                                            writetext.Close();
                                        }
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
