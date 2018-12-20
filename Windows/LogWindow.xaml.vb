Public Class LogWindow
    Public Sub Clear()
        txtLog.Text = ""
    End Sub
    Public Sub WriteLine(ByVal text As String)
        txtLog.Text &= text
        If Not mnuTurnOffTimeStamp.IsChecked Then txtLog.Text &= " @ " & Now.ToLongTimeString
        txtLog.Text &= vbNewLine
        txtLog.ScrollToEnd()
    End Sub

    Private Sub mnuTurnOffTimeStamp_CheckChanged(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        If mnuTurnOffTimeStamp.IsChecked And IsLoaded Then
            'For i = 0 To txtLog.LineCount - 1
            '    Dim lineText As String = txtLog.GetLineText(i)
            '    Dim charIndex As Integer = txtLog.GetCharacterIndexFromLineIndex(i)
            '    If lineText.Contains(" @ ") Then
            '        lineText = lineText.Substring(0, lineText.LastIndexOf(" @ "))
            '        txtLog.Text = txtLog.Text.Remove(charIndex, txtLog.GetLineLength(i))
            '        txtLog.Text = txtLog.Text.Insert(charIndex, lineText)
            '    End If
            'Next
        End If
    End Sub
    Private Sub mnuWordWrap_CheckChanged(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        If mnuWordWrap.IsChecked Then
            txtLog.TextWrapping = TextWrapping.Wrap
        Else
            txtLog.TextWrapping = TextWrapping.NoWrap
        End If
    End Sub
    Private Sub mnuClear_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        txtLog.Text = ""
    End Sub
    Private Sub mnuClose_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Close()
    End Sub

    Private Sub LogWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        mnuTurnOffTimeStamp.IsChecked = Boolean.Parse(GetSetting("QuickTrader", MyType, "TurnOffTimeStamp", "False"))
        mnuWordWrap.IsChecked = Boolean.Parse(GetSetting("QuickTrader", MyType, "WordWrap", "False"))
        txtLog.FontSize += 1
    End Sub

    Private Sub LogWindow_Closed(sender As Object, e As EventArgs) Handles Me.Closed
        SaveSetting("QuickTrader", MyType, "TurnOffTimeStamp", mnuTurnOffTimeStamp.IsChecked)
        SaveSetting("QuickTrader", MyType, "WordWrap", mnuWordWrap.IsChecked)
    End Sub
End Class
