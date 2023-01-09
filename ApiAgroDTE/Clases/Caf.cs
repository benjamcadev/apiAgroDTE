using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using System.Security.Cryptography;
using System.IO;

namespace ApiAgroDTE.Clases
{
    public class Caf
    {

        string algo = "";
        string archxml = "";
        static bool verbose = false;

        public string[] rescatarCaf(string TipoDTE)
        {

            //Capturar fecha actual
                    string fechahhmm = System.DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
                    //Traer el caf correspondiente a factura
                    //***************CONECTAR A LA BD Y TRAER LA RUTA DEL CAF
                    ConexionBD conexion = new ConexionBD();
                    List<string> nombreFileCaf = conexion.Select("SELECT ruta_caf FROM xml_caf WHERE estado_caf = '1' AND tipo_documento_caf = '"+TipoDTE+"'");
                    //string rutaCaf = Path.Combine(Environment.CurrentDirectory, @"\CAF_PRUEBA\"+ nombreFileCaf[0]);
                    string rutaCaf = @"C:\inetpub\wwwroot\api_agrodte\AgroDTE_Archivos\CAF_PRUEBA\" + nombreFileCaf[0];

                    
                    

                    //string rutaCaf = @"C:\Users\Benjamin\source\repos\ApiAgroDTE\ApiAgroDTE\CAF_PRUEBA\FoliosSII7695843033201202111151654.xml";
                    //Traer el CAF
                    System.Xml.XmlTextReader lector = new System.Xml.XmlTextReader(rutaCaf);

                    //GUARDAMOS EN VARIABLES LOS DATOS DEL CAF
                    lector.ReadStartElement("AUTORIZACION");
                    lector.ReadStartElement("CAF");
                    lector.ReadStartElement("DA");
                    lector.ReadStartElement("RE");
                    string rutfolio = lector.ReadString();
                    lector.ReadEndElement();
                    lector.ReadStartElement("RS");
                    string nomfolio = lector.ReadString();
                    lector.ReadEndElement();
                    lector.ReadStartElement("TD");
                    string tipofolio = lector.ReadString();
                    lector.ReadEndElement();
                    lector.ReadStartElement("RNG");

                    lector.ReadStartElement("D");
                    string desdefolio = lector.ReadString();
                    lector.ReadEndElement();

                    lector.ReadStartElement("H");
                    string hastafolio = lector.ReadString();
                    lector.ReadEndElement();

                    lector.ReadEndElement();

                    lector.ReadStartElement("FA");
                    string fechafolio = lector.ReadString();
                    lector.ReadEndElement();

                    lector.ReadStartElement("RSAPK");
                    lector.ReadStartElement("M");
                    string Mfolio = lector.ReadString();
                    lector.ReadEndElement();

                    lector.ReadStartElement("E");
                    string Efolio = lector.ReadString();
                    lector.ReadEndElement();
                    lector.ReadEndElement();

                    lector.ReadStartElement("IDK");
                    string IDKfolio = lector.ReadString();
                    lector.ReadEndElement();
                    lector.ReadEndElement();

                    lector.ReadStartElement("FRMA");
                    string algoritmofolio = lector.ReadString();
                    lector.ReadEndElement();


                    lector.ReadEndElement();
                    // EXTRAER CLAVE PRIVADA DE FOLIOS DE FACTURAS

                    lector.ReadStartElement("RSASK");
                    String pk = lector.ReadString();
                    lector.ReadEndElement();

                    
     
                    //  BUSCAR ELEMENTOS DE NODOS DE FOLIOS CAF-----------------------------

                    string caf_str = "";

                    XmlDocument doc = new XmlDocument();
                    doc.Load(rutaCaf);
                    int i;

                    XmlNodeList caf_folio = doc.GetElementsByTagName("CAF");
                    for (i = 0; i < caf_folio.Count; i++)
                    {

                        caf_str = caf_folio[i].OuterXml;
                    }

                    string[] stringArray = new string[] { rutaCaf, caf_str, fechahhmm, rutfolio, nomfolio, tipofolio, desdefolio, hastafolio, fechafolio, Mfolio, Efolio, IDKfolio, algoritmofolio,pk };

                    //return Tuple.Create(rutaCaf, caf_str, fechahhmm, rutfolio, nomfolio, tipofolio, desdefolio, hastafolio, fechafolio, Mfolio, Efolio, IDKfolio, algoritmofolio);
                    return stringArray;
/*
            switch (TipoDTE)
            {
                case "33":
                    
                   
                    break;



                default:
                    break;
            }
            */

            string[] stringArray_vacio = new string[0] { };


            return stringArray_vacio;

            //return Tuple.Create("", "", "", "", "", "", "", "", "", "", "", "", "");
        }

