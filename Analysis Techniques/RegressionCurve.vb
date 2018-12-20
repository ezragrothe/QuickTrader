'Imports QuickTrader.Methods
'Namespace AnalysisTechniques
'    Public Class RegressionCurve
'        Inherits AnalysisTechnique

'        <Input("The number of bars to use to determine the average price.")>
'        Public Property RegressionLength As Integer = 40

'        <Input("The color for the regression line.", "Center Lines")>
'        Public Property CenterLineColor As Color = Colors.Blue
'        <Input("The color for the trailing regression line.")>
'        Public Property CenterLineThickness As Decimal = 0.3
'        <Input("The option to show the center curved line.")>
'        Public Property ShowCenterCurvedLine As Boolean = True
'        <Input("The color for the center channel line.")>
'        Public Property ChannelCenterLineColor As Color = Colors.Transparent
'        <Input("The option to show the trailing plot.")>
'        Public Property ShowTrailingLine As Boolean = False
'        <Input("The line thickness of the outer curved lines.")>
'        Public Property TrailingLineColor As Color = ColorConverter.ConvertFromString("#FF904C00")
'        <Input("The line thickness of the center regression line.")>
'        Public Property TrailingLineThickness As Decimal = 1

'        <Input("Determines whether to show the outer channel lines.", "Outer Channel Lines")>
'        Public Property ShowOuterChannelLines As Boolean = False
'        <Input("The line thickness of the outer channel lines.")>
'        Public Property OuterChannelLineThickness As Decimal = 0.8
'        <Input("The color of the outer channel lines.")>
'        Public Property OuterChannelLineColor As Color = Colors.Black
'        <Input("The line thickness of the trailing line.", "Outer Curved Lines")>
'        Public Property ShowOuterCurvedLines1 As Boolean = False
'        <Input()> Public Property UseMultiplierForOuterLine1Offset As Boolean = True
'        <Input()> Public Property OffsetValueMultiplier1 As Decimal = 1
'        <Input()> Public Property OuterLineThickness1 As Decimal = 0.4
'        <Input()> Public Property OuterLineColor1 As Color = Colors.Brown
'        <Input()> Public Property ShowOuterCurvedLines2 As Boolean = True
'        <Input()> Public Property OffsetValueMultiplier2 As Decimal = 4
'        <Input()> Public Property OuterLineThickness2 As Decimal = 0.6
'        <Input()> Public Property OuterLineColor2 As Color = Colors.Blue

'        <Input("", "Bar Coloring")> Public Property ColorBars As Boolean = True
'        <Input("")> Public Property AboveBarColor As Color = Colors.Green
'        <Input()> Public Property BelowBarColor As Color = Colors.Red
'        <Input()> Public Property BarColoringAlgorithm As RegressionBarColoringAlgorithm = RegressionBarColoringAlgorithm.CurvedLineBased
'        <Input("", "Misc")> Public Property ShowBox As Boolean = False
'        <Input()> Public Property BoxColor As Color = Colors.Black
'        <Input()> Public Property BoxThickness As Decimal = 1
'        <Input()> Public Property AddFillLines As Boolean = True
'        <Input()> Public Property FillLineThickness As Decimal = 1.9
'        <Input()> Public Property FillLineUpColor As Color = ColorConverter.ConvertFromString("#20008000")
'        <Input()> Public Property FillLineDownColor As Color = ColorConverter.ConvertFromString("#20FF0000")

'        Enum RegressionBarColoringAlgorithm
'            StraightLineBased
'            CurvedLineBased
'        End Enum

'        Public Sub New(ByVal chart As Chart)
'            MyBase.New(chart)
'            Description = "Draws a regression curve that follows the price."
'        End Sub

