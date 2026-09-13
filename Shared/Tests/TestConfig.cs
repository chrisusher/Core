namespace ChrisUsher.Core.Shared.Tests;

public class TestConfig
{
    public EnvironmentType Environment { get; set; } = EnvironmentType.Local;

    public bool LocalSetup()
    {
        switch(Environment)
        {
            case EnvironmentType.Local:
            case EnvironmentType.Development:
                return true;
            default:
                return false;
        }
    }
}
