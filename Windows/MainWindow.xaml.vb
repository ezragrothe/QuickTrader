Imports Krs.Ats.IBNet
Imports System.Collections.Specialized


Class MainWindow
    Protected Overrides Sub OnInitialized(ByVal e As System.EventArgs)
        MyBase.OnInitialized(e)
        For Each item In ChartCommands.GetCommands
            Dim binding As New CommandBinding(item, AddressOf Command_Executed)
            CommandBindings.Add(binding)
        Next
        Visibility = Windows.Visibility.Visible
    End Sub

    Private Sub MainWindow_Activated(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Activated
        For Each item In Application.Current.Windows
            If TypeOf item Is LogWindow Then
                CType(item, LogWindow).Topmost = True
                Exit For
            End If
        Next
    End Sub
    Private Sub MainWindow_Deactivated(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Deactivated
        For Each item In Application.Current.Windows
            If TypeOf item Is LogWindow Then
                CType(item, LogWindow).Topmost = False
                Exit For
            End If
        Next
    End Sub
    Private _ib As New IBDataStream
    Public ReadOnly Property IB As IBDataStream
        Get
            Return _ib
        End Get
    End Property

    Private Sub MainWindow_Loaded(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs) Handles Me.Loaded
        Background = SystemColors.ControlBrush

        CType(Application.Current.FindResource("splashScreen"), SplashWindow).Close()

        If My.Application.ShowMenu Then
            mnuMain.Visibility = Windows.Visibility.Visible
        Else
            mnuMain.Visibility = Windows.Visibility.Collapsed
        End If
        Update()
        If Not Directory.Exists("workspaces") Then Directory.CreateDirectory("workspaces")
    End Sub
    Private Sub MainWindow_Closing(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles Me.Closing
        For Each workspace In Workspaces
            workspace.Save()
        Next
        WriteSetting(GLOBAL_CONFIG_FILE, "ShowMenu", My.Application.ShowMenu)
        WriteSetting(GLOBAL_CONFIG_FILE, "ShowMousePopups", My.Application.ShowMousePopups)
    End Sub
    Private Sub MainWindow_Closed(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Closed
        End
    End Sub

    Private Sub Window_PreviewMouseMove(ByVal sender As System.Object, ByVal e As System.Windows.Input.MouseEventArgs)
        Dim releaseMouse As Boolean = IsMouseDown And e.LeftButton = MouseButtonState.Released And e.RightButton = MouseButtonState.Released
        For Each chart In Charts
            If releaseMouse And chart.MouseDown <> MouseDownType.MouseUp Then
                Dim args As New MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left)
                chart.Window_MouseButtonUp(sender, New MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left))
            End If
            chart.Window_MouseMove(sender, e)
        Next
        If releaseMouse Then _isMouseDown = False
    End Sub
    Private Sub Window_PreviewMouseDown(ByVal sender As System.Object, ByVal e As System.Windows.Input.MouseButtonEventArgs)
        _isMouseDown = True
        For Each chart In Charts
            chart.Window_MouseButtonDown(sender, e)
        Next
        'CaptureMouse()
    End Sub
    Private Sub Window_PreviewMouseUp(ByVal sender As System.Object, ByVal e As System.Windows.Input.MouseButtonEventArgs)
        'ReleaseMouseCapture()
        _isMouseDown = False
        For Each chart In Charts
            chart.Window_MouseButtonUp(sender, e)
        Next
    End Sub
    Private Sub MainWindow_PreviewMouseDoubleClick(ByVal sender As Object, ByVal e As System.Windows.Input.MouseButtonEventArgs) Handles Me.PreviewMouseDoubleClick
        For Each chart In Charts
            chart.Window_MouseDoubleClick(sender, e)
        Next
    End Sub
    Private Sub MainWindow_PreviewKeyDown(ByVal sender As Object, ByVal e As System.Windows.Input.KeyEventArgs) Handles Me.PreviewKeyDown
        If e.SystemKey = Key.F4 And Keyboard.Modifiers = ModifierKeys.Alt Then e.Handled = True
        Dispatcher.BeginInvoke(
            CType(Sub(s As Object, ea As KeyEventArgs)
                For Each chart In Charts
                    chart.Window_KeyDown(Me, ea)
                Next
            End Sub, KeyEventHandler), sender, e)
    End Sub
    Private Sub MainWindow_PreviewKeyUp(ByVal sender As Object, ByVal e As System.Windows.Input.KeyEventArgs) Handles Me.PreviewKeyUp
        Dispatcher.BeginInvoke(
            CType(Sub(s As Object, ea As KeyEventArgs)
                For Each chart In Charts
                    chart.Window_KeyUp(Me, ea)
                Next
            End Sub, KeyEventHandler), sender, e)
    End Sub
    Private Sub MainWindow_PreviewMouseWheel(ByVal sender As Object, ByVal e As System.Windows.Input.MouseWheelEventArgs) Handles Me.PreviewMouseWheel
        For Each chart In Charts
            chart.Window_MouseWheel(Me, e)
        Next
    End Sub

    Private _isMouseDown As Boolean
    Public ReadOnly Property IsMouseDown As Boolean
        Get
            Return _isMouseDown
        End Get
    End Property
    Public ReadOnly Property Charts As List(Of Chart)
        Get
            Dim returnVal As New List(Of Chart)
            If grd.Children.Count > 0 Then
                For Each child In CurrentWorkspace.Children
                    If TypeOf child Is Chart Then returnVal.Add(child)
                Next
            End If
            Return returnVal
        End Get
    End Property
    Public ReadOnly Property Workspaces As List(Of Workspace)
        Get
            Dim lst As New List(Of Workspace)
            For Each workspace In grd.Children
                If TypeOf workspace Is Workspace Then
                    lst.Add(workspace)
                End If
            Next
            Return lst
        End Get
    End Property
    Public ReadOnly Property CurrentWorkspace As Workspace
        Get
            If grd.Children.Count > 0 Then
                Return CType(grd.Children(grd.Children.Count - 1), Workspace)
            Else
                Return Nothing
            End If
        End Get
    End Property
    Friend Sub Command_Executed(ByVal sender As Object, ByVal e As ExecutedRoutedEventArgs)
        'Dim t = Now
        If grd.Children.Count > 0 Then
            CurrentWorkspace.Command_Executed(sender, e)
            For Each item In CurrentWorkspace.Children
                If TypeOf item Is Chart AndAlso CType(item, Chart).IsSelected Then
                    CType(item, Chart).Command_Executed(sender, e)
                    Exit For
                End If
            Next
        End If
        'MsgBox((Now - t).TotalMilliseconds)
        If e.Command Is ChartCommands.CloseDesktop Then
            Close()
        ElseIf e.Command Is ChartCommands.NewDesktop Then
            Process.Start(Process.GetCurrentProcess().MainModule.FileName)
        ElseIf e.Command Is ChartCommands.FormatHotKeys Then
            Dim dialog As New ConfigureKeysWindow
            dialog.ShowDialog()
            Dim i As Integer
            For Each item In ChartCommands.GetCommands
                CommandBindings(i).Command = item
                i += 1
            Next
        ElseIf e.Command Is ChartCommands.Format AndAlso IsNumeric(e.Parameter) AndAlso e.Parameter <> 0 Then
            Dim win As New FormatWindow()
            win.tabMain.Items.RemoveAt(0)
            win.tabMain.Items.RemoveAt(0)
            win.tabMain.Items.RemoveAt(0)
            win.tabMain.SelectedIndex = 0
            win.ShowDialog()
        ElseIf e.Command Is ChartCommands.NewWorkspace Then
            Dim newWorkspaceWindow As New NewWorkspaceWindow
            newWorkspaceWindow.ShowDialog()
            If newWorkspaceWindow.ButtonOKPressed Then
                Dim workspace As New Workspace
                workspace.WorkspaceName = newWorkspaceWindow.WorkspaceName
                workspace.WorkspaceDescription = newWorkspaceWindow.WorkspaceDescription
                workspace.FilePath = newWorkspaceWindow.WorkspaceFilepath
                grd.Children.Add(workspace)
            End If
        ElseIf e.Command Is ChartCommands.OpenWorkspace Then
            Dim fileDialog As New Forms.OpenFileDialog
            fileDialog.CheckFileExists = True
            fileDialog.CheckPathExists = True
            fileDialog.Multiselect = True
            fileDialog.Filter = "Workspace (*.ws)|*.ws"
            If fileDialog.ShowDialog = Forms.DialogResult.OK Then
                For Each file In fileDialog.FileNames
                    Dim opened As Boolean
                    For Each item In Workspaces
                        If item.FilePath = file Then
                            opened = True
                        End If
                    Next
                    If Not opened Then
                        Dim workspace As New Workspace
                        workspace.LoadFromFile(file)
                        grd.Children.Add(workspace)
                    End If
                Next
            End If
        ElseIf e.Command Is ChartCommands.CloseWorkspace Then
            If CurrentWorkspace IsNot Nothing Then
                CurrentWorkspace.Save()
                grd.Children.Remove(CurrentWorkspace)
            End If
        ElseIf e.Command Is ChartCommands.FormatWorkspace Then
            If CurrentWorkspace IsNot Nothing Then
                Dim newWorkspaceWindow As New NewWorkspaceWindow
                newWorkspaceWindow.IsNewWorkspaceDialog = False
                newWorkspaceWindow.WorkspaceName = CurrentWorkspace.WorkspaceName
                newWorkspaceWindow.WorkspaceDescription = CurrentWorkspace.WorkspaceDescription
                newWorkspaceWindow.WorkspaceFilepath = CurrentWorkspace.FilePath
                newWorkspaceWindow.ShowDialog()
                If newWorkspaceWindow.ButtonOKPressed Then
                    CurrentWorkspace.WorkspaceName = newWorkspaceWindow.WorkspaceName
                    CurrentWorkspace.WorkspaceDescription = newWorkspaceWindow.WorkspaceDescription
                    'IO.File.Delete(CurrentWorkspace.FilePath)
                    CurrentWorkspace.FilePath = newWorkspaceWindow.WorkspaceFilepath
                    CurrentWorkspace.Save()
                End If
            End If
        ElseIf e.Command Is ChartCommands.SwitchWorkspace Then
            If CurrentWorkspace IsNot Nothing Then
                Dim workspace As Workspace = CurrentWorkspace
                grd.Children.Remove(workspace)
                grd.Children.Insert(0, workspace)
            End If
        ElseIf e.Command Is ChartCommands.SwitchChart Then
            If CurrentWorkspace IsNot Nothing Then
                For i = 0 To CurrentWorkspace.Children.Count - 1
                    If CType(CurrentWorkspace.Children(i), Chart).IsSelected Then
                        If i = CurrentWorkspace.Children.Count - 1 Then
                            CType(CurrentWorkspace.Children(0), Chart).IsSelected = True
                            Exit For
                        Else
                            CType(CurrentWorkspace.Children(i + 1), Chart).IsSelected = True
                            Exit For
                        End If
                    End If
                Next
            End If
        ElseIf e.Command Is ChartCommands.SaveWorkspace Then
            If CurrentWorkspace IsNot Nothing Then
                CurrentWorkspace.Save()
            End If
        ElseIf e.Command Is ChartCommands.NewAnalysisTechnique Then
            Dim editor As New CodeEditorWindow
            editor.Show()
            editor.AddTab()
            editor.NewDoc("AnalysisTechnique1", "VB")
        ElseIf e.Command Is ChartCommands.OpenAnalysisTechnique Then
            Dim editor As New CodeEditorWindow
            editor.Show()
            editor.OpenDoc()

        End If
        Update()
    End Sub

    Public Sub Update()
        If CurrentWorkspace IsNot Nothing Then
            Title = "Chartographer - " & CurrentWorkspace.WorkspaceName
        Else
            Title = "Chartographer"
        End If
        Dim workspaceVisible As Boolean = CurrentWorkspace IsNot Nothing
        Dim chartVisible As Boolean = workspaceVisible AndAlso CurrentWorkspace.SelectedChart IsNot Nothing
        mnuCloseWorkspace.IsEnabled = workspaceVisible
        mnuFormatWorkspace.IsEnabled = workspaceVisible
        mnuNewChart.IsEnabled = workspaceVisible
        mnuCloseChart.IsEnabled = chartVisible
        mnuDrawing.IsEnabled = chartVisible
        mnuFormatAnalysisTechniques.IsEnabled = chartVisible
        mnuFormatChart.IsEnabled = chartVisible
    End Sub

    Private Sub mnuAbout_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Dim win As New AboutWindow
        win.ShowDialog()
    End Sub

    Private Sub Dragger_MouseLeftButtonDown(ByVal sender As System.Object, ByVal e As System.Windows.Input.MouseButtonEventArgs)
        DragMove()
    End Sub
    Private Sub btnMinimize_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        WindowState = Windows.WindowState.Minimized
    End Sub
    Private Sub btnMaximize_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        If WindowState = Windows.WindowState.Maximized Then
            WindowState = Windows.WindowState.Normal
        Else
            WindowState = Windows.WindowState.Maximized
        End If
    End Sub
    Private Sub btnClose_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Close()
    End Sub
End Class
