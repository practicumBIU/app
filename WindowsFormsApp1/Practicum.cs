using iTextSharp.text.pdf.parser;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;

using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Table;
using System.Text.RegularExpressions;


namespace WindowsFormsApp1
{
    public partial class Practicum : Form
    {
        static ExcelPackage excel = new ExcelPackage();
        static ExcelWorksheet wsSheet1 = excel.Workbook.Worksheets.Add("Sheet1");
       
        public string firstLetterOfTeamName;
        public int totalSurvey;
        public int numberOfRounds = 8;
        public int round;
        public bool Button2Click = false;
        public bool Button1Click = false;
        public Dictionary<int, string> pdfNames = new Dictionary<int, string>(){
                    {5, "Traditional"},
                    {6, "Low End" },
                    {7, "High End" },
                    {8, "Performance" },
                    {9, "Size" }
                };
        public List<Color> colorsList = new List<Color>(){
            Color.Red ,
            Color.Green ,
            Color.Blue ,
            Color.Purple ,
            Color.Brown
        };

        public Dictionary<string, string> GrothRate = new Dictionary<string, string>();
        public Dictionary<string, int> IndustryUnitDemand = new Dictionary<string, int>();
        public Dictionary<string, string> d_Age = new Dictionary<string, string>();
        public Dictionary<string, string> d_mtbf = new Dictionary<string, string>();
        public Dictionary<string, Double> d_pfmn = new Dictionary<string, Double>();
        public Dictionary<string, Double> d_size = new Dictionary<string, Double>();
        Dictionary<string, Dictionary<string, double>> SegmentSurveyRate = new Dictionary<string, Dictionary<string, double>>();
        public Dictionary<string, List<Double>> IdealSpotGrothRate = new Dictionary<string, List<Double>>();
        List<string> segments = new List<string>(new string[] { "Traditional", "Low End", "High End", "Performance", "Size" });


        public Practicum()
        {
            InitializeComponent();
            richTextBox.Text = "Hello and welcome to the best app ever.\n\r" +
                "The app contain 2 undependant parts: \n" +
                "\t 1. It creates excel file with some tables that contain details on the market now and in the feature.\n" +
                "\t 2 It produces estimation of amount of units your company will sale at the next round (help with the Marketing section).\n\r" +
                "For creating the first excel file, please open the condition and courier reports (the 2 buttons of from the left).\n" +
                "If you want the prediction of units for next round, fill the text box click the buttons of the right area.";
        }

        private void RichTextBox1_TextChanged(object sender, EventArgs e)
        {
        }


