Imports System.Net
Imports System.Net.Http
Imports System.Security.Claims
Imports System.Web.Http
Imports MySql.Data.MySqlClient
Imports Newtonsoft.Json

Namespace Controllers
    <RoutePrefix("api/inventory")>
    Public Class InventorySystemController
        Inherits ApiController
        Dim identity As New ClaimsIdentity(User.Identity)
        Private response As HttpResponseMessage
        Dim ds As DataSet = New DataSet


        'SELECT ALL FROM LOGIN TABLE

        <Route("account/list", Name:="Get_account_list")>
        <Authorize(Roles:="Admin")>
        Public Function Get_account_list() As IHttpActionResult
            Dim stats As List(Of log_in) = New List(Of log_in)
            Dim sqlConn As New MySqlConnection(ConfigurationManager.ConnectionStrings("constr").ConnectionString)

            Try
                If sqlConn.State = ConnectionState.Closed Then
                    sqlConn.Open()
                End If

                Using msqlcom As New MySqlCommand("SELECT * FROM employee_acc", sqlConn)
                    Using dtReader As MySqlDataReader = msqlcom.ExecuteReader()
                        If dtReader.HasRows = True Then
                            While dtReader.Read()
                                Dim dataObj As New log_in
                                With dataObj
                                    .employee_id = dtReader("employee_id").ToString()
                                    .employee_name = dtReader("employee_name").ToString()
                                    .employee_last_name = dtReader("employee_last_name").ToString()
                                    .campus_name = dtReader("campus_name").ToString()
                                    .employee_user_name = dtReader("employee_user_name").ToString()
                                    .employee_password = dtReader("employee_password").ToString()
                                    .role = dtReader("role").ToString()
                                End With
                                stats.Add(dataObj)
                            End While
                            Return Ok(stats)
                        Else
                            Return NotFound()
                        End If
                    End Using
                End Using
            Catch ex As Exception
                Return InternalServerError(ex)
            Finally
                If sqlConn.State = ConnectionState.Open Then
                    sqlConn.Close()
                End If
                sqlConn.Dispose()
            End Try
        End Function




        'LOG IN

        <Route("Login/all", Name:="Post_login_acc")>
        Public Function Post_login_acc(ByVal employee_user_name As String, ByVal employee_password As String) As HttpResponseMessage
            Dim response As HttpResponseMessage

            Try
                Using sqlConn As New MySqlConnection(ConfigurationManager.ConnectionStrings("constr").ConnectionString)
                    sqlConn.Open()

                    ' Check if the username and password exist in the "login" table
                    Using cmdCheckCredentials As New MySqlCommand()
                        cmdCheckCredentials.Connection = sqlConn
                        cmdCheckCredentials.CommandText = "SELECT COUNT(*) FROM employee_acc WHERE employee_user_name = @employee_user_name AND employee_password = @employee_password"
                        cmdCheckCredentials.Parameters.AddWithValue("@employee_user_name", employee_user_name)
                        cmdCheckCredentials.Parameters.AddWithValue("@employee_password", employee_password)

                        Dim credentialsCount As Integer = Convert.ToInt32(cmdCheckCredentials.ExecuteScalar())

                        If credentialsCount = 0 Then
                            ' To prevent enumeration attacks, don't reveal if the username or password is incorrect
                            response = Request.CreateResponse(HttpStatusCode.Unauthorized)
                            response.Content = New StringContent("Invalid username or password", Encoding.UTF8)
                        Else
                            response = Request.CreateResponse(HttpStatusCode.OK)
                            response.Content = New StringContent("Successfully Logged In", Encoding.UTF8)
                        End If
                    End Using
                End Using
            Catch ex As MySqlException
                ' Handle database connection issues or SQL errors
                response = Request.CreateResponse(HttpStatusCode.InternalServerError)
                response.Content = New StringContent("Server Error: Unable to process the request", Encoding.UTF8)
            Catch ex As Exception
                ' Log application errors, but avoid exposing details in the response
                response = Request.CreateResponse(HttpStatusCode.InternalServerError)
                response.Content = New StringContent("Server Error: Unable to process the request", Encoding.UTF8)
            End Try

            Return response
        End Function



        'CREATE ACCOUNT

        <Route("create/acc", Name:="Post_create_acc")>
        Public Function Post_create_acc(ByVal user_name As String, ByVal password As String, ByVal firstname As String, ByVal lastname As String) As HttpResponseMessage
            Dim response As HttpResponseMessage

            Try
                Using sqlConn As New MySqlConnection(ConfigurationManager.ConnectionStrings("constr").ConnectionString)
                    sqlConn.Open()

                    Using cmdInsert As New MySqlCommand()
                        cmdInsert.Connection = sqlConn
                        cmdInsert.CommandText = "INSERT INTO employee_acc (employee_id, employee_name, employee_last_name, campus_name, employee_user_name, employee_password) VALUES (@user_name, @password, @firstname, @lastname)"
                        cmdInsert.Parameters.AddWithValue("@employee_id", user_name)
                        cmdInsert.Parameters.AddWithValue("@employee_name", password)
                        cmdInsert.Parameters.AddWithValue("@employee_last_name", firstname)
                        cmdInsert.Parameters.AddWithValue("@campus_name", lastname)
                        cmdInsert.Parameters.AddWithValue("@employee_user_name", lastname)
                        cmdInsert.Parameters.AddWithValue("@employee_password", lastname)

                        Dim rowsAffected = cmdInsert.ExecuteNonQuery()

                        If rowsAffected > 0 Then
                            response = Request.CreateResponse(HttpStatusCode.OK)
                            response.Content = New StringContent("Successfully added employee", Encoding.UTF8)
                        Else
                            response = Request.CreateResponse(HttpStatusCode.InternalServerError)
                            response.Content = New StringContent("Failed to add employee", Encoding.UTF8)
                        End If
                    End Using
                End Using

            Catch ex As MySqlException
                response = Request.CreateResponse(HttpStatusCode.InternalServerError)
                response.Content = New StringContent("Database Error: " & ex.Message, Encoding.UTF8)
            Catch ex As Exception
                response = Request.CreateResponse(HttpStatusCode.InternalServerError)
                response.Content = New StringContent("Application Error: " & ex.Message, Encoding.UTF8)
            End Try

            Return response
        End Function


        'ADD NAME HAVE NO ACCOUNT

        <Route("add/employeename", Name:="Post_add_employeename")>
        Public Function Post_add_employeename(ByVal employee_name As String, ByVal campus_name As String) As HttpResponseMessage
            Dim response As HttpResponseMessage

            Try
                Using sqlConn As New MySqlConnection(ConfigurationManager.ConnectionStrings("constr").ConnectionString)
                    sqlConn.Open()

                    ' Check if the employee_name and campus_name already exist
                    Using cmdCheckExistence As New MySqlCommand()
                        cmdCheckExistence.Connection = sqlConn
                        cmdCheckExistence.CommandText = "SELECT COUNT(*) FROM employee_acc WHERE employee_name = @employee_name AND campus_name = @campus_name"
                        cmdCheckExistence.Parameters.AddWithValue("@employee_name", employee_name)
                        cmdCheckExistence.Parameters.AddWithValue("@campus_name", campus_name)

                        Dim existingRecordsCount As Integer = Convert.ToInt32(cmdCheckExistence.ExecuteScalar())

                        If existingRecordsCount > 0 Then
                            ' Employee_name and campus_name already exist, return an error response
                            response = Request.CreateResponse(HttpStatusCode.BadRequest)
                            response.Content = New StringContent("Employee with the same name already exists", Encoding.UTF8)
                            Return response
                        End If
                    End Using

                    ' If not exists, proceed with the insertion
                    Using cmdInsert As New MySqlCommand()
                        cmdInsert.Connection = sqlConn
                        cmdInsert.CommandText = "INSERT INTO employee_acc (employee_name, campus_name) VALUES (@employee_name, @campus_name)"
                        cmdInsert.Parameters.AddWithValue("@employee_name", employee_name)
                        cmdInsert.Parameters.AddWithValue("@campus_name", campus_name)

                        Dim rowsAffected = cmdInsert.ExecuteNonQuery()

                        If rowsAffected > 0 Then
                            response = Request.CreateResponse(HttpStatusCode.OK)
                            response.Content = New StringContent("Successfully added employee", Encoding.UTF8)
                        Else
                            response = Request.CreateResponse(HttpStatusCode.InternalServerError)
                            response.Content = New StringContent("Failed to add employee", Encoding.UTF8)
                        End If
                    End Using
                End Using

            Catch ex As MySqlException
                response = Request.CreateResponse(HttpStatusCode.InternalServerError)
                response.Content = New StringContent("Database Error: " & ex.Message, Encoding.UTF8)
            Catch ex As Exception
                response = Request.CreateResponse(HttpStatusCode.InternalServerError)
                response.Content = New StringContent("Application Error: " & ex.Message, Encoding.UTF8)
            End Try

            Return response
        End Function




        'SHOW ALL ITEMS 

        <Route("item/all", Name:="Get_item_list")>
        Public Function Get_item_list(ByVal campus_name As String) As IHttpActionResult
            Dim stats As List(Of items) = New List(Of items)
            Using sqlConn As New MySqlConnection(ConfigurationManager.ConnectionStrings("constr").ConnectionString)
                If sqlConn.State = ConnectionState.Closed Then
                    Try
                        Using cmd As New MySqlCommand("GetItemList", sqlConn)
                            cmd.CommandType = CommandType.StoredProcedure
                            cmd.Parameters.AddWithValue("@campus_name_param", campus_name)

                            sqlConn.Open()
                            Using dtReader As MySqlDataReader = cmd.ExecuteReader()
                                If dtReader.HasRows = True Then
                                    While dtReader.Read()
                                        Dim dataObj As New items
                                        With dataObj
                                            .item_name = dtReader("item_name").ToString()
                                            .model = dtReader("model").ToString()
                                            .brand = dtReader("brand").ToString()
                                            .serial_number = dtReader("serial_number").ToString()
                                            .item_type = dtReader("item_type").ToString()
                                            .room_name = dtReader("room_name").ToString()
                                            .status = dtReader("status").ToString()
                                            .department = dtReader("department").ToString()
                                            .date_now = dtReader("date_now").ToString()
                                            .campus_name = dtReader("campus_name").ToString()
                                        End With
                                        stats.Add(dataObj)
                                    End While
                                    Return Ok(stats)
                                Else
                                    Return NotFound()
                                End If
                            End Using
                        End Using
                    Catch ex As Exception
                        Return InternalServerError(ex)
                    Finally
                        sqlConn.Close()
                    End Try
                Else
                    Return InternalServerError()
                End If
            End Using
        End Function




        'ADD ITEMS 
        <Route("add/newitems", Name:="Post_New_items")>
        Public Function Post_New_items(ByVal item_name As String, ByVal model As String, ByVal brand As String, ByVal serial_number As String, ByVal item_type As String, ByVal date_now As String, ByVal room_name As String, ByVal status As String, ByVal department As String, ByVal campus_name As String) As HttpResponseMessage
            Dim response As HttpResponseMessage

            Try
                Using sqlConn As New MySqlConnection(ConfigurationManager.ConnectionStrings("constr").ConnectionString)
                    sqlConn.Open()

                    Using cmd As New MySqlCommand("AddNewItems", sqlConn)
                        cmd.CommandType = CommandType.StoredProcedure
                        cmd.Parameters.AddWithValue("@item_name_param", item_name)
                        cmd.Parameters.AddWithValue("@model_param", model)
                        cmd.Parameters.AddWithValue("@brand_param", brand)
                        cmd.Parameters.AddWithValue("@serial_number_param", serial_number)
                        cmd.Parameters.AddWithValue("@item_type_param", item_type)
                        cmd.Parameters.AddWithValue("@date_now_param", date_now)
                        cmd.Parameters.AddWithValue("@room_name_param", room_name)
                        cmd.Parameters.AddWithValue("@status_param", status)
                        cmd.Parameters.AddWithValue("@department_param", department)
                        cmd.Parameters.AddWithValue("@campus_name_param", campus_name)

                        cmd.ExecuteNonQuery()

                        response = New HttpResponseMessage(HttpStatusCode.OK)
                        response.Content = New StringContent("Successfully Created", Encoding.UTF8)
                        Return response
                    End Using
                End Using
            Catch ex As Exception
                response = New HttpResponseMessage(HttpStatusCode.InternalServerError)
                response.Content = New StringContent("Application Error: There is an error in performing this action, " & ex.ToString(), Encoding.UTF8)
                Return response
            End Try
        End Function


        'UPDATE ITEMS 

        <Route("update/items", Name:="Post_update_items")>
        Public Function Post_update_items(
    ByVal item_name As String,
    ByVal model As String,
    ByVal brand As String,
    ByVal serial_number As String,
    ByVal item_type As String,
    ByVal room_name As String,
    ByVal date_now As String,
    ByVal department As String,
    ByVal status As String
) As HttpResponseMessage
            Dim response As HttpResponseMessage

            Try
                ' Open a MySqlConnection using the connection string from the configuration
                Using sqlConn As New MySqlConnection(ConfigurationManager.ConnectionStrings("constr").ConnectionString)
                    sqlConn.Open()

                    ' Check if the room exists in the item list
                    Using cmdCheckRoom As New MySqlCommand("CheckRoomExistence", sqlConn)
                        cmdCheckRoom.CommandType = CommandType.StoredProcedure
                        cmdCheckRoom.Parameters.AddWithValue("@room_name_param", room_name)
                        cmdCheckRoom.Parameters.Add("@roomCount", MySqlDbType.Int32).Direction = ParameterDirection.Output

                        cmdCheckRoom.ExecuteNonQuery()
                        Dim roomCount As Integer = Convert.ToInt32(cmdCheckRoom.Parameters("@roomCount").Value)

                        ' If the room does not exist, return a BadRequest response
                        If roomCount = 0 Then
                            response = New HttpResponseMessage(HttpStatusCode.BadRequest)
                            response.Content = New StringContent("Room does not exist in the item list", Encoding.UTF8)
                            Return response
                        End If
                    End Using

                    ' Update the item in the itemlist table
                    Using cmdUpdateItem As New MySqlCommand("UpdateItem", sqlConn)
                        cmdUpdateItem.CommandType = CommandType.StoredProcedure
                        cmdUpdateItem.Parameters.AddWithValue("@item_name_param", item_name)
                        cmdUpdateItem.Parameters.AddWithValue("@model_param", model)
                        cmdUpdateItem.Parameters.AddWithValue("@brand_param", brand)
                        cmdUpdateItem.Parameters.AddWithValue("@serial_number_param", serial_number)
                        cmdUpdateItem.Parameters.AddWithValue("@item_type_param", item_type)
                        cmdUpdateItem.Parameters.AddWithValue("@department_param", department)
                        cmdUpdateItem.Parameters.AddWithValue("@room_name_param", room_name)
                        cmdUpdateItem.Parameters.AddWithValue("@status_param", status)

                        ' Execute the update command
                        cmdUpdateItem.ExecuteNonQuery()

                        ' Return a success response
                        response = New HttpResponseMessage(HttpStatusCode.OK)
                        response.Content = New StringContent("Successfully Updated", Encoding.UTF8)
                        Return response
                    End Using
                End Using
            Catch ex As Exception
                ' Handle exceptions and return an internal server error response
                response = New HttpResponseMessage(HttpStatusCode.InternalServerError)
                response.Content = New StringContent("Application Error: There is an error in performing this action, " & ex.ToString(), Encoding.UTF8)
                Return response
            End Try
        End Function





        'SHOW EMPLOYEE LIST
        <Route("employee/list", Name:="Get_employee_list")>
        Public Function Get_employee_list(ByVal campus_name As String) As IHttpActionResult
            Dim stats As List(Of employee) = New List(Of employee)
            Using sqlConn As New MySqlConnection(ConfigurationManager.ConnectionStrings("constr").ConnectionString)
                If sqlConn.State = ConnectionState.Closed Then
                    Try
                        Using cmd As New MySqlCommand("GetEmployeeList", sqlConn)
                            cmd.CommandType = CommandType.StoredProcedure
                            cmd.Parameters.AddWithValue("@campus_name_param", campus_name)

                            sqlConn.Open()
                            Using dtReader As MySqlDataReader = cmd.ExecuteReader()
                                If dtReader.HasRows = True Then
                                    While dtReader.Read()
                                        Dim dataObj As New employee
                                        With dataObj
                                            .employee_name = dtReader("employee_name").ToString()
                                            .campus_name = dtReader("campus_name").ToString()
                                        End With
                                        stats.Add(dataObj)
                                    End While
                                    Return Ok(stats)
                                Else
                                    Return NotFound()
                                End If
                            End Using
                        End Using
                    Catch ex As Exception
                        Return InternalServerError(ex)
                    Finally
                        sqlConn.Close()
                    End Try
                Else
                    Return InternalServerError()
                End If
            End Using
        End Function




        'ADD EMPLOYEE
        <Route("add/employee", Name:="Post_add_employee")>
        Public Function PostAddEmployee(ByVal employee_name As String, ByVal campus_name As String) As HttpResponseMessage
            Dim response As HttpResponseMessage

            Try
                Using sqlConn As New MySqlConnection(ConfigurationManager.ConnectionStrings("constr").ConnectionString)
                    sqlConn.Open()

                    Using cmdInsert As New MySqlCommand("AddEmployee", sqlConn)
                        cmdInsert.CommandType = CommandType.StoredProcedure
                        cmdInsert.Parameters.AddWithValue("@employee_name_param", employee_name)
                        cmdInsert.Parameters.AddWithValue("@campus_name_param", campus_name)
                        cmdInsert.Parameters.Add("@result", MySqlDbType.Int32).Direction = ParameterDirection.Output

                        cmdInsert.ExecuteNonQuery()
                        Dim rowsAffected As Integer = Convert.ToInt32(cmdInsert.Parameters("@result").Value)

                        If rowsAffected > 0 Then
                            response = Request.CreateResponse(HttpStatusCode.OK)
                            response.Content = New StringContent("Successfully added employee", Encoding.UTF8, "text/plain")
                        Else
                            response = Request.CreateResponse(HttpStatusCode.InternalServerError)
                            response.Content = New StringContent("Failed to add employee", Encoding.UTF8)
                        End If
                    End Using
                End Using
            Catch ex As MySqlException
                response = Request.CreateResponse(HttpStatusCode.InternalServerError)
                response.Content = New StringContent("Database Error: " & ex.Message, Encoding.UTF8)
            Catch ex As Exception
                response = Request.CreateResponse(HttpStatusCode.InternalServerError)
                response.Content = New StringContent("Application Error: " & ex.Message, Encoding.UTF8)
            End Try

            Return response
        End Function




        'SHOW ALL BRAND

        <Route("brand/list", Name:="Get_brand_list")>
        Public Function Get_brand_list(ByVal campus_name As String) As IHttpActionResult
            Dim stats As List(Of brand) = New List(Of brand)
            Using sqlConn As New MySqlConnection(ConfigurationManager.ConnectionStrings("constr").ConnectionString)
                If sqlConn.State = ConnectionState.Closed Then
                    Try
                        Using cmd As New MySqlCommand("GetBrandList", sqlConn)
                            cmd.CommandType = CommandType.StoredProcedure
                            cmd.Parameters.AddWithValue("@campus_name_param", campus_name)

                            sqlConn.Open()
                            Using dtReader As MySqlDataReader = cmd.ExecuteReader()
                                If dtReader.HasRows = True Then
                                    While dtReader.Read()
                                        Dim dataObj As New brand
                                        With dataObj
                                            .brandname = dtReader("brandname").ToString()
                                            .campus_name = dtReader("campus_name").ToString()
                                        End With
                                        stats.Add(dataObj)
                                    End While
                                    Return Ok(stats)
                                Else
                                    Return NotFound()
                                End If
                            End Using
                        End Using
                    Catch ex As Exception
                        Return InternalServerError(ex)
                    Finally
                        sqlConn.Close()
                    End Try
                Else
                    Return InternalServerError()
                End If
            End Using
        End Function





        'ADD BRAND 

        <Route("add/brand", Name:="Post_add_brand")>
        Public Function Post_add_brand(ByVal brandname As String, ByVal campus_name As String) As HttpResponseMessage
            Dim response As HttpResponseMessage

            Try
                Using sqlConn As New MySqlConnection(ConfigurationManager.ConnectionStrings("constr").ConnectionString)
                    sqlConn.Open()

                    Using cmdInsert As New MySqlCommand("AddBrand", sqlConn)
                        cmdInsert.CommandType = CommandType.StoredProcedure
                        cmdInsert.Parameters.AddWithValue("@brandname_param", brandname)
                        cmdInsert.Parameters.AddWithValue("@campus_name_param", campus_name)
                        cmdInsert.Parameters.Add("@result", MySqlDbType.Int32).Direction = ParameterDirection.Output

                        cmdInsert.ExecuteNonQuery()
                        Dim rowsAffected As Integer = Convert.ToInt32(cmdInsert.Parameters("@result").Value)

                        If rowsAffected > 0 Then
                            response = Request.CreateResponse(HttpStatusCode.OK)
                            response.Content = New StringContent("Successfully added brand", Encoding.UTF8)
                        ElseIf rowsAffected = 0 Then
                            response = Request.CreateResponse(HttpStatusCode.Conflict)
                            response.Content = New StringContent("Brand already exists.", Encoding.UTF8)
                        Else
                            response = Request.CreateResponse(HttpStatusCode.InternalServerError)
                            response.Content = New StringContent("Failed to add brand", Encoding.UTF8)
                        End If
                    End Using
                End Using
            Catch ex As MySqlException
                response = Request.CreateResponse(HttpStatusCode.InternalServerError)
                response.Content = New StringContent("Database Error: " & ex.Message, Encoding.UTF8)
            Catch ex As Exception
                response = Request.CreateResponse(HttpStatusCode.InternalServerError)
                response.Content = New StringContent("Application Error: " & ex.Message, Encoding.UTF8)
            End Try

            Return response
        End Function



        'INSIDE CAMPUS

        <Route("room/campus", Name:="Get_inside_campus")>
        Public Function Get_inside_campus(ByVal campus_name As String) As IHttpActionResult
            Dim stats As List(Of room) = New List(Of room)
            Using sqlConn As New MySqlConnection(ConfigurationManager.ConnectionStrings("constr").ConnectionString)
                If sqlConn.State = ConnectionState.Closed Then
                    Try
                        Using cmd As New MySqlCommand("GetRoomsInCampus", sqlConn)
                            cmd.CommandType = CommandType.StoredProcedure
                            cmd.Parameters.AddWithValue("@campus_name_param", campus_name)

                            sqlConn.Open()
                            Using dtReader As MySqlDataReader = cmd.ExecuteReader()
                                If dtReader.HasRows = True Then
                                    While dtReader.Read()
                                        Dim dataObj As New room
                                        With dataObj
                                            .room_name = dtReader("room_name").ToString()
                                            .room_description = dtReader("room_description").ToString()
                                            .floorlevel = dtReader("floorlevel").ToString()
                                            .room_id = dtReader("room_id").ToString()
                                            .campus_name = dtReader("campus_name").ToString()
                                        End With
                                        stats.Add(dataObj)
                                    End While
                                    Return Ok(stats)
                                Else
                                    Return NotFound()
                                End If
                            End Using
                        End Using
                    Catch ex As Exception
                        Return InternalServerError(ex)
                    Finally
                        sqlConn.Close()
                    End Try
                Else
                    Return InternalServerError()
                End If
            End Using
        End Function



        'SHOW ALL ROOMS

        <Route("room/list", Name:="Get_room_list")>
        Public Function Get_room_list() As IHttpActionResult
            Dim stats As List(Of room) = New List(Of room)
            Using sqlConn As New MySqlConnection(ConfigurationManager.ConnectionStrings("constr").ConnectionString)
                If sqlConn.State = ConnectionState.Closed Then
                    Try
                        sqlConn.Open()
                        Using msqlcom As New MySqlCommand("SELECT * FROM rooms ", sqlConn)
                            Using dtReader As MySqlDataReader = msqlcom.ExecuteReader()
                                If dtReader.HasRows = True Then
                                    While dtReader.Read()
                                        Dim dataObj As New room
                                        With dataObj
                                            .room_name = dtReader("room_name").ToString()
                                            .room_description = dtReader("room_description").ToString()
                                            .floorlevel = dtReader("floorlevel").ToString()
                                            .room_id = dtReader("room_id").ToString()
                                            .campus_name = dtReader("campus_name").ToString()
                                        End With
                                        stats.Add(dataObj)
                                    End While
                                    Return Ok(stats)
                                Else
                                    Return NotFound()
                                End If
                                dtReader.Close()
                            End Using
                            msqlcom.Dispose()
                        End Using
                        sqlConn.Close()
                    Catch ex As Exception
                        Return InternalServerError(ex)
                    End Try
                Else
                    Return InternalServerError()
                End If
            End Using
        End Function


        'ADD ROOM

        <Route("add/newroom", Name:="Post_add_room")>
        Public Function Post_add_room(ByVal room_name As String, ByVal room_description As String, ByVal floorlevel As String, ByVal campus_name As String) As HttpResponseMessage
            Dim response As HttpResponseMessage

            If String.IsNullOrEmpty(room_name) Then
                response = Request.CreateResponse(HttpStatusCode.BadRequest)
                response.Content = New StringContent("Error: room_name is required", Encoding.UTF8)
                Return response
            ElseIf String.IsNullOrEmpty(campus_name) Then
                response = Request.CreateResponse(HttpStatusCode.BadRequest)
                response.Content = New StringContent("Error: campus_name is required", Encoding.UTF8)
                Return response
            End If

            Using sqlConn As New MySqlConnection(ConfigurationManager.ConnectionStrings("constr").ConnectionString)
                If sqlConn.State = ConnectionState.Closed Then
                    Try
                        ' Checking Room Existence
                        Using cmdCheckExistence As New MySqlCommand("CheckRoomExistence", sqlConn)
                            cmdCheckExistence.CommandType = CommandType.StoredProcedure
                            cmdCheckExistence.Parameters.AddWithValue("@room_name_param", room_name)
                            cmdCheckExistence.Parameters.AddWithValue("@campus_name_param", campus_name)
                            cmdCheckExistence.Parameters.Add("@roomCount", MySqlDbType.Int32).Direction = ParameterDirection.Output

                            sqlConn.Open()
                            cmdCheckExistence.ExecuteNonQuery()
                            Dim roomCount As Integer = Convert.ToInt32(cmdCheckExistence.Parameters("@roomCount").Value)

                            If roomCount > 0 Then
                                ' Room number already exists, return an error message
                                response = Request.CreateResponse(HttpStatusCode.BadRequest)
                                response.Content = New StringContent("Error: Room Name already exists", Encoding.UTF8)
                                Return response
                            End If
                        End Using

                        ' Adding a New Room
                        Using cmdAddRoom As New MySqlCommand("AddNewRoom", sqlConn)
                            cmdAddRoom.CommandType = CommandType.StoredProcedure
                            cmdAddRoom.Parameters.AddWithValue("@room_name_param", room_name)
                            cmdAddRoom.Parameters.AddWithValue("@room_description_param", room_description)
                            cmdAddRoom.Parameters.AddWithValue("@floorlevel_param", floorlevel)
                            cmdAddRoom.Parameters.AddWithValue("@campus_name_param", campus_name)

                            cmdAddRoom.ExecuteNonQuery()

                            response = Request.CreateResponse(HttpStatusCode.OK)
                            response.Content = New StringContent("Successfully Created", Encoding.UTF8)
                            Return response
                        End Using
                    Catch ex As Exception
                        response = Request.CreateResponse(HttpStatusCode.InternalServerError)
                        response.Content = New StringContent("Application Error: There is an error in performing this action, " & ex.ToString(), Encoding.UTF8)
                        Return response
                    Finally
                        sqlConn.Close()
                    End Try
                Else
                    response = Request.CreateResponse(HttpStatusCode.InternalServerError)
                    response.Content = New StringContent("Server Error: Unable to connect to the database server", Encoding.UTF8)
                    Return response
                End If
            End Using
        End Function


        'UPDATE ROOMS

        <Route("update/room", Name:="Post_update_rooms")>
        Public Function Post_update_rooms(ByVal room_id As String, ByVal room_name As String, ByVal floorlevel As String, ByVal room_description As String) As HttpResponseMessage
            Dim response As HttpResponseMessage

            ' Check if room_name is empty or contains only spaces
            If String.IsNullOrWhiteSpace(room_name) Then
                response = Request.CreateResponse(HttpStatusCode.BadRequest)
                response.Content = New StringContent("Error: Room name is required.", Encoding.UTF8)
                Return response
            End If

            Using sqlConn As New MySqlConnection(ConfigurationManager.ConnectionStrings("constr").ConnectionString)
                If sqlConn.State = ConnectionState.Closed Then
                    Try
                        ' Check if the room_name already exists
                        Using checkCmd As New MySqlCommand("CheckRoomNameExistence", sqlConn)
                            checkCmd.CommandType = CommandType.StoredProcedure
                            checkCmd.Parameters.AddWithValue("@room_id_param", room_id)
                            checkCmd.Parameters.AddWithValue("@room_name_param", room_name)
                            checkCmd.Parameters.Add("@roomCount", MySqlDbType.Int32).Direction = ParameterDirection.Output

                            sqlConn.Open()
                            checkCmd.ExecuteNonQuery()
                            Dim count As Integer = Convert.ToInt32(checkCmd.Parameters("@roomCount").Value)

                            If count > 0 Then
                                ' The room_name already exists, return an error response
                                response = Request.CreateResponse(HttpStatusCode.BadRequest)
                                response.Content = New StringContent("Error: The room name already exists.", Encoding.UTF8)
                                Return response
                            End If
                        End Using

                        ' Updating a Room
                        Using updateCmd As New MySqlCommand("UpdateRoom", sqlConn)
                            updateCmd.CommandType = CommandType.StoredProcedure
                            updateCmd.Parameters.AddWithValue("@room_id_param", room_id)
                            updateCmd.Parameters.AddWithValue("@room_name_param", room_name)
                            updateCmd.Parameters.AddWithValue("@floorlevel_param", floorlevel)
                            updateCmd.Parameters.AddWithValue("@room_description_param", room_description)

                            updateCmd.ExecuteNonQuery()

                            response = Request.CreateResponse(HttpStatusCode.OK)
                            response.Content = New StringContent("Successfully Updated", Encoding.UTF8)
                            Return response
                        End Using
                    Catch ex As Exception
                        response = Request.CreateResponse(HttpStatusCode.InternalServerError)
                        response.Content = New StringContent("Application Error: There is an error in performing this action, " & ex.ToString(), Encoding.UTF8)
                        Return response
                    Finally
                        sqlConn.Close()
                    End Try
                Else
                    response = Request.CreateResponse(HttpStatusCode.InternalServerError)
                    response.Content = New StringContent("Server Error: Unable to connect to the database server", Encoding.UTF8)
                    Return response
                End If
            End Using
        End Function


        'SHOW ALL IN HISTORY

        <Route("history/list", Name:="Get_history_list")>
        Public Function Get_history_list(ByVal campus_name As String) As IHttpActionResult
            Dim stats As List(Of history) = New List(Of history)
            Using sqlConn As New MySqlConnection(ConfigurationManager.ConnectionStrings("constr").ConnectionString)
                If sqlConn.State = ConnectionState.Closed Then
                    Try
                        sqlConn.Open()
                        Using msqlcom As New MySqlCommand("SELECT * FROM history
                                                            WHERE campus_name = '" & campus_name & "'", sqlConn)
                            Using dtReader As MySqlDataReader = msqlcom.ExecuteReader()
                                If dtReader.HasRows = True Then
                                    While dtReader.Read()
                                        Dim dataObj As New history
                                        With dataObj
                                            .item_name = dtReader("item_name").ToString()
                                            .serial_number = dtReader("serial_number").ToString()
                                            .date_transfer = dtReader("date_transfer").ToString()
                                            .last_location = dtReader("last_location").ToString()
                                            .current_location = dtReader("current_location").ToString()
                                            .transfer_by = dtReader("transfer_by").ToString()
                                            .requested_by = dtReader("requested_by").ToString()
                                            .campus_name = dtReader("campus_name").ToString()

                                        End With
                                        stats.Add(dataObj)
                                    End While
                                    Return Ok(stats)
                                Else
                                    Return NotFound()
                                End If
                                dtReader.Close()
                            End Using
                            msqlcom.Dispose()
                        End Using
                        sqlConn.Close()
                    Catch ex As Exception
                        Return InternalServerError(ex)
                    End Try
                Else
                    Return InternalServerError()
                End If
            End Using
        End Function



        'SHOW ALL ITEMS INSIDE ROOM

        <Route("inside/room", Name:="Get_inside_room")>
        Public Function Get_inside_room(ByVal room_name As String, ByVal campus_name As String) As IHttpActionResult
            Dim stats As New List(Of items)()


            Dim query As String = "SELECT * FROM itemlist WHERE room_name = @room_name AND campus_name = @campus_name"

            Using sqlConn As New MySqlConnection(ConfigurationManager.ConnectionStrings("constr").ConnectionString)
                If sqlConn.State = ConnectionState.Closed Then
                    Try
                        sqlConn.Open()
                        Using msqlcom As New MySqlCommand(query, sqlConn)
                            msqlcom.Parameters.AddWithValue("@room_name", room_name)
                            msqlcom.Parameters.AddWithValue("@campus_name", campus_name)

                            Using dtReader As MySqlDataReader = msqlcom.ExecuteReader()
                                If dtReader.HasRows Then
                                    While dtReader.Read()
                                        Dim dataObj As New items()
                                        With dataObj
                                            .item_name = dtReader("item_name").ToString()
                                            .model = dtReader("model").ToString()
                                            .brand = dtReader("brand").ToString()
                                            .serial_number = dtReader("serial_number").ToString()
                                            .item_type = dtReader("item_type").ToString()
                                            .room_name = dtReader("room_name").ToString()
                                            .status = dtReader("status").ToString()
                                            .department = dtReader("department").ToString()
                                            .date_now = dtReader("date_now").ToString()
                                            .campus_name = dtReader("campus_name").ToString()
                                        End With
                                        stats.Add(dataObj)
                                    End While
                                    Return Ok(stats)
                                Else
                                    Return NotFound()
                                End If
                            End Using
                        End Using
                    Catch ex As Exception
                        Return InternalServerError(ex)
                    End Try
                Else
                    Return InternalServerError()
                End If
            End Using
        End Function





        'transfer items from inside room

        <Route("transfer/items", Name:="Post_transfer_items")>
        Public Function Post_transfer_items(ByVal serial_number As String, ByVal room_name As String, ByVal transfer_by As String, ByVal requested_by As String, ByVal campus_name As String) As HttpResponseMessage
            Dim response As HttpResponseMessage

            Try
                ' Input Validation
                If String.IsNullOrEmpty(serial_number) OrElse String.IsNullOrEmpty(room_name) Then
                    response = Request.CreateResponse(HttpStatusCode.BadRequest)
                    response.Content = New StringContent("Invalid input parameters", Encoding.UTF8)
                    Return response
                End If

                Using sqlConn As New MySqlConnection(ConfigurationManager.ConnectionStrings("constr").ConnectionString)
                    sqlConn.Open()

                    ' Check if the room_name is the same as the current room_name
                    Using cmdCheckRoom As New MySqlCommand()
                        cmdCheckRoom.Connection = sqlConn
                        cmdCheckRoom.CommandText = "SELECT room_name FROM itemlist WHERE serial_number = @serial_number"
                        cmdCheckRoom.Parameters.AddWithValue("@serial_number", serial_number)

                        Dim currentRoom As String = cmdCheckRoom.ExecuteScalar()?.ToString()

                        If currentRoom = room_name Then
                            response = Request.CreateResponse(HttpStatusCode.BadRequest)
                            response.Content = New StringContent("Item is already in that room", Encoding.UTF8)
                            Return response
                        End If
                    End Using

                    ' Get the current date (today)
                    Dim currentDate As DateTime = DateTime.Now

                    Using cmdGethistory As New MySqlCommand()
                        cmdGethistory.Connection = sqlConn
                        cmdGethistory.CommandText = "INSERT INTO history (item_name,serial_number,last_location,current_location,date_transfer,transfer_by,requested_by,campus_name)
                                    SELECT item_name,serial_number,room_name,@room_name,@date_transfer,@transfer_by,@requested_by,@campus_name FROM itemlist
                                    WHERE serial_number = @serial_number"
                        cmdGethistory.Parameters.AddWithValue("@serial_number", serial_number)
                        cmdGethistory.Parameters.AddWithValue("@room_name", room_name)
                        cmdGethistory.Parameters.AddWithValue("@date_transfer", currentDate)
                        cmdGethistory.Parameters.AddWithValue("@transfer_by", transfer_by)
                        cmdGethistory.Parameters.AddWithValue("@requested_by", requested_by) ' Set the date_transfer value to the current date
                        cmdGethistory.Parameters.AddWithValue("@campus_name", campus_name)
                        cmdGethistory.ExecuteNonQuery()
                    End Using

                    Using cmdTransfer As New MySqlCommand()
                        cmdTransfer.Connection = sqlConn
                        cmdTransfer.CommandText = "UPDATE itemlist SET serial_number = @serial_number, 
                    room_name = @room_name
                    WHERE serial_number = @serial_number"

                        cmdTransfer.Parameters.AddWithValue("@serial_number", serial_number)
                        cmdTransfer.Parameters.AddWithValue("@room_name", room_name)
                        cmdTransfer.ExecuteNonQuery()

                        ' Construct the endpoint based on the current room_no value
                        Dim endpoint As String = "/api/rooms/" & room_name ' Modify this to match your endpoint structure

                        response = Request.CreateResponse(HttpStatusCode.OK)
                        response.Content = New StringContent("Successfully Updated", Encoding.UTF8)
                        ' You can also include the endpoint in the response content if needed.
                        ' response.Content = New StringContent("Successfully Updated. Endpoint: " & endpoint, Encoding.UTF8)
                    End Using
                End Using

                Return response
            Catch ex As Exception
                ' Log the exception
                ' Logging code here...

                response = Request.CreateResponse(HttpStatusCode.InternalServerError)
                response.Content = New StringContent("Application Error: There is an error in performing this action, " & ex.ToString(), Encoding.UTF8)
                Return response
            End Try
        End Function



        'HISTORY
        <Route("history/items", Name:="Get_history_items")>
        Public Function Get_history_items() As IHttpActionResult
            Dim stats As List(Of history) = New List(Of history)
            Using sqlConn As New MySqlConnection(ConfigurationManager.ConnectionStrings("constr").ConnectionString)
                If sqlConn.State = ConnectionState.Closed Then
                    Try
                        sqlConn.Open()
                        Using msqlcom As New MySqlCommand("SELECT * FROM history", sqlConn)
                            Using dtReader As MySqlDataReader = msqlcom.ExecuteReader()
                                If dtReader.HasRows = True Then
                                    While dtReader.Read()
                                        Dim dataObj As New history
                                        With dataObj
                                            .item_name = dtReader("item_name").ToString()
                                            .serial_number = dtReader("serial_number").ToString()
                                            .date_transfer = dtReader("date_transfer").ToString()
                                            .last_location = dtReader("last_location").ToString()
                                            .current_location = dtReader("current_location").ToString()
                                            .transfer_by = dtReader("transfer_by").ToString()
                                            .requested_by = dtReader("requested_by").ToString()

                                        End With
                                        stats.Add(dataObj)
                                    End While
                                    Return Ok(stats)
                                Else
                                    Return NotFound()
                                End If
                                dtReader.Close()
                            End Using
                            msqlcom.Dispose()
                        End Using
                        sqlConn.Close()
                    Catch ex As Exception
                        Return InternalServerError(ex)
                    End Try
                Else
                    Return InternalServerError()
                End If
            End Using
        End Function


        'SHOW ALL UNIFORM STOCK

        <Route("uniform/stock", Name:="Get_uniform_stock")>
        Public Function Get_uniform_stock(ByVal campus_name As String) As IHttpActionResult
            Dim stats As List(Of uniform_stock) = New List(Of uniform_stock)
            Using sqlConn As New MySqlConnection(ConfigurationManager.ConnectionStrings("constr").ConnectionString)
                If sqlConn.State = ConnectionState.Closed Then
                    Try
                        sqlConn.Open()
                        Using msqlcom As New MySqlCommand("SELECT * FROM uniform_stock
                                                            WHERE campus_name = '" & campus_name & "'", sqlConn)
                            Using dtReader As MySqlDataReader = msqlcom.ExecuteReader()
                                If dtReader.HasRows = True Then
                                    While dtReader.Read()
                                        Dim dataObj As New uniform_stock
                                        With dataObj
                                            .grade_level = dtReader("grade_level").ToString()
                                            .clothes_type = dtReader("clothes_type").ToString()
                                            .size = dtReader("size").ToString()
                                            .quantity = dtReader("quantity").ToString()
                                            .uniform_id = dtReader("uniform_id").ToString()
                                            .campus_name = dtReader("campus_name").ToString()

                                        End With
                                        stats.Add(dataObj)
                                    End While
                                    Return Ok(stats)
                                Else
                                    Return NotFound()
                                End If
                                dtReader.Close()
                            End Using
                            msqlcom.Dispose()
                        End Using
                        sqlConn.Close()
                    Catch ex As Exception
                        Return InternalServerError(ex)
                    End Try
                Else
                    Return InternalServerError()
                End If
            End Using
        End Function


        'ADD UNIFORM

        <Route("add/uniform", Name:="Post_add_uniform")>
        Public Function Post_add_uniform(ByVal grade_level As String, ByVal clothes_type As String, ByVal size As String, ByVal quantity As String, ByVal campus_name As String) As HttpResponseMessage
            Dim response As HttpResponseMessage

            Using sqlConn As New MySqlConnection(ConfigurationManager.ConnectionStrings("constr").ConnectionString)
                If sqlConn.State = ConnectionState.Closed Then
                    Try
                        ' Check if the record already exists
                        Using cmd As New MySqlCommand()
                            cmd.Connection = sqlConn
                            cmd.CommandText = "SELECT COUNT(*) FROM uniform_stock WHERE grade_level = @grade_level AND clothes_type = @clothes_type AND size = @size"

                            cmd.Parameters.AddWithValue("@grade_level", grade_level)
                            cmd.Parameters.AddWithValue("@clothes_type", clothes_type)
                            cmd.Parameters.AddWithValue("@size", size)

                            sqlConn.Open()
                            Dim count As Integer = Convert.ToInt32(cmd.ExecuteScalar())

                            If count > 0 Then
                                ' The record already exists, return an error response
                                response = Request.CreateResponse(HttpStatusCode.BadRequest)
                                response.Content = New StringContent("Error: The apparel you entered already exists.", Encoding.UTF8)
                                Return response
                            End If
                        End Using

                        ' Insert data into the uniform_stock table
                        Using cmd As New MySqlCommand()
                            cmd.Connection = sqlConn
                            cmd.CommandText = "INSERT INTO uniform_stock(grade_level, clothes_type, size, quantity, campus_name) VALUES (@grade_level, @clothes_type, @size, @quantity, @campus_name)"

                            cmd.Parameters.AddWithValue("@grade_level", grade_level)
                            cmd.Parameters.AddWithValue("@clothes_type", clothes_type)
                            cmd.Parameters.AddWithValue("@size", size)
                            cmd.Parameters.AddWithValue("@quantity", quantity)
                            cmd.Parameters.AddWithValue("@campus_name", campus_name)

                            cmd.ExecuteNonQuery()
                        End Using

                        ' Insert data into the clothes_log table
                        Using cmd As New MySqlCommand()
                            cmd.Connection = sqlConn
                            cmd.CommandText = "INSERT INTO clothes_log(grade_level, clothes_type, size, transaction, transaction_date, quantity, campus_name) VALUES (@grade_level, @clothes_type, @size, @transaction, @transaction_date, @quantity, @campus_name )"

                            cmd.Parameters.AddWithValue("@grade_level", grade_level)
                            cmd.Parameters.AddWithValue("@clothes_type", clothes_type)
                            cmd.Parameters.AddWithValue("@size", size)
                            cmd.Parameters.AddWithValue("@transaction", "ADD RECORDS")
                            cmd.Parameters.AddWithValue("@quantity", quantity)
                            cmd.Parameters.AddWithValue("@campus_name", campus_name)
                            cmd.Parameters.AddWithValue("@transaction_date", DateTime.Now) ' Use the current date

                            cmd.ExecuteNonQuery()
                        End Using

                        response = Request.CreateResponse(HttpStatusCode.OK)
                        response.Content = New StringContent("Successfully Created", Encoding.UTF8)
                        Return response
                    Catch ex As Exception
                        response = Request.CreateResponse(HttpStatusCode.InternalServerError)
                        response.Content = New StringContent("Application Error: There is an error in performing this action, " & ex.ToString(), Encoding.UTF8)
                        Return response
                    Finally
                        sqlConn.Close()
                    End Try
                Else
                    response = Request.CreateResponse(HttpStatusCode.InternalServerError)
                    response.Content = New StringContent("Server Error: Unable to connect to the database server", Encoding.UTF8)
                    Return response
                End If
            End Using
        End Function




        'ADD quantity

        <Route("add/stock", Name:="Post_update_stock_in")>
        Public Function Post_update_stock_in(ByVal uniform_id As Integer, ByVal stockChange As Integer) As HttpResponseMessage
            Dim response As HttpResponseMessage

            Using sqlConn As New MySqlConnection(ConfigurationManager.ConnectionStrings("constr").ConnectionString)
                If sqlConn.State = ConnectionState.Closed Then
                    Try
                        sqlConn.Open()

                        Using cmd As New MySqlCommand()
                            cmd.Connection = sqlConn
                            cmd.CommandText = "UPDATE uniform_stock SET quantity = quantity + @stockChange WHERE uniform_id = @uniform_id"

                            cmd.Parameters.AddWithValue("@uniform_id", uniform_id)
                            cmd.Parameters.AddWithValue("@stockChange", stockChange)

                            Dim rowsAffected As Integer = cmd.ExecuteNonQuery()

                            If rowsAffected > 0 Then
                                ' Insert a record into the clothes_log table
                                Using insertCmd As New MySqlCommand()
                                    insertCmd.Connection = sqlConn
                                    insertCmd.CommandText = "INSERT INTO clothes_log(grade_level, clothes_type, size, transaction, transaction_date, quantity, campus_name) " &
                                            "SELECT grade_level, clothes_type, size, 'ADD Quantity', @transaction_date, @quantity, campus_name " &
                                            "FROM uniform_stock WHERE uniform_id = @uniform_id"

                                    insertCmd.Parameters.AddWithValue("@uniform_id", uniform_id)
                                    insertCmd.Parameters.AddWithValue("@quantity", stockChange)
                                    insertCmd.Parameters.AddWithValue("@transaction_date", DateTime.Now) ' Use the current date

                                    Dim insertRowsAffected As Integer = insertCmd.ExecuteNonQuery()

                                    If insertRowsAffected > 0 Then
                                        response = Request.CreateResponse(HttpStatusCode.OK)
                                        response.Content = New StringContent("Stock updated successfully (IN)", Encoding.UTF8)
                                    Else
                                        ' Failed to insert into clothes_log
                                        response = Request.CreateResponse(HttpStatusCode.BadRequest)
                                        response.Content = New StringContent("Failed to insert into clothes_log", Encoding.UTF8)
                                    End If
                                End Using
                            Else
                                response = Request.CreateResponse(HttpStatusCode.BadRequest)
                                response.Content = New StringContent("Failed to update stock (IN)", Encoding.UTF8)
                            End If
                        End Using
                    Catch ex As Exception
                        response = Request.CreateResponse(HttpStatusCode.InternalServerError)
                        response.Content = New StringContent("Application Error (IN): There is an error in performing this action, " & ex.ToString(), Encoding.UTF8)
                    Finally
                        sqlConn.Close()
                    End Try
                Else
                    response = Request.CreateResponse(HttpStatusCode.InternalServerError)
                    response.Content = New StringContent("Server Error: Unable to connect to the database server", Encoding.UTF8)
                End If
            End Using

            Return response
        End Function





        'SHOW ALL GRADES
        <Route("grade/list", Name:="Get_grade_list")>
        Public Function Get_grade_list(ByVal campus_name As String) As IHttpActionResult
            Dim stats As List(Of grade) = New List(Of grade)
            Using sqlConn As New MySqlConnection(ConfigurationManager.ConnectionStrings("constr").ConnectionString)
                If sqlConn.State = ConnectionState.Closed Then
                    Try
                        sqlConn.Open()
                        Using msqlcom As New MySqlCommand("SELECT * FROM grade
                                                            WHERE campus_name = '" & campus_name & "'", sqlConn)
                            Using dtReader As MySqlDataReader = msqlcom.ExecuteReader()
                                If dtReader.HasRows = True Then
                                    While dtReader.Read()
                                        Dim dataObj As New grade
                                        With dataObj
                                            .grade_level = dtReader("grade_level").ToString()
                                            .campus_name = dtReader("campus_name").ToString()
                                        End With
                                        stats.Add(dataObj)
                                    End While
                                    Return Ok(stats)
                                Else
                                    Return NotFound()
                                End If
                                dtReader.Close()
                            End Using
                            msqlcom.Dispose()
                        End Using
                        sqlConn.Close()
                    Catch ex As Exception
                        Return InternalServerError(ex)
                    End Try
                Else
                    Return InternalServerError()
                End If
            End Using
        End Function


        'ADD GRADE LEVEL

        <Route("add/gradelevel", Name:="Post_add_gradelevel")>
        Public Function Post_add_gradelevel(ByVal grade_level As String, ByVal campus_name As String) As HttpResponseMessage
            Dim response As HttpResponseMessage

            Using sqlConn As New MySqlConnection(ConfigurationManager.ConnectionStrings("constr").ConnectionString)
                If sqlConn.State = ConnectionState.Closed Then
                    Try
                        sqlConn.Open()
                        ' Check if a record with the same grade_level and campus_name already exists
                        Using cmdCheck As New MySqlCommand()
                            cmdCheck.Connection = sqlConn
                            cmdCheck.CommandText = "SELECT COUNT(*) FROM grade WHERE grade_level = @grade_level AND campus_name = @campus_name"
                            cmdCheck.Parameters.AddWithValue("@grade_level", grade_level)
                            cmdCheck.Parameters.AddWithValue("@campus_name", campus_name)
                            Dim count As Integer = Convert.ToInt32(cmdCheck.ExecuteScalar())

                            If count > 0 Then
                                ' A record with the same grade_level and campus_name already exists, return an error message
                                response = Request.CreateResponse(HttpStatusCode.BadRequest)
                                response.Content = New StringContent("Error: Grade already exists", Encoding.UTF8)
                                Return response
                            End If
                        End Using

                        ' If no record with the same grade_level and campus_name exists, proceed with the insertion
                        Using cmdInsert As New MySqlCommand()
                            cmdInsert.Connection = sqlConn
                            cmdInsert.CommandText = "INSERT INTO grade(grade_level, campus_name) VALUES (@grade_level, @campus_name)"
                            cmdInsert.Parameters.AddWithValue("@grade_level", grade_level)
                            cmdInsert.Parameters.AddWithValue("@campus_name", campus_name)
                            cmdInsert.ExecuteNonQuery()

                            response = Request.CreateResponse(HttpStatusCode.OK)
                            response.Content = New StringContent("Added Successfully", Encoding.UTF8)
                            Return response
                        End Using
                    Catch ex As Exception
                        response = Request.CreateResponse(HttpStatusCode.InternalServerError)
                        response.Content = New StringContent("Application Error: There is an error in performing this action, " & ex.ToString(), Encoding.UTF8)
                        Return response
                    Finally
                        sqlConn.Close()
                    End Try
                Else
                    response = Request.CreateResponse(HttpStatusCode.InternalServerError)
                    response.Content = New StringContent("Server Error: Unable to connect to the database server", Encoding.UTF8)
                    Return response
                End If
            End Using
        End Function




        'SHOW ALL Size

        <Route("size/list", Name:="Get_size_list")>
        Public Function Get_size_list(ByVal campus_name As String) As IHttpActionResult
            Dim stats As List(Of size_list) = New List(Of size_list)
            Using sqlConn As New MySqlConnection(ConfigurationManager.ConnectionStrings("constr").ConnectionString)
                If sqlConn.State = ConnectionState.Closed Then
                    Try
                        sqlConn.Open()
                        Using msqlcom As New MySqlCommand("SELECT * FROM size_list
                                                            WHERE campus_name = '" & campus_name & "'", sqlConn)
                            Using dtReader As MySqlDataReader = msqlcom.ExecuteReader()
                                If dtReader.HasRows = True Then
                                    While dtReader.Read()
                                        Dim dataObj As New size_list
                                        With dataObj
                                            .size = dtReader("size").ToString()
                                            .campus_name = dtReader("campus_name").ToString()
                                        End With
                                        stats.Add(dataObj)
                                    End While
                                    Return Ok(stats)
                                Else
                                    Return NotFound()
                                End If
                                dtReader.Close()
                            End Using
                            msqlcom.Dispose()
                        End Using
                        sqlConn.Close()
                    Catch ex As Exception
                        Return InternalServerError(ex)
                    End Try
                Else
                    Return InternalServerError()
                End If
            End Using
        End Function



        'ADD SIZES
        <Route("add/sizes", Name:="Post_add_sizes")>
        Public Function Post_add_sizes(ByVal size As String, ByVal campus_name As String) As HttpResponseMessage
            Dim response As HttpResponseMessage

            Using sqlConn As New MySqlConnection(ConfigurationManager.ConnectionStrings("constr").ConnectionString)
                If sqlConn.State = ConnectionState.Closed Then
                    Try
                        sqlConn.Open()

                        ' Check if the size and campus_name already exist
                        Using cmdCheck As New MySqlCommand()
                            cmdCheck.Connection = sqlConn
                            cmdCheck.CommandText = "SELECT COUNT(*) FROM size_list WHERE size = @size AND campus_name = @campus_name"
                            cmdCheck.Parameters.AddWithValue("@size", size)
                            cmdCheck.Parameters.AddWithValue("@campus_name", campus_name)
                            Dim count As Integer = CInt(cmdCheck.ExecuteScalar())

                            If count > 0 Then
                                ' The size and campus_name already exist, return an error response
                                response = Request.CreateResponse(HttpStatusCode.BadRequest)
                                response.Content = New StringContent("Size already exists", Encoding.UTF8)
                            Else
                                ' The size and campus_name do not exist, proceed with the insertion
                                Using cmdInsert As New MySqlCommand()
                                    cmdInsert.Connection = sqlConn
                                    cmdInsert.CommandText = "INSERT INTO size_list(size, campus_name) VALUES (@size, @campus_name)"
                                    cmdInsert.Parameters.AddWithValue("@size", size)
                                    cmdInsert.Parameters.AddWithValue("@campus_name", campus_name)
                                    cmdInsert.ExecuteNonQuery()

                                    response = Request.CreateResponse(HttpStatusCode.OK)
                                    response.Content = New StringContent("Added Successfully", Encoding.UTF8)
                                End Using
                            End If
                        End Using
                    Catch ex As Exception
                        response = Request.CreateResponse(HttpStatusCode.InternalServerError)
                        response.Content = New StringContent("Application Error: There is an error in performing this action, " & ex.ToString(), Encoding.UTF8)
                    Finally
                        sqlConn.Close()
                    End Try
                Else
                    response = Request.CreateResponse(HttpStatusCode.InternalServerError)
                    response.Content = New StringContent("Server Error: Unable to connect to the database server", Encoding.UTF8)
                End If
            End Using

            Return response
        End Function



        'SHOW ALL CLOTHES

        <Route("clothes/list", Name:="Get_clothes_list")>
        Public Function Get_clothes_list(ByVal campus_name As String) As IHttpActionResult
            Dim stats As List(Of clothes_list) = New List(Of clothes_list)
            Using sqlConn As New MySqlConnection(ConfigurationManager.ConnectionStrings("constr").ConnectionString)
                If sqlConn.State = ConnectionState.Closed Then
                    Try
                        sqlConn.Open()
                        Using msqlcom As New MySqlCommand("SELECT * FROM clothes_list
                                                            WHERE campus_name = '" & campus_name & "'", sqlConn)
                            Using dtReader As MySqlDataReader = msqlcom.ExecuteReader()
                                If dtReader.HasRows = True Then
                                    While dtReader.Read()
                                        Dim dataObj As New clothes_list
                                        With dataObj
                                            .clothes_type = dtReader("clothes_type").ToString()
                                            .campus_name = dtReader("campus_name").ToString()
                                        End With
                                        stats.Add(dataObj)
                                    End While
                                    Return Ok(stats)
                                Else
                                    Return NotFound()
                                End If
                                dtReader.Close()
                            End Using
                            msqlcom.Dispose()
                        End Using
                        sqlConn.Close()
                    Catch ex As Exception
                        Return InternalServerError(ex)
                    End Try
                Else
                    Return InternalServerError()
                End If
            End Using
        End Function

        'ADD CLOTHES TYPE

        <Route("add/clothestype", Name:="Post_add_Clothes_type")>
        Public Function Post_add_Clothes_type(ByVal clothes_type As String, ByVal campus_name As String) As HttpResponseMessage
            Dim response As HttpResponseMessage

            Using sqlConn As New MySqlConnection(ConfigurationManager.ConnectionStrings("constr").ConnectionString)
                If sqlConn.State = ConnectionState.Closed Then
                    Try
                        sqlConn.Open()

                        ' Check if the record already exists
                        Using cmdCheck As New MySqlCommand()
                            cmdCheck.Connection = sqlConn
                            cmdCheck.CommandText = "SELECT COUNT(*) FROM clothes_list WHERE clothes_type = @clothes_type AND campus_name = @campus_name"
                            cmdCheck.Parameters.AddWithValue("@clothes_type", clothes_type)
                            cmdCheck.Parameters.AddWithValue("@campus_name", campus_name)

                            Dim recordCount As Integer = CInt(cmdCheck.ExecuteScalar())

                            If recordCount > 0 Then
                                ' Record already exists, return an error response
                                response = Request.CreateResponse(HttpStatusCode.BadRequest)
                                response.Content = New StringContent("Apparel already exists")
                                Return response
                            End If
                        End Using

                        ' If the record doesn't exist, proceed with the insertion
                        Using cmdInsert As New MySqlCommand()
                            cmdInsert.Connection = sqlConn
                            cmdInsert.CommandText = "INSERT INTO clothes_list(clothes_type, campus_name) VALUES (@clothes_type, @campus_name)"
                            cmdInsert.Parameters.AddWithValue("@clothes_type", clothes_type)
                            cmdInsert.Parameters.AddWithValue("@campus_name", campus_name)
                            cmdInsert.ExecuteNonQuery()

                            response = Request.CreateResponse(HttpStatusCode.OK)
                            response.Content = New StringContent("Added Successfully", Encoding.UTF8)
                            Return response
                        End Using
                    Catch ex As Exception
                        response = Request.CreateResponse(HttpStatusCode.InternalServerError)
                        response.Content = New StringContent("Application Error: There is an error in performing this action, " & ex.ToString(), Encoding.UTF8)
                        Return response
                    Finally
                        sqlConn.Close()
                    End Try
                Else
                    response = Request.CreateResponse(HttpStatusCode.InternalServerError)
                    response.Content = New StringContent("Server Error: Unable to connect to the database server", Encoding.UTF8)
                    Return response
                End If
            End Using
        End Function



        'SHOW ALL CLOTHES LOG 

        <Route("clothes/log", Name:="Get_clothes_log")>
        Public Function Get_clothes_log(ByVal campus_name As String) As IHttpActionResult
            Dim stats As List(Of clothes_log) = New List(Of clothes_log)
            Using sqlConn As New MySqlConnection(ConfigurationManager.ConnectionStrings("constr").ConnectionString)
                If sqlConn.State = ConnectionState.Closed Then
                    Try
                        sqlConn.Open()
                        Using msqlcom As New MySqlCommand("SELECT * FROM clothes_log
                                                            WHERE campus_name = '" & campus_name & "'", sqlConn)
                            Using dtReader As MySqlDataReader = msqlcom.ExecuteReader()
                                If dtReader.HasRows = True Then
                                    While dtReader.Read()
                                        Dim dataObj As New clothes_log
                                        With dataObj
                                            .grade_level = dtReader("grade_level").ToString()
                                            .size = dtReader("size").ToString()
                                            .transaction = dtReader("transaction").ToString()
                                            .transaction_date = dtReader("transaction_date").ToString()
                                            .quantity = dtReader("quantity").ToString()
                                            .first_name = dtReader("first_name").ToString()
                                            .last_name = dtReader("last_name").ToString()
                                            .campus_name = dtReader("campus_name").ToString()
                                            .clothes_type = dtReader("clothes_type").ToString()
                                            .id = dtReader("id").ToString()
                                        End With
                                        stats.Add(dataObj)
                                    End While
                                    Return Ok(stats)
                                Else
                                    Return NotFound()
                                End If
                                dtReader.Close()
                            End Using
                            msqlcom.Dispose()
                        End Using
                        sqlConn.Close()
                    Catch ex As Exception
                        Return InternalServerError(ex)
                    End Try
                Else
                    Return InternalServerError()
                End If
            End Using
        End Function


        'CAMPUS LIST

        <Route("campus/list", Name:="Get_campus_list")>
        Public Function Get_campus_list() As IHttpActionResult
            Dim stats As List(Of campus) = New List(Of campus)
            Using sqlConn As New MySqlConnection(ConfigurationManager.ConnectionStrings("constr").ConnectionString)
                If sqlConn.State = ConnectionState.Closed Then
                    Try
                        sqlConn.Open()
                        Using msqlcom As New MySqlCommand("SELECT * FROM campus", sqlConn)
                            Using dtReader As MySqlDataReader = msqlcom.ExecuteReader()
                                If dtReader.HasRows = True Then
                                    While dtReader.Read()
                                        Dim dataObj As New campus
                                        With dataObj
                                            .campus_name = dtReader("campus_name").ToString()
                                        End With
                                        stats.Add(dataObj)
                                    End While
                                    Return Ok(stats)
                                Else
                                    Return NotFound()
                                End If
                                dtReader.Close()
                            End Using
                            msqlcom.Dispose()
                        End Using
                        sqlConn.Close()
                    Catch ex As Exception
                        Return InternalServerError(ex)
                    End Try
                Else
                    Return InternalServerError()
                End If
            End Using
        End Function


        'ADD Campus

        <Route("add/campus", Name:="Post_add_campus")>
        Public Function Post_add_campus(ByVal campus_name As String) As HttpResponseMessage
            Dim response As HttpResponseMessage

            ' Trim and validate the input
            Dim trimmedCampusName As String = campus_name.Trim()

            If String.IsNullOrWhiteSpace(trimmedCampusName) OrElse trimmedCampusName.Length <= 1 OrElse IsNumeric(trimmedCampusName) Then
                ' Invalid input, return a bad request response
                response = Request.CreateResponse(HttpStatusCode.BadRequest)
                response.Content = New StringContent("Invalid campus name.", Encoding.UTF8)
                Return response
            End If

            Try
                Using sqlConn As New MySqlConnection(ConfigurationManager.ConnectionStrings("constr").ConnectionString)
                    sqlConn.Open()

                    ' Check if the campus name already exists
                    Using cmdCheck As New MySqlCommand()
                        cmdCheck.Connection = sqlConn
                        cmdCheck.CommandText = "SELECT COUNT(*) FROM campus WHERE campus_name = @campus_name"
                        cmdCheck.Parameters.AddWithValue("@campus_name", trimmedCampusName)
                        Dim campusCount As Integer = CInt(cmdCheck.ExecuteScalar())

                        If campusCount > 0 Then
                            ' Campus name already exists, return an error response
                            response = Request.CreateResponse(HttpStatusCode.BadRequest)
                            response.Content = New StringContent("Campus name already exists.", Encoding.UTF8)
                        Else
                            ' Campus name doesn't exist, proceed with the insertion
                            Using cmdInsert As New MySqlCommand()
                                cmdInsert.Connection = sqlConn
                                cmdInsert.CommandText = "INSERT INTO campus (campus_name) VALUES (@campus_name)"
                                cmdInsert.Parameters.AddWithValue("@campus_name", trimmedCampusName)
                                Dim rowsAffected = cmdInsert.ExecuteNonQuery()

                                If rowsAffected > 0 Then
                                    response = Request.CreateResponse(HttpStatusCode.OK)
                                    response.Content = New StringContent("Successfully added campus", Encoding.UTF8)
                                Else
                                    response = Request.CreateResponse(HttpStatusCode.InternalServerError)
                                    response.Content = New StringContent("Failed to add campus", Encoding.UTF8)
                                End If
                            End Using
                        End If
                    End Using
                End Using
            Catch ex As MySqlException
                response = Request.CreateResponse(HttpStatusCode.InternalServerError)
                response.Content = New StringContent("Database Error: " & ex.Message, Encoding.UTF8)
            Catch ex As Exception
                response = Request.CreateResponse(HttpStatusCode.InternalServerError)
                response.Content = New StringContent("Application Error: " & ex.Message, Encoding.UTF8)
            End Try

            Return response
        End Function





        'CLAIM STUB

        <Route("claim/list", Name:="Get_claim_list")>
        Public Function Get_claim_list(ByVal campus_name As String) As IHttpActionResult
            Dim stats As List(Of claim_stub) = New List(Of claim_stub)
            Using sqlConn As New MySqlConnection(ConfigurationManager.ConnectionStrings("constr").ConnectionString)
                If sqlConn.State = ConnectionState.Closed Then
                    Try
                        sqlConn.Open()
                        Using msqlcom As New MySqlCommand("SELECT * FROM claim_stub
                                                            WHERE campus_name = '" & campus_name & "'", sqlConn)
                            Using dtReader As MySqlDataReader = msqlcom.ExecuteReader()
                                If dtReader.HasRows = True Then
                                    While dtReader.Read()
                                        Dim dataObj As New claim_stub
                                        With dataObj
                                            .first_name = dtReader("first_name").ToString()
                                            .last_name = dtReader("last_name").ToString()
                                            .apparel_name = dtReader("apparel_name").ToString()
                                            .reciept_number = dtReader("reciept_number").ToString()
                                            .date_recieve = dtReader("date_recieve").ToString()
                                            .claim_id = dtReader("claim_id").ToString()
                                            .campus_name = dtReader("campus_name").ToString()
                                            .grade_level = dtReader("grade_level").ToString()
                                            .quantity = dtReader("quantity").ToString()
                                            .size = dtReader("size").ToString()
                                        End With
                                        stats.Add(dataObj)
                                    End While
                                    Return Ok(stats)
                                Else
                                    Return NotFound()
                                End If
                                dtReader.Close()
                            End Using
                            msqlcom.Dispose()
                        End Using
                        sqlConn.Close()
                    Catch ex As Exception
                        Return InternalServerError(ex)
                    End Try
                Else
                    Return InternalServerError()
                End If
            End Using
        End Function


        'CLAIM PROCESS 

        <Route("add/claimitems", Name:="Post_add_claimitem")>
        Public Function Post_add_claimitem(ByVal first_name As String, ByVal last_name As String, ByVal apparel_name As String, ByVal reciept_number As String, ByVal date_recieve As String, ByVal campus_name As String, ByVal grade_level As String, ByVal quantity As String, ByVal size As String) As HttpResponseMessage
            Dim response As HttpResponseMessage

            Try
                Using sqlConn As New MySqlConnection(ConfigurationManager.ConnectionStrings("constr").ConnectionString)
                    sqlConn.Open()

                    ' Start a transaction
                    Using transaction As MySqlTransaction = sqlConn.BeginTransaction()
                        Try
                            ' Check if there is sufficient stock before inserting the claim item
                            Dim stockCheckCmd As New MySqlCommand()
                            stockCheckCmd.Connection = sqlConn
                            stockCheckCmd.Transaction = transaction
                            stockCheckCmd.CommandText = "SELECT quantity FROM uniform_stock WHERE grade_level = @grade_level AND clothes_type = @apparel_name"
                            stockCheckCmd.Parameters.AddWithValue("@grade_level", grade_level)
                            stockCheckCmd.Parameters.AddWithValue("@apparel_name", apparel_name)
                            Dim stockQuantity As Integer = CInt(stockCheckCmd.ExecuteScalar())

                            Dim claimQuantity As Integer = CInt(quantity)

                            If stockQuantity >= claimQuantity Then
                                ' Sufficient stock, proceed with the insertion into claim_stub
                                Using cmd As New MySqlCommand()
                                    cmd.Connection = sqlConn
                                    cmd.Transaction = transaction
                                    cmd.CommandText = "INSERT INTO claim_stub(first_name, last_name, apparel_name, reciept_number, date_recieve, campus_name, grade_level, quantity, size) VALUES (@first_name, @last_name, @apparel_name, @reciept_number, @date_recieve, @campus_name, @grade_level,  @quantity, @size)"

                                    cmd.Parameters.AddWithValue("@first_name", first_name)
                                    cmd.Parameters.AddWithValue("@last_name", last_name)
                                    cmd.Parameters.AddWithValue("@apparel_name", apparel_name)
                                    cmd.Parameters.AddWithValue("@reciept_number", reciept_number)
                                    cmd.Parameters.AddWithValue("@date_recieve", date_recieve)
                                    cmd.Parameters.AddWithValue("@campus_name", campus_name)
                                    cmd.Parameters.AddWithValue("@grade_level", grade_level)
                                    cmd.Parameters.AddWithValue("@quantity", quantity)
                                    cmd.Parameters.AddWithValue("@size", size)

                                    cmd.ExecuteNonQuery()

                                    ' Insert data into clothes_log table
                                    Dim insertLogCmd As New MySqlCommand()
                                    insertLogCmd.Connection = sqlConn
                                    insertLogCmd.Transaction = transaction
                                    insertLogCmd.CommandText = "INSERT INTO clothes_log(grade_level, size, quantity, clothes_type, transaction_date, transaction, first_name, last_name, campus_name) VALUES (@grade_level, @size, @quantity, @clothes_type, @transaction_date, @transaction, @first_name, @last_name, @campus_name)"

                                    insertLogCmd.Parameters.AddWithValue("@grade_level", grade_level)
                                    insertLogCmd.Parameters.AddWithValue("@size", size)
                                    insertLogCmd.Parameters.AddWithValue("@quantity", quantity)
                                    insertLogCmd.Parameters.AddWithValue("@clothes_type", apparel_name)
                                    insertLogCmd.Parameters.AddWithValue("@transaction_date", DateTime.Now)
                                    insertLogCmd.Parameters.AddWithValue("@transaction", "Claim")
                                    insertLogCmd.Parameters.AddWithValue("@first_name", first_name)
                                    insertLogCmd.Parameters.AddWithValue("@last_name", last_name)
                                    insertLogCmd.Parameters.AddWithValue("@campus_name", campus_name)

                                    insertLogCmd.ExecuteNonQuery()

                                    ' Update uniform stock quantity
                                    Dim updateStockCmd As New MySqlCommand()
                                    updateStockCmd.Connection = sqlConn
                                    updateStockCmd.Transaction = transaction
                                    updateStockCmd.CommandText = "UPDATE uniform_stock SET quantity = quantity - @claimQuantity WHERE grade_level = @grade_level AND clothes_type = @apparel_name"
                                    updateStockCmd.Parameters.AddWithValue("@grade_level", grade_level)
                                    updateStockCmd.Parameters.AddWithValue("@apparel_name", apparel_name)
                                    updateStockCmd.Parameters.AddWithValue("@claimQuantity", claimQuantity)
                                    updateStockCmd.ExecuteNonQuery()

                                    ' Commit the transaction
                                    transaction.Commit()

                                    response = New HttpResponseMessage(HttpStatusCode.OK)
                                    response.Content = New StringContent("Successfully Created", Encoding.UTF8)
                                    Return response
                                End Using
                            Else
                                ' Insufficient stock, return an error response
                                response = New HttpResponseMessage(HttpStatusCode.BadRequest)
                                response.Content = New StringContent("Insufficient Stock", Encoding.UTF8)
                                Return response
                            End If
                        Catch
                            ' An error occurred, roll back the transaction
                            transaction.Rollback()
                            Throw
                        End Try
                    End Using
                End Using

            Catch ex As MySqlException
                response = Request.CreateResponse(HttpStatusCode.InternalServerError)
                response.Content = New StringContent("Database Error: " & ex.Message, Encoding.UTF8)
            Catch ex As Exception
                response = Request.CreateResponse(HttpStatusCode.InternalServerError)
                response.Content = New StringContent("Application Error: " & ex.Message, Encoding.UTF8)
            End Try

            Return response
        End Function


        'Create Account

        <Route("create/account", Name:="Post_create_account")>
        Public Function Post_add_campus(ByVal employee_name As String, ByVal employee_last_name As String, ByVal campus_name As String, ByVal employee_user_name As String, ByVal employee_password As String) As HttpResponseMessage
            Dim response As HttpResponseMessage = New HttpResponseMessage()
            Try
                Using sqlConn As New MySqlConnection(ConfigurationManager.ConnectionStrings("constr").ConnectionString)
                    sqlConn.Open()
                    Using transaction = sqlConn.BeginTransaction()
                        Try
                            Using cmdInsert As New MySqlCommand("INSERT INTO employee_acc (employee_name, employee_last_name, campus_name, employee_user_name, employee_password) VALUES (@employee_name, @employee_last_name, @campus_name, @employee_user_name, @employee_password)", sqlConn, transaction)
                                cmdInsert.Parameters.AddWithValue("@employee_name", employee_name)
                                cmdInsert.Parameters.AddWithValue("@employee_last_name", employee_last_name)
                                cmdInsert.Parameters.AddWithValue("@campus_name", campus_name)
                                cmdInsert.Parameters.AddWithValue("@employee_user_name", employee_user_name)
                                cmdInsert.Parameters.AddWithValue("@employee_password", employee_password)


                                Dim rowsAffected = cmdInsert.ExecuteNonQuery()

                                If rowsAffected > 0 Then
                                    transaction.Commit()
                                    response = Request.CreateResponse(HttpStatusCode.OK)
                                    response.Content = New StringContent("Successfully added Employee", Encoding.UTF8)
                                Else
                                    transaction.Rollback()
                                    response = Request.CreateResponse(HttpStatusCode.InternalServerError)
                                    response.Content = New StringContent("Failed to add Employee", Encoding.UTF8)
                                End If
                            End Using
                        Catch ex As Exception
                            transaction.Rollback()
                            ' Log specific details without exposing sensitive info
                            response = Request.CreateResponse(HttpStatusCode.InternalServerError)
                            response.Content = New StringContent("Application Error: There was a problem adding the employee", Encoding.UTF8)
                        End Try
                    End Using
                End Using
            Catch ex As MySqlException
                ' Log database error without exposing sensitive details
                response = Request.CreateResponse(HttpStatusCode.InternalServerError)
                response.Content = New StringContent("Database Error: There was a problem with the database", Encoding.UTF8)
            Catch ex As Exception
                ' Log application error without exposing sensitive details
                response = Request.CreateResponse(HttpStatusCode.InternalServerError)
                response.Content = New StringContent("Application Error: There was an application issue", Encoding.UTF8)
            End Try

            Return response
        End Function


        <Route("Dashboard/data", Name:="Get_dashboard_data")>
        Public Function Get_dashboard_data() As IHttpActionResult
            Dim stats As List(Of history) = New List(Of history)
            Using sqlConn As New MySqlConnection(ConfigurationManager.ConnectionStrings("constr").ConnectionString)
                If sqlConn.State = ConnectionState.Closed Then
                    Try
                        sqlConn.Open()
                        Using msqlcom As New MySqlCommand("SELECT * FROM history ", sqlConn)
                            Using dtReader As MySqlDataReader = msqlcom.ExecuteReader()
                                If dtReader.HasRows = True Then
                                    While dtReader.Read()
                                        Dim dataObj As New history
                                        With dataObj
                                            .item_name = dtReader("item_name").ToString()
                                            .serial_number = dtReader("serial_number").ToString()
                                            .date_transfer = dtReader("date_transfer").ToString()
                                            .last_location = dtReader("last_location").ToString()
                                            .current_location = dtReader("current_location").ToString()
                                            .transfer_by = dtReader("transfer_by").ToString()
                                            .requested_by = dtReader("requested_by").ToString()
                                            .campus_name = dtReader("campus_name").ToString()
                                        End With
                                        stats.Add(dataObj)
                                    End While
                                    Return Ok(stats)
                                Else
                                    Return NotFound()
                                End If
                                dtReader.Close()
                            End Using
                            msqlcom.Dispose()
                        End Using
                        sqlConn.Close()
                    Catch ex As Exception
                        Return InternalServerError(ex)
                    End Try
                Else
                    Return InternalServerError()
                End If
            End Using
        End Function



    End Class
End Namespace