Imports System.Math
Imports System.Windows.Threading
Imports System.Collections.ObjectModel
Imports System.Threading
Imports System.Collections.Specialized
Imports IBApi
Imports QuickTrader

Public Class IBDataStream
    Inherits IBApi.EClientSocket
    Implements DataStream

    Private eReaderSignal As EReaderMonitorSignal
    Public WithEvents EventSources As IBEventSource

    Public Shared ReadOnly Property DefaultTickTypes As Collection(Of GenericTickType)
        Get
            Dim lst As New Collections.ObjectModel.Collection(Of GenericTickType)
            lst.Add(GenericTickType.MarkPrice)
            Return lst
        End Get
    End Property

    Public Sub New(eventSource As IBEventSource, monitorSignal As EReaderMonitorSignal)
        MyBase.New(eventSource, monitorSignal)
        eReaderSignal = monitorSignal
        EventSources = eventSource
        EventSources.Client = Me

        ReplayTimerIndex = 0
        ReplayTimer = New DispatcherTimer(DispatcherPriority.Input)
        ReplayTimer.Interval = TimeSpan.FromMilliseconds(1)
        AddHandler ReplayTimer.Tick, AddressOf Replay_Timer_Tick
        lastTimerTimestamp = Now.Ticks
        ReplayTimer.Start()
    End Sub
    Public Event [Error](sender As Object, e As ErrMsgEventArgs)

    Public Overloads Sub Connect(id As Integer)
        If Not IsConnected() Then
            MyBase.eConnect("", 7496, id)
            Dim msgThread = New Threading.Thread(AddressOf MessageProcessing)
            msgThread.IsBackground = True
            If ServerVersion() > 0 Then msgThread.Start()

        End If
    End Sub
    Private Sub ManagedAccounts(sender As Object, e As ManagedAccountsEventArgs) Handles EventSources.ManagedAccounts

    End Sub
    Private Sub MessageProcessing()
        Dim reader = New EReader(Me, eReaderSignal)

        reader.Start()

        While IsConnected()
            eReaderSignal.waitForSignal()
            reader.processMsgs()
        End While
    End Sub
    Protected Sub OnError(sender As Object, e As ErrMsgEventArgs) Handles EventSources.ErrMsg
        Dim m As String
        m = "Error " & e.errorCode & " (" & [Enum].GetName(GetType(ErrorMessage), e.errorCode) & "): " & e.errorMsg
        Dim raiseError As Boolean = True
        Select Case CInt(e.errorCode)
            Case ErrorMessage.NotConnected  ' ErrorCode has a blank ErrorMsg

                m &= " Not Connected. Try restarting TWS."
            Case 366, 300, 102, 322
                raiseError = False
            Case 162
                e.errorMsg = m
                If e.errorMsg.Contains("pacing") Then
                    IBHistoricalDataFinished(e.id, True)
                Else
                    StepBackCount(e.id) += 1
                    HistoricalDataSectionCompleted(Nothing, New HistoricalDataEndEventArgs() With {.reqId = e.id})
                End If
                Log(e.errorMsg)
                raiseError = False
            Case Else
                e.errorMsg = m
        End Select
        If raiseError Then
            RaiseEvent [Error](sender, e)
        Else
            'Log(e.errorMsg)
        End If
    End Sub

    Private Function CanRecieveData(ByVal [date] As Date, ByVal id As Integer) As Boolean

        Return True
        'If UseRegularTradingHours(id) Then
        '    If [date].TimeOfDay > StartTimeTradingHours(id).TimeOfDay And [date].TimeOfDay < EndTimeTradingHours(id).TimeOfDay Then
        '        Return True
        '    Else
        '        Return False
        '    End If
        'Else
        '    Return True
        'End If
    End Function

    ' Public Properties -----
    Dim _bidAskReqID As New Dictionary(Of Integer, Integer)
    Public Property BidAskReqID(ByVal id As Integer) As Integer
        Get
            If Not _bidAskReqID.ContainsKey(id) Then
                _bidAskReqID.Add(id, 0)
            End If
            Return _bidAskReqID(id)
        End Get
        Set(value As Integer)
            If _bidAskReqID.ContainsKey(id) Then
                _bidAskReqID(id) = value
            Else
                _bidAskReqID.Add(id, value)
            End If
        End Set
    End Property
    Dim _stepBackCount As New Dictionary(Of Integer, Integer)
    Public Property StepBackCount(ByVal id As Integer) As Integer
        Get
            If Not _stepBackCount.ContainsKey(id) Then
                _stepBackCount.Add(id, 0)
            End If
            Return _stepBackCount(id)
        End Get
        Set(value As Integer)
            If _stepBackCount.ContainsKey(id) Then
                _stepBackCount(id) = value
            Else
                _stepBackCount.Add(id, value)
            End If
        End Set
    End Property
    Dim _realTimeReqID As New Dictionary(Of Integer, Integer)
    Public Property RealTimeReqID(ByVal id As Integer) As Integer
        Get
            If Not _realTimeReqID.ContainsKey(id) Then
                _realTimeReqID.Add(id, 0)
            End If
            Return _realTimeReqID(id)
        End Get
        Set(value As Integer)
            If _realTimeReqID.ContainsKey(id) Then
                _realTimeReqID(id) = value
            Else
                _realTimeReqID.Add(id, value)
            End If
        End Set
    End Property
    Dim _mainIDFromBidAskID As New Dictionary(Of Integer, Integer)
    Public Property MainIDFromBidAskID(ByVal id As Integer) As Integer
        Get
            If Not _mainIDFromBidAskID.ContainsKey(id) Then
                _mainIDFromBidAskID.Add(id, 0)
            End If
            Return _mainIDFromBidAskID(id)
        End Get
        Set(value As Integer)
            If _mainIDFromBidAskID.ContainsKey(id) Then
                _mainIDFromBidAskID(id) = value
            Else
                _mainIDFromBidAskID.Add(id, value)
            End If
        End Set
    End Property
    Dim _contract As New Dictionary(Of Integer, Contract)
    Public Property Contract(ByVal id As Integer) As Contract
        Get
            If Not _contract.ContainsKey(id) Then
                Dim defaultContract As New Contract
                defaultContract.Symbol = "AAPL"
                defaultContract.Exchange = "SMART"
                defaultContract.Currency = "USD"
                _contract.Add(id, defaultContract)
            End If
            Return _contract(id)
        End Get
        Set(ByVal value As Contract)
            If _contract.ContainsKey(id) Then
                _contract(id) = value
            Else
                _contract.Add(id, value)
            End If
        End Set
    End Property
    Dim _barSize As New Dictionary(Of Integer, BarSize)
    Public Property BarSize(ByVal id As Integer) As BarSize Implements DataStream.BarSize
        Get
            If Not _barSize.ContainsKey(id) Then
                _barSize.Add(id, BarSize.OneMinute)
            End If
            Return _barSize(id)
        End Get
        Set(ByVal value As BarSize)
            If _barSize.ContainsKey(id) Then
                _barSize(id) = value
            Else
                _barSize.Add(id, value)
            End If
        End Set
    End Property
    Dim _whatToShow As New Dictionary(Of Integer, HistoricalDataType)
    Public Property WhatToShow(ByVal id As Integer) As HistoricalDataType
        Get
            If Not _whatToShow.ContainsKey(id) Then
                _whatToShow.Add(id, HistoricalDataType.Trades)
            End If
            Return _whatToShow(id)
        End Get
        Set(ByVal value As HistoricalDataType)
            If _whatToShow.ContainsKey(id) Then
                _whatToShow(id) = value
            Else
                _whatToShow.Add(id, value)
            End If
        End Set
    End Property
    Dim _useCachedData As New Dictionary(Of Integer, Boolean)
    Public Property UseCachedData(ByVal id As Integer) As Boolean
        Get
            If Not _useCachedData.ContainsKey(id) Then
                _useCachedData.Add(id, True)
            End If
            Return _useCachedData(id)
        End Get
        Set(ByVal value As Boolean)
            If _useCachedData.ContainsKey(id) Then
                _useCachedData(id) = value
            Else
                _useCachedData.Add(id, value)
            End If
        End Set
    End Property


    Private __recieveData As New Dictionary(Of Integer, Integer)
    Private Property _receiveData(id As Integer) As Boolean
        Get
            If Not __recieveData.ContainsKey(id) Then
                __recieveData.Add(id, False)
            End If
            Return __recieveData(id)
        End Get
        Set(ByVal value As Boolean)
            If __recieveData.ContainsKey(id) Then
                __recieveData(id) = value
            Else
                __recieveData.Add(id, value)
            End If
        End Set
    End Property

    Dim _daysBack As New Dictionary(Of Integer, Decimal)
    Public Property DaysBack(ByVal id As Integer) As Decimal Implements DataStream.DaysBack
        Get
            If Not _daysBack.ContainsKey(id) Then
                _daysBack.Add(id, 1)
            End If
            Return _daysBack(id)
        End Get
        Set(ByVal value As Decimal)
            If _daysBack.ContainsKey(id) Then
                _daysBack(id) = value
            Else
                _daysBack.Add(id, value)
            End If
        End Set
    End Property
    Dim _minTick As New Dictionary(Of Integer, Double)
    Public Property MinTick(ByVal id As Integer) As Double
        Get
            If Not _minTick.ContainsKey(id) Then
                _minTick.Add(id, 0.25)
            End If
            Return _minTick(id)
        End Get
        Set(ByVal value As Double)
            If _minTick.ContainsKey(id) Then
                _minTick(id) = value
            Else
                _minTick.Add(id, value)
            End If
        End Set
    End Property
    'Dim _multiplier As New Dictionary(Of Integer, String)
    'Public Property Multiplier(ByVal id As Integer) As String
    '    Get
    '        If Not _multiplier.ContainsKey(id) Then
    '            _multiplier.Add(id, 1)
    '        End If
    '        Return _multiplier(id)
    '    End Get
    '    Set(ByVal value As String)
    '        If _multiplier.ContainsKey(id) Then
    '            _multiplier(id) = value
    '        Else
    '            _multiplier.Add(id, value)
    '        End If
    '    End Set
    'End Property
    Dim _useRegularTradingHour As New Dictionary(Of Integer, Boolean)
    Public Property UseRegularTradingHours(ByVal id As Integer) As Boolean
        Get
            If Not _useRegularTradingHour.ContainsKey(id) Then
                _useRegularTradingHour.Add(id, False)
            End If
            Return _useRegularTradingHour(id)
        End Get
        Set(ByVal value As Boolean)
            If _useRegularTradingHour.ContainsKey(id) Then
                _useRegularTradingHour(id) = value
            Else
                _useRegularTradingHour.Add(id, value)
            End If
        End Set
    End Property
    Public Property GenericTickTypes(ByVal id As Integer) As String
        Get
            Return "221" 'markprice
        End Get
        Set(ByVal value As String)
        End Set
    End Property

    Dim _sundayStartTimeTradingHours As New Dictionary(Of Integer, Date)
    Public Property SundayStartTimeTradingHours(ByVal id As Integer) As Date
        Get
            If Not _sundayStartTimeTradingHours.ContainsKey(id) Then
                _sundayStartTimeTradingHours.Add(id, New Date(1, 1, 1, 8, 30, 0))
            End If
            Return _sundayStartTimeTradingHours(id)
        End Get
        Set(ByVal value As Date)
            If _sundayStartTimeTradingHours.ContainsKey(id) Then
                _sundayStartTimeTradingHours(id) = value
            Else
                _sundayStartTimeTradingHours.Add(id, value)
            End If
        End Set
    End Property
    Dim _sundayEndTimeTradingHours As New Dictionary(Of Integer, Date)
    Public Property SundayEndTimeTradingHours(ByVal id As Integer) As Date
        Get
            If Not _sundayEndTimeTradingHours.ContainsKey(id) Then
                _sundayEndTimeTradingHours.Add(id, New Date(1, 1, 1, 15, 0, 0))
            End If
            Return _sundayEndTimeTradingHours(id)
        End Get
        Set(ByVal value As Date)
            If _sundayEndTimeTradingHours.ContainsKey(id) Then
                _sundayEndTimeTradingHours(id) = value
            Else
                _sundayEndTimeTradingHours.Add(id, value)
            End If
        End Set
    End Property
    Dim _weekdayStartTimeTradingHours As New Dictionary(Of Integer, Date)
    Public Property WeekdayStartTimeTradingHours(ByVal id As Integer) As Date
        Get
            If Not _weekdayStartTimeTradingHours.ContainsKey(id) Then
                _weekdayStartTimeTradingHours.Add(id, New Date(1, 1, 1, 8, 30, 0))
            End If
            Return _weekdayStartTimeTradingHours(id)
        End Get
        Set(ByVal value As Date)
            If _weekdayStartTimeTradingHours.ContainsKey(id) Then
                _weekdayStartTimeTradingHours(id) = value
            Else
                _weekdayStartTimeTradingHours.Add(id, value)
            End If
        End Set
    End Property
    Dim _weekdayEndTimeTradingHours As New Dictionary(Of Integer, Date)
    Public Property WeekdayEndTimeTradingHours(ByVal id As Integer) As Date
        Get
            If Not _weekdayEndTimeTradingHours.ContainsKey(id) Then
                _weekdayEndTimeTradingHours.Add(id, New Date(1, 1, 1, 15, 0, 0))
            End If
            Return _weekdayEndTimeTradingHours(id)
        End Get
        Set(ByVal value As Date)
            If _weekdayEndTimeTradingHours.ContainsKey(id) Then
                _weekdayEndTimeTradingHours(id) = value
            Else
                _weekdayEndTimeTradingHours.Add(id, value)
            End If
        End Set
    End Property

    Private _lastBidPriceValue As New Dictionary(Of Integer, Double)
    Private Property lastBidPriceValue(ByVal id As Integer) As Double
        Get
            If Not _lastBidPriceValue.ContainsKey(id) Then
                _lastBidPriceValue.Add(id, 0)
            End If
            Return _lastBidPriceValue(id)
        End Get
        Set(ByVal value As Double)
            If _lastBidPriceValue.ContainsKey(id) Then
                _lastBidPriceValue(id) = value
            Else
                _lastBidPriceValue.Add(id, value)
            End If
        End Set
    End Property
    Private _lastAskPriceValue As New Dictionary(Of Integer, Double)
    Private Property lastAskPriceValue(ByVal id As Integer) As Double
        Get
            If Not _lastAskPriceValue.ContainsKey(id) Then
                _lastAskPriceValue.Add(id, 0)
            End If
            Return _lastAskPriceValue(id)
        End Get
        Set(ByVal value As Double)
            If _lastAskPriceValue.ContainsKey(id) Then
                _lastAskPriceValue(id) = value
            Else
                _lastAskPriceValue.Add(id, value)
            End If
        End Set
    End Property
    Private _lastPriceValue As New Dictionary(Of Integer, Double)
    Private Property lastPriceValue(ByVal id As Integer) As Double
        Get
            If Not _lastPriceValue.ContainsKey(id) Then
                _lastPriceValue.Add(id, 0)
            End If
            Return _lastPriceValue(id)
        End Get
        Set(ByVal value As Double)
            If _lastPriceValue.ContainsKey(id) Then
                _lastPriceValue(id) = value
            Else
                _lastPriceValue.Add(id, value)
            End If
        End Set
    End Property
    Private _lastPrice As New Dictionary(Of Integer, Double)
    Private Property lastPrice(ByVal id As Integer) As Double
        Get
            If Not _lastPrice.ContainsKey(id) Then
                _lastPrice.Add(id, 0)
            End If
            Return _lastPrice(id)
        End Get
        Set(ByVal value As Double)
            If _lastPrice.ContainsKey(id) Then
                _lastPrice(id) = value
            Else
                _lastPrice.Add(id, value)
            End If
        End Set
    End Property
    Dim _currentHistoricalBarData As New Dictionary(Of Integer, List(Of BarData))
    Private Property incrementHistoricalBarData(ByVal id As Integer) As List(Of BarData)
        Get
            If Not _currentHistoricalBarData.ContainsKey(id) Then
                _currentHistoricalBarData.Add(id, Nothing)
            End If
            Return _currentHistoricalBarData(id)
        End Get
        Set(ByVal value As List(Of BarData))
            If _currentHistoricalBarData.ContainsKey(id) Then
                _currentHistoricalBarData(id) = value
            Else
                _currentHistoricalBarData.Add(id, value)
            End If
        End Set
    End Property
    Dim _totalHistoricalBarData As New Dictionary(Of Integer, List(Of BarData))
    Private Property allLoadedHistoricalBarData(ByVal id As Integer) As List(Of BarData)
        Get
            If Not _totalHistoricalBarData.ContainsKey(id) Then
                _totalHistoricalBarData.Add(id, Nothing)
            End If
            Return _totalHistoricalBarData(id)
        End Get
        Set(ByVal value As List(Of BarData))
            If _totalHistoricalBarData.ContainsKey(id) Then
                _totalHistoricalBarData(id) = value
            Else
                _totalHistoricalBarData.Add(id, value)
            End If
        End Set
    End Property
    Dim _historicalIncrement As New Dictionary(Of Integer, TimeSpan)
    Private Property historicalIncrement(ByVal id As Integer) As TimeSpan
        Get
            If Not _historicalIncrement.ContainsKey(id) Then
                _historicalIncrement.Add(id, Nothing)
            End If
            Return _historicalIncrement(id)
        End Get
        Set(ByVal value As TimeSpan)
            If _historicalIncrement.ContainsKey(id) Then
                _historicalIncrement(id) = value
            Else
                _historicalIncrement.Add(id, value)
            End If
        End Set
    End Property
    Dim _startHistRequestDate As New Dictionary(Of Integer, Date)
    Private Property startHistRequestDate(ByVal id As Integer) As Date
        Get
            If Not _startHistRequestDate.ContainsKey(id) Then
                _startHistRequestDate.Add(id, Nothing)
            End If
            Return _startHistRequestDate(id)
        End Get
        Set(ByVal value As Date)
            If _startHistRequestDate.ContainsKey(id) Then
                _startHistRequestDate(id) = value
            Else
                _startHistRequestDate.Add(id, value)
            End If
        End Set
    End Property
    Dim _startHistRequestCacheDate As New Dictionary(Of Integer, Date)
    Private Property startHistRequestCacheDate(ByVal id As Integer) As Date
        Get
            If Not _startHistRequestCacheDate.ContainsKey(id) Then
                _startHistRequestCacheDate.Add(id, Nothing)
            End If
            Return _startHistRequestCacheDate(id)
        End Get
        Set(ByVal value As Date)
            If _startHistRequestCacheDate.ContainsKey(id) Then
                _startHistRequestCacheDate(id) = value
            Else
                _startHistRequestCacheDate.Add(id, value)
            End If
        End Set
    End Property
    Dim _endHistRequestDate As New Dictionary(Of Integer, Date)
    Private Property endHistRequestDate(ByVal id As Integer) As Date
        Get
            If Not _endHistRequestDate.ContainsKey(id) Then
                _endHistRequestDate.Add(id, Nothing)
            End If
            Return _endHistRequestDate(id)
        End Get
        Set(ByVal value As Date)
            If _endHistRequestDate.ContainsKey(id) Then
                _endHistRequestDate(id) = value
            Else
                _endHistRequestDate.Add(id, value)
            End If
        End Set
    End Property

    Dim _cacheBars As New Dictionary(Of Integer, List(Of BarData))
    Private Property cacheBars(ByVal id As Integer) As List(Of BarData)
        Get
            If Not _cacheBars.ContainsKey(id) Then
                _cacheBars.Add(id, New List(Of BarData))
            End If
            Return _cacheBars(id)
        End Get
        Set(ByVal value As List(Of BarData))
            If _cacheBars.ContainsKey(id) Then
                _cacheBars(id) = value
            Else
                _cacheBars.Add(id, value)
            End If
        End Set
    End Property
    'Dim _loadedFromCache As New Dictionary(Of Integer, Boolean)
    'Private Property loadedFromCache(ByVal id As Integer) As Boolean
    '    Get
    '        If Not _loadedFromCache.ContainsKey(id) Then
    '            _loadedFromCache.Add(id, False)
    '        End If
    '        Return _loadedFromCache(id)
    '    End Get
    '    Set(ByVal value As Boolean)
    '        If _loadedFromCache.ContainsKey(id) Then
    '            _loadedFromCache(id) = value
    '        Else
    '            _loadedFromCache.Add(id, value)
    '        End If
    '    End Set
    'End Property
    'Dim _beginCacheBarIndex As New Dictionary(Of Integer, Integer)
    'Private Property beginCacheBarIndex(ByVal id As Integer) As Integer
    '    Get
    '        If Not _beginCacheBarIndex.ContainsKey(id) Then
    '            _beginCacheBarIndex.Add(id, 0)
    '        End If
    '        Return _beginCacheBarIndex(id)
    '    End Get
    '    Set(ByVal value As Integer)
    '        If _beginCacheBarIndex.ContainsKey(id) Then
    '            _beginCacheBarIndex(id) = value
    '        Else
    '            _beginCacheBarIndex.Add(id, value)
    '        End If
    '    End Set
    'End Property
    Dim _lastCacheDate As New Dictionary(Of Integer, Date)
    Private Property lastCacheDate(ByVal id As Integer) As Date
        Get
            If Not _lastCacheDate.ContainsKey(id) Then
                _lastCacheDate.Add(id, Date.MinValue)
            End If
            Return _lastCacheDate(id)
        End Get
        Set(ByVal value As Date)
            If _lastCacheDate.ContainsKey(id) Then
                _lastCacheDate(id) = value
            Else
                _lastCacheDate.Add(id, value)
            End If
        End Set
    End Property

    Public Property ReplaySpeedMultiplier As Integer = 1 ' the multiplier that specifies doubling or tripling the speed.
    Public Property IsReplayRunning As Boolean
    Public ReadOnly Property IsReplayEnabled As Boolean
        Get
            For Each item In _enableReplayMode
                If item.Value Then Return True
            Next
            Return False
        End Get
    End Property
    Private _currentReplayTime As Date
    Public Property CurrentReplayTime As Date Implements DataStream.CurrentReplayTime
        Get
            If _currentReplayTime = Date.MinValue Then Return Now Else Return _currentReplayTime
        End Get
        Set(value As Date)
            _currentReplayTime = value
        End Set
    End Property
    Dim ReplayTimer As DispatcherTimer
    Dim ReplayTimerIndex As Long
    Private lastTimerTimestamp As Long
    Private _currentReplaySpeed As Double
    Public ReadOnly Property CurrentReplaySpeed As Double ' returns how many times faster than realtime the data is being fired
        Get
            Return _currentReplaySpeed
        End Get
    End Property
    Public Event ReplaySpeedUpdated(newSpeed As Double)
    Dim _lastReplayBarNum As New Dictionary(Of Integer, Integer)
    Private Property lastReplayBarNum(ByVal id As Integer) As Integer
        Get
            If Not _lastReplayBarNum.ContainsKey(id) Then
                _lastReplayBarNum.Add(id, -1)
            End If
            Return _lastReplayBarNum(id)
        End Get
        Set(ByVal value As Integer)
            If _lastReplayBarNum.ContainsKey(id) Then
                _lastReplayBarNum(id) = value
            Else
                _lastReplayBarNum.Add(id, value)
            End If
        End Set
    End Property
    Dim _enableReplayMode As New Dictionary(Of Integer, Boolean)
    Public Property EnableReplayMode(ByVal id As Integer) As Boolean Implements DataStream.UseReplayMode
        Get
            If Not _enableReplayMode.ContainsKey(id) Then
                _enableReplayMode.Add(id, False)
            End If
            Return _enableReplayMode(id)
        End Get
        Set(ByVal value As Boolean)
            If _enableReplayMode.ContainsKey(id) Then
                _enableReplayMode(id) = value
            Else
                _enableReplayMode.Add(id, value)
            End If
        End Set
    End Property
    Public Property ReplaySpeed As Integer = 50 ' 1 = fastest, higher numbers are slower
    Private Sub Replay_Timer_Tick(sender As Object, e As EventArgs)
        ReplayTimerIndex += 1
        For Each item In _enableReplayMode
            Dim id = item.Key
            If _receiveData(id) AndAlso item.Value AndAlso IsReplayRunning AndAlso CanRecieveData(Now, id) AndAlso _cacheBars.ContainsKey(id) AndAlso
                cacheBars(id).Count > lastReplayBarNum(id) AndAlso lastReplayBarNum(id) > 0 Then
                Dim seconds = CInt(TimeSpanFromBarSize(BarSize(id)).TotalSeconds)
                Dim val = (seconds * ReplaySpeed)
                If val <= 0 Then val = 1
                If ReplayTimerIndex Mod val = 0 Then
                    For i = 1 To ReplaySpeedMultiplier
                        If cacheBars(id).Count > lastReplayBarNum(id) Then
                            lastPrice(id) = cacheBars(id)(lastReplayBarNum(id)).Close
                            CurrentReplayTime = cacheBars(id)(lastReplayBarNum(id)).Date
                            RaiseEvent MarketPriceData(TickType.LastPrice, cacheBars(id)(lastReplayBarNum(id)), id)
                            lastReplayBarNum(id) += 1
                        Else
                            Exit For
                        End If
                    Next
                End If
            End If
        Next
        If ReplayTimerIndex Mod 100 = 0 Then
            _currentReplaySpeed = ((1000 / ReplaySpeed) * ReplaySpeedMultiplier) / (((Now.Ticks - lastTimerTimestamp) / TimeSpan.TicksPerMillisecond) / 100)
            RaiseEvent ReplaySpeedUpdated(_currentReplaySpeed)
            lastTimerTimestamp = Now.Ticks
        End If
    End Sub
    Public Sub JumpForwardInReplayTime(timeAmount As TimeSpan)
        Dim startTime As Date = CurrentReplayTime
        For Each item In _enableReplayMode
            Dim id = item.Key
            If _receiveData(id) AndAlso item.Value AndAlso CanRecieveData(Now, id) AndAlso _cacheBars.ContainsKey(id) AndAlso
                cacheBars(id).Count > lastReplayBarNum(id) AndAlso lastReplayBarNum(id) > 0 Then
                While True
                    If lastReplayBarNum(id) >= cacheBars(id).Count OrElse cacheBars(id)(lastReplayBarNum(id)).Date > startTime + timeAmount Then Exit While
                    lastPrice(id) = cacheBars(id)(lastReplayBarNum(id)).Close
                    CurrentReplayTime = cacheBars(id)(lastReplayBarNum(id)).Date
                    RaiseEvent MarketPriceData(TickType.LastPrice, cacheBars(id)(lastReplayBarNum(id)), id)
                    lastReplayBarNum(id) += 1
                End While
            End If
        Next
    End Sub

    Private Function GetEndCacheTime(id As Integer) As Date
        If lastCacheDate(id) = Date.MinValue Then
            Return cacheBars(id)(cacheBars(id).Count - 1).Date
        Else
            Return lastCacheDate(id)
        End If
    End Function
    Public Sub ClearMemoryCache(id As Integer)
        cacheBars(id).Clear()
        lastCacheDate(id) = Date.MinValue
    End Sub
    Public Overloads Sub CancelHistoricalData(ByVal id As Integer) Implements DataStream.CancelHistoricalData
        MyBase.cancelHistoricalData(id)
    End Sub
    ' Master call for historical data ----
    Public Overloads Sub RequestHistoricalData(ByVal endDate As Date, ByVal duration As System.TimeSpan, ByVal id As Integer) Implements DataStream.RequestHistoricalData
        allLoadedHistoricalBarData(id) = New List(Of BarData)
        Dim beginDate As Date = endDate - duration
        'If Not UseReplayMode(id) Then endDate = Now

        Dim beginCacheBarIndex As Integer = -1
        Dim endCacheBarIndex As Integer = -1
        If UseCachedData(id) Then
            If cacheBars(id).Count = 0 Then ' if no memory cache
                'load cache from file 
                Dim cacheFileName As String = GetCacheFileName(id)
                Dim reader As New StreamReader(cacheFileName)
                Dim converter As New BarDataConverter
                Dim i As Integer = 0
                While reader.Peek <> -1
                    Dim line As String = reader.ReadLine
                    If converter.IsValid(line) Then
                        cacheBars(id).Add(converter.ConvertFromString(line))
                        If beginCacheBarIndex = -1 AndAlso cacheBars(id)(i).Date > beginDate Then
                            beginCacheBarIndex = i
                        End If
                        If i >= cacheBars(id).Count OrElse (cacheBars(id)(i).Date > endDate And endCacheBarIndex = -1) Then
                            endCacheBarIndex = Min(i, cacheBars(id).Count - 1)
                        End If
                    End If
                    i += 1
                End While
                reader.Close()
            Else
                ' otherwise use memory cache and find begin date index
                For i = 0 To cacheBars(id).Count - 1
                    If cacheBars(id)(i).Date > beginDate And beginCacheBarIndex = -1 Then
                        beginCacheBarIndex = i
                    End If
                    If cacheBars(id)(i).Date > endDate And endCacheBarIndex = -1 Then
                        endCacheBarIndex = i
                    End If
                    If endCacheBarIndex <> -1 And beginCacheBarIndex <> -1 Then Exit For
                Next
            End If
        End If
        If endCacheBarIndex = -1 Or Not EnableReplayMode(id) Then endCacheBarIndex = cacheBars(id).Count - 1
        lastReplayBarNum(id) = endCacheBarIndex

        If UseCachedData(id) And beginCacheBarIndex <> -1 And (cacheBars(id).Count > 0 AndAlso cacheBars(id)(0).Date < beginDate) Then ' if begin date of cache is before begin date of request
            Log("loaded " & Round((cacheBars(id)(endCacheBarIndex).Date - cacheBars(id)(beginCacheBarIndex).Date).TotalDays, 3) & " days from the cache (" &
               TimeAndDateStringFromDate(cacheBars(id)(beginCacheBarIndex).Date) & " - " & TimeAndDateStringFromDate(cacheBars(id)(endCacheBarIndex).Date) & ")")
            startHistRequestDate(id) = GetEndCacheTime(id)
            startHistRequestCacheDate(id) = endDate - duration
            endHistRequestDate(id) = endDate
            If IsConnected() And endDate - GetEndCacheTime(id) > TimeSpan.FromSeconds(60) Then
                RequestIBHistoricalData(endDate, endDate - GetEndCacheTime(id), id)
            Else
                IBHistoricalDataFinished(id, False)
            End If
        Else
            startHistRequestDate(id) = endDate - duration
            startHistRequestCacheDate(id) = endDate - duration
            endHistRequestDate(id) = endDate
            If IsConnected() And duration > TimeSpan.FromSeconds(120) Then
                cacheBars(id).Clear()
                lastCacheDate(id) = Date.MinValue
                RequestIBHistoricalData(endDate, duration, id)
            Else
                OnError(Me, New ErrMsgEventArgs() With {.id = id, .errorCode = ErrorMessage.FailSendRequestHistoricalData, .errorMsg = "You are not connected to IB and there is no cache data to display."})
            End If
        End If
    End Sub
    ' Helper caller for IB data
    Public Sub RequestIBHistoricalData(ByVal endDate As Date, ByVal duration As System.TimeSpan, ByVal id As Integer)
        If endDate > lastCacheDate(id) Then lastCacheDate(id) = endDate
        Dim incrementInMinutes As Integer
        Dim minuteDuration As Double = duration.TotalMinutes
        Select Case BarSize(id)
            Case QuickTrader.BarSize.OneSecond
                incrementInMinutes = 30
            Case QuickTrader.BarSize.FiveSeconds
                incrementInMinutes = 60
            Case QuickTrader.BarSize.FifteenSeconds
                incrementInMinutes = 240
            Case QuickTrader.BarSize.ThirtySeconds
                incrementInMinutes = 480
            Case QuickTrader.BarSize.OneMinute
                incrementInMinutes = 24 * 60
            Case QuickTrader.BarSize.FiveMinutes
                incrementInMinutes = 7 * 24 * 60
            Case QuickTrader.BarSize.FifteenMinutes
                incrementInMinutes = 7 * 24 * 60
            Case QuickTrader.BarSize.ThirtyMinutes
                incrementInMinutes = 30 * 24 * 60
            Case QuickTrader.BarSize.OneHour
                incrementInMinutes = 30 * 24 * 60
            Case QuickTrader.BarSize.OneDay
                incrementInMinutes = 364 * 60 * 24
            Case QuickTrader.BarSize.OneMonth
                incrementInMinutes = 364 * 60 * 24
            Case QuickTrader.BarSize.OneYear
                incrementInMinutes = 364 * 60 * 24
        End Select

        incrementHistoricalBarData(id) = New List(Of BarData)
        historicalIncrement(id) = TimeSpan.FromMinutes(incrementInMinutes)
        Dim logText As String
        If minuteDuration > incrementInMinutes Then
            logText = "loading " & Round(TimeSpan.FromMinutes(incrementInMinutes).TotalDays, 3) & " days of data beginning " & TimeAndDateStringFromDate(endDate) & "..."
            RequestIBHistoricalDataDirect(endDate, TimeSpan.FromMinutes(incrementInMinutes), id)
            Log("request received for " & CInt(Ceiling(duration.TotalMinutes / historicalIncrement(id).TotalMinutes)) & " calls, " & Round(duration.TotalDays, 3) & " days of data")
            Log(logText)
        Else
            logText = "loading " & Round(duration.TotalDays, 3) & " days of data beginning " & TimeAndDateStringFromDate(endDate) & "..."
            RequestIBHistoricalDataDirect(endDate, duration, id)
            Log(logText)
        End If

    End Sub
    ' Direct call to IB for data
    Public Sub RequestIBHistoricalDataDirect(ByVal endDate As Date, ByVal duration As TimeSpan, ByVal id As Integer)
        'CancelHistoricalData(id)
        'CancelMarketData(id)

        If BarSize(id) >= QuickTrader.BarSize.OneSecond And BarSize(id) <= QuickTrader.BarSize.ThirtySeconds Then
            reqHistoricalData(id, Contract(id), endDate.ToString("yyyyMMdd HH:mm:ss"), CInt(Ceiling(duration.TotalSeconds)) & " S", ParseBarSize(BarSize(id)), ParseWhatToShowType(WhatToShow(id)), If(UseRegularTradingHours(id), 1, 0), 1, False, Nothing)
        Else
            reqHistoricalData(id, Contract(id), endDate.ToString("yyyyMMdd HH:mm:ss"), CInt(Ceiling(duration.TotalDays)) & " D", ParseBarSize(BarSize(id)), ParseWhatToShowType(WhatToShow(id)), If(UseRegularTradingHours(id), 1, 0), 1, False, Nothing)
        End If

    End Sub

    Private Sub HistDataBarRecieved(ByVal sender As Object, ByVal e As HistoricalDataEventArgs) Handles EventSources.HistoricalData
        Dim d = Date.ParseExact(e.date, "yyyyMMdd  HH:mm:ss", Nothing)
        Dim bar As BarData = New BarData(e.open, e.high, e.low, e.close, d, TimeSpanFromBarSize(BarSize(e.reqId)))
        incrementHistoricalBarData(e.reqId).Add(bar)
        StepBackCount(e.reqId) = 0
    End Sub
    Private Sub HistoricalDataSectionCompleted(ByVal sender As Object, ByVal e As HistoricalDataEndEventArgs) Handles EventSources.HistoricalDataEnd
        Dim id As Integer = e.reqId
        SyncLock allLoadedHistoricalBarData(id)
            allLoadedHistoricalBarData(id).InsertRange(0, incrementHistoricalBarData(id))
            incrementHistoricalBarData(id).Clear()
            If allLoadedHistoricalBarData(id)(0).Date < startHistRequestDate(id) Then
                'were done
                While allLoadedHistoricalBarData(id)(0).Date < startHistRequestDate(id)
                    allLoadedHistoricalBarData(id).RemoveAt(0)
                End While

                Log("all data loading completed")
                IBHistoricalDataFinished(id, True)
            Else
                RaiseEvent HistoricalDataPartiallyCompleted(id, 0.5)
                Log("segment complete; loading " & Round(historicalIncrement(id).TotalDays, 3) & " days of data beginning " & TimeAndDateStringFromDate(allLoadedHistoricalBarData(id)(0).Date - TimeSpan.FromSeconds(historicalIncrement(id).TotalSeconds * StepBackCount(e.reqId))) & "...")
                Threading.Thread.Sleep(300)
                RequestIBHistoricalDataDirect(allLoadedHistoricalBarData(id)(0).Date - TimeSpan.FromSeconds(historicalIncrement(id).TotalSeconds * StepBackCount(e.reqId)), historicalIncrement(id), id)
            End If
        End SyncLock
    End Sub
    Private Sub IBHistoricalDataFinished(ByVal id As Integer, cacheData As Boolean)
        cacheBars(id).AddRange(allLoadedHistoricalBarData(id))
        If cacheData And allLoadedHistoricalBarData(id).Count <> 0 Then
            Dim t = Now
            Dim writer As New StreamWriter(GetCacheFileName(id), False)
            Dim converter As New BarDataConverter
            For Each item In cacheBars(id)
                writer.WriteLine(converter.ConvertToString(item))
            Next
            writer.Close()
        End If
        For i = 0 To cacheBars(id).Count - 1
            If cacheBars(id)(i).Date >= startHistRequestCacheDate(id) And cacheBars(id)(i).Date <= endHistRequestDate(id) Then
                RaiseEvent HistoricalDataBarRecieved(cacheBars(id)(i), id)
            End If
        Next
        RaiseEvent HistoricalDataCompleted(id)
    End Sub
    Private Sub TickByTickAllLast(ByVal sender As Object, ByVal e As TickByTickAllLastEventArgs) Handles EventSources.TickByTickAllLast
        If lastPriceValue(e.reqId) <> e.price Then RaiseEvent MarketPriceData(TickType.LastPrice, New BarData(e.price, e.price, e.price, e.price, Now, TimeSpanFromBarSize(BarSize(e.reqId))), e.reqId)
        lastPriceValue(e.reqId) = e.price
    End Sub
    Private Sub TickByTickBidAsk(ByVal sender As Object, ByVal e As TickByTickBidAskEventArgs) Handles EventSources.TickByTickBidAsk
        If lastAskPriceValue(e.reqId) <> e.askPrice Then RaiseEvent MarketPriceData(TickType.AskPrice, New BarData(e.askPrice, e.askPrice, e.askPrice, e.askPrice, Now, TimeSpanFromBarSize(BarSize(e.reqId))), e.reqId)
        lastAskPriceValue(e.reqId) = e.askPrice
        If lastBidPriceValue(e.reqId) <> e.bidPrice Then RaiseEvent MarketPriceData(TickType.BidPrice, New BarData(e.bidPrice, e.bidPrice, e.bidPrice, e.bidPrice, Now, TimeSpanFromBarSize(BarSize(e.reqId))), e.reqId)
        lastBidPriceValue(e.reqId) = e.bidPrice
    End Sub

    Private Sub EventSources_RealtimeBar(sender As Object, e As RealtimeBarEventArgs) Handles EventSources.RealtimeBar
        RaiseEvent RealTimeBarUpdate(New BarData(e.open, e.high, e.low, e.close, Now, TimeSpan.FromSeconds(5)), e.reqId)
    End Sub

    Private Sub EventSources_TickPrice(sender As Object, e As TickPriceEventArgs) Handles EventSources.TickPrice
        If CanRecieveData(Now, MainIDFromBidAskID(e.id)) Then
            If e.tickType = TickType.AskPrice Then
                If lastAskPriceValue(e.price) <> e.price Then RaiseEvent MarketPriceData(TickType.AskPrice, New BarData(e.price, e.price, e.price, e.price, Now, TimeSpanFromBarSize(BarSize(e.id))), e.id)
                lastAskPriceValue(e.id) = e.price
            ElseIf e.tickType = TickType.BidPrice Then
                If lastBidPriceValue(e.id) <> e.price Then RaiseEvent MarketPriceData(TickType.BidPrice, New BarData(e.price, e.price, e.price, e.price, Now, TimeSpanFromBarSize(BarSize(e.id))), e.id)
                lastBidPriceValue(e.id) = e.price
            ElseIf e.tickType = TickType.LastPrice Then
                If lastPriceValue(e.id) <> e.price Then RaiseEvent MarketPriceData(TickType.LastPrice, New BarData(e.price, e.price, e.price, e.price, Now, TimeSpanFromBarSize(BarSize(e.id))), e.id)
                lastPriceValue(e.id) = e.price
            End If
        End If
    End Sub
    Private Function NullCall() As KeyValuePair(Of DateTime, TimeSpan)
        Return New KeyValuePair(Of Date, TimeSpan)(Date.MinValue, TimeSpan.MinValue)
    End Function

    Public Overloads Sub CancelMarketData(ByVal id As Integer) Implements DataStream.CancelMarketData
        lastPrice(id) = 0
        _receiveData(id) = False
        'cancelMktData(id)
        'cancelRealTimeBars(id)
        cancelTickByTickData(id)
        cancelTickByTickData(BidAskReqID(id))
    End Sub
    Public Overloads Sub RequestMarketData(ByVal id As Integer) Implements DataStream.RequestMarketData
        lastPrice(id) = 0
        _receiveData(id) = True
        'reqMktData(id, Contract(id), GenericTickTypes(id), False, False, Nothing)
        'reqRealTimeBars(id, Contract(id), 5, "MIDPOINT", False, Nothing)
        reqTickByTickData(id, Contract(id), "Last", 0, True)
        reqTickByTickData(BidAskReqID(id), Contract(id), "BidAsk", 0, True)
    End Sub

    Public Event HistoricalDataBarRecieved(ByVal bar As BarData, ByVal id As Integer) Implements DataStream.HistoricalDataBarRecieved
    Public Event HistoricalDataCompleted(ByVal id As Integer) Implements DataStream.HistoricalDataCompleted
    Public Event HistoricalDataPartiallyCompleted(ByVal id As Integer, ByVal percentageCompleted As Double) Implements DataStream.HistoricalDataPartiallyCompleted
    Public Event MarketPriceData(ByVal tickType As TickType, bar As BarData, ByVal id As Integer) Implements DataStream.MarketPriceData
    Public Event RealTimeBarUpdate(bar As BarData, ByVal id As Integer)

    Private Function ReadBarsFromFile(ByVal file As String) As List(Of BarData)
        'Return New List(Of BarData)
        Dim reader As New StreamReader(file)
        Dim bars As New Dictionary(Of Date, BarData)
        Dim converter As New BarDataConverter
        While reader.Peek <> -1
            Dim line As String = reader.ReadLine
            line = line.Trim
            If line <> "" Then
                Dim data As BarData = converter.ConvertFromString(line)
                If Not bars.ContainsKey(data.Date) Then
                    bars.Add(data.Date, data)
                End If
            End If
        End While
        reader.Close()
        Dim lst = bars.ToList
        lst.Sort(New Comparison(Of KeyValuePair(Of Date, BarData))(
              Function(item1 As KeyValuePair(Of Date, BarData), item2 As KeyValuePair(Of Date, BarData)) As Boolean
                  Return item1.Key < item2.Key
              End Function)
        )
        Dim dic As New List(Of BarData)
        For Each item In lst
            dic.Add(item.Value)
        Next
        Return dic
    End Function

    Public Function GetCacheFileName(ByVal id As Integer) As String
        Dim cacheDir As String = AppDomain.CurrentDomain.BaseDirectory & "cache"
        Dim cacheFileName As String = cacheDir & "\" & Contract(id).Symbol & "-" & [Enum].GetName(GetType(BarSize), BarSize(id)) & If(ParseSecType(Contract(id).SecType) = SecurityType.FUT, "-" & Contract(id).LastTradeDateOrContractMonth, "") & If(UseRegularTradingHours(id), "-RTH", "") & ".dat" 'ES-FifteenSecond-RTH.dat
        If Not Directory.Exists(cacheDir) Then Directory.CreateDirectory(cacheDir)
        If Not File.Exists(cacheFileName) Then File.WriteAllBytes(cacheFileName, {})
        Return cacheFileName
    End Function
    Public Function GetTradesCacheFileName(ByVal id As Integer) As String
        Dim cacheDir As String = AppDomain.CurrentDomain.BaseDirectory & "cache\trades data"
        Dim cacheFileName As String = cacheDir & "\" & Contract(id).Symbol & "-" & [Enum].GetName(GetType(BarSize), BarSize(id)) & If(ParseSecType(Contract(id).SecType) = SecurityType.FUT, "-" & Contract(id).LastTradeDateOrContractMonth, "") & If(UseRegularTradingHours(id), "-RTH", "") & ".dat" 'ES-FifteenSecond-RTH.dat
        If Not Directory.Exists(cacheDir) Then Directory.CreateDirectory(cacheDir)
        If Not File.Exists(cacheFileName) Then File.WriteAllBytes(cacheFileName, {})
        Return cacheFileName
    End Function


    'Public Function GetIndexFileName(ByVal id As Integer) As String
    '    Dim cacheDir As String = AppDomain.CurrentDomain.BaseDirectory & "cache"
    '    Dim indexFileName As String = cacheDir & "\" & Contract(id).Symbol & "-" & [Enum].GetName(GetType(BarSize), BarSize(id)) & If(Contract(id).SecurityType = SecurityType.Future, "-" & Contract(id).Expiry, "") & If(UseRegularTradingHours(id), "-RTH", "") & ".idx" 'ES-FifteenSecond-RTH.idx
    '    If Not File.Exists(indexFileName) Then File.WriteAllText(indexFileName, "")
    '    Return indexFileName
    'End Function

