Imports System
Imports System.Globalization
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Data
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Threading
Imports Calendar = System.Windows.Controls.Calendar
Imports CalendarMode = System.Windows.Controls.CalendarMode
Imports CalendarModeChangedEventArgs = System.Windows.Controls.CalendarModeChangedEventArgs
Imports DatePicker = System.Windows.Controls.DatePicker
Imports System.Runtime.CompilerServices
Public Class DatePickerCalendar

    Public Shared ReadOnly IsMonthYearProperty As DependencyProperty = DependencyProperty.RegisterAttached("IsMonthYear", GetType(Boolean), GetType(DatePickerCalendar), New PropertyMetadata(AddressOf OnIsMonthYearChanged))

    Public Shared Function GetIsMonthYear(ByVal dobj As DependencyObject) As Boolean
        Return CBool(dobj.GetValue(IsMonthYearProperty))
    End Function

    Public Shared Sub SetIsMonthYear(ByVal dobj As DependencyObject, ByVal value As Boolean)
        dobj.SetValue(IsMonthYearProperty, value)
    End Sub

    Private Shared Sub OnIsMonthYearChanged(ByVal dobj As DependencyObject, ByVal e As DependencyPropertyChangedEventArgs)
        Dim datePicker = CType(dobj, DatePicker)
        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, New Action(Of DatePicker, DependencyPropertyChangedEventArgs)(AddressOf SetCalendarEventHandlers), datePicker, e)
    End Sub

    Private Shared Sub SetCalendarEventHandlers(ByVal datePicker As DatePicker, ByVal e As DependencyPropertyChangedEventArgs)
        If e.NewValue = e.OldValue Then Return
        If CBool(e.NewValue) Then
            AddHandler datePicker.CalendarOpened, AddressOf DatePickerOnCalendarOpened
            AddHandler datePicker.CalendarClosed, AddressOf DatePickerOnCalendarClosed
        Else
            RemoveHandler datePicker.CalendarOpened, AddressOf DatePickerOnCalendarOpened
            RemoveHandler datePicker.CalendarClosed, AddressOf DatePickerOnCalendarClosed
        End If
    End Sub

    Private Shared Sub DatePickerOnCalendarOpened(ByVal sender As Object, ByVal routedEventArgs As RoutedEventArgs)
        Dim calendar = GetDatePickerCalendar(sender)
        calendar.DisplayMode = CalendarMode.Year
        AddHandler calendar.DisplayModeChanged, AddressOf CalendarOnDisplayModeChanged
    End Sub

    Private Shared Sub DatePickerOnCalendarClosed(ByVal sender As Object, ByVal routedEventArgs As RoutedEventArgs)
        Dim datePicker = CType(sender, DatePicker)
        Dim calendar = GetDatePickerCalendar(sender)
        datePicker.SelectedDate = calendar.SelectedDate
        RemoveHandler calendar.DisplayModeChanged, AddressOf CalendarOnDisplayModeChanged
    End Sub

    Private Shared Sub CalendarOnDisplayModeChanged(ByVal sender As Object, ByVal e As CalendarModeChangedEventArgs)
        Dim calendar = CType(sender, Calendar)
        If calendar.DisplayMode <> CalendarMode.Month Then Return
        calendar.SelectedDate = GetSelectedCalendarDate(calendar.DisplayDate)
        Dim datePicker = GetCalendarsDatePicker(calendar)
        datePicker.IsDropDownOpen = False
    End Sub

    Private Shared Function GetDatePickerCalendar(ByVal sender As Object) As Calendar
        Dim datePicker = CType(sender, DatePicker)
        Dim popup = CType(datePicker.Template.FindName("PART_Popup", datePicker), Popup)
        Return (CType(popup.Child, Calendar))
    End Function

    Private Shared Function GetCalendarsDatePicker(ByVal child As FrameworkElement) As DatePicker
        Dim parent = CType(child.Parent, FrameworkElement)
        If parent.Name = "PART_Root" Then Return CType(parent.TemplatedParent, DatePicker)
        Return GetCalendarsDatePicker(parent)
    End Function

    Private Shared Function GetSelectedCalendarDate(ByVal selectedDate As DateTime?) As DateTime?
        If Not selectedDate.HasValue Then Return Nothing
        Return New DateTime(selectedDate.Value.Year, selectedDate.Value.Month, 1)
    End Function
