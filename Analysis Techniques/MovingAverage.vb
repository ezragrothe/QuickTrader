'Namespace AnalysisTechniques
'    Public Class MovingAverage
'        Inherits AnalysisTechnique

'        <Input>
'        Public Property InnerColor As Color = Colors.Red
'        <Input>
'        Public Property OuterColor As Color = Colors.Red
'        <Input("The number of bars to use to determine the average price.")>
'        Public Property AveragingAmount As Integer = 10
'        <Input>
'        Public Property HorizOffsetAmount As Integer = 10
'        <Input>
'        Public Property VertOffsetAmount As Double = 10

'        Public Sub New(ByVal chart As Chart)
'            MyBase.New(chart)
'            Description = "Draws an average curve that follows the price."
'        End Sub

'        Dim plot As Plot
'        Dim plot2 As Plot
'        Dim plot3 As Plot
'        Dim last10bars As Decimal()
'        Protected Overrides Sub Begin()
'            MyBase.Begin()
'            plot = New Plot
'            plot.AnalysisTechnique = Me
'            plot.UseNegativeCoordinates = False
'            plot.Pen = New Pen(New SolidColorBrush(InnerColor), 1)
'            plot.IsSelectable = False
'            plot.IsEditable = False
'            AddObjectToChart(plot)
'            plot2 = New Plot
'            plot2.AnalysisTechnique = Me
'            plot2.UseNegativeCoordinates = False
'            plot2.Pen = New Pen(New SolidColorBrush(OuterColor), 1)
'            plot2.IsSelectable = False
'            plot2.IsEditable = False
'            AddObjectToChart(plot2)
'            plot3 = New Plot
'            plot3.AnalysisTechnique = Me
'            plot3.UseNegativeCoordinates = False
'            plot3.Pen = New Pen(New SolidColorBrush(OuterColor), 1)
'            plot3.IsSelectable = False
'            plot3.IsEditable = False
'            AddObjectToChart(plot3)
'            ReDim last10bars(AveragingAmount - 1)
'        End Sub

'        Protected Overrides Sub Main()

'        End Sub

'        Protected Overrides Sub NewBar()
'            If CurrentBar.Number >= AveragingAmount Then
'                For i = CurrentBar.Number - AveragingAmount To CurrentBar.Number - 1
'                    last10bars(i - (CurrentBar.Number - AveragingAmount)) = Chart.bars(i).Data.Close
'                Next
'                plot.Points.Add(New Point(CurrentBar.Number - HorizOffsetAmount, last10bars.Average))
'                plot2.Points.Add(New Point(CurrentBar.Number - HorizOffsetAmount, last10bars.Average + VertOffsetAmount))
'                plot3.Points.Add(New Point(CurrentBar.Number - HorizOffsetAmount, last10bars.Average - VertOffsetAmount))
'                plot.RefreshVisual()
'                plot2.RefreshVisual()
'                plot3.RefreshVisual()
'                If IsLastBarOnChart Then plot.RefreshVisual()
'            End If
'        End Sub
'        Public Overrides Property Name As String = "MovingAverage"
'    End Class

'End Namespace

Namespace AnalysisTechniques
    Public Class MovingAverage
        Inherits AnalysisTechnique

        <Input("The number of bars to determine the average.")>
        Public Property AveragingAmount As Integer = 20

        <Input("Channel1 Width.")>
        Public Property Channel1Width As Decimal = 2

        <Input("Channel2 Width.")>
        Public Property Channel2Width As Decimal = 4

        <Input("Line color.")>
        Public Property LineColor As Color = Colors.Blue

        <Input("Center line thickness.")>
        Public Property CenterLineThickness As Decimal = 1

        <Input("ChannelLine1 thickness.")>
        Public Property ChannelLine1Thickness As Decimal = 1

        <Input("ChannelLine2 thickness.")>
        Public Property ChannelLine2Thickness As Decimal = 1


        Public Sub New(ByVal chart As Chart)
            MyBase.New(chart)
            Description = "Draws an average curve that follows the price."
        End Sub

        Dim plot(4) As Plot
        Dim lastbars() As Decimal
        Protected Overrides Sub Begin()
            MyBase.Begin()
            For i As Integer = 0 To 4
                plot(i) = New Plot
                plot(i).AnalysisTechnique = Me
                plot(i).UseNegativeCoordinates = False
                plot(i).IsSelectable = False
                AddObjectToChart(plot(i))
            Next
            plot(0).Pen = New Pen(New SolidColorBrush(LineColor), CenterLineThickness)
            plot(1).Pen = New Pen(New SolidColorBrush(LineColor), ChannelLine1Thickness)
            plot(2).Pen = New Pen(New SolidColorBrush(LineColor), ChannelLine1Thickness)
            plot(3).Pen = New Pen(New SolidColorBrush(LineColor), ChannelLine2Thickness)
            plot(4).Pen = New Pen(New SolidColorBrush(LineColor), ChannelLine2Thickness)

            ReDim lastbars(AveragingAmount - 1)
        End Sub

        Protected Overrides Sub Main()
        End Sub
        Public Function GetAvgAtBar(bar As Integer) As Double
            If plot(0).Points.Count > bar - AveragingAmount And bar - AveragingAmount > 0 Then
                Return plot(0).Points(bar - AveragingAmount).Y
            Else
                Return Chart.bars(bar).Data.Avg
            End If
        End Function
        Protected Overrides Sub NewBar()
            If CurrentBar.Number >= AveragingAmount Then
                For i = CurrentBar.Number - AveragingAmount To CurrentBar.Number - 1
                    lastbars(i - (CurrentBar.Number - AveragingAmount)) = Chart.bars(i).Data.Close

                Next
                plot(0).Points.Add(New Point(CurrentBar.Number, lastbars.Average))
                plot(1).Points.Add(New Point(CurrentBar.Number, lastbars.Average + Chart.Settings("RangeValue").Value * Channel1Width))
                plot(2).Points.Add(New Point(CurrentBar.Number, lastbars.Average - Chart.Settings("RangeValue").Value * Channel1Width))
                plot(3).Points.Add(New Point(CurrentBar.Number, lastbars.Average + Chart.Settings("RangeValue").Value * Channel2Width))
                plot(4).Points.Add(New Point(CurrentBar.Number, lastbars.Average - Chart.Settings("RangeValue").Value * Channel2Width))
                If IsLastBarOnChart Then
                    For i As Integer = 0 To 4
                        plot(i).RefreshVisual()
                    Next
                End If
            End If
        End Sub
        Public Overrides Property Name As String = "MovingAverage"
    End Class

End Namespace


