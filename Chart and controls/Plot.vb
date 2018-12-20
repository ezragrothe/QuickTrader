Public Class Plot
    Inherits ChartDrawingVisual
    Implements ISelectable

    Public Sub New()
        Pen = New Pen(Brushes.Red, 1)
    End Sub

    Private _drawing As DrawingGroup
    Private currentGeometry As GeometryGroup
    Protected Overrides Property Drawing As System.Windows.Media.DrawingGroup
        Get
            Return _drawing
        End Get
        Set(ByVal value As System.Windows.Media.DrawingGroup)
            _drawing = value
        End Set
    End Property
    Public Property IsSelectable As Boolean = True Implements ISelectable.IsSelectable
    Private _isSelected As Boolean
    Public Property IsSelected As Boolean Implements ISelectable.IsSelected
        Get
            Return _isSelected
        End Get
        Set(ByVal value As Boolean)
            _isSelected = value
            RefreshVisual()
        End Set
    End Property

    Private _points As New List(Of Point)
    Public Property Points As List(Of Point)
        Get
            Return _points
        End Get
        Set(value As List(Of Point))
            _points = value
            If AutoRefresh Then RefreshVisual()
        End Set
    End Property
    Public Property NonContinuousPoints As List(Of Point)
        Get
            Return _nonContinuousPoints
        End Get
        Set(value As List(Of Point))
            _nonContinuousPoints = value
        End Set
    End Property
    Private _nonContinuousPoints As New List(Of Point)
    Public Property PointCircleSize As Double = 3
    Public Overrides Sub ParentMouseLeftButtonDown(e As System.Windows.Input.MouseButtonEventArgs, location As System.Windows.Point)
        MyBase.ParentMouseLeftButtonDown(e, location)
        IsSelected = True
    End Sub
    Public Property IsLinePlot As Boolean = True

    Public Overrides Sub RefreshVisual()
        If Parent Is Nothing Then Exit Sub
        Dim geo As New GeometryGroup
        Dim pointsGeo As New GeometryGroup
        If Points.Count > 0 Then
            pointsGeo.Children.Add(New EllipseGeometry(New Rect(Parent.GetRealFromRelativeX(Points(0).X) - PointCircleSize / 2, Parent.GetRealFromRelativeY(If(UseNegativeCoordinates, 1, -1) * Points(0).Y) - PointCircleSize / 2, PointCircleSize, PointCircleSize)))
        End If
        For i = 1 To Points.Count - 1
            If nonContinuousPoints.Contains(Points(i)) Then Continue For
            Dim point1 As Point = Points(i - 1)
            Dim point2 As Point = Points(i)
            If Not UseNegativeCoordinates Then
                point1 = NegateY(point1)
                point2 = NegateY(point2)
            End If
            point1 = Parent.GetRealFromRelative(point1)
            point2 = Parent.GetRealFromRelative(point2)
            If IsLinePlot Then
                geo.Children.Add(New LineGeometry(point1, point2))
                If IsSelected Then
                    pointsGeo.Children.Add(New EllipseGeometry(New Rect(point2.X - PointCircleSize / 2, point2.Y - PointCircleSize / 2, PointCircleSize, PointCircleSize)))
                End If
            Else
                If IsSelected Then
                    pointsGeo.Children.Add(New EllipseGeometry(New Rect(point2.X - (PointCircleSize + 2) / 2, point2.Y - (PointCircleSize + 2) / 2, (PointCircleSize + 2), (PointCircleSize + 2))))
                Else
                    pointsGeo.Children.Add(New EllipseGeometry(New Rect(point2.X - PointCircleSize / 2, point2.Y - PointCircleSize / 2, PointCircleSize, PointCircleSize)))
                End If
            End If
        Next
        _drawing = New DrawingGroup
        If IsLinePlot Then _drawing.Children.Add(New GeometryDrawing(Pen.Brush, Pen, geo))
        If IsSelected Or Not IsLinePlot Then _drawing.Children.Add(New GeometryDrawing(Pen.Brush, Pen, pointsGeo))
        currentGeometry = New GeometryGroup
        If IsLinePlot Then currentGeometry.Children.Add(geo)
        If IsSelected Or Not IsLinePlot Then currentGeometry.Children.Add(pointsGeo)
        MyBase.RefreshVisual()
    End Sub
    Public Sub AddPoint(point As Point, Optional forceRefresh As Boolean = False)
        Points.Add(point)
        If forceRefresh Or AutoRefresh Then RefreshVisual()
    End Sub
    Public Sub AddNonContinuousPoint(point As Point)
        Points.Add(point)
        nonContinuousPoints.Add(point)
    End Sub
    Public Overrides Function ContainsPoint(point As System.Windows.Point) As Boolean
        Return currentGeometry.FillContains(point, ClickLeniency, ToleranceType.Absolute)
    End Function

    Protected Overrides Function PopupInformation() As System.Collections.Generic.List(Of System.Collections.Generic.KeyValuePair(Of String, String))
        Dim items As New List(Of KeyValuePair(Of String, String))
        items.Add(New KeyValuePair(Of String, String)("Number of Points: ", Points.Count))
        If AnalysisTechnique IsNot Nothing Then
            items.Add(New KeyValuePair(Of String, String)("Analysis Technique: ", AnalysisTechnique.Name))
        Else
            items.Add(New KeyValuePair(Of String, String)("Analysis Technique: ", "N/A"))
        End If
        Return items
    End Function

End Class