End Class

Public Class DatePickerDateFormat

    Public Shared ReadOnly DateFormatProperty As DependencyProperty = DependencyProperty.RegisterAttached("DateFormat", GetType(String), GetType(DatePickerDateFormat), New PropertyMetadata(AddressOf OnDateFormatChanged))

    Public Shared Function GetDateFormat(ByVal dobj As DependencyObject) As String
        Return CStr(dobj.GetValue(DateFormatProperty))
    End Function

    Public Shared Sub SetDateFormat(ByVal dobj As DependencyObject, ByVal value As String)
        dobj.SetValue(DateFormatProperty, value)
    End Sub

    Private Shared Sub OnDateFormatChanged(ByVal dobj As DependencyObject, ByVal e As DependencyPropertyChangedEventArgs)
        Dim datePicker = CType(dobj, DatePicker)
        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, New Action(Of DatePicker)(AddressOf ApplyDateFormat), datePicker)
    End Sub

    Private Shared Sub ApplyDateFormat(ByVal datePicker As DatePicker)
        Dim binding = New Binding("SelectedDate") With {.RelativeSource = New RelativeSource With {.AncestorType = GetType(DatePicker)}, .Converter = New DatePickerDateTimeConverter(), .ConverterParameter = New Tuple(Of DatePicker, String)(datePicker, GetDateFormat(datePicker)), .StringFormat = GetDateFormat(datePicker)}
        Dim textBox = GetTemplateTextBox(datePicker)
        textBox.SetBinding(TextBox.TextProperty, binding)
        RemoveHandler textBox.PreviewKeyDown, AddressOf TextBoxOnPreviewKeyDown
        AddHandler textBox.PreviewKeyDown, AddressOf TextBoxOnPreviewKeyDown
        Dim dropDownButton = GetTemplateButton(datePicker)
        RemoveHandler datePicker.CalendarOpened, AddressOf DatePickerOnCalendarOpened
        AddHandler datePicker.CalendarOpened, AddressOf DatePickerOnCalendarOpened
        RemoveHandler dropDownButton.PreviewMouseUp, AddressOf DropDownButtonPreviewMouseUp
        AddHandler dropDownButton.PreviewMouseUp, AddressOf DropDownButtonPreviewMouseUp
    End Sub

    Private Shared Function GetTemplateButton(ByVal datePicker As DatePicker) As ButtonBase
        Return CType(datePicker.Template.FindName("PART_Button", datePicker), ButtonBase)
    End Function

    Private Shared Sub DropDownButtonPreviewMouseUp(ByVal sender As Object, ByVal e As MouseButtonEventArgs)
        Dim fe = TryCast(sender, FrameworkElement)
        If fe Is Nothing Then Return
        Dim datePicker = fe.TryFindParent(Of DatePicker)()
        If datePicker Is Nothing OrElse datePicker.SelectedDate Is Nothing Then Return
        Dim dropDownButton = GetTemplateButton(datePicker)
        If e.OriginalSource Is dropDownButton AndAlso datePicker.IsDropDownOpen = False Then
            datePicker.SetCurrentValue(DatePicker.IsDropDownOpenProperty, True)
            datePicker.SetCurrentValue(DatePicker.DisplayDateProperty, datePicker.SelectedDate.Value)
            dropDownButton.ReleaseMouseCapture()
            e.Handled = True
        End If
    End Sub

    Private Shared Function GetTemplateTextBox(ByVal control As Control) As TextBox
        control.ApplyTemplate()
        Return CType(control?.Template?.FindName("PART_TextBox", control), TextBox)
    End Function

    Private Shared Sub TextBoxOnPreviewKeyDown(ByVal sender As Object, ByVal e As KeyEventArgs)
        If e.Key <> Key.[Return] Then Return
        e.Handled = True
        Dim textBox = CType(sender, TextBox)
        Dim datePicker = CType(textBox.TemplatedParent, DatePicker)
        Dim dateStr = textBox.Text
        Dim formatStr = GetDateFormat(datePicker)
        datePicker.SelectedDate = DatePickerDateTimeConverter.StringToDateTime(datePicker, formatStr, dateStr)
    End Sub

    Private Shared Sub DatePickerOnCalendarOpened(ByVal sender As Object, ByVal e As RoutedEventArgs)
        Dim datePicker = CType(sender, DatePicker)
        Dim textBox = GetTemplateTextBox(datePicker)
        Dim formatStr = GetDateFormat(datePicker)
        textBox.Text = DatePickerDateTimeConverter.DateTimeToString(formatStr, datePicker.SelectedDate)
    End Sub

    Private Class DatePickerDateTimeConverter
        Implements IValueConverter

        Public Function Convert(ByVal value As Object, ByVal targetType As Type, ByVal parameter As Object, ByVal culture As CultureInfo) As Object Implements IValueConverter.Convert
            Dim formatStr = (CType(parameter, Tuple(Of DatePicker, String))).Item2
            Dim selectedDate = CType(value, Date?)
            Return DateTimeToString(formatStr, selectedDate)
        End Function

        Public Function ConvertBack(ByVal value As Object, ByVal targetType As Type, ByVal parameter As Object, ByVal culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
            Dim tupleParam = (CType(parameter, Tuple(Of DatePicker, String)))
            Dim dateStr = CStr(value)
            Return StringToDateTime(tupleParam.Item1, tupleParam.Item2, dateStr)
        End Function

        Public Shared Function DateTimeToString(ByVal formatStr As String, ByVal selectedDate As DateTime?) As String
            Return If(selectedDate.HasValue, selectedDate.Value.ToString(formatStr), Nothing)
        End Function

        Public Shared Function StringToDateTime(ByVal datePicker As DatePicker, ByVal formatStr As String, ByVal dateStr As String) As DateTime?
            Dim [date] As DateTime
            Dim canParse = DateTime.TryParseExact(dateStr, formatStr, CultureInfo.CurrentCulture, DateTimeStyles.None, [date])
            If Not canParse Then canParse = DateTime.TryParse(dateStr, CultureInfo.CurrentCulture, DateTimeStyles.None, [date])
            Return If(canParse, [date], datePicker.SelectedDate)
        End Function
    End Class
End Class

Module FEExten

    <Extension()>
    Function TryFindParent(Of T As DependencyObject)(ByVal child As DependencyObject) As T
        Dim parentObject As DependencyObject = GetParentObject(child)
        If parentObject Is Nothing Then Return Nothing
        Dim parent As T = TryCast(parentObject, T)
        If parent IsNot Nothing Then
            Return parent
        Else
            Return TryFindParent(Of T)(parentObject)
        End If
    End Function

    <Extension()>
    Function GetParentObject(ByVal child As DependencyObject) As DependencyObject
        If child Is Nothing Then Return Nothing
        Dim contentElement As ContentElement = TryCast(child, ContentElement)
        If contentElement IsNot Nothing Then
            Dim parent As DependencyObject = ContentOperations.GetParent(contentElement)
            If parent IsNot Nothing Then Return parent
            Dim fce As FrameworkContentElement = TryCast(contentElement, FrameworkContentElement)
            Return If(fce IsNot Nothing, fce.Parent, Nothing)
        End If

        Dim frameworkElement As FrameworkElement = TryCast(child, FrameworkElement)
        If frameworkElement IsNot Nothing Then
            Dim parent As DependencyObject = frameworkElement.Parent
            If parent IsNot Nothing Then Return parent
        End If

        Return VisualTreeHelper.GetParent(child)
    End Function
End Module
