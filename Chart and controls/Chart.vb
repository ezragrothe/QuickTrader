Imports System.Math
Imports System.Windows.Markup
Imports System.ComponentModel
Imports System.Windows.Threading
Imports System.IO
Imports System.Threading
Imports System.Collections.ObjectModel
Imports System.Reflection
Imports QuickTrader.AnalysisTechniques
Imports System.Windows.Controls.Primitives
Imports System.Text
Imports System.Collections.Specialized


<ContentProperty("Children")>
Public Class Chart
    Inherits Grid
    Implements ISelectable

#Region "Global"

    Public Sub New()
        InitSettings()
        InitBorder()
        InitAxis()
    End Sub

    Private tmrTimeBarUpdate As New DispatcherTimer()
    Private timeBarTotalSecondsFromMidnight As Integer
    Private _designMode As Boolean
    Public DesktopWindow As DesktopWindow = Nothing
    Public MasterWindow As MasterWindow = Nothing
    Private _chartOverrideTickerID As Integer = -1
    Public Property OriginalTickerID As Integer = -1
    Public Property ChartOverrideTickerID As Integer
        Get
            Return _chartOverrideTickerID
        End Get
        Set(ByVal value As Integer)
            _chartOverrideTickerID = value
            OriginalTickerID = value
            'ShowInfoBox("ChartOverrideName was set to '" & value & "'.", DesktopWindow)
        End Set
    End Property


    Public Sub Load()
        Static Dim hasLoaded As Boolean = False
        If hasLoaded Then Exit Sub
        AddHandler ChartKeyUp, AddressOf Me_KeyUp

        DesktopWindow = CType(GetParent(Me, GetType(DesktopWindow)), DesktopWindow)
        _designMode = Not TypeOf DesktopWindow Is DesktopWindow
        MasterWindow = My.Application.MasterWindow

        If Not IsDesignTime Then

            IB = MasterWindow.IB


            TickerID = GetNextTickerID()
            IB.BidAskReqID(TickerID) = GetNextTickerID()
            IB.MainIDFromBidAskID(IB.BidAskReqID(TickerID)) = TickerID
            IB.RealTimeReqID(TickerID) = GetNextTickerID()

            ResetBars()
            LoadSettings()
            LoadMenus()

            _chartOverrideTickerID = -1

            mainCanvas.Focus()
            If Settings("UseRandom").Value Then DataStream = RAND Else DataStream = IB

            ChangeSetting("ChartBorderWidth")

            'If Settings("IsSlaveChart").Value Then
            '    If MasterChart Is Nothing Then
            '        For Each chart As Chart In Parent.Children
            '            If chart.IsLoaded AndAlso chart.OriginalTickerID <> -1 AndAlso chart.OriginalTickerID = Settings("MasterChartTickerID").Value Then
            '                Settings("MasterChartTickerID").Value = chart.TickerID
            '                chart.SlaveCharts.Add(Me)
            '                MasterChart = chart
            '            End If
            '        Next
            '    End If
            'If OriginalTickerID <> -1 Then
            '    For Each chart As Chart In Parent.Children
            '        If chart.Settings("IsSlaveChart").Value AndAlso chart.MasterChart Is Nothing AndAlso chart.Settings("MasterChartTickerID").Value = OriginalTickerID Then
            '            chart.Settings("MasterChartTickerID").Value = TickerID
            '            SlaveCharts.Add(chart)
            '            chart.MasterChart = Me
            '        End If
            '    Next
            'End If
            Settings("PriceBarWidth").Value = 30
            'Dim allLoaded As Boolean = True
            'For Each chart In My.Application.Charts
            '    If Not chart.IsLoaded Then allLoaded = False
            'Next
            'If allLoaded Then
            '    For Each chart In My.Application.Charts
            '        chart.AllChartsLoaded()
            '    Next
            'End If
            'Dim ids As New List(Of Integer)
            'For Each desktop In My.Application.Desktops
            '    For Each workspace In desktop.Workspaces
            '        For Each c As Chart In workspace.Charts
            '            ids.Add(c.TickerID)
            '        Next
            '    Next
            'Next
            'If ids.Contains(If(_chartOverrideTickerID = -1, TickerID, _chartOverrideTickerID)) Then
            '    _chartOverrideTickerID = -1
            '    TickerID = GetNextTickerID()
            'End If
            tmrTimeBarUpdate.IsEnabled = True
            tmrTimeBarUpdate.Interval = TimeSpan.FromSeconds(1)
            AddHandler tmrTimeBarUpdate.Tick, AddressOf TimeBarTickUpdate
        End If
        hasLoaded = True
    End Sub

    Dim mnuViewOrderBar As MenuItem
    Private Sub LoadMenus()
        Dim menu As New CommandContextMenu(DesktopWindow)
        mnuViewOrderBar = NewMenuItemWrapper("View Order Pad", "mnuViewOrderBar")
        mnuViewOrderBar.IsCheckable = True
        mnuViewOrderBar.IsChecked = Settings("OrderBarVisibility").Value
        AddHandler mnuViewOrderBar.Checked,
            Sub()
                If bars.Count > 0 AndAlso Not Children.Contains(orderBox) Then Children.Add(orderBox)
                Settings("OrderBarVisibility").Value = True
            End Sub
        AddHandler mnuViewOrderBar.Unchecked,
            Sub()
                Children.Remove(orderBox)
                Settings("OrderBarVisibility").Value = False
            End Sub
        Dim scalingMenu As CommandMenuItem = NewMenuItemWrapper("Scaling", "")
        Dim scalingMenuItems() As CommandMenuItem = {
         NewMenuItemWrapper("Set Time Scale as Global Default", "", ChartCommands.SetTimeScaleAsGlobalDefault),
         NewMenuItemWrapper("Snap Time Scaling to Global Default", "", ChartCommands.SnapTimeScaleAsGlobalDefault),
         NewMenuItemWrapper("Set Time Scale as Workspace Default", "", ChartCommands.SetTimeScaleAsGlobalDefault),
         NewMenuItemWrapper("Snap Time Scaling to Workspace Default", "", ChartCommands.SnapTimeScaleAsGlobalDefault)
        }
        'NewMenuItemWrapper("Set Scaling as Preset 1", "", ChartCommands.SetScalingAsPreset1),
        'NewMenuItemWrapper("Set Scaling as Preset 2", "", ChartCommands.SetScalingAsPreset2),
        'NewMenuItemWrapper("Snap Scaling to Preset 1", "", ChartCommands.SnapScalingToPreset1),
        'NewMenuItemWrapper("Snap Scaling to Preset 2", "", ChartCommands.SnapScalingToPreset2)
        scalingMenu.ItemsSource = scalingMenuItems
        Dim menuItems() As CommandMenuItem = {
         NewMenuItemWrapper("Format...", "", ChartCommands.Format),
         NewMenuItemWrapper("Format Analysis Techniques...", "", ChartCommands.FormatAnalysisTechniques),
         NewMenuItemWrapper("Format Hotkeys...", "", ChartCommands.FormatHotKeys),
         scalingMenu, mnuViewOrderBar
        }
        'NewMenuItemWrapper("Format Workspace...", "", ChartCommands.FormatWorkspace),

        'If HasParent Then
        '    For Each menuItem In CType(Parent.Parent, Grid).ContextMenu.Items
        '        If TypeOf menuItem Is MenuItem Then
        '            Dim item As New MenuItem
        '            item.Header = menuItem.Header
        '            menu.Items.Add(item)
        '        End If
        '    Next
        'End If
        menu.ItemsSource = menuItems
        mainCanvas.ContextMenu = menu
    End Sub
    Private Sub LoadCompleted()
        Exit Sub
        DispatcherHelper.DoEvents()
        CancelMarketData()
        Dim i As Integer
        While i < Children.Count
            If TypeOf Children(i) Is Bar OrElse Children(i).AnalysisTechnique IsNot Nothing Then
                Children.RemoveAt(i)
            Else
                i += 1
            End If
        End While
        _orders.Clear()
        ResetBars()
        InitAxis()
        RefreshAxisSize()
        If DataStream.DaysBack(TickerID) <> 0 Then
            SetStatusText("loading...")
            DataStream.RequestHistoricalData(If(DataStream.UseReplayMode(TickerID), DataStream.CurrentReplayTime, Now), If(Not DataStream.UseReplayMode(TickerID), TimeSpan.FromDays(DataStream.DaysBack(TickerID)), TimeSpan.FromDays(DataStream.DaysBack(TickerID)) - (Now - DataStream.CurrentReplayTime)), TickerID)
        Else
            HistoricalDataFinished()
        End If


    End Sub
    Private Sub Chart_Loaded(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs) Handles Me.Loaded
        Load()
        LoadBorder()
        InitOrderBox()
        InitChartPad()
        LoadCompleted()
    End Sub


    Public Function GetNextTickerID() As Integer
        Dim id As Integer = 1
        While MasterWindow.ChartIDsInUse.Contains(id)
            id += 1
        End While
        MasterWindow.ChartIDsInUse.Add(id)
        Return id
    End Function
    'Public Function GetNextZIndex() As Integer
    '    Dim ids As New List(Of Integer)
    '    For Each chart As Chart In Parent.Children
    '        ids.Add(Canvas.GetZIndex(chart))
    '    Next
    '    Dim id As Integer = 0
    '    For id = 1 To Parent.Children.Count + 1
    '        If Not ids.Contains(id) Then Return id
    '    Next
    '    Return Parent.Children.Count + 1
    'End Function

#End Region

#Region "Analysis Techniques"
    Private analysisTechniquesLoading As Boolean
    Private WithEvents _analysisTechniques As New NotifyingCollection(Of AnalysisTechnique)

    Private Sub LoadAnalysisTechniques()
        Dim ids() As String = Split(ReadSetting(Parent.FilePath, GetChartName() & ".AnalysisTechniqueIDs", ""), ",")
        Dim i As Integer
        For Each item In Split(ReadSetting(Parent.FilePath, GetChartName() & ".AnalysisTechniques", ""), ";")
            If item <> "" Then
                LoadAnalysisTechnique(New AnalysisTechniqueInformation(item, InstantiateAnalysisTechnique(AnalysisTechniqueHelper.LoadAnalysisTechnique(item), Me), ids(i)))
            End If
            i += 1
        Next
    End Sub
    Private Sub LoadAnalysisTechnique(ByVal technique As AnalysisTechniqueInformation)
        If technique.AnalysisTechnique IsNot Nothing Then
            Dim name As String = GetChartName()
            For Each input In technique.AnalysisTechnique.Inputs
                Dim val = ReadSetting(Parent.FilePath, name & "." & technique.AnalysisTechnique.Name & technique.ID & "." & input.Name, ReadSetting(GLOBAL_CONFIG_FILE, technique.AnalysisTechnique.Name & "." & input.Name, input.StringValue))
                If input.Converter.IsValid(val) Then
                    input.Value = input.Converter.ConvertFromString(val)
                End If
            Next
            technique.AnalysisTechnique.IsEnabled = Boolean.Parse(ReadSetting(Parent.FilePath, name & "." & technique.AnalysisTechnique.Name & technique.ID & ".IsEnabled", "False"))
            If GetType(IChartPadAnalysisTechnique).IsAssignableFrom(technique.AnalysisTechnique.GetType) Then
                CType(technique.AnalysisTechnique, IChartPadAnalysisTechnique).ChartPadLocation = (New PointConverter).ConvertFromString(ReadSetting(Parent.FilePath, name & "." & technique.AnalysisTechnique.Name & technique.ID & ".ChartPadLocation", CType(technique.AnalysisTechnique, IChartPadAnalysisTechnique).ChartPadLocation.ToString))
            End If

            AnalysisTechniques.Add(technique)
        End If
    End Sub
    Public AnalysisTechniques As New List(Of AnalysisTechniqueInformation)
    Public Sub ApplyAnalysisTechniques()
        If statusText.Text <> "" Then SetStatusText("applying analysis techniques...")
        For Each item In AnalysisTechniques
            ApplyAnalysisTechnique(item.AnalysisTechnique)
        Next
        If statusText.Text <> "" Then SetStatusTextToDefault(False)
    End Sub
    Public Sub ApplyAnalysisTechnique(ByVal analysisTechnique As AnalysisTechnique)
        analysisTechniquesLoading = True
        'MsgBox("applying")
        Dim t = Now
        Dim timedOut As Boolean = False
        Dim percentCompleted As Double
        For i = 0 To bars.Count - 1
            analysisTechnique._isNewBar = True
            analysisTechnique.Update(bars(i).Data, True)
            analysisTechnique.UpdateNewBar()
            analysisTechnique._isNewBar = False
        Next
        'Log((Now - t).TotalMilliseconds & " / " & bars.Count)
        If timedOut Then
            RemoveAnalysisTechnique(analysisTechnique)
            ShowInfoBox("Execution timed out at " & percentCompleted * 100 & "% completion.", DesktopWindow)
        Else
            analysisTechnique.HistoryLoaded()
            analysisTechniquesLoading = False
            SetControlsOnTop()
        End If
        'MsgBox("finished")
        'ShowInfoBox(analysisTechnique.Name & " took an average of " & (Now - t).TotalMilliseconds / bars.Count & " milliseconds per bar")
        'RefreshChartChildren()
        'ChildrenChangedAxis(Nothing)

        UpdateChartPad()
    End Sub
    Public Sub ReApplyAnalysisTechnique(ByVal analysisTechnique As AnalysisTechnique)
        If analysisTechnique.IsEnabled Then
            If statusText.Text <> "" Then SetStatusText("applying analysis techniques...")
            RemoveAnalysisTechnique(analysisTechnique)
            ApplyAnalysisTechnique(analysisTechnique)
            If statusText.Text <> "" Then SetStatusTextToDefault(False)
        End If
    End Sub
    Public Sub ResetAnalysisTechniques()
        For Each analysisTechnique In AnalysisTechniques
            analysisTechnique.AnalysisTechnique.Reset()
        Next
    End Sub
    Public Sub RemoveAnalysisTechnique(ByVal analysisTechnique As AnalysisTechnique)
        'If statusAxisText.Content <> "" Then SetstatusText("removing analysis techniques...")
        Dim i As Integer
        While i < Children.Count
            If Children(i).AnalysisTechnique Is analysisTechnique Then
                Children.Remove(Children(i))
            Else
                i += 1
            End If
        End While
        analysisTechnique.Reset()
        If GetType(IChartPadAnalysisTechnique).IsAssignableFrom(analysisTechnique.GetType) Then
            Children.Remove(CType(analysisTechnique, IChartPadAnalysisTechnique).ChartPad)
        End If
        'If statusAxisText.Content <> "" Then SetstatusText(GetDefaultStatusText, False)
    End Sub
    Public Sub UpdateAnalysisTechniques(ByVal price As Decimal)
        If Not analysisTechniquesLoading Then
            For Each item In AnalysisTechniques
                item.AnalysisTechnique.Update(price)
            Next
        End If
    End Sub
    Public Sub UpdateAnalysisTechniquesForNewBar()
        If Not IsLoadingHistory Then
            For Each item In AnalysisTechniques
                item.AnalysisTechnique.UpdateNewBar()
            Next
        End If
    End Sub
    Public Sub AddAnalysisTechnique(ByVal analysisTechniqueInformation As AnalysisTechniqueInformation)
        analysisTechniqueInformation.ID = GetNextAnalysisTechniqueID()
        AnalysisTechniques.Add(analysisTechniqueInformation)
    End Sub
    Private Sub SaveAnalysisTechniques()
        Dim name As String = GetChartName()
        Dim names As New List(Of String)
        Dim ids As New List(Of String)
        For Each technique In AnalysisTechniques
            names.Add(technique.Identifier)
            ids.Add(technique.ID)
            For Each input In technique.AnalysisTechnique.Inputs
                WriteSetting(Parent.FilePath, name & "." & technique.AnalysisTechnique.Name & technique.ID & "." & input.Name, input.StringValue, False)
            Next
            WriteSetting(Parent.FilePath, name & "." & technique.AnalysisTechnique.Name & technique.ID & ".IsEnabled", technique.AnalysisTechnique.IsEnabled.ToString, False)
            If GetType(IChartPadAnalysisTechnique).IsAssignableFrom(technique.AnalysisTechnique.GetType) Then
                WriteSetting(Parent.FilePath, name & "." & technique.AnalysisTechnique.Name & technique.ID & ".ChartPadLocation", (New PointConverter).ConvertToString(CType(technique.AnalysisTechnique, IChartPadAnalysisTechnique).ChartPadLocation), False)
            End If
        Next
        WriteSetting(Parent.FilePath, name & ".AnalysisTechniques", Join(names.ToArray, ";"), True)
        WriteSetting(Parent.FilePath, name & ".AnalysisTechniqueIDs", Join(ids.ToArray, ","), True)
    End Sub

    Public Function RecompileExternalAnalysisTechnique(ByVal analysisTechnique As AnalysisTechniqueInformation) As AnalysisTechniqueInformation
        With analysisTechnique
            If .Identifier.Substring(0, 2) = "f-" Then

                Dim prev As AnalysisTechnique = .AnalysisTechnique
                Dim prevInputs As ReadOnlyCollection(Of Input) = prev.Inputs

                .AnalysisTechnique = InstantiateAnalysisTechnique(AnalysisTechniqueHelper.LoadAnalysisTechnique(.Identifier), Me)
                If .AnalysisTechnique Is Nothing Then
                    ShowInfoBox("Error: Could not compile " & prev.Name & ".", DesktopWindow)
                    Return Nothing
                End If
                .AnalysisTechnique.IsEnabled = prev.IsEnabled
                Dim inputs As List(Of Input) = .AnalysisTechnique.Inputs.ToList
                For i = 0 To inputs.Count - 1
                    For Each prevInput In prevInputs
                        If inputs(i).Name = prevInput.Name AndAlso inputs(i).Type = prevInput.Type Then
                            inputs(i).Value = prevInput.Value
                            Exit For
                        End If
                    Next
                    For Each prop In .AnalysisTechnique.GetType.GetProperties
                        For Each input In inputs
                            If input.Name = prop.Name Then
                                prop.SetValue(.AnalysisTechnique, input.Value, Nothing)
                                Exit For
                            End If
                        Next
                    Next
                Next
            End If
        End With
        Return analysisTechnique
    End Function

    Private Function GetNextAnalysisTechniqueID() As Integer
        Dim ids As New List(Of Integer)
        For Each item In AnalysisTechniques
            ids.Add(item.ID)
        Next
        Dim id As Integer = 0
        For id = 0 To AnalysisTechniques.Count + 1
            If Not ids.Contains(id) Then Return id
        Next
        Return AnalysisTechniques.Count + 1
    End Function

#End Region

