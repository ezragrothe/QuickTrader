Imports System.Windows.Media
Imports System.Math
Public Class Rectangle
    Inherits ChartDrawingVisual
    Implements ISelectable, ISerializable

    'Protected chart As Chart
    'Public Sub New(ByVal chart As Chart)
    '    Me.chart = chart
    'End Sub
    Public Sub New()
    End Sub

    Protected baseRectGeometry As New RectangleGeometry
    Protected parallelLineGeometry As New GeometryGroup

    Protected _drawing As DrawingGroup
    Protected Overrides Property Drawing As System.Windows.Media.DrawingGroup
        Get
            Return _drawing
        End Get
        Set(ByVal value As System.Windows.Media.DrawingGroup)
            _drawing = value
        End Set
    End Property

    Protected _relativeCoordinates As New Rect

    Protected Function GetSnappedCoordinates() As Rect
        Dim coordinates As Rect
        If SnapToRelativePixels Then
            coordinates.Location = New Point(Round(Location.X), Location.Y)
        Else
            coordinates.Location = _relativeCoordinates.Location
        End If
        If SnapToRelativePixels Then
            coordinates.Size = New Point(Round(Width), Height)
        Else
            coordinates.Size = _relativeCoordinates.Size
        End If
        Return coordinates
    End Function

    Public Property Location As Point
        Get
            Return _relativeCoordinates.Location
        End Get
        Set(ByVal value As Point)
            _relativeCoordinates.Location = value
            If AutoRefresh Then RefreshVisual()
        End Set
    End Property
    Public Property X As Double
        Get
            Return _relativeCoordinates.Location.X
        End Get
        Set(ByVal value As Double)
            _relativeCoordinates.Location = New Point(value, _relativeCoordinates.Y)
            If AutoRefresh Then RefreshVisual()
        End Set
    End Property
    Public Property Y As Double
        Get
            Return _relativeCoordinates.Location.Y
        End Get
        Set(ByVal value As Double)
            _relativeCoordinates.Location = New Point(_relativeCoordinates.X, value)
            If AutoRefresh Then RefreshVisual()
        End Set
    End Property
    Public Property Width As Double
        Get
            Return _relativeCoordinates.Width
        End Get
        Set(value As Double)
            _relativeCoordinates.Width = value
            If AutoRefresh Then RefreshVisual()
        End Set
    End Property
    Public Property Height As Double
        Get
            Return _relativeCoordinates.Height
        End Get
        Set(value As Double)
            _relativeCoordinates.Height = value
            If AutoRefresh Then RefreshVisual()
        End Set
    End Property

    Public Property Coordinates As Rect
        Get
            Return _relativeCoordinates
        End Get
        Set(ByVal value As Rect)
            _relativeCoordinates = value
            If AutoRefresh Then
                RefreshVisual()
            End If
        End Set
    End Property
    Private _borderThickness As Thickness = New Thickness(1, 1, 1, 1)
    Public Property BorderThickness As Thickness
        Get
            Return _borderThickness
        End Get
        Set(value As Thickness)
            _borderThickness = value
            If AutoRefresh Then
                RefreshVisual()
            End If
        End Set
    End Property
    Protected _isSelected As Boolean
    Public Property IsSelected As Boolean Implements ISelectable.IsSelected
        Get
            Return _isSelected
        End Get
        Set(ByVal value As Boolean)
            _isSelected = value
            RefreshVisual()
        End Set
    End Property
    Public Property IsSelectable As Boolean = True Implements ISelectable.IsSelectable
    Public Overrides Property IsEditable As Boolean = True



    Protected _pen As Pen = New Pen(Nothing, 1)
    Public Overrides Property Pen As System.Windows.Media.Pen
        Get
            Return _pen
        End Get
        Set(ByVal value As System.Windows.Media.Pen)
            _pen = value
            If AutoRefresh Then RefreshVisual()
        End Set
    End Property
    Protected _fill As Brush = Nothing
    Public Property Fill As System.Windows.Media.Brush
        Get
            Return _fill
        End Get
        Set(ByVal value As System.Windows.Media.Brush)
            _fill = value
            If AutoRefresh Then RefreshVisual()
        End Set
    End Property
    
    ' for use only in the day box code. limits the amount of movement the box can do. does not affect resizing.
    Public Property MaxTopValue As Decimal
    Public Property MaxBottomValue As Decimal
    Public Property ResizeByEdges As Boolean = True
    Public Property DragCircleSize As Double = 6

    Public Overrides ReadOnly Property Parent As Chart
        Get
            Return MyBase.Parent
        End Get
    End Property

    Public Overrides Sub RefreshVisual()

        If HasParent Then
            Dim realCoordinates As Rect
            If UseNegativeCoordinates Then
                realCoordinates = Parent.GetRealFromRelative(GetSnappedCoordinates)
            Else
                realCoordinates = Parent.GetRealFromRelative(NegateY(GetSnappedCoordinates))
            End If

            baseRectGeometry = New RectangleGeometry(realCoordinates)
            Dim edgeGeometry As New GeometryGroup
            If BorderThickness.Left <> 0 Then edgeGeometry.Children.Add(New LineGeometry(realCoordinates.TopLeft, realCoordinates.BottomLeft))
            If BorderThickness.Top <> 0 Then edgeGeometry.Children.Add(New LineGeometry(realCoordinates.TopLeft, realCoordinates.TopRight))
            If BorderThickness.Right <> 0 Then edgeGeometry.Children.Add(New LineGeometry(realCoordinates.TopRight, realCoordinates.BottomRight))
            If BorderThickness.Bottom <> 0 Then edgeGeometry.Children.Add(New LineGeometry(realCoordinates.BottomLeft, realCoordinates.BottomRight))


            Dim lightweightGeometry As GeometryGroup = Nothing

            _drawing = New DrawingGroup

            If Not IsSelected Or Mouse.LeftButton = MouseButtonState.Pressed Then
                'lightweightGeometry = New GeometryGroup
                'lightweightGeometry.Children.Add(baseRectGeometry)
                'lightweightGeometry.Children.Add(edgeGeometry)
                _drawing.Children.Add(New GeometryDrawing(Nothing, Pen, edgeGeometry))
                _drawing.Children.Add(New GeometryDrawing(Fill, New Pen(Brushes.Black, 0), baseRectGeometry))
            Else
                Dim brush As SolidColorBrush
                If Parent IsNot Nothing Then
                    brush = New SolidColorBrush(GetForegroundColor(CType(Parent.Background, SolidColorBrush).Color))
                Else
                    brush = Brushes.White
                End If
                _drawing.Children.Add(New GeometryDrawing(Nothing, Pen, edgeGeometry))
                _drawing.Children.Add(New GeometryDrawing(Fill, New Pen(Brushes.Black, 0), baseRectGeometry))
                If ShowSelectionHandles Then _drawing.Children.Add(New GeometryDrawing(brush, New Pen(brush, 1), GetDotsGeometryForLine(realCoordinates)))
            End If
            If DrawVisual Then
                If Parent.GetRelativeFromReal(realCoordinates).IntersectsWith(Parent.Bounds.ToWindowsRect) Then
                    Using dc As DrawingContext = RenderOpen()
                        dc.DrawDrawing(_drawing)
                    End Using
                    'If IsSelected And Mouse.LeftButton = MouseButtonState.Released Then
                    'Else
                    '    Using dc As DrawingContext = RenderOpen()
                    '        dc.DrawGeometry(Fill, Pen, lightweightGeometry)
                    '    End Using
                    'End If
                Else
                    Dim dc As DrawingContext = RenderOpen()
                    dc.Close()
                End If
            End If
        End If
    End Sub



    Protected Function GetDotsGeometryForLine(ByVal coordinates As Rect) As GeometryGroup
        Dim group As New GeometryGroup
        group.Children.Add(New EllipseGeometry(New Windows.Rect(coordinates.TopLeft.X - DragCircleSize / 2, coordinates.TopLeft.Y - DragCircleSize / 2, DragCircleSize, DragCircleSize)))
        group.Children.Add(New EllipseGeometry(New Windows.Rect(coordinates.TopRight.X - DragCircleSize / 2, coordinates.TopRight.Y - DragCircleSize / 2, DragCircleSize, DragCircleSize)))
        group.Children.Add(New EllipseGeometry(New Windows.Rect(coordinates.BottomLeft.X - DragCircleSize / 2, coordinates.BottomLeft.Y - DragCircleSize / 2, DragCircleSize, DragCircleSize)))
        group.Children.Add(New EllipseGeometry(New Windows.Rect(coordinates.BottomRight.X - DragCircleSize / 2, coordinates.BottomRight.Y - DragCircleSize / 2, DragCircleSize, DragCircleSize)))
        Return group
    End Function

    Protected Overrides Function PopupInformation() As System.Collections.Generic.List(Of System.Collections.Generic.KeyValuePair(Of String, String))
        Dim rect = GetSnappedCoordinates()
        Dim items As New List(Of KeyValuePair(Of String, String))
        items.Add(New KeyValuePair(Of String, String)("Price: ", FormatNumber(If(UseNegativeCoordinates, -1, 1) * rect.Y, Parent.Settings("DecimalPlaces").Value).Replace(",", "")))
        items.Add(New KeyValuePair(Of String, String)("Bar: ", rect.X))
        If HasParent AndAlso rect.X > 0 AndAlso Parent.bars.Count >= rect.X Then
            items.Add(New KeyValuePair(Of String, String)("Date: ", Parent.bars(rect.X - 1).Data.Date.ToShortDateString))
            items.Add(New KeyValuePair(Of String, String)("Time: ", Parent.bars(rect.X - 1).Data.Date.ToLongTimeString))
        Else
            items.Add(New KeyValuePair(Of String, String)("Date: ", "N/A"))
            items.Add(New KeyValuePair(Of String, String)("Time: ", "N/A"))
        End If
        items.Add(New KeyValuePair(Of String, String)("Bar Spread: ", rect.Width))
        items.Add(New KeyValuePair(Of String, String)("Price Spread: ", FormatNumber(rect.Height, Parent.Settings("DecimalPlaces").Value).Replace(",", "")))
        If AnalysisTechnique IsNot Nothing Then
            items.Add(New KeyValuePair(Of String, String)("Analysis Technique: ", AnalysisTechnique.Name))
        Else
            items.Add(New KeyValuePair(Of String, String)("Analysis Technique: ", "N/A"))
        End If
        Return items
    End Function

    Protected dragAreaClicked As Integer = 4
    Protected mouseOffset As Point
    Protected clickCoordinates As Rect
    Public Overrides Sub ParentMouseLeftButtonUp(e As System.Windows.Input.MouseButtonEventArgs, location As System.Windows.Point)
        MyBase.ParentMouseLeftButtonUp(e, location)
        RefreshVisual()
    End Sub
    Public Overrides Sub ParentMouseLeftButtonDown(ByVal e As MouseButtonEventArgs, ByVal location As System.Windows.Point)
        MyBase.ParentMouseLeftButtonDown(e, location)

        If UseNegativeCoordinates Then
            clickCoordinates = Parent.GetRealFromRelative(_relativeCoordinates)
            mouseOffset = location - clickCoordinates.Location
        Else
            clickCoordinates = Parent.GetRealFromRelative(NegateY(_relativeCoordinates))
            mouseOffset = location - Parent.GetRealFromRelative(NegateY(location))
        End If
        dragAreaClicked = -1
        mouseOffset = location - clickCoordinates.TopLeft
        If Distance(location, clickCoordinates.BottomRight) < DragCircleSize / 2 + ClickLeniency Then
            dragAreaClicked = 4
        ElseIf Distance(location, clickCoordinates.TopLeft) < DragCircleSize / 2 + ClickLeniency Then
            dragAreaClicked = 1
        ElseIf Distance(location, clickCoordinates.TopRight) < DragCircleSize / 2 + ClickLeniency Then
            dragAreaClicked = 2
        ElseIf Distance(location, clickCoordinates.BottomLeft) < DragCircleSize / 2 + ClickLeniency Then
            dragAreaClicked = 3
        Else
            If ResizeByEdges Then
                If (New LineGeometry(clickCoordinates.TopLeft, clickCoordinates.BottomLeft)).StrokeContains(Pen, location, ClickLeniency, ToleranceType.Absolute) Then
                    dragAreaClicked = 5
                ElseIf (New LineGeometry(clickCoordinates.TopLeft, clickCoordinates.TopRight)).StrokeContains(Pen, location, ClickLeniency, ToleranceType.Absolute) Then
                    dragAreaClicked = 6
                ElseIf (New LineGeometry(clickCoordinates.TopRight, clickCoordinates.BottomRight)).StrokeContains(Pen, location, ClickLeniency, ToleranceType.Absolute) Then
                    dragAreaClicked = 7
                ElseIf (New LineGeometry(clickCoordinates.BottomLeft, clickCoordinates.BottomRight)).StrokeContains(Pen, location, ClickLeniency, ToleranceType.Absolute) Then
                    dragAreaClicked = 8
                End If
            End If
        End If
        If dragAreaClicked = -1 Then
            If ContainsPoint(location) Then
                dragAreaClicked = 9
            Else
                dragAreaClicked = 0
            End If
        End If
        IsSelected = True
    End Sub
    Public Overrides Sub ParentMouseMove(ByVal e As MouseEventArgs, ByVal location As System.Windows.Point)
        If IsMouseDown Then
            Dim prevAutoRefresh As Boolean = AutoRefresh
            AutoRefresh = False

            Dim realCoordinates As Rect
            If UseNegativeCoordinates Then
                realCoordinates = Parent.GetRealFromRelative(_relativeCoordinates)
            Else
                realCoordinates = Parent.GetRealFromRelative(NegateY(_relativeCoordinates))
            End If

            Select Case dragAreaClicked
                Case 1 ' top left
                    If CanResize Then
                        Me.Location = If(UseNegativeCoordinates, Parent.GetRelativeFromReal(VectorToPoint(location - mouseOffset)), NegateY(Parent.GetRelativeFromReal(VectorToPoint(location - mouseOffset))))
                        If (clickCoordinates.Location.X - (location.X - mouseOffset.X)) + clickCoordinates.Width > 0 Then Me.Width = Parent.GetRelativeFromRealWidth((clickCoordinates.Location.X - (location.X - mouseOffset.X)) + clickCoordinates.Width) Else Me.Width = Parent.GetRelativeFromRealWidth(0)
                        If (clickCoordinates.Location.Y - (location.Y - mouseOffset.Y)) + clickCoordinates.Height > 0 Then Me.Height = Parent.GetRelativeFromRealHeight((clickCoordinates.Location.Y - (location.Y - mouseOffset.Y)) + clickCoordinates.Height) Else Me.Height = Parent.GetRelativeFromRealWidth(0)
                    End If
                Case 2 ' top right
                    If CanResize Then
                        Me.Location = Parent.GetRelativeFromReal(New Point(Parent.GetRealFromRelativeX(Me.Location.X), location.Y - mouseOffset.Y))
                        If UseNegativeCoordinates = False Then Me.Location = NegateY(Me.Location)
                        If (clickCoordinates.Location.Y - (location.Y - mouseOffset.Y)) + clickCoordinates.Height > 0 Then Me.Height = Parent.GetRelativeFromRealHeight((clickCoordinates.Location.Y - (location.Y - mouseOffset.Y)) + clickCoordinates.Height) Else Me.Height = Parent.GetRelativeFromRealWidth(0)
                        If ((location.X - mouseOffset.X) - clickCoordinates.Location.X) + clickCoordinates.Width > 0 Then Me.Width = Parent.GetRelativeFromRealWidth(((location.X - mouseOffset.X) - clickCoordinates.Location.X) + clickCoordinates.Width) Else Me.Width = Parent.GetRelativeFromRealWidth(0)
                    End If
                Case 3 ' bottom left
                    If CanResize Then
                        Me.Location = New Point(Parent.GetRelativeFromRealX(location.X - mouseOffset.X), Me.Location.Y)
                        If (clickCoordinates.Location.X - (location.X - mouseOffset.X)) + clickCoordinates.Width > 0 Then Me.Width = Parent.GetRelativeFromRealWidth((clickCoordinates.Location.X - (location.X - mouseOffset.X)) + clickCoordinates.Width) Else Me.Width = Parent.GetRelativeFromRealWidth(0)
                        If (location.Y - mouseOffset.Y) - (clickCoordinates.Location.Y) + clickCoordinates.Height > 0 Then Me.Height = Parent.GetRelativeFromRealHeight((location.Y - mouseOffset.Y) - (clickCoordinates.Location.Y) + clickCoordinates.Height) Else Me.Height = Parent.GetRelativeFromRealWidth(0)
                    End If
                Case 4 ' bottom right
                    If CanResize Then
                        If ((location.X - mouseOffset.X) - clickCoordinates.Location.X) + clickCoordinates.Width > 0 Then Me.Width = Parent.GetRelativeFromRealWidth(((location.X - mouseOffset.X) - clickCoordinates.Location.X) + clickCoordinates.Width) Else Me.Width = Parent.GetRelativeFromRealWidth(0)
                        If ((location.Y - mouseOffset.Y) - clickCoordinates.Location.Y) + clickCoordinates.Height > 0 Then Me.Height = Parent.GetRelativeFromRealHeight(((location.Y - mouseOffset.Y) - clickCoordinates.Location.Y) + clickCoordinates.Height) Else Me.Height = Parent.GetRelativeFromRealWidth(0)
                    End If
                Case 5 ' left
                    If CanResize Then
                        Me.Location = New Point(Parent.GetRelativeFromRealX(location.X - mouseOffset.X), Me.Location.Y)
                        If (clickCoordinates.Location.X - (location.X - mouseOffset.X)) + clickCoordinates.Width > 0 Then Me.Width = Parent.GetRelativeFromRealWidth((clickCoordinates.Location.X - (location.X - mouseOffset.X)) + clickCoordinates.Width) Else Me.Width = Parent.GetRelativeFromRealWidth(0)
                    End If
                Case 6 ' top
                    If CanResize Then
                        Me.Location = Parent.GetRelativeFromReal(New Point(Parent.GetRealFromRelativeX(Me.Location.X), location.Y - mouseOffset.Y))
                        If UseNegativeCoordinates = False Then Me.Location = NegateY(Me.Location)
                        If (clickCoordinates.Location.Y - (location.Y - mouseOffset.Y)) + clickCoordinates.Height > 0 Then Me.Height = Parent.GetRelativeFromRealHeight((clickCoordinates.Location.Y - (location.Y - mouseOffset.Y)) + clickCoordinates.Height) Else Me.Height = Parent.GetRelativeFromRealWidth(0)
                    End If
                Case 7 ' right
                    If CanResize Then
                        If ((location.X - mouseOffset.X) - clickCoordinates.Location.X) + clickCoordinates.Width > 0 Then Me.Width = Parent.GetRelativeFromRealWidth(((location.X - mouseOffset.X) - clickCoordinates.Location.X) + clickCoordinates.Width) Else Me.Width = Parent.GetRelativeFromRealWidth(0)
                    End If
                Case 8 ' bottom
                    If CanResize Then
                        If ((location.Y - mouseOffset.Y) - clickCoordinates.Location.Y) + clickCoordinates.Height > 0 Then Me.Height = Parent.GetRelativeFromRealHeight(((location.Y - mouseOffset.Y) - clickCoordinates.Location.Y) + clickCoordinates.Height) Else Me.Height = Parent.GetRelativeFromRealWidth(0)
                    End If
                Case 9
                    If LockPositionOrientation = LockPositionOrientationTypes.Horizontally Then
                        Me.Location = New Point(Parent.GetRelativeFromReal(VectorToPoint(location - mouseOffset)).X, Me.Location.Y)
                    ElseIf LockPositionOrientation = LockPositionOrientationTypes.Vertically Then
                        Dim desiredPos = If(UseNegativeCoordinates = False, -1, 1) * Parent.GetRelativeFromReal(VectorToPoint(location - mouseOffset)).Y
                        If Round(desiredPos, 5) - Height > MaxTopValue Then desiredPos = MaxTopValue + Height
                        If Round(desiredPos, 5) < MaxBottomValue Then desiredPos = MaxBottomValue
                        Me.Location = New Point(Me.Location.X, desiredPos)
                    Else
                        Me.Location = If(Not UseNegativeCoordinates, NegateY(Parent.GetRelativeFromReal(VectorToPoint(location - mouseOffset))), Parent.GetRelativeFromReal(VectorToPoint(location - mouseOffset)))
                    End If
            End Select
            If dragAreaClicked > 0 And dragAreaClicked < 10 Then
                RaiseEvent ManuallyMoved(Me, dragAreaClicked, location)
            End If
            Coordinates = GetSnappedCoordinates()
            AutoRefresh = prevAutoRefresh
            RefreshVisual()
        End If

        MyBase.ParentMouseMove(e, location)
    End Sub
    Public Event ManuallyMoved(sender As Object, index As Integer, location As Point)
    Public Overrides Sub ParentMouseRightButtonDown(ByVal e As System.Windows.Input.MouseButtonEventArgs, ByVal location As System.Windows.Point)
        MyBase.ParentMouseRightButtonDown(e, location)
    End Sub
    Public Overrides Sub ParentMouseRightButtonUp(ByVal e As System.Windows.Input.MouseButtonEventArgs, ByVal location As System.Windows.Point)
        MyBase.ParentMouseRightButtonUp(e, location)
        If IsEditable Then
            Dim formatWin As New FormatRectangleWindow(Me)
        End If
    End Sub

    Friend Overrides Sub Command_Executed(ByVal sender As Object, ByVal e As System.Windows.Input.ExecutedRoutedEventArgs)
        If e.Command Is ChartCommands.RemoveObject Then
            Parent.Children.Remove(Me)
        Else
            MyBase.Command_Executed(sender, e)
        End If
    End Sub

    Public Overrides Function ContainsPoint(ByVal point As System.Windows.Point) As Boolean
        If TypeOf Fill Is SolidColorBrush AndAlso CType(Fill, SolidColorBrush).Color.A = 0 Then
            Return baseRectGeometry.StrokeContains(Pen, point, ClickLeniency, ToleranceType.Absolute)
        Else
            Return baseRectGeometry.FillContains(point, ClickLeniency, ToleranceType.Absolute)
        End If
    End Function

    Public Sub Deserialize(ByVal serializedString As String) Implements ISerializable.Deserialize
        Try
            Dim v() As String = Split(serializedString, "/")
            Pen = PenFromString(v(0))
            Fill = (New BrushConverter).ConvertFromString(v(1))
            Location = (New PointConverter).ConvertFromString(v(2))
            Width = v(3)
            Height = v(4)
            AutoRefresh = v(5)
            If (New Date(CLng(v(6)))).Ticks <> 0 And Parent IsNot Nothing Then Location = New Point(Parent.GetBarNumberFromDate(New Date(v(6))), Location.Y)
        Catch ex As Exception
            Throw New ArgumentException("'Rectangle.Deserialize' got an error parsing the string '" & serializedString & "'.")
        End Try
    End Sub
    Public Function Serialize() As String Implements ISerializable.Serialize
        Dim startDate As New Date(0)
        Dim endDate As New Date(0)
        If Parent IsNot Nothing Then
            startDate = Parent.GetDateFromBarNumber(Location.X)
        End If
        Return Join({
                    PenToString(Pen),
                    (New BrushConverter()).ConvertToString(Fill),
                    (New PointConverter).ConvertToString(Location),
                    Width,
                    Height,
                    AutoRefresh,
                    startDate.Ticks
                }, "/")
    End Function
End Class


