Imports System.Threading

Public Class BatchForm

    Private XmlForm As String = Replace(Application.StartupPath & "\Batch.srf", "\\", "\")
    Private WithEvents SBO_Application As SAPbouiCOM.Application
    Private oForm As SAPbouiCOM.Form
    Private oCompany As SAPbobsCOM.Company
    Private oFilters As SAPbouiCOM.EventFilters
    Private oFilter As SAPbouiCOM.EventFilter
    Private mThreadFic As Thread
    Private ProgressBar As SAPbouiCOM.ProgressBar

    'Private Sub SetApplication()


    '    '*******************************************************************
    '    '// Use an SboGuiApi object to establish connection
    '    '// with the SAP Business One application and return an
    '    '// initialized appliction object
    '    '*******************************************************************

    '    Dim SboGuiApi As SAPbouiCOM.SboGuiApi
    '    Dim sConnectionString As String

    '    Try
    '        SboGuiApi = New SAPbouiCOM.SboGuiApi

    '        '// by following the steps specified above, the following
    '        '// statment should be suficient for either development or run mode

    '        sConnectionString = Utils.ConnectionString  'Environment.GetCommandLineArgs.GetValue(1)

    '        '// connect to a running SBO Application

    '        SboGuiApi.Connect(sConnectionString)

    '        '// get an initialized application object

    '        SBO_Application = SboGuiApi.GetApplication(-1)

    '    Catch ex As Exception
    '        MsgBox(ex.Message, MsgBoxStyle.Critical, "Ocurrio un error")
    '    End Try

    'End Sub

    Public Sub New()
        MyBase.New()

        Try
            ''//*************************************************************
            ''// set SBO_Application with an initialized application object
            ''//*************************************************************
            'SetApplication()

            'If Not SetConnectionContext() = 0 Then
            '    SBO_Application.MessageBox("Failed setting a connection to DI API")
            '    End ' Terminating the Add-On Application
            'End If

            'If Not ConnectToCompany() = 0 Then
            '    SBO_Application.MessageBox("Failed connecting to the company's Data Base")
            '    End ' Terminating the Add-On Application
            'End If

            Me.SBO_Application = Utils.SBOApplication
            Me.oCompany = Utils.Company

            If Utils.ActivateFormIsOpen(SBO_Application, "SBOBatch") = False Then
                LoadFromXML(XmlForm)

                '// Get the added form object by using the form's UID
                oForm = SBO_Application.Forms.Item("SBOBatch")


                '// Show the loaded Form
                oForm.Visible = True
                oForm.PaneLevel = 1

                ' SetFilters()

                oForm.DataSources.DataTables.Add("MyDataTable")
                oForm.DataSources.UserDataSources.Add("CheckDS1", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 15)

                Dim chkRecha As SAPbouiCOM.CheckBox
                chkRecha = oForm.Items.Item("chkRecha").Specific
                chkRecha.DataBind.SetBound(True, "", "CheckDS1")

                Dim del As SAPbouiCOM.EditText
                Dim Al As SAPbouiCOM.EditText
                Dim cmdResult As SAPbouiCOM.Item
                Dim cmdenviar As SAPbouiCOM.Button

                oForm.DataSources.UserDataSources.Add("UDDate", SAPbouiCOM.BoDataType.dt_DATE)
                oForm.DataSources.UserDataSources.Add("UDDate2", SAPbouiCOM.BoDataType.dt_DATE)
                del = oForm.Items.Item("txtDel").Specific
                Al = oForm.Items.Item("txtAl").Specific
                cmdResult = oForm.Items.Item("cmdResult")
                cmdenviar = oForm.Items.Item("cmdEnviar").Specific
                del.DataBind.SetBound(True, "", "UDDate")
                Al.DataBind.SetBound(True, "", "UDDate2")

                If TipoGFACE = TipoFACE.GuateFacturas Then
                    cmdResult.Enabled = False
                    cmdenviar.Caption = "Generar archivo TXT"
                Else
                    cmdenviar.Caption = "Enviar"
                    cmdResult.Enabled = True
                End If

                LlenaSeries()
            Else
                oForm = SBO_Application.Forms.Item("SBOBatch")
            End If
        Catch ex As Exception
            SBO_Application.MessageBox(ex.Message)
        End Try
    End Sub

    Private Sub SetFilters()

        '// Create a new EventFilters object
        oFilters = New SAPbouiCOM.EventFilters()

        '// add an event type to the container
        '// this method returns an EventFilter object
        oFilter = oFilters.Add(SAPbouiCOM.BoEventTypes.et_CLICK)

        '// assign the form type on which the event would be processed
        oFilter.AddEx("60006") 'Orders Form


        'oFilter = oFilters.Add(SAPbouiCOM.BoEventTypes.et_KEY_DOWN)

        '// assign the form type on which the event would be processed
        oFilter.Add(60006) 'Orders Form

        '// For a list of all form types see the help or use the
        '// Tools -> User Tools -> Display Debug Information option
        '// in the SBO application
        '// then open the desired form and hover over it with the mouse
        '// the form's type will apear in the lower left side of the screen


        '// Setting the application with the EventFilters object
        '// in this case we will process a click event for form types 142 and 139
        '// and we will process a key down event for for form type 139

        SBO_Application.SetFilter(oFilters)

    End Sub

    Private Function SetConnectionContext() As Integer

        Dim sCookie As String
        Dim sConnectionContext As String

        '// First initialize the Company object

        oCompany = New SAPbobsCOM.Company

        '// Acquire the connection context cookie from the DI API.
        sCookie = oCompany.GetContextCookie

        '// Retrieve the connection context string from the UI API using the
        '// acquired cookie.
        sConnectionContext = SBO_Application.Company.GetConnectionContext(sCookie)

        '// before setting the SBO Login Context make sure the company is not
        '// connected

        If oCompany.Connected = True Then
            oCompany.Disconnect()
        End If

        '// Set the connection context information to the DI API.
        SetConnectionContext = oCompany.SetSboLoginContext(sConnectionContext)

    End Function

    Private Function ConnectToCompany() As Integer

        '// Make sure you're not already connected.
        If oCompany.Connected = True Then
            oCompany.Disconnect()
        End If

        '// Establish the connection to the company database.
        ConnectToCompany = oCompany.Connect

    End Function


    Private Sub LoadFromXML(ByRef FileName As String)

        Dim oXmlDoc As Xml.XmlDocument

        oXmlDoc = New Xml.XmlDocument

        ' ''// load the content of the XML File
        ''Dim sPath As String

        ''sPath = IO.Directory.GetParent(Application.StartupPath).ToString

        'oXmlDoc.Load(sPath & "\" & FileName)
        oXmlDoc.Load(FileName)

        '// load the form to the SBO application in one batch
        SBO_Application.LoadBatchActions(oXmlDoc.InnerXml)

    End Sub


    Private Sub SBO_Application_ItemEvent(ByVal FormUID As String, ByRef pVal As SAPbouiCOM.ItemEvent, ByRef BubbleEvent As Boolean) Handles SBO_Application.ItemEvent
        Try
            If oForm Is Nothing Then
                Exit Sub
            End If

            If pVal.EventType = SAPbouiCOM.BoEventTypes.et_FORM_CLOSE And pVal.BeforeAction = True And pVal.FormType = "60006" Then
                oForm = Nothing
                oCompany = Nothing
                SBO_Application = Nothing
            End If

            If pVal.FormType = "60006" Then

                If pVal.ItemUID = "cmdConsul" And pVal.EventType = SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED And pVal.Before_Action = True Then
                    Consulta()
                    BubbleEvent = False
                End If

                If pVal.ItemUID = "chkRecha" And pVal.EventType = SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED And pVal.Before_Action = False Then
                    Consulta()
                    BubbleEvent = False
                End If

                If pVal.ItemUID = "cmdEnviar" And pVal.EventType = SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED And pVal.Before_Action = False Then
                    mThreadFic = New Thread(New ThreadStart(AddressOf Enviar))
                    mThreadFic.Start()
                    BubbleEvent = False
                End If

                If pVal.ItemUID = "cmdResult" And pVal.EventType = SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED And pVal.Before_Action = True Then
                    If System.IO.File.Exists(Utils.FileLog) Then
                        System.Diagnostics.Process.Start("notepad.exe", Utils.FileLog)
                    Else
                        Throw New Exception("El archivo de resultados no existe")
                    End If
                    BubbleEvent = False
                End If

                If pVal.EventType = SAPbouiCOM.BoEventTypes.et_FORM_CLOSE Then
                    oForm = Nothing
                End If
            End If
        Catch ex As Exception
            SBO_Application.SetStatusBarMessage(ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, True)
        End Try
    End Sub

    Private Sub Enviar()
        Dim oGrid As SAPbouiCOM.Grid
        Dim Tipo As String = ""
        Dim Serie As String = ""
        Dim Doc As String = ""
        Dim SerieName As String = ""
        Dim sql As String = ""
        Dim cmdEnvio As SAPbouiCOM.Item
        Dim RecSet As SAPbobsCOM.Recordset
        Dim docEntry As String = ""
        Dim myLog As String = ""
        Dim TotalDocs As String
        Dim cont As Integer = 0
        Dim mySerie As SAPbouiCOM.ComboBox
        Dim lblProc As SAPbouiCOM.StaticText
        Dim content As String
        Dim dirXML As String = ObtieneValorParametro(oCompany, SBO_Application, "PATHXML")

        Try
            mySerie = oForm.Items.Item("cmbSerie").Specific
            oGrid = oForm.Items.Item("grdDatos").Specific
            lblProc = oForm.Items.Item("lblProc").Specific
            If oGrid.Rows.Count > 0 Then

                cmdEnvio = oForm.Items.Item("cmdEnviar")
                cmdEnvio.Enabled = False
                TotalDocs = oGrid.Rows.Count
                Utils.FileLog = Replace(Application.StartupPath & "\Resultados " & Format(Date.Now, "ddMMyyyyHHmmss") & ".txt", "\\", "\")
                Try

                    'ProgressBar = Me.SBO_Application.StatusBar.CreateProgressBar("Procesando documentos electrónicos por favor espere...", oGrid.Rows.Count - 1, False)
                    'SBO_Application.SetStatusBarMessage("Enviando documentos favor espere...", SAPbouiCOM.BoMessageTime.bmt_Short, False)
                    For i = 0 To oGrid.Rows.Count - 1
                        'If mySerie.Value = "0" Then
                        '    'sql = "select U_TIPO_DOC Tipo,b.Series,b.DocNum,c.SeriesName,b.docentry  " & _
                        '    '       "from [@FACE_RESOLUCION] a " & _
                        '    '       "inner join OINV b " & _
                        '    '       "on a.U_SERIE = b.Series " & _
                        '    '       "inner join NNM1 c " & _
                        '    '       "on b.Series=c.Series " & _
                        '    '       "where b.DocEntry = " & oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(1).Name, i) & _
                        '    '       " union " & _
                        '    '       "select U_TIPO_DOC Tipo,b.Series,b.DocNum,c.SeriesName,b.docentry   " & _
                        '    '       "from [@FACE_RESOLUCION] a " & _
                        '    '       "inner join ORIN b " & _
                        '    '       "on a.U_SERIE = b.Series " & _
                        '    '       "inner join NNM1 c " & _
                        '    '       "on b.Series=c.Series " & _
                        '    '       "where b.DocEntry = " & oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(1).Name, i) & _
                        '    '       " union " & _
                        '    '       "select U_TIPO_DOC Tipo,b.Series,b.DocNum,c.SeriesName,b.docentry   " & _
                        '    '       "from [@FACE_RESOLUCION] a " & _
                        '    '       "inner join OPCH b " & _
                        '    '       "on a.U_SERIE = b.Series " & _
                        '    '       "inner join NNM1 c " & _
                        '    '       "on b.Series=c.Series " & _
                        '    '       "where b.DocEntry = " & oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(1).Name, i)

                        'Else
                        '    sql = "select U_TIPO_DOC Tipo,b.Series,b.DocNum,c.SeriesName,b.docentry  " & _
                        '           "from [@FACE_RESOLUCION] a " & _
                        '           "inner join OINV b " & _
                        '           "on a.U_SERIE = b.Series " & _
                        '           "inner join NNM1 c " & _
                        '           "on b.Series=c.Series " & _
                        '           "where a.U_SERIE = " & mySerie.Value & " And b.DocEntry = " & oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(1).Name, i) & _
                        '           " union " & _
                        '           "select U_TIPO_DOC Tipo,b.Series,b.DocNum,c.SeriesName,b.docentry   " & _
                        '           "from [@FACE_RESOLUCION] a " & _
                        '           "inner join ORIN b " & _
                        '           "on a.U_SERIE = b.Series " & _
                        '           "inner join NNM1 c " & _
                        '           "on b.Series=c.Series " & _
                        '           "where a.U_SERIE = " & mySerie.Value & " And b.DocEntry = " & oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(1).Name, i) & _
                        '            " union " & _
                        '           "select U_TIPO_DOC Tipo,b.Series,b.DocNum,c.SeriesName,b.docentry   " & _
                        '           "from [@FACE_RESOLUCION] a " & _
                        '           "inner join OPCH b " & _
                        '           "on a.U_SERIE = b.Series " & _
                        '           "inner join NNM1 c " & _
                        '           "on b.Series=c.Series " & _
                        '           "where a.U_SERIE = " & mySerie.Value & " And b.DocEntry = " & oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(1).Name, i)
                        'End If
                        sql = "CALL SPFACE_DATOSDOC ( " & oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(1).Name, i) & ")"
                        RecSet = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                        RecSet.DoQuery(sql)
                        myLog = "Registros Obtenidos " & RecSet.RecordCount & vbNewLine
                        If RecSet.RecordCount > 0 Then

                            If oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(2).Name, i) = "Factura" Then
                                Tipo = "FAC"
                            ElseIf oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(2).Name, i) = "Nota debito" Then
                                Tipo = "ND"
                            ElseIf oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(2).Name, i) = "Factura Proveedor" Then
                                Tipo = "FACP"
                            Else
                                Tipo = "NC"
                            End If
                            myLog += "Tipo de documento a procesar " & Tipo & vbNewLine
                            Serie = RecSet.Fields.Item("Series").Value
                            Doc = RecSet.Fields.Item("DocNum").Value
                            SerieName = RecSet.Fields.Item("SeriesName").Value
                            docEntry = RecSet.Fields.Item("DocEntry").Value
                            myLog += "Enviando documento " & docEntry & vbNewLine
                            If TipoGFACE <> TipoFACE.GuateFacturas Then
                                Utils.EnviaDocumento(oCompany, SBO_Application, Tipo, Serie, Doc, SerieName, Utils.Pais, docEntry, True, i + 1, myLog)
                            Else
                                Dim del As SAPbouiCOM.EditText
                                Dim Al As SAPbouiCOM.EditText
                                del = oForm.Items.Item("txtDel").Specific
                                Al = oForm.Items.Item("txtAl").Specific

                                Dim path As String = dirXML & "txtGuateFacturas\"
                                Dim file As String = SerieName & "-" & del.Value & "-" & Al.Value & ".txt"
                                content += Utils.GeneraBatchTXT(oCompany, docEntry, Tipo)
                                If System.IO.Directory.Exists(path) = False Then
                                    System.IO.Directory.CreateDirectory(path)
                                End If
                                If content <> "" Then
                                    Dim vError As String = ""
                                    If SaveTextToFile(content & vbNewLine, path & file, vError) = False Then
                                        Throw New Exception("Error al intentar guardar archivo txt motivo: " & vError & vbNewLine & "Ubicación: " & path & file)
                                    End If
                                    'If Tipo <> "NC" Then
                                    '    sql = "UPDATE OINV SET U_ESTADO_FACE='A', U_FIRMA_ELETRONICA='GENERADO HACIA ARCHIVO TXT " & path & file & ".txt' WHERE DOCENTRY=" & docEntry
                                    'Else
                                    '    sql = "UPDATE ORIN SET U_ESTADO_FACE='A', U_FIRMA_ELETRONICA='GENERADO HACIA ARCHIVO TXT " & path & file & ".txt' WHERE DOCENTRY=" & docEntry
                                    'End If
                                    'RecSet.DoQuery(sql)

                                End If
                            End If
                        End If
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(RecSet)
                        RecSet = Nothing
                        GC.Collect()
                        cont += 1
                        'ProgressBar.Value += 1
                        lblProc.Caption = "Registros procesados (" & cont & " de " & TotalDocs & ")"

                        'SBO_Application.SetStatusBarMessage("Documentos Procesados (" & cont & " de " & TotalDocs & ")", SAPbouiCOM.BoMessageTime.bmt_Short, False)
                    Next
                    'ProgressBar.Stop()
                    'System.Runtime.InteropServices.Marshal.ReleaseComObject(ProgressBar)
                    oForm.DataSources.DataTables.Item(0).Clear()
                    myLog += "Proceso Finalizado" & vbNewLine
                    lblProc.Caption = "Proceso Finalizado..."
                    'SBO_Application.SetStatusBarMessage("Proceso finalizado...", SAPbouiCOM.BoMessageTime.bmt_Short, False)
                Catch ex As Exception
                    'ProgressBar.Stop()
                    myLog += "Ocurrio un error" & ex.Message & vbNewLine
                    SBO_Application.SetStatusBarMessage(ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short)
                End Try
            End If
        Catch ex As Exception
            Dim log As String = Utils.GetFileContents(Utils.FileLog) & vbNewLine & ex.Message
            Utils.SaveTextToFile(log, Utils.FileLog)
            SBO_Application.SetStatusBarMessage(ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, True)
        End Try
        Utils.SaveTextToFile(myLog, Application.StartupPath & "\BatchLog.log")
    End Sub

    Private Sub Consulta()
        Dim cmbItem As SAPbouiCOM.ComboBox
        Dim chkRecha As SAPbouiCOM.CheckBox
        Dim del As SAPbouiCOM.EditText
        Dim al As SAPbouiCOM.EditText
        Try
            cmbItem = oForm.Items.Item("cmbSerie").Specific
            chkRecha = oForm.Items.Item("chkRecha").Specific
            del = oForm.Items.Item("txtDel").Specific
            al = oForm.Items.Item("txtAl").Specific
            If cmbItem.Value = "" Then
                Throw (New Exception("Debe de seleccionar la serie a enviar"))
            End If
            If del.Value = "" Then
                Throw New Exception("Debe de ingresar la fecha inicial")
            End If
            If al.Value = "" Then
                Throw New Exception("Debe de ingresar la fecha final")
            End If
            If del.Value > al.Value Then
                Throw New Exception("El rango de fechas es inválido")
            End If
            LlengaGrid(cmbItem.Value, chkRecha.Checked, del.Value, al.Value)
        Catch ex As Exception
            SBO_Application.SetStatusBarMessage(ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, True)
        End Try
    End Sub

    Private Sub LlengaGrid(ByVal Serie As String, ByVal incluirRechazadas As Boolean, ByVal del As String, ByVal al As String)
        Dim sql As String
        Dim oGrid As SAPbouiCOM.Grid
        Dim oitem As SAPbouiCOM.Item
        Dim RecSet As SAPbobsCOM.Recordset
        Dim cmdEnviar As SAPbouiCOM.Item
        Dim chkRecha As SAPbouiCOM.Item
        Dim lblReg As SAPbouiCOM.StaticText
        Dim lblRegSum As SAPbouiCOM.StaticText

        Try

            If Utils.Empresa = EmpresaFACE.LLAMASA Or Utils.Empresa = EmpresaFACE.QUALIPHARM Or Utils.Empresa = EmpresaFACE.FFACSA Or Utils.Empresa = EmpresaFACE.PRINTER Then
                sql = "CALL SP_INT_LISTADOBATCH('" & Serie & "','" & del & "','" & al & "')"
            Else
                'sql = "select Estado=case isnull(a.U_ESTADO_FACE,'P') when 'P' then 'Pendiente' when 'R' then 'Rechazado' when 'A' then 'Autorizado' end ,a.docentry 'Correlativo','Tipo Documento'= case a.DocSubType when '--' then 'Factura' when 'DN' then 'Nota Debito' End , " & _
                '      "DocNum 'No. Documento',convert(char(10),DocDate,103)  'Fecha Documento' ,CardName  'Cliente',convert(numeric(18,2),DocTotal,1)  'Total Documento' " & _
                '      "from oinv a " & _
                '      "inner join NNM1 b " & _
                '      "on a.Series = b.Series "
                If incluirRechazadas Then
                    ' sql += "where isnull(U_ESTADO_FACE,'P') in ('P','R') "
                    sql = ("CALL SP_FACE_QUERYS_4P('2','" & del & "','" & al & "','" & Serie & "','')")
                Else
                    'sql += "where isnull(U_ESTADO_FACE,'P')='P' "
                    sql = ("CALL SP_FACE_QUERYS_4P('3','" & del & "','" & al & "','" & Serie & "','')")
                End If
                'sql += " and a.docdate between '" & del & "' and '" & al & "'"
                'sql += "and   b.Series = " & Serie & _
                '      " union " & _
                '      "select Estado=case isnull(a.U_ESTADO_FACE,'P') when 'P' then 'Pendiente' when 'R' then 'Rechazado' when 'A' then 'Autorizado' end ,a.docentry 'Correlativo','Nota Credito' 'Tipo Documento',  " & _
                '      "DocNum 'No. Documento',convert(char(10),DocDate,103)  'Fecha Documento' ,CardName  'Cliente',convert(numeric(18,2),DocTotal,1)  'Total Documento'  " & _
                '      "from ORIN  a " & _
                '      "inner join NNM1 b " & _
                '      "on a.Series = b.Series  "
                'If incluirRechazadas Then
                '    sql += "where isnull(U_ESTADO_FACE,'P') in ('P','R') "
                'Else
                '    sql += "where isnull(U_ESTADO_FACE,'P')='P' "
                'End If
                'sql += " and a.docdate between '" & del & "' and '" & al & "'"
                'sql += "and   b.Series =" & Serie & _
                '      " order by Correlativo desc"
            End If
            cmdEnviar = oForm.Items.Item("cmdEnviar")
            cmdEnviar.Enabled = False
            chkRecha = oForm.Items.Item("chkRecha")
            chkRecha.Enabled = False
            lblReg = oForm.Items.Item("lblReg").Specific
            lblRegSum = oForm.Items.Item("lblRegSum").Specific
            RecSet = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            RecSet.DoQuery(sql)
            If RecSet.RecordCount > 0 Then
                oitem = oForm.Items.Item("grdDatos")
                oGrid = oitem.Specific
                oForm.DataSources.DataTables.Item(0).ExecuteQuery(sql)
                oGrid.DataTable = oForm.DataSources.DataTables.Item("MyDataTable")
                oGrid.AutoResizeColumns()
                oGrid.Columns.Item(1).RightJustified = True
                oGrid.Columns.Item(3).RightJustified = True
                oGrid.Columns.Item(6).RightJustified = True
                oGrid.Columns.Item(0).Editable = False
                oGrid.Columns.Item(1).Editable = False
                oGrid.Columns.Item(2).Editable = False
                oGrid.Columns.Item(3).Editable = False
                oGrid.Columns.Item(4).Editable = False
                oGrid.Columns.Item(5).Editable = False
                oGrid.Columns.Item(6).Editable = False
                oGrid.Columns.Item(7).Editable = False
                If Utils.Empresa = EmpresaFACE.LLAMASA Or Utils.Empresa = EmpresaFACE.QUALIPHARM Or Utils.Empresa = EmpresaFACE.FFACSA Or Utils.Empresa = EmpresaFACE.PRINTER Then oGrid.Columns.Item(8).Editable = False
                cmdEnviar.Enabled = True
                chkRecha.Enabled = True
                lblReg.Caption = "Total registros (" & oGrid.Rows.Count & ")"
                'lblRegSum.Caption = "Monto Total (" & MontoTotal(oGrid) & ")"

            Else
                oForm.DataSources.DataTables.Item(0).Clear()
                SBO_Application.SetStatusBarMessage("La información solicitada no ha sido encontrada", SAPbouiCOM.BoMessageTime.bmt_Short, False)
            End If
        Catch ex As Exception
            SBO_Application.SetStatusBarMessage(ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, True)
        End Try
    End Sub

    Private Sub LlenaSeries()
        Dim sql As String
        Dim RecSet As SAPbobsCOM.Recordset
        Dim sUser = oCompany.UserSignature
        Try

            sql += ("CALL SP_FACE_QUERYS('9','','')")

            RecSet = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            RecSet.DoQuery(sql)
            If RecSet.RecordCount > 0 Then
                Dim cmbSerie As SAPbouiCOM.ComboBox
                For i = 0 To RecSet.RecordCount - 1
                    cmbSerie = oForm.Items.Item("cmbSerie").Specific
                    cmbSerie.ValidValues.Add(RecSet.Fields.Item("Series").Value, RecSet.Fields.Item("SeriesName").Value)
                    RecSet.MoveNext()
                Next
            End If
        Catch ex As Exception

        End Try
    End Sub

    Private Function MontoTotal(oGrid As SAPbouiCOM.Grid) As Double
        Dim conteiner As Double = 0

        For i As Integer = 0 To oGrid.Rows.Count - 1

            Dim obtiene As String = Convert.ToString(oGrid.DataTable.GetValue("Total Documento", i))

            conteiner = conteiner + Convert.ToDouble(obtiene)
        Next

        Return conteiner
    End Function

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub
End Class
