namespace DF2023.Mvc.Models
{
    public class ApiResult
    {
        public string Message { get; set; }

        public string Status { get; set; }

        public dynamic Data { get; set; }
        public ApiResult()
        { }
        public ApiResult(string message, bool isSuccessful, dynamic data)
        {
            this.Message = message;
            this.Status = isSuccessful ? "Y" : "N";
            this.Data = data;
        }
    }
}