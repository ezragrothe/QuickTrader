Public Interface IChartObject

    Property SnapToRelativePixels As Boolean
    Property Tag As Object
    Property AnalysisTechnique As AnalysisTechniques.AnalysisTechnique
    Sub ParentBoundsChanged()
    ReadOnly Property IsMouseDown As Boolean
    ReadOnly Property IsRightMouseButtonDown As Boolean
    ReadOnly Property Parent As Chart
    ReadOnly Property HasParent As Boolean
    Property HasPhantomParent As Boolean
    Sub ParentMouseMove(ByVal e As MouseEventArgs, ByVal location As Point)
    Sub ParentMouseLeftButtonDown(ByVal e As MouseButtonEventArgs, ByVal location As Point)
    Sub ParentMouseLeftButtonUp(ByVal e As MouseButtonEventArgs, ByVal location As Point)
    Sub ParentMouseRightButtonDown(ByVal e As MouseButtonEventArgs, ByVal location As Point)
    Sub ParentMouseRightButtonUp(ByVal e As MouseButtonEventArgs, ByVal location As Point)
    Sub ParentMouseMiddleButtonDown(ByVal e As MouseButtonEventArgs, ByVal location As Point)
    Sub ParentMouseMiddleButtonUp(ByVal e As MouseButtonEventArgs, ByVal location As Point)
    Sub ParentMouseDoubleClick(ByVal e As MouseButtonEventArgs, ByVal location As Point)
    Sub ParentMouseWheel(ByVal e As MouseWheelEventArgs)
    Sub Command_Executed(ByVal sender As Object, ByVal e As ExecutedRoutedEventArgs)

End Interface
