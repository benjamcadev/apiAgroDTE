using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace ApiAgroDTE.Clases
{
    public class AcuseRecibo
    {
        public string crearAcuseRecibo(string respuestaSobre,string[] respuestaDte, string PathDTE, string strRutEmpresa,string prefixPathXML)
        {
            try
            {
                string fecha = DateTime.Now.Date.ToString("yyyy/MM/dd");
                string hora = DateTime.Now.ToString("HH:mm:ss");
                string strCodEnvio = "SELECT MAX(id_acuse_recibo) FROM acuse_recibo";
                string strDatosEmpresa = "SELECT rut_empresa, nombre_contacto_empresa, fono_contacto_empresa, mail_contacto_empresa FROM empresa WHERE rut_empresa = '" + strRutEmpresa + "'";

                ConexionBD conexion = new ConexionBD();

                string fechaHora = fecha + "T" + hora;
                //Depende de la respuesta del chema tenemos que setear el estado de la recepcion del DTE            
                int estadoRecepEnv = 0;
                string glosaEstadoRecep = "Envío Recibido Conforme";
                int estadoRecepDTE = 0;
                string glosaRecepDTE = "DTE Recibido OK.";

                //List<string> listPathDteCompra = conexion.Select(strPathDteCompra);

                List<string> listRutEmpresa = conexion.Select(strDatosEmpresa);
                List<string> listEnvioDteId = conexion.Select(strCodEnvio);
                int intCodEnvioNuevo = int.Parse(listEnvioDteId[0]) + 1;

                string fileNameXml = Path.GetFileName(PathDTE);
                fileNameXml = fileNameXml.Replace(prefixPathXML,"");
                //string temp = @"C:\Users\Marcelo Riquelme\source\repos\ConsoleApp1\EnvioDTE.xml";

                //------------------------------------------------------------------------------------------------------------------
                //EXTRAER DATOS XML------------------------------<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
                //------------------------------------------------------------------------------------------------------------------

                XmlDocument documentoXml = new XmlDocument();
                //XmlElement root = documentoXml.DocumentElement;
                //documentoXml.Load(listPathDteCompra[0]);
                try
                {
                    documentoXml.Load(PathDTE);
                }
                catch (Exception ex)
                {

                    string respuestaCargaAcuseRecibo = "Error, no se pudo cargar el XML para generar el Acuse : " + ex.ToString();
                    enviarEmailAcuseRecibo("","",respuestaCargaAcuseRecibo);
                    return respuestaCargaAcuseRecibo;

                }
                XmlNodeList digestList = documentoXml.GetElementsByTagName("DigestValue");
                XmlNodeList rutEnviaList = documentoXml.GetElementsByTagName("RutEnvia");
                XmlNodeList rutEmisorList = documentoXml.GetElementsByTagName("RutEmisor");// emisor de la factura sobre

                XmlNodeList tipoDTEList = documentoXml.GetElementsByTagName("TipoDTE");
                XmlNodeList folioList = documentoXml.GetElementsByTagName("Folio");
                XmlNodeList fchEmisList = documentoXml.GetElementsByTagName("FchEmis");
                XmlNodeList rutRecepList = documentoXml.GetElementsByTagName("RUTRecep");
                XmlNodeList rutEmisor2List = documentoXml.GetElementsByTagName("RUTEmisor");// rut emiso factura dte
                XmlNodeList mntTotalList = documentoXml.GetElementsByTagName("MntTotal");

                List<string> listGlosaRecepDTE = new List<string>();
                List<int> listEstadoRecepDTE = new List<int>();

                int countDTE = documentoXml.GetElementsByTagName("DTE").Count;
                int countDigest = documentoXml.GetElementsByTagName("DigestValue").Count;

                var setDTE = documentoXml.GetElementsByTagName("SetDTE");

                if(setDTE.Count == 0){
                    return "Error - No se encuentra la etiqueta SetDTE"; 

                }
                if(digestList.Count == 0 || rutEnviaList.Count == 0 || rutEmisorList.Count == 0 || tipoDTEList.Count == 0 || folioList.Count == 0 || fchEmisList.Count == 0 || rutRecepList.Count == 0 || rutEmisor2List.Count == 0 || mntTotalList.Count == 0 || setDTE.Count == 0){
                    enviarEmailAcuseRecibo("","","Error: no se pueden obtener uno o más datos del archivo xml para generar la respuesta, Prefijo: "+prefixPathXML+" Path: "+PathDTE+ " Respuesta sobre: "+respuestaSobre+ " Respuesta dte: "+respuestaDte);
                    return "Error: no se pueden obtener uno o más datos del archivo xml";
                }

                string setDTEId = setDTE[0].Attributes[0].Value;

                if (respuestaSobre == "RecepcionEnvio-Conforme")
                {
                    glosaEstadoRecep = "Envío Recibido Conforme";
                    estadoRecepEnv = 0;
                }
                if (respuestaSobre == "RecepcionEnvio-Schema")
                {
                    glosaEstadoRecep = " Envío Rechazado – Error de Schema";
                    estadoRecepEnv = 1;
                }
                if (respuestaSobre == "RecepcionEnvio-Firma")
                {
                    glosaEstadoRecep = " Envío Rechazado - Error de Firma";
                    estadoRecepEnv = 2;
                }
                if (respuestaSobre == "RecepcionEnvio-Rut")
                {
                    glosaEstadoRecep = "Envío Rechazado - RUT Receptor No Corresponde";
                    estadoRecepEnv = 3;
                }
                if (respuestaSobre == "RecepcionEnvio-Repetido")
                {
                    glosaEstadoRecep = "Envío Rechazado - Archivo Repetido";
                    estadoRecepEnv = 90;
                }
                if (respuestaSobre == "RecepcionEnvio-Ilegible")
                {
                    glosaEstadoRecep = "Envío Rechazado - Archivo Ilegible";
                    estadoRecepEnv = 91;
                }
                if (respuestaSobre == "RecepcionEnvio-Otro")
                {
                    glosaEstadoRecep = "Envío Rechazado – Otro";
                    estadoRecepEnv = 99;
                }
                //-----------------------------------------------------------------------------------------------------------------------
                for (int i = 0; i < respuestaDte.Length; i++)
                {

                    if (respuestaDte[i] == "RecepcionDTE-OK")
                    {
                        glosaRecepDTE = "DTE Recibido OK";
                        estadoRecepDTE = 0;                  

                    }
                    if (respuestaDte[i] == "RecepcionDTE-Firma")
                    {
                        glosaRecepDTE = "DTE No Recibido - Error de Firma";
                        estadoRecepDTE = 1;
                    }
                    if (respuestaDte[i] == "RecepcionDTE-Rut Emisor")
                    {
                        glosaRecepDTE = "DTE No Recibido - Error en RUT Emisor";
                        estadoRecepDTE = 2;
                    }
                    if (respuestaDte[i] == "RecepcionDTE-Rut Receptor")
                    {
                        glosaRecepDTE = "DTE No Recibido - Error en RUT Receptor";
                        estadoRecepDTE = 3;
                    }
                    if (respuestaDte[i] == "RecepcionDTE-Repetido")
                    {
                        glosaRecepDTE = "DTE No Recibido - DTE Repetido";
                        estadoRecepDTE = 4;
                    }
                    if (respuestaDte[i] == "RecepcionDTE-Otro")
                    {
                        glosaRecepDTE = "DTE No Recibido - Otros";
                        estadoRecepDTE = 99;
                    }

                    listGlosaRecepDTE.Add(glosaRecepDTE);
                    listEstadoRecepDTE.Add(estadoRecepDTE);

                }
               

                //------------------------------------------------------------------------------------------------------------------
                //CONSTRUIR XML------------------------------<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
                //------------------------------------------------------------------------------------------------------------------

                XmlTextWriter writer;
                XNamespace xmlns = XNamespace.Get("http://www.sii.cl/SiiDte");

                string directorioAcuseReciboXml = @"C:\inetpub\wwwroot\api_agrodte\AgroDTE_Archivos\AcuseRecibo\" + rutEnviaList[0].InnerXml + "-" + intCodEnvioNuevo.ToString() + ".xml";
                writer = new XmlTextWriter(directorioAcuseReciboXml, Encoding.GetEncoding("ISO-8859-1"));


                //ENCABEZADO----------------------------------<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

                writer.Formatting = System.Xml.Formatting.Indented;
                writer.Indentation = 0;
                writer.WriteStartDocument();

                writer.WriteStartElement("RecepcionEnvio");
                writer.WriteElementString("NmbEnvio", fileNameXml); // NOMBRE DEL ARCHIVO
                writer.WriteElementString("FchRecep", fechaHora); // FECHA Y HORA DE RECEPCION
                writer.WriteElementString("CodEnvio", intCodEnvioNuevo.ToString());
                writer.WriteElementString("EnvioDTEID", setDTEId);
                writer.WriteElementString("Digest", digestList[countDigest - 1].InnerXml); // SE EXTRAE DEL SOBRE
                writer.WriteElementString("RutEmisor", rutEmisorList[0].InnerXml); // EMISOR DEL DTE QUE ESTAMOS RESPONDIENDO (A QUIEN VA DIRIGIDA LA RESPUESTA)
                writer.WriteElementString("RutReceptor", strRutEmpresa); // rut de nosotros empresa
                writer.WriteElementString("EstadoRecepEnv", estadoRecepEnv.ToString());//(0-1-2-3-90-91-99)
                writer.WriteElementString("RecepEnvGlosa", glosaEstadoRecep);//Envío Recibido Conforme – Error de Schema - Error de Firma - RUT Receptor No Corresponde - Archivo Repetido - Archivo Ilegible – Otros

                if (estadoRecepEnv == 0)
                {
                    writer.WriteElementString("NroDTE", countDTE.ToString());

                    for (int i = 0; i < countDTE; i++)
                    {
                        writer.WriteStartElement("RecepcionDTE");
                        writer.WriteElementString("TipoDTE", tipoDTEList[i].InnerXml);
                        writer.WriteElementString("Folio", folioList[i].InnerXml);
                        writer.WriteElementString("FchEmis", fchEmisList[i].InnerXml);
                        writer.WriteElementString("RUTEmisor", rutEmisor2List[i].InnerXml);
                        writer.WriteElementString("RUTRecep", rutRecepList[i].InnerXml);// rut responde
                        writer.WriteElementString("MntTotal", mntTotalList[i].InnerXml);
                        writer.WriteElementString("EstadoRecepDTE", listEstadoRecepDTE[i].ToString());
                        writer.WriteElementString("RecepDTEGlosa", listGlosaRecepDTE[i]);
                        writer.WriteEndElement();
                    }
                }


                writer.WriteEndElement();
                writer.Flush();
                writer.Close();

                //------------------------------------------------------------------------------------------------------------------
                // GUARDAR EN LA BASE DE DATOS------------------------------<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
                //------------------------------------------------------------------------------------------------------------------

                string nombreTabla = "";
                string nombreColumnaFolio = "";
                string queryUpdateDTECompra = "";
                string queryInsertAcuseReciboDTE = "";
                string queryInsertAcuseRecibo = "INSERT INTO acuse_recibo (id_acuse_recibo, fecha_acuse_recibo, hora_acuese_recibo, rut_receptor_acuse_recibo, estado_recep_env_acuse_recivo, recep_env_glosa_acuse_recibo)" +
                                                "VALUES ('" + intCodEnvioNuevo + "', '" + fecha + "', '" + hora + "', '" + rutEnviaList[0].InnerXml + "', '" + estadoRecepEnv + "', '" + glosaEstadoRecep + "')";
                conexion.Consulta(queryInsertAcuseRecibo);

                for (int j = 0; j < countDTE; j++)
                {

                    if (tipoDTEList[j].InnerXml == "33")
                    {
                        nombreTabla = "factura_compra";
                        nombreColumnaFolio = "folio_factura_compra";

                    }
                    if (tipoDTEList[j].InnerXml == "34")
                    {
                        nombreTabla = "factura_exenta_compra";
                        nombreColumnaFolio = "folio_factura_exenta_compra";

                    }
                    if (tipoDTEList[j].InnerXml == "52")
                    {
                        nombreTabla = "guia_despacho_compra";
                        nombreColumnaFolio = "folio_guia_despacho_compra";

                    }
                    if (tipoDTEList[j].InnerXml == "61")
                    {
                        nombreTabla = "nota_credito_compra";
                        nombreColumnaFolio = "folio_nota_credito_compra";

                    }
                    if (tipoDTEList[j].InnerXml == "56")
                    {
                        nombreTabla = "nota_debito_compra";
                        nombreColumnaFolio = "folio_nota_debito_compra";

                    }

                    queryUpdateDTECompra = "UPDATE " + nombreTabla + " SET id_acuse_recibo_fk = '" + intCodEnvioNuevo + "' WHERE " + nombreColumnaFolio + " = '" + folioList[j].InnerXml + "'";

                    queryInsertAcuseReciboDTE = "INSERT INTO acuse_recibo_dte (id_acuse_recibo_fk, tipodte_acuse_recibo_dte, folio_acuse_recibo_dte, estado_recep_acuse_recibo_dte, glosa_estado_acuse_recibo_dte)" +
                                                "VALUES ('" + intCodEnvioNuevo + "', '" + tipoDTEList[j].InnerXml + "', '" + folioList[j].InnerXml + "', '" + estadoRecepDTE + "', '" + glosaRecepDTE + "')";

                    conexion.Consulta(queryUpdateDTECompra);
                    conexion.Consulta(queryInsertAcuseReciboDTE);

                }
                

                string[] respuestaCrearAcuseRecibo = new string[] { directorioAcuseReciboXml, intCodEnvioNuevo.ToString(), rutEmisorList[0].InnerXml,strRutEmpresa};

                crearSobreAcuseRecibo(respuestaCrearAcuseRecibo, fecha, hora, listRutEmpresa);
                
            }catch(Exception ex)
            {
                return ex.Message;
              
            }

            return "ok";

        }
        
        public string crearSobreAcuseRecibo(string[] respuestaCrearAcuseRecibo,string fecha, string hora, List<string> listRutEmpresa)
        {
            Schemas schemas = new Schemas(); 
            
            string strReturnSchema = "";
            string rutaXml = respuestaCrearAcuseRecibo[0];
            string strCodEnvio = respuestaCrearAcuseRecibo[1];
            string strRutRecibe = respuestaCrearAcuseRecibo[2];
            string strRutResponde = respuestaCrearAcuseRecibo[3];
            string fechahhmm = fecha + "T" + hora;
            string fecresolucion3 = System.DateTime.Now.ToString("yyyy-MM-dd");
            XNamespace xmlns = XNamespace.Get("http://www.sii.cl/SiiDte");
            XNamespace xsi = XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance");              

            XNamespace schemaLocation = XNamespace.Get("http://www.sii.cl/SiiDte RespuestaEnvioDTE_v10.xsd");
            
            string fileNameXML = "EnvioRespuesta" + strRutRecibe + "-" + strCodEnvio + ".xml";
            string EnvioAcuseDTE_xml = @"C:\inetpub\wwwroot\api_agrodte\AgroDTE_Archivos\AcuseRecibo\" + fileNameXML;
            //string path = Path.Combine(Environment.CurrentDirectory, directorioFechaActual,fileNameXML);
            //string EnvioDTE_xml =@"C:\Users\Benjamin\source\repos\ApiAgroDTE\ApiAgroDTE\XML\SobreEnvio.xml";

            //string archxml = @"\\192.168.1.4\api_agrodte\XML\C#\SobreEnvio.xml";

            //Llamar datos de empresa de la base de datos

            ConexionBD conexion = new ConexionBD();
            List<string> resultDatosempresa = conexion.Select("SELECT rut_empresa,rut_rrll,DATE_FORMAT(fecha_res, '%Y-%m-%d') AS fecha ,numero_res FROM empresa WHERE id_empresa=1");

            //RUT DEL SII
            string RutReceptor = "60803000-K";

            XElement setdte = new XElement("Caratula",
                new XAttribute("version", "1.0"),
                new XElement("RutResponde", strRutResponde),
                new XElement("RutRecibe", strRutRecibe),
                new XElement("IdRespuesta", strCodEnvio),
                new XElement("NroDetalles", "1"),
                new XElement("NmbContacto", listRutEmpresa[1]),
                new XElement("FonoContacto", listRutEmpresa[2]),
                new XElement("MailContacto", listRutEmpresa[3]),
                new XElement("TmstFirmaResp", fechahhmm));

            string etiquetaEnvio = "RespuestaDTE";

            XDocument miXML = new XDocument(new XDeclaration("1.0", "ISO-8859-1", ""),
                new XElement(etiquetaEnvio,
                new XElement("Resultado",
                new XAttribute("ID", "EnvioRespuesta-" + strRutRecibe + "-" + strCodEnvio),
                setdte)));

            //// Quite la identacion del documento
            string[] strLineasDte = miXML.ToString().Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i <= strLineasDte.Length - 1; i++)
                strLineasDte[i] = strLineasDte[i].TrimStart();

            //// Recargue el documento xml sin identación
            //// utilizando el objeto xmlDocument
            string p = string.Join("\r\n", strLineasDte);
            miXML = XDocument.Parse(p, LoadOptions.PreserveWhitespace);
            miXML.Declaration = new XDeclaration("1.0", "ISO-8859-1", null);
            miXML.Save(EnvioAcuseDTE_xml);


            XmlDocument oDoc2 = new XmlDocument();
            oDoc2.PreserveWhitespace = true;
            oDoc2.Load(EnvioAcuseDTE_xml);
            oDoc2.InnerXml = oDoc2.InnerXml.Replace("<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>", "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>");
            oDoc2.Save(EnvioAcuseDTE_xml);

            //Cargar el XML DTE en el sobre
            XmlDocument xml_Dte = new XmlDocument();
            xml_Dte.PreserveWhitespace = true;

            xml_Dte.Load(rutaXml);


            oDoc2.SelectSingleNode(etiquetaEnvio + "/Resultado").AppendChild(oDoc2.ImportNode(xml_Dte.DocumentElement, true));
            oDoc2.PreserveWhitespace = true;
            oDoc2.Save(EnvioAcuseDTE_xml);

            // firma ENVIODTE--------------------------------------------------------------------------------------------------------------------

            oDoc2.DocumentElement.SetAttribute("xmlns", "http://www.sii.cl/SiiDte");
            oDoc2.DocumentElement.SetAttribute("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
            oDoc2.DocumentElement.SetAttribute("schemaLocation", "http://www.w3.org/2001/XMLSchema-instance", "http://www.sii.cl/SiiDte RespuestaEnvioDTE_v10.xsd");
            oDoc2.DocumentElement.SetAttribute("version", "1.0");
            oDoc2.PreserveWhitespace = true;
            oDoc2.Save(EnvioAcuseDTE_xml);

            StreamReader xml_firmar = new StreamReader(EnvioAcuseDTE_xml, Encoding.GetEncoding("ISO-8859-1"));
            string xml_firmar_setdte = xml_firmar.ReadToEnd();
            xml_firmar.Close();

            string nombrecertificado = "RUBEN SALATIEL RIVERA GALLEGUILLOS";
            Certificado certificado = new Certificado();
            string firmado = certificado.Firmar(nombrecertificado, xml_firmar_setdte,"#EnvioRespuesta-" + strRutRecibe + "-" + strCodEnvio);
            XDocument xml_firmado = XDocument.Parse(firmado, LoadOptions.PreserveWhitespace);
            xml_firmado.Save(EnvioAcuseDTE_xml);

            XmlDocument oDoc3 = new XmlDocument();
            oDoc3.PreserveWhitespace = true;
            oDoc3.Load(EnvioAcuseDTE_xml);
            oDoc3.InnerXml = oDoc3.InnerXml.Replace("<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>", "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>");
            oDoc3.Save(EnvioAcuseDTE_xml);

            string respuestaSchema = schemas.validarRespuestaXML(EnvioAcuseDTE_xml);

            if(respuestaSchema == "XML Valido"){

                string queryUpdateidSobre = "UPDATE acuse_recibo SET ubicacion_sobre_acuse_recibo = '" + EnvioAcuseDTE_xml + "' WHERE id_acuse_recibo = '" + strCodEnvio + "'";
                queryUpdateidSobre = queryUpdateidSobre.Replace("\\","\\\\");
                conexion.Consulta(queryUpdateidSobre);

                strReturnSchema = "Ok";

                enviarEmailAcuseRecibo(EnvioAcuseDTE_xml,strRutRecibe,"");

            }else{
                strReturnSchema = "Error schema Acuse Recibo";
            }

            
            //enviarEmailAcuseRecibo(directorioAcuseReciboXml, rutEnviaList[0].InnerXml);
            return strReturnSchema;
        }
        
        public string enviarEmailAcuseRecibo( string EnvioAcuseDTE_xml, string rutDestino, string mensaje)
        {
            //
            // ESTA FUNCION, RECIBE EL PATH EL RUT A QUIEN VA DIRIGIDO EL XM (A SEA ACUSE DE RECIBO O EL DTE PARA ENVIAR AL CLIENTE) Y UN MENSAJE, QUE PUEDE SER VACIO, ERROR, O XML CLIENTE() INDICANDO QUE LO QUE VIENE ES UN DTE PARA ENVIAR AL CLIENTE.
            //ConexionBD conexion = new ConexionBD();
            ConexionBD_2 conexion2 = new ConexionBD_2();
            string query_correoDestino = "SELECT correo FROM contribuyentes_correo WHERE rut='"+ rutDestino + "'";
            string asunto = "Acuse recibo DTE";
            string strCorreoDestino="";
            string mensajeCorreo = "";

// SI mensaje VIENE VACIO, SE BUSCA EL CORREO DEL CLIENTE Y SE LE ENVIA ACUSE DE RECIBO
            if(mensaje == "")
            {
                List<string> listCorreoDestino = conexion2.Select(query_correoDestino);
                if(listCorreoDestino.Count != 0){
                    strCorreoDestino = listCorreoDestino[0];
                }
                else
                {   
                    mensajeCorreo = "No se encontró el correo para enviar Acuse de recibo correspondiente, rut de destino: "+rutDestino;
                    //SI NO HAY CORREO ENVIAMOS A UNO POR DEFECTO
                    strCorreoDestino = "agrodte@agroplastic.cl";
                    //strCorreoDestino = "mriquelme@agroplastic.cl";


                }
               
            }
            // SI EL STRING mensaje DICE QUE ES XML CLIENTE, SE CAMBIA EL ASUNTO
            else if(mensaje == "XML Cliente")
            {
                asunto = "Entrega XML DTE Agroplastic";

                List<string> listCorreoDestino = conexion2.Select(query_correoDestino);
                if (listCorreoDestino.Count != 0)
                {
                    strCorreoDestino = listCorreoDestino[0];
                }
                else
                {
                    mensajeCorreo = "No se encontró el correo para enviar Copia cliente correspondiente, rut de destino: " + rutDestino;
                    //SI NO HAY CORREO ENVIAMOS A UNO POR DEFECTO
                    strCorreoDestino = "agrodte@agroplastic.cl";
                    //strCorreoDestino = "mriquelme@agroplastic.cl";


                }
            }
            else
            {
                //strCorreoDestino = "mriquelme@agroplastic.cl";
                mensajeCorreo = mensaje;

            }

            //strCorreoDestino = "mriquelme@agroplastic.cl"; // comentar para produccion     
            SmtpClient mySmtpClient = new SmtpClient("mail.agroplastic.cl");
            try
            {
                //TRAER EL CORREO Y PASS DESDE LA BD

                ConexionBD conexion = new ConexionBD();
                List<string> respuesta_correo = new List<string>();
                respuesta_correo = conexion.Select("SELECT mail_intercambio_empresa,pass_intercambio_empresa FROM empresa WHERE id_empresa = 1");

               

                // set smtp-client with basicAuthentication
                mySmtpClient.UseDefaultCredentials = false;
                mySmtpClient.EnableSsl = true;
                System.Net.NetworkCredential basicAuthenticationInfo = new System.Net.NetworkCredential(respuesta_correo[0], respuesta_correo[1]);
                mySmtpClient.Credentials = basicAuthenticationInfo;

                // add from,to mailaddresses
                MailAddress from = new MailAddress(respuesta_correo[0], "AgroDTE");
                //MailAddress to = new MailAddress("bmcortes@agroplastic.cl, mriquelme@agroplastic.cl", "Benjamin");
                MailMessage myMail = new MailMessage();
                myMail.To.Add(strCorreoDestino);
                //myMail.To.Add("bmcortes@agroplastic.cl");
               
                myMail.From = from;

                if(mensaje == "" || mensaje == "XML Cliente"){
                    myMail.Attachments.Add(new Attachment(EnvioAcuseDTE_xml));               }                 



                // add ReplyTo
                //MailAddress replyTo = new MailAddress("mriquelme@agroplastic.cl");
                //myMail.ReplyToList.Add(replyTo);

                // set subject and encoding
                myMail.Subject = asunto;
                myMail.SubjectEncoding = System.Text.Encoding.UTF8;

                // set body-message and encoding

                myMail.Body = mensajeCorreo;
                myMail.BodyEncoding = System.Text.Encoding.UTF8;
                // text or html
                myMail.IsBodyHtml = true;

                mySmtpClient.Send(myMail);

         
                mySmtpClient.Dispose();
            }

            catch (SmtpException ex)
            {
                mySmtpClient.Dispose();
                return "SmtpException has occured: " + ex.Message;
            }
            catch (Exception ex)
            {
                mySmtpClient.Dispose();
                return ex.Message;
            }



            return "ok";

            
        }
    }
}
