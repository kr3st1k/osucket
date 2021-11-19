cd .\osucket\bin\
mkdir output_x86
$Path = ".\Release\net5.0\win-x86\"

$fileArray ="Fleck.dll", "Newtonsoft.Json.dll", "osu.Game.dll", "osu.Framework.dll", "osu.Game.Rulesets.Catch.dll", "osu.Game.Rulesets.Mania.dll", "osu.Game.Rulesets.Osu.dll", "osu.Game.Rulesets.Taiko.dll", "osucket.dll", "osucket.exe",
"osucket.runtimeconfig.json", "OsuMemoryDataProvider.dll", "osuTK.dll",
"ProcessMemoryDataFinder.dll", "Realm.dll"


foreach ($item in $fileArray)
{
    copy $Path$item .\output_x86\
}
