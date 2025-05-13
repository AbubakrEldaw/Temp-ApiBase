namespace APIBase.PublicAPIModels
{
    public class BasicError
    {
        public string Error { get; set; }
        public string ErrorDescription { get; set; }
    }
    public class ErrorList
    {
        public List<BasicError> Errors { get; set; }
    }
}
