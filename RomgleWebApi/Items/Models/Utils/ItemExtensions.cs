using MongoDB.Driver;

namespace RotmgleWebApi.Items
{
    public static class ItemExtensions
    {
        #region public methods

        /// <summary>
        /// Generates anagram out of Item's name, ignoring letter doubles and special symbols
        /// </summary>
        public static string GenerateAnagram(this Item item)
        {
            string name = item.Name
                .RemoveSymbols(",.'`[]{}()-*_:;\"!?")
                .ToLower();
            List<string> initialWords = name.Split(' ').ToList();
            string anagram = "";
            foreach (char letter in name)
            {
                if (!anagram.Contains(letter) && letter != ' ')
                {
                    anagram += letter;
                }
            }
            bool check = true;
            while (check)
            {
                anagram = anagram.Shuffle();
                if (!anagram.Contains(StringUtils.IgnoredWords) && !anagram.Contains(initialWords))
                {
                    check = false;
                }
            }
            return anagram;
        }

        #endregion public methods
    }
}
