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

    Public Class RegressionTrend
        Inherits AnalysisTechnique
        Implements IChartPadAnalysisTechnique
#Region "AnalysisTechnique Inherited Code"
        Public Sub New(ByVal chart As Chart)
            MyBase.New(chart) ' Call the base class constructor.
            Description = "Two-swing AutoTrend analysis technique."
            Name = "RegressionTrend"
            If chart IsNot Nothing Then
                AddHandler chart.ChartKeyDown, AddressOf KeyPress
                AddHandler chart.ChartMouseDraggedEvent, AddressOf ChartMouseDrag
                AddHandler chart.ChartMouseDownEvent, AddressOf ChartMouseDrag
                AddHandler chart.ChartMouseUpEvent,
                    Sub()
                        doubleClick = False
                    End Sub
                AddHandler chart.ChartMouseDoubleClickEvent,
                    Sub(sender As Object, location As Point)
                        doubleClick = True
                        StartLocation = location.X
                        DrawLines(True)
                    End Sub
            End If
        End Sub
#End Region
        Private doubleClick As Boolean

        Public Overrides Property Name As String = "RegressionTrend"


#Region "Inputs"


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
        <Input(, "ABC")>
        Public Property AbcChannelAndTextColor As Color = (New ColorConverter).ConvertFrom("#FF008000")
        <Input()> Public Property AbcSwingLineThickness As Decimal = 0.7
        <Input()> Public Property AbcSwingChannelThickness As Decimal = 1
        '<Input()> Public Property AbcLengthTextFontSize As Double = 11
        '<Input()> Public Property AbcLengthTextFontWeight As FontWeight = FontWeights.Bold
        <Input()> Public Property AbcTargetTextFontSize As Double = 12
        <Input()> Public Property AbcTargetTextFontWeight As FontWeight = FontWeights.Bold
        <Input()> Public Property BCTargetFontSize As Double = 12
        <Input()> Public Property BCTargetTextFontWeight As FontWeight = FontWeights.Bold
        <Input()> Public Property ShadedBoxUpColor As Color = Colors.Green
        <Input()> Public Property ShadedBoxDownColor As Color = Colors.Green
        <Input> Public Property ShadedLadderWidth As Decimal = 25
        '<Input()> Public Property BCTargetThickness As Decimal = 2
        '<Input()> Public Property BCTargetWidth As Decimal = 8
        '<Input()> Public Property BCTargetLeftOffset As Decimal = 4

        Private _rv As Decimal = 2


        <Input(, "Trend")> Public Property TrendChannelThickness As Decimal = 0.7
        <Input()> Public Property PotentialTrendChannelThickness As Decimal
        '<Input()> Public Property PotentialTrendChannel2Thickness As Decimal
        <Input()> Public Property TrendChannelUpColor As Color = (New ColorConverter).ConvertFrom("#FF004C00")
        <Input()> Public Property TrendChannelDownColor As Color = (New ColorConverter).ConvertFrom("#FF7F0000")
        <Input()> Public Property PotentialTrendChannelUpColor As Color
        <Input()> Public Property PotentialTrendChannelDownColor As Color
        '<Input()> Public Property TrendLengthTextFontSize As Double = 14
        '<Input()> Public Property TrendLengthTextFontWeight As FontWeight = FontWeights.Bold
        <Input> Public Property ChannelBarsBack As Integer = 20

        <Input(, "Bar Coloring")>
        Public Property UpColor As Color = (New ColorConverter).ConvertFrom("#FF00B200")
        <Input()> Public Property NeutralColor As Color = (New ColorConverter).ConvertFrom("#FF000000")
        <Input()> Public Property DownColor As Color = (New ColorConverter).ConvertFrom("#FFB20000")
        <Input()> Public Property TargetTextPercentMultiplier As Decimal = 1





