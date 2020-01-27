using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EqRandomNameGeneration
{
    public class EqRandomNameGenerator
    {
        public static Random _randomNumber = new Random();

        public const int NameMinLength = 4;
        public const int NameMaxLength = 15;

        private bool _isInitialized = false;

        private List<string> _nameFragments = new List<string>();
        private List<string> _badWords = new List<string>();
        private List<string> _disallowedRaceCombos = new List<string>();

        public void Initialize(string pathToNameFragments, string pathToBadWords, string pathToProhibitedCombos)
        {
            if (_isInitialized)
            {
                return;
            }

            if (!File.Exists(pathToNameFragments))
            {
                return;
            }

            if (!File.Exists(pathToBadWords))
            {
                return;
            }

            if (!File.Exists(pathToProhibitedCombos))
            {
                return;
            }

            Initialize(File.ReadAllLines(pathToNameFragments).ToList(), File.ReadAllLines(pathToBadWords).ToList(),
                File.ReadAllLines(pathToProhibitedCombos).ToList());
        }

        public void Initialize(List<string> nameFragments, List<string> badWords, List<string> prohibitedRaceCombos)
        {
            if (_isInitialized)
            {
                return;
            }

            if (nameFragments == null || badWords == null || prohibitedRaceCombos == null)
            {
                return;
            }
            
            _nameFragments.AddRange(nameFragments);
            _badWords.AddRange(badWords);
            _disallowedRaceCombos.AddRange(prohibitedRaceCombos);
            
            _isInitialized = true;
        }

        public string GenerateRandomName(int raceId, int gender)
        {
            if (!_isInitialized)
            {
                return string.Empty;
            }
            
            int[] probabilityTable = new int[5];
            probabilityTable[0] = 100;
            probabilityTable[1] = 80;
            probabilityTable[2] = 80;
            probabilityTable[3] = 100;
            probabilityTable[4] = gender == 0 ? 0 : 100;

            string name = string.Empty;
            int maxLoops = 5;
            int creationAttempts = 0;
            
            while (creationAttempts < 50)
            {
                string[] nameFragments = new string[5];
                int totalFragmentsLength = 0;

                for (int i = 0; i < maxLoops; ++i)
                {
                    int chanceCurrent = GetRandomNumberInRange(0, 100);

                    if (probabilityTable[i] == 100 || chanceCurrent < probabilityTable[i])
                    {
                        int tableIndexRandom = GetRandomNumberInRange(0, 9);
                        int nameIndex = i + (tableIndexRandom + raceId * 10) * 5;
                        nameFragments[i] = _nameFragments[nameIndex];
                        totalFragmentsLength += nameFragments[i].Length;
                    }
                }

                string finalName = string.Empty;

                foreach (var fragment in nameFragments)
                {
                    finalName += fragment;
                }

                creationAttempts++;

                if (totalFragmentsLength < NameMinLength || totalFragmentsLength > NameMaxLength)
                {
                    continue;
                }

                if (ContainsFourConsecutiveVowels(finalName))
                {
                    continue;
                }

                if (ContainsDisallowedCombo(finalName, raceId))
                {
                    continue;
                }

                if (ContainsBadWord(finalName))
                {
                    continue;
                }

                if (!IsVowel(finalName[0]) && !IsVowel(finalName[1]) && finalName[1] == finalName[2])
                {
                    finalName = finalName.Substring(0, 1) + GetRandomVowel() + finalName.Substring(1);
                }

                return finalName;
            }

            return string.Empty;
        }
        
        private bool ContainsDisallowedCombo(string finalName, int raceId)
        {
            int start = raceId * 25;
            int end = start + 25;

            for(int i = start; i < end; ++i)
            {
                if (finalName.Contains(_disallowedRaceCombos[i]))
                {
                    return true;
                }
            }

            return false;
        }

        private bool ContainsBadWord(string finalName)
        {
            foreach (var badWordFragment in _badWords)
            {
                if (finalName.ToLower().Contains(badWordFragment))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsVowel(char letter)
        {
            return letter == 'e' || letter == 'E' ||
                letter == 'a' || letter == 'A' ||
                letter == 'o' || letter == 'O' ||
                letter == 'i' || letter == 'I' ||
                letter == 'u' || letter == 'U' ||
                letter == 'y' || letter == 'Y';
        }

        private bool ContainsFourConsecutiveVowels(string name)
        {
            for (int i = 0; i < name.Length - 4; ++i)
            {
                if (IsVowel(name[i]) && IsVowel(name[i + 1]) && IsVowel(name[i + 2]) && IsVowel(name[i + 3]))
                {
                    return true;
                }
            }

            return false;
        }

        private char GetRandomVowel()
        {
            int randomValue = GetRandomNumberInRange(0, 5);

            switch (randomValue)
            {
                case 0:
                    return 'a';
                case 1:
                    return 'e';
                case 2:
                    return 'i';
                case 3:
                    return 'o';
                case 4:
                    return 'u';
                default:
                    return 'y';
            }
        }

        private int GetRandomNumberInRange(int min, int max)
        {
            return _randomNumber.Next(min, max + 1);
        }
        
        public string GetRaceString(int race)
        {
            switch (race)
            {
                case 0:
                    return "soldier";
                case 1:
                    return "human";
                case 2:
                    return "barbarian";
                case 3:
                    return "erudite";
                case 4:
                    return "woodelf";
                case 5:
                    return "highelf";
                case 6:
                    return "darkelf";
                case 7:
                    return "halfelf";
                case 8:
                    return "dwarf";
                case 9: 
                    return "troll";
                case 10: 
                    return "ogre";
                case 11: 
                    return "halfling";
                case 12: 
                    return "gnome";
                case 13: 
                    return "iksar";
                case 14: 
                    return "vahshir";
                default: 
                    return "unknown";
            }
        }
        
        public string GetGenderString(int gender)
        {
            switch (gender)
            {
                case 0:
                    return "male";
                case 1:
                    return "female";
                default:
                    return "unknown";
            }
        }
    }
}