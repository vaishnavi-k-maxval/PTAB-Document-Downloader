using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Configuration;
using System.Threading;
using OpenQA.Selenium.Support.UI;
using BusinessLayer;
using System.IO;
using System.Net.Mail;
using System.Net;
using OpenQA.Selenium.Remote;
using System.Text.RegularExpressions;

namespace PTABDocumentDownloader
{
    public partial class Form1 : Form
    {
        DataAccess _objDataAccess = new DataAccess();
        SendMail _objSendMail = new SendMail();
        static string TempDownloadFolder = ConfigurationManager.AppSettings["TempDownloadFolder"].ToString();
        static string AttachDownloadFolder = ConfigurationManager.AppSettings["AttachDownloadFolder"].ToString();

        int Mode = 0;
        bool IsPDF = false;
        bool IsDoc = false;
        bool Isxls = false;
        bool pdfExists = false;
        bool docExists = false;
        bool xlsexists = false;
        bool IsEmptyFile = false;
        int IsMailRequired = Convert.ToInt32(ConfigurationManager.AppSettings["IsMailRequired"]);
        System.IO.DirectoryInfo di = new DirectoryInfo(TempDownloadFolder);
        public Form1()
        {

            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            _objSendMail.SendMailMessage("PTAB Document Download Service", "########################-PTAB Document Download Service Started-###########################################################", IsMailRequired, 0);
            Mode = Convert.ToInt32(ConfigurationManager.AppSettings["ApplicationType"]);
            LogFile.WriteToFile("########################-PTAB Document Download Service Started-###########################################################");
            //PTABDocDownloadStart();


            try
            {
                foreach (FileInfo file in di.EnumerateFiles())
                {
                    file.Delete();
                }
                PTABDocDownloadStart();

                LogFile.WriteToFile("########################-PTAB Document Download Service Ended-###########################################################");
                _objSendMail.SendMailMessage("PTAB Document Download Service", "########################-PTAB Document Download Service Ended-###########################################################", IsMailRequired, 1);
            }
            catch (Exception ex)
            {

                LogFile.WriteToFile("Error in Form Load: " + ex.ToString());

                _objSendMail.SendMailMessage("PTAB Document Download Service Error", "########################-PTAB Document Download Service Error-###########################################################", IsMailRequired, 1);


            }
            finally
            {
                Application.Exit();
            }


        }

        private void PTABDocDownloadStart()
        {
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            IWebDriver webdriver;

            // string ChromeBrowser_exe = ConfigurationManager.AppSettings["FilePath"].ToString() + ConfigurationManager.AppSettings["DriverPath"].ToString();

            Dictionary<String, Object> chromePrefs = new Dictionary<String, Object>();

            ChromeOptions options = new ChromeOptions();
            options.AddArguments("test-type");
            options.AddArguments("disable-popup-blocking");


            options.AddArgument("disable-extensions");
            options.AddArguments("disable-infobars");
            options.AddArguments("disable-infobars");
            options.AddArguments("enable-pdf-material-ui");

            options.AddUserProfilePreference("plugins.plugins_disabled", new[] {
               "Adobe Flash Player",
               "Chrome PDF Viewer"
           });
            options.AddUserProfilePreference("plugins.always_open_pdf_externally", true);

            //Dictionary<String, Object> plugin = new Dictionary<String, Object>();
            //plugin.Add("enabled", false);
            //plugin.Add("name", "Chrome PDF Viewer");

            //Dictionary<String, Object> prefs = new Dictionary<String, Object>();
            //prefs.Add("plugins.plugins_list", plugin.ToList() );	


            //options.AddAdditionalCapability("chrome.prefs", prefs);

            //            options.AddUserProfilePreference("plugins.plugins_disabled", new[] { "Chrome PDF Viewer" });

            //            chromePrefs.Add("plugins.plugins_disabled", new String[] {
            //    "Adobe Flash Player",
            //    "Chrome PDF Viewer"
            //});


            //options.AddUserProfilePreference("profile.default_content_settings", chromePrefs);

            options.AddUserProfilePreference("download.default_directory", TempDownloadFolder);
            options.AddArgument("no-sandbox");


            //options.AddAdditionalCapability("excludeSwitches", new object[] { "disable-default-apps" });



            webdriver = new ChromeDriver(System.AppDomain.CurrentDomain.BaseDirectory, options, TimeSpan.FromSeconds(600));


            try
            {

                #region Navigation to Website
                string PTABURL = ConfigurationManager.AppSettings["PTabURL"].ToString();

                webdriver.Manage().Cookies.DeleteAllCookies();
                webdriver.Manage().Window.Maximize();
                webdriver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(180));
                webdriver.Navigate().GoToUrl(PTABURL);
                Thread.Sleep(5000);
                #endregion
                LoginToWebsite(webdriver);
                Thread.Sleep(18000);
                SearchbyYear(webdriver);
                Thread.Sleep(13000);
                DownloadDocument(webdriver);
                  //DownloadDocument(webdriver, _lstDownload);
            
            }
            catch (Exception ex)
            {

                webdriver.Quit();
                LogFile.WriteToFile("Error in PTABDocDownloadStart: " + ex.ToString());
                _objSendMail.SendMailMessage("PTAB Document Download Service - Error", "Error occured in PTAB Document Download Service Kindly Check the atatched log file. Document Download application restarted again", IsMailRequired, 1);
                PTABDocDownloadStart();
                //throw ex;
            }
            finally
            {

                webdriver.Quit();
            }


        }

