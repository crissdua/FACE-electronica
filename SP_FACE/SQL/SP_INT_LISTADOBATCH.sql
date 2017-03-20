CREATE procedure [dbo].[SP_INT_LISTADOBATCH] @Serie int,@FechaIni  varchar(10),@FechaFin  varchar(10)
AS
if @Serie <> 0 
	select Estado=case isnull(a.U_ESTADO_FACE,'P') when 'P' then 'Pendiente' when 'R' then 'Rechazado' when 'A' then 'Autorizado' end ,
	a.docentry 'Correlativo',
	'Tipo Documento'= case a.DocSubType when '--' then 'Factura' when 'DN' then 'Nota Debito' End ,
	b.SeriesName 'Serie', 
	DocNum 'No. Documento',convert(char(10),DocDate,103)  'Fecha Documento' ,CardName  'Cliente',
	convert(numeric(18,2),DocTotal,1)  'Total Documento', case dbo.ufn_EstadoDocumento(a.docentry) when 'A' then 'Anulado' else 'Vigente' end 'Estado del Documento' 
	from oinv a 
	inner join NNM1 b 
	on a.Series = b.Series 
	where isnull(U_ESTADO_FACE,'P') in ('P','R')  
	and a.docdate between @FechaIni and  @FechaFin
	and   b.Series = @Serie 
	union 
	select Estado=case isnull(a.U_ESTADO_FACE,'P') when 'P' then 'Pendiente' when 'R' then 'Rechazado' when 'A' then 'Autorizado' end ,
	a.docentry 'Correlativo','Nota Credito' 'Tipo Documento',	b.SeriesName 'Serie',  DocNum 'No. Documento',convert(char(10),DocDate,103)  'Fecha Documento' ,
	CardName  'Cliente',convert(numeric(18,2),DocTotal,1)  'Total Documento', case dbo.ufn_EstadoDocumentoNC(a.docentry) when 'A' then 'Anulado' else 'Vigente' end 'Estado del Documento'   
	from ORIN  a 
	inner join NNM1 b 
	on a.Series = b.Series  
	where isnull(U_ESTADO_FACE,'P') in ('P','R')  
	and a.docdate between @FechaIni and  @FechaFin
	and   b.Series = @Serie 
	order by b.SeriesName, Correlativo 
else
	select Estado=case isnull(a.U_ESTADO_FACE,'P') when 'P' then 'Pendiente' when 'R' then 'Rechazado' when 'A' then 'Autorizado' end ,
	a.docentry 'Correlativo',
	'Tipo Documento'= case a.DocSubType when '--' then 'Factura' when 'DN' then 'Nota Debito' End , 
	b.SeriesName 'Serie', 
	DocNum 'No. Documento',convert(char(10),DocDate,103)  'Fecha Documento' ,CardName  'Cliente',
	convert(numeric(18,2),DocTotal,1)  'Total Documento', case dbo.ufn_EstadoDocumento(a.docentry) when 'A' then 'Anulado' else 'Vigente' end 'Estado del Documento' 
	from oinv a 
	inner join NNM1 b 
	on a.Series = b.Series 
	where isnull(U_ESTADO_FACE,'P') in ('P','R')  
	and a.docdate between @FechaIni and  @FechaFin
	and   b.Series in (select U_SERIE from [@FACE_RESOLUCION] where U_ES_BATCH='Y')
	union 
	select Estado=case isnull(a.U_ESTADO_FACE,'P') when 'P' then 'Pendiente' when 'R' then 'Rechazado' when 'A' then 'Autorizado' end ,
	a.docentry 'Correlativo','Nota Credito' 'Tipo Documento',	b.SeriesName 'Serie',  DocNum 'No. Documento',convert(char(10),DocDate,103)  'Fecha Documento' ,
	CardName  'Cliente',convert(numeric(18,2),DocTotal,1)  'Total Documento', case dbo.ufn_EstadoDocumentoNC(a.docentry) when 'A' then 'Anulado' else 'Vigente' end 'Estado del Documento'   
	from ORIN  a 
	inner join NNM1 b 
	on a.Series = b.Series  
	where isnull(U_ESTADO_FACE,'P') in ('P','R')  
	and a.docdate between @FechaIni and  @FechaFin
	and   b.Series in (select U_SERIE from [@FACE_RESOLUCION] where U_ES_BATCH='Y')
	order by b.SeriesName, Correlativo
