'Imports System.Threading

'Namespace AnalysisTechniques

'    Public Class PriceRV2Swing

'#Region "AnalysisTechnique Inherited Code"
'        Inherits AutoTrendBase
'        Public Sub New(ByVal chart As Chart)
'            MyBase.New(chart) ' Call the base class constructor.
'            Description = "Two-swing AutoTrend analysis technique."
'            Name = "PriceRV2Swing"
'            If chart IsNot Nothing Then AddHandler chart.ChartKeyDown, AddressOf KeyPress
'        End Sub
'#End Region

'        Const MANY_OPTIONS = True

'        Private bcLengths As New List(Of Double)
'        Private prevRV As Decimal = -1


'#Region "Commands"
'        Friend Overrides Sub PlusMinMove()
'            PriceRV += Chart.GetMinTick
'            Chart.ReApplyAnalysisTechnique(Me)
'        End Sub
'        Friend Overrides Sub MinusMinMove()
'            PriceRV -= Chart.GetMinTick
'            Chart.ReApplyAnalysisTechnique(Me)
'        End Sub
'        Friend Overrides Sub RVDown()
'            If bcLengths.Count > 0 Then
'                PriceRV = bcLengths(bcLengths.Count - 1)
'                bcLengths.RemoveAt(bcLengths.Count - 1)
'                Chart.ReApplyAnalysisTechnique(Me)
'            End If
'        End Sub
'        Friend Overrides Function GetRVBase() As Decimal
'            If CustomRangeValue = -1 Then
'                Return Chart.Settings("RangeValue").Value * 3
'            Else
'                Return CustomRangeValue * 3
'            End If
'        End Function
'        Friend Overrides Sub RVBase()
'            'If Not Chart.Settings("IsSlaveChart").Value Then
'            If PriceRV = GetRVBase() Then
'                If prevRV <> -1 Then
'                    PriceRV = prevRV
'                    Chart.ReApplyAnalysisTechnique(Me)
'                End If
'            Else
'                prevRV = PriceRV
'                PriceRV = GetRVBase()
'                Chart.ReApplyAnalysisTechnique(Me)
'            End If
'            'End If
'        End Sub
'        Friend Overrides Sub ApplyRV(value As Decimal)
'            PriceRV = value
'            Chart.ReApplyAnalysisTechnique(Me)
'        End Sub
'        Friend Sub ApplyRV1(value As Decimal)
'            RV1 = value
'            Chart.ReApplyAnalysisTechnique(Me)
'        End Sub
'        'Friend Overrides Sub RVUp()
'        '    Dim shortest As Decimal = Decimal.MaxValue
'        '    Dim nextShortest As Decimal = Decimal.MaxValue
'        '    For Each swing In swings2
'        '        If Abs(swing.StartPrice - swing.EndPrice) < shortest And Abs(swing.StartPrice - swing.EndPrice) >= RV2 Then
'        '            shortest = Round(Abs(swing.StartPrice - swing.EndPrice), 3)
'        '        End If
'        '    Next
'        '    If RV2 <> shortest Then
'        '        bcLengths.Add(RV2)
'        '        RV2 = shortest
'        '        Chart.ReApplyAnalysisTechnique(Me)
'        '        'MsgBox("")
'        '    Else
'        '        For Each swing In swings2
'        '            Dim height As Decimal = Abs(swing.StartPrice - swing.EndPrice)
'        '            If height > shortest Then
'        '                nextShortest = Min(nextShortest, height)
'        '            End If
'        '        Next
'        '        bcLengths.Add(RV2)
'        '        RV2 = nextShortest
'        '        Chart.ReApplyAnalysisTechnique(Me)
'        '    End If
'        '    'For i = channels.Count - 1 To 0 Step -1
'        '    '    Dim channelLine As Channel = channels(i)
'        '    '    If channelLine.IsConfirmed And Not channelLine.Is Then
'        '    '        bcLengths.Add(RV)
'        '    '        If RV = Round(Abs(CurrentSwing1.StartPrice - CurrentSwing1.EndPrice), 5) Then
'        '    '            RV = Round(Abs(CurrentSwing1.StartPrice - CurrentSwing1.EndPrice) + Chart.GetMinTick, 5)
'        '    '        ElseIf Round(Abs(CurrentSwing1.StartPrice - CurrentSwing1.EndPrice), 5) < Round(Abs(channelLine.BCSwingLine.EndPoint.Y - channelLine.BCSwingLine.StartPoint.Y), 5) Then
'        '    '            RV = Round(Abs(CurrentSwing1.StartPrice - CurrentSwing1.EndPrice), 5)
'        '    '        ElseIf RV = Round(Abs(channelLine.BCSwingLine.EndPoint.Y - channelLine.BCSwingLine.StartPoint.Y), 5) Then
'        '    '            RV = Round(Abs(channelLine.BCSwingLine.EndPoint.Y - channelLine.BCSwingLine.StartPoint.Y) + Chart.GetMinTick, 5)
'        '    '        Else
'        '    '            RV = Round(Abs((channelLine.BCSwingLine.EndPoint.Y - channelLine.BCSwingLine.StartPoint.Y)), 5)
'        '    '        End If
'        '    '        Chart.ReApplyAnalysisTechnique(Me)
'        '    '        Exit For
'        '    '    End If
'        '    'Next
'        'End Sub

'        Public Sub ApplyRegressionLength(value As Decimal)
'            If value <> 0 Then
'                If Length <> value Then
'                    Length = value
'                    Chart.ReApplyAnalysisTechnique(Me)
'                End If
'            End If
'        End Sub
'        Friend Overrides Sub ChannelMerge()
'            Dim prev As ChannelModeType = ChannelMode
'            ChannelMode = ChannelModeType.Merged
'            If prev <> ChannelMode Then Chart.ReApplyAnalysisTechnique(Me)
'        End Sub
'        Friend Overrides Sub ChannelUnmerge()
'            Dim prev As ChannelModeType = ChannelMode
'            ChannelMode = ChannelModeType.Unmerged
'            If prev <> ChannelMode Then Chart.ReApplyAnalysisTechnique(Me)
'        End Sub
'        Friend Overrides Sub ChannelUnmergeOverlap()
'            Dim prev As ChannelModeType = ChannelMode
'            ChannelMode = ChannelModeType.UnmergedOverlap
'            If prev <> ChannelMode Then Chart.ReApplyAnalysisTechnique(Me)
'        End Sub
'        Protected Sub KeyPress(ByVal sender As Object, ByVal e As KeyEventArgs)
'            Dim key As Key
'            If e.SystemKey = key.None Then
'                key = e.Key
'            Else
'                key = e.SystemKey
'            End If
'            If Keyboard.Modifiers = ModifierKeys.None Then
'                If key = MoveUpMomentumBarsPosition And MomentumBarsVisible = True Then
'                    VerticalMomentumBarsPosition += Chart.Bounds.Height / 15
'                    Chart.ReApplyAnalysisTechnique(Me)
'                ElseIf key = MoveDownMomentumBarsPosition And MomentumBarsVisible = True Then
'                    VerticalMomentumBarsPosition -= Chart.Bounds.Height / 15
'                    Chart.ReApplyAnalysisTechnique(Me)
'                ElseIf key = SetRVToLastSwingHotkey Then
'                    PriceRV = Abs(swings2(swings2.Count - 1).EndPrice - swings2(swings2.Count - 1).StartPrice) + Chart.GetMinTick()
'                End If
'            End If
'        End Sub
'#End Region

'#Region "Inputs"
'        <Input("Enter 0 for all bars")> Public Property BarProcessCount As Integer

'        <Input()> Public Property ChartPadRBDivider As Decimal = 4

'        'Public Property ChartPadRVMultiplier3 As Decimal = 2
'        Private _rv As Decimal = 2


'        Enum CurveFormula
'            MovingAverage
'            Regression
'        End Enum


'        Public Property CurvedLineFormula As CurveFormula = CurveFormula.MovingAverage
'        Private _regressionLength As Integer = 30
'        Public Property Length As Integer
'            Get
'                Return _regressionLength
'            End Get
'            Set(value As Integer)
'                _regressionLength = value
'                'If Chart IsNot Nothing Then
'                '    Chart.UpdateRangePad(Me)
'                'End If
'            End Set
'        End Property
'        <Input(, "Swing")>
'        Public Property RV1 As Decimal
'        <Input()> Public Property ChartPadRVMultiplier1 As Decimal = 4
'        <Input> Public Property SwingLineThickness As Decimal = 0.7
'        <Input> Public Property SwingLineColor As Color = (New ColorConverter).ConvertFrom("#FFB20000")
'        <Input()> Public Property LengthTextFontSize1 As Double = 11
'        <Input()> Public Property LengthTextFontWeight1 As FontWeight = FontWeights.Bold
'        <Input> Public Property RVAndTargetTextFontSize1 As Double = 12
'        <Input()> Public Property RVAndTargetTextFontWeight1 As FontWeight = FontWeights.Bold
'        <Input()> Public Property SwingChannelOn As Boolean = True
'        <Input()> Public Property SwingChannelHistoryOn As Boolean = True
'        '<Input()> Public Property RVTextFontWeight1 As FontWeight = FontWeights.Bold
'        '<Input()> Public Property RVTextFontSize1 As Double = 16

'        <Input(, "Trend")>
'        Public Overrides Property PriceRV As Decimal
'            Get
'                Return _rv
'            End Get
'            Set(value As Decimal)
'                _rv = Round(value, 7)
'            End Set
'        End Property
'        <Input()> Public Property ChartPadRVMultiplier2 As Decimal = 3
'        <Input()> Public Property TrendChannelThickness As Decimal = 0.7
'        <Input()> Public Property UpTrendChannelColor As Color = (New ColorConverter).ConvertFrom("#FF004C00")
'        <Input()> Public Property DownTrendChannelColor As Color = (New ColorConverter).ConvertFrom("#FF7F0000")
'        <Input()> Public Property NeutralChannel1Thickness As Decimal
'        <Input()> Public Property NeutralChannel2Thickness As Decimal
'        <Input()> Public Property NeutralChannelUp1Color As Color
'        <Input()> Public Property NeutralChannelDown1Color As Color
'        <Input()> Public Property NeutralChannelUp2Color As Color
'        <Input()> Public Property NeutralChannelDown2Color As Color
'        <Input()> Public Property LengthTextFontSize2 As Double = 14
'        <Input()> Public Property LengthTextFontWeight2 As FontWeight = FontWeights.Bold
'        <Input()> Public Property RVAndTargetTextFontSize2 As Double = 12
'        <Input()> Public Property RVAndTargetTextFontWeight2 As FontWeight = FontWeights.Bold
'        '<Input()> Public Property SwingChannelNeutralUpColor As Color
'        '<Input()> Public Property SwingChannelNeutralDownColor As Color
'        '<Input()> Public Property SwingChannelNeutralLineIsDashed As Boolean
'        <Input()> Public Property CounterTrendBarDelay As Integer = 10

'        Public Property TrendIndicationLineWidth As Decimal = 1
'        Public Property CurvedLineMultiplier As Decimal = 0
'        Public Property CurvedLineThickness As Decimal = 0
'        Public Property CurvedLineColor As Color = Colors.Black

'        'Public Property RVTextColor As Color = Colors.BlueViolet

'        <Input(, "Bar Coloring")>
'        Public Property UpColor As Color = (New ColorConverter).ConvertFrom("#FF00B200")
'        <Input()> Public Property NeutralColor As Color = (New ColorConverter).ConvertFrom("#FF000000")
'        <Input()> Public Property DownColor As Color = (New ColorConverter).ConvertFrom("#FFB20000")
'        <Input(, "Misc")> Public Property LastSwingIsChannel1 As Boolean = False
'        <Input> Public Property SetRVToLastSwingHotkey As Key = Key.S
'        <Input> Public Property ShowPercentPriceChangeOnSwing2 As Boolean = True
'        Public Property ColorSwingBySwing As Boolean = True


'        Public Property TrendChannelOn As Boolean = False
'        Public Property TrendChannelUpColor As Color = Colors.Green
'        Public Property TrendChannelDownColor As Color = Colors.Red
'        Public Property UpTrendChannelNeutralColor As Color = Colors.Black
'        Public Property DownTrendChannelNeutralColor As Color = Colors.Black
'        Public Property TrendChannelThickness2 As Decimal = 1.6
'        Public Property TrendChannelCenterLineThickness As Decimal = 1.6
'        Public Property PushTextFontSize As Double = 22
'        Public Property TrendTextFontSize As Double = 14
'        Public Property TrendTextFontWeight As FontWeight = FontWeights.Bold


'        Public Property SwingChannelOn2 As Boolean = True
'        Public Property SwingChannelOn1 As Boolean = False

'        Public Property SwingChannelHistoryThickness As Decimal = 1.1
'        Public Property SwingChannelLengthAndTargetTextsOn As Boolean = True



'        Public Property SwingLinesOn2 As Boolean = False
'        Public Property SwingLinesOn1 As Boolean = True
'        Public Property LastSwingLineOn As Boolean = False
'        Public Property ProjectionLinesOn As Boolean = False
'        Public Property ProjectionLineColor As Color = Colors.BlueViolet
'        Public Property ProjectionLineThickness As Decimal = 0.4


'        Public Property SwingTextsOn As Boolean = True

'        '<Input(, "Bar Coloring")>
'        'Public Property AboveBarColor As Color = Colors.Green
'        '<Input()> Public Property BelowBarColor As Color = Colors.Red

'        Public Property RVLineColor As Color = Colors.Black
'        Public Property DrawingCutoff As Integer = 100
'        Public Property SyncChartsHotKey As Key = Key.LeftCtrl
'        Public Property MomentumBarsVisible As Boolean = False
'        Public Property NoOfMomentumBars As Integer = 100
'        Public Property VerticalMomentumBarsPosition As Double = 0
'        Public Property MomentumOffsetLineMultiplier As Decimal = 2
'        Public Property MomentumOffsetLineColor As Color = Colors.Black
'        Public Property MomentumOffsetLineThickness As Decimal = 1
'        Public Property MoveUpMomentumBarsPosition As Key = Key.Down
'        Public Property MoveDownMomentumBarsPosition As Key = Key.Up

'        Public Property Mode As ModeType = ModeType.RegressionFocused
'        Enum ModeType
'            RegressionFocused
'            AutoTrendFocused
'        End Enum




'        Public Property ShowCenterCurvedLine As Boolean = True
'        Public Property ChannelCenterLineColor As Color = Colors.Transparent
'        Public Property ShowTrailingLine As Boolean = False
'        Public Property TrailingLineColor As Color = ColorConverter.ConvertFromString("#FF904C00")
'        Public Property TrailingLineThickness As Decimal = 1


'        Public Property ShowBox As Boolean = False
'        Public Property BoxColor As Color = Colors.Black
'        Public Property BoxThickness As Decimal = 1
'        Public Property AddFillLines As Boolean = False

'        Enum RegressionBarColoringAlgorithm
'            StraightLineBased
'            CurvedLineBased
'        End Enum

'        Public Overrides Property CustomRangeValue As Double = -1
'        Public Property RVTextSize As Decimal = 20

'        Public Property ChannelMode As ChannelModeType = ChannelModeType.Unmerged
'        Property SwingCountTextLocation As Point = New Point(1580, 680)
'        Property SwingCountTextColor As Color = Colors.Blue


'        Public Property UseRegressionConfirmedChannels As Boolean = True
'        Public Property UseRegressionPotentialChannels As Boolean = True
'        Public Property DrawRegressionChannels As Boolean = True

'        Public Overrides ReadOnly Property RVPadAppliesToAllCharts As Boolean
'            Get
'                Return Keyboard.IsKeyDown(SyncChartsHotKey)
'            End Get
'        End Property

'        '<Input("The color for swing lines during an up trend.")>
'        Public Property UpTrendSwingLineColor As Color = Colors.Black
'        '<Input("The color for swing lines during a down trend.")>
'        Public Property DownTrendSwingLineColor As Color = Colors.Black


'        Public Property ConfirmedUpChannelLineColor As Color = Colors.Green
'        '<Input("The color for confirmed down channel lines.")>
'        Public Property ConfirmedDownChannelLineColor As Color = Colors.Red



'#End Region


'        Public Class RegressionChannel
'            Public Sub New()
'                BCLengths = New List(Of ABCCombo)
'            End Sub
'            Public Property TL As TrendLine
'            Public Property StartSwingIndex As Integer
'            Public Property EndSwingIndex As Integer
'            Public Property Direction As Swing.ChannelDirectionType

'            Public Property IsConfirmed As Boolean
'            Public Property APoint As Point
'            'Public Property BPoint As Point
'            'Public Property CPoint As Point
'            Public Property DPoint As Point
'            Public Property BCLengths As List(Of ABCCombo)
'            Public Property Pushes As Integer
'        End Class

'        Dim regressionChannels As List(Of RegressionChannel)
'        Dim potentialRegressionChannel As RegressionChannel
'        Dim potentialSameDirRegressionChannel As RegressionChannel
'        Public Overrides Property Name As String

'        Dim rvTopLinePlot As Plot
'        Dim rvBotLinePlot As Plot
'        Dim curvedTopLinePlot As Plot
'        Dim curvedBotLinePlot As Plot
'        'Private rvText As Label
'        Private rvProNewLine As TrendLine

'        Private currentGapMark As GapMarker

'        Private barColorsDirty As Boolean

'        Private swings2 As List(Of Swing)
'        Private swings1 As List(Of Swing)
'        'Private channels As List(Of Channel)
'        Private coloringChannels As List(Of Channel)
'        'Private swingChannel As SwingChannelLine
'        'Private swingChannels As List(Of SwingChannelLine)
'        Private currentSwingBCLengthText As Label
'        Private currentSwingBCTargetText As Label
'        Private currentSwingPotentialBCTargetText As Label
'        Private currentPartiallyCutChannel As PartiallyCutChannel
'        Private dayHighPrice As Decimal
'        Private dayLowPrice As Decimal
'        'Private colorMergedChannels As Boolean = True
'        Private swingCountGraphYPlacement As Decimal
'        Private currentSwingCountGraphLine As TrendLine
'        Private timeLabel As Label
'        Dim slider As UIChartControl
'        Dim originalLocation As New Point(2, 28)
'        'Dim regressionChannelLines() As TrendLine
'        Dim currentRegressionChannel As TrendLine
'        Dim currentRegressionChannelDirection As Swing.ChannelDirectionType
'        Dim currentRegressionCenterPoint As Decimal
'        Dim regressionCenterPoints As Dictionary(Of Integer, Decimal)
'        'Dim currentSwingChannel As TrendLine
'        Dim regParams As RegressionParams
'        Dim regParams2 As RegressionParams
'        'Const predictionTextSuffix As String = "────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────"
'        'Dim targetTextBarOffset As Integer = 4
'        'Dim newSwingTextBarOffset As Integer = 15
'        Const predictionTextPrefix As String = "── "
'        Const rvTextPrefix2 As String = "──────── "
'        Const newRVTextPrefix2 As String = "──────── "
'        Const rvTextPrefix1 As String = "── "
'        Const newRVTextPrefix1 As String = "── "
'        Const trendTextOffset As Integer = 15
'        Const horizSwingTextAlignment As Double = 1
'        Const vertSwingTextAlignment As Double = 6
'        Private extendTrendTextObject As Label
'        Private newTrendTextObject As Label
'        Private extendTrendLineObject As TrendLine
'        Private newTrendLineObject As TrendLine
'        Private extendSwingTextObject2 As Label
'        Private extendSwingTextObject1 As Label
'        Private newSwingTextObject2 As Label
'        Private newSwingTextObject1 As Label
'        Dim processButtonUI As UIChartControl
'        Dim originalLocation2 As New Point(40, 40)
'        Dim contentButton As Button
'        Dim lastSwingPotentialLine2 As TrendLine
'        Dim lastSwingCounterPotentialLine2 As TrendLine
'        Dim lastSwingPotentialLine1 As TrendLine
'        Dim lastSwingCounterPotentialLine1 As TrendLine

'        Dim momentumBarsRVLines(1) As TrendLine
'        Dim momentumBarsOffsetLines(1) As TrendLine
'        Dim momentumBarsPlots(2) As Plot
'        Dim lastMomentumBar As TrendLine
'        Dim currentMomentumSwing As TrendLine

'        Private barsBackMark As TrendLine

