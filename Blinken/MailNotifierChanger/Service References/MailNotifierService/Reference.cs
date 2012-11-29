﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.17929
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MailNotifierClient.MailNotifierService {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="MailNotifierService.IMailNotifierService")]
    public interface IMailNotifierService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMailNotifierService/DoFadeTo", ReplyAction="http://tempuri.org/IMailNotifierService/DoFadeToResponse")]
        void DoFadeTo(byte red, byte green, byte blue);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMailNotifierService/DoFadeToMulti", ReplyAction="http://tempuri.org/IMailNotifierService/DoFadeToMultiResponse")]
        void DoFadeToMulti(byte[] colorBytes);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMailNotifierService/SetColor", ReplyAction="http://tempuri.org/IMailNotifierService/SetColorResponse")]
        void SetColor(byte red, byte green, byte blue);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IMailNotifierServiceChannel : MailNotifierClient.MailNotifierService.IMailNotifierService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class MailNotifierServiceClient : System.ServiceModel.ClientBase<MailNotifierClient.MailNotifierService.IMailNotifierService>, MailNotifierClient.MailNotifierService.IMailNotifierService {
        
        public MailNotifierServiceClient() {
        }
        
        public MailNotifierServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public MailNotifierServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public MailNotifierServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public MailNotifierServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public void DoFadeTo(byte red, byte green, byte blue) {
            base.Channel.DoFadeTo(red, green, blue);
        }
        
        public void DoFadeToMulti(byte[] colorBytes) {
            base.Channel.DoFadeToMulti(colorBytes);
        }
        
        public void SetColor(byte red, byte green, byte blue) {
            base.Channel.SetColor(red, green, blue);
        }
    }
}