#Region "Data Drawing"

    Friend WithEvents IB As IBDataStream
    Friend WithEvents IBorder As IBDataStream

    Friend WithEvents RAND As New RandomDataStream
    Friend WithEvents DataStream As DataStream

    Friend bars As New List(Of Bar)
    Private priceText As New TextBlock
    Friend IsLoadingHistory As Boolean
    Private IsDrawingBars As Boolean
    Private Delegate Sub MarketPriceHandler(ByVal ticktype As TickType, ByVal price As Double)
    Friend dontDrawBarVisuals As Boolean = False
    Private lastTickTimestamp As Date
    Friend Sub AddBarWrapper(ByVal o As Double, ByVal h As Double, ByVal l As Double, ByVal c As Double, Optional d As Date = Nothing, Optional drawBarVisuals As Boolean = True, Optional direction As Boolean = False)
        _isBarExtension = True
        Dim t As Date = If(d = Nothing, Now, d)
        Dim bar As New Bar(New BarData(o, h, l, c, t, TimeSpanFromBarSize(DataStream.BarSize(TickerID)), BarIndex + 1, direction))
        bar.DrawVisual = drawBarVisuals And (Not dontDrawBarVisuals)
        bar.AutoRefresh = False
        bar.Pen.Thickness = My.Application.BarWidth
        bar.Pen.Brush = New SolidColorBrush(Settings("Bar Color").Value)
        bar.OpenTick = Settings("DisplayOpenTick").Value
        bar.CloseTick = Settings("DisplayCloseTick").Value
        bar.TickWidth = My.Application.TickWidth

        'bar.IsSelectable = My.Application.ShowMousePopups
        If IsLoadingHistory And d = Nothing Then
            bar.Data = bar.Data.SetDate(historicalTime)
            If bars.Count > 0 Then bars(bars.Count - 1).Data = bars(bars.Count - 1).Data.SetDuration(historicalTime - bars(bars.Count - 1).Data.Date)
        Else
            bar.Data = bar.Data.SetDate(t)
            If bars.Count > 0 Then bars(bars.Count - 1).Data = bars(bars.Count - 1).Data.SetDuration(t - bars(bars.Count - 1).Data.Date)
        End If
        'If bars.Count > 0 Then
        '    bars(bars.Count - 1).ClearVisual()
        '    bars(bars.Count - 1).DrawVisual = False
        'End If
        bars.Add(bar)
        Children.Add(bar)
        UpdatePrice()
        If Not IsLoadingHistory Then UpdateAnalysisTechniquesForNewBar()
    End Sub
    Private Sub ResetBars()
        bars.Clear()
    End Sub

    Private Sub IB_Error(ByVal sender As Object, ByVal e As ErrMsgEventArgs) Handles IB.Error
        If e.id = TickerID Then
            If e.errorCode <> 366 Then
                If Not (e.errorCode = 1100 Or e.errorCode = 1102) Then
                    ShowInfoBox(e.errorMsg, DesktopWindow)
                    If IsLoadingHistory Then SetStatusText("error, try reloading")
                    IsLoadingHistory = False
                End If
            End If
        End If
    End Sub

    Private Sub TimeBarTickUpdate()
        If DataStream.BarSize(TickerID) = BarSize.OneDay Then
            If CInt(Now.TimeOfDay.TotalSeconds) = CInt(TimeSpan.FromHours(17).TotalSeconds) And recieveData And Settings("BarType").Value = BarTypes.TimeBars Then
                _isNewBar = True
                AddBarWrapper(CurrentBar.Close, CurrentBar.Close, CurrentBar.Close, CurrentBar.Close, Now, Not dontDrawBarVisuals)
            End If
        Else
            timeBarTotalSecondsFromMidnight = CInt(Now.TimeOfDay.TotalSeconds)
            If timeBarTotalSecondsFromMidnight Mod CInt(TimeSpanFromBarSize(DataStream.BarSize(TickerID)).TotalSeconds) = 0 And recieveData And Settings("BarType").Value = BarTypes.TimeBars Then
                _isNewBar = True
                AddBarWrapper(CurrentBar.Close, CurrentBar.Close, CurrentBar.Close, CurrentBar.Close, Now, Not dontDrawBarVisuals)
            End If
        End If
    End Sub

    Private recieveData As Boolean

    Friend Sub ib_realTimeBar(bar As BarData, ByVal id As Integer) Handles IB.RealTimeBarUpdate
        Dispatcher.Invoke(
         Sub()
             Dim masterID = GetMasterID()
             If (id = masterID Or masterID = -1) And bars.Count > 0 Then
                 'If TickerID = id Then Log(id & ": " & TickerID & ", " & bar.ToString)
                 If bar.High > CurrentBar.High Then
                     'Log("bar inaccuracy. original bar high: " & bars(maxPoint.X).Data.High & ", new bar high: " & bar.High)
                     Dim b = CeilingTo(bar.High, GetMinTick)
                     CalculatePriceChange(New BarData(b, b, b, b))
                 End If
                 If bar.Low < CurrentBar.Low Then
                     Dim b = FloorTo(bar.High, GetMinTick)
                     CalculatePriceChange(New BarData(b, b, b, b))
                 End If
             End If
         End Sub)
    End Sub
    Friend Sub ib_MarketPriceData(ByVal tickType As TickType, bar As BarData, ByVal id As Integer) Handles DataStream.MarketPriceData
        Dispatcher.Invoke(
             Sub()
                 Dim symbolMasterID As Integer = GetMasterID()

                 If (symbolMasterID = -1 And (id = TickerID Or id = IB.BidAskReqID(TickerID))) Or
                      (symbolMasterID <> -1 And (id = symbolMasterID Or id = IB.BidAskReqID(symbolMasterID))) Then

                     Dim isLoading As Boolean = False
                     For Each chart In My.Application.Charts
                         If chart.IsLoadingHistory Then
                             isLoading = True
                         End If
                     Next
                     If Not isLoading Then
                         MarketPrice(tickType, bar)
                         lastTickTimestamp = Now
                     End If

                 End If
             End Sub)
    End Sub
    Private Sub MarketPrice(ByVal ticktype As TickType, bar As BarData)
        If ticktype = TickType.LastPrice Then
            'Dim t = Now
            If recieveData Then
                If Settings("BarType").Value = BarTypes.RangeBars Then
                    CalculatePriceChange(bar)
                ElseIf Settings("BarType").Value = BarTypes.TimeBars Then
                    If bars.Count = 0 Then AddBarWrapper(bar.Open, bar.High, bar.Low, bar.Close, bar.Date)
                    currentBarLowPrice = Min(bar.Low, CurrentBar.Low)
                    currentBarHighPrice = Max(bar.High, CurrentBar.High)
                    Dim origLow = CurrentBar.Low, origHigh = CurrentBar.High
                    bars(bars.Count - 1).Data = CurrentBar.SetHigh(currentBarHighPrice)
                    bars(bars.Count - 1).Data = CurrentBar.SetLow(currentBarLowPrice)
                    bars(bars.Count - 1).Data = CurrentBar.SetClose(bar.Close)
                    If Not IsLoadingHistory Then CurrentBarLine.RefreshVisual()
                    If origLow <> CurrentBar.Low Or origHigh <> CurrentBar.High Then
                        _isBarExtension = True
                    Else
                        _isBarExtension = False
                    End If
                    _isNewBar = False
                    UpdatePrice()
                    'Static lastTickDivision As Long = Floor(Now.Ticks / TimeSpanFromBarSize(DataStream.BarSize(TickerID)).Ticks)
                    'If Floor(Now.Ticks / TimeSpanFromBarSize(DataStream.BarSize(TickerID)).Ticks) > lastTickDivision Then
                    'AddBarWrapper(price,
                    'End If
                ElseIf Settings("BarType").Value = BarTypes.VerticalSwingBars Then
                    CalculatePriceChange(bar)
                End If
            End If
            'Log((Now - t).TotalMilliseconds)
        ElseIf ticktype = TickType.BidPrice Then
            _bidPrice = bar.Close
            If bars.Count > 0 And Not IsLoadingHistory Then
                If Not analysisTechniquesLoading Then
                    For Each item In AnalysisTechniques
                        If TypeOf item.AnalysisTechnique Is BidAskPriceLines Then item.AnalysisTechnique.Update(CurrentBar.Close)
                    Next
                End If
            End If
        ElseIf ticktype = TickType.AskPrice Then
            _askPrice = bar.Close
            If bars.Count > 0 And Not IsLoadingHistory Then
                If Not analysisTechniquesLoading Then
                    For Each item In AnalysisTechniques
                        If TypeOf item.AnalysisTechnique Is BidAskPriceLines Then item.AnalysisTechnique.Update(CurrentBar.Close)
                    Next
                End If
            End If
        End If
    End Sub
    Private currentBarLowPrice As Double = Double.MaxValue, currentBarHighPrice As Double = Double.MinValue
    Private Delegate Sub CalculatePriceChangeHandler(ByVal bar As BarData)
    Private Sub AddBarForSlave(ByVal bar As BarData)
        AddBarWrapper(bar.Close, bar.Close, bar.Close, bar.Close)
        _isNewBar = True
    End Sub
    Friend Sub CalculatePriceChange(ByVal bar As BarData, Optional drawBarVisuals As Boolean = True)
        _isBarExtension = False
        Static once As Boolean = True
        If (once And dontDrawBarVisuals = False) Or BarIndex = 0 Then
            AddBarWrapper(bar.Close, bar.Close, bar.Close, bar.Close, , drawBarVisuals)
            ResetAnalysisTechniques()
        End If
        'AddBarWrapper(bar.Open, bar.High, bar.Low, bar.Close)
        'UpdateAnalysisTechniquesForNewBar()
        'once = False
        'IsLoadingHistory = False
        'Exit Sub
        If Settings("BarType").Value = BarTypes.RangeBars Then
            Dim rangeValue As Decimal = Settings("RangeValue").Value
            If (CurrentBar.High - bar.High) / rangeValue > 500 Then
                bars.Clear()
                AddBarWrapper(bar.Close, bar.Close, bar.Close, bar.Close, bar.Date, drawBarVisuals)
            End If
            If CurrentBar.High - bar.Low > rangeValue Or bar.High - CurrentBar.Low > rangeValue Then
                _isNewBar = True
                If bar.High > CurrentBar.High And bar.Low < CurrentBar.Low Then
                    If bar.Close > Avg(bar.Low, bar.High) Then
                        'white - down/up
                        If CurrentBar.High - bar.Low > rangeValue Then
                            bars(bars.Count - 1).Data = CurrentBar.SetLow(CurrentBar.High - rangeValue)
                            If Not IsLoadingHistory Then CurrentBarLine.RefreshVisual()
                            For Each fillBar In GetBarsToFillRange(CurrentBar.Low, bar.Low, rangeValue, Direction.Down)
                                AddBarWrapper(fillBar.Open, fillBar.High, fillBar.Low, fillBar.Close, bar.Date, drawBarVisuals)
                            Next
                            Dim bottomValue As Decimal = bar.Low
                            If CurrentBar.High - CurrentBar.Low < rangeValue Then
                                If bar.High - CurrentBar.Low > rangeValue Then
                                    bars(bars.Count - 1).Data = CurrentBar.SetHigh(CurrentBar.Low + rangeValue)
                                Else
                                    bars(bars.Count - 1).Data = CurrentBar.SetHigh(bar.High)
                                End If
                                If Not IsLoadingHistory Then CurrentBarLine.RefreshVisual()
                                bottomValue = CurrentBar.High
                            End If
                            For Each fillBar In GetBarsToFillRange(bottomValue, bar.High, rangeValue, Direction.Up)
                                AddBarWrapper(fillBar.Open, fillBar.High, fillBar.Low, fillBar.Close, bar.Date, drawBarVisuals)
                            Next
                        Else
                            If CurrentBar.High - bar.Low <= rangeValue Then
                                bars(bars.Count - 1).Data = CurrentBar.SetLow(bar.Low)
                            Else
                                bars(bars.Count - 1).Data = CurrentBar.SetLow(CurrentBar.High - rangeValue)
                                AddBarWrapper(CurrentBar.Low, bar.Low + rangeValue, bar.Low, bar.Low + rangeValue)
                            End If
                            bars(bars.Count - 1).Data = CurrentBar.SetHigh(bar.Low + rangeValue)
                            If Not IsLoadingHistory Then CurrentBarLine.RefreshVisual()
                            For Each fillBar In GetBarsToFillRange(bar.Low + rangeValue, bar.High, rangeValue, Direction.Up)
                                AddBarWrapper(fillBar.Open, fillBar.High, fillBar.Low, fillBar.Close, bar.Date, drawBarVisuals)
                            Next
                        End If
                    Else
                        'yellow - up/down
                        If bar.High - CurrentBar.High > rangeValue Then
                            bars(bars.Count - 1).Data = CurrentBar.SetHigh(CurrentBar.Low + rangeValue)
                            If Not IsLoadingHistory Then CurrentBarLine.RefreshVisual()
                            For Each fillBar In GetBarsToFillRange(CurrentBar.High, bar.High, rangeValue, Direction.Up)
                                AddBarWrapper(fillBar.Open, fillBar.High, fillBar.Low, fillBar.Close, bar.Date, drawBarVisuals)
                            Next
                            Dim topValue As Decimal = bar.High
                            If CurrentBar.High - CurrentBar.Low < rangeValue Then
                                If CurrentBar.High - bar.Low > rangeValue Then
                                    bars(bars.Count - 1).Data = CurrentBar.SetLow(CurrentBar.High - rangeValue)
                                Else
                                    bars(bars.Count - 1).Data = CurrentBar.SetLow(bar.Low)
                                End If
                                If Not IsLoadingHistory Then CurrentBarLine.RefreshVisual()
                                topValue = CurrentBar.Low
                            End If
                            For Each fillBar In GetBarsToFillRange(bar.Low, topValue, rangeValue, Direction.Down)
                                AddBarWrapper(fillBar.Open, fillBar.High, fillBar.Low, fillBar.Close, bar.Date, drawBarVisuals)
                            Next
                        Else
                            If bar.High - CurrentBar.Low <= rangeValue Then
                                bars(bars.Count - 1).Data = CurrentBar.SetHigh(bar.High)
                            Else
                                bars(bars.Count - 1).Data = CurrentBar.SetHigh(CurrentBar.Low + rangeValue)
                                AddBarWrapper(CurrentBar.High, bar.High, bar.High - rangeValue, bar.High - rangeValue, , drawBarVisuals)
                            End If
                            bars(bars.Count - 1).Data = CurrentBar.SetLow(bar.High - rangeValue)
                            If Not IsLoadingHistory Then CurrentBarLine.RefreshVisual()
                            For Each fillBar In GetBarsToFillRange(bar.High - rangeValue, bar.Low, rangeValue, Direction.Down)
                                AddBarWrapper(fillBar.Open, fillBar.High, fillBar.Low, fillBar.Close, bar.Date, drawBarVisuals)
                            Next
                        End If
                    End If
                ElseIf bar.High > CurrentBar.High Then
                    If bar.High > CurrentBar.Low + rangeValue Then
                        bars(bars.Count - 1).Data = CurrentBar.SetHigh(CurrentBar.Low + rangeValue)
                        If CurrentBar.Close > CurrentBar.High Then bars(bars.Count - 1).Data = CurrentBar.SetClose(CurrentBar.High)
                        If Not IsLoadingHistory Then CurrentBarLine.RefreshVisual()
                        For Each fillBar In GetBarsToFillRange(CurrentBar.High, bar.High, rangeValue, Direction.Up)
                            AddBarWrapper(fillBar.Open, fillBar.High, fillBar.Low, fillBar.Close, bar.Date, drawBarVisuals)
                        Next
                    Else
                        bars(bars.Count - 1).Data = CurrentBar.SetHigh(bar.High)
                        If Not IsLoadingHistory Then CurrentBarLine.RefreshVisual()
                    End If
                ElseIf bar.Low < CurrentBar.Low Then
                    If bar.Low < CurrentBar.High - rangeValue Then
                        bars(bars.Count - 1).Data = CurrentBar.SetLow(CurrentBar.High - rangeValue)
                        If CurrentBar.Close < CurrentBar.Low Then bars(bars.Count - 1).Data = CurrentBar.SetClose(CurrentBar.Low)
                        If Not IsLoadingHistory Then CurrentBarLine.RefreshVisual()
                        For Each fillBar In GetBarsToFillRange(CurrentBar.Low, bar.Low, rangeValue, Direction.Down)
                            AddBarWrapper(fillBar.Open, fillBar.High, fillBar.Low, fillBar.Close, bar.Date, drawBarVisuals)
                        Next
                    Else
                        bars(bars.Count - 1).Data = CurrentBar.SetLow(bar.Low)
                        If Not IsLoadingHistory Then CurrentBarLine.RefreshVisual()
                    End If
                End If
            Else
                currentBarLowPrice = Min(Min(bar.Low, CurrentBar.Low), bar.Close)
                currentBarHighPrice = Max(Max(bar.High, CurrentBar.High), bar.Close)
                Dim origLow = CurrentBar.Low, origHigh = CurrentBar.High
                bars(bars.Count - 1).Data = CurrentBar.SetHigh(currentBarHighPrice)
                bars(bars.Count - 1).Data = CurrentBar.SetLow(currentBarLowPrice)
                bars(bars.Count - 1).Data = CurrentBar.SetClose(bar.Close)
                If Not IsLoadingHistory Then CurrentBarLine.RefreshVisual()
                If origLow <> CurrentBar.Low Or origHigh <> CurrentBar.High Then _isBarExtension = True Else _isBarExtension = False
                _isNewBar = False
                UpdatePrice()
            End If
        ElseIf Settings("BarType").Value = BarTypes.TimeBars Then
            AddBarWrapper(bar.Open, bar.High, bar.Low, bar.Close, bar.Date, drawBarVisuals)
        ElseIf Settings("BarType").Value = BarTypes.VerticalSwingBars Then
            Dim ext As Boolean
            Dim newBar As Boolean
            If bars.Count = 1995 Then
                Dim a As New Object
            End If
            If (Round(bar.High - CurrentBar.Low, 5) >= Round(Settings("RangeValue").Value, 5) AndAlso CurrentBar.Direction = Direction.Down) Then
                bars(bars.Count - 1).Data = CurrentBar.SetClose(CurrentBar.Low)
                AddBarWrapper(CurrentBar.Low, bar.High, CurrentBar.Low, bar.Close, bar.Date, drawBarVisuals, Not CurrentBar.Direction)
                newBar = True
            ElseIf (Round(CurrentBar.High - bar.Low, 5) >= Round(Settings("RangeValue").Value, 5) AndAlso CurrentBar.Direction = Direction.Up) Then
                bars(bars.Count - 1).Data = CurrentBar.SetClose(CurrentBar.High)
                AddBarWrapper(CurrentBar.High, CurrentBar.High, bar.Low, bar.Close, bar.Date, drawBarVisuals, Not CurrentBar.Direction)
                newBar = True
            ElseIf (Round(bar.High, 5) >= Round(CurrentBar.High, 5) And CurrentBar.Direction = Direction.Up) Then
                bars(bars.Count - 1).Data = CurrentBar.SetHigh(bar.High)
                ext = True
                _isBarExtension = True
            ElseIf (Round(bar.Low, 5) <= Round(CurrentBar.Low, 5) And CurrentBar.Direction = Direction.Down) Then
                bars(bars.Count - 1).Data = CurrentBar.SetLow(bar.Low)
                ext = True
                _isBarExtension = True
            End If
            If Not newBar Then
                bars(bars.Count - 1).Data = CurrentBar.SetClose(bar.Close)
                If Not IsLoadingHistory Then CurrentBarLine.RefreshVisual()
                _isNewBar = False
                UpdatePrice()
            End If
        End If
        'System.Threading.Thread.Sleep(500)
        'DispatcherHelper.DoEvents()
        once = False
        If Not IsLoadingHistory AndAlso bars.Count > Settings("DesiredBarCount").Value + 200 And (Now - lastTickTimestamp).TotalSeconds > 2 Then
            Dim loadingHistory As Boolean = False
            For Each chart In My.Application.Charts
                If chart.IsLoadingHistory Then
                    loadingHistory = True
                End If
            Next
            If Not loadingHistory Then
                ordersToReplaceOnLoad = New List(Of OrderControl)
                For Each child In Children
                    If TypeOf child Is OrderControl Then
                        ordersToReplaceOnLoad.Add(child)
                    End If
                Next
                For Each item In ordersToReplaceOnLoad
                    Children.Remove(item)
                Next
                RequestData(TimeSpan.FromDays(DataStream.DaysBack(TickerID)), Now - DataStream.CurrentReplayTime, False)
            End If
        End If
    End Sub


    Private ordersToReplaceOnLoad As List(Of OrderControl)
    Private Function GetBarsToFillRange(point1 As Decimal, point2 As Decimal, barHeight As Decimal, direction As Direction) As List(Of BarData)
        If (Abs(point1 - point2) / barHeight) > 100 Then
            'ShowInfoBox("Warning! Maximum bar jump limit exceeded on chart " & GetChartName() & ". Please restart the progam.", DesktopWindow)
            Me.bars.Clear()
            Me.bars.Add(New Bar(New BarData(point2, point2, point2, point2)))
            Return New List(Of BarData)
            Dim a As New Object
        End If
        Dim lowPoint As Decimal = Min(point1, point2)
        Dim highPoint As Decimal = Max(point1, point2)
        Dim bars As New List(Of BarData)
        If direction = Direction.Down Then
            If highPoint - lowPoint >= barHeight Then
                For i = highPoint To lowPoint + barHeight Step -barHeight
                    bars.Add(New BarData(i, i, i - barHeight, i - barHeight))
                Next
            End If
            'If Not dontDrawBarVisuals Then
            Dim point As Decimal = (highPoint - lowPoint) Mod barHeight
            bars.Add(New BarData(lowPoint + point, lowPoint + point, lowPoint, lowPoint))
            'End If
        Else
            If highPoint - lowPoint >= barHeight Then
                For i = lowPoint To highPoint - barHeight Step barHeight
                    bars.Add(New BarData(i, i + barHeight, i, i + barHeight))
                Next
            End If
            'If Not dontDrawBarVisuals Then
            Dim point As Decimal = (highPoint - lowPoint) Mod barHeight
            bars.Add(New BarData(highPoint - point, highPoint, highPoint - point, highPoint))
            'End If
        End If
        Return bars
    End Function
    Private Sub CancelMarketData()
        recieveData = False
    End Sub
    Private Sub RequestMarketData()
        recieveData = True
    End Sub
    Friend Sub UpdatePrice()
        If Not IsLoadingHistory Then
            If Settings("UpdateAnalysisTechniquesEveryTick").Value Or IsNewBar Then
                If Not analysisTechniquesLoading Then
                    For Each item In AnalysisTechniques
                        If item.AnalysisTechnique.IsEnabled Then
                            If TypeOf item.AnalysisTechnique Is BidAskPriceLines Or IsBarExtension Then
                                item.AnalysisTechnique.Update(bars(bars.Count - 1).Data, False)
                            End If
                            item.AnalysisTechnique.UpdateIntraBar(Price)
                        End If
                    Next
                End If
            End If
            ''Log("1: " & (Now - t).TotalMilliseconds)
            If bars.Count Mod Settings("UpdateScalingBarInterval").Value = 0 Then
                    Dim barDif As Integer = Ceiling(BarIndex - (Bounds.X2 - GetRelativeFromRealWidth(Settings("XMargin").Value * mainCanvas.ActualWidth) - (GetRelativeFromReal(New Point(Settings("PriceBarWidth").Value, 0)).X - GetRelativeFromReal(New Point(0, 0)).X)))
                    Dim b As Bounds
                    If barDif <= 60 Then
                        b = New Bounds(Bounds.X1 + barDif, Bounds.Y1, Bounds.X2 + barDif, Bounds.Y2)
                        If b.ToWindowsRect <> Bounds.ToWindowsRect Then Bounds = b
                        ScaleRoutine(b)
                    End If

                    bars(BarIndex - 1).RefreshVisual()
                End If
                If Not IsLoadingHistory Then DrawPriceText()
                If True Then
                    For Each child In Children
                        If TypeOf child Is TrendLine AndAlso CType(child, TrendLine).IsRegressionLine AndAlso CType(child, TrendLine).LockToEnd Then
                            CType(child, TrendLine).RefreshVisual()
                        End If
                    Next
                End If
            End If
    End Sub

    Private Sub ScaleRoutine(Optional startBounds As Bounds? = Nothing)
        Dim height As Double = Bounds.Height
        Dim margin As Double = GetRelativeFromRealHeight(Settings("YMargin").Value * mainCanvas.ActualHeight)
        Dim timeBarHeight As Double = margin 'GetRelativeFromRealHeight(Settings("TimeBarHeight").Value)
        Dim centerTopMargin As Double = Avg(Bounds.Y1, Bounds.Y2) + GetRelativeFromRealHeight(Settings("CenterScalingOffsetHeight").Value * (ActualHeight / 2)) - margin
        Dim centerBottomMargin As Double = Avg(Bounds.Y1, Bounds.Y2) + GetRelativeFromRealHeight(Settings("CenterScalingOffsetHeight").Value * (ActualHeight / 2)) + margin
        'Dim topMargin As Double = newBounds.Y1 + margin
        'Dim bottomMargin As Double = newBounds.Y2 - margin
        Dim diff As Double = Double.MinValue
        Dim rightX As Integer = Min(Bounds.X2, bars.Count)
        Dim leftX As Integer = Max(Bounds.X1, 1)
        Dim topY As Double = -1
        Dim bottomY As Double = -1
        Dim bottomPoint As New Point(0, Double.MinValue)
        Dim topPoint As New Point(0, Double.MaxValue)
        Dim skipScaling As Boolean = False
        For Each item In AnalysisTechniques
            If TypeOf item.AnalysisTechnique Is ChartAlignment AndAlso CType(item.AnalysisTechnique, ChartAlignment).PrimaryTechnique IsNot Nothing Then
                skipScaling = True
                Exit For
            End If
        Next
        If Not skipScaling Then
            Dim newBounds As Bounds = If(startBounds IsNot Nothing, startBounds, Bounds)
            If -CurrentBar.High < centerTopMargin Then ' if price is above top of center margin
                newBounds = New Bounds(newBounds.X1, -CurrentBar.High - (centerTopMargin - newBounds.Y1), newBounds.X2, (-CurrentBar.High - (centerTopMargin - newBounds.Y1)) + height) ' set price to top of center margin
            ElseIf -CurrentBar.Low > centerBottomMargin Then ' if price is below bottom of center margin
                newBounds = New Bounds(newBounds.X1, (-CurrentBar.Low + (newBounds.Y2 - centerBottomMargin)) - height, newBounds.X2, -CurrentBar.Low + (newBounds.Y2 - centerBottomMargin)) ' set price to bottom of center margin
            End If
            For i = rightX To leftX Step -1 ' loop through all bars on chart
                If -bars(i - 1).Data.Low > bottomPoint.Y Then bottomPoint = New Point(i, -bars(i - 1).Data.Low) ' expand high
                If -bars(i - 1).Data.High < topPoint.Y Then topPoint = New Point(i, -bars(i - 1).Data.High) ' and low points
            Next
            If bottomPoint.Y >= newBounds.Y2 - timeBarHeight And topPoint.Y <= newBounds.Y1 + timeBarHeight Then ' if both high and low points of chart are above the bounds limits
                If bottomPoint.X > topPoint.X Then ' if bottom point is more recent than top point
                    newBounds = New Bounds(newBounds.X1, bottomPoint.Y + timeBarHeight - height, newBounds.X2, bottomPoint.Y + timeBarHeight) ' set bottom point to bottom bounds limit
                ElseIf bottomPoint.X < topPoint.X Then ' if top point is more recent than bottom point
                    newBounds = New Bounds(newBounds.X1, topPoint.Y - timeBarHeight, newBounds.X2, topPoint.Y - timeBarHeight + height) ' set top point to top bounds limit
                End If
            ElseIf bottomPoint.Y > newBounds.Y2 - timeBarHeight Then ' if bottom point is below bottom bounds limit
                newBounds = New Bounds(newBounds.X1, bottomPoint.Y + timeBarHeight - height, newBounds.X2, bottomPoint.Y + timeBarHeight) ' set bottom point to bottom bounds limit
                If topPoint.Y <= newBounds.Y1 + timeBarHeight And bottomPoint.X < topPoint.X Then ' if top point is above top bounds limit and top point is more recent than bottom point
                    newBounds = New Bounds(newBounds.X1, topPoint.Y - timeBarHeight, newBounds.X2, topPoint.Y - timeBarHeight + height) ' set top point to top bounds limit
                End If
            ElseIf topPoint.Y < newBounds.Y1 + timeBarHeight Then ' if top point is above top bounds limit
                newBounds = New Bounds(newBounds.X1, topPoint.Y - timeBarHeight, newBounds.X2, topPoint.Y - timeBarHeight + height) ' set top point to top bounds limit
                If bottomPoint.Y >= newBounds.Y2 - timeBarHeight And bottomPoint.X > topPoint.X Then ' if bottom point is below bottom bounds limit and bottom point is more recent than top point
                    newBounds = New Bounds(newBounds.X1, bottomPoint.Y + timeBarHeight - height, newBounds.X2, bottomPoint.Y + timeBarHeight) ' set bottom point to bottom bounds limit
                End If
            End If
            If -CurrentBar.High < newBounds.Y1 + margin Then ' if price is above top margin
                newBounds = New Bounds(newBounds.X1, -CurrentBar.High - margin, newBounds.X2, (-CurrentBar.High - margin) + height) ' set price to top margin
            ElseIf -CurrentBar.Low > newBounds.Y2 - margin Then ' if price is below bottom margin
                newBounds = New Bounds(newBounds.X1, (-CurrentBar.Low + margin) - height, newBounds.X2, -CurrentBar.Low + margin) ' set price to bottom margin
            End If
            If newBounds.ToWindowsRect <> Bounds.ToWindowsRect Then Bounds = newBounds
        End If
    End Sub

    Friend historicalBars As New List(Of BarData), historicalTime As Date
    Private Sub IB_HistoricalDataBarRecieved(ByVal bar As BarData, ByVal id As Integer) Handles DataStream.HistoricalDataBarRecieved
        If id = TickerID Then
            historicalBars.Add(bar)
        End If
    End Sub
    Private Sub IB_HistoricalDataPartiallyCompleted(ByVal id As Integer, ByVal percentCompleted As Double) Handles DataStream.HistoricalDataPartiallyCompleted
        Dispatcher.Invoke(
         Sub()
             If id = TickerID Then
                 loadingProgressBar.Value = percentCompleted
                 DispatcherHelper.DoEvents()
             End If
         End Sub)
    End Sub

    Private Sub HistoricalDataFinished()
        RefreshOrderBox()
        InitGrid()
        InitRandomDataControls()
        'InitReplayControls()
        Dim endBar As Integer = Round(BarIndex + GetRelativeFromRealWidth(Settings("XMargin").Value * mainCanvas.ActualWidth) + (GetRelativeFromReal(New Point(Settings("PriceBarWidth").Value, 0)).X - GetRelativeFromReal(New Point(0, 0)).X))
        Dim bnds As New Bounds(endBar - Bounds.X2 + Bounds.X1, Bounds.Y1, endBar, Bounds.Y2)
        TrySetBounds(bnds.TopLeft, bnds.BottomRight, True)
        SetStatusTextToDefault(True)
        RefreshAxisAndGrid()
        For i = 0 To savedChilds.Count - 1
            Dim closestBarDate As Date = Date.MinValue
            Dim closestBarNum As Integer
            For j = 0 To bars.Count - 1
                If bars(j).Data.Date - savedChildDates(i) < bars(j).Data.Date - closestBarDate Then
                    closestBarDate = bars(j).Data.Date
                    closestBarNum = bars(j).Data.Number
                End If
            Next
            'If TypeOf savedChilds(i) Is Arrow Then
            '    CType(savedChilds(i), Arrow).Location = New Point(closestBarNum, CType(savedChilds(i), Arrow).Location.Y)
            'Elseif
            If TypeOf savedChilds(i) Is OrderControl AndAlso CType(savedChilds(i), OrderControl).OrderStatus = OrderStatus.Filled Then
                CType(savedChilds(i), OrderControl).SetFillBar(closestBarNum)
            End If
            Children.Add(savedChilds(i))
        Next
        LoadDrawingObjects()
        Dim t = Now
        SetScaling(Bounds.Size)
        If Settings("UseRandom").Value = True Then
            Dim a As New Object
        End If
        If statusText.Text <> "" Then SetStatusText("applying analysis techniques...")
        For Each analysisTechnique In AnalysisTechniques
            ReApplyAnalysisTechnique(analysisTechnique.AnalysisTechnique)
        Next
        If statusText.Text <> "" Then SetStatusTextToDefault(False)
        IsLoadingHistory = False
        'MsgBox((Now - t).TotalMilliseconds)

        Dim symbolMasterID As Integer = GetMasterID()
        If symbolMasterID <> -1 Then
            DataStream.RequestMarketData(symbolMasterID)
        Else
            DataStream.RequestMarketData(TickerID)
        End If
        RequestMarketData()
        SetControlsOnTop()
        RefreshAllSize()
        If ordersToReplaceOnLoad IsNot Nothing Then
            For Each item In ordersToReplaceOnLoad
                Children.Add(item)
            Next
            ordersToReplaceOnLoad.Clear()
        End If
        RaiseEvent ChartLoaded(Me, New EventArgs)
        _hasLoadedData = True
    End Sub
    Private Function GetMasterID() As Integer
        Dim symbolMasterID As Integer = -1
        For Each chart In Parent.Charts
            If chart.Settings("IsSymbolMaster").Value = True Then
                symbolMasterID = chart.TickerID
            End If
        Next
        Return symbolMasterID
    End Function
    Dim _hasLoadedData As Boolean = False
    Public ReadOnly Property HasLoadedData As Boolean
        Get
            Return _hasLoadedData
        End Get
    End Property
    Friend Sub DrawHistoricalBars()
        loadingProgressBar.Visibility = Windows.Visibility.Hidden
        SetStatusText("processing...")
        'MsgBox(historicalBars(0).Date.ToShortDateString & " - " & historicalBars(historicalBars.Count - 1).Date.ToShortDateString)
        'historicalBars.Sort(
        ' Function(item1 As BarData, item2 As BarData) As Boolean
        '     Return item1.Date < item2.Date
        ' End Function)
        Dim barsBack As Integer = TimeSpan.FromDays(DataStream.DaysBack(TickerID)).TotalMinutes / TimeSpanFromBarSize(DataStream.BarSize(TickerID)).TotalMinutes
        'Dim startNum As Integer
        'If barsBack <= 0 Or barsBack > historicalBars.Count Then
        '    startNum = 0
        'ElseIf barsBack < historicalBars.Count Then
        '    startNum = historicalBars.Count - barsBack
        'End If
        'Dim t = Now
        For i = 0 To historicalBars.Count - 1
            IsLoadingHistory = True
            historicalTime = historicalBars(i).Date
            CalculatePriceChange(historicalBars(i))
        Next
        'Log((Now - t).TotalMilliseconds)
        If bars.Count > Settings("DesiredBarCount").Value Then
            Dim num As Integer = bars.Count - Settings("DesiredBarCount").Value - 1
            For i = 0 To num
                Children.Remove(bars(i))
            Next
            bars.RemoveRange(0, num + 1)
            For i = 0 To bars.Count - 1
                Dim data = bars(i).Data
                data.Number = i + 1
                bars(i).Data = data
            Next
            Log("removed first " & num + 1 & " bars to meet desired bar count")
        End If
        Log("")

        HistoricalDataFinished()
        'Log("t1: " & t1.TotalSeconds)
        'Log("t2: " & t2.TotalSeconds)
        'Log("t3: " & t3.TotalSeconds)
    End Sub
    Private Sub threadsafe_HistoricalDataFinished()
        IsDrawingBars = True
        If GetFirstParentOfType(Me, GetType(Window)) Is Nothing Then
            Finalize()
            GC.Collect()
            'If ShowInfoBox("This message is coming from an invisible chart! Do you want to prevent this chart from drawing bars?", DesktopWindow, "Yes, of course!", "No way") = 1 Then
            'DrawHistoricalBars()
            'End If
        Else
            DrawHistoricalBars()
        End If
        IsDrawingBars = False
    End Sub
    Private Sub DataStream_HistoricalDataFinished(ByVal id As Integer)
        If (id = TickerID) Then
            Dispatcher.Invoke(Sub() threadsafe_HistoricalDataFinished())
        End If
    End Sub
    Public Sub RemoveDataStreamHandlers()
        RemoveHandler DataStream.HistoricalDataCompleted, AddressOf DataStream_HistoricalDataFinished
    End Sub
    Dim savedChilds As List(Of ChartDrawingVisual)
    Dim savedChildDates As List(Of Date)
    Friend Sub ClearChart(symbolChanged As Boolean)
        CancelMarketData()
        'loadingProgressBar.SetValue(ProgressBar.ValueProperty, 0.0)
        savedChilds = New List(Of ChartDrawingVisual)
        savedChildDates = New List(Of Date)
        Dim i As Integer
        If bars.Count > 0 Then SaveDrawingObjects()
        While i < Children.Count
            'If TypeOf Children(i) Is Arrow Then
            '    'If Not symbolChanged Then
            '    If CType(Children(i), Arrow).Location.X - 1 < bars.Count Then
            '        savedChilds.Add(Children(i))
            '        savedChildDates.Add(bars(CType(Children(i), Arrow).Location.X - 1).Data.Date)
            '    End If
            '    'End If
            '    Children.RemoveAt(i)
            If TypeOf Children(i) Is OrderControl Then
                If bars.Count > 0 Then
                    savedChilds.Add(Children(i))
                    If CType(Children(i), OrderControl).OrderStatus = OrderStatus.Filled Then
                        If CType(Children(i), OrderControl).FillBar - 1 < bars.Count Then
                            savedChildDates.Add(CType(Children(i), OrderControl).FillDate)
                        Else
                            savedChilds.Remove(Children(i))
                        End If
                    Else
                        savedChildDates.Add(Date.MinValue)
                    End If
                End If
                Children.RemoveAt(i)
            ElseIf TypeOf Children(i) Is Bar OrElse Children(i).AnalysisTechnique IsNot Nothing Then
                Children.RemoveAt(i)
            Else
                i += 1
            End If
        End While
        Children.Clear()
        _orders.Clear()
        ResetBars()
        'InitAxis()
        'RefreshAxisSize()
        historicalBars.Clear()
        currentBarLowPrice = Double.MaxValue
        currentBarHighPrice = Double.MinValue
        'GC.Collect()
    End Sub
    Friend Sub RequestData(ByVal duration As TimeSpan, endDateDurationBack As TimeSpan, symbolChanged As Boolean)
        loadingProgressBar.Visibility = Windows.Visibility.Visible
        ClearChart(symbolChanged)
        IsLoadingHistory = True
        _hasLoadedData = False
        RemoveHandler DataStream.HistoricalDataCompleted, AddressOf DataStream_HistoricalDataFinished
        AddHandler DataStream.HistoricalDataCompleted, AddressOf DataStream_HistoricalDataFinished
        If Not shouldUpdateSilently Then DispatcherHelper.DoEvents()
        'If currentAutoTrend IsNot Nothing Then
        '	For Each item In AnalysisTechniques
        '		If TypeOf item.AnalysisTechnique Is AutoTrendBase Then
        '			CType(item.AnalysisTechnique, AutoTrendBase).RV = Settings("RangeValue").Value * DefaultAutoTrendRV
        '		End If
        '	Next
        'End If
        'If currentAutoTrend IsNot Nothing Then
        '      End If
        Log("chart " & GetChartName() & " has begun loading...")
        If duration.TotalDays <> 0 Then
            DataStream.DaysBack(TickerID) = Round(duration.TotalDays, 5)
            SetStatusText("loading...")

            'DataStream.RequestMarketData(TickerID)
            DataStream.RequestHistoricalData(If(DataStream.UseReplayMode(TickerID), Now - endDateDurationBack, Now), If(Not DataStream.UseReplayMode(TickerID), duration, duration - endDateDurationBack), TickerID)
        Else
            If Not Settings("UseRandom").Value Then
                IB.CancelMarketData(TickerID)
            End If
            Dim symbolMasterID As Integer = GetMasterID()
            If symbolMasterID <> -1 Then
                DataStream.RequestMarketData(symbolMasterID)
            Else
                DataStream.RequestMarketData(TickerID)
            End If
            DrawHistoricalBars()
        End If
    End Sub
    Private _isNewBar As Boolean
    Public ReadOnly Property IsNewBar As Boolean
        Get
            Return _isNewBar
        End Get
    End Property
    Public ReadOnly Property H(ByVal barNum As Integer) As Decimal
        Get
            If barNum > 0 AndAlso barNum <= bars.Count Then
                Return bars(barNum - 1).Data.High
            Else
                Return Nothing
            End If
        End Get
    End Property
    Public ReadOnly Property L(ByVal barNum As Integer) As Decimal
        Get
            If barNum > 0 AndAlso barNum <= bars.Count Then
                Return bars(barNum - 1).Data.Low
            Else
                Return Nothing
            End If
        End Get
    End Property
    Public ReadOnly Property O(ByVal barNum As Integer) As Decimal
        Get
            If barNum > 0 AndAlso barNum <= bars.Count Then
                Return bars(barNum - 1).Data.Open
            Else
                Return Nothing
            End If
        End Get
    End Property
    Public ReadOnly Property C(ByVal barNum As Integer) As Decimal
        Get
            If barNum > 0 AndAlso barNum <= bars.Count Then
                Return bars(barNum - 1).Data.Close
            Else
                Return Nothing
            End If
        End Get
    End Property
    Public ReadOnly Property H As Decimal
        Get
            Return CurrentBar.High
        End Get
    End Property
    Public ReadOnly Property L As Decimal
        Get
            Return CurrentBar.Low
        End Get
    End Property
    Public ReadOnly Property O As Decimal
        Get
            Return CurrentBar.Open
        End Get
    End Property
    Public ReadOnly Property C As Decimal
        Get
            Return CurrentBar.Close
        End Get
    End Property
    Private _askPrice As Decimal
    Public ReadOnly Property AskPrice As Decimal
        Get
            Return _askPrice
        End Get
    End Property
    Private _bidPrice As Decimal
    Public ReadOnly Property BidPrice As Decimal
        Get
            Return _bidPrice
        End Get
    End Property
    Public ReadOnly Property BarIndex As Integer
        <DebuggerStepThrough()>
        Get
            Return bars.Count
        End Get
    End Property
    Public ReadOnly Property CurrentBar As BarData
        <DebuggerStepThrough()>
        Get
            If bars.Count > 0 Then
                Return bars(BarIndex - 1).Data
            Else
                Return Nothing
            End If
        End Get
    End Property
    Public ReadOnly Property CurrentBarLine As Bar
        <DebuggerStepThrough()>
        Get
            Return bars(BarIndex - 1)
        End Get
    End Property
    Public ReadOnly Property Price As Double
        <DebuggerStepThrough()>
        Get
            If BarIndex <> 0 Then
                Return CurrentBar.Close
            Else
                Return 0
            End If
        End Get
    End Property
    Private _isBarExtension As Boolean
    Public ReadOnly Property IsBarExtension As Boolean
        Get
            Return _isBarExtension
        End Get
    End Property
#End Region

