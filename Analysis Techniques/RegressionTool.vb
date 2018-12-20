'Namespace AnalysisTechniques

'    Public Class RegressionTool

'#Region "AnalysisTechnique Inherited Code"
'        Inherits AnalysisTechnique
'        Public Sub New(ByVal chart As Chart)
'            MyBase.New(chart) ' Call the base class constructor.
'            Name = "RegressionTool"
'            If chart IsNot Nothing Then AddHandler chart.MouseLeftButtonDown, AddressOf MouseDown
'            If chart IsNot Nothing Then AddHandler chart.MouseLeftButtonUp, AddressOf Mouseup
'            If chart IsNot Nothing Then AddHandler chart.MouseMove, AddressOf MouseMove
'            If chart IsNot Nothing Then AddHandler chart.ChartKeyDown, AddressOf KeyPress
'        End Sub
'#End Region
'        Private Enum Direction
'            Up = -1
'            Down = 0
'        End Enum
'        Dim currentLines As New List(Of TrendLine())
'        Private Sub HandleMouseEvent(location As Point)
'            currentLines.Add({NewTrendLine(OuterLineColor, New Point(0, 0), New Point(0, 0), ShowOnlyMiddleLine = False), NewTrendLine(OuterLineColor, New Point(0, 0), New Point(0, 0), ShowOnlyMiddleLine = False), NewTrendLine(OuterLineColor, New Point(0, 0), New Point(0, 0))})
'            UpdateLine(currentLines.Count - 1, Chart.GetRelativeFromRealX(location.X))
'            For i = 0 To 1
'                currentLines(currentLines.Count - 1)(i).Pen = New Pen(New SolidColorBrush(OuterLineColor), OuterLineThickness)
'                currentLines(currentLines.Count - 1)(i).ExtendRight = True
'            Next
'            currentLines(currentLines.Count - 1)(2).Pen = New Pen(New SolidColorBrush(CenterLineColor), CenterLineThickness)
'        End Sub
'        Private Function CalculateNewStartPointRegressionMiddle(startPointX As Decimal, lineStartPoint As Decimal, lineEndPoint As Decimal, Optional endPointX As Decimal = -1) As LineCoordinates
'            If endPointX = -1 Then endPointX = CurrentBar.Number
'            Dim n As Decimal = (endPointX) - startPointX
'            Dim a As Decimal, b As Decimal
'            Dim sumx2 As Decimal = 0, sumx As Decimal = 0, sumy As Decimal = 0, sumxy As Decimal = 0
'            For bari = startPointX To endPointX - 1
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
'        Private Sub UpdateLine(index As Integer, startX As Integer)
'            If startX < 1 Then startX = 1
'            If startX >= CurrentBar.Number - 1 Then startX = CurrentBar.Number - 3
'            Dim highBar As New Point, lowBar As New Point
'            Dim maxAboveDistance As Double, maxBelowDistance As Double
'            Dim middleLine As LineCoordinates = CalculateNewStartPointRegressionMiddle(startX - 1, startX, CurrentBar.Number, CurrentBar.Number)
'            Dim direction As Direction = If((CurrentBar.High - CurrentBar.Low) / 2 + CurrentBar.High > (Chart.bars(startX).Data.High - Chart.bars(startX).Data.Low) / 2 + Chart.bars(startX).Data.Low, True, False)
'            For i = startX - 1 To CurrentBar.Number - 2
'                Dim aboveDistance = Sin(Atan((middleLine.EndPoint.X - middleLine.StartPoint.X) / Abs((middleLine.StartPoint.Y - middleLine.EndPoint.Y)))) * (Bars(i).High - LinCalc(middleLine.StartPoint.X, middleLine.StartPoint.Y, middleLine.EndPoint.X, middleLine.EndPoint.Y, i + 1))
'                Dim belowDistance = Sin(Atan((middleLine.EndPoint.X - middleLine.StartPoint.X) / Abs((middleLine.StartPoint.Y - middleLine.EndPoint.Y)))) * (LinCalc(middleLine.StartPoint.X, middleLine.StartPoint.Y, middleLine.EndPoint.X, middleLine.EndPoint.Y, i + 1) - Bars(i).Low)
'                If aboveDistance >= maxAboveDistance AndAlso i <> startX - 1 Then
'                    If (direction = RegressionTool.Direction.Down And Bars(i - 1).High < Bars(i).High) Or direction = RegressionTool.Direction.Up Then
'                        maxAboveDistance = aboveDistance
'                        highBar = New Point(i + 1, Bars(i).High)
'                    End If
'                End If
'                If belowDistance >= maxBelowDistance And i <> startX - 1 Then
'                    If (direction = RegressionTool.Direction.Up And Bars(i - 1).Low > Bars(i).Low) Or direction = RegressionTool.Direction.Down Then
'                        maxBelowDistance = belowDistance
'                        lowBar = New Point(i + 1, Bars(i).Low)
'                    End If
'                End If
'            Next
'            Dim rangeBar As Point = If(direction = RegressionTool.Direction.Down, highBar, lowBar)
'            If rangeBar.Y <> 0 Then 'if it is not a perfect swing with no retracements
'                Dim minDistance As Decimal = Min(Abs(highBar.Y - LinCalc(middleLine.StartPoint.X, middleLine.StartPoint.Y, middleLine.EndPoint.X, middleLine.EndPoint.Y, highBar.X)),
'                                                     Abs(lowBar.Y - LinCalc(middleLine.StartPoint.X, middleLine.StartPoint.Y, middleLine.EndPoint.X, middleLine.EndPoint.Y, lowBar.X)))
'                If direction = RegressionTool.Direction.Down Then minDistance = -minDistance
'                currentLines(index)(0).StartPoint = New Point(startX, middleLine.StartPoint.Y - minDistance)
'                currentLines(index)(0).EndPoint = New Point(rangeBar.X, LinCalc(middleLine, rangeBar.X) - minDistance)
'                currentLines(index)(1).StartPoint = New Point(startX, middleLine.StartPoint.Y + minDistance)
'                currentLines(index)(1).EndPoint = New Point(rangeBar.X, LinCalc(middleLine, rangeBar.X) + minDistance)
'                currentLines(index)(2).Coordinates = middleLine
'            End If
'        End Sub
'        Private Sub MouseMove(sender As Object, e As MouseEventArgs)
'            If e.LeftButton = MouseButtonState.Pressed And currentLines IsNot Nothing Then
'                For i = 0 To currentLines.Count - 1
'                    If currentLines(i)(0).IsSelected Or currentLines(i)(1).IsSelected Or currentLines(i)(2).IsSelected Then
'                        UpdateLine(i, Chart.GetRelativeFromRealX(e.GetPosition(Chart).X) - clickOffset)
'                        Exit For
'                    End If
'                Next
'            End If
'        End Sub
'        Private Sub MouseUp(sender As Object, e As MouseButtonEventArgs)

