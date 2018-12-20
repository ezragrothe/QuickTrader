Public Class FormatRectangleWindow
    Private _okPressed As Boolean
    Public ReadOnly Property ButtonOKPressed As Boolean
        Get
            Return _okPressed
        End Get
    End Property
    Public ReadOnly Property Pen As Pen
        Get
            Dim _pen As New Pen(New SolidColorBrush(colBorderColors.Color), 1)
            If IsNumeric(txtWeight.Text) AndAlso CDec(txtWeight.Text) >= 0 Then
                _pen.Thickness = CDec(txtWeight.Text)
            End If
            If lstDashStyle.SelectedItem IsNot Nothing Then
                _pen.DashStyle = New DashStyle(New DoubleCollection, _pen.DashStyle.Offset)
                For Each item In lstDashStyle.SelectedItem.Content.Children(0).StrokeDashArray
                    _pen.DashStyle.Dashes.Add(item)
                Next
            End If
            Return _pen
        End Get
    End Property
    Public ReadOnly Property Fill As Brush
        Get
            Return New SolidColorBrush(colFillColors.Color)
        End Get
    End Property


    Private oldRectStyle As New RectangleStyle
    Private rect As Rectangle

    Protected Overrides Sub OnInitialized(ByVal e As System.EventArgs)
        MyBase.OnInitialized(e)

    End Sub
    Public Sub New(ByVal rect As Rectangle)
        InitializeComponent()

        Me.rect = rect

        oldRectStyle.Fill = rect.Fill
        oldRectStyle.Pen = rect.Pen

        
        Dim num As Double = 15
        Dim dashStyles As New List(Of DashStyle)
        dashStyles.Add(New DashStyle({}, 0))

        '"15,3,4,3,4,3"
        dashStyles.Add(TrendLineDashStyle)
        dashStyles.Add(TrendLineDashDotDashStyle)
        'dashStyles.Add(CStr(num / 2))
        'dashStyles.Add(CStr(num) & "," & CStr(num / 2.5) & "," & CStr(num))
        'dashStyles.Add(CStr(num) & "," & CStr(num / 4))

        Dim yPos As Double = 7
        Dim height As Double = 19
        Dim width As Double = 366
        For Each dashStyle In dashStyles
            Dim item As New ListBoxItem
            Dim grd As New Grid
            Dim sampleLine As New Line
            item.Height = height
            sampleLine.Stroke = Brushes.Black
            sampleLine.X1 = 3
            sampleLine.StrokeDashArray = dashStyle.Dashes
            grd.Children.Add(sampleLine)
            item.Content = grd
            lstDashStyle.Items.Add(item)
            sampleLine.SetBinding(Shapes.Line.X2Property, New Binding With {.Source = lstDashStyle, .Path = New PropertyPath(ListBox.ActualWidthProperty), .Converter = New MathConverter, .ConverterParameter = "-15"})
            sampleLine.SetBinding(Shapes.Line.Y1Property, New Binding With {.RelativeSource = New RelativeSource(RelativeSourceMode.FindAncestor, GetType(ListBoxItem), 1), .Path = New PropertyPath(ListBox.ActualHeightProperty), .Converter = New MathConverter, .ConverterParameter = "/3"})
            sampleLine.SetBinding(Shapes.Line.Y2Property, New Binding With {.RelativeSource = New RelativeSource(RelativeSourceMode.FindAncestor, GetType(ListBoxItem), 1), .Path = New PropertyPath(ListBox.ActualHeightProperty), .Converter = New MathConverter, .ConverterParameter = "/3"})
        Next
        colFillColors.Color = CType(rect.Fill, SolidColorBrush).Color
        colBorderColors.Color = CType(rect.Pen.Brush, SolidColorBrush).Color
        For Each item In lstDashStyle.Items
            If CType(item.Content.Children(0).StrokeDashArray, DoubleCollection).Count = rect.Pen.DashStyle.Dashes.Count Then
                Dim equals As Boolean = True
                For i = 0 To CType(item.Content.Children(0).StrokeDashArray, DoubleCollection).Count - 1
                    If CType(item.Content.Children(0).StrokeDashArray, DoubleCollection)(i) <> rect.Pen.DashStyle.Dashes(i) Then
                        equals = False
                        Exit For
                    End If
                Next
                If equals Then
                    lstDashStyle.SelectedItem = item
                    Exit For
                End If
            End If
        Next
        txtWeight.Text = rect.Pen.Thickness
        RefreshPresets()
        Owner = rect.Parent.DesktopWindow
        ShowDialog()
    End Sub

    Private Sub RefreshPresets()
        Dim selectedIndex As Integer = lstStyles.SelectedIndex
        lstStyles.Items.Clear()
        For i = 1 To rect.Parent.PresetRectangleStyles.Count
            If rect.Parent.PresetRectangleStyles(i - 1) Is Nothing Then
                lstStyles.Items.Add("Preset " & i)
            Else
                lstStyles.Items.Add("Preset " & i & " (taken)")
            End If
        Next
        If selectedIndex = -1 Then selectedIndex = 0
        lstStyles.SelectedIndex = selectedIndex
    End Sub

    Private Sub UpdatePreview() Handles Me.Loaded, txtWeight.TextChanged
        If IsLoaded Then
            If colBorderColors.HasColor Then
                rect.Pen.Brush = New SolidColorBrush(colBorderColors.Color)
            End If
            If lstDashStyle.SelectedItem IsNot Nothing AndAlso IsNumeric(txtWeight.Text) AndAlso CDec(txtWeight.Text) >= 0 Then
                rect.Pen.Thickness = CDec(txtWeight.Text)
                rect.Pen.DashStyle = New DashStyle({}, 0)
                For Each item In lstDashStyle.SelectedItem.Content.Children(0).StrokeDashArray
                    rect.Pen.DashStyle.Dashes.Add(item)
                Next
                rect.Pen.DashStyle.Offset = lstDashStyle.SelectedItem.Content.Children(0).StrokeDashOffset
            End If
            If colFillColors.HasColor Then
                rect.Fill = New SolidColorBrush(colFillColors.Color)
            End If
            rect.RefreshVisual()
        End If
    End Sub

    Private Sub FormatTrendLineWindow_Closing(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles Me.Closing
        If Not ButtonOKPressed Then
            rect.Pen = oldRectStyle.Pen
            rect.Fill = oldRectStyle.Fill
        End If
    End Sub

    Private Sub btnOK_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        _okPressed = True
        Close()
    End Sub

    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Close()
    End Sub

    Private Sub cboDashStyle_SelectionChanged(ByVal sender As System.Object, ByVal e As System.Windows.Controls.SelectionChangedEventArgs)

    End Sub

    Private Sub cboColor_ColorChanged(ByVal sender As Object, ByVal e As RoutedEventArgs) Handles colBorderColors.ColorChanged, colFillColors.ColorChanged
        UpdatePreview()
    End Sub

    Private Sub btnApplyPreset_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        If lstStyles.SelectedItem IsNot Nothing Then
            Dim presetNum As Integer = lstStyles.SelectedItem.ToString.Substring(7, 1)
            Dim style As New RectangleStyle
            style.Pen = Pen.Clone
            style.Fill = Fill
            WriteSetting(GLOBAL_CONFIG_FILE, "PresetRectangleStyle" & presetNum & ".Pen.Brush", (New BrushConverter).ConvertToString(style.Pen.Brush))
            WriteSetting(GLOBAL_CONFIG_FILE, "PresetRectangleStyle" & presetNum & ".Pen.DashCap", style.Pen.DashCap)
            WriteSetting(GLOBAL_CONFIG_FILE, "PresetRectangleStyle" & presetNum & ".Pen.DashStyle.Dashes", (New DoubleCollectionConverter).ConvertToString(style.Pen.DashStyle.Dashes))
            WriteSetting(GLOBAL_CONFIG_FILE, "PresetRectangleStyle" & presetNum & ".Pen.DashStyle.Offset", style.Pen.DashStyle.Offset)
            WriteSetting(GLOBAL_CONFIG_FILE, "PresetRectangleStyle" & presetNum & ".Pen.EndLineCap", style.Pen.EndLineCap)
            WriteSetting(GLOBAL_CONFIG_FILE, "PresetRectangleStyle" & presetNum & ".Pen.LineJoin", style.Pen.LineJoin)
            WriteSetting(GLOBAL_CONFIG_FILE, "PresetRectangleStyle" & presetNum & ".Pen.MiterLimit", style.Pen.MiterLimit)
            WriteSetting(GLOBAL_CONFIG_FILE, "PresetRectangleStyle" & presetNum & ".Pen.StartLineCap", style.Pen.StartLineCap)
            WriteSetting(GLOBAL_CONFIG_FILE, "PresetRectangleStyle" & presetNum & ".Pen.Thickness", style.Pen.Thickness)
            WriteSetting(GLOBAL_CONFIG_FILE, "PresetRectangleStyle" & presetNum & ".Fill", style.Fill)
            rect.Parent.RefreshPresetStyles()
            RefreshPresets()
        End If
    End Sub
End Class