#Region "On Screen Controls"
    Private axisTextBlockStyle As New Style
    Private breakEvenLine As New TrendLine
    Private priceCursorText As TextBlock, timeCursorText As Controls.Label, statusText As New Controls.TextBlock
    Private loadingProgressBar As New ProgressBar With {.Minimum = 0, .Maximum = 1, .Visibility = Windows.Visibility.Hidden, .MinHeight = 33}
    Private gridLines As New List(Of TrendLine)
    Private WithEvents priceAxis As New UIChartControl
    Private WithEvents timeAxis As New UIChartControl
    'Friend replaySpeedSlider As ChartSlider
    Private speedSlider As ChartSlider
    Private deflectionSlider As ChartSlider
    Private statusAxis As New UIChartControl
    Private initialOrderBoxLocation As Point
    Private ValidQuantities() As Integer = {1, 2, 3, 4, 5, 10}
    'Private _currentQuantity As Integer = 1
    'Public Property CurrentQuantity As Integer
    '    Get
    '        Return _currentQuantity
    '    End Get
    '    Set(value As Integer)
    '        Dispatcher.BeginInvoke(
    '        Sub()
    '            For Each item In quantityButtons
    '                If item IsNot Nothing Then
    '                    If item.Content = value Then
    '                        item.Background = Brushes.Pink
    '                    Else
    '                        item.Background = Brushes.LightBlue
    '                    End If
    '                End If
    '            Next
    '            _currentQuantity = value
    '        End Sub)
    '    End Set
    'End Property
    Private orderBox As New UIChartControl
    'Private btnCloseAll As New Button With {.Content = "Close" & vbNewLine & "   All", .Width = 40, .Background = Brushes.Purple, .Foreground = Brushes.White}
    'Private btnBuyMarket As New Button With {.Content = "BUY", .Padding = New Thickness(4, 1, 4, 1), .Background = Brushes.Green, .Foreground = Brushes.White, .FontWeight = FontWeights.Bold}
    'Private btnBuyStop As New Button With {.Content = "Buy Stop", .Padding = New Thickness(3, 1, 3, 1), .Background = New SolidColorBrush(Colors.Orange), .Foreground = Brushes.Black}
    'Private btnBuyLimit As New Button With {.Content = "Buy Limit", .Padding = New Thickness(3, 1, 3, 1), .Background = Brushes.Green, .Foreground = Brushes.White}
    'Private btnTransmit As New Button With {.Content = "Transmit", .Padding = New Thickness(3, 1, 3, 1), .Background = Brushes.LightGray, .Foreground = Brushes.Black}
    'Private btnSellMarket As New Button With {.Content = "SELL", .Padding = New Thickness(4, 1, 4, 1), .Background = Brushes.DarkRed, .Foreground = Brushes.White, .FontWeight = FontWeights.Bold}
    'Private btnSellStop As New Button With {.Content = "Sell Stop", .Padding = New Thickness(3, 1, 3, 1), .Background = Brushes.Blue, .Foreground = Brushes.White}
    'Private btnSellLimit As New Button With {.Content = "Sell Limit", .Padding = New Thickness(3, 1, 3, 1), .Background = Brushes.DarkRed, .Foreground = Brushes.White}
    'Private btnCancel As New Button With {.Content = "Cancel", .Padding = New Thickness(3, 1, 3, 1), .Background = Brushes.DeepPink, .Foreground = Brushes.Black}
    'Private btnTab As New Button With {.Content = "T" & vbNewLine & "A" & vbNewLine & "B", .Background = New SolidColorBrush(Color.FromArgb(255, 255, 248, 206)), .Foreground = Brushes.Black}
    'Private btnOCA As New Button With {.Content = "OCA", .Background = New SolidColorBrush(Color.FromArgb(255, 251, 240, 255)), .Foreground = Brushes.Black}
    'Private btnQuantity As New Button With {.Content = "Quantity", .Background = New SolidColorBrush(Color.FromArgb(255, 255, 248, 206)), .Foreground = Brushes.Black, .FontWeight = FontWeights.Bold, .Tag = 0}

    Private btnTransmit As Button
    Private btnCancel As Button
    Private buttonDatabase As New List(Of KeyValuePair(Of Integer, Button))
    Private buttonForegroundMatrix() As Color = {Colors.White, Colors.Black, Colors.Black, Colors.White, Colors.White, Colors.White, Colors.White, Colors.Black, Colors.White, Colors.Black, Colors.Black}
    Private buttonColorMatrix() As Color = {Color.FromArgb(255, 223, 223, 223), '0
                                            Color.FromArgb(255, 158, 255, 145), '1
                                            Color.FromArgb(255, 255, 196, 215), '2
                                            Color.FromArgb(255, 94, 130, 255), '3
                                            Color.FromArgb(255, 236, 141, 0), '4
                                            Color.FromArgb(255, 22, 176, 0), '5
                                            Color.FromArgb(255, 213, 0, 32), '6
                                            Color.FromArgb(255, 255, 248, 206), '7
                                            Colors.DeepPink, '8
                                            Colors.Yellow, '9
                                            Color.FromArgb(255, 210, 251, 255) '10
                                            }
    'Dim quantityButtons(19) As Button
    'Friend QuantityButtonGrid As Grid
    'Dim currentRVLabel As Controls.Label


    Dim dayBoxGrid As Grid
    Dim dayBoxStartAdd As Button
    Dim dayBoxStartSubtract As Button
    Dim dayBoxStartTime As Button
    Dim dayBoxEndTime As Button
    Dim dayBoxEndAdd As Button
    Dim dayBoxEndSubtract As Button


    Private Sub RefreshAllSize()
        RefreshAxisSize()
        'RefreshReplayControlsLocation()
        RefreshRandomDataControlsLocation()
        'RefreshOrderButtonsLocation()
        RefreshOrderBarSize()
    End Sub
    Private Sub InitAll()
        InitAxis()
        InitGrid()
        InitOrderBox()
        InitChartPad()
        'InitReplayControls()
        InitRandomDataControls()
    End Sub
    Private Sub RefreshAxisSize()
        priceAxis.Top = 0
        statusAxis.Top = 0
        timeAxis.Left = 0
        statusAxis.Left = 0

        Settings("PriceBarWidth").Value = Settings("GridWidth").Value

        priceAxis.Width = Settings("GridWidth").Value
        timeAxis.Height = Settings("TimeBarHeight").Value
        statusAxis.Height = Settings("StatusBarHeight").Value
        priceAxis.Height = mainCanvas.ActualHeight
        timeAxis.Top = mainCanvas.ActualHeight - Settings("TimeBarHeight").Value
        priceAxis.Left = Round(mainCanvas.ActualWidth) - Settings("PriceBarWidth").Value
        timeAxis.Width = Round(mainCanvas.ActualWidth) - Settings("PriceBarWidth").Value
        statusAxis.Width = mainCanvas.ActualWidth
    End Sub
    Private Sub RefreshRandomDataControlsLocation()
        If speedSlider IsNot Nothing And deflectionSlider IsNot Nothing Then
            speedSlider.Width = 200
            speedSlider.Height = 25
            speedSlider.Left = mainCanvas.ActualWidth / 2 - speedSlider.Width / 2
            speedSlider.Top = 30
            deflectionSlider.Width = 200
            deflectionSlider.Height = 25
            deflectionSlider.Left = mainCanvas.ActualWidth / 2 - deflectionSlider.Width / 2
            deflectionSlider.Top = 15 + speedSlider.Height + speedSlider.Top
        End If
    End Sub
    Public Sub RefreshOrderBox()
        If IsLoaded And HasParent Then
            'Dim totalSellPrices As Decimal
            'Dim totalBuyPrices As Decimal
            'Dim sellCount As Integer
            'Dim buyCount As Integer
            'For Each order In Orders
            '    If order.OrderStatus = OrderStatus.Filled Then
            '        If order.OrderAction = ActionSide.Buy Then
            '            totalBuyPrices += order.FillPrice * order.Quantity
            '            buyCount += 1
            '        Else
            '            totalSellPrices += order.FillPrice * order.Quantity
            '            sellCount += 1
            '        End If
            '    End If
            'Next
            'If (sellCount <> 0 Or buyCount <> 0) And sellCount <> buyCount Then
            '    If breakEvenLine.Parent Is Nothing Then Children.Add(breakEvenLine)
            '    Dim p = -(Abs(totalBuyPrices - totalSellPrices) / Abs(sellCount - buyCount))
            '    breakEvenLine.StartPoint = New Point(0, p)
            '    breakEvenLine.EndPoint = New Point(100, p)
            'Else
            '    Children.Remove(breakEvenLine)
            'End If
            Dim SetState =
             Sub(btn As Button, enabled As Boolean)
                 If enabled Then
                     btn.IsEnabled = True
                 Else
                     btn.IsEnabled = False
                 End If
             End Sub
            ''If Keyboard.IsKeyDown(Key.LeftCtrl) Or Keyboard.IsKeyDown(Key.RightCtrl) Then
            'SetState(btnBuyMarket, True)
            'SetState(btnSellMarket, True)
            ''Else
            ''    SetState(btnBuyMarket, False)
            ''    SetState(btnSellMarket, False)
            ''End If
            ''SetState(btnQuantity, False)
            SetState(btnCancel, False)
            SetState(btnTransmit, False)
            'btnQuantity.Content = GetOrderQuantity()
            'btnQuantity.Tag = GetOrderQuantity()
            If btnTransmit IsNot Nothing And btnCancel IsNot Nothing Then
                For Each order In Orders
                    If order.IsSelectable AndAlso order.IsSelected AndAlso order.OrderStatus <> OrderStatus.Filled Then
                        'SetState(btnQuantity, True)
                        SetState(btnCancel, True)
                        btnTransmit.Background = New SolidColorBrush(order.SelectedColor)
                        btnTransmit.Content.Foreground = New SolidColorBrush(GetForegroundColor(order.SelectedColor))
                        SetState(btnTransmit, order.TransmitUpdateButtonVisible)
                        If order.IsUpdateNeeded Then
                            btnTransmit.Content.Text = VertifyText("UPDATE")
                            btnTransmit.Tag = True
                        Else
                            btnTransmit.Content.Text = VertifyText("TRANSMIT")
                            btnTransmit.Tag = False
                        End If
                    End If

                Next
            End If
            For Each item In buttonDatabase
                If item.Key > 2 And item.Key < 7 Then
                    item.Value.Background = New SolidColorBrush(buttonColorMatrix(item.Key))
                    item.Value.Content.Foreground = Brushes.White
                End If
            Next
            If SelectedOrder IsNot Nothing AndAlso SelectedOrder.OrderStatus <> OrderStatus.Filled AndAlso SelectedOrder.OrderStatus <> OrderStatus.ApiPending AndAlso SelectedOrder.OrderStatus <> OrderStatus.Submitted Then
                Dim index As Integer = 0
                Select Case True
                    Case SelectedOrder.OrderType = OrderType.STP And SelectedOrder.OrderAction = ActionSide.Sell
                        index = 3
                    Case SelectedOrder.OrderType = OrderType.STP And SelectedOrder.OrderAction = ActionSide.Buy
                        index = 4
                    Case SelectedOrder.OrderType = OrderType.LMT And SelectedOrder.OrderAction = ActionSide.Buy
                        index = 5
                    Case SelectedOrder.OrderType = OrderType.LMT And SelectedOrder.OrderAction = ActionSide.Sell
                        index = 6
                End Select
                For Each item In buttonDatabase
                    If item.Key > 2 And item.Key < 7 Then
                        If item.Key = index And CStr(item.Value.Content.Text) = CStr(SelectedOrder.Quantity) Then
                            item.Value.Background = Brushes.White
                            item.Value.Content.Foreground = Brushes.Black
                        End If
                    End If
                Next
            End If
        End If
    End Sub
    Private Sub RefreshOrderBarSize()
        If orderBox IsNot Nothing And orderBox.Content IsNot Nothing Then
            'orderBox.Width = 710
            'orderBox.Height = 129 '{X=93,Y=691}
        End If
    End Sub

    Dim buttonCount As Integer = 10

    Public Function getButtonVal(btnPos As Integer, currentValue As Decimal, minValue As Decimal, stepValue As Decimal) As Decimal
        Dim startValue As Decimal = RoundTo(Max(minValue, currentValue - stepValue * ((buttonCount - 1) / 2)), stepValue)
        Return ((buttonCount - 1) - btnPos) * stepValue + startValue
    End Function
    Public Sub ChartPadRangeButtonClick(sender As Object, e As EventArgs)
        'If sender IsNot rvButtons(0, 0) AndAlso CType(rvButtons(0, 0).Background, SolidColorBrush).Color = Brushes.Red.Color Then
        '    currentAutoTrend.ChannelMode = AutoTrendBase.ChannelModeType.SetToBase
        'ElseIf sender Is rvButtons(0, 0) Then
        '    currentAutoTrend.ChannelMode = AutoTrendBase.ChannelModeType.Unmerged
        'End If

        ChangeRangeWithDaysBackCalculating(sender.Tag)


    End Sub
    Public Sub ChangeRangeWithDaysBackCalculating(newRange As Double)
        If Settings("UseReplayMode").Value = False And Settings("UseRandom").Value = False And Round(newRange, 4) <> Round(Settings("RangeValue").Value, 4) Then
            Dim reader As New StreamReader(IB.GetCacheFileName(TickerID))
            Dim barList As New List(Of BarData)
            Dim converter As New BarDataConverter
            While reader.Peek <> -1
                Dim line = reader.ReadLine
                If converter.IsValid(line) Then
                    barList.Add(converter.ConvertFromString(line))
                End If
            End While
            Dim barRunningTotal As Double
            Dim daysBack As Double
            For i = barList.Count - 2 To 0 Step -1
                barRunningTotal += Max(Abs(barList(i).High - barList(i + 1).High), Abs(barList(i).Low - barList(i + 1).Low)) / 7 / newRange

                If barRunningTotal > Settings("DesiredBarCount").Value Then
                    daysBack = (Now - barList(i).Date).TotalDays
                    Exit For
                End If
            Next
            If daysBack <> 0 Then IB.DaysBack(TickerID) = daysBack
            reader.Close()


            ChangeRange(newRange)

        End If
    End Sub


    Private Function GetCurrentAutoTrend() As AutoTrendBase
        For Each AnalysisTechnique In AnalysisTechniques
            If TypeOf AnalysisTechnique.AnalysisTechnique Is AutoTrendBase AndAlso CType(AnalysisTechnique.AnalysisTechnique, AutoTrendBase).IsEnabled = True Then
                Return AnalysisTechnique.AnalysisTechnique
            End If
        Next
        Return Nothing
    End Function
    Private Function GetCurrentDayBox() As DayBox
        For Each AnalysisTechnique In AnalysisTechniques
            If TypeOf AnalysisTechnique.AnalysisTechnique Is DayBox AndAlso CType(AnalysisTechnique.AnalysisTechnique, DayBox).IsEnabled = True AndAlso CType(AnalysisTechnique.AnalysisTechnique, DayBox).AdjustTimesOnChartPad Then
                Return AnalysisTechnique.AnalysisTechnique
            End If
        Next
        Return Nothing
    End Function
    Private Function FormatNumberLengthAndPrefix(num As Decimal, placesAfterDecimal As Integer) As String
        'Return RemovePrefixZero(FormatNumber(num, placesAfterDecimal))
        Return Round(num * (10 ^ placesAfterDecimal))
    End Function

    Private Sub InitChartPad()



        'dayBoxGrid = New Grid
        'For i = 1 To 6
        '    dayBoxGrid.RowDefinitions.Add(New RowDefinition With {.Height = GridLength.Auto})
        'Next
        'Grid.SetRow(dayBoxGrid, 15)
        'grd.Children.Add(dayBoxGrid)

        'dayBoxStartAdd = New Button With {.Background = Brushes.White, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .MinHeight = 25, .Content = "+ 5"}
        'dayBoxStartSubtract = New Button With {.Background = Brushes.White, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .MinHeight = 25, .Content = "- 5"}
        'dayBoxStartTime = New Button With {.Background = New SolidColorBrush(Color.FromArgb(255, 230, 235, 255)), .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .MinHeight = 25, .Content = ""}
        'dayBoxEndTime = New Button With {.Background = New SolidColorBrush(Color.FromArgb(255, 230, 235, 255)), .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .MinHeight = 25, .Content = ""}
        'dayBoxEndAdd = New Button With {.Background = Brushes.White, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .MinHeight = 25, .Content = "+ 5"}
        'dayBoxEndSubtract = New Button With {.Background = Brushes.White, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .MinHeight = 25, .Content = "- 5"}
        'Grid.SetRow(dayBoxStartAdd, 0)
        'Grid.SetRow(dayBoxStartSubtract, 1)
        'Grid.SetRow(dayBoxStartTime, 2)
        'Grid.SetRow(dayBoxEndTime, 3)
        'Grid.SetRow(dayBoxEndAdd, 4)
        'Grid.SetRow(dayBoxEndSubtract, 5)
        'dayBoxGrid.Children.Add(dayBoxStartAdd)
        'dayBoxGrid.Children.Add(dayBoxStartSubtract)
        'dayBoxGrid.Children.Add(dayBoxStartTime)
        'dayBoxGrid.Children.Add(dayBoxEndTime)
        'dayBoxGrid.Children.Add(dayBoxEndAdd)
        'dayBoxGrid.Children.Add(dayBoxEndSubtract)

        'Dim popupDayBoxStartTime As New Popup With {.Placement = PlacementMode.Left, .PlacementTarget = dayBoxStartTime, .Width = 208, .Height = 231, .StaysOpen = False}
        'Dim popupDayBoxGridStartTime As New Grid With {.Background = Brushes.White}
        'For x = 1 To 4
        '    popupDayBoxGridStartTime.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})
        'Next
        'For y = 1 To 7
        '    popupDayBoxGridStartTime.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Star)})
        'Next
        'AddHandler popupDayBoxStartTime.Opened,
        '    Sub()
        '        popupDayBoxGridStartTime.Children.Clear()
        '        For x = 1 To 4
        '            For y = 1 To 9
        '                Dim current = GetCurrentDayBox()
        '                If current IsNot Nothing Then
        '                    Dim value As String = FormatNumberLength(CStr(y + 3), 2) & CStr(FormatNumberLength((x - 1) * 15, 2, True))
        '                    Dim btn As New Button With {.Margin = New Thickness(1), .Content = value, .FontSize = 14.5, .Foreground = Brushes.Black}
        '                    Grid.SetRow(btn, y - 1)
        '                    Grid.SetColumn(btn, x - 1)
        '                    btn.Background = Brushes.White
        '                    If Date.ParseExact(current.StartTime, "HHmm", Nothing).TimeOfDay = Date.ParseExact(value, "HHmm", Nothing).TimeOfDay Then
        '                        btn.Background = Brushes.Pink
        '                    End If
        '                    popupDayBoxGridStartTime.Children.Add(btn)
        '                    AddHandler btn.Click,
        '                        Sub(sender As Object, e As EventArgs)
        '                            popupDayBoxStartTime.IsOpen = False
        '                            GetCurrentDayBox().StartTime = CStr(sender.Content)
        '                            ReApplyAnalysisTechnique(GetCurrentDayBox())
        '                        End Sub
        '                End If
        '            Next
        '        Next
        '    End Sub
        'popupDayBoxStartTime.Child = popupDayBoxGridStartTime
        'AddHandler dayBoxStartTime.Click,
        '    Sub()
        '        popupDayBoxStartTime.IsOpen = True
        '    End Sub

        'Dim popupDayBoxEndTime As New Popup With {.Placement = PlacementMode.Left, .PlacementTarget = dayBoxStartTime, .Width = 208, .Height = 205, .StaysOpen = False}
        'Dim popupDayBoxGridEndTime As New Grid With {.Background = Brushes.White}
        'For x = 1 To 4
        '    popupDayBoxGridEndTime.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})
        'Next
        'For y = 1 To 7
        '    popupDayBoxGridEndTime.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Star)})
        'Next
        'AddHandler popupDayBoxEndTime.Opened,
        '    Sub()
        '        popupDayBoxGridEndTime.Children.Clear()
        '        For x = 1 To 4
        '            For y = 1 To 8
        '                Dim current = GetCurrentDayBox()
        '                If current IsNot Nothing Then
        '                    Dim value As String = FormatNumberLength(CStr(y + 8), 2) & CStr(FormatNumberLength((x - 1) * 15, 2, True))
        '                    Dim btn As New Button With {.Margin = New Thickness(1), .Content = value, .FontSize = 14.5, .Foreground = Brushes.Black}
        '                    Grid.SetRow(btn, y - 1)
        '                    Grid.SetColumn(btn, x - 1)
        '                    btn.Background = Brushes.White
        '                    If Date.ParseExact(current.EndTime, "HHmm", Nothing).TimeOfDay = Date.ParseExact(value, "HHmm", Nothing).TimeOfDay Then
        '                        btn.Background = Brushes.Pink
        '                    End If
        '                    popupDayBoxGridEndTime.Children.Add(btn)
        '                    AddHandler btn.Click,
        '                        Sub(sender As Object, e As EventArgs)
        '                            popupDayBoxEndTime.IsOpen = False
        '                            GetCurrentDayBox().EndTime = CStr(sender.Content)
        '                            ReApplyAnalysisTechnique(GetCurrentDayBox())
        '                        End Sub
        '                End If
        '            Next
        '        Next
        '    End Sub
        'popupDayBoxEndTime.Child = popupDayBoxGridEndTime
        'AddHandler dayBoxEndTime.Click,
        '    Sub()
        '        popupDayBoxEndTime.IsOpen = True
        '    End Sub
        'AddHandler dayBoxStartAdd.Click,
        '    Sub()
        '        If GetCurrentDayBox() IsNot Nothing Then
        '            GetCurrentDayBox().StartTime = Date.ParseExact(GetCurrentDayBox().StartTime, "HHmm", Nothing).AddMinutes(5).ToString("HHmm")
        '            ReApplyAnalysisTechnique(GetCurrentDayBox())
        '        End If
        '    End Sub
        'AddHandler dayBoxStartSubtract.Click,
        '    Sub()
        '        If GetCurrentDayBox() IsNot Nothing Then
        '            GetCurrentDayBox().StartTime = Date.ParseExact(GetCurrentDayBox().StartTime, "HHmm", Nothing).AddMinutes(-5).ToString("HHmm")
        '            ReApplyAnalysisTechnique(GetCurrentDayBox())
        '        End If
        '    End Sub
        'AddHandler dayBoxEndAdd.Click,
        '    Sub()
        '        If GetCurrentDayBox() IsNot Nothing Then
        '            GetCurrentDayBox().EndTime = Date.ParseExact(GetCurrentDayBox().EndTime, "HHmm", Nothing).AddMinutes(5).ToString("HHmm")
        '            ReApplyAnalysisTechnique(GetCurrentDayBox())
        '        End If
        '    End Sub
        'AddHandler dayBoxEndSubtract.Click,
        '    Sub()
        '        If GetCurrentDayBox() IsNot Nothing Then
        '            GetCurrentDayBox().EndTime = Date.ParseExact(GetCurrentDayBox().EndTime, "HHmm", Nothing).AddMinutes(-5).ToString("HHmm")
        '            ReApplyAnalysisTechnique(GetCurrentDayBox())
        '        End If
        '    End Sub

        'For Each technique In AnalysisTechniques
        '    If GetType(IChartPadAnalysisTechnique).IsAssignableFrom(technique.AnalysisTechnique.GetType) Then
        '        CType(technique.AnalysisTechnique, IChartPadAnalysisTechnique).InitChartPad()
        '    End If
        'Next

        'UpdateChartPad()
    End Sub
    Friend Sub UpdateChartPad()
        For Each technique In AnalysisTechniques
            If GetType(IChartPadAnalysisTechnique).IsAssignableFrom(technique.AnalysisTechnique.GetType) Then
                If CType(technique.AnalysisTechnique, IChartPadAnalysisTechnique).ChartPadVisible Then
                    CType(technique.AnalysisTechnique, IChartPadAnalysisTechnique).UpdateChartPad()
                End If
            End If
        Next
    End Sub
    Public Shared Function CreateQuanityGrid(order As OrderControl) As Grid
        Dim grd As New Grid
        grd.Resources.MergedDictionaries.Add(New ResourceDictionary)
        grd.Resources.MergedDictionaries(0).Source = New Uri("Themes/OrderButtonStyle.xaml", UriKind.Relative)
        For x = 1 To 5
            grd.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Auto)})
        Next
        For y = 1 To 4
            grd.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Star)})
        Next
        Dim q(19) As Button
        For i = 0 To 19
            q(i) = New Button With {.Content = i + 1, .Padding = New Thickness(3, 1, 3, 1), .Background = New SolidColorBrush(Colors.LightBlue), .Foreground = Brushes.Black, .Width = 27, .Height = 27, .FontWeight = FontWeights.Bold}
            If q(i).Content = order.Quantity Then
                q(i).Background = Brushes.Pink
            End If
            Grid.SetRow(q(i), i \ 5)
            Grid.SetColumn(q(i), i Mod 5)
        Next

        For Each btn In q
            grd.Children.Add(btn)
            btn.Margin = New Thickness(-0.5)
            AddHandler btn.Click,
                Sub()
                    If grd.Parent IsNot Nothing AndAlso TypeOf grd.Parent Is Popup Then
                        CType(grd.Parent, Popup).IsOpen = False
                    End If
                    If order IsNot Nothing AndAlso order.OrderType <> OrderType.MKT AndAlso order.OrderStatus <> OrderStatus.Filled Then
                        order.Quantity = btn.Content
                        order.Resend()
                        order.RefreshVisual()
                    End If
                End Sub
        Next
        Return grd
    End Function
    Private Sub InitOrderBox()
        breakEvenLine = New TrendLine
        breakEvenLine.IsSelectable = False
        breakEvenLine.IsEditable = False
        breakEvenLine.ExtendLeft = True
        breakEvenLine.ExtendRight = True
        breakEvenLine.Pen = New Pen(Brushes.Gray, 1)
        breakEvenLine.StartPoint = New Point(0, 0)
        breakEvenLine.EndPoint = New Point(1, 0)
        Children.Add(breakEvenLine)
        orderBox = New UIChartControl
        orderBox.Left = initialOrderBoxLocation.X
        orderBox.Top = initialOrderBoxLocation.Y
        'orderBox.Scaleable = ScaleType.Fixed
        orderBox.IsDraggable = True
        orderBox.Background = Brushes.Blue
        Dim bd As New Border With {.BorderBrush = Brushes.Blue, .BorderThickness = New Thickness(1, 1, 1, 1), .Padding = New Thickness(6, 6, 6, 3)}
        Dim grd As New Grid
        bd.Child = grd
        bd.Background = New SolidColorBrush(Color.FromArgb(255, 255, 255, 255))
        grd.Resources.MergedDictionaries.Add(New ResourceDictionary)
        grd.Resources.MergedDictionaries(0).Source = New Uri("Themes/OrderButtonStyle.xaml", UriKind.Relative)
        grd.Margin = New Thickness(-2)
        For i = 1 To 30
            grd.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Auto)})
        Next
        For i = 1 To 30
            grd.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Auto)})
        Next
        Dim getLabel =
                Function(text As String, row As Integer, column As Integer, colSpan As Integer) As Controls.Border
                    Dim b As New Border With {.Background = Brushes.Transparent, .BorderBrush = New SolidColorBrush(Color.FromArgb(255, 255, 242, 0)), .CornerRadius = New CornerRadius(0), .BorderThickness = New Thickness(0)}
                    Dim t = New Controls.Label With {.Height = 20, .Background = Brushes.White, .Foreground = Brushes.Black, .FontSize = 15, .FontWeight = FontWeights.Bold, .Content = text, .HorizontalContentAlignment = HorizontalAlignment.Center, .VerticalContentAlignment = VerticalAlignment.Center, .HorizontalAlignment = HorizontalAlignment.Stretch, .VerticalAlignment = VerticalAlignment.Stretch, .Padding = New Thickness(-5)}
                    Grid.SetRow(b, row)
                    Grid.SetColumn(b, column)
                    Grid.SetColumnSpan(b, colSpan)
                    b.Child = t
                    Return b
                End Function
        Dim getButton =
                Function(text As String, row As Integer, col As Integer, rowSpan As Integer, colSpan As Integer, tag As Object, operation As RoutedEventHandler, category As Integer) As Border
                    Dim t As New TextBlock With {.LineHeight = 14, .Background = Brushes.Transparent, .LineStackingStrategy = LineStackingStrategy.BlockLineHeight, .HorizontalAlignment = HorizontalAlignment.Center, .VerticalAlignment = VerticalAlignment.Center, .FontSize = 14.5, .FontWeight = FontWeights.Bold, .Text = text, .Foreground = New SolidColorBrush(buttonForegroundMatrix(category)), .Margin = New Thickness(0, 3, 0, 0)}
                    Dim b = New Button With {.Content = t, .Background = New SolidColorBrush(buttonColorMatrix(category)), .MinHeight = 28, .MinWidth = 34, .MaxWidth = 34, .Tag = tag}
                    Dim g As New Border With {.Background = New SolidColorBrush(buttonColorMatrix(category)), .BorderBrush = Brushes.LightGray, .BorderThickness = New Thickness(0), .CornerRadius = New CornerRadius(3), .Margin = New Thickness(0), .MaxWidth = 34}
                    g.Child = b
                    Grid.SetRow(g, row)
                    Grid.SetColumn(g, col)
                    If rowSpan <> 0 Then Grid.SetRowSpan(g, rowSpan)
                    If colSpan <> 0 Then Grid.SetColumnSpan(g, colSpan)
                    If operation IsNot Nothing Then AddHandler b.Click, operation
                    If category <> 0 Then buttonDatabase.Add(New KeyValuePair(Of Integer, Button)(category, b))
                    Return g
                End Function
        Dim getBorder =
            Function(row As Integer, col As Integer, minWidth As Decimal, minHeight As Decimal, rowSpan As Integer, colSpan As Integer, color As Color) As Border
                Dim b As New Border With {.Background = New SolidColorBrush(color), .MinHeight = minHeight, .MinWidth = minWidth}
                Grid.SetRow(b, row)
                Grid.SetColumn(b, col)
                Grid.SetRowSpan(b, rowSpan)
                Grid.SetColumnSpan(b, colSpan)
                Return b
            End Function
        Dim marketAction =
            Sub(sender As Object, e As EventArgs)
                Dim order As OrderControl
                If sender.Tag = True Then
                    order = New OrderControl(Me, IB.Contract(TickerID)) With {.Color = Colors.Green, .Quantity = sender.Content.Text, .SelectedColor = Colors.Green, .ButtonHoverColor = Colors.LightGreen,
                        .Price = Price, .OrderType = OrderType.MKT, .OrderAction = ActionSide.BUY}
                Else
                    order = New OrderControl(Me, IB.Contract(TickerID)) With {.Color = Colors.Red, .Quantity = sender.Content.Text, .SelectedColor = Colors.Red, .ButtonHoverColor = Colors.Pink,
                        .Price = Price, .OrderType = OrderType.MKT, .OrderAction = ActionSide.SELL}
                End If
                Children.Add(order)
                order.IsSelected = True
                order.Transmit()
            End Sub
        Dim stopAction =
            Sub(sender As Object, e As EventArgs)
                If SelectedOrder Is Nothing OrElse (SelectedOrder.OrderType <> OrderType.STP AndAlso SelectedOrder.OrderType <> OrderType.MKT AndAlso SelectedOrder.OrderStatus <> OrderStatus.Filled) OrElse
                   (SelectedOrder.OrderType = OrderType.STP And (SelectedOrder.OrderStatus = OrderStatus.Filled Or SelectedOrder.OrderStatus = OrderStatus.Submitted Or SelectedOrder.OrderStatus = OrderStatus.ApiPending Or ((sender.Tag = True And SelectedOrder.OrderAction = ActionSide.Sell) Or (sender.Tag = False And SelectedOrder.OrderAction = ActionSide.Buy)))) Then
                    Dim order As OrderControl
                    If sender.Tag = True Then
                        order = New OrderControl(Me, IB.Contract(TickerID)) With {.Color = Color.FromArgb(255, 204, 204, 204), .SelectedColor = Colors.Orange, .Quantity = sender.Content.Text, .ButtonHoverColor = Color.FromArgb(255, 255, 176, 98),
                        .Price = Price, .OrderType = OrderType.STP, .OrderAction = ActionSide.BUY}
                    Else
                        order = New OrderControl(Me, IB.Contract(TickerID)) With {.Color = Color.FromArgb(255, 204, 204, 204), .SelectedColor = Color.FromArgb(255, 0, 127, 255), .Quantity = sender.Content.Text, .ButtonHoverColor = Colors.LightBlue,
                        .Price = Price, .OrderType = OrderType.STP, .OrderAction = ActionSide.SELL}
                    End If
                    Children.Add(order)
                    order.IsSelected = True
                Else
                    SelectedOrder.Quantity = sender.Content.Text
                    SelectedOrder.RefreshVisual()
                End If
            End Sub
        Dim limitAction =
            Sub(sender As Object, e As EventArgs)
                If SelectedOrder Is Nothing OrElse (SelectedOrder.OrderType <> OrderType.LMT AndAlso SelectedOrder.OrderType <> OrderType.MKT AndAlso SelectedOrder.OrderStatus <> OrderStatus.Filled) OrElse
                   (SelectedOrder.OrderType = OrderType.LMT And (SelectedOrder.OrderStatus = OrderStatus.Filled Or SelectedOrder.OrderStatus = OrderStatus.Submitted Or SelectedOrder.OrderStatus = OrderStatus.ApiPending Or ((sender.Tag = True And SelectedOrder.OrderAction = ActionSide.Sell) Or (sender.Tag = False And SelectedOrder.OrderAction = ActionSide.Buy)))) Then
                    Dim order As OrderControl
                    If sender.Tag = True Then
                        order = New OrderControl(Me, IB.Contract(TickerID)) With {.Color = Color.FromArgb(255, 204, 204, 204), .SelectedColor = Colors.Green, .Quantity = sender.Content.Text, .ButtonHoverColor = Colors.LightGreen,
                        .Price = Price, .OrderType = OrderType.LMT, .OrderAction = ActionSide.BUY}
                    Else
                        order = New OrderControl(Me, IB.Contract(TickerID)) With {.Color = Color.FromArgb(255, 204, 204, 204), .SelectedColor = Color.FromArgb(255, 205, 0, 0), .Quantity = sender.Content.Text, .ButtonHoverColor = Colors.Pink,
                        .Price = Price, .OrderType = OrderType.LMT, .OrderAction = ActionSide.SELL}
                    End If
                    Children.Add(order)
                    order.IsSelected = True
                Else
                    SelectedOrder.Quantity = sender.Content.Text
                    SelectedOrder.RefreshVisual()
                End If
            End Sub

        Dim transmitAction =
            Sub(sender As Object, e As EventArgs)
                If btnTransmit.Tag = False Then
                    If SelectedOrder IsNot Nothing Then SelectedOrder.Transmit()
                Else
                    If SelectedOrder IsNot Nothing Then SelectedOrder.Resend()
                End If
            End Sub
        Dim cancelAction =
            Sub(sender As Object, e As EventArgs)
                If SelectedOrder IsNot Nothing Then SelectedOrder.Cancel()
            End Sub
        Dim tabAction =
            Sub(sender As Object, e As EventArgs)
                Dim orders As New List(Of OrderControl)
                For Each order In Me.Orders
                    If order.OrderStatus <> OrderStatus.Filled Then
                        orders.Add(order)
                    End If
                Next
                Dim selected As Boolean = False
                For i = 0 To orders.Count - 1
                    If orders(i).IsSelected Then
                        If i = orders.Count - 1 Then
                            orders(0).IsSelected = True
                        Else
                            orders(i + 1).IsSelected = True
                        End If
                        selected = True
                        Exit For
                    End If
                Next
                If Not selected And orders.Count > 0 Then
                    orders(0).IsSelected = True
                End If
            End Sub
        Dim ocaAction =
            Sub(sender As Object, e As EventArgs)
                Dim orderList As New List(Of OrderControl)
                For Each order In Orders
                    If order.OrderStatus = OrderStatus.Inactive Then
                        order.IsOCA = True
                        order.OCAGroupName = Now.ToShortTimeString
                    Else
                        order.IsOCA = False
                    End If
                Next
            End Sub

        Dim marketGrid As New Grid
        Grid.SetRowSpan(marketGrid, 5)
        marketGrid.RowDefinitions.Add(New RowDefinition() With {.Height = New GridLength(1, GridUnitType.Star)})
        marketGrid.RowDefinitions.Add(New RowDefinition() With {.Height = New GridLength(1, GridUnitType.Auto)})
        marketGrid.RowDefinitions.Add(New RowDefinition() With {.Height = New GridLength(1, GridUnitType.Star)})
        marketGrid.RowDefinitions.Add(New RowDefinition() With {.Height = New GridLength(1, GridUnitType.Auto)})
        marketGrid.RowDefinitions.Add(New RowDefinition() With {.Height = New GridLength(1, GridUnitType.Star)})

        marketGrid.Children.Add(getLabel("SELL", 0, 0, 1))
        CType(CType(marketGrid.Children(marketGrid.Children.Count - 1), Border).Child, Controls.Label).Foreground = Brushes.Red
        marketGrid.Children.Add(getLabel("MARKET", 2, 0, 1))
        marketGrid.Children.Add(getLabel("BUY", 4, 0, 1))
        CType(CType(marketGrid.Children(marketGrid.Children.Count - 1), Border).Child, Controls.Label).Foreground = Brushes.Green

        grd.Children.Add(marketGrid)

        Dim vertBar = New Border With {.BorderThickness = New Thickness(1, 0, 0, 0), .BorderBrush = Brushes.Blue, .Background = Brushes.Transparent, .MinWidth = 1, .Margin = New Thickness(3, 0, 0, 0)}
        Grid.SetRow(vertBar, 0) : Grid.SetRowSpan(vertBar, 5) : Grid.SetColumn(vertBar, 1)
        grd.Children.Add(vertBar)

        grd.Children.Add(getLabel("Sell Limit", 1, 2, 1))
        CType(CType(grd.Children(grd.Children.Count - 1), Border).Child, Controls.Label).Foreground = Brushes.Red
        CType(CType(grd.Children(grd.Children.Count - 1), Border).Child, Controls.Label).Margin = New Thickness(4, 0, 4, 0)
        grd.Children.Add(getLabel("Buy Limit", 3, 2, 1))
        CType(CType(grd.Children(grd.Children.Count - 1), Border).Child, Controls.Label).Foreground = Brushes.Green
        CType(CType(grd.Children(grd.Children.Count - 1), Border).Child, Controls.Label).Margin = New Thickness(4, 0, 4, 0)

        grd.Children.Add(getLabel("Sell Stop", 0, 2, 1))
        CType(CType(grd.Children(grd.Children.Count - 1), Border).Child, Controls.Label).Foreground = New SolidColorBrush(Color.FromArgb(255, 94, 130, 255))
        CType(CType(grd.Children(grd.Children.Count - 1), Border).Child, Controls.Label).Margin = New Thickness(4, 0, 4, 0)
        grd.Children.Add(getLabel("Buy Stop", 4, 2, 1))
        CType(CType(grd.Children(grd.Children.Count - 1), Border).Child, Controls.Label).Foreground = New SolidColorBrush(Color.FromArgb(255, 236, 141, 0))
        CType(CType(grd.Children(grd.Children.Count - 1), Border).Child, Controls.Label).Margin = New Thickness(4, 0, 4, 0)

        'markets
        marketGrid.Children.Add(getButton(ValidQuantities(1), 1, 0, 1, 1, True, marketAction, 1))
        'grd.Children.Add(getButton(ValidQuantities(1), 2, 1, 1, 1, True, marketAction, 1))
        'grd.Children.Add(getButton(ValidQuantities(2), 2, 0, 1, 1, True, marketAction, 1))
        'grd.Children.Add(getButton(ValidQuantities(3), 2, 2, 1, 1, True, marketAction, 1))
        'grd.Children.Add(getButton(ValidQuantities(4), 2, 1, 1, 1, True, marketAction, 1))
        'grd.Children.Add(getButton(ValidQuantities(5), 2, 0, 1, 1, True, marketAction, 1))
        marketGrid.Children.Add(getButton(ValidQuantities(0), 3, 0, 1, 1, False, marketAction, 2))
        'grd.Children.Add(getButton(ValidQuantities(1), 0, 1, 1, 1, False, marketAction, 2))
        'grd.Children.Add(getButton(ValidQuantities(2), 0, 0, 1, 1, False, marketAction, 2))
        'grd.Children.Add(getButton(ValidQuantities(3), 0, 2, 1, 1, False, marketAction, 2))
        'grd.Children.Add(getButton(ValidQuantities(4), 0, 1, 1, 1, False, marketAction, 2))
        'grd.Children.Add(getButton(ValidQuantities(5), 0, 0, 1, 1, False, marketAction, 2))

        'stops
        grd.Children.Add(getButton(ValidQuantities(0), 0, 5, 1, 1, False, stopAction, 3))
        grd.Children.Add(getButton(ValidQuantities(1), 0, 4, 1, 1, False, stopAction, 3))
        grd.Children.Add(getButton(ValidQuantities(2), 0, 3, 1, 1, False, stopAction, 3))
        'grd.Children.Add(getButton(ValidQuantities(3), 2, 10, 1, 1, False, stopAction, 3))
        'grd.Children.Add(getButton(ValidQuantities(4), 2, 9, 1, 1, False, stopAction, 3))
        'grd.Children.Add(getButton(ValidQuantities(5), 2, 8, 1, 1, False, stopAction, 3))
        grd.Children.Add(getButton(ValidQuantities(0), 4, 5, 1, 1, True, stopAction, 4))
        grd.Children.Add(getButton(ValidQuantities(1), 4, 4, 1, 1, True, stopAction, 4))
        grd.Children.Add(getButton(ValidQuantities(2), 4, 3, 1, 1, True, stopAction, 4))
        'grd.Children.Add(getButton(ValidQuantities(3), 0, 10, 1, 1, True, stopAction, 4))
        'grd.Children.Add(getButton(ValidQuantities(4), 0, 9, 1, 1, True, stopAction, 4))
        'grd.Children.Add(getButton(ValidQuantities(5), 0, 8, 1, 1, True, stopAction, 4))

        'limits
        grd.Children.Add(getButton(ValidQuantities(0), 3, 5, 1, 1, True, limitAction, 5))
        grd.Children.Add(getButton(ValidQuantities(1), 3, 4, 1, 1, True, limitAction, 5))
        grd.Children.Add(getButton(ValidQuantities(2), 3, 3, 1, 1, True, limitAction, 5))
        'grd.Children.Add(getButton(ValidQuantities(3), 2, 16, 1, 1, True, limitAction, 5))
        'grd.Children.Add(getButton(ValidQuantities(4), 2, 15, 1, 1, True, limitAction, 5))
        'grd.Children.Add(getButton(ValidQuantities(5), 2, 14, 1, 1, True, limitAction, 5))
        grd.Children.Add(getButton(ValidQuantities(0), 1, 5, 1, 1, False, limitAction, 6))
        grd.Children.Add(getButton(ValidQuantities(1), 1, 4, 1, 1, False, limitAction, 6))
        grd.Children.Add(getButton(ValidQuantities(2), 1, 3, 1, 1, False, limitAction, 6))
        'grd.Children.Add(getButton(ValidQuantities(3), 0, 16, 1, 1, False, limitAction, 6))
        'grd.Children.Add(getButton(ValidQuantities(4), 0, 15, 1, 1, False, limitAction, 6))
        'grd.Children.Add(getButton(ValidQuantities(5), 0, 14, 1, 1, False, limitAction, 6))


        'grd.Children.Add(getBorder(0, 12, 12, 0, 3, 1, Colors.White))

        btnTransmit = getButton(VertifyText("TRANSMIT"), 0, 7, 5, 1, False, transmitAction, 0).Child
        btnCancel = getButton(VertifyText("CANCEL"), 0, 9, 5, 1, False, cancelAction, 8).Child
        grd.Children.Add(getButton(VertifyText("OCA"), 0, 6, 5, 1, False, ocaAction, 10))
        Dim btnTab As Button = getButton(VertifyText("TAB"), 0, 8, 5, 1, False, tabAction, 7).Child
        btnTransmit.FontSize = 15
        btnTab.Margin = New Thickness(0, 0, -4, 0)
        grd.Children.Add(btnTransmit.Parent)
        grd.Children.Add(btnCancel.Parent)
        grd.Children.Add(btnTab.Parent)

        Dim dragger = New Border With {.BorderThickness = New Thickness(0, 1, 0, 0), .BorderBrush = Brushes.Blue, .Background = Brushes.Transparent, .MinWidth = 1, .Margin = New Thickness(0, 1, 0, 1)}
        Grid.SetRow(dragger, 2) : Grid.SetColumnSpan(dragger, 4) : Grid.SetColumn(dragger, 2) : dragger.VerticalAlignment = VerticalAlignment.Center
        grd.Children.Add(dragger)

        'QuantityButtonGrid = New Grid
        'QuantityButtonGrid.Margin = New Thickness(2)
        'grd.HorizontalAlignment = Windows.HorizontalAlignment.Center
        'grd.Background = Background
        'marketGrd.Background = Background
        'Dim leftGrd As New Grid
        'leftGrd.Margin = New Thickness(2)
        'leftGrd.VerticalAlignment = VerticalAlignment.Center
        'leftGrd.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Auto)})
        'leftGrd.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Auto)})
        'leftGrd.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Auto)})
        'leftGrd.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Auto)})
        'leftGrd.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Auto)})
        'For x = 1 To 5
        '    QuantityButtonGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Auto)})
        'Next
        'For y = 1 To 4
        '    QuantityButtonGrid.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Star)})
        'Next
        'For x = 1 To 6
        '    grd.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Auto)})
        'Next
        'For y = 1 To 4
        '    grd.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Star)})
        'Next
        'marketGrd.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Star)})
        'marketGrd.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Star)})
        ''marketGrd.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Star)})

        'bd.Child = grd
        'bd.BorderBrush = New SolidColorBrush(Color.FromArgb(40, 255, 255, 255))
        'bd.BorderThickness = New Thickness(0)
        'bd.Background = Brushes.White
        'bd.CornerRadius = New CornerRadius(0)
        'bd.HorizontalAlignment = Windows.HorizontalAlignment.Center
        'Dim removeMenuItem As New MenuItem With {.Header = "Hide"}
        'AddHandler removeMenuItem.Click,
        ' Sub()
        '     mnuViewOrderBar.IsChecked = False
        ' End Sub
        'bd.ContextMenu = New ContextMenu With {.ItemsSource = {removeMenuItem}}
        'Dim btnDrag As New Border With {.BorderThickness = New Thickness(1.5), .BorderBrush = Brushes.DarkGray, .Background = New SolidColorBrush(Color.FromArgb(255, 141, 106, 75))}

        'For i = 0 To 19
        '    quantityButtons(i) = New Button With {.Content = i + 1, .Padding = New Thickness(3, 1, 3, 1), .Background = New SolidColorBrush(Colors.LightBlue), .Foreground = Brushes.Black, .Width = 27, .FontWeight = FontWeights.Bold}

        '    Grid.SetRow(quantityButtons(i), i \ 5)
        '    Grid.SetColumn(quantityButtons(i), i Mod 5)
        'Next
        'Dim buttons =
        '{
        '   btnBuyStop, btnBuyLimit, btnCancel,
        '  btnTab, btnOCA, btnSellStop, btnSellLimit, btnTransmit
        '}
        'btnOCA.FontWeight = FontWeights.Bold
        'btnTab.FontWeight = FontWeights.Bold
        'btnBuyMarket.FontSize += 5
        'btnSellMarket.FontSize += 5
        'btnBuyMarket.FontWeight = FontWeights.Bold
        'btnSellMarket.FontWeight = FontWeights.Bold
        'btnBuyMarket.Margin = New Thickness(2)
        'btnSellMarket.Margin = New Thickness(2)
        'quantityButtons(0).Background = Brushes.Pink
        'For Each btn In quantityButtons
        '    QuantityButtonGrid.Children.Add(btn)
        '    btn.Margin = New Thickness(-0.5)
        '    AddHandler btn.Click,
        '    Sub()
        '        If SelectedOrder IsNot Nothing AndAlso SelectedOrder.OrderType <> OrderType.Market AndAlso SelectedOrder.OrderStatus <> OrderStatus.Filled Then
        '            SelectedOrder.Quantity = btn.Content
        '            SelectedOrder.RefreshVisual()
        '        End If
        '        For Each item In quantityButtons
        '            item.Background = Brushes.LightBlue
        '        Next
        '        btn.Background = Brushes.Pink
        '        _currentQuantity = btn.Content
        '    End Sub
        'Next
        ''Grid.SetColumn(btnCloseAll, 0)
        ''Grid.SetRowSpan(btnCloseAll, 2)

        ''Grid.SetColumn(btnQuantity, 4)
        ''Grid.SetRowSpan(btnQuantity, 2)

        'Grid.SetColumn(btnSellStop, 0)
        'Grid.SetRow(btnSellStop, 0)

        'Grid.SetColumn(btnBuyLimit, 3)
        'Grid.SetRow(btnBuyLimit, 0)

        'Grid.SetColumn(btnBuyStop, 0)
        'Grid.SetRow(btnBuyStop, 2)

        'Grid.SetColumn(btnSellLimit, 3)
        'Grid.SetRow(btnSellLimit, 2)

        'Grid.SetColumn(btnCancel, 3)
        'Grid.SetRow(btnCancel, 3)

        'Grid.SetColumn(btnTransmit, 3)
        'Grid.SetRow(btnTransmit, 1)

        'Grid.SetColumn(btnOCA, 0)
        'Grid.SetRow(btnOCA, 1)

        'Grid.SetColumn(btnTab, 4)
        'Grid.SetRowSpan(btnTab, 4)

        'Grid.SetColumn(QuantityButtonGrid, 2)
        'Grid.SetRowSpan(QuantityButtonGrid, 4)

        'Grid.SetColumn(marketGrd, 1)
        'Grid.SetRowSpan(marketGrd, 3)

        'Grid.SetColumn(btnBuyMarket, 0)
        'Grid.SetRow(btnBuyMarket, 0)

        'Grid.SetColumn(btnSellMarket, 0)
        'Grid.SetRow(btnSellMarket, 1)
        'Grid.SetRowSpan(leftGrd, 4)
        'Grid.SetColumn(btnDrag, 5)
        ''Grid.SetColumn(btnDrag, 2)
        'Grid.SetRowSpan(btnDrag, 4)
        'btnDrag.Margin = New Thickness(2)

        'marketGrd.Children.Add(btnBuyMarket)
        'marketGrd.Children.Add(btnSellMarket)
        'grd.Children.Add(btnDrag)
        'leftGrd.Children.Add(marketGrd)
        'grd.Children.Add(QuantityButtonGrid)
        'leftGrd.Children.Add(btnSellStop)
        'leftGrd.Children.Add(btnOCA)
        'leftGrd.Children.Add(btnBuyStop)
        'grd.Children.Add(leftGrd)
        'btnSellStop.Height = 24
        'btnBuyStop.Height = 24
        'btnOCA.Height = 24
        'For Each button In buttons
        '    If button IsNot Nothing Then
        '        button.Margin = New Thickness(2)
        '        button.Width = 68
        '        If button.Parent Is Nothing Then grd.Children.Add(button)
        '    End If
        'Next
        'btnTab.Width = 30
        'btnDrag.Width = 20
        'AddHandler btnTab.Click,
        ' Sub()
        '     Dim orders As New List(Of OrderControl)
        '     For Each order In Me.Orders
        '         If order.OrderStatus <> OrderStatus.Filled Then
        '             orders.Add(order)
        '         End If
        '     Next
        '     Dim selected As Boolean = False
        '     For i = 0 To orders.Count - 1
        '         If orders(i).IsSelected Then
        '             If i = orders.Count - 1 Then
        '                 orders(0).IsSelected = True
        '             Else
        '                 orders(i + 1).IsSelected = True
        '             End If
        '             selected = True
        '             Exit For
        '         End If
        '     Next
        '     If Not selected And orders.Count > 0 Then
        '         orders(0).IsSelected = True
        '     End If
        ' End Sub
        'AddHandler btnOCA.Click,
        '    Sub()
        '        Dim orderList As New List(Of OrderControl)
        '        For Each order In Orders
        '            If order.OrderStatus = OrderStatus.Inactive Then
        '                order.IsOCA = True
        '                order.OCAGroupName = Now.ToShortTimeString
        '            Else
        '                order.IsOCA = False
        '            End If
        '        Next
        '    End Sub
        'AddHandler btnBuyMarket.Click,
        ' Sub()
        '     Dim order As New OrderControl(Me) With {.Color = Colors.Green, .Quantity = GetOrderQuantity(), .SelectedColor = Colors.Green, .ButtonHoverColor = Colors.LightGreen,
        '        .Price = Price, .OrderType = OrderType.Market, .OrderAction = ActionSide.Buy}
        '     Children.Add(order)
        '     order.IsSelected = True
        '     order.Transmit()
        '     btnQuantity.Tag = 0
        ' End Sub
        'AddHandler btnSellMarket.Click,
        ' Sub()
        '     Dim order As New OrderControl(Me) With {.Color = Colors.Red, .Quantity = GetOrderQuantity(), .SelectedColor = Colors.Red, .ButtonHoverColor = Colors.Pink,
        '       .Price = Price, .OrderType = OrderType.Market, .OrderAction = ActionSide.Sell}
        '     Children.Add(order)
        '     order.IsSelected = True
        '     order.Transmit()
        '     btnQuantity.Tag = 0
        ' End Sub
        ''AddHandler btnQuantity.Click,
        '' Sub()
        ''     If SelectedOrder IsNot Nothing AndAlso SelectedOrder.OrderType <> OrderType.Market AndAlso SelectedOrder.OrderStatus <> OrderStatus.Filled Then
        ''         SelectedOrder.ChooseQuantity()
        ''     Else
        ''         ShowQuantityMenu(GetOrderQuantity, Settings("DefaultOrderQuantity").Value,
        ''          Sub(newQuantity As Integer)
        ''              btnQuantity.Content = newQuantity
        ''              btnQuantity.Tag = newQuantity
        ''          End Sub)
        ''     End If
        '' End Sub
        'AddHandler btnCancel.Click,
        ' Sub()
        '     If SelectedOrder IsNot Nothing Then SelectedOrder.Cancel()
        ' End Sub
        'AddHandler btnTransmit.Click,
        ' Sub()
        '     If btnTransmit.Content = "Transmit" Then
        '         If SelectedOrder IsNot Nothing Then SelectedOrder.Transmit()
        '     Else
        '         If SelectedOrder IsNot Nothing Then SelectedOrder.Resend()
        '     End If
        ' End Sub
        'AddHandler btnBuyStop.Click,
        '  Sub()
        '      Dim order As New OrderControl(Me) With {.Color = Color.FromArgb(255, 204, 204, 204), .SelectedColor = Colors.Orange, .ButtonHoverColor = Color.FromArgb(255, 255, 176, 98),
        '        .Price = Price, .Quantity = GetOrderQuantity(), .OrderType = OrderType.Stop, .OrderAction = ActionSide.Buy}
        '      Children.Add(order)
        '      order.IsSelected = True
        '      btnQuantity.Tag = 0
        '  End Sub
        'AddHandler btnSellStop.Click,
        ' Sub()
        '     Dim order As New OrderControl(Me) With {.Color = Color.FromArgb(255, 204, 204, 204), .SelectedColor = Color.FromArgb(255, 0, 127, 255), .ButtonHoverColor = Colors.LightBlue,
        '       .Price = Price, .Quantity = GetOrderQuantity(), .OrderType = OrderType.Stop, .OrderAction = ActionSide.Sell}
        '     Children.Add(order)
        '     order.IsSelected = True
        '     btnQuantity.Tag = 0
        ' End Sub
        'AddHandler btnBuyLimit.Click,
        ' Sub()
        '     Dim order As New OrderControl(Me) With {.Color = Color.FromArgb(255, 204, 204, 204), .SelectedColor = Colors.Green, .ButtonHoverColor = Colors.LightGreen,
        '      .Price = Price, .Quantity = GetOrderQuantity(), .OrderType = OrderType.Limit, .OrderAction = ActionSide.Buy}
        '     Children.Add(order)
        '     order.IsSelected = True
        '     btnQuantity.Tag = 0
        ' End Sub
        'AddHandler btnSellLimit.Click,
        ' Sub()
        '     Dim order As New OrderControl(Me) With {.Color = Color.FromArgb(255, 204, 204, 204), .SelectedColor = Color.FromArgb(255, 205, 0, 0), .ButtonHoverColor = Colors.Pink,
        '      .Price = Price, .Quantity = GetOrderQuantity(), .OrderType = OrderType.Limit, .OrderAction = ActionSide.Sell}
        '     Children.Add(order)
        '     order.IsSelected = True
        '     btnQuantity.Tag = 0
        ' End Sub
        'AddHandler btnCloseAll.Click,
        ' Sub()
        '     CloseOpenOrders()
        ' End Sub
        orderBox.Content = bd




        RefreshOrderBox()
    End Sub

    Private Sub CloseOpenOrders()
        If Not Settings("UseRandom").Value And Not DataStream.UseReplayMode(TickerID) Then
            If IB.IsConnected Then
                RemoveHandler IB.EventSources.OpenOrder, AddressOf OpenOrderReceived
                AddHandler IB.EventSources.OpenOrder, AddressOf OpenOrderReceived
                IB.reqAllOpenOrders()
            Else
                Log("attempted to load open orders but IB is not connected")
            End If
        End If
    End Sub
    Private Sub OpenOrderReceived(sender As Object, e As OpenOrderEventArgs)
        Dispatcher.BeginInvoke(
            Sub()
                ThreadSafe_OpenOrderReceived(sender, e)
            End Sub)
    End Sub
    Private Sub ThreadSafe_OpenOrderReceived(sender As Object, e As OpenOrderEventArgs)
        MsgBox(e.order.LmtPrice)
        'Dim chartIDToUse As Integer = -1
        'Dim existingOrderIDs As New List(Of Integer)
        'For Each chart In MasterWindow.Charts
        '    For Each ord In chart.Orders
        '        existingOrderIDs.Add(ord.OrderID)
        '    Next
        '    If chart.Settings("OrderBarVisibility").Value = True And chart.Settings("UseRandom").Value = False And chart.IB.UseReplayMode(chart.TickerID) = False And chart.IB.Contract(chart.TickerID).Symbol = e.Contract.Symbol And chart.HasLoadedData Then
        '        chartIDToUse = Max(chartIDToUse, chart.TickerID)
        '    End If
        'Next
        'If TickerID = chartIDToUse And Not existingOrderIDs.Contains(e.OrderId) Then
        '    Dim o As New OrderControl(Me, e.OrderId, e.OrderState, e.Contract)
        '    o.OrderAction = e.Order.Action
        '    o.Quantity = e.Order.TotalQuantity
        '    o.OrderType = e.Order.OrderType
        '    o.Price = If(e.Order.OrderType = OrderType.Stop Or e.Order.OrderType = OrderType.StopLimit, e.Order.AuxPrice, e.Order.LimitPrice)
        '    o.TransmittedPriceBeforeMove = If(e.Order.OrderType = OrderType.Stop Or e.Order.OrderType = OrderType.StopLimit, e.Order.AuxPrice, e.Order.LimitPrice)
        '    With o
        '        If .OrderAction = ActionSide.Buy And .OrderType = OrderType.Limit Then
        '            .Color = Color.FromArgb(255, 204, 204, 204) : .SelectedColor = Colors.Green : .ButtonHoverColor = Colors.LightGreen
        '        ElseIf .OrderAction = ActionSide.Buy And .OrderType = OrderType.Stop Then
        '            .Color = Color.FromArgb(255, 204, 204, 204) : .SelectedColor = Colors.Orange : .ButtonHoverColor = Color.FromArgb(255, 255, 176, 98)
        '        ElseIf .OrderAction = ActionSide.Sell And .OrderType = OrderType.Limit Then
        '            .Color = Color.FromArgb(255, 204, 204, 204) : .SelectedColor = Colors.Red : .ButtonHoverColor = Colors.Pink
        '        ElseIf .OrderAction = ActionSide.Sell And .OrderType = OrderType.Stop Then
        '            .Color = Color.FromArgb(255, 204, 204, 204) : .SelectedColor = Color.FromArgb(255, 0, 127, 255) : .ButtonHoverColor = Colors.LightBlue
        '        Else
        '            .Color = Color.FromArgb(255, 204, 204, 204) : .SelectedColor = Color.FromArgb(255, 150, 150, 150) : .ButtonHoverColor = Color.FromArgb(255, 250, 250, 250)
        '        End If
        '    End With
        '    Children.Add(o)
        'End If
    End Sub

    Private Sub InitGrid()
        For Each item In gridLines
            Children.Remove(item)
        Next
        gridLines.Clear()
        For i = 1 To Settings("GridLineCount").Value
            Dim ln As New TrendLine
            ln.Tag = "gridLine"
            ln.IsSelectable = False
            If Settings("ShowGrid").Value Then Children.Add(ln)
            gridLines.Add(ln)
        Next
    End Sub
    'Private Sub InitReplayControls()
    '    Children.Remove(replaySpeedSlider)
    '    replaySpeedSlider = New ChartSlider
    '    If IB.UseReplayMode(TickerID) And Not Settings("UseRandom").Value Then
    '        Children.Add(replaySpeedSlider)
    '    End If
    '    replaySpeedSlider.IsDraggable = False
    '    CType(replaySpeedSlider.Content, Slider).Focusable = False
    '    CType(replaySpeedSlider.Content, Slider).Minimum = 1
    '    CType(replaySpeedSlider.Content, Slider).SmallChange = 1
    '    CType(replaySpeedSlider.Content, Slider).LargeChange = 5
    '    CType(replaySpeedSlider.Content, Slider).Maximum = 50
    '    CType(replaySpeedSlider.Content, Slider).Height = 25
    '    CType(replaySpeedSlider.Content, Slider).Value = IB.ReplaySpeed(TickerID)
    '    AddHandler CType(replaySpeedSlider.Content, Slider).ValueChanged,
    '        Sub(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))
    '            IB.ReplaySpeed(TickerID) = CType(replaySpeedSlider.Content, Slider).Maximum + CType(replaySpeedSlider.Content, Slider).Minimum - e.NewValue
    '        End Sub
    'End Sub
    Private Sub InitRandomDataControls()
        Children.Remove(speedSlider)
        Children.Remove(deflectionSlider)
        speedSlider = New ChartSlider
        deflectionSlider = New ChartSlider
        If Settings("UseRandom").Value Then
            Children.Add(speedSlider)
            Children.Add(deflectionSlider)
        End If
        speedSlider.IsDraggable = False
        deflectionSlider.IsDraggable = False
        CType(speedSlider.Content, Slider).Focusable = False
        CType(speedSlider.Content, Slider).Minimum = 0
        CType(speedSlider.Content, Slider).SmallChange = 0.1
        CType(speedSlider.Content, Slider).LargeChange = 1
        CType(speedSlider.Content, Slider).Maximum = 25
        CType(speedSlider.Content, Slider).Height = 25
        CType(speedSlider.Content, Slider).Value = RAND.RandomSpeed
        CType(deflectionSlider.Content, Slider).Focusable = False
        CType(deflectionSlider.Content, Slider).Minimum = 0
        CType(deflectionSlider.Content, Slider).SmallChange = 0.01
        CType(deflectionSlider.Content, Slider).LargeChange = 0.1
        CType(deflectionSlider.Content, Slider).Maximum = 1
        CType(deflectionSlider.Content, Slider).Height = 25
        CType(deflectionSlider.Content, Slider).Value = RAND.RandomDeflection
        CType(deflectionSlider.Content, Slider).ContextMenu = New ContextMenu With {.ItemsSource = {New MenuItem With {.Header = "Center"}}}
        AddHandler CType(CType(deflectionSlider.Content, Slider).ContextMenu.Items(0), MenuItem).Click, Sub() CType(deflectionSlider.Content, Slider).Value = 0.5
        AddHandler CType(speedSlider.Content, Slider).ValueChanged, Sub(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) RAND.RandomSpeed = e.NewValue
        AddHandler CType(deflectionSlider.Content, Slider).ValueChanged, Sub(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) RAND.RandomDeflection = e.NewValue
        'AddHandler CType(speedSlider.Content, Slider).MouseUp, Sub() RAND.RandomSpeed = speedSlider.Content.Value
        RefreshRandomDataControlsLocation()
        'RefreshReplayControlsLocation()
    End Sub
    Private Sub InitAxis()
        Dim fontSize As Double = 14
        If axisTextBlockStyle.Setters.Count = 0 Then
            axisTextBlockStyle.Setters.Add(New Setter(TextBlock.FontFamilyProperty, (New FontFamilyConverter).ConvertFromString("Global Sans Serif")))
            axisTextBlockStyle.Setters.Add(New Setter(TextBlock.FontSizeProperty, fontSize))
            'axisTextBlockStyle.Setters.Add(New Setter(TextBlock.WidthProperty, Settings("PriceBarWidth").Value))
            axisTextBlockStyle.Setters.Add(New Setter(TextBlock.HeightProperty, fontSize))
            axisTextBlockStyle.Setters.Add(New Setter(TextBlock.MarginProperty, New Thickness(3, 0, 3, 0)))
        End If

        If statusAxis.Content Is Nothing Then
            statusAxis.Content = New Grid
            statusAxis.ContentChildren.Add(loadingProgressBar)
            statusAxis.ContentChildren.Add(statusText)
        End If

        priceAxis.Content = New Canvas
        timeAxis.Content = New Canvas

        priceAxis.Content.Background = New SolidColorBrush(Settings("Price Axis Background").Value)
        timeAxis.Content.Background = New SolidColorBrush(Settings("Time Axis Background").Value)
        statusAxis.Background = New SolidColorBrush(Settings("Status Text Background").Value)

        Children.Remove(priceAxis)
        Children.Add(priceAxis)
        Children.Remove(timeAxis)
        Children.Add(timeAxis)
        Children.Remove(statusAxis)
        Children.Add(statusAxis)
    End Sub
    Private Sub SetControlsOnTop(Optional ByVal parameter As Object = Nothing)
        If parameter IsNot priceAxis And parameter IsNot timeAxis And parameter IsNot statusAxis And parameter IsNot orderBox Then
            For Each technique In AnalysisTechniques
                If GetType(IChartPadAnalysisTechnique).IsAssignableFrom(technique.AnalysisTechnique.GetType) Then
                    If CType(technique.AnalysisTechnique, IChartPadAnalysisTechnique).ChartPad IsNot Nothing AndAlso parameter Is CType(technique.AnalysisTechnique, IChartPadAnalysisTechnique).ChartPad Then
                        Exit Sub
                    End If
                End If
            Next
            Children.Remove(priceAxis)
            Children.Add(priceAxis)
            Children.Remove(timeAxis)
            Children.Add(timeAxis)
            Children.Remove(statusAxis)
            Children.Add(statusAxis)
            Children.Remove(orderBox)
            If mnuViewOrderBar IsNot Nothing AndAlso mnuViewOrderBar.IsChecked And bars.Count > 0 Then
                Children.Add(orderBox)
            End If
            For Each technique In AnalysisTechniques
                If GetType(IChartPadAnalysisTechnique).IsAssignableFrom(technique.AnalysisTechnique.GetType) Then
                    If CType(technique.AnalysisTechnique, IChartPadAnalysisTechnique).ChartPadVisible Then
                        Children.Remove(CType(technique.AnalysisTechnique, IChartPadAnalysisTechnique).ChartPad)
                        If bars.Count > 0 And technique.AnalysisTechnique.IsEnabled Then Children.Add(CType(technique.AnalysisTechnique, IChartPadAnalysisTechnique).ChartPad)
                    End If
                End If
            Next
        End If
    End Sub
    Private Sub RefreshAxisAndGrid()
        If IsLoaded And BarIndex > 0 And Not Double.IsNaN(Bounds.Y1) And Not Double.IsNaN(Bounds.Y2) Then
            timeAxis.ContentChildren.Clear()
            priceAxis.ContentChildren.Clear()
            If BarIndex > 1 Then
                Dim topNum As Double = RoundTo(-CurrentBar.Close - Bounds.Height / 4, Settings("PriceBarTextInterval").Value)
                Dim botNum As Double = RoundTo(-CurrentBar.Close + Bounds.Height / 4, Settings("PriceBarTextInterval").Value)
                'Dim num As Integer = (botNum - topNum + Settings("PriceBarTextInterval").Value) / Settings("PriceBarTextInterval").Value
                'If Settings("ShowGrid").Value Then
                '    Dim gridLineIndex As Integer
                '    Dim gridLineCount As Integer = Settings("GridLineCount").Value - 1
                '    Dim interval As Double = Settings("PriceBarTextInterval").Value
                '    For price As Double = RoundTo(-C - gridLineCount * interval / 2, interval) To RoundTo(gridLineCount * interval / 2 - C, interval) Step interval
                '        gridLines(gridLineIndex).StartPoint = New Point(GetRelativeFromRealX(ActualWidth - Settings("GridWidth").Value * ActualWidth - Settings("PriceBarWidth").Value), price)
                '        gridLines(gridLineIndex).EndPoint = New Point(GetRelativeFromRealX(ActualWidth), price)
                '        gridLines(gridLineIndex).Pen.Brush = New SolidColorBrush(Settings("Grid").Value)
                '        gridLines(gridLineIndex).RefreshVisual()
                '        gridLineIndex += 1
                '    Next
                'End If
                For price As Double = topNum - Settings("PriceBarTextInterval").Value To botNum Step Settings("PriceBarTextInterval").Value
                    Dim txt As New Controls.TextBlock
                    txt.Text = "────────────────────────────────────────────────────" 'FormatNumber(-Math.Round(price, Settings("DecimalPlaces").Value), Settings("DecimalPlaces").Value, TriState.True, TriState.False).Replace(",", "")
                    txt.FontSize = 14
                    txt.Height = txt.FontSize + 2
                    txt.Foreground = New SolidColorBrush(Settings("Grid").Value)
                    txt.Background = New SolidColorBrush(Settings("Axis Text Background").Value)
                    priceAxis.ContentChildren.Add(txt)
                    Canvas.SetTop(txt, GetRealFromRelative(New Point(0, price)).Y - txt.Height / 2)
                Next

                For i As Integer = RoundTo(Max(0, Bounds.X1), Settings("TimeBarTextInterval").Value) + 1 To RoundTo(Min(BarIndex, Bounds.X2), Settings("TimeBarTextInterval").Value) Step Settings("TimeBarTextInterval").Value
                    'For I As Integer = Max(Min(BarIndex, Bounds.X1 + 1), 10) To Min(BarIndex, Bounds.X2) - 1 Step Settings("TimeBarTextInterval").Value
                    Dim txt As New TextBlock
                    txt.Text = bars(i - 1).Data.Date.ToShortTimeString
                    txt.FontSize = 14
                    txt.Height = txt.FontSize + 2
                    txt.Foreground = New SolidColorBrush(Settings("Axis Text Foreground").Value)
                    txt.Background = New SolidColorBrush(Settings("Axis Text Background").Value)
                    timeAxis.ContentChildren.Add(txt)
                    Canvas.SetTop(txt, 0)
                    Canvas.SetLeft(txt, GetRealFromRelativeX(i))
                    AddHandler txt.Loaded, AddressOf TimeTextLoaded
                Next
            End If
            If Mode = ClickMode.Normal Then Window_MouseMove(Me, New MouseEventArgs(Mouse.PrimaryDevice, My.Computer.Clock.TickCount))
            DrawCursorTexts()
            DrawPriceText()
        End If
        statusText.FontSize = 15
        SetStatusTextLocation()
        statusText.Foreground = New SolidColorBrush(Settings("Status Text Foreground").Value)
        statusAxis.Content.Background = New SolidColorBrush(Settings("Status Text Background").Value)
    End Sub
    Private Sub DrawPriceText()
        If priceAxis.ContentChildren.Contains(priceText) Then priceAxis.ContentChildren.Remove(priceText)
        priceText = New TextBlock
        priceAxis.ContentChildren.Add(priceText)
        priceText.Style = axisTextBlockStyle
        priceText.Padding = New Thickness(2, 5, 2, 5)
        priceText.Height = 26
        priceText.Foreground = New SolidColorBrush(Settings("Price Text Foreground").Value)
        priceText.Background = New SolidColorBrush(Settings("Price Text Background").Value)
        priceText.Text = FormatNumber(Price, Settings("DecimalPlaces").Value, TriState.True, TriState.False).Replace(",", "")
        priceText.FontWeight = FontWeights.Bold
        priceText.SetValue(Canvas.TopProperty, GetRealFromRelativeY(-Price) - priceText.Height / 2)
        priceText.SetValue(Canvas.LeftProperty, 0.0)
    End Sub
    Private Sub TimeTextLoaded(ByVal sender As Object, ByVal e As EventArgs)
        Canvas.SetLeft(sender, Canvas.GetLeft(sender) - sender.ActualWidth / 2)
        If sender Is timeCursorText Then
            sender.Width = sender.ActualWidth + 85
            Canvas.SetLeft(sender, Canvas.GetLeft(sender) - sender.Width / 2 - 10)
        End If
    End Sub
    Private Function FormattedPriceString() As String
        Return FormatNumber(Math.Round(Price, Settings("DecimalPlaces").Value), Settings("DecimalPlaces").Value, TriState.True, TriState.False)
    End Function
    Private Sub DrawCursorTexts()
        If MouseDown = MouseDownType.CapturedOnMe And Mode = ClickMode.Normal And Mouse.LeftButton = MouseButtonState.Pressed Then
            Dim relativeClickPoint As Point = GetRelativeFromReal(Mouse.GetPosition(mainCanvas))
            If timeAxis.ContentChildren.Contains(timeCursorText) Then timeAxis.ContentChildren.Remove(timeCursorText)
            If priceAxis.ContentChildren.Contains(priceCursorText) Then priceAxis.ContentChildren.Remove(priceCursorText)
            priceCursorText = New TextBlock
            timeCursorText = New Controls.Label
            timeCursorText.HorizontalContentAlignment = Windows.HorizontalAlignment.Center
            timeCursorText.Padding = New Thickness(0)
            AddHandler timeCursorText.Loaded, AddressOf TimeTextLoaded
            If bars.Count > 0 Then
                priceCursorText.Style = axisTextBlockStyle
                priceCursorText.Foreground = New SolidColorBrush(Settings("Cursor Text Foreground").Value)
                priceCursorText.Background = New SolidColorBrush(Settings("Cursor Text Background").Value)
                priceAxis.ContentChildren.Add(priceCursorText)
                priceCursorText.Text = FormatNumber(Math.Round(-RoundTo(relativeClickPoint.Y, GetMinTick), Settings("DecimalPlaces").Value), Settings("DecimalPlaces").Value, TriState.True, TriState.False).Replace(",", "")
                priceCursorText.SetValue(Canvas.TopProperty, GetRealFromRelativeY(RoundTo(GetRelativeFromRealY(Mouse.GetPosition(priceAxis).Y), GetMinTick)) - priceCursorText.Height / 2)
            End If
            If relativeClickPoint.X >= 1 And relativeClickPoint.X <= BarIndex Then
                timeCursorText.Style = axisTextBlockStyle
                timeCursorText.Foreground = New SolidColorBrush(Settings("Cursor Text Foreground").Value)
                timeCursorText.Background = New SolidColorBrush(Settings("Cursor Text Background").Value)
                timeCursorText.Content = bars(relativeClickPoint.X - 1).Data.Date.ToShortDateString & ", " & bars(relativeClickPoint.X - 1).Data.Date.ToShortTimeString
                timeAxis.ContentChildren.Add(timeCursorText)
                timeCursorText.SetValue(Canvas.LeftProperty, GetRealFromRelativeX(RoundTo(GetRelativeFromRealX(Mouse.GetPosition(timeAxis).X), 1)) +
                    8 + (New FormattedText(CStr(timeCursorText.Content), Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, New Typeface(timeCursorText.FontFamily, timeCursorText.FontStyle, timeCursorText.FontWeight, timeCursorText.FontStretch), timeCursorText.FontSize, Brushes.White)).WidthIncludingTrailingWhitespace)

            End If
        End If
    End Sub
    Private Sub TickVisibilityChanged()
        For Each item In bars
            item.OpenTick = Settings("DisplayOpenTick").Value
            item.CloseTick = Settings("DisplayCloseTick").Value
            item.RefreshVisual()
        Next
    End Sub

    Dim originalMode As ClickMode = ClickMode.Undefined
    Private Sub timeAxis_PreviewMouseMove(ByVal sender As Object, ByVal e As System.Windows.Input.MouseEventArgs) Handles timeAxis.PreviewMouseMove
        If Mode = ClickMode.TimeAxisDrag Then
            timeAxis.Cursor = Cursors.SizeWE
        Else
            timeAxis.Cursor = Cursors.Arrow
        End If
    End Sub
    Private Sub priceAxis_PreviewMouseMove(ByVal sender As Object, ByVal e As System.Windows.Input.MouseEventArgs) Handles priceAxis.PreviewMouseMove
        If Mode = ClickMode.PriceAxisDrag Then
            priceAxis.Cursor = Cursors.SizeNS
        Else
            priceAxis.Cursor = Cursors.Arrow
        End If
    End Sub
    Private Sub priceAxis_PreviewMouseLeftButtonDown(ByVal sender As Object, ByVal e As System.Windows.Input.MouseButtonEventArgs) Handles priceAxis.PreviewMouseLeftButtonDown
        originalMode = Mode
        Mode = ClickMode.PriceAxisDrag
        priceAxis.CaptureMouse()
    End Sub
    Private timeAxisClickMarginRelativeWidth As Double
    Private Sub timeAxis_PreviewMouseLeftButtonDown(ByVal sender As Object, ByVal e As System.Windows.Input.MouseButtonEventArgs) Handles timeAxis.PreviewMouseLeftButtonDown
        originalMode = Mode
        Mode = ClickMode.TimeAxisDrag
        timeAxisClickMarginRelativeWidth = GetRelativeFromRealWidth(Settings("XMargin").Value * mainCanvas.ActualWidth + Settings("PriceBarWidth").Value)
        timeAxis.CaptureMouse()
    End Sub

    Private Sub mainCanvas_SizeChanged(ByVal sender As Object, ByVal e As System.Windows.SizeChangedEventArgs) Handles mainCanvas.SizeChanged
        RefreshAllSize()
        RefreshAxisAndGrid()
        If IsLoaded Then
            For Each child As IChartObject In Children
                child.ParentBoundsChanged()
            Next
        End If
    End Sub



