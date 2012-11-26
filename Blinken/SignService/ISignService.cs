using System.ServiceModel;

namespace SignService
{
    [ServiceContract]
    public interface ISignService
    {
        [OperationContract]
        void ScrollText(string text);

        [OperationContract]
        void ScrollImage(bool[] imageData);

        [OperationContract]
        void SetText(string text);
    }
}
