using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using EquipmentManager.CustomWidgets;
using UnityEngine;
using Verse;

namespace EquipmentManager.Windows
{
    internal class ImportLoadoutsDialog : Window
    {
        private static EquipmentManagerGameComponent _equipmentManager;
        private readonly List<Loadout> _loadouts = new List<Loadout>();
        private readonly List<MeleeWeaponRule> _meleeWeaponRules = new List<MeleeWeaponRule>();
        private readonly List<RangedWeaponRule> _rangedWeaponRules = new List<RangedWeaponRule>();
        private readonly Dictionary<string, string> _savedGames = new Dictionary<string, string>();
        private readonly List<ToolRule> _toolRules = new List<ToolRule>();
        private readonly List<WorkTypeRule> _workTypeRules = new List<WorkTypeRule>();
        private Vector2 _loadoutsListScrollPosition;
        private Vector2 _savedGamesListScrollPosition;
        private string _selectedSaveGame;

        public ImportLoadoutsDialog()
        {
            forcePause = true;
            doCloseX = true;
            doCloseButton = false;
            closeOnClickedOutside = true;
            absorbInputAroundWindow = true;
        }

        private static EquipmentManagerGameComponent EquipmentManager =>
            _equipmentManager ?? (_equipmentManager = Current.Game.GetComponent<EquipmentManagerGameComponent>());

        public override Vector2 InitialSize => new Vector2(1000f, 500f);

        private void DoButtonRow(Rect rect)
        {
            var importButtonRect = new Rect(rect.center.x - UiHelpers.ActionButtonWidth - UiHelpers.ButtonGap, rect.y,
                UiHelpers.ActionButtonWidth, UiHelpers.ButtonHeight);
            if (Widgets.ButtonText(importButtonRect, Resources.Strings.Loadouts.ImportData,
                    active: _selectedSaveGame != null && _loadouts.Any()))
            {
                ImportSaveGameData();
                Close();
            }
            var cancelImportButtonRect = new Rect(rect.center.x + UiHelpers.ButtonGap, rect.y,
                UiHelpers.ActionButtonWidth, UiHelpers.ButtonHeight);
            if (Widgets.ButtonText(cancelImportButtonRect, Resources.Strings.Loadouts.CancelDataImport)) { Close(); }
        }

