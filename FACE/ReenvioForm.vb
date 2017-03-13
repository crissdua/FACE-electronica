Imports System.Threading

Public Class ReenvioForm

#Region "Load SBO Form"
    Private XmlForm As String = Replace(Application.StartupPath & "\Reenvio.srf", "\\", "\")
    Private WithEvents SBO_Application As SAPbouiCOM.Application
    Private oForm As SAPbouiCOM.Form
    Private oCompany As SAPbobsCOM.Company
    Private oFilters As SAPbouiCOM.EventFilters
    Private oFilter As SAPbouiCOM.EventFilter
    Private mThreadFic As Thread
    Private ProgressBar As SAPbouiCOM.ProgressBar
    Private Sub SetApplication()


        '*******************************************************************
        '// Use an SboGuiApi object to establish connection
        '// with the SAP Business One application and return an
        '// initialized appliction object
        '*******************************************************************

        Dim SboGuiApi As SAPbouiCOM.SboGuiApi
        Dim sConnectionString As String

        Try
            SboGuiApi = New SAPbouiCOM.SboGuiApi

            '// by following the steps specified above, the following
            '// statment should be suficient for either development or run mode

            sConnectionString = Utils.ConnectionString  'Environment.GetCommandLineArgs.GetValue(1)

            '// connect to a running SBO Application

            SboGuiApi.Connect(sConnectionString)

            '// get an initialized application object

            SBO_Application = SboGuiApi.GetApplication(-1)

        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical, "Ocurrio un error")
        End Try

    End Sub


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

            If Utils.ActivateFormIsOpen(SBO_Application, "SBOReenvio") = False Then
                LoadFromXML(XmlForm)

                '// Get the added form object by using the form's UID
                oForm = SBO_Application.Forms.Item("SBOReenvio")


                '// Show the loaded Form
                oForm.Visible = True
                oForm.PaneLevel = 1

                'SetFilters()

                oForm.DataSources.DataTables.Add("MyDataTable")

                Dim del As SAPbouiCOM.EditText
                Dim Al As SAPbouiCOM.EditText

                oForm.DataSources.UserDataSources.Add("UDDate", SAPbouiCOM.BoDataType.dt_DATE)
                oForm.DataSources.UserDataSources.Add("UDDate2", SAPbouiCOM.BoDataType.dt_DATE)
                del = oForm.Items.Item("txtDel").Specific
                Al = oForm.Items.Item("txtAl").Specific
                del.DataBind.SetBound(True, "", "UDDate")
                Al.DataBind.SetBound(True, "", "UDDate2")
            Else
                oForm = SBO_Application.Forms.Item("SBOReenvio")
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

