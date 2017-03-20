'Imports SESystem.Connection.DBConnection
Imports System.Data.SqlClient

Module Utils
    Enum TipoFACE
        GuateFacturas
        Documenta
        GYT
        InFile
    End Enum

    Enum EmpresaFACE
        FFACSA
        PRINTER
        PEGASUS
        LEWONSKI
        LLAMASA
        QUALIPHARM
    End Enum


    Private _SBOApplication As SAPbouiCOM.Application
    Public Property SBOApplication() As SAPbouiCOM.Application
        Get
            Return _SBOApplication
        End Get
        Set(ByVal value As SAPbouiCOM.Application)
            _SBOApplication = value
        End Set
    End Property


    Private _Company As SAPbobsCOM.Company
    Public Property Company() As SAPbobsCOM.Company
        Get
            Return _Company
        End Get
        Set(ByVal value As SAPbobsCOM.Company)
            _Company = value
        End Set
    End Property

    Private Function ConvToHex(ByVal x As Integer) As String
        If x > 9 Then
            ConvToHex = Chr(x + 55)
        Else
            ConvToHex = CStr(x)
        End If
    End Function

    Public Function GeneraBatchTXT(ByVal OCompany As SAPbobsCOM.Company, ByVal Docentry As Integer, ByVal Tipo As String) As String
        Dim Sql As String
        Dim Ds As New DataSet
        Dim result As String = ""
        Try
            Sql = "EXEC SP_ITFACE_GENERATXTGUATEFAC " & Docentry & ",'" & Tipo & "'" 'No esta SP
            Ds = TraeDataset(Sql)
            If Ds.Tables(0).Rows.Count > 0 Then
                For t = 0 To Ds.Tables.Count - 1
                    For r = 0 To Ds.Tables(t).Rows.Count - 1
                        For c = 0 To Ds.Tables(t).Columns.Count - 1
                            result += Ds.Tables(t).Rows(r)(c).ToString & "|"
                        Next
                        result = Mid(result, 1, Len(result) - 1) & vbNewLine
                    Next
                Next
            End If
            Return result
        Catch ex As Exception
            Throw New Exception(ex.Message)
        End Try
    End Function

    Public Function EstadoFACE(ByVal OCompany As SAPbobsCOM.Company, ByVal DocEntry As String, ByVal TipoDoc As String) As String
        Dim RecSet As SAPbobsCOM.Recordset
        Dim sql As String = ""

        RecSet = OCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
        If TipoDoc = "ND" Then
            sql = ("CALL SP_FACE_UTILS('1','" & DocEntry & "','','','','')")
        Else
            sql = ("CALL SP_FACE_UTILS('2','" & DocEntry & "','','','','')")
        End If
        RecSet.DoQuery(sql)
        Return RecSet.Fields.Item("estado").Value.ToString
    End Function

    Public Function GetDateTimeServer(ByVal OCompany As SAPbobsCOM.Company) As String
        Dim RecSet As SAPbobsCOM.Recordset
        Dim sql As String = ""

        RecSet = OCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
        'sql = "select replace(convert(varchar, getdate(), 111),'/','-') +'T'+convert(varchar, getdate(), 108) Fecha"
        sql = ("CALL SP_FACE_UTILS('3','','','','','')")
        RecSet.DoQuery(sql)
        Return RecSet.Fields.Item("Fecha").Value.ToString
    End Function

    Public ReadOnly Property Modalidad() As String
        Get
            Return GetSettingValue("Modalidad")
        End Get

    End Property

    Private _Empresa As EmpresaFACE
    Public Property Empresa() As EmpresaFACE
        Get
            Return _Empresa
        End Get
        Set(ByVal value As EmpresaFACE)
            _Empresa = value
        End Set
    End Property


    Private _TipoGFace As TipoFACE
    Public Property TipoGFACE() As TipoFACE
        Get
            Return _TipoGFace
        End Get
        Set(ByVal value As TipoFACE)
            _TipoGFace = value
        End Set
    End Property


    Public Identifier As String = "564552303542415349533132333435363738393A4B31373137353837363039A6B39E34A014F3E85D030530664E1E18B479346B"

    ' función que codifica el dato  
    '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''  
    Public Function Encriptar(ByVal DataValue As Object) As Object

        Dim x As Long
        Dim Temp As String
        Dim TempNum As Integer
        Dim TempChar As String
        Dim TempChar2 As String

        For x = 1 To Len(DataValue)
            TempChar2 = Mid(DataValue, x, 1)
            TempNum = Int(Asc(TempChar2) / 16)

            If ((TempNum * 16) < Asc(TempChar2)) Then

                TempChar = ConvToHex(Asc(TempChar2) - (TempNum * 16))
                Temp = Temp & ConvToHex(TempNum) & TempChar
            Else
                Temp = Temp & ConvToHex(TempNum) & "0"

            End If
        Next x


        Encriptar = Temp
    End Function

    Private Function ConvToInt(ByVal x As String) As Integer

        Dim x1 As String
        Dim x2 As String
        Dim Temp As Integer

        x1 = Mid(x, 1, 1)
        x2 = Mid(x, 2, 1)

        If IsNumeric(x1) Then
            Temp = 16 * Int(x1)
        Else
            Temp = (Asc(x1) - 55) * 16
        End If

        If IsNumeric(x2) Then
            Temp = Temp + Int(x2)
        Else
            Temp = Temp + (Asc(x2) - 55)
        End If

        ' retorno  
        ConvToInt = Temp

    End Function

    ' función que decodifica el dato  
    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''  
    Public Function Desencriptar(ByVal DataValue As Object) As Object

        Dim x As Long
        Dim Temp As String
        Dim HexByte As String

        For x = 1 To Len(DataValue) Step 2

            HexByte = Mid(DataValue, x, 2)
            Temp = Temp & Chr(ConvToInt(HexByte))

        Next x
        ' retorno  
        Desencriptar = Temp

    End Function

    Public Sub EnviaDocumentoGuateFAC(ByVal OCompany As SAPbobsCOM.Company, ByVal SBO_Application As SAPbouiCOM.Application, ByVal Tipo As String, ByVal CurrSerie As String, ByVal CurrDoc As String, ByVal CurrSerieName As String, ByVal Pais As String, ByVal DocEntry As String, Optional ByVal ProcesarBatch As Boolean = False)
        'Dim envio As New clsCapillas
        Dim Envio As Object
        Dim Respuesta As String = ""
        Dim dbUser As String = ""
        Dim dbPass As String = ""
        Dim dirXML As String = ""
        Dim dirPDF As String = ""
        Dim NumFac As Long
        Dim Serie As String
        Dim CodSerie As Integer
        Dim oItem As SAPbouiCOM.Item
        Dim myNumFac As SAPbouiCOM.EditText
        Dim mySerie As SAPbouiCOM.ComboBox
        Dim WS As New WSGuateFAC.Guatefac
        Dim doc As New Xml.XmlDataDocument()
        Dim doc2 As New Xml.XmlDataDocument()
        Dim RecSet As SAPbobsCOM.Recordset
        Dim QryStr As String
        Dim xmlResp As String = ""
        Dim Requestor As String
        Dim Entity As String
        Dim User As String
        Dim UserName As String
        Dim EmailFrom As String
        Dim xmlFile As String = ""
        Dim filename As String
        Dim firma As String = ""
        Dim filenamePDF As String
        Dim TipoDoc As Decimal
        Dim Response As String
        Dim xmlWSResp As New Xml.XmlDocument
        Dim log As String = ""
        Dim Rs As SAPbobsCOM.Recordset
        Dim xmlDoc As New Xml.XmlDocument

        Try

            CodSerie = CurrSerie


            rs = OCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            If Tipo = "FAC" Or Tipo = "ND" Then
                'QryStr = "select docnum from oinv where docentry=" & DocEntry
                QryStr = ("CALL SP_FACE_UTILS('4','" & DocEntry & "','','','','')")
            Else
                'QryStr = "select docnum from orin where docentry=" & DocEntry
                QryStr = ("CALL SP_FACE_UTILS('5','" & DocEntry & "','','','','')")
            End If
            rs.DoQuery(QryStr)
            NumFac = CLng(rs.Fields.Item("docnum").Value.ToString)
            System.Runtime.InteropServices.Marshal.ReleaseComObject(rs)
            rs = Nothing
            GC.Collect()

            If ValidaSerie(OCompany, SBO_Application, CodSerie, ProcesarBatch) And ExisteDocumento(OCompany, SBO_Application, DocEntry, Tipo) Then

                dbUser = ObtieneValorParametro(OCompany, SBO_Application, "USRDB")
                dbPass = Utils.Desencriptar(ObtieneValorParametro(OCompany, SBO_Application, "PASSDB"))
                dirXML = ObtieneValorParametro(OCompany, SBO_Application, "PATHXML")
                'dirPDF = Me.ObtieneValorParametro("PATHPDF")
                Requestor = ObtieneValorParametro(OCompany, SBO_Application, "IFACE")
                Entity = ObtieneValorParametro(OCompany, SBO_Application, "IENT")
                'User = Me.ObtieneValorParametro("IUSR")
                'UserName = Me.ObtieneValorParametro("IUSRN")
                EmailFrom = ObtieneValorParametro(OCompany, SBO_Application, "EMAILF")
                WS.Url = ObtieneValorParametro(OCompany, SBO_Application, "URLWS")
                WS.Timeout = 800000

                'If Utils.Modalidad = "R" Then
                Dim myCredentials As New System.Net.CredentialCache()
                Dim netCred As New Net.NetworkCredential(Requestor, Entity)
                myCredentials.Add(New Uri(WS.Url), "Basic", netCred)
                WS.Credentials = myCredentials
                'End If

                If ProcesarBatch = False Then SBO_Application.SetStatusBarMessage("Enviando documento para su autorización eléctronica", SAPbouiCOM.BoMessageTime.bmt_Short, False)

                Dim xmlEnviado As String
                xmlEnviado = GeneraXML(SBO_Application, TipoFACE.Documenta, OCompany, DocEntry, Tipo, ProcesarBatch)
                'LogEnvio += "Se genero XML de Envio" & vbNewLine
                Try
                    xmlDoc.LoadXml(XmlEnviado)
                Catch ex As Exception
                    Throw New Exception(ex.Message)
                End Try
                'If Envio.GrabarXml(xmlResp, Serie, NumFac, Tipo, xmlFile) Then

                Dim sNombreArchivo As String = String.Format("{0}{3}{1}-{2}.xml", dirXML, CurrSerieName, NumFac, Tipo)
                Dim ErrorSave As String = ""
                'xmlDoc.Save(sNombreArchivo)

                If SaveToXML(xmlEnviado, sNombreArchivo, ErrorSave) = False Then
                    Throw New Exception("Error al guardar archivo xml :" & ErrorSave & vbNewLine & "Ubicación :" & sNombreArchivo)
                End If

                RecSet = OCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)

                Try
                    'TipoDoc = CInt(TraeDato("SELECT U_TIPO_DOC FROM [@FACE_RESOLUCION] WHERE U_SERIE = " & CodSerie))
                    'Dim Sucursal As Decimal = CInt(TraeDato("SELECT U_SUCURSAL FROM [@FACE_RESOLUCION] WHERE U_SERIE = " & CodSerie))
                    'Dim Maquina As String = TraeDato("SELECT U_DISPOSITIVO FROM [@FACE_RESOLUCION] WHERE U_SERIE = " & CodSerie)
                    TipoDoc = CInt(TraeDato("CALL SP_FACE_UTILS('6','" & CodSerie & "','','','','')"))
                    Dim Sucursal As Decimal = CInt(TraeDato("CALL SP_FACE_UTILS('7','" & CodSerie & "','','','','')"))
                    Dim Maquina As String = TraeDato("CALL SP_FACE_UTILS('8','" & CodSerie & "','','','','')")
                    Response = WS.generaDocumento(ObtieneValorParametro(OCompany, SBO_Application, "IUSR"), ObtieneValorParametro(OCompany, SBO_Application, "IUSRN"), ObtieneValorParametro(OCompany, SBO_Application, "NIT"), Sucursal, TipoDoc, Maquina, "R", xmlDoc.InnerXml)
                    If Response <> "" Then
                        Try
                            xmlWSResp.LoadXml(Response)
                            filename = dirXML & "Resp" & Tipo & Serie & NumFac & ".xml"
                            xmlWSResp.Save(filename)
                        Catch ex As Exception
                            Throw New Exception("Error en xml de Respuesta :" & Response & " Error app: " & ex.Message)
                        End Try
                    Else
                        Throw New Exception("Xml respuesta vacio")
                    End If
                Catch ex As Exception
                    Select Case Tipo
                        Case Is = "FAC"
                            'QryStr = "update OINV set U_ESTADO_FACE ='R',U_FACE_XML='" & doc2.InnerXml & "',U_MOTIVO_RECHAZO='" & ex.Message & "' WHERE  docentry=" & DocEntry
                            QryStr = ("CALL SP_FACE_UTILS('10','" & doc2.InnerXml & "','" & ex.Message & "','" & DocEntry & "','','')")
                        Case Is = "ND"
                            'QryStr = "update OINV set U_ESTADO_FACE ='R',U_FACE_XML='" & doc2.InnerXml & "',U_MOTIVO_RECHAZO='" & ex.Message & "' WHERE docentry=" & DocEntry
                            QryStr = ("CALL SP_FACE_UTILS('10','" & doc2.InnerXml & "','" & ex.Message & "','" & DocEntry & "','','')")
                        Case Is = "NC"
                            'QryStr = "update OINV set U_ESTADO_FACE ='R',U_FACE_XML='" & doc2.InnerXml & "',U_MOTIVO_RECHAZO='" & ex.Message & "' WHERE docentry=" & DocEntry
                            QryStr = ("CALL SP_FACE_UTILS('10','" & doc2.InnerXml & "','" & ex.Message & "','" & DocEntry & "','','')")
                    End Select
                    RecSet.DoQuery(QryStr)
                    SBO_Application.SetStatusBarMessage("Falla al intentar registrar el documento , motivo de la fálla: " & ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, True)
                    Exit Sub
                End Try

                If InStr(xmlWSResp.InnerXml, "Preimpreso") > 0 Then

                    log += "Documento: " & Serie & " " & NumFac & " Estado: Aprobado" & vbNewLine

                    Dim xmlFirma As Xml.XmlNodeList = xmlWSResp.GetElementsByTagName("Firma")
                    Dim xmlSerie As Xml.XmlNodeList = xmlWSResp.GetElementsByTagName("Serie")
                    Dim xlPreimpreso As Xml.XmlNodeList = xmlWSResp.GetElementsByTagName("Preimpreso")
                    Dim xmlNombre As Xml.XmlNodeList = xmlWSResp.GetElementsByTagName("Nombre")
                    Dim xmlDir As Xml.XmlNodeList = xmlWSResp.GetElementsByTagName("Direccion")
                    'Dim fields As String

                    'fields = "U_ESTADO_FACE='A',"
                    'fields += "U_FACE_XML='" & xmlWSResp.InnerXml & "',"
                    'fields += "U_FIRMA_ELETRONICA='" & xmlFirma(0).InnerText & "',"
                    'fields += "U_NUMERO_DOCUMENTO='" & xlPreimpreso(0).InnerText & "',"
                    ''fields += "U_NOMBRE='" & xmlNombre(0).InnerText & "',"
                    ''fields += "U_DIRECCION='" & xmlDir(0).InnerText & "',"
                    'fields += "U_SERIE_FACE='" & xmlSerie(0).InnerText & "'"
                    ''fields += "U_FACTURA_INI='" & IniAut(0).InnerText & "',"
                    ''fields += "U_FACTURA_FIN='" & FinAut(0).InnerText & "' "

                    Select Case Tipo
                        Case Is = "FAC"
                            'QryStr = "update OINV set " & fields & " WHERE docentry=" & DocEntry
                            QryStr = ("CALL SP_FACE_UTILS('11','" & DocEntry & ",','" & xmlWSResp.InnerXml & ",','" & xmlFirma(0).InnerText & ",','" & xlPreimpreso(0).InnerText & ",','" & xmlSerie(0).InnerText & "')")
                        Case Is = "ND"
                            'QryStr = "update OINV set " & fields & " WHERE  docentry=" & DocEntry
                            QryStr = ("CALL SP_FACE_UTILS('11','" & DocEntry & ",','" & xmlWSResp.InnerXml & ",','" & xmlFirma(0).InnerText & ",','" & xlPreimpreso(0).InnerText & ",','" & xmlSerie(0).InnerText & "')")
                        Case Is = "NC"
                            'QryStr = "update ORIN set " & fields & " WHERE docentry=" & DocEntry
                            QryStr = ("CALL SP_FACE_UTILS('26','" & DocEntry & ",','" & xmlWSResp.InnerXml & ",','" & xmlFirma(0).InnerText & ",','" & xlPreimpreso(0).InnerText & ",','" & xmlSerie(0).InnerText & "')")
                    End Select
                    RecSet.DoQuery(QryStr)
                    If ProcesarBatch = False Then SBO_Application.SetStatusBarMessage("Documento electrónico ha sido autorizado correctamente", SAPbouiCOM.BoMessageTime.bmt_Short, False)
                Else
                    Dim RespuestaXML As Xml.XmlNodeList = xmlWSResp.GetElementsByTagName("Resultado")

                    log += "Documento: " & Serie & " " & NumFac & " Estado: Rechazado Motivo:" & RespuestaXML.Item(0).InnerText & vbNewLine

                    Select Case Tipo
                        Case Is = "FAC"
                            'QryStr = "update OINV set U_ESTADO_FACE ='R',U_FACE_XML='" & xmlWSResp.InnerXml & "',U_MOTIVO_RECHAZO='" & RespuestaXML.Item(0).InnerText & "' WHERE  docentry=" & DocEntry
                            QryStr = ("CALL SP_FACE_UTILS('12','" & xmlWSResp.InnerXml & "','" & RespuestaXML.Item(0).InnerText & "','" & DocEntry & "','','')")
                        Case Is = "ND"
                            'QryStr = "update OINV set U_ESTADO_FACE ='R',U_FACE_XML='" & xmlWSResp.InnerXml & "',U_MOTIVO_RECHAZO='" & RespuestaXML.Item(0).InnerText & "' WHERE  docentry=" & DocEntry
                            QryStr = ("CALL SP_FACE_UTILS('12','" & xmlWSResp.InnerXml & "','" & RespuestaXML.Item(0).InnerText & "','" & DocEntry & "','','')")
                        Case Is = "NC"
                            'QryStr = "update OINV set U_ESTADO_FACE ='R',U_FACE_XML='" & xmlWSResp.InnerXml & "',U_MOTIVO_RECHAZO='" & RespuestaXML.Item(0).InnerText & "' WHERE  docentry=" & DocEntry
                            QryStr = ("CALL SP_FACE_UTILS('12','" & xmlWSResp.InnerXml & "','" & RespuestaXML.Item(0).InnerText & "','" & DocEntry & "','','')")
                    End Select
                    RecSet.DoQuery(QryStr)
                    If ProcesarBatch = False Then SBO_Application.SetStatusBarMessage("Registro de documento electrónico fallído, motivo del rechazó: " & RespuestaXML.Item(0).InnerText, SAPbouiCOM.BoMessageTime.bmt_Short)
                End If
            End If

            'End If

        Catch ex As Exception
            log += ex.Message & vbNewLine
            If ProcesarBatch = False Then SBO_Application.SetStatusBarMessage(ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short)
        End Try
        If log <> "" Then
            Dim mylog As String
            mylog = GetFileContents(Utils.FileLog) & log
            SaveTextToFile(mylog, Utils.FileLog)
        End If
    End Sub


    Public Sub AddUserTable(ByVal oCompany As SAPbobsCOM.Company, ByVal TableName As String, ByVal TableDescription As String, ByVal typeTable As SAPbobsCOM.BoUTBTableType)

        Dim oUserTablesMD As SAPbobsCOM.UserTablesMD
        Dim iResult As Long
        Dim sMsg As String
        Dim sTable As String

        Try
            oUserTablesMD = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserTables)

            If (oUserTablesMD.GetByKey(TableName) = False) Then

                oUserTablesMD.TableName = TableName
                oUserTablesMD.TableDescription = TableDescription
                oUserTablesMD.TableType = typeTable
                oUserTablesMD.Add()

                oUserTablesMD.Update()

                System.Runtime.InteropServices.Marshal.ReleaseComObject(oUserTablesMD)
                oUserTablesMD = Nothing
                GC.Collect()
            End If


        Catch ex As Exception
            Throw New Exception(ex.Message)
        End Try

    End Sub

    Public Sub AddUserField(ByVal oCompany As SAPbobsCOM.Company, ByVal TableName As String, ByVal FieldName As String, ByVal FieldDescription As String, ByVal FieldType As SAPbobsCOM.BoFieldTypes, ByVal Size As Integer, Optional ByVal addSymbol As Boolean = True, Optional ByVal SubType As SAPbobsCOM.BoFldSubTypes = Nothing)

        Dim AddT As Integer
        Dim lerrcode As Integer
        Dim serrmsg As String = ""

        Try

            If ExistField(oCompany, TableName, FieldName, addSymbol) = False Then

                Dim oUserFieldsMD As SAPbobsCOM.UserFieldsMD
                oUserFieldsMD = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserFields)
                oUserFieldsMD.TableName = TableName
                oUserFieldsMD.Name = FieldName
                oUserFieldsMD.Description = FieldDescription
                oUserFieldsMD.Type = FieldType
                If Not IsNothing(oUserFieldsMD.SubType) Then oUserFieldsMD.SubType = SubType
                If FieldType = 2 Or FieldType = 0 Then
                    oUserFieldsMD.EditSize = Size
                End If
                AddT = oUserFieldsMD.Add

                If AddT <> 0 Then
                    oCompany.GetLastError(lerrcode, serrmsg)
                    Throw New Exception(serrmsg)
                Else
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oUserFieldsMD)
                    oUserFieldsMD = Nothing
                    GC.Collect()
                End If
            End If
        Catch ex As Exception
            Throw New Exception(ex.Message)
        End Try
    End Sub

    Public Sub AddDocumentType(ByVal oCompany As SAPbobsCOM.Company, ByVal Code As String, ByVal Description As String, ByVal LineID As Integer, Optional ByVal Code2 As String = "")
        Dim RecSet As SAPbobsCOM.Recordset
        Dim sql As String = ""

        Try
            If Code2 <> "" Then
                'sql = "select * from [@FACE_TIPODOC] where U_codigo='" & Code & "' and code='" & Code2 & "'"
                sql = ("CALL SP_FACE_QUERYS('3','" & Code & "','" & Code2 & "')")
            Else
                'sql = "select * from [@FACE_TIPODOC] where U_codigo='" & Code & "'"
                sql = ("CALL SP_FACE_QUERYS('4','" & Code & "','')")
            End If
            RecSet = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            RecSet.DoQuery(sql)
            If RecSet.RecordCount = 0 Then
                If Code2 <> "" Then
                    'sql = "insert into [@FACE_TIPODOC] (code,lineid,u_codigo,u_descripcion) values('" & Code2 & "'," & LineID & ",'" & Code & "','" & Description & "')"
                    sql = ("CALL SP_FACE_QUERYS_4P('1','" & Code2 & "','" & LineID & "','" & Code & "','" & Description & "')")
                Else
                    'sql = "insert into [@FACE_TIPODOC] (code,lineid,u_codigo,u_descripcion) values('" & Code & "'," & LineID & ",'" & Code & "','" & Description & "')"
                    sql = ("CALL SP_FACE_QUERYS_4P('1','" & Code & "','" & LineID & "','" & Code & "','" & Description & "')")
                End If
                RecSet.DoQuery(sql)
            End If
        Catch ex As Exception
            Throw New Exception(ex.Message)
        End Try
    End Sub

    Private Function ExistField(ByVal oCompany As SAPbobsCOM.Company, ByVal TableName As String, ByVal FieldName As String, ByVal addSymbol As Boolean) As Boolean
        Dim RecSet As SAPbobsCOM.Recordset
        Dim QryStr As String = ""
        Dim result As Boolean = False

        Try
            If addSymbol Then
                TableName = "@" & TableName
            End If
            'QryStr = "select TableID,FieldID,AliasID from CUFD WHERE TableID='" & TableName & "' and AliasID  ='" & FieldName & "'"
            QryStr = ("CALL SP_FACE_QUERYS( '1','" & TableName & "','" & FieldName & "')")
            RecSet = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            RecSet.DoQuery(QryStr)
            If RecSet.RecordCount > 0 Then
                result = True
            End If
            System.Runtime.InteropServices.Marshal.ReleaseComObject(RecSet)
            RecSet = Nothing
            GC.Collect()
            Return result
        Catch ex As Exception
            Throw New Exception(ex.Message)
        End Try
    End Function

    Public ReadOnly Property ConnectionString() As String
        Get
            'Return GetSettingValue("Connection")
            Return Environment.GetCommandLineArgs.GetValue(1)
        End Get

    End Property

    Public Function TotalExcento(ByVal docEntry As String, ByVal Tipo As String) As Decimal
        Dim sql As String
        Try
            If Tipo = "FAC" Or Tipo = "ND" Then
                'sql = "select isnull(sum( case doctype when 'S' then 1 else b.Quantity end * b.PriceAfVat),0)" & _
                '      "from oinv a " & _
                '      "inner join INV1 b " & _
                '      "on a.DocEntry=b.docentry " & _
                '      "where a.docentry =  " & docEntry & _
                '      " and   b.TaxCode='EXE' "
                sql = ("CALL SP_FACE_UTILS('13','" & docEntry & "','','','','')")
            Else
                'sql = "select isnull(sum( case doctype when 'S' then 1 else b.Quantity end * b.PriceAfVat),0)" & _
                '      "from oinv a " & _
                '      "inner join RIN1 b " & _
                '      "on a.DocEntry=b.docentry " & _
                '      "where a.docentry =  " & docEntry & _
                '      " and   b.TaxCode='EXE' "
                sql = ("CALL SP_FACE_UTILS('14','" & docEntry & "','','','','')")
            End If

            Return CDec(TraeDato(sql))
        Catch ex As Exception
            Throw New Exception(ex.Message)
        End Try
    End Function

    Public Function GeneraPDF(ByVal Tipo As String, ByVal oCompany As SAPbobsCOM.Company, ByVal serie As String, ByVal NumFAC As String, ByRef filenamePDF As String) As Boolean
        Dim xml As String = ""
        Dim requestor As String = ""
        Dim Country As String = Utils.Pais
        Dim Entity As String = ""
        Dim User As String = ""
        Dim UserName As String = ""
        Dim data1 As String = ""
        Dim data2 As String = ""
        Dim RecSet As SAPbobsCOM.Recordset
        Dim QryStr As String = ""
        Dim WS As New WSFace.FactWSFront
        Dim tag As New WSFace.TransactionTag
        Dim dirPDF As String

        Try
            If Utils.TipoGFACE = TipoFACE.Documenta Or Utils.TipoGFACE = TipoFACE.GYT Then
                QryStr = "select * from [@FACE_PARAMETROS] "
                QryStr = ("CALL SP_FACE_QUERYS('5','','')")
                RecSet = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                RecSet.DoQuery(QryStr)
                If RecSet.RecordCount > 0 Then
                    requestor = RecSet.Fields.Item("IFACE").Value
                    Entity = RecSet.Fields.Item("IENT").Value
                    User = RecSet.Fields.Item("IUSR").Value
                    UserName = RecSet.Fields.Item("IUSRN").Value
                    dirPDF = RecSet.Fields.Item("PATHPDF").Value
                    xml = "<?xml version=""""1.0"""" encoding=""""utf-8""""?>"
                    xml += "<soap:Envelope xmlns:xsi=""""http://www.w3.org/2001/XMLSchema-instance"""""
                    xml += "xmlns:      xsd = """"http://www.w3.org/2001/XMLSchema"""""
                    xml += "xmlns:soap=""""http://schemas.xmlsoap.org/soap/envelope/"""">"
                    xml += "<soap:Body>"
                    xml += "<RequestTransaction xmlns=""""http://www.fact.com.mx/schema/ws"""">"
                    xml += "<Requestor>" & requestor & "</Requestor>"
                    xml += "<Transaction>GET_DOCUMENT</Transaction>"
                    xml += "<Country>" & Pais & "</Country>"
                    xml += "<Entity>" & Entity & "</Entity>"
                    xml += "<User>" & User & "</User>"
                    xml += "<UserName" & UserName & "</UserName>"
                    xml += "<Data1>" & serie & "</Data1>"
                    xml += "<Data2>" & NumFAC & "</Data2>"
                    xml += "<Data3>PDF</Data3>"
                    xml += "</RequestTransaction>"
                    xml += "</soap:Body>"
                    xml += "</soap:Envelope>"
                    tag = WS.RequestTransaction(requestor, "GET_DOCUMENT", Pais, Entity, User, UserName, xml, "PDF", "")
                    If tag.Response.Result Then
                        If System.IO.Directory.Exists(dirPDF) = False Then
                            Throw New Exception("El path para almacenar el PDF no existe")
                        End If
                        filenamePDF = Replace(dirPDF & "\" & Tipo & serie & NumFAC & ".pdf", "\\", "\")
                        Dim oFileStream As System.IO.FileStream = New IO.FileStream(filenamePDF, System.IO.FileMode.Create)
                        oFileStream.Write(Base64String_ByteArray(tag.ResponseData.ResponseData3), 0, Base64String_ByteArray(tag.ResponseData.ResponseData3).Length)
                        oFileStream.Close()
                    Else
                        Throw New Exception("No se logro generar el PDF por el siguiente motivo: " & tag.Response.Description)
                    End If
                Else
                    Throw New Exception("Su proveedor de facturación electrónica no soporta esta funcionalidad")
                End If
            End If
            System.Runtime.InteropServices.Marshal.ReleaseComObject(RecSet)
            RecSet = Nothing
            GC.Collect()

            Return True
        Catch ex As Exception
            Throw New Exception(ex.Message)
        End Try
    End Function

    Public Function TotalNeto(ByVal docEntry As String, ByVal Tipo As String) As Decimal
        Dim sql As String
        Try
            If Tipo = "FAC" Or Tipo = "ND" Then
                'sql = "select isnull(SUM(LineTotal),0) from INV1 where TaxCode <>'EXE' and DocEntry=" & docEntry
                sql = ("CALL SP_FACE_UTILS('15','" & docEntry & "','','','','')")
            Else
                'sql = "select isnull(SUM(LineTotal),0) from RIN1 where TaxCode <>'EXE' and DocEntry=" & docEntry
                sql = ("CALL SP_FACE_UTILS('16','" & docEntry & "','','','','')")
            End If
            Return CDec(TraeDato(sql))
        Catch ex As Exception
            Throw New Exception(ex.Message)
        End Try
    End Function

    'Public Function TotalNeto(ByVal docEntry As String) As Decimal
    '    Dim sql As String
    '    Try
    '        sql = "select isnull(sum(b.Quantity * b.Price),0)" & _
    '              "from oinv a " & _
    '              "inner join INV1 b " & _
    '              "on a.DocEntry=b.docentry " & _
    '              "where a.docentry =  " & docEntry & _
    '              " and   b.TaxCode<>'EXE' "
    '        Return CDec(TraeDato(sql))
    '    Catch ex As Exception
    '        Throw New Exception(ex.Message)
    '    End Try
    'End Function

    Public ReadOnly Property Pais() As String
        Get
            Return "GT"
        End Get

    End Property

    Public Function GetSettingValue(ByVal Setting As String) As String
        Dim xml As New Xml.XmlDocument
        Dim xmlfile As String = Replace(Application.StartupPath & "\Settings.xml", "\\", "\")
        Try
            If IO.File.Exists(xmlfile) = False Then
                Throw New Exception("No existe el archivo de configuracion de la aplicacion")
            End If
            xml.Load(xmlfile)
            Return xml.SelectSingleNode("//Settings/" & Setting).InnerText
        Catch ex As Exception
            Throw New Exception(ex.Message)
        End Try
    End Function

    Public Function Letras(ByVal numero As String) As String
        '********Declara variables de tipo cadena************
        Dim palabras, entero, dec, flag As String

        '********Declara variables de tipo entero***********
        Dim num, x, y As Integer

        flag = "N"

        '**********Número Negativo***********
        If Mid(numero, 1, 1) = "-" Then
            numero = Mid(numero, 2, numero.ToString.Length - 1).ToString
            palabras = "menos "
        End If

        '**********Si tiene ceros a la izquierda*************
        For x = 1 To numero.ToString.Length
            If Mid(numero, 1, 1) = "0" Then
                numero = Trim(Mid(numero, 2, numero.ToString.Length).ToString)
                If Trim(numero.ToString.Length) = 0 Then palabras = ""
            Else
                Exit For
            End If
        Next

        '*********Dividir parte entera y decimal************
        For y = 1 To Len(numero)
            If Mid(numero, y, 1) = "." Then
                flag = "S"
            Else
                If flag = "N" Then
                    entero = entero + Mid(numero, y, 1)
                Else
                    dec = dec + Mid(numero, y, 1)
                End If
            End If
        Next y

        If Len(dec) = 1 Then dec = dec & "0"

        '**********proceso de conversión***********
        flag = "N"

        If Val(numero) <= 999999999 Then
            For y = Len(entero) To 1 Step -1
                num = Len(entero) - (y - 1)
                Select Case y
                    Case 3, 6, 9
                        '**********Asigna las palabras para las centenas***********
                        Select Case Mid(entero, num, 1)
                            Case "1"
                                If Mid(entero, num + 1, 1) = "0" And Mid(entero, num + 2, 1) = "0" Then
                                    palabras = palabras & "cien "
                                Else
                                    palabras = palabras & "ciento "
                                End If
                            Case "2"
                                palabras = palabras & "doscientos "
                            Case "3"
                                palabras = palabras & "trescientos "
                            Case "4"
                                palabras = palabras & "cuatrocientos "
                            Case "5"
                                palabras = palabras & "quinientos "
                            Case "6"
                                palabras = palabras & "seiscientos "
                            Case "7"
                                palabras = palabras & "setecientos "
                            Case "8"
                                palabras = palabras & "ochocientos "
                            Case "9"
                                palabras = palabras & "novecientos "
                        End Select
                    Case 2, 5, 8
                        '*********Asigna las palabras para las decenas************
                        Select Case Mid(entero, num, 1)
                            Case "1"
                                If Mid(entero, num + 1, 1) = "0" Then
                                    flag = "S"
                                    palabras = palabras & "diez "
                                End If
                                If Mid(entero, num + 1, 1) = "1" Then
                                    flag = "S"
                                    palabras = palabras & "once "
                                End If
                                If Mid(entero, num + 1, 1) = "2" Then
                                    flag = "S"
                                    palabras = palabras & "doce "
                                End If
                                If Mid(entero, num + 1, 1) = "3" Then
                                    flag = "S"
                                    palabras = palabras & "trece "
                                End If
                                If Mid(entero, num + 1, 1) = "4" Then
                                    flag = "S"
                                    palabras = palabras & "catorce "
                                End If
                                If Mid(entero, num + 1, 1) = "5" Then
                                    flag = "S"
                                    palabras = palabras & "quince "
                                End If
                                If Mid(entero, num + 1, 1) > "5" Then
                                    flag = "N"
                                    palabras = palabras & "dieci"
                                End If
                            Case "2"
                                If Mid(entero, num + 1, 1) = "0" Then
                                    palabras = palabras & "veinte "
                                    flag = "S"
                                Else
                                    palabras = palabras & "veinti"
                                    flag = "N"
                                End If
                            Case "3"
                                If Mid(entero, num + 1, 1) = "0" Then
                                    palabras = palabras & "treinta "
                                    flag = "S"
                                Else
                                    palabras = palabras & "treinta y "
                                    flag = "N"
                                End If
                            Case "4"
                                If Mid(entero, num + 1, 1) = "0" Then
                                    palabras = palabras & "cuarenta "
                                    flag = "S"
                                Else
                                    palabras = palabras & "cuarenta y "
                                    flag = "N"
                                End If
                            Case "5"
                                If Mid(entero, num + 1, 1) = "0" Then
                                    palabras = palabras & "cincuenta "
                                    flag = "S"
                                Else
                                    palabras = palabras & "cincuenta y "
                                    flag = "N"
                                End If
                            Case "6"
                                If Mid(entero, num + 1, 1) = "0" Then
                                    palabras = palabras & "sesenta "
                                    flag = "S"
                                Else
                                    palabras = palabras & "sesenta y "
                                    flag = "N"
                                End If
                            Case "7"
                                If Mid(entero, num + 1, 1) = "0" Then
                                    palabras = palabras & "setenta "
                                    flag = "S"
                                Else
                                    palabras = palabras & "setenta y "
                                    flag = "N"
                                End If
                            Case "8"
                                If Mid(entero, num + 1, 1) = "0" Then
                                    palabras = palabras & "ochenta "
                                    flag = "S"
                                Else
                                    palabras = palabras & "ochenta y "
                                    flag = "N"
                                End If
                            Case "9"
                                If Mid(entero, num + 1, 1) = "0" Then
                                    palabras = palabras & "noventa "
                                    flag = "S"
                                Else
                                    palabras = palabras & "noventa y "
                                    flag = "N"
                                End If
                        End Select
                    Case 1, 4, 7
                        '*********Asigna las palabras para las unidades*********
                        Select Case Mid(entero, num, 1)
                            Case "1"
                                If flag = "N" Then
                                    If y = 1 Then
                                        palabras = palabras & "uno "
                                    Else
                                        palabras = palabras & "un "
                                    End If
                                End If
                            Case "2"
                                If flag = "N" Then palabras = palabras & "dos "
                            Case "3"
                                If flag = "N" Then palabras = palabras & "tres "
                            Case "4"
                                If flag = "N" Then palabras = palabras & "cuatro "
                            Case "5"
                                If flag = "N" Then palabras = palabras & "cinco "
                            Case "6"
                                If flag = "N" Then palabras = palabras & "seis "
                            Case "7"
                                If flag = "N" Then palabras = palabras & "siete "
                            Case "8"
                                If flag = "N" Then palabras = palabras & "ocho "
                            Case "9"
                                If flag = "N" Then palabras = palabras & "nueve "
                        End Select
                End Select

                '***********Asigna la palabra mil***************
                If y = 4 Then
                    If Mid(entero, 6, 1) <> "0" Or Mid(entero, 5, 1) <> "0" Or Mid(entero, 4, 1) <> "0" Or _
                    (Mid(entero, 6, 1) = "0" And Mid(entero, 5, 1) = "0" And Mid(entero, 4, 1) = "0" And _
                    Len(entero) <= 6) Then palabras = palabras & "mil "
                End If

                '**********Asigna la palabra millón*************
                If y = 7 Then
                    If Len(entero) = 7 And Mid(entero, 1, 1) = "1" Then
                        palabras = palabras & "millón "
                    Else
                        palabras = palabras & "millones "
                    End If
                End If
            Next y

            '**********Une la parte entera y la parte decimal*************
            If dec <> "" Then
                Letras = palabras & "con " & dec
            Else
                Letras = palabras
            End If
        Else
            Letras = ""
        End If
    End Function

    Public Function ValidaDocumento(ByVal OCompany As SAPbobsCOM.Company, ByVal SBO_Application As SAPbouiCOM.Application, ByVal DocEntry As String, ByVal TypeDoc As String) As Boolean
        Dim RecSet As SAPbobsCOM.Recordset
        Dim QryStr As String
        Dim result As Boolean = False
        Try
            If TypeDoc = "FAC" Or TypeDoc = "ND" Then
                RecSet = OCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                'QryStr = "select * from OINV WHERE  docentry=" & DocEntry & " and isnull(u_estado_face,'P')='A'"
                QryStr = ("CALL SP_FACE_UTILS('17','" & DocEntry & "','','','','')")
                RecSet.DoQuery(QryStr)
                If RecSet.RecordCount > 0 Then
                    result = True
                End If
            End If
            If TypeDoc = "NC" Then
                RecSet = OCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                'QryStr = "select * from ORIN WHERE  docentry=" & DocEntry & " and isnull(u_estado_face,'P')='A'"
                QryStr = ("CALL SP_FACE_UTILS('18','" & DocEntry & "','','','','')")
                RecSet.DoQuery(QryStr)
                If RecSet.RecordCount > 0 Then
                    result = True
                End If
            End If
            If TypeDoc = "FACP" Then
                RecSet = OCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                'QryStr = "select * from OPCH WHERE  docentry=" & DocEntry & " and isnull(u_estado_face,'P')='A'"
                QryStr = ("CALL SP_FACE_UTILS('19','" & DocEntry & "','','','','')")
                RecSet.DoQuery(QryStr)
                If RecSet.RecordCount > 0 Then
                    result = True
                End If
            End If
            System.Runtime.InteropServices.Marshal.ReleaseComObject(RecSet)
            RecSet = Nothing
            GC.Collect()
            Return result
        Catch ex As Exception
            SBO_Application.MessageBox(ex.Message)
            Return False
        End Try
    End Function

    Public Function SerieEsBatch(ByVal OCompany As SAPbobsCOM.Company, ByVal SBO_Application As SAPbouiCOM.Application, ByVal CodeSerie As String) As Boolean
        Dim sql As String
        Dim RecSet As SAPbobsCOM.Recordset

        Try
            'sql = "SELECT isnull(U_ES_BATCH,'N') FROM [@FACE_RESOLUCION] WHERE CODE=" & CodeSerie
            sql = ("CALL SP_FACE_UTILS('20','" & CodeSerie & "','','','','')")
            RecSet = OCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            RecSet.DoQuery(sql)
            If RecSet.RecordCount > 0 Then
                If RecSet.Fields.Item(0).Value = "Y" Then
                    Return True
                Else
                    Return False
                End If
            End If
        Catch ex As Exception
            SBO_Application.MessageBox(ex.Message)

        End Try
    End Function

    Public Sub EnviaDocumento(ByVal OCompany As SAPbobsCOM.Company, ByVal SBO_Application As SAPbouiCOM.Application, ByVal Tipo As String, ByVal CurrSerie As String, ByVal CurrDoc As String, ByVal CurrSerieName As String, ByVal Pais As String, ByVal docEntry As String, Optional ByVal ProcesarBatch As Boolean = False, Optional ByVal Linea As String = "", Optional ByRef Log As String = "")

        'Utils.FileLog = Replace(Application.StartupPath & "\Resultados " & Format(Date.Now, "ddMMyyyyHHmmss") & ".txt", "\\", "\")

        ProcesarBatch = SerieEsBatch(OCompany, SBO_Application, CurrSerie)

        If ValidaDocumento(OCompany, SBO_Application, docEntry, Tipo) Then
            Log += "Documento ya se encuentra autorizado" & vbNewLine
            If ProcesarBatch = False Then SBO_Application.SetStatusBarMessage("El documento ya se encuentra con estado de autorizado", SAPbouiCOM.BoMessageTime.bmt_Short, False)
            Exit Sub
        End If


        Select Case Utils.TipoGFACE
            Case TipoFACE.Documenta
                If Utils.Empresa = EmpresaFACE.LLAMASA Then
                    EnviaDocFACE(OCompany, SBO_Application, Tipo, CurrSerie, docEntry, ProcesarBatch, Linea, , Log)
                Else
                    EnviaDocumentoOtros(OCompany, SBO_Application, Tipo, CurrSerie, CurrDoc, CurrSerieName, Pais, docEntry, ProcesarBatch)
                End If
            Case TipoFACE.GuateFacturas
                EnviaDocumentoGuateFAC(OCompany, SBO_Application, Tipo, CurrSerie, CurrDoc, CurrSerieName, Pais, docEntry, ProcesarBatch)
            Case TipoFACE.GYT
                If Utils.Empresa = EmpresaFACE.FFACSA Then
                    EnviaDocFACE(OCompany, SBO_Application, Tipo, CurrSerie, docEntry, ProcesarBatch, Linea)
                Else
                    EnviaDocumentoOtros(OCompany, SBO_Application, Tipo, CurrSerie, CurrDoc, CurrSerieName, Pais, docEntry, ProcesarBatch)
                End If
            Case TipoFACE.InFile
                If Utils.Empresa = EmpresaFACE.QUALIPHARM Or Utils.Empresa = EmpresaFACE.PRINTER Or Utils.Empresa = EmpresaFACE.PEGASUS Then
                    EnviaDocumentoInFileSP(OCompany, SBO_Application, Tipo, CurrSerie, CurrDoc, CurrSerieName, Pais, docEntry, ProcesarBatch)
                Else
                    EnviaDocumentoInFile(OCompany, SBO_Application, Tipo, CurrSerie, CurrDoc, CurrSerieName, Pais, docEntry, ProcesarBatch)
                End If
        End Select
    End Sub

    Private Sub EnviaDocFACE(ByVal OCompany As SAPbobsCOM.Company, ByVal SBO_Application As SAPbouiCOM.Application, ByVal Tipo As String, ByVal CodSerie As String, ByVal DocEntry As String, Optional ByVal ProcesarBatch As Boolean = False, Optional ByVal Linea As String = "", Optional ByRef LogBatch As String = "", Optional ByVal LogEnvio As String = "")
        Dim dbUser As String
        Dim dbPass As String
        Dim dirXML As String
        Dim dirPDF As String
        Dim Requestor As String
        Dim Entity As String
        Dim User As String
        Dim UserName As String
        Dim EmailFrom As String
        Dim WS As New WSFace.FactWSFront
        Dim XmlEnviado As String
        Dim xmlDoc As New Xml.XmlDocument
        Dim sNombreArchivo As String
        Dim RecSet As SAPbobsCOM.Recordset
        Dim Sql As String
        Dim tag As New WSFace.TransactionTag
        Dim Serie As String = ""
        Dim NumDoc As String = ""
        Dim Log As String = ""
        Dim fileName As String = ""
        Dim xmlDoc2 As New Xml.XmlDocument
        Dim firma As String
        Try
            If ValidaSerie(OCompany, SBO_Application, CodSerie, ProcesarBatch) Then
                ' If ProcesarBatch = False Then
                'SBO_Application.SetStatusBarMessage("Enviando documento para su autorización eléctronica", SAPbouiCOM.BoMessageTime.bmt_Short, False)
                dbUser = ObtieneValorParametro(OCompany, SBO_Application, "USRDB")
                dbPass = Utils.Desencriptar(ObtieneValorParametro(OCompany, SBO_Application, "PASSDB"))
                dirXML = ObtieneValorParametro(OCompany, SBO_Application, "PATHXML")
                dirPDF = ObtieneValorParametro(OCompany, SBO_Application, "PATHPDF")
                Requestor = ObtieneValorParametro(OCompany, SBO_Application, "IFACE")
                Entity = ObtieneValorParametro(OCompany, SBO_Application, "IENT")
                User = ObtieneValorParametro(OCompany, SBO_Application, "IUSR")
                UserName = ObtieneValorParametro(OCompany, SBO_Application, "IUSRN")
                EmailFrom = ObtieneValorParametro(OCompany, SBO_Application, "EMAILF")
                WS.Url = ObtieneValorParametro(OCompany, SBO_Application, "URLWS")
                WS.Timeout = 800000

                If ProcesarBatch = False Then
                    SBO_Application.SetStatusBarMessage("Enviando documento para su autorización eléctronica", SAPbouiCOM.BoMessageTime.bmt_Short, False)
                End If
                XmlEnviado = GeneraXML(SBO_Application, TipoFACE.Documenta, OCompany, DocEntry, Tipo, ProcesarBatch)
                LogEnvio += "Se genero XML de Envio" & vbNewLine
                Try
                    xmlDoc.LoadXml(XmlEnviado)
                Catch ex As Exception
                    Throw New Exception(ex.Message)
                End Try
                LogEnvio += "Cargando XML en memoria" & vbNewLine
                If System.IO.Directory.Exists(dirXML) = False Then
                    LogEnvio += "El path para almacenar el XML no existe" & vbNewLine

                    Throw New Exception("El path para almacenar el XML no existe")
                End If
                RecSet = OCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                If Tipo = "NC" Then
                    'Sql = "select nnm1.SeriesName,ORIN.DocNum from ORIN inner join NNM1 on ORIN.Series = isnull(nnm1.endstr,nnm1.Series)  where ORIN.DocEntry =" & DocEntry
                    Sql = ("CALL SP_FACE_UTILS('21','" & DocEntry & "','','','','')")
                ElseIf Tipo = "FACP" Then
                    'Sql = "select nnm1.SeriesName,OPCH.DocNum from OPCH inner join NNM1 on OPCH.Series = isnull(nnm1.endstr,nnm1.Series)  where OPCH.DocEntry =" & DocEntry
                    Sql = ("CALL SP_FACE_UTILS('22','" & DocEntry & "','','','','')")
                Else
                    'Sql = "select nnm1.SeriesName,oinv.DocNum from OINV inner join NNM1 on oinv.Series = isnull(nnm1.endstr,nnm1.Series)  where oinv.DocEntry =" & DocEntry
                    Sql = ("CALL SP_FACE_UTILS('23','" & DocEntry & "','','','','')")
                End If
                RecSet.DoQuery(Sql)
                Serie = RecSet.Fields.Item("SeriesName").Value.ToString
                NumDoc = RecSet.Fields.Item("DocNum").Value.ToString
                sNombreArchivo = Replace(String.Format("{0}\{3}{1}-{2}.xml", dirXML, Serie, NumDoc, Tipo), "\\", "\")
                xmlDoc.Save(sNombreArchivo)
                LogEnvio += "Creacion XML " & sNombreArchivo & vbNewLine
                Try
                    System.Net.ServicePointManager.Expect100Continue = False
                    LogEnvio += "Enviando XML " & vbNewLine
                    tag = WS.RequestTransaction(Requestor, "CONVERT_NATIVE_XML", Pais, Entity, User, UserName, xmlDoc.InnerXml, "XML", "")
                Catch ex As Exception
                    LogEnvio += "Ocurruio un error " & ex.Message & vbNewLine
                    Select Case Tipo
                        Case Is = "FAC"
                            'Sql = "update OINV set U_ESTADO_FACE ='R',U_FACE_XML='" & xmlDoc.InnerXml & "',U_MOTIVO_RECHAZO='" & ex.Message & "' WHERE docentry=" & DocEntry
                            Sql = ("CALL SP_FACE_UTILS('24','" & xmlDoc.InnerXml & "','" & ex.Message & "','" & DocEntry & "','','')")
                        Case Is = "FACP"
                            'Sql = "update OPCH set U_ESTADO_FACE ='R',U_FACE_XML='" & xmlDoc.InnerXml & "',U_MOTIVO_RECHAZO='" & ex.Message & "' WHERE docentry=" & DocEntry
                            Sql = ("CALL SP_FACE_UTILS('25','" & xmlDoc.InnerXml & "','" & ex.Message & "','" & DocEntry & "','','')")
                        Case Is = "ND"
                            'Sql = "update OINV set U_ESTADO_FACE ='R',U_FACE_XML='" & xmlDoc.InnerXml & "',U_MOTIVO_RECHAZO='" & ex.Message & "' WHERE docentry=" & DocEntry
                            Sql = ("CALL SP_FACE_UTILS('24','" & xmlDoc.InnerXml & "','" & ex.Message & "','" & DocEntry & "','','')")
                        Case Is = "NC"
                            'Sql = "update OINV set U_ESTADO_FACE ='R',U_FACE_XML='" & xmlDoc.InnerXml & "',U_MOTIVO_RECHAZO='" & ex.Message & "' WHERE  docentry=" & DocEntry
                            Sql = ("CALL SP_FACE_UTILS('24','" & xmlDoc.InnerXml & "','" & ex.Message & "','" & DocEntry & "','','')")
                    End Select
                    RecSet.DoQuery(Sql)
                    SBO_Application.SetStatusBarMessage("Falla al intentar registrar el documento , motivo de la fálla: " & ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, True)
                    Exit Sub
                End Try
                If tag.Response.Result Then
                    LogEnvio += "XML Autorizado " & vbNewLine
                    Log += IIf(Linea <> "", Linea & ") ", "") & "Documento: " & Serie & " " & NumDoc & " Estado: Aprobado" & vbNewLine
                    fileName = Replace(dirXML & "\Resp" & Tipo & Serie & NumDoc & ".xml", "\\", "\")
                    Dim f As New IO.FileInfo(fileName)
                    Dim w As IO.StreamWriter = f.CreateText()
                    w.Write(Base64String_String(tag.ResponseData.ResponseData1))
                    w.Close()

                    xmlDoc2.LoadXml(Base64String_String(tag.ResponseData.ResponseData1))

                    'doc2.SelectSingleNode("//FCAE").FirstChild.NextSibling.InnerText.ToString()

                    Dim fechaResol As Xml.XmlNodeList = xmlDoc2.GetElementsByTagName("FechaResolucion")
                    Dim nitGface As Xml.XmlNodeList = xmlDoc2.GetElementsByTagName("NITGFACE")
                    Dim nAutorizacion As Xml.XmlNodeList = xmlDoc2.GetElementsByTagName("NumeroAutorizacion")
                    Dim IniAut As Xml.XmlNodeList = xmlDoc2.GetElementsByTagName("rangoInicialAutorizado")
                    Dim FinAut As Xml.XmlNodeList = xmlDoc2.GetElementsByTagName("rangoFinalAutorizado")
                    Dim serieF As Xml.XmlNodeList = xmlDoc2.GetElementsByTagName("Serie")
                    Dim docF As Xml.XmlNodeList = xmlDoc2.GetElementsByTagName("uniqueCreatorIdentification")

                    If (IsNothing(IniAut(0).InnerText)) Then
                        IniAut = xmlDoc2.GetElementsByTagName("RangoInicialAutorizado")
                    End If
                    If (IsNothing(FinAut(0).InnerText)) Then
                        FinAut = xmlDoc2.GetElementsByTagName("RangoInicialAutorizado")
                    End If

                    firma = xmlDoc2.ChildNodes.Item(1).ChildNodes(0).ChildNodes(1).ChildNodes(1).ChildNodes(1).InnerText

                    'Dim fields As String
                    'fields = "U_ESTADO_FACE='A',"
                    'fields += "U_FACE_XML='" & Mid(Replace(xmlDoc2.InnerXml, "'", "''''"), 1, 254) & "',"
                    'fields += "U_FACE_PDFFILE=null,"
                    'fields += "U_FIRMA_ELETRONICA='" & firma & "',"
                    'fields += "U_NUMERO_DOCUMENTO='" & docF(0).InnerText & "',"
                    'fields += "U_NUMERO_RESOLUCION='" & nAutorizacion(0).InnerText & "',"
                    'fields += "U_SERIE_FACE='" & serieF(0).InnerText & "',"
                    'fields += "U_FACTURA_INI='" & IIf(IsNothing(IniAut(0).InnerText), "N", IniAut(0).InnerText) & "',"
                    'fields += "U_FACTURA_FIN='" & FinAut(0).InnerText & "' "

                    Select Case Tipo
                        Case Is = "FAC"
                            'Sql = "update OINV set " & fields & " WHERE  docentry=" & DocEntry
                            Sql = ("CALL SP_FACE_UTILS10('1','" & DocEntry & "','" & Mid(Replace(xmlDoc2.InnerXml, "'", "''''"), 1, 254) & ",','" & firma & ",','" & docF(0).InnerText & ",','" & nAutorizacion(0).InnerText & ",','" & serieF(0).InnerText & ",','" & IIf(IsNothing(IniAut(0).InnerText), "N", IniAut(0).InnerText) & ",','" & FinAut(0).InnerText & "')")
                        Case Is = "FACP"
                            'Sql = "update OPCH set " & fields & " WHERE  docentry=" & DocEntry
                            Sql = ("CALL SP_FACE_UTILS10('2','" & DocEntry & "','" & Mid(Replace(xmlDoc2.InnerXml, "'", "''''"), 1, 254) & ",','" & firma & ",','" & docF(0).InnerText & ",','" & nAutorizacion(0).InnerText & ",','" & serieF(0).InnerText & ",','" & IIf(IsNothing(IniAut(0).InnerText), "N", IniAut(0).InnerText) & ",','" & FinAut(0).InnerText & "')")
                        Case Is = "ND"
                            'Sql = "update OINV set " & fields & " WHERE  docentry=" & DocEntry
                            Sql = ("CALL SP_FACE_UTILS10('1','" & DocEntry & "','" & Mid(Replace(xmlDoc2.InnerXml, "'", "''''"), 1, 254) & ",','" & firma & ",','" & docF(0).InnerText & ",','" & nAutorizacion(0).InnerText & ",','" & serieF(0).InnerText & ",','" & IIf(IsNothing(IniAut(0).InnerText), "N", IniAut(0).InnerText) & ",','" & FinAut(0).InnerText & "')")
                        Case Is = "NC"
                            'Sql = "update ORIN set " & fields & " WHERE  docentry=" & DocEntry
                            Sql = ("CALL SP_FACE_UTILS10('3','" & DocEntry & "','" & Mid(Replace(xmlDoc2.InnerXml, "'", "''''"), 1, 254) & ",','" & firma & ",','" & docF(0).InnerText & ",','" & nAutorizacion(0).InnerText & ",','" & serieF(0).InnerText & ",','" & IIf(IsNothing(IniAut(0).InnerText), "N", IniAut(0).InnerText) & ",','" & FinAut(0).InnerText & "')")
                    End Select

                    Try
                        RecSet.DoQuery(Sql)
                    Catch ex As Exception
                        Log += "error: " & ex.Message & vbNewLine
                    End Try

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(RecSet)
                    RecSet = Nothing
                    GC.Collect()
                    If ProcesarBatch = False Then

                        SBO_Application.SetStatusBarMessage("Documento electrónico ha sido autorizado correctamente " & tag.Response.Description, SAPbouiCOM.BoMessageTime.bmt_Short, False)
                    End If
                Else
                    LogEnvio += "XML rechazado " & tag.Response.Description & " " & tag.Response.Hint & " " & Replace(tag.Response.Data, "'", "''''") & vbNewLine
                    Log += IIf(Linea <> "", Linea & ") ", "") & "Documento: " & Serie & " " & NumDoc & " Estado: Rechazado Motivo:" & tag.Response.Description & " " & tag.Response.Hint & " " & Replace(tag.Response.Data, "'", "''''") & vbNewLine
                    Select Case Tipo
                        Case Is = "FAC"
                            'Sql = "update OINV set U_ESTADO_FACE ='R',U_FACE_XML='" & Mid(Replace(xmlDoc2.InnerXml, "'", "''''"), 1, 254) & "',U_MOTIVO_RECHAZO='" & tag.Response.Description & " " & tag.Response.Hint & " " & Replace(tag.Response.Data, "'", "''''") & "' WHERE docentry=" & DocEntry
                            Sql = ("CALL SP_FACE_UTILS('27','" & Mid(Replace(xmlDoc2.InnerXml, "'", "''''"), 1, 254) & "','" & tag.Response.Description & " " & tag.Response.Hint & " " & Replace(tag.Response.Data, "'", "''''") & "','" & DocEntry & "','','')")
                        Case Is = "FACP"
                            'Sql = "update OPCH set U_ESTADO_FACE ='R',U_FACE_XML='" & Mid(Replace(xmlDoc2.InnerXml, "'", "''''"), 1, 254) & "',U_MOTIVO_RECHAZO='" & tag.Response.Description & " " & tag.Response.Hint & " " & Replace(tag.Response.Data, "'", "''''") & "' WHERE docentry=" & DocEntry
                            Sql = ("CALL SP_FACE_UTILS('28','" & Mid(Replace(xmlDoc2.InnerXml, "'", "''''"), 1, 254) & "','" & tag.Response.Description & " " & tag.Response.Hint & " " & Replace(tag.Response.Data, "'", "''''") & "','" & DocEntry & "','','')")
                        Case Is = "ND"
                            'Sql = "update OINV set U_ESTADO_FACE ='R',U_FACE_XML='" & Mid(Replace(xmlDoc2.InnerXml, "'", "''''"), 1, 254) & "',U_MOTIVO_RECHAZO='" & tag.Response.Description & " " & tag.Response.Hint & " " & Replace(tag.Response.Data, "'", "''''") & "' WHERE  docentry=" & DocEntry
                            Sql = ("CALL SP_FACE_UTILS('27','" & Mid(Replace(xmlDoc2.InnerXml, "'", "''''"), 1, 254) & "','" & tag.Response.Description & " " & tag.Response.Hint & " " & Replace(tag.Response.Data, "'", "''''") & "','" & DocEntry & "','','')")
                        Case Is = "NC"
                            'Sql = "update ORIN set U_ESTADO_FACE ='R',U_FACE_XML='" & Mid(Replace(xmlDoc2.InnerXml, "'", "''''"), 1, 254) & "',U_MOTIVO_RECHAZO='" & tag.Response.Description & " " & tag.Response.Hint & " " & Replace(tag.Response.Data, "'", "''''") & "' WHERE  docentry=" & DocEntry
                            Sql = ("CALL SP_FACE_UTILS('29','" & Mid(Replace(xmlDoc2.InnerXml, "'", "''''"), 1, 254) & "','" & tag.Response.Description & " " & tag.Response.Hint & " " & Replace(tag.Response.Data, "'", "''''") & "','" & DocEntry & "','','')")
                    End Select
                    Try
                        RecSet.DoQuery(Sql)
                    Catch ex As Exception
                        Log += "error: " & ex.Message & vbNewLine
                    End Try
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(RecSet)
                    RecSet = Nothing
                    GC.Collect()
                    If ProcesarBatch = False Then
                        SBO_Application.SetStatusBarMessage("Registro de documento electrónico fallído, motivo del rechazó: " & tag.Response.Description, SAPbouiCOM.BoMessageTime.bmt_Short, True)
                    End If
                End If
                'End If
            End If
        Catch ex As Exception
            LogEnvio += "Ocurrio un error" & ex.Message & vbNewLine
            Log += ex.Message & vbNewLine
            SBO_Application.SetStatusBarMessage(ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short)
        End Try
        If Log <> "" Then
            Dim mylog As String
            mylog = GetFileContents(Utils.FileLog) & Log
            SaveTextToFile(mylog, Utils.FileLog)
            LogBatch = Log
        End If
    End Sub

    Public Sub EnviaDocumentoInFileSP(ByVal OCompany As SAPbobsCOM.Company, ByVal SBO_Application As SAPbouiCOM.Application, ByVal Tipo As String, ByVal CurrSerie As String, ByVal CurrDoc As String, ByVal CurrSerieName As String, ByVal Pais As String, ByVal DocEntry As String, Optional ByVal ProcesarBatch As Boolean = False)
        Dim dte As New InfileWS.dte

        Dim registro As New InfileWS.requestDte
        Dim resultado As New InfileWS.responseDte
        Dim ws As New InfileWS.ingface

        Dim dbUser As String
        Dim dbPass As String
        Dim User As String = ""
        Dim UserName As String = ""
        Dim obOINV As DataTable
        Dim obOINV1 As DataTable
        Dim obOCRD As DataTable
        Dim obOCRD1 As DataTable
        Dim obOADM As DataTable
        Dim obRES As DataTable
        Dim obCountry As DataTable
        Dim obPAR As DataTable
        Dim log As String = ""
        Dim prefix As String = ""
        Dim deta As InfileWS.detalleDte
        Dim CurrentDoc As String = ""
        Dim CurrentSerie As String = ""
        Dim tipoFACE() As String
        Dim rs As SAPbobsCOM.Recordset

        Try
            If Utils.ValidaSerie(OCompany, SBO_Application, CInt(CurrSerie), ProcesarBatch) Then

                log = "**************************************************************************************************************"
                log += "Obteniendo parametros del sistema" & vbNewLine
                dbUser = ObtieneValorParametro(OCompany, SBO_Application, "USRDB")
                dbPass = Utils.Desencriptar(ObtieneValorParametro(OCompany, SBO_Application, "PASSDB"))
                ObtieneCredencialesSerie(OCompany, SBO_Application, CurrSerie, User, UserName)
                If User = "" Or User = "N/A" Then
                    User = ObtieneValorParametro(OCompany, SBO_Application, "IUSR")
                End If
                If UserName = "" Or UserName = "N/A" Then
                    UserName = ObtieneValorParametro(OCompany, SBO_Application, "IUSRN")
                End If
                log += "Credenciales WS Usuario: " & User & "Clave: " & UserName & vbNewLine

                ws.Url = ObtieneValorParametro(OCompany, SBO_Application, "URLWS")
                ws.Timeout = 800000

                log += "Conectando a base de datos" & vbNewLine
                'SESystem.Connection.DBConnection.Usuario = dbUser
                'SESystem.Connection.DBConnection.Password = dbPass

                'If Not SESystem.Connection.DBConnection.ConectDB(OCompany.Server, 1433, OCompany.CompanyDB) Then
                '    Throw New Exception("No se ha podido Conectar a la Base Datos")
                'End If

                log += "Obteniendo informacion de encabezado y detalle del documento " & DocEntry & " Tipo " & Tipo & vbNewLine
                Dim table2 As DataTable = EjecutaSqlTable("CALL SP_FACE_IT_DATOS_ENCABEZADO('" & DocEntry & "','" & Tipo & "')")
                Dim table As DataTable = EjecutaSqlTable("CALL SP_FACE_IT_DATOS_DETALLE( '" & DocEntry & "','" & Tipo & "')")
                If Not ProcesarBatch Then
                    SBO_Application.SetStatusBarMessage("Enviando documento para su autorización eléctronica", SAPbouiCOM.BoMessageTime.bmt_Short, False)
                End If

                tipoFACE = table2.Rows.Item(0).Item("TIPO_DOCUMENTO").ToString.Split("-")

                dte.idDispositivo = table2.Rows.Item(0).Item("DISPOSITIVO").ToString
                log += "idDispositivo: " & dte.idDispositivo & vbNewLine & vbNewLine
                dte.estadoDocumento = table2.Rows.Item(0).Item("ESTADO_DOCUMENTO").ToString
                log += "estadoDocumento: " & dte.estadoDocumento & vbNewLine
                dte.codigoMoneda = table2.Rows.Item(0).Item("CODIGO_MONEDA").ToString
                log += "codigoMoneda: " & dte.codigoMoneda & vbNewLine
                dte.tipoDocumento = tipoFACE(0)
                log += "tipoDocumento: " & dte.tipoDocumento & vbNewLine
                dte.serieDocumento = tipoFACE(1) 'table2.Rows.Item(0).Item("SERIE_AUTORIZADA").ToString
                log += "serieDocumento: " & dte.serieDocumento & vbNewLine
                dte.nitComprador = table2.Rows.Item(0).Item("NIT_COMPRADOR").ToString
                log += "nitComprador: " & dte.nitComprador & vbNewLine
                dte.nitVendedor = table2.Rows.Item(0).Item("NIT_VENDEDOR").ToString
                log += "nitVendedor: " & dte.nitVendedor & vbNewLine
                dte.serieAutorizada = table2.Rows.Item(0).Item("SERIE_AUTORIZADA").ToString
                log += "serieAutorizada: " & dte.serieAutorizada & vbNewLine
                dte.montoTotalOperacion = table2.Rows.Item(0).Item("TOTAL_DOCUMENTO")
                log += "montoTotalOperacion: " & dte.montoTotalOperacion & vbNewLine

                dte.fechaDocumento = table2.Rows.Item(0).Item("FECHA_DOCUMENTO")
                log += "fechaDocumento: " & dte.fechaDocumento & vbNewLine
                dte.fechaAnulacion = table2.Rows.Item(0).Item("FECHA_ANULACION")
                log += "fechaAnulacion: " & dte.fechaAnulacion & vbNewLine
                dte.observaciones = table2.Rows.Item(0).Item("OBSERVACIONES").ToString
                log += "observaciones: " & dte.observaciones & vbNewLine
                dte.telefonoComprador = table2.Rows.Item(0).Item("TELEFONO_COMPRADOR").ToString
                log += "telefonoComprador: " & dte.telefonoComprador & vbNewLine
                dte.importeDescuento = table2.Rows.Item(0).Item("IMPORTE_DESCUENTO")
                log += "importeDescuento: " & dte.importeDescuento & vbNewLine
                dte.importeTotalExento = table2.Rows.Item(0).Item("TOTAL_EXENTO")
                If dte.importeTotalExento > 0 Then
                    dte.regimen2989 = True
                    log += "regimen2989: " & dte.regimen2989 & vbNewLine
                End If

                log += "importeTotalExento: " & dte.importeTotalExento & vbNewLine
                dte.importeNetoGravado = table2.Rows.Item(0).Item("IMPORTE_NETO_GRAVADO")
                log += "importeNetoGravado: " & dte.importeNetoGravado & vbNewLine
                dte.detalleImpuestosIva = table2.Rows.Item(0).Item("DETALLE_IMPUESTO_IVA")
                log += "detalleImpuestosIva: " & dte.detalleImpuestosIva & vbNewLine
                dte.tipoCambio = table2.Rows.Item(0).Item("TIPO_CAMBIO")
                log += "tipoCambio: " & dte.tipoCambio & vbNewLine
                dte.direccionComercialComprador = table2.Rows.Item(0).Item("DIRECCION_COMPRADOR").ToString
                log += "direccionComercialComprador: " & dte.direccionComercialComprador & vbNewLine
                dte.serieAutorizada = table2.Rows.Item(0).Item("SERIE_AUTORIZADA").ToString
                log += "serieAutorizada: " & dte.serieAutorizada & vbNewLine
                CurrentSerie = dte.serieAutorizada
                dte.importeOtrosImpuestos = table2.Rows.Item(0).Item("IMPORTE_OTROS_IMPUESTOS")
                log += "importeOtrosImpuestos: " & dte.importeOtrosImpuestos & vbNewLine
                dte.numeroResolucion = table2.Rows.Item(0).Item("NUMERO_RESOLUCION").ToString
                log += "numeroResolucion: " & dte.numeroResolucion & vbNewLine
                dte.municipioComprador = table2.Rows.Item(0).Item("MUNICIPIO_COMPRADOR").ToString
                log += "municipioComprador: " & dte.municipioComprador & vbNewLine
                dte.departamentoComprador = table2.Rows.Item(0).Item("DEPARTAMENTO_COMPRADOR").ToString
                log += "departamentoComprador: " & dte.departamentoComprador & vbNewLine
                dte.nombreComercialComprador = table2.Rows.Item(0).Item("NOMBRE_COMPRADOR").ToString
                log += "nombreComercialComprador: " & dte.nombreComercialComprador & vbNewLine
                dte.nombreComercialRazonSocialVendedor = table2.Rows.Item(0).Item("NOMBRE_VENDEDOR").ToString
                log += "nombreComercialRazonSocialVendedor: " & dte.nombreComercialRazonSocialVendedor & vbNewLine
                dte.nombreCompletoVendedor = table2.Rows.Item(0).Item("NOMBRE_VENDEDOR").ToString
                log += "nombreCompletoVendedor: " & dte.nombreCompletoVendedor & vbNewLine
                dte.municipioVendedor = table2.Rows.Item(0).Item("MUNICIPIO_VENDEDOR").ToString
                log += "municipioVendedor: " & dte.municipioVendedor & vbNewLine
                dte.departamentoVendedor = table2.Rows.Item(0).Item("DEPARTAMENTO_VENDEDOR").ToString
                log += "departamentoVendedor: " & dte.departamentoVendedor & vbNewLine
                dte.direccionComercialVendedor = table2.Rows.Item(0).Item("DIRECCION_VENDEDOR").ToString
                log += "direccionComercialVendedor: " & dte.direccionComercialVendedor & vbNewLine
                dte.fechaResolucion = table2.Rows.Item(0).Item("FECHA_RESOLUCION").ToString
                log += "fechaResolucion: " & dte.fechaResolucion & vbNewLine
                dte.regimenISR = table2.Rows.Item(0).Item("REGIMEN_ISR")
                log += "regimenISR: " & dte.regimenISR & vbNewLine
                dte.importeBruto = table2.Rows.Item(0).Item("IMPORTE_BRUTO")
                log += "importeBruto: " & dte.importeBruto & vbNewLine
                dte.nitGFACE = table2.Rows.Item(0).Item("NIT_GFACE").ToString
                log += "nitGFACE: " & dte.nitGFACE & vbNewLine
                dte.codigoEstablecimiento = table2.Rows.Item(0).Item("CODIGO_SUCURSAL").ToString
                log += "codigoEstablecimiento: " & dte.codigoEstablecimiento & vbNewLine
                dte.correoComprador = table2.Rows.Item(0).Item("CORREO_COMPRADOR").ToString
                log += "correoComprador: " & dte.correoComprador & vbNewLine
                dte.descripcionOtroImpuesto = table2.Rows.Item(0).Item("DESCRIPCION_OTROS_IMPUESTOS").ToString
                log += "descripcionOtroImpuesto: " & dte.descripcionOtroImpuesto & vbNewLine
                dte.numeroDocumento = table2.Rows.Item(0).Item("NUMERO_DOCUMENTO").ToString
                log += "numeroDocumento: " & dte.numeroDocumento & vbNewLine
                CurrentDoc = dte.numeroDocumento
                dte.personalizado_01 = table2.Rows.Item(0).Item("PERSONALIZADO_1").ToString
                log += "personalizado_01: " & dte.personalizado_01 & vbNewLine
                dte.personalizado_02 = table2.Rows.Item(0).Item("PERSONALIZADO_2").ToString
                log += "personalizado_02: " & dte.personalizado_02 & vbNewLine
                dte.personalizado_03 = table2.Rows.Item(0).Item("PERSONALIZADO_3").ToString
                log += "personalizado_03: " & dte.personalizado_03 & vbNewLine
                dte.personalizado_04 = table2.Rows.Item(0).Item("PERSONALIZADO_4").ToString
                log += "personalizado_04: " & dte.personalizado_04 & vbNewLine
                dte.personalizado_05 = table2.Rows.Item(0).Item("PERSONALIZADO_5").ToString
                log += "personalizado_05: " & dte.personalizado_05 & vbNewLine
                dte.personalizado_06 = table2.Rows.Item(0).Item("PERSONALIZADO_6").ToString
                log += "personalizado_06: " & dte.personalizado_06 & vbNewLine
                dte.personalizado_07 = table2.Rows.Item(0).Item("PERSONALIZADO_7").ToString
                log += "personalizado_07: " & dte.personalizado_07 & vbNewLine
                dte.personalizado_08 = table2.Rows.Item(0).Item("PERSONALIZADO_8").ToString
                log += "personalizado_08: " & dte.personalizado_08 & vbNewLine
                dte.personalizado_09 = table2.Rows.Item(0).Item("PERSONALIZADO_9").ToString
                log += "personalizado_09: " & dte.personalizado_09 & vbNewLine
                dte.personalizado_10 = table2.Rows.Item(0).Item("PERSONALIZADO_10").ToString
                log += "personalizado_10: " & dte.personalizado_10 & vbNewLine
                dte.personalizado_11 = table2.Rows.Item(0).Item("PERSONALIZADO_11").ToString
                log += "personalizado_11: " & dte.personalizado_11 & vbNewLine
                dte.personalizado_12 = table2.Rows.Item(0).Item("PERSONALIZADO_12").ToString
                log += "personalizado_12: " & dte.personalizado_12 & vbNewLine
                dte.personalizado_13 = table2.Rows.Item(0).Item("PERSONALIZADO_13").ToString
                log += "personalizado_13: " & dte.personalizado_13 & vbNewLine
                dte.personalizado_14 = table2.Rows.Item(0).Item("PERSONALIZADO_14").ToString
                log += "personalizado_14: " & dte.personalizado_14 & vbNewLine
                dte.personalizado_15 = table2.Rows.Item(0).Item("PERSONALIZADO_15").ToString
                log += "personalizado_15: " & dte.personalizado_15 & vbNewLine
                dte.personalizado_16 = table2.Rows.Item(0).Item("PERSONALIZADO_16").ToString
                log += "personalizado_16: " & dte.personalizado_16 & vbNewLine
                dte.personalizado_17 = table2.Rows.Item(0).Item("PERSONALIZADO_17").ToString
                log += "personalizado_17: " & dte.personalizado_17 & vbNewLine
                dte.personalizado_18 = table2.Rows.Item(0).Item("PERSONALIZADO_18").ToString
                log += "personalizado_18: " & dte.personalizado_18 & vbNewLine
                dte.personalizado_19 = table2.Rows.Item(0).Item("PERSONALIZADO_19").ToString
                log += "personalizado_19: " & dte.personalizado_19 & vbNewLine
                dte.personalizado_20 = table2.Rows.Item(0).Item("PERSONALIZADO_20").ToString
                log += "personalizado_20: " & dte.personalizado_20 & vbNewLine


                dte.fechaAnulacionSpecified = True
                dte.fechaDocumentoSpecified = True
                dte.fechaResolucionSpecified = True
                dte.tipoCambioSpecified = True
                dte.detalleImpuestosIvaSpecified = True
                dte.importeOtrosImpuestosSpecified = True
                dte.importeDescuentoSpecified = True
                dte.importeBrutoSpecified = True
                dte.importeTotalExentoSpecified = True
                dte.importeNetoGravadoSpecified = True
                Dim index As Integer = 0
                Dim I As Integer = 0
                For Each obProd As DataRow In table.Rows
                    deta = New InfileWS.detalleDte
                    deta.cantidadSpecified = True
                    deta.cantidad = obProd("CANTIDAD")
                    log += I + 1 & ") cantidad: " & deta.cantidad & vbNewLine
                    deta.codigoProducto = obProd("CODIGO_PRODUCTO").ToString
                    log += I + 1 & ") codigoProducto: " & deta.codigoProducto & vbNewLine
                    deta.detalleImpuestosIvaSpecified = True
                    deta.descripcionProducto = obProd("DESCRIPCION_PRODUCTO").ToString
                    log += I + 1 & ") descripcionProducto: " & deta.descripcionProducto & vbNewLine
                    deta.montoBruto = obProd("MONTO_BRUTO")
                    log += I + 1 & ") montoBruto :" & deta.montoBruto & vbNewLine
                    deta.precioUnitario = obProd("PRECIO_UNITARIO")
                    log += I + 1 & ") precioUnitario: " & deta.precioUnitario & vbNewLine
                    deta.precioUnitarioSpecified = True

                    If obProd("TIPO_IMPUESTO") = "EXE" Then
                        deta.importeExento = obProd("IMPORTE_EXENTO")
                    Else
                        deta.importeExento = Convert.ToDouble(0.0)
                    End If
                    log += I + 1 & ") importeExento: " & deta.importeExento & vbNewLine
                    deta.importeExentoSpecified = True
                    deta.importeNetoGravado = obProd("IMPORTE_NETO_GRAVADO")
                    log += I + 1 & ") importeNetoGravado: " & deta.importeNetoGravado & vbNewLine
                    deta.importeNetoGravadoSpecified = True
                    'deta.importeTotalOperacion = obProd("linetotal")
                    deta.importeTotalOperacion = obProd("TOTAL_OPERACION")
                    log += I + 1 & ") importeTotalOperacion: " & deta.importeTotalOperacion & vbNewLine
                    deta.montoDescuento = obProd("TOTAL_DESCUENTO_LINEA")
                    log += I + 1 & ") montoDescuento: " & deta.montoDescuento & vbNewLine
                    deta.montoBrutoSpecified = True
                    deta.montoDescuentoSpecified = True
                    deta.importeTotalOperacionSpecified = True
                    deta.importeOtrosImpuestosSpecified = True
                    Try
                        deta.unidadMedida = obProd("UNIDAD_MEDIDA") 'TraeDato("select u_factura from [@UNIDADDEMEDIDA] where Code='" & obProd("U_UnidadMedida").ToString & "'")
                        log += I + 1 & ") unidadMedida: " & deta.unidadMedida & vbNewLine
                    Catch ex As Exception
                        Throw New Exception("La tabla de conversion de unidades de medida no tiene esta medida")
                    End Try
                    deta.detalleImpuestosIva = obProd("IMPORTE_OTROS_IMPUESTOS")
                    log += I + 1 & ") detalleImpuestosIva: " & deta.detalleImpuestosIva & vbNewLine
                    If table2.Rows(0)("TIPO_DOC").ToString = "I" Then
                        deta.tipoProducto = "B"
                    Else
                        deta.tipoProducto = "S"
                    End If
                    log += I + 1 & ") tipoProducto: " & deta.tipoProducto & vbNewLine
                    deta.importeOtrosImpuestos = Convert.ToDouble(0.0)
                    log += I + 1 & ") importeOtrosImpuestos: " & deta.importeOtrosImpuestos & vbNewLine
                    ReDim Preserve dte.detalleDte(I)
                    dte.detalleDte(I) = deta
                    I += 1
                Next
            End If

            registro.dte = dte
            registro.usuario = User
            registro.clave = UserName
            System.Net.ServicePointManager.Expect100Continue = False
            resultado = ws.registrarDte(registro)
            If (resultado.valido) Then
                Dim sql As String
                Dim RecSet As SAPbobsCOM.Recordset
                RecSet = OCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                log += "Documento: " & CurrSerieName & " " & CurrDoc & " Estado: Aprobado" & vbNewLine & "CAE: " & resultado.numeroDte & vbNewLine & "Firma: " & resultado.cae
                Select Case Tipo
                    Case Is = "FAC"
                        'sql = "update OINV set u_numero_documento='" & resultado.numeroDte & "',u_firma_eletronica='" & resultado.cae & "', U_ESTADO_FACE='A' WHERE   docentry=" & DocEntry
                        sql = ("CALL SP_FACE_UTILS('30','" & resultado.numeroDte & "','" & resultado.cae & "','" & DocEntry & "','','')")
                    Case Is = "ND"
                        'sql = "update OINV set u_numero_documento='" & resultado.numeroDte & "',u_firma_eletronica='" & resultado.cae & "', U_ESTADO_FACE='A' WHERE   docentry=" & DocEntry
                        sql = ("CALL SP_FACE_UTILS('30','" & resultado.numeroDte & "','" & resultado.cae & "','" & DocEntry & "','','')")
                    Case Is = "NC"
                        'sql = "update ORIN set u_numero_documento='" & resultado.numeroDte & "',u_firma_eletronica='" & resultado.cae & "', U_ESTADO_FACE='A' WHERE   docentry=" & DocEntry
                        sql = ("CALL SP_FACE_UTILS('31','" & resultado.numeroDte & "','" & resultado.cae & "','" & DocEntry & "','','')")
                End Select
                'log += sql & vbNewLine
                RecSet.DoQuery(sql)
                System.Runtime.InteropServices.Marshal.ReleaseComObject(RecSet)
                RecSet = Nothing
                GC.Collect()
                If ProcesarBatch = False Then
                    SBO_Application.SetStatusBarMessage("Documento electrónico ha sido autorizado correctamente ", SAPbouiCOM.BoMessageTime.bmt_Short, False)
                End If
            Else
                Dim sql As String
                Dim RecSet As SAPbobsCOM.Recordset
                RecSet = OCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                log += "Documento: " & CurrSerieName & " " & CurrDoc & " Estado: Rechazado Motivo:" & resultado.descripcion & vbNewLine
                Dim sqlaux As String = resultado.descripcion.Replace("'", " ")
                Select Case Tipo
                    Case Is = "FAC"
                        'sql = "update OINV set U_ESTADO_FACE ='R',U_MOTIVO_RECHAZO='" & sqlaux & "' WHERE   docentry=" & DocEntry
                        sql = ("CALL SP_FACE_UTILS('32','" & sqlaux & "','" & DocEntry & "','','','')")
                    Case Is = "ND"
                        'sql = "update OINV set U_ESTADO_FACE ='R',U_MOTIVO_RECHAZO='" & sqlaux & "' WHERE   docentry=" & DocEntry
                        sql = ("CALL SP_FACE_UTILS('32','" & sqlaux & "','" & DocEntry & "','','','')")
                    Case Is = "NC"
                        'sql = "update ORIN set U_ESTADO_FACE ='R',U_MOTIVO_RECHAZO='" & sqlaux & "' WHERE   docentry=" & DocEntry
                        sql = ("CALL SP_FACE_UTILS('33','" & sqlaux & "','" & DocEntry & "','','','')")
                End Select
                'log += sql & vbNewLine
                RecSet.DoQuery(sql)
                System.Runtime.InteropServices.Marshal.ReleaseComObject(RecSet)
                RecSet = Nothing
                GC.Collect()
                If ProcesarBatch = False Then
                    SBO_Application.SetStatusBarMessage("Registro de documento electrónico fallído, motivo del rechazó: " & resultado.descripcion, SAPbouiCOM.BoMessageTime.bmt_Short, True)
                End If
            End If
        Catch ex As Exception
            log += "Ocurrio un error: " & ex.Message
            If ProcesarBatch = False Then SBO_Application.SetStatusBarMessage(ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short)
        End Try
        If log <> "" Then
            Dim mylog As String
            mylog = GetFileContents(Utils.FileLog) & log
            SaveTextToFile(mylog, Utils.FileLog)
        End If
        If log <> "" Then
            Dim file As String = Replace(Utils.ObtieneValorParametro(OCompany, SBO_Application, "PATHXML") & "\Document_" & CurrentSerie & "-" & CurrentDoc & "_Log.txt", "\\", "\")
            SaveTextToFile(log, file)
        End If
    End Sub


    Private Function GeneraXML(ByVal SBO_Application As SAPbouiCOM.Application, ByVal ProveedorGFACE As TipoFACE, ByVal OCompany As SAPbobsCOM.Company, ByVal DocEntry As String, ByVal TipoDoc As String, Optional ByVal EsBatch As Boolean = False) As String
        Dim sql As String
        Dim result As String = ""
        Dim Enlinea As String = ""
        Try
            'SESystem.Connection.DBConnection.Usuario = ObtieneValorParametro(OCompany, SBO_Application, "USRDB")
            'SESystem.Connection.DBConnection.Password = Utils.Desencriptar(ObtieneValorParametro(OCompany, SBO_Application, "PASSDB"))

            'If SESystem.Connection.DBConnection.IsConnected = False Then
            '    If Not SESystem.Connection.DBConnection.ConectDB(OCompany.Server, 1433, OCompany.CompanyDB) Then
            '        Throw New Exception("No se ha podido Conectar a la Base Datos")
            '    End If
            'End If

            If EsBatch Then
                Enlinea = "N"
            Else
                Enlinea = "S"
            End If

            'sql = "EXEC SP_ITFACE_GENERAXML " & DocEntry & ",'" & TipoDoc & "','" & Enlinea & "'"   --FALTA SP
            result = TraeDato(sql)

            Return (result)

        Catch ex As Exception
            Throw New Exception(ex.Message)
        End Try
    End Function

    Public Sub EnviaDocumentoOtros(ByVal OCompany As SAPbobsCOM.Company, ByVal SBO_Application As SAPbouiCOM.Application, ByVal Tipo As String, ByVal CurrSerie As String, ByVal CurrDoc As String, ByVal CurrSerieName As String, ByVal Pais As String, ByVal DocEntry As String, Optional ByVal ProcesarBatch As Boolean = False)

        Dim Envio As Object
        'Select Case Utils.Empresa
        '    Case EmpresaFACE.FFACSA
        '        Envio = New clsFFACSA
        '    Case EmpresaFACE.PEGASUS
        '        Envio = New clsPEGASUS
        '    Case EmpresaFACE.PRINTER
        '        Envio = New clsPRINTER
        'End Select
        Dim Respuesta As String = ""
        Dim dbUser As String = ""
        Dim dbPass As String = ""
        Dim dirXML As String = ""
        Dim dirPDF As String = ""
        Dim NumFac As Long
        Dim Serie As String
        Dim CodSerie As Integer
        Dim oItem As SAPbouiCOM.Item
        Dim myNumFac As SAPbouiCOM.EditText
        Dim mySerie As SAPbouiCOM.ComboBox
        Dim WS As New WSFace.FactWSFront
        Dim doc As New Xml.XmlDataDocument()
        Dim doc2 As New Xml.XmlDataDocument()
        Dim RecSet As SAPbobsCOM.Recordset
        Dim QryStr As String
        Dim xmlResp As String = ""
        Dim Requestor As String
        Dim Entity As String
        Dim User As String
        Dim UserName As String
        Dim EmailFrom As String
        Dim xmlFile As String = ""
        Dim filename As String
        Dim firma As String = ""
        Dim filenamePDF As String
        Dim EsFACE As Boolean = False
        Dim Response As String
        Dim log As String = ""
        Dim tag As New WSFace.TransactionTag
        Dim rs As SAPbobsCOM.Recordset

        Try

            CodSerie = CurrSerie

            rs = OCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            If Tipo = "FAC" Or Tipo = "ND" Then
                'QryStr = "select docnum from oinv where docentry=" & DocEntry
                QryStr = ("CALL SP_FACE_UTILS('34','" & DocEntry & "','','','','')")
            Else
                'QryStr = "select docnum from orin where docentry=" & DocEntry
                QryStr = ("CALL SP_FACE_UTILS('35','" & DocEntry & "','','','','')")
            End If
            rs.DoQuery(QryStr)
            NumFac = CLng(rs.Fields.Item("docnum").Value.ToString)
            System.Runtime.InteropServices.Marshal.ReleaseComObject(rs)
            rs = Nothing
            GC.Collect()

            If ValidaSerie(OCompany, SBO_Application, CodSerie, ProcesarBatch) Then

                EsFACE = True
                dbUser = ObtieneValorParametro(OCompany, SBO_Application, "USRDB")
                dbPass = Utils.Desencriptar(ObtieneValorParametro(OCompany, SBO_Application, "PASSDB"))
                dirXML = ObtieneValorParametro(OCompany, SBO_Application, "PATHXML")
                dirPDF = ObtieneValorParametro(OCompany, SBO_Application, "PATHPDF")
                Requestor = ObtieneValorParametro(OCompany, SBO_Application, "IFACE")
                Entity = ObtieneValorParametro(OCompany, SBO_Application, "IENT")
                User = ObtieneValorParametro(OCompany, SBO_Application, "IUSR")
                UserName = ObtieneValorParametro(OCompany, SBO_Application, "IUSRN")
                EmailFrom = ObtieneValorParametro(OCompany, SBO_Application, "EMAILF")
                WS.Url = ObtieneValorParametro(OCompany, SBO_Application, "URLWS")
                WS.Timeout = 800000

                If ProcesarBatch = False Then
                    SBO_Application.SetStatusBarMessage("Enviando documento para su autorización eléctronica", SAPbouiCOM.BoMessageTime.bmt_Short, False)
                End If
                If Envio.GeneraXML(Tipo, OCompany.CompanyName, CodSerie, CurrSerieName, NumFac, OCompany.Server, OCompany.CompanyDB, dbUser, dbPass, EmailFrom, Respuesta, xmlResp, DocEntry, OCompany) = False Then
                    Throw New Exception(Respuesta)
                End If
                If Envio.GrabarXml(xmlResp, CurrSerieName, NumFac, Tipo, xmlFile) Then

                    doc.Load(xmlFile)

                    RecSet = OCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)

                    Try
                        System.Net.ServicePointManager.Expect100Continue = False
                        tag = WS.RequestTransaction(Requestor, "CONVERT_NATIVE_XML", Pais, Entity, User, UserName, doc.InnerXml, "XML", "")
                    Catch ex As Exception
                        Select Case Tipo
                            Case Is = "FAC"
                                'QryStr = "update OINV set U_ESTADO_FACE ='R',U_FACE_XML='" & doc2.InnerXml & "',U_MOTIVO_RECHAZO='" & ex.Message & "' WHERE docentry=" & DocEntry
                                QryStr = ("CALL SP_FACE_UTILS('36','" & doc2.InnerXml & "','" & ex.Message & "','" & DocEntry & "','','')")
                            Case Is = "ND"
                                'QryStr = "update OINV set U_ESTADO_FACE ='R',U_FACE_XML='" & doc2.InnerXml & "',U_MOTIVO_RECHAZO='" & ex.Message & "' WHERE docentry=" & DocEntry
                                QryStr = ("CALL SP_FACE_UTILS('36','" & doc2.InnerXml & "','" & ex.Message & "','" & DocEntry & "','','')")
                            Case Is = "NC"
                                'QryStr = "update OINV set U_ESTADO_FACE ='R',U_FACE_XML='" & doc2.InnerXml & "',U_MOTIVO_RECHAZO='" & ex.Message & "' WHERE  docentry=" & DocEntry
                                QryStr = ("CALL SP_FACE_UTILS('36','" & doc2.InnerXml & "','" & ex.Message & "','" & DocEntry & "','','')")
                        End Select
                        RecSet.DoQuery(QryStr)
                        SBO_Application.SetStatusBarMessage("Falla al intentar registrar el documento , motivo de la fálla: " & ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, True)
                        Exit Sub
                    End Try
                    If tag.Response.Result Then
                        log += "Documento: " & Serie & " " & NumFac & " Estado: Aprobado" & vbNewLine
                        'If System.IO.Directory.Exists(dirPDF) = False Then
                        '    Throw New Exception("El path para almacenar el PDF no existe")
                        'End If
                        'filenamePDF = Replace(dirPDF & "\" & Tipo & Serie & NumFac & ".pdf", "\\", "\")
                        'Dim oFileStream As System.IO.FileStream = New IO.FileStream(filenamePDF, System.IO.FileMode.Create)
                        'oFileStream.Write(Base64String_ByteArray(tag.ResponseData.ResponseData3), 0, Base64String_ByteArray(tag.ResponseData.ResponseData3).Length)
                        'oFileStream.Close()

                        filename = Replace(dirXML & "\Resp" & Tipo & Serie & NumFac & ".xml", "\\", "\")
                        Dim f As New IO.FileInfo(filename)
                        Dim w As IO.StreamWriter = f.CreateText()
                        w.Write(Base64String_String(tag.ResponseData.ResponseData1))
                        w.Close()

                        doc2.LoadXml(Base64String_String(tag.ResponseData.ResponseData1))
                        'doc2.SelectSingleNode("//FCAE").FirstChild.NextSibling.InnerText.ToString()

                        Dim fechaResol As Xml.XmlNodeList = doc2.GetElementsByTagName("fechaResolucion")
                        Dim nitGface As Xml.XmlNodeList = doc2.GetElementsByTagName("NITGFACE")
                        Dim nAutorizacion As Xml.XmlNodeList = doc2.GetElementsByTagName("NumeroAutorizacion")
                        Dim IniAut As Xml.XmlNodeList = doc2.GetElementsByTagName("rangoInicialAutorizado")
                        Dim FinAut As Xml.XmlNodeList = doc2.GetElementsByTagName("rangoFinalAutorizado")
                        Dim serieF As Xml.XmlNodeList = doc2.GetElementsByTagName("Serie")
                        Dim docF As Xml.XmlNodeList = doc2.GetElementsByTagName("NumeroDocumento")

                        firma = doc2.ChildNodes.Item(1).ChildNodes(0).ChildNodes(1).ChildNodes(1).ChildNodes(1).InnerText

                        Dim fields As String
                        fields = "U_ESTADO_FACE='A',"
                        fields += "U_FACE_XML='" & doc2.InnerXml & "',"
                        fields += "U_FACE_PDFFILE=null,"
                        fields += "U_FIRMA_ELETRONICA='" & firma & "',"
                        fields += "U_NUMERO_DOCUMENTO='" & docF(0).InnerText & "',"
                        fields += "U_NUMERO_RESOLUCION='" & nAutorizacion(0).InnerText & "',"
                        fields += "U_SERIE_FACE='" & serieF(0).InnerText & "',"
                        fields += "U_FACTURA_INI='" & IniAut(0).InnerText & "',"
                        fields += "U_FACTURA_FIN='" & FinAut(0).InnerText & "' "

                        Select Case Tipo
                            Case Is = "FAC"
                                'QryStr = "update OINV set " & fields & " WHERE  docentry=" & DocEntry
                                QryStr = ("CALL SP_FACE_UTILS10('1','" & DocEntry & ",','" & doc2.InnerXml & "','" & firma & ",','" & docF(0).InnerText & ",','" & nAutorizacion(0).InnerText & ",','" & serieF(0).InnerText & ",','" & IniAut(0).InnerText & ",','" & FinAut(0).InnerText & "')")
                            Case Is = "ND"
                                'QryStr = "update OINV set " & fields & " WHERE  docentry=" & DocEntry
                                QryStr = ("CALL SP_FACE_UTILS10('1','" & DocEntry & ",','" & doc2.InnerXml & "','" & firma & ",','" & docF(0).InnerText & ",','" & nAutorizacion(0).InnerText & ",','" & serieF(0).InnerText & ",','" & IniAut(0).InnerText & ",','" & FinAut(0).InnerText & "')")
                            Case Is = "NC"
                                'QryStr = "update ORIN set " & fields & " WHERE  docentry=" & DocEntry
                                QryStr = ("CALL SP_FACE_UTILS10('3','" & DocEntry & ",','" & doc2.InnerXml & "','" & firma & ",','" & docF(0).InnerText & ",','" & nAutorizacion(0).InnerText & ",','" & serieF(0).InnerText & ",','" & IniAut(0).InnerText & ",','" & FinAut(0).InnerText & "')")
                        End Select
                        RecSet.DoQuery(QryStr)
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(RecSet)
                        RecSet = Nothing
                        GC.Collect()
                        If ProcesarBatch = False Then

                            SBO_Application.SetStatusBarMessage("Documento electrónico ha sido autorizado correctamente " & tag.Response.Description, SAPbouiCOM.BoMessageTime.bmt_Short, False)
                        End If
                    Else
                        log += "Documento: " & Serie & " " & NumFac & " Estado: Rechazado Motivo:" & tag.Response.Description & vbNewLine
                        Select Case Tipo
                            Case Is = "FAC"
                                'QryStr = "update OINV set U_ESTADO_FACE ='R',U_FACE_XML='" & doc2.InnerXml & "',U_MOTIVO_RECHAZO='" & tag.Response.Description & "' WHERE docentry=" & DocEntry
                                QryStr = ("CALL SP_FACE_UTILS('37','" & doc2.InnerXml & "','" & tag.Response.Description & "','" & DocEntry & "','','')")
                            Case Is = "ND"
                                'QryStr = "update OINV set U_ESTADO_FACE ='R',U_FACE_XML='" & doc2.InnerXml & "',U_MOTIVO_RECHAZO='" & tag.Response.Description & "' WHERE  docentry=" & DocEntry
                                QryStr = ("CALL SP_FACE_UTILS('37','" & doc2.InnerXml & "','" & tag.Response.Description & "','" & DocEntry & "','','')")
                            Case Is = "NC"
                                'QryStr = "update ORIN set U_ESTADO_FACE ='R',U_FACE_XML='" & doc2.InnerXml & "',U_MOTIVO_RECHAZO='" & tag.Response.Description & "' WHERE  docentry=" & DocEntry
                                QryStr = ("CALL SP_FACE_UTILS('38','" & doc2.InnerXml & "','" & tag.Response.Description & "','" & DocEntry & "','','')")
                        End Select
                        RecSet.DoQuery(QryStr)
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(RecSet)
                        RecSet = Nothing
                        GC.Collect()
                        If ProcesarBatch = False Then

                            SBO_Application.SetStatusBarMessage("Registro de documento electrónico fallído, motivo del rechazó: " & tag.Response.Description, SAPbouiCOM.BoMessageTime.bmt_Short, True)
                        End If
                    End If
                End If

            End If
        Catch ex As Exception
            log += ex.Message & vbNewLine
            SBO_Application.SetStatusBarMessage(ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short)
        End Try
        If log <> "" Then
            Dim mylog As String
            mylog = GetFileContents(Utils.FileLog) & log
            SaveTextToFile(mylog, Utils.FileLog)
        End If

    End Sub

    Public Function ActivateFormIsOpen(ByVal SboApplication As SAPbouiCOM.Application, ByVal FormID As String) As Boolean
        Try
            Dim result As Boolean = False
            For x = 0 To SboApplication.Forms.Count - 1
                If SboApplication.Forms.Item(x).UniqueID = FormID Then
                    SboApplication.Forms.Item(x).Select()
                    result = True
                    Exit For
                End If
            Next
            Return result
        Catch ex As Exception
            Throw New Exception(ex.Message)
        End Try
    End Function

    Public Sub EnviaDocumentoInFile(ByVal OCompany As SAPbobsCOM.Company, ByVal SBO_Application As SAPbouiCOM.Application, ByVal Tipo As String, ByVal CurrSerie As String, ByVal CurrDoc As String, ByVal CurrSerieName As String, ByVal Pais As String, ByVal DocEntry As String, Optional ByVal ProcesarBatch As Boolean = False)
        Dim dte As New InfileWS.dte

        Dim registro As New InfileWS.requestDte
        Dim resultado As New InfileWS.responseDte
        Dim ws As New InfileWS.ingface

        Dim dbUser As String
        Dim dbPass As String
        Dim User As String = ""
        Dim UserName As String = ""
        Dim obOINV As DataTable
        Dim obOINV1 As DataTable
        Dim obOCRD As DataTable
        Dim obOCRD1 As DataTable
        Dim obOADM As DataTable
        Dim obRES As DataTable
        Dim obCountry As DataTable
        Dim obPAR As DataTable
        Dim log As String = ""
        Dim prefix As String = ""
        Dim deta As InfileWS.detalleDte
        Try
            If ValidaSerie(OCompany, SBO_Application, CurrSerie, ProcesarBatch) Then

                dbUser = ObtieneValorParametro(OCompany, SBO_Application, "USRDB")
                dbPass = Utils.Desencriptar(ObtieneValorParametro(OCompany, SBO_Application, "PASSDB"))
                ObtieneCredencialesSerie(OCompany, SBO_Application, CurrSerie, User, UserName)
                If User = "" Or User = "N/A" Then
                    User = ObtieneValorParametro(OCompany, SBO_Application, "IUSR")
                End If
                If UserName = "" Or User = "N/A" Then
                    UserName = ObtieneValorParametro(OCompany, SBO_Application, "IUSRN")
                End If
                ws.Url = ObtieneValorParametro(OCompany, SBO_Application, "URLWS")
                ws.Timeout = 800000

                'SESystem.Connection.DBConnection.Usuario = dbUser
                'SESystem.Connection.DBConnection.Password = dbPass

                'If Not SESystem.Connection.DBConnection.ConectDB(OCompany.Server, 1433, OCompany.CompanyDB) Then
                '    Throw New Exception("No se ha podido Conectar a la Base Datos")
                'End If

                If Tipo = "FAC" Or Tipo = "ND" Then
                    'obOINV = EjecutaSqlTable("SELECT * FROM OINV WHERE docentry=" & DocEntry)
                    obOINV = EjecutaSqlTable("CALL SP_FACE_UTILS('39','" & DocEntry & "','','','','')")
                Else
                    obOINV = EjecutaSqlTable("CALL SP_FACE_UTILS('40','" & DocEntry & "','','','','')")
                End If
                CurrDoc = obOINV.Rows(0).Item("docnum").ToString()
                obOCRD = EjecutaSqlTable("CALL SP_FACE_UTILS('41','" & obOINV.Rows(0)("CardCode") & "','','','','')")
                obOCRD1 = EjecutaSqlTable("CALL SP_FACE_UTILS('42','" & obOINV.Rows(0)("CardCode") & "','','','','')")
                obOADM = EjecutaSqlTable("CALL SP_FACE_UTILS('43','" & OCompany.CompanyName & "','','','','')")
                obRES = EjecutaSqlTable("CALL SP_FACE_UTILS('44','" & CurrSerie & "','','','','')")
                obCountry = EjecutaSqlTable("CALL SP_FACE_UTILS('45','','','','','')")
                'obOCRD = EjecutaSqlTable("SELECT * FROM OCRD WHERE CardCode = '" & obOINV.Rows(0)("CardCode") & "'")
                'obOCRD1 = EjecutaSqlTable("SELECT * FROM CRD1 WHERE CardCode = '" & obOINV.Rows(0)("CardCode") & "' AND AdresType = 'S'")
                'obOADM = EjecutaSqlTable("SELECT * FROM OADM WHERE CompnyName = '" & OCompany.CompanyName & "'")
                'obRES = EjecutaSqlTable("SELECT * FROM [@FACE_RESOLUCION] WHERE U_SERIE = '" & CurrSerie & "'")
                'obCountry = EjecutaSqlTable("select * from ADM1")


                If ProcesarBatch = False Then
                    SBO_Application.SetStatusBarMessage("Enviando documento para su autorización eléctronica", SAPbouiCOM.BoMessageTime.bmt_Short, False)
                End If


                dte.idDispositivo = obRES.Rows(0)("U_DISPOSITIVO").ToString
                log += ContruyeLog(SBO_Application, "idDispositivo", obRES.Rows(0)("U_DISPOSITIVO").ToString) & vbNewLine
                dte.estadoDocumento = "Activo".ToString
                log += ContruyeLog(SBO_Application, "estadoDocumento", "Activo".ToString) & vbNewLine
                dte.codigoMoneda = "GTQ".ToString
                log += ContruyeLog(SBO_Application, "codigoMoneda", "GTQ".ToString) & vbNewLine
                'Dim TipoDoc() As String = TraeDato("select code from [@FACE_TIPODOC] where U_codigo ='" & obRES.Rows(0)("U_TIPO_DOC").ToString & "'").Split("-")
                Dim TipoDoc() As String = TraeDato("CALL SP_FACE_UTILS('46','" & obRES.Rows(0)("U_TIPO_DOC").ToString & "','','','','')").Split("-")
                dte.tipoDocumento = TipoDoc(0)
                log += ContruyeLog(SBO_Application, "tipoDocumento", dte.tipoDocumento) & vbNewLine
                dte.nitComprador = obOINV.Rows(0)("U_Nit").ToString
                log += ContruyeLog(SBO_Application, "nitComprador", obOINV.Rows(0)("U_Nit").ToString) & vbNewLine
                dte.nitVendedor = ObtieneValorParametro(OCompany, SBO_Application, "NIT")
                log += ContruyeLog(SBO_Application, "nitVendedor", ObtieneValorParametro(OCompany, SBO_Application, "NIT")) & vbNewLine
                dte.serieAutorizada = CurrSerieName
                log += ContruyeLog(SBO_Application, "serieAutorizada", CurrSerieName) & vbNewLine
                dte.montoTotalOperacion = obOINV.Rows(0)("doctotal")
                log += ContruyeLog(SBO_Application, "montoTotalOperacion", obOINV.Rows(0)("doctotal")) & vbNewLine
                If TotalExcento(obOINV.Rows(0)("DocEntry"), Tipo) > 0 Then
                    dte.regimen2989 = True
                    log += ContruyeLog(SBO_Application, "regimen2989", "True") & vbNewLine
                End If
                dte.fechaDocumento = IIf(obOINV.Rows(0)("docdate").ToString = "", Date.Now, obOINV.Rows(0)("docdate"))
                log += ContruyeLog(SBO_Application, "fechaDocumento", IIf(obOINV.Rows(0)("docdate").ToString = "", Date.Now, obOINV.Rows(0)("docdate"))) & vbNewLine
                dte.fechaAnulacionSpecified = True
                log += ContruyeLog(SBO_Application, "fechaAnulacionSpecified", "True") & vbNewLine
                dte.fechaDocumentoSpecified = True
                log += ContruyeLog(SBO_Application, "fechaDocumentoSpecified", "True") & vbNewLine
                dte.fechaResolucionSpecified = True
                log += ContruyeLog(SBO_Application, "fechaResolucionSpecified", "True") & vbNewLine
                dte.tipoCambioSpecified = True
                log += ContruyeLog(SBO_Application, "tipoCambioSpecified", "True") & vbNewLine
                dte.detalleImpuestosIvaSpecified = True
                log += ContruyeLog(SBO_Application, "detalleImpuestosIvaSpecified", "True") & vbNewLine
                dte.importeOtrosImpuestosSpecified = True
                log += ContruyeLog(SBO_Application, "importeOtrosImpuestosSpecified", "True") & vbNewLine
                dte.fechaAnulacion = IIf(obOINV.Rows(0)("canceldate").ToString = "", Date.Now, obOINV.Rows(0)("docdate"))
                log += ContruyeLog(SBO_Application, "fechaAnulacion", IIf(obOINV.Rows(0)("canceldate").ToString = "", Date.Now, obOINV.Rows(0)("docdate"))) & vbNewLine
                prefix = ObtieneValorParametro(OCompany, SBO_Application, "PREFIX").ToString
                If prefix <> "" Then
                    If CInt(prefix) > 0 Then
                        dte.numeroDocumento = Mid(CurrDoc, CInt(prefix) + 1)
                    Else
                        dte.numeroDocumento = CurrDoc
                    End If
                Else
                    dte.numeroDocumento = CurrDoc
                End If
                log += ContruyeLog(SBO_Application, "numeroDocumento", dte.numeroDocumento) & vbNewLine

                dte.observaciones = IIf(obOINV.Rows(0)("comments").ToString = "", "N/D", obOINV.Rows(0)("comments").ToString)
                log += ContruyeLog(SBO_Application, "observaciones", IIf(obOINV.Rows(0)("comments").ToString = "", "N/D", obOINV.Rows(0)("comments").ToString)) & vbNewLine
                dte.telefonoComprador = IIf(obOCRD.Rows(0)("u_telefono").ToString = "", "N/D", obOCRD.Rows(0)("u_telefono").ToString)
                log += ContruyeLog(SBO_Application, "telefonoComprador", IIf(obOCRD.Rows(0)("u_telefono").ToString = "", "N/D", obOCRD.Rows(0)("u_telefono").ToString)) & vbNewLine
                dte.importeDescuento = Convert.ToDouble(0.0)
                log += ContruyeLog(SBO_Application, "importeDescuento", Convert.ToDouble(0.0).ToString) & vbNewLine
                dte.importeDescuentoSpecified = True
                log += ContruyeLog(SBO_Application, "importeDescuentoSpecified", "True") & vbNewLine
                dte.importeTotalExento = TotalExcento(obOINV.Rows(0)("DocEntry"), Tipo)
                log += ContruyeLog(SBO_Application, "importeTotalExento", TotalExcento(obOINV.Rows(0)("DocEntry"), Tipo)) & vbNewLine
                dte.importeTotalExentoSpecified = True
                log += ContruyeLog(SBO_Application, "importeTotalExentoSpecified", "True") & vbNewLine
                dte.importeNetoGravado = obOINV.Rows(0)("doctotal")
                log += ContruyeLog(SBO_Application, "importeNetoGravado", obOINV.Rows(0)("doctotal")) & vbNewLine
                dte.importeNetoGravadoSpecified = True
                log += ContruyeLog(SBO_Application, "importeNetoGravadoSpecified", "True") & vbNewLine
                dte.detalleImpuestosIva = obOINV.Rows(0)("Vatsum")
                log += ContruyeLog(SBO_Application, "detalleImpuestosIva", obOINV.Rows(0)("Vatsum")) & vbNewLine
                dte.tipoCambio = obOINV.Rows(0)("docrate")
                log += ContruyeLog(SBO_Application, "tipoCambio", obOINV.Rows(0)("docrate")) & vbNewLine
                dte.direccionComercialComprador = IIf(obOINV.Rows(0)("address").ToString = "", "Ciudad", obOINV.Rows(0)("address").ToString)
                log += ContruyeLog(SBO_Application, "direccionComercialComprador", IIf(obOINV.Rows(0)("address").ToString = "", "Ciudad", obOINV.Rows(0)("address").ToString)) & vbNewLine
                dte.serieDocumento = obRES.Rows(0)("U_TIPO_DOC").ToString
                log += ContruyeLog(SBO_Application, "serieDocumento", obRES.Rows(0)("U_TIPO_DOC").ToString) & vbNewLine
                dte.importeOtrosImpuestos = Convert.ToDouble(0.0)
                log += ContruyeLog(SBO_Application, "importeOtrosImpuestos", Convert.ToDouble(0.0).ToString) & vbNewLine
                dte.numeroResolucion = obRES.Rows(0)("U_RESOLUCION").ToString
                log += ContruyeLog(SBO_Application, "numeroResolucion", obRES.Rows(0)("U_RESOLUCION").ToString) & vbNewLine
                'Dim Municipio As String = TraeDato("select isnull(Name,'Guatemala')  from [@MUNICIPIO] where Code='" & obOCRD.Rows(0)("U_MUNICIPIO").ToString & "'")
                Dim Municipio As String = TraeDato("CALL SP_FACE_UTILS('47','" & obOCRD.Rows(0)("U_MUNICIPIO").ToString & "','','','','')")
                dte.municipioComprador = Municipio
                log += ContruyeLog(SBO_Application, "municipioComprador", Municipio) & vbNewLine
                dte.nombreComercialComprador = obOINV.Rows(0)("CardName").ToString
                log += ContruyeLog(SBO_Application, "nombreComercialComprador", obOINV.Rows(0)("CardName").ToString) & vbNewLine
                'Dim Departamento As String = TraeDato("select isnull(Name,'Guatemala')  from [@DEPARTAMENTO] where Code='" & obOCRD.Rows(0)("U_DEPARTAMENTO").ToString & "'")
                Dim Departamento As String = TraeDato("CALL SP_FACE_UTILS('48','" & obOCRD.Rows(0)("U_DEPARTAMENTO").ToString & "','','','','')")
                dte.departamentoComprador = Departamento
                log += ContruyeLog(SBO_Application, "departamentoComprador", Departamento) & vbNewLine
                dte.nombreComercialRazonSocialVendedor = ObtieneValorParametro(OCompany, SBO_Application, "NOMC")
                log += ContruyeLog(SBO_Application, "nombreComercialRazonSocialVendedor", ObtieneValorParametro(OCompany, SBO_Application, "NOMC")) & vbNewLine
                dte.nombreCompletoVendedor = dte.nombreComercialRazonSocialVendedor
                log += ContruyeLog(SBO_Application, "nombreCompletoVendedor", dte.nombreComercialRazonSocialVendedor) & vbNewLine
                dte.municipioVendedor = obCountry.Rows(0)("COUNTRY").ToString
                log += ContruyeLog(SBO_Application, "municipioVendedor", obCountry.Rows(0)("COUNTRY").ToString) & vbNewLine
                dte.departamentoVendedor = obOADM.Rows(0)("STATE").ToString
                log += ContruyeLog(SBO_Application, "departamentoVendedor", obOADM.Rows(0)("STATE").ToString) & vbNewLine
                dte.direccionComercialVendedor = ObtieneValorParametro(OCompany, SBO_Application, "DIRE")
                log += ContruyeLog(SBO_Application, "direccionComercialVendedor", ObtieneValorParametro(OCompany, SBO_Application, "DIRE")) & vbNewLine
                dte.fechaResolucion = obRES.Rows(0)("U_FECHA_AUTORIZACION").ToString
                log += ContruyeLog(SBO_Application, "fechaResolucion", obRES.Rows(0)("U_FECHA_AUTORIZACION").ToString) & vbNewLine
                dte.regimenISR = "ret definitiva"
                log += ContruyeLog(SBO_Application, "regimenISR", "ret definitiva") & vbNewLine
                dte.importeBruto = obOINV.Rows(0)("doctotal") - obOINV.Rows(0)("Vatsum")
                log += ContruyeLog(SBO_Application, "importeBruto", obOINV.Rows(0)("doctotal") - obOINV.Rows(0)("Vatsum")) & vbNewLine
                dte.importeBrutoSpecified = True
                log += ContruyeLog(SBO_Application, "importeBrutoSpecified", "True") & vbNewLine
                dte.nitGFACE = "12521337"
                log += ContruyeLog(SBO_Application, "nitGFACE", "12521337") & vbNewLine
                dte.codigoEstablecimiento = obRES.Rows(0)("U_SUCURSAL").ToString
                log += ContruyeLog(SBO_Application, "codigoEstablecimiento", obRES.Rows(0)("U_SUCURSAL").ToString) & vbNewLine
                dte.correoComprador = "N/A"
                log += ContruyeLog(SBO_Application, "correoComprador", dte.correoComprador) & vbNewLine
                dte.descripcionOtroImpuesto = "N/A"
                log += ContruyeLog(SBO_Application, "descripcionOtroImpuesto", dte.descripcionOtroImpuesto) & vbNewLine


                If Tipo = "NC" Then
                    'obOINV1 = EjecutaSqlTable("SELECT * FROM RIN1 WHERE DocEntry = " & obOINV.Rows(0)("DocEntry"))
                    obOINV1 = EjecutaSqlTable("CALL SP_FACE_UTILS('49','" & obOINV.Rows(0)("DocEntry") & "','','','','')")
                Else
                    'obOINV1 = EjecutaSqlTable("SELECT * FROM INV1 WHERE DocEntry = " & obOINV.Rows(0)("DocEntry"))
                    obOINV1 = EjecutaSqlTable("CALL SP_FACE_UTILS('50','" & obOINV.Rows(0)("DocEntry") & "','','','','')")
                End If
                If obOINV1.Rows.Count = 0 Then
                    Throw New Exception("El documento no tiene ningun detalle")
                End If
                Dim I As Integer = 0
                For Each obProd As DataRow In obOINV1.Rows
                    deta = New InfileWS.detalleDte
                    deta.cantidadSpecified = True
                    log += ContruyeLog(SBO_Application, "cantidadSpecified", "True", True, I) & vbNewLine
                    deta.cantidad = obProd("quantity")
                    log += ContruyeLog(SBO_Application, "cantidad", obProd("quantity"), True, I) & vbNewLine
                    deta.codigoProducto = obProd("itemcode").ToString
                    log += ContruyeLog(SBO_Application, "codigoProducto", obProd("itemcode").ToString, True, I) & vbNewLine
                    deta.detalleImpuestosIvaSpecified = True
                    log += ContruyeLog(SBO_Application, "detalleImpuestosIvaSpecified", "True", True, I) & vbNewLine
                    deta.descripcionProducto = obProd("dscription").ToString
                    log += ContruyeLog(SBO_Application, "descripcionProducto", obProd("dscription").ToString, True, I) & vbNewLine
                    deta.montoBruto = obProd("gtotal")
                    log += ContruyeLog(SBO_Application, "montoBruto", obProd("gtotal"), True, I) & vbNewLine
                    deta.precioUnitario = obProd("U_preciounidadmedida")
                    log += ContruyeLog(SBO_Application, "precioUnitario", deta.precioUnitario, True, I) & vbNewLine
                    deta.precioUnitarioSpecified = True
                    log += ContruyeLog(SBO_Application, "precioUnitarioSpecified", "True", True, I) & vbNewLine

                    If obProd("taxcode") = "EXE" Then
                        deta.importeExento = obProd("gtotal")
                    Else
                        deta.importeExento = Convert.ToDouble(0.0)
                    End If
                    log += ContruyeLog(SBO_Application, "importeExento", deta.importeExento, True, I) & vbNewLine
                    deta.importeExentoSpecified = True
                    log += ContruyeLog(SBO_Application, "importeExentoSpecified", "True", True, I) & vbNewLine
                    deta.importeNetoGravado = obProd("U_preciounidadmedida")
                    log += ContruyeLog(SBO_Application, "importeNetoGravado", deta.importeNetoGravado, True, I) & vbNewLine
                    deta.importeNetoGravadoSpecified = True
                    log += ContruyeLog(SBO_Application, "importeNetoGravadoSpecified", "True", True, I) & vbNewLine
                    'deta.importeTotalOperacion = obProd("linetotal")
                    deta.importeTotalOperacion = obProd("gtotal")
                    log += ContruyeLog(SBO_Application, "importeTotalOperacion", deta.importeTotalOperacion, True, I) & vbNewLine
                    deta.montoDescuento = Convert.ToDouble(0.0)
                    log += ContruyeLog(SBO_Application, "montoDescuento", Convert.ToDouble(0.0).ToString, True, I) & vbNewLine
                    deta.montoBrutoSpecified = True
                    log += ContruyeLog(SBO_Application, "montoBrutoSpecified", "True", True, I) & vbNewLine
                    deta.montoDescuentoSpecified = True
                    log += ContruyeLog(SBO_Application, "montoDescuentoSpecified", "True", True, I) & vbNewLine
                    deta.importeTotalOperacionSpecified = True
                    log += ContruyeLog(SBO_Application, "importeTotalOperacionSpecified", "True", True, I) & vbNewLine
                    deta.importeOtrosImpuestosSpecified = True
                    log += ContruyeLog(SBO_Application, "importeOtrosImpuestosSpecified", "True", True, I) & vbNewLine
                    Try
                        'deta.unidadMedida = TraeDato("select u_factura from [@UNIDADDEMEDIDA] where Code='" & obProd("U_UnidadMedida").ToString & "'")
                        deta.unidadMedida = TraeDato("CALL SP_FACE_UTILS('51','" & obProd("U_UnidadMedida").ToString & "','','','','')")
                        log += ContruyeLog(SBO_Application, "unidadMedida", deta.unidadMedida, True, I) & vbNewLine
                    Catch ex As Exception
                        Throw New Exception("La tabla de conversion de unidades de medida no tiene esta medida")
                    End Try
                    deta.detalleImpuestosIva = obProd("vatsum")
                    log += ContruyeLog(SBO_Application, "detalleImpuestosIva", obProd("vatsum"), True, I) & vbNewLine
                    If obOINV.Rows(0)("DocType").ToString = "I" Then
                        deta.tipoProducto = "B"
                    Else
                        deta.tipoProducto = "S"
                    End If
                    log += ContruyeLog(SBO_Application, "tipoProducto", deta.tipoProducto = "S", True) & vbNewLine
                    deta.importeOtrosImpuestos = Convert.ToDouble(0.0)
                    log += ContruyeLog(SBO_Application, "importeOtrosImpuestos", Convert.ToDouble(0.0), True) & vbNewLine
                    ReDim Preserve dte.detalleDte(I)
                    dte.detalleDte(I) = deta
                    I += 1
                Next
                registro.dte = dte
                registro.usuario = User
                registro.clave = UserName
                System.Net.ServicePointManager.Expect100Continue = False
                resultado = ws.registrarDte(registro)
                If (resultado.valido) Then
                    Dim sql As String
                    Dim RecSet As SAPbobsCOM.Recordset
                    RecSet = OCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                    log += "Documento: " & CurrSerieName & " " & CurrDoc & " Estado: Aprobado" & vbNewLine
                    Select Case Tipo
                        Case Is = "FAC"
                            sql = ("CALL SP_FACE_UTILS('52','" & resultado.numeroDte & "','" & resultado.cae & "','" & DocEntry & "','','')")
                            'sql = "update OINV set u_numero_documento='" & resultado.numeroDte & "',u_firma_eletronica='" & resultado.cae & "', U_ESTADO_FACE='A' WHERE   docentry=" & DocEntry
                        Case Is = "ND"
                            sql = ("CALL SP_FACE_UTILS('52','" & resultado.numeroDte & "','" & resultado.cae & "','" & DocEntry & "','','')")
                            'sql = "update OINV set u_numero_documento='" & resultado.numeroDte & "',u_firma_eletronica='" & resultado.cae & "', U_ESTADO_FACE='A' WHERE   docentry=" & DocEntry
                        Case Is = "NC"
                            sql = ("CALL SP_FACE_UTILS('53','" & resultado.numeroDte & "','" & resultado.cae & "','" & DocEntry & "','','')")
                            'sql = "update ORIN set u_numero_documento='" & resultado.numeroDte & "',u_firma_eletronica='" & resultado.cae & "', U_ESTADO_FACE='A' WHERE   docentry=" & DocEntry
                    End Select
                    'log += sql & vbNewLine
                    RecSet.DoQuery(sql)
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(RecSet)
                    RecSet = Nothing
                    GC.Collect()
                    If ProcesarBatch = False Then
                        SBO_Application.SetStatusBarMessage("Documento electrónico ha sido autorizado correctamente ", SAPbouiCOM.BoMessageTime.bmt_Short, False)
                    End If
                Else
                    Dim sql As String
                    Dim RecSet As SAPbobsCOM.Recordset
                    RecSet = OCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                    log += "Documento: " & CurrSerieName & " " & CurrDoc & " Estado: Rechazado Motivo:" & resultado.descripcion & vbNewLine
                    Select Case Tipo
                        Case Is = "FAC"
                            sql = ("CALL SP_FACE_UTILS('54','" & resultado.descripcion & "','" & DocEntry & "','','','')")
                           ' sql = "update OINV set U_ESTADO_FACE ='R',U_MOTIVO_RECHAZO='" & resultado.descripcion & "' WHERE   docentry=" & DocEntry
                        Case Is = "ND"
                            sql = ("CALL SP_FACE_UTILS('54','" & resultado.descripcion & "','" & DocEntry & "','','','')")
                           ' sql = "upda
                           ' sql = "update OINV set U_ESTADO_FACE ='R',U_MOTIVO_RECHAZO='" & resultado.descripcion & "' WHERE   docentry=" & DocEntry
                        Case Is = "NC"
                            sql = ("CALL SP_FACE_UTILS('55','" & resultado.descripcion & "','" & DocEntry & "','','','')")
                            ' sql = "upda
                            ' sql = "update ORIN set U_ESTADO_FACE ='R',U_MOTIVO_RECHAZO='" & resultado.descripcion & "' WHERE   docentry=" & DocEntry
                    End Select
                    'log += sql & vbNewLine
                    RecSet.DoQuery(sql)
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(RecSet)
                    RecSet = Nothing
                    GC.Collect()
                    If ProcesarBatch = False Then
                        SBO_Application.SetStatusBarMessage("Registro de documento electrónico fallído, motivo del rechazó: " & resultado.descripcion, SAPbouiCOM.BoMessageTime.bmt_Short, True)
                    End If
                End If

            End If
        Catch ex As Exception
            log += "HUBO ERROR EN LA OPERACION: " & ex.Message & vbNewLine
            If ProcesarBatch = False Then SBO_Application.SetStatusBarMessage(ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short)
        End Try
        If log <> "" Then
            Dim file As String = Replace(Utils.ObtieneValorParametro(OCompany, SBO_Application, "PATHXML") & "\" & Tipo & DocEntry & ".txt", "\\", "\")
            SaveTextToFile(log, file)
        End If
    End Sub

    Private Function ContruyeLog(ByVal SBO_Application As SAPbouiCOM.Application, ByVal Campo As String, ByVal valor As String, Optional ByVal EsDetalle As Boolean = False, Optional ByVal Linea As Integer = 0) As String
        Try
            If EsDetalle = False Then
                Return "(Encabezado) Campo: " & Campo & " Valor: " & valor
            Else
                Return "(Linea Detalle " & Linea & ") Campo: " & Campo & " Valor: " & valor
            End If
        Catch ex As Exception
            SBO_Application.MessageBox(ex.Message)
        End Try
    End Function

    Public Function ValidaSerie(ByVal OCompany As SAPbobsCOM.Company, ByVal SBO_Application As SAPbouiCOM.Application, ByVal codeSerie As Integer, Optional ByVal ProcesarBatch As Boolean = False) As Boolean
        Dim RecSet As SAPbobsCOM.Recordset
        Dim QryStr As String
        Dim result As Boolean = False
        Try
            RecSet = OCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            If ProcesarBatch = False Then
                QryStr = ("CALL SP_FACE_UTILS('56','" & codeSerie & "','','','','')")
                'QryStr = "select * from [@FACE_RESOLUCION] where U_SERIE = " & codeSerie & " AND ISNULL(U_ES_BATCH,'N') = 'N'"
            Else
                QryStr = ("CALL SP_FACE_UTILS('57','" & codeSerie & "','','','','')")
                'QryStr = "select * from [@FACE_RESOLUCION] where U_SERIE = " & codeSerie & " AND ISNULL(U_ES_BATCH,'N')='Y'"
            End If
            RecSet.DoQuery(QryStr)
            If RecSet.RecordCount > 0 Then
                result = True
            End If
            'System.Runtime.InteropServices.Marshal.ReleaseComObject(RecSet)
            'RecSet = Nothing
            'GC.Collect()
            Return result
        Catch ex As Exception
            SBO_Application.MessageBox(ex.Message)
        End Try

    End Function


    Public Function ExisteDocumento(ByVal OCompany As SAPbobsCOM.Company, ByVal SBO_Application As SAPbouiCOM.Application, ByVal DocEntry As String, ByVal TypeDoc As String) As Boolean
        Dim RecSet As SAPbobsCOM.Recordset
        Dim QryStr As String
        Dim result As Boolean = False
        Try
            If TypeDoc = "FAC" Or TypeDoc = "ND" Then
                RecSet = OCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                QryStr = ("CALL SP_FACE_UTILS('58','" & DocEntry & "','','','','')")
                'QryStr = "select * from OINV WHERE  docentry =" & DocEntry
                RecSet.DoQuery(QryStr)
                If RecSet.RecordCount > 0 Then
                    result = True
                End If
            End If
            If TypeDoc = "NC" Then
                RecSet = OCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                QryStr = ("CALL SP_FACE_UTILS('59','" & DocEntry & "','','','','')")
                'QryStr = "select * from ORIN WHERE  docentry =" & DocEntry
                RecSet.DoQuery(QryStr)
                If RecSet.RecordCount > 0 Then
                    result = True
                End If
            End If
            Return result
        Catch ex As Exception
            SBO_Application.MessageBox(ex.Message)
            Return False
        End Try
    End Function

    Public Function ObtieneValorParametro(ByVal OCompany As SAPbobsCOM.Company, ByVal SBO_Application As SAPbouiCOM.Application, ByVal Parametro As String) As String
        Dim RecSet As SAPbobsCOM.Recordset
        Dim QryStr As String

        Try
            RecSet = OCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            QryStr = ("CALL SP_FACE_UTILS('60','" & Parametro & "','','','','')")
            'QryStr = "select * from [@FACE_PARAMETROS] where U_PARAMETRO='" & Parametro & "'"
            RecSet.DoQuery(QryStr)
            RecSet.MoveFirst()
            Return RecSet.Fields.Item("U_VALOR").Value.ToString

            System.Runtime.InteropServices.Marshal.ReleaseComObject(RecSet)
            RecSet = Nothing
            GC.Collect()
        Catch ex As Exception
            SBO_Application.MessageBox(ex.Message)
        End Try
    End Function

    Public Sub ObtieneCredencialesSerie(ByVal OCompany As SAPbobsCOM.Company, ByVal SBO_Application As SAPbouiCOM.Application, ByVal Serie As Integer, ByRef UsuarioWS As String, ByRef ClaveWS As String)
        Dim RecSet As SAPbobsCOM.Recordset
        Dim QryStr As String

        Try
            QryStr = ("CALL SP_FACE_UTILS('61','" & Serie & "','','','','')")
            'QryStr = "select isnull(u_usuario,'N/A') usuario, isnull(u_clave,'N/A') clave from [@FACE_RESOLUCION] where U_SERIE = " & Serie
            RecSet = OCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            RecSet.DoQuery(QryStr)
            If RecSet.RecordCount > 0 Then
                UsuarioWS = RecSet.Fields.Item("usuario").Value.ToString
                ClaveWS = RecSet.Fields.Item("clave").Value.ToString
            End If
            System.Runtime.InteropServices.Marshal.ReleaseComObject(RecSet)
            RecSet = Nothing
            GC.Collect()
        Catch ex As Exception
            SBO_Application.MessageBox(ex.Message)
        End Try
    End Sub

    Private Function ByteArray_String(ByVal b As Byte()) As String
        Return New String(System.Text.Encoding.UTF8.GetChars(b))
    End Function

    Private Function Base64String_ByteArray(ByVal s As String) As Byte()
        Return Convert.FromBase64String(s)
    End Function

    Private Function Base64String_String(ByVal b64 As String) As String
        Try
            Return ByteArray_String(Base64String_ByteArray(b64))
        Catch
            Return b64
        End Try

    End Function

    Public Function GetFileContents(ByVal FullPath As String, _
       Optional ByRef ErrInfo As String = "") As String

        Dim strContents As String
        Dim objReader As IO.StreamReader
        Try

            objReader = New IO.StreamReader(FullPath)
            strContents = objReader.ReadToEnd()
            objReader.Close()
            Return strContents
        Catch Ex As Exception
            ErrInfo = Ex.Message
        End Try
    End Function

    Public Function SaveTextToFile(ByVal strData As String, _
     ByVal FullPath As String, _
       Optional ByRef ErrInfo As String = "") As Boolean

        Dim Contents As String
        Dim bAns As Boolean = False
        Dim objReader As IO.StreamWriter
        Dim enc As System.Text.Encoding
        Try


            objReader = New IO.StreamWriter(FullPath, False, System.Text.Encoding.Default)
            objReader.Write(strData)
            objReader.Close()
            bAns = True
        Catch Ex As Exception
            ErrInfo = Ex.Message

        End Try
        Return bAns
    End Function

    Public Function SaveToXML(ByVal strData As String, _
   ByVal FullPath As String, _
     Optional ByRef ErrInfo As String = "") As Boolean

        Dim Contents As String
        Dim bAns As Boolean = False
        Dim objReader As IO.StreamWriter
        Dim enc As System.Text.Encoding
        Try


            objReader = New IO.StreamWriter(FullPath)
            objReader.Write(strData)
            objReader.Close()
            bAns = True
        Catch Ex As Exception
            ErrInfo = Ex.Message
        End Try
        Return bAns
    End Function

    Private _FileLog As String
    Public Property FileLog() As String
        Get
            Return _FileLog
        End Get
        Set(ByVal value As String)
            _FileLog = value
        End Set
    End Property

    Public Function ValidaDato(ByVal dato As String)
        Try
            If InStr(dato, "&") > 0 Then
                dato = dato.Replace("&", "&amp;")
            ElseIf InStr(dato, ">") > 0 Then
                dato = dato.Replace(">", "&gt;")
            ElseIf InStr(dato, "<") > 0 Then
                dato = dato.Replace("<", "&lt;")
            ElseIf InStr(dato, """") > 0 Then
                dato = dato.Replace("""", "&quot;")
            ElseIf InStr(dato, """") > 0 Then
                dato = dato.Replace("""", "&quot;")
            ElseIf InStr(dato, "'") > 0 Then
                dato = dato.Replace("'", "&apos;")
            End If
            Return dato
        Catch ex As Exception
            Throw New Exception(ex.Message)
        End Try
    End Function

    Public Function ValidaCF(ByVal dato As String)
        Try
            If dato.ToLower = "cf" Or dato.ToLower = "c.f." Or dato.ToLower = "c/f" Then
                Select Case Utils.TipoGFACE
                    Case Is = TipoFACE.GuateFacturas
                        dato = "C/F"
                    Case Else
                        dato = "0000000000CF"
                End Select
            Else
                dato = dato.TrimStart("0")
            End If
            Return dato
        Catch ex As Exception
            Throw New Exception(ex.Message)
        End Try
    End Function

    Public Function GetDocEntry(ByVal CodSerie As String, ByVal CodeUser As String, ByVal DateDoc As String, ByVal DocTotal As String, ByVal CodeClient As String, ByVal TypeDoc As String) As String
        Dim sql As String = ""
        Try
            If TypeDoc = "NC" Then
                sql = ("CALL SP_FACE_UTILS('62','" & CodSerie & "','" & CodeUser & "','" & DateDoc & "','" & DocTotal & "','" & CodeClient & "')")
                'sql = "Select docentry " &
                '      "from ORIN " &
                '      "where Series='" & CodSerie & "' " &
                '      "and   UserSign ='" & CodeUser & "' " &
                '      "and   convert(varchar,UpdateDate,103) = '" & DateDoc & "' " &
                '      "and   DocTotal=  " & DocTotal &
                '      " and   CardCode ='" & CodeClient & "'"
            Else
                sql = ("CALL SP_FACE_UTILS('63','" & CodSerie & "','" & CodeUser & "','" & DateDoc & "','" & DocTotal & "','" & CodeClient & "')")
                'sql = "Select docentry " &
                '    "from OINV " &
                '    "where Series='" & CodSerie & "' " &
                '    "and   UserSign ='" & CodeUser & "' " &
                '    "and   convert(varchar,UpdateDate,103) = '" & DateDoc & "' " &
                '    "and   DocTotal=  " & DocTotal &
                '    " and   CardCode ='" & CodeClient & "'"
            End If
            Return TraeDato(sql)
        Catch ex As Exception
            Throw New Exception(ex.Message)
        End Try
    End Function
    Public Function VerificaNulo(ByVal valor As Object) As Boolean
        Try
            Return IsDBNull(valor)
        Catch ex As Exception
            Throw New Exception(ex.Message)
        End Try
    End Function


    Public Function TraeDataset(ByVal Sql As String) As DataSet
        Try
            Dim cm As SqlCommand
            Dim da As SqlDataAdapter
            Dim ds As DataSet
            Dim Cnn As New SqlConnection
            Dim UsuarioDB As String = ObtieneValorParametro(Company, SBOApplication, "USRDB")
            Dim PassDB As String = Desencriptar(ObtieneValorParametro(Company, SBOApplication, "PASSDB"))

            Cnn.ConnectionString = "Data Source=" & Company.Server & ";initial Catalog=" & Company.CompanyDB & ";Persist Security Info=True;User ID=" & UsuarioDB & ";Password=" & PassDB
            Cnn.Open()
            cm = New SqlCommand
            cm.CommandText = Sql
            cm.CommandType = CommandType.Text
            cm.Connection = Cnn
            da = New SqlDataAdapter(cm)
            ds = New DataSet()
            da.Fill(ds)
            Cnn.Close()

            Return ds

        Catch ex As Exception
            Throw New Exception(ex.Message)
        End Try
    End Function

    Public Function TraeDato(ByVal Sql As String) As Object
        Try
            Dim cm As SqlCommand
            Dim da As SqlDataAdapter
            Dim ds As DataSet
            Dim Cnn As New SqlConnection
            Dim UsuarioDB As String = ObtieneValorParametro(Company, SBOApplication, "USRDB")
            Dim PassDB As String = Desencriptar(ObtieneValorParametro(Company, SBOApplication, "PASSDB"))

            Cnn.ConnectionString = "Data Source=" & Company.Server & ";initial Catalog=" & Company.CompanyDB & ";Persist Security Info=True;User ID=" & UsuarioDB & ";Password=" & PassDB
            Cnn.Open()
            cm = New SqlCommand
            cm.CommandText = Sql
            cm.CommandType = CommandType.Text
            cm.Connection = Cnn
            da = New SqlDataAdapter(cm)
            ds = New DataSet()
            da.Fill(ds)
            Cnn.Close()

            If ds.Tables(0).Rows.Count > 0 Then
                Return ds.Tables(0).Rows(0)(0)
            Else
                Return Nothing
            End If

        Catch ex As Exception
            Throw New Exception(ex.Message)
        End Try
    End Function

    Public Function EjecutaSqlTable(ByVal Sql As String) As DataTable
        Try
            Dim cm As SqlCommand
            Dim da As SqlDataAdapter
            Dim ds As DataSet
            Dim Cnn As New SqlConnection
            Dim UsuarioDB As String = ObtieneValorParametro(Company, SBOApplication, "USRDB")
            Dim PassDB As String = Desencriptar(ObtieneValorParametro(Company, SBOApplication, "PASSDB"))

            Cnn.ConnectionString = "Data Source=" & Company.Server & ";initial Catalog=" & Company.CompanyDB & ";Persist Security Info=True;User ID=" & UsuarioDB & ";Password=" & PassDB
            Cnn.Open()
            cm = New SqlCommand
            cm.CommandText = Sql
            cm.CommandType = CommandType.Text
            cm.Connection = Cnn
            da = New SqlDataAdapter(cm)
            ds = New DataSet()
            da.Fill(ds)
            Cnn.Close()

            If ds.Tables(0).Rows.Count > 0 Then
                Return ds.Tables(0)
            Else
                Return Nothing
            End If

        Catch ex As Exception
            Throw New Exception(ex.Message)
        End Try
    End Function

    'Public Function EjecutaSqlTable(ByVal Sql As String) As DataTable
    '    Try
    '        Dim RecSet As SAPbobsCOM.Recordset
    '        Dim result As New DataTable
    '        Dim DS As New DataSet
    '        Dim DR As DataRow

    '        RecSet = Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
    '        RecSet.DoQuery(Sql)
    '        If RecSet.RecordCount > 0 Then
    '            RecSet.MoveFirst()
    '            'DS.Tables.Add("temp")
    '            For row = 0 To RecSet.RecordCount - 1
    '                DR = DS.Tables("temp").NewRow
    '                For col = 0 To RecSet.Fields.Count - 1
    '                    DR.Item(RecSet.Fields.Item(col).Name) = RecSet.Fields.Item(0).Value
    '                Next
    '                DS.Tables("temp").Rows.Add(DR)
    '                RecSet.MoveNext()
    '            Next
    '            result = DS.Tables(0)
    '        End If
    '        System.Runtime.InteropServices.Marshal.ReleaseComObject(RecSet)
    '        RecSet = Nothing
    '        GC.Collect()

    '        Return result
    '    Catch ex As Exception
    '        Throw New Exception(ex.Message)
    '    End Try
    'End Function
End Module
