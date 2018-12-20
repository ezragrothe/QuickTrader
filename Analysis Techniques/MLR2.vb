'Imports System.Runtime.InteropServices
'Imports System.Math
'Imports System.Windows.Markup
'Imports System.ComponentModel
'Imports System.Windows.Threading
'Imports System.IO
'Imports System.Threading
'Imports System.Collections.ObjectModel
'Imports System.Reflection
'Imports QuickTrader.AnalysisTechniques
'Imports System.Windows.Controls.Primitives
'Imports System.Text
'Imports System.Collections.Specialized
'Namespace AnalysisTechniques
'    Public Class MLR2
'        Inherits AnalysisTechnique
'        Implements IChartPadAnalysisTechnique
'        Public Sub New(ByVal chart As Chart)
'            MyBase.New(chart)
'            Description = "MLR."
'        End Sub
'        Dim lastTrends() As Swing
'        Dim fillUpColors() As Color
'        Dim fillDownColors() As Color
'        Dim swingevnt As Boolean
'        'Dim swingDir As Boolean
'        'Dim targetTextLine As Label
'        'Dim targetText As Label
'        'Dim extendStartTargetText As Label
'        'Dim extendEndTargetText As Label
'        'Dim secondLastPotSwing As TrendLine
'        'Dim lastPotSwing As TrendLine
'        'Dim regressionLine As TrendLine
'        'Dim potentialRegressionLine As TrendLine
'        'Dim lengthText As Label

'        'range box
'        Private boxes As List(Of Box)
'        Private upperMaxRangeLine As TrendLine
'        Private verticalRangeLine As TrendLine
'        Private lowerMinRangeLine As TrendLine
'        Private upperActualRangeLine As TrendLine
'        Private lowerActualRangeLine As TrendLine
'        Private trackBox As Box
'        Private potentialBox As Box
'        Private potentialFillBox1 As Rectangle
'        Private potentialFillBox2 As Rectangle
'        Private potentialFillBox3 As Rectangle

'        Private Class Box
'            Public Property Rect As Rectangle
'            Public Property MovementDirection As Boolean
'            Public Property Direction As Boolean
'        End Class

'        Private ReadOnly Property CurrentBox As Box
'            Get
'                Return boxes(boxes.Count - 1)
'            End Get
'        End Property

'        Public Property ChartPadVisible As Boolean = True Implements IChartPadAnalysisTechnique.ChartPadVisible
'        Public Property ChartPadLocation As Point Implements IChartPadAnalysisTechnique.ChartPadLocation
'            Get
'                Return New Point(ChartPad.Left, ChartPad.Top)
'            End Get
'            Set(value As Point)
'                ChartPad.Left = value.X
'                ChartPad.Top = value.Y
'            End Set
'        End Property
'        Public Property ChartPad As UIChartControl Implements IChartPadAnalysisTechnique.ChartPad

'        <Input> Public Property StepUpCount As Integer = 2
'        <Input> Public Property BaseTrendRV As Double = 2
'        <Input> Public Property TrendRVMultiplier As Double = 2
'        <Input> Public Property SwingRV As Double = 2
'        <Input> Public Property SwingRVMultiplier As Double = 2

'        <Input> Property SwingColor As Color = Colors.Gray

'        <Input> Property BCTextColor As Color = Colors.Gray
'        <Input> Property ConfirmedTrendUpColor As Color = Colors.Green
'        <Input> Property ConfirmedTrendDownColor As Color = Colors.Red
'        <Input> Property PotentialTrendUpColor As Color = Colors.SaddleBrown
'        <Input> Property PotentialTrendDownColor As Color = Colors.Blue
'        <Input> Property UpBarColor As Color = Colors.Green
'        <Input> Property NeutralBarColor As Color = Colors.Gray
'        <Input> Property DownBarColor As Color = Colors.Red
'        <Input> Property UpFillColor1 As Color = Colors.Red
'        <Input> Property DownFillColor1 As Color = Colors.Red
'        <Input> Property RangeBoxOn As Boolean = True
'        <Input> Property NumberOfBoxesToUseAsAverage As Integer = 5
'        <Input> Property ProjectionLinesColor As Color = Colors.Blue
'        <Input> Property RangeBoxUpColor As Color = Color.FromArgb(20, 0, 255, 0)
'        <Input> Property RangeBoxDownColor As Color = Color.FromArgb(20, 255, 0, 0)
'        <Input> Property RangeBoxLineThickness As Decimal = 0.2
'        <Input> Property ProjectionLineThickness As Decimal = 0.2
'        '<Input> Property SwingLineThickness As Decimal = 1
'        '<Input> Property ConfirmedSwingRegressionLineThickness As Decimal = 1
'        '<Input> Property ConfirmedSwingChannelLineThickness As Decimal = 1
'        '<Input> Property PotentialSwingRegressionLineThickness As Decimal = 1
'        '<Input> Property PotentialSwingChannelLineThickness As Decimal = 1
'        '<Input> Property HistorySwingChannelThickness As Decimal = 1
'        <Input> Property TrendLineThickness As Decimal = 1
'        <Input> Property ConfirmedTrendRegressionLineThicknessHLite As Decimal = 1
'        <Input> Property ConfirmedTrendChannelLineThicknessHLite As Decimal = 1
'        <Input> Property PotentialTrendRegressionLineThicknessHLite As Decimal = 1
'        <Input> Property PotentialTrendChannelLineThicknessHLite As Decimal = 1
'        '<Input> Property TrendLevelHLite As Integer
'        <Input> Property ConfirmedTrendRegressionLineThickness As Decimal = 1
'        <Input> Property ConfirmedTrendChannelLineThickness As Decimal = 1
'        <Input> Property PotentialTrendRegressionLineThickness As Decimal = 1
'        <Input> Property PotentialTrendChannelLineThickness As Decimal = 1

'        '<Input> Property SwingLengthTextFontSize As Decimal = 11
'        '<Input> Property SwingLengthTextFontWeight As FontWeight = FontWeights.Bold
'        <Input> Property TrendLengthTextFontSize As Decimal = 11
'        <Input> Property TrendLengthTextFontWeight As FontWeight = FontWeights.Bold
'        <Input> Property StepUpFontSizeIncrement As Decimal = 1.5

'        '<Input> Property PotentialBCLineThickness As Decimal = 1.5
'        '<Input> Property ConfirmedBCLineThickness As Decimal = 1.5

'        '<Input> Property BaseSwingLineThickness As Decimal = 1
'        '<Input> Property LastLineThickness As Decimal = 1
'        '<Input> Property CenterLineThickness As Decimal = 1
'        '<Input> Property BaseLengthTextFontSize As Decimal = 11
'        '<Input> Property BaseLengthTextFontWeight As FontWeight = FontWeights.Bold
'        '<Input> Property LengthTextFontWeight As FontWeight = FontWeights.Bold

'        <Input> Property BCTextFontSize As Decimal = 11
'        <Input> Property BCTextFontWeight As FontWeight = FontWeights.Bold
'        <Input> Property BCTextSpacing As Decimal = 10
'        <Input> Property BaseTargetTextFontSize As Decimal = 10
'        <Input> Property BaseTargetTextFontWeight As FontWeight = FontWeights.Bold
'        <Input> Property BaseTargetTextFontLineLength As Integer = 10
'        <Input> Property TrendTargetTextFontSize As Decimal = 10
'        <Input> Property TrendTargetTextFontWeight As FontWeight = FontWeights.Bold
'        <Input> Property TrendTargetTextLineLength As Integer = 10
'        '<Input> Property SwingTargetTextFontSize As Decimal = 11
'        '<Input> Property SwingTargetTextFontWeight As FontWeight = FontWeights.Bold
'        '<Input> Property SwingTargetTextLineLength As Integer = 12
'        '
'        '<Input> Property TargetTextFontSize As Decimal = 11
'        '<Input> Property TargetTextFontWeight As FontWeight = FontWeights.Bold
'        '<Input> Property RVTargetTextLineLength As Integer = 6
'        '<Input> Property ExtendTargetTextLineLength As Integer = 6
'        Property TextSpacing As Decimal = 0
'        '<Input> Property SwingHistoryOn As Boolean = False
'        '<Input> Property BarColoringOff As Boolean = False
'        Property LastRegressionFillToggle As Boolean = True
'        '<Input> Property TrendChannelsOn As Boolean = True
'        '<Input> Property SwingChannelsOn As Boolean = True
'        '<Input> Property RegressionMode As Boolean = True
'        '<Input> Property ExtendLastSwing As Boolean = True


'        Protected Overrides Sub Begin()
'            MyBase.Begin()

'            ReDim lastTrends(StepUpCount)
'            ReDim fillUpColors(StepUpCount)
'            ReDim fillDownColors(StepUpCount)

'            For i = 0 To StepUpCount
'                If i = 0 Then
'                    fillUpColors(i) = UpFillColor1
'                    fillDownColors(i) = DownFillColor1
'                ElseIf i = 1 Then
'                    fillUpColors(i) = UpFillColor1
'                    fillDownColors(i) = DownFillColor1
'                Else
'                    fillUpColors(i) = UpFillColor1
'                    fillDownColors(i) = DownFillColor1
'                End If

'                lastTrends(i) = New Swing(NewTrendLine(Colors.Gray, New Point(CurrentBar.Number, CurrentBar.Close), New Point(CurrentBar.Number, CurrentBar.Close), False), False)
'                lastTrends(i).LengthText = CreateTrendText(New Point(0, 0), Direction.Neutral, 0, Colors.Gray)
'                lastTrends(i).NeutralMaxPoint = lastTrends(i).EndPoint
'                lastTrends(i).NeutralMinPoint = lastTrends(i).EndPoint
'                lastTrends(i).PotentialTL = CreateFillTrendLine(Colors.Gray, Colors.Gray)
'                lastTrends(i).PotentialTL.IsRegressionLine = True
'                lastTrends(i).PotentialTL.UseFixedStartRegressionFormula = False
'                lastTrends(i).PotentialTL.HasParallel = True
'                lastTrends(i).PotentialTL.ExtendRight = True
'                'lastSwings(i).PotentialTL = CreateFillTrendLine(SwingColor, Colors.Transparent)
'                'lastSwings(i).PotentialTL.Pen.Thickness = PotentialRegressionLineThickness

