

Public Enum ClickMode
    Pan = 3
    'PanAndDrag = 2
    'Disabled = 0
    Normal = 1
    TimeAxisDrag = 4
    PriceAxisDrag = 5

    CreateLine = 6
    CreateLabel = 7
    CreateArrow = 8
    CreateRectangle = 9
    CreateBuyOrder = 10
    CreateSellOrder = 11
    CreateBuyMarketOrder = 12
    CreateSellMarketOrder = 13

    Undefined = -1
End Enum

Public Enum MouseDownType
    CapturedOnMe
    CapturedOnChild
    MouseDown
    MouseUp
End Enum

Enum BarTypes
    RangeBars = 0
    TimeBars = 1
    VerticalSwingBars = 2
End Enum

Public Enum Relation
    Child
    Parent
    Unrelated
End Enum

Public Enum ScaleType As Integer
    Fixed = 2
    MoveableOnly = 1
    MoveableAndScaleable = 0
End Enum

Public Enum LabelHorizontalAlignment
    Left
    Center
    Right
End Enum
Public Enum LabelVerticalAlignment
    Top
    Center
    Bottom
End Enum

Public Enum CodeLanguage
    VisualBasic
    VisualCSharp
End Enum

'Public Enum SecurityType
'    Stock = 0
'    [Option] = 1
'    Future = 2
'    Index = 3
'    FutureOption = 4
'    Cash = 5
'    Bag = 6
'    Bond = 7
'    Undefined = 9
'End Enum