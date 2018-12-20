'Imports System.Threading

'Namespace AnalysisTechniques

'    Public Class BarsRVAutoTrend

'#Region "AnalysisTechnique Inherited Code"
'        Inherits AutoTrendBase
'        Public Sub New(ByVal chart As Chart)
'            MyBase.New(chart) ' Call the base class constructor.
'            Description = "Main AutoTrend analysis technique."
'            Name = "BarsRVAutoTrend"
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
'        Friend Overrides Sub RVUp()
'            Dim shortest As Decimal = Decimal.MaxValue
'            Dim nextShortest As Decimal = Decimal.MaxValue
'            For Each swing In swings
'                If Abs(swing.StartPrice - swing.EndPrice) < shortest And Abs(swing.StartPrice - swing.EndPrice) >= PriceRV Then
'                    shortest = Round(Abs(swing.StartPrice - swing.EndPrice), 3)
'                End If
'            Next
'            If PriceRV <> shortest Then
'                bcLengths.Add(PriceRV)
'                PriceRV = shortest
'                Chart.ReApplyAnalysisTechnique(Me)
'                'MsgBox("")
'            Else
'                For Each swing In swings
'                    Dim height As Decimal = Abs(swing.StartPrice - swing.EndPrice)
'                    If height > shortest Then
'                        nextShortest = Min(nextShortest, height)
'                    End If
'                Next
'                bcLengths.Add(PriceRV)
'                PriceRV = nextShortest
'                Chart.ReApplyAnalysisTechnique(Me)
'            End If
'            'For i = channels.Count - 1 To 0 Step -1
'            '    Dim channelLine As Channel = channels(i)
'            '    If channelLine.IsConfirmed And Not channelLine.IsCancelled Then
'            '        bcLengths.Add(RV)
'            '        If RV = Round(Abs(CurrentSwing.StartPrice - CurrentSwing.EndPrice), 5) Then
'            '            RV = Round(Abs(CurrentSwing.StartPrice - CurrentSwing.EndPrice) + Chart.GetMinTick, 5)
'            '        ElseIf Round(Abs(CurrentSwing.StartPrice - CurrentSwing.EndPrice), 5) < Round(Abs(channelLine.BCSwingLine.EndPoint.Y - channelLine.BCSwingLine.StartPoint.Y), 5) Then
'            '            RV = Round(Abs(CurrentSwing.StartPrice - CurrentSwing.EndPrice), 5)
'            '        ElseIf RV = Round(Abs(channelLine.BCSwingLine.EndPoint.Y - channelLine.BCSwingLine.StartPoint.Y), 5) Then
'            '            RV = Round(Abs(channelLine.BCSwingLine.EndPoint.Y - channelLine.BCSwingLine.StartPoint.Y) + Chart.GetMinTick, 5)
'            '        Else
'            '            RV = Round(Abs((channelLine.BCSwingLine.EndPoint.Y - channelLine.BCSwingLine.StartPoint.Y)), 5)
'            '        End If
'            '        Chart.ReApplyAnalysisTechnique(Me)
'            '        Exit For
'            '    End If
'            'Next
'        End Sub

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
'                End If
'            End If
'        End Sub
'#End Region

'#Region "Inputs"

'        Private _rv As Decimal = 2
'        '<Input("The reversal value.", "General")>
'        Public Overrides Property PriceRV As Decimal
'            Get
'                Return _rv
'            End Get
'            Set(value As Decimal)
'                _rv = Round(value, 7)
'                'If Chart IsNot Nothing Then
'                '    Chart.UpdateRangePad(Me)
'                'End If
'            End Set
'        End Property

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
'        <Input()> Public Property BarProcessCount As Integer = 700
'        <Input()> Public Property RVBarCount As Integer = 10
'        Public Property CurvedLineMultiplier As Decimal = 0
'        Public Property CurvedLineThickness As Decimal = 0
'        Public Property CurvedLineColor As Color = Colors.Black
'        '<Input()> Public Property RVTextFontSize As Decimal = 12
'        '<Input()> Public Property RVTextFontWeight As FontWeight = FontWeights.Bold
'        '<Input(, "Curved Lines")>
'        'Public Property CurvedLinesOn As Boolean = True
'        '<Input()> Public Property CurvedLineMultiplier As Decimal = 0.4
'        '<Input()> Public Property RVLineColor As Color = Colors.Brown
'        '<Input()> Public Property CurvedLineColor As Color = Colors.Blue
'        '<Input()> Public Property CurvedLineThickness As Decimal = 0.4
'        '<Input()> Public Property CenterLineThickness As Decimal = 0.4
'        '<Input()> Public Property OuterLineColor As Color = Colors.Brown
'        '<Input()> Public Property OuterLineThickness As Decimal = 0.4
'        '<Input()> Public Property WideLengthMultiplier As Decimal = 0
'        '<Input()> Public Property ShortLengthMultiplier As Decimal = 0

'        <Input(, "Bar Coloring")>
'        Public Property UpColor As Color = (New ColorConverter).ConvertFrom("#2000CC00")
'        <Input()> Public Property NeutralColor As Color = (New ColorConverter).ConvertFrom("#200000FF")
'        <Input()> Public Property DownColor As Color = (New ColorConverter).ConvertFrom("#20FF0000")

'        <Input(, "Trend Channels")>
'        Public Property TrendChannelOn As Boolean = True
'        <Input()> Public Property TrendChannelUpColor As Color = Colors.Green
'        <Input()> Public Property TrendChannelDownColor As Color = Colors.Red
'        <Input()> Public Property UpTrendChannelNeutralColor As Color = Colors.Black
'        <Input()> Public Property DownTrendChannelNeutralColor As Color = Colors.Black
'        <Input()> Public Property TrendChannelThickness As Decimal = 1.6
'        <Input()> Public Property TrendChannelCenterLineThickness As Decimal = 1.6
'        <Input()> Public Property PushTextFontSize As Double = 22
'        <Input()> Public Property TrendTextFontSize As Double = 14
'        <Input()> Public Property TrendTextFontWeight As FontWeight = FontWeights.Bold

'        <Input(, "Swing Channels")>
'        Public Property SwingChannelOn As Boolean = True
'        <Input()> Public Property UpSwingChannelColor As Color = Colors.Black
'        <Input()> Public Property DownSwingChannelColor As Color = Colors.Black
'        <Input()> Public Property SwingChannelThickness As Decimal = 1.4
'        <Input()> Public Property SwingChannelHistoryOn As Boolean = True
'        <Input()> Public Property SwingChannelHistoryThickness As Decimal = 1.1
'        Public Property SwingChannelLengthAndTargetTextsOn As Boolean = True


'        <Input(, "Swing Lines")>
'        Public Property SwingLinesOn As Boolean = True
'        <Input()> Public Property SwingLineThickness As Decimal = 0.8
'        <Input()> Public Property LastSwingLineOn As Boolean = False
'        Public Property ProjectionLinesOn As Boolean = True
'        Public Property ProjectionLineColor As Color = Colors.BlueViolet
'        Public Property ProjectionLineThickness As Decimal = 0.4

'        <Input(, "Swing Text")>
'        Public Property LengthTextFontSize As Double = 14
'        <Input()> Public Property LengthTextFontWeight As FontWeight = FontWeights.Bold
'        Public Property BarsBackTextColor As Color = Colors.Blue
'        'Public Property TargetTextFontSize As Double = 14
'        'Public Property TargetTextFontWeight As FontWeight = FontWeights.Bold
'        Public Property SwingTextsOn As Boolean = True

'        '<Input(, "Bar Coloring")>
'        'Public Property AboveBarColor As Color = Colors.Green
'        '<Input()> Public Property BelowBarColor As Color = Colors.Red

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
'        Public Property RVTextColor As Color = Colors.BlueViolet
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
'        '<Input("The option to color the swings with alternating colors.")>
'        Public Property ColorSwingBySwing As Boolean = False


'        Public Property ConfirmedUpChannelLineColor As Color = Colors.Green
'        '<Input("The color for confirmed down channel lines.")>
'        Public Property ConfirmedDownChannelLineColor As Color = Colors.Red

'        Public Property NeutralBarColor As Color = Colors.Black
'        '<Input("The color for the bars during an up trend.")>
'        Public Property UpTrendBarColor As Color = Colors.Green
'        '<Input("The color for the bars during a down trend.")>
'        Public Property DownTrendBarColor As Color = Colors.Red


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

'        Private currentGapMark As GapMarker

