CREATE procedure SP_FACE_UTILS10 
(in
opcion INT, 
param1 NVARCHAR(100),
param2 NVARCHAR(100),
param3 NVARCHAR(100),
param4 NVARCHAR(100),
param5 NVARCHAR(100),
param6 NVARCHAR(100),
param7 NVARCHAR(100),
param8 NVARCHAR(100)
)
AS
--Forma de llamada
--"CALL SP_FACE_UTILS('1','','','','','','','','')"
BEGIN

/***** OPCION 1 *****/
	IF (:opcion = 1) THEN
		update "OINV" O set  O."U_ESTADO_FACE"='A' WHERE O."DocEntry"=:param1;
		update "OINV" O set  O."U_FACE_XML"=:param2 WHERE O."DocEntry"=:param1;
		update "OINV" O set  O."U_FACE_PDFFILE"= NULL WHERE O."DocEntry"=:param1;
		update "OINV" O set  O."U_FIRMA_ELETRONICA"=:param3 WHERE O."DocEntry"=:param1;
		update "OINV" O set  O."U_NUMERO_DOCUMENTO"=:param4 WHERE O."DocEntry"=:param1;
		update "OINV" O set  O."U_NUMERO_RESOLUCION"=:param5 WHERE O."DocEntry"=:param1;
		update "OINV" O set  O."U_SERIE_FACE"=:param6 WHERE O."DocEntry"=:param1;
		update "OINV" O set  O."U_FACTURA_INI"=:param7 WHERE O."DocEntry"=:param1;
		update "OINV" O set  O."U_FACTURA_FIN"=:param8 WHERE O."DocEntry"=:param1;
	END IF;
/***** OPCION 2 *****/
	IF (:opcion = 2) THEN
		update "OPCH" O set  O."U_ESTADO_FACE"='A' WHERE O."DocEntry"=:param1;
		update "OPCH" O set  O."U_FACE_XML"=:param2 WHERE O."DocEntry"=:param1;
		update "OPCH" O set  O."U_FACE_PDFFILE"= NULL WHERE O."DocEntry"=:param1;
		update "OPCH" O set  O."U_FIRMA_ELETRONICA"=:param3 WHERE O."DocEntry"=:param1;
		update "OPCH" O set  O."U_NUMERO_DOCUMENTO"=:param4 WHERE O."DocEntry"=:param1;
		update "OPCH" O set  O."U_NUMERO_RESOLUCION"=:param5 WHERE O."DocEntry"=:param1;
		update "OPCH" O set  O."U_SERIE_FACE"=:param6 WHERE O."DocEntry"=:param1;
		update "OPCH" O set  O."U_FACTURA_INI"=:param7 WHERE O."DocEntry"=:param1;
		update "OPCH" O set  O."U_FACTURA_FIN"=:param8 WHERE O."DocEntry"=:param1;
	END IF;
/***** OPCION 3 *****/
	IF (:opcion = 3) THEN
		update "ORIN" O set  O."U_ESTADO_FACE"='A' WHERE O."DocEntry"=:param1;
		update "ORIN" O set  O."U_FACE_XML"=:param2 WHERE O."DocEntry"=:param1;
		update "ORIN" O set  O."U_FACE_PDFFILE"= NULL WHERE O."DocEntry"=:param1;
		update "ORIN" O set  O."U_FIRMA_ELETRONICA"=:param3 WHERE O."DocEntry"=:param1;
		update "ORIN" O set  O."U_NUMERO_DOCUMENTO"=:param4 WHERE O."DocEntry"=:param1;
		update "ORIN" O set  O."U_NUMERO_RESOLUCION"=:param5 WHERE O."DocEntry"=:param1;
		update "ORIN" O set  O."U_SERIE_FACE"=:param6 WHERE O."DocEntry"=:param1;
		update "ORIN" O set  O."U_FACTURA_INI"=:param7 WHERE O."DocEntry"=:param1;
		update "ORIN" O set  O."U_FACTURA_FIN"=:param8 WHERE O."DocEntry"=:param1;
	END IF;
/***** OPCION 4 *****/
	IF (:opcion = 4) THEN

	END IF;
/***** OPCION 5 *****/
	IF (:opcion = 5) THEN

	END IF;	
/***** OPCION 6 *****/
	IF (:opcion = 6) THEN

	END IF;	
	
	
END;