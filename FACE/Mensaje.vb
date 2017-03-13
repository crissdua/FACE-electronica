Public Class Mensaje

    Private WithEvents SBO_Application As SAPbouiCOM.Application
    Private oForm As SAPbouiCOM.Form
    Private oCompany As SAPbobsCOM.Company
    Private XmlForm As String = Replace(Application.StartupPath & "\Mensaje.srf", "\\", "\")

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

    Public Sub New(ByVal Mensaje As String)
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
            oForm = SBO_Application.Forms.Item("SBOFormEditor_11")

            Dim txtMensaje As SAPbouiCOM.EditText = oForm.Items.Item("txtMensaje").Specific
            txtMensaje.Value = Replace(Mensaje, """", "")

        Catch ex As Exception
            SBO_Application.MessageBox(ex.Message)
        End Try
    End Sub

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
End Class
