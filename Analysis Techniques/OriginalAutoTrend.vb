
'Namespace AnalysisTechniques
'    Public Class OriginalAutoTrend

'#Region "AnalysisTechnique Inherited Code"
'        Inherits AutoTrendBase
'        Public Sub New(ByVal chart As Chart)
'            MyBase.New(chart) ' Call the base class constructor.
'            Description = "Main OriginalAutoTrend analysis technique."
'            Name = "AutoTrendV2"
'            'If chart IsNot Nothing Then AddHandler chart.ChartKeyDown, AddressOf KeyPress
'        End Sub
'#End Region

'        Const MANY_OPTIONS = True

'        Private bcLengths As New List(Of Double)
'        Private prevRV As Decimal = -1
'        'Protected Sub KeyPress(ByVal sender As Object, ByVal e As KeyEventArgs)
'        '    If Chart IsNot Nothing AndAlso IsEnabled Then
'        '        Dim key As Key
'        '        If e.SystemKey = key.None Then
'        '            key = e.Key
'        '        Else
'        '            key = e.SystemKey
'        '        End If
'        '        If Keyboard.Modifiers = ModifierKeys.None Then
'        '            If key = IncrementRVUpFineAmount Then
'        '                RV += FineRVIncrementValue
'        '                Chart.ReApplyAnalysisTechnique(Me)
'        '            ElseIf key = IncrementRVDownFineAmount And RV >= FineRVIncrementValue Then
'        '                RV -= FineRVIncrementValue
'        '                Chart.ReApplyAnalysisTechnique(Me)
'        '            ElseIf key = IncrementRVUpCoarseAmount Then
'        '                RV += CoarseRVIncrementValue
'        '                Chart.ReApplyAnalysisTechnique(Me)
'        '            ElseIf key = IncrementRVDownCoarseAmount And RV >= CoarseRVIncrementValue Then
'        '                RV -= CoarseRVIncrementValue
'        '                Chart.ReApplyAnalysisTechnique(Me)
'        '            ElseIf key = SetRVTo2xRange Then
'        '                RVBase()
'        '            ElseIf key = SetRVToNextBCLength Then
'        '                RVUp()
'        '            ElseIf key = SetRVToPreviousBCLength Then
'        '                RVDown()
'        '            ElseIf key = ToggleMergedChannelsColoring Then
'        '                If ChannelMode = ChannelModeType.Merged Then
'        '                    ChannelMode = ChannelModeType.Unmerged
'        '                Else
'        '                    ChannelMode = ChannelModeType.Merged
'        '                End If

'        '                Chart.ReApplyAnalysisTechnique(Me)
'        '            End If
'        '        End If
'        '    End If
'        'End Sub
'#Region "Commands"
'        Friend Overrides Sub PlusMinMove()
'            RV += Chart.GetMinTick
'            Chart.ReApplyAnalysisTechnique(Me)
'        End Sub
'        Friend Overrides Sub MinusMinMove()
'            RV -= Chart.GetMinTick
'            Chart.ReApplyAnalysisTechnique(Me)
'        End Sub
'        Friend Overrides Sub RVDown()
'            If bcLengths.Count > 0 Then
'                RV = bcLengths(bcLengths.Count - 1)
'                bcLengths.RemoveAt(bcLengths.Count - 1)
'                Chart.ReApplyAnalysisTechnique(Me)
'            End If
'        End Sub
'        Friend Overrides Function GetRVBase() As Decimal
'            If Not Chart.Settings("IsSlaveChart").Value Then
'                If CustomRangeValue = -1 Then
'                    Return Chart.Settings("RangeValue").Value * 2
'                Else
'                    Return CustomRangeValue * 2
'                End If
'            End If
'            Return 0
'        End Function
'        Friend Overrides Sub RVBase()
'            If Not Chart.Settings("IsSlaveChart").Value Then
'                If RV = GetRVBase() Then
'                    If prevRV <> -1 Then
'                        RV = prevRV
'                        Chart.ReApplyAnalysisTechnique(Me)
'                    End If
'                Else
'                    prevRV = RV
'                    RV = GetRVBase()
'                    Chart.ReApplyAnalysisTechnique(Me)
'                End If
'            End If
'        End Sub
'        Friend Overrides Sub RVUp()
'            Dim shortest As Decimal = Decimal.MaxValue
'            Dim nextShortest As Decimal = Decimal.MaxValue
'            For Each swing In swings
'                If Abs(swing.StartPrice - swing.EndPrice) < shortest And Abs(swing.StartPrice - swing.EndPrice) >= RV Then
'                    shortest = Round(Abs(swing.StartPrice - swing.EndPrice), 3)
'                End If
'            Next
'            If RV <> shortest Then
'                bcLengths.Add(RV)
'                RV = shortest
'                Chart.ReApplyAnalysisTechnique(Me)
'                'MsgBox("")
'            Else
'                For Each swing In swings
'                    Dim height As Decimal = Abs(swing.StartPrice - swing.EndPrice)
'                    If height > shortest Then
'                        nextShortest = Min(nextShortest, height)
'                    End If
'                Next
'                bcLengths.Add(RV)
'                RV = nextShortest
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

'        Friend Overrides Sub ApplyRV(value As Decimal)
'            If value <> 0 Then
'                If RV <> value Then
'                    RV = value
'                    If EnablePriceRVSlaveChart Then
'                        For Each a In priceSlaveChart.AnalysisTechniques
'                            If TypeOf a.AnalysisTechnique Is OriginalAutoTrend Then
'                                CType(a.AnalysisTechnique, OriginalAutoTrend).ApplyRV(value)
'                            End If
'                        Next
'                    End If
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
'#End Region

'#Region "Inputs"
'        <Input("", "BETA")>
'        Public Property EnableBarRVSlaveChart As Boolean = False
'        <Input("")>
'        Public Property BarRVSlaveChartIndex As Integer = 0
'        <Input("")>
'        Public Property EnablePriceRVSlaveChart As Boolean = False
'        <Input("")>
'        Public Property PriceRVSlaveChartIndex As Integer = 0
'        <Input("")>
'        Public Property EnablePriceRVSlaveChart2 As Boolean = False
'        <Input("")>
'        Public Property PriceRVSlaveChartIndex2 As Integer = 0
'        <Input("")>
'        Public Overrides Property CustomRangeValue As Double = -1

'        Private _rv As Decimal = 2
'        Public Overrides ReadOnly Property RawRV As Decimal
'            Get
'                Return _rv
'            End Get
'        End Property
'        <Input("The reversal value.", "General")>
'        Public Overrides Property RV As Decimal
'            Get
'                Return _rv
'            End Get
'            Set(value As Decimal)
'                _rv = Round(value, 7)
'                If Chart IsNot Nothing Then
'                    Chart.UpdateAutoTrendPad(Me)
'                End If
'            End Set
'        End Property
'        '<Input("The base RV.")> Public Property BaseRV As Decimal = 2
'        '<Input("The reversal value for the swing channel.")>
'        Public Property SwingRV As Decimal
'        '<Input("The dollar equivalent of the symbol minimum move.")> Public Property SymbolPriceMinimumMove As Decimal = 25

'        '<Input("The number of decimal places shown on all labels.")>
'        Public Property DecimalPlaces As Integer = 0

'        Enum SwingTextType
'            Length
'            BarCount
'        End Enum

'        <Input("The RV text color.")>
'        Public Property RVTextColor As Color = Colors.White
'        <Input("The channel mode.")>
'        Public Property ChannelMode As ChannelModeType = ChannelModeType.Merged
'        <Input("The number of bars before the left-most bar to begin drawing objects.")>
'        Public Property DrawingCutoff As Integer = 100
'        <Input("The option hide all bars on the chart.")>
'        Public Property HideBars As Boolean = False

'        <Input("The swing thickness.", "Swing Lines")>
'        Public Property SwingThickness As Decimal = 1.5
'        <Input("The thickness for the last swing line and projection line.")>
'        Public Property LastSwingThickness As Decimal = 2.5
'        <Input("The color for swing lines.")>
'        Public Property NeutralSwingLineColor As Color = Colors.LightGray
'        <Input("The color for swing lines during a down trend.")>
'        Public Property ProjectionLineColor As Color = Colors.Fuchsia
'        <Input("The color for swing lines during an up trend.")>
'        Public Property UpTrendSwingLineColor As Color = ColorConverter.ConvertFromString("#FF00BF00")
'        <Input("The color for swing lines during a down trend.")>
'        Public Property DownTrendSwingLineColor As Color = ColorConverter.ConvertFromString("#FFBF0000")
'        '<Input("The color for outside swings when in swing mode.")>
'        'Public Property OutsideSwingSwingModeColor As Color = Colors.Green
'        '<Input("The color for inside swings when in swing mode.")>
'        'Public Property InsideSwingSwingModeColor As Color = Colors.Red
'        '<Input("The color for up overlap markers.")>
'        'Public Property UpOverlapMarkerColor As Color = Colors.Green
'        '<Input("The color for down overlap markers.")>
'        'Public Property DownOverlapMarkerColor As Color = Colors.Red
'        <Input("The type of information to display in swing texts.")>
'        Public Property SwingTextOption As SwingTextType = SwingTextType.Length
'        <Input("The color for swing information texts.")>
'        Public Property SwingTextColor As Color = Colors.Gray
'        <Input("The font size for pivot and length texts.")>
'        Public Property LengthTextFontSize As Double = 10
'        <Input("The font size for target texts.")>
'        Public Property TargetTextFontSize As Double = 10
'        <Input("The font weight for target texts.")>
'        Public Property TargetTextFontWeight As FontWeight = FontWeights.Normal

'        <Input("The visibility for swing lines.", "Visibility Switches")>
'        Public Property SwingLinesVisible As Boolean = True
'        <Input("The option to hide the projection lines.")>
'        Public Property ProjectionLinesVisible As Boolean = True
'        '<Input("The option to hide the last swing line.")>
'        Public Property HideLastSwingLine As Boolean = False
'        <Input("The visibility for swing information texts.")>
'        Public Property SwingTextsVisible As Boolean = True
'        <Input("The visibility for channel lines.")>
'        Public Property ChannelLinesVisible As Boolean = True
'        <Input("The option to hide all potential channel lines except the last one in each direction.")>
'        Public Property ShowOnlyLastPotentialChannels As Boolean = True
'        <Input("The option to hide all confirmed channel lines except the last one in each direction.")>
'        Public Property ShowOnlyLastConfirmedChannels As Boolean = True
'        <Input("The option to hide channels which start point is off the left side of the chart.")>
'        Public Property ShowOnlyChannelsOnScreen As Boolean = False
'        '<Input("The option to highlight the last cut channel.")>
'        Public Property HighlightLastCutChannel As Boolean = True
'        <Input("The visibility for confirmed history channel lines.")>
'        Public Property ConfirmedHistoryChannelLinesVisible As Boolean = False
'        <Input("The visibility for potential history channel lines.")>
'        Public Property PotentialHistoryChannelLinesVisible As Boolean = False
'        <Input("The visibility for potential and confirmed history channel line parallels.")>
'        Public Property HistoryChannelLineParallelsVisible As Boolean = False
'        <Input("TThe visibility for gapped channel line parallels.")>
'        Public Property GappedChannelLineParallelsVisible As Boolean = True
'        <Input("The visibility for BC length texts.")>
'        Public Property BCLengthTextsVisible As Boolean = True
'        <Input("The multiplier of the range that the RV must be equal to or above in order to display history BC texts.")>
'        Public Property HistoryBCLengthVisibilityRVCutoff As Decimal = 2
'        <Input("The visibility for BC target texts.")>
'        Public Property BCTargetTextsVisible As Boolean = True
'        '<Input("The visibility for gap markers.", "Gap Markers")>
'        Public Property ShowGapMarkers As Boolean = False
'        '<Input("The color for up gap markers.")>
'        Public Property UpGapMarkerColor As Color = Colors.LightGreen
'        '<Input("The color for down gap markers.")>
'        Public Property DownGapMarkerColor As Color = Colors.Pink

'        <Input("The color for potential up channel lines.", "Channel Lines")>
'        Public Property PotentialUpChannelLineColor As Color = ColorConverter.ConvertFromString("#FF005000")
'        <Input("The color for potential down channel lines.")>
'        Public Property PotentialDownChannelLineColor As Color = ColorConverter.ConvertFromString("#FF550000")
'        <Input("The color for confirmed up channel lines.")>
'        Public Property ConfirmedUpChannelLineColor As Color = Colors.Lime
'        <Input("The color for confirmed down channel lines.")>
'        Public Property ConfirmedDownChannelLineColor As Color = Colors.Red
'        <Input("The color for backed up confirmed up channel lines.")>
'        Public Property BackedUpConfirmedUpChannelLineColor As Color = Colors.DarkGreen
'        <Input("The color for backed up confirmed down channel lines.")>
'        Public Property BackedUpConfirmedDownChannelLineColor As Color = Colors.DarkRed
'        <Input("The color for potential up channel lines that have been hit.")>
'        Public Property PotentialHistoryUpChannelLineColor As Color = ColorConverter.ConvertFromString("#FF404000")
'        <Input("The color for potential down channel lines that have been hit.")>
'        Public Property PotentialHistoryDownChannelLineColor As Color = ColorConverter.ConvertFromString("#FF002B55")
'        <Input("The color for confirmed up channel lines that have been hit.")>
'        Public Property ConfirmedHistoryUpChannelLineColor As Color = ColorConverter.ConvertFromString("#FF008500")
'        <Input("The color for confirmed down channel lines that have been hit.")>
'        Public Property ConfirmedHistoryDownChannelLineColor As Color = ColorConverter.ConvertFromString("#FFB40000")
'        <Input("The width for comfirmed gap lines.")>
'        Public Property GappedChannelLineWidth As Decimal = 2
'        <Input("The color for partial gap lines.")>
'        Public Property PartialGapChannelLineColor As Color = Colors.Pink
'        <Input("The color for gapped up channel lines.")>
'        Public Property GappedUpChannelLineColor As Color = Colors.Purple
'        <Input("The color for gapped down channel lines.")>
'        Public Property GappedDownChannelLineColor As Color = Colors.Purple

'        <Input("The option to hide the swing gap markers.", "Swing Gap Markers")>
'        Public Property CSwingGapMarkersVisible As Boolean = True
'        <Input("The option to hide the swing gap markers.")>
'        Public Property BSwingGapMarkersVisible As Boolean = True
'        <Input("The thickness for history swing C gap markers.")>
'        Public Property HistorySwingCGapMarkerThickness As Double = 1
'        <Input("The thickness for history swing B gap markers.")>
'        Public Property HistorySwingBGapMarkerThickness As Double = 3
'        <Input("The thickness for swing gap markers.")>
'        Public Property SwingGapMarkerThickness As Double = 3
'        <Input("The color for swing gap markers.")>
'        Public Property SwingGapMarkerColor As Color = Colors.Blue
'        <Input("The color for gapped swing gap markers.")>
'        Public Property GappedSwingGapMarkerColor As Color = Colors.Magenta
'        <Input("The color for potential gapped swing gap markers.")>
'        Public Property PotentialSwingGapMarkerColor As Color = Colors.LightPink

'        <Input("The visibility for the current swing channel.", "Swing Channels")>
'        Public Property SwingChannelLinesVisible As Boolean = True
'        <Input("The color for the confirmed swing channel.")>
'        Public Property SwingChannelConfirmedColor As Color = Colors.Gray
'        <Input("The color for the confirmed gapped swing channel.")>
'        Public Property SwingChannelConfirmedGappedColor As Color = Colors.Magenta
'        <Input("The color for the potential gapped swing channel.")>
'        Public Property SwingChannelPotentialGappedColor As Color = Colors.LightPink
'        <Input("The color for the potential up swing channels.")>
'        Public Property SwingChannelPotentialUpColor As Color = Colors.LightGray
'        <Input("The color for the potential down swing channels.")>
'        Public Property SwingChannelPotentialDownColor As Color = Colors.LightGray
'        <Input("The color for the history confirmed swing channel lines.")>
'        Public Property HistorySwingChannelColor As Color = Colors.DimGray
'        <Input("The visibility the history confirmed swing channel lines.")>
'        Public Property ShowSwingChannelHistoryLines As Boolean = True
'        <Input("The visibility the history confirmed gap swing channel lines.")>
'        Public Property ShowSwingChannelHistoryGapLines As Boolean = True
'		<Input("The font size for the BC length text.")>
'		Public Property SwingChannelBCLengthTextSize As Integer = 10
'		<Input("The font weight for the BC length text.")>
'		Public Property SwingChannelBCLengthTextWeight As FontWeight = FontWeights.Normal
'		<Input("The font color for the BC length text.")>
'		Public Property SwingChannelBCLengthTextColor As Color = Colors.Gray

'        <Input("The option to enable or disable bar coloring.", "Bar Coloring")>
'        Public Property ColorBars As Boolean = True
'        <Input("The color for the bars during a neutral trend direction state.")>
'        Public Property NeutralBarColor As Color = Colors.Gray
'        <Input("The color for the bars during an up trend.")>
'        Public Property UpTrendBarColor As Color = ColorConverter.ConvertFromString("#FF006A00")
'        <Input("The color for the bars during a down trend.")>
'        Public Property DownTrendBarColor As Color = ColorConverter.ConvertFromString("#FF6A0000")

'        '<Input("The fine adjustment for the RV.", "Hotkeys")> Public Property FineRVIncrementValue As Decimal = 0.5
'        '<Input("The course adjustment for the RV.")> Public Property CoarseRVIncrementValue As Decimal = 0.5
'        '<Input("The hotkey to increment the RV up the specified fine amount.")>
'        'Public Property IncrementRVUpFineAmount As Key = Key.None
'        '<Input("The hotkey to increment the RV down the specified fine amount.")>
'        'Public Property IncrementRVDownFineAmount As Key = Key.None
'        '<Input("The hotkey to increment the RV up the specified coarse amount.")>
'        'Public Property IncrementRVUpCoarseAmount As Key = Key.None
'        '<Input("The hotkey to increment the RV down the specified coarse amount.")>
'        'Public Property IncrementRVDownCoarseAmount As Key = Key.None
'        '<Input("The hotkey to toggle whether merged channels are colored.")>
'        'Public Property ToggleMergedChannelsColoring As Key = Key.None

'        '<Input()> Public Property SetRVTo3xRange As Key = Key.None
'        '<Input()> Public Property SetRVTo2xRange As Key = Key.None
'        '<Input()> Public Property SetRVToNextBCLength As Key = Key.None
'        '<Input()> Public Property SetRVToPreviousBCLength As Key = Key.None

'#End Region

'        Public Overrides Property Name As String

'        Private rvText As Label
'        Private rvProNewLine As TrendLine

'        Private currentGapMark As GapMarker

'        Private barColorsDirty As Boolean

'        Private swings As List(Of Swing)
'        Private channels As List(Of Channel)
'        Private coloringChannels As List(Of Channel)
'        Private swingChannel As SwingChannelLine
'        Private confirmedSwingChannels As List(Of SwingChannelLine)
'        Private currentSwingBCLengthText As Label
'        Private currentSwingBCTargetText As Label
'        Private currentSwingPotentialBCTargetText As Label
'        Private currentPartiallyCutChannel As PartiallyCutChannel
'        Private dayHighPrice As Decimal
'        Private dayLowPrice As Decimal
'        'Private colorMergedChannels As Boolean = True
'        Private swingCountGraphYPlacement As Decimal
'        Private currentSwingCountGraphLine As TrendLine
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
'        Private ReadOnly Property CurrentChannelLine As Channel
'            Get
'                If channels.Count > 0 Then
'                    Return channels(channels.Count - 1)
'                Else
'                    Return Nothing
'                End If
'            End Get
'        End Property
'        Private ReadOnly Property CurrentConfirmedChannelLine As Channel
'            Get
'                For i = channels.Count - 1 To 0 Step -1
'                    If channels(i).IsConfirmed Then Return channels(i)
'                Next
'                Return Nothing
'            End Get
'        End Property
'        Private ReadOnly Property CurrentChannelLineIfPotential As Channel
'            Get
'                If channels.Count > 0 AndAlso Not channels(channels.Count - 1).IsConfirmed Then
'                    Return channels(channels.Count - 1)
'                Else
'                    Return Nothing
'                End If
'                Return Nothing
'            End Get
'        End Property

'        Friend Overrides Sub OnCreate()
'            MyBase.OnCreate()
'            If Chart IsNot Nothing Then
'                If CustomRangeValue = -1 Then
'                    RV = Chart.Settings("RangeValue").Value * DefaultAutoTrendRV
'                Else
'                    RV = CustomRangeValue * DefaultAutoTrendRV
'                End If
'                'FineRVIncrementValue = Chart.GetMinTick
'                'CoarseRVIncrementValue = Chart.GetMinTick * 5
'            End If

'        End Sub
'        Public Overrides Sub Reset()
'            MyBase.Reset()
'            If barColorsDirty And Chart.bars.Count > 1 Then
'                BarColor(0, Chart.bars.Count - 1, Chart.Settings("Bar Color").Value, NeutralSwingLineColor, Swing.ChannelDirectionType.Neutral)
'                barColorsDirty = False
'            End If
'        End Sub
'        Protected Overrides Sub Begin()
'            MyBase.Begin()
'            dayHighPrice = Decimal.MinValue
'            dayLowPrice = Decimal.MaxValue
'            barBarsToDraw = New List(Of BarData)
'            slaveChartBarCurrentPrice = 1000
'            lastSlaveChartBarIndex = 1

'            priceBarsToDraw = New List(Of BarData)
'            lastSlaveChartPrice = CurrentBar.Close
'            slaveChartPriceCurrentPrice = CurrentBar.Close
'            priceBarsToDraw2 = New List(Of BarData)
'            lastSlaveChartPrice2 = CurrentBar.Close
'            slaveChartPriceCurrentPrice2 = CurrentBar.Close

