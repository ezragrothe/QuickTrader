Public Class CommandDefaultHotkeyAttribute
    Inherits Attribute
    'Private Sub New(Optional ByVal DefaultHotkey As Key = Key.None, Optional ByVal DefaultHotkeyModifiers As ModifierKeys = ModifierKeys.None)

    'End Sub
    Public Sub New()

    End Sub
    Public Sub New(ByVal hotkey As Key)
        DefaultHotkey = hotkey
    End Sub
    Public Sub New(ByVal modifiers As ModifierKeys, ByVal hotkey As Key)
        DefaultHotkey = hotkey
        DefaultHotkeyModifiers = modifiers
    End Sub
    Public Sub New(ByVal modifiers As ModifierKeys, ByVal hotkey As Key, ByVal shortcutText As String)
        DefaultHotkey = hotkey
        DefaultHotkeyModifiers = modifiers
        Me.ShortcutText = shortcutText
    End Sub
    Public Property DefaultHotkey As Key = Key.None
    Public Property DefaultHotkeyModifiers As ModifierKeys = ModifierKeys.None
    Public Property ShortcutText As String
End Class