'        Public Enum ChannelModeType
'            Merged
'            Unmerged
'            UnmergedOverlap
'        End Enum
'        'Private backupChannelLines As List(Of Channel)

'        Private ReadOnly Property CurrentSwing2 As Swing
'            Get
'                Return swings2(swings2.Count - 1)
'            End Get
'        End Property
'        Private ReadOnly Property CurrentSwing1 As Swing
'            Get
'                Return swings1(swings1.Count - 1)
'            End Get
'        End Property
'        'Private ReadOnly Property CurrentSwing1(barsBack As Integer) As Swing
'        '    Get
'        '        If swings2.Count > barsBack Then
'        '            Return swings2(swings2.Count - barsBack - 1)
'        '        Else
'        '            Return Nothing
'        '        End If
'        '    End Get
'        'End Property
'        Friend Overrides Sub OnCreate()
'            MyBase.OnCreate()
'            If Chart IsNot Nothing Then
'                If CustomRangeValue = -1 Then
'                    PriceRV = Chart.Settings("RangeValue").Value * DefaultAutoTrendRV
'                Else
'                    PriceRV = CustomRangeValue * DefaultAutoTrendRV
'                End If
'                'FineRVIncrementValue = Chart.GetMinTick
'                'CoarseRVIncrementValue = Chart.GetMinTick * 5
'            End If
'        End Sub
'        Public Overrides Sub Reset()
'            'If slider IsNot Nothing Then
'            '    originalLocation = New Point(slider.Left, slider.Top)
'            '    RemoveHandler CType(CType(slider.Content, Grid).Children(0), Slider).ValueChanged, AddressOf SliderMove
'            '    slider = Nothing
'            'End If
'            MyBase.Reset()
'            If barColorsDirty And Chart.bars.Count > 1 Then
'                BarColorRoutine(0, Chart.bars.Count - 1, Chart.Settings("Bar Color").Value)
'                barColorsDirty = False
'            End If
'            'Chart.dontDrawBarVisuals = False
'        End Sub
'        Private Sub ProcessButtonClick(s As Object, e As EventArgs)

'        End Sub

'        Function FormatNumberLengthAndPrefix(num As Decimal, placesAfterDecimal As Integer) As String
'            'Return RemovePrefixZero(FormatNumber(num, placesAfterDecimal))
'            Return Round(num * (10 ^ placesAfterDecimal))
'        End Function

'        Protected Overrides Sub Begin()
'            MyBase.Begin()

'            processButtonUI = New UIChartControl
'            processButtonUI.AnalysisTechnique = Me
'            Dim backgroundGrid As New Grid
'            backgroundGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})
'            backgroundGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(6, GridUnitType.Pixel)})
'            contentButton = New Button
'            AddHandler contentButton.Click, AddressOf ProcessButtonClick
'            Dim gridBackground As New Grid
'            gridBackground.Children.Add(New TextBlock With {.HorizontalAlignment = HorizontalAlignment.Center, .VerticalAlignment = VerticalAlignment.Center, .Text = "..."})
'            contentButton.Content = ".."
'            Grid.SetColumn(contentButton, 0)
'            backgroundGrid.Children.Add(contentButton)
'            Dim draggingBlock As New TextBlock
'            Grid.SetColumn(draggingBlock, 1)
'            draggingBlock.Background = Brushes.LightGray
'            backgroundGrid.Children.Add(draggingBlock)
'            processButtonUI.Content = backgroundGrid
'            processButtonUI.Width = 28
'            processButtonUI.Height = 22
'            processButtonUI.Left = originalLocation2.X
'            processButtonUI.Top = originalLocation2.Y
'            processButtonUI.IsDraggable = True
'            'AddObjectToChart(processButtonUI)


'            'Chart.dontDrawBarVisuals = True


'            'For i = 1 To 5
'            '    aboveCurvedLinePlots(i) = NewPlot(CurvedLineColor)
'            '    aboveCurvedLinePlots(i).IsSelectable = False
'            '    aboveCurvedLinePlots(i).Pen.Thickness = CurvedLineThickness
'            '    belowCurvedLinePlots(i) = NewPlot(CurvedLineColor)
'            '    belowCurvedLinePlots(i).IsSelectable = False
'            '    belowCurvedLinePlots(i).Pen.Thickness = CurvedLineThickness
'            'Next
'            rvTopLinePlot = NewPlot(RVLineColor)
'            rvTopLinePlot.IsSelectable = False
'            rvBotLinePlot = NewPlot(RVLineColor)
'            rvBotLinePlot.IsSelectable = False
'            rvTopLinePlot.Pen.Thickness = TrendChannelThickness
'            rvBotLinePlot.Pen.Thickness = TrendChannelThickness

'            curvedTopLinePlot = NewPlot(CurvedLineColor)
'            curvedTopLinePlot.IsSelectable = False
'            curvedBotLinePlot = NewPlot(CurvedLineColor)
'            curvedBotLinePlot.IsSelectable = False
'            curvedTopLinePlot.Pen.Thickness = CurvedLineThickness
'            curvedBotLinePlot.Pen.Thickness = CurvedLineThickness
'            'aboveCurvedLinePlots(1).Pen = New Pen(New SolidColorBrush(FillLineUpColor4), CurvedLineThickness)
'            'aboveCurvedLinePlots(2).Pen = New Pen(New SolidColorBrush(FillLineUpColor4), CurvedLineThickness)
'            'aboveCurvedLinePlots(3).Pen = New Pen(New SolidColorBrush(FillLineUpColor4), RVLineThickness)
'            'belowCurvedLinePlots(1).Pen = New Pen(New SolidColorBrush(FillLineDownColor4), CurvedLineThickness)
'            'belowCurvedLinePlots(2).Pen = New Pen(New SolidColorBrush(FillLineDownColor4), CurvedLineThickness)
'            'belowCurvedLinePlots(3).Pen = New Pen(New SolidColorBrush(FillLineDownColor4), RVLineThickness)

'            'aboveFillPlot(1) = NewPlot(FillLineUpColor1)
'            'belowFillPlot(1) = NewPlot(FillLineDownColor1)
'            'aboveFillPlot(2) = NewPlot(FillLineUpColor2)
'            'belowFillPlot(2) = NewPlot(FillLineDownColor2)
'            'aboveFillPlot(3) = NewPlot(FillLineUpColor3)
'            'belowFillPlot(3) = NewPlot(FillLineDownColor3)
'            'aboveFillPlot(4) = NewPlot(FillLineUpColor4)
'            'belowFillPlot(4) = NewPlot(FillLineDownColor4)

'            'For i = 1 To 4
'            '    aboveFillPlot(i).IsSelectable = False
'            '    aboveFillPlot(i).Pen.Thickness = FillLineThickness
'            '    belowFillPlot(i).IsSelectable = False
'            '    belowFillPlot(i).Pen.Thickness = FillLineThickness
'            'Next



'            regressionChannels = New List(Of RegressionChannel)

'            timeLabel = NewLabel(Now.ToShortTimeString, SwingCountTextColor, Chart.GetRelativeFromReal(SwingCountTextLocation), False, , False)
'            timeLabel.UseNegativeCoordinates = True
'            timeLabel.Font.FontSize = 14
'            dayHighPrice = Decimal.MinValue
'            dayLowPrice = Decimal.MaxValue
'            currentRegressionCenterPoint = CurrentBar.Close
'            coloringChannels = New List(Of Channel)
'            swings2 = New List(Of Swing)
'            swings2.Add(New Swing(NewTrendLine(Colors.Red,
'                      New Point(CurrentBar.Number, CurrentBar.Close),
'                      New Point(CurrentBar.Number, CurrentBar.Close),
'                      SwingLinesOn2), Direction.Down))
'            swings1 = New List(Of Swing)
'            swings1.Add(New Swing(NewTrendLine(Colors.Pink,
'                      New Point(CurrentBar.Number, CurrentBar.Close),
'                      New Point(CurrentBar.Number, CurrentBar.Close),
'                      SwingLinesOn1), Direction.Down))
'            CurrentSwing2.TL.Pen.Thickness = SwingLineThickness
'            CurrentSwing2.TL.IsSelectable = False
'            CurrentSwing2.TL.IsEditable = False


'            ' potential line initialization
'            lastSwingPotentialLine2 = NewTrendLine(Colors.Red, True)
'            lastSwingPotentialLine2.IsRegressionLine = True
'            lastSwingPotentialLine2.OuterPen = New Pen(New SolidColorBrush(Colors.Red), NeutralChannel2Thickness)
'            lastSwingPotentialLine2.Pen = New Pen(New SolidColorBrush(Colors.Red), 0)
'            lastSwingPotentialLine2.LockToEnd = True
'            lastSwingPotentialLine2.HasParallel = True
'            lastSwingPotentialLine2.IsSelectable = False
'            lastSwingPotentialLine2.IsEditable = False
'            lastSwingPotentialLine2.ExtendRight = True

'            lastSwingCounterPotentialLine2 = NewTrendLine(Colors.Red, True)
'            lastSwingCounterPotentialLine2.IsRegressionLine = True
'            lastSwingCounterPotentialLine2.OuterPen = New Pen(New SolidColorBrush(Colors.Red), NeutralChannel2Thickness)
'            lastSwingCounterPotentialLine2.Pen = New Pen(New SolidColorBrush(Colors.Red), 0)
'            lastSwingCounterPotentialLine2.LockToEnd = True
'            lastSwingCounterPotentialLine2.HasParallel = True
'            lastSwingCounterPotentialLine2.IsSelectable = False
'            lastSwingCounterPotentialLine2.IsEditable = False
'            lastSwingCounterPotentialLine2.ExtendRight = True

'            lastSwingPotentialLine1 = NewTrendLine(Colors.Red, True)
'            lastSwingPotentialLine1.IsRegressionLine = True
'            lastSwingPotentialLine1.OuterPen = New Pen(New SolidColorBrush(Colors.Red), NeutralChannel1Thickness)
'            lastSwingPotentialLine1.Pen = New Pen(New SolidColorBrush(Colors.Red), 0)
'            lastSwingPotentialLine1.LockToEnd = True
'            lastSwingPotentialLine1.HasParallel = True
'            lastSwingPotentialLine1.IsSelectable = False
'            lastSwingPotentialLine1.IsEditable = False
'            lastSwingPotentialLine1.ExtendRight = True

'            lastSwingCounterPotentialLine1 = NewTrendLine(Colors.Red, True)
'            lastSwingCounterPotentialLine1.IsRegressionLine = True
'            lastSwingCounterPotentialLine1.OuterPen = New Pen(New SolidColorBrush(Colors.Red), NeutralChannel1Thickness)
'            lastSwingCounterPotentialLine1.Pen = New Pen(New SolidColorBrush(Colors.Red), 0)
'            lastSwingCounterPotentialLine1.LockToEnd = True
'            lastSwingCounterPotentialLine1.HasParallel = True
'            lastSwingCounterPotentialLine1.IsSelectable = False
'            lastSwingCounterPotentialLine1.IsEditable = False
'            lastSwingCounterPotentialLine1.ExtendRight = True
'            ' end potential line initialization


'            currentSwingLengthTexts1 = New List(Of Label)
'            currentSwingLengthTexts1.Add(NewLabel(GetDollarValue(Abs(swings1(swings1.Count - 1).StartPrice - swings1(swings1.Count - 1).EndPrice)), Colors.Black, New Point(swings1(swings1.Count - 1).EndBar, swings1(swings1.Count - 1).EndPrice), SwingTextsOn, , False))
'            currentSwingLengthTexts1(currentSwingLengthTexts1.Count - 1).Font.FontSize = LengthTextFontSize2
'            currentSwingLengthTexts1(currentSwingLengthTexts1.Count - 1).Font.FontWeight = LengthTextFontWeight2
'            currentSwingLengthTexts1(currentSwingLengthTexts1.Count - 1).IsSelectable = False
'            currentSwingLengthTexts1(currentSwingLengthTexts1.Count - 1).IsEditable = False

'            currentSwingLengthTexts2 = New List(Of Label)
'            currentSwingLengthTexts2.Add(NewLabel(GetDollarValue(Abs(swings2(swings2.Count - 1).StartPrice - swings2(swings2.Count - 1).EndPrice)), Colors.Black, New Point(swings2(swings2.Count - 1).EndBar, swings2(swings2.Count - 1).EndPrice), SwingTextsOn, , False))
'            currentSwingLengthTexts2(currentSwingLengthTexts2.Count - 1).Font.FontSize = LengthTextFontSize2
'            currentSwingLengthTexts2(currentSwingLengthTexts2.Count - 1).Font.FontWeight = LengthTextFontWeight2
'            currentSwingLengthTexts2(currentSwingLengthTexts2.Count - 1).IsSelectable = False
'            currentSwingLengthTexts2(currentSwingLengthTexts2.Count - 1).IsEditable = False

'            regressionCenterPoints = New Dictionary(Of Integer, Decimal)

'            currentChannelEndSwingIndex = 0
'            currentChannelBeginSwingIndex = 0
'            currentChannelUnmerged = Nothing

'            extendTrendTextObject = NewLabel(" Ext Trend", Colors.Black, New Point(0, 0), False, , False)
'            newTrendTextObject = NewLabel(" New Trend", ColorConverter.ConvertFromString("#FF00CCFF"), New Point(0, 0), True, , False)
'            extendTrendTextObject.Font.FontSize = TrendTextFontSize
'            extendTrendTextObject.Font.FontWeight = TrendTextFontWeight
'            newTrendTextObject.Font.FontSize = TrendTextFontSize
'            newTrendTextObject.Font.FontWeight = TrendTextFontWeight

'            extendTrendLineObject = NewTrendLine(Colors.Black)
'            newTrendLineObject = NewTrendLine(Colors.Black)
'            extendTrendLineObject.Pen.Thickness = TrendIndicationLineWidth
'            newTrendLineObject.Pen.Thickness = TrendIndicationLineWidth
'            newTrendLineObject.IsSelectable = False
'            newTrendLineObject.IsEditable = False
'            extendTrendLineObject.IsSelectable = False
'            extendTrendLineObject.IsEditable = False

'            extendSwingTextObject2 = NewLabel(rvTextPrefix2 & " Ext Trend", Colors.Black, New Point(0, 0), True, , False)
'            extendSwingTextObject2.Font.FontSize = RVAndTargetTextFontSize2
'            extendSwingTextObject2.Font.FontWeight = RVAndTargetTextFontWeight2

'            newSwingTextObject2 = NewLabel(newRVTextPrefix2 & FormatNumberLengthAndPrefix(PriceRV, Chart.Settings("DecimalPlaces").Value) & " RV ", Colors.Black, New Point(0, 0), True, , False)
'            newSwingTextObject2.Font.FontSize = RVAndTargetTextFontSize2
'            newSwingTextObject2.Font.FontWeight = RVAndTargetTextFontWeight2

'            newSwingTextObject1 = NewLabel(newRVTextPrefix1 & FormatNumberLengthAndPrefix(RV1, Chart.Settings("DecimalPlaces").Value) & " RV ", SwingLineColor, New Point(0, 0), True, , False)
'            newSwingTextObject1.Font.FontSize = RVAndTargetTextFontSize1
'            newSwingTextObject1.Font.FontWeight = RVAndTargetTextFontWeight1
'            extendSwingTextObject1 = NewLabel(rvTextPrefix1 & " Ext Sw1", SwingLineColor, New Point(0, 0), False, , False)
'            extendSwingTextObject1.Font.FontSize = RVAndTargetTextFontSize1
'            extendSwingTextObject1.Font.FontWeight = RVAndTargetTextFontWeight1

'            'rvText = NewLabel("", RVLineColor, New Point(0, 0))
'            'rvText.Font.FontSize = RVTextFontSize
'            'rvText.Font.FontWeight = RVTextFontWeight
'            'rvText.VerticalAlignment = LabelVerticalAlignment.Center
'            'rvText.HorizontalAlignment = LabelHorizontalAlignment.Left
'            rvProNewLine = NewTrendLine(ProjectionLineColor, New Point(0, 0), New Point(0, 0), ProjectionLinesOn)
'            rvProNewLine.Pen.Thickness = ProjectionLineThickness
'            rvProNewLine.Pen.DashStyle = TrendLineDashStyle
'            rvProNewLine.IsSelectable = False
'            rvProNewLine.IsEditable = False

'            barsBackMark = NewTrendLine(RVLineColor)
'            barsBackMark.Pen.Thickness = TrendChannelThickness
'            barsBackMark.IsSelectable = False
'            barsBackMark.IsEditable = False

'            currentSwingBCTargetText = NewLabel("", Colors.Black, New Point(0, 0), True, , False)
'            currentSwingBCTargetText.HorizontalAlignment = LabelHorizontalAlignment.Left
'            currentSwingBCTargetText.VerticalAlignment = LabelVerticalAlignment.Center
'            currentSwingBCTargetText.Font.FontSize = RVAndTargetTextFontSize2
'            currentSwingBCTargetText.Font.FontWeight = RVAndTargetTextFontWeight2
'            'currentSwingBCTargetText.Font.Brush = New SolidColorBrush(SwingChannelBCLengthTextColor)
'            currentSwingPotentialBCTargetText = NewLabel("", Colors.Black, New Point(0, 0), True, , False)
'            currentSwingPotentialBCTargetText.HorizontalAlignment = LabelHorizontalAlignment.Right
'            currentSwingPotentialBCTargetText.VerticalAlignment = LabelVerticalAlignment.Center
'            currentSwingPotentialBCTargetText.Font.FontSize = RVAndTargetTextFontSize2
'            currentSwingPotentialBCTargetText.Font.FontWeight = RVAndTargetTextFontWeight2
'            'currentSwingPotentialBCTargetText.Font.Brush = New SolidColorBrush(SwingChannelBCLengthTextColor)
'            currentPartiallyCutChannel = Nothing

'            If CustomRangeValue = -1 Then
'                swingCountGraphYPlacement = CurrentBar.Close + Chart.Settings("RangeValue").Value * 20
'            Else
'                swingCountGraphYPlacement = CurrentBar.Close + CustomRangeValue * 20
'            End If
'            currentSwingCountGraphLine = NewTrendLine(UpTrendChannelColor, New Point(CurrentBar.Number, swingCountGraphYPlacement), New Point(CurrentBar.Number, swingCountGraphYPlacement), False)
'            currentSwingCountGraphLine.Pen.Thickness = 10
'            currentSwingCountGraphLine.IsSelectable = False
'            currentSwingCountGraphLine.IsEditable = False
'            potentialRegressionChannel = Nothing
'            potentialSameDirRegressionChannel = Nothing

'            curSwingEvent2 = SwingEvent.None

'            'regressionChannelLines = {NewTrendLine(RegressionChannelLineColor, New Point(0, 0), New Point(0, 0), ShowRegressionChannelLine), NewTrendLine(RegressionChannelLineColor, New Point(0, 0), New Point(0, 0), ShowRegressionChannelLine), NewTrendLine(RegressionChannelLineColor, New Point(0, 0), New Point(0, 0), ShowRegressionChannelLine)}
'            'For j = 0 To 2
'            '    regressionChannelLines(j).Pen = New Pen(New SolidColorBrush(RegressionChannelLineColor), RegressionChannelLineWidth)
'            '    regressionChannelLines(j).ExtendRight = True
'            'Next
'            'currentLine(2).Pen = New Pen(New SolidColorBrush(RegressionLineColor), CenterLineThickness)


'            'centerPlot = NewPlot(CurvedLineColor)
'            'centerPlot.IsSelectable = False
'            'centerPlot.Pen = New Pen(New SolidColorBrush(CurvedLineColor), CenterLineThickness)

'            'currentLine = {NewTrendLine(CenterLineColor, New Point(0, 0), New Point(0, 0), ShowOuterChannelLines), NewTrendLine(CenterLineColor, New Point(0, 0), New Point(0, 0), ShowOuterChannelLines), NewTrendLine(CenterLineColor, New Point(0, 0), New Point(0, 0))}
'            'For j = 0 To 2
'            '    currentLine(j).Pen = New Pen(New SolidColorBrush(OuterLineColor), OuterLineThickness)
'            '    currentLine(j).ExtendRight = True
'            '    currentLine(j).IsSelectable = False
'            '    currentLine(j).IsEditable = False
'            'Next
'            'currentLine(2).Pen = New Pen(New SolidColorBrush(ChannelCenterLineColor), CenterLineThickness)
'            'For j = 0 To 3
'            '    boxLines(j) = NewTrendLine(New Pen(New SolidColorBrush(BoxColor), BoxThickness), ShowBox)
'            '    boxLines(j).IsSelectable = False
'            '    boxLines(j).IsEditable = False
'            'Next
'            'OffsetValueMultiplier2 = -(5 * ShortLengthMultiplier * (RegressionLength - 150) - 5 * WideLengthMultiplier * (RegressionLength - 30) - 12 * (RegressionLength + 140)) / 600