#End Region

#Region "Children"
    Private _visuals As New List(Of Visual)
    Private WithEvents _children As New NotifyingCollection(Of IChartObject)
    Public Overloads Property Children As NotifyingCollection(Of IChartObject)
        Get
            Return _children
        End Get
        Set(ByVal value As NotifyingCollection(Of IChartObject))
            _children = value
        End Set
    End Property
    Protected Overrides Sub OnVisualChildrenChanged(ByVal visualAdded As System.Windows.DependencyObject, ByVal visualRemoved As System.Windows.DependencyObject)
        If Not borderLoading Then
            If visualAdded IsNot Nothing Then
                If MyBase.Children.Contains(visualAdded) Then
                    MyBase.Children.Remove(visualAdded)
                    mainCanvas.AddVisual(visualAdded)
                End If
            End If
            If visualRemoved IsNot Nothing Then mainCanvas.RemoveVisual(visualRemoved)
        Else
            MyBase.OnVisualChildrenChanged(visualAdded, visualRemoved)
        End If
    End Sub

    Private Sub _children_ChildAdded(ByVal sender As Object, ByVal e As ItemChangedEventArgs(Of IChartObject)) Handles _children.ItemAdded
        If Not TypeOf e.Item Is IChartObject Then
            e.Cancel = True
        Else
            mainCanvas.AddVisual(e.Item)
            If TypeOf e.Item Is OrderControl Then _orders.Add(e.Item)
            If Not IsLoadingHistory And analysisTechniquesLoading = False Then SetControlsOnTop(e.Item)
        End If
    End Sub
    Private Sub _children_ChildRemoved(ByVal sender As Object, ByVal e As ItemChangedEventArgs(Of IChartObject)) Handles _children.ItemRemoved
        If Not TypeOf e.Item Is IChartObject Then
            e.Cancel = True
        Else
            If TypeOf e.Item Is OrderControl Then _orders.Remove(e.Item)
            mainCanvas.RemoveVisual(e.Item)
        End If
    End Sub

    Private Sub RefreshChartChildren()
        For i = 0 To Children.Count - 1
            If i < Children.Count AndAlso TypeOf Children(i) Is DrawingVisual Then
                CType(Children(i), DrawingVisual).CanRefresh = True
                CType(Children(i), DrawingVisual).RefreshVisual()
            End If
        Next
    End Sub

    Public Sub DeselectAllChildren()
        For i = 0 To Children.Count - 1
            If i < Children.Count AndAlso TypeOf Children(i) Is ISelectable AndAlso CType(Children(i), ISelectable).IsSelectable Then CType(Children(i), ISelectable).IsSelected = False
        Next
    End Sub

    Private _orders As New List(Of OrderControl)
    Public ReadOnly Property Orders As ReadOnlyCollection(Of OrderControl)
        Get
            'Dim list As New List(Of Order)
            'For Each item In Children
            '    If TypeOf item Is Order Then list.Add(item)
            'Next
            Return New ReadOnlyCollection(Of OrderControl)(_orders)
        End Get
    End Property
    Public ReadOnly Property SelectedOrder As OrderControl
        Get
            For Each order In Orders
                If order.IsSelectable AndAlso order.IsSelected Then
                    Return order
                End If
            Next
            Return Nothing
        End Get
    End Property