'        Dim plot As Plot
'        Dim abovePlot As Plot
'        Dim belowPlot As Plot
'        Dim abovePlot2 As Plot
'        Dim belowPlot2 As Plot
'        Dim trailingPlot As Plot
'        Dim aboveFillPlot As Plot
'        Dim belowFillPlot As Plot
'        'Dim slider As UIChartControl
'        'Dim originalLocation As New Point(60, 120)
'        Dim currentLine() As TrendLine
'        Dim boxLines(3) As TrendLine
'        Private barColorsDirty As Boolean
'        Protected Overrides Sub Begin()
'            MyBase.Begin()
'            'slider = New UIChartControl
'            'slider.AnalysisTechnique = Me
'            'Dim contentSlider As New Slider
'            'contentSlider.Minimum = 70
'            'contentSlider.Maximum = 120
'            'contentSlider.VerticalAlignment = VerticalAlignment.Center
'            'contentSlider.HorizontalAlignment = HorizontalAlignment.Stretch
'            'contentSlider.Value = Length
'            'contentSlider.Background = New SolidColorBrush(Chart.Settings("Background").Value)
'            'contentSlider.Margin = New Thickness(0, 0, 0, 0)
'            'contentSlider.LargeChange = 5
'            'contentSlider.SmallChange = 5
'            'AddHandler contentSlider.ValueChanged, AddressOf SliderMove
'            'Dim gridBackground As New Grid
'            'gridBackground.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})
'            'gridBackground.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(30, GridUnitType.Pixel)})
'            'gridBackground.Background = New SolidColorBrush(Color.FromArgb(150, 200, 200, 200))
'            'gridBackground.Children.Add(contentSlider)
'            'Dim text As New TextBlock With {.HorizontalAlignment = HorizontalAlignment.Center, .VerticalAlignment = VerticalAlignment.Center, .Text = contentSlider.Value}
'            'Grid.SetColumn(text, 1)
'            'gridBackground.Children.Add(text)
'            'slider.Content = gridBackground
'            'slider.Width = 200
'            'slider.Height = 24
'            'slider.Left = originalLocation.X
'            'slider.Top = originalLocation.Y
'            'slider.IsDraggable = True
'            'AddObjectToChart(slider)
'            plot = NewPlot(CenterLineColor)
'            plot.IsSelectable = False
'            abovePlot = NewPlot(OuterLineColor1)
'            abovePlot.IsSelectable = False
'            abovePlot2 = NewPlot(OuterLineColor2)
'            abovePlot2.IsSelectable = False
'            belowPlot = NewPlot(OuterLineColor1)
'            belowPlot.IsSelectable = False
'            belowPlot2 = NewPlot(OuterLineColor2)
'            belowPlot2.IsSelectable = False
'            trailingPlot = NewPlot(TrailingLineColor)
'            plot.Pen = New Pen(New SolidColorBrush(CenterLineColor), CenterLineThickness)
'            abovePlot.Pen = New Pen(New SolidColorBrush(OuterLineColor1), OuterLineThickness1)
'            belowPlot.Pen = New Pen(New SolidColorBrush(OuterLineColor1), OuterLineThickness1)
'            abovePlot2.Pen = New Pen(New SolidColorBrush(OuterLineColor2), OuterLineThickness2)
'            belowPlot2.Pen = New Pen(New SolidColorBrush(OuterLineColor2), OuterLineThickness2)
'            aboveFillPlot = NewPlot(FillLineUpColor)
'            belowFillPlot = NewPlot(FillLineDownColor)
'            aboveFillPlot.IsSelectable = False
'            belowFillPlot.IsSelectable = False
'            aboveFillPlot.Pen.Thickness = FillLineThickness
'            belowFillPlot.Pen.Thickness = FillLineThickness
'            currentLine = {NewTrendLine(CenterLineColor, New Point(0, 0), New Point(0, 0), ShowOuterChannelLines), NewTrendLine(CenterLineColor, New Point(0, 0), New Point(0, 0), ShowOuterChannelLines), NewTrendLine(CenterLineColor, New Point(0, 0), New Point(0, 0))}
'            For j = 0 To 2
'                currentLine(j).Pen = New Pen(New SolidColorBrush(OuterChannelLineColor), OuterChannelLineThickness)
'                currentLine(j).ExtendRight = True
'                currentLine(j).IsSelectable = False
'                currentLine(j).IsEditable = False
'            Next
'            For j = 0 To 3
'                boxLines(j) = NewTrendLine(New Pen(New SolidColorBrush(BoxColor), BoxThickness), ShowBox)
'                boxLines(j).IsSelectable = False
'                boxLines(j).IsEditable = False
'            Next
'            currentLine(2).Pen = New Pen(New SolidColorBrush(ChannelCenterLineColor), CenterLineThickness)
'        End Sub
'        Public Overrides Sub Reset()
'            'If slider IsNot Nothing Then
'            '    originalLocation = New Point(slider.Left, slider.Top)
'            '    RemoveHandler CType(CType(slider.Content, Grid).Children(0), Slider).ValueChanged, AddressOf SliderMove
'            '    slider = Nothing
'            'End If

