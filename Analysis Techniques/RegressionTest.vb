Namespace AnalysisTechniques
	Public Class RegressionTest

#Region "AnalysisTechnique Inherited Code"
		Inherits AutoTrendBase
		Public Sub New(ByVal chart As Chart)
			MyBase.New(chart) ' Call the base class constructor.
			Name = "RegressionTest"
		End Sub
#End Region
		Dim tl As TrendLine
		Protected Overrides Sub Begin()
			MyBase.Begin()
			tl = NewTrendLine(Colors.Blue, New Point(0, 0), New Point(0, 0), True)
		End Sub
		Protected Overrides Sub Main()
			If IsLastBarOnChart Then
				Dim points As New List(Of Point)
				For Each bar In Chart.bars
					points.Add(New Point(bar.Data.Number, bar.Data.Close))
				Next
				Dim n As Decimal = Chart.bars.Count
				Dim a As Decimal
				Dim b As Decimal
				Dim sumx2 As Decimal = 0, sumx As Decimal = 0, sumy As Decimal = 0, sumxy As Decimal = 0
				For Each point In points
					sumx += point.X
					sumy += point.Y
					sumxy += point.X * point.Y
					sumx2 += point.X ^ 2
				Next
				b = (n * sumxy - sumx * sumy) / (n * sumx2 - sumx * sumx)
				a = (sumy - b * sumx) / n
				tl.StartPoint = New Point(1, a + b * 1)
				tl.EndPoint = New Point(Chart.bars.Count, a + b * Chart.bars.Count)
			End If
		End Sub


	End Class
End Namespace