#End Region

#Region "Border Code"

    Public WithEvents mainCanvas As New DrawingVisualCanvas

    Dim dragRects(7) As Shapes.Rectangle
    Dim borderRects(3) As Border
    'Dim bd As New Border
    Dim borderLoading As Boolean = True

    Private _selected As Boolean
    Public Property IsSelected As Boolean Implements ISelectable.IsSelected
        Get
            Return _selected
        End Get
        Set(ByVal value As Boolean)
            _selected = value
            If IsLoaded And DesktopWindow IsNot Nothing Then
                If value Then
                    SetBorderColor(Colors.DarkGray)
                    Dim chartCount As Integer = Parent.Charts.Count
                    Canvas.SetZIndex(Me, chartCount + 1)
                    If Settings("Topmost").Value Then
                        Canvas.SetZIndex(Me, chartCount * 2 + 1)
                    End If
                    For Each item In Parent.Charts
                        If item IsNot Me Then
                            If item.Settings("Topmost").Value Then
                                Canvas.SetZIndex(item, Canvas.GetZIndex(item) + chartCount)
                            End If
                            If item.IsSelected Then item.IsSelected = False
                        End If
                    Next
                    Parent.OrderCharts()
                Else
                    Dim col As Color = Colors.DarkGray
                    col.A = 30
                    SetBorderColor(col)
                    For Each item As IChartObject In Children
                        If TypeOf item Is ISelectable AndAlso CType(item, ISelectable).IsSelectable AndAlso CType(item, ISelectable).IsSelected Then CType(item, ISelectable).IsSelected = False
                    Next
                End If
            End If
        End Set
    End Property

    Private Sub SetBorderColor(ByVal color As Color)
        For i = 0 To 3
            borderRects(i).Background = New SolidColorBrush(color)
        Next
        'For i = 0 To 7
        'rects(i).Fill = New SolidColorBrush(color)
        'Next
    End Sub

    Public Property IsSelectable As Boolean = True Implements ISelectable.IsSelectable

    Private Sub InitBorder()
        MinHeight = 200
        MinWidth = 200
        ColumnDefinitions.Clear()
        RowDefinitions.Clear()
        MyBase.Children.Clear()
        SnapsToDevicePixels = True

        Dim bdWidth As Double = My.Application.ChartBorderWidth
        Dim column1 As New ColumnDefinition
        column1.Width = New GridLength(bdWidth)
        Dim column2 As New ColumnDefinition
        column2.Width = New GridLength(1, GridUnitType.Star)
        Dim column3 As New ColumnDefinition
        column3.Width = New GridLength(bdWidth)
        Dim row1 As New RowDefinition
        row1.Height = New GridLength(bdWidth)
        Dim row2 As New RowDefinition
        row2.Height = New GridLength(1, GridUnitType.Star)
        Dim row3 As New RowDefinition
        row3.Height = New GridLength(bdWidth)
        ColumnDefinitions.Add(column1)
        ColumnDefinitions.Add(column2)
        ColumnDefinitions.Add(column3)
        RowDefinitions.Add(row1)
        RowDefinitions.Add(row2)
        RowDefinitions.Add(row3)

        For i = 0 To 3
            borderRects(i) = New Border
        Next

        borderRects(0).CornerRadius = New CornerRadius(Max(bdWidth, 3), 0, 0, Max(bdWidth, 3))
        Grid.SetRowSpan(borderRects(0), 3)

        Grid.SetColumn(borderRects(1), 1)

        Grid.SetColumn(borderRects(2), 2)
        Grid.SetRowSpan(borderRects(2), 3)
        borderRects(2).CornerRadius = New CornerRadius(0, Max(bdWidth, 3), Max(bdWidth, 3), 0)

        Grid.SetColumn(borderRects(3), 1)
        Grid.SetRow(borderRects(3), 2)

        For i = 0 To 3
            MyBase.Children.Add(borderRects(i))
        Next


        mainCanvas.ClipToBounds = True
        Grid.SetRow(mainCanvas, 1)
        Grid.SetColumn(mainCanvas, 1)
        MyBase.Children.Add(mainCanvas)

        For i = 0 To 7
            dragRects(i) = New Shapes.Rectangle
            dragRects(i).Fill = Brushes.Transparent
        Next

        Dim col As Color = Colors.LightGray
        col.A = 100
        SetBorderColor(col)

        Grid.SetRow(dragRects(0), 0)
        Grid.SetColumn(dragRects(0), 0)
        Grid.SetRow(dragRects(1), 0)
        Grid.SetColumn(dragRects(1), 1)
        Grid.SetRow(dragRects(2), 0)
        Grid.SetColumn(dragRects(2), 2)
        Grid.SetRow(dragRects(3), 1)
        Grid.SetColumn(dragRects(3), 0)
        Grid.SetRow(dragRects(4), 1)
        Grid.SetColumn(dragRects(4), 2)
        Grid.SetRow(dragRects(5), 2)
        Grid.SetColumn(dragRects(5), 0)
        Grid.SetRow(dragRects(6), 2)
        Grid.SetColumn(dragRects(6), 1)
        Grid.SetRow(dragRects(7), 2)
        Grid.SetColumn(dragRects(7), 2)

        Dim innerClickAreaWidth As Double = 3 '6 / 3.3
        Dim outerClickAreaWidth As Double = 4

        dragRects(0).Margin = New Thickness(-outerClickAreaWidth, -outerClickAreaWidth, -innerClickAreaWidth, -innerClickAreaWidth)
        dragRects(1).Margin = New Thickness(innerClickAreaWidth, -outerClickAreaWidth, innerClickAreaWidth, -innerClickAreaWidth)
        dragRects(2).Margin = New Thickness(-innerClickAreaWidth, -outerClickAreaWidth, -outerClickAreaWidth, -innerClickAreaWidth)
        dragRects(3).Margin = New Thickness(-outerClickAreaWidth, innerClickAreaWidth, -innerClickAreaWidth, innerClickAreaWidth)
        dragRects(4).Margin = New Thickness(-innerClickAreaWidth, innerClickAreaWidth, -outerClickAreaWidth, innerClickAreaWidth)
        dragRects(5).Margin = New Thickness(-outerClickAreaWidth, -innerClickAreaWidth, -innerClickAreaWidth, -outerClickAreaWidth)
        dragRects(6).Margin = New Thickness(innerClickAreaWidth, -innerClickAreaWidth, innerClickAreaWidth, -outerClickAreaWidth)
        dragRects(7).Margin = New Thickness(-innerClickAreaWidth, -innerClickAreaWidth, -outerClickAreaWidth, -outerClickAreaWidth)

        dragRects(0).Cursor = Cursors.SizeNWSE
        dragRects(1).Cursor = Cursors.SizeAll
        dragRects(2).Cursor = Cursors.SizeNESW
        dragRects(3).Cursor = Cursors.SizeWE
        dragRects(4).Cursor = Cursors.SizeWE
        dragRects(5).Cursor = Cursors.SizeNESW
        dragRects(6).Cursor = Cursors.SizeNS
        dragRects(7).Cursor = Cursors.SizeNWSE

        For i = 0 To 7
            MyBase.Children.Add(dragRects(i))
        Next

        IsSelected = False
        borderLoading = False

        mainCanvas.Background = Brushes.Red
        SetBorderColor(Colors.Blue)
        MyBase.Background = Brushes.Transparent

    End Sub
    Private Sub LoadBorder()
        Dim contextMenu As New CommandContextMenu(DesktopWindow)
        Dim mnuTopmost As MenuItem = NewMenuItemWrapper("Always on Top", "")
        mnuTopmost.IsCheckable = True
        mnuTopmost.IsChecked = Settings("Topmost").Value
        Dim [sub] =
         Sub()
             Settings("Topmost").Value = mnuTopmost.IsChecked
             IsSelected = True
         End Sub
        AddHandler mnuTopmost.Checked, [sub]
        AddHandler mnuTopmost.Unchecked, [sub]
        contextMenu.Items.Add(NewMenuItemWrapper("Maximize", "", , Sub() Settings("Maximized").Value = True))
        contextMenu.Items.Add(NewMenuItemWrapper("Restore", "", , Sub() Settings("Maximized").Value = False))
        contextMenu.Items.Add(mnuTopmost)
        contextMenu.Items.Add(NewMenuItemWrapper("Close", "", ChartCommands.CloseChart))
        dragRects(1).ContextMenu = contextMenu
    End Sub
    Private Sub Parent_SizeChanged(ByVal sender As Object, ByVal e As System.Windows.SizeChangedEventArgs) Handles _parent.SizeChanged
        If Settings("Maximized").Value Then
            Width = Parent.ActualWidth
            Height = Parent.ActualHeight
        End If
    End Sub

    Dim _size As Size = New Size(MinWidth + 400, MinHeight + 400), _loc As New Point(0, 0)
    Public Property Size As Size
        Get
            Return _size
        End Get
        Set(ByVal value As Size)
            'ShowInfoBox("Size was set to " & value.ToString & ".")
            _size = value
            If Not Settings("Maximized").Value Then
                Width = value.Width
                Height = value.Height
            End If
        End Set
    End Property
    Public Property Location As Point
        Get
            Return _loc
        End Get
        Set(ByVal value As Point)
            _loc = value
            If Not Settings("Maximized").Value Then
                Canvas.SetLeft(Me, value.X)
                Canvas.SetTop(Me, value.Y)
            End If
        End Set
    End Property
    Public Property SizeHeight As Double
        Get
            Return Size.Height
        End Get
        Set(ByVal value As Double)
            Size = New Size(Size.Width, value)
        End Set
    End Property
    Public Property SizeWidth As Double
        Get
            Return Size.Width
        End Get
        Set(ByVal value As Double)
            Size = New Size(value, Size.Height)
        End Set
    End Property
    Public Property LocationX As Double
        Get
            Return Location.X
        End Get
        Set(ByVal value As Double)
            Location = New Point(value, Location.Y)
        End Set
    End Property
    Public Property LocationY As Double
        Get
            Return Location.Y
        End Get
        Set(ByVal value As Double)
            Location = New Point(Location.X, value)
        End Set
    End Property

    Dim borderClickPoint As Point, borderClickSize As Size, borderClickMePos As Point, borderClickItem As Shapes.Rectangle, borderPress As Boolean, prevCur As Cursor
    Private Sub Border_MouseMove(ByVal args As MouseEventArgs)
        If Settings("Maximized").Value Then Exit Sub
        Dim index As Integer = Array.IndexOf(dragRects, borderClickItem)
        If Not borderPress Or My.Application.ChartBorderWidth = 0 Then index = -1
        Dim dif As Point = borderClickPoint - args.GetPosition(Parent)
        Dim minimumSize As Size = New Size(MinWidth, MinHeight) 'New Size(100, 100)
        If LocationY <= 0 Then
            LocationY = 1
        End If
        Select Case index
            Case 0 ' top left
                If borderClickMePos.X - dif.X < 0 Then
                    LocationX = 1
                    SizeWidth = borderClickSize.Width + borderClickMePos.X - 1
                End If
                If borderClickSize.Width + dif.X > minimumSize.Width And borderClickMePos.X - dif.X > 0 Then
                    LocationX = borderClickMePos.X - dif.X
                    SizeWidth = borderClickSize.Width + dif.X
                End If
                If borderClickSize.Height + dif.Y > minimumSize.Height Then
                    LocationY = borderClickMePos.Y - dif.Y
                    SizeHeight = borderClickSize.Height + dif.Y
                End If
                If borderClickMePos.Y - dif.Y < 0 Then
                    LocationY = 1
                    SizeHeight = borderClickSize.Height + borderClickMePos.Y - 1
                End If
            Case 1 'top
                If borderClickMePos.Y - dif.Y > 0 And borderClickMePos.Y - dif.Y + SizeHeight < Parent.ActualHeight Then
                    LocationY = borderClickMePos.Y - dif.Y
                ElseIf borderClickMePos.Y - dif.Y <= 0 Then
                    LocationY = 1
                Else
                    LocationY = Parent.ActualHeight - SizeHeight - 1
                End If

                If borderClickMePos.X - dif.X > 0 And borderClickMePos.X - dif.X + SizeWidth < Parent.ActualWidth Then
                    LocationX = borderClickMePos.X - dif.X
                ElseIf borderClickMePos.X - dif.X <= 0 Then
                    LocationX = 1
                Else
                    LocationX = Parent.ActualWidth - SizeWidth - 1
                End If
            Case 2 'top right
                If borderClickSize.Height + dif.Y > minimumSize.Height Then
                    LocationY = borderClickMePos.Y - dif.Y
                    SizeHeight = borderClickSize.Height + dif.Y
                End If
                If borderClickMePos.Y - dif.Y < 0 Then
                    LocationY = 1
                    SizeHeight = borderClickSize.Height + borderClickMePos.Y - 1
                End If
                If borderClickSize.Width - dif.X > minimumSize.Width Then SizeWidth = borderClickSize.Width - dif.X
                If borderClickSize.Width - dif.X + borderClickMePos.X > Parent.ActualWidth - 1 Then SizeWidth = Parent.ActualWidth - borderClickMePos.X - 1
            Case 3 ' left
                If borderClickSize.Width + dif.X > minimumSize.Width And borderClickMePos.X - dif.X > 0 Then
                    LocationX = borderClickMePos.X - dif.X
                    SizeWidth = borderClickSize.Width + dif.X
                End If
                If borderClickMePos.X - dif.X < 0 Then
                    LocationX = 1
                    SizeWidth = borderClickSize.Width + borderClickMePos.X - 1
                End If
            Case 4 ' right
                If borderClickSize.Width - dif.X > minimumSize.Width Then SizeWidth = borderClickSize.Width - dif.X
                If borderClickSize.Width - dif.X + borderClickMePos.X > Parent.ActualWidth - 1 Then SizeWidth = Parent.ActualWidth - borderClickMePos.X - 1
            Case 5 ' bottom left
                If borderClickSize.Width + dif.X > minimumSize.Width And borderClickMePos.X - dif.X > 0 Then
                    LocationX = borderClickMePos.X - dif.X
                    SizeWidth = borderClickSize.Width + dif.X
                End If
                If borderClickMePos.X - dif.X < 0 Then
                    LocationX = 1
                    SizeWidth = borderClickSize.Width + borderClickMePos.X - 1
                End If
                If borderClickSize.Height - dif.Y > minimumSize.Height Then SizeHeight = borderClickSize.Height - dif.Y
                If borderClickSize.Height - dif.Y + borderClickMePos.Y > Parent.ActualHeight - 1 Then SizeHeight = Parent.ActualHeight - borderClickMePos.Y - 1
            Case 6 ' bottom
                If borderClickSize.Height - dif.Y > minimumSize.Height Then SizeHeight = borderClickSize.Height - dif.Y
                If borderClickSize.Height - dif.Y + borderClickMePos.Y > Parent.ActualHeight - 1 Then SizeHeight = Parent.ActualHeight - borderClickMePos.Y - 1
            Case 7 ' bottom right
                If borderClickSize.Width - dif.X > minimumSize.Width Then SizeWidth = borderClickSize.Width - dif.X
                If borderClickSize.Width - dif.X + borderClickMePos.X > Parent.ActualWidth - 1 Then SizeWidth = Parent.ActualWidth - borderClickMePos.X - 1
                If borderClickSize.Height - dif.Y > minimumSize.Height Then SizeHeight = borderClickSize.Height - dif.Y
                If borderClickSize.Height - dif.Y + borderClickMePos.Y > Parent.ActualHeight - 1 Then SizeHeight = Parent.ActualHeight - borderClickMePos.Y - 1
        End Select


        'ElseIf borderClickMePos.Y - dif.Y + SizeHeight >= Parent.ActualHeight Then
        '    LocationY = Parent.ActualHeight - SizeHeight - 1
        'If borderClickMePos.X - dif.X <= 0 Then
        '    LocationX = 1
        'ElseIf borderClickMePos.X - dif.X + SizeWidth >= Parent.ActualWidth Then
        '    LocationX = Parent.ActualWidth - SizeWidth - 1
        'End If

        If index >= 0 And index <= 7 And index <> 1 Then RefreshAllSize()

        'If Not (borderClickMePos.Y - dif.Y > 0 And borderClickMePos.Y - dif.Y + SizeHeight < Parent.ActualHeight) Then
        '    If borderClickMePos.Y - dif.Y <= 0 And borderClickMePos.Y - dif.Y + SizeHeight < Parent.ActualHeight Then
        '        LocationY = 1
        '    ElseIf borderClickMePos.Y - dif.Y > 0 And borderClickMePos.Y - dif.Y + SizeHeight >= Parent.ActualHeight Then
        '        LocationY = Parent.ActualHeight - SizeHeight - 1
        '    Else
        '        LocationY = 1
        '        SizeHeight = Parent.ActualHeight - 2
        '    End If
        'End If
        'If Not (borderClickMePos.X - dif.X > 0 And borderClickMePos.X - dif.X + SizeWidth < Parent.ActualWidth) Then
        '    If borderClickMePos.X - dif.X <= 0 Then
        '        LocationX = 1
        '    Else
        '        LocationX = Parent.ActualWidth - SizeWidth - 1
        '    End If
        'End If

    End Sub
    Private Sub Border_MouseLeftButtonDown(ByVal sender As Object, ByVal args As MouseButtonEventArgs) Handles Me.PreviewMouseLeftButtonDown
        prevCur = mainCanvas.Cursor
        If TypeOf args.Source Is Shapes.Rectangle AndAlso dragRects.Contains(args.Source) Then
            ChangeCursor(CType(args.Source, Shapes.Rectangle).Cursor)
            borderClickItem = args.Source
            borderClickPoint = args.GetPosition(Parent)
            borderClickSize = Size
            borderClickMePos = Location
            borderPress = True
            args.Source.CaptureMouse()
        Else
            borderClickItem = Nothing
            borderPress = False
            DesktopWindow.Cursor = Nothing
        End If
    End Sub
    Private Sub Border_MouseButtonDown(ByVal sender As Object, ByVal args As MouseButtonEventArgs) Handles Me.PreviewMouseDown
        If doubleClick Then
            doubleClick = False
            Exit Sub
        End If
        If Not IsSelected Then IsSelected = True
    End Sub
    Private Sub Border_MouseLeftButtonUp(ByVal sender As Object, ByVal args As MouseButtonEventArgs)
        If borderClickItem IsNot Nothing Then
            borderPress = False
            If prevCur IsNot Cursors.Arrow AndAlso prevCur IsNot Nothing Then
                ChangeCursor(prevCur)
            Else
                ResetCursor()
            End If
        End If
        For Each rect In dragRects
            If rect.IsMouseCaptured Then
                rect.ReleaseMouseCapture()
            End If
        Next
    End Sub
    Private Sub Border_MouseDoubleClick(ByVal sender As Object, ByVal args As MouseButtonEventArgs)
        If GetIsParent(args.OriginalSource, Me) Then
            If Array.IndexOf(dragRects, borderClickItem) = 1 Then
                'ShowInfoBox("Size is " & Size.ToString & ".")
                Settings("Maximized").Value = Not Settings("Maximized").Value
                'ShowInfoBox("Size is " & Size.ToString & ".")
            End If
        End If
    End Sub
#End Region