#End Region


        Private SwingChannel As TrendLine
        Dim ConfirmedChannel As TrendLine
        Dim SwingPotentialLine1 As TrendLine
        'Private TargetBCLength As TrendLine
        'Private SwingPotentialLine2 As TrendLine

        'manual
        Private ASwing As TrendLine
        Private BSwing As TrendLine
        Private CSwing As TrendLine
        Private PotentialBSwing As TrendLine
        Private PotentialASwing As TrendLine
        Dim targetTexts As List(Of Label)
        Dim shadedBox1 As Rectangle



        Private SwingChannel2 As TrendLine
        Private ConfirmedChannel2 As TrendLine
        Private SwingPotentialLine12 As TrendLine
        'Private TargetBCLength2 As TrendLine
        Private ASwing2 As TrendLine
        Private BSwing2 As TrendLine
        Private CSwing2 As TrendLine
        Private PotentialBSwing2 As TrendLine
        Private PotentialASwing2 As TrendLine
        Private targetTexts2 As List(Of Label)
        Dim shadedBox2 As Rectangle

        'Private ALengthText As Label
        'Private BLengthText As Label
        'Private CLengthText As Label
        'Private PotentialBSwingText As Label
        'Private TrendLengthText As Label
        'Dim NeutralBarsText As Label

        Private upperRangeLine As TrendLine
        Private lowerRangeLine As TrendLine
        'Private zoneUpperLine As TrendLine
        'Private zoneLowerLine As TrendLine
        'Private upperZone As Plot
        'Private lowerZone As Plot
        Const textPrefix As String = "── "

        Public Overrides Sub Reset()
            MyBase.Reset()
        End Sub

        Function FormatNumberLengthAndPrefix(num As Decimal, Optional placesAfterDecimal As Integer = -1) As String
            If placesAfterDecimal = -1 Then placesAfterDecimal = Chart.Settings("DecimalPlaces").Value
            Return Round(num * (10 ^ placesAfterDecimal))
        End Function

        Protected Sub KeyPress(ByVal sender As Object, ByVal e As KeyEventArgs)
            Dim key As Key
            If e.SystemKey = Key.None Then
                key = e.Key
            Else
                key = e.SystemKey
            End If

        End Sub
        Private Sub ChartMouseDrag(sender As Object, location As Point)
            If Keyboard.IsKeyDown(Key.LeftShift) Or Keyboard.GetKeyStates(Key.CapsLock) = KeyStates.Toggled Or doubleClick Then
                StartLocation = location.X
                DrawLines(True)
            End If
        End Sub

        Protected Overrides Sub Begin()
            MyBase.Begin()
            StartLocation = Chart.bars.Count - ChannelBarsBack
            ConfirmedChannel = NewTrendLine(Colors.Red, True)
            SwingPotentialLine1 = NewTrendLine(Colors.Red, True)
            ConfirmedChannel2 = NewTrendLine(Colors.Red, True)
            SwingPotentialLine12 = NewTrendLine(Colors.Red, True)
            'SwingPotentialLine2 = NewTrendLine(Colors.Red, True)
            Dim lines() = {SwingPotentialLine1, ConfirmedChannel, SwingPotentialLine12, ConfirmedChannel2}
            For Each line In lines
                line.Pen = New Pen(New SolidColorBrush(Colors.Red), 0)
                line.IsRegressionLine = True : line.HasParallel = True : line.IsSelectable = False : line.IsEditable = False : line.ExtendRight = True
            Next
            ASwing = NewTrendLine(Colors.Gray, True) : ASwing.IsEditable = False : ASwing.IsSelectable = False : ASwing.Pen.Thickness = AbcSwingLineThickness
            BSwing = NewTrendLine(Colors.Gray, True) : BSwing.IsEditable = False : BSwing.IsSelectable = False : BSwing.Pen.Thickness = AbcSwingLineThickness
            CSwing = NewTrendLine(Colors.Gray, True) : CSwing.IsEditable = False : CSwing.IsSelectable = False : CSwing.Pen.Thickness = AbcSwingLineThickness
            PotentialASwing = NewTrendLine(Colors.Gray, True) : PotentialASwing.IsEditable = False : PotentialASwing.IsSelectable = False : PotentialASwing.Pen.Thickness = AbcSwingLineThickness
            PotentialBSwing = NewTrendLine(Colors.Gray, True) : PotentialBSwing.IsEditable = False : PotentialBSwing.IsSelectable = False : PotentialBSwing.Pen.Thickness = AbcSwingLineThickness
            SwingChannel = NewTrendLine(AbcChannelAndTextColor, True) : SwingChannel.IsEditable = False : SwingChannel.IsSelectable = False : SwingChannel.Pen.Thickness = 0 : SwingChannel.OuterPen.Thickness = AbcSwingChannelThickness : SwingChannel.IsRegressionLine = True : SwingChannel.ExtendRight = True : SwingChannel.HasParallel = True
            'TargetBCLength = NewTrendLine(TrendChannelDownColor, True) : TargetBCLength.IsEditable = False : TargetBCLength.IsSelectable = False : TargetBCLength.Pen.Thickness = AbcSwingLineThickness
            ASwing2 = NewTrendLine(Colors.Gray, True) : ASwing2.IsEditable = False : ASwing2.IsSelectable = False : ASwing2.Pen.Thickness = AbcSwingLineThickness
            BSwing2 = NewTrendLine(Colors.Gray, True) : BSwing2.IsEditable = False : BSwing2.IsSelectable = False : BSwing2.Pen.Thickness = AbcSwingLineThickness
            CSwing2 = NewTrendLine(Colors.Gray, True) : CSwing2.IsEditable = False : CSwing2.IsSelectable = False : CSwing2.Pen.Thickness = AbcSwingLineThickness
            PotentialASwing2 = NewTrendLine(Colors.Gray, True) : PotentialASwing2.IsEditable = False : PotentialASwing2.IsSelectable = False : PotentialASwing2.Pen.Thickness = AbcSwingLineThickness
            PotentialBSwing2 = NewTrendLine(Colors.Gray, True) : PotentialBSwing2.IsEditable = False : PotentialBSwing2.IsSelectable = False : PotentialBSwing2.Pen.Thickness = AbcSwingLineThickness
            SwingChannel2 = NewTrendLine(AbcChannelAndTextColor, True) : SwingChannel2.IsEditable = False : SwingChannel2.IsSelectable = False : SwingChannel2.Pen.Thickness = 0 : SwingChannel2.OuterPen.Thickness = AbcSwingChannelThickness : SwingChannel2.IsRegressionLine = True : SwingChannel2.ExtendRight = True : SwingChannel2.HasParallel = True
            'TargetBCLength2 = NewTrendLine(TrendChannelDownColor, True) : TargetBCLength2.IsEditable = False : TargetBCLength2.IsSelectable = False : TargetBCLength2.Pen.Thickness = AbcSwingLineThickness

            shadedBox1 = NewRectangle(Colors.Transparent, Colors.Red, New Point(0, 0), New Point(0, 0)) : shadedBox1.IsEditable = False : shadedBox1.IsSelectable = False
            shadedBox2 = NewRectangle(Colors.Transparent, Colors.Red, New Point(0, 0), New Point(0, 0)) : shadedBox2.IsEditable = False : shadedBox2.IsSelectable = False


            'ALengthText = NewLabel("", AbcChannelAndTextColor, New Point(0, 0),, New Font With {.FontSize = AbcLengthTextFontSize, .FontWeight = AbcLengthTextFontWeight}, False) : ALengthText.HorizontalAlignment = LabelHorizontalAlignment.Right
            'BLengthText = NewLabel("", AbcChannelAndTextColor, New Point(0, 0),, New Font With {.FontSize = AbcLengthTextFontSize, .FontWeight = AbcLengthTextFontWeight}, False) : BLengthText.HorizontalAlignment = LabelHorizontalAlignment.Right
            'CLengthText = NewLabel("", AbcChannelAndTextColor, New Point(0, 0),, New Font With {.FontSize = AbcLengthTextFontSize, .FontWeight = AbcLengthTextFontWeight}, False) : CLengthText.HorizontalAlignment = LabelHorizontalAlignment.Right
            'TrendLengthText = NewLabel("", TrendChannelDownColor, New Point(0, 0),, New Font With {.FontSize = TrendLengthTextFontSize, .FontWeight = TrendLengthTextFontWeight}, False) : TrendLengthText.HorizontalAlignment = LabelHorizontalAlignment.Right
            'PotentialBSwingText = NewLabel("", AbcChannelAndTextColor, New Point(0, 0),, New Font With {.FontSize = AbcLengthTextFontSize, .FontWeight = AbcLengthTextFontWeight}, False) : PotentialBSwingText.HorizontalAlignment = LabelHorizontalAlignment.Right
            upperRangeLine = NewTrendLine(TrendChannelUpColor, True) : upperRangeLine.IsEditable = False : upperRangeLine.IsSelectable = False : upperRangeLine.Pen.Thickness = AbcSwingLineThickness : upperRangeLine.ExtendRight = True
            lowerRangeLine = NewTrendLine(TrendChannelDownColor, True) : lowerRangeLine.IsEditable = False : lowerRangeLine.IsSelectable = False : lowerRangeLine.Pen.Thickness = AbcSwingLineThickness : lowerRangeLine.ExtendRight = True
            'NeutralBarsText = NewLabel("", AbcChannelAndTextColor, New Point(0, 0),, New Font With {.FontSize = AbcLengthTextFontSize, .FontWeight = AbcLengthTextFontWeight}, False) : NeutralBarsText.VerticalAlignment = LabelVerticalAlignment.Center
            'upperZone = NewPlot(ZoneLineColor) : upperZone.Pen.Thickness = ZoneLineThickness
            'lowerZone = NewPlot(ZoneLineColor) : lowerZone.Pen.Thickness = ZoneLineThickness
            'zoneUpperLine = NewTrendLine(ZoneLineColor, True) : zoneUpperLine.IsEditable = False : zoneUpperLine.IsSelectable = False : zoneUpperLine.Pen.Thickness = ZoneLineThickness : zoneUpperLine.ExtendRight = True
            'zoneLowerLine = NewTrendLine(ZoneLineColor, True) : zoneLowerLine.IsEditable = False : zoneLowerLine.IsSelectable = False : zoneLowerLine.Pen.Thickness = ZoneLineThickness : zoneLowerLine.ExtendRight = True

            targetTexts = New List(Of Label)
            targetTexts2 = New List(Of Label)
            previousPosition = 0
            DrawLines(False)

            'If Chart.bars.Count > 0 Then
            '    Dim swingDir As Boolean
            '    Dim swing As LineCoordinates
            '    Dim rv As Decimal = Chart.Settings("RangeValue").Value * Stickiness
            '    pivots = New List(Of Point)
            '    pivots.Add(New Point(1, Chart.bars(0).Data.High))
            '    For i = 0 To Chart.bars.Count - 1
            '        If Chart.bars(i).Data.High - swing.EndPoint.Y >= rv AndAlso Chart.bars(i).Data.Number <> swing.EndPoint.X AndAlso swingDir = False Then
            '            swingDir = True
            '            swing = New LineCoordinates(swing.EndPoint, New Point(i + 1, Chart.bars(i).Data.High))
            '            pivots.Add(swing.EndPoint)
            '        ElseIf swing.EndPoint.Y - Chart.bars(i).Data.Low >= rv AndAlso Chart.bars(i).Data.Number <> swing.EndPoint.X AndAlso swingDir = True Then
            '            swingDir = False
            '            swing = New LineCoordinates(swing.EndPoint, New Point(i + 1, Chart.bars(i).Data.Low))
            '            pivots.Add(swing.EndPoint)
            '        ElseIf Chart.bars(i).Data.High >= swing.EndPoint.Y And swingDir = True Then
            '            ' extension up
            '            swing.EndPoint = New Point(i + 1, Chart.bars(i).Data.High)
            '            pivots(pivots.Count - 1) = swing.EndPoint
            '        ElseIf Chart.bars(i).Data.Low <= swing.EndPoint.Y And swingDir = False Then
            '            ' extension down
            '            swing.EndPoint = New Point(i + 1, Chart.bars(i).Data.Low)
            '            pivots(pivots.Count - 1) = swing.EndPoint
            '        End If
            '    Next
            'End If
        End Sub
        'Dim pivots As List(Of Point)
        Dim StartLocation As Integer
        Dim CurrentDirection As Direction
        Dim previousPosition As Integer
        Dim retracePoint As Decimal
        Dim retracePoint2 As Decimal
        Dim pivots As List(Of Point)
        Private Sub DrawLines(mouseTriggered As Boolean)
            If Not IsEnabled Then Exit Sub
            If StartLocation >= Chart.bars.Count - 1 Or StartLocation < 0 Or Chart.bars.Count = 0 Then
                StartLocation = Chart.bars.Count - 20
                Exit Sub
            End If

            Dim high As Point = New Point(0, 0)
            Dim low As Point = New Point(0, Decimal.MaxValue)
            For i = StartLocation - 1 To Chart.bars.Count - 1
                If Chart.bars(i).Data.High >= high.Y Then high = New Point(i + 1, Chart.bars(i).Data.High)
                If Chart.bars(i).Data.Low <= low.Y Then low = New Point(i + 1, Chart.bars(i).Data.Low)
            Next
            If low.X < high.X Then StartLocation = low.X Else StartLocation = high.X
            If StartLocation = previousPosition And mouseTriggered Then Exit Sub
            If StartLocation = low.X Then CurrentDirection = Direction.Up Else CurrentDirection = Direction.Down
            retracePoint = Chart.bars(StartLocation - 1).Data.Close
            Dim p = New Point(StartLocation, If(CurrentDirection = Direction.Up, Chart.bars(StartLocation - 1).Data.Low, Chart.bars(StartLocation - 1).Data.High))
            ASwing.Coordinates = New LineCoordinates(p, p)
            BSwing.Coordinates = New LineCoordinates(p, p)
            CSwing.Coordinates = New LineCoordinates(p, p)
            PotentialASwing.Coordinates = New LineCoordinates(p, p)
            PotentialBSwing.Coordinates = New LineCoordinates(p, p)

            For i = StartLocation - 1 To Chart.bars.Count - 1
                SetSwingPoints(Chart.bars(i).Data)
            Next

            ASwing.Pen.Brush = New SolidColorBrush(If(CurrentDirection = Direction.Up, TrendChannelUpColor, TrendChannelDownColor))
            BSwing.Pen.Brush = New SolidColorBrush(If(CurrentDirection = Direction.Up, TrendChannelUpColor, TrendChannelDownColor))
            CSwing.Pen.Brush = New SolidColorBrush(If(CurrentDirection = Direction.Up, TrendChannelUpColor, TrendChannelDownColor))
            PotentialASwing.Pen.Brush = New SolidColorBrush(If(CurrentDirection = Direction.Up, PotentialTrendChannelUpColor, PotentialTrendChannelDownColor))
            PotentialBSwing.Pen.Brush = New SolidColorBrush(If(CurrentDirection = Direction.Up, PotentialTrendChannelUpColor, PotentialTrendChannelDownColor))
            'ALengthText.Location = ASwing.EndPoint
            'BLengthText.Location = BSwing.EndPoint
            'CLengthText.Location = CSwing.EndPoint

            'ALengthText.VerticalAlignment = If(CurrentDirection = Direction.Down, LabelVerticalAlignment.Top, LabelVerticalAlignment.Bottom)
            'BLengthText.VerticalAlignment = If(CurrentDirection = Direction.Down, LabelVerticalAlignment.Bottom, LabelVerticalAlignment.Top)
            'CLengthText.VerticalAlignment = If(CurrentDirection = Direction.Down, LabelVerticalAlignment.Top, LabelVerticalAlignment.Bottom)

            For Each item In targetTexts
                RemoveObjectFromChart(item)
            Next
            targetTexts.Clear()
            shadedBox1.Coordinates = New Rect(0, 0, 0, 0)
            Dim percentBack As Decimal
            Dim [step] As Decimal = TargetTextPercentMultiplier / 100 ' * Chart.Settings("RangeValue").Value / Abs(CSwing.EndPoint.Y - ASwing.StartPoint.Y)
            If IsConfirmedChannel() Then
                PotentialASwing.Pen.Thickness = 0
                PotentialBSwing.Pen.Thickness = 0

                SwingChannel.Coordinates = New LineCoordinates(CSwing.StartPoint, CSwing.EndPoint)
                SwingChannel.OuterPen.Thickness = AbcSwingChannelThickness
                'PotentialBSwingText.Text = ""

                ASwing.Pen.Thickness = AbcSwingLineThickness
                BSwing.Pen.Thickness = AbcSwingLineThickness
                CSwing.Pen.Thickness = AbcSwingLineThickness

                'ALengthText.Text = FormatNumberLengthAndPrefix(Abs(ASwing.EndPoint.Y - ASwing.StartPoint.Y))
                'BLengthText.Text = FormatNumberLengthAndPrefix(Abs(BSwing.EndPoint.Y - BSwing.StartPoint.Y))
                'CLengthText.Text = FormatNumberLengthAndPrefix(Abs(CSwing.EndPoint.Y - CSwing.StartPoint.Y))

                ConfirmedChannel.StartPoint = ASwing.StartPoint
                ConfirmedChannel.EndPoint = CSwing.EndPoint
                ConfirmedChannel.OuterPen.Thickness = TrendChannelThickness
                ConfirmedChannel.OuterPen.Brush = New SolidColorBrush(If(CurrentDirection = Direction.Up, TrendChannelUpColor, TrendChannelDownColor))

                'SwingPotentialLine2.TL.OuterPen.Thickness = 0
                SwingPotentialLine1.OuterPen.Thickness = 0
                percentBack = Abs(retracePoint - CSwing.EndPoint.Y) / Abs(CSwing.EndPoint.Y - ASwing.StartPoint.Y)

                'TargetBCLength.Coordinates = New LineCoordinates(Chart.bars.Count - BCTargetLeftOffset, CSwing.EndPoint.Y - (BSwing.StartPoint.Y - BSwing.EndPoint.Y), Chart.bars.Count + BCTargetWidth - BCTargetLeftOffset, CSwing.EndPoint.Y - (BSwing.StartPoint.Y - BSwing.EndPoint.Y))
                'TargetBCLength.Pen.Brush = New SolidColorBrush(If(CurrentDirection = Direction.Down, TrendChannelDownColor, TrendChannelUpColor))
                'TargetBCLength.Pen.Thickness = BCTargetThickness
                'upperRangeLine.Pen.Thickness = AbcSwingLineThickness
                'upperRangeLine.Coordinates = New LineCoordinates(New Point(CurrentBar.Number, CSwing.EndPoint.Y), New Point(CurrentBar.Number + 1, CSwing.EndPoint.Y))

                'SwingPotentialLine2.OuterPen.Thickness = 0
            Else
                'if potential channel
                PotentialASwing.Pen.Thickness = AbcSwingLineThickness
                PotentialBSwing.Pen.Thickness = AbcSwingLineThickness
                ASwing.Pen.Thickness = 0
                BSwing.Pen.Thickness = 0
                CSwing.Pen.Thickness = 0
                'ALengthText.Text = ""
                'BLengthText.Text = ""
                'CLengthText.Text = ""
                'PotentialBSwingText.Text = Round(((PotentialBSwing.EndPoint.X - PotentialBSwing.StartPoint.X) / Abs(CSwing.EndPoint.X - ASwing.StartPoint.X)) * 100, 0) & "% " & PotentialBSwing.EndPoint.X - PotentialBSwing.StartPoint.X
                'PotentialBSwingText.Location = PotentialBSwing.EndPoint
                'PotentialBSwingText.VerticalAlignment = If(CurrentDirection = Direction.Down, LabelVerticalAlignment.Bottom, LabelVerticalAlignment.Top)
                SwingChannel.Coordinates = New LineCoordinates(PotentialBSwing.StartPoint, PotentialBSwing.EndPoint)
                SwingChannel.OuterPen.Thickness = 0
                ConfirmedChannel.StartPoint = ASwing.StartPoint
                ConfirmedChannel.EndPoint = CSwing.EndPoint
                ConfirmedChannel.OuterPen.Thickness = TrendChannelThickness
                ConfirmedChannel.OuterPen.Brush = New SolidColorBrush(If(CurrentDirection = Direction.Up, TrendChannelUpColor, TrendChannelDownColor))
                ConfirmedChannel.LockToEnd = False
                SwingPotentialLine1.StartPoint = ASwing.StartPoint
                SwingPotentialLine1.EndPoint = AddToX(FindRangeBar(PotentialASwing.EndPoint.X, Chart.bars.Count - 1, If(CurrentDirection = Direction.Down, True, False)), 1)
                SwingPotentialLine1.OuterPen.Brush = New SolidColorBrush(If(CurrentDirection = Direction.Up, PotentialTrendChannelUpColor, PotentialTrendChannelDownColor))
                SwingPotentialLine1.OuterPen.Thickness = PotentialTrendChannelThickness
                'SwingPotentialLine2.StartPoint = New Point(StartLocation, 0)
                'SwingPotentialLine2.EndPoint = New Point(Chart.bars.Count, 0)
                'SwingPotentialLine2.OuterPen.Brush = New SolidColorBrush(If(CurrentDirection = Direction.Up, PotentialTrendChannelUpColor, PotentialTrendChannelDownColor))
                'SwingPotentialLine2.OuterPen.Thickness = PotentialTrendChannel2Thickness


                percentBack = Abs(PotentialBSwing.StartPoint.Y - PotentialBSwing.EndPoint.Y) / Abs(CSwing.EndPoint.Y - ASwing.StartPoint.Y)
                'TargetBCLength.Pen.Thickness = 0

            End If
            'draw target texts
            If IsConfirmedChannel() Then
                Dim rightAdustment As Integer = 5
                Dim txtCol As Color = If(CurrentDirection = Direction.Up, TrendChannelDownColor, TrendChannelUpColor)
                Dim targetP As Decimal = If(IsConfirmedChannel(), retracePoint, PotentialBSwing.EndPoint.Y)
                For i = Max(percentBack + [step] - (percentBack Mod [step]), [step]) To 1 Step [step]
                    Dim point = New Point(Chart.bars.Count, CSwing.EndPoint.Y - i * (CSwing.EndPoint.Y - ASwing.StartPoint.Y))
                    If Abs(ASwing.StartPoint.Y - point.Y) > 0.75 * Chart.Settings("RangeValue").Value And Abs(point.Y - targetP) > 0.75 * Chart.Settings("RangeValue").Value Then
                        targetTexts.Add(NewLabel(textPrefix & CStr(Round(i * 100, 0)) & "%    " & CStr(FormatNumber(point.Y, Chart.Settings("DecimalPlaces").Value)), txtCol, AddToX(point, rightAdustment),,, False))
                    End If
                Next
                For Each t In targetTexts
                    t.Font.FontWeight = AbcTargetTextFontWeight
                    t.Font.FontSize = AbcTargetTextFontSize
                Next
                targetTexts.Add(NewLabel(textPrefix & "100%  " & CStr(Round(ASwing.StartPoint.Y, Chart.Settings("DecimalPlaces").Value)), txtCol, New Point(Chart.bars.Count + rightAdustment, ASwing.StartPoint.Y),,, False))
                targetTexts(targetTexts.Count - 1).Font.FontWeight = AbcTargetTextFontWeight
                targetTexts(targetTexts.Count - 1).Font.FontSize = AbcTargetTextFontSize
                'draw 0
                If IsConfirmedChannel() Then
                    targetTexts.Add(NewLabel(textPrefix & "0%      " & CStr(Round(CSwing.EndPoint.Y, Chart.Settings("DecimalPlaces").Value)), If(CurrentDirection = Direction.Down, TrendChannelDownColor, TrendChannelUpColor), New Point(Chart.bars.Count + rightAdustment, CSwing.EndPoint.Y),,, False))
                    targetTexts(targetTexts.Count - 1).Font.FontWeight = AbcTargetTextFontWeight
                    targetTexts(targetTexts.Count - 1).Font.FontSize = AbcTargetTextFontSize
                End If
                targetTexts.Add(NewLabel(textPrefix & CStr(FormatNumber(Abs(targetP - CSwing.EndPoint.Y) / Abs(ASwing.StartPoint.Y - CSwing.EndPoint.Y) * 100, 0)) & "%    " & CStr(FormatNumber(targetP, Chart.Settings("DecimalPlaces").Value)), If(CurrentDirection = Direction.Up, TrendChannelDownColor, TrendChannelUpColor), New Point(Chart.bars.Count + rightAdustment, targetP),,, False))
                targetTexts(targetTexts.Count - 1).Font.FontWeight = AbcTargetTextFontWeight
                targetTexts(targetTexts.Count - 1).Font.FontSize = AbcTargetTextFontSize
                If IsConfirmedChannel() Then
                    targetTexts.Add(NewLabel(textPrefix & " " & Round(Abs((BSwing.StartPoint.Y - BSwing.EndPoint.Y) * (10 ^ Chart.Settings("DecimalPlaces").Value))) & " RV " & FormatNumber(CSwing.EndPoint.Y - (BSwing.StartPoint.Y - BSwing.EndPoint.Y), Chart.Settings("DecimalPlaces").Value), If(CurrentDirection = Direction.Down, TrendChannelDownColor, TrendChannelUpColor), New Point(Chart.bars.Count + rightAdustment, CSwing.EndPoint.Y - (BSwing.StartPoint.Y - BSwing.EndPoint.Y)),,, False))
                    targetTexts(targetTexts.Count - 1).Font.FontWeight = BCTargetTextFontWeight
                    targetTexts(targetTexts.Count - 1).Font.FontSize = BCTargetFontSize
                End If
                For Each t In targetTexts
                    t.RefreshVisual()
                Next
                shadedBox1.Coordinates = New Rect(New Point(Chart.bars.Count + rightAdustment, Max(targetP, ASwing.StartPoint.Y)), New Size(Chart.GetRelativeFromRealWidth(ShadedLadderWidth), Abs(targetP - ASwing.StartPoint.Y)))
                shadedBox1.Fill = New SolidColorBrush(If(CurrentDirection = Direction.Up, ShadedBoxDownColor, ShadedBoxUpColor))
            End If
            'draw length text
            'TrendLengthText.Location = AddToY(CSwing.EndPoint, If(CurrentDirection = Direction.Up, 1, -1) * Chart.GetRelativeFromRealHeight(AbcLengthTextFontSize))
            'TrendLengthText.VerticalAlignment = If(CurrentDirection = Direction.Up, LabelVerticalAlignment.Bottom, LabelVerticalAlignment.Top)
            'TrendLengthText.Text = FormatNumberLengthAndPrefix(Abs(CSwing.EndPoint.Y - ASwing.StartPoint.Y))
            'TrendLengthText.Font.Brush = New SolidColorBrush(If(CurrentDirection = Direction.Up, TrendChannelUpColor, TrendChannelDownColor))
            'TrendLengthText.RefreshVisual()
            'color bars
            For i = 0 To Chart.bars.Count - 1
                If Not TypeOf Chart.bars(i).Pen.Brush Is SolidColorBrush Then
                    Chart.bars(i).Pen.Brush = New SolidColorBrush
                End If
                Dim color As Color
                If i >= StartLocation - 1 And i <= CSwing.EndPoint.X - 1 Then
                    color = If(CurrentDirection = Direction.Up, UpColor, DownColor)
                ElseIf i >= StartLocation - 1 Then
                    color = NeutralColor
                Else
                    color = Chart.Settings("Bar Color").Value
                End If
                If CType(Chart.bars(i).Pen.Brush, SolidColorBrush).Color <> color Then
                    Chart.bars(i).Pen.Brush = New SolidColorBrush(color)
                    RefreshObject(Chart.bars(i))
                End If
            Next
            If IsConfirmedChannel() Then
                DeleteSecondLines()
            Else
                retracePoint = Chart.bars(PotentialBSwing.StartPoint.X - 1).Data.Close
                DrawSecondLines(PotentialBSwing.StartPoint.X, Not CurrentDirection)
            End If
            'NeutralBarsText.Location = New Point(CurrentBar.Number + 14, CurrentBar.Close)
            'Dim v = (2550 * E ^ (-64.5 * Chart.Settings("RangeValue").Value) + 20) / ((Chart.bars.Count - ASwing.StartPoint.X) / Abs(CSwing.EndPoint.Y - ASwing.StartPoint.Y))
            'NeutralBarsText.Text = Chart.bars.Count - CSwing.EndPoint.X

            ChannelBarsBack = Chart.bars.Count - StartLocation
            previousPosition = StartLocation
        End Sub
        Private Sub DeleteSecondLines()
            ASwing2.Pen.Thickness = 0
            BSwing2.Pen.Thickness = 0
            CSwing2.Pen.Thickness = 0
            PotentialASwing2.Pen.Thickness = 0
            PotentialBSwing2.Pen.Thickness = 0
            SwingChannel2.OuterPen.Thickness = 0
            'TargetBCLength2.Pen.Thickness = 0
            ConfirmedChannel2.OuterPen.Thickness = 0
            SwingPotentialLine12.OuterPen.Thickness = 0
            For Each item In targetTexts2
                RemoveObjectFromChart(item)
            Next
            targetTexts2.Clear()
            shadedBox2.Fill = Brushes.Transparent
        End Sub
        Private Sub DrawSecondLines(location As Integer, direction As Direction)
            ASwing2.Pen.Thickness = AbcSwingLineThickness
            BSwing2.Pen.Thickness = AbcSwingLineThickness
            CSwing2.Pen.Thickness = AbcSwingLineThickness
            PotentialASwing2.Pen.Thickness = AbcSwingLineThickness
            PotentialBSwing2.Pen.Thickness = AbcSwingLineThickness
            SwingChannel2.OuterPen.Thickness = AbcSwingChannelThickness
            ConfirmedChannel2.OuterPen.Thickness = TrendChannelThickness
            SwingPotentialLine12.OuterPen.Thickness = PotentialTrendChannelThickness
            Dim p = New Point(location, If(direction = Direction.Up, Chart.bars(location - 1).Data.Low, Chart.bars(location - 1).Data.High))
            ASwing2.Coordinates = New LineCoordinates(p, p)
            BSwing2.Coordinates = New LineCoordinates(p, p)
            CSwing2.Coordinates = New LineCoordinates(p, p)
            PotentialASwing2.Coordinates = New LineCoordinates(p, p)
            PotentialBSwing2.Coordinates = New LineCoordinates(p, p)

            For i = location - 1 To Chart.bars.Count - 1
                SetSwingPoints2(Chart.bars(i).Data, direction)
            Next

            ASwing2.Pen.Brush = New SolidColorBrush(If(direction = Direction.Up, TrendChannelUpColor, TrendChannelDownColor))
            BSwing2.Pen.Brush = New SolidColorBrush(If(direction = Direction.Up, TrendChannelUpColor, TrendChannelDownColor))
            CSwing2.Pen.Brush = New SolidColorBrush(If(direction = Direction.Up, TrendChannelUpColor, TrendChannelDownColor))
            PotentialASwing2.Pen.Brush = New SolidColorBrush(If(direction = Direction.Up, PotentialTrendChannelUpColor, PotentialTrendChannelDownColor))
            PotentialBSwing2.Pen.Brush = New SolidColorBrush(If(direction = Direction.Up, PotentialTrendChannelUpColor, PotentialTrendChannelDownColor))
            'ALengthText.Location = ASwing.EndPoint
            'BLengthText.Location = BSwing.EndPoint
            'CLengthText.Location = CSwing.EndPoint

            'ALengthText.VerticalAlignment = If(direction = Direction.Down, LabelVerticalAlignment.Top, LabelVerticalAlignment.Bottom)
            'BLengthText.VerticalAlignment = If(direction = Direction.Down, LabelVerticalAlignment.Bottom, LabelVerticalAlignment.Top)
            'CLengthText.VerticalAlignment = If(direction = Direction.Down, LabelVerticalAlignment.Top, LabelVerticalAlignment.Bottom)

            For Each item In targetTexts2
                RemoveObjectFromChart(item)
            Next
            targetTexts2.Clear()
            Dim percentBack As Decimal
            Dim [step] As Decimal = TargetTextPercentMultiplier / 100 ' * Chart.Settings("RangeValue").Value / Abs(CSwing2.EndPoint.Y - ASwing2.StartPoint.Y)
            If PotentialBSwing2.EndPoint.X = PotentialBSwing2.StartPoint.X Then
                PotentialASwing2.Pen.Thickness = 0
                PotentialBSwing2.Pen.Thickness = 0

                SwingChannel2.Coordinates = New LineCoordinates(CSwing2.StartPoint, CSwing2.EndPoint)

                'PotentialBSwingText.Text = ""

                ASwing2.Pen.Thickness = AbcSwingLineThickness
                BSwing2.Pen.Thickness = AbcSwingLineThickness
                CSwing2.Pen.Thickness = AbcSwingLineThickness

                'ALengthText.Text = ASwing.EndPoint.X - ASwing.StartPoint.X
                'BLengthText.Text = BSwing.EndPoint.X - BSwing.StartPoint.X
                'CLengthText.Text = CSwing.EndPoint.X - CSwing.StartPoint.X

                ConfirmedChannel2.StartPoint = ASwing2.StartPoint
                ConfirmedChannel2.EndPoint = CSwing2.EndPoint
                ConfirmedChannel2.OuterPen.Thickness = TrendChannelThickness
                ConfirmedChannel2.OuterPen.Brush = New SolidColorBrush(If(direction = Direction.Up, TrendChannelUpColor, TrendChannelDownColor))

                'SwingPotentialLine2.TL.OuterPen.Thickness = 0
                SwingPotentialLine12.OuterPen.Thickness = 0
                percentBack = Abs(retracePoint2 - CSwing2.EndPoint.Y) / Abs(CSwing2.EndPoint.Y - ASwing2.StartPoint.Y)

                'TargetBCLength2.Coordinates = New LineCoordinates(Chart.bars.Count - BCTargetLeftOffset, CSwing2.EndPoint.Y - (BSwing2.StartPoint.Y - BSwing2.EndPoint.Y), Chart.bars.Count + BCTargetWidth - BCTargetLeftOffset, CSwing2.EndPoint.Y - (BSwing2.StartPoint.Y - BSwing2.EndPoint.Y))
                'TargetBCLength2.Pen.Brush = New SolidColorBrush(If(direction = Direction.Down, TrendChannelDownColor, TrendChannelUpColor))
                'TargetBCLength2.Pen.Thickness = BCTargetThickness
                'SwingPotentialLine2.OuterPen.Thickness = 0

            Else
                'if potential channel
                PotentialASwing2.Pen.Thickness = AbcSwingLineThickness
                PotentialBSwing2.Pen.Thickness = AbcSwingLineThickness
                ASwing2.Pen.Thickness = 0
                BSwing2.Pen.Thickness = 0
                CSwing2.Pen.Thickness = 0
                'ALengthText.Text = ""
                'BLengthText.Text = ""
                'CLengthText.Text = ""
                'PotentialBSwingText.Text = Round(((PotentialBSwing.EndPoint.X - PotentialBSwing.StartPoint.X) / Abs(CSwing.EndPoint.X - ASwing.StartPoint.X)) * 100, 0) & "% " & PotentialBSwing.EndPoint.X - PotentialBSwing.StartPoint.X
                'PotentialBSwingText.Location = PotentialBSwing.EndPoint
                'PotentialBSwingText.VerticalAlignment = If(CurrentDirection = Direction.Down, LabelVerticalAlignment.Bottom, LabelVerticalAlignment.Top)
                SwingChannel2.Coordinates = New LineCoordinates(PotentialBSwing2.StartPoint, PotentialBSwing2.EndPoint)
                SwingChannel2.OuterPen.Thickness = AbcSwingChannelThickness
                ConfirmedChannel2.StartPoint = ASwing2.StartPoint
                ConfirmedChannel2.EndPoint = CSwing2.EndPoint
                ConfirmedChannel2.OuterPen.Thickness = TrendChannelThickness
                ConfirmedChannel2.OuterPen.Brush = New SolidColorBrush(If(direction = Direction.Up, TrendChannelUpColor, TrendChannelDownColor))
                ConfirmedChannel2.LockToEnd = False
                SwingPotentialLine12.StartPoint = ASwing2.StartPoint
                SwingPotentialLine12.EndPoint = AddToX(FindRangeBar(PotentialASwing2.EndPoint.X, Chart.bars.Count - 1, If(direction = Direction.Down, True, False)), 1)
                SwingPotentialLine12.OuterPen.Brush = New SolidColorBrush(If(direction = Direction.Up, PotentialTrendChannelUpColor, PotentialTrendChannelDownColor))
                SwingPotentialLine12.OuterPen.Thickness = PotentialTrendChannelThickness
                'SwingPotentialLine2.StartPoint = New Point(location, 0)
                'SwingPotentialLine2.EndPoint = New Point(Chart.bars.Count, 0)
                'SwingPotentialLine2.OuterPen.Brush = New SolidColorBrush(If(CurrentDirection = Direction.Up, PotentialTrendChannelUpColor, PotentialTrendChannelDownColor))
                'SwingPotentialLine2.OuterPen.Thickness = PotentialTrendChannel2Thickness


                percentBack = Abs(PotentialBSwing2.StartPoint.Y - PotentialBSwing2.EndPoint.Y) / Abs(CSwing2.EndPoint.Y - ASwing2.StartPoint.Y)
                'TargetBCLength2.Pen.Thickness = 0

            End If
            'draw target texts
            Dim rightAdustment As Integer = 5
            Dim txtCol As Color = If(direction = Direction.Up, TrendChannelDownColor, TrendChannelUpColor)
            Dim targetP As Decimal = If(PotentialBSwing2.EndPoint.X = PotentialBSwing2.StartPoint.X, retracePoint2, PotentialBSwing2.EndPoint.Y)
            For i = Max(percentBack + [step] - (percentBack Mod [step]), [step]) To 1 Step [step]
                Dim point = New Point(Chart.bars.Count, CSwing2.EndPoint.Y - i * (CSwing2.EndPoint.Y - ASwing2.StartPoint.Y))
                If Abs(ASwing2.StartPoint.Y - point.Y) > 0.75 * Chart.Settings("RangeValue").Value And Abs(point.Y - targetP) > 0.75 * Chart.Settings("RangeValue").Value Then
                    targetTexts2.Add(NewLabel(textPrefix & CStr(Round(i * 100, 0)) & "%    " & CStr(FormatNumber(point.Y, Chart.Settings("DecimalPlaces").Value)), txtCol, AddToX(point, rightAdustment),,, False))
                End If
            Next

            For Each t In targetTexts2
                t.Font.FontWeight = AbcTargetTextFontWeight
                t.Font.FontSize = AbcTargetTextFontSize
            Next
            targetTexts2.Add(NewLabel(textPrefix & "100%  " & CStr(Round(ASwing2.StartPoint.Y, Chart.Settings("DecimalPlaces").Value)), txtCol, New Point(Chart.bars.Count + rightAdustment, ASwing2.StartPoint.Y),,, False))
            targetTexts2(targetTexts2.Count - 1).Font.FontWeight = AbcTargetTextFontWeight
            targetTexts2(targetTexts2.Count - 1).Font.FontSize = AbcTargetTextFontSize
            shadedBox2.Coordinates = New Rect(New Point(Chart.bars.Count + rightAdustment, Max(targetP, ASwing2.StartPoint.Y)), New Size(Chart.GetRelativeFromRealWidth(ShadedLadderWidth), Abs(targetP - ASwing2.StartPoint.Y)))
            shadedBox2.Fill = New SolidColorBrush(If(direction = Direction.Up, ShadedBoxDownColor, ShadedBoxUpColor))

            'draw 0
            targetTexts2.Add(NewLabel(textPrefix & "0%      " & CStr(Round(CSwing2.EndPoint.Y, Chart.Settings("DecimalPlaces").Value)), If(direction = Direction.Down, TrendChannelDownColor, TrendChannelUpColor), New Point(Chart.bars.Count + rightAdustment, CSwing2.EndPoint.Y),,, False))
            targetTexts2(targetTexts2.Count - 1).Font.FontWeight = AbcTargetTextFontWeight
            targetTexts2(targetTexts2.Count - 1).Font.FontSize = AbcTargetTextFontSize
            targetTexts2.Add(NewLabel(textPrefix & CStr(Round(Abs(targetP - CSwing2.EndPoint.Y) / Abs(ASwing2.StartPoint.Y - CSwing2.EndPoint.Y) * 100, 0)) & "%    " & CStr(FormatNumber(targetP, Chart.Settings("DecimalPlaces").Value)), If(direction = Direction.Up, TrendChannelDownColor, TrendChannelUpColor), New Point(Chart.bars.Count + rightAdustment, targetP),,, False))
            targetTexts2(targetTexts2.Count - 1).Font.FontWeight = AbcTargetTextFontWeight
            targetTexts2(targetTexts2.Count - 1).Font.FontSize = AbcTargetTextFontSize
            If PotentialBSwing2.EndPoint.X = PotentialBSwing2.StartPoint.X Then
                targetTexts2.Add(NewLabel(textPrefix & " " & Round(Abs((BSwing2.StartPoint.Y - BSwing2.EndPoint.Y) * (10 ^ Chart.Settings("DecimalPlaces").Value))) & " RV " & FormatNumber(CSwing2.EndPoint.Y - (BSwing2.StartPoint.Y - BSwing2.EndPoint.Y), Chart.Settings("DecimalPlaces").Value), If(direction = Direction.Down, TrendChannelDownColor, TrendChannelUpColor), New Point(Chart.bars.Count + rightAdustment, CSwing2.EndPoint.Y - (BSwing2.StartPoint.Y - BSwing2.EndPoint.Y)),,, False))
                targetTexts2(targetTexts2.Count - 1).Font.FontWeight = BCTargetTextFontWeight
                targetTexts2(targetTexts2.Count - 1).Font.FontSize = BCTargetFontSize
            End If
            For Each t In targetTexts2
                t.RefreshVisual()
            Next
            ''draw length text
            'TrendLengthText.Location = AddToY(CSwing.EndPoint, If(CurrentDirection = Direction.Up, 1, -1) * Chart.GetRelativeFromRealHeight(AbcLengthTextFontSize))
            'TrendLengthText.VerticalAlignment = If(CurrentDirection = Direction.Up, LabelVerticalAlignment.Bottom, LabelVerticalAlignment.Top)
            'TrendLengthText.Text = Abs(CSwing.EndPoint.X - ASwing.StartPoint.X)
            'TrendLengthText.Font.Brush = New SolidColorBrush(If(CurrentDirection = Direction.Up, TrendChannelUpColor, TrendChannelDownColor))
            'TrendLengthText.RefreshVisual()
            'color bars
            For i = location To Chart.bars.Count - 1
                If Not TypeOf Chart.bars(i).Pen.Brush Is SolidColorBrush Then
                    Chart.bars(i).Pen.Brush = New SolidColorBrush
                End If
                Dim color As Color
                If i >= location - 1 And i <= CSwing2.EndPoint.X - 1 Then
                    color = If(direction = Direction.Up, UpColor, DownColor)
                ElseIf i >= location - 1 Then
                    color = NeutralColor
                Else
                    color = Chart.Settings("Bar Color").Value
                End If
                If CType(Chart.bars(i).Pen.Brush, SolidColorBrush).Color <> color Then
                    Chart.bars(i).Pen.Brush = New SolidColorBrush(color)
                    RefreshObject(Chart.bars(i))
                End If
            Next
            'NeutralBarsText.Location = New Point(CurrentBar.Number + 14, CurrentBar.Close)
            'Dim v = (2550 * E ^ (-64.5 * Chart.Settings("RangeValue").Value) + 20) / ((Chart.bars.Count - ASwing.StartPoint.X) / Abs(CSwing.EndPoint.Y - ASwing.StartPoint.Y))
            'NeutralBarsText.Text = Chart.bars.Count - CSwing.EndPoint.X

            ChannelBarsBack = Chart.bars.Count - location
            previousPosition = location
            'DeleteSecondLines()
        End Sub
        Private Function IsConfirmedChannel() As Boolean
            Return PotentialBSwing.EndPoint.X = PotentialBSwing.StartPoint.X
        End Function
        Private Sub SetSwingPoints(bar As BarData)
            Dim a, b, c, d, potentialC As Point
            a = ASwing.StartPoint
            b = BSwing.StartPoint
            c = CSwing.StartPoint
            d = CSwing.EndPoint
            potentialC = PotentialBSwing.EndPoint
            If (CurrentDirection = Direction.Up And bar.High >= b.Y And bar.High >= d.Y) Or
                    (CurrentDirection = Direction.Down And bar.Low <= b.Y And bar.Low <= d.Y) Then 'extend the d point 
                If potentialC.X <> d.X Then
                    b = d
                    c = potentialC
                End If
                d = New Point(bar.Number, If(CurrentDirection = Direction.Up, bar.High, bar.Low))
                potentialC = d
                retracePoint = d.Y
            End If
            If ((CurrentDirection = Direction.Up And bar.Low <= c.Y) Or
                    (CurrentDirection = Direction.Down And bar.High >= c.Y)) And c.X = d.X Then ' extend c point
                c = New Point(bar.Number, If(CurrentDirection = Direction.Up, bar.Low, bar.High))
                d = c
            End If
            If ((CurrentDirection = Direction.Up And Round(bar.Low, 5) <= Round(potentialC.Y, 5) And Round(Abs(d.Y - bar.Low), 5) >= Round(Abs(b.Y - c.Y), 5)) Or
                    (CurrentDirection = Direction.Down And Round(bar.High, 5) >= Round(potentialC.Y, 5) And Round(Abs(d.Y - bar.High), 5) >= Round(Abs(b.Y - c.Y), 5))) And c.X <> d.X Then ' extend potential c point
                potentialC = New Point(bar.Number, If(CurrentDirection = Direction.Up, bar.Low, bar.High))
            End If
            If ((CurrentDirection = Direction.Up And Round(bar.Low, 5) <= Round(retracePoint, 5)) Or
                (CurrentDirection = Direction.Down And Round(bar.High, 5) >= Round(retracePoint, 5))) And c.X <> d.X Then ' extend potential c point
                retracePoint = If(CurrentDirection = Direction.Up, bar.Low, bar.High)
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
            'ALengthText.Text = If(a.Y - b.Y = 0, "", FormatNumberLengthAndPrefix(Abs(a.Y - b.Y), Chart.Settings("DecimalPlaces").Value))
            'BLengthText.Text = If(b.Y - c.Y = 0, "", FormatNumberLengthAndPrefix(Abs(b.Y - c.Y), Chart.Settings("DecimalPlaces").Value))
            'CLengthText.Text = If(d.Y - c.Y = 0, "", FormatNumberLengthAndPrefix(Abs(c.Y - d.Y), Chart.Settings("DecimalPlaces").Value))
            'PotentialBSwingText.Text = If(potentialC.Y - d.Y = 0, "", FormatNumberLengthAndPrefix(Abs(potentialC.Y - d.Y), Chart.Settings("DecimalPlaces").Value))
        End Sub
        Private Sub SetSwingPoints2(bar As BarData, direction As Direction)
            Dim a, b, c, d, potentialC As Point
            a = ASwing2.StartPoint
            b = BSwing2.StartPoint
            c = CSwing2.StartPoint
            d = CSwing2.EndPoint
            potentialC = PotentialBSwing2.EndPoint
            If (direction = Direction.Up And bar.High >= b.Y And bar.High >= d.Y) Or
                    (direction = Direction.Down And bar.Low <= b.Y And bar.Low <= d.Y) Then 'extend the d point 
                If potentialC.X <> d.X Then
                    b = d
                    c = potentialC
                End If
                d = New Point(bar.Number, If(direction = Direction.Up, bar.High, bar.Low))
                potentialC = d
                retracePoint2 = d.Y
            End If
            If ((direction = Direction.Up And bar.Low <= c.Y) Or
                    (direction = Direction.Down And bar.High >= c.Y)) And c.X = d.X Then ' extend c point
                c = New Point(bar.Number, If(direction = Direction.Up, bar.Low, bar.High))
                d = c
            End If
            If ((direction = Direction.Up And bar.Low <= potentialC.Y And Abs(d.Y - bar.Low) >= Abs(b.Y - c.Y)) Or
                    (direction = Direction.Down And bar.High >= potentialC.Y And Abs(d.Y - bar.High) >= Abs(b.Y - c.Y))) And c.X <> d.X Then ' extend potential c point
                potentialC = New Point(bar.Number, If(direction = Direction.Up, bar.Low, bar.High))
            End If
            If ((direction = Direction.Up And Round(bar.Low) <= Round(retracePoint2)) Or
                (direction = Direction.Down And Round(bar.High) >= Round(retracePoint2))) And c.X <> d.X Then ' extend potential c point
                retracePoint2 = If(direction = Direction.Up, bar.Low, bar.High)
            End If
            ASwing2.StartPoint = a
            ASwing2.EndPoint = b
            BSwing2.StartPoint = b
            BSwing2.EndPoint = c
            CSwing2.StartPoint = c
            CSwing2.EndPoint = d
            PotentialASwing2.StartPoint = a
            PotentialASwing2.EndPoint = d
            PotentialBSwing2.StartPoint = d
            PotentialBSwing2.EndPoint = potentialC
            'ALengthText.Text = If(a.Y - b.Y = 0, "", FormatNumberLengthAndPrefix(Abs(a.Y - b.Y), Chart.Settings("DecimalPlaces").Value))
            'BLengthText.Text = If(b.Y - c.Y = 0, "", FormatNumberLengthAndPrefix(Abs(b.Y - c.Y), Chart.Settings("DecimalPlaces").Value))
            'CLengthText.Text = If(d.Y - c.Y = 0, "", FormatNumberLengthAndPrefix(Abs(c.Y - d.Y), Chart.Settings("DecimalPlaces").Value))
            'PotentialBSwingText.Text = If(potentialC.Y - d.Y = 0, "", FormatNumberLengthAndPrefix(Abs(potentialC.Y - d.Y), Chart.Settings("DecimalPlaces").Value))
        End Sub
        Protected Overrides Sub NewBar()
            MyBase.NewBar()
        End Sub
        Dim curTrendEvent As SwingEvent = SwingEvent.None
        Dim lastTrendExtension As Decimal
        Dim curTrendRetracement As Decimal
        Dim currentSwingRetracement As Decimal


        Protected Overrides Sub Main()
            If IsLastBarOnChart Then DrawLines(False)

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
            'newTrendRVText.Location = New Point(CurrentBar.Number, CurrentTrendEndPrice() + If(MTrend.Direction = Direction.Up, -PriceRV, PriceRV))
            'newTrendRVText.Font.Brush = New SolidColorBrush(If(MTrend.Direction = Direction.Up, TrendChannelDownColor, TrendChannelUpColor))
            'If (MTrend.Direction = Direction.Up And curTrendRetracement <= CurrentTrendEndPrice() - PriceRV) Or (MTrend.Direction = Direction.Down And curTrendRetracement >= CurrentTrendEndPrice() + PriceRV) Then
            '    newTrendRVText.Location = New Point(CurrentBar.Number, curTrendRetracement)
            'Else
            '    If CurrentTrendEndPrice() <> CurrentTrendStartPrice() Then
            '        newTrendRVText.Text = newRVTextPrefix1 & Round(PriceRV / Abs(CurrentTrendEndPrice() - CurrentTrendStartPrice()) * 100) & "% " & FormatNumberLengthAndPrefix(PriceRV, Chart.Settings("DecimalPlaces").Value) & "RV " & FormatNumber(newTrendRVText.Location.Y, Chart.Settings("DecimalPlaces").Value) & " New"
            '    End If
            'End If
            'extendTrendText.Text = rvTextPrefix1 & "0% " & FormatNumber(CurrentTrendEndPrice, Chart.Settings("DecimalPlaces").Value) & " Extend"
            'extendTrendText.Location = New Point(CurrentBar.Number, CurrentTrendEndPrice)
            'extendTrendText.Font.Brush = New SolidColorBrush(If(MTrend.Direction = Direction.Up, TrendChannelUpColor, TrendChannelDownColor))

            'retraceTrendText.Location = New Point(CurrentBar.Number, CurrentTrendStartPoint.Y)
            'retraceTrendText.Text = rvTextPrefix1 & "100%  " & FormatNumber(retraceTrendText.Location.Y, Chart.Settings("DecimalPlaces").Value)
            'retraceTrendText.Font.Brush = New SolidColorBrush(If(MTrend.Direction = Direction.Up, TrendChannelDownColor, TrendChannelUpColor))

            'lastExtendText.Location = New Point(CurrentBar.Number, lastTrendExtension)
            'If MPotentialBSwing.EndPoint.X = MPotentialBSwing.StartPoint.X Or lastTrendExtension = 0 Or lastTrendExtension = Decimal.MaxValue Then
            '    lastExtendText.Text = ""
            'Else
            '    lastExtendText.Text = rvTextPrefix1 & Round(Abs(CurrentTrendEndPrice() - lastTrendExtension) / Abs(CurrentTrendEndPrice() - CurrentTrendStartPrice()) * 100) & "% " & FormatNumber(lastExtendText.Location.Y, Chart.Settings("DecimalPlaces").Value)
            'End If
            'lastExtendText.Font.Brush = New SolidColorBrush(AbcChannelAndTextColor)

            'If MPotentialBSwing.StartPoint.X <> MPotentialBSwing.EndPoint.X Or (MTrend.Direction = Direction.Up And curTrendRetracement <= MCSwing.EndPoint.Y - (MBSwing.StartPoint.Y - MCSwing.StartPoint.Y)) Or (MTrend.Direction = Direction.Down And curTrendRetracement >= MCSwing.EndPoint.Y - (MBSwing.StartPoint.Y - MCSwing.StartPoint.Y)) Then
            '    newSwingTextObject1.Text = newRVTextPrefix1 & Round(Abs(MPotentialBSwing.StartPoint.Y - MPotentialBSwing.EndPoint.Y) / Abs(MASwing.StartPoint.Y - MCSwing.EndPoint.Y) * 100, 0) & "% " &
            '                    FormatNumber(CurrentTrendEndPrice() - (MPotentialBSwing.StartPoint.Y - MPotentialBSwing.EndPoint.Y), Chart.Settings("DecimalPlaces").Value)
            '    newSwingTextObject1.Location = New Point(CurrentBar.Number, CurrentTrendEndPrice() - (MPotentialBSwing.StartPoint.Y - MPotentialBSwing.EndPoint.Y))
            'Else
            '    newSwingTextObject1.Text = newRVTextPrefix1 & Round(Abs(MBSwing.StartPoint.Y - MCSwing.StartPoint.Y) / Abs(MASwing.StartPoint.Y - MCSwing.EndPoint.Y) * 100, 0) & "% " &
            '                    FormatNumber(CurrentTrendEndPrice() - (MBSwing.StartPoint.Y - MCSwing.StartPoint.Y), Chart.Settings("DecimalPlaces").Value) & " " & FormatNumberLengthAndPrefix(Abs(MBSwing.StartPoint.Y - MCSwing.StartPoint.Y), Chart.Settings("DecimalPlaces").Value)
            '    newSwingTextObject1.Location = New Point(CurrentBar.Number, CurrentTrendEndPrice() - (MBSwing.StartPoint.Y - MCSwing.StartPoint.Y))
            'End If
            'newSwingTextObject1.Font.Brush = New SolidColorBrush(AbcChannelAndTextColor)

        End Sub

        Private Function NewSwing(ByVal color As Color, ByVal startPoint As Point, ByVal endPoint As Point, ByVal show As Boolean, ByVal direction As Direction) As Swing
            Return New Swing(NewTrendLine(color, startPoint, endPoint, show), direction)
        End Function

#Region "ChartPad"
        Dim grabArea As Border
        Dim currentRBPopupBtn As Button
        Dim addRBMinMove As Button
        Dim subtractRBMinMove As Button

        Dim popupRB As Popup
        Dim popupRBGrid As Grid
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
            grabArea = New Border With {.BorderThickness = New Thickness(1), .BorderBrush = Brushes.Gray, .Background = New SolidColorBrush(Color.FromArgb(255, 252, 184, 41)), .Margin = New Thickness(0.5)}
            grabArea.SetValue(Grid.ColumnSpanProperty, 3)
            grabArea.Height = 17
            grd.Children.Add(grabArea)
        End Sub
        Sub SetRows()

            Grid.SetRow(grabArea, 0)
            Grid.SetRow(currentRBPopupBtn, 1)
            Grid.SetRow(addRBMinMove, 2)
            Grid.SetRow(subtractRBMinMove, 3)

            grd.Children.Add(currentRBPopupBtn)
            grd.Children.Add(addRBMinMove)
            grd.Children.Add(subtractRBMinMove)
        End Sub
        Sub InitControls()
            Dim fontsize As Integer = 16

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
            If currentRBPopupBtn IsNot Nothing Then
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