'        Private barColorsDirty As Boolean

'        Private swings As List(Of Swing)
'        'Private channels As List(Of Channel)
'        Private coloringChannels As List(Of Channel)
'        'Private swingChannel As SwingChannelLine
'        'Private swingChannels As List(Of SwingChannelLine)
'        Private currentSwingBCLengthText As Label
'        'Private currentSwingBCTargetText As Label
'        'Private currentSwingPotentialBCTargetText As Label
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
'        Dim currentSwingChannel As TrendLine
'        Dim regParams As RegressionParams
'        Dim regParams2 As RegressionParams
'        'Const predictionTextSuffix As String = "────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────"
'        'Const predictionTextPrefix As String = "── "
'        Const rvTextPrefix As String = "───── "
'        Dim targetTextBarOffset As Integer = 7
'        Dim newSwingTextBarOffset As Integer = 15
'        Private extendTrendTextObject As Label
'        Private newTrendTextObject As Label
'        Private extendSwingTextObject As Label
'        Private newSwingTextObject As Label
'        Dim processButtonUI As UIChartControl
'        Dim originalLocation2 As New Point(40, 40)
'        Dim contentButton As Button


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

'        Private ReadOnly Property CurrentSwing As Swing
'            Get
'                Return swings(swings.Count - 1)
'            End Get
'        End Property
'        Private ReadOnly Property CurrentSwing(barsBack As Integer) As Swing
'            Get
'                If swings.Count > barsBack Then
'                    Return swings(swings.Count - barsBack - 1)
'                Else
'                    Return Nothing
'                End If
'            End Get
'        End Property
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
'            'rvTopLinePlot = NewPlot(RVLineColor)
'            'rvTopLinePlot.IsSelectable = False
'            'rvBotLinePlot = NewPlot(RVLineColor)
'            'rvBotLinePlot.IsSelectable = False
'            'rvTopLinePlot.Pen.Thickness = RVLineThickness
'            'rvBotLinePlot.Pen.Thickness = RVLineThickness

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
'            swings = New List(Of Swing)
'            swings.Add(New Swing(NewTrendLine(UpSwingChannelColor,
'                      New Point(CurrentBar.Number, CurrentBar.Close),
'                      New Point(CurrentBar.Number, CurrentBar.Close),
'                      SwingLinesOn), Direction.Down))
'            CurrentSwing.TL.Pen.Thickness = SwingLineThickness
'            CurrentSwing.TL.IsSelectable = False
'            CurrentSwing.TL.IsEditable = False

'            'currentSwingLengthTexts = New List(Of Label)
'            'currentSwingLengthTexts.Add(NewLabel(GetDollarValue(Abs(swings(swings.Count - 1).StartPrice - swings(swings.Count - 1).EndPrice)), Colors.Black, New Point(swings(swings.Count - 1).EndBar, swings(swings.Count - 1).EndPrice), True, , False))
'            'currentSwingLengthTexts(currentSwingLengthTexts.Count - 1).Font.FontSize = LengthTextFontSize
'            'currentSwingLengthTexts(currentSwingLengthTexts.Count - 1).Font.FontWeight = LengthTextFontWeight

'            currentSwingBarsBackTexts = New List(Of Label)
'            currentSwingBarsBackTexts.Add(NewLabel(GetDollarValue(Abs(swings(swings.Count - 1).StartPrice - swings(swings.Count - 1).EndPrice)), Colors.Black, New Point(swings(swings.Count - 1).EndBar, swings(swings.Count - 1).EndPrice), True, , False))
'            currentSwingBarsBackTexts(currentSwingBarsBackTexts.Count - 1).Font.FontSize = LengthTextFontSize
'            currentSwingBarsBackTexts(currentSwingBarsBackTexts.Count - 1).Font.FontWeight = LengthTextFontWeight

'            regressionCenterPoints = New Dictionary(Of Integer, Decimal)

'            currentChannelEndSwingIndex = 0
'            currentChannelBeginSwingIndex = 0
'            currentChannelUnmerged = Nothing

'            extendTrendTextObject = NewLabel(rvTextPrefix & "Extend Trend", Colors.Black, New Point(0, 0), False, , False)
'            newTrendTextObject = NewLabel(rvTextPrefix & "New Trend", Colors.Black, New Point(0, 0), True, , False)
'            extendTrendTextObject.Font.FontSize = TrendTextFontSize
'            extendTrendTextObject.Font.FontWeight = TrendTextFontWeight
'            newTrendTextObject.Font.FontSize = TrendTextFontSize
'            newTrendTextObject.Font.FontWeight = TrendTextFontWeight
'            extendSwingTextObject = NewLabel(rvTextPrefix & "Extend Swing", Colors.Black, New Point(0, 0), True, , False)
'            newSwingTextObject = NewLabel("New Swing " & RVBarCount, Colors.Black, New Point(0, 0), True, , False)

'            extendSwingTextObject.Font.FontSize = TrendTextFontSize
'            extendSwingTextObject.Font.FontWeight = TrendTextFontWeight
'            newSwingTextObject.Font.FontSize = TrendTextFontSize
'            newSwingTextObject.Font.FontWeight = TrendTextFontWeight

'            'rvText = NewLabel("", RVLineColor, New Point(0, 0))
'            'rvText.Font.FontSize = RVTextFontSize
'            'rvText.Font.FontWeight = RVTextFontWeight
'            'rvText.VerticalAlignment = LabelVerticalAlignment.Center
'            'rvText.HorizontalAlignment = LabelHorizontalAlignment.Left
'            'rvProNewLine = NewTrendLine(ProjectionLineColor, New Point(0, 0), New Point(0, 0), ProjectionLinesOn)
'            'rvProNewLine.Pen.Thickness = ProjectionLineThickness
'            'rvProNewLine.Pen.DashStyle = TrendLineDashStyle
'            'rvProNewLine.IsSelectable = False
'            'rvProNewLine.IsEditable = False

'            barsBackMark = NewTrendLine(Colors.Black)
'            barsBackMark.Pen.Thickness = 1
'            barsBackMark.IsSelectable = False
'            barsBackMark.IsEditable = False

'            'currentSwingBCTargetText = NewLabel("", RVTextColor, New Point(0, 0), True, , False)
'            'currentSwingBCTargetText.HorizontalAlignment = LabelHorizontalAlignment.Left
'            'currentSwingBCTargetText.VerticalAlignment = LabelVerticalAlignment.Center
'            'currentSwingBCTargetText.Font.FontSize = TargetTextFontSize
'            'currentSwingBCTargetText.Font.FontWeight = TargetTextFontWeight
'            'currentSwingBCTargetText.Font.Brush = New SolidColorBrush(SwingChannelBCLengthTextColor)
'            'currentSwingPotentialBCTargetText = NewLabel("", Colors.Black, New Point(0, 0), True, , False)
'            'currentSwingPotentialBCTargetText.HorizontalAlignment = LabelHorizontalAlignment.Right
'            'currentSwingPotentialBCTargetText.VerticalAlignment = LabelVerticalAlignment.Center
'            'currentSwingPotentialBCTargetText.Font.FontSize = TargetTextFontSize
'            'currentSwingPotentialBCTargetText.Font.FontWeight = TargetTextFontWeight
'            'currentSwingPotentialBCTargetText.Font.Brush = New SolidColorBrush(SwingChannelBCLengthTextColor)
'            currentPartiallyCutChannel = Nothing

'            If CustomRangeValue = -1 Then
'                swingCountGraphYPlacement = CurrentBar.Close + Chart.Settings("RangeValue").Value * 20
'            Else
'                swingCountGraphYPlacement = CurrentBar.Close + CustomRangeValue * 20
'            End If
'            currentSwingCountGraphLine = NewTrendLine(UpSwingChannelColor, New Point(CurrentBar.Number, swingCountGraphYPlacement), New Point(CurrentBar.Number, swingCountGraphYPlacement))
'            currentSwingCountGraphLine.Pen.Thickness = 10
'            currentSwingCountGraphLine.IsSelectable = False
'            currentSwingCountGraphLine.IsEditable = False
'            potentialRegressionChannel = Nothing
'            potentialSameDirRegressionChannel = Nothing