'            regParams = Nothing
'            regParams2 = Nothing

'            For i = 0 To 1
'                momentumBarsOffsetLines(i) = NewTrendLine(MomentumOffsetLineColor, MomentumBarsVisible)
'                momentumBarsOffsetLines(i).Pen = New Pen(New SolidColorBrush(MomentumOffsetLineColor), MomentumOffsetLineThickness)
'                momentumBarsRVLines(i) = NewTrendLine(RVLineColor, MomentumBarsVisible)
'                momentumBarsRVLines(i).Pen = New Pen(New SolidColorBrush(RVLineColor), TrendChannelThickness)
'            Next
'            If Chart.bars.Count > 0 Then
'                If VerticalMomentumBarsPosition = 0 Then VerticalMomentumBarsPosition = Chart.bars(Chart.bars.Count - 1).Data.Close
'                momentumBarsRVLines(0).StartPoint = New Point(Chart.bars.Count - NoOfMomentumBars, VerticalMomentumBarsPosition + Abs(CurvedLineMultiplier * Chart.Settings("RangeValue").Value))
'                momentumBarsRVLines(1).StartPoint = New Point(Chart.bars.Count - NoOfMomentumBars, VerticalMomentumBarsPosition - Abs(CurvedLineMultiplier * Chart.Settings("RangeValue").Value))
'                momentumBarsOffsetLines(0).StartPoint = New Point(Chart.bars.Count - NoOfMomentumBars, VerticalMomentumBarsPosition + Abs(MomentumOffsetLineMultiplier * Chart.Settings("RangeValue").Value))
'                momentumBarsOffsetLines(1).StartPoint = New Point(Chart.bars.Count - NoOfMomentumBars, VerticalMomentumBarsPosition - Abs(MomentumOffsetLineMultiplier * Chart.Settings("RangeValue").Value))
'            End If
'            momentumBarsPlots(0) = NewPlot(UpColor)
'            momentumBarsPlots(1) = NewPlot(NeutralColor)
'            momentumBarsPlots(2) = NewPlot(DownColor)
'            lastMomentumBar = NewTrendLine(NeutralColor)
'            For i = 0 To 1
'                momentumBarsPlots(i).IsSelectable = False
'                momentumBarsPlots(i).IsEditable = False
'                momentumBarsRVLines(i).IsSelectable = False
'                momentumBarsRVLines(i).IsEditable = False
'                momentumBarsOffsetLines(i).IsSelectable = False
'                momentumBarsOffsetLines(i).IsEditable = False
'            Next
'            momentumBarsPlots(2).IsSelectable = False
'            momentumBarsPlots(2).IsEditable = False
'            currentMomentumSwing = Nothing
'        End Sub

'        Dim curSwingEvent2 As SwingEvent = SwingEvent.None
'        Dim swingDir2 As Direction
'        Dim curSwingEvent1 As SwingEvent = SwingEvent.None
'        Dim swingDir1 As Direction
'        Dim currentSwingLengthTexts1 As List(Of Label)
'        Dim currentSwingLengthTexts2 As List(Of Label)
'        Dim currentChannelEndSwingIndex As Integer
'        Dim currentChannelBeginSwingIndex As Integer
'        Dim currentChannelUnmerged As Channel


'        Private Function MaxOrMinOfBarsBack(barsback As Integer, isMax As Boolean) As Point
'            Dim rangePoint As New Point(0, If(isMax, 0, Integer.MaxValue))
'            For i = CurrentBar.Number - 1 To Max(CurrentBar.Number - 1 - barsback, 0) Step -1
'                If isMax And Chart.bars(i).Data.High > rangePoint.Y Then
'                    rangePoint = New Point(i, Chart.bars(i).Data.High)
'                ElseIf Not isMax And Chart.bars(i).Data.Low < rangePoint.Y Then
'                    rangePoint = New Point(i, Chart.bars(i).Data.Low)
'                End If
'            Next
'            Return rangePoint
'        End Function
'        Protected Overrides Sub Main()

'            If CurrentBar.Number <= Chart.bars.Count - BarProcessCount And BarProcessCount <> 0 Then Exit Sub
'            If CurrentBar.Number < 2 Then Exit Sub
'            If IsLastBarOnChart Then
'                timeLabel.Location = Chart.GetRelativeFromReal(SwingCountTextLocation)
'                timeLabel.Text = swings2.Count
'            End If
'            'If CurrentBar.Number = Round(Chart.Bounds.TopLeft.X) - 100 Then
'            '    Reset()
'            '    Begin()
'            'End If
'            DrawVisuals = CurrentBar.Number > Round(Chart.Bounds.TopLeft.X) - DrawingCutoff

'            If Not regressionCenterPoints.ContainsKey(CurrentBar.Number) Then
'                If CurrentBar.Number > Length Then
'                    regParams2 = CalculateNextRegressionFromParams(CurrentBar.Number - Length - 1, CurrentBar.Number - 1, CurrentBar.Number - Length, CurrentBar.Number, regParams2)
'                    currentRegressionCenterPoint = regParams2.LineCoordinates.EndPoint.Y
'                Else
'                    currentRegressionCenterPoint = CurrentBar.Close
'                End If
'                If regressionCenterPoints.ContainsKey(CurrentBar.Number) Then regressionCenterPoints(CurrentBar.Number) = currentRegressionCenterPoint Else regressionCenterPoints.Add(CurrentBar.Number, currentRegressionCenterPoint)
'            End If




'            Dim cs2 As Swing = CurrentSwing2
'            Dim cs1 As Swing = CurrentSwing1
'            curSwingEvent2 = SwingEvent.None
'            'barsBackMark.Coordinates = New LineCoordinates(CurrentBar.Number - Max(CurrentBar.Number - cs.EndBar, RVBarsBack), rvPnt.Y, CurrentBar.Number, rvPnt.Y)
'            If (cs2.Direction = Direction.Down And Round(CurrentBar.High, 5) >= Round(cs2.EndPoint.Y + PriceRV, 5)) OrElse
'                (cs2.Direction = Direction.Up And Round(CurrentBar.Low, 5) <= Round(cs2.EndPoint.Y - PriceRV, 5)) Then
'                'new swing
'                'Dim canContinue As Boolean = True
'                'For i = cs.EndBar To CurrentBar.Number - 2
'                '    If (cs.Direction = Direction.Down And Chart.bars(i).Data.High > CurrentBar.High) Or (cs.Direction = Direction.Up And Chart.bars(i).Data.Low < CurrentBar.Low) Then
'                '        canContinue = False
'                '        Exit For
'                '    End If
'                'Next
'                'If canContinue Then
'                'If(ColorSwingBySwing, If(cs2.Direction = Direction.Down, UpTrendSwingLineColor, DownTrendSwingLineColor), If(cs2.Direction = Direction.Down, UpSwingChannelColor, DownSwingChannelColor))
'                swings2.Add(NewSwing(Colors.Pink, New Point(cs2.EndBar, cs2.EndPrice),
'                                    New Point(CurrentBar.Number, If(cs2.Direction = Direction.Down, CurrentBar.High, CurrentBar.Low)), True And SwingLinesOn2, Not cs2.Direction))
'                If swings2.Count > 1 And SwingLinesOn2 Then
'                    If LastSwingLineOn Then
'                        swings2(swings2.Count - 1).TL.Pen.Thickness = SwingLineThickness
'                        swings2(swings2.Count - 2).TL.Pen.Thickness = SwingLineThickness
'                    Else
'                        swings2(swings2.Count - 1).TL.Pen.Thickness = 0
'                        swings2(swings2.Count - 2).TL.Pen.Thickness = SwingLineThickness
'                    End If
'                End If
'                swings2(swings2.Count - 1).TL.IsSelectable = False
'                swings2(swings2.Count - 1).TL.IsEditable = False
'                curSwingEvent2 = AutoTrendBase.SwingEvent.NewSwing
'                swingDir2 = Not cs2.Direction

'                currentSwingLengthTexts2.Add(NewLabel("", If(swingDir2 = Direction.Up, UpTrendChannelColor, DownTrendChannelColor), New Point(swings2(swings2.Count - 1).EndBar + horizSwingTextAlignment, swings2(swings2.Count - 1).EndPrice + If(swingDir2 = Direction.Up, 1, -1) * Chart.GetRelativeFromRealHeight(vertSwingTextAlignment + 4)), SwingTextsOn, , True))
'                If swings2.Count > 2 Then
'                    currentSwingLengthTexts2(currentSwingLengthTexts2.Count - 1).Text = FormatNumber(Abs(swings2(swings2.Count - 1).StartPrice - swings2(swings2.Count - 1).EndPrice) / Abs(swings2(swings2.Count - 2).StartPrice - swings2(swings2.Count - 2).EndPrice) * 100, 0) & "% "
'                Else
'                    currentSwingLengthTexts2(currentSwingLengthTexts2.Count - 1).Text = ""
'                End If
'                If ShowPercentPriceChangeOnSwing2 Then currentSwingLengthTexts2(currentSwingLengthTexts2.Count - 1).Text &= RemovePrefixZero(FormatNumber(Abs(swings2(swings2.Count - 1).StartPrice - swings2(swings2.Count - 1).EndPrice) / swings2(swings2.Count - 1).StartPrice * 100, 2) & "% " & "         ")
'                'currentSwingLengthTexts2(currentSwingLengthTexts2.Count - 1).Text &= FormatNumberLengthAndPrefix(GetDollarValue(Abs(swings2(swings2.Count - 1).StartPrice - swings2(swings2.Count - 1).EndPrice)), Chart.Settings("DecimalPlaces").Value) & "         "
'                currentSwingLengthTexts2(currentSwingLengthTexts2.Count - 1).Tag = FormatNumberLengthAndPrefix(GetDollarValue(Abs(swings2(swings2.Count - 1).StartPrice - swings2(swings2.Count - 1).EndPrice)), Chart.Settings("DecimalPlaces").Value)
'                currentSwingLengthTexts2(currentSwingLengthTexts2.Count - 1).Font.FontSize = LengthTextFontSize2
'                currentSwingLengthTexts2(currentSwingLengthTexts2.Count - 1).Font.FontWeight = LengthTextFontWeight2
'                currentSwingLengthTexts2(currentSwingLengthTexts2.Count - 1).HorizontalAlignment = LabelHorizontalAlignment.Center
'                currentSwingLengthTexts2(currentSwingLengthTexts2.Count - 1).VerticalAlignment = LabelVerticalAlignment.Center
'                AddHandler currentSwingLengthTexts2(currentSwingLengthTexts2.Count - 1).MouseDownEvent,
'                Sub(sender As Object)
'                    ApplyRV(CDec(CType(sender, Label).Tag) / 10 ^ Chart.Settings("DecimalPlaces").Value + Chart.GetMinTick)
'                End Sub
'                swings2(swings2.Count - 1).LengthText = currentSwingLengthTexts2(currentSwingLengthTexts2.Count - 1)

'                If SwingChannelOn2 Then
'                    With swings2(swings2.Count - 1)
'                        .SwingChannel = NewTrendLine(Colors.Black, .StartPoint, .EndPoint, True)
'                        .SwingChannel.IsRegressionLine = True
'                        .SwingChannel.HasParallel = True
'                        .SwingChannel.Pen = New Pen(Brushes.Black, 0)
'                        .SwingChannel.OuterPen = New Pen(New SolidColorBrush(If(swingDir2 = Direction.Up, UpTrendChannelColor, DownTrendChannelColor)), TrendChannelThickness)
'                        .SwingChannel.ExtendRight = True
'                        .SwingChannel.IsSelectable = False
'                        .SwingChannel.IsEditable = False
'                        .ABCLengths = New List(Of ABCCombo)
'                    End With
'                    If swings2.Count > 1 AndAlso swings2(swings2.Count - 2).SwingChannel IsNot Nothing Then
'                        With swings2(swings2.Count - 2)
'                            .SwingChannel.ExtendRight = False
'                            If SwingChannelHistoryOn Then
'                            Else
'                                'RemoveObjectFromChart(.SwingChannel)
'                            End If
'                        End With
'                    End If
'                End If
'                If SwingChannelLengthAndTargetTextsOn Then
'                    swings2(swings2.Count - 1).ABCLengths = New List(Of ABCCombo)
'                End If
'                lastSwingPotentialLine2.StartPoint = CurrentSwing2.StartPoint
'                lastSwingPotentialLine2.EndPoint = CurrentSwing2.EndPoint
'                lastSwingPotentialLine2.OuterPen.Brush = New SolidColorBrush(If(lastSwingPotentialLine2.EndPoint.Y > lastSwingPotentialLine2.StartPoint.Y, NeutralChannelUp2Color, NeutralChannelDown2Color))
'                'begin backfill swing1 and remove history
'                If SwingChannelHistoryOn = False Then
'                    For Each sw In swings1
'                        If sw.EndBar <= CurrentSwing2.StartBar Then
'                            RemoveObjectFromChart(sw.TL)
'                            RemoveObjectFromChart(sw.LengthText)
'                            'RemoveObjectFromChart(sw.SwingChannel)
'                        End If
'                    Next
'                End If
'                Dim rangePoint As Double = CurrentSwing2.StartPrice
'                If CurrentSwing1.Direction = CurrentSwing2.Direction Then
'                    RemoveObjectFromChart(CurrentSwing1.SwingChannel)
'                    RemoveObjectFromChart(CurrentSwing1.TL)
'                    RemoveObjectFromChart(currentSwingLengthTexts1(currentSwingLengthTexts1.Count - 1))
'                    swings1.RemoveAt(swings1.Count - 1)
'                    currentSwingLengthTexts1.RemoveAt(currentSwingLengthTexts1.Count - 1)

'                End If
'                ''back fill swing 1 on swing 2 creation
'                'For i = Min(CurrentSwing2.StartBar + 1, Chart.bars.Count - 1) To Min(CurrentSwing2.EndBar, Chart.bars.Count - 1)
'                '    Dim swngevent2 As Boolean = False
'                '    If (CurrentSwing2.Direction = Direction.Up And Chart.bars(i).Data.High >= rangePoint) Or
'                '           (CurrentSwing2.Direction = Direction.Down And Chart.bars(i).Data.Low <= rangePoint) Then
'                '        rangePoint = If(CurrentSwing2.Direction = Direction.Up, Chart.bars(i).Data.High, Chart.bars(i).Data.Low)
'                '        swngevent2 = True
'                '    End If

'                '    If ((CurrentSwing1.Direction = CurrentSwing2.Direction And
'                '                             ((CurrentSwing1.Direction = Direction.Down And Abs(Chart.bars(i).Data.High - CurrentSwing1.EndPoint.Y) > RV1 / 100) OrElse
'                '                             (CurrentSwing1.Direction = Direction.Up And Abs(Chart.bars(i).Data.Low - CurrentSwing1.EndPoint.Y) > RV1 / 100))) Or
'                '                             (swngevent2 And CurrentSwing1.Direction <> CurrentSwing2.Direction)) Then

'                '        swings1.Add(NewSwing(SwingLineColor, New Point(CurrentSwing1.EndBar, CurrentSwing1.EndPrice),
'                '                                    New Point(i, If(CurrentSwing1.Direction = Direction.Down, Chart.bars(i).Data.High, Chart.bars(i).Data.Low)), SwingLinesOn1, Not CurrentSwing1.Direction))
'                '        If swings1.Count > 1 And SwingLinesOn1 Then
'                '            If Not LastSwingIsChannel1 Then
'                '                swings1(swings1.Count - 1).TL.Pen.Thickness = SwingLineThickness
'                '                swings1(swings1.Count - 2).TL.Pen.Thickness = SwingLineThickness
'                '            Else
'                '                swings1(swings1.Count - 1).TL.Pen.Thickness = 0
'                '                swings1(swings1.Count - 2).TL.Pen.Thickness = SwingLineThickness
'                '            End If
'                '        End If
'                '        swings1(swings1.Count - 1).TL.IsSelectable = False
'                '        swings1(swings1.Count - 1).TL.IsEditable = False
'                '        curSwingEvent1 = AutoTrendBase.SwingEvent.NewSwing
'                '        swingDir1 = CurrentSwing1.Direction

'                '        currentSwingLengthTexts1.Add(NewLabel("", SwingLineColor, New Point(swings1(swings1.Count - 1).EndBar + horizSwingTextAlignment, swings1(swings1.Count - 1).EndPrice + If(swingDir1 = Direction.Up, 1, -1) * Chart.GetRelativeFromRealHeight(vertSwingTextAlignment)), SwingTextsOn, , True))
'                '        If (swings1.Count > 0 And swings2.Count > 0) AndAlso CurrentSwing1.Direction <> CurrentSwing2.Direction Then
'                '            currentSwingLengthTexts1(currentSwingLengthTexts1.Count - 1).Text = FormatNumber(Abs(CurrentSwing1.StartPoint.Y - CurrentSwing1.EndPoint.Y) / Abs(rangePoint - CurrentSwing2.StartPrice) * 100, 0) & "%"
'                '            currentSwingLengthTexts1(currentSwingLengthTexts1.Count - 1).Tag = FormatNumber(Abs(CurrentSwing1.StartPoint.Y - CurrentSwing1.EndPoint.Y) / Abs(rangePoint - CurrentSwing2.StartPrice) * 100, 0)
'                '        Else
'                '            currentSwingLengthTexts1(currentSwingLengthTexts1.Count - 1).Text = ""
'                '        End If
'                '        currentSwingLengthTexts1(currentSwingLengthTexts1.Count - 1).Font.FontSize = LengthTextFontSize1
'                '        currentSwingLengthTexts1(currentSwingLengthTexts1.Count - 1).Font.FontWeight = LengthTextFontWeight1
'                '        currentSwingLengthTexts1(currentSwingLengthTexts1.Count - 1).HorizontalAlignment = LabelHorizontalAlignment.Center
'                '        currentSwingLengthTexts1(currentSwingLengthTexts1.Count - 1).VerticalAlignment = LabelVerticalAlignment.Center
'                '        swings1(swings1.Count - 1).LengthText = currentSwingLengthTexts1(currentSwingLengthTexts1.Count - 1)
'                '        AddHandler currentSwingLengthTexts1(currentSwingLengthTexts1.Count - 1).MouseDownEvent,
'                '                Sub(sender As Object)
'                '                    ApplyRV1(CDec(CType(sender, Label).Tag) + 1)
'                '                End Sub
'                '        swings1(swings1.Count - 1).LengthText = currentSwingLengthTexts1(currentSwingLengthTexts1.Count - 1)
'                '        If SwingChannelOn1 Or LastSwingIsChannel1 Then
'                '            With swings1(swings1.Count - 1)
'                '                .SwingChannel = NewTrendLine(Colors.Black, .StartPoint, .EndPoint, True)
'                '                .SwingChannel.IsRegressionLine = True
'                '                .SwingChannel.HasParallel = True
'                '                .SwingChannel.Pen = New Pen(Brushes.Black, 0)
'                '                .SwingChannel.OuterPen = New Pen(New SolidColorBrush(SwingLineColor), SwingLineThickness)
'                '                .SwingChannel.ExtendRight = True
'                '                .SwingChannel.IsSelectable = False
'                '                .SwingChannel.IsEditable = False
'                '                .BCLengths = New List(Of BCLength)
'                '            End With
'                '            If swings1.Count > 1 AndAlso swings1(swings1.Count - 2).SwingChannel IsNot Nothing Then
'                '                With swings1(swings1.Count - 2)
'                '                    If SwingChannelHistoryOn And Not LastSwingIsChannel1 Then
'                '                        .SwingChannel.OuterPen.Thickness = SwingChannelHistoryThickness
'                '                        .SwingChannel.ExtendRight = False
'                '                    Else
'                '                        RemoveObjectFromChart(.SwingChannel)
'                '                    End If
'                '                End With
'                '            End If
'                '        End If
'                '        If SwingChannelLengthAndTargetTextsOn Then
'                '            swings1(swings1.Count - 1).BCLengths = New List(Of BCLength)
'                '        End If
'                '    ElseIf (swings1.Count > 0 AndAlso Chart.bars(i).Data.High >= CurrentSwing1.EndPrice AndAlso CurrentSwing1.Direction = Direction.Up) OrElse (swings1.Count > 0 AndAlso Chart.bars(i).Data.Low <= CurrentSwing1.EndPrice AndAlso CurrentSwing1.Direction = Direction.Down) Then
'                '        'extension 
'                '        CurrentSwing1.EndBar = i
'                '        CurrentSwing1.EndPrice = If(CurrentSwing1.Direction = Direction.Up, Chart.bars(i).Data.High, Chart.bars(i).Data.Low)
'                '        curSwingEvent1 = AutoTrendBase.SwingEvent.Extension
'                '        swingDir1 = CurrentSwing1.Direction

