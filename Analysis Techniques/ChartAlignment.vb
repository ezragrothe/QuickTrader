Namespace AnalysisTechniques
    Public Class ChartAlignment
#Region "AnalysisTechnique Inherited Code"
        Inherits AnalysisTechnique

        ' Inherit the one-argument constructor from the base class.
        Public Sub New(ByVal chart As Chart)
            MyBase.New(chart) ' Call the base class constructor.
            If chart IsNot Nothing Then AddHandler chart.ChartKeyDown, AddressOf KeyPress
            If chart IsNot Nothing Then AddHandler chart.MouseLeftButtonDown, AddressOf MouseDown
            If chart IsNot Nothing Then AddHandler chart.MouseMove, AddressOf MouseMove
        End Sub
#End Region
        Sub MouseMove(sender As Object, e As MouseEventArgs)
            If e.LeftButton = MouseButtonState.Pressed And Keyboard.IsKeyDown(SetLocationHotKey) Then
                HandleMouseEvent(e.GetPosition(Chart))
            End If
        End Sub
        Private Sub HandleMouseEvent(location As Point)
            PrimaryTechnique = Nothing
            Dim index As Integer = Round(Chart.GetRelativeFromReal(location).X)
            DrawHighlighting(index)
            If index > 0 And index < Chart.bars.Count - 1 Then
                Chart.Bounds = New Bounds(Round(Chart.Bounds.X1), Chart.Bounds.Y1, Round(Chart.Bounds.X2), Chart.Bounds.Y2)
                HandleSecondaryCharts(index)
            End If
        End Sub
        Sub MouseDown(sender As Object, e As MouseButtonEventArgs)
            If Keyboard.IsKeyDown(SetLocationHotKey) Then
                HandleMouseEvent(e.GetPosition(Chart))
            End If
        End Sub

        Protected Sub KeyPress(ByVal sender As Object, ByVal e As KeyEventArgs)
            If Chart IsNot Nothing AndAlso IsEnabled AndAlso Keyboard.Modifiers = ModifierKeys.None Then
                Dim key As Key
                If e.SystemKey = key.None Then
                    key = e.Key
                Else
                    key = e.SystemKey
                End If
            End If
        End Sub
        <Input("The key to set the location.")> Public Property SetLocationHotKey As Key = Key.LeftShift
        <Input("The width for the marker.")> Public Property MarkerWidth As Decimal = 4
        <Input("The margin of bars to look for a range point.")> Public Property MaxSearchDistanceForRangePoint As Integer = 5
        <Input("")> Public Property ChartPixelSeparation As Decimal
        Private highlightLine As TrendLine
        Public lastHighlightPosition As Integer
        Public Overrides Property Name As String = "ChartAlignment"
        Public Property PrimaryTechnique As ChartAlignment
        Public Sub DrawHighlighting(bar As Integer)
            If Chart.bars.Count > bar And bar > 0 Then
                highlightLine.StartPoint = New Point(bar, Chart.bars(bar - 1).Data.High)
                highlightLine.EndPoint = New Point(bar, Chart.bars(bar - 1).Data.Low)
                lastHighlightPosition = bar - 1
            End If
        End Sub
        Protected Overrides Sub Begin()
            MyBase.Begin()

            'highlightLine = NewTrendLine(Chart.Settings("Bar Color").Value, New Point(0, 0), New Point(0, 0), True)
            'highlightLine.Pen.Thickness = MarkerWidth
            Chart.Settings("AutoScale").Value = False
        End Sub
        Dim lastBounds As Bounds
        Dim currentChartSeparation As Decimal
        Public Sub Align(chartSeparation As Decimal)
            If PrimaryTechnique IsNot Nothing And Chart.bars.Count > 1 Then
                currentChartSeparation = chartSeparation
                Dim primaryChart As Chart = PrimaryTechnique.Chart
				'If Chart.Location <> primaryChart.Location Then Chart.Location = primaryChart.Location
				'If Chart.Size <> primaryChart.Size Then Chart.Size = primaryChart.Size

                'Dim primaryratioofbarstowidth = primaryChart.bars.Count / primaryChart.Bounds.Width
                'Dim primaryWidth = primaryChart.Bounds.Width
                'Dim percentage = PrimaryTechnique.lastHighlightPosition / primaryChart.bars.Count

                'Dim a = primaryChart.GetRealFromRelativeX(primaryChart.Bounds.X2) - primaryChart.GetRealFromRelativeX(primaryChart.bars.Count)
                'Dim b = Chart.GetRelativeFromRealWidth(a)

                'Chart.SetScaling(Chart.Bounds.Size)

                'Dim width = Chart.GetRelativeFromRealWidth(dif.X)
                'Dim height = Chart.GetRelativeFromRealHeight(dif.Y)
                'dif = New Vector(width, height)
                'Chart.Bounds = New Bounds(Chart.Bounds.TopLeft - dif, Chart.Bounds.BottomRight - dif)
                'Dim primaryRealPositionForLastBar = primaryChart.GetRealFromRelativeX(primaryChart.bars.Count)
                'Dim realPositionForLastBar = Chart.GetRealFromRelativeX(Chart.bars.Count)
                'Dim c = (realPositionForLastBar - chartRealHighlightPosition.X) / (primaryRealPositionForLastBar - primaryChartRealHighlightPosition.X)
                'Chart.Bounds = New Bounds(Chart.Bounds.X2 - Chart.Bounds.Width * c, Chart.Bounds.Y1, Chart.Bounds.X2, Chart.Bounds.Y2)
                'Dim yDif = Chart.GetRelativeFromRealHeight(primaryChart.GetRealFromRelativeY(-primaryChart.bars(PrimaryTechnique.lastHighlightPosition).Data.High) - Chart.GetRealFromRelativeY(-Chart.bars(lastHighlightPosition).Data.High))
                'Dim ratio = (-primaryChart.bars(PrimaryTechnique.lastHighlightPosition).Data.High - primaryChart.Bounds.Y2) / (primaryChart.Bounds.Y1 - primaryChart.Bounds.Y2)
                'Dim topMargin = (1 - ratio) * (Chart.Bounds.Y1 - Chart.Bounds.Y2)
                'Dim bottomMargin = ratio * (Chart.Bounds.Y1 - Chart.Bounds.Y2)
                'Chart.GetRelativeFromRealWidth(primaryChart.Settings("XMargin").Value * primaryChart.ActualWidth + primaryChart.Settings("PriceBarWidth").Value)

                Dim distance = Chart.GetRealFromRelativeWidth(Chart.bars.Count - lastHighlightPosition - 1)
                Dim primaryDistance = primaryChart.GetRealFromRelativeWidth(primaryChart.bars.Count - PrimaryTechnique.lastHighlightPosition - 1)
                Dim div = distance / primaryDistance
                Dim newWidth = Chart.Bounds.Width * div
                Chart.SetScaling(New Size(newWidth, Chart.Bounds.Height))
                Dim primaryChartRealHighlightPosition = primaryChart.GetRealFromRelativeY(-primaryChart.bars(PrimaryTechnique.lastHighlightPosition).Data.High)
                Dim chartRealHighlightPosition = Chart.GetRealFromRelativeY(-Chart.bars(lastHighlightPosition).Data.High)
                Dim dif = Chart.GetRelativeFromRealHeight(primaryChartRealHighlightPosition - chartRealHighlightPosition)
                Dim relOffset As Decimal = Chart.GetRelativeFromRealHeight(currentChartSeparation)
                Chart.Bounds = New Bounds(New Point(Round(Chart.Bounds.X1), Chart.Bounds.Y1 - dif + relOffset), New Point(Round(Chart.Bounds.X2), Chart.Bounds.Y2 - dif + relOffset))
            End If
        End Sub
        Private Sub HandleSecondaryCharts(barIndex As Integer)
            Dim time As Date = Chart.bars(barIndex).Data.Date
            Dim chartSeparation As Decimal = ChartPixelSeparation
            For Each c In Chart.Parent.Charts
                If c IsNot Chart And c.bars.Count > 0 Then
                    For Each item In c.AnalysisTechniques
                        If TypeOf item.AnalysisTechnique Is ChartAlignment And item.AnalysisTechnique.IsEnabled AndAlso c.bars IsNot Nothing AndAlso c.bars.Count > 0 Then
                            CType(item.AnalysisTechnique, ChartAlignment).PrimaryTechnique = Me
                            If c.bars(0).Data.Date < time Then
                                For i = 0 To c.bars.Count - 1
                                    If c.bars(i).Data.Date >= time Then
                                        Dim highPoint As Point = New Point(0, 0)
                                        Dim lowPoint As Point = New Point(0, Decimal.MaxValue)
                                        For searchPoint = Max(i - MaxSearchDistanceForRangePoint, 0) To Min(i + MaxSearchDistanceForRangePoint, c.bars.Count - 2)
                                            If c.bars(searchPoint).Data.High >= highPoint.Y Then
                                                highPoint = New Point(searchPoint, c.bars(searchPoint).Data.High)
                                            End If
                                            If c.bars(searchPoint).Data.Low <= lowPoint.Y Then
                                                lowPoint = New Point(searchPoint, c.bars(searchPoint).Data.Low)
                                            End If
                                        Next
                                        If Abs(i - (lowPoint.X + 1)) < Abs(i - (highPoint.X + 1)) Then
                                            CType(item.AnalysisTechnique, ChartAlignment).DrawHighlighting(lowPoint.X + 1)
                                            CType(item.AnalysisTechnique, ChartAlignment).Align(chartSeparation)
                                        Else
                                            CType(item.AnalysisTechnique, ChartAlignment).DrawHighlighting(highPoint.X + 1)
                                            CType(item.AnalysisTechnique, ChartAlignment).Align(chartSeparation)
                                        End If
                                        'CType(item.AnalysisTechnique, ChartAlignment).DrawHighlighting(i)
                                        'CType(item.AnalysisTechnique, ChartAlignment).Align()
                                        Exit For
                                    End If
                                Next
                            End If
                            Exit For
                        End If
                    Next
                    chartSeparation += ChartPixelSeparation
                End If
            Next
        End Sub
        Protected Overrides Sub NewBar()
            MyBase.NewBar()
            If PrimaryTechnique IsNot Nothing And Not loadingHistory Then
                Align(currentChartSeparation)
            End If
        End Sub
        Protected Overrides Sub Main()

        End Sub
    End Class
End Namespace
