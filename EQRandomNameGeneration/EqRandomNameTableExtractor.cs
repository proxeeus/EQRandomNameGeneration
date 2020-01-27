using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace EqRandomNameGeneration
{
    public class EqRandomNameTableExtractor
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern bool ReadProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize,
            ref int lpNumberOfBytesRead);

        private const int ProcessWmRead = 0x0010;
        private const string EverQuestProcessName = "eqgame";
        private const string DirectoryName = "Data";


        private const int NameFragmentTableAddress = 0x00566838;
        private const int BadWordTableAddress = 0x005679d0;
        private const int DisallowedRaceComboAddress = 0x005673f0;

        public void DumpClientNameData()
        {
            Process[] processes = Process.GetProcessesByName(EverQuestProcessName);

            if (processes.Length == 0)
            {
                Console.WriteLine("Error: No EverQuest process found!");
                return;
            }

            if (processes.Length > 1)
            {
                Console.WriteLine("Error: stMultiple EverQuest processes found!");
                return;
            }

            Process process = processes[0];
            IntPtr processHandle = OpenProcess(ProcessWmRead, false, process.Id);

            // 750 = 50 fragments * 15 races
            string nameFragmentTable =
                ReadFixedLengthStringPointers(processHandle, NameFragmentTableAddress, 0x04, 750);

            Directory.CreateDirectory(DirectoryName);

            if (!string.IsNullOrEmpty(nameFragmentTable))
            {
                File.WriteAllText($"{DirectoryName}/namefragments.txt", nameFragmentTable);
            }

            string badWordTable = ReadVariableLengthStringPointers(processHandle, BadWordTableAddress, 25);

            if (!string.IsNullOrEmpty(badWordTable))
            {
                File.WriteAllText($"{DirectoryName}/ badwords.txt", badWordTable);
            }

            // 375 = 25 combos * 15 races
            string disallowedRaceComboTable =
                ReadFixedLengthStringPointers(processHandle, DisallowedRaceComboAddress, 0x04, 375);

            if (!string.IsNullOrEmpty(disallowedRaceComboTable))
            {
                File.WriteAllText($"{DirectoryName}/ disallowedracecombos.txt", disallowedRaceComboTable);
            }
        }

        private string ReadFixedLengthStringPointers(IntPtr processHandle, int tableStart, int entryLength,
            int entryCount)
        {
            int bytesRead = 0;
            byte[] pointerBuffer = new byte[entryLength];
            byte[] valueBuffer = new byte[entryLength];

            StringBuilder memoryRead = new StringBuilder();

            for (int i = 0; i < entryCount; ++i)
            {
                ReadProcessMemory((int) processHandle, tableStart, pointerBuffer, pointerBuffer.Length, ref bytesRead);
                ReadProcessMemory((int) processHandle, BitConverter.ToInt32(pointerBuffer, 0), valueBuffer,
                    valueBuffer.Length, ref bytesRead);
                memoryRead.AppendLine(Encoding.ASCII.GetString(valueBuffer).Replace("\0", string.Empty));
                tableStart += entryLength;
            }

            return memoryRead.ToString();
        }

        private string ReadVariableLengthStringPointers(IntPtr processHandle, int tableStart, int entryCount)
        {
            int bytesRead = 0;

            byte[] pointerBuffer = new byte[0x04];
            byte[] singleByteRead = new byte[1];

            StringBuilder memoryRead = new StringBuilder();

            for (int i = 0; i < entryCount; ++i)
            {
                ReadProcessMemory((int) processHandle, tableStart, pointerBuffer, pointerBuffer.Length, ref bytesRead);

                int pointerStart = BitConverter.ToInt32(pointerBuffer, 0);

                bool readCompleteString = false;

                while (!readCompleteString)
                {
                    ReadProcessMemory((int) processHandle, pointerStart, singleByteRead, 1, ref bytesRead);

                    if (singleByteRead[0] == '\0')
                    {
                        readCompleteString = true;
                        memoryRead.AppendLine();
                    }
                    else
                    {
                        memoryRead.Append(Encoding.ASCII.GetString(singleByteRead));
                        pointerStart += 0x1;
                    }
                }

                tableStart += 0x04;
            }

            return memoryRead.ToString();
        }
    }
}