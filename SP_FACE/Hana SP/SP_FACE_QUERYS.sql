CREATE PROCEDURE SP_FACE_QUERYS
(IN
condicion int,
param1 NVARCHAR(100),
param2 NVARCHAR(100)
)
LANGUAGE SQLSCRIPT
AS

BEGIN
--Condicion 1 = AddUserFields
--Condicion 2 = delete @Face_tipodoc
--Condicion 3 y 4 = addDocumentType
--Condicion 5 = select parametros
--Condicion 6 = inserts

/***** CONDICION 1 *****/
--TableID = TableName
--AliasID = FieldName
	IF (:condicion = 1) THEN 
		SELECT "TableID","FieldID","AliasID" FROM "CUFD" WHERE "TableID"= :param1 AND "AliasID"  = :param2;
	END IF;
/***** CONDICION 2 *****/
	IF (:condicion = 2) THEN
		DELETE FROM "@FACE_TIPODOC";
	END IF;
/***** CONDICION 3 y 4 *****/
	IF (:condicion = 3) THEN
		SELECT * FROM "@FACE_TIPODOC" WHERE "U_CODIGO"= :param1 AND "Code"= :param2;
	END IF;
	IF (:condicion = 4) THEN
		SELECT * FROM "@FACE_TIPODOC" WHERE "U_CODIGO"= :param1;
	END IF;
/***** CONDICION 5 *****/
	IF (:condicion = 5) THEN
		SELECT * FROM "@FACE_PARAMETROS";
	END IF;
/***** CONDICION 6 *****/
	IF (:condicion = 6) THEN
				insert into "@FACE_PARAMETROS" values('ASS',0,-3,0,'ASS',null);
                insert into "@FACE_PARAMETROS" values('CODE',1,-3,1,'CODE',null);
                insert into "@FACE_PARAMETROS" values('DIE',2,-3,2,'DIE',null);
                insert into "@FACE_PARAMETROS" values('DIRE',3,-3,3,'DIRE',null);
                insert into "@FACE_PARAMETROS" values('EMAILF',4,-3,4,'EMAILF',null);
                insert into "@FACE_PARAMETROS" values('IENT',5,-3,5,'IENT',null);
                insert into "@FACE_PARAMETROS" values('IFACE',6,-3,6,'IFACE',null);
                insert into "@FACE_PARAMETROS" values('IUSR',7,-3,7,'IUSR',null);
                insert into "@FACE_PARAMETROS" values('IUSRN',8,-3,8,'IUSRN',null);
                insert into "@FACE_PARAMETROS" values('NIT',9,-3,9,'NIT',null);
                insert into "@FACE_PARAMETROS" values('NOMC',10,-3,10,'NOMC',null);
                insert into "@FACE_PARAMETROS" values('NOME',11,-3,11,'NOME',null);
                insert into "@FACE_PARAMETROS" values('OFFL',12,-3,12,'OFFL',null);
                insert into "@FACE_PARAMETROS" values('PASSDB',0,-3,13,'PASSDB',null);
                insert into "@FACE_PARAMETROS" values('PATHPDF',0,-3,14,'PATHPDF',null);
                insert into "@FACE_PARAMETROS" values('PATHXML',0,-3,15,'PATHXML',null);
                insert into "@FACE_PARAMETROS" values('PREFIX',0,-3,16,'PREFIX',null);
                insert into "@FACE_PARAMETROS" values('PRINTB',0,-3,17,'PRINTB',null);
                insert into "@FACE_PARAMETROS" values('URLWS',0,-3,18,'URLWS',null);
                insert into "@FACE_PARAMETROS" values('USRDB',0,-3,19,'USRDB',null);
	END IF;
/***** CONDICION 7 *****/
	IF (:condicion = 7) THEN
		DELETE FROM "@FACE_RESOLUCION";
	END IF;
/***** CONDICION 8 *****/
	IF (:condicion = 8) THEN
		select "U_CODIGO","U_DESCRIPCION" from "@FACE_TIPODOC";
	END IF;

/***** CONDICION 9 *****/
	IF (:condicion = 9) THEN
		select 0 as "Series", 'Todos' as "SeriesName" FROM DUMMY
		union 
		select A."Series", A."SeriesName" ||' ('|| Case A."ObjectCode" WHEN 13 THEN   
		'Factura' WHEN 14  THEN 
		'Nota Credito' WHEN 18 THEN 
		'Factura Proveedor'WHEN 4 THEN  
		'Manual' WHEN 2 THEN
		'Manual' ELSE  
		'Nota Debito'  END||')' as "SeriesName" 
		from NNM1 A  
		inner join "@FACE_RESOLUCION" B  on A."Series"=B."U_SERIE" 
		where ifnull(B."U_ES_BATCH",'N')='Y';
	END IF;
	
/***** CONDICION 10 *****/
	IF (:condicion = 10) THEN
		select * from "@FACE_PARAMETROS" where "U_PARAMETRO" = :param1;
	END IF;
	
