Namespace AnalysisTechniques
    Public Class MovementCalculator
        Inherits AnalysisTechnique
        Public Sub New(ByVal chart As Chart)
            MyBase.New(chart)
            Description = "Click the Calc button to add texts to each of the user drawn trendlines on the chart."
        End Sub
        <Input> Public Property Color As Color = Colors.Black
        Dim processButtonUI As UIChartControl
        Dim originalLocation As New Point(400, 40)
        Dim progressBar As ProgressBar
        Dim contentButton As Button
        Protected Overrides Sub Begin()
            MyBase.Begin()
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
            'gridBackground.Children.Add(progressBar)
            gridBackground.Children.Add(New TextBlock With {.HorizontalAlignment = HorizontalAlignment.Center, .VerticalAlignment = VerticalAlignment.Center, .Text = "Calc"})
            contentButton.Content = gridBackground
            Grid.SetColumn(contentButton, 0)
            backgroundGrid.Children.Add(contentButton)
            Dim draggingBlock As New TextBlock
            Grid.SetColumn(draggingBlock, 1)
            draggingBlock.Background = Brushes.LightGray
            backgroundGrid.Children.Add(draggingBlock)
            processButtonUI.Content = backgroundGrid
            processButtonUI.Width = 69
            processButtonUI.Height = 28
            processButtonUI.Left = originalLocation.X
            processButtonUI.Top = originalLocation.Y
            processButtonUI.IsDraggable = True
            ' AddObjectToChart(processButtonUI)
            timer = New System.Windows.Forms.Timer
            timer.Interval = 2500
            timer.Start()
            AddHandler timer.Tick, AddressOf Elapsed
        End Sub
        Public Overrides Sub Reset()
            If processButtonUI IsNot Nothing Then
                originalLocation = New Point(processButtonUI.Left, processButtonUI.Top)
                RemoveHandler contentButton.Click, AddressOf ProcessButtonClick
                processButtonUI = Nothing
            End If
            MyBase.Reset()
        End Sub
        Dim newRegressionLength As Integer = 180
        Private Sub Elapsed()
            If Chart IsNot Nothing And IsEnabled Then
                ProcessButtonClick(Nothing, Nothing)
            End If
        End Sub

        Private Sub ProcessButtonClick(sender As Object, e As EventArgs)
            Dim i As Integer = 0
            While i < Chart.Children.Count
                If Chart.Children(i).AnalysisTechnique Is Me AndAlso (TypeOf Chart.Children(i) Is TrendLine Or TypeOf Chart.Children(i) Is Arrow Or TypeOf Chart.Children(i) Is Plot Or TypeOf Chart.Children(i) Is Label) Then
                    Chart.Children.RemoveAt(i)
                Else
                    i += 1
                End If
            End While
            If Chart.bars.Count > 0 Then
                Dim lst As New List(Of TrendLine)
                For Each ch In Chart.Children
                    If ch.AnalysisTechnique Is Nothing And TypeOf ch Is TrendLine Then
                        lst.Add(ch)
                    End If
                Next
                For Each ch In lst
                    If ch.AnalysisTechnique Is Nothing And TypeOf ch Is TrendLine Then
                        Dim absSum As Double = 0
                        Dim nonAbsSum As Double = 0
                        Dim t As TrendLine = ch
                        For i = Max(Min(t.StartPoint.X, t.EndPoint.X), 0) + 1 To Min(Max(t.StartPoint.X, t.EndPoint.X), Chart.bars.Count - 1)
                            absSum += Abs(Chart.bars(i).Data.Avg - Chart.bars(i - 1).Data.Avg)
                            nonAbsSum += Chart.bars(i).Data.Avg - Chart.bars(i - 1).Data.Avg
                        Next
                        If absSum <> 0 Then
                            Dim l As Label
                            If t.StartPoint.X > t.EndPoint.X Then
                                l = NewLabel(Round(Abs(nonAbsSum) / absSum * 100, 1), Color, NegateY(t.StartPoint))
                                If t.StartPoint.Y > t.EndPoint.Y Then
                                    l.VerticalAlignment = LabelVerticalAlignment.Top
                                Else
                                    l.VerticalAlignment = LabelVerticalAlignment.Bottom
                                End If
                            Else
                                l = NewLabel(Round(Abs(nonAbsSum) / absSum * 100, 1), Color, NegateY(t.EndPoint))
                                If t.EndPoint.Y > t.StartPoint.Y Then
                                    l.VerticalAlignment = LabelVerticalAlignment.Top
                                Else
                                    l.VerticalAlignment = LabelVerticalAlignment.Bottom
                                End If
                            End If
                            l.IsEditable = False
                            l.IsSelectable = False
                            l.HorizontalAlignment = LabelHorizontalAlignment.Center

                        End If
                    End If
                Next
            End If
            AddObjectToChart(processButtonUI)
        End Sub
        Private timer As System.Windows.Forms.Timer
        Protected Overrides Sub Main()

        End Sub

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

        Public Overrides Property Name As String = Me.GetType.Name
    End Class
End Namespace