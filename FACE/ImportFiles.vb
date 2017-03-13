Imports System.IO
Imports System.Data.SqlClient

Public Class ImportFiles

    Private WithEvents SBO_Application As SAPbouiCOM.Application
    Private oForm As SAPbouiCOM.Form
    Private oCompany As SAPbobsCOM.Company
    Private XmlForm As String = Replace(Application.StartupPath & "\ImportFiles.srf", "\\", "\")

    Dim txtFile As SAPbouiCOM.EditText
    Dim cmdOk As SAPbouiCOM.Button
    Dim cmdFile As SAPbouiCOM.Button
    Dim ContenidoOrigen As String = ""
    Dim txtServidor As SAPbouiCOM.EditText
    Dim txtDB As SAPbouiCOM.EditText
    Dim txtUsuario As SAPbouiCOM.EditText
    Dim txtClave As SAPbouiCOM.EditText

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

            LoadFromXML(XmlForm)

            '// Get the added form object by using the form's UID
            oForm = SBO_Application.Forms.Item("frmImport")

            InizializaObjetos()

        Catch ex As Exception
            SBO_Application.SetStatusBarMessage(ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short)
        End Try
    End Sub

    Private Sub InizializaObjetos()
        Try
            txtFile = oForm.Items.Item("11").Specific
            txtServidor = oForm.Items.Item("7").Specific
            txtDB = oForm.Items.Item("1000001").Specific
            txtUsuario = oForm.Items.Item("1000003").Specific
            txtClave = oForm.Items.Item("13").Specific
            cmdOk = oForm.Items.Item("9").Specific
            cmdFile = oForm.Items.Item("12").Specific
        Catch ex As Exception
            Throw New Exception(ex.Message)
        End Try
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
        If FormUID = "frmImport" Then
            If pVal.ItemUID = "12" And pVal.EventType = SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED And pVal.BeforeAction = False Then
                BuscarArchivo()
            End If
            If pVal.ItemUID = "9" And pVal.EventType = SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED And pVal.BeforeAction = False Then
                ProcesarArchivo()
            End If
        End If
    End Sub

    Private Sub BuscarArchivo()
        Try
            Dim ofd As New OpenFileDialog
            Dim nw As New NativeWindow

            nw.AssignHandle(System.Diagnostics.Process.GetProcessesByName("SAP Business One")(0).MainWindowHandle)
            ofd.ShowDialog(nw)

        Catch ex As Exception
            SBO_Application.SetStatusBarMessage(ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short)
        End Try
    End Sub

    Private Sub ProcesarArchivo()
        Try
            If Me.txtServidor.Value = "" Then
                Throw New Exception("Por favor ingresar el nombre del servidor de la base de datos")
            End If
            If Me.txtDB.Value = "" Then
                Throw New Exception("Por favor ingresar el nombre de la base de datos")
            End If
            If Me.txtUsuario.Value = "" Then
                Throw New Exception("Por favor ingresar el nombre del usuario de la base de datos")
            End If
            If Me.txtClave.Value = "" Then
                Throw New Exception("Por favor ingresar la clave de acceso de la base de datos")
            End If
            If Me.txtFile.Value = "" Then
                Throw New Exception("Debe de indicar la ubicacion y el nombre del archivo  a procesar")
            End If
            If System.IO.File.Exists(Me.txtFile.Value) = False Then
                Throw New Exception("El path o el nombre del archivo ingresado no existen")
            End If
            Application.UseWaitCursor = True
            ExecuteSqlQuery()
            SBO_Application.SetStatusBarMessage("Proceso finalizado", SAPbouiCOM.BoMessageTime.bmt_Short, False)
            Application.UseWaitCursor = False
        Catch ex As Exception
            Application.UseWaitCursor = False
            SBO_Application.SetStatusBarMessage(ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short)
        End Try
    End Sub

    Private Sub ExecuteSqlQuery()

        Try
            Dim sServer As SQLDMO.SQLServer = New SQLDMO.SQLServer

            sServer.Connect(Me.txtServidor.Value, Me.txtUsuario.Value, Me.txtClave.Value)

            Dim oDataBase As SQLDMO.Database

            Dim strSQL As String

            Dim f As System.IO.File

            Dim strR As System.IO.StreamReader

            strR = File.OpenText(Me.txtFile.Value)

            strSQL = Desencriptar(strR.ReadToEnd())

            sServer.Databases().Item(Me.txtDB.Value).ExecuteImmediate(strSQL)
        Catch ex As Exception
            Throw New Exception(ex.Message)
        End Try
    End Sub
End Class
