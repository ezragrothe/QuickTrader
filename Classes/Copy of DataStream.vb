'Imports System.Math
'Imports System.Windows.Threading
'Imports Krs.Ats.IBNet
'Imports System.Collections.ObjectModel
'Imports System.Threading
'Imports System.Collections.Specialized

'Public Class IBDataStream
'    Inherits IBClient
'    Implements DataStream

'    Public Shared ReadOnly Property DefaultTickTypes As Collection(Of GenericTickType)
'        Get
'            Dim lst As New Collections.ObjectModel.Collection(Of GenericTickType)
'            lst.Add(100) : lst.Add(101) : lst.Add(104) : lst.Add(105) : lst.Add(106) : lst.Add(107)
'            lst.Add(165) : lst.Add(221) : lst.Add(225) : lst.Add(233) : lst.Add(236) : lst.Add(258)
'            Return lst
'        End Get
'    End Property

'    Public Sub New()
'        ThrowExceptions = False
'    End Sub

'    Public Overloads Sub Connect(ByVal id As Integer)
'        MyBase.Connect("", 7496, id)
'    End Sub
'    Protected Overrides Sub OnError(ByVal e As ErrorEventArgs)
'        Dim m As String
'        m = "Error: " & e.ErrorCode & " " & e.ErrorMsg
'        Select Case CInt(e.ErrorCode)
'            'Case 162 ' Error:162 Historical Market Data Service error message:HMDS query returned no data: QQQQ@SMART [Perhaps incorrect exchange?]
'            'Case 321 ' Error:321 Historical data queries on this contract requesting any data earlier than one year back from now which is [date] are rejected. Your query would have run from [date] to [date].
'            'Case 366 ' Error:366 No historical data query found for ticker id:0
'            Case 504 ' ErrorCode has a blank ErrorMsg

'                m &= " Not Connected. Try restarting TWS."
'            Case 300, 202, 165, 2000 To 2999 'all 2xxx are warnings
'            Case Else
'                e.ErrorMsg = m
'                MyBase.OnError(e)
'        End Select
'    End Sub

'    Private Function CanRecieveData(ByVal [date] As Date, ByVal id As Integer) As Boolean
'        If UseRegularTradingHours(id) Then
'            If [date].TimeOfDay > StartTimeTradingHours(id).TimeOfDay And [date].TimeOfDay < EndTimeTradingHours(id).TimeOfDay Then
'                Return True
'            Else
'                Return False
'            End If
'        Else
'            Return True
'        End If
'    End Function

