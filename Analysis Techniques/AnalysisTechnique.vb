Imports System.Collections.ObjectModel
Namespace AnalysisTechniques

    Public MustInherit Class AnalysisTechnique
        Private ReadOnly _chart As Chart

        Public Sub New(ByVal chart As Chart)
            Me._chart = chart
            If chart IsNot Nothing AndAlso GetType(IChartPadAnalysisTechnique).IsAssignableFrom(Me.GetType) Then
                CType(Me, IChartPadAnalysisTechnique).InitChartPad()
                CType(Me, IChartPadAnalysisTechnique).ChartPad.Left = 400
                CType(Me, IChartPadAnalysisTechnique).ChartPad.Top = 100
            End If
        End Sub
        Protected ReadOnly Property Chart As Chart
            Get
                Return _chart
            End Get
        End Property
        Protected ReadOnly Property Bars(ByVal index As Integer) As BarData
            Get
                Return Chart.bars(index).Data
            End Get
        End Property
        Protected Property DrawVisuals As Boolean = True
        Private _currentBar As BarData
        Protected ReadOnly Property CurrentBar As BarData
            Get
                If loadingHistory Then
                    Return _currentBar
                Else
                    If Chart.bars.Count > 0 Then
                        Return Chart.bars(Chart.bars.Count - 1).Data
                    Else
                        Return Nothing
                    End If
                End If
            End Get
        End Property
        Protected ReadOnly Property CurrentBar(BarsBack As Integer) As BarData
            Get
                If loadingHistory Then
                    Return Chart.bars(_currentBar.Number - 1 - BarsBack).Data
                Else
                    If Chart.bars.Count > 0 Then
                        Return Chart.bars(Chart.bars.Count - 1 - BarsBack).Data
                    Else
                        Return Nothing
                    End If
                End If
            End Get
        End Property
        Public ReadOnly Property Inputs As ReadOnlyCollection(Of Input)
            Get
                Dim lst As New List(Of Input)
                For Each prop In Me.GetType.GetProperties
                    For Each attribute In prop.GetCustomAttributes(False)
                        If TypeOf attribute Is InputAttribute Then
                            lst.Add(New Input(Me, prop))
                            Exit For
                        End If
                    Next
                Next
                Return New ReadOnlyCollection(Of Input)(lst)
            End Get
        End Property

        Protected Sub SetLabelBar(ByVal lbl As Label, ByVal value As Integer)
            lbl.Location = New Point(value, lbl.Location.Y)
            lbl.RefreshVisual()
        End Sub
        Protected Sub SetLabelPrice(ByVal lbl As Label, ByVal value As Decimal)
            lbl.Location = New Point(lbl.Location.X, value)
            lbl.RefreshVisual()
        End Sub
        Protected Sub SetTLBeginBar(ByVal tl As TrendLine, ByVal value As Integer)
            tl.StartPoint = New Point(value, tl.StartPoint.Y)
            'tl.RefreshVisual()
        End Sub
        Protected Sub SetTLEndBar(ByVal tl As TrendLine, ByVal value As Integer)
            tl.EndPoint = New Point(value, tl.EndPoint.Y)
            'tl.RefreshVisual()
        End Sub
        Protected Sub SetTLBeginPrice(ByVal tl As TrendLine, ByVal value As Decimal)
            tl.StartPoint = New Point(tl.StartPoint.X, value)
            'tl.RefreshVisual()
        End Sub
        Protected Sub SetTLEndPrice(ByVal tl As TrendLine, ByVal value As Decimal)
            tl.EndPoint = New Point(tl.EndPoint.X, value)
            'tl.RefreshVisual()
        End Sub
        Protected Sub RemoveObjectFromChart(ByVal [object] As ChartDrawingVisual)
            If IsLastBarOnChart Or alwaysDisplayObjects Then
                Chart.Children.Remove([object])
            End If
            objectsToAdd.Remove([object])
            If [object] IsNot Nothing Then [object].HasPhantomParent = False
        End Sub
        Protected Sub AddObjectToChart(ByVal [object] As IChartObject)
            If IsLastBarOnChart Or alwaysDisplayObjects Then
                If [object] IsNot Nothing AndAlso [object].Parent Is Nothing Then Chart.Children.Add([object])
            Else
                If [object] IsNot Nothing AndAlso Not objectsToAdd.Contains([object]) Then
                    objectsToAdd.Add([object])
                    [object].HasPhantomParent = True
                End If
            End If
        End Sub
        Protected Sub RefreshObject(ByVal [object] As ChartDrawingVisual)
            If IsLastBarOnChart And Not loadingHistory Then [object].RefreshVisual()
        End Sub
        Private alwaysDisplayObjects As Boolean = True
        Private objectsToAdd As List(Of IChartObject)
        Protected Function NewPlot(ByVal color As Color, Optional pointCollection As List(Of Point) = Nothing, Optional ByVal addToChart As Boolean = True) As Plot
            Dim plot As New Plot
            plot.AutoRefresh = False
            plot.UseNegativeCoordinates = False
            plot.Pen = New Pen(New SolidColorBrush(color), 1)
            If pointCollection IsNot Nothing Then plot.Points = pointCollection
            plot.AnalysisTechnique = Me
            If IsLastBarOnChart Or alwaysDisplayObjects Then
                plot.DrawVisual = True
                plot.AutoRefresh = True
                If addToChart Then Chart.Children.Add(plot)
            Else
                plot.DrawVisual = False
                If addToChart Then
                    objectsToAdd.Add(plot)
                    plot.HasPhantomParent = True
                End If
            End If
            Return plot
        End Function
        Protected Function NewTrendLine(pen As Pen, point1 As Point, point2 As Point, Optional addToChart As Boolean = True) As TrendLine
            Dim tl = NewTrendLine(Colors.Red, point1, point2, addToChart)
            tl.Pen = pen.Clone
            Return tl
        End Function
        Protected Function NewTrendLine(pen As Pen, Optional addToChart As Boolean = True) As TrendLine
            Dim tl = NewTrendLine(Colors.Red, New Point(0, 0), New Point(0, 0), addToChart)
            tl.Pen = pen.Clone
            Return tl
        End Function
        Protected Function NewTrendLine(color As Color, Optional addToChart As Boolean = True) As TrendLine
            Return NewTrendLine(color, New Point(0, 0), New Point(0, 0), addToChart)
        End Function
        Protected Function NewTrendLine(ByVal color As Color, ByVal point1 As Point, ByVal point2 As Point, Optional ByVal addToChart As Boolean = True, Optional isEditable As Boolean = True) As TrendLine
            Dim line As New TrendLine
            line.AutoRefresh = False
            line.UseNegativeCoordinates = False
            line.IsEditable = False
            line.IsSelectable = False
            line.Pen = New Pen(New SolidColorBrush(color), 1)
            line.OuterPen = New Pen(New SolidColorBrush(color), 1)
            line.StartPoint = point1
            line.EndPoint = point2
            line.AnalysisTechnique = Me
            If IsLastBarOnChart Or alwaysDisplayObjects Then
                line.DrawVisual = True
                line.AutoRefresh = True
                If addToChart Then Chart.Children.Add(line)
            Else
                line.DrawVisual = False
                If addToChart Then
                    objectsToAdd.Add(line)
                    line.HasPhantomParent = True
                End If
            End If
            Return line
        End Function
        Protected Function NewLabel(ByVal text As String, ByVal foreground As Color, ByVal location As Point, Optional ByVal addToChart As Boolean = True, Optional font As Font = Nothing, Optional isEditable As Boolean = True) As Label
            Dim label As New Label
            label.AutoRefresh = False
            label.UseNegativeCoordinates = False
            label.IsEditable = isEditable
            label.IsSelectable = isEditable
            label.Text = text
            If font IsNot Nothing Then label.Font = font
            label.AnalysisTechnique = Me
            label.Font.Brush = New SolidColorBrush(foreground)
            label.Location = New Point(location.X, location.Y)
            If IsLastBarOnChart Or alwaysDisplayObjects Then
                label.DrawVisual = True
                label.AutoRefresh = True
                If addToChart Then Chart.Children.Add(label)
            Else
                label.DrawVisual = False
                If addToChart Then
                    objectsToAdd.Add(label)
                    label.HasPhantomParent = True
                End If
            End If
            Return label
        End Function
        Protected Function NewArrow(ByVal color As Color, ByVal location As Point, isFlipped As Boolean, Optional ByVal addToChart As Boolean = True) As Arrow
            Dim arrow As New Arrow
            arrow.AutoRefresh = False
            arrow.UseNegativeCoordinates = False
            arrow.Pen = New Pen(New SolidColorBrush(color), 1)
            arrow.AnalysisTechnique = Me
            arrow.IsFlipped = isFlipped
            arrow.Location = New Point(location.X, location.Y)
            If IsLastBarOnChart Or alwaysDisplayObjects Then
                arrow.DrawVisual = True
                arrow.AutoRefresh = True
                If addToChart Then Chart.Children.Add(arrow)
            Else
                arrow.DrawVisual = False
                If addToChart Then
                    objectsToAdd.Add(arrow)
                    arrow.HasPhantomParent = True
                End If
            End If
            Return arrow
        End Function
        Protected Function NewRectangle(ByVal borderColor As Color, fill As Color, ByVal topLeft As Point, bottomRight As Point, Optional addToChart As Boolean = True) As Rectangle
            Return NewRectangle(borderColor, fill, topLeft, Abs(bottomRight.X - topLeft.X), Abs(topLeft.Y - bottomRight.Y), addToChart)
        End Function
        Protected Function NewRectangle(ByVal borderColor As Color, fill As Color, ByVal location As Point, width As Double, height As Double, Optional addToChart As Boolean = True) As Rectangle
            Dim rect As New Rectangle
            rect.AutoRefresh = False
            rect.UseNegativeCoordinates = False
            rect.Pen = New Pen(New SolidColorBrush(borderColor), 1)
            rect.Fill = New SolidColorBrush(fill)
            rect.AnalysisTechnique = Me
            rect.Location = New Point(location.X, location.Y)
            rect.Width = width
            rect.Height = height
            If IsLastBarOnChart Or alwaysDisplayObjects Then
                rect.DrawVisual = True
                rect.AutoRefresh = True
                rect.RefreshVisual()
                If addToChart Then Chart.Children.Add(rect)
            Else
                rect.DrawVisual = False
                If addToChart Then
                    objectsToAdd.Add(rect)
                    rect.HasPhantomParent = True
                End If
            End If
            Return rect
        End Function
        Friend _isNewBar As Boolean
        Protected ReadOnly Property IsNewBar As Boolean
            Get
                Return _isNewBar Or Chart.IsNewBar
            End Get
        End Property

        Protected ReadOnly Property Price As Double
            Get
                Return CurrentBar.Close
            End Get
        End Property
        Private _lastBarOnChart As Boolean
        Protected ReadOnly Property IsLastBarOnChart As Boolean
            Get
                Return _lastBarOnChart
            End Get
        End Property
        Protected loadingHistory As Boolean
        Public Sub HistoryLoaded()
            If objectsToAdd IsNot Nothing Then
                For Each item In objectsToAdd
                    If item.Parent Is Nothing Then Chart.Children.Add(item)
                    If item.GetType.IsSubclassOf(GetType(DrawingVisual)) Then
                        'If TypeOf item Is Plot Then
                        '    If CType(item, Plot).Points.Count <> 0 Then
                        '        Dim a As New Object
                        '    End If
                        'End If
                        CType(item, DrawingVisual).DrawVisual = True
                        CType(item, DrawingVisual).AutoRefresh = True
                        CType(item, DrawingVisual).RefreshVisual()
                    End If
                Next
            End If
            OnLoaded()
        End Sub
        Friend Overridable Sub OnLoaded()

        End Sub
        Friend Overridable Sub OnCreate()

        End Sub
        Public Sub Update(ByVal price As Decimal)
            loadingHistory = False
            _lastBarOnChart = Not Chart.IsLoadingHistory
            If IsEnabled Then Main()
        End Sub
        Public Sub UpdateIntraBar(ByVal price As Decimal)
            loadingHistory = False
            _lastBarOnChart = Not Chart.IsLoadingHistory
            If IsEnabled Then Main_WithIntraBarMoves()
        End Sub
        Private [debug] As Boolean = True
        Public Sub Update(ByVal bar As BarData, history As Boolean)
            loadingHistory = history
            _lastBarOnChart = Chart.bars(Chart.bars.Count - 1).Data.Equals(bar)
            _currentBar = bar
            If IsEnabled Then
                'Try
                Main()
                'Catch ex As Exception
                '    'Chart.RemoveAnalysisTechnique(Me)
                '    If ShowInfoBox(Name & " encountered an error, and has been disabled.", Chart.DesktopWindow, "OK", "Details") = 1 Then
                '        ShowInfoBox(ex.Message & vbNewLine & ex.StackTrace, Chart.DesktopWindow)
                '    End If
                '    IsEnabled = False
                'End Try
                If debug Then
                    For Each child In Chart.Children
                        If TypeOf child Is TrendLine Then
                            CType(child, TrendLine).RefreshVisual()
                        End If
                    Next
                    DispatcherHelper.DoEvents()
                    System.Threading.Thread.Sleep(0)
                End If
            End If
        End Sub
        Public Sub UpdateNewBar()
            If IsEnabled Then
                'Try
                _isNewBar = True
                NewBar()
                'Catch ex As Exception
                '    IsEnabled = False
                '    Chart.RemoveAnalysisTechnique(Me)
                '    If ShowInfoBox(Name & " has encountered an error on a new bar.", Chart.DesktopWindow, "OK", "Details") = 1 Then
                '        ShowInfoBox(ex.Message & vbNewLine & ex.StackTrace, Chart.DesktopWindow)
                '    End If
                'End Try
            End If
        End Sub
        Protected Overridable Sub NewBar()

        End Sub
        Public Sub RemoveAllTechniqueObjects()
            Dim i As Integer
            While i < Chart.Children.Count
                If Chart.Children(i).AnalysisTechnique Is Me Then
                    Chart.Children.RemoveAt(i)
                Else
                    i += 1
                End If
            End While
        End Sub
        Public Overridable Sub Reset()
            debug = False
            _isNewBar = False
            alwaysDisplayObjects = debug
            RemoveAllTechniqueObjects()
            loadingHistory = True
            _lastBarOnChart = False
            If Chart.bars.Count > 0 Then _currentBar = Chart.bars(0).Data

            'Try
            Begin()

            'Catch ex As Exception
            '    If ShowInfoBox(Name & " has encountered an error on reset.", Chart.DesktopWindow, "OK", "Details") = 1 Then
            '        ShowInfoBox(ex.Message & vbNewLine & ex.StackTrace, Chart.DesktopWindow)
            '    End If
            'End Try
            'Dim t = Now
            'GC.Collect()
            'ShowInfoBox((Now - t).TotalMilliseconds)
        End Sub
        Public Property IsEnabled As Boolean

        Public Property Description As String = ""

        Protected MustOverride Sub Main()
        Protected Overridable Sub Main_WithIntraBarMoves()

        End Sub
        Protected Overridable Sub Begin()
            objectsToAdd = New List(Of IChartObject)
            If GetType(IChartPadAnalysisTechnique).IsAssignableFrom(Me.GetType) Then
                CType(Me, IChartPadAnalysisTechnique).ChartPad.Left = CType(Me, IChartPadAnalysisTechnique).ChartPadLocation.X
                CType(Me, IChartPadAnalysisTechnique).ChartPad.Top = CType(Me, IChartPadAnalysisTechnique).ChartPadLocation.Y
            End If
        End Sub
        Public MustOverride Property Name As String


    End Class

End Namespace