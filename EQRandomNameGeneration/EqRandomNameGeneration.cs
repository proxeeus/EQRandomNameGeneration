using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EqRandomNameGeneration
{
    public class EqRandomNameGeneration
    {
        public static void Main()
        {
            DumpClientNameData();
            DumpAllRaceAndGenderNames();
        }

        private static void DumpClientNameData()
        {
            EqRandomNameTableExtractor extractor = new EqRandomNameTableExtractor();
            extractor.DumpClientNameData();
        }

        private static void DumpAllRaceAndGenderNames()
        {
            EqRandomNameGenerator generator = new EqRandomNameGenerator();
            generator.Initialize("Data/namefragments.txt", "Data/badwords.txt", "Data/disallowedracecombos.txt");

            string directoryName = "Names";
            Directory.CreateDirectory(directoryName);

            for (int i = 0; i < 15; ++i)
            {
                for (int j = 0; j < 2; ++j)
                {
                    List<string> names = new List<string>();

                    for (int k = 0; k < 1000; ++k)
                    {
                        string name = generator.GenerateRandomName(i, j);

                        if (string.IsNullOrEmpty(name) || names.Contains(name))
                        {
                            continue;
                        }

                        names.Add(name);
                    }

                    names.Sort();

                    StringBuilder nameExport = new StringBuilder();

                    foreach (var name in names)
                    {
                        nameExport.AppendLine(name);
                    }

                    File.WriteAllText(
                        $"{directoryName}/{generator.GetRaceString(i)}_{generator.GetGenderString(j)}.txt",
                        nameExport.ToString());
                }
            }
        }
    }
}