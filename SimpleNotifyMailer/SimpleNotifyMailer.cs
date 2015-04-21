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

        public void Rewind() {
            this.Contents.Seek( 0, SeekOrigin.Begin );
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
            SimpleConfig appConfig = new SimpleConfig();

            appConfig.Set( "NotifyMailAddresses", string.Join( ",", toAddressesIn ) );
            appConfig.Set( "NotifyMailServer", serverIn );
            appConfig.Set( "NotifyMailFromAddress", fromAddressIn );
            try {
                appConfig.Set( "NotifyMailPort", int.Parse( portIn ).ToString() );
            } catch( FormatException ex ) {
                throw new SimpleNotifyMailerException( ex.Message );
            }
            
            appConfig.SaveConfigRegistry( appKeyIn );
        }

        public static SimpleNotifyMailer.Options GetNotifyOptionsRegistry( string appKeyIn ) {
            SimpleNotifyMailer.Options optionsOut = new SimpleNotifyMailer.Options();
            SimpleConfig appConfig = SimpleConfig.LoadConfigRegistry( appKeyIn );

            optionsOut.Server = appConfig.Get( "NotifyMailServer", "127.0.0.1" );
            optionsOut.Port = appConfig.Get( "NotifyMailPort", "25" );
            optionsOut.FromAddress = appConfig.Get( "NotifyMailFromAddress", "test@example.com" );
            optionsOut.ToAddresses = appConfig.GetList( "NotifyMailAddresses", ',' );
            
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
