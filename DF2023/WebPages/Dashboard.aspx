<%@ Page Title="" Language="C#" MasterPageFile="~/WebPages/CPanel.Master" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="DF2023.WebPages.Dashboard" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder2" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container">
        <h1 class="inner-nav__title">Add Delegations </h1>
        <div class="sf_colsIn columns medium-6 large-6">
            <div class="sf-fieldWrp row">
                <div class="sf_colsIn columns large-12">
                    <div class="sf-fieldWrp">
                        <label for="Conventions">Conventions:</label>
                        <asp:DropDownList runat="server" ClientIDMode="Static" ID="Conventions" class="qatar-select" AutoPostBack="false" required="true"></asp:DropDownList>
                    </div>
                    <p data-sf-role="error-message" role="alert"></p>
                    <div class="sf-fieldWrp">
                        <asp:Button runat="server" ClientIDMode="Static" ID="btnRelateDelegationToGuests" OnClick="btnRelateDelegationToGuests_Click" Text="Submit" class="button button-primary " />
                    </div>
                </div>
            </div>
        </div>
    </div>

</asp:Content>