'            channels = New List(Of Channel)
'            coloringChannels = New List(Of Channel)
'            swings = New List(Of Swing)
'            swings.Add(New Swing(NewTrendLine(NeutralSwingLineColor,
'                                              New Point(CurrentBar.Number, CurrentBar.Close),
'                                                    New Point(CurrentBar.Number, CurrentBar.Close),
'                                                    SwingLinesVisible), Direction.Down))
'            CurrentSwing.TL.Pen.Thickness = SwingThickness
'            currentSwingLengthText = NewLabel(GetDollarValue(Abs(swings(swings.Count - 1).StartPrice - swings(swings.Count - 1).EndPrice)), SwingTextColor, New Point(swings(swings.Count - 1).EndBar, swings(swings.Count - 1).EndPrice), True)
'            currentSwingLengthText.Font.FontSize = LengthTextFontSize

'            currentChannelEndSwingIndex = 0
'            currentChannelBeginSwingIndex = 0
'            currentChannelUnmerged = Nothing

'            rvText = NewLabel("", RVTextColor, New Point(0, 0))
'            rvText.Font.FontSize = TargetTextFontSize
'            rvText.Font.FontWeight = TargetTextFontWeight
'            rvText.VerticalAlignment = LabelVerticalAlignment.Center
'            rvText.HorizontalAlignment = LabelHorizontalAlignment.Left
'            rvProNewLine = NewTrendLine(ProjectionLineColor, New Point(0, 0), New Point(0, 0), ProjectionLinesVisible)
'            rvProNewLine.Pen.Thickness = LastSwingThickness
'            currentSwingBCTargetText = NewLabel("", RVTextColor, New Point(0, 0), True)
'            currentSwingBCTargetText.HorizontalAlignment = LabelHorizontalAlignment.Left
'            currentSwingBCTargetText.VerticalAlignment = LabelVerticalAlignment.Center
'			currentSwingBCTargetText.Font.FontSize = SwingChannelBCLengthTextSize
'			currentSwingBCTargetText.Font.FontWeight = SwingChannelBCLengthTextWeight
'			currentSwingBCTargetText.Font.Brush = New SolidColorBrush(SwingChannelBCLengthTextColor)
'            currentSwingPotentialBCTargetText = NewLabel("", If(CurrentSwing.Direction = Direction.Down, SwingChannelPotentialDownColor, SwingChannelPotentialUpColor), New Point(0, 0), True)
'            currentSwingPotentialBCTargetText.HorizontalAlignment = LabelHorizontalAlignment.Right
'            currentSwingPotentialBCTargetText.VerticalAlignment = LabelVerticalAlignment.Center
'			currentSwingPotentialBCTargetText.Font.FontSize = SwingChannelBCLengthTextSize
'			currentSwingPotentialBCTargetText.Font.FontWeight = SwingChannelBCLengthTextWeight
'			currentSwingPotentialBCTargetText.Font.Brush = New SolidColorBrush(SwingChannelBCLengthTextColor)
'            swingChannel = New SwingChannelLine(NewTrendLine(If(CurrentSwing.Direction = Direction.Down, SwingChannelPotentialDownColor, SwingChannelPotentialUpColor), New Point(0, 0), New Point(0, 0), True))
'            confirmedSwingChannels = New List(Of SwingChannelLine)
'            currentPartiallyCutChannel = Nothing

'            If CustomRangeValue = -1 Then
'                SwingRV = Chart.Settings("RangeValue").Value * 1.2
'                swingCountGraphYPlacement = CurrentBar.Close + Chart.Settings("RangeValue").Value * 20
'            Else
'                SwingRV = CustomRangeValue * 1.2
'                swingCountGraphYPlacement = CurrentBar.Close + CustomRangeValue * 20
'            End If
'            currentSwingCountGraphLine = NewTrendLine(ConfirmedDownChannelLineColor, New Point(CurrentBar.Number, swingCountGraphYPlacement), New Point(CurrentBar.Number, swingCountGraphYPlacement))
'            currentSwingCountGraphLine.Pen.Thickness = 10

'            Chart.dontDrawBarVisuals = HideBars
'            For Each bar In Chart.bars
'                bar.DrawVisual = Not HideBars
'                If HideBars Then
'                    bar.ClearVisual()
'                Else
'                    bar.RefreshVisual()
'                End If
'            Next

'            If EnableBarRVSlaveChart Then
'                Dim sc = barSlaveChart
'                sc.dontDrawBarVisuals = True
'                sc.RequestData(TimeSpan.FromSeconds(0), True)
'                sc.AddBarWrapper(1000, 1000, 1000, 1000)
'            End If
'            If EnablePriceRVSlaveChart Then
'                Dim sc = priceSlaveChart
'                sc.dontDrawBarVisuals = True
'                sc.RequestData(TimeSpan.FromSeconds(0), True)
'                sc.AddBarWrapper(CurrentBar.Open, CurrentBar.High, CurrentBar.Low, CurrentBar.Close)
'            End If
'            If EnablePriceRVSlaveChart2 Then
'                Dim sc = priceSlaveChart2
'                sc.dontDrawBarVisuals = True
'                sc.RequestData(TimeSpan.FromSeconds(0), True)
'                sc.AddBarWrapper(CurrentBar.Open, CurrentBar.High, CurrentBar.Low, CurrentBar.Close)
'            End If
'            curSwingEvent = SwingEvent.None
'        End Sub

'        Dim curSwingEvent As SwingEvent = SwingEvent.None
'        Dim swingDir As Direction
'        Dim currentSwingLengthText As Label
'        Dim currentChannelEndSwingIndex As Integer
'        Dim currentChannelBeginSwingIndex As Integer
'        Dim currentChannelUnmerged As Channel

'        Dim lastSlaveChartBarIndex As Integer
'        Dim barBarsToDraw As List(Of BarData)
'        Dim slaveChartBarCurrentPrice As Decimal

'        Dim lastSlaveChartPrice As Decimal
'        Dim priceBarsToDraw As List(Of BarData)
'        Dim slaveChartPriceCurrentPrice As Decimal
'        Dim lastSlaveChartPrice2 As Decimal
'        Dim priceBarsToDraw2 As List(Of BarData)
'        Dim slaveChartPriceCurrentPrice2 As Decimal
'        Private ReadOnly Property barSlaveChart As Chart
'            Get
'                Return Chart.Parent.Charts(BarRVSlaveChartIndex)
'            End Get
'        End Property
'        Private ReadOnly Property priceSlaveChart As Chart
'            Get
'                Try
'                    Return Chart.Parent.Charts(PriceRVSlaveChartIndex)
'                Catch ex As Exception
'                    ShowInfoBox("Could not find specified PriceSlaveChart.", Chart.DesktopWindow)
'                    Return Nothing
'                End Try
'            End Get
'        End Property
'        Private ReadOnly Property priceSlaveChart2 As Chart
'            Get
'                Try
'                    Return Chart.Parent.Charts(PriceRVSlaveChartIndex2)
'                Catch ex As Exception
'                    ShowInfoBox("Could not find specified PriceSlaveChart2.", Chart.DesktopWindow)
'                    Return Nothing
'                End Try
'            End Get
'        End Property
'        Protected Overrides Sub Main()
'            'If CurrentBar.Number = Round(Chart.Bounds.TopLeft.X) - 100 Then
'            '    Reset()
'            '    Begin()
'            'End If
'            DrawVisuals = CurrentBar.Number > Round(Chart.Bounds.TopLeft.X) - DrawingCutoff

'            Dim cs As Swing = CurrentSwing
'            curSwingEvent = SwingEvent.None
'            If CurrentBar.High - cs.EndPrice >= RV AndAlso CurrentBar.Number <> cs.EndBar AndAlso cs.Direction = Direction.Down Then
'                'new swing up
'                If swings.Count > 0 And SwingLinesVisible Then


'                    swings(swings.Count - 1).TL.Pen.Thickness = SwingThickness
'                End If
'                If HideLastSwingLine AndAlso swings.Count > 0 And SwingLinesVisible Then
'                    AddObjectToChart(swings(swings.Count - 1).TL)
'                    AddObjectToChart(swings(swings.Count - 1).PreviousOverlapTrendLine)
'                ElseIf HideLastSwingLine AndAlso swings.Count > 0 And SwingLinesVisible And ChannelMode = ChannelModeType.UnmergedOverlap Then
'                End If
'                swings.Add(NewSwing(NeutralSwingLineColor, New Point(cs.EndBar, cs.EndPrice), New Point(CurrentBar.Number, CurrentBar.High), Not HideLastSwingLine And SwingLinesVisible, Direction.Up))
'                CurrentSwing.TL.Pen.Thickness = LastSwingThickness
'                Dim info As Decimal
'                If SwingTextOption = SwingTextType.Length Then
'                    info = GetDollarValue(Abs(swings(swings.Count - 1).StartPrice - swings(swings.Count - 1).EndPrice))
'                Else
'                    info = swings(swings.Count - 1).EndBar - swings(swings.Count - 1).StartBar
'                End If
'                currentSwingLengthText = NewLabel(info & " ", SwingTextColor, New Point(swings(swings.Count - 1).EndBar - 1, swings(swings.Count - 1).EndPrice), False)
'                currentSwingLengthText.Font.FontSize = LengthTextFontSize
'                currentSwingLengthText.HorizontalAlignment = LabelHorizontalAlignment.Right
'                currentSwingLengthText.VerticalAlignment = LabelVerticalAlignment.Center
'                If SwingTextsVisible Then AddObjectToChart(currentSwingLengthText)
'                curSwingEvent = OriginalAutoTrend.SwingEvent.NewSwing
'                swingDir = Direction.Up
'                If swings.Count > 1 Then
'                    If EnableBarRVSlaveChart Then
'                        Dim sc = barSlaveChart
'                        'If IsLastBarOnChart Then slaveChartBarCurrentPrice += sc.Settings("RangeValue").Value
'                        For i = CurrentSwing.StartBar To CurrentSwing.EndBar - 1
'                            slaveChartBarCurrentPrice += sc.Settings("RangeValue").Value
'                            If IsLastBarOnChart And Not loadingHistory Then
'                                sc.CalculatePriceChange(New BarData(slaveChartBarCurrentPrice, slaveChartBarCurrentPrice + sc.Settings("RangeValue").Value,
'                                                                    slaveChartBarCurrentPrice, slaveChartBarCurrentPrice + sc.Settings("RangeValue").Value,
'                                                                    CurrentBar.Date, TimeSpanFromBarSize(sc.Settings("BarSize").Value)))
'                            Else
'                                barBarsToDraw.Add(New BarData(slaveChartBarCurrentPrice, slaveChartBarCurrentPrice + sc.Settings("RangeValue").Value,
'                                                           slaveChartBarCurrentPrice, slaveChartBarCurrentPrice + sc.Settings("RangeValue").Value,
'                                                           CurrentBar.Date, TimeSpanFromBarSize(sc.Settings("BarSize").Value)))
'                            End If
'                        Next
'                        slaveChartBarCurrentPrice += sc.Settings("RangeValue").Value
'                        lastSlaveChartBarIndex = CurrentSwing.EndBar
'                    End If
'                    If EnablePriceRVSlaveChart Then
'                        Dim sc = priceSlaveChart
'                        For i = CurrentSwing.StartPrice To CurrentSwing.EndPrice - sc.Settings("RangeValue").Value * 2 Step Chart.GetMinTick
'                            slaveChartPriceCurrentPrice += sc.Settings("RangeValue").Value
'                            If IsLastBarOnChart Then
'                                sc.CalculatePriceChange(New BarData(slaveChartPriceCurrentPrice, slaveChartPriceCurrentPrice + sc.Settings("RangeValue").Value,
'                                                                    slaveChartPriceCurrentPrice, slaveChartPriceCurrentPrice + sc.Settings("RangeValue").Value,
'                                                                    CurrentBar.Date, TimeSpanFromBarSize(sc.Settings("BarSize").Value)))
'                            Else
'                                priceBarsToDraw.Add(New BarData(slaveChartPriceCurrentPrice, slaveChartPriceCurrentPrice + sc.Settings("RangeValue").Value,
'                                                           slaveChartPriceCurrentPrice, slaveChartPriceCurrentPrice + sc.Settings("RangeValue").Value,
'                                                           CurrentBar.Date, TimeSpanFromBarSize(sc.Settings("BarSize").Value)))
'                            End If
'                        Next
'                        slaveChartPriceCurrentPrice += sc.Settings("RangeValue").Value
'                        lastSlaveChartPrice = CurrentSwing.EndPrice
'                    End If
'                    If EnablePriceRVSlaveChart2 Then
'                        Dim sc = priceSlaveChart2
'                        For i = CurrentSwing.StartPrice To CurrentSwing.EndPrice - Chart.GetMinTick * 2 Step Chart.GetMinTick
'                            slaveChartPriceCurrentPrice2 += sc.Settings("RangeValue").Value
'                            If IsLastBarOnChart Then
'                                sc.CalculatePriceChange(New BarData(slaveChartPriceCurrentPrice2, slaveChartPriceCurrentPrice2 + sc.Settings("RangeValue").Value,
'                                                                    slaveChartPriceCurrentPrice2, slaveChartPriceCurrentPrice2 + sc.Settings("RangeValue").Value,
'                                                                    CurrentBar.Date, TimeSpanFromBarSize(sc.Settings("BarSize").Value)))
'                            Else
'                                priceBarsToDraw2.Add(New BarData(slaveChartPriceCurrentPrice2, slaveChartPriceCurrentPrice2 + sc.Settings("RangeValue").Value,
'                                                           slaveChartPriceCurrentPrice2, slaveChartPriceCurrentPrice2 + sc.Settings("RangeValue").Value,
'                                                           CurrentBar.Date, TimeSpanFromBarSize(sc.Settings("BarSize").Value)))
'                            End If
'                        Next
'                        slaveChartPriceCurrentPrice2 += sc.Settings("RangeValue").Value
'                        lastSlaveChartPrice2 = CurrentSwing.EndPrice
'                    End If
'                End If
'            ElseIf cs.EndPrice - CurrentBar.Low >= RV AndAlso CurrentBar.Number <> cs.EndBar AndAlso cs.Direction = Direction.Up Then
'                ' new swing down
'                If swings.Count > 0 And SwingLinesVisible Then
'                    swings(swings.Count - 1).TL.Pen.Thickness = SwingThickness
'                End If
'                If HideLastSwingLine AndAlso swings.Count > 0 And SwingLinesVisible Then
'                    AddObjectToChart(swings(swings.Count - 1).TL)
'                    AddObjectToChart(swings(swings.Count - 1).PreviousOverlapTrendLine)
'                ElseIf HideLastSwingLine AndAlso swings.Count > 0 And SwingLinesVisible And ChannelMode = ChannelModeType.UnmergedOverlap Then
'                End If
'                swings.Add(NewSwing(NeutralSwingLineColor, New Point(cs.EndBar, cs.EndPrice), New Point(CurrentBar.Number, CurrentBar.Low), Not HideLastSwingLine And SwingLinesVisible, Direction.Down))
'                CurrentSwing.TL.Pen.Thickness = LastSwingThickness
'                Dim info As Decimal
'                If SwingTextOption = SwingTextType.Length Then
'                    info = GetDollarValue(Abs(swings(swings.Count - 1).StartPrice - swings(swings.Count - 1).EndPrice))
'                Else
'                    info = swings(swings.Count - 1).EndBar - swings(swings.Count - 1).StartBar
'                End If
'                currentSwingLengthText = NewLabel(info & " ", SwingTextColor, New Point(swings(swings.Count - 1).EndBar - 1, swings(swings.Count - 1).EndPrice), False)
'                currentSwingLengthText.Font.FontSize = LengthTextFontSize
'                currentSwingLengthText.HorizontalAlignment = LabelHorizontalAlignment.Right
'                currentSwingLengthText.VerticalAlignment = LabelVerticalAlignment.Center
'                If SwingTextsVisible Then AddObjectToChart(currentSwingLengthText)
'                curSwingEvent = OriginalAutoTrend.SwingEvent.NewSwing
'                swingDir = Direction.Down
'                If swings.Count > 1 Then
'                    If EnableBarRVSlaveChart Then
'                        Dim sc = barSlaveChart
'                        'If IsLastBarOnChart Then slaveChartBarCurrentPrice -= sc.Settings("RangeValue").Value
'                        For i = CurrentSwing.StartBar To CurrentSwing.EndBar - 1
'                            slaveChartBarCurrentPrice -= sc.Settings("RangeValue").Value
'                            If IsLastBarOnChart And Not loadingHistory Then
'                                sc.CalculatePriceChange(New BarData(slaveChartBarCurrentPrice, slaveChartBarCurrentPrice, slaveChartBarCurrentPrice - sc.Settings("RangeValue").Value,
'                                                                    slaveChartBarCurrentPrice - sc.Settings("RangeValue").Value,
'                                                                    CurrentBar.Date, TimeSpanFromBarSize(sc.Settings("BarSize").Value)))
'                            Else
'                                barBarsToDraw.Add(New BarData(slaveChartBarCurrentPrice, slaveChartBarCurrentPrice, slaveChartBarCurrentPrice - sc.Settings("RangeValue").Value,
'                                                           slaveChartBarCurrentPrice - sc.Settings("RangeValue").Value,
'                                                           CurrentBar.Date, TimeSpanFromBarSize(sc.Settings("BarSize").Value)))
'                            End If
'                        Next
'                        slaveChartBarCurrentPrice -= sc.Settings("RangeValue").Value
'                        lastSlaveChartBarIndex = CurrentSwing.EndBar
'                    End If
'                    If EnablePriceRVSlaveChart Then
'                        Dim sc = priceSlaveChart
'                        For i = CurrentSwing.StartPrice To CurrentSwing.EndPrice + Chart.GetMinTick * 2 Step -Chart.GetMinTick
'                            slaveChartPriceCurrentPrice -= sc.Settings("RangeValue").Value
'                            If IsLastBarOnChart Then
'                                sc.CalculatePriceChange(New BarData(slaveChartPriceCurrentPrice, slaveChartPriceCurrentPrice,
'                                                                    slaveChartPriceCurrentPrice - sc.Settings("RangeValue").Value, slaveChartPriceCurrentPrice - sc.Settings("RangeValue").Value,
'                                                                    CurrentBar.Date, TimeSpanFromBarSize(sc.Settings("BarSize").Value)))
'                            Else
'                                priceBarsToDraw.Add(New BarData(slaveChartPriceCurrentPrice, slaveChartPriceCurrentPrice,
'                                                           slaveChartPriceCurrentPrice - sc.Settings("RangeValue").Value, slaveChartPriceCurrentPrice - sc.Settings("RangeValue").Value,
'                                                           CurrentBar.Date, TimeSpanFromBarSize(sc.Settings("BarSize").Value)))
'                            End If
'                        Next
'                        slaveChartPriceCurrentPrice -= sc.Settings("RangeValue").Value
'                        lastSlaveChartPrice = CurrentSwing.EndPrice
'                    End If
'                    If EnablePriceRVSlaveChart2 Then
'                        Dim sc = priceSlaveChart2
'                        For i = CurrentSwing.StartPrice To CurrentSwing.EndPrice + Chart.GetMinTick * 2 Step -Chart.GetMinTick
'                            slaveChartPriceCurrentPrice2 -= sc.Settings("RangeValue").Value
'                            If IsLastBarOnChart Then
'                                sc.CalculatePriceChange(New BarData(slaveChartPriceCurrentPrice2, slaveChartPriceCurrentPrice2,
'                                                                    slaveChartPriceCurrentPrice2 - sc.Settings("RangeValue").Value, slaveChartPriceCurrentPrice2 - sc.Settings("RangeValue").Value,
'                                                                    CurrentBar.Date, TimeSpanFromBarSize(sc.Settings("BarSize").Value)))
'                            Else
'                                priceBarsToDraw2.Add(New BarData(slaveChartPriceCurrentPrice2, slaveChartPriceCurrentPrice2,
'                                                           slaveChartPriceCurrentPrice2 - sc.Settings("RangeValue").Value, slaveChartPriceCurrentPrice2 - sc.Settings("RangeValue").Value,
'                                                           CurrentBar.Date, TimeSpanFromBarSize(sc.Settings("BarSize").Value)))
'                            End If
'                        Next
'                        slaveChartPriceCurrentPrice2 -= sc.Settings("RangeValue").Value
'                        lastSlaveChartPrice2 = CurrentSwing.EndPrice
'                    End If
'                End If
'            ElseIf swings.Count > 0 AndAlso CurrentBar.High >= cs.EndPrice AndAlso cs.Direction = Direction.Up Then
'                ' extension up
'                If swings.Count > 0 Then
'                    If EnablePriceRVSlaveChart Then
'                        Dim sc = priceSlaveChart
'                        For i = cs.EndPrice + Chart.GetMinTick To CurrentBar.High Step Chart.GetMinTick
'                            If IsLastBarOnChart Then
'                                sc.CalculatePriceChange(New BarData(slaveChartPriceCurrentPrice, slaveChartPriceCurrentPrice + sc.Settings("RangeValue").Value,
'                                                                    slaveChartPriceCurrentPrice, slaveChartPriceCurrentPrice + sc.Settings("RangeValue").Value,
'                                                                    CurrentBar.Date, TimeSpanFromBarSize(sc.Settings("BarSize").Value)))
'                            Else
'                                priceBarsToDraw.Add(New BarData(slaveChartPriceCurrentPrice, slaveChartPriceCurrentPrice + sc.Settings("RangeValue").Value,
'                                                           slaveChartPriceCurrentPrice, slaveChartPriceCurrentPrice + sc.Settings("RangeValue").Value,
'                                                           CurrentBar.Date, TimeSpanFromBarSize(sc.Settings("BarSize").Value)))
'                            End If
'                            slaveChartPriceCurrentPrice += sc.Settings("RangeValue").Value
'                        Next
'                        lastSlaveChartPrice = CurrentSwing.EndPrice
'                    End If
'                    If EnablePriceRVSlaveChart2 Then
'                        Dim sc = priceSlaveChart2
'                        For i = cs.EndPrice + Chart.GetMinTick To CurrentBar.High Step Chart.GetMinTick
'                            If IsLastBarOnChart Then
'                                sc.CalculatePriceChange(New BarData(slaveChartPriceCurrentPrice2, slaveChartPriceCurrentPrice2 + sc.Settings("RangeValue").Value,
'                                                                    slaveChartPriceCurrentPrice2, slaveChartPriceCurrentPrice2 + sc.Settings("RangeValue").Value,
'                                                                    CurrentBar.Date, TimeSpanFromBarSize(sc.Settings("BarSize").Value)))
'                            Else
'                                priceBarsToDraw2.Add(New BarData(slaveChartPriceCurrentPrice2, slaveChartPriceCurrentPrice2 + sc.Settings("RangeValue").Value,
'                                                           slaveChartPriceCurrentPrice2, slaveChartPriceCurrentPrice2 + sc.Settings("RangeValue").Value,
'                                                           CurrentBar.Date, TimeSpanFromBarSize(sc.Settings("BarSize").Value)))
'                            End If
'                            slaveChartPriceCurrentPrice2 += sc.Settings("RangeValue").Value
'                        Next
'                        lastSlaveChartPrice2 = CurrentSwing.EndPrice
'                    End If
'                End If
'                cs.EndBar = CurrentBar.Number
'                cs.EndPrice = CurrentBar.High
'                currentSwingLengthText.Location = New Point(cs.EndBar - 1, cs.EndPrice)
'                Dim info As Decimal
'                If SwingTextOption = SwingTextType.Length Then
'                    info = GetDollarValue(Abs(swings(swings.Count - 1).StartPrice - swings(swings.Count - 1).EndPrice))
'                Else
'                    info = swings(swings.Count - 1).EndBar - swings(swings.Count - 1).StartBar
'                End If
'                currentSwingLengthText.Text = info & " "
'                curSwingEvent = OriginalAutoTrend.SwingEvent.Extension
'                swingDir = Direction.Up
'                If swings.Count > 1 Then
'                    If EnableBarRVSlaveChart Then
'                        Dim sc = barSlaveChart
'                        For i = lastSlaveChartBarIndex + 1 To CurrentSwing.EndBar
'                            If IsLastBarOnChart And Not loadingHistory Then
'                                sc.CalculatePriceChange(New BarData(slaveChartBarCurrentPrice, slaveChartBarCurrentPrice + sc.Settings("RangeValue").Value,
'                                                                    slaveChartBarCurrentPrice, slaveChartBarCurrentPrice + sc.Settings("RangeValue").Value,
'                                                                    CurrentBar.Date, TimeSpanFromBarSize(sc.Settings("BarSize").Value)))
'                            Else
'                                barBarsToDraw.Add(New BarData(slaveChartBarCurrentPrice, slaveChartBarCurrentPrice + sc.Settings("RangeValue").Value,
'                                                           slaveChartBarCurrentPrice, slaveChartBarCurrentPrice + sc.Settings("RangeValue").Value,
'                                                           CurrentBar.Date, TimeSpanFromBarSize(sc.Settings("BarSize").Value)))
'                            End If
'                            slaveChartBarCurrentPrice += sc.Settings("RangeValue").Value
'                        Next
'                        lastSlaveChartBarIndex = CurrentSwing.EndBar
'                    End If
'                End If
'            ElseIf swings.Count > 0 AndAlso CurrentBar.Low <= cs.EndPrice AndAlso cs.Direction = Direction.Down Then
'                ' extension down
'                If swings.Count > 0 Then
'                    If EnablePriceRVSlaveChart Then
'                        Dim sc = priceSlaveChart
'                        For i = cs.EndPrice - Chart.GetMinTick To CurrentBar.Low Step -Chart.GetMinTick
'                            If IsLastBarOnChart Then
'                                sc.CalculatePriceChange(New BarData(slaveChartPriceCurrentPrice, slaveChartPriceCurrentPrice,
'                                                                    slaveChartPriceCurrentPrice - sc.Settings("RangeValue").Value, slaveChartPriceCurrentPrice - sc.Settings("RangeValue").Value,
'                                                                    CurrentBar.Date, TimeSpanFromBarSize(sc.Settings("BarSize").Value)))
'                            Else
'                                priceBarsToDraw.Add(New BarData(slaveChartPriceCurrentPrice, slaveChartPriceCurrentPrice,
'                                                           slaveChartPriceCurrentPrice - sc.Settings("RangeValue").Value, slaveChartPriceCurrentPrice - sc.Settings("RangeValue").Value,
'                                                           CurrentBar.Date, TimeSpanFromBarSize(sc.Settings("BarSize").Value)))
'                            End If
'                            slaveChartPriceCurrentPrice -= sc.Settings("RangeValue").Value
'                        Next
'                        lastSlaveChartPrice = CurrentSwing.EndPrice - sc.Settings("RangeValue").Value
'                    End If
'                    If EnablePriceRVSlaveChart2 Then
'                        Dim sc = priceSlaveChart2
'                        For i = cs.EndPrice - Chart.GetMinTick To CurrentBar.Low Step -Chart.GetMinTick
'                            If IsLastBarOnChart Then
'                                sc.CalculatePriceChange(New BarData(slaveChartPriceCurrentPrice2, slaveChartPriceCurrentPrice2,
'                                                                    slaveChartPriceCurrentPrice2 - sc.Settings("RangeValue").Value, slaveChartPriceCurrentPrice2 - sc.Settings("RangeValue").Value,
'                                                                    CurrentBar.Date, TimeSpanFromBarSize(sc.Settings("BarSize").Value)))
'                            Else
'                                priceBarsToDraw2.Add(New BarData(slaveChartPriceCurrentPrice2, slaveChartPriceCurrentPrice2,
'                                                           slaveChartPriceCurrentPrice2 - sc.Settings("RangeValue").Value, slaveChartPriceCurrentPrice2 - sc.Settings("RangeValue").Value,
'                                                           CurrentBar.Date, TimeSpanFromBarSize(sc.Settings("BarSize").Value)))
'                            End If

