using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Runtime.InteropServices;
using System.IO;

namespace Simulator
{
    class SharableSpreadSheet
    {
        private string[,] m_spreadSheet;
     

        private ReaderWriterLockSlim[] m_readerWriterRows;
        private ReaderWriterLock pen;

        public SharableSpreadSheet(int nRows, int nCols, int nUsers = -1)
        {
            
            m_spreadSheet = new string[nRows, nCols];
            
            m_readerWriterRows = new ReaderWriterLockSlim[nRows];

            this.pen = new ReaderWriterLock();
            
            for (int i = 0; i < nRows; i++)
            {
                m_readerWriterRows[i] = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            }

        }
        public string getCell(int row, int col)
        {
            m_readerWriterRows[row].EnterReadLock();
            try
            {
                string value = m_spreadSheet[row, col];
                return value;
            }
            finally
            {
                m_readerWriterRows[row].ExitReadLock();
            }
        }
        public void setCell(int row, int col, string str)
        {
           
            
            m_readerWriterRows[row].EnterWriteLock();
            m_spreadSheet[row, col] = str;
            m_readerWriterRows[row].ExitWriteLock();


        }
        public Tuple<int, int> searchString(string str)
        {
           
            for (int i = 0; i < m_readerWriterRows.Length; i++)
            {
                m_readerWriterRows[i].EnterReadLock();
            }
            
            int row, col;
            for (int i = 0; i < m_spreadSheet.GetLength(0); i++)
            {
                for (int j = 0; j < m_spreadSheet.GetLength(1); j++)
                {
                    if (m_spreadSheet[i, j] == str)
                    {
                        Tuple<int, int> res = new Tuple<int, int>(i, j);
                          
                        for (int t = m_readerWriterRows.Length - 1; t >= 0; t--)
                        {
                            m_readerWriterRows[t].ExitReadLock();
                        }
                        return res;
                    }
                }
            }

            for (int t = m_readerWriterRows.Length - 1; t >= 0; t--)
            {
                m_readerWriterRows[t].ExitReadLock();
            }

         
            return null;
        }
        public void exchangeRows(int row1, int row2)
        {
         
            m_readerWriterRows[row1].EnterWriteLock();
            m_readerWriterRows[row2].EnterWriteLock();
            
           
                string[] rowA = new string[m_spreadSheet.GetLength(1)];
                string[] rowB = new string[m_spreadSheet.GetLength(1)];

                for (int i = 0; i < m_spreadSheet.GetLength(1); i++)
                {
                    rowA[i] = m_spreadSheet[row1, i];
                    rowB[i] = m_spreadSheet[row2, i];
                }

                for (int i = 0; i < rowA.Length; i++)
                {
                    string a = rowA[i];
                    string b = rowB[i];
                    m_spreadSheet[row1, i] = b;
                    m_spreadSheet[row2, i] = a;
                }
          
                m_readerWriterRows[row2].ExitWriteLock();
                m_readerWriterRows[row1].ExitWriteLock();
         
        }
        public void exchangeCols(int col1, int col2)
        {
            
            for (int i = 0; i < m_readerWriterRows.Length; i++)
            {
                m_readerWriterRows[i].EnterWriteLock();
            }

            try
            {
                string[] colA = new string[m_spreadSheet.GetLength(0)];
                string[] colB = new string[m_spreadSheet.GetLength(0)];

                for (int i = 0; i < m_spreadSheet.GetLength(0); i++)
                {
                    colA[i] = m_spreadSheet[i, col1];
                    colB[i] = m_spreadSheet[i, col2];
                }

                for (int i = 0; i < colA.Length; i++)
                {
                    string a = colA[i];
                    string b = colB[i];
                    m_spreadSheet[i, col1] = b;
                    m_spreadSheet[i, col2] = a;
                }
            }
            finally
            {
                
                for (int i = m_readerWriterRows.Length - 1; i >= 0; i--)
                {
                    m_readerWriterRows[i].ExitWriteLock();
                }
            }
        }


        public int searchInRow(int row, string str)
        {
            
            m_readerWriterRows[row].EnterReadLock();
           
                int col;
                string[] searchRow = new string[m_spreadSheet.GetLength(1)];
                for (int i = 0; i < m_spreadSheet.GetLength(1); i++)
                {
                    searchRow[i] = m_spreadSheet[row, i];
                }

                for (int i = 0; i < searchRow.Length; i++)
                {
                    if (searchRow[i].Equals(str))
                    {
                        int res = i;
                       
                        m_readerWriterRows[row].ExitReadLock();
                        return res;
                    }
                }
          
                m_readerWriterRows[row].ExitReadLock();     
          
            
            return -1;
        }
        
