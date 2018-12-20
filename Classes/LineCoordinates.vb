''' <summary>
''' Describes the coordinates for a line.
''' </summary>
Public Structure LineCoordinates
    ''' <summary>
    ''' Initializes a new LineCoordinates object with the specified points.
    ''' </summary>
    ''' <param name="startPoint">The start point for the line.</param>
    ''' <param name="endPoint">The end point for the line.</param>
    <DebuggerStepThrough()>
    Public Sub New(ByVal startPoint As Point, ByVal endPoint As Point)
        Me.StartPoint = startPoint
        Me.EndPoint = endPoint
    End Sub

    ''' <summary>
    ''' Initializes a new LineCoordinates object with the specified points.
    ''' </summary>
    ''' <param name="x1">The start point X component.</param>
    ''' <param name="y1">The start point Y component.</param>
    ''' <param name="x2">The end point X component.</param>
    ''' <param name="y2">The end point Y component.</param>
    ''' <remarks></remarks>
    <DebuggerStepThrough()>
    Public Sub New(ByVal x1 As Double, ByVal y1 As Double, ByVal x2 As Double, ByVal y2 As Double)
        Me.StartPoint = New Point(x1, y1)
        Me.EndPoint = New Point(x2, y2)
    End Sub

    ''' <summary>
    ''' Gets or sets the start point.
    ''' </summary>
    Public Property StartPoint As Point
    ''' <summary>
    ''' Gets or sets the end point.
    ''' </summary>
    Public Property EndPoint As Point

    ''' <summary>
    ''' Gets the left-most X value of the coordinates.
    ''' </summary>
    Public ReadOnly Property LeftX As Double
        <DebuggerStepThrough()>
        Get
            Return Math.Min(StartPoint.X, EndPoint.X)
        End Get
    End Property

    ''' <summary>
    ''' Gets the right-most X value of the coordinates.
    ''' </summary>
    Public ReadOnly Property RightX As Double
        <DebuggerStepThrough()>
        Get
            Return Math.Max(StartPoint.X, EndPoint.X)
        End Get
    End Property

    ''' <summary>
    ''' Gets the top-most Y value of the coordinates.
    ''' </summary>
    Public ReadOnly Property TopY As Double
        <DebuggerStepThrough()>
        Get
            Return Math.Min(StartPoint.Y, EndPoint.Y)
        End Get
    End Property

    ''' <summary>
    ''' Gets the bottom-most Y value of the coordinates.
    ''' </summary>
    Public ReadOnly Property BottomY As Double
        <DebuggerStepThrough()>
        Get
            Return Math.Max(StartPoint.Y, EndPoint.Y)
        End Get
    End Property

    ''' <summary>
    ''' Returns the LineCoordinates object converted to Rect.
    ''' </summary>
    <DebuggerStepThrough()>
    Public Function ToRect() As Rect
        Return New Rect(LeftX, TopY, RightX, BottomY)
    End Function

    ''' <summary>
    ''' Returns the LineCoordinates object converted to 
    ''' </summary>
    <DebuggerStepThrough()>
    Public Function ToBounds() As Bounds
        Return New Bounds(LeftX, TopY, RightX, BottomY)
    End Function

    ''' <summary>
    ''' Converts the specified Bounds object to a LineCoordinates object.
    ''' </summary>
    ''' <param name="bounds">The Bounds object to convert.</param>
    ''' <param name="drawFromBottomLeft">Specifies whether to draw the line from the bottom left corner or top left corner. This argument is optional.</param>
    <DebuggerStepThrough()>
    Public Shared Function FromBounds(ByVal bounds As Bounds, Optional ByVal drawFromBottomLeft As Boolean = False) As LineCoordinates
        If drawFromBottomLeft Then
            Return New LineCoordinates(bounds.X1, bounds.Y2, bounds.X2, bounds.Y1)
        Else
            Return New LineCoordinates(bounds.X1, bounds.Y1, bounds.X2, bounds.Y2)
        End If
    End Function
    Shared Operator <>(l1 As LineCoordinates, l2 As LineCoordinates) As Boolean
        Return l1.StartPoint <> l2.StartPoint Or l1.EndPoint <> l2.EndPoint
    End Operator
    Shared Operator =(l1 As LineCoordinates, l2 As LineCoordinates) As Boolean
        Return l1.StartPoint = l2.StartPoint And l1.EndPoint = l2.EndPoint
    End Operator
    <DebuggerStepThrough()>
    Public Overrides Function ToString() As String
        Return String.Format("({0}, {1}), ({2}, {3})", StartPoint.X, StartPoint.Y, EndPoint.X, EndPoint.Y)
    End Function
End Structure