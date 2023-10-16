using HandJob.WebApi.Auth;
using Microsoft.Extensions.Options;

namespace HandJob.WebApi.Options;

public class AppSettings : IOptions<AppSettings>
{
    public PermissionRequirement TokenParameter { get; set; } = new();
    public AppSettings Value => this;

    public string ImagePath { get; set; } = "images/";
}