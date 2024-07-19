<%@ Page Title="" Language="C#" MasterPageFile="~/WebPages/CPanel.Master" AutoEventWireup="true" CodeBehind="AddDelegation.aspx.cs" Inherits="DF2023.WebPages.AddDelegation" %>

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
                        <label for="NbrDelegation">Nbr Delegation:</label>
                        <asp:TextBox runat="server" ClientIDMode="Static" ID="NbrDelegation" placeholder="ex:100"></asp:TextBox>
                    </div>
                    <div class="sf-fieldWrp">
                        <asp:RadioButtonList ID="radioSingleStat" RepeatLayout="Flow" ClientIDMode="Static" runat="server">
                            <asp:ListItem Text="Single" Value="1"></asp:ListItem>
                            <asp:ListItem Text="Not Single" Value="0"></asp:ListItem>
                            <asp:ListItem Text="Random" Value="2" Selected="True"></asp:ListItem>
                        </asp:RadioButtonList>
                    </div>
                    <div class="sf-fieldWrp">
                        <label for="NbrDelegation">InvitationDate:</label>
                        <asp:TextBox runat="server" ClientIDMode="Static" ID="InvitationDate" placeholder="2024-06-01"></asp:TextBox>
                    </div>
                    <p data-sf-role="error-message" role="alert"></p>
                    <div class="sf-fieldWrp">
                        <asp:Button runat="server" ClientIDMode="Static" ID="btnGenerateDelegation" Text="Submit" OnClick="btnGenerateDelegation_Click" class="button button-primary " />
                    </div>
                </div>
            </div>
        </div>
        <div class="-sf-clearfix"></div>
        <div class="container">
            <div class="row">
                <div class="col-md-12">
                    <p>
                        <asp:Label Font-Size="Small" Font-Italic="true" Font-Bold="true" ForeColor="#8A1538" ID="labFailedResult" runat="server" Text="The following Delegation couldn't be saved for some reason [XXX]" Visible="false"></asp:Label></p>
                    <div class="table-responsive">
                        <asp:ListBox runat="server" ID="FailedResult" ClientIDMode="Static" Visible="false"></asp:ListBox>
                    </div>
                </div>
            </div>
        </div>
        <div class="-sf-clearfix"></div>
        <div class="container">
            <div class="row">
                <div class="col-md-12">
                    <div class="table-responsive">
                        <asp:GridView ID="grid" ClientIDMode="Static" runat="server" AutoGenerateColumns="false" OnRowCommand="grid_RowCommand" DataKeyNames="Id">
                            <Columns>
                                <asp:BoundField DataField="Title" HeaderText="Title" />
                                <asp:BoundField DataField="TitleAr" HeaderText="Title (Ar)" />
                                <asp:BoundField DataField="ContactName" HeaderText="Contact Name" />
                                <asp:BoundField DataField="ContactEmail" HeaderText="Email" />
                                <asp:BoundField DataField="ContactPhoneNumber" HeaderText="Phone" />
                                <asp:BoundField DataField="SecondaryEmail" HeaderText="Secondary Email" />
                                <asp:BoundField DataField="IsSingle" HeaderText="IsSingle" />
                                <asp:TemplateField HeaderText="Actions">
                                    <ItemTemplate>
                                        <asp:ImageButton runat="server" ClientIDMode="Static" ID="btnView" ImageUrl="~/WebAssets/Images/view50x50.png" CommandName="ViewItem" ToolTip="View" CommandArgument='<%# Eval("Id") %>' />
                                        <asp:ImageButton runat="server" ClientIDMode="Static" ID="btnViewUser" ImageUrl="~/WebAssets/Images/user50x50.png" CommandName="ViewUser" ToolTip="User" CommandArgument='<%# Eval("Id") %>' />
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <!-- jQuery (necessary for Bootstrap's JavaScript plugins) -->
    <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.3.1/jquery.min.js"></script>
    <!-- Include all compiled plugins (below), or include individual files as needed -->
    <script src="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/js/bootstrap.min.js"></script>
</asp:Content>



