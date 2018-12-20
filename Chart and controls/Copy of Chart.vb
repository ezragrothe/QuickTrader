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

    Private _designMode As Boolean
    Public DesktopWindow As DesktopWindow = Nothing
    Public MasterWindow As MasterWindow = Nothing

    Public Property ChartOverrideName As String = ""


    Public Sub Load()

        DesktopWindow = CType(GetParent(Me, GetType(DesktopWindow)), DesktopWindow)
        _designMode = Not TypeOf DesktopWindow Is DesktopWindow
        MasterWindow = My.Application.MasterWindow

        If Not IsDesignTime Then

            IB = MasterWindow.IB


            TickerID = GetNextTickerID()

            ResetBars()
            LoadSettings()
            LoadMenus()

            ChartOverrideName = ""

            mainCanvas.Focus()
            If Settings("UseRandom").Value Then DataStream = RAND Else DataStream = IB
            ChangeSetting("ChartBorderWidth")
        End If
    End Sub

    Dim mnuViewOrderBar As MenuItem
    Private Sub LoadMenus()
        Dim menu As New CommandContextMenu(DesktopWindow)
        mnuViewOrderBar = NewMenuItemWrapper("View Order Bar", "mnuViewOrderBar")
        mnuViewOrderBar.IsCheckable = True
        mnuViewOrderBar.IsChecked = Settings("OrderBarVisibility").Value
        AddHandler mnuViewOrderBar.Checked, Sub()
                                                If bars.Count > 0 AndAlso Not Children.Contains(orderBox) Then Children.Add(orderBox)
                                                Settings("OrderBarVisibility").Value = True
                                            End Sub
        AddHandler mnuViewOrderBar.Unchecked, Sub()
                                                  Children.Remove(orderBox)
                                                  Settings("OrderBarVisibility").Value = False
                                              End Sub
        Dim scalingMenu As CommandMenuItem = NewMenuItemWrapper("Scaling", "")
        Dim scalingMenuItems() As CommandMenuItem = {
            NewMenuItemWrapper("Set Time Scale as Global Default", "", ChartCommands.SetTimeScaleAsGlobalDefault),
            NewMenuItemWrapper("Snap Time Scaling to Global Default", "", ChartCommands.SnapTimeScalingToGlobalDefault),
            NewMenuItemWrapper("Set Scaling as Preset 1", "", ChartCommands.SetScalingAsPreset1),
            NewMenuItemWrapper("Set Scaling as Preset 2", "", ChartCommands.SetScalingAsPreset2),
            NewMenuItemWrapper("Snap Scaling to Preset 1", "", ChartCommands.SnapScalingToPreset1),
            NewMenuItemWrapper("Snap Scaling to Preset 2", "", ChartCommands.SnapScalingToPreset2)
        }
        scalingMenu.ItemsSource = scalingMenuItems
        Dim menuItems() As CommandMenuItem = {
            NewMenuItemWrapper("Format...", "", ChartCommands.Format),
            NewMenuItemWrapper("Format Analysis Techniques...", "", ChartCommands.FormatAnalysisTechniques),
            NewMenuItemWrapper("Format Hotkeys...", "", ChartCommands.FormatHotKeys),
            NewMenuItemWrapper("Format Workspace...", "", ChartCommands.FormatWorkspace),
            scalingMenu, mnuViewOrderBar
        }

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
        If Settings("DaysBack").Value <> 0 Then
            historicalDuration = TimeSpan.FromDays(Settings("DaysBack").Value)
            SetStatusText("loading...")
            DataStream.RequestHistoricalData(Now, historicalDuration, TickerID)
        Else
            HistoricalDataFinished()
        End If
    End Sub
    Private Sub Chart_Loaded(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs) Handles Me.Loaded
        Load()
        LoadBorder()
        InitOrderBox()
        LoadCompleted()
    End Sub

    Public Function GetNextTickerID() As Integer
        Dim ids As New List(Of Integer)
        For Each desktop In My.Application.Desktops
            For Each workspace In desktop.Workspaces
                For Each chart As Chart In workspace.Charts
                    ids.Add(chart.TickerID)
                Next
            Next
        Next
        Dim id As Integer = 1
        While ids.Contains(id)
            id += 1
        End While
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
            For Each input In technique.AnalysisTechnique.Inputs
                input.Value = input.Converter.ConvertFromString(ReadSetting(Parent.FilePath, GetChartName() & "." & technique.AnalysisTechnique.Name & technique.ID & "." & input.Name, ReadSetting(GLOBAL_CONFIG_FILE, technique.AnalysisTechnique.Name & "." & input.Name, input.StringValue)))
            Next
            technique.AnalysisTechnique.IsEnabled = Boolean.Parse(ReadSetting(Parent.FilePath, GetChartName() & "." & technique.AnalysisTechnique.Name & technique.ID & ".IsEnabled", "False"))
            AnalysisTechniques.Add(technique)
        End If
    End Sub
    Public AnalysisTechniques As New List(Of AnalysisTechniqueInformation)
    Private Sub ApplyAnalysisTechniques()
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
            analysisTechnique.Update(bars(i).Data)
            analysisTechnique.UpdateNewBar()
            If Now - t > TimeSpan.FromMinutes(1) Then
                'timedOut = True
                'percentCompleted = (i + 1) / bars.Count
                'Exit For
            End If
        Next
        If timedOut Then
            'RemoveAnalysisTechnique(analysisTechnique)
            'ShowInfoBox("Execution timed out at " & percentCompleted * 100 & "% completion.")
        Else
            analysisTechnique.HistoryLoaded()
            analysisTechniquesLoading = False
            SetControlsOnTop()
        End If

        'MsgBox("finished")
        'ShowInfoBox(analysisTechnique.Name & " took an average of " & (Now - t).TotalMilliseconds / bars.Count & " milliseconds per bar")
        'RefreshChartChildren()
        'ChildrenChangedAxis(Nothing)
    End Sub
    Public Sub ReApplyAnalysisTechnique(ByVal analysisTechnique As AnalysisTechnique)
        If analysisTechnique.IsEnabled Then
            RemoveAnalysisTechnique(analysisTechnique)
            analysisTechnique.Reset()
            ApplyAnalysisTechnique(analysisTechnique)
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
        For Each item In AnalysisTechniques
            item.AnalysisTechnique.UpdateNewBar()
        Next
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
                WriteSetting(Parent.FilePath, name & "." & technique.AnalysisTechnique.Name & technique.ID & "." & input.Name, input.StringValue)
            Next
            WriteSetting(Parent.FilePath, name & "." & technique.AnalysisTechnique.Name & technique.ID & ".IsEnabled", technique.AnalysisTechnique.IsEnabled.ToString)
        Next
        WriteSetting(Parent.FilePath, name & ".AnalysisTechniques", Join(names.ToArray, ";"))
        WriteSetting(Parent.FilePath, name & ".AnalysisTechniqueIDs", Join(ids.ToArray, ","))
    End Sub

    Public Function RecompileExternalAnalysisTechnique(ByVal analysisTechnique As AnalysisTechniqueInformation) As AnalysisTechniqueInformation
        With analysisTechnique
            If .Identifier.Substring(0, 2) = "f-" Then

                Dim prev As AnalysisTechnique = .AnalysisTechnique
                Dim prevInputs As ReadOnlyCollection(Of Input) = prev.Inputs

                .AnalysisTechnique = InstantiateAnalysisTechnique(AnalysisTechniqueHelper.LoadAnalysisTechnique(.Identifier), Me)
                If .AnalysisTechnique Is Nothing Then
                    ShowInfoBox("Error: Could not compile " & prev.Name & ".")
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

    Friend WithEvents RAND As New RandomDataStream
    Friend WithEvents DataStream As DataStream

    Friend bars As New List(Of Bar)
    Private priceText As New TextBlock
    Private LoadingHistory As Boolean
    Private Delegate Sub MarketPriceHandler(ByVal ticktype As TickType, ByVal price As Double)

    Private Sub AddBarWrapper(ByVal o As Double, ByVal h As Double, ByVal l As Double, ByVal c As Double)
        Dim bar As New Bar(New BarData(o, h, l, c, Now, TimeSpanFromBarSize(DataStream.BarSize(TickerID)), BarIndex + 1))
        bar.AutoRefresh = False
        bar.Pen.Thickness = My.Application.BarWidth
        bar.Pen.Brush = New SolidColorBrush(Settings("Bar Color").Value)
        bar.OpenTick = Settings("DisplayOpenTick").Value
        bar.CloseTick = Settings("DisplayCloseTick").Value
        bar.TickWidth = My.Application.TickWidth
        bar.IsSelectable = My.Application.ShowMousePopups

        If LoadingHistory Then
            bar.Data = bar.Data.SetDate(historicalTime)
            If bars.Count > 0 Then bars(bars.Count - 1).Data = bars(bars.Count - 1).Data.SetDuration(historicalTime - bars(bars.Count - 1).Data.Date)
        Else
            bar.Data = bar.Data.SetDate(Now)
            If bars.Count > 0 Then bars(bars.Count - 1).Data = bars(bars.Count - 1).Data.SetDuration(Now - bars(bars.Count - 1).Data.Date)
        End If
        bars.Add(bar)
        Children.Add(bar)
        UpdatePrice()
    End Sub
    Private Sub ResetBars()
        bars.Clear()
    End Sub

    Private Sub IB_Error(ByVal sender As Object, ByVal e As Krs.Ats.IBNet.ErrorEventArgs) Handles IB.Error
        If e.TickerId = TickerID Then
            If Not (e.ErrorCode = 1100 Or e.ErrorCode = 1102) Then
                ShowInfoBox([Enum].GetName(GetType(Krs.Ats.IBNet.ErrorMessage), e.ErrorCode) & vbNewLine & e.ErrorMsg)
                If LoadingHistory Then SetStatusText("error, try reloading")
                LoadingHistory = False
            End If
        End If
    End Sub

    Private recieveData As Boolean
    Private Sub ib_MarketPriceData(ByVal tickType As Krs.Ats.IBNet.TickType, ByVal price As Double, ByVal id As Integer) Handles DataStream.MarketPriceData
        If Settings("UseRandom").Value OrElse id = TickerID Then
            Dispatcher.BeginInvoke(CType(AddressOf MarketPrice, MarketPriceHandler), DispatcherPriority.Normal, tickType, price)
        End If
    End Sub
    Private Sub MarketPrice(ByVal ticktype As TickType, ByVal price As Decimal)
        If ticktype = Krs.Ats.IBNet.TickType.LastPrice Then
            If recieveData Then CalculatePriceChange(New BarData(price, price, price, price))
        ElseIf ticktype = Krs.Ats.IBNet.TickType.BidPrice Then
            _bidPrice = price
            If bars.Count > 0 Then
                UpdateAnalysisTechniques(CurrentBar.Close)
            End If
        ElseIf ticktype = Krs.Ats.IBNet.TickType.AskPrice Then
            _askPrice = price
            If bars.Count > 0 Then
                UpdateAnalysisTechniques(CurrentBar.Close)
            End If
        End If
    End Sub
    Private currentBarLowPrice As Double = Double.MaxValue, currentBarHighPrice As Double = Double.MinValue
    Private Delegate Sub CalculatePriceChangeHandler(ByVal bar As BarData)
    Private Sub CalculatePriceChange(ByVal bar As BarData)
        Static once As Boolean = True
        If once Or BarIndex = 0 Then
            AddBarWrapper(bar.Close, bar.Close, bar.Close, bar.Close)
            ResetAnalysisTechniques()
        End If
        If Max(currentBarHighPrice, bar.High) - Min(currentBarLowPrice, bar.Low) >= Settings("RangeValue").Value Then
            If bar.High > currentBarHighPrice Then
                bars(bars.Count - 1).Data = CurrentBar.SetHigh(CurrentBar.Low + Settings("RangeValue").Value)
                CurrentBarLine.RefreshVisual()
                For i = CDbl(CurrentBar.High) To bar.High - CDbl(Settings("RangeValue").Value) Step CDbl(Settings("RangeValue").Value)
                    AddBarWrapper(i, i + Settings("RangeValue").Value, i, i + Settings("RangeValue").Value)
                Next
                currentBarLowPrice = CurrentBar.High
                currentBarHighPrice = bar.High
                AddBarWrapper(CurrentBar.High, bar.High, CurrentBar.High, bar.High)
                UpdateAnalysisTechniquesForNewBar()
            ElseIf bar.Low < currentBarLowPrice Then
                bars(bars.Count - 1).Data = CurrentBar.SetLow(CurrentBar.High - Settings("RangeValue").Value)
                CurrentBarLine.RefreshVisual()
                For i = CDbl(CurrentBar.Low) To CDbl(bar.Low + CDbl(Settings("RangeValue").Value)) Step -CDbl(Settings("RangeValue").Value)
                    AddBarWrapper(i, i, i - Settings("RangeValue").Value, i - Settings("RangeValue").Value)
                Next
                currentBarHighPrice = CurrentBar.Low
                currentBarLowPrice = bar.Low
                AddBarWrapper(CurrentBar.Low, CurrentBar.Low, bar.Low, bar.Low)
                UpdateAnalysisTechniquesForNewBar()
            End If
            _isNewBar = True
        Else
            currentBarLowPrice = Min(Min(bar.Low, currentBarLowPrice), bar.Close)
            currentBarHighPrice = Max(Max(bar.High, currentBarHighPrice), bar.Close)
            bars(bars.Count - 1).Data = CurrentBar.SetHigh(currentBarHighPrice)
            bars(bars.Count - 1).Data = CurrentBar.SetLow(currentBarLowPrice)
            bars(bars.Count - 1).Data = CurrentBar.SetClose(bar.Close)
            CurrentBarLine.RefreshVisual()
            UpdatePrice()
            _isNewBar = False
        End If
        once = False
        LoadingHistory = False
    End Sub
    Private Sub CancelMarketData()
        recieveData = False
    End Sub
    Private Sub RequestMarketData()
        recieveData = True
    End Sub
    Private Sub UpdatePrice()
        If Not LoadingHistory Then
            UpdateAnalysisTechniques(CurrentBar.Close)
            For Each item In Children
                If TypeOf item Is Order Then CType(item, Order).RefreshVisual()
            Next
            If True Then
                Dim barDif As Integer = Ceiling(BarIndex - (Bounds.X2 - GetVirtualFromRealWidth(Settings("XMargin").Value * mainCanvas.ActualWidth) - (GetVirtualFromReal(New Point(Settings("PriceBarWidth").Value, 0)).X - GetVirtualFromReal(New Point(0, 0)).X)))
                If barDif <= 0 Then
                    If -Bounds.Y1 - GetVirtualFromRealHeight(Settings("YMargin").Value * mainCanvas.ActualHeight) < CurrentBar.Close Then
                        Dim dif = CurrentBar.Close + GetVirtualFromRealHeight(Settings("YMargin").Value * mainCanvas.ActualHeight) + Bounds.Y1
                        Bounds = New Bounds(Bounds.X1, Bounds.Y1 - dif, Bounds.X2, Bounds.Y2 - dif)
                    ElseIf GetVirtualFromRealHeight(Settings("YMargin").Value * mainCanvas.ActualHeight) - Bounds.Y2 > CurrentBar.Close Then
                        Dim dif = GetVirtualFromRealHeight(Settings("YMargin").Value * mainCanvas.ActualHeight) - Bounds.Y2 - CurrentBar.Close
                        Bounds = New Bounds(Bounds.X1, Bounds.Y1 + dif, Bounds.X2, Bounds.Y2 + dif)
                    End If
                End If
                If barDif = 1 Then
                    Bounds = New Bounds(Bounds.X1 + barDif, Bounds.Y1, Bounds.X2 + barDif, Bounds.Y2)
                End If
            End If
            bars(BarIndex - 1).RefreshVisual()

            Static once As Boolean = True
            If once Or Not LoadingHistory Then
                RefreshAxisAndGrid()
                once = False
            End If
        End If
    End Sub


    Private historicalBars As New List(Of BarData), historicalTime As Date
    Private Sub IB_HistoricalDataBarRecieved(ByVal bar As BarData, ByVal id As Integer) Handles DataStream.HistoricalDataBarRecieved
        If id = TickerID Or Settings("UseRandom").Value Then
            historicalBars.Add(bar)
        End If
    End Sub
    Private Sub IB_HistoricalDataPartiallyCompleted(ByVal id As Integer, ByVal percentCompleted As Double) Handles DataStream.HistoricalDataPartiallyCompleted
        Dispatcher.Invoke(
            Sub()
                If id = TickerID Or Settings("UseRandom").Value Then
                    loadingProgressBar.Value = percentCompleted
                    DispatcherHelper.DoEvents()
                End If
            End Sub)
    End Sub
    Private historicalDuration As TimeSpan

    Private Sub HistoricalDataFinished()
        RefreshOrderBox()
        InitGrid()
        'InitOrderButtons()
        'InitOrderBox()
        InitRandomDataControls()
        Dim endBar As Integer = Round(BarIndex + GetVirtualFromRealWidth(Settings("XMargin").Value * mainCanvas.ActualWidth) + (GetVirtualFromReal(New Point(Settings("PriceBarWidth").Value, 0)).X - GetVirtualFromReal(New Point(0, 0)).X))
        Dim bnds As New Bounds(endBar - Bounds.X2 + Bounds.X1, Bounds.Y1, endBar, Bounds.Y2)
        TrySetBounds(bnds.TopLeft, bnds.BottomRight, True)
        currentBarLowPrice = Double.MaxValue
        currentBarHighPrice = Double.MinValue
        'tickTimer.Change(CLng(TimeSpanFromBarSize(DataStream.BarSize).TotalMilliseconds), CLng(TimeSpanFromBarSize(DataStream.BarSize).TotalMilliseconds))
        historicalBars.Clear()
        historicalDuration = Nothing
        SetStatusTextToDefault(True)
        SetControlsOnTop()
        RefreshAxisAndGrid()
        RefreshAllSize()
        'RemoveHandler IB.OpenOrder, AddressOf IB_OpenOrder
        'AddHandler IB.OpenOrder, AddressOf IB_OpenOrder

        If Not Settings("UseRandom").Value Then IB.RequestAllOpenOrders()
        ApplyAnalysisTechniques()
        LoadingHistory = False

        RequestMarketData()
    End Sub
    Private Sub DrawHistoricalBars()
        loadingProgressBar.Visibility = Windows.Visibility.Hidden
        'If Not Settings("UseRandom").Value Then Log("drawing bars")
        SetStatusText("processing...")
        historicalBars.Sort(
            Function(item1 As BarData, item2 As BarData) As Boolean
                Return item1.Date < item2.Date
            End Function)
        Dim barsBack As Integer = historicalDuration.TotalMinutes / TimeSpanFromBarSize(DataStream.BarSize(TickerID)).TotalMinutes
        Dim startNum As Integer
        If barsBack <= 0 Or barsBack > historicalBars.Count Then
            startNum = 0
        ElseIf barsBack < historicalBars.Count Then
            startNum = historicalBars.Count - barsBack
        End If
        For i = startNum To historicalBars.Count - 1
            LoadingHistory = True
            historicalTime = historicalBars(i).Date
            CalculatePriceChange(historicalBars(i))
        Next
        currentBarLowPrice = CurrentBar.Low
        currentBarHighPrice = CurrentBar.High
        HistoricalDataFinished()
    End Sub
    Private Sub DataStream_HistoricalDataFinished(ByVal id As Integer) Handles DataStream.HistoricalDataCompleted
        If id = TickerID Or Settings("UseRandom").Value Then
            Dispatcher.Invoke(
                Sub()
                    Log("drawing bars     ticker id = " & id & "; random = " & Settings("UseRandom").Value)
                    DrawHistoricalBars()
                End Sub)
        End If
    End Sub
    Private Sub RequestData(ByVal duration As TimeSpan)
        loadingProgressBar.Visibility = Windows.Visibility.Visible
        loadingProgressBar.Value = 0
        CancelMarketData()
        Dim i As Integer
        While i < Children.Count
            If TypeOf Children(i) Is Bar OrElse Children(i).AnalysisTechnique IsNot Nothing OrElse TypeOf Children(i) Is Order Then
                Children.RemoveAt(i)
            Else
                i += 1
            End If
        End While
        _orders.Clear()
        ResetBars()
        InitAxis()
        RefreshAxisSize()
        LoadingHistory = True
        If Settings("DaysBack").Value <> 0 Then
            historicalDuration = TimeSpan.FromDays(Settings("DaysBack").Value)
            SetStatusText("loading...")
            DataStream.RequestHistoricalData(Now, historicalDuration, TickerID)
        Else
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
#End Region