End Class

Public Class RandomDataStream
    Implements DataStream

    Private tmr As New DispatcherTimer(DispatcherPriority.Send)
    Private _receiveData As Boolean
    Private _speed As Double = 1
    Public Property RandomDeflection As Double = 0.5
    Public Property RandomMinMove As Double = 0.25
    Public Property RandomMaxMove As Double = 0.25
    Public Property RandomSpeed As Double
        <DebuggerStepThrough>
        Get
            Return _speed
        End Get
        <DebuggerStepThrough>
        Set(ByVal value As Double)
            _speed = value
            If RandomSpeed > 0.000001 Then tmr.Interval = TimeSpan.FromSeconds((Rnd() + 0.25) / (RandomSpeed / 2)) Else tmr.Interval = TimeSpan.FromMilliseconds(Integer.MaxValue)
        End Set
    End Property
    Private _randomPrice As Double = RoundTo((Int(Rnd() * 101) - 50 + 1000), RandomMinMove)
    Public Property RandomPrice As Double
        <DebuggerStepThrough>
        Get
            Return RoundTo(_randomPrice, RandomMinMove)
        End Get
        <DebuggerStepThrough>
        Set(ByVal value As Double)
            _randomPrice = value
            RaiseEvent MarketPriceData(TickType.LastPrice, New BarData(_randomPrice, _randomPrice, _randomPrice, _randomPrice, Now, TimeSpanFromBarSize(BarSize(currentID))), 0)
        End Set
    End Property
    <DebuggerStepThrough>
    Private Function GetRandomPriceMove() As Double
        'Dim num As Double = Rnd() * (RandomMaxMove - RandomMinMove) + RandomMinMove
        'num *= RandomChoice(-1, 1, RandomDeflection) ' Floor(Rnd() * 3) - 1 ' make it positive or negative
        Dim nums(0) As Double
        For i = 0 To 0
            nums(i) = Rnd() * (RandomMaxMove - RandomMinMove) + RandomMinMove
            nums(i) *= RandomChoice(-1, 1, RandomDeflection)
        Next
        Return nums.Average
    End Function
    <DebuggerStepThrough>
    Private Sub tmr_Tick(ByVal sender As Object, ByVal e As EventArgs)
        If RandomSpeed > 0.000001 Then tmr.Interval = TimeSpan.FromSeconds((Rnd() + 0.25) / (RandomSpeed / 2)) Else tmr.Interval = TimeSpan.FromMilliseconds(Integer.MaxValue)
        _randomPrice += GetRandomPriceMove()
        If _receiveData Then
            RaiseEvent MarketPriceData(TickType.LastPrice, New BarData(RandomPrice, RandomPrice, RandomPrice, RandomPrice, Now, TimeSpanFromBarSize(BarSize(currentID))), currentID)
        End If
    End Sub
    Public Sub New()
        Randomize()
        tmr.IsEnabled = True
        tmr.Interval = TimeSpan.FromSeconds(RandomSpeed)
        AddHandler tmr.Tick, AddressOf tmr_Tick
    End Sub

    Public Event HistoricalDataBarRecieved(ByVal bar As BarData, ByVal id As Integer) Implements DataStream.HistoricalDataBarRecieved
    Public Event HistoricalDataCompleted(ByVal id As Integer) Implements DataStream.HistoricalDataCompleted
    Public Event HistoricalDataPartiallyCompleted(ByVal id As Integer, ByVal percentCompleted As Double) Implements DataStream.HistoricalDataPartiallyCompleted
    Public Event MarketPriceData(ByVal tickType As TickType, bar As BarData, ByVal id As Integer) Implements DataStream.MarketPriceData

    Public Sub CancelHistoricalData(ByVal id As Integer) Implements DataStream.CancelHistoricalData
    End Sub
    Public Sub CancelMarketData(ByVal id As Integer) Implements DataStream.CancelMarketData
        _receiveData = False
    End Sub
    Public Sub RequestHistoricalData(ByVal endDate As Date, ByVal duration As System.TimeSpan, ByVal id As Integer) Implements DataStream.RequestHistoricalData
        currentID = id
        Dim time As DateTime = endDate - duration
        Dim val As Double = 1
        Dim counter As Integer

        Dim total As Integer = CInt(duration.TotalMinutes / TimeSpanFromBarSize(BarSize(id)).TotalMinutes)
        While time < endDate
            RaiseEvent HistoricalDataBarRecieved(New BarData(_randomPrice, _randomPrice + Abs(GetRandomPriceMove), _randomPrice - Abs(GetRandomPriceMove), _randomPrice, time, TimeSpanFromBarSize(BarSize(id))), id)
            If counter Mod 100 = 0 Then RaiseEvent HistoricalDataPartiallyCompleted(id, counter / total)
            _randomPrice += GetRandomPriceMove()
            _randomPrice = RoundTo(_randomPrice, RandomMinMove)
            time = time.Add(TimeSpanFromBarSize(BarSize(id)))
            counter += 1
        End While
        RaiseEvent HistoricalDataCompleted(id)
    End Sub
    Private currentID As Integer
    Public Sub RequestMarketData(ByVal id As Integer) Implements DataStream.RequestMarketData
        _receiveData = True
        currentID = id
    End Sub
    Private _barSize As BarSize = QuickTrader.BarSize.OneMinute
    Public Property BarSize(ByVal id As Integer) As QuickTrader.BarSize Implements DataStream.BarSize
        Get
            Return _barSize
        End Get
        Set(ByVal value As BarSize)
            _barSize = value
        End Set
    End Property
    Dim _replayMode As Boolean
    Public Property UseReplayMode(ByVal id As Integer) As Boolean Implements DataStream.UseReplayMode
        Get
            Return _replayMode
        End Get
        Set(ByVal value As Boolean)
            _replayMode = value
        End Set
    End Property
    '
    Dim _daysBack As Decimal = 1
    Public Property DaysBack(ByVal id As Integer) As Decimal Implements DataStream.DaysBack
        Get
            Return _daysBack
        End Get
        Set(ByVal value As Decimal)
            _daysBack = value
        End Set
    End Property

    Public Property CurrentReplayTime As Date Implements DataStream.CurrentReplayTime

