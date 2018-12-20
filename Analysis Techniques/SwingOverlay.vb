Namespace AnalysisTechniques
    Public Class SwingOverlay
#Region "AnalysisTechnique Inherited Code"
        Inherits AnalysisTechnique

        ' Inherit the one-argument constructor from the base class.
        Public Sub New(ByVal chart As Chart)
            MyBase.New(chart) ' Call the base class constructor.
        End Sub
#End Region
        <Input("")>
        Public Property Color As Color = Colors.Gray
        <Input("")>
        Public Property Width As Decimal = 1
        Public Overrides Property Name As String = "SwingOverlay"
        Public lines As New List(Of TrendLine)
        Protected Overrides Sub Begin()
            MyBase.Begin()
            lines = New List(Of TrendLine)
        End Sub
        Protected Overrides Sub Main()

        End Sub
        Public Sub Refresh()
            Begin()
        End Sub
        Public Sub AddLine(startTime As Date, endTime As Date, direction As Direction)
            Dim startBar As Integer = -1
            Dim endBar As Integer = -1
            For i = 1 To Chart.bars.Count - 1
                If Chart.bars(i).Data.Date >= startTime And startBar = -1 Then
                    startBar = i
                End If
                If Chart.bars(i).Data.Date >= endTime And endBar = -1 Then
                    endBar = i
                End If
                If startBar <> -1 And endBar <> -1 Then Exit For
            Next
            If startBar <> -1 And endBar <> -1 Then
                If direction = Direction.Up Then
                    lines.Add(NewTrendLine(Color, New Point(startBar - 1, Chart.bars(startBar - 2).Data.Low), New Point(endBar, Chart.bars(endBar - 1).Data.High), True))
                    lines(lines.Count - 1).Pen.Thickness = Width
                Else
                    lines.Add(NewTrendLine(Color, New Point(startBar - 1, Chart.bars(startBar - 2).Data.High), New Point(endBar, Chart.bars(endBar - 1).Data.Low), True))
                    lines(lines.Count - 1).Pen.Thickness = Width
                End If
            End If
        End Sub
        Public Sub ExtendLine(endTime As Date, direction As Direction)
            Dim endBar As Integer = -1
            For i = 1 To Chart.bars.Count - 1
                If Chart.bars(i).Data.Date >= endTime And endBar = -1 Then
                    endBar = i
                    Exit For
                End If
            Next
            If endBar <> -1 And lines.Count > 0 Then
                If direction = Direction.Down Then
                    lines(lines.Count - 1).EndPoint = New Point(endBar, Chart.bars(endBar - 1).Data.Low)
                Else
                    lines(lines.Count - 1).EndPoint = New Point(endBar, Chart.bars(endBar - 1).Data.High)
                End If
            End If
        End Sub
    End Class
End Namespace
