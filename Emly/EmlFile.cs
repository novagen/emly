using kenjiuno.LEML;
using System;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Emly
{
	public class EmlFile
	{
		public string Sender { get; set; }
		public string Subject { get; set; }
		public string FilePath { get; set; }
		public DateTime Created { get; set; }

		public EmlFile(string path)
		{
			FilePath = path;
			var mail = GetEml();

			if (mail != null)
			{
				Subject = mail.Subject;
				Sender = mail.From;
				Created = mail.Date ?? DateTime.MinValue;
			}
			else
			{
				throw new IOException("File does not exist");
			}
		}

		public string FormattedSubject
		{
			get
			{
				return Decode(Subject);
			}
		}

		public string DateFormattedSubject
		{
			get
			{
				return Created.ToShortDateString() + " " + Created.ToShortTimeString() + " - " + Decode(Subject);
			}
		}

		public static string Decode(string value)
		{
			var attachment = Attachment.CreateAttachmentFromString("", value);
			return attachment.Name;
		}

		public string GetHtml()
		{
			var mail = GetEml();

			if (mail != null)
			{
				if (mail.multiparts.Any(c => c.ContentType == "text/html"))
				{
					return mail.multiparts.First(c => c.ContentType == "text/html").MessageBody;
				}

				return mail.MessageBody;
			}

			return string.Empty;
		}

		private EML GetEml()
		{
			if (File.Exists(FilePath))
			{
				return new EML(Mail.FromFile(FilePath));
			}

			return null;
		}
	}
}