'    ' Public Properties -----
'    Dim _contract As New Dictionary(Of Integer, Contract)
'    Public Property Contract(ByVal id As Integer) As Contract
'        Get
'            If Not _contract.ContainsKey(id) Then
'                Dim defaultContract As New Contract
'                defaultContract.Symbol = "AAPL"
'                defaultContract.Exchange = "SMART"
'                defaultContract.Currency = "USD"
'                _contract.Add(id, defaultContract)
'            End If
'            Return _contract(id)
'        End Get
'        Set(ByVal value As Contract)
'            If _contract.ContainsKey(id) Then
'                _contract(id) = value
'            Else
'                _contract.Add(id, value)
'            End If
'        End Set
'    End Property
'    Dim _barSize As New Dictionary(Of Integer, BarSize)
'    Public Property BarSize(ByVal id As Integer) As BarSize Implements DataStream.BarSize
'        Get
'            If Not _barSize.ContainsKey(id) Then
'                _barSize.Add(id, BarSize.OneMinute)
'            End If
'            Return _barSize(id)
'        End Get
'        Set(ByVal value As BarSize)
'            If _barSize.ContainsKey(id) Then
'                _barSize(id) = value
'            Else
'                _barSize.Add(id, value)
'            End If
'        End Set
'    End Property
'    Dim _whatToShow As New Dictionary(Of Integer, HistoricalDataType)
'    Public Property WhatToShow(ByVal id As Integer) As HistoricalDataType
'        Get
'            If Not _whatToShow.ContainsKey(id) Then
'                _whatToShow.Add(id, HistoricalDataType.Trades)
'            End If
'            Return _whatToShow(id)
'        End Get
'        Set(ByVal value As HistoricalDataType)
'            If _whatToShow.ContainsKey(id) Then
'                _whatToShow(id) = value
'            Else
'                _whatToShow.Add(id, value)
'            End If
'        End Set
'    End Property
'    Dim _minTick As New Dictionary(Of Integer, Double)
'    Public Property MinTick(ByVal id As Integer) As Double
'        Get
'            If Not _minTick.ContainsKey(id) Then
'                _minTick.Add(id, 0.25)
'            End If
'            Return _minTick(id)
'        End Get
'        Set(ByVal value As Double)
'            If _minTick.ContainsKey(id) Then
'                _minTick(id) = value
'            Else
'                _minTick.Add(id, value)
'            End If
'        End Set
'    End Property
'    Dim _useRegularTradingHour As New Dictionary(Of Integer, Boolean)
'    Public Property UseRegularTradingHours(ByVal id As Integer) As Boolean
'        Get
'            If Not _useRegularTradingHour.ContainsKey(id) Then
'                _useRegularTradingHour.Add(id, False)
'            End If
'            Return _useRegularTradingHour(id)
'        End Get
'        Set(ByVal value As Boolean)
'            If _useRegularTradingHour.ContainsKey(id) Then
'                _useRegularTradingHour(id) = value
'            Else
'                _useRegularTradingHour.Add(id, value)
'            End If
'        End Set
'    End Property
'    Dim _genericTickTypes As New Dictionary(Of Integer, Collection(Of GenericTickType))
'    Public Property GenericTickTypes(ByVal id As Integer) As Collection(Of GenericTickType)
'        Get
'            If Not _genericTickTypes.ContainsKey(id) Then
'                _genericTickTypes.Add(id, DefaultTickTypes)
'            End If
'            Return _genericTickTypes(id)
'        End Get
'        Set(ByVal value As Collection(Of GenericTickType))
'            If _genericTickTypes.ContainsKey(id) Then
'                _genericTickTypes(id) = value
'            Else
'                _genericTickTypes.Add(id, value)
'            End If
'        End Set
'    End Property
'    Dim _startTimeTradingHours As New Dictionary(Of Integer, Date)
'    Public Property StartTimeTradingHours(ByVal id As Integer) As Date
'        Get
'            If Not _startTimeTradingHours.ContainsKey(id) Then
'                _startTimeTradingHours.Add(id, New Date(1, 1, 1, 8, 30, 0))
'            End If
'            Return _startTimeTradingHours(id)
'        End Get
'        Set(ByVal value As Date)
'            If _startTimeTradingHours.ContainsKey(id) Then
'                _startTimeTradingHours(id) = value
'            Else
'                _startTimeTradingHours.Add(id, value)
'            End If
'        End Set
'    End Property
'    Dim _endTimeTradingHours As New Dictionary(Of Integer, Date)
'    Public Property EndTimeTradingHours(ByVal id As Integer) As Date
'        Get
'            If Not _endTimeTradingHours.ContainsKey(id) Then
'                _endTimeTradingHours.Add(id, New Date(1, 1, 1, 15, 0, 0))
'            End If
'            Return _endTimeTradingHours(id)
'        End Get
'        Set(ByVal value As Date)
'            If _endTimeTradingHours.ContainsKey(id) Then
'                _endTimeTradingHours(id) = value
'            Else
'                _endTimeTradingHours.Add(id, value)
'            End If
'        End Set
'    End Property

