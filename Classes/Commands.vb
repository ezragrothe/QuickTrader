Imports System.Reflection

Public Class ChartCommands

    Private Shared commands As New Dictionary(Of String, RoutedUICommand)
    Private Shared props() As PropertyInfo
    Private Shared Function GetHelpText(ByVal commandName As String) As String
        For Each prop In props
            If prop.Name = commandName Then
                For Each attribute In prop.GetCustomAttributes(False)
                    If TypeOf attribute Is CommandHelpTextAttribute Then
                        Return CType(attribute, CommandHelpTextAttribute).HelpText
                    End If
                Next
            End If
        Next
        Return commandName
    End Function
    Private Shared Function GetDefaultHotkey(ByVal commandName As String) As KeyGesture
        commandName = commandName.Replace(" ", "")
        For Each prop In props
            If prop.Name = commandName Then
                For Each attribute In prop.GetCustomAttributes(False)
                    If TypeOf attribute Is CommandDefaultHotkeyAttribute Then
                        Dim key As CommandDefaultHotkeyAttribute = attribute
                        Dim displayText As String = ""
                        If key.ShortcutText Is Nothing OrElse key.ShortcutText = "" Then displayText = HotkeyDisplayString(key.DefaultHotkeyModifiers, key.DefaultHotkey)
                        If key.DefaultHotkey = Windows.Input.Key.None Then displayText = ""
                        Return New KeyGesture(key.DefaultHotkey, key.DefaultHotkeyModifiers, displayText)
                    End If
                Next
            End If
        Next
        Return New KeyGesture(Key.None)
    End Function
    Private Shared Function InitCommand(ByVal name As String, ByVal helpText As String, ByVal ParamArray hotKeys() As KeyGesture) As RoutedUICommand
        If helpText = "" Then helpText = name
        Dim inputs As New InputGestureCollection
        For Each key In hotKeys
            inputs.Add(key)
        Next
        Return New RoutedUICommand(helpText, name, GetType(ChartCommands), inputs)
    End Function

    Shared Sub New()
        props = GetType(ChartCommands).GetProperties
        Dim name As String
        For Each name In GetCommandNames()
            name = name.Replace(" ", "")
            Dim hotkeys() As KeyGesture = GetSavedHotKey(name)
            commands.Add(name, InitCommand(name, GetHelpText(name), hotkeys))
        Next
    End Sub

    Public Shared Function GetCommandNames() As String()
        Dim names As New List(Of String)
        For Each item In props
            names.Add(Spacify(item.Name))
        Next
        names.Sort()
        Return names.ToArray
    End Function
    Public Shared Function GetCommands() As RoutedUICommand()
        If commands.Count = 0 Then
            Dim commands As New List(Of RoutedUICommand)
            Dim [me] As New ChartCommands
            For Each item In props
                commands.Add(item.GetValue([me], Nothing))
            Next
            Return commands.ToArray
        Else
            Return commands.Values.ToArray
        End If
    End Function
    Public Shared Sub AssignHotKey(ByVal commandName As String, ByVal ParamArray hotKeys() As KeyGesture)
        commandName = commandName.Replace(" ", "")
        Dim hotKeyNames As New List(Of String)
        Dim keyGestureConverter As New KeyGestureConverter
        Dim gestureCollection As New InputGestureCollection
        For Each item In hotKeys
            hotKeyNames.Add(keyGestureConverter.ConvertToString(item))
            gestureCollection.Add(item)
        Next
        commands(commandName) = New RoutedUICommand(commands(commandName).Text, commands(commandName).Name, commands(commandName).OwnerType, gestureCollection)
        WriteSetting(GLOBAL_CONFIG_FILE, commandName, Join(hotKeyNames.ToArray, "@"))
    End Sub
    Public Shared Function GetSavedHotKey(ByVal commandName As String) As KeyGesture()
        commandName = commandName.Replace(" ", "")
        Dim hotKeys As New List(Of KeyGesture)
        Dim keyGestureConverter As New KeyGestureConverter
        For Each item In Split(ReadSetting(GLOBAL_CONFIG_FILE, commandName, ""), "@")
            hotKeys.Add(keyGestureConverter.ConvertFromString(item))
        Next
        If ReadSetting(GLOBAL_CONFIG_FILE, commandName, "$") = "$" Then
            hotKeys.Clear()
            hotKeys.Add(GetDefaultHotkey(commandName))
        End If
        Return hotKeys.ToArray
    End Function
    Public Shared Function GetHotKey(ByVal commandName As String) As KeyGesture()
        commandName = commandName.Replace(" ", "")
        Dim keyGestures As New List(Of KeyGesture)
        For Each item In GetCommandFromName(commandName).InputGestures
            keyGestures.Add(CType(item, KeyGesture))
        Next
        Return keyGestures.ToArray
    End Function
    Public Shared Function GetCommandFromName(ByVal commandName As String) As RoutedUICommand
        commandName = commandName.Replace(" ", "")
        If commands.ContainsKey(commandName) Then
            Return commands(commandName)
        Else
            Return Nothing
        End If
    End Function
    Public Shared Function GetCanExecuteAlways(ByVal commandName As String) As Boolean
        commandName = commandName.Replace(" ", "")
        Return True
    End Function



    <CommandHelpText("Enables moving and sizing of objects when dragging."),
    CommandDefaultHotkey(Key.NumPad1)> Public Shared ReadOnly Property SetDragModeNormal As RoutedUICommand
        Get
            Return commands("SetDragModeNormal")
        End Get
    End Property
    <CommandHelpText("Enables panning when dragging."),
    CommandDefaultHotkey(Key.NumPad4)> Public Shared ReadOnly Property SetDragModePan As RoutedUICommand
        Get
            Return commands("SetDragModePan")
        End Get
    End Property
    <CommandHelpText("Deletes the selected object."),
    CommandDefaultHotkey(Key.Delete)> Public Shared ReadOnly Property RemoveObject As RoutedUICommand
        Get
            Return commands("RemoveObject")
        End Get
    End Property
    <CommandHelpText("Deletes all objects of the selected type."),
    CommandDefaultHotkey(ModifierKeys.Shift, Key.Delete)> Public Shared ReadOnly Property RemoveAllObjectsOfSelectedType As RoutedUICommand
        Get
            Return commands("RemoveAllObjectsOfSelectedType")
        End Get
    End Property


    <CommandHelpText("Toggles the selected line's extension right."),
    CommandDefaultHotkey()> Public Shared ReadOnly Property ToggleLineExtensionRight As RoutedUICommand
        Get
            Return commands("ToggleLineExtensionRight")
        End Get
    End Property
    <CommandHelpText("Toggles the selected line's extension left."),
    CommandDefaultHotkey()> Public Shared ReadOnly Property ToggleLineExtensionLeft As RoutedUICommand
        Get
            Return commands("ToggleLineExtensionLeft")
        End Get
    End Property
    <CommandHelpText("Pans to the right 3% of the chart width."),
    CommandDefaultHotkey(Key.Right)> Public Shared ReadOnly Property PanRight As RoutedUICommand
        Get
            Return commands("PanRight")
        End Get
    End Property
    <CommandHelpText("Pans to the left 3% of the chart width."),
    CommandDefaultHotkey(Key.Left)> Public Shared ReadOnly Property PanLeft As RoutedUICommand
        Get
            Return commands("PanLeft")
        End Get
    End Property
    <CommandHelpText("Pans upward 1.5% of the chart height."),
    CommandDefaultHotkey(Key.Up)> Public Shared ReadOnly Property PanUp As RoutedUICommand
        Get
            Return commands("PanUp")
        End Get
    End Property
    <CommandHelpText("Pans downward 1.5% of the chart height."),
    CommandDefaultHotkey(Key.Down)> Public Shared ReadOnly Property PanDown As RoutedUICommand
        Get
            Return commands("PanDown")
        End Get
    End Property
    <CommandHelpText("Pans to the end of the chart."),
    CommandDefaultHotkey(Key.End)> Public Shared ReadOnly Property PanEnd As RoutedUICommand
        Get
            Return commands("PanEnd")
        End Get
    End Property
    <CommandHelpText("Pans to the beginning of the chart."),
    CommandDefaultHotkey(Key.Home)> Public Shared ReadOnly Property PanBegin As RoutedUICommand
        Get
            Return commands("PanBegin")
        End Get
    End Property


    <CommandHelpText("Saves the current time of the selected chart as a point of interest for replaying."),
    CommandDefaultHotkey()> Public Shared ReadOnly Property SavePointOfInterest As RoutedUICommand
        Get
            Return commands("SavePointOfInterest")
        End Get
    End Property

    <CommandHelpText("Snaps the scaling to contain all bars."),
    CommandDefaultHotkey(Key.F11)> Public Shared ReadOnly Property SnapScalingToAllData As RoutedUICommand
        Get
            Return commands("SnapScalingToAllData")
        End Get
    End Property
    <CommandHelpText("Sets the current time scaling as the default for all charts."),
    CommandDefaultHotkey()> Public Shared ReadOnly Property SetTimeScaleAsGlobalDefault As RoutedUICommand
        Get
            Return commands("SetTimeScaleAsGlobalDefault")
        End Get
    End Property
    <CommandHelpText("Snaps the charts in the current workspace to the default workspace price scaling and the default time scaling."),
    CommandDefaultHotkey(Key.F4)> Public Shared ReadOnly Property SnapTimeScaleAsGlobalDefault As RoutedUICommand
        Get
            Return commands("SnapTimeScaleAsGlobalDefault")
        End Get
    End Property
    <CommandHelpText("Sets the current chart to the default time scaling."),
    CommandDefaultHotkey()> Public Shared ReadOnly Property SetTimeAndPriceScaleAsChartDefault As RoutedUICommand
        Get
            Return commands("SetTimeAndPriceScaleAsChartDefault")
        End Get
    End Property
    <CommandHelpText("Snaps the current chart to the default time scaling."),
    CommandDefaultHotkey()> Public Shared ReadOnly Property SnapTimeAndPriceScaleAsChartDefault As RoutedUICommand
        Get
            Return commands("SnapTimeAndPriceScaleAsChartDefault")
        End Get
    End Property
    '<CommandHelpText("Sets the current chart to the default time scaling."),
    'CommandDefaultHotkey()> Public Shared ReadOnly Property SetTimeAndPriceScaleAsChartDefault2 As RoutedUICommand
    '    Get
    '        Return commands("SetTimeAndPriceScaleAsChartDefault2")
    '    End Get
    'End Property
    '<CommandHelpText("Snaps the current chart to the default time scaling."),
    'CommandDefaultHotkey()> Public Shared ReadOnly Property SnapTimeAndPriceScaleAsChartDefault2 As RoutedUICommand
    '    Get
    '        Return commands("SnapTimeAndPriceScaleAsChartDefault2")
    '    End Get
    'End Property
    <CommandHelpText("Sets the current time scaling as the default for the current workspace."),
    CommandDefaultHotkey()> Public Shared ReadOnly Property SetTimeScaleAsWorkspaceDefault As RoutedUICommand
        Get
            Return commands("SetTimeScaleAsWorkspaceDefault")
        End Get
    End Property
    <CommandHelpText("Snaps the charts in the current workspace to the default workspace price and time scaling."),
    CommandDefaultHotkey(Key.F4)> Public Shared ReadOnly Property SnapTimeScaleAsWorkspaceDefault As RoutedUICommand
        Get
            Return commands("SnapTimeScaleAsWorkspaceDefault")
        End Get
    End Property

    <CommandHelpText("Resizes the horizontal axis smaller by a fine amount."),
    CommandDefaultHotkey(ModifierKeys.Control, Key.Up)> Public Shared ReadOnly Property ResizeHorizontalSmallerFine As RoutedUICommand
        Get
            Return commands("ResizeHorizontalSmallerFine")
        End Get
    End Property
    <CommandHelpText("Resizes the horizontal axis wider by a fine amount."),
    CommandDefaultHotkey(ModifierKeys.Control, Key.Down)> Public Shared ReadOnly Property ResizeHorizontalBiggerFine As RoutedUICommand
        Get
            Return commands("ResizeHorizontalBiggerFine")
        End Get
    End Property
    <CommandHelpText("Resizes the horizontal axis smaller by a coarse amount."),
    CommandDefaultHotkey()> Public Shared ReadOnly Property ResizeHorizontalSmallerCoarse As RoutedUICommand
        Get
            Return commands("ResizeHorizontalSmallerCoarse")
        End Get
    End Property
    <CommandHelpText("Resizes the horizontal axis wider by a coarse amount."),
    CommandDefaultHotkey()> Public Shared ReadOnly Property ResizeHorizontalBiggerCoarse As RoutedUICommand
        Get
            Return commands("ResizeHorizontalBiggerCoarse")
        End Get
    End Property

    <CommandHelpText("Resizes the vertical axis smaller by a fine amount."),
    CommandDefaultHotkey(ModifierKeys.Shift, Key.Down)> Public Shared ReadOnly Property ResizeVerticalSmallerFine As RoutedUICommand
        Get
            Return commands("ResizeVerticalSmallerFine")
        End Get
    End Property
    <CommandHelpText("Resizes the vertical axis taller by a fine amount."),
    CommandDefaultHotkey(ModifierKeys.Shift, Key.Up)> Public Shared ReadOnly Property ResizeVerticalBiggerFine As RoutedUICommand
        Get
            Return commands("ResizeVerticalBiggerFine")
        End Get
    End Property

    <CommandHelpText("Resizes the vertical axis smaller by a coarse amount."),
    CommandDefaultHotkey()> Public Shared ReadOnly Property ResizeVerticalSmallerCoarse As RoutedUICommand
        Get
            Return commands("ResizeVerticalSmallerCoarse")
        End Get
    End Property
    <CommandHelpText("Resizes the vertical axis taller by a coarse amount."),
    CommandDefaultHotkey()> Public Shared ReadOnly Property ResizeVerticalBiggerCoarse As RoutedUICommand
        Get
            Return commands("ResizeVerticalBiggerCoarse")
        End Get
    End Property
    <CommandHelpText("Creates a parallel for the selected line."),
    CommandDefaultHotkey(Key.Divide)> Public Shared ReadOnly Property CreateLineParallel As RoutedUICommand
        Get
            Return commands("CreateLineParallel")
        End Get
    End Property
    <CommandHelpText("Removes the parallel for the selected line."),
    CommandDefaultHotkey(Key.Subtract)> Public Shared ReadOnly Property RemoveLineParallel As RoutedUICommand
        Get
            Return commands("RemoveLineParallel")
        End Get
    End Property
    <CommandHelpText("Deletes all drawing objects within all the charts in the selected workspace."),
    CommandDefaultHotkey(ModifierKeys.Control, Key.Delete)> Public Shared ReadOnly Property RemoveAllDrawingObjects As RoutedUICommand
        Get
            Return commands("RemoveAllDrawingObjects")
        End Get
    End Property
    <CommandHelpText("Opens the replay window."),
    CommandDefaultHotkey()> Public Shared ReadOnly Property ShowReplayWindow As RoutedUICommand
        Get
            Return commands("ShowReplayWindow")
        End Get
    End Property
    <CommandHelpText("Opens the format hotkeys window."),
    CommandDefaultHotkey(ModifierKeys.Control, Key.H)> Public Shared ReadOnly Property FormatHotKeys As RoutedUICommand
        Get
            Return commands("FormatHotKeys")
        End Get
    End Property
    <CommandHelpText("Opens the format chart window."),
    CommandDefaultHotkey(ModifierKeys.Control, Key.F)> Public Shared ReadOnly Property Format As RoutedUICommand
        Get
            Return commands("Format")
        End Get
    End Property
    <CommandHelpText("Sets the current desktops as the default, in that they appear when the program starts."),
    CommandDefaultHotkey()> Public Shared ReadOnly Property SaveCurrentDesktopLayout As RoutedUICommand
        Get
            Return commands("SaveCurrentDesktopLayout")
        End Get
    End Property
    <CommandHelpText("Closes all desktops."),
    CommandDefaultHotkey()> Public Shared ReadOnly Property CloseApplication As RoutedUICommand
        Get
            Return commands("CloseApplication")
        End Get
    End Property
    <CommandHelpText("Closes the desktop."),
    CommandDefaultHotkey(ModifierKeys.Alt, Key.F4)>
    Public Shared ReadOnly Property CloseDesktop As RoutedUICommand
        Get
            Return commands("CloseDesktop")
        End Get
    End Property
    <CommandHelpText("Closes the selected chart."),
    CommandDefaultHotkey(ModifierKeys.Control, Key.Q)> Public Shared ReadOnly Property CloseChart As RoutedUICommand
        Get
            Return commands("CloseChart")
        End Get
    End Property
    <CommandHelpText("Creates a new chart with the default settings."),
    CommandDefaultHotkey(ModifierKeys.Control, Key.N)> Public Shared ReadOnly Property NewChart As RoutedUICommand
        Get
            Return commands("NewChart")
        End Get
    End Property
    '<CommandHelpText("Creates a slave chart for the selected chart."),
    'CommandDefaultHotkey()> Public Shared ReadOnly Property NewSlaveChart As RoutedUICommand
    '    Get
    '        Return commands("NewSlaveChart")
    '    End Get
    'End Property
    '<CommandHelpText("Maximizes the selected chart."),
    'CommandDefaultHotkey()> Public Shared ReadOnly Property MaximizeChart As RoutedUICommand
    '    Get
    '        Return commands("MaximizeChart")
    '    End Get
    'End Property
    <CommandHelpText("Creates a new workspace."),
    CommandDefaultHotkey()> Public Shared ReadOnly Property NewWorkspace As RoutedUICommand
        Get
            Return commands("NewWorkspace")
        End Get
    End Property
    <CommandHelpText("Formats the current workspace."),
    CommandDefaultHotkey()> Public Shared ReadOnly Property FormatWorkspace As RoutedUICommand
        Get
            Return commands("FormatWorkspace")
        End Get
    End Property
    <CommandHelpText("Opens a saved workspace."),
    CommandDefaultHotkey(ModifierKeys.Control, Key.O)> Public Shared ReadOnly Property OpenWorkspace As RoutedUICommand
        Get
            Return commands("OpenWorkspace")
        End Get
    End Property
    <CommandHelpText("Closes the current workspace."),
    CommandDefaultHotkey(ModifierKeys.Control + ModifierKeys.Shift, Key.Q)> Public Shared ReadOnly Property CloseWorkspace As RoutedUICommand
        Get
            Return commands("CloseWorkspace")
        End Get
    End Property
    <CommandHelpText("Manually saves the current workspace."),
    CommandDefaultHotkey(ModifierKeys.Control, Key.S)> Public Shared ReadOnly Property SaveWorkspace As RoutedUICommand
        Get
            Return commands("SaveWorkspace")
        End Get
    End Property
    <CommandHelpText("Saves the current workspace as a different name."),
    CommandDefaultHotkey(ModifierKeys.Control + ModifierKeys.Shift, Key.S)> Public Shared ReadOnly Property SaveWorkspaceAs As RoutedUICommand
        Get
            Return commands("SaveWorkspaceAs")
        End Get
    End Property
    '<CommandHelpText("Creates a new workspace."),
    'CommandDefaultHotkey(ModifierKeys.Control + ModifierKeys.Shift, Key.N)> Public Shared ReadOnly Property NewWorkspace As RoutedUICommand
    '    Get
    '        Return commands("NewWorkspace")
    '    End Get
    'End Property
    '<CommandHelpText("Creates a new analysis technique."),
    'CommandDefaultHotkey()> Public Shared ReadOnly Property NewAnalysisTechnique As RoutedUICommand
    '    Get
    '        Return commands("NewAnalysisTechnique")
    '    End Get
    'End Property
    '<CommandHelpText("Opens an analysis technique."),
    'CommandDefaultHotkey()> Public Shared ReadOnly Property OpenAnalysisTechnique As RoutedUICommand
    '    Get
    '        Return commands("OpenAnalysisTechnique")
    '    End Get
    'End Property
    <CommandHelpText("Creates a new instance of QuickTrader."),
    CommandDefaultHotkey()> Public Shared ReadOnly Property NewDesktop As RoutedUICommand
        Get
            Return commands("NewDesktop")
        End Get
    End Property

    <CommandHelpText("Switches to the next workspace.."),
    CommandDefaultHotkey(Key.NumPad0)> Public Shared ReadOnly Property NextWorkspace As RoutedUICommand
        Get
            Return commands("NextWorkspace")
        End Get
    End Property
    <CommandHelpText("Switches to the previous workspace.."),
    CommandDefaultHotkey(Key.NumPad0)> Public Shared ReadOnly Property PreviousWorkspace As RoutedUICommand
        Get
            Return commands("PreviousWorkspace")
        End Get
    End Property
    '<CommandHelpText("Opens the format workspace window."),
    'CommandDefaultHotkey(ModifierKeys.Control + ModifierKeys.Shift, Key.F)> Public Shared ReadOnly Property FormatWorkspace As RoutedUICommand
    '    Get
    '        Return commands("FormatWorkspace")
    '    End Get
    'End Property
    <CommandHelpText("Sets the scaling to contain all bars between the Y margin values."),
    CommandDefaultHotkey(Key.F6)> Public Shared ReadOnly Property AutoScale As RoutedUICommand
        Get
            Return commands("AutoScale")
        End Get
    End Property

    <CommandHelpText("Creates a new arrow from preset #1."),
    CommandDefaultHotkey(Key.NumPad7)> Public Shared ReadOnly Property CreateArrowWithPreset1 As RoutedUICommand
        Get
            Return commands("CreateArrowWithPreset1")
        End Get
    End Property
    <CommandHelpText("Creates a new arrow from preset #2."),
    CommandDefaultHotkey()> Public Shared ReadOnly Property CreateArrowWithPreset2 As RoutedUICommand
        Get
            Return commands("CreateArrowWithPreset2")
        End Get
    End Property
    <CommandHelpText("Creates a new arrow from preset #3."),
    CommandDefaultHotkey()> Public Shared ReadOnly Property CreateArrowWithPreset3 As RoutedUICommand
        Get
            Return commands("CreateArrowWithPreset3")
        End Get
    End Property
    <CommandHelpText("Creates a new arrow from preset #4."),
    CommandDefaultHotkey()> Public Shared ReadOnly Property CreateArrowWithPreset4 As RoutedUICommand
        Get
            Return commands("CreateArrowWithPreset4")
        End Get
    End Property
    <CommandHelpText("Creates a new line from preset #1."),
    CommandDefaultHotkey(Key.NumPad2)> Public Shared ReadOnly Property CreateLineWithPreset1 As RoutedUICommand
        Get
            Return commands("CreateLineWithPreset1")
        End Get
    End Property
    <CommandHelpText("Creates a new line from preset #2."),
    CommandDefaultHotkey()> Public Shared ReadOnly Property CreateLineWithPreset2 As RoutedUICommand
        Get
            Return commands("CreateLineWithPreset2")
        End Get
    End Property
    <CommandHelpText("Creates a new line from preset #3."),
    CommandDefaultHotkey()> Public Shared ReadOnly Property CreateLineWithPreset3 As RoutedUICommand
        Get
            Return commands("CreateLineWithPreset3")
        End Get
    End Property
    <CommandHelpText("Creates a new line from preset #4."),
    CommandDefaultHotkey()> Public Shared ReadOnly Property CreateLineWithPreset4 As RoutedUICommand
        Get
            Return commands("CreateLineWithPreset4")
        End Get
    End Property
    <CommandHelpText("Creates a new label from preset #1."),
    CommandDefaultHotkey(Key.NumPad3)> Public Shared ReadOnly Property CreateLabelWithPreset1 As RoutedUICommand
        Get
            Return commands("CreateLabelWithPreset1")
        End Get
    End Property
    <CommandHelpText("Creates a new label from preset #2."),
    CommandDefaultHotkey()> Public Shared ReadOnly Property CreateLabelWithPreset2 As RoutedUICommand
        Get
            Return commands("CreateLabelWithPreset2")
        End Get
    End Property
    <CommandHelpText("Creates a new label from preset #3."),
    CommandDefaultHotkey()> Public Shared ReadOnly Property CreateLabelWithPreset3 As RoutedUICommand
        Get
            Return commands("CreateLabelWithPreset3")
        End Get
    End Property
    <CommandHelpText("Creates a new label from preset #4."),
    CommandDefaultHotkey()> Public Shared ReadOnly Property CreateLabelWithPreset4 As RoutedUICommand
        Get
            Return commands("CreateLabelWithPreset4")
        End Get
    End Property
    <CommandHelpText("Creates a new rectangle from preset #1."),
