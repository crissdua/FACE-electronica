Imports System.IO
Imports System.Xml

Public Class SystemForm

#Region "INICIALIZA CONEXION CON SAP"

    Private WithEvents SBO_Application As SAPbouiCOM.Application
    Private oCompany As SAPbobsCOM.Company
    Private oFilters As SAPbouiCOM.EventFilters
    Private oFilter As SAPbouiCOM.EventFilter
    Private esFactura As Boolean
    Private IdForm As String
    Private IdItem As String
    Private IdEvent As Integer
    Private IdAction As Boolean = False
    Private Evento As New Accion

    Public Enum Accion
        Normal
        Duplicar
        CopiarNotaCredito
        CopiarNotaDebito
    End Enum
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

            SBO_Application = SboGuiApi.GetApplication()

        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical, "Ocurrio un error")
        End Try

    End Sub

    Public Sub New()
        MyBase.New()

        Try
            SetApplication()
            Dim result As Integer
            Dim lerrcode As Integer
            Dim serrmsg As String = ""

            If Not SetConnectionContext() = 0 Then
                SBO_Application.MessageBox("Failed setting a connection to DI API")
                End ' Terminating the Add-On Application
            End If

            result = ConnectToCompany()
            If Not result = 0 Then
                SBO_Application.MessageBox(result & " Failed connecting to the company's Data Base")
                End ' Terminating the Add-On Application
            End If

            SBO_Application.StatusBar.SetText("Iniciando add-on facturación electrónica..", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success)

            Utils.SBOApplication = Me.SBO_Application
            Utils.Company = Me.oCompany

            AddUserTables()

            AddMenuItems()

            Evento = Accion.Normal
        Catch ex As Exception
            System.Windows.Forms.MessageBox.Show(ex.Message & vbNewLine & "SBO application not found")
            System.Windows.Forms.Application.Exit()
        End Try

    End Sub

    Private Sub SetFilters()

        '// Create a new EventFilters object
        oFilters = New SAPbouiCOM.EventFilters()

        '// add an event type to the container
        '// this method returns an EventFilter object
        oFilter = oFilters.Add(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED)

        '// assign the form type on which the event would be processed
        oFilter.AddEx("133") 'Orders Form
        oFilter.AddEx("179") 'Orders Form
        oFilter.AddEx("65303") 'Orders Form
        oFilter.AddEx("141")

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

    Private Sub AddUserTables()
        Try
            Utils.AddUserTable(oCompany, "FACE_PARAMETROS", "PARAMETROS FACE", SAPbobsCOM.BoUTBTableType.bott_MasterDataLines)
            Utils.AddUserTable(oCompany, "FACE_RESOLUCION", "RES. FAC. FACE", SAPbobsCOM.BoUTBTableType.bott_MasterDataLines)
            Utils.AddUserTable(oCompany, "FACE_TIPODOC", "TIPOS DE DOC FACE", SAPbobsCOM.BoUTBTableType.bott_MasterDataLines)
            'Utils.AddUserTable(oCompany, "FACE_PAISEQUIV", "PAISES EQUIVALENCIAS GUATEF", SAPbobsCOM.BoUTBTableType.bott_MasterDataLines)


            'Utils.AddUserField(oCompany, "FACE_PAISEQUIV", "COD_PAIS_SAP", "CODIGO PAIS SAP", SAPbobsCOM.BoFieldTypes.db_Alpha, 15)
            'Utils.AddUserField(oCompany, "FACE_PAISEQUIV", "COD_PAIS_GUATEF", "CODIGO PAIS GUATEFACTURAS", SAPbobsCOM.BoFieldTypes.db_Alpha, 15)

            Utils.AddUserField(oCompany, "FACE_TIPODOC", "CODIGO", "CODIGO DOCUMENTO", SAPbobsCOM.BoFieldTypes.db_Alpha, 15)
            Utils.AddUserField(oCompany, "FACE_TIPODOC", "DESCRIPCION", "DESC TIPO DE DOC", SAPbobsCOM.BoFieldTypes.db_Alpha, 250)

            Utils.AddUserField(oCompany, "FACE_PARAMETROS", "PARAMETRO", "PARAMETRO FACE", SAPbobsCOM.BoFieldTypes.db_Alpha, 10)
            Utils.AddUserField(oCompany, "FACE_PARAMETROS", "VALOR", "VAL. PARAMETRO FACE", SAPbobsCOM.BoFieldTypes.db_Alpha, 254)

            Utils.AddUserField(oCompany, "FACE_RESOLUCION", "SERIE", "SERIE FACTURA", SAPbobsCOM.BoFieldTypes.db_Numeric, 11)
            Utils.AddUserField(oCompany, "FACE_RESOLUCION", "RESOLUCION", "RES. FACTURA", SAPbobsCOM.BoFieldTypes.db_Alpha, 50)
            Utils.AddUserField(oCompany, "FACE_RESOLUCION", "AUTORIZACION", "AUTO. FACTURA", SAPbobsCOM.BoFieldTypes.db_Alpha, 50)
            Utils.AddUserField(oCompany, "FACE_RESOLUCION", "FECHA_AUTORIZACION", "FECHA AUTO.", SAPbobsCOM.BoFieldTypes.db_Date, 8)
            Utils.AddUserField(oCompany, "FACE_RESOLUCION", "FACTURA_DEL", "FACTURA INICIAL", SAPbobsCOM.BoFieldTypes.db_Numeric, 11)
            Utils.AddUserField(oCompany, "FACE_RESOLUCION", "FACTURA_AL", "FACTURA FINAL", SAPbobsCOM.BoFieldTypes.db_Numeric, 11)
            Utils.AddUserField(oCompany, "FACE_RESOLUCION", "TIPO_DOC", "TIPO DE DOCUMENTO", SAPbobsCOM.BoFieldTypes.db_Alpha, 30)
            Utils.AddUserField(oCompany, "FACE_RESOLUCION", "ES_BATCH", "PROESO EN LINEA O BATCH", SAPbobsCOM.BoFieldTypes.db_Alpha, 1)
            Utils.AddUserField(oCompany, "FACE_RESOLUCION", "SUCURSAL", "NO. SUCURSAL", SAPbobsCOM.BoFieldTypes.db_Alpha, 10)
            Utils.AddUserField(oCompany, "FACE_RESOLUCION", "NOMBRE_SUCURSAL", "NOMBRE SUCURSAL", SAPbobsCOM.BoFieldTypes.db_Alpha, 100)
            Utils.AddUserField(oCompany, "FACE_RESOLUCION", "DISPOSITIVO", "No. DISPOSITIVO ELECTRONICO", SAPbobsCOM.BoFieldTypes.db_Alpha, 10)
            Utils.AddUserField(oCompany, "FACE_RESOLUCION", "DIR_SUCURSAL", "DIRECCION SUCURSAL", SAPbobsCOM.BoFieldTypes.db_Memo, 8000)
            Utils.AddUserField(oCompany, "FACE_RESOLUCION", "MUNI_SUCURSAL", "MUNICIPIO SUCURSAL", SAPbobsCOM.BoFieldTypes.db_Memo, 8000)
            Utils.AddUserField(oCompany, "FACE_RESOLUCION", "DEPTO_SUCURSAL", "DEPTO SUCURSAL", SAPbobsCOM.BoFieldTypes.db_Memo, 8000)
            Utils.AddUserField(oCompany, "FACE_RESOLUCION", "USUARIO", "USUARIO GFACE", SAPbobsCOM.BoFieldTypes.db_Memo, 8000)
            Utils.AddUserField(oCompany, "FACE_RESOLUCION", "CLAVE", "CLAVE GFACE", SAPbobsCOM.BoFieldTypes.db_Memo, 8000)

            Utils.AddUserField(oCompany, "OINV", "ESTADO_FACE", "ESTADO FACE", SAPbobsCOM.BoFieldTypes.db_Alpha, 10, False)
            Utils.AddUserField(oCompany, "OINV", "FACE_XML", "XML ENVIADO FACE", SAPbobsCOM.BoFieldTypes.db_Memo, 254, False)
            Utils.AddUserField(oCompany, "OINV", "MOTIVO_RECHAZO", "RECHAZO FACE", SAPbobsCOM.BoFieldTypes.db_Memo, 254, False)
            Utils.AddUserField(oCompany, "OINV", "FACE_PDFFILE", "PDF FACE", SAPbobsCOM.BoFieldTypes.db_Memo, 254, False)
            Utils.AddUserField(oCompany, "OINV", "FIRMA_ELETRONICA", "FIRMA ELECTRONICA FACE", SAPbobsCOM.BoFieldTypes.db_Memo, 254, False)
            Utils.AddUserField(oCompany, "OINV", "NUMERO_DOCUMENTO", "NUMERO DOC FACE", SAPbobsCOM.BoFieldTypes.db_Alpha, 150, False)
            Utils.AddUserField(oCompany, "OINV", "NUMERO_RESOLUCION", "NUMERO RESOLUCION FACE", SAPbobsCOM.BoFieldTypes.db_Alpha, 100, False)
            Utils.AddUserField(oCompany, "OINV", "SERIE_FACE", "NUMERO DE SERIE", SAPbobsCOM.BoFieldTypes.db_Alpha, 20, False)
            Utils.AddUserField(oCompany, "OINV", "FACTURA_INI", "FACTURA INICIAL", SAPbobsCOM.BoFieldTypes.db_Alpha, 20, False)
            Utils.AddUserField(oCompany, "OINV", "FACTURA_FIN", "FACTURA FINAL", SAPbobsCOM.BoFieldTypes.db_Alpha, 20, False)
            Utils.AddUserField(oCompany, "OINV", "FACTURA_SERIE", "SERIE FACE", SAPbobsCOM.BoFieldTypes.db_Alpha, 20, False)
            Utils.AddUserField(oCompany, "OINV", "FACTURA_PREIMPRESO", "PREIMPRESO FACE", SAPbobsCOM.BoFieldTypes.db_Alpha, 20, False)
            Utils.AddUserField(oCompany, "OINV", "FECHA_ENVIO_FACE", "FECHA ENVIO FACE", SAPbobsCOM.BoFieldTypes.db_Alpha, 22, False)
            Utils.AddUserField(oCompany, "OINV", "EMAIL_FACE", "EMAIL FACE", SAPbobsCOM.BoFieldTypes.db_Alpha, 100, False)


            Utils.AddUserField(oCompany, "ORIN", "ESTADO_FACE", "ESTADO FACE", SAPbobsCOM.BoFieldTypes.db_Alpha, 2, False)
            Utils.AddUserField(oCompany, "ORIN", "FACE_XML", "XML ENVIADO FACE", SAPbobsCOM.BoFieldTypes.db_Memo, 254, False)
            Utils.AddUserField(oCompany, "ORIN", "MOTIVO_RECHAZO", "RECHAZO FACE", SAPbobsCOM.BoFieldTypes.db_Memo, 254, False)
            Utils.AddUserField(oCompany, "ORIN", "FACE_PDFFILE", "PDF FACE", SAPbobsCOM.BoFieldTypes.db_Memo, 254, False)
            Utils.AddUserField(oCompany, "ORIN", "FIRMA_ELETRONICA", "FIRMA ELECTRONICA FACE", SAPbobsCOM.BoFieldTypes.db_Memo, 254, False)
            Utils.AddUserField(oCompany, "ORIN", "NUMERO_DOCUMENTO", "NUMERO DOC FACE", SAPbobsCOM.BoFieldTypes.db_Alpha, 150, False)
            Utils.AddUserField(oCompany, "ORIN", "NUMERO_RESOLUCION", "NUMERO RESOLUCION FACE", SAPbobsCOM.BoFieldTypes.db_Alpha, 100, False)
            Utils.AddUserField(oCompany, "ORIN", "SERIE_FACE", "NUMERO DE SERIE", SAPbobsCOM.BoFieldTypes.db_Alpha, 20, False)
            Utils.AddUserField(oCompany, "ORIN", "FACTURA_INI", "FACTURA INICIAL", SAPbobsCOM.BoFieldTypes.db_Alpha, 20, False)
            Utils.AddUserField(oCompany, "ORIN", "FACTURA_FIN", "FACTURA FINAL", SAPbobsCOM.BoFieldTypes.db_Alpha, 20, False)
            Utils.AddUserField(oCompany, "ORIN", "FACTURA_SERIE", "SERIE FACE", SAPbobsCOM.BoFieldTypes.db_Alpha, 20, False)
            Utils.AddUserField(oCompany, "ORIN", "FACTURA_PREIMPRESO", "PREIMPRESO FACE", SAPbobsCOM.BoFieldTypes.db_Alpha, 20, False)
            Utils.AddUserField(oCompany, "ORIN", "FECHA_ENVIO_FACE", "FECHA ENVIO FACE", SAPbobsCOM.BoFieldTypes.db_Alpha, 22, False)
            Utils.AddUserField(oCompany, "ORIN", "EMAIL_FACE", "EMAIL FACE", SAPbobsCOM.BoFieldTypes.db_Alpha, 100, False)
            Utils.AddUserField(oCompany, "ORIN", "DocstatusCC", "Estado Docto", SAPbobsCOM.BoFieldTypes.db_Alpha, 1, False)

            Dim RecSet As SAPbobsCOM.Recordset
            Dim sql As String = ""
            sql = "delete  [@FACE_TIPODOC]"
            RecSet = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            RecSet.DoQuery(sql)

            If Utils.TipoGFACE = TipoFACE.Documenta Or Utils.TipoGFACE = TipoFACE.GYT Then
                Utils.AddDocumentType(oCompany, "CFACE-1", "FACTURAS", 1)
                Utils.AddDocumentType(oCompany, "CNDE-4", "NOTAS DE DÉBITO", 2)
                Utils.AddDocumentType(oCompany, "CNCE-5", "NOTAS DE CRÉDITO", 3)
                Utils.AddDocumentType(oCompany, "CFACE-6", "FACTURA ESPECIAL", 4)
                Utils.AddDocumentType(oCompany, "CFACE-8", "FACTURAS CAMBIARIAS", 5)
                Utils.AddDocumentType(oCompany, "CFACE-30", "FACTURA POR MÁQUINA REGISTRADORA MECANIZADA", 6)
                Utils.AddDocumentType(oCompany, "CFACE-32", "FACTURA POR MÁQUINA REGISTRADORA COMPUTARIZADA", 7)
                Utils.AddDocumentType(oCompany, "CFACE-37", "FACTURA POR SISTEMA COMPUTARIZADO INTEGRADO  DE CONTABILIDAD", 8)
                Utils.AddDocumentType(oCompany, "CFACE-38", "FACTURA CAMBIARIA POR SISTEMA COMPUTARIZADO INTEGRADO DE CONTABILIDAD", 9)
                Utils.AddDocumentType(oCompany, "CNDE-39", "NOTA DE DÉBITO POR SISTEMA COMPUTARIZADO INTEGRADO DE CONTABILIDAD", 10)
                Utils.AddDocumentType(oCompany, "CNCE-40", "NOTA DE CRÉDITO POR SISTEMA COMPUTARIZADO INTEGRADO DE CONTABILIDAD", 11)
                Utils.AddDocumentType(oCompany, "CFACE-53", "FACTURA POR SISTEMA COMPUTARIZADO COMO MÁQUINA REGISTRADORA", 12)
                Utils.AddDocumentType(oCompany, "CNDE-55", "NOTA DE DÉBITO POR SISTEMA COMPUTARIZADO COMO MÁQUINA REGISTRADORA", 13)
                Utils.AddDocumentType(oCompany, "CNCE-56", "NOTA DE CRÉDITO POR SISTEMA COMPUTARIZADO COMO MÁQUINA REGISTRADORA", 14)
                Utils.AddDocumentType(oCompany, "CFACE-57", "FACTURA CAMBIARIA POR SISTEMA COMPUTARIZADO COMO MÁQUINA REGISTRADORA", 15)
                Utils.AddDocumentType(oCompany, "CRED-59", "RECIBOS PARA USO DE ONGS", 16)
                Utils.AddDocumentType(oCompany, "CFACE-60", "FACTURA PREIMPRESA PARA ESPECTÁCULOS PÚBLICOS", 17)
                Utils.AddDocumentType(oCompany, "CNCE-61", "NOTAS DE ABONO", 18)
                Utils.AddDocumentType(oCompany, "CFACE-62", "FACTURA COMERCIAL USUARIO ZONA FRANCA", 19)
                Utils.AddDocumentType(oCompany, "FACE-63", "FACTURA ELECTRÓNICA", 20)
                Utils.AddDocumentType(oCompany, "NCE-64", "NOTA DE CRÉDITO ELECTRÓNICA", 21)
                Utils.AddDocumentType(oCompany, "NDE-65", "NOTA DE DÉBITO ELECTRÓNICA", 22)
                Utils.AddDocumentType(oCompany, "FACE-66", "FACTURA CAMBIARIA ELECTRÓNICA", 23)
                Utils.AddDocumentType(oCompany, "FACE-67", "FACTURA COMERCIAL USUARIO ZONA FRANCA ELECTRÓNICA", 24)
                Utils.AddDocumentType(oCompany, "CFACE-68", "FACTURA COMERCIAL USUARIO ZONA FRANCA SISTEMA COMPUTARIZADO", 25)
                Utils.AddDocumentType(oCompany, "CFACE-69", "FACTURA COMERCIAL USUARIO ZONA FRANCA SISTEMA COMPUTARIZADO COMO MAQUINA REGISTRADORA", 26)
                Utils.AddDocumentType(oCompany, "CRED-70", "RECIBOS PARA USO DE PARTIDOS POLITICOS", 27)
                Utils.AddDocumentType(oCompany, "NCE-71", "NOTA DE ABONO ELECTRONICA", 28)
                Utils.AddDocumentType(oCompany, "FACE-72", "FACTURA COMERCIAL DE EXPORTACIÓN ELECTRÓNICA", 29)
                Utils.AddDocumentType(oCompany, "RED-73", "RECIBO EECTRÓNICO DE DONACIONES", 30)
                Utils.AddDocumentType(oCompany, "FACE-74", "FACTURA ESPECIAL ELECTRÓNICA", 31)
                Utils.AddDocumentType(oCompany, "CFACE1", "COPIA ELECTRÓNICA DE FACTURA", 32)
                Utils.AddDocumentType(oCompany, "CNDE4", "COPIA ELECTRÓNICA DE NOTA DE DEBITO", 33)
                Utils.AddDocumentType(oCompany, "CNCE5", "COPIA ELECTRÓNICA DE NOTA DE CREDITO", 34)
                Utils.AddDocumentType(oCompany, "CFACE6", "COPIA ELECTRÓNICA DE FACTURA ESPECIAL", 35)
                Utils.AddDocumentType(oCompany, "CFACE8", "COPIA ELECTRÓNICA DE FACTURA CAMBIARIA", 36)
                Utils.AddDocumentType(oCompany, "CFACE30", "COPIA ELECTRÓNICA DE FACTURA", 37)
                Utils.AddDocumentType(oCompany, "CFACE32", "COPIA ELECTRÓNICA DE FACTURA", 38)
                Utils.AddDocumentType(oCompany, "CFACE37", "COPIA ELECTRÓNICA DE FACTURA", 39)
                Utils.AddDocumentType(oCompany, "CFACE38", "COPIA ELECTRÓNICA DE FACTURA CAMBIARIA", 40)
                Utils.AddDocumentType(oCompany, "CNDE39", "COPIA ELECTRÓNICA DE NOTA DE DEBITO", 41)
                Utils.AddDocumentType(oCompany, "CNCE40", "COPIA ELECTRÓNICA DE NOTA DE CREDITO", 42)
                Utils.AddDocumentType(oCompany, "CFACE53", "COPIA ELECTRÓNICA DE FACTURA", 43)
                Utils.AddDocumentType(oCompany, "CNDE55", "COPIA ELECTRÓNICA DE NOTA DE DEBITO", 44)
                Utils.AddDocumentType(oCompany, "CNCE56", "COPIA ELECTRÓNICA DE NOTA DE CREDITO", 45)
                Utils.AddDocumentType(oCompany, "CFACE57", "COPIA ELECTRÓNICA DE FACTURA CAMBIARIA", 46)
                Utils.AddDocumentType(oCompany, "CRED59", "COPIA DE RECIBO PARA USO DE ONGS", 47)
                Utils.AddDocumentType(oCompany, "CFACE60", "FACTURA PREIMPRESA ESPECTÁCULOS PÚBLICOS", 48)
                Utils.AddDocumentType(oCompany, "CNCE61", "COPIA ELECTRÓNICA DE NOTA DE ABONO", 49)
                Utils.AddDocumentType(oCompany, "CFACE62", "COPIA ELECTRÓNICA DE FACTURA COMERCIAL USUARIO ZONA FRANCA", 50)
                Utils.AddDocumentType(oCompany, "FACE63", "FACTURA ELECTRONICA", 51)
                Utils.AddDocumentType(oCompany, "NCE64", "NOTA DE CRÉDITO ELECTRONICA", 52)
                Utils.AddDocumentType(oCompany, "NDE65", "NOTA DE DÉBITO ELECTRONICA", 53)
                Utils.AddDocumentType(oCompany, "FACE66", "FACTURA CAMBIARIA ELECTRÓNICA", 54)
                Utils.AddDocumentType(oCompany, "FACE67", "FACTURA COMERCIAL USUARIO ZONA FRANCA ELECTRÓNICA", 55)
                Utils.AddDocumentType(oCompany, "CFACE68", "COPIA ELECTRÓNICA FACTURA COMERCIAL USUARIO ZONA FRANCA SISTEMA COMPUTARIZADO", 56)
                Utils.AddDocumentType(oCompany, "CFACE69", "COPIA ELECTRÓNICA FACTURA COMERCIAL USUARIO ZONA FRANCA SISTEMA COMPUTARIZADO COMO MAQUINA REGISTRADORA", 57)
                Utils.AddDocumentType(oCompany, "CRED70", "COPIA ELECTRÓNICA RECIBOS PARA USO DE PARTIDOS POLITICOS", 58)
                Utils.AddDocumentType(oCompany, "NCE71", "NOTA DE ABONO ELECTRÓNICA", 59)
                Utils.AddDocumentType(oCompany, "FACE72", "FACTURA COMERCIAL DE EXPORTACIÓN ELECTRÓNICA", 60)
                Utils.AddDocumentType(oCompany, "RED73", "RECIBO ELECTRONICO DE DONACIONES", 61)
                Utils.AddDocumentType(oCompany, "FACE74", "FACTURA ESPECIAL ELECTRÓNICA", 62)
            ElseIf Utils.TipoGFACE = TipoFACE.GuateFacturas Then
                Utils.AddDocumentType(oCompany, "52", "FACTURA ELECTRONICA", 1)
                Utils.AddDocumentType(oCompany, "2", "NOTA DE CREDITO", 2)
                Utils.AddDocumentType(oCompany, "3", "NOTA DE DEBITO", 3)
                Utils.AddDocumentType(oCompany, "4", "COPIA FACTURA ELECTRONICA", 4)
                Utils.AddDocumentType(oCompany, "5", "COPIA NOTA DE CREDITO", 5)
                Utils.AddDocumentType(oCompany, "6", "COPIA NOTA DE DEBITO", 6)
            ElseIf Utils.TipoGFACE = TipoFACE.InFile Then
                Utils.AddDocumentType(oCompany, "1", "Facturas", 1, "CFACE-1")
                Utils.AddDocumentType(oCompany, "4", "Notas De Débito", 1, "CNDE-4")
                Utils.AddDocumentType(oCompany, "5", "Notas De Crédito", 1, "CNCE-5")
                Utils.AddDocumentType(oCompany, "6", "Factura Especial", 1, "CFACE-6")
                Utils.AddDocumentType(oCompany, "8", "Facturas Cambiarias", 1, "CFACE-8")
                Utils.AddDocumentType(oCompany, "30", "Factura Por Máquina Registradora Mecanizada", 1, "CFACE-30")
                Utils.AddDocumentType(oCompany, "32", "Factura Por Máquina Registradora Computarizada", 1, "CFACE-32")
                Utils.AddDocumentType(oCompany, "37", "Factura Por Sistema Computarizado Integrado  De Contabilidad", 1, "CFACE-37")
                Utils.AddDocumentType(oCompany, "38", "Factura Cambiaria Por Sistema Computarizado Integrado De Contabilidad", 1, "CFACE-38")
                Utils.AddDocumentType(oCompany, "39", "Nota De Débito Por Sistema Computarizado Integrado De Contabilidad", 1, "CNDE-39")
                Utils.AddDocumentType(oCompany, "40", "Nota De Crédito Por Sistema Computarizado Integrado De Contabilidad", 1, "CNCE-40")
                Utils.AddDocumentType(oCompany, "53", "Factura Por Sistema Computarizado Como Máquina Registradora", 1, "CFACE-53")
                Utils.AddDocumentType(oCompany, "55", "Nota De Débito Por Sistema Computarizado Como Máquina Registradora", 1, "CNDE-55")
                Utils.AddDocumentType(oCompany, "56", "Nota De Crédito Por Sistema Computarizado Como Máquina Registradora", 1, "CNCE-56")
                Utils.AddDocumentType(oCompany, "57", "Factura Cambiaria Por Sistema Computarizado Como Máquina Registradora", 1, "CFACE-57")
                Utils.AddDocumentType(oCompany, "59", "Recibos Para Uso De ONGs", 1, "CRED-59")
                Utils.AddDocumentType(oCompany, "60", "Factura Preimpresa Para Espectáculos Públicos", 1, "CFACE-60")
                Utils.AddDocumentType(oCompany, "61", "Notas De Abono", 1, "CNCE-61")
                Utils.AddDocumentType(oCompany, "62", "Factura Comercial Usuario Zona Franca", 1, "CFACE-62")
                Utils.AddDocumentType(oCompany, "63", "Factura Electrónica", 1, "FACE-63")
                Utils.AddDocumentType(oCompany, "64", "Nota De Crédito Electrónica", 1, "NCE-64")
                Utils.AddDocumentType(oCompany, "65", "Nota De Débito Electrónica", 1, "NDE-65")
                Utils.AddDocumentType(oCompany, "66", "Factura Cambiaria Electrónica", 1, "FACE-66")
                Utils.AddDocumentType(oCompany, "67", "Factura Comercial Usuario Zona Franca Electrónica", 1, "FACE-67")
                Utils.AddDocumentType(oCompany, "68", "Factura Comercial Usuario Zona Franca Sistema Computarizado", 1, "CFACE-68")
                Utils.AddDocumentType(oCompany, "69", "Factura Comercial Usuario Zona Franca Sistema Computarizado Como Maquina Registradora", 1, "CFACE-69")
                Utils.AddDocumentType(oCompany, "70", "Recibos Para Uso De Partidos Politicos", 1, "CRED-70")
                Utils.AddDocumentType(oCompany, "71", "Nota De Abono Electronica", 1, "NCE-71")
                Utils.AddDocumentType(oCompany, "72", "Factura Comercial de Exportación Electrónica ", 1, "FACE-72")
                Utils.AddDocumentType(oCompany, "73", "Recibo Eectrónico de Donaciones", 1, "RED-73")
                Utils.AddDocumentType(oCompany, "74", "Factura Especial Electrónica", 1, "FACE-74")
            End If

            sql = "select * from  [@FACE_PARAMETROS]"
            RecSet = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            RecSet.DoQuery(sql)
            If RecSet.RecordCount = 0 Then
                sql = "insert into [@FACE_PARAMETROS] values('ASS',0,-3,0,'ASS',null)"
                RecSet.DoQuery(sql)
                sql = "insert into [@FACE_PARAMETROS] values('CODE',1,-3,1,'CODE',null)"
                RecSet.DoQuery(sql)
                sql = "insert into [@FACE_PARAMETROS] values('DIE',2,-3,2,'DIE',null)"
                RecSet.DoQuery(sql)
                sql = "insert into [@FACE_PARAMETROS] values('DIRE',3,-3,3,'DIRE',null)"
                RecSet.DoQuery(sql)
                sql = "insert into [@FACE_PARAMETROS] values('EMAILF',4,-3,4,'EMAILF',null)"
                RecSet.DoQuery(sql)
                sql = "insert into [@FACE_PARAMETROS] values('IENT',5,-3,5,'IENT',null)"
                RecSet.DoQuery(sql)
                sql = "insert into [@FACE_PARAMETROS] values('IFACE',6,-3,6,'IFACE',null)"
                RecSet.DoQuery(sql)
                sql = "insert into [@FACE_PARAMETROS] values('IUSR',7,-3,7,'IUSR',null)"
                RecSet.DoQuery(sql)
                sql = "insert into [@FACE_PARAMETROS] values('IUSRN',8,-3,8,'IUSRN',null)"
                RecSet.DoQuery(sql)
                sql = "insert into [@FACE_PARAMETROS] values('NIT',9,-3,9,'NIT',null)"
                RecSet.DoQuery(sql)
                sql = "insert into [@FACE_PARAMETROS] values('NOMC',10,-3,10,'NOMC',null)"
                RecSet.DoQuery(sql)
                sql = "insert into [@FACE_PARAMETROS] values('NOME',11,-3,11,'NOME',null)"
                RecSet.DoQuery(sql)
                sql = "insert into [@FACE_PARAMETROS] values('OFFL',12,-3,12,'OFFL',null)"
                RecSet.DoQuery(sql)
                sql = "insert into [@FACE_PARAMETROS] values('PASSDB',0,-3,13,'PASSDB',null)"
                RecSet.DoQuery(sql)
                sql = "insert into [@FACE_PARAMETROS] values('PATHPDF',0,-3,14,'PATHPDF',null)"
                RecSet.DoQuery(sql)
                sql = "insert into [@FACE_PARAMETROS] values('PATHXML',0,-3,15,'PATHXML',null)"
                RecSet.DoQuery(sql)
                sql = "insert into [@FACE_PARAMETROS] values('PREFIX',0,-3,16,'PREFIX',null)"
                RecSet.DoQuery(sql)
                sql = "insert into [@FACE_PARAMETROS] values('PRINTB',0,-3,17,'PRINTB',null)"
                RecSet.DoQuery(sql)
                sql = "insert into [@FACE_PARAMETROS] values('URLWS',0,-3,18,'URLWS',null)"
                RecSet.DoQuery(sql)
                sql = "insert into [@FACE_PARAMETROS] values('USRDB',0,-3,19,'USRDB',null)"
                RecSet.DoQuery(sql)
            End If

        Catch ex As Exception
            SBO_Application.MessageBox(ex.Message)
            Application.Exit()
        End Try
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

        Try
            '// Make sure you're not already connected.
            If oCompany.Connected = True Then
                oCompany.Disconnect()
            End If

            'oCompany = SBO_Application.Company.GetDICompany

            '// Establish the connection to the company database.
            ConnectToCompany = oCompany.Connect
        Catch ex As Exception
            SBO_Application.MessageBox(ex.Message)
        End Try
   
    End Function