'                'If i = 0 Then lastSwings(i).PotentialTL.ExtendRight = True

'                'lastTrends(i).Channel = NewTrendLine(Colors.Gray)
'                'With lastTrends(i).Channel
'                '    .Pen.Thickness = 0
'                '    .OuterPen.Brush = New SolidColorBrush(SwingColor)
'                '    .IsRegressionLine = True
'                '    .OuterPen.Thickness = 0
'                '    .HasParallel = True
'                '    .ExtendRight = True
'                'End With
'                lastTrends(i).Direction = Direction.Neutral
'                lastTrends(i).TargetText = NewLabel("── " & FormatNumberLengthAndPrefix(BaseTrendRV) & " RV ", SwingColor, New Point(0, 0), True, , False)
'                lastTrends(i).TargetText.Font.FontSize = TrendTargetTextFontSize
'                lastTrends(i).TargetText.Font.FontWeight = TrendTargetTextFontWeight
'                lastTrends(i).ExtendTargetText = NewLabel(Strings.StrDup(TrendTargetTextLineLength, "─") & " Extend", SwingColor, New Point(0, 0), True, , False)
'                lastTrends(i).ExtendTargetText.Font.FontSize = TrendTargetTextFontSize
'                lastTrends(i).ExtendTargetText.Font.FontWeight = TrendTargetTextFontWeight

'                lastTrends(i).RegressionTL = CreateFillTrendLine(SwingColor, Colors.Gray)
'                lastTrends(i).RegressionTL.ExtendRight = True
'                lastTrends(i).RegressionTL.DrawZoneFill = False
'                'lastTrends(i).RegressionTLOuterLines = NewTrendLine(Colors.Gray)
'                If i = StepUpCount Then
'                    lastTrends(i).RegressionTL.Pen.Thickness = ConfirmedTrendRegressionLineThicknessHLite
'                    lastTrends(i).RegressionTL.OuterPen.Thickness = ConfirmedTrendChannelLineThicknessHLite
'                Else
'                    lastTrends(i).RegressionTL.Pen.Thickness = ConfirmedTrendRegressionLineThickness
'                    lastTrends(i).RegressionTL.OuterPen.Thickness = ConfirmedTrendChannelLineThickness
'                End If
'                'lastTrends(i).RegressionTLOuterLines.ExtendRight = True
'                'lastTrends(i).RegressionTLOuterLines.HasParallel = True
'                'lastTrends(i).RegressionTLOuterLines.Pen.Thickness = 0
'                'lastTrends(i).RegressionTLOuterLines.OuterPen.Thickness = ChannelLineThickness

'                lastTrends(i).BCLine = NewTrendLine(SwingColor)
'                lastTrends(i).BCText = NewLabel("", SwingColor, New Point(0, 0), True, New Font With {.FontSize = BCTextFontSize, .FontWeight = BCTextFontWeight}, False)
'                lastTrends(i).BCText.HorizontalAlignment = LabelHorizontalAlignment.Right : lastTrends(i).BCText.IsEditable = True : lastTrends(i).BCText.IsSelectable = True
'                AddHandler lastTrends(i).BCText.MouseDown, Sub(sender As Object, location As Point)
'                                                               BaseTrendRV = Round(CDec(CType(sender, Label).Tag) + Chart.GetMinTick, 5)
'                                                               Chart.ReApplyAnalysisTechnique(Me)
'                                                           End Sub
'                AddHandler lastTrends(i).BCText.MiddleMouseDown, Sub(sender As Object, location As Point)
'                                                                     BaseTrendRV = Round(CDec(CType(sender, Label).Tag), 5)
'                                                                     Chart.ReApplyAnalysisTechnique(Me)
'                                                                 End Sub


'                'lastSwings(i).LengthText = CreateText(AddToY(lastSwings(i).EndPoint, Chart.GetRelativeFromRealHeight(TextSpacing)), lastSwings(i).Direction, Abs(lastSwings(i).EndPrice - lastSwings(i).StartPrice), If(lastSwings(i).Direction, SwingUpColor, SwingDownColor))
'                'lastSwings(i).LengthText.Text = ""
'            Next


'            'range box
'            boxes = New List(Of Box)
'            boxes.Add(New Box With {.Rect = NewRectangle(SwingColor, RangeBoxUpColor, New Point(1, CurrentBar.High), New Point(1, CurrentBar.Low), RangeBoxOn)})
'            CurrentBox.Rect.Pen.Thickness = RangeBoxLineThickness
'            trackBox = New Box With {.Rect = NewRectangle(Colors.Transparent, Colors.Red, New Point(1, CurrentBar.High), New Point(1, CurrentBar.Low), RangeBoxOn)}
'            trackBox.Rect.IsEditable = False
'            trackBox.Rect.IsSelectable = False
'            potentialBox = New Box With {.Rect = NewRectangle(SwingColor, Color.FromArgb(0, 0, 0, 0), New Point(1, CurrentBar.High), New Point(1, CurrentBar.Low), RangeBoxOn)}
'            potentialBox.Rect.LockPositionOrientation = ChartDrawingVisual.LockPositionOrientationTypes.Vertically
'            potentialBox.Rect.CanResize = False
'            potentialBox.Rect.ShowSelectionHandles = False
'            potentialBox.Rect.Pen.Thickness = RangeBoxLineThickness
'            potentialBox.Rect.BorderThickness = New Thickness(0, 0, 0, 0) 'New Thickness(1, 1, 0, 1)
'            potentialFillBox1 = NewRectangle(Colors.Transparent, Colors.Red, New Point(0, 0), New Point(0, 0), RangeBoxOn) : potentialFillBox1.IsEditable = False : potentialFillBox1.IsSelectable = False
'            potentialFillBox2 = NewRectangle(Colors.Transparent, Colors.Red, New Point(0, 0), New Point(0, 0), RangeBoxOn) : potentialFillBox2.IsEditable = False : potentialFillBox2.IsSelectable = False
'            potentialFillBox3 = NewRectangle(Colors.Transparent, Colors.Red, New Point(0, 0), New Point(0, 0), RangeBoxOn) : potentialFillBox3.IsEditable = False : potentialFillBox3.IsSelectable = False

'            CurrentBox.Rect.IsEditable = False
'            CurrentBox.Rect.IsSelectable = False
'            upperMaxRangeLine = NewTrendLine(ProjectionLinesColor, RangeBoxOn) : upperMaxRangeLine.Pen.Thickness = ProjectionLineThickness
'            verticalRangeLine = NewTrendLine(ProjectionLinesColor, RangeBoxOn) : verticalRangeLine.Pen.Thickness = ProjectionLineThickness
'            lowerMinRangeLine = NewTrendLine(ProjectionLinesColor, RangeBoxOn) : lowerMinRangeLine.Pen.Thickness = ProjectionLineThickness
'            upperMaxRangeLine.ExtendRight = True
'            lowerMinRangeLine.ExtendRight = True
'            upperActualRangeLine = NewTrendLine(SwingColor, RangeBoxOn) : upperActualRangeLine.Pen.Thickness = RangeBoxLineThickness
'            lowerActualRangeLine = NewTrendLine(SwingColor, RangeBoxOn) : lowerActualRangeLine.Pen.Thickness = RangeBoxLineThickness

'            AddHandler potentialBox.Rect.ManuallyMoved, AddressOf UpdateProjectionBoxColors
'        End Sub

'        Private Function CreateFillTrendLine(color As Color, fill As Color) As TrendLine
'            Dim t As TrendLine = NewTrendLine(color, New Point(CurrentBar.Number, CurrentBar.Close), New Point(CurrentBar.Number, CurrentBar.Close))
'            t.IsRegressionLine = True
'            t.UseFixedStartRegressionFormula = False
'            t.DrawZoneFill = True
'            t.UpZoneFillBrush = New SolidColorBrush(fill)
'            t.UpNeutralZoneFillBrush = New SolidColorBrush(fill)
'            t.DownNeutralZoneFillBrush = New SolidColorBrush(fill)
'            t.DownZoneFillBrush = New SolidColorBrush(fill)
'            t.ConfirmedZoneBarStart = 0
'            t.NeutralZoneBarStart = Integer.MaxValue
'            t.ExtendRight = False
'            Return t
'        End Function

'        Private Function CalculateTrend(ByRef trend As Swing, bar As BarData, rv As Decimal, index As Integer) As SwingEvent
'            Dim evnt As SwingEvent = SwingEvent.None

'            If trend.Direction = Direction.Neutral Then
'                If bar.Low < trend.NeutralMinPoint.Y Then trend.NeutralMinPoint = New Point(bar.Number, bar.Low)
'                If bar.High < trend.NeutralMaxPoint.Y Then trend.NeutralMaxPoint = New Point(bar.Number, bar.High)
'            End If

'            If (Round(bar.High - trend.NeutralMinPoint.Y, 5) >= Round(rv, 5) And trend.Direction = Direction.Neutral) Then
'                trend.PointList.Clear()
'                trend.PointList.Add(trend.NeutralMinPoint)
'                trend.TL.StartPoint = trend.NeutralMinPoint
'                trend.TL.EndPoint = trend.NeutralMinPoint
'                trend.Direction = Direction.Down
'            ElseIf (Round(trend.NeutralMaxPoint.Y - bar.Low, 5) >= Round(rv, 5) And trend.Direction = Direction.Neutral) Then
'                trend.PointList.Clear()
'                trend.PointList.Add(trend.NeutralMaxPoint)
'                trend.TL.StartPoint = trend.NeutralMaxPoint
'                trend.TL.EndPoint = trend.NeutralMaxPoint
'                trend.Direction = Direction.Up
'            End If

'            If ((Round(bar.High - trend.EndPrice, 5) >= Round(rv, 5) And trend.Direction = Direction.Down) Or
'                         (Round(trend.EndPrice - bar.Low, 5) >= Round(rv, 5) AndAlso trend.Direction = Direction.Up)) And bar.Number <> trend.EndBar Then
'                'new swing
'                trend.Direction = Not trend.Direction

'                'BarColorRoutine(trend.StartBar, trend.EndBar, NeutralBarColor)

'                'multiple lines

