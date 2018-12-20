Public Class FormatTrendLineWindow
    Private _okPressed As Boolean
    Public ReadOnly Property ButtonOKPressed As Boolean
        Get
            Return _okPressed
        End Get
    End Property
    Public ReadOnly Property Pen As Pen
        Get
            Dim _pen As New Pen(New SolidColorBrush(colColors.Color), 1)
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
    Public ReadOnly Property OuterPen As Pen
        Get
            Dim _pen As New Pen(New SolidColorBrush(colColors.Color), 1)
            If IsNumeric(txtOuterWeight.Text) AndAlso CDec(txtOuterWeight.Text) >= 0 Then
                _pen.Thickness = CDec(txtOuterWeight.Text)
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
    Public ReadOnly Property ExtendLeft As Boolean
        Get
            Return chkExtendLeft.IsChecked
        End Get
    End Property
    Public ReadOnly Property ExtendRight As Boolean
        Get
            Return chkExtendRight.IsChecked
        End Get
    End Property

    Private oldLineStyle As New TrendLineStyle
    Private line As TrendLine

    Protected Overrides Sub OnInitialized(ByVal e As System.EventArgs)
        MyBase.OnInitialized(e)

    End Sub
    Public Sub New(ByVal line As TrendLine)
        InitializeComponent()

        Me.line = line

        oldLineStyle.Pen = line.Pen
        oldLineStyle.OuterPen = line.OuterPen
        oldLineStyle.IsRegressionLine = line.IsRegressionLine
        oldLineStyle.ExtendRight = line.ExtendRight
        oldLineStyle.ExtendLeft = line.ExtendLeft
        oldLineStyle.HasParallel = line.HasParallel
        oldLineStyle.LockToEnd = line.LockToEnd
        'grdPreview.Background = line.Parent.Background

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
        colColors.Color = CType(line.Pen.Brush, SolidColorBrush).Color
        For Each item In lstDashStyle.Items
            If CType(item.Content.Children(0).StrokeDashArray, DoubleCollection).Count = line.Pen.DashStyle.Dashes.Count Then
                Dim equals As Boolean = True
                For i = 0 To CType(item.Content.Children(0).StrokeDashArray, DoubleCollection).Count - 1
                    If CType(item.Content.Children(0).StrokeDashArray, DoubleCollection)(i) <> line.Pen.DashStyle.Dashes(i) Then
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
        txtWeight.Text = line.Pen.Thickness
        txtOuterWeight.Text = line.OuterPen.Thickness
        chkExtendLeft.IsChecked = line.ExtendLeft
        chkExtendRight.IsChecked = line.ExtendRight
        chkRegression.IsChecked = line.IsRegressionLine
        chkHasParallel.IsChecked = line.HasParallel
        chkLockToEnd.IsChecked = line.LockToEnd
        RefreshPresets()
        Owner = line.Parent.DesktopWindow
        ShowDialog()
    End Sub

    Private Sub RefreshPresets()
        Dim selectedIndex As Integer = lstStyles.SelectedIndex
        lstStyles.Items.Clear()
        For i = 1 To line.Parent.PresetLineStyles.Count
            If line.Parent.PresetLineStyles(i - 1) Is Nothing Then
                lstStyles.Items.Add("Preset " & i)
            Else
                lstStyles.Items.Add("Preset " & i & " (taken)")
            End If
        Next
        If selectedIndex = -1 Then selectedIndex = 0
        lstStyles.SelectedIndex = selectedIndex
    End Sub

    Private Sub UpdatePreview() Handles Me.Loaded, txtWeight.TextChanged, txtOuterWeight.TextChanged
        If IsLoaded Then
            line.ExtendLeft = chkExtendLeft.IsChecked
            line.ExtendRight = chkExtendRight.IsChecked
            line.IsRegressionLine = chkRegression.IsChecked
            If colColors.HasColor Then
                line.Pen.Brush = New SolidColorBrush(colColors.Color)
            End If
            If lstDashStyle.SelectedItem IsNot Nothing AndAlso IsNumeric(txtWeight.Text) AndAlso CDec(txtWeight.Text) >= 0 Then
                line.Pen.Thickness = CDec(txtWeight.Text)
                line.Pen.DashStyle = New DashStyle({}, 0)
                For Each item In lstDashStyle.SelectedItem.Content.Children(0).StrokeDashArray
                    line.Pen.DashStyle.Dashes.Add(item)
                Next
                line.Pen.DashStyle.Offset = lstDashStyle.SelectedItem.Content.Children(0).StrokeDashOffset
            End If
            line.OuterPen = line.Pen.Clone
            If colColors.HasColor Then
                line.OuterPen.Brush = New SolidColorBrush(colColors.Color)
            End If
            If chkRegression.IsChecked AndAlso IsNumeric(txtOuterWeight.Text) AndAlso CDec(txtOuterWeight.Text) >= 0 AndAlso lstDashStyle.SelectedItem IsNot Nothing Then
                line.OuterPen.Thickness = CDec(txtOuterWeight.Text)
            End If
            line.LockToEnd = chkLockToEnd.IsChecked
            line.HasParallel = chkHasParallel.IsChecked
            line.RefreshVisual()
        End If
    End Sub

    Private Sub FormatTrendLineWindow_Closing(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles Me.Closing
        If Not ButtonOKPressed Then
            line.Pen = oldLineStyle.Pen
            line.OuterPen = oldLineStyle.OuterPen
            line.IsRegressionLine = oldLineStyle.IsRegressionLine
            line.ExtendLeft = oldLineStyle.ExtendLeft
            line.ExtendRight = oldLineStyle.ExtendRight
            line.HasParallel = oldLineStyle.HasParallel
            line.LockToEnd = oldLineStyle.LockToEnd
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

    Private Sub cboColor_ColorChanged(ByVal sender As Object, ByVal e As RoutedEventArgs) Handles colColors.ColorChanged
        UpdatePreview()
    End Sub

    Private Sub btnApplyPreset_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        If lstStyles.SelectedItem IsNot Nothing Then
            Dim presetNum As Integer = lstStyles.SelectedItem.ToString.Substring(7, 1)
            Dim style As New TrendLineStyle
            style.ExtendLeft = ExtendLeft
            style.ExtendRight = ExtendRight
            style.Pen = Pen.Clone
            style.OuterPen = OuterPen.Clone
            style.HasParallel = chkHasParallel.IsChecked
            style.IsRegressionLine = chkRegression.IsChecked
            style.LockToEnd = chkLockToEnd.IsChecked
            WriteSetting(GLOBAL_CONFIG_FILE, "PresetLineStyle" & presetNum & ".Pen.Brush", (New BrushConverter).ConvertToString(style.Pen.Brush))
            WriteSetting(GLOBAL_CONFIG_FILE, "PresetLineStyle" & presetNum & ".Pen.DashCap", style.Pen.DashCap)
            WriteSetting(GLOBAL_CONFIG_FILE, "PresetLineStyle" & presetNum & ".Pen.DashStyle.Dashes", (New DoubleCollectionConverter).ConvertToString(style.Pen.DashStyle.Dashes))
            WriteSetting(GLOBAL_CONFIG_FILE, "PresetLineStyle" & presetNum & ".Pen.DashStyle.Offset", style.Pen.DashStyle.Offset)
            WriteSetting(GLOBAL_CONFIG_FILE, "PresetLineStyle" & presetNum & ".Pen.EndLineCap", style.Pen.EndLineCap)
            WriteSetting(GLOBAL_CONFIG_FILE, "PresetLineStyle" & presetNum & ".Pen.LineJoin", style.Pen.LineJoin)
            WriteSetting(GLOBAL_CONFIG_FILE, "PresetLineStyle" & presetNum & ".Pen.MiterLimit", style.Pen.MiterLimit)
            WriteSetting(GLOBAL_CONFIG_FILE, "PresetLineStyle" & presetNum & ".Pen.StartLineCap", style.Pen.StartLineCap)
            WriteSetting(GLOBAL_CONFIG_FILE, "PresetLineStyle" & presetNum & ".Pen.Thickness", style.Pen.Thickness)
            WriteSetting(GLOBAL_CONFIG_FILE, "PresetLineStyle" & presetNum & ".OuterPen.Thickness", style.OuterPen.Thickness)
            WriteSetting(GLOBAL_CONFIG_FILE, "PresetLineStyle" & presetNum & ".OuterPen.Brush", style.OuterPen.Brush)
            WriteSetting(GLOBAL_CONFIG_FILE, "PresetLineStyle" & presetNum & ".IsRegressionLine", style.IsRegressionLine)
            WriteSetting(GLOBAL_CONFIG_FILE, "PresetLineStyle" & presetNum & ".ExtendLeft", style.ExtendLeft)
            WriteSetting(GLOBAL_CONFIG_FILE, "PresetLineStyle" & presetNum & ".ExtendRight", style.ExtendRight)
            WriteSetting(GLOBAL_CONFIG_FILE, "PresetLineStyle" & presetNum & ".HasParallel", style.HasParallel)
            WriteSetting(GLOBAL_CONFIG_FILE, "PresetLineStyle" & presetNum & ".LockToEnd", style.LockToEnd)

            line.Parent.RefreshPresetStyles()
            RefreshPresets()
        End If
    End Sub
End Class
