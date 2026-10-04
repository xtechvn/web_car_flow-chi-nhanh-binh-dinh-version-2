namespace XTECH_FRONTEND.Model
{
    public class ZaloUserDetailRequest
    {
        public string Phone { get; set; } = string.Empty;
    }

    public class ZaloUserDetailResponse
    {
        public ZaloUserData? Data { get; set; }
        public int Error { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class ZaloUserData
    {
        public string user_id{ get; set; } = string.Empty;
        public string user_id_by_app { get; set; } = string.Empty;
        public string display_name { get; set; } = string.Empty;
        public bool user_is_follower { get; set; }
        public string Avatar { get; set; } = string.Empty;
        public string user_last_interaction_date { get; set; } = string.Empty;
    }

    public class ZaloMessageRequest
    {
        public ZaloRecipient Recipient { get; set; } = new();
        public ZaloMessage Message { get; set; } = new();
    }

    public class ZaloRecipient
    {
        public string user_id { get; set; } = string.Empty;
    }

    public class ZaloMessage
    {
        public string Text { get; set; } = string.Empty;
    }

    public class ZaloMessageResponse
    {
        public int Error { get; set; }
        public string Message { get; set; } = string.Empty;
        public ZaloMessageData? Data { get; set; }
    }

    public class ZaloMessageData
    {
        public string MessageId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }
}
