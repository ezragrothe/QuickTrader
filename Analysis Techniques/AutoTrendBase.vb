Namespace AnalysisTechniques
    Public MustInherit Class AutoTrendBase
        Inherits AnalysisTechnique

        Public Sub New(ByVal chart As Chart)
            MyBase.New(chart) ' Call the base class constructor.
        End Sub
        Friend Overridable Sub PlusMinMove()
        End Sub
        Friend Overridable Sub MinusMinMove()
        End Sub
        Friend Overridable Sub RVDown()
        End Sub
        Friend Overridable Function GetRVBase() As Decimal
            Return Nothing
        End Function
        Friend Overridable Sub RVBase()
        End Sub
        Friend Overridable Sub RVUp()
        End Sub
        Friend Overridable Sub ApplyRV(value As Decimal)
        End Sub
        Friend Overridable Sub ChannelMerge()
        End Sub
        Friend Overridable Sub ChannelUnmerge()
        End Sub
        Friend Overridable Sub ChannelUnmergeOverlap()
        End Sub
        Protected Overrides Sub Main()
        End Sub
        Public Overrides Property Name As String = "AutoTrend"

        <Input("The reversal value.", "General")>
        Public Overridable Property PriceRV As Decimal
        '<Input("")>
        Public Overridable Property CustomRangeValue As Double = -1
        Public Overridable ReadOnly Property RVPadAppliesToAllCharts As Boolean
            Get
                Return False
            End Get
        End Property
        Friend Overridable Function GetDollarValue(ByVal priceDif As Decimal) As Decimal
            Return Nothing
        End Function
        Friend Overridable Function GetRVFromDollar(ByVal dollarDif As Decimal) As Decimal
            Return Nothing
        End Function
        Public Overridable ReadOnly Property RawRV As Decimal
            Get
                Return Nothing
            End Get
        End Property
#Region "Types"

