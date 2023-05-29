﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Runtime.InteropServices;
using System.IO;

namespace SharableSpreadSheet
{
    class SharableSpreadSheet
    {
        private string[,] m_spreadSheet;
        private Mutex[] m_colMutex;
        private Mutex[] m_rowMutex;
        private Semaphore m_users;
        

        public SharableSpreadSheet(int nRows, int nCols, int nUsers = -1)
        {
            // nUsers used for setConcurrentSearchLimit, -1 mean no limit.
            // construct a nRows*nCols spreadsheet
            m_spreadSheet = new string[nRows, nCols];
            m_colMutex = new Mutex[nCols];
            m_rowMutex = new Mutex[nRows];
            m_users = new Semaphore(0,nUsers);//TODO maybe it should start from zero?

        }
        public string getCell(int row, int col)
        {
            m_users.WaitOne();// TODO maybe the get functions need to use the mutex and not the semaphore
            string value = m_spreadSheet[row, col]; 
            m_users.Release();
            // return the string at [row,col]
            return value;
        }
        public void setCell(int row, int col, string str)
        {
            /**
             * If we lock A,B,C
             * Then we need to release C,B,A*
             */
            
            m_colMutex[col].WaitOne();
            m_rowMutex[row].WaitOne();
            // set the string at [row,col]
            m_spreadSheet[row, col] = str;
            m_rowMutex[row].ReleaseMutex();
            m_colMutex[col].ReleaseMutex();

        }
        public Tuple<int, int> searchString(string str)
        {
            // return first cell indexes that contains the string (search from first row to the last row)
            m_users.WaitOne();
            int row, col;
            for (int i = 0; i < m_spreadSheet.GetLength(0); i++)
            {
                for (int j = 0; j < m_spreadSheet.GetLength(1); j++)
                {
                    if (m_spreadSheet[i, j] == str)
                    {
                        Tuple<int, int> res = new Tuple<int, int>(i, j);
                        m_users.Release();
                        return res;
                    }
                }
            }
            m_users.Release();
            return null;
        }
        public void exchangeRows(int row1, int row2)
        {
            m_rowMutex[row1].WaitOne();
            m_rowMutex[row2].WaitOne();

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

            // exchange the content of row1 and row2
            m_rowMutex[row2].ReleaseMutex();
            m_rowMutex[row1].ReleaseMutex();
        }
        public void exchangeCols(int col1, int col2)
        {
            m_rowMutex[col1].WaitOne();
            m_rowMutex[col2].WaitOne();
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
            // exchange the content of col1 and col2
            m_rowMutex[col2].ReleaseMutex();
            m_rowMutex[col1].ReleaseMutex();
        }
        
        public int searchInRow(int row, string str)
        {
            m_users.WaitOne();

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
                    m_users.Release();
                    return res;
                }
            }
            // perform search in specific row
            m_users.Release();
            return -1;
        }
        
        public int searchInCol(int col, string str)
        {
            m_users.WaitOne();
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
                    m_users.Release();
                    return i;
                }
            }
            // perform search in specific col
            m_users.Release();

            return -1;
        }
        public Tuple<int, int> searchInRange(int col1, int col2, int row1, int row2, string str)
        {
            m_users.WaitOne();

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
                        m_users.Release();
                        Tuple<int, int> res = new Tuple<int, int>(i, j);
                        return res;
                    }
                }
            }
            m_users.Release();

            return null;
        }
        public void addRow(int row1)
        {
            for (int i = 0; i < m_colMutex.Length; i++)
            {
                m_colMutex[i].WaitOne();
            }
            
            for (int i = 0; i < m_rowMutex.Length; i++)
            {
                m_rowMutex[i].WaitOne();
            }
            // add a row after row1
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

            for (int i = m_rowMutex.Length - 1; i >= 0; i--)
            {
                m_rowMutex[i].ReleaseMutex();
            }
            
            for (int i = m_colMutex.Length - 1; i >= 0; i--)
            {
                m_colMutex[i].ReleaseMutex();
            }
        }


        public void addCol(int col1)
        {
            for (int i = 0; i < m_colMutex.Length; i++)
            {
                m_colMutex[i].WaitOne();
            }
            
            for (int i = 0; i < m_rowMutex.Length; i++)
            {
                m_rowMutex[i].WaitOne();
            }
            // add a column after col1
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
            
            for (int i = m_rowMutex.Length - 1; i >= 0; i--)
            {
                m_rowMutex[i].ReleaseMutex();
            }
            
            for (int i = m_colMutex.Length - 1; i >= 0; i--)
            {
                m_colMutex[i].ReleaseMutex();
            }
        }
        public Tuple<int, int>[] FindAll(string str, bool caseSensitive)
        {
            m_users.WaitOne();
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

            m_users.Release();
            return cellList.ToArray();
        }

        public void SetAll(string oldStr, string newStr, bool caseSensitive)
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
        
        public Tuple<int, int> GetSize()
        {
            m_users.WaitOne();

            int nRows = m_spreadSheet.GetLength(0);
            int nCols = m_spreadSheet.GetLength(1);

            // return the size of the spreadsheet in nRows, nCols
            Tuple<int, int> res = new Tuple<int, int>(nRows, nCols);
            
            m_users.Release();
            
            return res;
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
            for (int i = 0; i < m_colMutex.Length; i++)
            {
                m_colMutex[i].WaitOne();
            }
            
            for (int i = 0; i < m_rowMutex.Length; i++)
            {
                m_rowMutex[i].WaitOne();
            }
            
            string filePath = "C:/Users/" + Environment.UserName + "/Desktop/" + fileName;

            // Check if the file exists
            if (File.Exists(filePath))
            {
                using (StreamReader fileReader = new StreamReader(filePath))
                {
                    string line;
                    int rowIndex = 0;

                    // Read lines from the file
                    while ((line = fileReader.ReadLine()) != null)
                    {
                        // Split the line into values
                        string[] values = line.Split(' ');

                        // Iterate over the values and parse them
                        for (int columnIndex = 0; columnIndex < values.Length; columnIndex++)
                        {
                            // Parse the value and assign it to the spreadsheet

                            m_spreadSheet[rowIndex, columnIndex] = values[columnIndex];
                        }

                        rowIndex++;
                    }
                }

                Console.WriteLine("Spreadsheet loaded successfully.");
            }
            else
            {
                Console.WriteLine("File not found: " + fileName);
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
    }
}