'            curSwingEvent = SwingEvent.None

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
'            currentSwingChannel = NewTrendLine(UpSwingChannelColor)
'            currentSwingChannel.IsRegressionLine = True
'            currentSwingChannel.OuterPen = New Pen(New SolidColorBrush(UpSwingChannelColor), SwingChannelThickness)
'            currentSwingChannel.Pen = New Pen(New SolidColorBrush(UpSwingChannelColor), 0)
'            currentSwingChannel.UseNegativeCoordinates = True
'            currentSwingChannel.HasParallel = True
'            currentSwingChannel.IsSelectable = False
'            currentSwingChannel.IsEditable = False
'            currentSwingChannel.ExtendRight = True

'            regParams = Nothing
'            regParams2 = Nothing

'            For i = 0 To 1
'                momentumBarsOffsetLines(i) = NewTrendLine(MomentumOffsetLineColor, MomentumBarsVisible)
'                momentumBarsOffsetLines(i).Pen = New Pen(New SolidColorBrush(MomentumOffsetLineColor), MomentumOffsetLineThickness)
'                momentumBarsRVLines(i) = NewTrendLine(Colors.Black, MomentumBarsVisible)
'                momentumBarsRVLines(i).Pen = New Pen(New SolidColorBrush(Colors.Black), 1)
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

'        Dim curSwingEvent As SwingEvent = SwingEvent.None
'        Dim swingDir As Direction
'        Dim curRVPnt As Point
'        'Dim currentSwingLengthTexts As List(Of Label)
'        Dim currentSwingBarsBackTexts As List(Of Label)
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
'        Private Function GetBarsBackNumber(dir As Direction, Optional point As Point? = Nothing) As Integer
'            Dim val As Decimal = If(point Is Nothing, If(dir = Direction.Up, CurrentBar.High, CurrentBar.Low), point.Value.Y)
'            Dim valX As Integer = If(point Is Nothing, CurrentBar.Number, point.Value.X)
'            For i = valX - 2 To 0 Step -1
'                If (dir = Direction.Up And Chart.bars(i).Data.High > val) Or (dir = Direction.Down And Chart.bars(i).Data.Low < val) Then
'                    Return CStr(valX - 1 - i)
'                End If
'            Next
'            Return -1
'        End Function
'        Protected Overrides Sub Main()

'            If IsLastBarOnChart Then
'                timeLabel.Location = Chart.GetRelativeFromReal(SwingCountTextLocation)
'                timeLabel.Text = swings.Count
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


'            Dim cs As Swing = CurrentSwing
'            curSwingEvent = SwingEvent.None
'            If (cs.Direction = Direction.Down And (CurrentBar.High >= cs.StartPrice Or (CurrentBar.High >= MaxOrMinOfBarsBack(CurrentBar.Number - cs.EndBar, cs.Direction = Direction.Down).Y And CurrentBar.Number - cs.EndBar >= RVBarCount))) OrElse
'                (cs.Direction = Direction.Up And (CurrentBar.Low <= cs.StartPrice Or (CurrentBar.Low <= MaxOrMinOfBarsBack(CurrentBar.Number - cs.EndBar, cs.Direction = Direction.Down).Y And CurrentBar.Number - cs.EndBar >= RVBarCount))) Then
'                'new swing
'                'Dim canContinue As Boolean = True
'                'For i = cs.EndBar To CurrentBar.Number - 2
'                '    If (cs.Direction = Direction.Down And Chart.bars(i).Data.High > CurrentBar.High) Or (cs.Direction = Direction.Up And Chart.bars(i).Data.Low < CurrentBar.Low) Then
'                '        canContinue = False
'                '        Exit For
'                '    End If
'                'Next
'                'If canContinue Then
'                swings.Add(NewSwing(If(ColorSwingBySwing, If(cs.Direction = Direction.Down, UpTrendSwingLineColor, DownTrendSwingLineColor), If(cs.Direction = Direction.Down, UpSwingChannelColor, DownSwingChannelColor)), New Point(cs.EndBar, cs.EndPrice),
'                                    New Point(CurrentBar.Number, If(cs.Direction = Direction.Down, CurrentBar.High, CurrentBar.Low)), True And SwingLinesOn, Not cs.Direction))
'                If swings.Count > 1 And SwingLinesOn Then
'                    If LastSwingLineOn Then
'                        swings(swings.Count - 1).TL.Pen.Thickness = SwingLineThickness
'                        swings(swings.Count - 2).TL.Pen.Thickness = SwingLineThickness
'                    Else
'                        swings(swings.Count - 1).TL.Pen.Thickness = 0
'                        swings(swings.Count - 2).TL.Pen.Thickness = SwingLineThickness
'                    End If
'                End If
'                swings(swings.Count - 1).TL.IsSelectable = False
'                swings(swings.Count - 1).TL.IsEditable = False
'                curSwingEvent = AutoTrendBase.SwingEvent.NewSwing
'                swingDir = Not cs.Direction

'                'currentSwingLengthTexts.Add(NewLabel(RemovePrefixZero(GetDollarValue(Abs(swings(swings.Count - 1).StartPrice - swings(swings.Count - 1).EndPrice))) & " ", If(swingDir = Direction.Up, UpSwingChannelColor, DownSwingChannelColor), New Point(swings(swings.Count - 1).EndBar - 1, swings(swings.Count - 1).EndPrice), SwingTextsOn, , False))
'                'currentSwingLengthTexts(currentSwingLengthTexts.Count - 1).Font.FontSize = LengthTextFontSize
'                'currentSwingLengthTexts(currentSwingLengthTexts.Count - 1).Font.FontWeight = LengthTextFontWeight
'                'currentSwingLengthTexts(currentSwingLengthTexts.Count - 1).HorizontalAlignment = LabelHorizontalAlignment.Right
'                'currentSwingLengthTexts(currentSwingLengthTexts.Count - 1).VerticalAlignment = LabelVerticalAlignment.Center
'                Dim v As Integer = GetBarsBackNumber(swingDir)
'                swings(swings.Count - 1).BarsBack = v
'                currentSwingBarsBackTexts.Add(NewLabel(swings(swings.Count - 1).EndBar - swings(swings.Count - 1).StartBar, BarsBackTextColor, New Point(swings(swings.Count - 1).EndBar - 1, swings(swings.Count - 1).EndPrice), SwingTextsOn, ))
'                currentSwingBarsBackTexts(currentSwingBarsBackTexts.Count - 1).Font.FontSize = LengthTextFontSize
'                currentSwingBarsBackTexts(currentSwingBarsBackTexts.Count - 1).Font.FontWeight = LengthTextFontWeight
'                currentSwingBarsBackTexts(currentSwingBarsBackTexts.Count - 1).HorizontalAlignment = LabelHorizontalAlignment.Right
'                currentSwingBarsBackTexts(currentSwingBarsBackTexts.Count - 1).VerticalAlignment = LabelVerticalAlignment.Center

'                AddHandler currentSwingBarsBackTexts(currentSwingBarsBackTexts.Count - 1).MouseDownEvent,
'                    Sub(sender As Object)
'                        RVBarCount = CDec(CType(sender, Label).Text)
'                        Chart.ReApplyAnalysisTechnique(Me)
'                    End Sub

'                If SwingChannelOn Then
'                    With swings(swings.Count - 1)
'                        .SwingChannel = NewTrendLine(Colors.Black, .StartPoint, .EndPoint, True)
'                        .SwingChannel.IsRegressionLine = True
'                        .SwingChannel.HasParallel = True
'                        .SwingChannel.Pen = New Pen(Brushes.Black, 0)
'                        .SwingChannel.OuterPen = New Pen(New SolidColorBrush(If(swingDir = Direction.Up, UpSwingChannelColor, DownSwingChannelColor)), SwingChannelThickness)
'                        .SwingChannel.ExtendRight = True
'                        .SwingChannel.IsSelectable = False
'                        .SwingChannel.IsEditable = False
'                        .ABCLengths = New List(Of ABCCombo)
'                    End With
'                    If swings.Count > 1 AndAlso swings(swings.Count - 2).SwingChannel IsNot Nothing Then
'                        With swings(swings.Count - 2)
'                            If SwingChannelHistoryOn Then
'                                .SwingChannel.OuterPen.Thickness = SwingChannelHistoryThickness
'                                '.SwingChannel.OuterPen.DashStyle = TrendLineDashStyle
'                                'If swingDir = Direction.Down Then ' up channel
'                                '    For i = .EndBar - 1 To CurrentBar.Number - 1
'                                '        'If Chart.bars(i).Data.Low < LinCalc(.SwingChannel.coo
'                                '    Next
'                                'End If
'                                .SwingChannel.ExtendRight = False
'                            Else
'                                RemoveObjectFromChart(.SwingChannel)
'                            End If
'                        End With
'                    End If
'                End If
'                If SwingChannelLengthAndTargetTextsOn Then
'                    swings(swings.Count - 1).ABCLengths = New List(Of ABCCombo)
'                End If
'                'End If
'            ElseIf (swings.Count > 0 AndAlso CurrentBar.High >= cs.EndPrice AndAlso cs.Direction = Direction.Up) OrElse (swings.Count > 0 AndAlso CurrentBar.Low <= cs.EndPrice AndAlso cs.Direction = Direction.Down) Then
'                'extension
'                cs.EndBar = CurrentBar.Number
'                cs.EndPrice = If(cs.Direction = Direction.Up, CurrentBar.High, CurrentBar.Low)
'                curSwingEvent = AutoTrendBase.SwingEvent.Extension
'                swingDir = cs.Direction

