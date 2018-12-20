Imports System.Reflection

Namespace AnalysisTechniques
    Public Class Statistics
#Region "AnalysisTechnique Inherited Code"
        Inherits AnalysisTechnique

        ' Inherit the one-argument constructor from the base class.
        Public Sub New(ByVal chart As Chart)
            MyBase.New(chart) ' Call the base class constructor.
        End Sub
#End Region
        Friend Class Swing
            Public Property twoXReachedUp As Boolean
            Public Property twoXReachedDown As Boolean
            Public Property twoXExtendedU As Boolean
            Public Property twoXExtendedDown As Boolean
            Public Property twoXReversedU As Boolean
            Public Property twoXReversedDown As Boolean
            Protected _tl As TrendLine
            Public ReadOnly Property TL As TrendLine
                Get
                    Return _tl
                End Get
            End Property
            Public Property Direction As Boolean
            Public Sub New(ByVal tl As TrendLine, ByVal direction As Boolean)
                _tl = tl
                StartBar = tl.StartPoint.X
                StartPrice = tl.StartPoint.Y
                EndBar = tl.EndPoint.X
                EndPrice = tl.EndPoint.Y
                Me.Direction = direction
            End Sub
            Public Property StartBar As Integer
                Get
                    Return TL.StartPoint.X
                End Get
                Set(ByVal value As Integer)
                    TL.StartPoint = New Point(value, TL.StartPoint.Y)
                End Set
            End Property
            Public Property StartPrice As Decimal
                Get
                    Return TL.StartPoint.Y
                End Get
                Set(ByVal value As Decimal)
                    TL.StartPoint = New Point(TL.StartPoint.X, value)
                End Set
            End Property
            Public Property EndBar As Integer
                Get
                    Return TL.EndPoint.X
                End Get
                Set(ByVal value As Integer)
                    TL.EndPoint = New Point(value, TL.EndPoint.Y)
                End Set
            End Property
            Public Property EndPrice As Decimal
                Get
                    Return TL.EndPoint.Y
                End Get
                Set(ByVal value As Decimal)
                    TL.EndPoint = New Point(TL.EndPoint.X, value)
                End Set
            End Property
            Public Property ChannelDirection As ChannelDirectionType = ChannelDirectionType.Neutral
            Public Enum ChannelDirectionType
                Up = -1
                Down = 0
                Neutral = 1
            End Enum
            Public Shared Operator =(ByVal swing1 As Swing, ByVal swing2 As Swing) As Boolean
                Return swing1 Is swing2
            End Operator
            Public Shared Operator <>(ByVal swing1 As Swing, ByVal swing2 As Swing) As Boolean
                Return swing1 IsNot swing2
            End Operator
            Public HasLiveOrder As Boolean
            Public OrderLine As TrendLine
            Public DoneTrading As Boolean
            Public Profit As Double
        End Class
        <Input("")>
        Public Property RV As Double = 2
        <Input("")>
        Public Property PrintData As Boolean = True
        <Input("")>
        Public Property AllowActualTrading As Boolean = False
        <Input("")>
        Public Property EnableSelling As Boolean = True
        <Input("")>
        Public Property EnableBuying As Boolean = True
        '<Input("")>
        'Public Property SwingLengthToPlaceOrderMultiplier As Decimal = 2
        '<Input("")>
        'Public Property ReverseMarginOnStraightSwing As Decimal = 0.5
        '<Input("")>
        'Public Property StopPriceDifferenceMultiplier As Decimal = 1
        '<Input("")>
        'Public Property FillPriceDifferenceMultiplier As Decimal = 2.1
        Dim swings As List(Of Swing)
        Dim swingDir As Boolean
        Protected Overrides Sub Begin()
            MyBase.Begin()

            Chart.dontDrawBarVisuals = False
            'Exit Sub
            swingDir = False
            swings = New List(Of Swing)
            swings.Add(New Swing(NewTrendLine(Colors.Gray, New Point(CurrentBar.Number, CurrentBar.Close), New Point(CurrentBar.Number, CurrentBar.Close), True), False))
            Description = "A statstical data gathering analysis technique."
            times2wasHit = 0
            times2wasReversed = 0
            times2wasExtended = 0

            lastBuyPrice = 0
            buyStopPrice = 0
            currentBuyExtendPrice = 0
            isLong = False

            lastSellPrice = 0
            sellStopPrice = 0
            currentSellExtendPrice = 0
            isShort = False

            profit = 0
            numTrades = 0
            openOrders = New List(Of Order)
            tickChanges = New List(Of Integer)
            prevTick = CurrentBar.Close
            margin = 10
        End Sub
        Private Property CurrentSwing As Swing
            Get
                Return swings(swings.Count - 1)
            End Get
            Set(ByVal value As Swing)
                swings(swings.Count - 1) = value
            End Set
        End Property
        Dim times2wasHit As Integer
        Dim times2wasReversed As Integer
        Dim times2wasExtended As Integer


        Dim lastBuyPrice As Decimal
        Dim buyStopPrice As Decimal
        Dim currentBuyExtendPrice As Decimal
        Dim isLong As Boolean

        Dim lastSellPrice As Decimal
        Dim isShort As Boolean
        Dim sellStopPrice As Decimal
        Dim currentSellExtendPrice As Decimal

        Dim profit As Decimal
        Dim numTrades As Integer
        Dim margin As Integer = 100
        Dim tickChanges As List(Of Integer)
        Private openOrders As List(Of Order)
        Dim prevTick As Decimal
        Protected Overrides Sub Main()
            If CurrentBar.Number > 1 Then
                tickChanges.Add(CurrentBar.Close - prevTick)
                prevTick = CurrentBar.Close
            End If
            'If CurrentBar.Number > margin Then
            '    If Chart.IsNewBar Then
            '        Dim total As Integer
            '        For i = CurrentBar.Number - 1 - margin To CurrentBar.Number - 2
            '            total += tickChanges(i)
            '        Next
            '        Dim c As Color
            '        If total < 0 Then
            '            c = Colors.Red
            '        Else
            '            c = Colors.Green
            '        End If
            '        'Chart.bars(Chart.bars.Count - 1).Data.Close
            '        NewTrendLine(c, New Point(CurrentBar.Number, 2792), New Point(CurrentBar.Number, 2792 + total / 2))
            '    End If
            'End If
            'If isLong And EnableBuying Then
            '    Dim n As Decimal = If(IsLastBarOnChart And Chart.BidPrice <> 0, Chart.BidPrice, CurrentBar.Close)
            '    If n <= buyStopPrice Then
            '        CloseBuy()
            '    End If
            '    If n > currentBuyExtendPrice - ReverseMarginOnStraightSwing Then
            '        currentBuyExtendPrice = n
            '        'buyStopPrice = currentBuyExtendPrice - StopPriceDifferenceMultiplier * RV
            '    ElseIf n >= lastBuyPrice + RV * FillPriceDifferenceMultiplier Then
            '        CloseBuy()
            '    End If
            'End If

            'If isShort And EnableSelling Then
            '    Dim n As Decimal = If(IsLastBarOnChart And Chart.AskPrice <> 0, Chart.AskPrice, CurrentBar.Close)
            '    If n >= sellStopPrice Then
            '        CloseSell()
            '    End If
            '    If n < currentSellExtendPrice + ReverseMarginOnStraightSwing Then
            '        currentSellExtendPrice = n
            '        'sellStopPrice = currentSellExtendPrice + StopPriceDifferenceMultiplier * RV
            '    ElseIf n <= lastSellPrice - RV * FillPriceDifferenceMultiplier Then
            '        CloseSell()
            '    End If
            'End If


            Dim swngEvent As Boolean = False

            If CurrentBar.High - CurrentSwing.EndPrice >= RV AndAlso CurrentBar.Number <> CurrentSwing.EndBar AndAlso swingDir = False Then
                'If CurrentSwing.twoXReachedUp And Not CurrentSwing.twoXExtendedUp Then
                '    times2wasReversed += 1
                '    CurrentSwing.twoXReversedUp = True
                '    NewLabel("^", Colors.White, New Point(CurrentBar.Number, CurrentBar.Close))
                'End If
                'new swing up
                swingDir = True
                swings.Add(New Swing(NewTrendLine(Colors.Gray, New Point(CurrentSwing.EndBar, CurrentSwing.EndPrice), New Point(CurrentBar.Number, CurrentBar.High), True), True))
                swngEvent = True
            ElseIf CurrentSwing.EndPrice - CurrentBar.Low >= RV AndAlso CurrentBar.Number <> CurrentSwing.EndBar AndAlso swingDir = True Then
                'If CurrentSwing.twoXReachedUp And Not CurrentSwing.twoXExtendedU Then
                '    times2wasReversed += 1
                '    CurrentSwing.twoXReversedU = True
                '    NewLabel("v", Colors.White, New Point(CurrentBar.Number, CurrentBar.Close))
                'End If
                ' new swing down
                swingDir = False
                swings.Add(New Swing(NewTrendLine(Colors.Gray, New Point(CurrentSwing.EndBar, CurrentSwing.EndPrice), New Point(CurrentBar.Number, CurrentBar.Low), True), True))
                swngEvent = True
            ElseIf CurrentBar.High >= CurrentSwing.EndPrice And swingDir = True Then
                ' extension up
                CurrentSwing.EndBar = CurrentBar.Number
                CurrentSwing.EndPrice = CurrentBar.High
                swngEvent = True
                'If Abs(CurrentSwing.EndPrice - CurrentSwing.StartPrice) >= SwingLengthToPlaceOrderMultiplier * RV And Not CurrentSwing.twoXReachedUp And EnableSelling Then
                '    SimpleSell()
                '    CurrentSwing.twoXReachedUp = True
                '    times2wasHit += 1
                '    numTrades += 1
                'End If

                'If Abs(CurrentSwing.StartPrice - CurrentSwing.EndPrice) >= (SwingLengthToPlaceOrderMultiplier + 1) * RV And Not CurrentSwing.twoXExtendedU Then
                '    times2wasExtended += 1
                '    CurrentSwing.twoXExtendedU = True
                '    NewLabel("^", Colors.White, New Point(CurrentBar.Number, CurrentBar.Close))
                'End If
            ElseIf CurrentBar.Low <= CurrentSwing.EndPrice And swingDir = False Then
                ' extension down
                CurrentSwing.EndBar = CurrentBar.Number
                CurrentSwing.EndPrice = CurrentBar.Low
                swngEvent = True
                'If Abs(CurrentSwing.EndPrice - CurrentSwing.StartPrice) >= SwingLengthToPlaceOrderMultiplier * RV And Not CurrentSwing.twoXReachedDown And EnableBuying Then
                '    SimpleBuy()
                '    CurrentSwing.twoXReachedDown = True
                '    times2wasHit += 1
                '    numTrades += 1
                'End If

                'If Abs(CurrentSwing.StartPrice - CurrentSwing.EndPrice) >= (SwingLengthToPlaceOrderMultiplier + 1) * RV And Not CurrentSwing.twoXExtendedUp Then
                '    times2wasExtended += 1
                '    CurrentSwing.twoXExtendedUp = True
                '    NewLabel("v", Colors.White, New Point(CurrentBar.Number, CurrentBar.Close))
                'End If


            End If
            If swngEvent And swings.Count > 1 Then
                Dim p = Abs(swings(swings.Count - 1).EndPrice - swings(swings.Count - 1).StartPrice) / Abs(swings(swings.Count - 2).EndPrice - swings(swings.Count - 2).StartPrice)
                If p > 0.3 And Not swings(swings.Count - 1).HasLiveOrder Then
                    swings(swings.Count - 1).HasLiveOrder = True
                    If swingDir Then
                        swings(swings.Count - 1).OrderLine = NewTrendLine(Colors.Green, New Point(CurrentBar.Number, CurrentBar.Close), New Point(CurrentBar.Number - 2, CurrentBar.Close))
                    Else
                        swings(swings.Count - 1).OrderLine = NewTrendLine(Colors.Red, New Point(CurrentBar.Number, CurrentBar.Close), New Point(CurrentBar.Number - 2, CurrentBar.Close))
                    End If
                ElseIf p >= 0.6 And swings(swings.Count - 1).HasLiveOrder And Not swings(swings.Count - 1).DoneTrading Then
                    swings(swings.Count - 1).DoneTrading = True
                    swings(swings.Count - 1).OrderLine.EndPoint = New Point(CurrentBar.Number, CurrentBar.Close)
                    swings(swings.Count - 1).Profit = (swings(swings.Count - 1).OrderLine.EndPoint.Y - swings(swings.Count - 1).OrderLine.StartPoint.Y) * If(swingDir, 1, -1)
                End If
                If swings(swings.Count - 2).DoneTrading = False And swings(swings.Count - 2).OrderLine IsNot Nothing Then
                    swings(swings.Count - 2).DoneTrading = True
                    swings(swings.Count - 2).OrderLine.EndPoint = New Point(CurrentBar.Number, CurrentBar.Close)
                    swings(swings.Count - 2).Profit = (swings(swings.Count - 2).OrderLine.EndPoint.Y - swings(swings.Count - 2).OrderLine.StartPoint.Y) * If(swingDir, -1, 1)
                End If
            End If
            If IsLastBarOnChart Then
                Dim total As Double
                For i = 0 To swings.Count - 1
                    total += swings(i).Profit
                Next
                Log(total & " with " & swings.Count & " swings. avg: " & Round(total / swings.Count, 4))
            End If
            'If IsLastBarOnChart And PrintData Then
            '    Dim foundWindow As Boolean
            '    For Each item In Application.Current.Windows
            '        If TypeOf item Is LogWindow Then
            '            foundWindow = True
            '            CType(item, LogWindow).Show()
            '            CType(item, LogWindow).Clear()
            '            Exit For
            '        End If
            '    Next
            '    Dim x1 As Decimal
            '    Dim x125 As Decimal
            '    Dim x15 As Decimal
            '    Dim x175 As Decimal
            '    Dim x2 As Decimal
            '    Dim x225 As Decimal
            '    Dim x25 As Decimal
            '    Dim x275 As Decimal
            '    Dim x3 As Decimal
            '    Dim x325 As Decimal
            '    Dim x35 As Decimal
            '    Dim l As New Dictionary(Of Decimal, Integer)
            '    Dim total As Decimal = 0
            '    Dim totalRev As Decimal = 0
            '    Dim totalExt As Decimal = 0
            '    Dim totalExtCount As Integer
            '    Dim totalRevCount As Integer
            '    For i = 1 To swings.Count - 1
            '        Dim swing = swings(i)
            '        'If swing.twoXExtendedUp Then
            '        '    totalExt += Abs(swing.StartPrice - swing.EndPrice) - (SwingLengthToPlaceOrderMultiplier + 1) * RV
            '        '    totalExtCount += 1
            '        'End If
            '        'If swing.twoXReversedUp Then
            '        '    totalRev += Abs(swing.StartPrice - swing.EndPrice)
            '        '    totalRevCount += 1
            '        'End If
            '        total += Abs(swings(i - 1).EndPrice - swings(i - 1).StartPrice)
            '        'If Abs(swings(i - 1).EndPrice - swings(i - 1).StartPrice) = 2 Then
            '        Dim n = Abs(swing.StartPrice - swing.EndPrice) / RV
            '        If l.ContainsKey(n) Then
            '            l(n) += 1
            '        Else
            '            l.Add(n, 1)
            '        End If
            '        'End If
            '        If Abs(swings(i).EndPrice - swings(i).StartPrice) = 1 Then x1 += 1
            '        If Abs(swings(i).EndPrice - swings(i).StartPrice) = 1.25 Then x125 += 1
            '        If Abs(swings(i).EndPrice - swings(i).StartPrice) = 1.5 Then x15 += 1
            '        If Abs(swings(i).EndPrice - swings(i).StartPrice) = 1.75 Then x175 += 1
            '        If Abs(swings(i).EndPrice - swings(i).StartPrice) = 2 Then x2 += 1
            '        If Abs(swings(i).EndPrice - swings(i).StartPrice) = 2.25 Then x225 += 1
            '        If Abs(swings(i).EndPrice - swings(i).StartPrice) = 2.5 Then x25 += 1
            '        If Abs(swings(i).EndPrice - swings(i).StartPrice) = 2.75 Then x275 += 1
            '        If Abs(swings(i).EndPrice - swings(i).StartPrice) = 3 Then x3 += 1
            '        If Abs(swings(i).EndPrice - swings(i).StartPrice) = 3.25 Then x325 += 1
            '        If Abs(swings(i).EndPrice - swings(i).StartPrice) = 3.5 Then x35 += 1

            '    Next
            '    'If 
            '    Log("average: " & total / swings.Count)
            '    'Log("1  : " & x1)
            '    'Log("125: " & x125)
            '    'Log("15 : " & x15)
            '    'Log("175: " & x175)
            '    'Log("2  : " & x2)
            '    'Log("225: " & x225)
            '    'Log("25 : " & x25)
            '    'Log("275: " & x275)
            '    'Log("3  : " & x3)
            '    'Log("325: " & x325)
            '    'Log("35 : " & x35)
            '    Log("reversed: " & totalRev)
            '    Log("extended: " & totalExt)
            '    If totalRevCount <> 0 Then Log("reversed avg: " & totalRev / totalRevCount)
            '    If totalExtCount <> 0 Then Log("extended avg: " & totalExt / totalExtCount)
            '    Log("commission: " & ToDollar(4.02 * numTrades / 20))
            '    Log("gross profit: " & ToDollar(profit))
            '    Log("net profit: " & ToDollar((profit - 4.02 * numTrades / 20)))
            '    Log("number of transactions: " & numTrades)
            '    If numTrades <> 0 Then Log("transaction profit rate: " & ToDollar((profit - 4.02 * numTrades / 20) / numTrades))
            '    If numTrades <> 0 Then Log("transaction rate: a transaction every " & Round((CurrentBar.Date - Chart.bars(0).Data.Date).TotalMinutes / numTrades, 0) & " minutes")
            '    Log("time period: " & (CurrentBar.Date - Chart.bars(0).Data.Date).Days & " days, " & (CurrentBar.Date - Chart.bars(0).Data.Date).Hours & " hours")
            '    Log("rate of profit change: " & ToDollar((profit - 4.02 * numTrades / 20) / (CurrentBar.Date - Chart.bars(0).Data.Date).TotalDays) & " per day")

            '    Log("bar count: " & CurrentBar.Number)
            '    Log("times " & SwingLengthToPlaceOrderMultiplier & " was hit: " & times2wasHit)
            '    Log("times " & SwingLengthToPlaceOrderMultiplier & " was extended: " & times2wasExtended)
            '    Log("times " & SwingLengthToPlaceOrderMultiplier & " was reversed: " & times2wasReversed)
            '    Log("odds of winning: " & percentage(times2wasReversed, times2wasHit))
            '    Log("odds of losing: " & percentage(times2wasExtended, times2wasHit))
            '    Dim keys = (From entry In l Order By entry.Key Descending Select entry)
            '    'Log("keys**************")
            '    For Each item In keys
            '        'Log(item.Key & "x: " & percentage(item.Value, swings.Count))
            '    Next
            '    Dim vals = (From entry In l Order By entry.Value Descending Select entry)
            '    'Log("values**************")
            '    For Each item In vals
            '        'Log(item.Key & "x: " & percentage(item.Value, swings.Count))
            '    Next
            '    'Dim index As Integer = 1
            '    'Dim barWidthCount As Integer
            '    'Dim rangeCount As Double
            '    'For Each item In swings
            '    '    barWidthCount += item.EndPoint.X - item.StartPoint.X
            '    '    rangeCount += Abs(item.StartPoint.Y - item.EndPoint.Y)
            '    '    index += 1
            '    'Next
            '    'ShowInfoBox(String.Format("The average swing had an width of {0} bars, and {1} point range.", Round(barWidthCount / swings.Count, 0), Round(rangeCount / swings.Count, 2)))
            'End If
        End Sub
        Private Sub CloseBuy()
            If isLong Then
                Dim n As Decimal = If(IsLastBarOnChart And Chart.BidPrice <> 0, Chart.BidPrice, CurrentBar.Close)
                If IsLastBarOnChart And AllowActualTrading Then Sell()
                profit += (n - lastBuyPrice)
                Dim l = NewLabel(ToDollar(n - lastBuyPrice), Colors.Yellow, New Point(CurrentBar.Number, n))
                l.HorizontalAlignment = LabelHorizontalAlignment.Center
                l.VerticalAlignment = LabelVerticalAlignment.Top
                Dim t = NewTrendLine(Colors.Red, New Point(CurrentBar.Number, n), New Point(CurrentBar.Number - 6, n))
                t.Pen.Thickness = 3
                isLong = False
                lastBuyPrice = 0
                buyStopPrice = 0
                currentBuyExtendPrice = 0
            End If
        End Sub
        Private Sub CloseSell()
            If isShort Then
                Dim n As Decimal = If(IsLastBarOnChart And Chart.AskPrice <> 0, Chart.AskPrice, CurrentBar.Close)
                If IsLastBarOnChart And AllowActualTrading Then Buy()
                profit += (lastSellPrice - n)
                Dim l = NewLabel(ToDollar(lastSellPrice - n), Colors.Yellow, New Point(CurrentBar.Number, n))
                l.HorizontalAlignment = LabelHorizontalAlignment.Center
                l.VerticalAlignment = LabelVerticalAlignment.Top
                Dim t = NewTrendLine(Colors.Green, New Point(CurrentBar.Number, n), New Point(CurrentBar.Number - 6, n))
                t.Pen.Thickness = 3
                isShort = False
                lastSellPrice = 0
                sellStopPrice = 0
                currentSellExtendPrice = 0
            End If
        End Sub
        Private Sub SimpleBuy()
            If Not isLong Then
                If IsLastBarOnChart And AllowActualTrading Then Buy()
                Dim t = NewTrendLine(Colors.Green, New Point(CurrentBar.Number, CurrentBar.Close), New Point(CurrentBar.Number - 6, CurrentBar.Close))
                t.Pen.Thickness = 3
                lastBuyPrice = CurrentBar.Close
                isLong = True
                'buyStopPrice = CurrentBar.Close - StopPriceDifferenceMultiplier * RV
                currentBuyExtendPrice = CurrentBar.Close
            End If
        End Sub
        Private Sub SimpleSell()
            If Not isShort Then
                If IsLastBarOnChart And AllowActualTrading Then Sell()
                Dim t = NewTrendLine(Colors.Red, New Point(CurrentBar.Number, CurrentBar.Close), New Point(CurrentBar.Number - 6, CurrentBar.Close))
                t.Pen.Thickness = 3
                lastSellPrice = CurrentBar.Close
                isShort = True
                'sellStopPrice = CurrentBar.Close + StopPriceDifferenceMultiplier * RV
                currentSellExtendPrice = CurrentBar.Close
            End If
        End Sub

        Private Sub Sell()
            Dim o As New Order(Chart, CurrentBar.Close)
            o.OrderAction = ActionSide.Sell
            o.OrderType = OrderType.MKT
            o.Quantity = 1
            o.TransmitOrder()
        End Sub
        Private Sub Buy()
            Dim o As New Order(Chart, CurrentBar.Close)
            o.OrderAction = ActionSide.Buy
            o.OrderType = OrderType.MKT
            o.Quantity = 1
            o.TransmitOrder()
        End Sub
        Public Function SortDictionaryAsc(ByVal dict As Dictionary(Of Long, Decimal)) As Dictionary(Of Long, Decimal)
            Dim final = From key In dict.Keys Order By key Ascending Select key
            Return final
        End Function
        Function percentage(a As Decimal, b As Decimal) As String
            If b <> 0 Then
                Return Round((a / b) * 100, 4) & "%"
            Else
                Return "0%"
            End If
        End Function
        Private Function ToDollar(number As Decimal) As String
            Return If(number * 20 < 0, "-$", "$") & Round(Abs(number * 20), 0)
        End Function
        Public Overrides Property Name As String = "Statistical Analysis Technique"


    End Class
End Namespace