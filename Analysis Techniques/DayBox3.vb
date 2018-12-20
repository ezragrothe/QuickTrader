Imports System.Collections.ObjectModel

Namespace AnalysisTechniques
    Public Class DayBox3

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
        Private _startTime As String = "0830"
        <Input> Public Property StartTime As String
            Get
                Return _startTime
            End Get
            Set(value As String)
                If IsNumeric(value) Then
                    _startTime = FormatNumberLength(CInt(value), 4)
                End If
            End Set
        End Property

        Private _divisionTime As String = "1000"
        <Input> Public Property DivisionTime As String
            Get
                Return _divisionTime
            End Get
            Set(value As String)
                If IsNumeric(value) Then
                    _divisionTime = FormatNumberLength(CInt(value), 4)
                End If
            End Set
        End Property

        'Private _endTime As String = "1600"
        '<Input> Public Property EndTime As String
        '    Get
        '        Return _endTime
        '    End Get
        '    Set(value As String)
        '        If IsNumeric(value) Then
        '            _endTime = FormatNumberLength(CInt(value), 4)
        '        End If
        '    End Set
        'End Property

        <Input("", "Display")>
        Public Property AscendingDayBoxFillColor As Color = Colors.Lime
        <Input> Public Property DescendingDayBoxFillColor As Color = Colors.Red
        <Input> Public Property BoxLineColor As Color = Colors.LightGray
        <Input> Public Property BoxLineWidth As Double = 1
        <Input> Public Property ProjectionBoxesOn As Boolean = True
        Public Property DashStyle As DoubleCollection = (New DoubleCollectionConverter).ConvertFromString("11 6 3 6 3 6")
        <Input> Public Property ProjectionLineColor As Color = Colors.LightGray
        <Input> Public Property ProjectionLineWidth As Double = 1
        <Input> Public Property NumberOfBoxesToUseAsAverage As Integer = 5


        <Input("", "Text and Lines")> Public Property HighLowTextsVisible As Boolean = True
        <Input> Public Property HighLowTextSize As Double = 16
        <Input> Public Property HighLowFontWeight As FontWeight = FontWeights.ExtraBold
        <Input> Public Property HighLowVerticalSpacing As Decimal = 5
        <Input> Public Property HighLowTimeTextColor As Color = Colors.Lime
        Dim highAndLowTextSpacing As Decimal ' set in Begin() routine

        <Input> Public Property ShowDayRange As Boolean = False
        <Input> Public Property DayRangeTextColor As Color = Colors.Blue
        <Input> Public Property DayRangeTextSize As Decimal = 12
        <Input> Public Property CloseLineWidth As Boolean = True
        <Input> Public Property CloseLineUpColor As Boolean = True
        <Input> Public Property CloseLineDownColor As Boolean = True

        <Input> Public Property NHNLLineWidth As Decimal = 0.4
        Public Property ShowCloseDot As Boolean = True
        Public Property ShowClockText As Boolean = True

        Property ClockDisplayStyle As String = "hh:mm:ss tt"


        Public Property DisableSundayBox As Boolean = True




        Private Class Box
            Public Property StartDate As Date
            Public Property IsActive As Boolean = True
            Public Property RectObj As Rectangle
            Public Property PercentageRatios As List(Of Object())
            Public Property SpacingPercentageRatios As List(Of Object())
            Public Property CloseDot As TrendLine
            Public Property RangeText As Label
            Public Property TimeLineLowTimeText As Label
            Public Property TimeLineHighTimeText As Label
        End Class

        Enum DayBoxMode
            Inactive = 0
            Overnight = 1
            Day = 2
        End Enum

        Dim high As Point
        Dim low As Point
        Dim boxStartDate As Date
        Dim mode As DayBoxMode

        Dim boxesFull As List(Of Box)
        Dim boxesOvernight As List(Of Box)
        Dim boxesDay As List(Of Box)

        Dim projectionBoxFull As Rectangle
        Dim projectionBoxOvernight As Rectangle
        Dim projectionBoxDay As Rectangle


        Protected Sub KeyPress(ByVal sender As Object, ByVal e As KeyEventArgs)
            If Chart IsNot Nothing AndAlso IsEnabled AndAlso Keyboard.Modifiers = ModifierKeys.None Then
                Dim key As Key
                If e.SystemKey = Key.None Then
                    key = e.Key
                Else
                    key = e.SystemKey
                End If
                'If key = ToggleAnchorPoint Then
                '    For Each c In Chart.Parent.Charts
                '        For Each a In c.AnalysisTechniques
                '            If TypeOf a.AnalysisTechnique Is DayBox Then
                '                CType(a.AnalysisTechnique, DayBox).Anchor = Not CType(a.AnalysisTechnique, DayBox).Anchor
                '                CType(a.AnalysisTechnique, DayBox).Main()
                '            End If
                '        Next
                '    Next
                'End If

            End If
        End Sub
        Protected Overrides Sub Begin()
            MyBase.Begin()
            boxesFull = New List(Of Box)
            mode = DayBoxMode.Inactive
            boxStartDate = Date.MinValue
            highAndLowTextSpacing = Chart.GetRelativeFromRealHeight(30)
            Dim projectionBoxes() As Rectangle = {projectionBoxFull, projectionBoxOvernight, projectionBoxDay}
            For Each box In projectionBoxes
                box = NewRectangle(ProjectionLineColor, Colors.Transparent, New Point(0, 0), New Point(0, 0), ProjectionBoxesOn)
                box.Pen.Thickness = ProjectionLineWidth
                box.LockPositionOrientation = ChartDrawingVisual.LockPositionOrientationTypes.Vertically
                box.CanResize = False
                box.ShowSelectionHandles = False
            Next
            timeLabel = NewLabel(Now.ToShortTimeString, BoxLineColor, New Point(0, 0), ShowClockText)
            timeLabel.VerticalAlignment = LabelVerticalAlignment.Top
            timeLabel.HorizontalAlignment = LabelHorizontalAlignment.Center
            timeLabel.IsEditable = False
            timeLabel.IsSelectable = False
            timeLabel.Font.FontSize += 1
            timeLabel.Font.FontWeight = FontWeights.Bold
            timer = New System.Windows.Forms.Timer
            timer.Interval = 1000
            timer.Start()
            AddHandler timer.Tick, AddressOf Elapsed

        End Sub

        Private Property CurrentBoxFull As Box
            Get
                Return boxesFull(boxesFull.Count - 1)
            End Get
            Set(value As Box)
                boxesFull(boxesFull.Count - 1) = value
            End Set
        End Property
        Private Property CurrentBoxOvernight As Box
            Get
                Return boxesOvernight(boxesOvernight.Count - 1)
            End Get
            Set(value As Box)
                boxesOvernight(boxesOvernight.Count - 1) = value
            End Set
        End Property
        Private Property CurrentBoxDay As Box
            Get
                Return boxesDay(boxesDay.Count - 1)
            End Get
            Set(value As Box)
                boxesDay(boxesDay.Count - 1) = value
            End Set
        End Property


        Private Sub StartBox()

            boxesFull.Add(New Box)
            With CurrentBoxFull
                .RectObj = NewRectangle(BoxLineColor, Colors.Red, New Point(CurrentBar.Number, CurrentBar.High), New Point(CurrentBar.Number, CurrentBar.Low))
                .RectObj.Pen.DashStyle = New DashStyle(DashStyle, 0)
                .RectObj.Pen.Thickness = If(boxesFull.Count <= NumberOfBoxesToUseAsAverage, BoxLineWidth, 0)
                .RectObj.IsSelectable = False
                .RectObj.IsEditable = False

                .CloseDot = NewTrendLine(BoxLineColor, New Point(CurrentBar.Number, CurrentBar.High), New Point(CurrentBar.Number, CurrentBar.Low), ShowCloseDot)
                .CloseDot.Pen.Thickness = 5
                .CloseDot.IsSelectable = False

                .RangeText = NewLabel("", DayRangeTextColor, New Point(0, 0), ShowDayRange)
                .RangeText.HorizontalAlignment = LabelHorizontalAlignment.Right
                .RangeText.VerticalAlignment = LabelVerticalAlignment.Top
                .RangeText.Font.FontSize = DayRangeTextSize
                .RangeText.Font.FontWeight = FontWeights.Bold
                .RangeText.IsEditable = False
                .RangeText.IsSelectable = False

                .TimeLineLowTimeText = NewLabel("", HighLowTimeTextColor, New Point(0, 0), HighLowTextsVisible)
                .TimeLineLowTimeText.HorizontalAlignment = LabelHorizontalAlignment.Right
                .TimeLineLowTimeText.VerticalAlignment = LabelVerticalAlignment.Top
                .TimeLineLowTimeText.Font.FontSize = HighLowTextSize
                .TimeLineLowTimeText.Font.FontWeight = HighLowFontWeight

                .TimeLineHighTimeText = NewLabel("", HighLowTimeTextColor, New Point(0, 0), HighLowTextsVisible)
                .TimeLineHighTimeText.HorizontalAlignment = LabelHorizontalAlignment.Right
                .TimeLineHighTimeText.VerticalAlignment = LabelVerticalAlignment.Bottom
                .TimeLineHighTimeText.Font.FontSize = HighLowTextSize
                .TimeLineHighTimeText.Font.FontWeight = HighLowFontWeight
                'If boxes.Count > 1 Then
                '    .SpacingPercentageRatios = New List(Of Object())
                '    Dim l = Double.MaxValue, h = Double.MinValue
                '    Dim spaceBetween As Integer = .RectObj.Location.X - boxes(boxes.Count - 2).RectObj.Coordinates.Right
                '    Dim start As Integer = boxes(boxes.Count - 2).RectObj.Coordinates.Right
                '    For i = boxes(boxes.Count - 2).RectObj.Coordinates.Right To .RectObj.Location.X
                '        l = Min(l, Chart.bars(i - 1).Data.Low)
                '        h = Max(h, Chart.bars(i - 1).Data.High)
                '        .SpacingPercentageRatios.Add({Chart.bars(i - 1).Data.Date, (i - start) / spaceBetween})
                '    Next
                'End If

            End With



            'If boxes.Count > NumberOfBoxesToUseAsAverage Then
            '    For i = boxes.Count - NumberOfBoxesToUseAsAverage - 1 To boxes.Count - 2
            '        totalWidth += boxes(i).RectObj.Width
            '        totalHeight += boxes(i).RectObj.Height
            '    Next
            '    averageWidth = totalWidth / NumberOfBoxesToUseAsAverage
            '    averageHeight = totalHeight / NumberOfBoxesToUseAsAverage
            '    With CurrentBox ' 
            '        .RightLine.StartPoint = New Point(CurrentBar.Number + averageWidth, CurrentBar.High)
            '        .RightLine.EndPoint = New Point(CurrentBar.Number + averageWidth, CurrentBar.Low)
            '        If .RightLine.StartPoint.X - 1 <= Chart.bars.Count - 1 And .RightLine.StartPoint.X - 1 >= 0 And ShowCloseDot Then .CloseDot.Coordinates = New LineCoordinates(.RightLine.StartPoint.X - 1, Bars(.RightLine.StartPoint.X - 1).Close, .RightLine.StartPoint.X + 1, Bars(.RightLine.StartPoint.X - 1).Close) Else .CloseDot.Coordinates = New LineCoordinates(0, 0, 0, 0)
            '        If True Then
            '            .TopLine.StartPoint = New Point(.TopLine.StartPoint.X, CurrentBar.High)
            '            .TopLine.EndPoint = New Point(.TopLine.EndPoint.X, CurrentBar.High)
            '            .BottomLine.StartPoint = New Point(.BottomLine.StartPoint.X, .TopLine.StartPoint.Y - averageHeight)
            '            .BottomLine.EndPoint = New Point(.BottomLine.EndPoint.X, .TopLine.EndPoint.Y - averageHeight)
            '            .ProjectionLeftLine.StartPoint = New Point(CurrentBar.Number, CurrentBar.Low)
            '            .ProjectionLeftLine.EndPoint = New Point(CurrentBar.Number, CurrentBar.Low)
            '            .ProjectionRightLine.StartPoint = New Point(CurrentBar.Number + averageWidth, CurrentBar.Low)
            '            .ProjectionRightLine.EndPoint = New Point(CurrentBar.Number + averageWidth, CurrentBar.Low)
            '            .ProjectionRangeLine.StartPoint = New Point(CurrentBar.Number, CurrentBar.Low)
            '            .ProjectionRangeLine.EndPoint = New Point(CurrentBar.Number + averageWidth, CurrentBar.Low)
            '        Else
            '            .BottomLine.StartPoint = New Point(.BottomLine.StartPoint.X, CurrentBar.Low)
            '            .BottomLine.EndPoint = New Point(.BottomLine.EndPoint.X, CurrentBar.Low)
            '            .TopLine.StartPoint = New Point(.TopLine.StartPoint.X, .BottomLine.StartPoint.Y + averageHeight)
            '            .TopLine.EndPoint = New Point(.TopLine.EndPoint.X, .BottomLine.EndPoint.Y + averageHeight)
            '            .ProjectionLeftLine.StartPoint = New Point(CurrentBar.Number, CurrentBar.High)
            '            .ProjectionLeftLine.EndPoint = New Point(CurrentBar.Number, CurrentBar.High)
            '            .ProjectionRightLine.StartPoint = New Point(CurrentBar.Number + averageWidth, CurrentBar.High)
            '            .ProjectionRightLine.EndPoint = New Point(CurrentBar.Number + averageWidth, CurrentBar.High)
            '            .ProjectionRangeLine.StartPoint = New Point(CurrentBar.Number, CurrentBar.High)
            '            .ProjectionRangeLine.EndPoint = New Point(CurrentBar.Number + averageWidth, CurrentBar.High)
            '        End If
            '    End With
            'Else
            '    With CurrentBox
            '        CurrentBox.RightLine.StartPoint = New Point(0, CurrentBar.High)
            '        CurrentBox.RightLine.EndPoint = New Point(0, CurrentBar.Low)
            '        If .RightLine.StartPoint.X - 1 <= Chart.bars.Count - 1 And .RightLine.StartPoint.X - 1 >= 0 And ShowCloseDot Then .CloseDot.Coordinates = New LineCoordinates(.RightLine.StartPoint.X - 1, Bars(.RightLine.StartPoint.X - 1).Close, .RightLine.StartPoint.X + 1, Bars(.RightLine.StartPoint.X - 1).Close) Else .CloseDot.Coordinates = New LineCoordinates(0, 0, 0, 0)
            '        RemoveObjectFromChart(CurrentBox.RightLine)
            '        If ShowCloseDot Then RemoveObjectFromChart(CurrentBox.CloseDot)
            '    End With
            'End If
            low = New Point(CurrentBar.Number, CurrentBar.Low)
            high = New Point(CurrentBar.Number, CurrentBar.High)
            mode = DayBoxMode.Overnight
        End Sub
        Private Sub RestartBox()
            'If boxes.Count > 0 AndAlso CurrentBox IsNot Nothing AndAlso CurrentBox.RectObj IsNot Nothing Then
            '    With CurrentBox
            '        If .RectObj.Coordinates.Right = CurrentBar.Number - 1 Then
            '            .RectObj.Width += 1
            '            If .RectObj.Coordinates.Right - 1 <= Chart.bars.Count - 1 And .RectObj.Coordinates.Right - 1 >= 0 And ShowCloseDot Then
            '                .CloseDot.Coordinates = New LineCoordinates(.RectObj.Coordinates.Right, Bars(.RectObj.Coordinates.Right - 1).Close, .RectObj.Coordinates.Right + 1, Bars(.RectObj.Coordinates.Right - 1).Close)
            '            Else
            '                .CloseDot.Coordinates = New LineCoordinates(0, 0, 0, 0)
            '            End If
            '        End If
            '    End With
            'End If

            If CurrentBar.High >= high.Y Then high = New Point(CurrentBar.Number, CurrentBar.High)
                If CurrentBar.Low <= low.Y Then low = New Point(CurrentBar.Number, CurrentBar.Low)

            With CurrentBoxFull
                .RectObj.Coordinates = New Rect(.RectObj.Location.X, high.Y, CurrentBar.Number - .RectObj.Location.X, Abs(low.Y - high.Y))
                .RectObj.Pen.Thickness = BoxLineWidth
                If .RectObj.Coordinates.Right - 1 <= Chart.bars.Count - 1 And .RectObj.Coordinates.Right - 1 >= 0 And ShowCloseDot Then
                    .CloseDot.Coordinates = New LineCoordinates(.RectObj.Coordinates.Right, Bars(.RectObj.Coordinates.Right - 1).Close, .RectObj.Coordinates.Right + 1, Bars(.RectObj.Coordinates.Right - 1).Close)
                Else
                    .CloseDot.Coordinates = New LineCoordinates(0, 0, 0, 0)
                End If

                If ShowDayRange And CurrentBar.Number - .RectObj.Location.X > 12 Then
                    Dim rangetextPoint As Point
                    If low.X > high.X Then
                        rangetextPoint = New Point(low.X, low.Y - Chart.GetRelativeFromRealHeight(HighLowVerticalSpacing) - GetTextOffset(low.X))
                        If HighLowTextsVisible Then rangetextPoint = AddToY(rangetextPoint, -Chart.GetRelativeFromRealHeight(HighLowTextSize))
                        .RangeText.VerticalAlignment = LabelVerticalAlignment.Top
                    Else
                        rangetextPoint = New Point(high.X, high.Y + Chart.GetRelativeFromRealHeight(HighLowVerticalSpacing) + GetTextOffset(high.X))
                        If HighLowTextsVisible Then rangetextPoint = AddToY(rangetextPoint, Chart.GetRelativeFromRealHeight(HighLowTextSize))
                        .RangeText.VerticalAlignment = LabelVerticalAlignment.Bottom
                    End If
                    .RangeText.Location = rangetextPoint
                End If
                '.PercentageRatios = New List(Of Object())
                '    Dim l As Double = Double.MaxValue, h As Double = Double.MinValue
                '    Dim range As Double = Abs(high.Y - low.Y)
                'For i = .RectObj.Location.X + 1 To .RectObj.Location.X + .RectObj.Width
                '    l = Min(l, Chart.bars(i - 1).Data.Low)
                '    h = Max(h, Chart.bars(i - 1).Data.High)
                '    .PercentageRatios.Add({Chart.bars(i - 1).Data.Date, (i - .RectObj.Location.X) / (.RectObj.Width), (h - l) / range})
                'Next
                'If HighLowTextsVisible And CurrentBar.Number - .RectObj.Location.X > 12 Then
                '    .TimeLineLowTimeText.Location = New Point(low.X, low.Y - Chart.GetRelativeFromRealHeight(HighLowVerticalSpacing) - GetTextOffset(low.X))
                '    .TimeLineHighTimeText.Location = New Point(high.X, high.Y + Chart.GetRelativeFromRealHeight(HighLowVerticalSpacing) + GetTextOffset(high.X))
                '    Dim n = FormatNumberLength(Chart.bars(low.X - 1).Data.Date.Hour, 2) & FormatNumberLength(Chart.bars(low.X - 1).Data.Date.Minute, 2)
                '    .TimeLineLowTimeText.Text = If(.TimeLineLowTimeText.Location.X > .TimeLineHighTimeText.Location.X, n(0) & n(1) & ":" & n(2) & n(3) & vbNewLine, "")
                '    n = FormatNumberLength(Chart.bars(high.X - 1).Data.Date.Hour, 2) & FormatNumberLength(Chart.bars(high.X - 1).Data.Date.Minute, 2)
                '    .TimeLineHighTimeText.Text = If(.TimeLineLowTimeText.Location.X > .TimeLineHighTimeText.Location.X, "", n(0) & n(1) & ":" & n(2) & n(3) & vbNewLine)
                'End If
            End With
            StartBox()
            '    If ShowCloseDot Then AddObjectToChart(CurrentBox.CloseDot)
            'If boxes.Count >= NumberOfBoxesToUseAsAverage Then
            '    Dim sumOfWidths As Decimal
            '    Dim sumOfHeights As Decimal
            '    Dim avgWidth As Decimal
            '    Dim avgHeight As Decimal
            '    For i = boxes.Count - NumberOfBoxesToUseAsAverage To boxes.Count - 1
            '        sumOfWidths += boxes(i).RectObj.Width
            '        sumOfHeights += boxes(i).RectObj.Height
            '    Next
            '    avgWidth = sumOfWidths / NumberOfBoxesToUseAsAverage
            '    avgHeight = sumOfHeights / NumberOfBoxesToUseAsAverage
            '    projectionBox.Coordinates = New Rect(CurrentBox.RectObj.Coordinates.Right, Bars(CurrentBox.RectObj.Coordinates.Right - 1).Close + avgHeight / 2, avgWidth, avgHeight)
            'End If
        End Sub
        Private Sub StartDayBox()

        End Sub
        Private Sub ContinueBox()
            If CurrentBar.High >= high.Y Then high = New Point(CurrentBar.Number, CurrentBar.High)
            If CurrentBar.Low <= low.Y Then low = New Point(CurrentBar.Number, CurrentBar.Low)
            If mode = DayBoxMode.Overnight Then

            ElseIf mode = DayBoxMode.Day Then

            End If

            With CurrentBoxFull
                Dim projectedHeight As Decimal = high.Y - low.Y
                Dim projectedWidth As Decimal = CurrentBar.Number - .RectObj.Location.X
                'If boxesFull.Count > NumberOfBoxesToUseAsAverage Then
                '    Dim sumOfWidths As Decimal
                '    Dim sumOfHeights As Decimal
                '    Dim avgWidth As Decimal
                '    Dim avgHeight As Decimal

                '    Dim sumOfWidthPercentages As Decimal
                '    Dim sumOfHeightPercentages As Decimal
                '    Dim avgWidthPercentage As Decimal
                '    Dim avgHeightPercentage As Decimal

                '    Dim isDataCorrect As Boolean = True

                '    For i = boxes.Count - NumberOfBoxesToUseAsAverage - 1 To boxes.Count - 2
                '        If boxes(i).PercentageRatios IsNot Nothing Then
                '            Dim currentClosestDate As Date = Date.MinValue
                '            Dim matchedWidthPercentage As Decimal = 0
                '            Dim matchedHeightPercentage As Decimal = 0
                '            Dim currentDate As Date = CurrentBar.Date
                '            For Each item In boxes(i).PercentageRatios
                '                Dim d As Date = item(0)
                '                If Abs((currentDate.TimeOfDay - d.TimeOfDay).TotalSeconds) < Abs((currentDate.TimeOfDay - currentClosestDate.TimeOfDay).TotalSeconds) Or currentClosestDate = Date.MinValue Then
                '                    currentClosestDate = d
                '                    matchedWidthPercentage = item(1)
                '                    matchedHeightPercentage = item(2)
                '                End If
                '            Next
                '            sumOfWidthPercentages += matchedWidthPercentage
                '            sumOfHeightPercentages += matchedHeightPercentage
                '        Else
                '            isDataCorrect = False
                '            Exit For
                '        End If
                '        sumOfWidths += boxes(i).RectObj.Width
                '        sumOfHeights += boxes(i).RectObj.Height
                '    Next
                '    avgWidthPercentage = sumOfWidthPercentages / NumberOfBoxesToUseAsAverage
                '    avgHeightPercentage = sumOfHeightPercentages / NumberOfBoxesToUseAsAverage
                '    avgWidth = sumOfWidths / NumberOfBoxesToUseAsAverage
                '    avgHeight = sumOfHeights / NumberOfBoxesToUseAsAverage
                '    Dim originalHeight As Decimal = projectionBox.Height

                '    If isDataCorrect And avgWidthPercentage <> 0 And avgHeightPercentage <> 0 Then
                '        Dim progress = GetTimeDifferenceWithRespectToDate(CurrentBar.Date.TimeOfDay, StartTime.TimeOfDay).TotalMilliseconds / GetTimeDifferenceWithRespectToDate(endTime.TimeOfDay + If(NextDay(), TimeSpan.FromDays(1), TimeSpan.FromDays(0)), StartTime.TimeOfDay).TotalMilliseconds
                '        projectedWidth = Max(CurrentBar.Number - .RectObj.Location.X, LinCalc(0, avgWidth, 1, (CurrentBar.Number - .RectObj.Location.X) / avgWidthPercentage, 2 * progress - progress ^ 2))
                '        projectedHeight = LinCalc(0, avgHeight, 1, (high.Y - low.Y) / avgHeightPercentage, 2 * progress - progress ^ 2)
                '    Else
                '        projectedWidth = avgWidth
                '        projectedHeight = avgHeight
                '    End If

                '    If high.Y - low.Y > projectedHeight Then
                '        projectionBox.Coordinates = New Rect(0, 0, 0, 0)
                '        .RectObj.Pen.Thickness = BoxLineWidth
                '    ElseIf projectionBox.Width <> 0 Then
                '        Dim offsetVal As Decimal
                '        If low.Y < projectionBox.Location.Y - projectionBox.Height Then offsetVal = low.Y - (projectionBox.Location.Y - projectionBox.Height)
                '        If high.Y > projectionBox.Location.Y Then offsetVal = high.Y - projectionBox.Location.Y
                '        Dim heightGain = (projectedHeight - originalHeight)
                '        Dim topsideLeniency = (projectionBox.Location.Y - high.Y) / ((projectionBox.Location.Y - high.Y) + (low.Y - (projectionBox.Location.Y - projectionBox.Height)))
                '        projectionBox.Coordinates = New Rect(.RectObj.Location.X, projectionBox.Location.Y + offsetVal, projectedWidth, projectedHeight)
                '        projectionBox.Location = New Point(projectionBox.Location.X, projectionBox.Location.Y + topsideLeniency * heightGain)
                '        projectionBox.Height = projectedHeight
                '        projectionBox.MaxBottomValue = high.Y
                '        projectionBox.MaxTopValue = low.Y
                '    End If
                'End If

                .RectObj.Coordinates = New Rect(.RectObj.Location.X, high.Y, projectedWidth, high.Y - low.Y)

                .RectObj.Fill = New SolidColorBrush(If(low.X < high.X, AscendingDayBoxFillColor, DescendingDayBoxFillColor))

                If .RectObj.Coordinates.Right - 1 <= Chart.bars.Count - 1 And .RectObj.Coordinates.Right - 1 >= 0 And ShowCloseDot Then
                    .CloseDot.Coordinates = New LineCoordinates(.RectObj.Coordinates.Right, Bars(.RectObj.Coordinates.Right - 1).Close, .RectObj.Coordinates.Right + 1, Bars(.RectObj.Coordinates.Right - 1).Close)
                Else
                    .CloseDot.Coordinates = New LineCoordinates(0, 0, 0, 0)
                End If


                If HighLowTextsVisible And CurrentBar.Number - .RectObj.Location.X > 12 Then
                    .TimeLineHighTimeText.Location = New Point(high.X, high.Y + Chart.GetRelativeFromRealHeight(HighLowVerticalSpacing) + GetTextOffset(high.X))
                    .TimeLineLowTimeText.Location = New Point(low.X, low.Y - Chart.GetRelativeFromRealHeight(HighLowVerticalSpacing) - GetTextOffset(low.X))
                    Dim n = FormatNumberLength(Chart.bars(high.X - 1).Data.Date.Hour, 2) & FormatNumberLength(Chart.bars(high.X - 1).Data.Date.Minute, 2)
                    .TimeLineHighTimeText.Text = If(.TimeLineLowTimeText.Location.X > .TimeLineHighTimeText.Location.X, "", n(0) & n(1) & ":" & n(2) & n(3))
                    n = FormatNumberLength(Chart.bars(low.X - 1).Data.Date.Hour, 2) & FormatNumberLength(Chart.bars(low.X - 1).Data.Date.Minute, 2)
                    .TimeLineLowTimeText.Text = If(.TimeLineLowTimeText.Location.X > .TimeLineHighTimeText.Location.X, n(0) & n(1) & ":" & n(2) & n(3), "")
                End If

                If ShowDayRange And CurrentBar.Number - .RectObj.Location.X > 12 Then
                    Dim rangetextPoint As Point
                    If low.X > high.X Then
                        rangetextPoint = New Point(low.X, low.Y - Chart.GetRelativeFromRealHeight(HighLowVerticalSpacing) - GetTextOffset(low.X))
                        If HighLowTextsVisible Then rangetextPoint = AddToY(rangetextPoint, -Chart.GetRelativeFromRealHeight(HighLowTextSize))
                        .RangeText.VerticalAlignment = LabelVerticalAlignment.Top
                    Else
                        rangetextPoint = New Point(high.X, high.Y + Chart.GetRelativeFromRealHeight(HighLowVerticalSpacing) + GetTextOffset(high.X))
                        If HighLowTextsVisible Then rangetextPoint = AddToY(rangetextPoint, Chart.GetRelativeFromRealHeight(HighLowTextSize))
                        .RangeText.VerticalAlignment = LabelVerticalAlignment.Bottom
                    End If
                    .RangeText.Location = rangetextPoint
                    .RangeText.Text = Round(GetDollarValue(high.Y - low.Y), 0)
                Else
                    .RangeText.Text = ""
                End If
            End With
        End Sub
        Private Function GetTextOffset(bar As Integer) As Decimal
            For Each item In Chart.AnalysisTechniques
                If TypeOf item.AnalysisTechnique Is MLRTrend1 Then
                    Return Chart.GetRelativeFromRealHeight(CType(item.AnalysisTechnique, MLRTrend1).GetTextOffset(bar))
                ElseIf TypeOf item.AnalysisTechnique Is MLRTrend2 Then
                    Return Chart.GetRelativeFromRealHeight(CType(item.AnalysisTechnique, MLRTrend2).GetTextOffset(bar))
                ElseIf TypeOf item.AnalysisTechnique Is FanTrend Then
                    Return Chart.GetRelativeFromRealHeight(CType(item.AnalysisTechnique, FanTrend).GetTextOffset(bar))
                End If
            Next
            Return 0
        End Function
        Protected Overrides Sub Main()
            If CurrentBar.Number > 1 Then
                Dim barDate As Date = CurrentBar.Date
                Dim prevBarDate As Date = Chart.bars(CurrentBar.Number - 2).Data.Date
                If Date.TryParseExact(Me.StartTime, "HHmm", Nothing, Globalization.DateTimeStyles.AssumeLocal, New Date) And Date.TryParseExact(Me.DivisionTime, "HHmm", Nothing, Globalization.DateTimeStyles.AssumeLocal, New Date) Then
                    Dim startTime As Date = Date.ParseExact(Me.StartTime, "HHmm", Nothing)
                    Dim divisionTime As Date = Date.ParseExact(Me.DivisionTime, "HHmm", Nothing)
                    If mode <> DayBoxMode.Inactive Then
                        If IsTimeBetween(prevBarDate, barDate, startTime) Then
                            RestartBox()
                        End If
                        If IsTimeBetween(prevBarDate, barDate, divisionTime) Then
                            StartDayBox()
                        End If
                    Else
                        If IsTimeBetween(prevBarDate, barDate, startTime) Then
                            StartBox()
                        End If
                    End If
                    If mode <> DayBoxMode.Inactive Then
                        ContinueBox()
                    End If


                End If


                'If boxes.Count > NumberOfBoxesToUseAsAverage Then
                '    Dim sumOfSpacings As Decimal = 0
                '    Dim avgSpacing As Decimal = 0

                '    Dim sumOfWidths As Decimal = 0
                '    Dim sumOfHeights As Decimal = 0
                '    Dim avgWidth As Decimal = 0
                '    Dim avgHeight As Decimal = 0
                '    Dim isDataCorrect As Boolean = True
                '    For i = boxes.Count - NumberOfBoxesToUseAsAverage To boxes.Count - 1
                '        sumOfSpacings += boxes(i).RectObj.Coordinates.Left - boxes(i - 1).RectObj.Coordinates.Right
                '        sumOfWidths += boxes(i).RectObj.Width
                '        sumOfHeights += boxes(i).RectObj.Height
                '    Next
                '    avgSpacing = sumOfSpacings / NumberOfBoxesToUseAsAverage
                '    avgWidth = sumOfWidths / NumberOfBoxesToUseAsAverage
                '    avgHeight = sumOfHeights / NumberOfBoxesToUseAsAverage

                '    If isDataCorrect Then
                '        'Dim progress = GetTimeDifferenceWithRespectToDate(CurrentBar.Date.TimeOfDay, Date.ParseExact(EndTime, "HHmm", Nothing).TimeOfDay).TotalMilliseconds / (GetTimeDifferenceWithRespectToDate(Date.ParseExact(Me.StartTime, "HHmm", Nothing).TimeOfDay, Date.ParseExact(EndTime, "HHmm", Nothing).TimeOfDay).TotalMilliseconds + If(NextDay, TimeSpan.FromDays(1), TimeSpan.FromDays(0)).TotalMilliseconds)
                '        Dim projectedSpacing As Decimal
                '        projectedSpacing = avgSpacing
                '        projectionBox2.Coordinates = New Rect(Max(projectionBox.Coordinates.Right + projectedSpacing, CurrentBar.Number), If(projectionBox2.Location.Y <> 0, projectionBox2.Location.Y, CurrentBar.Close), avgWidth, avgHeight)
                '        With projectionBox2
                '            .MaxTopValue = CurrentBar.Close
                '            .MaxBottomValue = CurrentBar.Close
                '            .Location = New Point(.Location.X, Max(.MaxBottomValue, .Location.Y))
                '            .Location = New Point(.Location.X, Min(.MaxTopValue, .Location.Y - .Height) + .Height)
                '        End With
                '    End If
            End If

        End Sub

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
            If (dateTime.TimeOfDay > firstDateTime.TimeOfDay And dateTime.TimeOfDay <= secondDateTime.TimeOfDay And firstDateTime.Date = secondDateTime.Date) Or
                   ((dateTime.TimeOfDay > firstDateTime.TimeOfDay Or dateTime.TimeOfDay < secondDateTime.TimeOfDay) And firstDateTime.Date <> secondDateTime.Date) Or (firstDateTime.Date - secondDateTime.Date).TotalDays > 1 Then
                Return True
            Else
                Return False
            End If
        End Function

        Public Overrides Property Name As String = "DayBox3"


        Public Enum TimeDisplayStyle
            hhmmss
            HHMM
            MMDDYY_HHMMSS
            MMDDYY_HHMM
        End Enum
        Private timer As System.Windows.Forms.Timer
        Private timeLabel As Label
        Private Sub Elapsed()

            If Chart IsNot Nothing And timeLabel IsNot Nothing Then
                Dim timeString As String = ""
                Dim startMarkerIndex As Integer
                Dim currentMarker As Char
                Dim i As Integer
                While i < ClockDisplayStyle.Length
                    If currentMarker = Char.MinValue Then
                        Select Case LCase(ClockDisplayStyle(i))
                            Case "m", "d", "y", "h", "s", "t"

                                If (i > 0 AndAlso ClockDisplayStyle(i - 1) <> "\") OrElse i = 0 Then
                                    currentMarker = ClockDisplayStyle(i)
                                    startMarkerIndex = i
                                Else
                                    timeString &= ClockDisplayStyle(i)
                                End If
                            Case "\"
                                'If (i > 0 AndAlso DisplayStyle(i - 1) = "\") OrElse i = 0 Then
                                '    timeString &= "\"
                                'End If
                            Case Else
                                'If i > 1 AndAlso DisplayStyle(i - 1) = "\" Then
                                '    timeString &= "\"
                                'End If
                                timeString &= ClockDisplayStyle(i)
                        End Select
                    Else
                        If ClockDisplayStyle(i) <> currentMarker Or i = ClockDisplayStyle.Length - 1 Then
                            Dim num As Integer = i - startMarkerIndex
                            Select Case currentMarker
                                Case "M"
                                    timeString &= FormatNumberLength(CInt(RepString(Now.Month, Ceiling(num / 2))), num)
                                Case "d"
                                    timeString &= FormatNumberLength(RepString(Now.Day, Ceiling(num / 2)), num)
                                Case "D"
                                    Dim day As String = [Enum].GetName(GetType(DayOfWeek), Now.DayOfWeek)
                                    If num >= day.Length Then
                                        timeString &= day
                                    Else
                                        timeString &= day.Substring(0, num)
                                    End If
                                Case "y", "Y"
                                    If num <= 2 Then
                                        timeString &= Now.Year.ToString.Substring(2)
                                    Else
                                        timeString &= RepString(Now.Year.ToString, (num + 1) \ 4)
                                    End If
                                Case "h"
                                    timeString &= FormatNumberLength(RepString(Now.Hour Mod 12, Ceiling(num / 2)), num)
                                Case "H"
                                    timeString &= FormatNumberLength(RepString(Now.Hour, Ceiling(num / 2)), num)
                                Case "m"
                                    timeString &= FormatNumberLength(RepString(Now.Minute, Ceiling(num / 2)), num)
                                Case "s", "S"
                                    timeString &= FormatNumberLength(RepString(Now.Second, Ceiling(num / 2)), num)
                                Case "t", "T"
                                    timeString &= RepString(If(Now.Hour >= 12, "PM", "AM"), Ceiling(num / 2))
                            End Select
                            currentMarker = Char.MinValue
                            startMarkerIndex = 0
                            i -= 1
                        End If
                    End If
                    i += 1
                End While
                timeLabel.Text = timeString

                'If ShowClockText Then timeLabel.Location = New Point(projectionBox.Location.X + projectionBox.Width / 2, projectionBox.Location.Y - projectionBox.Height)
                'If ShowTimeInfoTexts Then
                '    startTimeInfoText.Location = New Point(projectionBox.Location.X, projectionBox.Location.Y - projectionBox.Height)
                '    endTimeInfoText.Location = New Point(projectionBox.Location.X + projectionBox.Width, projectionBox.Location.Y - projectionBox.Height)

                '    If projectionBox.Location.X < Chart.bars.Count And projectionBox.Location.X > 0 Then
                '        startTimeInfoText.Text = Chart.bars(projectionBox.Location.X).Data.Date.ToString("HHmm")
                '    Else
                '        startTimeInfoText.Text = ""
                '    End If
                '    If projectionBox.Location.X + projectionBox.Width < Chart.bars.Count And projectionBox.Location.X + projectionBox.Width > 0 Then
                '        endTimeInfoText.Text = Chart.bars(projectionBox.Location.X + projectionBox.Width).Data.Date.ToString("HHmm")
                '    Else
                '        endTimeInfoText.Text = ""
                '    End If
                'End If
                'If ShowDayRange Then
                '    projectionRangeText.Location = New Point(projectionBox.Location.X + projectionBox.Width, projectionBox.Location.Y)
                '    projectionRangeText.Text = Round(GetDollarValue(projectionBox.Height), 0)
                'End If

                'Font = timeLabel.Font.FontFamily
                'FontSize = timeLabel.Font.FontSize
                'FontColor = CType(timeLabel.Font.Brush, SolidColorBrush).Color
                'FontWeight = timeLabel.Font.FontWeight
                'FontStyle = timeLabel.Font.FontStyle
            End If
        End Sub


    End Class
End Namespace
