cd .\osucket\bin\
New-Item -ItemType Directory -Force -Path .\output_x64\
$Path = ".\Release\net5.0\win-x64\"

$fileArray ="Fleck.dll", "Newtonsoft.Json.dll", "osu.Game.dll", "osu.Framework.dll", "osu.Game.Rulesets.Catch.dll", "osu.Game.Rulesets.Mania.dll", "osu.Game.Rulesets.Osu.dll", "osu.Game.Rulesets.Taiko.dll", "osucket.dll", "osucket.calculations.dll", "osucket.exe",
"osucket.runtimeconfig.json", "OsuMemoryDataProvider.dll", "osuTK.dll",
"ProcessMemoryDataFinder.dll", "Realm.dll"


foreach ($item in $fileArray)
{
    copy $Path$item .\output_x64\
}
