Public Class ConfigureKeysWindow

    Dim commands As New List(Of String)
    Private Sub Window_Loaded(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        lstKey.ItemsSource = GetType(Key).GetEnumNames
        lstModifier.Items.Add("None")
        lstModifier.Items.Add("Alt")
        lstModifier.Items.Add("Control")
        lstModifier.Items.Add("Control+Alt")
        lstModifier.Items.Add("Shift")
        lstModifier.Items.Add("Shift+Alt")
        lstModifier.Items.Add("Control+Shift")
        lstModifier.Items.Add("Control+Shift+Alt")
        'lstModifier.ItemsSource = GetType(ModifierKeys).GetEnumNames
        commands = ArrayToList(Of String)(ChartCommands.GetCommandNames)
        For Each command As String In commands
            Dim item As New ListBoxItem
            Dim textBlock As New TextBlock(New Run(command))
            textBlock.HorizontalAlignment = Windows.HorizontalAlignment.Stretch
            textBlock.VerticalAlignment = Windows.VerticalAlignment.Stretch
            item.Content = textBlock
            lstCommands.Items.Add(item)
        Next
        txtSearch_LostFocus(txtSearch, New RoutedEventArgs)
    End Sub
    Private Sub Grid_PreviewKeyDown(ByVal sender As System.Object, ByVal e As System.Windows.Input.KeyEventArgs)
        If e.Source IsNot txtSearch And chkListen.IsChecked And e.Source IsNot txtDisplayText Then
            If Keyboard.Modifiers < 8 And Keyboard.Modifiers > 0 Then lstModifier.SelectedIndex = Keyboard.Modifiers Else lstModifier.SelectedIndex = 0
            If e.Key = Key.Escape Then
                lstCommands_SelectionChanged(lstCommands, Nothing)
            ElseIf e.Key <> Key.LeftAlt And e.Key <> Key.LeftCtrl And e.Key <> Key.LeftShift And e.Key <> Key.LWin And
                e.Key <> Key.RightAlt And e.Key <> Key.RightCtrl And e.Key <> Key.RightShift And e.Key <> Key.RWin Then
                lstKey.SelectedItem = [Enum].GetName(GetType(Key), e.Key)
            End If
            e.Handled = True
        End If
    End Sub

    Private commandTaken As RoutedUICommand = Nothing
    Private Sub ComboBox_SelectionChanged(ByVal sender As System.Object, ByVal e As System.Windows.Controls.SelectionChangedEventArgs)
        If lstKey.SelectedIndex > 0 Then
            If lstModifier.SelectedIndex > 0 Then
                txtDisplayText.Text = CStr(lstModifier.SelectedItem).Replace("Control", "Ctrl") & "+" & lstKey.SelectedItem
            Else
                txtDisplayText.Text = lstKey.SelectedItem
            End If
        Else
            txtDisplayText.Text = ""
        End If
        CheckForConflictingHotkey()
        btnAssign.IsEnabled = lstCommands.SelectedIndex <> -1
        btnAssign.Content = "Assign"
        btnAssign.IsDefault = True
        btnOK.IsDefault = False
    End Sub
    Private Sub CheckForConflictingHotkey()
        txtHotkeyTaken.Text = ""
        txtHotkeyTaken.Visibility = Windows.Visibility.Collapsed
        btnClearConflict.Visibility = Windows.Visibility.Collapsed
        commandTaken = Nothing
        For Each command As RoutedUICommand In ChartCommands.GetCommands
            If SelectedItem IsNot Nothing AndAlso command.Name <> SelectedItem.Replace(" ", "") Then
                For Each item In ChartCommands.GetSavedHotKey(command.Name)
                    If item.Key <> Key.None AndAlso item.Modifiers = lstModifier.SelectedIndex And [Enum].GetName(GetType(Key), item.Key) = lstKey.SelectedItem Then
                        commandTaken = command
                        txtHotkeyTaken.Text = "This hotkey is also used by '" & Spacify(command.Name) & "'."
                        txtHotkeyTaken.Visibility = Windows.Visibility.Visible
                        btnClearConflict.Visibility = Windows.Visibility.Visible
                        Exit For
                    End If
                Next
            End If
        Next
    End Sub
    Private Sub lstCommands_SelectionChanged(ByVal sender As System.Object, ByVal e As System.Windows.Controls.SelectionChangedEventArgs)
        If lstCommands.SelectedIndex = -1 Then
            _selectedItem = ""
        Else
            Dim content As TextBlock = lstCommands.SelectedItem.Content 'CType(CType(lstCommands.SelectedItem, ListBoxItem).Content, TextBlock)
            _selectedItem = (New TextRange(content.ContentStart, content.ContentEnd)).Text
        End If
        If SelectedItem <> "" Then
            btnAssign.IsEnabled = True
            CheckForConflictingHotkey()
            Dim command As RoutedUICommand = ChartCommands.GetCommandFromName(SelectedItem)
            lblInfo.Text = command.Text
            If ChartCommands.GetHotKey(command.Name).Count <> 0 Then
                lstModifier.SelectedIndex = ChartCommands.GetHotKey(command.Name)(0).Modifiers
                lstKey.SelectedItem = [Enum].GetName(GetType(Key), ChartCommands.GetHotKey(command.Name)(0).Key)
                txtDisplayText.Text = CType(command.InputGestures(0), KeyGesture).DisplayString
            Else
                lstModifier.SelectedIndex = 0
                lstKey.SelectedIndex = 0
                txtDisplayText.Text = ""
            End If
            If ChartCommands.GetHotKey(command.Name)(0).Modifiers = ModifierKeys.None And ChartCommands.GetHotKey(command.Name)(0).Key = Key.None Then
                btnAssign.Content = "Assign"
                btnAssign.IsEnabled = False
            Else
                btnAssign.IsDefault = False
                btnOK.IsDefault = True
                btnAssign.Content = "Clear"
                btnAssign.IsEnabled = True
            End If
        Else
            txtDisplayText.Text = ""
            txtHotkeyTaken.Visibility = Windows.Visibility.Collapsed
            lstKey.SelectedIndex = -1
            lstModifier.SelectedIndex = -1
            btnAssign.IsEnabled = False
        End If
    End Sub

    Private Sub txtDisplayText_TextChanged(ByVal sender As System.Object, ByVal e As System.Windows.Controls.TextChangedEventArgs)
        btnAssign.IsEnabled = True
        btnAssign.Content = "Assign"
        btnAssign.IsDefault = True
        btnOK.IsDefault = False
    End Sub

    Private Sub btnClearConflict_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        ChartCommands.AssignHotKey(commandTaken.Name, New KeyGesture(Key.None, ModifierKeys.None))
        CheckForConflictingHotkey()
    End Sub

    Private Sub btnAssign_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        If btnAssign.Content = "Clear" Then
            If SelectedItem <> "" Then
                ChartCommands.AssignHotKey(SelectedItem, New KeyGesture(Windows.Input.Key.None, ModifierKeys.None, ""))
                lstKey.SelectedItem = "None"
                lstModifier.SelectedItem = "None"
                btnAssign.IsEnabled = False
            Else
                ShowInfoBox("Please select a command.", Me)
            End If
            Exit Sub
        End If
        If txtHotkeyTaken.Text <> "" AndAlso ShowInfoBox(txtHotkeyTaken.Text & " Are you sure you want to continue?" & vbNewLine & "(this will assign two commands to the same hotkey)", Me, "Yes", "No") <> 0 Then
            Exit Sub
        End If
        Dim names As String() = [Enum].GetNames(GetType(Key))
        Dim keyValues As Key() = [Enum].GetValues(GetType(Key))
        Dim key As Object = Windows.Input.Key.None
        For i = 0 To names.Length - 1
            If names(i) = lstKey.SelectedItem Then
                key = keyValues(i)
                Exit For
            End If
        Next
        If SelectedItem <> "" Then
            Try
                ChartCommands.AssignHotKey(SelectedItem, New KeyGesture(key, lstModifier.SelectedIndex, txtDisplayText.Text))
                btnAssign.IsEnabled = False
                btnAssign.IsDefault = False
                btnOK.IsDefault = True
            Catch exc As NotSupportedException
                ShowInfoBox("'" & txtDisplayText.Text & "' is not a valid hotkey.", Me)
            End Try
        Else
            ShowInfoBox("Please select a command.", Me)
        End If
    End Sub

    Private Sub btnOK_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        'If btnAssign.Content = "Assign" And btnAssign.IsEnabled Then btnAssign_Click(btnAssign, e)
        Close()
    End Sub

    Private formattedText As Run
    Private Sub txtSearch_GotFocus(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        If txtSearch.Text = "[ search ]" Then
            txtSearch.Text = ""
            txtSearch.ClearValue(TextBox.BackgroundProperty)
            txtSearch.ClearValue(TextBox.ForegroundProperty)
            txtSearch.ClearValue(TextBox.CursorProperty)
        End If
    End Sub
    Private Sub txtSearch_LostFocus(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        If txtSearch.Text = "" Then
            txtSearch.Text = "[ search ]"
            txtSearch.Foreground = New SolidColorBrush(Colors.Gray)
            txtSearch.Background = sender.Parent.Background
            txtSearch.Cursor = Cursors.Arrow
        End If
    End Sub
    Private Sub txtSearch_TextChanged(ByVal sender As System.Object, ByVal e As System.Windows.Controls.TextChangedEventArgs)
        If IsLoaded And txtSearch.Text <> "[ search ]" Then
            lstCommands.Items.Clear()
            For Each command As String In commands
                Dim index As Integer = LCase(command).IndexOf(LCase(txtSearch.Text))
                Dim length As Integer = txtSearch.Text.Length
                If index <> -1 Then
                    Dim item As New ListBoxItem
                    Dim textBlock As New TextBlock()
                    textBlock.HorizontalAlignment = Windows.HorizontalAlignment.Stretch
                    textBlock.VerticalAlignment = Windows.VerticalAlignment.Stretch
                    textBlock.Inlines.Add(New Run(command.Substring(0, index)))
                    formattedText = New Run(command.Substring(index, length))
                    formattedText.Background = NewRadialGradientBrushWrapper(Colors.Yellow, Colors.Transparent, 0, 1, , 0.7, 0.5)
                    textBlock.Inlines.Add(formattedText)
                    textBlock.Inlines.Add(New Run(command.Substring(index + length)))
                    item.Content = textBlock
                    lstCommands.Items.Add(item)
                End If
            Next
        End If
    End Sub

    Private _selectedItem As String
    Private ReadOnly Property SelectedItem As String
        Get
            Return _selectedItem
        End Get
    End Property
End Class
