namespace Utility
{
    public static class StaticData
    {
        public const String Cust_Role = "Customer";
        public const String Comp_Role = "Company";
        public const String Emp_Role = "Employee";
        public const String Admin_Role = "Admin";


        public const String OrderStatusInProcess = "StatusInProcess";
        public const String OrderStatusRefunded = "StatusRefunded";
        public const String OrderStatusShipped = "StatusShipped";
        public const String OrderStatusApproved = "StatusApproved";
        public const String OrderStatusCancelled = "StatusCancelled";
        public const String OrderStatusPending = "StatusPending";

        public const String PaymentStatusPending = "PaymentStatusPending";
        public const String PaymentStatusApproved = "PaymentStatusApproved";
        public const String PaymentStatusRejected = "PaymentStatusRejected";
        public const String PaymentStatusDelayedPayment = "PaymentApprovedForDelayedPayment";

        public const String SessionCart = "SessionCartCount";

        public static void Empty(System.IO.DirectoryInfo directory)
        {
            foreach (System.IO.FileInfo file in directory.GetFiles()) file.Delete();
            foreach (System.IO.DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
        }
    }
}