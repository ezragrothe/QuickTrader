Imports System.Collections.Specialized

Namespace AnalysisTechniques
    Public Class Automation
        Inherits AnalysisTechnique
        Public Sub New(ByVal chart As Chart)
            MyBase.New(chart)
            Description = "An automation technique in progress."
            If chart IsNot Nothing Then AddHandler chart.ChartKeyDown, AddressOf KeyPress
        End Sub
        Dim processButtonUI As UIChartControl
        Dim originalLocation As New Point(400, 40)
        Dim progressBar As ProgressBar
        Dim contentButton As Button
        Dim contentButton2 As Button
        Dim originalLocation2 As New Point(400, 90)
        Dim slider As UIChartControl
        Public Property plot As Plot
        Public Property avgdplot As Plot
        <Input> Public Property MoveUp As Key = Key.T
        <Input> Public Property MoveDown As Key = Key.G
        <Input> Public Property Color As Color = Colors.Red
        <Input> Public Property ShowCalculateButton As Boolean = False
        <Input> Public Property CalculatedBars As Integer = 0
        Private Function GetCalculatedBars() As Integer
            If Chart IsNot Nothing Then
                If CalculatedBars >= Chart.bars.Count Or CalculatedBars = 0 Then
                    Return Chart.bars.Count - 1
                Else
                    Return CalculatedBars
                End If
            Else
                Return 0
            End If
        End Function
        Protected Sub KeyPress(ByVal sender As Object, ByVal e As KeyEventArgs)
            If Chart IsNot Nothing AndAlso IsEnabled AndAlso Keyboard.Modifiers = ModifierKeys.None Then
                Dim key As Key
                If e.SystemKey = Key.None Then
                    key = e.Key
                Else
                    key = e.SystemKey
                End If
                If key = MoveUp Then
                    Dim copy(avgdplot.Points.Count - 1) As Point
                    avgdplot.Points.CopyTo(copy)
                    For i = 0 To avgdplot.Points.Count - 1
                        copy(i) = New Point(copy(i).X, copy(i).Y + Chart.Settings("RangeValue").Value / 4)
                    Next
                    avgdplot.Points = copy.ToList
                ElseIf key = MoveDown Then
                    Dim copy(avgdplot.Points.Count - 1) As Point
                    avgdplot.Points.CopyTo(copy)
                    For i = 0 To avgdplot.Points.Count - 1
                        copy(i) = New Point(copy(i).X, copy(i).Y - Chart.Settings("RangeValue").Value / 4)
                    Next
                    avgdplot.Points = copy.ToList
                End If
            End If
        End Sub
        Public Sub AlignToBar(bar As Integer, Optional price As Double = 0)
            Dim plotPoint As Integer = -1
            For i = 1 To avgdplot.Points.Count - 1
                If avgdplot.Points(i).X >= bar And avgdplot.Points(i - 1).X < bar Then
                    plotPoint = i
                    Exit For
                End If
            Next
            If plotPoint <> -1 Then
                Dim dif = avgdplot.Points(plotPoint).Y - If(price = 0, Chart.bars(bar).Data.Avg, price)
                Dim copy(avgdplot.Points.Count - 1) As Point
                avgdplot.Points.CopyTo(copy)
                For i = 0 To avgdplot.Points.Count - 1
                    copy(i) = New Point(copy(i).X, copy(i).Y - dif)
                Next
                avgdplot.Points = copy.ToList
            End If
        End Sub
        Protected Overrides Sub Begin()
            MyBase.Begin()
            'UI
            'slider = New UIChartControl
            'slider.AnalysisTechnique = Me
            'Dim contentSlider As New Slider
            ''contentSlider.Minimum = Length
            ''contentSlider.Maximum = Length * 4
            'contentSlider.VerticalAlignment = VerticalAlignment.Center
            'contentSlider.HorizontalAlignment = HorizontalAlignment.Stretch
            ''contentSlider.Value = Length
            'contentSlider.Background = New SolidColorBrush(Chart.Settings("Background").Value)
            'contentSlider.Margin = New Thickness(0, 0, 0, 0)
            'contentSlider.LargeChange = 1
            'contentSlider.SmallChange = 1
            'AddHandler contentSlider.ValueChanged, AddressOf sliderMove
            'Dim grdBackground As New Grid
            'grdBackground.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})
            'grdBackground.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(30, GridUnitType.Pixel)})
            'grdBackground.Background = New SolidColorBrush(Color.FromArgb(150, 200, 200, 200))
            'grdBackground.Children.Add(contentSlider)
            'Dim text As New TextBlock With {.HorizontalAlignment = HorizontalAlignment.Center, .VerticalAlignment = VerticalAlignment.Center, .Text = contentSlider.Value}
            'Grid.SetColumn(text, 1)
            'grdBackground.Children.Add(text)
            'slider.Content = grdBackground
            'slider.Width = 200
            'slider.Height = 24
            'slider.Left = originalLocation2.X
            'slider.Top = originalLocation2.Y
            'slider.IsDraggable = True
            'AddObjectToChart(slider)


            processButtonUI = New UIChartControl
            processButtonUI.AnalysisTechnique = Me
            Dim backgroundGrid As New Grid
            backgroundGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})
            backgroundGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(30, GridUnitType.Pixel)})
            backgroundGrid.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Star)})
            backgroundGrid.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Star)})
            contentButton = New Button
            contentButton2 = New Button
            AddHandler contentButton.Click, AddressOf ProcessButtonClick
            AddHandler contentButton2.Click, AddressOf ProcessButtonClick2
            Dim gridBackground As New Grid
            progressBar = New ProgressBar
            progressBar.Maximum = 1
            progressBar.Minimum = 0
            progressBar.Value = 0
            progressBar.Margin = New Thickness(-45.5, -6.5, -44, -5)
            progressBar.Opacity = 0.8
            gridBackground.Children.Add(progressBar)
            gridBackground.Children.Add(New TextBlock With {.HorizontalAlignment = HorizontalAlignment.Center, .VerticalAlignment = VerticalAlignment.Center, .Text = "Calculate"})
            contentButton.Content = gridBackground
            Grid.SetColumn(contentButton, 0)
            backgroundGrid.Children.Add(contentButton)
            Grid.SetColumn(contentButton2, 0)
            Grid.SetRow(contentButton2, 1)
            backgroundGrid.Children.Add(contentButton2)
            contentButton2.Content = "Process additional"
            Dim draggingBlock As New TextBlock
            Grid.SetColumn(draggingBlock, 1)
            draggingBlock.Background = Brushes.LightGray
            backgroundGrid.Children.Add(draggingBlock)
            processButtonUI.Content = backgroundGrid
            processButtonUI.Width = 170
            processButtonUI.Height = 56
            processButtonUI.Left = originalLocation.X
            processButtonUI.Top = originalLocation.Y
            processButtonUI.IsDraggable = True
            If ShowCalculateButton Then AddObjectToChart(processButtonUI)

        End Sub
        Private Sub sliderMove(sender As Object, e As EventArgs)
            ProcessButtonClick(Nothing, Nothing)
        End Sub
        Public Overrides Sub Reset()
            If processButtonUI IsNot Nothing Then
                originalLocation = New Point(processButtonUI.Left, processButtonUI.Top)
                RemoveHandler contentButton.Click, AddressOf ProcessButtonClick
                processButtonUI = Nothing
            End If
            If slider IsNot Nothing Then
                originalLocation2 = New Point(slider.Left, slider.Top)
                RemoveHandler CType(CType(slider.Content, Grid).Children(0), Slider).ValueChanged, AddressOf sliderMove
                slider = Nothing
            End If
            MyBase.Reset()
        End Sub
        Class AvgStruct
            Public Sum As Double
            Public Count As Integer
            Public Bar As Integer
            Public ReadOnly Property Avg As Double
                Get
                    Return Sum / Count
                End Get
            End Property

        End Class
        Public Sub DrawMultiplePlots()
            Dim i As Integer = 0
            While i < Chart.Children.Count
                If TypeOf Chart.Children(i).AnalysisTechnique Is Automation AndAlso (TypeOf Chart.Children(i) Is TrendLine Or TypeOf Chart.Children(i) Is Arrow Or TypeOf Chart.Children(i) Is Plot) Then
                    If Chart.Children(i).Tag Is Nothing Then
                        Chart.Children.RemoveAt(i)
                    Else
                        i += 1
                    End If
                Else
                    i += 1
                End If
            End While
            If plot IsNot Nothing Then
                For i = 0 To Chart.bars.Count - 1
                    Dim newPlot As Plot = Me.NewPlot(Color.FromArgb(5, 255, 0, 0))
                    Dim plotPoint As Integer = -1
                    For j = 1 To plot.Points.Count - 1
                        If plot.Points(j).X >= i And plot.Points(j - 1).X < i Then
                            plotPoint = j
                            Exit For
                        End If
                    Next
                    If plotPoint <> -1 Then
                        Dim dif = plot.Points(plotPoint).Y - Chart.bars(i).Data.Avg
                        For j = plotPoint To plot.Points.Count - 1
                            If plot.Points(j).X > 0 Then newPlot.Points.Add(New Point(plot.Points(j).X, plot.Points(j).Y - dif))
                        Next
                        newPlot.RefreshVisual()
                    End If
                Next
            End If
        End Sub
        Private Sub ProcessButtonClick2(sender As Object, e As EventArgs)
            DrawMultiplePlots()
        End Sub
        Private Sub ProcessButtonClick(sender As Object, e As EventArgs)
            For Each c In Chart.Parent.Charts
                Dim i As Integer = 0
                While i < c.Children.Count
                    If TypeOf c.Children(i).AnalysisTechnique Is Automation AndAlso (TypeOf c.Children(i) Is TrendLine Or TypeOf c.Children(i) Is Arrow Or TypeOf c.Children(i) Is Plot) Then
                        c.Children.RemoveAt(i)
                    Else
                        i += 1
                    End If
                End While
            Next

            Dim chartTimes As New Dictionary(Of Chart, OrderedDictionary)
            Dim maxStartingTime As Date = Date.MinValue
            Dim avgPrices As New OrderedDictionary
            For Each c In Chart.Parent.Charts
                If c.bars.Count > 1 Then
                    chartTimes.Add(c, New OrderedDictionary())
                    Dim curTech As Automation = Nothing
                    For Each tech In c.AnalysisTechniques
                        If TypeOf tech.AnalysisTechnique Is Automation Then
                            curTech = tech.AnalysisTechnique
                        End If
                    Next
                    If curTech IsNot Nothing Then
                        curTech.CalculatedBars = CalculatedBars
                        If c.bars(c.bars.Count - curTech.GetCalculatedBars).Data.Date > maxStartingTime Then
                            maxStartingTime = c.bars(c.bars.Count - curTech.GetCalculatedBars).Data.Date
                        End If
                    End If
                End If
            Next
            Dim currentTime As Date = maxStartingTime
            While True
                Dim minChange As TimeSpan = TimeSpan.MaxValue
                Dim [exit] As Boolean = False
                For Each c In Chart.Parent.Charts
                    Dim b = GetBarFromDate(c, currentTime)
                    If c.bars.Count > b + 1 And b <> -1 Then
                        Dim val As TimeSpan = (GetDateFromBar(c, GetBarFromDate(c, currentTime) + 1) - currentTime)
                        If val < minChange Then
                            minChange = val
                            '[exit] = False
                        End If
                    Else
                        [exit] = True
                    End If
                Next
                If [exit] Then Exit While
                currentTime += minChange
                avgPrices.Add(currentTime, 0)
                For Each item In chartTimes
                    If item.Key.TickerID = 2 Then
                        Dim a As New Object
                    End If
                    Dim val = (CalcPriceFromTime(item.Key, currentTime) - CalcPriceFromTime(item.Key, currentTime - minChange)) / item.Key.Settings("RangeValue").Value
                    chartTimes(item.Key).Add(currentTime, val)
                Next
            End While
            For i = 0 To avgPrices.Count - 1
                Dim sum As Double = 0
                For Each chartItem In chartTimes
                    sum += chartItem.Value.Item(i)
                Next
                avgPrices(i) = sum / chartTimes.Count
            Next
            Dim plots As New List(Of Plot)
            For Each item In chartTimes
                Dim curTech As Automation = Nothing
                For Each tech In item.Key.AnalysisTechniques
                    If TypeOf tech.AnalysisTechnique Is Automation Then
                        curTech = tech.AnalysisTechnique
                    End If
                Next
                Dim plot As New Plot

                'plot.AutoRefresh = False
                'plot.AnalysisTechnique = Me
                'plot.UseNegativeCoordinates = False
                'plot.DrawVisual = True
                'plot.AutoRefresh = True
                'plot.Pen = New Pen(New SolidColorBrush(Colors.Red), 1)

                If curTech IsNot Nothing Then plot.AddPoint(New Point(item.Key.bars.Count - curTech.GetCalculatedBars, item.Key.bars(item.Key.bars.Count - curTech.GetCalculatedBars).Data.Avg))

                'item.Key.Children.Add(plot)

                plots.Add(plot)
                For Each tech In item.Key.AnalysisTechniques
                    If TypeOf tech.AnalysisTechnique Is Automation Then
                        CType(tech.AnalysisTechnique, Automation).plot = plot
                    End If
                Next
            Next
            For Each p As DictionaryEntry In avgPrices
                Dim plotIndex As Integer = 0
                For Each c In chartTimes
                    Dim bar = GetBarFromDate(c.Key, p.Key)
                    If plots(plotIndex).Points.Count > 0 Then
                        plots(plotIndex).AddPoint(New Point(bar, plots(plotIndex).Points(plots(plotIndex).Points.Count - 1).Y + p.Value * c.Key.Settings("RangeValue").Value))
                    End If
                    plotIndex += 1
                Next
            Next
            For Each item In chartTimes
                Dim avgs As New Dictionary(Of Integer, AvgStruct)
                Dim curTech As Automation = Nothing
                For Each tech In item.Key.AnalysisTechniques
                    If TypeOf tech.AnalysisTechnique Is Automation Then
                        curTech = tech.AnalysisTechnique
                    End If
                Next
                If curTech IsNot Nothing Then
                    For i = item.Key.bars.Count - curTech.GetCalculatedBars To item.Key.bars.Count - 1
                        'Dim newPlot As Plot = Me.NewPlot(Color.FromArgb(5, 255, 0, 0))
                        Dim plotPoint As Integer = -1
                        For j = 1 To curTech.plot.Points.Count - 1
                            If curTech.plot.Points(j).X >= i And curTech.plot.Points(j - 1).X < i Then
                                plotPoint = j
                                Exit For
                            End If
                        Next
                        If plotPoint <> -1 Then
                            Dim dif = curTech.plot.Points(plotPoint).Y - item.Key.bars(i).Data.Avg
                            For j = plotPoint To curTech.plot.Points.Count - 1 'plotPoint instead of 0
                                If Not avgs.ContainsKey(j) Then
                                    avgs.Add(j, New AvgStruct() With {.Count = 1, .Sum = curTech.plot.Points(j).Y - dif, .Bar = curTech.plot.Points(j).X})
                                Else
                                    avgs(j).Count += 1
                                    avgs(j).Sum += curTech.plot.Points(j).Y - dif
                                End If

                            Next
                            'NewPlot.RefreshVisual()
                        End If
                    Next
                    Dim newPlot As Plot = curTech.NewPlot(Color)
                    newPlot.Tag = True
                    For Each a In avgs
                        If a.Value.Bar > 1 Then
                            newPlot.Points.Add(New Point(a.Value.Bar, a.Value.Avg))
                        Else
                            Dim b As New Object
                        End If
                    Next
                    newPlot.RefreshVisual()
                    curTech.avgdplot = newPlot
                End If
            Next
            ''11:26:30 tickerid 3


            'contentButton.Content = "time: " & CStr((Now - t).TotalMilliseconds)

            'progressBar.Value = 1

            'AddObjectToChart(processButtonUI)
        End Sub
        Protected Overrides Sub Main()

        End Sub
        Private Function CalcPriceFromTime(c As Chart, time As Date) As Double
            For i = 1 To c.bars.Count - 1
                If c.bars(i - 1).Data.Date < time And c.bars(i).Data.Date >= time Then
                    Dim ratio As Double = (time - c.bars(i - 1).Data.Date).TotalMilliseconds / (c.bars(i).Data.Date - c.bars(i - 1).Data.Date).TotalMilliseconds
                    Return (c.bars(i).Data.Avg - c.bars(i - 1).Data.Avg) * ratio + c.bars(i - 1).Data.Avg
                End If
            Next
            Return -100
        End Function
        Private Function GetBarFromDate(c As Chart, time As Date) As Integer
            For i = 1 To c.bars.Count - 1
                If c.bars(i - 1).Data.Date <= time And c.bars(i).Data.Date > time Then
                    Return i
                End If
            Next
            Return -1
        End Function
        Private Function GetDateFromBar(c As Chart, barNum As Integer) As Date
            Return c.bars(barNum).Data.Date
        End Function
        Private Function CalculateRegressionPoint(startPointX As Decimal, endPointX As Decimal, lineEndPointX As Decimal) As Point
            If endPointX = -1 Then endPointX = CurrentBar.Number - 1
            Dim n As Decimal = (endPointX + 1) - startPointX
            Dim a As Decimal, b As Decimal
            Dim sumx2 As Decimal = 0, sumx As Decimal = 0, sumy As Decimal = 0, sumxy As Decimal = 0
            For bari = startPointX To endPointX
                Dim point As New Point(bari, Chart.bars(bari).Data.Low + (Chart.bars(bari).Data.High - Chart.bars(bari).Data.Low) / 2)
                sumx += point.X
                sumy += point.Y
                sumxy += point.X * point.Y
                sumx2 += point.X ^ 2
            Next
            b = (n * sumxy - sumx * sumy) / (n * sumx2 - sumx * sumx)
            a = (sumy - b * sumx) / n
            Return New Point(lineEndPointX, a + b * lineEndPointX)
        End Function
        ''' <summary>
        ''' calculates the current regression fresh if no params are given.
        ''' </summary>
        Private Function CalculateNextRegressionFromParams(startPointX As Decimal, endPointX As Decimal, lineEndPoint As Decimal, Optional params As RegressionParams? = Nothing) As RegressionParams
            Dim n As Decimal = (endPointX + 1) - startPointX, a As Decimal, b As Decimal, sumx2 As Decimal = 0, sumx As Decimal = 0, sumy As Decimal = 0, sumxy As Decimal = 0
            If params Is Nothing OrElse params.Value.Params Is Nothing Then
                params = New RegressionParams With {.Params = CalculateRegressionParams(startPointX, endPointX)}
                sumx = params.Value.Params(0) : sumy = params.Value.Params(1) : sumxy = params.Value.Params(2) : sumx2 = params.Value.Params(3)
            Else
                Dim pointToRemove As New Point(startPointX - 1, Chart.bars(startPointX - 1).Data.Avg)
                Dim pointToAdd As New Point(endPointX, Chart.bars(endPointX).Data.Avg)
                sumx = params.Value.Params(0) - pointToRemove.X + pointToAdd.X
                sumy = params.Value.Params(1) - pointToRemove.Y + pointToAdd.Y
                sumxy = params.Value.Params(2) - pointToRemove.X * pointToRemove.Y + pointToAdd.X * pointToAdd.Y
                sumx2 = params.Value.Params(3) - pointToRemove.X ^ 2 + pointToAdd.X ^ 2
            End If
            b = (n * sumxy - sumx * sumy) / (n * sumx2 - sumx * sumx)
            a = (sumy - b * sumx) / n
            Return New RegressionParams() With {.EndPoint = New Point(lineEndPoint, a + b * lineEndPoint), .Params = {sumx, sumy, sumxy, sumx2}}
        End Function
        Private Function CalculateRegressionParams(startPointX As Decimal, endPointX As Decimal) As Decimal()
            If endPointX = -1 Then endPointX = CurrentBar.Number - 1
            Dim n As Decimal = (endPointX + 1) - startPointX
            Dim sumx2 As Decimal = 0, sumx As Decimal = 0, sumy As Decimal = 0, sumxy As Decimal = 0
            For bari = startPointX To endPointX
                Dim point As New Point(bari, Chart.bars(bari).Data.Low + (Chart.bars(bari).Data.High - Chart.bars(bari).Data.Low) / 2)
                sumx += point.X
                sumy += point.Y
                sumxy += point.X * point.Y
                sumx2 += point.X ^ 2
            Next
            Return {sumx, sumy, sumxy, sumx2}
        End Function
        Private Structure RegressionParams
            Public Property EndPoint As Point
            Public Property Params As Decimal()
        End Structure

        Public Overrides Property Name As String = "Automation"
    End Class
End Namespace