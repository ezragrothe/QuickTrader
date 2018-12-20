Public Class NewWorkspaceWindow
    Private _okPressed As Boolean
    Public ReadOnly Property ButtonOKPressed As Boolean
        Get
            Return _okPressed
        End Get
    End Property
    Public Property WorkspaceName As String
        Get
            Return txtName.Text
        End Get
        Set(ByVal value As String)
            txtName.Text = value
        End Set
    End Property
    Public Property WorkspaceDescription As String
        Get
            Return txtDescription.Text
        End Get
        Set(ByVal value As String)
            txtDescription.Text = value
        End Set
    End Property
    Public Property WorkspaceFilepath As String
        Get
            Return txtLocation.Text
        End Get
        Set(ByVal value As String)
            txtLocation.Text = value
        End Set
    End Property

    Public Property IsNewWorkspaceDialog As Boolean
        Get
            Return Title = "New Workspace"
        End Get
        Set(ByVal value As Boolean)
            If value Then
                txtName.IsEnabled = True
                Title = "New Workspace"
            Else
                txtName.IsEnabled = False
                Title = "Format Workspace"
            End If
        End Set
    End Property

    Private Sub btnOK_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        If txtName.Text = "" Then
            ShowInfoBox("Please enter a valid workspace name.", Me)
            txtName.Focus()
        ElseIf File.Exists(txtLocation.Text) AndAlso ShowInfoBox("Workspace file already exists. Overwrite?", Me, "Yes", "No") = 0 Or Not File.Exists(txtLocation.Text) Then
            _okPressed = True
            Close()
        End If
    End Sub
    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Close()
    End Sub
    Private path As String = IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, WORKSPACE_FOLDER)
    Private Sub btnLocation_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Dim fileDialog As New Forms.FolderBrowserDialog
        If fileDialog.ShowDialog = Forms.DialogResult.OK Then
            path = fileDialog.SelectedPath
            txtLocation.Text = IO.Path.Combine(path, txtName.Text & ".ws")
        End If
    End Sub

    Private Sub txtName_TextChanged(ByVal sender As System.Object, ByVal e As System.Windows.Controls.TextChangedEventArgs)
        txtLocation.Text = IO.Path.Combine(path, txtName.Text & ".ws")
    End Sub

    Private Sub Window_Loaded(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        txtName.Focus()
    End Sub
End Class
