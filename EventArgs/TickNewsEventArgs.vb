' Copyright (C) 2018 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
' and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable.

Public Class TickNewsEventArgs

    Property tickerId As Integer

    Property timeStamp As Long

    Property providerCode As String

    Property articleId As String

    Property headline As String

    Property extraData As String

End Class

