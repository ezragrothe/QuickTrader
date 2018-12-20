Imports System.Collections.ObjectModel

Public Class ColorChooser

    Public Sub New()
        InitializeComponent()
        RefreshColors()
    End Sub

    Public Property Color As Color
        Get
            For Each child In Children
                If CType(child, ColorRadioButton).IsChecked Then Return CType(CType(child, ColorRadioButton).ColorBrush, SolidColorBrush).Color
            Next
            Return Nothing
        End Get
        Set(ByVal value As Color)
            For Each child In Children
                If CType(CType(child, ColorRadioButton).ColorBrush, SolidColorBrush).Color = value Then
                    CType(child, ColorRadioButton).IsChecked = True
                    Exit Property
                End If
            Next
            Dim colorRad As ColorRadioButton = NewColorRadioButton(value)
            panelMain.Children.Insert(panelMain.Children.Count - 1, colorRad)
            colorRad.IsChecked = True
            SaveColors()
        End Set
    End Property
    Public ReadOnly Property HasColor As Boolean
        Get
            For Each child In Children
                If CType(child, ColorRadioButton).IsChecked Then Return True
            Next
            Return False
        End Get
    End Property
    Public ReadOnly Property Colors As ReadOnlyCollection(Of Color)
        Get
            Dim lst As New List(Of Color)
            For Each child In Children
                lst.Add(CType(CType(child, ColorRadioButton).ColorBrush, SolidColorBrush).Color)
            Next
            Return New ReadOnlyCollection(Of Color)(lst)
        End Get
    End Property
    Public Property AreColorsVisible As Boolean
        Get
            If panelMain.Children.Count > 0 Then Return panelMain.Children(0).Visibility = Windows.Visibility.Visible Else Return True
        End Get
        Set(ByVal value As Boolean)
            For Each child In Children
                If value Then
                    CType(child, ColorRadioButton).Visibility = Windows.Visibility.Visible
                Else
                    CType(child, ColorRadioButton).Visibility = Windows.Visibility.Collapsed
                End If
            Next
            If value Then
                btnAddColor.Visibility = Windows.Visibility.Visible
                btnDeleteColor.Visibility = Windows.Visibility.Visible
                btnEditColor.Visibility = Windows.Visibility.Visible
            Else
                btnAddColor.Visibility = Windows.Visibility.Collapsed
                btnDeleteColor.Visibility = Windows.Visibility.Collapsed
                btnEditColor.Visibility = Windows.Visibility.Collapsed
            End If
        End Set
    End Property

    Public Sub RefreshColors()
        Dim firstCol As Color
        If Color <> Color.FromArgb(0, 0, 0, 0) Then
            firstCol = Color
        End If
        Dim colorStrings As List(Of String) = Split(ReadSetting(GLOBAL_CONFIG_FILE, "UserColors", ""), ",").ToList
        colorStrings.RemoveAll(Function([object] As String) [object] = "")
        Dim i As Integer = 0
        While i < panelMain.Children.Count - 1
            If i < colorStrings.Count Then
                CType(panelMain.Children(i), ColorRadioButton).ColorBrush = New SolidColorBrush(ColorHelper.GetColor(colorStrings(i)))
                i += 1
            Else
                panelMain.Children.RemoveAt(i)
            End If
        End While
        For i = panelMain.Children.Count - 1 To colorStrings.Count - 1
            panelMain.Children.Insert(panelMain.Children.Count - 1, NewColorRadioButton(ColorHelper.GetColor(colorStrings(i))))
        Next
        If firstCol <> Color.FromArgb(0, 0, 0, 0) Then Color = firstCol
    End Sub

    Public Event ColorChanged(ByVal sender As Object, ByVal e As RoutedEventArgs)

    Private Sub btnAddColor_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Dim colDialog As New ColorDialog
        If HasColor Then colDialog.Color = Color
        colDialog.Owner = GetFirstParentOfType(Me, GetType(Window))
        colDialog.ShowDialog()
        If colDialog.ButtonOKPressed Then
            Dim col As Color = colDialog.Color
            If Colors.Contains(col) Then
                Color = col
            Else
                panelMain.Children.Insert(panelMain.Children.Count - 1, NewColorRadioButton(col))
                SaveColors()
                Color = col
            End If
        End If
    End Sub
    Private Sub btnDeleteColor_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        If HasColor Then
            Dim childToRemove As ColorRadioButton = Nothing
            For Each child In Children
                If CType(child, ColorRadioButton).IsChecked Then
                    childToRemove = child
                End If
            Next
            If childToRemove IsNot Nothing Then
                Dim index As Integer = Colors.IndexOf(CType(childToRemove.ColorBrush, SolidColorBrush).Color)
                panelMain.Children.Remove(childToRemove)
                SaveColors()
                Color = Colors(Min(Colors.Count - 1, index))
            End If
        End If
    End Sub

    Private Sub SaveColors()
        Dim list As New List(Of Object)
        For Each child In Children
            list.Add(ColorHelper.GetColorName(CType(CType(child, ColorRadioButton).ColorBrush, SolidColorBrush).Color))
        Next
        WriteSetting(GLOBAL_CONFIG_FILE, "UserColors", Join(list.ToArray, ","))
    End Sub
    Private Function NewColorRadioButton(ByVal color As Color) As ColorRadioButton
        Dim colorRad As New ColorRadioButton
        colorRad.ColorBrush = New SolidColorBrush(color)
        colorRad.SnapsToDevicePixels = True
        colorRad.Width = 22
        colorRad.Height = 22
        AddHandler colorRad.Checked, AddressOf ColorRadioButton_CheckChanged
        Return colorRad
    End Function
    Private Sub ColorRadioButton_CheckChanged(ByVal sender As Object, ByVal e As RoutedEventArgs)
        RaiseEvent ColorChanged(Me, New RoutedEventArgs)
    End Sub
    Private ReadOnly Property Children As ReadOnlyCollection(Of ColorRadioButton)
        Get
            Dim lst As New List(Of ColorRadioButton)
            For Each item In panelMain.Children
                If TypeOf item Is ColorRadioButton Then lst.Add(item)
            Next
            Return New ReadOnlyCollection(Of ColorRadioButton)(lst)
        End Get
    End Property

    Private Sub btnEditColor_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        For Each child In Children
            If CType(child, ColorRadioButton).IsChecked Then
                Dim colDialog As New ColorDialog
                If HasColor Then colDialog.Color = Color
                colDialog.Owner = GetFirstParentOfType(Me, GetType(Window))
                colDialog.ShowDialog()
                If colDialog.ButtonOKPressed Then
                    Dim col As Color = colDialog.Color
                    If Colors.Contains(col) Then
                        Color = col
                    Else
                        CType(child, ColorRadioButton).ColorBrush = New SolidColorBrush(col)
                        'Color = col
                        SaveColors()
                        RaiseEvent ColorChanged(Me, New RoutedEventArgs)
                    End If
                    Exit For
                End If
            End If
        Next
    End Sub
End Class
