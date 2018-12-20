Public Class CodeEditorOptionsWindow
    Private codeEditor As CodeEditorWindow
    Public Sub New(ByVal codeEditor As CodeEditorWindow)
        InitializeComponent()
        Me.codeEditor = codeEditor
    End Sub

    Private Sub btnOK_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Close()
    End Sub

    Private Sub btnFont_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Dim fontDialog As New FontDialog
        With fontDialog.Font
            fontDialog.Font = codeEditor.Font
            fontDialog.Owner = Me
            fontDialog.ShowDialog()
            If fontDialog.ButtonOKPressed = True Then
                codeEditor.Font = fontDialog.Font
            End If
        End With
        codeEditor.Refresh()
    End Sub
    Private Sub colBackground_ColorChanged(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs) Handles colBackground.ColorChanged
        codeEditor.TextBackgroundColor = colBackground.Color
        codeEditor.Refresh()
    End Sub
End Class