        private void Button4_Click(object sender, EventArgs e)
        {
            if (Button1Click == true)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "pdf files (*.pdf)|*.pdf|All files (*.*)|*.*";
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string finalWordInPath = ofd.FileName.Substring(ofd.FileName.LastIndexOf("\\", ofd.FileName.Length));
                    string folderPath = ofd.FileName.Replace(finalWordInPath, "") + "\\";
                    richTextBox.Text = folderPath;

                    SplitCourier(ofd.FileName, folderPath);
                    richTextBox.Text = "step 2 done, The courier report succecfully uploeaded. \nThe excel saved in the same folder of the pdf \n\r";

                    excelmainpu();
                }

            }
            else
            {
                MessageBox.Show("Please provide courier file first by clicking on the first step button");
            }
        }


        public string SplitCourier(string pdfFilePath, string outputPath)
        {

            StringBuilder text = new StringBuilder();
            PdfReader reader = new PdfReader(pdfFilePath);
            ITextExtractionStrategy its = new iTextSharp.text.pdf.parser.LocationTextExtractionStrategy();

            for (int pageNumber = 5; pageNumber <= 9; pageNumber += 1)
            {

                Document document = new Document();
                string currSegment = pdfNames[pageNumber];

                //Total Industry Unit Demand:
                string thePage = PdfTextExtractor.GetTextFromPage(reader, pageNumber);

                string toBeSearched2 = "Total Industry Unit Demand ";
                string industryUnitDemand = thePage.Substring(thePage.IndexOf(toBeSearched2) + toBeSearched2.Length);
                string tempIndustryUnitDemand = industryUnitDemand.Substring(0, industryUnitDemand.IndexOf(" "));
                tempIndustryUnitDemand = tempIndustryUnitDemand.Remove(tempIndustryUnitDemand.Length - 1);
                tempIndustryUnitDemand = Regex.Replace(tempIndustryUnitDemand, "[^0-9]", "");
                IndustryUnitDemand[currSegment] = Convert.ToInt32(tempIndustryUnitDemand);

                //Next Year's Segment Growth Rate :
                string toBeSearched = "Next Year's Segment Growth Rate |";
                string grothRate = thePage.Substring(thePage.IndexOf(toBeSearched) + toBeSearched.Length);
                GrothRate[currSegment] = grothRate.Split(new[] { '\r', '\n' }).FirstOrDefault();

                //age:
                toBeSearched = "Age = ";
                string age = thePage.Substring(thePage.IndexOf(toBeSearched) + toBeSearched.Length);
                age = age.Substring(0, age.IndexOf(" "));
                d_Age[currSegment] = age.Split(new[] { '\r', '\n' }).FirstOrDefault();

                //mtbf:
                toBeSearched = "MTBF ";
                string mtbf = thePage.Substring(thePage.IndexOf(toBeSearched) + toBeSearched.Length);
                mtbf = mtbf.Substring(0, mtbf.IndexOf(" "));
                mtbf = Regex.Replace(mtbf, "0", "");
                d_mtbf[currSegment] = mtbf.Split(new[] { '\r', '\n' }).FirstOrDefault();

                //pfmn:
                toBeSearched = "Pfmn ";
                string Pfmn = thePage.Substring(thePage.IndexOf(toBeSearched) + toBeSearched.Length);
                Pfmn = Pfmn.Substring(0, Pfmn.IndexOf(" "));
                d_pfmn[currSegment] = Convert.ToDouble(Pfmn.Split(new[] { '\r', '\n' }).FirstOrDefault());

                //size:
                toBeSearched = " Size ";
                string Size = thePage.Substring(thePage.IndexOf(toBeSearched) + toBeSearched.Length);
                Size = Size.Substring(0, Size.IndexOf(" "));
                d_size[currSegment] = Convert.ToDouble(Size.Split(new[] { '\r', '\n' }).FirstOrDefault());

                //get round:
                toBeSearched = " Round: ";
                string Round = thePage.Substring(thePage.IndexOf(toBeSearched) + toBeSearched.Length);
                round = Convert.ToInt32(Round.Split(new[] { '\r', '\n' }).FirstOrDefault());


                if (Button2Click)
                {
                    Dictionary<string, int> ourUserProducts = new Dictionary<string, int>();
                    Dictionary<string, double> tempOurUserProducts = new Dictionary<string, double>();
                    toBeSearched = "Survey\n";
                    string str = thePage.Substring(thePage.IndexOf(toBeSearched) + toBeSearched.Length);
                    totalSurvey = SumSurvey(str, ourUserProducts);
                    foreach (KeyValuePair<string, int> entry in ourUserProducts)
                    {
                        tempOurUserProducts.Add(entry.Key, (double)entry.Value / (double)totalSurvey);
                    }
                    SegmentSurveyRate.Add(currSegment, tempOurUserProducts);
                }
            }
            return text.ToString();
        }

        public int SumSurvey(string str, Dictionary<string, int> ourUserProducts)
        {

        int sumOfSurvey = 0;
            string[] linesArr = str.Split('\n');

            foreach (string currLine in linesArr)
            {
                string[] parts = currLine.Split(' ');
                if (parts[0] == "CAPSTONE")
                {
                    return sumOfSurvey;
                }
                //last word in line
                string lastWord = parts[parts.Length - 1];
                //parts[0][0] is the first letter in line
                if (parts[0][0] == Convert.ToChar(firstLetterOfTeamName))
                {
                    ourUserProducts.Add(parts[0], Convert.ToInt32(lastWord));
                }

                sumOfSurvey += Convert.ToInt32(lastWord);
            }
            return sumOfSurvey;

        }
        public void excelmainpu()
        {
            DrawTableUnitPerRounds();
            DrawTableMarketIdealSpot();


            FileInfo excelfile = new FileInfo("Tools.xlsx");
            if (excelfile.Exists)
            {
                try
                {
                    excelfile.Delete();
                }
                catch (IOException)
                {
                    MessageBox.Show("Excel file with the name 'Tools' already open. please close it and resetart the application");
                }

            }
            excel.SaveAs(excelfile);

            System.Diagnostics.Process.Start(excelfile.ToString());


        }
        public void DrawTableMarketIdealSpot()
        {
            // headline:
            using (ExcelRange Rng = wsSheet1.Cells["A12:K12"])
            {
                Rng.Value = "Market Ideal Spot:";
                Rng.Merge = true;
                Rng.Style.Font.Size = 16;
                Rng.Style.Font.Bold = true;
                Rng.Style.Font.Italic = true;
                Rng.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;


            }
            //subHeadLine segments:
            for (int segmentInd = 0; segmentInd < 5; segmentInd += 1)
            {

                int LetterIndA = (int)'B' + segmentInd * 2;
                int LetterIndB = (int)'C' + segmentInd * 2;
                string columnA = (char)LetterIndA + "13";
                string columnB = (char)LetterIndB + "13";


                using (ExcelRange Rng = wsSheet1.Cells[columnA + ":" + columnB])
                {
                    Rng.Value = segments[segmentInd];
                    Rng.Merge = true;
                    Rng.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    Rng.Style.Font.Size = 14;
                    Rng.Style.Font.Bold = true;
                    Rng.Style.Font.Color.SetColor(colorsList[segmentInd]);
                }
            }


            // set empty table with columns names:  
            using (ExcelRange Rng = wsSheet1.Cells["A14:K25"])
            {
                wsSheet1.Cells[wsSheet1.Dimension.Address].AutoFitColumns();
                ExcelTable TableMarketIdealSpot = wsSheet1.Tables.Add(Rng, "TableMarketIdealSpot");

                TableMarketIdealSpot.Columns[0].Name = "round";

                for (int Index = 0; Index < 5; Index += 1)
                {

                    TableMarketIdealSpot.Columns[Index * 2 + 1].Name = "age=" + d_Age[segments[Index]];
                    TableMarketIdealSpot.Columns[Index * 2 + 2].Name = "SIZE=" + d_mtbf[segments[Index]];

                }
                //Indirectly access ExcelTableCollection class
                TableMarketIdealSpot.ShowHeader = false;
                TableMarketIdealSpot.ShowFilter = false;
                TableMarketIdealSpot.ShowTotal = true;

            }
            /*            // set columns age and mtbf:
                        string cellNum = "A17";
                        using (ExcelRange Rng = wsSheet1.Cells[cellNum])
                        {
                            Rng.Value = "Year";
                        }*/

            for (int segmentInd = 0; segmentInd < 5; segmentInd += 1)
            {

                int LetterIndA = (int)'B' + segmentInd * 2;
                int LetterIndB = (int)'C' + segmentInd * 2;
                string columnA = (char)LetterIndA + "15";
                string columnB = (char)LetterIndB + "15";

                string cellNum = columnA;
                using (ExcelRange Rng = wsSheet1.Cells[cellNum])
                {
                    Rng.Value = "pfmn";
                    Rng.Style.Font.Color.SetColor(colorsList[segmentInd]);
                    Rng.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;


                }
                cellNum = columnB;
                using (ExcelRange Rng = wsSheet1.Cells[cellNum])
                {
                    Rng.Value = "size:";
                    Rng.Style.Font.Color.SetColor(colorsList[segmentInd]);
                    Rng.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                }

            }
            //fill A column in the table
            for (int roundInd = 0; roundInd < 9; roundInd += 1)
            {
                int ofsset = roundInd + 16;
                string cellNum = "A" + ofsset;
                using (ExcelRange Rng = wsSheet1.Cells[cellNum])
                {
                    Rng.Value = "round " + roundInd;
                    Rng.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                }

                for (int segmentInd = 0; segmentInd < 5; segmentInd += 1)
                {
                    int lineOfsset = roundInd + 16;
                    int columnOffsetPfmn = segmentInd * 2 + (int)'B';
                    int columnOffsetSize = segmentInd * 2 + (int)'C';
                    cellNum = ((char)columnOffsetPfmn).ToString() + lineOfsset;
                    using (ExcelRange Rng = wsSheet1.Cells[cellNum])
                    {
                        Rng.Value = d_pfmn[segments[segmentInd]] + IdealSpotGrothRate[segments[segmentInd]][0] * roundInd;
                        Rng.Style.Font.Color.SetColor(colorsList[segmentInd]);
                        Rng.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    }
                    cellNum = ((char)columnOffsetSize).ToString() + lineOfsset;
                    using (ExcelRange Rng = wsSheet1.Cells[cellNum])
                    {
                        Rng.Value = d_size[segments[segmentInd]] + IdealSpotGrothRate[segments[segmentInd]][1] * roundInd;
                        Rng.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        Rng.Style.Font.Color.SetColor(colorsList[segmentInd]);

                    }
                }
            }

        }

        public void DrawTableUnitPerRounds()
        {
            // headline:
            using (ExcelRange Rng = wsSheet1.Cells["A2:F2"])
            {
                Rng.Value = "Units per segments every Year:";
                Rng.Merge = true;
                Rng.Style.Font.Size = 16;
                Rng.Style.Font.Bold = true;
                Rng.Style.Font.Italic = true;
            }

            // set empty table with columns names:
            using (ExcelRange Rng = wsSheet1.Cells["A3:K8"])
            {
                //Indirectly access ExcelTableCollection class
                ExcelTable TableUnitPerRounds = wsSheet1.Tables.Add(Rng, "TableUnitPerRounds");
                //table.Name = "tblSalesman";

                //Set Columns position & name
                TableUnitPerRounds.Columns[0].Name = "Segment";
                TableUnitPerRounds.Columns[1].Name = "Groth rate";
                for (int roundIndex = 0; roundIndex <= numberOfRounds; roundIndex += 1)
                {
                    TableUnitPerRounds.Columns[2 + roundIndex].Name = "round " + roundIndex;
                }
                //table.ShowHeader = true;
                TableUnitPerRounds.ShowFilter = false;
                TableUnitPerRounds.ShowTotal = true;

            }

            //fill first column - segments:
            for (int segmentInd = 0; segmentInd < 5; segmentInd += 1)
            {
                string cellNum = "A" + (4 + segmentInd);
                using (ExcelRange Rng = wsSheet1.Cells[cellNum])
                {
                    Rng.Value = segments[segmentInd];
                }
            }

            //fill second column - groth rate:
            for (int segmentInd = 0; segmentInd < 5; segmentInd += 1)
            {
                string cellNum = "B" + (4 + segmentInd);
                using (ExcelRange Rng = wsSheet1.Cells[cellNum])
                {
                    Rng.Value = GrothRate[segments[segmentInd]];
                }
            }

            //fill column 3 - round 0:
            for (int segmentInd = 0; segmentInd < 5; segmentInd += 1)
            {
                string cellNum = "C" + (4 + segmentInd);
                using (ExcelRange Rng = wsSheet1.Cells[cellNum])
                {
                    Rng.Value = IndustryUnitDemand[segments[segmentInd]];
                }
            }

            //fill the rest of the columns:
            for (int column = (int)'D'; column <= (int)'K'; column += 1)
            {
                int currRound = column % 32 - 3;
                for (int segmentInd = 0; segmentInd < 5; segmentInd += 1)
                {
                    string currSeg = segments[segmentInd];
                    char c = (char)column;
                    string cellNum = c.ToString() + (4 + segmentInd);

                    using (ExcelRange Rng = wsSheet1.Cells[cellNum])
                    {
                        string currGroth = GrothRate[currSeg];
                        currGroth = currGroth.Remove(currGroth.Length - 1);
                        currGroth = currGroth.Replace(".", "").Replace("-", "");
                        double doubleCurrGroth = Convert.ToDouble(currGroth);
                        doubleCurrGroth = Math.Pow((doubleCurrGroth / 1000 + 1), currRound);
                        double toprint = IndustryUnitDemand[currSeg] * doubleCurrGroth;
                        Rng.Value = (int)toprint;
                    }
                }
            }


        }


        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Button1_Click(object sender, EventArgs e)
        {
            Button1Click = true;
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "pdf files (*.pdf)|*.pdf|All files (*.*)|*.*";

            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SplitConditionREport(ofd.FileName);
                richTextBox.Text = "Step 1 done, The condition report succecfully uploeaded \n" +
                    "please open the courior file (under Step 2).";

            }
        }
        public void SplitConditionREport(string pdfFilePath)
        {
            StringBuilder text = new StringBuilder();
            PdfReader reader = new PdfReader(pdfFilePath);

            int pageNumber = 2;


            Document document = new Document();
            //string currSegment = pdfNames[pageNumber];
            //Total Industry Unit Demand:
            string thePage = PdfTextExtractor.GetTextFromPage(reader, pageNumber);

            for (int segmentInd = 0; segmentInd < 5; segmentInd += 1)
            {
                string toBeSearched = segments[segmentInd];
                string val = thePage.Substring(thePage.IndexOf(toBeSearched) + toBeSearched.Length);
                int positionOfNewLine = val.IndexOf("\n");
                if (toBeSearched == "Size")
                {
                    val = val.Substring(val.IndexOf(toBeSearched) + toBeSearched.Length);
                    positionOfNewLine = val.IndexOf("\n");
                }
                if (positionOfNewLine >= 0)
                {
                    val = val.Substring(0, positionOfNewLine);
                }
                ArrayList ALwords = new ArrayList(val.Split(' '));
                ALwords.Remove("");

                List<Double> intWords = new List<Double>();
                foreach (string word in ALwords)
                {
                    string tempVal = word.Replace(".", "").Replace("-", "");
                    if (word[0].ToString() == "-")
                    {
                        intWords.Add(Convert.ToDouble(tempVal) / 10 * -1);
                    }
                    else
                    {
                        intWords.Add(Convert.ToDouble(tempVal) / 10);
                    }


                }
                IdealSpotGrothRate[toBeSearched] = intWords;

            }


        }

        private void Label1_Click(object sender, EventArgs e)
        {

        }

        private void Label1_Click_1(object sender, EventArgs e)
        {

        }

        private void Label1_Click_2(object sender, EventArgs e)
        {

        }

        private void Label2_Click(object sender, EventArgs e)
        {

        }

        private void GroupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void Button2_Click(object sender, EventArgs e)
        {
            Button2Click = true;
            if (firstLetterOfTeamName == null)
            {
                MessageBox.Show("your team first letter is " + firstLetterOfTeamName);
                return;
            }
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "pdf files (*.pdf)|*.pdf|All files (*.*)|*.*";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string finalWordInPath = ofd.FileName.Substring(ofd.FileName.LastIndexOf("\\", ofd.FileName.Length));
                string folderPath = ofd.FileName.Replace(finalWordInPath, "") + "\\";
                

                SplitCourier(ofd.FileName, folderPath);

                CalculatePredictedUnitsNextRound();
                
            }
        }
        public void CalculatePredictedUnitsNextRound()
        {
            Dictionary<string, Dictionary<string, int>> nextRoundForecast = new Dictionary<string, Dictionary<string, int>>();
            for (int segmentInd = 0; segmentInd < 5; segmentInd += 1)
            {
                string currSeg = segments[segmentInd];
                string currGroth = GrothRate[currSeg];
                currGroth = currGroth.Remove(currGroth.Length - 1);
                currGroth = currGroth.Replace(".", "").Replace("-", "");
                double doubleCurrGroth = Convert.ToDouble(currGroth);
                doubleCurrGroth = doubleCurrGroth / 1000 + 1;
                double nextYearDemand = IndustryUnitDemand[currSeg] * doubleCurrGroth;

                Dictionary<string, int> tempSegmentSurveyRate = new Dictionary<string, int>();

                foreach (KeyValuePair<string, double> entry in SegmentSurveyRate[currSeg])
                {
                    tempSegmentSurveyRate.Add(entry.Key, Convert.ToInt32(entry.Value * nextYearDemand));
                }

                nextRoundForecast.Add(currSeg, tempSegmentSurveyRate);

            }

            //print the forecast calculation:
            richTextBox.Text = "The calculation is base on the following calculation: \n" +
                " (product customer survey / total customer survey ) * next year growth rate * next year total industry demand :\n\r";
            foreach (KeyValuePair<string, Dictionary<string, int>> kvp in nextRoundForecast)
            {
                foreach (KeyValuePair<string, int> kvpInner in kvp.Value)
                {
                    richTextBox.Text += "In segment:  '" + kvp.Key + "'  for product:  '" + kvpInner.Key + "'   the prediction is: " + kvpInner.Value + "\n";
                }
                richTextBox.Text += "\n";
            }

            SegmentSurveyRate.Clear();

        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            button3.Enabled = true;
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            firstLetterOfTeamName = textBox1.Text;
            firstLetterOfTeamName = firstLetterOfTeamName.ToUpper();
            if (firstLetterOfTeamName == null || firstLetterOfTeamName == "")
            {
                MessageBox.Show("please insert your team name first letter (e.g. 'A' for 'andraw')");
                return;
            }
            else
            {
                richTextBox.Text = "your team first letter is " + firstLetterOfTeamName;
            }
        }

        private void Label3_Click(object sender, EventArgs e)
        {

        }
    }
}
