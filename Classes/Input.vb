Imports System.ComponentModel
Imports Microsoft.CSharp
Imports System.Reflection

Public NotInheritable Class Input
    Private [property] As PropertyInfo
    Private owner As Object
    Public Sub New(ByVal analysisTechnique As Object, ByVal [property] As PropertyInfo)
        owner = analysisTechnique
        Me.property = [property]
    End Sub

    Public Property Value As Object
        Get
            Return [property].GetValue(owner, Nothing)
        End Get
        Set(ByVal value As Object)
            [property].SetValue(owner, value, Nothing)
        End Set
    End Property

    Public ReadOnly Property Type As Type
        Get
            Return [property].PropertyType
        End Get
    End Property

    Public ReadOnly Property Name As String
        Get
            Return [property].Name
        End Get
    End Property

    Public ReadOnly Property StringValue As String
        Get
            Return Converter.ConvertToString(Value)
        End Get
    End Property

    Public ReadOnly Property Converter As TypeConverter
        Get
            Return (New CSharpCodeProvider).GetConverter([property].PropertyType)
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return Name
    End Function
End Class
