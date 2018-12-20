Public Class BarDataConverter
    Inherits System.ComponentModel.TypeConverter
    <DebuggerStepThrough()>
    Public Overrides Function CanConvertFrom(ByVal context As System.ComponentModel.ITypeDescriptorContext, ByVal sourceType As System.Type) As Boolean
        Return sourceType = GetType(String)
    End Function
    <DebuggerStepThrough()>
    Public Overrides Function CanConvertTo(ByVal context As System.ComponentModel.ITypeDescriptorContext, ByVal destinationType As System.Type) As Boolean
        Return destinationType = GetType(BarData)
    End Function
    '<DebuggerStepThrough()>
    Public Overrides Function ConvertFrom(ByVal context As System.ComponentModel.ITypeDescriptorContext, ByVal culture As System.Globalization.CultureInfo, ByVal value As Object) As Object
        'Try
        If TypeOf value Is String Then
            Dim v() As String = Split(value, ",")
            Dim t As TimeSpan
            If v(5).Contains(":") Then
                t = (New TimeSpanConverter).ConvertFromString(v(5))
            Else
                t = TimeSpan.FromSeconds(v(5))
            End If
            Return New BarData(v(0), v(1), v(2), v(3), StringToDate(v(4), False), t)
        End If
        Return Nothing
        'Catch
        '    Throw New ArgumentException("Could not convert '" & value.ToString & "'.")
        'End Try
        'Throw New ArgumentException("Could not convert '" & value.ToString & "'.")
    End Function
    '<DebuggerStepThrough()>
    Public Overrides Function ConvertTo(ByVal context As System.ComponentModel.ITypeDescriptorContext, ByVal culture As System.Globalization.CultureInfo, ByVal value As Object, ByVal destinationType As System.Type) As Object
        ' Try
        If destinationType = GetType(String) And TypeOf value Is BarData Then
            Dim bar As BarData = value
            Dim d = bar.Date
            Return String.Format("{0},{1},{2},{3},{4},{5}", bar.Open, bar.High, bar.Low, bar.Close, DateToString(d), Round(bar.Duration.TotalSeconds, 0))
        End If
        Return Nothing
        'Catch
        '    Throw New ArgumentException("Could not convert '" & value.ToString & "' to type '" & destinationType.ToString & "'.")
        'End Try
        'Throw New ArgumentException("Could not convert '" & value.ToString & "' to type '" & destinationType.ToString & "'.")
    End Function
    Public Overrides Function IsValid(ByVal context As System.ComponentModel.ITypeDescriptorContext, ByVal value As Object) As Boolean
        Dim v() As String = Split(value, ",")
        If v.GetUpperBound(0) = 5 Then
            If IsNumeric(v(0)) And IsNumeric(v(1)) And IsNumeric(v(2)) And IsNumeric(v(3)) And (New TimeSpanConverter).IsValid(v(5)) And IsNumeric(v(4)) Then
                Return True
            End If
        End If
        Return False
        'Return (New Windows.RectConverter).IsValid(context, value)
    End Function
End Class