'            MyBase.Reset()
'            If barColorsDirty And Chart.bars.Count > 1 And ColorBars Then
'                BarColor(0, Chart.bars.Count - 1, Chart.Settings("Bar Color").Value)
'                barColorsDirty = False
'            End If
'        End Sub
'        Protected Sub BarColor(ByVal startBar As Integer, ByVal endBar As Integer, ByVal color As Color)
'            For i = startBar To endBar
'                If Not TypeOf Chart.bars(i).Pen.Brush Is SolidColorBrush OrElse CType(Chart.bars(i).Pen.Brush, SolidColorBrush).Color <> color Then
'                    Chart.bars(i).Pen.Brush = New SolidColorBrush(color)
'                    RefreshObject(Chart.bars(i))
'                End If
'            Next
'        End Sub
'        'Private Sub SliderMove(sender As Object, e As EventArgs)
'        '    If barColorsDirty And Chart.bars.Count > 1 And ColorBars Then
'        '        BarColor(0, Chart.bars.Count - 1, Chart.Settings("Bar Color").Value)
'        '        barColorsDirty = False
'        '    End If
'        '    plot.Points.Clear()
'        '    abovePlot.Points.Clear()
'        '    belowPlot.Points.Clear()
'        '    abovePlot2.Points.Clear()
'        '    belowPlot2.Points.Clear()
'        '    trailingPlot.Points.Clear()
'        '    For i = GetLength() + 2 To Chart.bars.Count
'        '        UpdateLine(i - GetLength(), i, False)
'        '        If ColorBars Then
'        '            If BarColoringAlgorithm = RegressionBarColoringAlgorithm.CurvedLineBased Then
'        '                If Chart.bars(i - 1).Data.High < currentLine(2).EndPoint.Y Or Chart.bars(i - 1).Data.Low > currentLine(2).EndPoint.Y Then
'        '                    CType(Chart.bars(i - 1).Pen.Brush, SolidColorBrush).Color = If(Chart.bars(i - 1).Data.Close > currentLine(2).EndPoint.Y, AboveBarColor, BelowBarColor)
'        '                Else
'        '                    Dim brush As New LinearGradientBrush With {.StartPoint = New Point(0, 0), .EndPoint = New Point(0, 1)}
'        '                    brush.GradientStops.Add(New GradientStop(AboveBarColor, 1 - (currentLine(2).EndPoint.Y - Chart.bars(i - 1).Data.Low) / (Chart.bars(i - 1).Data.High - Chart.bars(i - 1).Data.Low)))
'        '                    brush.GradientStops.Add(New GradientStop(BelowBarColor, 1 - (currentLine(2).EndPoint.Y - Chart.bars(i - 1).Data.Low) / (Chart.bars(i - 1).Data.High - Chart.bars(i - 1).Data.Low)))
'        '                    Chart.bars(i - 1).Pen.Brush = brush
'        '                End If
'        '                RefreshObject(Chart.bars(i - 1))
'        '            Else
'        '                For j = i - GetLength() To i - 1
'        '                    Dim value As Decimal = LinCalc(currentLine(2).Coordinates, j + 1)
'        '                    If Chart.bars(j).Data.Close > value Then
'        '                        If CType(Chart.bars(j).Pen.Brush, SolidColorBrush).Color <> AboveBarColor Then
'        '                            Chart.bars(j).Pen.Brush = New SolidColorBrush(AboveBarColor)
'        '                            RefreshObject(Chart.bars(j))
'        '                        End If
'        '                    Else
'        '                        If CType(Chart.bars(j).Pen.Brush, SolidColorBrush).Color <> BelowBarColor Then
'        '                            Chart.bars(j).Pen.Brush = New SolidColorBrush(BelowBarColor)
'        '                            RefreshObject(Chart.bars(j))
'        '                        End If
'        '                    End If
'        '                Next
'        '            End If
'        '        End If
'        '    Next
'        '    barColorsDirty = True
'        '    CType(CType(slider.Content, Grid).Children(1), TextBlock).Text = GetLength()
'        '    plot.RefreshVisual()
'        '    ' If ShowOuterChannelLines Then
'        '    abovePlot.RefreshVisual()
'        '    belowPlot.RefreshVisual()
'        '    abovePlot2.RefreshVisual()
'        '    belowPlot2.RefreshVisual()
'        '    ' End If
'        '    'If ShowTrailingLine Then
'        '    trailingPlot.RefreshVisual()
'        '    'End If
'        '    Length = GetLength()
'        'End Sub
'        Protected Overrides Sub Main()
'            If ColorBars Then
'                barColorsDirty = True
'                If BarColoringAlgorithm = RegressionBarColoringAlgorithm.CurvedLineBased Then
'                    If Chart.bars(CurrentBar.Number - 1).Data.High < currentLine(2).EndPoint.Y Or Chart.bars(CurrentBar.Number - 1).Data.Low > currentLine(2).EndPoint.Y Then
'                        Chart.bars(CurrentBar.Number - 1).Pen.Brush = New SolidColorBrush(If(Chart.bars(CurrentBar.Number - 1).Data.Close > currentLine(2).EndPoint.Y, AboveBarColor, BelowBarColor))
'                    Else
'                        Dim brush As New LinearGradientBrush With {.StartPoint = New Point(0, 0), .EndPoint = New Point(0, 1)}
'                        brush.GradientStops.Add(New GradientStop(AboveBarColor, 1 - (currentLine(2).EndPoint.Y - Chart.bars(CurrentBar.Number - 1).Data.Low) / (Chart.bars(CurrentBar.Number - 1).Data.High - Chart.bars(CurrentBar.Number - 1).Data.Low)))
'                        brush.GradientStops.Add(New GradientStop(BelowBarColor, 1 - (currentLine(2).EndPoint.Y - Chart.bars(CurrentBar.Number - 1).Data.Low) / (Chart.bars(CurrentBar.Number - 1).Data.High - Chart.bars(CurrentBar.Number - 1).Data.Low)))
'                        Chart.bars(CurrentBar.Number - 1).Pen.Brush = brush
'                    End If
'                    RefreshObject(Chart.bars(CurrentBar.Number - 1))
'                Else
'                    For i = CurrentBar.Number - RegressionLength To CurrentBar.Number - 1
'                        Dim value As Decimal = LinCalc(currentLine(2).Coordinates, i + 1)
'                        If Not TypeOf Chart.bars(i).Pen.Brush Is SolidColorBrush Then Chart.bars(i).Pen.Brush = New SolidColorBrush
'                        If Chart.bars(i).Data.Close > value Then
'                            If CType(Chart.bars(i).Pen.Brush, SolidColorBrush).Color <> AboveBarColor Then
'                                Chart.bars(i).Pen.Brush = New SolidColorBrush(AboveBarColor)
'                                RefreshObject(Chart.bars(i))
'                            End If
'                        Else
'                            If CType(Chart.bars(i).Pen.Brush, SolidColorBrush).Color <> BelowBarColor Then
'                                Chart.bars(i).Pen.Brush = New SolidColorBrush(BelowBarColor)
'                                RefreshObject(Chart.bars(i))
'                            End If
'                        End If
'                    Next
'                End If
'            End If
'        End Sub
'        'Private Function GetLength() As Integer
'        '    Return Round(CType(CType(slider.Content, Grid).Children(0), Slider).Value)
'        'End Function
'        Protected Overrides Sub NewBar()
'            If CurrentBar.Number > RegressionLength + 1 Then
'                UpdateLine(CurrentBar.Number - RegressionLength, CurrentBar.Number, True)
'            End If
'        End Sub
'        Private Sub UpdateLine(startX As Integer, endX As Integer, shouldRefresh As Boolean)
'            Dim highBar As New Point, lowBar As New Point
'            Dim maxAboveDistance As Double, maxBelowDistance As Double
'            Dim direction As Direction = If((Chart.bars(endX - 1).Data.High - Chart.bars(endX - 1).Data.Low) / 2 + Chart.bars(endX - 1).Data.High > (Chart.bars(startX - 1).Data.High - Chart.bars(startX - 1).Data.Low) / 2 + Chart.bars(startX - 1).Data.Low, True, False)
'            Dim middleLine As LineCoordinates = CalculateNewStartPointRegressionMiddle(startX - 1, startX, endX, endX - 1)
'            Dim dir As Direction = If((Chart.bars(endX - 1).Data.High - Chart.bars(endX - 1).Data.Low) / 2 + Chart.bars(endX - 1).Data.High > (Chart.bars(startX).Data.High - Chart.bars(startX).Data.Low) / 2 + Chart.bars(startX).Data.Low, True, False)
'            Dim yMin As Decimal = Decimal.MaxValue, yMax As Decimal
'            For i = startX - 1 To endX - 1
'                If Bars(i).High > yMax Then yMax = Bars(i).High
'                If Bars(i).Low < yMin Then yMin = Bars(i).Low
'                Dim aboveDistance = Sin(Atan((middleLine.EndPoint.X - middleLine.StartPoint.X) / Abs((middleLine.StartPoint.Y - middleLine.EndPoint.Y)))) * (Bars(i).High - LinCalc(middleLine.StartPoint.X, middleLine.StartPoint.Y, middleLine.EndPoint.X, middleLine.EndPoint.Y, i + 1))
'                Dim belowDistance = Sin(Atan((middleLine.EndPoint.X - middleLine.StartPoint.X) / Abs((middleLine.StartPoint.Y - middleLine.EndPoint.Y)))) * (LinCalc(middleLine.StartPoint.X, middleLine.StartPoint.Y, middleLine.EndPoint.X, middleLine.EndPoint.Y, i + 1) - Bars(i).Low)
'                If aboveDistance >= maxAboveDistance Then
'                    If (dir = direction.Down And Bars(i - 1).High < Bars(i).High) Or dir = direction.Up Then
'                        maxAboveDistance = aboveDistance
'                        highBar = New Point(i + 1, Bars(i).High)
'                    End If
'                End If
'                If belowDistance >= maxBelowDistance Then
'                    If (dir = direction.Up And Bars(i - 1).Low > Bars(i).Low) Or dir = direction.Down Then
'                        maxBelowDistance = belowDistance
'                        lowBar = New Point(i + 1, Bars(i).Low)
'                    End If
'                End If
'            Next
'            Dim rangeBar As Point = If(dir = direction.Down, highBar, lowBar)
'            If rangeBar.Y <> 0 Then 'if it is not a perfect swing with no retracements
'                Dim minDistance As Decimal = Min(Abs(highBar.Y - LinCalc(middleLine.StartPoint.X, middleLine.StartPoint.Y, middleLine.EndPoint.X, middleLine.EndPoint.Y, highBar.X)),
'                                                     Abs(lowBar.Y - LinCalc(middleLine.StartPoint.X, middleLine.StartPoint.Y, middleLine.EndPoint.X, middleLine.EndPoint.Y, lowBar.X)))

