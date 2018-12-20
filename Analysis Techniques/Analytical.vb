Namespace AnalysisTechniques
    Public Class Analytical
        Inherits AnalysisTechnique
        Public Sub New(ByVal chart As Chart)
            MyBase.New(chart)
            Description = "Test analysis technique."
        End Sub
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
        End Class
        Dim plots As List(Of Plot)
        Dim processButtonUI As UIChartControl
        Dim originalLocation As New Point(400, 40)
        Dim progressBar As ProgressBar
        Dim contentButton As Button
        Dim originalLocation2 As New Point(400, 90)
        Dim swings As List(Of Swing)
        Dim swingDir As Boolean
        Dim slider As UIChartControl
        <Input("")>
        Public Property RV As Double = 2
        <Input> Property Length As Integer = 100
        Protected Overrides Sub Begin()
            MyBase.Begin()
            Slider = New UIChartControl
            Slider.AnalysisTechnique = Me
            Dim contentSlider As New Slider
            contentSlider.Minimum = 0
            contentSlider.Maximum = Length * 4
            contentSlider.VerticalAlignment = VerticalAlignment.Center
            contentSlider.HorizontalAlignment = HorizontalAlignment.Stretch
            contentSlider.Value = Length
            contentSlider.Background = New SolidColorBrush(Chart.Settings("Background").Value)
            contentSlider.Margin = New Thickness(0, 0, 0, 0)
            contentSlider.LargeChange = 1
            contentSlider.SmallChange = 1
            AddHandler contentSlider.ValueChanged, AddressOf SliderMove
            Dim grdBackground As New Grid
            grdBackground.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})
            grdBackground.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(30, GridUnitType.Pixel)})
            grdBackground.Background = New SolidColorBrush(Color.FromArgb(150, 200, 200, 200))
            grdBackground.Children.Add(contentSlider)
            Dim text As New TextBlock With {.HorizontalAlignment = HorizontalAlignment.Center, .VerticalAlignment = VerticalAlignment.Center, .Text = contentSlider.Value}
            Grid.SetColumn(text, 1)
            grdBackground.Children.Add(text)
            slider.Content = grdBackground
            Slider.Width = 200
            Slider.Height = 24
            slider.Left = originalLocation2.X
            slider.Top = originalLocation2.Y
            Slider.IsDraggable = True
            AddObjectToChart(slider)


            processButtonUI = New UIChartControl
            processButtonUI.AnalysisTechnique = Me
            Dim backgroundGrid As New Grid
            backgroundGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})
            backgroundGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(30, GridUnitType.Pixel)})
            contentButton = New Button
            AddHandler contentButton.Click, AddressOf ProcessButtonClick
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
            Dim draggingBlock As New TextBlock
            Grid.SetColumn(draggingBlock, 1)
            draggingBlock.Background = Brushes.LightGray
            backgroundGrid.Children.Add(draggingBlock)
            processButtonUI.Content = backgroundGrid
            processButtonUI.Width = 170
            processButtonUI.Height = 28
            processButtonUI.Left = originalLocation.X
            processButtonUI.Top = originalLocation.Y
            processButtonUI.IsDraggable = True
            AddObjectToChart(processButtonUI)
            swingDir = False
            swings = New List(Of Swing)
            swings.Add(New Swing(NewTrendLine(Colors.Gray, New Point(CurrentBar.Number, CurrentBar.Close), New Point(CurrentBar.Number, CurrentBar.Close), True), False))
        End Sub
        Private Sub sliderMove(sender As Object, e As EventArgs)
            newRegressionLength = CType(CType(slider.Content, Grid).Children(0), Slider).Value
            CType(CType(slider.Content, Grid).Children(1), TextBlock).Text = newRegressionLength
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
        Dim newRegressionLength As Integer = 180
        Private Sub ProcessButtonClick(sender As Object, e As EventArgs)
            Dim i As Integer
            'Dim priceDifSum As Double
            'Dim priceDifCount As Integer
            'Dim sum As Double
            'Dim nonAbsSum As Double

            While i < Chart.Children.Count
                If Chart.Children(i).AnalysisTechnique Is Me AndAlso (TypeOf Chart.Children(i) Is TrendLine Or TypeOf Chart.Children(i) Is Arrow Or TypeOf Chart.Children(i) Is Plot) Then
                    Chart.Children.RemoveAt(i)
                Else
                    i += 1
                End If
            End While

            'Dim p = NewPlot(Colors.Red)
            'For i = Chart.bars.Count - 1 To Chart.bars.Count - 1 - newRegressionLength Step -1
            '    sum += Abs(Chart.bars(i).Data.Avg - Chart.bars(i - 1).Data.Avg)
            '    nonAbsSum += Chart.bars(i).Data.Avg - Chart.bars(i - 1).Data.Avg
            'Next
            'If sum <> 0 Then Log(Round(Abs(nonAbsSum) / sum * 100, 5))
            'For i = Chart.bars.Count - 1 - newRegressionLength To Chart.bars.Count - 1
            '    p.AddPoint(New Point(i, Chart.bars(Chart.bars.Count - 1).Data.Low - Chart.Settings("RangeValue").Value * Length * (15 / 100) + Abs(nonAbsSum) / sum * Chart.Settings("RangeValue").Value * 50), False)
            '    sum -= Abs(Chart.bars(i).Data.Avg - Chart.bars(i - 1).Data.Avg)
            '    nonAbsSum -= Chart.bars(i).Data.Avg - Chart.bars(i - 1).Data.Avg
            'Next
            'p.RefreshVisual()
            'NewTrendLine(New Pen(Brushes.Red, 1), New Point(0, Chart.bars(Chart.bars.Count - 1).Data.Low - Chart.Settings("RangeValue").Value * Length * (15 / 100)), New Point(10000, Chart.bars(Chart.bars.Count - 1).Data.Low - Chart.Settings("RangeValue").Value * Length * (15 / 100)), True)


            'NewTrendLine(New Pen(Brushes.Red, 1), New Point(Chart.bars.Count - 1 - newRegressionLength, 0), New Point(Chart.bars.Count - 1 - newRegressionLength, 100), True)
            'Exit Sub
            'Dim stepamount As Integer = CType(CType(slider.Content, Grid).Children(0), Slider).Value
            'For [step] = 0 To stepamount - 1
            '    For x = 0 To Chart.bars.Count - stepamount - 1 Step stepamount
            '        priceDifSum += Chart.bars(x + stepamount).Data.Avg - Chart.bars(x).Data.Avg
            '        priceDifCount += 1
            '    Next
            'Next
            'Log("movement: " & priceDifSum / priceDifCount & ". expected chart dif: " & priceDifCount * priceDifSum)
            'Exit Sub

            'For Each bar In Chart.bars
            '    bar.DrawVisual = True
            'Next
            Dim plot As Plot
            Dim plot2 As Plot
            Dim plot3 As Plot
            Dim plot4 As Plot
            Dim plot5 As Plot
            Dim plot6 As Plot = NewPlot(Colors.Red)
            Dim plot7 As Plot = NewPlot(Colors.Green)
            Dim plot8 As Plot = NewPlot(Colors.Blue)
            plot = NewPlot(Colors.Red)
            plot2 = NewPlot(Colors.Blue)
            plot4 = NewPlot(Colors.Red)
            plot5 = NewPlot(Colors.Red)
            Dim t = Now
            Dim graphLocation As Decimal = Chart.bars(Chart.bars.Count - 1).Data.Low - 1.5
            Dim graphLocation2 As Decimal = Chart.bars(Chart.bars.Count - 1).Data.Low + 3
            Dim params As RegressionParams = Nothing
            Dim difs As New List(Of Point)
            Dim maxAnglePossible = 44.589 * newRegressionLength ^ (-1.047)
            Dim hasCrossedAngleBoundary As Boolean = False
            For i = Length + 1 To Chart.bars.Count - 1
                For Each swing In swings
                    If swing.StartBar = i Then
                        Exit For
                    End If
                Next
                params = CalculateNextRegressionFromParams(i - Length + 1, i, i + 1, params)
                Dim currentRegressionPoint As Point = params.EndPoint
                plot.AddPoint(currentRegressionPoint, False)
                difs.Add(New Point(i, Chart.bars(i).Data.Avg - currentRegressionPoint.Y))
                plot4.AddNonContinuousPoint(New Point(i, graphLocation))
                plot4.AddPoint(New Point(i, graphLocation + (Chart.bars(i).Data.Avg - currentRegressionPoint.Y)), False)
                'NewTrendLine(Colors.Gray, New Point(i, graphLocation), New Point(i, graphLocation + (Chart.bars(i).Data.Avg - currentRegressionPoint.Y)))
            Next
            Dim anglePoints As New List(Of Point)
            plot3 = NewPlot(Colors.Blue)
            For i = newRegressionLength To difs.Count - 1
                plot3.AddPoint(New Point(difs(i).X - 1, graphLocation + CalculateRegressionY(difs.GetRange(i - newRegressionLength, newRegressionLength), difs(i).X - 1)), False)

                If plot3.Points.Count > 1 Then
                    plot5.AddNonContinuousPoint(New Point(difs(i).X - 1, graphLocation2))
                    plot5.AddPoint(New Point(difs(i).X - 1, graphLocation2 + (plot3.Points(plot3.Points.Count - 1).Y - plot3.Points(plot3.Points.Count - 2).Y) * 10), False)
                    anglePoints.Add(New Point(difs(i).X - 1, (plot3.Points(plot3.Points.Count - 1).Y - plot3.Points(plot3.Points.Count - 2).Y) * 10))
                    If Abs((plot3.Points(plot3.Points.Count - 1).Y - plot3.Points(plot3.Points.Count - 2).Y) * 10) > maxAnglePossible * 0.35 Then
                        hasCrossedAngleBoundary = True
                    End If
                    If plot3.Points.Count > 2 Then
                        Dim val = plot3.Points(plot3.Points.Count - 1).Y - plot3.Points(plot3.Points.Count - 2).Y
                        Dim val2 = plot3.Points(plot3.Points.Count - 2).Y - plot3.Points(plot3.Points.Count - 3).Y
                        If ((val >= 0) <> (val2 >= 0)) And hasCrossedAngleBoundary Then
                            If val < 0 Then
                                plot7.AddNonContinuousPoint(New Point(difs(i).X - 1, Chart.bars(difs(i).X - 2).Data.Low))
                                plot7.AddPoint(New Point(difs(i).X - 1, graphLocation2), False)
                            Else
                                plot6.AddNonContinuousPoint(New Point(difs(i).X - 1, Chart.bars(difs(i).X - 2).Data.Low))
                                plot6.AddPoint(New Point(difs(i).X - 1, graphLocation2), False)
                            End If
                            hasCrossedAngleBoundary = False
                            'Dim tl = NewTrendLine(Colors.Black, New Point(difs(i).X - 1, 0), New Point(difs(i).X - 1, 1))
                            'tl.ExtendRight = True
                            'tl.ExtendLeft = True
                        End If
                    End If
                End If
            Next

            NewTrendLine(Colors.Red, New Point(0, graphLocation2 + maxAnglePossible), New Point(Chart.bars.Count, graphLocation2 + maxAnglePossible))
            NewTrendLine(Colors.Red, New Point(0, graphLocation2 - maxAnglePossible), New Point(Chart.bars.Count, graphLocation2 - maxAnglePossible))
            'Log(newRegressionLength & "," & CStr(maxAnglePossible - anglePoints.Max(Function(item As Point) Abs(item.Y))))
            'anglePoints
            Dim angleRegressionLength As Integer = 30
            For i = angleRegressionLength To anglePoints.Count - 1
                plot8.AddPoint(New Point(anglePoints(i).X - 1, graphLocation2 + CalculateRegressionY(anglePoints.GetRange(i - angleRegressionLength, angleRegressionLength), anglePoints(i).X - 1)), False)
            Next
            'plot.RefreshVisual()
            'plot2.RefreshVisual()
            plot3.RefreshVisual()
            plot4.RefreshVisual()
            plot5.RefreshVisual()
            plot6.RefreshVisual()
            plot7.RefreshVisual()
            plot8.RefreshVisual()
            NewTrendLine(Colors.Red, New Point(0, graphLocation), New Point(Chart.bars.Count, graphLocation))
            NewTrendLine(Colors.Red, New Point(0, graphLocation2), New Point(Chart.bars.Count, graphLocation2))
            contentButton.Content = "time: " & CStr((Now - t).TotalMilliseconds)



            'Dim fileLines As List(Of String) = File.ReadAllLines("C:\Documents and Settings\David\Desktop\data.txt").ToList
            'Dim newLines As New List(Of String)
            'For Each line In fileLines
            '    Dim vals() As String = Split(line, ",")
            '    Dim d As New Date(vals(4), vals(5), vals(6), vals(7), vals(8), vals(9))
            '    newLines.Add(String.Format("{0},{1},{2},{3},{4},{5}", vals(0), vals(1), vals(2), vals(3), DateToString(d), "15"))
            'Next
            'File.WriteAllLines("C:\Documents and Settings\David\Desktop\newdata.txt", newLines.ToArray)
            'Exit Sub


            'Dim maxOffset As Decimal
            'Dim maxStepLength As Integer
            'Dim drawLines As Boolean = True
            'Dim itterationLength As Integer = Length
            'Dim drawLocation As Decimal = Chart.bars(Chart.bars.Count - 1).Data.Low - 100
            'Dim drawLocation2 As Decimal = Chart.bars(Chart.bars.Count - 1).Data.Low - 200
            'Dim drawLocation3 As Decimal = Chart.bars(Chart.bars.Count - 1).Data.Low - 300
            'Dim magicValue As Decimal = 20
            'Length = 35
            ''NewTrendLine(Colors.Red, New Point(stepLength + 1, drawLocation), New Point(Chart.bars.Count, drawLocation), drawLines)
            ''NewTrendLine(Colors.Green, New Point(stepLength + 1, drawLocation - magicValue), New Point(Chart.bars.Count, drawLocation - magicValue), drawLines)
            ''NewTrendLine(Colors.Green, New Point(stepLength + 1, drawLocation + magicValue), New Point(Chart.bars.Count, drawLocation + magicValue), drawLines)
            ''Dim increased As Boolean = False
            'Dim lastTime As DateTime = Now
            'Dim startValue As Integer = 30
            'Dim endValue As Integer = 70
            'Dim stepValue As Integer = 1
            'Dim multiplier As Decimal = 0.8
            'Dim startTime As DateTime = Now

            'For multiplier = 0.1 To 1 Step 0.01
            '    Dim topMultiplier As Decimal = 0
            '    Dim topPercentage As Decimal = 0
            '    Dim topPercentageCount As Integer = 0
            '    Dim totalCount As Integer = 0
            '    Dim lines As New List(Of String)
            '    lines = File.ReadAllLines("data\" & multiplier & ".txt").ToList
            '    For i = lines.Count - 1 To 0 Step -1
            '        Dim values() As Decimal = Split(lines(i), ",").Select(Function(value) CDec(value)).ToArray
            '        If Floor(CInt(values(2))) >= 50 Then
            '            For j = i To 0 Step -1
            '                totalCount += Split(lines(j), ",").Select(Function(value) CDec(value)).ToArray(1)
            '            Next
            '            topPercentage = values(0)
            '            Exit For
            '        End If
            '    Next
            '    Log(multiplier & "," & topPercentage)
            'Next
            'Exit Sub


            'If Not IO.Directory.Exists("data") Then IO.Directory.CreateDirectory("data")
            ''For multiplier = 0.1 To 1 Step 0.01
            'Dim barnumberandnumberoflinecrosses As New Dictionary(Of Integer, Decimal)
            'Dim directions As New Dictionary(Of Integer, Boolean) ' up = true 
            'For itterationLength = endValue To endValue Step stepValue
            '    'avgScoreTotaler = 0
            '    'data = {New List(Of Decimal)(), New List(Of Decimal), New List(Of Decimal)}
            '    'For k = 18 To 300 Step 3
            '    magicValue = itterationLength * multiplier
            '    Dim color As Color = Colors.Green ' color.FromArgb(255, (stepLength - startValue) / endValue * 255, (stepLength - startValue) / endValue * 255, (stepLength - startValue) / endValue * 255)
            '    Dim currentSwing As TrendLine = NewTrendLine(color, New Point(itterationLength + 2, drawLocation), New Point(itterationLength + 2, drawLocation), False)
            '    Dim currentDirection As Boolean
            '    Dim scores As New List(Of Decimal)
            '    Dim moreThanMagicValue As Boolean = False
            '    For i = itterationLength + 1 To Chart.bars.Count - 1
            '        Dim middleLine As LineCoordinates = CalculateNewStartPointRegressionMiddle(i - itterationLength, i - itterationLength + 1, i + 1, i)
            '        Dim offset As Decimal
            '        offset = (Chart.bars(i).Data.High + Chart.bars(i).Data.Low) / 2 - middleLine.EndPoint.Y
            '        If (currentDirection And drawLocation + offset > currentSwing.EndPoint.Y) Or
            '            (Not currentDirection And drawLocation + offset < currentSwing.EndPoint.Y) Then
            '            currentSwing.EndPoint = New Point(i + 1, drawLocation + offset)
            '        End If
            '        If (currentDirection And drawLocation + offset > drawLocation + magicValue) Or
            '            (Not currentDirection And drawLocation + offset < drawLocation - magicValue) Then
            '            moreThanMagicValue = True
            '        End If
            '        If ((currentDirection And drawLocation + offset < drawLocation + magicValue) Or
            '            (Not currentDirection And drawLocation + offset > drawLocation - magicValue)) And moreThanMagicValue Then
            '            moreThanMagicValue = False
            '            'Log(CStr(i + 1) & "," & percentage & "%")
            '            If Not barnumberandnumberoflinecrosses.ContainsKey(i + 1) Then
            '                barnumberandnumberoflinecrosses.Add(i + 1, 1)
            '                directions.Add(i + 1, currentDirection)
            '            Else
            '                barnumberandnumberoflinecrosses(i + 1) += 1
            '                directions(i + 1) = currentDirection
            '            End If
            '            'Dim tl = NewTrendLine(If(currentDirection, Colors.DarkGreen, Colors.DarkRed), New Point(i + 1, 0), New Point(i + 1, 1), True)
            '            'tl.ExtendLeft = True
            '            'tl.ExtendRight = True
            '        End If
            '        If (currentDirection And offset < 0) Or (Not currentDirection And offset > 0) Then
            '            currentDirection = Not currentDirection
            '            moreThanMagicValue = False
            '            currentSwing = NewTrendLine(color, AddToX(currentSwing.EndPoint, 1), New Point(i + 1, drawLocation + offset), False)
            '        End If

            '        'NewTrendLine(Colors.Red, New Point(i + 1, drawLocation), New Point(i + 1, drawLocation + offset), drawLines)

            '        If (Now - lastTime).TotalMilliseconds > 1000 Then '(multiplier - 0.1) / (1 - 0.1)+   * 0.009
            '            CType(CType(contentButton.Content, Grid).Children(1), TextBlock).Text = Round((Now - startTime).TotalMinutes / ((multiplier - 0.1) / (1 - 0.1) + 0.000001) - (Now - startTime).TotalMinutes) ' (((itterationLength - startValue) / (endValue - startValue)) + 0.0000001)) & " min"
            '            lastTime = Now ' +
            '            progressBar.Value = ((multiplier - 0.1) / (1 - 0.1)) ' ((itterationLength - startValue) / (endValue - startValue))
            '            DispatcherHelper.DoEvents()
            '        End If
            '    Next
            'Next

            'Dim ratioandnumberofsaidratio As New Dictionary(Of Decimal, Integer)
            'Dim ratioandlistofbarnumbers As New Dictionary(Of Decimal, List(Of Integer))
            'For Each barnumberandnumberoflinecross In barnumberandnumberoflinecrosses
            '    Dim ratio As Decimal = Round(barnumberandnumberoflinecross.Value / ((endValue - startValue) / stepValue + 1), 2)
            '    If ratio >= 0 Then
            '        If Not ratioandnumberofsaidratio.ContainsKey(ratio) Then
            '            ratioandnumberofsaidratio.Add(ratio, 0)
            '            ratioandlistofbarnumbers.Add(ratio, New List(Of Integer))
            '        End If
            '        ratioandnumberofsaidratio(ratio) += 1
            '        ratioandlistofbarnumbers(ratio).Add(barnumberandnumberoflinecross.Key)
            '    End If
            'Next
            'Dim sortedratioandnumberofsaidratio = (From entry In ratioandnumberofsaidratio Order By entry.Key Descending).ToDictionary(Function(pair) pair.Key, Function(pair) pair.Value)
            'Dim sum As New List(Of Integer)
            'Dim divdeBy As New List(Of Integer)
            'Dim values As New List(Of Decimal)
            'For Each ratioandnumberofsaidratiovalue In sortedratioandnumberofsaidratio
            '    Dim successCount As Integer = 0
            '    For Each barLocation In ratioandlistofbarnumbers(ratioandnumberofsaidratiovalue.Key)
            '        Dim pass As Boolean = False
            '        Dim curBarIndex As Integer = barLocation + 1
            '        Dim goodSwingLength As Integer = 20
            '        Dim reverseLength As Integer = 10
            '        While True
            '            If curBarIndex >= Chart.bars.Count Then
            '                pass = True
            '                Exit While
            '            End If
            '            If directions(barLocation) And Chart.bars(curBarIndex - 1).Data.High >= Chart.bars(barLocation - 1).Data.High + reverseLength Then
            '                pass = False
            '            End If
            '            If Not directions(barLocation) And Chart.bars(curBarIndex - 1).Data.Low <= Chart.bars(barLocation - 1).Data.Low - reverseLength Then
            '                pass = False
            '            End If
            '            If Chart.bars(curBarIndex - 1).Data.High >= Chart.bars(barLocation - 1).Data.High + goodSwingLength And Not directions(barLocation) Then
            '                pass = True
            '                Exit While
            '            End If
            '            If Chart.bars(curBarIndex - 1).Data.Low <= Chart.bars(barLocation - 1).Data.Low - goodSwingLength And directions(barLocation) Then
            '                pass = True
            '                Exit While
            '            End If
            '            If pass = False Then
            '                For Each swing In swings
            '                    If Abs(swing.EndBar - barLocation) <= 1 Then
            '                        pass = True
            '                        Exit For
            '                    End If
            '                Next
            '                Exit While
            '            End If
            '            curBarIndex += 1
            '        End While
            '        If pass Then successCount += 1
            '    Next
            '    Dim a = Join(ratioandlistofbarnumbers(ratioandnumberofsaidratiovalue.Key).Select(Function(itemID) itemID.ToString()).ToArray(), ", ")
            '    For Each location In ratioandlistofbarnumbers(ratioandnumberofsaidratiovalue.Key)
            '        'Log("bar number: " & location 
            '    Next
            '    sum.Add(Round(successCount / ratioandlistofbarnumbers(ratioandnumberofsaidratiovalue.Key).Count, 2) * 100 * ratioandlistofbarnumbers(ratioandnumberofsaidratiovalue.Key).Count)
            '    divdeBy.Add(ratioandlistofbarnumbers(ratioandnumberofsaidratiovalue.Key).Count)
            '    values.Add(ratioandnumberofsaidratiovalue.Key)
            '    'Log(CStr(ratioandnumberofsaidratiovalue.Key & "," & ratioandlistofbarnumbers(ratioandnumberofsaidratiovalue.Key).Count & "," & failureCount & "," & Round(1 - failureCount / ratioandlistofbarnumbers(ratioandnumberofsaidratiovalue.Key).Count, 2) * 100))
            '    'Log(ratioandnumberofsaidratiovalue.Key & "," & ratioandlistofbarnumbers(ratioandnumberofsaidratiovalue.Key).Count & "," & Round(successCount / ratioandlistofbarnumbers(ratioandnumberofsaidratiovalue.Key).Count, 2) * 100)

            'Next
            'Dim sum2 As Decimal = 0
            'Dim divideBy2 As Decimal = 0
            'For i = 0 To sum.Count - 1
            '    sum2 += sum(i)
            '    divideBy2 += divdeBy(i)
            '    'Log(values(i) & "," & divideBy2 & "," & Round(sum2 / divideBy2, 2))
            '    File.AppendAllLines("data\" & multiplier & ".txt", {values(i) & "," & divideBy2 & "," & Round(sum2 / divideBy2, 2)})
            'Next
            'Next

            'NewTrendLine(Colors.Red, New Point(stepLength + 1, drawLocation2), New Point(Chart.bars.Count, drawLocation2), drawLines)
            'Dim derivativeLength As Integer = 3
            'Dim derivativeValues As New List(Of Decimal)
            'For i = derivativeLength - 1 To offsets.Count - 1
            '    Dim pointList As New List(Of Point)
            '    For j = i + 1 - derivativeLength To i
            '        pointList.Add(New Point(j, offsets(j)))
            '    Next
            '    Dim value As Vector = CalculateRegression(pointList)
            '    Dim valueY As Decimal = RoundTo(value.Y, 1)
            '    NewTrendLine(Colors.Blue, New Point(i + stepLength + 2, drawLocation2), New Point(i + stepLength + 2, drawLocation2 + valueY * 2), True)
            '    derivativeValues.Add(valueY)
            'Next
            'For i = 1 To derivativeValues.Count - 1
            '    If derivativeValues(i) <> 0 Then
            '        For j = i - 1 To 1 Step -1
            '            If derivativeValues(j) <> 0 Then
            '                If derivativeValues(j) / Abs(derivativeValues(j)) <> derivativeValues(i) / Abs(derivativeValues(i)) Then
            '                    NewArrow(Colors.Blue, New Point(i + derivativeLength + stepLength + 1, (Chart.bars(i + derivativeLength + stepLength).Data.High + Chart.bars(i + derivativeLength + stepLength).Data.Low) / 2), False)
            '                End If
            '                Exit For
            '            End If
            '        Next
            '    End If
            'Next

            'NewTrendLine(Colors.Red, New Point(stepLength + 1, drawLocation3), New Point(Chart.bars.Count, drawLocation3), drawLines)
            'Dim barDerivativeLength As Integer = 3
            'For i = barDerivativeLength - 1 To Chart.bars.Count - 1
            '    Dim pointList As New List(Of Point)
            '    For j = i + 1 - barDerivativeLength To i
            '        pointList.Add(New Point(j, (Chart.bars(j).Data.High + Chart.bars(j).Data.Low) / 2))
            '    Next
            '    Dim value As Vector = CalculateRegression(pointList)
            '    Dim valueY As Decimal = RoundTo(value.Y, 1)
            '    NewTrendLine(Colors.Blue, New Point(i + 1, drawLocation3), New Point(i + 1, drawLocation3 + valueY * 2), True)
            'Next



            progressBar.Value = 1
            'Log(maxStepLength & "," & maxOffset)

            AddObjectToChart(processButtonUI)
        End Sub
        Private Property CurrentSwing As Swing
            Get
                Return swings(swings.Count - 1)
            End Get
            Set(ByVal value As Swing)
                swings(swings.Count - 1) = value
            End Set
        End Property
        Protected Overrides Sub Main()
            'If IsLastBarOnChart Then
            '    plots = New List(Of Plot)
            '    For length = 15 To 100
            '        plots.Add(NewPlot(Color.FromArgb(80, 255, 0, 0)))
            '        For i = length + 2 To Chart.bars.Count
            '            UpdateLine(i - length, i, False, length - 15)
            '        Next
            '    Next
            'End If
            If CurrentBar.High - CurrentSwing.EndPrice >= RV AndAlso CurrentBar.Number <> CurrentSwing.EndBar AndAlso swingDir = False Then
                'new swing up
                swingDir = True
                swings.Add(New Swing(NewTrendLine(Colors.Red, New Point(CurrentSwing.EndBar, CurrentSwing.EndPrice), New Point(CurrentBar.Number, CurrentBar.High), True), True))
            ElseIf CurrentSwing.EndPrice - CurrentBar.Low >= RV AndAlso CurrentBar.Number <> CurrentSwing.EndBar AndAlso swingDir = True Then
                ' new swing down
                swingDir = False
                swings.Add(New Swing(NewTrendLine(Colors.Red, New Point(CurrentSwing.EndBar, CurrentSwing.EndPrice), New Point(CurrentBar.Number, CurrentBar.Low), True), True))
            ElseIf CurrentBar.High >= CurrentSwing.EndPrice And swingDir = True Then
                ' extension up
                CurrentSwing.EndBar = CurrentBar.Number
                CurrentSwing.EndPrice = CurrentBar.High
            ElseIf CurrentBar.Low <= CurrentSwing.EndPrice And swingDir = False Then
                ' extension down
                CurrentSwing.EndBar = CurrentBar.Number
                CurrentSwing.EndPrice = CurrentBar.Low
            End If
        End Sub
        'Private Sub UpdateLine(startX As Integer, endX As Integer, shouldRefresh As Boolean, plotIndex As Integer)
        '    Dim highBar As New Point, lowBar As New Point
        '    Dim maxAboveDistance As Double, maxBelowDistance As Double
        '    Dim direction As Direction = If((Chart.bars(endX - 1).Data.High - Chart.bars(endX - 1).Data.Low) / 2 + Chart.bars(endX - 1).Data.High > (Chart.bars(startX - 1).Data.High - Chart.bars(startX - 1).Data.Low) / 2 + Chart.bars(startX - 1).Data.Low, True, False)
        '    Dim middleLine As Point = CalculateRegressionPoint(startX - 1, endX - 1, endX)
        '    Dim dir As Direction = If((Chart.bars(endX - 1).Data.High - Chart.bars(endX - 1).Data.Low) / 2 + Chart.bars(endX - 1).Data.High > (Chart.bars(startX).Data.High - Chart.bars(startX).Data.Low) / 2 + Chart.bars(startX).Data.Low, True, False)
        '    Dim yMin As Decimal = Decimal.MaxValue, yMax As Decimal
        '    For i = startX - 1 To endX - 1
        '        If Bars(i).High > yMax Then yMax = Bars(i).High
        '        If Bars(i).Low < yMin Then yMin = Bars(i).Low
        '        Dim aboveDistance = Sin(Atan((middleLine.X - middleLine.StartPoint.X) / Abs((middleLine.StartPoint.Y - middleLine.EndPoint.Y)))) * (Bars(i).High - LinCalc(middleLine.StartPoint.X, middleLine.StartPoint.Y, middleLine.EndPoint.X, middleLine.EndPoint.Y, i + 1))
        '        Dim belowDistance = Sin(Atan((middleLine.EndPoint.X - middleLine.StartPoint.X) / Abs((middleLine.StartPoint.Y - middleLine.EndPoint.Y)))) * (LinCalc(middleLine.StartPoint.X, middleLine.StartPoint.Y, middleLine.EndPoint.X, middleLine.EndPoint.Y, i + 1) - Bars(i).Low)
        '        If aboveDistance >= maxAboveDistance Then
        '            If (dir = direction.Down And Bars(i - 1).High < Bars(i).High) Or dir = direction.Up Then
        '                maxAboveDistance = aboveDistance
        '                highBar = New Point(i + 1, Bars(i).High)
        '            End If
        '        End If
        '        If belowDistance >= maxBelowDistance Then
        '            If (dir = direction.Up And Bars(i - 1).Low > Bars(i).Low) Or dir = direction.Down Then
        '                maxBelowDistance = belowDistance
        '                lowBar = New Point(i + 1, Bars(i).Low)
        '            End If
        '        End If
        '    Next
        '    Dim rangeBar As Point = If(dir = direction.Down, highBar, lowBar)
        '    If rangeBar.Y <> 0 Then 'if it is not a perfect swing with no retracements
        '        Dim minDistance As Decimal = Min(Abs(highBar.Y - LinCalc(middleLine.StartPoint.X, middleLine.StartPoint.Y, middleLine.EndPoint.X, middleLine.EndPoint.Y, highBar.X)),
        '                                             Abs(lowBar.Y - LinCalc(middleLine.StartPoint.X, middleLine.StartPoint.Y, middleLine.EndPoint.X, middleLine.EndPoint.Y, lowBar.X)))
        '        plots(plotIndex).AddPoint(middleLine.EndPoint)
        '    End If
        'End Sub
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

        Public Overrides Property Name As String = "Analytical"
    End Class
End Namespace