CommandDefaultHotkey()> Public Shared ReadOnly Property CreateRectangleWithPreset1 As RoutedUICommand
        Get
            Return commands("CreateRectangleWithPreset1")
        End Get
    End Property
    <CommandHelpText("Creates a new rectangle from preset #2."),
    CommandDefaultHotkey()> Public Shared ReadOnly Property CreateRectangleWithPreset2 As RoutedUICommand
        Get
            Return commands("CreateRectangleWithPreset2")
        End Get
    End Property
    <CommandHelpText("Creates a new rectangle from preset #3."),
    CommandDefaultHotkey()> Public Shared ReadOnly Property CreateRectangleWithPreset3 As RoutedUICommand
        Get
            Return commands("CreateRectangleWithPreset3")
        End Get
    End Property
    <CommandHelpText("Creates a new rectangle from preset #4."),
    CommandDefaultHotkey()> Public Shared ReadOnly Property CreateRectangleWithPreset4 As RoutedUICommand
        Get
            Return commands("CreateRectangleWithPreset4")
        End Get
    End Property

    <CommandHelpText("Flips the selected arrow's direction."),
    CommandDefaultHotkey()> Public Shared ReadOnly Property ToggleArrowDirection As RoutedUICommand
        Get
            Return commands("ToggleArrowDirection")
        End Get
    End Property

    <CommandHelpText("Tabs through the open charts."),
    CommandDefaultHotkey(Key.Decimal)> Public Shared ReadOnly Property SwitchChart As RoutedUICommand
        Get
            Return commands("SwitchChart")
        End Get
    End Property
    <CommandHelpText("Creates a copy of the selected object."),
    CommandDefaultHotkey(Key.Multiply)> Public Shared ReadOnly Property CreateObjectDuplicate As RoutedUICommand
        Get
            Return commands("CreateObjectDuplicate")
        End Get
    End Property
    <CommandHelpText("Opens the format analysis techniques window."),
    CommandDefaultHotkey(ModifierKeys.Control, Key.A)> Public Shared ReadOnly Property FormatAnalysisTechniques As RoutedUICommand
        Get
            Return commands("FormatAnalysisTechniques")
        End Get
    End Property
    <CommandHelpText("Refreshes all analysis techniques currently enabled for the chart."),
    CommandDefaultHotkey(Key.F5)> Public Shared ReadOnly Property RefreshAllAnalysisTechniques As RoutedUICommand
        Get
            Return commands("RefreshAllAnalysisTechniques")
        End Get
    End Property
    <CommandHelpText("Moves the selected order one increment up."),
    CommandDefaultHotkey(Key.PageUp)> Public Shared ReadOnly Property MoveOrderUp As RoutedUICommand
        Get
            Return commands("MoveOrderUp")
        End Get
    End Property
    <CommandHelpText("Moves the selected order one increment down."),
    CommandDefaultHotkey(Key.PageDown)> Public Shared ReadOnly Property MoveOrderDown As RoutedUICommand
        Get
            Return commands("MoveOrderDown")
        End Get
    End Property
    '<CommandHelpText("Increases the range for the selected chart by the minimum move."),
    'CommandDefaultHotkey()> Public Shared ReadOnly Property IncreaseRange As RoutedUICommand
    '    Get
    '        Return commands("IncreaseRange")
    '    End Get
    'End Property
    '<CommandHelpText("Decreases the range for the selected chart by the minimum move."),
    'CommandDefaultHotkey()> Public Shared ReadOnly Property DecreaseRange As RoutedUICommand
    '    Get
    '        Return commands("DecreaseRange")
    '    End Get
    'End Property
    '<CommandHelpText("Increases the range for the selected chart by twice the minimum move."),
    'CommandDefaultHotkey()> Public Shared ReadOnly Property IncreaseRange2xMinMove As RoutedUICommand
    '    Get
    '        Return commands("IncreaseRange2xMinMove")
    '    End Get
    'End Property
    '<CommandHelpText("Decreases the range for the selected chart by twice the minimum move."),
    'CommandDefaultHotkey()> Public Shared ReadOnly Property DecreaseRange2xMinMove As RoutedUICommand
    '    Get
    '        Return commands("DecreaseRange2xMinMove")
    '    End Get
    'End Property
    <CommandHelpText("Reloads data from IB and refreshes the chart."),
    CommandDefaultHotkey()> Public Shared ReadOnly Property RefreshChart As RoutedUICommand
        Get
            Return commands("RefreshChart")
        End Get
    End Property
    <CommandHelpText("Loads all the charts in the current workspace."),
    CommandDefaultHotkey()> Public Shared ReadOnly Property LoadWorkspaceCharts As RoutedUICommand
        Get
            Return commands("LoadWorkspaceCharts")
        End Get
    End Property
    <CommandHelpText("Creates a new workspace with 3 timeframes of the chosen symbol, stacked vertically."),
    CommandDefaultHotkey()> Public Shared ReadOnly Property LoadSymbolChartGroup As RoutedUICommand
        Get
            Return commands("LoadSymbolChartGroup")
        End Get
    End Property


    '<CommandHelpText(),
    'CommandDefaultHotkey()> Public Shared ReadOnly Property LoadSymbolChartGroup As RoutedUICommand
    '    Get
    '        Return commands("LoadSymbolChartGroup")
    '    End Get
    'End Property

    '<CommandHelpText("Sets the current scaling to preset 1."),
    'CommandDefaultHotkey(ModifierKeys.Control, Key.F1)> Public Shared ReadOnly Property SetScalingAsPreset1 As RoutedUICommand
    '    Get
    '        Return commands("SetScalingAsPreset1")
    '    End Get
    'End Property
    '<CommandHelpText("Sets the current scaling to preset 2."),
    'CommandDefaultHotkey(ModifierKeys.Control, Key.F2)> Public Shared ReadOnly Property SetScalingAsPreset2 As RoutedUICommand
    '    Get
    '        Return commands("SetScalingAsPreset2")
    '    End Get
    'End Property
    '<CommandHelpText("Snaps the scaling to preset 1."),
    'CommandDefaultHotkey(Key.F1)> Public Shared ReadOnly Property SnapScalingToPreset1 As RoutedUICommand
    '    Get
    '        Return commands("SnapScalingToPreset1")
    '    End Get
    'End Property
    '<CommandHelpText("Snaps the scaling to preset 2."),
    'CommandDefaultHotkey(Key.F2)> Public Shared ReadOnly Property SnapScalingToPreset2 As RoutedUICommand
    '    Get
    '        Return commands("SnapScalingToPreset2")
    '    End Get
    'End Property


    '<CommandHelpText(""),
    'CommandDefaultHotkey()> Public Shared ReadOnly Property SwitchAutoTrendColoring As RoutedUICommand
    '    Get
    '        Return commands("SwitchAutoTrendColoring")
    '    End Get
    'End Property
    '<CommandHelpText(""),
    'CommandDefaultHotkey()> Public Shared ReadOnly Property ToggleAutoTrend As RoutedUICommand
    '    Get
    '        Return commands("ToggleAutoTrend")
    '    End Get
    'End Property
    '<CommandHelpText(""),
    'CommandDefaultHotkey()> Public Shared ReadOnly Property ToggleRegressionCurve As RoutedUICommand
    '    Get
    '        Return commands("ToggleRegressionCurve")
    '    End Get
    'End Property
End Class
