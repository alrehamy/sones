@echo off
echo Backing up ITestPlugins
md .temp
copy .\Tests\TestLibrary\PluginManager\ITestPluginDLLs\*.dll .\.temp\*.safe
echo Cleaning Solution...
cd Applications
del /S /Q *.pidb *.dll *.exe *.mdb *.pdb *.FilesWrittenAbsolute.txt
cd ..\GraphDB
del /S /Q *.pidb *.dll *.exe *.mdb *.pdb *.FilesWrittenAbsolute.txt
cd ..\GraphDS
del /S /Q *.pidb *.dll *.exe *.mdb *.pdb *.FilesWrittenAbsolute.txt
cd ..\GraphFS
del /S /Q *.pidb *.dll *.exe *.mdb *.pdb *.FilesWrittenAbsolute.txt
cd ..\GraphQL
del /S /Q *.pidb *.dll *.exe *.mdb *.pdb *.FilesWrittenAbsolute.txt
cd ..\Library
del /S /Q *.pidb *.dll *.exe *.mdb *.pdb *.FilesWrittenAbsolute.txt
cd ..\Plugins
del /S /Q *.pidb *.dll *.exe *.mdb *.pdb *.FilesWrittenAbsolute.txt
cd ..
echo Restoring ITestPlugins
copy .\.temp\*.safe .\Tests\TestLibrary\PluginManager\ITestPluginDLLs\*.dll
del .\.temp\*.safe
rd .temp