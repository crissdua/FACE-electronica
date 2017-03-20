CREATE PROCEDURE SPFACE_DATOSDOC
(in DOCENETRY INT)
AS
BEGIN
SELECT
	C."U_TIPO_DOC" as "Tipo",
	B."Series",
	A."DocNum",
	IFNULL(B."BeginStr",B."SeriesName") as "SeriesName", 
	A."DocEntry" 
FROM OINV A
INNER JOIN NNM1 B
ON A."Series"=B."Series" 
OR B."EndStr" = '-1' AND B."ObjectCode" = 13
INNER JOIN "@FACE_RESOLUCION" C
ON C."U_SERIE" =B."Series" 
WHERE A."DocEntry" = :DOCENETRY
UNION 
SELECT
	C."U_TIPO_DOC" as "Tipo",
	B."Series",
	A."DocNum",
	IfNULL(B."BeginStr",B."SeriesName") as "SeriesName", 
	A."DocEntry" 
FROM ORIN A
INNER JOIN NNM1 B
ON A."Series"=B."Series" 
OR B."EndStr" =-1 AND B."ObjectCode" = 14
INNER JOIN "@FACE_RESOLUCION" C
ON C."U_SERIE" =B."Series" 
WHERE A."DocEntry" = :DOCENETRY
UNION
SELECT
	C."U_TIPO_DOC" as "Tipo",
	B."Series",
	A."DocNum",
	IFNULL(B."BeginStr",B."SeriesName") as "SeriesName", 
	A."DocEntry" 
FROM OPCH A
INNER JOIN NNM1 B
ON A."Series"=B."Series" 
OR B."EndStr" =-1 AND B."ObjectCode" = 16
INNER JOIN "@FACE_RESOLUCION" C
ON C."U_SERIE" =B."Series" 
WHERE A."DocEntry" = :DOCENETRY;
END;