using System;
using tools.Dev;

namespace tools.Plugins
{
    [PluginInfo("English", "1.0", "Coolexa")]
    public class PriceCheck_EN : IPlugin
    {
        public PluginInfoAttribute PluginInfo { get; set; }

        private bool FilterClipValue(string ClipboardText)
        {
            return (ClipboardText.Length > 10 && ClipboardText.StartsWith("Rarity: "));
        }
        public TextResult ProcessClipValue(string ClipboardText, bool NameOnly)
        {
            if (!FilterClipValue(ClipboardText))
            {
                return null;
            }

            #region 对POE物品文本进行分类

            string[] clipboardTextArr = ClipboardText.Split(new string[] { "--------\r\n" }, StringSplitOptions.None);
            if (clipboardTextArr.Length < 2)
                return null;
            string[] clipboardTextArr_0 = clipboardTextArr[0].Split(new string[] { "\r\n" }, StringSplitOptions.None);
            if (clipboardTextArr.Length < 2)
                return null;

            string itemRarity = clipboardTextArr_0[0].Replace("Rarity: ", "");
            string itemName = clipboardTextArr_0[1];
            string itemBaseType = clipboardTextArr_0.Length > 2 ? clipboardTextArr_0[2] : string.Empty;
            if (itemRarity != "Unique" && itemBaseType.Contains(" Map"))
            {
                itemName = itemBaseType;
            }

            if (NameOnly)
                return new TextResult() { textTip = string.Empty, textClip = itemName };


            Int32 itemSize = 0;
            Int32 mapTier = 0;
            if (clipboardTextArr[1].StartsWith("Stack Size: "))
            {
                Int32.TryParse(Helper.GetBetweenText(clipboardTextArr[1], "Stack Size: ", "/").Replace(",", ""), out itemSize);
            }
            else if (clipboardTextArr[1].StartsWith("Map Tier: "))
            {
                Int32.TryParse(Helper.GetBetweenText(clipboardTextArr[1], "Map Tier: ", "\r\n"), out mapTier);
            }


            ItemInfo itemInfo = new ItemInfo() { Name = itemName };

            switch (itemRarity)
            {
                case "Currency":
                    {
                        itemInfo.Flag = ItemFlag.Currency;
                        itemInfo.Size = itemSize;
                        if (itemInfo.Name.Contains("Essence"))
                            itemInfo.Flag |= ItemFlag.Essence;
                        else if (itemInfo.Name.Contains("Breachstone"))
                            itemInfo.Flag |= ItemFlag.Breachstone;
                    }
                    break;
                case "Divination Card":
                    {
                        itemInfo.Flag = ItemFlag.Divination;
                        itemInfo.Size = itemSize;
                    }
                    break;
                case "Unique":
                    {
                        itemInfo.Flag = ItemFlag.Unique;

                        if (ClipboardText.Contains("\r\nUnidentified"))
                        {
                            itemInfo.Unidentified = true;
                        }

                        if (itemBaseType.Contains(" Map"))
                        {
                            itemInfo.Flag |= ItemFlag.Map;
                            itemInfo.MapTier = mapTier;
                        }
                        else if (itemBaseType.Contains(" Jewel"))
                        {
                            itemInfo.Flag |= ItemFlag.Jewels;
                        }
                        else if (itemBaseType.Contains(" Flask"))
                        {
                            itemInfo.Flag |= ItemFlag.Flasks;
                        }
                        else if (itemBaseType.Contains(" Amulet"))
                        {
                            itemInfo.Flag |= ItemFlag.Accessories;
                        }
                        else if (itemBaseType.Contains(" Ring"))
                        {
                            itemInfo.Flag |= ItemFlag.Accessories;
                        }
                        else if (itemBaseType.Contains(" Sash") || itemBaseType.Contains(" Vise") || itemBaseType.Contains(" Belt"))
                        {
                            itemInfo.Flag |= ItemFlag.Accessories;
                        }
                        else if (ClipboardText.Contains("Attacks per Second:"))
                        {
                            itemInfo.Flag |= ItemFlag.Weapons;
                        }

                    }
                    break;
                /*
                case "Gem":
                    break;
                */
                case "Normal":
                    {
                        itemInfo.Flag = ItemFlag.Normal;
                        if (itemName.Contains(" Map"))
                        {
                            itemInfo.Flag |= ItemFlag.Map;
                            itemInfo.MapTier = mapTier;
                        }
                        else if (itemName == "Offering to the Goddess" || itemName == "Divine Vessel" || itemName.StartsWith("Sacrifice at ") || itemName.StartsWith("Mortal ") || itemName.StartsWith("Fragment of") || itemName.EndsWith("s Key"))
                        {
                            itemInfo.Flag |= ItemFlag.Fragments;
                        }
                        else if (ClipboardText.Contains("add this prophecy to"))
                        {
                            itemInfo.Flag |= ItemFlag.Prophecies;
                        }
                    }
                    break;
                case "Magic":
                    {
                        if (itemName.Contains(" Map"))
                        {
                            itemInfo.Flag |= ItemFlag.Map;
                            itemInfo.MapTier = mapTier;
                        }
                    }
                    break;
                case "Rare":
                    {
                        if (itemName.Contains(" Map"))
                        {
                            itemInfo.Flag |= ItemFlag.Map;
                            itemInfo.MapTier = mapTier;
                        }
                    }
                    break;
                default:
                    return null;
            }
            #endregion

            string textTip = ItemQuery(itemInfo);

            return new TextResult() { textTip = textTip, textClip = itemName }; 
        }
        
