# apiAgroDTE
api que permite realizar la comunicacion y generacion de archivos para facturacion y boletas con SII


## IMPORTANTE
Para produccion se utiliza el servidor "palena" (Facturas) y "api" O "rahue" (boletas)  
Para certificacion se utiliza el servidor "maullin" (Facturas) y "apicert" (boletas)

## SE DEBEN CAMBIAR LAS LINEAS DE CODIGO
PruebaControlador.cs:  
* 35 public static string servidor_boletas = "apicert";  
* 36 public static string servidor_facturas = "maullin";  
* 692 string respuestaPing = checkPing("palena.sii.cl");  
* 693 string respuestaConexion = checkConnection("https://palena.sii.cl/DTEWS/CrSeed.jws");

EnvioDTE.cs
* 374 var client = new RestClient("https://rahue.sii.cl/recursos/v1/boleta.electronica.envio");
* 385 request.AddHeader("Host", "rahue.sii.cl"); 
* 476 var client = new RestClient("https://api.sii.cl/recursos/v1/");

