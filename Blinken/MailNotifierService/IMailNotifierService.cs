using System.Drawing;
using System.ServiceModel;

namespace MailNotifierService
{
    [ServiceContract]
    public interface IMailNotifierService
    {
        [OperationContract]
        void FadeToRgb(byte red, byte green, byte blue);

        [OperationContract]
        void FadeToMultiRgb(byte [] colorBytes);

        [OperationContract]
        void SetColorRgb(byte red, byte green, byte blue);
    }
}
