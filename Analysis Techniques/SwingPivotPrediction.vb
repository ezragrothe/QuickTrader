Namespace AnalysisTechniques
    Public Class SwingPivotPrediction
        Inherits AnalysisTechnique
        Public Sub New(ByVal chart As Chart)
            MyBase.New(chart)
            Description = "An analytical technique to predict swing pivots."
        End Sub
        <Input> Property OffsetTriggerMultiplier As Decimal = 0.75
        <Input> Property HighPivotTextColor As Color = Colors.Green
        <Input> Property LowPivotTextColor As Color = Colors.Red
        <Input> Property StartLength As Integer = 30
        <Input> Property EndLength As Integer = 30
        <Input> Property LengthStep As Integer = 1
        Property MinimumPredictionStrengthCutoff As Decimal = 0.2
        <Input> Property ShowEveryPrediction As Boolean = True
        Protected Overrides Sub Begin()
            MyBase.Begin()
            currentDirection = New Dictionary(Of Integer, Boolean)
            moreThanMagicValue = New Dictionary(Of Integer, Boolean)
            maxPredictionStrength = 0
            lastPlaceLocation = 0
            totalLoss = 0
            totalProfit = 0
            totalTrades = 0
            MinimumPredictionStrengthCutoff = 0.822 - OffsetTriggerMultiplier * 0.75
            dataFile = IO.File.ReadAllLines("data\" & FormatNumber(Round(OffsetTriggerMultiplier, 2), 2) & ".txt").ToList()
            For i = dataFile.Count - 1 To 0 Step -1
                Dim values() As Decimal = Split(dataFile(i), ",").Select(Function(value) CDec(value)).ToArray
                If Floor(CInt(values(2))) >= 50 Then
                    MinimumPredictionStrengthCutoff = values(0)
                    Exit For
                End If
            Next
            'ShowInfoBox(MinimumPredictionStrengthCutoff, Chart.DesktopWindow)
        End Sub
        Dim currentDirection As Dictionary(Of Integer, Boolean)
        Dim moreThanMagicValue As Dictionary(Of Integer, Boolean)
        Dim maxPredictionStrength As Integer
        Dim maxPredictedBar As Integer
        Dim predictedOnLastBar As Boolean

        Dim lastPlaceLocation As Integer
        Dim lastPlacePosition As Boolean 'true = buy
        Dim totalProfit As Integer
        Dim totalLoss As Integer
        Dim totalTrades As Integer
        Dim dataFile As List(Of String) = Nothing
        Protected Overrides Sub Main()

            'If lastPlaceLocation <> 0 Then
            '    If lastPlacePosition = True Then ' if buy
            '        If CurrentBar.High > Chart.bars(lastPlaceLocation).Data.Low + profit Then
            '            NewTrendLine(Colors.DarkGreen, New Point(lastPlaceLocation + 1, Chart.bars(lastPlaceLocation).Data.Low), New Point(CurrentBar.Number, CurrentBar.High))
            '            totalProfit += profit
            '            totalTrades += 1
            '            lastPlaceLocation = 0
            '        ElseIf CurrentBar.Low < Chart.bars(lastPlaceLocation).Data.Low - drawdown Then
            '            NewTrendLine(Colors.DarkRed, New Point(lastPlaceLocation + 1, Chart.bars(lastPlaceLocation).Data.Low), New Point(CurrentBar.Number, CurrentBar.Low))
            '            totalLoss += drawdown
            '            totalTrades += 1
            '            lastPlaceLocation = 0
            '        End If
            '    Else
            '        If CurrentBar.Low < Chart.bars(lastPlaceLocation).Data.High - profit Then
            '            NewTrendLine(Colors.DarkGreen, New Point(lastPlaceLocation + 1, Chart.bars(lastPlaceLocation).Data.High), New Point(CurrentBar.Number, CurrentBar.Low))
            '            totalProfit += profit
            '            totalTrades += 1
            '            lastPlaceLocation = 0
            '        ElseIf CurrentBar.High > Chart.bars(lastPlaceLocation).Data.High + drawdown Then
            '            NewTrendLine(Colors.DarkRed, New Point(lastPlaceLocation + 1, Chart.bars(lastPlaceLocation).Data.High), New Point(CurrentBar.Number, CurrentBar.High))
            '            totalLoss += drawdown
            '            totalTrades += 1
            '            lastPlaceLocation = 0
            '        End If
            '    End If
            'End If
            If CurrentBar.Number <= EndLength + 1 Then Exit Sub
            Dim magicValue As Decimal
            Dim predictionStrength As Integer = 0
            Dim downDirectionCount As Integer
            Dim upDirectionCount As Integer
            For stepLength = StartLength To EndLength Step LengthStep
                If Not currentDirection.ContainsKey(stepLength) Then currentDirection.Add(stepLength, False)
                If Not moreThanMagicValue.ContainsKey(stepLength) Then moreThanMagicValue.Add(stepLength, False)
                magicValue = stepLength * OffsetTriggerMultiplier
                Dim i = CurrentBar.Number - 1
                Dim middleLine As LineCoordinates = CalculateNewStartPointRegressionMiddle(i - stepLength, i - stepLength + 1, i + 1, i)
                Dim offset As Decimal
                offset = (CurrentBar.High + CurrentBar.Low) / 2 - middleLine.EndPoint.Y
                If (currentDirection(stepLength) And offset > magicValue) Or
                    (Not currentDirection(stepLength) And offset < -magicValue) Then
                    moreThanMagicValue(stepLength) = True
                End If
                If ((currentDirection(stepLength) And offset < magicValue) Or
                    (Not currentDirection(stepLength) And offset > -magicValue)) And moreThanMagicValue(stepLength) Then
                    moreThanMagicValue(stepLength) = False
                    predictionStrength += 1
                    If currentDirection(stepLength) Then upDirectionCount += 1 Else downDirectionCount += 1
                End If
                If (currentDirection(stepLength) And offset < 0) Or (Not currentDirection(stepLength) And offset > 0) Then
                    currentDirection(stepLength) = Not currentDirection(stepLength)
                    moreThanMagicValue(stepLength) = False
                End If
            Next
            Dim direction As Boolean = upDirectionCount > downDirectionCount
            Dim ratio As Decimal = Round(predictionStrength / ((EndLength - StartLength) / LengthStep + 1), 2)
            If predictionStrength <> 0 Then
                Dim truePercentage As Decimal = 0
                Dim accuracyNumber As Integer
                For Each line In dataFile
                    Dim values() As Decimal = Split(line, ",").Select(Function(value) CDec(value)).ToArray
                    If Abs(values(0) - ratio) <= 0.01 Then
                        truePercentage = values(2)
                        accuracyNumber = values(1)
                        Exit For
                    End If
                Next
                If Round(truePercentage) = 42 Then
                    Dim a As New Object
                End If
                If ratio >= MinimumPredictionStrengthCutoff And ((predictedOnLastBar = False And ShowEveryPrediction = False) Or ShowEveryPrediction = True) Then
                    Dim label = NewLabel("- " & If(truePercentage = 0, "NA", CStr(Round(truePercentage))) & "% ", If(direction, HighPivotTextColor, LowPivotTextColor), New Point(CurrentBar.Number, Avg(Chart.bars(CurrentBar.Number - 1).Data.High, Chart.bars(CurrentBar.Number - 1).Data.Low)))
                    label.HorizontalAlignment = LabelHorizontalAlignment.Left
                    label.VerticalAlignment = LabelVerticalAlignment.Center
                End If
                predictedOnLastBar = True
                If ratio >= 0.4 Then
                    If lastPlaceLocation = 0 Then
                        lastPlaceLocation = CurrentBar.Number - 1
                        lastPlacePosition = Not direction
                    End If
                End If
            Else
                predictedOnLastBar = False
            End If
            If IsLastBarOnChart Then
                'Log("profit: " & totalProfit)
                'Log("loss: " & totalLoss)
                'Log("trades: " & totalTrades)
            End If
        End Sub

        Private Function CalculateNewStartPointRegressionMiddle(startPointX As Decimal, lineStartPoint As Decimal, lineEndPoint As Decimal, Optional endPointX As Decimal = -1) As LineCoordinates
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
            Return New LineCoordinates(New Point(lineStartPoint, a + b * lineStartPoint), New Point(lineEndPoint, a + b * lineEndPoint))
        End Function

        Public Overrides Property Name As String = "Swing Pivot Prediction"
    End Class
End Namespace