'                'currentSwingLengthTexts(currentSwingLengthTexts.Count - 1).Location = New Point(cs.EndBar - 1, cs.EndPrice)
'                currentSwingBarsBackTexts(currentSwingBarsBackTexts.Count - 1).Location = New Point(cs.EndBar - 1, cs.EndPrice)
'                currentSwingBarsBackTexts(currentSwingBarsBackTexts.Count - 1).Text = cs.EndBar - cs.StartBar

'                If swings(swings.Count - 1).SwingChannel IsNot Nothing AndAlso SwingChannelOn Then
'                    swings(swings.Count - 1).SwingChannel.Coordinates = New LineCoordinates(cs.StartPoint, cs.EndPoint)
'                End If

'                If swings(swings.Count - 1).PushCountText IsNot Nothing Then
'                    swings(swings.Count - 1).PushCountText.Location = AddToY(swings(swings.Count - 1).EndPoint, If(swings(swings.Count - 1).Direction = Direction.Up, 1, -1) * 0.6 * Chart.Settings("RangeValue").Value)
'                End If
'            End If
'            'If cs.HasCrossedRVBand = False And ((swingDir = Direction.Up And cs.EndPrice >= currentRegressionCenterPoint + RVMultiplier * Chart.Settings("RangeValue").Value) Or (swingDir = Direction.Down And cs.EndPrice <= currentRegressionCenterPoint - RVMultiplier * Chart.Settings("RangeValue").Value)) Then cs.HasCrossedRVBand = True

'            'If curSwingEvent <> SwingEvent.None Or IsLastBarOnChart Then
'            '    If potentialRegressionChannel IsNot Nothing Then
'            '        'MsgBox("here")
'            '        Dim maxPoint As Double = If(regressionChannels(regressionChannels.Count - 1).Direction = Swing.ChannelDirectionType.Down, Double.MinValue, Double.MaxValue)
'            '        For i = potentialRegressionChannel.StartSwingIndex To swings.Count - 2
'            '            If regressionChannels(regressionChannels.Count - 1).Direction = Swing.ChannelDirectionType.Up Then maxPoint = Min(maxPoint, swings(i).EndPrice) Else maxPoint = Max(maxPoint, swings(i).EndPrice)
'            '        Next
'            '        newTrendTextObject.Location = New Point(CurrentBar.Number, maxPoint)
'            '        newTrendTextObject.Font.Brush = New SolidColorBrush(If(regressionChannels(regressionChannels.Count - 1).Direction = Swing.ChannelDirectionType.Down, TrendChannelUpColor, TrendChannelDownColor))
'            '    Else
'            '        If regressionChannels.Count > 0 Then
'            '            newTrendTextObject.Location = New Point(CurrentBar.Number, regressionChannels(regressionChannels.Count - 1).APoint.Y)
'            '            newTrendTextObject.Font.Brush = New SolidColorBrush(If(regressionChannels(regressionChannels.Count - 1).Direction = Swing.ChannelDirectionType.Down, TrendChannelUpColor, TrendChannelDownColor))
'            '        End If
'            '    End If
'            '    If regressionChannels.Count > 0 Then
'            '        If swings.Count <> regressionChannels(regressionChannels.Count - 1).EndSwingIndex Then
'            '            AddObjectToChart(extendTrendTextObject)
'            '            extendTrendTextObject.Text = rvTextPrefix & "Extend Trend"
'            '            extendTrendTextObject.Location = New Point(CurrentBar.Number, swings(regressionChannels(regressionChannels.Count - 1).EndSwingIndex).EndPrice)
'            '            extendTrendTextObject.Font.Brush = New SolidColorBrush(If(regressionChannels(regressionChannels.Count - 1).Direction = Swing.ChannelDirectionType.Down, TrendChannelDownColor, TrendChannelUpColor))
'            '        Else
'            '            RemoveObjectFromChart(extendTrendTextObject)
'            '        End If
'            '    End If
'            '    extendSwingTextObject.Text = rvTextPrefix & "Extend Swing"
'            '    extendSwingTextObject.Location = New Point(CurrentBar.Number, CurrentSwing.EndPrice)
'            '    If Round(CurrentSwing.EndPrice, 5) = Round(extendTrendTextObject.Location.Y, 5) Then
'            '        extendSwingTextObject.Text = rvTextPrefix & "Extend Swing & Trend"
'            '        extendTrendTextObject.Text = ""
'            '    End If
'            'End If

'            If curSwingEvent <> SwingEvent.None Then
'                '
'                If SwingChannelLengthAndTargetTextsOn And swings(swings.Count - 1).ABCLengths IsNot Nothing Then
'                    ' remove all previous bc lengths
'                    Dim bcLengths As New List(Of ABCCombo)
'                    While swings(swings.Count - 1).ABCLengths.Count > 0
'                        With swings(swings.Count - 1).ABCLengths
'                            RemoveObjectFromChart(.Item(0).BCProjectionLine)
'                            RemoveObjectFromChart(.Item(0).TargetText)
'                            RemoveObjectFromChart(.Item(0).LengthText)
'                            .Item(0).BCProjectionLine = Nothing
'                            .Item(0).TargetText = Nothing
'                            .Item(0).LengthText = Nothing
'                            .RemoveAt(0)
'                        End With
'                    End While
'                    ' calculate new bc points for swing bc lengths

'                    bcLengths = CalculateBCPointsForSwing(bcLengths, swings(swings.Count - 1).Direction, swings(swings.Count - 1).StartBar - 1, swings(swings.Count - 1).EndBar - 1)
'                    For Each blength In bcLengths
'                        Dim bcLength = blength.BCBarsBack
'                        If blength.TargetText Is Nothing Then
'                            'blength.TargetText = NewLabel(RemovePrefixZero(Round(bcLength, Chart.Settings("DecimalPlaces").Value)), If(swings(swings.Count - 1).Direction = Direction.Up, ConfirmedDownChannelLineColor, ConfirmedUpChannelLineColor), New Point(CurrentBar.Number, CurrentSwing.EndPrice - BooleanToInteger(swings(swings.Count - 1).Direction) * bcLength), , , False)
'                            'blength.TargetText.Font.FontSize = TargetTextFontSize
'                            'blength.TargetText.Font.FontWeight = TargetTextFontWeight
'                            'blength.TargetText.VerticalAlignment = LabelVerticalAlignment.Center
'                            'blength.TargetText.HorizontalAlignment = LabelHorizontalAlignment.Left

'                            'blength.LengthText = NewLabel(RemovePrefixZero(Round(bcLength, Chart.Settings("DecimalPlaces").Value)) & "  ", If(swings(swings.Count - 1).Direction = Direction.Up, ConfirmedDownChannelLineColor, ConfirmedUpChannelLineColor), AddToX(blength.LengthTextLocation, 1), )
'                            'blength.LengthText.Font.FontSize = TargetTextFontSize
'                            'blength.LengthText.Font.FontWeight = TargetTextFontWeight
'                            'blength.LengthText.VerticalAlignment = LabelVerticalAlignment.Center
'                            'blength.LengthText.HorizontalAlignment = LabelHorizontalAlignment.Right
'                            'AddHandler blength.LengthText.MouseDownEvent,
'                            '    Sub(sender As Object)
'                            '        RVBarCount = CDec(CType(sender, Label).Text)
'                            '        Chart.ReApplyAnalysisTechnique(Me)
'                            '    End Sub

