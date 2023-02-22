# apiAgroDTE
api que permite realizar la comunicacion y generacion de archivos para facturacion y boletas con SII
FrontEnd del proyecto: https://github.com/benjamcadev/WebAgroDTE


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
* 1352 string respuestaPing = checkPing("palena.sii.cl");  

EnvioDTE.cs
* 388 var client = new RestClient("https://rahue.sii.cl/recursos/v1/boleta.electronica.envio");
* 399 request.AddHeader("Host", "rahue.sii.cl"); 
* 490 var client = new RestClient("https://api.sii.cl/recursos/v1/");



