﻿Imports System.Runtime.InteropServices
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

Public Class clsMemory
    <DllImport("KERNEL32.DLL", EntryPoint:="SetProcessWorkingSetSize", SetLastError:=True, CallingConvention:=CallingConvention.StdCall)>
    Friend Shared Function SetProcessWorkingSetSize(ByVal pProcess As IntPtr, ByVal dwMinimumWorkingSetSize As Integer, ByVal dwMaximumWorkingSetSize As Integer) As Boolean
    End Function
    <DllImport("KERNEL32.DLL", EntryPoint:="GetCurrentProcess", SetLastError:=True, CallingConvention:=CallingConvention.StdCall)>
    Friend Shared Function GetCurrentProcess() As IntPtr
    End Function
    Public Sub New()
        Dim pHandle As IntPtr = GetCurrentProcess()
        SetProcessWorkingSetSize(pHandle, -1, -1)
    End Sub
End Class





Namespace AnalysisTechniques

    Public Class AutoTrend
#Region "AnalysisTechnique Inherited Code"
        Inherits AutoTrendBase
        Implements IChartPadAnalysisTechnique
        Public Sub New(ByVal chart As Chart)
            MyBase.New(chart) ' Call the base class constructor.
            Description = "Two-swing AutoTrend analysis technique."
            Name = "AutoTrend"
            If chart IsNot Nothing Then AddHandler chart.ChartKeyDown, AddressOf KeyPress
        End Sub
#End Region





#Region "Inputs"
        Enum PriceRVMode
            PriceRVMode
            BarCountMode
        End Enum

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
        <Input(, "Mode Settings")> Public Property PriceVsBarMode As PriceRVMode
        <Input()> Public Overrides Property PriceRV As Decimal
            Get
                Return _rv
            End Get
            Set(value As Decimal)
                _rv = Round(value, 7)
            End Set
        End Property
        <Input()> Public Property BasePriceRVMultiplier As Decimal = 3
        <Input()> Public Property BarCountRV As Integer = 40
        <Input()> Public Property BaseBarCountRV As Integer = 40

        <Input(, "ABC")>
        Public Property AbcChannelAndTextColor As Color = (New ColorConverter).ConvertFrom("#FF008000")
        <Input()> Public Property AbcSwingLineThickness As Decimal = 0.7
        <Input()> Public Property AbcSwingChannelThickness As Decimal = 1
        <Input()> Public Property AbcLengthTextFontSize As Double = 11
        <Input()> Public Property AbcLengthTextFontWeight As FontWeight = FontWeights.Bold
        <Input()> Public Property AbcTargetTextFontSize As Double = 12
        <Input()> Public Property AbcTargetTextFontWeight As FontWeight = FontWeights.Bold


        Private _rv As Decimal = 2


        <Input(, "Trend")> Public Property TrendChannelThickness As Decimal = 0.7
        <Input()> Public Property TrendHistoryThickness As Decimal = 0.7
        <Input()> Public Property TrendHistoryAsChannel As Boolean = True
        <Input()> Public Property PotentialTrendChannel1Thickness As Decimal
        <Input()> Public Property PotentialTrendChannel2Thickness As Decimal
        <Input()> Public Property TrendChannelUpColor As Color = (New ColorConverter).ConvertFrom("#FF004C00")
        <Input()> Public Property TrendChannelDownColor As Color = (New ColorConverter).ConvertFrom("#FF7F0000")
        <Input()> Public Property PotentialTrendChannelUpColor As Color
        <Input()> Public Property PotentialTrendChannelDownColor As Color
        <Input()> Public Property TrendLengthTextFontSize As Double = 14
        <Input()> Public Property TrendLengthTextFontWeight As FontWeight = FontWeights.Bold

        <Input(, "Bar Coloring")>
        Public Property UpColor As Color = (New ColorConverter).ConvertFrom("#FF00B200")
        <Input()> Public Property NeutralColor As Color = (New ColorConverter).ConvertFrom("#FF000000")
        <Input()> Public Property DownColor As Color = (New ColorConverter).ConvertFrom("#FFB20000")
        <Input()> Public Property OnlyColorLastSwing As Boolean = True
        <Input("Enter 0 for all bars", "Misc")> Public Property BarProcessCount As Integer = 0
        <Input()> Public Property SetRVToLastSwingHotkey As Key = Key.S
        <Input> Public Property HotkeyBarDelay As Integer = 1
        <Input> Public Property EnableEzraLayout As Boolean





