<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="HttpLibArticleSite.Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <style type="text/css">
        pre.box { border: solid 1px black; padding: 10px; margin: 10px; font-size: 11px; }
    </style>
    <title></title>

    <script src="Scripts/json2.js" type="text/javascript"></script>

    <script src="Scripts/jquery-1.4.1.min.js" type="text/javascript"></script>

    <script type="text/javascript">
    
    
    
        $().ready(function() {

            // create a settings constructor that we can reuse
            var wcfAjaxSettings = function(operation, data) {

                // ajax wcf services, [ScriptService] web services and .asp PageMethods all
                // share the same behavior and can be called thus:
                this.url = 'AjaxService.svc/' + operation;
                this.data = JSON.stringify(data);
                this.type = "POST";
                this.dataType = "json";
                this.contentType = "application/json";

                this.success = function(data) {
                    // ajax wcf json is d: wrapped
                    // use stringify to pretty print the object
                    var resultText = JSON.stringify(data.d, null, " ");
                    $('#divOut').text(resultText).css('color', 'green').css('border-color', 'green');
                    $('#txtInput').val('');
                };

                this.error = function(xhr, status, error) {
                    // parse into an object and then stringify to pretty print
                    var resultText = JSON.stringify(JSON.parse(xhr.responseText), null, " ");
                    $('#divOut').text(resultText).css('color', 'red').css('border-color', 'red');
                };
            };

            // bind each button to a endpoint method
            $('#btnPut').click(function() {
                var input = { input: $('#txtInput').val() };
                var settings = new wcfAjaxSettings('PutSessionVar', input);
                $.ajax(settings);
            });

            $('#btnGet').click(function() {
                var settings = new wcfAjaxSettings('GetSessionVar');
                $.ajax(settings);
            });

            $('#btnErr').click(function() {
                var settings = new wcfAjaxSettings('PutSessionVar');
                $.ajax(settings);
            });

        });
    </script>

</head>
<body>
    <h2>
        My Mindbendingly Complex Ajax App
    </h2>
    <p>
        &nbsp;</p>
    <form id="Form1" runat="server">
    <p>
        My Session Id :
        <asp:Label ID="SessionIdLabel" runat="server"></asp:Label></p>
    </form>
    Session Var :
    <input type="text" id="txtInput" /><br />
    <input type="button" id="btnPut" value="Put Session Var" />&nbsp;&nbsp;
    <input type="button" id="btnGet" value="Get Session Var" />&nbsp;&nbsp;
    <input type="button" id="btnErr" value="Generate Exception" /><br />
    <pre id="divOut" class="box"></pre>
</body>
</html>
