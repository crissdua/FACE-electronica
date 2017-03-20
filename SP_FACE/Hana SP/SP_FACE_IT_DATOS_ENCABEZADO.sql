CREATE procedure SP_FACE_IT_DATOS_ENCABEZADO 
(in Docentry int,
Tipo char(3)
)

AS
BEGIN	--SET ARITHABORT ON
		 
		IF (:Tipo='FAC' or :Tipo='ND') THEN
		Select DISTINCT 
				ifnull(FA."U_DISPOSITIVO",'N/D') as "DISPOSITIVO",
				(SELECT ESTADO_DOC((:Docentry)) FROM DUMMY) AS  "ESTADO_DOCUMENTO",
				CASE O."DocCur" WHEN 'QTZ' THEN 'GTQ' ELSE 'USD' END AS "CODIGO_MONEDA",
				(SELECT TIPODOC_GFACE(FA."U_TIPO_DOC") FROM DUMMY) AS "TIPO_DOCUMENTO",
				CASE O."LicTradNum"
                       When '000000000C.F.' Then 'C.F'
                       When '0000000000C/F' Then 'C.F'
                       Else SUBSTRING(replace(ifnull(O."LicTradNum",'C.F'),'-',''), LOCATE('%[^0 ]%', replace(ifnull(O."LicTradNum",'C.F'),'-','') || ' '), LENGTH(replace(ifnull(O."LicTradNum",'C.F'),'-','')))
                End AS "NIT_COMPRADOR",
				ifnull((select UFN_VALORPARAMETRO('NIT') FROM DUMMY),'N/D') AS "NIT_VENDEDOR",
				IFNULL(N."Remark",N."SeriesName") AS "SERIE_AUTORIZADA",
				O."DocTotal" AS "TOTAL_DOCUMENTO",
				TO_NVARCHAR(O."DocDate") AS "FECHA_DOCUMENTO",
				IFNULL(R."DocDate",(CURRENT_DATE)) AS "FECHA_ANULACION",
				IFNULL(O."Comments",'N/D') AS "OBSERVACIONES",
				IFNULL(OC."Phone1",'N/D') AS "TELEFONO_COMPRADOR",
				'0.0' AS "IMPORTE_DESCUENTO",
				(SELECT TOTAL_EXENTO(:Docentry,:Tipo)FROM DUMMY) AS "TOTAL_EXENTO",
				O."DocTotal" AS "IMPORTE_NETO_GRAVADO",
				O."VatSum" AS "DETALLE_IMPUESTO_IVA",
				ifnull(O."DocRate",0) AS "TIPO_CAMBIO",
				IFNULL(O."Address", 'CIUDAD') AS "DIRECCION_COMPRADOR",
				--'CIUDAD' DIRECCION_COMPRADOR,
				'0.0' AS "IMPORTE_OTROS_IMPUESTOS",
				ifnull(FA."U_RESOLUCION",'N/D') AS "NUMERO_RESOLUCION",
				ifnull(OC."City",'Guatemala') AS "MUNICIPIO_COMPRADOR",
				ifnull(OC."County",'Guatemala') AS "DEPARTAMENTO_COMPRADOR",
				ifnull(O."CardName",'Consumidor Final') AS "NOMBRE_COMPRADOR",
				ifnull((select UFN_VALORPARAMETRO('NOMC')FROM DUMMY),'N/D') AS "NOMBRE_VENDEDOR",
				--CASE WHEN OINV.SERIES IN(84,371) THEN 'Guatemala' ELSE 'Mazatenando' end  MUNICIPIO_VENDEDOR,
				--CASE WHEN OINV.SERIES NOT IN(84,371) THEN 'Guatemala' ELSE 'Suchitepequez' end DEPARTAMENTO_VENDEDOR,
				ifnull(TO_NVARCHAR(FA."U_MUNI_SUCURSAL"),'Guatemala') AS "MUNICIPIO_VENDEDOR",
				ifnull(TO_NVARCHAR(FA."U_DEPTO_SUCURSAL"),'Guatemala') AS "DEPARTAMENTO_VENDEDOR",
				ifnull((select UFN_VALORPARAMETRO('DIRE') FROM DUMMY),'N/D') AS "DIRECCION_VENDEDOR",
				replace(TO_NVARCHAR(FA."U_FECHA_AUTORIZACION"),'/','-') AS "FECHA_RESOLUCION",
				'RET_DEFINITIVA' AS "REGIMEN_ISR",
				(O."DocTotal" - O."VatSum") AS "IMPORTE_BRUTO",
				'12521337' AS "NIT_GFACE",
				ifnull(FA."U_SUCURSAL",'N/D') AS "CODIGO_SUCURSAL",
				CASE IFNULL(OC."E_Mail",'N/D') WHEN 'N/D' THEN 'N/D' ELSE (OC."E_Mail") END as "CORREO_COMPRADOR",
				'N/D' AS "DESCRIPCION_OTROS_IMPUESTOS",
				CASE O."Series" WHEN 29 THEN O."DocNum" ELSE (SELECT CORRIGEDOC_NUM(O."DocNum") FROM DUMMY) END AS "NUMERO_DOCUMENTO",
				O."DocType" AS "TIPO_DOC",
				'' AS "PERSONALIZADO_1",
				'' AS "PERSONALIZADO_2",
				'' AS "PERSONALIZADO_3",
				'' AS "PERSONALIZADO_4",
				'' AS "PERSONALIZADO_5",
				'' AS "PERSONALIZADO_6",
				'' AS "PERSONALIZADO_7",
				'' AS "PERSONALIZADO_8",
				'' AS "PERSONALIZADO_9",
				'' AS "PERSONALIZADO_10",
				'' AS "PERSONALIZADO_11",
				'' AS "PERSONALIZADO_12",
				'' AS "PERSONALIZADO_13",
				'' AS "PERSONALIZADO_14",
				'' AS "PERSONALIZADO_15",
				'' AS "PERSONALIZADO_16",
				'' AS "PERSONALIZADO_17",
				'' AS "PERSONALIZADO_18",
				'' AS "PERSONALIZADO_19",
				'' AS "PERSONALIZADO_20"					
			from "OINV" O
				left outer join "@FACE_RESOLUCION" FA
				on FA."U_SERIE"=O."Series"
				left outer join  "OCRD" OC
				on OC."CardCode" =O."CardCode"
				INNER JOIN "NNM1" N on O."Series" =N."Series" 
				LEFT OUTER JOIN "CRD1" CR
				ON OC."CardCode"=CR."CardCode"
				LEFT OUTER JOIN "RIN1" R
				ON O."DocNum" =R."BaseDocNum"
				AND R. "BaseType"=13
			where O."DocEntry" = :Docentry;
		END IF;
	IF (:Tipo='NC') THEN
			Select DISTINCT 
				ifnull(FA."U_DISPOSITIVO",'N/D') AS "DISPOSITIVO",
				'ACTIVO' AS "ESTADO_DOCUMENTO",
				CASE O."DocCur" WHEN 'QTZ' THEN 'GTQ' ELSE 'USD' END AS "CODIGO_MONEDA",
				(SELECT TIPODOC_GFACE(FA."U_TIPO_DOC") FROM DUMMY) AS "TIPO_DOCUMENTO",
                CASE O."LicTradNum"
                       When '000000000C.F.' Then 'C.F'
                       When '0000000000C/F' Then 'C.F'
                       Else SUBSTRING(replace(ifnull(O."LicTradNum",'C.F'),'-',''), LOCATE('%[^0 ]%', replace(ifnull(O."LicTradNum",'C.F'),'-','') || ' '), LENGTH(replace(ifnull(O."LicTradNum",'C.F'),'-','')))
                End AS "NIT_COMPRADOR",
				ifnull((select UFN_VALORPARAMETRO('NIT') FROM DUMMY),'N/D') AS "NIT_VENDEDOR",
				IFNULL(N."Remark",N."SeriesName") AS "SERIE_AUTORIZADA",
				O."DocTotal" AS "TOTAL_DOCUMENTO",
				TO_NVARCHAR(O."DocDate") AS "FECHA_DOCUMENTO",
				(CURRENT_DATE) AS "FECHA_ANULACION",
				IFNULL(O."Comments",'N/D') AS "OBSERVACIONES",
				IFNULL(OC."Phone1",'N/D') AS "TELEFONO_COMPRADOR",
				'0.0' AS "IMPORTE_DESCUENTO",
				(SELECT TOTAL_EXENTO(:Docentry,:Tipo) FROM DUMMY) AS "TOTAL_EXENTO",
				O."DocTotal" AS "IMPORTE_NETO_GRAVADO",
				O."VatSum" AS "DETALLE_IMPUESTO_IVA",
				ifnull(O."DocRate",0) AS "TIPO_CAMBIO",
				IFNULL(O."Address", 'CIUDAD') AS "DIRECCION_COMPRADOR",
				'0.0' AS "IMPORTE_OTROS_IMPUESTOS",
				ifnull(FA."U_RESOLUCION",'N/D') AS "NUMERO_RESOLUCION",
				ifnull(OC."City",'Guatemala') AS "MUNICIPIO_COMPRADOR",
				ifnull(OC."County",'Guatemala') AS "DEPARTAMENTO_COMPRADOR",
				ifnull(O."CardName",'Consumidor Final') AS "NOMBRE_COMPRADOR",
				ifnull((select UFN_VALORPARAMETRO('NOMC') FROM DUMMY) ,'N/D') AS "NOMBRE_VENDEDOR",
				ifnull(TO_NVARCHAR(FA."U_MUNI_SUCURSAL"),'Guatemala') AS "MUNICIPIO_VENDEDOR",
				ifnull(TO_NVARCHAR(FA."U_DEPTO_SUCURSAL"),'Guatemala') AS "DEPARTAMENTO_VENDEDOR",
				ifnull((select UFN_VALORPARAMETRO('DIRE') FROM DUMMY) ,'N/D') AS "DIRECCION_VENDEDOR",
				replace(TO_NVARCHAR(FA."U_FECHA_AUTORIZACION"),'/','-') AS "FECHA_RESOLUCION",
				'RET_DEFINITIVA' AS "REGIMEN_ISR",
				(O."DocTotal" - O."VatSum") AS "IMPORTE_BRUTO",
				'12521337' AS "NIT_GFACE",
				ifnull(FA."U_SUCURSAL",'N/D') AS "CODIGO_SUCURSAL",
				CASE IFNULL(OC."E_Mail",'N/D') WHEN 'N/D' THEN 'N/D' ELSE OC."E_Mail" END AS "CORREO_COMPRADOR",
				'N/D' AS "DESCRIPCION_OTROS_IMPUESTOS",
				CASE O."Series" WHEN 29 THEN O."DocNum" ELSE (SELECT CORRIGEDOC_NUM(O."DocNum") FROM DUMMY) END AS "NUMERO_DOCUMENTO",
				O."DocType" AS "TIPO_DOC",
				'' AS "PERSONALIZADO_1",
				'' AS "PERSONALIZADO_2",
				'' AS "PERSONALIZADO_3",
				'' AS "PERSONALIZADO_4",
				'' AS "PERSONALIZADO_5",
				'' AS "PERSONALIZADO_6",
				'' AS "PERSONALIZADO_7",
				'' AS "PERSONALIZADO_8",
				'' AS "PERSONALIZADO_9",
				'' AS "PERSONALIZADO_10",
				'' AS "PERSONALIZADO_11",
				'' AS "PERSONALIZADO_12",
				'' AS "PERSONALIZADO_13",
				'' AS "PERSONALIZADO_14",
				'' AS "PERSONALIZADO_15",
				'' AS "PERSONALIZADO_16",
				'' AS "PERSONALIZADO_17",
				'' AS "PERSONALIZADO_18",
				'' AS "PERSONALIZADO_19",
				'' AS "PERSONALIZADO_20"						
			from "ORIN" ORI, "OINV" O
				left outer join "@FACE_RESOLUCION" FA
				on FA."U_SERIE"=O."Series"
				left outer join "OCRD" OC 
				on OC."CardCode" = O."CardCode"
				INNER JOIN "NNM1" N on O."Series" =N."Series" 
				LEFT OUTER JOIN "CRD1" CR
				ON OC."CardCode" = CR."CardCode"
				where O."DocEntry" =:Docentry;
			END IF;
	END;