        private void DownloadDocument(IWebDriver webdriver)
        {
            DataSet dsCase = new DataSet();
            try
            {



                //Click on Primary Tab
                WebDriverWait driverWaitTitle = new WebDriverWait(webdriver, new TimeSpan(0, 10, 0));
                int PrimaryTabCnt = driverWaitTitle.Until(drv => drv.FindElements(By.XPath("/html/body/div[3]/div[1]/div/div/div/div/div[1]/div/uib-accordion/div/div[1]/div[1]/h4/a/span/div"))).Count();
                for (int i = 0; i < 10; i++)
                {
                    if (PrimaryTabCnt > 0)
                    {
                        IWebElement PrimaryTab = driverWaitTitle.Until(drv => drv.FindElement(By.XPath("/html/body/div[3]/div[1]/div/div/div/div/div[1]/div/uib-accordion/div/div[1]/div[1]/h4/a/span/div")));
                        PrimaryTab.Click();
                        break;
                    }
                    else
                    {
                        Thread.Sleep(5000);
                    }
                }
                Thread.Sleep(2000);





                #region Get Case for Downloading Documents
                dsCase = _objDataAccess.GetCaseNUmberforDownload(Mode);
                #endregion
                if(dsCase != null && dsCase.Tables.Count > 0)
                {
                    //int i = 0;
                    if (dsCase.Tables[0].Rows.Count > 0)
                    {
                        int i = 0;
                        while (i < dsCase.Tables[0].Rows.Count)
                        {
                            LogFile.WriteToFile("Case Number: " + dsCase.Tables[0].Rows[i]["CaseNum"].ToString() + " document download Stared.");
                            //_objDataAccess.UpdateCaseStatus("download in-progress", Convert.ToInt32(dsCase.Tables[0].Rows[i]["CaseID"]), dsCase.Tables[0].Rows[i]["DocName"].ToString());
                            string testcase_NO = dsCase.Tables[0].Rows[i]["CaseNum"].ToString();
                            SearchCaseandDownloadFile(webdriver, dsCase.Tables[0].Rows[i]["CaseNum"].ToString(), dsCase.Tables[0].Rows[i]["CaseProceeding"].ToString(), dsCase);
                            // SearchCaseandDownloadFile(webdriver, dsCase.Tables[0].Rows[0]["CaseNum"].ToString(), dsCase.Tables[0].Rows[0]["ProceedingName"].ToString());
                            _objDataAccess.UpdateCaseStatus("download complete", Convert.ToInt32(dsCase.Tables[0].Rows[i]["CaseID"]), dsCase.Tables[0].Rows[i]["DocName"].ToString());
                            //_objDataAccess.UpdateDownloadFlag(dsCase.Tables[0].Rows[0]["CaseNum"].ToString(), Mode);
                            _objDataAccess.UpdateMultiRunFlag(dsCase.Tables[0].Rows[0]["CaseNum"].ToString(), dsCase.Tables[0].Rows[0]["Caseid"].ToString(), "completed");
                            CloseThePopUp(webdriver);
                            
                            LogFile.WriteToFile("Case Number: " + dsCase.Tables[0].Rows[i]["CaseNum"].ToString() + " document(s) download Ended.");
                            i = i + 1;
                        }
                        
                    }
                    dsCase = _objDataAccess.GetCaseNUmberforDownload(Mode);
                    //i=i+1
                }
                //if (dsCase != null && dsCase.Tables.Count > 0)
                //{
                //    if (dsCase.Tables[0].Rows.Count > 0)
                //    {
                //        for (int i = 0; i < dsCase.Tables[0].Rows.Count; i++)
                //        {
                //            //if (_objDataAccess.CheckISDoc_Lock(dsCase.Tables[0].Rows[i]["CaseNo"].ToString(), dsCase.Tables[0].Rows[i]["Caseid"].ToString())==0)
                //            //{pta

                //            LogFile.WriteToFile("Case Number: " + dsCase.Tables[0].Rows[i]["CaseNo"].ToString() + " document download Stared.");
                //            _objDataAccess.UpdateCaseStatus("download in-progress", Convert.ToInt32(dsCase.Tables[0].Rows[i]["CaseNo"]));
                //            SearchCaseandDownloadFile(webdriver, dsCase.Tables[0].Rows[i]["CaseNo"].ToString());
                //            _objDataAccess.UpdateDownloadFlag(dsCase.Tables[0].Rows[i]["CaseNo"].ToString(), Mode);
                //            _objDataAccess.UpdateMultiRunFlag(dsCase.Tables[0].Rows[i]["CaseNo"].ToString(), dsCase.Tables[0].Rows[i]["Caseid"].ToString(), "completed");
                //            CloseThePopUp(webdriver);
                //            _objDataAccess.UpdateCaseStatus("download complete", Convert.ToInt32(dsCase.Tables[0].Rows[i]["CaseNo"]));
                //            LogFile.WriteToFile("Case Number: " + dsCase.Tables[0].Rows[i]["CaseNo"].ToString() + " document(s) download Ended.");
                //        }
                //        //Update the Case IsDownload Flag

                //        //}
                //    }
                //}
            }
            catch (Exception ex)
            {

                LogFile.WriteToFile("Error in DownloadDocument: " + ex.ToString());

                webdriver.Quit();
                throw ex;
            }
        }

