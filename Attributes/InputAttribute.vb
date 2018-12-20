''' <summary>
''' Indicates an input for an analysis technique. This class cannot be inherited.
''' </summary>
Public NotInheritable Class InputAttribute
    Inherits Attribute
    ' ''' <summary>
    ' ''' Creates a new, empty InputAttribute.
    ' ''' </summary>
    'Public Sub New()

    'End Sub
    ' ''' <summary>
    ' ''' Creates a new InputAttribute with the specified descriptive text.
    ' ''' </summary>
    ' ''' <param name="descriptiveText">The descriptive text for the input.</param>
    'Public Sub New(ByVal descriptiveText As String)
    '    Me.DescriptiveText = descriptiveText
    'End Sub
    ''' <summary>
    ''' Creates a new InputAttribute with the specified parameters.
    ''' </summary>
    ''' <param name="descriptiveText">The descriptive text for the input.</param>
    ''' <param name="categoryName">The category name for the input.</param>
    Public Sub New(Optional ByVal descriptiveText As String = "", Optional ByVal categoryName As String = "")
        Me.DescriptiveText = descriptiveText
        Me.CategoryName = categoryName
    End Sub
    ''' <summary>
    ''' Gets or sets the descriptive text for the input.
    ''' </summary>
    ''' <returns>The descriptive text for the input.</returns>
    Public Property DescriptiveText As String = ""
    ''' <summary>
    ''' Gets or sets the category name for the input.
    ''' </summary>
    ''' <returns>The category name for the input.</returns>
    Public Property CategoryName As String = ""
    ''' <summary>
    ''' Gets whether the input specifies a category name.
    ''' </summary>
    ''' <returns>true if the input specifies a category name, otherwise, false.</returns>
    Public ReadOnly Property HasCategoryName As Boolean
        Get
            Return CategoryName <> ""
        End Get
    End Property
End Class
