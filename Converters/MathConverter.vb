Public Class MathConverter
    Implements IValueConverter

    Public Function Convert(ByVal value As Object, ByVal targetType As System.Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.Convert
        Select Case parameter.ToString.Substring(0, 1)
            Case "/"
                Return value / parameter.ToString.Substring(1)
            Case "*"
                Return value * parameter.ToString.Substring(1)
            Case "+"
                Return value + parameter.ToString.Substring(1)
            Case "-"
                Return value - parameter.ToString.Substring(1)
            Case "^"
                Return value ^ parameter.ToString.Substring(1)
            Case "%"
                Return value Mod parameter.ToString.Substring(1)
            Case "\"
                Return value \ parameter.ToString.Substring(1)
            Case "="
                Return parameter.ToString.Substring(1)
        End Select
        Return Nothing
    End Function

    Public Function ConvertBack(ByVal value As Object, ByVal targetType As System.Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.ConvertBack
        Select Case parameter.ToString.Substring(0, 1)
            Case "/"
                Return value * parameter.ToString.Substring(1)
            Case "*"
                Return value / parameter.ToString.Substring(1)
            Case "+"
                Return value - parameter.ToString.Substring(1)
            Case "-"
                Return value + parameter.ToString.Substring(1)
            Case "^"
                Return Math.Pow(value, 1 / parameter.ToString.Substring(1))
            Case "%"
                Return value \ parameter.ToString.Substring(1)
            Case "\"
                Return value Mod parameter.ToString.Substring(1)
            Case "="
                Return value
        End Select
        Return Nothing
    End Function
End Class