'    ' Private variables for historical data calling -----
'    Dim _historicalDataBeginDate As New Dictionary(Of Integer, Date)
'    Private Property historicalDataBeginDate(ByVal id As Integer) As Date
'        Get
'            If Not _historicalDataBeginDate.ContainsKey(id) Then
'                _historicalDataBeginDate.Add(id, Nothing)
'            End If
'            Return _historicalDataBeginDate(id)
'        End Get
'        Set(ByVal value As Date)
'            If _historicalDataBeginDate.ContainsKey(id) Then
'                _historicalDataBeginDate(id) = value
'            Else
'                _historicalDataBeginDate.Add(id, value)
'            End If
'        End Set
'    End Property
'    Dim _readBeginDate As New Dictionary(Of Integer, Boolean)
'    Private Property readBeginDate(ByVal id As Integer) As Boolean
'        Get
'            If Not _readBeginDate.ContainsKey(id) Then
'                _readBeginDate.Add(id, False)
'            End If
'            Return _readBeginDate(id)
'        End Get
'        Set(ByVal value As Boolean)
'            If _readBeginDate.ContainsKey(id) Then
'                _readBeginDate(id) = value
'            Else
'                _readBeginDate.Add(id, value)
'            End If
'        End Set
'    End Property
'    Private _lastPrice As New Dictionary(Of Integer, Double)
'    Private Property lastPrice(ByVal id As Integer) As Double
'        Get
'            If Not _lastPrice.ContainsKey(id) Then
'                _lastPrice.Add(id, 0)
'            End If
'            Return _lastPrice(id)
'        End Get
'        Set(ByVal value As Double)
'            If _lastPrice.ContainsKey(id) Then
'                _lastPrice(id) = value
'            Else
'                _lastPrice.Add(id, value)
'            End If
'        End Set
'    End Property
'    Private _histTimer As New Dictionary(Of Integer, Timer)
'    Private Property histTimer(ByVal id As Integer) As Timer
'        Get
'            If Not _histTimer.ContainsKey(id) Then
'                _histTimer.Add(id, New Timer(AddressOf HistoricalDataSectionCompleted, id, -1, -1))
'            End If
'            Return _histTimer(id)
'        End Get
'        Set(ByVal value As Timer)
'            If _histTimer.ContainsKey(id) Then
'                _histTimer(id) = value
'            Else
'                _histTimer.Add(id, New Timer(AddressOf HistoricalDataSectionCompleted, id, -1, -1))
'            End If
'        End Set
'    End Property
'    Dim _currentHistoricalBarData As New Dictionary(Of Integer, List(Of BarData))
'    Private Property currentHistoricalBarData(ByVal id As Integer) As List(Of BarData)
'        Get
'            If Not _currentHistoricalBarData.ContainsKey(id) Then
'                _currentHistoricalBarData.Add(id, Nothing)
'            End If
'            Return _currentHistoricalBarData(id)
'        End Get
'        Set(ByVal value As List(Of BarData))
'            If _currentHistoricalBarData.ContainsKey(id) Then
'                _currentHistoricalBarData(id) = value
'            Else
'                _currentHistoricalBarData.Add(id, value)
'            End If
'        End Set
'    End Property
'    Dim _totalHistoricalBarData As New Dictionary(Of Integer, List(Of BarData))
'    Private Property totalHistoricalBarData(ByVal id As Integer) As List(Of BarData)
'        Get
'            If Not _totalHistoricalBarData.ContainsKey(id) Then
'                _totalHistoricalBarData.Add(id, Nothing)
'            End If
'            Return _totalHistoricalBarData(id)
'        End Get
'        Set(ByVal value As List(Of BarData))
'            If _totalHistoricalBarData.ContainsKey(id) Then
'                _totalHistoricalBarData(id) = value
'            Else
'                _totalHistoricalBarData.Add(id, value)
'            End If
'        End Set
'    End Property
'    Dim _historicalIncrement As New Dictionary(Of Integer, TimeSpan)
'    Private Property historicalIncrement(ByVal id As Integer) As TimeSpan
'        Get
'            If Not _historicalIncrement.ContainsKey(id) Then
'                _historicalIncrement.Add(id, Nothing)
'            End If
'            Return _historicalIncrement(id)
'        End Get
'        Set(ByVal value As TimeSpan)
'            If _historicalIncrement.ContainsKey(id) Then
'                _historicalIncrement(id) = value
'            Else
'                _historicalIncrement.Add(id, value)
'            End If
'        End Set
'    End Property
'    Dim _isHistoricalDataIncrementsLoaded As New Dictionary(Of Integer, Boolean)
'    Private Property isHistoricalDataIncrementsLoaded(ByVal id As Integer) As Boolean
'        Get
'            If Not _isHistoricalDataIncrementsLoaded.ContainsKey(id) Then
'                _isHistoricalDataIncrementsLoaded.Add(id, Nothing)
'            End If
'            Return _isHistoricalDataIncrementsLoaded(id)
'        End Get
'        Set(ByVal value As Boolean)
'            If _isHistoricalDataIncrementsLoaded.ContainsKey(id) Then
'                _isHistoricalDataIncrementsLoaded(id) = value
'            Else
'                _isHistoricalDataIncrementsLoaded.Add(id, value)
'            End If
'        End Set
'    End Property
'    Dim _incrementCount As New Dictionary(Of Integer, Integer)
'    Private Property incrementCount(ByVal id As Integer) As Integer
'        Get
'            If Not _incrementCount.ContainsKey(id) Then
'                _incrementCount.Add(id, 1)
'            End If
'            Return _incrementCount(id)
'        End Get
'        Set(ByVal value As Integer)
'            If _incrementCount.ContainsKey(id) Then
'                _incrementCount(id) = value
'            Else
'                _incrementCount.Add(id, value)
'            End If
'        End Set
'    End Property
'    Dim _maxIncrements As New Dictionary(Of Integer, Integer)
'    Private Property maxIncrements(ByVal id As Integer) As Integer
'        Get
'            If Not _maxIncrements.ContainsKey(id) Then
'                _maxIncrements.Add(id, 0)
'            End If
'            Return _maxIncrements(id)
'        End Get
'        Set(ByVal value As Integer)
'            If _maxIncrements.ContainsKey(id) Then
'                _maxIncrements(id) = value
'            Else
'                _maxIncrements.Add(id, value)
'            End If
'        End Set
'    End Property
'    Dim _cacheDataToAppend As New Dictionary(Of Integer, List(Of BarData))
'    Private Property cacheDataToAppend(ByVal id As Integer) As List(Of BarData)
'        Get
'            If Not _cacheDataToAppend.ContainsKey(id) Then
'                _cacheDataToAppend.Add(id, Nothing)
'            End If
'            Return _cacheDataToAppend(id)
'        End Get
'        Set(ByVal value As List(Of BarData))
'            If _cacheDataToAppend.ContainsKey(id) Then
'                _cacheDataToAppend(id) = value
'            Else
'                _cacheDataToAppend.Add(id, value)
'            End If
'        End Set
'    End Property
'    Dim _nextHistoricalDataCall As New Dictionary(Of Integer, KeyValuePair(Of Date, TimeSpan))
'    Private Property nextHistoricalDataCall(ByVal id As Integer) As KeyValuePair(Of DateTime, TimeSpan)
'        Get
'            If Not _nextHistoricalDataCall.ContainsKey(id) Then
'                _nextHistoricalDataCall.Add(id, NullCall)
'            End If
'            Return _nextHistoricalDataCall(id)
'        End Get
'        Set(ByVal value As KeyValuePair(Of Date, TimeSpan))
'            If _nextHistoricalDataCall.ContainsKey(id) Then
'                _nextHistoricalDataCall(id) = value
'            Else
'                _nextHistoricalDataCall.Add(id, value)
'            End If
'        End Set
'    End Property
'    Dim _cacheBars As New Dictionary(Of Integer, List(Of BarData))
'    Private Property cacheBars(ByVal id As Integer) As List(Of BarData)
'        Get
'            If Not _cacheBars.ContainsKey(id) Then
'                _cacheBars.Add(id, Nothing)
'            End If
'            Return _cacheBars(id)
'        End Get
'        Set(ByVal value As List(Of BarData))
'            If _cacheBars.ContainsKey(id) Then
'                _cacheBars(id) = value
'            Else
'                _cacheBars.Add(id, value)
'            End If
'        End Set
'    End Property

