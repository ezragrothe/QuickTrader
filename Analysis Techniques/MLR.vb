Imports System.Runtime.InteropServices
Imports System.Math
Imports System.Windows.Markup
Imports System.ComponentModel
Imports System.Windows.Threading
Imports System.IO
Imports System.Threading
Imports System.Collections.ObjectModel
Imports System.Reflection
Imports QuickTrader.AnalysisTechniques
Imports System.Windows.Controls.Primitives
Imports System.Text
Imports System.Collections.Specialized
Namespace AnalysisTechniques
    Public Class MLR
        Inherits AnalysisTechnique
        Implements IChartPadAnalysisTechnique
        Public Sub New(ByVal chart As Chart)
            MyBase.New(chart)
            Description = "Zig Zag."
        End Sub
        Dim mergedTrend As LineCoordinates
        Dim lastTrends() As Swing
        Dim trendHitCountText As Label
        Dim abcChannel As Swing
        Dim swings As List(Of Swing)
        Dim pointList As List(Of Point)
        Dim lastSwingDirection As Direction
        Private Property lastSwing As Swing
            Get
                If swings.Count = 0 Then
                    Dim s = New Swing(NewTrendLine(Colors.Gray, New Point(1, CurrentBar.Close), New Point(1, CurrentBar.Close)), Direction.Up)

                    s.TL.Pen.Thickness = SwingLineThickness
                    swings.Add(s)
                End If
                Return swings(swings.Count - 1)
            End Get
            Set(value As Swing)
                If swings.Count = 0 Then
                    swings.Add(Nothing)
                End If
                swings(swings.Count - 1) = value
            End Set
        End Property
        Dim currentSwingBCLengths As List(Of Label)
        Dim confirmedSwingChannel As TrendLine
        Dim prevSwing As LineCoordinates
        Dim potentialSwingChannel As TrendLine
        Dim swingTargetText As Label
        Dim swingExtendText As Label
        Dim swingPoints As List(Of Point)
        Dim swingevnt As Boolean
        Dim reversalSwingMove As Boolean
        'Dim swingDir As Boolean
        'Dim targetTextLine As Label
        'Dim targetText As Label
        'Dim extendStartTargetText As Label
        'Dim extendEndTargetText As Label
        'Dim secondLastPotSwing As TrendLine
        'Dim lastPotSwing As TrendLine
        'Dim regressionLine As TrendLine
        'Dim potentialRegressionLine As TrendLine
        'Dim lengthText As Label

        'range box
        Private boxes As List(Of Box)
        Private upperMaxRangeLine As TrendLine
        Private verticalRangeLine As TrendLine
        Private lowerMinRangeLine As TrendLine
        Private upperActualRangeLine As TrendLine
        Private lowerActualRangeLine As TrendLine
        Private trackBox As Box
        Private potentialBox As Box
        Private potentialFillBox1 As Rectangle
        Private potentialFillBox2 As Rectangle
        Private potentialFillBox3 As Rectangle

        Private Class Box
            Public Property Rect As Rectangle
            Public Property MovementDirection As Boolean
            Public Property Direction As Boolean
        End Class

        Private ReadOnly Property CurrentBox As Box
            Get
                Return boxes(boxes.Count - 1)
            End Get
        End Property

        Public Property ChartPadVisible As Boolean = True Implements IChartPadAnalysisTechnique.ChartPadVisible
        Public Property ChartPadLocation As Point Implements IChartPadAnalysisTechnique.ChartPadLocation
            Get
                Return New Point(ChartPad.Left, ChartPad.Top)
            End Get
            Set(value As Point)
                ChartPad.Left = value.X
                ChartPad.Top = value.Y
            End Set
        End Property
        Public Property ChartPad As UIChartControl Implements IChartPadAnalysisTechnique.ChartPad

        <Input> Public Property StepUpCount As Integer = 2
        <Input> Public Property StepUpStartBar As Integer = 0
        <Input> Public Property HiLiteIndex As Integer = 2
        <Input> Public Property AutoRVStepUp As Boolean = True
        <Input> Public Property BaseTrendRV As Double = 2
        <Input> Public Property TrendRVMultiplier As Double = 2
        <Input> Public Property SwingRV As Double = 2
        <Input> Public Property SecondSwingRV As Double = 2
        <Input> Public Property SwingRVMultiplier As Double = 2
        <Input> Public Property SecondSwingRVMultiplier As Double = 2
        <Input> Public Property SwingBCMultiplier As Double = 2
        <Input> Public Property SecondSwingBCMultiplier As Double = 2

        <Input> Property SwingColor As Color = Colors.Gray
        <Input> Property BarCountTextColor As Color = Colors.LightBlue

        <Input> Property ConfirmedTrendUpColor As Color = Colors.Green
        <Input> Property ConfirmedTrendDownColor As Color = Colors.Red
        <Input> Property PotentialTrendUpColor As Color = Colors.SaddleBrown
        <Input> Property PotentialTrendDownColor As Color = Colors.Blue
        <Input> Property NeutralBarColor As Color = Colors.Gray
        <Input> Property IsUpSwingColor As Color = Colors.LightBlue
        <Input> Property IsDownSwingColor As Color = Colors.Black
        <Input> Property OsUpSwingColor As Color = Colors.Lime
        <Input> Property OsDownSwingColor As Color = Colors.Red
        <Input> Property HitCountTextColor As Color = Colors.Blue
        <Input> Property IsOsSwingThickness As Decimal = 3
        <Input> Property AboveFillColor As Color = Colors.Red
        <Input> Property BelowFillColor As Color = Colors.Red
        Property AbcBarColoring As Boolean = False
        Property OsIsBarColoring As Boolean = True
        <Input> Property GapColor As Color = Colors.Red
        <Input> Property GapLineThickness As Decimal = 1
        <Input> Property SecondGapLineThickness As Decimal = 1
        <Input> Property GapLinesOnSwing As Boolean = True
        <Input> Property SecondGapLinesOnSwing As Boolean = True
        <Input> Property PushCountFontSize As Decimal = 11
        <Input> Property SecondPushCountFontSize As Decimal = 11
        <Input> Property PushCountFontWeight As FontWeight = FontWeights.Bold
        '<Input(, "Range Box")> Property RangeBoxOn As Boolean = True
        '<Input> Property NumberOfBoxesToUseAsAverage As Integer = 5
        '<Input> Property ProjectionLinesColor As Color = Colors.Blue
        '<Input> Property RangeBoxUpColor As Color = Color.FromArgb(20, 0, 255, 0)
        '<Input> Property RangeBoxDownColor As Color = Color.FromArgb(20, 255, 0, 0)
        '<Input> Property RangeBoxLineThickness As Decimal = 0.2
        '<Input> Property ProjectionLineThickness As Decimal = 0.2
        <Input(, "Swings")> Property SwingLineThickness As Decimal = 1
        <Input> Property SecondSwingLineThickness As Decimal = 1
        <Input> Property LastSwingLineThickness As Decimal = 1
        <Input> Property SecondLastSwingLineThickness As Decimal = 1
        <Input> Property ConfirmedSwingRegressionLineThickness As Decimal = 1
        <Input> Property ConfirmedSwingChannelLineThickness As Decimal = 1
        <Input> Property PotentialSwingRegressionLineThickness As Decimal = 1
        <Input> Property PotentialSwingChannelLineThickness As Decimal = 1
        <Input> Property SwingLengthTextFontSize As Decimal = 11
        <Input> Property SecondSwingLengthTextFontSize As Decimal = 11
        <Input> Property SwingLengthTextFontWeight As FontWeight = FontWeights.Bold
        <Input> Property SwingTargetTextFontSize As Decimal = 11
        <Input> Property SecondSwingTargetTextFontSize As Decimal = 11
        <Input> Property SwingTargetTextFontWeight As FontWeight = FontWeights.Bold
        <Input> Property SwingTargetTextLineLength As Integer = 12
        <Input> Property SwingChannelsOn As Boolean = True
        <Input> Property LastSwingChannelOn As Boolean = True
        <Input> Property SwingPresetPattern1 As String = "(?<![ud])[ud]{2}"
        <Input> Property SwingPresetPattern2 As String = "(?<![if])[if]{2}"
        <Input> Property SwingPresetPattern3 As String = "(?<![ud])[ud]{3}"
        <Input> Property SwingPresetPattern4 As String = "(?<![if])[if]{3}"
        <Input> Property SwingPresetPattern5 As String = "(?<![ud])[ud]{2,}"
        <Input> Property SwingPresetPattern6 As String = "(?<![if])[if]{2,}"
        <Input> Property CurrentPresetPattern As Integer = 1

        '<Input> Property HistorySwingChannelThickness As Decimal = 1
        Property TrendLineThickness As Decimal = 0
        Property LastTrendLineThickness As Decimal = 0
        <Input(, "Trends")> Property ConfirmedTrendRegressionLineThicknessHLite As Decimal = 1
        <Input> Property ConfirmedTrendChannelLineThicknessHLite As Decimal = 1
        <Input> Property PotentialTrendRegressionLineThicknessHLite As Decimal = 1
        <Input> Property PotentialTrendChannelLineThicknessHLite As Decimal = 1
        <Input(, "-")> Property ConfirmedTrendRegressionLineThickness As Decimal = 1
        <Input> Property ConfirmedTrendChannelLineThickness As Decimal = 1
        <Input> Property PotentialTrendRegressionLineThickness As Decimal = 1
        <Input> Property PotentialTrendChannelLineThickness As Decimal = 1
        <Input(, "-")> Property BeginningTrendChannelBuffer As Integer = 0
        <Input> Property TrendLengthTextFontSize As Decimal = 11
        <Input> Property TrendLengthTextFontWeight As FontWeight = FontWeights.Bold
        <Input> Property StepUpFontSizeIncrement As Decimal = 1.5
        Property BCTextSpacing As Decimal = 0
        <Input> Property TrendTargetTextFontSize As Decimal = 11
        <Input> Property TrendTargetTextFontWeight As FontWeight = FontWeights.Bold
        <Input> Property RVTargetTextLineLength As Integer = 6
        <Input> Property SecondRVTargetTextLineLength As Integer = 6
        <Input> Property ExtendTargetTextLineLength As Integer = 6
        <Input> Property SecondExtendTargetTextLineLength As Integer = 6
        <Input> Property TrendChannelsOn As Boolean = True
        <Input> Property AbcChannelMode As Boolean = True
        Property TextSpacing As Decimal = 0
        '<Input> Property SwingHistoryOn As Boolean = False
        '<Input> Property BarColoringOff As Boolean = False
        Property LastRegressionFillToggle As Boolean = True
        Property RegressionMode As Boolean = False
        '<Input> Property ExtendLastSwing As Boolean = True
        Private Function GetPreset(index As Integer) As String
            Select Case index
                Case 1
                    Return SwingPresetPattern1
                Case 2
                    Return SwingPresetPattern2
                Case 3
                    Return SwingPresetPattern3
                Case 4
                    Return SwingPresetPattern4
                Case 5
                    Return SwingPresetPattern5
                Case 6
                    Return SwingPresetPattern6
            End Select
            Return SwingPresetPattern1
        End Function

        Protected Overrides Sub Begin()
            MyBase.Begin()

            ReDim lastTrends(StepUpCount)

            For i = 0 To StepUpCount


                lastTrends(i) = New Swing(NewTrendLine(Colors.Gray, New Point(CurrentBar.Number, CurrentBar.Close), New Point(CurrentBar.Number, CurrentBar.Close), False), False)
                lastTrends(i).LengthText = CreateTrendText(New Point(0, 0), Direction.Neutral, 0, Colors.Gray)
                lastTrends(i).BCBarCountText = CreateBarCountText(New Point(0, 0), Direction.Up, "", BarCountTextColor)
                lastTrends(i).LengthBarCountText = CreateBarCountText(New Point(0, 0), Direction.Up, "", BarCountTextColor)
                lastTrends(i).NeutralMaxPoint = lastTrends(i).EndPoint
                lastTrends(i).NeutralMinPoint = lastTrends(i).EndPoint
                lastTrends(i).PotentialTL = CreateFillTrendLine(Colors.Gray, If(i = HiLiteIndex, AboveFillColor, Colors.Transparent), If(i = HiLiteIndex, BelowFillColor, Colors.Transparent))
                lastTrends(i).PotentialTL.HasParallel = True
                lastTrends(i).PotentialTL.ExtendRight = True
                lastTrends(i).Direction = Direction.Neutral
                lastTrends(i).TargetText = NewLabel("── " & FormatNumberLengthAndPrefix(BaseTrendRV) & " RV ", SwingColor, New Point(0, 0), True, , False)
                lastTrends(i).TargetText.Font.FontSize = TrendTargetTextFontSize
                lastTrends(i).TargetText.Font.FontWeight = TrendTargetTextFontWeight
                lastTrends(i).ExtendTargetText = NewLabel(Strings.StrDup(ExtendTargetTextLineLength, "─") & " Extend", SwingColor, New Point(0, 0), True, , False)
                lastTrends(i).ExtendTargetText.Font.FontSize = TrendTargetTextFontSize
                lastTrends(i).ExtendTargetText.Font.FontWeight = TrendTargetTextFontWeight

                lastTrends(i).RegressionTL = CreateFillTrendLine(SwingColor, Colors.Transparent, Colors.Transparent)
                lastTrends(i).RegressionTL.ExtendRight = True
                lastTrends(i).RegressionTL.OuterPen.Thickness = If(i = HiLiteIndex, ConfirmedTrendChannelLineThicknessHLite, ConfirmedTrendChannelLineThickness)
                lastTrends(i).RegressionTL.Pen.Thickness = If(i = HiLiteIndex, ConfirmedTrendRegressionLineThicknessHLite, ConfirmedTrendRegressionLineThickness)
                'lastTrends(i).RegressionTLOuterLines = NewTrendLine(Colors.Gray)
                'If i = HiLiteIndex Then
                '    lastTrends(i).RegressionTL.Pen.Thickness = ConfirmedTrendRegressionLineThicknessHLite
                '    lastTrends(i).RegressionTL.OuterPen.Thickness = ConfirmedTrendChannelLineThicknessHLite
                'Else
                '    lastTrends(i).RegressionTL.Pen.Thickness = ConfirmedTrendRegressionLineThickness
                '    lastTrends(i).RegressionTL.OuterPen.Thickness = ConfirmedTrendChannelLineThickness
                'End If
                'lastTrends(i).RegressionTLOuterLines.ExtendRight = True
                'lastTrends(i).RegressionTLOuterLines.HasParallel = True
                'lastTrends(i).RegressionTLOuterLines.Pen.Thickness = 0
                'lastTrends(i).RegressionTLOuterLines.OuterPen.Thickness = ChannelLineThickness

                lastTrends(i).BCLine = NewTrendLine(SwingColor)
                lastTrends(i).BCText = NewLabel("", SwingColor, New Point(0, 0), True, New Font With {.FontSize = TrendLengthTextFontSize, .FontWeight = TrendLengthTextFontWeight}, False)
                lastTrends(i).BCText.HorizontalAlignment = LabelHorizontalAlignment.Right : lastTrends(i).BCText.IsEditable = True : lastTrends(i).BCText.IsSelectable = True
                AddHandler lastTrends(i).BCText.MouseDown, Sub(sender As Object, location As Point)
                                                               BaseTrendRV = Round(CDec(CType(sender, Label).Tag) + Chart.GetMinTick, 5)
                                                               Chart.ReApplyAnalysisTechnique(Me)
                                                           End Sub
                AddHandler lastTrends(i).BCText.MiddleMouseDown, Sub(sender As Object, location As Point)
                                                                     BaseTrendRV = Round(CDec(CType(sender, Label).Tag), 5)
                                                                     Chart.ReApplyAnalysisTechnique(Me)
                                                                 End Sub


                'lastSwings(i).LengthText = CreateText(AddToY(lastSwings(i).EndPoint, Chart.GetRelativeFromRealHeight(TextSpacing)), lastSwings(i).Direction, Abs(lastSwings(i).EndPrice - lastSwings(i).StartPrice), If(lastSwings(i).Direction, SwingUpColor, SwingDownColor))
                'lastSwings(i).LengthText.Text = ""
            Next

            currentSwingBCLengths = New List(Of Label)
            currentSwingBCLengths.Add(Nothing)
            confirmedSwingChannel = NewTrendLine(SwingColor)
            confirmedSwingChannel.HasParallel = True
            confirmedSwingChannel.IsRegressionLine = True
            confirmedSwingChannel.UseFixedStartRegressionFormula = RegressionMode
            confirmedSwingChannel.Pen.Thickness = 0
            confirmedSwingChannel.OuterPen.Thickness = 0


            confirmedSwingChannel.ExtendRight = True
            swingTargetText = NewLabel("", SwingColor, New Point(0, 0))
            swingTargetText.Font.FontSize = SwingTargetTextFontSize
            swingTargetText.Font.FontWeight = SwingTargetTextFontWeight
            swingTargetText.IsSelectable = False
            swingTargetText.IsEditable = False
            swingExtendText = NewLabel("", SwingColor, New Point(0, 0))
            swingExtendText.Font.FontSize = SwingTargetTextFontSize
            swingExtendText.Font.FontWeight = SwingTargetTextFontWeight
            swingExtendText.IsSelectable = False
            swingExtendText.IsEditable = False
            swingExtendText.Text = Strings.StrDup(SwingTargetTextLineLength, "─") & " Extend"
            potentialSwingChannel = NewTrendLine(SwingColor)
            potentialSwingChannel.IsRegressionLine = True
            potentialSwingChannel.UseFixedStartRegressionFormula = RegressionMode
            potentialSwingChannel.OuterPen = New Pen(New SolidColorBrush(SwingColor), PotentialSwingChannelLineThickness)
            potentialSwingChannel.Pen = New Pen(New SolidColorBrush(SwingColor), PotentialSwingRegressionLineThickness)
            'lastSwingChannel.Pen.DashStyle = GetDashStyle(PotentialSwingRegressionLineThickness)
            potentialSwingChannel.OuterPen.DashStyle = GetDashStyle(PotentialSwingChannelLineThickness)
            potentialSwingChannel.Pen.DashStyle = GetDashStyle(PotentialSwingRegressionLineThickness)
            potentialSwingChannel.HasParallel = True
            potentialSwingChannel.ExtendRight = True
            abcChannel = New Swing(NewTrendLine(Colors.Gray, New Point(1, CurrentBar.Close), New Point(1, CurrentBar.Close)), Direction.Up)
            abcChannel.PotentialTL = CreateFillTrendLine(Colors.Gray, AboveFillColor, BelowFillColor)
            abcChannel.PotentialTL.ExtendRight = True
            abcChannel.PotentialTL.HasParallel = True
            abcChannel.Direction = Direction.Neutral
            abcChannel.TargetText = NewLabel("── " & FormatNumberLengthAndPrefix(BaseTrendRV) & " RV ", SwingColor, New Point(0, 0), True, , False)
            abcChannel.TargetText.Font.FontSize = TrendTargetTextFontSize
            abcChannel.TargetText.Font.FontWeight = TrendTargetTextFontWeight
            abcChannel.ExtendTargetText = NewLabel(Strings.StrDup(ExtendTargetTextLineLength, "─") & " Extend", SwingColor, New Point(0, 0), True, , False)
            abcChannel.ExtendTargetText.Font.FontSize = TrendTargetTextFontSize
            abcChannel.ExtendTargetText.Font.FontWeight = TrendTargetTextFontWeight


            'range box
            'boxes = New List(Of Box)
            'boxes.Add(New Box With {.Rect = NewRectangle(SwingColor, RangeBoxUpColor, New Point(1, CurrentBar.High), New Point(1, CurrentBar.Low), RangeBoxOn)})
            'CurrentBox.Rect.Pen.Thickness = RangeBoxLineThickness
            'trackBox = New Box With {.Rect = NewRectangle(Colors.Transparent, Colors.Red, New Point(1, CurrentBar.High), New Point(1, CurrentBar.Low), RangeBoxOn)}
            'trackBox.Rect.IsEditable = False
            'trackBox.Rect.IsSelectable = False
            'potentialBox = New Box With {.Rect = NewRectangle(SwingColor, Color.FromArgb(0, 0, 0, 0), New Point(1, CurrentBar.High), New Point(1, CurrentBar.Low), RangeBoxOn)}
            'potentialBox.Rect.LockPositionOrientation = ChartDrawingVisual.LockPositionOrientationTypes.Vertically
            'potentialBox.Rect.CanResize = False
            'potentialBox.Rect.ShowSelectionHandles = False
            'potentialBox.Rect.Pen.Thickness = RangeBoxLineThickness
            'potentialBox.Rect.BorderThickness = New Thickness(0, 0, 0, 0) 'New Thickness(1, 1, 0, 1)
            'potentialFillBox1 = NewRectangle(Colors.Transparent, Colors.Red, New Point(0, 0), New Point(0, 0), RangeBoxOn) : potentialFillBox1.IsEditable = False : potentialFillBox1.IsSelectable = False
            'potentialFillBox2 = NewRectangle(Colors.Transparent, Colors.Red, New Point(0, 0), New Point(0, 0), RangeBoxOn) : potentialFillBox2.IsEditable = False : potentialFillBox2.IsSelectable = False
            'potentialFillBox3 = NewRectangle(Colors.Transparent, Colors.Red, New Point(0, 0), New Point(0, 0), RangeBoxOn) : potentialFillBox3.IsEditable = False : potentialFillBox3.IsSelectable = False

            'CurrentBox.Rect.IsEditable = False
            'CurrentBox.Rect.IsSelectable = False
            'upperMaxRangeLine = NewTrendLine(ProjectionLinesColor, RangeBoxOn) : upperMaxRangeLine.Pen.Thickness = ProjectionLineThickness
            'verticalRangeLine = NewTrendLine(ProjectionLinesColor, RangeBoxOn) : verticalRangeLine.Pen.Thickness = ProjectionLineThickness
            'lowerMinRangeLine = NewTrendLine(ProjectionLinesColor, RangeBoxOn) : lowerMinRangeLine.Pen.Thickness = ProjectionLineThickness
            'upperMaxRangeLine.ExtendRight = True
            'lowerMinRangeLine.ExtendRight = True
            'upperActualRangeLine = NewTrendLine(SwingColor, RangeBoxOn) : upperActualRangeLine.Pen.Thickness = RangeBoxLineThickness
            'lowerActualRangeLine = NewTrendLine(SwingColor, RangeBoxOn) : lowerActualRangeLine.Pen.Thickness = RangeBoxLineThickness

            'AddHandler potentialBox.Rect.ManuallyMoved, AddressOf UpdateProjectionBoxColors

            gapLines = New Dictionary(Of Point, TrendLine)
            mergedTrend = New LineCoordinates(0, 0, 0, 0)
            swings = New List(Of Swing)
            pointList = New List(Of Point)
            pointList.Add(New Point(1, CurrentBar.Close))
            pointList.Add(New Point(1, CurrentBar.Close))
            lastSwingDirection = Direction.Up
        End Sub
        Private Function CreateParallelTrendLine(color As Color, outerColor As Color, thickness As Double, outerThickness As Double, extendRight As Boolean, point1 As Point, point2 As Point) As TrendLine
            Dim t As TrendLine = NewTrendLine(color, point1, point2)
            t.HasParallel = True
            t.IsRegressionLine = True
            t.UseFixedStartRegressionFormula = RegressionMode
            t.Pen = New Pen(New SolidColorBrush(color), thickness)
            t.OuterPen = New Pen(New SolidColorBrush(outerColor), outerThickness)
            t.ExtendRight = extendRight
            Return t
        End Function
        Private Function CreateFillTrendLine(color As Color, outerColor As Color, thickness As Double, outerThickness As Double, abovefill As Color, belowfill As Color, extendRight As Boolean, point1 As Point, point2 As Point) As TrendLine
            Dim t As TrendLine = NewTrendLine(color, point1, point2)
            t.HasParallel = True
            t.IsRegressionLine = True
            t.DrawZoneFill = True
            t.ConfirmedZoneBarStart = 0
            t.NeutralZoneBarStart = Integer.MaxValue
            t.UseFixedStartRegressionFormula = RegressionMode
            t.Pen = New Pen(New SolidColorBrush(color), thickness)
            t.OuterPen = New Pen(New SolidColorBrush(outerColor), outerThickness)
            t.UpZoneFillBrush = New SolidColorBrush(abovefill)
            t.UpNeutralZoneFillBrush = New SolidColorBrush(abovefill)
            t.DownNeutralZoneFillBrush = New SolidColorBrush(belowfill)
            t.DownZoneFillBrush = New SolidColorBrush(belowfill)
            t.ExtendRight = extendRight
            Return t
        End Function
        Private Function CreateFillTrendLine(color As Color, abovefill As Color, belowfill As Color) As TrendLine
            Dim t As TrendLine = NewTrendLine(color, New Point(CurrentBar.Number, CurrentBar.Close), New Point(CurrentBar.Number, CurrentBar.Close))
            t.IsRegressionLine = True
            t.UseFixedStartRegressionFormula = RegressionMode
            t.DrawZoneFill = True
            t.UpZoneFillBrush = New SolidColorBrush(abovefill)
            t.UpNeutralZoneFillBrush = New SolidColorBrush(abovefill)
            t.DownNeutralZoneFillBrush = New SolidColorBrush(belowfill)
            t.DownZoneFillBrush = New SolidColorBrush(belowfill)
            t.ConfirmedZoneBarStart = 0
            t.NeutralZoneBarStart = Integer.MaxValue
            t.ExtendRight = False
            Return t
        End Function
        Private Sub SetFillTrendLine(t As TrendLine, abovefill As Color, belowfill As Color)
            t.IsRegressionLine = True
            t.UseFixedStartRegressionFormula = RegressionMode
            t.DrawZoneFill = True
            t.UpZoneFillBrush = New SolidColorBrush(abovefill)
            t.UpNeutralZoneFillBrush = New SolidColorBrush(abovefill)
            t.DownNeutralZoneFillBrush = New SolidColorBrush(belowfill)
            t.DownZoneFillBrush = New SolidColorBrush(belowfill)
            t.ConfirmedZoneBarStart = 0
            t.NeutralZoneBarStart = Integer.MaxValue
        End Sub
        Private gapLines As Dictionary(Of Point, TrendLine)
        Property GapLine(rvPoint As Point) As TrendLine
            Get
                If Not gapLines.ContainsKey(rvPoint) Then
                    gapLines.Add(rvPoint, NewTrendLine(New Pen(New SolidColorBrush(GapColor), GapLineThickness)))
                End If
                Return gapLines(rvPoint)
            End Get
            Set(value As TrendLine)
                If Not gapLines.ContainsKey(rvPoint) Then
                    gapLines.Add(rvPoint, NewTrendLine(New Pen(New SolidColorBrush(GapColor), GapLineThickness)))
                End If
                gapLines(rvPoint) = value
            End Set
        End Property
        Private Function GapLineExists(rvpoint As Point) As Boolean
            Return gapLines.ContainsKey(rvpoint)
        End Function
        Private Function GetBCTextOffset(pointX As Integer) As Decimal
            Dim offset As Decimal = 0
            For i = 0 To StepUpCount
                If Abs(pointX - lastTrends(i).BCText.Location.X) <= 1 Then
                    offset += BCTextSpacing + TrendLengthTextFontSize
                End If
            Next
            Return offset
        End Function
        Private Sub CalculateSwing()
            swingevnt = False
            If ((Round(CurrentBar.High - pointList(pointList.Count - 1).Y, 5) >= Round(SwingRV, 5) And lastSwingDirection = Direction.Down) Or
                    (Round(pointList(pointList.Count - 1).Y - CurrentBar.Low, 5) >= Round(SwingRV, 5) AndAlso lastSwingDirection = Direction.Up)) And CurrentBar.Number <> CInt(pointList(pointList.Count - 1).X) Then

                prevSwing = lastSwing.TL.Coordinates
                Dim p = New Point(CurrentBar.Number, If(lastSwingDirection = Direction.Down, CurrentBar.High, CurrentBar.Low))
                Dim color As Color = SwingColor

                lastSwing.TL.Pen.Thickness = If(SwingChannelsOn, 0, lastSwing.TL.Pen.Thickness)
                If OsIsBarColoring Then
                    If Round(Abs(p.Y - lastSwing.EndPoint.Y), 5) >= Round(Abs(lastSwing.StartPoint.Y - lastSwing.EndPoint.Y), 5) Then
                        color = If(lastSwingDirection, OsUpSwingColor, OsDownSwingColor)
                    Else
                        color = If(lastSwingDirection, IsUpSwingColor, IsDownSwingColor)
                    End If
                End If
                swings.Add(New Swing(NewTrendLine(color, lastSwing.EndPoint, p, True), Not lastSwingDirection))
                lastSwingDirection = Not lastSwingDirection
                lastSwing.TL.Pen.Thickness = If(Not LastSwingChannelOn Or (LastSwingChannelOn And Not SwingChannelsOn), LastSwingLineThickness, 0)

                If SwingChannelsOn Then
                    confirmedSwingChannel.OuterPen.Thickness = ConfirmedSwingChannelLineThickness
                    confirmedSwingChannel.Pen.Thickness = ConfirmedSwingRegressionLineThickness
                    confirmedSwingChannel.ExtendRight = False
                    confirmedSwingChannel = NewTrendLine(color, lastSwing.TL.StartPoint, lastSwing.TL.EndPoint, True)
                    confirmedSwingChannel.HasParallel = True
                    confirmedSwingChannel.IsRegressionLine = True
                    confirmedSwingChannel.UseFixedStartRegressionFormula = RegressionMode
                    confirmedSwingChannel.OuterPen.Thickness = ConfirmedSwingChannelLineThickness
                    confirmedSwingChannel.ExtendRight = True
                    confirmedSwingChannel.OuterPen.Thickness = 0
                    confirmedSwingChannel.Pen.Thickness = 0
                End If
                If LastSwingChannelOn Then
                    confirmedSwingChannel.OuterPen.Thickness = ConfirmedSwingChannelLineThickness
                    confirmedSwingChannel.Pen.Thickness = ConfirmedSwingRegressionLineThickness
                End If
                confirmedSwingChannel.Coordinates = lastSwing.TL.Coordinates
                pointList.Add(p)
                lastSwing.LengthText = CreateText(AddToY(lastSwing.EndPoint, If(lastSwingDirection = Direction.Up, 1, -1) * Chart.GetRelativeFromRealHeight(TextSpacing)), lastSwingDirection, Abs(lastSwing.EndPrice - lastSwing.StartPrice), SwingColor)
                For Each item In currentSwingBCLengths
                    RemoveObjectFromChart(item)
                Next
                currentSwingBCLengths.Clear()
                currentSwingBCLengths.Add(Nothing)
                swingevnt = True
                lastSwing.IsOutsideSwing = False
            ElseIf (Round(CurrentBar.High, 5) >= Round(pointList(pointList.Count - 1).Y, 5) And lastSwingDirection = Direction.Up) Or
                       (Round(CurrentBar.Low, 5) <= Round(pointList(pointList.Count - 1).Y, 5) And lastSwingDirection = Direction.Down) Then
                Dim p = New Point(CurrentBar.Number, If(lastSwingDirection = Direction.Up, CurrentBar.High, CurrentBar.Low))
                Dim color As Color = SwingColor
                lastSwing.TL.EndPoint = p
                pointList(pointList.Count - 1) = p
                If OsIsBarColoring And pointList.Count > 2 Then
                    If Round(Abs(pointList(pointList.Count - 1).Y - pointList(pointList.Count - 2).Y), 5) >= Round(Abs(pointList(pointList.Count - 2).Y - pointList(pointList.Count - 3).Y), 5) Then
                        color = If(lastSwingDirection, OsUpSwingColor, OsDownSwingColor)
                    Else
                        color = If(lastSwingDirection, IsUpSwingColor, IsDownSwingColor)
                    End If
                End If
                lastSwing.TL.Pen.Brush = New SolidColorBrush(color)
                confirmedSwingChannel.Pen.Brush = New SolidColorBrush(color)
                UpdateText(lastSwing.LengthText, AddToY(lastSwing.EndPoint, If(lastSwingDirection = Direction.Up, 1, -1) * Chart.GetRelativeFromRealHeight(TextSpacing)),
                                lastSwingDirection, Abs(lastSwing.EndPrice - lastSwing.StartPrice))
                swingevnt = True
                confirmedSwingChannel.Coordinates = lastSwing.TL.Coordinates
            End If
            'up outside - u
            'up inside - i
            'down outside - d
            'down inside - f
            '
            If swingevnt Then
                Dim pattern As String = ""
                For i = 1 To swings.Count - 1
                    If swings(i).Direction = Direction.Up Then
                        If Round(Abs(swings(i).EndPrice - swings(i).StartPrice), 5) >= Round(Abs(swings(i - 1).EndPrice - swings(i - 1).StartPrice), 5) Then
                            pattern &= "u"
                        Else
                            pattern &= "i"
                        End If

                    Else
                        If Round(Abs(swings(i).EndPrice - swings(i).StartPrice), 5) >= Round(Abs(swings(i - 1).EndPrice - swings(i - 1).StartPrice), 5) Then
                            pattern &= "d"
                        Else
                            pattern &= "f"
                        End If
                    End If
                Next
                Dim match = System.Text.RegularExpressions.Regex.Match(pattern, GetPreset(CurrentPresetPattern), RegularExpressions.RegexOptions.IgnoreCase)
                For i = 0 To swings.Count - 2
                    swings(i).TL.Pen.Thickness = If(SwingChannelsOn, 0, SwingLineThickness)
                Next
                lastSwing.TL.Pen.Thickness = If(Not LastSwingChannelOn Or (LastSwingChannelOn And Not SwingChannelsOn), LastSwingLineThickness, 0)
                Do While match.Success
                    For i = match.Index To match.Index + match.Length - 1
                        swings(i + 1).TL.Pen.Thickness = IsOsSwingThickness
                    Next
                    match = match.NextMatch
                Loop
                Dim a As New Object
            End If
            If OsIsBarColoring And Not AbcChannelMode Then
                'If swingevnt Then
                '    BarColorRoutine(lastSwing.StartBar, CurrentBar.Number, lastSwing.TL.Pen.Brush)
                'Else
                ColorCurrentBar(NeutralBarColor)
                'End If
            Else
                If Not AbcBarColoring Then
                    If swingevnt Then
                        'BarColorRoutine(lastSwing.StartBar, CurrentBar.Number, lastTrends(0).TL.Pen.Brush)
                    Else
                        ColorCurrentBar(NeutralBarColor)
                    End If
                Else
                    ColorCurrentBar(NeutralBarColor)
                End If
            End If
            If LastSwingChannelOn Then
                potentialSwingChannel.Coordinates = New LineCoordinates(lastSwing.StartPoint, New Point(CurrentBar.Number, CurrentBar.Close))
            End If
            If swingevnt And ((AbcBarColoring) Or AbcChannelMode) Then
                If mergedTrend.StartPoint = New Point(0, 0) Then
                    mergedTrend = New LineCoordinates(lastSwing.StartPoint, lastSwing.EndPoint)
                    'NewTrendLine(Colors.Pink, mergedTrends(mergedTrends.Count - 1).StartPoint, mergedTrends(mergedTrends.Count - 1).EndPoint)
                    If AbcChannelMode Then
                        abcChannel.RegressionTL =
                                    CreateFillTrendLine(If(lastSwingDirection, ConfirmedTrendUpColor, ConfirmedTrendDownColor),
                                                            If(lastSwingDirection, ConfirmedTrendUpColor, ConfirmedTrendDownColor),
                                                            ConfirmedTrendRegressionLineThickness,
                                                            ConfirmedTrendChannelLineThickness,
                                                            AboveFillColor,
                                                            BelowFillColor,
                                                            True,
                                                            mergedTrend.StartPoint,
                                                            mergedTrend.EndPoint)
                        abcChannel.LengthText = CreateText(AddToY(lastSwing.EndPoint, If(lastSwingDirection, 1, -1) * Chart.GetRelativeFromRealHeight(TextSpacing + SwingLengthTextFontSize)), lastSwingDirection, Abs(mergedTrend.TopY - mergedTrend.BottomY), If(lastSwingDirection, ConfirmedTrendUpColor, ConfirmedTrendDownColor))
                    End If
                    If AbcChannelMode Or AbcBarColoring Then
                        RemoveObjectFromChart(abcChannel.PushCountText)
                        abcChannel.PushCountText = CreateText(lastSwing.EndPoint, TextSpacing + SwingLengthTextFontSize + TrendLengthTextFontSize, lastSwingDirection, "1", HitCountTextColor, PushCountFontSize, PushCountFontWeight)
                    End If
                    BarColorRoutine(lastSwing.StartBar, lastSwing.EndBar, If(lastSwingDirection = Direction.Up, ConfirmedTrendUpColor, ConfirmedTrendDownColor))
                End If
                Dim mergedTrendDir As Boolean
                mergedTrendDir = mergedTrend.EndPoint.Y > mergedTrend.StartPoint.Y

                If lastSwingDirection = mergedTrendDir Then ' extension
                    If (mergedTrendDir And Round(lastSwing.EndPoint.Y, 5) >= Round(mergedTrend.EndPoint.Y, 5)) Or
                            (Not mergedTrendDir And Round(lastSwing.EndPoint.Y, 5) <= Round(mergedTrend.EndPoint.Y, 5)) Then
                        BarColorRoutine(mergedTrend.EndPoint.X, lastSwing.EndBar, If(lastSwingDirection = Direction.Up, ConfirmedTrendUpColor, ConfirmedTrendDownColor))
                        mergedTrend = New LineCoordinates(mergedTrend.StartPoint, lastSwing.EndPoint)
                        If AbcChannelMode Then
                            abcChannel.RegressionTL.Coordinates = mergedTrend
                            UpdateText(abcChannel.LengthText, AddToY(lastSwing.EndPoint, If(lastSwingDirection, 1, -1) * Chart.GetRelativeFromRealHeight(TextSpacing + SwingLengthTextFontSize)), lastSwingDirection, Abs(mergedTrend.TopY - mergedTrend.BottomY), If(lastSwingDirection, ConfirmedTrendUpColor, ConfirmedTrendDownColor))
                        End If
                        If AbcChannelMode Or AbcBarColoring Then
                            If Not reversalSwingMove Then
                                abcChannel.PushCountText.Location = AddToY(lastSwing.EndPoint, If(lastSwingDirection, 1, -1) * Chart.GetRelativeFromRealHeight(TextSpacing + SwingLengthTextFontSize + TrendLengthTextFontSize))
                            Else
                                abcChannel.PushCountText.Location = AddToY(abcChannel.PushCountText.Location, If(Not lastSwingDirection, 1, -1) * Chart.GetRelativeFromRealHeight(TrendLengthTextFontSize))
                                RemoveObjectFromChart(abcChannel.PushCountText)
                                abcChannel.PushCountText = CreateText(lastSwing.EndPoint, TextSpacing + SwingLengthTextFontSize + TrendLengthTextFontSize, lastSwingDirection, CInt(abcChannel.PushCountText.Text) + 1, HitCountTextColor, PushCountFontSize, PushCountFontWeight)
                            End If
                        End If
                        'NewTrendLine(Colors.LightBlue, mergedTrend.StartPoint, mergedTrend.EndPoint)
                        reversalSwingMove = False
                    End If
                Else ' check for new 3-swing trend - check if there is at least a swing in between last trend and now
                    'swing against the direction the trend - set reversal flag
                    reversalSwingMove = True
                    If CInt(lastSwing.StartPoint.X) > CInt(mergedTrend.EndPoint.X) Then
                        Dim aPoint As Point = New Point(0, If(lastSwingDirection = False, Decimal.MinValue, Decimal.MaxValue)),
                        bPoint As Point = New Point(0, If(lastSwingDirection = True, Decimal.MinValue, Decimal.MaxValue))
                        Dim searchIndex = pointList.Count - 2
                        While searchIndex > 0 AndAlso CInt(pointList(searchIndex).X) > CInt(mergedTrend.EndPoint.X)
                            searchIndex -= 1
                        End While
                        While searchIndex < pointList.Count And searchIndex >= 0
                            If (lastSwingDirection And Round(pointList(searchIndex).Y, 5) <= Round(aPoint.Y, 5)) Or
                           (Not lastSwingDirection And Round(pointList(searchIndex).Y, 5) >= Round(aPoint.Y, 5)) Then
                                aPoint = New Point(searchIndex, pointList(searchIndex).Y)
                            End If
                            searchIndex += 1
                        End While
                        For searchIndex = aPoint.X + 1 To pointList.Count - 1
                            If (lastSwingDirection And Round(pointList(searchIndex).Y, 5) >= Round(bPoint.Y, 5)) Or
                           (Not lastSwingDirection And Round(pointList(searchIndex).Y, 5) <= Round(bPoint.Y, 5)) Then
                                bPoint = New Point(searchIndex, pointList(searchIndex).Y)
                            End If
                        Next
                        If bPoint.X = pointList.Count - 1 And CInt(pointList(aPoint.X).X) = CInt(mergedTrend.EndPoint.X) Then
                            mergedTrend = New LineCoordinates(mergedTrend.EndPoint, lastSwing.EndPoint)
                            'NewTrendLine(Colors.Pink, mergedTrend.StartPoint, mergedTrend.EndPoint)
                            If AbcChannelMode Then
                                abcChannel.RegressionTL.ExtendRight = False
                                'SetFillTrendLine(abcChannel.RegressionTL, AboveFillColor, BelowFillColor)
                                abcChannel.RegressionTL.DrawZoneFill = False
                                abcChannel.RegressionTL.OuterPen.Thickness = ConfirmedTrendChannelLineThickness
                                abcChannel.RegressionTL.Pen.Thickness = ConfirmedTrendRegressionLineThickness
                                abcChannel.RegressionTL =
                                    CreateParallelTrendLine(If(lastSwingDirection, ConfirmedTrendUpColor, ConfirmedTrendDownColor),
                                                            If(lastSwingDirection, ConfirmedTrendUpColor, ConfirmedTrendDownColor),
                                                            ConfirmedTrendRegressionLineThicknessHLite,
                                                            ConfirmedTrendChannelLineThicknessHLite,
                                                            True,
                                                            mergedTrend.StartPoint,
                                                            mergedTrend.EndPoint)
                                abcChannel.LengthText = CreateText(AddToY(lastSwing.EndPoint, If(lastSwingDirection, 1, -1) * Chart.GetRelativeFromRealHeight(TextSpacing + SwingLengthTextFontSize)), lastSwingDirection, Abs(mergedTrend.TopY - mergedTrend.BottomY), If(lastSwingDirection, ConfirmedTrendUpColor, ConfirmedTrendDownColor))
                            End If
                            If AbcChannelMode Or AbcBarColoring Then
                                'push count text
                                'CreateText(pointList(aPoint.X + 1), TextSpacing + SwingLengthTextFontSize, lastSwingDirection, "1", HitCountTextColor, PushCountFontSize, PushCountFontWeight)
                                abcChannel.PushCountText = CreateText(lastSwing.EndPoint, TextSpacing + SwingLengthTextFontSize + TrendLengthTextFontSize, lastSwingDirection, "2", HitCountTextColor, PushCountFontSize, PushCountFontWeight)
                                reversalSwingMove = False
                            End If
                            BarColorRoutine(mergedTrend.StartPoint.X, lastSwing.EndBar, If(lastSwingDirection = Direction.Up, ConfirmedTrendUpColor, ConfirmedTrendDownColor))
                        End If
                    Else
                        'single swing trend
                        If (mergedTrendDir And lastSwing.EndPoint.Y <= mergedTrend.StartPoint.Y) Or
                        (Not mergedTrendDir And lastSwing.EndPoint.Y >= mergedTrend.StartPoint.Y) Then
                            mergedTrend = New LineCoordinates(lastSwing.StartPoint, lastSwing.EndPoint)
                            'NewTrendLine(Colors.Blue, mergedTrend.StartPoint, mergedTrend.EndPoint)
                            If AbcChannelMode Then
                                abcChannel.RegressionTL.ExtendRight = False
                                'SetFillTrendLine(abcChannel.RegressionTL, AboveFillColor, BelowFillColor)
                                abcChannel.RegressionTL.DrawZoneFill = False
                                abcChannel.RegressionTL.OuterPen.Thickness = ConfirmedTrendChannelLineThickness
                                abcChannel.RegressionTL.Pen.Thickness = ConfirmedTrendRegressionLineThickness
                                abcChannel.RegressionTL =
                                    CreateParallelTrendLine(If(lastSwingDirection, ConfirmedTrendUpColor, ConfirmedTrendDownColor),
                                                            If(lastSwingDirection, ConfirmedTrendUpColor, ConfirmedTrendDownColor),
                                                            ConfirmedTrendRegressionLineThicknessHLite,
                                                            ConfirmedTrendChannelLineThicknessHLite,
                                                            True,
                                                            mergedTrend.StartPoint,
                                                            mergedTrend.EndPoint)
                                abcChannel.LengthText = CreateText(AddToY(lastSwing.EndPoint, If(lastSwingDirection, 1, -1) * Chart.GetRelativeFromRealHeight(TextSpacing + SwingLengthTextFontSize)), lastSwingDirection, Abs(mergedTrend.TopY - mergedTrend.BottomY), If(lastSwingDirection, ConfirmedTrendUpColor, ConfirmedTrendDownColor))
                            End If
                            If AbcChannelMode Or AbcBarColoring Then
                                'push count text
                                'RemoveObjectFromChart(abcChannel.PushCountText)
                                abcChannel.PushCountText = CreateText(lastSwing.EndPoint, TextSpacing + SwingLengthTextFontSize + TrendLengthTextFontSize, lastSwingDirection, "1", HitCountTextColor, PushCountFontSize, PushCountFontWeight)
                                reversalSwingMove = False
                            End If
                            BarColorRoutine(lastSwing.StartBar, lastSwing.EndBar, If(lastSwingDirection = Direction.Up, ConfirmedTrendUpColor, ConfirmedTrendDownColor))
                        End If
                    End If
                End If
            End If
            If swingevnt Then
                If GapLinesOnSwing Then
                    'check for gap swing
                    If pointList.Count > 2 AndAlso ((lastSwingDirection And lastSwing.EndPrice >= pointList(pointList.Count - 3).Y) Or (lastSwingDirection = False And lastSwing.EndPrice <= pointList(pointList.Count - 3).Y)) Then
                        GapLine(pointList(pointList.Count - 3)).StartPoint = pointList(pointList.Count - 3)
                        GapLine(pointList(pointList.Count - 3)).EndPoint = AddToX(pointList(pointList.Count - 3), 20)
                        GapLine(pointList(pointList.Count - 3)).ExtendRight = True
                    End If
                    If pointList.Count > 3 AndAlso (GapLineExists(pointList(pointList.Count - 4)) And
                            ((lastSwingDirection And lastSwing.EndPrice >= pointList(pointList.Count - 4).Y) Or (lastSwingDirection = False And lastSwing.EndPrice <= pointList(pointList.Count - 4).Y))) Then
                        RemoveObjectFromChart(GapLine(pointList(pointList.Count - 4)))
                    End If
                    If pointList.Count > 4 AndAlso (GapLineExists(pointList(pointList.Count - 5)) And ((lastSwingDirection And pointList(pointList.Count - 2).Y > pointList(pointList.Count - 5).Y) Or (lastSwingDirection = False And pointList(pointList.Count - 2).Y < pointList(pointList.Count - 5).Y))) Then
                        GapLine(pointList(pointList.Count - 5)).EndPoint = New Point(pointList(pointList.Count - 2).X, pointList(pointList.Count - 5).Y)
                        GapLine(pointList(pointList.Count - 5)).ExtendRight = False
                    End If
                End If
            End If
            If AbcChannelMode Then
                abcChannel.PotentialTL.Coordinates = New LineCoordinates(mergedTrend.StartPoint, New Point(CurrentBar.Number, 0))
                abcChannel.PotentialTL.Pen = New Pen(New SolidColorBrush(If(lastSwingDirection, PotentialTrendUpColor, PotentialTrendDownColor)), PotentialTrendRegressionLineThicknessHLite)
                abcChannel.PotentialTL.OuterPen = New Pen(New SolidColorBrush(If(lastSwingDirection, PotentialTrendUpColor, PotentialTrendDownColor)), PotentialTrendChannelLineThicknessHLite)
                abcChannel.ExtendTargetText.Location = New Point(CurrentBar.Number, mergedTrend.EndPoint.Y)
                abcChannel.ExtendTargetText.Text = Strings.StrDup(ExtendTargetTextLineLength, "─") & " Extend " &
                            Round(mergedTrend.EndPoint.Y, 2)
                abcChannel.ExtendTargetText.Font.Brush = New SolidColorBrush(If(mergedTrend.EndPoint.Y > mergedTrend.StartPoint.Y, ConfirmedTrendUpColor, ConfirmedTrendDownColor))
                'find swing index of mergedtrend endpoint
                Dim searchIndex = pointList.Count - 1
                While searchIndex > 0 AndAlso CInt(pointList(searchIndex).X) > CInt(mergedTrend.EndPoint.X)
                    searchIndex -= 1
                End While
                If pointList.Count - searchIndex >= 3 Then
                    abcChannel.TargetText.Location = New Point(CurrentBar.Number, pointList(searchIndex + 1).Y)
                    abcChannel.TargetText.Text = Strings.StrDup(RVTargetTextLineLength, "─") & " " &
                                FormatNumberLengthAndPrefix(Abs(pointList(searchIndex + 1).Y - mergedTrend.EndPoint.Y)) & " New " &
                                Round(pointList(searchIndex + 1).Y, 2)
                Else
                    abcChannel.TargetText.Location = New Point(CurrentBar.Number, mergedTrend.StartPoint.Y)
                    abcChannel.TargetText.Text = Strings.StrDup(RVTargetTextLineLength, "─") & " " &
                                FormatNumberLengthAndPrefix(Abs(mergedTrend.StartPoint.Y - mergedTrend.EndPoint.Y)) & " New " &
                                Round(mergedTrend.StartPoint.Y, 2)
                End If

                abcChannel.TargetText.Font.Brush = New SolidColorBrush(If(mergedTrend.EndPoint.Y < mergedTrend.StartPoint.Y, ConfirmedTrendUpColor, ConfirmedTrendDownColor))
            End If
            swingExtendText.Location = New Point(CurrentBar.Number, lastSwing.EndPoint.Y)
            swingTargetText.Location = New Point(CurrentBar.Number, lastSwing.EndPoint.Y + SwingRV * If(lastSwingDirection, -1, 1))
            swingTargetText.Text = Strings.StrDup(SwingTargetTextLineLength, "─") & " " & FormatNumberLengthAndPrefix(SwingRV) & " RV  " ' & Round(targetText.Location.Y, Chart.Settings("DecimalPlaces").Value)
            CalculateSwingBCTexts()
        End Sub
        Sub CalculateSwingBCTexts()
            Dim cpoint As Point
            Dim bpoint As New Point(0, If(lastSwingDirection, 0, Double.MaxValue))

            For i = CurrentBar.Number - 1 To lastSwing.StartBar - 1 Step -1
                If lastSwingDirection And Round(Chart.bars(i).Data.High, 5) > bpoint.Y Then
                    bpoint = New Point(i, Chart.bars(i).Data.High)
                ElseIf Not lastSwingDirection And Round(Chart.bars(i).Data.Low, 5) < bpoint.Y Then
                    bpoint = New Point(i, Chart.bars(i).Data.Low)
                End If
            Next
            cpoint = bpoint
            For i = bpoint.X To CurrentBar.Number - 1
                If Not lastSwingDirection And Round(Chart.bars(i).Data.High, 5) >= Round(cpoint.Y, 5) Then
                    cpoint = New Point(i, Chart.bars(i).Data.High)
                ElseIf lastSwingDirection And Round(Chart.bars(i).Data.Low, 5) <= Round(cpoint.Y, 5) Then
                    cpoint = New Point(i, Chart.bars(i).Data.Low)
                End If
            Next
            If bpoint.X = CurrentBar.Number - 1 Then
                If currentSwingBCLengths(currentSwingBCLengths.Count - 1) IsNot Nothing Then
                    currentSwingBCLengths.Add(Nothing)
                End If
            ElseIf cpoint.X = CurrentBar.Number - 1 And Round(Abs(cpoint.Y - bpoint.Y), 5) >= Round(SwingBCMultiplier * Chart.Settings("RangeValue").Value, 5) Then
                Dim foundBigger As Boolean = False
                For Each item In currentSwingBCLengths
                    If item IsNot Nothing AndAlso Round(item.Tag, 5) > Round(Abs(cpoint.Y - bpoint.Y), 5) Then
                        foundBigger = True
                    End If

                Next

                If currentSwingBCLengths(currentSwingBCLengths.Count - 1) Is Nothing Then
                    currentSwingBCLengths(currentSwingBCLengths.Count - 1) = NewLabel("", SwingColor, New Point(0, 0))
                    AddHandler currentSwingBCLengths(currentSwingBCLengths.Count - 1).MouseDown, Sub(sender As Object, location As Point)
                                                                                                     SwingRV = CDec(CType(sender, Label).Tag) + Chart.GetMinTick
                                                                                                     Chart.ReApplyAnalysisTechnique(Me)
                                                                                                 End Sub
                    AddHandler currentSwingBCLengths(currentSwingBCLengths.Count - 1).MiddleMouseDown, Sub(sender As Object, location As Point)
                                                                                                           SwingRV = Round(CDec(CType(sender, Label).Tag), 5)
                                                                                                           Chart.ReApplyAnalysisTechnique(Me)
                                                                                                       End Sub
                    With currentSwingBCLengths(currentSwingBCLengths.Count - 1)
                        .Font.FontSize = SwingLengthTextFontSize
                        .Font.FontWeight = SwingLengthTextFontWeight
                        .HorizontalAlignment = LabelHorizontalAlignment.Right
                        .VerticalAlignment = If(lastSwingDirection, VerticalAlignment.Top, VerticalAlignment.Bottom)
                        .IsEditable = False
                        'AddHandler .MouseDown, Sub(sender As Object, location As Point)
                        '                           MinRV = CDec(CType(sender, Label).Tag)
                        '                           ReapplyMe()
                        '                       End Sub
                    End With
                End If
                currentSwingBCLengths(currentSwingBCLengths.Count - 1).Location = New Point(CurrentBar.Number, If(lastSwingDirection, CurrentBar.Low, CurrentBar.High))
                currentSwingBCLengths(currentSwingBCLengths.Count - 1).Text = FormatNumberLengthAndPrefix(Abs(cpoint.Y - bpoint.Y))
                currentSwingBCLengths(currentSwingBCLengths.Count - 1).Tag = Abs(cpoint.Y - bpoint.Y)
            End If
        End Sub
        Private Function CalculateTrend(ByRef trend As Swing, bar As BarData, rv As Decimal, index As Integer) As SwingEvent
            Dim evnt As SwingEvent = SwingEvent.None

            If trend.Direction = Direction.Neutral Then
                If bar.Low < trend.NeutralMinPoint.Y Then trend.NeutralMinPoint = New Point(bar.Number, bar.Low)
                If bar.High < trend.NeutralMaxPoint.Y Then trend.NeutralMaxPoint = New Point(bar.Number, bar.High)
            End If

            If (Round(bar.High - trend.NeutralMinPoint.Y, 5) >= Round(rv, 5) And trend.Direction = Direction.Neutral) Then
                trend.PointList.Clear()
                trend.PointList.Add(trend.NeutralMinPoint)
                trend.TL.StartPoint = trend.NeutralMinPoint
                trend.TL.EndPoint = trend.NeutralMinPoint
                trend.Direction = Direction.Down
            ElseIf (Round(trend.NeutralMaxPoint.Y - bar.Low, 5) >= Round(rv, 5) And trend.Direction = Direction.Neutral) Then
                trend.PointList.Clear()
                trend.PointList.Add(trend.NeutralMaxPoint)
                trend.TL.StartPoint = trend.NeutralMaxPoint
                trend.TL.EndPoint = trend.NeutralMaxPoint
                trend.Direction = Direction.Up
            End If

            If ((Round(bar.High - trend.EndPrice, 5) >= Round(rv, 5) And trend.Direction = Direction.Down) Or
                (Round(trend.EndPrice - bar.Low, 5) >= Round(rv, 5) AndAlso trend.Direction = Direction.Up)) And bar.Number <> trend.EndBar Then

                'new swing
                If IsLastBarOnChart And Not loadingHistory And index = 0 And AutoRVStepUp Then
                    If trend.Direction = Direction.Down Then
                        BaseTrendRV = Abs(Round(bar.High - trend.EndPrice, 5)) + Chart.GetMinTick
                    Else
                        BaseTrendRV = Abs(Round(bar.Low - trend.EndPrice, 5)) + Chart.GetMinTick
                    End If
                    Chart.ReApplyAnalysisTechnique(Me)
                    Exit Function
                End If
                trend.Direction = Not trend.Direction


                'multiple lines
                trend.TLs.Add(NewTrendLine(New Pen(New SolidColorBrush(If(Not trend.Direction, ConfirmedTrendUpColor, ConfirmedTrendDownColor)), 1), trend.EndPoint, New Point(bar.Number, If(trend.Direction, bar.High, bar.Low)), True))
                trend.TL.Pen.Thickness = 0
                If index = 0 And Not AbcBarColoring Then
                    BarColorRoutine(trend.StartBar, trend.EndBar, If(trend.Direction, ConfirmedTrendUpColor, ConfirmedTrendDownColor))
                End If
                Dim offset = TextSpacing + SwingLengthTextFontSize + TrendLengthTextFontSize * index + StepUpFontSizeIncrement * Max(index - 1, 0)
                trend.LengthTexts.Add(CreateTrendText(AddToY(trend.EndPoint, If(trend.Direction, 1, -1) * Chart.GetRelativeFromRealHeight(offset)), trend.Direction, CStr(Abs(trend.EndPrice - trend.StartPrice)), If(trend.Direction, ConfirmedTrendUpColor, ConfirmedTrendDownColor)))
                trend.LengthBarCountText = CreateBarCountText(AddToY(trend.EndPoint, 0), trend.Direction, CStr(trend.EndBar - trend.StartBar), BarCountTextColor)
                trend.LengthText.Font.FontSize = TrendLengthTextFontSize + index * StepUpFontSizeIncrement
                trend.LengthText.RefreshVisual()

                If TrendChannelsOn And index = 0 Then
                    trend.RegressionTL.ExtendRight = False
                    If HiLiteIndex = 0 Then
                        'SetFillTrendLine(trend.RegressionTL, AboveFillColor, BelowFillColor)
                        trend.RegressionTL.DrawZoneFill = False
                        trend.RegressionTL.Pen.Thickness = ConfirmedTrendRegressionLineThicknessHLite
                        trend.RegressionTL.OuterPen.Thickness = ConfirmedTrendChannelLineThicknessHLite
                    Else
                        trend.RegressionTL.DrawZoneFill = False
                        trend.RegressionTL.Pen.Thickness = 0
                        trend.RegressionTL.OuterPen.Thickness = ConfirmedTrendChannelLineThickness
                    End If

                    trend.RegressionTL = NewTrendLine(Colors.Gray, New Point(0, 0), New Point(0, 1))


                    trend.RegressionTL.ExtendRight = True
                    trend.RegressionTL.HasParallel = True
                    trend.RegressionTL.IsRegressionLine = True
                    trend.RegressionTL.UseFixedStartRegressionFormula = RegressionMode
                    trend.RegressionTL.OuterPen.Thickness = If(index = HiLiteIndex, ConfirmedTrendChannelLineThicknessHLite, ConfirmedTrendChannelLineThickness)
                    trend.RegressionTL.Pen.Thickness = If(index = HiLiteIndex, ConfirmedTrendRegressionLineThicknessHLite, ConfirmedTrendRegressionLineThickness)
                    trend.RegressionTL.RefreshVisual()
                End If
                trend.StepUpIndex = index

                trend.PointList.Add(trend.EndPoint)

                If index = HiLiteIndex And Not AbcBarColoring Then BarColorRoutine(trend.StartBar, trend.EndBar, If(trend.Direction = Direction.Up, ConfirmedTrendUpColor, ConfirmedTrendDownColor))

                evnt = SwingEvent.NewSwing

                trend.IsActive = True

                trend.PotentialTL.Pen.Brush = New SolidColorBrush(If(trend.Direction = Direction.Up, PotentialTrendUpColor, PotentialTrendDownColor))
                trend.PotentialTL.OuterPen.Brush = New SolidColorBrush(If(trend.Direction = Direction.Up, PotentialTrendUpColor, PotentialTrendDownColor))
                trend.PotentialTL.ExtendRight = True
                If index = HiLiteIndex Then
                    trend.PotentialTL.OuterPen.Thickness = PotentialTrendChannelLineThicknessHLite
                    trend.PotentialTL.Pen.Thickness = PotentialTrendRegressionLineThicknessHLite
                Else
                    trend.PotentialTL.OuterPen.Thickness = PotentialTrendChannelLineThickness
                    trend.PotentialTL.Pen.Thickness = PotentialTrendRegressionLineThickness
                End If
                If Not AbcChannelMode And StepUpCount = 0 Then
                    trendHitCountText = CreateText(lastSwing.EndPoint, TextSpacing + SwingLengthTextFontSize + TrendLengthTextFontSize, lastSwingDirection, "1", HitCountTextColor, PushCountFontSize, PushCountFontWeight)
                End If
            ElseIf (bar.High >= trend.EndPrice And trend.Direction = Direction.Up) Or
                       (bar.Low <= trend.EndPrice And trend.Direction = Direction.Down) Then
                ' extension
                trend.EndBar = bar.Number
                trend.EndPrice = If(trend.Direction = Direction.Up, bar.High, bar.Low)
                If index = 0 And Not AbcBarColoring Then
                    BarColorRoutine(trend.StartBar, trend.EndBar, If(trend.Direction, ConfirmedTrendUpColor, ConfirmedTrendDownColor))
                End If

                trend.PointList(trend.PointList.Count - 1) = trend.EndPoint
                Dim offset = TextSpacing + SwingLengthTextFontSize + TrendLengthTextFontSize * index + StepUpFontSizeIncrement * Max(index - 1, 0)
                UpdateText(trend.LengthText, AddToY(trend.EndPoint, If(trend.Direction, 1, -1) * Chart.GetRelativeFromRealHeight(offset)), trend.Direction, CStr(Abs(trend.EndPrice - trend.StartPrice)), If(trend.Direction, ConfirmedTrendUpColor, ConfirmedTrendDownColor))
                trend.LengthBarCountText.Location = AddToX(trend.EndPoint, -2)
                trend.LengthBarCountText.Text = CStr(trend.EndBar-trend.StartBar )
                trend.LengthText.Font.FontSize = TrendLengthTextFontSize + index * StepUpFontSizeIncrement

                If index = HiLiteIndex And Not AbcBarColoring Then BarColorRoutine(trend.StartBar, trend.EndBar, If(trend.Direction = Direction.Up, ConfirmedTrendUpColor, ConfirmedTrendDownColor))
                evnt = SwingEvent.Extension
            End If
            If evnt <> SwingEvent.None Or swingevnt Then

                trend.RegressionTL.Pen.Brush = New SolidColorBrush(If(trend.Direction, ConfirmedTrendUpColor, ConfirmedTrendDownColor))
                trend.RegressionTL.OuterPen.Brush = New SolidColorBrush(If(trend.Direction, ConfirmedTrendUpColor, ConfirmedTrendDownColor))

                trend.RegressionTL.HasParallel = True
                trend.RegressionTL.RefreshVisual()
                If index = index Then
                    Dim aPoint As Point = New Point(0, 0), bPoint As Point = New Point(0, If(trend.Direction = False, Decimal.MaxValue, Decimal.MinValue)), cPoint As Point = New Point(0, If(trend.Direction = Direction.Down, Decimal.MinValue, Decimal.MaxValue))
                    Dim bcLength As Decimal
                    Dim biggestC As Point
                    Dim biggestB As Point
                    For i = pointList.Count - 1 To 0 Step -1
                        Dim p As Point = pointList(i)
                        If p.X >= trend.StartBar Then
                            If (trend.Direction = Direction.Down And p.Y > Round(cPoint.Y, 5)) Or
                                   (trend.Direction = Direction.Up And p.Y < Round(cPoint.Y, 5)) Then
                                cPoint = p
                                For j = i - 1 To 0 Step -1
                                    Dim p2 As Point = pointList(j)
                                    If p2.X >= trend.StartBar Then
                                        If (trend.Direction = Direction.Down And p2.Y > Round(cPoint.Y, 5)) Or
                                                (trend.Direction = Direction.Up And p2.Y < Round(cPoint.Y, 5)) Then
                                            Exit For
                                        Else
                                            If (trend.Direction = Direction.Down And Round(Abs(p2.Y - cPoint.Y), 5) > Round(bcLength, 5)) Or
                                               (trend.Direction = Direction.Up And Round(Abs(p2.Y - cPoint.Y), 5) > Round(bcLength, 5)) Then
                                                biggestC = cPoint
                                                biggestB = p2
                                                bcLength = If(trend.Direction, Abs(p2.Y - cPoint.Y), Abs(p2.Y - cPoint.Y))
                                            End If
                                        End If
                                    Else
                                        Exit For
                                    End If
                                Next
                            End If
                        End If
                    Next
                    Dim isBCConfirmed As Boolean
                    If trend.Direction = Direction.Up Then
                        isBCConfirmed = biggestC.Y + bcLength < trend.EndPrice
                    Else
                        isBCConfirmed = biggestC.Y - bcLength > trend.EndPrice
                    End If
                    'AddObjectToChart(trend.BCText)
                    trend.BCText.Location = AddToY(biggestC, If(trend.Direction = Direction.Up, -1, 1) * (Chart.GetRelativeFromRealHeight(BCTextSpacing + TextSpacing + SwingLengthTextFontSize)))
                    trend.BCText.Text = FormatNumberLengthAndPrefix(bcLength)
                    trend.BCText.Tag = bcLength
                    trend.BCText.VerticalAlignment = If(trend.Direction = Direction.Up, VerticalAlignment.Top, VerticalAlignment.Bottom)
                    trend.BCText.Font.Brush = New SolidColorBrush(SwingColor)
                    trend.BCBarCountText.Location = AddToY(biggestC, If(trend.Direction = Direction.Up, -1, 1) * (Chart.GetRelativeFromRealHeight(BCTextSpacing + TextSpacing + SwingLengthTextFontSize)))
                    trend.BCBarCountText.VerticalAlignment = If(trend.Direction = Direction.Down, VerticalAlignment.Top, VerticalAlignment.Bottom)
                    trend.BCBarCountText.Text = biggestC.X - trend.EndBar
                    If Not AbcChannelMode And trendHitCountText IsNot Nothing And StepUpCount = 0 Then
                        Dim startIndex As Integer
                        For i = pointList.Count - 1 To 0 Step -1
                            If pointList(i).X > trend.StartBar Then
                                startIndex = i
                            Else
                                Exit For
                            End If
                        Next
                        Dim hitCount As Integer
                        Dim curHigh As Double = pointList(startIndex).Y
                        For i = startIndex To pointList.Count - 1
                            If (trend.Direction And Round(pointList(i).Y, 5) >= Round(curHigh, 5)) Or (Not trend.Direction And Round(pointList(i).Y, 5) <= Round(curHigh, 5)) Then
                                hitCount += 1
                                curHigh = pointList(i).Y
                            End If
                        Next
                        trendHitCountText.Location = AddToY(trend.EndPoint, Chart.GetRelativeFromRealHeight((TextSpacing + SwingLengthTextFontSize + TrendLengthTextFontSize) * If(trend.Direction, 1, -1)))
                        trendHitCountText.Text = hitCount
                    End If
                End If
                    If index = 0 Then
                    If Not GapLinesOnSwing Then
                        'check for gap swing
                        If trend.PointList.Count > 2 AndAlso ((trend.Direction And trend.EndPrice >= trend.PointList(trend.PointList.Count - 3).Y) Or (trend.Direction = False And trend.EndPrice <= trend.PointList(trend.PointList.Count - 3).Y)) Then
                            GapLine(trend.PointList(trend.PointList.Count - 3)).StartPoint = trend.PointList(trend.PointList.Count - 3)
                            GapLine(trend.PointList(trend.PointList.Count - 3)).EndPoint = AddToX(trend.PointList(trend.PointList.Count - 3), 20)
                            GapLine(trend.PointList(trend.PointList.Count - 3)).ExtendRight = True
                        End If
                        If trend.PointList.Count > 3 AndAlso (GapLineExists(trend.PointList(trend.PointList.Count - 4)) And
                                ((trend.Direction And trend.EndPrice >= trend.PointList(trend.PointList.Count - 4).Y) Or (trend.Direction = False And trend.EndPrice <= trend.PointList(trend.PointList.Count - 4).Y))) Then
                            RemoveObjectFromChart(GapLine(trend.PointList(trend.PointList.Count - 4)))
                        End If
                        If trend.PointList.Count > 4 AndAlso (GapLineExists(trend.PointList(trend.PointList.Count - 5)) And ((trend.Direction And trend.PointList(trend.PointList.Count - 2).Y > trend.PointList(trend.PointList.Count - 5).Y) Or (trend.Direction = False And trend.PointList(trend.PointList.Count - 2).Y < trend.PointList(trend.PointList.Count - 5).Y))) Then
                            GapLine(trend.PointList(trend.PointList.Count - 5)).EndPoint = New Point(trend.PointList(trend.PointList.Count - 2).X, trend.PointList(trend.PointList.Count - 5).Y)
                            GapLine(trend.PointList(trend.PointList.Count - 5)).ExtendRight = False
                        End If
                    End If
                End If
            End If
            Return evnt
        End Function
        Private Sub CalculateSwingNeutralBar(ByRef trend As Swing, bar As BarData, rv As Decimal, index As Integer)


            trend.PotentialTL.Coordinates = New LineCoordinates(trend.StartPoint, New Point(bar.Number, bar.Close))
            trend.RegressionTL.Coordinates = New LineCoordinates(trend.StartPoint, trend.EndPoint)

            If IsLastBarOnChart Then
                For i = 0 To StepUpCount
                    Dim offset As Decimal = TextSpacing + SwingLengthTextFontSize + TrendLengthTextFontSize * i + StepUpFontSizeIncrement * Max(i - 1, 0) + GetBCTextOffset(lastTrends(i).EndPoint.X)
                    lastTrends(i).LengthText.Font.FontSize = TrendLengthTextFontSize + i * StepUpFontSizeIncrement
                    lastTrends(i).LengthText.IsEditable = True
                    lastTrends(i).LengthText.IsSelectable = True
                    UpdateText(lastTrends(i).LengthText, AddToY(lastTrends(i).EndPoint, If(lastTrends(i).Direction, 1, -1) * Chart.GetRelativeFromRealHeight(offset)), lastTrends(i).Direction, CStr(Abs(lastTrends(i).EndPrice - lastTrends(i).StartPrice)), If(lastTrends(i).Direction, ConfirmedTrendUpColor, ConfirmedTrendDownColor))
                Next
            End If

            If index = 0 Then
                trend.ExtendTargetText.Location = New Point(bar.Number, trend.EndPrice)
                trend.ExtendTargetText.Font.Brush = New SolidColorBrush(If(Not trend.Direction, ConfirmedTrendDownColor, ConfirmedTrendUpColor))
                trend.ExtendTargetText.Text = Strings.StrDup(ExtendTargetTextLineLength, "─") & " Extend " & Round(trend.EndPrice, 2)
                trend.TargetText.Location = New Point(bar.Number, If(trend.Direction, trend.EndPrice - rv, trend.EndPrice + rv))
                trend.TargetText.Text = Strings.StrDup(RVTargetTextLineLength, "─") & " " & FormatNumberLengthAndPrefix(rv) & " RV " & Round(trend.TargetText.Location.Y, 2) ' & Round(targetText.Location.Y, Chart.Settings("DecimalPlaces").Value)
                trend.TargetText.Font.Brush = New SolidColorBrush(If(trend.Direction, ConfirmedTrendDownColor, ConfirmedTrendUpColor))
            Else
                If trend.Direction = lastTrends(index - 1).Direction Then
                    trend.TargetText.Location = New Point(bar.Number, If(trend.Direction, trend.EndPrice - rv, trend.EndPrice + rv))
                    trend.TargetText.Text = Strings.StrDup(RVTargetTextLineLength, "─") & " " & FormatNumberLengthAndPrefix(rv) & " RV " & Round(trend.TargetText.Location.Y, 2) ' & Round(targetText.Location.Y, Chart.Settings("DecimalPlaces").Value)
                    trend.TargetText.Font.Brush = New SolidColorBrush(If(trend.Direction, ConfirmedTrendDownColor, ConfirmedTrendUpColor))
                    trend.ExtendTargetText.Font.Brush = Brushes.Transparent
                Else
                    trend.ExtendTargetText.Location = New Point(bar.Number, trend.EndPrice)
                    trend.ExtendTargetText.Font.Brush = New SolidColorBrush(If(Not trend.Direction, ConfirmedTrendDownColor, ConfirmedTrendUpColor))
                    trend.ExtendTargetText.Text = Strings.StrDup(ExtendTargetTextLineLength, "─") & " Extend " & Round(trend.EndPrice, 2)
                    trend.TargetText.Font.Brush = Brushes.Transparent
                End If
            End If
            If index = HiLiteIndex Then
                trend.TargetText.Font.FontWeight = TrendTargetTextFontWeight
                trend.ExtendTargetText.Font.FontWeight = TrendTargetTextFontWeight
            Else
                trend.TargetText.Font.FontWeight = FontWeights.Normal
                trend.ExtendTargetText.Font.FontWeight = FontWeights.Normal
            End If
        End Sub
        Protected Overrides Sub Main()
            If CurrentBar.Number < BeginningTrendChannelBuffer Then
                Exit Sub
            End If
            CalculateSwing()
            If Not AbcChannelMode Then
                Dim evnt As Boolean = CalculateTrend(lastTrends(0), CurrentBar, BaseTrendRV, 0) <> SwingEvent.None
                CalculateSwingNeutralBar(lastTrends(0), CurrentBar, BaseTrendRV, 0)
                If (Not loadingHistory) Or (loadingHistory And IsLastBarOnChart) Then
                    For j = 1 To StepUpCount
                        If lastTrends(j - 1).IsActive Then
                            Dim newRV As Decimal = Round(CalculateNewStepUpRV(lastTrends(j - 1).PointList, lastTrends(j - 1).Direction) + Chart.GetMinTick, 5)
                            If evnt Or (loadingHistory And IsLastBarOnChart) Then
                                For Each t In lastTrends(j).LengthTexts
                                    RemoveObjectFromChart(t)
                                Next
                                For Each t In lastTrends(j).TLs
                                    RemoveObjectFromChart(t)
                                Next
                                'RemoveObjectFromChart(lastTrends(j).BCText)
                                lastTrends(j).TLs.Clear()
                                lastTrends(j).LengthTexts.Clear()

                                lastTrends(j).NeutralMaxPoint = New Point(1, Chart.bars(0).Data.Close)
                                lastTrends(j).NeutralMinPoint = New Point(1, Chart.bars(0).Data.Close)
                                lastTrends(j).Direction = Direction.Neutral
                                For Each p In lastTrends(j - 1).PointList
                                    CalculateTrend(lastTrends(j), New BarData(p.Y, p.Y, p.Y, p.Y) With {.Number = p.X}, newRV, j)
                                Next
                            End If
                            CalculateSwingNeutralBar(lastTrends(j), CurrentBar, newRV, j)
                        End If

                        ' delete a bad swing if it spans the chart
                        If lastTrends(j).IsActive = False Or lastTrends(j).StartBar < StepUpStartBar Then
                            lastTrends(j).IsActive = False
                            RemoveObjectFromChart(lastTrends(j).TL)
                            RemoveObjectFromChart(lastTrends(j).RegressionTL)
                            RemoveObjectFromChart(lastTrends(j).PotentialTL)
                            RemoveObjectFromChart(lastTrends(j).LengthText)
                            RemoveObjectFromChart(lastTrends(j).TargetText)
                            RemoveObjectFromChart(lastTrends(j).BCLine)
                            RemoveObjectFromChart(lastTrends(j).ExtendTargetText)
                            RemoveObjectFromChart(lastTrends(j).Channel)
                        End If
                    Next
                End If
            End If
            'rangebox
            'If CurrentBar.Low <= trackBox.Rect.Y - trackBox.Rect.Height Then
            '    potentialBox.MovementDirection = False
            '    trackBox.Rect.Fill = New SolidColorBrush(RangeBoxDownColor)
            '    If CurrentBar.Low <= Round(trackBox.Rect.Y - BaseTrendRV, 5) Then
            '        CreateNewBox(False)
            '    Else
            '        trackBox.Rect.Height = trackBox.Rect.Y - CurrentBar.Low
            '    End If
            'ElseIf CurrentBar.High >= trackBox.Rect.Y Then
            '    potentialBox.MovementDirection = True
            '    trackBox.Rect.Fill = New SolidColorBrush(RangeBoxUpColor)
            '    If CurrentBar.High >= Round(trackBox.Rect.Y - trackBox.Rect.Height + BaseTrendRV, 5) Then
            '        CreateNewBox(True)
            '    Else
            '        trackBox.Rect.Height += CurrentBar.High - trackBox.Rect.Y
            '        trackBox.Rect.Y = CurrentBar.High
            '    End If
            'End If
            'UpdateRangeLines()
            'If potentialBox.Rect.Y < upperActualRangeLine.StartPoint.Y Or potentialBox.Rect.Y - potentialBox.Rect.Height > lowerActualRangeLine.StartPoint.Y Then
            '    potentialBox.Rect.Coordinates = trackBox.Rect.Coordinates
            '    potentialBox.Rect.Height = BaseTrendRV
            'End If
            'If CurrentBar.Close > (trackBox.Rect.Coordinates.Top - trackBox.Rect.Coordinates.Height / 2) Then
            '    potentialBox.Rect.Coordinates = New Rect(potentialBox.Rect.X, upperMaxRangeLine.StartPoint.Y, potentialBox.Rect.Width, potentialBox.Rect.Height)
            'Else
            '    potentialBox.Rect.Coordinates = New Rect(potentialBox.Rect.X, lowerMinRangeLine.StartPoint.Y + potentialBox.Rect.Height, potentialBox.Rect.Width, potentialBox.Rect.Height)
            'End If
            'UpdateProjectionBoxColors(Nothing, Nothing, Nothing)
        End Sub

        Private Function CalculateNewStepUpRV(p As List(Of Point), dir As Direction) As Decimal
            If p.Count > 2 Then
                For i = p.Count - 2 To 0 Step -1
                    If (p(i).Y > p(p.Count - 1).Y And dir = Direction.Up) Or
                           (p(i).Y < p(p.Count - 1).Y And dir = Direction.Down) Then
                        Return Abs(p(p.Count - 1).Y - p(p.Count - 2).Y)
                    ElseIf (p(i).Y < p(p.Count - 2).Y And dir = Direction.Up) Or
                               (p(i).Y > p(p.Count - 2).Y And dir = Direction.Down) Then
                        Dim maxima As Decimal = If(dir = Direction.Up, 0, Decimal.MaxValue)
                        For j = i To p.Count - 2
                            If (p(j).Y > maxima And dir = Direction.Up) Or
                                   (p(j).Y < maxima And dir = Direction.Down) Then
                                maxima = p(j).Y
                            End If
                        Next
                        Return Abs(maxima - p(p.Count - 2).Y)
                    End If
                Next
            End If
            Return Abs(p(p.Count - 1).Y - p(p.Count - 2).Y)
        End Function
        Function CreateText(position As Point, yOffset As Double, direction As Direction, text As String, color As Color, fontSize As Double, fontWeight As FontWeight) As Label
            Dim label As Label = NewLabel(text, color, AddToY(position, Chart.GetRelativeFromRealHeight(If(direction, 1, -1) * yOffset)))
            label.Font.FontSize = fontSize
            label.Font.FontWeight = fontWeight
            label.IsEditable = False
            label.IsSelectable = False
            label.HorizontalAlignment = LabelHorizontalAlignment.Right
            If direction = Direction.Up Then label.VerticalAlignment = LabelVerticalAlignment.Bottom Else label.VerticalAlignment = LabelVerticalAlignment.Top
            Return label
        End Function
        Sub UpdateText(label As Label, position As Point, direction As Direction, value As String, Optional color As Color = Nothing)
            If label IsNot Nothing Then
                label.Text = FormatNumberLengthAndPrefix(value)
                label.Tag = value
                label.Location = AddToX(position, 0)
                If color <> Nothing Then label.Font.Brush = New SolidColorBrush(color)
                If direction = Direction.Up Then label.VerticalAlignment = LabelVerticalAlignment.Bottom Else label.VerticalAlignment = LabelVerticalAlignment.Top
            End If
        End Sub
        Function CreateText(position As Point, direction As Direction, value As String, color As Color) As Label
            Dim lbl = NewLabel(FormatNumberLengthAndPrefix(value), color, New Point(0, 0),, New Font With {.FontSize = SwingLengthTextFontSize, .FontWeight = SwingLengthTextFontWeight}, False) : lbl.HorizontalAlignment = LabelHorizontalAlignment.Right : lbl.IsEditable = True : lbl.IsSelectable = True
            lbl.Tag = value
            lbl.Location = AddToX(position, 0)
            AddHandler lbl.MouseDown, Sub(sender As Object, location As Point)
                                          SwingRV = CDec(CType(sender, Label).Tag) + Chart.GetMinTick
                                          Chart.ReApplyAnalysisTechnique(Me)
                                      End Sub
            AddHandler lbl.MiddleMouseDown, Sub(sender As Object, location As Point)
                                                SwingRV = Round(CDec(CType(sender, Label).Tag), 5)
                                                Chart.ReApplyAnalysisTechnique(Me)
                                            End Sub
            If direction = Direction.Up Then lbl.VerticalAlignment = LabelVerticalAlignment.Bottom Else lbl.VerticalAlignment = LabelVerticalAlignment.Top
            Return lbl
        End Function
        Function CreateTrendText(position As Point, direction As Direction, value As String, color As Color) As Label

            Dim lbl = NewLabel(FormatNumberLengthAndPrefix(value), color, New Point(0, 0),, New Font With {.FontSize = TrendLengthTextFontSize, .FontWeight = TrendLengthTextFontWeight}, False) : lbl.HorizontalAlignment = LabelHorizontalAlignment.Right : lbl.IsEditable = True : lbl.IsSelectable = True
            lbl.Tag = value
            lbl.Location = AddToX(position, 0)
            AddHandler lbl.MouseDown, Sub(sender As Object, location As Point)
                                          BaseTrendRV = CDec(CType(sender, Label).Tag) + Chart.GetMinTick
                                          Chart.ReApplyAnalysisTechnique(Me)
                                      End Sub
            AddHandler lbl.MiddleMouseDown, Sub(sender As Object, location As Point)
                                                BaseTrendRV = Round(CDec(CType(sender, Label).Tag), 5)
                                                Chart.ReApplyAnalysisTechnique(Me)
                                            End Sub
            If direction = Direction.Up Then lbl.VerticalAlignment = LabelVerticalAlignment.Bottom Else lbl.VerticalAlignment = LabelVerticalAlignment.Top
            Return lbl
        End Function
        Function CreateBarCountText(position As Point, direction As Direction, value As String, color As Color) As Label
            Dim lbl = NewLabel(value, color, New Point(0, 0),, New Font With {.FontSize = TrendLengthTextFontSize, .FontWeight = TrendLengthTextFontWeight}, False) : lbl.HorizontalAlignment = LabelHorizontalAlignment.Right : lbl.IsEditable = False : lbl.IsSelectable = False

            lbl.Location = AddToX(position, -2)

            If direction = Direction.Up Then lbl.VerticalAlignment = LabelVerticalAlignment.Top Else lbl.VerticalAlignment = LabelVerticalAlignment.Bottom
            Return lbl
        End Function
        Public Overrides Property Name As String = Me.GetType.Name
        Function FormatNumberLengthAndPrefix(num As Decimal) As String
            Return Round(num * (10 ^ Chart.Settings("DecimalPlaces").Value))
        End Function
        Public Overrides Sub Reset()

            MyBase.Reset()
            If Chart.bars.Count > 1 Then
                For i = Max(0, 0) To Max(Chart.bars.Count - 1, 0)
                    If Not TypeOf Chart.bars(i).Pen.Brush Is SolidColorBrush Then
                        Chart.bars(i).Pen.Brush = New SolidColorBrush
                    End If
                    If CType(Chart.bars(i).Pen.Brush, SolidColorBrush).Color <> Chart.Settings("Bar Color").Value Then
                        Chart.bars(i).Pen.Brush = New SolidColorBrush(Chart.Settings("Bar Color").Value)
                        RefreshObject(Chart.bars(i))
                    End If
                Next
                BarColorRoutine(2, Chart.bars.Count - 1, NeutralBarColor)
            End If
            'Chart.dontDrawBarVisuals = False
        End Sub
        Protected Sub ColorCurrentBar(color As Brush)
            Chart.bars(CurrentBar.Number - 1).Pen.Brush = color
            Chart.bars(CurrentBar.Number - 1).RefreshVisual()
        End Sub
        Protected Sub ColorCurrentBar(color As Color)
            Chart.bars(CurrentBar.Number - 1).Pen.Brush = New SolidColorBrush(color)
            Chart.bars(CurrentBar.Number - 1).RefreshVisual()
        End Sub
        Protected Sub BarColorRoutine(ByVal startBar As Integer, ByVal endBar As Integer, ByVal color As Brush)
            'If BarColoringOff = False Then
            If startBar <> 1 Then
                For i = Max(startBar - 1, 0) To Max(endBar - 1, 0)
                    If TypeOf Chart.bars(i).Pen.Brush IsNot SolidColorBrush OrElse TypeOf color IsNot SolidColorBrush OrElse CType(Chart.bars(i).Pen.Brush, SolidColorBrush).Color <> CType(color, SolidColorBrush).Color Then
                        Chart.bars(i).Pen.Brush = color
                        RefreshObject(Chart.bars(i))
                    End If
                Next
            End If
            'End If
        End Sub
        Protected Sub BarColorRoutine(ByVal startBar As Integer, ByVal endBar As Integer, ByVal color As Color)
            'If BarColoringOff = False Then
            If startBar <> 1 Then
                For i = Max(startBar - 1, 0) To Max(endBar - 1, 0)
                    If Not TypeOf Chart.bars(i).Pen.Brush Is SolidColorBrush Then
                        Chart.bars(i).Pen.Brush = New SolidColorBrush
                    End If
                    If CType(Chart.bars(i).Pen.Brush, SolidColorBrush).Color <> color Then
                        Chart.bars(i).Pen.Brush = New SolidColorBrush(color)
                        RefreshObject(Chart.bars(i))
                    End If
                Next
            End If
            'End If
        End Sub

        'Private Sub UpdateProjectionBoxColors(sender As Object, index As Integer, location As Point)
        '    potentialFillBox1.Fill = Brushes.Transparent
        '    potentialFillBox2.Fill = Brushes.Transparent
        '    potentialFillBox3.Fill = Brushes.Transparent
        '    Exit Sub
        '    If potentialBox.Rect.Y - trackBox.Rect.Y > trackBox.Rect.Y - trackBox.Rect.Height - (potentialBox.Rect.Y - potentialBox.Rect.Height) Then
        '        potentialFillBox1.Fill = New SolidColorBrush(RangeBoxUpColor)
        '        potentialFillBox2.Fill = New SolidColorBrush(RangeBoxUpColor)
        '        potentialFillBox3.Fill = New SolidColorBrush(RangeBoxUpColor)
        '    Else
        '        potentialFillBox1.Fill = New SolidColorBrush(RangeBoxDownColor)
        '        potentialFillBox2.Fill = New SolidColorBrush(RangeBoxDownColor)
        '        potentialFillBox3.Fill = New SolidColorBrush(RangeBoxDownColor)
        '    End If
        '    potentialFillBox1.Coordinates = New Rect(New Point(potentialBox.Rect.X, potentialBox.Rect.Y), New Size(potentialBox.Rect.Width, Max(potentialBox.Rect.Y - trackBox.Rect.Y, 0)))
        '    potentialFillBox2.Coordinates = New Rect(New Point(trackBox.Rect.X + trackBox.Rect.Width, trackBox.Rect.Y), New Size(potentialBox.Rect.Width - trackBox.Rect.Width, trackBox.Rect.Height))
        '    potentialFillBox3.Coordinates = New Rect(New Point(potentialBox.Rect.X, trackBox.Rect.Y - trackBox.Rect.Height), New Size(potentialBox.Rect.Width, Max(trackBox.Rect.Y - trackBox.Rect.Height - (potentialBox.Rect.Y - potentialBox.Rect.Height), 0)))
        'End Sub
        Protected Overrides Sub NewBar()
            'trackBox.Rect.Width = CurrentBar.Number - trackBox.Rect.X
            'potentialBox.Rect.Width = Max(GetProjectedWidth(), trackBox.Rect.Width)
            'potentialBox.Rect.Y = Max(trackBox.Rect.Y, potentialBox.Rect.Y)
            'UpdateRangeLines()
            'UpdateProjectionBoxColors(Nothing, Nothing, Nothing)
        End Sub
        'Private Sub UpdateRangeLines()
        '    If CurrentBar.Number = CurrentBox.Rect.X + CurrentBox.Rect.Width Then
        '        upperActualRangeLine.StartPoint = New Point(upperActualRangeLine.StartPoint.X, Max(If(CurrentBox.Direction, CurrentBar.High, CurrentBox.Rect.Y - CurrentBox.Rect.Height), upperActualRangeLine.StartPoint.Y))
        '        upperActualRangeLine.EndPoint = New Point(trackBox.Rect.X + trackBox.Rect.Width, Max(upperActualRangeLine.StartPoint.Y, upperActualRangeLine.EndPoint.Y))
        '        lowerActualRangeLine.StartPoint = New Point(lowerActualRangeLine.StartPoint.X, Min(If(Not CurrentBox.Direction, CurrentBar.Low, CurrentBox.Rect.Y), lowerActualRangeLine.StartPoint.Y))
        '        lowerActualRangeLine.EndPoint = New Point(trackBox.Rect.X + trackBox.Rect.Width, Min(lowerActualRangeLine.StartPoint.Y, lowerActualRangeLine.EndPoint.Y))
        '    Else
        '        upperActualRangeLine.StartPoint = New Point(upperActualRangeLine.StartPoint.X, Max(CurrentBar.High, upperActualRangeLine.StartPoint.Y))
        '        upperActualRangeLine.EndPoint = New Point(trackBox.Rect.X + trackBox.Rect.Width, Max(upperActualRangeLine.StartPoint.Y, upperActualRangeLine.EndPoint.Y))
        '        lowerActualRangeLine.StartPoint = New Point(lowerActualRangeLine.StartPoint.X, Min(CurrentBar.Low, lowerActualRangeLine.StartPoint.Y))
        '        lowerActualRangeLine.EndPoint = New Point(trackBox.Rect.X + trackBox.Rect.Width, Min(lowerActualRangeLine.StartPoint.Y, lowerActualRangeLine.EndPoint.Y))
        '    End If
        '    upperMaxRangeLine.StartPoint = New Point(upperMaxRangeLine.StartPoint.X, lowerActualRangeLine.StartPoint.Y + BaseTrendRV)
        '    upperMaxRangeLine.EndPoint = New Point(trackBox.Rect.X + Max(trackBox.Rect.Width, 1), upperMaxRangeLine.StartPoint.Y)
        '    lowerMinRangeLine.StartPoint = New Point(lowerMinRangeLine.StartPoint.X, upperActualRangeLine.StartPoint.Y - BaseTrendRV)
        '    lowerMinRangeLine.EndPoint = New Point(trackBox.Rect.X + Max(trackBox.Rect.Width, 1), lowerMinRangeLine.StartPoint.Y)
        '    verticalRangeLine.StartPoint = upperMaxRangeLine.StartPoint
        '    verticalRangeLine.EndPoint = lowerMinRangeLine.StartPoint
        '    potentialBox.Rect.MaxTopValue = lowerActualRangeLine.StartPoint.Y
        '    potentialBox.Rect.MaxBottomValue = upperActualRangeLine.StartPoint.Y
        'End Sub
        'Private Sub CreateNewBox(direction As Boolean)

        '    CurrentBox.Direction = direction
        '    If direction Then
        '        boxes.Add(New Box With {.Rect = NewRectangle(SwingColor, If(potentialBox.MovementDirection, RangeBoxUpColor, RangeBoxDownColor), New Point(trackBox.Rect.X, trackBox.Rect.Y - trackBox.Rect.Height + BaseTrendRV), New Point(CurrentBar.Number, trackBox.Rect.Y - trackBox.Rect.Height), RangeBoxOn)})
        '    Else
        '        boxes.Add(New Box With {.Rect = NewRectangle(SwingColor, If(potentialBox.MovementDirection, RangeBoxUpColor, RangeBoxDownColor), New Point(trackBox.Rect.X, trackBox.Rect.Y), New Point(CurrentBar.Number, trackBox.Rect.Y - BaseTrendRV), RangeBoxOn)})
        '    End If
        '    CurrentBox.Rect.IsEditable = False
        '    CurrentBox.Rect.IsSelectable = False
        '    CurrentBox.Rect.Pen.Thickness = RangeBoxLineThickness

        '    trackBox.Rect.Y = CurrentBar.High
        '    trackBox.Rect.X = CurrentBar.Number
        '    trackBox.Rect.Width = 0
        '    trackBox.Rect.Height = CurrentBar.High - CurrentBar.Low
        '    upperActualRangeLine.Coordinates = New LineCoordinates(CurrentBar.Number, CurrentBox.Rect.Y - If(Not direction, BaseTrendRV, 0), CurrentBar.Number + 5, CurrentBox.Rect.Y - If(Not direction, BaseTrendRV, 0))
        '    lowerActualRangeLine.Coordinates = New LineCoordinates(CurrentBar.Number, CurrentBox.Rect.Y - If(Not direction, BaseTrendRV, 0), CurrentBar.Number + 5, CurrentBox.Rect.Y - If(Not direction, BaseTrendRV, 0))
        '    upperMaxRangeLine.Coordinates = New LineCoordinates(CurrentBar.Number, CurrentBox.Rect.Y, CurrentBar.Number + 1, CurrentBox.Rect.Y)
        '    lowerMinRangeLine.Coordinates = New LineCoordinates(CurrentBar.Number, CurrentBox.Rect.Y, CurrentBar.Number + 1, CurrentBox.Rect.Y)
        '    potentialBox.Rect.Coordinates = trackBox.Rect.Coordinates
        '    potentialBox.Rect.Height = BaseTrendRV
        'End Sub
        'Private Function GetProjectedWidth() As Integer
        '    If boxes.Count > 1 Then
        '        Dim avgWidthSum As Integer
        '        Dim frontBoxPriceRatio As Decimal = Max(Abs(upperActualRangeLine.StartPoint.Y - CurrentBar.Close), Abs(lowerActualRangeLine.StartPoint.Y - CurrentBar.Close)) / BaseTrendRV
        '        Dim barIndex As Integer = CurrentBar.Number - trackBox.Rect.X
        '        Dim percentagesSum As Decimal
        '        For i = boxes.Count - 1 To Max(0, boxes.Count - NumberOfBoxesToUseAsAverage) Step -1
        '            Dim minVal As Decimal = Decimal.MaxValue, maxVal As Decimal = 0
        '            'For j = boxes(i).Rect.X To boxes(i).Rect.X + boxes(i).Rect.Width + 1
        '            '    minVal = Min(minVal, Chart.bars(j - 1).Data.Low)
        '            '    maxVal = Max(maxVal, Chart.bars(j - 1).Data.High)
        '            '    If (maxVal - minVal) / BaseTrendRV >= frontBoxPriceRatio Then
        '            '        If boxes(i).Rect.Width <> 0 Then
        '            '            percentagesSum += (j - boxes(i).Rect.X + 1) / (boxes(i).Rect.Width + 1)
        '            '        Else
        '            '            percentagesSum += 1
        '            '        End If
        '            '        Exit For
        '            '    End If
        '            'Next
        '            avgWidthSum += boxes(i).Rect.Width
        '        Next
        '        Dim avg = avgWidthSum / NumberOfBoxesToUseAsAverage
        '        Dim interp = (CurrentBar.Number - trackBox.Rect.X) / (percentagesSum / NumberOfBoxesToUseAsAverage)
        '        If CurrentBar.Number > Chart.bars.Count - 400 Then
        '            'Log(boxes.Count)
        '            'Log(avg)
        '            'Log(Round(frontBoxPriceRatio, 2) & ", " & Round(frontBoxPriceRatio / (percentagesSum / NumberOfBoxesToUseAsAverage), 2))
        '        End If
        '        If True Then ' enableinterpolation
        '            Return 20 * Exp(-3.37 * frontBoxPriceRatio) * (CurrentBar.Number - trackBox.Rect.X)
        '        Else
        '            Return avg
        '        End If

        '        'If interp > avg Then
        '        '    Return CInt(RecipCalc(interp, avg, ProjectionInterpolationValue, 1 - frontBoxPriceRatio))
        '        'Else
        '        '    Return CInt(interp)
        '        'End If
        '        'Else
        '        ' Return CInt(avgWidthSum / NumberOfBoxesToUseAsAverage)
        '        'End If
        '    Else
        '            Return 0
        '    End If
        'End Function

