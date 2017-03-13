Imports SESystem.Connection.DBConnection
Imports System.Xml
Imports System.Xml.Serialization
Imports System.IO
Imports System
Imports Microsoft.VisualBasic


Public Class clsFERRETERIAPETAPA

    Function GeneraXML(ByVal Tipo As String, ByVal sCompanyName As String, ByVal iCodeSeries As Integer, ByVal sSerie As String, ByVal sNumDoc As String, ByVal sServidor As String, ByVal sBaseDatos As String, ByVal sUsuario As String, ByVal sPassword As String, ByVal EmailFrom As String, ByRef sMensajeRetorno As String, ByRef sXMLRetorno As String) As Boolean

        Dim sXML As String
        Dim tipoDOC As String
        Dim obOINV As DataTable
        Dim obOINV1 As DataTable
        Dim obOCRD As DataTable
        Dim obOCRD1 As DataTable
        Dim obOADM As DataTable
        Dim obCRD1 As DataTable
        Dim obRES As DataTable
        Dim obPAR As DataTable
        Dim obCountry As DataTable
        Dim obCountry2 As DataTable

        Dim dTotalBruto As Double
        Dim dTotalDescueto As Double
        Dim dTotalIva As Double
        Dim dpDescuento As Double
        Dim dtDescuento As Double
        Dim dpImpuesto As Double
        Dim dtImpuesto As Double
        Dim sTaxCode As String
        Dim isCode As Integer
        Dim sProdu As String
        Dim rows() As DataRow
        Dim base As Decimal
        Dim tasa As Decimal
        Dim monto As Decimal
        Dim DB As String
        Dim Desc As Decimal
        Try

            SESystem.Connection.DBConnection.Usuario = sUsuario
            SESystem.Connection.DBConnection.Password = sPassword

            If Not SESystem.Connection.DBConnection.ConectDB(sServidor, 1433, sBaseDatos) Then
                sMensajeRetorno = "No se ha podido Conectar a la Base Datos"
                Return False
            End If
            obPAR = SESystem.Connection.DBConnection.EjecutaSqlTable("SELECT * FROM [@FACE_PARAMETROS]")
            If Tipo = "FAC" Or Tipo = "ND" Then
                obOINV = SESystem.Connection.DBConnection.EjecutaSqlTable("SELECT * FROM OINV WHERE Series = " & iCodeSeries & " AND DOCNUM = " & sNumDoc & " and docsubtype='--'")
            Else
                obOINV = SESystem.Connection.DBConnection.EjecutaSqlTable("SELECT * FROM ORIN WHERE Series = " & iCodeSeries & " AND DOCNUM = " & sNumDoc)
            End If
            obOCRD = SESystem.Connection.DBConnection.EjecutaSqlTable("SELECT * FROM OCRD WHERE CardCode = " & SESystem.Utils.Generales.scm(obOINV.Rows(0)("CardCode")))
            obOCRD1 = SESystem.Connection.DBConnection.EjecutaSqlTable("SELECT * FROM CRD1 WHERE CardCode = " & SESystem.Utils.Generales.scm(obOINV.Rows(0)("CardCode")) & " AND AdresType = 'S'")
            obOADM = SESystem.Connection.DBConnection.EjecutaSqlTable("SELECT * FROM OADM WHERE CompnyName = " & SESystem.Utils.Generales.scm(sCompanyName))
            obRES = SESystem.Connection.DBConnection.EjecutaSqlTable("SELECT * FROM [@FACE_RESOLUCION] WHERE U_SERIE = " & iCodeSeries)
            obCountry = SESystem.Connection.DBConnection.EjecutaSqlTable("select * from ADM1")

            If obOADM.Rows.Count = 0 Then
                Throw New Exception("No se han definido datos de la empresa")
            End If
            If obRES.Rows.Count = 0 Then
                Throw New Exception("No se han definido una serie de facturas")
            End If
            If obPAR.Rows.Count = 0 Then
                Throw New Exception("No se han definido los parametros para la facturación electrónica")
            End If
            If obCountry.Rows.Count = 0 Then
                Throw New Exception("No se han definido los datos de ciudad, direccion de la empresa")
            End If

            tipoDOC = obRES.Rows(0)("U_TIPO_DOC").ToString
            'Select Case Tipo
            '    Case Is = "FAC"
            '        tipoDOC = "FACE63"
            '    Case Is = "ND"
            '        tipoDOC = ""
            '    Case Is = "NC"
            '        tipoDOC = ""
            'End Select

            sXML = String.Format("<FactDocGT xmlns=""http://www.fact.com.mx/schema/gt""  xmlns:xsi= ""http://www.w3.org/2001/XMLSchema-instance""  xsi:schemaLocation=""http://www.fact.com.mx/schema/gt http://www.mysuitemex.com/fact/schema/fx_2012_gt.xsd"">", "") & vbCrLf

            sXML &= String.Format("<Version>2</Version>", "") & vbCrLf


            'verifica si el paremtro de asignacion solicitada esta activo
            rows = obPAR.Select("U_PARAMETRO='ASS'")
            If rows(0).Item(5) = "SI" Then
                sXML &= String.Format("<AsignacionSolicitada>", "") & vbCrLf

                sXML &= String.Format("<Serie>{0}</Serie>", sSerie) & vbCrLf

                sXML &= String.Format("<NumeroDocumento>{0}</NumeroDocumento>", sNumDoc) & vbCrLf

                Dim fecha As String = Format(Date.Now, "yyyy-MM-ddThh:mm:ss")
                sXML &= String.Format("<FechaEmision>{0}</FechaEmision>", fecha) & vbCrLf

                sXML &= String.Format("<NumeroAutorizacion>{0}</NumeroAutorizacion>", obRES.Rows(0)("U_AUTORIZACION")) & vbCrLf

                fecha = Format(obRES.Rows(0)("U_FECHA_AUTORIZACION"), "yyyy-MM-dd")
                sXML &= String.Format("<FechaResolucion>{0}</FechaResolucion>", fecha) & vbCrLf

                sXML &= String.Format("<RangoInicialAutorizado>{0}</RangoInicialAutorizado>", obRES.Rows(0)("U_FACTURA_DEL")) & vbCrLf

                sXML &= String.Format("<RangoFinalAutorizado>{0}</RangoFinalAutorizado>", obRES.Rows(0)("U_FACTURA_AL")) & vbCrLf

                sXML &= String.Format("</AsignacionSolicitada>", "") & vbCrLf
            Else
                If tipoDOC = "CFACE1" Or tipoDOC = "CFACE8" Then
                    sXML &= String.Format("<AsignacionSolicitada>", "") & vbCrLf

                    Dim myS As String = SESystem.Connection.DBConnection.TraeDato("SELECT BeginStr FROM NNM1 WHERE SERIES=" & iCodeSeries)
                    sXML &= String.Format("<Serie>{0}</Serie>", myS) & vbCrLf

                    sXML &= String.Format("<NumeroDocumento>{0}</NumeroDocumento>", sNumDoc) & vbCrLf

                    Dim fecha As String = Format(obOINV.Rows(0)("DOCDATE"), "yyyy-MM-ddThh:mm:ss")
                    sXML &= String.Format("<FechaEmision>{0}</FechaEmision>", fecha) & vbCrLf

                    sXML &= String.Format("<NumeroAutorizacion>{0}</NumeroAutorizacion>", obRES.Rows(0)("U_AUTORIZACION")) & vbCrLf

                    fecha = Format(obRES.Rows(0)("U_FECHA_AUTORIZACION"), "yyyy-MM-dd")
                    sXML &= String.Format("<FechaResolucion>{0}</FechaResolucion>", fecha) & vbCrLf

                    sXML &= String.Format("<RangoInicialAutorizado>{0}</RangoInicialAutorizado>", obRES.Rows(0)("U_FACTURA_DEL")) & vbCrLf

                    sXML &= String.Format("<RangoFinalAutorizado>{0}</RangoFinalAutorizado>", obRES.Rows(0)("U_FACTURA_AL")) & vbCrLf

                    sXML &= String.Format("</AsignacionSolicitada>", "") & vbCrLf
                End If
            End If

            'sXML &= String.Format("<Procesamiento>", "") & vbCrLf
            'sXML &= String.Format("<Dictionary name=""{0}"">", "email") & vbCrLf
            'sXML &= String.Format("<Entry k=""from"" v=""{0}""/>", EmailFrom) & vbCrLf
            'sXML &= String.Format("<Entry k=""to"" v=""{0}""/>", obOCRD.Rows(0)("E_mail")) & vbCrLf
            'sXML &= String.Format("<Entry k=""cc"" v=""{0}""/>", "") & vbCrLf
            'sXML &= String.Format("<Entry k=""formats"" v=""pdf""/>", "") & vbCrLf
            'sXML &= String.Format("</Dictionary>", "") & vbCrLf
            'sXML &= String.Format("</Procesamiento>", "") & vbCrLf

            sXML &= String.Format("<Encabezado>", "") & vbCrLf

            sXML &= String.Format("<TipoActivo>" & tipoDOC & "</TipoActivo>", "") & vbCrLf

            sXML &= String.Format("<CodigoDeMoneda>GTQ</CodigoDeMoneda>", obOINV.Rows(0)("DocCur")) & vbCrLf

            sXML &= String.Format("<TipoDeCambio>{0}</TipoDeCambio>", obOINV.Rows(0)("DocRate")) & vbCrLf

            sXML &= String.Format("<InformacionDeRegimenIsr>PAGO_TRIMESTRAL</InformacionDeRegimenIsr>", "") & vbCrLf

            sXML &= String.Format("</Encabezado>", "") & vbCrLf


            sXML &= String.Format("<Vendedor>", "") & vbCrLf

            rows = obPAR.Select("U_PARAMETRO='NIT'")
            sXML &= String.Format("<Nit>{0}</Nit>", rows(0).Item(5)) & vbCrLf

            rows = obPAR.Select("U_PARAMETRO='NOMC'")
            sXML &= String.Format("<NombreComercial>{0}</NombreComercial>", Utils.ValidaDato(rows(0).Item(5))) & vbCrLf

            sXML &= String.Format("<Idioma>es</Idioma>", "") & vbCrLf

            sXML &= String.Format("<DireccionDeEmisionDeDocumento>", "") & vbCrLf

            'rows = obPAR.Select("U_PARAMETRO='NOME'")
            sXML &= String.Format("<NombreDeEstablecimiento>{0}</NombreDeEstablecimiento>", Utils.ValidaDato(obRES.Rows(0)("U_NOMBRE_SUCURSAL").ToString)) & vbCrLf

            'rows = obPAR.Select("U_PARAMETRO='CODE'")
            sXML &= String.Format("<CodigoDeEstablecimiento>{0}</CodigoDeEstablecimiento>", obRES.Rows(0)("U_SUCURSAL").ToString) & vbCrLf

            'rows = obPAR.Select("U_PARAMETRO='DIE'")
            sXML &= String.Format("<DispositivoElectronico>{0}</DispositivoElectronico>", obRES.Rows(0)("U_DISPOSITIVO").ToString) & vbCrLf

            rows = obPAR.Select("U_PARAMETRO='DIRE'")
            sXML &= String.Format("<Direccion1>{0}</Direccion1>", Utils.ValidaDato(rows(0).Item(5))) & vbCrLf

            'sXML &= String.Format("<Direccion2>{0}</Direccion2>", "") & vbCrLf
            sXML &= String.Format("<Municipio>{0}</Municipio>", Utils.ValidaDato(obCountry.Rows(0)("City").ToUpper)) & vbCrLf

            sXML &= String.Format("<Departamento>{0}</Departamento>", Utils.ValidaDato(obCountry.Rows(0)("CityF").ToUpper)) & vbCrLf

            sXML &= String.Format("<CodigoDePais>{0}</CodigoDePais>", Utils.ValidaDato(obOADM.Rows(0)("Country"))) & vbCrLf

            sXML &= String.Format("<CodigoPostal>{0}</CodigoPostal>", IIf(obCountry.Rows(0)("ZipCode").ToString = "", "00000", obCountry.Rows(0)("ZipCode").ToString)) & vbCrLf

            sXML &= String.Format("</DireccionDeEmisionDeDocumento>", "") & vbCrLf

            sXML &= String.Format("</Vendedor>", "") & vbCrLf


            sXML &= String.Format("<Comprador>", "") & vbCrLf

            'Dim NitComp As String =
            sXML &= String.Format(" <Nit>{0}</Nit>", Utils.ValidaCF(Replace(obOINV.Rows(0)("U_FacNit"), "-", ""))) & vbCrLf
            'sXML &= String.Format(" <Nit>33003386</Nit>", Replace(obOCRD1.Rows(0)("LicTradNum").ToString.Trim("0"), "-", "")) & vbCrLf

            sXML &= String.Format("<NombreComercial>{0}</NombreComercial>", Utils.ValidaDato(obOINV.Rows(0)("U_FacNom"))) & vbCrLf

            sXML &= String.Format("<Idioma>es</Idioma>", "") & vbCrLf

            sXML &= String.Format("<DireccionComercial>", "") & vbCrLf

            sXML &= String.Format("<Direccion1>{0}</Direccion1>", Utils.ValidaDato(obOINV.Rows(0)("U_Direccion"))) & vbCrLf

            'sXML &= String.Format("<Direccion2>{0}</Direccion2>", "") & vbCrLf
            sXML &= String.Format(" <Municipio>{0}</Municipio>", Utils.ValidaDato(IIf(obOCRD1.Rows(0)("city").ToString = "", "Guatemala", obOCRD1.Rows(0)("city")))) & vbCrLf

            Dim depto As String = SESystem.Connection.DBConnection.TraeDato("SELECT B.Name FROM OCRD A INNER JOIN [@DEPTOSGUA] B ON A.U_Depto=B.Code WHERE CardCode ='" & obOCRD.Rows(0)("CardCode").ToString & "'")
            sXML &= String.Format("<Departamento>Guatemala</Departamento>", Utils.ValidaDato(IIf(depto = "", "Guatemala", depto))) & vbCrLf

            sXML &= String.Format("<CodigoDePais>GT</CodigoDePais>", Utils.ValidaDato(obOCRD1.Rows(0)("Country"))) & vbCrLf

            sXML &= String.Format("<CodigoPostal>{0}</CodigoPostal>", IIf(obOCRD1.Rows(0)("ZipCode").ToString = "", "00000", obOCRD1.Rows(0)("ZipCode"))) & vbCrLf

            sXML &= String.Format("</DireccionComercial>", "") & vbCrLf

            sXML &= String.Format("</Comprador>", "") & vbCrLf

            sXML &= String.Format("<Detalles>", "") & vbCrLf

            obOINV1 = SESystem.Connection.DBConnection.EjecutaSqlTable("SELECT * FROM INV1 WHERE DocEntry = " & obOINV.Rows(0)("DocEntry"))
            If obOINV1.Rows.Count = 0 Then
                Throw New Exception("El documento no tiene ningun detalle")
            End If
            For Each obProd As DataRow In obOINV1.Rows
                sXML &= String.Format("<Detalle>", "") & vbCrLf
                sXML &= String.Format("<Descripcion>{0}</Descripcion>", Utils.ValidaDato(obProd("Dscription"))) & vbCrLf


                If Len(obProd("ItemCode")) < 14 Then
                    sProdu = "00000000000001"
                Else
                    sProdu = obProd("ItemCode")
                End If
                sXML &= String.Format("<CodigoEAN>{0}</CodigoEAN>", sProdu) & vbCrLf

                sXML &= String.Format("<UnidadDeMedida>{0}</UnidadDeMedida>", "UNI") & vbCrLf

                sXML &= String.Format("<Cantidad>{0}</Cantidad>", obProd("Quantity")) & vbCrLf

                sXML &= String.Format("<ValorSinDR>", "") & vbCrLf

                sXML &= String.Format("<Precio>{0}</Precio>", obProd("Price")) & vbCrLf

                sXML &= String.Format("<Monto>{0}</Monto>", obProd("linetotal")) & vbCrLf

                sXML &= String.Format("</ValorSinDR>", "") & vbCrLf

                sTaxCode = obProd("TaxCode")

                Desc = obProd("DiscPrcnt")
                If Desc > 0 Then
                    'Calculando los descuentos
                    dpDescuento = obProd("DiscPrcnt") * 100
                    If dpDescuento = 0 Then
                        dtDescuento = 0
                    Else
                        dtDescuento = (obProd("linetotal") * dpDescuento)
                    End If

                    sXML &= String.Format("<DescuentosYRecargos>", "") & vbCrLf





                    'Calculando impuesto
                    dpImpuesto = TraeDato("SELECT rate FROM OSTA  WHERE code = " & SESystem.Utils.Generales.scm(obProd("TaxCode")))
                    dtImpuesto = obProd("linetotal") * (obProd("VatPrcnt") / 100)


                    sXML &= String.Format("<SumaDeDescuentos>{0}</SumaDeDescuentos>", dtDescuento) & vbCrLf

                    ' sXML &= String.Format("<SumaDeRecargos>{0}</SumaDeRecargos>", 0) & vbCrLf

                    sXML &= String.Format("<DescuentoORecargo>", "") & vbCrLf

                    sXML &= String.Format("<Operacion>{0}</Operacion>", "DESCUENTO") & vbCrLf

                    sXML &= String.Format("<Servicio>{0}</Servicio>", "ALLOWANCE_GLOBAL") & vbCrLf

                    base = obProd("linetotal")
                    tasa = dpDescuento
                    monto = obProd("linetotal") * dtDescuento

                    sXML &= String.Format("<Base>{0}</Base>", base) & vbCrLf '+++++++++++++++++'VERIFICAR ESTA INFORMACION++++++++++++++

                    sXML &= String.Format("<Tasa>{0}</Tasa>", tasa) & vbCrLf '+++++++++++++++++'VERIFICAR ESTA INFORMACION++++++++++++++

                    sXML &= String.Format("<Monto>{0}</Monto>", monto) & vbCrLf

                    sXML &= String.Format("</DescuentoORecargo>", "") & vbCrLf

                    sXML &= String.Format("</DescuentosYRecargos>", "") & vbCrLf
                End If

                sXML &= String.Format("<ValorConDR>", "") & vbCrLf

                sXML &= String.Format("<Precio>{0}</Precio>", obProd("linetotal")) & vbCrLf

                sXML &= String.Format("<Monto>{0}</Monto>", obProd("linetotal")) & vbCrLf

                sXML &= String.Format("</ValorConDR>", "") & vbCrLf


                sXML &= String.Format("<Impuestos>", "") & vbCrLf

                sXML &= String.Format("<TotalDeImpuestos>{0}</TotalDeImpuestos>", dtImpuesto) & vbCrLf

                sXML &= String.Format("<IngresosNetosGravados>{0}</IngresosNetosGravados>", obProd("linetotal")) & vbCrLf

                sXML &= String.Format("<TotalDeIVA>{0}</TotalDeIVA>", dtImpuesto) & vbCrLf

                sXML &= String.Format("<Impuesto>", "") & vbCrLf

                sXML &= String.Format("<Tipo>{0}</Tipo>", obProd("TaxCode")) & vbCrLf

                sXML &= String.Format("<Base>{0}</Base>", obProd("linetotal")) & vbCrLf

                sXML &= String.Format("<Tasa>{0}</Tasa>", dpImpuesto) & vbCrLf

                sXML &= String.Format("<Monto>{0}</Monto>", dtImpuesto) & vbCrLf

                sXML &= String.Format("</Impuesto>", "") & vbCrLf

                sXML &= String.Format("</Impuestos>", "") & vbCrLf


                sXML &= String.Format("<Categoria>{0}</Categoria>", "SERVICIOS") & vbCrLf


                sXML &= String.Format("</Detalle>", "") & vbCrLf


            Next

            sXML &= String.Format("</Detalles>", "") & vbCrLf



            dTotalBruto = obOINV.Rows(0)("DocTotal") - obOINV.Rows(0)("VatSum") - obOINV.Rows(0)("DiscSum")
            dTotalDescueto = obOINV.Rows(0)("DiscSum")
            dTotalIva = obOINV.Rows(0)("VatSum")

            sXML &= String.Format("<Totales>", "") & vbCrLf

            sXML &= String.Format("<SubTotalSinDR>{0}</SubTotalSinDR>", dTotalBruto) & vbCrLf

            If Desc > 0 Then
                sXML &= String.Format("<DescuentosYRecargos>", "") & vbCrLf

                sXML &= String.Format("<SumaDeDescuentos>{0}</SumaDeDescuentos>", dTotalDescueto) & vbCrLf

                'sXML &= String.Format("<SumaDeRecargos>{0}</SumaDeRecargos>", 0) & vbCrLf

                sXML &= String.Format("<DescuentoORecargo>", "") & vbCrLf

                sXML &= String.Format("<Operacion>{0}</Operacion>", "DESCUENTO") & vbCrLf

                sXML &= String.Format("<Servicio>{0}</Servicio>", "ALLOWANCE_GLOBAL") & vbCrLf

                sXML &= String.Format("<Base>{0}</Base>", base) & vbCrLf '+++++++++++++++++'VERIFICAR ESTA INFORMACION++++++++++++++

                sXML &= String.Format("<Tasa>{0}</Tasa>", tasa) & vbCrLf '+++++++++++++++++'VERIFICAR ESTA INFORMACION++++++++++++++

                sXML &= String.Format("<Monto>{0}</Monto>", monto) & vbCrLf

                sXML &= String.Format("</DescuentoORecargo>", "") & vbCrLf
                sXML &= String.Format("</DescuentosYRecargos>", "") & vbCrLf
            End If

            Dim SubTotalConDR = dTotalBruto - dTotalDescueto
            sXML &= String.Format("<SubTotalConDR>{0}</SubTotalConDR>", SubTotalConDR) & vbCrLf '+++++++++++++++++'VERIFICAR ESTA INFORMACION++++++++++++++


            sXML &= String.Format("<Impuestos>", "") & vbCrLf

            sXML &= String.Format("<TotalDeImpuestos>{0}</TotalDeImpuestos>", dTotalIva) & vbCrLf

            sXML &= String.Format("<IngresosNetosGravados>{0}</IngresosNetosGravados>", dTotalBruto) & vbCrLf

            sXML &= String.Format("<TotalDeIVA>{0}</TotalDeIVA>", dTotalIva) & vbCrLf

            sXML &= String.Format("<Impuesto>", "") & vbCrLf

            sXML &= String.Format("<Tipo>{0}</Tipo>", sTaxCode) & vbCrLf

            sXML &= String.Format("<Base>{0}</Base>", dTotalBruto) & vbCrLf

            sXML &= String.Format("<Tasa>{0}</Tasa>", dpImpuesto) & vbCrLf

            sXML &= String.Format("<Monto>{0}</Monto>", dTotalIva) & vbCrLf

            sXML &= String.Format("</Impuesto>", "") & vbCrLf

            sXML &= String.Format("</Impuestos>", "") & vbCrLf


            sXML &= String.Format("<Total>{0}</Total>", obOINV.Rows(0)("DocTotal")) & vbCrLf

            sXML &= String.Format("<TotalLetras>{0}</TotalLetras>", Utils.ValidaDato(Utils.Letras(obOINV.Rows(0)("DocTotal").ToString))) & vbCrLf '+++++++++++++++++'VERIFICAR ESTA INFORMACION++++++++++++++

            sXML &= String.Format("</Totales>", "") & vbCrLf


            sXML &= String.Format("</FactDocGT>", "") & vbCrLf


            sXMLRetorno = sXML
            Return True
        Catch ex As Exception
            sMensajeRetorno = ex.Message
            Return False
        End Try



    End Function

    'Function GeneraXMLNotaDebito(ByVal sCompanyName As String, ByVal iCodeSeries As Integer, ByVal sSerie As String, ByVal sNumDoc As String, ByVal sServidor As String, ByVal sBaseDatos As String, ByVal sUsuario As String, ByVal sPassword As String, ByVal EmailFrom As String, ByRef sMensajeRetorno As String, ByRef sXMLRetorno As String) As Boolean
    '    Try

    '        Dim sXML As String

    '        Dim obOINV As DataTable
    '        Dim obOINV1 As DataTable
    '        Dim obOCRD As DataTable
    '        Dim obOCRD1 As DataTable
    '        Dim obOADM As DataTable
    '        Dim obCRD1 As DataTable
    '        Dim obPAR As DataTable
    '        Dim obRES As DataTable
    '        Dim rows() As DataRow
    '        Dim obCountry As DataTable

    '        Dim dTotalBruto As Double
    '        Dim dTotalDescueto As Double
    '        Dim dTotalIva As Double
    '        Dim dpDescuento As Double
    '        Dim dtDescuento As Double
    '        Dim dpImpuesto As Double
    '        Dim dtImpuesto As Double
    '        Dim sTaxCode As String
    '        Dim isCode As Integer
    '        Dim sProdu As String

    '        SESystem.Connection.DBConnection.Usuario = sUsuario
    '        SESystem.Connection.DBConnection.Password = sPassword

    '        If Not SESystem.Connection.DBConnection.ConectDB(sServidor, 1433, sBaseDatos) Then
    '            sMensajeRetorno = "No se ha podido Conectar a la Base Datos"
    '            Return False
    '        End If


    '        obOINV = SESystem.Connection.DBConnection.EjecutaSqlTable("SELECT * FROM OINV WHERE Series = " & iCodeSeries & " AND DOCNUM = " & sNumDoc & " and docsubtype='DN'")
    '        obOCRD = SESystem.Connection.DBConnection.EjecutaSqlTable("SELECT * FROM OCRD WHERE CardCode = " & SESystem.Utils.Generales.scm(obOINV.Rows(0)("CardCode")))
    '        obOCRD1 = SESystem.Connection.DBConnection.EjecutaSqlTable("SELECT * FROM CRD1 WHERE CardCode = " & SESystem.Utils.Generales.scm(obOINV.Rows(0)("CardCode")) & " AND AdresType = 'S'")
    '        obOADM = SESystem.Connection.DBConnection.EjecutaSqlTable("SELECT * FROM OADM WHERE CompnyName = " & SESystem.Utils.Generales.scm(sCompanyName))
    '        obRES = SESystem.Connection.DBConnection.EjecutaSqlTable("SELECT * FROM [@FACE_RESOLUCION] WHERE U_SERIE = " & iCodeSeries)
    '        obPAR = SESystem.Connection.DBConnection.EjecutaSqlTable("SELECT * FROM [@FACE_PARAMETROS]")
    '        obCountry = SESystem.Connection.DBConnection.EjecutaSqlTable("select * from ADM1")


    '        sXML = String.Format("<FactDocGT xmlns=""http://www.fact.com.mx/schema/gt""  xmlns:xsi= ""http://www.w3.org/2001/XMLSchema-instance""  xsi:schemaLocation=""http://www.fact.com.mx/schema/gt http://www.mysuitemex.com/fact/schema/fx_2010_gt.xsd"">", "") & vbCrLf
    '        sXML &= String.Format("<Version>1</Version>", "") & vbCrLf
    '        'sXML &= String.Format("<AsignacionSolicitada>", "") & vbCrLf
    '        'sXML &= String.Format("<Serie>{0}</Serie>", sSerie) & vbCrLf
    '        'sXML &= String.Format("<NumeroDocumento>{0}</NumeroDocumento>", sNumDoc) & vbCrLf
    '        'Dim fecha As String = Format(Date.Now, "yyyy-MM-ddThh:mm:ss")
    '        'sXML &= String.Format("<FechaEmision>{0}</FechaEmision>", fecha) & vbCrLf
    '        'sXML &= String.Format("<NumeroAutorizacion>{0}</NumeroAutorizacion>", obRES.Rows(0)("U_AUTORIZACION")) & vbCrLf
    '        'fecha = Format(obRES.Rows(0)("U_FECHA_AUTORIZACION"), "yyyy-MM-dd")
    '        'sXML &= String.Format("<FechaResolucion>{0}</FechaResolucion>", fecha) & vbCrLf
    '        'sXML &= String.Format("<RangoInicialAutorizado>{0}</RangoInicialAutorizado>", obRES.Rows(0)("U_FACTURA_DEL")) & vbCrLf
    '        'sXML &= String.Format("<RangoFinalAutorizado>{0}</RangoFinalAutorizado>", obRES.Rows(0)("U_FACTURA_AL")) & vbCrLf
    '        'sXML &= String.Format("</AsignacionSolicitada>", "") & vbCrLf
    '        sXML &= String.Format("<Procesamiento>", "") & vbCrLf
    '        sXML &= String.Format("<Dictionary name=""{0}"">", "email") & vbCrLf
    '        sXML &= String.Format("<Entry k=""from"" v=""{0}""/>", EmailFrom) & vbCrLf
    '        sXML &= String.Format("<Entry k=""to"" v=""{0}""/>", obOCRD.Rows(0)("E_mail")) & vbCrLf
    '        sXML &= String.Format("<Entry k=""cc"" v=""{0}""/>", "") & vbCrLf
    '        sXML &= String.Format("<Entry k=""formats"" v=""pdf""/>", "") & vbCrLf
    '        sXML &= String.Format("</Dictionary>", "") & vbCrLf
    '        sXML &= String.Format("</Procesamiento>", "") & vbCrLf
    '        sXML &= String.Format("<Encabezado>", "") & vbCrLf
    '        sXML &= String.Format("<TipoDeDocumento>INVOICE</TipoDeDocumento>", "") & vbCrLf
    '        sXML &= String.Format("<EstadoDeDocumento>ORIGINAL</EstadoDeDocumento>", "") & vbCrLf
    '        sXML &= String.Format("<CodigoDeMoneda>{0}</CodigoDeMoneda>", obOINV.Rows(0)("DocCur")) & vbCrLf
    '        sXML &= String.Format("<TipoDeCambio>{0}</TipoDeCambio>", obOINV.Rows(0)("DocRate")) & vbCrLf
    '        sXML &= String.Format("<InformacionDeRegimenIsr>PAGO_TRIMESTRAL</InformacionDeRegimenIsr>", "") & vbCrLf
    '        sXML &= String.Format("<ReferenciaInterna>{0}</ReferenciaInterna>", "PRUEBAS") & vbCrLf
    '        sXML &= String.Format("</Encabezado>", "") & vbCrLf

    '        sXML &= String.Format("<Vendedor>", "") & vbCrLf
    '        rows = obPAR.Select("U_PARAMETRO='NIT'")
    '        sXML &= String.Format("<Nit>{0}</Nit>", rows(0).Item(5)) & vbCrLf
    '        rows = obPAR.Select("U_PARAMETRO='NOMC'")
    '        sXML &= String.Format("<NombreComercial>{0}</NombreComercial>", rows(0).Item(5)) & vbCrLf
    '        sXML &= String.Format("<Idioma>es</Idioma>", "") & vbCrLf
    '        sXML &= String.Format("<DireccionDeEmisionDeDocumento>", "") & vbCrLf
    '        rows = obPAR.Select("U_PARAMETRO='NOME'")
    '        sXML &= String.Format("<NombreDeEstablecimiento>{0}</NombreDeEstablecimiento>", rows(0).Item(5)) & vbCrLf
    '        rows = obPAR.Select("U_PARAMETRO='CODE'")
    '        sXML &= String.Format("<CodigoDeEstablecimiento>{0}</CodigoDeEstablecimiento>", rows(0).Item(5)) & vbCrLf
    '        rows = obPAR.Select("U_PARAMETRO='DIRE'")
    '        sXML &= String.Format("<Direccion1>{0}</Direccion1>", rows(0).Item(5)) & vbCrLf
    '        sXML &= String.Format("<Municipio>{0}</Municipio>", obCountry.Rows(0)("City").ToUpper) & vbCrLf
    '        sXML &= String.Format("<Departamento>{0}</Departamento>", obCountry.Rows(0)("CityF").ToUpper) & vbCrLf
    '        sXML &= String.Format("<CodigoDePais>{0}</CodigoDePais>", obOADM.Rows(0)("Country")) & vbCrLf
    '        sXML &= String.Format("<CodigoPostal>{0}</CodigoPostal>", obCountry.Rows(0)("ZipCode")) & vbCrLf
    '        sXML &= String.Format("</DireccionDeEmisionDeDocumento>", "") & vbCrLf
    '        sXML &= String.Format("</Vendedor>", "") & vbCrLf

    '        sXML &= String.Format("<Comprador>", "") & vbCrLf
    '        sXML &= String.Format(" <Nit>8195641</Nit>", obOCRD1.Rows(0)("LicTradNum")) & vbCrLf
    '        sXML &= String.Format("<NombreComercial>{0}</NombreComercial>", obOCRD.Rows(0)("CardName")) & vbCrLf
    '        sXML &= String.Format("<Idioma>es</Idioma>", "") & vbCrLf
    '        sXML &= String.Format("<DireccionComercial>", "") & vbCrLf
    '        sXML &= String.Format("<Direccion1>{0}</Direccion1>", obOCRD.Rows(0)("MailAddres")) & vbCrLf
    '        sXML &= String.Format(" <Municipio>{0}</Municipio>", obOCRD1.Rows(0)("city")) & vbCrLf
    '        Dim depto As String = SESystem.Connection.DBConnection.TraeDato("SELECT name FROM OCST  WHERE code = '" & obOCRD1.Rows(0)("state") & "'")
    '        sXML &= String.Format("<Departamento>{0}</Departamento>", depto) & vbCrLf
    '        sXML &= String.Format("<CodigoDePais>{0}</CodigoDePais>", obOCRD1.Rows(0)("Country")) & vbCrLf
    '        sXML &= String.Format("<CodigoPostal>01010</CodigoPostal>", obOCRD1.Rows(0)("ZipCode")) & vbCrLf
    '        sXML &= String.Format("</DireccionComercial>", "") & vbCrLf
    '        sXML &= String.Format("</Comprador>", "") & vbCrLf
    '        sXML &= String.Format("<Detalles>", "") & vbCrLf


    '        obOINV1 = SESystem.Connection.DBConnection.EjecutaSqlTable("SELECT * FROM INV1 WHERE DocEntry = " & obOINV.Rows(0)("DocEntry"))


    '        For Each obProd As DataRow In obOINV1.Rows
    '            sXML &= String.Format("<Detalle>", "") & vbCrLf
    '            sXML &= String.Format("<Descripcion>{0}</Descripcion>", obProd("Dscription")) & vbCrLf

    '            If Len(obProd("ItemCode")) < 14 Then
    '                sProdu = "00000000000001"
    '            Else
    '                sProdu = obProd("ItemCode")
    '            End If
    '            sXML &= String.Format("<CodigoEAN>{0}</CodigoEAN>", sProdu) & vbCrLf
    '            sXML &= String.Format("<UnidadDeMedida>{0}</UnidadDeMedida>", "") & vbCrLf
    '            sXML &= String.Format("<Cantidad>{0}</Cantidad>", obProd("Quantity")) & vbCrLf
    '            sXML &= String.Format("<ValoresBrutoLista>", "") & vbCrLf
    '            sXML &= String.Format("<Precio>{0}</Precio>", obProd("Price")) & vbCrLf
    '            sXML &= String.Format("<Monto>{0}</Monto>", obProd("linetotal")) & vbCrLf
    '            sXML &= String.Format("</ValoresBrutoLista>", "") & vbCrLf
    '            sXML &= String.Format("<ResumenDeDescuentos>", "") & vbCrLf

    '            'Calculando los descuentos
    '            dpDescuento = obProd("DiscPrcnt")
    '            If dpDescuento = 0 Then
    '                dtDescuento = 0
    '            Else
    '                dtDescuento = (obProd("linetotal") * dpDescuento) / 100
    '            End If


    '            'Calculando impuesto
    '            dpImpuesto = TraeDato("SELECT rate FROM OSTA  WHERE code = " & SESystem.Utils.Generales.scm(obProd("TaxCode")))
    '            dtImpuesto = obProd("linetotal") * (obProd("VatPrcnt") / 100)
    '            sTaxCode = obProd("TaxCode")


    '            sXML &= String.Format("<TotalDeDescuentos>{0}</TotalDeDescuentos>", dtDescuento) & vbCrLf
    '            sXML &= String.Format("<Descuentos>", "") & vbCrLf
    '            sXML &= String.Format("<Descuento>", "") & vbCrLf
    '            sXML &= String.Format("<Monto>{0}</Monto>", dtDescuento) & vbCrLf
    '            sXML &= String.Format("</Descuento>", "") & vbCrLf
    '            sXML &= String.Format("</Descuentos>", "") & vbCrLf
    '            sXML &= String.Format("</ResumenDeDescuentos>", "") & vbCrLf
    '            sXML &= String.Format("<ValoresNetoAPagar>", "") & vbCrLf
    '            sXML &= String.Format("<Precio>{0}</Precio>", obProd("linetotal")) & vbCrLf
    '            sXML &= String.Format("<Monto>{0}</Monto>", obProd("linetotal")) & vbCrLf
    '            sXML &= String.Format("</ValoresNetoAPagar>", "") & vbCrLf
    '            sXML &= String.Format("<ResumenDeImpuestos>", "") & vbCrLf
    '            sXML &= String.Format("<TotalDeImpuestos>{0}</TotalDeImpuestos>", dtImpuesto) & vbCrLf
    '            sXML &= String.Format("<IngresosNetosGravados>{0}</IngresosNetosGravados>", obProd("linetotal")) & vbCrLf
    '            sXML &= String.Format("<TotalDeIVA>{0}</TotalDeIVA>", dtImpuesto) & vbCrLf
    '            sXML &= String.Format("<Impuestos>", "") & vbCrLf
    '            sXML &= String.Format("<Impuesto>", "") & vbCrLf
    '            sXML &= String.Format("<Tipo>{0}</Tipo>", obProd("TaxCode")) & vbCrLf
    '            sXML &= String.Format("<Base>{0}</Base>", obProd("linetotal")) & vbCrLf
    '            sXML &= String.Format("<Tasa>{0}</Tasa>", dpImpuesto) & vbCrLf
    '            sXML &= String.Format("<Monto>{0}</Monto>", dtImpuesto) & vbCrLf
    '            sXML &= String.Format("</Impuesto>", "") & vbCrLf
    '            sXML &= String.Format("</Impuestos>", "") & vbCrLf
    '            sXML &= String.Format("</ResumenDeImpuestos>", "") & vbCrLf
    '            sXML &= String.Format("<Categoria>{0}</Categoria>", "") & vbCrLf
    '            sXML &= String.Format("<TextosDePosicion>", "") & vbCrLf
    '            sXML &= String.Format("<Texto>{0}</Texto>", "") & vbCrLf
    '            sXML &= String.Format("<Texto>{0}</Texto>", "") & vbCrLf
    '            sXML &= String.Format("<Texto>{0}</Texto>", "") & vbCrLf
    '            sXML &= String.Format("<Texto>{0}</Texto>", "") & vbCrLf
    '            sXML &= String.Format("<Texto>{0}</Texto>", "") & vbCrLf
    '            sXML &= String.Format("<Texto>{0}</Texto>", "") & vbCrLf
    '            sXML &= String.Format("</TextosDePosicion>", "") & vbCrLf
    '            sXML &= String.Format("</Detalle>", "") & vbCrLf
    '        Next

    '        sXML &= String.Format("</Detalles>", "") & vbCrLf


    '        dTotalBruto = obOINV.Rows(0)("DocTotal") - obOINV.Rows(0)("VatSum") - obOINV.Rows(0)("DiscSum")
    '        dTotalDescueto = obOINV.Rows(0)("DiscSum")
    '        dTotalIva = obOINV.Rows(0)("VatSum")

    '        sXML &= String.Format("<Totales>", "") & vbCrLf
    '        sXML &= String.Format("<TotalBrutoLista>{0}</TotalBrutoLista>", dTotalBruto) & vbCrLf
    '        sXML &= String.Format("<ResumenDeDescuentos>", "") & vbCrLf
    '        sXML &= String.Format("<TotalDeDescuentos>{0}</TotalDeDescuentos>", dTotalDescueto) & vbCrLf
    '        sXML &= String.Format("<Descuentos>", "") & vbCrLf
    '        sXML &= String.Format("<Descuento>", "") & vbCrLf
    '        sXML &= String.Format("<Monto>{0}</Monto>", dTotalDescueto) & vbCrLf
    '        sXML &= String.Format("</Descuento>", "") & vbCrLf
    '        sXML &= String.Format("</Descuentos>", "") & vbCrLf
    '        sXML &= String.Format("</ResumenDeDescuentos>", "") & vbCrLf
    '        sXML &= String.Format("<ResumenDeImpuestos>", "") & vbCrLf
    '        sXML &= String.Format("<TotalDeImpuestos>{0}</TotalDeImpuestos>", dTotalIva) & vbCrLf
    '        sXML &= String.Format("<IngresosNetosGravados>{0}</IngresosNetosGravados>", dTotalBruto) & vbCrLf
    '        sXML &= String.Format("<TotalDeIVA>{0}</TotalDeIVA>", dTotalIva) & vbCrLf
    '        sXML &= String.Format("<Impuestos>", "") & vbCrLf
    '        sXML &= String.Format("<Impuesto>", "") & vbCrLf
    '        sXML &= String.Format("<Tipo>{0}</Tipo>", sTaxCode) & vbCrLf
    '        sXML &= String.Format("<Base>{0}</Base>", dTotalBruto) & vbCrLf
    '        sXML &= String.Format("<Tasa>{0}</Tasa>", dtImpuesto) & vbCrLf
    '        sXML &= String.Format("<Monto>{0}</Monto>", dTotalIva) & vbCrLf
    '        sXML &= String.Format("</Impuesto>", "") & vbCrLf
    '        sXML &= String.Format("</Impuestos>", "") & vbCrLf
    '        sXML &= String.Format("</ResumenDeImpuestos>", "") & vbCrLf
    '        sXML &= String.Format("<TotalNetoAPagar>{0}</TotalNetoAPagar>", obOINV.Rows(0)("DocTotal")) & vbCrLf
    '        sXML &= String.Format("</Totales>", "") & vbCrLf
    '        sXML &= String.Format("<TextosDePie>", "") & vbCrLf
    '        sXML &= String.Format("<Texto>{0}</Texto>", "") & vbCrLf
    '        sXML &= String.Format("<Texto>{0}</Texto>", "") & vbCrLf
    '        sXML &= String.Format("<Texto>{0}</Texto>", "") & vbCrLf
    '        sXML &= String.Format("<Texto>{0}</Texto>", "") & vbCrLf
    '        sXML &= String.Format("<Texto>{0}</Texto>", "") & vbCrLf
    '        sXML &= String.Format("</TextosDePie>", "") & vbCrLf

    '        sXML &= String.Format("</FactDocGT>", "") & vbCrLf

    '        sXMLRetorno = sXML
    '        Return True
    '    Catch ex As Exception
    '        sMensajeRetorno = ex.Message
    '        Return False

    '    End Try



    'End Function

    'Function GeneraXMLNotaCredito(ByVal sCompanyName As String, ByVal iCodeSeries As Integer, ByVal sSerie As String, ByVal sNumDoc As String, ByVal sServidor As String, ByVal sBaseDatos As String, ByVal sUsuario As String, ByVal sPassword As String, ByVal EmailFrom As String, ByRef sMensajeRetorno As String, ByRef sXMLRetorno As String) As Boolean
    '    Try

    '        Dim sXML As String

    '        Dim obOINV As DataTable
    '        Dim obOINV1 As DataTable
    '        Dim obOCRD As DataTable
    '        Dim obOCRD1 As DataTable
    '        Dim obOADM As DataTable
    '        Dim obCRD1 As DataTable
    '        Dim obRES As DataTable
    '        Dim obPAR As DataTable
    '        Dim obCountry As DataTable


    '        Dim dTotalBruto As Double
    '        Dim dTotalDescueto As Double
    '        Dim dTotalIva As Double
    '        Dim dpDescuento As Double
    '        Dim dtDescuento As Double
    '        Dim dpImpuesto As Double
    '        Dim dtImpuesto As Double
    '        Dim sTaxCode As String
    '        Dim isCode As Integer
    '        Dim sProdu As String
    '        Dim rows() As DataRow

    '        SESystem.Connection.DBConnection.Usuario = sUsuario
    '        SESystem.Connection.DBConnection.Password = sPassword

    '        If Not SESystem.Connection.DBConnection.ConectDB(sServidor, 1433, sBaseDatos) Then
    '            sMensajeRetorno = "No se ha podido Conectar a la Base Datos"
    '            Return False
    '        End If


    '        obOINV = SESystem.Connection.DBConnection.EjecutaSqlTable("SELECT * FROM ORIN WHERE Series = " & iCodeSeries & " AND DOCNUM = " & sNumDoc)
    '        obOCRD = SESystem.Connection.DBConnection.EjecutaSqlTable("SELECT * FROM OCRD WHERE CardCode = " & SESystem.Utils.Generales.scm(obOINV.Rows(0)("CardCode")))
    '        obOCRD1 = SESystem.Connection.DBConnection.EjecutaSqlTable("SELECT * FROM CRD1 WHERE CardCode = " & SESystem.Utils.Generales.scm(obOINV.Rows(0)("CardCode")) & " AND AdresType = 'S'")
    '        obOADM = SESystem.Connection.DBConnection.EjecutaSqlTable("SELECT * FROM OADM WHERE CompnyName = " & SESystem.Utils.Generales.scm(sCompanyName))
    '        obRES = SESystem.Connection.DBConnection.EjecutaSqlTable("SELECT * FROM [@FACE_RESOLUCION] WHERE U_SERIE = " & iCodeSeries)
    '        obPAR = SESystem.Connection.DBConnection.EjecutaSqlTable("SELECT * FROM [@FACE_PARAMETROS]")
    '        obCountry = SESystem.Connection.DBConnection.EjecutaSqlTable("select * from ADM1")


    '        sXML = String.Format("<FactDocGT xmlns=""http://www.fact.com.mx/schema/gt""  xmlns:xsi= ""http://www.w3.org/2001/XMLSchema-instance""  xsi:schemaLocation=""http://www.fact.com.mx/schema/gt http://www.mysuitemex.com/fact/schema/fx_2010_gt.xsd"">", "") & vbCrLf
    '        sXML &= String.Format("<Version>1</Version>", "") & vbCrLf
    '        'sXML &= String.Format("<AsignacionSolicitada>", "") & vbCrLf
    '        'sXML &= String.Format("<Serie>{0}</Serie>", sSerie) & vbCrLf
    '        'sXML &= String.Format("<NumeroDocumento>{0}</NumeroDocumento>", sNumDoc) & vbCrLf
    '        'Dim fecha As String = Format(Date.Now, "yyyy-MM-ddThh:mm:ss")
    '        'sXML &= String.Format("<FechaEmision>{0}</FechaEmision>", fecha) & vbCrLf
    '        'sXML &= String.Format("<NumeroAutorizacion>{0}</NumeroAutorizacion>", obRES.Rows(0)("U_AUTORIZACION")) & vbCrLf
    '        'fecha = Format(obRES.Rows(0)("U_FECHA_AUTORIZACION"), "yyyy-MM-dd")
    '        'sXML &= String.Format("<FechaResolucion>{0}</FechaResolucion>", fecha) & vbCrLf
    '        'sXML &= String.Format("<RangoInicialAutorizado>{0}</RangoInicialAutorizado>", obRES.Rows(0)("U_FACTURA_DEL")) & vbCrLf
    '        'sXML &= String.Format("<RangoFinalAutorizado>{0}</RangoFinalAutorizado>", obRES.Rows(0)("U_FACTURA_AL")) & vbCrLf
    '        'sXML &= String.Format("</AsignacionSolicitada>", "") & vbCrLf
    '        sXML &= String.Format("<Procesamiento>", "") & vbCrLf
    '        sXML &= String.Format("<Dictionary name=""{0}"">", "email") & vbCrLf
    '        sXML &= String.Format("<Entry k=""from"" v=""{0}""/>", EmailFrom) & vbCrLf
    '        sXML &= String.Format("<Entry k=""to"" v=""{0}""/>", obOCRD.Rows(0)("E_mail")) & vbCrLf
    '        sXML &= String.Format("<Entry k=""cc"" v=""{0}""/>", "") & vbCrLf
    '        sXML &= String.Format("<Entry k=""formats"" v=""pdf""/>", "") & vbCrLf
    '        sXML &= String.Format("</Dictionary>", "") & vbCrLf
    '        sXML &= String.Format("</Procesamiento>", "") & vbCrLf
    '        sXML &= String.Format("<Encabezado>", "") & vbCrLf
    '        sXML &= String.Format("<TipoDeDocumento>INVOICE</TipoDeDocumento>", "") & vbCrLf
    '        sXML &= String.Format("<EstadoDeDocumento>ORIGINAL</EstadoDeDocumento>", "") & vbCrLf
    '        sXML &= String.Format("<CodigoDeMoneda>{0}</CodigoDeMoneda>", obOINV.Rows(0)("DocCur")) & vbCrLf
    '        sXML &= String.Format("<TipoDeCambio>{0}</TipoDeCambio>", obOINV.Rows(0)("DocRate")) & vbCrLf
    '        sXML &= String.Format("<InformacionDeRegimenIsr>PAGO_TRIMESTRAL</InformacionDeRegimenIsr>", "") & vbCrLf
    '        sXML &= String.Format("<ReferenciaInterna>{0}</ReferenciaInterna>", "PRUEBAS") & vbCrLf
    '        sXML &= String.Format("</Encabezado>", "") & vbCrLf

    '        sXML &= String.Format("<Vendedor>", "") & vbCrLf
    '        rows = obPAR.Select("U_PARAMETRO='NIT'")
    '        sXML &= String.Format("<Nit>{0}</Nit>", rows(0).Item(5)) & vbCrLf
    '        rows = obPAR.Select("U_PARAMETRO='NOMC'")
    '        sXML &= String.Format("<NombreComercial>{0}</NombreComercial>", rows(0).Item(5)) & vbCrLf
    '        sXML &= String.Format("<Idioma>es</Idioma>", "") & vbCrLf
    '        sXML &= String.Format("<DireccionDeEmisionDeDocumento>", "") & vbCrLf
    '        rows = obPAR.Select("U_PARAMETRO='NOME'")
    '        sXML &= String.Format("<NombreDeEstablecimiento>{0}</NombreDeEstablecimiento>", rows(0).Item(5)) & vbCrLf
    '        rows = obPAR.Select("U_PARAMETRO='CODE'")
    '        sXML &= String.Format("<CodigoDeEstablecimiento>{0}</CodigoDeEstablecimiento>", rows(0).Item(5)) & vbCrLf
    '        rows = obPAR.Select("U_PARAMETRO='DIRE'")
    '        sXML &= String.Format("<Direccion1>{0}</Direccion1>", rows(0).Item(5)) & vbCrLf
    '        sXML &= String.Format("<Municipio>{0}</Municipio>", obCountry.Rows(0)("City").ToUpper) & vbCrLf
    '        sXML &= String.Format("<Departamento>{0}</Departamento>", obCountry.Rows(0)("CityF").ToUpper) & vbCrLf
    '        sXML &= String.Format("<CodigoDePais>{0}</CodigoDePais>", obOADM.Rows(0)("Country")) & vbCrLf
    '        sXML &= String.Format("<CodigoPostal>{0}</CodigoPostal>", obCountry.Rows(0)("ZipCode")) & vbCrLf
    '        sXML &= String.Format("</DireccionDeEmisionDeDocumento>", "") & vbCrLf
    '        sXML &= String.Format("</Vendedor>", "") & vbCrLf

    '        sXML &= String.Format("<Comprador>", "") & vbCrLf
    '        sXML &= String.Format(" <Nit>8195641</Nit>", obOCRD1.Rows(0)("LicTradNum")) & vbCrLf
    '        sXML &= String.Format("<NombreComercial>{0}</NombreComercial>", obOCRD.Rows(0)("CardName")) & vbCrLf
    '        sXML &= String.Format("<Idioma>es</Idioma>", "") & vbCrLf
    '        sXML &= String.Format("<DireccionComercial>", "") & vbCrLf
    '        sXML &= String.Format("<Direccion1>{0}</Direccion1>", obOCRD.Rows(0)("MailAddres")) & vbCrLf
    '        sXML &= String.Format(" <Municipio>{0}</Municipio>", obOCRD1.Rows(0)("city")) & vbCrLf
    '        Dim depto As String = SESystem.Connection.DBConnection.TraeDato("SELECT name FROM OCST  WHERE code = '" & obOCRD1.Rows(0)("state") & "'")
    '        sXML &= String.Format("<Departamento>{0}</Departamento>", depto) & vbCrLf
    '        sXML &= String.Format("<CodigoDePais>{0}</CodigoDePais>", obOCRD1.Rows(0)("Country")) & vbCrLf
    '        sXML &= String.Format("<CodigoPostal>01010</CodigoPostal>", obOCRD1.Rows(0)("ZipCode")) & vbCrLf
    '        sXML &= String.Format("</DireccionComercial>", "") & vbCrLf
    '        sXML &= String.Format("</Comprador>", "") & vbCrLf
    '        sXML &= String.Format("<Detalles>", "") & vbCrLf

    '        obOINV1 = SESystem.Connection.DBConnection.EjecutaSqlTable("SELECT * FROM RIN1 WHERE DocEntry = " & obOINV.Rows(0)("DocEntry"))


    '        For Each obProd As DataRow In obOINV1.Rows
    '            sXML &= String.Format("<Detalle>", "") & vbCrLf
    '            sXML &= String.Format("<Descripcion>{0}</Descripcion>", obProd("Dscription")) & vbCrLf

    '            If Len(obProd("ItemCode")) < 14 Then
    '                sProdu = "00000000000001"
    '            Else
    '                sProdu = obProd("ItemCode")
    '            End If
    '            sXML &= String.Format("<CodigoEAN>{0}</CodigoEAN>", sProdu) & vbCrLf
    '            sXML &= String.Format("<UnidadDeMedida>{0}</UnidadDeMedida>", "") & vbCrLf
    '            sXML &= String.Format("<Cantidad>{0}</Cantidad>", obProd("Quantity")) & vbCrLf
    '            sXML &= String.Format("<ValoresBrutoLista>", "") & vbCrLf
    '            sXML &= String.Format("<Precio>{0}</Precio>", obProd("Price")) & vbCrLf
    '            sXML &= String.Format("<Monto>{0}</Monto>", obProd("linetotal")) & vbCrLf
    '            sXML &= String.Format("</ValoresBrutoLista>", "") & vbCrLf
    '            sXML &= String.Format("<ResumenDeDescuentos>", "") & vbCrLf

    '            'Calculando los descuentos
    '            dpDescuento = obProd("DiscPrcnt")
    '            If dpDescuento = 0 Then
    '                dtDescuento = 0
    '            Else
    '                dtDescuento = (obProd("linetotal") * dpDescuento) / 100
    '            End If


    '            'Calculando impuesto
    '            dpImpuesto = TraeDato("SELECT rate FROM OSTA  WHERE code = " & SESystem.Utils.Generales.scm(obProd("TaxCode")))
    '            dtImpuesto = obProd("linetotal") * (obProd("VatPrcnt") / 100)
    '            sTaxCode = obProd("TaxCode")


    '            sXML &= String.Format("<TotalDeDescuentos>{0}</TotalDeDescuentos>", dtDescuento) & vbCrLf
    '            sXML &= String.Format("<Descuentos>", "") & vbCrLf
    '            sXML &= String.Format("<Descuento>", "") & vbCrLf
    '            sXML &= String.Format("<Monto>{0}</Monto>", dtDescuento) & vbCrLf
    '            sXML &= String.Format("</Descuento>", "") & vbCrLf
    '            sXML &= String.Format("</Descuentos>", "") & vbCrLf
    '            sXML &= String.Format("</ResumenDeDescuentos>", "") & vbCrLf
    '            sXML &= String.Format("<ValoresNetoAPagar>", "") & vbCrLf
    '            sXML &= String.Format("<Precio>{0}</Precio>", obProd("linetotal")) & vbCrLf
    '            sXML &= String.Format("<Monto>{0}</Monto>", obProd("linetotal")) & vbCrLf
    '            sXML &= String.Format("</ValoresNetoAPagar>", "") & vbCrLf
    '            sXML &= String.Format("<ResumenDeImpuestos>", "") & vbCrLf
    '            sXML &= String.Format("<TotalDeImpuestos>{0}</TotalDeImpuestos>", dtImpuesto) & vbCrLf
    '            sXML &= String.Format("<IngresosNetosGravados>{0}</IngresosNetosGravados>", obProd("linetotal")) & vbCrLf
    '            sXML &= String.Format("<TotalDeIVA>{0}</TotalDeIVA>", dtImpuesto) & vbCrLf
    '            sXML &= String.Format("<Impuestos>", "") & vbCrLf
    '            sXML &= String.Format("<Impuesto>", "") & vbCrLf
    '            sXML &= String.Format("<Tipo>{0}</Tipo>", obProd("TaxCode")) & vbCrLf
    '            sXML &= String.Format("<Base>{0}</Base>", obProd("linetotal")) & vbCrLf
    '            sXML &= String.Format("<Tasa>{0}</Tasa>", dpImpuesto) & vbCrLf
    '            sXML &= String.Format("<Monto>{0}</Monto>", dtImpuesto) & vbCrLf
    '            sXML &= String.Format("</Impuesto>", "") & vbCrLf
    '            sXML &= String.Format("</Impuestos>", "") & vbCrLf
    '            sXML &= String.Format("</ResumenDeImpuestos>", "") & vbCrLf
    '            sXML &= String.Format("<Categoria>{0}</Categoria>", "") & vbCrLf
    '            sXML &= String.Format("<TextosDePosicion>", "") & vbCrLf
    '            sXML &= String.Format("<Texto>{0}</Texto>", "") & vbCrLf
    '            sXML &= String.Format("<Texto>{0}</Texto>", "") & vbCrLf
    '            sXML &= String.Format("<Texto>{0}</Texto>", "") & vbCrLf
    '            sXML &= String.Format("<Texto>{0}</Texto>", "") & vbCrLf
    '            sXML &= String.Format("<Texto>{0}</Texto>", "") & vbCrLf
    '            sXML &= String.Format("<Texto>{0}</Texto>", "") & vbCrLf
    '            sXML &= String.Format("</TextosDePosicion>", "") & vbCrLf
    '            sXML &= String.Format("</Detalle>", "") & vbCrLf
    '        Next

    '        sXML &= String.Format("</Detalles>", "") & vbCrLf


    '        dTotalBruto = obOINV.Rows(0)("DocTotal") - obOINV.Rows(0)("VatSum") - obOINV.Rows(0)("DiscSum")
    '        dTotalDescueto = obOINV.Rows(0)("DiscSum")
    '        dTotalIva = obOINV.Rows(0)("VatSum")

    '        sXML &= String.Format("<Totales>", "") & vbCrLf
    '        sXML &= String.Format("<TotalBrutoLista>{0}</TotalBrutoLista>", dTotalBruto) & vbCrLf
    '        sXML &= String.Format("<ResumenDeDescuentos>", "") & vbCrLf
    '        sXML &= String.Format("<TotalDeDescuentos>{0}</TotalDeDescuentos>", dTotalDescueto) & vbCrLf
    '        sXML &= String.Format("<Descuentos>", "") & vbCrLf
    '        sXML &= String.Format("<Descuento>", "") & vbCrLf
    '        sXML &= String.Format("<Monto>{0}</Monto>", dTotalDescueto) & vbCrLf
    '        sXML &= String.Format("</Descuento>", "") & vbCrLf
    '        sXML &= String.Format("</Descuentos>", "") & vbCrLf
    '        sXML &= String.Format("</ResumenDeDescuentos>", "") & vbCrLf
    '        sXML &= String.Format("<ResumenDeImpuestos>", "") & vbCrLf
    '        sXML &= String.Format("<TotalDeImpuestos>{0}</TotalDeImpuestos>", dTotalIva) & vbCrLf
    '        sXML &= String.Format("<IngresosNetosGravados>{0}</IngresosNetosGravados>", dTotalBruto) & vbCrLf
    '        sXML &= String.Format("<TotalDeIVA>{0}</TotalDeIVA>", dTotalIva) & vbCrLf
    '        sXML &= String.Format("<Impuestos>", "") & vbCrLf
    '        sXML &= String.Format("<Impuesto>", "") & vbCrLf
    '        sXML &= String.Format("<Tipo>{0}</Tipo>", sTaxCode) & vbCrLf
    '        sXML &= String.Format("<Base>{0}</Base>", dTotalBruto) & vbCrLf
    '        sXML &= String.Format("<Tasa>{0}</Tasa>", dtImpuesto) & vbCrLf
    '        sXML &= String.Format("<Monto>{0}</Monto>", dTotalIva) & vbCrLf
    '        sXML &= String.Format("</Impuesto>", "") & vbCrLf
    '        sXML &= String.Format("</Impuestos>", "") & vbCrLf
    '        sXML &= String.Format("</ResumenDeImpuestos>", "") & vbCrLf
    '        sXML &= String.Format("<TotalNetoAPagar>{0}</TotalNetoAPagar>", obOINV.Rows(0)("DocTotal")) & vbCrLf
    '        sXML &= String.Format("</Totales>", "") & vbCrLf
    '        sXML &= String.Format("<TextosDePie>", "") & vbCrLf
    '        sXML &= String.Format("<Texto>{0}</Texto>", "") & vbCrLf
    '        sXML &= String.Format("<Texto>{0}</Texto>", "") & vbCrLf
    '        sXML &= String.Format("<Texto>{0}</Texto>", "") & vbCrLf
    '        sXML &= String.Format("<Texto>{0}</Texto>", "") & vbCrLf
    '        sXML &= String.Format("<Texto>{0}</Texto>", "") & vbCrLf
    '        sXML &= String.Format("</TextosDePie>", "") & vbCrLf

    '        sXML &= String.Format("</FactDocGT>", "") & vbCrLf

    '        sXMLRetorno = sXML
    '        Return True
    '    Catch ex As Exception
    '        sMensajeRetorno = ex.Message
    '        Return False

    '    End Try



    'End Function

    Function GrabarXml(ByVal sXML As String, ByVal sSerie As String, ByVal sNumDoc As String, ByVal TipoDoc As String, ByRef fileName As String) As Boolean
        Dim sPathXMl As String
        Dim file As String = ""

        sPathXMl = TraeDato("SELECT [U_VALOR] FROM [@FACE_PARAMETROS]  WHERE [U_PARAMETRO] = 'PATHXML'")

        'Grabando XMl
        Try
            Dim xmlDoc As New XmlDocument
            Dim sNombreArchivo As String
            xmlDoc.LoadXml(sXML)
            If System.IO.Directory.Exists(sPathXMl) = False Then
                Throw New Exception("El path para almacenar el XML no existe")
            End If
            sNombreArchivo = Replace(String.Format("{0}\{3}{1}-{2}.xml", sPathXMl, sSerie, sNumDoc, TipoDoc), "\\", "\")
            Dim writer As New StreamWriter(sNombreArchivo)
            writer.Write(sXML)
            writer.Close()
            fileName = sNombreArchivo
            Return True
        Catch ex As Exception
            MsgBox(ex.Message)
            Return False
        End Try

    End Function

End Class