'                '        currentSwingLengthTexts1(currentSwingLengthTexts1.Count - 1).Location = New Point(CurrentSwing1.EndBar + horizSwingTextAlignment, CurrentSwing1.EndPrice + If(swingDir1 = Direction.Up, 1, -1) * Chart.GetRelativeFromRealHeight(vertSwingTextAlignment))

'                '        If (swings1.Count > 0 And swings2.Count > 0) AndAlso CurrentSwing1.Direction <> CurrentSwing2.Direction Then
'                '            currentSwingLengthTexts1(currentSwingLengthTexts1.Count - 1).Text = FormatNumber(Abs(CurrentSwing1.StartPoint.Y - CurrentSwing1.EndPoint.Y) / Abs(rangePoint - CurrentSwing2.StartPrice) * 100, 0) & "%"
'                '            currentSwingLengthTexts1(currentSwingLengthTexts1.Count - 1).Tag = FormatNumber(Abs(CurrentSwing1.StartPoint.Y - CurrentSwing1.EndPoint.Y) / Abs(rangePoint - CurrentSwing2.StartPrice) * 100, 0)
'                '        Else
'                '            currentSwingLengthTexts1(currentSwingLengthTexts1.Count - 1).Text = ""
'                '        End If

'                '        If swings1(swings1.Count - 1).SwingChannel IsNot Nothing AndAlso (SwingChannelOn1 Or LastSwingIsChannel1) Then
'                '            swings1(swings1.Count - 1).SwingChannel.Coordinates = New LineCoordinates(CurrentSwing1.StartPoint, CurrentSwing1.EndPoint)
'                '        End If

'                '        If swings1(swings1.Count - 1).PushCountText IsNot Nothing Then
'                '            swings1(swings1.Count - 1).PushCountText.Location = AddToY(swings1(swings1.Count - 1).EndPoint, If(swings1(swings1.Count - 1).Direction = Direction.Up, 1, -1) * 0.6 * Chart.Settings("RangeValue").Value)
'                '        End If
'                '    End If
'                'Next
'                'end backfill swing1 and remove history
'            ElseIf (swings2.Count > 0 AndAlso CurrentBar.High >= cs2.EndPrice AndAlso cs2.Direction = Direction.Up) OrElse (swings2.Count > 0 AndAlso CurrentBar.Low <= cs2.EndPrice AndAlso cs2.Direction = Direction.Down) Then
'                'extension
'                cs2.EndBar = CurrentBar.Number
'                cs2.EndPrice = If(cs2.Direction = Direction.Up, CurrentBar.High, CurrentBar.Low)
'                curSwingEvent2 = AutoTrendBase.SwingEvent.Extension
'                swingDir2 = cs2.Direction

'                lastSwingCounterPotentialLine2.OuterPen.Thickness = 0
'                lastSwingCounterPotentialLine1.OuterPen.Thickness = 0

'                currentSwingLengthTexts2(currentSwingLengthTexts2.Count - 1).Location = New Point(cs2.EndBar + horizSwingTextAlignment,
'                                                                                                      cs2.EndPrice + If(swings1(swings1.Count - 1).Direction = Direction.Up, 1, -1) * Chart.GetRelativeFromRealHeight(vertSwingTextAlignment + 4))
'                If swings2.Count > 2 Then
'                    currentSwingLengthTexts2(currentSwingLengthTexts2.Count - 1).Text = FormatNumber(Abs(swings2(swings2.Count - 1).StartPrice - swings2(swings2.Count - 1).EndPrice) / Abs(swings2(swings2.Count - 2).StartPrice - swings2(swings2.Count - 2).EndPrice) * 100, 0) & "% "
'                Else
'                    currentSwingLengthTexts2(currentSwingLengthTexts2.Count - 1).Text = ""
'                End If
'                If ShowPercentPriceChangeOnSwing2 Then currentSwingLengthTexts2(currentSwingLengthTexts2.Count - 1).Text &= RemovePrefixZero(FormatNumber(Abs(swings2(swings2.Count - 1).StartPrice - swings2(swings2.Count - 1).EndPrice) / swings2(swings2.Count - 1).StartPrice * 100, 2) & "% " & "         ")
'                currentSwingLengthTexts2(currentSwingLengthTexts2.Count - 1).Tag = FormatNumberLengthAndPrefix(GetDollarValue(Abs(swings2(swings2.Count - 1).StartPrice - swings2(swings2.Count - 1).EndPrice)), Chart.Settings("DecimalPlaces").Value)
'                If swings2(swings2.Count - 1).SwingChannel IsNot Nothing AndAlso SwingChannelOn2 Then
'                    swings2(swings2.Count - 1).SwingChannel.Coordinates = New LineCoordinates(cs2.StartPoint, cs2.EndPoint)
'                End If

'                If swings2(swings2.Count - 1).PushCountText IsNot Nothing Then
'                    swings2(swings2.Count - 1).PushCountText.Location = AddToY(swings2(swings2.Count - 1).EndPoint, If(swings2(swings2.Count - 1).Direction = Direction.Up, 1, -1) * 0.6 * Chart.Settings("RangeValue").Value)
'                End If
'            End If

'            lastSwingPotentialLine1.StartPoint = CurrentSwing2.StartPoint
'            lastSwingPotentialLine1.EndPoint = FindRangeBar(CurrentSwing2.StartPoint.X, CurrentSwing2.EndPoint.X, If(CurrentSwing2.Direction = Direction.Up, True, False))
'            lastSwingPotentialLine1.OuterPen.Brush = New SolidColorBrush(If(lastSwingPotentialLine1.EndPoint.Y > lastSwingPotentialLine1.StartPoint.Y, NeutralChannelUp1Color, NeutralChannelDown1Color))
'            lastSwingPotentialLine1.LockToEnd = False

'            If Chart.bars.Count - CurrentSwing2.EndBar >= CounterTrendBarDelay Then
'                lastSwingCounterPotentialLine2.StartPoint = CurrentSwing2.EndPoint
'                lastSwingCounterPotentialLine2.EndPoint = New Point(CurrentBar.Number, CurrentBar.High)
'                lastSwingCounterPotentialLine2.OuterPen.Brush = New SolidColorBrush(If(lastSwingCounterPotentialLine2.EndPoint.Y > lastSwingCounterPotentialLine2.StartPoint.Y, NeutralChannelUp2Color, NeutralChannelDown2Color))
'                lastSwingCounterPotentialLine2.OuterPen.Thickness = NeutralChannel2Thickness
'                lastSwingCounterPotentialLine1.StartPoint = CurrentSwing2.EndPoint
'                lastSwingCounterPotentialLine1.EndPoint = FindRangeBar(CurrentSwing2.EndPoint.X, CurrentBar.Number, If(CurrentSwing2.Direction = Direction.Up, False, True))
'                lastSwingCounterPotentialLine1.OuterPen.Brush = New SolidColorBrush(If(lastSwingCounterPotentialLine1.EndPoint.Y > lastSwingCounterPotentialLine1.StartPoint.Y, NeutralChannelUp1Color, NeutralChannelDown2Color))
'                lastSwingCounterPotentialLine1.OuterPen.Thickness = NeutralChannel1Thickness
'                lastSwingCounterPotentialLine1.LockToEnd = False
'            End If




'            cs1 = CurrentSwing1
'            curSwingEvent1 = SwingEvent.None
'            'barsBackMark.Coordinates = New LineCoordinates(CurrentBar.Number - Max(CurrentBar.Number - cs.EndBar, RVBarsBack), rvPnt.Y, CurrentBar.Number, rvPnt.Y)
'            If swings2.Count > 0 AndAlso ((CurrentSwing1.Direction = CurrentSwing2.Direction And
'                                              ((cs1.Direction = Direction.Down And Abs(CurrentBar.High - cs1.EndPoint.Y) > RV1) OrElse
'                                              (cs1.Direction = Direction.Up And Abs(CurrentBar.Low - cs1.EndPoint.Y) > RV1))) Or
'                                              (curSwingEvent2 <> SwingEvent.None And CurrentSwing1.Direction <> CurrentSwing2.Direction)) Then
'                '((cs1.Direction = Direction.Down And Abs(CurrentBar.High - cs1.EndPoint.Y) / Abs(CurrentSwing2.EndPrice - CurrentSwing2.StartPrice) > RV1 / 100) OrElse
'                '(cs1.Direction = Direction.Up And Abs(CurrentBar.Low - cs1.EndPoint.Y) / Abs(CurrentSwing2.EndPrice - CurrentSwing2.StartPrice) > RV1 / 100))) Or

'                'new swing
'                'Dim canContinue As Boolean = True
'                'For i = cs.EndBar To CurrentBar.Number - 2
'                '    If (cs.Direction = Direction.Down And Chart.bars(i).Data.High > CurrentBar.High) Or (cs.Direction = Direction.Up And Chart.bars(i).Data.Low < CurrentBar.Low) Then
'                '        canContinue = False
'                '        Exit For
'                '    End If
'                'Next
'                'If canContinue Then
'                swings1.Add(NewSwing(SwingLineColor, New Point(cs1.EndBar, cs1.EndPrice),
'                                    New Point(CurrentBar.Number, If(cs1.Direction = Direction.Down, CurrentBar.High, CurrentBar.Low)), SwingLinesOn1, Not cs1.Direction))
'                If swings1.Count > 1 And SwingLinesOn1 Then
'                    If Not LastSwingIsChannel1 Then
'                        swings1(swings1.Count - 1).TL.Pen.Thickness = SwingLineThickness
'                        swings1(swings1.Count - 2).TL.Pen.Thickness = SwingLineThickness
'                    Else
'                        swings1(swings1.Count - 1).TL.Pen.Thickness = 0
'                        swings1(swings1.Count - 2).TL.Pen.Thickness = SwingLineThickness
'                    End If
'                End If
'                swings1(swings1.Count - 1).TL.IsSelectable = False
'                swings1(swings1.Count - 1).TL.IsEditable = False
'                curSwingEvent1 = AutoTrendBase.SwingEvent.NewSwing
'                swingDir1 = Not cs1.Direction

'                currentSwingLengthTexts1.Add(NewLabel("", SwingLineColor, New Point(swings1(swings1.Count - 1).EndBar + horizSwingTextAlignment, swings1(swings1.Count - 1).EndPrice + If(swingDir1 = Direction.Up, 1, -1) * Chart.GetRelativeFromRealHeight(vertSwingTextAlignment)), SwingTextsOn, , True))
'                If (swings1.Count > 0 And swings2.Count > 0) AndAlso CurrentSwing1.Direction <> CurrentSwing2.Direction Then
'                    currentSwingLengthTexts1(currentSwingLengthTexts1.Count - 1).Text = FormatNumber(Abs(CurrentSwing1.StartPoint.Y - CurrentSwing1.EndPoint.Y) / Abs(CurrentSwing2.EndPrice - CurrentSwing2.StartPrice) * 100, 0) & "%"
'                    currentSwingLengthTexts1(currentSwingLengthTexts1.Count - 1).Tag = Abs(CurrentSwing1.StartPoint.Y - CurrentSwing1.EndPoint.Y) + Chart.GetMinTick
'                Else
'                    currentSwingLengthTexts1(currentSwingLengthTexts1.Count - 1).Text = ""
'                End If
'                currentSwingLengthTexts1(currentSwingLengthTexts1.Count - 1).Font.FontSize = LengthTextFontSize1
'                currentSwingLengthTexts1(currentSwingLengthTexts1.Count - 1).Font.FontWeight = LengthTextFontWeight1
'                currentSwingLengthTexts1(currentSwingLengthTexts1.Count - 1).HorizontalAlignment = LabelHorizontalAlignment.Center
'                currentSwingLengthTexts1(currentSwingLengthTexts1.Count - 1).VerticalAlignment = LabelVerticalAlignment.Center
'                AddHandler currentSwingLengthTexts1(currentSwingLengthTexts1.Count - 1).MouseDownEvent,
'                    Sub(sender As Object)
'                        ApplyRV1(CDec(CType(sender, Label).Tag))
'                    End Sub
'                swings1(swings1.Count - 1).LengthText = currentSwingLengthTexts1(currentSwingLengthTexts1.Count - 1)
'                If SwingChannelOn1 Or LastSwingIsChannel1 Then
'                    With swings1(swings1.Count - 1)
'                        .SwingChannel = NewTrendLine(Colors.Black, .StartPoint, .EndPoint, True)
'                        .SwingChannel.IsRegressionLine = True
'                        .SwingChannel.HasParallel = True
'                        .SwingChannel.Pen = New Pen(Brushes.Black, 0)
'                        .SwingChannel.OuterPen = New Pen(New SolidColorBrush(SwingLineColor), SwingLineThickness)
'                        .SwingChannel.ExtendRight = True
'                        .SwingChannel.IsSelectable = False
'                        .SwingChannel.IsEditable = False
'                        .ABCLengths = New List(Of ABCCombo)
'                    End With
'                    If swings1.Count > 1 AndAlso swings1(swings1.Count - 2).SwingChannel IsNot Nothing Then
'                        With swings1(swings1.Count - 2)
'                            If SwingChannelHistoryOn And Not LastSwingIsChannel1 Then
'                                .SwingChannel.OuterPen.Thickness = SwingChannelHistoryThickness
'                                .SwingChannel.ExtendRight = False
'                            Else
'                                RemoveObjectFromChart(.SwingChannel)
'                            End If
'                        End With
'                    End If
'                End If
'                If SwingChannelLengthAndTargetTextsOn Then
'                    swings1(swings1.Count - 1).ABCLengths = New List(Of ABCCombo)
'                End If
'                'lastSwingPotentialLine1.StartPoint = CurrentSwing1.StartPoint
'                'lastSwingPotentialLine1.EndPoint = CurrentSwing1.EndPoint
'                'End If
'            ElseIf (swings1.Count > 0 AndAlso CurrentBar.High >= cs1.EndPrice AndAlso cs1.Direction = Direction.Up) OrElse (swings1.Count > 0 AndAlso CurrentBar.Low <= cs1.EndPrice AndAlso cs1.Direction = Direction.Down) Then
'                'extension
'                cs1.EndBar = CurrentBar.Number
'                cs1.EndPrice = If(cs1.Direction = Direction.Up, CurrentBar.High, CurrentBar.Low)
'                curSwingEvent1 = AutoTrendBase.SwingEvent.Extension
'                swingDir1 = cs1.Direction
'                currentSwingLengthTexts1(currentSwingLengthTexts1.Count - 1).Location = New Point(cs1.EndBar + horizSwingTextAlignment, cs1.EndPrice + If(swingDir1 = Direction.Up, 1, -1) * Chart.GetRelativeFromRealHeight(vertSwingTextAlignment))

'                If (swings1.Count > 0 And swings2.Count > 0) AndAlso CurrentSwing1.Direction <> CurrentSwing2.Direction Then
'                    currentSwingLengthTexts1(currentSwingLengthTexts1.Count - 1).Text = FormatNumber(Abs(CurrentSwing1.StartPoint.Y - CurrentSwing1.EndPoint.Y) / Abs(CurrentSwing2.EndPrice - CurrentSwing2.StartPrice) * 100, 0) & "%"
'                    currentSwingLengthTexts1(currentSwingLengthTexts1.Count - 1).Tag = Abs(CurrentSwing1.StartPoint.Y - CurrentSwing1.EndPoint.Y) + Chart.GetMinTick
'                Else
'                    currentSwingLengthTexts1(currentSwingLengthTexts1.Count - 1).Text = ""
'                End If

'                If swings1(swings1.Count - 1).SwingChannel IsNot Nothing AndAlso (SwingChannelOn1 Or LastSwingIsChannel1) Then
'                    swings1(swings1.Count - 1).SwingChannel.Coordinates = New LineCoordinates(cs1.StartPoint, cs1.EndPoint)
'                End If

'                If swings1(swings1.Count - 1).PushCountText IsNot Nothing Then
'                    swings1(swings1.Count - 1).PushCountText.Location = AddToY(swings1(swings1.Count - 1).EndPoint, If(swings1(swings1.Count - 1).Direction = Direction.Up, 1, -1) * 0.6 * Chart.Settings("RangeValue").Value)
'                End If
'            End If


'            'If cs.HasCrossedRVBand = False And ((swingDir = Direction.Up And cs.EndPrice >= currentRegressionCenterPoint + RVMultiplier * Chart.Settings("RangeValue").Value) Or (swingDir = Direction.Down And cs.EndPrice <= currentRegressionCenterPoint - RVMultiplier * Chart.Settings("RangeValue").Value)) Then cs.HasCrossedRVBand = True
'            lastSwingPotentialLine2.EndPoint = New Point(CurrentBar.Number, CurrentBar.High)
'            If curSwingEvent2 <> SwingEvent.None Then
'                If SwingChannelLengthAndTargetTextsOn And swings2(swings2.Count - 1).ABCLengths IsNot Nothing Then
'                    ' remove all previous bc lengths
'                    Dim bcLengths As New List(Of ABCCombo)
'                    While swings2(swings2.Count - 1).ABCLengths.Count > 0
'                        With swings2(swings2.Count - 1).ABCLengths
'                            RemoveObjectFromChart(.Item(0).BCProjectionLine)
'                            RemoveObjectFromChart(.Item(0).TargetText)
'                            RemoveObjectFromChart(.Item(0).LengthText)
'                            .Item(0).BCProjectionLine = Nothing
'                            .Item(0).TargetText = Nothing
'                            .Item(0).LengthText = Nothing
'                            .RemoveAt(0)
'                        End With
'                    End While
'                    ' calculate new bc points

'                    bcLengths = CalculateBCPointsForSwing(bcLengths, swings2(swings2.Count - 1).Direction, swings2(swings2.Count - 1).StartBar - 1, swings2(swings2.Count - 1).EndBar - 1)
'                    For Each blength In bcLengths
'                        Dim bcLength = blength.BCLength
'                        If blength.TargetText Is Nothing Then
'                            blength.TargetText = NewLabel(predictionTextPrefix & FormatNumberLengthAndPrefix(Round(bcLength, Chart.Settings("DecimalPlaces").Value), Chart.Settings("DecimalPlaces").Value), If(swings2(swings2.Count - 1).Direction = Direction.Up, ConfirmedDownChannelLineColor, ConfirmedUpChannelLineColor), New Point(CurrentBar.Number, CurrentSwing2.EndPrice - BooleanToInteger(swings2(swings2.Count - 1).Direction) * bcLength), False, , False)
'                            blength.TargetText.Font.FontSize = RVAndTargetTextFontSize2
'                            blength.TargetText.Font.FontWeight = RVAndTargetTextFontWeight2
'                            blength.TargetText.VerticalAlignment = LabelVerticalAlignment.Center
'                            blength.TargetText.HorizontalAlignment = LabelHorizontalAlignment.Left

'                            blength.LengthText = NewLabel(FormatNumberLengthAndPrefix(Round(bcLength, Chart.Settings("DecimalPlaces").Value), Chart.Settings("DecimalPlaces").Value), If(swings2(swings2.Count - 1).Direction = Direction.Up, ConfirmedDownChannelLineColor, ConfirmedUpChannelLineColor), New Point(blength.LengthTextLocation.X + horizSwingTextAlignment, blength.LengthTextLocation.Y + If(swingDir2 = Direction.Up, -1, 1) * (Chart.GetRelativeFromRealHeight(LengthTextFontSize2) + Chart.GetRelativeFromRealHeight(vertSwingTextAlignment))), False)
'                            blength.LengthText.Font.FontSize = RVAndTargetTextFontSize2
'                            blength.LengthText.Font.FontWeight = RVAndTargetTextFontWeight2
'                            blength.LengthText.VerticalAlignment = LabelVerticalAlignment.Center
'                            blength.LengthText.HorizontalAlignment = LabelHorizontalAlignment.Right
'                            AddHandler blength.LengthText.MouseDownEvent,
'                                Sub(sender As Object)
'                                    ApplyRV(CDec(CType(sender, Label).Text) / 10 ^ Chart.Settings("DecimalPlaces").Value + Chart.GetMinTick)
'                                End Sub

