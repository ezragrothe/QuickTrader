Imports System.Reflection
Imports System.CodeDom.Compiler
Imports System.Text
Imports QuickTrader.AnalysisTechniques
Imports System.IO.File
Public Module AnalysisTechniqueHelper
    Private QuickTraderExecutableLocation As String = "C:\Users\David\Desktop\QuickTrader\QuickTrader\bin\Debug\QuickTrader.exe"
    Private krsAtsIBNetDllLocation As String = "C:\Users\David\Desktop\QuickTrader\QuickTrader\bin\Debug\dll"
    Public Function CompileCodeFromFile(ByVal filename As String, Optional ByVal codeType As CodeLanguage = CodeLanguage.VisualBasic, Optional ByVal hideErrors As Boolean = False) As Type()
        If Not Exists(filename) Then
            ShowInfoBox("Unable to locate '" & filename & "'.", Nothing)
        Else
            Return CompileCode(ReadAllText(filename), codeType, hideErrors)
        End If
        Return Nothing
    End Function

    Public Function CompileCode(ByVal text As String, Optional ByVal codeType As CodeLanguage = CodeLanguage.VisualBasic, Optional ByVal hideErrors As Boolean = False) As Type()
        Try
            Dim provider As CodeDomProvider = Nothing
            Dim compileOk As Boolean = False
            If codeType = QuickTrader.CodeLanguage.VisualBasic Then
                provider = CodeDomProvider.CreateProvider("VisualBasic")
            ElseIf codeType = QuickTrader.CodeLanguage.VisualCSharp Then
                provider = CodeDomProvider.CreateProvider("CSharp")
            End If

            If Not provider Is Nothing Then
                Dim cp As CompilerParameters = New CompilerParameters()
                cp.GenerateExecutable = False
                cp.ReferencedAssemblies.Clear()
                If Not Exists(QuickTraderExecutableLocation) Then
                    ShowInfoBox("Unable to locate QuickTrader.exe.", Nothing)
                    Return Nothing
                End If
                If Not Exists(krsAtsIBNetDllLocation) Then
                    ShowInfoBox("Unable to locate dll.", Nothing)
                    Return Nothing
                End If
                cp.ReferencedAssemblies.Add(QuickTraderExecutableLocation)
                cp.ReferencedAssemblies.Add(krsAtsIBNetDllLocation)
                'cp.ReferencedAssemblies.Add("C:\Program Files\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0\PresentationCore.dll")
                cp.ReferencedAssemblies.Add("C:\Program Files\Reference Assemblies\Microsoft\Framework\v3.0\PresentationCore.dll")
                cp.ReferencedAssemblies.Add("C:\Program Files\Reference Assemblies\Microsoft\Framework\v3.0\PresentationFramework.dll")
                'cp.ReferencedAssemblies.Add("C:\Program Files\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0\System.dll")
                'cp.ReferencedAssemblies.Add("C:\Program Files\Reference Assemblies\Microsoft\Framework\v3.5\System.Core.dll")
                'cp.ReferencedAssemblies.Add("C:\Program Files\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0\System.Data.dll")
                'cp.ReferencedAssemblies.Add("C:\Program Files\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0\System.Data.DataSetExtensions.dll")
                'cp.ReferencedAssemblies.Add("C:\Program Files\Reference Assemblies\Microsoft\Framework\Silverlight\v3.0\System.Windows.dll")
                'cp.ReferencedAssemblies.Add("C:\Program Files\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0\System.Xaml.dll")
                'cp.ReferencedAssemblies.Add("C:\Program Files\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0\System.Xml.dll")
                'cp.ReferencedAssemblies.Add("C:\Program Files\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0\System.Xml.Linq.dll")
                cp.ReferencedAssemblies.Add("C:\Program Files\Reference Assemblies\Microsoft\Framework\v3.0\WindowsBase.dll")
                cp.GenerateInMemory = True
                cp.TreatWarningsAsErrors = False
                Dim cr As CompilerResults = provider.CompileAssemblyFromSource(cp, text)
                If cr.Errors.Count > 0 Then
                    If Not hideErrors AndAlso ShowInfoBox("Compilation failed. There" & If(cr.Errors.Count = 1, " was ", " were ") & cr.Errors.Count & " error" & If(cr.Errors.Count <> 1, "s", "") & ".", Nothing, "OK", "View Errors") = 1 Then
                        Dim sb As New StringBuilder
                        Dim ce As CompilerError
                        For Each ce In cr.Errors
                            sb.Append("Error " & ce.ErrorNumber & ": " & ce.ErrorText & " Line " & ce.Line & "." & vbNewLine)
                        Next
                        ShowInfoBox(sb.ToString, Nothing)
                    End If
                    Return Nothing
                Else
                    Return GetAnalysisTechniques(cr.CompiledAssembly)
                End If
                If cr.Errors.Count > 0 Then
                    compileOk = False
                Else
                    compileOk = True
                End If
            End If
        Catch ex As Exception
            If ShowInfoBox("Unable to compile code.", Nothing, "OK", "View Detail") = 1 Then
                ShowInfoBox(ex.Message & vbNewLine & ex.StackTrace, Nothing)
            End If
        End Try
        Return Nothing
    End Function

    Public Function GetAnalysisTechniques(ByVal assembly As Assembly) As Type()
        Dim types As New List(Of Type)
        For Each type In assembly.GetTypes
            If type.IsPublic And Not type.IsAbstract And type.IsSubclassOf(GetType(AnalysisTechniques.AnalysisTechnique)) Then
                types.Add(type)
            End If
        Next
        Return types.ToArray
    End Function

    Private analysisTechniqueTypes As New Dictionary(Of String, Type)
    Public Function LoadAnalysisTechnique(ByVal identifier As String) As Type
        Dim name As String = identifier.Substring(2)
        'Dim id As Integer
        'For i = 0 To name.Length - 1
        '    If Not IsNumeric(name.Substring(i, 1)) Then
        '        id = name.Substring(0, i)
        '        Exit For
        '    End If
        'Next
        'name = name.Substring(id.ToString.Length)

        Dim types() As Type = Nothing
        If identifier.Substring(0, 2) = "b-" Then
            types = GetBuiltInAnalysisTechniques()
        ElseIf identifier.Substring(0, 2) = "l-" Then
            Dim filePath As String = Split(name, ",")(0)
            name = Split(name, ",")(1)
            types = GetAnalysisTechniques(Assembly.LoadFile(filePath))
        ElseIf identifier.Substring(0, 2) = "f-" Then
            Dim filePath As String = Split(name, ",")(0)
            name = Split(name, ",")(1)
            types = CompileCodeFromFile(filePath)
        Else
            Return Nothing
        End If
        'If Not analysisTechniqueTypes.ContainsKey(name) Then
        If types IsNot Nothing Then
            For Each type In types
                If type.Name = name Then Return type
            Next
        End If
        'If analysisTechniqueTypes.ContainsKey(name) Then
        '    Return analysisTechniqueTypes(name)
        'Else
        '    Return Nothing
        'End If
        'Else
        'Return analysisTechniqueTypes(name)
        'End If
        Return Nothing
    End Function

    Public Function GetBuiltInAnalysisTechniques() As Type()
        Dim types As New List(Of Type)
        For Each type In Assembly.GetAssembly(GetType(AnalysisTechnique)).GetTypes
            If Not type.IsAbstract AndAlso type.IsSubclassOf(GetType(AnalysisTechnique)) Then
                types.Add(type)
            End If
        Next
        Return types.ToArray
    End Function
    Public Function InstantiateAnalysisTechnique(ByVal analysisTechniqueType As Type, ByVal chart As Chart) As AnalysisTechnique
        If analysisTechniqueType Is Nothing Then
            Return Nothing
        End If
        Return analysisTechniqueType.GetConstructor({GetType(Chart)}).Invoke({chart})
    End Function
End Module

