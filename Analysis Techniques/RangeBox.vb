Imports System.Collections.ObjectModel
Imports System.Windows.Controls.Primitives

Namespace AnalysisTechniques
    Public Class RangeBox
#Region "AnalysisTechnique Inherited Code"
        Inherits AnalysisTechnique
        Implements IChartPadAnalysisTechnique

        ' Inherit the one-argument constructor from the base class.
        Public Sub New(ByVal chart As Chart)
            MyBase.New(chart) ' Call the base class constructor.
            Description = "RangeBox"
        End Sub
#End Region

        <Input> Public Property Range As Decimal = 0.19
        <Input> Public Property RangeMultiplier As Decimal = 0.19
        <Input> Public Property StartBarOffset As Integer = 2
        <Input> Public Property NumberOfBoxesToUseAsAverage As Integer = 5
        <Input> Public Property ConfirmedBoxColor As Color = Colors.Green
        <Input> Public Property PotentialBoxColor As Color = Colors.Green
        <Input> Public Property ProjectionLinesColor As Color = Colors.Blue
        <Input> Public Property BoxISColor As Color = Color.FromArgb(20, 0, 0, 255)
        <Input> Public Property BoxNHColor As Color = Color.FromArgb(20, 0, 255, 0)
        <Input> Public Property BoxNLColor As Color = Color.FromArgb(20, 255, 0, 0)
        <Input> Public Property BoxLineThickness As Decimal = 0.2
        <Input> Public Property ProjectionLineThickness As Decimal = 0.2
        <Input> Public Property TextFontSize As Decimal = 12
        <Input> Public Property TextFontWeight As FontWeight = FontWeights.Bold
        <Input> Public Property TextColor As Color = Colors.Gray
        Public Overrides Property Name As String = "RangeBox"
        Private Class Box
            Public Property Rect As Rectangle
            Public Property Data As New Dictionary(Of Integer, Decimal)
            Public Property Direction As Boolean
        End Class

        Private ReadOnly Property CurrentBox As Box
            Get
                Return boxes(boxes.Count - 1)
            End Get
        End Property
        Private boxes As List(Of Box)
        Private upperPotRangeLine As TrendLine
        Private lowerPotRangeLine As TrendLine
        Private upperRangeLine As TrendLine
        Private lowerRangeLine As TrendLine
        Private trackBox As Box
        Private potentialBox As Box
        Private lengthText As Label


        Protected Overrides Sub Begin()
            MyBase.Begin()
            boxes = New List(Of Box)
            boxes.Add(New Box With {.Rect = NewRectangle(ConfirmedBoxColor, BoxISColor, New Point(1, CurrentBar.High), New Point(1, CurrentBar.Low))})
            CurrentBox.Rect.Pen.Thickness = BoxLineThickness
            trackBox = New Box With {.Rect = NewRectangle(Colors.Transparent, Color.FromArgb(0, 0, 0, 0), New Point(1, CurrentBar.High), New Point(1, CurrentBar.Low))}
            trackBox.Rect.IsEditable = False
            trackBox.Rect.IsSelectable = False
            potentialBox = New Box With {.Rect = NewRectangle(PotentialBoxColor, Color.FromArgb(1, 0, 0, 0), New Point(1, CurrentBar.High), New Point(1, CurrentBar.Low))}
            potentialBox.Rect.LockPositionOrientation = ChartDrawingVisual.LockPositionOrientationTypes.Vertically
            potentialBox.Rect.CanResize = False
            potentialBox.Rect.ShowSelectionHandles = False
            potentialBox.Rect.Pen.Thickness = BoxLineThickness
            CurrentBox.Rect.IsEditable = False
            CurrentBox.Rect.IsSelectable = False
            upperPotRangeLine = NewTrendLine(ProjectionLinesColor) : upperPotRangeLine.Pen.Thickness = ProjectionLineThickness
            lowerPotRangeLine = NewTrendLine(ProjectionLinesColor) : lowerPotRangeLine.Pen.Thickness = ProjectionLineThickness
            upperPotRangeLine.ExtendRight = True
            lowerPotRangeLine.ExtendRight = True
            upperRangeLine = NewTrendLine(ConfirmedBoxColor) : upperRangeLine.Pen.Thickness = BoxLineThickness
            lowerRangeLine = NewTrendLine(ConfirmedBoxColor) : lowerRangeLine.Pen.Thickness = BoxLineThickness
            lengthText = NewLabel(FormatNumberLengthAndPrefix(Range), TextColor, New Point(0, 0)) : lengthText.Font.FontSize = TextFontSize : lengthText.Font.FontWeight = TextFontWeight
            lengthText.HorizontalAlignment = LabelHorizontalAlignment.Right : lengthText.VerticalAlignment = LabelVerticalAlignment.Center : lengthText.IsSelectable = False : lengthText.IsEditable = False : lengthText.Fill = Brushes.White
            Dim rect_move =
                Sub(sender As Object, index As Integer, location As Point)
                    If potentialBox.Rect.Y > CurrentBox.Rect.Y Then
                        potentialBox.Rect.Fill = New SolidColorBrush(BoxNHColor)
                    ElseIf potentialBox.Rect.Y < CurrentBox.Rect.Y Then
                        potentialBox.Rect.Fill = New SolidColorBrush(BoxNLColor)
                    Else
                        potentialBox.Rect.Fill = New SolidColorBrush(BoxISColor)
                    End If
                    'lengthText.Location = New Point(potentialBox.Rect.X + potentialBox.Rect.Width, potentialBox.Rect.Y)
                End Sub
            AddHandler potentialBox.Rect.ManuallyMoved, rect_move

        End Sub
        Protected Overrides Sub NewBar()
            If CurrentBar.Number < StartBarOffset Then Exit Sub
            trackBox.Rect.Width = Max(GetProjectedWidth(), CurrentBar.Number - trackBox.Rect.X)
            potentialBox.Rect.Width = trackBox.Rect.Width
            UpdateRangeLines()
        End Sub
        Protected Overrides Sub Main()
            If CurrentBar.Number < StartBarOffset Then
                trackBox.Rect.X = CurrentBar.Number : trackBox.Rect.Y = CurrentBar.High
                potentialBox.Rect.X = CurrentBar.Number : potentialBox.Rect.Y = CurrentBar.High
                CurrentBox.Rect.X = CurrentBar.Number : CurrentBox.Rect.Y = CurrentBar.High
                Exit Sub
            End If
            If CurrentBar.Low < trackBox.Rect.Y - trackBox.Rect.Height Then
                If CurrentBar.Low <= trackBox.Rect.Y - Range Then
                    CreateNewBox(False)
                Else
                    trackBox.Rect.Height = trackBox.Rect.Y - CurrentBar.Low
                End If
            ElseIf CurrentBar.High > trackBox.Rect.Y Then
                If CurrentBar.High >= trackBox.Rect.Y - trackBox.Rect.Height + Range Then
                    CreateNewBox(True)
                Else
                    trackBox.Rect.Height += CurrentBar.High - trackBox.Rect.Y
                    trackBox.Rect.Y = CurrentBar.High
                End If
            End If
            lengthText.Location = New Point(CurrentBox.Rect.X + CurrentBox.Rect.Width, CurrentBox.Rect.Y - CurrentBox.Rect.Height / 2)
            UpdateRangeLines()
            If Not (potentialBox.Rect.Y >= upperRangeLine.StartPoint.Y And potentialBox.Rect.Y - potentialBox.Rect.Height <= lowerRangeLine.StartPoint.Y) Then
                potentialBox.Rect.Coordinates = trackBox.Rect.Coordinates
                potentialBox.Rect.Height = Range
            End If
            If potentialBox.Rect.Y > CurrentBox.Rect.Y Then
                potentialBox.Rect.Fill = New SolidColorBrush(BoxNHColor)
            ElseIf potentialBox.Rect.Y < CurrentBox.Rect.Y Then
                potentialBox.Rect.Fill = New SolidColorBrush(BoxNLColor)
            Else
                potentialBox.Rect.Fill = New SolidColorBrush(BoxISColor)
            End If
        End Sub
        Private Sub UpdateRangeLines()
            If CurrentBar.Number = CurrentBox.Rect.X + CurrentBox.Rect.Width Then
                upperRangeLine.StartPoint = New Point(upperRangeLine.StartPoint.X, Max(If(CurrentBox.Direction, CurrentBar.High, CurrentBox.Rect.Y - CurrentBox.Rect.Height), upperRangeLine.StartPoint.Y))
                upperRangeLine.EndPoint = New Point(trackBox.Rect.X + trackBox.Rect.Width, Max(upperRangeLine.StartPoint.Y, upperRangeLine.EndPoint.Y))
                lowerRangeLine.StartPoint = New Point(lowerRangeLine.StartPoint.X, Min(If(Not CurrentBox.Direction, CurrentBar.Low, CurrentBox.Rect.Y), lowerRangeLine.StartPoint.Y))
                lowerRangeLine.EndPoint = New Point(trackBox.Rect.X + trackBox.Rect.Width, Min(lowerRangeLine.StartPoint.Y, lowerRangeLine.EndPoint.Y))
            Else
                upperRangeLine.StartPoint = New Point(upperRangeLine.StartPoint.X, Max(CurrentBar.High, upperRangeLine.StartPoint.Y))
                upperRangeLine.EndPoint = New Point(trackBox.Rect.X + trackBox.Rect.Width, Max(upperRangeLine.StartPoint.Y, upperRangeLine.EndPoint.Y))
                lowerRangeLine.StartPoint = New Point(lowerRangeLine.StartPoint.X, Min(CurrentBar.Low, lowerRangeLine.StartPoint.Y))
                lowerRangeLine.EndPoint = New Point(trackBox.Rect.X + trackBox.Rect.Width, Min(lowerRangeLine.StartPoint.Y, lowerRangeLine.EndPoint.Y))
            End If
            upperPotRangeLine.StartPoint = New Point(upperPotRangeLine.StartPoint.X, lowerRangeLine.StartPoint.Y + Range)
            upperPotRangeLine.EndPoint = New Point(trackBox.Rect.X + trackBox.Rect.Width, upperPotRangeLine.StartPoint.Y)
            lowerPotRangeLine.StartPoint = New Point(lowerPotRangeLine.StartPoint.X, upperRangeLine.StartPoint.Y - Range)
            lowerPotRangeLine.EndPoint = New Point(trackBox.Rect.X + trackBox.Rect.Width, lowerPotRangeLine.StartPoint.Y)
            potentialBox.Rect.MaxTopValue = lowerRangeLine.StartPoint.Y
            potentialBox.Rect.MaxBottomValue = upperRangeLine.StartPoint.Y
        End Sub
        Private Sub CreateNewBox(direction As Boolean)
            CurrentBox.Direction = direction
            If direction Then
                boxes.Add(New Box With {.Rect = NewRectangle(ConfirmedBoxColor, BoxISColor, New Point(trackBox.Rect.X, trackBox.Rect.Y - trackBox.Rect.Height + Range), New Point(CurrentBar.Number, trackBox.Rect.Y - trackBox.Rect.Height))})
            Else
                boxes.Add(New Box With {.Rect = NewRectangle(ConfirmedBoxColor, BoxISColor, New Point(trackBox.Rect.X, trackBox.Rect.Y), New Point(CurrentBar.Number, trackBox.Rect.Y - Range))})
            End If
            CurrentBox.Rect.IsEditable = False
            CurrentBox.Rect.IsSelectable = False
            CurrentBox.Rect.Pen.Thickness = BoxLineThickness

            If boxes.Count > 1 Then
                If CurrentBox.Rect.Y > boxes(boxes.Count - 2).Rect.Y Then
                    CurrentBox.Rect.Fill = New SolidColorBrush(BoxNHColor)
                ElseIf CurrentBox.Rect.Y < boxes(boxes.Count - 2).Rect.Y Then
                    CurrentBox.Rect.Fill = New SolidColorBrush(BoxNLColor)
                Else
                    CurrentBox.Rect.Fill = New SolidColorBrush(BoxISColor)
                End If
            End If
            RemoveObjectFromChart(lengthText)
            AddObjectToChart(lengthText)
            trackBox.Rect.Y = CurrentBar.High
            trackBox.Rect.X = CurrentBar.Number
            trackBox.Rect.Width = 0
            trackBox.Rect.Height = CurrentBar.High - CurrentBar.Low
            upperRangeLine.Coordinates = New LineCoordinates(CurrentBar.Number, CurrentBox.Rect.Y - If(Not direction, Range, 0), CurrentBar.Number + 5, CurrentBox.Rect.Y - If(Not direction, Range, 0))
            lowerRangeLine.Coordinates = New LineCoordinates(CurrentBar.Number, CurrentBox.Rect.Y - If(Not direction, Range, 0), CurrentBar.Number + 5, CurrentBox.Rect.Y - If(Not direction, Range, 0))
            upperPotRangeLine.Coordinates = New LineCoordinates(CurrentBar.Number, CurrentBox.Rect.Y, CurrentBar.Number, CurrentBox.Rect.Y)
            lowerPotRangeLine.Coordinates = New LineCoordinates(CurrentBar.Number, CurrentBox.Rect.Y, CurrentBar.Number, CurrentBox.Rect.Y)
        End Sub
        Private Function GetProjectedWidth() As Integer
            If boxes.Count > 1 Then
                Dim avgWidthSum As Integer
                Dim frontBoxPriceRatio As Decimal = (upperRangeLine.StartPoint.Y - lowerRangeLine.StartPoint.Y) / Range
                Dim barIndex As Integer = CurrentBar.Number - trackBox.Rect.X
                'Dim percentagesSum As Decimal
                For i = boxes.Count - 1 To Max(0, boxes.Count - NumberOfBoxesToUseAsAverage) Step -1
                    'Dim minVal As Decimal = Decimal.MaxValue, maxVal As Decimal = 0
                    'For j = boxes(i).Rect.X To boxes(i).Rect.X + boxes(i).Rect.Width + 1
                    '    minVal = Min(minVal, Chart.bars(j - 1).Data.Low)
                    '    maxVal = Max(maxVal, Chart.bars(j - 1).Data.High)
                    '    If (maxVal - minVal) / Range >= frontBoxPriceRatio Then
                    '        If boxes(i).Rect.Width <> 0 Then
                    '            percentagesSum += (j - boxes(i).Rect.X + 1) / (boxes(i).Rect.Width + 1)
                    '        Else
                    '            percentagesSum += 1
                    '        End If
                    '        Exit For
                    '    End If
                    'Next
                    avgWidthSum += boxes(i).Rect.Width
                Next
                Return CInt(avgWidthSum / NumberOfBoxesToUseAsAverage) ' LinCalc(0, avgWidthSum / NumberOfBoxesToUseAsAverage, 1, (CurrentBar.Number - CurrentBox.Rect.X) / (percentagesSum / NumberOfBoxesToUseAsAverage), frontBoxPriceRatio)
            Else
                Return 0
            End If
        End Function
