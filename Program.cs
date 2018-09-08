using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ba_uzd3
{

    class Row
    {
        //DARZ_ID;SCHOOL_NAME;TYPE_ID;TYPE_LABEL;LAN_ID;LAN_LABEL;CHILDS_COUNT;FREE_SPACE
        public int darzID;
        public string schoolName;
        public int typeID;
        public string typeLabel;
        public int lanID;
        public string lanLabel;
        public int childsCount;
        public int freeSpace;

        public Row(string lineFromCSV) {

            string[] splitLine = lineFromCSV.Split(';');

            this.darzID = Convert.ToInt32(splitLine[0]);
            this.schoolName = splitLine[1];
            this.typeID = Convert.ToInt32(splitLine[2]);
            this.typeLabel = splitLine[3];
            this.lanID = Convert.ToInt32(splitLine[4]);
            this.lanLabel = splitLine[5];
            this.childsCount = Convert.ToInt32(splitLine[6]);
            this.freeSpace = Convert.ToInt32(splitLine[7]);

        }

        public string PrintAttributes() {

            return string.Format("{0} {1} {2} {3} {4} {5} {6} {7}",
                darzID, schoolName, typeID, typeLabel, lanID, lanLabel, childsCount, freeSpace);
        }
    }

    class Program
    {


        static void Main(string[] args)
        {
            string pathToCSV = "Darzeliu galimu priimti ir lankantys vaikai2018.csv"; // CSV file in debug directory
            Row[] rows = ReadFromCSVToRowArray(pathToCSV);

            // get min & max child counts
            var minChildCount = rows.Min(x => x.childsCount);
            var maxChildCount = rows.Max(x => x.childsCount);

            WriteMinMaxChildCounts(minChildCount, maxChildCount);

            WriteRowsWithMinMaxChildCounts(minChildCount, maxChildCount, rows);

            WriteLanguageGroupWithMostFreeSpacesPercentage(rows);

            WriteKindergartensFromFreeSpaceIntervalDescending(2, 4, rows);
            
            //Print location of text files with answers
            DirectoryInfo dirOfTextFiles = new DirectoryInfo("."); 
            Console.WriteLine("Text files with results of tasks at:\n" + dirOfTextFiles.FullName);
            Console.ReadKey();

        }

        static Row[] ReadFromCSVToRowArray(string sourceFile)
        {
            var arrayOfFileLines = File.ReadAllLines(sourceFile);
            var rowArray = new Row[arrayOfFileLines.Length - 1]; // create row object for every file line, excluding header line

            using (var sw = new StreamWriter("Data from CSV.txt"))
            {
                for (int i = 0; i < rowArray.Length; i++)
                {
                    rowArray[i] = new Row(arrayOfFileLines[i + 1]); // i+1 to skip header line (first line)
                    sw.WriteLine(rowArray[i].PrintAttributes());
                }
            }
            return rowArray;
        }

        static void WriteMinMaxChildCounts(int min, int max)
        {

            Console.WriteLine("Childs Count");
            Console.WriteLine("Min: " + min);
            Console.WriteLine("Max: " + max);

            using (var sw = new StreamWriter("Min & max child counts.txt")) {
                sw.WriteLine("Min: " + min);
                sw.WriteLine("Max: " + max);
            }

        }

        static void WriteRowsWithMinMaxChildCounts(int minChildCount, int maxChildCount, Row[] rows)
        {
            // get rows where child count is min/max
            var minChildCountRow = rows.Where(x => x.childsCount == minChildCount).First();
            var maxChildCountRow = rows.Where(x => x.childsCount == maxChildCount).First();

            // print to file
            using (var sw = new StreamWriter("Kindergartens with min & max childs counts.txt"))
            {
                sw.WriteLine("Min: " + Shorten(minChildCountRow));
                sw.WriteLine("Max: " + Shorten(maxChildCountRow));
            }
        }

        static string Shorten(Row rowToShorten)
        {

            string newString = rowToShorten.typeLabel;
            newString = newString.Remove(0, 5); // remove substring ""Nuo ".
            newString = newString.Remove(newString.Length - 6, 6); // remove substring " metų"".
            newString = newString.Replace(" iki ", "-"); 
            newString = rowToShorten.schoolName.Substring(0, 3) + "_" + newString;
            newString += "_" + rowToShorten.lanLabel.Substring(0,4);

            return newString;
        }


        static void WriteLanguageGroupWithMostFreeSpacesPercentage(Row[] rows)
        {
            var totalFreeSpaces = rows.Sum(x => x.freeSpace);
            //get percentage of free spaces for each language group
            var languages = from row in rows
                            group row by row.lanLabel into g
                            select new
                            {
                                freeSpacesPercentage = Math.Round((double)g.Sum(x => x.freeSpace) / totalFreeSpaces * 100, 2), 
                                lanLabel = g.Key
                            };

            var maxLanguage = languages.Where(x => x.freeSpacesPercentage == languages.Max(y => y.freeSpacesPercentage)).First(); // find language with highest percentage

            using (var sw = new StreamWriter("Language group with most free spaces.txt"))          
                sw.WriteLine(maxLanguage.lanLabel + ": " + maxLanguage.freeSpacesPercentage + "%");       
        }


        static void WriteKindergartensFromFreeSpaceIntervalDescending(int intervalStart, int intervalEnd, Row[] rows)
        {
            var kindergartens = from row in rows
                                where row.freeSpace >= intervalStart && row.freeSpace <= intervalEnd
                                orderby row.schoolName descending
                                select row;
            // print to file
            using (var sw = new StreamWriter($"Kindergartens with {intervalStart} to {intervalEnd} free spaces.txt"))
                foreach (var kG in kindergartens) sw.WriteLine(kG.PrintAttributes());

        }
    }
}