        public string[] crearDD(string rutaCaf, JsonElement Detalles, JsonElement Encabezado,string TipoDTE,string Folio,string caf_str,string fechahhmm, string MntTotal)
        {
            //CultureInfo cultureUS = new CultureInfo("en-US");
            // precioItem = decimal.Parse(PrcItem[i].InnerXml, cultureUS).ToString("N");

            JArray detallesArray = JArray.Parse(Detalles.ToString());
          
            List<object> ListaDetalles = new List<object>();

            //Recorro los detalles enviados y los agrego a una lista
            foreach (var detalle in detallesArray)
            {
                ListaDetalles.Add(detalle);
               
            }
            var detalle1 = ListaDetalles[0].ToString();
            Detalle detalleObject = JsonConvert.DeserializeObject<Detalle>(detalle1);

            //SETEAMOS EL LARGO MAX DEL NMB ITEM ES DE 40 CARACTERES
            int MaxLength = 40;
            string nmbItem = "";
            if (detalleObject.NmbItem.Length > MaxLength)
            {
                nmbItem = detalleObject.NmbItem.Substring(0, 39);
            }
            else
            {
                nmbItem = detalleObject.NmbItem;
            }
            string item_dd = string.Empty;
            item_dd = nmbItem;
            item_dd = item_dd.Replace("&", "&amp;");
            item_dd = item_dd.Replace("<", "&lt;");
            item_dd = item_dd.Replace("<", "&lt;");
            item_dd = item_dd.Replace(">", "&gt;");
            item_dd = item_dd.Replace("'", "&apos;");
            item_dd = item_dd.Replace("\"", "&quot;");

            
            byte[] tempBytes;
            tempBytes = System.Text.Encoding.GetEncoding("ISO-8859-8").GetBytes(item_dd);
            item_dd = System.Text.Encoding.UTF8.GetString(tempBytes);

            JsonElement Receptor = Encabezado.GetProperty("Receptor");
            string razonReceptor = "";
            if (TipoDTE == "39" || TipoDTE == "41")
            {
                razonReceptor = "66666666-6";
            }
            else
            {
                razonReceptor = Receptor.GetProperty("RznSocRecep").ToString();
            }

            //SETEAMOS EL LARGO MAX DEL razonReceptor ES DE 40 CARACTERES
            //int MaxLength = 40;

            if (razonReceptor.Length > MaxLength)
            {
                razonReceptor = razonReceptor.Substring(0, 39);
            }
            else
            {
                razonReceptor = razonReceptor;
            }

            string razonreceptor_dd = string.Empty;
            razonreceptor_dd = razonReceptor;
            razonreceptor_dd = razonreceptor_dd.Replace("&", "&amp;");
            razonreceptor_dd = razonreceptor_dd.Replace("<", "&lt;");
            razonreceptor_dd = razonreceptor_dd.Replace("<", "&lt;");
            razonreceptor_dd = razonreceptor_dd.Replace(">", "&gt;");
            razonreceptor_dd = razonreceptor_dd.Replace("'", "&apos;");
            razonreceptor_dd = razonreceptor_dd.Replace("\"", "&quot;");

            byte[] tempBytes2;
            tempBytes2 = System.Text.Encoding.GetEncoding("ISO-8859-8").GetBytes(razonreceptor_dd);
            razonreceptor_dd = System.Text.Encoding.UTF8.GetString(tempBytes2);

            //Rescatar datos de las variables nuevas del xml

            JsonElement Emisor = Encabezado.GetProperty("Emisor");
            string rutEmisor = Emisor.GetProperty("RUTEmisor").ToString();
            string fechadocu2 = System.DateTime.Now.ToString("yyyy-MM-dd");
            string rutReceptor = Receptor.GetProperty("RUTRecep").ToString();
            //JsonElement Totales = Encabezado.GetProperty("Totales");
            //string totales = Totales.GetProperty("MntTotal").ToString();

            String DD = "<DD><RE>" +rutEmisor + "</RE><TD>" + TipoDTE + "</TD><F>" + Folio;
            DD += "</F><FE>" + fechadocu2 + "</FE><RR>" + rutReceptor + "</RR><RSR>" + razonreceptor_dd;
            DD += "</RSR><MNT>" + MntTotal + "</MNT><IT1>" + item_dd + "</IT1>";
            DD += caf_str + "<TSTED>" + fechahhmm + "</TSTED></DD>";


            {
                XmlDocument xDoc = new XmlDocument();

                // BUSCAR ATRIBUTO DE ALGORITMO

                xDoc.Load(rutaCaf);

                XmlNodeList personas = xDoc.GetElementsByTagName("AUTORIZACION");

                XmlNodeList lista =
                ((XmlElement)personas[0]).GetElementsByTagName("FRMA");

                foreach (XmlElement nodo in lista)
                {

                    algo = nodo.GetAttribute("algoritmo");

                }

            }
            string[] stringArray = new string[] {algo,DD};
            return stringArray;
        }
        public string crearTimbre(String pk_2, String strDD)
        {      
        /*Calcule el hash de los datos a firmar DD
        transformando la cadena DD a arreglo de bytes, luego con
        el objeto 'SHA1CryptoServiceProvider' creamos el Hash del
        arreglo de bytes que representa los datos del DD*/
            
            strDD = strDD.Replace("\t", string.Empty);
            strDD = strDD.Replace("\r\n", string.Empty);
            strDD = strDD.Replace("\n", string.Empty);

            Encoding encode = Encoding.GetEncoding("ISO-8859-1");
            byte[] bytesStrDD = encode.GetBytes(strDD);
            byte[] HashValue = new SHA1CryptoServiceProvider().ComputeHash(bytesStrDD);

            /*Cree el objeto Rsa para poder firmar el hashValue creado
            en el punto anterior. La clase FuncionesComunes.crearRsaDesdePEM()
            Transforma la llave rivada del CAF en formato PEM a el objeto
            Rsa necesario para la firma.*/

            RSACryptoServiceProvider rsa = crearRsaDesdePEM(pk_2);

            /*Firme el HashValue ( arreglo de bytes representativo de DD )
            utilizando el formato de firma SHA1, lo cual regresará un nuevo 
             arreglo de bytes.*/
            byte[] bytesSing = rsa.SignHash(HashValue, "SHA1");

            /*Recupere la representación en base 64 de la firma, es decir de
            el arreglo de bytes */
            string FRMT1 = Convert.ToBase64String(bytesSing);


          
            return FRMT1;
        }

