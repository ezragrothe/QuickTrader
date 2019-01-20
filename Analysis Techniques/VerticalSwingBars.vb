Namespace AnalysisTechniques

    Public Class VerticalSwingBars

#Region "AnalysisTechnique Inherited Code"
        Inherits AnalysisTechnique
        Implements IChartPadAnalysisTechnique

        ' Inherit the one-argument constructor from the base class.
        Public Sub New(ByVal chart As Chart)
            MyBase.New(chart) ' Call the base class constructor.
            Description = "The technique meant to be paired with the vertical swing bars bar type."
        End Sub
#End Region
        <Input()> Public Property UpTrendColor As Color = Colors.Green
        <Input()> Public Property DownTrendColor As Color = Colors.Red
        <Input()> Public Property NeutralColor As Color = Colors.Gray
        <Input()> Public Property GapColor As Color = Colors.Pink
        <Input()> Public Property OsBarColor As Color = Colors.Pink
        <Input()> Public Property PushCountTextColor As Color = Colors.Blue
        <Input()> Public Property PushCountFontSize As Decimal = 20
        <Input()> Public Property PushCountFontWeight As FontWeight = FontWeights.Bold
        <Input()> Public Property AbcBarColoring As Boolean = False
        <Input()> Public Property OsBarColoring As Boolean = False
        <Input()> Public Property OsHitLineThickness As Decimal = 1.2
        <Input()> Public Property NeutralBarThickness As Decimal = 1.2
        <Input()> Public Property TargetTextFontSize As Decimal = 12
        <Input()> Public Property TargetTextFontWeight As FontWeight = FontWeights.Bold
        <Input()> Public Property RVTargetTextLineLength As Integer = 4
        <Input()> Public Property BaseRange As Integer = 10
        <Input()> Public Property BarThickness As Decimal = 1
        <Input()> Public Property GapThickness As Decimal = 1


        Private currentGapLine As TrendLine
        Private rangePoints As List(Of Point)
        Private potentialPoint As Point
        Private trendDirection As Direction
        Private rvText As Label
        Private osHitLine1 As TrendLine
        Private mergedTrend As LineCoordinates
        Private mergedDirection As Boolean
        Private pushCountText As Label
        Private reversalSwingMove As Boolean

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

        Function CreateText(position As Point, yOffset As Double, direction As Direction, text As String, color As Color, fontSize As Double, fontWeight As FontWeight) As Label
            Dim label As Label = NewLabel(text, color, AddToY(position, Chart.GetRelativeFromRealHeight(If(direction, 1, -1) * yOffset)))
            label.Font.FontSize = fontSize
            label.Font.FontWeight = fontWeight
            label.IsEditable = False
            label.IsSelectable = False
            label.HorizontalAlignment = LabelHorizontalAlignment.Center
            If direction = Direction.Up Then label.VerticalAlignment = LabelVerticalAlignment.Bottom Else label.VerticalAlignment = LabelVerticalAlignment.Top
            Return label
        End Function

        Protected Overrides Sub Begin()
            MyBase.Begin()
            currentGapLine = NewTrendLine(GapColor)
            currentGapLine.Pen.Thickness = GapThickness
            rangePoints = New List(Of Point)
            rangePoints.Add(New Point(CurrentBar.Number, CurrentBar.Close))
            rangePoints.Add(New Point(CurrentBar.Number, CurrentBar.Close))
            potentialPoint = New Point(CurrentBar.Number, CurrentBar.Close)
            trendDirection = Direction.Up

            rvText = NewLabel("", NeutralColor, New Point(0, 0))
            rvText.Font.FontSize = TargetTextFontSize
            rvText.Font.FontWeight = TargetTextFontWeight
            rvText.IsSelectable = False
            rvText.IsEditable = False
            rvText.Text = Strings.StrDup(RVTargetTextLineLength, "─") & FormatNumberLengthAndPrefix(Chart.Settings("RangeValue").Value)

            osHitLine1 = NewTrendLine(OsBarColor) : osHitLine1.Pen.Thickness = OsHitLineThickness

            For i = 0 To Chart.bars.Count - 1
                'Chart.bars(i).Pen.Thickness = BarThickness
                'Chart.bars(i).TickWidth = 1
                If Not TypeOf Chart.bars(i).Pen.Brush Is SolidColorBrush Then
                    Chart.bars(i).Pen.Brush = New SolidColorBrush
                End If
                If CType(Chart.bars(i).Pen.Brush, SolidColorBrush).Color <> NeutralColor Then
                    Chart.bars(i).Pen.Brush = New SolidColorBrush(NeutralColor)
                    Chart.bars(i).Pen.Thickness = NeutralBarThickness
                    RefreshObject(Chart.bars(i))
                End If
            Next

            mergedTrend = New LineCoordinates(0, 0, 0, 0)
        End Sub
        Private Property CurrentPoint(Optional index As Integer = 0) As Point
            Get
                Return rangePoints(rangePoints.Count - 1 - index)
            End Get
            Set(value As Point)
                rangePoints(rangePoints.Count - 1 - index) = value
            End Set
        End Property
        Protected Overrides Sub NewBar()
            Dim range As Decimal = Chart.Settings("RangeValue").Value
            If CurrentBar.Number > 4 Then
                If (CurrentBar.Direction = False And CurrentBar(2).Low < CurrentBar(3).Low - range And CurrentBar(1).High < CurrentBar(3).High - range) Or (CurrentBar.Direction = True And CurrentBar(2).High > CurrentBar(3).High + range And CurrentBar(1).Low > CurrentBar(3).Low + range) Then
                    'If currentGapLine.HasParent Or currentGapLine.HasPhantomParent Then
                    'If Not ((CurrentBar.Direction = False And CurrentBar.Low < CurrentBar(1).Low - range) Or (CurrentBar.Direction = True And CurrentBar.High > CurrentBar(1).High + range)) Then
                    If Not loadingHistory Then
                        currentGapLine = NewTrendLine(GapColor)
                        currentGapLine.Pen.Thickness = GapThickness
                        RemoveObjectFromChart(currentGapLine)
                    End If
                    '    End If
                    'End If
                End If
            End If
        End Sub
        Protected Overrides Sub Main()
            'check for gap swing
            Chart.bars(CurrentBar.Number - 1).Pen.Thickness = NeutralBarThickness
            'Log("hello")
            Chart.bars(CurrentBar.Number - 1).TickWidth = 1
            Chart.bars(CurrentBar.Number - 1).Pen.Brush = New SolidColorBrush(NeutralColor)
            RefreshObject(Chart.bars(CurrentBar.Number - 1))
            Dim range As Decimal = Chart.Settings("RangeValue").Value
            If CurrentBar.Number > 4 Then
                If (CurrentBar.Direction = False And CurrentBar(2).Low < CurrentBar(3).Low - range And CurrentBar(1).High < CurrentBar(3).High - range) Or (CurrentBar.Direction = True And CurrentBar(2).High > CurrentBar(3).High + range And CurrentBar(1).Low > CurrentBar(3).Low + range) Then
                    'If currentGapLine.HasParent Or currentGapLine.HasPhantomParent Then
                    'If Not ((CurrentBar.Direction = False And CurrentBar.Low < CurrentBar(1).Low - range) Or (CurrentBar.Direction = True And CurrentBar.High > CurrentBar(1).High + range)) Then
                    If loadingHistory Then
                        currentGapLine = NewTrendLine(GapColor)
                        RemoveObjectFromChart(currentGapLine)
                        currentGapLine.Pen.Thickness = GapThickness
                    End If
                    '    End If
                    'End If
                End If
                Dim update As Boolean
                If CurrentBar.Direction = False And CurrentBar.Low < CurrentBar(1).Low - range Then
                    currentGapLine.Coordinates = New LineCoordinates(CurrentBar.Number, CurrentBar(1).Low, CurrentBar.Number, CurrentBar.Low + range)
                    update = True
                ElseIf CurrentBar.Direction = True And CurrentBar.High > CurrentBar(1).High + range Then
                    currentGapLine.Coordinates = New LineCoordinates(CurrentBar.Number, CurrentBar(1).High, CurrentBar.Number, CurrentBar.High - range)
                    update = True
                ElseIf CurrentBar.Direction = True And CurrentBar(1).Low < CurrentBar(2).Low - range Then
                    currentGapLine.Coordinates = New LineCoordinates(CurrentBar(1).Number, CurrentBar(2).Low, CurrentBar(1).Number, Min(CurrentBar(2).Low, CurrentBar.High))
                    update = True
                ElseIf CurrentBar.Direction = False And CurrentBar(1).High > CurrentBar(2).High + range Then
                    currentGapLine.Coordinates = New LineCoordinates(CurrentBar(1).Number, CurrentBar(2).High, CurrentBar(1).Number, Max(CurrentBar(2).High, CurrentBar.Low))
                    update = True
                End If
                If update Then
                    currentGapLine.DrawVisual = True
                    currentGapLine.AutoRefresh = True
                    currentGapLine.RefreshVisual()
                    RemoveObjectFromChart(currentGapLine)
                    AddObjectToChart(currentGapLine)
                End If
            End If
            If Not OsBarColoring Xor AbcBarColoring Then
                Dim startColorBar As Integer = -1
                Dim u = CurrentBar.Direction
                For i = CurrentBar.Number - 1 To 0 Step -1
                    If (u And Chart.bars(i).Data.High > CurrentBar.High) Or
                        (Not u And Chart.bars(i).Data.Low < CurrentBar.Low) Then
                        Dim startRangePoint As New Point(0, If(u, Double.MaxValue, 0))
                        For j = i To CurrentBar.Number - 1
                            If (u And Chart.bars(j).Data.Low <= startRangePoint.Y) Or
                                (Not u And Chart.bars(j).Data.High >= startRangePoint.Y) Then
                                startRangePoint = New Point(j, If(u, Chart.bars(j).Data.Low, Chart.bars(j).Data.High))
                            End If
                        Next
                        Dim endRangePoint As New Point(0, If(u, 0, Double.MaxValue))
                        For j = startRangePoint.X To CurrentBar.Number - 1
                            If (u And Chart.bars(j).Data.High >= endRangePoint.Y) Or
                                (Not u And Chart.bars(j).Data.Low <= endRangePoint.Y) Then
                                endRangePoint = New Point(j, If(u, Chart.bars(j).Data.High, Chart.bars(j).Data.Low))
                            End If
                        Next
                        If endRangePoint.X = CurrentBar.Number - 1 And endRangePoint.X - startRangePoint.X >= 2 Then
                            startColorBar = startRangePoint.X
                        End If
                        Exit For
                    ElseIf i = 0 Then
                        startColorBar = 0
                    End If
                Next
                If startColorBar <> -1 Then
                    BarColorRoutine(startColorBar + 1, CurrentBar.Number, If(CurrentBar.Direction, UpTrendColor, DownTrendColor))
                End If
            ElseIf OsBarColoring Then
                If CurrentBar.Number > 1 Then
                    If Round(CurrentBar.Range, 5) >= Round(CurrentBar(1).Range, 5) Then
                        ColorCurrentBar(OsBarColor)
                        If CurrentBar.Direction Then
                            osHitLine1.Coordinates = New LineCoordinates(CurrentBar.Number + 1, CurrentBar.Low, CurrentBar.Number + 2, CurrentBar.Low) : osHitLine1.ExtendRight = True
                        Else
                            osHitLine1.Coordinates = New LineCoordinates(CurrentBar.Number + 1, CurrentBar.High, CurrentBar.Number + 2, CurrentBar.High) : osHitLine1.ExtendRight = True
                        End If
                    Else
                        If CurrentBar(1).Low < CurrentBar.Low Then
                            osHitLine1.Coordinates = New LineCoordinates(CurrentBar.Number + 1, CurrentBar(1).Low, CurrentBar.Number + 2, CurrentBar(1).Low) : osHitLine1.ExtendRight = True
                        ElseIf CurrentBar(1).High > CurrentBar.High Then
                            osHitLine1.Coordinates = New LineCoordinates(CurrentBar.Number + 1, CurrentBar(1).High, CurrentBar.Number + 2, CurrentBar(1).High) : osHitLine1.ExtendRight = True
                        End If
                    End If
                End If
            ElseIf AbcBarColoring Then
                If mergedTrend.StartPoint = New Point(0, 0) Then
                    If CurrentBar.Direction Then
                        mergedTrend = New LineCoordinates(New Point(CurrentBar.Number, CurrentBar.Low), New Point(CurrentBar.Number, CurrentBar.High))
                        mergedDirection = True
                    Else
                        mergedTrend = New LineCoordinates(New Point(CurrentBar.Number, CurrentBar.High), New Point(CurrentBar.Number, CurrentBar.Low))
                        mergedDirection = False
                    End If
                    pushCountText = CreateText(mergedTrend.EndPoint, 0, mergedDirection, "1", PushCountTextColor, PushCountFontSize, PushCountFontWeight)
                    ColorCurrentBar(If(CurrentBar.Direction = Direction.Up, UpTrendColor, DownTrendColor))
                End If

                'extension
                Dim ext As Boolean
                If mergedDirection And CurrentBar.Direction And Round(CurrentBar.High, 5) >= Round(mergedTrend.EndPoint.Y, 5) Then
                    BarColorRoutine(mergedTrend.EndPoint.X, CurrentBar.Number, UpTrendColor)
                    mergedTrend = New LineCoordinates(mergedTrend.StartPoint, New Point(CurrentBar.Number, CurrentBar.High))
                    ext = True
                ElseIf Not mergedDirection And Not CurrentBar.Direction And Round(CurrentBar.Low, 5) <= Round(mergedTrend.EndPoint.Y, 5) Then
                    BarColorRoutine(mergedTrend.EndPoint.X, CurrentBar.Number, DownTrendColor)
                    mergedTrend = New LineCoordinates(mergedTrend.StartPoint, New Point(CurrentBar.Number, CurrentBar.Low))
                    ext = True
                End If
                If ext Then
                    pushCountText.Location = mergedTrend.EndPoint
                    If reversalSwingMove Then
                        pushCountText.Text = CInt(pushCountText.Text) + 1
                    End If
                    reversalSwingMove = False
                End If


                ' check for new 3-swing trend - check if there is at least a couple swings in between last trend and now
                If CurrentBar.Number >= CInt(mergedTrend.EndPoint.X) + 3 Then
                    Dim bPoint As Point = New Point(0, If(mergedDirection, Double.MaxValue, Double.MinValue))

                    For searchBar = CInt(mergedTrend.EndPoint.X) + 1 To CurrentBar.Number
                        If Not mergedDirection And Round(Chart.bars(searchBar - 1).Data.High, 5) >= Round(bPoint.Y, 5) Then
                            bPoint = New Point(searchBar, Chart.bars(searchBar - 1).Data.High)
                        ElseIf mergedDirection And Round(Chart.bars(searchBar - 1).Data.Low, 5) <= Round(bPoint.Y, 5) Then
                            bPoint = New Point(searchBar, Chart.bars(searchBar - 1).Data.Low)
                        End If
                    Next
                    If bPoint.X = CurrentBar.Number Then
                        reversalSwingMove = False
                        If mergedDirection Then
                            mergedTrend = New LineCoordinates(AddToX(mergedTrend.EndPoint, 1), New Point(CurrentBar.Number, CurrentBar.Low))
                        Else
                            mergedTrend = New LineCoordinates(AddToX(mergedTrend.EndPoint, 1), New Point(CurrentBar.Number, CurrentBar.High))
                        End If
                        'NewTrendLine(Colors.Pink, mergedTrend.StartPoint, mergedTrend.EndPoint)
                        pushCountText = CreateText(mergedTrend.EndPoint, 0, Not mergedDirection, "2", PushCountTextColor, PushCountFontSize, PushCountFontWeight)
                        BarColorRoutine(mergedTrend.StartPoint.X, CurrentBar.Number, If(Not mergedDirection, UpTrendColor, DownTrendColor))
                        mergedDirection = Not mergedDirection
                    End If
                End If
                'reversal trend
                If mergedDirection And Not CurrentBar.Direction And Round(CurrentBar.Low, 5) <= Round(mergedTrend.StartPoint.Y, 5) Then
                    mergedTrend = New LineCoordinates(New Point(CurrentBar.Number, CurrentBar.High), New Point(CurrentBar.Number, CurrentBar.Low))
                    ColorCurrentBar(DownTrendColor)
                    mergedDirection = Not mergedDirection
                    reversalSwingMove = False
                    pushCountText = CreateText(mergedTrend.EndPoint, 0, mergedDirection, "1", PushCountTextColor, PushCountFontSize, PushCountFontWeight)
                ElseIf Not mergedDirection And CurrentBar.Direction And Round(CurrentBar.High, 5) >= Round(mergedTrend.StartPoint.Y, 5) Then
                    mergedTrend = New LineCoordinates(New Point(CurrentBar.Number, CurrentBar.Low), New Point(CurrentBar.Number, CurrentBar.High))
                    ColorCurrentBar(UpTrendColor)
                    mergedDirection = Not mergedDirection
                    reversalSwingMove = False
                    pushCountText = CreateText(mergedTrend.EndPoint, 0, mergedDirection, "1", PushCountTextColor, PushCountFontSize, PushCountFontWeight)
                End If

                If mergedDirection <> CurrentBar.Direction Then
                    reversalSwingMove = True
                End If

            End If

            rvText.Location = New Point(CurrentBar.Number, If(CurrentBar.Direction, CurrentBar.High - Chart.Settings("RangeValue").Value, CurrentBar.Low + Chart.Settings("RangeValue").Value))
        End Sub
        Private Sub ColorCurrentBar(color As Color)
            Chart.bars(CurrentBar.Number - 1).Pen.Thickness = BarThickness
            Chart.bars(CurrentBar.Number - 1).Pen.Brush = New SolidColorBrush(color)
            RefreshObject(Chart.bars(CurrentBar.Number - 1))
        End Sub

        Protected Sub BarColorRoutine(ByVal startBar As Integer, ByVal endBar As Integer, ByVal color As Color)
            For i = Max(startBar - 1, 0) To Max(endBar - 1, 0)
                Chart.bars(i).Pen.Thickness = BarThickness
                If Not TypeOf Chart.bars(i).Pen.Brush Is SolidColorBrush Then
                    Chart.bars(i).Pen.Brush = New SolidColorBrush
                End If
                If CType(Chart.bars(i).Pen.Brush, SolidColorBrush).Color <> color Then
                    Chart.bars(i).Pen.Brush = New SolidColorBrush(color)
                    RefreshObject(Chart.bars(i))
                End If
            Next
        End Sub
        Public Overrides Property Name As String = Me.GetType.Name
