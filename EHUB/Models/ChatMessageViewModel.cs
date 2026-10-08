namespace EHUB.Models
{
    public class ChatMessageViewModel
    {
        public string Message { get; set; }

        public IFormFile Attachment { get; set; }

        public List<ChatMessage> ChatHistory { get; set; } = new List<ChatMessage>();
    }

    public class ChatMessage
    {
        public string Text { get; set; }
        public string FileName { get; set; }
        public DateTime Timestamp { get; set; }
    }

}