#Region "Properties And Settings"
    Public StatusTextForeground As Color = Colors.Gray
    Private Sub InitSettings()
        Settings.AddSetting(New Setting("Bar Color", Colors.Gray))
        Settings.AddSetting(New Setting("Background", Colors.Black))
        Settings.AddSetting(New Setting("Grid", Colors.LightGray))
        Settings.AddSetting(New Setting("Cursor Line", Colors.LightGray))
        Settings.AddSetting(New Setting("Price Axis Background", Colors.Black))
        Settings.AddSetting(New Setting("Time Axis Background", Colors.Black))
        Settings.AddSetting(New Setting("Status Text Background", Colors.Black))
        Settings.AddSetting(New Setting("Price Text Background", Colors.LightGray))
        Settings.AddSetting(New Setting("Price Text Foreground", Colors.Black))
        Settings.AddSetting(New Setting("Cursor Text Background", Colors.DarkGray))
        Settings.AddSetting(New Setting("Cursor Text Foreground", Colors.White))
        Settings.AddSetting(New Setting("Axis Text Background", Colors.Transparent))
        Settings.AddSetting(New Setting("Axis Text Foreground", Colors.White))
        Settings.AddSetting(New Setting("Status Text Foreground", Colors.White))
        Settings.AddSetting(New Setting("BarType", BarTypes.RangeBars))
        Settings.AddSetting(New Setting("UseRandom", False))
        Settings.AddSetting(New Setting("RangeValue", 1.0))
        Settings.AddSetting(New Setting("XMargin", 0.1))
        Settings.AddSetting(New Setting("YMargin", 0.1))
        Settings.AddSetting(New Setting("PriceBarWidth", 100.0))
        Settings.AddSetting(New Setting("TimeBarHeight", 30.0))
        Settings.AddSetting(New Setting("StatusBarHeight", 30.0))
        Settings.AddSetting(New Setting("RestrictMovement", False))
        Settings.AddSetting(New Setting("PriceBarTextInterval", 5.0))
        Settings.AddSetting(New Setting("TimeBarTextInterval", 100.0))
        Settings.AddSetting(New Setting("IsSymbolMaster", False))
        Settings.AddSetting(New Setting("BarSize", GetType(BarSize)))
        Settings.AddSetting(New Setting("UseReplayMode", GetType(Boolean)))
        Settings.AddSetting(New Setting("ReplaySpeed", GetType(Decimal)))
        Settings.AddSetting(New Setting("DaysBack", GetType(Decimal)))
        Settings.AddSetting(New Setting("EndDaysBack", GetType(Decimal)))
        Settings.AddSetting(New Setting("IB.Contract.ComboLegsDescription", GetType(String)))
        Settings.AddSetting(New Setting("IB.Contract.ContractID", GetType(Integer)))
        Settings.AddSetting(New Setting("IB.Contract.Currency", GetType(String)))
        Settings.AddSetting(New Setting("IB.Contract.Exchange", GetType(String)))
        Settings.AddSetting(New Setting("IB.Contract.LastTradeDateOrContractMonth", GetType(String)))
        Settings.AddSetting(New Setting("IB.Contract.IncludeExpired", GetType(Boolean)))
        Settings.AddSetting(New Setting("IB.Contract.LocalSymbol", GetType(String)))
        Settings.AddSetting(New Setting("IB.Multiplier", GetType(String)))
        Settings.AddSetting(New Setting("IB.Contract.PrimaryExchange", GetType(String)))
        Settings.AddSetting(New Setting("IB.Contract.Right", GetType(RightType)))
        Settings.AddSetting(New Setting("IB.Contract.SecId", GetType(String)))
        Settings.AddSetting(New Setting("IB.Contract.SecIdType", GetType(SecurityIdType)))
        Settings.AddSetting(New Setting("IB.Contract.SecurityType", GetType(SecurityType)))
        Settings.AddSetting(New Setting("IB.Contract.Strike", GetType(Double)))
        Settings.AddSetting(New Setting("IB.Contract.Symbol", GetType(String)))

        Settings.AddSetting(New Setting("IB.SundayStartTimeTradingHours", GetType(Date)))
        Settings.AddSetting(New Setting("IB.SundayEndTimeTradingHours", GetType(Date)))
        Settings.AddSetting(New Setting("IB.WeekdayStartTimeTradingHours", GetType(Date)))
        Settings.AddSetting(New Setting("IB.WeekdayEndTimeTradingHours", GetType(Date)))

        Settings.AddSetting(New Setting("IB.UseCachedData", GetType(Boolean)))
        Settings.AddSetting(New Setting("IB.TickerID", GetType(Integer)))
        Settings.AddSetting(New Setting("IB.UseRegularTradingHours", GetType(Boolean)))
        Settings.AddSetting(New Setting("IB.MinTick", GetType(Double)))
        Settings.AddSetting(New Setting("IB.WhatToShow", GetType(HistoricalDataType)))
        Settings.AddSetting(New Setting("RAND.RandomMaxMove", GetType(Double)))
        Settings.AddSetting(New Setting("RAND.RandomMinMove", GetType(Double)))
        Settings.AddSetting(New Setting("RAND.RandomPrice", GetType(Double)))
        Settings.AddSetting(New Setting("RAND.RandomSpeed", GetType(Double)))
        Settings.AddSetting(New Setting("ChartRect", GetType(Rect)))
        Settings.AddSetting(New Setting("Bounds", GetType(Bounds)))
        Settings.AddSetting(New Setting("ShowGrid", False))
        Settings.AddSetting(New Setting("GridWidth", 65.0))
        Settings.AddSetting(New Setting("DisplayOpenTick", False))
        Settings.AddSetting(New Setting("DisplayCloseTick", True))
        Settings.AddSetting(New Setting("DesiredBarCount", 400))
        Settings.AddSetting(New Setting("Maximized", False))
        Settings.AddSetting(New Setting("AutoScale", False))
        Settings.AddSetting(New Setting("OrderBarLocation", GetType(Point)))
        Settings.AddSetting(New Setting("OrderBarVisibility", True))
        Settings.AddSetting(New Setting("GridLineCount", 7))
        Settings.AddSetting(New Setting("AnalysisTechniques", GetType(String)))
        Settings.AddSetting(New Setting("ScalingPreset1", New Bounds(0, 0, 200, 30)))
        Settings.AddSetting(New Setting("ScalingPreset2", New Bounds(0, 0, 200, 30)))
        Settings.AddSetting(New Setting("ScalingPreset3", New Bounds(0, 0, 200, 30)))
        Settings.AddSetting(New Setting("DecimalPlaces", 2))
        Settings.AddSetting(New Setting("ZIndex", GetType(Integer)))
        Settings.AddSetting(New Setting("Topmost", False))
        Settings.AddSetting(New Setting("IsSelected", GetType(Boolean)))
        Settings.AddSetting(New Setting("StatusTextLocation", HorizontalAlignment.Center))
        Settings.AddSetting(New Setting("DefaultOrderQuantity", 1))
        Settings.AddSetting(New Setting("MasterChartTickerID", 0))
        Settings.AddSetting(New Setting("CenterScalingOffsetHeight", 0.0))
        Settings.AddSetting(New Setting("ChartDefaultTimeScale2", 200.0))
        Settings.AddSetting(New Setting("ChartDefaultTimeScale1", 200.0))
        Settings.AddSetting(New Setting("ChartDefaultPriceScale2", 30.0))
        Settings.AddSetting(New Setting("ChartDefaultPriceScale1", 30.0))
        Settings.AddSetting(New Setting("UpdateScalingBarInterval", 4))
        Settings.AddSetting(New Setting("UpdateAnalysisTechniquesEveryTick", True))
        For Each item In Settings.Settings
            AddHandler item.ValueChanged, AddressOf SettingValueChanged
        Next
        'For Each color In ColorProperties
        '    Settings.AddSetting(New ChartSetting2(color.Key, color.Value))
        'Next

    End Sub
    Public Sub LoadSettings()
        Dim workspaceFile As String = Parent.FilePath
        Dim name As String = GetChartName()
        'On Error Resume Next
        For Each item In Settings.Settings
            If item.HasValue Then item.Load(workspaceFile, name, item.Value)
        Next
        Settings("TimeBarHeight").Value = 25
        RAND.UseReplayMode(TickerID) = Settings("UseReplayMode").Load(workspaceFile, name, RAND.UseReplayMode(TickerID))
        'RAND.ReplaySpeed(TickerID) = Settings("ReplaySpeed").Load(workspaceFile, name, RAND.ReplaySpeed(TickerID))
        RAND.DaysBack(TickerID) = Settings("DaysBack").Load(workspaceFile, name, RAND.DaysBack(TickerID))
        'RAND.EndDaysBack(TickerID) = Settings("EndDaysBack").Load(workspaceFile, name, RAND.EndDaysBack(TickerID))
        RAND.BarSize(TickerID) = Settings("BarSize").Load(workspaceFile, name, RAND.BarSize(TickerID))
        IB.EnableReplayMode(TickerID) = Settings("UseReplayMode").Load(workspaceFile, name, IB.EnableReplayMode(TickerID))
        'IB.ReplaySpeed(TickerID) = Settings("ReplaySpeed").Load(workspaceFile, name, IB.ReplaySpeed(TickerID))
        IB.DaysBack(TickerID) = Settings("DaysBack").Load(workspaceFile, name, IB.DaysBack(TickerID))
        'IB.EndDaysBack(TickerID) = Settings("EndDaysBack").Load(workspaceFile, name, IB.EndDaysBack(TickerID))
        IB.BarSize(TickerID) = Settings("BarSize").Load(workspaceFile, name, IB.BarSize(TickerID))
        IB.Contract(TickerID).ComboLegsDescription = Settings("IB.Contract.ComboLegsDescription").Load(workspaceFile, name, IB.Contract(TickerID).ComboLegsDescription)
        IB.Contract(TickerID).ConId = Settings("IB.Contract.ContractID").Load(workspaceFile, name, IB.Contract(TickerID).ConId)
        IB.Contract(TickerID).Currency = Settings("IB.Contract.Currency").Load(workspaceFile, name, IB.Contract(TickerID).Currency)
        IB.Contract(TickerID).Exchange = Settings("IB.Contract.Exchange").Load(workspaceFile, name, IB.Contract(TickerID).Exchange)
        IB.Contract(TickerID).LastTradeDateOrContractMonth = Settings("IB.Contract.LastTradeDateOrContractMonth").Load(workspaceFile, name, IB.Contract(TickerID).LastTradeDateOrContractMonth)
        IB.Contract(TickerID).IncludeExpired = Settings("IB.Contract.IncludeExpired").Load(workspaceFile, name, IB.Contract(TickerID).IncludeExpired)
        IB.Contract(TickerID).LocalSymbol = Settings("IB.Contract.LocalSymbol").Load(workspaceFile, name, IB.Contract(TickerID).LocalSymbol)
        IB.Contract(TickerID).Multiplier = Settings("IB.Multiplier").Load(workspaceFile, name, IB.Contract(TickerID).Multiplier)
        IB.Contract(TickerID).PrimaryExch = Settings("IB.Contract.PrimaryExchange").Load(workspaceFile, name, IB.Contract(TickerID).PrimaryExch)
        IB.Contract(TickerID).Right = ParseRightType(Settings("IB.Contract.Right").Load(workspaceFile, name, ParseRightType(IB.Contract(TickerID).Right)))
        IB.Contract(TickerID).SecType = ParseSecType(Settings("IB.Contract.SecurityType").Load(workspaceFile, name, ParseSecType(IB.Contract(TickerID).SecType)))
        IB.Contract(TickerID).SecIdType = ParseSecIDType(Settings("IB.Contract.SecIdType").Load(workspaceFile, name, ParseSecIDType(IB.Contract(TickerID).SecIdType)))
        IB.Contract(TickerID).SecId = Settings("IB.Contract.SecId").Load(workspaceFile, name, IB.Contract(TickerID).SecId)
        IB.Contract(TickerID).Strike = Settings("IB.Contract.Strike").Load(workspaceFile, name, IB.Contract(TickerID).Strike)
        IB.Contract(TickerID).Symbol = Settings("IB.Contract.Symbol").Load(workspaceFile, name, IB.Contract(TickerID).Symbol)
        IB.WhatToShow(TickerID) = Settings("IB.WhatToShow").Load(workspaceFile, name, IB.WhatToShow(TickerID))
        IB.MinTick(TickerID) = Settings("IB.MinTick").Load(workspaceFile, name, IB.MinTick(TickerID))
        IB.UseCachedData(TickerID) = Settings("IB.UseCachedData").Load(workspaceFile, name, IB.UseCachedData(TickerID))
        IB.UseRegularTradingHours(TickerID) = Settings("IB.UseRegularTradingHours").Load(workspaceFile, name, IB.UseRegularTradingHours(TickerID))
        IB.SundayStartTimeTradingHours(TickerID) = Settings("IB.SundayStartTimeTradingHours").Value
        IB.SundayEndTimeTradingHours(TickerID) = Settings("IB.SundayEndTimeTradingHours").Value
        IB.WeekdayStartTimeTradingHours(TickerID) = Settings("IB.WeekdayStartTimeTradingHours").Value
        IB.WeekdayEndTimeTradingHours(TickerID) = Settings("IB.WeekdayEndTimeTradingHours").Value

        RAND.RandomMaxMove = Settings("RAND.RandomMaxMove").Load(workspaceFile, name, RAND.RandomMaxMove)
        RAND.RandomMinMove = Settings("RAND.RandomMinMove").Load(workspaceFile, name, RAND.RandomMinMove)
        RAND.RandomPrice = Settings("RAND.RandomPrice").Load(workspaceFile, name, RAND.RandomPrice)
        RAND.RandomSpeed = Settings("RAND.RandomSpeed").Load(workspaceFile, name, RAND.RandomSpeed)
        IsSelected = Settings("IsSelected").Load(workspaceFile, name, True)
        Canvas.SetZIndex(Me, Settings("ZIndex").Load(workspaceFile, name, 1))
        Bounds = Settings("Bounds").Load(workspaceFile, name, Bounds)
        initialOrderBoxLocation = Settings("OrderBarLocation").Load(workspaceFile, name, New Point(100, 100))
        If Double.IsNaN(initialOrderBoxLocation.X) Or Double.IsNaN(initialOrderBoxLocation.Y) Then initialOrderBoxLocation = New Point(100, 100)
        Dim rect As Rect = Settings("ChartRect").Load(workspaceFile, name, New Rect(Canvas.GetLeft(Me), Canvas.GetTop(Me), Width, Height))
        If Settings("ChartRect").IsSettingSaved(workspaceFile, name) Then
            Location = rect.TopLeft
            SizeWidth = rect.Width
            SizeHeight = rect.Height
        Else
            Dim locationOffset As Double
            While True
                Dim foundPlace As Boolean = True
                For Each child In Parent.Children
                    If TypeOf child Is Chart AndAlso CType(child, Chart).Location = New Point(50 + locationOffset, 50 + locationOffset) Then
                        foundPlace = False
                        Exit For
                    End If
                Next
                If foundPlace Then Exit While
                locationOffset += 40
            End While
            Location = New Point(50 + locationOffset, 50 + locationOffset)
            SizeWidth = 800
            SizeHeight = 800
            IsSelected = True
        End If
        LoadAnalysisTechniques()

    End Sub
    Public Sub SaveSettings()
        Dim workspaceFile As String = Parent.FilePath
        Dim timestamp = Now
        Dim name As String = GetChartName()
        For Each item In Settings.Settings
            If item.HasValue Then item.Save(workspaceFile, name)
        Next

        Settings("BarSize").Save(workspaceFile, name, DataStream.BarSize(TickerID))
        Settings("UseReplayMode").Save(workspaceFile, name, DataStream.UseReplayMode(TickerID))
        'Settings("ReplaySpeed").Save(workspaceFile, name, DataStream.ReplaySpeed(TickerID))
        Settings("DaysBack").Save(workspaceFile, name, DataStream.DaysBack(TickerID))
        'Settings("EndDaysBack").Save(workspaceFile, name, DataStream.EndDaysBack(TickerID))

        Settings("IB.Contract.ComboLegsDescription").Save(workspaceFile, name, IB.Contract(TickerID).ComboLegsDescription)
        Settings("IB.Contract.ContractID").Save(workspaceFile, name, IB.Contract(TickerID).ConId)
        Settings("IB.Contract.Currency").Save(workspaceFile, name, IB.Contract(TickerID).Currency)
        Settings("IB.Contract.Exchange").Save(workspaceFile, name, IB.Contract(TickerID).Exchange)
        Settings("IB.Contract.LastTradeDateOrContractMonth").Save(workspaceFile, name, IB.Contract(TickerID).LastTradeDateOrContractMonth)
        Settings("IB.Contract.IncludeExpired").Save(workspaceFile, name, IB.Contract(TickerID).IncludeExpired)
        Settings("IB.Contract.LocalSymbol").Save(workspaceFile, name, IB.Contract(TickerID).LocalSymbol)
        Settings("IB.Multiplier").Save(workspaceFile, name, IB.Contract(TickerID).Multiplier)
        Settings("IB.Contract.PrimaryExchange").Save(workspaceFile, name, IB.Contract(TickerID).PrimaryExch)
        Settings("IB.Contract.Right").Save(workspaceFile, name, ParseRightType(IB.Contract(TickerID).Right))
        Settings("IB.Contract.SecId").Save(workspaceFile, name, IB.Contract(TickerID).SecId)
        Settings("IB.Contract.SecIdType").Save(workspaceFile, name, ParseSecIDType(IB.Contract(TickerID).SecIdType))
        Settings("IB.Contract.SecurityType").Save(workspaceFile, name, ParseSecType(IB.Contract(TickerID).SecType))
        Settings("IB.Contract.Strike").Save(workspaceFile, name, IB.Contract(TickerID).Strike)
        Settings("IB.Contract.Symbol").Save(workspaceFile, name, IB.Contract(TickerID).Symbol)
        Settings("IB.SundayStartTimeTradingHours").Save(workspaceFile, name, IB.SundayStartTimeTradingHours(TickerID))
        Settings("IB.SundayEndTimeTradingHours").Save(workspaceFile, name, IB.SundayEndTimeTradingHours(TickerID))
        Settings("IB.WeekdayStartTimeTradingHours").Save(workspaceFile, name, IB.WeekdayStartTimeTradingHours(TickerID))
        Settings("IB.WeekdayEndTimeTradingHours").Save(workspaceFile, name, IB.WeekdayEndTimeTradingHours(TickerID))
        Settings("IB.UseRegularTradingHours").Save(workspaceFile, name, IB.UseRegularTradingHours(TickerID))
        Settings("IB.MinTick").Save(workspaceFile, name, IB.MinTick(TickerID))
        Settings("IB.WhatToShow").Save(workspaceFile, name, IB.WhatToShow(TickerID))
        Settings("IB.UseCachedData").Save(workspaceFile, name, IB.UseCachedData(TickerID))

        Settings("RAND.RandomMaxMove").Save(workspaceFile, name, RAND.RandomMaxMove)

        Settings("RAND.RandomMinMove").Save(workspaceFile, name, RAND.RandomMinMove)
        Settings("RAND.RandomPrice").Save(workspaceFile, name, RAND.RandomPrice)
        Settings("RAND.RandomSpeed").Save(workspaceFile, name, RAND.RandomSpeed)
        Settings("ChartRect").Save(workspaceFile, name, New Rect(Location, New Size(SizeWidth, SizeHeight)))
        Settings("Bounds").Save(workspaceFile, name, Bounds)
        Settings("ZIndex").Save(workspaceFile, name, Canvas.GetZIndex(Me))
        Settings("IsSelected").Save(workspaceFile, name, IsSelected, True)
        If Not Double.IsNaN(orderBox.Left) And Not Double.IsNaN(orderBox.Top) Then Settings("OrderBarLocation").Save(workspaceFile, name, New Point(orderBox.Left, orderBox.Top), True)


        'Settings("IsSelected").Save(workspaceFile, name, IsSelected, True)
        'MsgBox((Now - timestamp).TotalMilliseconds)
        'Settings("OrderBarVisibility").Save(workspaceFile, name, mnuViewOrderBar.IsChecked)
        'Settings("OrderBarLocation").Save(workspaceFile, name, New Point(orderBar.RealLeft, orderBar.RealTop))
        SaveAnalysisTechniques()
        SaveDrawingObjects()
        'File.WriteAllText(Path.Combine(My.Computer.FileSystem.SpecialDirectories.Desktop, "ESRangeData.txt"), "")
        'Dim writer As New StreamWriter(Path.Combine(My.Computer.FileSystem.SpecialDirectories.Desktop, "ESRangeData.txt"), True)
        'Dim converter As New BarDataConverter
        'For Each item In bars
        '    writer.WriteLine(converter.ConvertToString(item.Data))
        'Next
        'writer.Close()
    End Sub
    Public Sub ResetSettings()
        Dim workspaceFile As String = Parent.FilePath
        Dim name As String = GetChartName()
        For Each setting In Settings.Settings
            setting.Reset(workspaceFile, name)
        Next
    End Sub
    Public Sub RefreshPresetStyles()
        'DefaultLabelStyle.Font.Effect = ReadSetting(WORKSPACE_FOLDER_WITH_SLASH & workspaceName, name & ".DefaultLabelStyle.Font.Effect", Nothing)
        'If ReadSetting(WORKSPACE_FOLDER_WITH_SLASH & workspaceName, name & ".DefaultLabelStyle.Font.Transform", "-") <> "-" Then DefaultLabelStyle.Font.Transform = (New TransformConverter).ConvertFromString(ReadSetting(WORKSPACE_FOLDER_WITH_SLASH & workspaceName, name & ".DefaultLabelStyle.Font.Transform", (New TransformConverter).ConvertToString(New ScaleTransform())))
        PresetLineStyles.Clear()
        PresetLabelStyles.Clear()
        PresetArrowStyles.Clear()
        PresetRectangleStyles.Clear()

        For i = 1 To 4

            If True Then
                Dim trendLineStyle As New TrendLineStyle
                Dim dashes As DoubleCollection = (New DoubleCollectionConverter).ConvertFromString(ReadSetting(GLOBAL_CONFIG_FILE, "PresetLineStyle" & i & ".Pen.DashStyle.Dashes", "1"))
                trendLineStyle.Pen.Brush = (New BrushConverter).ConvertFromString(ReadSetting(GLOBAL_CONFIG_FILE, "PresetLineStyle" & i & ".Pen.Brush", "Red"))
                trendLineStyle.Pen.DashCap = [Enum].Parse(GetType(PenLineCap), ReadSetting(GLOBAL_CONFIG_FILE, "PresetLineStyle" & i & ".Pen.DashCap", "Square"))
                trendLineStyle.Pen.DashStyle = New DashStyle(dashes, ReadSetting(GLOBAL_CONFIG_FILE, "PresetLineStyle" & i & ".Pen.DashStyle.Offset", "0"))
                trendLineStyle.Pen.EndLineCap = [Enum].Parse(GetType(PenLineCap), ReadSetting(GLOBAL_CONFIG_FILE, "PresetLineStyle" & i & ".Pen.EndLineCap", "Flat"))
                trendLineStyle.Pen.LineJoin = [Enum].Parse(GetType(PenLineJoin), ReadSetting(GLOBAL_CONFIG_FILE, "PresetLineStyle" & i & ".Pen.LineJoin", "Miter"))
                trendLineStyle.Pen.MiterLimit = ReadSetting(GLOBAL_CONFIG_FILE, Name & ".PresetLineStyle" & i & ".Pen.MiterLimit", "0")
                trendLineStyle.Pen.StartLineCap = [Enum].Parse(GetType(PenLineCap), ReadSetting(GLOBAL_CONFIG_FILE, "PresetLineStyle" & i & ".Pen.StartLineCap", "Flat"))
                trendLineStyle.Pen.Thickness = ReadSetting(GLOBAL_CONFIG_FILE, "PresetLineStyle" & i & ".Pen.Thickness", "1")
                trendLineStyle.OuterPen = trendLineStyle.Pen.Clone
                trendLineStyle.OuterPen.Thickness = ReadSetting(GLOBAL_CONFIG_FILE, "PresetLineStyle" & i & ".OuterPen.Thickness", "1")
                trendLineStyle.OuterPen.Brush = (New BrushConverter).ConvertFromString(ReadSetting(GLOBAL_CONFIG_FILE, "PresetLineStyle" & i & ".OuterPen.Brush", "Red"))
                trendLineStyle.IsRegressionLine = Boolean.Parse(ReadSetting(GLOBAL_CONFIG_FILE, "PresetLineStyle" & i & ".IsRegressionLine", "False"))
                trendLineStyle.ExtendRight = Boolean.Parse(ReadSetting(GLOBAL_CONFIG_FILE, "PresetLineStyle" & i & ".ExtendRight", "False"))
                trendLineStyle.ExtendLeft = Boolean.Parse(ReadSetting(GLOBAL_CONFIG_FILE, "PresetLineStyle" & i & ".ExtendLeft", "False"))
                trendLineStyle.HasParallel = Boolean.Parse(ReadSetting(GLOBAL_CONFIG_FILE, "PresetLineStyle" & i & ".HasParallel", "False"))
                trendLineStyle.LockToEnd = Boolean.Parse(ReadSetting(GLOBAL_CONFIG_FILE, "PresetLineStyle" & i & ".LockToEnd", "False"))
                PresetLineStyles.Add(trendLineStyle)
            End If

            Dim labelStyle As New LabelStyle
            labelStyle.Font.Brush = (New BrushConverter).ConvertFromString(ReadSetting(GLOBAL_CONFIG_FILE, "PresetLabelStyle" & i & ".Font.Brush", "White"))
            labelStyle.Font.FontFamily = (New FontFamilyConverter).ConvertFromString(ReadSetting(GLOBAL_CONFIG_FILE, "PresetLabelStyle" & i & ".Font.FontFamily", "Arial"))
            labelStyle.Font.FontSize = ReadSetting(GLOBAL_CONFIG_FILE, "PresetLabelStyle" & i & ".Font.FontSize", "12")
            labelStyle.Font.FontStyle = (New FontStyleConverter).ConvertFromString(ReadSetting(GLOBAL_CONFIG_FILE, "PresetLabelStyle" & i & ".Font.FontStyle", "Normal"))
            labelStyle.Font.FontWeight = (New FontWeightConverter).ConvertFromString(ReadSetting(GLOBAL_CONFIG_FILE, "PresetLabelStyle" & i & ".Font.FontWeight", "Normal"))
            labelStyle.Font.Transform = RotateTransformFromString(ReadSetting(GLOBAL_CONFIG_FILE, "PresetLabelStyle" & i & ".Font.Transform", RotateTransformToString(New RotateTransform(0))))
            labelStyle.Font.Effect = EffectFromString(ReadSetting(GLOBAL_CONFIG_FILE, "PresetLabelStyle" & i & ".Font.Effect", EffectToString(New Effects.BlurEffect)))
            labelStyle.Text = ReadSetting(GLOBAL_CONFIG_FILE, "PresetLabelStyle" & i & ".Text", "Text")
            PresetLabelStyles.Add(labelStyle)

            Dim arrowStyle As New ArrowStyle
            arrowStyle.Pen = New Pen((New BrushConverter).ConvertFromString(ReadSetting(GLOBAL_CONFIG_FILE, "PresetArrowStyle" & i & ".Pen.Brush", "Red")), ReadSetting(GLOBAL_CONFIG_FILE, "PresetArrowStyle" & i & ".Pen.Thickness", "1"))
            arrowStyle.IsFlipped = ReadSetting(GLOBAL_CONFIG_FILE, "PresetArrowStyle" & i & ".IsFlipped", "False")
            arrowStyle.Width = ReadSetting(GLOBAL_CONFIG_FILE, "PresetArrowStyle" & i & ".Width", "12")
            arrowStyle.Height = ReadSetting(GLOBAL_CONFIG_FILE, "PresetArrowStyle" & i & ".Height", "4")
            PresetArrowStyles.Add(arrowStyle)

            If True Then
                Dim rectStyle As New RectangleStyle
                Dim dashes As DoubleCollection = (New DoubleCollectionConverter).ConvertFromString(ReadSetting(GLOBAL_CONFIG_FILE, "PresetRectangleStyle" & i & ".Pen.DashStyle.Dashes", "1"))
                rectStyle.Pen.Brush = (New BrushConverter).ConvertFromString(ReadSetting(GLOBAL_CONFIG_FILE, "PresetRectangleStyle" & i & ".Pen.Brush", "Red"))
                rectStyle.Pen.DashCap = [Enum].Parse(GetType(PenLineCap), ReadSetting(GLOBAL_CONFIG_FILE, "PresetRectangleStyle" & i & ".Pen.DashCap", "Square"))
                rectStyle.Pen.DashStyle = New DashStyle(dashes, ReadSetting(GLOBAL_CONFIG_FILE, "PresetRectangleStyle" & i & ".Pen.DashStyle.Offset", "0"))
                rectStyle.Pen.EndLineCap = [Enum].Parse(GetType(PenLineCap), ReadSetting(GLOBAL_CONFIG_FILE, "PresetRectangleStyle" & i & ".Pen.EndLineCap", "Flat"))
                rectStyle.Pen.LineJoin = [Enum].Parse(GetType(PenLineJoin), ReadSetting(GLOBAL_CONFIG_FILE, "PresetRectangleStyle" & i & ".Pen.LineJoin", "Miter"))
                rectStyle.Pen.MiterLimit = ReadSetting(GLOBAL_CONFIG_FILE, Name & ".PresetRectangleStyle" & i & ".Pen.MiterLimit", "0")
                rectStyle.Pen.StartLineCap = [Enum].Parse(GetType(PenLineCap), ReadSetting(GLOBAL_CONFIG_FILE, "PresetLineStyle" & i & ".Pen.StartLineCap", "Flat"))
                rectStyle.Pen.Thickness = ReadSetting(GLOBAL_CONFIG_FILE, "PresetRectangleStyle" & i & ".Pen.Thickness", "1")
                rectStyle.Fill = (New BrushConverter).ConvertFromString(ReadSetting(GLOBAL_CONFIG_FILE, "PresetRectangleStyle" & i & ".Fill", "Transparent"))
                PresetRectangleStyles.Add(rectStyle)
            End If
        Next

    End Sub

    Private shouldUpdateSilently As Boolean = False
    Public Sub ChangeRange(newRangeValue As Decimal)

        'Dim t = Now
        Settings("RangeValue").Value = newRangeValue
        shouldUpdateSilently = True
        RequestData(TimeSpan.FromDays(DataStream.DaysBack(TickerID)), (Now - DataStream.CurrentReplayTime), False)
        shouldUpdateSilently = False
        'Dim margin As Decimal = ((Settings("RangeValue").Value / GetRelativeFromRealHeight(Parent.WorkspaceDefaultRealBarHeight)) * Bounds.Height - Bounds.Height) / 2
        'Bounds = New Bounds(Bounds.X1, Bounds.Y1 - margin, Bounds.X2, Bounds.Y2 + margin)

        'Log("total: " & (Now - t).TotalMilliseconds)
        SetScaling(Bounds.Size)
    End Sub
    Private Sub SettingValueChanged(ByVal sender As Object, ByVal e As PropertyChangedEventArgs)
        Dim setting As Setting = CType(sender, Setting)
        Select Case setting.Name
            Case "ShowGrid"
                RefreshAxisAndGrid()
            Case "DisplayOpenTick", "DisplayCloseTick"
                TickVisibilityChanged()
            Case "Background"
                Background = New SolidColorBrush(setting.Value)
                If orderBox.Content IsNot Nothing Then
                    Dim color As Color = CType(setting.Value, Color)
                    If color.A = 255 Then
                        'orderBox.Content.Child.Background = New SolidColorBrush(setting.Value)
                    Else
                        'orderBox.Content.Child.Background = New SolidColorBrush(Color.FromArgb(255, color.R, color.G, color.B))
                    End If
                End If
                For Each technique In AnalysisTechniques
                    If GetType(IChartPadAnalysisTechnique).IsAssignableFrom(technique.AnalysisTechnique.GetType) Then
                        If CType(technique.AnalysisTechnique, IChartPadAnalysisTechnique).ChartPadVisible Then
                            CType(technique.AnalysisTechnique, IChartPadAnalysisTechnique).UpdateChartPadColor(CType(setting.Value, Color))
                        End If
                    End If
                Next
            Case "Price Axis Background", "Time Axis Background", "Status Text Background"
                priceAxis.Content.Background = New SolidColorBrush(Settings("Price Axis Background").Value)
                timeAxis.Content.Background = New SolidColorBrush(Settings("Time Axis Background").Value)
                statusAxis.Background = New SolidColorBrush(Settings("Status Text Background").Value)
            Case "Maximized"
                If Settings("Maximized").Value Then
                    Canvas.SetLeft(Me, 0)
                    Canvas.SetTop(Me, 0)
                    Width = CType(Parent, Canvas).ActualWidth
                    Height = CType(Parent, Canvas).ActualHeight
                Else
                    Canvas.SetLeft(Me, Location.X)
                    Canvas.SetTop(Me, Location.Y)
                    Width = SizeWidth
                    Height = SizeHeight
                End If
            Case "RangeValue"
                UpdateChartPad()
        End Select
    End Sub
    Private Sub SetStatusTextLocation()
        statusText.HorizontalAlignment = Settings("StatusTextLocation").Value
        statusText.VerticalAlignment = Windows.VerticalAlignment.Center
        If Settings("StatusTextLocation").Value = Windows.HorizontalAlignment.Right Then
            statusText.Margin = New Thickness(2, -1, 3 + Settings("PriceBarWidth").Value, 3)
        Else
            statusText.Margin = New Thickness(3, -1, 3, 3)
        End If
    End Sub
    Public Sub ChangeSetting(ByVal settingName As String)
        Select Case settingName
            Case "ChartBorderWidth"
                ColumnDefinitions(0).Width = New GridLength(My.Application.ChartBorderWidth)
                ColumnDefinitions(2).Width = New GridLength(My.Application.ChartBorderWidth)
                RowDefinitions(0).Height = New GridLength(My.Application.ChartBorderWidth)
                RowDefinitions(2).Height = New GridLength(My.Application.ChartBorderWidth)
                borderRects(0).CornerRadius = New CornerRadius(Max(My.Application.ChartBorderWidth, 3), 0, 0, Max(My.Application.ChartBorderWidth, 3))
                borderRects(2).CornerRadius = New CornerRadius(0, Max(My.Application.ChartBorderWidth, 3), Max(My.Application.ChartBorderWidth, 3), 0)
            Case "BarWidth"
                For Each item In bars
                    item.Pen.Thickness = My.Application.BarWidth
                    item.RefreshVisual()
                Next
            Case "TickWidth"
                For Each item In bars
                    item.TickWidth = My.Application.TickWidth
                    item.RefreshVisual()
                Next
            Case "ShowMousePopups"
                'For Each item In bars
                '    item.IsSelectable = My.Application.ShowMousePopups
                'Next
        End Select
    End Sub


    Private Sub LoadDrawingObjects()
        Dim name As String = GetChartName()
        Dim indexes As New Dictionary(Of String, Integer)
        indexes.Add("TrendLine", ReadSetting(Parent.FilePath, name & ".TrendLine", 0))
        indexes.Add("Label", ReadSetting(Parent.FilePath, name & ".Label", 0))
        indexes.Add("Arrow", ReadSetting(Parent.FilePath, name & ".Arrow", 0))
        indexes.Add("Rectangle", ReadSetting(Parent.FilePath, name & ".Rectangle", 0))
        For Each item In indexes
            For i = 1 To item.Value
                Dim obj As Object = Nothing
                If item.Key = "TrendLine" Then obj = New TrendLine
                If item.Key = "Label" Then obj = New Label
                If item.Key = "Arrow" Then obj = New Arrow
                If item.Key = "Rectangle" Then obj = New Rectangle
                Dim str As String = ReadSetting(Parent.FilePath, name & "." & item.Key & i, "")
                If str <> "" Then
                    Children.Add(obj)
                    CType(obj, ISerializable).Deserialize(str)
                End If
            Next
        Next
    End Sub
    Private Sub SaveDrawingObjects()
        Dim name As String = GetChartName()
        Dim loadindexes As New Dictionary(Of String, Integer)
        loadindexes.Add("TrendLine", ReadSetting(Parent.FilePath, name & ".TrendLine", 0))
        loadindexes.Add("Label", ReadSetting(Parent.FilePath, name & ".Label", 0))
        loadindexes.Add("Arrow", ReadSetting(Parent.FilePath, name & ".Arrow", 0))
        loadindexes.Add("Rectangle", ReadSetting(Parent.FilePath, name & ".Rectangle", 0))
        For Each item In loadindexes
            For i = 1 To item.Value
                WriteSetting(Parent.FilePath, name & "." & item.Key & i, "")
            Next
        Next
        Dim indexes As New Dictionary(Of String, Integer)
        For Each child In Children
            If TypeOf child Is ChartDrawingVisual AndAlso TypeOf child Is ISerializable AndAlso Not TypeOf child Is Bar AndAlso child.AnalysisTechnique Is Nothing Then
                Dim vis As ChartDrawingVisual = child
                Dim typeName As String = vis.GetType.Name
                If Not indexes.ContainsKey(typeName) Then indexes.Add(typeName, 1) Else indexes(typeName) += 1
                WriteSetting(Parent.FilePath, name & "." & typeName & indexes(typeName).ToString, CType(vis, ISerializable).Serialize, True)

            End If
        Next
        Dim typeNames As New List(Of String)
        For Each item In indexes
            typeNames.Add(item.Key)
            WriteSetting(Parent.FilePath, name & "." & item.Key, item.Value.ToString, True)
        Next
        'WriteSetting(Parent.FilePath, name & ".UserObjectTypes", Join(typeNames.ToArray, ","))
    End Sub

    Public Function GetDateFromBarNumber(barNumber As Integer) As Date
        Dim startDate As Date = Date.MinValue
        If barNumber > 0 And barNumber < bars.Count Then startDate = bars(barNumber - 1).Data.Date
        Return startDate
    End Function
    Public Function GetBarNumberFromDate([date] As Date) As Integer
        Dim closestBarDate As Date = Date.MinValue
        Dim closestBarNum As Integer = 0
        For j = 0 To bars.Count - 1
            If bars(j).Data.Date - [date] < bars(j).Data.Date - closestBarDate Then
                closestBarDate = bars(j).Data.Date
                closestBarNum = bars(j).Data.Number
            End If
        Next
        Return closestBarNum
    End Function

    Public Property PresetLineStyles As New List(Of TrendLineStyle)
    Public Property PresetLabelStyles As New List(Of LabelStyle)
    Public Property PresetRectangleStyles As New List(Of RectangleStyle)
    Public Property PresetArrowStyles As New List(Of ArrowStyle)
    Public Property Settings As New SettingList
    Public Property TickerID As Integer

#Region "ReadOnly and complex properties"

    Public ReadOnly Property RelativeRightMarginLocation As Double
        Get
            Return GetRelativeFromRealX(mainCanvas.ActualWidth - Settings("XMargin").Value * mainCanvas.ActualWidth - Settings("PriceBarWidth").Value)
        End Get
    End Property
    Public ReadOnly Property RelativeTopMarginLocation As Double
        Get
            Return GetRelativeFromRealY(Settings("YMargin").Value * mainCanvas.ActualHeight)
        End Get
    End Property
    Public ReadOnly Property RelativeBottomMarginLocation As Double
        Get
            Return GetRelativeFromRealY(mainCanvas.ActualHeight - Settings("YMargin").Value * mainCanvas.ActualHeight)
        End Get
    End Property

    Public Overloads Property Background As Brush
        Get
            Return mainCanvas.Background
        End Get
        Set(ByVal value As Brush)
            mainCanvas.Background = value
            'timeAxis.Content.Background = value
            'priceAxis.Content.Background = value
            'statusAxis.Content.Background = value
        End Set
    End Property
    Public Property ChartBorderBackground As Brush
        Get
            Return MyBase.Background
        End Get
        Set(ByVal value As Brush)
            MyBase.Background = value
        End Set
    End Property


    Public ReadOnly Property IsDesignTime As Boolean
        Get
            Return _designMode
        End Get
    End Property

    Private _mouseDown As MouseDownType = MouseDownType.MouseUp
    Public Shadows ReadOnly Property MouseDown As MouseDownType
        Get
            Return _mouseDown
        End Get
    End Property


    Private _mode As ClickMode = ClickMode.Normal
    Public Property Mode As ClickMode
        Get
            Return _mode
        End Get
        Set(ByVal value As ClickMode)
            'If Mouse.LeftButton = MouseButtonState.Released Or value = ClickMode.Normal Then
            If value = _mode Then value = ClickMode.Normal
            _mode = value
            Select Case value
                Case ClickMode.Pan
                    ChangeCursor(Cursors.ScrollAll)
                Case ClickMode.Normal
                    ResetCursor()
            End Select
            If IsModeInCreateObjectMode() Then ChangeCursor(Cursors.Pen)
        End Set
    End Property

    Private WithEvents _parent As Workspace
    <DebuggerStepThrough()>
    Protected Overrides Sub OnVisualParentChanged(ByVal oldParent As System.Windows.DependencyObject)
        If TypeOf VisualParent Is Workspace Or VisualParent Is Nothing Then
            MyBase.OnVisualParentChanged(oldParent)
            If VisualParent IsNot Nothing Then _parent = VisualParent
        Else
            Throw New InvalidOperationException("The Chart control can only be owned by the workspace control.")
        End If
    End Sub
    Public ReadOnly Property HasParent As Boolean
        Get
            Return Parent IsNot Nothing
        End Get
    End Property
    Public Overloads ReadOnly Property Parent As Workspace
        Get
            Return _parent
        End Get
    End Property

#End Region

#End Region

