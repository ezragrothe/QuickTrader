'Namespace AnalysisTechniques
'    Public Class PerformanceTest

'#Region "AnalysisTechnique Inherited Code"
'        Inherits AnalysisTechnique

'        ' Inherit the one-argument constructor from the base class.
'        Public Sub New(ByVal chart As Chart)
'            MyBase.New(chart) ' Call the base class constructor.
'            Description = "A basic line-drawing performance test analysis technique"
'        End Sub
'#End Region
'        <Input()> Public Property Count As Integer = 100
'        Protected Overrides Sub Begin()
'            once = True
'        End Sub
'        Private once As Boolean
'        Protected Overrides Sub Main()
'            If once Then
'                For num = CurrentBar.High + 5 To CurrentBar.High - 5 Step -10 / Count
'                    NewTrendLine(Colors.DimGray, New Point(1, num), New Point(100, num))
'                Next
'                once = False
'            End If
'        End Sub

'        ' The name of the analysis technique.
'        Public Overrides Property Name As String = "Performance Test"

'    End Class
'End Namespace