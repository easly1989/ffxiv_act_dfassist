@echo off
set from=%1
set resources=%2Resources\
set libs=libs
set pdb=pdb
set data=data
set loc=localization
echo "Creating folders if necessary..."
if not exist %libs% mkdir %libs%
if not exist %pdb% mkdir %pdb%
if not exist %data% mkdir %data%
if not exist %loc% mkdir %loc%
echo "Moving pdb files..."
move /Y %from%*.pdb %pdb%
move /Y %libs%\*.pdb %pdb%
echo "Deleting not needed files..."
for /f %%F in ('dir  %libs% /b /a-d ^| findstr /vile ".dll"') do del "%%F" /F /S /Q
echo "Copying files to resources"
xcopy /S /Y %resources%data %data%
xcopy /S /Y %resources%localization %loc%
echo "All done! :)"