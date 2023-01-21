# apiAgroDTE
api que permite realizar la comunicacion y generacion de archivos para facturacion y boletas con SII


## IMPORTANTE
Para produccion se utiliza el servidor "palena" (Facturas) y "api" O "rahue" (boletas)  
Para certificacion se utiliza el servidor "maullin" (Facturas) y "apicert" o "pangal" (boletas)+

Para pedir token de boletas sirve "apicert" certificacion o "api" produccion
Para el envio post de boletas sirve "pangal" certificacion y "rahue" produccion

Se deben obtener los CAF de produccion o certificacion

en shemas.cs y envioDTE.cs localhost:90 (Si esta montado en el server) localhost.81 (si esta en local)


## SE DEBEN CAMBIAR LAS LINEAS DE CODIGO
PruebaControlador.cs:  
* 35 public static string servidor_boletas = "apicert";  
* 36 public static string servidor_facturas = "maullin";  
* 1191 string respuestaPing = checkPing("palena.sii.cl");  
* 1192 string respuestaConexion = checkConnection("https://palena.sii.cl/DTEWS/CrSeed.jws");

EnvioDTE.cs
* 376 var client = new RestClient("https://rahue.sii.cl/recursos/v1/boleta.electronica.envio");
* 387 request.AddHeader("Host", "rahue.sii.cl"); 
* 476 var client = new RestClient("https://api.sii.cl/recursos/v1/");