'                            slaveChartPriceCurrentPrice2 -= sc.Settings("RangeValue").Value
'                        Next
'                        lastSlaveChartPrice2 = CurrentSwing.EndPrice - sc.Settings("RangeValue").Value
'                    End If
'                End If
'                cs.EndBar = CurrentBar.Number
'                cs.EndPrice = CurrentBar.Low
'                currentSwingLengthText.Location = New Point(cs.EndBar - 1, cs.EndPrice)
'                Dim info As Decimal
'                If SwingTextOption = SwingTextType.Length Then
'                    info = GetDollarValue(Abs(swings(swings.Count - 1).StartPrice - swings(swings.Count - 1).EndPrice))
'                Else
'                    info = swings(swings.Count - 1).EndBar - swings(swings.Count - 1).StartBar
'                End If
'                currentSwingLengthText.Text = info & " "
'                curSwingEvent = OriginalAutoTrend.SwingEvent.Extension
'                swingDir = Direction.Down
'                If swings.Count > 1 Then
'                    If EnableBarRVSlaveChart Then
'                        Dim sc = barSlaveChart
'                        For i = lastSlaveChartBarIndex + 1 To CurrentSwing.EndBar
'                            If IsLastBarOnChart And Not loadingHistory Then
'                                sc.CalculatePriceChange(New BarData(slaveChartBarCurrentPrice, slaveChartBarCurrentPrice, slaveChartBarCurrentPrice - sc.Settings("RangeValue").Value,
'                                                                    slaveChartBarCurrentPrice - sc.Settings("RangeValue").Value,
'                                                                    CurrentBar.Date, TimeSpanFromBarSize(sc.Settings("BarSize").Value)))
'                            Else
'                                barBarsToDraw.Add(New BarData(slaveChartBarCurrentPrice, slaveChartBarCurrentPrice, slaveChartBarCurrentPrice - sc.Settings("RangeValue").Value,
'                                                           slaveChartBarCurrentPrice - sc.Settings("RangeValue").Value,
'                                                           CurrentBar.Date, TimeSpanFromBarSize(sc.Settings("BarSize").Value)))
'                            End If
'                            slaveChartBarCurrentPrice -= sc.Settings("RangeValue").Value
'                        Next
'                        lastSlaveChartBarIndex = CurrentSwing.EndBar
'                    End If
'                End If
'            End If

'            If True Then
'                If curSwingEvent = SwingEvent.NewSwing Then
'                    If CurrentSwing.OverlapGapLine Is Nothing Then
'                        CurrentSwing.OverlapGapLine = NewTrendLine(SwingGapMarkerColor, New Point(CurrentSwing.StartBar, CurrentSwing.StartPrice), New Point(CurrentSwing.EndBar, CurrentSwing.StartPrice), CSwingGapMarkersVisible)
'                        CurrentSwing.OverlapGapLine.Pen.Thickness = SwingGapMarkerThickness
'                        CurrentSwing.OverlapGapLine.ExtendRight = True
'                    End If
'                    If swings.Count > 2 Then
'                        If swings(swings.Count - 3).OverlapGapLine IsNot Nothing AndAlso (Not swings(swings.Count - 3).OverlapGapLineGapped And Not swings(swings.Count - 3).OverlapGapLineDestroyed And Not swings(swings.Count - 3).OverlapGapLineHit) Then
'                            swings(swings.Count - 3).OverlapGapLine.ExtendRight = False
'                            swings(swings.Count - 3).OverlapGapLine.Pen.Thickness = HistorySwingCGapMarkerThickness
'                            swings(swings.Count - 3).OverlapGapLine.EndPoint = New Point(CurrentSwing.StartBar, swings(swings.Count - 3).OverlapGapLine.StartPoint.Y)
'                            swings(swings.Count - 3).OverlapGapLineGapped = True
'                            swings(swings.Count - 3).OverlapGapLine.Pen.Brush = New SolidColorBrush(GappedSwingGapMarkerColor)
'                        End If
'                    End If
'                    If swings.Count > 3 Then
'                        If swings(swings.Count - 4).OverlapGapLine IsNot Nothing AndAlso (Not swings(swings.Count - 4).OverlapGapLineGapped And Not swings(swings.Count - 4).OverlapGapLineDestroyed And swings(swings.Count - 4).OverlapGapLineHit) Then
'                            swings(swings.Count - 4).OverlapGapLine.ExtendRight = False
'                            swings(swings.Count - 4).OverlapGapLine.EndPoint = New Point(CurrentSwing.StartBar, swings(swings.Count - 4).OverlapGapLine.StartPoint.Y)
'                            swings(swings.Count - 4).OverlapGapLine.Pen.Thickness = HistorySwingBGapMarkerThickness
'                            swings(swings.Count - 4).OverlapGapLineGapped = True
'                            swings(swings.Count - 4).OverlapGapLine.Pen.Brush = New SolidColorBrush(GappedSwingGapMarkerColor)
'                        End If
'                    End If
'                ElseIf curSwingEvent = SwingEvent.Extension Then
'                    If CurrentSwing.OverlapGapLine IsNot Nothing And Not CurrentSwing.OverlapGapLineGapped And Not CurrentSwing.OverlapGapLineDestroyed And Not CurrentSwing.OverlapGapLineHit Then
'                        CurrentSwing.OverlapGapLine.EndPoint = New Point(CurrentSwing.EndBar, CurrentSwing.StartPrice)
'                    End If
'                End If
'                If curSwingEvent <> SwingEvent.None Then
'                    If swings.Count > 1 Then
'                        If swings(swings.Count - 2).OverlapGapLine IsNot Nothing AndAlso
'                            ((swingDir = Direction.Up And CurrentSwing.EndPrice >= swings(swings.Count - 2).OverlapGapLine.StartPoint.Y) Or
'                             (swingDir = Direction.Down And CurrentSwing.EndPrice <= swings(swings.Count - 2).OverlapGapLine.StartPoint.Y)) Then
'                            'swings(swings.Count - 2).OverlapGapLine.Pen.Brush = New SolidColorBrush(GappedSwingGapMarkerColor)
'                            swings(swings.Count - 2).OverlapGapLine.EndPoint = New Point(LinCalc(CurrentSwing.StartPrice, CurrentSwing.StartBar, CurrentSwing.EndPrice, CurrentSwing.EndBar, swings(swings.Count - 2).StartPrice), swings(swings.Count - 2).StartPrice)
'                            swings(swings.Count - 2).OverlapGapLineHit = True
'                            If BSwingGapMarkersVisible Then
'                                AddObjectToChart(swings(swings.Count - 2).OverlapGapLine)
'                                swings(swings.Count - 2).OverlapGapLine.RefreshVisual()
'                            Else
'                                RemoveObjectFromChart(swings(swings.Count - 2).OverlapGapLine)
'                            End If
'                        End If
'                        If swings(swings.Count - 2).OverlapGapLine IsNot Nothing AndAlso
'                            ((swingDir = Direction.Up And CurrentSwing.EndPrice >= swings(swings.Count - 2).OverlapGapLine.StartPoint.Y + RV) Or
'                             (swingDir = Direction.Down And CurrentSwing.EndPrice <= swings(swings.Count - 2).OverlapGapLine.StartPoint.Y - RV)) Then
'                            swings(swings.Count - 2).OverlapGapLine.Pen.Brush = New SolidColorBrush(PotentialSwingGapMarkerColor)
'                        End If
'                        If swings(swings.Count - 2).OverlapGapLine IsNot Nothing AndAlso (Not swings(swings.Count - 2).OverlapGapLineGapped And
'                            Not swings(swings.Count - 2).OverlapGapLineDestroyed And Not swings(swings.Count - 2).OverlapGapLineHit) Then
'                            swings(swings.Count - 2).OverlapGapLine.EndPoint = New Point(CurrentSwing.EndBar, swings(swings.Count - 2).OverlapGapLine.EndPoint.Y)
'                        End If

'                    End If
'                    If swings.Count > 2 Then
'                        If swings(swings.Count - 3).OverlapGapLine IsNot Nothing AndAlso (Not swings(swings.Count - 3).OverlapGapLineGapped And
'                            Not swings(swings.Count - 3).OverlapGapLineDestroyed And swings(swings.Count - 3).OverlapGapLineHit And
'                            ((swingDir = Direction.Up And CurrentSwing.EndPrice >= swings(swings.Count - 3).OverlapGapLine.StartPoint.Y) Or
'                            (swingDir = Direction.Down And CurrentSwing.EndPrice <= swings(swings.Count - 3).OverlapGapLine.StartPoint.Y))) Then
'                            RemoveObjectFromChart(swings(swings.Count - 3).OverlapGapLine)
'                            swings(swings.Count - 3).OverlapGapLineDestroyed = True
'                        End If
'                    End If
'                End If
'            End If
'            Dim evnt As Boolean = False
'            'If curSwingEvent <> SwingEvent.None And swings.Count > 1 Then
'            '    If ChannelMode = ChannelModeType.UnmergedOverlap Then
'            '        Dim ss As List(Of Swing)
'            '        ss = swings
'            '        If ss.Count > 1 Then
'            '            Dim s As Point = New Point(ss(ss.Count - 2).StartBar, ss(ss.Count - 2).StartPrice)
'            '            Dim e As Point = New Point(ss(ss.Count - 2).EndBar, ss(ss.Count - 2).EndPrice)
'            '            Dim dif As Decimal = ss(ss.Count - 1).EndPrice - ss(ss.Count - 1).StartPrice
'            '            Dim limitPoint As Decimal = e.Y + dif
'            '            If ss(ss.Count - 1).Direction = Direction.Up Then
'            '                limitPoint = Min(limitPoint, ss(ss.Count - 2).StartPrice)
'            '            Else
'            '                limitPoint = Max(limitPoint, ss(ss.Count - 2).StartPrice)
'            '            End If
'            '            If ss(ss.Count - 2).PreviousOverlapTrendLine IsNot Nothing Then
'            '                ss(ss.Count - 2).PreviousOverlapTrendLine.StartPoint = New Point(ss(ss.Count - 2).StartBar, ss(ss.Count - 2).StartPrice)
'            '                ss(ss.Count - 2).PreviousOverlapTrendLine.EndPoint = New Point(LinCalc(s.Y, s.X, e.Y, e.X, limitPoint), limitPoint)
'            '            End If
'            '            Dim s2 As Point = New Point(ss(ss.Count - 1).StartBar, ss(ss.Count - 1).StartPrice)
'            '            Dim e2 As Point = New Point(ss(ss.Count - 1).EndBar, ss(ss.Count - 1).EndPrice)
'            '            Dim rangePoint2 As Decimal
'            '            If ss(ss.Count - 1).Direction = Direction.Up Then
'            '                rangePoint2 = ss(ss.Count - 1).EndPrice - RawRV
'            '            Else ' these 
'            '                rangePoint2 = ss(ss.Count - 1).EndPrice + RawRV
'            '            End If
'            '            If ss(ss.Count - 1).PreviousOverlapTrendLine IsNot Nothing Then
'            '                ss(ss.Count - 1).PreviousOverlapTrendLine.StartPoint = New Point(ss(ss.Count - 1).StartBar, ss(ss.Count - 1).StartPrice)
'            '                ss(ss.Count - 1).PreviousOverlapTrendLine.EndPoint = New Point(LinCalc(s2.Y, s2.X, e2.Y, e2.X, rangePoint2), rangePoint2)
'            '            Else
'            '                ss(ss.Count - 1).PreviousOverlapTrendLine = NewTrendLine(If(ss(ss.Count - 1).Direction = Direction.Down, DownOverlapMarkerColor, UpOverlapMarkerColor), s2, New Point(LinCalc(s2.Y, s2.X, e2.Y, e2.X, rangePoint2), rangePoint2), Not HideLastSwingLine)
'            '                ss(ss.Count - 1).PreviousOverlapTrendLine.Pen.Thickness = 5
'            '            End If
'            '        End If
'            '    End If
'            'End If
'            If curSwingEvent = SwingEvent.NewSwing Then
'                'currentSwingCountGraphLine = NewTrendLine(If(swingDir = Direction.Down, ConfirmedDownChannelLineColor, ConfirmedUpChannelLineColor), New Point(CurrentBar.Number, swingCountGraphYPlacement), New Point(CurrentBar.Number, swingCountGraphYPlacement))
'                'currentSwingCountGraphLine.Pen.Thickness = 10
'            End If




'            ColorCurrentBars()

'            'check for removing of potential channels that are more than 3 swings in length
'            'If ChannelMode = ChannelModeType.Unmerged Then
'            '    Dim i As Integer
'            '    While i < channels.Count
'            '        Dim channel As Channel = channels(i)
'            '        If channel.IsConfirmed = False And channel.IsCut = False And channel.IsCancelled = False Then
'            '            If curSwingEvent = SwingEvent.NewSwing And channel.Direction <> swingDir Then
'            '                RemoveObjectFromChart(channel.TL)
'            '                channels.RemoveAt(i)
'            '                i -= 1
'            '            End If
'            '        End If
'            '        i += 1
'            '    End While
'            'End If












'            If swings.Count > 2 AndAlso curSwingEvent <> OriginalAutoTrend.SwingEvent.None Then ' AndAlso (channelLines.Count = 0 OrElse swingDir <> CurrentChannelLine.Direction) Then
'                Dim aPoint As Point, bPoint As Point, cPoint As Point = New Point(CurrentSwing.EndBar, CurrentSwing.EndPrice)
'                Dim setPoints As Boolean
'                Dim rangeBar As Integer
'                Dim rangePoint As Decimal
'                Dim heightToMatch As Decimal
'                Dim highPoint As Decimal, highBar As Integer, lowPoint As Decimal, lowBar As Integer
'                Dim lastConfirmedSwingIndex As Integer = GetLastConfirmedSwingIndex()
'                Dim firstConfirmedSwingIndex = GetFirstConfirmedSwingIndex(lastConfirmedSwingIndex)
'                If ChannelMode <> ChannelModeType.Merged Then
'                    If ChannelMode <> ChannelModeType.Merged Then
'                        Dim k As Integer
'                        While k < channels.Count
'                            If ((channels(k).Hidden And ChannelMode = ChannelModeType.Merged) Or ChannelMode <> ChannelModeType.Merged) And
'                             channels(k).APoint.X < swings(firstConfirmedSwingIndex).StartBar And channels(k).CPoint.X > swings(firstConfirmedSwingIndex).StartBar Then
'                                'Log("removing channel at bar " & channels(k).APoint.X & ". firstConfirmedSwingIndex is " & swings(firstConfirmedSwingIndex).StartBar & ", and cpoint is " & channels(k).CPoint.X)