#Region "Mouse Interaction"
    Private Function GetClosestObject(ByVal e As MouseButtonEventArgs) As IChartObject
        Dim point As Point = e.GetPosition(mainCanvas)
        Dim visual As IChartObject = Nothing
        Dim result As HitTestResult = VisualTreeHelper.HitTest(mainCanvas, e.GetPosition(mainCanvas))
        If result IsNot Nothing AndAlso TypeOf result.VisualHit Is IChartObject AndAlso TypeOf result.VisualHit Is ISelectable AndAlso CType(result.VisualHit, ISelectable).IsSelectable Then
            'Return result.VisualHit
        End If
        If result IsNot Nothing Then
            Dim prnt As UIChartControl = GetFirstParentOfType(result.VisualHit, GetType(UIChartControl))
            If prnt IsNot Nothing Then Return prnt
        End If
        Dim returnChild As IChartObject = Nothing
        For Each child As IChartObject In Children
            If TypeOf child Is ChartDrawingVisual AndAlso TypeOf child Is ISelectable AndAlso CType(child, ISelectable).IsSelectable Then
                If Mode = ClickMode.Normal AndAlso CType(child, ChartDrawingVisual).ContainsPoint(point) Then
                    returnChild = child
                Else
                    If TypeOf child Is ISelectable AndAlso CType(child, ISelectable).IsSelectable AndAlso
                     (IsModeInCreateObjectMode() OrElse (visual IsNot child AndAlso Not TypeOf visual Is UIChartControl)) Then
                        If CType(child, ISelectable).IsSelected Then CType(child, ISelectable).IsSelected = False
                    End If
                End If
            ElseIf TypeOf child Is UIChartControl AndAlso CType(child, UIChartControl).Content Is e.Source Then
                returnChild = child
            Else
                If TypeOf child Is ISelectable AndAlso CType(child, ISelectable).IsSelectable AndAlso
                 (IsModeInCreateObjectMode() OrElse (visual IsNot child AndAlso Not TypeOf visual Is UIChartControl)) Then
                    If CType(child, ISelectable).IsSelected Then CType(child, ISelectable).IsSelected = False
                End If
            End If
        Next
        Return returnChild
    End Function
    Dim clickPoint As Point, clickBounds As Bounds, childClick As Boolean, cursorLines(1) As TrendLine, doubleClick As Boolean
    Friend Sub Window_MouseButtonDown(ByVal sender As Object, ByVal e As System.Windows.Input.MouseButtonEventArgs)
        If Children.Contains(cursorLines(0)) Then Children.Remove(cursorLines(0))
        If Children.Contains(cursorLines(1)) Then Children.Remove(cursorLines(1))
        If priceAxis.ContentChildren.Contains(priceCursorText) Then priceAxis.ContentChildren.Remove(priceCursorText)
        If timeAxis.ContentChildren.Contains(timeCursorText) Then timeAxis.ContentChildren.Remove(timeCursorText)
        _mouseDown = MouseDownType.MouseDown
        If GetIsParent(e.Source, Me) Then ' New Rect(mainCanvas.RenderSize).Contains(e.GetPosition(mainCanvas)) Then
            Dim visual As IChartObject = GetClosestObject(e)
            For Each child In Children
                If TypeOf child Is ISelectable AndAlso CType(child, ISelectable).IsSelectable AndAlso
                 (IsModeInCreateObjectMode() OrElse (visual IsNot child AndAlso Not TypeOf visual Is UIChartControl)) Then
                    If CType(child, ISelectable).IsSelected Then CType(child, ISelectable).IsSelected = False
                End If
            Next

            If IsMouseOver Then
                If visual IsNot Nothing Then
                    If Not IsModeInCreateObjectMode() Then
                        If e.LeftButton = MouseButtonState.Pressed Then
                            visual.ParentMouseLeftButtonDown(e, CType(visual, Visual).PointFromScreen(mainCanvas.PointToScreen(e.GetPosition(mainCanvas))))
                        ElseIf e.RightButton = MouseButtonState.Pressed Then
                            visual.ParentMouseRightButtonDown(e, CType(visual, Visual).PointFromScreen(mainCanvas.PointToScreen(e.GetPosition(mainCanvas))))
                        ElseIf e.MiddleButton = MouseButtonState.Pressed Then
                            visual.ParentMouseMiddleButtonDown(e, CType(visual, Visual).PointFromScreen(mainCanvas.PointToScreen(e.GetPosition(mainCanvas))))
                        End If
                    End If
                    _mouseDown = MouseDownType.CapturedOnChild
                ElseIf New Rect(mainCanvas.RenderSize).Contains(e.GetPosition(mainCanvas)) Then
                    _mouseDown = MouseDownType.CapturedOnMe
                End If
            Else
                _mouseDown = MouseDownType.MouseDown
            End If
            clickPoint = e.GetPosition(mainCanvas)
            clickBounds = Bounds
            If MouseDown = MouseDownType.CapturedOnMe And e.LeftButton = MouseButtonState.Pressed Then
                RaiseEvent ChartMouseDownEvent(Me, GetRelativeFromReal(e.GetPosition(mainCanvas)))
                If Mode = ClickMode.Normal Then
                    For i = 0 To 1
                        cursorLines(i) = New TrendLine
                        cursorLines(i).SnapToRelativePixels = True
                        cursorLines(i).StartPoint = New Point(0, 0)
                        cursorLines(i).EndPoint = New Point(0, 0)
                        cursorLines(i).Pen = New Pen(New SolidColorBrush(Settings("Cursor Line").Value), 1)
                        cursorLines(i).IsEditable = False
                        cursorLines(i).IsSelectable = True
                        Children.Add(cursorLines(i))
                    Next
                    Dim relativeClickPoint As Point = GetRelativeFromReal(e.GetPosition(mainCanvas))
                    If MouseDown = MouseDownType.CapturedOnMe Then
                        cursorLines(0).Coordinates = New LineCoordinates(relativeClickPoint.X, Bounds.Y1, relativeClickPoint.X, Bounds.Y2)
                        cursorLines(0).RefreshVisual()
                        cursorLines(1).Coordinates = New LineCoordinates(Bounds.X1, RoundTo(relativeClickPoint.Y, GetMinTick), Bounds.X2, RoundTo(relativeClickPoint.Y, GetMinTick))
                        cursorLines(1).RefreshVisual()
                        DrawCursorTexts()
                    End If
                End If
            End If
            HandleModes(e)
        End If
        'If MouseDown = MouseDownType.CapturedOnMe Or TypeOf visual Is ChartDrawingVisual Then
        'CaptureMouse()
        'End If
    End Sub
    Friend Sub Window_MouseButtonUp(ByVal sender As Object, ByVal e As System.Windows.Input.MouseButtonEventArgs)
        'If doubleClick Then
        '    doubleClick = False
        '    Exit Sub
        'End If 
        Try

            ReleaseMouseCapture()
            timeAxis.ReleaseMouseCapture()
            priceAxis.ReleaseMouseCapture()
            If e.ChangedButton = MouseButton.Left Then
                If Children.Contains(cursorLines(0)) Then Children.Remove(cursorLines(0))
                If Children.Contains(cursorLines(1)) Then Children.Remove(cursorLines(1))
                If priceAxis.ContentChildren.Contains(priceCursorText) Then priceAxis.ContentChildren.Remove(priceCursorText)
                If timeAxis.ContentChildren.Contains(timeCursorText) Then timeAxis.ContentChildren.Remove(timeCursorText)
                RaiseEvent ChartMouseUpEvent(Me, GetRelativeFromReal(e.GetPosition(mainCanvas)))
                DispatcherHelper.DoEvents()
            End If
            If (Mode = ClickMode.PriceAxisDrag Or Mode = ClickMode.TimeAxisDrag) Then
                If originalMode = ClickMode.Undefined Then
                    Mode = ClickMode.Normal
                Else
                    Mode = originalMode
                End If
                originalMode = ClickMode.Undefined
            End If
            If IsModeInCreateObjectMode() Then Mode = ClickMode.Normal
            If e.ChangedButton = MouseButton.Left Then Border_MouseLeftButtonUp(Me, e)
            _mouseDown = MouseDownType.MouseUp
            Dim screenPoint As Point = mainCanvas.PointToScreen(e.GetPosition(mainCanvas))
            Dim i As Integer
            While i < Children.Count
                Dim childPoint As Point = CType(Children(i), Visual).PointFromScreen(screenPoint)
                If e.ChangedButton = MouseButton.Left Then
                    If Children(i).IsMouseDown Then Children(i).ParentMouseLeftButtonUp(e, childPoint)
                Else
                    If Children(i).IsRightMouseButtonDown Then Children(i).ParentMouseRightButtonUp(e, childPoint)
                End If
                i += 1
            End While
        Catch ex As Exception
            Log("caught chart mouse-up error")
        End Try
    End Sub
    Friend Sub Window_MouseMove(ByVal sender As Object, ByVal e As System.Windows.Input.MouseEventArgs)
        Dim d = Now
        If GetIsParent(e.Source, Me) Then
            If e.LeftButton = MouseButtonState.Pressed And borderPress Then Border_MouseMove(e)
            'Log("a: " & (Now - d).TotalMilliseconds)
            'd = Now
            Dim screenPoint As Point = mainCanvas.PointToScreen(e.GetPosition(mainCanvas))
            If e.LeftButton = MouseButtonState.Pressed Then
                If (Mode <> ClickMode.Pan And MouseDown = MouseDownType.CapturedOnChild) Or IsModeInCreateObjectMode() Then
                    For Each child As IChartObject In Children
                        If Not TypeOf child Is Bar Then
                            Dim childPoint As Point = CType(child, Visual).PointFromScreen(screenPoint)
                            child.ParentMouseMove(e, childPoint)
                        End If
                    Next
                End If
                'Log("b: " & (Now - d).TotalMilliseconds)
                'd = Now
            Else
                If IsMouseOver Then
                    For Each order In Orders
                        Dim orderPoint As Point = CType(order, Visual).PointFromScreen(screenPoint)
                        order.ParentMouseMove(e, orderPoint)
                    Next
                    'Log("b2: " & (Now - d).TotalMilliseconds)
                    'd = Now
                End If
            End If
            If e.LeftButton = MouseButtonState.Pressed Then HandleMouseDrag(e)
            'Log("c: " & (Now - d).TotalMilliseconds)
            'd = Now
            If Mode = ClickMode.Normal And (MouseDown = MouseDownType.CapturedOnMe Or MouseDown = MouseDownType.CapturedOnChild) Then
                Dim relativeClickPoint As Point = GetRelativeFromReal(e.GetPosition(mainCanvas))
                If MouseDown = MouseDownType.CapturedOnMe And e.LeftButton = MouseButtonState.Pressed Then
                    RaiseEvent ChartMouseDraggedEvent(Me, relativeClickPoint)
                    cursorLines(0).Coordinates = New LineCoordinates(relativeClickPoint.X, Bounds.Y1, relativeClickPoint.X, Bounds.Y2)
                    cursorLines(0).RefreshVisual()
                    cursorLines(1).Coordinates = New LineCoordinates(Bounds.X1, RoundTo(relativeClickPoint.Y, GetMinTick), Bounds.X2, RoundTo(relativeClickPoint.Y, GetMinTick))
                    cursorLines(1).RefreshVisual()
                    DrawCursorTexts()
                End If
                'Log("d: " & (Now - d).TotalMilliseconds)
                'd = Now
            End If
        End If
    End Sub
    Friend Sub Window_MouseDoubleClick(ByVal sender As Object, ByVal e As System.Windows.Input.MouseButtonEventArgs)
        Border_MouseDoubleClick(Me, e)
        Dim visual As IChartObject = GetClosestObject(e)
        If visual IsNot Nothing Then
            visual.ParentMouseDoubleClick(e, CType(visual, Visual).PointFromScreen(mainCanvas.PointToScreen(e.GetPosition(mainCanvas))))
        End If
        If GetIsParent(e.OriginalSource, Me) Then
            doubleClick = True
            RaiseEvent ChartMouseDoubleClickEvent(Me, GetRelativeFromReal(e.GetPosition(mainCanvas)))
        End If
    End Sub
    Friend Sub Window_MouseWheel(ByVal sender As Object, ByVal e As System.Windows.Input.MouseWheelEventArgs)
        For Each child In Children
            child.ParentMouseWheel(e)
        Next
        If GetIsParent(e.OriginalSource, Me) Then
            MasterWindow.totalMouseWheel += 1
            Dim selected As Boolean = False
            For Each item In Orders
                If item.IsSelected Then selected = True
            Next
            If Not selected Then
                If Keyboard.IsKeyDown(Key.LeftCtrl) Then
                    Dim zoomFactor As Double = 0.075 'the percentage of the screen to zoom in
                    Dim xInc As Double
                    Dim yInc As Double
                    Dim newBounds As Bounds = Bounds
                    Dim mousePos As Point = e.GetPosition(Me)
                    xInc = -e.Delta / Abs(e.Delta) * Bounds.Width * zoomFactor
                    yInc = -e.Delta / Abs(e.Delta) * Bounds.Height * zoomFactor
                    newBounds = New Bounds(Bounds.X1 - LinCalc(0, 0, GetRealFromRelativeWidth(Bounds.Width), 1, mousePos.X) * xInc,
                                               Bounds.Y1 - LinCalc(0, 0, GetRealFromRelativeHeight(Bounds.Height), 1, mousePos.Y) * yInc,
                                               New Size(Bounds.Width + xInc, Bounds.Height + yInc))
                    Bounds = newBounds
                Else
                    If e.Delta > 0 Then
                        ChartCommands.NextWorkspace.Execute(Nothing, Me)
                    Else
                        ChartCommands.PreviousWorkspace.Execute(Nothing, Me)
                    End If
                End If
            End If
        End If
    End Sub
    Private Sub HandleModes(ByVal args As MouseButtonEventArgs)
        Select Case Mode
            Case ClickMode.CreateLine
                RefreshPresetStyles()
                Dim pos = GetRelativeFromReal(args.GetPosition(mainCanvas))
                Dim ln As New TrendLine
                Children.Add(ln)
                ln.Coordinates = New LineCoordinates(pos, pos)
                ln.IsEditable = True
                ln.UseNegativeCoordinates = True
                If PresetLineStyles(newObjectPresetNum) IsNot Nothing Then
                    ln.Pen = PresetLineStyles(newObjectPresetNum).Pen.Clone
                    ln.ExtendLeft = PresetLineStyles(newObjectPresetNum).ExtendLeft
                    ln.ExtendRight = PresetLineStyles(newObjectPresetNum).ExtendRight
                    ln.HasParallel = PresetLineStyles(newObjectPresetNum).HasParallel
                    ln.LockToEnd = PresetLineStyles(newObjectPresetNum).LockToEnd
                    ln.OuterPen = PresetLineStyles(newObjectPresetNum).OuterPen.Clone
                    ln.IsRegressionLine = PresetLineStyles(newObjectPresetNum).IsRegressionLine
                Else
                    Dim style As New TrendLineStyle
                    ln.Pen = style.Pen.Clone
                    ln.OuterPen = style.OuterPen.Clone
                    ln.ExtendLeft = style.ExtendLeft
                    ln.ExtendRight = style.ExtendRight
                    ln.HasParallel = False
                    ln.IsRegressionLine = False
                End If
                ln.AutoRefresh = True
                ln.ParentMouseLeftButtonDown(args, args.GetPosition(mainCanvas))
            Case ClickMode.CreateLabel
                RefreshPresetStyles()
                Dim lbl As New Label
                Children.Add(lbl)
                lbl.IsEditable = True
                lbl.UseNegativeCoordinates = True
                If PresetLabelStyles(newObjectPresetNum) IsNot Nothing Then
                    lbl.Font = PresetLabelStyles(newObjectPresetNum).Font.Clone
                    lbl.Text = PresetLabelStyles(newObjectPresetNum).Text
                Else
                    Dim style As New LabelStyle
                    lbl.Font = style.Font.Clone
                    lbl.Text = style.Text
                End If
                lbl.Location = GetRelativeFromReal(args.GetPosition(mainCanvas))
                lbl.IsSelectable = True
                lbl.IsSelected = True
                lbl.AutoRefresh = True
                lbl.RefreshVisual()
            Case ClickMode.CreateArrow
                RefreshPresetStyles()
                Dim arrow As New Arrow
                Children.Add(arrow)
                arrow.IsEditable = True
                arrow.UseNegativeCoordinates = True
                If PresetArrowStyles(newObjectPresetNum) IsNot Nothing Then
                    arrow.Width = PresetArrowStyles(newObjectPresetNum).Width
                    arrow.Height = PresetArrowStyles(newObjectPresetNum).Height
                    arrow.IsFlipped = PresetArrowStyles(newObjectPresetNum).IsFlipped
                    arrow.Pen = PresetArrowStyles(newObjectPresetNum).Pen.Clone
                Else
                    Dim style As New ArrowStyle
                    arrow.Width = style.Width
                    arrow.Height = style.Height
                    arrow.IsFlipped = style.IsFlipped
                    arrow.Pen = style.Pen.Clone
                End If
                arrow.Location = GetRelativeFromReal(args.GetPosition(mainCanvas))
                arrow.IsSelected = True
                arrow.IsSelectable = True
                arrow.AutoRefresh = True
                arrow.RefreshVisual()
            Case ClickMode.CreateRectangle
                RefreshPresetStyles()
                Dim pos = GetRelativeFromReal(args.GetPosition(mainCanvas))
                Dim rect As New Rectangle
                Children.Add(rect)
                rect.Location = pos
                rect.IsEditable = True
                rect.UseNegativeCoordinates = True
                If PresetLineStyles(newObjectPresetNum) IsNot Nothing Then
                    rect.Pen = PresetRectangleStyles(newObjectPresetNum).Pen.Clone
                    rect.Fill = PresetRectangleStyles(newObjectPresetNum).Fill
                Else
                    Dim style As New RectangleStyle
                    rect.Pen = style.Pen.Clone
                    rect.Fill = style.Fill
                End If
                rect.AutoRefresh = True
                rect.ParentMouseLeftButtonDown(args, args.GetPosition(mainCanvas))
            Case ClickMode.CreateBuyOrder, ClickMode.CreateSellOrder, ClickMode.CreateBuyMarketOrder, ClickMode.CreateSellMarketOrder

        End Select
    End Sub
    Private Sub HandleMouseDrag(ByVal args As MouseEventArgs)
        If Mode = ClickMode.Pan Then
            If args.LeftButton = MouseButtonState.Pressed And (MouseDown = MouseDownType.CapturedOnMe Or MouseDown = MouseDownType.CapturedOnChild) Then
                Dim vec As Point = GetRelativeFromReal(CType(args.GetPosition(mainCanvas) - clickPoint, Point)) - GetRelativeFromReal(New Point(0, 0))
                Dim topLeft As Point = CType(Bounds.TopLeft - vec, Point)
                Dim bottomRight As Point = CType(Bounds.BottomRight - vec, Point)
                TrySetBounds(topLeft, bottomRight)
                clickPoint = args.GetPosition(mainCanvas)
            End If
        End If
        If Mode = ClickMode.PriceAxisDrag Then
            Dim c As Double = (clickBounds.Y2 - clickBounds.Y1) / 2
            Dim changeAmount As Double = 1.002 ^ (args.GetPosition(mainCanvas).Y - clickPoint.Y) * c - c
            TrySetBounds(New Bounds(Bounds.X1, clickBounds.Y1 - changeAmount, Bounds.X2, clickBounds.Y2 + changeAmount))
        End If
        If Mode = ClickMode.TimeAxisDrag Then
            Dim clickBoundsMiddle As Double = (clickBounds.X2 - clickBounds.X1) / 2
            Dim changeAmount As Double = 1.002 ^ (args.GetPosition(mainCanvas).X - clickPoint.X) * clickBoundsMiddle - clickBoundsMiddle
            Dim realWidth As Double = Settings("XMargin").Value * mainCanvas.ActualWidth + Settings("PriceBarWidth").Value
            Dim newWidth = GetRealWithRelativeWidthInRelative(realWidth, clickBounds.Width + changeAmount - timeAxisClickMarginRelativeWidth)
            TrySetBounds(New Bounds(clickBounds.X1 - changeAmount, Bounds.Y1, clickBounds.X1 - changeAmount + newWidth, Bounds.Y2))
            'Dim multiplier As Double = LinCalc(0, 1, ActualWidth, 2, args.GetPosition(mainCanvas).X - clickPoint.X)
            'Dim newWidth As Double = multiplier * clickBounds.Width
            'Dim endPoint As Double = BarIndex + GetRelativeFromRealWidth(Settings("XMargin").Value * mainCanvas.ActualWidth + Settings("PriceBarWidth").Value)
            'TrySetBounds(New Bounds(endPoint - newWidth, Bounds.Y1, endPoint, Bounds.Y2))
        End If
    End Sub
    Private Function IsModeInCreateObjectMode() As Boolean
        Return Mode = ClickMode.CreateLine OrElse Mode = ClickMode.CreateLabel OrElse Mode = ClickMode.CreateArrow OrElse Mode = ClickMode.CreateBuyOrder OrElse Mode = ClickMode.CreateSellOrder OrElse Mode = ClickMode.CreateBuyMarketOrder OrElse Mode = ClickMode.CreateSellMarketOrder OrElse Mode = ClickMode.CreateRectangle
    End Function
    Public Event ChartMouseDraggedEvent(sender As Object, location As Point)
    Public Event ChartMouseDownEvent(sender As Object, location As Point)
    Public Event ChartMouseDoubleClickEvent(sender As Object, location As Point)
    Public Event ChartMouseUpEvent(sender As Object, location As Point)
#End Region

#Region "Keyboard Interaction"
    Public Event ChartKeyDown As KeyEventHandler
    Public Event ChartKeyUp As KeyEventHandler
    Friend Sub Window_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Input.KeyEventArgs)
        RefreshOrderBox()
        If IsSelected Then
            RaiseEvent ChartKeyDown(sender, e)
            'If e.Key = Key.A And Keyboard.Modifiers = ModifierKeys.Control Then
            '    Dim order As New OrderControl(Me) With {.Color = Colors.DarkGreen, .Quantity = 1, .SelectedColor = Colors.Green, .ButtonHoverColor = Colors.LightGreen, .Price = Price, .OrderType = OrderType.Market, .OrderAction = ActionSide.Buy}
            '    Children.Add(order)
            '    order.IsSelected = True
            '    order.Transmit()
            'ElseIf e.Key = Key.Q And Keyboard.Modifiers = ModifierKeys.Control Then
            '    Dim order As New OrderControl(Me) With {.Color = Colors.DarkRed, .Quantity = 1, .SelectedColor = Colors.Red, .ButtonHoverColor = Colors.Pink, .Price = Price, .OrderType = OrderType.Market, .OrderAction = ActionSide.Sell}
            '    Children.Add(order)
            '    order.IsSelected = True
            '    order.Transmit()
            'End If

        End If

    End Sub
    Friend Sub Window_KeyUp(ByVal sender As Object, ByVal e As System.Windows.Input.KeyEventArgs)
        RefreshOrderBox()
        If IsSelected Then RaiseEvent ChartKeyUp(sender, e)

    End Sub
    Sub Me_KeyUp(ByVal sender As Object, ByVal e As System.Windows.Input.KeyEventArgs)

    End Sub
#End Region

#Region "Misc"
    Public Sub SetStatusTextToDefault(ByVal forceRefresh As Boolean)
        Dim symbol As String = ""

        symbol = If(IB.Contract(TickerID).LocalSymbol <> "", IB.Contract(TickerID).LocalSymbol, IB.Contract(TickerID).Symbol)
        Dim spacing As String = "     "
        If Settings("UseRandom").Value Then symbol = "(RANDOM)"
        statusText.Inlines.Clear()
        Dim run As New Run(symbol & " ")
        run.FontSize = 20
        'run.FontWeight = FontWeights.Bold
        statusText.Inlines.Add(run)
        If Not Settings("UseRandom").Value And IB.Contract(TickerID).LastTradeDateOrContractMonth <> "" And ParseSecType(IB.Contract(TickerID).SecType) = SecurityType.FUT Then
            statusText.Inlines.Add(spacing & GetContractMonthFormattedString(IB.Contract(TickerID).LastTradeDateOrContractMonth))
        End If
        Dim barText As String = ""
        Dim timeString As String = ""
        Select Case DataStream.BarSize(TickerID)
            Case BarSize.FifteenMinutes
                timeString = "15 min"
            Case BarSize.FifteenSeconds
                timeString = "15 sec"
            Case BarSize.FiveMinutes
                timeString = "5 min"
            Case BarSize.FiveSeconds
                timeString = "5 sec"
            Case BarSize.OneDay
                timeString = "1 day"
            Case BarSize.OneHour
                timeString = "1 hr"
            Case BarSize.OneMinute
                timeString = "1 min"
            Case BarSize.OneMonth
                timeString = "1 month"
            Case BarSize.OneSecond
                timeString = "1 sec"
            Case BarSize.OneWeek
                timeString = "1 wk"
            Case BarSize.OneYear
                timeString = "1 yr"
            Case BarSize.ThirtyMinutes
                timeString = "30 min"
            Case BarSize.ThirtySeconds
                timeString = "30 sec"
            Case BarSize.TwoMinutes
                timeString = "2 min"
        End Select
        If Settings("BarType").Value = BarTypes.RangeBars Then
            barText = String.Format("RB {0}{1}{2}", CStr(Settings("RangeValue").Value).TrimStart("0"c), spacing, timeString)
        ElseIf Settings("BarType").Value = BarTypes.TimeBars Then
            barText = String.Format("{0}", timeString)
        ElseIf Settings("BarType").Value = BarTypes.VerticalSwingBars Then
            'barText = String.Format("Swing Bar: {0} RV", Settings("RangeValue").Value)
        End If

        statusText.Inlines.Add(spacing & barText & If(IB.EnableReplayMode(TickerID), " (REPLAY)", ""))
    End Sub
    Public Sub SetStatusText(ByVal text As String, Optional ByVal forceRefresh As Boolean = True)
        Dispatcher.Invoke(
         Sub()
             statusText.Text = text
             If forceRefresh And Not shouldUpdateSilently Then DispatcherHelper.DoEvents()
         End Sub)
    End Sub

    Public Function GetMinTick() As Double
        If Settings("UseRandom").Value Then
            Return RAND.RandomMinMove
        Else
            Return IB.MinTick(TickerID)
        End If
    End Function


    Public Sub ChangeCursor(ByVal cursor As Cursor)
        DesktopWindow.Cursor = cursor
        mainCanvas.Cursor = cursor
        Me.Cursor = cursor
        For Each chart In DesktopWindow.Charts
            For Each rect In chart.dragRects
                rect.Cursor = cursor
            Next
        Next
    End Sub
    Public Sub ResetCursor()
        mainCanvas.Cursor = Cursors.Arrow
        DesktopWindow.Cursor = Cursors.Arrow
        Me.Cursor = Cursors.Arrow
        For Each chart In DesktopWindow.Charts
            chart.dragRects(0).Cursor = Cursors.SizeNWSE
            chart.dragRects(1).Cursor = Cursors.SizeAll
            chart.dragRects(2).Cursor = Cursors.SizeNESW
            chart.dragRects(3).Cursor = Cursors.SizeWE
            chart.dragRects(4).Cursor = Cursors.SizeWE
            chart.dragRects(5).Cursor = Cursors.SizeNESW
            chart.dragRects(6).Cursor = Cursors.SizeNS
            chart.dragRects(7).Cursor = Cursors.SizeNWSE
        Next
    End Sub

    Public Function GetChartName() As String
        If ChartOverrideTickerID = -1 Then
            Return TickerID
        Else
            Return ChartOverrideTickerID
        End If
    End Function
#End Region
#Region "Events"
    Public Event ChartFormatted(sender As Object, e As EventArgs)
    Public Event ChartLoaded(sender As Object, e As EventArgs)

#End Region
#Region "Scaling Calculation"
    ''' <summary>
    ''' Gets a non-scaled x component from a scaled x component.
    ''' </summary>
    ''' <param name="x">The scaled x component to be converted.</param>
    Public Function GetRealFromRelativeX(ByVal x As Double) As Double
        Return ((x - Bounds.X1) / (Bounds.X2 - Bounds.X1)) * ActualWidth
    End Function
    ''' <summary>
    ''' Gets a scaled x component from a non-scaled x component.
    ''' </summary>
    ''' <param name="x">The non-scaled x component to be converted.</param>
    Public Function GetRelativeFromRealX(ByVal x As Double) As Double
        Return ((Bounds.X2 - Bounds.X1) * x) / ActualWidth + Bounds.X1
    End Function
    ''' <summary>
    ''' Gets a non-scaled y component from a scaled y component.
    ''' </summary>
    ''' <param name="y">The scaled y component to be converted.</param>
    Public Function GetRealFromRelativeY(ByVal y As Double) As Double
        Return ((y - Bounds.Y1) / (Bounds.Y2 - Bounds.Y1)) * ActualHeight
    End Function
    ''' <summary>
    ''' Gets a scaled y component from a non-scaled y component.
    ''' </summary>
    ''' <param name="y">The non-scaled y component to be converted.</param>
    Public Function GetRelativeFromRealY(ByVal y As Double) As Double
        Return ((Bounds.Y2 - Bounds.Y1) * y) / ActualHeight + Bounds.Y1
    End Function
    ''' <summary>
    ''' Gets a non-scaled point from a scaled point.
    ''' </summary>
    ''' <param name="point">The scaled point to be converted.</param>
    Public Function GetRealFromRelative(ByVal point As Point) As Point
        Return New Point(GetRealFromRelativeX(point.X), GetRealFromRelativeY(point.Y))
    End Function
    ''' <summary>
    ''' Gets a scaled point from a non-scaled point.
    ''' </summary>
    ''' <param name="point">The non-scaled point to be converted.</param>
    Public Function GetRelativeFromReal(ByVal point As Point) As Point
        Return New Point(GetRelativeFromRealX(point.X), GetRelativeFromRealY(point.Y))
    End Function
    ''' <summary>
    ''' Gets a non-scaled rectangle from a scaled rectangle.
    ''' </summary>
    ''' <param name="rect">The scaled rectangle to be converted.</param>
    Public Function GetRealFromRelative(ByVal rect As Bounds) As Bounds
        Return New Bounds(GetRealFromRelative(rect.TopLeft), GetRealFromRelative(rect.BottomRight))
    End Function
    Public Function GetRealFromRelative(ByVal rect As Rect) As Rect
        Return New Rect(GetRealFromRelative(rect.TopLeft), GetRealFromRelative(rect.BottomRight))
    End Function
    ''' <summary>
    ''' Gets a scaled rectangle from a non-scaled rectangle.
    ''' </summary>
    ''' <param name="rect">The non-scaled rectangle to be converted.</param>
    Public Function GetRelativeFromReal(ByVal rect As Bounds) As Bounds
        Return New Bounds(GetRelativeFromReal(rect.TopLeft), GetRelativeFromReal(rect.BottomRight))
    End Function
    Public Function GetRelativeFromReal(ByVal rect As Rect) As Rect
        Return New Rect(GetRelativeFromReal(rect.TopLeft), GetRelativeFromReal(rect.BottomRight))
    End Function
    ''' <summary>
    ''' Gets a non-scaled size from a scaled size.
    ''' </summary>
    ''' <param name="size">The scaled size to be converted.</param>
    Public Function GetRealFromRelative(ByVal size As Size) As Size
        Return GetRealFromRelative(New Bounds(0, 0, size)).Size
    End Function
    ''' <summary>
    ''' Gets a scaled size from a non-scaled size.
    ''' </summary>
    ''' <param name="size">The non-scaled size to be converted.</param>
    Public Function GetRelativeFromReal(ByVal size As Size) As Size
        Return GetRelativeFromReal(New Bounds(0, 0, size)).Size
    End Function
    ''' <summary>
    ''' Gets a non-scaled line from a scaled line.
    ''' </summary>
    ''' <param name="line">The scaled line to be converted.</param>
    Public Function GetRealFromRelative(ByVal line As LineCoordinates) As LineCoordinates
        Return New LineCoordinates(GetRealFromRelative(line.StartPoint), GetRealFromRelative(line.EndPoint))
    End Function
    ''' <summary>
    ''' Gets a scaled line from a non-scaled line.
    ''' </summary>
    ''' <param name="line">The non-scaled line to be converted.</param>
    Public Function GetRelativeFromReal(ByVal line As LineCoordinates) As LineCoordinates
        Return New LineCoordinates(GetRelativeFromReal(line.StartPoint), GetRelativeFromReal(line.EndPoint))
    End Function
    ''' <summary>
    ''' Gets a scaled width from a non-scaled width.
    ''' </summary>
    ''' <param name="width">The non-scaled width to be converted.</param>
    Public Function GetRelativeFromRealWidth(ByVal width As Double) As Double
        Return GetRelativeFromRealX(width) - GetRelativeFromRealX(0)
    End Function
    ''' <summary>
    ''' Gets a non-scaled width from a scaled width.
    ''' </summary>
    ''' <param name="width">The scaled width to be converted.</param>
    Public Function GetRealFromRelativeWidth(ByVal width As Double) As Double
        Return GetRealFromRelativeX(width) - GetRealFromRelativeX(0)
    End Function
    ''' <summary>
    ''' Gets a scaled height from a non-scaled height.
    ''' </summary>
    ''' <param name="height">The scaled non-height to be converted.</param>
    Public Function GetRelativeFromRealHeight(ByVal height As Double) As Double
        Return GetRelativeFromRealY(height) - GetRelativeFromRealY(0)
    End Function
    ''' <summary>
    ''' Gets a non-scaled height from a scaled height.
    ''' </summary>
    ''' <param name="height">The scaled height to be converted.</param>
    Public Function GetRealFromRelativeHeight(ByVal height As Double) As Double
        Return GetRealFromRelativeY(height) - GetRealFromRelativeY(0)
    End Function

    Public Function GetRealWithRelativeWidthInRelative(ByVal realWidth As Double, ByVal relativeWidth As Double) As Double
        Return (relativeWidth * mainCanvas.ActualWidth) / (mainCanvas.ActualWidth - realWidth)
    End Function
    Public Function GetRealWithRelativeHeightInRelative(ByVal realHeight As Double, ByVal relativeHeight As Double) As Double
        Return (relativeHeight * mainCanvas.ActualHeight) / (mainCanvas.ActualHeight - realHeight)
    End Function
#End Region

#Region "Scaling"

    Private _bounds As Bounds = New Bounds(0, 0, 150, 20)
    Public Property Bounds As Bounds
        <DebuggerStepThrough()>
        Get
            Return _bounds
        End Get
        Set(ByVal value As Bounds)
            'Dim t = Now
            Dim args As New RoutedPropertyChangedEventArgs(Of Bounds)(_bounds, value)
            _bounds = value
            RaiseEvent BoundsChanged(Me, args)
            If IsLoaded And Not shouldUpdateSilently Then
                For Each child As IChartObject In Children
                    child.ParentBoundsChanged()
                Next
            End If
            'Log((Now - t).TotalMilliseconds)
        End Set
    End Property

    Friend Sub SetScaling(ByVal scale As Size)
        If bars.Count > 0 Then
            Dim newBounds As New Bounds
            Dim highPoint = New Point(0, Double.MinValue), startPoint As Point = New Point(bars(0).Data.Number, -bars(0).Data.High), lowPoint As Point = New Point(0, Double.MaxValue)
            For i = Max(Bounds.X1, 0) To Min(bars.Count - 1, Bounds.X2)
                Dim bar As Bar = bars(i)
                If highPoint.Y < bar.Data.High Then
                    highPoint = New Point(bar.Data.Number, bar.Data.High)
                End If
                If lowPoint.Y > bar.Data.Low Then
                    lowPoint = New Point(bar.Data.Number, bar.Data.Low)
                End If
            Next
            highPoint = NegateY(highPoint)
            lowPoint = NegateY(lowPoint)
            Dim rightEdge As Integer = BarIndex + GetRelativeFromRealBounds(New Point(Settings("XMargin").Value * mainCanvas.ActualWidth + Settings("PriceBarWidth").Value, 0),
                New Bounds(0, 0, scale.Width, 0), New Size(mainCanvas.ActualWidth, mainCanvas.ActualHeight)).X
            Dim margin As Double = GetRelativeFromRealBounds(New Point(0, Settings("YMargin").Value * mainCanvas.ActualHeight),
                New Bounds(0, 0, 0, scale.Height), New Size(mainCanvas.ActualWidth, mainCanvas.ActualHeight)).Y
            If GetRealWithRelativeHeightInRelative(Settings("YMargin").Value * mainCanvas.ActualHeight * 2, Abs(lowPoint.Y - highPoint.Y)) > scale.Height Then
                If lowPoint.X > highPoint.X Then
                    newBounds.Y2 = lowPoint.Y + margin
                    newBounds.Y1 = newBounds.Y2 - scale.Height
                Else
                    newBounds.Y1 = highPoint.Y - margin
                    newBounds.Y2 = newBounds.Y1 + scale.Height
                End If
            Else
                Dim n = (scale.Height - Abs(lowPoint.Y - highPoint.Y)) / 2
                newBounds.Y1 = highPoint.Y - n
                newBounds.Y2 = lowPoint.Y + n
            End If
            newBounds.X1 = rightEdge - scale.Width
            newBounds.X2 = rightEdge
            'Bounds = newBounds
            TrySetBounds(newBounds)
            ScaleRoutine()
            'SetScaling(scale)
        End If
    End Sub

    Private Sub Chart_BoundsChanged(ByVal sender As Object, ByVal e As System.Windows.RoutedPropertyChangedEventArgs(Of Bounds)) Handles Me.BoundsChanged
        If IsLoaded Then
            RefreshAxisAndGrid()
            'RefreshAllSize()
        End If
    End Sub

    Public Event BoundsChanged(ByVal sender As Object, ByVal e As RoutedPropertyChangedEventArgs(Of Bounds))

    Private Sub TrySetBounds(ByVal topLeft As Point, ByVal bottomRight As Point, Optional ByVal snap As Boolean = False, Optional ByVal autoScale As Boolean = True)
        If bars.Count > 0 Then
            If (Settings("RestrictMovement").Value Or snap) Then
                If BarIndex < bottomRight.X Then
                    If Settings("YMargin").Value * (topLeft.Y - bottomRight.Y) + -topLeft.Y < CurrentBar.Close Then
                        Dim top As Double = topLeft.Y
                        topLeft.Y = -CurrentBar.Close + Settings("YMargin").Value * (topLeft.Y - bottomRight.Y)
                        bottomRight.Y = topLeft.Y - top + bottomRight.Y
                    End If
                    If -bottomRight.Y - Settings("YMargin").Value * (topLeft.Y - bottomRight.Y) > CurrentBar.Close Then
                        Dim bottom As Double = bottomRight.Y
                        bottomRight.Y = -CurrentBar.Close - Settings("YMargin").Value * (topLeft.Y - bottomRight.Y)
                        topLeft.Y = topLeft.Y - bottom + bottomRight.Y
                    End If
                End If
                'If BarIndex <= Ceiling(Bounds.X1) + 2 Then 'Round(Bounds.X1 - topLeft.X) = Round(Bounds.X2 - bottomRight.X) And Round(Bounds.X2 - bottomRight.X) < 0 And Ceiling(BarIndex - (Bounds.X2 - XMargin * Bounds.Width - (GetRelativeFromReal(New Point(PriceBarWidth, 0)).X - GetRelativeFromReal(New Point(0, 0)).X))) < 0 Then
                '    bottomRight.X = Bounds.X2
                '    topLeft.X = Bounds.X1
                'End If
                If topLeft.X < 0 Then
                    Dim dif As Double = bottomRight.X - topLeft.X
                    topLeft.X = 0
                    bottomRight.X = dif
                End If
                If topLeft.X >= BarIndex - 1 Then
                    Dim dif As Double = bottomRight.X - topLeft.X
                    topLeft.X = BarIndex - 1
                    bottomRight.X = topLeft.X + dif
                End If
            End If
            If Settings("AutoScale").Value And autoScale And BarIndex > Bounds.X1 Then
                Dim highValue As Double = Double.MinValue
                Dim lowValue As Double = Double.MaxValue
                For i = Max(Min(Bounds.X1, BarIndex), 1) To Min(BarIndex - 1, Bounds.X2)
                    highValue = Max(highValue, bars(i).Data.High)
                    lowValue = Min(lowValue, bars(i).Data.Low)
                Next
                Dim bnds As Bounds = New Bounds(Bounds.X1, -highValue, Bounds.X2, -lowValue)
                Dim sze As Size = New Size(mainCanvas.ActualWidth, mainCanvas.ActualHeight)
                Dim zero As Point = GetRelativeFromRealBounds(New Point(0, 0), bnds, sze)
                Dim point As Point = GetRelativeFromRealBounds(New Point(0, Settings("YMargin").Value * mainCanvas.ActualHeight), bnds, sze)
                If highValue <> Double.MinValue Then
                    highValue += point.Y - zero.Y
                    lowValue -= point.Y - zero.Y
                    topLeft.Y = -highValue
                    bottomRight.Y = -lowValue
                End If
            End If
        End If
        Bounds = New Bounds(topLeft, bottomRight)
    End Sub
    Private Sub TrySetBounds(ByVal bounds As Bounds, Optional ByVal snap As Boolean = False, Optional ByVal autoScale As Boolean = True)
        TrySetBounds(bounds.TopLeft, bounds.BottomRight, snap, autoScale)
    End Sub
#End Region