        public static RSACryptoServiceProvider crearRsaDesdePEM(string base64)
    {


        ////
        //// Extraiga de la cadena los header y footer
        base64 = base64.Replace("-----BEGIN RSA PRIVATE KEY-----", string.Empty);
        base64 = base64.Replace("-----END RSA PRIVATE KEY-----", string.Empty);


        ////
        //// el resultado que se encuentra en base 64 cambielo a
        //// resultado string
        byte[] arrPK = Convert.FromBase64String(base64);

        ////
        //// obtenga el Rsa object a partir de
        return DecodeRSAPrivateKey(arrPK);

    }

    public static RSACryptoServiceProvider DecodeRSAPrivateKey(byte[] privkey)
    {
        byte[] MODULUS, E, D, P, Q, DP, DQ, IQ;


        // --------- Set up stream to decode the asn.1 encoded RSA private key ------
        MemoryStream mem = new MemoryStream(privkey);
        BinaryReader binr = new BinaryReader(mem);  //wrap Memory Stream with BinaryReader for easy reading
        byte bt = 0;
        ushort twobytes = 0;
        int elems = 0;
        try
        {
            twobytes = binr.ReadUInt16();
            if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
                binr.ReadByte();        //advance 1 byte
            else if (twobytes == 0x8230)
                binr.ReadInt16();        //advance 2 bytes
            else
                return null;


            twobytes = binr.ReadUInt16();
            if (twobytes != 0x0102) //version number
                return null;
            bt = binr.ReadByte();
            if (bt != 0x00)
                return null;




            //------ all private key components are Integer sequences ----
            elems = GetIntegerSize(binr);
            MODULUS = binr.ReadBytes(elems);


            elems = GetIntegerSize(binr);
            E = binr.ReadBytes(elems);


            elems = GetIntegerSize(binr);
            D = binr.ReadBytes(elems);


            elems = GetIntegerSize(binr);
            P = binr.ReadBytes(elems);


            elems = GetIntegerSize(binr);
            Q = binr.ReadBytes(elems);


            elems = GetIntegerSize(binr);
            DP = binr.ReadBytes(elems);


            elems = GetIntegerSize(binr);
            DQ = binr.ReadBytes(elems);


            elems = GetIntegerSize(binr);
            IQ = binr.ReadBytes(elems);


            Console.WriteLine("showing components ..");
            if (verbose)
            {
                showBytes("\nModulus", MODULUS);
                showBytes("\nExponent", E);
                showBytes("\nD", D);
                showBytes("\nP", P);
                showBytes("\nQ", Q);
                showBytes("\nDP", DP);
                showBytes("\nDQ", DQ);
                showBytes("\nIQ", IQ);
            }


            // ------- create RSACryptoServiceProvider instance and initialize with public key -----
            CspParameters CspParameters = new CspParameters();
            CspParameters.Flags = CspProviderFlags.UseMachineKeyStore;
            RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(1024, CspParameters);
            RSAParameters RSAparams = new RSAParameters();
            RSAparams.Modulus = MODULUS;
            RSAparams.Exponent = E;
            RSAparams.D = D;
            RSAparams.P = P;
            RSAparams.Q = Q;
            RSAparams.DP = DP;
            RSAparams.DQ = DQ;
            RSAparams.InverseQ = IQ;
            RSA.ImportParameters(RSAparams);
            return RSA;
        }
        catch (Exception ex)
        {
            return null;
        }
        finally
        {
            binr.Close();
        }
    }