'        End Sub
'        Dim clickOffset As Decimal
'        Private Sub MouseDown(sender As Object, e As MouseButtonEventArgs)
'            If Keyboard.IsKeyDown(CreateRegressionHotkey) Then
'                HandleMouseEvent(e.GetPosition(Chart))
'            End If
'            If currentLines IsNot Nothing Then
'                For i = 0 To currentLines.Count - 1
'                    If currentLines(i)(0).IsSelected Or currentLines(i)(1).IsSelected Or currentLines(i)(2).IsSelected Then
'                        clickOffset = Chart.GetRelativeFromRealX(e.GetPosition(Chart).X) - currentLines(i)(0).StartPoint.X
'                        Exit For
'                    End If
'                Next
'            End If
'        End Sub
'        Private Sub KeyPress(ByVal sender As Object, ByVal e As KeyEventArgs)
'            If Chart IsNot Nothing AndAlso IsEnabled AndAlso Keyboard.Modifiers = ModifierKeys.None Then
'                Dim key As Key
'                If e.SystemKey = key.None Then
'                    key = e.Key
'                Else
'                    key = e.SystemKey
'                End If
'                If key = DeleteRegressionHotkey Then
'                    For i = 0 To currentLines.Count - 1
'                        If currentLines(i)(0).IsSelected Or currentLines(i)(1).IsSelected Or currentLines(i)(2).IsSelected Then
'                            For j = 0 To 2
'                                RemoveObjectFromChart(currentLines(i)(j))
'                            Next
'                            currentLines.RemoveAt(i)
'                            Exit For
'                        End If
'                    Next
'                End If
'            End If
'        End Sub

'        <Input()> Public Property CreateRegressionHotkey As Key = Key.OemBackslash
'        Public Property DeleteRegressionHotkey As Key = Key.Delete
'        <Input()> Public Property CenterLineColor As Color = Colors.Red
'        <Input()> Public Property CenterLineThickness As Decimal = 1
'        <Input()> Public Property OuterLineColor As Color = Colors.Red
'        <Input()> Public Property OuterLineThickness As Decimal = 1
'        <Input()> Public Property ShowOnlyMiddleLine As Boolean = False
'        Protected Overrides Sub Begin()
'            MyBase.Begin()
'        End Sub
'        Protected Overrides Sub Main()
'            If IsLastBarOnChart Then
'                Dim i As Integer = 0
'                If currentLines IsNot Nothing Then
'                    While i < currentLines.Count
'                        UpdateLine(i, currentLines(i)(2).StartPoint.X)
'                        If Not ShowOnlyMiddleLine Then AddObjectToChart(currentLines(i)(0)) Else RemoveObjectFromChart(currentLines(i)(0))
'                        If Not ShowOnlyMiddleLine Then AddObjectToChart(currentLines(i)(1)) Else RemoveObjectFromChart(currentLines(i)(1))
'                        AddObjectToChart(currentLines(i)(2))
'                        For j = 0 To 2
'                            currentLines(i)(j).Pen = New Pen(New SolidColorBrush(OuterLineColor), OuterLineThickness)
'                            currentLines(i)(j).ExtendRight = True
'                        Next
'                        currentLines(i)(2).Pen = New Pen(New SolidColorBrush(CenterLineColor), CenterLineThickness)
'                        i += 1
'                    End While
'                End If
'            End If
'        End Sub
'        Public Overrides Property Name As String = "RegressionTool"
'    End Class
'End Namespace