#Region "AutoTrendPad"

        Dim hilite1 As Button
        Dim hilite2 As Button
        Dim hilite3 As Button
        Dim hilite4 As Button

        Dim btnPresetPattern1 As Button
        Dim btnPresetPattern2 As Button
        Dim btnPresetPattern3 As Button
        Dim btnPresetPattern4 As Button
        Dim btnPresetPattern5 As Button
        Dim btnPresetPattern6 As Button

        Dim increaseTrendHilite As Button
        Dim decreaseTrendHilite As Button
        Dim resetTrendHilite As Button
        Dim abcColorToggle As Button
        Dim osColorToggle As Button
        Dim abcChannelToggle As Button

        Dim addRVMinMoveSwing As Button
        Dim subtractRVMinMoveSwing As Button
        Dim rvBaseValueSwing As Button
        Dim currentRVPopupBtnSwing As Button

        Dim addStepUpMinMove As Button
        Dim subtractStepUpMinMove As Button
        Dim stepUpBaseValue As Button
        Dim currentStepUpPopupBtn As Button


        Dim setAllSwingsChannels As Button
        Dim setLastSwingToChannel As Button
        Dim setAllSwingsLines As Button
        Dim toggleButtonTrend As Button

        Dim addRVMinMove As Button
        Dim subtractRVMinMove As Button
        Dim rvBaseValue As Button
        Dim currentRVPopupBtn As Button


        Dim grabArea As Border
        Dim currentRBPopupBtn As Button
        Dim addRBMinMove As Button
        Dim subtractRBMinMove As Button

        Dim stepUpPopup As Popup
        Dim stepUpPopupGrid As Grid
        Dim rvPopup As Popup
        Dim rvPopupGrid As Grid
        Dim rvPopupSwing As Popup
        Dim rvPopupGridSwing As Grid

        Dim setStartPoint As Button

        Dim popupRB As Popup
        Dim popupRBGrid As Grid
        Dim bd As Border
        Dim grd As Grid
        Private Sub InitGrid()
            grd = New Grid
            bd = New Border
            grd.Margin = New Thickness(0)
            grd.HorizontalAlignment = Windows.HorizontalAlignment.Center
            For i = 1 To 2
                grd.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Auto)})
            Next
            For i = 1 To 26
                grd.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Auto)})
            Next

            grd.Background = Chart.Background
            grd.Resources.MergedDictionaries.Add(New ResourceDictionary)
            grd.Resources.MergedDictionaries(0).Source = New Uri("Themes/OrderButtonStyle.xaml", UriKind.Relative) 'AutoTrendPadButtonStyle
            bd.Child = grd
            bd.BorderBrush = Brushes.Transparent ' New SolidColorBrush(Color.FromArgb(40, 255, 255, 255))
            bd.BorderThickness = New Thickness(0)
            bd.Background = Chart.Background
            bd.CornerRadius = New CornerRadius(0)
            bd.HorizontalAlignment = Windows.HorizontalAlignment.Center

            Dim c As New ContextMenu
            AddHandler c.Opened, Sub() c.IsOpen = False
            bd.ContextMenu = c
            grabArea = New Border With {.BorderThickness = New Thickness(0), .BorderBrush = Brushes.Gray, .Background = Brushes.DarkBlue, .Margin = New Thickness(0.5)}
            grabArea.SetValue(Grid.RowSpanProperty, 1)
            'grabArea.Height = 17
            grd.Children.Add(grabArea)
        End Sub
        Sub SetRows()

            Grid.SetRow(hilite1, 12)
            Grid.SetRow(hilite2, 12)
            Grid.SetRow(hilite3, 13)
            Grid.SetRow(hilite4, 13)
            Grid.SetColumn(hilite1, 0)
            Grid.SetColumn(hilite2, 1)
            Grid.SetColumn(hilite3, 1)
            Grid.SetColumn(hilite4, 0)


            Grid.SetRow(addStepUpMinMove, 0)
            Grid.SetRow(subtractStepUpMinMove, 1)
            Grid.SetRow(stepUpBaseValue, 2)
            Grid.SetRow(currentStepUpPopupBtn, 3)
            Grid.SetColumn(addStepUpMinMove, 1)
            Grid.SetColumn(subtractStepUpMinMove, 1)
            Grid.SetColumn(stepUpBaseValue, 1)
            Grid.SetColumn(currentStepUpPopupBtn, 1)

            Grid.SetRow(addRVMinMove, 0)
            Grid.SetRow(subtractRVMinMove, 1)
            Grid.SetRow(rvBaseValue, 2)
            Grid.SetRow(currentRVPopupBtn, 3)

            Grid.SetRow(increaseTrendHilite, 5)
            Grid.SetRow(decreaseTrendHilite, 6)
            Grid.SetRow(resetTrendHilite, 4)
            Grid.SetColumn(increaseTrendHilite, 1)
            Grid.SetColumn(decreaseTrendHilite, 1)
            Grid.SetColumn(resetTrendHilite, 1)

            Grid.SetRow(toggleButtonTrend, 4)
            Grid.SetRow(setAllSwingsChannels, 5)
            Grid.SetRow(setLastSwingToChannel, 6)
            Grid.SetRow(setAllSwingsLines, 7)


            'Grid.SetRow(abcColorToggle, 6)
            Grid.SetRow(abcChannelToggle, 7)

            'Grid.SetColumn(abcColorToggle, 1)

            Grid.SetColumn(abcChannelToggle, 1)

            Grid.SetRow(addRVMinMoveSwing, 8)
            Grid.SetRow(subtractRVMinMoveSwing, 9)
            Grid.SetRow(rvBaseValueSwing, 10)
            Grid.SetRow(currentRVPopupBtnSwing, 11)

            Grid.SetRow(currentRBPopupBtn, 8)
            Grid.SetRow(addRBMinMove, 9)
            Grid.SetRow(subtractRBMinMove, 10)
            Grid.SetColumn(currentRBPopupBtn, 1)
            Grid.SetColumn(addRBMinMove, 1)
            Grid.SetColumn(subtractRBMinMove, 1)


            Grid.SetRow(grabArea, 11)
            Grid.SetColumn(grabArea, 1)

            Grid.SetRow(btnPresetPattern1, 12)
            Grid.SetColumn(btnPresetPattern1, 0)
            Grid.SetRow(btnPresetPattern2, 12)
            Grid.SetColumn(btnPresetPattern2, 1)
            Grid.SetRow(btnPresetPattern3, 13)
            Grid.SetColumn(btnPresetPattern3, 0)
            Grid.SetRow(btnPresetPattern4, 13)
            Grid.SetColumn(btnPresetPattern4, 1)
            Grid.SetRow(btnPresetPattern5, 14)
            Grid.SetColumn(btnPresetPattern5, 0)
            Grid.SetRow(btnPresetPattern6, 14)
            Grid.SetColumn(btnPresetPattern6, 1)

            grd.Children.Add(addStepUpMinMove)
            grd.Children.Add(subtractStepUpMinMove)
            grd.Children.Add(stepUpBaseValue)
            grd.Children.Add(currentStepUpPopupBtn)

            grd.Children.Add(toggleButtonTrend)
            grd.Children.Add(setAllSwingsChannels)
            grd.Children.Add(setLastSwingToChannel)
            grd.Children.Add(setAllSwingsLines)
            grd.Children.Add(addRVMinMoveSwing)
            grd.Children.Add(subtractRVMinMoveSwing)
            grd.Children.Add(rvBaseValueSwing)
            grd.Children.Add(currentRVPopupBtnSwing)

            grd.Children.Add(addRVMinMove)
            grd.Children.Add(subtractRVMinMove)
            grd.Children.Add(rvBaseValue)
            grd.Children.Add(currentRVPopupBtn)

            grd.Children.Add(currentRBPopupBtn)
            grd.Children.Add(addRBMinMove)
            grd.Children.Add(subtractRBMinMove)
            grd.Children.Add(increaseTrendHilite)
            grd.Children.Add(decreaseTrendHilite)
            grd.Children.Add(resetTrendHilite)
            'grd.Children.Add(abcColorToggle)
            'grd.Children.Add(osColorToggle)
            grd.Children.Add(abcChannelToggle)
            grd.Children.Add(btnPresetPattern1)
            grd.Children.Add(btnPresetPattern2)
            grd.Children.Add(btnPresetPattern3)
            grd.Children.Add(btnPresetPattern4)
            grd.Children.Add(btnPresetPattern5)
            grd.Children.Add(btnPresetPattern6)
            'grd.Children.Add(rBtn)
            'grd.Children.Add(hilite1)
            'grd.Children.Add(hilite2)
            'grd.Children.Add(hilite3)
            'grd.Children.Add(hilite4)

            'grd.Children.Add(setStartPoint)
        End Sub
        Sub InitControls()
            Dim fontsize As Integer = 16
            addStepUpMinMove = New Button With {.Background = Brushes.LightGray, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 31, .Content = "+1"}
            subtractStepUpMinMove = New Button With {.Background = Brushes.LightGray, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "-1"}
            stepUpBaseValue = New Button With {.Background = New SolidColorBrush(Color.FromArgb(255, 230, 235, 255)), .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "-1"}

            currentStepUpPopupBtn = New Button With {.Background = Brushes.Cyan, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19}
            setAllSwingsChannels = New Button With {.Background = Brushes.Yellow, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "///"}
            setLastSwingToChannel = New Button With {.Background = Brushes.Yellow, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "/ /"}
            setAllSwingsLines = New Button With {.Background = Brushes.Yellow, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "/"}
            toggleButtonTrend = New Button With {.Background = Brushes.Pink, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "/ /", .ToolTip = "Toggle the history for level 1 trends"}
            addRVMinMove = New Button With {.Background = Brushes.LightGray, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 31, .Content = "+1"}
            subtractRVMinMove = New Button With {.Background = Brushes.LightGray, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "-1"}
            rvBaseValue = New Button With {.Background = New SolidColorBrush(Color.FromArgb(255, 230, 235, 255)), .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "-1"}
            currentRVPopupBtn = New Button With {.Background = Brushes.Cyan, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19}

            addRVMinMoveSwing = New Button With {.Background = Brushes.LightGray, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "+1"}
            subtractRVMinMoveSwing = New Button With {.Background = Brushes.LightGray, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "-1"}
            rvBaseValueSwing = New Button With {.Background = New SolidColorBrush(Color.FromArgb(255, 230, 235, 255)), .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "-1"}
            currentRVPopupBtnSwing = New Button With {.Background = Brushes.Cyan, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19}

            setStartPoint = New Button With {.Background = Brushes.LightGray, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "Set Start"}

            increaseTrendHilite = New Button With {.Background = New SolidColorBrush(Color.FromArgb(255, 226, 255, 221)), .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "+1"}
            decreaseTrendHilite = New Button With {.Background = New SolidColorBrush(Color.FromArgb(255, 226, 255, 221)), .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "-1"}
            resetTrendHilite = New Button With {.Background = New SolidColorBrush(Color.FromArgb(255, 226, 255, 221)), .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "1"}
            abcColorToggle = New Button With {.Background = New SolidColorBrush(Color.FromArgb(255, 251, 204, 255)), .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "ABC", .ToolTip = "Toggle the coloring of the bars between ABC swing mode and fixed RV trend mode"}
            osColorToggle = New Button With {.Background = Brushes.LightBlue, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "OS"}
            abcChannelToggle = New Button With {.Background = New SolidColorBrush(Color.FromArgb(255, 251, 204, 255)), .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "ABC", .ToolTip = "Toggle the channel drawing mode between fixed RV mode and ABC swing mode"}

            hilite1 = New Button With {.Tag = 0, .Background = Brushes.White, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "1"}
            hilite2 = New Button With {.Tag = 1, .Background = Brushes.White, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "2"}
            hilite3 = New Button With {.Tag = 2, .Background = Brushes.White, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "3"}
            hilite4 = New Button With {.Tag = 3, .Background = Brushes.White, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "4"}

            btnPresetPattern1 = New Button With {.Tag = 1, .Background = Brushes.White, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "1"}
            btnPresetPattern2 = New Button With {.Tag = 2, .Background = Brushes.White, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "2"}
            btnPresetPattern3 = New Button With {.Tag = 3, .Background = Brushes.White, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "3"}
            btnPresetPattern4 = New Button With {.Tag = 4, .Background = Brushes.White, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "4"}
            btnPresetPattern5 = New Button With {.Tag = 5, .Background = Brushes.White, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "5"}
            btnPresetPattern6 = New Button With {.Tag = 6, .Background = Brushes.White, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "6"}

            Dim presetPatternClick =
                Sub(sender As Object, e As EventArgs)
                    btnPresetPattern1.Background = Brushes.White
                    btnPresetPattern2.Background = Brushes.White
                    btnPresetPattern3.Background = Brushes.White
                    btnPresetPattern4.Background = Brushes.White
                    btnPresetPattern5.Background = Brushes.White
                    btnPresetPattern6.Background = Brushes.White
                    CType(sender, Button).Background = Brushes.Orange
                    CurrentPresetPattern = sender.Tag
                    Chart.ReApplyAnalysisTechnique(Me)
                End Sub
            Dim hilite = Sub(sender As Object, e As EventArgs)
                             hilite1.Background = Brushes.White
                             hilite2.Background = Brushes.White
                             hilite3.Background = Brushes.White
                             hilite4.Background = Brushes.White
                             CType(sender, Button).Background = Brushes.Orange
                             HiLiteIndex = sender.Tag
                             Chart.ReApplyAnalysisTechnique(Me)
                         End Sub
            AddHandler hilite1.Click, hilite
            AddHandler hilite2.Click, hilite
            AddHandler hilite3.Click, hilite
            AddHandler hilite4.Click, hilite

            AddHandler btnPresetPattern1.Click, presetPatternClick
            AddHandler btnPresetPattern2.Click, presetPatternClick
            AddHandler btnPresetPattern3.Click, presetPatternClick
            AddHandler btnPresetPattern4.Click, presetPatternClick
            AddHandler btnPresetPattern5.Click, presetPatternClick
            AddHandler btnPresetPattern6.Click, presetPatternClick

            stepUpPopup = New Popup With {.Placement = PlacementMode.Left, .PlacementTarget = currentStepUpPopupBtn, .Width = 208, .Height = 52, .StaysOpen = False}
            stepUpPopupGrid = New Grid With {.Background = Brushes.White}
            stepUpPopup.Child = stepUpPopupGrid
            For x = 1 To 4 : stepUpPopupGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)}) : Next
            For y = 1 To 2 : stepUpPopupGrid.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Star)}) : Next

            rvPopup = New Popup With {.Placement = PlacementMode.Left, .PlacementTarget = currentRVPopupBtn, .Width = 520, .Height = 310, .StaysOpen = False}
            rvPopupGrid = New Grid With {.Background = Brushes.White}
            rvPopup.Child = rvPopupGrid
            For x = 1 To 10 : rvPopupGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)}) : Next
            For y = 1 To 12 : rvPopupGrid.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Star)}) : Next

            rvPopupSwing = New Popup With {.Placement = PlacementMode.Left, .PlacementTarget = currentRVPopupBtnSwing, .Width = 520, .Height = 310, .StaysOpen = False}
            rvPopupGridSwing = New Grid With {.Background = Brushes.White}
            rvPopupSwing.Child = rvPopupGridSwing
            For x = 1 To 10 : rvPopupGridSwing.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)}) : Next
            For y = 1 To 12 : rvPopupGridSwing.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Star)}) : Next


            currentRBPopupBtn = New Button With {.Background = Brushes.Red, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19}
            addRBMinMove = New Button With {.Background = Brushes.LightGray, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "+1"}
            subtractRBMinMove = New Button With {.Background = Brushes.LightGray, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "-1"}

            popupRB = New Popup With {.Placement = PlacementMode.Left, .PlacementTarget = currentRBPopupBtn, .Width = 520, .Height = 310, .StaysOpen = False}
            popupRBGrid = New Grid With {.Background = Brushes.White}
            popupRB.Child = popupRBGrid
            For x = 1 To 10 : popupRBGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)}) : Next
            For y = 1 To 12 : popupRBGrid.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Star)}) : Next
        End Sub
        Sub AddHandlers()
            AddHandler currentStepUpPopupBtn.Click,
                    Sub()
                        stepUpPopup.IsOpen = True
                    End Sub

            AddHandler currentRVPopupBtnSwing.Click,
                    Sub()
                        rvPopupSwing.IsOpen = True
                    End Sub
            AddHandler currentRVPopupBtn.Click,
                    Sub()
                        rvPopup.IsOpen = True
                    End Sub
            AddHandler stepUpPopup.Opened,
                  Sub()
                      stepUpPopupGrid.Children.Clear()
                      For x = 1 To 4
                          For y = 1 To 2
                              Dim value As Integer = x + (y - 1) * 4
                              Dim btn As New Button With {.Margin = New Thickness(1), .Tag = value, .Content = value, .FontSize = 14.5, .Foreground = Brushes.Black}
                              Grid.SetRow(btn, y - 1)
                              Grid.SetColumn(btn, x - 1)
                              btn.Background = Brushes.White
                              If StepUpCount = value Then
                                  btn.Background = Brushes.LightBlue
                              End If
                              stepUpPopupGrid.Children.Add(btn)
                              AddHandler btn.Click,
                                          Sub(sender As Object, e As EventArgs)
                                              stepUpPopup.IsOpen = False
                                              If Round(CDec(CType(sender, Button).Tag)) <> StepUpCount Then
                                                  StepUpCount = CInt(CType(sender, Button).Tag) - 1
                                                  Chart.ReApplyAnalysisTechnique(Me)
                                              End If
                                          End Sub
                          Next
                      Next
                  End Sub

            AddHandler rvPopup.Opened,
                  Sub()
                      rvPopupGrid.Children.Clear()
                      For x = 1 To 10
                          For y = 1 To 12
                              Dim value = Round(((y - 1) * 10 + (1 * Chart.Settings("RangeValue").Value) / Chart.GetMinTick() + x - 1) * Chart.GetMinTick(), 4)
                              Dim btn As New Button With {.Margin = New Thickness(1), .Tag = value, .Content = FormatNumberLengthAndPrefix(value), .FontSize = 14.5, .Foreground = Brushes.Black}
                              Grid.SetRow(btn, y - 1)
                              Grid.SetColumn(btn, x - 1)
                              btn.Background = Brushes.White
                              If Round(BaseTrendRV, 4) = value Then
                                  btn.Background = Brushes.LightBlue
                              End If
                              If value = Round(RoundTo(Chart.Settings("RangeValue").Value * TrendRVMultiplier, Chart.GetMinTick), 4) Then
                                  btn.Background = New SolidColorBrush(Colors.Orange)
                              End If
                              rvPopupGrid.Children.Add(btn)
                              AddHandler btn.Click,
                                          Sub(sender As Object, e As EventArgs)
                                              rvPopup.IsOpen = False
                                              If Round(CDec(CType(sender, Button).Tag), 4) <> Round(BaseTrendRV, 4) Then
                                                  BaseTrendRV = CType(sender, Button).Tag
                                                  Chart.ReApplyAnalysisTechnique(Me)
                                              End If
                                          End Sub
                          Next
                      Next
                  End Sub

            AddHandler rvPopupSwing.Opened,
                  Sub()
                      rvPopupGridSwing.Children.Clear()
                      For x = 1 To 10
                          For y = 1 To 12
                              Dim value = Round(((y - 1) * 10 + (1 * Chart.Settings("RangeValue").Value) / Chart.GetMinTick() + x - 1) * Chart.GetMinTick(), 4)
                              Dim btn As New Button With {.Margin = New Thickness(1), .Tag = value, .Content = FormatNumberLengthAndPrefix(value), .FontSize = 14.5, .Foreground = Brushes.Black}
                              Grid.SetRow(btn, y - 1)
                              Grid.SetColumn(btn, x - 1)
                              btn.Background = Brushes.White
                              If Round(SwingRV, 4) = value Then
                                  btn.Background = Brushes.LightBlue
                              End If
                              If value = Round(RoundTo(Chart.Settings("RangeValue").Value * SwingRVMultiplier, Chart.GetMinTick), 4) Then
                                  btn.Background = New SolidColorBrush(Colors.Orange)
                              End If
                              rvPopupGridSwing.Children.Add(btn)
                              AddHandler btn.Click,
                                          Sub(sender As Object, e As EventArgs)
                                              rvPopupSwing.IsOpen = False
                                              If Round(CDec(CType(sender, Button).Tag), 4) <> Round(SwingRV, 4) Then
                                                  SwingRV = CType(sender, Button).Tag
                                                  Chart.ReApplyAnalysisTechnique(Me)
                                              End If
                                          End Sub
                          Next
                      Next
                  End Sub

            AddHandler addStepUpMinMove.Click,
                    Sub()
                        StepUpCount += 1
                        Chart.ReApplyAnalysisTechnique(Me)
                    End Sub
            AddHandler subtractStepUpMinMove.Click,
                    Sub()
                        If StepUpCount > 0 Then StepUpCount -= 1
                        Chart.ReApplyAnalysisTechnique(Me)
                    End Sub
            AddHandler stepUpBaseValue.Click,
                    Sub(sender As Object, e As EventArgs)
                        If CInt(sender.CommandParameter) - 1 <> StepUpCount Then
                            StepUpCount = CInt(sender.CommandParameter) - 1
                            Chart.ReApplyAnalysisTechnique(Me)
                        End If
                    End Sub

            AddHandler addRVMinMove.Click,
                    Sub()
                        BaseTrendRV += Chart.GetMinTick()
                        Chart.ReApplyAnalysisTechnique(Me)
                    End Sub
            AddHandler subtractRVMinMove.Click,
                    Sub()
                        BaseTrendRV -= Chart.GetMinTick()
                        Chart.ReApplyAnalysisTechnique(Me)
                    End Sub
            AddHandler rvBaseValue.Click,
                    Sub(sender As Object, e As EventArgs)
                        If Round(sender.CommandParameter, 5) <> Round(BaseTrendRV, 5) Then
                            BaseTrendRV = sender.CommandParameter
                            Chart.ReApplyAnalysisTechnique(Me)
                        End If
                    End Sub
            AddHandler addRVMinMoveSwing.Click,
                   Sub()
                       SwingRV += Chart.GetMinTick()
                       Chart.ReApplyAnalysisTechnique(Me)
                   End Sub
            AddHandler subtractRVMinMoveSwing.Click,
                    Sub()
                        SwingRV -= Chart.GetMinTick()
                        Chart.ReApplyAnalysisTechnique(Me)
                    End Sub
            AddHandler rvBaseValueSwing.Click,
                    Sub(sender As Object, e As EventArgs)
                        If Round(sender.CommandParameter, 5) <> Round(SwingRV, 5) Then
                            SwingRV = sender.CommandParameter
                            Chart.ReApplyAnalysisTechnique(Me)
                        End If
                    End Sub
            AddHandler toggleButtonTrend.Click,
                    Sub(sender As Object, e As EventArgs)
                        TrendChannelsOn = Not TrendChannelsOn
                        Chart.ReApplyAnalysisTechnique(Me)
                    End Sub
            AddHandler setAllSwingsChannels.Click,
                    Sub(sender As Object, e As EventArgs)
                        SwingChannelsOn = True
                        LastSwingChannelOn = True
                        Chart.ReApplyAnalysisTechnique(Me)
                    End Sub
            AddHandler setLastSwingToChannel.Click,
                    Sub(sender As Object, e As EventArgs)
                        SwingChannelsOn = False
                        LastSwingChannelOn = True
                        Chart.ReApplyAnalysisTechnique(Me)
                    End Sub
            AddHandler setAllSwingsLines.Click,
                    Sub(sender As Object, e As EventArgs)
                        SwingChannelsOn = False
                        LastSwingChannelOn = False
                        Chart.ReApplyAnalysisTechnique(Me)
                    End Sub

            AddHandler currentRBPopupBtn.Click,
                   Sub()
                       popupRB.IsOpen = True
                   End Sub
            AddHandler popupRB.Opened,
                    Sub()
                        popupRBGrid.Children.Clear()
                        For x = 1 To 10
                            For y = 1 To 12
                                Dim value = Round(((y - 1) * 10 + x) * Chart.GetMinTick(), 4)
                                Dim btn As New Button With {.Margin = New Thickness(1), .Tag = value, .Content = FormatNumberLengthAndPrefix(value), .FontSize = 14.5, .Foreground = Brushes.Black}
                                Grid.SetRow(btn, y - 1)
                                Grid.SetColumn(btn, x - 1)
                                btn.Background = Brushes.White
                                If Round(Chart.Settings("RangeValue").Value, 4) = value Then
                                    btn.Background = Brushes.Pink
                                ElseIf value = RoundTo(BaseTrendRV / 2, Chart.GetMinTick) Or value = RoundTo(BaseTrendRV / 3, Chart.GetMinTick) Or value = RoundTo(BaseTrendRV / 4, Chart.GetMinTick) Then
                                    btn.Background = New SolidColorBrush(Colors.Orange)
                                End If
                                popupRBGrid.Children.Add(btn)
                                AddHandler btn.Click,
                                        Sub(sender As Object, e As EventArgs)
                                            popupRB.IsOpen = False
                                            Chart.ChangeRangeWithDaysBackCalculating(CDec(sender.Tag))
                                        End Sub
                            Next
                        Next
                    End Sub
            AddHandler addRBMinMove.Click,
                    Sub()
                        Chart.ChangeRange(Round(Chart.Settings("RangeValue").Value + Chart.GetMinTick(), 4))
                    End Sub
            AddHandler subtractRBMinMove.Click,
                    Sub()
                        Chart.ChangeRange(Round(Chart.Settings("RangeValue").Value - Chart.GetMinTick(), 4))
                    End Sub
            AddHandler setStartPoint.Click,
                    Sub()
                        For i = StepUpCount To 0 Step -1
                            If lastTrends(i).IsActive Then
                                StepUpStartBar = lastTrends(i).StartBar
                                Exit Sub
                            End If
                        Next
                        StepUpStartBar = 0
                    End Sub
            AddHandler increaseTrendHilite.Click,
                    Sub()
                        HiLiteIndex += 1
                        Chart.ReApplyAnalysisTechnique(Me)
                    End Sub
            AddHandler decreaseTrendHilite.Click,
                    Sub()
                        HiLiteIndex = Max(0, HiLiteIndex - 1)
                        Chart.ReApplyAnalysisTechnique(Me)
                    End Sub
            AddHandler resetTrendHilite.Click,
                    Sub()
                        HiLiteIndex = 0
                        Chart.ReApplyAnalysisTechnique(Me)
                    End Sub

            AddHandler abcColorToggle.Click,
                    Sub()
                        AbcBarColoring = Not AbcBarColoring
                        Chart.ReApplyAnalysisTechnique(Me)
                    End Sub
            AddHandler abcChannelToggle.Click,
                Sub()
                    Dim temp As Double = SecondSwingRV
                    SecondSwingRV = SwingRV
                    SwingRV = temp

                    temp = SecondSwingRVMultiplier
                    SecondSwingRVMultiplier = SwingRVMultiplier
                    SwingRVMultiplier = temp

                    temp = SecondSwingBCMultiplier
                    SecondSwingBCMultiplier = SwingBCMultiplier
                    SwingBCMultiplier = temp

                    Dim tempDec As Decimal
                    tempDec = SecondGapLineThickness
                    SecondGapLineThickness = GapLineThickness
                    GapLineThickness = tempDec

                    tempDec = SecondPushCountFontSize
                    SecondPushCountFontSize = PushCountFontSize
                    PushCountFontSize = tempDec

                    Dim tempbool As Boolean
                    tempbool = SecondGapLinesOnSwing
                    SecondGapLinesOnSwing = GapLinesOnSwing
                    GapLinesOnSwing = tempbool

                    tempDec = SecondSwingLineThickness
                    SecondSwingLineThickness = SwingLineThickness
                    SwingLineThickness = tempDec

                    tempDec = SecondLastSwingLineThickness
                    SecondLastSwingLineThickness = LastSwingLineThickness
                    LastSwingLineThickness = tempDec

                    tempDec = SecondSwingLengthTextFontSize
                    SecondSwingLengthTextFontSize = SwingLengthTextFontSize
                    SwingLengthTextFontSize = tempDec

                    tempDec = SecondSwingTargetTextFontSize
                    SecondSwingTargetTextFontSize = SwingTargetTextFontSize
                    SwingTargetTextFontSize = tempDec

                    Dim tempint = SecondRVTargetTextLineLength
                    SecondRVTargetTextLineLength = RVTargetTextLineLength
                    RVTargetTextLineLength = tempint

                    tempint = SecondExtendTargetTextLineLength
                    SecondExtendTargetTextLineLength = ExtendTargetTextLineLength
                    ExtendTargetTextLineLength = tempint

                    AbcChannelMode = Not AbcChannelMode
                    Chart.ReApplyAnalysisTechnique(Me)
                End Sub
            AddHandler osColorToggle.Click,
                Sub()
                    AbcChannelMode = False
                    OsIsBarColoring = True
                    Chart.ReApplyAnalysisTechnique(Me)
                End Sub
        End Sub
        Public Sub InitChartPad() Implements IChartPadAnalysisTechnique.InitChartPad
            ChartPad = New UIChartControl
            ChartPad.Left = ChartPadLocation.X
            ChartPad.Top = ChartPadLocation.Y
            ChartPad.IsDraggable = True
            InitGrid()
            InitControls()
            SetRows()
            AddHandlers()
            ChartPad.Content = bd
        End Sub

        Public Sub UpdateChartPad() Implements IChartPadAnalysisTechnique.UpdateChartPad
            If addRVMinMove IsNot Nothing Then

                addStepUpMinMove.Background = Brushes.LightGray
                subtractStepUpMinMove.Background = Brushes.LightGray
                'cpBtn.Content = If(LastRegressionFillToggle, "P", "C")

                stepUpBaseValue.Content = 1
                stepUpBaseValue.CommandParameter = 1
                stepUpBaseValue.Background = Brushes.White
                stepUpBaseValue.Foreground = Brushes.Black

                addRVMinMove.Background = Brushes.LightGray
                subtractRVMinMove.Background = Brushes.LightGray

                rvBaseValue.Content = FormatNumberLengthAndPrefix(RoundTo(Chart.Settings("RangeValue").Value * TrendRVMultiplier, Chart.GetMinTick))
                rvBaseValue.CommandParameter = Round(RoundTo(Chart.Settings("RangeValue").Value * TrendRVMultiplier, Chart.GetMinTick), Chart.Settings("DecimalPlaces").Value)
                rvBaseValue.Background = Brushes.White

                currentRVPopupBtn.Content = FormatNumberLengthAndPrefix(BaseTrendRV)
                currentRVPopupBtn.Background = Brushes.LightBlue

                addRVMinMoveSwing.Background = Brushes.LightGray
                subtractRVMinMoveSwing.Background = Brushes.LightGray

                rvBaseValueSwing.Content = FormatNumberLengthAndPrefix(RoundTo(Chart.Settings("RangeValue").Value * SwingRVMultiplier, Chart.GetMinTick))
                rvBaseValueSwing.CommandParameter = Round(RoundTo(Chart.Settings("RangeValue").Value * SwingRVMultiplier, Chart.GetMinTick), Chart.Settings("DecimalPlaces").Value)
                rvBaseValueSwing.Background = Brushes.White

                currentRVPopupBtnSwing.Content = FormatNumberLengthAndPrefix(SwingRV)
                currentRVPopupBtnSwing.Background = Brushes.LightBlue

                currentStepUpPopupBtn.Content = StepUpCount + 1
                currentStepUpPopupBtn.Background = Brushes.LightBlue
                currentStepUpPopupBtn.Foreground = Brushes.Black


                currentRBPopupBtn.Content = FormatNumberLengthAndPrefix(Chart.Settings("RangeValue").Value)
                currentRBPopupBtn.Background = Brushes.Pink

                hilite1.Background = Brushes.White
                hilite2.Background = Brushes.White
                hilite3.Background = Brushes.White
                hilite4.Background = Brushes.White
                If HiLiteIndex = 0 Then hilite1.Background = Brushes.Orange
                If HiLiteIndex = 1 Then hilite2.Background = Brushes.Orange
                If HiLiteIndex = 2 Then hilite3.Background = Brushes.Orange
                If HiLiteIndex = 3 Then hilite4.Background = Brushes.Orange


                btnPresetPattern1.Background = Brushes.White
                btnPresetPattern2.Background = Brushes.White
                btnPresetPattern3.Background = Brushes.White
                btnPresetPattern4.Background = Brushes.White
                btnPresetPattern5.Background = Brushes.White
                btnPresetPattern6.Background = Brushes.White
                If CurrentPresetPattern = 1 Then btnPresetPattern1.Background = Brushes.Orange
                If CurrentPresetPattern = 2 Then btnPresetPattern2.Background = Brushes.Orange
                If CurrentPresetPattern = 3 Then btnPresetPattern3.Background = Brushes.Orange
                If CurrentPresetPattern = 4 Then btnPresetPattern4.Background = Brushes.Orange
                If CurrentPresetPattern = 5 Then btnPresetPattern5.Background = Brushes.Orange
                If CurrentPresetPattern = 6 Then btnPresetPattern6.Background = Brushes.Orange

                addStepUpMinMove.IsEnabled = Not AbcChannelMode
                subtractStepUpMinMove.IsEnabled = Not AbcChannelMode
                stepUpBaseValue.IsEnabled = Not AbcChannelMode
                currentStepUpPopupBtn.IsEnabled = Not AbcChannelMode
                addRVMinMove.IsEnabled = Not AbcChannelMode
                subtractRVMinMove.IsEnabled = Not AbcChannelMode
                rvBaseValue.IsEnabled = Not AbcChannelMode
                rvBaseValue.IsEnabled = Not AbcChannelMode
                currentRVPopupBtn.IsEnabled = Not AbcChannelMode
                increaseTrendHilite.IsEnabled = Not AbcChannelMode
                decreaseTrendHilite.IsEnabled = Not AbcChannelMode
                resetTrendHilite.IsEnabled = Not AbcChannelMode
                abcColorToggle.IsEnabled = Not AbcChannelMode
                toggleButtonTrend.IsEnabled = Not AbcChannelMode

                abcColorToggle.FontWeight = If(AbcBarColoring, FontWeights.Bold, FontWeights.Normal)
            End If
        End Sub
        Public Sub UpdateChartPadColor(color As Color) Implements IChartPadAnalysisTechnique.UpdateChartPadColor
            If ChartPad.Content IsNot Nothing Then
                If color.A = 255 Then
                    ChartPad.Content.Child.Background = New SolidColorBrush(color)
                Else
                    ChartPad.Content.Child.Background = New SolidColorBrush(Color.FromArgb(255, color.R, color.G, color.B))
                End If
            End If
        End Sub

#End Region
    End Class
End Namespace



