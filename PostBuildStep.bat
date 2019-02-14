@echo off
set from=%1
set libs=%2libs
set pdb=%2pdb
echo %from%
echo %libs%
echo %pdb%
echo "Creating folders if necessary..."
if not exist %libs% mkdir %libs%
if not exist %pdb% mkdir %pdb%
echo "Moving all ref files..."
move /Y %from%*.ref %libs%
move /Y %from%*.winmd %libs%
echo "Moving pdb files..."
move /Y %from%*.pdb %pdb%
echo "Deleteing all xml files..."
del /S /F /Q %from%*.xml