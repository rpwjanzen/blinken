﻿using System.ServiceModel;

namespace SignService
{
    [ServiceContract]
    public interface ISignService
    {
        [OperationContract]
        void SetText(string text);
    }
}