    private static int GetIntegerSize(BinaryReader binr)
    {
        byte bt = 0;
        byte lowbyte = 0x00;
        byte highbyte = 0x00;
        int count = 0;
        bt = binr.ReadByte();
        if (bt != 0x02)            //expect integer
            return 0;
        bt = binr.ReadByte();


        if (bt == 0x81)
            count = binr.ReadByte();    // data size in next byte
        else
            if (bt == 0x82)
            {
                highbyte = binr.ReadByte();    // data size in next 2 bytes
                lowbyte = binr.ReadByte();
                byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                count = BitConverter.ToInt32(modint, 0);
            }
            else
            {
                count = bt;            // we already have the data size
            }


        while (binr.ReadByte() == 0x00)
        {    //remove high order zeros in data
            count -= 1;
        }
        binr.BaseStream.Seek(-1, SeekOrigin.Current);            //last ReadByte wasn't a removed zero, so back up a byte
        return count;
    }

    private static void showBytes(String info, byte[] data)
    {
        Console.WriteLine("{0} [{1} bytes]", info, data.Length);
        for (int i = 1; i <= data.Length; i++)
        {
            Console.Write("{0:X2} ", data[i - 1]);
            if (i % 16 == 0)
                Console.WriteLine();
        }
        Console.WriteLine("\n\n");
    }

        public class Detalle
        {
            public string NroLinDet { get; set; }
            public string NmbItem { get; set; }
            public string QtyItem { get; set; }
            public string PrcItem { get; set; }
            public string MontoItem { get; set; }
        }
    }
}
