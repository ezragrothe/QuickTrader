Public Class FontConverter
    Inherits System.ComponentModel.TypeConverter

    <DebuggerStepThrough()>
    Public Overrides Function CanConvertFrom(ByVal context As System.ComponentModel.ITypeDescriptorContext, ByVal sourceType As System.Type) As Boolean
        Return sourceType = GetType(String)
    End Function
    <DebuggerStepThrough()>
    Public Overrides Function CanConvertTo(ByVal context As System.ComponentModel.ITypeDescriptorContext, ByVal destinationType As System.Type) As Boolean
        Return destinationType = GetType(Font)
    End Function
    <DebuggerStepThrough()>
    Public Overrides Function ConvertFrom(ByVal context As System.ComponentModel.ITypeDescriptorContext, ByVal culture As System.Globalization.CultureInfo, ByVal value As Object) As Object
        Try
            If TypeOf value Is String Then
                Dim v() As String = Split(value, ";")
                Dim font As New Font
                font.Brush = (New BrushConverter).ConvertFromString(v(0))
                font.Effect = EffectFromString(v(1))
                font.FontFamily = (New FontFamilyConverter).ConvertFromString(v(2))
                font.FontSize = v(3)
                font.FontStyle = (New FontStyleConverter).ConvertFromString(v(4))
                font.FontWeight = (New FontWeightConverter).ConvertFromString(v(5))
                font.Transform = RotateTransformFromString(v(6))
                Return font
            End If
        Catch
            Throw New ArgumentException("Could not convert '" & value.ToString & "'.")
        End Try
        Throw New ArgumentException("Could not convert '" & value.ToString & "'.")
    End Function
    <DebuggerStepThrough()>
    Public Overrides Function ConvertTo(ByVal context As System.ComponentModel.ITypeDescriptorContext, ByVal culture As System.Globalization.CultureInfo, ByVal value As Object, ByVal destinationType As System.Type) As Object
        Try
            If destinationType = GetType(String) And TypeOf value Is Font Then
                Dim font As Font = value
                Return Join({
                            (New BrushConverter).ConvertToString(font.Brush),
                            EffectToString(font.Effect),
                            (New FontFamilyConverter).ConvertToString(font.FontFamily),
                            font.FontSize,
                            (New FontStyleConverter).ConvertToString(font.FontStyle),
                            (New FontWeightConverter).ConvertToString(font.FontWeight),
                            RotateTransformToString(font.Transform)
                        },";")
            End If
        Catch
            Throw New ArgumentException("Could not convert '" & value.ToString & "' to type '" & destinationType.ToString & "'.")
        End Try
        Throw New ArgumentException("Could not convert '" & value.ToString & "' to type '" & destinationType.ToString & "'.")
    End Function
End Class
