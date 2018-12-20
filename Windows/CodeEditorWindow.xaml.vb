Imports System.Windows.Forms
Imports System.CodeDom.Compiler
Imports System.Reflection
Imports System.Text

Public Class CodeEditorWindow

    Private Property txtCode As Controls.TextBox
        Get
            If tabMain.Items.Count = 0 Then
                Return New Controls.TextBox
            Else
                Return tabMain.SelectedItem.Tag(0)
            End If
        End Get
        Set(ByVal value As Controls.TextBox)
            If tabMain.Items.Count <> 0 Then
                tabMain.SelectedItem.Tag(0) = value
            End If
        End Set
    End Property
    Private Property IsDirty As Boolean
        Get
            If tabMain.Items.Count = 0 Then
                Return False
            Else
                Return tabMain.SelectedItem.Tag(1)
            End If
        End Get
        Set(ByVal value As Boolean)
            If tabMain.Items.Count <> 0 Then
                tabMain.SelectedItem.Tag(1) = value
            End If
        End Set
    End Property
    Private Property DocName As String
        Get
            If tabMain.Items.Count = 0 Then
                Return ""
            Else
                Return tabMain.SelectedItem.Tag(2)
            End If
        End Get
        Set(ByVal value As String)
            If tabMain.Items.Count <> 0 Then
                tabMain.SelectedItem.Tag(2) = value
            End If
        End Set
    End Property
    Private Property FilePath As String
        Get
            If tabMain.Items.Count = 0 Then
                Return ""
            Else
                Return tabMain.SelectedItem.Tag(3)
            End If
        End Get
        Set(ByVal value As String)
            If tabMain.Items.Count <> 0 Then
                tabMain.SelectedItem.Tag(3) = value
            End If
        End Set
    End Property
    Private Property IsSaved As Boolean
        Get
            If tabMain.Items.Count = 0 Then
                Return ""
            Else
                Return tabMain.SelectedItem.Tag(4)
            End If
        End Get
        Set(ByVal value As Boolean)
            If tabMain.Items.Count <> 0 Then
                tabMain.SelectedItem.Tag(4) = value
            End If
        End Set
    End Property
    Private Property CodeLanguage As String
        Get
            If tabMain.Items.Count = 0 Then
                Return ""
            Else
                Return tabMain.SelectedItem.Tag(5)
            End If
        End Get
        Set(ByVal value As String)
            If tabMain.Items.Count <> 0 Then
                tabMain.SelectedItem.Tag(5) = value
            End If
        End Set
    End Property
    Private Property txtCode(ByVal item As TabItem) As Controls.TextBox
        Get
            Return item.Tag(0)
        End Get
        Set(ByVal value As Controls.TextBox)
            item.Tag(0) = value
        End Set
    End Property
    Private Property IsDirty(ByVal item As TabItem) As Boolean
        Get
            Return item.Tag(1)
        End Get
        Set(ByVal value As Boolean)
            item.Tag(1) = value
        End Set
    End Property
    Private Property DocName(ByVal item As TabItem) As String
        Get
            Return item.Tag(2)
        End Get
        Set(ByVal value As String)
            item.Tag(2) = value
        End Set
    End Property
    Private Property FilePath(ByVal item As TabItem) As String
        Get
            Return item.Tag(3)
        End Get
        Set(ByVal value As String)
            item.Tag(3) = value
        End Set
    End Property
    Private Property IsSaved(ByVal item As TabItem) As Boolean
        Get
            Return item.Tag(4)
        End Get
        Set(ByVal value As Boolean)
            item.Tag(4) = value
        End Set
    End Property
    Private Property CodeLanguage(ByVal item As TabItem) As String
        Get
            Return item.Tag(5)
        End Get
        Set(ByVal value As String)
            item.Tag(5) = value
        End Set
    End Property

    Public Property Font As Font = New Font With {.FontFamily = New FontFamily("Global Monospace"), .FontSize = 13, .Brush = Brushes.Black}
    Public Property TextBackgroundColor As Color = Colors.White
    Private Sub mnuNew_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        AddTab()
        Dim result = ShowInfoBox("Would you like to use VB or C#?", Me, "VB", "C#")

        NewDoc("AnalysisTechnique" & tabMain.Items.Count, If(result = 0, "VB", "C#"))
    End Sub
    Private Sub mnuOpen_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        'If IsDirty() AndAlso ShowInfoBox(DocName & " is unsaved. Save Changes?", "Yes", "No") = InfoBoxResult.Button1 Then Save()
        OpenDoc()
    End Sub
    Private Sub mnuSave_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Save()
    End Sub
    Private Sub mnuSaveAs_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Dim fileDialog As New SaveFileDialog
        fileDialog.CheckFileExists = False
        fileDialog.CheckPathExists = True
        fileDialog.FileName = FilePath
        fileDialog.Title = "Save Analysis Technique"
        fileDialog.Filter = "Visual Basic File (.vb)|*.vb|C# File (.cs)|*.cs"
        If fileDialog.ShowDialog = Forms.DialogResult.OK Then
            FilePath = fileDialog.FileName
            IsDirty = False
            IsSaved = True
            IO.File.WriteAllText(FilePath, txtCode.Text)
            RefreshTitle()
        End If
    End Sub
    Private Sub mnuClose_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        If (IsDirty Or Not IsSaved) AndAlso ShowInfoBox(DocName & " is unsaved. Save Changes?", Me, "Yes", "No") = 0 Then Save()
        tabMain.Items.RemoveAt(tabMain.SelectedIndex)
    End Sub
    Private Sub mnuCheckForErrors_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Dim lang As CodeLanguage
        If CodeLanguage = "VB" Then
            lang = QuickTrader.CodeLanguage.VisualBasic
        Else
            lang = QuickTrader.CodeLanguage.VisualCSharp
        End If
        If CompileCode(txtCode.Text, lang) IsNot Nothing Then
            ShowInfoBox("There are no errors.", Me)
        End If
    End Sub
    Private Sub mnuExecute_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        'ShowInfoBox("This feature is not implemented.")
        Dim masterWin As MasterWindow = Nothing
        For Each item In Application.Current.Windows
            If TypeOf item Is MasterWindow Then
                masterWin = item
                Exit For
            End If
        Next
        For Each win In masterWin.Desktops
            For Each workspace In win.Workspaces
                For Each chart In workspace.Charts
                    For i = 0 To chart.AnalysisTechniques.Count - 1
                        If Split(chart.AnalysisTechniques(i).Identifier.Substring(2), ",")(0) = FilePath Then
                            chart.AnalysisTechniques(i) = chart.RecompileExternalAnalysisTechnique(chart.AnalysisTechniques(i))
                            If chart.AnalysisTechniques(i).AnalysisTechnique Is Nothing Then
                                chart.AnalysisTechniques.RemoveAt(i)
                            Else
                                chart.ReApplyAnalysisTechnique(chart.AnalysisTechniques(i).AnalysisTechnique)
                            End If
                        End If
                    Next
                Next
            Next
        Next
    End Sub
    Private Sub mnuSaveAndExecute_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Save()
        mnuExecute_Click(Nothing, Nothing)
    End Sub
    Private Sub mnuOptions_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Dim win As New CodeEditorOptionsWindow(Me)
        win.Owner = Me
        win.ShowDialog()
    End Sub
    Private Sub mnuInsertTemplate_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        If (txtCode.Text <> "" AndAlso ShowInfoBox("Text already exists. Overwrite?", Me, "Yes", "No") = 0) OrElse txtCode.Text = "" Then
            Dim sb As New StringBuilder
            If CodeLanguage = "C#" Then
                sb.AppendLine("using QuickTrader;")
                sb.AppendLine("using QuickTrader.AnalysisTechniques;")
                sb.AppendLine("using System;")
                sb.AppendLine("using System.Collections;")
                sb.AppendLine("using System.Collections.Generic;")
                sb.AppendLine("using System.Diagnostics;")
                sb.AppendLine("using System.Windows;")
                sb.AppendLine("using System.Windows.Controls;")
                sb.AppendLine("using System.Windows.Data;")
                sb.AppendLine("using System.Windows.Documents;")
                sb.AppendLine("using System.Windows.Input;")
                sb.AppendLine("using System.Windows.Media;")
                sb.AppendLine("using System.Windows.Media.Imaging;")
                sb.AppendLine("using System.Windows.Navigation;")
                sb.AppendLine("")
                sb.AppendLine("namespace AnalysisTechniques")
                sb.AppendLine("{")
                sb.AppendLine(" public class AnalysisTechnique1 : AnalysisTechnique")
                sb.AppendLine(" {")
                sb.AppendLine("     // Inherit the one-argument constructor from the base class.")
                sb.AppendLine("     public AnalysisTechnique1(Chart chart)")
                sb.AppendLine("     {")
                sb.AppendLine("         base(chart); // Call the base class constructor.")
                sb.AppendLine("     }")
                sb.AppendLine("")
                sb.AppendLine("     // The Begin() sub is fired when applying or reapplying an analysis technique.")
                sb.AppendLine("     protected override void Begin()")
                sb.AppendLine("     {")
                sb.AppendLine("")
                sb.AppendLine("     }")
                sb.AppendLine("")
                sb.AppendLine("     // The Main() sub is fired every time a new tick is made.")
                sb.AppendLine("     protected override void Main()")
                sb.AppendLine("     {")
                sb.AppendLine("")
                sb.AppendLine("     }")
                sb.AppendLine("")
                sb.AppendLine("     // The name of the analysis technique.")
                sb.AppendLine("     public override string Name")
                sb.AppendLine("     {")
                sb.AppendLine("         get { return Name; }")
                sb.AppendLine("         set { Name = value; }")
                sb.AppendLine("     }")
                sb.AppendLine("")
                sb.AppendLine(" }")
                sb.AppendLine("}")
            ElseIf CodeLanguage = "VB" Then
                sb.AppendLine("Imports QuickTrader")
                sb.AppendLine("Imports QuickTrader.AnalysisTechniques")
                sb.AppendLine("Imports System.Linq")
                sb.AppendLine("Imports System.Xml.Linq")
                sb.AppendLine("Imports Microsoft.VisualBasic")
                sb.AppendLine("Imports System")
                sb.AppendLine("Imports System.Collections")
                sb.AppendLine("Imports System.Collections.Generic")
                sb.AppendLine("Imports System.Diagnostics")
                sb.AppendLine("Imports System.Windows")
                sb.AppendLine("Imports System.Windows.Controls")
                sb.AppendLine("Imports System.Windows.Data")
                sb.AppendLine("Imports System.Windows.Documents")
                sb.AppendLine("Imports System.Windows.Input")
                sb.AppendLine("Imports System.Windows.Media")
                sb.AppendLine("Imports System.Windows.Media.Imaging")
                sb.AppendLine("Imports System.Windows.Navigation")
                sb.AppendLine("")
                sb.AppendLine("Namespace AnalysisTechniques")
                sb.AppendLine(" Public Class AnalysisTechnique1")
                sb.AppendLine("     Inherits AnalysisTechnique")
                sb.AppendLine("")
                sb.AppendLine("     ' Inherit the one-argument constructor from the base class.")
                sb.AppendLine("     Public Sub New(ByVal chart As Chart)")
                sb.AppendLine("         MyBase.New(chart) ' Call the base class constructor.")
                sb.AppendLine("     End Sub")
                sb.AppendLine("")
                sb.AppendLine("     ' The Begin() sub is fired when applying or reapplying an analysis technique.")
                sb.AppendLine("     Protected Overrides Sub Begin()")
                sb.AppendLine("")
                sb.AppendLine("     End Sub")
                sb.AppendLine("")
                sb.AppendLine("     ' The Main() sub is fired every time a new tick is made.")
                sb.AppendLine("     Protected Overrides Sub Main()")
                sb.AppendLine("")
                sb.AppendLine("     End Sub")
                sb.AppendLine("")
                sb.AppendLine("     ' The name of the analysis technique.")
                sb.AppendLine("     Public Overrides Property Name As String = Me.GetType.Name")
                sb.AppendLine("")
                sb.AppendLine(" End Class")
                sb.AppendLine("End Namespace")
            End If

            txtCode.Text = sb.ToString
        End If
    End Sub
    Private Sub txtCode_TextChanged(ByVal sender As System.Object, ByVal e As System.Windows.Controls.TextChangedEventArgs)
        IsDirty = True
        RefreshTitle()
    End Sub
    Private Sub tabMain_SelectionChanged(ByVal sender As System.Object, ByVal e As System.Windows.Controls.SelectionChangedEventArgs)
        RefreshTitle()
        mnuSave.IsEnabled = Not tabMain.SelectedIndex = -1
        mnuSaveAs.IsEnabled = Not tabMain.SelectedIndex = -1
        mnuExecute.IsEnabled = Not tabMain.SelectedIndex = -1
        mnuCheckForErrors.IsEnabled = Not tabMain.SelectedIndex = -1
        mnuSaveAndExecute.IsEnabled = Not tabMain.SelectedIndex = -1
        mnuOptions.IsEnabled = Not tabMain.SelectedIndex = -1
        mnuInsertTemplate.IsEnabled = Not tabMain.SelectedIndex = -1
        If tabMain.SelectedIndex <> -1 Then
            With Font
                txtCode.FontFamily = .FontFamily
                txtCode.FontSize = .FontSize
                txtCode.FontStyle = .FontStyle
                txtCode.FontWeight = .FontWeight
                txtCode.Effect = .Effect
                txtCode.RenderTransform = .Transform
                txtCode.RenderTransformOrigin = New Point(0.5, 0.5)
                txtCode.Background = New SolidColorBrush(TextBackgroundColor)
                txtCode.Foreground = Font.Brush
            End With
        End If
    End Sub
    Private Sub CodeEditorWindow_Closing(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles Me.Closing
        Dim repeatAction As Integer = -1
        For Each item In tabMain.Items
            Dim result As Integer = -1
            If (IsDirty(item) Or Not IsSaved(item)) And repeatAction = -1 Then
                result = ShowInfoBox(DocName(item) & " is unsaved. Save Changes?", Me, "Yes", "Yes To All", "No", "No To All")
                If result = 1 Or result = 3 Then repeatAction = result
            End If
            If repeatAction = 1 OrElse result = 0 Then Save(item)
            'tabMain.Items.RemoveAt(tabMain.Items.IndexOf(item))
        Next
    End Sub

    Public Sub AddTab()
        Dim tabItem As New TabItem
        Dim grd As New Grid
        grd.RowDefinitions.Add(New RowDefinition)
        grd.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Auto)})
        Dim txt As New Controls.TextBox With {.Background = Brushes.White, .Foreground = Brushes.Black, .AcceptsTab = True,
                                                    .AcceptsReturn = True, .IsReadOnly = True, .HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                                                    .VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                                                    .FontFamily = New FontFamily("Global Monospace"), .FontSize = 13}
        AddHandler txt.TextChanged, AddressOf txtCode_TextChanged
        grd.Children.Add(txt)
        tabItem.Content = grd
        tabItem.Tag = {txt,
                       False,
                       "",
                       "",
                       False,
                       CObj("")} ' textbox, isDirty, docName, filePath, isSaved, language
        tabMain.Items.Add(tabItem)
        tabMain.SelectedItem = tabItem
    End Sub

    Public Sub Save()
        Save(tabMain.SelectedItem)
    End Sub
    Public Sub Save(ByVal item As TabItem)
        If IsSaved(item) Then
            IsDirty(item) = False
            IO.File.WriteAllText(FilePath(item), txtCode(item).Text)
            RefreshTitle()
        Else
            Dim fileDialog As New SaveFileDialog
            fileDialog.CheckFileExists = False
            fileDialog.CheckPathExists = True
            fileDialog.FileName = FilePath
            fileDialog.Title = "Save Analysis Technique"
            'If CodeLanguage = "Vis
            fileDialog.Filter = "Visual Basic File (.vb)|*.vb|C# File (.cs)|*.cs"
            If fileDialog.ShowDialog = Forms.DialogResult.OK Then
                FilePath(item) = fileDialog.FileName
                IsDirty(item) = False
                IsSaved(item) = True
                IO.File.WriteAllText(FilePath(item), txtCode.Text)
                RefreshTitle()
            End If
        End If
    End Sub
    Public Sub LoadDoc(ByVal path As String)
        Dim file As New FileInfo(path)
        DocName = file.Name.Replace(file.Extension, "")
        For Each item In tabMain.Items
            If item.Tag(3) = path Then
                'showInfoBox(DocName & " is already opened.")
                tabMain.Items.Remove(tabMain.SelectedItem)
                Exit Sub
            End If
        Next
        FilePath = path
        If file.Extension.ToUpper = ".VB" Then
            CodeLanguage = "VB"
        ElseIf file.Extension.ToUpper = ".CS" Then
            CodeLanguage = "C#"
        End If
        NewDoc(DocName, CodeLanguage)
        IsSaved = True
        If Not IO.File.Exists(path) Then IO.File.WriteAllText(path, "")
        txtCode.Text = IO.File.ReadAllText(path)
        IsDirty = False
        RefreshTitle()
    End Sub
    Public Sub NewDoc(ByVal name As String, ByVal language As String)
        DocName = name
        CodeLanguage = language
        txtCode.IsReadOnly = False
        IsSaved = False
        tabMain.SelectedItem.Header = GenerateTabItemHeader(name)
        RefreshTitle()
    End Sub
    Public Sub OpenDoc()
        Dim fileDialog As New OpenFileDialog
        fileDialog.CheckFileExists = True
        fileDialog.CheckPathExists = True
        fileDialog.Title = "Open Analysis Technique"
        fileDialog.Filter = "Code Files (*.vb, *.cs)|*.vb;*.cs"
        fileDialog.Multiselect = True
        If fileDialog.ShowDialog = Forms.DialogResult.OK Then
            For Each fileName In fileDialog.FileNames
                AddTab()
                LoadDoc(fileName)
            Next
        End If
    End Sub
    Private Function GenerateTabItemHeader(ByVal text As String) As Grid
        Dim grd As New Grid
        grd.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})
        grd.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Auto)})
        grd.Children.Add(New TextBlock With {.Text = text, .Tag = text, .VerticalAlignment = Windows.VerticalAlignment.Center})
        Dim btn As New Controls.Button With {.Content = "X", .VerticalAlignment = Windows.VerticalAlignment.Center, .Padding = New Thickness(5, 1, 5, 1), .Margin = New Thickness(5, 0, -5, 0)}
        btn.SetValue(Grid.ColumnProperty, 1)
        grd.Children.Add(btn)
        Dim binding As New Windows.Data.Binding
        binding.Path = New PropertyPath(TabItem.IsSelectedProperty)
        binding.RelativeSource = New RelativeSource(RelativeSourceMode.FindAncestor, GetType(TabItem), 1)
        binding.Converter = New BooleanToVisibilityConverter
        btn.SetBinding(Controls.Button.VisibilityProperty, binding)
        AddHandler btn.Click,
            Sub(sender As Object, e As EventArgs)
                Dim item As TabItem = sender.Parent.Parent
                If (IsDirty(item) Or Not IsSaved(item)) AndAlso ShowInfoBox(DocName(item) & " is unsaved. Save Changes?", Me, "Yes", "No") = 0 Then
                    Save(item)
                End If
                tabMain.Items.RemoveAt(tabMain.Items.IndexOf(item))
            End Sub
        Return grd
    End Function
    Private Sub RefreshTitle()
        If tabMain.SelectedIndex <> -1 Then
            Title = "Analysis Technique " & CodeLanguage & " Code Editor" & " - " & DocName
            If tabMain.SelectedItem.Header IsNot Nothing Then tabMain.SelectedItem.Header.Children(0).Text = tabMain.SelectedItem.Header.Children(0).Tag & If(IsDirty, "*", "")
        Else
            Title = "Analysis Technique Code Editor"
        End If
    End Sub
    'Dim compiledAssembly As Assembly
    'Dim errors As CompilerErrorCollection
    'Private Sub CompileCode(ByVal text As String, ByVal name As String, ByVal language As String)
    '    Dim provider As CodeDomProvider = Nothing
    '    Dim compileOk As Boolean = False
    '    If language.ToUpper = "VB" Then
    '        provider = CodeDomProvider.CreateProvider("VisualBasic")
    '    Else
    '        provider = CodeDomProvider.CreateProvider("CSharp")
    '    End If

    '    If Not provider Is Nothing Then
    '        Dim exeName As String = String.Format("{0}\{1}", My.Computer.FileSystem.SpecialDirectories.Desktop, name.Replace(".", "_"))
    '        Dim cp As CompilerParameters = New CompilerParameters()
    '        cp.GenerateExecutable = False
    '        cp.OutputAssembly = exeName
    '        cp.ReferencedAssemblies.Clear()
    '        cp.ReferencedAssemblies.Add("C:\Documents and Settings\David Grothe\My Documents\Visual Studio 2010\Projects\QuickTrader\bin\Debug\QuickTrader.exe")
    '        cp.ReferencedAssemblies.Add("C:\Documents and Settings\David Grothe\My Documents\Visual Studio 2010\Projects\QuickTraderLib\bin\Debug\dll")
    '        cp.ReferencedAssemblies.Add("C:\Program Files\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0\PresentationCore.dll")
    '        cp.ReferencedAssemblies.Add("C:\Program Files\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0\PresentationFramework.dll")
    '        cp.ReferencedAssemblies.Add("C:\Program Files\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0\System.dll")
    '        cp.ReferencedAssemblies.Add("C:\Program Files\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0\System.Core.dll")
    '        cp.ReferencedAssemblies.Add("C:\Program Files\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0\System.Data.dll")
    '        cp.ReferencedAssemblies.Add("C:\Program Files\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0\System.Data.DataSetExtensions.dll")
    '        cp.ReferencedAssemblies.Add("C:\Program Files\Reference Assemblies\Microsoft\Framework\Silverlight\v3.0\System.Windows.dll")
    '        cp.ReferencedAssemblies.Add("C:\Program Files\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0\System.Xaml.dll")
    '        cp.ReferencedAssemblies.Add("C:\Program Files\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0\System.Xaml.dll")
    '        cp.ReferencedAssemblies.Add("C:\Program Files\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0\System.Xml.dll")
    '        cp.ReferencedAssemblies.Add("C:\Program Files\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0\System.Xml.Linq.dll")
    '        cp.ReferencedAssemblies.Add("C:\Program Files\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0\WindowsBase.dll")
    '        cp.GenerateInMemory = True
    '        cp.TreatWarningsAsErrors = False
    '        Dim cr As CompilerResults = provider.CompileAssemblyFromSource(cp, text)
    '        errors = cr.Errors
    '        If errors.Count = 0 Then
    '            compiledAssembly = cr.CompiledAssembly
    '        Else
    '            compiledAssembly = Nothing
    '        End If
    '    End If
    'End Sub

    Public Sub Refresh()
        tabMain_SelectionChanged(Nothing, Nothing)
    End Sub
End Class
