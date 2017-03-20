CREATE FUNCTION UFN_VALORPARAMETRO(IN val nvarchar(300)) 
RETURNS a nvarchar(300)
LANGUAGE SQLSCRIPT READS SQL DATA 
AS 
BEGIN 

select "U_VALOR" 
INTO a 
	from "@FACE_PARAMETROS" 
	where "U_PARAMETRO" = :val;

--    SELECT sso_obtieneserieCV('1010101') from dummy
END;