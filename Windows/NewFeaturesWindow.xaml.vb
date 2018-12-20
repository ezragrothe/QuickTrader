Public Class NewFeaturesWindow

    Private Sub BaseWindow_Loaded(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Title = "New Features For v" & Version
        txtNewFeatures.Text = ""
        For Each item In VersionChanges
            txtNewFeatures.Text &= item & vbNewLine & vbNewLine
        Next
    End Sub

    Private Sub btnOK_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Close()
    End Sub

End Class
