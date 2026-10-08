using EHUB.Models;
using Microsoft.AspNetCore.Mvc;

namespace EHUB.Controllers
{
    public class ChatController : Controller
    {
        private static List<ChatMessage> _chatHistory = new List<ChatMessage>();

        public IActionResult Index()
        {
            var model = new ChatMessageViewModel
            {
                ChatHistory = _chatHistory
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(ChatMessageViewModel model)
        {
            string fileName = null;

            if (model.Attachment != null && model.Attachment.Length > 0)
            {
                var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                Directory.CreateDirectory(uploads); // ensure path exists

                fileName = Path.GetFileName(model.Attachment.FileName);
                var filePath = Path.Combine(uploads, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.Attachment.CopyToAsync(stream);
                }
            }

            _chatHistory.Add(new ChatMessage
            {
                Text = model.Message,
                FileName = fileName,
                Timestamp = DateTime.Now
            });

            return RedirectToAction("Index");
        }
    }
}