'                currentLine(2).Coordinates = New LineCoordinates(middleLine.StartPoint, middleLine.EndPoint)
'                If (AddFillLines And (plot.Points.Count = 0 OrElse plot.Points(plot.Points.Count - 1) <> middleLine.EndPoint)) And (Chart.bars(middleLine.EndPoint.X - 1).Data.Low > middleLine.EndPoint.Y Or Chart.bars(middleLine.EndPoint.X - 1).Data.High < middleLine.EndPoint.Y) Then
'                    If Chart.bars(middleLine.EndPoint.X - 1).Data.Close > middleLine.EndPoint.Y Then
'                        aboveFillPlot.AddNonContinuousPoint(AddToX(middleLine.EndPoint, 0))
'                        aboveFillPlot.AddPoint(New Point(middleLine.EndPoint.X, Chart.bars(middleLine.EndPoint.X - 1).Data.Low))
'                    Else
'                        belowFillPlot.AddNonContinuousPoint(AddToX(middleLine.EndPoint, 0))
'                        belowFillPlot.AddPoint(New Point(middleLine.EndPoint.X, Chart.bars(middleLine.EndPoint.X - 1).Data.High))
'                    End If
'                    'Dim tl = NewTrendLine(If(Chart.bars(middleLine.EndPoint.X - 1).Data.Close > middleLine.EndPoint.Y, AboveBarColor, BelowBarColor), AddToX(middleLine.EndPoint, 0), New Point(middleLine.EndPoint.X, If(Chart.bars(middleLine.EndPoint.X - 1).Data.Close > middleLine.EndPoint.Y, Chart.bars(middleLine.EndPoint.X - 1).Data.Low, Chart.bars(middleLine.EndPoint.X - 1).Data.High)))
'                    'tl.IsSelectable = False
'                    'tl.Pen = New Pen(tl.Pen.Brush, FillLineThickness)
'                End If
'                If ShowCenterCurvedLine And (plot.Points.Count = 0 OrElse plot.Points(plot.Points.Count - 1) <> middleLine.EndPoint) Then plot.AddPoint(middleLine.EndPoint, loadingHistory = False And shouldRefresh)

