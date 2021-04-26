using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace MINEM.MIGI.Util
{
    public class Mailing
    {
        public enum Templates
        {
            [Description("RecuperarClave.html")]
            RecuperarClave
        }

        static string basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory);
        static string templateDirectory = ConfigurationManager.AppSettings["Mailing.TemplateDirectory"];
        static string imagesDirectory = ConfigurationManager.AppSettings["Mailing.ImagesDirectory"];
        static string displayNameFrom = ConfigurationManager.AppSettings["Mailing.Mail.From.DisplayName"];
        static string addressFrom = ConfigurationManager.AppSettings["Mailing.Mail.From.Address"];
        static string host = ConfigurationManager.AppSettings["Mailing.Smtp.Host"];
        static int port = Convert.ToInt32(ConfigurationManager.AppSettings["Mailing.Smtp.Port"]);
        static string credencialCorreo = ConfigurationManager.AppSettings["Mailing.Smtp.Credentials.UserName"];
        static string credencialContraseña = ConfigurationManager.AppSettings["Mailing.Smtp.Credentials.Password"];
        static bool enableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["Mailing.Smtp.EnableSsl"]);
        static bool useDefaultCredentials = Convert.ToBoolean(ConfigurationManager.AppSettings["Mailing.Smtp.UseDefaultCredentials"]);

        SmtpClient smtp = new SmtpClient(host, port);

        public Mailing()
        {
            smtp.EnableSsl = enableSsl;
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.UseDefaultCredentials = useDefaultCredentials;
            smtp.Credentials = new NetworkCredential(credencialCorreo, credencialContraseña);
        }


        void AddAddressCollection(MailAddressCollection addressCollectionField, MailAddressCollection addressCollection)
        {
            if (addressCollection != null)
            {
                foreach (MailAddress cc in addressCollection)
                {
                    addressCollectionField.Add(cc);
                }
            }
        }

        void ValidateDataBody(string templateBody, Dictionary<string, string> dataBody, string[] fields, string[] fieldsRequire, out string outTemplateBody, out string[] outFieldsNotExists, out string[] outFieldsRequireWithNullValue)
        {
            List<string> keysFieldNotExists = new List<string>();
            List<string> keysFieldRequireWithNullValue = new List<string>();

            outFieldsNotExists = keysFieldNotExists.ToArray();
            outFieldsRequireWithNullValue = keysFieldRequireWithNullValue.ToArray();
            outTemplateBody = templateBody;

            foreach (string key in fields)
            {
                if (!dataBody.ContainsKey(key)) keysFieldNotExists.Add(key);
                else
                {
                    if (fieldsRequire.Contains(key) && dataBody[key] == null) keysFieldRequireWithNullValue.Add(key);
                    else outTemplateBody = outTemplateBody.Replace(key, string.IsNullOrEmpty(dataBody[key]) ? "" : dataBody[key]);
                }
            }
        }

        void AddImages(AlternateView av, Dictionary<string, Dictionary<string, string>> images)
        {
            if (images != null)
            {
                foreach (KeyValuePair<string, Dictionary<string, string>> image in images)
                {
                    string directoryImages = Path.Combine(imagesDirectory);
                    string imageSource = $"{Path.Combine(basePath, directoryImages, image.Value.Keys.First())}";
                    if (!File.Exists(imageSource)) continue;
                    LinkedResource img = new LinkedResource(imageSource, image.Value.Values.First());
                    img.ContentId = image.Key;
                    av.LinkedResources.Add(img);
                }
            }
        }

        public bool SendMail(Templates template, Dictionary<string, string> dataBody, string[] fields, string[] fieldsRequire, string subject, MailAddressCollection addressTo, MailAddressCollection addressCC = null, MailAddressCollection addressCCO = null)
        {
            bool sendMail = false;

            try
            {
                string directoryTemplate = Path.Combine(templateDirectory);
                string fileName = template.GetAttributeOfType<DescriptionAttribute>().Description;
                string fullPathFile = Path.Combine(basePath, directoryTemplate, fileName);

                if (!File.Exists(fullPathFile)) throw new Exception($"No se encontró la ruta especificada: {fullPathFile}");

                string templateBody = File.ReadAllText(fullPathFile);

                string[] fieldsNotExists, fieldsRequireWithNullValue;

                ValidateDataBody(templateBody, dataBody, fields, fieldsRequire, out templateBody, out fieldsNotExists, out fieldsRequireWithNullValue);

                if (fieldsNotExists.Length > 0) throw new Exception($"No se encontraron los siguientes campos: {string.Join(", ", fieldsNotExists)}");
                if (fieldsRequireWithNullValue.Length > 0) throw new Exception($"Los siguientes campos requeridos no tienen valor: {string.Join(", ", fieldsRequireWithNullValue)}");

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(addressFrom, displayNameFrom);

                if (addressTo == null) throw new Exception("No existe datos del destinatario");
                if (addressTo.Count == 0) throw new Exception("No existe datos del destinatario");

                AddAddressCollection(mail.To, addressTo);
                AddAddressCollection(mail.CC, addressCC);
                AddAddressCollection(mail.Bcc, addressCCO);

                ServicePointManager.ServerCertificateValidationCallback = delegate (object s
                    , X509Certificate certificate
                    , X509Chain chain
                    , SslPolicyErrors sslPolicyErrors)
                { return true; };

                mail.Subject = subject;

                AlternateView viewHtml = AlternateView.CreateAlternateViewFromString(templateBody, Encoding.UTF8, MediaTypeNames.Text.Html);

                Dictionary<string, Dictionary<string, string>> images = new Dictionary<string, Dictionary<string, string>>
                {
                    ["imagenMEM"] = new Dictionary<string, string> { ["logo-minem.jpg"] = MediaTypeNames.Image.Jpeg },
                    //["imagenBanner"] = new Dictionary<string, string> { ["sres-logo.png"] = "image/png" },
                    ["imagenGEF"] = new Dictionary<string, string> { ["logo_gef.jpg"] = MediaTypeNames.Image.Jpeg },
                    ["imagenPNUD"] = new Dictionary<string, string> { ["logo_pnud.jpg"] = MediaTypeNames.Image.Jpeg },
                    //["imagenFRECUENTE"] = new Dictionary<string, string> { ["preguntas_frecuentes_email.jpg"] = MediaTypeNames.Image.Jpeg },
                    //["imagenCINTA"] = new Dictionary<string, string> { ["cinta.png"] = "image/png" },
                    //["imagenLAZO"] = new Dictionary<string, string> { ["lazo.png"] = "image/png" }
                };

                AddImages(viewHtml, images);

                mail.AlternateViews.Add(viewHtml);

                smtp.Send(mail);
                sendMail = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            return sendMail;
        }
    }
}
