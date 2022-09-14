namespace RomgleWebApi.Data.Models
{
    public class Response
    {
        public Response() 
        {
            ErrorCode = 0;
            ErrorMessage = "";
        }

        public Response(int errorCode, string errorMessage)
        {
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
        }

        public int ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class Response<T> : Response
    {
        public Response(T body)
        {
            Body = body;
        }

        public Response(int errorCode, string errorMessage) : base(errorCode, errorMessage)
        {
            Body = default;
        }
        
        public T Body { get; set; }

        public static implicit operator Response<T>(T body) 
        { 
            return new Response<T>(body);
        }
    }
}