'                If Not TrendChannelsOn And index = 0 Then
'                    'AddObjectToChart(trend.TL)
'                    'trend.TL.HasParallel = False
'                    'trend.TL.RefreshVisual()
'                    'NewTrendLine(New Pen(Brushes.Red, 1), New Point(300, 62.38), New Point(400, 62.49), True)
'                    NewTrendLine(New Pen(New SolidColorBrush(SwingColor), TrendLineThickness), trend.TL.StartPoint, trend.TL.EndPoint, True)
'                    trend.TL = NewTrendLine(New Pen(New SolidColorBrush(SwingColor), TrendLineThickness), trend.TL.StartPoint, trend.TL.EndPoint, True)

'                End If
'                If index = 0 Then
'                    trend.TL.Pen.Brush = New SolidColorBrush(SwingColor)
'                    trend.TL.Pen.Thickness = TrendLineThickness
'                Else
'                    trend.TL.Pen.Thickness = 0
'                End If
'                trend.TL.Coordinates = New LineCoordinates(trend.EndPoint, New Point(bar.Number, If(trend.Direction, bar.High, bar.Low)))
'                'If index = 0 Then
'                '    swing.TL.ExtendRight = False
'                '    swing.TL.Pen.Thickness = BaseSwingLineThickness
'                '    swing.TL = NewTrendLine(Colors.Gray, swing.StartPoint, swing.EndPoint, True)
'                If index = 0 Then
'                    trend.LengthText = CreateTrendText(AddToY(trend.EndPoint, If(trend.Direction, 1, -1) * Chart.GetRelativeFromRealHeight(TextSpacing + TrendLengthTextFontSize * index + StepUpFontSizeIncrement * Max(index - 1, 0))), trend.Direction, Abs(trend.EndPrice - trend.StartPrice), If(trend.Direction, ConfirmedTrendUpColor, ConfirmedTrendDownColor))
'                    trend.LengthText.Font.FontSize = TrendLengthTextFontSize + index * StepUpFontSizeIncrement
'                    trend.LengthText.RefreshVisual()
'                    trend.RegressionTL.ExtendRight = True
'                    If StepUpCount = 0 Then
'                        trend.RegressionTL.ExtendRight = False
'                        trend.RegressionTL.DrawZoneFill = True
'                        Dim fill As Color = If(Not trend.Direction, UpFillColor1, DownFillColor1)
'                        trend.RegressionTL.UpZoneFillBrush = New SolidColorBrush(fill)
'                        trend.RegressionTL.UpNeutralZoneFillBrush = New SolidColorBrush(fill)
'                        trend.RegressionTL.DownNeutralZoneFillBrush = New SolidColorBrush(fill)
'                        trend.RegressionTL.DownZoneFillBrush = New SolidColorBrush(fill)
'                        trend.RegressionTL.ConfirmedZoneBarStart = 0
'                        trend.RegressionTL.NeutralZoneBarStart = Integer.MaxValue
'                        trend.RegressionTL.RefreshVisual()
'                        trend.RegressionTL = NewTrendLine(Colors.Gray)
'                        trend.RegressionTL.ExtendRight = True
'                        trend.RegressionTL.HasParallel = True
'                        trend.RegressionTL.IsRegressionLine = True
'                        trend.RegressionTL.UseFixedStartRegressionFormula = False
'                        trend.RegressionTL.Pen.Thickness = ConfirmedTrendRegressionLineThickness
'                        trend.RegressionTL.OuterPen.Thickness = ConfirmedTrendChannelLineThickness
'                    End If
'                Else
'                    Dim offset = TextSpacing + TrendLengthTextFontSize * index + StepUpFontSizeIncrement * Max(index - 1, 0)
'                    UpdateText(trend.LengthText, AddToY(trend.EndPoint, If(trend.Direction, 1, -1) * Chart.GetRelativeFromRealHeight(offset)), trend.Direction, Abs(trend.EndPrice - trend.StartPrice), If(trend.Direction, ConfirmedTrendUpColor, ConfirmedTrendDownColor))
'                End If
'                'End If
'                trend.StepUpIndex = index

'                trend.PointList.Add(trend.EndPoint)

'                BarColorRoutine(trend.StartBar, trend.EndBar, If(trend.Direction = Direction.Up, UpBarColor, DownBarColor))

'                'If ChannelsOn Then
'                '    trend.Channel.ExtendRight = True ' index = 0
'                '    trend.Channel.OuterPen.Thickness = ChannelLineThickness
'                '    trend.Channel.Pen.Thickness = 0
'                '    trend.Channel.OuterPen.Brush = New SolidColorBrush(If(trend.Direction = Direction.Up, TrendUpColor, TrendDownColor))
'                '    trend.Channel.Pen.Brush = Brushes.Transparent
'                '    trend.Channel.Coordinates = trend.TL.Coordinates
'                'Else
'                '    trend.Channel.OuterPen.Thickness = 0
'                'End If
'                evnt = SwingEvent.NewSwing

'                trend.IsActive = True

'            ElseIf (bar.High >= trend.EndPrice And trend.Direction = Direction.Up) Or
'                       (bar.Low <= trend.EndPrice And trend.Direction = Direction.Down) Then
'                ' extension
'                trend.EndBar = bar.Number
'                trend.EndPrice = If(trend.Direction = Direction.Up, bar.High, bar.Low)

'                'trend.Channel.Coordinates = trend.TL.Coordinates

'                trend.PointList(trend.PointList.Count - 1) = trend.EndPoint
'                'If index = 0 Then
'                Dim offset = TextSpacing + TrendLengthTextFontSize * index + StepUpFontSizeIncrement * Max(index - 1, 0)
'                UpdateText(trend.LengthText, AddToY(trend.EndPoint, If(trend.Direction, 1, -1) * Chart.GetRelativeFromRealHeight(offset)), trend.Direction, Abs(trend.EndPrice - trend.StartPrice), If(trend.Direction, ConfirmedTrendUpColor, ConfirmedTrendDownColor))
'                trend.LengthText.Font.FontSize = TrendLengthTextFontSize + index * StepUpFontSizeIncrement
'                If index <> 0 Then
'                    Dim a As New Object
'                End If
'                'End If

'                BarColorRoutine(trend.StartBar, trend.EndBar, If(trend.Direction = Direction.Up, UpBarColor, DownBarColor))
'                evnt = SwingEvent.Extension

'            End If

'            If Abs(trend.EndPrice - If(trend.Direction = Direction.Up, bar.Low, bar.High)) >= trend.BCText.Tag Then
'                'trend.BCText.Text = ""
'            End If


'            If evnt <> SwingEvent.None Or swingevnt Then
'                'Dim endBar As Integer = swing.EndBar
'                'Dim offset As Decimal = If(index = 0, 0, BaseLengthTextFontSize)
'                'Dim s = swing

'                'For Each s In lastSwings
'                '    If Abs(s.EndBar - endBar) <= 1 Then
'                '        UpdateText(s.LengthText, AddToY(s.EndPoint, If(s.Direction = Direction.Up, 1, -1) * Chart.GetRelativeFromRealHeight(TextSpacing + offset)),
'                '                           s.Direction, Abs(s.EndPrice - s.StartPrice), If(s.StepUpIndex = 0, SwingColor, If(s.Direction, SwingUpColor, SwingDownColor)))
'                '        s.LengthText.Font.FontSize = BaseLengthTextFontSize + StepUpFontSizeIncrement * s.StepUpIndex
'                '        offset += BaseLengthTextFontSize + StepUpFontSizeIncrement * s.StepUpIndex
'                '    End If
'                'Next

'                'trend.RegressionTLOuterLines.OuterPen.Brush = New SolidColorBrush(If(trend.Direction, TrendUpColor, TrendDownColor))
'                'trend.RegressionTLOuterLines.Coordinates = New LineCoordinates(trend.StartPoint, trend.EndPoint)

'                trend.RegressionTL.Pen.Brush = New SolidColorBrush(If(trend.Direction, ConfirmedTrendUpColor, ConfirmedTrendDownColor))
'                trend.RegressionTL.OuterPen.Brush = New SolidColorBrush(If(trend.Direction, ConfirmedTrendUpColor, ConfirmedTrendDownColor))


'                trend.RegressionTL.HasParallel = True
'                trend.RegressionTL.RefreshVisual()
'                If index = 0 Then
'                    Dim aPoint As Point = New Point(0, 0), bPoint As Point = New Point(0, If(trend.Direction = False, Decimal.MaxValue, Decimal.MinValue)), cPoint As Point = New Point(0, If(trend.Direction = Direction.Down, Decimal.MinValue, Decimal.MaxValue))
'                    Dim bcLength As Decimal
'                    Dim biggestC As Point
'                    Dim biggestB As Point
'                    For i = lastSwing.PointList.Count - 1 To 0 Step -1
'                        Dim p As Point = lastSwing.PointList(i)
'                        If p.X >= trend.StartBar Then
'                            If (trend.Direction = Direction.Down And p.Y > Round(cPoint.Y, 5)) Or
'                           (trend.Direction = Direction.Up And p.Y < Round(cPoint.Y, 5)) Then
'                                cPoint = p
'                                For j = i - 1 To 0 Step -1
'                                    Dim p2 As Point = lastSwing.PointList(j)
'                                    If p2.X >= trend.StartBar Then
'                                        If (trend.Direction = Direction.Down And p2.Y > Round(cPoint.Y, 5)) Or
'                                        (trend.Direction = Direction.Up And p2.Y < Round(cPoint.Y, 5)) Then
'                                            Exit For
'                                        Else
'                                            If (trend.Direction = Direction.Down And Round(Abs(p2.Y - cPoint.Y), 5) > Round(bcLength, 5)) Or
'                                           (trend.Direction = Direction.Up And Round(Abs(p2.Y - cPoint.Y), 5) > Round(bcLength, 5)) Then
'                                                biggestC = cPoint
'                                                biggestB = p2
'                                                bcLength = If(trend.Direction, Abs(p2.Y - cPoint.Y), Abs(p2.Y - cPoint.Y))
'                                            End If
'                                        End If
'                                    Else
'                                        Exit For
'                                    End If
'                                Next
'                            End If
'                        End If
'                    Next
'                    Dim isBCConfirmed As Boolean
'                    If trend.Direction = Direction.Up Then
'                        isBCConfirmed = biggestC.Y + bcLength < trend.EndPrice
'                    Else
'                        isBCConfirmed = biggestC.Y - bcLength > trend.EndPrice
'                    End If