'    Public Overloads Sub CancelHistoricalData(ByVal id As Integer) Implements DataStream.CancelHistoricalData
'        MyBase.CancelHistoricalData(id)
'    End Sub
'    ' Master call for historical data ----
'    Public Overloads Sub RequestHistoricalData(ByVal endDate As Date, ByVal duration As System.TimeSpan, ByVal id As Integer) Implements DataStream.RequestHistoricalData
'        RequestIBHistoricalData(endDate, duration, id)
'        'RequestIBHistoricalData(id, Contract(id), endDate, duration, BarSize(id), WhatToShow(id), If(UseRegularTradingHours(id), 1, 0))
'        'cacheBars(id) = ReadBarsFromFile(CacheFileName(id))

'        'For i = 0 To cacheBars(id).Count - 1
'        'If cacheBars(id)(i).Date >
'        'Next
'        'If CType(bars(0), BarData).Date > endDate - duration Then
'        '    RequestIBHistoricalData(CType(bars(0), BarData).Date, CType(bars(0), BarData).Date - (endDate - duration), id)
'        'Else

'        'End If
'    End Sub
'    ' Helper caller for IB data
'    Public Sub RequestIBHistoricalData(ByVal endDate As Date, ByVal duration As System.TimeSpan, ByVal id As Integer)
'        Log("request for historical data recieved, duration " & duration.TotalDays & " days")
'        totalHistoricalBarData(id) = New List(Of BarData)
'        currentHistoricalBarData(id) = New List(Of BarData)
'        Dim incrementInDays As Double = 100000
'        Dim minuteDuration As Double = duration.TotalMinutes
'        Select Case BarSize(id)
'            Case BarSize.OneSecond
'                incrementInDays = 0.01
'            Case BarSize.FiveSeconds
'                incrementInDays = 0.11
'            Case BarSize.FifteenSeconds
'                incrementInDays = 0.3
'            Case BarSize.ThirtySeconds
'                incrementInDays = 0.8
'            Case BarSize.OneMinute
'                incrementInDays = 3
'            Case BarSize.FiveMinutes
'                incrementInDays = 5
'            Case BarSize.FifteenMinutes
'                incrementInDays = 10
'            Case BarSize.ThirtyMinutes
'                incrementInDays = 15
'            Case BarSize.OneHour
'                incrementInDays = 20
'            Case BarSize.OneDay
'                incrementInDays = 364
'            Case BarSize.OneMonth
'                incrementInDays = 364
'            Case BarSize.OneYear
'                incrementInDays = 364
'        End Select
'        incrementInDays *= 24 * 60
'        readBeginDate(id) = True
'        If minuteDuration > incrementInDays Then
'            Dim remainder As Double = minuteDuration Mod incrementInDays
'            historicalIncrement(id) = TimeSpan.FromMinutes(incrementInDays)
'            maxIncrements(id) = Ceiling(duration.TotalMinutes / historicalIncrement(id).TotalMinutes)
'            isHistoricalDataIncrementsLoaded(id) = False
'            If Round(remainder, 1) = 0 Then
'                Log("calling data for " & endDate.ToShortDateString & ", " & endDate.ToShortTimeString & "; duration " & TimeSpan.FromMinutes(incrementInDays).TotalDays & " days")
'                RequestIBHistoricalDataDirect(endDate, TimeSpan.FromMinutes(incrementInDays), id)
'                'RequestHistoricalData(id, Contract(id), endDate, TimeSpan.FromMinutes(incrementInDays), BarSize(id), WhatToShow(id), 0)
'            Else
'                Log("calling data for " & endDate.ToShortDateString & ", " & endDate.ToShortTimeString & "; duration " & TimeSpan.FromMinutes(remainder).TotalDays & " days")
'                RequestIBHistoricalDataDirect(endDate, TimeSpan.FromMinutes(remainder), id)
'                'RequestHistoricalData(id, Contract(id), endDate, TimeSpan.FromMinutes(remainder), BarSize(id), WhatToShow(id), 0)
'            End If
'        Else
'            Log("calling data for " & endDate.ToShortDateString & ", " & endDate.ToShortTimeString & "; duration " & duration.TotalDays & " days")
'            isHistoricalDataIncrementsLoaded(id) = True
'            RequestIBHistoricalDataDirect(endDate, duration, id)
'        End If
'    End Sub
'    ' Direct call to IB for data
'    Public Sub RequestIBHistoricalDataDirect(ByVal endDate As Date, ByVal duration As TimeSpan, ByVal id As Integer)
'        If duration.TotalSeconds < 60 Then
'            MyBase.RequestHistoricalData(id, Contract(id), endDate, CInt(duration.TotalSeconds) & " S", BarSize(id), WhatToShow(id), If(UseRegularTradingHours(id), 1, 0))
'        Else
'            MyBase.RequestHistoricalData(id, Contract(id), endDate, duration, BarSize(id), WhatToShow(id), If(UseRegularTradingHours(id), 1, 0))
'        End If
'    End Sub