'                            blength.BCProjectionLine = NewTrendLine(If(swings2(swings2.Count - 1).Direction = Direction.Up, ConfirmedDownChannelLineColor, ConfirmedUpChannelLineColor), ProjectionLinesOn)
'                            blength.BCProjectionLine.Pen.Brush = New SolidColorBrush(If(swings2(swings2.Count - 1).Direction = Direction.Up, ConfirmedDownChannelLineColor, ConfirmedUpChannelLineColor))
'                            'blength.BCProjectionLine.Pen.DashStyle = TrendLineDashStyle
'                            blength.BCProjectionLine.Pen.Thickness = ProjectionLineThickness
'                            blength.BCProjectionLine.IsSelectable = False
'                            blength.BCProjectionLine.IsEditable = False
'                        Else
'                            blength.TargetText.Text = predictionTextPrefix & FormatNumberLengthAndPrefix(Round(bcLength, Chart.Settings("DecimalPlaces").Value), Chart.Settings("DecimalPlaces").Value)
'                        End If
'                    Next
'                    swings2(swings2.Count - 1).ABCLengths = bcLengths
'                End If


'            End If
'            If SwingChannelLengthAndTargetTextsOn And swings2(swings2.Count - 1).ABCLengths IsNot Nothing Then
'                Dim highestPoint As New Point(-1, 0)
'                Dim lowestPoint As New Point(-1, Decimal.MaxValue)
'                For i = CurrentSwing2.EndBar - 1 To CurrentBar.Number - 1
'                    If Chart.bars(i).Data.High >= highestPoint.Y Then
'                        highestPoint = New Point(i, Chart.bars(i).Data.High)
'                    End If
'                    If Chart.bars(i).Data.Low <= lowestPoint.Y Then
'                        lowestPoint = New Point(i, Chart.bars(i).Data.Low)
'                    End If
'                Next
'                If highestPoint.Y <> 0 Then
'                    Dim indx As Integer = 0
'                    While indx <= swings2(swings2.Count - 1).ABCLengths.Count - 1
'                        Dim blength As ABCCombo = swings2(swings2.Count - 1).ABCLengths(indx)
'                        If blength.TargetText IsNot Nothing Then
'                            If Abs(highestPoint.Y - lowestPoint.Y) >= blength.BCLength Then ' if bc swing length matched
'                                RemoveObjectFromChart(blength.TargetText)
'                                RemoveObjectFromChart(blength.BCProjectionLine)
'                                RemoveObjectFromChart(blength.LengthText)
'                                blength.TargetText = Nothing
'                                blength.BCProjectionLine = Nothing
'                                blength.LengthText = Nothing
'                                swings2(swings2.Count - 1).ABCLengths.RemoveAt(indx)
'                                indx -= 1
'                            End If
'                        End If
'                        indx += 1
'                    End While
'                End If
'                If swings2.Count > 1 AndAlso swings2(swings2.Count - 2).ABCLengths IsNot Nothing Then
'                    While swings2(swings2.Count - 2).ABCLengths.Count > 0
'                        With swings2(swings2.Count - 2).ABCLengths
'                            RemoveObjectFromChart(.Item(0).BCProjectionLine)
'                            RemoveObjectFromChart(.Item(0).TargetText)
'                            RemoveObjectFromChart(.Item(0).LengthText)
'                            .Item(0).BCProjectionLine = Nothing
'                            .Item(0).TargetText = Nothing
'                            .Item(0).LengthText = Nothing
'                            .RemoveAt(0)
'                        End With
'                    End While
'                End If
'            End If


'            ColorCurrentBars()


'            If DrawRegressionChannels And TrendChannelOn Then
'                If curSwingEvent2 <> SwingEvent.None Then
'                    Dim lastConfirmedSwingIndex As Integer
'                    If regressionChannels.Count = 0 Then
'                        lastConfirmedSwingIndex = 0
'                    Else
'                        lastConfirmedSwingIndex = regressionChannels(regressionChannels.Count - 1).EndSwingIndex
'                    End If
'                    If swings2.Count - 1 >= lastConfirmedSwingIndex + 3 Then ' if there's been 3 neutral swings1 in a row

'                        Dim highestPoint As New Point(-1, 0)
'                        Dim secondHighestPoint As New Point(-1, 0)
'                        Dim lowestPoint As New Point(-1, Decimal.MaxValue)
'                        Dim secondLowestPoint As New Point(-1, Decimal.MaxValue)
'                        For i = lastConfirmedSwingIndex + 1 To swings2.Count - 1
'                            If swings2(i).EndPrice >= highestPoint.Y Then
'                                highestPoint = New Point(i, swings2(i).EndPrice)
'                            End If
'                            If swings2(i).EndPrice <= lowestPoint.Y Then
'                                lowestPoint = New Point(i, swings2(i).EndPrice)
'                            End If
'                        Next
'                        For i = lastConfirmedSwingIndex + 1 To swings2.Count - 1
'                            If swings2(i).EndPrice > secondHighestPoint.Y And swings2(i).EndPrice <= highestPoint.Y And i <> highestPoint.X Then
'                                secondHighestPoint = New Point(i, swings2(i).EndPrice)
'                            End If
'                            If swings2(i).EndPrice < secondLowestPoint.Y And swings2(i).EndPrice >= lowestPoint.Y And i <> lowestPoint.X Then
'                                secondLowestPoint = New Point(i, swings2(i).EndPrice)
'                            End If
'                        Next
'                        Dim channelDir As Direction = Direction.Neutral
'                        If swings2.Count - 1 = highestPoint.X And lowestPoint.Y > swings2(lastConfirmedSwingIndex).EndPrice Then
'                            channelDir = Direction.Up
'                        ElseIf swings2.Count - 1 = lowestPoint.X And highestPoint.Y < swings2(lastConfirmedSwingIndex).EndPrice Then
'                            channelDir = Direction.Down
'                        End If

'                        'channel is confirmed; create new channel
'                        If channelDir <> Direction.Neutral Then
'                            Dim newChannel As RegressionChannel = CreateRegressionChannel(If(channelDir = Direction.Up, TrendChannelUpColor, TrendChannelDownColor), lastConfirmedSwingIndex, swings2.Count - 1, channelDir, True)
'                            Dim bPoint, cPoint As Point
'                            If channelDir = Direction.Up Then
'                                bPoint = New Point(swings2(secondHighestPoint.X).EndBar, swings2(secondHighestPoint.X).EndPrice)
'                                cPoint = New Point(swings2(lowestPoint.X).EndBar, swings2(lowestPoint.X).EndPrice)
'                                AddPushPointText(swings2(secondHighestPoint.X), 1)
'                            Else
'                                bPoint = New Point(swings2(secondLowestPoint.X).EndBar, swings2(secondLowestPoint.X).EndPrice)
'                                cPoint = New Point(swings2(highestPoint.X).EndBar, swings2(highestPoint.X).EndPrice)
'                                AddPushPointText(swings2(secondLowestPoint.X), 1)
'                            End If
'                            AddPushPointText(swings2(swings2.Count - 1), 2)
'                            newChannel.Pushes = 2
'                            AssignRegressionChannelProperties(newChannel, New Point(swings2(lastConfirmedSwingIndex).EndBar, swings2(lastConfirmedSwingIndex).EndPrice), , New Point(swings2(swings2.Count - 1).EndBar, swings2(swings2.Count - 1).EndPrice), , , True)
'                            regressionChannels(regressionChannels.Count - 1).TL.ExtendRight = False
'                            regressionChannels.Add(newChannel)
'                        End If
'                    Else
'                        ' outside swing
'                        If regressionChannels.Count > 0 AndAlso regressionChannels(regressionChannels.Count - 1).EndSwingIndex + 1 = swings2.Count - 1 Then ' if it's been one swing since last confirmed channel
'                            Dim channelDir As Direction = Direction.Neutral
'                            If regressionChannels(regressionChannels.Count - 1).Direction = Swing.ChannelDirectionType.Up And swings2(swings2.Count - 1).EndPrice < regressionChannels(regressionChannels.Count - 1).APoint.Y Then
'                                channelDir = Direction.Down
'                            ElseIf regressionChannels(regressionChannels.Count - 1).Direction = Swing.ChannelDirectionType.Down And swings2(swings2.Count - 1).EndPrice > regressionChannels(regressionChannels.Count - 1).APoint.Y Then
'                                channelDir = Direction.Up
'                            End If
'                            If channelDir <> Direction.Neutral Then
'                                Dim newChannel As RegressionChannel = CreateRegressionChannel(If(channelDir = Direction.Down, TrendChannelDownColor, TrendChannelUpColor), lastConfirmedSwingIndex, swings2.Count - 1, channelDir, True)
'                                newChannel.APoint = New Point(swings2(lastConfirmedSwingIndex).EndBar, swings2(lastConfirmedSwingIndex).EndPrice)
'                                newChannel.DPoint = New Point(swings2(swings2.Count - 1).EndBar, swings2(swings2.Count - 1).EndPrice)
'                                regressionChannels(regressionChannels.Count - 1).TL.ExtendRight = False
'                                regressionChannels.Add(newChannel)
'                                If potentialRegressionChannel IsNot Nothing Then
'                                    RemoveObjectFromChart(potentialRegressionChannel.TL)
'                                    potentialRegressionChannel = Nothing
'                                End If
'                                If potentialSameDirRegressionChannel IsNot Nothing Then
'                                    RemoveObjectFromChart(potentialSameDirRegressionChannel.TL)
'                                    potentialSameDirRegressionChannel = Nothing
'                                End If
'                                newChannel.Pushes = 1
'                                AddPushPointText(CurrentSwing2, 1)
'                            End If
'                        End If
'                        If regressionChannels.Count = 0 And swings2.Count > 1 Then ' if there's no channels yet
'                            'get things started by adding one
'                            Dim newChannel As RegressionChannel = CreateRegressionChannel(If(swings2(swings2.Count - 1).Direction = Direction.Down, TrendChannelDownColor, TrendChannelUpColor), swings2.Count - 2, swings2.Count - 1, swings2(swings2.Count - 1).Direction, True)
'                            newChannel.APoint = New Point(swings2(swings2.Count - 2).EndBar, swings2(swings2.Count - 2).EndPrice)
'                            newChannel.DPoint = New Point(swings2(swings2.Count - 1).EndBar, swings2(swings2.Count - 1).EndPrice)
'                            regressionChannels.Add(newChannel)
'                            If potentialRegressionChannel IsNot Nothing Then
'                                RemoveObjectFromChart(potentialRegressionChannel.TL)
'                                potentialRegressionChannel = Nothing
'                            End If
'                            If potentialSameDirRegressionChannel IsNot Nothing Then
'                                RemoveObjectFromChart(potentialSameDirRegressionChannel.TL)
'                                potentialSameDirRegressionChannel = Nothing
'                            End If
'                        End If
'                    End If
'                    If regressionChannels.Count > 0 Then
'                        ' extend channel
'                        If (regressionChannels(regressionChannels.Count - 1).Direction = Swing.ChannelDirectionType.Up And swings2(swings2.Count - 1).EndPrice >= regressionChannels(regressionChannels.Count - 1).DPoint.Y) Or
'                            (regressionChannels(regressionChannels.Count - 1).Direction = Swing.ChannelDirectionType.Down And swings2(swings2.Count - 1).EndPrice <= regressionChannels(regressionChannels.Count - 1).DPoint.Y) Then
'                            'clear previous bc lengths to make room for (possibly) different ones 
'                            Dim bcLengths As New List(Of ABCCombo)
'                            While regressionChannels(regressionChannels.Count - 1).BCLengths.Count > 0
'                                With regressionChannels(regressionChannels.Count - 1).BCLengths
'                                    RemoveObjectFromChart(.Item(0).BCProjectionLine)
'                                    RemoveObjectFromChart(.Item(0).TargetText)
'                                    .Item(0).BCProjectionLine = Nothing
'                                    .Item(0).TargetText = Nothing
'                                    .RemoveAt(0)
'                                End With
'                            End While
'                            ' calculate new bc points
'                            bcLengths = CalculateBCPoints(bcLengths, regressionChannels(regressionChannels.Count - 1).Direction, regressionChannels(regressionChannels.Count - 1).StartSwingIndex, regressionChannels(regressionChannels.Count - 1).EndSwingIndex)
'                            AssignRegressionChannelProperties(regressionChannels(regressionChannels.Count - 1), , bcLengths, swings2(swings2.Count - 1).EndPoint, , swings2.Count - 1, True)
'                            If swings2(swings2.Count - 1).PushCountText Is Nothing Then
'                                AddPushPointText(swings2(swings2.Count - 1), regressionChannels(regressionChannels.Count - 1).Pushes + 1)
'                                regressionChannels(regressionChannels.Count - 1).Pushes += 1
'                            End If
'                            If potentialSameDirRegressionChannel IsNot Nothing Then
'                                RemoveObjectFromChart(potentialSameDirRegressionChannel.TL)
'                                potentialSameDirRegressionChannel = Nothing
'                            End If
'                        End If
'                    End If
'                End If
'                ' every bar ****
'                'check for bc lengths for match
'                For Each channel In regressionChannels
'                    If channel.IsConfirmed Then
'                        Dim highestPoint As New Point(-1, 0)
'                        Dim lowestPoint As New Point(-1, Decimal.MaxValue)
'                        For i = swings2(channel.EndSwingIndex).EndBar - 1 To CurrentBar.Number - 1
'                            If Chart.bars(i).Data.High >= highestPoint.Y Then
'                                highestPoint = New Point(i, Chart.bars(i).Data.High)
'                            End If
'                            If Chart.bars(i).Data.Low <= lowestPoint.Y Then
'                                lowestPoint = New Point(i, Chart.bars(i).Data.Low)
'                            End If
'                        Next
'                        Dim indx As Integer = 0
'                        While indx <= channel.BCLengths.Count - 1
'                            Dim blength As ABCCombo = channel.BCLengths(indx)
'                            If blength.TargetText IsNot Nothing Then
'                                If Abs(highestPoint.Y - lowestPoint.Y) >= blength.BCLength Then ' if bc swing length matched
'                                    RemoveObjectFromChart(blength.TargetText)
'                                    RemoveObjectFromChart(blength.BCProjectionLine)
'                                    blength.TargetText = Nothing
'                                    blength.BCProjectionLine = Nothing
'                                    channel.BCLengths.RemoveAt(indx)
'                                    indx -= 1
'                                End If
'                            End If
'                            indx += 1
'                        End While
'                    End If
'                Next
'                If regressionChannels.Count > 0 AndAlso regressionChannels(regressionChannels.Count - 1).EndSwingIndex < swings2.Count - 2 Then ' if it's been at least 2 swings1 since last confirmed channel
'                    ' update potential line
'                    If potentialRegressionChannel Is Nothing Then
'                        potentialRegressionChannel = CreateRegressionChannel(If(regressionChannels(regressionChannels.Count - 1).Direction = Swing.ChannelDirectionType.Up, DownTrendChannelNeutralColor, UpTrendChannelNeutralColor), regressionChannels(regressionChannels.Count - 1).EndSwingIndex, swings2.Count - 1, Direction.Neutral, False)
'                        potentialRegressionChannel.TL.Pen.Thickness = 0
'                    End If
'                    potentialRegressionChannel.StartSwingIndex = regressionChannels(regressionChannels.Count - 1).EndSwingIndex
'                    potentialRegressionChannel.EndSwingIndex = swings2.Count - 1
'                    potentialRegressionChannel.TL.Coordinates = New LineCoordinates(New Point(swings2(potentialRegressionChannel.StartSwingIndex).EndBar, swings2(potentialRegressionChannel.StartSwingIndex).EndPrice), New Point(CurrentBar.Number, CurrentBar.High))
'                End If
'                If regressionChannels.Count > 0 AndAlso regressionChannels(regressionChannels.Count - 1).EndSwingIndex <= swings2.Count - 2 Then ' if it's been at least 1 swing since last confirmed channel
'                    ' update potential line
'                    If potentialSameDirRegressionChannel Is Nothing Then
'                        potentialSameDirRegressionChannel = CreateRegressionChannel(If(regressionChannels(regressionChannels.Count - 1).Direction = Swing.ChannelDirectionType.Up, UpTrendChannelNeutralColor, DownTrendChannelNeutralColor), regressionChannels(regressionChannels.Count - 1).StartSwingIndex, swings2.Count - 1, Direction.Neutral, False)
'                        potentialSameDirRegressionChannel.TL.Pen.Thickness = 0
'                    End If
'                    potentialSameDirRegressionChannel.StartSwingIndex = regressionChannels(regressionChannels.Count - 1).StartSwingIndex
'                    potentialSameDirRegressionChannel.EndSwingIndex = swings2.Count - 1
'                    potentialSameDirRegressionChannel.TL.Coordinates = New LineCoordinates(New Point(swings2(potentialSameDirRegressionChannel.StartSwingIndex).EndBar, swings2(potentialSameDirRegressionChannel.StartSwingIndex).EndPrice), New Point(CurrentBar.Number, CurrentBar.High))
'                End If
'            End If

'            If curSwingEvent2 <> SwingEvent.None Or IsLastBarOnChart Then
'                If potentialRegressionChannel IsNot Nothing Then
'                    'MsgBox("here")
'                    Dim maxPoint As Point = New Point(0, If(regressionChannels(regressionChannels.Count - 1).Direction = Swing.ChannelDirectionType.Down, Double.MinValue, Double.MaxValue))
'                    For i = potentialRegressionChannel.StartSwingIndex To swings2.Count - 2
'                        If regressionChannels(regressionChannels.Count - 1).Direction = Swing.ChannelDirectionType.Up Then
'                            If swings2(i).EndPrice <= maxPoint.Y Then maxPoint = New Point(swings2(i).EndBar, swings2(i).EndPrice)
'                        Else
'                            If swings2(i).EndPrice >= maxPoint.Y Then maxPoint = New Point(swings2(i).EndBar, swings2(i).EndPrice)
'                        End If
'                    Next
'                    newTrendTextObject.Location = New Point(CurrentBar.Number + trendTextOffset, maxPoint.Y)
'                    newTrendTextObject.Font.Brush = New SolidColorBrush(If(regressionChannels(regressionChannels.Count - 1).Direction = Swing.ChannelDirectionType.Down, TrendChannelUpColor, TrendChannelDownColor))
'                    newTrendLineObject.Coordinates = New LineCoordinates(maxPoint, New Point(CurrentBar.Number + trendTextOffset, maxPoint.Y))
'                    newTrendLineObject.Pen.Brush = New SolidColorBrush(If(regressionChannels(regressionChannels.Count - 1).Direction = Swing.ChannelDirectionType.Down, TrendChannelUpColor, TrendChannelDownColor))
'                Else
'                    If regressionChannels.Count > 0 Then
'                        newTrendTextObject.Location = New Point(CurrentBar.Number + trendTextOffset, regressionChannels(regressionChannels.Count - 1).APoint.Y)
'                        newTrendTextObject.Font.Brush = New SolidColorBrush(If(regressionChannels(regressionChannels.Count - 1).Direction = Swing.ChannelDirectionType.Down, TrendChannelUpColor, TrendChannelDownColor))
'                        newTrendLineObject.Coordinates = New LineCoordinates(regressionChannels(regressionChannels.Count - 1).APoint, New Point(CurrentBar.Number + trendTextOffset, regressionChannels(regressionChannels.Count - 1).APoint.Y))
'                        newTrendLineObject.Pen.Brush = New SolidColorBrush(If(regressionChannels(regressionChannels.Count - 1).Direction = Swing.ChannelDirectionType.Down, TrendChannelUpColor, TrendChannelDownColor))
'                    End If
'                End If
'                If regressionChannels.Count > 0 Then
'                    If swings2.Count <> regressionChannels(regressionChannels.Count - 1).EndSwingIndex Then
'                        AddObjectToChart(extendTrendTextObject)
'                        extendTrendTextObject.Text = " Ext Trend"
'                        extendTrendTextObject.Location = New Point(CurrentBar.Number + trendTextOffset, swings2(regressionChannels(regressionChannels.Count - 1).EndSwingIndex).EndPrice)
'                        extendTrendTextObject.Font.Brush = New SolidColorBrush(If(regressionChannels(regressionChannels.Count - 1).Direction = Swing.ChannelDirectionType.Down, TrendChannelDownColor, TrendChannelUpColor))

