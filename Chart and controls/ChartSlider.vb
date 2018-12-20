Public Class ChartSlider
    Inherits UIChartControl
    Public Sub New()
        Content = New Slider
        AddHandler CType(Content, Slider).ValueChanged, Sub(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) RaiseEvent ValueChanged(sender, e)
        Content.Width = 200
        Content.Height = 20
    End Sub
    Public Event ValueChanged(ByVal sender As Object, ByVal e As RoutedPropertyChangedEventArgs(Of Double))
End Class