'                                RemoveObjectFromChart(channels(k).TL)
'                                RemoveObjectFromChart(channels(k).BCLengthText)
'                                channels.RemoveAt(k)
'                            Else
'                                k += 1
'                            End If
'                        End While
'                    End If
'                    ' draw potential channel
'                    rangeBar = 0
'                    rangePoint = If(swingDir = Direction.Down, Decimal.MinValue, Decimal.MaxValue)
'                    ' find B-point
'                    For i = swings.Count - 1 To firstConfirmedSwingIndex Step -1
'                        If (swingDir = Direction.Down And swings(i).StartPrice > rangePoint) Or
'                            (swingDir = Direction.Up And swings(i).StartPrice < rangePoint) Then
'                            rangeBar = swings(i).StartBar
'                            rangePoint = swings(i).StartPrice
'                        End If
'                        If (swingDir = Direction.Down And swings(i).StartPrice < CurrentSwing.EndPrice) Or
'                            (swingDir = Direction.Up And swings(i).StartPrice > CurrentSwing.EndPrice) Then Exit For
'                    Next
'                    bPoint = New Point(rangeBar, rangePoint)
'                    ' find A-point 
'                    heightToMatch = Round(If(swingDir = Direction.Down, bPoint.Y - CurrentSwing.EndPrice, CurrentSwing.EndPrice - bPoint.Y), 5)
'                    highBar = 0
'                    lowBar = 0
'                    highPoint = Decimal.MinValue
'                    lowPoint = Decimal.MaxValue
'                    If swingDir = Direction.Down Then
'                        For i = swings.Count - 2 To firstConfirmedSwingIndex Step -1
'                            If swings(i).StartPrice < lowPoint Then
'                                lowBar = swings(i).StartBar
'                                lowPoint = swings(i).StartPrice
'                                highPoint = Decimal.MinValue
'                            Else
'                                highPoint = Max(highPoint, swings(i).StartPrice)
'                            End If
'                            If (highPoint <> Decimal.MinValue AndAlso highPoint - lowPoint > heightToMatch) Or i = firstConfirmedSwingIndex Then
'                                aPoint = New Point(lowBar, lowPoint)
'                                Exit For
'                            End If
'                        Next
'                    Else
'                        For i = swings.Count - 2 To firstConfirmedSwingIndex Step -1
'                            If swings(i).StartPrice > highPoint Then
'                                highBar = swings(i).StartBar
'                                highPoint = swings(i).StartPrice
'                                lowPoint = Decimal.MaxValue
'                            Else
'                                lowPoint = Min(lowPoint, swings(i).StartPrice)
'                            End If
'                            If highPoint - lowPoint > heightToMatch Or i = firstConfirmedSwingIndex Then
'                                aPoint = New Point(highBar, highPoint)
'                                Exit For
'                            End If
'                        Next
'                    End If
'                    setPoints = True

'                    If swings.Count - 1 = lastConfirmedSwingIndex + 1 Then
'                        If swings(firstConfirmedSwingIndex).Direction = Direction.Down Then
'                            If CurrentBar.High >= swings(firstConfirmedSwingIndex).StartPrice Then
'                                BarColor(CurrentSwing.StartBar, CurrentBar.Number - 1, UpTrendBarColor, UpTrendSwingLineColor, Swing.ChannelDirectionType.Up)
'                            End If
'                        Else
'                            If CurrentBar.Low <= swings(firstConfirmedSwingIndex).StartPrice Then
'                                BarColor(CurrentSwing.StartBar, CurrentBar.Number - 1, DownTrendBarColor, DownTrendSwingLineColor, Swing.ChannelDirectionType.Down)
'                            End If
'                        End If
'                    End If
'                    If (swingDir = Direction.Up And cPoint.Y < aPoint.Y) Or (swingDir = Direction.Down And cPoint.Y > aPoint.Y) Then
'                        DrawChannelIfLegalPlacement(aPoint, bPoint, cPoint, setPoints, False, False)
'                    End If
'                End If

'                If ChannelMode = ChannelModeType.Merged Then
'                    ' draw potential channel
'                    rangeBar = 0
'                    rangePoint = If(swingDir = Direction.Down, Decimal.MinValue, Decimal.MaxValue)
'                    ' find B-point
'                    For i = swings.Count - 1 To 0 Step -1
'                        If (swingDir = Direction.Down And swings(i).StartPrice > rangePoint) Or
'                            (swingDir = Direction.Up And swings(i).StartPrice < rangePoint) Then
'                            rangeBar = swings(i).StartBar
'                            rangePoint = swings(i).StartPrice
'                        End If
'                        If (swingDir = Direction.Down And swings(i).StartPrice < CurrentSwing.EndPrice) Or
'                            (swingDir = Direction.Up And swings(i).StartPrice > CurrentSwing.EndPrice) Then Exit For
'                    Next
'                    bPoint = New Point(rangeBar, rangePoint)
'                    ' find A-point 
'                    heightToMatch = Round(If(swingDir = Direction.Down, bPoint.Y - CurrentSwing.EndPrice, CurrentSwing.EndPrice - bPoint.Y), 5)
'                    highBar = 0
'                    lowBar = 0
'                    highPoint = Decimal.MinValue
'                    lowPoint = Decimal.MaxValue
'                    If swingDir = Direction.Down Then
'                        For i = swings.Count - 2 To 0 Step -1
'                            If swings(i).StartPrice < lowPoint Then
'                                lowBar = swings(i).StartBar
'                                lowPoint = swings(i).StartPrice
'                                highPoint = Decimal.MinValue
'                            Else
'                                highPoint = Max(highPoint, swings(i).StartPrice)
'                            End If
'                            If highPoint <> Decimal.MinValue AndAlso highPoint - lowPoint > heightToMatch Or i = 0 Then
'                                aPoint = New Point(lowBar, lowPoint)
'                                Exit For
'                            End If
'                        Next
'                    Else
'                        For i = swings.Count - 2 To 0 Step -1
'                            If swings(i).StartPrice > highPoint Then
'                                highBar = swings(i).StartBar
'                                highPoint = swings(i).StartPrice
'                                lowPoint = Decimal.MaxValue
'                            Else
'                                lowPoint = Min(lowPoint, swings(i).StartPrice)
'                            End If
'                            If highPoint - lowPoint > heightToMatch Or i = 0 Then
'                                aPoint = New Point(highBar, highPoint)
'                                Exit For
'                            End If
'                        Next
'                    End If
'                    setPoints = True

'                    'DrawChannelIfLegalPlacement(aPoint, bPoint, cPoint, setPoints, False, False)
'                End If


















'                ' check current potential channel for cancelling
'                'If swings.Count > 0 AndAlso CurrentSwing.ChannelLine IsNot Nothing AndAlso CurrentSwing.ChannelLine.IsCancelled = False AndAlso CurrentSwing.ChannelLine.IsConfirmed = False Then
'                '    If (CurrentSwing.ChannelLine.Direction = Direction.Down And CurrentSwing.EndPrice >= CurrentSwing.ChannelLine.APoint.Y) Or
'                '        (CurrentSwing.ChannelLine.Direction = Direction.Up And CurrentSwing.EndPrice <= CurrentSwing.ChannelLine.APoint.Y) Then ' if price crosses cancel point
'                '        ' cancel
'                '        CurrentSwing.ChannelLine.IsCancelled = True
'                '        channels.Remove(CurrentSwing.ChannelLine)
'                '        RemoveObjectFromChart(CurrentSwing.ChannelLine.BCLengthText)
'                '        RemoveObjectFromChart(CurrentSwing.ChannelLine.TL)
'                '        ShowNextBackedUpChannel(CurrentSwing.ChannelLine.Direction, False)
'                '        CurrentSwing.ChannelLine = Nothing
'                '    End If
'                'End If

'                ' check to remove current partially cut channel
'                'If HighlightLastCutChannel AndAlso currentPartiallyCutChannel IsNot Nothing AndAlso swings.Count - 1 > currentPartiallyCutChannel.SwingNum Then
'                '    Dim swingLowPoint As Decimal = Min(swings(currentPartiallyCutChannel.SwingNum).EndPrice, swings(currentPartiallyCutChannel.SwingNum).StartPrice)
'                '    Dim swingHighPoint As Decimal = Max(swings(currentPartiallyCutChannel.SwingNum).EndPrice, swings(currentPartiallyCutChannel.SwingNum).StartPrice)
'                '    If CurrentSwing.EndPrice <= swingLowPoint Or CurrentSwing.EndPrice >= swingHighPoint Then
'                '        FullyCutChannel(currentPartiallyCutChannel.Channel)
'                '        currentPartiallyCutChannel = Nothing
'                '    End If
'                'End If

'                Dim indx As Integer
'                While indx < channels.Count
'                    Dim channel As Channel = channels(indx)
'                    If channel IsNot Nothing AndAlso channel.IsCancelled = False AndAlso channel.IsConfirmed = False Then
'                        If (channel.Direction = Direction.Down And CurrentSwing.EndPrice > channel.CPoint.Y And channel.Swing.StartBar = swings(swings.Count - 1).StartBar) Or
'                            (channel.Direction = Direction.Up And CurrentSwing.EndPrice < channel.CPoint.Y And channel.Swing.StartBar = swings(swings.Count - 1).StartBar) Then ' if price crosses cancel point
'                            ' cancel
'                            channel.IsCancelled = True
'                            channels.Remove(channel)
'                            RemoveObjectFromChart(channel.BCLengthText)
'                            RemoveObjectFromChart(channel.TL)
'                            If Not channel.Hidden And channel.DontDraw = False Then
'                                'ShowNextBackedUpChannel(channel.Direction, False)
'                            End If
'                            If CurrentSwing.ChannelLine Is channel Then CurrentSwing.ChannelLine = Nothing
'                            indx -= 1
'                        End If
'                    End If

'                    If channel.IsCut = False Then
'                        ' check to update BCSwingMatched flag
'                        If channel.IsConfirmed And channel.BCSwingMatched = False And swingDir <> channel.Direction And CurrentSwing.TL.EndPoint <> channel.Swing.TL.EndPoint Then
'                            If SwingTextOption = SwingTextType.Length Then
'                                If Abs(bPoint.Y - cPoint.Y) >= Abs(channel.BPoint.Y - channel.CPoint.Y) Then ' if bc swing length matched
'                                    channel.BCSwingMatched = True
'                                End If
'                            Else
'                                If Abs(bPoint.X - cPoint.X) >= Abs(channel.BPoint.X - channel.CPoint.X) Then ' if bc swing bar count matched
'                                    channel.BCSwingMatched = True
'                                End If
'                            End If
'                        End If
'                        ' check potential channels for confirmation
'                        If channel.IsConfirmed = False And channel.IsCancelled = False Then
'                            If ((channel.Direction = Direction.Down And CurrentSwing.EndPrice <= channel.BPoint.Y) Or
'                                (channel.Direction = Direction.Up And CurrentSwing.EndPrice >= channel.BPoint.Y)) Then
'                                ' confirm channel

'                                'If ChannelMode = ChannelModeType.Merged Then
'                                '    If GetChannelSwingLength(channel) = 3 Then
'                                '        BarColor(channel.APoint.X, CurrentBar.Number - 1, If(channel.Direction = Direction.Down, DownTrendBarColor, UpTrendBarColor), If(channel.Direction = Direction.Down, DownTrendSwingLineColor, UpTrendSwingLineColor), channel.Direction)
'                                '    Else
'                                '        BarColor(channel.APoint.X, CurrentBar.Number - 1, If(channel.Direction = Direction.Down, DownTrendBarColor, UpTrendBarColor), If(channel.Direction = Direction.Down, DownTrendSwingLineColor, UpTrendSwingLineColor), channel.Direction, True, NeutralSwingLineColor, NeutralBarColor)
'                                '    End If
'                                'Else
'                                If channel.DontColorBars = False Then '(ChannelMode = ChannelModeType.Merged And channel.Hidden = True) Or (ChannelMode <> ChannelModeType.Merged) Then
'                                    'If channel.APoint.X <= swings(GetLastConfirmedSwingIndex).StartBar Then
'                                    BarColor(channel.APoint.X, CurrentBar.Number - 1, If(channel.Direction = Direction.Down, DownTrendBarColor, UpTrendBarColor), If(channel.Direction = Direction.Down, DownTrendSwingLineColor, UpTrendSwingLineColor), channel.Direction, False)
'                                    'End If
'                                End If
'                                'End If

'                                channel.CutBarNumber = -1
'                                channel.IsConfirmed = True
'                                channel.TL.Pen = New Pen(New SolidColorBrush(If(channel.Direction = Direction.Down, ConfirmedDownChannelLineColor, ConfirmedUpChannelLineColor)), 1)
'                                'NewLabel(channel.TL.EndPoint.ToString, Colors.White, channel.TL.EndPoint, True)


'                                Dim bcLength As Decimal = GetDollarValue(Abs(channel.CPoint.Y - channel.BPoint.Y))
'                                If SwingTextOption = SwingTextType.BarCount Then
'                                    bcLength = channel.CPoint.X - channel.BPoint.X
'                                End If
'                                If Not channel.Hidden And channel.DontDraw = False Then
'                                    channel.BCLengthText.Font.Brush = New SolidColorBrush(If(channel.Direction = Direction.Down, ConfirmedDownChannelLineColor, ConfirmedUpChannelLineColor))
'                                    channel.BCLengthText.Text = FormatNumber(bcLength, DecimalPlaces, TriState.True)
'                                    channel.BCLengthText.Location = AddToX(channel.CPoint, -2)
'                                End If
'                                ' target text
'                                channel.TargetText = NewLabel("- " & FormatNumber(bcLength, DecimalPlaces), If(channel.Direction = Direction.Down, ConfirmedDownChannelLineColor, ConfirmedUpChannelLineColor), New Point(CurrentBar.Number, CurrentSwing.EndPrice - BooleanToInteger(channel.Direction) * bcLength), BCTargetTextsVisible And Not channel.Hidden And channel.DontDraw = False)
'                                channel.TargetText.Font.FontSize = TargetTextFontSize
'                                channel.TargetText.Font.FontWeight = TargetTextFontWeight
'                                channel.TargetText.VerticalAlignment = LabelVerticalAlignment.Center
'                                channel.TargetText.HorizontalAlignment = LabelHorizontalAlignment.Left

'                                ' extension point
'                                channel.DPoint = CurrentSwing.TL.EndPoint
'                                ' swing lines
'                                'channel.ABSwingLine = NewTrendLine(If(channel.Direction = Direction.Down, ConfirmedDownChannelLineColor, ConfirmedUpChannelLineColor), channel.APoint, channel.BPoint, ChannelLinesVisible)
'                                'channel.ABSwingLine.Pen.Thickness = 2
'                                channel.BCSwingLine = NewTrendLine(If(channel.Direction = Direction.Down, ConfirmedDownChannelLineColor, ConfirmedUpChannelLineColor), channel.BPoint, channel.CPoint, False)
'                                channel.BCSwingLine.Pen.Thickness = 2
'                                'channel.CDSwingLine = NewTrendLine(If(channel.Direction = Direction.Down, ConfirmedDownChannelLineColor, ConfirmedUpChannelLineColor), channel.CPoint, channel.DPoint, ChannelLinesVisible)
'                                'channel.CDSwingLine.Pen.Thickness = 2

'                                BackupChannel(channel)
'                                If Not channel.Hidden And channel.DontDraw = False Then
'                                    ShowNextBackedUpChannel(channel.Direction, False)
'                                End If

'                                If ChannelMode = ChannelModeType.Unmerged Or ChannelMode = ChannelModeType.UnmergedOverlap Then
'                                    If currentChannelEndSwingIndex = 0 Then
'                                        currentChannelEndSwingIndex = swings.Count - 1
'                                        currentChannelUnmerged = channel
'                                        For i = 0 To swings.Count - 1
'                                            If channel.APoint.X = swings(i).StartBar Then
'                                                currentChannelBeginSwingIndex = i
'                                                Exit For
'                                            End If
'                                        Next
'                                    End If
'                                End If

'                                'If CurrentChannelLine Is channel Then CurrentSwing.ChannelLine = Nothing

'                            Else
'                                'channels.Remove(channel)
'                                'Continue While
'                            End If
'                        End If
'                        ' update D-point (extend)
'                        If channel.IsConfirmed And swingDir = channel.Direction Then
'                            If (swingDir = Direction.Down And CurrentSwing.EndPrice <= channel.DPoint.Y) Or
'                                (swingDir = Direction.Up And CurrentSwing.EndPrice >= channel.DPoint.Y) Then ' if extension
'                                channel.DPoint = CurrentSwing.TL.EndPoint

'                                'channel.CDSwingLine.EndPoint = channel.DPoint
'                            End If
'                        End If
'                        ' check channel for BC swing length match
'                        If channel.IsConfirmed And channel.BCSwingLineRemoved = False And channel.BCSwingMatched And swingDir <> channel.Direction Then
'                            'channel.TL.Pen = New Pen(New SolidColorBrush(If(channel.Direction = Direction.Down, ConfirmedHistoryDownChannelLineColor, ConfirmedHistoryUpChannelLineColor)), 1)
'                            'channel.ABSwingLine.Pen = New Pen(New SolidColorBrush(If(channel.Direction = Direction.Down, ConfirmedHistoryDownChannelLineColor, ConfirmedHistoryUpChannelLineColor)), 1)
'                            channel.BCSwingLine.Pen = New Pen(New SolidColorBrush(If(channel.Direction = Direction.Down, ConfirmedHistoryDownChannelLineColor, ConfirmedHistoryUpChannelLineColor)), 1)
'                            'channel.CDSwingLine.Pen = New Pen(New SolidColorBrush(If(channel.Direction = Direction.Down, ConfirmedHistoryDownChannelLineColor, ConfirmedHistoryUpChannelLineColor)), 1)
'                            If RV < HistoryBCLengthVisibilityRVCutoff * Chart.Settings("RangeValue").Value Then RemoveObjectFromChart(channel.BCLengthText)
'                            RemoveObjectFromChart(channel.TargetText)
'                            channel.BCSwingLineRemoved = True
'                            'ShowNextBackedUpChannel(channel.Direction, True)
'                        End If
'                        ' check channels for hit
'                        If channel.IsCancelled = False And channel.CutBarNumber = -1 And swingDir <> channel.Direction And channel.CPoint.X <> cPoint.X Then
'                            If (channel.Direction = Direction.Down AndAlso CurrentBar.High > LinCalc(channel.TL.StartPoint.X, channel.TL.StartPoint.Y, channel.TL.EndPoint.X, channel.TL.EndPoint.Y, CurrentBar.Number)) OrElse
'                                (channel.Direction = Direction.Up AndAlso CurrentBar.Low < LinCalc(channel.TL.StartPoint.X, channel.TL.StartPoint.Y, channel.TL.EndPoint.X, channel.TL.EndPoint.Y, CurrentBar.Number)) Then ' if base line has been crossed
'                                channel.CutBarNumber = CurrentBar.Number
'                            End If
'                        End If
'                        ' check channels to cut
'                        If channel.CutBarNumber <> -1 And channel.CPoint <> CurrentSwing.TL.EndPoint And swingDir <> channel.Direction And
'                            (
'                                (channel.IsConfirmed And channel.BCSwingMatched) Or
'                                (
'                                    channel.IsConfirmed = False And channel.IsCancelled = False And
'                                    (
'                                        (channel.Direction = Direction.Down And CurrentSwing.EndPrice >= channel.CPoint.Y) Or
'                                        (channel.Direction = Direction.Up And CurrentSwing.EndPrice <= channel.CPoint.Y)
'                                    )
'                                )
'                            ) Then
'                            CutChannel(channel)
'                        End If
'                        If channel.IsConfirmed And channel.BCSwingMatched And channel.IsCut = False And channel.IsGapped = False And channel.GappedSwing Is Nothing Then
'                            channel.GappedSwing = CurrentSwing
'                            channel.TL.Pen.Brush = New SolidColorBrush(PartialGapChannelLineColor)
'                            If channel.IsBackedUp Then
'                                channel.IsBackedUp = False
'                                AddObjectToChart(channel.TL)
'                            End If
'                        End If
'                        If channel.IsGapped = False AndAlso channel.GappedSwing IsNot Nothing AndAlso ((channel.Direction = Direction.Down And CurrentSwing.EndPrice > channel.GappedSwing.EndPrice) Or (channel.Direction = Direction.Up And CurrentSwing.EndPrice < channel.GappedSwing.EndPrice)) Then
'                            channel.GappedSwing = CurrentSwing
'                        End If
'                        If channel.IsGapped = False AndAlso channel.GappedSwing IsNot Nothing AndAlso ((channel.Direction = Direction.Down And CurrentSwing.EndPrice < channel.GappedSwing.EndPrice - Abs(channel.BPoint.Y - channel.CPoint.Y)) Or (channel.Direction = Direction.Up And CurrentSwing.EndPrice > channel.GappedSwing.EndPrice + Abs(channel.BPoint.Y - channel.CPoint.Y))) Then
'                            channel.IsGapped = True
'                            channel.TL.ExtendRight = False
'                            channel.TL.EndPoint = New Point(channel.GappedSwing.EndBar, LinCalc(channel.TL.StartPoint.X, channel.TL.StartPoint.Y, channel.TL.EndPoint.X, channel.TL.EndPoint.Y, channel.GappedSwing.EndBar))
'                            channel.TL.Pen = New Pen(New SolidColorBrush(If(channel.Direction = Direction.Down, GappedDownChannelLineColor, GappedUpChannelLineColor)), GappedChannelLineWidth)
'                            If Not GappedChannelLineParallelsVisible Then channel.TL.HasParallel = False
'                            If RV < HistoryBCLengthVisibilityRVCutoff * Chart.Settings("RangeValue").Value Then RemoveObjectFromChart(channel.BCLengthText)
'                            If Not channel.Hidden And channel.IsBackedUp = False And channel.DontDraw = False Then
'                                '(channel.Direction, True)
'                            End If
'                            If ShowOnlyLastConfirmedChannels Then
'                                channel.IsBackedUp = False
'                                channel.TL.Pen.Thickness = GappedChannelLineWidth
'                            End If
'                        End If
'                    End If
'                    indx += 1
'                End While
'            End If
'            'Dim curChannel As Channel = CurrentConfirmedChannelLine
'            'If curChannel IsNot Nothing Then
'            '    If (swingDir = Direction.Down And CurrentSwing.EndPrice <= curChannel.DPoint.Y) Or
'            '                    (swingDir = Direction.Up And CurrentSwing.EndPrice >= curChannel.DPoint.Y) Then
'            '        If curChannel.Hidden Or ChannelMode <> ChannelModeType.Merged Then BarColor(curChannel.APoint.X, CurrentBar.Number - 1, If(curChannel.Direction = Direction.Down, DownTrendBarColor, UpTrendBarColor), If(curChannel.Direction = Direction.Down, DownTrendSwingLineColor, UpTrendSwingLineColor), curChannel.Direction)
'            '    End If
'            'End If
'            If curSwingEvent = SwingEvent.NewSwing Then RemoveSwingChannel()
'            If swings.Count > 0 And SwingChannelLinesVisible Then ProcessSwingChannel(CurrentSwing)
'            DrawProjectionLineAndRVText()
'            ProcessCurrentGapMark()
'            ' check to remove partially cut channel
'            'If HighlightLastCutChannel AndAlso currentPartiallyCutChannel IsNot Nothing Then
'            '    For Each channel In channels
'            '        If channel.IsConfirmed And channel.IsGapped = False And channel.APoint.X > currentPartiallyCutChannel.Channel.CPoint.X Then
'            '            FullyCutChannel(currentPartiallyCutChannel.Channel)
'            '            currentPartiallyCutChannel = Nothing
'            '            Exit For
'            '        End If
'            '    Next
'            'End If
'            If IsLastBarOnChart Then
'                'If Chart.TickerID = 7 Then
'                'End If
'                Dim zeros As Boolean = True
'                If barBarsToDraw.Count <> 0 AndAlso barSlaveChart IsNot Chart Then
'                    Dim sc = barSlaveChart
'                    For Each item In barBarsToDraw
'                        sc.IsLoadingHistory = True
'                        sc.AddBarWrapper(item.Open, item.High, item.Low, item.Close, item.Date)
'                    Next
'                    sc.IsLoadingHistory = False
'                    barBarsToDraw.Clear()

