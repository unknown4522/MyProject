Public Class items
    Public Property item_name As String
    Public Property model As String
    Public Property brand As String
    Public Property serial_number As String
    Public Property item_type As String
    Public Property room_name As String
    Public Property status As String
    Public Property department As String
    Public Property date_now As String
    Public Property campus_name As String
End Class
Public Class employee
    Public Property employee_name As String
    Public Property campus_name As String

End Class
Public Class campus

    Public Property campus_name As String

End Class
Public Class brand
    Public Property brandname As String
    Public Property campus_name As String

End Class
Public Class grade
    Public Property grade_level As String
    Public Property campus_name As String

End Class
Public Class size_list
    Public Property size As String
    Public Property campus_name As String

End Class
Public Class clothes_list
    Public Property clothes_type As String
    Public Property campus_name As String


End Class
Public Class log_in
    Public Property employee_id As String
    Public Property employee_name As String
    Public Property employee_last_name As String
    Public Property campus_name As String
    Public Property employee_user_name As String
    Public Property employee_password As String
    Public Property role As String

End Class
Public Class clothes_log
    Public Property transaction_date As String
    Public Property quantity As String
    Public Property size As String
    Public Property transaction As String
    Public Property grade_level As String
    Public Property first_name As String
    Public Property last_name As String
    Public Property campus_name As String
    Public Property clothes_type As String
    Public Property id As String
End Class
Public Class room
    Public Property room_id As String
    Public Property room_name As String
    Public Property room_description As String
    Public Property floorlevel As String
    Public Property campus_name As String

End Class
Public Class uniform_stock
    Public Property grade_level As String
    Public Property clothes_type As String
    Public Property size As String
    Public Property quantity As String
    Public Property uniform_id As String
    Public Property transaction As String
    Public Property campus_name As String
End Class

Public Class history
    Public Property item_name As String
    Public Property serial_number As String
    Public Property date_transfer As String
    Public Property last_location As String
    Public Property current_location As String
    Public Property transfer_by As String
    Public Property requested_by As String
    Public Property campus_name As String
End Class
Public Class claim_stub
    Public Property first_name As String
    Public Property last_name As String
    Public Property apparel_name As String
    Public Property reciept_number As String
    Public Property date_recieve As String
    Public Property claim_id As String
    Public Property grade_level As String
    Public Property campus_name As String
    Public Property quantity As String
    Public Property size As String
End Class