'                        AddObjectToChart(extendTrendLineObject)
'                        extendTrendLineObject.Coordinates = New LineCoordinates(swings2(regressionChannels(regressionChannels.Count - 1).EndSwingIndex).EndPoint, New Point(CurrentBar.Number + trendTextOffset, swings2(regressionChannels(regressionChannels.Count - 1).EndSwingIndex).EndPrice))
'                        extendTrendLineObject.Pen.Brush = New SolidColorBrush(If(regressionChannels(regressionChannels.Count - 1).Direction = Swing.ChannelDirectionType.Down, TrendChannelDownColor, TrendChannelUpColor))
'                    Else
'                        RemoveObjectFromChart(extendTrendTextObject)
'                        RemoveObjectFromChart(extendTrendLineObject)
'                    End If
'                End If
'                extendSwingTextObject2.Text = rvTextPrefix2 & " Ext Trend"
'                extendSwingTextObject2.Location = New Point(CurrentBar.Number, CurrentSwing2.EndPrice)
'                extendSwingTextObject2.Font.Brush = New SolidColorBrush(If(swingDir2 = Direction.Up, UpTrendChannelColor, DownTrendChannelColor))

'                extendSwingTextObject1.Text = rvTextPrefix1 & " Ext Sw1"
'                extendSwingTextObject1.Location = New Point(CurrentBar.Number, CurrentSwing1.EndPrice)

'            End If

'            UpdateHorizontalLines()
'            If IsLastBarOnChart Then
'                DrawProjectionLineAndRVText()
'            End If
'            If CurrentBar.Number > Length + 1 Then
'                UpdateLine(CurrentBar.Number - Length, CurrentBar.Number)
'            End If
'            If MomentumBarsVisible And CurrentBar.Number > Chart.bars.Count - NoOfMomentumBars Then
'                Dim topPoint As Double = momentumBarsRVLines(0).StartPoint.Y
'                Dim botPoint As Double = momentumBarsRVLines(0).StartPoint.Y
'                If IsLastBarOnChart Then
'                    momentumBarsRVLines(0).EndPoint = New Point(CurrentBar.Number, momentumBarsRVLines(0).StartPoint.Y)
'                    momentumBarsRVLines(1).EndPoint = New Point(CurrentBar.Number, momentumBarsRVLines(1).StartPoint.Y)
'                    momentumBarsOffsetLines(0).EndPoint = New Point(CurrentBar.Number, momentumBarsOffsetLines(0).StartPoint.Y)
'                    momentumBarsOffsetLines(1).EndPoint = New Point(CurrentBar.Number, momentumBarsOffsetLines(1).StartPoint.Y)
'                End If
'                If regressionCenterPoints.ContainsKey(CurrentBar.Number - 1) Then

'                    Dim point1 = regressionCenterPoints(CurrentBar.Number - 1) + Abs(CurvedLineMultiplier * Chart.Settings("RangeValue").Value)
'                    Dim point2 = regressionCenterPoints(CurrentBar.Number - 1) - Abs(CurvedLineMultiplier * Chart.Settings("RangeValue").Value)
'                    If Chart.bars(CurrentBar.Number - 2).Data.High > point1 Then
'                        momentumBarsPlots(0).AddNonContinuousPoint(New Point(CurrentBar.Number - 1, Chart.bars(CurrentBar.Number - 2).Data.High - point1 + topPoint))
'                        momentumBarsPlots(0).AddPoint(New Point(CurrentBar.Number - 1, Max(Chart.bars(CurrentBar.Number - 2).Data.Low, point1) - point1 + topPoint))
'                    End If
'                    If Chart.bars(CurrentBar.Number - 2).Data.Low < point2 Then
'                        momentumBarsPlots(2).AddNonContinuousPoint(New Point(CurrentBar.Number - 1, Chart.bars(CurrentBar.Number - 2).Data.Low - point1 + topPoint))
'                        momentumBarsPlots(2).AddPoint(New Point(CurrentBar.Number - 1, Min(Chart.bars(CurrentBar.Number - 2).Data.High, point2) - point1 + topPoint))
'                    End If
'                    If Chart.bars(CurrentBar.Number - 2).Data.Low < point1 And Chart.bars(CurrentBar.Number - 2).Data.High > point2 Then
'                        momentumBarsPlots(1).AddNonContinuousPoint(New Point(CurrentBar.Number - 1, Min(Chart.bars(CurrentBar.Number - 2).Data.High, point1) - point1 + topPoint))
'                        momentumBarsPlots(1).AddPoint(New Point(CurrentBar.Number - 1, Max(Chart.bars(CurrentBar.Number - 2).Data.Low, point2) - point1 + topPoint))
'                    End If
'                End If
'                If IsLastBarOnChart And regressionCenterPoints.ContainsKey(CurrentBar.Number) Then
'                    Dim point1 = regressionCenterPoints(CurrentBar.Number) + Abs(CurvedLineMultiplier * Chart.Settings("RangeValue").Value)
'                    Dim point2 = regressionCenterPoints(CurrentBar.Number) - Abs(CurvedLineMultiplier * Chart.Settings("RangeValue").Value)
'                    If Chart.bars(CurrentBar.Number - 1).Data.Low > point2 Then
'                        lastMomentumBar.Pen.Brush = GetGradientBarBrush(UpColor, NeutralColor, point1)
'                    Else
'                        lastMomentumBar.Pen.Brush = GetGradientBarBrush(NeutralColor, DownColor, point2)
'                    End If
'                    lastMomentumBar.Coordinates = New LineCoordinates(CurrentBar.Number, CurrentBar.High - point1 + topPoint, CurrentBar.Number, CurrentBar.Low - point1 + topPoint)

'                End If
'                If curSwingEvent2 = SwingEvent.NewSwing Then
'                    If regressionCenterPoints.ContainsKey(CurrentSwing2.StartBar) And regressionCenterPoints.ContainsKey(CurrentSwing2.EndBar) Then
'                        currentMomentumSwing = NewTrendLine(If(cs2.Direction = Direction.Down, UpTrendChannelColor, DownTrendChannelColor), New Point(CurrentSwing2.StartBar, CurrentSwing2.StartPrice - regressionCenterPoints(CurrentSwing2.StartBar) + VerticalMomentumBarsPosition),
'                                                                                                                                         New Point(CurrentSwing2.EndBar, CurrentSwing2.EndPrice - regressionCenterPoints(CurrentSwing2.EndBar) + VerticalMomentumBarsPosition))
'                        currentMomentumSwing.IsSelectable = False
'                        currentMomentumSwing.IsEditable = False
'                    End If
'                ElseIf curSwingEvent2 = SwingEvent.Extension And currentMomentumSwing IsNot Nothing Then
'                    If regressionCenterPoints.ContainsKey(CurrentSwing2.EndBar) Then
'                        currentMomentumSwing.EndPoint = New Point(CurrentSwing2.EndBar, CurrentSwing2.EndPrice - regressionCenterPoints(CurrentSwing2.EndBar) + VerticalMomentumBarsPosition)
'                    End If
'                End If
'            End If
'        End Sub
'        Private Sub AddPushPointText(swing As Swing, pushNumber As Integer)
'            With swing
'                .PushCountText = NewLabel(pushNumber, If(swing.Direction = Direction.Up, TrendChannelUpColor, TrendChannelDownColor), AddToY(swing.EndPoint, If(swings2(swings2.Count - 1).Direction = Direction.Up, 1, -1) * 0.6 * Chart.Settings("RangeValue").Value), , , False)
'                .PushCountText.Font.FontSize = PushTextFontSize
'                .PushCountText.Font.FontWeight = TrendTextFontWeight
'                .PushCountText.HorizontalAlignment = LabelHorizontalAlignment.Center
'                .PushCountText.VerticalAlignment = If(swing.Direction = Direction.Up, LabelVerticalAlignment.Bottom, LabelVerticalAlignment.Top)
'            End With
'        End Sub
'        Private Function CalculateBCPoints(ByRef bcLengths As List(Of ABCCombo), direction As Direction, startSwingIndex As Integer, endSwingIndex As Integer) As List(Of ABCCombo)
'            If startSwingIndex + 1 >= endSwingIndex Then Return bcLengths
'            Dim rangePoint As New Point(0, If(direction = AutoTrendBase.Direction.Down, 0, Decimal.MaxValue)) : Dim opposedRangePoint As New Point(0, If(direction = AutoTrendBase.Direction.Down, Decimal.MaxValue, 0))
'            For i = endSwingIndex To startSwingIndex + 1 Step -1
'                If (direction = AutoTrendBase.Direction.Down And swings2(i).EndPrice >= rangePoint.Y) Or (direction = AutoTrendBase.Direction.Up And swings2(i).EndPrice <= rangePoint.Y) Then rangePoint = New Point(i, swings2(i).EndPrice)
'            Next
'            For i = rangePoint.X To startSwingIndex Step -1
'                If (direction = AutoTrendBase.Direction.Down And swings2(i).EndPrice < opposedRangePoint.Y) Or (direction = AutoTrendBase.Direction.Up And swings2(i).EndPrice > opposedRangePoint.Y) Then opposedRangePoint = New Point(i, swings2(i).EndPrice)
'            Next
'            Dim bcLength As Decimal = Round(Abs(rangePoint.Y - opposedRangePoint.Y), 6) : Dim indx As Integer = 0
'            While indx <= bcLengths.Count - 1
'                If bcLength >= bcLengths(indx).BCLength Then
'                    RemoveObjectFromChart(bcLengths(indx).TargetText)
'                    RemoveObjectFromChart(bcLengths(indx).BCProjectionLine)
'                    bcLengths.RemoveAt(indx) : indx -= 1
'                End If : indx += 1
'            End While

'            bcLengths.Add(New ABCCombo With {.BCLength = bcLength})
'            Return CalculateBCPoints(bcLengths, direction, rangePoint.X, endSwingIndex)
'        End Function
'        Private Function CalculateBCPointsForSwing(ByRef bcLengths As List(Of ABCCombo), direction As Direction, startBar As Integer, endBar As Integer) As List(Of ABCCombo)
'            If startBar + 1 >= endBar Then Return bcLengths
'            Dim rangePoint As New Point(0, If(direction = AutoTrendBase.Direction.Down, 0, Decimal.MaxValue))
'            Dim opposedRangePoint As New Point(0, If(direction = AutoTrendBase.Direction.Down, Decimal.MaxValue, 0))
'            For i = endBar To startBar + 1 Step -1
'                If (direction = AutoTrendBase.Direction.Down And Chart.bars(i).Data.High >= rangePoint.Y) Or (direction = AutoTrendBase.Direction.Up And Chart.bars(i).Data.Low <= rangePoint.Y) Then
'                    rangePoint = New Point(i, If(direction = AutoTrendBase.Direction.Down, Chart.bars(i).Data.High, Chart.bars(i).Data.Low))
'                End If
'            Next
'            For i = rangePoint.X To startBar Step -1
'                If (direction = AutoTrendBase.Direction.Down And Chart.bars(i).Data.Low < opposedRangePoint.Y) Or (direction = AutoTrendBase.Direction.Up And Chart.bars(i).Data.High > opposedRangePoint.Y) Then
'                    opposedRangePoint = New Point(i, If(direction = AutoTrendBase.Direction.Down, Chart.bars(i).Data.Low, Chart.bars(i).Data.High))
'                End If
'            Next
'            Dim bcLength As Decimal = Round(Abs(rangePoint.Y - opposedRangePoint.Y), 6) : Dim indx As Integer = 0
'            While indx <= bcLengths.Count - 1
'                If bcLength >= bcLengths(indx).BCLength Then
'                    RemoveObjectFromChart(bcLengths(indx).TargetText)
'                    RemoveObjectFromChart(bcLengths(indx).BCProjectionLine)
'                    bcLengths.RemoveAt(indx) : indx -= 1
'                End If : indx += 1
'            End While
'            If bcLength > Chart.Settings("RangeValue").Value Then bcLengths.Add(New ABCCombo With {.BCLength = bcLength, .LengthTextLocation = rangePoint})
'            Return CalculateBCPointsForSwing(bcLengths, direction, rangePoint.X, endBar)
'        End Function
'        Private Sub AssignRegressionChannelProperties(newChannel As RegressionChannel, Optional aPoint As Point = Nothing, Optional bcLengths As List(Of ABCCombo) = Nothing, Optional dPoint As Point = Nothing, Optional startSwingIndex As Integer = 0, Optional endSwingIndex As Integer = 0, Optional showTargetText As Boolean = False)
'            If aPoint <> Nothing Then newChannel.APoint = aPoint
'            If bcLengths IsNot Nothing Then newChannel.BCLengths = bcLengths
'            If dPoint <> Nothing Then newChannel.DPoint = dPoint
'            If showTargetText Then
'                For Each blength In newChannel.BCLengths
'                    Dim bcLength = blength.BCLength
'                    If blength.TargetText Is Nothing Then
'                        blength.TargetText = NewLabel(predictionTextPrefix & FormatNumberLengthAndPrefix(Round(bcLength, Chart.Settings("DecimalPlaces").Value), Chart.Settings("DecimalPlaces").Value), If(newChannel.Direction = Direction.Up, ConfirmedDownChannelLineColor, ConfirmedUpChannelLineColor), New Point(CurrentBar.Number, CurrentSwing2.EndPrice - BooleanToInteger(newChannel.Direction) * bcLength), False, , False)
'                        blength.TargetText.Font.FontSize = RVAndTargetTextFontSize2
'                        blength.TargetText.Font.FontWeight = RVAndTargetTextFontWeight2
'                        blength.TargetText.VerticalAlignment = LabelVerticalAlignment.Center
'                        blength.TargetText.HorizontalAlignment = LabelHorizontalAlignment.Left
'                        blength.BCProjectionLine = NewTrendLine(If(newChannel.Direction = Direction.Up, ConfirmedDownChannelLineColor, ConfirmedUpChannelLineColor), ProjectionLinesOn)
'                        blength.BCProjectionLine.Pen.Brush = New SolidColorBrush(If(newChannel.Direction = Direction.Up, ConfirmedDownChannelLineColor, ConfirmedUpChannelLineColor))
'                        'blength.BCProjectionLine.Pen.DashStyle = TrendLineDashStyle
'                        blength.BCProjectionLine.Pen.Thickness = ProjectionLineThickness
'                        blength.BCProjectionLine.IsSelectable = False
'                        blength.BCProjectionLine.IsEditable = False
'                    Else
'                        blength.TargetText.Text = predictionTextPrefix & FormatNumberLengthAndPrefix(Round(bcLength, Chart.Settings("DecimalPlaces").Value), Chart.Settings("DecimalPlaces").Value)
'                    End If
'                Next
'            End If
'            If startSwingIndex <> 0 Then
'                newChannel.StartSwingIndex = startSwingIndex
'                newChannel.TL.StartPoint = swings2(startSwingIndex).EndPoint
'            End If
'            If endSwingIndex <> 0 Then
'                newChannel.EndSwingIndex = endSwingIndex
'                newChannel.TL.EndPoint = swings2(endSwingIndex).EndPoint
'            End If
'            If potentialRegressionChannel IsNot Nothing Then
'                RemoveObjectFromChart(potentialRegressionChannel.TL)
'                potentialRegressionChannel = Nothing
'            End If
'            If potentialSameDirRegressionChannel IsNot Nothing Then
'                RemoveObjectFromChart(potentialSameDirRegressionChannel.TL)
'                potentialSameDirRegressionChannel = Nothing
'            End If
'            If newChannel.IsConfirmed Then BarColor(swings2(newChannel.StartSwingIndex).EndBar - 1, swings2(newChannel.EndSwingIndex).EndBar - 1, If(newChannel.Direction = Swing.ChannelDirectionType.Up, UpColor, DownColor), Colors.Black, newChannel.Direction)
'            If newChannel.IsConfirmed Then
'                For i = newChannel.StartSwingIndex + 1 To newChannel.EndSwingIndex
'                    If swings2(i).Direction = newChannel.Direction Then
'                        RemoveObjectFromChart(swings2(i).LengthText)
'                    End If
'                Next
'            End If
'        End Sub
'        Private Function CreateRegressionChannel(color As Color, startSwingIndex As Integer, endSwingIndex As Integer, direction As Direction, isConfirmed As Boolean) As RegressionChannel
'            Dim newChannel As RegressionChannel
'            newChannel = New RegressionChannel
'            newChannel.BCLengths = New List(Of ABCCombo)
'            newChannel.TL = NewTrendLine(color, New Point(swings2(startSwingIndex).EndBar, swings2(startSwingIndex).EndPrice), New Point(swings2(endSwingIndex).EndBar, swings2(endSwingIndex).EndPrice), TrendChannelOn)
'            newChannel.TL.HasParallel = True
'            newChannel.TL.Pen = New Pen(New SolidColorBrush(color), TrendChannelCenterLineThickness)
'            newChannel.TL.OuterPen = New Pen(New SolidColorBrush(color), TrendChannelThickness2)
'            newChannel.TL.IsRegressionLine = True
'            newChannel.StartSwingIndex = startSwingIndex
'            newChannel.EndSwingIndex = endSwingIndex
'            newChannel.TL.ExtendRight = True
'            newChannel.Direction = direction
'            newChannel.IsConfirmed = isConfirmed
'            newChannel.TL.IsSelectable = False
'            newChannel.TL.IsEditable = False
'            If isConfirmed Then BarColor(swings2(newChannel.StartSwingIndex).EndBar - 1, swings2(newChannel.EndSwingIndex).EndBar - 1, If(direction = Swing.ChannelDirectionType.Up, UpColor, DownColor), Colors.Black, newChannel.Direction)
'            If TrendChannelOn Then AddObjectToChart(newChannel.TL)
'            Return newChannel
'        End Function
'        Protected Overrides Sub NewBar()
'            If CurrentBar.Number > Length + 1 Then
'                UpdateLineNewBar(CurrentBar.Number - Length, CurrentBar.Number)
'            End If
'        End Sub
'        'Dim counter1 As Integer
'        Private Function CalculateNewStartPointRegression(startPointX As Decimal, aPoint As Point, cPoint As Point, Optional endPointX As Decimal = -1) As Point
'            If endPointX = -1 Then endPointX = CurrentBar.Number
'            Dim n As Decimal = (endPointX) - startPointX
'            Dim a As Decimal, b As Decimal
'            Dim sumx2 As Decimal = 0, sumx As Decimal = 0, sumy As Decimal = 0, sumxy As Decimal = 0
'            For bari = startPointX To endPointX - 1
'                Dim point As New Point(bari, Chart.bars(bari).Data.Low + (Chart.bars(bari).Data.High - Chart.bars(bari).Data.Low) / 2)
'                sumx += point.X
'                sumy += point.Y
'                sumxy += point.X * point.Y
'                sumx2 += point.X ^ 2
'            Next
'            b = (n * sumxy - sumx * sumy) / (n * sumx2 - sumx * sumx)
'            a = (sumy - b * sumx) / n
'            Dim cdif As Decimal = cPoint.Y - (a + b * cPoint.X)
'            Dim adif As Decimal = aPoint.Y - (a + b * aPoint.X)
'            Dim dif As Decimal = adif - cdif
'            Return New Point(startPointX, aPoint.Y - dif)
'        End Function
'        Public Function CalculateNewStartPointRegressionMiddle(pointCollection As List(Of Point), startPointX As Decimal, endPointX As Decimal) As LineCoordinates
'            Dim n As Decimal = pointCollection.Count
'            Dim a As Decimal, b As Decimal
'            Dim sumx2 As Decimal = 0, sumx As Decimal = 0, sumy As Decimal = 0, sumxy As Decimal = 0
'            For Each point In pointCollection
'                sumx += point.X
'                sumy += point.Y
'                sumxy += point.X * point.Y
'                sumx2 += point.X ^ 2
'            Next
'            If n * sumx2 - sumx * sumx <> 0 Then
'                b = (n * sumxy - sumx * sumy) / (n * sumx2 - sumx * sumx)
'                a = (sumy - b * sumx) / n
'                Return New LineCoordinates(New Point(startPointX, a + b * startPointX), New Point(endPointX, a + b * endPointX))
'            Else
'                Return New LineCoordinates(New Point(startPointX, a + b * startPointX), New Point(endPointX, a + b * endPointX))
'            End If
'        End Function
'        Private Function CalculateNewStartPointRegressionMiddle(startPointX As Decimal, aPoint As Point, cPoint As Point, Optional endPointX As Decimal = -1) As LineCoordinates
'            If endPointX = -1 Then endPointX = CurrentBar.Number
'            Dim n As Decimal = (endPointX) - startPointX
'            Dim a As Decimal, b As Decimal
'            Dim sumx2 As Decimal = 0, sumx As Decimal = 0, sumy As Decimal = 0, sumxy As Decimal = 0
'            For bari = startPointX To endPointX - 1
'                Dim point As New Point(bari, Chart.bars(bari).Data.Low + (Chart.bars(bari).Data.High - Chart.bars(bari).Data.Low) / 2)
'                sumx += point.X
'                sumy += point.Y
'                sumxy += point.X * point.Y
'                sumx2 += point.X ^ 2
'            Next
'            If n * sumx2 - sumx * sumx <> 0 Then
'                b = (n * sumxy - sumx * sumy) / (n * sumx2 - sumx * sumx)
'                a = (sumy - b * sumx) / n
'                Return New LineCoordinates(New Point(aPoint.X, a + b * aPoint.X), New Point(cPoint.X, a + b * cPoint.X))
'            Else
'                Return New LineCoordinates(New Point(aPoint.X, a + b * aPoint.X), New Point(cPoint.X, a + b * cPoint.X))
'            End If
'        End Function

