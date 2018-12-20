''' <summary>
''' Describes the top left and bottom right points for a rectangle.
''' </summary>
<System.ComponentModel.TypeConverterAttribute(GetType(BoundsConverter))> _
Public Structure Bounds
    Public _x, _y, _width, _height As Double
    ''' <summary>
    ''' Initializes a new Bounds with the specified dimensions.
    ''' </summary>
    ''' <param name="x1">The initial top left X value for the </param>
    ''' <param name="y1">The initial top left Y value for the </param>
    ''' <param name="x2">The initial bottom right X value for the </param>
    ''' <param name="y2">The initial bottom rightY value for the </param>

    <DebuggerStepThrough()>
    Public Sub New(ByVal x1 As Double, ByVal y1 As Double, ByVal x2 As Double, ByVal y2 As Double)
        TopLeft = New Point(x1, y1)
        BottomRight = New Point(x2, y2)
    End Sub

    ''' <summary>
    ''' Initializes a new Bounds with the specified dimensions.
    ''' </summary>
    ''' <param name="topLeft">The initial top left point for the </param>
    ''' <param name="bottomRight">The initial bottom right for the </param>
    <DebuggerStepThrough()>
    Public Sub New(ByVal topLeft As Point, ByVal bottomRight As Point)
        Me.TopLeft = topLeft
        Me.BottomRight = bottomRight
    End Sub

    ''' <summary>
    ''' Initializes a new Bounds with the specified dimensions.
    ''' </summary>
    ''' <param name="x">The initial X position for the </param>
    ''' <param name="y">The initial Y position for the </param>
    ''' <param name="size">The initial size for the </param>
    <DebuggerStepThrough()>
    Public Sub New(ByVal x As Double, ByVal y As Double, ByVal size As Size)
        X1 = x
        Y1 = y
        Me.Size = size
    End Sub

    ''' <summary>
    ''' Initializes a new Bounds with the specified dimensions.
    ''' </summary>
    ''' <param name="pos">The initial position for the </param>
    ''' <param name="size">The initial size for the </param>
    <DebuggerStepThrough()>
    Public Sub New(ByVal pos As Point, ByVal size As Size)
        X1 = pos.X
        Y1 = pos.Y
        Me.Size = size
    End Sub

    ''' <summary>
    ''' Gets or sets the top left X value for the 
    ''' </summary>
    Public Property X1 As Double
        <DebuggerStepThrough()>
        Get
            Return _x
        End Get
        <DebuggerStepThrough()>
        Set(ByVal value As Double)
            Dim _x2 As Double = X2
            _x = value
            _width = _x2 - X1
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the top left Y value for the 
    ''' </summary>
    Public Property Y1 As Double
        Get
            Return _y
        End Get
        Set(ByVal value As Double)
            Dim _y2 As Double = Y2
            _y = value
            _height = _y2 - Y1
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the bottom right X value for the 
    ''' </summary>
    Public Property X2 As Double
        Get
            Return _x + _width
        End Get
        Set(ByVal value As Double)
            _width = value - _x
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the bottom right Y value for the 
    ''' </summary>
    Public Property Y2 As Double
        Get
            Return _y + _Height
        End Get
        Set(ByVal value As Double)
            _height = value - _Y
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the top right point for the 
    ''' </summary>
    Public Property TopRight As Point
        <DebuggerStepThrough()>
        Get
            Return New Point(_X + _Width, _Y)
        End Get
        <DebuggerStepThrough()>
        Set(ByVal value As Point)
            _Width = value.X - _X
            _Y = value.Y
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the bottom right point for the 
    ''' </summary>
    Public Property BottomRight As Point
        <DebuggerStepThrough()>
        Get
            Return New Point(_X + _Width, _Y + _Height)
        End Get
        <DebuggerStepThrough()>
        Set(ByVal value As Point)
            _Width = value.X - _X
            _Height = value.Y - _Y
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the bottom left point for the 
    ''' </summary>
    Public Property BottomLeft As Point
        <DebuggerStepThrough()>
        Get
            Return New Point(_X, _Y + _Height)
        End Get
        <DebuggerStepThrough()>
        Set(ByVal value As Point)
            _X = value.X
            _Height = value.Y - _Y
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the top left point for the 
    ''' </summary>
    Public Property TopLeft As Point
        <DebuggerStepThrough()>
        Get
            Return New Point(_X, _Y)
        End Get
        <DebuggerStepThrough()>
        Set(ByVal value As Point)
            _X = value.X
            _Y = value.Y
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the width for the 
    ''' </summary>
    Public Property Width As Double
        <DebuggerStepThrough()>
        Get
            Return _Width
        End Get
        <DebuggerStepThrough()>
        Set(ByVal value As Double)
            _Width = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the height for the 
    ''' </summary>
    Public Property Height As Double
        <DebuggerStepThrough()>
        Get
            Return _Height
        End Get
        <DebuggerStepThrough()>
        Set(ByVal value As Double)
            _Height = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the size for the 
    ''' </summary>
    Public Property Size As Size
        <DebuggerStepThrough()>
        Get
            Return New Size(_Width, _Height)
        End Get
        <DebuggerStepThrough()>
        Set(ByVal value As Size)
            _Width = value.Width
            _Height = value.Height
        End Set
    End Property

    ''' <summary>
    ''' Gets the equivalent System.Windows.Rect for this 
    ''' </summary>
    <DebuggerStepThrough()>
    Public Function ToWindowsRect() As Windows.Rect
        Return New Windows.Rect(TopLeft, Size)
    End Function

    ''' <summary>
    ''' Converts a System.Windows.Rect object to 
    ''' </summary>
    ''' <param name="windowsRect">The System.Windows.Bounds object to convert.</param>
    <DebuggerStepThrough()>
    Public Shared Function FromWindowsRect(ByVal windowsRect As Windows.Rect) As Bounds
        Return New Bounds(windowsRect.TopLeft, windowsRect.Size)
    End Function

    <DebuggerStepThrough()>
    Public Overrides Function ToString() As String
        Return String.Format("({0}, {1}, {2}, {3})", X1, Y1, X2, Y2)
    End Function
End Structure