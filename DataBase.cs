using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Data.SqlClient;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Linq;
namespace WebAppForPpt
{

    /// <summary>
    /// DataBase 的摘要说明
    /// </summary>
    public class DataBase
    {
        public DataBase()
        {
            //
            // TODO: 在此处添加构造函数逻辑
            //
        }
        public static SqlConnection createConnection()
        {
            //数据库连接字符串
            SqlConnection cnn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DBConn"].ConnectionString);
            return cnn;
        }
        public static DataTable Get_Table(string sqlStr)
        {
            //定义数据库连接　将要查询的数据集　填充到DataSet中
            SqlConnection tmpCnn = DataBase.createConnection();
            if (tmpCnn.State != 0)
            {
                tmpCnn.Close();
            }
            tmpCnn.Open();
            SqlDataAdapter cmd = new SqlDataAdapter(sqlStr, tmpCnn);
            DataSet tmpDataSet = new DataSet();
            cmd.Fill(tmpDataSet);
            return tmpDataSet.Tables[0];
        }

        public static string ExecSql(string name, string sqlStr)
        {
            //定义数据库连接　　执行数据库的增加　修改和删除数据的功能
            SqlConnection tmpCnn = DataBase.createConnection();
            if (tmpCnn.State != 0)
            {
                tmpCnn.Close();
            }
            tmpCnn.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = tmpCnn;
            cmd.CommandText = "select 1 from  dbo.tz_administrator where adm_un='" + name + "'";
            if (Convert.ToString(cmd.ExecuteScalar()) == "1")
            {
                return "此账号已被注册，请直接登录。";
            }
            cmd.CommandText = sqlStr;
            cmd.ExecuteNonQuery();
            return "";
        }

        public static void ExceUpLoad(string[] strs)
        {
            string xzcs = strs[5];
            if (string.IsNullOrEmpty(xzcs) == true)
            {
                xzcs = "0";
            }
            string sql = "insert into dbo.ppt_mode_save values(newid(),'" + strs[0] + "','" + strs[1] + "','0" + strs[2] + "','" + strs[3] +
                "','" + strs[4] + "','" + strs[5] + "','gly',getdate(),'gly',getdate())";
            SqlConnection tmpCnn = DataBase.createConnection();
            if (tmpCnn.State != 0)
            {
                tmpCnn.Close();
            }
            tmpCnn.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = tmpCnn;
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
        }
        public static string ExceCheck(string[] strs)
        {
            //定义数据库连接　　执行数据库的增加　修改和删除数据的功能
            SqlConnection tmpCnn = DataBase.createConnection();
            if (tmpCnn.State != 0)
            {
                tmpCnn.Close();
            }
            tmpCnn.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = tmpCnn;
            cmd.CommandText = "select 1 from  dbo.ppt_mode_save where img_path='" + strs[0] + "'";
            if (Convert.ToString(cmd.ExecuteScalar()) == "1")
            {
                return "上传图片名" + strs[0] + "已经存在";
            }
            cmd.CommandText = "select 1 from  dbo.ppt_mode_save where ppt_path='" + strs[1] + "'";
            if (Convert.ToString(cmd.ExecuteScalar()) == "1")
            {
                return "上传ppt名" + strs[1] + "已经存在";
            }
            return "";
        }
        /// <summary>
        /// str :lx^热门最新^页数     例：2^1^2
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public string GetData(string str)
        {
            string[] strs = str.Split('^');
            string lx = strs[0];
            string rmzx = strs[1];
            string page = strs[2];
            string gjz = strs[3];//关键字
            string json = string.Empty;
            string MainSql = @"SELECT TOP 32 img_path,ppt_path,xzcs from  dbo.ppt_mode_save WHERE sjnm NOT IN(
                    SELECT TOP {0} sjnm FROM ppt_mode_save  WHERE 
                        (sjlx like '%{1}%' OR sjlx = '{1}' OR sjlx like '%{1}' OR sjlx like '{1}%') 
                   
                    {2}
                    ) AND (sjlx like '%{1}%' OR sjlx = '{1}' OR sjlx like '%{1}' OR sjlx like '{1}%') 
                    
                      {2}
                     ";
            string gjzWhere = "  AND (gjz like '%{0}%' OR gjz = '{0}' OR gjz like '%{0}' OR gjz like '{0}%')  ";
            string where = string.Empty;

