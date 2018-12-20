Imports QuickTrader.AnalysisTechniques
Imports System.Reflection

Public Class InsertAnalysisTechniqueWindow
    Private analysisTechniqueWindow As FormatAnalysisTechniquesWindow
    Private analysisTechniques As New List(Of AnalysisTechniqueInformation)
    Private builtInAnalysisTechniques As New List(Of AnalysisTechniqueInformation)
    Public Sub New(ByVal analysisTechniqueWindow As FormatAnalysisTechniquesWindow)
        InitializeComponent()
        Me.analysisTechniqueWindow = analysisTechniqueWindow
        For Each type In GetBuiltInAnalysisTechniques()
            Dim info As New AnalysisTechniqueInformation
            info.Identifier = "b-" & type.Name
            builtInAnalysisTechniques.Add(info)
            Dim item As New ListBoxItem
            item.Content = InstantiateAnalysisTechnique(type, Nothing).Name
            item.Tag = info
            Select Case item.Content
                'Case "MovingAverage", "Bar Counter", "AutoTrendV3", "Statistical Analysis Technique"
                Case Else
                    lstBuiltInAnalysisTechniques.Items.Add(item)
            End Select
        Next
        'analysisTechniqueWindow.chart.
    End Sub
    Private Sub btnOK_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        For Each technique In analysisTechniques
            Dim analysisTechnique As AnalysisTechnique = InstantiateAnalysisTechnique(LoadAnalysisTechnique(technique.Identifier), analysisTechniqueWindow.chart)
            For Each input In analysisTechnique.Inputs
                input.Value = input.Converter.ConvertFromString(ReadSetting(GLOBAL_CONFIG_FILE, analysisTechnique.Name & "." & input.Name, input.StringValue))
            Next
            technique.AnalysisTechnique = analysisTechnique
            technique.AnalysisTechnique.IsEnabled = True
            technique.AnalysisTechnique.OnCreate()
            analysisTechniqueWindow.AddAnalysisTechnique(technique, True)
        Next
        Close()
    End Sub

    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Close()
    End Sub

    Private Sub btnAddBuiltInAnalysisTechniques_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        For Each item In lstBuiltInAnalysisTechniques.SelectedItems
            Dim contains As Boolean = False
            For Each item2 In lstAnalysisTechniquesToAdd.Items
                If item.Content = item2.Content Then contains = True
            Next
            'If Not contains Then
            Dim lstItem As New ListBoxItem
            lstItem.Content = item.Content
            lstItem.Tag = item.Tag
            lstAnalysisTechniquesToAdd.Items.Add(lstItem)
            analysisTechniques.Add(item.Tag)
            'End If
        Next
    End Sub
    Private Sub btnRemove_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Dim i As Integer
        While i < lstAnalysisTechniquesToAdd.Items.Count
            If lstAnalysisTechniquesToAdd.SelectedItems.Contains(lstAnalysisTechniquesToAdd.Items(i)) Then
                analysisTechniques.Remove(lstAnalysisTechniquesToAdd.Items(i).Tag)
                lstAnalysisTechniquesToAdd.Items.RemoveAt(i)
            Else
                i += 1
            End If
        End While
    End Sub
    Private Sub btnImport_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Dim fileDialog As New Forms.OpenFileDialog
        fileDialog.CheckFileExists = True
        fileDialog.CheckPathExists = True
        fileDialog.Title = "Open Analysis Technique"
        fileDialog.Filter = "Code File (*.vb, *.cs)|*.vb;*.cs|Library (*.dll)|*.dll"
        fileDialog.Multiselect = True
        If fileDialog.ShowDialog = Forms.DialogResult.OK Then
            Dim box As InfoBox = ShowInfoBoxAsync("Importing...")
            For Each fileName In fileDialog.FileNames
                Dim file As New FileInfo(fileName)
                Dim types As Type() = Nothing
                Dim identifier As String = ""
                If file.Extension = ".dll" Then
                    identifier = "d-" & file.FullName
                    types = GetAnalysisTechniques(Assembly.LoadFile(file.FullName))
                ElseIf file.Extension = ".vb" Then
                    identifier = "f-" & file.FullName
                    types = CompileCodeFromFile(file.FullName)
                ElseIf file.Extension = ".cs" Then
                    identifier = "f-" & file.FullName
                    types = CompileCodeFromFile(file.FullName, CodeLanguage.VisualCSharp)
                End If
                If types IsNot Nothing Then
                    For Each type In types
                        AddAnalysisTechnique(identifier & "," & type.Name, type.Name)
                    Next
                End If
            Next
            box.Close()
        End If
    End Sub
    Private Sub AddAnalysisTechnique(ByVal identifier As String, ByVal name As String)
        Dim info As New AnalysisTechniqueInformation
        info.Identifier = identifier
        analysisTechniques.Add(info)
        Dim item As New ListBoxItem
        item.Content = name
        item.Tag = info
        lstAnalysisTechniquesToAdd.Items.Add(item)
    End Sub
End Class