#End Region


    Private Sub SBO_Application_ItemEvent(ByVal FormUID As String, ByRef pVal As SAPbouiCOM.ItemEvent, ByRef BubbleEvent As Boolean) Handles SBO_Application.ItemEvent
        Try

          

            If pVal.FormType = "60006" Then

                If pVal.EventType = SAPbouiCOM.BoEventTypes.et_FORM_CLOSE And pVal.BeforeAction = True Then
                    oForm = Nothing
                    oCompany = Nothing
                    SBO_Application = Nothing
                End If

                If pVal.ItemUID = "cmdBuscar" And pVal.EventType = SAPbouiCOM.BoEventTypes.et_CLICK And pVal.Before_Action = True Then
                    Dim Del As SAPbouiCOM.EditText
                    Dim Al As SAPbouiCOM.EditText
                    Dim result As Integer
                    Dim cmdBuscar As SAPbouiCOM.Button

                    Del = oForm.Items.Item("txtDel").Specific
                    Al = oForm.Items.Item("txtAl").Specific
                    If Del.Value = "" And Al.Value = "" Then
                        result = SBO_Application.MessageBox("Desea obtener todos los documentos rechazados?", 1, "SI", "NO")
                        If result <> 1 Then Exit Sub
                    Else
                        If Del.Value = "" And Al.Value <> "" Then
                            Throw New Exception("Debe de ingresar la fecha inicial")
                        End If
                        If Del.Value <> "" And Al.Value = "" Then
                            Throw New Exception("Debe de ingresar la fecha final")
                        End If
                        If Del.Value > Al.Value Then
                            Throw New Exception("El rango de fechas es inválido")
                        End If
                    End If
                    LlengaGrid()
                    BubbleEvent = False
                End If

                If pVal.ItemUID = "cmdResult" And pVal.EventType = SAPbouiCOM.BoEventTypes.et_CLICK And pVal.Before_Action = False Then
                    If System.IO.File.Exists(Utils.FileLog) Then
                        System.Diagnostics.Process.Start("notepad.exe", Utils.FileLog)
                    Else
                        Throw New Exception("El archivo de resultados no existe")
                    End If
                    BubbleEvent = False
                End If

                'If pVal.ItemUID = "cmdSalir" And pVal.EventType = SAPbouiCOM.BoEventTypes.et_CLICK And pVal.Before_Action = True Then
                '    oForm.Close()
                '    BubbleEvent = False
                'End If

                If pVal.EventType = SAPbouiCOM.BoEventTypes.et_CLICK And pVal.ItemUID = "grdDatos" And pVal.ColUID = "Seleccionar" And pVal.Row = -1 Then
                    Dim oGrid As SAPbouiCOM.Grid
                    Dim valCol As String
                    oGrid = oForm.Items.Item("grdDatos").Specific
                    For i = 0 To oGrid.Rows.Count - 1
                        valCol = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(0).Name, i)
                        If valCol = "Y" Then
                            oGrid.DataTable.SetValue(oGrid.DataTable.Columns.Item(0).Name, i, "")
                        Else
                            oGrid.DataTable.SetValue(oGrid.DataTable.Columns.Item(0).Name, i, "Y")
                        End If
                    Next
                    BubbleEvent = False
                End If

                If pVal.ItemUID = "cmdEnviar" And pVal.EventType = SAPbouiCOM.BoEventTypes.et_CLICK And pVal.Before_Action = False Then
                    mThreadFic = New Thread(New ThreadStart(AddressOf Enviar))
                    mThreadFic.Start()
                    BubbleEvent = False
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
        Dim docEntry As String = ""
        Dim sql As String = ""
        Dim cmdEnvio As SAPbouiCOM.Item
        Dim RecSet As SAPbobsCOM.Recordset
        Dim TotalDocs As String
        Dim cont As Integer = 0

        Try
            oGrid = oForm.Items.Item("grdDatos").Specific
            If oGrid.Rows.Count > 0 Then
                cmdEnvio = oForm.Items.Item("cmdEnviar")
                cmdEnvio.Enabled = False
                TotalDocs = oGrid.Rows.Count
                Utils.FileLog = Replace(Application.StartupPath & "\ResultadosReenvio" & Format(Date.Now, "ddMMyyyyHHmmss") & ".txt", "\\", "\")
                'SBO_Application.SetStatusBarMessage("Reenviando documentos favor espere...", SAPbouiCOM.BoMessageTime.bmt_Short, False)
                ProgressBar = Me.SBO_Application.StatusBar.CreateProgressBar("Procesando documentos electrónicos por favor espere...", oGrid.Rows.Count - 1, False)
                For i = 0 To oGrid.Rows.Count - 1
                    If oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(0).Name, i) = "Y" Then
                        If oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(3).Name, i) = "Factura" Then
                            Tipo = "FAC"
                            sql = "select U_TIPO_DOC Tipo,b.Series,b.DocNum,c.SeriesName,b.docentry  " & _
                                  "from [@FACE_RESOLUCION] a " & _
                                  "inner join OINV b " & _
                                  "on a.U_SERIE = b.Series " & _
                                  "inner join NNM1 c " & _
                                  "on b.Series=c.Series " & _
                                  "where a.U_SERIE =b.series And b.DocEntry = " & oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(2).Name, i)
                        ElseIf oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(3).Name, i) = "Nota debito" Then
                            Tipo = "ND"
                            sql = "select U_TIPO_DOC Tipo,b.Series,b.DocNum,c.SeriesName,b.docentry  " & _
                                  "from [@FACE_RESOLUCION] a " & _
                                  "inner join OINV b " & _
                                  "on a.U_SERIE = b.Series " & _
                                  "inner join NNM1 c " & _
                                  "on b.Series=c.Series " & _
                                  "where a.U_SERIE =b.series And b.DocEntry = " & oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(2).Name, i)
                        Else
                            Tipo = "NC"
                            sql = "select U_TIPO_DOC Tipo,b.Series,b.DocNum,c.SeriesName,b.docentry  " & _
                                  "from [@FACE_RESOLUCION] a " & _
                                  "inner join ORIN b " & _
                                  "on a.U_SERIE = b.Series " & _
                                  "inner join NNM1 c " & _
                                  "on b.Series=c.Series " & _
                                  "where a.U_SERIE =b.series And b.DocEntry = " & oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(2).Name, i)
                        End If
                        RecSet = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                        RecSet.DoQuery(sql)
                        If RecSet.RecordCount > 0 Then
                            Serie = RecSet.Fields.Item("Series").Value
                            Doc = RecSet.Fields.Item("DocNum").Value
                            SerieName = RecSet.Fields.Item("SeriesName").Value
                            docEntry = RecSet.Fields.Item("docentry").Value
                            Utils.EnviaDocumento(oCompany, SBO_Application, Tipo, Serie, Doc, SerieName, Utils.Pais, docEntry, True)
                        End If
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(RecSet)
                        RecSet = Nothing
                        GC.Collect()
                    End If

                    cont += 1
                    ProgressBar.Value += 1
                    ProgressBar.Text = "Documentos electrónicos reenviados (" & cont & " de " & TotalDocs & ")"

                    'SBO_Application.SetStatusBarMessage("Documentos reenviados (" & cont & " de " & TotalDocs & ")", SAPbouiCOM.BoMessageTime.bmt_Short, False)
                Next
                ProgressBar.Stop()
                System.Runtime.InteropServices.Marshal.ReleaseComObject(ProgressBar)
                oForm.DataSources.DataTables.Item(0).Clear()
                SBO_Application.SetStatusBarMessage("Proceso finalizado...", SAPbouiCOM.BoMessageTime.bmt_Short, False)
            End If
        Catch ex As Exception
            'ProgressBar.Stop()
            Dim log As String = Utils.GetFileContents(Utils.FileLog) & vbNewLine & ex.Message
            Utils.SaveTextToFile(log, Utils.FileLog)
            SBO_Application.SetStatusBarMessage(ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, True)
        End Try
    End Sub



    Private Sub LlengaGrid()
        Dim sql As String
        Dim oGrid As SAPbouiCOM.Grid
        Dim oItem As SAPbouiCOM.Item
        Dim Del As SAPbouiCOM.EditText
        Dim Al As SAPbouiCOM.EditText
        Dim RecSet As SAPbobsCOM.Recordset
        Dim cmdEnviar As SAPbouiCOM.Item
        Dim lblReg As SAPbouiCOM.StaticText

        Try
            Del = oForm.Items.Item("txtDel").Specific
            Al = oForm.Items.Item("txtAl").Specific

            sql = "select 'Y' Seleccionar,a.u_motivo_rechazo 'Descripcion Rechazó',a.docentry 'Correlativo','Tipo Documento'= case a.DocSubType when '--' then 'Factura' when 'DN' then 'Nota Debito' End ,SeriesName  'Serie Documento', " & _
                  "DocNum 'No. Documento',convert(char(10),DocDate,103)  'Fecha Documento' ,CardName  'Cliente',convert(numeric(18,2),DocTotal,1)  'Total Documento'  " & _
                  "from oinv a  " & _
                  "inner join NNM1 b  " & _
                  "on a.Series = b.Series " & _
                  "where U_ESTADO_FACE='R' "
            If Del.Value <> "" And Al.Value <> "" Then sql += " and DocDate between '" & Del.Value & "' and '" & Al.Value & "' "
            sql += "union " & _
                  "select 'Y' Seleccionar,a.u_motivo_rechazo 'Descripcion Rechazó',a.docentry 'Correlativo','Nota Credito',SeriesName  'Serie Documento', DocNum 'No. Documento',convert(char(10),DocDate,103)  'Fecha Documento' ,CardName  'Cliente',convert(numeric(18,2),DocTotal,1)  'Total Documento'  " & _
                  "from ORIN  a " & _
                  "inner join NNM1 b  " & _
                  "on a.Series = b.Series " & _
                  "where U_ESTADO_FACE='R' "
            If Del.Value <> "" And Al.Value <> "" Then sql += " and DocDate between '" & Del.Value & "' and '" & Al.Value & "' "
            sql += "order by a.docentry desc"

            RecSet = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            RecSet.DoQuery(sql)
            cmdEnviar = oForm.Items.Item("cmdEnviar")
            cmdEnviar.Enabled = False
            If RecSet.RecordCount > 0 Then
                oItem = oForm.Items.Item("grdDatos")
                oGrid = oItem.Specific
                oForm.DataSources.DataTables.Item(0).ExecuteQuery(sql)
                oGrid.DataTable = oForm.DataSources.DataTables.Item("MyDataTable")
                oGrid.Columns.Item(0).Type = SAPbouiCOM.BoGridColumnType.gct_CheckBox
                oGrid.Columns.Item(1).Editable = False
                oGrid.Columns.Item(2).Editable = False
                oGrid.Columns.Item(3).Editable = False
                oGrid.Columns.Item(4).Editable = False
                oGrid.Columns.Item(5).Editable = False
                oGrid.Columns.Item(6).Editable = False
                oGrid.Columns.Item(7).Editable = False
                oGrid.Columns.Item(8).Editable = False
                oGrid.Columns.Item(2).RightJustified = True
                oGrid.Columns.Item(4).RightJustified = True
                oGrid.Columns.Item(8).RightJustified = True
                cmdEnviar.Enabled = True
                lblReg = oForm.Items.Item("lblReg").Specific
                lblReg.Caption = "Total registros (" & IIf((oGrid.Rows.Count - 1) = 0, "1", oGrid.Rows.Count) & ")"
            Else
                oForm.DataSources.DataTables.Item(0).Clear()
                SBO_Application.SetStatusBarMessage("La información solicitada no ha sido encontrada", SAPbouiCOM.BoMessageTime.bmt_Short, False)
            End If
        Catch ex As Exception
            SBO_Application.SetStatusBarMessage(ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, True)
        End Try
    End Sub
End Class
