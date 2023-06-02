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
        private Mutex[] m_colMutex;
        private Mutex[] m_rowMutex;
        private Semaphore m_users;

        private ReaderWriterLockSlim[] m_readerWriterRows;// TODO replace the mutex arrays with this one 
        

        public SharableSpreadSheet(int nRows, int nCols, int nUsers = -1)
        {
            // nUsers used for setConcurrentSearchLimit, -1 mean no limit.
            // construct a nRows*nCols spreadsheet
            m_spreadSheet = new string[nRows, nCols];
            m_colMutex = new Mutex[nCols];
            m_readerWriterRows = new ReaderWriterLockSlim[nRows];
            for (int i = 0; i < nCols; i++)
            {
                m_colMutex[i] = new Mutex();
            }
            m_rowMutex = new Mutex[nRows];
            
            for (int i = 0; i < nRows; i++)
            {
                m_rowMutex[i] = new Mutex();
            }
            m_users = new Semaphore(nUsers,nUsers);//TODO maybe it should start from zero?
            
            for (int i = 0; i < nRows; i++)
            {
                m_readerWriterRows[i] = new ReaderWriterLockSlim();
            }

        }
        public string getCell(int row, int col)
        {
            /*m_users.WaitOne();// TODO maybe the get functions need to use the mutex and not the semaphore
            string value = m_spreadSheet[row, col]; 
            m_users.Release();*/
            // return the string at [row,col]

            try
            {
                m_readerWriterRows[row].EnterReadLock();
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
            /**
             * If we lock A,B,C
             * Then we need to release C,B,A*
             */
            
            /*m_colMutex[col].WaitOne();
            m_rowMutex[row].WaitOne();*/
            // set the string at [row,col]
            m_readerWriterRows[row].EnterWriteLock();
            try
            {
                m_spreadSheet[row, col] = str;

            }
            finally
            {
                m_readerWriterRows[row].ExitWriteLock();

            }
            /*m_rowMutex[row].ReleaseMutex();
            m_colMutex[col].ReleaseMutex();*/

        }
        public Tuple<int, int> searchString(string str)
        {
            // return first cell indexes that contains the string (search from first row to the last row)
            
            /*
            m_users.WaitOne();
            */
            for (int i = 0; i < m_readerWriterRows.Length; i++)
            {
                m_readerWriterRows[i].EnterReadLock();
            }

            try
            {
                int row, col;
                for (int i = 0; i < m_spreadSheet.GetLength(0); i++)
                {
                    for (int j = 0; j < m_spreadSheet.GetLength(1); j++)
                    {
                        if (m_spreadSheet[i, j] == str)
                        {
                            Tuple<int, int> res = new Tuple<int, int>(i, j);
                            /*
                            m_users.Release();
                            */
                            return res;
                        }
                    }
                }
            }
            finally
            {
                for (int t = m_readerWriterRows.Length - 1; t >= 0; t--)
                {
                    m_readerWriterRows[t].ExitReadLock();
                }
            }

            /*
            m_users.Release();
            */
            return null;
        }
        public void exchangeRows(int row1, int row2)
        {
            m_readerWriterRows[row1].EnterWriteLock();
            m_readerWriterRows[row2].EnterWriteLock();

            /*
            m_rowMutex[row1].WaitOne();
            m_rowMutex[row2].WaitOne();
            */
            try
            {
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
            }
            finally
            {
                m_readerWriterRows[row2].ExitWriteLock();
                m_readerWriterRows[row1].ExitWriteLock();
            }
            
            /*m_rowMutex[row2].ReleaseMutex();
            m_rowMutex[row1].ReleaseMutex();*/

            // exchange the content of row1 and row2
        }
        public void exchangeCols(int col1, int col2)
        {
            /*m_rowMutex[col1].WaitOne();
            m_rowMutex[col2].WaitOne();*/

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
                    m_spreadSheet[i, col1] = a;
                }
            }
            
            finally
            {
                for (int i = m_readerWriterRows.Length - 1; i >= 0; i--)
                {
                    m_readerWriterRows[i].ExitWriteLock();
                }
            }
            // exchange the content of col1 and col2
            // m_rowMutex[col2].ReleaseMutex();
            // m_rowMutex[col1].ReleaseMutex();
        }
        
        public int searchInRow(int row, string str)
        {
            /*
            m_readerWriterRows[row].EnterReadLock();
            */
            /*
            m_users.WaitOne();
            */
            
            m_readerWriterRows[row].EnterReadLock();
            try
            {
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
                        /*
                        m_users.Release();
                        */
                        return res;
                    }
                }
            }
            finally
            {
                m_readerWriterRows[row].ExitReadLock();     
            }
            
            // perform search in specific row
            /*
            m_users.Release();
            */
            
            return -1;
        }
        
        public int searchInCol(int col, string str)
        {
            /*
            m_users.WaitOne();
            */
            
            for(int i = 0; i < m_readerWriterRows.Length; i++)
            {
                m_readerWriterRows[i].EnterReadLock();
            }

            try
            {
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
                        /*
                        m_users.Release();
                        */
                        return i;
                    }
                }
                // perform search in specific col
                /*
                m_users.Release();
            */
            }
            
            finally
            {
                for (int i = m_readerWriterRows.Length - 1; i >= 0; i--)
                {
                    m_readerWriterRows[i].ExitReadLock();
                }
            }

            return -1;
        }
        public Tuple<int, int> searchInRange(int col1, int col2, int row1, int row2, string str)
        {
            /*
            m_users.WaitOne();
            */


            for(int i = 0; i < m_readerWriterRows.Length; i++)
            {
                m_readerWriterRows[i].EnterReadLock();
            }

            try
            {
                int row, col;
                // perform search within spesific range: [row1:row2,col1:col2] 
                //includes col1,col2,row1,row2
                string[,] searchMatrix = new string[col2 - col1, row2 - row1];
                for (int i = col1; i <= col2; i++)
                {
                    for (int j = row1; j <= row2; j++)
                    {
                        if (m_spreadSheet[i, j].Equals(str))
                        {
                            /*
                            m_users.Release();
                            */
                            Tuple<int, int> res = new Tuple<int, int>(i, j);
                            return res;
                        }
                    }
                }
            }
            
            finally
            {
                for (int i = m_readerWriterRows.Length - 1; i >= 0; i--)
                {
                    m_readerWriterRows[i].ExitReadLock();
                }
            }

            /*
            m_users.Release();
            */

            return null;
        }
        public void addRow(int row1)
        {
            /*for (int i = 0; i < m_colMutex.Length; i++)
            {
                m_colMutex[i].WaitOne();
            }
            
            for (int i = 0; i < m_rowMutex.Length; i++)
            {
                m_rowMutex[i].WaitOne();
            }*/
            // add a row after row1
            
            for(int i = 0; i < m_readerWriterRows.Length; i++)
            {
                m_readerWriterRows[i].EnterWriteLock();
            }

            try
            {
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

                m_spreadSheet = newSpreadSheet;
                ReaderWriterLockSlim[] newLocks = new ReaderWriterLockSlim[m_readerWriterRows.Length + 1];
                /*
                Mutex[] newLocks = new Mutex[m_rowMutex.Length + 1];
                */
            
                for (int i = 0; i < m_readerWriterRows.Length; i++)
                {
                    newLocks[i] = m_readerWriterRows[i];
                }

                newLocks[newLocks.Length - 1] = new ReaderWriterLockSlim();
                m_readerWriterRows = newLocks;
                /*
                m_rowMutex = newLocks;
            */
            }
        
            finally
            {
                for (int i = m_readerWriterRows.Length - 2; i >= 0; i--)
                {
                    m_readerWriterRows[i].ExitWriteLock();
                }
            }
            
            /*for (int i = m_rowMutex.Length - 2; i >= 0; i--)
            {
                m_rowMutex[i].ReleaseMutex();
            }
            
            for (int i = m_colMutex.Length - 1; i >= 0; i--)
            {
                m_colMutex[i].ReleaseMutex();
            }*/
        }


        public void addCol(int col1)
        {
            /*for (int i = 0; i < m_colMutex.Length; i++)
            {
                m_colMutex[i].WaitOne();
            }
            
            for (int i = 0; i < m_rowMutex.Length; i++)
            {
                m_rowMutex[i].WaitOne();
            }*/
            // add a column after col1
            for(int i = 0; i < m_readerWriterRows.Length; i++)
            {
                m_readerWriterRows[i].EnterWriteLock();
            }

            try
            {
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

                m_spreadSheet = newSpreadSheet;

                // Mutex[] newLocks = new Mutex[m_colMutex.Length + 1];
                //
                // for (int i = 0; i < m_colMutex.Length; i++)
                // {
                //     newLocks[i] = m_colMutex[i];
                // }
                //
                // newLocks[newLocks.Length - 1] = new Mutex();
                //
                // m_colMutex = newLocks;
            }

            finally
            {
                for (int i = m_readerWriterRows.Length - 1; i >= 0; i--)
                {
                    m_readerWriterRows[i].ExitWriteLock();
                }
            }
            
            // for (int i = m_rowMutex.Length - 1; i >= 0; i--)
            // {
            //     m_rowMutex[i].ReleaseMutex();
            // }
            //
            // for (int i = m_colMutex.Length - 2; i >= 0; i--)
            // {
            //     m_colMutex[i].ReleaseMutex();
            // }
        }
        public Tuple<int, int>[] FindAll(string str, bool caseSensitive)
        {
            /*
            m_users.WaitOne();
            */
            
            for(int i = 0; i < m_readerWriterRows.Length; i++)
            {
                m_readerWriterRows[i].EnterReadLock();
            }

            try
            {
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
                return cellList.ToArray();
            }
            
            finally
            {
                for (int i = m_readerWriterRows.Length - 1; i >= 0; i--)
                {
                    m_readerWriterRows[i].ExitReadLock();
                }
            }
            
            /*
            m_users.Release();
            */
        }

        public void SetAll(string oldStr, string newStr, bool caseSensitive)
        {
            for(int i = 0; i < m_readerWriterRows.Length; i++)
            {
                m_readerWriterRows[i].EnterWriteLock();
            }

            try
            {
                // replace all oldStr cells with the newStr str according to caseSensitive param
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
            }
            
            finally
            {
                for (int i = m_readerWriterRows.Length - 1; i >= 0; i--)
                {
                    m_readerWriterRows[i].ExitWriteLock();
                }
            }
        }
        
        public Tuple<int, int> GetSize()
        {
            /*
            m_users.WaitOne();
            */
            
            for(int i = 0; i < m_readerWriterRows.Length; i++)
            {
                m_readerWriterRows[i].EnterReadLock();
            }

            try
            {
                int nRows = m_spreadSheet.GetLength(0);
                int nCols = m_spreadSheet.GetLength(1);

                // return the size of the spreadsheet in nRows, nCols
                Tuple<int, int> res = new Tuple<int, int>(nRows, nCols);
            
                /*
                m_users.Release();
                */
            
                return res;
            }
            
            finally
            {
                for (int i = m_readerWriterRows.Length - 1; i >= 0; i--)
                {
                    m_readerWriterRows[i].ExitReadLock();
                }
            }

        }

        public void Save(string fileName)
        {
            for (int i = 0; i < m_colMutex.Length; i++)
            {
                m_colMutex[i].WaitOne();
            }
            
            for (int i = 0; i < m_rowMutex.Length; i++)
            {
                m_rowMutex[i].WaitOne();
            }
            // Specify the file name
            string filePath = "C:/Users/" + Environment.UserName + "/Desktop/" + fileName;

            // Save the spreadsheet to a file
            using (StreamWriter fileWriter = new StreamWriter(filePath))
            {
                // Write array to file
                for (int i = 0; i < m_spreadSheet.GetLength(0); i++)
                {
                    for (int j = 0; j < m_spreadSheet.GetLength(1); j++)
                    {
                        fileWriter.WriteLine("" + m_spreadSheet[i, j] + " ");
                    }
                    fileWriter.WriteLine(Environment.NewLine);

                }
            }
            
            for (int i = m_rowMutex.Length - 1; i >= 0; i--)
            {
                m_rowMutex[i].ReleaseMutex();
            }
            
            for (int i = m_colMutex.Length - 1; i >= 0; i--)
            {
                m_colMutex[i].ReleaseMutex();
            }
        }
        public void load(string fileName)
        {
            // for (int i = 0; i < m_colMutex.Length; i++)
            // {
            //     m_colMutex[i].WaitOne();
            // }
            //
            // for (int i = 0; i < m_rowMutex.Length; i++)
            // {
            //     m_rowMutex[i].WaitOne();
            // }
            
            string currentDirectory = Directory.GetCurrentDirectory();
            DirectoryInfo parentDirectory = Directory.GetParent(currentDirectory)?.Parent;
            string filePath = parentDirectory.FullName + "\\" + fileName;



            // Check if the file exists
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
                    // Read lines from the file
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

            Mutex[] newRowLocks = new Mutex[m_spreadSheet.GetLength(0)];
            Mutex[] newColLocks = new Mutex[m_spreadSheet.GetLength(1)];

            for (int i = 0; i < newRowLocks.Length; i++)
            {
                newRowLocks[i] = new Mutex();
            }
            
            for (int i = 0; i < newColLocks.Length; i++)
            {
                newColLocks[i] = new Mutex();
            }

            m_colMutex = newColLocks;
            m_rowMutex = newRowLocks;
            // for (int i = m_rowMutex.Length - 1; i >= 0; i--)
            // {
            //     m_rowMutex[i].ReleaseMutex();
            // }
            //
            // for (int i = m_colMutex.Length - 1; i >= 0; i--)
            // {
            //     m_colMutex[i].ReleaseMutex();
            // }
        }

        public int getCol()
        {
            m_users.WaitOne();
            int res =  m_spreadSheet.GetLength(1);
            m_users.Release();

            return res;
        }

        public int getRow()
        {
            m_users.WaitOne();
            int res =  m_spreadSheet.GetLength(0);
            m_users.Release();
            return res;
        }
    }
}