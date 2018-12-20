Imports System.Collections.ObjectModel

Namespace AnalysisTechniques
    Public Class DayBox2

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

        Private _endTime As String = "1600"
        <Input> Public Property EndTime As String
            Get
                Return _endTime
            End Get
            Set(value As String)
                If IsNumeric(value) Then
                    _endTime = FormatNumberLength(CInt(value), 4)
                End If
            End Set
        End Property
        <Input> Public Property AdjustTimesOnChartPad As Boolean = False

        <Input("", "Display")>
        Public Property LineColor As Color = Colors.LightGray
        <Input> Public Property SecondaryBoxLineColor As Color = Colors.LightGray
        <Input> Public Property AscendingSecondaryBoxFillColor As Color = Colors.Lime
        <Input> Public Property DescendingSecondaryBoxFillColor As Color = Colors.Red
        <Input> Public Property BoxLineWidth As Double = 1
        Public Property DashStyle As DoubleCollection = (New DoubleCollectionConverter).ConvertFromString("11 6 3 6 3 6")

        <Input("", "Projections")> Public Property ProjectionBoxesOn As Boolean = True

        <Input> Public Property ProjectionBoxDivisionTimes As String = "1000,1200"
        <Input> Public Property ProjectionLineColor As Color = Colors.LightGray
        <Input> Public Property ProjectionLineWidth As Double = 1
        <Input> Public Property SecondaryProjectionLineColor As Color = Colors.LightGray
        <Input> Public Property SecondaryProjectionLineWidth As Double = 1
        <Input> Public Property NumberOfBoxesToUseAsAverage As Integer = 5
        <Input("A value to skew beteween percentage calculation and average based projections. Enter between 1 and 100. 1 is fully percentage based, and 100 is fully average.")> Public Property PercentageVsAverageSkew As Integer = 50


        Public Property LowTimeTextColor As Color = Colors.Red
        Public Property HighTimeTextColor As Color = Colors.Lime
        Public Property HighLowTextSize As Double = 16
        Public Property HighLowFontWeight As FontWeight = FontWeights.ExtraBold
        Public Property HighLowTextsVisible As Boolean = False
        Dim highAndLowTextSpacing As Decimal ' set in Begin() routine

        <Input("", "Misc")> Public Property ShowDayRange As Boolean = False
        <Input> Public Property DayRangeTextColor As Color = Colors.Blue
        Public Property ShowTimeInfoTexts As Boolean = False
        Public Property ShowClockText As Boolean = False
        Property ClockDisplayStyle As String = "hh:mm:ss tt"
        <Input> Public Property DayRangeTextSize As Decimal = 12
        Public Property ShowCloseDot As Boolean = False
        <Input> Public Property DisableSundayBox As Boolean = True




        Private Class Box
            Public Property IsActive As Boolean = True
            Public Property RectObj As Rectangle
            Public Property PercentageRatios As List(Of Object())
            Public Property SpacingPercentageRatios As List(Of Object())
            Public Property CloseDot As TrendLine
            Public Property RangeText As Label
            Public Property TimeLineLowTimeText As Label
            Public Property TimeLineHighTimeText As Label
            Public Property SecondaryBoxes As List(Of Rectangle)
            Public Property SecondaryBoxRatios As List(Of Object())()
            Public Property SecondaryBoxPositionRatios As List(Of Double)
            Public Property SecondaryHigh As Point
            Public Property SecondaryLow As Point
        End Class



        Dim high As Point
        Dim low As Point
        Dim boxStartDate As Date
        Dim boxIsStarted As Boolean
        Dim boxes As List(Of Box)
        Dim projectionBox As Box
        Dim times As List(Of Date)

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
            boxes = New List(Of Box)
            boxIsStarted = False
            boxStartDate = Date.MinValue
            highAndLowTextSpacing = Chart.GetRelativeFromRealHeight(30)
            projectionBox = New Box
            projectionBox.RectObj = NewRectangle(ProjectionLineColor, Color.FromArgb(1, 0, 0, 0), New Point(0, 0), New Point(0, 0), ProjectionBoxesOn)
            projectionBox.RectObj.Pen.Thickness = ProjectionLineWidth
            projectionBox.RectObj.LockPositionOrientation = ChartDrawingVisual.LockPositionOrientationTypes.Vertically
            projectionBox.RectObj.CanResize = False
            projectionBox.RectObj.ShowSelectionHandles = False
            projectionBox.RectObj.IsEditable = False
            projectionBox.RangeText = NewLabel("", DayRangeTextColor, New Point(0, 0), ShowDayRange)
            projectionBox.RangeText.HorizontalAlignment = LabelHorizontalAlignment.Right
            projectionBox.RangeText.VerticalAlignment = LabelVerticalAlignment.Top
            projectionBox.RangeText.Font.FontSize = DayRangeTextSize
            projectionBox.RangeText.IsEditable = False
            projectionBox.RangeText.IsSelectable = False

            projectionBox.SecondaryBoxes = New List(Of Rectangle)
            For i = 1 To Split(ProjectionBoxDivisionTimes, ",").Count + 1
                projectionBox.SecondaryBoxes.Add(NewRectangle(SecondaryProjectionLineColor, Color.FromArgb(1, 0, 0, 0), New Point(0, 0), New Point(0, 0), ProjectionBoxesOn))
                projectionBox.SecondaryBoxes(projectionBox.SecondaryBoxes.Count - 1).Pen.Thickness = SecondaryProjectionLineWidth
                projectionBox.SecondaryBoxes(projectionBox.SecondaryBoxes.Count - 1).LockPositionOrientation = ChartDrawingVisual.LockPositionOrientationTypes.Vertically
                projectionBox.SecondaryBoxes(projectionBox.SecondaryBoxes.Count - 1).CanResize = False
                projectionBox.SecondaryBoxes(projectionBox.SecondaryBoxes.Count - 1).ShowSelectionHandles = False
                projectionBox.SecondaryBoxes(projectionBox.SecondaryBoxes.Count - 1).IsEditable = False
            Next


            timeLabel = NewLabel(Now.ToShortTimeString, LineColor, New Point(0, 0), ShowClockText)
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
            'startTimeInfoText = NewLabel("", LineColor, New Point(0, 0), ShowTimeInfoTexts)
            'endTimeInfoText = NewLabel("", LineColor, New Point(0, 0), ShowTimeInfoTexts)

            'If ShowTimeInfoTexts Then
            '    startTimeInfoText.HorizontalAlignment = LabelHorizontalAlignment.Right
            '    startTimeInfoText.VerticalAlignment = LabelVerticalAlignment.Top
            '    startTimeInfoText.IsEditable = False
            '    startTimeInfoText.IsSelectable = False
            '    endTimeInfoText.HorizontalAlignment = LabelHorizontalAlignment.Left
            '    endTimeInfoText.VerticalAlignment = LabelVerticalAlignment.Top
            '    endTimeInfoText.IsEditable = False
            '    endTimeInfoText.IsSelectable = False
            '    startTimeInfoText.Font.FontSize += 1
            '    endTimeInfoText.Font.FontSize += 1
            '    startTimeInfoText.Font.FontWeight = FontWeights.Bold
            '    endTimeInfoText.Font.FontWeight = FontWeights.Bold
            'End If
        End Sub

        Private Property CurrentBox As Box
            Get
                Return boxes(boxes.Count - 1)
            End Get
            Set(value As Box)
                boxes(boxes.Count - 1) = value
            End Set
        End Property
        Private ReadOnly Property IsBoxActive As Boolean
            Get
                Return boxes.Count > 0 AndAlso CurrentBox IsNot Nothing AndAlso CurrentBox.IsActive AndAlso CurrentBox.RectObj IsNot Nothing
            End Get
        End Property

        Private Sub StartBox()
            If boxes.Count > 0 AndAlso CurrentBox IsNot Nothing AndAlso CurrentBox.RectObj IsNot Nothing Then
                With CurrentBox
                    If .RectObj.Coordinates.Right = CurrentBar.Number - 1 Then
                        .RectObj.Width += 1
                        If .RectObj.Coordinates.Right - 1 <= Chart.bars.Count - 1 And .RectObj.Coordinates.Right - 1 >= 0 And ShowCloseDot Then
                            .CloseDot.Coordinates = New LineCoordinates(.RectObj.Coordinates.Right, Bars(.RectObj.Coordinates.Right - 1).Close, .RectObj.Coordinates.Right + 1, Bars(.RectObj.Coordinates.Right - 1).Close)
                        Else
                            .CloseDot.Coordinates = New LineCoordinates(0, 0, 0, 0)
                        End If
                    End If
                End With
            End If
            boxes.Add(New Box)
            With CurrentBox
                .RectObj = NewRectangle(LineColor, Colors.Transparent, New Point(CurrentBar.Number, CurrentBar.High), New Point(CurrentBar.Number, CurrentBar.Low))
                .RectObj.Pen.DashStyle = New DashStyle(DashStyle, 0)
                .RectObj.Pen.Thickness = BoxLineWidth ' If(boxes.Count <= NumberOfBoxesToUseAsAverage, BoxLineWidth, 0)
                .RectObj.IsSelectable = False
                .RectObj.IsEditable = False

                .CloseDot = NewTrendLine(LineColor, New Point(CurrentBar.Number, CurrentBar.High), New Point(CurrentBar.Number, CurrentBar.Low), ShowCloseDot)

                .RangeText = NewLabel("", DayRangeTextColor, New Point(0, 0), ShowDayRange)
                .RangeText.HorizontalAlignment = LabelHorizontalAlignment.Left
                .RangeText.VerticalAlignment = LabelVerticalAlignment.Top
                .RangeText.Font.FontSize = DayRangeTextSize
                .RangeText.IsEditable = False
                .RangeText.IsSelectable = False

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

                If boxes.Count > 1 Then
                    .SpacingPercentageRatios = New List(Of Object())
                    Dim l = Double.MaxValue, h = Double.MinValue
                    Dim spaceBetween As Integer = .RectObj.Location.X - boxes(boxes.Count - 2).RectObj.Coordinates.Right
                    Dim start As Integer = boxes(boxes.Count - 2).RectObj.Coordinates.Right
                    For i = boxes(boxes.Count - 2).RectObj.Coordinates.Right To .RectObj.Location.X
                        l = Min(l, Chart.bars(i - 1).Data.Low)
                        h = Max(h, Chart.bars(i - 1).Data.High)
                        .SpacingPercentageRatios.Add({Chart.bars(i - 1).Data.Date, (i - start) / spaceBetween})
                    Next
                End If

                .SecondaryBoxes = New List(Of Rectangle)
                .SecondaryBoxes.Add(NewRectangle(SecondaryBoxLineColor, Colors.Transparent, New Point(CurrentBar.Number, CurrentBar.High), New Point(CurrentBar.Number, CurrentBar.Low)))
                .SecondaryBoxes(.SecondaryBoxes.Count - 1).Tag = StartTime
                .SecondaryBoxes(.SecondaryBoxes.Count - 1).Pen.Thickness = BoxLineWidth
                .SecondaryBoxes(.SecondaryBoxes.Count - 1).Pen.DashStyle = New DashStyle(DashStyle, 0)
                .SecondaryBoxes(.SecondaryBoxes.Count - 1).IsSelectable = False
                .SecondaryBoxes(.SecondaryBoxes.Count - 1).IsEditable = False
                .SecondaryLow = New Point(CurrentBar.Number, CurrentBar.Low)
                .SecondaryHigh = New Point(CurrentBar.Number, CurrentBar.High)
            End With
            If ShowCloseDot Then
                CurrentBox.CloseDot.Pen.Thickness = 5
                CurrentBox.CloseDot.IsSelectable = False
            End If


            Dim totalWidth As Integer = 0
            Dim totalHeight As Double = 0

            low = New Point(CurrentBar.Number, CurrentBar.Low)
            high = New Point(CurrentBar.Number, CurrentBar.High)

        End Sub
        Private Sub StopBox()
            If IsBoxActive Then

                If CurrentBar.High >= high.Y Then high = New Point(CurrentBar.Number, CurrentBar.High)
                If CurrentBar.Low <= low.Y Then low = New Point(CurrentBar.Number, CurrentBar.Low)

                With CurrentBox
                    .RectObj.Coordinates = New Rect(.RectObj.Location.X, high.Y, CurrentBar.Number - .RectObj.Location.X, Abs(low.Y - high.Y))
                    .RectObj.Pen.Thickness = BoxLineWidth
                    If .RectObj.Coordinates.Right - 1 <= Chart.bars.Count - 1 And .RectObj.Coordinates.Right - 1 >= 0 And ShowCloseDot Then
                        .CloseDot.Coordinates = New LineCoordinates(.RectObj.Coordinates.Right, Bars(.RectObj.Coordinates.Right - 1).Close, .RectObj.Coordinates.Right + 1, Bars(.RectObj.Coordinates.Right - 1).Close)
                    Else
                        .CloseDot.Coordinates = New LineCoordinates(0, 0, 0, 0)
                    End If
                    For Each box In projectionBox.SecondaryBoxes
                        box.Pen.Thickness = 0
                    Next

                    If ShowDayRange Then .RangeText.Location = New Point(.RectObj.Location.X, high.Y)
                    .PercentageRatios = New List(Of Object())
                    Dim l As Double = Double.MaxValue, h As Double = Double.MinValue
                    Dim range As Double = Abs(high.Y - low.Y)
                    For i = .RectObj.Location.X + 1 To .RectObj.Location.X + .RectObj.Width
                        l = Min(l, Chart.bars(i - 1).Data.Low)
                        h = Max(h, Chart.bars(i - 1).Data.High)
                        .PercentageRatios.Add({Chart.bars(i - 1).Data.Date, (i - .RectObj.Location.X) / (.RectObj.Width), (h - l) / range})
                    Next


                    If CurrentBar.High >= .SecondaryHigh.Y Then .SecondaryHigh = New Point(CurrentBar.Number, CurrentBar.High)
                    If CurrentBar.Low <= .SecondaryLow.Y Then .SecondaryLow = New Point(CurrentBar.Number, CurrentBar.Low)
                    .SecondaryBoxes(.SecondaryBoxes.Count - 1).Coordinates = New Rect(.SecondaryBoxes(.SecondaryBoxes.Count - 1).Coordinates.X, .SecondaryHigh.Y, CurrentBar.Number - .SecondaryBoxes(.SecondaryBoxes.Count - 1).Coordinates.X, .SecondaryHigh.Y - .SecondaryLow.Y)
                    .SecondaryBoxes(.SecondaryBoxes.Count - 1).Pen.Thickness = BoxLineWidth

                    ReDim .SecondaryBoxRatios(.SecondaryBoxes.Count - 1)
                    For j = 0 To .SecondaryBoxes.Count - 1
                        .SecondaryBoxRatios(j) = New List(Of Object())
                        l = Double.MaxValue
                        h = Double.MinValue
                        For i = .SecondaryBoxes(j).Coordinates.Left + 1 To .SecondaryBoxes(j).Coordinates.Right
                            l = Min(l, Chart.bars(i - 1).Data.Low)
                            h = Max(h, Chart.bars(i - 1).Data.High)
                            .SecondaryBoxRatios(j).Add({Chart.bars(i - 1).Data.Date, (i - .SecondaryBoxes(j).Coordinates.Left) / (.SecondaryBoxes(j).Coordinates.Right - .SecondaryBoxes(j).Coordinates.Left), (h - l) / .SecondaryBoxes(j).Height})
                        Next
                    Next


                End With



                If boxes.Count >= NumberOfBoxesToUseAsAverage Then
                    Dim sumOfWidths As Decimal
                    Dim sumOfHeights As Decimal
                    Dim avgWidth As Decimal
                    Dim avgHeight As Decimal
                    For i = boxes.Count - NumberOfBoxesToUseAsAverage To boxes.Count - 1
                        sumOfWidths += boxes(i).RectObj.Width
                        sumOfHeights += boxes(i).RectObj.Height
                    Next
                    avgWidth = sumOfWidths / NumberOfBoxesToUseAsAverage
                    avgHeight = sumOfHeights / NumberOfBoxesToUseAsAverage
                    projectionBox.RectObj.Coordinates = New Rect(CurrentBox.RectObj.Coordinates.Right, Bars(CurrentBox.RectObj.Coordinates.Right - 1).Close + avgHeight / 2, avgWidth, avgHeight)

                End If

                If ShowCloseDot Then AddObjectToChart(CurrentBox.CloseDot)
            End If
            CurrentBox.IsActive = False
        End Sub

        Protected Overrides Sub Main()
            If CurrentBar.Number > 1 And Date.TryParseExact(Me.StartTime, "HHmm", Nothing, Globalization.DateTimeStyles.AssumeLocal, New Date) And Date.TryParseExact(Me.EndTime, "HHmm", Nothing, Globalization.DateTimeStyles.AssumeLocal, New Date) Then
                For Each t In Split(ProjectionBoxDivisionTimes, ",")
                    If Not Date.TryParseExact(t, "HHmm", Nothing, Globalization.DateTimeStyles.AssumeLocal, New Date) And ProjectionBoxDivisionTimes <> "" Then Exit Sub
                Next
                Dim startTime As Date = Date.ParseExact(Me.StartTime, "HHmm", Nothing)
                Dim endTime As Date = Date.ParseExact(Me.EndTime, "HHmm", Nothing)
                Dim barDate As Date = CurrentBar.Date
                Dim prevBarDate As Date = Chart.bars(CurrentBar.Number - 2).Data.Date
                If boxIsStarted Then
                    If IsTimeBetween(prevBarDate, barDate, endTime) Then
                        StopBox()
                        boxIsStarted = False
                    End If
                    If IsTimeBetween(prevBarDate, barDate, startTime) And Not boxIsStarted Then
                        StartBox()
                        boxStartDate = barDate
                        boxIsStarted = True
                    End If
                Else
                    'If IsTimeBetween(prevBarDate, barDate, startTime) And prevBarDate.Day = barDate.Day And boxes.Count = 0 Then
                    '    ' CreateBox(1, CurrentBar.Number - 1)
                    '    StartBox()
                    '    boxStartDate = barDate
                    '    boxIsStarted = True
                    'End If
                    If IsTimeBetween(prevBarDate, barDate, startTime) Then 'If IsTimeBetween(prevBarDate, barDate, startTime) AndAlso Not IsTimeBetween(prevBarDate, barDate, endTime) AndAlso ((DisableSundayBox And barDate.DayOfWeek <> DayOfWeek.Sunday) Or Not DisableSundayBox) Then
                        StartBox()
                        boxStartDate = barDate
                        boxIsStarted = True
                    End If
                    If Not NextDay() Then
                        If IsTimeBetween(prevBarDate, barDate, endTime) And boxIsStarted Then
                            StopBox()
                            boxIsStarted = False
                        End If
                    End If
                    'If NextDay() Then
                    '    If IsTimeBetween(prevBarDate, barDate, endTime) And ((NextDay() And barDate.Date > boxStartDate.Date) Or Not NextDay()) And boxIsStarted Then
                    '        StopBox()
                    '        boxIsStarted = False
                    '    End If
                    'Else
                    '    If IsTimeBetween(prevBarDate, barDate, endTime) And boxIsStarted Then
                    '        StopBox()
                    '        boxIsStarted = False
                    '    End If
                    'End If
                End If
                If CurrentBar.High >= high.Y Then high = New Point(CurrentBar.Number, CurrentBar.High)
                If CurrentBar.Low <= low.Y Then low = New Point(CurrentBar.Number, CurrentBar.Low)


                If IsBoxActive Then
                    If CurrentBar.High >= CurrentBox.SecondaryHigh.Y Then CurrentBox.SecondaryHigh = New Point(CurrentBar.Number, CurrentBar.High)
                    If CurrentBar.Low <= CurrentBox.SecondaryLow.Y Then CurrentBox.SecondaryLow = New Point(CurrentBar.Number, CurrentBar.Low)
                    Dim curSecondaryBox = CurrentBox.SecondaryBoxes(CurrentBox.SecondaryBoxes.Count - 1)
                    With CurrentBox
                        Dim projectedHeight As Decimal = high.Y - low.Y
                        Dim projectedWidth As Decimal = CurrentBar.Number - .RectObj.Location.X
                        If boxes.Count > NumberOfBoxesToUseAsAverage Then
                            Dim sumOfWidths As Decimal
                            Dim sumOfHeights As Decimal
                            Dim avgWidth As Decimal
                            Dim avgHeight As Decimal

                            Dim sumOfWidthPercentages As Decimal
                            Dim sumOfHeightPercentages As Decimal
                            Dim avgWidthPercentage As Decimal
                            Dim avgHeightPercentage As Decimal

                            Dim isDataCorrect As Boolean = True

                            For i = boxes.Count - NumberOfBoxesToUseAsAverage - 1 To boxes.Count - 2
                                If boxes(i).PercentageRatios IsNot Nothing Then
                                    Dim currentClosestDate As Date = Date.MinValue
                                    Dim matchedWidthPercentage As Decimal = 0
                                    Dim matchedHeightPercentage As Decimal = 0
                                    Dim currentDate As Date = CurrentBar.Date
                                    For Each item In boxes(i).PercentageRatios
                                        Dim d As Date = item(0)
                                        If Abs((currentDate.TimeOfDay - d.TimeOfDay).TotalSeconds) < Abs((currentDate.TimeOfDay - currentClosestDate.TimeOfDay).TotalSeconds) Or currentClosestDate = Date.MinValue Then
                                            currentClosestDate = d
                                            matchedWidthPercentage = item(1)
                                            matchedHeightPercentage = item(2)
                                        End If
                                    Next
                                    sumOfWidthPercentages += matchedWidthPercentage
                                    sumOfHeightPercentages += matchedHeightPercentage
                                Else
                                    isDataCorrect = False
                                    Exit For
                                End If
                                sumOfWidths += boxes(i).RectObj.Width
                                sumOfHeights += boxes(i).RectObj.Height
                            Next
                            avgWidthPercentage = sumOfWidthPercentages / NumberOfBoxesToUseAsAverage
                            avgHeightPercentage = sumOfHeightPercentages / NumberOfBoxesToUseAsAverage
                            avgWidth = sumOfWidths / NumberOfBoxesToUseAsAverage
                            avgHeight = sumOfHeights / NumberOfBoxesToUseAsAverage
                            If isDataCorrect And avgWidthPercentage <> 0 And avgHeightPercentage <> 0 Then
                                Dim progress = GetTimeDifferenceWithRespectToDate(CurrentBar.Date.TimeOfDay, startTime.TimeOfDay).TotalMilliseconds / If(endTime = startTime, TimeSpan.FromDays(1).TotalMilliseconds, GetTimeDifferenceWithRespectToDate(endTime.TimeOfDay, startTime.TimeOfDay).TotalMilliseconds)
                                projectedWidth = Max(CurrentBar.Number - .RectObj.Location.X, LinCalc(0, avgWidth, 1, (CurrentBar.Number - .RectObj.Location.X) / avgWidthPercentage, GetCalculationSkew(progress)))
                                projectedHeight = LinCalc(0, avgHeight, 1, (high.Y - low.Y) / avgHeightPercentage, GetCalculationSkew(progress))
                            Else
                                projectedWidth = avgWidth
                                projectedHeight = avgHeight
                            End If
                            projectionBox.RectObj.Fill = New SolidColorBrush(Color.FromArgb(1, 0, 0, 0))
                            If high.Y - low.Y > projectedHeight Then
                                projectionBox.RectObj.Coordinates = New Rect(.RectObj.Location.X, high.Y, projectedWidth, high.Y - low.Y)
                                projectionBox.RectObj.MaxBottomValue = high.Y
                                projectionBox.RectObj.MaxTopValue = low.Y
                                .RectObj.Pen.Thickness = BoxLineWidth
                            ElseIf projectionBox.RectObj.Width <> 0 Then
                                Dim offsetVal As Decimal
                                If low.Y < projectionBox.RectObj.Location.Y - projectionBox.RectObj.Height Then offsetVal = low.Y - (projectionBox.RectObj.Location.Y - projectionBox.RectObj.Height)
                                If high.Y > projectionBox.RectObj.Location.Y Then offsetVal = high.Y - projectionBox.RectObj.Location.Y
                                Dim heightGain = (projectedHeight - projectionBox.RectObj.Height)
                                Dim topsideLeniency = (projectionBox.RectObj.Location.Y - high.Y) / ((projectionBox.RectObj.Location.Y - high.Y) + (low.Y - (projectionBox.RectObj.Location.Y - projectionBox.RectObj.Height)))
                                projectionBox.RectObj.Coordinates = New Rect(.RectObj.Location.X, projectionBox.RectObj.Location.Y + offsetVal, projectedWidth, projectedHeight)
                                projectionBox.RectObj.Location = New Point(projectionBox.RectObj.Location.X, projectionBox.RectObj.Location.Y + topsideLeniency * heightGain)
                                projectionBox.RectObj.Height = projectedHeight
                                projectionBox.RectObj.MaxBottomValue = high.Y
                                projectionBox.RectObj.MaxTopValue = low.Y
                            End If

                            .RectObj.Coordinates = New Rect(.RectObj.Location.X, high.Y, CurrentBar.Number - .RectObj.Location.X, Abs(low.Y - high.Y))
                            .RectObj.Pen.Thickness = BoxLineWidth



                            If ProjectionBoxDivisionTimes <> "" Then

                                Dim secondarySumOfWidths As New Dictionary(Of String, Decimal)
                                Dim secondarySumOfHeights As New Dictionary(Of String, Decimal)
                                Dim secondaryAvgOfWidths As New Dictionary(Of String, Decimal)
                                Dim secondaryAvgOfHeights As New Dictionary(Of String, Decimal)

                                Dim secondarySumOfWidthPercentages As New Dictionary(Of String, Decimal)
                                Dim secondarySumOfHeightPercentages As New Dictionary(Of String, Decimal)
                                Dim secondaryAvgWidthPercentage As New Dictionary(Of String, Decimal)
                                Dim secondaryAvgHeightPercentage As New Dictionary(Of String, Decimal)

                                For i = boxes.Count - NumberOfBoxesToUseAsAverage - 1 To boxes.Count - 2 ' loop through each box 
                                    For k = 0 To boxes(i).SecondaryBoxes.Count - 1 ' loop through each little box inside of the main box
                                        Dim currentClosestDate As Date = Date.MinValue
                                        Dim matchedWidthPercentage As Decimal = 0
                                        Dim matchedHeightPercentage As Decimal = 0
                                        Dim currentDate As Date = CurrentBar.Date
                                        For Each item In boxes(i).SecondaryBoxRatios(k) ' loop through each bar inside of little box
                                            Dim d As Date = item(0)
                                            If Abs((currentDate.TimeOfDay - d.TimeOfDay).TotalSeconds) < Abs((currentDate.TimeOfDay - currentClosestDate.TimeOfDay).TotalSeconds) Or currentClosestDate = Date.MinValue Then
                                                currentClosestDate = d
                                                matchedWidthPercentage = item(1)
                                                matchedHeightPercentage = item(2)
                                            End If
                                        Next
                                        If Not secondarySumOfWidthPercentages.ContainsKey(boxes(i).SecondaryBoxes(k).Tag) Then secondarySumOfWidthPercentages.Add(boxes(i).SecondaryBoxes(k).Tag, 0)
                                        If Not secondarySumOfHeightPercentages.ContainsKey(boxes(i).SecondaryBoxes(k).Tag) Then secondarySumOfHeightPercentages.Add(boxes(i).SecondaryBoxes(k).Tag, 0)
                                        secondarySumOfWidthPercentages(boxes(i).SecondaryBoxes(k).Tag) += matchedWidthPercentage
                                        secondarySumOfHeightPercentages(boxes(i).SecondaryBoxes(k).Tag) += matchedHeightPercentage
                                    Next
                                Next
                                For Each item In secondarySumOfWidthPercentages
                                    If Not secondaryAvgWidthPercentage.ContainsKey(item.Key) Then secondaryAvgWidthPercentage.Add(item.Key, 0)
                                    If Not secondaryAvgHeightPercentage.ContainsKey(item.Key) Then secondaryAvgHeightPercentage.Add(item.Key, 0)
                                    secondaryAvgWidthPercentage(item.Key) = secondarySumOfWidthPercentages(item.Key) / NumberOfBoxesToUseAsAverage
                                    secondaryAvgHeightPercentage(item.Key) = secondarySumOfHeightPercentages(item.Key) / NumberOfBoxesToUseAsAverage
                                Next


                                For i = boxes.Count - NumberOfBoxesToUseAsAverage To boxes.Count - 1
                                    For Each secondBox In boxes(i).SecondaryBoxes
                                        If Not secondarySumOfWidths.ContainsKey(secondBox.Tag) Then secondarySumOfWidths.Add(secondBox.Tag, 0)
                                        If Not secondarySumOfHeights.ContainsKey(secondBox.Tag) Then secondarySumOfHeights.Add(secondBox.Tag, 0)
                                        secondarySumOfWidths(secondBox.Tag) += secondBox.Width
                                        secondarySumOfHeights(secondBox.Tag) += secondBox.Height
                                    Next
                                Next
                                For Each item In secondarySumOfWidths
                                    If Not secondaryAvgOfWidths.ContainsKey(item.Key) Then secondaryAvgOfWidths.Add(item.Key, 0)
                                    If Not secondaryAvgOfHeights.ContainsKey(item.Key) Then secondaryAvgOfHeights.Add(item.Key, 0)
                                    secondaryAvgOfWidths(item.Key) = secondarySumOfWidths(item.Key) / NumberOfBoxesToUseAsAverage
                                    secondaryAvgOfHeights(item.Key) = secondarySumOfHeights(item.Key) / NumberOfBoxesToUseAsAverage
                                Next




                                Dim totalFutureSecondaryWidthAverages As Decimal = 0
                                Dim totalFutureSecondaryWidthRemaining As Decimal = 0
                                For Each item In Split(ProjectionBoxDivisionTimes, ",")
                                    If Date.ParseExact(item, "HHmm", Nothing) > CurrentBar.Date Then
                                        If secondaryAvgOfWidths.ContainsKey(item) Then
                                            totalFutureSecondaryWidthAverages += secondaryAvgOfWidths(item)
                                        End If
                                    End If
                                Next
                                Dim j As Integer = 0
                                Dim curSecondaryBoxPosition As Decimal = 0
                                Dim list = {Me.StartTime}.Concat(Split(ProjectionBoxDivisionTimes, ","))
                                For Each item In list
                                    Dim secondaryCurProjectedWidth As Decimal = 0
                                    Dim secondaryCurProjectedHeight As Decimal = 0
                                    Dim secondaryCurStartTime As Date = Date.ParseExact(item, "HHmm", Nothing)
                                    Dim secondaryCurEndTime As Date = Date.ParseExact(If(j = list.Count - 1, Me.EndTime, list(j + 1)), "HHmm", Nothing)
                                    projectionBox.SecondaryBoxes(j).Pen.Thickness = SecondaryProjectionLineWidth
                                    projectionBox.SecondaryBoxes(j).Fill = New SolidColorBrush(Color.FromArgb(1, 0, 0, 0))

                                    If IsTimeBetweenTimeSpan(secondaryCurEndTime, endTime, CurrentBar.Date) And secondaryCurEndTime.TimeOfDay <> endTime.TimeOfDay Then ' box is completed
                                        projectionBox.SecondaryBoxes(j).Coordinates = New Rect(0, 0, 0, 0)
                                        projectionBox.SecondaryBoxes(j).Pen.Thickness = 0
                                        If .SecondaryBoxes.Count > j Then
                                            curSecondaryBoxPosition += .SecondaryBoxes(j).Width
                                        End If
                                    ElseIf IsTimeBetweenTimeSpan(secondaryCurStartTime, secondaryCurEndTime, CurrentBar.Date) Then
                                        projectionBox.SecondaryBoxes(j).Pen.Thickness = SecondaryProjectionLineWidth
                                        If secondaryAvgWidthPercentage(item) <> 0 And secondaryAvgHeightPercentage(item) <> 0 Then
                                            Dim progress = GetTimeDifferenceWithRespectToDate(CurrentBar.Date.TimeOfDay, secondaryCurStartTime.TimeOfDay).TotalMilliseconds / GetTimeDifferenceWithRespectToDate(secondaryCurEndTime.TimeOfDay, secondaryCurStartTime.TimeOfDay).TotalMilliseconds

                                            secondaryCurProjectedWidth = Max(CurrentBar.Number - curSecondaryBox.Location.X, LinCalc(0, secondaryAvgOfWidths(item), 1, (CurrentBar.Number - curSecondaryBox.Location.X) / secondaryAvgWidthPercentage(item), GetCalculationSkew(progress)))
                                            secondaryCurProjectedHeight = LinCalc(0, secondaryAvgOfHeights(item), 1, (.SecondaryHigh.Y - .SecondaryLow.Y) / secondaryAvgHeightPercentage(item), GetCalculationSkew(progress))
                                        Else
                                            secondaryCurProjectedWidth = secondaryAvgOfWidths(item)
                                            secondaryCurProjectedHeight = secondaryAvgOfHeights(item)
                                        End If
                                        If j = projectionBox.SecondaryBoxes.Count - 1 Then
                                            secondaryCurProjectedWidth = projectionBox.RectObj.Width - curSecondaryBoxPosition
                                        End If
                                        If Round(secondaryCurProjectedWidth, 2) > Round(projectionBox.RectObj.Width - curSecondaryBoxPosition, 2) Then
                                            secondaryCurProjectedWidth = secondaryCurProjectedWidth * ((projectionBox.RectObj.Width - curSecondaryBoxPosition) / (totalFutureSecondaryWidthAverages + secondaryCurProjectedWidth))
                                        End If
                                        Dim heightGain = (secondaryCurProjectedHeight - projectionBox.SecondaryBoxes(j).Height)
                                        Dim topsideLeniency = (projectionBox.SecondaryBoxes(j).Location.Y - .SecondaryHigh.Y) / ((projectionBox.SecondaryBoxes(j).Location.Y - .SecondaryHigh.Y) + (.SecondaryLow.Y - (projectionBox.SecondaryBoxes(j).Location.Y - projectionBox.SecondaryBoxes(j).Height)))
                                        projectionBox.SecondaryBoxes(j).Coordinates = New Rect(projectionBox.RectObj.Location.X + curSecondaryBoxPosition, projectionBox.SecondaryBoxes(j).Location.Y, Max(0, secondaryCurProjectedWidth), secondaryCurProjectedHeight)
                                        projectionBox.SecondaryBoxes(j).Location = New Point(projectionBox.SecondaryBoxes(j).Location.X, projectionBox.SecondaryBoxes(j).Location.Y + topsideLeniency * heightGain)
                                        projectionBox.SecondaryBoxes(j).MaxBottomValue = .SecondaryHigh.Y
                                        projectionBox.SecondaryBoxes(j).MaxTopValue = .SecondaryLow.Y

                                        curSecondaryBoxPosition += secondaryCurProjectedWidth
                                    ElseIf IsTimeBetweenTimeSpan(startTime, secondaryCurStartTime, CurrentBar.Date) Then ' box is projected and future
                                        If totalFutureSecondaryWidthRemaining = 0 Then totalFutureSecondaryWidthRemaining = projectionBox.RectObj.Width - curSecondaryBoxPosition
                                        If totalFutureSecondaryWidthAverages = 0 Then
                                            secondaryCurProjectedWidth = totalFutureSecondaryWidthRemaining
                                        Else
                                            secondaryCurProjectedWidth = secondaryAvgOfWidths(item) * (totalFutureSecondaryWidthRemaining / totalFutureSecondaryWidthAverages)
                                        End If
                                        If secondaryAvgOfHeights.ContainsKey(item) Then
                                            secondaryCurProjectedHeight = secondaryAvgOfHeights(item)
                                        End If
                                        If Double.IsNaN(projectionBox.RectObj.Location.Y) = False Then
                                            projectionBox.SecondaryBoxes(j).Coordinates = New Rect(projectionBox.RectObj.Location.X + curSecondaryBoxPosition, projectionBox.SecondaryBoxes(j).Location.Y, Max(0, secondaryCurProjectedWidth), secondaryCurProjectedHeight)
                                            projectionBox.SecondaryBoxes(j).MaxBottomValue = projectionBox.RectObj.Location.Y - projectionBox.RectObj.Height + projectionBox.SecondaryBoxes(j).Height
                                            projectionBox.SecondaryBoxes(j).MaxTopValue = projectionBox.RectObj.Location.Y - projectionBox.SecondaryBoxes(j).Height
                                            projectionBox.SecondaryBoxes(j).Location = New Point(projectionBox.SecondaryBoxes(j).Location.X, Max(Min(low.Y + projectionBox.RectObj.Height, projectionBox.SecondaryBoxes(j).Location.Y), high.Y - projectionBox.RectObj.Height + projectionBox.SecondaryBoxes(j).Height))
                                        End If
                                        curSecondaryBoxPosition += secondaryCurProjectedWidth
                                    End If


                                    j += 1
                                Next


                                'If isDataCorrect And secondaryCurAvgWidthPercentage <> 0 And secondaryCurAvgHeightPercentage <> 0 Then
                                '    Dim progress = GetTimeDifferenceWithRespectToDate(CurrentBar.Date.TimeOfDay, secondaryCurStartTime.TimeOfDay).TotalMilliseconds / GetTimeDifferenceWithRespectToDate(secondaryCurEndTime.TimeOfDay, secondaryCurStartTime.TimeOfDay).TotalMilliseconds
                                '    secondaryCurProjectedWidth = Max(CurrentBar.Number - curSecondaryBox.Location.X, LinCalc(0, secondaryCurAvgWidth, 1, (CurrentBar.Number - curSecondaryBox.Location.X) / secondaryCurAvgWidthPercentage, 2 * progress - progress ^ 2))
                                '    secondaryCurProjectedHeight = LinCalc(0, secondaryCurAvgHeight, 1, (.SecondaryHigh - .SecondaryHigh) / secondaryCurAvgHeightPercentage, 2 * progress - progress ^ 2)
                                'Else
                                '    secondaryCurProjectedWidth = secondaryCurAvgWidth
                                '    secondaryCurProjectedHeight = secondaryCurAvgHeight
                                'End If


                                'If .SecondaryHigh - .SecondaryLow > secondaryCurProjectedHeight Then
                                '    'curSecondaryBox.Coordinates = New Rect(0, 0, 0, 0)
                                '    'curSecondaryBox.Pen.Thickness = BoxLineWidth

                                '    'curSecondaryBox.IsSelectable = False
                                '    'curSecondaryBox.IsEditable = False

                                'Else
                                '    Dim curSecondaryOffsetVal As Decimal
                                '    If .SecondaryLow < curSecondaryBox.Location.Y - curSecondaryBox.Height Then curSecondaryOffsetVal = .SecondaryLow - (curSecondaryBox.Location.Y - curSecondaryBox.Height)
                                '    If .SecondaryHigh > curSecondaryBox.Location.Y Then curSecondaryOffsetVal = .SecondaryHigh - curSecondaryBox.Location.Y
                                '    curSecondaryBox.Coordinates = New Rect(curSecondaryBox.Location.X, curSecondaryBox.Location.Y + curSecondaryOffsetVal, secondaryCurProjectedWidth, secondaryCurProjectedHeight)
                                '    'curSecondaryBox.MaxBottomValue = high.Y
                                '    'curSecondaryBox.MaxTopValue = low.Y

                                'End If

                            End If




                        End If
                        If ProjectionBoxDivisionTimes <> "" Then
                            curSecondaryBox.Coordinates = New Rect(curSecondaryBox.Coordinates.X, .SecondaryHigh.Y, CurrentBar.Number - curSecondaryBox.Coordinates.X, .SecondaryHigh.Y - .SecondaryLow.Y)
                            curSecondaryBox.Fill = New SolidColorBrush(If(.SecondaryHigh.X > .SecondaryLow.X, AscendingSecondaryBoxFillColor, DescendingSecondaryBoxFillColor))
                            curSecondaryBox.Pen = New Pen(New SolidColorBrush(SecondaryBoxLineColor), BoxLineWidth)
                            curSecondaryBox.Pen.DashStyle = New DashStyle(DashStyle, 0)
                            Dim items = Split(ProjectionBoxDivisionTimes, ",")
                            For j = 0 To items.Count - 1
                                If IsTimeBetween(prevBarDate, barDate, Date.ParseExact(items(j), "HHmm", Nothing)) And curSecondaryBox.Location.X <> CurrentBar.Number Then
                                    'secondary box is completed
                                    .SecondaryBoxes(.SecondaryBoxes.Count - 1).Pen.Thickness = BoxLineWidth
                                    .SecondaryBoxes.Add(NewRectangle(SecondaryBoxLineColor, Colors.Transparent, New Point(CurrentBar.Number, CurrentBar.High), New Point(CurrentBar.Number, CurrentBar.Low)))
                                    .SecondaryBoxes(.SecondaryBoxes.Count - 1).Tag = items(j)
                                    .SecondaryBoxes(.SecondaryBoxes.Count - 1).Pen.Thickness = 0
                                    .SecondaryBoxes(.SecondaryBoxes.Count - 1).Pen.DashStyle = New DashStyle(DashStyle, 0)
                                    .SecondaryBoxes(.SecondaryBoxes.Count - 1).IsEditable = False
                                    .SecondaryBoxes(.SecondaryBoxes.Count - 1).IsSelectable = False
                                    .SecondaryHigh = New Point(CurrentBar.Number, CurrentBar.High)
                                    .SecondaryLow = New Point(CurrentBar.Number, CurrentBar.Low)
                                End If
                            Next
                        End If

                        .RectObj.Fill = New SolidColorBrush(Color.FromArgb(1, 0, 0, 0)) 'If(low.X < high.X, AscendingDayBoxFillColor, DescendingDayBoxFillColor))

                        If .RectObj.Coordinates.Right - 1 <= Chart.bars.Count - 1 And .RectObj.Coordinates.Right - 1 >= 0 And ShowCloseDot Then
                            .CloseDot.Coordinates = New LineCoordinates(.RectObj.Coordinates.Right, Bars(.RectObj.Coordinates.Right - 1).Close, .RectObj.Coordinates.Right + 1, Bars(.RectObj.Coordinates.Right - 1).Close)
                        Else
                            .CloseDot.Coordinates = New LineCoordinates(0, 0, 0, 0)
                        End If

                        If ShowDayRange Then
                            .RangeText.Location = New Point(.RectObj.Location.X, high.Y)
                            .RangeText.Text = Round(GetDollarValue(high.Y - low.Y), 0)
                        End If

                        .TimeLineLowTimeText.Location = New Point(low.X, low.Y - highAndLowTextSpacing)
                        Dim n = FormatNumberLength(Chart.bars(low.X - 1).Data.Date.Hour, 2) & FormatNumberLength(Chart.bars(low.X - 1).Data.Date.Minute, 2)
                        .TimeLineLowTimeText.Text = n(0) & vbNewLine & n(1) & vbNewLine & n(2) & vbNewLine & n(3) & vbNewLine
                        .TimeLineHighTimeText.Location = New Point(high.X, high.Y + highAndLowTextSpacing)
                        n = FormatNumberLength(Chart.bars(high.X - 1).Data.Date.Hour, 2) & FormatNumberLength(Chart.bars(high.X - 1).Data.Date.Minute, 2)
                        .TimeLineHighTimeText.Text = n(0) & vbNewLine & n(1) & vbNewLine & n(2) & vbNewLine & n(3) & vbNewLine



                    End With
                End If
            Else
                Exit Sub
            End If
            If IsBoxActive = False Then
                ' box is yet to start
                If boxes.Count > NumberOfBoxesToUseAsAverage Then
                    Dim sumOfSpacings As Decimal = 0
                    Dim sumOfSpacingPercentages As Decimal = 0
                    Dim avgSpacingPercentage As Decimal = 0
                    Dim avgSpacing As Decimal = 0

                    Dim sumOfWidths As Decimal = 0
                    Dim sumOfHeights As Decimal = 0
                    Dim avgWidth As Decimal = 0
                    Dim avgHeight As Decimal = 0

                    Dim isDataCorrect As Boolean = True
                    For i = boxes.Count - NumberOfBoxesToUseAsAverage To boxes.Count - 1
                        If boxes(i).SpacingPercentageRatios IsNot Nothing Then
                            Dim currentClosestDate As Date = Date.MinValue
                            Dim matchedSpacingPercentage As Decimal = 0
                            Dim currentDate As Date = CurrentBar.Date
                            For Each item In boxes(i).SpacingPercentageRatios
                                Dim d As Date = item(0)
                                If Abs(GetTimeDifferenceWithRespectToDate(currentDate.TimeOfDay, d.TimeOfDay).TotalSeconds) < Abs(GetTimeDifferenceWithRespectToDate(currentDate.TimeOfDay, currentClosestDate.TimeOfDay).TotalSeconds) Or currentClosestDate = Date.MinValue Then
                                    currentClosestDate = d
                                    If Not Double.IsInfinity(CType(item(1), Double)) And Not Double.IsNaN(CDbl(item(1))) Then matchedSpacingPercentage = item(1)
                                End If
                            Next
                            sumOfSpacingPercentages += matchedSpacingPercentage
                        Else
                            isDataCorrect = False
                            Exit For
                        End If
                        sumOfSpacings += boxes(i).RectObj.Coordinates.Left - boxes(i - 1).RectObj.Coordinates.Right
                        sumOfWidths += boxes(i).RectObj.Width
                        sumOfHeights += boxes(i).RectObj.Height
                    Next
                    avgSpacingPercentage = sumOfSpacingPercentages / NumberOfBoxesToUseAsAverage
                    avgSpacing = sumOfSpacings / NumberOfBoxesToUseAsAverage
                    avgWidth = sumOfWidths / NumberOfBoxesToUseAsAverage
                    avgHeight = sumOfHeights / NumberOfBoxesToUseAsAverage

                    If isDataCorrect Then
                        Dim progress = GetTimeDifferenceWithRespectToDate(CurrentBar.Date.TimeOfDay, Date.ParseExact(EndTime, "HHmm", Nothing).TimeOfDay).TotalMilliseconds / If(StartTime = EndTime, TimeSpan.FromDays(1).TotalMilliseconds, GetTimeDifferenceWithRespectToDate(Date.ParseExact(Me.StartTime, "HHmm", Nothing).TimeOfDay, Date.ParseExact(EndTime, "HHmm", Nothing).TimeOfDay).TotalMilliseconds)
                        Dim projectedSpacing As Decimal
                        If avgSpacingPercentage = 0 Then
                            projectedSpacing = avgSpacing
                        Else
                            projectedSpacing = LinCalc(0, avgSpacing, 1, (CurrentBar.Number - CurrentBox.RectObj.Coordinates.Right) / avgSpacingPercentage, 2 * progress - progress ^ 2)
                        End If
                        projectionBox.RectObj.Coordinates = New Rect(Max(CurrentBox.RectObj.Coordinates.Right + projectedSpacing, CurrentBar.Number), projectionBox.RectObj.Location.Y, avgWidth, avgHeight)
                        projectionBox.RectObj.MaxTopValue = CurrentBar.Close
                        projectionBox.RectObj.MaxBottomValue = CurrentBar.Close

                        Dim secondarySumOfWidths As New Dictionary(Of String, Decimal)
                        Dim secondarySumOfHeights As New Dictionary(Of String, Decimal)
                        Dim secondaryAvgOfWidths As New Dictionary(Of String, Decimal)
                        Dim secondaryAvgOfHeights As New Dictionary(Of String, Decimal)
                        For i = boxes.Count - NumberOfBoxesToUseAsAverage To boxes.Count - 1
                            For Each secondBox In boxes(i).SecondaryBoxes
                                If Not secondarySumOfWidths.ContainsKey(secondBox.Tag) Then secondarySumOfWidths.Add(secondBox.Tag, 0)
                                If Not secondarySumOfHeights.ContainsKey(secondBox.Tag) Then secondarySumOfHeights.Add(secondBox.Tag, 0)
                                secondarySumOfWidths(secondBox.Tag) += secondBox.Width
                                secondarySumOfHeights(secondBox.Tag) += secondBox.Height
                            Next
                        Next
                        For Each item In secondarySumOfWidths
                            If Not secondaryAvgOfWidths.ContainsKey(item.Key) Then secondaryAvgOfWidths.Add(item.Key, 0)
                            If Not secondaryAvgOfHeights.ContainsKey(item.Key) Then secondaryAvgOfHeights.Add(item.Key, 0)
                            secondaryAvgOfWidths(item.Key) = secondarySumOfWidths(item.Key) / NumberOfBoxesToUseAsAverage
                            secondaryAvgOfHeights(item.Key) = secondarySumOfHeights(item.Key) / NumberOfBoxesToUseAsAverage
                        Next

                        Dim j As Integer = 0
                        Dim curSecondaryBoxPosition As Decimal
                        For Each item In secondaryAvgOfWidths
                            projectionBox.SecondaryBoxes(j).Coordinates = New Rect(projectionBox.RectObj.Location.X + curSecondaryBoxPosition, projectionBox.RectObj.Location.Y, secondaryAvgOfWidths(item.Key), secondaryAvgOfHeights(item.Key))
                            'projectionBox.SecondaryBoxes(j).MaxTopValue = CurrentBar.Close
                            'projectionBox.SecondaryBoxes(j).MaxBottomValue = CurrentBar.Close
                            projectionBox.SecondaryBoxes(j).Pen.Thickness = ProjectionLineWidth
                            curSecondaryBoxPosition += secondaryAvgOfWidths(item.Key)
                            j += 1
                        Next

                    End If
                End If
            End If
        End Sub
        Private Function NextDay() As Boolean
            Return StartTime >= EndTime
        End Function
        Private Function GetCalculationSkew(value As Double) As Double
            If PercentageVsAverageSkew > 50 And PercentageVsAverageSkew <= 100 Then
                Return value ^ ((PercentageVsAverageSkew - 50) / 2)
            ElseIf PercentageVsAverageSkew < 50 And PercentageVsAverageSkew >= 1 Then
                Return 1 - (1 - value) ^ ((50 - PercentageVsAverageSkew) / 2)
            Else
                Return value
            End If
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

        Private Function IsTimeBetweenTimeSpan(firstDateTime As Date, secondDateTime As Date, dateTime As Date) As Boolean
            If (dateTime.TimeOfDay > firstDateTime.TimeOfDay And dateTime.TimeOfDay < secondDateTime.TimeOfDay And firstDateTime.TimeOfDay < secondDateTime.TimeOfDay) Or
                       ((dateTime.TimeOfDay > firstDateTime.TimeOfDay Or dateTime.TimeOfDay < secondDateTime.TimeOfDay) And firstDateTime.TimeOfDay >= secondDateTime.TimeOfDay) Then

                Return True
            Else
                Return False
            End If

        End Function
        Private Function IsTimeBetween(firstDateTime As Date, secondDateTime As Date, dateTime As Date) As Boolean
            If (dateTime.TimeOfDay > firstDateTime.TimeOfDay And dateTime.TimeOfDay <= secondDateTime.TimeOfDay And firstDateTime.Date = secondDateTime.Date) Or
                       ((dateTime.TimeOfDay > firstDateTime.TimeOfDay Or dateTime.TimeOfDay < secondDateTime.TimeOfDay) And firstDateTime.Date <> secondDateTime.Date) Then

                Return True
            Else
                Return False
            End If

        End Function

        Public Overrides Property Name As String = "DayBox2"



        Public Enum TimeDisplayStyle
            hhmmss
            HHMM
            MMDDYY_HHMMSS
            MMDDYY_HHMM
        End Enum
        Private timer As System.Windows.Forms.Timer
        Private timeLabel As Label
        Private Sub Elapsed()