'                    'trend.BCLine.Coordinates = New LineCoordinates(biggestB, biggestC)
'                    'RemoveObjectFromChart(trend.BCLine)
'                    'AddObjectToChart(trend.BCLine)
'                    'trend.BCLine.Pen = New Pen(New SolidColorBrush(If(isBCConfirmed, If(trend.Direction = Direction.Up, TrendDownColor, TrendUpColor), SwingColor)),
'                    'If (isBCConfirmed, ConfirmedBCLineThickness, PotentialBCLineThickness))
'                    trend.BCText.Location = AddToY(biggestC, If(trend.Direction = Direction.Up, -1, 1) * (Chart.GetRelativeFromRealHeight(BCTextSpacing + SwingLengthTextFontSize)))
'                    trend.BCText.Text = FormatNumberLengthAndPrefix(bcLength)
'                    trend.BCText.Tag = bcLength
'                    trend.BCText.VerticalAlignment = If(trend.Direction = Direction.Up, VerticalAlignment.Top, VerticalAlignment.Bottom)
'                    trend.BCText.Font.Brush = New SolidColorBrush(SwingColor)

'                End If
'            End If
'            Return evnt
'        End Function
'        Private Sub CalculateSwingNeutralBar(ByRef trend As Swing, bar As BarData, rv As Decimal, index As Integer)
'            trend.PotentialTL.Pen.Brush = New SolidColorBrush(If(trend.Direction = Direction.Up, PotentialTrendUpColor, PotentialTrendDownColor))
'            trend.PotentialTL.OuterPen.Brush = New SolidColorBrush(If(trend.Direction = Direction.Up, PotentialTrendUpColor, PotentialTrendDownColor))
'            trend.PotentialTL.ExtendRight = True
'            If index = StepUpCount Then
'                trend.PotentialTL.OuterPen.Thickness = PotentialTrendChannelLineThicknessHLite
'                trend.PotentialTL.Pen.Thickness = PotentialTrendRegressionLineThicknessHLite
'            Else
'                trend.PotentialTL.OuterPen.Thickness = PotentialTrendChannelLineThickness
'                trend.PotentialTL.Pen.Thickness = PotentialTrendRegressionLineThickness
'            End If

'            If index = StepUpCount Then
'                trend.PotentialTL.UpZoneFillBrush = New SolidColorBrush(If(trend.Direction, fillUpColors(index), fillDownColors(index)))
'                trend.PotentialTL.DownZoneFillBrush = New SolidColorBrush(If(trend.Direction, fillUpColors(index), fillDownColors(index)))
'            Else
'                trend.PotentialTL.UpZoneFillBrush = Brushes.Transparent
'                trend.PotentialTL.DownZoneFillBrush = Brushes.Transparent
'            End If
'            'If index = 0 Then
'            '    swing.PotentialTL.UpZoneFillBrush = New SolidColorBrush(If(LastRegressionFillToggle, If(swing.Direction, UpPotentialFillColor, DownPotentialFillColor), Colors.Transparent))
'            '    swing.PotentialTL.DownZoneFillBrush = New SolidColorBrush(If(LastRegressionFillToggle, If(swing.Direction, UpPotentialFillColor, DownPotentialFillColor), Colors.Transparent))
'            'Else
'            '    swing.PotentialTL.UpZoneFillBrush = New SolidColorBrush(Colors.Transparent)
'            '    swing.PotentialTL.DownZoneFillBrush = New SolidColorBrush(Colors.Transparent)
'            '    'swing.PotentialTL.HasParallel = False
'            'End If
'            'If LastRegressionFillToggle And ChannelsOn Then
'            '    swing.PotentialTL.HasParallel = True
'            '    swing.PotentialTL.Pen.Thickness = PotentialRegressionLineThickness
'            'Else
'            '    swing.PotentialTL.HasParallel = False
'            '    swing.PotentialTL.Pen.Thickness = 0
'            'End If
'            ''swing.PotentialT
'            trend.PotentialTL.Coordinates = New LineCoordinates(trend.StartPoint, New Point(bar.Number, bar.Close))

'            trend.RegressionTL.Coordinates = New LineCoordinates(trend.StartPoint, trend.EndPoint)
'            If index = 0 Then
'                trend.ExtendTargetText.Location = New Point(bar.Number, trend.EndPrice)
'                trend.ExtendTargetText.Font.Brush = New SolidColorBrush(If(Not trend.Direction, ConfirmedTrendDownColor, ConfirmedTrendUpColor))

'                trend.TargetText.Location = New Point(bar.Number, If(trend.Direction, trend.EndPrice - rv, trend.EndPrice + rv))
'                trend.TargetText.Text = Strings.StrDup(TrendTargetTextLineLength, "─") & " " & FormatNumberLengthAndPrefix(rv) & " RV  " ' & Round(targetText.Location.Y, Chart.Settings("DecimalPlaces").Value)
'                trend.TargetText.Font.Brush = New SolidColorBrush(If(Not trend.Direction, ConfirmedTrendDownColor, ConfirmedTrendUpColor))
'            End If
'        End Sub
'        Protected Overrides Sub Main()
'            Dim evnt As Boolean = CalculateTrend(lastTrends(0), CurrentBar, BaseTrendRV, 0) <> SwingEvent.None
'            CalculateSwingNeutralBar(lastTrends(0), CurrentBar, BaseTrendRV, 0)
'            If (Not loadingHistory) Or (loadingHistory And IsLastBarOnChart) Then
'                For j = 1 To StepUpCount
'                    If lastTrends(j - 1).IsActive Then
'                        Dim newRV As Decimal = CalculateNewStepUpRV(lastTrends(j - 1).PointList, lastTrends(j - 1).Direction) + Chart.GetMinTick
'                        If evnt Or (loadingHistory And IsLastBarOnChart) Then
'                            lastTrends(j).NeutralMaxPoint = New Point(1, Chart.bars(0).Data.Close)
'                            lastTrends(j).NeutralMinPoint = New Point(1, Chart.bars(0).Data.Close)
'                            lastTrends(j).Direction = Direction.Neutral
'                            For Each p In lastTrends(j - 1).PointList
'                                CalculateTrend(lastTrends(j), New BarData(p.Y, p.Y, p.Y, p.Y) With {.Number = p.X}, newRV, j)
'                            Next
'                        End If
'                        CalculateSwingNeutralBar(lastTrends(j), CurrentBar, newRV, j)
'                    End If

'                    ' delete a bad swing if it spans the chart
'                    If lastTrends(j).IsActive = False Then
'                        RemoveObjectFromChart(lastTrends(j).TL)
'                        RemoveObjectFromChart(lastTrends(j).RegressionTL)
'                        RemoveObjectFromChart(lastTrends(j).PotentialTL)
'                        RemoveObjectFromChart(lastTrends(j).LengthText)
'                        RemoveObjectFromChart(lastTrends(j).TargetText)
'                        RemoveObjectFromChart(lastTrends(j).BCLine)
'                        RemoveObjectFromChart(lastTrends(j).ExtendTargetText)
'                        RemoveObjectFromChart(lastTrends(j).Channel)
'                    End If
'                Next
'            End If


