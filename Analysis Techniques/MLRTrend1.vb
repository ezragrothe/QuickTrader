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
    Public Class MLRTrend1
        Inherits AnalysisTechnique
        Implements IChartPadAnalysisTechnique
        Public Overrides Property Name As String = "MLRTrend1"
        Public Sub New(ByVal chart As Chart)
            MyBase.New(chart)
            Description = "Test analysis technique."
            If chart IsNot Nothing Then
                AddHandler chart.ChartMouseDraggedEvent, AddressOf ChartMouseDrag
                AddHandler chart.ChartMouseDownEvent, AddressOf ChartMouseDrag
                AddHandler chart.ChartMouseUpEvent,
                    Sub()
                        doubleClick = False
                    End Sub
                'AddHandler chart.ChartMouseDoubleClickEvent,
                '    Sub(sender As Object, location As Point)
                '        If location.X < chart.bars.Count Then
                '            doubleClick = True
                '            StartLocation = location.X
                '            DrawLines()
                '        End If
                '    End Sub
            End If
        End Sub
        Dim doubleClick As Boolean
        Private Sub ChartMouseDrag(sender As Object, location As Point)
            If Keyboard.GetKeyStates(Key.CapsLock) = KeyStates.Toggled Or doubleClick Or Keyboard.IsKeyDown(SetStartPointHotkey) Then
                If location.X > 0 Then
                    StartLocation = location.X
                    DrawLines()
                End If
            End If
        End Sub
        Dim swings As OrderedDictionary ' List(Of Swing)
        Dim lastSwingChannel As TrendLine
        Dim lastSwingPotentialChannel As TrendLine
        Dim extensionText As Label
        Dim newTrendText As Label
        Dim currentSwingBCLengths As List(Of Label)
        Dim swingDir As OrderedDictionary  'Boolean
        Dim targetText As OrderedDictionary 'Label
        Dim lastChannel As OrderedDictionary
        Dim channels As List(Of Channel)
        Dim lastStartColoringBar As Integer
        Dim lastEndColoringIndex As Integer
        Dim lastColorDirection As Direction
        Dim currentTrendLine As TrendLine
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

        <Input> Public Property MinRV As Decimal = 2
        <Input> Public Property BasePriceRVMultiplier As Double = 2
        <Input> Public Property SwingBCLengthRVMultiplier As Double = 2

        <Input(, "Coloring")> Property TextColor As Color = Colors.Gray
        <Input> Property UpConfirmedChannelColor As Color = Colors.Green
        <Input> Property DownConfirmedChannelColor As Color = Colors.Red
        <Input> Property UpPotentialChannelColor As Color = Colors.Yellow
        <Input> Property DownPotentialChannelColor As Color = Colors.Blue
        <Input> Property UpBarColor As Color = Colors.Green
        <Input> Property NeutralBarColor As Color = Colors.Gray
        <Input> Property DownBarColor As Color = Colors.Red
        <Input> Property OSSwingColor As Color = Colors.Red
        <Input> Property ISSwingColor As Color = Colors.Red
        <Input> Property OSLineColor As Color = Colors.Red
        <Input> Property GapColor As Color = Colors.Red
        '<Input> Property BarColoringOff As Boolean = False
        <Input(, "Properties")> Property TrendChannelThickness As Decimal = 1
        <Input> Property LastTrendChannelThickness As Decimal = 1
        <Input> Property HistoryTrendChannelThickness As Decimal = 1
        <Input> Property SwingChannelThickness As Decimal = 1
        <Input> Property HistorySwingChannelThickness As Decimal = 1
        <Input> Property TrendLineThickness As Decimal = 1
        <Input> Property SwingLineThickness As Decimal = 1
        <Input> Property OSLineThickness As Decimal = 1
        <Input> Property OSLineLastThickness As Decimal = 1
        <Input> Property GapLineThickness As Decimal = 1
        <Input> Property SwingBCLengthTextFontSize As Decimal = 11
        <Input> Property SwingBCLengthTextFontWeight As FontWeight = FontWeights.Bold
        <Input> Property LengthTextFontSize As Decimal = 11
        <Input> Property LengthTextFontWeight As FontWeight = FontWeights.Bold
        '<Input> Property TargetTextFontSize As Decimal = 11
        '<Input> Property TargetTextFontWeight As FontWeight = FontWeights.Bold
        <Input> Property TargetTextLineLength As Integer = 12
        <Input> Property RVTargetTextSpacing As Integer = 12
        <Input> Property RVTargetTextFontSize As Integer = 12
        <Input> Property RVTargetTextFontWeight As FontWeight = FontWeights.Bold
        <Input> Property DisplayAllSwingLines As Boolean = True
        <Input> Property LastSwingIsChannel As Boolean = True
        <Input> Property AlternateTrendColoring As Boolean = True
        <Input> Public Property ChannelBarsBack As Integer = 20
        <Input> Public Property SetStartPointHotkey As Key = Key.LeftCtrl
        Dim StartLocation As Integer
        Property SpaceBelowTexts As Decimal = 1.5
        Dim textStackingPoints As Dictionary(Of Point, Integer)

        '<Input> Property DisplayHorizontalLines As Boolean = True
        Structure RVSwing
            Property RV As Decimal
            Property Swing As Swing
            Public Sub New(rv As Decimal, swing As Swing)
                Me.RV = rv
                Me.Swing = swing
            End Sub
        End Structure
        Public Function GetTextOffset(bar As Integer) As Decimal
            If bar = 161 Then
                Dim a As New Object
            End If
            Dim uniqueCount As Integer = 0
            For Each p In rvPoints
                For Each s In CurSwings(p)
                    If Abs(s.Swing.EndBar - bar) <= 2 Then
                        uniqueCount += 1
                    End If
                Next
            Next
            For Each t In currentSwingBCLengths
                If t IsNot Nothing AndAlso Abs(t.Location.X - bar) <= 2 Then
                    uniqueCount += 1
                End If
            Next
            Return uniqueCount * LengthTextFontSize ' + If(uniqueCount > 0, SpaceBelowTexts, 0)
        End Function
        Protected Overrides Sub Begin()
            MyBase.Begin()
            StartLocation = Chart.bars.Count - ChannelBarsBack
            'swingDir = New OrderedDictionary ' False
            'swings = New OrderedDictionary ' New List(Of Swing)
            'targetText = New OrderedDictionary ' label
            'lastChannel = New OrderedDictionary
            'For i = MinRV To MaxRV Step StepRV
            '    swingDir.Add(i, False)
            '    swings.Add(i, New List(Of Swing))
            '    CType(swings(CObj(i)), List(Of Swing)).Add(New Swing(NewTrendLine(Colors.Gray, New Point(CurrentBar.Number, CurrentBar.Close), New Point(CurrentBar.Number, CurrentBar.Close), True), False))
            '    targetText.Add(i, NewLabel("── " & FormatNumberLengthAndPrefix(i) & " RV ", TextColor, New Point(0, 0), True, , False))
            '    CType(targetText(CObj(i)), Label).Font.FontSize = TargetTextFontSize
            '    CType(targetText(CObj(i)), Label).Font.FontWeight = TargetTextFontWeight
            '    lastChannel.Add(i, NewTrendLine(Colors.Gray))
            '    CurLastChannel(i).IsSelectable = False
            '    CurLastChannel(i).IsEditable = False
            '    CurLastChannel(i).Pen.Thickness = 0
            '    CurLastChannel(i).OuterPen.Brush = New SolidColorBrush(TextColor)
            '    CurLastChannel(i).IsRegressionLine = True
            '    CurLastChannel(i).OuterPen.Thickness = 0
            '    CurLastChannel(i).HasParallel = True
            '    CurLastChannel(i).ExtendRight = True
            'Next
            channels = New List(Of Channel)
            rvPoints = New List(Of Point)
            rvPoints.Add(New Point(CurrentBar.Number, CurrentBar.Avg))
            rvPoints.Add(New Point(CurrentBar.Number, CurrentBar.Avg))
            lastSwingChannel = NewTrendLine(TextColor, LastSwingIsChannel) : lastSwingChannel.Pen.Thickness = 0 : lastSwingChannel.OuterPen.Thickness = SwingChannelThickness : lastSwingChannel.IsRegressionLine = True : lastSwingChannel.ExtendRight = True : lastSwingChannel.HasParallel = True
            lastSwingPotentialChannel = NewTrendLine(TextColor, LastSwingIsChannel) : lastSwingPotentialChannel.Pen.Thickness = 0 : lastSwingPotentialChannel.OuterPen.Thickness = SwingChannelThickness : lastSwingPotentialChannel.IsRegressionLine = True : lastSwingPotentialChannel.ExtendRight = True : lastSwingPotentialChannel.HasParallel = True
            currentSwingBCLengths = New List(Of Label)
            currentSwingDir = True
            swingEvent = SwingEvent.None
            textStackingPoints = New Dictionary(Of Point, Integer)
            currentSwings = New Dictionary(Of Point, List(Of RVSwing))
            CurSwings(New Point(CurrentBar.Number, CurrentBar.Close)).Add(New RVSwing(MinRV, New Swing(NewTrendLine(Colors.Gray, New Point(CurrentBar.Number, CurrentBar.Close), New Point(CurrentBar.Number, CurrentBar.Close), True), False)))
            extensionText = NewLabel("", TextColor, New Point(0, 0)) : extensionText.Font.FontSize = RVTargetTextFontSize : extensionText.Font.FontWeight = RVTargetTextFontWeight
            extensionText.IsSelectable = False
            extensionText.IsEditable = False
            'newTrendText = NewLabel("", TextColor, New Point(0, 0)) : newTrendText.Font.FontSize = TargetTextFontSize : newTrendText.Font.FontWeight = TargetTextFontWeight
            'newTrendText.IsSelectable = False
            'newTrendText.IsEditable = False
            lastStartColoringBar = 0
            lastEndColoringIndex = 0
            lastColorDirection = Direction.Up
            Dim col As Brush
            If AlternateTrendColoring Then
                col = GetSplitBrush(Color.FromArgb(255, 30, 255, 0), Colors.Red, 0.5, False)
            Else
                col = GetSplitBrush(Color.FromArgb(255, 166, 166, 166), Color.FromArgb(255, 140, 140, 140), 0.5, False)
            End If
            toggleColoringButton.Background = col
            gapLines = New Dictionary(Of Point, TrendLine)
            osLines = New Dictionary(Of Point, TrendLine)
        End Sub
        Property CurSwings(rvPoint As Point) As List(Of RVSwing)
            Get
                If Not currentSwings.ContainsKey(rvPoint) Then
                    currentSwings.Add(rvPoint, New List(Of RVSwing))
                End If
                Return currentSwings(rvPoint)
            End Get
            Set(value As List(Of RVSwing))
                If Not currentSwings.ContainsKey(rvPoint) Then
                    currentSwings.Add(rvPoint, New List(Of RVSwing))
                End If
                currentSwings(rvPoint) = value
            End Set
        End Property
        Private osLines As Dictionary(Of Point, TrendLine)
        Property OSLine(rvPoint As Point) As TrendLine
            Get
                If Not osLines.ContainsKey(rvPoint) Then
                    osLines.Add(rvPoint, NewTrendLine(New Pen(New SolidColorBrush(OSLineColor), OSLineThickness)))
                End If
                Return osLines(rvPoint)
            End Get
            Set(value As TrendLine)
                If Not osLines.ContainsKey(rvPoint) Then
                    osLines.Add(rvPoint, NewTrendLine(New Pen(New SolidColorBrush(OSLineColor), OSLineThickness)))
                End If
                osLines(rvPoint) = value
            End Set
        End Property
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
        Private Function OSLineExists(rvpoint As Point) As Boolean
            Return osLines.ContainsKey(rvpoint)
        End Function
        Private Function GapLineExists(rvpoint As Point) As Boolean
            Return gapLines.ContainsKey(rvpoint)
        End Function
        Dim currentSwingDir As Boolean
        Dim currentSwings As Dictionary(Of Point, List(Of RVSwing))
        Dim rvPoints As List(Of Point)
        Dim swingEvent As SwingEvent
        Private Property CurrentSwing As Swing
            Get
                Return swings(0)
            End Get
            Set(value As Swing)
                swings(0) = value
            End Set
        End Property
        Private Property LastRVPoint(Optional indexBack As Integer = 1) As Point
            Get
                Return rvPoints(rvPoints.Count - indexBack)
            End Get
            Set(value As Point)
                rvPoints(rvPoints.Count - indexBack) = value
            End Set
        End Property
        Dim prevStartLocation As Integer
        Sub DrawLines()
            Dim h As New Point(0, 0) : Dim l As New Point(0, Double.MaxValue)
            For i = StartLocation To Chart.bars.Count - 1
                If Chart.bars(i).Data.High > h.Y Then h = New Point(i, Chart.bars(i).Data.High)
                If Chart.bars(i).Data.Low < l.Y Then l = New Point(i, Chart.bars(i).Data.Low)
            Next
            Dim startPoint As Integer = Min(h.X, l.X)
            If startPoint <> prevStartLocation Then
                ChannelBarsBack = Chart.bars.Count - StartLocation
                ReapplyMe()
                'For Each item In channels
                '    RemoveObjectFromChart(item.TL)
                '    RemoveObjectFromChart(item.TargetText)
                'Next
                'channels.Clear()
                'For i = 1 To rvPoints.Count - 1
                '    IterateDrawLines(1, i, rvPoints(i).Y > rvPoints(i - 1).Y)
                'Next
                ' FinishDrawLines()

            End If
            prevStartLocation = startPoint
        End Sub
        Sub CalculateSwingBCTexts(bar As Integer, direction As Direction)
            Dim cpoint As Point
            Dim bpoint As New Point(0, If(direction, 0, Double.MaxValue))
            If bar = 227 Then
                Dim a As New Object
            End If
            For i = bar To LastRVPoint(2).X - 1 Step -1
                If direction And Round(Chart.bars(i).Data.High, 5) > bpoint.Y Then
                    bpoint = New Point(i, Chart.bars(i).Data.High)
                ElseIf Not direction And Round(Chart.bars(i).Data.Low, 5) < bpoint.Y Then
                    bpoint = New Point(i, Chart.bars(i).Data.Low)
                End If
            Next
            cpoint = bpoint
            For i = bpoint.X To bar
                If Not direction And Round(Chart.bars(i).Data.High, 5) >= Round(cpoint.Y, 5) Then
                    cpoint = New Point(i, Chart.bars(i).Data.High)
                ElseIf direction And Round(Chart.bars(i).Data.Low, 5) <= Round(cpoint.Y, 5) Then
                    cpoint = New Point(i, Chart.bars(i).Data.Low)
                End If
            Next
            If bpoint.X = bar Then
                If currentSwingBCLengths(currentSwingBCLengths.Count - 1) IsNot Nothing Then
                    currentSwingBCLengths.Add(Nothing)
                End If
            ElseIf cpoint.X = bar And Round(Abs(cpoint.Y - bpoint.Y), 5) >= Round(SwingBCLengthRVMultiplier * Chart.Settings("RangeValue").Value, 5) Then
                Dim foundBigger As Boolean = False
                For Each item In currentSwingBCLengths
                    If item IsNot Nothing AndAlso Round(item.Tag, 5) > Round(Abs(cpoint.Y - bpoint.Y), 5) Then
                        foundBigger = True
                    End If

                Next
                If foundBigger = False Then
                    lastSwingChannel.OuterPen.DashStyle = TrendLineDashStyle
                    lastSwingChannel.OuterPen.Thickness = HistorySwingChannelThickness
                    lastSwingPotentialChannel.OuterPen.Thickness = SwingChannelThickness
                    lastSwingPotentialChannel.Coordinates = New LineCoordinates(lastSwingChannel.StartPoint, AddToX(cpoint, 1))
                    lastSwingPotentialChannel.OuterPen.Brush = New SolidColorBrush(If(currentSwingDir, UpPotentialChannelColor, DownPotentialChannelColor))
                Else
                    lastSwingChannel.OuterPen.DashStyle = DashStyles.Solid
                    lastSwingChannel.OuterPen.Thickness = SwingChannelThickness
                    lastSwingPotentialChannel.OuterPen.Thickness = 0
                End If
                If currentSwingBCLengths(currentSwingBCLengths.Count - 1) Is Nothing Then
                    currentSwingBCLengths(currentSwingBCLengths.Count - 1) = NewLabel("", TextColor, New Point(0, 0))
                    With currentSwingBCLengths(currentSwingBCLengths.Count - 1)
                        .Font.FontSize = SwingBCLengthTextFontSize
                        .Font.FontWeight = SwingBCLengthTextFontWeight
                        .HorizontalAlignment = LabelHorizontalAlignment.Right
                        .VerticalAlignment = If(direction, VerticalAlignment.Top, VerticalAlignment.Bottom)
                        .IsEditable = False
                        AddHandler .MouseDown, Sub(sender As Object, location As Point)
                                                   MinRV = CDec(CType(sender, Label).Tag)
                                                   ReapplyMe()
                                               End Sub
                    End With
                End If
                currentSwingBCLengths(currentSwingBCLengths.Count - 1).Location = New Point(bar + 1, If(direction, Chart.bars(bar).Data.Low, Chart.bars(bar).Data.High))
                currentSwingBCLengths(currentSwingBCLengths.Count - 1).Text = FormatNumberLengthAndPrefix(Abs(cpoint.Y - bpoint.Y))
                currentSwingBCLengths(currentSwingBCLengths.Count - 1).Tag = Abs(cpoint.Y - bpoint.Y)
            End If
        End Sub
        Sub IterateDrawLines(startRVIndex As Integer, endRVIndex As Integer, currentSwingDir As Boolean)
            Dim extendChannel As Channel = Nothing
            Dim k As Integer
            If CurrentBar.Number = 429 Then
                Dim a As New Object
            End If
            While k < channels.Count
                Dim c = channels(k)
                If (c.Direction = Direction.Up And Round(rvPoints(endRVIndex).Y, 5) <= Round(c.APoint.Y, 5)) Or
                   (c.Direction = Direction.Down And Round(rvPoints(endRVIndex).Y, 5) >= Round(c.APoint.Y, 5)) Then
                    'delete channel
                    RemoveObjectFromChart(c.TL)
                    RemoveObjectFromChart(c.TargetText)
                    'c.TargetText = Nothing
                    channels.RemoveAt(k)
                    If k > 0 Then k -= 1
                    If k >= channels.Count Then Exit While
                    c = channels(k)
                ElseIf c.IsConfirmed = False And ((c.Direction = Direction.Up And Round(rvPoints(endRVIndex).Y, 5) <= Round(c.CPoint.Y, 5)) Or
                   (c.Direction = Direction.Down And Round(rvPoints(endRVIndex).Y, 5) >= Round(c.CPoint.Y, 5))) Then
                    'extend cpoint
                    extendChannel = c
                    c.CPoint = rvPoints(endRVIndex)
                    c.TL.EndPoint = rvPoints(endRVIndex)
                ElseIf (((c.Direction = Direction.Up And Round(rvPoints(endRVIndex).Y, 5) >= Round(c.DPoint.Y, 5)) Or
                       (c.Direction = Direction.Down And Round(rvPoints(endRVIndex).Y, 5) <= Round(c.DPoint.Y, 5)))) Then
                    'extend dpoint and confirm
                    c.IsConfirmed = True
                    If c.IsCut = False Then
                        c.DPoint = rvPoints(endRVIndex)
                        c.TL.OuterPen = New Pen(New SolidColorBrush(If(c.Direction = Direction.Up, UpConfirmedChannelColor, DownConfirmedChannelColor)), TrendChannelThickness)
                        c.TL.EndPoint = rvPoints(endRVIndex)
                        'If c.TL.HasParent = True Then
                        If c.TargetText Is Nothing Then
                            c.TargetText = NewLabel(Strings.StrDup(TargetTextLineLength, "─") & " " &
                                                    FormatNumberLengthAndPrefix(Abs(c.BPoint.Y - c.CPoint.Y)) & " RV " & vbNewLine & FormatNumberLengthAndPrefix(rvPoints(endRVIndex).Y + If(currentSwingDir, -1, 1) * Abs(c.BPoint.Y - c.CPoint.Y)),
                                                                     If(c.Direction = Direction.Up, UpConfirmedChannelColor, DownConfirmedChannelColor),
                                                                     New Point(CurrentBar.Number, rvPoints(endRVIndex).Y + If(c.Direction = Direction.Up, -1, 1) * Abs(c.BPoint.Y - c.CPoint.Y)), True)
                            c.TargetText.Font.FontSize = RVTargetTextFontSize
                            c.TargetText.Font.FontWeight = RVTargetTextFontWeight
                            c.TargetText.IsSelectable = False
                            c.TargetText.IsEditable = False
                        Else
                            AddObjectToChart(c.TargetText)
                        End If
                        'End If
                    Else
                        RemoveObjectFromChart(c.TL)
                        RemoveObjectFromChart(c.TargetText)
                        'c.TargetText = Nothing
                        channels.RemoveAt(k)
                        If k > 0 Then k -= 1
                        If k >= channels.Count Then Exit While
                        c = channels(k)
                    End If
                ElseIf c.IsConfirmed = True And ((c.Direction = Direction.Up And (Round(c.DPoint.Y - rvPoints(endRVIndex).Y, 5)) >= Round(Abs(c.BPoint.Y - c.CPoint.Y), 5)) Or
                           (c.Direction = Direction.Down And Round((rvPoints(endRVIndex).Y - c.DPoint.Y), 5) >= Round(Abs(c.BPoint.Y - c.CPoint.Y), 5))) Then
                    'bc length hit
                    RemoveObjectFromChart(c.TargetText)
                    'c.TargetText = Nothing
                    c.TL.OuterPen.DashStyle = TrendLineDashStyle
                    c.IsCut = True
                End If
                k += 1
            End While
            Dim apoint As New Point(startRVIndex, rvPoints(endRVIndex).Y)
            Dim bpoint As Point = New Point(startRVIndex, If(currentSwingDir, Decimal.MaxValue, 0))
            For i = endRVIndex - 1 To startRVIndex Step -1
                If (currentSwingDir = True And Round(rvPoints(i).Y, 5) > Round(apoint.Y, 5)) Or (currentSwingDir = False And Round(rvPoints(i).Y, 5) < Round(apoint.Y, 5)) Then
                    'apoint = New Point(i, rvPoints(i).Y)
                    bpoint = apoint
                    For j = i To endRVIndex
                        If If(currentSwingDir, Round(rvPoints(j).Y, 5) < Round(bpoint.Y, 5), Round(rvPoints(j).Y, 5) > Round(bpoint.Y, 5)) Then
                            bpoint = New Point(j, rvPoints(j).Y)
                        End If
                    Next
                    Exit For
                End If
            Next
            If bpoint.Y <> Decimal.MaxValue Then
                Dim bLength As Decimal = Round(Abs(rvPoints(endRVIndex).Y - bpoint.Y), 5)
                Dim foundChannel As Boolean
                For i = endRVIndex - 1 To startRVIndex Step -1
                    If (currentSwingDir = True And Round(rvPoints(i).Y, 5) > Round(apoint.Y, 5)) Or (currentSwingDir = False And Round(rvPoints(i).Y, 5) < Round(apoint.Y, 5)) Then
                        apoint = New Point(i, rvPoints(i).Y)
                        For j = i To startRVIndex Step -1
                            If (currentSwingDir = True And Round(rvPoints(j).Y, 5) > Round(apoint.Y, 5)) Or (currentSwingDir = False And Round(rvPoints(j).Y, 5) < Round(apoint.Y, 5)) Then
                                Exit For
                            End If
                            If Round(If(currentSwingDir, apoint.Y - rvPoints(j).Y, rvPoints(j).Y - apoint.Y), 5) > Round(bLength, 5) Then
                                foundChannel = True
                                Exit For
                            End If
                        Next
                        If foundChannel Then Exit For
                    End If
                Next
                If foundChannel Then
                    Dim dup As Boolean
                    For Each c In channels
                        If c.CPoint.X = rvPoints(endRVIndex).X Then
                            dup = True
                        End If
                    Next
                    If Not dup Then
                        channels.Add(New Channel(NewTrendLine(Colors.Gray, rvPoints(apoint.X), rvPoints(endRVIndex))))
                        channels(channels.Count - 1).TL.IsRegressionLine = True : channels(channels.Count - 1).TL.HasParallel = True : channels(channels.Count - 1).TL.Pen.Thickness = 0 : channels(channels.Count - 1).TL.ExtendRight = True
                        channels(channels.Count - 1).TL.OuterPen = New Pen(New SolidColorBrush(If(Not currentSwingDir, UpPotentialChannelColor, DownPotentialChannelColor)), TrendChannelThickness)
                        channels(channels.Count - 1).IsConfirmed = False
                        channels(channels.Count - 1).Direction = Not currentSwingDir
                        channels(channels.Count - 1).APoint = rvPoints(apoint.X) : channels(channels.Count - 1).BPoint = rvPoints(bpoint.X)
                        channels(channels.Count - 1).CPoint = rvPoints(endRVIndex) : channels(channels.Count - 1).DPoint = rvPoints(bpoint.X)
                        channels(channels.Count - 1).TargetText = NewLabel("", If(currentSwingDir, DownConfirmedChannelColor, UpConfirmedChannelColor),
                                                                 New Point(CurrentBar.Number, rvPoints(endRVIndex).Y + If(currentSwingDir, -1, 1) * Abs(bpoint.Y - rvPoints(endRVIndex).Y)), True)
                        channels(channels.Count - 1).TargetText.Font.FontSize = RVTargetTextFontSize
                        channels(channels.Count - 1).TargetText.Font.FontWeight = RVTargetTextFontWeight
                        channels(channels.Count - 1).TargetText.IsSelectable = False
                        channels(channels.Count - 1).TargetText.IsEditable = False
                        If extendChannel Is Nothing Then extendChannel = channels(channels.Count - 1)
                    End If
                    If extendChannel IsNot Nothing Then
                        Dim i As Integer
                        While i < channels.Count
                            If extendChannel.APoint.X < channels(i).APoint.X And channels(i).IsConfirmed = True And channels(i).IsCut = True Then
                                RemoveObjectFromChart(channels(i).TL)
                                RemoveObjectFromChart(channels(i).TargetText)
                                channels.RemoveAt(i)
                                i -= 1
                            End If
                            i += 1
                        End While
                    End If
                    If dup Then
                        If extendChannel IsNot Nothing Then
                            extendChannel.APoint = rvPoints(apoint.X)
                            extendChannel.TL.StartPoint = rvPoints(apoint.X)
                        End If
                    End If
                End If
            End If
        End Sub
        Sub FinishDrawLines()
            Dim j As Integer
            While j <= channels.Count - 1
                If channels(j).TL IsNot Nothing AndAlso channels(j).TL.StartPoint.X < StartLocation Then
                    RemoveObjectFromChart(channels(j).TL)
                    RemoveObjectFromChart(channels(j).TargetText)
                    channels.RemoveAt(j)
                    j -= 1
                End If
                j += 1
            End While
            Dim potUp, potDown, confUp, confDown As Boolean
            For i = channels.Count - 1 To 0 Step -1
                If channels(i).Direction = Direction.Up And channels(i).IsConfirmed = False And potUp = False Then
                    channels(i).TL.OuterPen.Thickness = LastTrendChannelThickness
                    potUp = True
                ElseIf channels(i).Direction = Direction.Down And channels(i).IsConfirmed = False And potDown = False Then
                    channels(i).TL.OuterPen.Thickness = LastTrendChannelThickness
                    potDown = True
                ElseIf channels(i).Direction = Direction.Up And channels(i).IsConfirmed = True And channels(i).IsCut = False And confUp = False Then
                    channels(i).TL.OuterPen.Thickness = LastTrendChannelThickness
                    confUp = True
                ElseIf channels(i).Direction = Direction.Down And channels(i).IsConfirmed = True And channels(i).IsCut = False And confDown = False Then
                    channels(i).TL.OuterPen.Thickness = LastTrendChannelThickness
                    confDown = True
                ElseIf channels(i).IsCut = True Then
                    channels(i).TL.OuterPen.Thickness = HistoryTrendChannelThickness
                Else
                    channels(i).TL.OuterPen.Thickness = TrendChannelThickness
                End If
            Next
        End Sub
        Protected Overrides Sub Main()
            Dim i As Integer = 0
            Dim textStackPosition As Integer = 0
            Dim rv = MinRV
            If CurrentBar.Number = 127 Then
                Dim a As New Object
            End If
            swingEvent = SwingEvent.None
            If (Round(CurrentBar.High - LastRVPoint.Y, 5) >= Round(rv, 5) AndAlso CurrentBar.Number <> LastRVPoint.X AndAlso currentSwingDir = False) Or
               (Round(LastRVPoint.Y - CurrentBar.Low, 5) >= Round(rv, 5) AndAlso CurrentBar.Number <> LastRVPoint.X AndAlso currentSwingDir = True) Then
                currentSwingDir = Not currentSwingDir
                rvPoints.Add(New Point(CurrentBar.Number, If(currentSwingDir, CurrentBar.High, CurrentBar.Low)))
                swingEvent = SwingEvent.NewSwing
            ElseIf (Round(CurrentBar.High, 5) >= Round(LastRVPoint.Y, 5) And currentSwingDir = True) Or
                   (Round(CurrentBar.Low, 5) <= Round(LastRVPoint.Y, 5) And currentSwingDir = False) Then
                ' extension
                LastRVPoint = New Point(CurrentBar.Number, If(currentSwingDir, CurrentBar.High, CurrentBar.Low))
                swingEvent = SwingEvent.Extension
            End If
            If swingEvent = SwingEvent.NewSwing Then
                Dim s = New RVSwing(MinRV, New Swing(NewTrendLine(TextColor, LastRVPoint(2), LastRVPoint(1), True), currentSwingDir))
                lastSwingChannel.Coordinates = New LineCoordinates(LastRVPoint(2), LastRVPoint(1))
                s.Swing.TL.Pen.Thickness = SwingLineThickness
                s.Swing.LengthText =
                    CreateText(AddToY(LastRVPoint(1), If(currentSwingDir, 1, -1) * Chart.GetRelativeFromRealHeight(SpaceBelowTexts + textStackPosition * LengthTextFontSize)),
                              currentSwingDir,
                               Abs(LastRVPoint(2).Y - LastRVPoint(1).Y),
                               If(currentSwingDir, UpConfirmedChannelColor, DownConfirmedChannelColor), True)
                textStackPosition += 1
                's.Swing.Channel = NewTrendLine(Colors.Gray, s.Swing.TL.StartPoint, s.Swing.TL.EndPoint) : s.Swing.Channel.IsRegressionLine = True : s.Swing.Channel.HasParallel = True : s.Swing.Channel.Pen.Thickness = 0 : s.Swing.Channel.ExtendRight = True
                's.Swing.Channel.OuterPen = New Pen(New SolidColorBrush(If(currentSwingDir, SwingUpColor, SwingDownColor)), ChannelLineThickness)
                's.Swing.ChannelHorizontalLine = NewTrendLine(If(currentSwingDir, DownConfirmedChannelColor, UpConfirmedChannelColor), s.Swing.TL.StartPoint, AddToX(s.Swing.TL.StartPoint, 1), DisplayHorizontalLines) : s.Swing.ChannelHorizontalLine.ExtendRight = True
                s.Swing.TargetText = NewLabel(Strings.StrDup(TargetTextLineLength, "─") & " " & FormatNumberLengthAndPrefix(rv) & " RV " & FormatNum(s.Swing.EndPrice + If(currentSwingDir, -1, 1) * rv),
                                                            TextColor,
                                                          New Point(CurrentBar.Number, s.Swing.EndPrice + If(currentSwingDir, -1, 1) * rv), True)
                s.Swing.TargetText.Font.FontSize = RVTargetTextFontSize
                s.Swing.TargetText.Font.FontWeight = RVTargetTextFontWeight
                s.Swing.TargetText.IsSelectable = False
                s.Swing.TargetText.IsEditable = False
                For Each swing In CurSwings(LastRVPoint(3))
                    If Round(swing.RV, 5) = Round(rv, 5) Then
                        'If swing.Swing.Channel IsNot Nothing Then RemoveObjectFromChart(swing.Swing.Channel)
                        If swing.Swing.TargetText IsNot Nothing Then RemoveObjectFromChart(swing.Swing.TargetText)
                        If swing.Swing.ChannelHorizontalLine IsNot Nothing Then RemoveObjectFromChart(swing.Swing.ChannelHorizontalLine)
                        'swing.Swing.Channel = Nothing
                        swing.Swing.TargetText = Nothing
                    End If
                Next
                CurSwings(LastRVPoint(2)).Add(s)
                For Each item In currentSwingBCLengths
                    RemoveObjectFromChart(item)
                Next
                currentSwingBCLengths.Clear()
                currentSwingBCLengths.Add(Nothing)
                For i = LastRVPoint(2).X - 1 To LastRVPoint.X - 1
                    CalculateSwingBCTexts(i, currentSwingDir)
                Next



            End If
            If swingEvent = SwingEvent.Extension Then
                For Each swing In CurSwings(LastRVPoint(2))
                    If swing.RV = MinRV Then
                        'extend base rv swing
                        swing.Swing.TL.EndPoint = LastRVPoint
                        lastSwingChannel.EndPoint = LastRVPoint
                        UpdateText(swing.Swing.LengthText,
                                   AddToY(LastRVPoint, If(currentSwingDir, 1, -1) * Chart.GetRelativeFromRealHeight(SpaceBelowTexts + textStackPosition * LengthTextFontSize)),
                                   Abs(LastRVPoint(2).Y - LastRVPoint(1).Y), If(currentSwingDir, UpConfirmedChannelColor, DownConfirmedChannelColor))
                        textStackPosition += 1
                        'If swing.Swing.Channel IsNot Nothing Then swing.Swing.Channel.Coordinates = swing.Swing.TL.Coordinates
                        Exit For
                    End If
                Next

            End If
            If swingEvent <> SwingEvent.NewSwing Then
                If currentSwingBCLengths.Count = 0 Then currentSwingBCLengths.Add(Nothing)
                CalculateSwingBCTexts(CurrentBar.Number - 1, currentSwingDir)
            End If


            Chart.bars(CurrentBar.Number - 1).Pen.Brush = New SolidColorBrush(NeutralBarColor)



            If swingEvent <> SwingEvent.None Then
                'outside swings
                If rvPoints.Count > 2 Then
                    For Each swing In CurSwings(LastRVPoint(2))
                        If swing.RV = MinRV Then
                            If ((currentSwingDir And LastRVPoint.Y >= LastRVPoint(3).Y) Or (currentSwingDir = False And LastRVPoint.Y <= LastRVPoint(3).Y)) Then
                                swing.Swing.TL.Pen = New Pen(New SolidColorBrush(OSSwingColor), SwingLineThickness)
                            Else
                                swing.Swing.TL.Pen = New Pen(New SolidColorBrush(ISSwingColor), SwingLineThickness)
                            End If
                            Exit For
                        End If
                    Next
                End If

                'check for outside swing
                If rvPoints.Count > 1 Then
                    OSLine(LastRVPoint(2)).StartPoint = LastRVPoint(2)
                    OSLine(LastRVPoint(2)).EndPoint = AddToX(LastRVPoint(2), 2)
                    OSLine(LastRVPoint(2)).ExtendRight = True
                    OSLine(LastRVPoint(2)).Pen.Thickness = OSLineLastThickness
                End If
                If rvPoints.Count > 2 AndAlso ((currentSwingDir And LastRVPoint.Y >= LastRVPoint(3).Y) Or (currentSwingDir = False And LastRVPoint.Y <= LastRVPoint(3).Y)) Then
                    RemoveObjectFromChart(OSLine(LastRVPoint(3)))
                End If
                If rvPoints.Count > 3 Then
                    OSLine(LastRVPoint(4)).EndPoint = New Point(LastRVPoint(2).X, LastRVPoint(4).Y)
                    OSLine(LastRVPoint(4)).Pen.Thickness = OSLineThickness
                    OSLine(LastRVPoint(4)).ExtendRight = False
                End If
                'check for gap swing
                If rvPoints.Count > 2 AndAlso ((currentSwingDir And LastRVPoint.Y >= LastRVPoint(3).Y) Or (currentSwingDir = False And LastRVPoint.Y <= LastRVPoint(3).Y)) Then
                    GapLine(LastRVPoint(3)).StartPoint = LastRVPoint(3)
                    GapLine(LastRVPoint(3)).EndPoint = AddToX(LastRVPoint(3), 20)
                    GapLine(LastRVPoint(3)).ExtendRight = True
                End If
                If rvPoints.Count > 3 AndAlso (GapLineExists(LastRVPoint(4)) And ((currentSwingDir And LastRVPoint.Y >= LastRVPoint(4).Y) Or (currentSwingDir = False And LastRVPoint.Y <= LastRVPoint(4).Y))) Then
                    RemoveObjectFromChart(GapLine(LastRVPoint(4)))
                End If
                If rvPoints.Count > 4 AndAlso (GapLineExists(LastRVPoint(5)) And ((currentSwingDir And LastRVPoint(2).Y > LastRVPoint(5).Y) Or (currentSwingDir = False And LastRVPoint(2).Y < LastRVPoint(5).Y))) Then
                    GapLine(LastRVPoint(5)).EndPoint = New Point(LastRVPoint(2).X, LastRVPoint(5).Y)
                    GapLine(LastRVPoint(5)).ExtendRight = False
                End If
                lastSwingChannel.OuterPen.DashStyle = DashStyles.Solid
                lastSwingChannel.OuterPen.Thickness = SwingChannelThickness
                lastSwingPotentialChannel.OuterPen.Thickness = 0

                'first step is to find the startpoint 
                Dim lowPoint As New Point(0, Decimal.MaxValue)
                Dim highPoint As New Point(0, 0)
                For i = 0 To rvPoints.Count - 1
                    If rvPoints(i).Y < lowPoint.Y Then lowPoint = New Point(i, rvPoints(i).Y)
                    If rvPoints(i).Y > highPoint.Y Then highPoint = New Point(i, rvPoints(i).Y)
                Next
                Dim startPoint As Point = If(lowPoint.X < highPoint.X, lowPoint, highPoint)

                'rangepoint is startpoint
                Dim curSetOfRVPoints As List(Of Point) = rvPoints.ToArray.ToList
                Dim upColoredAreas As New List(Of LineCoordinates)
                Dim downColoredAreas As New List(Of LineCoordinates)
                If CurrentBar.Number = 236 Then
                    Dim a As New Object
                End If
                Do
                    Dim curMinRV As Decimal = Decimal.MaxValue
                    For i = 1 To curSetOfRVPoints.Count - 1
                        If Round(Abs(curSetOfRVPoints(i).Y - curSetOfRVPoints(i - 1).Y), 5) <> 0 Then
                            curMinRV = Min(Round(Abs(curSetOfRVPoints(i).Y - curSetOfRVPoints(i - 1).Y), 5), curMinRV)
                        End If
                    Next
                    If curMinRV = Decimal.MaxValue Then Continue Do
                    curMinRV += Chart.GetMinTick()
                    If Round(curMinRV, 5) = 0.33 Then
                        Dim a As New Object
                    End If
                    Dim newSwingDir As Boolean = If(lowPoint.X < highPoint.X, True, False)
                    Dim newSetOfRVPoints As New List(Of Point)
                    newSetOfRVPoints.Add(rvPoints(startPoint.X))
                    newSetOfRVPoints.Add(rvPoints(startPoint.X))
                    For i = startPoint.X To rvPoints.Count - 1
                        If (Round(rvPoints(i).Y - newSetOfRVPoints(newSetOfRVPoints.Count - 1).Y, 5) >= Round(curMinRV, 5) AndAlso newSwingDir = False) Or
                       (Round(newSetOfRVPoints(newSetOfRVPoints.Count - 1).Y - rvPoints(i).Y, 5) >= Round(curMinRV, 5) AndAlso newSwingDir = True) Then
                            If Round(curMinRV, 5) = 0.33 Then
                                Dim a As New Object
                            End If
                            newSwingDir = Not newSwingDir
                            newSetOfRVPoints.Add(rvPoints(i))
                        ElseIf (Round(rvPoints(i).Y, 5) >= Round(newSetOfRVPoints(newSetOfRVPoints.Count - 1).Y, 5) And newSwingDir = True) Or
                           (Round(rvPoints(i).Y, 5) <= Round(newSetOfRVPoints(newSetOfRVPoints.Count - 1).Y, 5) And newSwingDir = False) Then
                            ' extension
                            newSetOfRVPoints(newSetOfRVPoints.Count - 1) = rvPoints(i)
                        End If
                    Next

                    'If newSetOfRVPoints.Count < 2 Then Exit Do

                    Dim foundDuplicate As Boolean = False
                    Dim foundSmaller As Boolean = False
                    Dim pnt As Point
                    If newSetOfRVPoints.Count > 1 Then
                        pnt = newSetOfRVPoints(newSetOfRVPoints.Count - 2)
                    Else
                        pnt = rvPoints(startPoint.X)
                    End If
                    For i = 0 To CurSwings(pnt).Count - 1
                        If newSetOfRVPoints(newSetOfRVPoints.Count - 1).X = rvPoints(rvPoints.Count - 1).X Then
                            If Round(CurSwings(pnt)(i).RV, 5) = Round(curMinRV, 5) Then
                                If CurSwings(pnt)(i).Swing.StartBar = pnt.X Then
                                    CurSwings(pnt)(i).Swing.TL.EndPoint = newSetOfRVPoints(newSetOfRVPoints.Count - 1)
                                    If CurSwings(pnt)(i).Swing.Channel IsNot Nothing Then CurSwings(pnt)(i).Swing.Channel.EndPoint = newSetOfRVPoints(newSetOfRVPoints.Count - 1)
                                End If
                            End If
                        End If
                    Next
                    If newSetOfRVPoints.Count > 2 Then
                        For i = 0 To newSetOfRVPoints.Count - 3
                            For Each s In CurSwings(newSetOfRVPoints(i))
                                If Round(s.RV, 5) = Round(curMinRV, 5) And s.Swing.Channel IsNot Nothing Then
                                    If (s.Swing.Direction = Direction.Down And newSetOfRVPoints(newSetOfRVPoints.Count - 1).Y <= s.Swing.EndPrice) Or
                                   (s.Swing.Direction = Direction.Up And newSetOfRVPoints(newSetOfRVPoints.Count - 1).Y >= s.Swing.EndPrice) Or
                                   (s.Swing.Direction = Direction.Down And newSetOfRVPoints(newSetOfRVPoints.Count - 1).Y >= s.Swing.StartPrice) Or
                                   (s.Swing.Direction = Direction.Up And newSetOfRVPoints(newSetOfRVPoints.Count - 1).Y <= s.Swing.StartPrice) Then
                                        RemoveObjectFromChart(s.Swing.Channel)
                                        RemoveObjectFromChart(s.Swing.ChannelHorizontalLine)
                                        If Round(curMinRV, 5) = 0.33 Then
                                            Dim a As New Object
                                        End If
                                    End If
                                End If
                            Next
                        Next
                    End If
                    For i = 0 To CurSwings(pnt).Count - 1
                        If newSetOfRVPoints(newSetOfRVPoints.Count - 1).X = rvPoints(rvPoints.Count - 1).X Then
                            If Round(CurSwings(pnt)(i).RV, 5) = Round(curMinRV, 5) And Round(curMinRV, 5) <> MinRV Then
                                If CurSwings(pnt)(i).Swing.StartBar = pnt.X Then
                                    'found prev swing, extend it
                                    'move line and text
                                    Dim dup As Boolean = False
                                    For Each s In CurSwings(pnt)
                                        If s.Swing IsNot CurSwings(pnt)(i).Swing And s.Swing.StartBar = CurSwings(pnt)(i).Swing.StartBar And s.Swing.EndBar = CurSwings(pnt)(i).Swing.EndBar Then
                                            dup = True
                                        End If
                                    Next
                                    If newSetOfRVPoints.Count > 1 Then
                                        If Not dup Then '
                                            UpdateText(CurSwings(pnt)(i).Swing.LengthText,
                                                AddToY(newSetOfRVPoints(newSetOfRVPoints.Count - 1), If(newSwingDir, 1, -1) * Chart.GetRelativeFromRealHeight(SpaceBelowTexts + textStackPosition * LengthTextFontSize)),
                                                Abs(newSetOfRVPoints(newSetOfRVPoints.Count - 1).Y - newSetOfRVPoints(newSetOfRVPoints.Count - 2).Y),
                                                If(newSwingDir, UpConfirmedChannelColor, DownConfirmedChannelColor))
                                            If Round(Abs(newSetOfRVPoints(newSetOfRVPoints.Count - 1).Y - newSetOfRVPoints(newSetOfRVPoints.Count - 2).Y), 5) = 0 Then
                                                Dim a As New Object
                                            End If
                                            'CurSwings(pnt)(i).Swing.LengthText.Text = FormatNumberLengthAndPrefix(Abs(newSetOfRVPoints(newSetOfRVPoints.Count - 1).Y - newSetOfRVPoints(newSetOfRVPoints.Count - 2).Y)) & " " & FormatNumberLengthAndPrefix(CurSwings(pnt)(i).RV)
                                            textStackPosition += 1
                                        Else
                                            RemoveObjectFromChart(CurSwings(pnt)(i).Swing.LengthText)
                                        End If
                                    End If
                                    'move channel line and target text
                                    'CurSwings(pnt)(i).Swing.Channel.Coordinates = CurSwings(pnt)(i).Swing.TL.Coordinates
                                    foundDuplicate = True
                                    Exit For
                                End If

                            End If
                        End If
                        If CurSwings(pnt)(i).Swing.StartBar = pnt.X And CurSwings(pnt)(i).Swing.EndBar = newSetOfRVPoints(newSetOfRVPoints.Count - 1).X Then
                            'swing = New RVSwing(curMinRV, swing.Swing)
                            'CurSwings(pnt)(i) = New RVSwing(curMinRV, CurSwings(pnt)(i).Swing)
                            foundDuplicate = True
                            Exit For
                        End If
                    Next
                    If foundDuplicate = False And Round(Abs(newSetOfRVPoints(newSetOfRVPoints.Count - 1).Y - pnt.Y), 5) <> 0 And
                    Round(Abs(newSetOfRVPoints(newSetOfRVPoints.Count - 1).Y - newSetOfRVPoints(newSetOfRVPoints.Count - 2).Y), 5) >= Round(curMinRV, 5) Then
                        'unique startpoint/rv. create new swing line
                        'create line and text
                        Dim s = New RVSwing(curMinRV, New Swing(NewTrendLine(TextColor, pnt, newSetOfRVPoints(newSetOfRVPoints.Count - 1), DisplayAllSwingLines), newSwingDir))
                        s.Swing.TL.Pen.Thickness = SwingLineThickness '
                        s.Swing.LengthText =
                                        CreateText(AddToY(newSetOfRVPoints(newSetOfRVPoints.Count - 1), If(newSwingDir, 1, -1) * Chart.GetRelativeFromRealHeight(SpaceBelowTexts + textStackPosition * LengthTextFontSize)),
                                                  newSwingDir,
                                                   Abs(newSetOfRVPoints(newSetOfRVPoints.Count - 1).Y - pnt.Y),
                                                       If(newSwingDir, UpConfirmedChannelColor, DownConfirmedChannelColor), True)
                        If Round(Abs(newSetOfRVPoints(newSetOfRVPoints.Count - 1).Y - pnt.Y), 5) = 0 Then
                            Dim a As New Object
                        End If
                        's.Swing.LengthText.Text = FormatNumberLengthAndPrefix(Abs(newSetOfRVPoints(newSetOfRVPoints.Count - 1).Y - pnt.Y)) & " " & FormatNumberLengthAndPrefix(s.RV)
                        textStackPosition += 1
                        'create channel and target text
                        's.Swing.Channel = NewTrendLine(Colors.Gray, s.Swing.TL.StartPoint, s.Swing.TL.EndPoint) : s.Swing.Channel.IsRegressionLine = True : s.Swing.Channel.HasParallel = True : s.Swing.Channel.Pen.Thickness = 0 : s.Swing.Channel.ExtendRight = True
                        's.Swing.Channel.OuterPen = New Pen(New SolidColorBrush(If(newSwingDir, SwingUpColor, SwingDownColor)), ChannelLineThickness)
                        's.Swing.ChannelHorizontalLine = NewTrendLine(If(newSwingDir, SwingDownColor, SwingUpColor), s.Swing.TL.StartPoint, AddToX(s.Swing.TL.StartPoint, 1), DisplayHorizontalLines) : s.Swing.ChannelHorizontalLine.ExtendRight = True
                        's.Swing.TargetText = NewLabel(Strings.StrDup(TargetTextLineLength, "─") & " " & FormatNumberLengthAndPrefix(curMinRV) & " RV " & FormatNumberLengthAndPrefix(newSetOfRVPoints(newSetOfRVPoints.Count - 1).Y + If(currentSwingDir, -1, 1) * curMinRV),
                        'If (newSwingDir, DownConfirmedChannelColor, UpConfirmedChannelColor),
                        '                                              New Point(CurrentBar.Number, newSetOfRVPoints(newSetOfRVPoints.Count - 1).Y + If(newSwingDir, -1, 1) * curMinRV))
                        's.Swing.TargetText.Font.FontSize = TargetTextFontSize
                        's.Swing.TargetText.Font.FontWeight = TargetTextFontWeight
                        CurSwings(pnt).Add(s)
                    End If
                    'End If
                    If newSetOfRVPoints.Count > 1 Then
                        If newSwingDir Then upColoredAreas.Add(New LineCoordinates(newSetOfRVPoints(newSetOfRVPoints.Count - 2), newSetOfRVPoints(newSetOfRVPoints.Count - 1))) Else downColoredAreas.Add(New LineCoordinates(newSetOfRVPoints(newSetOfRVPoints.Count - 2), newSetOfRVPoints(newSetOfRVPoints.Count - 1)))
                    End If
                    'If newSetOfRVPoints.Count > 2 Then
                    '    For Each swing In CurSwings(newSetOfRVPoints(newSetOfRVPoints.Count - 3))
                    '        If swing.Swing.TargetText IsNot Nothing Then
                    '            RemoveObjectFromChart(swing.Swing.TargetText)
                    '            swing.Swing.TargetText = Nothing
                    '        End If
                    '    Next
                    'End If
                    curSetOfRVPoints = newSetOfRVPoints.ToArray.ToList
                Loop While curSetOfRVPoints.Count > 2
                ' color bars
                Dim index As Integer = rvPoints.Count - 1
                    Dim currentPoint As Decimal = LastRVPoint.Y
                    While index > 0 AndAlso ((currentSwingDir = True And Round(rvPoints(index).Y, 5) <= Round(LastRVPoint.Y, 5)) Or (currentSwingDir = False And Round(rvPoints(index).Y, 5) >= Round(LastRVPoint.Y, 5)))
                        index -= 1
                    End While

                    Dim rangePoint As Point = New Point(0, rvPoints(index).Y)
                    If index <> 0 Then
                        For i = index To rvPoints.Count - 1
                            If (currentSwingDir = True And Round(rvPoints(i).Y, 5) <= Round(rangePoint.Y, 5)) Or (currentSwingDir = False And Round(rvPoints(i).Y, 5) >= Round(rangePoint.Y, 5)) Then
                                rangePoint = New Point(i, rvPoints(i).Y)
                            End If
                        Next
                    End If
                    If rangePoint.X < rvPoints.Count - 3 Then
                        Dim startColorBar As Integer = -1
                        For i = 0 To CurrentBar.Number - 1
                            If TypeOf Chart.bars(i).Pen.Brush Is SolidColorBrush AndAlso CType(Chart.bars(i).Pen.Brush, SolidColorBrush).Color <> NeutralBarColor Then
                                startColorBar = i
                            End If
                            If startColorBar <> -1 AndAlso TypeOf Chart.bars(i).Pen.Brush Is SolidColorBrush AndAlso CType(Chart.bars(i).Pen.Brush, SolidColorBrush).Color = NeutralBarColor Then
                                startColorBar = i
                                Exit For
                            End If
                        Next

                        If AlternateTrendColoring Then
                            If startColorBar >= rvPoints(rangePoint.X).X Then
                                If lastColorDirection <> currentSwingDir Then
                                    lastStartColoringBar = Max(startColorBar - 1, 0)
                                End If
                                If lastColorDirection <> currentSwingDir Or currentTrendLine Is Nothing Then
                                    Dim s = Max(startColorBar, rvPoints(rangePoint.X).X)
                                    'If startColorBar = rvPoints(rangePoint.X).X Then
                                    currentTrendLine = NewTrendLine(New Pen(New SolidColorBrush(If(currentSwingDir, UpBarColor, DownBarColor)), TrendLineThickness),
                                                                New Point(s, If(currentSwingDir, Chart.bars(s - 1).Data.Low, Chart.bars(s - 1).Data.High)),
                                                                New Point(CurrentBar.Number, If(currentSwingDir, CurrentBar.High, CurrentBar.Low)))
                                    'End If
                                Else
                                    currentTrendLine.EndPoint = New Point(CurrentBar.Number, If(currentSwingDir, CurrentBar.High, CurrentBar.Low))
                                End If
                                lastEndColoringIndex = rvPoints.Count - 1
                                lastColorDirection = currentSwingDir
                            End If
                            BarColorRoutine(Max(startColorBar, rvPoints(rangePoint.X).X) + 1, CurrentBar.Number, If(currentSwingDir, UpBarColor, DownBarColor))
                        Else
                            lastStartColoringBar = rvPoints(rangePoint.X).X - 1
                            lastEndColoringIndex = rvPoints.Count - 1
                            lastColorDirection = currentSwingDir
                            BarColorRoutine(rvPoints(rangePoint.X).X + 1, CurrentBar.Number, If(currentSwingDir, UpBarColor, DownBarColor))
                        End If
                    ElseIf rvPoints.Count > 2 And AlternateTrendColoring Then
                        Dim s As Integer
                        For i = CurrentBar.Number - 1 To 0 Step -1
                            If TypeOf Chart.bars(i).Pen.Brush Is SolidColorBrush AndAlso CType(Chart.bars(i).Pen.Brush, SolidColorBrush).Color = If(currentSwingDir, UpBarColor, DownBarColor) Then
                                s = i
                                Exit For
                            End If
                        Next
                        Dim p = If(currentSwingDir, Chart.bars(s).Data.High, Chart.bars(s).Data.Low)
                        If (currentSwingDir = True And rvPoints(rvPoints.Count - 1).Y >= p) Or (currentSwingDir = False And rvPoints(rvPoints.Count - 1).Y <= p) Then
                            If lastColorDirection <> currentSwingDir Then
                                lastStartColoringBar = rvPoints(rvPoints.Count - 2).X
                            End If
                            lastEndColoringIndex = rvPoints.Count - 1
                            BarColorRoutine(rvPoints(rvPoints.Count - 2).X + 1, CurrentBar.Number, If(currentSwingDir, UpBarColor, DownBarColor))
                            If lastColorDirection <> currentSwingDir Or currentTrendLine Is Nothing Then
                                currentTrendLine = NewTrendLine(New Pen(New SolidColorBrush(If(currentSwingDir, UpBarColor, DownBarColor)), TrendLineThickness), rvPoints(rvPoints.Count - 2), rvPoints(rvPoints.Count - 1))
                            Else
                                currentTrendLine.EndPoint = rvPoints(rvPoints.Count - 1)
                            End If
                            lastColorDirection = currentSwingDir
                        End If
                    End If
                    ' calculate channels
                    If CurrentBar.Number > StartLocation Then
                        IterateDrawLines(1, rvPoints.Count - 1, currentSwingDir)
                    End If
                End If
                Dim minChannel As Channel = New Channel(Nothing) With {.APoint = New Point(0, 0)}
            'If rvPoints.Count - 2 > lastEndColoringIndex Then
            '    extensionText.Location = New Point(CurrentBar.Number, rvPoints(lastEndColoringIndex).Y)
            '    extensionText.Text = Strings.StrDup(TargetTextLineLength, "─") & " Ext Trend " & FormatNum(rvPoints(lastEndColoringIndex).Y)
            '    extensionText.Font.Brush = New SolidColorBrush(If(lastColorDirection = Direction.Up, UpConfirmedChannelColor, DownConfirmedChannelColor))
            '    newTrendText.Location = New Point(CurrentBar.Number, rvPoints(lastEndColoringIndex + 1).Y)
            '    newTrendText.Text = Strings.StrDup(TargetTextLineLength, "─") & " New Trend " & FormatNum(rvPoints(lastEndColoringIndex + 1).Y)
            '    newTrendText.Font.Brush = New SolidColorBrush(If(lastColorDirection = Direction.Down, UpConfirmedChannelColor, DownConfirmedChannelColor))
            '    'ElseIf rvPoints.Count - 2 = lastEndColoringIndex Then
            '    '    newTrendText.Location = New Point(CurrentBar.Number, rvPoints(lastEndColoringIndex).Y)
            '    '    newTrendText.Text = Strings.StrDup(TargetTextLineLength, "─") & " New Trend " & FormatNum(rvPoints(lastEndColoringIndex).Y)
            '    '    newTrendText.Font.Brush = New SolidColorBrush(If(lastColorDirection = Direction.Down, UpConfirmedChannelColor, DownConfirmedChannelColor))
            '    '    extensionText.Location = New Point(CurrentBar.Number, rvPoints(rvPoints.Count - 1).Y)
            '    '    extensionText.Text = Strings.StrDup(TargetTextLineLength, "─") & " Ext Trend " & FormatNum(rvPoints(rvPoints.Count - 1).Y)
            '    '    extensionText.Font.Brush = New SolidColorBrush(If(lastColorDirection = Direction.Up, UpConfirmedChannelColor, DownConfirmedChannelColor))
            'Else
            '    newTrendText.Location = New Point(CurrentBar.Number, If(lastColorDirection, Chart.bars(lastStartColoringBar).Data.Low, Chart.bars(lastStartColoringBar).Data.High))
            '    newTrendText.Text = Strings.StrDup(TargetTextLineLength, "─") & " New Trend " & If(lastColorDirection, FormatNum(Chart.bars(lastStartColoringBar).Data.Low), FormatNum(Chart.bars(lastStartColoringBar).Data.High))
            '    newTrendText.Font.Brush = New SolidColorBrush(If(lastColorDirection = Direction.Down, UpConfirmedChannelColor, DownConfirmedChannelColor))
            extensionText.Location = New Point(CurrentBar.Number + RVTargetTextSpacing, rvPoints(rvPoints.Count - 1).Y)
            extensionText.Text = Strings.StrDup(TargetTextLineLength, "─") & " Ext " & FormatNum(rvPoints(rvPoints.Count - 1).Y)
            extensionText.Font.Brush = New SolidColorBrush(TextColor) 'If(lastColorDirection = Direction.Up, UpConfirmedChannelColor, DownConfirmedChannelColor))
            'End If
            ''update extension text
            'Dim minChannel As Channel = New Channel(Nothing) With {.APoint = New Point(0, 0)}
            'For i = channels.Count - 1 To 0 Step -1
            '    If channels(i).APoint.X > minChannel.APoint.X Or (channels(i).IsConfirmed And channels(i).APoint.X = minChannel.APoint.X) Then
            '        minChannel = channels(i)
            '    End If
            'Next
            'If minChannel.APoint.X <> 0 Then
            '    If minChannel.IsConfirmed Then
            '        extensionText.Location = New Point(CurrentBar.Number, minChannel.DPoint.Y)
            '        extensionText.Text = Strings.StrDup(TargetTextLineLength, "─") & " Ext Trend " & FormatNum(minChannel.DPoint.Y)
            '        extensionText.Font.Brush = New SolidColorBrush(If(minChannel.Direction = Direction.Up, UpConfirmedChannelColor, DownConfirmedChannelColor))
            '        newTrendText.Location = New Point(CurrentBar.Number, minChannel.APoint.Y)
            '        newTrendText.Text = Strings.StrDup(TargetTextLineLength, "─") & " New Trend " & FormatNum(minChannel.APoint.Y)
            '        newTrendText.Font.Brush = New SolidColorBrush(If(minChannel.Direction = Direction.Down, UpConfirmedChannelColor, DownConfirmedChannelColor))
            '    Else
            '        extensionText.Location = New Point(CurrentBar.Number, minChannel.APoint.Y)
            '        extensionText.Text = Strings.StrDup(TargetTextLineLength, "─") & " Ext Trend " & FormatNum(minChannel.APoint.Y)
            '        extensionText.Font.Brush = New SolidColorBrush(If(minChannel.Direction = Direction.Down, UpConfirmedChannelColor, DownConfirmedChannelColor))
            '        newTrendText.Location = New Point(CurrentBar.Number, minChannel.BPoint.Y)
            '        newTrendText.Text = Strings.StrDup(TargetTextLineLength, "─") & " New Trend " & FormatNum(minChannel.BPoint.Y)
            '        newTrendText.Font.Brush = New SolidColorBrush(If(minChannel.Direction = Direction.Up, UpConfirmedChannelColor, DownConfirmedChannelColor))
            '    End If
            'End If
            'update extension text
            If IsLastBarOnChart Then
                'For Each c In channels
                'If c.TargetText IsNot Nothing Then
                '        c.TargetText.Location = New Point(CurrentBar.Number, c.DPoint.Y + If(c.Direction = Direction.Up, -1, 1) * Abs(c.BPoint.Y - c.CPoint.Y))
                '        c.TargetText.Text = Strings.StrDup(TargetTextLineLength, "─") & " " & FormatNumberLengthAndPrefix(Abs(c.BPoint.Y - c.CPoint.Y)) & " RV " & FormatNum(c.TargetText.Location.Y)
                '    End If
                'Next
                For Each swing In CurSwings(LastRVPoint(2))
                    If swing.RV = MinRV Then
                        If swing.Swing.TargetText IsNot Nothing Then
                            If Round(swing.RV, 5) = Round(MinRV, 5) Then
                                swing.Swing.TargetText.Text = Strings.StrDup(TargetTextLineLength, "─") & " " & FormatNumberLengthAndPrefix(rv) & " RV " & FormatNum(swing.Swing.EndPrice + If(currentSwingDir, -1, 1) * rv)
                                'swing.Swing.TargetText.Location = New Point(Chart.GetRelativeFromRealX(Chart.ActualWidth - Chart.Settings("PriceBarWidth").Value - 5), swing.Swing.EndPrice + If(currentSwingDir, -1, 1) * rv)
                                swing.Swing.TargetText.Location = New Point(CurrentBar.Number + RVTargetTextSpacing, swing.Swing.EndPrice + If(currentSwingDir, -1, 1) * rv)
                            Else
                                swing.Swing.TargetText.Text = Strings.StrDup(TargetTextLineLength, "─") & " " & FormatNumberLengthAndPrefix(rv) & " RV " & FormatNum(swing.Swing.EndPrice + If(currentSwingDir, -1, 1) * rv)
                                swing.Swing.TargetText.Location = New Point(CurrentBar.Number, swing.Swing.EndPrice + If(currentSwingDir, -1, 1) * rv)
                            End If
                        End If
                    End If
                Next
                FinishDrawLines()
            End If

        End Sub

        Sub UpdateText(label As Label, position As Point, value As String, color As Color)
            If label IsNot Nothing Then
                label.Text = FormatNumberLengthAndPrefix(value)
                label.Tag = value
                label.Location = AddToX(position, 0)
                'label.Font.Brush = New SolidColorBrush(color)
            End If
        End Sub
        Function CreateText(position As Point, direction As Direction, value As String, color As Color, addToChart As Boolean) As Label
            Dim lbl = NewLabel(FormatNumberLengthAndPrefix(value), TextColor, New Point(0, 0), addToChart, New Font With {.FontSize = LengthTextFontSize, .FontWeight = LengthTextFontWeight}, False) : lbl.HorizontalAlignment = LabelHorizontalAlignment.Right : lbl.IsEditable = True : lbl.IsSelectable = True
            lbl.Tag = value
            lbl.Location = AddToX(position, 0)
            AddHandler lbl.MouseDown, Sub(sender As Object, location As Point)
                                          MinRV = CDec(CType(sender, Label).Tag) + Chart.GetMinTick
                                          ReapplyMe()
                                      End Sub
            If direction = Direction.Up Then lbl.VerticalAlignment = LabelVerticalAlignment.Bottom Else lbl.VerticalAlignment = LabelVerticalAlignment.Top
            Return lbl
        End Function
        Function FormatNumberLengthAndPrefix(num As Decimal) As String
            Return Round(num * (10 ^ Chart.Settings("DecimalPlaces").Value))
        End Function
        Function FormatNum(num As Decimal) As String
            Return FormatNumber(num, Chart.Settings("DecimalPlaces").Value)
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
                        'RefreshObject(Chart.bars(i))
                    End If
                Next
                'BarColorRoutine(0, Chart.bars.Count - 1, Chart.Settings("Bar Color").Value)
            End If
            'Chart.dontDrawBarVisuals = False
        End Sub
        Protected Sub BarColorRoutine(ByVal startBar As Integer, ByVal endBar As Integer, ByVal color As Color)
            'If BarColoringOff = False Then
            For i = Max(startBar - 1, 0) To Max(endBar - 1, 0)
                If Not TypeOf Chart.bars(i).Pen.Brush Is SolidColorBrush Then
                    Chart.bars(i).Pen.Brush = New SolidColorBrush
                End If
                If CType(Chart.bars(i).Pen.Brush, SolidColorBrush).Color <> color Then
                    Chart.bars(i).Pen.Brush = New SolidColorBrush(color)
                    RefreshObject(Chart.bars(i))
                End If
            Next
            'End If
        End Sub

