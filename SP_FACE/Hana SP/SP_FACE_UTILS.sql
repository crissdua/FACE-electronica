CREATE procedure SP_FACE_UTILS 
(in
opcion INT, 
param1 NVARCHAR(100),
param2 NVARCHAR(100),
param3 NVARCHAR(100),
param4 NVARCHAR(100),
param5 NVARCHAR(100)
)
AS
--Forma de llamada
--"CALL SP_FACE_UTILS('1','','','','','')"
BEGIN

/***** OPCION 1 *****/
	IF (:opcion = 1) THEN
		select iFnull("U_ESTADO_FACE",'P') AS "estado" from "OINV" where "DocEntry"=:param1;
	END IF;
/***** OPCION 2 *****/
	IF (:opcion = 2) THEN
		select iFnull("U_ESTADO_FACE",'P') AS "estado" from "ORIN" where "DocEntry"=:param1;
	END IF;
/***** OPCION 3 *****/
	IF (:opcion = 3) THEN
		select 
	replace(TO_DATE(CURRENT_DATE,'YYYY-MM-DD'),'/','-')
	||'T'|| CURRENT_TIME FROM DUMMY AS "Fecha";
	END IF;
/***** OPCION 4 *****/
	IF (:opcion = 4) THEN
		select "DocNum" from "OINV" where "DocEntry"=:param1;
	END IF;	
/***** OPCION 5 *****/
	IF (:opcion = 5) THEN
		select "DocNum" from "ORIN" where "DocEntry"=:param1;
	END IF;	
/***** OPCION 6 *****/
	IF (:opcion = 6) THEN
		SELECT "U_TIPO_DOC" FROM "@FACE_RESOLUCION" WHERE "U_SERIE" = :param1;
	END IF;	
/***** OPCION 7 *****/
	IF (:opcion = 7) THEN
		SELECT "U_SUCURSAL" FROM "@FACE_RESOLUCION" WHERE "U_SERIE" = :param1;
	END IF;
/***** OPCION 8 *****/
	IF (:opcion = 8) THEN
		SELECT "U_DISPOSITIVO" FROM "@FACE_RESOLUCION" WHERE "U_SERIE" = :param1;
	END IF;
/***** OPCION 9 *****/
	IF (:opcion = 9) THEN
		update "OINV" O set O."U_ESTADO_FACE" ='R',O."U_FACE_XML"= :param1 ,O."U_MOTIVO_RECHAZO"= :param2 WHERE  O."DocEntry"=:param3;
	END IF;
/***** OPCION 10 *****/
	IF (:opcion = 10) THEN
		update "OINV" O set O."U_ESTADO_FACE" ='R',O."U_FACE_XML"= :param1 ,O."U_MOTIVO_RECHAZO"= :param2 WHERE  O."DocEntry"=:param3;
	END IF;
/***** OPCION 11 *****/
	IF (:opcion = 11) THEN
		update "OINV" O set  O."U_ESTADO_FACE"='A' WHERE O."DocEntry"=:param1;
		update "OINV" O set  O."U_FACE_XML"=:param2 WHERE O."DocEntry"=:param1;
		update "OINV" O set  O."U_FIRMA_ELETRONICA"=:param3 WHERE O."DocEntry"=:param1;
		update "OINV" O set  O."U_NUMERO_DOCUMENTO"=:param4 WHERE O."DocEntry"=:param1;
		update "OINV" O set  O."U_SERIE_FACE"=:param5 WHERE O."DocEntry"=:param1;
	END IF;	
/***** OPCION 12 *****/
	IF (:opcion = 12) THEN
		update "OINV" O set O."U_ESTADO_FACE" ='R',O."U_FACE_XML"=:param1,O."U_MOTIVO_RECHAZO"=:param2 WHERE O."DocEntry"=:param3;
	END IF;	
/***** OPCION 13 *****/
	IF (:opcion = 13) THEN
		select ifnull(sum( case "DocType" when 'S' then 1* B."PriceAfVAT" else B."Quantity" * B."PriceAfVAT" END),0)
        from OINV A inner join INV1 B on A."DocEntry"=B."DocEntry" 
        where A."DocEntry" = :param1 and   B."TaxCode"='EXE';
	END IF;	