'    Private Sub HistDataBarRecieved(ByVal sender As Object, ByVal e As HistoricalDataEventArgs) Handles MyBase.HistoricalData
'        Dim bar As BarData = New BarData(e.Open, e.High, e.Low, e.Close, e.Date, TimeSpanFromBarSize(BarSize(e.RequestId)))
'        If CanRecieveData(e.Date, e.RequestId) Then
'            If readBeginDate(e.RequestId) Then
'                historicalDataBeginDate(e.RequestId) = e.Date
'                readBeginDate(e.RequestId) = False
'            End If
'            currentHistoricalBarData(e.RequestId).Add(bar)

'            IBBarCameIn(e.RequestId, bar)

'        End If
'        histTimer(e.RequestId).Change(1200, -1)
'    End Sub

'    Private Sub HistoricalDataSectionCompleted(ByVal state As Object)
'        Dim id As Integer = state
'        If isHistoricalDataIncrementsLoaded(id) Then
'            histTimer(id).Change(-1, -1)
'            currentHistoricalBarData(id).Reverse()
'            totalHistoricalBarData(id).AddRange(currentHistoricalBarData(id))
'            currentHistoricalBarData(id).Clear()
'            totalHistoricalBarData(id).Reverse()

'            Log("historical data section finished")

'            IBHistoricalDataFinished(id, totalHistoricalBarData(id).GetRange(0, totalHistoricalBarData(id).Count - 1 - Min(1, totalHistoricalBarData(id).Count - 1)))

'            totalHistoricalBarData(id).Clear()
'        Else
'            currentHistoricalBarData(id).Reverse()
'            totalHistoricalBarData(id).AddRange(currentHistoricalBarData(id))
'            currentHistoricalBarData(id).Clear()
'            Log("historical data finished, now calling data for " & historicalDataBeginDate(id).ToShortDateString & ", " & historicalDataBeginDate(id).ToShortTimeString & "; duration " & historicalIncrement(id).TotalDays & " days")
'            RaiseEvent HistoricalDataPartiallyCompleted(id, incrementCount(id) / maxIncrements(id))
'            incrementCount(id) += 1
'            readBeginDate(id) = True
'            RequestIBHistoricalDataDirect(historicalDataBeginDate(id), historicalIncrement(id), id)
'            If incrementCount(id) >= maxIncrements(id) Then isHistoricalDataIncrementsLoaded(id) = True
'        End If
'        histTimer(id).Change(-1, -1)
'    End Sub
'    Private Sub IBHistoricalDataFinished(ByVal id As Integer, ByVal data As List(Of BarData))
'        For Each item In data
'            RaiseEvent HistoricalDataBarRecieved(item, id)
'        Next
'        RaiseEvent HistoricalDataCompleted(id)
'        incrementCount(id) = 1
'    End Sub

'    Private Sub IBBarCameIn(ByVal id As Integer, ByVal bar As BarData)
'        'for caching
'        CacheBar(bar, id)
'    End Sub

