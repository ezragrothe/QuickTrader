Imports System.Collections.Specialized
Imports System.Threading.Tasks
Imports System.Threading

Public Class ReplayWindow
    Public Sub New(masterWin As MasterWindow)
        MasterWindow = masterWin
        AddHandler masterWin.IB.ReplaySpeedUpdated, AddressOf ReplaySpeedUpdated
        InitializeComponent()
    End Sub
    Public Property MasterWindow As MasterWindow

    Private Sub ReplayWindow_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        RemoveHandler MasterWindow.IB.ReplaySpeedUpdated, AddressOf ReplaySpeedUpdated
    End Sub

    Private Sub ReplayWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        txtDate.Value = Now
        If MasterWindow.IB.ReplaySpeedMultiplier > 1 Then
            sldSpeed.Value = (MasterWindow.IB.ReplaySpeedMultiplier - 1) / 40 + 1
        Else
            sldSpeed.Value = -(MasterWindow.IB.ReplaySpeed - 50) / (2 * ((Sqrt(30) + 5) * MasterWindow.IB.ReplaySpeed + 25))
        End If
        If MasterWindow.IB.IsReplayRunning And MasterWindow.IB.IsReplayEnabled Then UpdateStatus("running") Else UpdateStatus("not running")
    End Sub
    Dim syncingRealtime As Boolean = False
    Private SyncDate As Date? = Nothing
    Private Sub Chart_AllDataLoaded(sender As Object, e As EventArgs)
        Dim chart As Chart = sender
        If chartLoadingList.IndexOf(chart) <> chartLoadingList.Count - 1 Then
            Dim nextChart As Chart = chartLoadingList(chartLoadingList.IndexOf(chart) + 1)
            If chkSyncAll.IsChecked Or (nextChart.HasLoadedData And nextChart.IB.EnableReplayMode(nextChart.TickerID) = True) Then
                nextChart.IB.EnableReplayMode(nextChart.TickerID) = Not syncingRealtime
                If SyncDate Is Nothing OrElse (Now - SyncDate.Value) < TimeSpan.FromDays(nextChart.DataStream.DaysBack(nextChart.TickerID)) Then
                    nextChart.RequestData(TimeSpan.FromDays(nextChart.DataStream.DaysBack(nextChart.TickerID)), (Now - nextChart.DataStream.CurrentReplayTime), False)
                Else
                    Log("chart " & chartLoadingList(0).TickerID & " had to load extra data to cover replay date")
                    nextChart.RequestData((Now - nextChart.DataStream.CurrentReplayTime) + TimeSpan.FromDays(1), (Now - nextChart.DataStream.CurrentReplayTime), False)
                    Chart_AllDataLoaded(nextChart, Nothing)
                End If
            Else
                Chart_AllDataLoaded(nextChart, Nothing)
            End If
        Else
            For Each chart In MasterWindow.Charts.Concat(chartLoadingList)
                RemoveHandler chart.ChartLoaded, AddressOf Chart_AllDataLoaded
            Next
            Log("all charts have completed loading")
            UpdateStatus("not running")
        End If
    End Sub



    Private Sub btnSave_Click(sender As Object, e As RoutedEventArgs)
        'lstSavedPoints.Items.Add(New ListBoxItem(Now.ToShortDateString & ", " & Now.ToShortTimeString) With {.Tag = Now})
    End Sub
    'Private Sub btnDeleteSavedPoint_Click(sender As Object, e As RoutedEventArgs)
    '    If lstSavedPoints.SelectedItem IsNot Nothing Then
    '        lstSavedPoints.Items.Remove(lstSavedPoints.SelectedItem)
    '        MasterWindow.ReplayPointsOfInterest.Remove(lstSavedPoints.SelectedItem)
    '    End If
    'End Sub

    Private Sub btnJumpAhead_Click(sender As Object, e As RoutedEventArgs)
        MasterWindow.IB.JumpForwardInReplayTime(TimeSpan.FromMinutes(CInt(CType(sender, Button).Content)))
    End Sub

    'Private Sub sldHyperSpeed_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of System.Double))
    '    MasterWindow.IB.ReplaySpeedMultiplier = sldHyperSpeed.Value
    'End Sub
    Private Sub sldSpeed_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of System.Double))
        If IsLoaded Then
            If sldSpeed.Value <= 1 Then
                Dim newvalue As Double = 25 / (10 * sldSpeed.Value + Sqrt(30) - 5) - 5 * (Sqrt(30) - 5)
                newvalue = Ceiling(newvalue)
                If newvalue = 0 Then newvalue = 1
                MasterWindow.IB.ReplaySpeed = newvalue
                MasterWindow.IB.ReplaySpeedMultiplier = 1
            Else
                MasterWindow.IB.ReplaySpeed = 1
                MasterWindow.IB.ReplaySpeedMultiplier = Round((sldSpeed.Value - 1) * 40 + 1)
            End If
        End If
    End Sub
    Private Sub ReplaySpeedUpdated(newValue As Double)
        If MasterWindow.IB.IsReplayRunning Then grpSpeed.Header = "Current Replay Speed (" & Round(newValue, 1) & "x)" Else grpSpeed.Header = "Current Replay Speed (0x)"
        textCurrentTime.Text = MasterWindow.IB.CurrentReplayTime.ToLongDateString & " " & MasterWindow.IB.CurrentReplayTime.ToShortTimeString
        If MasterWindow.IB.IsReplayRunning Then txtDate.Value = MasterWindow.IB.CurrentReplayTime
    End Sub
    Private Sub txtDaysBack_GotFocus(sender As Object, e As RoutedEventArgs) Handles txtDaysBack.GotFocus
        radDaysBack.IsChecked = True
    End Sub
    Private Sub txtDate_GotFocus(sender As Object, e As RoutedEventArgs) Handles txtDate.GotFocus
        radDate.IsChecked = True
    End Sub
    Private Sub UpdateStatus(text As String)
        textStatus.Text = "Status: " & text
    End Sub
    Dim chartLoadingList As List(Of Chart)
    Private Sub btnSync_Click(sender As Object, e As RoutedEventArgs)
        SyncDate = Nothing
        If radDate.IsChecked Then
            If txtDate.Value.HasValue Then
                If txtDate.Value < Now Then
                    SyncDate = txtDate.Value
                End If
            End If
        ElseIf radDaysBack.IsChecked Then
            If IsNumeric(txtDaysBack.Text) AndAlso CDec(txtDaysBack.Text) > 0 Then
                SyncDate = Now - (TimeSpan.FromDays(txtDaysBack.Text))
            End If
            'ElseIf radSavedPoint.IsChecked Then
            '    If lstSavedPoints.SelectedItem IsNot Nothing Then
            '        SyncDate = lstSavedPoints.SelectedItem
            '    End If
        End If
        If SyncDate.HasValue Then
            UpdateStatus("syncing...")
            syncingRealtime = False
            MasterWindow.IB.IsReplayRunning = False
            chartLoadingList = New List(Of Chart)
            For Each chart In MasterWindow.Charts
                chartLoadingList.Add(chart)
            Next
            MasterWindow.IB.CurrentReplayTime = SyncDate.Value
            If chartLoadingList.Count > 0 Then
                For Each chart In chartLoadingList
                    RemoveHandler chart.ChartLoaded, AddressOf Chart_AllDataLoaded
                    AddHandler chart.ChartLoaded, AddressOf Chart_AllDataLoaded
                Next
                If chkSyncAll.IsChecked Or (chartLoadingList(0).HasLoadedData And chartLoadingList(0).IB.EnableReplayMode(chartLoadingList(0).TickerID) = True) Then
                    chartLoadingList(0).IB.EnableReplayMode(chartLoadingList(0).TickerID) = True
                    If (Now - SyncDate.Value) < TimeSpan.FromDays(chartLoadingList(0).DataStream.DaysBack(chartLoadingList(0).TickerID)) Then
                        chartLoadingList(0).RequestData(TimeSpan.FromDays(chartLoadingList(0).DataStream.DaysBack(chartLoadingList(0).TickerID)), (Now - MasterWindow.IB.CurrentReplayTime), False)
                    Else
                        Log("chart " & chartLoadingList(0).TickerID & " had to load extra data to cover replay date")
                        chartLoadingList(0).RequestData((Now - MasterWindow.IB.CurrentReplayTime) + TimeSpan.FromDays(1), (Now - MasterWindow.IB.CurrentReplayTime), False)
                        Chart_AllDataLoaded(chartLoadingList(0), Nothing)
                    End If
                Else
                    Chart_AllDataLoaded(chartLoadingList(0), Nothing)
                End If
            End If
        Else
            ShowInfoBox("Please enter a valid sync date.", Me)
        End If
    End Sub

    Private Sub btnStart_Click(sender As Object, e As RoutedEventArgs)
        UpdateStatus("running")
        MasterWindow.IB.IsReplayRunning = True
    End Sub

    Private Sub btnStop_Click(sender As Object, e As RoutedEventArgs)
        MasterWindow.IB.IsReplayRunning = False
        UpdateStatus("not running")
    End Sub

    Private Sub btnReturn_Click(sender As Object, e As RoutedEventArgs)
        chartLoadingList = New List(Of Chart)
        For Each chart In MasterWindow.Charts
            chartLoadingList.Add(chart)
        Next
        If chartLoadingList.Count > 0 Then
            UpdateStatus("syncing...")
            syncingRealtime = True
            MasterWindow.IB.IsReplayRunning = False
            MasterWindow.IB.CurrentReplayTime = Now
            For Each chart In chartLoadingList
                RemoveHandler chart.ChartLoaded, AddressOf Chart_AllDataLoaded
                AddHandler chart.ChartLoaded, AddressOf Chart_AllDataLoaded

            Next
            If chkSyncAll.IsChecked Or (chartLoadingList(0).HasLoadedData And chartLoadingList(0).IB.EnableReplayMode(chartLoadingList(0).TickerID) = True) Then
                chartLoadingList(0).IB.EnableReplayMode(chartLoadingList(0).TickerID) = False
                chartLoadingList(0).RequestData(TimeSpan.FromDays(chartLoadingList(0).DataStream.DaysBack(chartLoadingList(0).TickerID)), (Now - chartLoadingList(0).DataStream.CurrentReplayTime), False)
            Else
                Chart_AllDataLoaded(chartLoadingList(0), Nothing)
            End If
        End If
    End Sub

    Private Sub btnCancelSync_Click(sender As Object, e As RoutedEventArgs)
        If chartLoadingList IsNot Nothing Then
            For Each chart In MasterWindow.Charts.Concat(chartLoadingList)
                RemoveHandler chart.ChartLoaded, AddressOf Chart_AllDataLoaded
            Next
            UpdateStatus("unsynced")
        End If
    End Sub

End Class
