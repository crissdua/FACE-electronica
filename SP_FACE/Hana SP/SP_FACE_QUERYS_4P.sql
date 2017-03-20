CREATE PROCEDURE SP_FACE_QUERYS_4P
(IN
condicion int,
param1 NVARCHAR(100),
param2 NVARCHAR(100),
param3 NVARCHAR(100),
param4 NVARCHAR(100)
)
LANGUAGE SQLSCRIPT
AS

BEGIN
/***** CONDICION 1 *****/
	IF ( :condicion=1) THEN
		insert into "@FACE_TIPODOC" ("Code","LineId","U_CODIGO","U_DESCRIPCION") values(:param1,:param2,:param3,:param4);
	END IF;
	
	
/***** CONDICION 2 *****/
	IF (:condicion = 2) THEN
		select 
		case IFNULL(A."U_ESTADO_FACE",'P') when 'P' then 'Pendiente' when 'R' then 'Rechazado' when 'A' then 'Autorizado' end AS "Estado",
		A."DocEntry" AS "Correlativo",
		case A."DocSubType" when (char(45)||char(45)) then 'Factura' when 'DN' then 'Nota Debito' End as "Tipo Documento",
        "DocNum" as "No. Documento",
        TO_NVARCHAR("DocDate") AS "Fecha Documento" ,
        "CardName" AS "Cliente",
        TO_DECIMAL("DocTotal",25,3) AS "Total Documento" 
        from OINV A
        inner join NNM1 B
        on A."Series" = B."Series"
        where ifnull("U_ESTADO_FACE",'P') in ('P','R')
        and A."DocDate" between :param1 and :param2
        and   b."Series" =  :param3
        union 
        select 
        case ifnull(A."U_ESTADO_FACE",'P') when 'P' then 'Pendiente' when 'R' then 'Rechazado' when 'A' then 'Autorizado' end AS "Estado" ,
        A."DocEntry" AS "Correlativo",
        'Nota Credito' AS "Tipo Documento",
        "DocNum" AS "No. Documento",
        TO_NVARCHAR("DocDate") AS "Fecha Documento" ,
        "CardName" AS "Cliente",
        TO_DECIMAL("DocTotal",25,3) AS "Total Documento"
        from ORIN  A inner join NNM1 B on A."Series" = B."Series"
        where ifnull("U_ESTADO_FACE",'P') in ('P','R')
        and A."DocDate" between :param1 and :param2
        and B."Series" = :param3                      
        order by "Correlativo" desc;
	END IF;
	
/***** CONDICION 3 *****/
	IF (:condicion = 3) THEN
		select 
		case IFNULL(A."U_ESTADO_FACE",'P') when 'P' then 'Pendiente' when 'R' then 'Rechazado' when 'A' then 'Autorizado' end AS "Estado",
		A."DocEntry" AS "Correlativo",
		case A."DocSubType" when (char(45)||char(45)) then 'Factura' when 'DN' then 'Nota Debito' End as "Tipo Documento",
        "DocNum" as "No. Documento",
        TO_NVARCHAR("DocDate") AS "Fecha Documento" ,
        "CardName" AS "Cliente",
        TO_DECIMAL("DocTotal",25,3) AS "Total Documento" 
        from OINV A
        inner join NNM1 B
        on A."Series" = B."Series"
        where ifnull(U_ESTADO_FACE,'P')='P' 
        and A."DocDate" between :param1 and :param2
        and   b."Series" =  :param3
        union 
        select 
        case ifnull(A."U_ESTADO_FACE",'P') when 'P' then 'Pendiente' when 'R' then 'Rechazado' when 'A' then 'Autorizado' end AS "Estado" ,
        A."DocEntry" AS "Correlativo",
        'Nota Credito' AS "Tipo Documento",
        "DocNum" AS "No. Documento",
        TO_NVARCHAR("DocDate") AS "Fecha Documento" ,
        "CardName" AS "Cliente",
        TO_DECIMAL("DocTotal",25,3) AS "Total Documento"
        from ORIN  A inner join NNM1 B on A."Series" = B."Series"
        where ifnull(U_ESTADO_FACE,'P')='P'
        and A."DocDate" between :param1 and :param2
        and   B."Series" = :param3                      
        order by "Correlativo" desc;
	END IF;

/***** CONDICION 4 *****/
	IF (:condicion = 4) THEN
		select "U_MOTIVO_RECHAZO"  from OINV  where "Series"= :param1 and "DocNum" = :param2 and "DocSubType" = :param3;	
	END IF;

/***** CONDICION 5 *****/
	IF (:condicion = 5) THEN
		select "U_MOTIVO_RECHAZO"  from OINV  where "Series"= :param1 and "DocNum" = :param2;		
	END IF;
	
/***** CONDICION 6 *****/
	IF (:condicion = 6) THEN
		select A."Series" AS "Codigo Serie",
		"SeriesName" AS "Serie Documento", 
		"DocNum" AS "No. Documento",
		TO_NVARCHAR("DocDate") AS "Fecha Documento" ,
		"CardName" AS "Cliente",
		TO_DECIMAL("DocTotal",25,3) AS "Total Documento"
        from OINV A
        inner join NNM1 B on A."Series" = B."Series"
        where "U_ESTADO_FACE"= :param1
        AND A."DocSubType" = :param2
        and A."DocDate" between :param3 and :param4
        order by A."DocDate" desc;		
	END IF;
	
/***** CONDICION 7 *****/
	IF (:condicion = 7) THEN
		select A."Series" AS "Codigo Serie",
		"SeriesName" AS "Serie Documento", 
		"DocNum" AS "No. Documento",
		TO_NVARCHAR("DocDate") AS "Fecha Documento" ,
		"CardName" AS "Cliente",
		TO_DECIMAL("DocTotal",25,3) AS "Total Documento"
        from OINV A
        inner join NNM1 B on A."Series" = B."Series"
        where "U_ESTADO_FACE"= :param1
        AND A."DocSubType" = char(45)||char(45)
        and A."DocDate" between :param3 and :param4
        order by A."DocDate" desc;		
	END IF;
END;