Imports System.Runtime.InteropServices
Imports System.Windows.Interop

Public Class DesktopWindow
    Protected Overrides Sub OnInitialized(ByVal e As System.EventArgs)
        MyBase.OnInitialized(e)
        Visibility = Windows.Visibility.Visible
        'SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased)
    End Sub
    Private isActivated As Boolean
    <DebuggerStepThrough()>
    Private Sub Window_Activated(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Activated
        For Each item In Application.Current.Windows
            If TypeOf item Is LogWindow Then
                CType(item, LogWindow).Topmost = True
                Exit For
            End If
        Next
        isActivated = True
    End Sub
    <DebuggerStepThrough()>
    Private Sub Window_Deactivated(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Deactivated
        For Each item In Application.Current.Windows
            If TypeOf item Is LogWindow Then
                CType(item, LogWindow).Topmost = False
                Exit For
            End If
        Next
        isActivated = False
    End Sub

    Public Property ID As Integer = -1

    Public Sub SetScalingFactor(scale As Double)
        CType(grd.LayoutTransform, ScaleTransform).ScaleX = scale
        CType(grd.LayoutTransform, ScaleTransform).ScaleY = scale
    End Sub

    Private Sub Window_Loaded(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs) Handles Me.Loaded
        grd.Background = SystemColors.ControlBrush

        If ID = -1 Then
            Dim desktops As List(Of DesktopWindow) = My.Application.Desktops.ToList
            For i = 1 To desktops.Count + 1
                Dim contained As Boolean = False
                For Each desktop In desktops
                    If desktop.ID = i Then
                        contained = True
                    End If
                Next
                If Not contained Then
                    ID = i
                    Exit For
                End If
            Next
        End If
        For Each item In ChartCommands.GetCommands
            Dim binding As New CommandBinding(item, AddressOf Command_Executed)
            CommandBindings.Add(binding)
        Next
        If Environment.OSVersion.Version.Major >= 6 Then
            'ExtendGlass(Me, 0, topBar.ActualHeight, 0, 0)
            'grd.Background = Background
            'Background = Brushes.Transparent
        End If
    End Sub
    Dim askToSave As Boolean = True
    Private Sub Window_Closing(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles Me.Closing
        For Each workspace In Workspaces
            If workspace.Closing(askToSave) = False Then
                e.Cancel = True
                Exit For
            End If
        Next
        My.Application.MasterWindow.Desktops.Remove(Me)
        If Not e.Cancel Then If My.Application.MasterWindow IsNot Nothing AndAlso My.Application.Desktops.Count <= 1 Then My.Application.MasterWindow.Close()
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
        If Not GetIsParent(e.Source, topBar) Then topBar.Height = 0
        'CaptureMouse()
    End Sub
    Private Sub Window_PreviewMouseUp(ByVal sender As System.Object, ByVal e As System.Windows.Input.MouseButtonEventArgs)
        'ReleaseMouseCapture()
        _isMouseDown = False
        For Each chart In Charts
            chart.Window_MouseButtonUp(sender, e)
        Next
    End Sub
    Private Sub Window_PreviewMouseDoubleClick(ByVal sender As Object, ByVal e As System.Windows.Input.MouseButtonEventArgs) Handles Me.PreviewMouseDoubleClick
        For Each chart In Charts
            chart.Window_MouseDoubleClick(sender, e)
        Next
    End Sub
    Private Sub Window_PreviewKeyDown(ByVal sender As Object, ByVal e As System.Windows.Input.KeyEventArgs) Handles Me.PreviewKeyDown
        If e.SystemKey = Key.F4 And Keyboard.Modifiers = ModifierKeys.Alt Then e.Handled = True
        If e.Key = Key.LeftAlt Or e.Key = Key.RightAlt Or e.SystemKey = Key.LeftAlt Or e.SystemKey = Key.RightAlt Then
            Update()
            topBar.Height = 23
        End If
        Dispatcher.BeginInvoke(
            CType(Sub(s As Object, ea As KeyEventArgs)
                      For Each chart In Charts
                          chart.Window_KeyDown(Me, ea)
                      Next
                  End Sub, KeyEventHandler), sender, e)
    End Sub
    Private Sub Window_PreviewKeyUp(ByVal sender As Object, ByVal e As System.Windows.Input.KeyEventArgs) Handles Me.PreviewKeyUp
        Dispatcher.BeginInvoke(
            CType(Sub(s As Object, ea As KeyEventArgs)
                      For Each chart In Charts
                          chart.Window_KeyUp(Me, ea)
                      Next
                  End Sub, KeyEventHandler), sender, e)
    End Sub
    Private Sub Window_PreviewMouseWheel(ByVal sender As Object, ByVal e As System.Windows.Input.MouseWheelEventArgs) Handles Me.PreviewMouseWheel
        Dim goAhead As Boolean = False
        For Each Chart In Charts
            If GetIsParent(e.OriginalSource, Chart) Then
                goAhead = True
                Exit For
            End If
        Next
        If goAhead = False Then
            If e.Delta < 0 Then
                ChartCommands.NextWorkspace.Execute(Nothing, Me)
            Else
                ChartCommands.PreviousWorkspace.Execute(Nothing, Me)
            End If
        End If
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
        If isActivated Then
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
                Dim desktop As New DesktopWindow
                desktop.Show()
            ElseIf e.Command Is ChartCommands.FormatHotKeys Then
                Dim dialog As New ConfigureKeysWindow
                dialog.Owner = Me
                dialog.ShowDialog()
                Dim i As Integer
                For Each item In ChartCommands.GetCommands
                    CommandBindings(i).Command = item
                    i += 1
                Next
            ElseIf e.Command Is ChartCommands.ShowReplayWindow Then
                Dim alreadyOpen As Boolean = False
                Dim existingReplayWin As ReplayWindow = Nothing
                For Each win In My.Application.Windows
                    If TypeOf win Is ReplayWindow Then
                        alreadyOpen = True
                        existingReplayWin = win
                    End If
                Next
                If Not alreadyOpen Then
                    Dim replayWin As New ReplayWindow(My.Application.MasterWindow)
                    replayWin.Show()
                Else
                    existingReplayWin.Close()
                End If
            ElseIf e.Command Is ChartCommands.Format AndAlso ((IsNumeric(e.Parameter) AndAlso e.Parameter <> 0) OrElse Workspaces.Count = 0) Then
                Dim win As New FormatWindow()
                win.tabMain.Items.RemoveAt(0)
                win.tabMain.Items.RemoveAt(0)
                win.tabMain.Items.RemoveAt(0)
                win.tabMain.SelectedIndex = 0
                win.Owner = Me
                win.ShowDialog()
            ElseIf e.Command Is ChartCommands.NewWorkspace Then
                Dim newWorkspaceWindow As New NewWorkspaceWindow
                newWorkspaceWindow.Owner = Me
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
                        LoadWorkspace(file)
                    Next
                End If
            ElseIf e.Command Is ChartCommands.CloseWorkspace Then
                If CurrentWorkspace IsNot Nothing Then
                    For Each chart In CurrentWorkspace.Charts
                        chart.RemoveDataStreamHandlers()
                    Next
                    If CurrentWorkspace.Closing(True) = True Then
                        grd.Children.Remove(CurrentWorkspace)
                    End If
                End If
            ElseIf e.Command Is ChartCommands.FormatWorkspace Then
                If CurrentWorkspace IsNot Nothing Then
                    Dim newWorkspaceWindow As New NewWorkspaceWindow
                    newWorkspaceWindow.IsNewWorkspaceDialog = False
                    newWorkspaceWindow.WorkspaceName = CurrentWorkspace.WorkspaceName
                    newWorkspaceWindow.WorkspaceDescription = CurrentWorkspace.WorkspaceDescription
                    newWorkspaceWindow.WorkspaceFilepath = CurrentWorkspace.FilePath
                    newWorkspaceWindow.Owner = Me
                    newWorkspaceWindow.ShowDialog()
                    If newWorkspaceWindow.ButtonOKPressed Then
                        CurrentWorkspace.WorkspaceName = newWorkspaceWindow.WorkspaceName
                        CurrentWorkspace.WorkspaceDescription = newWorkspaceWindow.WorkspaceDescription
                        'IO.File.Delete(CurrentWorkspace.FilePath)
                        CurrentWorkspace.FilePath = newWorkspaceWindow.WorkspaceFilepath
                        CurrentWorkspace.Save()
                    End If
                End If
            ElseIf e.Command Is ChartCommands.NextWorkspace Then
                If CurrentWorkspace IsNot Nothing Then
                    Dim workspace As Workspace = CurrentWorkspace
                    grd.Children.Remove(workspace)
                    grd.Children.Insert(0, workspace)
                End If
            ElseIf e.Command Is ChartCommands.PreviousWorkspace Then
                If CurrentWorkspace IsNot Nothing Then
                    Dim workspace As Workspace = grd.Children(0)
                    grd.Children.Remove(workspace)
                    grd.Children.Add(workspace)
                End If
            ElseIf e.Command Is ChartCommands.SwitchChart Then
                Dim workspace As Workspace = CurrentWorkspace
                If workspace IsNot Nothing Then
                    topBar.Height = 0
                    For i = 0 To workspace.Children.Count - 1
                        If CType(workspace.Children(i), Chart).IsSelected Then
                            If i = workspace.Children.Count - 1 Then
                                CType(workspace.Children(0), Chart).IsSelected = True
                                Exit For
                            Else
                                CType(workspace.Children(i + 1), Chart).IsSelected = True
                                Exit For
                            End If
                        End If
                    Next
                End If
            ElseIf e.Command Is ChartCommands.SaveWorkspace Then
                If CurrentWorkspace IsNot Nothing Then
                    CurrentWorkspace.Save()
                End If
            ElseIf e.Command Is ChartCommands.SaveWorkspaceAs Then
                If CurrentWorkspace IsNot Nothing Then
                    Dim fileDialog As New Forms.SaveFileDialog
                    fileDialog.CheckFileExists = False
                    fileDialog.CheckPathExists = True
                    fileDialog.Filter = "Workspace (*.ws)|*.ws"
                    If fileDialog.ShowDialog = Forms.DialogResult.OK Then
                        CurrentWorkspace.WorkspaceName = IO.Path.GetFileNameWithoutExtension(fileDialog.FileName)
                        CurrentWorkspace.FilePath = fileDialog.FileName
                        CurrentWorkspace.Save()
                    End If
                End If
                'ElseIf e.Command Is ChartCommands.NewAnalysisTechnique Then
                '    Dim editor As New CodeEditorWindow
                '    editor.Show()
                '    editor.AddTab()
                '    editor.NewDoc("AnalysisTechnique1", "VB")
                'ElseIf e.Command Is ChartCommands.OpenAnalysisTechnique Then
                '    Dim editor As New CodeEditorWindow
                '    editor.Show()
                '    editor.OpenDoc()
            ElseIf e.Command Is ChartCommands.CloseApplication Then
                For Each desktop In My.Application.Desktops
                    desktop.askToSave = False
                Next
                My.Application.MasterWindow.CloseApplication()
            ElseIf e.Command Is ChartCommands.SaveCurrentDesktopLayout Then
                My.Application.MasterWindow.SaveDesktopLayouts()
            ElseIf e.Command Is ChartCommands.LoadSymbolChartGroup Then
                Dim win As New LoadSymbolGroupWindow(My.Application.MasterWindow.IB)
                win.ShowDialog()
                If win.buttonOKPressed Then
                    chartLoadingList = New List(Of Chart)

                    Dim c As IBApi.ContractDetails = win.SelectedContractDetails
                    Dim workspace As New Workspace
                    workspace.WorkspaceName = c.Contract.Symbol & "-" & Now.ToShortTimeString
                    workspace.FilePath = WORKSPACE_FOLDER_WITH_SLASH & workspace.WorkspaceName & ".ws"
                    grd.Children.Add(workspace)

                    Dim barSize As BarSize
                    Dim daysBack As Integer
                    Select Case win.SelectedGroupTimeFrame
                        Case LoadSymbolGroupWindow.SymbolGroupTimeFrame._300Days
                            barSize = BarSize.ThirtyMinutes
                            daysBack = 300

                        Case LoadSymbolGroupWindow.SymbolGroupTimeFrame._60Days
                            barSize = BarSize.OneMinute
                            daysBack = 60
                    End Select
                    chartLoadingList.Add(CreateDefaultChart(workspace, c, daysBack, barSize))
                    DispatcherHelper.DoEvents()
                    For Each chart In chartLoadingList
                        AddHandler chart.ChartLoaded, AddressOf Chart_AllDataLoaded
                    Next
                    Dispatcher.BeginInvoke(
                        Sub()
                            chartLoadingList(0).RequestData(TimeSpan.FromDays(chartLoadingList(0).DataStream.DaysBack(chartLoadingList(0).TickerID)), TimeSpan.FromSeconds(0), False)
                        End Sub, Windows.Threading.DispatcherPriority.Background)
                End If
            End If
        End If
        grd.Focusable = True
        grd.Focus()
        'mnuAbout.Focus()
    End Sub
    Private Sub Chart_AllDataLoaded(sender As Object, e As EventArgs)
        Dim chart As Chart = sender
        If chartLoadingList.IndexOf(chart) = 0 Then
            Static Dim secondTimeThrough As Boolean = False
            If secondTimeThrough = False Then
                secondTimeThrough = True
                Dim exp = 2 ' 1.735
                Dim a As Double = chart.Settings("RangeValue").Value ^ (-exp) * ((chart.bars(chart.bars.Count - 1).Data.Date - (chart.bars(0).Data.Date)).TotalSeconds / chart.bars.Count)
                Dim d As Double = chart.IB.DaysBack(chart.TickerID) * 24 * 3600 / chart.Settings("DesiredBarCount").Value
                Dim newRB As Double = (d / a) ^ (1 / exp)
                chart.Settings("RangeValue").Value = Round(newRB, chart.Settings("DecimalPlaces").Value)
                chart.RequestData(TimeSpan.FromDays(chart.DataStream.DaysBack(chart.TickerID)), TimeSpan.FromSeconds(0), False)
            End If
        End If
    End Sub
    Dim chartLoadingList As List(Of Chart)
    Public Function CreateDefaultChart(ws As Workspace, contractDetails As IBApi.ContractDetails, daysBack As Integer, barSize As BarSize) As Chart
        Dim chart As New Chart
        chart.Bounds = New Bounds(0, 0, 30, 30)
        ws.Children.Add(chart)
        chart.Load()
        chart.Settings("UseRandom").Value = False
        chart.Settings("RangeValue").Value = contractDetails.MinTick * daysBack * 4
        chart.IB.DaysBack(chart.TickerID) = daysBack
        chart.IB.BarSize(chart.TickerID) = barSize
        chart.IB.EnableReplayMode(chart.TickerID) = False
        chart.Settings("DesiredBarCount").Value = 800
        chart.Settings("OrderBarVisibility").Value = False
        chart.Settings("DecimalPlaces").Value = CInt(Max(Ceiling(-Log10(contractDetails.MinTick)), 0))
        chart.Settings("PriceBarTextInterval").Value = 2000
        chart.Settings("TimeBarTextInterval").Value = 2000

        'analysis techniques
        SetContractToContract(chart.IB.Contract(chart.TickerID), contractDetails.Contract)

        Return chart
    End Function
    Public Sub LoadWorkspace(ByVal filename As String)
        Dim opened As Boolean
        For Each item In Workspaces
            If item.FilePath = filename Then
                opened = True
            End If
        Next
        If Not opened Then
            If File.Exists(filename) Then
                Dim workspace As New Workspace
                If workspace.LoadFromFile(filename, Me) Then grd.Children.Add(workspace)
            Else
                ShowInfoBox("Could not find workspace file '" & filename & "'.", Me)
            End If
        End If
    End Sub
    Public Sub Update()
        If CurrentWorkspace IsNot Nothing Then
            Title = "Desktop " & ID & " - " & CurrentWorkspace.WorkspaceName
            lblTitle.Content = "Desktop" & ID & " - " & CurrentWorkspace.WorkspaceName
        Else
            Title = "QuickTrader - Desktop " & ID
            lblTitle.Content = "QuickTrader - Desktop" & ID
        End If
        Dim workspaceVisible As Boolean = CurrentWorkspace IsNot Nothing
        Dim chartVisible As Boolean = workspaceVisible AndAlso CurrentWorkspace.SelectedChart IsNot Nothing
        mnuCloseWorkspace.IsEnabled = workspaceVisible
        mnuFormatWorkspace.IsEnabled = workspaceVisible
        mnuNewChart.IsEnabled = workspaceVisible
        'mnuNewSlaveChart.IsEnabled = chartVisible
        mnuCloseChart.IsEnabled = chartVisible
        mnuDrawing.IsEnabled = chartVisible
        mnuFormatAnalysisTechniques.IsEnabled = chartVisible
        mnuFormatChart.IsEnabled = chartVisible
        Dim workspaces = Me.Workspaces
        mnuWorkspaces.Items.Clear()
        mnuCharts.Items.Clear()
        mnuWorkspaces.IsEnabled = False
        mnuCharts.IsEnabled = False
        If workspaces.Count > 0 Then
            mnuWorkspaces.IsEnabled = True
            For Each workspace In workspaces
                Dim mnu As MenuItem = NewMenuItemWrapper(workspace.WorkspaceName, "")
                If workspace Is CurrentWorkspace Then mnu.IsChecked = True
                mnu.Tag = workspace
                AddHandler mnu.Click,
                    Sub(sender As Object, e As EventArgs)
                        grd.Children.Remove(sender.Tag)
                        grd.Children.Add(sender.Tag)
                    End Sub
                mnuWorkspaces.Items.Add(mnu)
            Next
            Dim charts = CurrentWorkspace.Charts
            If charts.Count > 0 Then
                mnuCharts.IsEnabled = True
                For Each chart In charts
                    Dim mnu As MenuItem = NewMenuItemWrapper("Chart" & chart.GetChartName, "")
                    'If chart.Settings("IsSlaveChart").Value Then
                    'If chart.MasterChart IsNot Nothing Then
                    '    mnu.Header &= " (slave of Chart" & chart.MasterChart.GetChartName & ")"
                    'Else
                    'mnu.Header &= " (could not load master)"
                    'End If
                    'End If
                    If chart.IsSelected Then mnu.IsChecked = True
                    mnu.Tag = chart
                    AddHandler mnu.Click,
                        Sub(sender As Object, e As EventArgs)
                    sender.Tag.IsSelected = True
                End Sub
                    mnuCharts.Items.Add(mnu)
                Next
            End If
        End If
    End Sub

    Private Sub mnuAbout_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Dim win As New AboutWindow
        win.Owner = Me
        win.ShowDialog()
    End Sub

    Private Sub Dragger_MouseLeftButtonDown(ByVal sender As System.Object, ByVal e As System.Windows.Input.MouseButtonEventArgs)
        If e.LeftButton = MouseButtonState.Pressed And e.RightButton = MouseButtonState.Released Then DragMove()
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
        'If My.Application.Desktops.Count <= 1 Then My.Application.MasterWindow.Close()
        Close()
    End Sub
End Class
