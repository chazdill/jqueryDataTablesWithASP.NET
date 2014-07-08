<%@ Page Language="VB" AutoEventWireup="false" CodeFile="AjaxJqueryDataTables.aspx.vb" Inherits="AjaxJqueryDataTables" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Ajax Jquery Datatables</title>
    <!-- HTML5 shim and Respond.js IE8 support of HTML5 elements and media queries -->
    <!--[if lt IE 9]>
      <script src="https://oss.maxcdn.com/libs/html5shiv/3.7.0/html5shiv.js"></script>
      <script src="https://oss.maxcdn.com/libs/respond.js/1.4.2/respond.min.js"></script>
    <![endif]-->
    <script src="http://ajax.aspnetcdn.com/ajax/modernizr/modernizr-2.7.2.js"></script>
    <script src="//ajax.googleapis.com/ajax/libs/jquery/1.11.0/jquery.min.js"></script>
    <link href="//cdn.datatables.net/1.10.0/css/jquery.dataTables.css" rel="stylesheet" />
    <script type="text/javascript" src="//cdn.datatables.net/1.10.0/js/jquery.dataTables.js"></script>
    <script type="text/javascript">
        $(document).ready(function () {
            var dtData = new Object();

            $('#displayData').dataTable({
                "processing": true,
                "serverSide": true,
                "ajax": function (data, callback, settings) {
                    dtData = data;

                    var paramlist = new Array();
                    paramlist[0] = dtData.draw;
                    paramlist[1] = dtData.length;
                    paramlist[2] = dtData.order[0].column;
                    paramlist[3] = dtData.order[0].dir;
                    paramlist[4] = dtData.search.value;
                    paramlist[5] = dtData.start;

                    var jsonText = JSON.stringify({ paramlist: paramlist });

                    $.ajax({
                        type: "POST",
                        url: "AjaxJqueryDataTables.aspx/GetItemsPost",
                        data: jsonText,
                        contentType: "application/json; charset=utf-8",
                        dataType: "json",
                        success: function (msg) {
                            var json = $.parseJSON(msg.d);
                            callback(json);
                        }
                    });

                },
                "columns": [
                    { "data": "ID" },
                    { "data": "Engine" },
                    { "data": "Browser" },
                    { "data": "Platform" },
                    { "data": "Version" },
                    { "data": "Grade" }
                ]
            });
        });
    </script>
</head>
<body>
    <form id="form1" runat="server">
    <div>
<table cellpadding="0" cellspacing="0" border="0" class="display" id="displayData"> 
            <thead>
                <tr>
                    <th align="left">ID</th>
                    <th align="left">Engine</th>
                    <th align="left">Browser</th>
                    <th align="left">Platform</th>
                    <th align="left">Version</th>
                    <th align="left">Grade</th>
                </tr>
            </thead>
            <tbody>

            </tbody>
        </table>   
    </div>
    </form>
</body>
</html>