        public int searchInCol(int col, string str)
        {
          
            
            for(int i = 0; i < m_readerWriterRows.Length; i++)
            {
                m_readerWriterRows[i].EnterReadLock();
            }

           
                int row;
                string[] searchCol = new string[m_spreadSheet.GetLength(0)];
                for (int i = 0; i < m_spreadSheet.GetLength(0); i++)
                {
                    searchCol[i] = m_spreadSheet[i, col];
                }

                for (int i = 0; i < searchCol.Length; i++)
                {
                    if (searchCol[i].Equals(str))
                    {
                        int res = i;
                       
                        for (int k = m_readerWriterRows.Length - 1; k >= 0; k--)
                        {
                            m_readerWriterRows[k].ExitReadLock();
                        }
                        return res;
                    }
                }
            
                for (int i = m_readerWriterRows.Length - 1; i >= 0; i--)
                {
                    m_readerWriterRows[i].ExitReadLock();
                }
           

            return -1;
        }
        public Tuple<int, int> searchInRange(int col1, int col2, int row1, int row2, string str)
        {
          


            for(int i = row1; i <= row2; i++)
            {
                m_readerWriterRows[i].EnterReadLock();
            }

          
                int row, col;
               
                string[,] searchMatrix = new string[col2 - col1, row2 - row1];
                for (int i = col1; i <= col2; i++)
                {
                    for (int j = row1; j <= row2; j++)
                    {
                        if (m_spreadSheet[j, i].Equals(str))
                        {
                           
                            Tuple<int, int> res = new Tuple<int, int>(i, j);
                            for (int k = row2; k >= row1; k--)
                            {
                                m_readerWriterRows[k].ExitReadLock();
                            }
                            return res;
                        }
                    }
                }
           
            for (int k = row2; k >= row1; k--)
            {
                m_readerWriterRows[k].ExitReadLock();
            }
           

            return null;
        }
        public void addRow(int row1)
        {
           
            
            for(int i = 0; i < m_readerWriterRows.Length; i++)
            {
                m_readerWriterRows[i].EnterWriteLock();
            }

           
                int numRows = m_spreadSheet.GetLength(0);
                int numCols = m_spreadSheet.GetLength(1);

                string[,] newSpreadSheet = new string[numRows + 1, numCols];

                for (int i = 0; i < numRows + 1; i++)
                {
                    if (i < row1 + 1)
                    {
                        for (int j = 0; j < numCols; j++)
                        {
                            newSpreadSheet[i, j] = m_spreadSheet[i, j];
                        }
                    }
                    else if (i > row1)
                    {
                        for (int j = 0; j < numCols; j++)
                        {
                            newSpreadSheet[i, j] = m_spreadSheet[i - 1, j];
                        }
                    }
                }

                for (int i = 0; i < newSpreadSheet.GetLength(1); i++)
                {
                    newSpreadSheet[row1, i] = "new Row";
                }
                
                m_spreadSheet = newSpreadSheet;
                ReaderWriterLockSlim[] newLocks = new ReaderWriterLockSlim[m_readerWriterRows.Length + 1];
               
            
                for (int i = 0; i < m_readerWriterRows.Length; i++)
                {
                    newLocks[i] = m_readerWriterRows[i];
                }

                newLocks[newLocks.Length - 1] = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
                m_readerWriterRows = newLocks;
              
                for (int i = m_readerWriterRows.Length - 2; i >= 0; i--)
                {
                    m_readerWriterRows[i].ExitWriteLock();
                }
          
        }


        public void addCol(int col1)
        {
          
            for(int i = 0; i < m_readerWriterRows.Length; i++)
            {
                m_readerWriterRows[i].EnterWriteLock();
            }

           
                int numRows = m_spreadSheet.GetLength(0);
                int numCols = m_spreadSheet.GetLength(1);

                string[,] newSpreadSheet = new string[numRows, numCols + 1];

                for (int i = 0; i < numRows; i++)
                {
                    for (int j = 0; j < numCols + 1; j++)
                    {
                        if (j < col1 + 1)
                        {
                            newSpreadSheet[i, j] = m_spreadSheet[i, j];
                        }
                        else if (j > col1)
                        {
                            newSpreadSheet[i, j] = m_spreadSheet[i, j - 1];
                        }
                    }
                }
                
                for (int i = 0; i < newSpreadSheet.GetLength(0); i++)
                {
                    newSpreadSheet[i, col1] = "new Row";
                }

                m_spreadSheet = newSpreadSheet;

                for (int i = m_readerWriterRows.Length - 1; i >= 0; i--)
                {
                    m_readerWriterRows[i].ExitWriteLock();
                }
            
        }
        public Tuple<int, int>[] FindAll(string str, bool caseSensitive)
        {
          
            for(int i = 0; i < m_readerWriterRows.Length; i++)
            {
                m_readerWriterRows[i].EnterReadLock();
            }

                List<Tuple<int, int>> cellList = new List<Tuple<int, int>>();

                for (int i = 0; i < m_spreadSheet.GetLength(0); i++)
                {
                    for (int j = 0; j < m_spreadSheet.GetLength(1); j++)
                    {
                        string cellValue = m_spreadSheet[i, j];

                        if (caseSensitive)
                        {
                            if (cellValue == str)
                            {
                                cellList.Add(Tuple.Create(i, j));
                            }
                        }
                        else
                        {
                            if (cellValue.Equals(str, StringComparison.OrdinalIgnoreCase))
                            {
                                cellList.Add(Tuple.Create(i, j));
                            }
                        }
                    }
                }
                Tuple<int, int>[] res = cellList.ToArray();
                for (int i = m_readerWriterRows.Length - 1; i >= 0; i--)
                {
                    m_readerWriterRows[i].ExitReadLock();
                }
                
                return res;
           
        }

