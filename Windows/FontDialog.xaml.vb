Public Class FontDialog

    Public Sub New()
        InitializeComponent()
        For Each item In GetType(FontWeights).GetProperties
            weights.Add(item.Name, item.GetValue(item, Nothing))
            lstWeight.Items.Add(item.Name)
        Next
        For Each item In GetType(FontStyles).GetProperties
            styles.Add(item.Name, item.GetValue(item, Nothing))
            lstStyle.Items.Add(item.Name)
        Next
        For Each item In Fonts.SystemFontFamilies
            If item.ToString <> "BwSymbol" Then ' This font gets an error
                families.Add(item.ToString, item)
                lstFont.Items.Add(item.ToString)
            End If
        Next
        lstFont.SelectedIndex = 0
        lstSize.ItemsSource = {8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 44, 72}
        shadowEffect.BlurRadius = 3
        shadowEffect.Direction = -35
    End Sub
    Private _buttonOKPressed As Boolean
    Public ReadOnly Property ButtonOKPressed As Boolean
        Get
            Return _buttonOKPressed
        End Get
    End Property
    Public Property BackgroundPreviewColor As Color
        Get
            Return CType(rectPreviewBackground.Fill, SolidColorBrush).Color
        End Get
        Set(ByVal value As Color)
            rectPreviewBackground.Fill = New SolidColorBrush(value)
        End Set
    End Property
    Public Property Font As Font
        Get
            Dim _font As New Font
            _font.Brush = txtPreview.Foreground
            _font.Effect = txtPreview.Effect
            _font.FontFamily = txtPreview.FontFamily
            _font.FontSize = txtPreview.FontSize
            _font.FontStyle = txtPreview.FontStyle
            _font.FontWeight = txtPreview.FontWeight
            _font.Transform = txtPreview.LayoutTransform
            Return _font
        End Get
        Set(ByVal value As Font)
            If TypeOf value.Brush Is SolidColorBrush Then
                cboColor.Color = CType(value.Brush, SolidColorBrush).Color
            End If
            If TypeOf value.Effect Is Effects.BlurEffect AndAlso CType(value.Effect, Effects.BlurEffect).Radius <= sldBlur.Maximum Then
                sldBlur.Value = CType(value.Effect, Effects.BlurEffect).Radius
                chkBlur.IsChecked = True
                chkShadow.IsChecked = False
            ElseIf TypeOf value.Effect Is Effects.DropShadowEffect AndAlso CType(value.Effect, Effects.DropShadowEffect).ShadowDepth <= sldShadow.Maximum Then
                sldShadow.Value = CType(value.Effect, Effects.DropShadowEffect).ShadowDepth
                chkShadow.IsChecked = True
                chkBlur.IsChecked = False
            End If
            sldRotation.Value = value.Transform.Angle Mod 360
            lstFont.SelectedItem = value.FontFamily.Source
            txtSize.Text = value.FontSize
            lstStyle.SelectedItem = value.FontStyle.ToString
            lstWeight.SelectedItem = value.FontWeight.ToString
        End Set
    End Property

    Private weights As New Dictionary(Of String, FontWeight)
    Private styles As New Dictionary(Of String, FontStyle)
    Private colors As New Dictionary(Of String, Color)
    Private families As New Dictionary(Of String, FontFamily)
    Private Sub txtSize_PreviewKeyDown(ByVal sender As System.Object, ByVal e As System.Windows.Input.KeyEventArgs)
        Select Case e.Key
            Case Key.Decimal, Key.OemPeriod
                If txtSize.Text.Contains(".") Then e.Handled = True
            Case Key.D1, Key.D2, Key.D3, Key.D4, Key.D5, Key.D6, Key.D7, Key.D8, Key.D9, Key.D0,
                Key.NumPad0, Key.NumPad1, Key.NumPad2, Key.NumPad3, Key.NumPad4, Key.NumPad5, Key.NumPad6, Key.NumPad7, Key.NumPad8, Key.NumPad9,
                Key.Back, Key.Delete, Key.Right, Key.Left, Key.Up, Key.Down, Key.RightAlt, Key.RightCtrl, Key.RightShift, Key.LeftAlt, Key.LeftCtrl, Key.LeftShift,
                Key.Home, Key.End, Key.Insert, Key.NumLock, Key.PrintScreen, Key.LWin, Key.RWin, Key.CapsLock, Key.Escape
            Case Else
                e.Handled = True
        End Select
    End Sub
    Private Sub txtSize_TextChanged(ByVal sender As System.Object, ByVal e As System.Windows.Controls.TextChangedEventArgs)
        lstSize.SelectedItem = Nothing
        For Each item In lstSize.Items
            If CStr(item) = txtSize.Text Then lstSize.SelectedItem = item
        Next
        If txtSize.Text <> "" AndAlso txtSize.Text > 1 Then
            txtPreview.FontSize = txtSize.Text
        ElseIf lstSize.SelectedIndex <> -1 Then
            txtPreview.FontSize = lstSize.SelectedItem
        Else
            txtPreview.FontSize = 10
            lstSize.SelectedItem = 10
        End If
    End Sub
    Private Sub lstFont_SelectionChanged(ByVal sender As System.Object, ByVal e As System.Windows.Controls.SelectionChangedEventArgs)
        If lstFont.SelectedIndex <> -1 Then txtPreview.FontFamily = families(lstFont.SelectedItem)
    End Sub
    Private Sub lstWeight_SelectionChanged(ByVal sender As System.Object, ByVal e As System.Windows.Controls.SelectionChangedEventArgs)
        If lstWeight.SelectedIndex <> -1 Then txtPreview.FontWeight = weights(lstWeight.SelectedItem)
    End Sub
    Private Sub lstStyle_SelectionChanged(ByVal sender As System.Object, ByVal e As System.Windows.Controls.SelectionChangedEventArgs)
        If lstStyle.SelectedIndex <> -1 Then txtPreview.FontStyle = styles(lstStyle.SelectedItem)
    End Sub
    Private Sub lstSize_SelectionChanged(ByVal sender As System.Object, ByVal e As System.Windows.Controls.SelectionChangedEventArgs)
        If lstSize.SelectedIndex <> -1 Then
            txtPreview.FontSize = lstSize.SelectedItem
            txtSize.Text = lstSize.SelectedItem
        End If
    End Sub
    Private Sub cboColor_ColorChanged(ByVal sender As Object, ByVal e As RoutedEventArgs) Handles cboColor.ColorChanged
        If cboColor.HasColor Then
            txtPreview.Foreground = New SolidColorBrush(cboColor.Color)
            shadowEffect.Color = cboColor.Color
        End If
    End Sub

    Dim blurEffect As New Effects.BlurEffect
    Dim shadowEffect As New Effects.DropShadowEffect
    Private Sub sldShadow_ValueChanged(ByVal sender As System.Object, ByVal e As System.Windows.RoutedPropertyChangedEventArgs(Of System.Double))
        shadowEffect.ShadowDepth = sldShadow.Value
    End Sub
    Private Sub sldBlur_ValueChanged(ByVal sender As System.Object, ByVal e As System.Windows.RoutedPropertyChangedEventArgs(Of System.Double))
        blurEffect.Radius = sldBlur.Value
    End Sub
    Private Sub sldRotation_ValueChanged(ByVal sender As System.Object, ByVal e As System.Windows.RoutedPropertyChangedEventArgs(Of System.Double))
        rotTransform.Angle = sldRotation.Value
    End Sub
    Private Sub chkEffect_CheckedChanged(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        If e.Source Is chkBlur And chkBlur.IsChecked Then
            chkShadow.IsChecked = False
            txtPreview.Effect = blurEffect
        ElseIf e.Source Is chkShadow And chkShadow.IsChecked Then
            chkBlur.IsChecked = False
            txtPreview.Effect = shadowEffect
        Else
            txtPreview.Effect = Nothing
        End If
    End Sub

    Private Sub btnOK_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        _buttonOKPressed = True
        Close()
    End Sub
    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Close()
    End Sub
End Class

