using HtmlAgilityPack;
using Microsoft.Extensions.Options;
using RomgleWebApi.Data.Extensions;
using RomgleWebApi.Data.Models;
using RomgleWebApi.Data.Structures;
using RomgleWebApi.Services;
using System.Linq;
using System.Net;

namespace RomgleWebApi.Data
{
    public class RealmeyeScraper
    {
        private List<string> ignoreList;

        public RealmeyeScraper()
        {
            ignoreList = new List<string>(new string[] { "status-effects", "oryx-the-mad-god-3", "headless-ted", "alien-gear"});
        }

        public void Start()
        {
            IEnumerable<WikiLink> itemTypesLinks = ParseEquipmentPage("https://www.realmeye.com/wiki/equipment").Concat(ParseRingsPage());
            List<WikiLink> equipList = new List<WikiLink>();
            //foreach (WikiLink itemTypeLink in itemTypesLinks)
            //{
            //    Console.WriteLine($"{itemTypeLink.ItemType}, {itemTypeLink.Link}");
            //}
            //ParseItemPage($"https://www.realmeye.com/wiki/seal-of-invocation", "Ability");
            //ParseItemPage($"https://www.realmeye.com/wiki/helm-of-the-juggernaut", "Ability");
            //ParseItemPage($"https://www.realmeye.com/wiki/kageboshi", "Ability");
            foreach (var wikiLink in itemTypesLinks)
            {
                equipList.AddRange(ParseItemTypePage($"https://www.realmeye.com{wikiLink.Link}")
                    .Select(link => new WikiLink { Link = link, ItemType = wikiLink.ItemType }));
            }
            equipList = equipList.Distinct().ToList();
            Console.WriteLine(equipList.Count);
            foreach (var equip in equipList)
            {
                ParseItemPage($"https://www.realmeye.com{equip.Link}", equip.ItemType);
            }
        }

        public string? GetAttributeFromXml(string itemName, string attributeName)
        {
            //using (var client = new WebClient())
            //{
            //    client.DownloadFile("https://static.drips.pw/rotmg/production/current/xml/Equip.xml", @".\Assets\Xmls\Equip.xml");
            //}
            HtmlDocument document = new HtmlDocument();
            document.Load(@".\Assets\Xmls\Equip.xml");
            HtmlNode selectedNodes = document.DocumentNode.SelectSingleNode($"//object[contains(@id, \"{itemName.ScrubHtml()}\")]");
            if(selectedNodes == null)
            {
                selectedNodes = document.DocumentNode.SelectSingleNode($"//object/displayid[contains(text(), \"{itemName.ScrubHtml()}\")]");
                if (selectedNodes == null) return null;
            }
            foreach (HtmlNode objectChild in selectedNodes.ChildNodes)
            {
                if (objectChild.GetAttributeValue(attributeName, null) != null)
                {
                    return objectChild.GetAttributeValue(attributeName, null);
                }
                else if (objectChild.Name.ToLower() == attributeName.ToLower() && objectChild.GetDirectInnerText() != null)
                {
                    return objectChild.GetDirectInnerText();
                }
                else if(objectChild.Name.ToLower() == "projectile" || objectChild.Name.ToLower() == "extratooltipdata")
                {
                    foreach(HtmlNode nestedAttribute in objectChild.ChildNodes)
                    {
                        if (nestedAttribute.Name.ToLower() == attributeName.ToLower() && nestedAttribute.GetDirectInnerText() != null)
                        {
                            return nestedAttribute.GetDirectInnerText();
                        }
                        else if (nestedAttribute.GetAttributeValue(attributeName, null) != null)
                        {
                            return nestedAttribute.GetAttributeValue(attributeName, null);
                        }
                    }
                }
            }
            return null;
        }

        private IEnumerable<WikiLink> ParseRingsPage()
        {
            const string url = "https://www.realmeye.com/wiki/rings";
            HtmlDocument document = new HtmlWeb().Load(url);
            List<WikiLink> wikiLinks = new List<WikiLink>();
            foreach(HtmlNode linkTag in document.DocumentNode.SelectNodes("//tr/td/a[@href]"))
            {
                wikiLinks.Add(new WikiLink { Link = linkTag.GetAttributeValue("href", null), ItemType = "Ring" });
                if (linkTag.GetAttributeValue("href", null) == "/wiki/limited-rings")
                {
                    break;
                }
            }
            return wikiLinks;
        }
        private IEnumerable<WikiLink> ParseEquipmentPage(string url)
        {
            HtmlDocument document = new HtmlWeb().Load(url);
            List<WikiLink> wikiLinks = new List<WikiLink>();
            List<string> itemTypes = new List<string>();
            HtmlNode table = document.DocumentNode.SelectSingleNode("//table");
            foreach (HtmlNode node in table.SelectNodes("//th/a[@href]/b"))
            {
                itemTypes.Add(node.GetDirectInnerText().ToSingular());
            }
            int itemTypeCount = itemTypes.Count;
            int tdIndex = 0;
            foreach (HtmlNode td in table.SelectNodes("//tr/td")) 
            {
                if (!td.HasAttributes)
                {
                    tdIndex = (tdIndex + 1) % itemTypeCount;
                    continue;
                }
                HtmlNodeCollection links = td.SelectNodes(".//a[@href]");
                if (links == null)
                {
                    continue;
                }
                foreach (HtmlNode link in links)
                {
                    if (link.GetAttributeValue("href", null).Contains("/wiki/untiered-drops"))
                    {
                        break;
                    }
                    wikiLinks.Add(new WikiLink { Link = link.GetAttributeValue("href", null), ItemType = itemTypes[tdIndex]});
                }
                tdIndex = (tdIndex + 1) % itemTypeCount;
                if (wikiLinks.Last().Link == "/wiki/heavy-armors")
                {
                    break;
                }
            }
            return wikiLinks;
        }

