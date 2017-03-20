CREATE FUNCTION TOTAL_EXENTO(IN DOCENTRY NVARCHAR(20), TIPO_DOC CHAR(4)) 
RETURNS a DECIMAL(18,4)
LANGUAGE SQLSCRIPT READS SQL DATA 
AS 
BEGIN 

	IF (:TIPO_DOC='FAC' OR :TIPO_DOC='ND') THEN
		--select @RESULT=isnull(sum( case doctype when 'S' then 1 else b.Quantity end * b.PriceAfVat),0)  Cambio asandoval para reportar en exento lo requerido por wmontenegro 29/10/14
		select iFnull(sum(B."LineTotal"),0) INTO a
		from OINV A
		inner join INV1 B
		on A."DocEntry"=B."DocEntry"
		where A."DocEntry" =  :DOCENTRY
		and   B."TaxCode"='EXE';
	ELSE
		--select @RESULT=isnull(sum( case doctype when 'S' then 1 else b.Quantity end * b.PriceAfVat),0)
		select ifnull(sum(B."LineTotal"),0) INTO a
		from ORIN A
		inner join RIN1  B
		on A."DocEntry"=B."DocEntry"
		where A."DocEntry" = :DOCENTRY
		and   B."TaxCode"='EXE';
    END IF;
--    SELECT sso_obtieneserieCV('1010101') from dummy
END;