#Region "On Screen Controls"
    Private axisTextBlockStyle As New Style

    Private priceCursorText As TextBlock, timeCursorText As Controls.Label, statusText As New Controls.TextBlock
    Private loadingProgressBar As New ProgressBar With {.Minimum = 0, .Maximum = 1, .Visibility = Windows.Visibility.Hidden}
    Private gridLines As New List(Of TrendLine)
    Private WithEvents priceAxis As New UIChartControl
    Private WithEvents timeAxis As New UIChartControl
    Private speedSlider As ChartSlider
    Private deflectionSlider As ChartSlider
    Private statusAxis As New UIChartControl
    Private initialOrderBoxLocation As Point

    Private orderBox As New UIChartControl
    Private btnBuyMarket As New Button With {.Content = "Buy", .Padding = New Thickness(6, 1, 6, 1), .Background = Brushes.Green, .Foreground = Brushes.White, .FontWeight = FontWeights.Bold, .ToolTip = "Press Ctrl to Activate"}
    Private btnBuyStop As New Button With {.Content = "Buy Stop", .Padding = New Thickness(3, 1, 3, 1), .Background = New SolidColorBrush(Color.FromArgb(255, 180, 180, 0)), .Foreground = Brushes.Black}
    Private btnBuyLimit As New Button With {.Content = "Buy Limit", .Padding = New Thickness(3, 1, 3, 1), .Background = Brushes.Green, .Foreground = Brushes.White}
    Private btnTransmit As New Button With {.Content = "Transmit", .Padding = New Thickness(3, 1, 3, 1), .Background = Brushes.LightGray, .Foreground = Brushes.Black}
    Private btnSellMarket As New Button With {.Content = "Sell", .Padding = New Thickness(6, 1, 6, 1), .Background = Brushes.DarkRed, .Foreground = Brushes.White, .FontWeight = FontWeights.Bold, .ToolTip = "Press Ctrl to Activate"}
    Private btnSellStop As New Button With {.Content = "Sell Stop", .Padding = New Thickness(3, 1, 3, 1), .Background = Brushes.Blue, .Foreground = Brushes.White}
    Private btnSellLimit As New Button With {.Content = "Sell Limit", .Padding = New Thickness(3, 1, 3, 1), .Background = Brushes.DarkRed, .Foreground = Brushes.White}
    Private btnCancel As New Button With {.Content = "Cancel", .Padding = New Thickness(3, 1, 3, 1), .Background = Brushes.DeepPink, .Foreground = Brushes.Black}
    Private btnTab As New Button With {.Content = "T" & vbNewLine & "A" & vbNewLine & "B", .Width = 30, .Background = Brushes.LightGray, .Foreground = Brushes.Black}
    Private btnQuantity As New Button With {.Content = "Quantity", .Background = Brushes.LightGray, .Width = 30, .Foreground = Brushes.Black, .FontWeight = FontWeights.Bold, .Tag = 0}

    Private Sub RefreshAllSize()
        RefreshAxisSize()
        RefreshRandomDataControlsLocation()
        'RefreshOrderButtonsLocation()
        RefreshOrderBarSize()
    End Sub
    Private Sub InitAll()
        InitAxis()
        InitGrid()
        InitOrderBox()
        InitRandomDataControls()
    End Sub
    Private Sub RefreshAxisSize()
        priceAxis.Top = 0
        statusAxis.Top = 0
        timeAxis.Left = 0
        statusAxis.Left = 0
        priceAxis.Width = Settings("PriceBarWidth").Value
        timeAxis.Height = Settings("TimeBarHeight").Value
        statusAxis.Height = Settings("InfoBarHeight").Value
        priceAxis.Height = mainCanvas.ActualHeight
        timeAxis.Top = mainCanvas.ActualHeight - Settings("TimeBarHeight").Value
        priceAxis.Left = mainCanvas.ActualWidth - Settings("PriceBarWidth").Value
        timeAxis.Width = mainCanvas.ActualWidth - Settings("PriceBarWidth").Value
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
    'Private Sub RefreshOrderButtonsLocation()
    '    If orderBar IsNot Nothing AndAlso orderBar.Content IsNot Nothing Then
    '        orderBar.RealWidth = Settings("PriceBarWidth").Value
    '        orderBar.RealHeight = orderBar.Content.ActualHeight
    '        orderBar.RealLeft = mainCanvas.ActualWidth - Settings("PriceBarWidth").Value ' Settings("OrderBarLocation").Value.X
    '        orderBar.RealTop = mainCanvas.ActualHeight / 2 - orderBar.RealHeight / 2 'Settings("OrderBarLocation").Value.Y
    '    End If
    'End Sub
    'Private Sub InitOrderButtons()
    '    Dim grd As New Grid
    '    grd.Width = Settings("PriceBarWidth").Value
    '    grd.Resources.MergedDictionaries.Add(New ResourceDictionary)
    '    grd.Resources.MergedDictionaries(0).Source = New Uri("Themes/OrderButtonStyle.xaml", UriKind.Relative)
    '    Dim BuyLimit As New Button
    '    Dim SellLimit As New Button
    '    'Dim BuyMarket As New Button
    '    'Dim SellMarket As New Button
    '    Dim btns() As Button = {BuyLimit, SellLimit}
    '    Dim i As Integer
    '    For Each item In btns
    '        item.HorizontalContentAlignment = Windows.HorizontalAlignment.Center
    '        item.Width = Settings("PriceBarWidth").Value
    '        item.Height = Settings("PriceBarWidth").Value
    '        item.HorizontalContentAlignment = Windows.HorizontalAlignment.Left
    '        item.FontSize += 1
    '        item.FontWeight = FontWeights.Bold
    '        item.Padding = New Thickness(10)
    '        grd.RowDefinitions.Add(New RowDefinition)
    '        grd.Children.Add(item)
    '        Grid.SetRow(item, i)
    '        i += 1
    '    Next
    '    BuyLimit.Content = "Buy" & vbCrLf & "Limit"
    '    SellLimit.Content = "Sell" & vbCrLf & "Limit"
    '    'BuyMarket.Content = "Buy Market"
    '    'SellMarket.Content = "Sell Market"
    '    BuyLimit.Background = Brushes.Green
    '    SellLimit.Background = Brushes.DarkRed
    '    'BuyMarket.Background = New SolidColorBrush(Color.FromArgb(255, 220, 220, 0)) ' Yellow
    '    'SellMarket.Background = New SolidColorBrush(Color.FromArgb(255, 0, 128, 255)) ' Blue
    '    'BuyMarket.Foreground = Brushes.Black

    '    AddHandler SellLimit.Click, Sub() If Mode = ClickMode.Normal And Settings("UseRandom").Value = False And BarIndex > 0 Then ChartCommands.CreateSellOrder.Execute(Nothing, ParentWindow)
    '    'AddHandler SellLimit.Click, Sub() If Mode = ClickMode.Normal Then ChartCommands.CreateSellOrder.Execute(Nothing, ParentWindow)
    '    AddHandler BuyLimit.Click, Sub() If Mode = ClickMode.Normal And Settings("UseRandom").Value = False And BarIndex > 0 Then ChartCommands.CreateBuyOrder.Execute(Nothing, ParentWindow)
    '    'AddHandler BuyLimit.Click, Sub() If Mode = ClickMode.Normal Then ChartCommands.CreateBuyOrder.Execute(Nothing, ParentWindow)


    '    'AddHandler BuyMarket.Click, Sub() If Mode = ClickMode.Normal And Settings("UseRandom").Value = False And BarIndex > 0 Then ChartCommands.CreateBuyMarketOrder.Execute(Nothing, ParentWindow)
    '    'AddHandler SellMarket.Click, Sub() If Mode = ClickMode.Normal And Settings("UseRandom").Value = False And BarIndex > 0 Then ChartCommands.CreateSellMarketOrder.Execute(Nothing, ParentWindow)
    '    grd.Height = i * (Settings("PriceBarWidth").Value + 12)
    '    orderBar.Content = grd
    '    orderBar.IsDraggable = False

    'End Sub
    Public Sub RefreshOrderBox()
        If IsLoaded And HasParent Then
            Dim SetState =
                Sub(btn As Button, enabled As Boolean)
                    If enabled Then
                        btn.IsEnabled = True
                    Else
                        btn.IsEnabled = False
                    End If
                End Sub
            If Keyboard.IsKeyDown(Key.LeftCtrl) Or Keyboard.IsKeyDown(Key.RightCtrl) Then
                SetState(btnBuyMarket, True)
                SetState(btnSellMarket, True)
            Else
                SetState(btnBuyMarket, False)
                SetState(btnSellMarket, False)
            End If
            'SetState(btnQuantity, False)
            SetState(btnCancel, False)
            SetState(btnTransmit, False)
            btnQuantity.Content = GetOrderQuantity()
            btnQuantity.Tag = GetOrderQuantity()
            For Each order In Orders
                If order.IsSelectable AndAlso order.IsSelected AndAlso order.OrderStatus <> OrderStatus.Filled Then
                    'SetState(btnQuantity, True)
                    SetState(btnCancel, True)
                    btnTransmit.Background = New SolidColorBrush(order.SelectedColor)
                    btnTransmit.Foreground = New SolidColorBrush(GetForegroundColor(order.SelectedColor))
                    SetState(btnTransmit, order.TransmitUpdateButtonVisible)
                    If order.IsUpdateNeeded Then
                        btnTransmit.Content = "Update"
                    Else
                        btnTransmit.Content = "Transmit"
                    End If
                    btnQuantity.Content = CStr(order.Quantity)
                End If
            Next
        End If
    End Sub
    Private Sub RefreshOrderBarSize()
        If orderBox IsNot Nothing And orderBox.Content IsNot Nothing Then
            orderBox.Width = 283
            orderBox.Height = 50
        End If
    End Sub
    Private Function GetOrderQuantity() As Integer
        Return If(btnQuantity.Tag = 0, Settings("DefaultOrderQuantity").Value, btnQuantity.Tag) 'If(btnQuantity.Tag = 0, If(IB.Contract(TickerID).SecurityType <> SecurityType.Stock Or Settings("UseRandom").Value, 1, 100), btnQuantity.Tag)
    End Function
    Private Sub InitOrderBox()
        orderBox = New UIChartControl
        orderBox.Left = initialOrderBoxLocation.X
        orderBox.Top = initialOrderBoxLocation.Y
        'orderBox.Scaleable = ScaleType.Fixed
        orderBox.IsDraggable = True
        Dim bd As New Border
        Dim grd As New Grid
        grd.Margin = New Thickness(0)
        grd.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Auto)})
        grd.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Auto)})
        grd.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Auto)})
        grd.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Auto)})
        grd.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Auto)})
        grd.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Auto)})
        grd.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Star)})
        grd.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Star)})

        grd.Resources.MergedDictionaries.Add(New ResourceDictionary)
        grd.Resources.MergedDictionaries(0).Source = New Uri("Themes/OrderButtonStyle.xaml", UriKind.Relative)
        bd.Child = grd
        bd.BorderBrush = New SolidColorBrush(Color.FromArgb(40, 255, 255, 255))
        bd.BorderThickness = New Thickness(0)
        bd.Background = Brushes.Transparent
        bd.CornerRadius = New CornerRadius(0)
        Dim removeMenuItem As New MenuItem With {.Header = "Hide"}
        AddHandler removeMenuItem.Click,
            Sub()
                mnuViewOrderBar.IsChecked = False
            End Sub
        bd.ContextMenu = New ContextMenu With {.ItemsSource = {removeMenuItem}}
        Dim buttons() As FrameworkElement =
        {
            btnQuantity, btnBuyMarket, btnBuyStop, btnBuyLimit, btnCancel,
                btnTab, btnSellMarket, btnSellStop, btnSellLimit, btnTransmit
        }

        Grid.SetColumn(btnBuyMarket, 0)

        Grid.SetColumn(btnSellStop, 1)

        Grid.SetColumn(btnBuyLimit, 2)

        Grid.SetColumn(btnCancel, 3)

        Grid.SetColumn(btnTab, 4)
        Grid.SetRowSpan(btnTab, 2)

        Grid.SetColumn(btnQuantity, 5)
        Grid.SetRowSpan(btnQuantity, 2)

        Grid.SetColumn(btnSellMarket, 0)
        Grid.SetRow(btnSellMarket, 1)

        Grid.SetColumn(btnBuyStop, 1)
        Grid.SetRow(btnBuyStop, 1)

        Grid.SetColumn(btnSellLimit, 2)
        Grid.SetRow(btnSellLimit, 1)

        Grid.SetColumn(btnTransmit, 3)
        Grid.SetRow(btnTransmit, 1)

        For Each button In buttons
            If button IsNot Nothing Then
                button.Margin = New Thickness(2)
                If button.Parent Is Nothing Then grd.Children.Add(button)
            End If
        Next


        AddHandler btnTab.Click,
            Sub()
                Dim orders As New List(Of Order)
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
        AddHandler btnBuyMarket.Click,
            Sub()
                Dim order As New Order(Me) With {.Color = Colors.DarkGreen, .Quantity = GetOrderQuantity(), .SelectedColor = Colors.Green, .ButtonHoverColor = Colors.LightGreen,
                                        .Price = Price, .OrderType = OrderType.Market, .OrderAction = ActionSide.Buy}
                Children.Add(order)
                order.IsSelected = True
                order.Transmit()
                btnQuantity.Tag = 0
            End Sub
        AddHandler btnSellMarket.Click,
            Sub()
                Dim order As New Order(Me) With {.Color = Colors.DarkRed, .Quantity = GetOrderQuantity(), .SelectedColor = Colors.Red, .ButtonHoverColor = Colors.Pink,
                                       .Price = Price, .OrderType = OrderType.Market, .OrderAction = ActionSide.Sell}
                Children.Add(order)
                order.IsSelected = True
                order.Transmit()
                btnQuantity.Tag = 0
            End Sub
        AddHandler btnQuantity.Click,
            Sub()
                If SelectedOrder IsNot Nothing AndAlso SelectedOrder.OrderType <> OrderType.Market AndAlso SelectedOrder.OrderStatus <> OrderStatus.Filled Then
                    SelectedOrder.ChooseQuantity()
                Else
                    ShowQuantityMenu(GetOrderQuantity, Settings("DefaultOrderQuantity").Value,
                        Sub(newQuantity As Integer)
                            btnQuantity.Content = newQuantity
                            btnQuantity.Tag = newQuantity
                        End Sub)
                End If
            End Sub
        AddHandler btnCancel.Click,
            Sub()
                If SelectedOrder IsNot Nothing Then SelectedOrder.Cancel()
            End Sub
        AddHandler btnTransmit.Click,
            Sub()
                If btnTransmit.Content = "Transmit" Then
                    If SelectedOrder IsNot Nothing Then SelectedOrder.Transmit()
                Else
                    If SelectedOrder IsNot Nothing Then SelectedOrder.Resend()
                End If
            End Sub

        AddHandler btnBuyStop.Click,
            Sub()
                Dim order As New Order(Me) With {.Color = Color.FromArgb(255, 180, 180, 0), .SelectedColor = Colors.Yellow, .ButtonHoverColor = Colors.LightYellow,
                                       .Price = Price, .Quantity = GetOrderQuantity(), .OrderType = OrderType.Stop, .OrderAction = ActionSide.Buy}
                Children.Add(order)
                order.IsSelected = True
                btnQuantity.Tag = 0
            End Sub
        AddHandler btnSellStop.Click,
            Sub()
                Dim order As New Order(Me) With {.Color = Colors.DarkBlue, .SelectedColor = Colors.Blue, .ButtonHoverColor = Colors.LightBlue,
                                      .Price = Price, .Quantity = GetOrderQuantity(), .OrderType = OrderType.Stop, .OrderAction = ActionSide.Sell}
                Children.Add(order)
                order.IsSelected = True
                btnQuantity.Tag = 0
            End Sub
        AddHandler btnBuyLimit.Click,
            Sub()
                Dim order As New Order(Me) With {.Color = Colors.DarkGreen, .SelectedColor = Colors.Green, .ButtonHoverColor = Colors.LightGreen,
                                      .Price = Price, .Quantity = GetOrderQuantity(), .OrderType = OrderType.Limit, .OrderAction = ActionSide.Buy}
                Children.Add(order)
                order.IsSelected = True
                btnQuantity.Tag = 0
            End Sub
        AddHandler btnSellLimit.Click,
            Sub()
                Dim order As New Order(Me) With {.Color = Colors.DarkRed, .SelectedColor = Colors.Red, .ButtonHoverColor = Colors.Pink,
                                         .Price = Price, .Quantity = GetOrderQuantity(), .OrderType = OrderType.Limit, .OrderAction = ActionSide.Sell}
                Children.Add(order)
                order.IsSelected = True
                btnQuantity.Tag = 0
            End Sub
        orderBox.Content = bd
        RefreshOrderBox()
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
    End Sub
    Private Sub InitAxis()
        Dim fontSize As Double = 14
        If axisTextBlockStyle.Setters.Count = 0 Then
            axisTextBlockStyle.Setters.Add(New Setter(TextBlock.FontFamilyProperty, (New FontFamilyConverter).ConvertFromString("Global Sans Serif")))
            axisTextBlockStyle.Setters.Add(New Setter(TextBlock.FontSizeProperty, fontSize))
            axisTextBlockStyle.Setters.Add(New Setter(TextBlock.WidthProperty, Settings("PriceBarWidth").Value))
            axisTextBlockStyle.Setters.Add(New Setter(TextBlock.HeightProperty, fontSize))
        End If

        If statusAxis.Content Is Nothing Then
            statusAxis.Content = New Grid
            statusAxis.ContentChildren.Add(loadingProgressBar)
            statusAxis.ContentChildren.Add(statusText)
        End If

        priceAxis.Content = New Canvas
        timeAxis.Content = New Canvas


        Children.Remove(priceAxis)
        Children.Add(priceAxis)
        Children.Remove(timeAxis)
        Children.Add(timeAxis)
        Children.Remove(statusAxis)
        Children.Add(statusAxis)
    End Sub
    Private Sub SetControlsOnTop(Optional ByVal parameter As Object = Nothing)
        If parameter IsNot priceAxis And parameter IsNot timeAxis And parameter IsNot statusAxis And parameter IsNot orderBox Then
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
            'If Settings("UseRandom").Value = False And BarIndex > 0 And mnuViewOrderBar.IsChecked Then Children.Add(orderBar)
        End If
    End Sub
    Private Sub RefreshAxisAndGrid()
        If IsLoaded And BarIndex > 0 Then
            timeAxis.ContentChildren.Clear()
            priceAxis.ContentChildren.Clear()
            If BarIndex > 1 Then
                Dim topNum As Double = RoundTo(Bounds.Y1, Settings("PriceBarTextInterval").Value)
                Dim botNum As Double = RoundTo(Bounds.Y2, Settings("PriceBarTextInterval").Value)
                Dim num As Integer = (botNum - topNum + Settings("PriceBarTextInterval").Value) / Settings("PriceBarTextInterval").Value
                If Settings("ShowGrid").Value Then
                    Dim gridLineIndex As Integer
                    Dim gridLineCount As Integer = Settings("GridLineCount").Value - 1
                    Dim interval As Double = Settings("PriceBarTextInterval").Value
                    For price As Double = RoundTo(-C - gridLineCount * interval / 2, interval) To RoundTo(gridLineCount * interval / 2 - C, interval) Step interval
                        gridLines(gridLineIndex).StartPoint = New Point(GetVirtualFromRealX(ActualWidth - Settings("GridWidth").Value * ActualWidth - Settings("PriceBarWidth").Value), price)
                        gridLines(gridLineIndex).EndPoint = New Point(GetVirtualFromRealX(ActualWidth), price)
                        gridLines(gridLineIndex).Pen.Brush = New SolidColorBrush(Settings("Grid").Value)
                        gridLines(gridLineIndex).RefreshVisual()
                        gridLineIndex += 1
                    Next
                End If
                For price As Double = topNum - Settings("PriceBarTextInterval").Value To botNum Step Settings("PriceBarTextInterval").Value
                    Dim txt As New Controls.TextBlock
                    txt.Text = FormatNumber(-Math.Round(price, Settings("DecimalPlaces").Value), Settings("DecimalPlaces").Value, TriState.True, TriState.False).Replace(",", "")
                    txt.FontSize = 14
                    txt.Height = txt.FontSize + 2
                    txt.Foreground = New SolidColorBrush(Settings("Axis Text Foreground").Value)
                    txt.Background = New SolidColorBrush(Settings("Axis Text Background").Value)
                    priceAxis.ContentChildren.Add(txt)
                    Canvas.SetTop(txt, GetRealFromVirtual(New Point(0, price)).Y - txt.Height / 2)
                Next

                For I As Integer = RoundTo(Max(0, Bounds.X1), Settings("TimeBarTextInterval").Value) + 1 To RoundTo(Min(BarIndex, Bounds.X2), Settings("TimeBarTextInterval").Value) Step Settings("TimeBarTextInterval").Value
                    'For I As Integer = Max(Min(BarIndex, Bounds.X1 + 1), 10) To Min(BarIndex, Bounds.X2) - 1 Step Settings("TimeBarTextInterval").Value
                    Dim txt As New TextBlock
                    txt.Text = bars(I - 1).Data.Date.ToShortTimeString
                    txt.FontSize = 14
                    txt.Height = txt.FontSize + 2
                    txt.Foreground = New SolidColorBrush(Settings("Axis Text Foreground").Value)
                    txt.Background = New SolidColorBrush(Settings("Axis Text Background").Value)
                    timeAxis.ContentChildren.Add(txt)
                    Canvas.SetTop(txt, 0)
                    Canvas.SetLeft(txt, GetRealFromVirtualX(I))
                    AddHandler txt.Loaded, AddressOf TimeTextLoaded
                Next
            End If
            If Mode = ClickMode.Normal Then Window_MouseMove(Me, New MouseEventArgs(Mouse.PrimaryDevice, My.Computer.Clock.TickCount))
            DrawCursorTexts()
            If priceAxis.ContentChildren.Contains(priceText) Then priceAxis.ContentChildren.Remove(priceText)
            priceText = New TextBlock
            priceAxis.ContentChildren.Add(priceText)
            priceText.Style = axisTextBlockStyle
            priceText.Foreground = New SolidColorBrush(Settings("Price Text Foreground").Value)
            priceText.Background = New SolidColorBrush(Settings("Price Text Background").Value)
            priceText.Text = FormatNumber(Price, Settings("DecimalPlaces").Value, TriState.True, TriState.False).Replace(",", "")
            priceText.SetValue(Canvas.TopProperty, GetRealFromVirtualY(-Price) - priceText.Height / 2)
            priceText.SetValue(Canvas.LeftProperty, 0.0)
        End If
        statusText.FontSize = 15
        SetStatusTextLocation()
        statusText.Foreground = New SolidColorBrush(Settings("Status Text Foreground").Value)

    End Sub
    Private Sub TimeTextLoaded(ByVal sender As Object, ByVal e As EventArgs)
        Canvas.SetLeft(sender, Canvas.GetLeft(sender) - sender.ActualWidth / 2)
        If sender Is timeCursorText Then
            sender.Width = sender.ActualWidth + 85
            Canvas.SetLeft(sender, Canvas.GetLeft(sender) - sender.Width / 2 - 10)
        End If
    End Sub
    Private Function FormattedPriceString() As String
        Return FormatNumber(Math.Round(Price, 2), 2, TriState.True, TriState.False)
    End Function
    Private Sub DrawCursorTexts()
        If MouseDown = MouseDownType.CapturedOnMe And Mode = ClickMode.Normal And Mouse.LeftButton = MouseButtonState.Pressed Then
            Dim virtualClickPoint As Point = GetVirtualFromReal(Mouse.GetPosition(mainCanvas))
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
                priceCursorText.Text = FormatNumber(Math.Round(-RoundTo(virtualClickPoint.Y, GetMinTick), Settings("DecimalPlaces").Value), Settings("DecimalPlaces").Value, TriState.True, TriState.False).Replace(",", "")
                priceCursorText.SetValue(Canvas.TopProperty, GetRealFromVirtualY(RoundTo(GetVirtualFromRealY(Mouse.GetPosition(priceAxis).Y), GetMinTick)) - priceCursorText.Height / 2)
            End If
            If virtualClickPoint.X >= 1 And virtualClickPoint.X <= BarIndex Then
                timeCursorText.Style = axisTextBlockStyle
                timeCursorText.Foreground = New SolidColorBrush(Settings("Cursor Text Foreground").Value)
                timeCursorText.Background = New SolidColorBrush(Settings("Cursor Text Background").Value)
                timeCursorText.Content = bars(virtualClickPoint.X - 1).Data.Date.ToShortDateString & ", " & bars(virtualClickPoint.X - 1).Data.Date.ToShortTimeString
                timeAxis.ContentChildren.Add(timeCursorText)
                timeCursorText.SetValue(Canvas.LeftProperty, GetRealFromVirtualX(RoundTo(GetVirtualFromRealX(Mouse.GetPosition(timeAxis).X), 1)))
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
    Private timeAxisClickMarginVirtualWidth As Double
    Private Sub timeAxis_PreviewMouseLeftButtonDown(ByVal sender As Object, ByVal e As System.Windows.Input.MouseButtonEventArgs) Handles timeAxis.PreviewMouseLeftButtonDown
        originalMode = Mode
        Mode = ClickMode.TimeAxisDrag
        timeAxisClickMarginVirtualWidth = GetVirtualFromRealWidth(Settings("XMargin").Value * mainCanvas.ActualWidth + Settings("PriceBarWidth").Value)
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
            If TypeOf e.Item Is Order Then _orders.Add(e.Item)
            If Not LoadingHistory And analysisTechniquesLoading = False Then SetControlsOnTop(e.Item)
        End If
    End Sub
    Private Sub _children_ChildRemoved(ByVal sender As Object, ByVal e As ItemChangedEventArgs(Of IChartObject)) Handles _children.ItemRemoved
        If Not TypeOf e.Item Is IChartObject Then
            e.Cancel = True
        Else
            If TypeOf e.Item Is Order Then _orders.Remove(e.Item)
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

    Private _orders As New List(Of Order)
    Public ReadOnly Property Orders As ReadOnlyCollection(Of Order)
        Get
            'Dim list As New List(Of Order)
            'For Each item In Children
            '    If TypeOf item Is Order Then list.Add(item)
            'Next
            Return New ReadOnlyCollection(Of Order)(_orders)
        End Get
    End Property
    Public ReadOnly Property SelectedOrder As Order
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

    Dim WithEvents mainCanvas As New DrawingVisualCanvas

    Dim rects(7) As Shapes.Rectangle
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
                    col.A = 100
                    SetBorderColor(col)
                    For Each item As IChartObject In Children
                        If TypeOf item Is ISelectable AndAlso CType(item, ISelectable).IsSelectable AndAlso CType(item, ISelectable).IsSelected Then CType(item, ISelectable).IsSelected = False
                    Next
                End If
            End If
        End Set
    End Property

    Private Sub SetBorderColor(ByVal color As Color)
        For i = 0 To 7
            rects(i).Fill = New SolidColorBrush(color)
        Next
    End Sub

    Public Property IsSelectable As Boolean = True Implements ISelectable.IsSelectable

    Private Sub InitBorder()
        MyBase.Background = Brushes.Transparent
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


        'Dim bd As New Border
        'bd.Background = Brushes.Transparent
        'bd.CornerRadius = New CornerRadius(3)
        'Grid.SetRowSpan(bd, 3)
        'Grid.SetColumnSpan(bd, 3)
        'MyBase.Children.Add(bd)
        'bd.ClipToBounds = True
        'mainCanvas.Background = Brushes.Black
        'mainCanvas.ClipToBounds = True
        Grid.SetRow(mainCanvas, 1)
        Grid.SetColumn(mainCanvas, 1)
        MyBase.Children.Add(mainCanvas)

        Dim col As Color = Colors.LightGray
        col.A = 100
        For i = 0 To 7
            rects(i) = New Shapes.Rectangle
            col = Colors.Red
            col.A = 100
            col = Colors.Transparent
            rects(i).Fill = New SolidColorBrush(col)
        Next

        Grid.SetRow(rects(0), 0)
        Grid.SetColumn(rects(0), 0)
        Grid.SetRow(rects(1), 0)
        Grid.SetColumn(rects(1), 1)
        Grid.SetRow(rects(2), 0)
        Grid.SetColumn(rects(2), 2)
        Grid.SetRow(rects(3), 1)
        Grid.SetColumn(rects(3), 0)
        Grid.SetRow(rects(4), 1)
        Grid.SetColumn(rects(4), 2)
        Grid.SetRow(rects(5), 2)
        Grid.SetColumn(rects(5), 0)
        Grid.SetRow(rects(6), 2)
        Grid.SetColumn(rects(6), 1)
        Grid.SetRow(rects(7), 2)
        Grid.SetColumn(rects(7), 2)

        Dim innerClickAreaWidth As Double = 3 '6 / 3.3
        Dim outerClickAreaWidth As Double = 4

        rects(0).Margin = New Thickness(-outerClickAreaWidth, -outerClickAreaWidth, -innerClickAreaWidth, -innerClickAreaWidth)
        rects(1).Margin = New Thickness(innerClickAreaWidth, -outerClickAreaWidth, innerClickAreaWidth, -innerClickAreaWidth)
        rects(2).Margin = New Thickness(-innerClickAreaWidth, -outerClickAreaWidth, -outerClickAreaWidth, -innerClickAreaWidth)
        rects(3).Margin = New Thickness(-outerClickAreaWidth, innerClickAreaWidth, -innerClickAreaWidth, innerClickAreaWidth)
        rects(4).Margin = New Thickness(-innerClickAreaWidth, innerClickAreaWidth, -outerClickAreaWidth, innerClickAreaWidth)
        rects(5).Margin = New Thickness(-outerClickAreaWidth, -innerClickAreaWidth, -innerClickAreaWidth, -outerClickAreaWidth)
        rects(6).Margin = New Thickness(innerClickAreaWidth, -innerClickAreaWidth, innerClickAreaWidth, -outerClickAreaWidth)
        rects(7).Margin = New Thickness(-innerClickAreaWidth, -innerClickAreaWidth, -outerClickAreaWidth, -outerClickAreaWidth)

        rects(0).Cursor = Cursors.SizeNWSE
        rects(1).Cursor = Cursors.SizeAll
        rects(2).Cursor = Cursors.SizeNESW
        rects(3).Cursor = Cursors.SizeWE
        rects(4).Cursor = Cursors.SizeWE
        rects(5).Cursor = Cursors.SizeNESW
        rects(6).Cursor = Cursors.SizeNS
        rects(7).Cursor = Cursors.SizeNWSE

        For i = 0 To 7
            MyBase.Children.Add(rects(i))
        Next
        IsSelected = False
        borderLoading = False
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
        rects(1).ContextMenu = contextMenu
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

    Dim borderClickPoint As Point, borderClickSize As Size, borderClickMePos As Point, borderClickItem As Rectangle, borderPress As Boolean, prevCur As Cursor
    Private Sub Border_MouseMove(ByVal args As MouseEventArgs)
        If Settings("Maximized").Value Then Exit Sub
        Dim index As Integer = Array.IndexOf(rects, borderClickItem)
        If Not borderPress Then index = -1
        Dim dif As Point = borderClickPoint - args.GetPosition(Parent)
        Dim minimumSize As Size = New Size(MinWidth, MinHeight) 'New Size(100, 100)
        Select Case index
            Case 0
                If borderClickSize.Width + dif.X > minimumSize.Width Then
                    LocationX = borderClickMePos.X - dif.X
                    SizeWidth = borderClickSize.Width + dif.X
                End If
                If borderClickSize.Height + dif.Y > minimumSize.Height Then
                    LocationY = borderClickMePos.Y - dif.Y
                    SizeHeight = borderClickSize.Height + dif.Y
                End If
            Case 1
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
            Case 2
                If borderClickSize.Height + dif.Y > minimumSize.Height Then
                    LocationY = borderClickMePos.Y - dif.Y
                    SizeHeight = borderClickSize.Height + dif.Y
                End If
                If borderClickSize.Width - dif.X > minimumSize.Width Then SizeWidth = borderClickSize.Width - dif.X
            Case 3
                If borderClickSize.Width + dif.X > minimumSize.Width Then
                    LocationX = borderClickMePos.X - dif.X
                    SizeWidth = borderClickSize.Width + dif.X
                End If
            Case 4
                If borderClickSize.Width - dif.X > minimumSize.Width Then SizeWidth = borderClickSize.Width - dif.X
            Case 5
                If borderClickSize.Width + dif.X > minimumSize.Width Then
                    LocationX = borderClickMePos.X - dif.X
                    SizeWidth = borderClickSize.Width + dif.X
                End If
                If borderClickSize.Height - dif.Y > minimumSize.Height Then SizeHeight = borderClickSize.Height - dif.Y
            Case 6
                If borderClickSize.Height - dif.Y > minimumSize.Height Then SizeHeight = borderClickSize.Height - dif.Y
            Case 7
                If borderClickSize.Height - dif.Y > minimumSize.Height Then SizeHeight = borderClickSize.Height - dif.Y
                If borderClickSize.Width - dif.X > minimumSize.Width Then SizeWidth = borderClickSize.Width - dif.X
        End Select

        If borderClickMePos.Y - dif.Y <= 0 Then
            LocationY = 1
            'ElseIf borderClickMePos.Y - dif.Y + SizeHeight >= Parent.ActualHeight Then
            '    LocationY = Parent.ActualHeight - SizeHeight - 1
        End If
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
        If TypeOf args.Source Is Shapes.Rectangle AndAlso rects.Contains(args.Source) Then
            ChangeCursor(CType(args.Source, Shapes.Rectangle).Cursor)
            borderClickItem = args.Source
            borderClickPoint = args.GetPosition(Parent)
            borderClickSize = Size
            borderClickMePos = Location
            borderPress = True
        Else
            borderClickItem = Nothing
            borderPress = False
            DesktopWindow.Cursor = Nothing
        End If
    End Sub
    Private Sub Border_MouseButtonDown(ByVal sender As Object, ByVal args As MouseButtonEventArgs) Handles Me.PreviewMouseDown
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
    End Sub
    Private Sub Border_MouseDoubleClick(ByVal sender As Object, ByVal args As MouseButtonEventArgs)
        If GetIsParent(args.OriginalSource, Me) Then
            If Array.IndexOf(rects, borderClickItem) = 1 Then
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
        Settings.AddSetting(New Setting("Price Text Background", Colors.LightGray))
        Settings.AddSetting(New Setting("Price Text Foreground", Colors.Black))
        Settings.AddSetting(New Setting("Cursor Text Background", Colors.LightGray))
        Settings.AddSetting(New Setting("Cursor Text Foreground", Colors.White))
        Settings.AddSetting(New Setting("Axis Text Background", Colors.Transparent))
        Settings.AddSetting(New Setting("Axis Text Foreground", Colors.White))
        Settings.AddSetting(New Setting("Status Text Foreground", Colors.White))
        Settings.AddSetting(New Setting("UseRandom", False))
        Settings.AddSetting(New Setting("RVValue", 1.0))
        Settings.AddSetting(New Setting("RangeValue", 1.0))
        Settings.AddSetting(New Setting("XMargin", 0.1))
        Settings.AddSetting(New Setting("YMargin", 0.1))
        Settings.AddSetting(New Setting("PriceBarWidth", 60.0))
        Settings.AddSetting(New Setting("TimeBarHeight", 30.0))
        Settings.AddSetting(New Setting("InfoBarHeight", 30.0))
        Settings.AddSetting(New Setting("RestrictMovement", True))
        Settings.AddSetting(New Setting("PriceBarTextInterval", 8.0))
        Settings.AddSetting(New Setting("TimeBarTextInterval", 5.0))
        Settings.AddSetting(New Setting("BarSize", GetType(BarSize)))
        Settings.AddSetting(New Setting("IB.Contract.ComboLegsDescription", GetType(String)))
        Settings.AddSetting(New Setting("IB.Contract.ContractID", GetType(Integer)))
        Settings.AddSetting(New Setting("IB.Contract.Currency", GetType(String)))
        Settings.AddSetting(New Setting("IB.Contract.Exchange", GetType(String)))
        Settings.AddSetting(New Setting("IB.Contract.Expiry", GetType(String)))
        Settings.AddSetting(New Setting("IB.Contract.IncludeExpired", GetType(Boolean)))
        Settings.AddSetting(New Setting("IB.Contract.LocalSymbol", GetType(String)))
        Settings.AddSetting(New Setting("IB.Contract.Multiplier", GetType(String)))
        Settings.AddSetting(New Setting("IB.Contract.PrimaryExchange", GetType(String)))
        Settings.AddSetting(New Setting("IB.Contract.Right", GetType(RightType)))
        Settings.AddSetting(New Setting("IB.Contract.SecId", GetType(String)))
        Settings.AddSetting(New Setting("IB.Contract.SecIdType", GetType(SecurityIdType)))
        Settings.AddSetting(New Setting("IB.Contract.SecurityType", GetType(SecurityType)))
        Settings.AddSetting(New Setting("IB.Contract.Strike", GetType(Double)))
        Settings.AddSetting(New Setting("IB.Contract.Symbol", GetType(String)))
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
        Settings.AddSetting(New Setting("GridWidth", 0.3))
        Settings.AddSetting(New Setting("DisplayOpenTick", False))
        Settings.AddSetting(New Setting("DisplayCloseTick", True))
        Settings.AddSetting(New Setting("DaysBack", 1.0))
        Settings.AddSetting(New Setting("Maximized", False))
        Settings.AddSetting(New Setting("WorkOffline", False))
        Settings.AddSetting(New Setting("AutoScale", True))
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
        RAND.BarSize(TickerID) = Settings("BarSize").Load(workspaceFile, name, RAND.BarSize(TickerID))

        IB.BarSize(TickerID) = Settings("BarSize").Load(workspaceFile, name, IB.BarSize(TickerID))
        IB.Contract(TickerID).ComboLegsDescription = Settings("IB.Contract.ComboLegsDescription").Load(workspaceFile, name, IB.Contract(TickerID).ComboLegsDescription)
        IB.Contract(TickerID).ContractId = Settings("IB.Contract.ContractId").Load(workspaceFile, name, IB.Contract(TickerID).ContractId)
        IB.Contract(TickerID).Currency = Settings("IB.Contract.Currency").Load(workspaceFile, name, IB.Contract(TickerID).Currency)
        IB.Contract(TickerID).Exchange = Settings("IB.Contract.Exchange").Load(workspaceFile, name, IB.Contract(TickerID).Exchange)
        IB.Contract(TickerID).Expiry = Settings("IB.Contract.Expiry").Load(workspaceFile, name, IB.Contract(TickerID).Expiry)
        IB.Contract(TickerID).IncludeExpired = Settings("IB.Contract.IncludeExpired").Load(workspaceFile, name, IB.Contract(TickerID).IncludeExpired)
        IB.Contract(TickerID).LocalSymbol = Settings("IB.Contract.LocalSymbol").Load(workspaceFile, name, IB.Contract(TickerID).LocalSymbol)
        IB.Contract(TickerID).Multiplier = Settings("IB.Contract.Multiplier").Load(workspaceFile, name, IB.Contract(TickerID).Multiplier)
        IB.Contract(TickerID).PrimaryExchange = Settings("IB.Contract.PrimaryExchange").Load(workspaceFile, name, IB.Contract(TickerID).PrimaryExchange)
        IB.Contract(TickerID).Right = Settings("IB.Contract.Right").Load(workspaceFile, name, IB.Contract(TickerID).Right)
        IB.Contract(TickerID).SecurityType = Settings("IB.Contract.SecurityType").Load(workspaceFile, name, IB.Contract(TickerID).SecurityType)
        IB.Contract(TickerID).SecIdType = Settings("IB.Contract.SecIdType").Load(workspaceFile, name, IB.Contract(TickerID).SecIdType)
        IB.Contract(TickerID).Strike = Settings("IB.Contract.Strike").Load(workspaceFile, name, IB.Contract(TickerID).Strike)
        IB.Contract(TickerID).Symbol = Settings("IB.Contract.Symbol").Load(workspaceFile, name, IB.Contract(TickerID).Symbol)
        IB.WhatToShow(TickerID) = Settings("IB.WhatToShow").Load(workspaceFile, name, IB.WhatToShow(TickerID))
        IB.MinTick(TickerID) = Settings("IB.MinTick").Load(workspaceFile, name, IB.MinTick(TickerID))
        IB.UseRegularTradingHours(TickerID) = Settings("IB.UseRegularTradingHours").Load(workspaceFile, name, IB.UseRegularTradingHours(TickerID))

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
        LoadDrawingObjects()
    End Sub
    Public Sub SaveSettings()
        Dim workspaceFile As String = Parent.FilePath
        Dim name As String = GetChartName()
        For Each item In Settings.Settings
            If item.HasValue Then item.Save(workspaceFile, name)
        Next
        Settings("BarSize").Save(workspaceFile, name, DataStream.BarSize(TickerID))

        Settings("IB.Contract.ComboLegsDescription").Save(workspaceFile, name, IB.Contract(TickerID).ComboLegsDescription)
        Settings("IB.Contract.ContractID").Save(workspaceFile, name, IB.Contract(TickerID).ContractId)
        Settings("IB.Contract.Currency").Save(workspaceFile, name, IB.Contract(TickerID).Currency)
        Settings("IB.Contract.Exchange").Save(workspaceFile, name, IB.Contract(TickerID).Exchange)
        Settings("IB.Contract.Expiry").Save(workspaceFile, name, IB.Contract(TickerID).Expiry)
        Settings("IB.Contract.IncludeExpired").Save(workspaceFile, name, IB.Contract(TickerID).IncludeExpired)
        Settings("IB.Contract.LocalSymbol").Save(workspaceFile, name, IB.Contract(TickerID).LocalSymbol)
        Settings("IB.Contract.Multiplier").Save(workspaceFile, name, IB.Contract(TickerID).Multiplier)
        Settings("IB.Contract.PrimaryExchange").Save(workspaceFile, name, IB.Contract(TickerID).PrimaryExchange)
        Settings("IB.Contract.Right").Save(workspaceFile, name, IB.Contract(TickerID).Right)
        Settings("IB.Contract.SecId").Save(workspaceFile, name, IB.Contract(TickerID).SecId)
        Settings("IB.Contract.SecIdType").Save(workspaceFile, name, IB.Contract(TickerID).SecIdType)
        Settings("IB.Contract.SecurityType").Save(workspaceFile, name, IB.Contract(TickerID).SecurityType)
        Settings("IB.Contract.Strike").Save(workspaceFile, name, IB.Contract(TickerID).Strike)
        Settings("IB.Contract.Symbol").Save(workspaceFile, name, IB.Contract(TickerID).Symbol)
        Settings("IB.UseRegularTradingHours").Save(workspaceFile, name, IB.UseRegularTradingHours(TickerID))
        Settings("IB.MinTick").Save(workspaceFile, name, IB.MinTick(TickerID))
        Settings("IB.WhatToShow").Save(workspaceFile, name, IB.WhatToShow(TickerID))

        Settings("RAND.RandomMaxMove").Save(workspaceFile, name, RAND.RandomMaxMove)
        Settings("RAND.RandomMinMove").Save(workspaceFile, name, RAND.RandomMinMove)
        Settings("RAND.RandomPrice").Save(workspaceFile, name, RAND.RandomPrice)
        Settings("RAND.RandomSpeed").Save(workspaceFile, name, RAND.RandomSpeed)
        Settings("ChartRect").Save(workspaceFile, name, New Rect(Location, New Size(SizeWidth, SizeHeight)))
        Settings("Bounds").Save(workspaceFile, name, Bounds)
        Settings("ZIndex").Save(workspaceFile, name, Canvas.GetZIndex(Me))
        Settings("IsSelected").Save(workspaceFile, name, IsSelected)
        If Not Double.IsNaN(orderBox.Left) And Not Double.IsNaN(orderBox.Top) Then Settings("OrderBarLocation").Save(workspaceFile, name, New Point(orderBox.Left, orderBox.Top))
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

        For i = 1 To 2
            If SettingExists(GLOBAL_CONFIG_FILE, "PresetLineStyle" & i & ".Pen.Brush") Then
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
                trendLineStyle.ExtendRight = Boolean.Parse(ReadSetting(GLOBAL_CONFIG_FILE, "PresetLineStyle" & i & ".ExtendRight", "False"))
                trendLineStyle.ExtendLeft = Boolean.Parse(ReadSetting(GLOBAL_CONFIG_FILE, "PresetLineStyle" & i & ".ExtendLeft", "False"))
                PresetLineStyles.Add(trendLineStyle)
            Else
                PresetLineStyles.Add(Nothing)
            End If
            If SettingExists(GLOBAL_CONFIG_FILE, "PresetLabelStyle" & i & ".Font.Brush") Then
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
            Else
                PresetLabelStyles.Add(Nothing)
            End If
            If SettingExists(GLOBAL_CONFIG_FILE, "PresetArrowStyle" & i & ".Pen.Brush") Then
                Dim arrowStyle As New ArrowStyle
                arrowStyle.Pen = New Pen((New BrushConverter).ConvertFromString(ReadSetting(GLOBAL_CONFIG_FILE, "PresetArrowStyle" & i & ".Pen.Brush", "Red")), ReadSetting(GLOBAL_CONFIG_FILE, "PresetArrowStyle" & i & ".Pen.Thickness", "1"))
                arrowStyle.IsFlipped = ReadSetting(GLOBAL_CONFIG_FILE, "PresetArrowStyle" & i & ".IsFlipped", "False")
                arrowStyle.Width = ReadSetting(GLOBAL_CONFIG_FILE, "PresetArrowStyle" & i & ".Width", "12")
                arrowStyle.Height = ReadSetting(GLOBAL_CONFIG_FILE, "PresetArrowStyle" & i & ".Height", "4")
                PresetArrowStyles.Add(arrowStyle)
            Else
                PresetArrowStyles.Add(Nothing)
            End If
        Next

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
                priceAxis.Content.Background = Brushes.Black
                timeAxis.Content.Background = Brushes.Black
                statusAxis.Background = Brushes.Black
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
            Case "StatusTextLocation"

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
                For Each item In bars
                    item.IsSelectable = My.Application.ShowMousePopups
                Next
        End Select
    End Sub


    Private Sub LoadDrawingObjects()
        Dim name As String = GetChartName()
        Dim indexes As New Dictionary(Of String, Integer)
        indexes.Add("TrendLine", ReadSetting(Parent.FilePath, name & ".TrendLine", 0))
        indexes.Add("Label", ReadSetting(Parent.FilePath, name & ".Label", 0))
        indexes.Add("Arrow", ReadSetting(Parent.FilePath, name & ".Arrow", 0))
        For Each item In indexes
            For i = 1 To item.Value
                Dim obj As Object = Nothing
                If item.Key = "TrendLine" Then obj = New TrendLine
                If item.Key = "Label" Then obj = New Label
                If item.Key = "Arrow" Then obj = New Arrow
                Dim str As String = ReadSetting(Parent.FilePath, name & "." & item.Key & i, "")
                If str <> "" Then
                    CType(obj, ISerializable).Deserialize(str)
                    Children.Add(obj)
                End If
            Next
        Next
    End Sub
    Private Sub SaveDrawingObjects()
        Dim name As String = GetChartName()
        Dim indexes As New Dictionary(Of String, Integer)
        For Each child In Children
            If TypeOf child Is ChartDrawingVisual AndAlso TypeOf child Is ISerializable AndAlso Not TypeOf child Is Bar AndAlso child.AnalysisTechnique Is Nothing Then
                Dim vis As ChartDrawingVisual = child
                Dim typeName As String = vis.GetType.Name
                If Not indexes.ContainsKey(typeName) Then indexes.Add(typeName, 1)
                WriteSetting(Parent.FilePath, name & "." & typeName & indexes(typeName).ToString, CType(vis, ISerializable).Serialize)
                indexes(typeName) += 1
            End If
        Next
        Dim typeNames As New List(Of String)
        For Each item In indexes
            typeNames.Add(item.Key)
            WriteSetting(Parent.FilePath, name & "." & item.Key, item.Value.ToString)
        Next
        'WriteSetting(Parent.FilePath, name & ".UserObjectTypes", Join(typeNames.ToArray, ","))
    End Sub

    Public Property PresetLineStyles As New List(Of TrendLineStyle)
    Public Property PresetLabelStyles As New List(Of LabelStyle)
    Public Property PresetArrowStyles As New List(Of ArrowStyle)
    Public Property Settings As New SettingList
    Public Property TickerID As Integer

