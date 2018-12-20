Public Class CommandMenuItem
    Inherits MenuItem
    Public Sub New()

    End Sub
    Private Sub CommandMenuItem_SubmenuOpened(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs) Handles Me.SubmenuOpened
        For Each item In Items
            If TypeOf item Is MenuItem Then
                For Each cmd In ChartCommands.GetCommands
                    If item.Command IsNot Nothing AndAlso item.Command.Name = CType(cmd, RoutedUICommand).Name Then
                        CType(item, MenuItem).Command = Nothing
                        CType(item, MenuItem).Command = cmd
                        Exit For
                    End If
                Next
            End If
        Next
    End Sub
End Class
