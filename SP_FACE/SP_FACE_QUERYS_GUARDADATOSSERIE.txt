CREATE PROCEDURE SP_FACE_QUERYS_GUARDADATOSSERIE
(IN
 serie NVARCHAR(200),
 resolucion NVARCHAR(200),
 autorizacion NVARCHAR(200),
 fechares NVARCHAR(200),
 del NVARCHAR(200),
 al NVARCHAR(200),
 tipodoc NVARCHAR(200),
 esbatch NVARCHAR(200),
 sucursal NVARCHAR(200),
 dispositivo NVARCHAR(200),
 nomsucursal NVARCHAR(200),
 dirsucursal NVARCHAR(200),
 munisucursal NVARCHAR(200),
 deptosucursal NVARCHAR(200),
 usuario NVARCHAR(200),
 clave NVARCHAR(200)
)
LANGUAGE SQLSCRIPT
AS

BEGIN

insert into 
"@FACE_RESOLUCION" 
("Code",
"LineId",
"Object",
"LogInst",
"U_SERIE",
"U_RESOLUCION",
"U_AUTORIZACION",
"U_FECHA_AUTORIZACION",
"U_FACTURA_DEL",
"U_FACTURA_AL",
"U_TIPO_DOC",
"U_ES_BATCH",
"U_SUCURSAL",
"U_DISPOSITIVO",
"U_NOMBRE_SUCURSAL",
"U_DIR_SUCURSAL",
"U_MUNI_SUCURSAL",
"U_DEPTO_SUCURSAL",
"U_USUARIO",
"U_CLAVE")
values 
(:serie,:serie,null,null,:serie,:resolucion,:autorizacion,:fechares,:del,:al,:tipodoc,:esbatch,:sucursal,:dispositivo,:nomsucursal,:dirsucursal,:munisucursal,:deptosucursal,:usuario,:clave);
END;