#End Region

        Private barColorsDirty As Boolean

        Private trends As List(Of Swing)

        Const TrendTextFontSize As Double = 14
        Private TrendTextFontWeight As FontWeight = FontWeights.Bold
        Const predictionTextPrefix As String = "── "
        Const rvTextPrefix2 As String = "────────── "
        Const newRVTextPrefix2 As String = "─────────── "
        Const rvTextPrefix1 As String = "── "
        Const newRVTextPrefix1 As String = "── "
        Const trendTextOffset As Integer = 15
        Const horizSwingTextAlignment As Double = 1
        Const vertSwingTextAlignment As Double = 0
        Private extendTrendText As Label
        Private retraceTrendText As Label
        Private lastExtendText As Label
        Private newTrendRVText As Label
        Private newSwingTextObject1 As Label
        Private newBarRVLine As TrendLine
        Private newBarRVText As Label


        Private ASwing As TrendLine
        Private BSwing As TrendLine
        Private CSwing As TrendLine
        Private PotentialBSwing As TrendLine
        Private PotentialASwing As TrendLine
        Private SwingChannel As TrendLine
        Private ALengthText As Label
        Private BLengthText As Label
        Private CLengthText As Label
        Private PotentialBSwingText As Label


        Private ABarCountText As Label
        Private BBarCountText As Label
        Private CBarCountText As Label
        Dim lastSwingPotentialLine2 As TrendLine
        Dim lastSwingCounterPotentialLine2 As TrendLine
        Dim lastSwingPotentialLine1 As TrendLine
        Dim lastSwingCounterPotentialLine1 As TrendLine
        Dim neutralBarsText As Label
        Dim tickMoveHistory As List(Of Integer)
        Dim lastTick As Double
        Dim energyLine As TrendLine
        Dim energyLineMarker As TrendLine
        Private timer As System.Windows.Forms.Timer
        'manual
        Private MASwing As TrendLine
        Private MBSwing As TrendLine
        Private MCSwing As TrendLine
        Private MPotentialBSwing As TrendLine
        Private MPotentialASwing As TrendLine
        Private MSwingChannel As TrendLine
        Private MALengthText As Label
        Private MBLengthText As Label
        Private MCLengthText As Label
        Private MPotentialBSwingText As Label
        Private MTrend As Swing
        Private lastSwing As TrendLine

        Private ReadOnly Property CurrentTrend As Swing
            Get
                Return trends(trends.Count - 1)
            End Get
        End Property


        Friend Overrides Sub OnCreate()
            MyBase.OnCreate()
            If Chart IsNot Nothing Then
                If CustomRangeValue = -1 Then
                    PriceRV = Chart.Settings("RangeValue").Value * DefaultAutoTrendRV
                Else
                    PriceRV = CustomRangeValue * DefaultAutoTrendRV
                End If
            End If
        End Sub
        Public Overrides Sub Reset()

            MyBase.Reset()
            If barColorsDirty And Chart.bars.Count > 1 Then
                BarColorRoutine(0, Chart.bars.Count - 1, Chart.Settings("Bar Color").Value)
                barColorsDirty = False
            End If
            'Chart.dontDrawBarVisuals = False
        End Sub

        Function FormatNumberLengthAndPrefix(num As Decimal, placesAfterDecimal As Integer) As String
            Return Round(num * (10 ^ placesAfterDecimal))
        End Function

        Protected Sub KeyPress(ByVal sender As Object, ByVal e As KeyEventArgs)
            Dim key As Key
            If e.SystemKey = Key.None Then
                key = e.Key
            Else
                key = e.SystemKey
            End If
            If Keyboard.Modifiers = ModifierKeys.None Then
                If key = SetRVToLastSwingHotkey Then
                    For Each c In Chart.Parent.Charts
                        For Each a In c.AnalysisTechniques
                            If TypeOf a.AnalysisTechnique Is AutoTrend And a.AnalysisTechnique.IsEnabled Then
                                If CType(a.AnalysisTechnique, AutoTrend).trends.Count > 0 Then
                                    If CType(a.AnalysisTechnique, AutoTrend).CurrentBar.Number <= CType(a.AnalysisTechnique, AutoTrend).CurrentTrend.EndBar + HotkeyBarDelay Then
                                        CType(a.AnalysisTechnique, AutoTrend).PriceRV = Abs(CType(a.AnalysisTechnique, AutoTrend).CurrentTrendEndPrice() - CType(a.AnalysisTechnique, AutoTrend).CurrentTrendStartPrice()) + c.GetMinTick
                                        c.ReApplyAnalysisTechnique(CType(a.AnalysisTechnique, AutoTrend))
                                    End If
                                End If
                            End If
                        Next
                    Next
                End If
            End If
        End Sub

        Protected Overrides Sub Begin()
            MyBase.Begin()
            trends = New List(Of Swing)
            trends.Add(New Swing(NewTrendLine(Colors.Red,
                      New Point(CurrentBar.Number, CurrentBar.Close),
                      New Point(CurrentBar.Number, CurrentBar.Close),
                      True), Direction.Down))
            curTrendRetracement = CurrentBar.Close
            CreateLengthTexts(CurrentTrend.LengthText)
            lastSwing = NewTrendLine(Colors.Gray, True) : lastSwing.IsEditable = False : lastSwing.IsSelectable = False : lastSwing.Pen.Thickness = TrendHistoryThickness
            ASwing = NewTrendLine(Colors.Gray, True) : ASwing.IsEditable = False : ASwing.IsSelectable = False : ASwing.Pen.Thickness = AbcSwingLineThickness
            BSwing = NewTrendLine(Colors.Gray, True) : BSwing.IsEditable = False : BSwing.IsSelectable = False : BSwing.Pen.Thickness = AbcSwingLineThickness
            CSwing = NewTrendLine(Colors.Gray, True) : CSwing.IsEditable = False : CSwing.IsSelectable = False : CSwing.Pen.Thickness = AbcSwingLineThickness
            PotentialASwing = NewTrendLine(Colors.Gray, True) : PotentialASwing.IsEditable = False : PotentialASwing.IsSelectable = False : PotentialASwing.Pen.Thickness = AbcSwingLineThickness
            PotentialBSwing = NewTrendLine(Colors.Gray, True) : PotentialBSwing.IsEditable = False : PotentialBSwing.IsSelectable = False : PotentialBSwing.Pen.Thickness = AbcSwingLineThickness
            SwingChannel = NewTrendLine(AbcChannelAndTextColor, True) : SwingChannel.IsEditable = False : SwingChannel.IsSelectable = False : SwingChannel.Pen.Thickness = 0 : SwingChannel.OuterPen.Thickness = AbcSwingChannelThickness : SwingChannel.IsRegressionLine = True : SwingChannel.ExtendRight = True : SwingChannel.HasParallel = True
            ALengthText = NewLabel("", AbcChannelAndTextColor, New Point(0, 0),, New Font With {.FontSize = AbcLengthTextFontSize, .FontWeight = AbcLengthTextFontWeight}, False) : ALengthText.HorizontalAlignment = LabelHorizontalAlignment.Right : ALengthText.IsEditable = True : ALengthText.IsSelectable = True
            BLengthText = NewLabel("", AbcChannelAndTextColor, New Point(0, 0),, New Font With {.FontSize = AbcLengthTextFontSize, .FontWeight = AbcLengthTextFontWeight}, False) : BLengthText.HorizontalAlignment = LabelHorizontalAlignment.Right : BLengthText.IsEditable = True : BLengthText.IsSelectable = True
            CLengthText = NewLabel("", AbcChannelAndTextColor, New Point(0, 0),, New Font With {.FontSize = AbcLengthTextFontSize, .FontWeight = AbcLengthTextFontWeight}, False) : CLengthText.HorizontalAlignment = LabelHorizontalAlignment.Right : CLengthText.IsEditable = True : CLengthText.IsSelectable = True
            PotentialBSwingText = NewLabel("", AbcChannelAndTextColor, New Point(0, 0),, New Font With {.FontSize = AbcLengthTextFontSize, .FontWeight = AbcLengthTextFontWeight}, False) : PotentialBSwingText.HorizontalAlignment = LabelHorizontalAlignment.Right : PotentialBSwingText.IsEditable = True : PotentialBSwingText.IsSelectable = True
            Dim s = Sub(sender As Object, location As Point)
                        If PriceVsBarMode = PriceRVMode.PriceRVMode Then
                            PriceRV = CDec(CType(sender, Label).Text / (10 ^ Chart.Settings("DecimalPlaces").Value))
                        ElseIf PriceVsBarMode = PriceRVMode.BarCountMode Then
                            BarCountRV = CDec(CType(sender, Label).Text / (10 ^ Chart.Settings("DecimalPlaces").Value))
                        End If
                        Chart.ReApplyAnalysisTechnique(Me)
                    End Sub
            AddHandler ALengthText.MouseDown, s
            AddHandler BLengthText.MouseDown, s
            AddHandler CLengthText.MouseDown, s
            AddHandler PotentialBSwingText.MouseDown, s
            ' potential line initialization
            lastSwingPotentialLine2 = NewTrendLine(Colors.Red, True)
            lastSwingCounterPotentialLine2 = NewTrendLine(Colors.Red, True)
            lastSwingPotentialLine1 = NewTrendLine(Colors.Red, True)
            lastSwingCounterPotentialLine1 = NewTrendLine(Colors.Red, True)
            Dim lines() = {lastSwingCounterPotentialLine1, lastSwingCounterPotentialLine2, lastSwingPotentialLine1, lastSwingPotentialLine2}
            For Each line In lines
                line.Pen = New Pen(New SolidColorBrush(Colors.Red), 0)
                line.IsRegressionLine = True : line.HasParallel = True : line.IsSelectable = False : line.IsEditable = False : line.ExtendRight = True
            Next
            lastSwingPotentialLine2.OuterPen = New Pen(New SolidColorBrush(Colors.Red), PotentialTrendChannel2Thickness)
            lastSwingCounterPotentialLine2.OuterPen = New Pen(New SolidColorBrush(Colors.Red), PotentialTrendChannel2Thickness)
            lastSwingPotentialLine1.OuterPen = New Pen(New SolidColorBrush(Colors.Red), PotentialTrendChannel1Thickness)
            lastSwingCounterPotentialLine1.OuterPen = New Pen(New SolidColorBrush(Colors.Red), PotentialTrendChannel1Thickness)
            ' end potential line initialization

            neutralBarsText = NewLabel("", AbcChannelAndTextColor, New Point(0, 0))
            neutralBarsText.HorizontalAlignment = LabelHorizontalAlignment.Left : neutralBarsText.VerticalAlignment = LabelHorizontalAlignment.Center : neutralBarsText.Font.FontSize = AbcLengthTextFontSize : neutralBarsText.Font.FontWeight = AbcLengthTextFontWeight : neutralBarsText.IsSelectable = False : neutralBarsText.IsEditable = False
            extendTrendText = NewLabel(rvTextPrefix2 & " Ext Trend", Colors.Black, New Point(0, 0), True, , False)
            extendTrendText.Font.FontSize = AbcTargetTextFontSize
            extendTrendText.Font.FontWeight = AbcTargetTextFontWeight

            retraceTrendText = NewLabel("", Colors.Black, New Point(0, 0), True, , False)
            retraceTrendText.Font.FontSize = AbcTargetTextFontSize
            retraceTrendText.Font.FontWeight = AbcTargetTextFontWeight

            lastExtendText = NewLabel("", Colors.Black, New Point(0, 0), True, , False)
            lastExtendText.Font.FontSize = AbcTargetTextFontSize
            lastExtendText.Font.FontWeight = AbcTargetTextFontWeight

            newTrendRVText = NewLabel(newRVTextPrefix2 & FormatNumberLengthAndPrefix(PriceRV, Chart.Settings("DecimalPlaces").Value) & " RV ", Colors.Gray, New Point(0, 0), True, , False)
            newTrendRVText.Font.FontSize = AbcTargetTextFontSize
            newTrendRVText.Font.FontWeight = AbcTargetTextFontWeight

            newSwingTextObject1 = NewLabel(newRVTextPrefix1 & FormatNumberLengthAndPrefix(0, Chart.Settings("DecimalPlaces").Value) & " RV ", AbcChannelAndTextColor, New Point(0, 0), True, , False)
            newSwingTextObject1.Font.FontSize = AbcTargetTextFontSize
            newSwingTextObject1.Font.FontWeight = AbcTargetTextFontWeight

            newBarRVLine = NewTrendLine(Colors.Gray)
            newBarRVLine.IsSelectable = False
            newBarRVLine.IsEditable = False
            newBarRVText = NewLabel("", Colors.Gray, New Point(0, 0), True,, False)
            newBarRVText.Font.FontSize = AbcLengthTextFontSize
            newBarRVText.Font.FontWeight = AbcLengthTextFontWeight
            curTrendEvent = SwingEvent.None

            'PriceRV = Chart.Settings("RangeValue").Value * ChartPadRVMultiplier

            tickMoveHistory = New List(Of Integer)
            lastTick = 0
            energyLine = NewTrendLine(Colors.Red, True)
            energyLineMarker = NewTrendLine(Colors.Red, True)

        End Sub

        Dim curTrendEvent As SwingEvent = SwingEvent.None
        Dim lastTrendExtension As Decimal
        Dim curTrendRetracement As Decimal
        Dim currentSwingRetracement As Decimal


        Protected Overrides Sub Main()
            If CurrentBar.Number <= Chart.bars.Count - BarProcessCount And BarProcessCount <> 0 Then Exit Sub
            If CurrentBar.Number < 2 Then Exit Sub

            curTrendEvent = SwingEvent.None
            Dim newSwingCondition As Boolean
            If PriceVsBarMode = PriceRVMode.BarCountMode Then
                newSwingCondition = CurrentBar.Number - CurrentTrend.EndBar >= BarCountRV And ((CurrentTrend.Direction = Direction.Down And CurrentBar.High >= curTrendRetracement) Or (CurrentTrend.Direction = Direction.Up And CurrentBar.Low <= curTrendRetracement))
            Else
                newSwingCondition =
                   ((CurrentTrend.Direction = Direction.Down And Round(CurrentBar.High, 5) >= Round(CurrentTrendEndPrice() + PriceRV, 5)) OrElse
                       (CurrentTrend.Direction = Direction.Up And Round(CurrentBar.Low, 5) <= Round(CurrentTrendEndPrice() - PriceRV, 5)))
            End If
            If newSwingCondition Then
                CurrentTrend.TL.OuterPen.Thickness = TrendHistoryThickness
                If (TrendHistoryAsChannel = False) Then
                    CurrentTrend.TL.IsRegressionLine = False
                    CurrentTrend.TL.HasParallel = False
                    CurrentTrend.TL.Pen.Thickness = TrendHistoryThickness
                    CurrentTrend.TL.Pen.Brush = CurrentTrend.TL.OuterPen.Brush
                    CurrentTrend.TL.RefreshVisual()
                End If
                If OnlyColorLastSwing Then
                    BarColorRoutine(trends(trends.Count - 1).StartBar - 1, trends(trends.Count - 1).EndBar - 1, NeutralColor)
                End If
                'new swing
                If TrendHistoryAsChannel = False Then
                    lastSwing.StartPoint = New Point(CurrentTrend.EndBar, CurrentTrendEndPrice)
                    lastSwing.EndPoint = New Point(CurrentBar.Number, If(CurrentTrend.Direction = Direction.Down, CurrentBar.High, CurrentBar.Low))
                    lastSwing.Pen.Brush = New SolidColorBrush(If(CurrentTrend.Direction = Direction.Down, TrendChannelUpColor, TrendChannelDownColor))
                    lastSwing.IsRegressionLine = False
                End If
                trends.Add(NewSwing(Colors.Pink, New Point(CurrentTrend.EndBar, CurrentTrendEndPrice),
                                        New Point(CurrentBar.Number, If(CurrentTrend.Direction = Direction.Down, CurrentBar.High, CurrentBar.Low)), True, Not CurrentTrend.Direction))
                CurrentTrend.TL.IsSelectable = False
                CurrentTrend.TL.IsEditable = False
                CurrentTrend.TL.IsRegressionLine = True
                CurrentTrend.TL.HasParallel = True
                CurrentTrend.TL.Pen = New Pen(Brushes.Black, 0)
                CurrentTrend.TL.OuterPen = New Pen(New SolidColorBrush(If(CurrentTrend.Direction = Direction.Up, TrendChannelUpColor, TrendChannelDownColor)), TrendChannelThickness)
                CurrentTrend.TL.ExtendRight = True

                If trends.Count > 1 Then trends(trends.Count - 2).TL.ExtendRight = False

                curTrendEvent = SwingEvent.NewSwing

                ' calculate swings 
                currentSwingRetracement = CurrentTrendStartPrice()
                ASwing.Coordinates = New LineCoordinates(CurrentTrendStartPoint(), CurrentTrendStartPoint())
                BSwing.Coordinates = New LineCoordinates(CurrentTrendStartPoint(), CurrentTrendStartPoint())
                CSwing.Coordinates = New LineCoordinates(CurrentTrendStartPoint(), CurrentTrendStartPoint())
                PotentialASwing.Coordinates = New LineCoordinates(CurrentTrendStartPoint(), CurrentTrendStartPoint())
                PotentialBSwing.Coordinates = New LineCoordinates(CurrentTrendStartPoint(), CurrentTrendStartPoint())
                For i = CurrentTrend.StartBar - 1 To CurrentTrend.EndBar - 1
                    CalculateABCLines(Chart.bars(i).Data)
                Next
                ProcessABCCalculation()
                ' end calculate swings
                CreateLengthTexts(CurrentTrend.LengthText)
                SetLengthTexts(CurrentTrend.LengthText, CurrentTrendStartPoint, CurrentTrendEndPoint, CurrentTrend.Direction, If(CurrentTrend.Direction = Direction.Up, TrendChannelUpColor, TrendChannelDownColor))
            ElseIf (trends.Count > 0 AndAlso Round(CurrentBar.High, 5) >= Round(CurrentTrendEndPrice(), 5) AndAlso CurrentTrend.Direction = Direction.Up) OrElse (trends.Count > 0 AndAlso Round(CurrentBar.Low, 5) <= Round(CurrentTrendEndPrice(), 5) AndAlso CurrentTrend.Direction = Direction.Down) Then
                'extension
                CurrentTrend.EndBar = CurrentBar.Number
                CurrentTrend.EndPrice = If(CurrentTrend.Direction = Direction.Up, CurrentBar.High, CurrentBar.Low)
                curTrendEvent = SwingEvent.Extension
                If TrendHistoryAsChannel = False Then
                    lastSwing.EndPoint = New Point(CurrentBar.Number, If(CurrentTrend.Direction = Direction.Down, CurrentBar.Low, CurrentBar.High))
                    lastSwing.StartPoint = New Point(CurrentTrend.StartBar, If(CurrentTrend.Direction = Direction.Down, Chart.bars(CurrentTrend.StartBar - 1).Data.High, Chart.bars(CurrentTrend.StartBar - 1).Data.Low))
                End If

                lastSwingCounterPotentialLine2.OuterPen.Thickness = 0
                    lastSwingCounterPotentialLine1.OuterPen.Thickness = 0

                    curTrendRetracement = CurrentTrendEndPrice()
                    If CurrentTrend.LengthText IsNot Nothing Then SetLengthTexts(CurrentTrend.LengthText, CurrentTrendStartPoint, CurrentTrendEndPoint, CurrentTrend.Direction, If(CurrentTrend.Direction = Direction.Up, TrendChannelUpColor, TrendChannelDownColor))
                End If
                If curTrendEvent = SwingEvent.None Then
                lastTrendExtension = If(CurrentTrend.Direction = Direction.Up, Max(lastTrendExtension, CurrentBar.High), Min(lastTrendExtension, CurrentBar.Low))
            End If
            If CurrentTrend.Direction = Direction.Up Then
                curTrendRetracement = Min(curTrendRetracement, CurrentBar.Low)
            Else
                curTrendRetracement = Max(curTrendRetracement, CurrentBar.High)
            End If
            'calculate ABC swing positions
            'If curTrendEvent = SwingEvent.Extension Then
            CalculateABCLines(CurrentBar)
            ProcessABCCalculation()
            'ProcessABCCalculation(CurrentTrend.ABCLengths.Count - 1, CurrentBar)
            'Dim biggestABC = HighlightBiggestABC()
            'CSwingChannel.Coordinates = New LineCoordinates(biggestABC.C, biggestABC.CLine.EndPoint)
            'If biggestABC.C.X = biggestABC.D.X Then
            '    CSwingPotentialChannel.Coordinates = New LineCoordinates(biggestABC.C, biggestABC.C)
            '    CSwingChannel.OuterPen.Thickness = 0
            '    CSwingPotentialChannel.OuterPen.Thickness = 0
            'Else
            '    CSwingPotentialChannel.Coordinates = New LineCoordinates(biggestABC.C, New Point(CurrentBar.Number, CurrentBar.Close))
            '    CSwingChannel.OuterPen.Thickness = AbcSwingChannelThickness
            '    CSwingPotentialChannel.OuterPen.Thickness = PotentialTrendChannel1Thickness
            'End If

            'End If

            'end calculate ABC swing positions

            If PotentialBSwing.StartPoint.X <> PotentialBSwing.EndPoint.X Then 'if a cswing doesnt exist
                lastSwingPotentialLine2.StartPoint = Me.CurrentTrend.StartPoint
                lastSwingPotentialLine2.EndPoint = New Point(CurrentBar.Number, CurrentBar.Avg)
                lastSwingPotentialLine2.OuterPen.Brush = New SolidColorBrush(If(CurrentTrend.Direction = Direction.Up, PotentialTrendChannelUpColor, PotentialTrendChannelDownColor))
                If IsLastBarOnChart Then
                    lastSwingPotentialLine1.StartPoint = Me.CurrentTrend.StartPoint
                    lastSwingPotentialLine1.EndPoint = AddToX(FindRangeBar(Me.CurrentTrend.EndPoint.X, CurrentBar.Number, If(Me.CurrentTrend.Direction = Direction.Down, True, False)), 1)
                    lastSwingPotentialLine1.OuterPen.Brush = New SolidColorBrush(If(CurrentTrend.Direction = Direction.Up, PotentialTrendChannelUpColor, PotentialTrendChannelDownColor))
                    lastSwingPotentialLine1.LockToEnd = False

                    'lastSwingCounterPotentialLine2.StartPoint = Me.CurrentTrend.EndPoint
                    'lastSwingCounterPotentialLine2.EndPoint = New Point(CurrentBar.Number, CurrentBar.High)
                    'lastSwingCounterPotentialLine2.OuterPen.Brush = New SolidColorBrush(AbcChannelAndTextColor)
                    'lastSwingCounterPotentialLine2.OuterPen.Thickness = PotentialTrendChannel2Thickness
                    Dim r = FindRangeBar(Me.CurrentTrend.EndPoint.X, CurrentBar.Number, If(Me.CurrentTrend.Direction = Direction.Up, False, True))
                    'lastSwingCounterPotentialLine1.StartPoint = Me.CurrentTrend.EndPoint
                    'lastSwingCounterPotentialLine1.EndPoint = AddToX(r, 0)
                    'lastSwingCounterPotentialLine1.OuterPen.Brush = New SolidColorBrush(AbcChannelAndTextColor)
                    'If PriceVsBarMode = PriceRVMode.BarCountMode And r.X - CurrentTrend.EndPoint.X < BarCountHistoryRV Then
                    '    lastSwingCounterPotentialLine1.OuterPen.Thickness = 0
                    '    lastSwingCounterPotentialLine2.OuterPen.Thickness = 0
                    'Else
                    'lastSwingCounterPotentialLine1.OuterPen.Thickness = AbcSwingChannelThickness
                    'lastSwingCounterPotentialLine2.OuterPen.Thickness = PotentialTrendChannel1Thickness
                    'End If
                    'lastSwingCounterPotentialLine1.LockToEnd = False

                End If
            Else
                lastSwingCounterPotentialLine1.OuterPen.Brush = Brushes.Transparent
                lastSwingCounterPotentialLine2.OuterPen.Brush = Brushes.Transparent
                lastSwingPotentialLine1.OuterPen.Brush = Brushes.Transparent
                lastSwingPotentialLine2.OuterPen.Brush = Brushes.Transparent
                lastSwingCounterPotentialLine1.Coordinates = New LineCoordinates(0, 0, 0, 0)
                lastSwingCounterPotentialLine2.Coordinates = New LineCoordinates(0, 0, 0, 0)
                lastSwingPotentialLine1.Coordinates = New LineCoordinates(0, 0, 0, 0)
                lastSwingPotentialLine2.Coordinates = New LineCoordinates(0, 0, 0, 0)
                lastSwingPotentialLine1.LockToEnd = False
                lastSwingPotentialLine2.LockToEnd = False
            End If
            ColorCurrentBars()
            If IsLastBarOnChart Then
                DrawProjectionLineAndRVText()

                'Dim percenText As String = ""
                'If trends.Count > 1 Then
                '    For i = 1 To trends.Count - 1
                '        If trends(i - 1).EndPrice - trends(i - 1).StartPrice <> 0 Then percenText &= CInt(Abs(trends(i).EndPrice - trends(i).StartPrice) / Abs(trends(i - 1).EndPrice - trends(i - 1).StartPrice) * 100) & ","
                '    Next
                '    Log(percenText)
                '    File.WriteAllLines(FileIO.SpecialDirectories.Desktop & "\temp.txt", Split(percenText, ","))
                'End If
            End If

        End Sub
        'Function HighlightBiggestABC() As ABCCombo
        '    Dim biggestABC = FindLargestABC()
        '    If biggestABC Is Nothing Then Return Nothing
        '    For i = 0 To CurrentTrend.ABCLengths.Count - 1
        '        Dim abc As ABCCombo = CurrentTrend.ABCLengths(i)
        '        If abc.ALine IsNot Nothing Then
        '            abc.ALine.Pen.Thickness = If(abc Is biggestABC, AbcSwingLineThickness, SwingLineHistoryThickness)
        '            abc.BLine.Pen.Thickness = If(abc Is biggestABC, AbcSwingLineThickness, SwingLineHistoryThickness)
        '            abc.CLine.Pen.Thickness = If(abc Is biggestABC, AbcSwingLineThickness, SwingLineHistoryThickness)
        '            abc.ALine.Pen.DashStyle = If(abc Is biggestABC, DashStyles.Solid, TrendLineDashStyle)
        '            abc.BLine.Pen.DashStyle = If(abc Is biggestABC, DashStyles.Solid, TrendLineDashStyle)
        '            abc.CLine.Pen.DashStyle = If(abc Is biggestABC, DashStyles.Solid, TrendLineDashStyle)
        '            abc.ALengthText.Font.FontSize = If(abc Is biggestABC, AbcLengthTextFontSize, SwingLengthHistoryTextFontSize)
        '            abc.ALengthText.Font.FontWeight = If(abc Is biggestABC, AbcLengthTextFontWeight, SwingLengthHistoryTextFontWeight)
        '            abc.BLengthText.Font.FontSize = If(abc Is biggestABC, AbcLengthTextFontSize, SwingLengthHistoryTextFontSize)
        '            abc.BLengthText.Font.FontWeight = If(abc Is biggestABC, AbcLengthTextFontWeight, SwingLengthHistoryTextFontWeight)
        '            abc.CLengthText.Font.FontSize = If(abc Is biggestABC, AbcLengthTextFontSize, SwingLengthHistoryTextFontSize)
        '            abc.CLengthText.Font.FontWeight = If(abc Is biggestABC, AbcLengthTextFontWeight, SwingLengthHistoryTextFontWeight)
        '            If abc Is biggestABC Then
        '                biggestABC.CLine.EndPoint = CurrentTrendEndPoint()
        '                biggestABC.CLengthText.Location = AddToY(biggestABC.CLine.EndPoint, If(CurrentTrend.Direction = Direction.Up, 1, -1) * Chart.GetRelativeFromRealHeight(vertSwingTextAlignment + SwingLengthHistoryTextFontSize))
        '            Else
        '                If i <> CurrentTrend.ABCLengths.Count - 1 Then
        '                    abc.CLine.EndPoint = CurrentTrend.ABCLengths(i + 1).B
        '                    abc.CLengthText.Location = abc.CLine.EndPoint
        '                End If
        '            End If
        '        End If
        '    Next
        '    If CurrentTrend.ABCLengths.Count > 0 Then
        '        If CurrentTrend.ABCLengths(CurrentTrend.ABCLengths.Count - 1).C.X = CurrentTrend.ABCLengths(CurrentTrend.ABCLengths.Count - 1).D.X And
        '           biggestABC IsNot CurrentTrend.ABCLengths(CurrentTrend.ABCLengths.Count - 1) Then
        '            CurrentTrend.ABCLengths(CurrentTrend.ABCLengths.Count - 1).BLine.Pen = New Pen(New SolidColorBrush(AbcChannelAndTextColor), AbcSwingLineThickness)
        '            CurrentTrend.ABCLengths(CurrentTrend.ABCLengths.Count - 1).BLine.Pen.DashStyle = DashStyles.Solid
        '        End If
        '    End If
        '    Return biggestABC
        'End Function
        Function FindLargestABC() As ABCCombo
            If CurrentTrend.ABCLengths.Count = 0 Then Return Nothing
            Dim biggestABC As ABCCombo = CurrentTrend.ABCLengths(0)
            For Each abc In CurrentTrend.ABCLengths
                If (PriceVsBarMode = PriceRVMode.PriceRVMode And Round(Abs(abc.B.Y - abc.C.Y), 5) >= Round(Abs(biggestABC.B.Y - biggestABC.C.Y), 5)) Or (PriceVsBarMode = PriceRVMode.BarCountMode And Abs(abc.B.X - abc.C.X) >= Abs(biggestABC.B.X - biggestABC.C.X)) Then
                    biggestABC = abc
                End If
            Next
            Return biggestABC
        End Function
        Sub CalculateABCLines(bar As BarData)
            Dim a, b, c, d, potentialC As Point
            a = ASwing.StartPoint
            b = BSwing.StartPoint
            c = CSwing.StartPoint
            d = CSwing.EndPoint
            potentialC = PotentialBSwing.EndPoint
            If (CurrentTrend.Direction = Direction.Up And Round(bar.High, 5) >= Round(b.Y, 5) And Round(bar.High, 5) >= Round(d.Y, 5)) Or
                (CurrentTrend.Direction = Direction.Down And Round(bar.Low, 5) <= Round(b.Y, 5) And Round(bar.Low, 5) <= Round(d.Y, 5)) Then 'extend the d point 
                If potentialC.X <> d.X Then
                    b = d
                    c = potentialC
                End If
                d = New Point(bar.Number, If(CurrentTrend.Direction = Direction.Up, bar.High, bar.Low))
                potentialC = d
            End If
            If ((CurrentTrend.Direction = Direction.Up And Round(bar.Low, 5) <= Round(c.Y, 5)) Or
                (CurrentTrend.Direction = Direction.Down And Round(bar.High, 5) >= Round(c.Y, 5))) And c.X = d.X Then ' extend c point
                c = New Point(bar.Number, If(CurrentTrend.Direction = Direction.Up, bar.Low, bar.High))
                d = c
            End If
            If ((CurrentTrend.Direction = Direction.Up And Round(bar.Low, 5) <= Round(potentialC.Y, 5) And Round(Abs(d.Y - bar.Low), 5) >= Round(Abs(b.Y - c.Y), 5)) Or
                (CurrentTrend.Direction = Direction.Down And Round(bar.High, 5) >= Round(potentialC.Y, 5) And Round(Abs(d.Y - bar.High), 5) >= Round(Abs(b.Y - c.Y), 5))) And c.X <> d.X Then ' extend potential c point
                potentialC = New Point(bar.Number, If(CurrentTrend.Direction = Direction.Up, bar.Low, bar.High))
                If CurrentTrend.Direction = Direction.Up Then lastTrendExtension = 0 Else lastTrendExtension = Decimal.MaxValue
            End If
            ASwing.StartPoint = a
            ASwing.EndPoint = b
            BSwing.StartPoint = b
            BSwing.EndPoint = c
            CSwing.StartPoint = c
            CSwing.EndPoint = d
            PotentialASwing.StartPoint = a
            PotentialASwing.EndPoint = d
            PotentialBSwing.StartPoint = d
            PotentialBSwing.EndPoint = potentialC
            If PriceVsBarMode = PriceRVMode.PriceRVMode Then
                ALengthText.Text = If(a.Y - b.Y = 0, "", FormatNumberLengthAndPrefix(Abs(a.Y - b.Y), Chart.Settings("DecimalPlaces").Value))
                BLengthText.Text = If(b.Y - c.Y = 0, "", FormatNumberLengthAndPrefix(Abs(b.Y - c.Y), Chart.Settings("DecimalPlaces").Value))
                CLengthText.Text = If(d.Y - c.Y = 0, "", FormatNumberLengthAndPrefix(Abs(c.Y - d.Y), Chart.Settings("DecimalPlaces").Value))
                PotentialBSwingText.Text = If(potentialC.Y - d.Y = 0, "", FormatNumberLengthAndPrefix(Abs(potentialC.Y - d.Y), Chart.Settings("DecimalPlaces").Value))
            Else
                ALengthText.Text = If(b.X - a.X = 0, "", CStr(b.X - a.X))
                BLengthText.Text = If(c.X - b.X = 0, "", CStr(c.X - b.X))
                CLengthText.Text = If(d.X - c.X = 0, "", CStr(d.X - c.X))
                PotentialBSwingText.Text = If(d.X - potentialC.X = 0, "", CStr(d.X - potentialC.X))
            End If

        End Sub
        Sub ProcessABCCalculation()
            ASwing.Pen.Brush = New SolidColorBrush(If(CurrentTrend.Direction = Direction.Up, TrendChannelUpColor, TrendChannelDownColor))
            BSwing.Pen.Brush = New SolidColorBrush(If(CurrentTrend.Direction = Direction.Up, TrendChannelUpColor, TrendChannelDownColor))
            CSwing.Pen.Brush = New SolidColorBrush(If(CurrentTrend.Direction = Direction.Up, TrendChannelUpColor, TrendChannelDownColor))
            PotentialASwing.Pen.Brush = New SolidColorBrush(If(CurrentTrend.Direction = Direction.Up, PotentialTrendChannelUpColor, PotentialTrendChannelDownColor))
            PotentialBSwing.Pen.Brush = New SolidColorBrush(If(CurrentTrend.Direction = Direction.Up, PotentialTrendChannelUpColor, PotentialTrendChannelDownColor))
            ALengthText.Location = ASwing.EndPoint
            BLengthText.Location = BSwing.EndPoint
            CLengthText.Location = CSwing.EndPoint

            ALengthText.VerticalAlignment = If(CurrentTrend.Direction = Direction.Down, LabelVerticalAlignment.Top, LabelVerticalAlignment.Bottom)
            BLengthText.VerticalAlignment = If(CurrentTrend.Direction = Direction.Down, LabelVerticalAlignment.Bottom, LabelVerticalAlignment.Top)
            CLengthText.VerticalAlignment = If(CurrentTrend.Direction = Direction.Down, LabelVerticalAlignment.Top, LabelVerticalAlignment.Bottom)

            If PotentialBSwing.EndPoint.X = PotentialBSwing.StartPoint.X Then
                PotentialASwing.Pen.Thickness = 0
                PotentialBSwing.Pen.Thickness = 0
                SwingChannel.Coordinates = New LineCoordinates(CSwing.StartPoint, CSwing.EndPoint)
                PotentialBSwingText.Text = ""
                ASwing.Pen.Thickness = AbcSwingLineThickness
                BSwing.Pen.Thickness = AbcSwingLineThickness
                CSwing.Pen.Thickness = AbcSwingLineThickness
            Else
                PotentialASwing.Pen.Thickness = AbcSwingLineThickness
                PotentialBSwing.Pen.Thickness = AbcSwingLineThickness
                SwingChannel.Coordinates = New LineCoordinates(PotentialBSwing.StartPoint, PotentialBSwing.EndPoint)
                PotentialBSwingText.Location = PotentialBSwing.EndPoint
                PotentialBSwingText.VerticalAlignment = If(CurrentTrend.Direction = Direction.Down, LabelVerticalAlignment.Bottom, LabelVerticalAlignment.Top)
                ASwing.Pen.Thickness = 0
                BSwing.Pen.Thickness = 0
                CSwing.Pen.Thickness = 0
                ALengthText.Text = ""
                BLengthText.Text = ""
                CLengthText.Text = ""
            End If

        End Sub
        'Sub ProcessABCCalculation(abcIndex As Integer, bar As BarData)
        '    Dim c As ABCCombo = CurrentTrend.ABCLengths(abcIndex)
        '    If (CurrentTrend.Direction = Direction.Up And bar.High >= c.B.Y And bar.High >= c.D.Y) Or
        '                    (CurrentTrend.Direction = Direction.Down And bar.Low <= c.B.Y And bar.Low <= c.D.Y) Then 'extend the d point 
        '        c.D = New Point(bar.Number, If(CurrentTrend.Direction = Direction.Up, bar.High, bar.Low))
        '        currentSwingRetracement = c.D.Y
        '        SetABCLines(c, CurrentTrend.Direction)
        '        If CurrentTrend.ABCLengths.Count > 1 Then
        '            If CurrentTrend.ABCLengths(abcIndex - 1).C.X = c.A.X Then
        '                CurrentTrend.ABCLengths(abcIndex - 1).CLengthText.Font.Brush = Brushes.Transparent
        '            Else
        '                CurrentTrend.ABCLengths(abcIndex - 1).CLengthText.Font.Brush = New SolidColorBrush(AbcChannelAndTextColor)
        '            End If
        '        End If
        '    End If
        '    If c.D.X <> c.C.X AndAlso
        '              ((Abs(c.D.Y - If(CurrentTrend.Direction = Direction.Up, bar.Low, bar.High)) >= PriceSwingHistoryRVMultiplier * Chart.Settings("RangeValue").Value And PriceVsBarMode = PriceRVMode.PriceRVMode) Or
        '              (Abs(c.D.X - bar.Number) >= BarCountHistoryRV And If(CurrentTrend.Direction = Direction.Up, bar.Low <= currentSwingRetracement, bar.High >= currentSwingRetracement) And PriceVsBarMode = PriceRVMode.BarCountMode)) Then  ' bPoint.Y - cPoint.Y Then ' reset the b point  (new longest bc length )
        '        If c.A.X <> c.B.X Then
        '            CurrentTrend.ABCLengths.Add(New ABCCombo With {.A = CurrentTrendStartPoint(), .B = c.D, .C = New Point(bar.Number, If(CurrentTrend.Direction = Direction.Up, bar.Low, bar.High)), .D = .C})
        '            c = CurrentTrend.ABCLengths(CurrentTrend.ABCLengths.Count - 1)
        '        Else
        '            c.B = c.D
        '            c.C = New Point(bar.Number, If(CurrentTrend.Direction = Direction.Up, bar.Low, bar.High))
        '            c.D = c.C
        '        End If
        '        SetABCLines(c, CurrentTrend.Direction)
        '    End If
        '    If ((CurrentTrend.Direction = Direction.Up And bar.Low <= c.C.Y) Or
        '        (CurrentTrend.Direction = Direction.Down And bar.High >= c.C.Y)) And c.C.X = c.D.X Then ' extend c point downwards
        '        c.C = New Point(bar.Number, If(CurrentTrend.Direction = Direction.Up, bar.Low, bar.High))
        '        c.D = c.C
        '        c.A = CurrentTrendStartPoint()
        '        For lengthI As Integer = CurrentTrend.ABCLengths.Count - 2 To 0 Step -1
        '            If (PriceVsBarMode = PriceRVMode.PriceRVMode And Abs(CurrentTrend.ABCLengths(lengthI).B.Y - CurrentTrend.ABCLengths(lengthI).C.Y) >= Abs(c.B.Y - c.C.Y)) Or
        '                   (PriceVsBarMode = PriceRVMode.BarCountMode And Abs(CurrentTrend.ABCLengths(lengthI).B.X - CurrentTrend.ABCLengths(lengthI).C.X) >= Abs(c.B.X - c.C.X)) Then
        '                c.A = CurrentTrend.ABCLengths(lengthI).C
        '                Exit For
        '            Else
        '                c.IsSpanner = True
        '            End If
        '        Next
        '        SetABCLines(c, CurrentTrend.Direction)
        '    End If
        '    currentSwingRetracement = If(CurrentTrend.Direction = Direction.Up, Min(currentSwingRetracement, bar.Low), Max(currentSwingRetracement, bar.High))
        'End Sub
        'Sub SetABCLines(abc As ABCCombo, direction As Direction)
        '    Dim col As Color
        '    If abc.D.X = abc.C.X Then 'if a cswing doesnt exist
        '        col = If(CurrentTrend.Direction = Direction.Up, PotentialTrendChannelUpColor, PotentialTrendChannelDownColor)
        '    Else
        '        col = If(CurrentTrend.Direction = Direction.Up, TrendChannelUpColor, TrendChannelDownColor)
        '    End If
        '    If abc.ALine Is Nothing Then abc.ALine = NewTrendLine(col)
        '    If abc.BLine Is Nothing Then abc.BLine = NewTrendLine(col)
        '    If abc.CLine Is Nothing Then abc.CLine = NewTrendLine(col)
        '    abc.ALine.IsSelectable = False : abc.ALine.IsEditable = False
        '    abc.BLine.IsSelectable = False : abc.BLine.IsEditable = False
        '    abc.CLine.IsSelectable = False : abc.CLine.IsEditable = False
        '    abc.ALine.Coordinates = New LineCoordinates(abc.A, abc.B)
        '    abc.BLine.Coordinates = New LineCoordinates(abc.B, abc.C)
        '    abc.CLine.Coordinates = New LineCoordinates(abc.C, abc.D)
        '    abc.ALine.Pen = New Pen(New SolidColorBrush(col), AbcSwingLineThickness)
        '    abc.BLine.Pen = New Pen(New SolidColorBrush(col), AbcSwingLineThickness)
        '    abc.CLine.Pen = New Pen(New SolidColorBrush(col), AbcSwingLineThickness)
        '    If abc.ALengthText Is Nothing Then abc.ALengthText = NewLabel("", AbcChannelAndTextColor, New Point(0, 0), True, , True)
        '    If abc.BLengthText Is Nothing Then abc.BLengthText = NewLabel("", AbcChannelAndTextColor, New Point(0, 0), True, , True)
        '    If abc.CLengthText Is Nothing Then abc.CLengthText = NewLabel("", AbcChannelAndTextColor, New Point(0, 0), True, , True)
        '    abc.ALengthText.IsSelectable = False : abc.ALengthText.IsEditable = False
        '    abc.BLengthText.IsSelectable = False : abc.BLengthText.IsEditable = False
        '    abc.CLengthText.IsSelectable = False : abc.CLengthText.IsEditable = False
        '    abc.ALengthText.VerticalAlignment = If(direction = Direction.Up, LabelVerticalAlignment.Bottom, LabelVerticalAlignment.Top)
        '    abc.BLengthText.VerticalAlignment = If(direction = Direction.Up, LabelVerticalAlignment.Top, LabelVerticalAlignment.Bottom)
        '    abc.CLengthText.VerticalAlignment = If(direction = Direction.Up, LabelVerticalAlignment.Bottom, LabelVerticalAlignment.Top)
        '    abc.ALengthText.HorizontalAlignment = LabelHorizontalAlignment.Right
        '    abc.BLengthText.HorizontalAlignment = LabelHorizontalAlignment.Right
        '    abc.CLengthText.HorizontalAlignment = LabelHorizontalAlignment.Right
        '    abc.ALengthText.Location = AddToY(abc.B, If(direction = Direction.Up, 1, -1) * If(abc.IsSpanner, Chart.GetRelativeFromRealHeight(vertSwingTextAlignment + SwingLengthHistoryTextFontSize), 0))
        '    abc.BLengthText.Location = abc.C
        '    abc.CLengthText.Location = abc.D ' AddToY(abc.D, If(direction = Direction.Up, 1, -1) * If(abc.IsSpanner, Chart.GetRelativeFromRealHeight(vertSwingTextAlignment + LengthTextFontSize1), 0))
        '    abc.ALengthText.Font.FontSize = SwingLengthHistoryTextFontSize
        '    abc.BLengthText.Font.FontSize = SwingLengthHistoryTextFontSize
        '    abc.CLengthText.Font.FontSize = SwingLengthHistoryTextFontSize
        '    abc.ALengthText.Font.FontWeight = SwingLengthHistoryTextFontWeight
        '    abc.BLengthText.Font.FontWeight = SwingLengthHistoryTextFontWeight
        '    abc.CLengthText.Font.FontWeight = SwingLengthHistoryTextFontWeight
        '    If EnableEzraLayout Then
        '        If PriceVsBarMode = PriceRVMode.PriceRVMode Then
        '            abc.ALengthText.Text = If(abc.A.Y - abc.B.Y = 0, "", FormatNumberLengthAndPrefix(Abs(abc.A.Y - abc.B.Y), Chart.Settings("DecimalPlaces").Value))
        '            abc.BLengthText.Text = If(abc.B.Y - abc.C.Y = 0, "", FormatNumberLengthAndPrefix(Abs(abc.B.Y - abc.C.Y), Chart.Settings("DecimalPlaces").Value) & " " &
        '                Round(Abs(abc.B.Y - abc.C.Y) / Abs(CurrentTrendStartPrice() - CurrentTrendEndPrice()) * 100, 0) & "%") & " " & abc.C.X - abc.B.X
        '            abc.CLengthText.Text = If(abc.D.Y - abc.C.Y = 0, "", FormatNumberLengthAndPrefix(Abs(abc.C.Y - abc.D.Y), Chart.Settings("DecimalPlaces").Value))
        '        Else
        '            abc.ALengthText.Text = If(abc.B.X - abc.A.X = 0, "", CStr(abc.B.X - abc.A.X))
        '            abc.BLengthText.Text = If(abc.C.X - abc.B.X = 0, "", CStr(abc.C.X - abc.B.X) & " " & Round(Abs(abc.B.X - abc.C.X) / Abs(CurrentTrend.StartBar - CurrentTrend.EndBar) * 100, 0) & "%")
        '            abc.CLengthText.Text = If(abc.D.X - abc.C.X = 0, "", CStr(abc.D.X - abc.C.X))
        '        End If
        '    Else
        '        If PriceVsBarMode = PriceRVMode.PriceRVMode Then
        '            abc.ALengthText.Text = If(abc.A.Y - abc.B.Y = 0, "", FormatNumberLengthAndPrefix(Abs(abc.A.Y - abc.B.Y), Chart.Settings("DecimalPlaces").Value))
        '            abc.BLengthText.Text = If(abc.B.Y - abc.C.Y = 0, "", FormatNumberLengthAndPrefix(Abs(abc.B.Y - abc.C.Y), Chart.Settings("DecimalPlaces").Value))
        '            abc.CLengthText.Text = If(abc.D.Y - abc.C.Y = 0, "", FormatNumberLengthAndPrefix(Abs(abc.C.Y - abc.D.Y), Chart.Settings("DecimalPlaces").Value))
        '        Else
        '            abc.ALengthText.Text = If(abc.B.X - abc.A.X = 0, "", CStr(abc.B.X - abc.A.X))
        '            abc.BLengthText.Text = If(abc.C.X - abc.B.X = 0, "", CStr(abc.C.X - abc.B.X))
        '            abc.CLengthText.Text = If(abc.D.X - abc.C.X = 0, "", CStr(abc.D.X - abc.C.X))
        '        End If
        '    End If
        'End Sub
        Private Sub SetLengthTexts(ByRef lbl As Label, priceValStartPoint As Point, priceValEndPoint As Point, direction As Direction, color As Color)
            If direction = Direction.Up Then
                lbl.VerticalAlignment = LabelVerticalAlignment.Bottom
            Else
                lbl.VerticalAlignment = LabelVerticalAlignment.Top
            End If

            lbl.Font.Brush = New SolidColorBrush(color)
            If PriceVsBarMode = PriceRVMode.PriceRVMode Then
                lbl.Text = FormatNumberLengthAndPrefix(Abs(priceValStartPoint.Y - priceValEndPoint.Y), Chart.Settings("DecimalPlaces").Value) & If(EnableEzraLayout, " " & priceValEndPoint.X - priceValStartPoint.X & " " & Round(CDec(FormatNumberLengthAndPrefix(Abs(priceValStartPoint.Y - priceValEndPoint.Y), Chart.Settings("DecimalPlaces").Value)) / (priceValEndPoint.X - priceValStartPoint.X) * 100, 0) & "%", "")
                lbl.Tag = Abs(priceValStartPoint.Y - priceValEndPoint.Y)
            ElseIf PriceVsBarMode = PriceRVMode.BarCountMode Then
                lbl.Text = priceValEndPoint.X - priceValStartPoint.X
                lbl.Tag = Abs(priceValStartPoint.X - priceValEndPoint.X)
            End If
            lbl.Location = New Point(priceValEndPoint.X, priceValEndPoint.Y + If(direction = Direction.Up, 1, -1) * Chart.GetRelativeFromRealHeight(vertSwingTextAlignment + AbcLengthTextFontSize))
        End Sub
        Private Sub CreateLengthTexts(ByRef lbl As Label)
            lbl = NewLabel("", Colors.Gray, New Point(0, 0), True, , True)
            lbl.HorizontalAlignment = LabelHorizontalAlignment.Right

            AddHandler lbl.MouseDown,
                Sub(sender As Object, location As Point)
                    If PriceVsBarMode = PriceRVMode.PriceRVMode Then
                        PriceRV = CDec(CType(sender, Label).Tag) + Chart.GetMinTick
                    ElseIf PriceVsBarMode = PriceRVMode.BarCountMode Then
                        BarCountRV = CDec(CType(sender, Label).Tag)
                    End If
                    Chart.ReApplyAnalysisTechnique(Me)
                End Sub
            lbl.Font.FontSize = TrendLengthTextFontSize
            lbl.Font.FontWeight = TrendLengthTextFontWeight
        End Sub
        Protected Overrides Sub Main_WithIntraBarMoves()
            'If lastTick <> 0 And EnableEzraLayout Then
            '    If CurrentBar.Close - lastTick <> 0 Then tickMoveHistory.Insert(0, CInt((CurrentBar.Close - lastTick) / Abs(CurrentBar.Close - lastTick)))
            '    Dim tickCount As Integer = 30
            '    Dim max = tickCount / 2
            '    If tickMoveHistory.Count > tickCount Then
            '        tickMoveHistory.RemoveAt(tickCount)
            '        Dim energy As Double

            '        For i = 1 To tickCount - 1
            '            If tickMoveHistory(i) = tickMoveHistory(i - 1) Then
            '                energy += RecipCalc(1, 0, 50, i / tickCount)
            '            End If
            '        Next
            '        If IsLastBarOnChart Then
            '            'Log(energy)
            '            Dim point As Point = Chart.Bounds.TopRight
            '            point = NegateY(point)
            '            point = AddToX(point, -Chart.GetRelativeFromRealWidth(Chart.Settings("PriceBarWidth").Value))
            '            point = AddToX(point, -Chart.GetRelativeFromRealWidth(110))
            '            point = AddToY(point, -Chart.GetRelativeFromRealHeight(20))

            '            energyLine.Coordinates = New LineCoordinates(point.X, point.Y, point.X + Chart.GetRelativeFromRealWidth(energy / max * 100), point.Y)
            '            energyLine.Pen = New Pen(New SolidColorBrush(Color.FromArgb(255, 255, LinCalc(0, 255, max, 0, energy), 0)), 12)
            '            energyLineMarker.Coordinates = New LineCoordinates(point.X + Chart.GetRelativeFromRealWidth(100), point.Y - Chart.GetRelativeFromRealHeight(6), point.X + Chart.GetRelativeFromRealWidth(100), point.Y + Chart.GetRelativeFromRealHeight(6))
            '            energyLineMarker.Pen = New Pen(Brushes.Red, 1)
            '        End If

            '    End If
            'End If
            'lastTick = CurrentBar.Close
        End Sub

        Private Function FindRangeBar(startBar As Integer, endBar As Integer, findHighestPoint As Boolean) As Point
            Dim pnt As New Point(0, If(findHighestPoint, Double.MinValue, Double.MaxValue))
            For i = startBar - 1 To endBar - 1
                If findHighestPoint And Chart.bars(i).Data.High >= pnt.Y Then
                    pnt = New Point(i, Chart.bars(i).Data.High)
                ElseIf findHighestPoint = False And Chart.bars(i).Data.Low <= pnt.Y Then
                    pnt = New Point(i, Chart.bars(i).Data.Low)
                End If
            Next
            Return pnt
        End Function

        Private Sub ColorCurrentBars()

            If curTrendEvent = SwingEvent.Extension Or curTrendEvent = SwingEvent.NewSwing Then
                If CurrentTrend.Direction = Direction.Up Then
                    BarColorRoutine(CurrentTrend.StartBar, CurrentBar.Number - 1, UpColor)
                ElseIf CurrentTrend.Direction = Direction.Down Then
                    BarColorRoutine(CurrentTrend.StartBar, CurrentBar.Number - 1, DownColor)
                End If
            Else
                CType(Chart.bars(CurrentBar.Number - 1).Pen.Brush, SolidColorBrush).Color = NeutralColor
                RefreshObject(Chart.bars(CurrentBar.Number - 1))
            End If

            barColorsDirty = True

        End Sub
        Private Function GetGradientBarBrush(aboveColor As Color, belowColor As Color, point As Decimal) As LinearGradientBrush
            Dim brush As New LinearGradientBrush With {.StartPoint = New Point(0, 0), .EndPoint = New Point(0, 1)}
            If Chart.bars(CurrentBar.Number - 1).Data.High < point Then
                brush.GradientStops.Add(New GradientStop(belowColor, 0.5))
            ElseIf Chart.bars(CurrentBar.Number - 1).Data.Low > point Then
                brush.GradientStops.Add(New GradientStop(aboveColor, 0.5))
            Else
                brush.GradientStops.Add(New GradientStop(aboveColor, 1 - (point - Chart.bars(CurrentBar.Number - 1).Data.Low) / Chart.bars(CurrentBar.Number - 1).Data.Range))
                brush.GradientStops.Add(New GradientStop(belowColor, 1 - (point - Chart.bars(CurrentBar.Number - 1).Data.Low) / Chart.bars(CurrentBar.Number - 1).Data.Range))
            End If
            Return brush
        End Function

        Protected Sub BarColorRoutine(ByVal startBar As Integer, ByVal endBar As Integer, ByVal color As Color)
            For i = startBar To endBar
                If Not TypeOf Chart.bars(i).Pen.Brush Is SolidColorBrush Then
                    Chart.bars(i).Pen.Brush = New SolidColorBrush
                End If
                If CType(Chart.bars(i).Pen.Brush, SolidColorBrush).Color <> color Then
                    Chart.bars(i).Pen.Brush = New SolidColorBrush(color)
                    RefreshObject(Chart.bars(i))
                End If
            Next
        End Sub

        Private Sub DrawProjectionLineAndRVText()
            If PriceVsBarMode = PriceRVMode.PriceRVMode Then
                newTrendRVText.Location = New Point(CurrentBar.Number, CurrentTrendEndPrice() + If(CurrentTrend.Direction = Direction.Up, -PriceRV, PriceRV))
                newTrendRVText.Font.Brush = New SolidColorBrush(If(CurrentTrend.Direction = Direction.Up, TrendChannelDownColor, TrendChannelUpColor))
                If (CurrentTrend.Direction = Direction.Up And Round(curTrendRetracement, 5) <= Round(CurrentTrendEndPrice() - PriceRV, 5)) Or (CurrentTrend.Direction = Direction.Down And Round(curTrendRetracement, 5) >= Round(CurrentTrendEndPrice() + PriceRV, 5)) Then
                    newTrendRVText.Location = New Point(CurrentBar.Number, curTrendRetracement)
                Else
                    If CurrentTrendEndPrice() <> CurrentTrendStartPrice() Then
                        newTrendRVText.Text = newRVTextPrefix1 & Round(PriceRV / Abs(CurrentTrendEndPrice() - CurrentTrendStartPrice()) * 100) & "%  " & FormatNumberLengthAndPrefix(PriceRV, Chart.Settings("DecimalPlaces").Value) & " RV  " & FormatNumber(newTrendRVText.Location.Y, Chart.Settings("DecimalPlaces").Value)
                    End If
                End If
                extendTrendText.Text = rvTextPrefix1 & "0% " & FormatNumber(CurrentTrendEndPrice, Chart.Settings("DecimalPlaces").Value)
                extendTrendText.Location = New Point(CurrentBar.Number, CurrentTrendEndPrice)
                extendTrendText.Font.Brush = New SolidColorBrush(If(CurrentTrend.Direction = Direction.Up, TrendChannelUpColor, TrendChannelDownColor))

                'retraceTrendText.Location = New Point(CurrentBar.Number, CurrentTrendStartPoint.Y)
                'retraceTrendText.Text = rvTextPrefix1 & "100%  " & FormatNumber(retraceTrendText.Location.Y, Chart.Settings("DecimalPlaces").Value)
                'retraceTrendText.Font.Brush = New SolidColorBrush(If(CurrentTrend.Direction = Direction.Up, TrendChannelDownColor, TrendChannelUpColor))

                'lastExtendText.Location = New Point(CurrentBar.Number, lastTrendExtension)
                'If PotentialBSwing.EndPoint.X = PotentialBSwing.StartPoint.X Or lastTrendExtension = 0 Or lastTrendExtension = Decimal.MaxValue Then
                '    lastExtendText.Text = ""
                'Else
                '    lastExtendText.Text = rvTextPrefix1 & Round(Abs(CurrentTrendEndPrice() - lastTrendExtension) / Abs(CurrentTrendEndPrice() - CurrentTrendStartPrice()) * 100) & "% " & FormatNumber(lastExtendText.Location.Y, Chart.Settings("DecimalPlaces").Value)
                'End If
                'lastExtendText.Font.Brush = New SolidColorBrush(AbcChannelAndTextColor)

                'If PotentialBSwing.StartPoint.X <> PotentialBSwing.EndPoint.X Or (CurrentTrend.Direction = Direction.Up And curTrendRetracement <= CSwing.EndPoint.Y - (BSwing.StartPoint.Y - CSwing.StartPoint.Y)) Or (CurrentTrend.Direction = Direction.Down And curTrendRetracement >= CSwing.EndPoint.Y - (BSwing.StartPoint.Y - CSwing.StartPoint.Y)) Then
                '    newSwingTextObject1.Text = newRVTextPrefix1 & Round(Abs(PotentialBSwing.StartPoint.Y - PotentialBSwing.EndPoint.Y) / Abs(ASwing.StartPoint.Y - CSwing.EndPoint.Y) * 100, 0) & "% " & FormatNumberLengthAndPrefix(Abs(PotentialBSwing.StartPoint.Y - PotentialBSwing.EndPoint.Y), Chart.Settings("DecimalPlaces").Value) & "RV " &
                '            FormatNumber(CurrentTrendEndPrice() - (PotentialBSwing.StartPoint.Y - PotentialBSwing.EndPoint.Y), Chart.Settings("DecimalPlaces").Value)
                '    newSwingTextObject1.Location = New Point(CurrentBar.Number, CurrentTrendEndPrice() - (PotentialBSwing.StartPoint.Y - PotentialBSwing.EndPoint.Y))
                'Else
                '    newSwingTextObject1.Text = newRVTextPrefix1 & Round(Abs(BSwing.StartPoint.Y - CSwing.StartPoint.Y) / Abs(ASwing.StartPoint.Y - CSwing.EndPoint.Y) * 100, 0) & "% " & FormatNumberLengthAndPrefix(Abs(BSwing.StartPoint.Y - CSwing.StartPoint.Y), Chart.Settings("DecimalPlaces").Value) & "RV " &
                '            FormatNumber(CurrentTrendEndPrice() - (BSwing.StartPoint.Y - CSwing.StartPoint.Y), Chart.Settings("DecimalPlaces").Value) & " " & FormatNumberLengthAndPrefix(Abs(BSwing.StartPoint.Y - CSwing.StartPoint.Y), Chart.Settings("DecimalPlaces").Value)
                '    newSwingTextObject1.Location = New Point(CurrentBar.Number, CurrentTrendEndPrice() - (BSwing.StartPoint.Y - CSwing.StartPoint.Y))
                'End If
                'newSwingTextObject1.Font.Brush = New SolidColorBrush(AbcChannelAndTextColor)

            End If
            If PriceVsBarMode = PriceRVMode.BarCountMode Then

                newTrendRVText.Location = New Point(CurrentBar.Number, curTrendRetracement)
                newTrendRVText.Font.Brush = New SolidColorBrush(If(CurrentTrend.Direction = Direction.Up, TrendChannelDownColor, TrendChannelUpColor))
                newTrendRVText.Text = newRVTextPrefix2 & " " & curTrendRetracement
                extendTrendText.Text = rvTextPrefix2 & " " & CurrentTrendEndPrice()
                extendTrendText.Location = New Point(CurrentBar.Number, CurrentTrendEndPrice)
                extendTrendText.Font.Brush = New SolidColorBrush(If(CurrentTrend.Direction = Direction.Up, TrendChannelUpColor, TrendChannelDownColor))

                newBarRVLine.Coordinates = New LineCoordinates(CurrentTrend.EndBar + BarCountRV, CurrentTrendEndPrice, CurrentTrend.EndBar + BarCountRV, If(CurrentTrend.Direction = Direction.Up, Min(CurrentTrendEndPrice() - PriceRV, curTrendRetracement), Max(CurrentTrendEndPrice() + PriceRV, curTrendRetracement)))
                newBarRVLine.Pen.Brush = New SolidColorBrush(If(CurrentTrend.Direction = Direction.Up, TrendChannelDownColor, TrendChannelUpColor))

                newBarRVText.Location = New Point(CurrentTrend.EndBar + BarCountRV + 1, Avg(newBarRVLine.StartPoint.Y, newBarRVLine.EndPoint.Y))
                newBarRVText.Font.Brush = New SolidColorBrush(If(CurrentTrend.Direction = Direction.Up, TrendChannelDownColor, TrendChannelUpColor))
                newBarRVText.Text = BarCountRV
                If CurrentBar.Number >= CurrentTrend.EndBar + BarCountRV Then
                    newBarRVText.Location = New Point(0, 0)
                    newBarRVLine.Coordinates = New LineCoordinates(0, 0, 0, 0)
                End If
            End If
        End Sub

        Private Function CurrentTrendEndPrice() As Decimal
            If CurrentTrend.Direction = Direction.Up Then
                Return Chart.bars(CurrentTrend.EndBar - 1).Data.High
            Else
                Return Chart.bars(CurrentTrend.EndBar - 1).Data.Low
            End If
        End Function
        Private Function CurrentTrendStartPrice() As Decimal
            If CurrentTrend.Direction = Direction.Up Then
                Return Chart.bars(CurrentTrend.StartBar - 1).Data.Low
            Else
                Return Chart.bars(CurrentTrend.StartBar - 1).Data.High
            End If
        End Function
        Private Function CurrentTrendStartPoint() As Point
            Return New Point(CurrentTrend.StartBar, CurrentTrendStartPrice)
        End Function
        Private Function CurrentTrendEndPoint() As Point
            Return New Point(CurrentTrend.EndBar, CurrentTrendEndPrice)
        End Function
        Private Function PrevTrendStartPoint() As Point
            If trends.Count > 1 Then
                Dim startY As Decimal
                If trends(trends.Count - 2).Direction = Direction.Up Then
                    startY = Chart.bars(trends(trends.Count - 2).StartBar - 1).Data.Low
                Else
                    startY = Chart.bars(trends(trends.Count - 2).StartBar - 1).Data.High
                End If
                Return New Point(trends(trends.Count - 2).StartBar, startY)
            Else
                Return New Point(0, 0)
            End If
        End Function
        Private Function PrevTrendEndPoint() As Point
            If trends.Count > 1 Then
                Dim startY As Decimal
                If trends(trends.Count - 2).Direction = Direction.Up Then
                    startY = Chart.bars(trends(trends.Count - 2).EndBar - 1).Data.High
                Else
                    startY = Chart.bars(trends(trends.Count - 2).EndBar - 1).Data.Low
                End If
                Return New Point(trends(trends.Count - 2).EndBar, startY)
            Else
                Return New Point(0, 0)
            End If
        End Function
        Friend Overrides Function GetDollarValue(ByVal priceDif As Decimal) As Decimal
            Dim multiplier As Double
            If Chart.Settings("UseRandom").Value Then
                multiplier = 1
            Else
                If Chart.IB.Contract(Chart.TickerID).Multiplier = "" OrElse Not IsNumeric(Chart.IB.Contract(Chart.TickerID).Multiplier) Then
                    multiplier = 1
                Else
                    multiplier = Chart.IB.Contract(Chart.TickerID).Multiplier
                End If
            End If
            Dim shares As Integer = Chart.Settings("DefaultOrderQuantity").Value
            Dim a = priceDif / Chart.GetMinTick * Chart.GetMinTick * multiplier * shares
            Return Round(priceDif, 0)
        End Function
        Friend Overrides Function GetRVFromDollar(ByVal dollarDif As Decimal) As Decimal
            Dim multiplier As Double
            If Chart.Settings("UseRandom").Value Then
                multiplier = 1
            Else
                If Chart.IB.Contract(Chart.TickerID).Multiplier = "" OrElse Not IsNumeric(Chart.IB.Contract(Chart.TickerID).Multiplier) Then
                    multiplier = 1
                Else
                    multiplier = Chart.IB.Contract(Chart.TickerID).Multiplier
                End If
            End If
            Dim shares As Integer = Chart.Settings("DefaultOrderQuantity").Value
            'Dim a = priceDif / Chart.GetMinTick * Chart.GetMinTick * multiplier * shares
            Return Round(dollarDif, 6) 'Round(dollarDif / (multiplier * shares), 6)
        End Function

        Private Function NewSwing(ByVal color As Color, ByVal startPoint As Point, ByVal endPoint As Point, ByVal show As Boolean, ByVal direction As Direction) As Swing
            Return New Swing(NewTrendLine(color, startPoint, endPoint, show), direction)
        End Function

        Private Sub Elapsed()

        End Sub
