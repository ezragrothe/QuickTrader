Namespace AnalysisTechniques
    Public Class VelocityGraph
        Inherits AnalysisTechnique

        <Input> Public Property MinValue As Integer = 15
        <Input> Public Property MaxValue As Integer = 300
        Public Sub New(ByVal chart As Chart)
            MyBase.New(chart)
            Description = "Draws an average curve that follows the price."
        End Sub

        Dim plot As Plot
        Dim last10bars As Decimal()
        Protected Overrides Sub Begin()
            MyBase.Begin()
        End Sub
        Public Overrides Sub Reset()
            BarColorRoutine(0, Chart.bars.Count - 1, Chart.Settings("Bar Color").Value)
        End Sub
        Protected Overrides Sub Main()

        End Sub

        Protected Overrides Sub NewBar()
            If CurrentBar.Number > 1 Then
                Dim sec As Double = (Chart.bars(CurrentBar.Number - 1).Data.Date - Chart.bars(CurrentBar.Number - 2).Data.Date).TotalSeconds
                Dim col As Color
                If sec < MinValue Then
                    col = Color.FromArgb(255, 255, 0, 0)
                ElseIf sec > MaxValue Then
                    col = Color.FromArgb(255, 0, 0, 255)
                Else
                    col = Color.FromArgb(255, LinCalc(MinValue, 255, MaxValue, 0, sec), 0, LinCalc(MinValue, 0, MaxValue, 255, sec))
                End If
                Chart.bars(CurrentBar.Number - 1).Pen.Brush = New SolidColorBrush(col)
            End If
        End Sub
        Protected Sub BarColorRoutine(ByVal startBar As Integer, ByVal endBar As Integer, ByVal color As Color)
            For i = startBar To endBar
                If Not TypeOf Chart.bars(i).Pen.Brush Is SolidColorBrush Then
                    Chart.bars(i).Pen.Brush = New SolidColorBrush
                End If
                If CType(Chart.bars(i).Pen.Brush, SolidColorBrush).Color <> color Then
                    Chart.bars(i).Pen.Brush = New SolidColorBrush(color)
                    RefreshObject(Chart.bars(i))
                End If
            Next
        End Sub
        Public Overrides Property Name As String = "VelocityGraph"
    End Class

End Namespace