'                If UseMultiplierForOuterLine1Offset Then
'                    currentLine(0).StartPoint = New Point(endX + 1 - RegressionLength, middleLine.StartPoint.Y - OffsetValueMultiplier1 * Chart.Settings("RangeValue").Value)
'                    currentLine(0).EndPoint = New Point(endX + 1, middleLine.EndPoint.Y - OffsetValueMultiplier1 * Chart.Settings("RangeValue").Value)
'                    currentLine(1).StartPoint = New Point(endX + 1 - RegressionLength, middleLine.StartPoint.Y + OffsetValueMultiplier1 * Chart.Settings("RangeValue").Value)
'                    currentLine(1).EndPoint = New Point(endX + 1, middleLine.EndPoint.Y + OffsetValueMultiplier1 * Chart.Settings("RangeValue").Value)
'                    If ShowOuterCurvedLines1 And (abovePlot.Points.Count = 0 OrElse abovePlot.Points(abovePlot.Points.Count - 1) <> AddToXY(middleLine.EndPoint, 1, Abs(OffsetValueMultiplier1 * Chart.Settings("RangeValue").Value))) Then abovePlot.AddPoint(AddToXY(middleLine.EndPoint, 1, Abs(OffsetValueMultiplier1 * Chart.Settings("RangeValue").Value)), loadingHistory = False And shouldRefresh)
'                    If ShowOuterCurvedLines1 And (belowPlot.Points.Count = 0 OrElse belowPlot.Points(belowPlot.Points.Count - 1) <> AddToXY(middleLine.EndPoint, 1, -Abs(OffsetValueMultiplier1 * Chart.Settings("RangeValue").Value))) Then belowPlot.AddPoint(AddToXY(middleLine.EndPoint, 1, -Abs(OffsetValueMultiplier1 * Chart.Settings("RangeValue").Value)), loadingHistory = False And shouldRefresh)
'                    If ShowOuterCurvedLines2 And (abovePlot2.Points.Count = 0 OrElse abovePlot2.Points(abovePlot2.Points.Count - 1) <> AddToXY(middleLine.EndPoint, 1, Abs(OffsetValueMultiplier2 * Chart.Settings("RangeValue").Value))) Then abovePlot2.AddPoint(AddToXY(middleLine.EndPoint, 1, Abs(OffsetValueMultiplier2 * Chart.Settings("RangeValue").Value)), loadingHistory = False And shouldRefresh)
'                    If ShowOuterCurvedLines2 And (belowPlot2.Points.Count = 0 OrElse belowPlot2.Points(belowPlot2.Points.Count - 1) <> AddToXY(middleLine.EndPoint, 1, -Abs(OffsetValueMultiplier2 * Chart.Settings("RangeValue").Value))) Then belowPlot2.AddPoint(AddToXY(middleLine.EndPoint, 1, -Abs(OffsetValueMultiplier2 * Chart.Settings("RangeValue").Value)), loadingHistory = False And shouldRefresh)
'                    If ShowTrailingLine And (trailingPlot.Points.Count = 0 OrElse trailingPlot.Points(trailingPlot.Points.Count - 1) <> AddToX(middleLine.StartPoint, 1)) Then trailingPlot.AddPoint(AddToX(middleLine.StartPoint, 1), loadingHistory = False And shouldRefresh)
'                Else
'                    currentLine(0).StartPoint = New Point(middleLine.StartPoint.X, middleLine.StartPoint.Y - minDistance)
'                    currentLine(0).EndPoint = New Point(middleLine.EndPoint.X, LinCalc(middleLine, middleLine.EndPoint.X) - minDistance)
'                    currentLine(1).StartPoint = New Point(middleLine.StartPoint.X, middleLine.StartPoint.Y + minDistance)
'                    currentLine(1).EndPoint = New Point(middleLine.EndPoint.X, LinCalc(middleLine, middleLine.EndPoint.X) + minDistance)
'                    If ShowOuterCurvedLines1 And (abovePlot.Points.Count = 0 OrElse abovePlot.Points(abovePlot.Points.Count - 1) <> currentLine(0).EndPoint) Then abovePlot.AddPoint(currentLine(0).EndPoint, loadingHistory = False And shouldRefresh)
'                    If ShowOuterCurvedLines1 And (belowPlot.Points.Count = 0 OrElse belowPlot.Points(belowPlot.Points.Count - 1) <> currentLine(1).EndPoint) Then belowPlot.AddPoint(currentLine(1).EndPoint, loadingHistory = False And shouldRefresh)
'                    If ShowOuterCurvedLines2 And (abovePlot2.Points.Count = 0 OrElse abovePlot2.Points(abovePlot2.Points.Count - 1) <> AddToXY(middleLine.EndPoint, 1, Abs(OffsetValueMultiplier2 * Chart.Settings("RangeValue").Value))) Then abovePlot2.AddPoint(AddToXY(middleLine.EndPoint, 1, Abs(OffsetValueMultiplier2 * Chart.Settings("RangeValue").Value)), loadingHistory = False And shouldRefresh)
'                    If ShowOuterCurvedLines2 And (belowPlot2.Points.Count = 0 OrElse belowPlot2.Points(belowPlot2.Points.Count - 1) <> AddToXY(middleLine.EndPoint, 1, -Abs(OffsetValueMultiplier2 * Chart.Settings("RangeValue").Value))) Then belowPlot2.AddPoint(AddToXY(middleLine.EndPoint, 1, -Abs(OffsetValueMultiplier2 * Chart.Settings("RangeValue").Value)), loadingHistory = False And shouldRefresh)
'                    If ShowTrailingLine And (trailingPlot.Points.Count = 0 OrElse trailingPlot.Points(trailingPlot.Points.Count - 1) <> middleLine.StartPoint) Then trailingPlot.AddPoint(middleLine.StartPoint, loadingHistory = False And shouldRefresh)
'                End If
'                If ShowBox Then
'                    boxLines(0).Coordinates = New LineCoordinates(startX, yMin, startX, yMax)
'                    boxLines(1).Coordinates = New LineCoordinates(startX, yMax, endX, yMax)
'                    boxLines(2).Coordinates = New LineCoordinates(endX, yMax, endX, yMin)
'                    boxLines(3).Coordinates = New LineCoordinates(endX, yMin, startX, yMin)
'                End If
'            End If
'        End Sub
'        Private Function CalculateNewStartPointRegressionMiddle(startPointX As Decimal, lineStartPoint As Decimal, lineEndPoint As Decimal, Optional endPointX As Decimal = -1) As LineCoordinates
'            If endPointX = -1 Then endPointX = CurrentBar.Number - 1
'            Dim n As Decimal = (endPointX + 1) - startPointX
'            Dim a As Decimal, b As Decimal
'            Dim sumx2 As Decimal = 0, sumx As Decimal = 0, sumy As Decimal = 0, sumxy As Decimal = 0
'            For bari = startPointX To endPointX
'                Dim point As New Point(bari, Chart.bars(bari).Data.Low + (Chart.bars(bari).Data.High - Chart.bars(bari).Data.Low) / 2)
'                sumx += point.X
'                sumy += point.Y
'                sumxy += point.X * point.Y
'                sumx2 += point.X ^ 2
'            Next
'            b = (n * sumxy - sumx * sumy) / (n * sumx2 - sumx * sumx)
'            a = (sumy - b * sumx) / n
'            Return New LineCoordinates(New Point(lineStartPoint, a + b * lineStartPoint), New Point(lineEndPoint, a + b * lineEndPoint))
'        End Function