        private void CloseThePopUp(IWebDriver webdriver)
        {
            try
            {
                WebDriverWait driverWaitTitle = new WebDriverWait(webdriver, new TimeSpan(0, 30, 0));

                int PopUpCloseCnt = 0;

                for (int i = 0; i < 20; i++)
                {
                    //*[@id="ng-app"]/body/div[9]/div/div/div/div[1]/button
                    //*[@id="ng-app"]/body/div[9]/div/div/div/div[1]/button
                    PopUpCloseCnt = driverWaitTitle.Until(drv => drv.FindElements(By.XPath("//*[@id='ng-app']/body/div[9]/div/div/div/div[1]/button"))).Count();
                    if (PopUpCloseCnt > 0)
                    {
                        IWebElement PopUpClose = driverWaitTitle.Until(drv => drv.FindElement(By.XPath("//*[@id='ng-app']/body/div[9]/div/div/div/div[1]/button")));
                        PopUpClose.Click();
                        break;
                    }
                    else
                    {
                        Thread.Sleep(5000);

                    }
                }
            }
            catch (Exception ex)
            {

                LogFile.WriteToFile("Error in CloseThePopUp: " + ex.ToString());

                webdriver.Quit();
                throw ex;
            }
        }

        private void SearchCaseandDownloadFile(IWebDriver webdriver, string CaseNumber, string proceeedingnamefordownload,DataSet dscase)
        {
            try
            {

                Thread.Sleep(5000);
                WebDriverWait driverWaitTitle = new WebDriverWait(webdriver, new TimeSpan(0, 10, 0));
                //Clear the Search fields
                WebDriverWait driverWaitcancelButton = new WebDriverWait(webdriver, new TimeSpan(0, 10, 0));
                int cancelButtonCnt = 0;

                for (int i = 0; i < 20; i++)
                {
                    cancelButtonCnt = driverWaitTitle.Until(drv => drv.FindElements(By.Id("cancelButton"))).Count();
                    if (cancelButtonCnt > 0)
                    {
                        IWebElement cancelButton = driverWaitcancelButton.Until(drv => drv.FindElement(By.Id("cancelButton")));
                        cancelButton.Click();
                        break;
                    }
                    else
                    {
                        Thread.Sleep(5000);

                    }
                }
                Thread.Sleep(5000);

                //Search the Case Number
                WebDriverWait driverWaitTabproceedingNumber = new WebDriverWait(webdriver, new TimeSpan(0, 10, 0));
                int TabproceedingNumberCnt = 0;

                for (int i = 0; i < 20; i++)
                {
                    TabproceedingNumberCnt = driverWaitTitle.Until(drv => drv.FindElements(By.Id("proceedingNumber"))).Count();
                    if (TabproceedingNumberCnt > 0)
                    {
                        IWebElement TabproceedingNumber = driverWaitTabproceedingNumber.Until(drv => drv.FindElement(By.Id("proceedingNumber")));
                        TabproceedingNumber.SendKeys(CaseNumber);
                        break;
                    }
                    else
                    {
                        Thread.Sleep(5000);
                    }
                }

                //Click on Search Button
                WebDriverWait driverWaitTabsearchButton = new WebDriverWait(webdriver, new TimeSpan(0, 10, 0));
                int searchButtonCnt = 0;

                for (int i = 0; i < 20; i++)
                {
                    searchButtonCnt = driverWaitTitle.Until(drv => drv.FindElements(By.Id("searchButton"))).Count();
                    if (searchButtonCnt > 0)
                    {
                        IWebElement TabsearchButton = driverWaitTabsearchButton.Until(drv => drv.FindElement(By.Id("searchButton")));
                        TabsearchButton.Click();
                        break;
                    }
                    else
                    {
                        Thread.Sleep(5000);
                    }
                }
                Thread.Sleep(7000);

                //Click on View Document
                WebDriverWait driverWaitViewDocument = new WebDriverWait(webdriver, new TimeSpan(0, 10, 0));
                int showDldButtonCnt = 0;

                for (int i = 0; i < 20; i++)
                {
                    showDldButtonCnt = driverWaitTitle.Until(drv => drv.FindElements(By.Id("showDldButton"))).Count();
                    if (showDldButtonCnt > 0)
                    {
                        IWebElement ViewDocument = driverWaitViewDocument.Until(drv => drv.FindElement(By.Id("showDldButton")));
                        ViewDocument.Click();
                        break;
                    }
                    else
                    {
                        Thread.Sleep(5000);
                    }
                }
                Thread.Sleep(7000);


                List<Download> _lstDownload = new List<Download>();
                Download _objDownload = null;

                int cnt = 0;

                int rowCount = 0;

                //Code Changed by Karthik T 04Jun18
                for (int i = 0; i < 10; i++)
                {
                    IList<IWebElement> allElement = webdriver.FindElements(By.Id("dataTables-example1"));
                    foreach (IWebElement element1 in allElement)
                    {
                        cnt = Regex.Matches(element1.Text, "Download").Count - 1;
                    }


                    //cnt = driverWaitTitle.Until(drv => drv.FindElements(By.XPath("//*[@id='dataTables-example1']//table"))).Count();

                    //cnt = driverWaitTitle.Until(drv => drv.FindElements(By.XPath("*[@id='dataTables-example1']//table"))).Count();              

                    if (cnt > 0)
                    {
                        //rowCount = driverWaitTitle.Until(drv => drv.FindElements(By.XPath("//html/body/div[9]/div/div/div/div[2]/div/table[@id='dataTables-example1']/tbody/tr"))).Count();
                        //rowCount = driverWaitTitle.Until(drv => drv.FindElements(By.XPath("*[@id='dataTables-example1']//table"))).Count();
                        rowCount = cnt + 1;
                        break;
                    }
                    else
                    {
                        Thread.Sleep(5000);
                    }
                }

                for (int i = 1; i <= rowCount; i++)
                {
                    _objDownload = new Download();

                    string proceedingname = string.Empty;
                    string DocumentType = string.Empty;
                    string Exhibit_PaperNo = string.Empty;
                    string FilingDate = string.Empty;
                    string FilingParty = string.Empty;


                    proceedingname = driverWaitTitle.Until(drv => drv.FindElement(By.XPath("//html/body/div[9]/div/div/div/div[2]/div/table[@id='dataTables-example1']/tbody/tr[" + i + "]/td[2]"))).Text;
                    DocumentType = driverWaitTitle.Until(drv => drv.FindElement(By.XPath("//html/body/div[9]/div/div/div/div[2]/div/table[@id='dataTables-example1']/tbody/tr[" + i + "]/td[3]"))).Text;
                    Exhibit_PaperNo = driverWaitTitle.Until(drv => drv.FindElement(By.XPath("//html/body/div[9]/div/div/div/div[2]/div/table[@id='dataTables-example1']/tbody/tr[" + i + "]/td[4]"))).Text;
                    FilingDate = driverWaitTitle.Until(drv => drv.FindElement(By.XPath("//html/body/div[9]/div/div/div/div[2]/div/table[@id='dataTables-example1']/tbody/tr[" + i + "]/td[5]"))).Text;
                    FilingParty = driverWaitTitle.Until(drv => drv.FindElement(By.XPath("//html/body/div[9]/div/div/div/div[2]/div/table[@id='dataTables-example1']/tbody/tr[" + i + "]/td[6]"))).Text;

                    string containsstring = ConfigurationManager.AppSettings["Contains"];

                    string Startwithstring = ConfigurationManager.AppSettings["Startswith"];

                    List<string> containstr = containsstring.Split('|').Reverse().ToList<string>();

                    List<string> Startwithstr = Startwithstring.Split('|').Reverse().ToList<string>();


                    //for (int j = 1; j <= containstr.Count; j++)
                    //{
                    ////string XPathtoDownload = _lstDownload[i].DocXPath.ToString();
                    //string XPathtoDownload = "D:\\PTAB FILES\\PTABDownloads";

                    //var casenumber = _lstDownload.Where(s => s.CaseNumber == "IPR2013-00012"); 

                    // //var docname = Convert.ToBoolean(_lstDownload.Where(t => t.DocName == "MajumdarDocsConsidered"));
                    // var docname = Convert.ToBoolean(_lstDownload.Where(a => a.DocName.Equals("Majumdar Docs Considered")).FirstOrDefault());
                    //if (docname == true)
                    ////if (_lstDownload.Select(s => s.DocName) = _lstDownload.Single("MajumdarDocsConsidered"))
                    //{

                    //    IWebElement DownloadButton = driverWaitTitle.Until(drv => drv.FindElement(By.XPath(XPathtoDownload)));

                    //    DownloadButton.Click();

                    //}

                    proceeedingnamefordownload = proceeedingnamefordownload.Replace("\n", " ");
                        //if (proceedingname.ToUpper().Contains(containstr[j - 1].ToString().ToUpper()) || DocumentType.ToUpper().Contains(containstr[j - 1].Trim().ToString().ToUpper()))
                        if (proceedingname.ToString().TrimStart().TrimEnd()==proceeedingnamefordownload.ToString().TrimStart().TrimEnd())
                        {
                            
                            _objDownload.CaseNumber = CaseNumber;
                            _objDownload.DocName = proceedingname;
                            _objDownload.DocType = DocumentType;
                            _objDownload.ENumber = Exhibit_PaperNo;
                            _objDownload.DocXPath = "//html/body/div[9]/div/div/div/div[2]/div/table[@id='dataTables-example1']/tbody/tr[" + i + "]/td[8]/a";

                            _lstDownload.Add(_objDownload);
                        }
                    //}


                    //for (int j = 1; j <= Startwithstr.Count; j++)
                    //{

                    //    if (proceedingname.ToUpper().StartsWith(Startwithstr[j - 1].ToString().ToUpper()) || proceedingname.ToUpper().Contains(Startwithstr[j - 1].Trim().ToString().ToUpper()) )
                    //    {

                    //        _objDownload.CaseNumber = CaseNumber;
                    //        _objDownload.DocName = proceedingname;
                    //        _objDownload.DocType = DocumentType;
                    //        _objDownload.ENumber = Exhibit_PaperNo;
                    //        _objDownload.DocXPath = "//html/body/div[9]/div/div/div/div[2]/div/table[@id='dataTables-example1']/tbody/tr[" + i + "]/td[8]/a";

                    //        _lstDownload.Add(_objDownload);

                    //    }
                    //}
                    // created the path for file downloads

                    string XPathtoDownload = "D:\\PTAB FILES\\PTABDownloads";

                    //var casenumber = _lstDownload.Where(s => s.CaseNumber == "IPR2013-00012");

                    //var docname = Convert.ToBoolean(_lstDownload.Where(t => t.DocName == "MajumdarDocsConsidered"));
                    //var docname = Convert.ToBoolean(_lstDownload.Where(a => a.DocName.Equals(proceeedingnamefordownload)).FirstOrDefault());
                    //if (docname == true)
                    ////if (_lstDownload.Select(s => s.DocName) = _lstDownload.Single("MajumdarDocsConsidered"))
                    //{

                    //    IWebElement DownloadButton = driverWaitTitle.Until(drv => drv.FindElement(By.XPath(XPathtoDownload)));

                    //    DownloadButton.Click();

                    //}

                }
                if (_lstDownload.Count > 0)
                {
                    _lstDownload = _lstDownload.Distinct().ToList();

                    DownloadDocument(webdriver, _lstDownload,dscase);
                }

            }
            catch (Exception ex)
            {

                LogFile.WriteToFile("Error in SearchCaseandDownloadFile: " + ex.ToString());

                webdriver.Quit();
                throw ex;
            }
        }

