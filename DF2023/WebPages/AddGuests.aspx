<%@ Page Title="" Language="C#" MasterPageFile="~/WebPages/CPanel.Master" AutoEventWireup="true" CodeBehind="AddGuests.aspx.cs" Inherits="DF2023.WebPages.AddGuests" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder2" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
     <div class="container">
     <h1 class="inner-nav__title">Add Guests </h1>
     <div class="sf_colsIn columns medium-6 large-6">
         <div class="sf-fieldWrp row">
             <div class="sf_colsIn columns large-12">
                 <div class="sf-fieldWrp">
                     <label for="Conventions">Conventions:</label>
                     <asp:DropDownList runat="server" ClientIDMode="Static" ID="Conventions" class="qatar-select" AutoPostBack="false" required="true"></asp:DropDownList>
                 </div>
                 <p data-sf-role="error-message" role="alert"></p>
                 <div class="sf-fieldWrp">
                     <label for="NbrDelegation">Nbr Guests:</label>
                     <asp:TextBox runat="server" ClientIDMode="Static" ID="NbrGuest" placeholder="ex:100"></asp:TextBox>
                 </div>
                 <p data-sf-role="error-message" role="alert"></p>
                 <div class="sf-fieldWrp">
                     <asp:Button runat="server" ClientIDMode="Static" ID="btnGenerateGuest" Text="Submit" OnClick="btnGenerateGuest_Click" class="button button-primary " />
                 </div>
             </div>
         </div>
     </div>
     <div class="-sf-clearfix"></div>
     <div class="container">
         <div class="row">
             <div class="col-md-12">
                 <p><asp:Label Font-Size="Small" Font-Italic="true" Font-Bold="true" ForeColor="#8A1538" ID ="labFailedResult" runat="server" Text="The following Guests couldn't be saved for some reason [XXX]" Visible="false"></asp:Label></p>
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
                             <asp:BoundField DataField="FirstName" HeaderText="First Name" />
                             <asp:BoundField DataField="LastName" HeaderText="Last Name" />
                             <asp:BoundField DataField="Phone" HeaderText="Phone" />
                             <asp:BoundField DataField="Email" HeaderText="Email" />
                             <asp:BoundField DataField="IsLocal" HeaderText="IsLocal" />
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
