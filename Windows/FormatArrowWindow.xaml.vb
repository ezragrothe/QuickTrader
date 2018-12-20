Imports System.ComponentModel

Public Class FormatArrowWindow
    Private _okPressed As Boolean
    Public ReadOnly Property ButtonOKPressed As Boolean
        Get
            Return _okPressed
        End Get
    End Property
    Public ReadOnly Property Pen As Pen
        Get
            Dim _pen As New Pen
            If lstWeight.SelectedItem IsNot Nothing Then _pen.Thickness = lstWeight.SelectedItem.Content.Children(0).StrokeThickness
            _pen.Brush = New SolidColorBrush(cboColor.Color)
            Return _pen
        End Get
    End Property
    Public ReadOnly Property ArrowWidth As Double
        Get
            If IsNumeric(txtWidth.Text) And txtWidth.Text <> "" Then Return txtWidth.Text
            Return 12
        End Get
    End Property
    Public ReadOnly Property ArrowHeight As Double
        Get
            If IsNumeric(txtHeight.Text) And txtHeight.Text <> "" Then Return txtHeight.Text
            Return 4
        End Get
    End Property
    Public ReadOnly Property IsFlipped As Boolean
        Get
            Return chkIsFlipped.IsChecked
        End Get
    End Property
    Private arrow As Arrow
    Private oldArrowStyle As New ArrowStyle
    Public Sub New(ByVal arrow As Arrow)
        InitializeComponent()
        Me.arrow = arrow
        oldArrowStyle.Pen = arrow.Pen.Clone
        oldArrowStyle.IsFlipped = arrow.IsFlipped
        oldArrowStyle.Width = arrow.Width
        oldArrowStyle.Height = arrow.Height
        If TypeOf arrow.Pen.Brush Is SolidColorBrush Then cboColor.Color = CType(arrow.Pen.Brush, SolidColorBrush).Color
        Dim yPos As Double = 7
        For Each weight In {1, 2, 3, 4}
            Dim item As New ListBoxItem
            Dim grd As New Grid
            Dim sampleLine As New Line
            item.Height = 19
            sampleLine.Stroke = Brushes.Black
            sampleLine.X1 = 3
            Dim widthBinding As New Binding
            widthBinding.Source = lstWeight
            widthBinding.Path = New PropertyPath(ListBox.ActualWidthProperty)
            widthBinding.Converter = New MathConverter
            widthBinding.ConverterParameter = "-15"
            sampleLine.SetBinding(Line.X2Property, widthBinding)
            sampleLine.Y1 = yPos + weight / 2
            sampleLine.Y2 = yPos + weight / 2
            sampleLine.StrokeThickness = weight
            grd.Children.Add(sampleLine)
            item.Content = grd
            lstWeight.Items.Add(item)
            If weight = arrow.Pen.Thickness Then lstWeight.SelectedItem = item
        Next
        txtWidth.Text = arrow.Width
        txtHeight.Text = arrow.Height
        chkIsFlipped.IsChecked = arrow.IsFlipped
        RefreshPresets()
        Owner = arrow.Parent.DesktopWindow
        ShowDialog()
    End Sub
    Private Sub RefreshPresets()
        Dim selectedIndex As Integer = lstStyles.SelectedIndex
        lstStyles.Items.Clear()
        For i = 1 To arrow.Parent.PresetArrowStyles.Count
            If arrow.Parent.PresetArrowStyles(i - 1) Is Nothing Then
                lstStyles.Items.Add("Preset " & i)
            Else
                lstStyles.Items.Add("Preset " & i & " (taken)")
            End If
        Next
        If selectedIndex = -1 Then selectedIndex = 0
        lstStyles.SelectedIndex = selectedIndex
    End Sub

    Private Sub btnOK_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        If Not cboColor.HasColor Then
            ShowInfoBox("Please select a color.", Me)
            cboColor.Focus()
        ElseIf lstWeight.SelectedIndex = -1 Then
            ShowInfoBox("Please select a Weight.", Me)
            lstWeight.Focus()
        ElseIf txtWidth.Text = "" Or Not IsNumeric(txtWidth.Text) Then
            ShowInfoBox("Please select a width.", Me)
            txtWidth.Focus()
        ElseIf txtHeight.Text = "" Or Not IsNumeric(txtHeight.Text) Then
            ShowInfoBox("Please select a height.", Me)
            txtHeight.Focus()
        Else
            _okPressed = True
            Close()
        End If
    End Sub
    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Close()
    End Sub
    Private Sub FormatArrowWindow_Closing(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles Me.Closing
        If Not ButtonOKPressed Then
            arrow.IsFlipped = oldArrowStyle.IsFlipped
            arrow.Width = oldArrowStyle.Width
            arrow.Height = oldArrowStyle.Height
            arrow.Pen = oldArrowStyle.Pen.Clone
        End If
    End Sub

    Private Sub UpdatePreview() Handles cboColor.ColorChanged
        If IsLoaded Then
            arrow.Pen = Pen.Clone
            arrow.Width = ArrowWidth
            arrow.Height = ArrowHeight
            arrow.IsFlipped = IsFlipped
            arrow.RefreshVisual()
        End If
    End Sub

    Private Sub cboWeight_SelectionChanged(ByVal sender As System.Object, ByVal e As System.Windows.Controls.SelectionChangedEventArgs)
        If e.AddedItems.Count = 1 Then
            Dim pen As New Pen(New SolidColorBrush(cboColor.Color), e.AddedItems(0).Content.Children(0).StrokeThickness)
            arrow.Pen = pen.Clone
            arrow.RefreshVisual()
        End If
    End Sub

    Private Sub btnApplyPreset_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        If lstStyles.SelectedItem IsNot Nothing Then
            Dim presetNum As Integer = lstStyles.SelectedItem.ToString.Substring(7, 1)
            'If lstStyles.SelectedItem.ToString.Contains("(taken)") AndAlso ShowInfoBox("Preset" & presetNum & " has already been assigned. Overwrite?", "Yes", "No") <> InfoBoxResult.Button1 Then
            '    Exit Sub
            'End If
            WriteSetting(GLOBAL_CONFIG_FILE, "PresetArrowStyle" & presetNum & ".Pen.Brush", (New BrushConverter).ConvertToString(Pen.Brush))
            WriteSetting(GLOBAL_CONFIG_FILE, "PresetArrowStyle" & presetNum & ".Pen.Thickness", (New BrushConverter).ConvertToString(Pen.Thickness))
            WriteSetting(GLOBAL_CONFIG_FILE, "PresetArrowStyle" & presetNum & ".Width", ArrowWidth)
            WriteSetting(GLOBAL_CONFIG_FILE, "PresetArrowStyle" & presetNum & ".Height", ArrowHeight)
            WriteSetting(GLOBAL_CONFIG_FILE, "PresetArrowStyle" & presetNum & ".IsFlipped", (New BooleanConverter).ConvertToString(IsFlipped))
            arrow.Parent.RefreshPresetStyles()
            RefreshPresets()
        End If
    End Sub
End Class
