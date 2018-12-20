Namespace AnalysisTechniques
    Public Class BarCount
        Inherits AnalysisTechnique

        <Input("The color for the label.")>
        Public Property Color As Color = Colors.Red

        Public Sub New(ByVal chart As Chart)
            MyBase.New(chart)
            Description = "Displays a label in the top right corner indicating the number of bars on the chart."
        End Sub

        Dim lbl As Label
        Dim plot As Plot
        Dim last10bars As Decimal()
        Protected Overrides Sub Begin()
            MyBase.Begin()
            lbl = NewLabel("", Colors.Green, New Point(0, 0))
            'plot.Points.Clear()
            'plot.RefreshVisual()
            plot = New Plot
            plot.AnalysisTechnique = Me
            plot.UseNegativeCoordinates = False
            'AddObjectToChart(plot)
            ReDim last10bars(9)
        End Sub

        Protected Overrides Sub Main()
            If IsLastBarOnChart Then
                If Not lbl.HasParent Then Chart.Children.Add(lbl)
                lbl.Text = "Bar Count: " & CurrentBar.Number
                Dim point As Point = Chart.Bounds.TopRight
                point = NegateY(point)
                point = AddToX(point, -Chart.GetRelativeFromRealWidth(Chart.Settings("PriceBarWidth").Value))
                point = AddToX(point, -Chart.GetRelativeFromRealWidth(10))
                point = AddToY(point, -Chart.GetRelativeFromRealHeight(20))
                lbl.Location = point
                lbl.IsEditable = True
                lbl.IsSelectable = True
                lbl.HorizontalAlignment = LabelHorizontalAlignment.Right
                lbl.Font.Brush = New SolidColorBrush(Color)
            End If
        End Sub

        Protected Overrides Sub NewBar()
            If CurrentBar.Number >= 10 Then
                For i = CurrentBar.Number - 10 To CurrentBar.Number - 1
                    last10bars(i - (CurrentBar.Number - 10)) = Chart.bars(i).Data.Close
                Next
                plot.Points.Add(New Point(CurrentBar.Number, last10bars.Average))
            End If
        End Sub
        Public Overrides Property Name As String = "Bar Counter"
    End Class

End Namespace