'                    sc.SetScaling(barSlaveChart.Bounds.Size)
'                    'sc.SetStatusText("applying analysis techniques...", True)
'                    For Each item In sc.AnalysisTechniques
'                        sc.RemoveAnalysisTechnique(item.AnalysisTechnique)
'                        item.AnalysisTechnique.Reset()
'                        sc.ApplyAnalysisTechnique(item.AnalysisTechnique)
'                    Next
'                    'sc.SetStatusTextToDefault(False)
'                    zeros = False
'                End If
'                If priceBarsToDraw.Count <> 0 AndAlso priceSlaveChart IsNot Chart Then
'                    Dim sc = priceSlaveChart
'                    For Each item In priceBarsToDraw
'                        sc.IsLoadingHistory = True
'                        sc.AddBarWrapper(item.Open, item.High, item.Low, item.Close, item.Date)
'                    Next
'                    sc.IsLoadingHistory = False
'                    priceBarsToDraw.Clear()
'                    sc.SetScaling(sc.Bounds.Size)
'                    'sc.SetStatusText("applying analysis techniques...", True)
'                    For Each item In sc.AnalysisTechniques
'                        sc.RemoveAnalysisTechnique(item.AnalysisTechnique)
'                        item.AnalysisTechnique.Reset()
'                        sc.ApplyAnalysisTechnique(item.AnalysisTechnique)
'                    Next
'                    'sc.SetStatusTextToDefault(False)
'                    zeros = False
'                End If
'                If priceBarsToDraw2.Count <> 0 AndAlso priceSlaveChart2 IsNot Chart Then
'                    Dim sc = priceSlaveChart2
'                    For Each item In priceBarsToDraw2
'                        sc.IsLoadingHistory = True
'                        sc.AddBarWrapper(item.Open, item.High, item.Low, item.Close, item.Date)
'                    Next
'                    sc.IsLoadingHistory = False
'                    priceBarsToDraw2.Clear()
'                    sc.SetScaling(sc.Bounds.Size)
'                    'sc.SetStatusText("applying analysis techniques...", True)
'                    For Each item In sc.AnalysisTechniques
'                        sc.RemoveAnalysisTechnique(item.AnalysisTechnique)
'                        item.AnalysisTechnique.Reset()
'                        sc.ApplyAnalysisTechnique(item.AnalysisTechnique)
'                    Next
'                    'sc.SetStatusTextToDefault(False)
'                    zeros = False
'                End If
'                If zeros Then
'                    If EnablePriceRVSlaveChart AndAlso priceSlaveChart IsNot Chart Then
'                        Dim sc = priceSlaveChart
'                        sc.bars(sc.bars.Count - 1).Data = sc.bars(sc.bars.Count - 1).Data.SetClose(CurrentBar.Close)
'                        sc.UpdatePrice()
'                    End If
'                    If EnablePriceRVSlaveChart2 AndAlso priceSlaveChart2 IsNot Chart Then
'                        Dim sc = priceSlaveChart2
'                        sc.bars(sc.bars.Count - 1).Data = sc.bars(sc.bars.Count - 1).Data.SetClose(CurrentBar.Close)
'                        sc.UpdatePrice()
'                    End If
'                End If
'            End If
'            If ColorBars And ChannelMode = ChannelModeType.Merged Then
'                If CurrentBar.Low < dayLowPrice Then
'                    dayLowPrice = CurrentBar.Low
'                    BarColor(0, CurrentBar.Number - 1, DownTrendBarColor, DownTrendSwingLineColor, Swing.ChannelDirectionType.Down)
'                End If
'                If CurrentBar.High > dayHighPrice Then
'                    dayHighPrice = CurrentBar.High
'                    BarColor(0, CurrentBar.Number - 1, UpTrendBarColor, UpTrendSwingLineColor, Swing.ChannelDirectionType.Up)
'                End If
'            End If
'        End Sub
'        Protected Overrides Sub NewBar()
'            ' remove channels for crossing the left side of the chart
'            For Each channel In channels
'                If channel.IsCancelled = False Then
'                    If ShowOnlyChannelsOnScreen Then
'                        If channel.TL.StartPoint.X < Chart.Bounds.X1 Then ' if channel startpoint is off the left of the chart
'                            RemoveObjectFromChart(channel.TL)
'                        ElseIf (
'                                (channel.IsCut = False And ChannelLinesVisible) Or
'                                (channel.IsCut And
'                                    (
'                                         (channel.IsConfirmed And ConfirmedHistoryChannelLinesVisible) Or
'                                         (channel.IsConfirmed = False And PotentialHistoryChannelLinesVisible)
'                                     )
'                                )
'                               ) And
'                                channel.IsBackedUp = False Then
'                            If channel.Hidden = False And channel.DontDraw = False Then
'                                AddObjectToChart(channel.TL)
'                            End If
'                        End If
'                    ElseIf ((channel.IsCut = False And ChannelLinesVisible) Or (channel.IsCut And ((channel.IsConfirmed And ConfirmedHistoryChannelLinesVisible) Or (channel.IsConfirmed = False And PotentialHistoryChannelLinesVisible)))) And channel.IsBackedUp = False Then
'                        If channel.Hidden = False And channel.DontDraw = False Then
'                            AddObjectToChart(channel.TL)
'                        End If
'                    End If
'                End If
'            Next
'        End Sub
'        Private Function GetLastConfirmedSwingIndex() As Integer
'            Dim lastConfirmedSwingIndex As Integer = 0
'            For i = 0 To swings.Count - 1
'                If swings(i).ChannelDirection <> Swing.ChannelDirectionType.Neutral Then
'                    For j = i To swings.Count - 1
'                        If swings(j).ChannelDirection = Swing.ChannelDirectionType.Neutral Then
'                            lastConfirmedSwingIndex = j - 1
'                            Exit For
'                        End If
'                    Next
'                    If lastConfirmedSwingIndex = 0 Then
'                        lastConfirmedSwingIndex = swings.Count - 1
'                    End If
'                    Exit For
'                End If
'            Next
'            Return lastConfirmedSwingIndex
'        End Function
'        Private Function GetFirstConfirmedSwingIndex(lastConfirmedSwingIndex As Integer) As Integer
'            Dim firstConfirmedSwingIndex As Integer
'            For i = lastConfirmedSwingIndex To 0 Step -1
'                Dim currentCol = swings(lastConfirmedSwingIndex).ChannelDirection
'                If swings(i).ChannelDirection <> currentCol Then
'                    firstConfirmedSwingIndex = i + 1
'                    Exit For
'                End If
'            Next
'            If firstConfirmedSwingIndex = 0 Then firstConfirmedSwingIndex = 1
'            Return firstConfirmedSwingIndex
'        End Function
'        Private Sub DrawChannelIfLegalPlacement(apoint As Point, bpoint As Point, cpoint As Point, setPoints As Boolean, dontColorBars As Boolean, hidden As Boolean)
'            ' draw potential channel if legal placement 
'            Dim legal As Boolean
'            legal = True

'            If ((swingDir = Direction.Down And apoint.Y < cpoint.Y) Or
'                (swingDir = Direction.Up And apoint.Y > cpoint.Y)) And setPoints And legal And apoint.X <> 0 Then
'                Dim go As Boolean
'                If ChannelMode <> ChannelModeType.Merged Then
'                    go = CurrentSwing.ChannelLine Is Nothing
'                Else
'                    If channels.Count > 1 Then
'                        go = channels(channels.Count - 2).CPoint.X <> cpoint.X
'                    Else
'                        go = True
'                    End If
'                End If
'                'go = CurrentSwing.ChannelLine Is Nothing
'                Dim lowPoint As New Point(0, Decimal.MaxValue), highPoint As Point = New Point(0, 0)
'                Dim startIndex As Integer = -1
'                For i = 0 To swings.Count - 1
'                    If apoint.X = swings(i).StartBar Then
'                        startIndex = i - 1
'                    End If
'                Next
'                For i = 0 To startIndex
'                    If swings(i).StartPrice <= lowPoint.Y Then
'                        lowPoint = New Point(swings(i).StartBar, swings(i).StartPrice)
'                    End If
'                    If swings(i).EndPrice <= lowPoint.Y Then
'                        lowPoint = New Point(swings(i).EndBar, swings(i).EndPrice)
'                    End If
'                    If swings(i).StartPrice >= highPoint.Y Then
'                        highPoint = New Point(swings(i).StartBar, swings(i).StartPrice)
'                    End If
'                    If swings(i).EndPrice >= highPoint.Y Then
'                        highPoint = New Point(swings(i).EndBar, swings(i).EndPrice)
'                    End If
'                Next
'                Dim bcExceeded As Boolean
'                If swingDir = Direction.Down Then
'                    If highPoint.Y - apoint.Y < Abs(cpoint.Y - bpoint.Y) Then
'                        bcExceeded = True
'                    End If
'                Else
'                    If apoint.Y - lowPoint.Y < Abs(cpoint.Y - bpoint.Y) Then
'                        bcExceeded = True
'                    End If
'                End If
'                If bcExceeded Then Exit Sub
'                If go Then ' if no channel on current swing
'                    Dim channel As New Channel(NewTrendLine(If(swingDir = Direction.Down, PotentialUpChannelLineColor, PotentialDownChannelLineColor),
'                                                          apoint, CurrentSwing.TL.EndPoint, ChannelLinesVisible And Not hidden And Not bcExceeded))

'                    CurrentSwing.ChannelLine = channel
'                    channel.Swing = CurrentSwing
'                    channels.Add(channel)
'                    ' properties
'                    channel.TL.ExtendRight = True
'                    'If Not hidden Then channel.TL.DragCircleSize = 15
'                    channel.IsCancelled = False
'                    channel.IsConfirmed = False
'                    channel.IsCut = False
'                    channel.Direction = Not swingDir
'                    channel.TL.HasParallel = True
'                    ' reset extension point
'                    channel.DPoint = If(channel.Direction = Direction.Down, New Point(0, Double.MaxValue), New Point(0, Double.MinValue))
'                    channel.DontColorBars = dontColorBars
'                    channel.Hidden = hidden
'                    If bcExceeded Then channel.DontDraw = True
'                    'If bcExceeded Then channel.TL.ExtendLeft = True
'                    'If hidden = False Then RemoveObjectFromChart(channel.TL)
'                    BackupChannel(channel)
'                Else
'                    ' update points
'                    If CurrentChannelLineIfPotential IsNot Nothing Then
'                        CurrentChannelLineIfPotential.TL.EndPoint = CurrentSwing.TL.EndPoint
'                        If setPoints Then
'                            CurrentChannelLineIfPotential.TL.StartPoint = apoint
'                            Dim curChannel As Channel = CurrentChannelLineIfPotential

'                        End If

'                    End If
'                End If
'                ' update
'                If CurrentChannelLineIfPotential IsNot Nothing Then
'                    If setPoints And hidden = CurrentChannelLineIfPotential.Hidden Then
'                        Dim curChannel As Channel = CurrentChannelLineIfPotential
'                        curChannel.APoint = apoint
'                        curChannel.BPoint = bpoint
'                        curChannel.CPoint = cpoint
'                        curChannel.TL.ParallelDistance = bpoint.Y - LinCalc(apoint.X, apoint.Y, cpoint.X, cpoint.Y, bpoint.X)
'                        If Not curChannel.Hidden And curChannel.DontDraw = False Then
'                            ' bc text
'                            Dim bcLength As Decimal = GetDollarValue(Abs(curChannel.CPoint.Y - curChannel.BPoint.Y))
'                            If SwingTextOption = SwingTextType.BarCount Then
'                                bcLength = curChannel.CPoint.X - curChannel.BPoint.X
'                            End If
'                            If curChannel.BCLengthText Is Nothing Then
'                                curChannel.BCLengthText = NewLabel(FormatNumber(bcLength, DecimalPlaces, TriState.True), If(curChannel.Direction = Direction.Down, PotentialDownChannelLineColor, PotentialUpChannelLineColor), AddToX(curChannel.CPoint, -2), BCLengthTextsVisible And curChannel.IsBackedUp = False)
'                                curChannel.BCLengthText.Font.FontSize = LengthTextFontSize
'                                curChannel.BCLengthText.HorizontalAlignment = LabelHorizontalAlignment.Right
'                                curChannel.BCLengthText.VerticalAlignment = LabelVerticalAlignment.Center
'                            Else
'                                curChannel.BCLengthText.Text = FormatNumber(bcLength, DecimalPlaces, TriState.True)
'                                curChannel.BCLengthText.Location = AddToX(curChannel.CPoint, -2)
'                            End If
'                        End If
'                    End If
'                    'If CurrentSwing.ChannelLine Is Nothing Then
'                    CurrentSwing.ChannelLine = CurrentChannelLineIfPotential
'                    CurrentChannelLineIfPotential.Swing = CurrentSwing

'                    'End If
'                End If
'            End If
'        End Sub

'        Private Sub ColorCurrentBars()
'            If ColorBars Then
'                barColorsDirty = True

'                CType(Chart.bars(CurrentBar.Number - 1).Pen.Brush, SolidColorBrush).Color = NeutralBarColor
'                RefreshObject(Chart.bars(CurrentBar.Number - 1))
'                'Exit Sub

'                If curSwingEvent = SwingEvent.Extension Or curSwingEvent = SwingEvent.NewSwing Then
'                    If CurrentSwing.ChannelDirection = Swing.ChannelDirectionType.Up Then
'                        BarColorRoutine(CurrentSwing.StartBar, CurrentBar.Number - 1, UpTrendBarColor)
'                    ElseIf CurrentSwing.ChannelDirection = Swing.ChannelDirectionType.Neutral Then
'                        BarColorRoutine(CurrentSwing.StartBar, CurrentBar.Number - 1, NeutralBarColor)
'                    ElseIf CurrentSwing.ChannelDirection = Swing.ChannelDirectionType.Down Then
'                        BarColorRoutine(CurrentSwing.StartBar, CurrentBar.Number - 1, DownTrendBarColor)
'                    End If
'                    'If ChannelMode = ChannelModeType.Merged Or ChannelMode = ChannelModeType.SetToBase Then
'                    '    Dim curChannel As Channel = Nothing
'                    '    For i = channels.Count - 1 To 0 Step -1
'                    '        If channels(i).IsConfirmed Then
'                    '            Dim count As Integer
'                    '            For Each swing In swings
'                    '                If swing.StartBar >= channels(i).APoint.X And swing.EndBar <= channels(i).CPoint.X Then
'                    '                    count += 1
'                    '                End If
'                    '            Next
'                    '            If count = 3 Then
'                    '                curChannel = channels(i)
'                    '                Exit For
'                    '            End If
'                    '        End If
'                    '    Next
'                    '    If curChannel IsNot Nothing AndAlso curChannel.Direction = CurrentSwing.Direction Then
'                    '        If (curChannel.Direction = Direction.Down And CurrentSwing.EndPrice <= curChannel.DPoint.Y) Or
'                    '            (curChannel.Direction = Direction.Up And CurrentSwing.EndPrice >= curChannel.DPoint.Y) Then
'                    '            BarColor(curChannel.APoint.X, CurrentBar.Number - 1, If(curChannel.Direction = Direction.Down, DownTrendBarColor, UpTrendBarColor), If(curChannel.Direction = Direction.Down, DownTrendSwingLineColor, UpTrendSwingLineColor), curChannel.Direction)
'                    '        End If
'                    '    End If
'                    'Else
'                    '    Dim r = GetLastSingleChannel()
'                    '    If r <> -2 Then
'                    '        If r = Direction.Up Then
'                    '            BarColor(Min(CurrentSwing.StartBar, Chart.bars.Count), Min(CurrentSwing.EndBar, Chart.bars.Count - 1), UpTrendBarColor, UpTrendSwingLineColor, Direction.Up)
'                    '        Else
'                    '            BarColor(Min(CurrentSwing.StartBar, Chart.bars.Count), Min(CurrentSwing.EndBar, Chart.bars.Count - 1), DownTrendBarColor, DownTrendSwingLineColor, Direction.Down)
'                    '        End If
'                    '    End If
'                    'End If
'                End If
'            End If
'        End Sub
'        Private Function GetLastSingleChannel() As Integer
'            If swings.Count > 2 Then
'                If CurrentSwing(2).StartPrice > CurrentSwing(2).EndPrice And
'                    CurrentSwing(1).EndPrice < CurrentSwing(2).StartPrice And
'                    CurrentSwing(0).EndPrice <= CurrentSwing(1).StartPrice Then
'                    Return Direction.Down
'                ElseIf CurrentSwing(2).StartPrice < CurrentSwing(2).EndPrice And
'                    CurrentSwing(1).EndPrice > CurrentSwing(2).StartPrice And
'                    CurrentSwing(0).EndPrice >= CurrentSwing(1).StartPrice Then
'                    Return Direction.Up
'                End If
'            End If
'            Return -2
'        End Function
'        Private Function GetChannelSwingLength(ByVal channel As Channel) As Integer
'            Dim startIndex As Integer
'            For i = 0 To swings.Count - 1
'                If channel.APoint.X = swings(i).StartBar Then
'                    startIndex = i
'                ElseIf channel.CPoint.X = swings(i).StartBar Then
'                    Return i - startIndex + 1
'                End If
'            Next
'            Return 0
'        End Function
'        Private Function GetLastConfirmedChannel() As Channel
'            For i = channels.Count - 1 To 0 Step -1
'                If channels(i).IsConfirmed Then Return channels(i)
'            Next
'            Return Nothing
'        End Function
'        Private Function GetLastConfirmedChannelIndex() As Integer
'            For i = channels.Count - 1 To 0 Step -1
'                If channels(i).IsConfirmed Then Return i
'            Next
'            Return -1
'        End Function
'        'Protected Sub BarColor(channel As Channel, startBar As Integer, endBar As Integer, ByVal color As Color, ByVal swingColor As Color)
'        '    If ColorBars Then
'        '        If colorMergedChannels Then
'        '            Dim swingsToColor As New List(Of Swing)
'        '            For Each swing In swings
'        '                If swing.StartBar >= startBar And swing.EndBar <= endBar + 1 Then
'        '                    swingsToColor.Add(swing)
'        '                End If
'        '            Next
'        '            Dim pointToStartColoringBars As Integer = 0
'        '            For Each stepChannel In channels
'        '                If stepChannel.APoint.X > startBar And stepChannel.CPoint.X < endBar And stepChannel.IsConfirmed And stepChannel.Direction <> channel.Direction Then
'        '                    pointToStartColoringBars = Max(pointToStartColoringBars, stepChannel.CPoint.X)
'        '                End If
'        '            Next
'        '            If pointToStartColoringBars <> 0 Then
'        '                Dim i As Integer
'        '                Do While i < swingsToColor.Count
'        '                    If swingsToColor(i).StartBar < pointToStartColoringBars Then swingsToColor.RemoveAt(i) Else Exit Do
'        '                Loop
'        '                If swingsToColor.Count < 3 Then
'        '                    swingsToColor.Clear()
'        '                Else
'        '                    Dim met As Boolean
'        '                    For Each channel In channels
'        '                        If channel.IsCancelled = False And channel.APoint.X = swingsToColor(0).StartBar Then met = True
'        '                    Next
'        '                    If Not met Then swingsToColor.Clear()
'        '                End If
'        '            End If
'        '            For Each swing In swingsToColor
'        '                If CType(swing.TL.Pen.Brush, SolidColorBrush).Color = swingColor Or CType(swing.TL.Pen.Brush, SolidColorBrush).Color = NeutralSwingLineColor Then
'        '                    swing.TL.Pen.Brush = New SolidColorBrush(swingColor)
'        '                    For i = swing.StartBar To Min(swing.EndBar, Chart.bars.Count - 1)
'        '                        If CType(Chart.bars(i).Pen.Brush, SolidColorBrush).Color <> color Then
'        '                            Chart.bars(i).Pen.Brush = New SolidColorBrush(color)
'        '                            RefreshObject(Chart.bars(i))
'        '                        End If
'        '                    Next
'        '                End If
'        '            Next
'        '        Else
'        '            For Each swing In swings
'        '                If swing.StartBar >= startBar And swing.EndBar <= endBar + 1 Then
'        '                    swing.TL.Pen.Brush = New SolidColorBrush(swingColor)
'        '                End If
'        '            Next
'        '            For i = startBar To endBar
'        '                If CType(Chart.bars(i).Pen.Brush, SolidColorBrush).Color <> color Then
'        '                    Chart.bars(i).Pen.Brush = New SolidColorBrush(color)
'        '                    RefreshObject(Chart.bars(i))
'        '                End If
'        '            Next
'        '        End If
'        '    End If
'        'End Sub
'        Protected Sub BarColor(ByVal startBar As Integer, ByVal endBar As Integer, ByVal color As Color, swingColor As Color, direction As Swing.ChannelDirectionType, Optional onlyColorNeutral As Boolean = False, Optional neutralColor As Color = Nothing, Optional neutralBarColor As Color = Nothing)
'            Dim swingsToColor As New List(Of Swing)
'            'If ChannelMode = ChannelModeType.Swing Then
'            '    For Each swing In secondarySwings
'            '        If swing.StartBar >= startBar And swing.EndBar <= endBar + 1 Then
'            '            swing.TL.Pen.Brush = New SolidColorBrush(swingColor)
'            '        End If
'            '    Next
'            'End If