'        Private Function FindRangeBar(startBar As Integer, endBar As Integer, findHighestPoint As Boolean) As Point
'            Dim pnt As New Point(0, If(findHighestPoint, Double.MinValue, Double.MaxValue))
'            For i = startBar - 1 To endBar - 1
'                If findHighestPoint And Chart.bars(i).Data.High >= pnt.Y Then
'                    pnt = New Point(i, Chart.bars(i).Data.High)
'                ElseIf findHighestPoint = False And Chart.bars(i).Data.Low <= pnt.Y Then
'                    pnt = New Point(i, Chart.bars(i).Data.Low)
'                End If
'                If i = 787 Then
'                    Dim a As New Object
'                End If
'            Next
'            Return pnt
'        End Function

'        Private Sub ColorCurrentBars()
'            'If Mode = ModeType.AutoTrendFocused Then
'            '    barColorsDirty = True

'            '    If Not TypeOf Chart.bars(CurrentBar.Number - 1).Pen.Brush Is SolidColorBrush Then
'            '        Chart.bars(CurrentBar.Number - 1).Pen.Brush = New SolidColorBrush
'            '    End If
'            If ColorSwingBySwing Then
'                If curSwingEvent2 = SwingEvent.Extension Or curSwingEvent2 = SwingEvent.NewSwing Then
'                    If CurrentSwing2.Direction = Direction.Up Then
'                        BarColorRoutine(CurrentSwing2.StartBar, CurrentBar.Number - 1, UpColor)
'                    ElseIf CurrentSwing2.Direction = Direction.Down Then
'                        BarColorRoutine(CurrentSwing2.StartBar, CurrentBar.Number - 1, DownColor)
'                    End If
'                Else
'                    CType(Chart.bars(CurrentBar.Number - 1).Pen.Brush, SolidColorBrush).Color = NeutralColor
'                    RefreshObject(Chart.bars(CurrentBar.Number - 1))
'                End If
'            Else
'                CType(Chart.bars(CurrentBar.Number - 1).Pen.Brush, SolidColorBrush).Color = NeutralColor
'                RefreshObject(Chart.bars(CurrentBar.Number - 1))
'            End If
'            '    'Exit Sub

'            '    If curSwingEvent = SwingEvent.Extension Or curSwingEvent = SwingEvent.NewSwing Then
'            '        If ColorSwingBySwing Then
'            '            If CurrentSwing1.Direction = Direction.Up Then
'            '                BarColorRoutine(CurrentSwing1.StartBar, CurrentBar.Number - 1, UpTrendBarColor)
'            '            ElseIf CurrentSwing1.Direction = Direction.Down Then
'            '                BarColorRoutine(CurrentSwing1.StartBar, CurrentBar.Number - 1, DownTrendBarColor)
'            '            End If
'            '        Else
'            '            If CurrentSwing1.ChannelDirection = Swing.ChannelDirectionType.Up Then
'            '                BarColorRoutine(CurrentSwing1.StartBar, CurrentBar.Number - 1, UpTrendBarColor)
'            '            ElseIf CurrentSwing1.ChannelDirection = Swing.ChannelDirectionType.Neutral Then
'            '                BarColorRoutine(CurrentSwing1.StartBar, CurrentBar.Number - 1, NeutralBarColor)
'            '            ElseIf CurrentSwing1.ChannelDirection = Swing.ChannelDirectionType.Down Then
'            '                BarColorRoutine(CurrentSwing1.StartBar, CurrentBar.Number - 1, DownTrendBarColor)
'            '            End If
'            '        End If
'            '    End If
'            'Else
'            barColorsDirty = True
'            'Chart.bars(CurrentBar.Number - 1).Pen.Brush = New SolidColorBrush(CenterColor)
'            'If curSwingEvent <> SwingEvent.None Then
'            '    If CurrentSwing1.Direction = Direction.Up Then
'            '        Chart.bars(CurrentBar.Number - 1).Pen.Brush = New SolidColorBrush(UpColor)
'            '    Else
'            '        Chart.bars(CurrentBar.Number - 1).Pen.Brush = New SolidColorBrush(DownColor)
'            '    End If
'            'End If

'            'If Not regressionCenterPoints.ContainsKey(CurrentBar.Number) Then Exit Sub
'            'Dim point1 = regressionCenterPoints(CurrentBar.Number) + Abs(CurvedLineMultiplier * Chart.Settings("RangeValue").Value)
'            'Dim point2 = regressionCenterPoints(CurrentBar.Number) - Abs(CurvedLineMultiplier * Chart.Settings("RangeValue").Value)
'            'If Chart.bars(CurrentBar.Number - 1).Data.Low > point2 Then
'            '    Chart.bars(CurrentBar.Number - 1).Pen.Brush = GetGradientBarBrush(UpColor, NeutralColor, point1)
'            'Else
'            '    Chart.bars(CurrentBar.Number - 1).Pen.Brush = GetGradientBarBrush(NeutralColor, DownColor, point2)
'            'End If


'            'RefreshObject(Chart.bars(CurrentBar.Number - 1))

'            'End If
'        End Sub
'        Private Function GetGradientBarBrush(aboveColor As Color, belowColor As Color, point As Decimal) As LinearGradientBrush
'            Dim brush As New LinearGradientBrush With {.StartPoint = New Point(0, 0), .EndPoint = New Point(0, 1)}
'            If Chart.bars(CurrentBar.Number - 1).Data.High < point Then
'                brush.GradientStops.Add(New GradientStop(belowColor, 0.5))
'            ElseIf Chart.bars(CurrentBar.Number - 1).Data.Low > point Then
'                brush.GradientStops.Add(New GradientStop(aboveColor, 0.5))
'            Else
'                brush.GradientStops.Add(New GradientStop(aboveColor, 1 - (point - Chart.bars(CurrentBar.Number - 1).Data.Low) / Chart.bars(CurrentBar.Number - 1).Data.Range))
'                brush.GradientStops.Add(New GradientStop(belowColor, 1 - (point - Chart.bars(CurrentBar.Number - 1).Data.Low) / Chart.bars(CurrentBar.Number - 1).Data.Range))
'            End If
'            Return brush
'        End Function

'        Protected Sub BarColor(ByVal startBar As Integer, ByVal endBar As Integer, ByVal color As Color, swingColor As Color, direction As Swing.ChannelDirectionType, Optional onlyColorNeutral As Boolean = False, Optional neutralColor As Color = Nothing, Optional neutralBarColor As Color = Nothing)
'            Dim swingsToColor As New List(Of Swing)
'            'If ChannelMode = ChannelModeType.Swing Then
'            '    For Each swing In secondarySwings
'            '        If swing.StartBar >= startBar And swing.EndBar <= endBar + 1 Then
'            '            swing.TL.Pen.Brush = New SolidColorBrush(swingColor)
'            '        End If
'            '    Next
'            'End If
'            'If ColorSwingBySwing Then
'            '    For Each swing In swings1
'            '        If swing.StartBar >= startBar And swing.EndBar <= endBar + 1 Then
'            '            If onlyColorNeutral Then
'            '                If swing.ChannelDirection = AutoTrendBase.Swing.ChannelDirectionType.Neutral Then
'            '                    If Not ColorSwingBySwing Then CType(swing.TL.Pen.Brush, SolidColorBrush).Color = swingColor
'            '                    swing.ChannelDirection = direction
'            '                End If
'            '            Else
'            '                If Not ColorSwingBySwing Then swing.TL.Pen.Brush = New SolidColorBrush(swingColor)
'            '                swing.ChannelDirection = direction
'            '            End If
'            '        End If
'            '    Next
'            'Else
'            If Not ColorSwingBySwing Then
'                For i = startBar To endBar
'                    If Not TypeOf Chart.bars(i).Pen.Brush Is SolidColorBrush Then
'                        Chart.bars(i).Pen.Brush = New SolidColorBrush
'                    End If
'                    If CType(Chart.bars(i).Pen.Brush, SolidColorBrush).Color <> color Then
'                        If onlyColorNeutral Then
'                            If CType(Chart.bars(i).Pen.Brush, SolidColorBrush).Color = neutralBarColor Then
'                                Chart.bars(i).Pen.Brush = New SolidColorBrush(color)
'                                RefreshObject(Chart.bars(i))
'                            End If
'                        Else
'                            Chart.bars(i).Pen.Brush = New SolidColorBrush(color)
'                            RefreshObject(Chart.bars(i))
'                        End If
'                    End If
'                Next
'            End If
'        End Sub
'        Protected Sub BarColorRoutine(ByVal startBar As Integer, ByVal endBar As Integer, ByVal color As Color)
'            For i = startBar To endBar
'                If Not TypeOf Chart.bars(i).Pen.Brush Is SolidColorBrush Then
'                    Chart.bars(i).Pen.Brush = New SolidColorBrush
'                End If
'                If CType(Chart.bars(i).Pen.Brush, SolidColorBrush).Color <> color Then
'                    Chart.bars(i).Pen.Brush = New SolidColorBrush(color)
'                    RefreshObject(Chart.bars(i))
'                End If
'            Next
'        End Sub
'        Private Function GetProjectedLineBarSkip(priceDifference As Decimal) As Integer
'            Dim averageSwingAngle As Double = GetProjectionLineAngle()
'            Dim cs = CurrentSwing2
'            Dim multiplier = 4
'            Return (priceDifference / Chart.Settings("RangeValue").Value) * multiplier
'            If averageSwingAngle <> 0 And Double.IsNaN(averageSwingAngle) = False Then
'                'rvProNewLine.StartPoint = New Point(cs.EndBar, cs.EndPrice)
'                Dim newLineBarSkip As Integer
'                Dim extLineBarSkip As Integer
'                'If Chart.Settings("IsSlaveChart").Value Then
'                '    newLineBarSkip = 1
'                '    extLineBarSkip = 1
'                'Else
'                newLineBarSkip = Abs(CurrentBar.Close - (cs.EndPrice - BooleanToInteger(cs.Direction) * priceDifference)) / If(CustomRangeValue = -1, Chart.Settings("RangeValue").Value, CustomRangeValue) * 2
'                extLineBarSkip = Abs(CurrentBar.Close - cs.EndPrice) / If(CustomRangeValue = -1, Chart.Settings("RangeValue").Value, CustomRangeValue) * 2
'                'End If
'                ' Max(, (CurrentBar.Number + newLineBarSkip) - cs.EndBar)
'            Else
'                'Return 0
'            End If
'            Return (priceDifference * Chart.Settings("RangeValue").Value / 0.2) * multiplier
'        End Function
'        Private Sub UpdateHorizontalLines()
'            If curSwingEvent2 <> SwingEvent.None Then

'            End If
'        End Sub
'        Private Sub DrawProjectionLineAndRVText()
'            ' draw projection line
'            Dim cs = CurrentSwing2
'            Dim rv = Me.PriceRV
'            Dim averageSwingAngle As Double = GetProjectionLineAngle()
'            Dim endPoint As Decimal
'            If averageSwingAngle <> 0 Then
'                rvProNewLine.StartPoint = New Point(cs.EndBar, cs.EndPrice)
'                Dim endBar As Integer = CurrentBar.Number + 2 'GetProjectedLineBarSkip(Abs(CurrentBar.Close - (currentRegressionCenterPoint + If(swingDir = Direction.Up, -1, 1) * RVMultiplier * Chart.Settings("RangeValue").Value)))

'                'Dim maxRangePoint As Decimal = If(swingDir = Direction.Up, Decimal.MaxValue, 0)
'                'For i = cs.EndBar To CurrentBar.Number - 1
'                '    If (swingDir = Direction.Up And Chart.bars(i).Data.Low < maxRangePoint) Or (swingDir = Direction.Down And Chart.bars(i).Data.High > maxRangePoint) Then
'                '        maxRangePoint = If(swingDir = Direction.Up, Chart.bars(i).Data.Low, Chart.bars(i).Data.High)
'                '    End If
'                'Next
'                'If cs.HasCrossedRVBand Then
'                If swingDir2 = Direction.Up Then
'                    endPoint = CurrentSwing2.EndPoint.Y - rv '  Min(maxRangePoint, Max(cs.StartPrice, currentRegressionCenterPoint - RVMultiplier * Chart.Settings("RangeValue").Value))
'                Else
'                    endPoint = CurrentSwing2.EndPoint.Y + rv
'                End If
'                'Else
'                '    endPoint = cs.StartPrice
'                'End If
'                rvProNewLine.EndPoint = New Point(CurrentBar.Number + GetProjectedLineBarSkip(Abs(endPoint - CurrentBar.Close)), endPoint)
'                newSwingTextObject2.Location = New Point(CurrentBar.Number, rvProNewLine.EndPoint.Y)
'                newSwingTextObject2.Font.Brush = New SolidColorBrush(If(swingDir2 = Direction.Up, DownTrendChannelColor, UpTrendChannelColor))
'                If CurrentSwing2.EndPrice <> CurrentSwing2.StartPrice Then
'                    newSwingTextObject2.Text = newRVTextPrefix2 & Round(PriceRV / Abs(CurrentSwing2.EndPrice - CurrentSwing2.StartPrice) * 100) & "% " & FormatNumberLengthAndPrefix(PriceRV, Chart.Settings("DecimalPlaces").Value) & " RV"
'                End If
'                newSwingTextObject1.Location = New Point(CurrentBar.Number, If(swingDir1 = Direction.Up, CurrentSwing1.EndPoint.Y - RV1, CurrentSwing1.EndPoint.Y + RV1))
'                If swingDir1 = swingDir2 Then
'                    If CurrentSwing1.EndPrice <> CurrentSwing1.StartPrice Then
'                        newSwingTextObject1.Text = newRVTextPrefix1 & " RV " & FormatNumberLengthAndPrefix(RV1, Chart.Settings("DecimalPlaces").Value)
'                    End If
'                Else
'                    newSwingTextObject1.Text = ""
'                End If
'                'rvText.Location = New Point(CurrentBar.Number, rvProNewLine.EndPoint.Y)
'                averageSwingAngle = Atan(Abs(rvProNewLine.EndPoint.Y - rvProNewLine.StartPoint.Y) / Abs(rvProNewLine.EndPoint.X - rvProNewLine.StartPoint.X))
'                If swings2.Count > 1 Then
'                    If swings2(swings2.Count - 1).OverlapGapLine IsNot Nothing Then
'                        swings2(swings2.Count - 1).OverlapGapLine.EndPoint = New Point(endBar, swings2(swings2.Count - 1).OverlapGapLine.EndPoint.Y)
'                    End If
'                    If swings2.Count > 2 Then
'                        If swings2(swings2.Count - 2).OverlapGapLine IsNot Nothing AndAlso (Not swings2(swings2.Count - 2).OverlapGapLineGapped And
'                         Not swings2(swings2.Count - 2).OverlapGapLineDestroyed And swings2(swings2.Count - 2).OverlapGapLineHit) Then
'                            swings2(swings2.Count - 2).OverlapGapLine.EndPoint = New Point(endBar, swings2(swings2.Count - 2).OverlapGapLine.EndPoint.Y)
'                        End If
'                    End If
'                End If
'                ' move target texts along
'                For Each channel In regressionChannels
'                    For Each blength In channel.BCLengths
'                        If blength.TargetText IsNot Nothing Then
'                            Dim bcHeight As Decimal = blength.BCLength
'                            Dim yPoint As Double = channel.DPoint.Y - BooleanToInteger(channel.Direction) * bcHeight
'                            endBar = CurrentBar.Number + 1 ' GetProjectedLineBarSkip(Abs(yPoint - CurrentBar.Close))
'                            blength.TargetText.Location = New Point(endBar, yPoint)
'                            Dim maxBarPrice As Decimal = If(channel.Direction = Direction.Up, 0, Decimal.MaxValue)
'                            Dim maxBarIndex As Integer
'                            For i = channel.APoint.X To CurrentBar.Number - 1
'                                If (channel.Direction = Direction.Up And Chart.bars(i).Data.High >= maxBarPrice) Or (channel.Direction = Direction.Down And Chart.bars(i).Data.Low <= maxBarPrice) Then
'                                    maxBarPrice = If(channel.Direction = Direction.Up, Chart.bars(i).Data.High, Chart.bars(i).Data.Low)
'                                    maxBarIndex = i
'                                End If
'                            Next
'                            'blength.BCProjectionLine.StartPoint = New Point(maxBarIndex + 1, maxBarPrice)
'                            'blength.BCProjectionLine.EndPoint = New Point(endBar, yPoint)
'                        End If
'                    Next
'                Next
'                If SwingChannelLengthAndTargetTextsOn And CurrentSwing2.ABCLengths IsNot Nothing Then
'                    With CurrentSwing2
'                        For Each blength In CurrentSwing2.ABCLengths
'                            If blength.TargetText IsNot Nothing Then
'                                Dim bcHeight As Decimal = blength.BCLength
'                                Dim yPoint As Double = .EndPrice - BooleanToInteger(.Direction) * bcHeight
'                                endBar = CurrentBar.Number + 1 ' GetProjectedLineBarSkip(Abs(yPoint - CurrentBar.Close))
'                                blength.TargetText.Location = New Point(endBar, yPoint)
'                                Dim maxBarPrice As Decimal = If(.Direction = Direction.Up, 0, Decimal.MaxValue)
'                                Dim maxBarIndex As Integer
'                                For i = .StartBar To CurrentBar.Number - 1
'                                    If (.Direction = Direction.Up And Chart.bars(i).Data.High >= maxBarPrice) Or (.Direction = Direction.Down And Chart.bars(i).Data.Low <= maxBarPrice) Then
'                                        maxBarPrice = If(.Direction = Direction.Up, Chart.bars(i).Data.High, Chart.bars(i).Data.Low)
'                                        maxBarIndex = i
'                                    End If
'                                Next
'                                'blength.BCProjectionLine.StartPoint = New Point(maxBarIndex + 1, maxBarPrice)
'                                'blength.BCProjectionLine.EndPoint = New Point(endBar, yPoint)
'                            End If
'                        Next
'                    End With
'                End If
'                currentSwingBCTargetText.Text = ""

'            End If
'            'rvText.Text = rvTextPrefix & RemovePrefixZero(Round(Abs(CurrentSwing1.EndPoint.Y - endPoint), Chart.Settings("DecimalPlaces").Value)) & " RV" & predictionTextSuffix ' & Round(GetDollarValue(rv), Chart.Settings("DecimalPlaces").Value)
'            'rvText.Font.Brush = New SolidColorBrush(If(CurrentSwing1.Direction = Direction.Down, RVLineColor, RVLineColor))
'        End Sub