'            'rangebox
'            If CurrentBar.Low <= trackBox.Rect.Y - trackBox.Rect.Height Then
'                potentialBox.MovementDirection = False
'                trackBox.Rect.Fill = New SolidColorBrush(RangeBoxDownColor)
'                If CurrentBar.Low <= Round(trackBox.Rect.Y - BaseTrendRV, 5) Then
'                    CreateNewBox(False)
'                Else
'                    trackBox.Rect.Height = trackBox.Rect.Y - CurrentBar.Low
'                End If
'            ElseIf CurrentBar.High >= trackBox.Rect.Y Then
'                potentialBox.MovementDirection = True
'                trackBox.Rect.Fill = New SolidColorBrush(RangeBoxUpColor)
'                If CurrentBar.High >= Round(trackBox.Rect.Y - trackBox.Rect.Height + BaseTrendRV, 5) Then
'                    CreateNewBox(True)
'                Else
'                    trackBox.Rect.Height += CurrentBar.High - trackBox.Rect.Y
'                    trackBox.Rect.Y = CurrentBar.High
'                End If
'            End If
'            UpdateRangeLines()
'            'If potentialBox.Rect.Y < upperActualRangeLine.StartPoint.Y Or potentialBox.Rect.Y - potentialBox.Rect.Height > lowerActualRangeLine.StartPoint.Y Then
'            '    potentialBox.Rect.Coordinates = trackBox.Rect.Coordinates
'            '    potentialBox.Rect.Height = BaseTrendRV
'            'End If
'            If CurrentBar.Close > (trackBox.Rect.Coordinates.Top - trackBox.Rect.Coordinates.Height / 2) Then
'                potentialBox.Rect.Coordinates = New Rect(potentialBox.Rect.X, upperMaxRangeLine.StartPoint.Y, potentialBox.Rect.Width, potentialBox.Rect.Height)
'            Else
'                potentialBox.Rect.Coordinates = New Rect(potentialBox.Rect.X, lowerMinRangeLine.StartPoint.Y + potentialBox.Rect.Height, potentialBox.Rect.Width, potentialBox.Rect.Height)
'            End If
'            UpdateProjectionBoxColors(Nothing, Nothing, Nothing)
'        End Sub
'        Private Function CalculateNewStepUpRV(p As List(Of Point), dir As Direction) As Decimal
'            If p.Count > 2 Then
'                For i = p.Count - 2 To 0 Step -1
'                    If (p(i).Y > p(p.Count - 1).Y And dir = Direction.Up) Or
'                       (p(i).Y < p(p.Count - 1).Y And dir = Direction.Down) Then
'                        Return Abs(p(p.Count - 1).Y - p(p.Count - 2).Y)
'                    ElseIf (p(i).Y < p(p.Count - 2).Y And dir = Direction.Up) Or
'                           (p(i).Y > p(p.Count - 2).Y And dir = Direction.Down) Then
'                        Dim maxima As Decimal = If(dir = Direction.Up, 0, Decimal.MaxValue)
'                        For j = i To p.Count - 2
'                            If (p(j).Y > maxima And dir = Direction.Up) Or
'                               (p(j).Y < maxima And dir = Direction.Down) Then
'                                maxima = p(j).Y
'                            End If
'                        Next
'                        Return Abs(maxima - p(p.Count - 2).Y)
'                    End If
'                Next
'            End If
'            Return Abs(p(p.Count - 1).Y - p(p.Count - 2).Y)
'        End Function
'        Sub UpdateText(label As Label, position As Point, direction As Direction, value As String, color As Color)
'            If label IsNot Nothing Then
'                label.Text = FormatNumberLengthAndPrefix(value)
'                label.Tag = value
'                label.Location = AddToX(position, 0)
'                label.Font.Brush = New SolidColorBrush(color)
'                If direction = Direction.Up Then label.VerticalAlignment = LabelVerticalAlignment.Bottom Else label.VerticalAlignment = LabelVerticalAlignment.Top
'            End If
'        End Sub
'        Sub UpdateTrendText(label As Label, position As Point, direction As Direction, value As String, color As Color)
'            If label IsNot Nothing Then
'                label.Text = FormatNumberLengthAndPrefix(value)
'                label.Tag = value
'                label.Location = AddToX(position, 0)
'                label.Font.Brush = New SolidColorBrush(color)
'                If direction = Direction.Up Then label.VerticalAlignment = LabelVerticalAlignment.Bottom Else label.VerticalAlignment = LabelVerticalAlignment.Top
'            End If
'        End Sub
'        Function CreateText(position As Point, direction As Direction, value As String, color As Color) As Label
'            Dim lbl = NewLabel(FormatNumberLengthAndPrefix(value), color, New Point(0, 0),, New Font With {.FontSize = TrendTargetTextFontSize, .FontWeight = TrendTargetTextFontWeight}, False) : lbl.HorizontalAlignment = LabelHorizontalAlignment.Right : lbl.IsEditable = True : lbl.IsSelectable = True
'            lbl.Tag = value
'            lbl.Location = AddToX(position, 0)
'            AddHandler lbl.MouseDown, Sub(sender As Object, location As Point)
'                                          SwingRV = CDec(CType(sender, Label).Tag) + Chart.GetMinTick
'                                          Chart.ReApplyAnalysisTechnique(Me)
'                                      End Sub
'            AddHandler lbl.MiddleMouseDown, Sub(sender As Object, location As Point)
'                                                SwingRV = Round(CDec(CType(sender, Label).Tag), 5)
'                                                Chart.ReApplyAnalysisTechnique(Me)
'                                            End Sub
'            If direction = Direction.Up Then lbl.VerticalAlignment = LabelVerticalAlignment.Bottom Else lbl.VerticalAlignment = LabelVerticalAlignment.Top
'            Return lbl
'        End Function
'        Function CreateTrendText(position As Point, direction As Direction, value As String, color As Color) As Label
'            Dim lbl = NewLabel(FormatNumberLengthAndPrefix(value), color, New Point(0, 0),, New Font With {.FontSize = TrendLengthTextFontSize, .FontWeight = TrendLengthTextFontWeight}, False) : lbl.HorizontalAlignment = LabelHorizontalAlignment.Right : lbl.IsEditable = True : lbl.IsSelectable = True
'            lbl.Tag = value
'            lbl.Location = AddToX(position, 0)
'            AddHandler lbl.MouseDown, Sub(sender As Object, location As Point)
'                                          BaseTrendRV = CDec(CType(sender, Label).Tag) + Chart.GetMinTick
'                                          Chart.ReApplyAnalysisTechnique(Me)
'                                      End Sub
'            AddHandler lbl.MiddleMouseDown, Sub(sender As Object, location As Point)
'                                                BaseTrendRV = Round(CDec(CType(sender, Label).Tag), 5)
'                                                Chart.ReApplyAnalysisTechnique(Me)
'                                            End Sub
'            If direction = Direction.Up Then lbl.VerticalAlignment = LabelVerticalAlignment.Bottom Else lbl.VerticalAlignment = LabelVerticalAlignment.Top
'            Return lbl
'        End Function
'        Public Overrides Property Name As String = Me.GetType.Name
'        Function FormatNumberLengthAndPrefix(num As Decimal) As String
'            Return Round(num * (10 ^ Chart.Settings("DecimalPlaces").Value))
'        End Function
'        Public Overrides Sub Reset()

'            MyBase.Reset()
'            If Chart.bars.Count > 1 Then
'                For i = Max(0, 0) To Max(Chart.bars.Count - 1, 0)
'                    If Not TypeOf Chart.bars(i).Pen.Brush Is SolidColorBrush Then
'                        Chart.bars(i).Pen.Brush = New SolidColorBrush
'                    End If
'                    If CType(Chart.bars(i).Pen.Brush, SolidColorBrush).Color <> Chart.Settings("Bar Color").Value Then
'                        Chart.bars(i).Pen.Brush = New SolidColorBrush(Chart.Settings("Bar Color").Value)
'                        RefreshObject(Chart.bars(i))
'                    End If
'                Next
'                'BarColorRoutine(0, Chart.bars.Count - 1, Chart.Settings("Bar Color").Value)
'            End If
'            'Chart.dontDrawBarVisuals = False
'        End Sub
'        Protected Sub BarColorRoutine(ByVal startBar As Integer, ByVal endBar As Integer, ByVal color As Color)
'            'If BarColoringOff = False Then
'            If startBar <> 1 Then
'                For i = Max(startBar - 1, 0) To Max(endBar - 1, 0)
'                    If Not TypeOf Chart.bars(i).Pen.Brush Is SolidColorBrush Then
'                        Chart.bars(i).Pen.Brush = New SolidColorBrush
'                    End If
'                    If CType(Chart.bars(i).Pen.Brush, SolidColorBrush).Color <> color Then
'                        Chart.bars(i).Pen.Brush = New SolidColorBrush(color)
'                        RefreshObject(Chart.bars(i))
'                    End If
'                Next
'            End If
'            'End If
'        End Sub

'        Private Sub UpdateProjectionBoxColors(sender As Object, index As Integer, location As Point)
'            potentialFillBox1.Fill = Brushes.Transparent
'            potentialFillBox2.Fill = Brushes.Transparent
'            potentialFillBox3.Fill = Brushes.Transparent
'            Exit Sub
'            If potentialBox.Rect.Y - trackBox.Rect.Y > trackBox.Rect.Y - trackBox.Rect.Height - (potentialBox.Rect.Y - potentialBox.Rect.Height) Then
'                potentialFillBox1.Fill = New SolidColorBrush(RangeBoxUpColor)
'                potentialFillBox2.Fill = New SolidColorBrush(RangeBoxUpColor)
'                potentialFillBox3.Fill = New SolidColorBrush(RangeBoxUpColor)
'            Else
'                potentialFillBox1.Fill = New SolidColorBrush(RangeBoxDownColor)
'                potentialFillBox2.Fill = New SolidColorBrush(RangeBoxDownColor)
'                potentialFillBox3.Fill = New SolidColorBrush(RangeBoxDownColor)
'            End If
'            potentialFillBox1.Coordinates = New Rect(New Point(potentialBox.Rect.X, potentialBox.Rect.Y), New Size(potentialBox.Rect.Width, Max(potentialBox.Rect.Y - trackBox.Rect.Y, 0)))
'            potentialFillBox2.Coordinates = New Rect(New Point(trackBox.Rect.X + trackBox.Rect.Width, trackBox.Rect.Y), New Size(potentialBox.Rect.Width - trackBox.Rect.Width, trackBox.Rect.Height))
'            potentialFillBox3.Coordinates = New Rect(New Point(potentialBox.Rect.X, trackBox.Rect.Y - trackBox.Rect.Height), New Size(potentialBox.Rect.Width, Max(trackBox.Rect.Y - trackBox.Rect.Height - (potentialBox.Rect.Y - potentialBox.Rect.Height), 0)))
'        End Sub
'        Protected Overrides Sub NewBar()
'            trackBox.Rect.Width = CurrentBar.Number - trackBox.Rect.X
'            potentialBox.Rect.Width = Max(GetProjectedWidth(), trackBox.Rect.Width)
'            potentialBox.Rect.Y = Max(trackBox.Rect.Y, potentialBox.Rect.Y)
'            UpdateRangeLines()
'            UpdateProjectionBoxColors(Nothing, Nothing, Nothing)
'        End Sub
'        Private Sub UpdateRangeLines()
'            If CurrentBar.Number = CurrentBox.Rect.X + CurrentBox.Rect.Width Then
'                upperActualRangeLine.StartPoint = New Point(upperActualRangeLine.StartPoint.X, Max(If(CurrentBox.Direction, CurrentBar.High, CurrentBox.Rect.Y - CurrentBox.Rect.Height), upperActualRangeLine.StartPoint.Y))
'                upperActualRangeLine.EndPoint = New Point(trackBox.Rect.X + trackBox.Rect.Width, Max(upperActualRangeLine.StartPoint.Y, upperActualRangeLine.EndPoint.Y))
'                lowerActualRangeLine.StartPoint = New Point(lowerActualRangeLine.StartPoint.X, Min(If(Not CurrentBox.Direction, CurrentBar.Low, CurrentBox.Rect.Y), lowerActualRangeLine.StartPoint.Y))
'                lowerActualRangeLine.EndPoint = New Point(trackBox.Rect.X + trackBox.Rect.Width, Min(lowerActualRangeLine.StartPoint.Y, lowerActualRangeLine.EndPoint.Y))
'            Else
'                upperActualRangeLine.StartPoint = New Point(upperActualRangeLine.StartPoint.X, Max(CurrentBar.High, upperActualRangeLine.StartPoint.Y))
'                upperActualRangeLine.EndPoint = New Point(trackBox.Rect.X + trackBox.Rect.Width, Max(upperActualRangeLine.StartPoint.Y, upperActualRangeLine.EndPoint.Y))
'                lowerActualRangeLine.StartPoint = New Point(lowerActualRangeLine.StartPoint.X, Min(CurrentBar.Low, lowerActualRangeLine.StartPoint.Y))
'                lowerActualRangeLine.EndPoint = New Point(trackBox.Rect.X + trackBox.Rect.Width, Min(lowerActualRangeLine.StartPoint.Y, lowerActualRangeLine.EndPoint.Y))
'            End If
'            upperMaxRangeLine.StartPoint = New Point(upperMaxRangeLine.StartPoint.X, lowerActualRangeLine.StartPoint.Y + BaseTrendRV)
'            upperMaxRangeLine.EndPoint = New Point(trackBox.Rect.X + Max(trackBox.Rect.Width, 1), upperMaxRangeLine.StartPoint.Y)
'            lowerMinRangeLine.StartPoint = New Point(lowerMinRangeLine.StartPoint.X, upperActualRangeLine.StartPoint.Y - BaseTrendRV)
'            lowerMinRangeLine.EndPoint = New Point(trackBox.Rect.X + Max(trackBox.Rect.Width, 1), lowerMinRangeLine.StartPoint.Y)
'            verticalRangeLine.StartPoint = upperMaxRangeLine.StartPoint
'            verticalRangeLine.EndPoint = lowerMinRangeLine.StartPoint
'            potentialBox.Rect.MaxTopValue = lowerActualRangeLine.StartPoint.Y
'            potentialBox.Rect.MaxBottomValue = upperActualRangeLine.StartPoint.Y
'        End Sub
'        Private Sub CreateNewBox(direction As Boolean)

