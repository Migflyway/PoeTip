using System;
using tools.Dev;

namespace tools.Plugins
{
    [PluginInfo("繁體中文", "1.0", "Coolexa")]
    public class PriceCheck_TW : IPlugin
    {
        public PluginInfoAttribute PluginInfo { get; set; }

        private bool FilterClipValue(string ClipboardText)
        {
            return (ClipboardText.Length > 10 && ClipboardText.StartsWith("稀有度: "));
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
            {
                return null;
            }

            int startIndex = 0;
            string itemRarity = Helper.GetBetweenText(clipboardTextArr[0], "稀有度: ", "\r\n", ref startIndex);
            string itemName = Helper.GetBetweenText(clipboardTextArr[0], "(", ")", ref startIndex);
            string itemBaseType = Helper.GetBetweenText(clipboardTextArr[0], "(", ")", ref startIndex);
            if (itemRarity != "傳奇" && itemBaseType.Contains(" Map"))
            {
                itemName = itemBaseType;
            }

            if (NameOnly)
                return new TextResult() { textTip = string.Empty, textClip = itemName };


            Int32 itemSize = 0;
            Int32 mapTier = 0;
            if (clipboardTextArr[1].StartsWith("堆疊數: "))
            {
                Int32.TryParse(Helper.GetBetweenText(clipboardTextArr[1], "堆疊數: ", "/").Replace(",", ""), out itemSize);
            }
            else if (clipboardTextArr[1].StartsWith("地圖等階: "))
            {
                Int32.TryParse(Helper.GetBetweenText(clipboardTextArr[1], "地圖等階: ", "\r\n"), out mapTier);
            }

            ItemInfo itemInfo = new ItemInfo() { Name = itemName };

            switch (itemRarity)
            {
                case "通貨":
                    {
                        itemInfo.Size = itemSize;
                        itemInfo.Flag = ItemFlag.Currency;
                        if (itemInfo.Name.Contains("Essence"))
                            itemInfo.Flag |= ItemFlag.Essence;
                        else if (itemInfo.Name.Contains("Breachstone"))
                            itemInfo.Flag |= ItemFlag.Breachstone;
                    }
                    break;
                case "命運卡":
                    {
                        itemInfo.Size = itemSize;
                        itemInfo.Flag = ItemFlag.Divination;
                    }
                    break;
                case "暗金":
                case "傳奇":
                    {
                        itemInfo.Flag = ItemFlag.Unique;

                        if (ClipboardText.Contains("\r\n未鑒定"))
                        {//未鉴定
                            itemInfo.Unidentified = true;
                        }

                        if (itemBaseType.Contains(" Map"))
                        {//地图
                            itemInfo.Flag |= ItemFlag.Map;
                            itemInfo.MapTier = mapTier;
                        }
                        else if (itemBaseType.Contains(" Jewel"))
                        {//珠宝
                            itemInfo.Flag |= ItemFlag.Jewels;
                        }
                        else if (itemBaseType.Contains(" Flask"))
                        {//药剂
                            itemInfo.Flag |= ItemFlag.Flasks;
                        }
                        else if (itemBaseType.Contains(" Amulet"))
                        {//首饰 项链
                            itemInfo.Flag |= ItemFlag.Accessories;
                        }
                        else if (itemBaseType.Contains(" Ring"))
                        {//首饰 戒指
                            itemInfo.Flag |= ItemFlag.Accessories;
                        }
                        else if (itemBaseType.Contains(" Sash") || itemBaseType.Contains(" Vise") || itemBaseType.Contains(" Belt"))
                        {//首饰 腰带
                            itemInfo.Flag |= ItemFlag.Accessories;
                        }
                        else if (clipboardTextArr[1].Contains("攻擊頻率:"))
                        {//武器
                            itemInfo.Flag |= ItemFlag.Weapons;
                        }

                    }
                    break;
                /*
                case "宝石": //技能石
                    break;
                */
                case "普通":
                    {
                        itemInfo.Flag = ItemFlag.Normal;

                        if (itemName.Contains(" Map"))
                        {//地图
                            itemInfo.Flag |= ItemFlag.Map;
                            itemInfo.MapTier = mapTier;
                        }
                        else if (itemName == "Offering to the Goddess" || itemName == "Divine Vessel" || itemName.StartsWith("Sacrifice at ") || itemName.StartsWith("Mortal ") || itemName.StartsWith("Fragment of") || itemName.EndsWith("s Key"))
                        {
                            itemInfo.Flag |= ItemFlag.Fragments;
                        }
                        else if (clipboardTextArr[0].Contains("[預言]"))
                        {
                            itemInfo.Flag |= ItemFlag.Prophecies;
                        }
                    }
                    break;
                case "魔法":
                    {
                        if (itemName.Contains(" Map"))
                        {
                            itemInfo.Flag = ItemFlag.Map;
                            itemInfo.MapTier = mapTier;
                        }
                    }
                    break;
                case "稀有":
                    {
                        if (itemName.Contains(" Map"))
                        {
                            itemInfo.Flag = ItemFlag.Map;
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
                    _text = $"普通白色 || 單價: {_item.ChaosValue} Chaos || 最近7天走勢: {_item.Sparkline.TotalChange}%";
            }
            #endregion
            #region Unique Maps
            else if (itemInfo.Flag.HasFlag(ItemFlag.Unique) && itemInfo.Flag.HasFlag(ItemFlag.Map))
            {
                var _item = Api.Ninja.UniqueMaps.Lines.Find(x => x.Name == itemInfo.Name);
                if (_item != null && _item.ChaosValue > 0)
                    _text = $"單價: {_item.ChaosValue} Chaos || 最近7天走勢: {_item.Sparkline.TotalChange}%";
            }
            #endregion
            #region Divination Cards
            else if (itemInfo.Flag.HasFlag(ItemFlag.Divination))
            {
                var _item = Api.Ninja.DivinationCards.Lines.Find(x => x.Name == itemInfo.Name);
                if (_item != null && _item.ChaosValue > 0)
                {
                    _text = $"單價: {_item.ChaosValue} Chaos || 最近7天走勢: {_item.Sparkline.TotalChange}%";
                    if (itemInfo.Size > 1)
                    {
                        _text += $"\n總價: {itemInfo.Size * _item.ChaosValue} Chaos";
                    }
                }
            }
            #endregion
            #region Fragments
            else if (itemInfo.Flag.HasFlag(ItemFlag.Normal) && itemInfo.Flag.HasFlag(ItemFlag.Fragments))
            {
                var _item = Api.Ninja.Fragments.Lines.Find(x => x.CurrencyTypeName == itemInfo.Name);
                if (_item != null && _item.ChaosEquivalent > 0)
                    _text = $"單價: {_item.ChaosEquivalent} Chaos || 最近7天走勢: {_item.ReceiveSparkLine.TotalChange}%";
            }
            #endregion
            #region Unique Accessories
            else if (itemInfo.Flag.HasFlag(ItemFlag.Unique) && itemInfo.Flag.HasFlag(ItemFlag.Accessories) && !itemInfo.Unidentified)
            {
                string fixItemName = (itemInfo.Name == "Tasalio’s Sign") ? "Tasalio's Sign" : itemInfo.Name;

                var _item = Api.Ninja.UniqueAccessories.Lines.Find(x => x.Name == fixItemName);
                if (_item != null && _item.ChaosValue > 0)
                    _text = $"單價: {_item.ChaosValue} Chaos || 最近7天走勢: {_item.Sparkline.TotalChange}%";
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
                        //_text = $"{_item.variant} || 單價: {_item.ChaosValue} Chaos || 最近7天走勢: {_item.Sparkline.TotalChange}%";
                        _temp = $"攻擊附加 || 單價: {_item.ChaosValue} Chaos || 最近7天走勢: {_item.Sparkline.TotalChange}%";
                        _text = string.IsNullOrEmpty(_text) ? _temp : _text + Environment.NewLine + _temp;
                    }
                    _item = Api.Ninja.UniqueFlasks.Lines.Find(x => x.Name == itemInfo.Name && x.Variant == "Penetration");
                    if (_item != null && _item.ChaosValue > 0)
                    {
                        _temp = $"電抗穿透 || 單價: {_item.ChaosValue} Chaos || 最近7天走勢: {_item.Sparkline.TotalChange}%";
                        _text = string.IsNullOrEmpty(_text) ? _temp : _text + Environment.NewLine + _temp;
                    }
                    _item = Api.Ninja.UniqueFlasks.Lines.Find(x => x.Name == itemInfo.Name && x.Variant == "Added Spells");
                    if (_item != null && _item.ChaosValue > 0)
                    {
                        _temp = $"法術附加 || 單價: {_item.ChaosValue} Chaos || 最近7天走勢: {_item.Sparkline.TotalChange}%";
                        _text = string.IsNullOrEmpty(_text) ? _temp : _text + Environment.NewLine + _temp;
                    }
                    _item = Api.Ninja.UniqueFlasks.Lines.Find(x => x.Name == itemInfo.Name && x.Variant == "Conversion");
                    if (_item != null && _item.ChaosValue > 0)
                    {
                        _temp = $"物理轉換 || 單價: {_item.ChaosValue} Chaos || 最近7天走勢: {_item.Sparkline.TotalChange}%";
                        _text = string.IsNullOrEmpty(_text) ? _temp : _text + Environment.NewLine + _temp;
                    }
                }
                else
                {
                    var _item = Api.Ninja.UniqueFlasks.Lines.Find(x => x.Name == itemInfo.Name);
                    if (_item != null && _item.ChaosValue > 0)
                        _text = $"單價: {_item.ChaosValue} Chaos || 最近7天走勢: {_item.Sparkline.TotalChange}%";
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
                    _text = $"單價: {_item.ChaosValue} Chaos || 最近7天走勢: {_item.Sparkline.TotalChange}%";
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
                    _temp = $"0連 || 單價: {_item.ChaosValue} Chaos || 最近7天走勢: {_item.Sparkline.TotalChange}%";
                    _text = string.IsNullOrEmpty(_text) ? _temp : _text + Environment.NewLine + _temp;
                }
                _item = Api.Ninja.UniqueWeapons.Lines.Find(x => x.Name == itemInfo.Name && x.Links == 5);
                if (_item != null && _item.ChaosValue > 0)
                {
                    _temp = $"5連 || 單價: {_item.ChaosValue} Chaos || 最近7天走勢: {_item.Sparkline.TotalChange}%";
                    _text = string.IsNullOrEmpty(_text) ? _temp : _text + Environment.NewLine + _temp;
                }
                _item = Api.Ninja.UniqueWeapons.Lines.Find(x => x.Name == itemInfo.Name && x.Links == 6);
                if (_item != null && _item.ChaosValue > 0)
                {
                    _temp = $"6連 || 單價: {_item.ChaosValue} Chaos || 最近7天走勢: {_item.Sparkline.TotalChange}%";
                    _text = string.IsNullOrEmpty(_text) ? _temp : _text + Environment.NewLine + _temp;
                }
            }
            #endregion
            #region Breachstone
            else if (itemInfo.Flag.HasFlag(ItemFlag.Breachstone))
            {
                var _item = Api.Ninja.Fragments.Lines.Find(x => x.CurrencyTypeName == itemInfo.Name);
                if (_item != null && _item.ChaosEquivalent > 0)
                    _text = $"單價: {_item.ChaosEquivalent} Chaos || 最近7天走勢: {_item.ReceiveSparkLine.TotalChange}%";
            }
            #endregion
            #region Prophecies
            else if (itemInfo.Flag.HasFlag(ItemFlag.Prophecies))
            {
                var _item = Api.Ninja.Prophecies.Lines.Find(x => x.Name == itemInfo.Name);
                if (_item != null && _item.ChaosValue > 0)
                    _text = $"單價: {_item.ChaosValue} Chaos || 最近7天走勢: {_item.Sparkline.TotalChange}%";
            }
            #endregion
            #region Essence
            else if (itemInfo.Flag.HasFlag(ItemFlag.Essence) || itemInfo.Name.Contains("Remnant of"))
            {
                var _item = Api.Ninja.Essences.Lines.Find(x => x.Name == itemInfo.Name);
                if (_item != null && _item.ChaosValue > 0)
                {
                    _text = $"單價: {_item.ChaosValue} Chaos || 最近7天走勢: {_item.Sparkline.TotalChange}%";
                    if (itemInfo.Size > 1)
                    {
                        _text += $"\n總價: {itemInfo.Size * _item.ChaosValue} Chaos";
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
                //处理汉化补丁名词不全问题
                string fixItemName = itemInfo.Name;
                if (itemInfo.Name == "GCP")
                {
                    fixItemName = "Gemcutter's Prism";
                }
                else if (itemInfo.Name == "GBB")
                {
                    fixItemName = "Glassblower's Bauble";
                }
                else if (itemInfo.Name == "Chisel")
                {
                    fixItemName = "Cartographer's Chisel";
                }
                else if (itemInfo.Name == "Exalted" || itemInfo.Name == "Regal" || itemInfo.Name == "Divine" || itemInfo.Name == "Blessed" || itemInfo.Name == "Vaal" || itemInfo.Name == "Eternal" || itemInfo.Name == "Chromatic")
                {
                    fixItemName = itemInfo.Name + " Orb";
                }
                else if (itemInfo.Name == "Fusing" || itemInfo.Name == "Regret" || itemInfo.Name == "Alchemy" || itemInfo.Name == "Chance" || itemInfo.Name == "Transmutation" || itemInfo.Name == "Alteration" || itemInfo.Name == "Scouring" || itemInfo.Name == "Augmentation")
                {
                    fixItemName = "Orb of " + itemInfo.Name;
                }

                var _item = Api.Ninja.Currency.Lines.Find(x => x.CurrencyTypeName == fixItemName);
                if (_item != null && _item.ChaosEquivalent > 0)
                {
                    _text = $"單價: {_item.ChaosEquivalent} Chaos || 最近7天走勢: {_item.ReceiveSparkLine.TotalChange}%";
                    if (itemInfo.Size > 1)
                    {
                        _text += $"\n總價: {itemInfo.Size * _item.ChaosEquivalent} Chaos";
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
                    _temp = $"0連 || 單價: {_item.ChaosValue} Chaos || 最近7天走勢: {_item.Sparkline.TotalChange}%";
                    _text = string.IsNullOrEmpty(_text) ? _temp : _text + Environment.NewLine + _temp;
                }
                _item = Api.Ninja.UniqueArmours.Lines.Find(x => x.Name == fixItemName && x.Links == 5);
                if (_item != null && _item.ChaosValue > 0)
                {
                    _temp = $"5連 || 單價: {_item.ChaosValue} Chaos || 最近7天走勢: {_item.Sparkline.TotalChange}%";
                    _text = string.IsNullOrEmpty(_text) ? _temp : _text + Environment.NewLine + _temp;
                }
                _item = Api.Ninja.UniqueArmours.Lines.Find(x => x.Name == fixItemName && x.Links == 6);
                if (_item != null && _item.ChaosValue > 0)
                {
                    _temp = $"6連 || 單價: {_item.ChaosValue} Chaos || 最近7天走勢: {_item.Sparkline.TotalChange}%";
                    _text = string.IsNullOrEmpty(_text) ? _temp : _text + Environment.NewLine + _temp;
                }
            }
            #endregion

            return _text;
        }

    }
}
