Imports System.Collections.ObjectModel

Class Application

    Private Sub Application_DispatcherUnhandledException(ByVal sender As Object, ByVal e As System.Windows.Threading.DispatcherUnhandledExceptionEventArgs) Handles Me.DispatcherUnhandledException
        If ShowInfoBox("QuickTrader has encountered an error." & vbNewLine & vbNewLine & "Message: " & e.Exception.Message, Nothing, "OK", "Details") = 1 Then
            ShowInfoBox("A " & e.GetType.Name & " occurred " & e.Exception.StackTrace, Nothing)
        End If
        e.Handled = True
    End Sub
    Private Sub SetSetting(ByVal settingName As String)
        For Each chart In Charts
            chart.ChangeSetting(settingName)
        Next
    End Sub
    Private _isFirstOpen As Boolean
    Public ReadOnly Property IsFirstOpen As Boolean
        Get
            Return _isFirstOpen
        End Get
    End Property
    Private _workspaceBackgroundColor As Color = Colors.DimGray
    Public Property WorkspaceBackgroundColor As Color
        Get
            Return _workspaceBackgroundColor
        End Get
        Set(ByVal value As Color)
            Dim prev = _workspaceBackgroundColor
            _workspaceBackgroundColor = value
            If prev <> value Then
                For Each desktop In Desktops
                    For Each workspace In desktop.Workspaces
                        workspace.Background = New SolidColorBrush(value)
                    Next
                Next
            End If
        End Set
    End Property
    Private _showMousePopups As Boolean = True
    Public Property ShowMousePopups As Boolean
        Get
            Return _showMousePopups
        End Get
        Set(ByVal value As Boolean)
            Dim prev = _showMousePopups
            _showMousePopups = value
            If prev <> value Then SetSetting("ShowMousePopups")
        End Set
    End Property
    Private _chartBorderWidth As Double = 1.5
    Public Property ChartBorderWidth As Double
        Get
            Return _chartBorderWidth
        End Get
        Set(ByVal value As Double)
            Dim prev = _chartBorderWidth
            _chartBorderWidth = value
            If prev <> value Then SetSetting("ChartBorderWidth")
        End Set
    End Property
    Private _fineChartResizeValue As Double = 5
    Public Property FineChartResizeValue As Double
        Get
            Return _fineChartResizeValue
        End Get
        Set(ByVal value As Double)
            Dim prev = _fineChartResizeValue
            _fineChartResizeValue = value
            If prev <> value Then SetSetting("FineChartResizeValue")
        End Set
    End Property
    Private _coarseChartResizeValue As Double = 20
    Public Property CoarseChartResizeValue As Double
        Get
            Return _coarseChartResizeValue
        End Get
        Set(ByVal value As Double)
            Dim prev = _coarseChartResizeValue
            _coarseChartResizeValue = value
            If prev <> value Then SetSetting("CoarseChartResizeValue")
        End Set
    End Property

    Private _barWidth As Double = 1
    Public Property BarWidth As Double
        Get
            Return _barWidth
        End Get
        Set(ByVal value As Double)
            Dim prev = _barWidth
            _barWidth = value
            If prev <> value Then SetSetting("BarWidth")
        End Set
    End Property
    Private _tickWidth As Double = 0.5
    Public Property TickWidth As Double
        Get
            Return _tickWidth
        End Get
        Set(ByVal value As Double)
            Dim prev = _tickWidth
            _tickWidth = value
            If prev <> value Then SetSetting("TickWidth")
        End Set
    End Property
    Private _hideLogWindow As Boolean = True
    Public Property HideLogWindow As Boolean
        Get
            Return _hideLogWindow
        End Get
        Set(ByVal value As Boolean)
            Dim prev = _hideLogWindow
            _hideLogWindow = value
            'If prev <> value Then SetSetting("HideLogWindow")
        End Set
    End Property
    Private _desktopScaleFactor As Double = 1
    Public Property DesktopScaleFactor As Double
        Get
            Return _desktopScaleFactor
        End Get
        Set(ByVal value As Double)
            Dim prev = _hideLogWindow
            _desktopScaleFactor = value
            If prev <> value Then
                For Each desktop In Desktops
                    desktop.SetScalingFactor(value)
                Next
            End If
        End Set
    End Property

    Private Sub Application_Exit(ByVal sender As Object, ByVal e As System.Windows.ExitEventArgs) Handles Me.Exit
        MsgBox("")
    End Sub

    Public Sub SaveSettings()
        WriteSetting(GLOBAL_CONFIG_FILE, "ShowMousePopups", ShowMousePopups, False)
        WriteSetting(GLOBAL_CONFIG_FILE, "TickWidth", TickWidth, False)
        WriteSetting(GLOBAL_CONFIG_FILE, "BarWidth", BarWidth, False)
        WriteSetting(GLOBAL_CONFIG_FILE, "ChartBorderWidth", ChartBorderWidth, False)
        WriteSetting(GLOBAL_CONFIG_FILE, "FineChartResizeValue", FineChartResizeValue, False)
        WriteSetting(GLOBAL_CONFIG_FILE, "CoarseChartResizeValue", CoarseChartResizeValue, False)
        WriteSetting(GLOBAL_CONFIG_FILE, "WorkspaceBackgroundColor", (New ColorConverter).ConvertToString(WorkspaceBackgroundColor), False)
        WriteSetting(GLOBAL_CONFIG_FILE, "HideLogWindow", (New BooleanConverter).ConvertToString(HideLogWindow), False)
        WriteSetting(GLOBAL_CONFIG_FILE, "DesktopScaleFactor", DesktopScaleFactor, True)
    End Sub
    Public Property EfficiencyTimeStamp As DateTime
    Private Sub Application_Startup(ByVal sender As Object, ByVal e As System.Windows.StartupEventArgs) Handles Me.Startup
        EfficiencyTimeStamp = DateTime.Now
        AddTheme("Dark", "Dark", "#FF313031")
        AddTheme("Light", "Light", "#FFADAEAD")
        AddTheme("Silver", "Silver", "#FFEFEFEF")
        'If Not File.Exists("dll") Then
        '    ShowInfoBox("Could not find file 'dll'. This application needs this library to run.", Nothing)
        '    End
        'End If
        Dim count As Integer
        For Each item In Process.GetProcesses
            If item.ProcessName = "QuickTrader" Then
                count += 1
            End If
        Next
        If count >= 2 Then End
        If GetSetting("QuickTrader", "Version", "Version", "[null]") = "[null]" Then _isFirstOpen = True

        Dim ver As String = GetSetting("QuickTrader", "Version", "Version", "")
        If ver <> Version And Not IsFirstOpen Then
            Dim win As New NewFeaturesWindow
            win.ShowDialog()
            SaveSetting("QuickTrader", "Version", "Version", Version)
        End If
        If IsFirstOpen Then
            'ShowInfoBox("QuickTrader is a analytical charting program for market data. To get started, press ALT to access the menu.", Nothing)
        End If
        CType(Me.FindResource("splashScreen"), SplashWindow).Show("Loading...")
        ShowMousePopups = ReadSetting(GLOBAL_CONFIG_FILE, "ShowMousePopups", ShowMousePopups)
        TickWidth = ReadSetting(GLOBAL_CONFIG_FILE, "TickWidth", TickWidth)
        BarWidth = ReadSetting(GLOBAL_CONFIG_FILE, "BarWidth", BarWidth)
        ChartBorderWidth = ReadSetting(GLOBAL_CONFIG_FILE, "ChartBorderWidth", ChartBorderWidth)
        FineChartResizeValue = ReadSetting(GLOBAL_CONFIG_FILE, "FineChartResizeValue", FineChartResizeValue)
        CoarseChartResizeValue = ReadSetting(GLOBAL_CONFIG_FILE, "CoarseChartResizeValue", CoarseChartResizeValue)
        WorkspaceBackgroundColor = ColorConverter.ConvertFromString(ReadSetting(GLOBAL_CONFIG_FILE, "WorkspaceBackgroundColor", (New ColorConverter).ConvertToString(WorkspaceBackgroundColor)))
        HideLogWindow = (New BooleanConverter).ConvertFromString(ReadSetting(GLOBAL_CONFIG_FILE, "HideLogWindow", (New BooleanConverter).ConvertToString(HideLogWindow)))
        DesktopScaleFactor = ReadSetting(GLOBAL_CONFIG_FILE, "DesktopScaleFactor", DesktopScaleFactor)
    End Sub

    Public ReadOnly Property ThemeNames As List(Of String)
        Get
            Dim list As New List(Of String)
            For i = 0 To Resources.Values.Count - 1
                If TypeOf Resources.Values(i) Is ResourceDictionary Then
                    list.Add(Resources.Keys(i))
                End If
            Next
            list.Add("Classic")
            Return list
        End Get
    End Property

    Private Sub AddTheme(ByVal name As String, ByVal fileName As String, ByVal hexBackgroundColor As String)
        Dim theme As New ResourceDictionary
        theme.Source = New Uri("Themes/" & fileName & ".xaml", UriKind.Relative)
        theme.Add("ThemeBackground", (New BrushConverter).ConvertFromString(hexBackgroundColor))
        Resources.Add(name, theme)
    End Sub
    Public ReadOnly Property SplashWindow As SplashWindow
        Get
            Return CType(Me.FindResource("splashScreen"), SplashWindow)
        End Get
    End Property
    Public ReadOnly Property Charts As ReadOnlyCollection(Of Chart)
        Get
            Dim lst As New List(Of Chart)
            For Each desktop In Desktops
                For Each workspace In desktop.Workspaces
                    For Each chart In workspace.Charts
                        lst.Add(chart)
                    Next
                Next
            Next
            Return New ReadOnlyCollection(Of Chart)(lst)
        End Get
    End Property
    Public ReadOnly Property Desktops As ReadOnlyCollection(Of DesktopWindow)
        Get
            Dim lst As New List(Of DesktopWindow)
            For Each item In MyBase.Windows
                If TypeOf item Is DesktopWindow Then
                    lst.Add(item)
                End If
            Next
            Return New ReadOnlyCollection(Of DesktopWindow)(lst)
        End Get
    End Property
    Public Overloads ReadOnly Property MasterWindow As MasterWindow
        Get
            For Each item In MyBase.Windows
                If TypeOf item Is MasterWindow Then
                    Return item
                End If
            Next
            Return Nothing
        End Get
    End Property

End Class