'            CurrentBox.Direction = direction
'            If direction Then
'                boxes.Add(New Box With {.Rect = NewRectangle(SwingColor, If(potentialBox.MovementDirection, RangeBoxUpColor, RangeBoxDownColor), New Point(trackBox.Rect.X, trackBox.Rect.Y - trackBox.Rect.Height + BaseTrendRV), New Point(CurrentBar.Number, trackBox.Rect.Y - trackBox.Rect.Height), RangeBoxOn)})
'            Else
'                boxes.Add(New Box With {.Rect = NewRectangle(SwingColor, If(potentialBox.MovementDirection, RangeBoxUpColor, RangeBoxDownColor), New Point(trackBox.Rect.X, trackBox.Rect.Y), New Point(CurrentBar.Number, trackBox.Rect.Y - BaseTrendRV), RangeBoxOn)})
'            End If
'            CurrentBox.Rect.IsEditable = False
'            CurrentBox.Rect.IsSelectable = False
'            CurrentBox.Rect.Pen.Thickness = RangeBoxLineThickness

'            trackBox.Rect.Y = CurrentBar.High
'            trackBox.Rect.X = CurrentBar.Number
'            trackBox.Rect.Width = 0
'            trackBox.Rect.Height = CurrentBar.High - CurrentBar.Low
'            upperActualRangeLine.Coordinates = New LineCoordinates(CurrentBar.Number, CurrentBox.Rect.Y - If(Not direction, BaseTrendRV, 0), CurrentBar.Number + 5, CurrentBox.Rect.Y - If(Not direction, BaseTrendRV, 0))
'            lowerActualRangeLine.Coordinates = New LineCoordinates(CurrentBar.Number, CurrentBox.Rect.Y - If(Not direction, BaseTrendRV, 0), CurrentBar.Number + 5, CurrentBox.Rect.Y - If(Not direction, BaseTrendRV, 0))
'            upperMaxRangeLine.Coordinates = New LineCoordinates(CurrentBar.Number, CurrentBox.Rect.Y, CurrentBar.Number + 1, CurrentBox.Rect.Y)
'            lowerMinRangeLine.Coordinates = New LineCoordinates(CurrentBar.Number, CurrentBox.Rect.Y, CurrentBar.Number + 1, CurrentBox.Rect.Y)
'            potentialBox.Rect.Coordinates = trackBox.Rect.Coordinates
'            potentialBox.Rect.Height = BaseTrendRV
'        End Sub
'        Private Function GetProjectedWidth() As Integer
'            If boxes.Count > 1 Then
'                Dim avgWidthSum As Integer
'                Dim frontBoxPriceRatio As Decimal = Max(Abs(upperActualRangeLine.StartPoint.Y - CurrentBar.Close), Abs(lowerActualRangeLine.StartPoint.Y - CurrentBar.Close)) / BaseTrendRV
'                Dim barIndex As Integer = CurrentBar.Number - trackBox.Rect.X
'                Dim percentagesSum As Decimal
'                For i = boxes.Count - 1 To Max(0, boxes.Count - NumberOfBoxesToUseAsAverage) Step -1
'                    Dim minVal As Decimal = Decimal.MaxValue, maxVal As Decimal = 0
'                    'For j = boxes(i).Rect.X To boxes(i).Rect.X + boxes(i).Rect.Width + 1
'                    '    minVal = Min(minVal, Chart.bars(j - 1).Data.Low)
'                    '    maxVal = Max(maxVal, Chart.bars(j - 1).Data.High)
'                    '    If (maxVal - minVal) / BaseTrendRV >= frontBoxPriceRatio Then
'                    '        If boxes(i).Rect.Width <> 0 Then
'                    '            percentagesSum += (j - boxes(i).Rect.X + 1) / (boxes(i).Rect.Width + 1)
'                    '        Else
'                    '            percentagesSum += 1
'                    '        End If
'                    '        Exit For
'                    '    End If
'                    'Next
'                    avgWidthSum += boxes(i).Rect.Width
'                Next
'                Dim avg = avgWidthSum / NumberOfBoxesToUseAsAverage
'                Dim interp = (CurrentBar.Number - trackBox.Rect.X) / (percentagesSum / NumberOfBoxesToUseAsAverage)
'                If CurrentBar.Number > Chart.bars.Count - 400 Then
'                    'Log(boxes.Count)
'                    'Log(avg)
'                    'Log(Round(frontBoxPriceRatio, 2) & ", " & Round(frontBoxPriceRatio / (percentagesSum / NumberOfBoxesToUseAsAverage), 2))
'                End If
'                If True Then ' enableinterpolation
'                    Return 20 * Exp(-3.37 * frontBoxPriceRatio) * (CurrentBar.Number - trackBox.Rect.X)
'                Else
'                    Return avg
'                End If

'                'If interp > avg Then
'                '    Return CInt(RecipCalc(interp, avg, ProjectionInterpolationValue, 1 - frontBoxPriceRatio))
'                'Else
'                '    Return CInt(interp)
'                'End If
'                'Else
'                ' Return CInt(avgWidthSum / NumberOfBoxesToUseAsAverage)
'                'End If
'            Else
'                Return 0
'            End If
'        End Function

'#Region "AutoTrendPad"

'        Dim hilite1 As Button
'        Dim hilite2 As Button
'        Dim hilite3 As Button
'        Dim hilite4 As Button

'        Dim addRVMinMoveSwing As Button
'        Dim subtractRVMinMoveSwing As Button
'        Dim rvBaseValueSwing As Button
'        Dim currentRVPopupBtnSwing As Button

'        Dim addStepUpMinMove As Button
'        Dim subtractStepUpMinMove As Button
'        Dim stepUpBaseValue As Button
'        Dim currentStepUpPopupBtn As Button

'        Dim rBtn As Button


'        Dim addRVMinMove As Button
'        Dim subtractRVMinMove As Button
'        Dim rvBaseValue As Button
'        Dim currentRVPopupBtn As Button


'        Dim grabArea As Border
'        Dim currentRBPopupBtn As Button
'        Dim addRBMinMove As Button
'        Dim subtractRBMinMove As Button

'        Dim stepUpPopup As Popup
'        Dim stepUpPopupGrid As Grid
'        Dim rvPopup As Popup
'        Dim rvPopupGrid As Grid
'        Dim rvPopupSwing As Popup
'        Dim rvPopupGridSwing As Grid

'        Dim popupRB As Popup
'        Dim popupRBGrid As Grid
'        Dim bd As Border
'        Dim grd As Grid
'        Private Sub InitGrid()
'            grd = New Grid
'            bd = New Border
'            grd.Margin = New Thickness(0)
'            grd.HorizontalAlignment = Windows.HorizontalAlignment.Center
'            For i = 1 To 2
'                grd.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Auto)})
'            Next
'            For i = 1 To 26
'                grd.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Auto)})
'            Next

'            grd.Background = Chart.Background
'            grd.Resources.MergedDictionaries.Add(New ResourceDictionary)
'            grd.Resources.MergedDictionaries(0).Source = New Uri("Themes/OrderButtonStyle.xaml", UriKind.Relative) 'AutoTrendPadButtonStyle
'            bd.Child = grd
'            bd.BorderBrush = Brushes.Transparent ' New SolidColorBrush(Color.FromArgb(40, 255, 255, 255))
'            bd.BorderThickness = New Thickness(0)
'            bd.Background = Chart.Background
'            bd.CornerRadius = New CornerRadius(0)
'            bd.HorizontalAlignment = Windows.HorizontalAlignment.Center

'            Dim c As New ContextMenu
'            AddHandler c.Opened, Sub() c.IsOpen = False
'            bd.ContextMenu = c
'            grabArea = New Border With {.BorderThickness = New Thickness(0), .BorderBrush = Brushes.Gray, .Background = Brushes.DarkBlue, .Margin = New Thickness(0.5)}
'            grabArea.SetValue(Grid.RowSpanProperty, 2)
'            'grabArea.Height = 17
'            grd.Children.Add(grabArea)
'        End Sub
'        Sub SetRows()

'            Grid.SetRow(hilite1, 0)
'            Grid.SetRow(hilite2, 0)
'            Grid.SetRow(hilite3, 1)
'            Grid.SetRow(hilite4, 1)
'            Grid.SetColumn(hilite1, 0)
'            Grid.SetColumn(hilite2, 1)
'            Grid.SetColumn(hilite3, 0)
'            Grid.SetColumn(hilite4, 1)


'            Grid.SetRow(addStepUpMinMove, 2)
'            Grid.SetRow(subtractStepUpMinMove, 3)
'            Grid.SetRow(stepUpBaseValue, 4)
'            Grid.SetRow(currentStepUpPopupBtn, 5)
'            Grid.SetColumn(addStepUpMinMove, 1)
'            Grid.SetColumn(subtractStepUpMinMove, 1)
'            Grid.SetColumn(stepUpBaseValue, 1)
'            Grid.SetColumn(currentStepUpPopupBtn, 1)

'            'swing
'            Grid.SetRow(addRVMinMoveSwing, 8)
'            Grid.SetRow(subtractRVMinMoveSwing, 9)
'            Grid.SetRow(rvBaseValueSwing, 10)
'            Grid.SetRow(currentRVPopupBtnSwing, 11)