#Region "ReadOnly and complex properties"

    Public ReadOnly Property VirtualRightMarginLocation As Double
        Get
            Return GetVirtualFromRealX(mainCanvas.ActualWidth - Settings("XMargin").Value * mainCanvas.ActualWidth - Settings("PriceBarWidth").Value)
        End Get
    End Property
    Public ReadOnly Property VirtualTopMarginLocation As Double
        Get
            Return GetVirtualFromRealY(Settings("YMargin").Value * mainCanvas.ActualHeight)
        End Get
    End Property
    Public ReadOnly Property VirtualBottomMarginLocation As Double
        Get
            Return GetVirtualFromRealY(mainCanvas.ActualHeight - Settings("YMargin").Value * mainCanvas.ActualHeight)
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
            Return result.VisualHit
        ElseIf result IsNot Nothing Then
            Dim prnt As UIChartControl = GetFirstParentOfType(result.VisualHit, GetType(UIChartControl))
            If prnt IsNot Nothing Then Return prnt
        End If
        For Each child As IChartObject In Children
            If TypeOf child Is ChartDrawingVisual AndAlso TypeOf child Is ISelectable AndAlso CType(child, ISelectable).IsSelectable Then
                If Mode = ClickMode.Normal AndAlso CType(child, ChartDrawingVisual).ContainsPoint(point) Then
                    Return child
                End If
            ElseIf TypeOf child Is UIChartControl AndAlso CType(child, UIChartControl).Content Is e.Source Then
                Return child
            End If
        Next
        Return Nothing
    End Function
    Dim clickPoint As Point, clickBounds As Bounds, childClick As Boolean, cursorLines(1) As TrendLine
    Friend Sub Window_MouseButtonDown(ByVal sender As Object, ByVal e As System.Windows.Input.MouseButtonEventArgs)
        If Children.Contains(cursorLines(0)) Then Children.Remove(cursorLines(0))
        If Children.Contains(cursorLines(1)) Then Children.Remove(cursorLines(1))
        If priceAxis.ContentChildren.Contains(priceCursorText) Then priceAxis.ContentChildren.Remove(priceCursorText)
        If timeAxis.ContentChildren.Contains(timeCursorText) Then timeAxis.ContentChildren.Remove(timeCursorText)
        _mouseDown = MouseDownType.MouseDown
        If New Rect(mainCanvas.RenderSize).Contains(e.GetPosition(mainCanvas)) Then
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
                If Mode = ClickMode.Normal Then
                    For i = 0 To 1
                        cursorLines(i) = New TrendLine
                        cursorLines(i).SnapToVirtualPixels = True
                        cursorLines(i).StartPoint = New Point(0, 0)
                        cursorLines(i).EndPoint = New Point(0, 0)
                        cursorLines(i).Pen = New Pen(New SolidColorBrush(Settings("Cursor Line").Value), 1)
                        cursorLines(i).IsEditable = False
                        cursorLines(i).IsSelectable = True
                        Children.Add(cursorLines(i))
                    Next
                    Dim virtualClickPoint As Point = GetVirtualFromReal(e.GetPosition(mainCanvas))
                    If MouseDown = MouseDownType.CapturedOnMe Then
                        cursorLines(0).Coordinates = New LineCoordinates(virtualClickPoint.X, Bounds.Y1, virtualClickPoint.X, Bounds.Y2)
                        cursorLines(0).RefreshVisual()
                        cursorLines(1).Coordinates = New LineCoordinates(Bounds.X1, RoundTo(virtualClickPoint.Y, GetMinTick), Bounds.X2, RoundTo(virtualClickPoint.Y, GetMinTick))
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
        ReleaseMouseCapture()
        timeAxis.ReleaseMouseCapture()
        priceAxis.ReleaseMouseCapture()
        If e.ChangedButton = MouseButton.Left Then
            If Children.Contains(cursorLines(0)) Then Children.Remove(cursorLines(0))
            If Children.Contains(cursorLines(1)) Then Children.Remove(cursorLines(1))
            If priceAxis.ContentChildren.Contains(priceCursorText) Then priceAxis.ContentChildren.Remove(priceCursorText)
            If timeAxis.ContentChildren.Contains(timeCursorText) Then timeAxis.ContentChildren.Remove(timeCursorText)
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
    End Sub
    Friend Sub Window_MouseMove(ByVal sender As Object, ByVal e As System.Windows.Input.MouseEventArgs)
        If e.LeftButton = MouseButtonState.Pressed And borderPress Then Border_MouseMove(e)
        Dim screenPoint As Point = mainCanvas.PointToScreen(e.GetPosition(mainCanvas))
        If e.LeftButton = MouseButtonState.Pressed Then
            For Each child As IChartObject In Children
                If Not TypeOf child Is Bar Then
                    Dim childPoint As Point = CType(child, Visual).PointFromScreen(screenPoint)
                    child.ParentMouseMove(e, childPoint)
                End If
            Next
        Else
            If IsMouseOver Then
                For Each order In Orders
                    Dim orderPoint As Point = CType(order, Visual).PointFromScreen(screenPoint)
                    order.ParentMouseMove(e, orderPoint)
                Next
            End If
        End If
        If e.LeftButton = MouseButtonState.Pressed Then HandleMouseDrag(e)
        If Mode = ClickMode.Normal And (MouseDown = MouseDownType.CapturedOnMe Or MouseDown = MouseDownType.CapturedOnChild) Then
            Dim virtualClickPoint As Point = GetVirtualFromReal(e.GetPosition(mainCanvas))
            If MouseDown = MouseDownType.CapturedOnMe And e.LeftButton = MouseButtonState.Pressed Then
                cursorLines(0).Coordinates = New LineCoordinates(virtualClickPoint.X, Bounds.Y1, virtualClickPoint.X, Bounds.Y2)
                cursorLines(0).RefreshVisual()
                cursorLines(1).Coordinates = New LineCoordinates(Bounds.X1, RoundTo(virtualClickPoint.Y, GetMinTick), Bounds.X2, RoundTo(virtualClickPoint.Y, GetMinTick))
                cursorLines(1).RefreshVisual()
                DrawCursorTexts()
            End If
        End If
    End Sub
    Friend Sub Window_MouseDoubleClick(ByVal sender As Object, ByVal e As System.Windows.Input.MouseButtonEventArgs)
        Border_MouseDoubleClick(Me, e)
        Dim visual As IChartObject = GetClosestObject(e)
        If visual IsNot Nothing Then
            visual.ParentMouseDoubleClick(e, CType(visual, Visual).PointFromScreen(mainCanvas.PointToScreen(e.GetPosition(mainCanvas))))
        End If
    End Sub
    Friend Sub Window_MouseWheel(ByVal sender As Object, ByVal e As System.Windows.Input.MouseWheelEventArgs)
        For Each child In Children
            child.ParentMouseWheel(e)
        Next
    End Sub
    Private Sub HandleModes(ByVal args As MouseButtonEventArgs)
        Select Case Mode
            Case ClickMode.CreateLine
                RefreshPresetStyles()
                Dim pos = GetVirtualFromReal(args.GetPosition(mainCanvas))
                Dim ln As New TrendLine
                Children.Add(ln)
                ln.Coordinates = New LineCoordinates(pos, pos)
                ln.IsEditable = True
                ln.FlipYLocation = True
                If PresetLineStyles(newObjectPresetNum) IsNot Nothing Then
                    ln.Pen = PresetLineStyles(newObjectPresetNum).Pen.Clone
                    ln.ExtendLeft = PresetLineStyles(newObjectPresetNum).ExtendLeft
                    ln.ExtendRight = PresetLineStyles(newObjectPresetNum).ExtendRight
                Else
                    Dim style As New TrendLineStyle
                    ln.Pen = style.Pen.Clone
                    ln.ExtendLeft = style.ExtendLeft
                    ln.ExtendRight = style.ExtendRight
                End If
                ln.AutoRefresh = True
                ln.RefreshVisual()
                ln.ParentMouseLeftButtonDown(args, args.GetPosition(mainCanvas))
            Case ClickMode.CreateLabel
                RefreshPresetStyles()
                Dim lbl As New Label
                Children.Add(lbl)
                lbl.IsEditable = True
                lbl.FlipYLocation = True
                If PresetLabelStyles(newObjectPresetNum) IsNot Nothing Then
                    lbl.Font = PresetLabelStyles(newObjectPresetNum).Font.Clone
                    lbl.Text = PresetLabelStyles(newObjectPresetNum).Text
                Else
                    Dim style As New LabelStyle
                    lbl.Font = style.Font.Clone
                    lbl.Text = style.Text
                End If
                lbl.Location = GetVirtualFromReal(args.GetPosition(mainCanvas))
                lbl.IsSelectable = True
                lbl.AutoRefresh = True
                lbl.RefreshVisual()
            Case ClickMode.CreateArrow
                RefreshPresetStyles()
                Dim arrow As New Arrow
                Children.Add(arrow)
                arrow.IsEditable = True
                arrow.FlipYLocation = True
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
                arrow.Location = GetVirtualFromReal(args.GetPosition(mainCanvas))
                arrow.IsSelectable = True
                arrow.AutoRefresh = True
                arrow.RefreshVisual()
            Case ClickMode.CreateBuyOrder, ClickMode.CreateSellOrder, ClickMode.CreateBuyMarketOrder, ClickMode.CreateSellMarketOrder

        End Select
    End Sub
    Private Sub HandleMouseDrag(ByVal args As MouseEventArgs)
        If Mode = ClickMode.Pan Then
            If args.LeftButton = MouseButtonState.Pressed And (MouseDown = MouseDownType.CapturedOnMe Or MouseDown = MouseDownType.CapturedOnChild) Then
                Dim vec As Point = GetVirtualFromReal(CType(args.GetPosition(mainCanvas) - clickPoint, Point)) - GetVirtualFromReal(New Point(0, 0))
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
            Dim c As Double = (clickBounds.X2 - clickBounds.X1) / 2
            Dim changeAmount As Double = 1.002 ^ (args.GetPosition(mainCanvas).X - clickPoint.X) * c - c
            Dim m As Double = Settings("XMargin").Value * mainCanvas.ActualWidth + Settings("PriceBarWidth").Value
            Dim a = GetRealWithVirtualWidthInVirtual(m, clickBounds.Width + changeAmount - timeAxisClickMarginVirtualWidth)
            TrySetBounds(New Bounds(clickBounds.X1 - changeAmount, Bounds.Y1, clickBounds.X1 - changeAmount + a, Bounds.Y2))
            'Dim multiplier As Double = LinCalc(0, 1, ActualWidth, 2, args.GetPosition(mainCanvas).X - clickPoint.X)
            'Dim newWidth As Double = multiplier * clickBounds.Width
            'Dim endPoint As Double = BarIndex + GetVirtualFromRealWidth(Settings("XMargin").Value * mainCanvas.ActualWidth + Settings("PriceBarWidth").Value)
            'TrySetBounds(New Bounds(endPoint - newWidth, Bounds.Y1, endPoint, Bounds.Y2))
        End If
    End Sub
    Private Function IsModeInCreateObjectMode() As Boolean
        Return Mode = ClickMode.CreateLine OrElse Mode = ClickMode.CreateLabel OrElse Mode = ClickMode.CreateArrow OrElse Mode = ClickMode.CreateBuyOrder OrElse Mode = ClickMode.CreateSellOrder OrElse Mode = ClickMode.CreateBuyMarketOrder OrElse Mode = ClickMode.CreateSellMarketOrder
    End Function
