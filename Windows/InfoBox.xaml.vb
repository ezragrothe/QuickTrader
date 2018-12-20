Public Class InfoBox
    Private _infoBoxButtonIndex As Integer = -1
    Public ReadOnly Property ButtonResultIndex As Integer
        Get
            Return _infoBoxButtonIndex
        End Get
    End Property
    <DebuggerStepThrough()>
    Public Sub New(ByVal text As String, ByVal owner As Window)
        InitializeComponent()
        _infoBoxButtonIndex = -1
        lblText.Content = text
        AddButton("OK")
        CType(grdButtons.Children(0), Button).IsDefault = True
        If owner IsNot Nothing AndAlso owner.IsVisible Then
            Me.Owner = owner
        Else
            ShowInTaskbar = True
        End If
        ShowDialog()
    End Sub
    <DebuggerStepThrough()>
    Public Sub New(ByVal text As String, ByVal owner As Window, topMost As Boolean, ByVal ParamArray buttonTexts() As String)
        Me.Topmost = topMost
        InitializeComponent()
        lblText.Content = text
        If buttonTexts IsNot Nothing Then
            For Each btn In buttonTexts
                AddButton(btn)
            Next
            CType(grdButtons.Children(0), Button).IsDefault = True
            CType(grdButtons.Children(1), Button).IsCancel = True
            grdButtons.Children(0).Focus()
            If owner IsNot Nothing AndAlso owner.IsVisible Then
                Me.Owner = owner
            Else
                ShowInTaskbar = True
            End If
            ShowDialog()
        Else
            Show()
        End If
    End Sub

    Private Sub InfoBox_Loaded(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs) Handles Me.Loaded
        For Each child In grdButtons.Children
            If TypeOf child Is Button AndAlso CType(child, Button).IsDefault Then CType(child, Button).Focus()
        Next
    End Sub

    Private Sub btn_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        _infoBoxButtonIndex = sender.Tag
        Close()
    End Sub
    Private Sub AddButton(ByVal text As String)
        Dim btn As New Button With {.Content = text, .Padding = New Thickness(23, 1, 23, 1), .Margin = New Thickness(3), .MinHeight = 23, .MinWidth = 82, .HorizontalAlignment = Windows.HorizontalAlignment.Center, .VerticalAlignment = Windows.VerticalAlignment.Center}
        btn.Tag = grdButtons.Children.Count
        AddHandler btn.Click, AddressOf btn_Click
        btn.SetValue(Grid.ColumnProperty, grdButtons.Children.Count)
        grdButtons.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Auto)})
        grdButtons.Children.Add(btn)
    End Sub

End Class
