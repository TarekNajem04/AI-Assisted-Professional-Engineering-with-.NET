@echo off
setlocal

echo +=======================+
echo += Author: Tarek Najem =+
echo += Simple Clean Script =+
echo +=======================+
echo.
echo +===============================================+
echo += BIN / OBJ / OUTPUT / ARTIFACTS / TESTRESULTS CLEAN STARTED / Test Coverage Report Detailed =
echo +===============================================+
echo.

REM Folders to delete
set TARGETS=bin obj output artifacts TestResults TestResultsLocal TestCoverageReportDetailed TestCoverageReport

REM Search and delete all matching folders
for %%T in (%TARGETS%) do (
    for /D /R %%G in (%%T) do (
        if exist "%%G" (
            echo Deleting: "%%G"
            rmdir /S /Q "%%G"
        )
    )
)

echo.
echo Cleanup complete.
pause
