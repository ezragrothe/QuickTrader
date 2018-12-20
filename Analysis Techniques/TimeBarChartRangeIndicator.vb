Imports System.Collections.ObjectModel

Namespace AnalysisTechniques
    Public Class TimeBarChartRangeIndicator
#Region "AnalysisTechnique Inherited Code"
        Inherits AnalysisTechnique

        ' Inherit the one-argument constructor from the base class.
        Public Sub New(ByVal chart As Chart)
            MyBase.New(chart) ' Call the base class constructor.
            Description = "A technique designed for time bar charts."
        End Sub
        <Input> Public Property Color As Color = Colors.Gray
        Dim rangePlot As Plot
        Protected Overrides Sub Begin()
            MyBase.Begin()
            rangePlot = NewPlot(Color)
        End Sub
        Protected Overrides Sub Main()
            If IsLastBarOnChart And (loadingHistory Or Chart.IsBarExtension) Then
                rangePlot.Points.Clear()
                Dim bottomLocation As Decimal = -Chart.GetRelativeFromRealY(Chart.ActualHeight)
                rangePlot.AutoRefresh = False
                For i = 0 To Chart.bars.Count - 1
                    rangePlot.AddNonContinuousPoint(New Point(i + 1, bottomLocation))
                    rangePlot.AddPoint(New Point(i + 1, bottomLocation + Chart.bars(i).Data.Range), False)
                Next
                rangePlot.RefreshVisual()
            End If
        End Sub
        Public Overrides Property Name As String = "TimeBarChartRangeIndicator"

#End Region
    End Class
End Namespace