<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DelegationPanel.aspx.cs" Inherits="DF2023.WebPages.DelegationPanel" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:Button runat="server" Text="Generate Delegations" ClientIDMode="Static" ID="btnGenerateDelegation" OnClick="btnGenerateDelegation_Click" />
        </div>
    </form>
</body>
</html>
