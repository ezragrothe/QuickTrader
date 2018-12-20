Namespace AnalysisTechniques

    Public Class BidAskPriceLines

#Region "AnalysisTechnique Inherited Code"
        Inherits AnalysisTechnique

        ' Inherit the one-argument constructor from the base class.
        Public Sub New(ByVal chart As Chart)
            MyBase.New(chart) ' Call the base class constructor.
            Description = "Draws lines indicating the bid, ask and last prices for live data. The last price line will only be shown with random data."
        End Sub
#End Region
        <Input()> Public Property BidLineVisible As Boolean = True
        <Input()> Public Property AskLineVisible As Boolean = True
        Public Property LastPriceLineVisible As Boolean = True
        <Input()> Public Property BidLineColor As Color = Colors.Red
        <Input()> Public Property AskLineColor As Color = Colors.Lime
        <Input()> Public Property LastPriceLineColor As Color = Colors.White
        <Input()> Public Property LastPriceLineExtendLeft As Boolean = False
        <Input()> Public Property LastPriceLineWidth As Decimal = 1

        '<Input()> Public Property BidAndAskLineWidth As Decimal = 10

        Dim askLine As TrendLine
        Dim bidLine As TrendLine
        Dim priceLine As TrendLine

        Protected Overrides Sub Begin()
            MyBase.Begin()
            Chart.Children.Remove(askLine)
            Chart.Children.Remove(bidLine)
            Chart.Children.Remove(priceLine)
            askLine = Nothing
            bidLine = Nothing
            priceLine = Nothing
            If AskLineVisible Then
                askLine = NewTrendLine(AskLineColor, New Point(0, 0), New Point(0, 0))
                askLine.AutoRefresh = True
                askLine.Pen.Thickness = LastPriceLineWidth
                askLine.IsEditable = False
                askLine.IsSelectable = False
            End If
            If BidLineVisible Then
                bidLine = NewTrendLine(BidLineColor, New Point(0, 0), New Point(0, 0))
                bidLine.AutoRefresh = True
                bidLine.Pen.Thickness = LastPriceLineWidth
                bidLine.IsEditable = False
                bidLine.IsSelectable = False
            End If
            If LastPriceLineVisible Then
                priceLine = NewTrendLine(LastPriceLineColor, New Point(0, 0), New Point(0, 0))
                priceLine.ExtendLeft = LastPriceLineExtendLeft
                priceLine.Pen.Thickness = LastPriceLineWidth
                priceLine.AutoRefresh = True
                priceLine.IsEditable = False
                priceLine.IsSelectable = False
            End If
        End Sub
        Protected Overrides Sub Main()
            'Dim startpoint As Decimal
            'startpoint = Chart.Bounds.X2 - Chart.GetRelativeFromRealWidth(BidAndAskLineWidth) - Chart.GetRelativeFromRealWidth(Chart.Settings("PriceBarWidth").Value)
            If IsLastBarOnChart Then
                Dim moveOverWidth As Decimal = Chart.GetRelativeFromRealWidth(Chart.Settings("PriceBarWidth").Value)
                If Not Chart.Settings("UseRandom").Value Then
                    If askLine IsNot Nothing Then
                        askLine.StartPoint = New Point(Chart.BarIndex, Chart.AskPrice)
                        askLine.EndPoint = New Point(Chart.Bounds.X2 - moveOverWidth, Chart.AskPrice)
                    End If
                    If bidLine IsNot Nothing Then
                        bidLine.StartPoint = New Point(Chart.BarIndex, Chart.BidPrice)
                        bidLine.EndPoint = New Point(Chart.Bounds.X2 - moveOverWidth, Chart.BidPrice)
                    End If
                End If
                If priceLine IsNot Nothing Then
                    priceLine.StartPoint = New Point(Chart.BarIndex, Chart.C)
                    priceLine.EndPoint = New Point(Chart.Bounds.X2 - moveOverWidth, Chart.C)
                End If
            End If
        End Sub

        Public Overrides Property Name As String = "Price Lines"
    End Class
End Namespace