        private void DownloadDocument(IWebDriver webdriver, List<Download> _lstDownload,DataSet Dscases)
        {
            try
            {
                string value = "";
                bool donotwait = false;
                LogFile.WriteToFile("Total : " + Convert.ToString(_lstDownload.Count) + " to be downloaded for Case: " + _lstDownload[0].CaseNumber);
                for (int i = 0; i < _lstDownload.Count; i++)
                {
                    WebDriverWait driverWaitTitle1 = new WebDriverWait(webdriver, new TimeSpan(0, 5, 0));

                    string FileName = string.Empty;
                    string XPathtoDownload = _lstDownload[i].DocXPath.ToString();
                    // declared the document type
                    string Doctype = string.Empty;
                    string ret = string.Empty;
                    //var casenumber = _lstDownload.Where(s => s.CaseNumber == "IPR2013-00012");
                    //var docname = Convert.ToBoolean(_lstDownload.Where(t => t.DocName == "MajumdarDocsConsidered"));
                    //if(docname == true)
                    ////if (_lstDownload.Select(s => s.DocName) = _lstDownload.Single("MajumdarDocsConsidered"))
                    //{

                    //    IWebElement DownloadButton = driverWaitTitle1.Until(drv => drv.FindElement(By.XPath(XPathtoDownload)));

                    //    DownloadButton.Click();

                    //}
                    //var FileNameyy = Dscases.Tables[0].AsEnumerable().Where(r => ((string)r["CaseProceeding"]) == Convert.ToString(_lstDownload[i].DocName));
                    // Enumerate the datarows of the table into a collection of IEnumerable
                    IEnumerable<DataRow> eDR = Dscases.Tables[0].AsEnumerable();
                    // Select the rows you wish to copy to the new table by runing a Linq query.

                    IEnumerable<DataRow> query = (from recr in eDR
                                                 where recr.Field<String>("CaseProceeding") == Convert.ToString(_lstDownload[0].DocName)
                                                 select recr).Take(1);

                    //added the conditions for doc type and doc name
                    if (query.Count() == 1)
                    {
                         ret = query.First()["DocName"].ToString();
                    }
                    FileName = ret;

                    if (Convert.ToString(_lstDownload[i].DocType) != "PAPER" && _lstDownload[i].ENumber != "0")
                    {
                          FileName = ret;
                            //Dscases.Tables[0].AsEnumerable().Where(r => ((string)r["CaseProceeding"]) == Convert.ToString(_lstDownload[i].DocName));
                            //Dscases.Tables[0].AsEnumerable().Where(r => ((string)r["CaseProceeding"]) == Convert.ToString(_lstDownload[i].DocName)).First().ToString();
                            //---FileName = Convert.ToString(_lstDownload[i].DocType) + "_" + Convert.ToString(_lstDownload[i].ENumber) + "_" + Convert.ToString(_lstDownload[i].CaseNumber);

                    }
                    else
                    {
                        Doctype = _lstDownload[i].DocName.Replace("/", " ").Replace("\\", " ").Replace(":", " ").Replace("*", " ").Replace("?", " ").Replace("\"", " ").Replace("<", " ").Replace(">", " ").Replace("|", " ");
                        if (Doctype.Length >= 200)
                        {
                            Doctype = Doctype.Substring(0, Doctype.Length - 170);
                        }
                        //---FileName = Convert.ToString(Doctype) + "_" + Convert.ToString(_lstDownload[i].ENumber) + "_" + Convert.ToString(_lstDownload[i].CaseNumber);
                        FileName = ret;
                            //Dscases.Tables[0].AsEnumerable().Where(r => ((string)r["CaseProceeding"]) == Convert.ToString(_lstDownload[i].DocName)).First().ToString();
                    }

                    //bool contains = Directory.EnumerateFiles(AttachDownloadFolder).Any(f => f.Contains(FileName));

                    if (!File.Exists(AttachDownloadFolder + FileName))
                    {
                        LogFile.WriteToFile("Document: " + FileName + ".pdf" + " is taken for Download");
                        WebDriverWait driverWaitTitle = new WebDriverWait(webdriver, new TimeSpan(0, 5, 0));
                        int DownloadButtonCnt = 0;
                        for (int chk = 0; chk < 20; chk++)
                        {
                            DownloadButtonCnt = driverWaitTitle.Until(drv => drv.FindElements(By.XPath(XPathtoDownload))).Count();
                            if (DownloadButtonCnt > 0)
                            {
                                IJavaScriptExecutor javascriptDriver = (IJavaScriptExecutor)webdriver;
                                var element = webdriver.FindElement(By.XPath(XPathtoDownload));
                                Dictionary<string, object> attributes = javascriptDriver.ExecuteScript("var items = {}; for (index = 0; index < arguments[0].attributes.length; ++index) { items[arguments[0].attributes[index].name] = arguments[0].attributes[index].value }; return items;", element) as Dictionary<string, object>;
                                value = webdriver.FindElement(By.XPath(XPathtoDownload)).GetAttribute("disabled");

                                if (value == null && value != "true")
                                {
                                    IWebElement DownloadButton = driverWaitTitle.Until(drv => drv.FindElement(By.XPath(XPathtoDownload)));
                                    webdriver.Manage().Window.Maximize();                                    
                                    DownloadButton.Click();                                    
                                    donotwait = false;
                                    break;
                                }
                                else
                                {
                                    donotwait = true;
                                    break;
                                }
                            }
                            else
                            {
                                Thread.Sleep(5000);
                            }
                        }
                        if (donotwait != true)
                        {
                            int waittime = Convert.ToInt32(ConfigurationManager.AppSettings["DocDownloadWaitTime"]);
                            Thread.Sleep(15000);
                            di = new DirectoryInfo(TempDownloadFolder);

                            for (int doc = 0; doc < 20; doc++)
                            {
                                if (di.EnumerateFiles().Count() > 0)
                                {
                                    string[] files = System.IO.Directory.GetFiles(TempDownloadFolder, "*.pdf");
                                    if (files.Count() > 0)
                                    {
                                        LogFile.WriteToFile("Document: " + FileName + " has been downloaded and rename move Process Started");
                                        DocProcess(FileName);
                                        LogFile.WriteToFile("Document: " + FileName + " rename and move Process Ended");
                                    }
                                    else
                                    {
                                        IsPDF = false;
                                        IsEmptyFile = true;
                                    }

                                    if (IsPDF == true)
                                    {
                                        //FileName = FileName + ".pdf";
                                        LogFile.WriteToFile("Document: " + FileName + " Save File name to DB Started");
                                        _objDataAccess.SaveFileName(Convert.ToString(_lstDownload[i].CaseNumber), FileName, Convert.ToString(_lstDownload[i].DocType), Convert.ToString(_lstDownload[i].ENumber));
                                        LogFile.WriteToFile("Document: " + FileName + " Save File name to DB Ended");
                                    }
                                    //else if (IsDoc == true)
                                    //{
                                    //    FileName = FileName + ".doc";
                                    //    LogFile.WriteToFile("Document: " + FileName + " Save File name to DB Started");
                                    //    _objDataAccess.SaveFileName(Convert.ToString(_lstDownload[i].CaseNumber), FileName, Convert.ToString(_lstDownload[i].DocType), Convert.ToString(_lstDownload[i].ENumber));
                                    //    LogFile.WriteToFile("Document: " + FileName + " Save File name to DB Ended");
                                    //}
                                    //else if (Isxls == true)
                                    //{
                                    //    FileName = FileName + ".xls";
                                    //    LogFile.WriteToFile("Document: " + FileName + " Save File name to DB Started");
                                    //    _objDataAccess.SaveFileName(Convert.ToString(_lstDownload[i].CaseNumber), FileName, Convert.ToString(_lstDownload[i].DocType), Convert.ToString(_lstDownload[i].ENumber));
                                    //    LogFile.WriteToFile("Document: " + FileName + " Save File name to DB Ended");
                                    //}
                                    Thread.Sleep(1000);
                                    break;
                                }
                                else
                                {
                                    Thread.Sleep(waittime);
                                }
                            }
                            if (IsEmptyFile != true)
                            {
                                if (IsPDF != true)
                                {
                                    if (IsDoc != true)
                                    {
                                        if (Isxls != true)
                                        {

                                            if (!File.Exists(AttachDownloadFolder + FileName))
                                            {
                                                LogFile.WriteToFile("Document download Error: " + FileName + " not download from PTAB Site.");
                                                _objSendMail.SendMailMessage("PTAB Document Download - Error", "Document download Error: " + FileName + " not download from PTAB Site restarting Application", IsMailRequired, 1);

                                                webdriver.Quit();
                                            }

                                        }
                                    }
                                }
                            }

                        }

                    }
                    else
                    {
                        LogFile.WriteToFile("File : " + FileName + " already exists!!!");

                    }






                }
            }
            catch (Exception ex)
            {

                LogFile.WriteToFile("Error in DownloadDocument: " + ex.ToString());

                webdriver.Quit();
                throw ex;
            }
        }

