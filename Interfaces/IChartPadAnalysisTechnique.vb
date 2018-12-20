Public Interface IChartPadAnalysisTechnique
    Property ChartPadLocation As Point
    Property ChartPad As UIChartControl
    Property ChartPadVisible As Boolean
    Sub InitChartPad()
    Sub UpdateChartPad()
    Sub UpdateChartPadColor(color As Color)
End Interface