#End Region

#Region "Keyboard Interaction"
    Public Event ChartKeyDown As KeyEventHandler
    Public Event ChartKeyUp As KeyEventHandler
    Friend Sub Window_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Input.KeyEventArgs)
        RefreshOrderBox()
        If IsSelected Then RaiseEvent ChartKeyDown(sender, e)
    End Sub
    Friend Sub Window_KeyUp(ByVal sender As Object, ByVal e As System.Windows.Input.KeyEventArgs)
        RefreshOrderBox()
        If IsSelected Then RaiseEvent ChartKeyUp(sender, e)

    End Sub
#End Region

#Region "Misc"
    Private Sub SetStatusTextToDefault(ByVal forceRefresh As Boolean)
        Dim symbol As String = ""

        symbol = IB.Contract(TickerID).Symbol
        Dim spacing As String = "     "
        If Settings("UseRandom").Value Then symbol = "(RANDOM)"
        statusText.Inlines.Clear()
        Dim run As New Run(symbol & " ")
        run.FontSize = 20
        'run.FontWeight = FontWeights.Bold
        statusText.Inlines.Add(run)
        If Not Settings("UseRandom").Value And IB.Contract(TickerID).Expiry <> "" And IB.Contract(TickerID).SecurityType = SecurityType.Future Then
            statusText.Inlines.Add(spacing & GetExpiryFormattedString(IB.Contract(TickerID).Expiry))
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
        barText = String.Format("RB {0}{1}{2}", CStr(Settings("RangeValue").Value).TrimStart("0"c), spacing, timeString)
        statusText.Inlines.Add(spacing & barText)
    End Sub
    Private Sub SetStatusText(ByVal text As String, Optional ByVal forceRefresh As Boolean = True)
        Dispatcher.Invoke(
            Sub()
                statusText.Text = text
                If forceRefresh Then DispatcherHelper.DoEvents()
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
            For Each rect In chart.rects
                rect.Cursor = cursor
            Next
        Next
    End Sub
    Public Sub ResetCursor()
        mainCanvas.Cursor = Cursors.Arrow
        DesktopWindow.Cursor = Cursors.Arrow
        Me.Cursor = Cursors.Arrow
        For Each chart In DesktopWindow.Charts
            chart.rects(0).Cursor = Cursors.SizeNWSE
            chart.rects(1).Cursor = Cursors.SizeAll
            chart.rects(2).Cursor = Cursors.SizeNESW
            chart.rects(3).Cursor = Cursors.SizeWE
            chart.rects(4).Cursor = Cursors.SizeWE
            chart.rects(5).Cursor = Cursors.SizeNESW
            chart.rects(6).Cursor = Cursors.SizeNS
            chart.rects(7).Cursor = Cursors.SizeNWSE
        Next
    End Sub

    Public Function GetChartName() As String
        If ChartOverrideName = "" Then
            Return "Chart" & TickerID
        Else
            Return ChartOverrideName
        End If
    End Function
#End Region

#Region "Scaling Calculation"
    ''' <summary>
    ''' Gets a non-scaled x component from a scaled x component.
    ''' </summary>
    ''' <param name="x">The scaled x component to be converted.</param>
    Public Function GetRealFromVirtualX(ByVal x As Double) As Double
        Return ((x - Bounds.X1) / (Bounds.X2 - Bounds.X1)) * ActualWidth
    End Function
    ''' <summary>
    ''' Gets a scaled x component from a non-scaled x component.
    ''' </summary>
    ''' <param name="x">The non-scaled x component to be converted.</param>
    Public Function GetVirtualFromRealX(ByVal x As Double) As Double
        Return ((Bounds.X2 - Bounds.X1) * x) / ActualWidth + Bounds.X1
    End Function
    ''' <summary>
    ''' Gets a non-scaled y component from a scaled y component.
    ''' </summary>
    ''' <param name="y">The scaled y component to be converted.</param>
    Public Function GetRealFromVirtualY(ByVal y As Double) As Double
        Return ((y - Bounds.Y1) / (Bounds.Y2 - Bounds.Y1)) * ActualHeight
    End Function
    ''' <summary>
    ''' Gets a scaled y component from a non-scaled y component.
    ''' </summary>
    ''' <param name="y">The non-scaled y component to be converted.</param>
    Public Function GetVirtualFromRealY(ByVal y As Double) As Double
        Return ((Bounds.Y2 - Bounds.Y1) * y) / ActualHeight + Bounds.Y1
    End Function
    ''' <summary>
    ''' Gets a non-scaled point from a scaled point.
    ''' </summary>
    ''' <param name="point">The scaled point to be converted.</param>
    Public Function GetRealFromVirtual(ByVal point As Point) As Point
        Return New Point(GetRealFromVirtualX(point.X), GetRealFromVirtualY(point.Y))
    End Function
    ''' <summary>
    ''' Gets a scaled point from a non-scaled point.
    ''' </summary>
    ''' <param name="point">The non-scaled point to be converted.</param>
    Public Function GetVirtualFromReal(ByVal point As Point) As Point
        Return New Point(GetVirtualFromRealX(point.X), GetVirtualFromRealY(point.Y))
    End Function
    ''' <summary>
    ''' Gets a non-scaled rectangle from a scaled rectangle.
    ''' </summary>
    ''' <param name="rect">The scaled rectangle to be converted.</param>
    Public Function GetRealFromVirtual(ByVal rect As Bounds) As Bounds
        Return New Bounds(GetRealFromVirtual(rect.TopLeft), GetRealFromVirtual(rect.BottomRight))
    End Function
    ''' <summary>
    ''' Gets a scaled rectangle from a non-scaled rectangle.
    ''' </summary>
    ''' <param name="rect">The non-scaled rectangle to be converted.</param>
    Public Function GetVirtualFromReal(ByVal rect As Bounds) As Bounds
        Return New Bounds(GetVirtualFromReal(rect.TopLeft), GetVirtualFromReal(rect.BottomRight))
    End Function
    ''' <summary>
    ''' Gets a non-scaled size from a scaled size.
    ''' </summary>
    ''' <param name="size">The scaled size to be converted.</param>
    Public Function GetRealFromVirtual(ByVal size As Size) As Size
        Return GetRealFromVirtual(New Bounds(0, 0, size)).Size
    End Function
    ''' <summary>
    ''' Gets a scaled size from a non-scaled size.
    ''' </summary>
    ''' <param name="size">The non-scaled size to be converted.</param>
    Public Function GetVirtualFromReal(ByVal size As Size) As Size
        Return GetVirtualFromReal(New Bounds(0, 0, size)).Size
    End Function
    ''' <summary>
    ''' Gets a non-scaled line from a scaled line.
    ''' </summary>
    ''' <param name="line">The scaled line to be converted.</param>
    Public Function GetRealFromVirtual(ByVal line As LineCoordinates) As LineCoordinates
        Return New LineCoordinates(GetRealFromVirtual(line.StartPoint), GetRealFromVirtual(line.EndPoint))
    End Function
    ''' <summary>
    ''' Gets a scaled line from a non-scaled line.
    ''' </summary>
    ''' <param name="line">The non-scaled line to be converted.</param>
    Public Function GetVirtualFromReal(ByVal line As LineCoordinates) As LineCoordinates
        Return New LineCoordinates(GetVirtualFromReal(line.StartPoint), GetVirtualFromReal(line.EndPoint))
    End Function
    ''' <summary>
    ''' Gets a scaled width from a non-scaled width.
    ''' </summary>
    ''' <param name="width">The non-scaled width to be converted.</param>
    Public Function GetVirtualFromRealWidth(ByVal width As Double) As Double
        Return GetVirtualFromRealX(width) - GetVirtualFromRealX(0)
    End Function
    ''' <summary>
    ''' Gets a non-scaled width from a scaled width.
    ''' </summary>
    ''' <param name="width">The scaled width to be converted.</param>
    Public Function GetRealFromVirtualWidth(ByVal width As Double) As Double
        Return GetRealFromVirtualX(width) - GetRealFromVirtualX(0)
    End Function
    ''' <summary>
    ''' Gets a scaled height from a non-scaled height.
    ''' </summary>
    ''' <param name="height">The scaled non-height to be converted.</param>
    Public Function GetVirtualFromRealHeight(ByVal height As Double) As Double
        Return GetVirtualFromRealY(height) - GetVirtualFromRealY(0)
    End Function
    ''' <summary>
    ''' Gets a non-scaled height from a scaled height.
    ''' </summary>
    ''' <param name="height">The scaled height to be converted.</param>
    Public Function GetRealFromVirtualHeight(ByVal height As Double) As Double
        Return GetRealFromVirtualY(height) - GetRealFromVirtualY(0)
    End Function

    Public Function GetRealWithVirtualWidthInVirtual(ByVal realWidth As Double, ByVal virtualWidth As Double) As Double
        Return (virtualWidth * mainCanvas.ActualWidth) / (mainCanvas.ActualWidth - realWidth)
    End Function
    Public Function GetRealWithVirtualHeightInVirtual(ByVal realHeight As Double, ByVal virtualHeight As Double) As Double
        Return (virtualHeight * mainCanvas.ActualHeight) / (mainCanvas.ActualHeight - realHeight)
    End Function
#End Region

#Region "Scaling"

    Private _bounds As Bounds = New Bounds(0, 0, 1, 1)
    Public Property Bounds As Bounds
        <DebuggerStepThrough()>
        Get
            Return _bounds
        End Get
        Set(ByVal value As Bounds)
            Dim args As New RoutedPropertyChangedEventArgs(Of Bounds)(_bounds, value)
            _bounds = value
            RaiseEvent BoundsChanged(Me, args)
            If IsLoaded Then
                For Each child As IChartObject In Children
                    child.ParentBoundsChanged()
                Next
            End If
        End Set
    End Property

    Private Sub SetScaling(ByVal scale As Size)
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
            Dim rightEdge As Integer = BarIndex + GetVirtualFromRealBounds(New Point(Settings("XMargin").Value * mainCanvas.ActualWidth + Settings("PriceBarWidth").Value, 0),
                                                                         New Bounds(0, 0, scale.Width, 0), New Size(mainCanvas.ActualWidth, mainCanvas.ActualHeight)).X
            Dim margin As Double = GetVirtualFromRealBounds(New Point(0, Settings("YMargin").Value * mainCanvas.ActualHeight),
                                                                         New Bounds(0, 0, 0, scale.Height), New Size(mainCanvas.ActualWidth, mainCanvas.ActualHeight)).Y
            If GetRealWithVirtualHeightInVirtual(Settings("YMargin").Value * mainCanvas.ActualHeight * 2, Abs(lowPoint.Y - highPoint.Y)) > scale.Height Then
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
            TrySetBounds(newBounds)
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
                'If BarIndex <= Ceiling(Bounds.X1) + 2 Then 'Round(Bounds.X1 - topLeft.X) = Round(Bounds.X2 - bottomRight.X) And Round(Bounds.X2 - bottomRight.X) < 0 And Ceiling(BarIndex - (Bounds.X2 - XMargin * Bounds.Width - (GetVirtualFromReal(New Point(PriceBarWidth, 0)).X - GetVirtualFromReal(New Point(0, 0)).X))) < 0 Then
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
                Dim zero As Point = GetVirtualFromRealBounds(New Point(0, 0), bnds, sze)
                Dim point As Point = GetVirtualFromRealBounds(New Point(0, Settings("YMargin").Value * mainCanvas.ActualHeight), bnds, sze)
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
        ElseIf e.Command Is ChartCommands.MaximizeChart Then
            Settings("Maximized").Value = Not Settings("Maximized").Value
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

        ElseIf command Is ChartCommands.RemoveAllDrawingObjects Then
            Dim i As Integer
            While i < Children.Count
                If Children(i).AnalysisTechnique Is Nothing And
                    Not TypeOf Children(i) Is Bar And
                    Not TypeOf Children(i) Is Order And
                    Not TypeOf Children(i) Is UIChartControl And
                    (
                       (TypeOf Children(i) Is ISelectable AndAlso CType(Children(i), ISelectable).IsSelectable = True) OrElse
                        Not TypeOf Children(i) Is ISelectable
                     ) Then
                    Children.RemoveAt(i)
                Else
                    i += 1
                End If
            End While
        ElseIf command Is ChartCommands.PanDown Then
            TrySetBounds(New Bounds(Bounds.X1, Bounds.Y1 + Bounds.Height / 30, Bounds.X2, Bounds.Y2 + Bounds.Height / 30))
        ElseIf command Is ChartCommands.PanUp Then
            TrySetBounds(New Bounds(Bounds.X1, Bounds.Y1 - Bounds.Height / 30, Bounds.X2, Bounds.Y2 - Bounds.Height / 30))
        ElseIf command Is ChartCommands.PanLeft Then
            TrySetBounds(New Bounds(Bounds.X1 - Bounds.Width / 35, Bounds.Y1, Bounds.X2 - Bounds.Width / 35, Bounds.Y2))
        ElseIf command Is ChartCommands.PanRight Then
            TrySetBounds(New Bounds(Bounds.X1 + Bounds.Width / 35, Bounds.Y1, Bounds.X2 + Bounds.Width / 35, Bounds.Y2))

        ElseIf command Is ChartCommands.PanEnd Then
            SetScaling(Bounds.Size)
        ElseIf command Is ChartCommands.PanBegin Then
            TrySetBounds(New Bounds(0, Bounds.Y1, Bounds.X2 - Bounds.X1, Bounds.Y2))

        ElseIf command Is ChartCommands.ResizeHorizontalBigger Then
            Dim percentage As Double = (GetVirtualFromRealWidth(Settings("PriceBarWidth").Value) + GetVirtualFromRealWidth(Settings("XMargin").Value * mainCanvas.ActualWidth)) / Bounds.Width
            Dim sizeChange As Double = LinCalc(50, 20, 1000, 500, Bounds.Width)
            TrySetBounds(New Bounds(Bounds.X1 - LinCalc(0.5, sizeChange / 2, 0, sizeChange, percentage), Bounds.Y1, Bounds.X2 + LinCalc(0.5, sizeChange / 2, 0, 0, percentage), Bounds.Y2), , True)
            'TrySetBounds(New Bounds(Bounds.X1 - GetVirtualFromRealWidth(Settings("PriceBarWidth").Value), Bounds.Y1, Bounds.X2 + 120 - GetVirtualFromRealWidth(Settings("PriceBarWidth").Value), Bounds.Y2))
        ElseIf command Is ChartCommands.ResizeHorizontalSmaller Then
            Dim percentage As Double = (GetVirtualFromRealWidth(Settings("PriceBarWidth").Value) + GetVirtualFromRealWidth(Settings("XMargin").Value * mainCanvas.ActualWidth)) / Bounds.Width
            Dim sizeChange As Double = LinCalc(50, 20, 1000, 500, Bounds.Width)
            If Bounds.X2 - Bounds.X1 > 5 Then TrySetBounds(New Bounds(Bounds.X1 + LinCalc(0.5, sizeChange / 2, 0, sizeChange, percentage), Bounds.Y1, Bounds.X2 - LinCalc(0.5, sizeChange / 2, 0, 0, percentage), Bounds.Y2), , True)
            'If Bounds.X2 - Bounds.X1 > 30 Then TrySetBounds(New Bounds(Bounds.X1 + GetVirtualFromRealWidth(Settings("PriceBarWidth").Value), Bounds.Y1, Bounds.X2 - 120 + GetVirtualFromRealWidth(Settings("PriceBarWidth").Value), Bounds.Y2))
        ElseIf command Is ChartCommands.ResizeVerticalBigger Then
            TrySetBounds(New Bounds(Bounds.X1, Bounds.Y1 - 1, Bounds.X2, Bounds.Y2 + 1))
        ElseIf command Is ChartCommands.ResizeVerticalSmaller Then
            If Bounds.Y2 - Bounds.Y1 > 2 Then TrySetBounds(New Bounds(Bounds.X1, Bounds.Y1 + 1, Bounds.X2, Bounds.Y2 - 1)) ' Resizing
        ElseIf command Is ChartCommands.AutoScale Then
            If BarIndex > 0 AndAlso BarIndex > Bounds.X1 Then
                Dim highValue As Double = Double.MinValue
                Dim lowValue As Double = Double.MaxValue
                For i = Max(Min(Bounds.X1, BarIndex), 1) To Min(BarIndex - 1, Bounds.X2)
                    highValue = Max(highValue, bars(i).Data.High)
                    lowValue = Min(lowValue, bars(i).Data.Low)
                Next
                Dim bnds As Bounds = New Bounds(Bounds.X1, -highValue, Bounds.X2, -lowValue)
                Dim sze As Size = New Size(mainCanvas.ActualWidth, mainCanvas.ActualHeight)
                Dim zero As Point = GetVirtualFromRealBounds(New Point(0, 0), bnds, sze)
                Dim point As Point = GetVirtualFromRealBounds(New Point(0, Settings("YMargin").Value * mainCanvas.ActualHeight), bnds, sze)
                highValue += point.Y - zero.Y
                lowValue -= point.Y - zero.Y
                TrySetBounds(New Bounds(Bounds.X1, -highValue, Bounds.X2, -lowValue))
            End If
        ElseIf command Is ChartCommands.SetTimeScaleAsGlobalDefault Then
            WriteSetting(GLOBAL_CONFIG_FILE, "DefaultTimeScale", Bounds.X2 - Bounds.X1)
        ElseIf command Is ChartCommands.SnapTimeScalingToGlobalDefault Then
            SetScaling(New Size(ReadSetting(GLOBAL_CONFIG_FILE, "DefaultTimeScale", 200), Bounds.Height))
        ElseIf command.Name.Contains("SetScalingAsPreset") Then
            Settings("ScalingPreset" & command.Name.Substring(18)).Value = Bounds
        ElseIf command.Name.Contains("SnapScalingToPreset") Then
            SetScaling(Settings("ScalingPreset" & command.Name.Substring(19)).Value.Size)
        ElseIf command Is ChartCommands.FormatAnalysisTechniques Then
            Dim formatWin As New FormatAnalysisTechniquesWindow(Me)
        ElseIf command Is ChartCommands.RefreshAllAnalysisTechniques Then
            If statusText.Text <> "" Then SetStatusText("reapplying analysis techniques...")
            For Each analysisTechnique In AnalysisTechniques
                ReApplyAnalysisTechnique(analysisTechnique.AnalysisTechnique)
            Next
            If statusText.Text <> "" Then SetStatusTextToDefault(False)
        ElseIf command Is ChartCommands.SnapScalingToFitBars Then
            Dim low As Double = Double.MaxValue, high As Double = 0
            For Each bar In bars
                low = Min(bar.Data.Low, low)
                high = Max(bar.Data.High, high)
            Next
            Dim margin As Double = (GetRealWithVirtualHeightInVirtual(Settings("YMargin").Value * mainCanvas.ActualHeight * 2, high - low) - (high - low)) / 2
            Dim littleMargin As Double = (GetRealWithVirtualHeightInVirtual(0.03 * mainCanvas.ActualHeight * 2, high - low) - (high - low)) / 2
            TrySetBounds(New Bounds(0, Min(-high - littleMargin, -CurrentBar.Close - margin), GetRealWithVirtualWidthInVirtual(Settings("XMargin").Value * mainCanvas.ActualWidth + Settings("PriceBarWidth").Value, bars.Count), Max(-low + littleMargin, -CurrentBar.Close + margin)))

            'SetScaling(New Size(bars.Count, high - low))
        ElseIf e.Command Is ChartCommands.CloseChart Then
            ResetSettings()
            Parent.Children.Remove(Me)
        ElseIf e.Command Is ChartCommands.DuplicateChart Then
            ShowInfoBox("This feature is just not implemented!")
            'Dim parent As Workspace = Me.Parent
            'parent.Children.Remove(Me)
            'Dim newChart As New Chart
            'newChart.LocationX = LocationX
            'newChart.LocationY = LocationY
            'newChart.SizeWidth = SizeWidth
            'newChart.SizeHeight = SizeHeight
            'newChart.Background = Background
            'For Each item In Settings.Settings
            '    newChart.Settings(item.Name).Value = item.Value
            'Next
            'parent.Children.Add(Me)
            'parent.Children.Add(newChart)
        End If
    End Sub

    Private Sub FormatChart()
        Dim loadingHistory As Boolean = False
        For Each desktop In My.Application.Desktops
            For Each workspace In desktop.Workspaces
                For Each chart In workspace.Charts
                    If chart.LoadingHistory Then loadingHistory = True
                Next
            Next
        Next
        If loadingHistory Then
            ShowInfoBox("You can not format a chart when a chart is loading data.")
            Exit Sub
        End If
        Static once As Boolean = True
        Static beenRefreshed As Boolean = False
        Dim winConfig As New FormatWindow(Me)
        If once Then
            winConfig.RefreshChart = True
            beenRefreshed = True
        End If
        once = False
        winConfig.ShowDialog()
        For Each chart In Parent.Charts
            chart.RefreshOrderBox()
        Next
        If winConfig.ButtonOKPressed Then
            beenRefreshed = True
            If winConfig.Settings("UseRandom").Value Then
                DataStream = RAND
            Else
                DataStream = IB
            End If
            With winConfig
                If Settings("Bar Color").Value <> .Settings("Bar Color").Value Then
                    For Each bar In bars
                        bar.Pen = New Pen(New SolidColorBrush(.Settings("Bar Color").Value), BarWidth)
                        bar.CloseTickPen = New Pen(New SolidColorBrush(.Settings("Bar Color").Value), 1)
                        bar.OpenTickPen = New Pen(New SolidColorBrush(.Settings("Bar Color").Value), 1)
                        bar.TickWidth = TickWidth
                        bar.RefreshVisual()
                    Next
                End If
                DataStream.BarSize(TickerID) = .Settings("BarSize").Value
                RAND.RandomMaxMove = .Settings("RAND.RandomMaxMove").Value
                RAND.RandomMinMove = .Settings("RAND.RandomMinMove").Value
                RAND.RandomSpeed = .Settings("RAND.RandomSpeed").Value
                Dim daysBackChanged As Boolean = .Settings("DaysBack").Value <> Settings("DaysBack").Value
                For Each item In .Settings.Settings
                    If Settings.Contains(item.Name) AndAlso Settings(item.Name).HasValue Then Settings(item.Name).Value = item.Value
                Next
                Settings("DecimalPlaces").Value = CInt(Settings("DecimalPlaces").Value)
                InitGrid()
                If Settings("UseRandom").Value = False Then
                    IB.CancelMarketData(TickerID)
                    IB.RequestMarketData(TickerID, IB.Contract(TickerID), IB.GenericTickTypes(TickerID), False, False)
                Else
                    RAND.RequestMarketData(TickerID)
                End If
                If .RefreshChart Then
                    RequestData(TimeSpan.FromDays(Settings("DaysBack").Value))
                Else
                    RefreshAxisAndGrid()
                    RefreshOrderBox()
                End If
            End With
        Else
            If Not beenRefreshed Then once = True
        End If
    End Sub
#End Region
End Class