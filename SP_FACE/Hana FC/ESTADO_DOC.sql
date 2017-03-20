CREATE FUNCTION ESTADO_DOC (IN val nvarchar(10)) 
RETURNS a nvarchar(10)
LANGUAGE SQLSCRIPT READS SQL DATA 
AS 
stat nvarchar(10);
BEGIN 

SELECT "CANCELED"
into stat
	FROM OINV
	WHERE "DocEntry" = :val;
  
	IF (:stat = 'N') THEN
	select 'ACTIVO' as a into a from dummy;
	ELSE
	select 'ANULADO' as a into a from dummy;
	END IF;
		
	
--    SELECT ESTADO_DOC(15) FROM DUMMY
END;