CREATE FUNCTION [dbo].[ESTADO_DOC](@docentry int) RETURNS varchar(10) 
AS 
BEGin
 
/*  DECLARE @COUNT INT 
 DECLARE @docnum int */
	DECLARE @RESULT VARCHAR(10)
	DECLARE @STATUS VARCHAR(10)
 
/*  select @docnum = docnum 
 from OINV 
 where DocEntry =@docentry

 select @COUNT=COUNT(1)
 from OJDT 
 where Ref2=convert(varchar,@docnum) 
 and  TransType =14 
 
 IF @COUNT > 0 
  SET @RESULT= 'ANULADO'
 ELSE
  SET @RESULT= 'ACTIVO' */
  
	SELECT @STATUS=CANCELED
	FROM OINV
	WHERE DocEntry = @docentry
  
	IF @STATUS = 'N'
		SET @RESULT = 'ACTIVO'
	ELSE
		SET @RESULT = 'ANULADO'
 
 RETURN @RESULT
END