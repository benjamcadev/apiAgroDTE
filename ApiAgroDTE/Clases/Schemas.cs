using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace ApiAgroDTE.Clases
{
    public class Schemas
    {
        public string validarXML(string directorioxml,string TipoDTE){
            string schemaNamespace = "http://www.sii.cl/SiiDte";
            string NombreArchivoSchema = "";
            string schemaFileName = "";

            if (TipoDTE == "39" || TipoDTE == "41")
            {
                NombreArchivoSchema = "EnvioBOLETA_v11.xsd";
                schemaFileName = @"C:\inetpub\wwwroot\api_agrodte\AgroDTE_Archivos\Schemas\Boletas\" + NombreArchivoSchema;
            }
            else {
                NombreArchivoSchema = "EnvioDTE_v10.xsd";
                schemaFileName = @"C:\inetpub\wwwroot\api_agrodte\AgroDTE_Archivos\Schemas\DTEs\" + NombreArchivoSchema;
            }
            
            

            //string schemaFileName = @"C:\Users\Benjamin\source\repos\ApiAgroDTE\ApiAgroDTE\Schemas\EnvioDTE_v10.xsd";
            string filename = directorioxml;

            /*se hace la request al web service para validar el XML con un Schema. Se tuvo que realizar esto
            ya que la api esta basada en .NET CORE 5 y no corre el codigo en esta base.
            El servicio web ValidarXMLSchema esta con base frameqork 4.0*/

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

           
            string respuestaSchema = "";

            try
            {
                WebRequest request = WebRequest.Create("http://192.168.1.9:90/WebServiceValidarXML/ValidarXML.asmx/ValidarXml?xmlFilename=" + filename + "&schemaFilename=" + schemaFileName);
                //WebRequest request = WebRequest.Create("http://localhost:81/WebServiceValidarXML/ValidarXML.asmx/ValidarXml?xmlFilename=" + filename + "&schemaFilename=" + schemaFileName);
                request.Method = "GET";
                WebResponse response = request.GetResponse();

                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    respuestaSchema = reader.ReadToEnd(); // do something fun...
                }
            }
            catch (Exception e)
            {

                File.WriteAllText(@"..\logerror.txt", e.Message);
            }
              

           
            //ESTA RESPUESTA ES UN XML PERO ES UN STRING, POR LO TANTO PARSEAMOS DE STRING A XML
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(respuestaSchema);

            XmlNodeList elemlist = xmlDoc.GetElementsByTagName("string");
            string resultadoSchema = elemlist[0].InnerXml;
            string mensajeSchema = elemlist[1].InnerXml;


            //parsear xml para obtener la respuesta de la etiqueta string "Document is valid"
            if (resultadoSchema == "True")
            {
                //NO HAY ERRORES
                return "XML Valido";
            }else{
                //HAY ERRORES EN XML
                //CAMBIAR EL NOMBRE DEL XML Y AGREGARLE UN "ERROR" EN EL NOMBRE
                string filenameError = filename.Substring(0, filename.Length - 4);
                System.IO.File.Move(filename, filenameError + "ERRORSCHEMA.xml");
                return "XML Invalido:"+mensajeSchema;
            }
            
        }

        public string validarRespuestaXML(string directorioxml)
        {
            string schemaNamespace = "http://www.sii.cl/SiiDte";            
            string schemaFileName = "";

            //NombreArchivoSchema = "EnvioBOLETA_v11.xsd";
            schemaFileName = @"C:\inetpub\wwwroot\api_agrodte\AgroDTE_Archivos\Schemas\DTEs\RespuestaEnvioDTE_v10.xsd";    
            

            //string schemaFileName = @"C:\Users\Benjamin\source\repos\ApiAgroDTE\ApiAgroDTE\Schemas\EnvioDTE_v10.xsd";
            string filename = directorioxml;

            /*se hace la request al web service para validar el XML con un Schema. Se tuvo que realizar esto
            ya que la api esta basada en .NET CORE 5 y no corre el codigo en esta base.
            El servicio web ValidarXMLSchema esta con base frameqork 4.0*/

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
           
            string respuestaSchema = "";

            try
            {
                WebRequest request = WebRequest.Create("http://192.168.1.9:90/WebServiceValidarXML/ValidarXML.asmx/ValidarXml?xmlFilename=" + filename + "&schemaFilename=" + schemaFileName);
                //WebRequest request = WebRequest.Create("http://localhost:81/WebServiceValidarXML/ValidarXML.asmx/ValidarXml?xmlFilename=" + filename + "&schemaFilename=" + schemaFileName);
                request.Method = "GET";
                WebResponse response = request.GetResponse();

                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    respuestaSchema = reader.ReadToEnd(); // do something fun...
                }
            }
            catch (Exception e)
            {

                File.WriteAllText(@"..\logerror.txt", e.Message);
            }
              

           
            //ESTA RESPUESTA ES UN XML PERO ES UN STRING, POR LO TANTO PARSEAMOS DE STRING A XML
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(respuestaSchema);

            XmlNodeList elemlist = xmlDoc.GetElementsByTagName("string");
            string resultadoSchema = elemlist[0].InnerXml;
            string mensajeSchema = elemlist[1].InnerXml;


            //parsear xml para obtener la respuesta de la etiqueta string "Document is valid"
            if (resultadoSchema == "True")
            {
                //NO HAY ERRORES
                return "XML Valido";
            }else{
                //HAY ERRORES EN XML
                //CAMBIAR EL NOMBRE DEL XML Y AGREGARLE UN "ERROR" EN EL NOMBRE
                string filenameError = filename.Substring(0, filename.Length - 4);
                System.IO.File.Move(filename, filenameError + "ERRORSCHEMA.xml");
                return "XML Invalido:"+mensajeSchema;
            }
            
        }

        
    }
    
}