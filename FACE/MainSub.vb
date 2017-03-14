Module MainSub

    Public Sub Main()

        Dim SBOSysForm As SystemForm

        'Aqui se define el tipo de empresa y GFACE a utilizar
        Utils.TipoGFACE = TipoFACE.Documenta
        Utils.Empresa = EmpresaFACE.LLAMASA

        SBOSysForm = New SystemForm()
        ' Starting the Application
        System.Windows.Forms.Application.Run()

    End Sub

End Module

