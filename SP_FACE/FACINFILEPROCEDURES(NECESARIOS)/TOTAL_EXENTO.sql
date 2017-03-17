CREATE FUNCTION [dbo].[TOTAL_EXENTO](@DOCENTRY VARCHAR(20),@TIPO_DOC CHAR(4)) RETURNS DECIMAL(18,4) 
AS 
BEGin 
	DECLARE @RESULT DECIMAL(18,4)
	
	IF @TIPO_DOC='FAC' OR @TIPO_DOC='ND'
		--select @RESULT=isnull(sum( case doctype when 'S' then 1 else b.Quantity end * b.PriceAfVat),0)  Cambio asandoval para reportar en exento lo requerido por wmontenegro 29/10/14
		select @RESULT=isnull(sum(LineTotal),0)
		from oinv a
		inner join INV1 b
		on a.DocEntry=b.docentry
		where a.docentry =  @DOCENTRY
		and   b.TaxCode='EXE'
	ELSE
		--select @RESULT=isnull(sum( case doctype when 'S' then 1 else b.Quantity end * b.PriceAfVat),0)
		select @RESULT=isnull(sum(LineTotal),0)
		from ORIN a
		inner join RIN1  b
		on a.DocEntry=b.docentry
		where a.docentry =  @DOCENTRY
		and   b.TaxCode='EXE'
			
	RETURN @RESULT 
END