<System.ComponentModel.TypeConverterAttribute(GetType(FontConverter))> _
Public Class Font
    Public Sub New()
    End Sub
    Public Sub New(ByVal fontFamily As FontFamily, ByVal brush As Brush)
        Me.FontFamily = fontFamily
        Me.Brush = brush
    End Sub
    Public Property FontWeight As FontWeight = FontWeights.Normal
    Public Property FontStyle As FontStyle = FontStyles.Normal
    Public Property FontSize As Double = 12
    Public Property FontFamily As FontFamily = New FontFamily("Arial")
    Public Property Effect As Effects.Effect = Nothing
    Public Property Transform As RotateTransform = New RotateTransform(0)
    Public Property Brush As Brush = Brushes.White
    Public Function Clone() As Font
        Dim font As New Font
        font.FontWeight = FontWeight
        font.FontStyle = FontStyle
        font.FontSize = FontSize
        font.FontFamily = FontFamily
        font.Effect = Effect
        font.Transform = Transform
        font.Brush = Brush
        Return font
    End Function
End Class