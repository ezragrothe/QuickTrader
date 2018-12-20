Public Class LabelStyle
    Public Property Font As New Font
    Public Property Text As String = "Text"
    Public Function Clone() As LabelStyle
        Dim style As New LabelStyle
        style.Font = Font.Clone
        style.Text = Text
        Return style
    End Function
End Class

Public Class TrendLineStyle
    Public Property Pen As New Pen(Brushes.Red, 1)
    Public Property OuterPen As New Pen(Brushes.Red, 1)
    Public Property ExtendRight As Boolean
    Public Property ExtendLeft As Boolean
    Public Property IsRegressionLine As Boolean
    Public Property HasParallel As Boolean
    Public Property LockToEnd As Boolean
    Public Function Clone() As TrendLineStyle
        Dim style As New TrendLineStyle
        style.Pen = Pen.Clone
        style.OuterPen = OuterPen.Clone
        style.ExtendLeft = ExtendLeft
        style.ExtendRight = ExtendRight
        style.IsRegressionLine = IsRegressionLine
        style.HasParallel = HasParallel
        style.LockToEnd = LockToEnd
        Return style
    End Function
End Class

Public Class RectangleStyle
    Public Property Pen As New Pen(Brushes.Red, 1)
    Public Property Fill As Brush
    Public Function Clone() As RectangleStyle
        Dim style As New RectangleStyle
        style.Pen = Pen.Clone
        style.Fill = Fill
        Return style
    End Function
End Class

Public Class ArrowStyle
    Public Property Pen As New Pen(Brushes.Red, 2)
    Public Property IsFlipped As Boolean
    Public Property Width As Double = 12
    Public Property Height As Double = 4
    Public Function Clone() As ArrowStyle
        Dim style As New ArrowStyle
        style.Pen = Pen.Clone
        style.IsFlipped = IsFlipped
        style.Width = Width
        style.Height = Height
        Return style
    End Function
End Class