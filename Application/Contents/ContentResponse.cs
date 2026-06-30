namespace Application.Contents;

public class ContentResponse
{
    public List<ProvinceResponse> Provinces { get; set; }
}

public class ProvinceResponse
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<DistrictResponse> Districts { get; set; }
}

public class ProvinceResponseV2
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class DistrictResponse
{
    public int Id { get; set; }
    public int ProvinceId { get; set; }
    public string Name { get; set; }
}