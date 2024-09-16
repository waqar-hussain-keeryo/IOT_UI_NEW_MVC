﻿namespace IOT_UI.Models
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null;
        public T Data { get; set; }
    }
}