#If Not DEBUG Then
            Try
#End If
            If Chart IsNot Nothing And timeLabel IsNot Nothing And projectionBox IsNot Nothing And IsEnabled Then
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

                    If ShowClockText Then timeLabel.Location = New Point(projectionBox.RectObj.Location.X + projectionBox.RectObj.Width / 2, projectionBox.RectObj.Location.Y - projectionBox.RectObj.Height - Chart.Settings("RangeValue").Value / 2)
                    'If ShowTimeInfoTexts Then
                    '    startTimeInfoText.Location = New Point(projectionBox.RectObj.Location.X, projectionBox.RectObj.Location.Y - projectionBox.RectObj.Height)
                    '    endTimeInfoText.Location = New Point(projectionBox.RectObj.Location.X + projectionBox.RectObj.Width, projectionBox.RectObj.Location.Y - projectionBox.RectObj.Height)

                    '    If projectionBox.RectObj.Location.X < Chart.bars.Count And projectionBox.RectObj.Location.X > 0 Then
                    '        startTimeInfoText.Text = Chart.bars(projectionBox.RectObj.Location.X).Data.Date.ToString("HHmm")
                    '    Else
                    '        startTimeInfoText.Text = ""
                    '    End If
                    '    If projectionBox.RectObj.Location.X + projectionBox.RectObj.Width < Chart.bars.Count And projectionBox.RectObj.Location.X + projectionBox.RectObj.Width > 0 Then
                    '        endTimeInfoText.Text = Chart.bars(projectionBox.RectObj.Location.X + projectionBox.RectObj.Width).Data.Date.ToString("HHmm")
                    '    Else
                    '        endTimeInfoText.Text = ""
                    '    End If
                    'End If
                    If ShowDayRange Then
                        projectionBox.RangeText.Location = New Point(projectionBox.RectObj.Location.X + projectionBox.RectObj.Width, projectionBox.RectObj.Location.Y)
                        projectionBox.RangeText.Text = Round(GetDollarValue(projectionBox.RectObj.Height), 0)
                    End If
                    Dim canContinue As Boolean = True
                    If CurrentBar.Number > 1 And Date.TryParseExact(Me.StartTime, "HHmm", Nothing, Globalization.DateTimeStyles.AssumeLocal, New Date) And Date.TryParseExact(Me.EndTime, "HHmm", Nothing, Globalization.DateTimeStyles.AssumeLocal, New Date) Then
                        For Each t In Split(ProjectionBoxDivisionTimes, ",")
                            If Not Date.TryParseExact(t, "HHmm", Nothing, Globalization.DateTimeStyles.AssumeLocal, New Date) Then canContinue = False
                        Next
                    Else
                        canContinue = False
                    End If
                    If boxes.Count > 0 AndAlso CurrentBox IsNot Nothing And canContinue And projectionBox.SecondaryBoxes.Count - 1 = Split(ProjectionBoxDivisionTimes, ",").Count Then
                        Dim j As Integer = 0
                        Dim list = {Me.StartTime}.Concat(Split(ProjectionBoxDivisionTimes, ","))
                        For Each item In list
                            Dim secondaryCurStartTime As Date = Date.ParseExact(item, "HHmm", Nothing)
                            Dim secondaryCurEndTime As Date = Date.ParseExact(If(j = list.Count - 1, Me.EndTime, list(j + 1)), "HHmm", Nothing)
                            With projectionBox.SecondaryBoxes(j)
                                If IsBoxActive Then
                                    If IsTimeBetweenTimeSpan(secondaryCurStartTime, secondaryCurEndTime, CurrentBar.Date) Then

                                        'if box is current
                                        If Double.IsNaN(.Location.Y) Then .Location = New Point(.Location.X, 0)
                                        If Not Double.IsNaN(projectionBox.RectObj.Location.Y) Then
                                            .MaxBottomValue = Max(CurrentBox.SecondaryHigh.Y - .Height, projectionBox.RectObj.Location.Y - projectionBox.RectObj.Height) + .Height
                                            .MaxTopValue = Min(CurrentBox.SecondaryLow.Y + .Height, projectionBox.RectObj.Location.Y) - .Height

                                            .Location = New Point(.Location.X, Max(.MaxBottomValue, .Location.Y))
                                            .Location = New Point(.Location.X, Min(.MaxTopValue, .Location.Y - .Height) + .Height)
                                        Else
                                            .Location = New Point(.Location.X, CurrentBox.SecondaryHigh.Y - .Height)
                                            .Location = New Point(.Location.X, CurrentBox.SecondaryLow.Y + .Height)
                                        End If
                                        'If j <> 0 Then
                                        '    Dim linkedToTopSide As Boolean = Abs(projectionBox.RectObj.Location.Y - CurrentBox.SecondaryBoxes(0).Location.Y) > Abs((CurrentBox.SecondaryBoxes(0).Location.Y - CurrentBox.SecondaryBoxes(0).Height) - (projectionBox.RectObj.Location.Y - projectionBox.RectObj.Height))
                                        '    If linkedToTopSide Then
                                        '        .Location = New Point(.Location.X, Min(.MaxTopValue, projectionBox.RectObj.Location.Y) + .Height)
                                        '    Else
                                        '        .Location = New Point(.Location.X, Max(.MaxBottomValue, projectionBox.RectObj.Location.Y - projectionBox.RectObj.Height + .Height))
                                        '    End If
                                        'End If
                                    Else
                                    'if box is free floating
                                    If Double.IsNaN(projectionBox.RectObj.Location.Y) = False Then
                                        .MaxBottomValue = projectionBox.RectObj.Location.Y - projectionBox.RectObj.Height + .Height
                                        .MaxTopValue = projectionBox.RectObj.Location.Y - .Height
                                        .Location = New Point(.Location.X, Max(.MaxBottomValue, .Location.Y))
                                        .Location = New Point(.Location.X, Min(.MaxTopValue, .Location.Y - .Height) + .Height)
                                    End If
                                    Dim linkedToTopSide As Boolean
                                        If CurrentBox.SecondaryBoxes.Count > 1 Then
                                            linkedToTopSide = projectionBox.RectObj.Location.Y - CurrentBox.SecondaryBoxes(0).Location.Y > ((CurrentBox.SecondaryBoxes(0).Location.Y - CurrentBox.SecondaryBoxes(0).Height) - (projectionBox.RectObj.Location.Y - projectionBox.RectObj.Height))
                                        Else
                                            linkedToTopSide = projectionBox.RectObj.Location.Y - projectionBox.SecondaryBoxes(0).Location.Y > ((projectionBox.SecondaryBoxes(0).Location.Y - projectionBox.SecondaryBoxes(0).Height) - (projectionBox.RectObj.Location.Y - projectionBox.RectObj.Height))
                                        End If
                                        If linkedToTopSide Then
                                            .Location = New Point(.Location.X, Min(.MaxTopValue, projectionBox.RectObj.Location.Y) + .Height)
                                        Else
                                            .Location = New Point(.Location.X, Max(.MaxBottomValue, projectionBox.RectObj.Location.Y - projectionBox.RectObj.Height + .Height))
                                        End If

                                        'projectionBox.SecondaryBoxes(j).Location = New Point(projectionBox.SecondaryBoxes(j).Location.X, Max(Min(low.Y + projectionBox.RectObj.Height, projectionBox.SecondaryBoxes(j).Location.Y), high.Y - projectionBox.RectObj.Height + projectionBox.SecondaryBoxes(j).Height))

                                    End If
                                Else
                                    .MaxBottomValue = projectionBox.RectObj.Location.Y - projectionBox.RectObj.Height + .Height
                                    .MaxTopValue = projectionBox.RectObj.Location.Y - .Height
                                    .Location = New Point(.Location.X, Max(.MaxBottomValue, .Location.Y))
                                    .Location = New Point(.Location.X, Min(.MaxTopValue, .Location.Y - .Height) + .Height)
                                    If j <> 0 Then
                                        Dim linkedToTopSide As Boolean = Abs(projectionBox.RectObj.Location.Y - projectionBox.SecondaryBoxes(0).Location.Y) > Abs((projectionBox.SecondaryBoxes(0).Location.Y - projectionBox.SecondaryBoxes(0).Height) - (projectionBox.RectObj.Location.Y - projectionBox.RectObj.Height))
                                        If linkedToTopSide Then
                                            .Location = New Point(.Location.X, Min(.MaxTopValue, projectionBox.RectObj.Location.Y) + .Height)
                                        Else
                                            .Location = New Point(.Location.X, Max(.MaxBottomValue, projectionBox.RectObj.Location.Y - projectionBox.RectObj.Height + .Height))
                                        End If
                                    End If
                                End If
                            End With

                            j += 1
                        Next
                    End If
                End If
#If Not DEBUG Then
            Catch
                ShowInfoBox("DayBox2 ran into an error on chart " & Chart.TickerID & ". It will still function, but automatic projection placement is disabled. Remove and re-add the DayBox2 to resume operation.", Chart.DesktopWindow)
            End Try
#End If
        End Sub


    End Class
End Namespace