        private string ItemQuery(ItemInfo itemInfo)
        {
            string _text = string.Empty;

            if (string.IsNullOrEmpty(itemInfo.Name) || itemInfo.Flag == 0 || !Api.DownloadDone)
            {
                return _text;
            }

            #region Non-Unique Maps
            if (!itemInfo.Flag.HasFlag(ItemFlag.Unique) && itemInfo.Flag.HasFlag(ItemFlag.Map))
            {
                var _item = Api.Ninja.WhiteMaps.Lines.Find(x => x.MapTier == itemInfo.MapTier && x.Name == itemInfo.Name);// && x.variant == "Atlas");
                if (_item != null && _item.ChaosValue > 0)
                    _text = $"Normal || Chaos: {_item.ChaosValue} || Changes last 7 days: {_item.Sparkline.TotalChange}%";
            }
            #endregion
            #region Unique Maps
            else if (itemInfo.Flag.HasFlag(ItemFlag.Unique) && itemInfo.Flag.HasFlag(ItemFlag.Map))
            {
                var _item = Api.Ninja.UniqueMaps.Lines.Find(x => x.Name == itemInfo.Name);
                if (_item != null && _item.ChaosValue > 0)
                    _text = $"Chaos: {_item.ChaosValue} || Changes last 7 days: {_item.Sparkline.TotalChange}%";
            }
            #endregion
            #region Divination Cards
            else if (itemInfo.Flag.HasFlag(ItemFlag.Divination))
            {
                var _item = Api.Ninja.DivinationCards.Lines.Find(x => x.Name == itemInfo.Name);
                if (_item != null && _item.ChaosValue > 0)
                {
                    _text = $"Chaos: {_item.ChaosValue} || Changes last 7 days: {_item.Sparkline.TotalChange}%";
                    if (itemInfo.Size > 1)
                    {
                        _text += $"\nTotal: {itemInfo.Size * _item.ChaosValue} Chaos";
                    }
                }
            }
            #endregion
            #region Fragments
            else if (itemInfo.Flag.HasFlag(ItemFlag.Normal) && itemInfo.Flag.HasFlag(ItemFlag.Fragments))
            {
                var _item = Api.Ninja.Fragments.Lines.Find(x => x.CurrencyTypeName == itemInfo.Name);
                if (_item != null && _item.ChaosEquivalent > 0)
                    _text = $"Chaos: {_item.ChaosEquivalent} || Changes last 7 days: {_item.ReceiveSparkLine.TotalChange}%";
            }
            #endregion
            #region Unique Accessories
            else if (itemInfo.Flag.HasFlag(ItemFlag.Unique) && itemInfo.Flag.HasFlag(ItemFlag.Accessories) && !itemInfo.Unidentified)
            {
                string fixItemName = (itemInfo.Name == "Tasalio’s Sign") ? "Tasalio's Sign" : itemInfo.Name;

                var _item = Api.Ninja.UniqueAccessories.Lines.Find(x => x.Name == fixItemName);
                if (_item != null && _item.ChaosValue > 0)
                    _text = $"Chaos: {_item.ChaosValue} || Changes last 7 days: {_item.Sparkline.TotalChange}%";
            }
            #endregion
            #region Unique Flasks
            else if (itemInfo.Flag.HasFlag(ItemFlag.Unique) && itemInfo.Flag.HasFlag(ItemFlag.Flasks) && !itemInfo.Unidentified)
            {
                if (itemInfo.Name == "Vessel of Vinktar")
                {//电水多版本处理:
                    string _temp;
                    var _item = Api.Ninja.UniqueFlasks.Lines.Find(x => x.Name == itemInfo.Name && x.Variant == "Added Attacks");
                    if (_item != null && _item.ChaosValue > 0)
                    {
                        _temp = $"Added Attacks || Chaos: {_item.ChaosValue} || Changes last 7 days: {_item.Sparkline.TotalChange}%";
                        _text = string.IsNullOrEmpty(_text) ? _temp : _text + Environment.NewLine + _temp;
                    }
                    _item = Api.Ninja.UniqueFlasks.Lines.Find(x => x.Name == itemInfo.Name && x.Variant == "Penetration");
                    if (_item != null && _item.ChaosValue > 0)
                    {
                        _temp = $"Penetration || Chaos: {_item.ChaosValue} || Changes last 7 days: {_item.Sparkline.TotalChange}%";
                        _text = string.IsNullOrEmpty(_text) ? _temp : _text + Environment.NewLine + _temp;
                    }
                    _item = Api.Ninja.UniqueFlasks.Lines.Find(x => x.Name == itemInfo.Name && x.Variant == "Added Spells");
                    if (_item != null && _item.ChaosValue > 0)
                    {
                        _temp = $"Added Spells || Chaos: {_item.ChaosValue} || Changes last 7 days: {_item.Sparkline.TotalChange}%";
                        _text = string.IsNullOrEmpty(_text) ? _temp : _text + Environment.NewLine + _temp;
                    }
                    _item = Api.Ninja.UniqueFlasks.Lines.Find(x => x.Name == itemInfo.Name && x.Variant == "Conversion");
                    if (_item != null && _item.ChaosValue > 0)
                    {
                        _temp = $"Conversion || Chaos: {_item.ChaosValue} || Changes last 7 days: {_item.Sparkline.TotalChange}%";
                        _text = string.IsNullOrEmpty(_text) ? _temp : _text + Environment.NewLine + _temp;
                    }
                }
                else
                {
                    var _item = Api.Ninja.UniqueFlasks.Lines.Find(x => x.Name == itemInfo.Name);
                    if (_item != null && _item.ChaosValue > 0)
                        _text = $"Chaos: {_item.ChaosValue} || Changes last 7 days: {_item.Sparkline.TotalChange}%";
                }
            }
            #endregion
            #region Unique Jewels
            else if (itemInfo.Flag.HasFlag(ItemFlag.Unique) && itemInfo.Flag.HasFlag(ItemFlag.Jewels) && !itemInfo.Unidentified)
            {
                string fixItemName = (itemInfo.Name == "Bulwark Legion") ? "Fortified Legion" : itemInfo.Name;
                var _item = Api.Ninja.UniqueJewels.Lines.Find(x => x.Name == fixItemName);
                if (_item != null && _item.ChaosValue > 0)
                {
                    _text = $"Chaos: {_item.ChaosValue} || Changes last 7 days: {_item.Sparkline.TotalChange}%";
                }
            }
            #endregion
            #region Unique Weapons
            else if (itemInfo.Flag.HasFlag(ItemFlag.Unique) && itemInfo.Flag.HasFlag(ItemFlag.Weapons) && !itemInfo.Unidentified)
            {
                string _temp;
                var _item = Api.Ninja.UniqueWeapons.Lines.Find(x => x.Name == itemInfo.Name && x.Links == 0);
                if (_item != null && _item.ChaosValue > 0)
                {
                    _temp = $"0L || Chaos: {_item.ChaosValue} || Changes last 7 days: {_item.Sparkline.TotalChange}%";
                    _text = string.IsNullOrEmpty(_text) ? _temp : _text + Environment.NewLine + _temp;
                }
                _item = Api.Ninja.UniqueWeapons.Lines.Find(x => x.Name == itemInfo.Name && x.Links == 5);
                if (_item != null && _item.ChaosValue > 0)
                {
                    _temp = $"5L || Chaos: {_item.ChaosValue} || Changes last 7 days: {_item.Sparkline.TotalChange}%";
                    _text = string.IsNullOrEmpty(_text) ? _temp : _text + Environment.NewLine + _temp;
                }
                _item = Api.Ninja.UniqueWeapons.Lines.Find(x => x.Name == itemInfo.Name && x.Links == 6);
                if (_item != null && _item.ChaosValue > 0)
                {
                    _temp = $"6L || Chaos: {_item.ChaosValue} || Changes last 7 days: {_item.Sparkline.TotalChange}%";
                    _text = string.IsNullOrEmpty(_text) ? _temp : _text + Environment.NewLine + _temp;
                }
            }
            #endregion
            #region Breachstone
            else if (itemInfo.Flag.HasFlag(ItemFlag.Breachstone))
            {
                var _item = Api.Ninja.Fragments.Lines.Find(x => x.CurrencyTypeName == itemInfo.Name);
                if (_item != null && _item.ChaosEquivalent > 0)
                    _text = $"Chaos: {_item.ChaosEquivalent} || Changes last 7 days: {_item.ReceiveSparkLine.TotalChange}%";
            }
            #endregion
            #region Prophecies
            else if (itemInfo.Flag.HasFlag(ItemFlag.Prophecies))
            {
                var _item = Api.Ninja.Prophecies.Lines.Find(x => x.Name == itemInfo.Name);
                if (_item != null && _item.ChaosValue > 0)
                    _text = $"Chaos: {_item.ChaosValue} || Changes last 7 days: {_item.Sparkline.TotalChange}%";
            }
            #endregion
            #region Essence
            else if (itemInfo.Flag.HasFlag(ItemFlag.Essence) || itemInfo.Name.Contains("Remnant of"))
            {
                var _item = Api.Ninja.Essences.Lines.Find(x => x.Name == itemInfo.Name);
                if (_item != null && _item.ChaosValue > 0)
                {
                    _text = $"Chaos: {_item.ChaosValue} || Changes last 7 days: {_item.Sparkline.TotalChange}%";
                    if (itemInfo.Size > 1)
                    {
                        _text += $"\nTotal: {itemInfo.Size * _item.ChaosValue} Chaos";
                    }
                }
            }
            #endregion
            #region Shards //TODO:
            else if (itemInfo.Flag.HasFlag(ItemFlag.Currency) && itemInfo.Name.Contains("Shard") && !itemInfo.Name.Contains("Wisdom") && itemInfo.Name != "Prophecy")
            {
            }
            #endregion
            #region Currency
            else if (itemInfo.Flag.HasFlag(ItemFlag.Currency) && itemInfo.Name != "Chaos" && itemInfo.Name != "Chaos Orb" && !itemInfo.Name.Contains("Wisdom") && itemInfo.Name != "Prophecy")
            {
                var _item = Api.Ninja.Currency.Lines.Find(x => x.CurrencyTypeName == itemInfo.Name);
                if (_item != null && _item.ChaosEquivalent > 0)
                {
                    _text = $"Chaos: {_item.ChaosEquivalent} || Changes last 7 days: {_item.ReceiveSparkLine.TotalChange}%";
                    if (itemInfo.Size > 1)
                    {
                        _text += $"\nTotal: {itemInfo.Size * _item.ChaosEquivalent} Chaos";
                    }
                }
            }
            #endregion
            #region Unique Armours
            else if (itemInfo.Flag.HasFlag(ItemFlag.Unique) && !itemInfo.Unidentified)
            {
                string fixItemName = (itemInfo.Name == "Ondar's Flight") ? "Victario's Flight" : itemInfo.Name;
                string _temp;

                var _item = Api.Ninja.UniqueArmours.Lines.Find(x => x.Name == fixItemName && x.Links == 0);
                if (_item != null && _item.ChaosValue > 0)
                {
                    _temp = $"0L || Chaos: {_item.ChaosValue} || Changes last 7 days: {_item.Sparkline.TotalChange}%";
                    _text = string.IsNullOrEmpty(_text) ? _temp : _text + Environment.NewLine + _temp;
                }
                _item = Api.Ninja.UniqueArmours.Lines.Find(x => x.Name == fixItemName && x.Links == 5);
                if (_item != null && _item.ChaosValue > 0)
                {
                    _temp = $"5L || Chaos: {_item.ChaosValue} || Changes last 7 days: {_item.Sparkline.TotalChange}%";
                    _text = string.IsNullOrEmpty(_text) ? _temp : _text + Environment.NewLine + _temp;
                }
                _item = Api.Ninja.UniqueArmours.Lines.Find(x => x.Name == fixItemName && x.Links == 6);
                if (_item != null && _item.ChaosValue > 0)
                {
                    _temp = $"6L || Chaos: {_item.ChaosValue} || Changes last 7 days: {_item.Sparkline.TotalChange}%";
                    _text = string.IsNullOrEmpty(_text) ? _temp : _text + Environment.NewLine + _temp;
                }
            }
            #endregion

            return _text;
        }

    }
}