            if (string.IsNullOrEmpty(gjz) == false)
            {
                gjzWhere = string.Format(gjzWhere, gjz);
            }
            int num = 32 * (Convert.ToInt32(page) - 1);
            string sql = string.Empty;
            if (string.IsNullOrEmpty(gjz) == false)//如果关键字不为空
            {
                sql = string.Format(MainSql, num.ToString(), lx, gjzWhere);
                if (rmzx == "2")
                {
                    sql += "ORDER BY CreatedDate DESC ";
                }
                else
                {
                    sql += "ORDER BY xzcs DESC ";

                }
            }
            else
            {
                sql = string.Format(MainSql, num.ToString(), lx, "");
                if (rmzx == "2")
                {
                    sql += "ORDER BY CreatedDate DESC ";
                }
                else
                {
                    sql += "ORDER BY xzcs DESC ";

                }
            }
            SqlConnection tmpCnn = DataBase.createConnection();
            if (tmpCnn.State != 0)
            {
                tmpCnn.Close();
            }
            tmpCnn.Open();
            SqlDataAdapter da = new SqlDataAdapter(sql, tmpCnn);
            DataSet ds = new DataSet();
            da.Fill(ds);
            int pageNum = ds.Tables[0].Rows.Count;
            int ys = pageNum / 32;
            DataTable dt = ds.Tables[0].Copy();
            dt.TableName="testNmae";
            ds.Tables.Add(dt);
            DataTable dtt = new DataTable();
            ds.Tables.Add(dtt);
            if (ds.Tables[0].Rows.Count > 0)
            {
                string strr = DataSetToJson(ds);
                JsonToDataSet(strr);
                return DataSetToJson(ds);
            }
            else
            {
                return "";
            }
        }

        public  DataSet JsonToDataSet( string json)
        {
            DataSet ds = new DataSet();
            try
            {
                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                javaScriptSerializer.MaxJsonLength = Int32.MaxValue; //取得最大数值
                ArrayList arrayList = javaScriptSerializer.Deserialize<ArrayList>(json);
                if (arrayList.Count > 0)
                {
                    DataTable dataTable = null;
                    foreach (Dictionary<string, object> item in arrayList)
                    {

                        dataTable = new DataTable();  //实例化
                        dataTable.TableName = item.Keys.First();
                        foreach (Dictionary<string, object> dictionary in (ArrayList)item.Values.First())
                        {
                            if (dictionary.Keys.Count == 0)
                            {
                                break;
                            }
                            if (dataTable.Columns.Count == 0)
                            {
                                foreach (string current in dictionary.Keys)
                                {
                                    dataTable.Columns.Add(current, dictionary[current].GetType());
                                }
                            }
                            DataRow dataRow = dataTable.NewRow();
                            foreach (string current in dictionary.Keys)
                            {
                                dataRow[current] = dictionary[current];
                            }

                            dataTable.Rows.Add(dataRow); //循环添加行到DataTable中
                        }
                        ds.Tables.Add(dataTable);
                    }
                }
            }
            catch
            {
            }
            return ds;
        }

