Namespace AnalysisTechniques
    Public Class BarColoring
        Inherits AnalysisTechnique


        Public Sub New(ByVal chart As Chart)
            MyBase.New(chart)
        End Sub
        <Input> Public Property ISColor As Color = Colors.Black
        <Input> Public Property OSColor As Color = Colors.Blue
        <Input> Public Property NHColor As Color = Colors.Green
        <Input> Public Property NLColor As Color = Colors.Red
        <Input> Public Property BarThickness As Decimal = 1

        Protected Overrides Sub Begin()
            MyBase.Begin()

        End Sub

        Protected Overrides Sub Main()
            If CurrentBar.Number > 1 Then
                Dim b1 = Chart.bars(CurrentBar.Number - 2).Data
                Dim b2 = CurrentBar
                Dim col As Color
                If b2.High > b1.High And b2.Low >= b1.Low Then
                    col = NHColor
                ElseIf b2.High > b1.High And b2.Low < b1.Low Then
                    col = OSColor
                ElseIf b2.High <= b1.High And b2.Low < b1.Low Then
                    col = NLColor
                Else
                    col = ISColor
                End If
                Chart.bars(CurrentBar.Number - 1).Pen = New Pen(New SolidColorBrush(col), BarThickness)
                Chart.bars(CurrentBar.Number - 1).RefreshVisual()
            End If
        End Sub

        Public Overrides Property Name As String = Me.GetType.Name
    End Class

End Namespace