/***** OPCION 14 *****/
	IF (:opcion = 14) THEN
		select ifnull(sum( case "DocType" when 'S' then 1* B."PriceAfVAT" else B."Quantity" * B."PriceAfVAT" END),0)
        from OINV A inner join RIN1 B on A."DocEntry"=B."DocEntry" 
        where A."DocEntry" = :param1 and   B."TaxCode"='EXE';
	END IF;	
/***** OPCION 15 *****/
	IF (:opcion = 15) THEN
		select iFnull(SUM("LineTotal"),0) from INV1 where "TaxCode" <>'EXE' and "DocEntry"=:param1;
	END IF; 
/***** OPCION 16 *****/
	IF (:opcion = 16) THEN
		select iFnull(SUM("LineTotal"),0) from RIN1 where "TaxCode" <>'EXE' and "DocEntry"=:param1;
	END IF;	
/***** OPCION 17 *****/
	IF (:opcion = 17) THEN
		select * from OINV WHERE  "DocEntry"=:param1 and ifnull("U_ESTADO_FACE",'P')='A';
	END IF;
/***** OPCION 18 *****/
	IF (:opcion = 18) THEN
		select * from ORIN WHERE  "DocEntry"=:param1 and ifnull("U_ESTADO_FACE",'P')='A';
	END IF;
/***** OPCION 19 *****/
	IF (:opcion = 19) THEN
		select * from OPCH WHERE  "DocEntry"=:param1 and ifnull("U_ESTADO_FACE",'P')='A';
	END IF;	
/***** OPCION 20 *****/
	IF (:opcion = 20) THEN
		SELECT ifnull("U_ES_BATCH",'N') FROM "@FACE_RESOLUCION" WHERE "Code" = :param1;
	END IF;
/***** OPCION 21 *****/
	IF (:opcion = 21) THEN
		select N."SeriesName",O."DocNum" from ORIN O inner join NNM1 N on O."Series" = ifnull(N."EndStr",N."Series")  
		where O."DocEntry" = :param1;
	END IF;
/***** OPCION 22 *****/
	IF (:opcion = 22) THEN
		select N."SeriesName",O."DocNum" from OPCH O inner join NNM1 N on O."Series" = ifnull(N."EndStr",N."Series")  
		where O."DocEntry" = :param1;
	END IF;
/***** OPCION 23 *****/
	IF (:opcion = 23) THEN
		select N."SeriesName",O."DocNum" from OINV O inner join NNM1 N on O."Series" = ifnull(N."EndStr",N."Series")  
		where O."DocEntry" = :param1;
	END IF;
/***** OPCION 24 *****/
	IF (:opcion = 24) THEN
		update "OINV" O set O."U_ESTADO_FACE" ='R',O."U_FACE_XML"=:param1 ,O."U_MOTIVO_RECHAZO"=:param2 WHERE O."DocEntry"= :param3;
	END IF;
/***** OPCION 25 *****/
	IF (:opcion = 25) THEN
		update "OPCH" O set O."U_ESTADO_FACE" ='R',O."U_FACE_XML"=:param1 ,O."U_MOTIVO_RECHAZO"=:param2 WHERE O."DocEntry"= :param3;
	END IF;
/***** OPCION 26 *****/
	IF (:opcion = 26) THEN
		update "ORIN" O set  O."U_ESTADO_FACE"='A' WHERE O."DocEntry"=:param1;
		update "ORIN" O set  O."U_FACE_XML"=:param2 WHERE O."DocEntry"=:param1;
		update "ORIN" O set  O."U_FIRMA_ELETRONICA"=:param3 WHERE O."DocEntry"=:param1;
		update "ORIN" O set  O."U_NUMERO_DOCUMENTO"=:param4 WHERE O."DocEntry"=:param1;
		update "ORIN" O set  O."U_SERIE_FACE"=:param5 WHERE O."DocEntry"=:param1;
	END IF;	
/***** OPCION 27 *****/
	IF (:opcion = 27) THEN
		update "OINV" O set O."U_ESTADO_FACE" ='R',O."U_FACE_XML"= :param1, O."U_MOTIVO_RECHAZO" = :param2 WHERE O."DocEntry"= :param3;
	END IF;	
