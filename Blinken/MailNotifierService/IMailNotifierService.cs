using System.Drawing;
using System.ServiceModel;

namespace MailNotifierService
{
    [ServiceContract]
    public interface IMailNotifierService
    {
        [OperationContract]
        void DoFadeTo(byte red, byte green, byte blue);

        [OperationContract]
        void SetColor(byte red, byte green, byte blue);
    }
}
