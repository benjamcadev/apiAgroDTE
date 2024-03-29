using System.Xml.Linq;
using System.Collections.Generic;
using System;
using System.Xml;
using System.IO;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using Hefesto.Proxys.Certificacion;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using RestSharp;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace ApiAgroDTE.Clases
{
    public class EnvioDTE
    {
        string fechaActual = DateTime.Now.ToString("yyyyMMddhhmmssfff");
        public string crearSobreEnvio(string TpoDTE, string rutaXml,string directorioFechaActual,int folio){

            string fechahhmm = System.DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
            string fecresolucion3 = System.DateTime.Now.ToString("yyyy-MM-dd");
            XNamespace xmlns = XNamespace.Get("http://www.sii.cl/SiiDte");
            XNamespace xsi = XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance");

            string locationSchema = "";
            if (TpoDTE == "39" || TpoDTE == "41")
            {
                locationSchema = "EnvioBOLETA_v11.xsd";
                
            }
            else { locationSchema = "EnvioDTE_v10.xsd"; }

            XNamespace schemaLocation = XNamespace.Get("http://www.sii.cl/SiiDte "+locationSchema);


            
            string fileNameXML =  @"\SobreEnvio" + fechaActual + ".xml";
            string EnvioDTE_xml =  directorioFechaActual + fileNameXML;
            //string path = Path.Combine(Environment.CurrentDirectory, directorioFechaActual,fileNameXML);
            //string EnvioDTE_xml =@"C:\Users\Benjamin\source\repos\ApiAgroDTE\ApiAgroDTE\XML\SobreEnvio.xml";

            //string archxml = @"\\192.168.1.4\api_agrodte\XML\C#\SobreEnvio.xml";

            //Llamar datos de empresa de la base de datos

            ConexionBD conexion = new ConexionBD();
            List<string> resultDatosempresa = conexion.Select("SELECT rut_empresa,rut_rrll,DATE_FORMAT(fecha_res, '%Y-%m-%d') AS fecha ,numero_res FROM empresa WHERE id_empresa=1");
            
            //RUT DEL SII
            string RutReceptor = "60803000-K";

             XElement setdte =   new XElement("Caratula",
                new XAttribute("version", "1.0"),
                new XElement("RutEmisor", resultDatosempresa[0]),
                new XElement("RutEnvia", resultDatosempresa[1]),
                new XElement("RutReceptor", RutReceptor),
                new XElement("FchResol", resultDatosempresa[2]),
                new XElement("NroResol", resultDatosempresa[3]),
                new XElement("TmstFirmaEnv", fechahhmm));

                setdte.Add(new XElement("SubTotDTE",
                            new XElement("TpoDTE", TpoDTE),
                            new XElement("NroDTE", "1")));

           
                string etiquetaEnvio = "";
                if (TpoDTE == "39" || TpoDTE == "41")
                {
                    etiquetaEnvio = "EnvioBOLETA";
                }
                else { etiquetaEnvio = "EnvioDTE"; }


                XDocument miXML = new XDocument(new XDeclaration("1.0", "ISO-8859-1", ""),
               new XElement(etiquetaEnvio,
               new XElement("SetDTE",
               new XAttribute("ID", "SetDoc"),
               setdte
               )));
            
            

                

                //// Quite la identacion del documento
                string[] strLineasDte = miXML.ToString().Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i <= strLineasDte.Length - 1; i++)
                strLineasDte[i] = strLineasDte[i].TrimStart();

                //// Recargue el documento xml sin identación
                //// utilizando el objeto xmlDocument
                string p = string.Join("\r\n", strLineasDte);
                miXML = XDocument.Parse(p, LoadOptions.PreserveWhitespace);
                miXML.Declaration = new XDeclaration("1.0", "ISO-8859-1", null);
                miXML.Save(EnvioDTE_xml);

                
                XmlDocument oDoc2 = new XmlDocument();
                oDoc2.PreserveWhitespace = true;
                oDoc2.Load(EnvioDTE_xml);
                oDoc2.InnerXml = oDoc2.InnerXml.Replace("<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>", "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>");
                oDoc2.Save(EnvioDTE_xml);

                //Cargar el XML DTE en el sobre
                XmlDocument xml_Dte = new XmlDocument();
                xml_Dte.PreserveWhitespace = true;

                xml_Dte.Load(rutaXml);


                oDoc2.SelectSingleNode(etiquetaEnvio+"/SetDTE").AppendChild(oDoc2.ImportNode(xml_Dte.DocumentElement, true));
                oDoc2.PreserveWhitespace = true;
                oDoc2.Save(EnvioDTE_xml);

                // firma ENVIODTE--------------------------------------------------------------------------------------------------------------------
                
                oDoc2.DocumentElement.SetAttribute("xmlns", "http://www.sii.cl/SiiDte");
                oDoc2.DocumentElement.SetAttribute("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
                oDoc2.DocumentElement.SetAttribute("schemaLocation", "http://www.w3.org/2001/XMLSchema-instance", "http://www.sii.cl/SiiDte "+locationSchema);
                oDoc2.DocumentElement.SetAttribute("version", "1.0");
                oDoc2.PreserveWhitespace = true;
                oDoc2.Save(EnvioDTE_xml);

                StreamReader xml_firmar = new StreamReader(EnvioDTE_xml, Encoding.GetEncoding("ISO-8859-1"));
                string xml_firmar_setdte = xml_firmar.ReadToEnd();
                xml_firmar.Close();

                string nombrecertificado = "RUBEN SALATIEL RIVERA GALLEGUILLOS";
                Certificado certificado = new Certificado();   
                string firmado = certificado.Firmar(nombrecertificado, xml_firmar_setdte, "#SetDoc");
                XDocument xml_firmado = XDocument.Parse(firmado, LoadOptions.PreserveWhitespace);
                xml_firmado.Save(EnvioDTE_xml);


                XmlDocument oDoc3 = new XmlDocument();
                oDoc3.PreserveWhitespace = true;
                oDoc3.Load(EnvioDTE_xml);
                oDoc3.InnerXml = oDoc3.InnerXml.Replace("<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>", "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>");
                oDoc3.Save(EnvioDTE_xml);

            //GUARDAR EN LA BASE DE DATOS EL SOBRE DE ENVIO SIN NUMERO DE TRACK ID AUN
                string envio_cliente = "";
                if (TpoDTE == "39" || TpoDTE == "41")
                {
                envio_cliente = "2";
            }
                else
                {
                envio_cliente = "0";
            }
                string queryUpdateDirectorio = "INSERT INTO envio_dte (estado_envio_dte,rutaxml_envio_dte,fecha_envio_dte,envio_cliente_envio_dte,tipo_dte_envio_dte) VALUES ('No Enviado','" + EnvioDTE_xml + "',NOW(),'"+envio_cliente+"','"+TpoDTE+"')";
                queryUpdateDirectorio = queryUpdateDirectorio.Replace("\\", "\\\\");
                conexion.Consulta(queryUpdateDirectorio);

            //RESCATAR E INSERTAR EL ID DEL SOBRE EN LA FACTURA
                string queryIdSobre = "SELECT id_envio_dte FROM envio_dte WHERE rutaxml_envio_dte = '"+ EnvioDTE_xml + "'";
                queryIdSobre = queryIdSobre.Replace("\\", "\\\\");
                List<string> ResultIdSobre = conexion.Select(queryIdSobre);

                string tableName = "";
                string folioDte = "";

            if (TpoDTE == "33")
            {
                tableName = "factura";
                folioDte = "folio_factura";

            }
            else if (TpoDTE == "34")
            {
                tableName = "factura_exenta";
                folioDte = "folio_factura_exenta";
            }
            else if (TpoDTE == "61")
            {
                tableName = "nota_credito";
                folioDte = "folio_nota_credito";
            }
            else if (TpoDTE == "56")
            {
                tableName = "nota_debito";
                folioDte = "folio_nota_debito";
            }
            else if (TpoDTE == "52")
            {
                tableName = "guia_despacho";
                folioDte = "folio_guia_despacho";
            }
            else if (TpoDTE == "39")
            {
                tableName = "boleta";
                folioDte = "folio_boleta";
            }
            else if (TpoDTE == "41")
            {
                tableName = "boleta_exenta";
                folioDte = "folio_boleta_exenta";
            }

            string queryUpdateidSobre = "UPDATE "+tableName+" SET id_envio_dte_fk = '"+ResultIdSobre[0]+"' WHERE "+folioDte+" = '"+folio+"'";

                conexion.Consulta(queryUpdateidSobre);


            return EnvioDTE_xml;
        }

        public string crearSobreEnvioCliente(string TpoDTE, string rutaXml,string directorioFechaActual,int folio, string strRUTRecep){

            string fechahhmm = System.DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
            string fecresolucion3 = System.DateTime.Now.ToString("yyyy-MM-dd");
            XNamespace xmlns = XNamespace.Get("http://www.sii.cl/SiiDte");
            XNamespace xsi = XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance");

            string locationSchema = "";
            if (TpoDTE == "39" || TpoDTE == "41")
            {
                locationSchema = "EnvioBOLETA_v11.xsd";
                
            }
            else { locationSchema = "EnvioDTE_v10.xsd"; }

            XNamespace schemaLocation = XNamespace.Get("http://www.sii.cl/SiiDte "+locationSchema);


            //string fechaActual = DateTime.Now.ToString("yyyyMMddhhmmssfff");
            string fileNameXML =  @"\SobreEnvio" + fechaActual +"-cliente"+".xml";
            string EnvioDTE_xml =  directorioFechaActual + fileNameXML;
            //string path = Path.Combine(Environment.CurrentDirectory, directorioFechaActual,fileNameXML);
            //string EnvioDTE_xml =@"C:\Users\Benjamin\source\repos\ApiAgroDTE\ApiAgroDTE\XML\SobreEnvio.xml";

            //string archxml = @"\\192.168.1.4\api_agrodte\XML\C#\SobreEnvio.xml";

            //Llamar datos de empresa de la base de datos

            ConexionBD conexion = new ConexionBD();
            List<string> resultDatosempresa = conexion.Select("SELECT rut_empresa,rut_rrll,DATE_FORMAT(fecha_res, '%Y-%m-%d') AS fecha ,numero_res FROM empresa WHERE id_empresa=1");
            
            //RUT DEL SII
            string RutReceptor = strRUTRecep.ToUpper();

             XElement setdte =   new XElement("Caratula",
                new XAttribute("version", "1.0"),
                new XElement("RutEmisor", resultDatosempresa[0]),
                new XElement("RutEnvia", resultDatosempresa[1]),
                new XElement("RutReceptor", RutReceptor),
                new XElement("FchResol", resultDatosempresa[2]),
                new XElement("NroResol", resultDatosempresa[3]),
                new XElement("TmstFirmaEnv", fechahhmm));

            setdte.Add(new XElement("SubTotDTE",
                        new XElement("TpoDTE", TpoDTE),
                        new XElement("NroDTE", "1")));

        
            string etiquetaEnvio = "";
            if (TpoDTE == "39" || TpoDTE == "41")
            {
                etiquetaEnvio = "EnvioBOLETA";
            }
            else { etiquetaEnvio = "EnvioDTE"; }


            XDocument miXML = new XDocument(new XDeclaration("1.0", "ISO-8859-1", ""),
            new XElement(etiquetaEnvio,
            new XElement("SetDTE",
            new XAttribute("ID", "SetDoc"),
            setdte
            )));           

            //// Quite la identacion del documento
            string[] strLineasDte = miXML.ToString().Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i <= strLineasDte.Length - 1; i++)
            strLineasDte[i] = strLineasDte[i].TrimStart();

            //// Recargue el documento xml sin identación
            //// utilizando el objeto xmlDocument
            string p = string.Join("\r\n", strLineasDte);
            miXML = XDocument.Parse(p, LoadOptions.PreserveWhitespace);
            miXML.Declaration = new XDeclaration("1.0", "ISO-8859-1", null);
            miXML.Save(EnvioDTE_xml);

            
            XmlDocument oDoc2 = new XmlDocument();
            oDoc2.PreserveWhitespace = true;
            oDoc2.Load(EnvioDTE_xml);
            oDoc2.InnerXml = oDoc2.InnerXml.Replace("<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>", "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>");
            oDoc2.Save(EnvioDTE_xml);

            //Cargar el XML DTE en el sobre
            XmlDocument xml_Dte = new XmlDocument();
            xml_Dte.PreserveWhitespace = true;

            xml_Dte.Load(rutaXml);


            oDoc2.SelectSingleNode(etiquetaEnvio+"/SetDTE").AppendChild(oDoc2.ImportNode(xml_Dte.DocumentElement, true));
            oDoc2.PreserveWhitespace = true;
            oDoc2.Save(EnvioDTE_xml);

            // firma ENVIODTE--------------------------------------------------------------------------------------------------------------------
            
            oDoc2.DocumentElement.SetAttribute("xmlns", "http://www.sii.cl/SiiDte");
            oDoc2.DocumentElement.SetAttribute("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
            oDoc2.DocumentElement.SetAttribute("schemaLocation", "http://www.w3.org/2001/XMLSchema-instance", "http://www.sii.cl/SiiDte "+locationSchema);
            oDoc2.DocumentElement.SetAttribute("version", "1.0");
            oDoc2.PreserveWhitespace = true;
            oDoc2.Save(EnvioDTE_xml);

            StreamReader xml_firmar = new StreamReader(EnvioDTE_xml, Encoding.GetEncoding("ISO-8859-1"));
            string xml_firmar_setdte = xml_firmar.ReadToEnd();
            xml_firmar.Close();

            string nombrecertificado = "RUBEN SALATIEL RIVERA GALLEGUILLOS";
            Certificado certificado = new Certificado();   
            string firmado = certificado.Firmar(nombrecertificado, xml_firmar_setdte, "#SetDoc");
            XDocument xml_firmado = XDocument.Parse(firmado, LoadOptions.PreserveWhitespace);
            xml_firmado.Save(EnvioDTE_xml);


            XmlDocument oDoc3 = new XmlDocument();
            oDoc3.PreserveWhitespace = true;
            oDoc3.Load(EnvioDTE_xml);
            oDoc3.InnerXml = oDoc3.InnerXml.Replace("<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>", "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>");
            oDoc3.Save(EnvioDTE_xml);

            return EnvioDTE_xml;
        }


        public string enviarSobre(string archivo, string rutEmisor, string rutEmpresa)
        {
            
            //WebRequest request = WebRequest.Create("http://localhost:81/WebServiceEnvioDTE/EnvioSobreDTE.asmx/enviarSobreSII?archivo=" + archivo + "&rutEmisor=" + rutEmisor + "&rutEmpresa=" + rutEmpresa);
            //WebRequest request = WebRequest.Create("http://192.168.1.9:90/WebServiceEnvioDTE_Maullin/EnvioSobreDTE.asmx/enviarSobreSII?archivo="+ archivo + "&rutEmisor="+ rutEmisor + "&rutEmpresa="+ rutEmpresa);
            WebRequest request = WebRequest.Create("http://192.168.1.9:90/WebServiceEnvioDTE/EnvioSobreDTE.asmx/enviarSobreSII?archivo="+ archivo + "&rutEmisor="+ rutEmisor + "&rutEmpresa="+ rutEmpresa);
            request.Method = "GET";
            WebResponse response = request.GetResponse();
            string respuestaEnvio = "";
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                respuestaEnvio = reader.ReadToEnd(); // do something fun...
            }



            //27-01-2023: QUIERO GUARDAR LA RESPUESTA DEL SII PORQUE ALGUNOS NUMEROS DE ENVIO LOS GUARDA EN 0
            string archivostr = respuestaEnvio;
            string hora_actual = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff");
            using (StreamWriter writetext = new StreamWriter(@"C:\inetpub\wwwroot\api_agrodte\AgroDTE_Archivos\Log\Respuesta_SII_Sobre_"+hora_actual+".txt"))
            {
                writetext.WriteLine(archivostr);
            }

            return respuestaEnvio;
        }

        public string enviarSobreBoleta(string ruta_archivo, string rutEmisor, string rutEmpresa)
        {
            //archivo = ruta del xml a enviar

            string[] respuesta_token = obtenerToken();
            //respuesta_token[0] = true o false
            //respuesta_token[1] = Viene un mensaje de error si lo hay. En caso contrario viene el TOKEN

            if (respuesta_token[0] == "true")
            {
                //TOKEN VALIDO, UPDATEAR LA FECHA DEL ULTIMO USO DEL TOKEN
                ConexionBD conexion = new ConexionBD();
                conexion.Consulta("UPDATE token SET fecha_ultimo_uso_token= NOW() WHERE digitos_token= '"+ respuesta_token[1] + "'");

                //PREPARAMOS LOS CAMPOS A ENVIAR
                rutEmisor = rutEmisor.Replace("-", string.Empty);
                rutEmpresa = rutEmpresa.Replace("-", string.Empty);

                string pRutEmisor = rutEmisor.Substring(0, (rutEmisor.Length - 1));
                string pDigEmisor = rutEmisor.Substring(rutEmisor.Length - 1);
                string pRutEmpresa = rutEmpresa.Substring(0, (rutEmpresa.Length - 1));
                string pDigEmpresa = rutEmpresa.Substring(rutEmpresa.Length - 1);

                //METODO RESTSHARP----------------------------------------------------
                //SETEAMOS PARAMETROS DEL CLIENTE
               var client = new RestClient("https://rahue.sii.cl/recursos/v1/boleta.electronica.envio");// PRODUCCION
               //var client = new RestClient("https://pangal.sii.cl/recursos/v1/boleta.electronica.envio");// CERTIFICACION
                client.UserAgent = "Mozilla/4.0 ( compatible; PROG 1.0; Windows NT)";

                //SETEAMOS PARAMETROS DE LA REQUEST
                var request = new RestRequest(Method.POST);
                request.AlwaysMultipartFormData = true;
                request.AddHeader("Cookie", "TOKEN="+respuesta_token[1]);
                request.AddHeader("Accept", "application/json");
                request.AlwaysMultipartFormData = true;
                request.AddHeader("Content-Type", "multipart/form-data");
                request.AddHeader("Host", "rahue.sii.cl"); //PRODUCCION
               //request.AddHeader("Host", "pangal.sii.cl"); //CERTIFICACION
                
                //AGREGAMOS LOS PARAMETROS
                request.AddParameter("rutSender", pRutEmisor);
                request.AddParameter("dvSender", pDigEmisor);
                request.AddParameter("rutCompany", pRutEmpresa);
                request.AddParameter("dvCompany", pDigEmpresa);
                
                //AGREGAMOS EL ARCHIVO XML
                request.AddFile("archivo", ruta_archivo);

                //EJECUTAMOS LA REQUEST
                IRestResponse response = client.Execute(request);
                string respuesta_envio_boleta = response.Content;





                //CHEQUEAR SI LA RESPUESTA ES UN JSON, SI ES ASI
                //LA KEY "estado" == "REC"

                if (!string.IsNullOrEmpty(respuesta_envio_boleta))
                {
                   respuesta_envio_boleta = respuesta_envio_boleta.Trim();

                    if ((respuesta_envio_boleta.StartsWith("{") && respuesta_envio_boleta.EndsWith("}")) || (respuesta_envio_boleta.StartsWith("[") && respuesta_envio_boleta.EndsWith("]")))
                    {
                        try
                        {
                           // var respuesta_json_boleta = JToken.Parse(respuesta_envio_boleta);
                            dynamic respuesta_boleta_json = JsonConvert.DeserializeObject(respuesta_envio_boleta);
                            

                            if (respuesta_boleta_json["estado"] == "REC")
                            {
                                return respuesta_boleta_json["trackid"];
                            }
                            
                            return respuesta_boleta_json["estado"];
                        }
                        catch (Exception ex) //some other exception
                        {
                           
                            return ex.ToString();
                        }

                    }
                    return respuesta_envio_boleta;
                }else 
                { 
                 return respuesta_envio_boleta; 
                }


               

            }
            else
            {
                //TOKEN INVALIDO O ERROR
                return respuesta_token[1];
            }
        
             
           

            


           
        }

       public string[] obtenerToken()
        {
            string token = "";
            string[] respuesta_array = new string[2];
            //VERIFICAR SI EXISTE UN TOKEN ACTIVO EN LA BASE DE DATOS
            ConexionBD conexion = new ConexionBD();
            List<string> respuesta_token_activo = conexion.Select("SELECT digitos_token FROM token WHERE estado_token = 1 AND servidor_token = '"+Controllers.PruebaControlador.servidor_boletas+"'");

            if (respuesta_token_activo.Count == 1)
            {
                //EXISTE TOKEN ACTIVO
                respuesta_array[0] = "true";
                respuesta_array[1] = respuesta_token_activo[0];
                return respuesta_array;
            }
            else
            {
                //NO EXISTE TOKEN ACTIVO, SE PIDE UNO
                //OBTENER SEMILLA
                var client = new RestClient("https://api.sii.cl/recursos/v1/"); //PRODUCCION
                //var client = new RestClient("https://apicert.sii.cl/recursos/v1/"); //CERTIFICACION
                var request = new RestRequest("boleta.electronica.semilla", Method.GET);
                var response = client.Execute(request);

                string respuesta_xml_semilla = response.Content;

                XmlDocument XMLRespuestaSemilla = new XmlDocument();
                XMLRespuestaSemilla.LoadXml(respuesta_xml_semilla);

                XmlNodeList elemlist = XMLRespuestaSemilla.GetElementsByTagName("SEMILLA");
                XmlNodeList elemlist2 = XMLRespuestaSemilla.GetElementsByTagName("ESTADO");
                string semilla = elemlist[0].InnerXml;
                string estado_respuesta_semilla = elemlist2[0].InnerXml;

                if (estado_respuesta_semilla != "00")
                {
                    //SEMILLA INVALIDA
                    respuesta_array[0] = "false";
                    respuesta_array[1] = "Error al obtener semilla: " + respuesta_xml_semilla;
                    return respuesta_array;
                    
                }
                else
                {
                    //SEMILLA VALIDA, HAY QUE FIRMARLA PARA OBTENER TOKEN

                    string resultado = string.Empty;
                    string body = string.Format("<getToken><item><Semilla>{0}</Semilla></item></getToken>", semilla.ToString());

                    Certificado certificado = new Certificado();
                    string nombrecertificado = "RUBEN SALATIEL RIVERA GALLEGUILLOS";
                    string xmlparafirmar = XMLRespuestaSemilla.OuterXml;
                    X509Certificate2 certificado_digital = certificado.ObtenerCertificado(nombrecertificado);

                    //FIRMAR XML

                    string xml_semilla_firmada = firmarDocumentoSemilla(body, certificado_digital);


                    //ENVIARLA POR POST EL XML
                    var requestToken = new RestRequest("boleta.electronica.token", Method.POST);
                    requestToken.AddParameter("application/xml", xml_semilla_firmada, ParameterType.RequestBody);

                    IRestResponse response_token = client.Execute(requestToken);

                    string respuesta_xml_token = response_token.Content;

                    XmlDocument XMLRespuestaToken = new XmlDocument();
                    XMLRespuestaToken.LoadXml(respuesta_xml_token);

                    XmlNodeList elemlist3 = XMLRespuestaToken.GetElementsByTagName("TOKEN");
                    XmlNodeList elemlist4 = XMLRespuestaToken.GetElementsByTagName("ESTADO");
                    token = elemlist3[0].InnerXml;
                    string estado_respuesta_token = elemlist4[0].InnerXml;

                    if (estado_respuesta_token != "00")
                    {
                        //TOKEN INVALIDO
                        respuesta_array[0] = "false";
                        respuesta_array[1] = "Error al obtener token: " + respuesta_xml_token;
                        return respuesta_array;
                        
                    }
                    else
                    {
                        //TOKEN VALIDO, GUARDAR EN BD
                        //EL TOKEN AL MOMENTO DE SOLICITARLO TIENE UNA VALIDEZ DE 1 HORA, Y CADA VEZ QUE SE USA SE RENUEVA SU VALIDEZ

                        string guardar_token_query = "INSERT INTO token (digitos_token,fecha_solicitud_token,fecha_ultimo_uso_token,estado_token,servidor_token) VALUES ('" + token + "',NOW(),NOW(),1,'"+Controllers.PruebaControlador.servidor_boletas+"')";
                        conexion.Consulta(guardar_token_query);
                        respuesta_array[0] = "true";
                        respuesta_array[1] = token;
                        return respuesta_array;

                    }


                }

            }


        }

        public static string firmarDocumentoSemilla(string documento, X509Certificate2 certificado)
        {

            ////
            //// Cree un nuevo documento xml y defina sus caracteristicas
            XmlDocument doc = new XmlDocument();
            doc.PreserveWhitespace = false;
            doc.LoadXml(documento);

            // Create a SignedXml object.
            SignedXml signedXml = new SignedXml(doc);

            // Add the key to the SignedXml document.  'key'
            signedXml.SigningKey = certificado.PrivateKey;

            // Get the signature object from the SignedXml object.
            Signature XMLSignature = signedXml.Signature;

            // Create a reference to be signed.  Pass "" 
            // to specify that all of the current XML
            // document should be signed.
            Reference reference = new Reference("");

            // Add an enveloped transformation to the reference.
            XmlDsigEnvelopedSignatureTransform env = new XmlDsigEnvelopedSignatureTransform();
            reference.AddTransform(env);

            // Add the Reference object to the Signature object.
            XMLSignature.SignedInfo.AddReference(reference);

            // Add an RSAKeyValue KeyInfo (optional; helps recipient find key to validate).
            KeyInfo keyInfo = new KeyInfo();
            keyInfo.AddClause(new RSAKeyValue((RSA)certificado.PrivateKey));


            ////
            //// Agregar información del certificado x509
            //// X509Certificate MSCert = X509Certificate.CreateFromCertFile(Certificate);
            keyInfo.AddClause(new KeyInfoX509Data(certificado));


            // Add the KeyInfo object to the Reference object.
            XMLSignature.KeyInfo = keyInfo;

            // Compute the signature.
            signedXml.ComputeSignature();

            // Get the XML representation of the signature and save
            // it to an XmlElement object.
            XmlElement xmlDigitalSignature = signedXml.GetXml();

            // Append the element to the XML document.
            doc.DocumentElement.AppendChild(doc.ImportNode(xmlDigitalSignature, true));


            if (doc.FirstChild is XmlDeclaration)
            {
                doc.RemoveChild(doc.FirstChild);
            }

            // Save the signed XML document to a file specified
            // using the passed string.
            //XmlTextWriter xmltw = new XmlTextWriter(@"d:\ResultadoFirma.xml", new UTF8Encoding(false));
            //doc.WriteTo(xmltw);
            //xmltw.Close();
            return doc.InnerXml;

        }

        public string consultarEstadoEnvio(string trackID)
        {

           // WebRequest request = WebRequest.Create("http://localhost:81/WebServiceEnvioDTE/EnvioSobreDTE.asmx/consultarEstadoEnvio?Numero_Envio=" + trackID); //CERTIFICACION
            //WebRequest request = WebRequest.Create("http://localhost:90/WebServiceEnvioDTE/EnvioSobreDTE.asmx/consultarEstadoEnvio?Numero_Envio=" + trackID); 
            WebRequest request = WebRequest.Create("http://192.168.1.9:90/WebServiceEnvioDTE/EnvioSobreDTE.asmx/consultarEstadoEnvio?Numero_Envio=" + trackID); //PRODUCCION
            request.Method = "GET";
            WebResponse response = request.GetResponse();
            string respuestaEnvio = "";
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                respuestaEnvio = reader.ReadToEnd(); // do something fun...
            }
           
            respuestaEnvio = respuestaEnvio.Replace(@"<?xml version=""1.0"" encoding=""utf-8""?>", String.Empty);
            respuestaEnvio = respuestaEnvio.Replace(@"<string xmlns=""http://tempuri.org/"">", "");
            respuestaEnvio = respuestaEnvio.Replace(@"</string>", "");
            respuestaEnvio = respuestaEnvio.Replace("&lt;", "<");
            respuestaEnvio = respuestaEnvio.Replace("&gt;", ">");
            respuestaEnvio = respuestaEnvio.Replace("\n", String.Empty);
            respuestaEnvio = respuestaEnvio.Replace("\r", String.Empty);
            respuestaEnvio = respuestaEnvio.Replace("\t", String.Empty);
            return respuestaEnvio;
        }

        public  string updateEstadoSobre(string TrackId_str, string servidor)
        {
            if (TrackId_str != "")
            {


                try
                {
                    string datetime_str = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

                    ConexionBD conexion = new ConexionBD();
                    //CONSULTAR ESTADO DE ENVIO

                    string resultadoEnvio = "";
                    string estado_str = "";
                    string detalle_str = "";
                    XmlDocument xmlDoc = new XmlDocument();
                    dynamic respuesta_envio_estado_json;

                    if (servidor == "api" || servidor == "apicert")
                    {
                        //OBTENER TOKEN PARA CONSULTAR ESTADO DE ENVIO
                        string token = "";

                        //VERIFICAR SI EXISTE UN TOKEN ACTIVO EN LA BASE DE DATOS
                        List<string> respuesta_token_activo = conexion.Select("SELECT digitos_token FROM token WHERE estado_token = 1 AND servidor_token = '" + servidor + "'");
                        if (respuesta_token_activo.Count == 0)
                        {
                            //NO EXISTE TOKEN ACTIVO DE BOLETAS
                             return "NO EXISTE TOKEN ACTIVO DE BOLETAS";
                        }
                        else
                        {
                            //EXISTE TOKEN VALIDO DE BOLETAS
                            token = respuesta_token_activo[0];
                            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                            string host_servidor = "";
                                host_servidor = "https://api.sii.cl/recursos/v1/boleta.electronica.envio/76958430-7-";

                                //host_servidor = "https://apicert.sii.cl/recursos/v1/boleta.electronica.envio/76958430-7-"; //CERTIFICACION
                            
                            var client = new RestClient(host_servidor + TrackId_str);
                            client.Timeout = -1;
                            var request = new RestRequest(Method.GET);
                            request.AddCookie("Cookie", "TOKEN=" + token);
                            request.AlwaysMultipartFormData = true;
                            IRestResponse response = client.Execute(request);

                            resultadoEnvio = response.Content;

                            if ((resultadoEnvio.StartsWith("{") && resultadoEnvio.EndsWith("}")) || (resultadoEnvio.StartsWith("[") && resultadoEnvio.EndsWith("]")))
                            {
                                //ES UN JSON
                                try
                                {
                                    respuesta_envio_estado_json = JsonConvert.DeserializeObject(resultadoEnvio);
                                    estado_str = respuesta_envio_estado_json["estado"];

                                    //RESCATAR EL CAMPO "detalle_rep_rech" Y GUARDARLO EN LA BD EN UNA COLUMNA NUEVA
                                    detalle_str = resultadoEnvio;



                                }
                                catch (Exception ex)
                                {
                                   return "Hubo un error, respuesta del servidor del SII:" + resultadoEnvio;

                                }

                            }
                            else
                            {
                                return "Hubo un error, respuesta del servidor del SII:" + resultadoEnvio;
                            }


                        }

                    }

                    if (servidor == "maullin" || servidor == "palena")
                    {
                        resultadoEnvio = consultarEstadoEnvio(TrackId_str);
                        //string resultadoEnvio es la respuesta en XML


                        xmlDoc.LoadXml(resultadoEnvio);
                        XmlNodeList estado = xmlDoc.GetElementsByTagName("ESTADO");
                        XmlNodeList respuesta_completa = xmlDoc.GetElementsByTagName("SII:RESPUESTA");



                        if (estado.Count == 0)
                        {
                            return "Hubo un error, respuesta del servidor del SII:" + resultadoEnvio;
                        }
                        else
                        {
                            try
                            {
                                estado_str = estado[0].InnerXml;
                                detalle_str = respuesta_completa[0].InnerXml;

                            }
                            catch (Exception e)
                            {
                              return "Hubo un error, respuesta del servidor del SII:" + resultadoEnvio;
                            }
                        }
                    }


                    //RESPUESTA DEFAULT
                    if (estado_str != "EPR")
                    {
                        string respuesta = "Hubo un error, respuesta del servidor del SII: " + resultadoEnvio + " Track ID : " + TrackId_str;
                        string queryUpdateEstado = "UPDATE envio_dte SET  estado_envio_dte = 'Enviado',  revision_envio_dte = '0', detalle_envio_dte = '" + detalle_str + "'  WHERE trackid_envio_dte = '" + TrackId_str + "';";
                        conexion.Consulta(queryUpdateEstado);
                        return respuesta;

                    }



                    if (estado_str == "-11")
                    {
                        //MANDAR MENSAJE ERROR DE REINTENTOS DE FOLIO REPETIDO
                        string respuesta = "Error: Sobre de envio trackID: " + TrackId_str + ", aun no procesado o con error en SII";
                        conexion.Consulta("INSERT INTO log_event (id_log_event, mensaje_log_event, fecha_log_event, referencia_log_event, query_request_log_event) VALUES (NULL, '" + respuesta + "', NOW(), 'TrackID: " + TrackId_str + " Aplicacion de Servicio ASConsultaEstadoEnvio', '') ");
                        //mandarEmailSobre(TrackId_str, "Error", detalle_str);

                        string queryUpdateEstado = "UPDATE envio_dte SET  estado_envio_dte = 'Enviado',  revision_envio_dte = '0', detalle_envio_dte = '" + detalle_str + "'  WHERE trackid_envio_dte = '" + TrackId_str + "';";
                        conexion.Consulta(queryUpdateEstado);

                        return respuesta;

                    }


                    string informados_str = "";
                    string aceptados_str = "";
                    string rechazados_str = "";
                    string reparos_str = "";
                    string consumo_folio_str = "";

                    if (estado_str == "RCT")
                    {
                        //UPDATE EL SOBRE RECHAZADO CON EL ESTADO "RECHAZADO"
                        string queryUpdateEstado = "UPDATE envio_dte SET  estado_envio_dte = 'Enviado', rechazos_envio_dte = '1', informados_envio_dte = '1', revision_envio_dte = '1', detalle_envio_dte = '" + detalle_str + "'  WHERE trackid_envio_dte = '" + TrackId_str + "';";
                        conexion.Consulta(queryUpdateEstado);
                    }

                    if (estado_str == "EPR")
                    {
                        //ENVIO PROCESADO
                        if (servidor == "api" || servidor == "apicert")
                        {
                            respuesta_envio_estado_json = JsonConvert.DeserializeObject(resultadoEnvio);
                            var estadisticas_array = JsonConvert.DeserializeObject(respuesta_envio_estado_json["estadistica"].ToString());
                            string estadisticas_str_json = estadisticas_array[0].ToString();
                            dynamic estadisticas_json = JsonConvert.DeserializeObject(estadisticas_str_json);

                            informados_str = estadisticas_json["informados"].ToString();
                            aceptados_str = estadisticas_json["aceptados"].ToString();
                            rechazados_str = estadisticas_json["rechazados"].ToString();
                            reparos_str = estadisticas_json["reparos"].ToString();

                        }

                        if (servidor == "maullin" || servidor == "palena")
                        {
                            //RESCATAMOS LOS VALORES DE LA RESPUESTA EN XML

                            XmlNodeList informados = xmlDoc.GetElementsByTagName("INFORMADOS");
                            XmlNodeList aceptados = xmlDoc.GetElementsByTagName("ACEPTADOS");
                            XmlNodeList rechazados = xmlDoc.GetElementsByTagName("RECHAZADOS");
                            XmlNodeList reparos = xmlDoc.GetElementsByTagName("REPAROS");

                            if (informados.Count == 0 && aceptados.Count == 0 && rechazados.Count == 0 && reparos.Count == 0)
                            {
                                //QUIERE DECIR QUE ES UNA RESPUESTA DE CONSUMO FOLIOS
                                consumo_folio_str = "1";
                                informados_str = "0";
                                aceptados_str = "0";
                                rechazados_str = "0";
                                reparos_str = "0";
                            }
                            else
                            {
                                informados_str = informados[0].InnerXml;
                                aceptados_str = aceptados[0].InnerXml;
                                rechazados_str = rechazados[0].InnerXml;
                                reparos_str = reparos[0].InnerXml;
                                consumo_folio_str = "0";
                            }



                        }


                        //SI HAY UN RECHAZADO MANDAR MENSAJE DE ERROR
                        if (rechazados_str != "0") //Si es distinto a 0 es porque hay rechazo
                        {

                            //UPDATE EL SOBRE RECHAZADO CON EL ESTADO "RECHAZADO"
                            string queryUpdateEstado = "UPDATE envio_dte SET  estado_envio_dte = 'Enviado', rechazos_envio_dte = '1', informados_envio_dte = '1', revision_envio_dte = '1', detalle_envio_dte = '" + detalle_str + "'  WHERE trackid_envio_dte = '" + TrackId_str + "';";
                            conexion.Consulta(queryUpdateEstado);

                            //CREAMOS LA RESPUESTA DEL RECHAZO
                            string respuesta = "Error: Sobre de envio trackID: " + TrackId_str + ", fue rechazado";
                            conexion.Consulta("INSERT INTO log_event (id_log_event, mensaje_log_event, fecha_log_event, referencia_log_event, query_request_log_event) VALUES (NULL, '" + respuesta + "', NOW(), 'TrackID: " + TrackId_str + " Aplicacion de Servicio ASConsultaEstadoEnvio', '') ");
                            //mandarEmailSobre(TrackId_str, "Rechazo DTE", detalle_str);
                            return respuesta;
                        }

                        //SI HAY UN REPARO, INFORMAR
                        if (reparos_str != "0") //Si es distinto a 0 es porque hay reparo
                        {
                            //UPDATE EL SOBRE RECHAZADO CON EL ESTADO "RECHAZADO"
                            string queryUpdateEstado = "UPDATE envio_dte SET  estado_envio_dte = 'Enviado', reparos_envio_dte = '1', informados_envio_dte = '1', revision_envio_dte = '1', detalle_envio_dte = '" + detalle_str + "'   WHERE trackid_envio_dte = '" + TrackId_str + "';";
                            conexion.Consulta(queryUpdateEstado);

                            //CREAMOS LA RESPUESTA DEL RECHAZO
                            string respuesta = "Advertencia: Sobre de envio trackID: " + TrackId_str + ", fue enviado con reparo";
                            conexion.Consulta("INSERT INTO log_event (id_log_event, mensaje_log_event, fecha_log_event, referencia_log_event, query_request_log_event) VALUES (NULL, '" + respuesta + "', NOW(), 'TrackID: " + TrackId_str + " Aplicacion de Servicio ASConsultaEstadoEnvio', '') ");
                            Console.ForegroundColor = ConsoleColor.White;
                            //mandarEmailSobre(TrackId_str, "Reparo DTE", detalle_str);
                            return respuesta;
                        }

                        if (aceptados_str != "0")
                        {
                            //XML ACEPTADO MANDAR MENSAJE DE EXITO
                            //UPDATE EL SOBRE RECHAZADO CON EL ESTADO "RECHAZADO"
                            string queryUpdateEstado = "UPDATE envio_dte SET  estado_envio_dte = 'Enviado', aceptados_envio_dte = '1', informados_envio_dte = '1', revision_envio_dte = '1',detalle_envio_dte = '" + detalle_str + "'  WHERE trackid_envio_dte = '" + TrackId_str + "';";
                            conexion.Consulta(queryUpdateEstado);
                            string respuesta = "Aceptado: Sobre de envio trackID: " + TrackId_str + "";
                            return respuesta;


                        }

                        if (consumo_folio_str != "0")
                        {
                            string queryUpdateEstado = "UPDATE envio_dte SET  estado_envio_dte = 'Enviado', aceptados_envio_dte = '1', informados_envio_dte = '1',envio_cliente_envio_dte = '2', revision_envio_dte = '1',detalle_envio_dte = '" + detalle_str + "'  WHERE trackid_envio_dte = '" + TrackId_str + "';";
                            conexion.Consulta(queryUpdateEstado);
                        }
                    }
                }
                catch (Exception ex)
                {
                   string respuesta = "HUBO ERROR EN UpdateEstadoSobre() :" + ex.Message;
                    return respuesta;

                }
            }
            else
            {
                string respuesta = "HUBO ERROR CON EL TRACKID UpdateEstadoSobre() :" + TrackId_str;
            }

            return "NO ES UN TRACK ID VALIDO";
        }




    }
}