Imports System.IO
Imports System.Management

Public Class License

    Dim LicFile As String = Replace(Application.StartupPath & "\Settings.xml", "\\", "\")

    Private Sub cmdSalir_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdSalir.Click
        Application.Exit()
    End Sub

    Private Sub License_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        If File.Exists(LicFile) Then
            Me.txtHarwareKey.Text = GetHKey()
        Else
            Application.Exit()
        End If
    End Sub

    Private Function GetHKey() As String
        Try
            Dim objMOS As ManagementObjectSearcher
            Dim objMOC As Management.ManagementObjectCollection
            Dim objMO As Management.ManagementObject
            Dim result As String

            objMOS = New ManagementObjectSearcher("Select * From Win32_Processor")
            objMOC = objMOS.Get

            For Each objMO In objMOC
                result = objMO("ProcessorID")
            Next
            objMOS.Dispose()
            objMOS = Nothing
            objMO.Dispose()
            objMO = Nothing
            Return result
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical, "Error")
        End Try
    End Function
End Class