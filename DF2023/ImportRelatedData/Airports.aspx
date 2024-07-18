<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Airports.aspx.cs" Inherits="DF2023.ImportRelatedData.Airports" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:Literal ID="Literal1" runat="server"></asp:Literal>
            <asp:Button ID="ImportAirports" runat="server" Text="Import Airports" OnClick="ImportAirports_Click" />
        </div>
    </form>
</body>
</html>