        public  string DataSetToJson( DataSet ds)
        {
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue; //取得最大数值
            ArrayList arrayList_z = new ArrayList();
            foreach (DataTable dt in ds.Tables)
            {
                ArrayList arrayList = new ArrayList();
                foreach (DataRow dataRow in dt.Rows)
                {
                    Dictionary<string, object> dictionary = new Dictionary<string, object>();  //实例化一个参数集合
                    foreach (DataColumn dataColumn in dt.Columns)
                    {
                        dictionary.Add(dataColumn.ColumnName, dataRow[dataColumn.ColumnName].ToString());
                    }
                    arrayList.Add(dictionary); //ArrayList集合中添加键值
                }
                Dictionary<string, object> dictionary2 = new Dictionary<string, object>();  //实例化一个参数集合
                dictionary2.Add(dt.TableName, arrayList);
                arrayList_z.Add(dictionary2);
            }

            return javaScriptSerializer.Serialize(arrayList_z);  //返回一个json字符串
        }
        public string GetJson(DataTable dt, string ys)
        {
            string imgpath = "http://180.76.144.72//ppt_image//";
            string pptpath = "http://180.76.144.72//ppt_ppt//";
            StringBuilder MainMsg = new StringBuilder();
            /*
            示例：
            [{"imgName":"ppt.jpg","pptName":"ppt.ppt","DownLoadNum":"21"},{"imgName":"ppt2.jpg","pptName":"ppt2.ppt","DownLoadNum":"31"}]
            */
            MainMsg.Append("[");
            foreach (DataRow item in dt.Rows)
            {
                MainMsg.Append("{\"imgName\":\"" + imgpath + "");
                MainMsg.Append(item[0]);
                MainMsg.Append("\",\"pptName\":\"" + pptpath + "");
                MainMsg.Append(item[1]);
                MainMsg.Append("\",\"DownLoadNum\":\"");
                MainMsg.Append(item[2]);
                MainMsg.Append("\"},");
            }

            string jsonStr = MainMsg.ToString().TrimEnd(',') + "]";
            string json = "{\"MainMsg\": " + jsonStr + ",\"PageNum\":\"" + ys + "\"}";
            return json;
        }
        public DataTable GetDataTable(string str)
        {
            string[] strs = str.Split('^');
            string lx = strs[0];
            string rmzx = strs[1];
            string page = strs[2];
            string gjz = strs[3];//关键字
            string json = string.Empty;
            string MainSql = @"SELECT TOP 32 img_path,ppt_path,xzcs from  dbo.ppt_mode_save WHERE sjnm NOT IN(
                    SELECT TOP {0} sjnm FROM ppt_mode_save  WHERE 
                        (sjlx like '%{1}%' OR sjlx = '{1}' OR sjlx like '%{1}' OR sjlx like '{1}%') 
                   
                    {2}   {3}
                    ) AND (sjlx like '%{1}%' OR sjlx = '{1}' OR sjlx like '%{1}' OR sjlx like '{1}%') 
                    
                      {2}  {3}
                     ";
            string gjzWhere = "  AND (gjz like '%{0}%' OR gjz = '{0}' OR gjz like '%{0}' OR gjz like '{0}%')  ";
            string where = string.Empty;

