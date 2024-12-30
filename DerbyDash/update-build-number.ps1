# Check if the Awk directory exists
$awkDirectory = "C:\Program Files\Utilities\Awk"
$logFile = "D:\Brad\VSProjects\DerbyDash3\DerbyDash\build-log.txt"
$projectDir = $args[0]

if (Test-Path $awkDirectory) {
	$inputfile = "$projectDir\Components\AppInfo.cs"
	$bakfile = "$projectDir\Components\AppInfo.bak"
	$tmpfile = "$projectDir\Components\AppInfo.tmp"
	$outputfile = $inputfile
    Copy-Item -Path $inputfile -Destination $bakfile -Force

    # Running the AWK script
    $awkCommand = "& `"$awkDirectory\bin\gawk.exe`" -f `"$projectDir\update-build-number.awk`" $bakfile > $tmpfile "# 2>> $logFile"
    Invoke-Expression $awkCommand
	Get-Content $tmpfile | Set-Content -Encoding utf8 $outputfile

}
