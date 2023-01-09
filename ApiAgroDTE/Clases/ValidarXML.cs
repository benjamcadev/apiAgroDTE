using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Xml;
using System.Xml.Schema;

namespace ValidarXMLconXSD
{
    public class ValidarXML
    {
        private bool falhou;
        public bool Falhou
        {
            get { return falhou; }
        }

        public bool ValidarXml(string xmlFilename, string schemaFilename)
        {
            // Define o tipo de validação
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.Schema;
            // Carrega o arquivo de esquema
            XmlSchemaSet schemas = new XmlSchemaSet();
            settings.Schemas = schemas;
            // Quando carregar o eschema, especificar o namespace que ele valida
            // e a localização do arquivo 
            schemas.Add(null, schemaFilename);
            // Especifica o tratamento de evento para os erros de validacao
            settings.ValidationEventHandler += ValidationEventHandler;
            // cria um leitor para validação
            XmlReader validator = XmlReader.Create(xmlFilename, settings);
            falhou = false;
            try
            {
                // Faz a leitura de todos os dados XML
                while (validator.Read()) {}
            }
            catch (XmlException err)
            {
                // Um erro ocorre se o documento XML inclui caracteres ilegais
                // ou tags que não estão aninhadas corretamente
                   // GlobalVar.Mensaje = "Ocurrio un error critico durante la validacion XML." + "\r\n";
                    //GlobalVar.Mensaje += err.Message + "\r\n";
                falhou = true;
            }
            finally
            {
                validator.Close();
            }
            return !falhou;
        }

        private void ValidationEventHandler(object sender, ValidationEventArgs args)
        {
            falhou = true;
            // Exibe o erro da validação
            //GlobalVar.Mensaje = "Error de validacion : " + args.Message  + "\r\n";

        }
   }

    
}