#Region "AutoTrendPad"
        Dim toggleColoringButton As Button

        Dim toggleButton As Button

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
            Grid.SetRow(addMinRVMinMove, 2)
            Grid.SetRow(subtractMinRVMinMove, 3)
            Grid.SetRow(minRVBaseValue, 4)
            Grid.SetRow(currentMinRVPopupBtn, 5)

            Grid.SetRow(divider, 6)

            Grid.SetRow(addMaxRVMinMove, 7)
            Grid.SetRow(subtractMaxRVMinMove, 8)
            Grid.SetRow(maxRVBaseValue, 9)
            Grid.SetRow(currentMaxRVPopupBtn, 10)

            Grid.SetRow(grabArea, 11)
            Grid.SetRow(currentRBPopupBtn, 12)
            Grid.SetRow(addRBMinMove, 13)
            Grid.SetRow(subtractRBMinMove, 14)

            grd.Children.Add(toggleColoringButton)
            grd.Children.Add(toggleButton)
            grd.Children.Add(addMinRVMinMove)
            grd.Children.Add(subtractMinRVMinMove)
            grd.Children.Add(minRVBaseValue)
            grd.Children.Add(currentMinRVPopupBtn)

            'grd.Children.Add(divider)

            'grd.Children.Add(addMaxRVMinMove)
            'grd.Children.Add(subtractMaxRVMinMove)
            'grd.Children.Add(maxRVBaseValue)
            'grd.Children.Add(currentMaxRVPopupBtn)

            grd.Children.Add(currentRBPopupBtn)
            grd.Children.Add(addRBMinMove)
            grd.Children.Add(subtractRBMinMove)
        End Sub
        Sub InitControls()
            Dim fontsize As Integer = 16
            Dim col As Brush
            If AlternateTrendColoring Then
                col = GetSplitBrush(Color.FromArgb(255, 30, 255, 0), Colors.Red, 0.5, False)
            Else
                col = GetSplitBrush(Color.FromArgb(255, 166, 166, 166), Color.FromArgb(255, 140, 140, 140), 0.5, False)
            End If
            toggleColoringButton = New Button With {.Background = col, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = ""}

            toggleButton = New Button With {.Background = Brushes.Yellow, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "/ /"}
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
                          If Round(MinRV, 4) = value Then
                              btn.Background = Brushes.LightBlue
                          End If
                          If value = Round(RoundTo(Chart.Settings("RangeValue").Value * BasePriceRVMultiplier, Chart.GetMinTick), 4) Then
                              btn.Background = New SolidColorBrush(Colors.Orange)
                          End If
                          minRVPopupGrid.Children.Add(btn)
                          AddHandler btn.Click,
                                      Sub(sender As Object, e As EventArgs)
                                          minRVPopup.IsOpen = False
                                          If Round(CDec(CType(sender, Button).Tag), 4) <> Round(MinRV, 4) Then
                                              MinRV = CType(sender, Button).Tag
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
                            ElseIf value = RoundTo(MinRV / 2, Chart.GetMinTick) Or value = RoundTo(MinRV / 3, Chart.GetMinTick) Or value = RoundTo(MinRV / 4, Chart.GetMinTick) Then
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
            AddHandler addMinRVMinMove.Click,
                Sub()
                    MinRV += Chart.GetMinTick()
                    ReapplyMe()
                End Sub
            AddHandler subtractMinRVMinMove.Click,
                Sub()
                    MinRV -= Chart.GetMinTick()
                    ReapplyMe()
                End Sub
            AddHandler minRVBaseValue.Click,
                Sub(sender As Object, e As EventArgs)
                    If Round(sender.CommandParameter, 5) <> Round(MinRV, 5) Then
                        MinRV = sender.CommandParameter
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

            AddHandler toggleButton.Click,
                Sub(sender As Object, e As EventArgs)
                    LastSwingIsChannel = Not LastSwingIsChannel
                    ReapplyMe()
                End Sub
            AddHandler toggleColoringButton.Click,
                Sub()
                    AlternateTrendColoring = Not AlternateTrendColoring
                    Dim col As Brush
                    If AlternateTrendColoring Then
                        col = GetSplitBrush(Color.FromArgb(255, 30, 255, 0), Colors.Red, 0.5, False)
                    Else
                        col = GetSplitBrush(Color.FromArgb(255, 166, 166, 166), Color.FromArgb(255, 140, 140, 140), 0.5, False)
                    End If
                    toggleColoringButton.Background = col
                    ReapplyMe()
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
                addMinRVMinMove.Background = Brushes.LightGray
                subtractMinRVMinMove.Background = Brushes.LightGray

                minRVBaseValue.Content = FormatNumberLengthAndPrefix(RoundTo(Chart.Settings("RangeValue").Value * BasePriceRVMultiplier, Chart.GetMinTick))
                minRVBaseValue.CommandParameter = Round(RoundTo(Chart.Settings("RangeValue").Value * BasePriceRVMultiplier, Chart.GetMinTick), Chart.Settings("DecimalPlaces").Value)
                minRVBaseValue.Background = Brushes.White

                currentMinRVPopupBtn.Content = FormatNumberLengthAndPrefix(MinRV)
                currentMinRVPopupBtn.Background = Brushes.LightBlue

                addMaxRVMinMove.Background = Brushes.LightGray
                subtractMaxRVMinMove.Background = Brushes.LightGray

                maxRVBaseValue.Content = FormatNumberLengthAndPrefix(RoundTo(Chart.Settings("RangeValue").Value * BasePriceRVMultiplier, Chart.GetMinTick))
                maxRVBaseValue.CommandParameter = Round(RoundTo(Chart.Settings("RangeValue").Value * BasePriceRVMultiplier, Chart.GetMinTick), Chart.Settings("DecimalPlaces").Value)
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