'            Grid.SetRow(rBtn, 6)
'            Grid.SetColumn(rBtn, 1)
'            Grid.SetColumn(grabArea, 1)
'            Grid.SetRow(grabArea, 7)

'            Grid.SetRow(currentRBPopupBtn, 9)
'            Grid.SetRow(addRBMinMove, 10)
'            Grid.SetRow(subtractRBMinMove, 11)

'            Grid.SetColumn(currentRBPopupBtn, 1)
'            Grid.SetColumn(addRBMinMove, 1)
'            Grid.SetColumn(subtractRBMinMove, 1)


'            Grid.SetRow(addRVMinMove, 2)
'            Grid.SetRow(subtractRVMinMove, 3)
'            Grid.SetRow(rvBaseValue, 4)
'            Grid.SetRow(currentRVPopupBtn, 5)


'            grd.Children.Add(addStepUpMinMove)
'            grd.Children.Add(subtractStepUpMinMove)
'            grd.Children.Add(stepUpBaseValue)
'            grd.Children.Add(currentStepUpPopupBtn)

'            grd.Children.Add(addRVMinMoveSwing)
'            grd.Children.Add(subtractRVMinMoveSwing)
'            grd.Children.Add(rvBaseValueSwing)
'            grd.Children.Add(currentRVPopupBtnSwing)

'            grd.Children.Add(addRVMinMove)
'            grd.Children.Add(subtractRVMinMove)
'            grd.Children.Add(rvBaseValue)
'            grd.Children.Add(currentRVPopupBtn)

'            grd.Children.Add(currentRBPopupBtn)
'            grd.Children.Add(addRBMinMove)
'            grd.Children.Add(subtractRBMinMove)
'            grd.Children.Add(rBtn)
'            grd.Children.Add(hilite1)
'            grd.Children.Add(hilite2)
'            grd.Children.Add(hilite3)
'            grd.Children.Add(hilite4)
'        End Sub
'        Sub InitControls()
'            Dim fontsize As Integer = 16
'            addStepUpMinMove = New Button With {.Background = Brushes.LightGray, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "+1"}
'            subtractStepUpMinMove = New Button With {.Background = Brushes.LightGray, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "-1"}
'            stepUpBaseValue = New Button With {.Background = New SolidColorBrush(Color.FromArgb(255, 230, 235, 255)), .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "-1"}
'            rBtn = New Button With {.Background = Brushes.Lime, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "R", .FontWeight = FontWeights.Bold}

'            currentStepUpPopupBtn = New Button With {.Background = Brushes.Cyan, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19}
'            addRVMinMove = New Button With {.Background = Brushes.LightGray, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "+1"}
'            subtractRVMinMove = New Button With {.Background = Brushes.LightGray, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "-1"}
'            rvBaseValue = New Button With {.Background = New SolidColorBrush(Color.FromArgb(255, 230, 235, 255)), .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "-1"}
'            currentRVPopupBtn = New Button With {.Background = Brushes.Cyan, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19}

'            addRVMinMoveSwing = New Button With {.Background = Brushes.LightGray, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "+1"}
'            subtractRVMinMoveSwing = New Button With {.Background = Brushes.LightGray, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "-1"}
'            rvBaseValueSwing = New Button With {.Background = New SolidColorBrush(Color.FromArgb(255, 230, 235, 255)), .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "-1"}
'            currentRVPopupBtnSwing = New Button With {.Background = Brushes.Cyan, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19}

'            hilite1 = New Button With {.Background = Brushes.White, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "1"}
'            hilite2 = New Button With {.Background = Brushes.White, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "2"}
'            hilite3 = New Button With {.Background = Brushes.White, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "3"}
'            hilite4 = New Button With {.Background = Brushes.White, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "4"}
'            Dim hilite = Sub(sender As Object, e As EventArgs)
'                             hilite1.Background = Brushes.White
'                             hilite2.Background = Brushes.White
'                             hilite3.Background = Brushes.White
'                             hilite4.Background = Brushes.White
'                             CType(sender, Button).Background = Brushes.Orange
'                             If StepUpCount <> CInt(CType(sender, Button).Content) - 1 Then
'                                 StepUpCount = CInt(CType(sender, Button).Content) - 1
'                                 Chart.ReApplyAnalysisTechnique(Me)
'                             End If
'                         End Sub
'            AddHandler hilite1.Click, hilite
'            AddHandler hilite2.Click, hilite
'            AddHandler hilite3.Click, hilite
'            AddHandler hilite4.Click, hilite

'            stepUpPopup = New Popup With {.Placement = PlacementMode.Left, .PlacementTarget = currentStepUpPopupBtn, .Width = 208, .Height = 52, .StaysOpen = False}
'            stepUpPopupGrid = New Grid With {.Background = Brushes.White}
'            stepUpPopup.Child = stepUpPopupGrid
'            For x = 1 To 4 : stepUpPopupGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)}) : Next
'            For y = 1 To 2 : stepUpPopupGrid.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Star)}) : Next

'            rvPopup = New Popup With {.Placement = PlacementMode.Left, .PlacementTarget = currentRVPopupBtn, .Width = 520, .Height = 310, .StaysOpen = False}
'            rvPopupGrid = New Grid With {.Background = Brushes.White}
'            rvPopup.Child = rvPopupGrid
'            For x = 1 To 10 : rvPopupGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)}) : Next
'            For y = 1 To 12 : rvPopupGrid.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Star)}) : Next

'            rvPopupSwing = New Popup With {.Placement = PlacementMode.Left, .PlacementTarget = currentRVPopupBtnSwing, .Width = 520, .Height = 310, .StaysOpen = False}
'            rvPopupGridSwing = New Grid With {.Background = Brushes.White}
'            rvPopupSwing.Child = rvPopupGridSwing
'            For x = 1 To 10 : rvPopupGridSwing.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)}) : Next
'            For y = 1 To 12 : rvPopupGridSwing.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Star)}) : Next


'            currentRBPopupBtn = New Button With {.Background = Brushes.Red, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19}
'            addRBMinMove = New Button With {.Background = Brushes.LightGray, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "+1"}
'            subtractRBMinMove = New Button With {.Background = Brushes.LightGray, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "-1"}

'            popupRB = New Popup With {.Placement = PlacementMode.Left, .PlacementTarget = currentRBPopupBtn, .Width = 520, .Height = 310, .StaysOpen = False}
'            popupRBGrid = New Grid With {.Background = Brushes.White}
'            popupRB.Child = popupRBGrid
'            For x = 1 To 10 : popupRBGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)}) : Next
'            For y = 1 To 12 : popupRBGrid.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Star)}) : Next
'        End Sub
'        Sub AddHandlers()
'            AddHandler currentStepUpPopupBtn.Click,
'                Sub()
'                    stepUpPopup.IsOpen = True
'                End Sub

'            AddHandler currentRVPopupBtnSwing.Click,
'                Sub()
'                    rvPopupSwing.IsOpen = True
'                End Sub
'            AddHandler currentRVPopupBtn.Click,
'                Sub()
'                    rvPopup.IsOpen = True
'                End Sub
'            AddHandler stepUpPopup.Opened,
'              Sub()
'                  stepUpPopupGrid.Children.Clear()
'                  For x = 1 To 4
'                      For y = 1 To 2
'                          Dim value As Integer = x + (y - 1) * 4
'                          Dim btn As New Button With {.Margin = New Thickness(1), .Tag = value, .Content = value, .FontSize = 14.5, .Foreground = Brushes.Black}
'                          Grid.SetRow(btn, y - 1)
'                          Grid.SetColumn(btn, x - 1)
'                          btn.Background = Brushes.White
'                          If StepUpCount = value Then
'                              btn.Background = Brushes.LightBlue
'                          End If
'                          stepUpPopupGrid.Children.Add(btn)
'                          AddHandler btn.Click,
'                                      Sub(sender As Object, e As EventArgs)
'                                          stepUpPopup.IsOpen = False
'                                          If Round(CDec(CType(sender, Button).Tag)) <> StepUpCount Then
'                                              StepUpCount = CInt(CType(sender, Button).Tag) - 1
'                                              Chart.ReApplyAnalysisTechnique(Me)
'                                          End If
'                                      End Sub
'                      Next
'                  Next
'              End Sub

'            AddHandler rvPopup.Opened,
'              Sub()
'                  rvPopupGrid.Children.Clear()
'                  For x = 1 To 10
'                      For y = 1 To 12
'                          Dim value = Round(((y - 1) * 10 + (1 * Chart.Settings("RangeValue").Value) / Chart.GetMinTick() + x - 1) * Chart.GetMinTick(), 4)
'                          Dim btn As New Button With {.Margin = New Thickness(1), .Tag = value, .Content = FormatNumberLengthAndPrefix(value), .FontSize = 14.5, .Foreground = Brushes.Black}
'                          Grid.SetRow(btn, y - 1)
'                          Grid.SetColumn(btn, x - 1)
'                          btn.Background = Brushes.White
'                          If Round(BaseTrendRV, 4) = value Then
'                              btn.Background = Brushes.LightBlue
'                          End If
'                          If value = Round(RoundTo(Chart.Settings("RangeValue").Value * TrendRVMultiplier, Chart.GetMinTick), 4) Then
'                              btn.Background = New SolidColorBrush(Colors.Orange)
'                          End If
'                          rvPopupGrid.Children.Add(btn)
'                          AddHandler btn.Click,
'                                      Sub(sender As Object, e As EventArgs)
'                                          rvPopup.IsOpen = False
'                                          If Round(CDec(CType(sender, Button).Tag), 4) <> Round(BaseTrendRV, 4) Then
'                                              BaseTrendRV = CType(sender, Button).Tag
'                                              Chart.ReApplyAnalysisTechnique(Me)
'                                          End If
'                                      End Sub
'                      Next
'                  Next
'              End Sub

