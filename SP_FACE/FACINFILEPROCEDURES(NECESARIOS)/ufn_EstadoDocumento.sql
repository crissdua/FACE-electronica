CREATE FUNCTION [dbo].[ufn_EstadoDocumento](@DocEntry int)
RETURNS char(1)
AS
begin
	DECLARE @DocEntryNC int
	DECLARE @SerieNC int
	DECLARE @Result CHAR(1)

	select distinct @DocEntryNC=isnull(TrgetEntry,0) 
	from INV1
	where DocEntry =@DocEntry and isnull(TrgetEntry,0) <>0
	

	SELECT @SerieNC=B.Series 
	FROM NNM1 A
	INNER JOIN ORIN B
	ON A.Series = B.Series 
	WHERE DocEntry =@DocEntryNC

	IF @SerieNC=3 OR @SerieNC=418 
		set @Result='A'
	ELSE
		set @Result='V';

      return @result 
end