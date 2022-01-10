namespace Webdev.Core
{
    public static class Constants
    {
        public const string ResponseOk = "ok";
        public const string ResponsePaid = "paid";
        public const string ResponseError = "error";
        public const string ResponseFailed = "failed";
        public const string ResponseDelivered = "delivered";
        public const string ResponseCancelled = "cancelled";
        public const string ResponseInvalidId = "invalid id.";
        public const string ResponseAwaitingRedirect = "awaiting redirect";
        public const string ResponseAwaitingDelivery = "awaiting delivery";
        public const string ResponseCreatedNotPaid = "created but not paid";

        public const string UrlInitiateTransaction = "/interface/initiatetransaction";
        public const string UrlInitiateMobileTransaction = "/interface/remotetransaction";
        public const string MobileMoneyMethodEcocash = "ecocash";
    }
}