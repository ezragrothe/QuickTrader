Public Class UpdateHistoryWindow

    Private Sub Button_Click(sender As System.Object, e As System.Windows.RoutedEventArgs)
        Close()
    End Sub

    Private Sub BaseWindow_Loaded(sender As System.Object, e As System.Windows.RoutedEventArgs)
        Dim lines As New List(Of String)
        For Each prop In GetType(VersionInfo).GetProperties
            If prop.Name <> "Version" And prop.Name <> "CurrentVersionIndex" Then
                Dim lst As List(Of String) = prop.GetValue(Nothing, Nothing)
                If lines.Count > 0 Then lines.Add("")
                If prop.Name <> "VersionChanges" Then
                    lines.Add((New Date(CInt("201" & prop.Name.Substring(7, 1)), CInt(prop.Name.Substring(8, 2)), CInt(prop.Name.Substring(10, 2)))).ToLongDateString & " ----")
                Else
                    lines.Add((New Date(CInt("201" & Version.Substring(0, 1)), CInt(Version.Substring(2, 2)), CInt(Version.Substring(5, 2)))).ToLongDateString & " (current version) ----")
                End If
                For Each item In lst
                    lines.Add("    " & item)
                Next
            End If
        Next
        For Each line In lines
            txtText.Text &= line & vbNewLine
        Next
    End Sub
End Class
