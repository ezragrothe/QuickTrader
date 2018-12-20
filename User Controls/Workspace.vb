Imports System.Reflection
Imports System.Collections.ObjectModel

Public Class Workspace
    Inherits Canvas
    Public Sub New()

    End Sub
    Public Sub New(ByVal name As String)
        WorkspaceName = name
    End Sub
    Private Sub Workspace_Loaded(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs) Handles Me.Loaded
        HorizontalAlignment = Windows.HorizontalAlignment.Stretch
        VerticalAlignment = Windows.VerticalAlignment.Stretch
        Background = New SolidColorBrush(My.Application.WorkspaceBackgroundColor) ' New SolidColorBrush(ColorConverter.ConvertFromString("#FF5F5F5F")) ' Color.FromArgb(100, 255, 0, 255))
    End Sub
    Public Property WorkspaceDefaultTimeScale As Integer = 400
    Public Property WorkspaceDefaultRealBarHeight As Decimal = 15
    Public Property WorkspaceName As String = "workspace1"
    Public Property WorkspaceDescription As String = ""
    Public Property FilePath As String
    Public Function Closing(askToSave As Boolean) As Boolean
        For Each Chart In Children
            If TypeOf Chart Is Chart Then
                CType(Chart, Chart).DataStream.CancelHistoricalData(CType(Chart, Chart).TickerID)
                CType(Chart, Chart).DataStream.CancelMarketData(CType(Chart, Chart).TickerID)
            End If
        Next
        If askToSave Then
            Dim result As Integer = ShowInfoBox("Save workspace '" & WorkspaceName & "'?", GetFirstParentOfType(Me, GetType(Window)), "Yes", "No", "Cancel")
            If result = 0 Then
                Save()
            ElseIf result = 2 Then
                Return False
            End If
        Else
            Save()
        End If
        Return True
    End Function
    Dim chartLoadingList As List(Of Chart)
    Public Function LoadFromFile(ByVal filename As String, desktop As DesktopWindow) As Boolean
        FilePath = filename
        WorkspaceName = ReadSetting(FilePath, "WorkspaceName", WorkspaceName)
        WorkspaceDescription = CStr(ReadSetting(FilePath, "WorkspaceDescription", WorkspaceDescription.Replace(vbNewLine, Chr(1)))).Replace(Chr(1), vbNewLine)
        WorkspaceDefaultRealBarHeight = ReadSetting(FilePath, "WorkspaceDefaultRealBarHeight", WorkspaceDefaultRealBarHeight)
        WorkspaceDefaultTimeScale = ReadSetting(FilePath, "WorkspaceDefaultTimeScale", WorkspaceDefaultTimeScale)
        Dim chartNames() As String = Split(ReadSetting(FilePath, "Charts", ""), ",")
        'Dim box = ShowInfoBoxAsync("Loading workspace '" & WorkspaceName & "'...")
        chartLoadingList = New List(Of Chart)
        For Each item In chartNames
            If item <> "" Then
                If IsNumeric(item) Then
                    Dim chart As New Chart
                    Dim ids As New List(Of Integer)
                    For Each desktop In My.Application.Desktops
                        For Each workspace In desktop.Workspaces
                            For Each c As Chart In workspace.Charts
                                ids.Add(c.TickerID)
                            Next
                        Next
                    Next
                    'If Not ids.Contains(item) Then
                    chart.ChartOverrideTickerID = item
                    'End If
                    chartLoadingList.Add(chart)
                    Children.Add(chart)
                Else
                    ShowInfoBox("'" & WorkspaceName & "' is an outdated workspace and cannot be loaded.", GetFirstParentOfType(Me, GetType(Window)))
                    Return False
                End If
            End If
        Next
        'box.Close()
        OrderCharts()

        If My.Application.MasterWindow.IsInitialLoad = False Then
            Dim win As New InfoBox("Auto-load workspace charts?", desktop, True, "Yes", "No", "Cancel")
            If chartLoadingList.Count > 0 And win.ButtonResultIndex = 0 Then
                If Not My.Application.MasterWindow.IB.IsConnected Then
                    Log("connecting to IB...")
                    DispatcherHelper.DoEvents()
                    Try
                        My.Application.MasterWindow.IB.Connect(chartLoadingList(0).TickerID)
                    Catch ex As Exception
                        Log("could not connect")
                    End Try
                End If
                For Each chart In chartLoadingList
                    AddHandler chart.ChartLoaded, AddressOf Chart_AllDataLoaded
                Next
                DispatcherHelper.DoEvents()
                Dispatcher.BeginInvoke(
                    Sub()
                        chartLoadingList(0).RequestData(TimeSpan.FromDays(chartLoadingList(0).DataStream.DaysBack(chartLoadingList(0).TickerID)), (Now - chartLoadingList(0).DataStream.CurrentReplayTime), False)
                    End Sub, Windows.Threading.DispatcherPriority.Background)
            End If
        End If
        Return True
    End Function

    Private Sub Chart_AllDataLoaded(sender As Object, e As EventArgs)
        Dim chart As Chart = sender
        If chartLoadingList.IndexOf(chart) <> chartLoadingList.Count - 1 Then
            Dim nextChart As Chart = chartLoadingList(chartLoadingList.IndexOf(chart) + 1)
            nextChart.RequestData(TimeSpan.FromDays(nextChart.DataStream.DaysBack(nextChart.TickerID)), (Now - nextChart.DataStream.CurrentReplayTime), False)
        Else
            For Each chart In Me.Charts
                RemoveHandler chart.ChartLoaded, AddressOf Chart_AllDataLoaded
            Next
            Log("loading complete")
        End If
    End Sub
    Public Sub Delete()
        For Each Chart In Children
            If TypeOf Chart Is Chart Then
                CType(Chart, Chart).DataStream.CancelHistoricalData(CType(Chart, Chart).TickerID)
                CType(Chart, Chart).DataStream.CancelMarketData(CType(Chart, Chart).TickerID)
                CType(Chart, Chart).ResetSettings()
            End If
        Next
        ResetSetting(FilePath, "Charts", False)
        ResetSetting(FilePath, "WorkspaceName", False)
        ResetSetting(FilePath, "WorkspaceDescription")
        ResetSetting(FilePath, "WorkspaceDefaultRealBarHeight")
        ResetSetting(FilePath, "WorkspaceDefaultTimeScale")
        ' box.Close()
    End Sub
    Public Sub Save()
        'IO.File.Create(AppDomain.CurrentDomain.BaseDirectory & WORKSPACE_FOLDER_WITH_SLASH & WorkspaceName & ".ws")
        'If FilePath = "" Then FilePath = AppDomain.CurrentDomain.BaseDirectory & WORKSPACE_FOLDER_WITH_SLASH & WorkspaceName & ".ws"
        'Dim box = ShowInfoBoxAsync("Saving workspace '" & WorkspaceName & "'...")
        For Each Chart In Children
            If TypeOf Chart Is Chart Then
                CType(Chart, Chart).SaveSettings()
            End If
        Next
        Dim names As New List(Of String)
        For Each item In Children
            If TypeOf item Is Chart Then
                names.Add(CType(item, Chart).GetChartName)
            End If
        Next
        WriteSetting(FilePath, "Charts", Join(names.ToArray, ","), False)
        WriteSetting(FilePath, "WorkspaceName", WorkspaceName, False)
        WriteSetting(FilePath, "WorkspaceDescription", WorkspaceDescription.Replace(vbNewLine, Chr(1)), False)
        WriteSetting(FilePath, "WorkspaceDefaultRealBarHeight", WorkspaceDefaultRealBarHeight)
        WriteSetting(FilePath, "WorkspaceDefaultTimeScale", WorkspaceDefaultTimeScale)
        ' box.Close()

        Dim lines As New List(Of String)
        Using reader As New StreamReader(FilePath)
            While reader.Peek <> -1
                lines.Add(reader.ReadLine)
            End While
        End Using
        Dim i As Integer = 0
        While i < lines.Count
            Dim line As String = lines(i)
            If line.Contains(".") AndAlso IsNumeric(line.Substring(0, line.IndexOf("."))) Then
                If Not names.Contains(line.Substring(0, line.IndexOf("."))) Then
                    lines.RemoveAt(i)
                    i -= 1
                Else
                    For Each item In Children
                        If TypeOf item Is Chart Then
                            Dim chart As Chart = item
                            If chart.GetChartName = line.Substring(0, line.IndexOf(".")) Then
                                Dim ids As New List(Of String)
                                For Each technique In chart.AnalysisTechniques
                                    ids.Add(technique.AnalysisTechnique.Name & technique.ID)
                                Next
                                If line.IndexOf(".", line.IndexOf(".") + 1) <> -1 Then
                                    Dim name As String = line.Substring(line.IndexOf(".") + 1, line.IndexOf(".", line.IndexOf(".") + 1) - (line.IndexOf(".") + 1))
                                    If Not name.Contains(":") Then
                                        If ids.Contains(name) = False And name <> "IB" And name <> "RAND" Then
                                            lines.RemoveAt(i)
                                            i -= 1
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    Next
                End If
            End If
            i += 1
        End While
        Using writer As New StreamWriter(FilePath, False)
            For Each line In lines
                writer.WriteLine(line)
            Next
        End Using
    End Sub

    Public ReadOnly Property SelectedChart As Chart
        Get
            For Each item In Children
                If TypeOf item Is Chart AndAlso CType(item, Chart).IsSelected Then Return CType(item, Chart)
            Next
            Return Nothing
        End Get
    End Property
    Public ReadOnly Property Charts As ReadOnlyCollection(Of Chart)
        Get
            Dim returnVal As New List(Of Chart)
            For Each child In Children
                If TypeOf child Is Chart Then returnVal.Add(child)
            Next
            Return New ReadOnlyCollection(Of Chart)(returnVal)
        End Get
    End Property
    Public Sub OrderCharts()

        Dim charts As List(Of Chart) = Me.Charts.ToList
        For i = 1 To charts.Count
            Dim minIndex As Integer = Integer.MaxValue
            Dim minChart As Chart = Nothing
            For Each chart In charts
                If Canvas.GetZIndex(chart) < minIndex Then
                    minIndex = Canvas.GetZIndex(chart)
                    minChart = chart
                End If
            Next
            Canvas.SetZIndex(minChart, i)
            charts.Remove(minChart)
        Next
    End Sub

    Friend Sub Command_Executed(ByVal sender As Object, ByVal e As ExecutedRoutedEventArgs)
        Dim selected As Chart = SelectedChart
        If e.Command Is ChartCommands.NewChart Then
            Dim chart As New Chart
            chart.Bounds = New Bounds(0, 0, 30, 30)
            Children.Add(chart)
        ElseIf e.Command Is ChartCommands.LoadWorkspaceCharts Then
            chartLoadingList = New List(Of Chart)
            For Each c As Chart In Charts
                chartLoadingList.Add(c)
            Next
            If chartLoadingList.Count > 0 Then
                If Not My.Application.MasterWindow.IB.IsConnected Then
                    Log("connecting to IB...")
                    DispatcherHelper.DoEvents()
                    Try
                        My.Application.MasterWindow.IB.Connect(chartLoadingList(0).TickerID)
                    Catch ex As Exception
                        Log("could not connect")
                    End Try
                End If
                For Each chart In chartLoadingList
                    RemoveHandler chart.ChartLoaded, AddressOf Chart_AllDataLoaded
                    AddHandler chart.ChartLoaded, AddressOf Chart_AllDataLoaded
                Next
                DispatcherHelper.DoEvents()
                Dispatcher.BeginInvoke(
                    Sub()
                        chartLoadingList(0).RequestData(TimeSpan.FromDays(chartLoadingList(0).DataStream.DaysBack(chartLoadingList(0).TickerID)), (Now - chartLoadingList(0).DataStream.CurrentReplayTime), False)
                    End Sub, Windows.Threading.DispatcherPriority.Background)
            End If
        End If
    End Sub
    <DebuggerStepThrough()>
    Protected Overrides Sub OnVisualChildrenChanged(ByVal visualAdded As System.Windows.DependencyObject, ByVal visualRemoved As System.Windows.DependencyObject)
        If visualAdded Is Nothing OrElse TypeOf visualAdded Is Chart Then
            If visualAdded IsNot Nothing Then CType(visualAdded, Chart).IsSelected = True
            MyBase.OnVisualChildrenChanged(visualAdded, visualRemoved)
        Else
            Throw New ArgumentException("Workspace children can only be charts.")
        End If
    End Sub
End Class
