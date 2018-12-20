Public Class ListBoxItem
    Inherits System.Windows.Controls.ListBoxItem
    Public Sub New()
    End Sub
    Public Sub New(ByVal header As String)
        Content = header
    End Sub
    Protected Overrides Sub OnVisualParentChanged(ByVal oldParent As System.Windows.DependencyObject)
        MyBase.OnVisualParentChanged(oldParent)
        'If VisualParent 
    End Sub

    Private Sub ListBoxItem_Loaded(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs) Handles Me.Loaded
        'Background = Brushes.Red
        'Dim widthBinding As New Binding
        'widthBinding.RelativeSource = New RelativeSource(RelativeSourceMode.FindAncestor, GetType(ListBox), 1)
        'widthBinding.Path = New PropertyPath(ListBox.ActualWidthProperty)
        'SetBinding(ListBox.WidthProperty, widthBinding)
    End Sub
    Public Overrides Function ToString() As String
        Return Content.ToString
    End Function
End Class