'    Private Sub MarketData(ByVal sender As Object, ByVal e As TickPriceEventArgs) Handles MyBase.TickPrice
'        If CanRecieveData(Now, e.TickerId) Then
'            If e.TickType = TickType.LastPrice Then
'                'If lastPrice(e.TickerId) = 0 Then tickTimer(e.TickerId).Change(CLng(TimeSpanFromBarSize(BarSize(e.TickerId)).TotalMilliseconds), CLng(TimeSpanFromBarSize(BarSize(e.TickerId)).TotalMilliseconds))
'                lastPrice(e.TickerId) = e.Price
'            End If
'            RaiseEvent MarketPriceData(e.TickType, e.Price, e.TickerId)
'        End If
'    End Sub


'    Private Function NullCall() As KeyValuePair(Of DateTime, TimeSpan)
'        Return New KeyValuePair(Of Date, TimeSpan)(Date.MinValue, TimeSpan.MinValue)
'    End Function

'    Public Overloads Sub CancelMarketData(ByVal id As Integer) Implements DataStream.CancelMarketData
'        lastPrice(id) = 0
'        MyBase.CancelMarketData(id)
'    End Sub
'    Public Overloads Sub RequestMarketData(ByVal id As Integer) Implements DataStream.RequestMarketData
'        lastPrice(id) = 0
'        MyBase.RequestMarketData(CInt(id), Contract(id), DefaultTickTypes, False, False)
'    End Sub

'    Public Event HistoricalDataBarRecieved(ByVal bar As BarData, ByVal id As Integer) Implements DataStream.HistoricalDataBarRecieved
'    Public Event HistoricalDataCompleted(ByVal id As Integer) Implements DataStream.HistoricalDataCompleted
'    Public Event HistoricalDataPartiallyCompleted(ByVal id As Integer, ByVal percentageCompleted As Double) Implements DataStream.HistoricalDataPartiallyCompleted
'    Public Event MarketPriceData(ByVal tickType As TickType, ByVal price As Double, ByVal id As Integer) Implements DataStream.MarketPriceData

'    Private Sub CacheBar(ByVal bar As BarData, ByVal id As Integer)
'        Dim file As String = GetCacheFileName(id)
'        My.Computer.FileSystem.WriteAllText(file, (New BarDataConverter).ConvertToString(bar) & vbNewLine & My.Computer.FileSystem.ReadAllText(file), False)
'        'Dim writer As New StreamWriter(CacheFileName(id), True)
'    End Sub
'    Private Function ReadBarsFromFile(ByVal file As String) As List(Of BarData)
'        'Return New List(Of BarData)
'        Dim reader As New StreamReader(file)
'        Dim bars As New Dictionary(Of Date, BarData)
'        Dim converter As New BarDataConverter
'        While reader.Peek <> -1
'            Dim line As String = reader.ReadLine
'            line = line.Trim
'            If line <> "" Then
'                Dim data As BarData = converter.ConvertFromString(line)
'                If Not bars.ContainsKey(data.Date) Then
'                    bars.Add(data.Date, data)
'                End If
'            End If
'        End While
'        reader.Close()
'        Dim lst = bars.ToList
'        lst.Sort(New Comparison(Of KeyValuePair(Of Date, BarData))(
'              Function(item1 As KeyValuePair(Of Date, BarData), item2 As KeyValuePair(Of Date, BarData)) As Boolean
'                  Return item1.Key < item2.Key
'              End Function)
'        )
'        Dim dic As New List(Of BarData)
'        For Each item In lst
'            dic.Add(item.Value)
'        Next
'        Return dic
'    End Function

'    Private Function GetCacheFileName(ByVal id As Integer) As String
'        If Not Directory.Exists(AppDomain.CurrentDomain.BaseDirectory & "cache") Then Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory & "cache")
'        If Not Directory.Exists(AppDomain.CurrentDomain.BaseDirectory & "cache\" & Contract(id).Symbol) Then Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory & "cache\" & Contract(id).Symbol)
'        Return AppDomain.CurrentDomain.BaseDirectory & "cache\" & Contract(id).Symbol & "\" & [Enum].GetName(GetType(BarSize), BarSize(id)) & ".dat"
'    End Function

'End Class

'Public Class RandomDataStream
'    Implements DataStream