#Region "AutoTrendPad"

        Dim grabArea As Border
        Dim bd As Border
        Dim grd As Grid
        Private Sub InitGrid()
            grd = New Grid
            bd = New Border
            grd.Margin = New Thickness(0)
            grd.HorizontalAlignment = Windows.HorizontalAlignment.Center
            For i = 1 To 20
                grd.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Auto)})
            Next
            For i = 1 To 2
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
            'grabArea.Height = 17
            grabArea.Width = 12
        End Sub
        Sub SetRows()
            Grid.SetColumn(grabArea, 10)
            Grid.SetColumn(abcBtn, 11)
            Grid.SetColumn(osBtn, 12)
            Grid.SetColumn(plusBtn, 0)
            Grid.SetColumn(minusBtn, 9)
            grd.Children.Add(minusBtn)
            grd.Children.Add(plusBtn)
            grd.Children.Add(grabArea)
            grd.Children.Add(abcBtn)
            grd.Children.Add(osBtn)
        End Sub
        Dim plusBtn As Button
        Dim minusBtn As Button
        Dim abcBtn As Button
        Dim osBtn As Button
        Sub InitControls()
            Dim fontsize As Integer = 16
            plusBtn = New Button With {.Background = New SolidColorBrush(Color.FromArgb(255, 230, 235, 255)), .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "+"}
            minusBtn = New Button With {.Background = New SolidColorBrush(Color.FromArgb(255, 230, 235, 255)), .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "-"}
            abcBtn = New Button With {.Background = Brushes.White, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "ABC"}
            osBtn = New Button With {.Background = Brushes.White, .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "OS"}
            Dim rBtn = New Button With {.Background = New SolidColorBrush(Color.FromArgb(255, 230, 235, 255)), .Padding = New Thickness(3, 2, 3, 2), .Foreground = Brushes.Black, .Margin = New Thickness(0.5), .FontSize = fontsize, .MinWidth = 19, .Content = "-1"}
            AddHandler plusBtn.Click,
                Sub()
                    BaseRange += 4
                    UpdateChartPad()
                End Sub
            AddHandler minusBtn.Click,
                Sub()
                    BaseRange = Max(BaseRange - 4, 1)
                    UpdateChartPad()
                End Sub
            AddHandler osBtn.Click,
                Sub()
                    AbcBarColoring = False
                    OsBarColoring = Not OsBarColoring
                    Chart.ReApplyAnalysisTechnique(Me)
                End Sub
            AddHandler abcBtn.Click,
                Sub()
                    OsBarColoring = False
                    AbcBarColoring = Not AbcBarColoring
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
            ChartPad.Content = bd
        End Sub
        Dim buttonCount As Integer = 7
        Public Sub UpdateChartPad() Implements IChartPadAnalysisTechnique.UpdateChartPad
            grd.Children.Clear()
            grd.Children.Add(grabArea)
            grd.Children.Add(plusBtn)
            grd.Children.Add(minusBtn)
            grd.Children.Add(osBtn)
            grd.Children.Add(abcBtn)
            For y = 0 To 0
                For x = 0 To buttonCount
                    Dim value = Round(((buttonCount - x) + BaseRange) * Chart.GetMinTick(), 4)
                    Dim btn As New Button With {.Padding = New Thickness(3, 2, 3, 2), .Tag = value, .MinWidth = 25, .MinHeight = 25, .Margin = New Thickness(0.5), .Content = FormatNumberLengthAndPrefix(value), .FontSize = 14.5, .Foreground = Brushes.Black}
                    Grid.SetRow(btn, y)
                    Grid.SetColumn(btn, x + 1)
                    btn.Background = Brushes.White
                    If value = Round(RoundTo(Chart.Settings("RangeValue").Value, Chart.GetMinTick), 4) Then
                        btn.Background = New SolidColorBrush(Colors.LightBlue)
                    End If
                    grd.Children.Add(btn)
                    AddHandler btn.Click,
                        Sub(sender As Object, e As EventArgs)
                            Chart.ChangeRange(CDec(sender.Tag))
                        End Sub
                Next
            Next
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

