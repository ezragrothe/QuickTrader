Public Class CommandHelpTextAttribute
    Inherits Attribute
    Public Sub New(ByVal text As String)
        _helpText = text
    End Sub
    Private _helpText As String
    Public ReadOnly Property HelpText As String
        Get
            Return _helpText
        End Get
    End Property
End Class