'        Public Overrides Property Name As String = "RegressionCurve"
'    End Class

'End Namespace


''Imports QuickTrader.Methods
''Namespace AnalysisTechniques
''    Public Class RegressionCurve
''        Inherits AnalysisTechnique

''        <Input("The color for the regression line.")>
''        Public Property LineColor As Color = Colors.Red
''        <Input("The color for the trailing regression line.")>
''        Public Property TrailingLineColor As Color = Colors.Yellow
''        <Input("The color for the center channel line.")>
''        Public Property ChannelCenterLineColor As Color = Colors.Blue
''        <Input("The number of bars to use to determine the average price.")>
''        Public Property Length As Integer = 5
''        <Input("The line thickness of the center regression line.")>
''        Public Property CenterLineThickness As Decimal = 1
''        <Input("The line thickness of the trailing line.")>
''        Public Property TrailingLineThickness As Decimal = 1
''        <Input("The line thickness of the outer curved lines.")>
''        Public Property OuterLineDotThickness As Decimal = 1
''        <Input("The line thickness of the outer channel lines.")>
''        Public Property OuterLineChannelThickness As Decimal = 1
''        <Input("Determines whether to show the outer curved lines.")>
''        Public Property ShowOuterCurvedLines As Boolean = True
''        <Input("Determines whether to show the outer channel lines.")>
''        Public Property ShowOuterChannelLines As Boolean = True
''        <Input("The option to show the trailing plot.")>
''        Public Property ShowTrailingLine As Boolean = True
''        <Input("The multiplier for the curved outer lines.")>
''        Public Property OffsetValueMultiplier As Decimal
''        Public Sub New(ByVal chart As Chart)
''            MyBase.New(chart)
''            Description = "Draws a regression curve that follows the price."
''        End Sub

