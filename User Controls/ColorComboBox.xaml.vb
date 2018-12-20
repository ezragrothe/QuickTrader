
<System.Windows.Markup.ContentProperty("UserColors")>
Public Class ColorComboBox1
    Public Sub New()
        InitializeComponent()
    End Sub

    Private otherColors As New List(Of Color)

    Private _showSystemColors As Boolean
    Public Property ShowSystemColors As Boolean
        Get
            Return _showSystemColors
        End Get
        Set(ByVal value As Boolean)
            _showSystemColors = value
        End Set
    End Property
    Private _classicItemStyle As Boolean
    Public Property UseClassicItemStyle As Boolean
        Get
            Return _classicItemStyle
        End Get
        Set(ByVal value As Boolean)
            _classicItemStyle = value
        End Set
    End Property
    Public ReadOnly Property Colors As List(Of Color)
        Get
            Dim _colors As New List(Of Color)
            For Each item In cboColor.Items
                _colors.Add(GetColorFromColorSelectorBoxItem(item, UseClassicItemStyle))
            Next
            Return _colors
        End Get
    End Property

    Public ReadOnly Property HasColor As Boolean
        Get
            Return cboColor.SelectedItem IsNot Nothing
        End Get
    End Property
    Public Property Color As Color
        Get
            If cboColor.SelectedItem IsNot Nothing Then
                Return GetColorFromColorSelectorBoxItem(cboColor.SelectedItem, UseClassicItemStyle)
            Else
                Return Nothing
            End If
        End Get
        Set(ByVal value As Color)
            cboColor.SelectedItem = Nothing
            For Each item In cboColor.Items
                If GetColorFromColorSelectorBoxItem(item, UseClassicItemStyle) = value Then
                    cboColor.SelectedItem = item
                End If
            Next
            If cboColor.SelectedItem Is Nothing Then
                otherColors.Add(value)
                cboColor.Items.Add(NewSelectorBoxColorItem(value, UseClassicItemStyle))
                cboColor.SelectedIndex = cboColor.Items.Count - 1
            End If
        End Set
    End Property
    Private Sub btnColor_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Dim col As Color = ChooseOtherColorForSelectorBox(cboColor, UseClassicItemStyle)
        If col <> Color.FromArgb(0, 0, 0, 0) Then
            cboColor.Items.Add(NewSelectorBoxColorItem(col, UseClassicItemStyle))
            cboColor.SelectedIndex = cboColor.Items.Count - 1
            Dim list As New List(Of Object)
            For Each item In Colors
                list.Add(ColorHelper.GetColorName(item))
            Next
            WriteSetting(GLOBAL_CONFIG_FILE, "UserColors", Join(list.ToArray, ","))
            RaiseEvent UserColorsChanged(Me, CType(e, EventArgs))
        End If
    End Sub
    Private Sub btnDelete_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        If HasColor Then
            cboColor.Items.RemoveAt(cboColor.SelectedIndex)
            Dim list As New List(Of Object)
            For Each item In Colors
                list.Add(ColorHelper.GetColorName(item))
            Next
            WriteSetting(GLOBAL_CONFIG_FILE, "UserColors", Join(list.ToArray, ","))
            RaiseEvent UserColorsChanged(Me, CType(e, EventArgs))
        End If
    End Sub

    Public Event ColorChanged(ByVal sender As Object, ByVal e As SelectionChangedEventArgs)
    Public Event UserColorsChanged(ByVal sender As Object, ByVal e As EventArgs)
    Private Sub cboColor_SelectionChanged(ByVal sender As System.Object, ByVal e As System.Windows.Controls.SelectionChangedEventArgs)
        RaiseEvent ColorChanged(Me, e)
    End Sub

    Private Sub cboColor_DropDownOpened(ByVal sender As Object, ByVal e As System.EventArgs) Handles cboColor.DropDownOpened
        Dim selectionIndex As Integer = cboColor.SelectedIndex
        cboColor.Items.Clear()
        Dim brushConverter As New BrushConverter
        For Each item In Split(ReadSetting(GLOBAL_CONFIG_FILE, "UserColors", ""), ",")
            If item <> "" Then
                cboColor.Items.Add(NewSelectorBoxColorItem(ColorHelper.GetColor(item), UseClassicItemStyle))
            End If
        Next
        For Each item In otherColors
            cboColor.Items.Add(NewSelectorBoxColorItem(item, UseClassicItemStyle))
        Next
        If ShowSystemColors Then
            For Each prop In GetType(Brushes).GetProperties
                cboColor.Items.Add(NewSelectorBoxColorItem(brushConverter.ConvertFromString(prop.Name).Color, UseClassicItemStyle))
            Next
        End If
        cboColor.SelectedIndex = selectionIndex
    End Sub
End Class
