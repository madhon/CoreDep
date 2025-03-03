namespace CoreDep;

using System.Threading.Tasks;
using Pulumi;

public class Program
{
    public static Task<int> Main() => Deployment.RunAsync<CoreDepStack>();
}