#Region "AutoTrendPad"
        Dim addRVMinMove As Button
        Dim subtractRVMinMove As Button
        Dim rvBaseValue As Button
        Dim currentRVPopupBtn As Button

        Dim addBarRVMinMove As Button
        Dim subtractBarRVMinMove As Button
        Dim barRVBaseValue As Button
        Dim currentBarRVPopupBtn As Button

        Dim grabArea As Border
        Dim currentRBPopupBtn As Button
        Dim addRBMinMove As Button
        Dim subtractRBMinMove As Button

        Dim rvPopup As Popup
        Dim rvPopupGrid As Grid
        Dim barRVPopup As Popup
        Dim barRVPopupGrid As Grid
        Dim popupRB As Popup
        Dim popupRBGrid As Grid
        Dim divider As Border
        Dim bd As Border
        Dim grd As Grid
        Private Sub InitGrid()
            grd = New Grid
            bd = New Border
            grd.Margin = New Thickness(0)
            grd.HorizontalAlignment = Windows.HorizontalAlignment.Center
            For i = 1 To 4
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
            divider = New Border With {.BorderThickness = New Thickness(0), .BorderBrush = Brushes.Gray, .Background = Brushes.Black, .Margin = New Thickness(0.5)}
            divider.Height = 4
            grabArea = New Border With {.BorderThickness = New Thickness(1), .BorderBrush = Brushes.Gray, .Background = New SolidColorBrush(Color.FromArgb(255, 252, 184, 41)), .Margin = New Thickness(0.5)}
            grabArea.SetValue(Grid.ColumnSpanProperty, 3)
            grabArea.Height = 17
            grd.Children.Add(grabArea)
        End Sub
        Sub SetRows()
            Grid.SetRow(addRVMinMove, 0)
            Grid.SetRow(subtractRVMinMove, 1)
            Grid.SetRow(rvBaseValue, 2)
            Grid.SetRow(currentRVPopupBtn, 3)

            Grid.SetRow(divider, 4)

            Grid.SetRow(addBarRVMinMove, 5)
            Grid.SetRow(subtractBarRVMinMove, 6)
            Grid.SetRow(barRVBaseValue, 7)
            Grid.SetRow(currentBarRVPopupBtn, 8)

            Grid.SetRow(grabArea, 9)
            Grid.SetRow(currentRBPopupBtn, 10)
            Grid.SetRow(addRBMinMove, 11)
            Grid.SetRow(subtractRBMinMove, 12)

            If PriceVsBarMode = PriceRVMode.PriceRVMode Then
                grd.Children.Add(addRVMinMove)
                grd.Children.Add(subtractRVMinMove)
                grd.Children.Add(rvBaseValue)
                grd.Children.Add(currentRVPopupBtn)
            End If
            grd.Children.Add(divider)
            If PriceVsBarMode = PriceRVMode.BarCountMode Then
                grd.Children.Add(addBarRVMinMove)
                grd.Children.Add(subtractBarRVMinMove)
                grd.Children.Add(barRVBaseValue)
                grd.Children.Add(currentBarRVPopupBtn)
            End If
            grd.Children.Add(currentRBPopupBtn)
            grd.Children.Add(addRBMinMove)
            grd.Children.Add(subtractRBMinMove)
        End Sub
        Sub InitControls()
            Dim fontsize As Integer = 16
            addRVMinMove = New Button With {.Background = Brushes.LightGray, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "+1"}
            subtractRVMinMove = New Button With {.Background = Brushes.LightGray, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "-1"}
            rvBaseValue = New Button With {.Background = New SolidColorBrush(Color.FromArgb(255, 230, 235, 255)), .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "-1"}
            currentRVPopupBtn = New Button With {.Background = Brushes.Cyan, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19}

            addBarRVMinMove = New Button With {.Background = Brushes.LightGray, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "+1"}
            subtractBarRVMinMove = New Button With {.Background = Brushes.LightGray, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "-1"}
            barRVBaseValue = New Button With {.Background = New SolidColorBrush(Color.FromArgb(255, 230, 235, 255)), .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "-1"}
            currentBarRVPopupBtn = New Button With {.Background = Brushes.Cyan, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19}

            currentRBPopupBtn = New Button With {.Background = Brushes.Red, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19}
            addRBMinMove = New Button With {.Background = Brushes.LightGray, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "+1"}
            subtractRBMinMove = New Button With {.Background = Brushes.LightGray, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "-1"}


            rvPopup = New Popup With {.Placement = PlacementMode.Left, .PlacementTarget = currentRVPopupBtn, .Width = 520, .Height = 310, .StaysOpen = False}
            rvPopupGrid = New Grid With {.Background = Brushes.White}
            rvPopup.Child = rvPopupGrid
            For x = 1 To 10 : rvPopupGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)}) : Next
            For y = 1 To 12 : rvPopupGrid.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Star)}) : Next

            barRVPopup = New Popup With {.Placement = PlacementMode.Left, .PlacementTarget = currentBarRVPopupBtn, .Width = 520, .Height = 310, .StaysOpen = False}
            barRVPopupGrid = New Grid With {.Background = Brushes.White}
            barRVPopup.Child = barRVPopupGrid
            For x = 1 To 10 : barRVPopupGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)}) : Next
            For y = 1 To 12 : barRVPopupGrid.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Star)}) : Next

            popupRB = New Popup With {.Placement = PlacementMode.Left, .PlacementTarget = currentRBPopupBtn, .Width = 520, .Height = 310, .StaysOpen = False}
            popupRBGrid = New Grid With {.Background = Brushes.White}
            popupRB.Child = popupRBGrid
            For x = 1 To 10 : popupRBGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)}) : Next
            For y = 1 To 12 : popupRBGrid.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Star)}) : Next

        End Sub
        Sub AddHandlers()
            AddHandler currentRVPopupBtn.Click,
                Sub()
                    rvPopup.IsOpen = True
                End Sub

            AddHandler rvPopup.Opened,
              Sub()
                  rvPopupGrid.Children.Clear()
                  For x = 1 To 10
                      For y = 1 To 12
                          Dim value = Round(((y - 1) * 10 + (1 * Chart.Settings("RangeValue").Value) / Chart.GetMinTick() + x - 1) * Chart.GetMinTick(), 4)
                          Dim btn As New Button With {.Margin = New Thickness(1), .Tag = value, .Content = FormatNumberLengthAndPrefix(value, Chart.Settings("DecimalPlaces").Value), .FontSize = 14.5, .Foreground = Brushes.Black}
                          Grid.SetRow(btn, y - 1)
                          Grid.SetColumn(btn, x - 1)
                          btn.Background = Brushes.White
                          If Round(PriceRV, 4) = value Then
                              btn.Background = Brushes.LightBlue
                          End If
                          If value = Round(RoundTo(Chart.Settings("RangeValue").Value * BasePriceRVMultiplier, Chart.GetMinTick), 4) Then
                              btn.Background = New SolidColorBrush(Colors.Orange)
                          End If
                          rvPopupGrid.Children.Add(btn)
                          AddHandler btn.Click,
                                      Sub(sender As Object, e As EventArgs)
                                          rvPopup.IsOpen = False
                                          If Round(CDec(CType(sender, Button).Tag), 4) <> Round(PriceRV, 4) Then
                                              PriceRV = CType(sender, Button).Tag
                                              Chart.ReApplyAnalysisTechnique(Me)
                                          End If
                                      End Sub
                      Next
                  Next
              End Sub

            AddHandler currentBarRVPopupBtn.Click,
                Sub()
                    barRVPopup.IsOpen = True
                End Sub

            AddHandler barRVPopup.Opened,
              Sub()
                  barRVPopupGrid.Children.Clear()
                  For x = 1 To 10
                      For y = 1 To 12
                          Dim value As Integer = (y - 1) * 10 + x
                          Dim btn As New Button With {.Margin = New Thickness(1), .Tag = value, .Content = value, .FontSize = 14.5, .Foreground = Brushes.Black}
                          Grid.SetRow(btn, y - 1)
                          Grid.SetColumn(btn, x - 1)
                          btn.Background = Brushes.White
                          If Round(BarCountRV, 4) = value Then
                              btn.Background = Brushes.LightBlue
                          End If
                          barRVPopupGrid.Children.Add(btn)
                          AddHandler btn.Click,
                                      Sub(sender As Object, e As EventArgs)
                                          barRVPopup.IsOpen = False
                                          If Round(CDec(CType(sender, Button).Tag), 4) <> Round(BarCountRV, 4) Then
                                              BarCountRV = CType(sender, Button).Tag
                                              Chart.ReApplyAnalysisTechnique(Me)
                                          End If
                                      End Sub
                      Next
                  Next
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
                            Dim btn As New Button With {.Margin = New Thickness(1), .Tag = value, .Content = FormatNumberLengthAndPrefix(value, Chart.Settings("DecimalPlaces").Value), .FontSize = 14.5, .Foreground = Brushes.Black}
                            Grid.SetRow(btn, y - 1)
                            Grid.SetColumn(btn, x - 1)
                            btn.Background = Brushes.White
                            If Round(Chart.Settings("RangeValue").Value, 4) = value Then
                                btn.Background = Brushes.Pink
                            ElseIf value = RoundTo(PriceRV / 2, Chart.GetMinTick) Or value = RoundTo(PriceRV / 3, Chart.GetMinTick) Or value = RoundTo(PriceRV / 4, Chart.GetMinTick) Then
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

            AddHandler addBarRVMinMove.Click,
                Sub()
                    BarCountRV += 1
                    Chart.ReApplyAnalysisTechnique(Me)
                End Sub
            AddHandler subtractBarRVMinMove.Click,
                Sub()
                    BarCountRV -= 1
                    Chart.ReApplyAnalysisTechnique(Me)
                End Sub
            AddHandler barRVBaseValue.Click,
                Sub(sender As Object, e As EventArgs)
                    If Round(sender.CommandParameter, 5) <> Round(BarCountRV, 5) Then
                        BarCountRV = sender.CommandParameter
                        Chart.ReApplyAnalysisTechnique(Me)
                    End If
                End Sub
            AddHandler addRVMinMove.Click,
                Sub()
                    PriceRV += Chart.GetMinTick()
                    Chart.ReApplyAnalysisTechnique(Me)
                End Sub
            AddHandler subtractRVMinMove.Click,
                Sub()
                    PriceRV -= Chart.GetMinTick()
                    Chart.ReApplyAnalysisTechnique(Me)
                End Sub
            AddHandler rvBaseValue.Click,
                Sub(sender As Object, e As EventArgs)
                    If Round(sender.CommandParameter, 5) <> Round(PriceRV, 5) Then
                        PriceRV = sender.CommandParameter
                        Chart.ReApplyAnalysisTechnique(Me)
                    End If
                End Sub
            AddHandler addRBMinMove.Click,
                Sub()
                    Chart.ChangeRange(Round(Chart.Settings("RangeValue").Value + Chart.GetMinTick(), 4))
                End Sub
            AddHandler subtractRBMinMove.Click,
                Sub()
                    Chart.ChangeRange(Round(Chart.Settings("RangeValue").Value - Chart.GetMinTick(), 4))
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
                addRVMinMove.Background = Brushes.LightGray
                subtractRVMinMove.Background = Brushes.LightGray

                rvBaseValue.Content = FormatNumberLengthAndPrefix(RoundTo(Chart.Settings("RangeValue").Value * BasePriceRVMultiplier, Chart.GetMinTick), Chart.Settings("DecimalPlaces").Value)
                rvBaseValue.CommandParameter = Round(RoundTo(Chart.Settings("RangeValue").Value * BasePriceRVMultiplier, Chart.GetMinTick), Chart.Settings("DecimalPlaces").Value)
                rvBaseValue.Background = Brushes.White

                currentRVPopupBtn.Content = FormatNumberLengthAndPrefix(PriceRV, Chart.Settings("DecimalPlaces").Value)
                currentRVPopupBtn.Background = Brushes.LightBlue

                addBarRVMinMove.Background = Brushes.LightGray
                subtractBarRVMinMove.Background = Brushes.LightGray

                barRVBaseValue.Content = BaseBarCountRV
                barRVBaseValue.CommandParameter = BaseBarCountRV
                barRVBaseValue.Background = Brushes.White

                currentBarRVPopupBtn.Content = BarCountRV
                currentBarRVPopupBtn.Background = Brushes.LightBlue

                currentRBPopupBtn.Content = FormatNumberLengthAndPrefix(Chart.Settings("RangeValue").Value, Chart.Settings("DecimalPlaces").Value)
                currentRBPopupBtn.Background = Brushes.Pink

                addRBMinMove.Background = Brushes.LightGray
                subtractRBMinMove.Background = Brushes.LightGray
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






