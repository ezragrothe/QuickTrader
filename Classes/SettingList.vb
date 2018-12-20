Public Class SettingList
    Private _settings As New Dictionary(Of String, Setting)
    Default Public Property Setting(ByVal name As String) As Setting
        <DebuggerStepThrough()>
        Get
            If _settings.ContainsKey(name) Then
                Return _settings(name)
            Else
                Throw New KeyNotFoundException("The setting '" & name & "' was not found.")
            End If
        End Get
        Set(ByVal value As Setting)
            _settings(value.Name) = value
        End Set
    End Property
    Public Function Contains(ByVal settingName As String) As Boolean
        For Each item In Settings()
            If item.Name.ToLower = settingName.ToLower Then Return True
        Next
        Return False
    End Function
    Public Sub AddSetting(ByVal setting As Setting)
        _settings.Add(setting.Name, setting)
    End Sub
    Public Sub DeleteSetting(ByVal setting As Setting)
        _settings.Remove(setting.Name)
    End Sub
    Public Function Settings() As List(Of Setting)
        Return _settings.Values.ToList
    End Function

End Class
