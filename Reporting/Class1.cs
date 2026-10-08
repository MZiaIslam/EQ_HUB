using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Reporting
{
    public class Rpt
    {
       
        public byte[] RunReport(string[] DSet, string[] SQL, string RptName, string ExportType, HttpContext oContext, bool bDownloadAttachment = false)
        {
            LocalReport viewer = new LocalReport();
            viewer.ReportEmbeddedResource = RptName;
            for (int i = 0; i < DSet.Length; i++)
            {
                DataTable dt = new DataTable();
                dt = FillDSet(SQL[i]).Tables[0];
                ReportDataSource datasource = new ReportDataSource(DSet[i], dt);
                viewer.DataSources.Add(datasource);
            }
            //viewer.Refresh();
            byte[] Contents = viewer.Render(ExportType);
            return Contents;
        }
    }
}
