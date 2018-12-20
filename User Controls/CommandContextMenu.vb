Public Class CommandContextMenu
    Inherits ContextMenu

    Private desktopWindow As DesktopWindow
    Public Sub New(ByVal desktopWindow As DesktopWindow)
        Me.desktopWindow = desktopWindow
    End Sub
    Private Sub CommandContextMenu_Opened(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs) Handles Me.Opened
        CommandBindings.Clear()
        For Each cmd In ChartCommands.GetCommands
            CommandBindings.Add(New CommandBinding(cmd, AddressOf command_executed))
        Next
        For Each item In Items
            If TypeOf item Is MenuItem Then
                For Each cmd As RoutedUICommand In ChartCommands.GetCommands
                    If CType(item, MenuItem).Command IsNot Nothing AndAlso CType(CType(item, MenuItem).Command, RoutedUICommand).Name = cmd.Name Then
                        CType(item, MenuItem).Command = Nothing
                        CType(item, MenuItem).Command = cmd
                        Exit For
                    End If
                Next
            End If
        Next
    End Sub
    Private Sub command_executed(ByVal sender As Object, ByVal e As ExecutedRoutedEventArgs)
        desktopWindow.Command_Executed(sender, e)
    End Sub
End Class