/***** CONDICION 11 *****/
	IF (:condicion = 11) THEN
		update "@FACE_PARAMETROS" set "U_VALOR"= :param1 where "U_PARAMETRO"= :param2;
	END IF;
	
/***** CONDICION 12 *****/
	IF (:condicion = 12) THEN
		select "U_FACE_PDFFILE"  from OINV WHERE "Series"= :param1 aND "DocNum" =:param2 AND "DocSubType" = char(45)||char(45);
	END IF;

/***** CONDICION 13 *****/
	IF (:condicion = 13) THEN
		select "U_FACE_PDFFILE"  from OINV WHERE "Series"= :param1 aND "DocNum" =:param2 AND "DocSubType" = 'DN';		
	END IF;

/***** CONDICION 14 *****/
	IF (:condicion = 14) THEN
		select "U_FACE_PDFFILE"  from OINV WHERE "Series"= :param1 aND "DocNum" =:param2;		
	END IF;
	
/***** CONDICION 15 *****/
	IF (:condicion = 15) THEN
		select A."U_TIPO_DOC" as "Tipo",B."Series",B."DocNum",C."SeriesName",B."DocEntry"
        from "@FACE_RESOLUCION" A
        inner join OINV B 
        on A."U_SERIE" = B."Series"
        inner join NNM1 C 
        on B."Series"=C."Series" 
        where A."U_SERIE" =B."Series" And B."DocEntry" = :param1;	
	END IF;
	
/***** CONDICION 16 *****/
	IF (:condicion = 16) THEN
		select 'Y' AS "Seleccionar",
		TO_ALPHANUM(A."U_MOTIVO_RECHAZO") AS "Descripcion Rechazó",
		A."DocEntry" AS "Correlativo",
		case A."DocSubType" when (char(45)||char(45)) then 'Factura' when 'DN' then 'Nota Debito' End AS "Tipo Documento" ,
		"SeriesName" as "Serie Documento",
        "DocNum" as "No. Documento",
        TO_NVARCHAR("DocDate") AS "Fecha Documento" ,
        "CardName" AS "Cliente",
        TO_DECIMAL("DocTotal",25,3) AS "Total Documento"
        from OINV A
        inner join NNM1 B on A."Series" = B."Series"
        where "U_ESTADO_FACE" ='R'
        union 
        select 'Y' AS "Seleccionar",
        TO_ALPHANUM(A."U_MOTIVO_RECHAZO") AS "Descripcion Rechazó",
        A."DocEntry" AS "Correlativo",
        '' AS "Nota Credito",
        "SeriesName" AS "Serie Documento", 
        "DocNum" AS "No. Documento",
        TO_NVARCHAR("DocDate") AS "Fecha Documento" ,
        "CardName" AS "Cliente",
        TO_DECIMAL("DocTotal",25,3) AS "Total Documento"
        from ORIN  A 
        inner join NNM1 B on A."Series" = B."Series"
        where "U_ESTADO_FACE"='R'
        order by "Correlativo" desc;		
	END IF;

/***** CONDICION 17 *****/
	IF (:condicion = 17) THEN
		select 'Y' AS "Seleccionar",
		TO_ALPHANUM(A."U_MOTIVO_RECHAZO") AS "Descripcion Rechazó",
		A."DocEntry" AS "Correlativo",
		case A."DocSubType" when (char(45)||char(45)) then 'Factura' when 'DN' then 'Nota Debito' End AS "Tipo Documento" ,
		"SeriesName" as "Serie Documento",
        "DocNum" as "No. Documento",
        TO_NVARCHAR("DocDate") AS "Fecha Documento" ,
        "CardName" AS "Cliente",
        TO_DECIMAL("DocTotal",25,3) AS "Total Documento"
        from OINV A
        inner join NNM1 B on A."Series" = B."Series"
        where "U_ESTADO_FACE" ='R'
        and "DocDate" between :param1 and :param2 
         union 
        select 'Y' AS "Seleccionar",
        TO_ALPHANUM(A."U_MOTIVO_RECHAZO") AS "Descripcion Rechazó",
        A."DocEntry" AS "Correlativo",
        '' AS "Nota Credito",
        "SeriesName" AS "Serie Documento", 
        "DocNum" AS "No. Documento",
        TO_NVARCHAR("DocDate") AS "Fecha Documento" ,
        "CardName" AS "Cliente",
        TO_DECIMAL("DocTotal",25,3) AS "Total Documento"
        from ORIN  A 
        inner join NNM1 B on A."Series" = B."Series"
        where "U_ESTADO_FACE"='R'
        and "DocDate" between :param1 and :param2 
        order by "Correlativo" desc;			
	END IF;

/***** CONDICION 18 *****/
	IF (:condicion = 18) THEN
		select "TableID","FieldID","AliasID" from "CUFD" WHERE "TableID"= :param1 and "AliasID"  = :param2;	
	END IF;
	
END;