Public MustInherit Class GeometryVisual
    Inherits Windows.Media.DrawingVisual

    Public Sub New()
    End Sub

    Private _fill As Brush
    Private _data As Geometry
    Private _pen As Pen = New Pen(Nothing, 1)

    Protected Overridable Property Fill As Brush
        Get
            Return _fill
        End Get
        Set(ByVal value As Brush)
            _fill = value
            If AutoRefresh And Not UseStrokeAsFill Then RefreshVisual()
        End Set
    End Property
    Protected Overridable Property Geometry As Geometry
        Get
            Return _data
        End Get
        Set(ByVal value As Geometry)
            _data = value
            If AutoRefresh Then RefreshVisual()
        End Set
    End Property
    Public Overridable Property Pen As Pen
        Get
            Return _pen
        End Get
        Set(ByVal value As Pen)
            _pen = value
            If AutoRefresh Then RefreshVisual()
        End Set
    End Property

    Public Property UseStrokeAsFill As Boolean = True
    Public Property AutoRefresh As Boolean = False

    Public ReadOnly Property Data As Geometry
        Get
            Return Geometry
        End Get
    End Property

    Public Overridable Sub RefreshVisual()
        Using dc As DrawingContext = RenderOpen()
            If UseStrokeAsFill Then
                dc.DrawGeometry(Pen.Brush, Pen, Geometry)
            Else
                dc.DrawGeometry(Fill, Pen, Geometry)
            End If
        End Using
    End Sub
End Class