''        Dim plot As Plot
''        Dim abovePlot As Plot
''        Dim belowPlot As Plot
''        Dim trailingPlot As Plot
''        Dim slider As UIChartControl
''        Dim originalLocation As New Point(2, 28)
''        Dim currentLine() As TrendLine
''        Dim regressionLinesData As Dictionary(Of Integer, LineCoordinates)
''        Protected Overrides Sub Begin()
''            MyBase.Begin()
''            regressionLinesData = New Dictionary(Of Integer, LineCoordinates)
''            slider = New UIChartControl
''            slider.AnalysisTechnique = Me
''            Dim contentSlider As New Slider
''            contentSlider.Minimum = 20
''            contentSlider.Maximum = 200
''            contentSlider.VerticalAlignment = VerticalAlignment.Center
''            contentSlider.HorizontalAlignment = HorizontalAlignment.Stretch
''            contentSlider.Value = Length
''            contentSlider.Background = New SolidColorBrush(Chart.Settings("Background").Value)
''            contentSlider.Margin = New Thickness(0, 0, 0, 0)
''            contentSlider.LargeChange = 10
''            contentSlider.SmallChange = 10
''            AddHandler contentSlider.ValueChanged, AddressOf SliderMove
''            Dim gridBackground As New Grid
''            gridBackground.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})
''            gridBackground.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(30, GridUnitType.Pixel)})
''            gridBackground.Background = New SolidColorBrush(Color.FromArgb(150, 200, 200, 200))
''            gridBackground.Children.Add(contentSlider)
''            Dim text As New TextBlock With {.HorizontalAlignment = HorizontalAlignment.Center, .VerticalAlignment = VerticalAlignment.Center, .Text = contentSlider.Value}
''            Grid.SetColumn(text, 1)
''            gridBackground.Children.Add(text)
''            slider.Content = gridBackground
''            slider.Width = 400
''            slider.Height = 24
''            slider.Left = originalLocation.X
''            slider.Top = originalLocation.Y
''            slider.IsDraggable = True
''            AddObjectToChart(slider)
''            plot = NewPlot(LineColor)
''            'plot.IsSelectable = False
''            abovePlot = NewPlot(LineColor)
''            'abovePlot.IsSelectable = False
''            belowPlot = NewPlot(LineColor)
''            'belowPlot.IsSelectable = False
''            trailingPlot = NewPlot(LineColor)
''            'trailingPlot.IsSelectable = False
''            plot.Pen = New Pen(New SolidColorBrush(LineColor), CenterLineThickness)
''            abovePlot.Pen = New Pen(New SolidColorBrush(LineColor), OuterLineDotThickness)
''            belowPlot.Pen = New Pen(New SolidColorBrush(LineColor), OuterLineDotThickness)
''            trailingPlot.Pen = New Pen(New SolidColorBrush(TrailingLineColor), CenterLineThickness)
''            currentLine = {NewTrendLine(LineColor, New Point(0, 0), New Point(0, 0), ShowOuterChannelLines), NewTrendLine(LineColor, New Point(0, 0), New Point(0, 0), ShowOuterChannelLines), NewTrendLine(LineColor, New Point(0, 0), New Point(0, 0))}
''            For j = 0 To 2
''                currentLine(j).Pen = New Pen(New SolidColorBrush(LineColor), OuterLineChannelThickness)
''                currentLine(j).ExtendRight = True
''                currentLine(j).IsSelectable = False
''                currentLine(j).IsEditable = False
''            Next
''            currentLine(2).Pen.Brush = New SolidColorBrush(ChannelCenterLineColor)
''        End Sub
''        Public Overrides Sub Reset()
''            If slider IsNot Nothing Then
''                originalLocation = New Point(slider.Left, slider.Top)
''                RemoveHandler CType(CType(slider.Content, Grid).Children(0), Slider).ValueChanged, AddressOf SliderMove
''                slider = Nothing
''            End If
''            MyBase.Reset()
''        End Sub
''        Private Sub AddPlotPoint(point As Point, forceRefresh As Boolean)
''            plot.AddPoint(point, forceRefresh)
''            If ShowOuterCurvedLines Then
''                abovePlot.AddPoint(AddToY(point, OffsetValueMultiplier * Chart.Settings("RangeValue").Value), forceRefresh)
''                belowPlot.AddPoint(AddToY(point, -OffsetValueMultiplier * Chart.Settings("RangeValue").Value), forceRefresh)
''            End If
''        End Sub
''        Private Sub SliderMove(sender As Object, e As EventArgs)
''            'plot.Points.Clear()
''            'abovePlot.Points.Clear()
''            'belowPlot.Points.Clear()
''            'For i = GetLength() To Chart.bars.Count - 2
''            '    AddPlotPoint(New Point(i + 1, CalculateNewStartPointRegressionMiddle(i - GetLength(), i - GetLength(), i, i).EndPoint.Y), False)
''            'Next
''            'Dim coor As LineCoordinates = CalculateNewStartPointRegressionMiddle(CurrentBar.Number - GetLength(), CurrentBar.Number - GetLength(), CurrentBar.Number, CurrentBar.Number)
''            'AddPlotPoint(New Point(CurrentBar.Number, coor.EndPoint.Y), False)
''            'CType(CType(slider.Content, Grid).Children(1), TextBlock).Text = GetLength()
''            'plot.RefreshVisual()
''            'If ShowOffsetLines Then
''            '    abovePlot.RefreshVisual()
''            '    belowPlot.RefreshVisual()
''            'End If
''            'Length = GetLength()
''            'UpdateLine(CurrentBar.Number - GetLength(), coor)
''        End Sub
''        Protected Overrides Sub Main()
''        End Sub
''        Private Function GetLength() As Integer
''            Return Round(CType(CType(slider.Content, Grid).Children(0), Slider).Value)
''        End Function
''        Protected Overrides Sub NewBar()
''            If slider IsNot Nothing And CurrentBar.Number > GetLength() Then
''                Dim middleLine As LineCoordinates = CalculateNewStartPointRegressionMiddle(CurrentBar.Number - 1 - GetLength(), CurrentBar.Number - GetLength() - 1, CurrentBar.Number - 1, CurrentBar.Number)
''                If regressionLinesData.ContainsKey(CurrentBar.Number - 1) Then
''                    Dim a As New Object
''                Else
''                    regressionLinesData.Add(CurrentBar.Number - 1, middleLine)
''                End If
''                UpdateLine(CurrentBar.Number - 1, True)
''            End If
''        End Sub
''        Private Sub UpdateLine(endX As Integer, shouldRefresh As Boolean)
''            Dim maxAboveDistance As Double = 0, maxBelowDistance As Double = 0
''            currentLine(2).Coordinates = regressionLinesData(endX)
''            If plot.Points.Count = 0 OrElse plot.Points(plot.Points.Count - 1) <> regressionLinesData(endX).EndPoint Then plot.AddPoint(regressionLinesData(endX).EndPoint, loadingHistory = False And shouldRefresh)
''            If regressionLinesData.Count > GetLength() Then
''                For i = endX - GetLength() To endX
''                    If ((Chart.bars(i).Data.High - Chart.bars(i).Data.Low) / 2 + Chart.bars(i).Data.Low) - regressionLinesData(i).EndPoint.Y <= 0 Then 'below
''                        If regressionLinesData(i).EndPoint.Y - Chart.bars(i).Data.Low >= maxBelowDistance Then
''                            maxBelowDistance = regressionLinesData(i).EndPoint.Y - Chart.bars(i).Data.Low
''                        End If
''                    Else
''                        If Chart.bars(i).Data.High - regressionLinesData(i).EndPoint.Y >= maxAboveDistance Then
''                            maxAboveDistance = Chart.bars(i).Data.High - regressionLinesData(i).EndPoint.Y
''                        End If
''                    End If
''                Next
''                Dim minDistance As Decimal = If(maxAboveDistance <= maxBelowDistance, maxAboveDistance, maxBelowDistance)
''                currentLine(0).StartPoint = New Point(endX + 1 - GetLength(), regressionLinesData(endX).StartPoint.Y - minDistance)
''                currentLine(0).EndPoint = New Point(endX + 1, regressionLinesData(endX).EndPoint.Y - minDistance)
''                currentLine(1).StartPoint = New Point(endX + 1 - GetLength(), regressionLinesData(endX).StartPoint.Y + minDistance)
''                currentLine(1).EndPoint = New Point(endX + 1, regressionLinesData(endX).EndPoint.Y + minDistance)
''                If ShowOuterCurvedLines And (abovePlot.Points.Count = 0 OrElse abovePlot.Points(abovePlot.Points.Count - 1) <> AddToXY(regressionLinesData(endX).EndPoint, 1, Abs(minDistance))) Then abovePlot.AddPoint(AddToXY(regressionLinesData(endX).EndPoint, 1, Abs(minDistance)), loadingHistory = False And shouldRefresh)
''                If ShowOuterCurvedLines And (belowPlot.Points.Count = 0 OrElse belowPlot.Points(belowPlot.Points.Count - 1) <> AddToXY(regressionLinesData(endX).EndPoint, 1, -Abs(minDistance))) Then belowPlot.AddPoint(AddToXY(regressionLinesData(endX).EndPoint, 1, -Abs(minDistance)), loadingHistory = False And shouldRefresh)
''                If ShowTrailingLine And (trailingPlot.Points.Count = 0 OrElse trailingPlot.Points(trailingPlot.Points.Count - 1) <> AddToX(regressionLinesData(endX).StartPoint, 1)) Then trailingPlot.AddPoint(AddToX(regressionLinesData(endX).StartPoint, 1), loadingHistory = False And shouldRefresh)
''            End If

