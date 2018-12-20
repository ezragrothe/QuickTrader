Public Class ColorToBrushConverter
    Implements IValueConverter

    Public Function Convert(ByVal value As Object, ByVal targetType As System.Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.Convert
        If TypeOf value Is Color Then
            Return New SolidColorBrush(value)
        Else
            Return Nothing
        End If
    End Function

    Public Function ConvertBack(ByVal value As Object, ByVal targetType As System.Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.ConvertBack
        If TypeOf value Is SolidColorBrush Then
            Return CType(value, SolidColorBrush).Color
        Else
            Return Nothing
        End If
    End Function
End Class