'                            blength.BCProjectionLine = NewTrendLine(If(swings(swings.Count - 1).Direction = Direction.Up, ConfirmedDownChannelLineColor, ConfirmedUpChannelLineColor), ProjectionLinesOn)
'                            blength.BCProjectionLine.Pen.Brush = New SolidColorBrush(If(swings(swings.Count - 1).Direction = Direction.Up, ConfirmedDownChannelLineColor, ConfirmedUpChannelLineColor))
'                            blength.BCProjectionLine.Pen.Thickness = ProjectionLineThickness
'                            blength.BCProjectionLine.IsSelectable = False
'                            blength.BCProjectionLine.IsEditable = False
'                        Else
'                            blength.TargetText.Text = RemovePrefixZero(Round(bcLength, Chart.Settings("DecimalPlaces").Value))
'                        End If
'                    Next
'                    swings(swings.Count - 1).ABCLengths = bcLengths
'                End If
'            End If


'            If SwingChannelLengthAndTargetTextsOn And swings(swings.Count - 1).ABCLengths IsNot Nothing Then
'                Dim highestPoint As New Point(-1, 0)
'                Dim lowestPoint As New Point(-1, Decimal.MaxValue)
'                For i = CurrentSwing.EndBar - 1 To CurrentBar.Number - 1
'                    If Chart.bars(i).Data.High >= highestPoint.Y Then
'                        highestPoint = New Point(i, Chart.bars(i).Data.High)
'                    End If
'                    If Chart.bars(i).Data.Low <= lowestPoint.Y Then
'                        lowestPoint = New Point(i, Chart.bars(i).Data.Low)
'                    End If
'                Next
'                If highestPoint.Y <> 0 Then
'                    Dim indx As Integer = 0
'                    While indx <= swings(swings.Count - 1).ABCLengths.Count - 1
'                        Dim blength As ABCCombo = swings(swings.Count - 1).ABCLengths(indx)
'                        If blength.TargetText IsNot Nothing Then
'                            If Abs(highestPoint.Y - lowestPoint.Y) >= blength.BCLength Then ' if bc swing length matched
'                                RemoveObjectFromChart(blength.TargetText)
'                                RemoveObjectFromChart(blength.BCProjectionLine)
'                                RemoveObjectFromChart(blength.LengthText)
'                                blength.TargetText = Nothing
'                                blength.BCProjectionLine = Nothing
'                                blength.LengthText = Nothing
'                                swings(swings.Count - 1).ABCLengths.RemoveAt(indx)
'                                indx -= 1
'                            End If
'                        End If
'                        indx += 1
'                    End While
'                End If
'                If swings.Count > 1 AndAlso swings(swings.Count - 2).ABCLengths IsNot Nothing Then
'                    While swings(swings.Count - 2).ABCLengths.Count > 0
'                        With swings(swings.Count - 2).ABCLengths
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
'                If curSwingEvent <> SwingEvent.None Then
'                    Dim lastConfirmedSwingIndex As Integer
'                    If regressionChannels.Count = 0 Then
'                        lastConfirmedSwingIndex = 0
'                    Else
'                        lastConfirmedSwingIndex = regressionChannels(regressionChannels.Count - 1).EndSwingIndex
'                    End If
'                    If swings.Count - 1 >= lastConfirmedSwingIndex + 3 Then ' if there's been 3 neutral swings in a row

'                        Dim highestPoint As New Point(-1, 0)
'                        Dim secondHighestPoint As New Point(-1, 0)
'                        Dim lowestPoint As New Point(-1, Decimal.MaxValue)
'                        Dim secondLowestPoint As New Point(-1, Decimal.MaxValue)
'                        For i = lastConfirmedSwingIndex + 1 To swings.Count - 1
'                            If swings(i).EndPrice >= highestPoint.Y Then
'                                highestPoint = New Point(i, swings(i).EndPrice)
'                            End If
'                            If swings(i).EndPrice <= lowestPoint.Y Then
'                                lowestPoint = New Point(i, swings(i).EndPrice)
'                            End If
'                        Next
'                        For i = lastConfirmedSwingIndex + 1 To swings.Count - 1
'                            If swings(i).EndPrice > secondHighestPoint.Y And swings(i).EndPrice <= highestPoint.Y And i <> highestPoint.X Then
'                                secondHighestPoint = New Point(i, swings(i).EndPrice)
'                            End If
'                            If swings(i).EndPrice < secondLowestPoint.Y And swings(i).EndPrice >= lowestPoint.Y And i <> lowestPoint.X Then
'                                secondLowestPoint = New Point(i, swings(i).EndPrice)
'                            End If
'                        Next
'                        Dim channelDir As Direction = Direction.Neutral
'                        If swings.Count - 1 = highestPoint.X And lowestPoint.Y > swings(lastConfirmedSwingIndex).EndPrice Then
'                            channelDir = Direction.Up
'                        ElseIf swings.Count - 1 = lowestPoint.X And highestPoint.Y < swings(lastConfirmedSwingIndex).EndPrice Then
'                            channelDir = Direction.Down
'                        End If

