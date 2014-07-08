Imports System.Web.Services
Imports System.Data
Imports System.Data.SqlClient
Imports System.Collections.Generic
Imports System.Web.Script.Serialization
Imports System.Web.Script.Services

Partial Class AjaxJqueryDataTables
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(sender As Object, e As System.EventArgs) Handles Me.Load

    End Sub

    <WebMethod()> _
    Public Shared Function GetItemsPost(ByVal paramlist As List(Of String)) As String
        '*******************************************************************************
        'Paramlist List:    paramlist(0) - sEcho (draw) 
        '                   paramlist(1) - DisplayLength (how many rows on the page)
        '                   paramlist(2) - Order by field
        '                   paramlist(3) - Order by Direction
        '                   paramlist(4) - Datatables Search field
        '                   paramlist(5) - DisplayStart (what row to start on)
        '*******************************************************************************

        Dim sEcho As Integer = Convert.ToInt32(paramlist(0).ToString())
        Dim iDisplayLength As Integer = Convert.ToInt32(paramlist(1).ToString())
        Dim iDisplayStart As Integer = Convert.ToInt32(paramlist(5).ToString())
        Dim rawSearch As String = paramlist(4).ToString()

        Dim sb = New StringBuilder()

        Dim whereClause = String.Empty

        Dim filteredWhere = String.Empty

        sb.Clear()

        'SEARCHING 

        Dim wrappedSearch = "'%" & rawSearch & "%'"

        If rawSearch.Length > 0 Then
            sb.Append(" WHERE id LIKE ")
            sb.Append(wrappedSearch)
            sb.Append(" OR engine LIKE ")
            sb.Append(wrappedSearch)
            sb.Append(" OR browser LIKE ")
            sb.Append(wrappedSearch)
            sb.Append(" OR platform LIKE ")
            sb.Append(wrappedSearch)
            sb.Append(" OR version LIKE ")
            sb.Append(wrappedSearch)
            sb.Append(" OR grade LIKE ")
            sb.Append(wrappedSearch)

            filteredWhere = sb.ToString()
        End If

        'ORDERING

        sb.Clear()

        Dim orderByClause As String = String.Empty
        sb.Append(Convert.ToInt32(paramlist(2).ToString()))

        sb.Append(" ")

        sb.Append(paramlist(3).ToString())

        orderByClause = sb.ToString()

        If Not [String].IsNullOrEmpty(orderByClause) Then

            orderByClause = orderByClause.Replace("0", ", id ")
            orderByClause = orderByClause.Replace("1", ", engine ")
            orderByClause = orderByClause.Replace("2", ", browser ")
            orderByClause = orderByClause.Replace("3", ", platform ")
            orderByClause = orderByClause.Replace("4", ", version ")
            orderByClause = orderByClause.Replace("5", ", grade ")

            orderByClause = orderByClause.Remove(0, 1)
        Else
            orderByClause = "id ASC"
        End If
        orderByClause = "ORDER BY " & orderByClause

        sb.Clear()

        Dim numberOfRowsToReturn = ""
        numberOfRowsToReturn = If(iDisplayLength = -1, "TotalRows", (iDisplayStart + iDisplayLength).ToString())


        Dim tb As New StringBuilder()
        tb.Length = 0
        tb.AppendLine(" DECLARE @MA TABLE ( id int, engine varchar(255), browser varchar(255), platform varchar(255), version float, grade varchar(20) ) ")
        tb.AppendLine(" INSERT INTO @MA ( id, engine, browser, platform, version, grade ) ")
        tb.AppendLine("                 SELECT id, engine, browser, platform, version, grade ")
        tb.AppendLine("                 FROM ajax ")
        tb.AppendLine("                 {4}     ")
        tb.AppendLine("             SELECT * FROM ")
        tb.AppendLine("                 (SELECT row_number() OVER ({0}) AS RowNumber, * ")
        tb.AppendLine("                 FROM (SELECT (SELECT count([@MA].id) ")
        tb.AppendLine("                      FROM @MA) as TotalRows ")
        tb.AppendLine("                     , ( SELECT COUNT( [@MA].id) FROM @MA {1}) AS TotalDisplayRows ")
        tb.AppendLine("                     ,[@MA].id ")
        tb.AppendLine("                     ,[@MA].engine ")
        tb.AppendLine("                     ,[@MA].browser ")
        tb.AppendLine("                     ,[@MA].platform ")
        tb.AppendLine("                     ,[@MA].version ")
        tb.AppendLine("                     ,[@MA].grade ")
        tb.AppendLine("                     FROM @MA {1}) RawResults) Results ")
        tb.AppendLine(" WHERE RowNumber between {2} and {3} ")
        Dim query As String = tb.ToString()

        query = String.Format(query, orderByClause, filteredWhere, iDisplayStart + 1, numberOfRowsToReturn, whereClause)


        Dim conn As New SqlConnection(VW.SQLConnect.ConnectGuest())

        Try
            conn.Open()
        Catch e As Exception
            Console.WriteLine(e.ToString())
        End Try

        Dim DB = New SqlDataAdapter(query, conn)
        Dim data As New DataSet()
        DB.Fill(data)

        DB.Dispose()

        Dim totalDisplayRecords = ""
        Dim totalRecords = ""
        Dim outputJson As String = String.Empty

        Dim rowClass = ""
        Dim count = 1

        If data.Tables(0).Rows.Count > 0 Then
            totalRecords = data.Tables(0).Rows(0)("TotalRows").ToString()
            totalDisplayRecords = data.Tables(0).Rows(0)("TotalDisplayRows").ToString()
        End If

        For Each dr As DataRow In data.Tables(0).Rows
            sb.Append("{")
            sb.AppendFormat("""DT_RowId"": ""{0}""", System.Math.Max(System.Threading.Interlocked.Increment(count), count - 1))
            sb.Append(",")
            sb.AppendFormat("""ID"": ""{0}""", dr("id"))
            sb.Append(",")
            sb.AppendFormat("""Engine"": ""{0}""", dr("engine"))
            sb.Append(",")
            sb.AppendFormat("""Browser"": ""{0}""", dr("browser"))
            sb.Append(",")
            sb.AppendFormat("""Platform"": ""{0}""", dr("platform"))
            sb.Append(",")
            sb.AppendFormat("""Version"": ""{0}""", dr("version"))
            sb.Append(",")
            sb.AppendFormat("""Grade"": ""{0}""", dr("grade"))
            sb.Append("},")
        Next


        ' handles zero records
        If totalRecords = 0 Then
            sb.Append("{")
            sb.Append("""draw"": ")
            sb.AppendFormat("""{0}""", sEcho)
            sb.Append(",")
            sb.Append("""recordsTotal"": 0")
            sb.Append(",")
            sb.Append("""recordsFiltered"": 0")
            sb.Append(", ")
            sb.Append("""data"": [ ")
            sb.Append("]}")
            outputJson = sb.ToString()

            Return outputJson
        End If
        outputJson = sb.Remove(sb.Length - 1, 1).ToString()
        sb.Clear()

        sb.Append("{")
        sb.Append("""draw"": ")
        sb.AppendFormat("""{0}""", sEcho)
        sb.Append(",")
        sb.Append("""recordsTotal"": ")
        sb.AppendFormat("""{0}""", totalRecords)
        sb.Append(",")
        sb.Append("""recordsFiltered"": ")
        sb.AppendFormat("""{0}""", totalDisplayRecords)
        sb.Append(", ")
        sb.Append("""data"": [ ")
        sb.Append(outputJson)
        sb.Append("]}")
        outputJson = sb.ToString()

        Return outputJson
    End Function

End Class