'    Private tmr As New DispatcherTimer()
'    Private _receiveData As Boolean
'    Private _speed As Double = 1
'    Public Property RandomDeflection As Double = 0.5
'    Public Property RandomMinMove As Double = 0.25
'    Public Property RandomMaxMove As Double = 0.25
'    Public Property RandomSpeed As Double
'        Get
'            Return _speed
'        End Get
'        Set(ByVal value As Double)
'            _speed = value
'            If RandomSpeed > 0.000001 Then tmr.Interval = TimeSpan.FromSeconds((Rnd() + 0.25) / (RandomSpeed / 2)) Else tmr.Interval = TimeSpan.FromMilliseconds(Integer.MaxValue)
'        End Set
'    End Property
'    Private _randomPrice As Double = RoundTo((Int(Rnd() * 101) - 50 + 1000), RandomMinMove)
'    Public Property RandomPrice As Double
'        Get
'            Return RoundTo(_randomPrice, RandomMinMove)
'        End Get
'        Set(ByVal value As Double)
'            _randomPrice = value
'            RaiseEvent MarketPriceData(TickType.LastPrice, _randomPrice, 0)
'        End Set
'    End Property
'    Private Function GetRandomPriceMove() As Double
'        'Dim num As Double = Rnd() * (RandomMaxMove - RandomMinMove) + RandomMinMove
'        'num *= RandomChoice(-1, 1, RandomDeflection) ' Floor(Rnd() * 3) - 1 ' make it positive or negative
'        Dim nums(0) As Double
'        For i = 0 To 0
'            nums(i) = Rnd() * (RandomMaxMove - RandomMinMove) + RandomMinMove
'            nums(i) *= RandomChoice(-1, 1, RandomDeflection)
'        Next
'        Return nums.Average
'    End Function
'    Private Sub tmr_Tick(ByVal sender As Object, ByVal e As EventArgs)
'        If RandomSpeed > 0.000001 Then tmr.Interval = TimeSpan.FromSeconds((Rnd() + 0.25) / (RandomSpeed / 2)) Else tmr.Interval = TimeSpan.FromMilliseconds(Integer.MaxValue)
'        _randomPrice += GetRandomPriceMove()
'        If _receiveData Then
'            RaiseEvent MarketPriceData(TickType.LastPrice, RandomPrice, 0)
'        End If
'    End Sub
'    Public Sub New()
'        Randomize()
'        tmr.IsEnabled = True
'        tmr.Interval = TimeSpan.FromSeconds(RandomSpeed)
'        AddHandler tmr.Tick, AddressOf tmr_Tick
'    End Sub

'    Public Event HistoricalDataBarRecieved(ByVal bar As BarData, ByVal id As Integer) Implements DataStream.HistoricalDataBarRecieved
'    Public Event HistoricalDataCompleted(ByVal id As Integer) Implements DataStream.HistoricalDataCompleted
'    Public Event HistoricalDataPartiallyCompleted(ByVal id As Integer, ByVal percentCompleted As Double) Implements DataStream.HistoricalDataPartiallyCompleted
'    Public Event MarketPriceData(ByVal tickType As TickType, ByVal price As Double, ByVal id As Integer) Implements DataStream.MarketPriceData

'    Public Sub CancelHistoricalData(ByVal id As Integer) Implements DataStream.CancelHistoricalData
'    End Sub
'    Public Sub CancelMarketData(ByVal id As Integer) Implements DataStream.CancelMarketData
'        _receiveData = False
'    End Sub
'    Public Sub RequestHistoricalData(ByVal endDate As Date, ByVal duration As System.TimeSpan, ByVal id As Integer) Implements DataStream.RequestHistoricalData
'        Dim time As DateTime = endDate - duration
'        Dim val As Double = 1
'        Dim counter As Integer
'        Dim total As Integer = CInt(duration.TotalMinutes / TimeSpanFromBarSize(BarSize(id)).TotalMinutes)
'        While time < endDate
'            RaiseEvent HistoricalDataBarRecieved(New BarData(_randomPrice, _randomPrice + Abs(GetRandomPriceMove), _randomPrice - Abs(GetRandomPriceMove), _randomPrice, time, TimeSpanFromBarSize(BarSize(id))), id)
'            If counter Mod 100 = 0 Then RaiseEvent HistoricalDataPartiallyCompleted(id, counter / total)
'            _randomPrice += GetRandomPriceMove()
'            _randomPrice = RoundTo(_randomPrice, RandomMinMove)
'            time = time.Add(TimeSpanFromBarSize(BarSize(id)))
'            counter += 1
'        End While
'        RaiseEvent HistoricalDataCompleted(id)
'    End Sub
'    Public Sub RequestMarketData(ByVal id As Integer) Implements DataStream.RequestMarketData
'        _receiveData = True
'    End Sub
'    Private _barSize As BarSize = BarSize.OneMinute
'    Public Property BarSize(ByVal id As Integer) As BarSize Implements DataStream.BarSize
'        Get
'            Return _barSize
'        End Get
'        Set(ByVal value As BarSize)
'            _barSize = value
'        End Set
'    End Property

'End Class

