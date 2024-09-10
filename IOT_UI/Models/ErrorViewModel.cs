namespace IOT_UI.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }

	public class ErrorResponseDTO
	{
		public bool Success { get; set; }
		public string Message { get; set; }
		public object Data { get; set; }
	}
	
}
