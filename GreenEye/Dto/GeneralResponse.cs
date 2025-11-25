namespace GreenEye.Dto
{
    public class GeneralResponse<T>
    {
        bool IsSuccess { get; set; }
        string? Message { get; set; }
        T? Data { get; set; }
    }
}
