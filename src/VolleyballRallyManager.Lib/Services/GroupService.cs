using System.Security.Cryptography;
using System.Text;

namespace VolleyballRallyManager.Lib.Services;

public class GroupService
{
    private static Dictionary<string, string> groupColorMap = new Dictionary<string, string>();
    private static string[] colors = new string[] {
        "#98FB98", //Pale Green	
        // "#FFFFE0", //Light Yellow
        "#FFA07A", //Light Salmon	
        "#E0FFFF", //Light Cyan	
        //"#FFF8DC", // Cornsilk
        //"#F0FFFF", //Azure
        "#F5F5DC", //Beige
        "#F0F8FF", //Alice Blue
        "#F0F0F0",//Light Gray	
         "#FFEFD5"//"Papaya Whip"
    };
    //   public static Dictionary<string, string> groupIconMap = new Dictionary<string, string>();
    public string GetColor(string groupName)
    {
        groupName = groupName.ToLower();
        if (groupColorMap.TryGetValue(groupName, out string? value))
        {
            return value;
        }
        int i = groupColorMap.Count();
        if (i < colors.Length)
        {
            groupColorMap.Add(groupName, colors[i]);
            return colors[i];
        }
        string color = GetRandomColor(groupName);
        groupColorMap.Add(groupName, color);
        return color;
    }
    private string GetRandomColor(string groupName)
    {
        using (MD5 md5Hash = MD5.Create())
        {
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(groupName));
            return BitConverter.ToString(data).Replace("-", string.Empty).Substring(0, 6);
        }

    }
}
