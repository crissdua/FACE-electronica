Imports System.IO

Public Class ConsultaForm

#Region "Load SBO Form"
    Dim XmlForm As String = Replace(Application.StartupPath & "\Consulta_FACE.srf", "\\", "\")
    Dim Doc As String = Replace(Application.StartupPath & "\mensaje.txt", "\\", "\")

    Private WithEvents SBO_Application As SAPbouiCOM.Application
    Private oForm As SAPbouiCOM.Form
    Private oDBDataSource As SAPbouiCOM.DBDataSource
    Private oCompany As SAPbobsCOM.Company
    Private oitem As SAPbouiCOM.Item
    Private cmdVer As SAPbouiCOM.Button
    Private cmdDenegada As SAPbouiCOM.Button
    Private cmbTipo As SAPbouiCOM.ComboBox
    Private cmbEstado As SAPbouiCOM.ComboBox
    Private oFilters As SAPbouiCOM.EventFilters
    Private oFilter As SAPbouiCOM.EventFilter


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


            If Utils.ActivateFormIsOpen(SBO_Application, "SBOConsultaFACE") = False Then
                LoadFromXML(XmlForm)

                '// Get the added form object by using the form's UID
                oForm = SBO_Application.Forms.Item("SBOConsultaFACE")

                'SetFilters()

                Me.LLenaCombos()

                oitem = oForm.Items.Item("cmdVer")
                cmdVer = oitem.Specific

                oitem = oForm.Items.Item("cmdMotivo")
                cmdDenegada = oitem.Specific

                oitem = oForm.Items.Item("cmbTipo")
                cmbTipo = oitem.Specific

                oitem = oForm.Items.Item("cmbEstado")
                cmbEstado = oitem.Specific

                Dim del As SAPbouiCOM.EditText
                Dim Al As SAPbouiCOM.EditText

                oForm.DataSources.UserDataSources.Add("UDDate", SAPbouiCOM.BoDataType.dt_DATE)
                oForm.DataSources.UserDataSources.Add("UDDate2", SAPbouiCOM.BoDataType.dt_DATE)
                del = oForm.Items.Item("txtDel").Specific
                Al = oForm.Items.Item("txtAl").Specific
                del.DataBind.SetBound(True, "", "UDDate")
                Al.DataBind.SetBound(True, "", "UDDate2")
            Else
                oForm = SBO_Application.Forms.Item("SBOConsultaFACE")
            End If

        Catch ex As Exception
            SBO_Application.MessageBox(ex.Message)
        End Try
    End Sub

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
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
        Dim lRetCode As Integer

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

#Region "Codigo General"

    Private Sub SBO_Application_ItemEvent(ByVal FormUID As String, ByRef pVal As SAPbouiCOM.ItemEvent, ByRef BubbleEvent As Boolean) Handles SBO_Application.ItemEvent
        Try
            If pVal.FormType = 60006 Then


                If pVal.EventType = SAPbouiCOM.BoEventTypes.et_FORM_CLOSE And pVal.BeforeAction = True Then
                    oForm = Nothing
                    oCompany = Nothing
                    SBO_Application = Nothing
                End If

                If pVal.ItemUID = "cmdCons" And pVal.EventType = SAPbouiCOM.BoEventTypes.et_CLICK And pVal.Before_Action = False Then
                    LlenaGrid(cmbTipo.Value, cmbEstado.Value)
                End If

                If pVal.ItemUID = "cmdVer" And pVal.EventType = SAPbouiCOM.BoEventTypes.et_CLICK And pVal.Before_Action = False Then
                    verPDF()
                End If

                If pVal.ItemUID = "cmdMotivo" And pVal.EventType = SAPbouiCOM.BoEventTypes.et_CLICK And pVal.Before_Action = False Then
                    verMotivo()
                End If

            End If
        Catch ex As Exception
            SBO_Application.MessageBox(ex.Message)
        End Try
    End Sub

    Private Sub verPDF()
        Dim oitem As SAPbouiCOM.Item
        Dim oGrid As SAPbouiCOM.Grid
        Dim sql As String
        Dim RecSet As SAPbobsCOM.Recordset
        Dim PathPDF As String
        Dim FileName As String
        Dim Serie As String
        Dim Doc As String
        Dim Tipo As String
        Dim cmbTipo As SAPbouiCOM.ComboBox

        Try
            oitem = oForm.Items.Item("grdDatos")
            oGrid = oitem.Specific

            If oGrid.Rows.SelectedRows.Count = 0 Then
                Throw New Exception("Debe de seleccionar un documento")
            End If
           
            oitem = oForm.Items.Item("cmbTipo")
            cmbTipo = oitem.Specific

            If cmbTipo.Value = "Facturas" Then
                Tipo = "F"
            ElseIf cmbTipo.Value = "Notas de débito" Then
                Tipo = "N"
            Else
                Tipo = "C"
            End If
            For i = 0 To oGrid.Rows.Count - 1
                If oGrid.Rows.IsSelected(i) Then
                    Serie = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(0).Name, i)
                    Doc = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(2).Name, i)
                    If Tipo = "F" Then
                        sql = "select U_FACE_PDFFILE  from OINV WHERE SERIES='" & Serie & "' aND   DocNum ='" & Doc & "' AND DocSubType ='--'"
                    ElseIf Tipo = "N" Then
                        sql = "select U_FACE_PDFFILE  from OINV WHERE SERIES='" & Serie & "' aND   DocNum ='" & Doc & "' AND DocSubType ='DN'"
                    Else
                        sql = "select U_FACE_PDFFILE  from ORIN WHERE SERIES='" & Serie & "' aND   DocNum ='" & Doc & "'"
                    End If
                    RecSet = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                    RecSet.DoQuery(sql)
                    System.Diagnostics.Process.Start(RecSet.Fields.Item("U_FACE_PDFFILE").Value)
                    Exit For
                End If
            Next
            System.Runtime.InteropServices.Marshal.ReleaseComObject(RecSet)
            RecSet = Nothing
            GC.Collect()
        Catch ex As Exception
            SBO_Application.MessageBox(ex.Message)
        End Try
    End Sub

    Private Sub verMotivo()
        Dim oitem As SAPbouiCOM.Item
        Dim oGrid As SAPbouiCOM.Grid
        Dim Serie As String
        Dim Doc As String
        Dim Tipo As String
        Dim sql As String
        Dim RecSet As SAPbobsCOM.Recordset

        Try
            oitem = oForm.Items.Item("grdDatos")
            oGrid = oitem.Specific

            If oGrid.Rows.SelectedRows.Count = 0 Then
                Throw New Exception("Debe de seleccionar un documento")
            End If

            If cmbTipo.Value = "Facturas" Then
                Tipo = "--"
            ElseIf cmbTipo.Value = "Notas de débito" Then
                Tipo = "DN"
            End If

            For i = 0 To oGrid.Rows.Count - 1
                If oGrid.Rows.IsSelected(i) Then
                    Serie = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(0).Name, i)
                    Doc = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(2).Name, i)
                    If cmbTipo.Value = "Facturas" Or cmbTipo.Value = "Notas de débito" Then
                        sql = "select U_MOTIVO_RECHAZO  from OINV  where Series=" & Serie & " and DocNum ='" & Doc & "' and DocSubType ='" & Tipo & "'"
                    Else
                        sql = "select U_MOTIVO_RECHAZO  from ORIN  where Series=" & Serie & " and DocNum ='" & Doc & "'"
                    End If
                    RecSet = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                    RecSet.DoQuery(sql)
                    If RecSet.RecordCount > 0 Then
                        Dim frmM As New Mensaje(RecSet.Fields.Item("U_MOTIVO_RECHAZO").Value.ToString)

                        'SaveTextToFile, Doc)
                        'System.Diagnostics.Process.Start(Doc)
                    End If
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(RecSet)
                    RecSet = Nothing
                    GC.Collect()
                    Exit For
                End If
            Next
        Catch ex As Exception
            SBO_Application.MessageBox(ex.Message)
        End Try
    End Sub


    Public Function SaveTextToFile(ByVal strData As String, _
     ByVal FullPath As String, _
       Optional ByVal ErrInfo As String = "") As Boolean

        Dim Contents As String
        Dim bAns As Boolean = False
        Dim objReader As StreamWriter
        Try


            objReader = New StreamWriter(FullPath)
            objReader.Write(strData)
            objReader.Close()
            bAns = True
        Catch Ex As Exception
            ErrInfo = Ex.Message

        End Try
        Return bAns
    End Function

    Private Sub LLenaCombos()

        Dim oCombo As SAPbouiCOM.ComboBox
        Dim oItem As SAPbouiCOM.Item
        Try
            oItem = oForm.Items.Item("cmbTipo")
            oCombo = oItem.Specific
            oCombo.ValidValues.Add("Facturas", "")
            oCombo.ValidValues.Add("Notas de débito", "")
            oCombo.ValidValues.Add("Notas de crédito", "")

            oItem = oForm.Items.Item("cmbEstado")
            oCombo = oItem.Specific
            oCombo.ValidValues.Add("Aprobadas", "")
            oCombo.ValidValues.Add("Denegadas", "")
        Catch ex As Exception
            SBO_Application.MessageBox(ex.Message)
        End Try
    End Sub

    Private Sub LlenaGrid(ByVal Tipo As String, ByVal Estado As String)
        Dim sql As String
        Dim RecSet As SAPbobsCOM.Recordset
        Dim oitem As SAPbouiCOM.Item
        Dim oGrid As SAPbouiCOM.Grid
        Dim docType As String
        Dim estatus As String
        Dim del As SAPbouiCOM.EditText = oForm.Items.Item("txtDel").Specific
        Dim Al As SAPbouiCOM.EditText = oForm.Items.Item("txtAl").Specific
        Try
            If Estado = "Aprobadas" Then
                estatus = "A"
            Else
                estatus = "R"
            End If
            If Tipo = "Facturas" Or Tipo = "Notas de débito" Then
                If Tipo = "Facturas" Then
                    docType = "--"
                Else
                    docType = "DN"
                End If
                sql = "select a.Series 'Codigo Serie',SeriesName  'Serie Documento', DocNum 'No. Documento',convert(char(10),DocDate,103)  'Fecha Documento' ,CardName  'Cliente',convert(numeric(18,2),DocTotal,1)  'Total Documento' " & _
                      "from oinv a " & _
                      "inner join NNM1 b " & _
                      "on a.Series = b.Series " & _
                      "where U_ESTADO_FACE='" & estatus & "' " & _
                      "AND a.DocSubType ='" & docType & "'" & _
                      " and a.docdate between '" & del.Value & "' and '" & Al.Value & "'" & _
                      " order by a.docdate desc "

            Else
                sql = "select a.Series 'Codigo Serie',SeriesName 'Serie Documento', DocNum 'No. Documento',convert(char(10),DocDate,103)  'Fecha Documento',CardName 'Cliente',convert(numeric(18,2),DocTotal,1) 'Total Documento' " & _
                      "from orin a " & _
                      "inner join NNM1 b " & _
                      "on a.Series = b.Series " & _
                      "where U_ESTADO_FACE='" & estatus & "'" & _
                      " AND a.DocSubType ='--'" & _
                      " and a.docdate between '" & del.Value & "' and '" & Al.Value & "'" & _
                      " order by a.docdate desc "
            End If

            RecSet = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            RecSet.DoQuery(sql)

            oitem = oForm.Items.Item("grdDatos")
            oGrid = oitem.Specific


            If RecSet.RecordCount > 0 Then

                If oForm.DataSources.DataTables.Count = 0 Then oForm.DataSources.DataTables.Add("MyDataTable")
                oForm.DataSources.DataTables.Item(0).Clear()
                oForm.DataSources.DataTables.Item(0).ExecuteQuery(sql)

                oGrid.DataTable = oForm.DataSources.DataTables.Item("MyDataTable")
                oGrid.SelectionMode = SAPbouiCOM.BoMatrixSelect.ms_Single
                For i = 0 To oGrid.Columns.Count - 1
                    oGrid.Columns.Item(i).Editable = False
                Next

                If estatus = "A" Then
                    oitem = oForm.Items.Item("cmdVer")
                    oitem.Enabled = True
                    oitem = oForm.Items.Item("cmdMotivo")
                    oitem.Enabled = False
                Else
                    oitem = oForm.Items.Item("cmdMotivo")
                    oitem.Enabled = True
                    oitem = oForm.Items.Item("cmdVer")
                    oitem.Enabled = False
                End If
            Else
                oitem = oForm.Items.Item("cmdVer")
                oitem.Enabled = False
                oitem = oForm.Items.Item("cmdMotivo")
                oitem.Enabled = False
                If oForm.DataSources.DataTables.Count > 0 Then oForm.DataSources.DataTables.Item(0).Clear()
                SBO_Application.MessageBox("No se encontró información con los críterios seleccionados")
            End If
        Catch ex As Exception
            SBO_Application.MessageBox(ex.Message)
        End Try
    End Sub
#End Region

End Class
