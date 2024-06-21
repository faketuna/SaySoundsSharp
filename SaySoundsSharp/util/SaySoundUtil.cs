namespace SaySoundsSharp;

public static class SaySoundUtil {
    public static UserSaySoundInput processUserInput(string argString) {
        string[] args = argString.Split(" ");

        string saySound = "";
        float volume = 1.0F;
        float pitch = 1.0F;

        foreach(string arg in args) {
            if(arg.Contains("@", StringComparison.InvariantCultureIgnoreCase)) {
                pitch = float.Parse(arg[1..]) / 100;
            }
            else if (arg.Contains("%", StringComparison.InvariantCultureIgnoreCase)) {
                // TODO()
            }
            else {
                saySound += $" {arg}";
            }
        }
    

        return new UserSaySoundInput(saySound[1..].Replace("\"", ""), volume, pitch);
    }
}