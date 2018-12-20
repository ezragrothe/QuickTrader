Imports System.ComponentModel
Imports Microsoft.CSharp

Public Class Setting

    Private _type As Type
    Private _converter As TypeConverter
    Private _name As String
    Private _hasValue As Boolean
    Private _value As Object

    ''' <summary>
    ''' Creates a value-less ChartSetting object with a name and type.
    ''' </summary>
    Public Sub New(ByVal name As String, ByVal type As Type)
        _type = type
        _converter = (New CSharpCodeProvider).GetConverter(type)
        _name = name
        _hasValue = False
    End Sub
    ''' <summary>
    ''' Creates a ChartSetting object with a name and value.
    ''' </summary>
    Public Sub New(ByVal name As String, ByVal value As Object)
        _type = value.GetType
        _converter = (New CSharpCodeProvider).GetConverter(Type)
        _name = name
        _hasValue = True
        _value = value
    End Sub
    ''' <summary>
    ''' Resets the setting in the specified file.
    ''' </summary>
    Public Sub Reset(ByVal filename As String, ByVal chartName As String)
        ResetSetting(filename, chartName & "." & Name)
    End Sub
    ''' <summary>
    ''' Saves the setting in the specified file. This function works only if the property HasValue is true. Otherwise, use the Save sub-routine that asks for a value object.
    ''' </summary>
    Public Sub Save(ByVal filename As String, ByVal chartName As String)
        If TypeOf Value Is Integer OrElse TypeOf Value Is String OrElse TypeOf Value Is Double OrElse TypeOf Value Is Decimal Then
            If HasValue Then WriteSetting(filename, chartName & "." & Name, CStr(Value), False) Else Throw New ArgumentException("This setting does not have a value.")
        Else
            If HasValue Then WriteSetting(filename, chartName & "." & Name, Converter.ConvertToString(Value), False) Else Throw New ArgumentException("This setting does not have a value.")
        End If
    End Sub
    ''' <summary>
    ''' Saves the setting with the specified value in the specified file.
    ''' </summary>
    Public Sub Save(ByVal filename As String, ByVal chartName As String, ByVal value As Object, Optional writeFile As Boolean = False)
        If TypeOf value Is Integer OrElse TypeOf value Is String OrElse TypeOf value Is Double OrElse TypeOf value Is Decimal Then
            WriteSetting(filename, chartName & "." & Name, CStr(value), writeFile)
        Else
            WriteSetting(filename, chartName & "." & Name, Converter.ConvertToString(value), writeFile)
        End If
    End Sub
    ''' <summary>
    ''' Loads the setting from the specified file and returns the result, as well as setting the Value property if HasValue is true.
    ''' </summary>
    Public Function Load(ByVal filename As String, ByVal chartName As String, ByVal defaultValue As Object) As Object
        Try
            Dim value As Object = Converter.ConvertFromString(ReadSetting(filename, chartName & "." & Name, ReadSetting(GLOBAL_CONFIG_FILE, "DefaultChartStyle." & Name, Converter.ConvertToString(defaultValue))))
            If HasValue Then Me.Value = value
            Return value
        Catch ex As Exception
            Log("Error: " & ex.Message & " '" & filename & "' Attempting to load: " & Name)
            Return Converter.ConvertToString(defaultValue)
        End Try
    End Function
    Public Function IsSettingSaved(ByVal filename As String, ByVal chartName As String) As Boolean
        Return SettingExists(filename, chartName & "." & Name)
    End Function
    Public ReadOnly Property Type As Type
        Get
            Return _type
        End Get
    End Property
    Public ReadOnly Property Converter As TypeConverter
        Get
            Return _converter
        End Get
    End Property
    Public ReadOnly Property HasConverter As Boolean
        Get
            Return Converter IsNot Nothing
        End Get
    End Property
    Public ReadOnly Property Name As String
        Get
            Return _name
        End Get
    End Property
    Public ReadOnly Property StringValue As String
        Get
            Return Converter.ConvertToString(Value)
        End Get
    End Property
    Public Property Value As Object
        Get
            If HasValue Then Return _value Else Return Nothing
        End Get
        Set(ByVal value As Object)
            If value IsNot Nothing AndAlso _value IsNot Nothing AndAlso value.GetType <> _value.GetType Then
                Try
                    _value = value
                    '_value = Converter.ConvertFrom(_value)
                Catch ex As Exception
                    Throw New ArgumentException("You cannot change the value type.")
                    Exit Property
                End Try
            End If
            If HasValue Then
                _value = value
                RaiseEvent ValueChanged(Me, New PropertyChangedEventArgs("Value"))
            Else
                Throw New Exception("This setting does not have a value.")
                Exit Property
            End If
        End Set
    End Property
    Public ReadOnly Property HasValue As Boolean
        Get
            Return _hasValue
        End Get
    End Property
    Public ReadOnly Property IsColorSetting As Boolean
        Get
            Return Type = GetType(Color)
        End Get
    End Property

    Public Event ValueChanged(ByVal sender As Object, ByVal e As PropertyChangedEventArgs)

    Public Overrides Function ToString() As String
        Return Name & ": " & StringValue
    End Function
End Class
