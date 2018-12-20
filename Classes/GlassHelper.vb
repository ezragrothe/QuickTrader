Imports System.Runtime.InteropServices
Imports System.Windows.Interop

Public Class GlassHelper
    <DllImport("DwmApi.dll")> Public Shared Function DwmExtendFrameIntoClientArea(hwnd As IntPtr, ByRef pMarInset As Margins) As Integer
    End Function
    <StructLayout(LayoutKind.Sequential)>
    Public Structure Margins
        Public cxLeftWidth As Integer
        Public cxRightWidth As Integer
        Public cxTopHeight As Integer
        Public cxBottomHeight As Integer
    End Structure
    Public Shared Function GetDpiAdjustedMargins(windowHandle As IntPtr, left As Integer, right As Integer, top As Integer, bottom As Integer) As Margins
        Dim desktop As System.Drawing.Graphics = System.Drawing.Graphics.FromHwnd(windowHandle)
        Dim desktopDpiX As Single = desktop.DpiX
        Dim desktopDpiY As Single = desktop.DpiY
        Dim margins As New Margins
        margins.cxLeftWidth = Convert.ToInt32(left * (desktopDpiX / 96))
        margins.cxRightWidth = Convert.ToInt32(right * (desktopDpiX / 96))
        margins.cxTopHeight = Convert.ToInt32(bottom * (desktopDpiY / 96))
        margins.cxBottomHeight = Convert.ToInt32(top * (desktopDpiY / 96))
        Return margins
    End Function
    Public Shared Sub ExtendGlass(win As Window, left As Integer, top As Integer, right As Integer, bottom As Integer)
        Dim windowInterop As New WindowInteropHelper(win)
        Dim windowHandle As IntPtr = windowInterop.Handle
        Dim mainWindowSrc As HwndSource = HwndSource.FromHwnd(windowHandle)
        mainWindowSrc.CompositionTarget.BackgroundColor = Colors.Transparent

        Dim margins As Margins = GetDpiAdjustedMargins(windowHandle, left, right, bottom, top)
        Dim returnVal As Integer = DwmExtendFrameIntoClientArea(mainWindowSrc.Handle, margins)
        If returnVal < 0 Then
            Throw New NotSupportedException("Could not adjust window glass border.")
        End If
    End Sub
End Class
