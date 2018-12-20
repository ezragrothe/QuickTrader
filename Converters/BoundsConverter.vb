''' <summary>
''' Converts instances of other types to and from instances of 
''' </summary>
Public Class BoundsConverter
    Inherits System.ComponentModel.TypeConverter

    <DebuggerStepThrough()>
    Public Overrides Function CanConvertFrom(ByVal context As System.ComponentModel.ITypeDescriptorContext, ByVal sourceType As System.Type) As Boolean
        Return (New Windows.RectConverter).CanConvertFrom(context, sourceType)
    End Function
    <DebuggerStepThrough()>
    Public Overrides Function CanConvertTo(ByVal context As System.ComponentModel.ITypeDescriptorContext, ByVal destinationType As System.Type) As Boolean
        Return (New Windows.RectConverter).CanConvertTo(context, destinationType)
    End Function
    <DebuggerStepThrough()>
    Public Overrides Function ConvertFrom(ByVal context As System.ComponentModel.ITypeDescriptorContext, ByVal culture As System.Globalization.CultureInfo, ByVal value As Object) As Object
        Dim rect As Windows.Rect = (New Windows.RectConverter).ConvertFrom(context, culture, value)
        Return Bounds.FromWindowsRect(rect)
    End Function
    <DebuggerStepThrough()>
    Public Overrides Function ConvertTo(ByVal context As System.ComponentModel.ITypeDescriptorContext, ByVal culture As System.Globalization.CultureInfo, ByVal value As Object, ByVal destinationType As System.Type) As Object
        Return (New Windows.RectConverter).ConvertTo(context, culture, CType(value, Bounds).ToWindowsRect, destinationType)
    End Function
    <DebuggerStepThrough()>
    Public Overrides Function IsValid(ByVal context As System.ComponentModel.ITypeDescriptorContext, ByVal value As Object) As Boolean
        Return (New Windows.RectConverter).IsValid(context, value)
    End Function
End Class