'            For Each swing In swings
'                If swing.StartBar >= startBar And swing.EndBar <= endBar + 1 Then
'                    If onlyColorNeutral Then
'                        If CType(swing.TL.Pen.Brush, SolidColorBrush).Color = neutralColor Then
'                            CType(swing.TL.Pen.Brush, SolidColorBrush).Color = swingColor
'                            swing.ChannelDirection = direction
'                        End If
'                    Else
'                        swing.TL.Pen.Brush = New SolidColorBrush(swingColor)
'                        swing.ChannelDirection = direction
'                    End If
'                End If
'            Next
'            If ColorBars Then
'                For i = startBar To endBar
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
'                If CType(Chart.bars(i).Pen.Brush, SolidColorBrush).Color <> color Then
'                    Chart.bars(i).Pen.Brush = New SolidColorBrush(color)
'                    RefreshObject(Chart.bars(i))
'                End If
'            Next
'        End Sub
'        Private Function GetProjectedLineBarSkip(priceDifference As Decimal) As Integer
'            Dim averageSwingAngle As Double = GetAverageSwingAngle()
'            Dim cs = CurrentSwing
'            If averageSwingAngle <> 0 Then
'                rvProNewLine.StartPoint = New Point(cs.EndBar, cs.EndPrice)
'                Dim newLineBarSkip As Integer
'                Dim extLineBarSkip As Integer
'                If Chart.Settings("IsSlaveChart").Value Then
'                    newLineBarSkip = 1
'                    extLineBarSkip = 1
'                Else
'                    newLineBarSkip = Abs(CurrentBar.Close - (cs.EndPrice - BooleanToInteger(cs.Direction) * priceDifference)) / Chart.Settings("RangeValue").Value * 2
'                    extLineBarSkip = Abs(CurrentBar.Close - cs.EndPrice) / Chart.Settings("RangeValue").Value * 2
'                End If
'                Return Max(Round(priceDifference / Tan(averageSwingAngle), 0), (CurrentBar.Number + newLineBarSkip) - cs.EndBar)
'            Else
'                Return 0
'            End If
'        End Function
'        Private Sub DrawProjectionLineAndRVText()
'            ' draw projection line
'            Dim cs = CurrentSwing
'            Dim rv = Me.RV
'            If SwingTextOption = SwingTextType.Length Then
'                Dim averageSwingAngle As Double = GetAverageSwingAngle()
'                If averageSwingAngle <> 0 Then
'                    rvProNewLine.StartPoint = New Point(cs.EndBar, cs.EndPrice)
'                    Dim endBar As Integer = cs.EndBar + GetProjectedLineBarSkip(rv)
'                    rvProNewLine.EndPoint = New Point(endBar, cs.EndPrice - BooleanToInteger(cs.Direction) * rv)
'                    rvText.Location = New Point(endBar, cs.EndPrice - BooleanToInteger(cs.Direction) * rv)
'                    averageSwingAngle = Atan(Abs(rvProNewLine.EndPoint.Y - rvProNewLine.StartPoint.Y) / Abs(rvProNewLine.EndPoint.X - rvProNewLine.StartPoint.X))
'                    If swings.Count > 1 Then
'                        If swings(swings.Count - 1).OverlapGapLine IsNot Nothing Then
'                            swings(swings.Count - 1).OverlapGapLine.EndPoint = New Point(endBar, swings(swings.Count - 1).OverlapGapLine.EndPoint.Y)
'                        End If
'                        If swings.Count > 2 Then
'                            If swings(swings.Count - 2).OverlapGapLine IsNot Nothing AndAlso (Not swings(swings.Count - 2).OverlapGapLineGapped And
'                                Not swings(swings.Count - 2).OverlapGapLineDestroyed And swings(swings.Count - 2).OverlapGapLineHit) Then
'                                swings(swings.Count - 2).OverlapGapLine.EndPoint = New Point(endBar, swings(swings.Count - 2).OverlapGapLine.EndPoint.Y)
'                            End If
'                        End If
'                    End If
'                    ' move target texts along
'                    For Each channel In channels
'                        If channel.TargetText IsNot Nothing Then
'                            Dim bcHeight As Decimal = Abs(channel.BPoint.Y - channel.CPoint.Y)
'                            Dim yPoint As Double = channel.DPoint.Y - BooleanToInteger(channel.Direction) * bcHeight
'                            endBar = cs.EndBar + GetProjectedLineBarSkip(bcHeight)
'                            channel.TargetText.Location = New Point(endBar, yPoint)
'                        End If
'                    Next
'                    currentSwingBCTargetText.Text = ""
'                    If confirmedSwingChannels.Count > 0 Then
'                        If confirmedSwingChannels(confirmedSwingChannels.Count - 1).Swing Is CurrentSwing Then
'                            If Not confirmedSwingChannels(confirmedSwingChannels.Count - 1).BCLengthMet Then
'                                Dim bcHeight As Decimal = Abs(confirmedSwingChannels(confirmedSwingChannels.Count - 1).BPoint.Y - confirmedSwingChannels(confirmedSwingChannels.Count - 1).CPoint.Y)
'                                Dim yPoint As Double = confirmedSwingChannels(confirmedSwingChannels.Count - 1).DPoint.Y - BooleanToInteger(confirmedSwingChannels(confirmedSwingChannels.Count - 1).Direction) * bcHeight
'                                endBar = cs.EndBar + GetProjectedLineBarSkip(bcHeight)
'                                currentSwingBCTargetText.Text = "- " & GetDollarValue(Abs(confirmedSwingChannels(confirmedSwingChannels.Count - 1).BPoint.Y - confirmedSwingChannels(confirmedSwingChannels.Count - 1).CPoint.Y))
'                                currentSwingBCTargetText.Location = New Point(endBar, yPoint)
'                            Else
'                                currentSwingBCTargetText.Text = ""
'                            End If
'                        End If
'                    End If
'                Else
'                    rvText.Location = New Point(CurrentBar.Number, cs.EndPrice - BooleanToInteger(cs.Direction) * rv)
'                    ' move target texts along
'                    For Each channel In channels
'                        If channel.TargetText IsNot Nothing Then
'                            channel.TargetText.Location = New Point(CurrentBar.Number + 1, channel.DPoint.Y + BooleanToInteger(channel.Direction) * (channel.BPoint.Y - channel.CPoint.Y))
'                        End If
'                    Next
'                    currentSwingBCTargetText.Text = ""
'                    If confirmedSwingChannels.Count > 0 Then
'                        If confirmedSwingChannels(confirmedSwingChannels.Count - 1).Swing Is CurrentSwing Then
'                            If Not confirmedSwingChannels(confirmedSwingChannels.Count - 1).BCLengthMet Then
'                                Dim bcHeight As Decimal = Abs(confirmedSwingChannels(confirmedSwingChannels.Count - 1).BPoint.Y - confirmedSwingChannels(confirmedSwingChannels.Count - 1).CPoint.Y)
'                                Dim yPoint As Double = confirmedSwingChannels(confirmedSwingChannels.Count - 1).DPoint.Y - BooleanToInteger(confirmedSwingChannels(confirmedSwingChannels.Count - 1).Direction) * bcHeight
'                                currentSwingBCTargetText.Text = "- " & GetDollarValue(Abs(confirmedSwingChannels(confirmedSwingChannels.Count - 1).BPoint.Y - confirmedSwingChannels(confirmedSwingChannels.Count - 1).CPoint.Y))
'                                currentSwingBCTargetText.Location = New Point(CurrentBar.Number, yPoint)
'                            Else
'                                currentSwingBCTargetText.Text = ""
'                            End If
'                        End If
'                    End If
'                End If
'                rvText.Text = "- " & FormatNumber(GetDollarValue(rv), DecimalPlaces)
'            Else
'                Dim rvBars As Integer = Ceiling(rv / Chart.Settings("RangeValue").Value) - 1
'                Dim endBar As Integer = Max(cs.EndBar + rvBars, CurrentBar.Number + rvBars)
'                rvProNewLine.StartPoint = New Point(cs.EndBar, cs.EndPrice)
'                rvProNewLine.EndPoint = New Point(endBar, cs.EndPrice - BooleanToInteger(cs.Direction) * rv)
'                rvText.Location = New Point(endBar, cs.EndPrice - BooleanToInteger(cs.Direction) * rv)
'                rvText.Text = "- " & FormatNumber(rvBars, DecimalPlaces)
'                ' move target texts along
'                For Each channel In channels
'                    If channel.TargetText IsNot Nothing Then
'                        Dim bcWidth As Decimal = Abs(channel.BPoint.X - channel.CPoint.X)
'                        endBar = channel.DPoint.X + bcWidth
'                        channel.TargetText.Location = New Point(endBar, channel.DPoint.Y - BooleanToInteger(channel.Direction) * (bcWidth + 1) * Chart.Settings("RangeValue").Value)
'                    End If
'                Next
'            End If
'        End Sub
'        Private Sub ProcessCurrentGapMark()
'            Dim swings As List(Of Swing)
'            Dim curSwingEvent As SwingEvent
'            curSwingEvent = Me.curSwingEvent
'            swings = Me.swings
'            If curSwingEvent = OriginalAutoTrend.SwingEvent.NewSwing Then
'                If swings.Count > 1 AndAlso currentGapMark.Swing Is swings(swings.Count - 2) Then currentGapMark.State = GapMarkerState.FollowsExtension
'                If swings.Count > 2 AndAlso currentGapMark.Swing Is swings(swings.Count - 3) Then currentGapMark.State = GapMarkerState.Inactive
'            End If
'            If swings.Count > 2 Then
'                Dim p As Double = CurrentBar.Close
'                If (swings(swings.Count - 1).Direction = Direction.Down And p < swings(swings.Count - 3).EndPrice - RV) Or
'                    (swings(swings.Count - 1).Direction = Direction.Up And p > swings(swings.Count - 3).EndPrice + RV) Then ' if gap is being created
'                    If currentGapMark.Swing IsNot swings(swings.Count - 1) Then ' if new gap
'                        'If currentGapMark.Line IsNot Nothing Then AddObjectToChart(currentGapMark.Line)
'                        currentGapMark.Swing = swings(swings.Count - 1)
'                        currentGapMark.Line = NewTrendLine(If(swings(swings.Count - 1).Direction = Direction.Down, DownGapMarkerColor, UpGapMarkerColor), New Point(0, 0), New Point(0, 0), ShowGapMarkers And ChannelMode <> ChannelModeType.UnmergedOverlap)
'                        currentGapMark.Line.Pen.Thickness = 5
'                        currentGapMark.State = GapMarkerState.FollowsRVPoint
'                    End If
'                    currentGapMark.Line.StartPoint = New Point(LinCalc(swings(swings.Count - 1).StartPrice, swings(swings.Count - 1).StartBar, swings(swings.Count - 1).EndPrice, swings(swings.Count - 1).EndBar, swings(swings.Count - 3).EndPrice), swings(swings.Count - 3).EndPrice)
'                End If
'                If currentGapMark.Swing Is swings(swings.Count - 2) Then
'                    If (currentGapMark.Swing.Direction = Direction.Up And p <= currentGapMark.Line.StartPoint.Y) Or
'                        (currentGapMark.Swing.Direction = Direction.Down And p >= currentGapMark.Line.StartPoint.Y) Then
'                        currentGapMark.Swing = Nothing
'                        RemoveObjectFromChart(currentGapMark.Line)
'                        currentGapMark.Line = Nothing
'                        currentGapMark.State = GapMarkerState.Inactive
'                    End If
'                End If
'                'If currentGapMark.State = GapMarkerState.FollowsRVPoint Then
'                '    currentGapMark.Line.EndPoint = New Point(LinCalc(currentGapMark.Swing.StartPrice, currentGapMark.Swing.StartBar, currentGapMark.Swing.EndPrice, currentGapMark.Swing.EndBar,
'                '                                                     currentGapMark.Swing.EndPrice - BooleanToInteger(currentGapMark.Swing.Direction) * RV), currentGapMark.Line.StartPoint.Y)
'                'ElseIf currentGapMark.State = GapMarkerState.FollowsExtension Then
'                '    currentGapMark.Line.EndPoint = New Point(LinCalc(currentGapMark.Swing.StartPrice, currentGapMark.Swing.StartBar, currentGapMark.Swing.EndPrice, currentGapMark.Swing.EndBar, swings(swings.Count - 1).EndPrice), currentGapMark.Line.StartPoint.Y)
'                'End If
'                If currentGapMark.State = GapMarkerState.FollowsRVPoint Then
'                    currentGapMark.Line.EndPoint = New Point(LinCalc(currentGapMark.Swing.StartPrice, currentGapMark.Swing.StartBar, currentGapMark.Swing.EndPrice, currentGapMark.Swing.EndBar,
'                                                                     currentGapMark.Swing.EndPrice - BooleanToInteger(currentGapMark.Swing.Direction) * RV), currentGapMark.Swing.EndPrice - BooleanToInteger(currentGapMark.Swing.Direction) * RV)
'                ElseIf currentGapMark.State = GapMarkerState.FollowsExtension Then
'                    currentGapMark.Line.EndPoint = New Point(LinCalc(currentGapMark.Swing.StartPrice, currentGapMark.Swing.StartBar, currentGapMark.Swing.EndPrice, currentGapMark.Swing.EndBar, swings(swings.Count - 1).EndPrice), swings(swings.Count - 1).EndPrice)
'                End If
'            End If
'        End Sub
'        Protected Sub ProcessSwingChannel(ByVal swing As Swing)
'            If Chart.bars.Count <= 1 Or swing.StartBar = 0 Then Exit Sub
'            If swings.Count > 1 Then
'                If swing.Direction = Direction.Down Then
'                    If Not swingChannel.IsActive Then
'                        Dim highBar As New Point
'                        Dim lowBar As New Point(0, Double.MaxValue)
'                        Dim bPoint As Point, cPoint As Point, rvMatched As Boolean
'                        Dim prevHighest As Double
'                        For i = swing.StartBar - 1 To CurrentBar.Number - 1
'                            If Bars(i).Low < lowBar.Y Then
'                                lowBar = New Point(i + 1, Bars(i).Low)
'                                highBar = New Point(0, 0)
'                            End If
'                            If Bars(i).High >= highBar.Y And Bars(i).Number <> lowBar.X Then highBar = New Point(i + 1, Bars(i).High)
'                            If highBar.Y - lowBar.Y >= prevHighest Then
'                                bPoint = lowBar
'                                cPoint = highBar
'                                prevHighest = highBar.Y - lowBar.Y
'                                rvMatched = True
'                            End If
'                        Next
'                        If rvMatched Then
'                            With swingChannel
'                                .IsActive = True
'                                .APoint = swing.TL.StartPoint
'                                .BPoint = bPoint
'                                .CPoint = cPoint
'                                RemoveObjectFromChart(.TL)
'                                .TL = NewTrendLine(If(.Direction = Direction.Down, SwingChannelPotentialDownColor, SwingChannelPotentialUpColor), swing.TL.StartPoint, cPoint)
'                                .TL.HasParallel = True
'                                .TL.ParallelDistance = .BPoint.Y - LinCalc(.APoint.X, .APoint.Y, .CPoint.X, .CPoint.Y, .BPoint.X)
'                                .TL.ExtendRight = True
'                                .BCLengthMet = False
'                            End With
'                            currentSwingPotentialBCTargetText.Text = GetDollarValue(Abs(swingChannel.BPoint.Y - swingChannel.CPoint.Y))
'                            currentSwingPotentialBCTargetText.Location = AddToX(swingChannel.CPoint, -1)
'                        End If
'                    End If
'                    If swingChannel.IsActive And Not swingChannel.IsConfirmed And swingChannel.CPoint.X <> 0 Then
'                        Dim lowBar As New Point(0, Double.MaxValue)
'                        Dim highBar As New Point
'                        For i = swingChannel.CPoint.X - 1 To CurrentBar.Number - 1
'                            If Bars(i).Low <= lowBar.Y Then lowBar = New Point(i + 1, Bars(i).Low)
'                            If Bars(i).High >= highBar.Y Then highBar = New Point(i + 1, Bars(i).High)
'                        Next
'                        If highBar.Y >= swingChannel.CPoint.Y Then
'                            With swingChannel
'                                .CPoint = highBar
'                                .TL.EndPoint = highBar
'                                .TL.ParallelDistance = .BPoint.Y - LinCalc(.APoint.X, .APoint.Y, .CPoint.X, .CPoint.Y, .BPoint.X)
'                            End With
'                        End If
'                        currentSwingPotentialBCTargetText.Text = GetDollarValue(Abs(swingChannel.BPoint.Y - swingChannel.CPoint.Y))
'                        currentSwingPotentialBCTargetText.Location = AddToX(swingChannel.CPoint, -1)
'                        swingChannel.TL.Pen.Thickness = 1
'                        If lowBar.Y <= swingChannel.BPoint.Y Then
'                            'swingChannel.IsActive = False
'                            swingChannel.TL.Pen.Thickness = 0
'                            currentSwingPotentialBCTargetText.Text = ""
'                            swingChannel.IsConfirmed = True
'                            swingChannel.TL.Pen = New Pen(New SolidColorBrush(SwingChannelConfirmedColor), 0)
'                            'swingChannel.BCSwingLine = NewTrendLine(NeutralSwingLineColor, swingChannel.BPoint, swingChannel.CPoint, True)
'                            If confirmedSwingChannels.Count > 0 AndAlso confirmedSwingChannels(confirmedSwingChannels.Count - 1).Swing = CurrentSwing Then
'                                RemoveObjectFromChart(confirmedSwingChannels(confirmedSwingChannels.Count - 1).TL)
'                                'RemoveObjectFromChart(confirmedSwingChannels(confirmedSwingChannels.Count - 1).BCSwingLine)
'                                confirmedSwingChannels.RemoveAt(confirmedSwingChannels.Count - 1)
'                            End If
'                            With confirmedSwingChannels
'                                RemoveObjectFromChart(currentSwingBCLengthText)
'                                currentSwingBCLengthText = NewLabel(GetDollarValue(Abs(swingChannel.BPoint.Y - swingChannel.CPoint.Y)), CType(swingChannel.TL.Pen.Brush, SolidColorBrush).Color, AddToX(swingChannel.CPoint, -1))
'								currentSwingBCLengthText.Font.FontSize = SwingChannelBCLengthTextSize
'								currentSwingBCLengthText.Font.FontWeight = SwingChannelBCLengthTextWeight
'								currentSwingBCLengthText.Font.Brush = New SolidColorBrush(SwingChannelBCLengthTextColor)
'                                currentSwingBCLengthText.HorizontalAlignment = LabelHorizontalAlignment.Right
'                                currentSwingBCLengthText.VerticalAlignment = LabelVerticalAlignment.Center
'                                confirmedSwingChannels.Add(New SwingChannelLine(NewTrendLine(If(TypeOf swingChannel.TL.Pen.Brush Is SolidColorBrush, CType(swingChannel.TL.Pen.Brush, SolidColorBrush).Color, Colors.Pink), swingChannel.TL.StartPoint, swingChannel.TL.EndPoint, True)))
'                                confirmedSwingChannels(.Count - 1).Direction = Direction.Down
'                                confirmedSwingChannels(.Count - 1).TL.HasParallel = True
'                                confirmedSwingChannels(.Count - 1).TL.ParallelDistance = swingChannel.TL.ParallelDistance
'                                confirmedSwingChannels(.Count - 1).APoint = swingChannel.APoint
'                                confirmedSwingChannels(.Count - 1).BPoint = swingChannel.BPoint
'                                confirmedSwingChannels(.Count - 1).CPoint = swingChannel.CPoint
'                                confirmedSwingChannels(.Count - 1).IsActive = swingChannel.IsActive
'                                confirmedSwingChannels(.Count - 1).IsConfirmed = swingChannel.IsConfirmed
'                                confirmedSwingChannels(.Count - 1).TL.ExtendRight = True
'                                confirmedSwingChannels(.Count - 1).Swing = CurrentSwing
'                                'confirmedSwingChannels(.Count - 1).BCSwingLine = NewTrendLine(If(TypeOf swingChannel.TL.Pen.Brush Is SolidColorBrush, CType(swingChannel.TL.Pen.Brush, SolidColorBrush).Color, Colors.Pink), swingChannel.BPoint, swingChannel.CPoint)
'                                CurrentSwing.SwingChannel = confirmedSwingChannels(.Count - 1)
'                            End With
'                        End If
'                    End If
'                    If swingChannel.IsConfirmed And swingChannel.IsActive = True Then
'                        Dim highBar As Point
'                        For i = swing.EndBar - 1 To CurrentBar.Number - 1
'                            If Bars(i).High > highBar.Y Then highBar = New Point(i + 1, Bars(i).High)
'                        Next
'                        If highBar.Y - swing.EndPrice >= Abs(swingChannel.CPoint.Y - swingChannel.BPoint.Y) Then
'                            confirmedSwingChannels(confirmedSwingChannels.Count - 1).BCLengthMet = True
'                            RemoveObjectFromChart(currentSwingBCLengthText)
'                            currentSwingBCTargetText.Text = ""
'                            'confirmedSwingChannels(confirmedSwingChannels.Count - 1).TL.EndPoint = New Point(CurrentBar.Number, LinCalc(confirmedSwingChannels(confirmedSwingChannels.Count - 1).TL.StartPoint.X, confirmedSwingChannels(confirmedSwingChannels.Count - 1).TL.StartPoint.Y, confirmedSwingChannels(confirmedSwingChannels.Count - 1).TL.EndPoint.X, confirmedSwingChannels(confirmedSwingChannels.Count - 1).TL.EndPoint.Y, CurrentBar.Number))
'                            With swingChannel
'                                'Log("bclengthmet")
'                                .BCLengthMet = True
'                                .IsConfirmed = False
'                                .TL.Pen = New Pen(New SolidColorBrush(If(.Direction = Direction.Down, SwingChannelPotentialDownColor, SwingChannelPotentialUpColor)), 1)
'                                .CPoint = highBar
'                                .BPoint = New Point(swing.EndBar, swing.EndPrice)
'                                .TL.EndPoint = highBar
'                                .TL.ParallelDistance = .BPoint.Y - LinCalc(.APoint.X, .APoint.Y, .CPoint.X, .CPoint.Y, .BPoint.X)
'                                'If confirmedSwingChannel IsNot Nothing Then
'                                '    confirmedSwingChannel.TL.Pen.DashStyle = TrendLineDashStyle
'                                '    confirmedSwingChannel.BCLengthMet = True
'                                '    If LinCalc(confirmedSwingChannel.TL.StartPoint.X, confirmedSwingChannel.TL.StartPoint.Y, confirmedSwingChannel.TL.EndPoint.X, confirmedSwingChannel.TL.EndPoint.Y, CurrentBar.Number - 1) < CurrentBar.High Then
'                                '        confirmedSwingChannel.IsCut = True
'                                '    End If
'                                'End If
'                            End With
'                            currentSwingPotentialBCTargetText.Text = GetDollarValue(Abs(swingChannel.BPoint.Y - swingChannel.CPoint.Y))
'                            currentSwingPotentialBCTargetText.Location = AddToX(swingChannel.CPoint, -1)
'                        End If
'                    End If
'                Else
'                    ' up
'                    If Not swingChannel.IsActive Then
'                        Dim highBar As New Point
'                        Dim lowBar As New Point(0, Double.MaxValue)
'                        Dim bPoint As Point, cPoint As Point, rvMatched As Boolean
'                        Dim prevHighest As Double
'                        For i = swing.StartBar - 1 To CurrentBar.Number - 1
'                            If Bars(i).High > highBar.Y Then
'                                highBar = New Point(i + 1, Bars(i).High)
'                                lowBar = New Point(0, Double.MaxValue)
'                            End If
'                            If Bars(i).Low <= lowBar.Y And Bars(i).Number <> highBar.X Then lowBar = New Point(i + 1, Bars(i).Low)
'                            If highBar.Y - lowBar.Y >= prevHighest Then
'                                bPoint = highBar
'                                cPoint = lowBar

