namespace Nop.Web.Controllers.api.Models
{
    public class ApiGenericModel<T>
    {
        public bool IsSuccessStatusCode { get; set; } = true;
        public string ErrorMessage { get; set; }
        public T Data { get; set; }
    }
}
