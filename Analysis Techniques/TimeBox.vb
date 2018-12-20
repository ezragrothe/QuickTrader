Imports System.Collections.ObjectModel

Namespace AnalysisTechniques
    Public Class TimeBox

#Region "AnalysisTechnique Inherited Code"
        Inherits AnalysisTechnique

        ' Inherit the one-argument constructor from the base class.
        Public Sub New(ByVal chart As Chart)
            MyBase.New(chart) ' Call the base class constructor.
            If chart IsNot Nothing Then AddHandler chart.ChartKeyDown, AddressOf KeyPress
            Description = "A technique that indicates each distinct day, as well as performing analytical functions on data during a day."
        End Sub
#End Region

        Public Enum AnchorType
            High = True
            Low = False
        End Enum
        <Input("The start time for the daily box.", "Times")> Public Property StartTime As String = "8:30 AM"
        <Input("The end time for the daily box.")> Public Property EndTime As String = "11:00 PM"
        <Input("The color for the boxes.", "Colors")> Public Property Color As Color = Colors.Gray
        <Input("The color for the projection lines for the box.")> Public Property ProjectionLineColor As Color = Colors.LightGray
        <Input("The color for the text indicating the low of the day.")> Public Property LowTimeTextColor As Color = Colors.Red
        <Input("The color for the text indicating the high of the day.")> Public Property HighTimeTextColor As Color = Colors.Lime
        '<Input("The option to anchor the time box to the high of the day or low.", "Anchor")> Public Property Anchor As AnchorType = AnchorType.High
        '<Input("The hotkey to toggle the projection timebox anchor point between the high or low of the day.")> Public Property ToggleAnchorPoint As Key = Key.None
        <Input("The font size for high and low texts.", "High/Low Texts")> Public Property HighLowTextSize As Double = 16
        <Input("The font weight for high and low texts.")> Public Property HighLowFontWeight As FontWeight = FontWeights.ExtraBold
        <Input("The visibility for high and low texts.")> Public Property HighLowTextsVisible As Boolean = True
        <Input("", "High/Low Lines")> Public Property ShowHLLines As Boolean = True
        <Input("")> Public Property AscendingTimeBoxFillColor As Color = Colors.Lime
        <Input("")> Public Property DescendingTimeBoxFillColor As Color = Colors.Red
        '<Input("")> Public Property HLLineWidth As Decimal = 2
        <Input("", "Misc")> Public Property LineWidth As Double = 1
        <Input("The number of past boxes used to determine the size of the average projection box.")> Public Property NumberOfBoxesToUseAsAverage As Integer = 5
        <Input("The option to show the range in dollars for each day")> Public Property ShowDayRange As Boolean = False
        <Input("The option to disable the sunday day box.")> Public Property DisableSundayBox As Boolean = True
        '<Input("The option whether the box will span until the next day, or be contained on only one day.")> Public Property NextDay As Boolean = False
        Public Property DashStyle As DoubleCollection = (New DoubleCollectionConverter).ConvertFromString("11 6 3 6 3 6")

        Private Class DayBox
            Public Property IsActive As Boolean = True
            Public Property TopLine As TrendLine
            Public Property BottomLine As TrendLine
            Public Property LeftLine As TrendLine
            Public Property RightLine As TrendLine
            Public Property ProjectionLeftLine As TrendLine
            Public Property ProjectionRightLine As TrendLine
            Public Property ProjectionRangeLine As TrendLine
            Public Property ProjectionRangeText As Label
            Public Property PercentageRatios As List(Of Object())
            Public Property RangeText As Label
            'Public Property TimeLineStartLine As Label
            'Public Property TimeLineEndLine As Label
            Public Property TimeLineLowTimeText As Label
            Public Property TimeLineHighTimeText As Label
            Public Property HLLine As TrendLine
            Public ReadOnly Property Lines As ReadOnlyCollection(Of TrendLine)
                Get
                    Return New ReadOnlyCollection(Of TrendLine)({LeftLine, BottomLine, TopLine, RightLine, ProjectionLeftLine, ProjectionRightLine, ProjectionRangeLine}.ToList)
                End Get
            End Property
            Public ReadOnly Property Rect As Rect
                Get
                    Return New Rect(TopLine.StartPoint, BottomLine.EndPoint)
                End Get
            End Property
        End Class
        Dim high As Point
        Dim low As Point
        Dim boxStartDate As Date
        Dim boxIsStarted As Boolean
        Dim boxes As List(Of DayBox)
        Dim futureBox As DayBox
        Protected Sub KeyPress(ByVal sender As Object, ByVal e As KeyEventArgs)
            If Chart IsNot Nothing AndAlso IsEnabled AndAlso Keyboard.Modifiers = ModifierKeys.None Then
                Dim key As Key
                If e.SystemKey = key.None Then
                    key = e.Key
                Else
                    key = e.SystemKey
                End If
                'If key = ToggleAnchorPoint Then
                '    For Each c In Chart.Parent.Charts
                '        For Each a In c.AnalysisTechniques
                '            If TypeOf a.AnalysisTechnique Is TimeBox Then
                '                CType(a.AnalysisTechnique, TimeBox).Anchor = Not CType(a.AnalysisTechnique, TimeBox).Anchor
                '                CType(a.AnalysisTechnique, TimeBox).Main()
                '            End If
                '        Next
                '    Next
                'End If

            End If
        End Sub
        Protected Overrides Sub Begin()
            MyBase.Begin()
            boxes = New List(Of DayBox)
            boxIsStarted = False
            boxStartDate = Date.MinValue
            futureBox = New DayBox
        End Sub

        Private Property CurrentBox As DayBox
            Get
                Return boxes(boxes.Count - 1)
            End Get
            Set(value As DayBox)
                boxes(boxes.Count - 1) = value
            End Set
        End Property
        Private ReadOnly Property IsBoxActive As Boolean
            Get
                Return boxes.Count > 0 AndAlso CurrentBox IsNot Nothing AndAlso CurrentBox.IsActive AndAlso CurrentBox.LeftLine IsNot Nothing AndAlso CurrentBox.RightLine IsNot Nothing AndAlso CurrentBox.TopLine IsNot Nothing AndAlso CurrentBox.BottomLine IsNot Nothing
            End Get
        End Property

        Private Sub StartBox()
            If boxes.Count > 0 AndAlso CurrentBox IsNot Nothing AndAlso CurrentBox.RightLine IsNot Nothing AndAlso CurrentBox.TopLine IsNot Nothing AndAlso CurrentBox.BottomLine IsNot Nothing Then
                With CurrentBox
                    If .RightLine.EndPoint.X = CurrentBar.Number - 1 Then
                        .RightLine.Coordinates = New LineCoordinates(.RightLine.StartPoint.X + 1, .RightLine.StartPoint.Y, .RightLine.EndPoint.X + 1, .RightLine.EndPoint.Y)
                        .TopLine.EndPoint = New Point(.TopLine.EndPoint.X + 1, .TopLine.EndPoint.Y)
                        .BottomLine.EndPoint = New Point(.BottomLine.EndPoint.X + 1, .BottomLine.EndPoint.Y)
                    End If
                End With
            End If
            boxes.Add(New DayBox)
            With CurrentBox
                .LeftLine = NewTrendLine(Color, New Point(CurrentBar.Number, CurrentBar.High), New Point(CurrentBar.Number, CurrentBar.Low))
                .BottomLine = NewTrendLine(Color, New Point(CurrentBar.Number, CurrentBar.Low), New Point(CurrentBar.Number, CurrentBar.Low))
                .TopLine = NewTrendLine(Color, New Point(CurrentBar.Number, CurrentBar.High), New Point(CurrentBar.Number, CurrentBar.High))
                .RightLine = NewTrendLine(Color, New Point(CurrentBar.Number, CurrentBar.High), New Point(CurrentBar.Number, CurrentBar.Low))
                .ProjectionLeftLine = NewTrendLine(ProjectionLineColor, New Point(CurrentBar.Number, CurrentBar.High), New Point(CurrentBar.Number, CurrentBar.High))
                .ProjectionRightLine = NewTrendLine(ProjectionLineColor, New Point(CurrentBar.Number, CurrentBar.High), New Point(CurrentBar.Number, CurrentBar.High))
                .ProjectionRangeLine = NewTrendLine(ProjectionLineColor, New Point(CurrentBar.Number, CurrentBar.High), New Point(CurrentBar.Number, CurrentBar.High))
                .ProjectionRangeText = NewLabel("", ProjectionLineColor, New Point(0, 0), ShowDayRange)
                .ProjectionRangeText.HorizontalAlignment = LabelHorizontalAlignment.Right
                .RangeText = NewLabel("", Color, New Point(0, 0), ShowDayRange)
                .RangeText.HorizontalAlignment = LabelHorizontalAlignment.Right
                .RangeText.VerticalAlignment = LabelVerticalAlignment.Bottom
                '.TimeLineStartLine = NewLabel("I", Color, New Point(0, 0))
                '.TimeLineStartLine.HorizontalAlignment = LabelHorizontalAlignment.Center
                '.TimeLineEndLine = NewLabel("I", Color, New Point(0, 0))
                '.TimeLineEndLine.HorizontalAlignment = LabelHorizontalAlignment.Center
                .TimeLineLowTimeText = NewLabel("", LowTimeTextColor, New Point(0, 0), HighLowTextsVisible)
                .TimeLineLowTimeText.HorizontalAlignment = LabelHorizontalAlignment.Center
                .TimeLineLowTimeText.VerticalAlignment = LabelVerticalAlignment.Top
                .TimeLineLowTimeText.Font.FontSize = HighLowTextSize
                .TimeLineLowTimeText.Font.FontWeight = HighLowFontWeight
                .TimeLineHighTimeText = NewLabel("", HighTimeTextColor, New Point(0, 0), HighLowTextsVisible)
                .TimeLineHighTimeText.HorizontalAlignment = LabelHorizontalAlignment.Center
                .TimeLineHighTimeText.VerticalAlignment = LabelVerticalAlignment.Bottom
                .TimeLineHighTimeText.Font.FontSize = HighLowTextSize
                .TimeLineHighTimeText.Font.FontWeight = HighLowFontWeight
                .HLLine = NewTrendLine(Colors.Transparent, New Point(0, 0), New Point(0, 0), ShowHLLines)
            End With
            For Each line In CurrentBox.Lines
                line.Pen.DashStyle = New DashStyle(DashStyle, 0)
                line.Pen.Thickness = LineWidth
                'line.IsSelectable = False
                'line.IsEditable = False
                'line.AutoRefresh = True
            Next
            Dim averageWidth As Integer
            Dim averageHeight As Decimal
            Dim totalWidth As Integer = 0
            Dim totalHeight As Double = 0
            If boxes.Count > NumberOfBoxesToUseAsAverage Then
                For i = boxes.Count - NumberOfBoxesToUseAsAverage - 1 To boxes.Count - 2
                    totalWidth += boxes(i).Rect.Width
                    totalHeight += boxes(i).Rect.Height
                Next
                averageWidth = totalWidth / NumberOfBoxesToUseAsAverage
                averageHeight = totalHeight / NumberOfBoxesToUseAsAverage
                With CurrentBox ' 
                    .RightLine.StartPoint = New Point(CurrentBar.Number + averageWidth, CurrentBar.High)
                    .RightLine.EndPoint = New Point(CurrentBar.Number + averageWidth, CurrentBar.Low)
                    If True Then
                        .TopLine.StartPoint = New Point(.TopLine.StartPoint.X, CurrentBar.High)
                        .TopLine.EndPoint = New Point(.TopLine.EndPoint.X, CurrentBar.High)
                        .BottomLine.StartPoint = New Point(.BottomLine.StartPoint.X, .TopLine.StartPoint.Y - averageHeight)
                        .BottomLine.EndPoint = New Point(.BottomLine.EndPoint.X, .TopLine.EndPoint.Y - averageHeight)
                        .ProjectionLeftLine.StartPoint = New Point(CurrentBar.Number, CurrentBar.Low)
                        .ProjectionLeftLine.EndPoint = New Point(CurrentBar.Number, CurrentBar.Low)
                        .ProjectionRightLine.StartPoint = New Point(CurrentBar.Number + averageWidth, CurrentBar.Low)
                        .ProjectionRightLine.EndPoint = New Point(CurrentBar.Number + averageWidth, CurrentBar.Low)
                        .ProjectionRangeLine.StartPoint = New Point(CurrentBar.Number, CurrentBar.Low)
                        .ProjectionRangeLine.EndPoint = New Point(CurrentBar.Number + averageWidth, CurrentBar.Low)
                    Else
                        .BottomLine.StartPoint = New Point(.BottomLine.StartPoint.X, CurrentBar.Low)
                        .BottomLine.EndPoint = New Point(.BottomLine.EndPoint.X, CurrentBar.Low)
                        .TopLine.StartPoint = New Point(.TopLine.StartPoint.X, .BottomLine.StartPoint.Y + averageHeight)
                        .TopLine.EndPoint = New Point(.TopLine.EndPoint.X, .BottomLine.EndPoint.Y + averageHeight)
                        .ProjectionLeftLine.StartPoint = New Point(CurrentBar.Number, CurrentBar.High)
                        .ProjectionLeftLine.EndPoint = New Point(CurrentBar.Number, CurrentBar.High)
                        .ProjectionRightLine.StartPoint = New Point(CurrentBar.Number + averageWidth, CurrentBar.High)
                        .ProjectionRightLine.EndPoint = New Point(CurrentBar.Number + averageWidth, CurrentBar.High)
                        .ProjectionRangeLine.StartPoint = New Point(CurrentBar.Number, CurrentBar.High)
                        .ProjectionRangeLine.EndPoint = New Point(CurrentBar.Number + averageWidth, CurrentBar.High)
                    End If
                End With
            Else
                CurrentBox.RightLine.StartPoint = New Point(0, CurrentBar.High)
                CurrentBox.RightLine.EndPoint = New Point(0, CurrentBar.Low)
                RemoveObjectFromChart(CurrentBox.RightLine)
            End If
            low = New Point(CurrentBar.Number, CurrentBar.Low)
            high = New Point(CurrentBar.Number, CurrentBar.High)

        End Sub
        Private Sub StopBox()
            If IsBoxActive Then
                With CurrentBox
                    .RightLine.StartPoint = New Point(CurrentBar.Number, high.Y)
                    .RightLine.EndPoint = New Point(CurrentBar.Number, low.Y)
                    .BottomLine.StartPoint = New Point(.BottomLine.StartPoint.X, low.Y)
                    .BottomLine.EndPoint = New Point(CurrentBar.Number, low.Y)
                    .TopLine.StartPoint = New Point(.TopLine.StartPoint.X, high.Y)
                    .TopLine.EndPoint = New Point(CurrentBar.Number, high.Y)
                    .LeftLine.StartPoint = New Point(.LeftLine.StartPoint.X, high.Y)
                    .LeftLine.EndPoint = New Point(.LeftLine.EndPoint.X, low.Y)
                    If ShowDayRange Then .RangeText.Location = New Point(CurrentBar.Number, high.Y)
                    .PercentageRatios = New List(Of Object())
                    Dim l As Double = Double.MaxValue, h As Double = Double.MinValue
                    Dim range As Double = Abs(high.Y - low.Y)
                    For i = .LeftLine.StartPoint.X + 1 To .RightLine.StartPoint.X
                        l = Min(l, Chart.bars(i - 1).Data.Low)
                        h = Max(h, Chart.bars(i - 1).Data.High)
                        .PercentageRatios.Add({Chart.bars(i - 1).Data.Date, (i - .LeftLine.StartPoint.X) / (.RightLine.StartPoint.X - .LeftLine.StartPoint.X), (h - l) / range})
                    Next
                    RemoveObjectFromChart(.ProjectionLeftLine)
                    RemoveObjectFromChart(.ProjectionRightLine)
                    RemoveObjectFromChart(.ProjectionRangeLine)
                    RemoveObjectFromChart(.ProjectionRangeText)
                End With
                AddObjectToChart(CurrentBox.RightLine)
            End If
            CurrentBox.IsActive = False
        End Sub

        Protected Overrides Sub Main()
            If CurrentBar.Number > 1 And Date.TryParse(Me.StartTime, New Date) And Date.TryParse(Me.EndTime, New Date) Then
                Dim startTime As Date = Date.Parse(Me.StartTime)
                Dim endTime As Date = Date.Parse(Me.EndTime)
                Dim barDate As Date = CurrentBar.Date
                Dim prevBarDate As Date = Chart.bars(CurrentBar.Number - 2).Data.Date
                If boxIsStarted Then
                    If IsTimeBetween(prevBarDate, barDate, endTime) And ((NextDay() And barDate.Date > boxStartDate.Date) Or Not NextDay()) Then
                        StopBox()
                        boxIsStarted = False
                    End If
                    If IsTimeBetween(prevBarDate, barDate, startTime) And Not boxIsStarted And ((DisableSundayBox And barDate.DayOfWeek <> DayOfWeek.Sunday) Or Not DisableSundayBox) Then
                        StartBox()
                        boxStartDate = barDate
                        boxIsStarted = True
                    End If
                Else
                    If IsTimeBetween(prevBarDate, barDate, startTime) AndAlso Not IsTimeBetween(prevBarDate, barDate, endTime) AndAlso ((DisableSundayBox And barDate.DayOfWeek <> DayOfWeek.Sunday) Or Not DisableSundayBox) Then
                        StartBox()
                        boxStartDate = barDate
                        boxIsStarted = True
                    End If
                    If NextDay() Then
                        If IsTimeBetween(prevBarDate, barDate, endTime) And ((NextDay() And barDate.Date > boxStartDate.Date) Or Not NextDay()) And boxIsStarted Then
                            StopBox()
                            boxIsStarted = False
                        End If
                    Else
                        If IsTimeBetween(prevBarDate, barDate, endTime) And boxIsStarted Then
                            StopBox()
                            boxIsStarted = False
                        End If
                    End If
                End If
                If CurrentBar.High >= high.Y Then high = New Point(CurrentBar.Number, CurrentBar.High)
                If CurrentBar.Low <= low.Y Then low = New Point(CurrentBar.Number, CurrentBar.Low)
                If IsBoxActive Then
                    With CurrentBox
                        Dim endBar As Integer = Max(.RightLine.StartPoint.X, CurrentBar.Number)
                        Dim projectedHeight As Decimal = high.Y - low.Y
                        If boxes.Count > NumberOfBoxesToUseAsAverage Then
                            Dim widthPercentagesTotal As Decimal = 0
                            Dim averageWidthPercentage As Decimal = 0
                            Dim heightPercentagesTotal As Decimal = 0
                            Dim averageHeightPercentage As Decimal = 0
                            For i = boxes.Count - NumberOfBoxesToUseAsAverage - 1 To boxes.Count - 2
                                If boxes(i).PercentageRatios IsNot Nothing Then
                                    Dim closestDate As Date = Date.MinValue
                                    Dim closestWidthPercentage As Decimal = 0
                                    Dim closestHeightPercentage As Decimal = 0
                                    Dim currentDate As Date = CurrentBar.Date
                                    For Each item In boxes(i).PercentageRatios
                                        Dim d As Date = item(0)
                                        If Abs((currentDate.TimeOfDay - d.TimeOfDay).TotalSeconds) < Abs((currentDate.TimeOfDay - closestDate.TimeOfDay).TotalSeconds) Or closestDate = Date.MinValue Then
                                            closestDate = d
                                            closestWidthPercentage = item(1)
                                            closestHeightPercentage = item(2)
                                        End If
                                    Next
                                    widthPercentagesTotal += closestWidthPercentage
                                    heightPercentagesTotal += closestHeightPercentage
                                End If
                            Next
                            averageWidthPercentage = widthPercentagesTotal / NumberOfBoxesToUseAsAverage
                            averageHeightPercentage = heightPercentagesTotal / NumberOfBoxesToUseAsAverage
                            If averageWidthPercentage <> 0 Then endBar = (CurrentBar.Number - .LeftLine.StartPoint.X) / averageWidthPercentage + .LeftLine.StartPoint.X
                            If averageHeightPercentage <> 0 Then projectedHeight = (high.Y - low.Y) / averageHeightPercentage
                        End If
                        Dim bottomPrice As Double
                        Dim topPrice As Double
                        If high.X < low.X Then
                            If .BottomLine.StartPoint.Y >= high.Y - projectedHeight Then
                                .ProjectionLeftLine.StartPoint = New Point(.ProjectionLeftLine.StartPoint.X, .BottomLine.StartPoint.Y)
                                .ProjectionLeftLine.EndPoint = New Point(.ProjectionLeftLine.EndPoint.X, high.Y - projectedHeight)
                                .ProjectionRightLine.StartPoint = New Point(endBar, .BottomLine.StartPoint.Y)
                                .ProjectionRightLine.EndPoint = New Point(endBar, high.Y - projectedHeight)
                                .ProjectionRangeLine.StartPoint = New Point(.ProjectionRangeLine.StartPoint.X, high.Y - projectedHeight)
                                .ProjectionRangeLine.EndPoint = New Point(endBar, high.Y - projectedHeight)
                                AddObjectToChart(.ProjectionLeftLine)
                                AddObjectToChart(.ProjectionRightLine)
                                AddObjectToChart(.ProjectionRangeLine)
                            Else
                                RemoveObjectFromChart(.ProjectionLeftLine)
                                RemoveObjectFromChart(.ProjectionRightLine)
                                RemoveObjectFromChart(.ProjectionRangeLine)
                            End If
                            'topPrice = Max(.TopLine.StartPoint.Y, high)
                            'bottomPrice = Min(topPrice - averageHeight, low)
                            .ProjectionRangeText.VerticalAlignment = LabelVerticalAlignment.Top
                        Else
                            If .TopLine.StartPoint.Y <= low.Y + projectedHeight Then
                                .ProjectionLeftLine.StartPoint = New Point(.ProjectionLeftLine.StartPoint.X, .TopLine.StartPoint.Y)
                                .ProjectionLeftLine.EndPoint = New Point(.ProjectionLeftLine.EndPoint.X, low.Y + projectedHeight)
                                .ProjectionRightLine.StartPoint = New Point(endBar, .TopLine.StartPoint.Y)
                                .ProjectionRightLine.EndPoint = New Point(endBar, low.Y + projectedHeight)
                                .ProjectionRangeLine.StartPoint = New Point(.ProjectionRangeLine.StartPoint.X, low.Y + projectedHeight)
                                .ProjectionRangeLine.EndPoint = New Point(endBar, low.Y + projectedHeight)
                                AddObjectToChart(.ProjectionLeftLine)
                                AddObjectToChart(.ProjectionRightLine)
                                AddObjectToChart(.ProjectionRangeLine)
                            Else
                                RemoveObjectFromChart(.ProjectionLeftLine)
                                RemoveObjectFromChart(.ProjectionRightLine)
                                RemoveObjectFromChart(.ProjectionRangeLine)
                            End If
                            'bottomPrice = Min(.BottomLine.StartPoint.Y, low)
                            'topPrice = Max(bottomPrice + averageHeight, high)
                            .ProjectionRangeText.VerticalAlignment = LabelVerticalAlignment.Bottom
                        End If
                        If ShowDayRange And projectedHeight <> 0 Then
                            .ProjectionRangeText.Location = .ProjectionRangeLine.EndPoint
                            .ProjectionRangeText.Text = Round(GetDollarValue(projectedHeight), 0)
                        End If
                        topPrice = high.Y
                        bottomPrice = low.Y
                        .LeftLine.StartPoint = New Point(.LeftLine.StartPoint.X, topPrice)
                        .LeftLine.EndPoint = New Point(.LeftLine.EndPoint.X, bottomPrice)
                        .BottomLine.StartPoint = New Point(.BottomLine.StartPoint.X, bottomPrice)
                        .BottomLine.EndPoint = New Point(endBar, bottomPrice)
                        .TopLine.StartPoint = New Point(.TopLine.StartPoint.X, topPrice)
                        .TopLine.EndPoint = New Point(endBar, topPrice)
                        .RightLine.StartPoint = New Point(endBar, topPrice)
                        .RightLine.EndPoint = New Point(endBar, bottomPrice)
                        If ShowDayRange Then
                            .RangeText.Location = New Point(endBar, topPrice)
                            .RangeText.Text = Round(GetDollarValue(topPrice - bottomPrice), 0)
                        End If
                        'Dim timeLineTopPoint As Decimal = -(Chart.Bounds.Y2 - Chart.GetRelativeFromRealHeight(Chart.Settings("TimeBarHeight").Value))
                        '.TimeLineStartLine.Location = New Point(.LeftLine.StartPoint.X, timeLineTopPoint + Chart.GetRelativeFromRealHeight(10))
                        '.TimeLineEndLine.Location = New Point(CurrentBar.Number, timeLineTopPoint + Chart.GetRelativeFromRealHeight(10))
                        .TimeLineLowTimeText.Location = New Point(low.X, bottomPrice - Chart.GetRelativeFromRealHeight(8))
                        Dim n = FormatNumberLength(Chart.bars(low.X - 1).Data.Date.Hour, 2) & FormatNumberLength(Chart.bars(low.X - 1).Data.Date.Minute, 2)
                        .TimeLineLowTimeText.Text = n(0) & vbNewLine & n(1) & vbNewLine & n(2) & vbNewLine & n(3) & vbNewLine
                        .TimeLineHighTimeText.Location = New Point(high.X, topPrice + Chart.GetRelativeFromRealHeight(8))
                        n = FormatNumberLength(Chart.bars(high.X - 1).Data.Date.Hour, 2) & FormatNumberLength(Chart.bars(high.X - 1).Data.Date.Minute, 2)
                        .TimeLineHighTimeText.Text = n(0) & vbNewLine & n(1) & vbNewLine & n(2) & vbNewLine & n(3) & vbNewLine
                        .HLLine.StartPoint = New Point(Avg(.RightLine.StartPoint.X, .LeftLine.StartPoint.X), topPrice)
                        .HLLine.EndPoint = New Point(Avg(.RightLine.StartPoint.X, .LeftLine.StartPoint.X), bottomPrice)
                        .HLLine.Pen.Brush = New SolidColorBrush(If(low.X < high.X, AscendingTimeBoxFillColor, DescendingTimeBoxFillColor))
                        .HLLine.Pen.Thickness = Chart.GetRealFromRelativeWidth((Max(.RightLine.StartPoint.X, CurrentBar.Number) - .LeftLine.StartPoint.X))
                        'For Each box In boxes
                        '    If box.IsActive = False Then
                        '        box.TimeLineLowTimeText.Location = New Point(box.TimeLineLowTimeText.Location.X, timeLineTopPoint)
                        '        box.TimeLineHighTimeText.Location = New Point(box.TimeLineHighTimeText.Location.X, timeLineTopPoint)
                        '        'box.TimeLineStartLine.Location = New Point(box.TimeLineStartLine.Location.X, timeLineTopPoint + Chart.GetRelativeFromRealHeight(10))
                        '        'box.TimeLineEndLine.Location = New Point(box.TimeLineEndLine.Location.X, timeLineTopPoint + Chart.GetRelativeFromRealHeight(10))
                        '    End If
                        'Next
                    End With
                End If
            End If

        End Sub

        Private Function NextDay() As Boolean
            Return StartTime = EndTime
        End Function
        Private Function GetDollarValue(ByVal priceDif As Decimal) As Decimal
            Dim multiplier As Double
            If Chart.Settings("UseRandom").Value Then
                multiplier = 1
            Else
                If Chart.IB.Contract(Chart.TickerID).Multiplier = "" OrElse Not IsNumeric(Chart.IB.Contract(Chart.TickerID).Multiplier) Then
                    multiplier = 1
                Else
                    multiplier = Chart.IB.Contract(Chart.TickerID).Multiplier
                End If
            End If
            Dim shares As Integer = Chart.Settings("DefaultOrderQuantity").Value
            'Dim a = priceDif / Chart.GetMinTick * Chart.GetMinTick * multiplier * shares
            Return priceDif * multiplier * shares
        End Function
        Private Function IsTimeBetween(firstDateTime As Date, secondDateTime As Date, dateTime As Date) As Boolean
            Dim prevDate, [date] As Date
            If firstDateTime < secondDateTime Then
                prevDate = firstDateTime
                [date] = secondDateTime
            Else
                [date] = firstDateTime
                prevDate = secondDateTime
            End If
            Return ([date].TimeOfDay >= dateTime.TimeOfDay And prevDate.TimeOfDay <= dateTime.TimeOfDay) Or
                   ([date].Date > prevDate.Date And (prevDate.TimeOfDay <= dateTime.TimeOfDay Or [date].TimeOfDay >= dateTime.TimeOfDay Or ([date].TimeOfDay >= dateTime.TimeOfDay And prevDate.TimeOfDay <= dateTime.TimeOfDay)))
        End Function
        Public Overrides Property Name As String = "TimeBox"

    End Class
End Namespace