#End Region
    End Class
    Class BCLine
        Public Property Line As TrendLine
        Public Property TrendLine As TrendLine
        Public Property PotentialTrendLine As TrendLine
        Public Property LengthText As Label
        Public Property TargetText As Label
        Public Property Swing As Swing
        Public Property IsHit As Boolean
        Public Property APoint As Point
        Public Property DPoint As Point
        Public Property IsConfirmed As Boolean
        Public Property Direction As Direction
    End Class
    Public Class Swing
        Public Property PrevExtendedLength As Decimal
        Public Property OverlapGapLine As TrendLine
        Public Property OverlapGapLineHit As Boolean = False
        Public Property OverlapGapLineDestroyed As Boolean = False
        Public Property OverlapGapLineGapped As Boolean = False
        Public Property PreviousOverlapTrendLine As TrendLine
        Public Property BarsBack As Integer
        Public Property BCLine As TrendLine
        Public Property PotentialTL As TrendLine
        Public Property RegressionTL As TrendLine
        Public Property RegressionTLOuterLines As TrendLine
        Public Property TLs As New List(Of TrendLine)
        Public Property Direction As Direction
        Public Property IsActive As Boolean
        Public Property PointList As New List(Of Point)
        Public Property StepUpIndex As Integer
        Public Property NeutralMinPoint As Point
        Public Property NeutralMaxPoint As Point
        Public Sub New(ByVal tl As TrendLine, ByVal direction As Direction)
            TLs.Add(tl)
            StartBar = tl.StartPoint.X
            StartPrice = tl.StartPoint.Y
            EndBar = tl.EndPoint.X
            EndPrice = tl.EndPoint.Y
            Me.Direction = direction
        End Sub
        Public Property TL As TrendLine
            Get
                If TLs.Count = 0 Then TLs.Add(New TrendLine)
                Return TLs(TLs.Count - 1)
            End Get
            Set(value As TrendLine)
                If TLs.Count = 0 Then TLs.Add(New TrendLine)
                TLs(TLs.Count - 1) = value
            End Set
        End Property
        Public Property StartBar As Integer
            Get
                Return TL.StartPoint.X
            End Get
            Set(ByVal value As Integer)
                TL.StartPoint = New Point(value, TL.StartPoint.Y)
            End Set
        End Property
        Public Property StartPrice As Decimal
            Get
                Return TL.StartPoint.Y
            End Get
            Set(ByVal value As Decimal)
                TL.StartPoint = New Point(TL.StartPoint.X, value)
            End Set
        End Property
        Public Property EndBar As Integer
            Get
                Return TL.EndPoint.X
            End Get
            Set(ByVal value As Integer)
                TL.EndPoint = New Point(value, TL.EndPoint.Y)
            End Set
        End Property
        Public Property EndPrice As Decimal
            Get
                Return TL.EndPoint.Y
            End Get
            Set(ByVal value As Decimal)
                TL.EndPoint = New Point(TL.EndPoint.X, value)
            End Set
        End Property
        Public Property ChannelDirection As ChannelDirectionType = ChannelDirectionType.Neutral
        Public Enum ChannelDirectionType
            Up = -1
            Down = 0
            Neutral = 1
        End Enum
        Public Shared Operator =(ByVal swing1 As Swing, ByVal swing2 As Swing) As Boolean
            Return swing1 Is swing2
        End Operator
        Public Shared Operator <>(ByVal swing1 As Swing, ByVal swing2 As Swing) As Boolean
            Return swing1 IsNot swing2
        End Operator
        Public Property ChannelLine As Channel
        Public Property SwingChannel As TrendLine
        Public Property SwingChannelCut As Boolean
        Public Property RegressionPlot As Plot
        Public Property HasCrossedRVBand As Boolean
        Public ReadOnly Property EndPoint As Point
            Get
                Return New Point(EndBar, EndPrice)
            End Get
        End Property
        Public ReadOnly Property StartPoint As Point
            Get
                Return New Point(StartBar, StartPrice)
            End Get
        End Property
        Public Property LengthText As Label
            Get
                If LengthTexts.Count = 0 Then LengthTexts.Add(New Label)
                Return LengthTexts(LengthTexts.Count - 1)
            End Get
            Set(value As Label)
                If LengthTexts.Count = 0 Then LengthTexts.Add(New Label)
                LengthTexts(LengthTexts.Count - 1) = value
            End Set
        End Property
        Public Property Channel As TrendLine
        Public Property ChannelHorizontalLine As TrendLine
        Public Property PushCountText As Label
        Public Property LengthTexts As New List(Of Label)
        Public Property BCText As Label
        Public Property TargetText As Label
        Public Property ExtendTargetText As Label
        'Public Property BarCountText As Label
        Public Property ABCLengths As New List(Of ABCCombo)
        Public Property ABC As ABCCombo
        Public Property IsOutsideSwing As Boolean
        Public Property IsInsideSwing As Boolean
    End Class
    Public Class ABCCombo
        Public Property A As Point
        Public Property B As Point
        Public Property C As Point
        Public Property D As Point
        Public Property ALengthText As Label
        Public Property BLengthText As Label
        Public Property CLengthText As Label
        Public Property ALine As TrendLine
        Public Property BLine As TrendLine
        Public Property CLine As TrendLine
        Public Property IsSpanner As Boolean
    End Class
    Public Class Channel
        Public Sub New(ByVal tl As TrendLine)
            Me.TL = tl
        End Sub
        Public Property TL As TrendLine
        Public Property MiddleTL As TrendLine

        Public Property APoint As Point
        Public Property BPoint As Point
        Public Property CPoint As Point
        Public Property DPoint As Point

        Public Property Direction As Direction

        Public Property Swing As Swing

        Public Property BCSwingLine As TrendLine
        Public Property ABSwingLine As TrendLine
        Public Property CDSwingLine As TrendLine
        Public Property BCLengthText As Label
        Public Property BCProjectionLine As TrendLine
        Public Property TargetText As Label

        Public Property IsCancelled As Boolean
        Public Property IsConfirmed As Boolean
        Public Property IsCut As Boolean
        Public Property IsGapped As Boolean
        Public Property BCSwingLineRemoved As Boolean
        Public Property BCSwingMatched As Boolean
        Public Property CutBarNumber As Integer = -1
        Public Property IsBackedUp As Boolean
        Public Property IsPartiallyCut As Boolean

        Public Property IsTrendModeChannel As Boolean
        Public Property DontColorBars As Boolean
        Public Property Hidden As Boolean
        Public Property DontDraw As Boolean
        Public Property GappedSwing As Swing
    End Class
    Public Class SwingChannelLine
        Public Sub New(ByVal tl As TrendLine)
            Me.TL = tl
        End Sub
        Public Property TL As TrendLine
        Public Property MiddleTL As TrendLine
        Public Property BCSwingLine As TrendLine
        Public Property IsActive As Boolean
        Public Property IsConfirmed As Boolean
        Public Property APoint As Point
        Public Property BPoint As Point
        Public Property CPoint As Point
        Public Property Swing As Swing
        Public Property IsCut As Boolean
        Public Property IsGapped As Boolean
        Public Property DPoint As Point
        Public Property Direction As Direction
        Public Property BCLengthMet As Boolean
        Public Property PrevBaseLineCoordinates As LineCoordinates
        Public Property PrevParallelDistance As Decimal
        Public Property PrevMiddleLineCoordinates As LineCoordinates
    End Class
    Public Structure GapMarker
        Public Property Line As TrendLine
        Public Property Swing As Swing
        Public Property State As GapMarkerState
    End Structure
    Public Enum GapMarkerState
        Inactive
        FollowsRVPoint
        FollowsExtension
    End Enum
    Public Enum Direction
        Up = -1
        Down = 0
        Neutral = 1
    End Enum
    Public Enum SwingEvent
        None = -1
        NewSwing = 0
        Extension = 1
    End Enum

    Public Class PartiallyCutChannel
        Public Sub New(channel As Channel, swingNum As Integer)
            Me.Channel = channel
            Me.SwingNum = swingNum
        End Sub
        Public Channel As Channel
        Public SwingNum As Integer
    End Class
End Namespace
