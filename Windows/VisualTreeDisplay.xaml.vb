Public Class VisualTreeDisplay
    Public Sub ShowVisualTree(ByVal element As DependencyObject)
        treeElements.Items.Clear()
        ProcessElement(element, Nothing)
    End Sub
    Private Sub ProcessElement(ByVal element As DependencyObject, ByVal previousItem As TreeViewItem)
        Dim item As New TreeViewItem
        item.Header = element.GetType.Name
        item.IsExpanded = True
        If previousItem Is Nothing Then
            treeElements.Items.Add(item)
        Else
            previousItem.Items.Add(item)
        End If
        For i = 0 To VisualTreeHelper.GetChildrenCount(element) - 1
            ProcessElement(VisualTreeHelper.GetChild(element, i), item)
        Next
    End Sub
End Class
