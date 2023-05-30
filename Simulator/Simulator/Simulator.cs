using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Simulator
{
    internal class Simulator
    {
        private int rows;
        private int columns;
        private SharableSpreadSheet sharableSpreadSheet;
        private int usres;
        private int nOperations;
        private int _msSleep;
        private Thread[] threads;
        private List<int> func_id;
        private List<String> animeNames;

        public Simulator(int rows, int columns, int usres, int nOperations, int msSleep)
        {
            this.rows = rows;
            this.columns = columns;
            sharableSpreadSheet = new SharableSpreadSheet(rows, columns, usres);
            sharableSpreadSheet.load("TextFile1.txt");
            this.usres = usres;
            this.nOperations = nOperations;
            this._msSleep = msSleep;
            threads = new Thread[usres];
            //add the idss of the functions

            this.func_id = new List<int>();
            for (int i = 1; i < 14; i++)
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
                threads[i] = new Thread(new ThreadStart(() => UserRunnable(i, nOperations, sharableSpreadSheet, msSleep)));
                threads[i].Start();
            }
           

        }

        private void UserRunnable(int userId, int nOperations, SharableSpreadSheet sharableSpreadSheet, int mssleep)
        {

            Shuffle(func_id);
            for (int i = 1; i < nOperations; i++)
            {
                int functionNumber = func_id[i]; // The function number to check

                if (functionNumber == 1)
                {
                    
                    /***************/

                    //need to cheak whta if the size is get bigger or smaller
                    
                    Console.WriteLine("Get cell " + Thread.CurrentThread.ManagedThreadId );
                    Random random = new Random();
                    int colm = random.Next(0, this.sharableSpreadSheet.getCol());

                    int rows = random.Next(0, this.sharableSpreadSheet.getRow());
                    
                    Console.WriteLine("user number: "+userId+" fuction get cell in row: " + rows + " colm: " + colm + " : " + this.sharableSpreadSheet.getCell(rows, colm));
                }
                else if (functionNumber == 2)
                {

                    int variable = this.sharableSpreadSheet.getCol();

                    Random random = new Random();
                    int colm = random.Next(0, variable);

                    int rows = random.Next(0, this.sharableSpreadSheet.getRow() );
                    Shuffle(animeNames);
                    Console.WriteLine("Set cell " + Thread.CurrentThread.ManagedThreadId );

                    this.sharableSpreadSheet.setCell(rows, colm, animeNames[0]);
                    Console.WriteLine("user number: " + userId + " set cell row: " + rows + " set colm: " + colm + " to a string :" + animeNames[0]);
                }
                else if (functionNumber == 3)
                {
                    Shuffle(animeNames);
                    
                    Console.WriteLine("Search String " + Thread.CurrentThread.ManagedThreadId );

                    this.sharableSpreadSheet.searchString(animeNames[0]);
                    Console.WriteLine("user number: " + userId + " search a  string : " + animeNames[0]);
                }
                else if (functionNumber == 4)
                {
                    Random random = new Random();
                    int row1 = random.Next(0, this.sharableSpreadSheet.getRow());

                    int row2 = random.Next(0, this.sharableSpreadSheet.getRow() );
                    
                    Console.WriteLine("Exchange Rows " + Thread.CurrentThread.ManagedThreadId );

                    this.sharableSpreadSheet.exchangeRows(row1,row2);
                    Console.WriteLine("user number: " + userId + " exchange rows : "+ row1+" , "+row2);

                }
                else if (functionNumber == 5)
                {
                    int variable = this.columns;

                    Random random = new Random();
                    int colm1 = random.Next(0, this.sharableSpreadSheet.getCol());

                    int colm2 = random.Next(0, this.sharableSpreadSheet.getCol());
                    
                    Console.WriteLine("Exchange cols " + Thread.CurrentThread.ManagedThreadId );

                    this.sharableSpreadSheet.exchangeCols( colm1,  colm2);
                    Console.WriteLine("user number: " + userId + " exchange colm : " + colm1 + " , " + colm2);

                }
                else if (functionNumber == 6)
                {
                    Random random = new Random();
                    int row1 = random.Next(0, this.sharableSpreadSheet.getRow());
                    Shuffle(animeNames);
                    Console.WriteLine("Search in row " + Thread.CurrentThread.ManagedThreadId );

                    this.sharableSpreadSheet.searchInRow( row1, animeNames[0]);
                    Console.WriteLine("user number: " + userId + " search in row : " + row1 + " the string " + animeNames[0]);

                }
                else if (functionNumber == 7)
                {
                    Random random = new Random();
                    int col1 = random.Next(0, this.sharableSpreadSheet.getCol());
                    Shuffle(animeNames);
                    Console.WriteLine("search in col " + Thread.CurrentThread.ManagedThreadId );
                    this.sharableSpreadSheet.searchInCol( col1, animeNames[0]);
                    Console.WriteLine("user number: " + userId + " search in colm : " + col1 + " the string" + animeNames[0]);

                }
                else if (functionNumber == 8)
                {
                    Random random = new Random();
                    int col1 = random.Next(0, this.sharableSpreadSheet.getCol()) ;
                    int col2 = random.Next(col1, this.sharableSpreadSheet.getCol());
                    
                    Random random1 = new Random();
                    int row1 = random.Next(0, this.sharableSpreadSheet.getRow());
                    int row2 = random.Next(row1, this.sharableSpreadSheet.getCol());
                    Shuffle(animeNames);
                    
                    Console.WriteLine("Search in range " + Thread.CurrentThread.ManagedThreadId );

                    this.sharableSpreadSheet.searchInRange( col1,  col2,  row1,  row2, animeNames[0]);
                    Console.WriteLine("user number: " + userId + " search in range colm : " + col1+" ,"+ col2 + "search in range rows: "+row1+", "+row2 + " the string " + animeNames[0]);

                }
                else if (functionNumber == 9)
                {
                    Random random = new Random();
                    int row1 = random.Next(0, this.sharableSpreadSheet.getRow());
                    Console.WriteLine("Add row " + Thread.CurrentThread.ManagedThreadId );
                    this.sharableSpreadSheet.addRow( row1);
                    Console.WriteLine("user number : " + userId + "add " + row1 + " rows to the sheet");

                }
                else if (functionNumber == 10)
                {
                    Random random = new Random();
                    int col1 = random.Next(0, this.sharableSpreadSheet.getCol());
                    Console.WriteLine("add col " + Thread.CurrentThread.ManagedThreadId );
                    this.sharableSpreadSheet.addCol( col1);
                    Console.WriteLine("user number : " + userId + "add " + col1 + " colm to the sheet");
                }

                else if (functionNumber == 11)
                {
                    Shuffle(animeNames);
                    Console.WriteLine("Find all " + Thread.CurrentThread.ManagedThreadId );

                    Console.WriteLine("user number : " + userId + "get all the positions of the string " + animeNames[0]+ " : "+ this.sharableSpreadSheet.FindAll(animeNames[0], false));

                }
                else if (functionNumber == 12)
                {
                    String str1 = animeNames[0];
                    
                    Shuffle(animeNames);
                    
                    String str2 = animeNames[0];
                    Console.WriteLine("Set all " + Thread.CurrentThread.ManagedThreadId );

                    this.sharableSpreadSheet.SetAll(str1, str2, false);
                    Console.WriteLine("user number : " + userId + " set all "+str1+" to " +str2);


                }
                else if (functionNumber == 13)
                {
                    Console.WriteLine("Get size " + Thread.CurrentThread.ManagedThreadId );

                    Console.WriteLine("user number :" + userId +" get the size of the sheet : " +this.sharableSpreadSheet.GetSize());

                }
             
            }
            Thread.Sleep(this._msSleep);


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