'Public Interface DataStream
'    Sub RequestMarketData(ByVal id As Integer)
'    Sub CancelMarketData(ByVal id As Integer)
'    Event MarketPriceData(ByVal tickType As TickType, ByVal price As Double, ByVal id As Integer)
'    Event HistoricalDataBarRecieved(ByVal bar As BarData, ByVal id As Integer)
'    Event HistoricalDataCompleted(ByVal id As Integer)
'    Event HistoricalDataPartiallyCompleted(ByVal id As Integer, ByVal percentCompleted As Double)
'    Sub RequestHistoricalData(ByVal endDate As Date, ByVal duration As TimeSpan, ByVal id As Integer)
'    Sub CancelHistoricalData(ByVal id As Integer)
'    Property BarSize(ByVal id As Integer) As BarSize
'End Interface


''0 =  
''1 = 
''2 = 
''3 = 
''4 = 
''5 = 
''6 = 
''7 = 
''8 = 
''9 = 	
''10 = 

''11 = 
''12 = 
''13 = 

''14 = 
''15 = 
''16 = 
''17 = 
''18 = 
''19 = 
''20 = 
''21 = 
''22 = 
''23 = 
''24 = 
''25 = 
''26 = 
''27 = 
''28 = 
''29 = 
''30 = 
''31 = 
''32 =  
''33 = !
''34 = "
''35 = #
''36 = $
''37 = %
''38 = &
''39 = '
''40 = (
''41 = )
''42 = *
''43 = +
''44 = ,
''45 = -
''46 = .
''47 = /
''48 = 0
''49 = 1
''50 = 2
''51 = 3
''52 = 4
''53 = 5
''54 = 6
''55 = 7
''56 = 8
''57 = 9
''58 = :
''59 = ;
''60 = <
''61 = =
''62 = >
''63 = ?
''64 = @
''65 = A
''66 = B
''67 = C
''68 = D
''69 = E
''70 = F
''71 = G
''72 = H
''73 = I
''74 = J
''75 = K
''76 = L
''77 = M
''78 = N
''79 = O
''80 = P
''81 = Q
''82 = R
''83 = S
''84 = T
''85 = U
''86 = V
''87 = W
''88 = X
''89 = Y
''90 = Z
''91 = [
''92 = \
''93 = ]
''94 = ^
''95 = _
''96 = `
''97 = a
''98 = b
''99 = c
''100 = d
''101 = e
''102 = f
''103 = g
''104 = h
''105 = i
''106 = j
''107 = k
''108 = l
''109 = m
''110 = n
''111 = o
''112 = p
''113 = q
''114 = r
''115 = s
''116 = t
''117 = u
''118 = v
''119 = w
''120 = x
''121 = y
''122 = z
''123 = {
''124 = |
''125 = }
''126 = ~
''127 = 
''128 = €
''129 = 
''130 = ‚
''131 = ƒ
''132 = „
''133 = …
''134 = †
''135 = ‡
''136 = ˆ
''137 = ‰
''138 = Š
''139 = ‹
''140 = Œ
''141 = 
''142 = Ž
''143 = 
''144 = 
''145 = ‘
''146 = ’
''147 = “
''148 = ”
''149 = •
''150 = –
''151 = —
''152 = ˜
''153 = ™
''154 = š
''155 = ›
''156 = œ
''157 = 
''158 = ž
''159 = Ÿ
''160 =  
''161 = ¡
''162 = ¢
''163 = £
''164 = ¤
''165 = ¥
''166 = ¦
''167 = §
''168 = ¨
''169 = ©
''170 = ª
''171 = «
''172 = ¬
''173 = ­
''174 = ®
''175 = ¯
''176 = °
''177 = ±
''178 = ²
''179 = ³
''180 = ´
''181 = µ
''182 = ¶
''183 = ·
''184 = ¸
''185 = ¹
''186 = º
''187 = »
''188 = ¼
''189 = ½
''190 = ¾
''191 = ¿
''192 = À
''193 = Á
''194 = Â
''195 = Ã
''196 = Ä
''197 = Å
''198 = Æ
''199 = Ç
''200 = È
''201 = É
''202 = Ê
''203 = Ë
''204 = Ì
''205 = Í
''206 = Î
''207 = Ï
''208 = Ð
''209 = Ñ
''210 = Ò
''211 = Ó
''212 = Ô
''213 = Õ
''214 = Ö
''215 = ×
''216 = Ø
''217 = Ù
''218 = Ú
''219 = Û
''220 = Ü
''221 = Ý
''222 = Þ
''223 = ß
''224 = à
''225 = á
''226 = â
''227 = ã
''228 = ä
''229 = å
''230 = æ
''231 = ç
''232 = è
''233 = é
''234 = ê
''235 = ë
''236 = ì
''237 = í
''238 = î
''239 = ï
''240 = ð
''241 = ñ
''242 = ò
''243 = ó
''244 = ô
''245 = õ
''246 = ö
''247 = ÷
''248 = ø
''249 = ù
''250 = ú
''251 = û
''252 = ü
''253 = ý
''254 = þ
''255 = ÿ
