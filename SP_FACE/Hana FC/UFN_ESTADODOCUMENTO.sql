CREATE FUNCTION UFN_ESTADODOCUMENTO (IN val INT) 
RETURNS a nvarchar(10)
LANGUAGE SQLSCRIPT READS SQL DATA 
AS 
DocEntryNC int;
SerieNC int;
BEGIN 

	select TOP 1 ifnull("TrgetEntry",'0')
	INTO DocEntryNC
	from INV1
	where "DocEntry" = :val;

	if (:DocEntryNC is null or :DocEntryNC = 0) then
	DocEntryNC := '0';
	else
	SELECT  B."Series" as "Series"
	INTO SerieNC
	FROM NNM1 A
	INNER JOIN ORIN B
	ON A."Series" = B."Series" 
	WHERE "DocEntry" = :DocEntryNC;
	end if;
	IF (:SerieNC = 3 OR :SerieNC = 418) then
	select 'A' as a into a from dummy;
		--Result :='A';
	ELSE
	select 'V' as a into a from dummy;
		--Result :='V';
	END IF;

--    SELECT sso_obtieneserieCV('1010101') from dummy
END;