#End Region

#Region "CODIGO GENERAL DEL FORM"

    Private oOrderForm As SAPbouiCOM.Form
    Private oNewItem As SAPbouiCOM.Item
    Private oItem As SAPbouiCOM.Item
    Private oFolderItem As SAPbouiCOM.Folder
    Private oCmdVerFactura As SAPbouiCOM.Button
    Private oCmdLabel1 As SAPbouiCOM.StaticText
    Private olblEstado As SAPbouiCOM.StaticText
    Private oCmdReintento As SAPbouiCOM.Button
    Private Const Pais = "GT"
    Private CurrDoc As String
    Private CurrSerie As String
    Private CurrSerieName As String
    Private i As Integer


    Private Sub moSBOApplication_FormDataEvent(ByRef BusinessObjectInfo As SAPbouiCOM.BusinessObjectInfo, ByRef BubbleEvent As Boolean) Handles SBO_Application.FormDataEvent
      
        If (BusinessObjectInfo.EventType = SAPbouiCOM.BoEventTypes.et_FORM_DATA_ADD Or BusinessObjectInfo.EventType = SAPbouiCOM.BoEventTypes.et_FORM_DATA_UPDATE) And BusinessObjectInfo.FormTypeEx = "133" And BusinessObjectInfo.ActionSuccess = True Then
            Dim xml As New Xml.XmlDocument
            Dim Serie As SAPbouiCOM.ComboBox = oOrderForm.Items.Item("88").Specific

            xml.LoadXml(BusinessObjectInfo.ObjectKey)
            Dim DocEntry As Xml.XmlNodeList = xml.GetElementsByTagName("DocEntry")

            If SerieEsBatch(oCompany, SBO_Application, Serie.Value) = False Then
                'Dim xmldoc As New XmlDataDocument()
                'Dim xmlnode As XmlNodeList
                'Dim i As Integer
                'Dim str As String
                'Dim fs As New FileStream("C:\Users\Josue\Downloads\RespFACA237.xml", FileMode.Open, FileAccess.Read)
                'xmldoc.Load(fs)
                'xmlnode = xmldoc.GetElementsByTagName("Product")
                'Dim fechaResol As Xml.XmlNodeList = xmldoc.GetElementsByTagName("FechaResolucion")
                'Dim nitGface As Xml.XmlNodeList = xmldoc.GetElementsByTagName("NITGFACE")
                'Dim nAutorizacion As Xml.XmlNodeList = xmldoc.GetElementsByTagName("NumeroAutorizacion")
                'Dim IniAut As Xml.XmlNodeList = xmldoc.GetElementsByTagName("rangoInicialAutorizado")
                'Dim FinAut As Xml.XmlNodeList = xmldoc.GetElementsByTagName("rangoFinalAutorizado")
                'Dim serieF As Xml.XmlNodeList = xmldoc.GetElementsByTagName("Serie")
                'Dim docF As Xml.XmlNodeList = xmldoc.GetElementsByTagName("uniqueCreatorIdentification")
                'str = xmldoc.ChildNodes.Item(1).ChildNodes(0).ChildNodes(1).ChildNodes(1).ChildNodes(1).InnerText
                'Dim fields As String
                'fields = "U_ESTADO_FACE='A',"
                'fields += "U_FACE_XML='" & Replace(xmldoc.InnerXml, "'", "''''") & "',"
                'fields += "U_FACE_PDFFILE=null,"
                'fields += "U_FIRMA_ELETRONICA='" & str & "',"
                'fields += "U_NUMERO_DOCUMENTO='" & docF(0).InnerText & "',"
                'fields += "U_NUMERO_RESOLUCION='" & nAutorizacion(0).InnerText & "',"
                'fields += "U_SERIE_FACE='" & serieF(0).InnerText & "',"
                'fields += "U_FACTURA_INI='" & IniAut(0).InnerText & "',"
                'fields += "U_FACTURA_FIN='" & FinAut(0).InnerText & "' "

                EnviaDocumento(oCompany, SBO_Application, "FAC", Serie.Selected.Value, "", Serie.Selected.Description, Pais, DocEntry(0).InnerText)
            End If
        End If

        If (BusinessObjectInfo.EventType = SAPbouiCOM.BoEventTypes.et_FORM_DATA_ADD Or BusinessObjectInfo.EventType = SAPbouiCOM.BoEventTypes.et_FORM_DATA_UPDATE) And BusinessObjectInfo.FormTypeEx = "141" And BusinessObjectInfo.ActionSuccess = True Then
            Dim xml As New Xml.XmlDocument
            Dim Serie As SAPbouiCOM.ComboBox = oOrderForm.Items.Item("88").Specific

            xml.LoadXml(BusinessObjectInfo.ObjectKey)
            Dim DocEntry As Xml.XmlNodeList = xml.GetElementsByTagName("DocEntry")

            If SerieEsBatch(oCompany, SBO_Application, Serie.Value) = False Then
                EnviaDocumento(oCompany, SBO_Application, "FACP", Serie.Selected.Value, "", Serie.Selected.Description, Pais, DocEntry(0).InnerText)
            End If
        End If


        If (BusinessObjectInfo.EventType = SAPbouiCOM.BoEventTypes.et_FORM_DATA_ADD Or BusinessObjectInfo.EventType = SAPbouiCOM.BoEventTypes.et_FORM_DATA_UPDATE) And BusinessObjectInfo.FormTypeEx = "60091" And BusinessObjectInfo.ActionSuccess = True Then
            Dim xml As New Xml.XmlDocument
            Dim Serie As SAPbouiCOM.ComboBox = oOrderForm.Items.Item("88").Specific

            xml.LoadXml(BusinessObjectInfo.ObjectKey)
            Dim DocEntry As Xml.XmlNodeList = xml.GetElementsByTagName("DocEntry")

            If SerieEsBatch(oCompany, SBO_Application, Serie.Value) = False Then
                EnviaDocumento(oCompany, SBO_Application, "FAC", Serie.Selected.Value, "", Serie.Selected.Description, Pais, DocEntry(0).InnerText)
            End If
        End If

        If (BusinessObjectInfo.EventType = SAPbouiCOM.BoEventTypes.et_FORM_DATA_ADD Or BusinessObjectInfo.EventType = SAPbouiCOM.BoEventTypes.et_FORM_DATA_UPDATE) And BusinessObjectInfo.FormTypeEx = "60090" And BusinessObjectInfo.ActionSuccess = True Then
            Dim xml As New Xml.XmlDocument
            Dim Serie As SAPbouiCOM.ComboBox = oOrderForm.Items.Item("88").Specific

            xml.LoadXml(BusinessObjectInfo.ObjectKey)
            Dim DocEntry As Xml.XmlNodeList = xml.GetElementsByTagName("DocEntry")

            If SerieEsBatch(oCompany, SBO_Application, Serie.Value) = False Then
                EnviaDocumento(oCompany, SBO_Application, "FAC", Serie.Selected.Value, "", Serie.Selected.Description, Pais, DocEntry(0).InnerText)
            End If
        End If

        If (BusinessObjectInfo.EventType = SAPbouiCOM.BoEventTypes.et_FORM_DATA_ADD Or BusinessObjectInfo.EventType = SAPbouiCOM.BoEventTypes.et_FORM_DATA_UPDATE) And BusinessObjectInfo.FormTypeEx = "65303" And BusinessObjectInfo.ActionSuccess = True Then
            Dim xml As New Xml.XmlDocument
            Dim Serie As SAPbouiCOM.ComboBox = oOrderForm.Items.Item("88").Specific

            xml.LoadXml(BusinessObjectInfo.ObjectKey)
            Dim DocEntry As Xml.XmlNodeList = xml.GetElementsByTagName("DocEntry")

            If SerieEsBatch(oCompany, SBO_Application, Serie.Value) = False Then
                EnviaDocumento(oCompany, SBO_Application, "ND", Serie.Selected.Value, "", Serie.Selected.Description, Pais, DocEntry(0).InnerText)
            End If
        End If

        If (BusinessObjectInfo.EventType = SAPbouiCOM.BoEventTypes.et_FORM_DATA_ADD Or BusinessObjectInfo.EventType = SAPbouiCOM.BoEventTypes.et_FORM_DATA_UPDATE) And BusinessObjectInfo.FormTypeEx = "179" And BusinessObjectInfo.ActionSuccess = True Then
            Dim xml As New Xml.XmlDocument
            Dim Serie As SAPbouiCOM.ComboBox = oOrderForm.Items.Item("88").Specific

            xml.LoadXml(BusinessObjectInfo.ObjectKey)
            Dim DocEntry As Xml.XmlNodeList = xml.GetElementsByTagName("DocEntry")

            If SerieEsBatch(oCompany, SBO_Application, Serie.Value) = False Then
                EnviaDocumento(oCompany, SBO_Application, "NC", Serie.Selected.Value, "", Serie.Selected.Description, Pais, DocEntry(0).InnerText)
            End If
        End If

        'If (BusinessObjectInfo.FormTypeEx = 133 Or BusinessObjectInfo.FormTypeEx = 179 Or BusinessObjectInfo.FormTypeEx = 65303 Or BusinessObjectInfo.FormTypeEx = 60091 Or BusinessObjectInfo.FormTypeEx = 60090) And BusinessObjectInfo.EventType = SAPbouiCOM.BoEventTypes.et_FORM_DATA_LOAD And BusinessObjectInfo.ActionSuccess Then
        '    Dim xml As New Xml.XmlDocument
        '    Dim Serie As SAPbouiCOM.ComboBox = oOrderForm.Items.Item("88").Specific

        '    xml.LoadXml(BusinessObjectInfo.ObjectKey)
        '    Dim DocEntry As Xml.XmlNodeList = xml.GetElementsByTagName("DocEntry")
        '    If Utils.EstadoFACE(oCompany, DocEntry(0).InnerText, "") <> "A" And Utils.ValidaSerie(oCompany, SBO_Application, ) Then

        '    End If
        'End If
    End Sub

    Private Sub SBO_Application_ItemEvent(ByVal FormUID As String, ByRef pVal As SAPbouiCOM.ItemEvent, ByRef BubbleEvent As Boolean) Handles SBO_Application.ItemEvent
        If (pVal.FormType = 133 Or pVal.FormType = 179 Or pVal.FormType = 65303 Or pVal.FormType = 60091 Or pVal.FormType = 60090 Or pVal.FormType = 141) And ((pVal.ItemUID = "1") And (pVal.EventType = SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED) And (pVal.Before_Action = True)) Then
            oOrderForm = SBO_Application.Forms.GetFormByTypeAndCount(pVal.FormType, pVal.FormTypeCount)
        End If
        If (pVal.FormType = 133 Or pVal.FormType = 179 Or pVal.FormType = 65303 Or pVal.FormType = 60091 Or pVal.FormType = 60090 Or pVal.FormType = 141) And pVal.EventType = SAPbouiCOM.BoEventTypes.et_FORM_LOAD And pVal.BeforeAction = False Then
            oOrderForm = SBO_Application.Forms.GetFormByTypeAndCount(pVal.FormType, pVal.FormTypeCount)
        End If
        If (pVal.FormType = 133 Or pVal.FormType = 179 Or pVal.FormType = 65303 Or pVal.FormType = 60091 Or pVal.FormType = 60090 Or pVal.FormType = 141) And ((pVal.ItemUID = "10000329") And (pVal.EventType = SAPbouiCOM.BoEventTypes.et_CLICK) And (pVal.BeforeAction = False)) Then
            LimpiaUDF()
        End If

        'If (pVal.FormType = 133 Or pVal.FormType = 179 Or pVal.FormType = 65303 Or pVal.FormType = 60091 Or pVal.FormType = 60090) And pVal.EventType = SAPbouiCOM.BoEventTypes.et_FORM_LOAD And pVal.Before_Action = True Then
        '    Dim cmdReenvio As SAPbouiCOM.Button

        '    oOrderForm = SBO_Application.Forms.GetFormByTypeAndCount(pVal.FormType, pVal.FormTypeCount)
        '    oNewItem = oOrderForm.Items.Add("cmdReenvio", SAPbouiCOM.BoFormItemTypes.it_BUTTON)
        '    oItem = oOrderForm.Items.Item("2")
        '    oNewItem.Top = oItem.Top
        '    oNewItem.Height = oItem.Height
        '    oNewItem.Width = oItem.Width + 100
        '    oNewItem.Left = oItem.Left + oItem.Width + 5
        '    oNewItem.Visible = False
        '    cmdReenvio = oNewItem.Specific
        '    cmdReenvio.Caption = "Reenvio documento electrónico"
        'End If
        'If (pVal.FormType = 133 Or pVal.FormType = 179 Or pVal.FormType = 65303 Or pVal.FormType = 60091 Or pVal.FormType = 60090) And pVal.ItemUID = "1" And pVal.EventType = SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED And pVal.Before_Action = True Then
        '             EnviaDocumento(oCompany, SBO_Application, "NC", serie.Selected.Value, "", serie.Selected.Description, Pais, DocEntry(0).InnerText)
        'End If
    End Sub
    'Private Sub SBO_Application_ItemEvent(ByVal FormUID As String, ByRef pVal As SAPbouiCOM.ItemEvent, ByRef BubbleEvent As Boolean) Handles SBO_Application.ItemEvent

    '    Try

    '        esFactura = False
    '        If ((pVal.FormType = 179 And pVal.EventType <> SAPbouiCOM.BoEventTypes.et_FORM_UNLOAD) And (pVal.Before_Action = True)) Then
    '            oOrderForm = SBO_Application.Forms.GetFormByTypeAndCount(pVal.FormType, pVal.FormTypeCount)
    '        End If

    '        If ((pVal.FormType = 65303 And pVal.EventType <> SAPbouiCOM.BoEventTypes.et_FORM_UNLOAD) And (pVal.Before_Action = True)) Then
    '            oOrderForm = SBO_Application.Forms.GetFormByTypeAndCount(pVal.FormType, pVal.FormTypeCount)

    '        End If

    '        If ((pVal.FormType = 60091 And pVal.EventType <> SAPbouiCOM.BoEventTypes.et_FORM_UNLOAD) And (pVal.Before_Action = True)) Then
    '            oOrderForm = SBO_Application.Forms.GetFormByTypeAndCount(pVal.FormType, pVal.FormTypeCount)
    '        End If

    '        If ((pVal.FormType = 60090 And pVal.EventType <> SAPbouiCOM.BoEventTypes.et_FORM_UNLOAD) And (pVal.Before_Action = True)) Then
    '            oOrderForm = SBO_Application.Forms.GetFormByTypeAndCount(pVal.FormType, pVal.FormTypeCount)
    '        End If


    '        If ((pVal.FormType = 133) And (pVal.EventType = SAPbouiCOM.BoEventTypes.et_FORM_LOAD) And (pVal.Before_Action = True)) Then

    '            esFactura = True

    '            oOrderForm = SBO_Application.Forms.GetFormByTypeAndCount(pVal.FormType, pVal.FormTypeCount)

    '            ''// add a new folder item to the form
    '            'oNewItem = oOrderForm.Items.Add("FACE", SAPbouiCOM.BoFormItemTypes.it_FOLDER)

    '            ''// use an existing folder item for grouping and setting the
    '            ''// items properties (such as location properties)
    '            ''// use the 'Display Debug Information' option (under 'Tools')
    '            ''// in the application to acquire the UID of the desired folder
    '            'oItem = oOrderForm.Items.Item("138")


    '            'oNewItem.Top = oItem.Top
    '            'oNewItem.Height = oItem.Height
    '            'oNewItem.Width = oItem.Width
    '            'oNewItem.Left = oItem.Left + oItem.Width

    '            'oFolderItem = oNewItem.Specific

    '            'oFolderItem.Caption = "Documento Electrónico"

    '            ''// group the folder with the desired folder item
    '            'oFolderItem.GroupWith("138")



    '            ''// add your own items to the form
    '            'AddItemsToOrderForm()

    '            ''DisableUserFields()

    '            'oOrderForm.PaneLevel = 1

    '        End If

    '        'If pVal.FormType = 133 And pVal.ItemUID = "FACE" And pVal.EventType = SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED And pVal.Before_Action = True Then

    '        '    oOrderForm.PaneLevel = 5

    '        'End If

    '        'If ((pVal.FormType = 133 And pVal.EventType <> SAPbouiCOM.BoEventTypes.et_FORM_UNLOAD) And (pVal.Before_Action = True)) Then

    '        '    esFactura = True
    '        '    '// get the event sending form

    '        '    If ((pVal.EventType = SAPbouiCOM.BoEventTypes.et_FORM_LOAD) And (pVal.Before_Action = True)) Then

    '        '        oOrderForm = SBO_Application.Forms.GetFormByTypeAndCount(pVal.FormType, pVal.FormTypeCount)

    '        '        '// add a new folder item to the form
    '        '        oNewItem = oOrderForm.Items.Add("FACE", SAPbouiCOM.BoFormItemTypes.it_FOLDER)

    '        '        '// use an existing folder item for grouping and setting the
    '        '        '// items properties (such as location properties)
    '        '        '// use the 'Display Debug Information' option (under 'Tools')
    '        '        '// in the application to acquire the UID of the desired folder
    '        '        oItem = oOrderForm.Items.Item("138")


    '        '        oNewItem.Top = oItem.Top
    '        '        oNewItem.Height = oItem.Height
    '        '        oNewItem.Width = oItem.Width
    '        '        oNewItem.Left = oItem.Left + oItem.Width

    '        '        oFolderItem = oNewItem.Specific

    '        '        oFolderItem.Caption = "Documento Electrónico"

    '        '        '// group the folder with the desired folder item
    '        '        oFolderItem.GroupWith("138")



    '        '        '// add your own items to the form
    '        '        AddItemsToOrderForm()

    '        '        'DisableUserFields()

    '        '        oOrderForm.PaneLevel = 1

    '        '    End If

    '        '    If pVal.ItemUID = "FACE" And pVal.EventType = SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED And pVal.Before_Action = True Then

    '        '        '// when the new folder is clicked change the form's pane level
    '        '        '// by doing so your items will apear on the new folder
    '        '        '// assuming they were placed correctly and their pane level
    '        '        '// was also set accordingly
    '        '        oOrderForm.PaneLevel = 5

    '        '    End If

    '        'End If

    '        'If pVal.FormType = 133 And ((pVal.ItemUID = "cmdVer") And (pVal.EventType = SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED) And (pVal.Before_Action = False)) Then
    '        '    VerPDF("FAC")
    '        'End If

    '        'If pVal.FormType = 133 And ((pVal.ItemUID = "cmdEnviar") And (pVal.EventType = SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED) And (pVal.Before_Action = False)) Then
    '        '    EnviaDocumento(oCompany, SBO_Application, "FAC", Me.CurrSerie, Me.CurrDoc, Me.CurrSerieName, Pais)
    '        'End If


    '        ''If pVal.FormType = 133 And ((pVal.ItemUID = "1") And (pVal.EventType = SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED) And (pVal.Before_Action = False)) Then
    '        ''    'Dim Docentry As String = ""
    '        ''    'Docentry = Utils.GetDocEntry(MySerie.Value.Trim, oCompany.UserSignature, DateDocument.String, docTotal.Value, Client.Value, "FAC")
    '        ''    EnviaDocumento(oCompany, SBO_Application, "FAC", Me.CurrSerie, Me.CurrDoc, Me.CurrSerieName, Pais, Docentry)
    '        ''End If

    '        ''If pVal.FormType = 60091 And ((pVal.ItemUID = "1") And (pVal.EventType = SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED) And (pVal.Before_Action = False)) Then
    '        ''    Dim Docentry As String = ""
    '        ''    Docentry = Utils.GetDocEntry(MySerie.Value, oCompany.UserSignature, DateDocument.Value, docTotal.Value, Client.Value, "FAC")
    '        ''    EnviaDocumento(oCompany, SBO_Application, "FAC", Me.CurrSerie, Me.CurrDoc, Me.CurrSerieName, Pais, Docentry)
    '        ''End If
    '        ''If pVal.FormType = 60090 And ((pVal.ItemUID = "1") And (pVal.EventType = SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED) And (pVal.Before_Action = False)) Then
    '        ''    Dim Docentry As String = ""
    '        ''    Docentry = Utils.GetDocEntry(MySerie.Value, oCompany.UserSignature, DateDocument.Value, docTotal.Value, Client.Value, "FAC")
    '        ''    EnviaDocumento(oCompany, SBO_Application, "FAC", Me.CurrSerie, Me.CurrDoc, Me.CurrSerieName, Pais, Docentry)
    '        ''End If

    '        ''If pVal.FormType = 65303 And ((pVal.ItemUID = "1") And (pVal.EventType = SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED) And (pVal.Before_Action = False)) Then
    '        ''    Dim Docentry As String = ""
    '        ''    Docentry = Utils.GetDocEntry(MySerie.Value, oCompany.UserSignature, DateDocument.Value, docTotal.Value, Client.Value, "ND")
    '        ''    EnviaDocumento(oCompany, SBO_Application, "ND", Me.CurrSerie, Me.CurrDoc, Me.CurrSerieName, Pais, Docentry)
    '        ''End If

    '        ''If pVal.FormType = 179 And ((pVal.ItemUID = "1") And (pVal.EventType = SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED) And (pVal.Before_Action = False)) Then

    '        ''    Dim Docentry As String = ""
    '        ''    Docentry = Utils.GetDocEntry(MySerie.Value, oCompany.UserSignature, DateDocument.Value, docTotal.Value, Client.Value, "NC")
    '        ''    EnviaDocumento(oCompany, SBO_Application, "NC", Me.CurrSerie, Me.CurrDoc, Me.CurrSerieName, Pais, Docentry)
    '        ''End If


    '        'If (pVal.FormType = 133 Or pVal.FormType = 179 Or pVal.FormType = 65303 Or pVal.FormType = 60091 Or pVal.FormType = 60090) And ((pVal.ItemUID = "1") And (pVal.EventType = SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED) And (pVal.Before_Action = True)) Then
    '        '    oOrderForm = SBO_Application.Forms.GetFormByTypeAndCount(pVal.FormType, pVal.FormTypeCount)
    '        '    ObtieneSerieyNumDOC(CurrSerie, CurrSerieName, CurrDoc)

    '        '    DateDocument = oOrderForm.Items.Item("10").Specific
    '        '    Client = oOrderForm.Items.Item("4").Specific
    '        '    docTotal = oOrderForm.Items.Item("29").Specific
    '        '    MySerie = oOrderForm.Items.Item("88").Specific
    '        'End If

    '    Catch ex As Exception
    '        SBO_Application.MessageBox(ex.Message)
    '    End Try

    'End Sub

    Private Sub ObtieneSerieyNumDOC(ByRef CurrSerie As String, ByRef CurrSerieName As String, ByRef CurrDoc As String)

        Try
            Dim oItem As SAPbouiCOM.Item
            Dim mySerie As SAPbouiCOM.ComboBox
            Dim myNumFac As SAPbouiCOM.EditText


            oItem = oOrderForm.Items.Item("88")
            mySerie = oItem.Specific
            CurrSerie = mySerie.Selected.Value
            CurrSerieName = mySerie.Selected.Description


            oItem = oOrderForm.Items.Item("8")
            myNumFac = oItem.Specific
            If myNumFac.Value <> "" Then CurrDoc = myNumFac.Value

        Catch ex As Exception
            SBO_Application.SetStatusBarMessage(ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short)
        End Try

    End Sub

    Private Sub VerPDF(ByVal TipoFile As String)

        Dim filePDF As String = ""
        Dim oItem As SAPbouiCOM.Item
        Dim myNumFac As SAPbouiCOM.EditText
        Dim mySerie As SAPbouiCOM.ComboBox
        Dim CodSerie As Integer
        Dim NumFac As String

        Try

            oItem = oOrderForm.Items.Item("88")
            mySerie = oItem.Specific
            CodSerie = mySerie.Selected.Value


            oItem = oOrderForm.Items.Item("8")
            myNumFac = oItem.Specific
            NumFac = myNumFac.Value
            If Utils.GeneraPDF(TipoFile, oCompany, CodSerie, NumFac, filePDF) Then
                System.Diagnostics.Process.Start(filePDF)
            End If
        Catch ex As Exception
            SBO_Application.MessageBox(ex.Message)
        End Try
    End Sub

    Private Sub SBO_Application_MenuEvent(ByRef pVal As SAPbouiCOM.MenuEvent, ByRef BubbleEvent As Boolean) Handles SBO_Application.MenuEvent

        If (pVal.MenuUID = "paramFACE") And (pVal.BeforeAction = False) Then
            Dim initFrm As New ParametrosForm
            BubbleEvent = False
        End If

        If (pVal.MenuUID = "procReenvioFACE") And (pVal.BeforeAction = False) Then
            Dim initFrm As New ReenvioForm
            BubbleEvent = False
        End If

        If (pVal.MenuUID = "procBatchFACE") And (pVal.BeforeAction = False) Then
            Dim initFrm As New BatchForm
            BubbleEvent = False
        End If

        If (pVal.MenuUID = "consFACE") And (pVal.BeforeAction = False) Then
            Dim initFrm As New ConsultaForm
            BubbleEvent = False
        End If

        If (pVal.MenuUID = "ImpFACE") And (pVal.BeforeAction = False) Then
            Dim initFrm As New ImportFiles
            BubbleEvent = False
        End If

        If pVal.MenuUID = "1287" And pVal.BeforeAction = False Then
            LimpiaUDF()
        End If
        'If esFactura Then
        '    If (pVal.MenuUID = "1288" Or pVal.MenuUID = "1289" Or pVal.MenuUID = "1290" Or pVal.MenuUID = "1291") And pVal.BeforeAction = False Then
        '        HabilitarBotonesFACE()
        '        BubbleEvent = False
        '    End If
        'End If
    End Sub


    Private Sub LimpiaUDF()
        Try
            If (oOrderForm.TypeEx = "133" Or oOrderForm.TypeEx = "179" Or oOrderForm.TypeEx = "65303" Or oOrderForm.TypeEx = "60091" Or oOrderForm.TypeEx = "60090" Or oOrderForm.TypeEx = "141") Then
                Dim udfForm As SAPbouiCOM.Form = SBO_Application.Forms.Item(SBO_Application.Forms.GetForm(oOrderForm.TypeEx, oOrderForm.TypeCount).UDFFormUID)

                If Not udfForm Is Nothing Then
                    Dim item1 As SAPbouiCOM.EditText = udfForm.Items.Item("U_ESTADO_FACE").Specific
                    Dim item2 As SAPbouiCOM.EditText = udfForm.Items.Item("U_FACE_XML").Specific
                    Dim item3 As SAPbouiCOM.EditText = udfForm.Items.Item("U_MOTIVO_RECHAZO").Specific
                    Dim item4 As SAPbouiCOM.EditText = udfForm.Items.Item("U_FACE_PDFFILE").Specific
                    Dim item5 As SAPbouiCOM.EditText = udfForm.Items.Item("U_FIRMA_ELETRONICA").Specific
                    Dim item6 As SAPbouiCOM.EditText = udfForm.Items.Item("U_NUMERO_DOCUMENTO").Specific
                    Dim item7 As SAPbouiCOM.EditText = udfForm.Items.Item("U_NUMERO_RESOLUCION").Specific
                    Dim item8 As SAPbouiCOM.EditText = udfForm.Items.Item("U_SERIE_FACE").Specific
                    Dim item9 As SAPbouiCOM.EditText = udfForm.Items.Item("U_FACTURA_INI").Specific
                    Dim item10 As SAPbouiCOM.EditText = udfForm.Items.Item("U_FACTURA_FIN").Specific
                    Dim item11 As SAPbouiCOM.EditText = udfForm.Items.Item("U_FACTURA_SERIE").Specific
                    Dim item12 As SAPbouiCOM.EditText = udfForm.Items.Item("U_FACTURA_PREIMPRESO").Specific
                    Dim item13 As SAPbouiCOM.EditText = udfForm.Items.Item("U_FECHA_ENVIO_FACE").Specific
                    Dim item14 As SAPbouiCOM.EditText = udfForm.Items.Item("U_EMAIL_FACE").Specific

                    oOrderForm.Freeze(True)
                    udfForm.Freeze(True)
                    item1.Value = ""
                    item2.Value = ""
                    item3.Value = ""
                    item4.Value = ""
                    item5.Value = ""
                    item6.Value = ""
                    item7.Value = ""
                    item8.Value = ""
                    item9.Value = ""
                    item10.Value = ""
                    item11.Value = ""
                    item12.Value = ""
                    item13.Value = ""
                    item14.Value = ""
                    oOrderForm.Freeze(False)
                    udfForm.Freeze(False)
                End If
            End If
        Catch ex As Exception
            'SBO_Application.MessageBox(ex.Message)
        End Try
    End Sub
    Private Sub AddMenuItems()

        Try

            '//******************************************************************
            '// Let's add a separator, a pop-up menu item and a string menu item
            '//******************************************************************

            Dim oMenus As SAPbouiCOM.Menus
            Dim oMenuItem As SAPbouiCOM.MenuItem

            Dim i As Integer '// to be used as counter
            Dim lAddAfter As Integer
            Dim sXML As String

            '// Get the menus collection from the application
            oMenus = SBO_Application.Menus
            '--------------------------------------------
            'Save an XML file containing the menus...
            '--------------------------------------------
            'sXML = SBO_Application.Menus.GetAsXML
            'Dim xmlD As System.Xml.XmlDocument
            'xmlD = New System.Xml.XmlDocument
            'xmlD.LoadXml(sXML)
            'xmlD.Save("c:\\mnu.xml")
            '--------------------------------------------


            Dim oCreationPackage As SAPbouiCOM.MenuCreationParams
            oCreationPackage = SBO_Application.CreateObject(SAPbouiCOM.BoCreatableObjectType.cot_MenuCreationParams)
            oMenuItem = SBO_Application.Menus.Item("43520") 'moudles'

            Dim sPath As String

            sPath = Application.StartupPath
            sPath = sPath.Remove(sPath.Length - 3, 3)

            '// find the place in wich you want to add your menu item
            '// in this example I chose to add my menu item under
            '// SAP Business One.
            If SBO_Application.Menus.Exists("FACE") Then
                SBO_Application.Menus.RemoveEx("FACE")
            End If
            oCreationPackage.Type = SAPbouiCOM.BoMenuType.mt_POPUP
            oCreationPackage.UniqueID = "FACE"
            oCreationPackage.String = "Facturación Electrónica"
            oCreationPackage.Enabled = True
            oCreationPackage.Image = Replace(Application.StartupPath & "\invoice.png", "\\", "\")
            oCreationPackage.Position = 1

            oMenus = oMenuItem.SubMenus

            Try ' If the manu already exists this code will fail
                oMenus.AddEx(oCreationPackage)

                '// Get the menu collection of the newly added pop-up item
                oMenuItem = SBO_Application.Menus.Item("FACE")
                oMenus = oMenuItem.SubMenus

                '// Create s sub menu
                oCreationPackage.Type = SAPbouiCOM.BoMenuType.mt_STRING
                oCreationPackage.UniqueID = "paramFACE"
                oCreationPackage.String = "Parámetros"
                oMenus.AddEx(oCreationPackage)

                oCreationPackage.Type = SAPbouiCOM.BoMenuType.mt_STRING
                oCreationPackage.UniqueID = "consFACE"
                oCreationPackage.String = "Consulta de documentos"
                oMenus.AddEx(oCreationPackage)

                oCreationPackage.Type = SAPbouiCOM.BoMenuType.mt_STRING
                oCreationPackage.UniqueID = "procBatchFACE"
                oCreationPackage.String = "Proceso Batch de documentos"
                oMenus.AddEx(oCreationPackage)

                oCreationPackage.Type = SAPbouiCOM.BoMenuType.mt_STRING
                oCreationPackage.UniqueID = "procReenvioFACE"
                oCreationPackage.String = "Reenvio de documentos"
                oMenus.AddEx(oCreationPackage)

                oCreationPackage.Type = SAPbouiCOM.BoMenuType.mt_STRING
                oCreationPackage.UniqueID = "ImpFACE"
                oCreationPackage.String = "Archivos Config. Base de datos"
                oMenus.AddEx(oCreationPackage)


            Catch er As Exception ' Menu already exists
                'SBO_Application.MessageBox("Menu Already Exists")
            End Try

        Catch ex As Exception
            SBO_Application.MessageBox(ex.Message)
        End Try
    End Sub

    Private Sub AddItemsToOrderForm()

        Try

            oItem = oOrderForm.Items.Item("147")
            oNewItem = oOrderForm.Items.Add("cmdVer", SAPbouiCOM.BoFormItemTypes.it_BUTTON)
            oNewItem.Left = oItem.Left + 50
            oNewItem.Width = 100
            oNewItem.Top = oItem.Top + (i - 1) * 19
            oNewItem.Height = 19
            oNewItem.FromPane = 5
            oNewItem.ToPane = 5
            oNewItem.Enabled = False

            oCmdVerFactura = oNewItem.Specific
            oCmdVerFactura.Caption = "Ver documento"

            oItem = oOrderForm.Items.Item("149")
            oNewItem = oOrderForm.Items.Add("Label1", SAPbouiCOM.BoFormItemTypes.it_STATIC)
            oNewItem.Left = oItem.Left + 50
            oNewItem.Width = 100
            oNewItem.Top = oItem.Top + (i - 1) * 19
            oNewItem.Height = 19
            oNewItem.FromPane = 5
            oNewItem.ToPane = 5

            oCmdLabel1 = oNewItem.Specific
            oCmdLabel1.Caption = "Estado documento:"

            oItem = oOrderForm.Items.Item("150")
            oNewItem = oOrderForm.Items.Add("LblEstado", SAPbouiCOM.BoFormItemTypes.it_STATIC)
            oNewItem.Left = oItem.Left + 100
            oNewItem.Width = 200
            oNewItem.Top = oItem.Top + (i - 1) * 19
            oNewItem.Height = 19
            oNewItem.FromPane = 5
            oNewItem.ToPane = 5

            olblEstado = oNewItem.Specific
            olblEstado.Caption = "N/A"

            oItem = oOrderForm.Items.Item("66")
            oNewItem = oOrderForm.Items.Add("cmdEnviar", SAPbouiCOM.BoFormItemTypes.it_BUTTON)
            oNewItem.Left = oItem.Left + 50
            oNewItem.Width = 100
            oNewItem.Top = oItem.Top + (i - 1) * 19
            oNewItem.Height = 19
            oNewItem.FromPane = 5
            oNewItem.ToPane = 5
            oNewItem.Enabled = False

            oCmdReintento = oNewItem.Specific
            oCmdReintento.Caption = "Reenviar documento"


        Catch ex As Exception
            SBO_Application.MessageBox(ex.Message)
        End Try

    End Sub

    'Private Sub HabilitarBotonesFACE()
    '    Dim cmdEnviar As SAPbouiCOM.Button
    '    Dim mySerie As SAPbouiCOM.ComboBox
    '    Dim myNumFac As SAPbouiCOM.EditText
    '    Dim lblEstado As SAPbouiCOM.StaticText
    '    Dim Estado As String
    '    Dim RecSet As SAPbobsCOM.Recordset
    '    Dim QryStr As String
    '    Try

    '        oItem = oOrderForm.Items.Item("88")
    '        mySerie = oItem.Specific

    '        oItem = oOrderForm.Items.Item("8")
    '        myNumFac = oItem.Specific

    '        If Utils.ExisteDocumento(oCompany, SBO_Application, mySerie.Selected.Value, myNumFac.Value, "FAC") And Utils.ValidaSerie(oCompany, SBO_Application, mySerie.Selected.Value) Then



    '            RecSet = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
    '            QryStr = "select * from OINV where Series =" & mySerie.Selected.Value & " and DocNum =" & myNumFac.Value & " and DocType ='I'"
    '            RecSet.DoQuery(QryStr)
    '            If RecSet.RecordCount > 0 Then
    '                oItem = oOrderForm.Items.Item("LblEstado")
    '                lblEstado = oItem.Specific

    '                Select Case RecSet.Fields.Item("U_ESTADO_FACE").Value
    '                    Case Is = "R"
    '                        lblEstado.Caption = "RECHAZADO" '& RecSet.Fields.Item("U_MOTIVO_RECHAZO").Value.ToString.ToUpper

    '                        oItem = oOrderForm.Items.Item("cmdVer")
    '                        oItem.Enabled = False

    '                        oItem = oOrderForm.Items.Item("cmdEnviar")
    '                        oItem.Enabled = True


    '                    Case Is = "A"
    '                        lblEstado.Caption = "AUTORIZADO"

    '                        oItem = oOrderForm.Items.Item("cmdVer")
    '                        oItem.Enabled = True

    '                        oItem = oOrderForm.Items.Item("cmdEnviar")
    '                        oItem.Enabled = False
    '                    Case Else
    '                        oItem = oOrderForm.Items.Item("cmdVer")
    '                        oItem.Enabled = False

    '                        oItem = oOrderForm.Items.Item("cmdEnviar")
    '                        oItem.Enabled = False

    '                        oItem = oOrderForm.Items.Item("LblEstado")
    '                        lblEstado = oItem.Specific
    '                        lblEstado.Caption = "N/A"
    '                End Select

    '            End If
    '            System.Runtime.InteropServices.Marshal.ReleaseComObject(RecSet)
    '            RecSet = Nothing
    '            GC.Collect()
    '        Else

    '            oItem = oOrderForm.Items.Item("cmdVer")
    '            oItem.Enabled = False

    '            oItem = oOrderForm.Items.Item("cmdEnviar")
    '            oItem.Enabled = False

    '            oItem = oOrderForm.Items.Item("LblEstado")
    '            lblEstado = oItem.Specific
    '            lblEstado.Caption = "N/A"

    '        End If

    '    Catch ex As Exception
    '        SBO_Application.MessageBox(ex.Message)
    '    End Try
    'End Sub

    Private Sub SBO_Application_AppEvent(ByVal EventType As SAPbouiCOM.BoAppEventTypes) Handles SBO_Application.AppEvent
        Select Case EventType
            Case SAPbouiCOM.BoAppEventTypes.aet_ShutDown
                '//**************************************************************
                '//
                '// Take care of terminating your AddOn application
                '//
                '//**************************************************************
                SBO_Application.SetStatusBarMessage("Finalizando add-on facturación electrónica...", SAPbouiCOM.BoMessageTime.bmt_Short, False)
                System.Environment.Exit(0)
            Case SAPbouiCOM.BoAppEventTypes.aet_CompanyChanged
                SBO_Application.SetStatusBarMessage("Finalizando add-on facturación electrónica...", SAPbouiCOM.BoMessageTime.bmt_Short, False)
                Company.Disconnect()
                System.Environment.Exit(0)
            Case SAPbouiCOM.BoAppEventTypes.aet_ServerTerminition
                SBO_Application.SetStatusBarMessage("Finalizando add-on facturación electrónica...", SAPbouiCOM.BoMessageTime.bmt_Short, False)
                System.Environment.Exit(0)
                'Case SAPbouiCOM.BoAppEventTypes.aet_ShutDown
                '    System.Windows.Forms.Application.Exit()
            Case SAPbouiCOM.BoAppEventTypes.aet_LanguageChanged
                SBO_Application.SetStatusBarMessage("Finalizando add-on facturación electrónica...", SAPbouiCOM.BoMessageTime.bmt_Short, False)
                System.Environment.Exit(0)
        End Select
    End Sub

#End Region

    'Private Sub SBO_Application_PrintEvent(ByRef eventInfo As SAPbouiCOM.PrintEventInfo, ByRef BubbleEvent As Boolean) Handles SBO_Application.PrintEvent
    '    Try

    '        If ObtieneValorParametro(Me.oCompany, SBO_Application, "PRINTB") = "1" Then
    '            If IdForm = 133 And ((IdItem = "1") And (IdEvent = SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED) And (IdAction = False)) Then
    '                IdAction = False
    '                EnviaDocumento(oCompany, SBO_Application, "FAC", Me.CurrSerie, Me.CurrDoc, Me.CurrSerieName, Pais)
    '            End If

    '            If IdForm = 60091 And ((IdItem = "1") And (IdEvent = SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED) And (IdAction = True)) Then
    '                IdAction = False
    '                EnviaDocumento(oCompany, SBO_Application, "FAC", Me.CurrSerie, Me.CurrDoc, Me.CurrSerieName, Pais)
    '            End If
    '            If IdForm = 60090 And ((IdItem = "1") And (IdEvent = SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED) And (IdAction = True)) Then
    '                IdAction = False
    '                EnviaDocumento(oCompany, SBO_Application, "FAC", Me.CurrSerie, Me.CurrDoc, Me.CurrSerieName, Pais)
    '            End If

    '            If IdForm = 65303 And ((IdItem = "1") And (IdEvent = SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED) And (IdAction = True)) Then
    '                IdAction = False
    '                EnviaDocumento(oCompany, SBO_Application, "ND", Me.CurrSerie, Me.CurrDoc, Me.CurrSerieName, Pais)
    '            End If

    '            If IdForm = 179 And ((IdItem = "1") And (IdEvent = SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED) And (IdAction = True)) Then
    '                IdAction = False
    '                EnviaDocumento(oCompany, SBO_Application, "NC", Me.CurrSerie, Me.CurrDoc, Me.CurrSerieName, Pais)
    '            End If
    '        End If
    '    Catch ex As Exception
    '        SBO_Application.SetStatusBarMessage(ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short)
    '    End Try

    'End Sub
End Class
