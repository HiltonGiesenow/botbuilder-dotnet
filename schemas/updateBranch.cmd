@echo off
setlocal

rem ensure we are in the right directory
cd /D "%~dp0"

rem Get the current branch
set current=
for /f "delims=" %%a in ('git rev-parse --abbrev-ref HEAD') do @set current=%%a 

rem Ensure we have an explicit branch
set branch=%1
if "%branch%" == "" goto usage

rem Switch to new branch
:switch
echo *** This will checkout branch %branch%, do a pull, update schemas to point to it and do a push. ***
set /p yes=Are you sure you want to do this [y/n]? 
if "%yes%" neq "y" goto usage
echo Switching to branch %branch% from %current%
git checkout %branch%
if %errorlevel% neq 0 goto done
git pull
if %errorlevel% neq 0 goto done

rem Update .schema
:update
echo Updating .schema files and building sdk.schema
call dialogSchema ../libraries/**/*.schema -u -b %branch% -o sdk.schema
if %errorlevel% neq 0 goto done

rem Commit
echo Committing
git commit -a -m "Update .schema files to point to branch %branch%"
if %errorlevel% neq 0 goto done

goto done

rem Push
echo Pushing schema changes to branch %branch% and switching back to %current%
git push
if %errorlevel% neq 0 goto done

git checkout %current%
goto done

:usage
echo Usage: update branch
echo Schema files have a problem in that they need to be present in order to be referred to, but we want them to be release specific.
echo This batch file will:
echo 1) Checkout and pull branch. 
echo 2) Run dialogSchema to modify component.schema and all .schema files to point to new branch and be aggregated in sdk.schema
echo 3) Commit the resulting changes to branch.
echo 4) Push the results to branch.
goto done

:done
popd