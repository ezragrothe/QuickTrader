Public Class AboutWindow

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles Button1.Click
        Close()
    End Sub

    Private Sub Border_PreviewMouseLeftButtonDown(ByVal sender As System.Object, ByVal e As System.Windows.Input.MouseButtonEventArgs)
        If Not TypeOf e.Source Is Button And Not TypeOf e.Source Is Run Then
            DragMove()
        End If
    End Sub

    Private Sub AboutWindow_Loaded(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs) Handles Me.Loaded
        lblVersion.Content = "v" & Version
    End Sub

    Private Sub link_Click(sender As Object, e As System.Windows.RoutedEventArgs) Handles link.Click
        Dim win As New UpdateHistoryWindow
        win.ShowDialog()
    End Sub

End Class
