@echo off
set from=%1
set resources=%2Resources\
set libs=libs
set pdb=pdb
set data=data
set loc=localization
set img=images
echo %from%
echo %libs%
echo %pdb%
echo %data%
echo %loc%
echo %img%
echo %resources%
echo "Creating folders if necessary..."
if not exist %libs% mkdir %libs%
if not exist %pdb% mkdir %pdb%
if not exist %data% mkdir %data%
if not exist %loc% mkdir %loc%
if not exist %img% mkdir %img%
echo "Moving all dll files..."
move /Y %from%*.dll %libs%
move /Y %from%DFAssist.Plugin.dll.config %libs%
echo "Moving pdb files..."
move /Y %from%*.pdb %pdb%
echo "Deleteing all unused files..."
del /F /Q %from%*.*
echo "Copying files to resources"
xcopy /S /Y %resources%data %data%
xcopy /S /Y %resources%localization %loc%
xcopy /S /Y %resources%images %img%
echo "Moving DFAssist back to main directory..."
move /Y %libs%\DFAssist.dll %from%DFAssist.dll
echo "All done! :)"