#Region "AutoTrendPad"
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

        Dim addMinMoveBtn As Button
        Dim subtractMinMoveBtn As Button
        Dim baseValueBtn As Button
        Dim currentPopupBtn As Button
        Dim slider As Slider
        Dim grabArea As Border

        Dim popup As Popup
        Dim popupGrid As Grid

        Dim bd As Border
        Dim grd As Grid
        Private Sub InitGrid()
            grd = New Grid
            bd = New Border
            grd.Margin = New Thickness(0)
            grd.HorizontalAlignment = Windows.HorizontalAlignment.Center
            For i = 1 To 1
                grd.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Auto)})
            Next
            For i = 1 To 7
                grd.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Auto)})
            Next

            grd.Background = Chart.Background
            grd.Resources.MergedDictionaries.Add(New ResourceDictionary)
            grd.Resources.MergedDictionaries(0).Source = New Uri("Themes/OrderButtonStyle.xaml", UriKind.Relative) 'AutoTrendPadButtonStyle
            bd.Child = grd
            bd.BorderBrush = Brushes.Transparent
            bd.BorderThickness = New Thickness(0)
            bd.Background = Chart.Background
            bd.CornerRadius = New CornerRadius(0)
            bd.HorizontalAlignment = Windows.HorizontalAlignment.Center

            Dim c As New ContextMenu
            AddHandler c.Opened, Sub() c.IsOpen = False
            bd.ContextMenu = c
            grabArea = New Border With {.BorderThickness = New Thickness(1), .BorderBrush = Brushes.Gray, .Background = New SolidColorBrush(Color.FromArgb(255, 252, 184, 41)), .MinHeight = 20, .Margin = New Thickness(0.5)}

            grd.Children.Add(grabArea)
        End Sub
        Sub SetRows()

            'swing
            Grid.SetRow(addMinMoveBtn, 0)
            Grid.SetRow(subtractMinMoveBtn, 1)
            Grid.SetRow(baseValueBtn, 2)
            Grid.SetRow(currentPopupBtn, 3)
            Grid.SetRow(grabArea, 4)
            Grid.SetRow(slider, 5)

            grd.Children.Add(addMinMoveBtn)
            grd.Children.Add(subtractMinMoveBtn)
            grd.Children.Add(baseValueBtn)
            grd.Children.Add(currentPopupBtn)
            'grd.Children.Add(slider)

        End Sub
        Sub InitControls()
            Dim fontsize = 16
            addMinMoveBtn = New Button With {.Background = Brushes.LightGray, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "+1"}
            subtractMinMoveBtn = New Button With {.Background = Brushes.LightGray, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "-1"}
            baseValueBtn = New Button With {.Background = New SolidColorBrush(Color.FromArgb(255, 230, 235, 255)), .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "-1"}
            currentPopupBtn = New Button With {.Background = Brushes.Cyan, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19}

            slider = New Slider With {.Minimum = 0, .Maximum = 50, .Value = 0, .LargeChange = 1, .SmallChange = 1, .MinHeight = 110, .Orientation = Orientation.Vertical, .HorizontalAlignment = HorizontalAlignment.Center, .Margin = New Thickness(0, 4, 0, 0)}

            popup = New Popup With {.Placement = PlacementMode.Left, .PlacementTarget = currentPopupBtn, .Width = 520, .Height = 310, .StaysOpen = False}
            popupGrid = New Grid With {.Background = Brushes.White}
            popup.Child = popupGrid
            For x = 1 To 10 : popupGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)}) : Next
            For y = 1 To 12 : popupGrid.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Star)}) : Next

        End Sub
        Sub AddHandlers()
            AddHandler slider.ValueChanged,
                Sub()
                    StartBarOffset = slider.Value
                    Chart.ReApplyAnalysisTechnique(Me)
                End Sub
            AddHandler popup.Opened,
              Sub()
                  popupGrid.Children.Clear()
                  For x = 1 To 10
                      For y = 1 To 12
                          Dim value = Round(((y - 1) * 10 + (1 * Chart.Settings("RangeValue").Value) / Chart.GetMinTick() + x - 1) * Chart.GetMinTick(), 4)
                          Dim btn As New Button With {.Margin = New Thickness(1), .Tag = value, .Content = FormatNumberLengthAndPrefix(value), .FontSize = 14.5, .Foreground = Brushes.Black}
                          Grid.SetRow(btn, y - 1)
                          Grid.SetColumn(btn, x - 1)
                          btn.Background = Brushes.White
                          If Round(Range, 4) = value Then
                              btn.Background = Brushes.LightBlue
                          End If
                          If value = Round(RoundTo(Chart.Settings("RangeValue").Value * RangeMultiplier, Chart.GetMinTick), 4) Then
                              btn.Background = New SolidColorBrush(Colors.Orange)
                          End If
                          popupGrid.Children.Add(btn)
                          AddHandler btn.Click,
                                      Sub(sender As Object, e As EventArgs)
                                          popup.IsOpen = False
                                          If Round(CDec(CType(sender, Button).Tag), 4) <> Round(Range, 4) Then
                                              Range = CType(sender, Button).Tag
                                              Chart.ReApplyAnalysisTechnique(Me)
                                          End If
                                      End Sub
                      Next
                  Next
              End Sub
            AddHandler currentPopupBtn.Click,
                Sub()
                    popup.IsOpen = True
                End Sub
            AddHandler addMinMoveBtn.Click,
               Sub()
                   Range += Chart.GetMinTick()
                   Chart.ReApplyAnalysisTechnique(Me)
               End Sub
            AddHandler subtractMinMoveBtn.Click,
                Sub()
                    Range -= Chart.GetMinTick()
                    Chart.ReApplyAnalysisTechnique(Me)
                End Sub
            AddHandler baseValueBtn.Click,
                Sub(sender As Object, e As EventArgs)
                    If Round(sender.CommandParameter, 5) <> Round(Range, 5) Then
                        Range = sender.CommandParameter
                        Chart.ReApplyAnalysisTechnique(Me)
                    End If
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

            addMinMoveBtn.Background = Brushes.LightGray
            subtractMinMoveBtn.Background = Brushes.LightGray

            baseValueBtn.Content = FormatNumberLengthAndPrefix(RoundTo(Chart.Settings("RangeValue").Value * RangeMultiplier, Chart.GetMinTick))
            baseValueBtn.CommandParameter = Round(RoundTo(Chart.Settings("RangeValue").Value * RangeMultiplier, Chart.GetMinTick), Chart.Settings("DecimalPlaces").Value)
            baseValueBtn.Background = Brushes.White

            currentPopupBtn.Content = FormatNumberLengthAndPrefix(Range)
            currentPopupBtn.Background = Brushes.LightBlue

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
        Function FormatNumberLengthAndPrefix(num As Decimal) As String
            Return Round(num * (10 ^ Chart.Settings("DecimalPlaces").Value))
        End Function
#End Region
    End Class

End Namespace