        private void DocProcess(string FileName)
        {
            try
            {
                RenameandMoveFile(FileName);


            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        private void RenameandMoveFile(string FileName)
        {
            try
            {


                string oldFileName = string.Empty;

                if (oldFileName == "")
                {
                    FileInfo finfo = GetNewestFile();
                    oldFileName = finfo.Name;
                }



                if (oldFileName.Contains(".pdf"))
                {
                    oldFileName = oldFileName.Substring(0, oldFileName.IndexOf("."));
                    IsPDF = true;
                    IsEmptyFile = false;
                }
                else if (oldFileName.Contains(".doc"))// || oldFileName.Contains(".docx"))
                {
                    //oldFileName = oldFileName.Substring(0, oldFileName.IndexOf("."));
                    IsDoc = true;
                    IsEmptyFile = false;
                }
                else if (oldFileName.Contains(".xls"))// || oldFileName.Contains(".xlsx"))
                {
                    //oldFileName = oldFileName.Substring(0, oldFileName.IndexOf("."));
                    Isxls = true;
                    IsEmptyFile = false;
                }
                else
                {
                    IsPDF = false;
                    IsDoc = false;
                    Isxls = false;
                    IsEmptyFile = true;
                    LogFile.WriteToFile("Unknown File: " + oldFileName + " of " + FileName);


                }

                if (di.EnumerateFiles().Count() > 0)
                {
                    if (IsPDF == true)
                    {

                        if (File.Exists(TempDownloadFolder + oldFileName + ".pdf"))
                        //if (File.Exists(TempDownloadFolder + oldFileName + ".pdf") && !File.Exists(AttachDownloadFolder + FileName))
                        {
                            if (File.Exists(AttachDownloadFolder + FileName + ".pdf"))
                            {
                                string MoveAttachBckFolder = AttachDownloadFolder + "PTABBackupDocuments";

                                if (!Directory.Exists(MoveAttachBckFolder))
                                {
                                    Directory.CreateDirectory(MoveAttachBckFolder);
                                }
                                LogFile.WriteToFile("File Name: " + FileName + ".pdf replaced and moved to" + MoveAttachBckFolder + "\\" + FileName + ".pdf");
                                File.Replace(TempDownloadFolder + oldFileName + ".pdf", AttachDownloadFolder + FileName, MoveAttachBckFolder + "\\" + FileName + ".pdf");
                            }
                            else
                            {
                                LogFile.WriteToFile("File Name: " + FileName + ".pdf moved to" + AttachDownloadFolder + FileName + ".pdf");
                                //File.Move(TempDownloadFolder + oldFileName + ".pdf", AttachDownloadFolder + FileName + ".pdf");
                                File.Move(TempDownloadFolder + oldFileName + ".pdf", AttachDownloadFolder + FileName);
                            }



                        }
                        else
                        {

                            if (!File.Exists(AttachDownloadFolder + FileName + ".pdf"))
                            {
                                RenameandMoveFile(FileName);
                            }
                            else
                            {
                                LogFile.WriteToFile("Error in File: " + FileName + ".pdf" + " not moved to" + AttachDownloadFolder + FileName);



                            }
                        }
                    }

                    //else if (IsDoc == true)
                    //{
                    //    if (File.Exists(TempDownloadFolder + oldFileName + ".doc"))// || File.Exists(TempDownloadFolder + oldFileName + ".docx"))
                    //    //if (File.Exists(TempDownloadFolder + oldFileName + ".pdf") && !File.Exists(AttachDownloadFolder + FileName))
                    //    {
                    //        if (File.Exists(AttachDownloadFolder + FileName + ".doc"))
                    //        {
                    //            string MoveAttachBckFolder = AttachDownloadFolder + "PTABBackupDocuments";

                    //            if (!Directory.Exists(MoveAttachBckFolder))
                    //            {
                    //                Directory.CreateDirectory(MoveAttachBckFolder);
                    //            }
                    //            LogFile.WriteToFile("File Name: " + FileName + ".doc replaced and moved to" + MoveAttachBckFolder + "\\" + FileName);
                    //            File.Replace(TempDownloadFolder + oldFileName + ".doc", AttachDownloadFolder + FileName, MoveAttachBckFolder + "\\" + FileName+".doc");
                    //        }
                    //        else
                    //        {
                    //            LogFile.WriteToFile("File Name: " + FileName + ".doc moved to" + AttachDownloadFolder + FileName);
                    //            File.Move(TempDownloadFolder + oldFileName + ".doc", AttachDownloadFolder + FileName + ".doc");
                    //        }


                    //    }
                    //    else
                    //    {

                    //        if (!File.Exists(AttachDownloadFolder + FileName + ".doc"))
                    //        {
                    //            RenameandMoveFile(FileName);
                    //        }
                    //        else
                    //        {
                    //            LogFile.WriteToFile("Error in File: " + FileName + ".doc" + " not moved to" + AttachDownloadFolder + FileName);


                    //        }
                    //    } 
                    //}
                    //else if (Isxls == true)
                    //{
                    //    if (File.Exists(TempDownloadFolder + oldFileName + ".xls"))// || File.Exists(TempDownloadFolder + oldFileName + ".xlsx"))
                    //    //if (File.Exists(TempDownloadFolder + oldFileName + ".pdf") && !File.Exists(AttachDownloadFolder + FileName))
                    //    {
                    //        if (File.Exists(AttachDownloadFolder + FileName + ".xls"))
                    //        {
                    //            string MoveAttachBckFolder = AttachDownloadFolder + "PTABBackupDocuments";

                    //            if (!Directory.Exists(MoveAttachBckFolder))
                    //            {
                    //                Directory.CreateDirectory(MoveAttachBckFolder);
                    //            }
                    //            LogFile.WriteToFile("File Name: " + FileName + ".xls replaced and moved to" + MoveAttachBckFolder + "\\" + FileName);
                    //            File.Replace(TempDownloadFolder + oldFileName + ".xls", AttachDownloadFolder + FileName, MoveAttachBckFolder + "\\" + FileName + ".xls");
                    //        }
                    //        else
                    //        {
                    //            LogFile.WriteToFile("File Name: " + FileName + ".xls moved to" + AttachDownloadFolder + FileName + ".xls");
                    //            File.Move(TempDownloadFolder + oldFileName + ".xls", AttachDownloadFolder + FileName + ".xls");
                    //        }



                    //    }

                    //    else
                    //    {

                    //        if (!File.Exists(AttachDownloadFolder + FileName + ".xls"))
                    //        {
                    //            RenameandMoveFile(FileName);
                    //        }
                    //        else
                    //        {
                    //            LogFile.WriteToFile("Error in File: " + FileName + ".xls" + " not moved to" + AttachDownloadFolder + FileName);



                    //        }
                    //    } 

                    //}

                }

            }
            catch (Exception ex)
            {
                
                LogFile.WriteToFile("Error in RenameandMoveFile: " + ex.ToString());
                throw ex;

            }
        }

        public FileInfo GetNewestFile()
        {
            try
            {
                //System.IO.DirectoryInfo directory = null;

                //directory = new DirectoryInfo(TempDownloadFolder);

                //return di.EnumerateFiles()
                //       .Union(di.GetDirectories().Select(d => GetNewestFile()))
                //       .OrderByDescending(f => (f == null ? DateTime.MinValue : f.LastWriteTime))
                //       .FirstOrDefault();

                FileInfo result = null;
                var list = di.EnumerateFiles();
                if (list.Any())
                {
                    result = list.OrderByDescending(f => f.LastWriteTime).First();
                }
                return result;
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        private void LoginToWebsite(IWebDriver webdriver)
        {
            try
            {
                WebDriverWait driverWaitTitle = new WebDriverWait(webdriver, new TimeSpan(0, 5, 0));

                string UserName = ConfigurationManager.AppSettings["UserName"].ToString();
                string Password = ConfigurationManager.AppSettings["Password"].ToString();

                int txtLogin = 0;

                for (int i = 0; i < 10; i++)
                {
                    txtLogin = driverWaitTitle.Until(drv => drv.FindElements(By.Id("loginUsername"))).Count();
                    if (txtLogin > 0)
                    {
                        IWebElement LoginUserName = driverWaitTitle.Until(drv => drv.FindElement(By.Id("loginUsername")));
                        LoginUserName.SendKeys(UserName);
                        break;
                    }
                    else
                    {
                        Thread.Sleep(5000);
                    }
                }

                int txtLPwd = 0;

                WebDriverWait driverWaitLoginPassword = new WebDriverWait(webdriver, new TimeSpan(0, 5, 0));
                for (int i = 0; i < 10; i++)
                {
                    txtLPwd = driverWaitTitle.Until(drv => drv.FindElements(By.Id("loginPassword"))).Count();
                    if (txtLPwd > 0)
                    {
                        IWebElement LoginPassword = driverWaitLoginPassword.Until(drv => drv.FindElement(By.Id("loginPassword")));
                        LoginPassword.SendKeys(Password);
                        break;
                    }
                    else
                    {
                        Thread.Sleep(5000);
                    }
                }
                Thread.Sleep(2000);
                int LoginButtonCnt = 0;

                WebDriverWait driverWaitLoginButton = new WebDriverWait(webdriver, new TimeSpan(0, 5, 0));
                for (int i = 0; i < 10; i++)
                {
                    LoginButtonCnt = driverWaitTitle.Until(drv => drv.FindElements(By.Id("PTAB-login-button"))).Count();
                    if (LoginButtonCnt > 0)
                    {
                        driverWaitLoginButton.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("block-ui-overlay")));
                        IWebElement LoginButton = driverWaitLoginButton.Until(drv => drv.FindElement(By.Id("PTAB-login-button")));
                        LoginButton.Click();
                        break;
                    }
                    else
                    {
                        Thread.Sleep(5000);
                    }
                }

            }
            catch (Exception ex)
            {
                LogFile.WriteToFile("Error in LoginToWebsite: " + ex.ToString());

                webdriver.Quit();
                throw ex;
            }
        }

        private void SearchbyYear(IWebDriver webdriver)
        {
            try
            {
                WebDriverWait driverWaitTitle = new WebDriverWait(webdriver, new TimeSpan(0, 5, 0));
                int SearchTabCnt = 0;

                for (int i = 0; i < 10; i++)
                {
                    SearchTabCnt = driverWaitTitle.Until(drv => drv.FindElements(By.Id("externalSearch"))).Count();
                    if (SearchTabCnt > 0)
                    {
                        driverWaitTitle.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("global-backdrop modal-backdrop fade in")));
                        IWebElement SearchTab = driverWaitTitle.Until(drv => drv.FindElement(By.Id("externalSearch")));
                        SearchTab.Click();
                        break;
                    }
                    else
                    {
                        Thread.Sleep(5000);
                    }
                }
                Thread.Sleep(2000);
                int proceedingNumberCnt = 0;

                for (int i = 0; i < 10; i++)
                {
                    proceedingNumberCnt = driverWaitTitle.Until(drv => drv.FindElements(By.Id("proceedingNumber"))).Count();
                    if (proceedingNumberCnt > 0)
                    {
                        IWebElement proceedingNumber = driverWaitTitle.Until(drv => drv.FindElement(By.Id("proceedingNumber")));
                        proceedingNumber.SendKeys("IPR2013-00012");
                        //proceedingNumber.SendKeys("2012");
                        break;
                    }
                    else
                    {
                        Thread.Sleep(5000);
                    }
                }
                Thread.Sleep(2000);
                int searchButtonCnt = 0;

                for (int i = 0; i < 10; i++)
                {
                    searchButtonCnt = driverWaitTitle.Until(drv => drv.FindElements(By.Id("searchButton"))).Count();
                    if (searchButtonCnt > 0)
                    {
                        IWebElement SearchButton = driverWaitTitle.Until(drv => drv.FindElement(By.Id("searchButton")));
                        SearchButton.Click();
                        break;
                    }
                    else
                    {
                        Thread.Sleep(5000);
                    }
                }

            }
            catch (Exception ex)
            {
                LogFile.WriteToFile("Error in SearchbyYear: " + ex.ToString());

                webdriver.Quit();
                throw ex;
            }
        }


    }
}
