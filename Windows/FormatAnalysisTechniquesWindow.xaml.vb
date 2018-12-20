Imports System.Reflection
Imports System.ComponentModel
Imports Microsoft.CSharp
Imports QuickTrader.AnalysisTechniques
Imports System.Collections.ObjectModel

Public Class FormatAnalysisTechniquesWindow
    Private Class AnalysisTechniqueListBoxItemInformation
        Public Property AnalysisTechniqueInformation As AnalysisTechniqueInformation
        Public Property IsDirty As Boolean
        Public Property IsDefault As Boolean = True
        Public Property SetForAllSettings As New Dictionary(Of String, Boolean)
        Public Property SetForAll As Boolean = True
    End Class
    Friend chart As Chart
    Private techniques As New List(Of AnalysisTechniqueListBoxItemInformation)
    Public Sub New(ByVal chart As Chart)
        InitializeComponent()
        Me.chart = chart
        For Each analysisTechnique In chart.AnalysisTechniques
            AddAnalysisTechnique(analysisTechnique)
        Next
		Owner = chart.DesktopWindow
		cboProfiles.Items.Add("Preset 1")
		cboProfiles.Items.Add("Preset 2")
		cboProfiles.Items.Add("Preset 3")
		cboProfiles.SelectedIndex = 0
        ShowDialog()
    End Sub
    Private Sub lstAnalysisTechniques_SelectionChanged(ByVal sender As System.Object, ByVal e As System.Windows.Controls.SelectionChangedEventArgs)
        grdBottomOptions.Visibility = Windows.Visibility.Hidden
        grdMessage.Visibility = Windows.Visibility.Hidden
        grdProperties.Children.Clear()
        grdProperties.RowDefinitions.Clear()
        If lstAnalysisTechniques.SelectedItem IsNot Nothing Then
            If lstAnalysisTechniques.SelectedItem.Tag.AnalysisTechniqueInformation.Identifier.Substring(0, 2) = "f-" Then
                btnEditSource.Visibility = Windows.Visibility.Visible
            Else
                btnEditSource.Visibility = Windows.Visibility.Collapsed
            End If
            grdBottomOptions.Visibility = Windows.Visibility.Visible

            Dim splitter As New GridSplitter With {.Width = 3, .Focusable = False, .Background = Background, .HorizontalAlignment = Windows.HorizontalAlignment.Center, .VerticalAlignment = Windows.VerticalAlignment.Stretch, .ResizeDirection = GridResizeDirection.Columns}
            Grid.SetColumn(splitter, 2)
            Grid.SetRowSpan(splitter, 2)
            grdProperties.Children.Add(splitter)

            Dim techniqueIndex As Integer, propertyIndex As Integer
            For Each technique In techniques
                If lstAnalysisTechniques.SelectedItem.Tag Is technique Then
                    txtInfo.Text = technique.AnalysisTechniqueInformation.AnalysisTechnique.Description
                    'technique.IsEnabled = lstAnalysisTechniques.SelectedItem.Content.IsChecked
                    propertyIndex = 0
                    For Each prop In technique.AnalysisTechniqueInformation.AnalysisTechnique.GetType.GetProperties
                        For Each attribute In prop.GetCustomAttributes(False)
                            If TypeOf attribute Is InputAttribute Then
                                Dim input As InputAttribute = CType(attribute, InputAttribute)

                                grdProperties.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Auto)})

                                Grid.SetRowSpan(splitter, Grid.GetRowSpan(splitter) + 1)

                                If propertyIndex = 0 And Not input.HasCategoryName Then

                                    Dim header As New TextBlock With {.TextWrapping = TextWrapping.Wrap, .Margin = New Thickness(3, 5, 3, 5), .HorizontalAlignment = Windows.HorizontalAlignment.Left, .FontSize = FontSize + 3, .FontWeight = FontWeights.Bold}
                                    header.Text = "General"
                                    Grid.SetRow(header, grdProperties.RowDefinitions.Count - 1)
                                    Grid.SetColumn(header, 1)
                                    Grid.SetColumnSpan(header, 3)
                                    grdProperties.Children.Add(header)

                                    grdProperties.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Auto)})

                                ElseIf input.HasCategoryName Then

                                    Dim header As New TextBlock With {.TextWrapping = TextWrapping.Wrap, .Margin = New Thickness(3, 5, 3, 5), .HorizontalAlignment = Windows.HorizontalAlignment.Left, .FontSize = FontSize + 3, .FontWeight = FontWeights.Bold}
                                    header.Text = input.CategoryName
                                    Grid.SetRow(header, grdProperties.RowDefinitions.Count - 1)
                                    Grid.SetColumn(header, 1)
                                    Grid.SetColumnSpan(header, 3)
                                    grdProperties.Children.Add(header)

                                End If

                                grdProperties.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Auto)})

                                Grid.SetRowSpan(splitter, Grid.GetRowSpan(splitter) + 1)

                                Dim chkCopy As New CheckBox With {.Content = "", .Margin = New Thickness(3, 3, 0, 3), .Tag = prop.Name, .HorizontalAlignment = Windows.HorizontalAlignment.Center, .VerticalAlignment = Windows.VerticalAlignment.Stretch, .Focusable = False}
                                chkCopy.IsChecked = technique.SetForAllSettings(prop.Name)

                                AddHandler chkCopy.Checked, Sub(s As Object, e2 As EventArgs)
                                                                DirectCast(lstAnalysisTechniques.SelectedItem.Tag, QuickTrader.FormatAnalysisTechniquesWindow.AnalysisTechniqueListBoxItemInformation).SetForAllSettings(s.Tag) = s.IsChecked
                                                            End Sub
                                AddHandler chkCopy.Unchecked, Sub(s As Object, e2 As EventArgs)
                                                                  DirectCast(lstAnalysisTechniques.SelectedItem.Tag, QuickTrader.FormatAnalysisTechniquesWindow.AnalysisTechniqueListBoxItemInformation).SetForAllSettings(s.Tag) = s.IsChecked
                                                              End Sub
                                Grid.SetRow(chkCopy, grdProperties.RowDefinitions.Count - 1)
                                Grid.SetColumn(chkCopy, 0)
                                grdProperties.Children.Add(chkCopy)

                                Dim lbl As New TextBlock With {.TextWrapping = TextWrapping.Wrap, .Margin = New Thickness(3), .HorizontalAlignment = Windows.HorizontalAlignment.Left, .VerticalAlignment = Windows.VerticalAlignment.Center, .Focusable = False}
                                lbl.Text = Spacify(prop.Name)
                                Grid.SetRow(lbl, grdProperties.RowDefinitions.Count - 1)
                                Grid.SetColumn(lbl, 1)
                                grdProperties.Children.Add(lbl)

                                Dim optionControl As FrameworkElement
                                Dim converter As TypeConverter = (New CSharpCodeProvider).GetConverter(prop.PropertyType)
                                If prop.PropertyType = GetType(Boolean) Then
                                    Dim chkOption As New CheckBox With {.Margin = New Thickness(3), .HorizontalAlignment = Windows.HorizontalAlignment.Left, .VerticalContentAlignment = Windows.VerticalAlignment.Center, .VerticalAlignment = Windows.VerticalAlignment.Center}
                                    chkOption.IsChecked = prop.GetValue(technique.AnalysisTechniqueInformation.AnalysisTechnique, Nothing)
                                    chkOption.Tag = New KeyValuePair(Of Integer, PropertyInfo)(techniqueIndex, prop)
                                    AddHandler chkOption.Checked, AddressOf Field_Changed
                                    AddHandler chkOption.Unchecked, AddressOf Field_Changed
                                    optionControl = chkOption
                                ElseIf prop.PropertyType = GetType(Color) Then
                                    Dim grdColorOptions As New Grid
                                    grdColorOptions.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})
                                    grdColorOptions.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Auto)})
                                    Dim txtOption As New TextBox With {.Margin = New Thickness(3), .VerticalContentAlignment = Windows.VerticalAlignment.Center, .VerticalAlignment = Windows.VerticalAlignment.Center}
                                    txtOption.Tag = New KeyValuePair(Of Integer, PropertyInfo)(techniqueIndex, prop)
                                    AddHandler txtOption.TextChanged,
                                        Sub(s2 As Object, e2 As EventArgs)
                                            Try
                                                Dim color = ColorConverter.ConvertFromString(CType(s2, TextBox).Text)
                                                CType(s2, TextBox).Background = New SolidColorBrush(color)
                                                CType(s2, TextBox).Foreground = New SolidColorBrush(GetForegroundColor(color))
                                            Catch

                                            End Try
                                        End Sub

                                    txtOption.Text = converter.ConvertToString(prop.GetValue(technique.AnalysisTechniqueInformation.AnalysisTechnique, Nothing))
                                    txtOption.Background = New SolidColorBrush(prop.GetValue(technique.AnalysisTechniqueInformation.AnalysisTechnique, Nothing))
                                    grdColorOptions.Children.Add(txtOption)
                                    Dim colorChooserButton As New Button With {.Content = "...", .Margin = New Thickness(3), .MinHeight = 23, .MinWidth = 23, .Tag = txtOption}
                                    Grid.SetColumn(colorChooserButton, 1)
                                    AddHandler colorChooserButton.Click,
                                        Sub(sender2 As Object, e2 As EventArgs)
                                            Dim colorPickerWindow As New ColorDialog
                                            colorPickerWindow.Color = ColorConverter.ConvertFromString(CType(CType(sender2, Button).Tag, TextBox).Text)
                                            colorPickerWindow.ShowDialog()
                                            If colorPickerWindow.ButtonOKPressed Then CType(CType(sender2, Button).Tag, TextBox).Text = (New ColorConverter).ConvertToString(colorPickerWindow.Color)
                                        End Sub
                                    grdColorOptions.Children.Add(colorChooserButton)
                                    AddHandler txtOption.TextChanged, AddressOf Field_Changed
                                    optionControl = grdColorOptions
                                ElseIf prop.PropertyType.IsEnum Then
                                    Dim cboOption As New ComboBox With {.Margin = New Thickness(3), .IsEditable = True, .VerticalContentAlignment = Windows.VerticalAlignment.Center, .VerticalAlignment = Windows.VerticalAlignment.Center}
                                    cboOption.ItemsSource = [Enum].GetNames(prop.PropertyType)
                                    cboOption.Text = converter.ConvertToString(prop.GetValue(technique.AnalysisTechniqueInformation.AnalysisTechnique, Nothing))
                                    cboOption.Tag = New KeyValuePair(Of Integer, PropertyInfo)(techniqueIndex, prop)
                                    AddHandler cboOption.SelectionChanged, Sub(s As Object, e2 As EventArgs) CType(s, ComboBox).Text = CType(s, ComboBox).SelectedItem
                                    AddHandler cboOption.KeyDown, AddressOf Field_Changed
                                    AddHandler cboOption.KeyUp, AddressOf Field_Changed
                                    AddHandler cboOption.SelectionChanged, AddressOf Field_Changed
                                    optionControl = cboOption
                                Else
                                    Dim txtOption As New TextBox With {.Margin = New Thickness(3), .VerticalContentAlignment = Windows.VerticalAlignment.Center, .VerticalAlignment = Windows.VerticalAlignment.Center}
                                    txtOption.Text = converter.ConvertToString(prop.GetValue(technique.AnalysisTechniqueInformation.AnalysisTechnique, Nothing))
                                    txtOption.Tag = New KeyValuePair(Of Integer, PropertyInfo)(techniqueIndex, prop)
                                    AddHandler txtOption.PreviewKeyDown, AddressOf TextBox_KeyDown
                                    AddHandler txtOption.GotFocus, AddressOf TextBox_GotFocus
                                    AddHandler txtOption.TextChanged, AddressOf Field_Changed
                                    optionControl = txtOption
                                End If

                                If CType(attribute, InputAttribute).DescriptiveText <> "" Then
                                    optionControl.ToolTip = New ToolTip With {.Content = input.DescriptiveText & " {type: " & prop.PropertyType.Name & "}"}
                                Else
                                    optionControl.ToolTip = New ToolTip With {.Content = "{type: " & prop.PropertyType.Name & "}"}
                                End If
                                Grid.SetRow(optionControl, grdProperties.RowDefinitions.Count - 1)
                                Grid.SetColumn(optionControl, 3)
                                grdProperties.Children.Add(optionControl)



                                propertyIndex += 1
                            End If
                        Next
                    Next
                End If
                techniqueIndex += 1
            Next
            If grdProperties.Children.Count = 1 Then
                DisplayMessage("There are no inputs to display.")
                chkDefault.IsEnabled = False
                chkDefault.IsChecked = False
                chkSetForAll.IsEnabled = False
                chkSetForAll.IsChecked = False
            Else

                chkSetForAll.IsEnabled = True
                chkSetForAll.Content = "Enable Copying Of Settings"
                RemoveHandler chkSetForAll.Checked, AddressOf chkSetForAll_CheckChanged
                RemoveHandler chkSetForAll.Unchecked, AddressOf chkSetForAll_CheckChanged
                chkSetForAll.IsChecked = lstAnalysisTechniques.SelectedItem.Tag.SetForAll
                AddHandler chkSetForAll.Checked, AddressOf chkSetForAll_CheckChanged
                AddHandler chkSetForAll.Unchecked, AddressOf chkSetForAll_CheckChanged
                If IsLoaded Then
                    If chkSetForAll.IsChecked Then
                        colCheckBoxes.Width = New GridLength(1, GridUnitType.Auto)
                    Else
                        colCheckBoxes.Width = New GridLength(0, GridUnitType.Pixel)
                    End If
                End If
                chkDefault.IsEnabled = True
                RemoveHandler chkDefault.Checked, AddressOf chkDefault_CheckChanged
                RemoveHandler chkDefault.Unchecked, AddressOf chkDefault_CheckChanged
                chkDefault.IsChecked = lstAnalysisTechniques.SelectedItem.Tag.IsDefault
                AddHandler chkDefault.Checked, AddressOf chkDefault_CheckChanged
                AddHandler chkDefault.Unchecked, AddressOf chkDefault_CheckChanged
            End If
        Else
            DisplayMessage("Choose an analysis technique on the left, or insert a new one.")
            btnEditSource.Visibility = Windows.Visibility.Collapsed
        End If
    End Sub
    Friend Sub AddAnalysisTechnique(ByVal analysisTechniqueInfo As AnalysisTechniqueInformation, Optional ByVal specifiesAnalysisTechnique As Boolean = True)
        If Not specifiesAnalysisTechnique Then
            analysisTechniqueInfo.AnalysisTechnique = InstantiateAnalysisTechnique(LoadAnalysisTechnique(analysisTechniqueInfo.Identifier), chart)
        End If
        Dim info As New AnalysisTechniqueListBoxItemInformation
        info.AnalysisTechniqueInformation = analysisTechniqueInfo
        For Each prop In info.AnalysisTechniqueInformation.AnalysisTechnique.GetType.GetProperties
            For Each attribute In prop.GetCustomAttributes(False)
                If TypeOf attribute Is InputAttribute Then
                    info.SetForAllSettings.Add(prop.Name, False)
                    Exit For
                End If
            Next
        Next
        techniques.Add(info)
        Dim item As New ListBoxItem
        Dim grd As New Grid
        grd.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Auto)})
        grd.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})
        Dim chk As New CheckBox With {.Margin = New Thickness(2), .IsChecked = info.AnalysisTechniqueInformation.AnalysisTechnique.IsEnabled}
        grd.Children.Add(chk)
        Dim lbl As New TextBlock With {.Margin = New Thickness(2), .Text = info.AnalysisTechniqueInformation.AnalysisTechnique.Name}
        Grid.SetColumn(lbl, 1)
        grd.Children.Add(lbl)
        Dim [sub] =
            Sub(sender As Object, e As EventArgs)
                lstAnalysisTechniques.SelectedItem = sender.Parent.Parent
            End Sub
        AddHandler chk.Checked, [sub]
        AddHandler chk.Unchecked, [sub]
        item.Tag = info
        item.Content = grd
        lstAnalysisTechniques.Items.Add(item)
        lstAnalysisTechniques.SelectedItem = item
    End Sub

    Private Function PullInformation(ByVal control As FrameworkElement) As String
        If TypeOf control Is CheckBox Then
            Return CType(control, CheckBox).IsChecked
        ElseIf TypeOf control Is ComboBox Then
            Return CType(control, ComboBox).Text
        ElseIf TypeOf control Is Grid Then
            Return CType(CType(control, Grid).Children(0), TextBox).Text
        ElseIf TypeOf control Is TextBox Then
            Return CType(control, TextBox).Text
        End If
        Return ""
    End Function
    Private Function GetText(ByVal item As ListBoxItem) As String
        'lstAnalysisTechniques.SelectedItem.Content.Children(1).Text
        Return item.Content.Children(1).Text
    End Function
    Private Function GetIsChecked(ByVal item As ListBoxItem) As String
        'lstAnalysisTechniques.SelectedItem.Content.Children(0).IsChecked
        Return item.Content.Children(0).IsChecked
    End Function
    Private Sub DisplayMessage(ByVal message As String)
        textMessage.Text = message
        textMessage.FontSize = FontSize + 3
        grdMessage.Visibility = Windows.Visibility.Visible
    End Sub

    Private Sub chkUnAll_Click(sender As Object, e As RoutedEventArgs) Handles chkUnAll.Click
        For Each child In grdProperties.Children
            If Grid.GetColumn(child) = 0 AndAlso TypeOf child Is CheckBox Then
                CType(child, CheckBox).IsChecked = False
            End If
        Next
    End Sub
    Private Sub chkAll_Click(sender As Object, e As RoutedEventArgs) Handles chkAll.Click
        For Each child In grdProperties.Children
            If Grid.GetColumn(child) = 0 AndAlso TypeOf child Is CheckBox Then
                CType(child, CheckBox).IsChecked = True
            End If
        Next
    End Sub
    Private Sub chkSetForAll_CheckChanged(ByVal sender As Object, ByVal e As EventArgs)
        If colCheckBoxes IsNot Nothing Then
            If chkSetForAll.IsChecked Then
                colCheckBoxes.Width = New GridLength(1, GridUnitType.Auto)
                grdCheckAll.Visibility = Windows.Visibility.Visible
            Else
                colCheckBoxes.Width = New GridLength(0, GridUnitType.Pixel)
                grdCheckAll.Visibility = Windows.Visibility.Collapsed
            End If
        End If
        If lstAnalysisTechniques.SelectedItem IsNot Nothing Then lstAnalysisTechniques.SelectedItem.Tag.SetForAll = chkSetForAll.IsChecked
    End Sub
    Private Sub chkDefault_CheckChanged(ByVal sender As Object, ByVal e As EventArgs)
        lstAnalysisTechniques.SelectedItem.Tag.IsDefault = chkDefault.IsChecked
    End Sub
    Private Sub TextBox_GotFocus(ByVal sender As Object, ByVal e As EventArgs)
    End Sub
    Private Sub TextBox_KeyDown(ByVal sender As Object, ByVal e As KeyEventArgs)
        Dim textBoxes As New List(Of TextBox)
        For Each item In grdProperties.Children
            If TypeOf item Is TextBox Then textBoxes.Add(item)
        Next
        If e.Key = Key.Down Then
            If sender IsNot textBoxes(textBoxes.Count - 1) Then
                textBoxes(textBoxes.IndexOf(sender) + 1).Focus()
                textBoxes(textBoxes.IndexOf(sender) + 1).SelectionLength = textBoxes(textBoxes.IndexOf(sender) + 1).Text.Length
                textBoxes(textBoxes.IndexOf(sender) + 1).SelectionStart = 0
            End If
        ElseIf e.Key = Key.Up Then
            If sender IsNot textBoxes(0) Then
                textBoxes(textBoxes.IndexOf(sender) - 1).Focus()
                textBoxes(textBoxes.IndexOf(sender) - 1).SelectionLength = textBoxes(textBoxes.IndexOf(sender) - 1).Text.Length
                textBoxes(textBoxes.IndexOf(sender) - 1).SelectionStart = 0
            End If
        End If
    End Sub
    Private Sub Field_Changed(ByVal sender As Object, ByVal e As RoutedEventArgs)
        Dim control As Control = sender
        Try
            'sender.tag.value.setvalue(techniques(sender.tag.key), (New CSharpCodeProvider).GetConverter(sender.tag.value.propertytype).ConvertFromString(sender.text), Nothing)
            'CType(textBox.Tag, KeyValuePair(Of Integer, PropertyInfo)).Value.SetValue(techniques(CType(textBox.Tag, KeyValuePair(Of Integer, PropertyInfo)).Key), (New CSharpCodeProvider).GetConverter(CType(textBox.Tag, KeyValuePair(Of Integer, PropertyInfo)).Value.PropertyType).ConvertFromString(sender.Text), Nothing)
            Dim prop As PropertyInfo = CType(control.Tag, KeyValuePair(Of Integer, PropertyInfo)).Value
            Dim index As Integer = CType(control.Tag, KeyValuePair(Of Integer, PropertyInfo)).Key
            Dim converter As TypeConverter = (New CSharpCodeProvider).GetConverter(prop.PropertyType)
            Dim value As Object = converter.ConvertFromString(PullInformation(control))
            prop.SetValue(techniques(index).AnalysisTechniqueInformation.AnalysisTechnique, value, Nothing)
            control.Effect = Nothing
            techniques(index).IsDirty = True
        Catch ex As Exception
            Dim glow As New Effects.DropShadowEffect
            glow.BlurRadius = 10
            glow.Color = Colors.Red
            glow.ShadowDepth = 0
            control.Effect = glow
            'textBox.ToolTip = "Cannot convert '" & textBox.Text & "'."
        End Try
    End Sub


    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Close()
    End Sub
    Private Sub btnOK_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Close()
        DispatcherHelper.DoEvents()
        For Each item In lstAnalysisTechniques.Items
            Dim technique As AnalysisTechnique = item.Tag.AnalysisTechniqueInformation.AnalysisTechnique
            Dim info As AnalysisTechniqueListBoxItemInformation = item.Tag
            If item.Tag.IsDefault Then
                For Each input In technique.Inputs
                    WriteSetting(GLOBAL_CONFIG_FILE, technique.Name & "." & input.Name, input.StringValue)
                Next
            End If
            Dim prevEnabled As Boolean = item.Tag.AnalysisTechniqueInformation.AnalysisTechnique.IsEnabled
            technique.IsEnabled = GetIsChecked(item)
            Dim doCopy As Boolean = False
            Dim copySettings As New List(Of String)
            For Each setting In info.SetForAllSettings
                If setting.Value Then
                    copySettings.Add(setting.Key)
                    doCopy = True
                End If
            Next
            Dim alreadyApplied As Boolean = False
            If doCopy Then
                alreadyApplied = True
                'If ShowInfoBox("Copy selected settings on '" & technique.Name & "' to all instances?", Me, "Yes", "No") = 0 Then
                For Each win In My.Application.Desktops
                    For Each workspace In win.Workspaces
                        For Each c In workspace.Charts
                            For i = 0 To c.AnalysisTechniques.Count - 1
                                Dim t As AnalysisTechnique = c.AnalysisTechniques(i).AnalysisTechnique
                                If t.GetType = technique.GetType Then
                                    If t IsNot Nothing Then
                                        For Each prop In t.GetType.GetProperties
                                            For Each copyPropName In copySettings
                                                If copyPropName = prop.Name Then
                                                    prop.SetValue(t, technique.GetType.GetProperty(copyPropName).GetValue(technique, Nothing), Nothing)
                                                    Exit For
                                                End If
                                            Next
                                        Next
                                        c.ReApplyAnalysisTechnique(t)
                                    End If
                                End If
                            Next
                        Next
                    Next
                Next
                'End If
            End If
            If chart.AnalysisTechniques.Contains(item.Tag.AnalysisTechniqueInformation) Then
                If item.Tag.IsDirty And Not alreadyApplied Then
                    'For Each prop In item.Value.GetType.GetProperties
                    '    Dim isInput As Boolean = False
                    '    For Each attribute In prop.GetCustomAttributes(False)
                    '        If TypeOf attribute Is InputAttribute Then
                    '            isInput = True
                    '            Exit For
                    '        End If
                    '    Next
                    '    If isInput Then
                    '        prop.SetValue(chart.AnalysisTechniques(item.Key), prop.GetValue(item.Value, Nothing), Nothing)
                    '    End If
                    'Next
                    alreadyApplied = True
                    item.Tag.AnalysisTechniqueInformation.AnalysisTechnique.Reset()
                    chart.ApplyAnalysisTechnique(item.Tag.AnalysisTechniqueInformation.AnalysisTechnique)
                End If
                If prevEnabled <> technique.IsEnabled Then
                    If technique.IsEnabled Then
                        If Not alreadyApplied Then
                            item.Tag.AnalysisTechniqueInformation.AnalysisTechnique.Reset()
                            chart.ApplyAnalysisTechnique(item.Tag.AnalysisTechniqueInformation.AnalysisTechnique)
                        End If
                    Else
                        chart.RemoveAnalysisTechnique(technique)
                    End If
                End If
            Else
                technique.Reset()
                chart.AddAnalysisTechnique(item.Tag.AnalysisTechniqueInformation)
                If technique.IsEnabled = True Then chart.ApplyAnalysisTechnique(technique)
            End If
        Next
    End Sub

    Private Sub FormatAnalysisTechniquesWindow_Closing(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles Me.Closing
        SaveSetting("QuickTrader", "FormatAnalysisTechniquesWindow", "GridCol1Width", col1.Width.Value)
        SaveSetting("QuickTrader", "FormatAnalysisTechniquesWindow", "GridCol2Width", col2.Width.Value)
        SaveSetting("QuickTrader", "FormatAnalysisTechniquesWindow", "PropertyGridCol1Width", grdProperties.ColumnDefinitions(1).Width.Value)
        SaveSetting("QuickTrader", "FormatAnalysisTechniquesWindow", "PropertyGridCol2Width", grdProperties.ColumnDefinitions(3).Width.Value)
        SaveSetting("QuickTrader", "FormatAnalysisTechniquesWindow", "SelectedAnalysisTechnique", lstAnalysisTechniques.SelectedIndex)
    End Sub
    Dim colCheckBoxes As ColumnDefinition
    Private Sub FormatAnalysisTechniquesWindow_Loaded(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs) Handles Me.Loaded
        colCheckBoxes = New ColumnDefinition With {.Width = New GridLength(0, GridUnitType.Pixel)}
        grdProperties.ColumnDefinitions.Add(colCheckBoxes)
        grdProperties.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})
        grdProperties.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Auto)})
        grdProperties.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})
        grdProperties.ColumnDefinitions(1).Width = New GridLength(GetSetting("QuickTrader", "FormatAnalysisTechniquesWindow", "PropertyGridCol1Width", "1"), GridUnitType.Star)
        grdProperties.ColumnDefinitions(3).Width = New GridLength(GetSetting("QuickTrader", "FormatAnalysisTechniquesWindow", "PropertyGridCol2Width", "1"), GridUnitType.Star)
        col1.Width = New GridLength(GetSetting("QuickTrader", "FormatAnalysisTechniquesWindow", "GridCol1Width", "1"), GridUnitType.Star)
        col2.Width = New GridLength(GetSetting("QuickTrader", "FormatAnalysisTechniquesWindow", "GridCol2Width", "1"), GridUnitType.Star)
        lstAnalysisTechniques.SelectedIndex = GetSetting("QuickTrader", "FormatAnalysisTechniquesWindow", "SelectedAnalysisTechnique", -1)
        If lstAnalysisTechniques.SelectedIndex = -1 Then lstAnalysisTechniques_SelectionChanged(Nothing, Nothing)
        chkSetForAll_CheckChanged(Nothing, Nothing)
    End Sub

    Private Sub btnRemove_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Dim items(lstAnalysisTechniques.SelectedItems.Count - 1) As Object
        lstAnalysisTechniques.SelectedItems.CopyTo(items, 0)
        For Each item In items
            techniques.Remove(item.Tag)
            Dim technique As AnalysisTechnique = item.Tag.AnalysisTechniqueInformation.AnalysisTechnique
            If technique.IsEnabled And chart.AnalysisTechniques.Contains(item.Tag.AnalysisTechniqueInformation) Then
                chart.RemoveAnalysisTechnique(technique)
            End If
            technique.IsEnabled = False
            chart.AnalysisTechniques.Remove(item.Tag.AnalysisTechniqueInformation)
            lstAnalysisTechniques.Items.Remove(item)
        Next
    End Sub

    Private Sub btnInsert_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Dim win As New InsertAnalysisTechniqueWindow(Me)
        win.Owner = Me
        win.ShowDialog()
    End Sub

    Private Sub btnCreate_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Dim codeWin As New CodeEditorWindow
        codeWin.Show()
    End Sub


    Private Sub btnEditSource_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Dim win As New CodeEditorWindow
        win.Show()
        win.AddTab()
        win.Refresh()
        win.LoadDoc(Split(lstAnalysisTechniques.SelectedItem.Tag.AnalysisTechniqueInformation.Identifier.Substring(2), ",")(0))
    End Sub

	Private Sub txtProfileHotkey_KeyDown(sender As System.Object, e As System.Windows.Input.KeyEventArgs)
		Dim modifier As String = GetModifierString(Keyboard.Modifiers) + "+"
		If e.Key <> Key.LeftCtrl And e.Key <> Key.RightCtrl And e.Key <> Key.LeftAlt And e.Key <> Key.RightAlt And e.Key <> Key.LeftShift And e.Key <> Key.RightShift Then
			txtProfileHotkey.Text = modifier + [Enum].GetName(GetType(Key), e.Key)
			txtProfileHotkey.Tag = New Hotkey(Keyboard.Modifiers, e.Key)
		Else
			txtProfileHotkey.Text = modifier
		End If
		
	End Sub

	Private Sub btnSavePreset_Click(sender As System.Object, e As System.Windows.RoutedEventArgs)
		If cboProfiles.SelectedIndex <> -1 Then
			Dim technique As AnalysisTechnique = lstAnalysisTechniques.SelectedItem.Tag.AnalysisTechniqueInformation.AnalysisTechnique
			For Each input In technique.Inputs
				WriteSetting(GLOBAL_CONFIG_FILE, cboProfiles.SelectedItem & technique.Name & "." & input.Name, input.StringValue)
			Next
			WriteSetting(GLOBAL_CONFIG_FILE, cboProfiles.SelectedItem & "HotkeyModifier", CInt(CType(txtProfileHotkey.Tag, Hotkey).Modifier))
			WriteSetting(GLOBAL_CONFIG_FILE, cboProfiles.SelectedItem & "Hotkey", CInt(CType(txtProfileHotkey.Tag, Hotkey).Key))
			'ShowInfoBox("Preset saved sucessfully.", Me)
		End If
	End Sub

	Private Sub btnLoadPreset_Click(sender As System.Object, e As System.Windows.RoutedEventArgs)
		If cboProfiles.SelectedIndex <> -1 Then
			Dim technique As AnalysisTechnique = lstAnalysisTechniques.SelectedItem.Tag.AnalysisTechniqueInformation.AnalysisTechnique
			For i = 0 To technique.Inputs.Count - 1
				technique.Inputs(i).Value = technique.Inputs(i).Converter.ConvertFromString(ReadSetting(GLOBAL_CONFIG_FILE, cboProfiles.SelectedItem & technique.Name & "." & technique.Inputs(i).Name, technique.Inputs(i).StringValue))
			Next
			lstAnalysisTechniques_SelectionChanged(Nothing, Nothing)
		End If
	End Sub

	Private Sub cboProfiles_SelectionChanged(sender As System.Object, e As System.Windows.Controls.SelectionChangedEventArgs)
		If cboProfiles.SelectedIndex <> -1 Then
			Dim modifier As String = GetModifierString(ReadSetting(GLOBAL_CONFIG_FILE, cboProfiles.SelectedItem & "HotkeyModifier", ModifierKeys.None))
			Dim hotkey As Key = ReadSetting(GLOBAL_CONFIG_FILE, cboProfiles.SelectedItem & "Hotkey", Key.None)
			If modifier <> "" Then
				txtProfileHotkey.Text = GetModifierString(ReadSetting(GLOBAL_CONFIG_FILE, cboProfiles.SelectedItem & "HotkeyModifier", ModifierKeys.None)) + If(Hotkey = Key.None, "", "+" + [Enum].GetName(GetType(Key), Hotkey))
			Else
				txtProfileHotkey.Text = If(hotkey = Key.None, "", [Enum].GetName(GetType(Key), hotkey))
			End If
		Else
			txtProfileHotkey.Text = ""
		End If
	End Sub

End Class

