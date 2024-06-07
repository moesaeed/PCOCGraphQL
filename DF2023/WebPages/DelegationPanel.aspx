<%@ Page Title="" Language="C#" MasterPageFile="~/WebPages/CPanel.Master" AutoEventWireup="true" CodeBehind="DelegationPanel.aspx.cs" Inherits="DF2023.WebPages.DelegationPanel" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder2" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="sf-fieldWrp row">
        <div class="sf_colsIn columns large-12">
            <div class="point-text">
                <h6 class="point-title">Add Random Delegation</h6>
            </div>
            <div class="sf-fieldWrp">
                 <asp:DropDownList runat="server" ClientIDMode="Static" ID="Conventions" class="qatar-select" AutoPostBack="false" required></asp:DropDownList>
            </div>
            <div class="sf-fieldWrp">
                <asp:TextBox runat="server" ClientIDMode="Static" ID="NbrDelegation" placeholder="ex:100"></asp:TextBox>
            </div>
            <div class="sf-fieldWrp">
                <asp:Button runat="server" ClientIDMode="Static" ID="btnGenerateDelegation" Text="Submit" OnClick="btnGenerateDelegation_Click" />
            </div>
        </div>
    </div>

        <div class="sf-fieldWrp row">
        <div class="sf_colsIn columns large-12">
            <div class="point-text">
                <h6 class="point-title">Add Random Guests</h6>
            </div>
            <div class="sf-fieldWrp">
                <asp:TextBox runat="server" ClientIDMode="Static" ID="NbrGuests" placeholder="ex:100"></asp:TextBox>
            </div>
            <div class="sf-fieldWrp">
                <asp:Button runat="server" ClientIDMode="Static" ID="btnGenerateGuests" Text="Submit" OnClick="btnGenerateGuests_Click" />
            </div>
        </div>
    </div>
</asp:Content>



