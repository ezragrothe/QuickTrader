Imports System.Windows.Media
Imports System.Math
Public Class TrendLine
    Inherits ChartDrawingVisual
    Implements ISelectable, ISerializable

    'Protected chart As Chart
    'Public Sub New(ByVal chart As Chart)
    '    Me.chart = chart
    'End Sub
    Public Sub New()
    End Sub

    Protected baseLineGeometry As New LineGeometry
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

    Protected _relativeCoordinates As New LineCoordinates

    Protected Function GetSnappedCoordinates() As LineCoordinates
        Dim coordinates As LineCoordinates
        If SnapToRelativePixels Then
            coordinates.StartPoint = New Point(Round(StartPoint.X), StartPoint.Y)
        Else
            coordinates.StartPoint = _relativeCoordinates.StartPoint
        End If
        If SnapToRelativePixels Then
            coordinates.EndPoint = New Point(Round(EndPoint.X), EndPoint.Y)
        Else
            coordinates.EndPoint = _relativeCoordinates.EndPoint
        End If
        Return coordinates
    End Function

    Public Property StartPoint As Point
        Get
            Return _relativeCoordinates.StartPoint
        End Get
        Set(ByVal value As Point)
            _relativeCoordinates.StartPoint = value
            If AutoRefresh Then RefreshVisual()
        End Set
    End Property
    Public Property EndPoint As Point
        Get
            Return _relativeCoordinates.EndPoint
        End Get
        Set(ByVal value As Point)
            _relativeCoordinates.EndPoint = value
            If AutoRefresh Then RefreshVisual()
        End Set
    End Property
    Public Property Coordinates As LineCoordinates
        Get
            Return New LineCoordinates(StartPoint, EndPoint)
        End Get
        Set(ByVal value As LineCoordinates)
            StartPoint = value.StartPoint
            EndPoint = value.EndPoint
        End Set
    End Property
    Private _useFixedStartRegressionFormula As Boolean
    Public Property UseFixedStartRegressionFormula As Boolean
        Get
            Return _useFixedStartRegressionFormula
        End Get
        Set(value As Boolean)
            _useFixedStartRegressionFormula = value
            If AutoRefresh Then RefreshVisual()
        End Set
    End Property
    Private _isRegressionLine As Boolean
    Public Property IsRegressionLine As Boolean
        Get
            Return _isRegressionLine
        End Get
        Set(value As Boolean)
            _isRegressionLine = value
            If AutoRefresh Then RefreshVisual()
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

    Protected _parallelDistance As Double
    Public Property ParallelDistance As Double
        Get
            Return _parallelDistance
        End Get
        Set(ByVal value As Double)
            _parallelDistance = value
            If AutoRefresh Then RefreshVisual()
        End Set
    End Property
    Protected _hasParallel As Boolean
    Public Property HasParallel As Boolean
        Get
            Return _hasParallel
        End Get
        Set(ByVal value As Boolean)
            _hasParallel = value
            If AutoRefresh Then RefreshVisual()
        End Set
    End Property

    Public Property DrawZoneFill As Boolean = False
    Public Property UpZoneFillBrush As Brush = Brushes.Green
    Public Property DownZoneFillBrush As Brush = Brushes.Red
    Public Property UpNeutralZoneFillBrush As Brush = Brushes.Red
    Public Property DownNeutralZoneFillBrush As Brush = Brushes.Red
    Public Property NeutralZoneBarStart As Integer
    Public Property ConfirmedZoneBarStart As Integer

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
    Protected _outerpen As Pen = New Pen(Nothing, 1)
    Public Property OuterPen As System.Windows.Media.Pen
        Get
            Return _outerpen
        End Get
        Set(ByVal value As System.Windows.Media.Pen)
            _outerpen = value
            If AutoRefresh Then RefreshVisual()
        End Set
    End Property
    Protected _lockToEnd As Boolean
    Public Property LockToEnd As Boolean
        Get
            Return _lockToEnd
        End Get
        Set(value As Boolean)
            _lockToEnd = value
            If AutoRefresh Then RefreshVisual()
        End Set
    End Property

    Protected _extendRight As Boolean
    Public Property ExtendRight As Boolean
        Get
            Return _extendRight
        End Get
        Set(ByVal value As Boolean)
            _extendRight = value
            If AutoRefresh Then RefreshVisual()
        End Set
    End Property
    Protected _extendLeft As Boolean
    Public Property ExtendLeft As Boolean
        Get
            Return _extendLeft
        End Get
        Set(ByVal value As Boolean)
            _extendLeft = value
            If AutoRefresh Then RefreshVisual()
        End Set
    End Property

    ''' <summary>
    ''' FOR USE ONLY WITH REGRESSION PARALLEL CHANNELS IN SPECIAL SITUATIONS
    ''' </summary>
    Public Property TopLineEndPoint As Double? = Nothing
    ''' <summary>
    ''' FOR USE ONLY WITH REGRESSION PARALLEL CHANNELS IN SPECIAL SITUATIONS
    ''' </summary>
    Public Property BottomLineEndPoint As Double? = Nothing

    Public Property DragCircleSize As Double = 6

    Public Overrides ReadOnly Property Parent As Chart
        Get
            Return MyBase.Parent
        End Get
    End Property

    Public Overrides Sub RefreshVisual()
        If IsRegressionLine Then
            If HasParent Then
                Dim startBar As Integer = 1
                Dim endBar As Integer = 1
                Dim dontDraw As Boolean = False
                If _relativeCoordinates.LeftX > 1 And _relativeCoordinates.LeftX <= Parent.bars.Count And _relativeCoordinates.RightX > 1 And _relativeCoordinates.RightX <= Parent.bars.Count Then
                    startBar = _relativeCoordinates.LeftX
                    endBar = _relativeCoordinates.RightX
                ElseIf _relativeCoordinates.LeftX > 1 And _relativeCoordinates.LeftX <= Parent.bars.Count And _relativeCoordinates.RightX > Parent.bars.Count Then
                    startBar = _relativeCoordinates.LeftX
                    endBar = Parent.bars.Count
                ElseIf _relativeCoordinates.LeftX <= 1 And _relativeCoordinates.RightX <= Parent.bars.Count And _relativeCoordinates.RightX > 1 Then
                    startBar = 2
                    endBar = _relativeCoordinates.RightX
                ElseIf _relativeCoordinates.LeftX > Parent.bars.Count Or _relativeCoordinates.RightX <= 1 Then
                    dontDraw = True
                End If
                If LockToEnd Then
                    _relativeCoordinates = New LineCoordinates(_relativeCoordinates.LeftX, 0, Parent.bars.Count, 0)
                    startBar = Max(_relativeCoordinates.LeftX, 2)
                    endBar = Parent.bars.Count
                    If _relativeCoordinates.LeftX > Parent.bars.Count Or _relativeCoordinates.RightX <= 1 Then dontDraw = True Else dontDraw = False
                End If
                If dontDraw = False Then
                    Dim middleLine As LineCoordinates
                    If UseFixedStartRegressionFormula Then
                        middleLine = CalculateNewStartPointFixedRegression(startBar - 1, _relativeCoordinates.StartPoint.X, _relativeCoordinates.EndPoint.X, endBar - 1)
                    Else
                        middleLine = CalculateNewStartPointRegressionMiddle(startBar - 1, _relativeCoordinates.StartPoint.X, _relativeCoordinates.EndPoint.X, endBar - 1)
                    End If
                    'If middleLine.StartPoint.X < middleLine.EndPoint.X Then
                    '    Dim startDif As Decimal
                    '    If middleLine.StartPoint.Y > Parent.bars(startBar - 1).Data.Low Then
                    '        startDif = middleLine.StartPoint.Y - Parent.bars(startBar - 1).Data.Low
                    '    Else
                    '        startDif = middleLine.StartPoint.Y - Parent.bars(startBar - 1).Data.High
                    '    End If
                    '    middleLine = New LineCoordinates(New Point(middleLine.StartPoint.X, middleLine.StartPoint.Y - startDif), New Point(middleLine.EndPoint.X, middleLine.EndPoint.Y + startDif)) 'Parent.bars(middleLine.StartPoint.X - 1).Data.High) ' middleLine.StartPoint.Y - startDif)
                    'Else
                    '    Dim startDif As Decimal
                    '    If middleLine.EndPoint.Y > Parent.bars(startBar - 1).Data.Low Then
                    '        startDif = middleLine.EndPoint.Y - Parent.bars(startBar - 1).Data.Low
                    '    Else
                    '        startDif = middleLine.EndPoint.Y - Parent.bars(startBar - 1).Data.High
                    '    End If
                    '    middleLine = New LineCoordinates(New Point(middleLine.StartPoint.X, middleLine.StartPoint.Y + startDif), New Point(middleLine.EndPoint.X, middleLine.EndPoint.Y - startDif)) 'Parent.bars(middleLine.StartPoint.X - 1).Data.High) ' middleLine.StartPoint.Y - startDif)
                    'End If

                    If middleLine.StartPoint.X <> middleLine.EndPoint.X Then ' if the swing is not 1 bar width wide
                        If UseNegativeCoordinates Then
                            _relativeCoordinates = NegateY(middleLine)
                        Else
                            _relativeCoordinates = middleLine
                        End If
                    End If
                    Dim realLineCoordinates As LineCoordinates
                    If UseNegativeCoordinates Then
                        realLineCoordinates = Parent.GetRealFromRelative(GetSnappedCoordinates)
                    Else
                        realLineCoordinates = Parent.GetRealFromRelative(NegateY(GetSnappedCoordinates))
                    End If

                    Dim realCoordinates As LineCoordinates = realLineCoordinates

                    If ExtendRight Then realLineCoordinates = GetExtendedRightCoordinates(realLineCoordinates)
                    If ExtendLeft Then realLineCoordinates = GetExtendedLeftCoordinates(realLineCoordinates)
                    baseLineGeometry = New LineGeometry(realLineCoordinates.StartPoint, realLineCoordinates.EndPoint)

                    Dim lightweightGeometry As GeometryGroup = Nothing

                    _drawing = New DrawingGroup
                    _drawing.Children.Add(New GeometryDrawing(Pen.Brush, Pen, baseLineGeometry))
                    Dim brush As SolidColorBrush
                    If Parent IsNot Nothing Then
                        brush = New SolidColorBrush(GetForegroundColor(CType(Parent.Background, SolidColorBrush).Color))
                    Else
                        brush = Brushes.White
                    End If
                    If IsSelected And Mouse.LeftButton = MouseButtonState.Released Then _drawing.Children.Add(New GeometryDrawing(brush, New Pen(brush, 1), GetDotsGeometryForLine(realLineCoordinates, realCoordinates, True)))
                    If HasParallel Then
                        Dim highBar As New Point, lowBar As New Point
                        Dim maxAboveDistance As Double, maxBelowDistance As Double
                        Dim direction As Direction = If((Parent.bars(endBar - 1).Data.High - Parent.bars(endBar - 1).Data.Low) / 2 + Parent.bars(endBar - 1).Data.High > (Parent.bars(startBar - 1).Data.High - Parent.bars(startBar - 1).Data.Low) / 2 + Parent.bars(startBar - 1).Data.Low, True, False)
                        For i = startBar - 1 To endBar - 1
                            Dim aboveDistance = Sin(Atan((Abs(middleLine.EndPoint.X - middleLine.StartPoint.X)) / Abs(middleLine.StartPoint.Y - middleLine.EndPoint.Y))) * (Parent.bars(i).Data.High - LinCalc(middleLine.StartPoint.X, middleLine.StartPoint.Y, middleLine.EndPoint.X, middleLine.EndPoint.Y, i + 1))
                            Dim belowDistance = Sin(Atan((Abs(middleLine.EndPoint.X - middleLine.StartPoint.X)) / Abs(middleLine.StartPoint.Y - middleLine.EndPoint.Y))) * (LinCalc(middleLine.StartPoint.X, middleLine.StartPoint.Y, middleLine.EndPoint.X, middleLine.EndPoint.Y, i + 1) - Parent.bars(i).Data.Low)
                            If aboveDistance >= maxAboveDistance Then
                                If (direction = Direction.Down And Parent.bars(i - 1).Data.High < Parent.bars(i).Data.High) Or direction = Direction.Up Then
                                    maxAboveDistance = aboveDistance
                                    highBar = New Point(i + 1, Parent.bars(i).Data.High)
                                End If
                            End If
                            If belowDistance >= maxBelowDistance Then
                                If (direction = Direction.Up And Parent.bars(i - 1).Data.Low > Parent.bars(i).Data.Low) Or direction = Direction.Down Then
                                    maxBelowDistance = belowDistance
                                    lowBar = New Point(i + 1, Parent.bars(i).Data.Low)
                                End If
                            End If
                        Next
                        Dim rangeBar As Point = If(direction = Direction.Down, highBar, lowBar)
                        If rangeBar.Y <> 0 Then 'if it is not a perfect swing with no retracements
                            Dim upperDistance As Decimal = Abs(highBar.Y - LinCalc(middleLine.StartPoint.X, middleLine.StartPoint.Y, middleLine.EndPoint.X, middleLine.EndPoint.Y, highBar.X))
                            Dim lowerDistance As Decimal = Abs(lowBar.Y - LinCalc(middleLine.StartPoint.X, middleLine.StartPoint.Y, middleLine.EndPoint.X, middleLine.EndPoint.Y, lowBar.X))
                            'If direction = direction.Down Then minDistance = -minDistance
                            upperDistance = Min(upperDistance, lowerDistance)
                            lowerDistance = Min(upperDistance, lowerDistance)
                            Dim parallelLine0 As New LineCoordinates, parallelLine1 As New LineCoordinates
                            parallelLine0.StartPoint = New Point(startBar, LinCalc(middleLine, startBar) - lowerDistance)
                            parallelLine0.EndPoint = New Point(If(TopLineEndPoint.HasValue, TopLineEndPoint, endBar), LinCalc(middleLine, endBar) - lowerDistance)
                            parallelLine1.StartPoint = New Point(startBar, LinCalc(middleLine, startBar) + upperDistance)
                            parallelLine1.EndPoint = New Point(If(BottomLineEndPoint.HasValue, BottomLineEndPoint, endBar), LinCalc(middleLine, endBar) + upperDistance)
                            'If Not UseNegativeCoordinates Then
                            parallelLine0 = NegateY(parallelLine0)
                            parallelLine1 = NegateY(parallelLine1)
                            'End If
                            If SnapToRelativePixels Then
                                parallelLine0.StartPoint = New Point(Round(parallelLine0.StartPoint.X), parallelLine0.StartPoint.Y)
                                parallelLine0.EndPoint = New Point(Round(parallelLine0.EndPoint.X), parallelLine0.EndPoint.Y)
                                parallelLine1.StartPoint = New Point(Round(parallelLine1.StartPoint.X), parallelLine1.StartPoint.Y)
                                parallelLine1.EndPoint = New Point(Round(parallelLine1.EndPoint.X), parallelLine1.EndPoint.Y)
                            End If
                            parallelLine0 = Parent.GetRealFromRelative(parallelLine0)
                            parallelLine1 = Parent.GetRealFromRelative(parallelLine1)
                            Dim pre0 = parallelLine0, pre1 = parallelLine1
                            If ExtendRight Then
                                parallelLine0 = GetExtendedRightCoordinates(parallelLine0)
                                parallelLine1 = GetExtendedRightCoordinates(parallelLine1)
                            End If
                            If ExtendLeft Then
                                parallelLine0 = GetExtendedLeftCoordinates(parallelLine0)
                                parallelLine1 = GetExtendedLeftCoordinates(parallelLine1)
                            End If
                            parallelLineGeometry = New GeometryGroup
                            parallelLineGeometry.Children.Add(New LineGeometry(parallelLine0.StartPoint, parallelLine0.EndPoint))
                            parallelLineGeometry.Children.Add(New LineGeometry(parallelLine1.StartPoint, parallelLine1.EndPoint))
                            _drawing.Children.Add(New GeometryDrawing(OuterPen.Brush, OuterPen, parallelLineGeometry))
                            If IsSelected And Mouse.LeftButton = MouseButtonState.Released Then _drawing.Children.Add(New GeometryDrawing(brush, New Pen(brush, 1), New GeometryGroup() With {.Children = New GeometryCollection({GetDotsGeometryForLine(pre0, pre0, True), GetDotsGeometryForLine(pre1, pre1, True)})}))
                        End If
                    End If
                    If DrawZoneFill Then
                        Dim geom = GetRegressionFillLinesGeometry()
                        _drawing.Children.Add(New GeometryDrawing(UpZoneFillBrush, New Pen(UpZoneFillBrush, 0), geom(0)))
                        _drawing.Children.Add(New GeometryDrawing(DownZoneFillBrush, New Pen(DownZoneFillBrush, 0), geom(1)))
                        _drawing.Children.Add(New GeometryDrawing(UpNeutralZoneFillBrush, New Pen(UpNeutralZoneFillBrush, 0), geom(2)))
                        _drawing.Children.Add(New GeometryDrawing(DownNeutralZoneFillBrush, New Pen(DownNeutralZoneFillBrush, 0), geom(3)))
                    End If
                    If DrawVisual Then
                            If Parent.GetRelativeFromReal(realLineCoordinates).ToBounds.ToWindowsRect.IntersectsWith(Parent.Bounds.ToWindowsRect) Then
                                Using dc As DrawingContext = RenderOpen()
                                    dc.DrawDrawing(_drawing)
                                End Using
                            Else
                                Dim dc As DrawingContext = RenderOpen()
                                dc.Close()
                            End If
                        End If
                    Else
                        'this is if the line is outside the bars
                    End If
            End If
        Else
            If HasParent Then
                Dim realLineCoordinates As LineCoordinates
                If UseNegativeCoordinates Then
                    realLineCoordinates = Parent.GetRealFromRelative(GetSnappedCoordinates)
                Else
                    realLineCoordinates = Parent.GetRealFromRelative(NegateY(GetSnappedCoordinates))
                End If

                Dim realCoordinates As LineCoordinates = realLineCoordinates

                If ExtendRight Then realLineCoordinates = GetExtendedRightCoordinates(realLineCoordinates)
                If ExtendLeft Then realLineCoordinates = GetExtendedLeftCoordinates(realLineCoordinates)
                baseLineGeometry = New LineGeometry(realLineCoordinates.StartPoint, realLineCoordinates.EndPoint)

                Dim realParallelDistance As Double = Parent.GetRealFromRelativeHeight(ParallelDistance)
                Dim realParallelLineCoordinates As New LineCoordinates(New Point(baseLineGeometry.StartPoint.X, baseLineGeometry.StartPoint.Y + BooleanToInteger(UseNegativeCoordinates) * realParallelDistance), New Point(baseLineGeometry.EndPoint.X, baseLineGeometry.EndPoint.Y + BooleanToInteger(UseNegativeCoordinates) * realParallelDistance))
                If ExtendRight Then realParallelLineCoordinates = GetExtendedRightCoordinates(realParallelLineCoordinates)
                If ExtendLeft Then realParallelLineCoordinates = GetExtendedLeftCoordinates(realParallelLineCoordinates)
                If HasParallel Then
                    parallelLineGeometry = New GeometryGroup
                    parallelLineGeometry.Children.Add(New LineGeometry(realParallelLineCoordinates.StartPoint, realParallelLineCoordinates.EndPoint))
                End If
                Dim lightweightGeometry As GeometryGroup = Nothing

                If Not IsSelected Or Mouse.LeftButton = MouseButtonState.Pressed Then
                    lightweightGeometry = New GeometryGroup
                    lightweightGeometry.Children.Add(baseLineGeometry)
                    If HasParallel Then lightweightGeometry.Children.Add(parallelLineGeometry)
                Else
                    _drawing = New DrawingGroup
                    Dim brush As SolidColorBrush
                    If Parent IsNot Nothing Then
                        brush = New SolidColorBrush(GetForegroundColor(CType(Parent.Background, SolidColorBrush).Color))
                    Else
                        brush = Brushes.White
                    End If
                    _drawing.Children.Add(New GeometryDrawing(Pen.Brush, Pen, baseLineGeometry))
                    _drawing.Children.Add(New GeometryDrawing(brush, New Pen(brush, 1), GetDotsGeometryForLine(realLineCoordinates, realCoordinates, True)))
                    If HasParallel Then
                        _drawing.Children.Add(New GeometryDrawing(Pen.Brush, Pen, parallelLineGeometry))
                        _drawing.Children.Add(New GeometryDrawing(brush, New Pen(brush, 1), GetDotsGeometryForLine(realParallelLineCoordinates, New LineCoordinates(New Point(realCoordinates.StartPoint.X, realCoordinates.StartPoint.Y + BooleanToInteger(UseNegativeCoordinates) * realParallelDistance), New Point(realCoordinates.EndPoint.X, realCoordinates.EndPoint.Y + BooleanToInteger(UseNegativeCoordinates) * realParallelDistance)), False)))
                    End If
                End If
                If DrawZoneFill Then
                    Dim geom = GetRegressionFillLinesGeometry()
                    _drawing.Children.Add(New GeometryDrawing(UpZoneFillBrush, New Pen(UpZoneFillBrush, 0), geom(0)))
                    _drawing.Children.Add(New GeometryDrawing(DownZoneFillBrush, New Pen(DownZoneFillBrush, 0), geom(1)))
                    _drawing.Children.Add(New GeometryDrawing(UpNeutralZoneFillBrush, New Pen(UpNeutralZoneFillBrush, 0), geom(2)))
                    _drawing.Children.Add(New GeometryDrawing(DownNeutralZoneFillBrush, New Pen(DownNeutralZoneFillBrush, 0), geom(3)))
                End If
                If DrawVisual Then
                    If Parent.GetRelativeFromReal(realLineCoordinates).ToBounds.ToWindowsRect.IntersectsWith(Parent.Bounds.ToWindowsRect) Then
                        If IsSelected And Mouse.LeftButton = MouseButtonState.Released Then
                            Using dc As DrawingContext = RenderOpen()
                                dc.DrawDrawing(_drawing)
                            End Using
                        Else
                            Using dc As DrawingContext = RenderOpen()
                                dc.DrawGeometry(Pen.Brush, Pen, lightweightGeometry)
                            End Using
                        End If
                    Else
                        Dim dc As DrawingContext = RenderOpen()
                        dc.Close()
                    End If
                End If
            End If
        End If
    End Sub

    Protected Function GetRegressionFillLinesGeometry() As Geometry()
        Dim realLineCoordinates As LineCoordinates
        Dim c = GetSnappedCoordinates()
        Dim negated As LineCoordinates = c
        If Not UseNegativeCoordinates Then
            c = NegateY(c)
        Else
            negated = NegateY(c)
        End If
        realLineCoordinates = Parent.GetRealFromRelative(c)
        Dim upGeom As New PathGeometry
        Dim downGeom As New PathGeometry
        Dim upNeutralGeom As New PathGeometry
        Dim downNeutralGeom As New PathGeometry
        Dim upFigures As New PathFigureCollection
        Dim downFigures As New PathFigureCollection
        Dim upNeutralFigures As New PathFigureCollection
        Dim downNeutralFigures As New PathFigureCollection
        Dim currentFigure As PathFigure = Nothing
        Dim mode As Direction = Direction.Up
        Dim isActive As Boolean = False
        Dim startvalue = Max(Min(StartPoint.X - 1, EndPoint.X - 1), 0)
        Dim endvalue = Min(Max(StartPoint.X - 1, EndPoint.X - 1), Parent.bars.Count - 1)
        Dim startBar As Integer
        For i = startvalue To endvalue
            Dim p As Double = LinCalc(negated, i + 1)
            If isActive Then
                'if current section continues
                If (mode = Direction.Up And Parent.bars(i).Data.Close > p) Then
                    currentFigure.Segments.Add(New LineSegment(Parent.GetRealFromRelative(New Point(i + 1, -Parent.bars(i).Data.Close)), True)) 'low
                    currentFigure.Segments.Add(New LineSegment(Parent.GetRealFromRelative(New Point(i + 2, -Parent.bars(i).Data.Close)), True))
                ElseIf (mode = Direction.Down And Parent.bars(i).Data.close < p) Then
                    currentFigure.Segments.Add(New LineSegment(Parent.GetRealFromRelative(New Point(i + 1, -Parent.bars(i).Data.Close)), True))
                    currentFigure.Segments.Add(New LineSegment(Parent.GetRealFromRelative(New Point(i + 2, -Parent.bars(i).Data.Close)), True))
                End If
                If (mode = Direction.Down And Parent.bars(i).Data.Close > p) Or
                   (mode = Direction.Up And Parent.bars(i).Data.Close < p) Or i = NeutralZoneBarStart Then ' if the current color area is finished
                    currentFigure.Segments.Add(New LineSegment(Parent.GetRealFromRelative(New Point(i + 1, -p)), True))
                    If startBar >= NeutralZoneBarStart Then
                        If mode = Direction.Down Then downNeutralFigures.Add(currentFigure) Else upNeutralFigures.Add(currentFigure)
                    Else
                        If mode = Direction.Down Then downFigures.Add(currentFigure) Else upFigures.Add(currentFigure)
                    End If
                    isActive = False
                End If
            End If
            If Not isActive Then
                If i >= ConfirmedZoneBarStart Or i >= NeutralZoneBarStart Then
                    If Parent.bars(i).Data.Close < p Then 'if new below zone
                        currentFigure = New PathFigure With {.IsClosed = True, .IsFilled = True, .StartPoint = Parent.GetRealFromRelative(New Point(i + 1, -p))}
                        'currentFigure.Segments.Add(New LineSegment(Parent.GetRealFromRelative(currentFigure.StartPoint), True))
                        currentFigure.Segments.Add(New LineSegment(Parent.GetRealFromRelative(New Point(i + 1, -Parent.bars(i).Data.Close)), True))
                        currentFigure.Segments.Add(New LineSegment(Parent.GetRealFromRelative(New Point(i + 2, -Parent.bars(i).Data.Close)), True))
                        isActive = True
                        startBar = i
                        mode = Direction.Down
                    ElseIf Parent.bars(i).Data.close > p Then 'if new above zone
                        currentFigure = New PathFigure With {.IsClosed = True, .IsFilled = True, .StartPoint = Parent.GetRealFromRelative(New Point(i + 1, -p))}
                        currentFigure.Segments.Add(New LineSegment(Parent.GetRealFromRelative(New Point(i + 1, -Parent.bars(i).Data.Close)), True))
                        currentFigure.Segments.Add(New LineSegment(Parent.GetRealFromRelative(New Point(i + 2, -Parent.bars(i).Data.Close)), True))
                        'currentFigure.Segments.Add(New LineSegment(Parent.GetRealFromRelative(currentFigure.StartPoint), True))
                        isActive = True
                        startBar = i
                        mode = Direction.Up
                    End If
                End If
            End If
            If i = endvalue And isActive Then
                currentFigure.Segments.Add(New LineSegment(Parent.GetRealFromRelative(New Point(i + 1, -p)), True))
                If startBar >= NeutralZoneBarStart Then
                    If mode = Direction.Down Then downNeutralFigures.Add(currentFigure) Else upNeutralFigures.Add(currentFigure)
                Else
                    If mode = Direction.Down Then downFigures.Add(currentFigure) Else upFigures.Add(currentFigure)
                End If
            End If
        Next
        upGeom.Figures = upFigures
        upNeutralGeom.Figures = upNeutralFigures
        downNeutralGeom.Figures = downNeutralFigures
        downGeom.Figures = downFigures
        Return {upGeom, downGeom, upNeutralGeom, downNeutralGeom}
    End Function
    Protected Function GetDotsGeometryForLine(ByVal coordinates As LineCoordinates, ByVal endDotPoints As LineCoordinates, ByVal showEndDots As Boolean) As GeometryGroup
        Dim group As New GeometryGroup
        Dim length As Double = Distance(coordinates.StartPoint, coordinates.EndPoint)
        Dim squareCount As Double
        Select Case length
            Case Is >= 400
                squareCount = Ceiling(length / 150) + 1
            Case Is >= 60
                squareCount = 3
            Case Is >= 20
                squareCount = 1
            Case Is < 20
                squareCount = 0
        End Select
        squareCount = 1 / (squareCount + 1)
        'For t = squareCount To 1 - squareCount Step squareCount
        '    group.Children.Add(New EllipseGeometry(New Windows.Rect((coordinates.EndPoint.X - coordinates.StartPoint.X) * t + coordinates.StartPoint.X - DragCircleSize / 3, (coordinates.EndPoint.Y - coordinates.StartPoint.Y) * t + coordinates.StartPoint.Y - DragCircleSize / 3, DragCircleSize / 1.5, DragCircleSize / 1.5)))
        'Next
        If showEndDots Then
            group.Children.Add(New EllipseGeometry(New Windows.Rect(endDotPoints.StartPoint.X - DragCircleSize / 2, endDotPoints.StartPoint.Y - DragCircleSize / 2, DragCircleSize, DragCircleSize)))
            group.Children.Add(New EllipseGeometry(New Windows.Rect(endDotPoints.EndPoint.X - DragCircleSize / 2, endDotPoints.EndPoint.Y - DragCircleSize / 2, DragCircleSize, DragCircleSize)))
        End If
        Return group
    End Function

    Protected Function GetExtendedRightCoordinates(ByVal coordinates As LineCoordinates) As LineCoordinates
        With coordinates
            Dim bounds As Bounds = Parent.GetRealFromRelative(Parent.Bounds)
            Dim startPoint As Point
            Dim endPoint As Point
            Dim lineBackwards As Boolean

            If .EndPoint.X = .EndPoint.Y Then

            ElseIf .EndPoint.X > .StartPoint.X Then
                startPoint = .StartPoint
                endPoint = .EndPoint
            Else
                lineBackwards = True
                startPoint = .EndPoint
                endPoint = .StartPoint
            End If

            Dim edge As Double
            If endPoint.Y <= startPoint.Y Then
                edge = bounds.Y1
            Else
                edge = bounds.Y2
            End If
            If endPoint.X <> startPoint.X Then
                Dim xPoint As Point = New Point(bounds.X2, LinCalc(startPoint.X, startPoint.Y, endPoint.X, endPoint.Y, bounds.X2))
                Dim xDis As Double = Distance(startPoint, xPoint)
                Dim yPoint As Point = New Point(LinCalc(startPoint.Y, startPoint.X, endPoint.Y, endPoint.X, edge), edge)
                Dim yDis As Double = Distance(startPoint, yPoint)
                If xDis <= yDis Or Double.IsNaN(yDis) Then
                    endPoint = xPoint
                Else
                    endPoint = yPoint
                End If
                If lineBackwards Then
                    .StartPoint = endPoint
                Else
                    .EndPoint = endPoint
                End If
            Else
                If .EndPoint.Y < .StartPoint.Y Then
                    .EndPoint = New Point(.EndPoint.X, bounds.Y1)
                Else
                    .StartPoint = New Point(.StartPoint.X, bounds.Y1)
                End If
            End If
        End With
        Return coordinates
    End Function
    Protected Function GetExtendedLeftCoordinates(ByVal coordinates As LineCoordinates) As LineCoordinates
        With coordinates
            Dim bounds As Bounds = Parent.GetRealFromRelative(Parent.Bounds)
            Dim startPoint As Point
            Dim endPoint As Point
            Dim lineBackwards As Boolean
            If .EndPoint.X <= .StartPoint.X Then
                startPoint = .StartPoint
                endPoint = .EndPoint
            Else
                lineBackwards = True
                startPoint = .EndPoint
                endPoint = .StartPoint
            End If

            Dim edge As Double
            If endPoint.Y <= startPoint.Y Then
                edge = bounds.Y1
            Else
                edge = bounds.Y2
            End If
            If endPoint.X <> startPoint.X Then
                Dim xPoint As Point = New Point(bounds.X1, LinCalc(startPoint.X, startPoint.Y, endPoint.X, endPoint.Y, bounds.X1))
                Dim xDis As Double = Distance(startPoint, xPoint)
                Dim yPoint As Point = New Point(LinCalc(startPoint.Y, startPoint.X, endPoint.Y, endPoint.X, edge), edge)
                Dim yDis As Double = Distance(startPoint, yPoint)
                If xDis <= yDis Or Double.IsNaN(yDis) Then
                    startPoint = xPoint
                Else
                    startPoint = yPoint
                End If
                If lineBackwards Then
                    .StartPoint = startPoint
                Else
                    .EndPoint = startPoint
                End If
            Else
                If .EndPoint.Y > .StartPoint.Y Then
                    .EndPoint = New Point(.EndPoint.X, bounds.Y2)
                Else
                    .StartPoint = New Point(.StartPoint.X, bounds.Y2)
                End If
            End If
        End With
        Return coordinates
    End Function


    Protected Overrides Function PopupInformation() As System.Collections.Generic.List(Of System.Collections.Generic.KeyValuePair(Of String, String))
        Dim startpoint As Point = GetSnappedCoordinates.StartPoint
        Dim endpoint As Point = GetSnappedCoordinates.EndPoint
        Dim items As New List(Of KeyValuePair(Of String, String))
        items.Add(New KeyValuePair(Of String, String)("Start Price: ", FormatNumber(If(UseNegativeCoordinates, -1, 1) * startpoint.Y, Parent.Settings("DecimalPlaces").Value).Replace(",", "")))
        items.Add(New KeyValuePair(Of String, String)("Start Bar: ", startpoint.X))
        If HasParent AndAlso startpoint.X > 0 AndAlso Parent.bars.Count >= startpoint.X Then
            items.Add(New KeyValuePair(Of String, String)("Start Date: ", Parent.bars(startpoint.X - 1).Data.Date.ToShortDateString))
            items.Add(New KeyValuePair(Of String, String)("Start Time: ", Parent.bars(startpoint.X - 1).Data.Date.ToLongTimeString))
        Else
            items.Add(New KeyValuePair(Of String, String)("Start Date: ", "N/A"))
            items.Add(New KeyValuePair(Of String, String)("Start Time: ", "N/A"))
        End If
        items.Add(New KeyValuePair(Of String, String)("End Bar: ", endpoint.X))
        items.Add(New KeyValuePair(Of String, String)("End Price: ", FormatNumber(If(UseNegativeCoordinates, -1, 1) * endpoint.Y, Parent.Settings("DecimalPlaces").Value).Replace(",", "")))
        If HasParent AndAlso endpoint.X > 0 AndAlso Parent.bars.Count >= endpoint.X Then
            items.Add(New KeyValuePair(Of String, String)("End Date: ", Parent.bars(endpoint.X - 1).Data.Date.ToShortDateString))
            items.Add(New KeyValuePair(Of String, String)("End Time: ", Parent.bars(endpoint.X - 1).Data.Date.ToLongTimeString))
        Else
            items.Add(New KeyValuePair(Of String, String)("End Date: ", "N/A"))
            items.Add(New KeyValuePair(Of String, String)("End Time: ", "N/A"))
        End If
        items.Add(New KeyValuePair(Of String, String)("Bar Spread: ", Abs(endpoint.X - startpoint.X)))
        items.Add(New KeyValuePair(Of String, String)("Price Spread: ", FormatNumber(Abs(endpoint.Y - startpoint.Y), Parent.Settings("DecimalPlaces").Value).Replace(",", "")))
        If AnalysisTechnique IsNot Nothing Then
            items.Add(New KeyValuePair(Of String, String)("Analysis Technique: ", AnalysisTechnique.Name))
        Else
            items.Add(New KeyValuePair(Of String, String)("Analysis Technique: ", "N/A"))
        End If
        Return items
    End Function

    Protected dragAreaClicked As Integer = -1
    Protected mouseOffset As Point
    Protected clickCoordinates As LineCoordinates
    Public Overrides Sub ParentMouseLeftButtonUp(e As System.Windows.Input.MouseButtonEventArgs, location As System.Windows.Point)
        MyBase.ParentMouseLeftButtonUp(e, location)
        RefreshVisual()
    End Sub
    Public Overrides Sub ParentMouseLeftButtonDown(ByVal e As MouseButtonEventArgs, ByVal location As System.Windows.Point)
        MyBase.ParentMouseLeftButtonDown(e, location)

        If UseNegativeCoordinates Then
            clickCoordinates = Parent.GetRealFromRelative(_relativeCoordinates)
            mouseOffset = location - clickCoordinates.StartPoint
        Else
            clickCoordinates = Parent.GetRealFromRelative(NegateY(_relativeCoordinates))
            mouseOffset = location - Parent.GetRealFromRelative(NegateY(StartPoint))
        End If

        If Distance(location, clickCoordinates.StartPoint) < DragCircleSize / 2 + ClickLeniency Then
            dragAreaClicked = 1
        ElseIf Distance(location, clickCoordinates.EndPoint) < DragCircleSize / 2 + ClickLeniency Then
            dragAreaClicked = 2
            mouseOffset = location - clickCoordinates.EndPoint
        Else
            If HasParallel And IsRegressionLine = False Then
                If baseLineGeometry.FillContains(location, ClickLeniency, ToleranceType.Absolute) Then
                    dragAreaClicked = 0
                Else
                    dragAreaClicked = 3
                End If
            Else
                dragAreaClicked = 0
            End If
        End If
        IsSelected = True
    End Sub
    Public Overrides Sub ParentMouseMove(ByVal e As MouseEventArgs, ByVal location As System.Windows.Point)
        If IsMouseDown And IsEditable Then
            Dim prevAutoRefresh As Boolean = AutoRefresh
            AutoRefresh = False

            Dim realCoordinates As LineCoordinates
            If UseNegativeCoordinates Then
                realCoordinates = Parent.GetRealFromRelative(_relativeCoordinates)
            Else
                realCoordinates = Parent.GetRealFromRelative(NegateY(_relativeCoordinates))
            End If

            Select Case dragAreaClicked
                Case 0
                    Coordinates = Parent.GetRelativeFromReal(New LineCoordinates(location - mouseOffset, location + (realCoordinates.EndPoint - realCoordinates.StartPoint) - mouseOffset))
                    If Not UseNegativeCoordinates Then Coordinates = NegateY(Coordinates)
                Case 1
                    If UseNegativeCoordinates Then
                        Coordinates = Parent.GetRelativeFromReal(New LineCoordinates(location - mouseOffset, realCoordinates.EndPoint))
                    Else
                        Coordinates = NegateY(Parent.GetRelativeFromReal(New LineCoordinates(location - mouseOffset, realCoordinates.EndPoint)))
                    End If
                Case 2
                    If UseNegativeCoordinates Then
                        Coordinates = Parent.GetRelativeFromReal(New LineCoordinates(realCoordinates.StartPoint, location - mouseOffset))
                    Else
                        Coordinates = NegateY(Parent.GetRelativeFromReal(New LineCoordinates(realCoordinates.StartPoint, location - mouseOffset)))
                    End If
                Case 3
                    If HasParallel Then
                        ParallelDistance = -((LinCalc(StartPoint.X, StartPoint.Y, EndPoint.X, EndPoint.Y, Parent.GetRelativeFromRealX(Mouse.GetPosition(Parent).X)) + (CInt(UseNegativeCoordinates) * 2 + 1) * Parent.GetRelativeFromRealY(Mouse.GetPosition(Parent).Y)))
                    End If
            End Select
            Coordinates = GetSnappedCoordinates()
            RaiseMouseDraggedEvent()
            AutoRefresh = prevAutoRefresh
            RefreshVisual()
        End If

        MyBase.ParentMouseMove(e, location)
    End Sub
    Public Overrides Sub ParentMouseRightButtonDown(ByVal e As System.Windows.Input.MouseButtonEventArgs, ByVal location As System.Windows.Point)
        MyBase.ParentMouseRightButtonDown(e, location)
    End Sub
    Public Overrides Sub ParentMouseRightButtonUp(ByVal e As System.Windows.Input.MouseButtonEventArgs, ByVal location As System.Windows.Point)
        MyBase.ParentMouseRightButtonUp(e, location)
        If IsEditable Then
            Dim formatWin As New FormatTrendLineWindow(Me)
        End If
    End Sub

    Friend Overrides Sub Command_Executed(ByVal sender As Object, ByVal e As System.Windows.Input.ExecutedRoutedEventArgs)
        If e.Command Is ChartCommands.RemoveObject Then
            Parent.Children.Remove(Me)
        ElseIf e.Command Is ChartCommands.CreateLineParallel Then
            HasParallel = True
            If HasParent Then
                ParallelDistance = -((LinCalc(StartPoint.X, StartPoint.Y, EndPoint.X, EndPoint.Y, Parent.GetRelativeFromRealX(Mouse.GetPosition(Parent).X)) + (CInt(UseNegativeCoordinates) * 2 + 1) * Parent.GetRelativeFromRealY(Mouse.GetPosition(Parent).Y)))
            End If
            If Not AutoRefresh Then RefreshVisual()
        ElseIf e.Command Is ChartCommands.RemoveLineParallel Then
            HasParallel = False
            If Not AutoRefresh Then RefreshVisual()
        ElseIf e.Command Is ChartCommands.CreateObjectDuplicate Then
            Dim tl As New TrendLine
            Dim realPos As LineCoordinates = Parent.GetRealFromRelative(New LineCoordinates(StartPoint, EndPoint))
            realPos = New LineCoordinates(New Point(realPos.StartPoint.X, realPos.StartPoint.Y - 40), New Point(realPos.EndPoint.X, realPos.EndPoint.Y - 40))
            tl.UseNegativeCoordinates = UseNegativeCoordinates
            tl.Coordinates = Parent.GetRelativeFromReal(realPos)
            tl.Pen = Pen.Clone
            tl.OuterPen = OuterPen.Clone
            tl.ExtendLeft = ExtendLeft
            tl.ExtendRight = ExtendRight
            tl.AutoRefresh = AutoRefresh
            tl.IsSelectable = IsSelectable
            tl.IsEditable = IsEditable
            tl.RefreshVisual()
            Parent.Children.Add(tl)
        ElseIf e.Command Is ChartCommands.ToggleLineExtensionLeft Then
            ExtendLeft = Not ExtendLeft
        ElseIf e.Command Is ChartCommands.ToggleLineExtensionRight Then
            ExtendRight = Not ExtendRight
        Else
            MyBase.Command_Executed(sender, e)
        End If
    End Sub

    Public Overrides Function ContainsPoint(ByVal point As System.Windows.Point) As Boolean
        If HasParallel Then
            Return baseLineGeometry.FillContains(point, ClickLeniency, ToleranceType.Absolute) OrElse parallelLineGeometry.FillContains(point, ClickLeniency, ToleranceType.Absolute)
        Else
            Return baseLineGeometry.FillContains(point, ClickLeniency, ToleranceType.Absolute)
        End If
    End Function

    Public Sub Deserialize(ByVal serializedString As String) Implements ISerializable.Deserialize
        Try
            Dim v() As String = Split(serializedString, "/")
            Pen = PenFromString(v(0))
            StartPoint = (New PointConverter).ConvertFromString(v(1))
            EndPoint = (New PointConverter).ConvertFromString(v(2))
            ExtendLeft = v(3)
            ExtendRight = v(4)
            UseNegativeCoordinates = v(5)
            AutoRefresh = v(6)
            If (New Date(CLng(v(7)))).Ticks <> 0 And Parent IsNot Nothing Then StartPoint = New Point(Parent.GetBarNumberFromDate(New Date(v(7))), StartPoint.Y)
            If (New Date(CLng(v(8)))).Ticks <> 0 And Parent IsNot Nothing Then EndPoint = New Point(Parent.GetBarNumberFromDate(New Date(v(8))), EndPoint.Y)
        Catch ex As Exception
            Throw New ArgumentException("'TrendLine.Deserialize' got an error parsing the string '" & serializedString & "'.")
        End Try
    End Sub
    Public Function Serialize() As String Implements ISerializable.Serialize
        Dim startDate As New Date(0)
        Dim endDate As New Date(0)
        If Parent IsNot Nothing Then
            startDate = Parent.GetDateFromBarNumber(StartPoint.X)
            endDate = Parent.GetDateFromBarNumber(EndPoint.X)
        End If
        Return Join({
                    PenToString(Pen),
                    (New PointConverter).ConvertToString(StartPoint),
                    (New PointConverter).ConvertToString(EndPoint),
                    ExtendLeft,
                    ExtendRight,
                    UseNegativeCoordinates,
                    AutoRefresh,
                    startDate.Ticks,
                    endDate.Ticks
                }, "/")
    End Function

    Private Function CalculateNewStartPointFixedRegression(startPointX As Decimal, lineStartPoint As Decimal, lineEndPoint As Decimal, endPointX As Decimal) As LineCoordinates
        If startPointX < 0 Or endPointX >= Parent.bars.Count Then Return New LineCoordinates
        Dim n As Decimal = (endPointX) - startPointX + 1
        Dim b As Decimal
        Dim sumx2 As Decimal = 0, sumx As Decimal = 0, sumy As Decimal = 0, sumxyhigh As Decimal = 0, sumxylow As Decimal = 0
        Dim sumSlopesHigh As Decimal = 0
        Dim sumSlopesLow As Decimal = 0
        Dim startYhigh As Decimal = Parent.bars(startPointX).Data.High
        Dim startYlow As Decimal = Parent.bars(startPointX).Data.Low
        For bari = startPointX + 1 To endPointX
            Dim point As New Point(bari + 1, Parent.bars(bari).Data.Avg)
            sumSlopesHigh += (point.Y - startYhigh)
            sumSlopesLow += point.Y - startYlow
        Next
        If ((endPointX - startPointX) * (endPointX - startPointX + 1) / 2) = 0 Then Return New LineCoordinates
        Dim startY As Decimal
        If Avg(sumSlopesHigh, sumSlopesLow) < 0 Then
            startY = startYhigh
            b = sumSlopesHigh / ((endPointX - startPointX) * (endPointX - startPointX + 1) / 2)
        Else
            startY = startYlow
            b = sumSlopesLow / ((endPointX - startPointX) * (endPointX - startPointX + 1) / 2)
        End If

        Dim l As LineCoordinates
        If lineStartPoint < lineEndPoint Then
            l = New LineCoordinates(New Point(lineStartPoint, startY), New Point(lineEndPoint, startY + (lineEndPoint - lineStartPoint) * b))
        Else
            l = New LineCoordinates(New Point(lineStartPoint, startY + Abs(lineEndPoint - lineStartPoint) * b), New Point(lineEndPoint, startY))
        End If
        'Dim val As Double2
        'Dim totalArea As Double
        'For i = l.LeftX - 1 To l.RightX
        '    If i < Parent.bars.Count - 1 Then
        '        val += Parent.bars(i).Data.Avg - LinCalc(l, i + 1)
        '        totalArea += Abs(Parent.bars(i).Data.Avg - LinCalc(l, i + 1))
        '    End If
        'Next
        'Log("val: " & val / totalArea)
        Return l
        'Return New LineCoordinates(New Point(lineStartPoint,a+ b * lineStartPoint), New Point(lineEndPoint, a + b * lineEndPoint))
    End Function
    Private Function CalculateNewStartPointRegressionMiddle(startPointX As Decimal, lineStartPoint As Decimal, lineEndPoint As Decimal, endPointX As Decimal) As LineCoordinates
        If startPointX < 0 Or Parent.bars.Count = 0 Then Return New LineCoordinates
        Dim n As Decimal = (endPointX) - startPointX + 1
        Dim a As Decimal, b As Decimal
        Dim sumx2 As Decimal = 0, sumx As Decimal = 0, sumy As Decimal = 0, sumxy As Decimal = 0, sumxylow As Decimal = 0

        For bari = startPointX To endPointX
            Dim point As New Point(bari, Parent.bars(bari - 1).Data.Avg)
            sumx += point.X
            sumy += point.Y
            sumxy += point.X * point.Y
            sumx2 += (point.X) ^ 2
        Next
        If (n * sumx2 - sumx * sumx) = 0 Or n = 0 Then Return New LineCoordinates
        b = (n * sumxy - sumx * sumy) / (n * sumx2 - sumx * sumx)
        a = (sumy - b * sumx) / n

        Dim l As LineCoordinates
        l = New LineCoordinates(New Point(lineStartPoint, a + b * lineStartPoint), New Point(lineEndPoint, a + b * lineEndPoint))

        Return l
    End Function
End Class



