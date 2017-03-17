CREATE FUNCTION [dbo].[ufn_ValorParametro](@Parametro varchar(300))
RETURNS varchar(300)
AS
begin
	declare @result varchar(300)
	
	select @Result=U_VALOR 
	from [@FACE_PARAMETROS] 
	where U_PARAMETRO =@Parametro 
	return @result 
end