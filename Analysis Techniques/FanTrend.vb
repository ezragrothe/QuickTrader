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
    Public Class FanTrend
        Inherits AnalysisTechnique
        Implements IChartPadAnalysisTechnique
        Public Sub New(ByVal chart As Chart)
            MyBase.New(chart)
            Description = "Test analysis technique."
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
                        'drawlines
                    End Sub
            End If
        End Sub



        Dim swings As List(Of Swing)
        Dim barColorings As List(Of Point)
        Dim swingDir As Boolean
        Dim rvText As Label
        Dim extendText As Label

        Dim swingRVText As Label
        Dim swingExtendText As Label
        Dim currentTrendDir As Boolean
        Dim trendEvent As SwingEvent
        Dim trendLines As List(Of Swing)
        Dim regressionLines As List(Of TrendLine)
        Dim potentialRegressionLine As TrendLine
        Dim lastConfirmedRegressionLine As TrendLine
        Dim bcLines As List(Of BCLine)
        Dim potentialSwingRegressionLine As TrendLine
        Dim confirmedSwingRegressionLine As TrendLine
        Private Property CurrentTrend As Swing
            Get
                Return trendLines(trendLines.Count - 1)
            End Get
            Set(value As Swing)
                trendLines(trendLines.Count - 1) = value
            End Set
        End Property
        Private Property CurrentRegression As TrendLine
            Get
                Return regressionLines(regressionLines.Count - 1)
            End Get
            Set(value As TrendLine)
                regressionLines(regressionLines.Count - 1) = value
            End Set
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

        <Input> Public Property TrendRV As Double = 2
        <Input> Public Property TrendRVMultiplier As Double = 2
        <Input> Public Property SwingRV As Double = 2
        <Input> Public Property SwingRVMultiplier As Double = 2
        <Input(, "Coloring")> Public Property TextLineColor As Color = Colors.Gray
        <Input> Public Property UpRegressionLineColor As Color = Colors.Green
        <Input> Public Property DownRegressionLineColor As Color = Colors.Red
        <Input> Public Property UpFanLineColor As Color = Colors.Green
        <Input> Public Property DownFanLineColor As Color = Colors.Red

        <Input> Public Property UpPotentialRegressionLineColor As Color = Colors.Green
        <Input> Public Property DownPotentialRegressionLineColor As Color = Colors.Red
        <Input> Public Property LiveLineColor As Color = Colors.Blue

        <Input> Public Property UpBarColor As Color = Colors.Green
        <Input> Public Property DownBarColor As Color = Colors.Red

        <Input> Public Property UpFillColor As Color = Colors.Red
        <Input> Public Property DownFillColor As Color = Colors.Red
        <Input> Public Property NeutralFillColor As Color = Colors.Gray
        '<Input> Public Property UpPotentialBarColor As Color = Colors.Green
        '<Input> Public Property DownPotentialBarColor As Color = Colors.Red
        '<Input> Public Property UpPotentialFillColor As Color = Colors.Red
        '<Input> Public Property DownPotentialFillColor As Color = Colors.Red

        <Input(, "Line")> Property SwingLineThickness As Decimal = 1
        <Input> Property PotentialBCLineThickness As Decimal = 1
        <Input> Property ConfirmedBCLineThickness As Decimal = 1
        <Input> Property BCLineHistoryThickness As Decimal = 1
        <Input> Property TrendLineThickness As Decimal = 1
        <Input> Property LastTrendLineThickness As Decimal = 1
        <Input> Property ConfirmedRegressionLineThickness As Decimal = 1
        <Input> Property LastConfirmedRegressionLineThickness As Decimal = 1
        <Input> Property PotentialRegressionLineThickness As Decimal = 1
        <Input> Property FanLineThickness As Decimal = 1
        <Input> Property HistoryFanLineThickness As Decimal = 1
        <Input> Property LiveLineThickness As Decimal = 2
        <Input(, "Text")> Property SwingLengthTextFontSize As Decimal = 10
        <Input> Property SwingLengthTextFontWeight As FontWeight = FontWeights.Bold
        <Input> Property BCLengthTextFontSize As Decimal = 10
        <Input> Property BCLengthTextFontWeight As FontWeight = FontWeights.Bold
        <Input> Property BCLengthTextSpacing As Decimal = 0
        <Input> Property TrendLengthTextFontSize As Decimal = 10
        <Input> Property TrendLengthTextFontWeight As FontWeight = FontWeights.Bold
        <Input> Property TrendLengthTextSpacing As Decimal = 0
        <Input> Property SwingTargetTextFontSize As Decimal = 11
        <Input> Property SwingTargetTextFontWeight As FontWeight = FontWeights.Bold
        <Input> Property BCTargetTextFontSize As Decimal = 11
        <Input> Property BCTargetTextFontWeight As FontWeight = FontWeights.Bold
        <Input> Property TrendTargetTextFontSize As Decimal = 11
        <Input> Property TrendTargetTextFontWeight As FontWeight = FontWeights.Bold
        <Input> Property SwingTargetTextLineLength As Integer = 5
        <Input> Property BCTargetTextLineLength As Integer = 5
        <Input> Property TrendTargetTextLineLength As Integer = 5
        <Input(, "Misc")> Property BarsBackStartPoint As Integer = 50

        Private StartLocation As Integer
        Private doubleClick As Boolean
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
                If pivots IsNot Nothing Then
                    Dim prev = StartLocation
                    Dim closest As Integer = Integer.MaxValue
                    Dim index As Integer = 0
                    For Each p In pivots
                        If Abs(p.X - location.X) < Abs(location.X - closest) Then
                            closest = p.X
                        Else Exit For
                        End If
                        index += 1
                    Next
                    Dim v As Integer
                    If index < pivots.Count - 1 Then
                        v = pivots(index).X
                    Else
                        v = closest
                    End If
                    If v <> prev Then
                        'StartLocation = v
                        'mousedriven = True
                        'DrawLines()
                    End If
                End If
            End If
        End Sub

        Private FanLines As List(Of TrendLine)
        Dim pivots As List(Of Point)
        Protected Overrides Sub Begin()
            MyBase.Begin()

            swingDir = False
            swings = New List(Of Swing)
            swings.Add(New Swing(NewTrendLine(TextLineColor, New Point(CurrentBar.Number, CurrentBar.Close), New Point(CurrentBar.Number, CurrentBar.Close), True), False))
            CurrentSwing.TL.Pen.Thickness = SwingLineThickness
            barColorings = New List(Of Point)
            barColorings.Add(New Point(0, -1))
            CurrentSwing.SwingChannel = NewTrendLine(Colors.Red) : CurrentSwing.SwingChannel.IsEditable = False : CurrentSwing.SwingChannel.IsSelectable = False : CurrentSwing.SwingChannel.ExtendRight = False
            rvText = NewLabel("── " & FormatNumberLengthAndPrefix(TrendTargetTextLineLength) & " RV ", TextLineColor, New Point(0, 0), True, , False)
            rvText.Font.FontSize = TrendTargetTextFontSize
            rvText.Font.FontWeight = TrendTargetTextFontWeight
            extendText = NewLabel("── " & FormatNumberLengthAndPrefix(TrendTargetTextLineLength) & " Ext ", TextLineColor, New Point(0, 0), True, , False)
            extendText.Font.FontSize = TrendTargetTextFontSize
            extendText.Font.FontWeight = TrendTargetTextFontWeight
            swingRVText = NewLabel(Strings.StrDup(SwingTargetTextLineLength, "─") & " " & FormatNumberLengthAndPrefix(TrendTargetTextLineLength) & " RV ", TextLineColor, New Point(0, 0), True, , False)
            swingRVText.Font.FontSize = SwingTargetTextFontSize
            swingRVText.Font.FontWeight = SwingTargetTextFontWeight
            swingExtendText = NewLabel(Strings.StrDup(SwingTargetTextLineLength, "─") & " " & FormatNumberLengthAndPrefix(TrendTargetTextLineLength) & " Ext ", TextLineColor, New Point(0, 0), True, , False)
            swingExtendText.Font.FontSize = SwingTargetTextFontSize
            swingExtendText.Font.FontWeight = SwingTargetTextFontWeight
            StartLocation = Chart.bars.Count - Abs(BarsBackStartPoint)
            trendLines = New List(Of Swing)
            bcLines = New List(Of BCLine)
            trendLines.Add(New Swing(NewTrendLine(Colors.Gray, New Point(CurrentBar.Number, CurrentBar.Close), New Point(CurrentBar.Number, CurrentBar.Close)), Direction.Up))
            potentialRegressionLine = NewTrendLine(Colors.Gray, New Point(CurrentBar.Number, CurrentBar.Close), New Point(CurrentBar.Number, CurrentBar.Close))
            potentialRegressionLine.IsRegressionLine = True
            potentialRegressionLine.DrawZoneFill = False
            potentialRegressionLine.UpZoneFillBrush = New SolidColorBrush(UpFillColor)
            potentialRegressionLine.UpNeutralZoneFillBrush = New SolidColorBrush(UpFillColor)
            potentialRegressionLine.DownNeutralZoneFillBrush = New SolidColorBrush(DownFillColor)
            potentialRegressionLine.DownZoneFillBrush = New SolidColorBrush(DownFillColor)
            potentialRegressionLine.ExtendRight = False
            lastConfirmedRegressionLine = NewTrendLine(Colors.Gray, New Point(CurrentBar.Number, CurrentBar.Close), New Point(CurrentBar.Number, CurrentBar.Close))
            lastConfirmedRegressionLine.Pen.Thickness = 0
            lastConfirmedRegressionLine.IsRegressionLine = True
            lastConfirmedRegressionLine.DrawZoneFill = True
            lastConfirmedRegressionLine.NeutralZoneBarStart = Integer.MaxValue
            lastConfirmedRegressionLine.ConfirmedZoneBarStart = 0
            lastConfirmedRegressionLine.UpZoneFillBrush = New SolidColorBrush(UpFillColor)
            lastConfirmedRegressionLine.DownZoneFillBrush = New SolidColorBrush(DownFillColor)

            potentialSwingRegressionLine = NewTrendLine(Colors.Gray, New Point(CurrentBar.Number, CurrentBar.Close), New Point(CurrentBar.Number, CurrentBar.Close))
            potentialSwingRegressionLine.Pen = New Pen(New SolidColorBrush(Colors.Gray), PotentialRegressionLineThickness)
            potentialSwingRegressionLine.IsRegressionLine = True
            potentialSwingRegressionLine.ExtendRight = False
            confirmedSwingRegressionLine = NewTrendLine(TextLineColor, New Point(CurrentBar.Number, CurrentBar.Close), New Point(CurrentBar.Number, CurrentBar.Close))
            confirmedSwingRegressionLine.Pen.Thickness = LastConfirmedRegressionLineThickness
            confirmedSwingRegressionLine.IsRegressionLine = True
            confirmedSwingRegressionLine.DrawZoneFill = True
            confirmedSwingRegressionLine.NeutralZoneBarStart = Integer.MaxValue
            confirmedSwingRegressionLine.ConfirmedZoneBarStart = 0
            confirmedSwingRegressionLine.UpZoneFillBrush = New SolidColorBrush(NeutralFillColor)
            confirmedSwingRegressionLine.DownZoneFillBrush = New SolidColorBrush(NeutralFillColor)

            regressionLines = New List(Of TrendLine)
            regressionLines.Add(NewTrendLine(Colors.Gray, New Point(CurrentBar.Number, CurrentBar.Close), New Point(CurrentBar.Number, CurrentBar.Close)))
            If Chart.bars.Count > 0 Then
                Dim swingDir As Boolean
                Dim swing As LineCoordinates
                'Dim rv As Decimal = Me.RV
                pivots = New List(Of Point)
                pivots.Add(New Point(1, Chart.bars(0).Data.High))
                For i = 0 To Chart.bars.Count - 1
                    If Chart.bars(i).Data.High - swing.EndPoint.Y >= SwingRV AndAlso Chart.bars(i).Data.Number <> swing.EndPoint.X AndAlso swingDir = False Then
                        swingDir = True
                        swing = New LineCoordinates(swing.EndPoint, New Point(i + 1, Chart.bars(i).Data.High))
                        pivots.Add(swing.EndPoint)
                    ElseIf swing.EndPoint.Y - Chart.bars(i).Data.Low >= SwingRV AndAlso Chart.bars(i).Data.Number <> swing.EndPoint.X AndAlso swingDir = True Then
                        swingDir = False
                        swing = New LineCoordinates(swing.EndPoint, New Point(i + 1, Chart.bars(i).Data.Low))
                        pivots.Add(swing.EndPoint)
                    ElseIf Chart.bars(i).Data.High >= swing.EndPoint.Y And swingDir = True Then
                        ' extension up
                        swing.EndPoint = New Point(i + 1, Chart.bars(i).Data.High)
                        pivots(pivots.Count - 1) = swing.EndPoint
                    ElseIf Chart.bars(i).Data.Low <= swing.EndPoint.Y And swingDir = False Then
                        ' extension down
                        swing.EndPoint = New Point(i + 1, Chart.bars(i).Data.Low)
                        pivots(pivots.Count - 1) = swing.EndPoint
                    End If
                Next
            End If
        End Sub
        Public Overrides Sub Reset()
            MyBase.Reset()
        End Sub
        Private Sub DrawLines()
            For Each item In swings
                RemoveObjectFromChart(item.TL)
                RemoveObjectFromChart(item.LengthText)
                RemoveObjectFromChart(item.SwingChannel)
            Next
            swings.Clear()
            For Each item In bcLines
                RemoveObjectFromChart(item.TrendLine)
                RemoveObjectFromChart(item.Line)
                RemoveObjectFromChart(item.LengthText)
                RemoveObjectFromChart(item.TargetText)
            Next
            bcLines.Clear()
            swingDir = currentTrendDir
            swings.Add(New Swing(NewTrendLine(TextLineColor, New Point(Chart.bars(StartLocation - 1).Data.Number, If(swingDir, Chart.bars(StartLocation - 1).Data.Low, Chart.bars(StartLocation - 1).Data.High)), New Point(Chart.bars(StartLocation - 1).Data.Number, If(swingDir, Chart.bars(StartLocation - 1).Data.Low, Chart.bars(StartLocation - 1).Data.High)), True), swingDir))
            CurrentSwing.TL.Pen.Thickness = SwingLineThickness
            barColorings.Clear()
            barColorings.Add(New Point(0, -1))
            CurrentSwing.SwingChannel = NewTrendLine(Colors.Red) : CurrentSwing.SwingChannel.IsEditable = False : CurrentSwing.SwingChannel.IsSelectable = False : CurrentSwing.SwingChannel.ExtendRight = False
            'Dim high As Point = New Point(0, 0)
            'Dim low As Point = New Point(0, Decimal.MaxValue)
            'For i = StartLocation - 1 To Chart.bars.Count - 1
            '    If Chart.bars(i).Data.High >= high.Y Then high = New Point(i + 1, Chart.bars(i).Data.High)
            '    If Chart.bars(i).Data.Low <= low.Y Then low = New Point(i + 1, Chart.bars(i).Data.Low)
            'Next
            'BarColorRoutine(0, Chart.bars.Count - 1, Chart.Settings("Bar Color").Value)
            'For i = 0 To Chart.bars.Count - 1
            '    If Not TypeOf Chart.bars(i).Pen.Brush Is SolidColorBrush Then
            '        Chart.bars(i).Pen.Brush = New SolidColorBrush
            '    End If
            '    Dim color As Color
            '    If i >= StartLocation - 1 And i <= Min(high.X, low.X) Then
            '        color = If(high.X < low.X, UpColor, DownColor)
            '    ElseIf i >= StartLocation - 1 And i <= Max(high.X, low.X) Then
            '        color = If(Not high.X < low.X, UpColor, DownColor)
            '    Else
            '        color = Chart.Settings("Bar Color").Value
            '    End If
            '    If CType(Chart.bars(i).Pen.Brush, SolidColorBrush).Color <> color Then
            '        Chart.bars(i).Pen.Brush = New SolidColorBrush(color)
            '        RefreshObject(Chart.bars(i))
            '    End If
            'Next
            If StartLocation > 1 Then
                For i = StartLocation - 1 To CurrentBar.Number - 1
                    DrawLines(Chart.bars(i).Data)
                Next
            End If
        End Sub
        Protected Sub BarColorRoutine(ByVal startBar As Integer, ByVal endBar As Integer, ByVal color As Color)
            For i = Max(0, startBar - 1) To Max(endBar - 1, 0)
                If Not TypeOf Chart.bars(i).Pen.Brush Is SolidColorBrush Then
                    Chart.bars(i).Pen.Brush = New SolidColorBrush
                End If
                If CType(Chart.bars(i).Pen.Brush, SolidColorBrush).Color <> color Then
                    Chart.bars(i).Pen.Brush = New SolidColorBrush(color)
                    RefreshObject(Chart.bars(i))
                End If
            Next
        End Sub
        Private Sub DrawLines(bar As BarData)
            Dim evnt As Boolean = False
            Dim newSwingEvent As Boolean
            Dim extendEvent As Boolean
            If Round(bar.High - CurrentSwing.EndPrice, 5) >= Round(SwingRV, 5) AndAlso bar.Number <> CurrentSwing.EndBar AndAlso swingDir = False Then
                'new swing up
                swingDir = True
                CurrentSwing.TL.Pen.Thickness = SwingLineThickness
                swings.Add(New Swing(NewTrendLine(TextLineColor, New Point(CurrentSwing.EndBar, CurrentSwing.EndPrice), New Point(bar.Number, bar.High), True), False))
                CurrentSwing.Direction = Direction.Up
                'CurrentSwing.LengthText = CreateText(CurrentSwing.EndPoint, Direction.Up, Abs(CurrentSwing.EndPrice - CurrentSwing.StartPrice), UpColor)
                CurrentSwing.TL.Pen.Thickness = 0
                CurrentSwing.TL.IsEditable = False : CurrentSwing.TL.IsSelectable = False
                CurrentSwing.SwingChannel = NewTrendLine(Colors.Red) : CurrentSwing.SwingChannel.IsEditable = False : CurrentSwing.SwingChannel.IsSelectable = False : CurrentSwing.SwingChannel.ExtendRight = True
                CurrentSwing.LengthText = NewLabel("", TextLineColor, New Point(0, 0))
                AddHandler CurrentSwing.LengthText.MouseDown,
                 Sub(sender As Object, location As Point)
                     SwingRV = CDec(CType(sender, Label).Tag) + Chart.GetMinTick
                     Chart.ReApplyAnalysisTechnique(Me)
                 End Sub
                CurrentSwing.LengthText.Font.FontSize = SwingLengthTextFontSize
                CurrentSwing.LengthText.Font.FontWeight = SwingLengthTextFontWeight

                evnt = True
                newSwingEvent = True
            ElseIf Round(CurrentSwing.EndPrice - bar.Low, 5) >= Round(SwingRV, 5) AndAlso bar.Number <> CurrentSwing.EndBar AndAlso swingDir = True Then
                ' new swing down
                swingDir = False
                CurrentSwing.TL.Pen.Thickness = SwingLineThickness
                swings.Add(New Swing(NewTrendLine(TextLineColor, New Point(CurrentSwing.EndBar, CurrentSwing.EndPrice), New Point(bar.Number, bar.Low), True), False))
                CurrentSwing.Direction = Direction.Down
                'CurrentSwing.LengthText = CreateText(CurrentSwing.EndPoint, Direction.Down, Abs(CurrentSwing.EndPrice - CurrentSwing.StartPrice), DownColor)
                CurrentSwing.TL.Pen.Thickness = 0
                CurrentSwing.TL.IsEditable = False : CurrentSwing.TL.IsSelectable = False
                CurrentSwing.SwingChannel = NewTrendLine(Colors.Red) : CurrentSwing.SwingChannel.IsEditable = False : CurrentSwing.SwingChannel.IsSelectable = False : CurrentSwing.SwingChannel.ExtendRight = True
                CurrentSwing.LengthText = NewLabel("", TextLineColor, New Point(0, 0))
                AddHandler CurrentSwing.LengthText.MouseDown,
                 Sub(sender As Object, location As Point)
                     SwingRV = CDec(CType(sender, Label).Tag) + Chart.GetMinTick
                     Chart.ReApplyAnalysisTechnique(Me)
                 End Sub
                CurrentSwing.LengthText.Font.FontSize = SwingLengthTextFontSize
                CurrentSwing.LengthText.Font.FontWeight = SwingLengthTextFontWeight
                evnt = True
                newSwingEvent = True
            ElseIf Round(bar.High, 5) >= Round(CurrentSwing.EndPrice, 5) And swingDir = True Then
                ' extension up
                CurrentSwing.EndBar = bar.Number
                CurrentSwing.EndPrice = bar.High
                'UpdateText(CurrentSwing.LengthText, CurrentSwing.EndPoint, Abs(CurrentSwing.EndPrice - CurrentSwing.StartPrice))
                evnt = True
                extendEvent = True
            ElseIf Round(bar.Low, 5) <= Round(CurrentSwing.EndPrice, 5) And swingDir = False Then
                ' extension down
                CurrentSwing.EndBar = bar.Number
                CurrentSwing.EndPrice = bar.Low
                'UpdateText(CurrentSwing.LengthText, CurrentSwing.EndPoint, Abs(CurrentSwing.EndPrice - CurrentSwing.StartPrice))
                evnt = True
                extendEvent = True
            End If


            If evnt Then
                confirmedSwingRegressionLine.Coordinates = New LineCoordinates(CurrentSwing.StartPoint, CurrentSwing.EndPoint)
            End If
            potentialSwingRegressionLine.Coordinates = New LineCoordinates(CurrentSwing.StartPoint, New Point(CurrentBar.Number, CurrentBar.Close))
            potentialSwingRegressionLine.Pen.Brush = New SolidColorBrush(If(swingDir, UpPotentialRegressionLineColor, DownPotentialRegressionLineColor))

            swingRVText.Location = New Point(CurrentBar.Number, CurrentSwing.EndPoint.Y + If(swingDir, -SwingRV, SwingRV))
            swingRVText.Text = Strings.StrDup(SwingTargetTextLineLength, "─") & " " & FormatNumberLengthAndPrefix(SwingRV) & " RV " & FormatNum(CurrentSwing.EndPrice + If(swingDir, -1, 1) * SwingRV)
            'swingExtendText.Location = New Point(CurrentBar.Number, CurrentSwing.EndPoint.Y)
            'swingExtendText.Text = Strings.StrDup(SwingTargetTextLineLength, "─") & " Ext " & FormatNum(CurrentSwing.EndPrice)
            If evnt And CurrentSwing.LengthText IsNot Nothing Then
                CurrentSwing.LengthText.Location = CurrentSwing.TL.EndPoint
                CurrentSwing.LengthText.VerticalAlignment = If(swingDir, LabelVerticalAlignment.Bottom, LabelVerticalAlignment.Top)
                CurrentSwing.LengthText.HorizontalAlignment = LabelHorizontalAlignment.Right
                CurrentSwing.LengthText.Text = FormatNumberLengthAndPrefix(Abs(CurrentSwing.TL.EndPoint.Y - CurrentSwing.TL.StartPoint.Y))
                CurrentSwing.LengthText.Tag = Abs(CurrentSwing.TL.EndPoint.Y - CurrentSwing.TL.StartPoint.Y)
            End If
            Dim addBCLine =
                Sub()
                    bcLines.Add(New BCLine)
                    With bcLines(bcLines.Count - 1)
                        .TrendLine = NewTrendLine(If(currentTrendDir, UpRegressionLineColor, DownRegressionLineColor), CurrentTrend.StartPoint, CurrentSwing.EndPoint, False)
                        .TrendLine.IsRegressionLine = True
                        .TrendLine.Pen.Thickness = LastConfirmedRegressionLineThickness
                        .Line = NewTrendLine(TextLineColor, CurrentSwing.EndPoint, CurrentSwing.EndPoint)
                        .Line.Pen.Thickness = ConfirmedBCLineThickness
                        .LengthText = NewLabel("", TextLineColor, CurrentSwing.EndPoint,,, True)
                        .LengthText.HorizontalAlignment = LabelHorizontalAlignment.Right
                        .LengthText.Font.FontSize = BCLengthTextFontSize
                        .LengthText.Font.FontWeight = BCLengthTextFontWeight
                        AddHandler .LengthText.MouseDown,
                             Sub(sender As Object, location As Point)
                                 TrendRV = CDec(CType(sender, Label).Tag)
                                 Chart.ReApplyAnalysisTechnique(Me)
                             End Sub
                        .TargetText = NewLabel("", TextLineColor, CurrentSwing.EndPoint,,, False)
                        .TargetText.Font.FontWeight = BCTargetTextFontWeight
                        .TargetText.Font.FontSize = BCTargetTextFontSize
                        .Swing = CurrentSwing
                    End With
                End Sub
            If newSwingEvent And swingDir = currentTrendDir Then
                If bcLines.Count = 0 OrElse ((
                    (currentTrendDir And CurrentSwing.EndPoint.Y >= bcLines(bcLines.Count - 1).Line.EndPoint.Y) Or
                    (Not currentTrendDir And CurrentSwing.EndPoint.Y <= bcLines(bcLines.Count - 1).Line.EndPoint.Y)
                    ) AndAlso bcLines(bcLines.Count - 1).Swing <> CurrentSwing) Then
                    addBCLine()
                End If
            End If
            If extendEvent And swingDir = currentTrendDir Then
                If bcLines.Count <> 0 AndAlso bcLines(bcLines.Count - 1).Swing = CurrentSwing Then
                    bcLines(bcLines.Count - 1).TrendLine.EndPoint = CurrentSwing.EndPoint
                    bcLines(bcLines.Count - 1).Line.Coordinates = New LineCoordinates(CurrentSwing.EndPoint, CurrentSwing.EndPoint)
                End If
            End If
            If (extendEvent Or newSwingEvent) And bcLines.Count > 0 And swingDir = currentTrendDir Then
                Dim extension As Boolean = True
                For i = swings.Count - 1 To 0 Step -1
                    If (swingDir = False And swings(i).EndPoint.Y < bcLines(bcLines.Count - 1).TrendLine.EndPoint.Y) Or
                       (swingDir = True And swings(i).EndPoint.Y > bcLines(bcLines.Count - 1).TrendLine.EndPoint.Y) Then
                        extension = False
                    End If
                Next
                If extension Then
                    AddObjectToChart(bcLines(bcLines.Count - 1).TrendLine)
                End If
                For i = 0 To bcLines.Count - 2
                    bcLines(i).TrendLine.Pen.Thickness = 0
                Next
            End If
                If evnt And swingDir <> currentTrendDir Then
                If bcLines.Count = 0 Then
                    addBCLine()
                    bcLines(bcLines.Count - 1).Line.StartPoint = CurrentSwing.StartPoint
                    bcLines(bcLines.Count - 1).Line.EndPoint = CurrentSwing.StartPoint
                    If swings.Count > 1 Then bcLines(bcLines.Count - 1).Swing = swings(swings.Count - 2)
                End If
                If bcLines.Count <> 0 Then
                    With bcLines(bcLines.Count - 1)
                        .Line.EndPoint = CurrentSwing.EndPoint
                        If bcLines.Count > 1 Then
                            For i = bcLines.Count - 2 To 0 Step -1
                                If (currentTrendDir And .Line.EndPoint.Y > bcLines(i).Line.EndPoint.Y) Or
                                   (Not currentTrendDir And .Line.EndPoint.Y < bcLines(i).Line.EndPoint.Y) Then
                                    Exit For
                                End If
                                If (currentTrendDir And .Line.StartPoint.Y < bcLines(i).Line.StartPoint.Y And .Line.EndPoint.Y <= bcLines(i).Line.EndPoint.Y) Or
                                   (Not currentTrendDir And .Line.StartPoint.Y > bcLines(i).Line.StartPoint.Y And .Line.EndPoint.Y >= bcLines(i).Line.EndPoint.Y) Then
                                    .Line.StartPoint = bcLines(i).Line.StartPoint
                                    .TrendLine.EndPoint = bcLines(i).Line.StartPoint
                                    If bcLines(i).TrendLine.HasPhantomParent Or bcLines(i).TrendLine.HasParent Then
                                        AddObjectToChart(.TrendLine)
                                    End If
                                    Dim c = bcLines.Count - 2
                                    For j = i To c
                                        RemoveObjectFromChart(bcLines(i).TrendLine)
                                        RemoveObjectFromChart(bcLines(i).Line)
                                        RemoveObjectFromChart(bcLines(i).LengthText)
                                        RemoveObjectFromChart(bcLines(i).TargetText)
                                        bcLines.RemoveAt(i)
                                    Next
                                    Exit For
                                End If
                            Next
                        End If
                        .LengthText.Location = AddToY(.Line.EndPoint, Chart.GetRelativeFromRealHeight(If(swingDir, 1, -1) * (SwingLengthTextFontSize + BCLengthTextSpacing)))
                        .LengthText.VerticalAlignment = If(currentTrendDir, LabelVerticalAlignment.Top, LabelVerticalAlignment.Bottom)
                        .LengthText.Text = FormatNumberLengthAndPrefix(Abs(.Line.EndPoint.Y - .Line.StartPoint.Y))
                        .LengthText.Tag = Abs(.Line.EndPoint.Y - .Line.StartPoint.Y)
                        For i = 0 To bcLines.Count - 2
                            If bcLines(i).IsHit = False And Abs(.Line.EndPoint.Y - .Line.StartPoint.Y) >= Abs(bcLines(i).Line.EndPoint.Y - bcLines(i).Line.StartPoint.Y) Then
                                bcLines(i).Line.Pen.Thickness = BCLineHistoryThickness
                                bcLines(i).Line.Pen.DashStyle = TrendLineDashStyle
                                bcLines(i).IsHit = True
                                RemoveObjectFromChart(bcLines(i).LengthText)
                                RemoveObjectFromChart(bcLines(i).TargetText)
                            End If
                        Next
                    End With
                End If
            End If
            For Each item In bcLines
                If Not item.IsHit Then
                    If item.Line.StartPoint.Y <> item.Line.EndPoint.Y Then
                        Dim rangePoint As Double = If(currentTrendDir, 0, Double.MaxValue)
                        For i = item.Line.EndPoint.X - 1 To CurrentBar.Number - 1
                            If currentTrendDir Then
                                rangePoint = Max(rangePoint, Chart.bars(i).Data.High)
                            Else
                                rangePoint = Min(rangePoint, Chart.bars(i).Data.Low)
                            End If
                        Next
                        If currentTrendDir Then
                            item.TargetText.Location = New Point(CurrentBar.Number, Max(item.Line.EndPoint.Y, rangePoint - (item.Line.StartPoint.Y - item.Line.EndPoint.Y)))
                        Else
                            item.TargetText.Location = New Point(CurrentBar.Number, Min(item.Line.EndPoint.Y, rangePoint - (item.Line.StartPoint.Y - item.Line.EndPoint.Y)))
                        End If
                        If CurrentTrend.EndPoint.X > item.Line.EndPoint.X Then
                            item.TargetText.Text = Strings.StrDup(BCTargetTextLineLength, "─") &
                            FormatNumberLengthAndPrefix(Abs(item.Line.StartPoint.Y - item.Line.EndPoint.Y)) ' & " " &
                            item.Line.Pen.Thickness = ConfirmedBCLineThickness
                        Else
                            item.TargetText.Text = ""
                            item.Line.Pen.Thickness = PotentialBCLineThickness
                        End If
                        'FormatNum(CurrentTrend.EndPoint.Y - (item.Line.StartPoint.Y - item.Line.EndPoint.Y))
                    End If
                    End If
            Next


            If evnt Then
                Dim maxAngle As Double = If(CurrentSwing.Direction = Direction.Down, Double.MinValue, Double.MaxValue)
                Dim index As Integer
                For i = swings.Count - 2 To 0 Step -1
                    If swings(i).Direction = CurrentSwing.Direction Then
                        If If(CurrentSwing.Direction = Direction.Down,
                            (CurrentSwing.EndPrice - swings(i).EndPrice) / (CurrentSwing.EndBar - swings(i).EndBar) >= maxAngle,
                            (CurrentSwing.EndPrice - swings(i).EndPrice) / (CurrentSwing.EndBar - swings(i).EndBar) <= maxAngle) Then
                            maxAngle = (CurrentSwing.EndPrice - swings(i).EndPrice) / (CurrentSwing.EndBar - swings(i).EndBar)
                            index = i
                        End If
                    End If
                Next
                If swings.Count > 1 Then
                    If If(CurrentSwing.Direction = Direction.Down,
                                (CurrentSwing.EndPrice - swings(0).StartPrice) / (CurrentSwing.EndBar - swings(0).StartBar) >= maxAngle,
                               (CurrentSwing.EndPrice - swings(0).StartPrice) / (CurrentSwing.EndBar - swings(0).StartBar) <= maxAngle) Then
                        maxAngle = (CurrentSwing.EndPrice - swings(0).StartPrice) / (CurrentSwing.EndBar - swings(0).StartBar)
                        index = -1
                    End If
                End If
                If swings.Count > 1 Then
                    CurrentSwing.SwingChannel.StartPoint = If(index <> -1, swings(index).EndPoint, swings(0).StartPoint)
                    CurrentSwing.SwingChannel.EndPoint = CurrentSwing.EndPoint
                    CurrentSwing.SwingChannel.Pen.Brush = New SolidColorBrush(LiveLineColor)
                    CurrentSwing.SwingChannel.Tag = If(If(index <> -1, swings(index).EndPrice, swings(0).StartPrice) < CurrentSwing.EndPrice, UpFanLineColor, DownFanLineColor)
                    CurrentSwing.SwingChannel.Pen.Thickness = LiveLineThickness

                End If
            End If
            If swings.Count > 1 Then
                swings(swings.Count - 2).SwingChannel.Pen.Thickness = FanLineThickness
                swings(swings.Count - 2).SwingChannel.Pen.Brush = New SolidColorBrush(swings(swings.Count - 2).SwingChannel.Tag)
                swings(swings.Count - 2).SwingChannel.ExtendRight = True
                For i = swings.Count - 3 To 0 Step -1
                    If swings(i).Direction = swings(swings.Count - 2).Direction Then
                        swings(i).SwingChannel.ExtendRight = False
                        Exit For
                    End If
                Next
            End If
            For i = 0 To swings.Count - 1
                Dim item As Swing = swings(i)
                If item IsNot CurrentSwing Then
                    Dim l = LinCalc(item.SwingChannel.StartPoint.X, item.SwingChannel.StartPoint.Y, item.SwingChannel.EndPoint.X, item.SwingChannel.EndPoint.Y, bar.Number)
                    If ((item.Direction = Direction.Up And l <= bar.High) Or
                           (item.Direction = Direction.Down And l >= bar.Low)) And item.SwingChannelCut = False And swings.Count > i + 2 Then
                        item.SwingChannel.Pen.DashStyle = GetDashStyle(item.SwingChannel.Pen.Thickness)
                        item.SwingChannel.Pen.Thickness = HistoryFanLineThickness
                        item.SwingChannel.ExtendRight = False
                        item.SwingChannelCut = True
                        item.SwingChannel.EndPoint = New Point(bar.Number, l)
                    End If
                End If
            Next
            'Dim high As Point = New Point(0, 0)
            'Dim low As Point = New Point(0, Decimal.MaxValue)
            'If mousedriven = False And StartLocation <> 0 Then
            '    For i = StartLocation - 1 To Chart.bars.Count - 1
            '        If Chart.bars(i).Data.High >= high.Y Then high = New Point(i + 1, Chart.bars(i).Data.High)
            '        If Chart.bars(i).Data.Low <= low.Y Then low = New Point(i + 1, Chart.bars(i).Data.Low)
            '    Next
            '    If Chart.bars.Count - 1 = high.X Or Chart.bars.Count - 1 = low.X Then
            '        For i = 0 To Chart.bars.Count - 1
            '            If Not TypeOf Chart.bars(i).Pen.Brush Is SolidColorBrush Then
            '                Chart.bars(i).Pen.Brush = New SolidColorBrush
            '            End If
            '            Dim color As Color
            '            If i >= StartLocation - 1 And i <= Min(high.X, low.X) Then
            '                color = If(high.X < low.X, UpColor, DownColor)
            '            ElseIf i >= StartLocation - 1 And i <= Max(high.X, low.X) Then
            '                color = If(Not high.X < low.X, UpColor, DownColor)
            '            Else
            '                color = Chart.Settings("Bar Color").Value
            '            End If
            '            If CType(Chart.bars(i).Pen.Brush, SolidColorBrush).Color <> color Then
            '                Chart.bars(i).Pen.Brush = New SolidColorBrush(color)
            '                RefreshObject(Chart.bars(i))
            '            End If
            '        Next

            '    End If
            'End If
            BarsBackStartPoint = Chart.bars.Count - StartLocation
        End Sub
        Private mousedriven As Boolean
        Private Sub NewFanLine(direction As Direction)
            FanLines.Add(NewTrendLine(If(direction = Direction.Up, UpFanLineColor, DownFanLineColor)))
            CurrentFan.IsEditable = False
            CurrentFan.IsSelectable = False
            CurrentFan.Pen.Thickness = FanLineThickness
        End Sub
        Private Function GetHighestAnglePoint(direction As Direction) As Point

        End Function
        Private Property CurrentSwing As Swing
            Get
                Return swings(swings.Count - 1)
            End Get
            Set(ByVal value As Swing)
                swings(swings.Count - 1) = value
            End Set
        End Property
        Private Property CurrentFan As TrendLine
            Get
                Return FanLines(FanLines.Count - 1)
            End Get
            Set(ByVal value As TrendLine)
                FanLines(FanLines.Count - 1) = value
            End Set
        End Property
        Public Function GetTextOffset(bar As Integer) As Decimal
            Dim offset As Integer = 0
            For Each item In trendLines
                If Abs(item.EndPoint.X - bar) <= 1 Then
                    offset += TrendLengthTextFontSize
                End If
            Next
            For Each item In bcLines
                If Abs(item.Line.EndPoint.X - bar) <= 1 Then
                    offset += BCLengthTextFontSize
                End If
            Next
            For Each item In swings
                If Abs(item.TL.EndPoint.X - bar) <= 1 Then
                    offset += SwingLengthTextFontSize
                End If
            Next

            Return offset
        End Function
        Protected Overrides Sub Main()
            trendEvent = SwingEvent.None
            If (Round(CurrentBar.High - CurrentTrend.EndPoint.Y, 5) >= Round(TrendRV, 5) AndAlso CurrentBar.Number <> CurrentTrend.EndPoint.X AndAlso currentTrendDir = False) Or
               (Round(CurrentTrend.EndPoint.Y - CurrentBar.Low, 5) >= Round(TrendRV, 5) AndAlso CurrentBar.Number <> CurrentTrend.EndPoint.X AndAlso currentTrendDir = True) Then
                CurrentTrend.TL.Pen.Thickness = TrendLineThickness
                CurrentRegression.Pen.Thickness = ConfirmedRegressionLineThickness
                'RemoveObjectFromChart(CurrentTrend.LengthText)
                trendLines.Add(New Swing(NewTrendLine(If(currentTrendDir, DownRegressionLineColor, UpRegressionLineColor), CurrentTrend.EndPoint, New Point(CurrentBar.Number, If(currentTrendDir, CurrentBar.Low, CurrentBar.High))), Not currentTrendDir))
                regressionLines.Add(NewTrendLine(If(currentTrendDir, DownRegressionLineColor, UpRegressionLineColor), CurrentTrend.StartPoint, CurrentTrend.EndPoint))
                potentialRegressionLine.StartPoint = CurrentTrend.StartPoint
                potentialRegressionLine.EndPoint = CurrentTrend.EndPoint
                potentialRegressionLine.Pen = New Pen(New SolidColorBrush(If(currentTrendDir, DownPotentialRegressionLineColor, UpPotentialRegressionLineColor)), PotentialRegressionLineThickness)

                currentTrendDir = Not currentTrendDir
                CurrentTrend.TL.Pen.Thickness = LastTrendLineThickness
                CurrentRegression.Pen.Thickness = LastConfirmedRegressionLineThickness
                CurrentRegression.IsRegressionLine = True
                CurrentTrend.LengthText = CreateText(AddToY(CurrentTrend.EndPoint, Chart.GetRelativeFromRealHeight(If(currentTrendDir, 1, -1) * (SwingLengthTextFontSize + TrendLengthTextSpacing))), currentTrendDir, Abs(CurrentTrend.EndPrice - CurrentTrend.StartPrice), If(currentTrendDir, UpRegressionLineColor, DownRegressionLineColor))
                StartLocation = CurrentTrend.StartPoint.X
                trendEvent = SwingEvent.NewSwing
                DrawLines()
                BarColorRoutine(CurrentTrend.StartBar, CurrentBar.Number, If(currentTrendDir, UpBarColor, DownBarColor))
            ElseIf (Round(CurrentBar.High, 5) >= Round(CurrentTrend.EndPoint.Y, 5) And currentTrendDir = True) Or
                   (Round(CurrentBar.Low, 5) <= Round(CurrentTrend.EndPoint.Y, 5) And currentTrendDir = False) Then
                CurrentTrend.TL.EndPoint = New Point(CurrentBar.Number, If(currentTrendDir, CurrentBar.High, CurrentBar.Low))
                CurrentRegression.EndPoint = CurrentTrend.TL.EndPoint
                UpdateText(CurrentTrend.LengthText, AddToY(CurrentTrend.EndPoint, Chart.GetRelativeFromRealHeight(If(currentTrendDir, 1, -1) * (SwingLengthTextFontSize + TrendLengthTextSpacing))), Abs(CurrentTrend.EndPrice - CurrentTrend.StartPrice))
                BarColorRoutine(CurrentTrend.StartBar, CurrentBar.Number, If(currentTrendDir, UpBarColor, DownBarColor))
                trendEvent = SwingEvent.Extension
            End If

            rvText.Location = New Point(CurrentBar.Number, CurrentTrend.EndPrice + If(currentTrendDir, -TrendRV, TrendRV))
            rvText.Text = Strings.StrDup(TrendTargetTextLineLength, "─") & " " & FormatNumberLengthAndPrefix(TrendRV) & " RV " '& FormatNum(CurrentTrend.EndPrice + If(currentTrendDir, -1, 1) * TrendRV)
            rvText.Font.Brush = New SolidColorBrush(If(currentTrendDir, DownFanLineColor, UpFanLineColor))
            extendText.Location = New Point(CurrentBar.Number, CurrentTrend.EndPrice)
            extendText.Text = Strings.StrDup(TrendTargetTextLineLength, "─") & " Extend" ' & FormatNum(CurrentTrend.EndPrice)
            extendText.Font.Brush = New SolidColorBrush(If(currentTrendDir, UpFanLineColor, DownFanLineColor))
            potentialRegressionLine.NeutralZoneBarStart = CurrentTrend.EndPoint.X-1
            potentialRegressionLine.ConfirmedZoneBarStart = Integer.MaxValue
            potentialRegressionLine.EndPoint = New Point(CurrentBar.Number, CurrentBar.Close)
            lastConfirmedRegressionLine.UpZoneFillBrush = New SolidColorBrush(If(currentTrendDir, UpFillColor, DownFillColor))
            lastConfirmedRegressionLine.DownZoneFillBrush = New SolidColorBrush(If(currentTrendDir, UpFillColor, DownFillColor))
            lastConfirmedRegressionLine.Coordinates = CurrentTrend.TL.Coordinates


            If IsLastBarOnChart Then
                OnLoaded()
                potentialRegressionLine.RefreshVisual()
            End If
            'If IsLastBarOnChart Then
            'mousedriven = False
            DrawLines(CurrentBar)
            'End If
        End Sub
        Friend Overrides Sub OnLoaded()
            For i = potentialRegressionLine.StartPoint.X - 1 To potentialRegressionLine.EndPoint.X - 1

                If i < CurrentTrend.EndPoint.X Then
                    If currentTrendDir Then
                        Chart.bars(i).Pen.Brush = New SolidColorBrush(UpBarColor)
                    Else
                        Chart.bars(i).Pen.Brush = New SolidColorBrush(DownBarColor)
                    End If
                Else
                    Chart.bars(i).Pen.Brush = New SolidColorBrush(TextLineColor)
                End If
                Chart.bars(i).RefreshVisual()
            Next
        End Sub

        Private Function GetGradientBarBrush(aboveColor As Color, belowColor As Color, x As Integer, point As Double) As LinearGradientBrush
            Dim brush As New LinearGradientBrush With {.StartPoint = New Point(0, 0), .EndPoint = New Point(0, 1)}
            If Chart.bars(x).Data.High < point Then
                brush.GradientStops.Add(New GradientStop(belowColor, 0.5))
            ElseIf Chart.bars(x).Data.Low > point Then
                brush.GradientStops.Add(New GradientStop(aboveColor, 0.5))
            Else
                brush.GradientStops.Add(New GradientStop(aboveColor, 1 - (point - Chart.bars(x).Data.Low) / Chart.bars(x).Data.Range))
                brush.GradientStops.Add(New GradientStop(belowColor, 1 - (point - Chart.bars(x).Data.Low) / Chart.bars(x).Data.Range))
            End If
            Return brush
        End Function
        Sub UpdateText(label As Label, position As Point, value As String)
            If label IsNot Nothing Then
                label.Text = FormatNumberLengthAndPrefix(value)
                label.Tag = value
                label.Location = position
            End If
        End Sub
        Function CreateText(position As Point, direction As Direction, value As String, color As Color) As Label
            Dim lbl = NewLabel(FormatNumberLengthAndPrefix(value), color, New Point(0, 0),, New Font With {.FontSize = TrendLengthTextFontSize, .FontWeight = TrendLengthTextFontWeight}, False) : lbl.HorizontalAlignment = LabelHorizontalAlignment.Right : lbl.IsEditable = True : lbl.IsSelectable = True
            lbl.Tag = value
            lbl.Location = position
            AddHandler lbl.MouseDown, Sub(sender As Object, location As Point)
                                          TrendRV = CDec(CType(sender, Label).Tag) + Chart.GetMinTick
                                          Chart.ReApplyAnalysisTechnique(Me)
                                      End Sub
            If direction = Direction.Up Then lbl.VerticalAlignment = LabelVerticalAlignment.Bottom Else lbl.VerticalAlignment = LabelVerticalAlignment.Top
            Return lbl
        End Function
        Public Overrides Property Name As String = "FanTrend"
        Function FormatNumberLengthAndPrefix(num As Decimal) As String
            Return Round(num * (10 ^ Chart.Settings("DecimalPlaces").Value))
        End Function
        Function FormatNum(num As Decimal) As String
            Return FormatNumber(num, Chart.Settings("DecimalPlaces").Value)
        End Function