        public void SetAll(string oldStr, string newStr, bool caseSensitive)
        {
            for(int i = 0; i < m_readerWriterRows.Length; i++)
            {
                m_readerWriterRows[i].EnterWriteLock();
            }

           
                for (int i = 0; i < m_spreadSheet.GetLength(0); i++)
                {
                    for (int j = 0; j < m_spreadSheet.GetLength(1); j++)
                    {
                        string cellValue = m_spreadSheet[i, j];

                        if (caseSensitive)
                        {
                            if (cellValue == oldStr)
                            {
                                m_spreadSheet[i, j] = newStr;
                            }
                        }
                        else
                        {
                            if (cellValue.Equals(oldStr, StringComparison.OrdinalIgnoreCase))
                            {
                                m_spreadSheet[i, j] = newStr;
                            }
                        }
                    }
                }
          
                for (int i = m_readerWriterRows.Length - 1; i >= 0; i--)
                {
                    m_readerWriterRows[i].ExitWriteLock();
                }
          
        }
        
        public Tuple<int, int> GetSize()
        {
          
            for(int i = 0; i < m_readerWriterRows.Length; i++)
            {
                m_readerWriterRows[i].EnterReadLock();
            }

            
                int nRows = m_spreadSheet.GetLength(0);
                int nCols = m_spreadSheet.GetLength(1);

               
                Tuple<int, int> res = new Tuple<int, int>(nRows, nCols);
            
               
                
                for (int i = m_readerWriterRows.Length - 1; i >= 0; i--)
                {
                    m_readerWriterRows[i].ExitReadLock();
                }
                return res;
            

        }

        public void Save(string fileName)
        {
           
            string currentDirectory = Directory.GetCurrentDirectory();
            DirectoryInfo parentDirectory = Directory.GetParent(currentDirectory)?.Parent;
            string filePath = parentDirectory.FullName + "\\" + fileName;
         
            using (StreamWriter fileWriter = new StreamWriter(filePath))
            {
                
                for (int i = 0; i < m_spreadSheet.GetLength(0); i++)
                {
                    for (int j = 0; j < m_spreadSheet.GetLength(1); j++)
                    {
                        fileWriter.WriteLine("" + m_spreadSheet[i, j] + " ");
                    }
                    fileWriter.WriteLine(Environment.NewLine);

                }
            }
            
         
        }
        public void load(string fileName)
        {
            
            
            string currentDirectory = Directory.GetCurrentDirectory();
            DirectoryInfo parentDirectory = Directory.GetParent(currentDirectory)?.Parent;
            string filePath = parentDirectory.FullName + "\\" + fileName;



            
            if (File.Exists(filePath))
            {
                StreamReader fileReader = new StreamReader(filePath);
                
                string line;
                int rowIndex = 0; 
                int numOfRows = 0;
                int numOfCols = 0;
                while ((line = fileReader.ReadLine()) != null)
                {
                    numOfRows++;
                    string[] values = line.Split(',');
                    if (values.Length > numOfCols)
                    {
                        numOfCols = values.Length;
                    }

                }

                m_spreadSheet = new string[numOfRows, numOfCols];
                   
                fileReader = new StreamReader(filePath);
                for (int i = 0; i < numOfRows; i++)
                {
                    line = fileReader.ReadLine();
                    string[] values = line.Split(',');
                    for (int j = 0; j < values.Length; j++)
                    {
                        m_spreadSheet[i, j] = values[j];
                    }
                }

                Console.WriteLine("Spreadsheet loaded successfully.");
            }
            
            else
            {
                Console.WriteLine("File not found: " + fileName);
            }

            
        }

        public int getCol()
        {
            int res = 0;
            lock (pen)
            {

                 res = m_spreadSheet.GetLength(1);
            }
           
            return res;
        }

        public int getRow()
        {
            int res = 0;
            lock (pen)
            {

                 res = m_spreadSheet.GetLength(0);
            }
           

            return res;
        }
    }
}