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

    Public Class ManualTrendEzra
        Inherits AnalysisTechnique
        Implements IChartPadAnalysisTechnique
#Region "AnalysisTechnique Inherited Code"
        Public Sub New(ByVal chart As Chart)
            MyBase.New(chart) ' Call the base class constructor.
            Description = "Two-swing AutoTrend analysis technique."
            If chart IsNot Nothing Then
                AddHandler chart.ChartKeyDown, AddressOf KeyPress
                AddHandler chart.ChartMouseDraggedEvent, AddressOf ChartMouseDrag
                AddHandler chart.ChartMouseDownEvent, AddressOf ChartMouseDrag
            End If
        End Sub
        Public Overrides Property Name As String = "ManualTrendEzra"

#End Region



#Region "Inputs"

        <Input> Public Property ChartPadVisible As Boolean = True Implements IChartPadAnalysisTechnique.ChartPadVisible
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
        <Input()> Public Property AbcLengthTextFontSize As Double = 11
        <Input()> Public Property AbcLengthTextFontWeight As FontWeight = FontWeights.Bold
        <Input()> Public Property AbcTargetTextFontSize As Double = 12
        <Input()> Public Property AbcTargetTextFontWeight As FontWeight = FontWeights.Bold
        <Input()> Public Property BCTargetThickness As Decimal = 2
        <Input()> Public Property BCTargetWidth As Decimal = 8
        <Input()> Public Property ZoneRV As Decimal = 2

        Private _rv As Decimal = 2


        <Input(, "Trend")> Public Property TrendChannelThickness As Decimal = 0.7
        <Input()> Public Property PotentialTrendChannel1Thickness As Decimal
        <Input()> Public Property PotentialTrendChannel2Thickness As Decimal
        <Input()> Public Property TrendChannelUpColor As Color = (New ColorConverter).ConvertFrom("#FF004C00")
        <Input()> Public Property TrendChannelDownColor As Color = (New ColorConverter).ConvertFrom("#FF7F0000")
        <Input()> Public Property PotentialTrendChannelUpColor As Color
        <Input()> Public Property PotentialTrendChannelDownColor As Color
        <Input()> Public Property TrendLengthTextFontSize As Double = 14
        <Input()> Public Property TrendLengthTextFontWeight As FontWeight = FontWeights.Bold
        <Input()> Public Property ChannelBarsBack As Integer = 20

        <Input(, "Bar Coloring")>
        Public Property UpColor As Color = (New ColorConverter).ConvertFrom("#FF00B200")
        <Input()> Public Property NeutralColor As Color = (New ColorConverter).ConvertFrom("#FF000000")
        <Input()> Public Property DownColor As Color = (New ColorConverter).ConvertFrom("#FFB20000")
        <Input()> Public Property TargetTextStepRangeMultiplier As Decimal = 1


