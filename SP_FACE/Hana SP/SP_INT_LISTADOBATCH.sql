CREATE procedure SP_INT_LISTADOBATCH 
(in 
Serie int,
FechaIni  nvarchar(10),
FechaFin  nvarchar(10))
AS
BEGIN

IF (:Serie <> 0) THEN 
	select 
	case IFNULL(A."U_ESTADO_FACE",'P') when 'P' then 'Pendiente' when 'R' then 'Rechazado' when 'A' then 'Autorizado' end AS "Estado",
	A."DocEntry" AS "Correlativo",
	case A."DocSubType" when (CHAR(45)||char(45)) then 'Factura' when 'DN' then 'Nota Debito' End AS "Tipo Documento",
	B."SeriesName" AS "Serie", 
	"DocNum" AS "No. Documento", TO_NVARCHAR("DocDate") AS "Fecha Documento" ,"CardName" AS "Cliente",
	TO_DECIMAL("DocTotal",25,3) AS "Total Documento", 
	case (SELECT UFN_ESTADODOCUMENTO(A."DocEntry") from dummy) when 'A' then 'Anulado' else 'Vigente' end AS "Estado del Documento" 
	from OINV A 
	inner join NNM1 B on A."Series" = B."Series" 
	where IFNULL("U_ESTADO_FACE",'P') in ('P','R')  
	--and A."DocDate" between :FechaIni and  :FechaFin
	and   B."Series" = :Serie 
	union 
	select 
	case IFNULL(A."U_ESTADO_FACE",'P') when 'P' then 'Pendiente' when 'R' then 'Rechazado' when 'A' then 'Autorizado' end AS "Estado",
	A."DocEntry" AS "Correlativo",
	'Nota Credito' AS "Tipo Documento",	
	B."SeriesName" AS "Serie",  
	"DocNum" AS "No. Documento",
	TO_NVARCHAR("DocDate") AS "Fecha Documento" ,
	"CardName" AS "Cliente",
	TO_DECIMAL("DocTotal",25,3) AS "Total Documento", 
	case (select "U_DocstatusCC" from ORIN where "DocEntry" = A."DocEntry") when 'A' then 'Anulado' else 'Vigente' end AS "Estado del Documento"   
	from ORIN  A 
	inner join NNM1 B 
	on A."Series" = B."Series"  
	where IFNULL("U_ESTADO_FACE",'P') in ('P','R')  
	and A."DocDate" between :FechaIni and  :FechaFin
	and   B."Series" = :Serie 
	order by "Serie", "Correlativo";
else
	select 
	case IFNULL(A."U_ESTADO_FACE",'P') when 'P' then 'Pendiente' when 'R' then 'Rechazado' when 'A' then 'Autorizado' end AS "Estado",
	A."DocEntry" AS "Correlativo",
	case A."DocSubType" when (char(45)||char(45))then 'Factura' when 'DN' then 'Nota Debito' End as "Tipo Documento", 
	B."SeriesName" AS "Serie", 
	"DocNum" AS "No. Documento",
	TO_NVARCHAR("DocDate") AS "Fecha Documento" ,
	"CardName" AS "Cliente",
	TO_DECIMAL("DocTotal",25,3) AS "Total Documento", 
	case (SELECT UFN_ESTADODOCUMENTO(A."DocEntry") from dummy) when 'A' then 'Anulado' else 'Vigente' end AS "Estado del Documento" 
	from OINV A 
	inner join NNM1 B 
	on A."Series" = B."Series" 
	where IFNULL("U_ESTADO_FACE",'P') in ('P','R')  
	and A."DocDate" between :FechaIni and  :FechaFin
	and   B."Series" in (select "U_SERIE" from "@FACE_RESOLUCION" where "U_ES_BATCH"='Y')
	union 
	select 
	case IFNULL(A."U_ESTADO_FACE",'P') when 'P' then 'Pendiente' when 'R' then 'Rechazado' when 'A' then 'Autorizado' end AS "Estado",
	A."DocEntry" AS "Correlativo",
	'Nota Credito' AS "Tipo Documento",	
	B."SeriesName" AS "Serie",  
	"DocNum" AS "No. Documento",
	TO_NVARCHAR("DocDate") AS "Fecha Documento" ,
	"CardName" AS "Cliente",
	TO_DECIMAL("DocTotal",25,3) AS "Total Documento", 
	case (select "U_DocstatusCC" from ORIN where "DocEntry" = A."DocEntry") when 'A' then 'Anulado' else 'Vigente' end AS "Estado del Documento"   
	from ORIN  A 
	inner join NNM1 B 
	on A."Series" = B."Series"  
	where IFNULL("U_ESTADO_FACE",'P') in ('P','R')  
	and A."DocDate" between :FechaIni and  :FechaFin
	and   B."Series" in (select "U_SERIE" from "@FACE_RESOLUCION" where "U_ES_BATCH"='Y')
	order by "Serie", "Correlativo";
END IF;
END;