#Region "Commands"
    Private newObjectPresetNum As Integer
    Friend Sub Command_Executed(ByVal sender As Object, ByVal e As ExecutedRoutedEventArgs)
        Dim index As Integer = 0
        While index < Children.Count
            If TypeOf Children(index) Is IChartObject AndAlso TypeOf Children(index) Is ISelectable AndAlso CType(Children(index), ISelectable).IsSelectable AndAlso CType(Children(index), ISelectable).IsSelected Then
                CType(Children(index), IChartObject).Command_Executed(Children(index), e)
            End If
            index += 1
        End While
        Dim command As RoutedUICommand = e.Command
        If command Is ChartCommands.Format AndAlso Not (IsNumeric(e.Parameter) AndAlso e.Parameter <> 0) Then
            FormatChart()
            'ElseIf e.Command Is ChartCommands.MaximizeChart Then
            '    Settings("Maximized").Value = Not Settings("Maximized").Value
        ElseIf command Is ChartCommands.SetDragModeNormal Then
            Mode = ClickMode.Normal
        ElseIf command Is ChartCommands.SetDragModePan Then
            Mode = ClickMode.Pan
        ElseIf command.Name.Contains("CreateLineWithPreset") Then
            Mode = ClickMode.CreateLine
            newObjectPresetNum = command.Name.Substring(command.Name.Length - 1) - 1
        ElseIf command.Name.Contains("CreateLabelWithPreset") Then
            Mode = ClickMode.CreateLabel
            newObjectPresetNum = command.Name.Substring(command.Name.Length - 1) - 1
        ElseIf command.Name.Contains("CreateArrowWithPreset") Then
            Mode = ClickMode.CreateArrow
            newObjectPresetNum = command.Name.Substring(command.Name.Length - 1) - 1
        ElseIf command.Name.Contains("CreateRectangleWithPreset") Then
            Mode = ClickMode.CreateRectangle
            newObjectPresetNum = command.Name.Substring(command.Name.Length - 1) - 1
        ElseIf command Is ChartCommands.RemoveAllDrawingObjects Then
            For Each chart In Parent.Charts
                Dim i As Integer = 0
                While i < chart.Children.Count
                    If chart.Children(i).AnalysisTechnique Is Nothing And
                     Not TypeOf chart.Children(i) Is Bar And
                     Not TypeOf chart.Children(i) Is OrderControl And
                     Not TypeOf chart.Children(i) Is UIChartControl And
                     (
                     (TypeOf chart.Children(i) Is ISelectable AndAlso CType(chart.Children(i), ISelectable).IsSelectable = True) OrElse
                      Not TypeOf chart.Children(i) Is ISelectable
                      ) Then
                        chart.Children.RemoveAt(i)
                    Else
                        i += 1
                    End If
                End While
            Next
        ElseIf command Is ChartCommands.PanDown Then
            'Settings(
            Settings("CenterScalingOffsetHeight").Value -= 0.1
            'Dim margin As Decimal = ((Settings("RangeValue").Value / GetRelativeFromRealHeight(Height)) * Bounds.Height - Bounds.Height) / 2
            'SetScaling(New Size(ReadSetting(GLOBAL_CONFIG_FILE, "DefaultTimeScale", 200), Bounds.Height))
            'Bounds = New Bounds(Bounds.X1, Bounds.Y1 - margin, Bounds.X2, Bounds.Y2 + margin)
            'TrySetBounds(New Bounds(Bounds.X1, Bounds.Y1 + Bounds.Height / 66, Bounds.X2, Bounds.Y2 + Bounds.Height / 66))
            Dim height As Integer = Parent.WorkspaceDefaultRealBarHeight
            For Each Chart In Parent.Charts
                Dim margin As Decimal = ((Chart.Settings("RangeValue").Value / Chart.GetRelativeFromRealHeight(height)) * Chart.Bounds.Height - Chart.Bounds.Height) / 2
                Chart.SetScaling(New Size(ReadSetting(GLOBAL_CONFIG_FILE, "DefaultTimeScale", 200), Chart.Bounds.Height))
                Chart.Bounds = New Bounds(Chart.Bounds.X1, Chart.Bounds.Y1 - margin, Chart.Bounds.X2, Chart.Bounds.Y2 + margin)
            Next
        ElseIf command Is ChartCommands.PanUp Then
            Settings("CenterScalingOffsetHeight").Value += 0.1
            Dim height As Integer = Parent.WorkspaceDefaultRealBarHeight
            For Each Chart In Parent.Charts
                Dim margin As Decimal = ((Chart.Settings("RangeValue").Value / Chart.GetRelativeFromRealHeight(height)) * Chart.Bounds.Height - Chart.Bounds.Height) / 2
                Chart.SetScaling(New Size(ReadSetting(GLOBAL_CONFIG_FILE, "DefaultTimeScale", 200), Chart.Bounds.Height))
                Chart.Bounds = New Bounds(Chart.Bounds.X1, Chart.Bounds.Y1 - margin, Chart.Bounds.X2, Chart.Bounds.Y2 + margin)
            Next
            'TrySetBounds(New Bounds(Bounds.X1, Bounds.Y1 - Bounds.Height / 66, Bounds.X2, Bounds.Y2 - Bounds.Height / 66))

        ElseIf command Is ChartCommands.PanLeft Then
            TrySetBounds(New Bounds(Bounds.X1 - Bounds.Width / 35, Bounds.Y1, Bounds.X2 - Bounds.Width / 35, Bounds.Y2))
        ElseIf command Is ChartCommands.PanRight Then
            TrySetBounds(New Bounds(Bounds.X1 + Bounds.Width / 35, Bounds.Y1, Bounds.X2 + Bounds.Width / 35, Bounds.Y2))

        ElseIf command Is ChartCommands.PanEnd Then
            SetScaling(Bounds.Size)
        ElseIf command Is ChartCommands.PanBegin Then
            TrySetBounds(New Bounds(0, Bounds.Y1, Bounds.X2 - Bounds.X1, Bounds.Y2))

        ElseIf command Is ChartCommands.ResizeHorizontalBiggerfiNe Then
            Dim percentage As Double = (GetRelativeFromRealWidth(Settings("PriceBarWidth").Value) + GetRelativeFromRealWidth(Settings("XMargin").Value * mainCanvas.ActualWidth)) / Bounds.Width
            Dim sizeChange As Double = GetRelativeFromRealWidth(My.Application.FineChartResizeValue)
            If Bounds.X2 - Bounds.X1 > 5 Then TrySetBounds(New Bounds(Bounds.X1 - LinCalc(0.5, sizeChange / 2, 0, sizeChange, percentage), Bounds.Y1, Bounds.X2 + LinCalc(0.5, sizeChange / 2, 0, 0, percentage), Bounds.Y2), , True)
            SetScaling(Bounds.Size)
        ElseIf command Is ChartCommands.ResizeHorizontalSmallerFine Then
            Dim percentage As Double = (GetRelativeFromRealWidth(Settings("PriceBarWidth").Value) + GetRelativeFromRealWidth(Settings("XMargin").Value * mainCanvas.ActualWidth)) / Bounds.Width
            Dim sizeChange As Double = GetRelativeFromRealWidth(My.Application.FineChartResizeValue)
            If Bounds.X2 - Bounds.X1 > 5 Then TrySetBounds(New Bounds(Bounds.X1 + LinCalc(0.5, sizeChange / 2, 0, sizeChange, percentage), Bounds.Y1, Bounds.X2 - LinCalc(0.5, sizeChange / 2, 0, 0, percentage), Bounds.Y2), , True)
            SetScaling(Bounds.Size)
        ElseIf command Is ChartCommands.ResizeHorizontalBiggercoarse Then
            Dim percentage As Double = (GetRelativeFromRealWidth(Settings("PriceBarWidth").Value) + GetRelativeFromRealWidth(Settings("XMargin").Value * mainCanvas.ActualWidth)) / Bounds.Width
            Dim sizeChange As Double = GetRelativeFromRealWidth(My.Application.CoarseChartResizeValue)
            If Bounds.X2 - Bounds.X1 > 5 Then TrySetBounds(New Bounds(Bounds.X1 - LinCalc(0.5, sizeChange / 2, 0, sizeChange, percentage), Bounds.Y1, Bounds.X2 + LinCalc(0.5, sizeChange / 2, 0, 0, percentage), Bounds.Y2), , True)
            SetScaling(Bounds.Size)
        ElseIf command Is ChartCommands.ResizeHorizontalSmallercoarse Then
            Dim percentage As Double = (GetRelativeFromRealWidth(Settings("PriceBarWidth").Value) + GetRelativeFromRealWidth(Settings("XMargin").Value * mainCanvas.ActualWidth)) / Bounds.Width
            Dim sizeChange As Double = GetRelativeFromRealWidth(My.Application.CoarseChartResizeValue)
            If Bounds.X2 - Bounds.X1 > 5 Then TrySetBounds(New Bounds(Bounds.X1 + LinCalc(0.5, sizeChange / 2, 0, sizeChange, percentage), Bounds.Y1, Bounds.X2 - LinCalc(0.5, sizeChange / 2, 0, 0, percentage), Bounds.Y2), , True)
            SetScaling(Bounds.Size)

        ElseIf command Is ChartCommands.ResizeVerticalBiggerfine Then
            TrySetBounds(New Bounds(Bounds.X1, Bounds.Y1 - GetRelativeFromRealHeight(My.Application.FineChartResizeValue), Bounds.X2, Bounds.Y2 + GetRelativeFromRealHeight(My.Application.FineChartResizeValue)))
            SetScaling(Bounds.Size)
        ElseIf command Is ChartCommands.ResizeVerticalSmallerfine Then
            TrySetBounds(New Bounds(Bounds.X1, Bounds.Y1 + GetRelativeFromRealHeight(My.Application.FineChartResizeValue), Bounds.X2, Bounds.Y2 - GetRelativeFromRealHeight(My.Application.FineChartResizeValue)))
            SetScaling(Bounds.Size)
        ElseIf command Is ChartCommands.ResizeVerticalBiggercoarse Then
            TrySetBounds(New Bounds(Bounds.X1, Bounds.Y1 - GetRelativeFromRealHeight(My.Application.CoarseChartResizeValue), Bounds.X2, Bounds.Y2 + GetRelativeFromRealHeight(My.Application.CoarseChartResizeValue)))
            SetScaling(Bounds.Size)
        ElseIf command Is ChartCommands.ResizeVerticalSmallercoarse Then
            TrySetBounds(New Bounds(Bounds.X1, Bounds.Y1 + GetRelativeFromRealHeight(My.Application.CoarseChartResizeValue), Bounds.X2, Bounds.Y2 - GetRelativeFromRealHeight(My.Application.CoarseChartResizeValue)))
            SetScaling(Bounds.Size)
        ElseIf command Is ChartCommands.AutoScale Then
            For Each ch In Parent.Charts
                If ch.BarIndex > 0 AndAlso ch.BarIndex > ch.Bounds.X1 Then
                    Dim highValue As Double = Double.MinValue
                    Dim lowValue As Double = Double.MaxValue
                    For i = Max(Min(ch.Bounds.X1, ch.BarIndex), 1) To Min(ch.BarIndex - 1, ch.Bounds.X2)
                        highValue = Max(highValue, ch.bars(i).Data.High)
                        lowValue = Min(lowValue, ch.bars(i).Data.Low)
                    Next
                    Dim bnds As Bounds = New Bounds(ch.Bounds.X1, -highValue, ch.Bounds.X2, -lowValue)
                    Dim sze As Size = New Size(ch.mainCanvas.ActualWidth, ch.mainCanvas.ActualHeight)
                    Dim zero As Point = GetRelativeFromRealBounds(New Point(0, 0), bnds, sze)
                    Dim point As Point = GetRelativeFromRealBounds(New Point(0, ch.Settings("YMargin").Value * ch.mainCanvas.ActualHeight), bnds, sze)
                    highValue += point.Y - zero.Y
                    lowValue -= point.Y - zero.Y
                    ch.TrySetBounds(New Bounds(ch.Bounds.X1, -highValue, ch.Bounds.X2, -lowValue))
                End If
            Next
        ElseIf command Is ChartCommands.SetTimeScaleAsGlobalDefault Then
            WriteSetting(GLOBAL_CONFIG_FILE, "DefaultTimeScale", Bounds.X2 - Bounds.X1)
            'For Each d In My.Application.Desktops
            '    For Each w In d.Workspaces
            '        w.WorkspaceDefaultRealBarHeight = GetRealFromRelativeHeight(Settings("RangeValue").Value)
            '    Next
            'Next
            Log("set scaling to global default")
        ElseIf command Is ChartCommands.SetTimeScaleAsWorkspaceDefault Then
            Parent.WorkspaceDefaultTimeScale = Bounds.X2 - Bounds.X1
            Parent.WorkspaceDefaultRealBarHeight = GetRealFromRelativeHeight(Settings("RangeValue").Value)
            Log("set scaling to workspace default")

        ElseIf command Is ChartCommands.SetTimeAndPriceScaleAsChartDefault Then
            For Each ch In Parent.Charts
                ch.Settings("ChartDefaultTimeScale1").Value = ch.Bounds.X2 - ch.Bounds.X1
                ch.Settings("ChartDefaultPriceScale1").Value = ch.GetRealFromRelativeHeight(ch.Settings("RangeValue").Value)
            Next
            Log("set all chart scalings to chart default")
        ElseIf command Is ChartCommands.SnapTimeAndPriceScaleAsChartDefault Then
            For Each ch In Parent.Charts
                Dim height As Integer = ch.Settings("ChartDefaultPriceScale1").Value
                Dim margin As Decimal = ((ch.Settings("RangeValue").Value / ch.GetRelativeFromRealHeight(height)) * ch.Bounds.Height - ch.Bounds.Height) / 2
                ch.SetScaling(New Size(ch.Settings("ChartDefaultTimeScale1").Value, Abs((ch.Bounds.Y1 - margin) - (ch.Bounds.Y2 + margin))))
            Next
            'Bounds = New Bounds(Bounds.X1, Bounds.Y1 - margin, Bounds.X2, Bounds.Y2 + margin)
            'ElseIf command Is ChartCommands.SetTimeAndPriceScaleAsChartDefault2 Then
            '    Settings("ChartDefaultTimeScale2").Value = Bounds.X2 - Bounds.X1
            '    Settings("ChartDefaultPriceScale2").Value = GetRealFromRelativeHeight(Settings("RangeValue").Value)
            '    Log("set scaling to chart default 2")
            'ElseIf command Is ChartCommands.SnapTimeAndPriceScaleAsChartDefault2 Then
            '    Dim height As Integer = Settings("ChartDefaultPriceScale2").Value
            '    Dim margin As Decimal = ((Settings("RangeValue").Value / GetRelativeFromRealHeight(height)) * Bounds.Height - Bounds.Height) / 2
            '    SetScaling(New Size(Settings("ChartDefaultTimeScale2").Value, Abs((Bounds.Y1 - margin) - (Bounds.Y2 + margin))))
        ElseIf command Is ChartCommands.SnapTimeScaleAsGlobalDefault Then
            For Each chart In Parent.Charts
                'Dim chart = Me
                'Dim margin As Decimal = ((Chart.Settings("RangeValue").Value / Chart.GetRelativeFromRealHeight(height)) * Chart.Bounds.Height - Chart.Bounds.Height) / 2
                chart.SetScaling(New Size(ReadSetting(GLOBAL_CONFIG_FILE, "DefaultTimeScale", 200), chart.Bounds.Height))
                'chart.Bounds = New Bounds(chart.Bounds.X1, chart.Bounds.Y1, chart.Bounds.X2, chart.Bounds.Y2)
            Next
            'height = Parent.WorkspaceDefaultRealBarHeight
            'For Each chart In Parent.Charts
            '    Dim margin As Decimal = ((chart.Settings("RangeValue").Value / chart.GetRelativeFromRealHeight(Height)) * chart.Bounds.Height - chart.Bounds.Height) / 2
            '    chart.SetScaling(New Size(ReadSetting(GLOBAL_CONFIG_FILE, "DefaultTimeScale", 200), chart.Bounds.Height))
            '    chart.Bounds = New Bounds(chart.Bounds.X1, chart.Bounds.Y1 - margin, chart.Bounds.X2, chart.Bounds.Y2 + margin)
            'Next
        ElseIf command Is ChartCommands.SnapTimeScaleAsWorkspaceDefault Then
            'Dim height As Integer = Parent.WorkspaceDefaultRealBarHeight
            'For Each Chart In Parent.Charts
            '    Dim margin As Decimal = ((Chart.Settings("RangeValue").Value / Chart.GetRelativeFromRealHeight(height)) * Chart.Bounds.Height - Chart.Bounds.Height) / 2
            '    Chart.SetScaling(New Size(ReadSetting(GLOBAL_CONFIG_FILE, "DefaultTimeScale", 200), Chart.Bounds.Height))
            '    Chart.Bounds = New Bounds(Chart.Bounds.X1, Chart.Bounds.Y1 - margin, Chart.Bounds.X2, Chart.Bounds.Y2 + margin)
            'Next
            'height = Parent.WorkspaceDefaultRealBarHeight
            For Each Chart In Parent.Charts
                'Dim margin As Decimal = ((Chart.Settings("RangeValue").Value / Chart.GetRelativeFromRealHeight(Height)) * Chart.Bounds.Height - Chart.Bounds.Height) / 2
                Chart.SetScaling(New Size(Parent.WorkspaceDefaultTimeScale, Chart.Bounds.Height))
                'Chart.Bounds = New Bounds(Chart.Bounds.X1, Chart.Bounds.Y1 - margin, Chart.Bounds.X2, Chart.Bounds.Y2 + margin)
            Next
            'SetScaling(New Size(Parent.WorkspaceDefaultTimeScale, Bounds.Height))
        ElseIf command.Name.Contains("SetScalingAsPreset") Then
            Settings("ScalingPreset" & command.Name.Substring(18)).Value = Bounds
        ElseIf command.Name.Contains("SnapScalingToPreset") Then
            SetScaling(Settings("ScalingPreset" & command.Name.Substring(19)).Value.Size)
        ElseIf command Is ChartCommands.FormatAnalysisTechniques Then
            Dim formatWin As New FormatAnalysisTechniquesWindow(Me)
        ElseIf command Is ChartCommands.RefreshAllAnalysisTechniques Then
            For Each Chart In Parent.Charts
                If Chart.statusText.Text <> "" Then Chart.SetStatusText("reapplying analysis techniques...")
                For Each analysisTechnique In Chart.AnalysisTechniques
                    Chart.ReApplyAnalysisTechnique(analysisTechnique.AnalysisTechnique)
                Next
                If Chart.statusText.Text <> "" Then Chart.SetStatusTextToDefault(False)
                DispatcherHelper.DoEvents()
            Next
        ElseIf command Is ChartCommands.SnapScalingToAllData Then
            Dim ch = Me
            Static hasPreviousSize As Boolean = False
                Static previousSize As Size
                Static scaledBounds As Bounds
                Dim low As Double = Double.MaxValue, high As Double = 0
                For Each bar In ch.bars
                    low = Min(bar.Data.Low, low)
                    high = Max(bar.Data.High, high)
                Next

                If hasPreviousSize = False Or ch.Bounds.ToWindowsRect <> scaledBounds.ToWindowsRect Then
                    previousSize = ch.Bounds.Size
                    Dim margin As Double = (ch.GetRealWithRelativeHeightInRelative(Settings("YMargin").Value * ch.mainCanvas.ActualHeight * 2, high - low) - (high - low)) / 2
                    Dim littleMargin As Double = (ch.GetRealWithRelativeHeightInRelative(0.03 * ch.mainCanvas.ActualHeight * 2, high - low) - (high - low)) / 2
                    ch.TrySetBounds(New Bounds(0, Min(-high - littleMargin, -ch.CurrentBar.Close - margin), ch.GetRealWithRelativeWidthInRelative(ch.Settings("XMargin").Value * ch.mainCanvas.ActualWidth + ch.Settings("PriceBarWidth").Value, ch.bars.Count), Max(-low + littleMargin, -ch.CurrentBar.Close + margin)))
                    scaledBounds = ch.Bounds
                    hasPreviousSize = True
                Else
                    scaledBounds = New Bounds(0, 0, 1, 1)
                    ch.SetScaling(previousSize)
                    hasPreviousSize = False
                End If

                Dim height As Integer = ch.Parent.WorkspaceDefaultRealBarHeight
                Dim Margin1 = ((ch.Settings("RangeValue").Value / ch.GetRelativeFromRealHeight(height)) * ch.Bounds.Height - ch.Bounds.Height) / 2
            ch.Bounds = New Bounds(ch.Bounds.X1, ch.Bounds.Y1 - Margin1, ch.Bounds.X2, ch.Bounds.Y2 + Margin1)
            If ch.BarIndex > 0 AndAlso ch.BarIndex > ch.Bounds.X1 Then
                Dim highValue As Double = Double.MinValue
                Dim lowValue As Double = Double.MaxValue
                For i = Max(Min(ch.Bounds.X1, ch.BarIndex), 1) To Min(ch.BarIndex - 1, ch.Bounds.X2)
                    highValue = Max(highValue, ch.bars(i).Data.High)
                    lowValue = Min(lowValue, ch.bars(i).Data.Low)
                Next
                Dim bnds As Bounds = New Bounds(ch.Bounds.X1, -highValue, ch.Bounds.X2, -lowValue)
                Dim sze As Size = New Size(ch.mainCanvas.ActualWidth, ch.mainCanvas.ActualHeight)
                Dim zero As Point = GetRelativeFromRealBounds(New Point(0, 0), bnds, sze)
                Dim point As Point = GetRelativeFromRealBounds(New Point(0, ch.Settings("YMargin").Value * ch.mainCanvas.ActualHeight), bnds, sze)
                highValue += point.Y - zero.Y
                lowValue -= point.Y - zero.Y
                ch.TrySetBounds(New Bounds(ch.Bounds.X1, -highValue, ch.Bounds.X2, -lowValue))
            End If
        ElseIf e.Command Is ChartCommands.CloseChart Then
            ResetSettings()
            DataStream.CancelMarketData(TickerID)
            DataStream.CancelHistoricalData(TickerID)
            RemoveDataStreamHandlers()
            Parent.Children.Remove(Me)
            'ElseIf e.Command Is ChartCommands.NewSlaveChart Then
            '    If Not Settings("IsSlaveChart").Value Then
            '        Dim c As New Chart
            '        c.Settings("IsSlaveChart").Value = True
            '        c.MasterChart = Me
            '        c.Settings("MasterChartTickerID").Value = TickerID
            '        'c.DataStream.BarSize(TickerID) = DataStream.BarSize(MasterChart.TickerID)
            '        Parent.Children.Add(c)
            '        SlaveCharts.Add(c)
            '    Else
            '        ShowInfoBox("Slave charts cannot be masters of other charts.", DesktopWindow)
            '    End If
            '    'ElseIf e.Command Is ChartCommands.IncreaseRange Or e.Command Is ChartCommands.DecreaseRange Or e.Command Is ChartCommands.IncreaseRange2xMinMove Or e.Command Is ChartCommands.DecreaseRange2xMinMove Then
            '    '    If Not Settings("IsSlaveChart").Value Then
            '    '        Settings("RangeValue").Value += If(e.Command Is ChartCommands.IncreaseRange Or e.Command Is ChartCommands.IncreaseRange2xMinMove, 1, -1) * GetMinTick() * If(e.Command Is ChartCommands.IncreaseRange2xMinMove Or e.Command Is ChartCommands.DecreaseRange2xMinMove, 2, 1)
            '    '        For Each item In AnalysisTechniques
            '    '            If TypeOf item.AnalysisTechnique Is AutoTrendBase Then
            '    '                CType(item.AnalysisTechnique, AutoTrendBase).RV = 2.5 * Settings("RangeValue").Value
            '    '            End If
            '    '        Next
            '    '        RequestData(TimeSpan.FromDays(Settings("DaysBack").Value), False)
            '    '    Else
            '    '        ShowInfoBox("You cannot adjust the range on a slave chart.", DesktopWindow)
            '    '    End If
        ElseIf e.Command Is ChartCommands.RefreshChart Then
            RequestData(TimeSpan.FromDays(DataStream.DaysBack(TickerID)), (Now - DataStream.CurrentReplayTime), False)

            'Next
            'Next
            'ElseIf e.Command Is ChartCommands.SwitchAutoTrendColoring Then
            '    Dim allSwingsHidden As Boolean = True
            '    For Each item In AnalysisTechniques
            '        If TypeOf item.AnalysisTechnique Is AutoTrendNew Then
            '            If CType(item.AnalysisTechnique, AutoTrendNew).SwingLinesVisible Then allSwingsHidden = False
            '            'ElseIf TypeOf item.AnalysisTechnique Is AutoTrend Then
            '            '    If CType(item.AnalysisTechnique, AutoTrend).SwingLinesVisible Then allSwingsHidden = False
            '            'ElseIf TypeOf item.AnalysisTechnique Is AutoTrendV2 Then
            '            '    If CType(item.AnalysisTechnique, AutoTrendV2).SwingLinesVisible Then allSwingsHidden = False
            '            'ElseIf TypeOf item.AnalysisTechnique Is OriginalAutoTrend Then
            '            '    If CType(item.AnalysisTechnique, OriginalAutoTrend).SwingLinesVisible Then allSwingsHidden = False
            '        End If
            '    Next
            '    Dim autoTrendToColor As AutoTrendBase = Nothing
            '    For Each item In AnalysisTechniques
            '        If TypeOf item.AnalysisTechnique Is AutoTrendNew Then
            '            CType(item.AnalysisTechnique, AutoTrendNew).ColorBars = Not CType(item.AnalysisTechnique, AutoTrendNew).ColorBars
            '            If Not allSwingsHidden Then CType(item.AnalysisTechnique, AutoTrendNew).SwingLinesVisible = CType(item.AnalysisTechnique, AutoTrendNew).ColorBars
            '            If CType(item.AnalysisTechnique, AutoTrendNew).ColorBars Then
            '                autoTrendToColor = item.AnalysisTechnique
            '            Else
            '                ReApplyAnalysisTechnique(item.AnalysisTechnique)
            '            End If
            '            'ElseIf TypeOf item.AnalysisTechnique Is AutoTrend Then
            '            '    CType(item.AnalysisTechnique, AutoTrend).ColorBars = Not CType(item.AnalysisTechnique, AutoTrend).ColorBars
            '            '    If Not allSwingsHidden Then CType(item.AnalysisTechnique, AutoTrend).SwingLinesVisible = CType(item.AnalysisTechnique, AutoTrend).ColorBars
            '            '    If CType(item.AnalysisTechnique, AutoTrend).ColorBars Then
            '            '        autoTrendToColor = item.AnalysisTechnique
            '            '    Else
            '            '        ReApplyAnalysisTechnique(item.AnalysisTechnique)
            '            '    End If
            '            'ElseIf TypeOf item.AnalysisTechnique Is AutoTrendV2 Then
            '            '    CType(item.AnalysisTechnique, AutoTrendV2).ColorBars = Not CType(item.AnalysisTechnique, AutoTrendV2).ColorBars
            '            '    If Not allSwingsHidden Then CType(item.AnalysisTechnique, AutoTrendV2).SwingLinesVisible = CType(item.AnalysisTechnique, AutoTrendV2).ColorBars
            '            '    If CType(item.AnalysisTechnique, AutoTrendV2).ColorBars Then
            '            '        autoTrendToColor = item.AnalysisTechnique
            '            '    Else
            '            '        ReApplyAnalysisTechnique(item.AnalysisTechnique)
            '            '    End If
            '            '    ElseIf TypeOf item.AnalysisTechnique Is OriginalAutoTrend Then
            '            '    CType(item.AnalysisTechnique, OriginalAutoTrend).ColorBars = Not CType(item.AnalysisTechnique, OriginalAutoTrend).ColorBars
            '            '    If Not allSwingsHidden Then CType(item.AnalysisTechnique, OriginalAutoTrend).SwingLinesVisible = CType(item.AnalysisTechnique, OriginalAutoTrend).ColorBars
            '            '    If CType(item.AnalysisTechnique, OriginalAutoTrend).ColorBars Then
            '            '        autoTrendToColor = item.AnalysisTechnique
            '            '    Else
            '            '        ReApplyAnalysisTechnique(item.AnalysisTechnique)
            '            '    End If
            '        End If
            '    Next
            '    If autoTrendToColor IsNot Nothing Then ReApplyAnalysisTechnique(autoTrendToColor)
            'ElseIf e.Command Is ChartCommands.ToggleAutoTrend Then
            '    Dim turnedOn As Boolean = False
            '    For Each item In Me.AnalysisTechniques
            '        If TypeOf item.AnalysisTechnique Is AutoTrendBase Then
            '            item.AnalysisTechnique.IsEnabled = Not item.AnalysisTechnique.IsEnabled
            '            If item.AnalysisTechnique.IsEnabled Then
            '                turnedOn = True
            '                Me.ReApplyAnalysisTechnique(item.AnalysisTechnique)
            '            Else
            '                Me.RemoveAnalysisTechnique(item.AnalysisTechnique)
            '            End If
            '        End If
            '    Next
            '    For Each item In Me.AnalysisTechniques
            '        If TypeOf item.AnalysisTechnique Is RegressionCurve Then
            '            Me.ReApplyAnalysisTechnique(item.AnalysisTechnique)
            '        End If
            '    Next
            '    For Each ch In Parent.Charts
            '        For Each item In ch.AnalysisTechniques
            '            If TypeOf item.AnalysisTechnique Is AutoTrendBase Then
            '                item.AnalysisTechnique.IsEnabled = turnedOn
            '                If item.AnalysisTechnique.IsEnabled Then
            '                    ch.ReApplyAnalysisTechnique(item.AnalysisTechnique)
            '                Else
            '                    ch.RemoveAnalysisTechnique(item.AnalysisTechnique)
            '                End If
            '            End If
            '        Next
            '        For Each item In ch.AnalysisTechniques
            '            If TypeOf item.AnalysisTechnique Is RegressionCurve Then
            '                ch.ReApplyAnalysisTechnique(item.AnalysisTechnique)
            '            End If
            '        Next
            '    Next
            'ElseIf e.Command Is ChartCommands.ToggleRegressionCurve Then
            '    Dim turnedOn As Boolean = False
            '    For Each item In Me.AnalysisTechniques
            '        If TypeOf item.AnalysisTechnique Is RegressionCurve Then
            '            item.AnalysisTechnique.IsEnabled = Not item.AnalysisTechnique.IsEnabled
            '            If item.AnalysisTechnique.IsEnabled Then
            '                Me.ReApplyAnalysisTechnique(item.AnalysisTechnique)
            '                turnedOn = True
            '            Else
            '                Me.RemoveAnalysisTechnique(item.AnalysisTechnique)
            '            End If
            '        End If
            '    Next
            '    For Each item In Me.AnalysisTechniques
            '        If TypeOf item.AnalysisTechnique Is AutoTrendNew Then
            '            CType(item.AnalysisTechnique, AutoTrendNew).SwingLinesVisible = Not turnedOn
            '            Me.ReApplyAnalysisTechnique(item.AnalysisTechnique)
            '        End If
            '    Next

            '    For Each ch In Parent.Charts
            '        If ch IsNot Me Then
            '            For Each item In ch.AnalysisTechniques
            '                If TypeOf item.AnalysisTechnique Is RegressionCurve Then
            '                    item.AnalysisTechnique.IsEnabled = turnedOn
            '                    If item.AnalysisTechnique.IsEnabled Then
            '                        ch.ReApplyAnalysisTechnique(item.AnalysisTechnique)
            '                    Else
            '                        ch.RemoveAnalysisTechnique(item.AnalysisTechnique)
            '                    End If
            '                End If
            '            Next
            '            For Each item In ch.AnalysisTechniques
            '                If TypeOf item.AnalysisTechnique Is AutoTrendNew Then
            '                    CType(item.AnalysisTechnique, AutoTrendNew).SwingLinesVisible = Not turnedOn
            '                    ch.ReApplyAnalysisTechnique(item.AnalysisTechnique)
            '                End If
            '            Next
            '        End If
            '    Next
        ElseIf e.Command Is ChartCommands.SavePointOfInterest Then
            If bars.Count > 0 Then MasterWindow.ReplayPointsOfInterest.Add(CurrentBar.Date)
        End If
    End Sub

    Private Sub FormatChart()
        Dim loadingHistory As Boolean = False
        Dim loadingChart As Chart = Nothing
        For Each desktop In My.Application.Desktops
            For Each workspace In desktop.Workspaces
                For Each chart In workspace.Charts
                    If chart.IsLoadingHistory Then
                        loadingHistory = True
                        loadingChart = chart
                    End If
                Next
            Next
        Next
        If loadingHistory Then
            If ShowInfoBox("You can not format a chart when a chart is loading data. Do you want to cancel loading data?", DesktopWindow, "Yes", "No") = 0 Then
                loadingChart.IsLoadingHistory = False
                loadingChart.DataStream.CancelHistoricalData(TickerID)
            End If
            Exit Sub
        End If
        Dim winConfig As New FormatWindow(Me)
        If bars.Count = 0 Then winConfig.RefreshChart = True
        winConfig.Owner = DesktopWindow
        winConfig.ShowDialog()
        For Each chart In Parent.Charts
            chart.RefreshOrderBox()
        Next
        If winConfig.ButtonOKPressed Then
            If winConfig.Settings("UseRandom").Value Then
                DataStream = RAND
            Else
                DataStream = IB
            End If
            With winConfig
                If Settings("Bar Color").Value <> .Settings("Bar Color").Value Then
                    For Each bar In bars
                        bar.Pen = New Pen(New SolidColorBrush(.Settings("Bar Color").Value), My.Application.BarWidth)
                        bar.CloseTickPen = New Pen(New SolidColorBrush(.Settings("Bar Color").Value), 1)
                        bar.OpenTickPen = New Pen(New SolidColorBrush(.Settings("Bar Color").Value), 1)
                        bar.TickWidth = My.Application.TickWidth
                        bar.RefreshVisual()
                    Next
                End If
                DataStream.BarSize(TickerID) = .Settings("BarSize").Value
                RAND.RandomMaxMove = .Settings("RAND.RandomMaxMove").Value
                RAND.RandomMinMove = .Settings("RAND.RandomMinMove").Value
                RAND.RandomSpeed = .Settings("RAND.RandomSpeed").Value
                Dim daysBackChanged As Boolean = .Settings("DaysBack").Value <> DataStream.DaysBack(TickerID)
                For Each item In .Settings.Settings
                    If Settings.Contains(item.Name) AndAlso Settings(item.Name).HasValue Then Settings(item.Name).Value = item.Value
                Next
                Settings("DecimalPlaces").Value = CInt(Settings("DecimalPlaces").Value)
                InitGrid()
                If deflectionSlider IsNot Nothing Then CType(deflectionSlider.Content, Slider).Value = 0.5
                If .RefreshChart Then
                    RequestData(TimeSpan.FromDays(DataStream.DaysBack(TickerID)), (Now - DataStream.CurrentReplayTime), winConfig.ChangedSettingNames.Contains("IB.Symbol"))
                    'End If
                Else
                    'If .ChangedSettingNames.Contains("DefaultOrderQuantity") Then
                    '    For Each chart In Parent.Charts
                    '        If (chart.Settings("UseRandom").Value = Settings("UseRandom").Value And Settings("UseRandom").Value = False) And chart.IB.Contract(chart.TickerID).Symbol = IB.Contract(TickerID).Symbol Then
                    '            chart.Settings("DefaultOrderQuantity").Value = Settings("DefaultOrderQuantity").Value
                    '            chart.btnQuantity.Tag = 0
                    '            'For Each item In chart.AnalysisTechniques
                    '            'If TypeOf item.AnalysisTechnique Is AutoTrendNew Then ReApplyAnalysisTechnique(item.AnalysisTechnique)
                    '            'Next
                    '            chart.RefreshOrderBox()
                    '        End If
                    '    Next
                    'End If

                    RefreshAxisAndGrid()
                    RefreshOrderBox()

                End If
            End With
            RaiseEvent ChartFormatted(Me, New EventArgs)
            Dim a = New EventArgs
        End If
    End Sub
#End Region


End Class