'                                prevHighest = highBar.Y - lowBar.Y
'                                rvMatched = True
'                            End If
'                        Next
'                        If rvMatched Then
'                            With swingChannel
'                                .IsActive = True
'                                .APoint = swing.TL.StartPoint
'                                .BPoint = bPoint
'                                .CPoint = cPoint
'                                RemoveObjectFromChart(.TL)
'                                .TL = NewTrendLine(If(.Direction = Direction.Down, SwingChannelPotentialDownColor, SwingChannelPotentialUpColor), swing.TL.StartPoint, cPoint)
'                                .TL.HasParallel = True
'                                .TL.ParallelDistance = .BPoint.Y - LinCalc(.APoint.X, .APoint.Y, .CPoint.X, .CPoint.Y, .BPoint.X)
'                                .TL.ExtendRight = True
'                                .BCLengthMet = False
'                            End With
'                            currentSwingPotentialBCTargetText.Text = GetDollarValue(Abs(swingChannel.BPoint.Y - swingChannel.CPoint.Y))
'                            currentSwingPotentialBCTargetText.Location = AddToX(swingChannel.CPoint, -1)
'                        End If
'                    End If
'                    If swingChannel.IsActive And Not swingChannel.IsConfirmed Then
'                        Dim lowBar As New Point(0, Double.MaxValue)
'                        Dim highBar As New Point
'                        For i = swingChannel.CPoint.X - 1 To CurrentBar.Number - 1
'                            If Bars(i).Low <= lowBar.Y Then lowBar = New Point(i + 1, Bars(i).Low)
'                            If Bars(i).High >= highBar.Y Then highBar = New Point(i + 1, Bars(i).High)
'                        Next
'                        If lowBar.Y <= swingChannel.CPoint.Y Then
'                            With swingChannel
'                                .CPoint = lowBar
'                                .TL.EndPoint = lowBar
'                                .TL.ParallelDistance = .BPoint.Y - LinCalc(.APoint.X, .APoint.Y, .CPoint.X, .CPoint.Y, .BPoint.X)
'                            End With
'                        End If
'                        currentSwingPotentialBCTargetText.Text = GetDollarValue(Abs(swingChannel.BPoint.Y - swingChannel.CPoint.Y))
'                        currentSwingPotentialBCTargetText.Location = AddToX(swingChannel.CPoint, -1)
'                        swingChannel.TL.Pen.Thickness = 1
'                        If highBar.Y >= swingChannel.BPoint.Y Then
'                            'swingChannel.IsActive = False
'                            swingChannel.TL.Pen.Thickness = 0
'                            currentSwingPotentialBCTargetText.Text = ""
'                            swingChannel.IsConfirmed = True
'                            swingChannel.TL.Pen = New Pen(New SolidColorBrush(SwingChannelConfirmedColor), 0)
'                            'swingChannel.BCSwingLine = NewTrendLine(NeutralSwingLineColor, swingChannel.BPoint, swingChannel.CPoint, True)
'                            If confirmedSwingChannels.Count > 0 AndAlso confirmedSwingChannels(confirmedSwingChannels.Count - 1).Swing = CurrentSwing Then
'                                RemoveObjectFromChart(confirmedSwingChannels(confirmedSwingChannels.Count - 1).TL)
'                                'RemoveObjectFromChart(confirmedSwingChannels(confirmedSwingChannels.Count - 1).BCSwingLine)
'                                confirmedSwingChannels.RemoveAt(confirmedSwingChannels.Count - 1)
'                            End If
'                            With confirmedSwingChannels
'                                RemoveObjectFromChart(currentSwingBCLengthText)
'                                currentSwingBCLengthText = NewLabel(GetDollarValue(Abs(swingChannel.BPoint.Y - swingChannel.CPoint.Y)), CType(swingChannel.TL.Pen.Brush, SolidColorBrush).Color, AddToX(swingChannel.CPoint, -1))
'                                currentSwingBCLengthText.Font.FontSize = SwingChannelBCLengthTextSize
'                                currentSwingBCLengthText.Font.FontWeight = SwingChannelBCLengthTextWeight
'                                currentSwingBCLengthText.Font.Brush = New SolidColorBrush(SwingChannelBCLengthTextColor)
'                                currentSwingBCLengthText.HorizontalAlignment = LabelHorizontalAlignment.Right
'                                currentSwingBCLengthText.VerticalAlignment = LabelVerticalAlignment.Center
'                                confirmedSwingChannels.Add(New SwingChannelLine(NewTrendLine(If(TypeOf swingChannel.TL.Pen.Brush Is SolidColorBrush, CType(swingChannel.TL.Pen.Brush, SolidColorBrush).Color, Colors.Pink), swingChannel.TL.StartPoint, swingChannel.TL.EndPoint, True)))
'                                confirmedSwingChannels(.Count - 1).Direction = Direction.Up
'                                confirmedSwingChannels(.Count - 1).TL.HasParallel = True
'                                confirmedSwingChannels(.Count - 1).TL.ParallelDistance = swingChannel.TL.ParallelDistance
'                                confirmedSwingChannels(.Count - 1).APoint = swingChannel.APoint
'                                confirmedSwingChannels(.Count - 1).BPoint = swingChannel.BPoint
'                                confirmedSwingChannels(.Count - 1).CPoint = swingChannel.CPoint
'                                confirmedSwingChannels(.Count - 1).IsActive = swingChannel.IsActive
'                                confirmedSwingChannels(.Count - 1).IsConfirmed = swingChannel.IsConfirmed
'                                confirmedSwingChannels(.Count - 1).TL.ExtendRight = True
'                                confirmedSwingChannels(.Count - 1).Swing = CurrentSwing
'                                'confirmedSwingChannels(.Count - 1).BCSwingLine = NewTrendLine(If(TypeOf swingChannel.TL.Pen.Brush Is SolidColorBrush, CType(swingChannel.TL.Pen.Brush, SolidColorBrush).Color, Colors.Pink), swingChannel.BPoint, swingChannel.CPoint)
'                                CurrentSwing.SwingChannel = confirmedSwingChannels(.Count - 1)
'                            End With
'                        End If
'                    End If
'                    If swingChannel.IsConfirmed And swingChannel.IsActive = True Then
'                        Dim lowBar As New Point(0, Double.MaxValue)
'                        For i = swing.EndBar - 1 To CurrentBar.Number - 1
'                            If Bars(i).Low < lowBar.Y Then lowBar = New Point(i + 1, Bars(i).Low)
'                        Next
'                        If swing.EndPrice - lowBar.Y >= Abs(swingChannel.CPoint.Y - swingChannel.BPoint.Y) Then
'                            confirmedSwingChannels(confirmedSwingChannels.Count - 1).BCLengthMet = True
'                            RemoveObjectFromChart(currentSwingBCLengthText)
'                            currentSwingBCTargetText.Text = ""
'                            'confirmedSwingChannels(confirmedSwingChannels.Count - 1).TL.EndPoint = New Point(CurrentBar.Number, LinCalc(confirmedSwingChannels(confirmedSwingChannels.Count - 1).TL.StartPoint.X, confirmedSwingChannels(confirmedSwingChannels.Count - 1).TL.StartPoint.Y, confirmedSwingChannels(confirmedSwingChannels.Count - 1).TL.EndPoint.X, confirmedSwingChannels(confirmedSwingChannels.Count - 1).TL.EndPoint.Y, CurrentBar.Number))
'                            With swingChannel
'                                .TL.Pen = New Pen(New SolidColorBrush(If(.Direction = Direction.Down, SwingChannelPotentialDownColor, SwingChannelPotentialUpColor)), 1)
'                                .IsConfirmed = False
'                                .CPoint = lowBar
'                                .BPoint = New Point(swing.EndBar, swing.EndPrice)
'                                .TL.EndPoint = lowBar
'                                .TL.ParallelDistance = .BPoint.Y - LinCalc(.APoint.X, .APoint.Y, .CPoint.X, .CPoint.Y, .BPoint.X)
'                            End With
'                            currentSwingPotentialBCTargetText.Text = GetDollarValue(Abs(swingChannel.BPoint.Y - swingChannel.CPoint.Y))
'                            currentSwingPotentialBCTargetText.Location = AddToX(swingChannel.CPoint, -1)
'                        End If
'                        'If confirmedSwingChannel IsNot Nothing Then
'                        '    If swing.EndPrice - lowBar.Y >= Abs(confirmedSwingChannel.BPoint.Y - confirmedSwingChannel.CPoint.Y) Then
'                        '        confirmedSwingChannel.TL.Pen.DashStyle = TrendLineDashStyle
'                        '        confirmedSwingChannel.BCLengthMet = True
'                        '        If LinCalc(confirmedSwingChannel.TL.StartPoint.X, confirmedSwingChannel.TL.StartPoint.Y, confirmedSwingChannel.TL.EndPoint.X, confirmedSwingChannel.TL.EndPoint.Y, CurrentBar.Number - 1) > CurrentBar.Low Then
'                        '            confirmedSwingChannel.IsCut = True
'                        '        End If
'                        '    End If
'                        'End If
'                    End If
'                End If
'                If swings.Count > 2 Then
'                    If swings(swings.Count - 3).SwingChannel IsNot Nothing Then
'                        If swings(swings.Count - 3).SwingChannel.IsCut = False Then
'                            swings(swings.Count - 3).SwingChannel.TL.EndPoint = New Point(CurrentSwing.StartBar, LinCalc(swings(swings.Count - 3).SwingChannel.TL.StartPoint.X, swings(swings.Count - 3).SwingChannel.TL.StartPoint.Y, swings(swings.Count - 3).SwingChannel.TL.EndPoint.X, swings(swings.Count - 3).SwingChannel.TL.EndPoint.Y, CurrentSwing.StartBar))
'                            swings(swings.Count - 3).SwingChannel.IsCut = True
'                            If swings(swings.Count - 3).SwingChannel.BCLengthMet Then
'                                swings(swings.Count - 3).SwingChannel.TL.Pen.Brush = New SolidColorBrush(SwingChannelConfirmedGappedColor)
'                                'If swings(swings.Count - 3).SwingChannel.BCSwingLine IsNot Nothing Then swings(swings.Count - 3).SwingChannel.BCSwingLine.Pen.Brush = New SolidColorBrush(SwingChannelConfirmedGappedColor)
'                            Else
'                                swings(swings.Count - 3).SwingChannel.TL.Pen.Brush = New SolidColorBrush(HistorySwingChannelColor)
'                                'If swings(swings.Count - 3).SwingChannel.BCSwingLine IsNot Nothing Then swings(swings.Count - 3).SwingChannel.BCSwingLine.Pen.Brush = New SolidColorBrush(HistorySwingChannelColor)
'                            End If
'                            swings(swings.Count - 3).SwingChannel.TL.ExtendRight = False
'                        End If
'                    End If
'                End If
'                If swings.Count > 0 AndAlso swings(swings.Count - 1).SwingChannel IsNot Nothing Then
'                    If swings(swings.Count - 1).SwingChannel.Swing IsNot CurrentSwing Then
'                        RemoveObjectFromChart(currentSwingBCLengthText)
'                        currentSwingPotentialBCTargetText.Text = ""
'                    End If
'                    If swings(swings.Count - 1).SwingChannel.IsCut = False And
'                            ((swings(swings.Count - 1).SwingChannel.Direction = Direction.Down And CurrentBar.High >= LinCalc(swings(swings.Count - 1).SwingChannel.TL.StartPoint.X, swings(swings.Count - 1).SwingChannel.TL.StartPoint.Y, swings(swings.Count - 1).SwingChannel.TL.EndPoint.X, swings(swings.Count - 1).SwingChannel.TL.EndPoint.Y, CurrentBar.Number)) Or
'                            (swings(swings.Count - 1).SwingChannel.Direction = Direction.Up And CurrentBar.Low <= LinCalc(swings(swings.Count - 1).SwingChannel.TL.StartPoint.X, swings(swings.Count - 1).SwingChannel.TL.StartPoint.Y, swings(swings.Count - 1).SwingChannel.TL.EndPoint.X, swings(swings.Count - 1).SwingChannel.TL.EndPoint.Y, CurrentBar.Number))) Then
'                        If ((swings(swings.Count - 1).SwingChannel.Direction = Direction.Down And CurrentSwing.EndPrice >= LinCalc(swings(swings.Count - 1).SwingChannel.TL.StartPoint.X, swings(swings.Count - 1).SwingChannel.TL.StartPoint.Y, swings(swings.Count - 1).SwingChannel.TL.EndPoint.X, swings(swings.Count - 1).SwingChannel.TL.EndPoint.Y, CurrentBar.Number)) Or
'                            (swings(swings.Count - 1).SwingChannel.Direction = Direction.Up And CurrentSwing.EndPrice <= LinCalc(swings(swings.Count - 1).SwingChannel.TL.StartPoint.X, swings(swings.Count - 1).SwingChannel.TL.StartPoint.Y, swings(swings.Count - 1).SwingChannel.TL.EndPoint.X, swings(swings.Count - 1).SwingChannel.TL.EndPoint.Y, CurrentBar.Number))) Then
'                            swings(swings.Count - 1).SwingChannel.TL.EndPoint = New Point(Min(CurrentBar.Number, CurrentSwing.EndBar), LinCalc(swings(swings.Count - 1).SwingChannel.TL.StartPoint.X, swings(swings.Count - 1).SwingChannel.TL.StartPoint.Y, swings(swings.Count - 1).SwingChannel.TL.EndPoint.X, swings(swings.Count - 1).SwingChannel.TL.EndPoint.Y, Min(CurrentBar.Number, CurrentSwing.EndBar)))
'                        Else
'                            swings(swings.Count - 1).SwingChannel.TL.EndPoint = New Point(CurrentBar.Number, LinCalc(swings(swings.Count - 1).SwingChannel.TL.StartPoint.X, swings(swings.Count - 1).SwingChannel.TL.StartPoint.Y, swings(swings.Count - 1).SwingChannel.TL.EndPoint.X, swings(swings.Count - 1).SwingChannel.TL.EndPoint.Y, CurrentBar.Number))
'                        End If
'                        swings(swings.Count - 1).SwingChannel.IsCut = True
'                    End If
'                    If ((swings(swings.Count - 1).SwingChannel.Direction = Direction.Down And (CurrentBar.Low <= swings(swings.Count - 1).SwingChannel.DPoint.Y Or swings(swings.Count - 1).SwingChannel.DPoint.Y = 0)) Or
'                        (swings(swings.Count - 1).SwingChannel.Direction = Direction.Up And CurrentBar.High >= swings(swings.Count - 1).SwingChannel.DPoint.Y)) Then
'                        swings(swings.Count - 1).SwingChannel.DPoint = New Point(CurrentBar.Number, If(swings(swings.Count - 1).SwingChannel.Direction = Direction.Down, CurrentBar.Low, CurrentBar.High))
'                        swings(swings.Count - 1).SwingChannel.IsCut = False
'                    End If
'                    ''If swings(swings.Count - 1).SwingChannel.IsCut Then
'                    ''    swings(swings.Count - 1).SwingChannel.TL.Pen.DashStyle = TrendLineDashStyle
'                    ''End If
'                    If swings(swings.Count - 1).SwingChannel.BCLengthMet Then
'                        If Not swings(swings.Count - 1).SwingChannel.IsCut Then
'                            'swings(swings.Count - 1).SwingChannel.TL.EndPoint = New Point(swingChannel.CPoint.X, LinCalc(swings(swings.Count - 1).SwingChannel.TL.StartPoint.X, swings(swings.Count - 1).SwingChannel.TL.StartPoint.Y, swings(swings.Count - 1).SwingChannel.TL.EndPoint.X, swings(swings.Count - 1).SwingChannel.TL.EndPoint.Y, swingChannel.CPoint.X))
'                            'swings(swings.Count - 1).SwingChannel.TL.Pen.Brush = New SolidColorBrush(HistorySwingChannelColor)
'                            'If swings(swings.Count - 1).SwingChannel.BCSwingLine IsNot Nothing Then swings(swings.Count - 1).SwingChannel.BCSwingLine.Pen.Brush = New SolidColorBrush(HistorySwingChannelColor)
'                            'If swings(swings.Count - 1).SwingChannel.BCLengthMet Then swings(swings.Count - 1).SwingChannel.TL.Pen.DashStyle = TrendLineDashStyle
'                        Else
'                            swings(swings.Count - 1).SwingChannel.TL.Pen.DashStyle = TrendLineDashStyle
'                            swings(swings.Count - 1).SwingChannel.TL.ExtendRight = False
'                        End If
'                    End If
'                    If swings(swings.Count - 1).SwingChannel.BCLengthMet Or swings(swings.Count - 1).SwingChannel.IsCut Then
'                        'RemoveObjectFromChart(swings(swings.Count - 1).SwingChannel.BCSwingLine)
'                    End If

