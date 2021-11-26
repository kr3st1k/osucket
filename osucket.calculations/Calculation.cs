﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using osu.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Taiko;
using osucket.Calculations.Enums;
using osucket.Calculations.Memory;
using osucket.Calculations.Models;
using osucket.Calculations.OsuPerformanceCalculator;
using OsuPerformanceCalculator;
using OsuMemoryDataProvider;
using OsuMemoryDataProvider.OsuMemoryModels;
using OsuMemoryDataProvider.OsuMemoryModels.Direct;
using Math = System.Math;
using Mods = OsuMemoryDataProvider.OsuMemoryModels.Abstract.Mods;
using WorkingBeatmap = osucket.Calculations.OsuPerformanceCalculator.WorkingBeatmap;

namespace osucket.Calculations
{
	public static class Calculation
	{
		private static readonly OsuBaseAddresses BaseAddresses = new();
		private static Ruleset GetRuleset(RuleSet ruleSet)
		{
			return ruleSet switch
			{
				RuleSet.Osu => new OsuRuleset(),
				RuleSet.Mania => new ManiaRuleset(),
				RuleSet.Taiko => new TaikoRuleset(),
				_ => new CatchRuleset()
			};
		}

		private static double CalculateUnstableRate(IReadOnlyCollection<int> hitError)
		{
			if (hitError == null || hitError.Count < 1) return 0;

			int totalAll = hitError.Sum();
			int average = totalAll / hitError.Count;
			double variance = hitError.Sum(item => Math.Pow(item - average, 2));
			variance /= hitError.Count;

			return Math.Sqrt(variance) * 10;
		}

		private static double GetAccuracy(GamePlay gameplay)
		{
			return gameplay.GameMode switch
			{
				RuleSet.Osu => 100.00 *
				                (6 * (double) gameplay.Count300 + 2 * (double) gameplay.Count100 + (double) gameplay.Count50) /
				                (6 * ((double) gameplay.Count50 + (double) gameplay.Count100 + (double) gameplay.Count300 +
				                      (double) gameplay.CountMiss)),
				RuleSet.Taiko => 100.00 * (2 * (double) gameplay.Count300 + (double) gameplay.Count100) / (2 *
					((double) gameplay.Count300 + (double) gameplay.Count100 + (double) gameplay.CountMiss)),
				RuleSet.Fruits => 100.00 * ((double) gameplay.Count300 + (double) gameplay.Count100 + (double) gameplay.Count50) /
				               ((double) gameplay.Count300 + (double) gameplay.Count100 + (double) gameplay.Count50 +
				                (double) gameplay.CountKatu + (double) gameplay.CountGeki),
				RuleSet.Mania => 100.00 *
					(6 * (double) gameplay.CountGeki + 6 * (double) gameplay.Count300 + 4 * (double) gameplay.CountKatu +
					 2 * (double) gameplay.Count100 + (double) gameplay.Count50) / (6 * ((double) gameplay.Count50 +
						(double) gameplay.Count100 + (double) gameplay.Count300 + (double) gameplay.CountMiss +
						(double) gameplay.CountGeki + (double) gameplay.CountKatu)),
				_ => throw new ArgumentOutOfRangeException("gameplay")
			};
		}

		private static List<Mod> GetMods(IEnumerable<string> mods, Ruleset ruleset)
		{
			var availableMods = ruleset.CreateAllMods().ToList();
			return mods.Select(modString => availableMods.FirstOrDefault(m => string.Equals(m.Acronym, modString, StringComparison.CurrentCultureIgnoreCase))).Where(newMod => newMod != null).ToList();
		}

