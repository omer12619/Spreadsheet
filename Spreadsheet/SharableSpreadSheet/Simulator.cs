using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SharableSpreadSheet
{
    internal class Simulator
    {
        private int rows;
        private int columns;
        private SharableSpreadSheet sharableSpreadSheet;
        private int usres;
        private int nOperations;
        private int mssleep;
        private Thread[] threads;
        private List<int> func_id;
        private List<String> animeNames;

        public Simulator(int rows, int columns, int usres, int nOperations, int mssleep)
        {
            this.rows = rows;
            this.columns = columns;
            sharableSpreadSheet = new SharableSpreadSheet(rows, columns, usres);
            sharableSpreadSheet.load("TextFile1.txt");
            this.usres = usres;
            this.nOperations = nOperations;
            this.mssleep = mssleep;
            threads = new Thread[usres];
            //add the idss of the functions

            this.func_id = new List<int>();
            for (int i = 1; i < 15; i++)
            {
                this.func_id.Add(i);
            }


            //string array to random the fuction argument

            string[] animeNames = new string[]
            {
            "Naruto",
            "Attack on Titan",
            "One Piece",
            "Dragon Ball Z",
            "Death Note",
            "Fullmetal Alchemist: Brotherhood",
            "My Hero Academia",
            "One Punch Man",
            "Sword Art Online",
            "Demon Slayer",
            "Tokyo Ghoul",
            "Hunter x Hunter",
            "Fairy Tail",
            "Black Clover",
            "Code Geass",
            "Steins;Gate",
            "Bleach",
            "Cowboy Bebop",
            "Attack on Titan",
            "Dragon Ball Super"
            };
            this.animeNames = animeNames.ToList();




















            for (int i = 0; i < usres; i++)
            {
                threads[i] = new Thread(new ThreadStart(() => UserRunnable(i, nOperations, sharableSpreadSheet, mssleep)));
                threads[i].Start();
            }
           

        }

        private void UserRunnable(int userId, int nOperations, SharableSpreadSheet sharableSpreadSheet, int mssleep)
        {

            Shuffle(func_id);
            




            for (int i = 0; i < nOperations; i++)
            {
                int functionNumber = i; // The function number to check

                if (functionNumber == 1)
                {
                    /***************/

                    //need to cheak whta if the size is get bigger or smaller
                    int variable = this.columns;

                    Random random = new Random();
                    int colm = random.Next(0, variable + 1);

                    int rows = random.Next(0, this.rows + 1);
                    this.sharableSpreadSheet.getCell(rows, colm);
                }
                else if (functionNumber == 2)
                {

                    int variable = this.columns;

                    Random random = new Random();
                    int colm = random.Next(0, variable + 1);

                    int rows = random.Next(0, this.rows + 1);
                    Shuffle(animeNames);



                    this.sharableSpreadSheet.setCell(rows, colm, animeNames[0]);
                }
                else if (functionNumber == 3)
                {
                    Shuffle(animeNames);
                    this.sharableSpreadSheet.searchString(animeNames[0]);
                }
                else if (functionNumber == 4)
                {


                    int variable = this.rows;

                    Random random = new Random();
                    int row1 = random.Next(0, variable + 1);

                    int row2 = random.Next(0, variable + 1);
                    this.sharableSpreadSheet.exchangeRows(row1,row2);

                }
                else if (functionNumber == 5)
                {
                    int variable = this.columns;

                    Random random = new Random();
                    int colm1 = random.Next(0, variable + 1);

                    int colm2 = random.Next(0, variable + 1);
                    this.sharableSpreadSheet.exchangeCols( colm1,  colm2);

                }
                else if (functionNumber == 6)
                {


                    int variable = this.rows;

                    Random random = new Random();
                    int row1 = random.Next(0, variable + 1);
                    Shuffle(animeNames);
                    this.sharableSpreadSheet.searchInRow( row1, animeNames[0]);

                }
                else if (functionNumber == 7)
                {

                    int variable = this.columns;
                    Random random = new Random();
                    int col1 = random.Next(0, variable + 1);
                    Shuffle(animeNames);
                    this.sharableSpreadSheet.searchInCol( col1, animeNames[0]);

                }
                else if (functionNumber == 8)
                {
                    int variable = this.columns;
                    Random random = new Random();
                    int col1 = random.Next(0, variable );
                    int col2 = random.Next(col1, variable + 1);
                    int variable1 = this.rows;
                    Random random1 = new Random();
                    int row1 = random.Next(0, variable );
                    int row2 = random.Next(row1, variable + 1);
                    Shuffle(animeNames);
                    this.sharableSpreadSheet.searchInRange( col1,  col2,  row1,  row2, animeNames[0]);

                }
                else if (functionNumber == 9)
                {
                    int variable = this.rows;
                    Random random = new Random();
                    int row1 = random.Next(0, variable+1);
                    this.sharableSpreadSheet.addRow( row1);

                }
                else if (functionNumber == 10)
                {

                    int variable = this.columns;
                    Random random = new Random();
                    int col1 = random.Next(0, variable);
                    this.sharableSpreadSheet.addCol( col1);
                }

                else if (functionNumber == 11)
                {
                    Shuffle(animeNames);
                    this.sharableSpreadSheet.FindAll(animeNames[0], false);

                }
                else if (functionNumber == 12)
                {
                    String str1 = animeNames[0];
                    Shuffle(animeNames);
                    String str2 = animeNames[0];
                    this.sharableSpreadSheet.SetAll(str1, str2, false);

                }
                else if (functionNumber == 13)
                {
                    this.sharableSpreadSheet.GetSize();

                }
                else if (functionNumber == 14)
                {
                    Random random = new Random();
                    int var = random.Next(0, 10);

                    this.sharableSpreadSheet.Save("save_to_gile"+var);

                }
            }
            Thread.Sleep(this.mssleep);


        }























        static void Shuffle<T>(List<T> list)
        {
            Random random = new Random();

            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }


    }
}
