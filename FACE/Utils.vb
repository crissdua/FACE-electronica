Module Utils

    Private Function ConvToHex(ByVal x As Integer) As String
        If x > 9 Then
            ConvToHex = Chr(x + 55)
        Else
            ConvToHex = CStr(x)
        End If
    End Function

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

    Public Sub AddUserField(ByVal oCompany As SAPbobsCOM.Company, ByVal TableName As String, ByVal FieldName As String, ByVal FieldDescription As String, ByVal FieldType As SAPbobsCOM.BoFieldTypes, ByVal Size As Integer, Optional ByVal addSymbol As Boolean = True)
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

    Private Function ExistField(ByVal oCompany As SAPbobsCOM.Company, ByVal TableName As String, ByVal FieldName As String, ByVal addSymbol As Boolean) As Boolean
        Dim RecSet As SAPbobsCOM.Recordset
        Dim QryStr As String = ""
        Dim result As Boolean = False

        Try
            If addSymbol Then
                TableName = "@" & TableName
            End If
            QryStr = "select TableID,FieldID,AliasID from CUFD WHERE TableID='" & TableName & "' and AliasID  ='" & FieldName & "'"
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

   
End Module