/***** OPCION 28 *****/
	IF (:opcion = 28) THEN
		update "OPCH" O set O."U_ESTADO_FACE" ='R',O."U_FACE_XML"= :param1, O."U_MOTIVO_RECHAZO" = :param2 WHERE O."DocEntry"= :param3;
	END IF;	
/***** OPCION 29 *****/
	IF (:opcion = 29) THEN
		update "ORIN" O set O."U_ESTADO_FACE" ='R',O."U_FACE_XML"= :param1, O."U_MOTIVO_RECHAZO" = :param2 WHERE O."DocEntry"= :param3;
	END IF;	
/***** OPCION 30 *****/
	IF (:opcion = 30) THEN
		update "OINV" O set O."U_NUMERO_DOCUMENTO" = :param1,O."U_FIRMA_ELETRONICA"=:param2, O."U_ESTADO_FACE"='A' WHERE O."DocEntry"= :param3;
	END IF;	
/***** OPCION 31 *****/
	IF (:opcion = 31) THEN
		update "ORIN" O set O."U_NUMERO_DOCUMENTO" = :param1,O."U_FIRMA_ELETRONICA"=:param2, O."U_ESTADO_FACE"='A' WHERE O."DocEntry"= :param3;
	END IF;	
/***** OPCION 32 *****/
	IF (:opcion = 32) THEN
		update "OINV" O set O."U_ESTADO_FACE" ='R',O."U_MOTIVO_RECHAZO"= :param1 WHERE O."DocEntry" = :param2;
	END IF;	
/***** OPCION 33 *****/
	IF (:opcion = 33) THEN
		update "ORIN" O set O."U_ESTADO_FACE" ='R',O."U_MOTIVO_RECHAZO"= :param1 WHERE O."DocEntry" = :param2;
	END IF;				
/***** OPCION 34 *****/
	IF (:opcion = 34) THEN
		select "DocNum" from OINV where "DocEntry"=:param1;
	END IF;	
/***** OPCION 35 *****/
	IF (:opcion = 35) THEN
		select "DocNum" from ORIN where "DocEntry"=:param1;
	END IF;	
/***** OPCION 36 *****/
	IF (:opcion = 36) THEN
		update "OINV" O set O."U_ESTADO_FACE" ='R',O."U_FACE_XML"=:param1,O."U_MOTIVO_RECHAZO"=:param2 WHERE O."DocEntry"=:param3;
	END IF;	
/***** OPCION 37 *****/
	IF (:opcion = 37) THEN
		update "OINV" O set O."U_ESTADO_FACE" ='R',O."U_FACE_XML"=:param1,O."U_MOTIVO_RECHAZO"=:param2 WHERE O."DocEntry"=:param3;
	END IF;	
/***** OPCION 38 *****/
	IF (:opcion = 38) THEN
		update "ORIN" O set O."U_ESTADO_FACE" ='R',O."U_FACE_XML"=:param1,O."U_MOTIVO_RECHAZO"=:param2 WHERE O."DocEntry"=:param3;
	END IF;	
/***** OPCION 39 *****/
	IF (:opcion = 39) THEN
		SELECT * FROM OINV WHERE "DocEntry"=:param1;
	END IF;	
/***** OPCION 40 *****/
	IF (:opcion = 40) THEN
		SELECT * FROM ORIN WHERE "DocEntry"=:param1;
	END IF;	
/***** OPCION 41 *****/
	IF (:opcion = 41) THEN
		SELECT * FROM "OCRD" WHERE "CardCode" = :param1;
	END IF;	
/***** OPCION 42 *****/
	IF (:opcion = 42) THEN
		SELECT * FROM "CRD1" WHERE "CardCode" = :param1 AND "AdresType" = 'S';
	END IF;
/***** OPCION 43 *****/
	IF (:opcion = 43) THEN
		SELECT * FROM "OADM" WHERE "CompnyName" = :param1;
	END IF;	
/***** OPCION 44 *****/
	IF (:opcion = 44) THEN
		SELECT * FROM "@FACE_RESOLUCION" WHERE "U_SERIE" = :param1;
	END IF;	
/***** OPCION 45 *****/
	IF (:opcion = 45) THEN
		select * from "ADM1";
	END IF;