'                        'channel is confirmed; create new channel
'                        If channelDir <> Direction.Neutral Then
'                            Dim newChannel As RegressionChannel = CreateRegressionChannel(If(channelDir = Direction.Up, TrendChannelUpColor, TrendChannelDownColor), lastConfirmedSwingIndex, swings.Count - 1, channelDir, True)
'                            Dim bPoint, cPoint As Point
'                            If channelDir = Direction.Up Then
'                                bPoint = New Point(swings(secondHighestPoint.X).EndBar, swings(secondHighestPoint.X).EndPrice)
'                                cPoint = New Point(swings(lowestPoint.X).EndBar, swings(lowestPoint.X).EndPrice)
'                                AddPushPointText(swings(secondHighestPoint.X), 1)
'                            Else
'                                bPoint = New Point(swings(secondLowestPoint.X).EndBar, swings(secondLowestPoint.X).EndPrice)
'                                cPoint = New Point(swings(highestPoint.X).EndBar, swings(highestPoint.X).EndPrice)
'                                AddPushPointText(swings(secondLowestPoint.X), 1)
'                            End If
'                            AddPushPointText(swings(swings.Count - 1), 2)
'                            newChannel.Pushes = 2
'                            AssignRegressionChannelProperties(newChannel, New Point(swings(lastConfirmedSwingIndex).EndBar, swings(lastConfirmedSwingIndex).EndPrice), , New Point(swings(swings.Count - 1).EndBar, swings(swings.Count - 1).EndPrice), , , True)
'                            regressionChannels(regressionChannels.Count - 1).TL.ExtendRight = False
'                            regressionChannels.Add(newChannel)
'                        End If
'                    Else
'                        ' outside swing
'                        If regressionChannels.Count > 0 AndAlso regressionChannels(regressionChannels.Count - 1).EndSwingIndex + 1 = swings.Count - 1 Then ' if it's been one swing since last confirmed channel
'                            Dim channelDir As Direction = Direction.Neutral
'                            If regressionChannels(regressionChannels.Count - 1).Direction = Swing.ChannelDirectionType.Up And swings(swings.Count - 1).EndPrice < regressionChannels(regressionChannels.Count - 1).APoint.Y Then
'                                channelDir = Direction.Down
'                            ElseIf regressionChannels(regressionChannels.Count - 1).Direction = Swing.ChannelDirectionType.Down And swings(swings.Count - 1).EndPrice > regressionChannels(regressionChannels.Count - 1).APoint.Y Then
'                                channelDir = Direction.Up
'                            End If
'                            If channelDir <> Direction.Neutral Then
'                                Dim newChannel As RegressionChannel = CreateRegressionChannel(If(channelDir = Direction.Down, TrendChannelDownColor, TrendChannelUpColor), lastConfirmedSwingIndex, swings.Count - 1, channelDir, True)
'                                newChannel.APoint = New Point(swings(lastConfirmedSwingIndex).EndBar, swings(lastConfirmedSwingIndex).EndPrice)
'                                newChannel.DPoint = New Point(swings(swings.Count - 1).EndBar, swings(swings.Count - 1).EndPrice)
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
'                                AddPushPointText(CurrentSwing, 1)
'                            End If
'                        End If
'                        If regressionChannels.Count = 0 And swings.Count > 1 Then ' if there's no channels yet
'                            'get things started by adding one
'                            Dim newChannel As RegressionChannel = CreateRegressionChannel(If(swings(swings.Count - 1).Direction = Direction.Down, TrendChannelDownColor, TrendChannelUpColor), swings.Count - 2, swings.Count - 1, swings(swings.Count - 1).Direction, True)
'                            newChannel.APoint = New Point(swings(swings.Count - 2).EndBar, swings(swings.Count - 2).EndPrice)
'                            newChannel.DPoint = New Point(swings(swings.Count - 1).EndBar, swings(swings.Count - 1).EndPrice)
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
'                        If (regressionChannels(regressionChannels.Count - 1).Direction = Swing.ChannelDirectionType.Up And swings(swings.Count - 1).EndPrice >= regressionChannels(regressionChannels.Count - 1).DPoint.Y) Or
'                            (regressionChannels(regressionChannels.Count - 1).Direction = Swing.ChannelDirectionType.Down And swings(swings.Count - 1).EndPrice <= regressionChannels(regressionChannels.Count - 1).DPoint.Y) Then
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
'                            AssignRegressionChannelProperties(regressionChannels(regressionChannels.Count - 1), , bcLengths, swings(swings.Count - 1).EndPoint, , swings.Count - 1, True)
'                            If swings(swings.Count - 1).PushCountText Is Nothing Then
'                                AddPushPointText(swings(swings.Count - 1), regressionChannels(regressionChannels.Count - 1).Pushes + 1)
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
'                        For i = swings(channel.EndSwingIndex).EndBar - 1 To CurrentBar.Number - 1
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
'                If regressionChannels.Count > 0 AndAlso regressionChannels(regressionChannels.Count - 1).EndSwingIndex < swings.Count - 2 Then ' if it's been at least 2 swings since last confirmed channel
'                    ' update potential line
'                    If potentialRegressionChannel Is Nothing Then
'                        potentialRegressionChannel = CreateRegressionChannel(If(regressionChannels(regressionChannels.Count - 1).Direction = Swing.ChannelDirectionType.Up, DownTrendChannelNeutralColor, UpTrendChannelNeutralColor), regressionChannels(regressionChannels.Count - 1).EndSwingIndex, swings.Count - 1, Direction.Neutral, False)
'                    End If
'                    potentialRegressionChannel.StartSwingIndex = regressionChannels(regressionChannels.Count - 1).EndSwingIndex
'                    potentialRegressionChannel.EndSwingIndex = swings.Count - 1
'                    potentialRegressionChannel.TL.Coordinates = New LineCoordinates(New Point(swings(potentialRegressionChannel.StartSwingIndex).EndBar, swings(potentialRegressionChannel.StartSwingIndex).EndPrice), New Point(CurrentBar.Number, CurrentBar.High))
'                End If
'                If regressionChannels.Count > 0 AndAlso regressionChannels(regressionChannels.Count - 1).EndSwingIndex < swings.Count - 1 Then ' if it's been at least 1 swing since last confirmed channel
'                    ' update potential line
'                    If potentialSameDirRegressionChannel Is Nothing Then
'                        potentialSameDirRegressionChannel = CreateRegressionChannel(If(regressionChannels(regressionChannels.Count - 1).Direction = Swing.ChannelDirectionType.Up, UpTrendChannelNeutralColor, DownTrendChannelNeutralColor), regressionChannels(regressionChannels.Count - 1).StartSwingIndex, swings.Count - 1, Direction.Neutral, False)
'                    End If
'                    potentialSameDirRegressionChannel.StartSwingIndex = regressionChannels(regressionChannels.Count - 1).StartSwingIndex
'                    potentialSameDirRegressionChannel.EndSwingIndex = swings.Count - 1
'                    potentialSameDirRegressionChannel.TL.Coordinates = New LineCoordinates(New Point(swings(potentialSameDirRegressionChannel.StartSwingIndex).EndBar, swings(potentialSameDirRegressionChannel.StartSwingIndex).EndPrice), New Point(CurrentBar.Number, CurrentBar.High))
'                End If
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
'                If curSwingEvent = SwingEvent.NewSwing Then
'                    If regressionCenterPoints.ContainsKey(CurrentSwing.StartBar) And regressionCenterPoints.ContainsKey(CurrentSwing.EndBar) Then
'                        currentMomentumSwing = NewTrendLine(If(cs.Direction = Direction.Down, UpSwingChannelColor, DownSwingChannelColor), New Point(CurrentSwing.StartBar, CurrentSwing.StartPrice - regressionCenterPoints(CurrentSwing.StartBar) + VerticalMomentumBarsPosition),
'                                                                                                                                         New Point(CurrentSwing.EndBar, CurrentSwing.EndPrice - regressionCenterPoints(CurrentSwing.EndBar) + VerticalMomentumBarsPosition))
'                        currentMomentumSwing.IsSelectable = False
'                        currentMomentumSwing.IsEditable = False
'                    End If
'                ElseIf curSwingEvent = SwingEvent.Extension And currentMomentumSwing IsNot Nothing Then
'                    If regressionCenterPoints.ContainsKey(CurrentSwing.EndBar) Then
'                        currentMomentumSwing.EndPoint = New Point(CurrentSwing.EndBar, CurrentSwing.EndPrice - regressionCenterPoints(CurrentSwing.EndBar) + VerticalMomentumBarsPosition)
'                    End If
'                End If
'            End If
'        End Sub
'        Private Sub AddPushPointText(swing As Swing, pushNumber As Integer)
'            With swing
'                .PushCountText = NewLabel(pushNumber, If(swing.Direction = Direction.Up, TrendChannelUpColor, TrendChannelDownColor), AddToY(swing.EndPoint, If(swings(swings.Count - 1).Direction = Direction.Up, 1, -1) * 0.6 * Chart.Settings("RangeValue").Value), , , False)
'                .PushCountText.Font.FontSize = PushTextFontSize
'                .PushCountText.Font.FontWeight = TrendTextFontWeight
'                .PushCountText.HorizontalAlignment = LabelHorizontalAlignment.Center
'                .PushCountText.VerticalAlignment = If(swing.Direction = Direction.Up, LabelVerticalAlignment.Bottom, LabelVerticalAlignment.Top)
'            End With
'        End Sub
'        Private Function CalculateBCPoints(ByRef bcLengths As List(Of ABCCombo), direction As Direction, startSwingIndex As Integer, endSwingIndex As Integer) As List(Of ABCCombo)
'            If startSwingIndex + 1 >= endSwingIndex Then Return bcLengths
'            Dim rangePoint As New Point(0, If(direction = AutoTrendBase.Direction.Down, 0, Decimal.MaxValue))
'            Dim opposedRangePoint As New Point(0, If(direction = AutoTrendBase.Direction.Down, Decimal.MaxValue, 0))
'            Dim rangePointSwing As Integer
'            For i = endSwingIndex To startSwingIndex + 1 Step -1
'                If (direction = AutoTrendBase.Direction.Down And swings(i).EndPrice >= rangePoint.Y) Or (direction = AutoTrendBase.Direction.Up And swings(i).EndPrice <= rangePoint.Y) Then
'                    rangePoint = New Point(i, swings(i).EndPrice)
'                    rangePointSwing = i
'                End If
'            Next
'            For i = rangePoint.X To startSwingIndex Step -1
'                If (direction = AutoTrendBase.Direction.Down And swings(i).EndPrice < opposedRangePoint.Y) Or (direction = AutoTrendBase.Direction.Up And swings(i).EndPrice > opposedRangePoint.Y) Then
'                    opposedRangePoint = New Point(i, swings(i).EndPrice)
'                End If
'            Next
'            Dim bcLength As Decimal = Round(Abs(rangePoint.Y - opposedRangePoint.Y), 6)
'            Dim indx As Integer = 0
'            While indx <= bcLengths.Count - 1
'                If bcLength >= bcLengths(indx).BCLength Then
'                    RemoveObjectFromChart(bcLengths(indx).TargetText)
'                    RemoveObjectFromChart(bcLengths(indx).BCProjectionLine)
'                    bcLengths.RemoveAt(indx) : indx -= 1
'                End If : indx += 1
'            End While

