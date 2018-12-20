Imports System.Collections.Specialized
Imports System.Threading.Tasks
Imports System.Threading

Class MasterWindow
    Protected Overrides Sub OnInitialized(ByVal e As System.EventArgs)
        MyBase.OnInitialized(e)
    End Sub

    Public totalMouseWheel As Integer
    Private _ib As New IBDataStream(New IBEventSource, New IBApi.EReaderMonitorSignal)
    Public IBorder As New IBDataStream(New IBEventSource, New IBApi.EReaderMonitorSignal)
    Public ReadOnly Property IB As IBDataStream
        Get
            Return _ib
        End Get
    End Property

    Public Sub CloseApplication()
        SaveDesktopLayouts()
        For Each desktop In Desktops
            desktop.Close()
        Next
    End Sub
    Public Property ChartIDsInUse As New List(Of Integer)
    Public Sub SaveDesktopLayouts()
        Dim dir As String = ApplicationSettingsFileLocation
        IO.File.WriteAllText(dir, "")
        Dim ids As New List(Of String)
        Dim desktops As List(Of DesktopWindow) = Me.Desktops
        Dim writer As New StreamWriter(dir)
        For Each desktop In desktops
            ids.Add(desktop.ID)
            Dim fileNames As New List(Of String)
            For Each workspace In desktop.Workspaces
                fileNames.Add(workspace.FilePath)
            Next
            writer.WriteLine("Desktop" & desktop.ID & "Maximized:" & (desktop.WindowState = Windows.WindowState.Maximized).ToString)
            writer.WriteLine("Desktop" & desktop.ID & "Workspaces:" & Join(fileNames.ToArray, "|"))
            writer.WriteLine("Desktop" & desktop.ID & "Rect:" & (New RectConverter).ConvertToString(New Rect(desktop.Left, desktop.Top, desktop.Width, desktop.Height)))
        Next
        writer.WriteLine("Desktops:" & Join(ids.ToArray, ","))
        writer.Close()
    End Sub
    Public Property ReplayPointsOfInterest As New List(Of Date)
    Private Sub Window_Loaded(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs) Handles Me.Loaded
        CType(Application.Current.FindResource("splashScreen"), SplashWindow).Close()

        If Not Directory.Exists("workspaces") Then Directory.CreateDirectory("workspaces")
        If Not Directory.Exists("settings") Then Directory.CreateDirectory("settings")
        If Not File.Exists(GlobalSettingsFileLocation) Then File.WriteAllText(GlobalSettingsFileLocation, "")
        If Not File.Exists(ApplicationSettingsFileLocation) Then File.WriteAllText(ApplicationSettingsFileLocation, "")
        Dim box = ShowInfoBoxAsync("Loading desktops...")
        Dim desktopIDs As List(Of String) = Split(ReadSetting(ApplicationSettingsFileLocation, "Desktops", ""), ",").ToList

        For Each id In desktopIDs
            If id <> "" Then
                Dim desktop As New DesktopWindow
                Dim rect As Rect = (New RectConverter).ConvertFromString(ReadSetting(ApplicationSettingsFileLocation, "Desktop" & id & "Rect", (New RectConverter).ConvertToString(New Rect(200, 200, 800, 800))))
                desktop.Left = rect.Left
                desktop.Top = rect.Top
                desktop.Width = rect.Width
                desktop.Height = rect.Height
                desktop.ID = id
                If ReadSetting(ApplicationSettingsFileLocation, "Desktop" & id & "Maximized", "False") = "True" Then desktop.WindowState = Windows.WindowState.Maximized
                Dim workspaces As List(Of String) = Split(ReadSetting(ApplicationSettingsFileLocation, "Desktop" & id & "Workspaces", ""), "|").ToList
                For Each fileName In workspaces
                    If fileName <> "" Then desktop.LoadWorkspace(fileName)
                Next
            End If
        Next
        If desktopIDs.Count = 0 OrElse (desktopIDs.Count = 1 AndAlso desktopIDs(0) = "") Then
            Dim desktop As New DesktopWindow
            desktop.Show()
        End If
        box.Close()
        Try
            IBorder.Connect(999)
        Catch ex As Exception
        End Try
        If Not IBorder.IsConnected Then
            ShowInfoBox("Could not connect to IB for separate feed ordering. A program restart is recommended.", Me)
        End If

        Dim result As Integer
        Dim b As New InfoBox("Auto load all charts?", Me, True, "Yes", "No", "Cancel")


        result = b.ButtonResultIndex
        AddHandler IB.EventSources.ConnectionClosed,
              Sub()
                  Dispatcher.Invoke(
                      Sub()
                          Log("lost connection to IB")
                      End Sub)
              End Sub
        If result = 0 Then
            For Each desktop In Desktops
                For Each workspace In desktop.Workspaces
                    For Each chart In workspace.Charts
                        chartLoadingList.Add(chart)
                    Next
                Next
            Next

            If chartLoadingList.Count > 0 Then

                Dim box2 = ShowInfoBoxAsync("Connecting to IB...")
                DispatcherHelper.DoEvents()
                Dim isConnected As Boolean = False

                Try
                    IB.Connect(chartLoadingList(0).TickerID)
                Catch ex As Exception

                End Try
                box2.Close()
                Log("connecting to IB...")
                DispatcherHelper.DoEvents()
                If chartLoadingList(0).IB.IsConnected Then
                    ShowTransientInfoBox("connected", 250)
                    Log("connected")
                    For Each chart In chartLoadingList
                        AddHandler chart.ChartLoaded, AddressOf Chart_AllDataLoaded
                    Next
                    chartLoadingList(0).RequestData(TimeSpan.FromDays(chartLoadingList(0).DataStream.DaysBack(chartLoadingList(0).TickerID)), (Now - chartLoadingList(0).DataStream.CurrentReplayTime), False)
                Else
                    Dim threads As New List(Of Thread)
                    For index = chartLoadingList.Count - 1 To 0 Step -1
                        Dim i As Integer = index
                        Dim syncSub1 = Sub()
                                           Dispatcher.BeginInvoke(
                                               Sub()
                                                   'SyncLock Me
                                                   chartLoadingList(i).RequestData(TimeSpan.FromDays(chartLoadingList(i).DataStream.DaysBack(chartLoadingList(i).TickerID)), (Now - chartLoadingList(i).DataStream.CurrentReplayTime), False)
                                                   'End SyncLock
                                               End Sub)
                                       End Sub
                        threads.Add(New Thread(syncSub1))
                    Next
                    For Each thread In threads
                        thread.Start()
                    Next
                    ShowTransientInfoBox("not connected", 250)
                    Log("not connected")
                End If
            End If
        Else
            _isInitialLoad = False
        End If
        'Dim replayWin As New ReplayWindow(Me)
        'replayWin.Show()
    End Sub
    Private _isInitialLoad As Boolean = True
    Public ReadOnly Property IsInitialLoad As Boolean
        Get
            Return _isInitialLoad
        End Get
    End Property
    Dim chartLoadingList As New List(Of Chart)
    Private Sub Chart_AllDataLoaded(sender As Object, e As EventArgs)
        Dim chart As Chart = sender
        If chartLoadingList.IndexOf(chart) <> chartLoadingList.Count - 1 Then
            Dim nextChart As Chart = chartLoadingList(chartLoadingList.IndexOf(chart) + 1)
            nextChart.RequestData(TimeSpan.FromDays(nextChart.DataStream.DaysBack(nextChart.TickerID)), (Now - nextChart.DataStream.CurrentReplayTime), False)
        Else
            For Each desktop In Desktops
                For Each workspace In desktop.Workspaces
                    For Each chart In workspace.Charts
                        RemoveHandler chart.ChartLoaded, AddressOf Chart_AllDataLoaded
                    Next
                Next
            Next
            _isInitialLoad = False
            Log("all charts have completed loading")
        End If
    End Sub
    Private Sub Window_Closing(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles Me.Closing
        My.Application.SaveSettings()
    End Sub
    Private Sub Window_Closed(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Closed
        End
    End Sub

    Public ReadOnly Property Desktops As List(Of DesktopWindow)
        Get
            Dim wins As New List(Of DesktopWindow)
            For Each item In Application.Current.Windows
                If TypeOf item Is DesktopWindow Then
                    wins.Add(item)
                End If
            Next
            Return wins
        End Get
    End Property
    Public ReadOnly Property Charts As List(Of Chart)
        Get
            Dim cs As New List(Of Chart)
            For Each d In Desktops
                For Each w In d.Workspaces
                    For Each c In w.Charts
                        cs.Add(c)
                    Next
                Next
            Next
            Return cs
        End Get
    End Property
End Class
