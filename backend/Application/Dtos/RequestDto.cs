using backend.Application.Dtos.Requests;

namespace backend.Application.Dtos.Requests;

public class StartGameRequest
{
    public int UserId { get; set; }
    public string SaveName { get; set; }
}

public class MakeChoiceRequest
{
    public int SaveId { get; set; }
    public int ChoiceId { get; set; }
}