'            If swings(rangePointSwing).BarsBack <> -1 Then bcLengths.Add(New ABCCombo With {.BCLength = bcLength, .BCBarsBack = swings(rangePointSwing).BarsBack})
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
'            Dim val As Integer = GetBarsBackNumber(Not direction, New Point(rangePoint.X + 1, If(direction = AutoTrendBase.Direction.Down, Chart.bars(rangePoint.X).Data.High, Chart.bars(rangePoint.X).Data.Low)))
'            If bcLength > Chart.Settings("RangeValue").Value And val <> -1 Then bcLengths.Add(New ABCCombo With {.BCLength = bcLength, .LengthTextLocation = rangePoint, .BCBarsBack = val})
'            Return CalculateBCPointsForSwing(bcLengths, direction, rangePoint.X, endBar)
'        End Function
'        Private Sub AssignRegressionChannelProperties(newChannel As RegressionChannel, Optional aPoint As Point = Nothing, Optional bcLengths As List(Of ABCCombo) = Nothing, Optional dPoint As Point = Nothing, Optional startSwingIndex As Integer = 0, Optional endSwingIndex As Integer = 0, Optional showTargetText As Boolean = False)
'            If aPoint <> Nothing Then newChannel.APoint = aPoint
'            If bcLengths IsNot Nothing Then newChannel.BCLengths = bcLengths
'            If dPoint <> Nothing Then newChannel.DPoint = dPoint
'            'If showTargetText Then
'            '    For Each blength In newChannel.BCLengths
'            '        Dim bcLength = blength.BCBarsBack
'            '        If blength.TargetText Is Nothing Then
'            '            blength.TargetText = NewLabel(RemovePrefixZero(Round(bcLength, Chart.Settings("DecimalPlaces").Value)), If(newChannel.Direction = Direction.Up, ConfirmedDownChannelLineColor, ConfirmedUpChannelLineColor), New Point(CurrentBar.Number, CurrentSwing.EndPrice - BooleanToInteger(newChannel.Direction) * bcLength), True, , False)
'            '            'blength.TargetText.Font.FontSize = TargetTextFontSize
'            '            'blength.TargetText.Font.FontWeight = TargetTextFontWeight
'            '            'blength.TargetText.VerticalAlignment = LabelVerticalAlignment.Center
'            '            'blength.TargetText.HorizontalAlignment = LabelHorizontalAlignment.Left
'            '            'blength.BCProjectionLine = NewTrendLine(If(newChannel.Direction = Direction.Up, ConfirmedDownChannelLineColor, ConfirmedUpChannelLineColor), ProjectionLinesOn)
'            '            'blength.BCProjectionLine.Pen.Brush = New SolidColorBrush(If(newChannel.Direction = Direction.Up, ConfirmedDownChannelLineColor, ConfirmedUpChannelLineColor))
'            '            'blength.BCProjectionLine.Pen.DashStyle = TrendLineDashStyle
'            '            'blength.BCProjectionLine.Pen.Thickness = ProjectionLineThickness
'            '            'blength.BCProjectionLine.IsSelectable = False
'            '            'blength.BCProjectionLine.IsEditable = False
'            '        Else
'            '            blength.TargetText.Text = RemovePrefixZero(Round(bcLength, Chart.Settings("DecimalPlaces").Value))
'            '        End If
'            '    Next
'            'End If
'            If startSwingIndex <> 0 Then
'                newChannel.StartSwingIndex = startSwingIndex
'                newChannel.TL.StartPoint = swings(startSwingIndex).EndPoint
'            End If
'            If endSwingIndex <> 0 Then
'                newChannel.EndSwingIndex = endSwingIndex
'                newChannel.TL.EndPoint = swings(endSwingIndex).EndPoint
'            End If
'            If potentialRegressionChannel IsNot Nothing Then
'                RemoveObjectFromChart(potentialRegressionChannel.TL)
'                potentialRegressionChannel = Nothing
'            End If
'            If potentialSameDirRegressionChannel IsNot Nothing Then
'                RemoveObjectFromChart(potentialSameDirRegressionChannel.TL)
'                potentialSameDirRegressionChannel = Nothing
'            End If
'            If newChannel.IsConfirmed Then BarColor(swings(newChannel.StartSwingIndex).EndBar - 1, swings(newChannel.EndSwingIndex).EndBar - 1, If(newChannel.Direction = Swing.ChannelDirectionType.Up, UpColor, DownColor), Colors.Black, newChannel.Direction)
'        End Sub
'        Private Function CreateRegressionChannel(color As Color, startSwingIndex As Integer, endSwingIndex As Integer, direction As Direction, isConfirmed As Boolean) As RegressionChannel
'            Dim newChannel As RegressionChannel
'            newChannel = New RegressionChannel
'            newChannel.BCLengths = New List(Of ABCCombo)
'            newChannel.TL = NewTrendLine(color, New Point(swings(startSwingIndex).EndBar, swings(startSwingIndex).EndPrice), New Point(swings(endSwingIndex).EndBar, swings(endSwingIndex).EndPrice), TrendChannelOn)
'            newChannel.TL.HasParallel = True
'            newChannel.TL.Pen = New Pen(New SolidColorBrush(color), TrendChannelCenterLineThickness)
'            newChannel.TL.OuterPen = New Pen(New SolidColorBrush(color), TrendChannelThickness)
'            newChannel.TL.IsRegressionLine = True
'            newChannel.StartSwingIndex = startSwingIndex
'            newChannel.EndSwingIndex = endSwingIndex
'            newChannel.TL.ExtendRight = True
'            newChannel.Direction = direction
'            newChannel.IsConfirmed = isConfirmed
'            newChannel.TL.IsSelectable = False
'            newChannel.TL.IsEditable = False
'            If isConfirmed Then BarColor(swings(newChannel.StartSwingIndex).EndBar - 1, swings(newChannel.EndSwingIndex).EndBar - 1, If(direction = Swing.ChannelDirectionType.Up, UpColor, DownColor), Colors.Black, newChannel.Direction)
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



'        Private Sub ColorCurrentBars()
'            'If Mode = ModeType.AutoTrendFocused Then
'            '    barColorsDirty = True

'            '    If Not TypeOf Chart.bars(CurrentBar.Number - 1).Pen.Brush Is SolidColorBrush Then
'            '        Chart.bars(CurrentBar.Number - 1).Pen.Brush = New SolidColorBrush
'            '    End If
'            CType(Chart.bars(CurrentBar.Number - 1).Pen.Brush, SolidColorBrush).Color = NeutralColor
'            RefreshObject(Chart.bars(CurrentBar.Number - 1))
'            '    'Exit Sub

'            '    If curSwingEvent = SwingEvent.Extension Or curSwingEvent = SwingEvent.NewSwing Then
'            '        If ColorSwingBySwing Then
'            '            If CurrentSwing.Direction = Direction.Up Then
'            '                BarColorRoutine(CurrentSwing.StartBar, CurrentBar.Number - 1, UpTrendBarColor)
'            '            ElseIf CurrentSwing.Direction = Direction.Down Then
'            '                BarColorRoutine(CurrentSwing.StartBar, CurrentBar.Number - 1, DownTrendBarColor)
'            '            End If
'            '        Else
'            '            If CurrentSwing.ChannelDirection = Swing.ChannelDirectionType.Up Then
'            '                BarColorRoutine(CurrentSwing.StartBar, CurrentBar.Number - 1, UpTrendBarColor)
'            '            ElseIf CurrentSwing.ChannelDirection = Swing.ChannelDirectionType.Neutral Then
'            '                BarColorRoutine(CurrentSwing.StartBar, CurrentBar.Number - 1, NeutralBarColor)
'            '            ElseIf CurrentSwing.ChannelDirection = Swing.ChannelDirectionType.Down Then
'            '                BarColorRoutine(CurrentSwing.StartBar, CurrentBar.Number - 1, DownTrendBarColor)
'            '            End If
'            '        End If
'            '    End If
'            'Else
'            barColorsDirty = True
'            'Chart.bars(CurrentBar.Number - 1).Pen.Brush = New SolidColorBrush(CenterColor)
'            'If curSwingEvent <> SwingEvent.None Then
'            '    If CurrentSwing.Direction = Direction.Up Then
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
'            'For Each swing In swings
'            '    If swing.StartBar >= startBar And swing.EndBar <= endBar + 1 Then
'            '        If onlyColorNeutral Then
'            '            If swing.ChannelDirection = AutoTrendBase.Swing.ChannelDirectionType.Neutral Then
'            '                If Not ColorSwingBySwing Then CType(swing.TL.Pen.Brush, SolidColorBrush).Color = swingColor
'            '                swing.ChannelDirection = direction
'            '            End If
'            '        Else
'            '            If Not ColorSwingBySwing Then swing.TL.Pen.Brush = New SolidColorBrush(swingColor)
'            '            swing.ChannelDirection = direction
'            '        End If
'            '    End If
'            'Next
'            'If Mode = ModeType.AutoTrendFocused And Not ColorSwingBySwing Then
'            For i = startBar To endBar
'                If Not TypeOf Chart.bars(i).Pen.Brush Is SolidColorBrush Then
'                    Chart.bars(i).Pen.Brush = New SolidColorBrush
'                End If
'                If CType(Chart.bars(i).Pen.Brush, SolidColorBrush).Color <> color Then
'                    If onlyColorNeutral Then
'                        If CType(Chart.bars(i).Pen.Brush, SolidColorBrush).Color = neutralBarColor Then
'                            Chart.bars(i).Pen.Brush = New SolidColorBrush(color)
'                            RefreshObject(Chart.bars(i))
'                        End If
'                    Else
'                        Chart.bars(i).Pen.Brush = New SolidColorBrush(color)
'                        RefreshObject(Chart.bars(i))
'                    End If
'                End If
'            Next
'            'End If
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
'            Dim cs = CurrentSwing
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
'            If curSwingEvent <> SwingEvent.None Then

