Public Class SplashWindow
    Public Sub New()
        InitializeComponent()
    End Sub
    Public Sub New(ByVal infoMessage As String)
        InitializeComponent()
        'lblText.Content = infoMessage
        MyBase.Show()
    End Sub
    Dim time As DateTime
    ''' <summary>
    ''' Opens the window with the specified info message and returns without waiting for the newly opened window to close.
    ''' </summary>
    ''' <param name="infoMessage">The message to display.</param>
    Public Overloads Sub Show(ByVal infoMessage As String)
        'lblText.Content = infoMessage
        MyBase.Show()
    End Sub
    Private Sub SplashScreen_Closed(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Closed
        'showinfobox((Now - time).TotalMilliseconds)
    End Sub
    Private Sub SplashScreen_Loaded(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs) Handles Me.Loaded
        time = Now
    End Sub

    Private Sub Storyboard_Completed(ByVal sender As System.Object, ByVal e As System.EventArgs)
        'Close()
    End Sub
End Class
