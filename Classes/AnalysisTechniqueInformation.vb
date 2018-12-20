Public Structure AnalysisTechniqueInformation
    Public Sub New(ByVal identifier As String)
        Me.Identifier = identifier
    End Sub
    Public Sub New(ByVal identifier As String, ByVal analysisTechnique As AnalysisTechniques.AnalysisTechnique, ByVal id As Integer)
        Me.Identifier = identifier
        Me.AnalysisTechnique = analysisTechnique
        Me.ID = id
    End Sub
    Public Property AnalysisTechnique As AnalysisTechniques.AnalysisTechnique
    Public Property Identifier As String
    Public Property ID As Integer
End Structure
