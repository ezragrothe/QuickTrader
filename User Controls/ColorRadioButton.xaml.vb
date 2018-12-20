Public Class ColorRadioButton
    Public Shared ColorBrushProperty As DependencyProperty = DependencyProperty.Register("ColorBrush", GetType(Brush), GetType(ColorRadioButton), New PropertyMetadata(Brushes.White))
    Public Sub New()
        InitializeComponent()
        If TypeOf ColorBrush Is SolidColorBrush Then
            ToolTip = GetColorName(CType(ColorBrush, SolidColorBrush).Color)
        End If
    End Sub
    Public Property ColorBrush As Brush
        Get
            Return GetValue(ColorBrushProperty)
        End Get
        Set(ByVal value As Brush)
            SetValue(ColorBrushProperty, value)
            If TypeOf value Is SolidColorBrush Then
                ToolTip = GetColorName(CType(value, SolidColorBrush).Color)
            Else
                ToolTip = ""
            End If
        End Set
    End Property
    Protected Overrides Sub OnPropertyChanged(ByVal e As System.Windows.DependencyPropertyChangedEventArgs)
        MyBase.OnPropertyChanged(e)
        If e.Property Is ColorBrushProperty Then
            ColorBrushChangedCallback(Nothing, Nothing)
        End If
    End Sub
    Private Sub ColorBrushChangedCallback(ByVal d As DependencyObject, ByVal e As DependencyPropertyChangedEventArgs)
        RaiseEvent ColorBrushChanged(Me, New EventArgs)
    End Sub

    Public Event ColorBrushChanged(ByVal sender As Object, ByVal e As EventArgs)

End Class
