<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Countries.aspx.cs" Inherits="DF2023.ImportRelatedData.Countries" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            
            <asp:Literal ID="Literal1" runat="server"></asp:Literal>
            <asp:Button ID="ImportCountries" runat="server" Text="Import Countries" OnClick="ImportCountries_Click" />
        </div>
    </form>
</body>
</html>