'            AddHandler rvPopupSwing.Opened,
'              Sub()
'                  rvPopupGridSwing.Children.Clear()
'                  For x = 1 To 10
'                      For y = 1 To 12
'                          Dim value = Round(((y - 1) * 10 + (1 * Chart.Settings("RangeValue").Value) / Chart.GetMinTick() + x - 1) * Chart.GetMinTick(), 4)
'                          Dim btn As New Button With {.Margin = New Thickness(1), .Tag = value, .Content = FormatNumberLengthAndPrefix(value), .FontSize = 14.5, .Foreground = Brushes.Black}
'                          Grid.SetRow(btn, y - 1)
'                          Grid.SetColumn(btn, x - 1)
'                          btn.Background = Brushes.White
'                          If Round(SwingRV, 4) = value Then
'                              btn.Background = Brushes.LightBlue
'                          End If
'                          If value = Round(RoundTo(Chart.Settings("RangeValue").Value * SwingRVMultiplier, Chart.GetMinTick), 4) Then
'                              btn.Background = New SolidColorBrush(Colors.Orange)
'                          End If
'                          rvPopupGridSwing.Children.Add(btn)
'                          AddHandler btn.Click,
'                                      Sub(sender As Object, e As EventArgs)
'                                          rvPopupSwing.IsOpen = False
'                                          If Round(CDec(CType(sender, Button).Tag), 4) <> Round(SwingRV, 4) Then
'                                              SwingRV = CType(sender, Button).Tag
'                                              Chart.ReApplyAnalysisTechnique(Me)
'                                          End If
'                                      End Sub
'                      Next
'                  Next
'              End Sub

'            AddHandler addStepUpMinMove.Click,
'                Sub()
'                    StepUpCount += 1
'                    Chart.ReApplyAnalysisTechnique(Me)
'                End Sub
'            AddHandler subtractStepUpMinMove.Click,
'                Sub()
'                    If StepUpCount > 0 Then StepUpCount -= 1
'                    Chart.ReApplyAnalysisTechnique(Me)
'                End Sub
'            AddHandler stepUpBaseValue.Click,
'                Sub(sender As Object, e As EventArgs)
'                    If CInt(sender.CommandParameter) <> StepUpCount Then
'                        StepUpCount = CInt(sender.CommandParameter) - 1
'                        Chart.ReApplyAnalysisTechnique(Me)
'                    End If
'                End Sub

'            AddHandler addRVMinMove.Click,
'                Sub()
'                    BaseTrendRV += Chart.GetMinTick()
'                    Chart.ReApplyAnalysisTechnique(Me)
'                End Sub
'            AddHandler subtractRVMinMove.Click,
'                Sub()
'                    BaseTrendRV -= Chart.GetMinTick()
'                    Chart.ReApplyAnalysisTechnique(Me)
'                End Sub
'            AddHandler rvBaseValue.Click,
'                Sub(sender As Object, e As EventArgs)
'                    If Round(sender.CommandParameter, 5) <> Round(BaseTrendRV, 5) Then
'                        BaseTrendRV = sender.CommandParameter
'                        Chart.ReApplyAnalysisTechnique(Me)
'                    End If
'                End Sub
'            AddHandler addRVMinMoveSwing.Click,
'               Sub()
'                   SwingRV += Chart.GetMinTick()
'                   Chart.ReApplyAnalysisTechnique(Me)
'               End Sub
'            AddHandler subtractRVMinMoveSwing.Click,
'                Sub()
'                    SwingRV -= Chart.GetMinTick()
'                    Chart.ReApplyAnalysisTechnique(Me)
'                End Sub
'            AddHandler rvBaseValueSwing.Click,
'                Sub(sender As Object, e As EventArgs)
'                    If Round(sender.CommandParameter, 5) <> Round(SwingRV, 5) Then
'                        SwingRV = sender.CommandParameter
'                        Chart.ReApplyAnalysisTechnique(Me)
'                    End If
'                End Sub

'            AddHandler rBtn.Click,
'                Sub(sender As Object, e As EventArgs)
'                    RangeBoxOn = Not RangeBoxOn
'                    Chart.ReApplyAnalysisTechnique(Me)
'                End Sub
'            AddHandler currentRBPopupBtn.Click,
'               Sub()
'                   popupRB.IsOpen = True
'               End Sub
'            AddHandler popupRB.Opened,
'                Sub()
'                    popupRBGrid.Children.Clear()
'                    For x = 1 To 10
'                        For y = 1 To 12
'                            Dim value = Round(((y - 1) * 10 + x) * Chart.GetMinTick(), 4)
'                            Dim btn As New Button With {.Margin = New Thickness(1), .Tag = value, .Content = FormatNumberLengthAndPrefix(value), .FontSize = 14.5, .Foreground = Brushes.Black}
'                            Grid.SetRow(btn, y - 1)
'                            Grid.SetColumn(btn, x - 1)
'                            btn.Background = Brushes.White
'                            If Round(Chart.Settings("RangeValue").Value, 4) = value Then
'                                btn.Background = Brushes.Pink
'                            ElseIf value = RoundTo(BaseTrendRV / 2, Chart.GetMinTick) Or value = RoundTo(BaseTrendRV / 3, Chart.GetMinTick) Or value = RoundTo(BaseTrendRV / 4, Chart.GetMinTick) Then
'                                btn.Background = New SolidColorBrush(Colors.Orange)
'                            End If
'                            popupRBGrid.Children.Add(btn)
'                            AddHandler btn.Click,
'                                    Sub(sender As Object, e As EventArgs)
'                                        popupRB.IsOpen = False
'                                        Chart.ChangeRangeWithDaysBackCalculating(CDec(sender.Tag))
'                                    End Sub
'                        Next
'                    Next
'                End Sub
'            AddHandler addRBMinMove.Click,
'                Sub()
'                    Chart.ChangeRange(Round(Chart.Settings("RangeValue").Value + Chart.GetMinTick(), 4))
'                End Sub
'            AddHandler subtractRBMinMove.Click,
'                Sub()
'                    Chart.ChangeRange(Round(Chart.Settings("RangeValue").Value - Chart.GetMinTick(), 4))
'                End Sub
'        End Sub
'        Public Sub InitChartPad() Implements IChartPadAnalysisTechnique.InitChartPad
'            ChartPad = New UIChartControl
'            ChartPad.Left = ChartPadLocation.X
'            ChartPad.Top = ChartPadLocation.Y
'            ChartPad.IsDraggable = True
'            InitGrid()
'            InitControls()
'            SetRows()
'            AddHandlers()
'            ChartPad.Content = bd
'        End Sub

'        Public Sub UpdateChartPad() Implements IChartPadAnalysisTechnique.UpdateChartPad
'            If addRVMinMove IsNot Nothing Then

'                addStepUpMinMove.Background = Brushes.LightGray
'                subtractStepUpMinMove.Background = Brushes.LightGray
'                rBtn.Background = New SolidColorBrush(If(RangeBoxOn, Colors.Lime, Colors.Gray))
'                'cpBtn.Content = If(LastRegressionFillToggle, "P", "C")

'                stepUpBaseValue.Content = 1
'                stepUpBaseValue.CommandParameter = 1
'                stepUpBaseValue.Background = Brushes.White
'                stepUpBaseValue.Foreground = Brushes.Black

'                addRVMinMove.Background = Brushes.LightGray
'                subtractRVMinMove.Background = Brushes.LightGray

'                rvBaseValue.Content = FormatNumberLengthAndPrefix(RoundTo(Chart.Settings("RangeValue").Value * TrendRVMultiplier, Chart.GetMinTick))
'                rvBaseValue.CommandParameter = Round(RoundTo(Chart.Settings("RangeValue").Value * TrendRVMultiplier, Chart.GetMinTick), Chart.Settings("DecimalPlaces").Value)
'                rvBaseValue.Background = Brushes.White

'                currentRVPopupBtn.Content = FormatNumberLengthAndPrefix(BaseTrendRV)
'                currentRVPopupBtn.Background = Brushes.LightBlue

'                addRVMinMoveSwing.Background = Brushes.LightGray
'                subtractRVMinMoveSwing.Background = Brushes.LightGray

'                rvBaseValueSwing.Content = FormatNumberLengthAndPrefix(RoundTo(Chart.Settings("RangeValue").Value * SwingRVMultiplier, Chart.GetMinTick))
'                rvBaseValueSwing.CommandParameter = Round(RoundTo(Chart.Settings("RangeValue").Value * SwingRVMultiplier, Chart.GetMinTick), Chart.Settings("DecimalPlaces").Value)
'                rvBaseValueSwing.Background = Brushes.White

'                currentRVPopupBtnSwing.Content = FormatNumberLengthAndPrefix(SwingRV)
'                currentRVPopupBtnSwing.Background = Brushes.LightBlue

'                currentStepUpPopupBtn.Content = StepUpCount + 1
'                currentStepUpPopupBtn.Background = Brushes.LightBlue
'                currentStepUpPopupBtn.Foreground = Brushes.Black


'                currentRBPopupBtn.Content = FormatNumberLengthAndPrefix(Chart.Settings("RangeValue").Value)
'                currentRBPopupBtn.Background = Brushes.Pink

'                hilite1.Background = Brushes.White
'                hilite2.Background = Brushes.White
'                hilite3.Background = Brushes.White
'                hilite4.Background = Brushes.White
'                If TrendLevelHLite = 0 Then hilite1.Background = Brushes.Orange
'                If TrendLevelHLite = 1 Then hilite2.Background = Brushes.Orange
'                If TrendLevelHLite = 2 Then hilite3.Background = Brushes.Orange
'                If TrendLevelHLite = 3 Then hilite4.Background = Brushes.Orange

'            End If
'        End Sub
'        Public Sub UpdateChartPadColor(color As Color) Implements IChartPadAnalysisTechnique.UpdateChartPadColor
'            If ChartPad.Content IsNot Nothing Then
'                If color.A = 255 Then
'                    ChartPad.Content.Child.Background = New SolidColorBrush(color)
'                Else
'                    ChartPad.Content.Child.Background = New SolidColorBrush(Color.FromArgb(255, color.R, color.G, color.B))
'                End If
'            End If
'        End Sub
'        Public Sub UpdateChartPadSize() Implements IChartPadAnalysisTechnique.UpdateChartPadSize
'            If ChartPad IsNot Nothing And ChartPad.Content IsNot Nothing Then
'                'ChartPad.Height = CType(CType(ChartPad.Content, Border).Child, Grid).ActualHeight ' If(CType(CType(ChartPad.Content, Border).Child, Grid).ActualHeight = 0, 200, CType(CType(ChartPad.Content, Border).Child, Grid).ActualHeight)
'                'ChartPad.Width = 200 'CType(CType(rangePad.Content, Border).Child, Grid).ActualWidth
'            End If
'        End Sub
'#End Region
'    End Class
'End Namespace


