using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Net;
using System.IO;
using System.Configuration;

namespace BusinessLayer
{
    public class SendMail
    {
        private  StringBuilder GetIndexNotificationContent(string Subject, string MessageBody)
        {
            StringBuilder Sup = new StringBuilder();
            Sup = Sup.Append("<style type='text/css'>");
            Sup = Sup.Append(".CommonFont {	font-family: Arial;	font-size: 12px; color:Gray;}");
            Sup = Sup.Append("</style>");
            Sup = Sup.Append("<table style='width:100%' class='MailTable'>");
            Sup = Sup.Append("<tr class='CommonFont'>");
            Sup = Sup.Append("<td align='left'>");
            Sup = Sup.Append("Dear Team,");
            Sup = Sup.Append("</tr>");

            Sup = Sup.Append("<tr class='CommonFont'>");
            Sup = Sup.Append("<td align='left'>");
            Sup = Sup.Append("&nbsp;</td>");
            Sup = Sup.Append("</tr>");

            Sup = Sup.Append("<tr class='CommonFont'>");
            Sup = Sup.Append("<td align='left'>");
            Sup = Sup.Append(" ##### </td>");
            Sup = Sup.Append("</tr>");

            Sup = Sup.Append("<tr class='CommonFont'>");
            Sup = Sup.Append("<td align='left'>");

            Sup = Sup.Append("<tr class='CommonFont'>");
            Sup = Sup.Append("<td align='left'>");

            Sup = Sup.Append("</td>");
            Sup = Sup.Append("</tr>");

            Sup = Sup.Append("<tr class='CommonFont'>");
            Sup = Sup.Append("<td align='left'>");
            Sup = Sup.Append("&nbsp;</td>");
            Sup = Sup.Append("<tr>");


            Sup = Sup.Append("<tr class='CommonFont'>");
            Sup = Sup.Append("<td align='left'>");
            Sup = Sup.Append("</br>Best Regards," + "</td>");
            Sup = Sup.Append("<tr>");

            Sup = Sup.Append("<tr class='CommonFont'>");
            Sup = Sup.Append("<td align='left'>");
            Sup = Sup.Append("Litigation Team" + "</td>");
            Sup = Sup.Append("<tr>");

            Sup = Sup.Append("<tr class='CommonFont'>");
            Sup = Sup.Append("<td align='center'>");
            Sup = Sup.Append("<hr></td>");
            Sup = Sup.Append("<tr>");

            Sup = Sup.Append("<tr class='CommonFont'>");
            Sup = Sup.Append("<td align='left'><b>");
            Sup = Sup.Append("PRIVILEGED AND CONFIDENTIAL COMMUNICATION" + "</b></td>");
            Sup = Sup.Append("<tr>");

            Sup = Sup.Append("<tr class='CommonFont'>");
            Sup = Sup.Append("<td align='justify'>");
            Sup = Sup.Append("This electronic transmission, and any documents attached hereto, may contain confidential and/or legally privileged information. The information is intended only for use by the recipient named above. If you have received this electronic message in error, please notify the sender and delete the electronic message. Any disclosure, copying, distribution, or use of the contents of information received in error is strictly prohibited." + "</td>");
            Sup = Sup.Append("<tr>");

            Sup = Sup.Append("<tr class='CommonFont'>");
            Sup = Sup.Append("<td align='left'><b>");
            Sup = Sup.Append("MaxVal Disclaimer" + "</b></td>");
            Sup = Sup.Append("<tr>");

            Sup = Sup.Append("<tr class='CommonFont'>");
            Sup = Sup.Append("<td align='justify'>");
            Sup = Sup.Append("The information provided in the attachment(s) has been obtained from sources deemed reliable. MaxVal does not guarantee the accuracy, completeness or adequacy of such information and expressly disclaims any liability or responsibility for the accuracy, errors, omissions or inadequacies in the enclosed information or for interpretations thereof. Any decisions or actions by any party based in any way whatsoever on the contents of the attachment(s) shall be the sole responsibility of that party." + "</td>");
            Sup = Sup.Append("<tr>");
            Sup = Sup.Append("</table>");
            return Sup;
        }
        public void SendMailMessage(string Subject, string MessageBody, int IsMailRequired, int IsAttachFilesRequired)
        {
            string serverDt;
            string logPath;

            serverDt = DateTime.Now.ToString("MM/dd/yyyy");
            serverDt = serverDt.Replace('/', '-');
            serverDt = serverDt.Replace(':', ' ');
          
            logPath = System.AppDomain.CurrentDomain.BaseDirectory + "\\Log";

            try
            {

                if (IsMailRequired == 1)
                {
                    string EndIndexMessageBody = "";
                    StringBuilder Sup = GetIndexNotificationContent(Subject, MessageBody);
                    EndIndexMessageBody = Sup.ToString();
                    EndIndexMessageBody = EndIndexMessageBody.Replace("#####", MessageBody);

                    string AdminReceiver = ConfigurationManager.AppSettings["AdminReceiver"].ToString();
                    string AdminReceiverName = ConfigurationManager.AppSettings["ReceiverName"].ToString();
                    string AuthenticationUserName = ConfigurationManager.AppSettings["AuthenticationUserName"].ToString();
                    string AuthenticationUserPassword = ConfigurationManager.AppSettings["AuthenticationUserPassword"].ToString();
                    string SMTPHost = ConfigurationManager.AppSettings["SMTPHost"].ToString();
                    string SMTPPort = ConfigurationManager.AppSettings["SMTPPort"].ToString();
                    bool IsBodyHtml = Convert.ToBoolean(ConfigurationManager.AppSettings["IsBodyHtml"].ToString());
                    string SenderMailID = ConfigurationManager.AppSettings["SenderMailID"].ToString();
                    string SenderDisplayName = ConfigurationManager.AppSettings["SenderDisplayName"].ToString();
                    string ReceiverCC = ConfigurationManager.AppSettings["ReceiverCC"].ToString();
                    string ReceiverNameCC = ConfigurationManager.AppSettings["ReceiverNameCC"].ToString();
                    string[] MailCC = new string[100];
                    string[] NameCC = new string[100];

                    string[] ReceiverMail = AdminReceiver.Split(',');
                    string[] ReceiverName = AdminReceiverName.Split(',');
                    if (ReceiverCC != string.Empty && ReceiverNameCC != string.Empty)
                    {
                        MailCC = ReceiverCC.Split(',');
                        NameCC = ReceiverNameCC.Split(',');
                    }

                    Dictionary<string, string> Receiver = new Dictionary<string, string>();
                    Dictionary<string, string> ReceiverCCMail = new Dictionary<string, string>();

                    for (int i = 0; i < ReceiverMail.Count(); i++)
                    {
                        string mail = ReceiverMail[i];
                        string name = ReceiverName[i];
                        Receiver.Add(mail, name);
                    }
                    for (int i = 0; i < MailCC.Count(); i++)
                    {
                        string ccmail = MailCC[i];
                        string CCName = NameCC[i];
                        ReceiverCCMail.Add(ccmail, CCName);
                    }
                    MailMessage objMailMessage = new MailMessage();
                    objMailMessage.From = new MailAddress(SenderMailID, SenderDisplayName);

                    foreach (KeyValuePair<string, string> ReceiverInfo in Receiver)
                    {
                        objMailMessage.To.Add(new MailAddress(ReceiverInfo.Key, ReceiverInfo.Value));
                    }

                    if (ReceiverCCMail.Count > 0)
                    {
                        foreach (KeyValuePair<string, string> ReceiverCCInfo in ReceiverCCMail)
                        {
                            objMailMessage.CC.Add(new MailAddress(ReceiverCCInfo.Key, ReceiverCCInfo.Value));
                        }
                    }
                    objMailMessage.Priority = MailPriority.High;
                    objMailMessage.Body = EndIndexMessageBody;
                    objMailMessage.IsBodyHtml = IsBodyHtml;
                    objMailMessage.Subject = Subject;
                    if (IsAttachFilesRequired == 1)
                    {
                        string[] AttachedFiles = Directory.GetFiles(logPath, "PTABDocumentDownloadLog_" + serverDt + ".txt");
                        //Adding attached files into mail
                        if (AttachedFiles != null && AttachedFiles.Length > 0)
                        {
                            foreach (string AttachedFilePath in AttachedFiles)
                            {
                                if (AttachedFilePath + "" != "")
                                {
                                    FileStream inStream = File.OpenRead(AttachedFilePath);
                                    MemoryStream storeStream = new MemoryStream();
                                    // copy all data from in to store
                                    storeStream.SetLength(inStream.Length);
                                    inStream.Read(storeStream.GetBuffer(), 0, (int)inStream.Length);
                                    storeStream.Flush();
                                    inStream.Close();
                                    string filename = AttachedFilePath.Substring(AttachedFilePath.LastIndexOf("\\"));

                                    filename = filename.Replace("\\", "");

                                    Attachment myAttachment = new Attachment(storeStream, filename);
                                    objMailMessage.Attachments.Add(myAttachment);
                                }
                            }
                        }
                    }

                    using (SmtpClient objSMTPClient = new SmtpClient(SMTPHost))
                    {
                        objSMTPClient.Port = Convert.ToInt32(SMTPPort);
                        objSMTPClient.Credentials = new NetworkCredential(AuthenticationUserName, AuthenticationUserPassword);
                        objSMTPClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                       // objSMTPClient.Send(objMailMessage);
                    }


                }

            }
            catch (Exception ex)
            {
                LogFile.WriteToFile("Mail Send Failed because of " + ex.ToString());

            }
        }
    }
}