        //private List<string> ParseItemTypePage(string url)
        //{
        //    List<string> itemLinks = new List<string>();
        //    HtmlDocument document = new HtmlWeb().Load(url);
        //    IEnumerable<HtmlNode> selectedNodes = document.DocumentNode.SelectNodes("//td/b/a") ?? Enumerable.Empty<HtmlNode>();
        //    selectedNodes = selectedNodes.Concat(document.DocumentNode.SelectNodes("//td/a") ?? Enumerable.Empty<HtmlNode>());

        //    foreach (var link in selectedNodes)
        //    {
        //        if (IsValid(link) && !IsIgnored(link))
        //        {
        //            itemLinks.Add(link.GetAttributeValue("href", null));
        //            Console.WriteLine(link.GetAttributeValue("href", null));
        //        }
        //    }
        //    return itemLinks;
        //}

        private List<string> ParseItemTypePage(string url)
        {
            List<string> itemLinks = new List<string>();
            HtmlDocument document = new HtmlWeb().Load(url);
            IEnumerable<HtmlNode> selectedNodes = document.DocumentNode.SelectNodes("//li[contains(@class, \"active\")]/ul/li/a")
                ?? Enumerable.Empty<HtmlNode>();

            foreach (var link in selectedNodes)
            {
                if (IsValid(link) && !IsIgnored(link))
                {
                    itemLinks.Add(link.GetAttributeValue("href", null));
                }
            }
            return itemLinks;
        }

        private Item ParseItemPage(string url, string itemType)
        {
            Item retrievedItem = new Item();
            HtmlDocument document = new HtmlWeb().Load(url);
            IEnumerable<HtmlNode> selectedNodes = document.DocumentNode.SelectNodes("//table/tbody/tr") ?? Enumerable.Empty<HtmlNode>();
            retrievedItem.Name = document.DocumentNode.SelectSingleNode("//h1").GetDirectInnerText().ScrubHtml();
            foreach (HtmlNode tableRow in selectedNodes)
            {
                foreach (HtmlNode tableRowChild in tableRow.ChildNodes)
                {
                    switch (tableRowChild.GetDirectInnerText())
                    {
                        case "When Key Released" or "Reactive Proc(s)" or "Effect(s)": 
                            foreach(HtmlNode boldTag in tableRowChild.SelectNodes("//b"))
                            {
                                if(boldTag.GetDirectInnerText() == "Damage:")
                                {
                                    retrievedItem.DamageRangeToMargins(boldTag.NextSibling.GetDirectInnerText().Trim().Strip());
                                }
                                else if(boldTag.GetDirectInnerText() == "Range:")
                                {
                                    retrievedItem.Range = boldTag.NextSibling.GetDirectInnerText().Trim().Strip().ParseDouble();
                                    if (retrievedItem.Range == null)
                                    {
                                        retrievedItem.Range = GetAttributeFromXml(retrievedItem.Name, "range").ParseDouble() ?? 0;
                                    }
                                }
                                else if (boldTag.GetDirectInnerText() == "Shots:")
                                {
                                    retrievedItem.NumberOfShots = boldTag.NextSibling.GetDirectInnerText().Trim().Strip().ParseInt();
                                }
                                else if(boldTag.GetDirectInnerText() == "Projectile Speed:" && retrievedItem.NumberOfShots == 0)
                                {
                                    retrievedItem.NumberOfShots = 1;
                                }
                            }
                            break;
                        case "Reskin of": retrievedItem.Reskin = true;
                            break;
                        case "Tier":
                            if (tableRow.ChildNodes[3].GetDirectInnerText().ParseInt() == null) 
                            {
                                retrievedItem.Tier = tableRow.ChildNodes[3].GetDirectInnerText().Strip(); 
                            }
                            break;
                        case "Damage":
                            retrievedItem.DamageRangeToMargins(tableRow.ChildNodes[3].GetDirectInnerText().Strip());
                            break;
                        case "Shots":
                            retrievedItem.NumberOfShots = tableRow.ChildNodes[3].GetDirectInnerText().ScrubHtml().Strip().ParseInt();
                            break;
                        case "Range":
                            retrievedItem.Range = tableRow.ChildNodes[3].GetDirectInnerText().Strip().ParseDouble();
                            if (retrievedItem.Range == null)
                            {
                                retrievedItem.Range = GetAttributeFromXml(retrievedItem.Name, "range").ParseDouble() ?? 0;
                            }
                            break;
                        case "Feed Power":
                            retrievedItem.Feedpower = tableRow.ChildNodes[3].GetDirectInnerText().Replace(",", "").ParseInt();
                            break;
                        case "XP Bonus":
                            retrievedItem.XpBonus = tableRow.ChildNodes[3].GetDirectInnerText().Replace("%", "").ParseInt();
                            break;
                    }
                }
            }
            retrievedItem.Type = itemType;
            Console.WriteLine(retrievedItem.ToString());
            return retrievedItem;
        }

        private bool IsValid(HtmlNode node)
        {
            if (node.GetDirectInnerText() != "UT"
                && node.GetDirectInnerText() != "ST"
                && node.FirstChild.GetDirectInnerText() != "UT"
                && node.FirstChild.GetDirectInnerText() != "ST"
                && node.FirstChild.Name != "abbr"
                && node.FirstChild.Name != "img"
                )
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool IsIgnored(HtmlNode node)
        {
            foreach (var bannedString in ignoreList)
            {
                if (node.GetAttributeValue("href", null).Contains(bannedString))
                {
                    return true;
                }
            }
            return false;
        }

    }

}
