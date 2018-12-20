Module StaticSettings
    Public ReadOnly Property TrendLineDashStyle As DashStyle
        Get '                                          1.5, 5
            Return New DashStyle(New DoubleCollection({3}), 3)
        End Get
    End Property
    Public ReadOnly Property TrendLineDashDotDashStyle As DashStyle
        Get
            Return New DashStyle(New DoubleCollection({12, 5, 4, 5, 4, 5}), 0)
        End Get
    End Property

    Public ReadOnly Property ApplicationDirectory As String
        Get
            Return AppDomain.CurrentDomain.BaseDirectory
        End Get
    End Property
    Public ReadOnly Property ApplicationSettingsFileLocation As String
        Get
            Return IO.Path.Combine(ApplicationDirectory, "settings\application.chr")
        End Get
    End Property
    Public ReadOnly Property GlobalSettingsFileLocation As String
        Get
            Return IO.Path.Combine(ApplicationDirectory, "settings\global.chr")
        End Get
    End Property
    Public ReadOnly Property DefaultAutoTrendRV As Decimal
        Get
            Return 3
        End Get
    End Property
    'Public ReadOnly Property Menu
End Module
