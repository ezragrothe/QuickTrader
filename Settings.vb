'This class allows you to handle specific events on the settings class:
' The SettingChanging event is raised before a setting's value is changed.
' The PropertyChanged event is raised after a setting's value is changed.
' The SettingsLoaded event is raised after the setting values are loaded.
' The SettingsSaving event is raised before the setting values are saved.
Partial Public NotInheritable Class MySettings

    Private Sub MySettings_SettingChanging(sender As Object, e As System.Configuration.SettingChangingEventArgs) Handles Me.SettingChanging

    End Sub
End Class
