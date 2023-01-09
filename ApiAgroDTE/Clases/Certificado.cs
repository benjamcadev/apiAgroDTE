using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using System.Xml.Linq;

namespace ApiAgroDTE.Clases
{
    public class Certificado
    {

        public string firmarConCertificado(string T33F, XmlDocument oDocument,string DirectorioDTE){

    
        string nombrecertificado = "RUBEN SALATIEL RIVERA GALLEGUILLOS";
        string xmlparafirmar = oDocument.OuterXml;

        string ID_xml = "#" + T33F;
        string firmado = Firmar(nombrecertificado, xmlparafirmar, ID_xml);
        XDocument xml_firmado = XDocument.Parse(firmado, LoadOptions.PreserveWhitespace);
        xml_firmado.Save(DirectorioDTE);

        return "Listo !";
        }

        public string Firmar(string str_nombrecertificado, string str_estructuraXML, string str_referenciaUri)
        {
            //RichTextBox richTextBox1 = new RichTextBox();
            //string str_resultado = string.Empty;
            string str_TMPXML  = string.Empty;

            //string referencianueva = str_referenciaUri1;

            X509Certificate2 obtcertificado = ObtenerCertificado(str_nombrecertificado);

            try
            {
                str_TMPXML = FirmarDocumentoXML(str_estructuraXML, obtcertificado, str_referenciaUri);
            }
            catch (Exception e)
            {
                string error = e.ToString();
                str_TMPXML = string.Empty; //INDICAR QUE NO HAY CERTIFICADO INTALADO EN EL PC
            }
            return str_TMPXML;

        }

         public X509Certificate2 ObtenerCertificado(string str_nombrecertificado)
        {
            X509Certificate2 certificado = new X509Certificate2();
            try
            {

                string certPath = @"..\AgroDTE_Archivos\Certificado\firma_6402678K.pfx";
                string certPass = "agro1113";

                // Create a collection object and populate it using the PFX file
                X509Certificate2Collection Certificados3 = new X509Certificate2Collection();
                Certificados3.Import(certPath, certPass, X509KeyStorageFlags.PersistKeySet);

                if (Certificados3 != null && Certificados3.Count != 0)
                {
                    certificado = Certificados3[0];
                }


            }
            catch (Exception e)
            {
                certificado = null;


            }



            return certificado;
            //return certificado.FriendlyName.ToString();

        }

        public string FirmarDocumentoXML(string str_documentoXML, X509Certificate2 certificado, string str_referenciaUri)
        {
            //RichTextBox richTextBox1 = new RichTextBox();

            XmlDocument DocumentoXML = new XmlDocument();
            DocumentoXML.PreserveWhitespace = true;
            DocumentoXML.LoadXml(str_documentoXML);

            SignedXml signedXml = new SignedXml(DocumentoXML);
            signedXml.SigningKey = certificado.PrivateKey;
            Signature XMLSignature = signedXml.Signature;
            
            //Esto lo agregamos SHA1
            signedXml.SignedInfo.SignatureMethod = SignedXml.XmlDsigRSASHA1Url;

            Reference reference = new Reference();
            reference.Uri = str_referenciaUri;
            //Esto lo agregamos SHA1
            reference.DigestMethod = "http://www.w3.org/2000/09/xmldsig#sha1";

            XMLSignature.SignedInfo.AddReference(reference);
            KeyInfo keyInfo = new KeyInfo();
            keyInfo.AddClause(new RSAKeyValue((RSA)certificado.PrivateKey));
            keyInfo.AddClause(new KeyInfoX509Data(certificado));
            XMLSignature.KeyInfo = keyInfo;

            signedXml.ComputeSignature();
            XmlElement xmlDigitalSignature = signedXml.GetXml();

            DocumentoXML.DocumentElement.AppendChild(DocumentoXML.ImportNode(xmlDigitalSignature, true));

            //richTextBox1.Text = str_documentoXML;

            return DocumentoXML.OuterXml;
        } 

          public static class GlobalVar
        {
        static string _estcert;
        public static string EstadodeCertificado
        {
            get { return _estcert; }
            set { _estcert = value; }
        }
        }
        
  
    }
}