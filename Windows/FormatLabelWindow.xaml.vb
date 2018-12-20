Public Class FormatLabelWindow

    Private label As Label
    Private oldFont As Font
    Private oldText As String

    Public Sub New(ByVal label As Label)
        InitializeComponent()
        Me.label = label
        oldFont = label.Font.Clone
        oldText = label.Text
        _font = label.Font.Clone
        rectBackground.Fill = label.Parent.Background
        txtText.FontFamily = label.Font.FontFamily
        txtText.FontSize = label.Font.FontSize
        txtText.FontStyle = label.Font.FontStyle
        txtText.FontWeight = label.Font.FontWeight
        txtText.LayoutTransform = label.Font.Transform
        txtText.Effect = label.Font.Effect
        txtText.Foreground = label.Font.Brush
        txtText.Text = label.Text
        RefreshPresets()
        Owner = label.Parent.DesktopWindow
        ShowDialog()
    End Sub

    Private Sub RefreshPresets()
        Dim selectedIndex As Integer = lstStyles.SelectedIndex
        lstStyles.Items.Clear()
        For i = 1 To label.Parent.PresetLabelStyles.Count
            If label.Parent.PresetLabelStyles(i - 1) Is Nothing Then
                lstStyles.Items.Add("Preset " & i)
            Else
                lstStyles.Items.Add("Preset " & i & " (taken)")
            End If
        Next
        If selectedIndex = -1 Then selectedIndex = 0
        lstStyles.SelectedIndex = selectedIndex
    End Sub
    Private _okPressed As Boolean = False
    Public ReadOnly Property ButtonOKPressed As Boolean
        Get
            Return _okPressed
        End Get
    End Property

    Private _font As New Font
    Public ReadOnly Property Font As Font
        Get
            Return _font
        End Get
    End Property
    Public ReadOnly Property Text As String
        Get
            Return txtText.Text
        End Get
    End Property
    Private Sub btnOK_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        _okPressed = True
        Close()
    End Sub
    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Close()
    End Sub
    Private Sub FormatTextWindow_Closing(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles Me.Closing
        If Not ButtonOKPressed Then
            label.Font = oldFont.Clone
            label.Text = oldText
        End If
    End Sub
    Private Sub FormatTextWindow_Loaded(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs) Handles Me.Loaded
        txtText.Focus()
        txtText.SelectionLength = txtText.Text.Length
    End Sub

    Private Sub txtText_TextChanged(ByVal sender As System.Object, ByVal e As System.Windows.Controls.TextChangedEventArgs)
        UpdateLabel()
    End Sub
    Private Sub UpdateLabel()
        label.Text = txtText.Text
        label.Font = Font.Clone
    End Sub

    Private Sub btnFont_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Dim fontDialog As New FontDialog()
        fontDialog.Font = _font.Clone
        fontDialog.BackgroundPreviewColor = CType(rectBackground.Fill, SolidColorBrush).Color
        fontDialog.Owner = Me
        fontDialog.ShowDialog()
        If fontDialog.ButtonOKPressed Then
            _font = fontDialog.Font.Clone
            txtText.FontFamily = Font.FontFamily
            txtText.FontSize = Font.FontSize
            txtText.FontStyle = Font.FontStyle
            txtText.FontWeight = Font.FontWeight
            txtText.LayoutTransform = Font.Transform
            txtText.Effect = Font.Effect
            txtText.Foreground = Font.Brush
            UpdateLabel()
        End If
    End Sub

    Private Sub btnApplyPreset_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        If lstStyles.SelectedItem IsNot Nothing Then
            Dim presetNum As Integer = lstStyles.SelectedItem.ToString.Substring(7, 1)
            'If lstStyles.SelectedItem.ToString.Contains("(taken)") AndAlso ShowInfoBox("Preset" & presetNum & " has already been assigned. Overwrite?", "Yes", "No") <> InfoBoxResult.Button1 Then
            '    Exit Sub
            'End If
            Dim style As New LabelStyle
            style.Font = Font.Clone
            style.Text = txtText.Text
            If style.Font.Effect IsNot Nothing Then WriteSetting(GLOBAL_CONFIG_FILE, "PresetLabelStyle" & presetNum & ".Font.Effect", style.Font.Effect.ToString)
            If style.Font.Transform IsNot Nothing Then WriteSetting(GLOBAL_CONFIG_FILE, "PresetLabelStyle" & presetNum & ".Font.Transform", (New TransformConverter).ConvertToString(style.Font.Transform))
            WriteSetting(GLOBAL_CONFIG_FILE, "PresetLabelStyle" & presetNum & ".Font.Brush", (New BrushConverter).ConvertToString(style.Font.Brush))
            WriteSetting(GLOBAL_CONFIG_FILE, "PresetLabelStyle" & presetNum & ".Font.FontFamily", (New FontFamilyConverter).ConvertToString(style.Font.FontFamily))
            WriteSetting(GLOBAL_CONFIG_FILE, "PresetLabelStyle" & presetNum & ".Font.FontSize", style.Font.FontSize)
            WriteSetting(GLOBAL_CONFIG_FILE, "PresetLabelStyle" & presetNum & ".Font.FontStyle", (New FontStyleConverter).ConvertToString(style.Font.FontStyle))
            WriteSetting(GLOBAL_CONFIG_FILE, "PresetLabelStyle" & presetNum & ".Font.FontWeight", (New FontWeightConverter).ConvertToString(style.Font.FontWeight))
            WriteSetting(GLOBAL_CONFIG_FILE, "PresetLabelStyle" & presetNum & ".Font.Transform", RotateTransformToString(style.Font.Transform))
            WriteSetting(GLOBAL_CONFIG_FILE, "PresetLabelStyle" & presetNum & ".Font.Effect", EffectToString(style.Font.Effect))
            WriteSetting(GLOBAL_CONFIG_FILE, "PresetLabelStyle" & presetNum & ".Text", style.Text)
            label.Parent.RefreshPresetStyles()
            RefreshPresets()
        End If
    End Sub
End Class