        private void DoLoadoutList(Rect rect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, Text.LineHeight),
                Resources.Strings.Loadouts.LoadoutListHeader);
            Text.Font = GameFont.Small;
            var listingRect = new Rect(rect.x, rect.y + Text.LineHeightOf(GameFont.Medium) + UiHelpers.ElementGap,
                rect.width, rect.height - Text.LineHeightOf(GameFont.Medium) - (UiHelpers.ElementGap * 2));
            Widgets.DrawBoxSolidWithOutline(listingRect, new Color(1f, 1f, 1f, 0.05f), new Color(1f, 1f, 1f, 0.4f));
            var listing = new Listing_Standard(listingRect, () => _loadoutsListScrollPosition);
            var viewRect = new Rect(rect.x, rect.y, rect.width, _loadouts.Count * UiHelpers.ListRowHeight);
            Widgets.BeginScrollView(listingRect, ref _loadoutsListScrollPosition, viewRect);
            listing.Begin(viewRect);
            Text.Anchor = TextAnchor.MiddleLeft;
            foreach (var loadout in _loadouts)
            {
                Widgets.Label(listing.GetRect(UiHelpers.ListRowHeight).ContractedBy(4f), loadout.Label);
            }
            Text.Anchor = TextAnchor.UpperLeft;
            listing.End();
            Widgets.EndScrollView();
        }

        private void DoSavedGamesList(Rect rect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, Text.LineHeight),
                Resources.Strings.Loadouts.SavedGamesListHeader);
            Text.Font = GameFont.Small;
            var listingRect = new Rect(rect.x, rect.y + Text.LineHeightOf(GameFont.Medium) + UiHelpers.ElementGap,
                rect.width, rect.height - Text.LineHeightOf(GameFont.Medium) - (UiHelpers.ElementGap * 2));
            Widgets.DrawBoxSolidWithOutline(listingRect, new Color(1f, 1f, 1f, 0.05f), new Color(1f, 1f, 1f, 0.4f));
            var listing = new Listing_Standard(listingRect, () => _savedGamesListScrollPosition);
            var viewRect = new Rect(rect.x, rect.y, rect.width, _savedGames.Count * UiHelpers.ListRowHeight);
            Widgets.BeginScrollView(listingRect, ref _savedGamesListScrollPosition, viewRect);
            listing.Begin(viewRect);
            Text.Anchor = TextAnchor.MiddleLeft;
            foreach (var savedGame in _savedGames)
            {
                var rowRect = listing.GetRect(UiHelpers.ListRowHeight);
                var toggleButtonRect = new Rect(rowRect.x,
                    rowRect.y + ((UiHelpers.ListRowHeight - Math.Min(32f, UiHelpers.ListRowHeight)) / 2f),
                    Math.Min(32f, UiHelpers.ListRowHeight), Math.Min(32f, UiHelpers.ListRowHeight)).ContractedBy(4f);
                ButtonImageToggle.DoButtonImageToggle(() => savedGame.Key == _selectedSaveGame, newValue =>
                {
                    if (!newValue) { return; }
                    _selectedSaveGame = savedGame.Key;
                    ReadSaveGameData(savedGame.Key);
                }, toggleButtonRect, Widgets.CheckboxOnTex, Widgets.CheckboxOffTex);
                var nameRectX = toggleButtonRect.x + toggleButtonRect.width + 4f;
                Widgets.Label(new Rect(nameRectX, rowRect.y, rowRect.xMax - nameRectX, rowRect.height).ContractedBy(4f),
                    savedGame.Value);
            }
            Text.Anchor = TextAnchor.UpperLeft;
            listing.End();
            Widgets.EndScrollView();
        }

        public override void DoWindowContents(Rect inRect)
        {
            const int columnCount = 2;
            var columnWidth = (inRect.width - (UiHelpers.ElementGap * 2) - (UiHelpers.ElementGap * (columnCount - 1))) /
                columnCount;
            var columnHeight = inRect.height - (UiHelpers.ElementGap * 2) -
                (UiHelpers.ButtonHeight + (UiHelpers.ButtonGap * 2));
            var savedGamesRect = new Rect(inRect.x + UiHelpers.ElementGap, inRect.y + UiHelpers.ElementGap, columnWidth,
                columnHeight);
            DoSavedGamesList(savedGamesRect);
            var loadoutsRect = new Rect(savedGamesRect.xMax + UiHelpers.ElementGap, inRect.y + UiHelpers.ElementGap,
                columnWidth, columnHeight);
            DoLoadoutList(loadoutsRect);
            var actionButtonsRect = new Rect(inRect.x, inRect.yMax - UiHelpers.ButtonHeight - UiHelpers.ButtonGap,
                inRect.width, UiHelpers.ButtonHeight);
            DoButtonRow(actionButtonsRect);
        }

        private void ImportSaveGameData()
        {
            Find.WindowStack.WindowOfType<ManageWeaponRulesDialog>()?.Close();
            Find.WindowStack.WindowOfType<ManageLoadoutsDialog>()?.Close();
            foreach (var loadout in EquipmentManager.GetLoadouts().ToList())
            {
                EquipmentManager.DeleteLoadout(loadout);
            }
            foreach (var rule in EquipmentManager.GetMeleeWeaponRules().ToList())
            {
                EquipmentManager.DeleteMeleeWeaponRule(rule);
            }
            foreach (var rule in _meleeWeaponRules) { EquipmentManager.AddMeleeWeaponRule(rule); }
            foreach (var rule in EquipmentManager.GetRangedWeaponRules().ToList())
            {
                EquipmentManager.DeleteRangedWeaponRule(rule);
            }
            foreach (var rule in _rangedWeaponRules) { EquipmentManager.AddRangedWeaponRule(rule); }
            foreach (var rule in EquipmentManager.GetToolRules().ToList()) { EquipmentManager.DeleteToolRule(rule); }
            foreach (var rule in _toolRules) { EquipmentManager.AddToolRule(rule); }
            foreach (var rule in EquipmentManager.GetWorkTypeRules().ToList())
            {
                EquipmentManager.DeleteWorkTypeRule(rule);
            }
            foreach (var rule in _workTypeRules) { EquipmentManager.AddWorkTypeRule(rule); }
            foreach (var loadout in _loadouts) { EquipmentManager.AddLoadout(loadout); }
        }

        private void LoadSavedGames()
        {
            foreach (var file in GenFilePaths.AllSavedGameFiles.OrderByDescending(info => info.LastWriteTimeUtc))
            {
                try
                {
                    var xmlReader = XmlReader.Create(file.FullName,
                        new XmlReaderSettings
                        {
                            IgnoreWhitespace = true, IgnoreComments = true, IgnoreProcessingInstructions = true
                        });
                    if (xmlReader.ReadToFollowing("gameVersion"))
                    {
                        var xml = xmlReader.ReadInnerXml();
                        _savedGames.Add(file.FullName,
                            $"{file.Name} {xml.Substring(0, xml.FirstIndexOf(char.IsWhiteSpace))}");
                    }
                    xmlReader.Close();
                }
                catch { Log.Warning($"Equipment Manager: Could not process save game file {file.FullName}"); }
            }
        }

        public override void PostOpen()
        {
            base.PostOpen();
            LoadSavedGames();
        }

        private static Dictionary<string, string> ReadDictionary(XmlReader xmlReader)
        {
            var result = new Dictionary<string, string>();
            var keys = new List<string>();
            var values = new List<string>();
            if (xmlReader.Name != "keys" || xmlReader.NodeType != XmlNodeType.Element)
            {
                throw new ArgumentException("XmlReader was not on a 'keys' node while parsing a dictionary",
                    nameof(xmlReader));
            }
            if (xmlReader.IsEmptyElement) { _ = xmlReader.Read(); }
            else
            {
                _ = xmlReader.Read();
                while (xmlReader.Name == "li" && xmlReader.NodeType == XmlNodeType.Element)
                {
                    keys.Add(xmlReader.ReadElementContentAsString());
                }
                xmlReader.ReadEndElement();
            }
            if (xmlReader.Name != "values" || xmlReader.NodeType != XmlNodeType.Element)
            {
                throw new ArgumentException(
                    "XmlReader was not on a 'values' node after closing 'keys' node while parsing a dictionary",
                    nameof(xmlReader));
            }
            if (xmlReader.IsEmptyElement) { _ = xmlReader.Read(); }
            else
            {
                _ = xmlReader.Read();
                while (xmlReader.Name == "li" && xmlReader.NodeType == XmlNodeType.Element)
                {
                    values.Add(xmlReader.ReadElementContentAsString());
                }
                xmlReader.ReadEndElement();
            }
            for (var i = 0; i < keys.Count; i++) { result.Add(keys[i], values[i]); }
            return result;
        }

        private void ReadLoadoutData(XmlReader xmlReader)
        {
            if (!xmlReader.ReadToFollowing("li") || !xmlReader.Read()) { return; }
            var id = 0;
            var label = string.Empty;
            var isProtected = false;
            var priority = 0;
            var primaryRuleType = Loadout.PrimaryWeaponType.None;
            int? primaryRangedWeaponRuleId = null;
            int? primaryMeleeWeaponRuleId = null;
            List<int> rangedSidearmRules = null;
            List<int> meleeSidearmRules = null;
            int? toolRuleId = null;
            Dictionary<string, bool> pawnTraits = null;
            Dictionary<string, bool> pawnCapacities = null;
            HashSet<string> preferredSkills = null;
            HashSet<string> undesirableSkills = null;
            var dropUnassignedWeapons = true;
            while (true)
            {
                if (xmlReader.NodeType != XmlNodeType.Element || xmlReader.IsEmptyElement)
                {
                    if (!xmlReader.Read()) { break; }
                    continue;
                }
                switch (xmlReader.Name)
                {
                    case "Id":
                        id = xmlReader.ReadElementContentAsInt();
                        break;
                    case "Label":
                        label = xmlReader.ReadElementContentAsString();
                        break;
                    case "Protected":
                        isProtected = bool.Parse(xmlReader.ReadElementContentAsString());
                        break;
                    case "Priority":
                        priority = xmlReader.ReadElementContentAsInt();
                        break;
                    case "PrimaryRuleType":
                        _ = Enum.TryParse(xmlReader.ReadElementContentAsString(), out primaryRuleType);
                        break;
                    case "PrimaryRangedWeaponRuleId":
                        primaryRangedWeaponRuleId = xmlReader.ReadElementContentAsInt();
                        break;
                    case "PrimaryMeleeWeaponRuleId":
                        primaryMeleeWeaponRuleId = xmlReader.ReadElementContentAsInt();
                        break;
                    case "RangedSidearmRules":
                        _ = xmlReader.Read();
                        rangedSidearmRules = new List<int>();
                        while (xmlReader.Name == "li" && xmlReader.NodeType == XmlNodeType.Element)
                        {
                            rangedSidearmRules.Add(xmlReader.ReadElementContentAsInt());
                        }
                        xmlReader.ReadEndElement();
                        break;
                    case "MeleeSidearmRules":
                        _ = xmlReader.Read();
                        meleeSidearmRules = new List<int>();
                        while (xmlReader.Name == "li" && xmlReader.NodeType == XmlNodeType.Element)
                        {
                            meleeSidearmRules.Add(xmlReader.ReadElementContentAsInt());
                        }
                        xmlReader.ReadEndElement();
                        break;
                    case "ToolRuleId":
                        toolRuleId = xmlReader.ReadElementContentAsInt();
                        break;
                    case "PawnTraits":
                        _ = xmlReader.Read();
                        pawnTraits = ReadDictionary(xmlReader)
                            .ToDictionary(pair => pair.Key, pair => bool.Parse(pair.Value));
                        xmlReader.ReadEndElement();
                        break;
                    case "PawnCapacities":
                        _ = xmlReader.Read();
                        pawnCapacities = ReadDictionary(xmlReader)
                            .ToDictionary(pair => pair.Key, pair => bool.Parse(pair.Value));
                        xmlReader.ReadEndElement();
                        break;
                    case "PreferredSkills":
                        _ = xmlReader.Read();
                        preferredSkills = new HashSet<string>();
                        while (xmlReader.Name == "li" && xmlReader.NodeType == XmlNodeType.Element)
                        {
                            _ = preferredSkills.Add(xmlReader.ReadElementContentAsString());
                        }
                        xmlReader.ReadEndElement();
                        break;
                    case "UndesirableSkills":
                        _ = xmlReader.Read();
                        undesirableSkills = new HashSet<string>();
                        while (xmlReader.Name == "li" && xmlReader.NodeType == XmlNodeType.Element)
                        {
                            _ = undesirableSkills.Add(xmlReader.ReadElementContentAsString());
                        }
                        xmlReader.ReadEndElement();
                        break;
                    case "DropUnassignedWeapons":
                        dropUnassignedWeapons = bool.Parse(xmlReader.ReadElementContentAsString());
                        break;
                    default:
                        Log.Warning($"Equipment Manager: Unknown Loadout property '{xmlReader.Name}'");
                        break;
                }
            }
            _loadouts.Add(new Loadout(id, label, isProtected, priority, primaryRuleType, primaryRangedWeaponRuleId,
                primaryMeleeWeaponRuleId, rangedSidearmRules, meleeSidearmRules, toolRuleId, pawnTraits, pawnCapacities,
                preferredSkills, undesirableSkills, dropUnassignedWeapons));
        }

        private void ReadLoadoutsData(string savedGameFile, XmlReader xmlReader)
        {
            if (xmlReader.ReadToFollowing("Loadouts"))
            {
                var node = xmlReader.ReadSubtree();
                if (node.ReadToDescendant("li"))
                {
                    do { ReadLoadoutData(node.ReadSubtree()); } while (node.ReadToNextSibling("li"));
                }
                else
                {
                    Log.Error(
                        $"Equipment Manager: Could not find any 'li' nodes inside the 'Loadouts' node in the save game file {savedGameFile}");
                }
            }
            else
            {
                Log.Error($"Equipment Manager: Could not find 'Loadouts' node in the save game file {savedGameFile}");
            }
        }

        private void ReadMeleeWeaponRuleData(XmlReader xmlReader)
        {
            if (!xmlReader.ReadToFollowing("li") || !xmlReader.Read()) { return; }
            var id = 0;
            var label = string.Empty;
            var isProtected = false;
            List<StatWeight> statWeights = null;
            List<StatLimit> statLimits = null;
            HashSet<string> whitelistedItemsDefNames = null;
            HashSet<string> blacklistedItemsDefNames = null;
            var equipMode = ItemRule.WeaponEquipMode.BestOne;
            bool? usableWithShields = null;
            bool? rottable = null;
            while (true)
            {
                if (xmlReader.NodeType != XmlNodeType.Element || xmlReader.IsEmptyElement)
                {
                    if (!xmlReader.Read()) { break; }
                    continue;
                }
                switch (xmlReader.Name)
                {
                    case "Id":
                        id = xmlReader.ReadElementContentAsInt();
                        break;
                    case "Label":
                        label = xmlReader.ReadElementContentAsString();
                        break;
                    case "Protected":
                        isProtected = bool.Parse(xmlReader.ReadElementContentAsString());
                        break;
                    case "StatWeights":
                        _ = xmlReader.Read();
                        statWeights = new List<StatWeight>();
                        while (xmlReader.Name == "li" && xmlReader.NodeType == XmlNodeType.Element)
                        {
                            _ = xmlReader.Read();
                            statWeights.Add(ReadStatWeightData(xmlReader));
                            xmlReader.ReadEndElement();
                        }
                        xmlReader.ReadEndElement();
                        break;
                    case "StatLimits":
                        _ = xmlReader.Read();
                        statLimits = new List<StatLimit>();
                        while (xmlReader.Name == "li" && xmlReader.NodeType == XmlNodeType.Element)
                        {
                            _ = xmlReader.Read();
                            statLimits.Add(ReadStatLimitData(xmlReader));
                            xmlReader.ReadEndElement();
                        }
                        xmlReader.ReadEndElement();
                        break;
                    case "WhitelistedItemsDefNames":
                        _ = xmlReader.Read();
                        whitelistedItemsDefNames = new HashSet<string>();
                        while (xmlReader.Name == "li" && xmlReader.NodeType == XmlNodeType.Element)
                        {
                            _ = whitelistedItemsDefNames.Add(xmlReader.ReadElementContentAsString());
                        }
                        xmlReader.ReadEndElement();
                        break;
                    case "BlacklistedItemsDefNames":
                        _ = xmlReader.Read();
                        blacklistedItemsDefNames = new HashSet<string>();
                        while (xmlReader.Name == "li" && xmlReader.NodeType == XmlNodeType.Element)
                        {
                            _ = blacklistedItemsDefNames.Add(xmlReader.ReadElementContentAsString());
                        }
                        xmlReader.ReadEndElement();
                        break;
                    case "EquipMode":
                        _ = Enum.TryParse(xmlReader.ReadElementContentAsString(), out equipMode);
                        break;
                    case "UsableWithShields":
                        usableWithShields = bool.Parse(xmlReader.ReadElementContentAsString());
                        break;
                    case "Rottable":
                        rottable = bool.Parse(xmlReader.ReadElementContentAsString());
                        break;
                    default:
                        Log.Warning($"Equipment Manager: Unknown MeleeWeaponRule property '{xmlReader.Name}'");
                        break;
                }
            }
            _meleeWeaponRules.Add(new MeleeWeaponRule(id, label, isProtected, statWeights, statLimits,
                whitelistedItemsDefNames, blacklistedItemsDefNames, equipMode, usableWithShields, rottable));
        }

        private void ReadMeleeWeaponRulesData(string savedGameFile, XmlReader xmlReader)
        {
            if (xmlReader.ReadToFollowing("MeleeWeaponRules"))
            {
                var node = xmlReader.ReadSubtree();
                if (node.ReadToDescendant("li"))
                {
                    do { ReadMeleeWeaponRuleData(node.ReadSubtree()); } while (node.ReadToNextSibling("li"));
                }
                else
                {
                    Log.Error(
                        $"Equipment Manager: Could not find any 'li' nodes inside the 'MeleeWeaponRules' node in the save game file {savedGameFile}");
                }
            }
            else
            {
                Log.Error(
                    $"Equipment Manager: Could not find 'MeleeWeaponRules' node in the save game file {savedGameFile}");
            }
        }

        private void ReadRangedWeaponRuleData(XmlReader xmlReader)
        {
            if (!xmlReader.ReadToFollowing("li") || !xmlReader.Read()) { return; }
            var id = 0;
            var label = string.Empty;
            var isProtected = false;
            List<StatWeight> statWeights = null;
            List<StatLimit> statLimits = null;
            HashSet<string> whitelistedItemsDefNames = null;
            HashSet<string> blacklistedItemsDefNames = null;
            var equipMode = ItemRule.WeaponEquipMode.BestOne;
            bool? explosive = null;
            bool? manualCast = null;
            while (true)
            {
                if (xmlReader.NodeType != XmlNodeType.Element || xmlReader.IsEmptyElement)
                {
                    if (!xmlReader.Read()) { break; }
                    continue;
                }
                switch (xmlReader.Name)
                {
                    case "Id":
                        id = xmlReader.ReadElementContentAsInt();
                        break;
                    case "Label":
                        label = xmlReader.ReadElementContentAsString();
                        break;
                    case "Protected":
                        isProtected = bool.Parse(xmlReader.ReadElementContentAsString());
                        break;
                    case "StatWeights":
                        _ = xmlReader.Read();
                        statWeights = new List<StatWeight>();
                        while (xmlReader.Name == "li" && xmlReader.NodeType == XmlNodeType.Element)
                        {
                            _ = xmlReader.Read();
                            statWeights.Add(ReadStatWeightData(xmlReader));
                            xmlReader.ReadEndElement();
                        }
                        xmlReader.ReadEndElement();
                        break;
                    case "StatLimits":
                        _ = xmlReader.Read();
                        statLimits = new List<StatLimit>();
                        while (xmlReader.Name == "li" && xmlReader.NodeType == XmlNodeType.Element)
                        {
                            _ = xmlReader.Read();
                            statLimits.Add(ReadStatLimitData(xmlReader));
                            xmlReader.ReadEndElement();
                        }
                        xmlReader.ReadEndElement();
                        break;
                    case "WhitelistedItemsDefNames":
                        _ = xmlReader.Read();
                        whitelistedItemsDefNames = new HashSet<string>();
                        while (xmlReader.Name == "li" && xmlReader.NodeType == XmlNodeType.Element)
                        {
                            _ = whitelistedItemsDefNames.Add(xmlReader.ReadElementContentAsString());
                        }
                        xmlReader.ReadEndElement();
                        break;
                    case "BlacklistedItemsDefNames":
                        _ = xmlReader.Read();
                        blacklistedItemsDefNames = new HashSet<string>();
                        while (xmlReader.Name == "li" && xmlReader.NodeType == XmlNodeType.Element)
                        {
                            _ = blacklistedItemsDefNames.Add(xmlReader.ReadElementContentAsString());
                        }
                        xmlReader.ReadEndElement();
                        break;
                    case "EquipMode":
                        _ = Enum.TryParse(xmlReader.ReadElementContentAsString(), out equipMode);
                        break;
                    case "Explosive":
                        explosive = bool.Parse(xmlReader.ReadElementContentAsString());
                        break;
                    case "ManualCast":
                        manualCast = bool.Parse(xmlReader.ReadElementContentAsString());
                        break;
                    default:
                        Log.Warning($"Equipment Manager: Unknown RangedWeaponRule property '{xmlReader.Name}'");
                        break;
                }
            }
            _rangedWeaponRules.Add(new RangedWeaponRule(id, label, isProtected, statWeights, statLimits,
                whitelistedItemsDefNames, blacklistedItemsDefNames, equipMode, explosive, manualCast));
        }

        private void ReadRangedWeaponRulesData(string savedGameFile, XmlReader xmlReader)
        {
            if (xmlReader.ReadToFollowing("RangedWeaponRules"))
            {
                var node = xmlReader.ReadSubtree();
                if (node.ReadToDescendant("li"))
                {
                    do { ReadRangedWeaponRuleData(node.ReadSubtree()); } while (node.ReadToNextSibling("li"));
                }
                else
                {
                    Log.Error(
                        $"Equipment Manager: Could not find any 'li' nodes inside the 'RangedWeaponRules' node in the save game file {savedGameFile}");
                }
            }
            else
            {
                Log.Error(
                    $"Equipment Manager: Could not find 'RangedWeaponRules' node in the save game file {savedGameFile}");
            }
        }

        private void ReadSaveGameData(string savedGameFile)
        {
            _loadoutsListScrollPosition = Vector2.zero;
            _loadouts.Clear();
            _meleeWeaponRules.Clear();
            _rangedWeaponRules.Clear();
            _toolRules.Clear();
            _workTypeRules.Clear();
            try
            {
                var xmlReader = XmlReader.Create(savedGameFile,
                    new XmlReaderSettings
                    {
                        IgnoreWhitespace = true, IgnoreComments = true, IgnoreProcessingInstructions = true
                    });
                if (xmlReader.ReadToFollowing("savegame"))
                {
                    if (xmlReader.ReadToDescendant("game"))
                    {
                        if (xmlReader.ReadToDescendant("components"))
                        {
                            if (xmlReader.ReadToDescendant("li"))
                            {
                                do
                                {
                                    if (!xmlReader.HasAttributes || xmlReader.IsEmptyElement) { continue; }
                                    while (xmlReader.MoveToNextAttribute())
                                    {
                                        if (xmlReader.Name != "Class") { continue; }
                                        if (xmlReader.Value == typeof(EquipmentManagerGameComponent).FullName)
                                        {
                                            _ = xmlReader.MoveToElement();
                                            ReadLoadoutsData(savedGameFile, xmlReader);
                                            ReadMeleeWeaponRulesData(savedGameFile, xmlReader);
                                            ReadRangedWeaponRulesData(savedGameFile, xmlReader);
                                            ReadToolRulesData(savedGameFile, xmlReader);
                                            ReadWorkTypeRulesData(savedGameFile, xmlReader);
                                            xmlReader.Close();
                                            return;
                                        }
                                        _ = xmlReader.MoveToElement();
                                        break;
                                    }
                                } while (xmlReader.ReadToNextSibling("li"));
                            }
                        }
                        else
                        {
                            Log.Warning(
                                $"Equipment Manager: Could not find game components' data in the save game file {savedGameFile}");
                        }
                    }
                    else
                    {
                        Log.Error($"Equipment Manager: Could not find game data in the save game file {savedGameFile}");
                    }
                }
                else
                {
                    Log.Error($"Equipment Manager: Could not find root node in the save game file {savedGameFile}");
                }
                xmlReader.Close();
            }
            catch (Exception exception)
            {
                Log.Warning(
                    $"Equipment Manager: Could not process save game file {savedGameFile}{Environment.NewLine}{exception.Message}");
            }
        }

        private static StatLimit ReadStatLimitData(XmlReader xmlReader)
        {
            var statDefName = string.Empty;
            float? minValue = null;
            float? maxValue = null;
            var keepParsing = true;
            do
            {
                switch (xmlReader.Name)
                {
                    case "StatDefName":
                        statDefName = xmlReader.ReadElementContentAsString();
                        break;
                    case "MinValue":
                        minValue = xmlReader.ReadElementContentAsFloat();
                        break;
                    case "MaxValue":
                        maxValue = xmlReader.ReadElementContentAsFloat();
                        break;
                    default:
                        keepParsing = false;
                        break;
                }
            } while (keepParsing);
            return new StatLimit(statDefName, minValue, maxValue);
        }

        private static StatWeight ReadStatWeightData(XmlReader xmlReader)
        {
            var statDefName = string.Empty;
            var isProtected = false;
            var weight = 0f;
            var keepParsing = true;
            do
            {
                switch (xmlReader.Name)
                {
                    case "StatDefName":
                        statDefName = xmlReader.ReadElementContentAsString();
                        break;
                    case "Protected":
                        isProtected = bool.Parse(xmlReader.ReadElementContentAsString());
                        break;
                    case "Weight":
                        weight = xmlReader.ReadElementContentAsFloat();
                        break;
                    default:
                        keepParsing = false;
                        break;
                }
            } while (keepParsing);
            return new StatWeight(statDefName, weight, isProtected);
        }

        private void ReadToolRuleData(XmlReader xmlReader)
        {
            if (!xmlReader.ReadToFollowing("li") || !xmlReader.Read()) { return; }
            var id = 0;
            var label = string.Empty;
            var isProtected = false;
            List<StatWeight> statWeights = null;
            List<StatLimit> statLimits = null;
            HashSet<string> whitelistedItemsDefNames = null;
            HashSet<string> blacklistedItemsDefNames = null;
            var equipMode = ItemRule.ToolEquipMode.OneForEveryAssignedWorkType;
            bool? ranged = null;
            while (true)
            {
                if (xmlReader.NodeType != XmlNodeType.Element || xmlReader.IsEmptyElement)
                {
                    if (!xmlReader.Read()) { break; }
                    continue;
                }
                switch (xmlReader.Name)
                {
                    case "Id":
                        id = xmlReader.ReadElementContentAsInt();
                        break;
                    case "Label":
                        label = xmlReader.ReadElementContentAsString();
                        break;
                    case "Protected":
                        isProtected = bool.Parse(xmlReader.ReadElementContentAsString());
                        break;
                    case "StatWeights":
                        _ = xmlReader.Read();
                        statWeights = new List<StatWeight>();
                        while (xmlReader.Name == "li" && xmlReader.NodeType == XmlNodeType.Element)
                        {
                            _ = xmlReader.Read();
                            statWeights.Add(ReadStatWeightData(xmlReader));
                            xmlReader.ReadEndElement();
                        }
                        xmlReader.ReadEndElement();
                        break;
                    case "StatLimits":
                        _ = xmlReader.Read();
                        statLimits = new List<StatLimit>();
                        while (xmlReader.Name == "li" && xmlReader.NodeType == XmlNodeType.Element)
                        {
                            _ = xmlReader.Read();
                            statLimits.Add(ReadStatLimitData(xmlReader));
                            xmlReader.ReadEndElement();
                        }
                        xmlReader.ReadEndElement();
                        break;
                    case "WhitelistedItemsDefNames":
                        _ = xmlReader.Read();
                        whitelistedItemsDefNames = new HashSet<string>();
                        while (xmlReader.Name == "li" && xmlReader.NodeType == XmlNodeType.Element)
                        {
                            _ = whitelistedItemsDefNames.Add(xmlReader.ReadElementContentAsString());
                        }
                        xmlReader.ReadEndElement();
                        break;
                    case "BlacklistedItemsDefNames":
                        _ = xmlReader.Read();
                        blacklistedItemsDefNames = new HashSet<string>();
                        while (xmlReader.Name == "li" && xmlReader.NodeType == XmlNodeType.Element)
                        {
                            _ = blacklistedItemsDefNames.Add(xmlReader.ReadElementContentAsString());
                        }
                        xmlReader.ReadEndElement();
                        break;
                    case "EquipMode":
                        _ = Enum.TryParse(xmlReader.ReadElementContentAsString(), out equipMode);
                        break;
                    case "Ranged":
                        ranged = bool.Parse(xmlReader.ReadElementContentAsString());
                        break;
                    default:
                        Log.Warning($"Equipment Manager: Unknown ToolRule property '{xmlReader.Name}'");
                        break;
                }
            }
            _toolRules.Add(new ToolRule(id, label, isProtected, statWeights, statLimits, whitelistedItemsDefNames,
                blacklistedItemsDefNames, equipMode, ranged));
        }

        private void ReadToolRulesData(string savedGameFile, XmlReader xmlReader)
        {
            if (xmlReader.ReadToFollowing("ToolRules"))
            {
                var node = xmlReader.ReadSubtree();
                if (node.ReadToDescendant("li"))
                {
                    do { ReadToolRuleData(node.ReadSubtree()); } while (node.ReadToNextSibling("li"));
                }
                else
                {
                    Log.Error(
                        $"Equipment Manager: Could not find any 'li' nodes inside the 'ToolRules' node in the save game file {savedGameFile}");
                }
            }
            else
            {
                Log.Error($"Equipment Manager: Could not find 'ToolRules' node in the save game file {savedGameFile}");
            }
        }

        private void ReadWorkTypeRuleData(XmlReader xmlReader)
        {
            if (!xmlReader.ReadToFollowing("li") || !xmlReader.Read()) { return; }
            var workTypeDefName = string.Empty;
            List<StatWeight> statWeights = null;
            while (true)
            {
                if (xmlReader.NodeType != XmlNodeType.Element || xmlReader.IsEmptyElement)
                {
                    if (!xmlReader.Read()) { break; }
                    continue;
                }
                switch (xmlReader.Name)
                {
                    case "WorkTypeDefName":
                        workTypeDefName = xmlReader.ReadElementContentAsString();
                        break;
                    case "StatWeights":
                        _ = xmlReader.Read();
                        statWeights = new List<StatWeight>();
                        while (xmlReader.Name == "li" && xmlReader.NodeType == XmlNodeType.Element)
                        {
                            _ = xmlReader.Read();
                            statWeights.Add(ReadStatWeightData(xmlReader));
                            xmlReader.ReadEndElement();
                        }
                        xmlReader.ReadEndElement();
                        break;
                    default:
                        Log.Warning($"Equipment Manager: Unknown WorkTypeRule property '{xmlReader.Name}'");
                        break;
                }
            }
            _workTypeRules.Add(new WorkTypeRule(workTypeDefName, statWeights));
        }

        private void ReadWorkTypeRulesData(string savedGameFile, XmlReader xmlReader)
        {
            if (xmlReader.ReadToFollowing("WorkTypeRules"))
            {
                var node = xmlReader.ReadSubtree();
                if (node.ReadToDescendant("li"))
                {
                    do { ReadWorkTypeRuleData(node.ReadSubtree()); } while (node.ReadToNextSibling("li"));
                }
                else
                {
                    Log.Error(
                        $"Equipment Manager: Could not find any 'li' nodes inside the 'WorkTypeRules' node in the save game file {savedGameFile}");
                }
            }
            else
            {
                Log.Error(
                    $"Equipment Manager: Could not find 'WorkTypeRules' node in the save game file {savedGameFile}");
            }
        }
    }
}