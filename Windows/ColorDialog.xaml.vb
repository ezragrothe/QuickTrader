Public Class ColorDialog
    Private _color As Color
    Public Property Color As Color
        Get
            Return _color
        End Get
        Set(ByVal value As Color)
            _color = value
            SetColor(value)
        End Set
    End Property
    Private Sub SetColor(ByVal color As Color)
        If IsLoaded Then
            sldRed.Value = color.R
            sldGreen.Value = color.G
            sldBlue.Value = color.B
            sldOpacity.Value = color.A
        End If
    End Sub
    Private _buttonOKPressed As Boolean
    Public ReadOnly Property ButtonOKPressed As Boolean
        Get
            Return _buttonOKPressed
        End Get
    End Property

    Private Sub ColorDialog_Loaded(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs) Handles Me.Loaded
        bdPrevious.Fill = New SolidColorBrush(Color)
        txtPreviousColorName.Text = GetColorName(Color)
        bdPreviousColorGlow.Background = New SolidColorBrush(GetInverseColor(Color))
        txtPreviousColorName.Foreground = New SolidColorBrush(GetForegroundColor(GetInverseColor(Color)))
        SetColor(Color)
    End Sub

    Private Sub btnOK_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        _buttonOKPressed = True
        Color = If(bdCurrent.Fill Is Nothing, Colors.Transparent, CType(bdCurrent.Fill, SolidColorBrush).Color)
        Close()
    End Sub

    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        _buttonOKPressed = False
        Close()
    End Sub

    Private Sub UpdateColor()
        If IsLoaded Then
            bdCurrent.Fill = New SolidColorBrush(Color.FromArgb(sldOpacity.Value, sldRed.Value, sldGreen.Value, sldBlue.Value))
            If btnEdit.IsChecked Then
                txtRed.Text = FormatNumber(sldRed.Value, 0)
                txtGreen.Text = FormatNumber(sldGreen.Value, 0)
                txtBlue.Text = FormatNumber(sldBlue.Value, 0)
                'txtHue.Text = FormatNumber(sldHue.Value, 0)
                'txtSaturation.Text = FormatNumber(sldSaturation.Value, 0)
                'txtLuminosity.Text = FormatNumber(sldLuminosity.Value, 0)
            Else
                txtRed.Text = FormatNumber(sldRed.Value / 255 * 100, 0) & "%"
                txtGreen.Text = FormatNumber(sldGreen.Value / 255 * 100, 0) & "%"
                txtBlue.Text = FormatNumber(sldBlue.Value / 255 * 100, 0) & "%"
                'txtHue.Text = FormatNumber(sldHue.Value / 239 * 100, 0) & "%"
                'txtSaturation.Text = FormatNumber(sldSaturation.Value / 240 * 100, 0) & "%"
                'txtLuminosity.Text = FormatNumber(sldLuminosity.Value / 240 * 100, 0) & "%"
            End If
            txtOpacity.Text = FormatNumber(sldOpacity.Value / 255 * 100, 0) & "%"
            txtNewColorName.Text = GetColorName(Color.FromArgb(sldOpacity.Value, sldRed.Value, sldGreen.Value, sldBlue.Value))
            bdNewColorGlow.Background = New SolidColorBrush(GetInverseColor(Color.FromArgb(255, sldRed.Value, sldGreen.Value, sldBlue.Value)))
            txtNewColorName.Foreground = New SolidColorBrush(GetForegroundColor(GetInverseColor(Color.FromArgb(255, sldRed.Value, sldGreen.Value, sldBlue.Value))))
        End If
    End Sub

    Private Sub btnEdit_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        UpdateColor()
        If btnEdit.IsChecked Then
            txtRed.ClearValue(TextBox.BackgroundProperty)
            txtRed.IsReadOnly = False
            txtBlue.ClearValue(TextBox.BackgroundProperty)
            txtBlue.IsReadOnly = False
            txtGreen.ClearValue(TextBox.BackgroundProperty)
            txtGreen.IsReadOnly = False
        Else
            txtRed.Background = Brushes.Transparent
            txtRed.IsReadOnly = True
            txtGreen.Background = Brushes.Transparent
            txtGreen.IsReadOnly = True
            txtBlue.Background = Brushes.Transparent
            txtBlue.IsReadOnly = True
        End If
    End Sub

    Private Sub bdPrevious_MouseUp(ByVal sender As System.Object, ByVal e As System.Windows.Input.MouseButtonEventArgs)
        Color = CType(bdPrevious.Fill, SolidColorBrush).Color
    End Sub
End Class