#Region "AutoTrendPad"
        Dim toggleColoringButton As Button

        Dim toggleButton As Button

        Dim addMinRVMinMove1 As Button
        Dim subtractMinRVMinMove1 As Button
        Dim minRVBaseValue1 As Button
        Dim currentMinRVPopupBtn1 As Button

        Dim addMinRVMinMove As Button
        Dim subtractMinRVMinMove As Button
        Dim minRVBaseValue As Button
        Dim currentMinRVPopupBtn As Button

        Dim addMaxRVMinMove As Button
        Dim subtractMaxRVMinMove As Button
        Dim maxRVBaseValue As Button
        Dim currentMaxRVPopupBtn As Button

        Dim grabArea As Border
        Dim currentRBPopupBtn As Button
        Dim addRBMinMove As Button
        Dim subtractRBMinMove As Button

        Dim minRVPopup As Popup
        Dim minRVPopupGrid As Grid
        Dim minRVPopup1 As Popup
        Dim minRVPopupGrid1 As Grid
        Dim maxRVPopup As Popup
        Dim maxRVPopupGrid As Grid
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
            Grid.SetRow(toggleColoringButton, 0)
            Grid.SetRow(toggleButton, 1)
            Grid.SetRow(addMinRVMinMove1, 2)
            Grid.SetRow(subtractMinRVMinMove1, 3)
            Grid.SetRow(minRVBaseValue1, 4)
            Grid.SetRow(currentMinRVPopupBtn1, 5)

            Grid.SetRow(divider, 6)

            Grid.SetRow(addMinRVMinMove, 7)
            Grid.SetRow(subtractMinRVMinMove, 8)
            Grid.SetRow(minRVBaseValue, 9)
            Grid.SetRow(currentMinRVPopupBtn, 10)

            Grid.SetRow(grabArea, 11)
            Grid.SetRow(currentRBPopupBtn, 12)
            Grid.SetRow(addRBMinMove, 13)
            Grid.SetRow(subtractRBMinMove, 14)

            'grd.Children.Add(toggleColoringButton)
            'grd.Children.Add(toggleButton)
            grd.Children.Add(addMinRVMinMove1)
            grd.Children.Add(subtractMinRVMinMove1)
            grd.Children.Add(minRVBaseValue1)
            grd.Children.Add(currentMinRVPopupBtn1)

            grd.Children.Add(divider)

            grd.Children.Add(addMinRVMinMove)
            grd.Children.Add(subtractMinRVMinMove)
            grd.Children.Add(minRVBaseValue)
            grd.Children.Add(currentMinRVPopupBtn)


            grd.Children.Add(currentRBPopupBtn)
            grd.Children.Add(addRBMinMove)
            grd.Children.Add(subtractRBMinMove)
        End Sub
        Sub InitControls()
            Dim fontsize As Integer = 16
            Dim col As Brush = Nothing

            toggleColoringButton = New Button With {.Background = col, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = ""}

            toggleButton = New Button With {.Background = Brushes.Yellow, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "/ /"}
            addMinRVMinMove1 = New Button With {.Background = Brushes.LightGray, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "+1"}
            subtractMinRVMinMove1 = New Button With {.Background = Brushes.LightGray, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "-1"}
            minRVBaseValue1 = New Button With {.Background = New SolidColorBrush(Color.FromArgb(255, 230, 235, 255)), .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "-1"}
            currentMinRVPopupBtn1 = New Button With {.Background = Brushes.Cyan, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19}

            addMinRVMinMove = New Button With {.Background = Brushes.LightGray, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "+1"}
            subtractMinRVMinMove = New Button With {.Background = Brushes.LightGray, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "-1"}
            minRVBaseValue = New Button With {.Background = New SolidColorBrush(Color.FromArgb(255, 230, 235, 255)), .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "-1"}
            currentMinRVPopupBtn = New Button With {.Background = Brushes.Cyan, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19}

            addMaxRVMinMove = New Button With {.Background = Brushes.LightGray, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "+1"}
            subtractMaxRVMinMove = New Button With {.Background = Brushes.LightGray, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "-1"}
            maxRVBaseValue = New Button With {.Background = New SolidColorBrush(Color.FromArgb(255, 230, 235, 255)), .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "-1"}
            currentMaxRVPopupBtn = New Button With {.Background = Brushes.Cyan, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19}

            currentRBPopupBtn = New Button With {.Background = Brushes.Red, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19}
            addRBMinMove = New Button With {.Background = Brushes.LightGray, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "+1"}
            subtractRBMinMove = New Button With {.Background = Brushes.LightGray, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "-1"}


            minRVPopup1 = New Popup With {.Placement = PlacementMode.Left, .PlacementTarget = currentMinRVPopupBtn1, .Width = 520, .Height = 310, .StaysOpen = False}
            minRVPopupGrid1 = New Grid With {.Background = Brushes.White}
            minRVPopup1.Child = minRVPopupGrid1
            For x = 1 To 10 : minRVPopupGrid1.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)}) : Next
            For y = 1 To 12 : minRVPopupGrid1.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Star)}) : Next

            minRVPopup = New Popup With {.Placement = PlacementMode.Left, .PlacementTarget = currentMinRVPopupBtn, .Width = 520, .Height = 310, .StaysOpen = False}
            minRVPopupGrid = New Grid With {.Background = Brushes.White}
            minRVPopup.Child = minRVPopupGrid
            For x = 1 To 10 : minRVPopupGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)}) : Next
            For y = 1 To 12 : minRVPopupGrid.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Star)}) : Next

            maxRVPopup = New Popup With {.Placement = PlacementMode.Left, .PlacementTarget = currentMaxRVPopupBtn, .Width = 520, .Height = 310, .StaysOpen = False}
            maxRVPopupGrid = New Grid With {.Background = Brushes.White}
            maxRVPopup.Child = maxRVPopupGrid
            For x = 1 To 10 : maxRVPopupGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)}) : Next
            For y = 1 To 12 : maxRVPopupGrid.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Star)}) : Next

            popupRB = New Popup With {.Placement = PlacementMode.Left, .PlacementTarget = currentRBPopupBtn, .Width = 520, .Height = 310, .StaysOpen = False}
            popupRBGrid = New Grid With {.Background = Brushes.White}
            popupRB.Child = popupRBGrid
            For x = 1 To 10 : popupRBGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)}) : Next
            For y = 1 To 12 : popupRBGrid.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Star)}) : Next

        End Sub
        Sub AddHandlers()
            AddHandler currentMinRVPopupBtn1.Click,
                Sub()
                    minRVPopup1.IsOpen = True
                End Sub

            AddHandler minRVPopup1.Opened,
              Sub()
                  minRVPopupGrid1.Children.Clear()
                  For x = 1 To 10
                      For y = 1 To 12
                          Dim value = Round(((y - 1) * 10 + (1 * Chart.Settings("RangeValue").Value) / Chart.GetMinTick() + x - 1) * Chart.GetMinTick(), 4)
                          Dim btn As New Button With {.Margin = New Thickness(1), .Tag = value, .Content = FormatNumberLengthAndPrefix(value), .FontSize = 14.5, .Foreground = Brushes.Black}
                          Grid.SetRow(btn, y - 1)
                          Grid.SetColumn(btn, x - 1)
                          btn.Background = Brushes.White
                          If Round(TrendRV, 4) = value Then
                              btn.Background = Brushes.LightBlue
                          End If
                          If value = Round(RoundTo(Chart.Settings("RangeValue").Value * TrendRVMultiplier, Chart.GetMinTick), 4) Then
                              btn.Background = New SolidColorBrush(Colors.Orange)
                          End If
                          minRVPopupGrid1.Children.Add(btn)
                          AddHandler btn.Click,
                                      Sub(sender As Object, e As EventArgs)
                                          minRVPopup1.IsOpen = False
                                          If Round(CDec(CType(sender, Button).Tag), 4) <> Round(TrendRV, 4) Then
                                              TrendRV = Round(CType(sender, Button).Tag, Chart.Settings("DecimalPlaces").Value)
                                              ReapplyMe()
                                          End If
                                      End Sub
                      Next
                  Next
              End Sub

            AddHandler currentMinRVPopupBtn.Click,
                Sub()
                    minRVPopup.IsOpen = True
                End Sub

            AddHandler minRVPopup.Opened,
              Sub()
                  minRVPopupGrid.Children.Clear()
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
                          minRVPopupGrid.Children.Add(btn)
                          AddHandler btn.Click,
                                      Sub(sender As Object, e As EventArgs)
                                          minRVPopup.IsOpen = False
                                          If Round(CDec(CType(sender, Button).Tag), 4) <> Round(SwingRV, 4) Then
                                              SwingRV = Round(CType(sender, Button).Tag, 5)
                                              ReapplyMe()
                                          End If
                                      End Sub
                      Next
                  Next
              End Sub

            AddHandler currentMaxRVPopupBtn.Click,
                Sub()
                    maxRVPopup.IsOpen = True
                End Sub

            'AddHandler maxRVPopup.Opened,
            '  Sub()
            '      maxRVPopupGrid.Children.Clear()
            '      For x = 1 To 10
            '          For y = 1 To 12
            '              Dim value = Round(((y - 1) * 10 + (1 * Chart.Settings("RangeValue").Value) / Chart.GetMinTick() + x - 1) * Chart.GetMinTick(), 4)
            '              Dim btn As New Button With {.Margin = New Thickness(1), .Tag = value, .Content = FormatNumberLengthAndPrefix(value), .FontSize = 14.5, .Foreground = Brushes.Black}
            '              Grid.SetRow(btn, y - 1)
            '              Grid.SetColumn(btn, x - 1)
            '              btn.Background = Brushes.White
            '              If Round(MaxRV, 4) = value Then
            '                  btn.Background = Brushes.LightBlue
            '              End If
            '              maxRVPopupGrid.Children.Add(btn)
            '              AddHandler btn.Click,
            '                          Sub(sender As Object, e As EventArgs)
            '                              maxRVPopup.IsOpen = False
            '                              If Round(CDec(CType(sender, Button).Tag), 4) <> Round(MaxRV, 4) Then
            '                                  MaxRV = CType(sender, Button).Tag
            '                                  ReapplyMe
            '                              End If
            '                          End Sub
            '          Next
            '      Next
            '  End Sub

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
                            ElseIf value = RoundTo(SwingRV / 2, Chart.GetMinTick) Or value = RoundTo(SwingRV / 3, Chart.GetMinTick) Or value = RoundTo(SwingRV / 4, Chart.GetMinTick) Then
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

            'AddHandler addMaxRVMinMove.Click,
            '    Sub()
            '        MaxRV += Chart.GetMinTick()
            '        ReapplyMe
            '    End Sub
            'AddHandler subtractMaxRVMinMove.Click,
            '    Sub()
            '        MaxRV -= Chart.GetMinTick()
            '        ReapplyMe
            '    End Sub
            'AddHandler maxRVBaseValue.Click,
            '    Sub(sender As Object, e As EventArgs)
            '        If Round(sender.CommandParameter, 5) <> Round(MaxRV, 5) Then
            '            MaxRV = sender.CommandParameter
            '            ReapplyMe
            '        End If
            '    End Sub
            AddHandler addMinRVMinMove1.Click,
               Sub()
                   TrendRV += Round(Chart.GetMinTick(), Chart.Settings("DecimalPlaces").Value)
                   ReapplyMe()
               End Sub
            AddHandler subtractMinRVMinMove1.Click,
                Sub()
                    TrendRV -= Round(Chart.GetMinTick(), Chart.Settings("DecimalPlaces").Value)
                    ReapplyMe()
                End Sub
            AddHandler minRVBaseValue1.Click,
                Sub(sender As Object, e As EventArgs)
                    If Round(sender.CommandParameter, 5) <> Round(SwingRV, 5) Then
                        TrendRV = Round(sender.CommandParameter, Chart.Settings("DecimalPlaces").Value)
                        ReapplyMe()
                    End If
                End Sub

            AddHandler addMinRVMinMove.Click,
                Sub()
                    SwingRV += Round(Chart.GetMinTick(), 5)
                    ReapplyMe()
                End Sub
            AddHandler subtractMinRVMinMove.Click,
                Sub()
                    SwingRV -= Round(Chart.GetMinTick(), 5)
                    ReapplyMe()
                End Sub
            AddHandler minRVBaseValue.Click,
                Sub(sender As Object, e As EventArgs)
                    If Round(sender.CommandParameter, 5) <> Round(SwingRV, 5) Then
                        SwingRV = Round(sender.CommandParameter, 5)
                        ReapplyMe()
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
        Private Sub ReapplyMe()
            Chart.ReApplyAnalysisTechnique(Me)
            For Each item In Chart.AnalysisTechniques
                If TypeOf item.AnalysisTechnique Is DayBox Then
                    Chart.ReApplyAnalysisTechnique(item.AnalysisTechnique)
                End If
            Next
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
            If addMinRVMinMove IsNot Nothing Then
                addMinRVMinMove1.Background = Brushes.LightGray
                subtractMinRVMinMove1.Background = Brushes.LightGray

                minRVBaseValue1.Content = FormatNumberLengthAndPrefix(RoundTo(Chart.Settings("RangeValue").Value * TrendRVMultiplier, Chart.GetMinTick))
                minRVBaseValue1.CommandParameter = Round(RoundTo(Chart.Settings("RangeValue").Value * TrendRVMultiplier, Chart.GetMinTick), Chart.Settings("DecimalPlaces").Value)
                minRVBaseValue1.Background = Brushes.White

                currentMinRVPopupBtn1.Content = FormatNumberLengthAndPrefix(TrendRV)
                currentMinRVPopupBtn1.Background = Brushes.LightBlue

                addMinRVMinMove.Background = Brushes.LightGray
                subtractMinRVMinMove.Background = Brushes.LightGray

                minRVBaseValue.Content = FormatNumberLengthAndPrefix(RoundTo(Chart.Settings("RangeValue").Value * SwingRVMultiplier, Chart.GetMinTick))
                minRVBaseValue.CommandParameter = Round(RoundTo(Chart.Settings("RangeValue").Value * SwingRVMultiplier, Chart.GetMinTick), Chart.Settings("DecimalPlaces").Value)
                minRVBaseValue.Background = Brushes.White

                currentMinRVPopupBtn.Content = FormatNumberLengthAndPrefix(SwingRV)
                currentMinRVPopupBtn.Background = Brushes.LightBlue

                addMaxRVMinMove.Background = Brushes.LightGray
                subtractMaxRVMinMove.Background = Brushes.LightGray

                maxRVBaseValue.Content = FormatNumberLengthAndPrefix(RoundTo(Chart.Settings("RangeValue").Value * SwingRVMultiplier, Chart.GetMinTick))
                maxRVBaseValue.CommandParameter = Round(RoundTo(Chart.Settings("RangeValue").Value * SwingRVMultiplier, Chart.GetMinTick), Chart.Settings("DecimalPlaces").Value)
                maxRVBaseValue.Background = Brushes.White

                'currentMaxRVPopupBtn.Content = FormatNumberLengthAndPrefix(MaxRV)
                'currentMaxRVPopupBtn.Background = Brushes.LightBlue

                currentRBPopupBtn.Content = FormatNumberLengthAndPrefix(Chart.Settings("RangeValue").Value)
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
