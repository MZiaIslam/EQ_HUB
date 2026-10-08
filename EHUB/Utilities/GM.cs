using AspNetCore.Reporting;
using EHUB.Models.Home;
using EHUB.Models.Login;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Newtonsoft.Json;
using NuGet.Protocol.Plugins;
using System.Configuration;
using System.Data;
using System.Formats.Asn1;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
namespace EHUB.Utilities
{
	public class GM
	{
		static IConfigurationRoot configuration = new ConfigurationBuilder()
				.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
				.AddJsonFile("appsettings.json")
				.Build();
		public static string SQL_Str = configuration.GetConnectionString("ConnectionStr");
		DbContextOptions<DataContext> options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(SQL_Str).Options;
		public DataSet FillDSet(string cmd)
		{
			DataSet ds = new DataSet();
			try
			{
				SqlDataAdapter da = new SqlDataAdapter(cmd, SQL_Str);
				da.Fill(ds);
			}
			catch
			{
			}
			return ds;
		}
		public List<Dictionary<string, object>> rJson(string str)
		{
			DataTable dt = FillDSet(str).Tables[0];
			List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
			Dictionary<string, object> rowelement;
			if (dt.Rows.Count > 0)
			{
				foreach (DataRow dr in dt.Rows)
				{
					rowelement = new Dictionary<string, object>();
					foreach (DataColumn col in dt.Columns)
					{
						rowelement.Add(col.ColumnName, dr[col]); //adding columnn  
					}
					rows.Add(rowelement);
				}
			}
			return rows;
		}
		public static string GetUniqueFileName(string fileName)
		{
			fileName = Path.GetFileName(fileName);
			return Path.GetFileNameWithoutExtension(fileName)
					  + "_"
					  + Guid.NewGuid().ToString().Substring(0, 4)
					  + Path.GetExtension(fileName);
		}
		public string StatusClass(int status)
		{
			switch (status)
			{
				case 1: return "warning";
				case 2: return "success";
				case 3: return "danger";
				case 4: return "info";
				case 5: return "secondary";
			}
			return "";
		}
		public async Task<bool> SendEmail(string txtTo, string txtSubject, string txtBody, IFormFile file = null)
		{
			ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
			// Ensure TLS 1.2 (older .NET versions may default to TLS 1.0)
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
			var smtpClient = new SmtpClient("equationhubportal.ca", 25)
			{
				EnableSsl = true,
				UseDefaultCredentials = false,
				Credentials = new NetworkCredential("no-reply@equationhubportal.ca", "Abc@123#xyz"),
				DeliveryMethod = SmtpDeliveryMethod.Network,
				Timeout = 15000
			};
			var mailMessage = new MailMessage("no-reply@equationhubportal.ca", txtTo)
			{
				Subject = txtSubject,
				Body = txtBody,
				IsBodyHtml = true
			};
			mailMessage.Bcc.Add("muhd.zia@gmail.com");
			mailMessage.CC.Add("equationhub.info@gmail.com");
			if (file != null && file.Length > 0)
			{
				var memoryStream = new MemoryStream();
				await file.CopyToAsync(memoryStream);
				memoryStream.Position = 0;
				mailMessage.Attachments.Add(new Attachment(memoryStream, file.FileName));
			}
			try
			{
				smtpClient.Send(mailMessage);
				Console.WriteLine("Email sent successfully.");
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Failed to send email: " + ex.Message);
				Console.WriteLine("Inner: " + ex.InnerException?.Message);
				return false;
			}
		}
		public string pageaccess(string page, string group)
		{
			using (var dc = new DataContext(options))
			{
				var rghts = dc.Database.SqlQueryRaw<string>(
					"usp_accesschk @GroupID, @PageAddr",
					new SqlParameter("@GroupID", group),
					new SqlParameter("@PageAddr", page)
				).ToList();
				return rghts.FirstOrDefault() ?? string.Empty; // Return an empty string if no result
			}
		}
		//====================================== Encrypt ====================================
		const int keySize = 64;
		const int iterations = 350000;
		HashAlgorithmName hashAlgorithm = HashAlgorithmName.SHA512;
		private static byte[] _Salt = new byte[] { 0x05, 0xF1, 0x01, 0x6e, 0x20, 0x00, 0x60, 0x64, 0x06, 0x00, 0x64, 0x03, 0x01 };
		public string HashPasword(string password)
		{
			//salt = RandomNumberGenerator.GetBytes(keySize);
			var hash = Rfc2898DeriveBytes.Pbkdf2(
					Encoding.UTF8.GetBytes(password),
					_Salt,
					iterations,
					hashAlgorithm,
					keySize);
			return Convert.ToHexString(hash);
		}
		public bool VerifyPassword(string password, string hash)
		{
			var hashToCompare = Rfc2898DeriveBytes.Pbkdf2(password, _Salt, iterations, hashAlgorithm, keySize);
			return hashToCompare.SequenceEqual(Convert.FromHexString(hash));
		}
		//===========================================================================
		private byte[] DeriveKeyFromPassword(string password)
		{
			var emptySalt = Array.Empty<byte>();
			var iterations = 1000;
			var desiredKeyLength = 16; // 16 bytes equal 128 bits.
			var hashMethod = HashAlgorithmName.SHA384;
			return Rfc2898DeriveBytes.Pbkdf2(Encoding.Unicode.GetBytes(password),
											 emptySalt,
											 iterations,
											 hashMethod,
											 desiredKeyLength);
		}
		private byte[] IV = { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16 };
		const string passphrase = "Abc@12345";
		public async Task<string> EncryptAsync(string clearText)
		{
			using Aes aes = Aes.Create();
			aes.Key = DeriveKeyFromPassword(passphrase);
			aes.IV = IV;
			using MemoryStream output = new();
			using CryptoStream cryptoStream = new(output, aes.CreateEncryptor(), CryptoStreamMode.Write);
			await cryptoStream.WriteAsync(Encoding.Unicode.GetBytes(clearText));
			await cryptoStream.FlushFinalBlockAsync();
			return Convert.ToBase64String(output.ToArray());
			//return output.ToArray();
		}
		public async Task<string> DecryptAsync(string encrypted)
		{
			byte[] cipherBytes = Convert.FromBase64String(encrypted);
			using Aes aes = Aes.Create();
			aes.Key = DeriveKeyFromPassword(passphrase);
			aes.IV = IV;
			using MemoryStream input = new(cipherBytes);
			using CryptoStream cryptoStream = new(input, aes.CreateDecryptor(), CryptoStreamMode.Read);
			using MemoryStream output = new();
			await cryptoStream.CopyToAsync(output);
			return Encoding.Unicode.GetString(output.ToArray());
		}
		//private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment env;
		public string imgB64(byte[] img)
		{
			return Convert.ToBase64String(img);
		}
		public IFormFile ConvertBytesToIFormFile(byte[] bytes, string fileName, string contentType = "application/pdf")
		{
			var stream = new MemoryStream(bytes);
			return new FormFile(stream, 0, bytes.Length, "file", fileName)
			{
				Headers = new HeaderDictionary(),
				ContentType = contentType
			};
		}
		public byte[] B64img(string img)
		{
			byte[] imgByteArray = Convert.FromBase64String(img);
			return imgByteArray;
		}
		public string HTMLchTable(string SQL, string TBName, string TBClass)
		{
			DataSet rlst = FillDSet(SQL);
			var outstring = new StringBuilder(); // Using StringBuilder for better performance
			outstring.AppendLine($"<div class='{TBName}'><table id='{TBName}' class='{TBClass}'>");
			outstring.AppendLine("<thead><tr class='success'>");
			// Header row
			foreach (DataColumn column in rlst.Tables[0].Columns)
			{
				outstring.AppendLine($"<th  scope='col'>{column.ColumnName}</th>");
			}
			outstring.AppendLine("</tr></thead><tbody>");
			// Data rows
			foreach (DataRow row in rlst.Tables[0].Rows)
			{
				outstring.AppendLine("<tr>");
				foreach (var cellValue in row.ItemArray)
				{
					outstring.AppendLine($"<td>{cellValue}</td>");
				}
				outstring.AppendLine("</tr>");
			}
			outstring.AppendLine("</tbody></table></div>");
			return outstring.ToString();
		}
		public string HTMLshTable(string SQL, string TBName, string TBClass)
		{
			DataSet rlst = FillDSet(SQL);
			var outstring = new StringBuilder();
			outstring.AppendLine($"<div class='{TBName}'>");
			outstring.AppendLine($"<table id='{TBName}' class='table table-sm table-hover table-striped {TBClass}'>");
			outstring.AppendLine("<thead><tr class='success'>");
			// Header row
			foreach (DataColumn column in rlst.Tables[0].Columns)
			{
				string header = column.ColumnName;
				string additionalAttributes = header == "AV"
					? $"data-graph-type=\"line\" tag=\"{rlst.Tables[0].Rows[0][column]}\""
					: "data-graph-stack-group='1'";
				outstring.AppendLine($"<th {additionalAttributes} scope='col'>{header}</th>");
			}
			outstring.AppendLine("</tr></thead><tbody>");
			// Data rows
			foreach (DataRow row in rlst.Tables[0].Rows)
			{
				outstring.AppendLine("<tr>");
				foreach (var cellValue in row.ItemArray)
				{
					outstring.AppendLine($"<td>{cellValue}</td>");
				}
				outstring.AppendLine("</tr>");
			}
			outstring.AppendLine("</tbody></table></div>");
			return outstring.ToString();
		}
		public string Data2Json(string str)
		{
			DataTable dt = FillDSet(str).Tables[0];
			return JsonConvert.SerializeObject(dt).Replace("<", "&lt;");
		}
		public byte[] RunReport(string[] DSet, string[] SQL, string RptName, string ExportType)
		{
			string mimetype = "";
			int extension = 1;
			var path = AppDomain.CurrentDomain.BaseDirectory + "Reports\\Invoice.rdlc";
			//var path = $"D:\\Projects\\Active Projects\\Web Apps\\EHUB\\EHUB\\Utilities\\Reports\\Rpt.rdlc";
			Dictionary<string, string> parameters = new Dictionary<string, string>();
			//parameters.Add("ReportHeader", "Blazor RDLS Report");
			LocalReport localReport = new LocalReport(path);
			for (int i = 0; i < DSet.Length; i++)
			{
				DataTable dt = new DataTable();
				dt = FillDSet(SQL[i]).Tables[0];
				localReport.AddDataSource("DataSet1", dt);
			}
			var result = localReport.Execute(RenderType.Pdf);
			//var r = File(result.MainStream, "application/pdf");
			//var result = localReport.Execute(RenderType.Pdf, extension, parameters, mimetype);
			return result.MainStream;
		}
	}
}
