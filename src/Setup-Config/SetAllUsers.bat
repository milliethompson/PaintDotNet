echo Executing SQL command on PaintDotNet.msi: UPDATE Property SET Property.Value = 'ALL' WHERE Property.Property = 'FolderForm_AllUsers'
echo wirunsql.vbs "%1" "UPDATE Property SET Property.Value = 'ALL' WHERE Property.Property = 'FolderForm_AllUsers'"
wirunsql.vbs ""%1"" "UPDATE Property SET Property.Value = 'ALL' WHERE Property.Property = 'FolderForm_AllUsers'"