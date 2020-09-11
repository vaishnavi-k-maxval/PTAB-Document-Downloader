using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BusinessLayer
{
    public class Download
    {
        public string DocName { get; set; }
        public string DocXPath { get; set; }
        public string CaseNumber { get; set; }
        public string DocType { get; set; }
        public string ENumber { get; set; }
        public int SNo { get; set; }
    }

    public class ProceedingCompare
    {
        public int Sno { get; set; }

        public string PName { get; set; }

        public string Type { get; set; }

        public string Eno { get; set; }

        public int PDownloadflag { get; set; }
    }

    public class SPNames
    {
        public string GetInstitutedorFiltered = "usp_GetInstitutedorFiltered";
        public string Updateyearfilter = "usp_update_year_filter";
        public string PTabTempInsertBulkCases = "usp_PTabTemp_InsertBulkCases";
        public string PTABCaseInsert = "usp_PTAB_Case_Insert";
        public string PTABInsertAttachenmt_Details = "usp_PTAB_Insert_Attachenmt_Details";
        public string PTabCaseProceedings = "usp_PTab_CaseProceedings";
        public string PTABFileNameSave = "usp_PTAB_FileName_Save";
        public string GetCaseNoYearDAILY = "usp_Get_CaseNo_Year_DAILY";
        //public string GetCaseNoforDownloadDoc = "usp_Get_PTAB_CaseNumbers_for_Download_Test";//usp_Get_PTAB_CaseNumbers_for_DownloadMultMachine;usp_Get_PTAB_CaseNumbers_for_Download_from_bottom
        public string GetCaseNoforDownloadDoc = "usp_Get_PTAB_CaseNumbers_for_Download_Test1";//usp_Get_PTAB_CaseNumbers_for_DownloadMultMachine;usp_Get_PTAB_CaseNumbers_for_Download_from_bottom

        public string UpdateDownloadFlag = "usp_Update_PTAB_Is_Download";
    }
}