/***** OPCION 46 *****/
	IF (:opcion = 46) THEN
		select "Code" from "@FACE_TIPODOC" where "U_CODIGO" = :param1;
	END IF;	
/***** OPCION 47 *****/
	IF (:opcion = 47) THEN
		--select ifnull("Name",'Guatemala')  from "@MUNICIPIO" where "Code"=:param1;
	END IF;	
/***** OPCION 48 *****/
	IF (:opcion = 48) THEN
		--select ifnull("Name",'Guatemala')  from "@DEPARTAMENTO" where "Code"= :param1;
	END IF;
/***** OPCION 49 *****/
	IF (:opcion = 49) THEN
		SELECT * FROM "RIN1" WHERE "DocEntry" = :param1;
	END IF;
/***** OPCION 50 *****/
	IF (:opcion = 50) THEN
		SELECT * FROM "INV1" WHERE "DocEntry" = :param1;		
	END IF;	
/***** OPCION 51 *****/
	IF (:opcion = 51) THEN
		--select "U_FACTURA" from "@UNIDADDEMEDIDA" where "Code"= :param1;
	END IF;
/***** OPCION 52 *****/
	IF (:opcion = 52) THEN
		update "OINV" O set O."U_NUMERO_DOCUMENTO" = :param1,O."U_FIRMA_ELETRONICA"=:param2, O."U_ESTADO_FACE"='A' WHERE O."DocEntry"=:param3;
	END IF;	
/***** OPCION 53 *****/
	IF (:opcion = 53) THEN
		update "ORIN" O set O."U_NUMERO_DOCUMENTO" = :param1,O."U_FIRMA_ELETRONICA"=:param2, O."U_ESTADO_FACE"='A' WHERE O."DocEntry"=:param3;
	END IF;	
/***** OPCION 54 *****/
	IF (:opcion = 54) THEN
		update "OINV" O set O."U_ESTADO_FACE" ='R', O."U_MOTIVO_RECHAZO"=:param1 WHERE O."DocEntry"=:param2;
	END IF;	
/***** OPCION 55 *****/
	IF (:opcion = 55) THEN
		update "ORIN" O set O."U_ESTADO_FACE" ='R', O."U_MOTIVO_RECHAZO"=:param1 WHERE O."DocEntry"=:param2;
	END IF;		
/***** OPCION 56 *****/
	IF (:opcion = 56) THEN
		select * from "@FACE_RESOLUCION" where "U_SERIE" = :param1 AND IFNULL("U_ES_BATCH",'N') = 'N';
	END IF;	
/***** OPCION 57 *****/
	IF (:opcion = 57) THEN
		select * from "@FACE_RESOLUCION" where "U_SERIE" = :param1 AND IFNULL("U_ES_BATCH",'N') = 'Y';		
	END IF;	
/***** OPCION 58 *****/
	IF (:opcion = 58) THEN
		select * from "OINV" WHERE  "DocEntry" =:param1;
	END IF;	
/***** OPCION 59 *****/
	IF (:opcion = 59) THEN
		select * from "ORIN" WHERE  "DocEntry" =:param1;
	END IF;		
/***** OPCION 60 *****/
	IF (:opcion = 60) THEN
		select * from "@FACE_PARAMETROS" where "U_PARAMETRO"=:param1;
	END IF;	
/***** OPCION 61 *****/
	IF (:opcion = 61) THEN
		select ifnull("U_USUARIO",'N/A') AS "usuario", ifnull("U_CLAVE",'N/A') AS "clave" from "@FACE_RESOLUCION" where "U_SERIE" = :param1;
	END IF;	
/***** OPCION 62 *****/
	IF (:opcion = 62) THEN
		Select "DocEntry" from "ORIN" where "Series"=:param1 and "UserSign" =:param2 and TO_NVARCHAR("UpdateDate") = :param3 and "DocTotal"= :param4 and "CardCode" =:param5;
	END IF;	
/***** OPCION 63 *****/
	IF (:opcion = 63) THEN
		Select "DocEntry" from "OINV" where "Series"=:param1 and "UserSign" =:param2 and TO_NVARCHAR("UpdateDate") = :param3 and "DocTotal"= :param4 and "CardCode" =:param5;
	END IF;	
END;