using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;

namespace SimpleUtils {
    public class SimpleNotifyMailerException : Exception {
        public SimpleNotifyMailerException( string message ) : base( message ) { }
    }

    public class SimpleNotifyAttachmentStream : MemoryStream, IDisposable {
        public override void Close() {
            // Don't allow closing the normal way, so the report can't close it.
        }

        public new void Dispose() {
            base.Close();
            base.Dispose();
        }
    }

    public class SimpleNotifyAttachment : IDisposable {
        public string FileName { get; set; }
        public SimpleNotifyAttachmentStream Contents { get; set; }

        public SimpleNotifyAttachment( string fileNameIn, SimpleNotifyAttachmentStream contentsIn ) {
            this.FileName = fileNameIn;
            this.Contents = contentsIn;
        }

        public void Dispose() {
            this.Contents.Dispose();
        }
    }

    public class SimpleNotifyMailer {

        public class Options {
            public string Server { get; set; }
            public string Port { get; set; }
            public string FromAddress { get; set; }
            public string[] ToAddresses { get; set; }
        }

        private SmtpClient notifyClient;
        private string serverName;
        private string fromAddress;

        public SimpleNotifyMailer( string serverNameIn, string mailHostIn, int mailPortIn, string fromAddressIn ) {
            this.serverName = serverNameIn;
            this.fromAddress = fromAddressIn;

            this.notifyClient = new SmtpClient( mailHostIn, mailPortIn );
            this.notifyClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            this.notifyClient.UseDefaultCredentials = false;
        }

        public static void SetNotifyOptionsRegistry( string appKeyIn, SimpleNotifyMailer.Options optionsIn ) {
            // TODO        
        }

        public static void SetNotifyOptionsRegistry( string appKeyIn, string serverIn, string portIn, string fromAddressIn, string[] toAddressesIn ) {
            Registry.SetValue(
                "HKEY_LOCAL_MACHINE\\Software\\" + appKeyIn,
                "NotifyMailAddresses",
                string.Join( ",", toAddressesIn )
            );
            Registry.SetValue(
                "HKEY_LOCAL_MACHINE\\Software\\" + appKeyIn,
                "NotifyMailServer",
                serverIn
            );
            Registry.SetValue(
                "HKEY_LOCAL_MACHINE\\Software\\" + appKeyIn,
                "NotifyMailFromAddress",
                fromAddressIn
            );
            try {
                Registry.SetValue(
                    "HKEY_LOCAL_MACHINE\\Software\\" + appKeyIn,
                    "NotifyMailPort",
                    int.Parse( portIn ).ToString()
                );
            } catch( FormatException ex ) {
                throw new SimpleNotifyMailerException( ex.Message );
            }
        }

        public static SimpleNotifyMailer.Options GetNotifyOptionsRegistry( string appKeyIn ) {
            SimpleNotifyMailer.Options optionsOut = new SimpleNotifyMailer.Options();

            optionsOut.Server = (string)Registry.GetValue(
                "HKEY_LOCAL_MACHINE\\Software\\" + appKeyIn,
                "NotifyMailServer",
                "127.0.0.1"
            );
            if( null == optionsOut.Server ) {
                optionsOut.Server = "";
            }

            optionsOut.Port = (string)Registry.GetValue(
                "HKEY_LOCAL_MACHINE\\Software\\" + appKeyIn,
                "NotifyMailPort",
                "25"
            );
            if( null == optionsOut.Port ) {
                optionsOut.Port = "";
            }
            
            optionsOut.FromAddress = (string)Registry.GetValue(
                "HKEY_LOCAL_MACHINE\\Software\\" + appKeyIn,
                "NotifyMailFromAddress",
                "test@example.com"
            );
            if( null == optionsOut.FromAddress ) {
                optionsOut.FromAddress = "";
            }

            optionsOut.ToAddresses = ((string)Registry.GetValue(
                "HKEY_LOCAL_MACHINE\\Software\\" + appKeyIn,
                "NotifyMailAddresses",
                ""
            )).Split( ',' );
            if( null == optionsOut.ToAddresses ) {
                optionsOut.ToAddresses = new string[] { };
            }

            return optionsOut;
        }

        public void Notify( string toAddress, string subjectIn, string bodyIn ) {
            this.Notify( toAddress, subjectIn, bodyIn, new SimpleNotifyAttachment[] { } );
        }

        public void Notify( string toAddress, string subjectIn, string bodyIn, SimpleNotifyAttachment[] attachmentsIn ) {
            MailMessage notifyMessage = new MailMessage(
                this.fromAddress,
                toAddress,
                subjectIn,
                bodyIn
            );

            foreach( SimpleNotifyAttachment attachment in attachmentsIn ) {
                Attachment attachmentObject = new Attachment( attachment.Contents, attachment.FileName );
                notifyMessage.Attachments.Add( attachmentObject );
            }

            //notifyMessage.Subject = "Potential Ransomeware Activity Detected";

            //notifyMessage.Subject = ;

            try {
                this.notifyClient.Send( notifyMessage );
            } catch( SmtpException ex ) {
                throw new SimpleNotifyMailerException( ex.Message );
            }
        }
    }
}