#End Region


        Dim ConfirmedChannel As TrendLine
        Dim SwingPotentialLine1 As TrendLine

        'manual
        Private ASwing As TrendLine
        Private BSwing As TrendLine
        Private CSwing As TrendLine
        Private PotentialBSwing As TrendLine
        Private PotentialASwing As TrendLine
        Private ALengthText As Label
        Private BLengthText As Label
        Private CLengthText As Label
        Private PotentialBSwingText As Label
        Private TrendLengthText As Label
        Dim NeutralBarsText As Label
        Private TargetBCLength As TrendLine

        Private upperRangeLine As TrendLine
        Private lowerRangeLine As TrendLine
        Private zoneUpperLine As TrendLine
        Private zoneLowerLine As TrendLine
        Private zoneUpperLine2 As TrendLine
        Private zoneLowerLine2 As TrendLine
        Private zoneUpperLine3 As TrendLine
        Private zoneLowerLine3 As TrendLine
        Private upperZone As Plot
        Private lowerZone As Plot
        Dim targetTexts As List(Of Label)
        Const textPrefix As String = "── "

        Public Overrides Sub Reset()
            MyBase.Reset()
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

        End Sub

        Protected Overrides Sub Begin()
            MyBase.Begin()
            StartLocation = Chart.bars.Count - ChannelBarsBack
            ConfirmedChannel = NewTrendLine(Colors.Red, True)
            SwingPotentialLine1 = NewTrendLine(Colors.Red, True)
            Dim lines() = {SwingPotentialLine1, ConfirmedChannel}
            For Each line In lines
                line.Pen = New Pen(New SolidColorBrush(Colors.Red), 0)
                line.IsRegressionLine = True : line.HasParallel = True : line.IsSelectable = False : line.IsEditable = False : line.ExtendRight = True
            Next
            ASwing = NewTrendLine(Colors.Gray, True) : ASwing.IsEditable = False : ASwing.IsSelectable = False : ASwing.Pen.Thickness = AbcSwingLineThickness
            BSwing = NewTrendLine(Colors.Gray, True) : BSwing.IsEditable = False : BSwing.IsSelectable = False : BSwing.Pen.Thickness = AbcSwingLineThickness
            CSwing = NewTrendLine(Colors.Gray, True) : CSwing.IsEditable = False : CSwing.IsSelectable = False : CSwing.Pen.Thickness = AbcSwingLineThickness
            PotentialASwing = NewTrendLine(Colors.Gray, True) : PotentialASwing.IsEditable = False : PotentialASwing.IsSelectable = False : PotentialASwing.Pen.Thickness = AbcSwingLineThickness
            PotentialBSwing = NewTrendLine(Colors.Gray, True) : PotentialBSwing.IsEditable = False : PotentialBSwing.IsSelectable = False : PotentialBSwing.Pen.Thickness = AbcSwingLineThickness
            ALengthText = NewLabel("", AbcChannelAndTextColor, New Point(0, 0),, New Font With {.FontSize = AbcLengthTextFontSize, .FontWeight = AbcLengthTextFontWeight}, False) : ALengthText.HorizontalAlignment = LabelHorizontalAlignment.Right
            BLengthText = NewLabel("", AbcChannelAndTextColor, New Point(0, 0),, New Font With {.FontSize = AbcLengthTextFontSize, .FontWeight = AbcLengthTextFontWeight}, False) : BLengthText.HorizontalAlignment = LabelHorizontalAlignment.Right
            CLengthText = NewLabel("", AbcChannelAndTextColor, New Point(0, 0),, New Font With {.FontSize = AbcLengthTextFontSize, .FontWeight = AbcLengthTextFontWeight}, False) : CLengthText.HorizontalAlignment = LabelHorizontalAlignment.Right
            TrendLengthText = NewLabel("", TrendChannelDownColor, New Point(0, 0),, New Font With {.FontSize = TrendLengthTextFontSize, .FontWeight = TrendLengthTextFontWeight}, False) : TrendLengthText.HorizontalAlignment = LabelHorizontalAlignment.Right
            PotentialBSwingText = NewLabel("", AbcChannelAndTextColor, New Point(0, 0),, New Font With {.FontSize = AbcLengthTextFontSize, .FontWeight = AbcLengthTextFontWeight}, False) : PotentialBSwingText.HorizontalAlignment = LabelHorizontalAlignment.Right
            NeutralBarsText = NewLabel("", AbcChannelAndTextColor, New Point(0, 0),, New Font With {.FontSize = AbcLengthTextFontSize, .FontWeight = AbcLengthTextFontWeight}, False) : NeutralBarsText.VerticalAlignment = LabelVerticalAlignment.Center
            upperRangeLine = NewTrendLine(TrendChannelUpColor, True) : upperRangeLine.IsEditable = False : upperRangeLine.IsSelectable = False : upperRangeLine.Pen.Thickness = AbcSwingLineThickness : upperRangeLine.ExtendRight = True
            lowerRangeLine = NewTrendLine(TrendChannelDownColor, True) : lowerRangeLine.IsEditable = False : lowerRangeLine.IsSelectable = False : lowerRangeLine.Pen.Thickness = AbcSwingLineThickness : lowerRangeLine.ExtendRight = True

            upperZone = NewPlot(Colors.Orange)
            lowerZone = NewPlot(Colors.Orange)
            zoneUpperLine = NewTrendLine(Colors.Orange, True) : zoneUpperLine.IsEditable = False : zoneUpperLine.IsSelectable = False : zoneUpperLine.Pen.Thickness = AbcSwingLineThickness : zoneUpperLine.ExtendRight = True
            zoneLowerLine = NewTrendLine(Colors.Orange, True) : zoneLowerLine.IsEditable = False : zoneLowerLine.IsSelectable = False : zoneLowerLine.Pen.Thickness = AbcSwingLineThickness : zoneLowerLine.ExtendRight = True
            zoneUpperLine2 = NewTrendLine(Colors.Orange, True) : zoneUpperLine2.IsEditable = False : zoneUpperLine2.IsSelectable = False : zoneUpperLine2.Pen.Thickness = AbcSwingLineThickness : zoneUpperLine2.ExtendRight = True
            zoneLowerLine2 = NewTrendLine(Colors.Orange, True) : zoneLowerLine2.IsEditable = False : zoneLowerLine2.IsSelectable = False : zoneLowerLine2.Pen.Thickness = AbcSwingLineThickness : zoneLowerLine2.ExtendRight = True
            zoneUpperLine3 = NewTrendLine(Colors.Orange, True) : zoneUpperLine3.IsEditable = False : zoneUpperLine3.IsSelectable = False : zoneUpperLine3.Pen.Thickness = AbcSwingLineThickness : zoneUpperLine3.ExtendRight = True
            zoneLowerLine3 = NewTrendLine(Colors.Orange, True) : zoneLowerLine3.IsEditable = False : zoneLowerLine3.IsSelectable = False : zoneLowerLine3.Pen.Thickness = AbcSwingLineThickness : zoneLowerLine3.ExtendRight = True

            TargetBCLength = NewTrendLine(TrendChannelDownColor, True) : lowerRangeLine.IsEditable = False : lowerRangeLine.IsSelectable = False : TargetBCLength.Pen.Thickness = AbcSwingLineThickness
            targetTexts = New List(Of Label)
            previousPosition = 0
            DrawLines(False)
        End Sub
        Dim pivots As List(Of Point)
        Dim StartLocation As Integer
        Dim CurrentDirection As Direction
        Private Sub ChartMouseDrag(sender As Object, location As Point)
            If Keyboard.IsKeyDown(Key.LeftShift) Or Keyboard.GetKeyStates(Key.CapsLock) = KeyStates.Toggled Then
                StartLocation = location.X
                DrawLines(True)
            End If
        End Sub
        Dim previousPosition As Integer
        Dim retracePoint As Decimal
        Private Sub DrawLines(mouseDriven As Boolean)
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
            If StartLocation = previousPosition And mouseDriven Then Exit Sub
            If StartLocation = low.X Then CurrentDirection = Direction.Up Else CurrentDirection = Direction.Down
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
            ALengthText.Location = ASwing.EndPoint
            BLengthText.Location = BSwing.EndPoint
            CLengthText.Location = CSwing.EndPoint

            ALengthText.VerticalAlignment = If(CurrentDirection = Direction.Down, LabelVerticalAlignment.Top, LabelVerticalAlignment.Bottom)
            BLengthText.VerticalAlignment = If(CurrentDirection = Direction.Down, LabelVerticalAlignment.Bottom, LabelVerticalAlignment.Top)
            CLengthText.VerticalAlignment = If(CurrentDirection = Direction.Down, LabelVerticalAlignment.Top, LabelVerticalAlignment.Bottom)

            For Each item In targetTexts
                RemoveObjectFromChart(item)
            Next
            targetTexts.Clear()
            Dim percentBack As Decimal
            Dim [step] As Decimal = 0.25
            If IsConfirmedChannel() Then
                PotentialASwing.Pen.Thickness = 0
                PotentialBSwing.Pen.Thickness = 0


                PotentialBSwingText.Text = ""

                ASwing.Pen.Thickness = 0
                BSwing.Pen.Thickness = 0
                CSwing.Pen.Thickness = 0

                ConfirmedChannel.StartPoint = ASwing.StartPoint
                ConfirmedChannel.EndPoint = CSwing.EndPoint
                ConfirmedChannel.OuterPen.Thickness = TrendChannelThickness
                ConfirmedChannel.OuterPen.Brush = New SolidColorBrush(If(CurrentDirection = Direction.Up, TrendChannelUpColor, TrendChannelDownColor))

                'SwingPotentialLine2.TL.OuterPen.Thickness = 0
                SwingPotentialLine1.OuterPen.Thickness = 0
                percentBack = Abs(retracePoint - CSwing.EndPoint.Y) / Abs(CSwing.EndPoint.Y - ASwing.StartPoint.Y)

                TargetBCLength.Coordinates = New LineCoordinates(Chart.bars.Count, CSwing.EndPoint.Y - (BSwing.StartPoint.Y - BSwing.EndPoint.Y), Chart.bars.Count + BCTargetWidth, CSwing.EndPoint.Y - (BSwing.StartPoint.Y - BSwing.EndPoint.Y))
                TargetBCLength.Pen.Brush = New SolidColorBrush(If(CurrentDirection = Direction.Down, TrendChannelDownColor, TrendChannelUpColor))
                TargetBCLength.Pen.Thickness = BCTargetThickness

            Else
                'if potential channel
                PotentialASwing.Pen.Thickness = 0
                PotentialBSwing.Pen.Thickness = 0
                ASwing.Pen.Thickness = 0
                BSwing.Pen.Thickness = 0
                CSwing.Pen.Thickness = 0
                ALengthText.Text = ""
                BLengthText.Text = ""
                CLengthText.Text = ""
                PotentialBSwingText.Location = PotentialBSwing.EndPoint
                PotentialBSwingText.VerticalAlignment = If(CurrentDirection = Direction.Down, LabelVerticalAlignment.Bottom, LabelVerticalAlignment.Top)
                ConfirmedChannel.StartPoint = ASwing.StartPoint
                ConfirmedChannel.EndPoint = CSwing.EndPoint
                ConfirmedChannel.OuterPen.Thickness = TrendChannelThickness
                ConfirmedChannel.OuterPen.Brush = New SolidColorBrush(If(CurrentDirection = Direction.Up, TrendChannelUpColor, TrendChannelDownColor))
                ConfirmedChannel.LockToEnd = False
                SwingPotentialLine1.StartPoint = ASwing.StartPoint
                SwingPotentialLine1.EndPoint = AddToX(FindRangeBar(PotentialASwing.EndPoint.X, Chart.bars.Count - 1, If(CurrentDirection = Direction.Down, True, False)), 1)
                SwingPotentialLine1.OuterPen.Brush = New SolidColorBrush(If(CurrentDirection = Direction.Up, PotentialTrendChannelUpColor, PotentialTrendChannelDownColor))
                SwingPotentialLine1.OuterPen.Thickness = PotentialTrendChannel1Thickness

                percentBack = Abs(PotentialBSwing.StartPoint.Y - PotentialBSwing.EndPoint.Y) / Abs(CSwing.EndPoint.Y - ASwing.StartPoint.Y)
                TargetBCLength.Pen.Thickness = 0
            End If
            'draw target texts
            Dim txtCol As Color = If(CurrentDirection = Direction.Up, TrendChannelDownColor, TrendChannelUpColor)
            For i = percentBack + [step] - (percentBack Mod [step]) To 1 Step [step]
                Dim point = New Point(Chart.bars.Count, CSwing.EndPoint.Y - i * (CSwing.EndPoint.Y - ASwing.StartPoint.Y))
                If Abs(ASwing.StartPoint.Y - point.Y) > 0.75 * Chart.Settings("RangeValue").Value Then
                    targetTexts.Add(NewLabel(textPrefix & CStr(Round(i * 100, 0)) & "%    " & CStr(FormatNumber(point.Y, Chart.Settings("DecimalPlaces").Value)), txtCol, AddToX(point, 1),,, False))
                End If
            Next
            For Each t In targetTexts
                t.Font.FontWeight = AbcTargetTextFontWeight
            Next
            targetTexts.Add(NewLabel(textPrefix & "100%  " & CStr(Round(ASwing.StartPoint.Y, Chart.Settings("DecimalPlaces").Value)), txtCol, New Point(Chart.bars.Count + 1, ASwing.StartPoint.Y),,, False))
            targetTexts(targetTexts.Count - 1).Font.FontWeight = FontWeights.Bold
            If IsConfirmedChannel() Then
                'draw 0
                targetTexts.Add(NewLabel(textPrefix & "0%      " & CStr(Round(CSwing.EndPoint.Y, Chart.Settings("DecimalPlaces").Value)), If(CurrentDirection = Direction.Down, TrendChannelDownColor, TrendChannelUpColor), New Point(Chart.bars.Count + 1, CSwing.EndPoint.Y),,, False))
                targetTexts(targetTexts.Count - 1).Font.FontWeight = FontWeights.Bold
            End If
            Dim targetP As Decimal = If(IsConfirmedChannel(), retracePoint, PotentialBSwing.EndPoint.Y)
            targetTexts.Add(NewLabel(textPrefix & CStr(Round(Abs(targetP - CSwing.EndPoint.Y) / Abs(ASwing.StartPoint.Y - CSwing.EndPoint.Y) * 100, 0)) & "%    " & CStr(Round(targetP, Chart.Settings("DecimalPlaces").Value)), If(CurrentDirection = Direction.Down, TrendChannelDownColor, TrendChannelUpColor), New Point(Chart.bars.Count + 1, targetP),,, False))
            For Each t In targetTexts
                t.Font.FontSize = AbcTargetTextFontSize
                t.RefreshVisual()
            Next
            'draw length text
            TrendLengthText.Location = CSwing.EndPoint
            TrendLengthText.VerticalAlignment = If(CurrentDirection = Direction.Up, LabelVerticalAlignment.Bottom, LabelVerticalAlignment.Top)
            TrendLengthText.Text = RemovePrefixZero(Round(Abs(CSwing.EndPoint.Y - ASwing.StartPoint.Y), Chart.Settings("DecimalPlaces").Value)) & " " & Abs(CSwing.EndPoint.X - ASwing.StartPoint.X)
            TrendLengthText.Font.Brush = New SolidColorBrush(If(CurrentDirection = Direction.Up, TrendChannelUpColor, TrendChannelDownColor))
            TrendLengthText.RefreshVisual()
            'color bars
            'For i = 0 To Chart.bars.Count - 1
            '    If Not TypeOf Chart.bars(i).Pen.Brush Is SolidColorBrush Then
            '        Chart.bars(i).Pen.Brush = New SolidColorBrush
            '    End If
            '    Dim color As Color
            '    If i >= StartLocation - 1 And i <= CSwing.EndPoint.X Then
            '        color = If(CurrentDirection = Direction.Up, UpColor, DownColor)
            '    ElseIf i >= StartLocation - 1 Then
            '        color = NeutralColor
            '    Else
            '        color = Chart.Settings("Bar Color").Value
            '    End If
            '    If CType(Chart.bars(i).Pen.Brush, SolidColorBrush).Color <> color Then
            '        Chart.bars(i).Pen.Brush = New SolidColorBrush(color)
            '        RefreshObject(Chart.bars(i))
            '    End If
            'Next
            NeutralBarsText.Location = New Point(CurrentBar.Number + 7, CurrentBar.Close)
            Dim v = (2550 * E ^ (-64.5 * Chart.Settings("RangeValue").Value) + 20) / ((Chart.bars.Count - ASwing.StartPoint.X) / Abs(CSwing.EndPoint.Y - ASwing.StartPoint.Y))
            NeutralBarsText.Text = Chart.bars.Count - CSwing.EndPoint.X & "  " & CurrentBar.Number - StartLocation & " " & Round(v, 2)

            If Chart.bars.Count > 0 Then
                Dim swingDir As Boolean = If(CurrentDirection = Direction.Up, True, False)
                Dim swing As LineCoordinates
                Dim rv As Decimal = Chart.Settings("RangeValue").Value * ZoneRV
                pivots = New List(Of Point)
                pivots.Add(New Point(StartLocation, If(CurrentDirection = Direction.Up, Chart.bars(StartLocation - 1).Data.Low, Chart.bars(StartLocation - 1).Data.High)))
                Dim upperPoints As New List(Of Point)
                Dim lowerPoints As New List(Of Point)
                lowerPoints.Add(pivots(pivots.Count - 1))
                upperPoints.Add(pivots(pivots.Count - 1))
                For i = StartLocation - 1 To Chart.bars.Count - 1
                    If Chart.bars(i).Data.High - swing.EndPoint.Y >= rv AndAlso Chart.bars(i).Data.Number <> swing.EndPoint.X AndAlso swingDir = False Then
                        swingDir = True
                        swing = New LineCoordinates(swing.EndPoint, New Point(i + 1, Chart.bars(i).Data.High))
                        pivots.Add(swing.EndPoint)
                        upperPoints.Add(pivots(pivots.Count - 1))
                    ElseIf swing.EndPoint.Y - Chart.bars(i).Data.Low >= rv AndAlso Chart.bars(i).Data.Number <> swing.EndPoint.X AndAlso swingDir = True Then
                        swingDir = False
                        swing = New LineCoordinates(swing.EndPoint, New Point(i + 1, Chart.bars(i).Data.Low))
                        pivots.Add(swing.EndPoint)
                        lowerPoints.Add(pivots(pivots.Count - 1))
                    ElseIf Chart.bars(i).Data.High >= swing.EndPoint.Y And swingDir = True Then
                        ' extension up
                        swing.EndPoint = New Point(i + 1, Chart.bars(i).Data.High)
                        pivots(pivots.Count - 1) = swing.EndPoint
                        upperPoints(upperPoints.Count - 1) = pivots(pivots.Count - 1)
                    ElseIf Chart.bars(i).Data.Low <= swing.EndPoint.Y And swingDir = False Then
                        ' extension down
                        swing.EndPoint = New Point(i + 1, Chart.bars(i).Data.Low)
                        pivots(pivots.Count - 1) = swing.EndPoint
                        lowerPoints(lowerPoints.Count - 1) = pivots(pivots.Count - 1)
                    End If
                Next
                upperZone.Points = upperPoints
                lowerZone.Points = lowerPoints
                If upperZone.Points.Count > 1 Then zoneUpperLine.Coordinates = New LineCoordinates(upperZone.Points(upperZone.Points.Count - 2), upperZone.Points(upperZone.Points.Count - 1))
                If lowerZone.Points.Count > 1 Then zoneLowerLine.Coordinates = New LineCoordinates(lowerZone.Points(lowerZone.Points.Count - 2), lowerZone.Points(lowerZone.Points.Count - 1))
                If upperZone.Points.Count > 2 And zoneUpperLine2 IsNot Nothing Then zoneUpperLine2.Coordinates = New LineCoordinates(upperZone.Points(upperZone.Points.Count - 3), upperZone.Points(upperZone.Points.Count - 1))
                If lowerZone.Points.Count > 2 And zoneLowerLine2 IsNot Nothing Then zoneLowerLine2.Coordinates = New LineCoordinates(lowerZone.Points(lowerZone.Points.Count - 3), lowerZone.Points(lowerZone.Points.Count - 1))
                If upperZone.Points.Count > 3 And zoneUpperLine3 IsNot Nothing Then zoneUpperLine3.Coordinates = New LineCoordinates(upperZone.Points(upperZone.Points.Count - 4), upperZone.Points(upperZone.Points.Count - 1))
                If lowerZone.Points.Count > 3 And zoneLowerLine3 IsNot Nothing Then zoneLowerLine3.Coordinates = New LineCoordinates(lowerZone.Points(lowerZone.Points.Count - 4), lowerZone.Points(lowerZone.Points.Count - 1))
            End If


                ChannelBarsBack = Chart.bars.Count - StartLocation
            previousPosition = StartLocation
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
            If ((CurrentDirection = Direction.Up And bar.Low <= potentialC.Y And Abs(d.Y - bar.Low) >= Abs(b.Y - c.Y)) Or
                    (CurrentDirection = Direction.Down And bar.High >= potentialC.Y And Abs(d.Y - bar.High) >= Abs(b.Y - c.Y))) And c.X <> d.X Then ' extend potential c point
                potentialC = New Point(bar.Number, If(CurrentDirection = Direction.Up, bar.Low, bar.High))
            End If
            If ((CurrentDirection = Direction.Up And bar.Low <= retracePoint) Or
                (CurrentDirection = Direction.Down And bar.High >= retracePoint)) And c.X <> d.X Then ' extend potential c point
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
        Protected Overrides Sub NewBar()
            MyBase.NewBar()
        End Sub
        Dim curTrendEvent As SwingEvent = SwingEvent.None
        Dim lastTrendExtension As Decimal
        Dim curTrendRetracement As Decimal
        Dim currentSwingRetracement As Decimal


        Protected Overrides Sub Main()
            If IsLastBarOnChart Then
                DrawLines(False)
            End If

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
        Dim slider As Slider
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

            slider = New Slider
            slider.Minimum = 1
            slider.Maximum = 5
            slider.Orientation = Orientation.Vertical
            slider.VerticalAlignment = VerticalAlignment.Center
            slider.HorizontalAlignment = HorizontalAlignment.Stretch
            slider.Value = 2
            slider.Background = New SolidColorBrush(Chart.Settings("Background").Value)
            slider.Margin = New Thickness(0, 0, 0, 0)
            slider.MinHeight = 150
            slider.LargeChange = 0.5
            slider.SmallChange = 0.05

        End Sub
        Sub SetRows()
            Grid.SetRow(slider, 0)
            Grid.SetRow(grabArea, 1)
            Grid.SetRow(currentRBPopupBtn, 2)
            Grid.SetRow(addRBMinMove, 3)
            Grid.SetRow(subtractRBMinMove, 4)

            grd.Children.Add(slider)
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
        Sub SliderMoved()
            ZoneRV = slider.Value
            If Chart.bars.Count > 0 Then
                Dim swingDir As Boolean = If(CurrentDirection = Direction.Up, True, False)
                Dim swing As LineCoordinates
                Dim rv As Decimal = Chart.Settings("RangeValue").Value * ZoneRV
                pivots = New List(Of Point)
                pivots.Add(New Point(StartLocation, If(CurrentDirection = Direction.Up, Chart.bars(StartLocation - 1).Data.Low, Chart.bars(StartLocation - 1).Data.High)))
                Dim upperPoints As New List(Of Point)
                Dim lowerPoints As New List(Of Point)
                lowerPoints.Add(pivots(pivots.Count - 1))
                upperPoints.Add(pivots(pivots.Count - 1))
                For i = StartLocation - 1 To Chart.bars.Count - 1
                    If Chart.bars(i).Data.High - swing.EndPoint.Y >= rv AndAlso Chart.bars(i).Data.Number <> swing.EndPoint.X AndAlso swingDir = False Then
                        swingDir = True
                        swing = New LineCoordinates(swing.EndPoint, New Point(i + 1, Chart.bars(i).Data.High))
                        pivots.Add(swing.EndPoint)
                        upperPoints.Add(pivots(pivots.Count - 1))
                    ElseIf swing.EndPoint.Y - Chart.bars(i).Data.Low >= rv AndAlso Chart.bars(i).Data.Number <> swing.EndPoint.X AndAlso swingDir = True Then
                        swingDir = False
                        swing = New LineCoordinates(swing.EndPoint, New Point(i + 1, Chart.bars(i).Data.Low))
                        pivots.Add(swing.EndPoint)
                        lowerPoints.Add(pivots(pivots.Count - 1))
                    ElseIf Chart.bars(i).Data.High >= swing.EndPoint.Y And swingDir = True Then
                        ' extension up
                        swing.EndPoint = New Point(i + 1, Chart.bars(i).Data.High)
                        pivots(pivots.Count - 1) = swing.EndPoint
                        upperPoints(upperPoints.Count - 1) = pivots(pivots.Count - 1)
                    ElseIf Chart.bars(i).Data.Low <= swing.EndPoint.Y And swingDir = False Then
                        ' extension down
                        swing.EndPoint = New Point(i + 1, Chart.bars(i).Data.Low)
                        pivots(pivots.Count - 1) = swing.EndPoint
                        lowerPoints(lowerPoints.Count - 1) = pivots(pivots.Count - 1)
                    End If
                Next
                upperZone.Points = upperPoints
                lowerZone.Points = lowerPoints
                If upperZone.Points.Count > 1 Then zoneUpperLine.Coordinates = New LineCoordinates(upperZone.Points(upperZone.Points.Count - 2), upperZone.Points(upperZone.Points.Count - 1))
                If lowerZone.Points.Count > 1 Then zoneLowerLine.Coordinates = New LineCoordinates(lowerZone.Points(lowerZone.Points.Count - 2), lowerZone.Points(lowerZone.Points.Count - 1))
                If upperZone.Points.Count > 2 And zoneUpperLine2 IsNot Nothing Then zoneUpperLine2.Coordinates = New LineCoordinates(upperZone.Points(upperZone.Points.Count - 3), upperZone.Points(upperZone.Points.Count - 1))
                If lowerZone.Points.Count > 2 And zoneLowerLine2 IsNot Nothing Then zoneLowerLine2.Coordinates = New LineCoordinates(lowerZone.Points(lowerZone.Points.Count - 3), lowerZone.Points(lowerZone.Points.Count - 1))
                If upperZone.Points.Count > 3 And zoneUpperLine3 IsNot Nothing Then zoneUpperLine3.Coordinates = New LineCoordinates(upperZone.Points(upperZone.Points.Count - 4), upperZone.Points(upperZone.Points.Count - 1))
                If lowerZone.Points.Count > 3 And zoneLowerLine3 IsNot Nothing Then zoneLowerLine3.Coordinates = New LineCoordinates(lowerZone.Points(lowerZone.Points.Count - 4), lowerZone.Points(lowerZone.Points.Count - 1))
            End If
        End Sub
        Sub AddHandlers()

            AddHandler slider.ValueChanged, Sub() SliderMoved()

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