'                    If (swings(swings.Count - 1).SwingChannel.Direction = Direction.Down And CurrentBar.Low < LinCalc(swings(swings.Count - 1).SwingChannel.TL.StartPoint.X,
'                                                                                                                      swings(swings.Count - 1).SwingChannel.TL.StartPoint.Y,
'                                                                                                                      swings(swings.Count - 1).SwingChannel.TL.EndPoint.X,
'                                                                                                                      swings(swings.Count - 1).SwingChannel.TL.EndPoint.Y,
'                                                                                                                      CurrentBar.Number + GetProjectedLineBarSkip(RV)) - RV) Or
'                        (swings(swings.Count - 1).SwingChannel.Direction = Direction.Up And CurrentBar.High > LinCalc(swings(swings.Count - 1).SwingChannel.TL.StartPoint.X,
'                                                                                                                      swings(swings.Count - 1).SwingChannel.TL.StartPoint.Y,
'                                                                                                                      swings(swings.Count - 1).SwingChannel.TL.EndPoint.X,
'                                                                                                                      swings(swings.Count - 1).SwingChannel.TL.EndPoint.Y,
'                                                                                                                      CurrentBar.Number + GetProjectedLineBarSkip(RV)) + RV) Then
'                        swings(swings.Count - 1).SwingChannel.TL.Pen.Brush = New SolidColorBrush(SwingChannelPotentialGappedColor)
'                        'If swings(swings.Count - 1).SwingChannel.BCSwingLine IsNot Nothing Then swings(swings.Count - 1).SwingChannel.BCSwingLine.Pen.Brush = New SolidColorBrush(SwingChannelPotentialGappedColor)
'                    End If
'                End If
'                If swings.Count > 1 Then
'                    If swings(swings.Count - 2).SwingChannel IsNot Nothing Then
'                        If Not ShowSwingChannelHistoryLines Then
'                            RemoveObjectFromChart(swings(swings.Count - 2).SwingChannel.TL)
'                            'RemoveObjectFromChart(swings(swings.Count - 2).SwingChannel.BCSwingLine)
'                        End If
'                        'swings(swings.Count - 2).SwingChannel.TL.Pen.Brush = New SolidColorBrush(HistorySwingChannelColor)
'                        'If swings(swings.Count - 2).SwingChannel.BCSwingLine IsNot Nothing Then swings(swings.Count - 2).SwingChannel.BCSwingLine.Pen.Brush = New SolidColorBrush(HistorySwingChannelColor)
'                        If swings(swings.Count - 2).SwingChannel.BCLengthMet = False Then
'                            swings(swings.Count - 2).SwingChannel.TL.ExtendRight = True
'                            If swings(swings.Count - 2).SwingChannel.IsCut Then
'                                '    swings(swings.Count - 2).SwingChannel.TL.EndPoint = New Point(CurrentSwing.EndBar, LinCalc(swings(swings.Count - 2).SwingChannel.TL.StartPoint.X, swings(swings.Count - 2).SwingChannel.TL.StartPoint.Y, swings(swings.Count - 2).SwingChannel.TL.EndPoint.X, swings(swings.Count - 2).SwingChannel.TL.EndPoint.Y, CurrentSwing.EndBar))
'                                '    If Not ShowSwingChannelHistoryGapLines Then
'                                '        RemoveObjectFromChart(swings(swings.Count - 2).SwingChannel.TL)
'                                '    Else
'                                '        AddObjectToChart(swings(swings.Count - 2).SwingChannel.TL)
'                                '    End If
'                                'Else
'                                swings(swings.Count - 2).SwingChannel.TL.Pen.DashStyle = TrendLineDashStyle
'                                swings(swings.Count - 2).SwingChannel.TL.Pen.Brush = New SolidColorBrush(HistorySwingChannelColor)
'                                'If swings(swings.Count - 2).SwingChannel.BCSwingLine IsNot Nothing Then swings(swings.Count - 2).SwingChannel.BCSwingLine.Pen.Brush = New SolidColorBrush(HistorySwingChannelColor)
'                                If Not ShowSwingChannelHistoryGapLines Then
'                                    AddObjectToChart(swings(swings.Count - 2).SwingChannel.TL)
'                                End If
'                            End If
'                            swings(swings.Count - 2).SwingChannel.BCLengthMet = True
'                        End If
'                        If swings(swings.Count - 2).SwingChannel.IsCut = False And
'                            ((swings(swings.Count - 2).SwingChannel.Direction = Direction.Down And CurrentBar.High >= LinCalc(swings(swings.Count - 2).SwingChannel.TL.StartPoint.X, swings(swings.Count - 2).SwingChannel.TL.StartPoint.Y, swings(swings.Count - 2).SwingChannel.TL.EndPoint.X, swings(swings.Count - 2).SwingChannel.TL.EndPoint.Y, CurrentBar.Number)) Or
'                            (swings(swings.Count - 2).SwingChannel.Direction = Direction.Up And CurrentBar.Low <= LinCalc(swings(swings.Count - 2).SwingChannel.TL.StartPoint.X, swings(swings.Count - 2).SwingChannel.TL.StartPoint.Y, swings(swings.Count - 2).SwingChannel.TL.EndPoint.X, swings(swings.Count - 2).SwingChannel.TL.EndPoint.Y, CurrentBar.Number))) Then
'                            swings(swings.Count - 2).SwingChannel.TL.EndPoint = New Point(CurrentBar.Number, LinCalc(swings(swings.Count - 2).SwingChannel.TL.StartPoint.X, swings(swings.Count - 2).SwingChannel.TL.StartPoint.Y, swings(swings.Count - 2).SwingChannel.TL.EndPoint.X, swings(swings.Count - 2).SwingChannel.TL.EndPoint.Y, CurrentBar.Number))
'                            swings(swings.Count - 2).SwingChannel.IsCut = True
'                            swings(swings.Count - 2).SwingChannel.TL.Pen.DashStyle = TrendLineDashStyle
'                            If Not ShowSwingChannelHistoryGapLines Then
'                                AddObjectToChart(swings(swings.Count - 2).SwingChannel.TL)
'                            End If
'                        End If
'                        If swings(swings.Count - 2).SwingChannel.IsCut Then
'                            swings(swings.Count - 2).SwingChannel.TL.ExtendRight = False
'                            swings(swings.Count - 2).SwingChannel.TL.Pen.DashStyle = TrendLineDashStyle
'                            swings(swings.Count - 2).SwingChannel.TL.Pen.Brush = New SolidColorBrush(HistorySwingChannelColor)
'                            'If swings(swings.Count - 2).SwingChannel.BCSwingLine IsNot Nothing Then swings(swings.Count - 2).SwingChannel.BCSwingLine.Pen.Brush = New SolidColorBrush(HistorySwingChannelColor)
'                            If Not ShowSwingChannelHistoryGapLines Then
'                                AddObjectToChart(swings(swings.Count - 2).SwingChannel.TL)
'                            End If
'                        End If
'                        If swings(swings.Count - 2).SwingChannel.BCLengthMet And swings(swings.Count - 2).SwingChannel.IsCut = False Then
'                            swings(swings.Count - 2).SwingChannel.TL.EndPoint = New Point(CurrentSwing.EndBar, LinCalc(swings(swings.Count - 2).SwingChannel.TL.StartPoint.X, swings(swings.Count - 2).SwingChannel.TL.StartPoint.Y, swings(swings.Count - 2).SwingChannel.TL.EndPoint.X, swings(swings.Count - 2).SwingChannel.TL.EndPoint.Y, CurrentSwing.EndBar))
'                            swings(swings.Count - 2).SwingChannel.TL.Pen.Brush = New SolidColorBrush(SwingChannelPotentialGappedColor)
'                            If Not ShowSwingChannelHistoryGapLines Then
'                                RemoveObjectFromChart(swings(swings.Count - 2).SwingChannel.TL)
'                            Else
'                                AddObjectToChart(swings(swings.Count - 2).SwingChannel.TL)
'                            End If
'                        End If
'                        If swings(swings.Count - 2).SwingChannel.BCLengthMet Or swings(swings.Count - 2).SwingChannel.IsCut Then
'                            'RemoveObjectFromChart(swings(swings.Count - 2).SwingChannel.BCSwingLine)
'                        End If
'                    End If
'                End If
'                If swings.Count > 2 Then
'                    If swings(swings.Count - 3).SwingChannel IsNot Nothing Then
'                        If swings(swings.Count - 3).SwingChannel.BCLengthMet = True And swings(swings.Count - 3).SwingChannel.IsCut = False Then
'                            swings(swings.Count - 3).SwingChannel.TL.Pen.Brush = New SolidColorBrush(SwingChannelConfirmedGappedColor)
'                            'If swings(swings.Count - 3).SwingChannel.BCSwingLine IsNot Nothing Then swings(swings.Count - 3).SwingChannel.BCSwingLine.Pen.Brush = New SolidColorBrush(SwingChannelConfirmedGappedColor)
'                            swings(swings.Count - 3).SwingChannel.TL.ExtendRight = False
'                        End If
'                    End If
'                End If
'                For i = 0 To confirmedSwingChannels.Count - 1
'                    'If confirmedSwingChannels(i).Swing IsNot CurrentSwing And confirmedSwingChannels(i).BCLengthMet = False Then
'                    '    confirmedSwingChannels(i).BCLengthMet = True
'                    'End If
'                    'If confirmedSwingChannels(i).IsCut = False And
'                    '    ((confirmedSwingChannels(i).Direction = Direction.Down And CurrentBar.High >= LinCalc(confirmedSwingChannels(i).TL.StartPoint.X, confirmedSwingChannels(i).TL.StartPoint.Y, confirmedSwingChannels(i).TL.EndPoint.X, confirmedSwingChannels(i).TL.EndPoint.Y, CurrentBar.Number)) Or
'                    '    (confirmedSwingChannels(i).Direction = Direction.Up And CurrentBar.Low <= LinCalc(confirmedSwingChannels(i).TL.StartPoint.X, confirmedSwingChannels(i).TL.StartPoint.Y, confirmedSwingChannels(i).TL.EndPoint.X, confirmedSwingChannels(i).TL.EndPoint.Y, CurrentBar.Number))) Then
'                    '    confirmedSwingChannels(i).TL.EndPoint = New Point(CurrentBar.Number, LinCalc(confirmedSwingChannels(i).TL.StartPoint.X, confirmedSwingChannels(i).TL.StartPoint.Y, confirmedSwingChannels(i).TL.EndPoint.X, confirmedSwingChannels(i).TL.EndPoint.Y, CurrentBar.Number))
'                    '    confirmedSwingChannels(i).IsCut = True
'                    'End If
'                    'If confirmedSwingChannels(i).BCLengthMet And confirmedSwingChannels(i).IsCut Then confirmedSwingChannels(i).TL.Pen.DashStyle = TrendLineDashStyle
'                    'If confirmedSwingChannels(i).IsCut And confirmedSwingChannels(i).BCLengthMet And confirmedSwingChannels(i).Swing IsNot CurrentSwing Then
'                    '    If confirmedSwingChannels(i).TL.EndPoint.X >= confirmedSwingChannels(i).Swing.EndBar Then
'                    '        confirmedSwingChannels(i).TL.ExtendRight = False
'                    '    Else
'                    '        confirmedSwingChannels(i).IsCut = False
'                    '    End If
'                    'End If
'                Next
'            End If
'        End Sub
'        Protected Sub RemoveSwingChannel()
'            'If swingChannel.IsConfirmed = False Then
'            RemoveObjectFromChart(swingChannel.TL)
'            'RemoveObjectFromChart(swingChannel.BCSwingLine)
'            'Else
'            'If confirmedSwingChannel IsNot Nothing Then
'            '    With confirmedSwingChannel
'            '        If ShowSwingChannelHistoryLines Then
'            '            AddObjectToChart(.TL)
'            '        Else
'            '            RemoveObjectFromChart(.TL)
'            '        End If
'            '        '.TL.ExtendRight = False
'            '        .TL.Pen = New Pen(New SolidColorBrush(HistorySwingChannelColor), 1)
'            '        .TL.Pen.DashStyle = TrendLineDashStyle
'            '        If .IsCut Then
'            '            .TL.EndPoint = New Point(CurrentSwing.StartBar, LinCalc(.TL.StartPoint.X, .TL.StartPoint.Y, .TL.EndPoint.X, .TL.EndPoint.Y, CurrentSwing.StartBar))
'            '        Else
'            '            .TL.EndPoint = New Point(CurrentBar.Number, LinCalc(.TL.StartPoint.X, .TL.StartPoint.Y, .TL.EndPoint.X, .TL.EndPoint.Y, CurrentBar.Number))
'            '        End If
'            '    End With
'            'End If
'            'swingChannel.TL.ExtendRight = False
'            'swingChannel.TL.EndPoint = New Point(CurrentSwing.EndBar, LinCalc(swingChannel.TL.StartPoint.X, swingChannel.TL.StartPoint.Y, swingChannel.TL.EndPoint.X, swingChannel.TL.EndPoint.Y, CurrentSwing.EndBar))
'            'End If
'            swingChannel.IsConfirmed = False
'            swingChannel.IsActive = False
'            'confirmedSwingChannel = Nothing
'        End Sub

'        Private Sub CutChannel(channel As Channel)
'            channel.IsCut = True
'            If channel.IsGapped Then Exit Sub
'            If (channel.IsConfirmed And ConfirmedHistoryChannelLinesVisible) Or (channel.IsConfirmed = False And PotentialHistoryChannelLinesVisible) Then
'                If channel.Hidden = False And channel.DontDraw = False Then
'                    AddObjectToChart(channel.TL)

'                End If
'            Else
'                RemoveObjectFromChart(channel.TL)
'                If channel.IsConfirmed = False Then
'                    RemoveObjectFromChart(channel.BCLengthText)

'                End If
'            End If

'            If channel.IsConfirmed = False Then
'                'ShowNextBackedUpChannel(channel.Direction, False)
'                If channel.Swing.StartBar = swings(swings.Count - 1).StartBar Then
'                    RemoveObjectFromChart(channel.TL)
'                    channels.Remove(channel)
'                Else
'                    channel.TL.EndPoint = New Point(channel.CutBarNumber, LinCalc(channel.TL.StartPoint.X, channel.TL.StartPoint.Y, channel.TL.EndPoint.X, channel.TL.EndPoint.Y, channel.CutBarNumber))
'                    channel.TL.Pen = New Pen(New SolidColorBrush(If(channel.Direction = Direction.Down, PotentialHistoryDownChannelLineColor, PotentialHistoryUpChannelLineColor)), 1)
'                    channel.TL.ExtendRight = False
'                End If
'            Else
'                'If HighlightLastCutChannel And channel.IsGapped = False Then
'                '    If currentPartiallyCutChannel IsNot Nothing Then
'                '        If currentPartiallyCutChannel.Channel.APoint.X < channel.APoint.X Then
'                '            FullyCutChannel(currentPartiallyCutChannel.Channel)
'                '            channel.TL.Pen = New Pen(New SolidColorBrush(If(channel.Direction = Direction.Down, ConfirmedDownChannelLineColor, ConfirmedUpChannelLineColor)), 1)
'                '            currentPartiallyCutChannel = New PartiallyCutChannel(channel, swings.Count - 1)
'                '        Else
'                '            FullyCutChannel(channel)
'                '        End If
'                '    Else
'                '        channel.TL.Pen = New Pen(New SolidColorBrush(If(channel.Direction = Direction.Down, ConfirmedDownChannelLineColor, ConfirmedUpChannelLineColor)), 1)
'                '        currentPartiallyCutChannel = New PartiallyCutChannel(channel, swings.Count - 1)
'                '    End If
'                'Else
'                FullyCutChannel(channel)
'                'End If
'            End If
'            channel.TL.Pen.DashStyle = TrendLineDashStyle

'            If HistoryChannelLineParallelsVisible = False Or channel.IsConfirmed = False Then channel.TL.HasParallel = False

'            If RV < HistoryBCLengthVisibilityRVCutoff * Chart.Settings("RangeValue").Value Then RemoveObjectFromChart(channel.BCLengthText)
'            RemoveObjectFromChart(channel.TargetText)
'            If channel.IsGapped Then
'                channel.TL.HasParallel = True
'            End If
'            channel.BCLengthText = Nothing
'            channel.TargetText = Nothing
'        End Sub
'        Private Sub FullyCutChannel(channel As Channel)
'            If Not channel.Hidden And channel.IsBackedUp = False And channel.IsGapped = False And channel.DontDraw = False Then
'                ShowNextBackedUpChannel(channel.Direction, True)
'            End If
'            channel.TL.Pen = New Pen(New SolidColorBrush(If(channel.Direction = Direction.Down, ConfirmedHistoryDownChannelLineColor, ConfirmedHistoryUpChannelLineColor)), 1)
'            channel.TL.Pen.DashStyle = TrendLineDashStyle
'            If channel.IsConfirmed = False Then RemoveObjectFromChart(channel.BCLengthText)
'            channel.TL.EndPoint = New Point(channel.CutBarNumber, LinCalc(channel.TL.StartPoint.X, channel.TL.StartPoint.Y, channel.TL.EndPoint.X, channel.TL.EndPoint.Y, channel.CutBarNumber))
'            channel.TL.ExtendRight = False
'        End Sub

'        Private Sub ShowNextBackedUpChannel(direction As Direction, isConfirmed As Boolean)
'            'Exit Sub
'            If (isConfirmed And ShowOnlyLastConfirmedChannels) Or (isConfirmed = False And ShowOnlyLastPotentialChannels) Then
'                If isConfirmed = True Then
'                    Dim g As New Integer
'                End If
'                'For Each channel In channels
'                '    If channel.Direction = direction And channel.IsBackedUp = False And channel.IsCut = False And channel.IsCancelled = False And channel.BCSwingMatched = False And channel.IsConfirmed = isConfirmed And channel.IsGapped = False Then
'                '        Exit Sub
'                '    End If
'                'Next
'                Dim c As Channel = Nothing
'                For i = channels.Count - 1 To 0 Step -1
'                    Dim backup = channels(i)
'                    If backup.Direction = direction And backup.IsBackedUp And backup.IsCut = False And backup.IsCancelled = False And backup.IsConfirmed = isConfirmed And backup.IsGapped = False And backup.GappedSwing Is Nothing Then
'                        c = channels(i)
'                        Exit For
'                    End If
'                Next
'                If c IsNot Nothing Then
'                    If isConfirmed Then
'                        AddObjectToChart(c.TL)
'                        c.TL.Pen = New Pen(New SolidColorBrush(If(c.Direction = OriginalAutoTrend.Direction.Down, ConfirmedDownChannelLineColor, ConfirmedUpChannelLineColor)), 1)
'                    Else
'                        If c.Hidden = False And c.DontDraw = False Then
'                            AddObjectToChart(c.TL)
'                        End If
'                    End If
'                    c.IsBackedUp = False
'                    'If c.IsConfirmed Then c.TL.Pen.Thickness = 1.5
'                End If
'            End If
'        End Sub
'        Private Sub BackupChannel(channelToShow As Channel)
'            If (channelToShow.IsConfirmed And ShowOnlyLastConfirmedChannels) Or (channelToShow.IsConfirmed = False And ShowOnlyLastPotentialChannels) Then
'                If channelToShow.IsConfirmed = True Then
'                    Dim g As New Integer
'                End If
'                'Dim firstChannelLocation As Integer
'                For Each otherChannel In channels
'                    If otherChannel.IsConfirmed = channelToShow.IsConfirmed And otherChannel.IsCut = False And otherChannel.IsCancelled = False And otherChannel.IsGapped = False And otherChannel.GappedSwing Is Nothing And otherChannel.Direction = channelToShow.Direction And
'                        otherChannel IsNot channelToShow And otherChannel.IsBackedUp = False Then ' AndAlso otherChannel.CPoint.X > channel.CPoint.X AndAlso otherChannel.CPoint.X >= firstChannelLocation
'                        channelToShow = otherChannel
'                        'firstChannelLocation = otherChannel.CPoint.X
'                    End If
'                Next
'                For Each otherChannel In channels
'                    If otherChannel.IsConfirmed = channelToShow.IsConfirmed And otherChannel.IsCut = False And otherChannel.IsCancelled = False And otherChannel.IsGapped = False And otherChannel.GappedSwing Is Nothing And otherChannel.Direction = channelToShow.Direction And
'                        otherChannel IsNot channelToShow And otherChannel.IsBackedUp = False Then
'                        otherChannel.IsBackedUp = True
'                        If otherChannel.IsConfirmed Then otherChannel.TL.Pen.Thickness = 1
'                        If otherChannel.IsConfirmed Then
'                            RemoveObjectFromChart(otherChannel.TL)
'                            'otherChannel.TL.Pen = New Pen(New SolidColorBrush(If(otherChannel.Direction = Direction.Down, BackedUpConfirmedDownChannelLineColor, BackedUpConfirmedUpChannelLineColor)), 1)
'                            'otherChannel.BCSwingLine.Pen = New Pen(New SolidColorBrush(If(otherChannel.Direction = Direction.Down, BackedUpConfirmedDownChannelLineColor, BackedUpConfirmedUpChannelLineColor)), 1)
'                            'RemoveObjectFromChart(otherChannel.ABSwingLine)
'                            'RemoveObjectFromChart(otherChannel.BCSwingLine)
'                            'RemoveObjectFromChart(otherChannel.CDSwingLine)
'                            'otherChannel.ABSwingLine.Pen = New Pen(New SolidColorBrush(If(otherChannel.Direction = Direction.Down, BackedUpConfirmedDownChannelLineColor, BackedUpConfirmedUpChannelLineColor)), 1)
'                            'otherChannel.CDSwingLine.Pen = New Pen(New SolidColorBrush(If(otherChannel.Direction = Direction.Down, BackedUpConfirmedDownChannelLineColor, BackedUpConfirmedUpChannelLineColor)), 1)
'                            'RemoveObjectFromChart(otherChannel.TL)

'                            'RemoveObjectFromChart(otherChannel.BCLengthText)
'                            'RemoveObjectFromChart(otherChannel.TargetText)
'                        Else
'                            RemoveObjectFromChart(otherChannel.TL)
'                        End If
'                    End If
'                Next
'            End If
'        End Sub

'        Private Function GetAverageSwingAngle() As Double
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
'                If Chart.IB.Multiplier(Chart.TickerID) = "" OrElse Not IsNumeric(Chart.IB.Multiplier(Chart.TickerID)) Then
'                    multiplier = 1
'                Else
'                    multiplier = Chart.IB.Multiplier(Chart.TickerID)
'                End If
'            End If
'            Dim shares As Integer = Chart.Settings("DefaultOrderQuantity").Value
'            'Dim a = priceDif / Chart.GetMinTick * Chart.GetMinTick * multiplier * shares
'            Return Round(priceDif * multiplier * shares, 2)
'        End Function
'        Friend Overrides Function GetRVFromDollar(ByVal dollarDif As Decimal) As Decimal
'            Dim multiplier As Double
'            If Chart.Settings("UseRandom").Value Then
'                multiplier = 1
'            Else
'                If Chart.IB.Multiplier(Chart.TickerID) = "" OrElse Not IsNumeric(Chart.IB.Multiplier(Chart.TickerID)) Then
'                    multiplier = 1
'                Else
'                    multiplier = Chart.IB.Multiplier(Chart.TickerID)
'                End If
'            End If
'            Dim shares As Integer = Chart.Settings("DefaultOrderQuantity").Value
'            'Dim a = priceDif / Chart.GetMinTick * Chart.GetMinTick * multiplier * shares
'            Return Round(dollarDif / (multiplier * shares), 6)
'        End Function
'        Private Function NewSwing(ByVal color As Color, ByVal startPoint As Point, ByVal endPoint As Point, ByVal show As Boolean, ByVal direction As Direction) As Swing
'            Return New Swing(NewTrendLine(color, startPoint, endPoint, show), direction)
'        End Function
'    End Class

'End Namespace