            if (string.IsNullOrEmpty(gjz) == false)
            {
                gjzWhere = string.Format(gjzWhere, gjz);
            }
            int num = 32 * (Convert.ToInt32(page) - 1);
            string sql = string.Empty;
            string orderby = string.Empty;
            if (string.IsNullOrEmpty(gjz) == false)//如果关键字不为空
            {
                if (rmzx == "2")
                {
                    orderby = "ORDER BY CreatedDate DESC ";
                }
                else
                {
                    orderby = "ORDER BY xzcs DESC ";

                }
                sql = string.Format(MainSql, num.ToString(), lx, gjzWhere, orderby);
            }
            else
            {
                if (rmzx == "2")
                {
                    orderby = "ORDER BY CreatedDate DESC ";
                }
                else
                {
                    orderby = "ORDER BY xzcs DESC ";

                }
                sql = string.Format(MainSql, num.ToString(), lx, "", orderby);
            }
            SqlConnection tmpCnn = DataBase.createConnection();
            if (tmpCnn.State != 0)
            {
                tmpCnn.Close();
            }
            tmpCnn.Open();
            SqlDataAdapter da = new SqlDataAdapter(sql, tmpCnn);
            DataSet ds = new DataSet();
            da.Fill(ds);
            da.Dispose();
            tmpCnn.Close();
            int pageNum = ds.Tables[0].Rows.Count;
            int ys = pageNum / 32;
            if (ds.Tables[0].Rows.Count > 0)
            {
                return ds.Tables[0];
            }
            else
            {
                return ds.Tables[0];

            }
        }
        public int GetDataPage(string str)
        {
            string[] strs = str.Split('^');
            string lx = strs[0];
            string rmzx = strs[1];
            string page = strs[2];
            string gjz = strs[3];//关键字
            string json = string.Empty;
            string MainSql = @"SELECT count(1) from  dbo.ppt_mode_save WHERE  (sjlx like '%{0}%' OR sjlx = '{0}' OR sjlx like '%{0}' OR sjlx like '{0}%') 
                      {1}
                     ";
            string gjzWhere = "  AND (gjz like '%{0}%' OR gjz = '{0}' OR gjz like '%{0}' OR gjz like '{0}%')  ";
            string where = string.Empty;

            if (string.IsNullOrEmpty(gjz) == false)
            {
                gjzWhere = string.Format(gjzWhere, gjz);
            }
            string sql = string.Empty;
            if (string.IsNullOrEmpty(gjz) == false)//如果关键字不为空
            {
                sql = string.Format(MainSql, lx, gjzWhere);
            }
            else
            {
                sql = string.Format(MainSql, lx, "");
            }
            SqlConnection tmpCnn = DataBase.createConnection();
            if (tmpCnn.State != 0)
            {
                tmpCnn.Close();
            }
            tmpCnn.Open();
            SqlCommand da = new SqlCommand(sql, tmpCnn);
            int pageNum = Convert.ToInt32(da.ExecuteScalar());
            da.Dispose();
            tmpCnn.Close();
            return pageNum / 32 + 1;
        }
        public void AddXZCS(string str)
        {
            string sql = @"UPDATE dbo.ppt_mode_save SET xzcs=xzcs+1 WHERE sjnm=(SELECT TOP 1 sjnm FROM dbo.ppt_mode_save
                            WHERE ppt_path='{0}')";
            sql = string.Format(sql, str);
            SqlConnection tmpCnn = DataBase.createConnection();
            if (tmpCnn.State != 0)
            {
                tmpCnn.Close();
            }
            tmpCnn.Open();
            SqlCommand da = new SqlCommand(sql, tmpCnn);
            da.ExecuteNonQuery();
            da.Dispose();
            tmpCnn.Close();
        }
        public void RecordLoad(string users, string ip)
        {
            string sql = "INSERT INTO ppt_administrator(Adm_name,Adm_pw,isvip,LastLoasTime) VALUES('{0}','{1}','0',getdate())";
            sql = string.Format(sql, users, ip);
            SqlConnection tmpCnn = DataBase.createConnection();
            if (tmpCnn.State != 0)
            {
                tmpCnn.Close();
            }
            tmpCnn.Open();
            SqlCommand da = new SqlCommand(sql, tmpCnn);
            da.ExecuteNonQuery();
            da.Dispose();
            tmpCnn.Close();
        }
        public void RecordDwonLoad(string users, string ip, string pptname)
        {
            string sql = "INSERT INTO ppt_administrator(Adm_name,Adm_pw,downloadcontent,isvip,LastLoasTime) VALUES('{0}','{1}','{2}','0',getdate())";
            sql = string.Format(sql, users, ip, pptname);
            SqlConnection tmpCnn = DataBase.createConnection();
            if (tmpCnn.State != 0)
            {
                tmpCnn.Close();
            }
            tmpCnn.Open();
            SqlCommand da = new SqlCommand(sql, tmpCnn);
            da.ExecuteNonQuery();
            da.Dispose();
            tmpCnn.Close();
        }
        public DataTable GetUserLoadMsg()
        {
            //定义数据库连接　将要查询的数据集　填充到DataSet中
            SqlConnection tmpCnn = DataBase.createConnection();
            if (tmpCnn.State != 0)
            {
                tmpCnn.Close();
            }
            tmpCnn.Open();
            string sql = "SELECT TOP 200 adm_name AS userip ,'系统测试用户' as username,'' AS useraddr,lastloastime,'' as downloadcontent FROM ppt_administrator where downloadcontent is null ORDER BY LastLoasTime desc ";
            SqlDataAdapter cmd = new SqlDataAdapter(sql, tmpCnn);
            DataSet tmpDataSet = new DataSet();
            cmd.Fill(tmpDataSet);
            return tmpDataSet.Tables[0];
        }
        public DataTable GetDownLoadMsg()
        {
            //定义数据库连接　将要查询的数据集　填充到DataSet中
            SqlConnection tmpCnn = DataBase.createConnection();
            if (tmpCnn.State != 0)
            {
                tmpCnn.Close();
            }
            tmpCnn.Open();
            string sql = "SELECT TOP 200 adm_name AS userip ,'系统测试用户' as username,'' AS useraddr,lastloastime,downloadcontent FROM ppt_administrator where downloadcontent IS not null ORDER BY LastLoasTime desc ";
            SqlDataAdapter cmd = new SqlDataAdapter(sql, tmpCnn);
            DataSet tmpDataSet = new DataSet();
            cmd.Fill(tmpDataSet);
            return tmpDataSet.Tables[0];
        }
    }
}