'        Private Function GetProjectionLineAngle() As Double
'            Return 75 * (PI / 180)
'            Dim swings1 As List(Of Swing)
'            swings1 = Me.swings2
'            If swings1.Count >= 5 Then
'                Dim averageCount As Integer = 4
'                Dim averageAngleCount As Double = 0
'                For i = swings1.Count - 1 To swings1.Count - averageCount Step -1
'                    averageAngleCount += Atan(Abs(swings1(i).TL.EndPoint.Y - swings1(i).TL.StartPoint.Y) / Abs(swings1(i).TL.EndPoint.X - swings1(i).TL.StartPoint.X))
'                Next
'                Return averageAngleCount / averageCount
'            Else
'                Return 0
'            End If
'            Return 0
'        End Function
'        Friend Overrides Function GetDollarValue(ByVal priceDif As Decimal) As Decimal
'            Dim multiplier As Double
'            If Chart.Settings("UseRandom").Value Then
'                multiplier = 1
'            Else
'                If Chart.IB.Contract(Chart.TickerID).Multiplier = "" OrElse Not IsNumeric(Chart.IB.Contract(Chart.TickerID).Multiplier) Then
'                    multiplier = 1
'                Else
'                    multiplier = Chart.IB.Contract(Chart.TickerID).Multiplier
'                End If
'            End If
'            Dim shares As Integer = Chart.Settings("DefaultOrderQuantity").Value
'            'Dim a = priceDif / Chart.GetMinTick * Chart.GetMinTick * multiplier * shares
'            Return Round(priceDif, Chart.Settings("DecimalPlaces").Value)
'        End Function
'        Friend Overrides Function GetRVFromDollar(ByVal dollarDif As Decimal) As Decimal
'            Dim multiplier As Double
'            If Chart.Settings("UseRandom").Value Then
'                multiplier = 1
'            Else
'                If Chart.IB.Contract(Chart.TickerID).Multiplier = "" OrElse Not IsNumeric(Chart.IB.Contract(Chart.TickerID).Multiplier) Then
'                    multiplier = 1
'                Else
'                    multiplier = Chart.IB.Contract(Chart.TickerID).Multiplier
'                End If
'            End If
'            Dim shares As Integer = Chart.Settings("DefaultOrderQuantity").Value
'            'Dim a = priceDif / Chart.GetMinTick * Chart.GetMinTick * multiplier * shares
'            Return Round(dollarDif, 6) 'Round(dollarDif / (multiplier * shares), 6)
'        End Function
'        Private Function NewSwing(ByVal color As Color, ByVal startPoint As Point, ByVal endPoint As Point, ByVal show As Boolean, ByVal direction As Direction) As Swing
'            Return New Swing(NewTrendLine(color, startPoint, endPoint, show), direction)
'        End Function


'        Private Function CalculateNewStartPointRegressionMiddle(startPointX As Decimal, lineStartPoint As Decimal, lineEndPoint As Decimal, Optional endPointX As Decimal = -1) As LineCoordinates
'            If endPointX = -1 Then endPointX = CurrentBar.Number
'            Dim n As Decimal = (endPointX) - startPointX
'            Dim a As Decimal, b As Decimal
'            Dim sumx2 As Decimal = 0, sumx As Decimal = 0, sumy As Decimal = 0, sumxy As Decimal = 0
'            For bari = startPointX To endPointX - 1
'                Dim point As New Point(bari, Chart.bars(bari).Data.Low + (Chart.bars(bari).Data.High - Chart.bars(bari).Data.Low) / 2)
'                sumx += point.X
'                sumy += point.Y
'                sumxy += point.X * point.Y
'                sumx2 += point.X ^ 2
'            Next
'0:          b = (n * sumxy - sumx * sumy) / (n * sumx2 - sumx * sumx)
'            a = (sumy - b * sumx) / n
'            Return New LineCoordinates(New Point(lineStartPoint, a + b * lineStartPoint), New Point(lineEndPoint, a + b * lineEndPoint))
'        End Function
'        'Private Function GetLength() As Integer
'        '    Return Round(CType(CType(slider.Content, Grid).Children(0), Slider).Value)
'        'End Function
'        Private newBarFlag As Boolean

'        Private Sub UpdateLineNewBar(startX As Integer, endX As Integer)
'            'regParams = CalculateNextRegressionFromParams(startX - 1, endX - 1, startX, endX, regParams)
'            Dim regressionCoor As LineCoordinates = regParams2.LineCoordinates
'            Dim location As Point = regParams2.LineCoordinates.EndPoint
'            If regParams2.Params Is Nothing Then Exit Sub
'            'If AddFillLines Then
'            '    location = New Point(location.X, regressionCenterPoints(regressionCenterPoints.Count - 1))
'            '    Dim abovePoint1 = location.Y + Abs(CurvedLine1Multiplier * Chart.Settings("RangeValue").Value)
'            '    Dim belowPoint1 = location.Y - Abs(CurvedLine1Multiplier * Chart.Settings("RangeValue").Value)
'            '    Dim abovePoint2 = location.Y + Abs(CurvedLine2Multiplier * Chart.Settings("RangeValue").Value)
'            '    Dim belowPoint2 = location.Y - Abs(CurvedLine2Multiplier * Chart.Settings("RangeValue").Value)
'            '    Dim abovePoint3 = location.Y + Abs(RVMultiplier * Chart.Settings("RangeValue").Value)
'            '    Dim belowPoint3 = location.Y - Abs(RVMultiplier * Chart.Settings("RangeValue").Value)
'            '    Dim offset = -1
'            '    If (Chart.bars(location.X + offset - 1).Data.High > location.Y Or Chart.bars(location.X + offset - 1).Data.Low < location.Y) Then
'            '        If Chart.bars(location.X + offset - 1).Data.High > location.Y Then
'            '            aboveFillPlot(1).AddNonContinuousPoint(AddToX(location, offset))
'            '            aboveFillPlot(1).AddPoint(New Point(location.X + offset, Min(Chart.bars(location.X + offset - 1).Data.High, abovePoint1)), False)
'            '        End If
'            '        If Chart.bars(location.X + offset - 1).Data.Low < location.Y Then
'            '            belowFillPlot(1).AddNonContinuousPoint(AddToX(location, offset))
'            '            belowFillPlot(1).AddPoint(New Point(location.X + offset, Max(Chart.bars(location.X + offset - 1).Data.Low, belowPoint1)), False)
'            '        End If
'            '    End If
'            '    If (Chart.bars(location.X + offset - 1).Data.High > abovePoint1 Or Chart.bars(location.X + offset - 1).Data.Low < belowPoint1) Then
'            '        If Chart.bars(location.X + offset - 1).Data.Avg > location.Y Then
'            '            aboveFillPlot(2).AddNonContinuousPoint(New Point(location.X + offset, abovePoint1))
'            '            aboveFillPlot(2).AddPoint(New Point(location.X + offset, Min(Chart.bars(location.X + offset - 1).Data.High, abovePoint2)), False)
'            '        Else
'            '            belowFillPlot(2).AddNonContinuousPoint(New Point(location.X + offset, belowPoint1))
'            '            belowFillPlot(2).AddPoint(New Point(location.X + offset, Max(Chart.bars(location.X + offset - 1).Data.Low, belowPoint2)), False)
'            '        End If
'            '    End If
'            '    If (Chart.bars(location.X + offset - 1).Data.High > abovePoint2 Or Chart.bars(location.X + offset - 1).Data.Low < belowPoint2) Then
'            '        If Chart.bars(location.X + offset - 1).Data.Avg > location.Y Then
'            '            aboveFillPlot(3).AddNonContinuousPoint(New Point(location.X + offset, abovePoint2))
'            '            aboveFillPlot(3).AddPoint(New Point(location.X + offset, Chart.bars(location.X + offset - 1).Data.High), False)
'            '        Else
'            '            belowFillPlot(3).AddNonContinuousPoint(New Point(location.X + offset, belowPoint2))
'            '            belowFillPlot(3).AddPoint(New Point(location.X + offset, Chart.bars(location.X + offset - 1).Data.Low), False)
'            '        End If
'            '    End If
'            '    If (Chart.bars(location.X + offset - 1).Data.High > abovePoint3 Or Chart.bars(location.X + offset - 1).Data.Low < belowPoint3) Then
'            '        If Chart.bars(location.X + offset - 1).Data.Avg > location.Y Then
'            '            aboveFillPlot(4).AddNonContinuousPoint(New Point(location.X + offset, abovePoint3))
'            '            aboveFillPlot(4).AddPoint(New Point(location.X + offset, Chart.bars(location.X + offset - 1).Data.High), False)
'            '        Else
'            '            belowFillPlot(4).AddNonContinuousPoint(New Point(location.X + offset, belowPoint3))
'            '            belowFillPlot(4).AddPoint(New Point(location.X + offset, Chart.bars(location.X + offset - 1).Data.Low), False)
'            '        End If
'            '    End If
'            'End If

'            location = regParams2.LineCoordinates.EndPoint

'            'If ShowCenterCurvedLine And (centerPlot.Points.Count = 0 OrElse centerPlot.Points(centerPlot.Points.Count - 1) <> location) Then centerPlot.AddPoint(location)
'            'If CurvedLinesOn Then

'            '    If (aboveCurvedLinePlots(1).Points.Count = 0 OrElse aboveCurvedLinePlots(1).Points(aboveCurvedLinePlots(1).Points.Count - 1) <> AddToXY(location, 0, Abs(CurvedLineMultiplier * Chart.Settings("RangeValue").Value))) Then
'            '        aboveCurvedLinePlots(1).AddPoint(AddToXY(location, 0, Abs(CurvedLineMultiplier * Chart.Settings("RangeValue").Value)))
'            '    End If
'            '    If (belowCurvedLinePlots(1).Points.Count = 0 OrElse belowCurvedLinePlots(1).Points(belowCurvedLinePlots(1).Points.Count - 1) <> AddToXY(location, 0, -Abs(CurvedLineMultiplier * Chart.Settings("RangeValue").Value))) Then
'            '        belowCurvedLinePlots(1).AddPoint(AddToXY(location, 0, -Abs(CurvedLineMultiplier * Chart.Settings("RangeValue").Value)))
'            '    End If

'            '    If (aboveCurvedLinePlots(2).Points.Count = 0 OrElse aboveCurvedLinePlots(2).Points(aboveCurvedLinePlots(2).Points.Count - 1) <> AddToXY(location, 0, Abs(CurvedLineMultiplier * 2 * Chart.Settings("RangeValue").Value))) Then
'            '        aboveCurvedLinePlots(2).AddPoint(AddToXY(location, 0, Abs(CurvedLineMultiplier * 2 * Chart.Settings("RangeValue").Value)))
'            '    End If
'            '    If (belowCurvedLinePlots(2).Points.Count = 0 OrElse belowCurvedLinePlots(2).Points(belowCurvedLinePlots(2).Points.Count - 1) <> AddToXY(location, 0, -Abs(CurvedLineMultiplier * 2 * Chart.Settings("RangeValue").Value))) Then
'            '        belowCurvedLinePlots(2).AddPoint(AddToXY(location, 0, -Abs(CurvedLineMultiplier * 2 * Chart.Settings("RangeValue").Value)))
'            '    End If

'            '    If (aboveCurvedLinePlots(3).Points.Count = 0 OrElse aboveCurvedLinePlots(3).Points(aboveCurvedLinePlots(3).Points.Count - 1) <> AddToXY(location, 0, Abs(CurvedLineMultiplier * 3 * Chart.Settings("RangeValue").Value))) Then
'            '        aboveCurvedLinePlots(3).AddPoint(AddToXY(location, 0, Abs(CurvedLineMultiplier * 3 * Chart.Settings("RangeValue").Value)))
'            '    End If
'            '    If (belowCurvedLinePlots(3).Points.Count = 0 OrElse belowCurvedLinePlots(3).Points(belowCurvedLinePlots(3).Points.Count - 1) <> AddToXY(location, 0, -Abs(CurvedLineMultiplier * 3 * Chart.Settings("RangeValue").Value))) Then
'            '        belowCurvedLinePlots(3).AddPoint(AddToXY(location, 0, -Abs(CurvedLineMultiplier * 3 * Chart.Settings("RangeValue").Value)))
'            '    End If

'            '    If (aboveCurvedLinePlots(4).Points.Count = 0 OrElse aboveCurvedLinePlots(4).Points(aboveCurvedLinePlots(4).Points.Count - 1) <> AddToXY(location, 0, Abs(CurvedLineMultiplier * 4 * Chart.Settings("RangeValue").Value))) Then
'            '        aboveCurvedLinePlots(4).AddPoint(AddToXY(location, 0, Abs(CurvedLineMultiplier * 4 * Chart.Settings("RangeValue").Value)))
'            '    End If
'            '    If (belowCurvedLinePlots(4).Points.Count = 0 OrElse belowCurvedLinePlots(4).Points(belowCurvedLinePlots(4).Points.Count - 1) <> AddToXY(location, 0, -Abs(CurvedLineMultiplier * 4 * Chart.Settings("RangeValue").Value))) Then
'            '        belowCurvedLinePlots(4).AddPoint(AddToXY(location, 0, -Abs(CurvedLineMultiplier * 4 * Chart.Settings("RangeValue").Value)))
'            '    End If
'            'End If



'            'If (rvTopLinePlot.Points.Count = 0 OrElse rvTopLinePlot.Points(rvTopLinePlot.Points.Count - 1) <> AddToXY(location, 0, Abs(RVMultiplier * Chart.Settings("RangeValue").Value))) Then
'            '    rvTopLinePlot.AddPoint(AddToXY(location, 0, Abs(RVMultiplier * Chart.Settings("RangeValue").Value)))
'            'End If
'            'If (rvBotLinePlot.Points.Count = 0 OrElse rvBotLinePlot.Points(rvBotLinePlot.Points.Count - 1) <> AddToXY(location, 0, -Abs(RVMultiplier * Chart.Settings("RangeValue").Value))) Then
'            '    rvBotLinePlot.AddPoint(AddToXY(location, 0, -Abs(RVMultiplier * Chart.Settings("RangeValue").Value)))
'            'End If
'            If (curvedTopLinePlot.Points.Count = 0 OrElse curvedTopLinePlot.Points(curvedTopLinePlot.Points.Count - 1) <> AddToXY(location, 0, Abs(CurvedLineMultiplier * Chart.Settings("RangeValue").Value))) Then
'                curvedTopLinePlot.AddPoint(AddToXY(location, 0, Abs(CurvedLineMultiplier * Chart.Settings("RangeValue").Value)))
'            End If
'            If (curvedBotLinePlot.Points.Count = 0 OrElse curvedBotLinePlot.Points(curvedBotLinePlot.Points.Count - 1) <> AddToXY(location, 0, -Abs(CurvedLineMultiplier * Chart.Settings("RangeValue").Value))) Then
'                curvedBotLinePlot.AddPoint(AddToXY(location, 0, -Abs(CurvedLineMultiplier * Chart.Settings("RangeValue").Value)))
'            End If


'            newBarFlag = True
'        End Sub
'        Private Sub UpdateLine(startX As Integer, endX As Integer)
'            If newBarFlag Or Not IsLastBarOnChart Then

'            End If
'            newBarFlag = False
'        End Sub


'        Private Function CalculateRegressionPoint(startPointX As Decimal, endPointX As Decimal, lineEndPointX As Decimal) As Point
'            If endPointX = -1 Then endPointX = CurrentBar.Number - 1
'            Dim n As Decimal = (endPointX + 1) - startPointX
'            Dim a As Decimal, b As Decimal
'            Dim sumx2 As Decimal = 0, sumx As Decimal = 0, sumy As Decimal = 0, sumxy As Decimal = 0
'            For bari = startPointX To endPointX
'                Dim point As New Point(bari, Chart.bars(bari).Data.Low + (Chart.bars(bari).Data.High - Chart.bars(bari).Data.Low) / 2)
'                sumx += point.X
'                sumy += point.Y
'                sumxy += point.X * point.Y
'                sumx2 += point.X ^ 2
'            Next
'            b = (n * sumxy - sumx * sumy) / (n * sumx2 - sumx * sumx)
'            a = (sumy - b * sumx) / n
'            Return New Point(lineEndPointX, a + b * lineEndPointX)
'        End Function
'        ''' <summary>
'        ''' calculates the current regression fresh if no params are given.
'        ''' </summary>
'        Private Function CalculateNextRegressionFromParams(startPointX As Decimal, endPointX As Decimal, lineStartPoint As Decimal, lineEndPoint As Decimal, Optional params As RegressionParams? = Nothing) As RegressionParams
'            If CurvedLineFormula = CurveFormula.Regression Then
'                Dim n As Decimal = (endPointX + 1) - startPointX, a As Decimal, b As Decimal, sumx2 As Decimal = 0, sumx As Decimal = 0, sumy As Decimal = 0, sumxy As Decimal = 0
'                If params Is Nothing OrElse params.Value.Params Is Nothing Then
'                    params = New RegressionParams With {.Params = CalculateRegressionParams(startPointX, endPointX)}
'                    sumx = params.Value.Params(0) : sumy = params.Value.Params(1) : sumxy = params.Value.Params(2) : sumx2 = params.Value.Params(3)
'                Else
'                    Dim pointToRemove As New Point(startPointX - 1, Chart.bars(startPointX - 1).Data.Avg)
'                    Dim pointToAdd As New Point(endPointX, Chart.bars(endPointX).Data.Avg)
'                    sumx = params.Value.Params(0) - pointToRemove.X + pointToAdd.X
'                    sumy = params.Value.Params(1) - pointToRemove.Y + pointToAdd.Y
'                    sumxy = params.Value.Params(2) - pointToRemove.X * pointToRemove.Y + pointToAdd.X * pointToAdd.Y
'                    sumx2 = params.Value.Params(3) - pointToRemove.X ^ 2 + pointToAdd.X ^ 2
'                End If
'                b = (n * sumxy - sumx * sumy) / (n * sumx2 - sumx * sumx)
'                a = (sumy - b * sumx) / n
'                Return New RegressionParams() With {.LineCoordinates = New LineCoordinates(New Point(lineStartPoint, a + b * lineStartPoint), New Point(lineEndPoint, a + b * lineEndPoint)), .Params = {sumx, sumy, sumxy, sumx2}}
'            Else
'                Dim n As Decimal = (endPointX + 1) - startPointX
'                If params Is Nothing OrElse params.Value.Params Is Nothing Then
'                    Dim barTotal As Decimal = 0
'                    For bari = startPointX To endPointX
'                        barTotal += Chart.bars(bari).Data.Avg
'                    Next
'                    Return New RegressionParams With {.LineCoordinates = New LineCoordinates(New Point(lineStartPoint, barTotal / n), New Point(lineEndPoint, barTotal / n)), .Params = {barTotal}}
'                Else
'                    Dim pointToRemove As New Point(startPointX - 1, Chart.bars(startPointX - 1).Data.Avg)
'                    Dim pointToAdd As New Point(endPointX, Chart.bars(endPointX).Data.Avg)
'                    Dim newTotal As Decimal = params.Value.Params(0) - pointToRemove.Y + pointToAdd.Y
'                    Return New RegressionParams With {.LineCoordinates = New LineCoordinates(New Point(lineStartPoint, newTotal / n), New Point(lineEndPoint, newTotal / n)), .Params = {newTotal}}
'                End If
'            End If
'        End Function
'        Private Function CalculateRegressionParams(startPointX As Decimal, endPointX As Decimal) As Decimal()
'            If endPointX = -1 Then endPointX = CurrentBar.Number - 1
'            Dim n As Decimal = (endPointX + 1) - startPointX
'            Dim sumx2 As Decimal = 0, sumx As Decimal = 0, sumy As Decimal = 0, sumxy As Decimal = 0
'            For bari = startPointX To endPointX
'                Dim point As New Point(bari, Chart.bars(bari).Data.Low + (Chart.bars(bari).Data.High - Chart.bars(bari).Data.Low) / 2)
'                sumx += point.X
'                sumy += point.Y
'                sumxy += point.X * point.Y
'                sumx2 += point.X ^ 2
'            Next
'            Return {sumx, sumy, sumxy, sumx2}
'        End Function
'        Private Structure RegressionParams
'            Public Property LineCoordinates As LineCoordinates
'            Public Property Params As Decimal()
'        End Structure



'    End Class

'End Namespace


