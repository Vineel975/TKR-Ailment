Server Error in '/' Application.
Parser Error
Description: An error occurred during the parsing of a resource required to service this request. Please review the following specific parse error details and modify your source file appropriately.

Parser Error Message: The using block is missing a closing "}" character.  Make sure you have a matching "}" character for all the "{" characters within this block, and that none of the "}" characters are being interpreted as markup.


Source Error:


Line 1123:
Line 1124:<div>
Line 1125:    @using (Html.BeginForm("Index", "MedicalScrutiny", FormMethod.Post, new { @class = "form-group", role = "form", id = "form" }))
Line 1126:    {
Line 1127:        <!--POPUP Details-->

Source File: /Views/MedicalScrutiny/Index.cshtml    Line: 1125

Version Information: Microsoft .NET Framework Version:4.0.30319; ASP.NET Version:4.8.9319.0
