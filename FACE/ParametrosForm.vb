Imports System.IO

Public Class ParametrosForm

#Region "Load SBO Form"
    Dim XmlForm As String = Replace(Application.StartupPath & "\Parametros_FACE.srf", "\\", "\")

    Private WithEvents SBO_Application As SAPbouiCOM.Application
    Private oForm As SAPbouiCOM.Form
    Private oDBDataSource As SAPbouiCOM.DBDataSource
    Private oFilters As SAPbouiCOM.EventFilters
    Private oFilter As SAPbouiCOM.EventFilter

    ''Private Sub SetApplication()


    ''    '*******************************************************************
    ''    '// Use an SboGuiApi object to establish connection
    ''    '// with the SAP Business One application and return an
    ''    '// initialized appliction object
    ''    '*******************************************************************

    ''    Dim SboGuiApi As SAPbouiCOM.SboGuiApi
    ''    Dim sConnectionString As String

    ''    Try
    ''        SboGuiApi = New SAPbouiCOM.SboGuiApi

    ''        '// by following the steps specified above, the following
    ''        '// statment should be suficient for either development or run mode

    ''        sConnectionString = Utils.ConnectionString  'Environment.GetCommandLineArgs.GetValue(1)

    ''        '// connect to a running SBO Application

    ''        SboGuiApi.Connect(sConnectionString)

    ''        '// get an initialized application object

    ''        SBO_Application = SboGuiApi.GetApplication(-1)

    ''    Catch ex As Exception
    ''        MsgBox(ex.Message, MsgBoxStyle.Critical, "Ocurrio un error")
    ''    End Try

    ''End Sub

    'Private Sub SetApplication()

    '    ' *******************************************************************
    '    '  Use an SboGuiApi object to establish connection
    '    '  with the SAP Business One application and return an
    '    '  initialized appliction object
    '    ' *******************************************************************

    '    Dim SboGuiApi As SAPbouiCOM.SboGuiApi = Nothing
    '    Dim sConnectionString As String = Nothing

    '    SboGuiApi = New SAPbouiCOM.SboGuiApi()

    '    '  by following the steps specified above, the following
    '    '  statment should be suficient for either development or run mode
    '    Try
    '        sConnectionString = Utils.ConnectionString
    '    Catch
    '        System.Windows.Forms.MessageBox.Show("AddOn must start in SAP Business One")
    '        System.Environment.[Exit](0)
    '    End Try

    '    '  connect to a running SBO Application        
    '    Try
    '        ' If there's no active application the connection will fail
    '        SboGuiApi.Connect(sConnectionString)
    '    Catch
    '        '  Connection failed
    '        System.Windows.Forms.MessageBox.Show("No SAP Business One Application was found")
    '        System.Environment.[Exit](0)
    '    End Try

    '    '  get an initialized application object

    '    SBO_Application = SboGuiApi.GetApplication()

    'End Sub

    Public Sub New()
        MyBase.New()

        Dim oTab As SAPbouiCOM.Folder
        Dim oItem As SAPbouiCOM.Item
        Dim oCombo As SAPbouiCOM.ComboBox
        Dim lblVer As SAPbouiCOM.StaticText

        Try
            '//*************************************************************
            '// set SBO_Application with an initialized application object
            '//*************************************************************
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

            If Utils.ActivateFormIsOpen(SBO_Application, "SBOParametrosFACE") = False Then

                LoadFromXML(XmlForm)

                '// Get the added form object by using the form's UID
                oForm = SBO_Application.Forms.Item("SBOParametrosFACE")

                ' SetFilters()

                '// Show the loaded Form
                oForm.Visible = True
                oForm.PaneLevel = 1


                oItem = oForm.Items.Item("cmbSAP")
                oCombo = oItem.Specific
                oCombo.ValidValues.Add("SI", "")
                oCombo.ValidValues.Add("NO", "")

                oItem = oForm.Items.Item("cmbPrint")
                oCombo = oItem.Specific
                oCombo.ValidValues.Add("0", "NO")
                oCombo.ValidValues.Add("1", "SI")

                lblVer = oForm.Items.Item("lblVersion").Specific
                lblVer.Caption = "Version " & Application.ProductVersion
                LlenaGrid()
                oForm.Freeze(True)
                LLenaParametros()
                oForm.Freeze(False)
                oItem = oForm.Items.Item("tabGen")
                oTab = oItem.Specific
                oTab.Select()

            Else
                oForm = SBO_Application.Forms.Item("SBOParametrosFACE")
            End If
        Catch ex As Exception
            oForm.Freeze(False)
            SBO_Application.MessageBox(ex.Message)
        End Try
    End Sub

    Private Sub SetFilters()

        '// Create a new EventFilters object
        oFilters = New SAPbouiCOM.EventFilters()

        '// add an event type to the container
        '// this method returns an EventFilter object
        oFilter = oFilters.Add(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED)

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

    'Private Function SetConnectionContext() As Integer

    '    Dim sCookie As String
    '    Dim sConnectionContext As String
    '    Dim lRetCode As Integer

    '    '// First initialize the Company object

    '    oCompany = New SAPbobsCOM.Company

    '    '// Acquire the connection context cookie from the DI API.
    '    sCookie = oCompany.GetContextCookie

    '    '// Retrieve the connection context string from the UI API using the
    '    '// acquired cookie.
    '    sConnectionContext = SBO_Application.Company.GetConnectionContext(sCookie)

    '    '// before setting the SBO Login Context make sure the company is not
    '    '// connected

    '    If oCompany.Connected = True Then
    '        oCompany.Disconnect()
    '    End If

    '    '// Set the connection context information to the DI API.
    '    SetConnectionContext = oCompany.SetSboLoginContext(sConnectionContext)

    'End Function

    'Private Function ConnectToCompany() As Integer

    '    '// Make sure you're not already connected.
    '    If oCompany.Connected = True Then
    '        oCompany.Disconnect()
    '    End If

    '    '// Establish the connection to the company database.
    '    ConnectToCompany = oCompany.Connect

    'End Function
   

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

    Private oCompany As SAPbobsCOM.Company

    Private Sub SBO_Application_ItemEvent(ByVal FormUID As String, ByRef pVal As SAPbouiCOM.ItemEvent, ByRef BubbleEvent As Boolean) Handles SBO_Application.ItemEvent
        Try

            If pVal.FormType = 60006 And (pVal.EventType = SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED) And (pVal.Before_Action = False) Then
                Select Case pVal.ItemUID
                    Case Is = "tabGen"
                        oForm.PaneLevel = 1
                    Case Is = "tabSeries"
                        oForm.PaneLevel = 2
                    Case Is = "tabCNN"
                        oForm.PaneLevel = 3
                    Case Is = "tabFACE"
                        oForm.PaneLevel = 4
                    Case Is = "tabEMP"
                        oForm.PaneLevel = 5
                End Select
            End If
            If pVal.ItemUID = "cmdOk" And pVal.FormType = 60006 And pVal.EventType = SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED And pVal.Before_Action = True Then
                Me.GuardarParametros()
                BubbleEvent = False
            End If
            'If pVal.ItemUID = "cmdCancel" And pVal.FormType = 60006 And pVal.EventType = SAPbouiCOM.BoEventTypes.et_CLICK And pVal.Before_Action = True Then
            '    oForm.Close()
            '    BubbleEvent = False
            'End If

            If pVal.ItemUID = "cmbSAP" And pVal.FormType = 60006 And pVal.EventType = SAPbouiCOM.BoEventTypes.et_COMBO_SELECT And pVal.Before_Action = False Then
                Dim cmb As SAPbouiCOM.ComboBox
                Dim txt As SAPbouiCOM.EditText
                cmb = oForm.Items.Item("cmbSAP").Specific
                txt = oForm.Items.Item("txtPrefix").Specific
                If cmb.Value = "SI" Then
                    txt.Value = ""
                    oForm.Items.Item("txtPrefix").Enabled = True
                Else
                    txt.Value = ""
                    Try
                        oForm.Items.Item("txtPrefix").Enabled = False
                    Catch ex As Exception
                        Dim oItem As SAPbouiCOM.Item = oForm.Items.Item("txtPathXML")
                        oItem.Click(SAPbouiCOM.BoCellClickType.ct_Regular)
                        oForm.Items.Item("txtPrefix").Enabled = False
                    End Try

                End If
            End If

            If pVal.EventType = SAPbouiCOM.BoEventTypes.et_FORM_CLOSE And pVal.BeforeAction = True And pVal.FormType = 60006 Then
                oForm = Nothing
                oCompany = Nothing
                SBO_Application = Nothing
            End If

        Catch ex As Exception
            SBO_Application.MessageBox(ex.Message)
        End Try
    End Sub

    Private Sub GuardarParametros()
        Dim oUsrTbl As SAPbobsCOM.UserTable
        Dim Res As Integer
        Dim oChk As SAPbouiCOM.CheckBox
        Dim oEdit As SAPbouiCOM.EditText
        Dim oItem As SAPbouiCOM.Item
        Dim oTab As SAPbouiCOM.Folder
        Dim oCmb As SAPbouiCOM.ComboBox
        Dim ProgressBar As SAPbouiCOM.ProgressBar

        Try

            ProgressBar = Me.SBO_Application.StatusBar.CreateProgressBar("Guardando parámetros por favor espere...", 18, False)

            oUsrTbl = oCompany.UserTables.Item("FACE_PARAMETROS")

            'Guarda el valor del XMLPath
            oItem = oForm.Items.Item("txtPathXML")
            oEdit = oItem.Specific
            If oEdit.Value.Trim = "" Then
                oForm.PaneLevel = 1
                oEdit.Active = True
                oItem = oForm.Items.Item("tabGen")
                oTab = oItem.Specific
                oTab.Select()
                Throw New Exception("Debe de Ingresar el Path del XML")
            End If
            Me.GuardaParametro(oUsrTbl, "PATHXML", oEdit.Value)
            ProgressBar.Value += 1

            'Guarda el valor del PDFPath
            oItem = oForm.Items.Item("txtPathPDF")
            oEdit = oItem.Specific
            If oEdit.Value.Trim = "" Then
                oForm.PaneLevel = 1
                oEdit.Active = True
                oItem = oForm.Items.Item("tabGen")
                oTab = oItem.Specific
                oTab.Select()
                Throw New Exception("Debe de Ingresar el Path del PDF")
            End If
            Me.GuardaParametro(oUsrTbl, "PATHPDF", oEdit.Value)
            ProgressBar.Value += 1

            oItem = oForm.Items.Item("txtEmailF")
            oEdit = oItem.Specific
            If oEdit.Value.Trim = "" Then
                oForm.PaneLevel = 1
                oEdit.Active = True
                oItem = oForm.Items.Item("tabGen")
                oTab = oItem.Specific
                oTab.Select()
                Throw New Exception("Debe de ingresa el Email From")
            End If
            Me.GuardaParametro(oUsrTbl, "EMAILF", oEdit.Value)
            ProgressBar.Value += 1

            oItem = oForm.Items.Item("cmbSAP")
            oCmb = oItem.Specific
            If oCmb.Value.Trim = "" Then
                oForm.PaneLevel = 1
                oEdit.Active = True
                oItem = oForm.Items.Item("tabGen")
                oTab = oItem.Specific
                oTab.Select()
                Throw New Exception("Debe de indicar si SAP manejara el correlativo de los documentos")
            End If
            Me.GuardaParametro(oUsrTbl, "ASS", oCmb.Value)
            ProgressBar.Value += 1

            oItem = oForm.Items.Item("txtPrefix")
            oEdit = oItem.Specific
            If oCmb.Value = "SI" Then
                If oEdit.Value.Trim = "" Then
                    oForm.PaneLevel = 1
                    oEdit.Active = True
                    oItem = oForm.Items.Item("tabGen")
                    oTab = oItem.Specific
                    oTab.Select()
                    Throw New Exception("Debe de ingresa la cantidad a digitos a suprimir en el correlativo de documentos, si no desea suprimir ninguno coloque el valor de 0")
                End If
                If Not IsNumeric(oEdit.Value) Then
                    Throw New Exception("El dato ingresado en la cantidad a digitos a suprimir en el correlativo de documentos es invalido")
                End If
            Else
                oEdit.Value = ""
            End If
            Me.GuardaParametro(oUsrTbl, "PREFIX", oEdit.Value)
            ProgressBar.Value += 1

            oItem = oForm.Items.Item("cmbPrint")
            oCmb = oItem.Specific
            If oCmb.Value.Trim = "" Then
                oForm.PaneLevel = 1
                oEdit.Active = True
                oItem = oForm.Items.Item("tabGen")
                oTab = oItem.Specific
                oTab.Select()
                Throw New Exception("Debe de indicar si se enviará el documento antes de la impresión")
            End If
            Me.GuardaParametro(oUsrTbl, "PRINTB", oCmb.Value)
            ProgressBar.Value += 1

            'Guarda el valor del Usuario de la DB
            oItem = oForm.Items.Item("txtUsuario")
            oEdit = oItem.Specific
            If oEdit.Value.Trim = "" Then
                oForm.PaneLevel = 3
                oEdit.Active = True
                oItem = oForm.Items.Item("tabCNN")
                oTab = oItem.Specific
                oTab.Select()
                Throw New Exception("Debe de Ingresar el usuario de la base de datos")
            End If
            Me.GuardaParametro(oUsrTbl, "USRDB", oEdit.Value)
            ProgressBar.Value += 1

            'Guarda el valor del Password de la DB
            oItem = oForm.Items.Item("txtPass")
            oEdit = oItem.Specific
            If oEdit.Value.Trim = "" Then
                oForm.PaneLevel = 3
                oEdit.Active = True
                oItem = oForm.Items.Item("tabCNN")
                oTab = oItem.Specific
                oTab.Select()
                Throw New Exception("Debe de Ingresar el password de la base de datos")
            End If
            Me.GuardaParametro(oUsrTbl, "PASSDB", Utils.Encriptar(oEdit.Value).ToString)
            ProgressBar.Value += 1

            'Guarda el valor del URL Webservice
            oItem = oForm.Items.Item("txtURLWS")
            oEdit = oItem.Specific
            If oEdit.Value.Trim = "" Then
                oForm.PaneLevel = 4
                oEdit.Active = True
                oItem = oForm.Items.Item("tabFACE")
                oTab = oItem.Specific
                oTab.Select()

                Throw New Exception("Debe de Ingresar la URL del WebService a utilizar para la autorización de la factura electrónica")
            End If
        
            'If CheckURL(oEdit.Value) = False Then
            '    oForm.PaneLevel = 4
            '    oEdit.Active = True
            '    oItem = oForm.Items.Item("tabFACE")
            '    oTab = oItem.Specific
            '    oTab.Select()
            '    Throw New Exception("La URL del Webservice no es valida o no existe")
            'End If
            Me.GuardaParametro(oUsrTbl, "URLWS", oEdit.Value)
            ProgressBar.Value += 1


            'Guarda el valor del Password de la DB
            oItem = oForm.Items.Item("txtIFACE")
            oEdit = oItem.Specific
            If oEdit.Value.Trim = "" Then
                oForm.PaneLevel = 4
                oEdit.Active = True
                oItem = oForm.Items.Item("tabFACE")
                oTab = oItem.Specific
                oTab.Select()
                Throw New Exception("Debe de Ingresar el número de identificador de la factura electrónica")
            End If
            Me.GuardaParametro(oUsrTbl, "IFACE", oEdit.Value)
            ProgressBar.Value += 1

            oItem = oForm.Items.Item("txtENT")
            oEdit = oItem.Specific
            If oEdit.Value.Trim = "" Then
                oForm.PaneLevel = 4
                oEdit.Active = True
                oItem = oForm.Items.Item("tabFACE")
                oTab = oItem.Specific
                oTab.Select()
                Throw New Exception("Debe de Ingresar la entidad de la factura electrónica")
            End If
            Me.GuardaParametro(oUsrTbl, "IENT", oEdit.Value)
            ProgressBar.Value += 1


            oItem = oForm.Items.Item("txtUSR")
            oEdit = oItem.Specific
            If oEdit.Value.Trim = "" Then
                oForm.PaneLevel = 4
                oEdit.Active = True
                oItem = oForm.Items.Item("tabFACE")
                oTab = oItem.Specific
                oTab.Select()
                Throw New Exception("Debe de Ingresar el usuario de la factura electrónica")
            End If
            Me.GuardaParametro(oUsrTbl, "IUSR", oEdit.Value)
            ProgressBar.Value += 1


            oItem = oForm.Items.Item("txtNUSR")
            oEdit = oItem.Specific
            If oEdit.Value.Trim = "" Then
                oForm.PaneLevel = 4
                oEdit.Active = True
                oItem = oForm.Items.Item("tabFACE")
                oTab = oItem.Specific
                oTab.Select()
                Throw New Exception("Debe de Ingresar el nombre de usuario de la factura electrónica")
            End If
            Me.GuardaParametro(oUsrTbl, "IUSRN", oEdit.Value)
            ProgressBar.Value += 1


            oItem = oForm.Items.Item("txtNit")
            oEdit = oItem.Specific
            If oEdit.Value.Trim = "" Then
                oForm.PaneLevel = 5
                oEdit.Active = True
                oItem = oForm.Items.Item("tabEMP")
                oTab = oItem.Specific
                oTab.Select()
                Throw New Exception("Debe de Ingresar el Nit de la empresa")
            End If
            Me.GuardaParametro(oUsrTbl, "NIT", oEdit.Value)
            ProgressBar.Value += 1

            oItem = oForm.Items.Item("txtNombreC")
            oEdit = oItem.Specific
            If oEdit.Value.Trim = "" Then
                oForm.PaneLevel = 5
                oEdit.Active = True
                oItem = oForm.Items.Item("tabEMP")
                oTab = oItem.Specific
                oTab.Select()
                Throw New Exception("Debe de Ingresar el nombre comercial la empresa")
            End If
            Me.GuardaParametro(oUsrTbl, "NOMC", oEdit.Value)
            ProgressBar.Value += 1

          


            oItem = oForm.Items.Item("txtdirec")
            oEdit = oItem.Specific
            If oEdit.Value.Trim = "" Then
                oForm.PaneLevel = 5
                oEdit.Active = True
                oItem = oForm.Items.Item("tabEMP")
                oTab = oItem.Specific
                oTab.Select()
                Throw New Exception("Debe de Ingresar la direccion de la empresa")
            End If
            Me.GuardaParametro(oUsrTbl, "DIRE", oEdit.Value)
            ProgressBar.Value += 1


            GuardaDatosSeries()
            ProgressBar.Value += 1

            ProgressBar.Stop()
            System.Runtime.InteropServices.Marshal.ReleaseComObject(ProgressBar)
            ProgressBar = Nothing
            GC.Collect()

            SBO_Application.SetStatusBarMessage("Parámetros guardados exítosamente", SAPbouiCOM.BoMessageTime.bmt_Short, False)
        Catch ex As Exception
            ProgressBar.Stop()
            System.Runtime.InteropServices.Marshal.ReleaseComObject(ProgressBar)
            ProgressBar = Nothing
            GC.Collect()
            SBO_Application.MessageBox(ex.Message)
        End Try
    End Sub

    Private Sub GuardaDatosSeries()
        Dim esFACe As String
        Dim serie As Integer
        Dim Autorizacion As String
        Dim Resolucion As String
        Dim FechaRes As String
        Dim Del As String
        Dim Al As String
        Dim oGrid As SAPbouiCOM.Grid
        Dim oItem As SAPbouiCOM.Item
        Dim Sql As String
        Dim RecSet As SAPbobsCOM.Recordset
        Dim colunnCheck As SAPbouiCOM.CheckBoxColumn
        Dim QryStr As String
        Dim TipoDoc As String
        Dim EsBatch As String
        Dim Sucursal As String
        Dim Dispositivo As String
        Dim nomSucursal As String
        Dim DirSucursal As String
        Dim MuniSucursal As String
        Dim DeptoSucursal As String
        Dim Usuario As String
        Dim Clave As String
        Try

            oItem = oForm.Items.Item("grdDatos")
            oGrid = oItem.Specific

            For i = 0 To oGrid.Rows.Count - 1
                If oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(3).Name, i) = "Y" Then
                    serie = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(0).Name, i)
                    Resolucion = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(4).Name, i)
                    Autorizacion = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(5).Name, i)
                    FechaRes = Format(oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(6).Name, i), "yyyyMMdd")
                    Del = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(7).Name, i)
                    Al = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(8).Name, i)
                    esFACe = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(3).Name, i)
                    TipoDoc = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(9).Name, i)
                    EsBatch = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(10).Name, i)
                    Sucursal = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(11).Name, i)
                    Dispositivo = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(13).Name, i)
                    nomSucursal = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(12).Name, i)
                    DirSucursal = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(14).Name, i)
                    MuniSucursal = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(15).Name, i)
                    DeptoSucursal = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(16).Name, i)
                    Usuario = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(17).Name, i)
                    Clave = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(18).Name, i)

                    If esFACe = "Y" Then
                        If TipoDoc = "" Then
                            Throw New Exception("El tipo de documento de la serie debe ser definido")
                        End If
                        If Sucursal = "" Then
                            Throw New Exception("El codigo de sucursal debe ser definido")
                        End If
                        If nomSucursal = "" Then
                            Throw New Exception("El nombre de la sucursal debe ser definido")
                        End If
                        If Dispositivo = "" Then
                            Throw New Exception("El codigo de dispositivo debe ser definido")
                        End If
                    End If
                End If
            Next

            RecSet = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            'QryStr = "delete  [@FACE_RESOLUCION]"
            QryStr = ("CALL SP_FACE_QUERYS('7','','')")
            RecSet.DoQuery(QryStr)

            For i = 0 To oGrid.Rows.Count - 1
                If oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(3).Name, i) = "Y" Then
                    serie = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(0).Name, i)
                    Resolucion = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(4).Name, i)
                    Autorizacion = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(5).Name, i)
                    FechaRes = Format(oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(6).Name, i), "yyyyMMdd")
                    Del = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(7).Name, i)
                    Al = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(8).Name, i)
                    esFACe = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(3).Name, i)
                    TipoDoc = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(9).Name, i)
                    If oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(10).Name, i) = "0" Or oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(10).Name, i) = "N" Then
                        EsBatch = "null"
                    Else
                        EsBatch = "'" & oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(10).Name, i) & "'"
                    End If
                    Sucursal = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(11).Name, i)
                    Dispositivo = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(13).Name, i)
                    nomSucursal = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(12).Name, i)
                    DirSucursal = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(14).Name, i)
                    MuniSucursal = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(15).Name, i)
                    DeptoSucursal = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(16).Name, i)
                    Usuario = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(17).Name, i)
                    Clave = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(18).Name, i)
                    ''If esFACe = "Y" Then
                    'Sql = "select * from [@FACE_RESOLUCION] where U_SERIE='" & serie & "'"
                    'RecSet = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                    'RecSet.DoQuery(Sql)
                    'If RecSet.RecordCount = 0 Then
                    'Sql = "insert into [@FACE_RESOLUCION] (Code,LineId,Object,LogInst,U_SERIE,U_RESOLUCION,U_AUTORIZACION,U_FECHA_AUTORIZACION,U_FACTURA_DEL,U_FACTURA_AL,U_TIPO_DOC,U_ES_BATCH,U_SUCURSAL,U_DISPOSITIVO,U_NOMBRE_SUCURSAL,U_DIR_SUCURSAL,U_MUNI_SUCURSAL,U_DEPTO_SUCURSAL,U_USUARIO,U_CLAVE) " & _
                    '      "values ('" & serie & "'," & serie & ",null,null,'" & serie & "','" & Resolucion & "','" & Autorizacion & "','" & FechaRes & "'," & Del & "," & Al & ",'" & TipoDoc & "'," & EsBatch & ",'" & Sucursal & "','" & Dispositivo & "','" & nomSucursal & "','" & DirSucursal & "','" & MuniSucursal & "','" & DeptoSucursal & "','" & Usuario & "','" & Clave & "')"
                    Sql = ("CALL SP_FACE_QUERYS_GUARDADATOSSERIE")
                    RecSet = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                    RecSet.DoQuery(Sql)
                    'Else
                    '    Sql = "update [@FACE_RESOLUCION] set " & _
                    '          "U_RESOLUCION='" & Resolucion & "',U_AUTORIZACION='" & Autorizacion & "',U_FECHA_AUTORIZACION='" & FechaRes & "',U_FACTURA_DEL=" & Del & ",U_FACTURA_AL=" & Al
                    '    RecSet = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                    'End If
                End If
            Next
        Catch ex As Exception
            Throw New Exception(ex.Message)
        End Try
    End Sub

    Private Sub LLenaParametros()
        Dim RecSet As SAPbobsCOM.Recordset
        Dim QryStr As String
        Dim RecCount As Long
        Dim RecIndex As Long
        Dim oEdit As SAPbouiCOM.EditText
        Dim oCmb As SAPbouiCOM.ComboBox
        Dim oItem As SAPbouiCOM.Item
        Dim Valor As String = ""
        Try

            RecSet = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            ' QryStr = "Select * from [@FACE_PARAMETROS]"
            QryStr = ("CALL SP_FACE_QUERYS('5','','')")
            RecSet.DoQuery(QryStr)
            RecCount = RecSet.RecordCount
            RecSet.MoveFirst()

            For RecIndex = 0 To RecCount - 1
                Select Case RecSet.Fields.Item("U_PARAMETRO").Value
                    Case Is = "PATHPDF"
                        oItem = oForm.Items.Item("txtPathPDF")
                        Valor = RecSet.Fields.Item("U_VALOR").Value
                    Case Is = "PATHXML"
                        oItem = oForm.Items.Item("txtPathXML")
                        Valor = RecSet.Fields.Item("U_VALOR").Value
                    Case Is = "URLWS"
                        oItem = oForm.Items.Item("txtURLWS")
                        Valor = RecSet.Fields.Item("U_VALOR").Value
                    Case Is = "USRDB"
                        oItem = oForm.Items.Item("txtUsuario")
                        Valor = RecSet.Fields.Item("U_VALOR").Value
                    Case Is = "PASSDB"
                        oItem = oForm.Items.Item("txtPass")
                        Valor = Utils.Desencriptar(RecSet.Fields.Item("U_VALOR").Value)
                    Case Is = "IENT"
                        oItem = oForm.Items.Item("txtENT")
                        Valor = RecSet.Fields.Item("U_VALOR").Value
                    Case Is = "IFACE"
                        oItem = oForm.Items.Item("txtIFACE")
                        Valor = RecSet.Fields.Item("U_VALOR").Value
                    Case Is = "IUSR"
                        oItem = oForm.Items.Item("txtUSR")
                        Valor = RecSet.Fields.Item("U_VALOR").Value
                    Case Is = "IUSRN"
                        oItem = oForm.Items.Item("txtNUSR")
                        Valor = RecSet.Fields.Item("U_VALOR").Value
                    Case Is = "EMAILF"
                        oItem = oForm.Items.Item("txtEmailF")
                        Valor = RecSet.Fields.Item("U_VALOR").Value
                    Case Is = "DIRE"
                        oItem = oForm.Items.Item("txtdirec")
                        Valor = RecSet.Fields.Item("U_VALOR").Value
                    Case Is = "NIT"
                        oItem = oForm.Items.Item("txtNit")
                        Valor = RecSet.Fields.Item("U_VALOR").Value
                    Case Is = "NOMC"
                        oItem = oForm.Items.Item("txtNombreC")
                        Valor = RecSet.Fields.Item("U_VALOR").Value
                    Case Is = "ASS"
                        oItem = oForm.Items.Item("cmbSAP")
                        Valor = RecSet.Fields.Item("U_VALOR").Value
                    Case Is = "PREFIX"
                        oItem = oForm.Items.Item("txtPrefix")
                        Valor = RecSet.Fields.Item("U_VALOR").Value
                    Case Is = "PRINTB"
                        oItem = oForm.Items.Item("cmbPrint")
                        Valor = IIf(RecSet.Fields.Item("U_VALOR").Value = "", "0", RecSet.Fields.Item("U_VALOR").Value)

                End Select
                If Not IsNothing(oItem) Then
                    If oItem.Type = SAPbouiCOM.BoFormItemTypes.it_COMBO_BOX Then
                        oCmb = oItem.Specific
                        oCmb.Select(Valor, SAPbouiCOM.BoSearchKey.psk_ByValue)
                    Else
                        oEdit = oItem.Specific
                        oEdit.Value = Valor
                    End If
                End If
                RecSet.MoveNext()
            Next RecIndex

            System.Runtime.InteropServices.Marshal.ReleaseComObject(RecSet)
            RecSet = Nothing
            GC.Collect()

        Catch ex As Exception
            SBO_Application.MessageBox(ex.Message)
        End Try
    End Sub


    Private Sub LlenaGrid()
        Dim QryStr As String
        Dim oGrid As SAPbouiCOM.Grid
        Dim oItem As SAPbouiCOM.Item
        Dim chkcol As SAPbouiCOM.CheckBoxColumn

        Try

            QryStr = ("CALL SP_FACE_LLENAGRID")
            'QryStr = "select  a.Series,a.SeriesName," & _
            '         "Case objectcode WHEN 13 THEN CASE DocSubType WHEN 'DN' THEN 'Nota de Debito' ELSE 'Factura' END ELSE CASE objectcode WHEN 14 THEN 'Nota de Credito' ELSE 'Factura Proveedor' end End 'Tipo Serie'," & _
            '         "'Es documento electrónico' = Case isnull(b.U_SERIE, '100') WHEN '100' THEN '0' ELSE 'Y' End," & _
            '         "U_RESOLUCION Resolucion,U_AUTORIZACION Autorizacion,U_FECHA_AUTORIZACION Fecha, U_FACTURA_DEL 'De la Factura', U_FACTURA_AL 'A la factura',  " & _
            '         "b.U_TIPO_DOC 'Tipo Documento','Es batch' = Case isnull(b.U_ES_BATCH, '100') WHEN '100' THEN '0' ELSE 'Y' End ,b.U_SUCURSAL '# Sucursal',b.U_NOMBRE_SUCURSAL 'Nombre Sucursal', b.U_DISPOSITIVO '# Dispositivo',    " & _
            '         "b.U_DIR_SUCURSAL 'Direccion Sucursal',b.U_MUNI_SUCURSAL Municipio, b.U_DEPTO_SUCURSAL Departamento,b.U_USUARIO 'Usuario GFACE', b.U_CLAVE 'Clave GFACE' " & _
            '         "from NNM1 a left outer join [@FACE_RESOLUCION] b " & _
            '         "on  a.Series =b.U_SERIE " & _
            '         " where a.objectcode in ('13','14','18','2','4') " & _
            '         " order by a.objectcode,a.docsubtype "
            ''"where ObjectCode in(13,18) "
            oForm.DataSources.DataTables.Add("MyDataTable")

            oForm.DataSources.DataTables.Item(0).ExecuteQuery(QryStr)

            oItem = oForm.Items.Item("grdDatos")
            oGrid = oItem.Specific
            oGrid.DataTable = oForm.DataSources.DataTables.Item("MyDataTable")
            oGrid.Columns.Item(0).Editable = False
            oGrid.Columns.Item(1).Editable = False
            oGrid.Columns.Item(2).Editable = False
            oGrid.Columns.Item(3).Type = SAPbouiCOM.BoGridColumnType.gct_CheckBox
            oGrid.Columns.Item(2).Width = 100
            oGrid.Columns.Item(4).Width = 100
            oGrid.Columns.Item(5).Width = 100
            oGrid.Columns.Item(6).Width = 100
            oGrid.Columns.Item(7).Width = 100
            oGrid.Columns.Item(8).Width = 100
            oGrid.Columns.Item(9).Width = 100
            oGrid.Columns.Item(10).Width = 100
            oGrid.Columns.Item(11).Width = 100  
            oGrid.Columns.Item(14).Width = 100
            oGrid.Columns.Item(15).Width = 100
            oGrid.Columns.Item(16).Width = 100
            oGrid.Columns.Item(9).Type = SAPbouiCOM.BoGridColumnType.gct_ComboBox
            oGrid.Columns.Item(10).Type = SAPbouiCOM.BoGridColumnType.gct_CheckBox

            Dim oGridColumn As SAPbouiCOM.ComboBoxColumn = oGrid.Columns.Item(9)
            Dim RecSet As SAPbobsCOM.Recordset
            Dim Sql As String
            'Sql = "select u_codigo,u_descripcion from [@FACE_TIPODOC]"
            Sql = ("CALL SP_FACE_QUERYS('8','','')")
            RecSet = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            RecSet.DoQuery(Sql)
            oGridColumn.DisplayType = SAPbouiCOM.BoComboDisplayType.cdt_Description
            For index = 0 To RecSet.RecordCount - 1
                oGridColumn.ValidValues.Add(RecSet.Fields.Item("U_CODIGO").Value, RecSet.Fields.Item("U_DESCRIPCION").Value)
                RecSet.MoveNext()
            Next
        Catch ex As Exception
            SBO_Application.MessageBox(ex.Message)
        End Try
    End Sub

    Public Function CheckURL(ByVal HostAddress As String) As Boolean
        CheckURL = False

        Dim url As New System.Uri(HostAddress)
        Dim wRequest As System.Net.WebRequest
        wRequest = System.Net.WebRequest.Create(url)
        Dim wResponse As System.Net.WebResponse
        Try
            wResponse = wRequest.GetResponse()
            'Is the responding address the same as HostAddress to avoid false positive from an automatic redirect.
            If wResponse.ResponseUri.AbsoluteUri().ToString = HostAddress Then 'include query strings
                CheckURL = True
            End If
            wResponse.Close()
            wRequest = Nothing
        Catch ex As Exception
            wRequest = Nothing
            MsgBox(ex.ToString)
        End Try

        Return CheckURL
    End Function

    Private Function ExisteParametro(ByVal IDParametro As String) As Boolean
        Dim result As Boolean = False
        Dim RecSet As SAPbobsCOM.Recordset
        Dim QryStr As String
        Try
            RecSet = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            'QryStr = "select * from [@FACE_PARAMETROS] where U_PARAMETRO='" & IDParametro & "'"
            QryStr = ("CALL SP_FACE_QUERYS('10','" & IDParametro & "',''")
            RecSet.DoQuery(QryStr)
            If RecSet.RecordCount > 0 Then
                result = True
            End If
            System.Runtime.InteropServices.Marshal.ReleaseComObject(RecSet)
            RecSet = Nothing
            GC.Collect()
            Return result
        Catch ex As Exception
            SBO_Application.MessageBox(ex.Message)
        End Try
    End Function

    Private Sub GuardaParametro(ByVal oUsrTbl As SAPbobsCOM.UserTable, ByVal IDParametro As String, ByVal ValorParmetro As String)
        Dim Res As Integer
        Dim RecSet As SAPbobsCOM.Recordset
        Dim QryStr As String
        Try
            oUsrTbl.Code = IDParametro
            oUsrTbl.Name = "PARAM"
            oUsrTbl.UserFields.Fields.Item("U_PARAMETRO").Value = IDParametro
            oUsrTbl.UserFields.Fields.Item("U_VALOR").Value = ValorParmetro
            If ExisteParametro(IDParametro) = False Then
                Res = oUsrTbl.Add()
                If Res <> 0 Then
                    Throw New Exception("Hubo un error al intentar guardar los parametros")
                End If
            Else
                RecSet = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                'QryStr = "update [@FACE_PARAMETROS] set U_VALOR='" & ValorParmetro & "' where U_PARAMETRO='" & IDParametro & "'"
                QryStr = ("CALL SP_FACE_QUERYS('11','" & ValorParmetro & "','" & IDParametro & "'")
                RecSet.DoQuery(QryStr)
                System.Runtime.InteropServices.Marshal.ReleaseComObject(RecSet)
                RecSet = Nothing
                GC.Collect()
            End If
        Catch ex As Exception
            Throw New Exception(ex.Message)
        End Try
    End Sub

#End Region

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub
End Class