''        End Sub
''        Private Function CalculateNewStartPointRegressionMiddle(startPointX As Decimal, lineStartPoint As Decimal, lineEndPoint As Decimal, Optional endPointX As Decimal = -1) As LineCoordinates
''            If endPointX = -1 Then endPointX = CurrentBar.Number
''            Dim n As Decimal = (endPointX) - startPointX
''            Dim a As Decimal, b As Decimal
''            Dim sumx2 As Decimal = 0, sumx As Decimal = 0, sumy As Decimal = 0, sumxy As Decimal = 0
''            For bari = startPointX To endPointX - 1
''                Dim point As New Point(bari, Chart.bars(bari).Data.Low + (Chart.bars(bari).Data.High - Chart.bars(bari).Data.Low) / 2)
''                sumx += point.X
''                sumy += point.Y
''                sumxy += point.X * point.Y
''                sumx2 += point.X ^ 2
''            Next
''            b = (n * sumxy - sumx * sumy) / (n * sumx2 - sumx * sumx)
''            a = (sumy - b * sumx) / n
''            Return New LineCoordinates(New Point(lineStartPoint, a + b * lineStartPoint), New Point(lineEndPoint, a + b * lineEndPoint))
''        End Function

''        Public Overrides Property Name As String = "RegressionCurve"
''    End Class

''End Namespace