End Class

Public Interface DataStream
    Sub RequestMarketData(ByVal id As Integer)
    Sub CancelMarketData(ByVal id As Integer)
    Event MarketPriceData(ByVal tickType As TickType, bar As BarData, ByVal id As Integer)
    Event HistoricalDataBarRecieved(ByVal bar As BarData, ByVal id As Integer)
    Event HistoricalDataCompleted(ByVal id As Integer)
    Event HistoricalDataPartiallyCompleted(ByVal id As Integer, ByVal percentCompleted As Double)
    Sub RequestHistoricalData(ByVal endDate As Date, ByVal duration As TimeSpan, ByVal id As Integer)
    Sub CancelHistoricalData(ByVal id As Integer)
    Property BarSize(ByVal id As Integer) As BarSize
    Property UseReplayMode(ByVal id As Integer) As Boolean
    Property DaysBack(ByVal id As Integer) As Decimal
    Property CurrentReplayTime As Date

End Interface


'0 =  
'1 = 
'2 = 
'3 = 
'4 = 
'5 = 
'6 = 
'7 = 
'8 = 
'9 = 	
'10 = 

'11 = 
'12 = 
'13 = 

'14 = 
'15 = 
'16 = 
'17 = 
'18 = 
'19 = 
'20 = 
'21 = 
'22 = 
'23 = 
'24 = 
'25 = 
'26 = 
'27 = 
'28 = 
'29 = 
'30 = 
'31 = 
'32 =  
'33 = !
'34 = "
'35 = #
'36 = $
'37 = %
'38 = &
'39 = '
'40 = (
'41 = )
'42 = *
'43 = +
'44 = ,
'45 = -
'46 = .
'47 = /
'48 = 0
'49 = 1
'50 = 2
'51 = 3
'52 = 4
'53 = 5
'54 = 6
'55 = 7
'56 = 8
'57 = 9
'58 = :
'59 = ;
'60 = <
'61 = =
'62 = >
'63 = ?
'64 = @
'65 = A
'66 = B
'67 = C
'68 = D
'69 = E
'70 = F
'71 = G
'72 = H
'73 = I
'74 = J
'75 = K
'76 = L
'77 = M
'78 = N
'79 = O
'80 = P
'81 = Q
'82 = R
'83 = S
'84 = T
'85 = U
'86 = V
'87 = W
'88 = X
'89 = Y
'90 = Z
'91 = [
'92 = \
'93 = ]
'94 = ^
'95 = _
'96 = `
'97 = a
'98 = b
'99 = c
'100 = d
'101 = e
'102 = f
'103 = g
'104 = h
'105 = i
'106 = j
'107 = k
'108 = l
'109 = m
'110 = n
'111 = o
'112 = p
'113 = q
'114 = r
'115 = s
'116 = t
'117 = u
'118 = v
'119 = w
'120 = x
'121 = y
'122 = z
'123 = {
'124 = |
'125 = }
'126 = ~
'127 = 
'128 = €
'129 = 
'130 = ‚
'131 = ƒ
'132 = „
'133 = …
'134 = †
'135 = ‡
'136 = ˆ
'137 = ‰
'138 = Š
'139 = ‹
'140 = Œ
'141 = 
'142 = Ž
'143 = 
'144 = 
'145 = ‘
'146 = ’
'147 = “
'148 = ”
'149 = •
'150 = –
'151 = —
'152 = ˜
'153 = ™
'154 = š
'155 = ›
'156 = œ
'157 = 
'158 = ž
'159 = Ÿ
'160 =  
'161 = ¡
'162 = ¢
'163 = £
'164 = ¤
'165 = ¥
'166 = ¦
'167 = §
'168 = ¨
'169 = ©
'170 = ª
'171 = «
'172 = ¬
'173 = ­
'174 = ®
'175 = ¯
'176 = °
'177 = ±
'178 = ²
'179 = ³
'180 = ´
'181 = µ
'182 = ¶
'183 = ·
'184 = ¸
'185 = ¹
'186 = º
'187 = »
'188 = ¼
'189 = ½
'190 = ¾
'191 = ¿
'192 = À
'193 = Á
'194 = Â
'195 = Ã
'196 = Ä
'197 = Å
'198 = Æ
'199 = Ç
'200 = È
'201 = É
'202 = Ê
'203 = Ë
'204 = Ì
'205 = Í
'206 = Î
'207 = Ï
'208 = Ð
'209 = Ñ
'210 = Ò
'211 = Ó
'212 = Ô
'213 = Õ
'214 = Ö
'215 = ×
'216 = Ø
'217 = Ù
'218 = Ú
'219 = Û
'220 = Ü
'221 = Ý
'222 = Þ
'223 = ß
'224 = à
'225 = á
'226 = â
'227 = ã
'228 = ä
'229 = å
'230 = æ
'231 = ç
'232 = è
'233 = é
'234 = ê
'235 = ë
'236 = ì
'237 = í
'238 = î
'239 = ï
'240 = ð
'241 = ñ
'242 = ò
'243 = ó
'244 = ô
'245 = õ
'246 = ö
'247 = ÷
'248 = ø
'249 = ù
'250 = ú
'251 = û
'252 = ü
'253 = ý
'254 = þ
'255 = ÿ

