Imports System.Windows.Threading

Public Class DispatcherHelper
    Private Shared exitFrameCallback As New DispatcherOperationCallback(AddressOf ExitFrame)
    Public Shared Sub DoEvents()
        Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, New Action(Sub()
                                                                                        End Sub))

        'Dim nestedFrame As New DispatcherFrame()
        'Dim exitOperation As DispatcherOperation = Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, exitFrameCallback, nestedFrame)
        'Dispatcher.PushFrame(nestedFrame)
        'If exitOperation.Status <> DispatcherOperationStatus.Completed Then
        '    exitOperation.Abort()
        'End If
    End Sub

    Private Shared Function ExitFrame(ByVal state As Object) As Object
        Dim frame As DispatcherFrame = CType(state, DispatcherFrame)
        frame.Continue = False
        Return Nothing
    End Function

End Class
