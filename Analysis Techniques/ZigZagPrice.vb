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
    Public Class ZigZagPrice
        Inherits AnalysisTechnique
        Implements IChartPadAnalysisTechnique
        Public Sub New(ByVal chart As Chart)
            MyBase.New(chart)
            Description = "Zig Zag."
        End Sub

        Dim swings As List(Of Swing)
        Dim swingDir As Boolean
        Dim targetTextLine As Label
        Dim targetText As Label
        Dim extendStartTargetText As Label
        Dim extendEndTargetText As Label
        'Dim secondLastPotSwing As TrendLine
        Dim lastPotSwing As TrendLine
        Dim regressionLine As TrendLine
        Dim potentialRegressionLine As TrendLine
        Dim lengthText As Label

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

        <Input> Public Property RV As Double = 2
        <Input> Public Property BasePriceRVMultiplier As Double = 2
        <Input> Property TextColor As Color = Colors.Gray
        <Input> Property SwingUpColor As Color = Colors.Gray
        <Input> Property SwingDownColor As Color = Colors.Gray
        <Input> Property UpPotentialColor As Color = Colors.Gray
        <Input> Property DownPotentialColor As Color = Colors.Gray
        <Input> Property UpColor As Color = Colors.Green
        <Input> Property NeutralColor As Color = Colors.Gray
        <Input> Property DownColor As Color = Colors.Red
        <Input> Property UpFillColor As Color = Colors.Red
        <Input> Property DownFillColor As Color = Colors.Red
        <Input> Property LineThickness As Decimal = 1
        <Input> Property LastLineThickness As Decimal = 1
        <Input> Property RegressionLineThickness As Decimal = 1
        <Input> Property ChannelLineThickness As Decimal = 1
        '<Input> Property CenterLineThickness As Decimal = 1
        <Input> Property LengthTextFontSize As Decimal = 11
        <Input> Property LengthTextFontWeight As FontWeight = FontWeights.Bold

        <Input> Property BCTextFontSize As Decimal = 11
        <Input> Property BCTextFontWeight As FontWeight = FontWeights.Bold
        <Input> Property BCTextSpacing As Decimal = 10
        <Input> Property SwingTargetTextFontSize As Decimal = 11
        <Input> Property SwingTargetTextFontWeight As FontWeight = FontWeights.Bold
        <Input> Property TargetTextFontSize As Decimal = 11
        <Input> Property TargetTextFontWeight As FontWeight = FontWeights.Bold
        <Input> Property SwingTargetTextLineLength As Integer = 12
        <Input> Property TargetTextLineLength As Integer = 12
        <Input> Property TextSpacing As Decimal = 5
        <Input> Property BarColoringOff As Boolean = False
        <Input> Property LastSwingIsChannel As Boolean = True
        <Input> Property SwingHistoryOn As Boolean = True
        <Input> Property ExtendLastSwing As Boolean = True


        Protected Overrides Sub Begin()
            MyBase.Begin()
            'secondLastPotSwing = NewTrendLine(Colors.Gray)
            lastPotSwing = NewTrendLine(Colors.Gray)
            If ExtendLastSwing Then
                'secondLastPotSwing.ExtendRight = True
                lastPotSwing.ExtendRight = True
            End If

            swingDir = False
            swings = New List(Of Swing)

            swings.Add(New Swing(NewTrendLine(Colors.Gray, New Point(CurrentBar.Number, CurrentBar.Close), New Point(CurrentBar.Number, CurrentBar.Close), True), False))
            targetText = NewLabel("── " & FormatNumberLengthAndPrefix(RV) & " RV ", TextColor, New Point(0, 0), True, , False)
            targetText.Font.FontSize = SwingTargetTextFontSize
            targetText.Font.FontWeight = SwingTargetTextFontWeight
            'extendStartTargetText = NewLabel(Strings.StrDup(TargetTextLineLength, "─") & " Extend", TextColor, New Point(0, 0), True, , False)
            'extendStartTargetText.Font.FontSize = TargetTextFontSize
            'extendStartTargetText.Font.FontWeight = TargetTextFontWeight
            extendEndTargetText = NewLabel(Strings.StrDup(TargetTextLineLength, "─") & " Extend", TextColor, New Point(0, 0), True, , False)
            extendEndTargetText.Font.FontSize = TargetTextFontSize
            extendEndTargetText.Font.FontWeight = TargetTextFontWeight
            lastChannel = NewTrendLine(Colors.Gray)
            lastChannel.IsSelectable = False
            lastChannel.IsEditable = False
            lastChannel.Pen.Thickness = 0
            lastChannel.OuterPen.Brush = New SolidColorBrush(TextColor)
            lastChannel.IsRegressionLine = True
            lastChannel.OuterPen.Thickness = 0
            lastChannel.HasParallel = True
            lastChannel.ExtendRight = True

            'potentialRegressionLine = NewTrendLine(Colors.Gray, New Point(CurrentBar.Number, CurrentBar.Close), New Point(CurrentBar.Number, CurrentBar.Close))
            'potentialRegressionLine.IsRegressionLine = True
            'potentialRegressionLine.ExtendRight = True
            regressionLine = CreateFillTrendLine(TextColor, Colors.Gray)

            regressionLine.Pen.Thickness = RegressionLineThickness
            'potentialRegressionLine.Pen.Thickness = PotentialRegressionLineThickness

            lengthText = NewLabel("", TextColor, New Point(0, 0), True, New Font With {.FontSize = BCTextFontSize, .FontWeight = BCTextFontWeight}, False) : lengthText.HorizontalAlignment = LabelHorizontalAlignment.Right : lengthText.IsEditable = True : lengthText.IsSelectable = True
            AddHandler lengthText.MouseDown, Sub(sender As Object, location As Point)
                                                 RV = Round(CDec(CType(sender, Label).Tag), 5)
                                                 Chart.ReApplyAnalysisTechnique(Me)
                                             End Sub
        End Sub
        Private Function CreateFillTrendLine(color As Color, fill As Color) As TrendLine
            Dim t As TrendLine = NewTrendLine(color, New Point(CurrentBar.Number, CurrentBar.Close), New Point(CurrentBar.Number, CurrentBar.Close))
            t.IsRegressionLine = True
            t.DrawZoneFill = True
            t.UpZoneFillBrush = New SolidColorBrush(fill)
            t.UpNeutralZoneFillBrush = New SolidColorBrush(fill)
            t.DownNeutralZoneFillBrush = New SolidColorBrush(fill)
            t.DownZoneFillBrush = New SolidColorBrush(fill)
            t.ConfirmedZoneBarStart = 0
            t.NeutralZoneBarStart = Integer.MaxValue
            t.ExtendRight = False
            Return t
        End Function
        Private lastChannel As TrendLine
        Private Property CurrentSwing As Swing
            Get
                Return swings(swings.Count - 1)
            End Get
            Set(ByVal value As Swing)
                swings(swings.Count - 1) = value
            End Set
        End Property
        Protected Overrides Sub Main()
            Dim evnt As Boolean = False
            If Round(CurrentBar.High - CurrentSwing.EndPrice, 5) >= Round(RV, 5) AndAlso CurrentBar.Number <> CurrentSwing.EndBar AndAlso swingDir = False Then
                'new swing up
                If SwingHistoryOn = False Then
                    RemoveObjectFromChart(CurrentSwing.TL)
                End If
                swingDir = True
                swings.Add(New Swing(NewTrendLine(SwingUpColor, New Point(CurrentSwing.EndBar, CurrentSwing.EndPrice), New Point(CurrentBar.Number, CurrentBar.High), True), True))
                CurrentSwing.LengthText = CreateText(AddToY(CurrentSwing.EndPoint, Chart.GetRelativeFromRealHeight(TextSpacing)), Direction.Up, Abs(CurrentSwing.EndPrice - CurrentSwing.StartPrice), SwingUpColor)
                CurrentSwing.TL.Pen.Thickness = LineThickness
                CurrentSwing.TL.IsEditable = False : CurrentSwing.TL.IsSelectable = False
                If ExtendLastSwing Then
                    CurrentSwing.TL.ExtendRight = True
                End If
                CurrentSwing.TL.Pen.Thickness = LastLineThickness
                If BarColoringOff = False Then
                    BarColorRoutine(swings(swings.Count - 2).StartBar, swings(swings.Count - 2).EndBar, NeutralColor)
                    BarColorRoutine(CurrentSwing.StartBar, CurrentSwing.EndBar, UpColor)
                End If

                If LastSwingIsChannel Then
                    'CurrentSwing.TL.Pen.Thickness = 0
                    lastChannel.OuterPen.Thickness = ChannelLineThickness
                    lastChannel.Pen.Thickness = 0
                    lastChannel.OuterPen.Brush = New SolidColorBrush(SwingUpColor)
                    lastChannel.Pen.Brush = Brushes.Transparent
                    lastChannel.Coordinates = CurrentSwing.TL.Coordinates
                    If swings.Count > 1 Then
                        swings(swings.Count - 2).TL.Pen.Thickness = LineThickness
                    End If
                End If
                If swings.Count > 2 Then
                    If ExtendLastSwing Then swings(swings.Count - 2).TL.ExtendRight = False
                    swings(swings.Count - 2).TL.Pen.Thickness = LineThickness
                End If
                evnt = True

            ElseIf Round(CurrentSwing.EndPrice - CurrentBar.Low, 5) >= Round(RV, 5) AndAlso CurrentBar.Number <> CurrentSwing.EndBar AndAlso swingDir = True Then
                ' new swing down
                If SwingHistoryOn = False Then
                    RemoveObjectFromChart(CurrentSwing.TL)
                End If
                swingDir = False
                swings.Add(New Swing(NewTrendLine(SwingDownColor, New Point(CurrentSwing.EndBar, CurrentSwing.EndPrice), New Point(CurrentBar.Number, CurrentBar.Low), True), True))
                CurrentSwing.LengthText = CreateText(AddToY(CurrentSwing.EndPoint, -Chart.GetRelativeFromRealHeight(TextSpacing)), Direction.Down, Abs(CurrentSwing.EndPrice - CurrentSwing.StartPrice), SwingDownColor)
                lastChannel.OuterPen.Brush = New SolidColorBrush(SwingDownColor)
                lastChannel.Pen.Brush = Brushes.Transparent
                CurrentSwing.TL.IsEditable = False : CurrentSwing.TL.IsSelectable = False
                If ExtendLastSwing Then
                    CurrentSwing.TL.ExtendRight = True
                End If
                CurrentSwing.TL.Pen.Thickness = LastLineThickness
                If BarColoringOff = False Then
                    BarColorRoutine(swings(swings.Count - 2).StartBar, swings(swings.Count - 2).EndBar, NeutralColor)
                    BarColorRoutine(CurrentSwing.StartBar, CurrentSwing.EndBar, DownColor)
                End If

                If LastSwingIsChannel Then
                    'CurrentSwing.TL.Pen.Thickness = 0
                    lastChannel.OuterPen.Thickness = ChannelLineThickness
                    lastChannel.Pen.Thickness = 0
                    lastChannel.Coordinates = CurrentSwing.TL.Coordinates
                    If swings.Count > 1 Then
                        swings(swings.Count - 2).TL.Pen.Thickness = LineThickness
                    End If
                End If
                If swings.Count > 2 Then
                    If ExtendLastSwing Then swings(swings.Count - 2).TL.ExtendRight = False
                    swings(swings.Count - 2).TL.Pen.Thickness = LineThickness
                End If
                evnt = True
            ElseIf CurrentBar.High >= CurrentSwing.EndPrice And swingDir = True Then
                ' extension up
                CurrentSwing.EndBar = CurrentBar.Number
                CurrentSwing.EndPrice = CurrentBar.High
                UpdateText(CurrentSwing.LengthText, AddToY(CurrentSwing.EndPoint, Chart.GetRelativeFromRealHeight(TextSpacing)), Abs(CurrentSwing.EndPrice - CurrentSwing.StartPrice), SwingUpColor)

                BarColorRoutine(CurrentSwing.StartBar, CurrentSwing.EndBar, UpColor)
                evnt = True
            ElseIf CurrentBar.Low <= CurrentSwing.EndPrice And swingDir = False Then
                ' extension down
                CurrentSwing.EndBar = CurrentBar.Number
                CurrentSwing.EndPrice = CurrentBar.Low
                UpdateText(CurrentSwing.LengthText, AddToY(CurrentSwing.EndPoint, -Chart.GetRelativeFromRealHeight(TextSpacing)), Abs(CurrentSwing.EndPrice - CurrentSwing.StartPrice), SwingDownColor)
                'lastChannel.Coordinates = CurrentSwing.TL.Coordinates
                BarColorRoutine(CurrentSwing.StartBar, CurrentSwing.EndBar, DownColor)
                evnt = True
            End If
            lastChannel.Coordinates = New LineCoordinates(CurrentSwing.StartPoint, New Point(CurrentBar.Number, CurrentBar.Avg))
            If evnt Then
                regressionLine.Pen.Brush = New SolidColorBrush(If(swingDir, SwingUpColor, SwingDownColor))
                regressionLine.UpZoneFillBrush = New SolidColorBrush(If(swingDir, UpFillColor, DownFillColor))
                regressionLine.DownZoneFillBrush = New SolidColorBrush(If(swingDir, UpFillColor, DownFillColor))
                Dim aPoint As Point = New Point(0, 0), bPoint As Point = New Point(0, If(swingDir = False, Decimal.MaxValue, Decimal.MinValue)), cPoint As Point = New Point(0, If(swingDir = False, Decimal.MinValue, Decimal.MaxValue))
                Dim bcLength As Decimal
                Dim biggestC As Point
                For i = CurrentSwing.EndBar - 1 To CurrentSwing.StartBar - 1 Step -1
                    If (swingDir = Direction.Down And Chart.bars(i).Data.High > Round(cPoint.Y, 5)) Or
                       (swingDir = Direction.Up And Chart.bars(i).Data.Low < Round(cPoint.Y, 5)) Then
                        cPoint = New Point(i + 1, If(swingDir, Chart.bars(i).Data.Low, Chart.bars(i).Data.High))
                        For j = i - 1 To CurrentSwing.StartBar - 1 Step -1
                            If (swingDir = Direction.Down And Chart.bars(j).Data.High > Round(cPoint.Y, 5)) Or
                               (swingDir = Direction.Up And Chart.bars(j).Data.Low < Round(cPoint.Y, 5)) Then
                                Exit For
                            Else
                                If (swingDir = Direction.Down And Round(Abs(Chart.bars(j).Data.Low - cPoint.Y), 5) > Round(bcLength, 5)) Or
                                    (swingDir = Direction.Up And Round(Abs(Chart.bars(j).Data.High - cPoint.Y), 5) > Round(bcLength, 5)) Then
                                    biggestC = cPoint
                                    bcLength = If(swingDir, Abs(Chart.bars(j).Data.High - cPoint.Y), Abs(Chart.bars(j).Data.Low - cPoint.Y))
                                End If
                            End If
                        Next
                    End If
                Next

                lengthText.Location = AddToY(biggestC, If(swingDir, -1, 1) * Chart.GetRelativeFromRealHeight(BCTextSpacing))
                lengthText.Text = FormatNumberLengthAndPrefix(bcLength)
                lengthText.Tag = bcLength
                lengthText.VerticalAlignment = If(swingDir, VerticalAlignment.Top, VerticalAlignment.Bottom)
            End If
            If Abs(CurrentSwing.EndPrice - If(swingDir, CurrentBar.Low, CurrentBar.High)) >= lengthText.Tag Then
                lengthText.Text = ""
            End If
            regressionLine.Coordinates = New LineCoordinates(CurrentSwing.StartPoint, New Point(CurrentBar.Number, CurrentBar.Close))
            'potentialRegressionLine.Coordinates = New LineCoordinates(CurrentSwing.StartPoint, New Point(CurrentBar.Number, CurrentBar.Close))
            'potentialRegressionLine.Pen.Brush = New SolidColorBrush(If(swingDir, UpPotentialColor, DownPotentialColor))
            If swings.Count > 1 Then
                ' secondLastPotSwing.Coordinates = New LineCoordinates(swings(swings.Count - 2).StartPoint, New Point(CurrentBar.Number, Chart.bars(Chart.bars.Count - 1).Data.Close))
                'lastPotSwing.Coordinates = New LineCoordinates(swings(swings.Count - 1).StartPoint, New Point(CurrentBar.Number, Chart.bars(Chart.bars.Count - 1).Data.Close))
                'secondLastPotSwing.Pen.Brush = New SolidColorBrush(If(swingDir, DownPotentialColor, UpPotentialColor))
                'lastPotSwing.UpZoneFillBrush = New SolidColorBrush(FillColor)
                'lastPotSwing.DownZoneFillBrush = New SolidColorBrush(FillColor)
            End If
            extendEndTargetText.Location = New Point(CurrentBar.Number, CurrentSwing.EndPrice)
            extendEndTargetText.Font.Brush = New SolidColorBrush(If(swingDir, SwingUpColor, SwingDownColor))
            'extendStartTargetText.Location = New Point(CurrentBar.Number, CurrentSwing.StartPrice)
            'extendStartTargetText.Font.Brush = New SolidColorBrush(If(swingDir, SwingDownColor, SwingUpColor))
            targetText.Location = New Point(CurrentBar.Number, If(swingDir, CurrentSwing.EndPrice - RV, CurrentSwing.EndPrice + RV))
            targetText.Text = Strings.StrDup(SwingTargetTextLineLength, "─") & " " & FormatNumberLengthAndPrefix(RV) & " RV  " ' & Round(targetText.Location.Y, Chart.Settings("DecimalPlaces").Value)
            targetText.Font.Brush = New SolidColorBrush(If(swingDir, SwingDownColor, SwingUpColor))
        End Sub
        Sub UpdateText(label As Label, position As Point, value As String, color As Color)
            If label IsNot Nothing Then
                label.Text = FormatNumberLengthAndPrefix(value)
                label.Tag = value
                label.Location = AddToX(position, -2)
                label.Font.Brush = New SolidColorBrush(color)
            End If
        End Sub
        Function CreateText(position As Point, direction As Direction, value As String, color As Color) As Label
            Dim lbl = NewLabel(FormatNumberLengthAndPrefix(value), color, New Point(0, 0),, New Font With {.FontSize = LengthTextFontSize, .FontWeight = LengthTextFontWeight}, False) : lbl.HorizontalAlignment = LabelHorizontalAlignment.Right : lbl.IsEditable = True : lbl.IsSelectable = True
            lbl.Tag = value
            lbl.Location = AddToX(position, -2)
            AddHandler lbl.MouseDown, Sub(sender As Object, location As Point)
                                          RV = CDec(CType(sender, Label).Tag) + Chart.GetMinTick
                                          Chart.ReApplyAnalysisTechnique(Me)
                                      End Sub
            If direction = Direction.Up Then lbl.VerticalAlignment = LabelVerticalAlignment.Bottom Else lbl.VerticalAlignment = LabelVerticalAlignment.Top
            Return lbl
        End Function
        Public Overrides Property Name As String = "Zig Zag Price"
        Function FormatNumberLengthAndPrefix(num As Decimal) As String
            Return Round(num * (10 ^ Chart.Settings("DecimalPlaces").Value))
        End Function
        Public Overrides Sub Reset()

            MyBase.Reset()
            If Chart.bars.Count > 1 And BarColoringOff = False Then
                For i = Max(0, 0) To Max(Chart.bars.Count - 1, 0)
                    If Not TypeOf Chart.bars(i).Pen.Brush Is SolidColorBrush Then
                        Chart.bars(i).Pen.Brush = New SolidColorBrush
                    End If
                    If CType(Chart.bars(i).Pen.Brush, SolidColorBrush).Color <> Chart.Settings("Bar Color").Value Then
                        Chart.bars(i).Pen.Brush = New SolidColorBrush(Chart.Settings("Bar Color").Value)
                        RefreshObject(Chart.bars(i))
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
        Dim toggleButton As Button
        Dim addRVMinMove As Button
        Dim subtractRVMinMove As Button
        Dim rvBaseValue As Button
        Dim currentRVPopupBtn As Button


        Dim grabArea As Border
        Dim currentRBPopupBtn As Button
        Dim addRBMinMove As Button
        Dim subtractRBMinMove As Button

        Dim rvPopup As Popup
        Dim rvPopupGrid As Grid
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
            Grid.SetRow(toggleButton, 0)
            Grid.SetRow(addRVMinMove, 1)
            Grid.SetRow(subtractRVMinMove, 2)
            Grid.SetRow(rvBaseValue, 3)
            Grid.SetRow(currentRVPopupBtn, 4)

            Grid.SetRow(grabArea, 9)
            Grid.SetRow(currentRBPopupBtn, 12)
            Grid.SetRow(addRBMinMove, 13)
            Grid.SetRow(subtractRBMinMove, 14)

            grd.Children.Add(toggleButton)
            grd.Children.Add(addRVMinMove)
            grd.Children.Add(subtractRVMinMove)
            grd.Children.Add(rvBaseValue)
            grd.Children.Add(currentRVPopupBtn)

            grd.Children.Add(currentRBPopupBtn)
            grd.Children.Add(addRBMinMove)
            grd.Children.Add(subtractRBMinMove)
        End Sub
        Sub InitControls()
            Dim fontsize As Integer = 16
            toggleButton = New Button With {.Background = Brushes.Yellow, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "/ /"}
            addRVMinMove = New Button With {.Background = Brushes.LightGray, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "+1"}
            subtractRVMinMove = New Button With {.Background = Brushes.LightGray, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "-1"}
            rvBaseValue = New Button With {.Background = New SolidColorBrush(Color.FromArgb(255, 230, 235, 255)), .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "-1"}
            currentRVPopupBtn = New Button With {.Background = Brushes.Cyan, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19}

            rvPopup = New Popup With {.Placement = PlacementMode.Left, .PlacementTarget = currentRVPopupBtn, .Width = 520, .Height = 310, .StaysOpen = False}
            rvPopupGrid = New Grid With {.Background = Brushes.White}
            rvPopup.Child = rvPopupGrid
            For x = 1 To 10 : rvPopupGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)}) : Next
            For y = 1 To 12 : rvPopupGrid.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Star)}) : Next

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
                          Dim btn As New Button With {.Margin = New Thickness(1), .Tag = value, .Content = FormatNumberLengthAndPrefix(value), .FontSize = 14.5, .Foreground = Brushes.Black}
                          Grid.SetRow(btn, y - 1)
                          Grid.SetColumn(btn, x - 1)
                          btn.Background = Brushes.White
                          If Round(RV, 4) = value Then
                              btn.Background = Brushes.LightBlue
                          End If
                          If value = Round(RoundTo(Chart.Settings("RangeValue").Value * BasePriceRVMultiplier, Chart.GetMinTick), 4) Then
                              btn.Background = New SolidColorBrush(Colors.Orange)
                          End If
                          rvPopupGrid.Children.Add(btn)
                          AddHandler btn.Click,
                                      Sub(sender As Object, e As EventArgs)
                                          rvPopup.IsOpen = False
                                          If Round(CDec(CType(sender, Button).Tag), 4) <> Round(RV, 4) Then
                                              RV = CType(sender, Button).Tag
                                              Chart.ReApplyAnalysisTechnique(Me)
                                          End If
                                      End Sub
                      Next
                  Next
              End Sub


            AddHandler addRVMinMove.Click,
                Sub()
                    RV += Chart.GetMinTick()
                    Chart.ReApplyAnalysisTechnique(Me)
                End Sub
            AddHandler subtractRVMinMove.Click,
                Sub()
                    RV -= Chart.GetMinTick()
                    Chart.ReApplyAnalysisTechnique(Me)
                End Sub
            AddHandler rvBaseValue.Click,
                Sub(sender As Object, e As EventArgs)
                    If Round(sender.CommandParameter, 5) <> Round(RV, 5) Then
                        RV = sender.CommandParameter
                        Chart.ReApplyAnalysisTechnique(Me)
                    End If
                End Sub
            AddHandler toggleButton.Click,
                Sub(sender As Object, e As EventArgs)
                    LastSwingIsChannel = Not LastSwingIsChannel
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
                            ElseIf value = RoundTo(rv / 2, Chart.GetMinTick) Or value = RoundTo(rv / 3, Chart.GetMinTick) Or value = RoundTo(RV / 4, Chart.GetMinTick) Then
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

                rvBaseValue.Content = FormatNumberLengthAndPrefix(RoundTo(Chart.Settings("RangeValue").Value * BasePriceRVMultiplier, Chart.GetMinTick))
                rvBaseValue.CommandParameter = Round(RoundTo(Chart.Settings("RangeValue").Value * BasePriceRVMultiplier, Chart.GetMinTick), Chart.Settings("DecimalPlaces").Value)
                rvBaseValue.Background = Brushes.White

                currentRVPopupBtn.Content = FormatNumberLengthAndPrefix(RV)
                currentRVPopupBtn.Background = Brushes.LightBlue

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

