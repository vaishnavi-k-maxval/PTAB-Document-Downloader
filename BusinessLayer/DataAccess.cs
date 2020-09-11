using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;

namespace BusinessLayer
{
    public class DataAccess
    {
        private string m_sConnectionString;
        private SPNames _objSPNames;


        public void SaveFileName(string CaseNo, string FileName, string Type, string ExibitNo)
        {
            _objSPNames = new SPNames();
            try
            {
                SqlTransaction sqlTrans;
                SqlCommand cmdPtabCaseDetails;

                m_sConnectionString = ConfigurationManager.AppSettings["LitigationConnectionString"];

                using (SqlConnection cnnUpdatePtabCase = new SqlConnection(m_sConnectionString))
                {
                    cnnUpdatePtabCase.Open();

                    using (sqlTrans = cnnUpdatePtabCase.BeginTransaction())
                    {
                        try
                        {
                            using (cmdPtabCaseDetails = new SqlCommand(_objSPNames.PTABFileNameSave, cnnUpdatePtabCase, sqlTrans))
                            {
                                cmdPtabCaseDetails.CommandType = CommandType.StoredProcedure;
                                cmdPtabCaseDetails.Parameters.AddWithValue("@pCaseNumber", CheckDBNull(CaseNo));
                                cmdPtabCaseDetails.Parameters.AddWithValue("@pFileName", CheckDBNull(FileName));
                                cmdPtabCaseDetails.Parameters.AddWithValue("@pType", CheckDBNull(Type));
                                cmdPtabCaseDetails.Parameters.AddWithValue("@pExibitNumber", CheckDBNull(ExibitNo));

                                cmdPtabCaseDetails.ExecuteNonQuery();

                                sqlTrans.Commit();
                            }
                        }
                        catch (Exception ex)
                        {
                            sqlTrans.Rollback();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // InsertLog.WriteToFile("Error in connection to server. " + ex.Message);
                int linenum = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                LogFile.WriteToFile("Error Line :" + linenum + "-" + ex.Message); // Log
            }
        }
        public void UpdateMultiRunFlag(string CaseNo, string Caseid, string flg)
        {

            try
            {
                SqlTransaction sqlTrans;
                SqlCommand cmdPtabCaseDetails;
                m_sConnectionString = ConfigurationManager.AppSettings["LitigationConnectionString"];

                using (SqlConnection cnnUpdatePtabCase = new SqlConnection(m_sConnectionString))
                {
                    cnnUpdatePtabCase.Open();

                    using (sqlTrans = cnnUpdatePtabCase.BeginTransaction())
                    {
                        try
                        {
                            using (cmdPtabCaseDetails = new SqlCommand("usp_DeleteDocCaseno_TempTable", cnnUpdatePtabCase, sqlTrans))
                            {
                                cmdPtabCaseDetails.CommandType = CommandType.StoredProcedure;
                                cmdPtabCaseDetails.Parameters.AddWithValue("@Value", CheckDBNull(CaseNo));
                                cmdPtabCaseDetails.Parameters.AddWithValue("@Caseid", Caseid);
                                cmdPtabCaseDetails.Parameters.AddWithValue("@flg", flg);                              
                                cmdPtabCaseDetails.ExecuteNonQuery();
                                sqlTrans.Commit();
                            }
                        }
                        catch (Exception ex)
                        {
                            sqlTrans.Rollback();
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                LogFile.WriteToFile(" UpdateMultiRunFlag :" + ex.Message); // Log
            }
        }

        public void UpdateDownloadFlag(string CaseNo,int Mode)
        {
            _objSPNames = new SPNames();
            try
            {
                SqlTransaction sqlTrans;
                SqlCommand cmdPtabCaseDetails;

                m_sConnectionString = ConfigurationManager.AppSettings["LitigationConnectionString"];

                using (SqlConnection cnnUpdatePtabCase = new SqlConnection(m_sConnectionString))
                {
                    cnnUpdatePtabCase.Open();

                    using (sqlTrans = cnnUpdatePtabCase.BeginTransaction())
                    {
                        try
                        {
                            using (cmdPtabCaseDetails = new SqlCommand(_objSPNames.UpdateDownloadFlag, cnnUpdatePtabCase, sqlTrans))
                            {
                                cmdPtabCaseDetails.CommandType = CommandType.StoredProcedure;
                                cmdPtabCaseDetails.Parameters.AddWithValue("@pCaseNUmber", CheckDBNull(CaseNo));
                                cmdPtabCaseDetails.Parameters.AddWithValue("@Mode", Mode);
                               

                                cmdPtabCaseDetails.ExecuteNonQuery();

                                sqlTrans.Commit();
                            }
                        }
                        catch (Exception ex)
                        {
                            sqlTrans.Rollback();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // InsertLog.WriteToFile("Error in connection to server. " + ex.Message);
                int linenum = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                LogFile.WriteToFile("Error Line :" + linenum + "-" + ex.Message); // Log
            }
        }
        public int CheckISDoc_Lock(string caseno ,string caseID)
        {
            _objSPNames = new SPNames();
            int  ds =0;
            try
            {
                SqlTransaction sqlTrans;
                SqlCommand cmdPtabCaseDetails;

                m_sConnectionString = ConfigurationManager.AppSettings["LitigationConnectionString"];
                using (SqlConnection cnnPtabCase = new SqlConnection(m_sConnectionString))
                {
                    cnnPtabCase.Open();

                    using (sqlTrans = cnnPtabCase.BeginTransaction())
                    {
                        try
                        {
                            using (cmdPtabCaseDetails = new SqlCommand("usp_ptab_CheckDoc_log", cnnPtabCase, sqlTrans))
                            {
                                cmdPtabCaseDetails.CommandType = CommandType.StoredProcedure;
                                cmdPtabCaseDetails.Parameters.AddWithValue("@Value", CheckDBNull(caseno));
                                cmdPtabCaseDetails.Parameters.AddWithValue("@Caseid", caseID);                              
                               ds= Convert.ToInt32 (cmdPtabCaseDetails.ExecuteScalar());
                               sqlTrans.Commit();
                            }
                        }
                        catch (Exception)
                        {
                            sqlTrans.Rollback();
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return ds;
        }

        public  void UpdateCaseStatus(string status,int caseId,string DocName)
        {
            _objSPNames = new SPNames();
            DataSet ds = new DataSet();
            try
            {
                SqlTransaction sqlTrans;
                SqlCommand cmdPtabDownloadStatus;

                m_sConnectionString = ConfigurationManager.AppSettings["LitigationConnectionString"];
                using (SqlConnection cnnPtabCase = new SqlConnection(m_sConnectionString))
                {
                    cnnPtabCase.Open();

                   
                       //changed the stored procedure name
                            using (cmdPtabDownloadStatus = new SqlCommand("usp_update_PTAB_Download_Status_test", cnnPtabCase))
                            {
                                cmdPtabDownloadStatus.CommandType = CommandType.StoredProcedure;
                               
                                cmdPtabDownloadStatus.Parameters.AddWithValue("@caseId", caseId);
                                cmdPtabDownloadStatus.Parameters.AddWithValue("@docname", DocName);
                                cmdPtabDownloadStatus.ExecuteNonQuery();
                            }
                                       
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            
        }

        public DataSet GetCaseNUmberforDownload(int Mode)
        {
            _objSPNames = new SPNames();
            DataSet ds = new DataSet();
            try
            {
                SqlTransaction sqlTrans;
                SqlCommand cmdPtabCaseDetails; 

                 m_sConnectionString = ConfigurationManager.AppSettings["LitigationConnectionString"];
                using (SqlConnection cnnPtabCase = new SqlConnection(m_sConnectionString))
                {
                    cnnPtabCase.Open();

                    using (sqlTrans = cnnPtabCase.BeginTransaction())
                    {
                        try
                        {
                            string sp_name = ConfigurationManager.AppSettings["GetCaseDetailsProc"].ToString(); ;
                            using (cmdPtabCaseDetails = new SqlCommand(sp_name, cnnPtabCase, sqlTrans))
                            {
                                cmdPtabCaseDetails.CommandType = CommandType.StoredProcedure;
                                //cmdPtabCaseDetails.Parameters.AddWithValue("@Mode", Mode);

                                SqlDataAdapter sqdaa = new SqlDataAdapter(cmdPtabCaseDetails);

                                sqdaa.Fill(ds);
                            }
                        }
                        catch (Exception ex)
                        {
                            sqlTrans.Rollback();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                
                throw ex;
            }
            return ds;
        }

        private dynamic CheckDBNull(dynamic value)
        {
            if (value == null)
                return DBNull.Value;
            else if (string.IsNullOrEmpty(Convert.ToString(value)))
                return DBNull.Value;
            else
                return value;
        }
    }
}
