Public Interface ISerializable
    Function Serialize() As String
    Sub Deserialize(ByVal serializedString As String)
End Interface
