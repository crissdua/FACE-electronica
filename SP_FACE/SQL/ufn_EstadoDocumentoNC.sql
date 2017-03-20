CREATE FUNCTION [dbo].[ufn_EstadoDocumentoNC](@DocEntry int)
RETURNS char(1)
AS
begin
	DECLARE @Result CHAR(1)
	select @result=U_DocstatusCC from ORIN where DocEntry =@DocEntry
    return @result 
end