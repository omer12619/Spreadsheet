using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public Simulator(int rows, int columns, int usres, int nOperations, int mssleep)
        {
            this.rows = rows;
            this.columns = columns;
            sharableSpreadSheet = new SharableSpreadSheet(rows,columns,usres);
            sharableSpreadSheet.load("TextFile1.txt");
            this.usres = usres;
            this.nOperations = nOperations;
            this.mssleep = mssleep;
        }
    }
}