		public static string GetData(string osuDir, string windowTitle)
		{
			StructuredOsuMemoryReader structuredOsuMemoryReader =
				StructuredOsuMemoryReader.Instance.GetInstanceForWindowTitleHint(windowTitle);
			var memoryReader = new MemoryReader
			{
				StructuredOsuMemoryReader = structuredOsuMemoryReader
			};

			var general = new General
			{
				OsuMemoryStatus = OsuMemoryReader.Instance.GetCurrentStatus(out int _),
				MenuRuleSet = (RuleSet)OsuMemoryReader.Instance.ReadSongSelectGameMode(),
				Song = OsuMemoryReader.Instance.GetSongString(),
				AudioPosition = memoryReader.ReadInt(BaseAddresses.GeneralData, nameof(GeneralData.AudioTime)),
				IsInterface = memoryReader.ReadBool(BaseAddresses.GeneralData, nameof(GeneralData.ShowPlayingInterface)),
				SkinName = OsuMemoryReader.Instance.GetSkinFolderName(),
				SkinDirectory = Path.Join(osuDir, "Skins", OsuMemoryReader.Instance.GetSkinFolderName()), 
				MapDirectory = Path.Join(osuDir, "Songs", OsuMemoryReader.Instance.GetMapFolderName()),
				MenuMods = OsuMemoryReader.Instance.GetMods(),
				BeatMapId = OsuMemoryReader.Instance.GetMapId(),
				CountRetries = OsuMemoryReader.Instance.GetRetrys(),
				SubmissionStatus = (SubmissionStatus)memoryReader.ReadShort(BaseAddresses.Beatmap, nameof(CurrentBeatmap.Status)),
			};
			general.MapFile = Path.Join(general.MapDirectory,
				memoryReader.ReadString(BaseAddresses.Beatmap, nameof(CurrentBeatmap.OsuFileName)));

			var gamePlay = new GamePlay();
			var osuKey = new OsuKey();
			var performance = new PerformancePoints();
			
			WorkingBeatmap currentBeatMap = general.MapFile.Contains("nekodex - circles! (peppy).osu") ? null : MapCache.GetBeatMap(general.MapFile);

			var ruleSet = currentBeatMap != null ? GetRuleset((RuleSet)currentBeatMap.RulesetID) : GetRuleset(0);
			var playableBeatMap = currentBeatMap != null ? currentBeatMap.GetPlayableBeatmap(ruleSet.RulesetInfo) : null;
			
			if (currentBeatMap is not null)
			{	
				general.SongLength = currentBeatMap.Length;
				general.BackgroundFile = Path.Join(general.MapDirectory, currentBeatMap.BackgroundFile);

				if (general.MenuRuleSet != RuleSet.Osu && currentBeatMap.RulesetID == 0)
				{
					ruleSet = GetRuleset(general.MenuRuleSet); // TODO: use extension method
					// PPCalculator calculator = PpCalculatorHelpers.GetPpCalculator(general.MenuRuleSet);
					currentBeatMap = new WorkingBeatmap(ruleSet.CreateBeatmapConverter(playableBeatMap).Convert());

				}
				var lazerMods = GetMods(((Enums.Mods)general.MenuMods).ToString().Split(","), ruleSet);
				general.MapDifficulty = DiffCalculator.GetStarRate(lazerMods, currentBeatMap, ruleSet);

			}
			
			if (general.OsuMemoryStatus == OsuMemoryStatus.Playing)
			{
				
				gamePlay.GameMode = (RuleSet)memoryReader.ReadInt(BaseAddresses.Player, nameof(Player.Mode));

				if (gamePlay.GameMode is  RuleSet.Osu or RuleSet.Fruits)
				{
					osuKey.CountKeyLeft = 
						memoryReader.ReadInt(BaseAddresses.KeyOverlay, nameof(KeyOverlay.K1Count));
					osuKey.CountKeyRight = 
						memoryReader.ReadInt(BaseAddresses.KeyOverlay, nameof(KeyOverlay.K2Count));
					osuKey.CountMouseLeft = 
						memoryReader.ReadInt(BaseAddresses.KeyOverlay, nameof(KeyOverlay.M1Count));
					osuKey.CountMouseRight = 
						memoryReader.ReadInt(BaseAddresses.KeyOverlay, nameof(KeyOverlay.M2Count));
					osuKey.CountPressedKeyLeft = 
						memoryReader.ReadBool(BaseAddresses.KeyOverlay, nameof(KeyOverlay.K1Pressed));
					osuKey.CountPressedKeyRight = 
						memoryReader.ReadBool(BaseAddresses.KeyOverlay, nameof(KeyOverlay.K2Pressed));
					osuKey.CountPressedMouseLeft = 
						memoryReader.ReadBool(BaseAddresses.KeyOverlay, nameof(KeyOverlay.M1Pressed));
					osuKey.CountPressedMouseRight = 
						memoryReader.ReadBool(BaseAddresses.KeyOverlay, nameof(KeyOverlay.M2Pressed));
				
					gamePlay.OsuKey = osuKey;
				}
				
				gamePlay.Username = 
					memoryReader.ReadString(BaseAddresses.Player, nameof(Player.Username));
				gamePlay.Accuracy = 
					memoryReader.ReadProperty<double>(BaseAddresses.Player, nameof(Player.Accuracy));
				gamePlay.Count300 = 
					memoryReader.ReadUShort(BaseAddresses.Player, nameof(Player.Hit300));
				gamePlay.Count100 = 
					memoryReader.ReadUShort(BaseAddresses.Player, nameof(Player.Hit100));
				gamePlay.Count50 = 
					memoryReader.ReadUShort(BaseAddresses.Player, nameof(Player.Hit50));
				gamePlay.CountMiss = 
					memoryReader.ReadUShort(BaseAddresses.Player, nameof(Player.HitMiss));
				gamePlay.CountGeki = 
					memoryReader.ReadUShort(BaseAddresses.Player, nameof(Player.HitGeki));
				gamePlay.CountKatu = 
					memoryReader.ReadUShort(BaseAddresses.Player, nameof(Player.HitKatu));
				gamePlay.Combo = 
					memoryReader.ReadUShort(BaseAddresses.Player, nameof(Player.Combo));
				gamePlay.MaxCombo = 
					memoryReader.ReadUShort(BaseAddresses.Player, nameof(Player.MaxCombo));
				gamePlay.IsReplay = 
					OsuMemoryReader.Instance.IsReplay();
				gamePlay.Health = 
					memoryReader.ReadProperty<double>(BaseAddresses.Player, nameof(Player.HP));
				gamePlay.HealthSmooth =
					memoryReader.ReadProperty<double>(BaseAddresses.Player, nameof(Player.HPSmooth));
				
				if (structuredOsuMemoryReader.TryReadProperty(BaseAddresses.Player, nameof(Player.ScoreV2),
					out object score))
				{
					if (score is null)
						structuredOsuMemoryReader.TryReadProperty(BaseAddresses.Player, nameof(Player.Score),
							out score);
					gamePlay.Score = (int)score;
				}
				if (structuredOsuMemoryReader.TryReadProperty(BaseAddresses.Player, nameof(Player.Mods),
					out object mods))
					gamePlay.Mods = ((Enums.Mods)((Mods)mods).Value);
				


				if (gamePlay.MaxCombo > 0)
				{
					PPCalculator calculator = PpCalculatorHelpers.GetPpCalculator(gamePlay.GameMode);
					
					performance.Performance =  calculator.Calculate(
								currentBeatMap, 
								general.AudioPosition, 
								gamePlay.Accuracy / 100, 
								gamePlay.MaxCombo, 
								gamePlay.CountMiss, 
								gamePlay.Count50, 
								string.Join(",",Enum.GetValues(typeof(Enums.Mods)).Cast<Enum>().Where(value => gamePlay.Mods.HasFlag(value))).Split(","), 
								gamePlay.Score
								);


					if (gamePlay.GameMode != RuleSet.Mania)
					{
						performance.FullComboPerformance = calculator.Calculate(
							currentBeatMap, 
							gamePlay.Accuracy / 100,
							calculator.GetMaxCombo(playableBeatMap),
							0, 
							0, 
							string.Join(",",Enum.GetValues(typeof(Enums.Mods)).Cast<Enum>().Where(value => gamePlay.Mods.HasFlag(value))).Split(","), 
							1000000
							);

					}
					performance.SuperSkillPerformance = calculator.Calculate(
						currentBeatMap, 
						1, 
						calculator.GetMaxCombo(playableBeatMap), 
						0, 
						0,
						string.Join(",",Enum.GetValues(typeof(Enums.Mods)).Cast<Enum>().Where(value => gamePlay.Mods.HasFlag(value))).Split(","), 
						1000000);

				}

				gamePlay.PerformancePoints = performance;


				gamePlay.HitError =
					memoryReader.ReadClassProperty<List<int>>(BaseAddresses.Player, nameof(Player.HitErrors));
				gamePlay.UnstableRate = CalculateUnstableRate(gamePlay.HitError);
				general.Gameplay = gamePlay;
			}

			if (general.OsuMemoryStatus == OsuMemoryStatus.ResultsScreen)
			{
				gamePlay.Username =
					memoryReader.ReadString(BaseAddresses.ResultsScreen, nameof(ResultsScreen.Username));
				gamePlay.Count300 = 
					memoryReader.ReadUShort(BaseAddresses.ResultsScreen, nameof(ResultsScreen.Hit300));
				gamePlay.Count100 = 
					memoryReader.ReadUShort(BaseAddresses.ResultsScreen, nameof(ResultsScreen.Hit100));
				gamePlay.Count50 = 
					memoryReader.ReadUShort(BaseAddresses.ResultsScreen, nameof(ResultsScreen.Hit50));
				gamePlay.CountMiss = 
					memoryReader.ReadUShort(BaseAddresses.ResultsScreen, nameof(ResultsScreen.HitMiss));
				gamePlay.CountGeki = 
					memoryReader.ReadUShort(BaseAddresses.ResultsScreen, nameof(ResultsScreen.HitGeki));
				gamePlay.CountKatu = 
					memoryReader.ReadUShort(BaseAddresses.ResultsScreen, nameof(ResultsScreen.HitKatu));
				gamePlay.MaxCombo =
					memoryReader.ReadUShort(BaseAddresses.ResultsScreen, nameof(ResultsScreen.MaxCombo));

				if (structuredOsuMemoryReader.TryReadProperty(BaseAddresses.ResultsScreen, nameof(Player.ScoreV2),
					out object score))
				{
					if (score is null)
						structuredOsuMemoryReader.TryReadProperty(BaseAddresses.ResultsScreen, nameof(Player.Score),
							out score);
					gamePlay.Score = (int)score;
				}
				if (structuredOsuMemoryReader.TryReadProperty(BaseAddresses.ResultsScreen, nameof(Player.Mods),
					out object mods))
					gamePlay.Mods = ((Enums.Mods)((Mods)mods).Value);
				
				gamePlay.GameMode = (RuleSet)memoryReader.ReadInt(BaseAddresses.ResultsScreen, nameof(Player.Mode));

				PPCalculator calculator = PpCalculatorHelpers.GetPpCalculator(gamePlay.GameMode);

				gamePlay.Accuracy = 
					GetAccuracy(gamePlay);


				performance.Performance = calculator.Calculate(
					currentBeatMap, 
					gamePlay.Accuracy / 100, 
					gamePlay.MaxCombo,
					gamePlay.CountMiss, 
					gamePlay.Count50, 
					string.Join(",",Enum.GetValues(typeof(Enums.Mods)).Cast<Enum>().Where(value => gamePlay.Mods.HasFlag(value))).Split(","), 
					gamePlay.Score);
				gamePlay.PerformancePoints = performance;
				general.ResultScreen = gamePlay;
			}

			return JsonSerializer.Serialize(general);
		}
	}
}