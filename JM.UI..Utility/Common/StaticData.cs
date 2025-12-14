using System;
using System.Collections.Generic;
using System.Text;

namespace InvPosUtility.Common
{
    public class OrderState
    {
        public static int ORDER_CONFIRMATION_PENDING = 100;
        //public static int ORDER_CONFIRM = 101;
        public static int ORDER_CONFIRMATION_REJECTED = 102;

        public static int PAYMENT_CONFIRMATION_PENDING = 103;
        //public static int PAYMENT_CONFIRM = 104;
        public static int PAYMENT_CONFIRMATION_REJECTED = 105;

        public static int PAYMENT_VERIFICATION_PENDING = 106;
        //public static int PAYMENT_VERIFIED = 107;
        public static int PAYMENT_VERIFICATION_REJECTED = 108;

        //public static int ORDER_COLLECTION_PENDING = 109;
        //public static int ORDER_COLLECTED = 110;
        //public static int ORDER_COLLECTION_REJECTED = 111;


        public static int DELIVERY_PENDING = 112;
        public static int DELIVERY_COMPLETED = 113;
        public static int DELIVERY_REJECTED = 114;
        public static int PARTIAL_DELIVERY = 115;
    }
}
