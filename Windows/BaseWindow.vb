Public MustInherit Class BaseWindow
    Inherits Window
    Public Sub New()
        Resources.MergedDictionaries.Add(New ResourceDictionary)
    End Sub
    Public Sub ApplyTheme(ByVal themeName As String)
        If themeName <> "Classic" Then
            Resources.MergedDictionaries(0) = My.Application.Resources(themeName)
            Background = My.Application.Resources(themeName)("ThemeBackground")
        Else
            Resources.MergedDictionaries(0) = New ResourceDictionary
            Background = SystemColors.ControlBrush
        End If
    End Sub
    Public Sub LoadTheme()
        Dim themeName As String = ReadSetting(GLOBAL_CONFIG_FILE, "Theme", "Dark")
        ApplyTheme(themeName)
    End Sub

    Protected ReadOnly MyType As String = Me.GetType.ToString.Substring(Me.GetType.ToString.LastIndexOf(".") + 1)

    Public Property SaveSizeAndPosition As Boolean = True

    Private Sub BaseWindow_Closing(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles Me.Closing
        If SaveSizeAndPosition Then
            SaveSettings()
        End If
    End Sub
    Public Sub SaveSettings()
        SaveSetting("QuickTrader", MyType, "Size", ActualWidth & "," & ActualHeight)
        SaveSetting("QuickTrader", MyType, "Location", Left & "," & Top)
        SaveSetting("QuickTrader", MyType, "WindowState", WindowState.ToString)
    End Sub
    Protected Overrides Sub OnInitialized(ByVal e As System.EventArgs)
        MyBase.OnInitialized(e)
        If GetSetting("QuickTrader", MyType, "Size") <> "" Then
            LoadSettings()
        End If
        LoadTheme()
    End Sub
    Private Sub BaseWindow_Loaded(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs) Handles Me.Loaded
        If GetSetting("QuickTrader", MyType, "Size") = "" Then
            LoadSettings()
        End If
        FontSize = 12
        TextOptions.SetTextFormattingMode(Me, TextFormattingMode.Display)
        If File.Exists("Images/icon.ico") Then
            Icon = New BitmapImage(New Uri("Images/icon.ico", UriKind.Relative))
        End If
    End Sub
    Public Sub LoadSettings()
        If SaveSizeAndPosition Then
            Dim size As String() = GetSetting("QuickTrader", MyType, "Size", ActualWidth & "," & ActualHeight).Split(",")
            Width = size(0)
            Height = size(1)
            Dim location As String() = GetSetting("QuickTrader", MyType, "Location", Left & "," & Top).Split(",")
            Left = location(0)
            Top = location(1)
            WindowState = [Enum].Parse(GetType(System.Windows.WindowState), GetSetting("QuickTrader", MyType, "WindowState", WindowState.ToString))
        End If
    End Sub
End Class
