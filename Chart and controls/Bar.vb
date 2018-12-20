Public Class ChartBars
    Inherits ChartDrawingVisual


    Private _bars As New List(Of BarData)
    Public ReadOnly Property Bars As ObjectModel.ReadOnlyCollection(Of BarData)
        Get
            Return New ObjectModel.ReadOnlyCollection(Of BarData)(_bars)
        End Get
    End Property

    Protected Overrides Property Drawing As System.Windows.Media.DrawingGroup

    Public Sub AddBar(barData As BarData)

    End Sub
    Public Sub UpdateLastBar(barData As BarData)

    End Sub
    Public Overrides Sub RefreshVisual()

    End Sub
    ''' <summary>
    ''' Returns the bar at the specified index.
    ''' </summary>
    Default Public ReadOnly Property Item(index As Integer) As BarData
        Get
            Return _bars(index)
        End Get
    End Property
    Public Overrides Function ContainsPoint(point As System.Windows.Point) As Boolean
        Return False
    End Function
End Class

Public Class Bar
    Inherits ChartDrawingVisual
    Implements ISelectable

    Public Sub New()
        'Dim blur As New Effects.DropShadowEffect
        'blur.ShadowDepth = 30
        'blur.BlurRadius = 6
        'blur.Color = Colors.LightGray
        'Effect = blur
        TickWidth = My.Application.TickWidth
    End Sub


    Public Sub New(ByVal data As BarData)
        Me.New()
        Me.Data = data
        IsPopupMoveable = False
    End Sub
    Public Overrides Property UseNegativeCoordinates As Boolean
        Get
            Return False
        End Get
        Set(ByVal value As Boolean)
            If value Then Throw New System.Data.ReadOnlyException("Bars can not be user controls.")
        End Set
    End Property
    Private _data As BarData
    Public Overloads Property Data As BarData
        Get
            Return _data
        End Get
        Set(ByVal value As BarData)
            _data = value
            RefreshVisual()
        End Set
    End Property

    Private _close As Boolean
    Public Property CloseTick As Boolean
        Get
            Return _close
        End Get
        Set(ByVal value As Boolean)
            _close = value
            If AutoRefresh Then RefreshVisual()
        End Set
    End Property
    Private _open As Boolean
    Public Property OpenTick As Boolean
        Get
            Return _open
        End Get
        Set(ByVal value As Boolean)
            _open = value
            If AutoRefresh Then RefreshVisual()
        End Set
    End Property
    Public Property TickWidth As Double = 0.7
    Private _openPen As Pen = Pen
    Public Property CloseTickPen As Pen
        Get
            Return _openPen
        End Get
        Set(ByVal value As Pen)
            _openPen = Pen
            If AutoRefresh Then RefreshVisual()
        End Set
    End Property
    Private _closePen As Pen = Pen
    Public Property OpenTickPen As Pen
        Get
            Return _closePen
        End Get
        Set(ByVal value As Pen)
            _closePen = Pen
            If AutoRefresh Then RefreshVisual()
        End Set
    End Property
    'Public Overrides Property Pen As System.Windows.Media.Pen = New Pen(white, 2)
    'Public Overrides ReadOnly Property Geometry As System.Windows.Media.Geometry
    '    Get
    '        Dim high As Double = Parent.GetRealFromRelativeY(-Data.High)
    '        Dim low As Double = Parent.GetRealFromRelativeY(-Data.Low)
    '        Dim index As Double = Parent.GetRealFromRelativeX(Data.Number)
    '        Return New LineGeometry(New Point(index, high), New Point(index, low))
    '    End Get
    'End Property
    Private _drawing As DrawingGroup
    Protected Overrides Property Drawing As System.Windows.Media.DrawingGroup
        Get
            Return _drawing
        End Get
        Set(ByVal value As System.Windows.Media.DrawingGroup)
            _drawing = value
        End Set
    End Property
    Private visualCleared As Boolean
    Public Overrides Sub RefreshVisual()
        If HasParent Then
            Dim high As Double = Parent.GetRealFromRelativeY(-Data.High)
            Dim low As Double = Parent.GetRealFromRelativeY(-Data.Low)
            Dim open As Double = Parent.GetRealFromRelativeY(-Data.Open)
            Dim close As Double = Parent.GetRealFromRelativeY(-Data.Close)
            Dim index As Double = Parent.GetRealFromRelativeX(Data.Number)
            Dim tickWidth As Double = Parent.GetRealFromRelativeWidth(Me.TickWidth)
            If Parent.Bounds.X1 < Data.Number AndAlso Parent.Bounds.X2 > Data.Number Then
                If DrawVisual Then
                    visualCleared = False
                    Dim geo As New GeometryGroup
                    geo.Children.Add(New LineGeometry(New Point(index, high), New Point(index, low)))
                    If CloseTick Then
                        geo.Children.Add(New LineGeometry(New Point(index + tickWidth, close), New Point(index, close)))
                    End If
                    If OpenTick Then
                        'dc.DrawGeometry(_openPen.Brush, _openPen, New LineGeometry(New Point(index - tickWidth, open), New Point(index, open)))
                        geo.Children.Add(New LineGeometry(New Point(index - tickWidth, open), New Point(index, open)))
                    End If
                    Using dc As DrawingContext = RenderOpen()
                        dc.DrawGeometry(Pen.Brush, Pen, geo)
                    End Using
                Else
                    If visualCleared = False Then
                        RenderOpen.Close()
                        visualCleared = True
                    End If
                End If
            ElseIf Not visualCleared Then
                If DrawVisual Then
                    RenderOpen.Close()
                End If
                visualCleared = True
            End If
        End If
    End Sub
    Private Sub RefreshGeometry()

    End Sub

    Protected Overrides Function PopupInformation() As System.Collections.Generic.List(Of System.Collections.Generic.KeyValuePair(Of String, String))
        'Dim items As New List(Of KeyValuePair(Of String, String))
        'items.Add(New KeyValuePair(Of String, String)("High: ", FormatNumber(Data.High, Parent.Settings("DecimalPlaces").Value).Replace(",", "")))
        'items.Add(New KeyValuePair(Of String, String)("Low: ", FormatNumber(Data.Low, Parent.Settings("DecimalPlaces").Value).Replace(",", "")))
        'items.Add(New KeyValuePair(Of String, String)("Close: ", FormatNumber(Data.Close, Parent.Settings("DecimalPlaces").Value).Replace(",", "")))
        'items.Add(New KeyValuePair(Of String, String)("Open: ", FormatNumber(Data.Open, Parent.Settings("DecimalPlaces").Value).Replace(",", "")))
        'items.Add(New KeyValuePair(Of String, String)("Date: ", Data.Date.ToShortDateString))
        'items.Add(New KeyValuePair(Of String, String)("Time: ", Data.Date.ToLongTimeString))
        'items.Add(New KeyValuePair(Of String, String)("Number: ", Data.Number))
        'items.Add(New KeyValuePair(Of String, String)("Duration: ", Data.Duration.ToString))
        'items.Add(New KeyValuePair(Of String, String)("Range: ", Data.Volume))
        'Return items
        Return Nothing
    End Function

    'Private _isSelected As Boolean
    'Private originalThickness As Double = -1
    Public Property IsSelected As Boolean Implements ISelectable.IsSelected
    '    Get
    '        Return _isSelected
    '    End Get
    '    Set(ByVal value As Boolean)
    '        _isSelected = value
    '        If value Then
    '            If originalThickness = -1 Then originalThickness = Pen.Thickness
    '            Pen.Thickness += 2
    '        Else
    '            If originalThickness <> -1 Then
    '                Pen.Thickness = originalThickness
    '                originalThickness = -1
    '            End If
    '        End If
    '    End Set
    'End Property
    Public Property IsSelectable As Boolean = False Implements ISelectable.IsSelectable
    Public Overrides Sub ParentMouseLeftButtonDown(ByVal e As MouseButtonEventArgs, ByVal location As System.Windows.Point)
        MyBase.ParentMouseLeftButtonDown(e, location)
        'If My.Application.ShowMousePopups = False Then
        '    IsSelectable = False
        'Else
        '    IsSelectable = True
        '    IsSelected = True
        'End If
    End Sub
    Public Overrides Sub ParentMouseLeftButtonUp(ByVal e As System.Windows.Input.MouseButtonEventArgs, ByVal location As System.Windows.Point)
        MyBase.ParentMouseLeftButtonUp(e, location)
        'If IsSelectable Then IsSelected = False
    End Sub
    Public Overrides Sub ParentMouseDoubleClick(ByVal e As MouseButtonEventArgs, ByVal location As System.Windows.Point)
        MyBase.ParentMouseDoubleClick(e, location)
        ChartCommands.Format.Execute(Nothing, Parent)
    End Sub
    Public Overrides Property ClickLeniency As Double = 5
    Friend Overrides Sub Command_Executed(ByVal sender As Object, ByVal e As System.Windows.Input.ExecutedRoutedEventArgs)
    End Sub
    Public Overrides Function ToString() As String
        Return String.Format("{0},{1},{2},{3}; {4}", Data.Open, Data.High, Data.Low, Data.Close, Data.Date.ToShortTimeString)
    End Function

    Public Overrides Function ContainsPoint(ByVal point As System.Windows.Point) As Boolean
        Dim high As Double = Parent.GetRealFromRelativeY(-Data.High)
        Dim low As Double = Parent.GetRealFromRelativeY(-Data.Low)
        Dim index As Double = Parent.GetRealFromRelativeX(Data.Number)
        Dim rect As New Rect(New Point(index - 2.5, high - 2), New Point(index + 2.5, low + 2))
        Return rect.Contains(point)
    End Function
End Class