Namespace AnalysisTechniques
    Public Class AutoTrendSwingBarCount
#Region "AnalysisTechnique Inherited Code"
        Inherits AutoTrendBase

        Public Sub New(ByVal chart As Chart)
            MyBase.New(chart) ' Call the base class constructor.
        End Sub
#End Region
        <Input()> Public Property PriceDataRV As Decimal = 2
        <Input()> Public Property PriceOffset As Decimal = 30
        <Input()> Public Property Increment As Decimal = 0.25

        Protected Overrides Sub Begin()
            SetCurrentBar(CurrentBarWithOffset)
            currentPrice = CurrentBar.Close - PriceOffset
            swing = New LineCoordinates(New Point(CurrentBar.Number, CurrentBar.Open), New Point(CurrentBar.Number, CurrentBar.Open))
            barCountSwing = New Swing(New TrendLine, Direction.Down)
            lastBarIndex = 1
            MyBase.Begin()
        End Sub
        Private currentPrice As Decimal
        Private Shadows swing As LineCoordinates
        Private swingDir As Direction
        Private barCountSwing As Swing
        Private Shadows isNewBar As Boolean
        Private lastBarIndex As Integer

        Protected Overrides Sub Main()
            'CalculateBar(New BarData(curprice, curprice, curprice - 1, curprice, time, TimeSpan.FromMinutes(1), index))
            Dim eventHappened As Boolean
            If CurrentBar.High - swing.EndPoint.Y >= PriceDataRV AndAlso CurrentBar.Number <> swing.EndPoint.X Then
                'new swing up
                swing = New LineCoordinates(New Point(swing.EndPoint.X, swing.EndPoint.Y), New Point(CurrentBar.Number, CurrentBar.High))
                swingDir = Direction.Up
                eventHappened = True
            ElseIf swing.EndPoint.Y - CurrentBar.Low >= PriceDataRV AndAlso CurrentBar.Number <> swing.EndPoint.X Then
                ' new swing down
                swing = New LineCoordinates(New Point(swing.EndPoint.X, swing.EndPoint.Y), New Point(CurrentBar.Number, CurrentBar.Low))
                swingDir = Direction.Down
                eventHappened = True
            ElseIf CurrentBar.High >= swing.EndPoint.Y And swingDir = Direction.Up Then
                ' extension up
                swing.EndPoint = New Point(CurrentBar.Number, CurrentBar.High)
                eventHappened = True
            ElseIf CurrentBar.Low <= swing.EndPoint.Y And swingDir = Direction.Down Then
                ' extension down
                swing.EndPoint = New Point(CurrentBar.Number, CurrentBar.Low)
                eventHappened = True
            End If
            If eventHappened Then
                For i = lastBarIndex + 1 To CurrentBar.Number
                    If swingDir = Direction.Up Then
                        currentPrice += Increment
                    Else
                        currentPrice -= Increment
                    End If

                    'Dim tl As TrendLine = NewTrendLine(Colors.LightGray, New Point(i, currentPrice), New Point(i, currentPrice - 0.03))
                    'tl.Pen.EndLineCap = PenLineCap.Round
                    'tl.Pen.StartLineCap = PenLineCap.Round
                    'tl.Pen.Thickness = 2
                    'tl.IsEditable = False
                    'tl.IsSelectable = False

                    CalculateBar(New BarData(currentPrice, currentPrice, currentPrice, currentPrice, CurrentBar.Date, CurrentBar.Duration, i))
                Next
                lastBarIndex = CurrentBar.Number
            End If
            isNewBar = False
        End Sub
        Protected Overrides Sub NewBar()
            isNewBar = True
           
        End Sub
        Private Function CurrentBarWithOffset() As BarData
            Return New BarData(CurrentBar.Open - PriceOffset, CurrentBar.High - PriceOffset, CurrentBar.Low - PriceOffset, CurrentBar.Close - PriceOffset, CurrentBar.Date, CurrentBar.Duration, CurrentBar.Number)
        End Function

        Public Overrides Property Name As String = "AutoTrendSwingBarCount"

    End Class
End Namespace
