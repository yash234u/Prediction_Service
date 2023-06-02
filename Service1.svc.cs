using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using System.Web;

namespace Prediction_Service
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
     [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class Service1 : IService1
    {
        string constr = ConfigurationManager.ConnectionStrings["connect"].ConnectionString;
        SqlConnection con;
        SqlCommand cmd;
        SqlDataAdapter da;
        DataTable dt;

        public ResponseMessage registerUser(User user)
        {
            ResponseMessage resp = new ResponseMessage();

            try
            {
                con = new SqlConnection(constr);
                cmd = new SqlCommand("insert into User_master values(@iName,@iEmailID,@iPassword,@iMobileNo)", con);         
                cmd.Parameters.AddWithValue("@iName", user.Name);
                cmd.Parameters.AddWithValue("@iEmailID", user.EmailID);
                cmd.Parameters.AddWithValue("@iPassword", user.Password);
                cmd.Parameters.AddWithValue("@iMobileNo", user.MobileNo);
                da = new SqlDataAdapter(cmd);
                dt = new DataTable();
                da.Fill(dt);
                int result = cmd.ExecuteNonQuery();
                if (result > 0)
                {
                    resp.Status = 400;
                    resp.Message = "Successfull";
                }
                else
                {
                    resp.Status = 409;
                    resp.Message = "Something went wrong";
                }

            }
            catch (Exception ex)
            {
                resp.Status = 409;
                resp.Message = ex.ToString();
            }
            return resp;
        }

        public ResponseMessage addnews(News user)
        {
            ResponseMessage resp = new ResponseMessage();

            try
            {

                string file_name = "";
                 string filePath = "";

                Random r = new Random();
                file_name = r.Next(200, 10000).ToString();

                byte[] imagebytes = Convert.FromBase64String(user.News_image);
                filePath = HttpContext.Current.Server.MapPath("~/Images/" + Path.GetFileName(file_name) + ".jpg");
                string path = "Images/" + Path.GetFileName(file_name) + ".jpg";
                File.WriteAllBytes(filePath, imagebytes);   // write image to food_images folder

        
          
                con = new SqlConnection(constr);
                con.Open();
                cmd = new SqlCommand("insert into News_master values(@News_Title,@News_Headline,@News_Discription,@News_image)", con);
                cmd.Parameters.AddWithValue("@News_Title", user.News_Title);
                cmd.Parameters.AddWithValue("@News_Headline", user.News_Headline);
                cmd.Parameters.AddWithValue("@News_Discription", user.News_Discription);
                cmd.Parameters.AddWithValue("@News_image", path);
           
                int result = cmd.ExecuteNonQuery();
                if (result > 0)
                {
                    resp.Status = 400;
                    resp.Message = "Successfull";
                }
                else
                {
                    resp.Status = 409;
                    resp.Message = "Something went wrong";
                }

            }
            catch (Exception ex)
            {
                resp.Status = 409;
                resp.Message = ex.ToString();
            }
            return resp;
        }

        public ResponseMessage Feedback(string data)
        {
            ResponseMessage resp = new ResponseMessage();
            try
            {
                con = new SqlConnection(constr);
                con.Open();
                cmd = new SqlCommand("insert into Notice values(@Notice_disc,getdate())", con);
                cmd.Parameters.AddWithValue("@Notice_disc", data);
                int abs = cmd.ExecuteNonQuery();
                if (abs > 0)
                {
                    resp.Status = 400;
                    resp.Message = "SuccessFull";
                }
                else
                {
                    resp.Status = 409;
                    resp.Message = "Something went wrong";
                }
            }
            catch (Exception ex)
            {
                resp.Status = 409;
                resp.Message = ex.ToString();
            }
            return resp;
        }

        public List<News> getQuestions()
        {
            List<News> list = new List<News>();

            try
            {
                con = new SqlConnection(constr);
                cmd = new SqlCommand("Select * from News_master", con);
                da = new SqlDataAdapter(cmd);
                dt = new DataTable();
                da.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        News a = new News
                        {
                            News_Title = dt.Rows[i]["News_Title"].ToString(),

                            News_Headline = dt.Rows[i]["News_Headline"].ToString(),
                            News_Discription = dt.Rows[i]["News_Discription"].ToString(),
                            News_image = dt.Rows[i]["News_image"].ToString(),
                          
                        };
                        list.Add(a);
                    }
                }
            }
            catch (Exception ex)
            {
               
            }
            return list;
        }

        public List<Notice> getNotice()
        {
            List<Notice> list = new List<Notice>();
            try
            {
                con = new SqlConnection(constr);
                cmd = new SqlCommand("Select * from Notice Order by Date Desc", con);
                da = new SqlDataAdapter(cmd);
                dt = new DataTable();
                da.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        Notice u = new Notice
                        {
                            Notice_id = dt.Rows[i]["Notice_id"].ToString(),
                            Notice_Disc = dt.Rows[i]["Notice_Disc"].ToString(),
                            Date = dt.Rows[i]["Date"].ToString(),
                                              
                        };
                        list.Add(u);
                    }
                }
            }
            catch(Exception ex)
            {

            }
            return list;
        }

        public ResponseMessage forgotPassword(string EmailID)
        {
            try
            {
                con = new SqlConnection(constr);
                cmd = new SqlCommand("select * from mstUser where EmailID = @iEmailID", con);
                cmd.Parameters.AddWithValue("@iEmailID", EmailID);
                da = new SqlDataAdapter(cmd);
                dt = new DataTable();
                da.Fill(dt);
                if(dt.Rows.Count >0)
                {
                    string Password = dt.Rows[0]["Password"].ToString();
                    if (sendMail(EmailID, "Your password is:"+Password)) 
                    {
                        return new ResponseMessage { Message = "Please check your email" };
                    }
                    else
                    {
                        return new ResponseMessage { Message = "Error while sending email" };
                    }
                }
                else
                {
                    return new ResponseMessage { Message = "Something went wrong" };
                }
            }
            catch(Exception ex)
            {
                return new ResponseMessage { Message = ex.ToString() };
            }
        }

        private bool sendMail(string email, string body)     // send email when status updates
        {
            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("demoproject.in");

                mail.From = new MailAddress("test@demoproject.in");
                //recipient address
                mail.To.Add(new MailAddress(email));    // receiver's email address
                mail.Subject = "New Notification";
                mail.Body = body;
                SmtpServer.Port = 25;
                SmtpServer.Credentials = new System.Net.NetworkCredential("test@demoproject.in", "Password@123");
                SmtpServer.Send(mail);
                return true;
            }
            catch (Exception ex)
            {
                return false;
                throw ex;        
            }
        }



    }
}