'            End If
'        End Sub
'        Private Sub DrawProjectionLineAndRVText()
'            ' draw projection line
'            Dim cs = CurrentSwing
'            Dim rv = Me.PriceRV
'            Dim averageSwingAngle As Double = GetProjectionLineAngle()
'            Dim endPoint As Decimal
'            If averageSwingAngle <> 0 Then
'                'rvProNewLine.StartPoint = New Point(cs.EndBar, cs.EndPrice)
'                Dim endBar As Integer = CurrentBar.Number + 2 'GetProjectedLineBarSkip(Abs(CurrentBar.Close - (currentRegressionCenterPoint + If(swingDir = Direction.Up, -1, 1) * RVMultiplier * Chart.Settings("RangeValue").Value)))

'                'Dim maxRangePoint As Decimal = If(swingDir = Direction.Up, Decimal.MaxValue, 0)
'                'For i = cs.EndBar To CurrentBar.Number - 1
'                '    If (swingDir = Direction.Up And Chart.bars(i).Data.Low < maxRangePoint) Or (swingDir = Direction.Down And Chart.bars(i).Data.High > maxRangePoint) Then
'                '        maxRangePoint = If(swingDir = Direction.Up, Chart.bars(i).Data.Low, Chart.bars(i).Data.High)
'                '    End If
'                'Next
'                'If cs.HasCrossedRVBand Then
'                If swingDir = Direction.Up Then
'                    endPoint = CurrentBar.Low  '  Min(maxRangePoint, Max(cs.StartPrice, currentRegressionCenterPoint - RVMultiplier * Chart.Settings("RangeValue").Value))
'                Else
'                    endPoint = CurrentBar.High  ' Max(maxRangePoint, Min(cs.StartPrice, currentRegressionCenterPoint + RVMultiplier * Chart.Settings("RangeValue").Value))
'                End If
'                'Else
'                '    endPoint = cs.StartPrice
'                'End If
'                'rvProNewLine.EndPoint = New Point(CurrentBar.Number + GetProjectedLineBarSkip(Abs(endPoint - CurrentBar.Close)), endPoint)
'                'newSwingTextObject.Location = New Point(CurrentBar.Number + newSwingTextBarOffset, endPoint)
'                'rvText.Location = New Point(CurrentBar.Number, rvProNewLine.EndPoint.Y)
'                'averageSwingAngle = Atan(Abs(rvProNewLine.EndPoint.Y - rvProNewLine.StartPoint.Y) / Abs(rvProNewLine.EndPoint.X - rvProNewLine.StartPoint.X))
'                If swings.Count > 1 Then
'                    If swings(swings.Count - 1).OverlapGapLine IsNot Nothing Then
'                        swings(swings.Count - 1).OverlapGapLine.EndPoint = New Point(endBar, swings(swings.Count - 1).OverlapGapLine.EndPoint.Y)
'                    End If
'                    If swings.Count > 2 Then
'                        If swings(swings.Count - 2).OverlapGapLine IsNot Nothing AndAlso (Not swings(swings.Count - 2).OverlapGapLineGapped And
'                         Not swings(swings.Count - 2).OverlapGapLineDestroyed And swings(swings.Count - 2).OverlapGapLineHit) Then
'                            swings(swings.Count - 2).OverlapGapLine.EndPoint = New Point(endBar, swings(swings.Count - 2).OverlapGapLine.EndPoint.Y)
'                        End If
'                    End If
'                End If
'                ' move target texts along
'                For Each channel In regressionChannels
'                    For Each blength In channel.BCLengths
'                        If blength.TargetText IsNot Nothing Then
'                            Dim bcHeight As Decimal = blength.BCLength
'                            Dim pnt As Point = MaxOrMinOfBarsBack(blength.BCBarsBack, channel.Direction = Swing.ChannelDirectionType.Down)
'                            blength.TargetText.Location = New Point(CurrentBar.Number + targetTextBarOffset, pnt.Y)

'                            'Dim maxBarPrice As Decimal = If(channel.Direction = Direction.Up, 0, Decimal.MaxValue)
'                            'Dim maxBarIndex As Integer
'                            'For i = channel.APoint.X To CurrentBar.Number - 1
'                            '    If (channel.Direction = Direction.Up And Chart.bars(i).Data.High >= maxBarPrice) Or (channel.Direction = Direction.Down And Chart.bars(i).Data.Low <= maxBarPrice) Then
'                            '        maxBarPrice = If(channel.Direction = Direction.Up, Chart.bars(i).Data.High, Chart.bars(i).Data.Low)
'                            '        maxBarIndex = i
'                            '    End If
'                            'Next
'                            blength.BCProjectionLine.StartPoint = pnt
'                            blength.BCProjectionLine.EndPoint = New Point(CurrentBar.Number + targetTextBarOffset, pnt.Y)
'                        End If
'                    Next
'                Next
'                If SwingChannelLengthAndTargetTextsOn And CurrentSwing.ABCLengths IsNot Nothing Then
'                    With CurrentSwing
'                        For Each blength In CurrentSwing.ABCLengths
'                            If blength.TargetText IsNot Nothing Then
'                                Dim bcHeight As Decimal = blength.BCLength
'                                Dim pnt As Point = MaxOrMinOfBarsBack(blength.BCBarsBack, CurrentSwing.Direction = Direction.Down)
'                                endBar = CurrentBar.Number + 1 ' GetProjectedLineBarSkip(Abs(yPoint - CurrentBar.Close))
'                                blength.TargetText.Location = New Point(CurrentBar.Number + targetTextBarOffset, pnt.Y)
'                                'Dim maxBarPrice As Decimal = If(.Direction = Direction.Up, 0, Decimal.MaxValue)
'                                'Dim maxBarIndex As Integer
'                                'For i = .StartBar To CurrentBar.Number - 1
'                                '    If (.Direction = Direction.Up And Chart.bars(i).Data.High >= maxBarPrice) Or (.Direction = Direction.Down And Chart.bars(i).Data.Low <= maxBarPrice) Then
'                                '        maxBarPrice = If(.Direction = Direction.Up, Chart.bars(i).Data.High, Chart.bars(i).Data.Low)
'                                '        maxBarIndex = i
'                                '    End If
'                                'Next
'                                blength.BCProjectionLine.StartPoint = pnt
'                                blength.BCProjectionLine.EndPoint = New Point(CurrentBar.Number + targetTextBarOffset, pnt.Y)
'                            End If
'                        Next
'                    End With
'                End If
'                'currentSwingBCTargetText.Text = ""

'            End If
'            'rvText.Text = rvTextPrefix & RemovePrefixZero(Round(Abs(CurrentSwing.EndPoint.Y - endPoint), Chart.Settings("DecimalPlaces").Value)) & " RV" & predictionTextSuffix ' & Round(GetDollarValue(rv), Chart.Settings("DecimalPlaces").Value)
'            'rvText.Font.Brush = New SolidColorBrush(If(CurrentSwing.Direction = Direction.Down, RVLineColor, RVLineColor))
'        End Sub


'        Private Function GetProjectionLineAngle() As Double
'            Return 75 * (PI / 180)
'            Dim swings As List(Of Swing)
'            swings = Me.swings
'            If swings.Count >= 5 Then
'                Dim averageCount As Integer = 4
'                Dim averageAngleCount As Double = 0
'                For i = swings.Count - 1 To swings.Count - averageCount Step -1
'                    averageAngleCount += Atan(Abs(swings(i).TL.EndPoint.Y - swings(i).TL.StartPoint.Y) / Abs(swings(i).TL.EndPoint.X - swings(i).TL.StartPoint.X))
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
'            b = (n * sumxy - sumx * sumy) / (n * sumx2 - sumx * sumx)
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

